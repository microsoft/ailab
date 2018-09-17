// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using SnipInsight.Properties;
using SnipInsight.Util;

namespace SnipInsight.ImageCapture
{
    /// <summary>
    /// Implements a cache for getting monitor properties
    /// Note: Cache must get invalidated if user changes screen resolution
    /// </summary>
    public class ScreenProperties
    {
        public class MonitorInformation
        {
            public string deviceName;
            public bool isPrimary;
            internal NativeMethods.RECT rcMonitor;
            internal NativeMethods.RECT rcWork;
            public double scalingFactor;
            public double dpiX;
            public double dpiY;
        }

        Dictionary<IntPtr, MonitorInformation> _monitorInfoCache = new Dictionary<IntPtr, MonitorInformation>();

        public ScreenProperties()
        {
            GetMonitorsInformation();

            // CheckMonitorScales();
        }

        public MonitorInformation GetMonitorInformation(IntPtr hMonitor)
        {
            if (_monitorInfoCache.ContainsKey(hMonitor))
                return _monitorInfoCache[hMonitor];

            if (hMonitor == IntPtr.Zero)
            {
                // Get handle to the default monitor
                const int MONITOR_DEFAULTTOPRIMARY = 0x00000001;
                hMonitor = NativeMethods.MonitorFromWindow(IntPtr.Zero, MONITOR_DEFAULTTOPRIMARY);
                if (_monitorInfoCache.ContainsKey(hMonitor))
                {
                    _monitorInfoCache[IntPtr.Zero] = _monitorInfoCache[hMonitor];
                    return _monitorInfoCache[hMonitor];
                }
            }

            var monitorInfo = NativeMethods.MONITORINFOEX.New();
            if (!NativeMethods.GetMonitorInfoEx(hMonitor, ref monitorInfo))
            {
                _monitorInfoCache[hMonitor] = null;
                return null;
            }

            System.Diagnostics.Trace.WriteLine("Monitor:\"" + monitorInfo.deviceName + "\" Handle:" + hMonitor
                + " Left:" + monitorInfo.rcMonitor.left + " Top:" + monitorInfo.rcMonitor.top
                + " Right:" + monitorInfo.rcMonitor.right + " Bottom:" + monitorInfo.rcMonitor.bottom);

            UInt32 effectiveDPIx, effectiveDPIy;
            //GetDpiForMonitor(hMonitor, MONITOR_DPI_TYPE.MDT_Effective_DPI, out effectiveDPIx, out effectiveDPIy);
            DpiUtilities.GetMonitorEffectiveDpi(hMonitor, out effectiveDPIx, out effectiveDPIy);
            System.Diagnostics.Trace.WriteLine("Effective DPI:" + effectiveDPIx + " " + effectiveDPIy);

            //UInt32 rawDPIx, rawDPIy;
            //GetDpiForMonitor(hMonitor, MONITOR_DPI_TYPE.MDT_Raw_DPI, out rawDPIx, out rawDPIy);
            //System.Diagnostics.Trace.WriteLine("Raw DPI:" + rawDPIx + " " + rawDPIy);

            var monitorInformation = new MonitorInformation()
            {
                deviceName = monitorInfo.deviceName,
                isPrimary = (monitorInfo.dwFlags & NativeMethods.MONITORINFOF_PRIMARY) != 0,
                rcMonitor = monitorInfo.rcMonitor,
                rcWork = monitorInfo.rcWork,
                scalingFactor = DpiUtilities.GetScreenScalingFactor(monitorInfo.deviceName),
                dpiX = effectiveDPIx,
                dpiY = effectiveDPIy,
            };

            _monitorInfoCache[hMonitor] = monitorInformation;
            if (monitorInformation.isPrimary && !_monitorInfoCache.ContainsKey(IntPtr.Zero))
                _monitorInfoCache[IntPtr.Zero] = monitorInformation;
            return monitorInformation;
        }

        public void GetMonitorsInformation()
        {
            NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
                {
                    GetMonitorInformation(hMonitor);
                    return true;
                }, IntPtr.Zero);
        }

        public System.Drawing.Rectangle GetMaxRectangleFromMonitors()
        {
            var minX = 0;
            var maxX = 0;
            var minY = 0;
            var maxY = 0;

            foreach (var tuple in _monitorInfoCache)
            {
                var monitor = tuple.Value;
                if (monitor != null)
                {
                    // No scaled values, trying to get the real virtual screen
                    var rcMonitor = monitor.rcMonitor;

                    if (minX > rcMonitor.left)
                    {
                        minX = rcMonitor.left;
                    }

                    if (minY > rcMonitor.top)
                    {
                        minY = rcMonitor.top;
                    }

                    var tempX = rcMonitor.left + ((double)rcMonitor.width) * monitor.scalingFactor;
                    if (maxX < tempX)
                    {
                        maxX = (int)tempX;
                    }

                    var tempY = rcMonitor.top + (rcMonitor.height) * monitor.scalingFactor;
                    if (maxY < tempY)
                    {
                        maxY = (int)tempY;
                    }
                }
            }

            var rect = new System.Drawing.Rectangle(minX, minY, maxX-minX, maxY-minY);
            return rect;
        }

        private void CheckMonitorScales()
        {
            var distinctScales = _monitorInfoCache.Values.Select(m => m.scalingFactor).Distinct();
            if (distinctScales.Count() > 1)
            {
                MessageBox.Show(Resources.Screen_Diferent_Scale_Factor_Detected, Resources.WindowTitle_Toast);
            }
        }
    }
}
