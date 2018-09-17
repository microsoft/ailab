// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using WinInterop = System.Windows.Interop;
using SnipInsight.Util;
using System.Security;
using DpiScale = SnipInsight.Util.DpiScale;

namespace SnipInsight.Controls
{
    public class DpiAwareWindow : Window
    {
        private IntPtr _hwnd;

        public DpiAwareWindow()
        {
            RecalculateSystemScale();

            Loaded += DpiAwareWindow_Loaded;
        }

        private void DpiAwareWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Save our Hwnd for future use
            _hwnd = NativeMethods.GetWindowHwnd(this);

            WinInterop.HwndSource.FromHwnd(_hwnd).AddHook(new WinInterop.HwndSourceHook(WindowProc));

            RecalculateSystemScale();
            RecalculateMonitorScale();

            // Just to be extra sure that this fires for the first time!
            // There seems to be an occassional race condition where the Visual
            // Child doesn't exist, so this extra call should prevent that.
            //
            // I believe that the root cause is that occassionally the DpiChanged
            // method happens before our visual tree is loaded. This ensures that
            // we don't fail to set the ScaleTransform.
            ApplyScaleTransformToWindow();
        }

        private System.IntPtr WindowProc(
            System.IntPtr hwnd,
            int msg,
            System.IntPtr wParam,
            System.IntPtr lParam,
            ref bool handled)
        {
            switch (msg)
            {
                case (int)NativeMethods.WindowMsg.WM_DPICHANGED: // DPI Changed
                    NativeMethods.RECT rect = (NativeMethods.RECT)PtrToStructure(lParam, typeof(NativeMethods.RECT));

                    // It's important to call these methods to set the Scale
                    // variables before attempting to move the window. The location of
                    // these methods were critical to creating the desired effect.
                    //
                    // There is a subclass called AriModernWindow that handles Min/Max
                    // events. To function correctly, it needs the Scale variables to be set.
                    // The SetWindowPos caused the Mix/Max events to fire; therefore, we
                    // must call these methods first, before positioning the window.
                    RecalculateSystemScale();
                    RecalculateMonitorScale();

                    // SWP_NOZORDER | SWP_NOOWNERZORDER | SWP_NOACTIVATE = 0x214
                    NativeMethods.SetWindowPos(hwnd, IntPtr.Zero, rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top, (uint)(NativeMethods.SetWindowPosFlags.SWP_NOZORDER | NativeMethods.SetWindowPosFlags.SWP_NOOWNERZORDER | NativeMethods.SetWindowPosFlags.SWP_NOACTIVATE));

                    handled = false;
                    break;
            }

            return (System.IntPtr)0;
        }

        #region DpiScalors

        private uint _systemDpiX = 96;
        private uint _systemDpiY = 96;

        private DpiScale _monitorScale = new DpiScale(1, 1);
        private DpiScale _virtualPixelScale = new DpiScale(1, 1);
        private DpiScale _systemScale = new DpiScale(1, 1);

        /// <summary>
        /// Gets the monitor scale, which is the monitor DPI versus System DPI.
        /// </summary>
        /// <value>
        /// The monitor scale.
        /// </value>
        /// <remarks>
        /// <para>This is the ratio of the Effective DPI of a specific Monitor, versus it's
        /// physical DPI.</para>
        /// <para>When developing a DPI-aware application, this is useful for applying a ScaleTransform
        /// to your window so all fonts and graphics are scaled and rendered beautifully based on the
        /// physical capabilities of the display and the accessibility settings of the user.</para>
        /// </remarks>
        protected DpiScale MonitorScale
        {
            get { return _monitorScale; }
            private set
            {
                ThrowIfNullArgument(value);

                if (!value.Equals(_monitorScale))
                {
                    _monitorScale = value;
                    OnMonitorScaleChanged();
                }
            }
        }

        /// <summary>
        /// Gets the virtual pixel scale, which is the monitor's effective DPI versus 96 DPI.
        /// </summary>
        /// <value>
        /// The virtual pixel scale.
        /// </value>
        /// <remarks>
        /// <para>This is the ratio of the Effective DPI of a specific Monitor, versus 96 DPI.</para>
        /// <para>When developing a DPI-aware application, this is useful for responding to
        /// Min/Max Height/Width requests. The values that you return must be scaled against
        /// the VirtualPixelScale of the Window.</para>
        /// </remarks>
        protected DpiScale VirtualPixelScale
        {
            get { return _virtualPixelScale; }
            private set
            {
                ThrowIfNullArgument(value);

                if (!value.Equals(_virtualPixelScale))
                {
                    _virtualPixelScale = value;
                }
            }
        }

        /// <summary>
        /// Gets the virtual pixel scale, which is the system's effective DPI versus 96 DPI.
        /// </summary>
        /// <value>
        /// The virtual pixel scale.
        /// </value>
        /// <remarks>
        /// <para>The System Effective DPI is derived by the operating system
        /// by looking across all monitors and determining an "Effective DPI" that works
        /// well across all the screens for applications that do not support
        ///  per-monitor DPI.</para>
        /// <para>When developing a DPI-aware application, this is useful for translating
        /// window geometry (Left, Top, Width, Height) against the screen coordinates and
        /// across monitors.</para>
        /// </remarks>
        protected DpiScale SystemScale
        {
            get { return _systemScale; }
            private set
            {
                ThrowIfNullArgument(value);

                if (!value.Equals(_systemScale))
                {
                    _systemScale = value;
                }
            }
        }

        /// <summary>
        /// Called when the MonitorScale has changed. This method handles local changes
        /// to ensure they occur before the OnMonitorScaleChanged event happens.
        /// </summary>
        private void OnMonitorScaleChanged()
        {
            ApplyScaleTransformToWindow();
        }

        private void ApplyScaleTransformToWindow()
        {
#if (DEBUG)
            System.Diagnostics.Debug.WriteLine("Scale X=" + MonitorScale.X.ToString() + ", Y=" + MonitorScale.Y.ToString());
#endif

            if (VisualChildrenCount == 0)
                return;

            var child = GetVisualChild(0);

            if (MonitorScale.X == 1 && MonitorScale.Y == 1)
            {
                child.SetValue(LayoutTransformProperty, null);
            }
            else
            {
                child.SetValue(LayoutTransformProperty, new ScaleTransform(MonitorScale.X, MonitorScale.Y));
            }
        }


        private void ThrowIfNullArgument(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }
        }

        private uint lastDpiX = 0;
        private uint lastDpiY = 0;

        private void RecalculateMonitorScale()
        {
            uint dpiX;
            uint dpiY;

            DpiUtilities.GetWindowEffectiveDpi(_hwnd, out dpiX, out dpiY);

            if (dpiX != lastDpiX && dpiY != lastDpiY)
            {
                VirtualPixelScale = DpiUtilities.CalculateScale(dpiX, dpiY, 96, 96);

                lastDpiX = dpiX;
                lastDpiY = dpiY;
            }
        }

        private void RecalculateSystemScale()
        {
            DpiUtilities.GetSystemEffectiveDpi(out _systemDpiX, out _systemDpiY);

            SystemScale = DpiUtilities.CalculateScale(_systemDpiX, _systemDpiY, 96, 96);

#if (DEBUG)
            System.Diagnostics.Debug.WriteLine("System DPI = " + _systemDpiX.ToString());
#endif
        }

        #endregion

        [SecurityCritical]
        private static Object PtrToStructure(IntPtr lparam, Type clrType)
        {
            return Marshal.PtrToStructure(lparam, clrType);
        }
    }
}
