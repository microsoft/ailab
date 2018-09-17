// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Globalization;
using System.Windows;
using SnipInsight.ImageCapture;
using SnipInsight.Util;
using DpiScale = SnipInsight.Util.DpiScale;

namespace SnipInsight.Views
{
    internal static class LayoutUtilities
    {
        #region Position Window

        public static void PositionWindowOnPrimaryWorkingArea(Window window, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
        {
            PositionWindowOnScreen(window, System.Windows.Forms.Screen.PrimaryScreen, horizontalAlignment, verticalAlignment);
        }

        public static void PositionWindowOnPrimaryWorkingAreaWithOffset(Window window, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, float hOffset, float vOffset)
        {
            PositionWindowOnScreenWithOffset(window, System.Windows.Forms.Screen.PrimaryScreen, horizontalAlignment, verticalAlignment, hOffset, vOffset);
        }

        public static void PositionWindowOnPrimaryWorkingAreaWithOffsetIngnoringSystemDPI(Window window, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, float hOffset, float vOffset)
        {
            PositionWindowOnScreenWithOffsetIgnoringSystemDPI(window, System.Windows.Forms.Screen.PrimaryScreen, horizontalAlignment, verticalAlignment, hOffset, vOffset);
        }

        public static void PositionWindowOnScreen(Window window, System.Windows.Forms.Screen screen, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
        {
            PositionWindowOnScreenWithOffset(window, screen, horizontalAlignment, verticalAlignment, 0.0f, 0.0f);
        }

        public static void PositionWindowOnScreenWithOffset(Window window, System.Windows.Forms.Screen screen, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, float hOffset, float vOffset)
        {
            var workingArea = screen.WorkingArea;
            var dpiScale = DpiUtilities.GetSystemScale();

            double workingLeft = (workingArea.Left + hOffset) / dpiScale.X;
            double workingWidth = workingArea.Width / dpiScale.X;
            double workingTop = (workingArea.Top + vOffset) / dpiScale.Y;
            double workingHeight = workingArea.Height / dpiScale.Y;

            PositionWindow(window, workingLeft, workingTop, workingWidth, workingHeight, horizontalAlignment, verticalAlignment);
        }

        public static void PositionWindowOnScreenWithOffsetIgnoringSystemDPI(Window window, System.Windows.Forms.Screen screen, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, float hOffset, float vOffset)
        {
            System.IntPtr monitor = NativeMethods.GetMonitorFromWindow(NativeMethods.GetWindowHwnd(window));


            var _screenProps = new ScreenProperties();
            _screenProps.GetMonitorsInformation();
            var monitorInfo = _screenProps.GetMonitorInformation(monitor);
            var workingArea = monitorInfo.rcWork;
            var dpiScale = DpiUtilities.CalculateScale((uint)monitorInfo.dpiX, (uint)monitorInfo.dpiY, 96, 96);
            double workingLeft = (workingArea.left + hOffset) / dpiScale.X;
            double workingWidth = workingArea.width / dpiScale.X;
            double workingTop = (workingArea.top + vOffset) / dpiScale.Y;
            double workingHeight = workingArea.height / dpiScale.Y;

            PositionWindow(window, workingLeft, workingTop, workingWidth, workingHeight, horizontalAlignment, verticalAlignment);
        }

        public static void PositionWindow(Window window, double canvasLeft, double canvasTop, double canvasWidth, double canvasHeight, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
        {
            window.Left = AlignHorizontally(canvasLeft, canvasWidth, window.Width, horizontalAlignment);
            window.Top = AlignVertically(canvasTop, canvasHeight, window.Height, verticalAlignment);
        }

        /// <summary>
        /// Positions a window such that it is located entirely inside a specific canvas area.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <param name="canvasLeft">The canvas left.</param>
        /// <param name="canvasTop">The canvas top.</param>
        /// <param name="canvasWidth">Width of the canvas.</param>
        /// <param name="canvasHeight">Height of the canvas.</param>
        public static void PositionWindowInsideCanvas(Window window, double canvasLeft, double canvasTop, double canvasWidth, double canvasHeight)
        {
            double winLeft = window.Left;
            double winTop = window.Top;
            double winWidth = window.ActualWidth;
            double winHeight = window.ActualHeight;

            if (winLeft + winWidth > canvasLeft + canvasWidth)
            {
                window.Left = AlignHorizontally(canvasLeft, canvasWidth, winWidth, HorizontalAlignment.Right);
            }

            if (winTop + winHeight > canvasTop + canvasHeight)
            {
                window.Top = AlignVertically(canvasTop, canvasHeight, winHeight, VerticalAlignment.Bottom);
            }

            if (winLeft < canvasLeft)
            {
                window.Left = AlignHorizontally(canvasLeft, canvasWidth, winWidth, HorizontalAlignment.Left);
            }

            if (winTop < canvasTop)
            {
                window.Top = AlignVertically(canvasTop, canvasHeight, winHeight, VerticalAlignment.Top);
            }
        }

        public static void PositionWindowInsideWorkingArea(Window window)
        {
            var workingArea = GetWorkingAreaInSystemScale(window);

            PositionWindowInsideCanvas(window, workingArea.Item1, workingArea.Item2, workingArea.Item3, workingArea.Item4);
        }

        #endregion

        #region Alignment

        public static double AlignVertically(double canvasTop, double canvasHeight, double objectHeight, VerticalAlignment alignment)
        {
            return AlignEdge(canvasTop, canvasHeight, objectHeight, (int)alignment);
        }

        public static double AlignHorizontally(double canvasLeft, double canvasWidth, double objectWidth, HorizontalAlignment alignment)
        {
            return AlignEdge(canvasLeft, canvasWidth, objectWidth, (int)alignment);
        }

        private static double AlignEdge(double canvasStartPosition, double canvasLength, double objectLength, int alignment)
        {
            switch (alignment)
            {
                case 0: // Left or Top
                    return canvasStartPosition;
                case 2: // Right or Bottom;
                    return canvasStartPosition + canvasLength - objectLength;
                default: // Center
                    return canvasStartPosition + ((canvasLength - objectLength)) / 2;
            }
        }

        #endregion

        #region Window Location Storage

        public static string SaveWindowLocationToString(Window window)
        {
            string result = null;

            if (window != null)
            {
                // We are rounding doubles to ints to simplify storage

                int left = (int)window.Left;
                int top = (int)window.Top;
                int width = (int)window.Width;
                int height = (int)window.Height;
                bool isMaximized = window.WindowState == WindowState.Maximized;

                result = left.ToString(CultureInfo.InvariantCulture)
                         + "," + top.ToString(CultureInfo.InvariantCulture)
                         + "," + width.ToString(CultureInfo.InvariantCulture)
                         + "," + height.ToString(CultureInfo.InvariantCulture)
                         + "," + (isMaximized ? "1" : "0");
            }

            return result;
        }

        public static void RestoreWindowLocation(Window window, string location, bool includeSize = true)
        {
            return;
        }

        private static int? ParseStringPartAsInt(string[] parts, int index)
        {
            if (index <= parts.Length - 1)
            {
                string part = parts[index];

                int value;

                if (int.TryParse(part, out value))
                {
                    return value;
                }
            }

            return null;
        }

        #endregion

        #region Screen + WorkingAreas

        /// <summary>
        /// Gets the working area in virtual pixels (Left, Top, Width, Height).
        /// </summary>
        /// <param name="window">The window.</param>
        /// <returns></returns>
        public static Tuple<double, double, double, double> GetWorkingAreaInVirtualPixels(Window window)
        {
            return GetWorkingAreaInVirtualPixels(window, GetDpiScale(window));
        }

        /// <summary>
        /// Gets the working area in virtual pixels (Left, Top, Width, Height).
        /// </summary>
        /// <param name="window">The window.</param>
        /// <returns></returns>
        public static Tuple<double, double, double, double> GetWorkingAreaInVirtualPixels(Window window, DpiScale dpiScale)
        {
            var workingArea = GetWorkingArea(window);

            return new Tuple<double, double, double, double>(workingArea.Item1 / dpiScale.X,
                                                             workingArea.Item2 / dpiScale.Y,
                                                             workingArea.Item3 / dpiScale.X,
                                                             workingArea.Item4 / dpiScale.Y);
        }

        public static Tuple<double, double, double, double> GetWorkingAreaInSystemScale(Window window)
        {
            var dpiScale = DpiUtilities.GetSystemScale();
            var workingArea = GetWorkingArea(window);

            return new Tuple<double, double, double, double>(workingArea.Item1 / dpiScale.X,
                                                             workingArea.Item2 / dpiScale.Y,
                                                             workingArea.Item3 / dpiScale.X,
                                                             workingArea.Item4 / dpiScale.Y);
        }

        public static Tuple<double, double, double, double> GetWorkingArea(Window window)
        {
            return GetWorkingArea(NativeMethods.GetWindowHwnd(window));
        }

        public static Tuple<double, double, double, double> GetWorkingArea(IntPtr hwnd)
        {
            return GetWorkingAreaForMonitor(NativeMethods.GetMonitorFromWindow(hwnd));
        }

        public static Tuple<double, double, double, double> GetWorkingAreaForMonitor(IntPtr monitor)
        {
            NativeMethods.MONITORINFO monitorInfo = new NativeMethods.MONITORINFO();
            NativeMethods.GetMonitorInfo(monitor, monitorInfo);
            NativeMethods.RECT rcWorkArea = monitorInfo.rcWork;
            NativeMethods.RECT rcMonitorArea = monitorInfo.rcMonitor;

            return new Tuple<double, double, double, double>(rcWorkArea.left, rcWorkArea.top, rcWorkArea.width, rcWorkArea.height);
        }

        private static System.Windows.Forms.Screen GetScreen(Window window)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;

            return System.Windows.Forms.Screen.FromHandle(hwnd);
        }

        #endregion

        #region DPI

        /// <summary>
        /// Returns DPI (X, Y)
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public static DpiScale GetDpiScale(Window window)
        {
            return DpiUtilities.GetVirtualPixelScale(window);
        }

        #endregion

        #region Helpers

        private static int DoubleToInt32(double value)
        {
            return (int)Math.Ceiling(value);
        }

        #endregion
    }
}
