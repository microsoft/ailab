// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using SnipInsight.Util;
using DpiScale = SnipInsight.Util.DpiScale;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;


namespace SnipInsight.ImageCapture
{
    /// <summary>
    /// Interaction logic for ImageCaptureWindow.xaml
    /// </summary>
    public partial class ImageCaptureWindow
    {
        enum MagnifierPosition
        {
            // magnifier stays at the cursor upper left quadrant
            UpperLeft,
            // magnifier stays at the cursor upper right quadrant
            UpperRight,
            // magnifier stays at the cursor lower left quadrant
            LowerRight,
            // magnifier stays at the cursor lower left quadrant
            LowerLeft,
            // magnifier overlays the cursor
            Center,
        }

        public static RoutedCommand CaptureFullScreenCommand = new RoutedCommand();
        public static RoutedCommand CaptureDoneCommand = new RoutedCommand();
        public static RoutedCommand CaptureCancelCommand = new RoutedCommand();
        public static RoutedCommand CaptureKeySpaceCommand = new RoutedCommand();
        public static RoutedCommand CaptureKeyRightCommand = new RoutedCommand();
        public static RoutedCommand CaptureKeyLeftCommand = new RoutedCommand();
        public static RoutedCommand CaptureKeyUpCommand = new RoutedCommand();
        public static RoutedCommand CaptureKeyDownCommand = new RoutedCommand();

        public event EventHandler NotifyCapturingDone;
        public event EventHandler NotifyCapturingCancel;

        private readonly AreaSelection _areaSelection;
        private readonly DpiScale _scalor;
        private readonly double _screenScalor;

        private ScreenshotImage _screenShotImage = null;

        private const int MagnifierRadius = 21;
        private const int MagnifierDiameter = MagnifierRadius*2;
        private const int MagnifierHorizonSpace = 8;
        private const int MagnifierVerticalSpace = 10;
        private const int MagnifierTextHeight = 10;
        private const int MagnifierHeight = MagnifierVerticalSpace + MagnifierDiameter + MagnifierTextHeight; //100
        private const int MagnifierWidth = MagnifierHorizonSpace + MagnifierDiameter;
        private const double MagnifierZoomFactor = 2;
        private bool ClickSpace = false;

        public ImageCaptureWindow(System.Drawing.Rectangle rect, AreaSelection areaSelection)
        {
            InitializeComponent();

            MouseUp += OnMouseUp;
            MouseDown += OnMouseDown;
            MouseMove += OnMouseMove;
            Loaded += OnMainWindowLoaded;
            _scalor = DpiUtilities.GetVirtualPixelScale(this);

            _areaSelection = areaSelection;
            var monitors = _areaSelection.GetMonitorsInformation();
            _screenShotImage = new ScreenshotImage();

            _screenScalor = DpiUtilities.GetScreenScalingFactor();
            _screenShotImage.SnapShot(rect, _screenScalor);

            WindowStartupLocation = WindowStartupLocation.Manual;

            SourceInitialized += (sender, e) =>
            {
                IntPtr hWnd = new WindowInteropHelper(this).Handle;
                NativeMethods.SetWindowPos(hWnd, (IntPtr)NativeMethods.SetWindowPosInsertAfter.HWND_TOP, rect.Left, rect.Top, rect.Width, rect.Height, 0);
            };

            var bmDesktopSource = ScreenCapture.GetBitmapSource(_screenShotImage.ScreenSnapshotImage);

            BackgroundImage.Fill = new ImageBrush(bmDesktopSource);
            // MagnifierBackgroundImage.Source = bmDesktopSource;
        }

        void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            var animateAnts = TryFindResource("animateAnts") as Storyboard;
            if (animateAnts != null)
            {
                animateAnts.Begin();
            }
        }

        private BitmapSource GetCapturedImage()
        {
            var rect = _areaSelection.GetSelectedArea();
            // decide which monitor the selected area belongs to by checking the mid point of the selected area
            var x = (rect.left + rect.width/2);
            var y = (rect.top + rect.height/2);

            var hMonitor = NativeMethods.MonitorFromPoint(new NativeMethods.POINT() { x = x, y = y }, 0);
            var curMon = _areaSelection.ScreenProps.GetMonitorInformation(hMonitor);
            var monitorScalingX = curMon.dpiX / 96 / _screenScalor;
            var monitorScalingY = curMon.dpiY / 96 / _screenScalor;
            var dpiScaler = new DpiScale(monitorScalingX, monitorScalingY);
            var capturedImage = _screenShotImage.GetCaptureImage(rect, dpiScaler);

            // Copies to clipboard based on user setting
            if (UserSettings.CopyToClipboardAfterSnip)
            {
                System.Windows.Clipboard.SetImage(capturedImage);
            }

            return capturedImage;
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            _areaSelection.EndDragging();
            var capturedImage = GetCapturedImage();

            if (NotifyCapturingDone != null)
            {
                NotifyCapturingDone(this, new ImageCaptureEventArgs(capturedImage));
            }
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            var p = Mouse.GetPosition(this);
            var pos = PointToScreen(p);
            _areaSelection.StartDragging((int)pos.X, (int)pos.Y);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var p = Mouse.GetPosition(this);
            var cursorPos = PointToScreen(p);

            _areaSelection.Dragging((int)cursorPos.X, (int)cursorPos.Y);
            var selectedArea = _areaSelection.GetSelectedArea();

            UpdateSelectedArea(selectedArea);

            //double length = MagnifierCircle.ActualWidth / MagnifierZoomFactor;
            //double radius = (length - 1) / 2;
            //Rect viewboxRect = new Rect(p.X * _screenScalor - radius, (p.Y * _screenScalor - radius), length, length);
            //MagnifierBrush.Viewbox = viewboxRect;

            var width = selectedArea.right - selectedArea.left;
            var height = selectedArea.bottom - selectedArea.top;
            MagnifierText.Text = string.Format("{0} x {1}", width, height);

            UpdateMagnifierPosition(p);
            MagnifierPanel.Visibility = Visibility.Visible;
        }

        internal void CapturingFullScreen(object sender, ExecutedRoutedEventArgs e)
        {
            var capturedImage = ScreenCapture.GetBitmapSource(_screenShotImage.ScreenSnapshotImage);

            if (NotifyCapturingDone != null)
            {
                NotifyCapturingDone(this, new ImageCaptureEventArgs(capturedImage));
            }
        }

        internal void CapturingDone(object sender, ExecutedRoutedEventArgs e)
        {
            var capturedImage = GetCapturedImage();

            if (NotifyCapturingDone != null)
            {
                NotifyCapturingDone(this, new ImageCaptureEventArgs(capturedImage));
            }
        }

        internal void CapturingCancel(object sender, ExecutedRoutedEventArgs e)
        {
            if (NotifyCapturingCancel != null)
            {
                NotifyCapturingCancel(this, null);
            }
        }

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        /// <summary>
        /// Move pointer using mouse
        /// </summary>
        internal void CapturingUp(object sender, ExecutedRoutedEventArgs e)
        {
            var p = Mouse.GetPosition(this);
            var cursorPos = PointToScreen(p);

            SetCursorPos((int)cursorPos.X, (int)cursorPos.Y-10);

        }

        /// <summary>
        /// Move pointer using mouse
        /// </summary>
        internal void CapturingDown(object sender, ExecutedRoutedEventArgs e)
        {
            var p = Mouse.GetPosition(this);
            var cursorPos = PointToScreen(p);

            SetCursorPos((int)cursorPos.X, (int)cursorPos.Y + 10);

        }

        /// <summary>
        /// Move pointer using mouse
        /// </summary>
        internal void CapturingRight(object sender, ExecutedRoutedEventArgs e)
        {
            var p = Mouse.GetPosition(this);
            var cursorPos = PointToScreen(p);

            SetCursorPos((int)cursorPos.X + 10, (int)cursorPos.Y);

        }

        /// <summary>
        /// Move pointer using mouse
        /// </summary>
        internal void CapturingLeft(object sender, ExecutedRoutedEventArgs e)
        {
            var p = Mouse.GetPosition(this);
            var cursorPos = PointToScreen(p);

            SetCursorPos((int)cursorPos.X - 10, (int)cursorPos.Y);

        }

        /// <summary>
        /// Start and stop snip cropping
        /// </summary>
        internal void CapturingSpace(object sender, ExecutedRoutedEventArgs e)
        {
            if (!ClickSpace)
            {
                var p = Mouse.GetPosition(this);
                var pos = PointToScreen(p);
                _areaSelection.StartDragging((int)pos.X, (int)pos.Y);
            }
            else
            {
                _areaSelection.EndDragging();
                var capturedImage = GetCapturedImage();

                if (NotifyCapturingDone != null)
                {
                    NotifyCapturingDone(this, new ImageCaptureEventArgs(capturedImage));
                }
            }
            ClickSpace = !ClickSpace;
        }

        private void UpdateSelectedArea(NativeMethods.RECT area)
        {
            var topLeft = PointFromScreen(new System.Windows.Point(area.left, area.top));
            var bottomRight = PointFromScreen(new System.Windows.Point(area.right, area.bottom));

            ForegroundClip.Rect = new Rect(topLeft, bottomRight);
            ForegroundAnts.Width = bottomRight.X - topLeft.X;
            ForegroundAnts.Height = bottomRight.Y - topLeft.Y;
            ForegroundAntsTransform.X = topLeft.X;
            ForegroundAntsTransform.Y = topLeft.Y;
        }

#region Magnifier

        private MagnifierPosition GetMagnifierPosition(out double monitorScalingX, out double monitorScalingY)
        {
            var pos = System.Windows.Forms.Control.MousePosition;
            var screen = System.Windows.Forms.Screen.FromPoint(pos);

            var hMonitor = NativeMethods.MonitorFromPoint(new NativeMethods.POINT() { x = pos.X, y = pos.Y }, 0);
            var curMon = _areaSelection.ScreenProps.GetMonitorInformation(hMonitor);
            monitorScalingX = curMon.dpiX / 96 /_screenScalor;
            monitorScalingY = curMon.dpiY / 96 / _screenScalor;
//#if (TRACE)
//            System.Diagnostics.Trace.WriteLine("Screen :" + screen);
//            System.Diagnostics.Trace.WriteLine("_screenScalor :" + _screenScalor);
//            System.Diagnostics.Trace.WriteLine("pos x:" + pos.X + " y:" + pos.Y);
//            System.Diagnostics.Trace.WriteLine("monitorScalingX:" + monitorScalingX + " monitorScalingY:" + monitorScalingY);
//#endif

            var x = Math.Abs(pos.X - screen.Bounds.Left);
            var y = Math.Abs(pos.Y - screen.Bounds.Top);

            // the cursor is at the monitor's right edge
            bool isRight = x + MagnifierWidth * _scalor.X * monitorScalingX >= screen.Bounds.Width;
            // the cursor is at the monitor's left edge
            bool isLeft = x <= MagnifierWidth * _scalor.X * monitorScalingX;
            // the cursor is at the monitor's bottom edge
            bool isLower = y + MagnifierHeight * _scalor.Y * monitorScalingY >= screen.Bounds.Height;
            // the cursor is at the monitor's top edge
            bool isUpper = y <= MagnifierHeight * _scalor.Y * monitorScalingY;

            // Removed Magnifier Position, alwais center
            return MagnifierPosition.Center;
        }

        private void UpdateMagnifierPosition(System.Windows.Point p)
        {
            double left;
            double top;
            double monitorScalingX, monitorScalingY;
#if CHANGE_MAGNIFIER_POSITION
            var pos = MagnifierPosition.Center;
#else
            var pos = GetMagnifierPosition(out monitorScalingX, out monitorScalingY);
#endif

            switch (pos)
            {
                case MagnifierPosition.LowerLeft:
                    left = p.X - MagnifierWidth;
                    top = p.Y + MagnifierVerticalSpace;
                    break;
                case MagnifierPosition.UpperLeft:
                    left = p.X - MagnifierWidth;
                    top = p.Y - MagnifierHeight;
                    break;
                case MagnifierPosition.UpperRight:
                    left = p.X + MagnifierHorizonSpace;
                    top = p.Y - MagnifierHeight;
                    break;
                case MagnifierPosition.LowerRight:
                    left = p.X + MagnifierHorizonSpace;
                    top = p.Y + MagnifierVerticalSpace;
                    break;
                case MagnifierPosition.Center:
                    left = p.X - MagnifierRadius;
                    top = p.Y - MagnifierRadius;
                    break;
                default:
                    throw new InvalidOperationException("invalid MagnifierPosition");
            }

            MagnifierTransform.X = left;
            MagnifierTransform.Y = top;
            MagnifierScale.CenterX = p.X;
            MagnifierScale.CenterY = p.Y;
            MagnifierScale.ScaleX = monitorScalingX;
            MagnifierScale.ScaleY = monitorScalingY;
        }
#endregion
    }
}
