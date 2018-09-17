// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using SnipInsight.Util;

namespace SnipInsight.ImageCapture
{
    /// <summary>
    /// Algorithm to detect the UI Element where the mouse cursor is
    /// </summary>
    public class SmartBoundaryDetection
    {
        /// <summary>
        /// Cache of multiple monitor properties
        /// </summary>
        private static ScreenProperties _screenProps;

        /// <summary>
        /// It has all the top level UI element handles
        /// </summary>
        private List<IntPtr> _topElementHnds = new List<IntPtr>();

        /// <summary>
        ///  the window handle corresponding zorder
        /// </summary>
        private Dictionary<IntPtr, int> _windowZOrderMapping = new Dictionary<IntPtr, int>();

        /// <summary>
        ///  the window handle corresponding Bounding rectangle
        /// </summary>
        private Dictionary<IntPtr, NativeMethods.RECT> _windowRectMapping = new Dictionary<IntPtr, NativeMethods.RECT>();

        /// <summary>
        ///  the window handle corresponding monitor handle
        /// </summary>
        private Dictionary<IntPtr, IntPtr> _windowMonitorMapping = new Dictionary<IntPtr, IntPtr>();

        private const string LogId = "SmartBoundaryDetection:";

        /// <summary>
        /// constructor
        /// As the capture will put two forms on the UI, SmartBoundaryDetection has
        /// to take a snapshot of the UI elements before the two forms show up
        /// </summary>
        public SmartBoundaryDetection(ScreenProperties screenProps)
        {
            _screenProps = screenProps;

            GetTopLevelWindows();
        }

        /// <summary>
        /// Get desktop outer boundaries in screen coordinates
        /// </summary>
        /// <returns></returns>
        public static Rectangle GetDesktopBounds()
        {
            _screenProps.GetMonitorsInformation();
            var rectangle = _screenProps.GetMaxRectangleFromMonitors();
            return rectangle;

            // GetShellWindow returns Windows Explorer window which covers all monitors.
            // Notes:
            // GetDesktopWindow returns a window for the primary monitor only.
            // SystemInformation.VirtualScreen may be incorrect if the primary monitor has low DPI
            //
            //////////////////////////////////////////////////////////////////////////////////////
            // NOTE: GetWindowRect returns a window using the MINIMUM rectangle that intersects every monitor, so 
            // it's dpi tied, no matter the monitor (primary or other type). When any monitor owns different DPI will 
            // returns wrong rectangle. Luckily worked mostly but not enought
            //////////////////////////////////////////////////////////////////////////////////////

            //Rectangle rctDeskTop;
            //IntPtr hWndProgMan = NativeMethods.GetShellWindow();

            //if (hWndProgMan != IntPtr.Zero)
            //{
            //    NativeMethods.RECT rect;
            //    NativeMethods.GetWindowRect(hWndProgMan, out rect);
            //    rctDeskTop = new Rectangle(rect.left, rect.top, rect.width, rect.height);
            //}
            //else
            //{
            //    // In the unlikely case shell window doesn't exist...
            //    rctDeskTop = new Rectangle(
            //        SystemInformation.VirtualScreen.Left,
            //        SystemInformation.VirtualScreen.Top,
            //        SystemInformation.VirtualScreen.Right - SystemInformation.VirtualScreen.Left,
            //        SystemInformation.VirtualScreen.Bottom - SystemInformation.VirtualScreen.Top
            //        );
            //}
            //return rctDeskTop;
        }

        /// <summary>
        /// get the top UI elements
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public IntPtr GetTopElement(int x, int y)
        {
            List<IntPtr> elementHnds = GetElementHndsInRange(_topElementHnds, x, y);
            if (elementHnds == null || elementHnds.Count == 0)
            {
                return IntPtr.Zero;
            }

            var minElementHandle = elementHnds[0];
            int minZOrder = int.MaxValue;
            foreach (var ohWnd in elementHnds)
            {
                int zOrder = 0;

                if (_windowZOrderMapping.TryGetValue(ohWnd, out zOrder))
                {
                    if (zOrder < minZOrder)
                    {
                        minZOrder = zOrder;
                        minElementHandle = ohWnd;
                    }
                }
            }

            // Check if the window is fully visible before looking for child windows
            if (minElementHandle == IntPtr.Zero)
                return IntPtr.Zero;

            var hdc = NativeMethods.GetDC(minElementHandle);
            if (hdc != IntPtr.Zero)
            {
                NativeMethods.RECT rcClip, rcClient;
                var ret = NativeMethods.GetClipBox(hdc, out rcClip);
                NativeMethods.GetClientRect(minElementHandle, out rcClient);
                NativeMethods.ReleaseDC(minElementHandle, hdc);
                if (ret == (int)NativeMethods.GetClipBoxReturn.SimpleRegion &&
                    rcClip.left == rcClient.left &&
                    rcClip.right == rcClient.right &&
                    rcClip.bottom == rcClient.bottom &&
                    rcClip.top == rcClient.top)
                {
                    IntPtr element = GetTopElement(minElementHandle, x, y);
                    if (element != IntPtr.Zero)
                    {
                        return element;
                    }
                }

            }
            return minElementHandle;
        }

        /// <summary>
        /// Return the UI windows with the smallest zorder, i.e. the top window
        /// </summary>
        /// <param name="eHnd"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public IntPtr GetTopElement(IntPtr eHnd, int x, int y)
        {
            List<IntPtr> childrenWHnds = new List<IntPtr>();
            try
            {
                NativeMethods.EnumChildWindows(
                    eHnd, (hWnd, lParam) =>
                    {
                        try
                        {
                            if (IsVisibleWindow(hWnd))
                            {
                                childrenWHnds.Add(hWnd);
                            }
                            return true;
                        }
                        catch (Exception ex)
                        {
                            Diagnostics.LogTrace("EnumChildrenWindowsProc Exception hWnd:" + hWnd);
                            Diagnostics.LogException(ex);
                            return false;
                        }

                    }, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                Diagnostics.LogException(ex);
            }

            if (childrenWHnds.Count > 0)
            {
                List<IntPtr> eleChildren = GetElementHndsInRange(childrenWHnds, x, y);
                if (eleChildren != null && eleChildren.Count > 0)
                {
                    if (eleChildren.Count > 1)
                    {
                        //TODO(qiazhang) should we return the smallest window?
                        return eleChildren[0];
                    }
                }
            }
            return eHnd;
        }

        /// <summary>
        /// Return the window boundary rectangle
        /// </summary>
        /// <param name="wHnd"></param>
        /// <returns></returns>
        internal NativeMethods.RECT GetBoundaryRect(IntPtr wHnd)
        {
            NativeMethods.RECT rect;
            if (_windowRectMapping.ContainsKey(wHnd))
            {
                if (_windowRectMapping.TryGetValue(wHnd, out rect))
                {
                    return rect;
                }
            }

            // go get it
            NativeMethods.GetWindowRect(wHnd, out rect);
            _windowRectMapping.Add(wHnd, rect);
            return rect;
        }

        /// <summary>
        /// Return the window monitor
        /// </summary>
        /// <param name="wHnd"></param>
        /// <returns></returns>
        internal IntPtr GetMonitor(IntPtr wHnd)
        {
            IntPtr monitor = IntPtr.Zero;
            if (_windowMonitorMapping.ContainsKey(wHnd))
            {
                if (_windowMonitorMapping.TryGetValue(wHnd, out monitor))
                {
                    return monitor;
                }
            }

            // go get it
            const int MONITOR_DEFAULTTONEAREST = 0x00000002;
            monitor = NativeMethods.MonitorFromWindow(wHnd, MONITOR_DEFAULTTONEAREST);
            _windowMonitorMapping.Add(wHnd, monitor);
            return monitor;
        }

        /// <summary>
        /// get the UI elements at point(x,y)
        /// </summary>
        /// <param name="elementCollection"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private List<IntPtr> GetElementHndsInRange(List<IntPtr> elementCollection, double x, double y)
        {
            List<IntPtr> elements = new List<IntPtr>();
            if (elementCollection != null && elementCollection.Count > 0)
            {
                foreach (IntPtr elementHnd in elementCollection)
                {
                    if (InRange((IntPtr)elementHnd, x, y))
                    {
                        elements.Add(elementHnd);
                    }
                }
            }
            return elements;
        }

        /// <summary>
        /// whether the UI element contains point(x,y)
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool InRange(IntPtr hWnd, double x, double y)
        {
            try
            {
                NativeMethods.RECT clientRect = GetBoundaryRect(hWnd);

                if (clientRect.width == 0 || clientRect.height == 0)
                {
                    return false;
                }

                double monitorScaling = 1.0d;
                var monitor = GetMonitor(hWnd);
                if (monitor != IntPtr.Zero)
                {
                    // Calculate monitor scaling relative to primary monitor
                    var monitorInfo = _screenProps.GetMonitorInformation(monitor);
                    var monitorScalingFactor = monitorInfo.scalingFactor;
                    var defaultScalingFactor = _screenProps.GetMonitorInformation(IntPtr.Zero).scalingFactor;
                    monitorScaling = monitorScalingFactor / defaultScalingFactor;
                }

                if ((clientRect.left * monitorScaling <= x && x <= clientRect.right * monitorScaling) &&
                    (clientRect.top * monitorScaling <= y && y < clientRect.bottom * monitorScaling))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Diagnostics.LogException(ex);
            }
            return false;
        }

        protected bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam)
        {
            try
            {
                var processId = Process.GetCurrentProcess().Id;
                uint wndProcessId;
                NativeMethods.GetWindowThreadProcessId(hWnd, out wndProcessId);
                if (wndProcessId != processId)
                {
                    if (IsVisibleWindow(hWnd))
                    {
                        _topElementHnds.Add(hWnd);

                        var zOrder = GetZOrder(hWnd);
                        if (!_windowZOrderMapping.ContainsKey(hWnd))
                        {
                            _windowZOrderMapping.Add(hWnd, zOrder);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Diagnostics.LogTrace("EnumWindowsProc Exception hWnd:" + hWnd);
                Diagnostics.LogException(ex);
                return false;
            }
        }

        /// <summary>
        /// Enumerates all top-level windows on the screen by passing the handle to each window,
        /// If EnumWindows meets some exception the callback function returns FALSE so that will stop enumwindow.
        /// </summary>
        private void GetTopLevelWindows()
        {
            try
            {
                NativeMethods.EnumWindows(new NativeMethods.EnumWindowsProc(EnumWindowsProc), IntPtr.Zero);
            }
            catch (Exception ex)
            {
                Diagnostics.LogException(ex);
            }
        }

        /// <summary>
        /// get z order of the UI element
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        private int GetZOrder(IntPtr hWnd)
        {
            var z = 0;
            for (IntPtr h = hWnd; h != IntPtr.Zero; h = NativeMethods.GetWindow(h, NativeMethods.GW_HWNDPREV)) z++;
            return z;
        }

        /// <summary>
        /// The window meets our conditions:
        /// 1. visible
        /// 2. transparent
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        private bool IsVisibleWindow(IntPtr hWnd)
        {
            if (NativeMethods.IsWindowVisible(hWnd))
            {
                // only add non transparent window
                uint style = NativeMethods.GetWindowLong(hWnd, NativeMethods.GWL_EXSTYLE);
                if ((style & NativeMethods.WS_EX_TRANSPARENT) != NativeMethods.WS_EX_TRANSPARENT)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
