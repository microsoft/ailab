using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Speaker.Recorder.Services
{
    public class AppUpdaterService : IHostedService
    {
        private readonly HttpClient http;
        private readonly string updateUrl;
        private readonly FileVersionInfo assemblyFileVersionInfo;
        private readonly IHostApplicationLifetime applicationLifetime;
        private readonly ILogger<AppUpdaterService> logger;

        public AppUpdaterService(
            IConfiguration configuration,
            IHostApplicationLifetime applicationLifetime,
            ILogger<AppUpdaterService> logger)
        {
            this.http = new HttpClient();
            this.updateUrl = configuration.GetValue<string>("Data:AppUpdateUrl");
            this.assemblyFileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            this.applicationLifetime = applicationLifetime;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation($"Starting update");
            if (await this.CheckAndUpdate())
            {
                this.applicationLifetime.StopApplication();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task<bool> CheckAndUpdate()
        {
            if (this.updateUrl != null && Version.TryParse(assemblyFileVersionInfo.ProductVersion, out var currentVersion) && currentVersion > new Version(1, 0, 0))
            {
                DirectoryInfo tempUpdateExtractFolder = null;
                string tempUpdateFile = null;
                logger.LogInformation($"Current version : {currentVersion}");
                try
                {
                    var timeoutCancellationToken = new CancellationTokenSource();
                    timeoutCancellationToken.CancelAfter(5000);
                    using var httpResponse = await this.http.GetAsync(this.updateUrl, HttpCompletionOption.ResponseHeadersRead, timeoutCancellationToken.Token).ConfigureAwait(false);
                    if (httpResponse.Headers.TryGetValues("x-ms-meta-AppVersion", out var versionValues) && versionValues.Any())
                    {
                        if (Version.TryParse(versionValues.First(), out var lastVersion))
                        {
                            logger.LogInformation($"Last version: {lastVersion}");
                            logger.LogInformation(currentVersion < lastVersion ? $"The application will be updated" : "No update required");
                            if (currentVersion < lastVersion)
                            {
                                tempUpdateFile = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
                                using (var tempUpdateStream = File.Create(tempUpdateFile, 16 * 1024))
                                {
                                    await httpResponse.Content.CopyToAsync(tempUpdateStream).ConfigureAwait(false);
                                }

                                tempUpdateExtractFolder = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(tempUpdateFile)));
                                logger.LogInformation($"Created temp directory in {tempUpdateExtractFolder}");
                                ZipFile.ExtractToDirectory(tempUpdateFile, tempUpdateExtractFolder.FullName);
                                logger.LogInformation("Extracted new version");
                                UpdateSelf(tempUpdateExtractFolder.FullName);
                                return true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Exception trying download new version");
                }
                finally
                {
                    if (tempUpdateFile != null)
                    {
                        File.Delete(tempUpdateFile);
                    }
                }
            }

            return false;
        }

        public static void UpdateSelf(string updateFolder)
        {
            var selfExecutable = Process.GetCurrentProcess().MainModule.FileName;
            var self = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var updateBatFile = Path.Combine(self, "Update.bat");

            using (var batFile = new StreamWriter(File.Create(updateBatFile)))
            {
                batFile.WriteLine("@ECHO OFF");
                batFile.WriteLine("TIMEOUT /t 1 /nobreak > NUL");
                batFile.WriteLine("TASKKILL /IM \"{0}\" > NUL", Path.GetFileName(selfExecutable));
                batFile.WriteLine("MOVE \"{0}\\*\" \"{1}\"", updateFolder, self);
                batFile.WriteLine("DEL \"%~f0\" & START \"\" /B \"{0}\"", selfExecutable);
            }

            ProcessStartInfo startInfo = new ProcessStartInfo(updateBatFile)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = self
            };

            Process.Start(startInfo);
        }
    }
}
