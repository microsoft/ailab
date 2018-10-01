// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Windows;
using SnipInsight.Util;
using Point = System.Drawing.Point;

namespace SnipInsight.ImageCapture
{
    public class AreaSelection
    {
        /// <summary>
        /// detect the UI element for the point cursor is
        /// </summary>
        private SmartBoundaryDetection _boundaryDetection = null;

        /// <summary>
        /// Cache of multiple monitor properties
        /// </summary>
        private ScreenProperties _screenProps = null;

        /// <summary>
        /// the UI element where the cursor point is
        /// </summary>
        private IntPtr _detectedElementHnd = IntPtr.Zero;

        /// <summary>
        /// When user have left mouse down, we will start to select area
        /// </summary>
        private bool _selectingArea = false;

        /// <summary>
        /// The point left mouse clicks
        /// </summary>
        private Point _clickPoint = new Point();

        /// <summary>
        /// The point mouse position
        /// </summary>
        private Point _mousePoint = new Point();

        /// <summary>
        /// the selectedArea coordinators
        /// </summary>
        private NativeMethods.RECT _selectedArea = new NativeMethods.RECT();

        private const string LogId = "AreaSelection:";

        private const int MinSelectedAreaSide = 10;

        private static int _borderWidth;
        private static int _borderHeight;

        /// <summary>
        /// Get system metrics (default window border width) at application startup
        /// Changes to system metrics require machine reboot, so it is OK to cache the values
        /// </summary>
        static AreaSelection()
        {
            // Default border thickness includes resize frame + padding. High DPI scaling is not needed here.
            // See https://connect.microsoft.com/VisualStudio/feedback/details/763767
            var borderPadding = NativeMethods.GetSystemMetrics((int)NativeMethods.SystemMetrixIndex.SM_CXPADDEDBORDER);
            _borderWidth = NativeMethods.GetSystemMetrics((int)NativeMethods.SystemMetrixIndex.SM_CXSIZEFRAME) + borderPadding;
            _borderHeight = NativeMethods.GetSystemMetrics((int)NativeMethods.SystemMetrixIndex.SM_CYSIZEFRAME) + borderPadding;
        }

        public bool SelectingArea
        {
            get { return _selectingArea; }
        }

        public ScreenProperties.MonitorInformation GetMonitorInformation(IntPtr hMonitor)
        {
            return _screenProps.GetMonitorInformation(hMonitor);
        }

        public List<ScreenProperties.MonitorInformation> GetMonitorsInformation()
        {
            var res = new List<ScreenProperties.MonitorInformation>();

            NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
                {
                    res.Add(GetMonitorInformation(hMonitor));
                    return true;
                }, IntPtr.Zero);

            return res;
        }

        public bool DraggingRight
        {
            get { return _mousePoint.X >= _clickPoint.X; }
        }

        public bool DraggingDown
        {
            get { return _mousePoint.Y >= _clickPoint.Y; }
        }

        public ScreenProperties ScreenProps
        {
            get { return _screenProps; }
        }

        public AreaSelection()
        {
            _screenProps = new ScreenProperties();
            _boundaryDetection = new SmartBoundaryDetection(_screenProps);
        }

        /// <summary>
        /// When mouse up we end dragging
        /// </summary>
        public void EndDragging()
        {
            _selectingArea = false;
        }

        /// <summary>
        /// When mouse clicks, we start to drag
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void StartDragging(int x, int y)
        {
            _selectingArea = true;
            _clickPoint = new Point(x, y);
        }

        /// <summary>
        /// When mouse moves you might be choosing the area
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Dragging(int x, int y)
        {
            NativeMethods.RECT rect = new NativeMethods.RECT();
            _mousePoint.X = x;
            _mousePoint.Y = y;

            if (!_selectingArea)
            {
                rect = AutoDetectWindow(x, y);
            }
            else
            {
                rect = SelectArea(x, y);
                if (!EmptyRect(rect))
                {
                    _detectedElementHnd = IntPtr.Zero;
                    _selectedArea = rect;
                }
            }
        }

        internal NativeMethods.RECT GetSelectedArea()
        {
            if (EmptyRect(_selectedArea))
            {
                if (_detectedElementHnd != IntPtr.Zero)
                {
                    // Bounding rectangle defines the outer limits of the window area
                    var rect = _boundaryDetection.GetBoundaryRect(_detectedElementHnd);
                    System.Windows.Thickness borderThickness;
                    double monitorScaling = 1.0d;

                    NativeMethods.WINDOWPLACEMENT wndPlacement;
                    NativeMethods.GetWindowPlacement(_detectedElementHnd, out wndPlacement);
                    bool fullScreen = wndPlacement.showCmd == NativeMethods.ShowWindowCommands.SW_SHOWMAXIMIZED;

                    var monitor = _boundaryDetection.GetMonitor(_detectedElementHnd);
                    if (monitor != IntPtr.Zero)
                    {
                        var monitorInfo = _screenProps.GetMonitorInformation(monitor);
                        NativeMethods.RECT rcWorkArea = monitorInfo.rcWork;

                        // Calculate monitor scaling relative to primary monitor
                        var monitorScalingFactor = monitorInfo.scalingFactor;
                        var defaultScalingFactor = _screenProps.GetMonitorInformation(IntPtr.Zero).scalingFactor;
                        monitorScaling = monitorScalingFactor / defaultScalingFactor;

                        // Window may be maximized even if SW_SHOWMAXIMIZED is not set - check the monitor work area
                        if (fullScreen ||
                            rect.left == rcWorkArea.left &&
                            rect.top == rcWorkArea.top &&
                            rect.width == (rcWorkArea.right - rcWorkArea.left) &&
                            rect.height == (rcWorkArea.bottom - rcWorkArea.top))
                        {
                            return new NativeMethods.RECT
                            {
                                left = (int)(rcWorkArea.left * monitorScaling),
                                right = (int)(rcWorkArea.right * monitorScaling),
                                top = (int)(rcWorkArea.top * monitorScaling),
                                bottom = (int)(rcWorkArea.bottom * monitorScaling),
                            };
                        }
                        // fall through
                    }

                    // Get client rectangle to check if it uses custom or default frame
                    NativeMethods.RECT clientRect;
                    NativeMethods.GetClientRect(_detectedElementHnd, out clientRect);

                    // For normal frames retain 1px border around sides and bottom and leave the window title bar on the top
                    // Owner drawn frames may not have the border - make sure clientRect is left intact
                    borderThickness = new System.Windows.Thickness(
                        Math.Min(Math.Max(_borderWidth - 1, 0), (rect.width - (clientRect.right - clientRect.left)) / 2),
                        0,
                        Math.Min(Math.Max(_borderWidth - 1, 0), (rect.width - (clientRect.right - clientRect.left)) / 2),
                        Math.Min(Math.Max(_borderHeight - 1, 0), rect.height - (clientRect.bottom - clientRect.top)));

                    return new NativeMethods.RECT
                    {
                        left = (int)((rect.left + borderThickness.Left) * monitorScaling),
                        right = (int)((rect.right - borderThickness.Right) * monitorScaling),
                        top = (int)((rect.top + borderThickness.Top) * monitorScaling),
                        bottom = (int)((rect.bottom - borderThickness.Bottom) * monitorScaling),
                    };
                }
            }
            return _selectedArea;
        }

        private NativeMethods.RECT AutoDetectWindow(int x, int y)
        {
            _detectedElementHnd = _boundaryDetection.GetTopElement(x, y);

            if (_detectedElementHnd != IntPtr.Zero)
            {
                var rect = _boundaryDetection.GetBoundaryRect(_detectedElementHnd);
                return rect;
            }
            return new NativeMethods.RECT();
        }

        private NativeMethods.RECT SelectArea(int x, int y)
        {
            NativeMethods.RECT selectedArea = new NativeMethods.RECT();

            //Calculate X Coordinates
            if (x < _clickPoint.X)
            {
                selectedArea.left = x;
                selectedArea.right = _clickPoint.X;
            }
            else
            {
                selectedArea.left = _clickPoint.X;
                selectedArea.right = x;
            }

            //Calculate Y Coordinates
            if (y < _clickPoint.Y)
            {
                selectedArea.top = y;
                selectedArea.bottom = _clickPoint.Y;
            }
            else
            {
                selectedArea.top = _clickPoint.Y;
                selectedArea.bottom = y;
            }
            return selectedArea;
        }

        private bool EmptyRect(NativeMethods.RECT rect)
        {
            var width = rect.right - rect.left;
            var height = rect.bottom - rect.top;
            if (width > MinSelectedAreaSide && height > MinSelectedAreaSide)
            {
                return false;
            }
            return true;
        }
    }
}
