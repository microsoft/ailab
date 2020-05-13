using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Speaker.Recorder.Helpers
{
    static class WindowEnumerationHelper
    {
        public static IEnumerable<Process> GetProcesses()
        {
            var processesWithWindows = from p in Process.GetProcesses()
                                       where !string.IsNullOrWhiteSpace(p.MainWindowTitle) && WindowHelper.IsWindowValidForCapture(p.MainWindowHandle)
                                       select p;
            return processesWithWindows;
        }

        public static IEnumerable<Process> GetProcesses(string processName)
        {
            var processesWithWindows = from p in Process.GetProcesses()
                                       where p.ProcessName == processName && !string.IsNullOrWhiteSpace(p.MainWindowTitle) && WindowHelper.IsWindowValidForCapture(p.MainWindowHandle)
                                       select p;
            return processesWithWindows;
        }
    }
}
