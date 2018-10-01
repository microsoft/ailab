// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WinInterop = System.Windows.Interop;
using SnipInsight.Util;
using DpiScale = SnipInsight.Util.DpiScale;

namespace SnipInsight.Controls.Ariadne
{
    public class AriModernWindow : DpiAwareWindow
    {
        ContentPresenter contentArea;
        TextBlock captionArea;

        public AriModernWindow()
        {
            SourceInitialized += ModernWindow_SourceInitialized;
        }

        static AriModernWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AriModernWindow), new FrameworkPropertyMetadata(typeof(AriModernWindow)));
        }

        #region Apply Template

        public override void OnApplyTemplate()
        {
            bool canMinimize = ResizeMode != System.Windows.ResizeMode.NoResize;
            bool canRestore = ResizeMode != System.Windows.ResizeMode.NoResize && ResizeMode != System.Windows.ResizeMode.CanMinimize;

            Button closeButton = GetTemplateChild("CloseButton") as Button;

            if (closeButton != null)
                closeButton.Click += closeButton_Click;

            Button restoreButton = GetTemplateChild("RestoreButton") as Button;

            if (restoreButton != null)
            {
                restoreButton.Click += restoreButton_Click;

                if (!canMinimize && !canRestore)
                    restoreButton.Visibility = Visibility.Collapsed;
                else if (!canRestore)
                    restoreButton.IsEnabled = false;
            }

            Button maxButton = GetTemplateChild("MaximizeButton") as Button;

            if (maxButton != null)
            {
                maxButton.Click += restoreButton_Click;

                if (!canMinimize && !canRestore)
                    maxButton.Visibility = Visibility.Collapsed;
                else if (!canRestore)
                    maxButton.IsEnabled = false;
            }

            Button minButton = GetTemplateChild("MinimizeButton") as Button;

            if (minButton != null)
            {
                minButton.Click += minButton_Click;

                if (!canMinimize)
                    minButton.Visibility = Visibility.Collapsed;
            }

            contentArea = GetTemplateChild("ContentArea") as ContentPresenter;
            captionArea = GetTemplateChild("CaptionArea") as TextBlock;

            if (ShowWindowCaption == true)
            {
                if (captionArea != null)
                    captionArea.Visibility = Visibility.Visible;
            }
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
                Close();
        }

        private void restoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Maximized)
                WindowState = System.Windows.WindowState.Normal;
            else
                WindowState = System.Windows.WindowState.Maximized;
        }

        private void minButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = System.Windows.WindowState.Minimized;
        }

        #endregion

        #region Virtual MinHeight + MinWidth

        /// <summary>
        /// Gets or sets the minimum width of the window. Setting MinWidth causes problems with DPI-Awareness,
        /// so we use this value instead.
        /// </summary>
        /// <value>
        /// The minimum width.
        /// </value>
        public double VirtualMinWidth
        {
            get { return (double)GetValue(VirtualMinWidthProperty); }
            set { SetValue(VirtualMinWidthProperty, value); }
        }

        public static readonly DependencyProperty VirtualMinWidthProperty =
            DependencyProperty.Register("VirtualMinWidth", typeof(double), typeof(AriModernWindow), new PropertyMetadata(double.NaN));

        /// <summary>
        /// Gets or sets the minimum height of the window. Setting MinHeight causes problems with DPI-Awareness,
        /// so we use this value instead.
        /// </summary>
        /// <value>
        /// The minimum height.
        /// </value>
        public double VirtualMinHeight
        {
            get { return (double)GetValue(VirtualMinHeightProperty); }
            set { SetValue(VirtualMinHeightProperty, value); }
        }

        public static readonly DependencyProperty VirtualMinHeightProperty =
            DependencyProperty.Register("VirtualMinHeight", typeof(double), typeof(AriModernWindow), new PropertyMetadata(double.NaN));

        #endregion

        #region Helper Properties

        public static readonly DependencyProperty ModernBorderThicknessProperty = DependencyProperty.Register("ModernBorderThickness", typeof(Thickness), typeof(AriModernWindow));

        public Thickness ModernBorderThickness
        {
            get { return (Thickness)GetValue(ModernBorderThicknessProperty); }
            set { SetValue(ModernBorderThicknessProperty, value); }
        }

        public static readonly DependencyProperty ModernBorderBrushProperty = DependencyProperty.Register("ModernBorderBrush", typeof(Brush), typeof(AriModernWindow), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 255, 0, 0))));

        public Brush ModernBorderBrush
        {
            get { return (Brush)GetValue(ModernBorderBrushProperty); }
            set { SetValue(ModernBorderBrushProperty, value); }
        }

        public static readonly DependencyProperty ModernCaptionBrushProperty = DependencyProperty.Register("ModernCaptionBrush", typeof(Brush), typeof(AriModernWindow), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 105, 105, 105))));

        public Brush ModernCaptionBrush
        {
            get { return (Brush)GetValue(ModernCaptionBrushProperty); }
            set { SetValue(ModernCaptionBrushProperty, value); }
        }

        public static readonly DependencyProperty ModernCaptionButtonBrushProperty = DependencyProperty.Register("ModernCaptionButtonBrush", typeof(Brush), typeof(AriModernWindow), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 68, 68, 68))));

        public Brush ModernCaptionButtonBrush
        {
            get { return (Brush)GetValue(ModernCaptionButtonBrushProperty); }
            set { SetValue(ModernCaptionButtonBrushProperty, value); }
        }

        public static readonly DependencyProperty ShowWindowCaptionProperty = DependencyProperty.Register("ShowWindowCaption", typeof(bool), typeof(AriModernWindow), new PropertyMetadata(true));

        public bool ShowWindowCaption
        {
            get { return (bool)GetValue(ShowWindowCaptionProperty); }
            set { SetValue(ShowWindowCaptionProperty, value); }
        }

        public static readonly DependencyProperty CaptionBarHeightProperty = DependencyProperty.Register("CaptionBarHeight", typeof(double), typeof(AriModernWindow), new PropertyMetadata(32.0));

        public double CaptionBarHeight
        {
            get { return (double)GetValue(CaptionBarHeightProperty); }
            set { SetValue(CaptionBarHeightProperty, value); }
        }

        #endregion

        #region Modern Window Resize

        void ModernWindow_SourceInitialized(object sender, EventArgs e)
        {
            System.IntPtr handle = (new WinInterop.WindowInteropHelper(this)).Handle;
            WinInterop.HwndSource.FromHwnd(handle).AddHook(new WinInterop.HwndSourceHook(WindowProc));
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
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);

                    // This may seem odd, but setting handled = false allows some extra
                    // system code to fire that is necessary for correct behavior when
                    // the DPI of the screen changes. There is a glitch with WPF and
                    // folks on the web have discovered that this is the fix.
                    handled = false;
                    break;
            }

            return (System.IntPtr)0;
        }

        private int GetMinWidthInScreenPixels(DpiScale dpiScale)
        {
            double value = double.IsNaN(VirtualMinWidth) ? SystemParameters.MinimumWindowWidth : VirtualMinWidth;

            return DoubleToInt32(value * dpiScale.X);
        }

        private int GetMinHeightInScreenPixels(DpiScale dpiScale)
        {
            double value = double.IsNaN(VirtualMinHeight) ? SystemParameters.MinimumWindowHeight : VirtualMinHeight;

            return DoubleToInt32(value * dpiScale.Y);
        }

        private void WmGetMinMaxInfo(System.IntPtr hwnd, System.IntPtr lParam)
        {
            NativeMethods.MINMAXINFO mmi = (NativeMethods.MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(NativeMethods.MINMAXINFO));

            // Adjust the maximized size and position to fit the work area of the correct monitor
            System.IntPtr monitor = NativeMethods.GetMonitorFromWindow(hwnd);

            // MinHeight and MinWidth need to be scaled from Virtual Pixels back to Logical Pixels
            DpiScale dpiScale = VirtualPixelScale;

            if (monitor != System.IntPtr.Zero)
            {
                NativeMethods.MONITORINFO monitorInfo = new NativeMethods.MONITORINFO();
                NativeMethods.GetMonitorInfo(monitor, monitorInfo);
                NativeMethods.RECT rcWorkArea = monitorInfo.rcWork;
                NativeMethods.RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = DoubleToInt32(Math.Abs(rcWorkArea.right - rcWorkArea.left));
                mmi.ptMaxSize.y = DoubleToInt32(Math.Abs(rcWorkArea.bottom - rcWorkArea.top));
                // After much research, it appears that the MaxTrackSize is used for secondary monitors
                // while MaxSize is used for the primary monitor.
                mmi.ptMaxTrackSize.x = DoubleToInt32(Math.Abs(rcWorkArea.right - rcWorkArea.left));
                mmi.ptMaxTrackSize.y = DoubleToInt32(Math.Abs(rcWorkArea.bottom - rcWorkArea.top));
                mmi.ptMinTrackSize.x = GetMinWidthInScreenPixels(dpiScale);
                mmi.ptMinTrackSize.y = GetMinHeightInScreenPixels(dpiScale);
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        [SecurityCritical]
        public static Object PtrToStructure(IntPtr lparam, Type clrType)
        {
            return Marshal.PtrToStructure(lparam, clrType);
        }

        private Int32 DoubleToInt32(Double value)
        {
            return (Int32)Math.Ceiling(value);
        }

        #endregion
    }
}
