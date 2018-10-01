// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Windows;

namespace SnipInsight.Util
{
    public static class DpiUtilities
    {
        #region DpiAwareness

        private static ProcessDpiAwareness _dpiAwareness = (ProcessDpiAwareness)(-1);

        public static ProcessDpiAwareness DpiAwareness
        {
            get
            {
                if (_dpiAwareness == (ProcessDpiAwareness)(-1))
                {
                    _dpiAwareness = GetDpiAwareness();
                }

                return _dpiAwareness;
            }
        }

        private static ProcessDpiAwareness GetDpiAwareness()
        {
            return ProcessDpiAwareness.PerMonitorDpiAware;
            ProcessDpiAwareness result = ProcessDpiAwareness.DpiUnaware;

            try
            {
                // If at least Windows 8.1
                if (IsOsGreaterOrEqualTo(6, 3))
                {
                    int value = 0;

                    if (NativeMethods.GetProcessDpiAwareness(IntPtr.Zero, ref value) == 0)
                    {
                        result = (ProcessDpiAwareness)value;
                    }
                }
            }
            catch
            {
                result = ProcessDpiAwareness.DpiUnaware;
            }

            return result;
        }

        private static bool IsOsGreaterOrEqualTo(int major, int minor)
        {
            int currentMajor = System.Environment.OSVersion.Version.Major;
            int currentMinor = System.Environment.OSVersion.Version.Minor;

            if (currentMajor == major && currentMinor >= minor)
            {
                return true;
            }
            else if (currentMajor > major)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Scale

        public static DpiScale CalculateScale(uint dpi1X, uint dpi1Y, uint dpi2X, uint dpi2Y)
        {
            return new DpiScale((double)dpi1X / (double)dpi2X, (double)dpi1Y / (double)dpi2Y);
        }

        public static DpiScale GetSystemScale()
        {
            uint systemDpiX;
            uint systemDpiY;

            GetSystemEffectiveDpi(out systemDpiX, out systemDpiY);

            return CalculateScale(systemDpiX, systemDpiY, 96, 96);
        }

        /// <summary>
        /// Gets the window (monitor) scale versus System DPI.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>When developing a DPI-aware application, this is useful for applying a ScaleTransform
        /// to your window so all fonts and graphics are scaled and rendered beautifully based on the
        /// physical capabilities of the display and the accessibility settings of the user.</para>
        /// </remarks>
        public static DpiScale GetWindowScale(Window window)
        {
            return GetWindowScale(GetWindowHwnd(window));
        }

        /// <summary>
        /// Gets the window (monitor) scale versus System DPI.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>When developing a DPI-aware application, this is useful for applying a ScaleTransform
        /// to your window so all fonts and graphics are scaled and rendered beautifully based on the
        /// physical capabilities of the display and the accessibility settings of the user.</para>
        /// </remarks>
        public static DpiScale GetWindowScale(IntPtr hwnd)
        {
            uint dpiX;
            uint dpiY;

            GetWindowEffectiveDpi(hwnd, out dpiX, out dpiY);

            uint systemDpiX;
            uint systemDpiY;

            GetSystemEffectiveDpi(out systemDpiX, out systemDpiY);

            return CalculateScale(dpiX, dpiY, systemDpiX, systemDpiY);
        }

        /// <summary>
        /// Gets the virtual pixel scale for a window. This is essentially
        /// the Effective DPI of the monitor versus a standard 96 DPI.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <returns></returns>
        public static DpiScale GetVirtualPixelScale(Window window)
        {
            return GetVirtualPixelScale(GetWindowHwnd(window));
        }

        /// <summary>
        /// Gets the virtual pixel scale for a window. This is essentially
        /// the Effective DPI of the monitor versus a standard 96 DPI.
        /// </summary>
        /// <param name="hwnd">The HWND.</param>
        /// <returns></returns>
        public static DpiScale GetVirtualPixelScale(IntPtr hwnd)
        {
            uint dpiX;
            uint dpiY;

            GetWindowEffectiveDpi(hwnd, out dpiX, out dpiY);

            return CalculateScale(dpiX, dpiY, 96, 96);
        }

        /// <summary>
        /// Gets the virtual pixel scale for a monitor. This is essentially
        /// the Effective DPI of the monitor versus a standard 96 DPI.
        /// </summary>
        /// <param name="hMonitor">The h monitor.</param>
        /// <returns></returns>
        public static DpiScale GetVirtualPixelScaleByMonitor(IntPtr hMonitor)
        {
            uint dpiX;
            uint dpiY;

            GetMonitorEffectiveDpi(hMonitor, out dpiX, out dpiY);

            return CalculateScale(dpiX, dpiY, 96, 96);
        }

        #endregion

        #region Effective DPI

        /// <summary>
        /// Get the Effective DPI of a monitor after a user's accessibility
        /// preferences have been applied.
        /// </summary>
        /// <param name="hMonitor">The monitor.</param>
        /// <param name="dpiX">The dpi x.</param>
        /// <param name="dpiY">The dpi y.</param>
        public static void GetMonitorEffectiveDpi(IntPtr hMonitor, out uint dpiX, out uint dpiY)
        {
            dpiX = 96;
            dpiY = 96;

            ProcessDpiAwareness awareness = DpiAwareness;

            if (awareness >= ProcessDpiAwareness.PerMonitorDpiAware)
            {
                int hresult = NativeMethods.GetDpiForMonitor(hMonitor, NativeMethods.Monitor_DPI_Type.MDT_Effective_DPI, ref dpiX, ref dpiY);

                if (hresult != 0)
                {
                    // If anything goes wrong, return a reasonable default

                    dpiX = 96;
                    dpiY = 96;
                }
            }
            else if (awareness == ProcessDpiAwareness.SystemDpiAware)
            {
                GetSystemEffectiveDpi(out dpiX, out dpiY);
            }
            else
            {
                // Use the default of 96
                return;
            }
        }

        /// <summary>
        /// Get the System Effective DPI.
        /// </summary>
        /// <remarks>
        /// <para>The System Effective DPI is derived by the operating system
        /// by looking across all monitors and determining an "Effective DPI" that works
        /// well across all the screens for applications that do not support
        ///  per-monitor DPI.</para>
        /// </remarks>
        /// <param name="dpiX">The dpi x.</param>
        /// <param name="dpiY">The dpi y.</param>
        public static void GetSystemEffectiveDpi(out uint dpiX, out uint dpiY)
        {
            IntPtr handle = NativeMethods.GetDC(IntPtr.Zero);

            int x = NativeMethods.GetDeviceCaps(handle, (int)NativeMethods.DeviceCap.LOGPIXELSX);
            int y = NativeMethods.GetDeviceCaps(handle, (int)NativeMethods.DeviceCap.LOGPIXELSY);

            // If anything goes wrong, return a reasonable DPI

            dpiX = x > 0 ? (uint)x : 96;
            dpiY = y > 0 ? (uint)y : 96;
        }

        /// <summary>
        /// Get the Effective DPI of a windows (monitor) after a user's accessibility
        /// preferences have been applied.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <param name="dpiX">The dpi x.</param>
        /// <param name="dpiY">The dpi y.</param>
        public static void GetWindowEffectiveDpi(Window window, out uint dpiX, out uint dpiY)
        {
            GetWindowEffectiveDpi(GetWindowHwnd(window), out dpiX, out dpiY);
        }

        /// <summary>
        /// Get the Effective DPI of a windows (monitor) after a user's accessibility
        /// preferences have been applied.
        /// </summary>
        /// <param name="hwnd">The window.</param>
        /// <param name="dpiX">The dpi x.</param>
        /// <param name="dpiY">The dpi y.</param>
        public static void GetWindowEffectiveDpi(IntPtr hwnd, out uint dpiX, out uint dpiY)
        {
            IntPtr hMonitor = GetMonitorFromWindow(hwnd);

            GetMonitorEffectiveDpi(hMonitor, out dpiX, out dpiY);
        }

        #endregion

        #region Miscellaneous

        /// <summary>
        /// Gets scaling factor for a monitor
        /// This is defined as current pixel size relative to effective DPI
        /// </summary>
        /// <param name="deviceName">Monitor device name or null for the primary monitor</param>
        /// <returns></returns>
        public static double GetScreenScalingFactor(string deviceName = null)
        {
            // WIFRY: I tried to replace this method with one of the other methods in this class,
            // but for some reason, it generates a result that I can't quite match. Therefore,
            // I'm leaving this method in place as it seems to work and service a specific need.
            // Ultimately, it might be nice to find a way to consolidate it later.

            var dc = NativeMethods.CreateDC("DISPLAY", deviceName, null, IntPtr.Zero);
            if (dc == IntPtr.Zero)
                return 1.0d;

            int LogicalScreenHeight = NativeMethods.GetDeviceCaps(dc, (int)NativeMethods.DeviceCap.VERTRES);
            int PhysicalScreenHeight = NativeMethods.GetDeviceCaps(dc, (int)NativeMethods.DeviceCap.DESKTOPVERTRES);

            double ScreenScalingFactor = (double)PhysicalScreenHeight / (double)LogicalScreenHeight;

            System.Diagnostics.Trace.WriteLine("Monitor:\"" + deviceName + "\" Scaling factor:" + ScreenScalingFactor
                 + " Logical height:" + LogicalScreenHeight + " Physical height:" + PhysicalScreenHeight);

            NativeMethods.DeleteDC(dc);

            return ScreenScalingFactor;
        }

        #endregion

        #region Helpers

        private static IntPtr GetWindowHwnd(Window window)
        {
            return NativeMethods.GetWindowHwnd(window);
        }

        private static IntPtr GetMonitorFromWindow(IntPtr hwnd)
        {
            return NativeMethods.GetMonitorFromWindow(hwnd);
        }

        #endregion
    }
}
