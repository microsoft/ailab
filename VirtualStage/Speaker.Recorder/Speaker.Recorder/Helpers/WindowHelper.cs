using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace Speaker.Recorder.Helpers
{
    static class WindowHelper
    {
        enum GetAncestorFlags
        {
            // Retrieves the parent window. This does not include the owner, as it does with the GetParent function.
            GetParent = 1,
            // Retrieves the root window by walking the chain of parent windows.
            GetRoot = 2,
            // Retrieves the owned root window by walking the chain of parent and owner windows returned by GetParent.
            GetRootOwner = 3
        }

        public enum WindowMessages : uint
        {
            WM_KEYUP = 0x0101,
            WM_KEYDOWN = 0x0100,
            WM_SETFOCUS = 0x0007,
            WM_MBUTTONDBLCLK = 0x0209,
        }

        public enum GetWindowLongs
        {
            GWL_WNDPROC = (-4),
            GWL_HINSTANCE = (-6),
            GWL_HWNDPARENT = (-8),
            GWL_STYLE = (-16),
            GWL_EXSTYLE = (-20),
            GWL_USERDATA = (-21),
            GWL_ID = (-12)
        }

        enum SetWindowPositions : uint
        {
            SWP_ASYNCWINDOWPOS = 0x4000,
        }

        [Flags]
        public enum WindowStyles : uint
        {
            WS_BORDER = 0x800000,
            WS_CAPTION = 0xc00000,
            WS_CHILD = 0x40000000,
            WS_CLIPCHILDREN = 0x2000000,
            WS_CLIPSIBLINGS = 0x4000000,
            WS_DISABLED = 0x8000000,
            WS_DLGFRAME = 0x400000,
            WS_GROUP = 0x20000,
            WS_HSCROLL = 0x100000,
            WS_MAXIMIZE = 0x1000000,
            WS_MAXIMIZEBOX = 0x10000,
            WS_MINIMIZE = 0x20000000,
            WS_MINIMIZEBOX = 0x20000,
            WS_OVERLAPPED = 0x0,
            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_SIZEFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            WS_POPUP = 0x80000000u,
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
            WS_SIZEFRAME = 0x40000,
            WS_SYSMENU = 0x80000,
            WS_TABSTOP = 0x10000,
            WS_VISIBLE = 0x10000000,
            WS_VSCROLL = 0x200000
        }

        enum DWMWINDOWATTRIBUTE : uint
        {
            NCRenderingEnabled = 1,
            NCRenderingPolicy,
            TransitionsForceDisabled,
            AllowNCPaint,
            CaptionButtonBounds,
            NonClientRtlLayout,
            ForceIconicRepresentation,
            Flip3DPolicy,
            ExtendedFrameBounds,
            HasIconicBitmap,
            DisallowPeek,
            ExcludedFromPeek,
            Cloak,
            Cloaked,
            FreezeRepresentation
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        public enum WS_EX
        {
            None = 0,
            WS_EX_LAYERED = 0x80000,
            WS_EX_TRANSPARENT = 0x00000020,
        }

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPositions uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, WindowMessages Msg, IntPtr wParam, UIntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr PostMessage(IntPtr hWnd, WindowMessages Msg, IntPtr wParam, UIntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern IntPtr GetShellWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", ExactSpelling = true)]
        static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags flags);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        // This static method is required because Win32 does not support
        // GetWindowLongPtr directly.
        // http://pinvoke.net/default.aspx/user32/GetWindowLong.html
        static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            else
                return GetWindowLongPtr32(hWnd, nIndex);
        }

        [DllImport("dwmapi.dll")]
        static extern int DwmGetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, out bool pvAttribute, int cbAttribute);

        public static bool IsWindowValidForCapture(IntPtr hwnd)
        {
            if (hwnd.ToInt32() == 0)
            {
                return false;
            }

            if (hwnd == GetShellWindow())
            {
                return false;
            }

            if (!IsWindowVisible(hwnd))
            {
                return false;
            }

            if (GetAncestor(hwnd, GetAncestorFlags.GetRoot) != hwnd)
            {
                return false;
            }

            var style = (WindowStyles)(ulong)GetWindowLongPtr(hwnd, (int)GetWindowLongs.GWL_STYLE);
            if (style.HasFlag(WindowStyles.WS_DISABLED))
            {
                return false;
            }

            var hrTemp = DwmGetWindowAttribute(hwnd, DWMWINDOWATTRIBUTE.Cloaked, out var cloaked, Marshal.SizeOf<bool>());
            if (hrTemp == 0 && cloaked)
            {
                return false;
            }

            return true;
        }

        public static Size GetWindowSize(IntPtr hwnd)
        {
            GetWindowRect(hwnd, out RECT rc);
            var width = rc.Right - rc.Left;
            var height = rc.Bottom - rc.Top;
            return new Size(width, height);
        }

        internal static void SetWindowPosition(IntPtr hwnd, Rect rectangle)
        {
            SetWindowPos(hwnd, IntPtr.Zero, (int)rectangle.X - 1, (int)rectangle.Y - 1, (int)rectangle.Width, (int)rectangle.Height, SetWindowPositions.SWP_ASYNCWINDOWPOS);
        }

        internal static void SendKey(IntPtr hwnd, IntPtr key)
        {
            PostMessage(hwnd, WindowMessages.WM_SETFOCUS, IntPtr.Zero, UIntPtr.Zero);
            PostMessage(hwnd, WindowMessages.WM_KEYDOWN, key, UIntPtr.Zero);
            PostMessage(hwnd, WindowMessages.WM_KEYUP, key, UIntPtr.Zero);
            PostMessage(hwnd, WindowMessages.WM_MBUTTONDBLCLK, IntPtr.Zero, UIntPtr.Zero);
        }
    }
}
