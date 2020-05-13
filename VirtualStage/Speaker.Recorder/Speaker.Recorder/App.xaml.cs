using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using Serilog;
using Speaker.Recorder.Kinect;
using Speaker.Recorder.PowerPoint;
using Speaker.Recorder.Services;
using Speaker.Recorder.Services.IdentificationService;
using Speaker.Recorder.ViewModels;
using System;
using System.Diagnostics;
using System.Windows;

namespace Speaker.Recorder
{
    public partial class App : Application
    {
        public event EventHandler Loaded;

        public IHost AppHost { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.AppHost = Host.CreateDefaultBuilder(e.Args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddJsonFile("app.settings.json", optional: true, reloadOnChange: true)
                           .AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    services.Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromSeconds(30));

                    // Updater service must be the first!
                    services.AddTransient<IHostedService, AppUpdaterService>();
                    services.AddSingleton<IHostedService>(x => x.GetRequiredService<MainWindow>());
                    services.AddSingleton<IHostedService, KinectLoggingService>();

                    services.AddTransient<PlayerViewModel>();
                    services.AddTransient<RecordViewModel>();
                    services.AddTransient<Session>();
                    services.AddTransient<KinectRecorderService>();
                    services.AddTransient<PowerPointRecorderService>();

                    services.AddSingleton<SessionsService>();
                    services.AddSingleton<UploadManager>();
                    services.AddSingleton<LocalPathsHelper>();
                    services.AddSingleton<Func<KinectRecorderService>>(x => () => x.GetRequiredService<KinectRecorderService>());
                    services.AddSingleton<Func<PowerPointRecorderService>>(x => () => x.GetRequiredService<PowerPointRecorderService>());

                    services.AddSingleton<KinectCaptureProvidersFactory>();
                    services.AddSingleton<PowerPointGraphicsCaptureItemsFactory>();

                    services.AddSingleton((Application)this);
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<INavigationService>(x => x.GetRequiredService<MainWindow>());

                    var customIndetification = context.Configuration.GetValue("Data:SessionsIdentifier", (string)null);
                    if (string.IsNullOrEmpty(customIndetification))
                    {
                        services.AddSingleton<IIdentificationService, DesktopNameIdentificationService>();
                    }
                    else
                    {
                        services.AddSingleton((IIdentificationService)new CustomIdentificationService(customIndetification));
                    }
                })
                .ConfigureLogging((context, builder) =>
                {
                    var logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(context.Configuration)
                        .CreateLogger();

                    builder.AddDebug();
                    builder.AddSerilog(logger);
                })
                .Build();

            PowerPointHelper.Initilize();

            await this.AppHost.StartAsync();

            this.Loaded?.Invoke(this, EventArgs.Empty);
            this.Loaded = null;

            await this.AppHost.WaitForShutdownAsync();
            this.Shutdown();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            await this.AppHost.StopAsync();
            this.AppHost.Dispose();
        }
    }
}
