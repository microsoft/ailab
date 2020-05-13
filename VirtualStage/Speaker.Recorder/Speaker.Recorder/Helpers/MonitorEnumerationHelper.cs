using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Windows.Foundation;

namespace Speaker.Recorder.Helpers
{
    class MonitorInfo
    {
        public bool IsPrimary { get; set; }
        public Vector2 ScreenSize { get; set; }
        public Rect MonitorArea { get; set; }
        public Rect WorkArea { get; set; }
        public string DeviceName { get; set; }
        public IntPtr Hmon { get; set; }
    }

    static class MonitorEnumerationHelper
    {
        delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        private const int CCHDEVICENAME = 32;
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct MonitorInfoEx
        {
            public int Size;
            public RECT Monitor;
            public RECT WorkArea;
            public uint Flags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string DeviceName;
        }

        [DllImport("user32.dll")]
        static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);

        public static IEnumerable<MonitorInfo> GetMonitors()
        {
            var result = new List<MonitorInfo>();

            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
                {
                    MonitorInfoEx mi = new MonitorInfoEx();
                    mi.Size = Marshal.SizeOf(mi);
                    bool success = GetMonitorInfo(hMonitor, ref mi);
                    if (success)
                    {
                        var info = new MonitorInfo
                        {
                            ScreenSize = new Vector2(mi.Monitor.right - mi.Monitor.left, mi.Monitor.bottom - mi.Monitor.top),
                            MonitorArea = new Rect(mi.Monitor.left, mi.Monitor.top, mi.Monitor.right - mi.Monitor.left, mi.Monitor.bottom - mi.Monitor.top),
                            WorkArea = new Rect(mi.WorkArea.left, mi.WorkArea.top, mi.WorkArea.right - mi.WorkArea.left, mi.WorkArea.bottom - mi.WorkArea.top),
                            IsPrimary = mi.Flags > 0,
                            Hmon = hMonitor,
                            DeviceName = mi.DeviceName
                        };
                        result.Add(info);
                    }
                    return true;
                }, IntPtr.Zero);
            return result;
        }
    }
}
