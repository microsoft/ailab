// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using SnipInsight.Util;

namespace SnipInsight.ImageCapture
{
    public class DpiScalors
    {
        public double X;
        public double Y;
    }

    public static class DpiScalor
    {
        /// <summary>
        /// Gets scaling factor for a monitor
        /// This is defined as current pixel size relative to effective DPI
        /// </summary>
        /// <param name="deviceName">Monitor device name or null for the primary monitor</param>
        /// <returns></returns>
        public static double GetScreenScalingFactor(string deviceName = null)
        {
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

        public static DpiScalors GetScalor()
        {
            return GetScalor(AppManager.TheBoss.MainWindow);
        }

        public static DpiScalors GetScalor(Window window)
        {
            DpiScalors scalor = null;
            try
            {
                PresentationSource MainWindowPresentationSource = PresentationSource.FromVisual(window);

                Matrix m;
                if (MainWindowPresentationSource != null)
                {
                    m = MainWindowPresentationSource.CompositionTarget.TransformToDevice;
                }
                else
                {
                    using (var src = new HwndSource(new HwndSourceParameters()))
                    {
                        m = src.CompositionTarget.TransformToDevice;
                    }
                }

                scalor = new DpiScalors();
                scalor.X = m.M11;
                scalor.Y = m.M22;
            }
            catch (Exception ex)
            {
                Diagnostics.LogException(ex);
            }
            return scalor;
        }
    }

}
