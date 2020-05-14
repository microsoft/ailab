using Speaker.Recorder.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Speaker.Recorder.PowerPoint
{
    static class PowerPointHelper
    {
        private static string powerPointPath;

        public static string FindExtensionExe(string extension)
        {
            try
            {
                var assocInfo = new ProcessStartInfo("cmd.exe");
                assocInfo.ArgumentList.Add("/c");
                assocInfo.ArgumentList.Add("assoc");
                assocInfo.ArgumentList.Add(extension);
                assocInfo.CreateNoWindow = true;
                assocInfo.WindowStyle = ProcessWindowStyle.Hidden;
                assocInfo.RedirectStandardOutput = true;
                using var assocProcess = Process.Start(assocInfo);
                assocProcess.WaitForExit();
                var assocResult = assocProcess.StandardOutput.ReadToEnd();
                var assocType = assocResult.Substring(extension.Length + 1).Trim(); // .pptx=PowerPoint.Show.12

                var ftypeInfo = new ProcessStartInfo("cmd.exe");
                ftypeInfo.ArgumentList.Add("/c");
                ftypeInfo.ArgumentList.Add("ftype");
                ftypeInfo.ArgumentList.Add(assocType);
                ftypeInfo.CreateNoWindow = true;
                ftypeInfo.WindowStyle = ProcessWindowStyle.Hidden;
                ftypeInfo.RedirectStandardOutput = true;
                using var ftypeProcess = Process.Start(ftypeInfo);
                ftypeProcess.WaitForExit();
                var ftypeResult = ftypeProcess.StandardOutput.ReadToEnd();
                var fileMatch = Regex.Match(ftypeResult, @"^[^=]+=(""([^""]+)""|([^\s]+))"); // PowerPoint.Show.12="C:\Program Files\Microsoft Office\Root\Office16\POWERPNT.EXE" "%1" /ou "%u"
                var ftypePath = fileMatch.Groups[1].Value.StartsWith('"') ? fileMatch.Groups[2].Value.Trim() : fileMatch.Groups[3].Value.Trim();

                return ftypePath;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception finding program for {extension}: {e}");
                return null;
            }
        }

        public static void Initilize()
        {
            powerPointPath = FindExtensionExe(".pptx");
        }

        public static Process StartPowerPoint(string pptPath)
        {
            var success = FileUnblocker.Unblock(pptPath);
            if (!success)
            {
                throw new InvalidOperationException($"The specified file is blocked and can not be unblocked. {pptPath}");
            }

            if (powerPointPath == null)
            {
                powerPointPath = FindExtensionExe(Path.GetExtension(pptPath));
            }

            var processName = Path.GetFileNameWithoutExtension(powerPointPath);
            var existingProcesses = Process.GetProcessesByName(processName);
            foreach (var item in existingProcesses)
            {
                var cmd = GetCommandLine(item);
                if (cmd?.Contains("/s", StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    item.Kill();
                }
            }

            ProcessStartInfo psi = new ProcessStartInfo(powerPointPath);
            psi.ArgumentList.Add("/S");
            psi.ArgumentList.Add(pptPath);
            psi.UseShellExecute = true;
            using var process = Process.Start(psi);
            process.WaitForInputIdle();
            Process powerPointProcess;
            int maxTries = 50;
            do
            {
                maxTries--;
                if (!process.HasExited)
                {
                    Thread.Sleep(10);
                }
                powerPointProcess = WindowEnumerationHelper.GetProcesses(processName).FirstOrDefault(x => GetCommandLine(x)?.Contains(pptPath) == true);
                if (powerPointProcess == null)
                {
                    Thread.Sleep(10);
                }
            } while (powerPointProcess == null && maxTries > 0);

            return powerPointProcess;
        }

        public static string GetCommandLine(Process process)
        {
            try
            {
                string cmdLine = null;
                using (var searcher = new ManagementObjectSearcher($"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}"))
                {
                    // By definition, the query returns at most 1 match, because the process
                    // is looked up by ID (which is unique by definition).
                    var matchEnum = searcher.Get().GetEnumerator();
                    if (matchEnum.MoveNext())
                    {
                        cmdLine = matchEnum.Current["CommandLine"]?.ToString();
                    }
                }

                return cmdLine;
            }
            catch
            {
                return null;
            }
        }
    }
}
