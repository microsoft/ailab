// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace SnipInsight.Util
{
    internal class NativeMethods
    {

        #region User32
        internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll")]
        internal static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        internal static extern bool GetClientRect(IntPtr hWnd, out NativeMethods.RECT lpRect);

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);
        [DllImport("user32", EntryPoint = "GetMonitorInfo", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool GetMonitorInfoEx(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        [DllImport("user32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);
        [DllImport("user32")]
        internal static extern IntPtr MonitorFromPoint(POINT pt, int flags);

        [DllImport("user32.dll")]
        internal static extern int GetSystemMetrics(int nIndex);

        internal enum GetClipBoxReturn : int
        {
            Error = 0,
            NullRegion = 1,
            SimpleRegion = 2,
            ComplexRegion = 3
        }

        [Flags]
        internal enum WindowStyles : uint
        {
            // ReSharper disable InconsistentNaming
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
            WS_THICKFRAME = 0x40000,
            WS_TABSTOP = 0x10000,
            WS_VISIBLE = 0x10000000,
            WS_VSCROLL = 0x200000,
            // ReSharper restore InconsistentNaming
        }

        [Flags]
        internal enum WindowStylesEx : uint
        {
            // ReSharper disable InconsistentNaming
            WS_EX_TRANSPARENT = 0x00000020,
            WS_EX_TOOLWINDOW = 0x00000080,
            // ReSharper restore InconsistentNaming
        }

        internal enum ShowWindowCommands : int
        {
            // ReSharper disable InconsistentNaming
            SW_FORCEMINIMIZE = 11,
            SW_HIDE = 0,
            SW_MAXIMIZE = 3,
            SW_MINIMIZE = 6,
            SW_RESTORE = 9,
            SW_SHOW = 5,
            SW_SHOWDEFAULT = 10,
            SW_SHOWMAXIMIZED = 3,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOWNORMAL = 1,
            // ReSharper restore InconsistentNaming
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public int dwFlags = 0;
        }

        internal const int MONITORINFOF_PRIMARY = 1;
        internal const int CCHDEVICENAME = 32;
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct MONITORINFOEX
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public int dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string deviceName;
            static public MONITORINFOEX New()
            {
                return new MONITORINFOEX
                {
                    cbSize = Marshal.SizeOf(typeof(MONITORINFOEX)),
                    deviceName = string.Empty,
                };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            // ReSharper disable InconsistentNaming
            public int x;
            public int y;
            // ReSharper restore InconsistentNaming

            /// <summary>
            /// Construct a point of coordinates (x,y).
            /// </summary>
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public override string ToString()
            {
                return "(" + x.ToString() + "," + y.ToString() + ")";
        }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        internal struct RECT
        {
            // ReSharper disable InconsistentNaming
            internal int left;
            internal int top;
            internal int right;
            internal int bottom;

            internal int width
            {
                get { return right - left; }
            }

            internal int height
            {
                get { return bottom - top; }
            }
            // ReSharper restore InconsistentNaming


            /// <summary> Win32 </summary>
            public static readonly RECT Empty = new RECT();

            /// <summary> Win32 </summary>
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }


            /// <summary> Win32 </summary>
            public RECT(RECT rcSrc)
            {
                this.left = rcSrc.left;
                this.top = rcSrc.top;
                this.right = rcSrc.right;
                this.bottom = rcSrc.bottom;
            }

            /// <summary> Win32 </summary>
            public bool IsEmpty
            {
                get
                {
                    // BUGBUG : On Bidi OS (hebrew arabic) left > right
                    return left >= right || top >= bottom;
                }
            }
            /// <summary> Return a user friendly representation of this struct </summary>
            public override string ToString()
            {
                if (this == RECT.Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }

            /// <summary> Determine if 2 RECT are equal (deep compare) </summary>
            public override bool Equals(object obj)
            {
                if (!(obj is Rect)) { return false; }
                return (this == (RECT)obj);
            }

            /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
            public override int GetHashCode()
            {
                return left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            }


            /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
            public static bool operator ==(RECT rect1, RECT rect2)
            {
                return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom);
            }

            /// <summary> Determine if 2 RECT are different(deep compare)</summary>
            public static bool operator !=(RECT rect1, RECT rect2)
            {
                return !(rect1 == rect2);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPLACEMENT
        {
            // ReSharper disable InconsistentNaming
            internal int length;
            internal int flags;
            internal ShowWindowCommands showCmd;
            internal POINT ptMinPosition;
            internal POINT ptMaxPosition;
            internal RECT rcNormalPosition;
            internal static WINDOWPLACEMENT Empty
            {
                get
                {
                    WINDOWPLACEMENT result = new WINDOWPLACEMENT();
                    result.length = Marshal.SizeOf(result);
                    return result;
                }
            }
            // ReSharper restore InconsistentNaming
        }

        internal enum WindowMsg : uint
        {
            // ReSharper disable InconsistentNaming
            WM_SIZE = 0x0005,
            WM_CLOSE = 0x0010,
            WM_MOUSEACTIVATE = 0x0021,
            WM_MOUSEMOVE = 0x0200,
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_LBUTTONDBLCLK = 0x0203,
            WM_DPICHANGED = 0x02E0,

            WM_NCCREATE = 0x0081,
            WM_NCDESTROY = 0x0082,
            WM_NCCALCSIZE = 0x0083,
            WM_NCHITTEST = 0x0084,
            WM_NCPAINT = 0x0085,
            WM_NCACTIVATE = 0x0086,

            WM_HOTKEY = 0x0312,
            // ReSharper restore InconsistentNaming
        }

        [Flags]
        internal enum SetWindowPosFlags : uint
        {
            SWP_NOSIZE          = 0x0001,
            SWP_NOMOVE          = 0x0002,
            SWP_NOZORDER        = 0x0004,
            SWP_NOREDRAW        = 0x0008,
            SWP_NOACTIVATE      = 0x0010,
            SWP_FRAMECHANGED    = 0x0020,  /* The frame changed: send WM_NCCALCSIZE */
            SWP_SHOWWINDOW      = 0x0040,
            SWP_HIDEWINDOW      = 0x0080,
            SWP_NOCOPYBITS      = 0x0100,
            SWP_NOOWNERZORDER   = 0x0200,  /* Don't do owner Z ordering */
            SWP_NOSENDCHANGING  = 0x0400,  /* Don't send WM_WINDOWPOSCHANGING */

            SWP_DRAWFRAME       = SWP_FRAMECHANGED,
            SWP_NOREPOSITION    = SWP_NOOWNERZORDER,
        }

        internal enum HResults : int
        {
            // ReSharper disable InconsistentNaming
            S_OK = 0,
            S_FALSE = 1,
            S_ENDOFSTREAM = 513183767,
            E_NOTIMPL = -2147467263,
            E_OUTOFMEMORY = -2147024882,
            E_INVALIDARG = -2147024809,
            E_NOINTERFACE = -2147467262,
            E_POINTER = -2147467261,
            E_HANDLE = -2147024890,
            E_ABORT = -2147467260,
            E_FAIL = -2147467259,
            E_ACCESSDENIED = -2147024891,
            E_NOSUITABLEAUDIOSTREAM = -1634299904,
            E_NOSUITABLEVIDEOSTREAM = -1634299903,
            E_NOTRANSCODEAUDIOTYPE = -1634299902,
            MF_E_SINK_NO_SAMPLES_PROCESSED = -1072870844,
            E_NOAUDIORECEIVED = -1634299880,
            // ReSharper restore InconsistentNaming
        }

        internal enum GaFlags
        {
            /// <summary>
            /// Retrieves the parent window. This does not include the owner, as it does with the GetParent function.
            /// </summary>
            GA_PARENT = 1,
            /// <summary>
            /// Retrieves the root window by walking the chain of parent windows.
            /// </summary>
            GA_ROOT = 2,
            /// <summary>
            /// Retrieves the owned root window by walking the chain of parent and owner windows returned by GetParent.
            /// </summary>
            GA_ROOTOWNER = 3
        }

        internal enum GwlIndex
        {
            // ReSharper disable InconsistentNaming

            /// <summary>
            /// Gets/sets the window styles.
            /// </summary>
            GWL_STYLE = -16,

            /// <summary>
            /// Gets/sets the extended window styles.
            /// </summary>
            GWL_EXSTYLE = -20,

            // ReSharper restore InconsistentNaming
        }

        internal enum MouseActivate
        {
            // ReSharper disable InconsistentNaming
            MA_ACTIVATE = 1,
            MA_ACTIVATEANDEAT = 2,
            MA_NOACTIVATE = 3,
            MA_NOACTIVATEANDEAT = 4,
            // ReSharper restore InconsistentNaming
        }

        internal enum HookType : int
        {
            WH_JOURNALRECORD    = 0,
            WH_JOURNALPLAYBACK  = 1,
            WH_KEYBOARD         = 2,
            WH_GETMESSAGE       = 3,
            WH_CALLWNDPROC      = 4,
            WH_CBT              = 5,
            WH_SYSMSGFILTER     = 6,
            WH_MOUSE            = 7,
            WH_HARDWARE         = 8,
            WH_DEBUG            = 9,
            WH_SHELL            = 10,
            WH_FOREGROUNDIDLE   = 11,
            WH_CALLWNDPROCRET   = 12,
            WH_KEYBOARD_LL      = 13,
            WH_MOUSE_LL         = 14
        }

        internal const int WM_KEYDOWN = 0x0100;
        internal const int WM_KEYUP=    0x0101;

        [Flags]
        public enum HotKeyModifiers : uint
        {
            MOD_ALT = 0x0001,       // Either ALT key must be held down
            MOD_CONTROL = 0x0002,   // Either CTRL key must be held down.
            MOD_NOREPEAT = 0x4000,  // Changes the hotkey behavior so that the keyboard auto-repeat does not yield multiple hotkey notifications.
            MOD_SHIFT = 0x0004,     // Either SHIFT key must be held down.
            MOD_WIN = 0x0008,       // Either WINDOWS key was held down. These keys are labeled with the Windows logo. Keyboard shortcuts that involve the WINDOWS key are reserved for use by the operating system.
        }

        internal enum SystemMetrixIndex : int
        {
            // ReSharper disable InconsistentNaming
            SM_CXSIZEFRAME = 32,
            SM_CYSIZEFRAME = 33,
            SM_CXPADDEDBORDER = 92,
            // ReSharper restore InconsistentNaming
        }

        [DllImport("user32.dll")]
        internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        internal static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetAncestor(IntPtr hwnd, GaFlags flags);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern Int32 GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        // supress CA1400 for 64-bit systems
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist"), DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        /// <summary>
        /// Handles 32/64 bit differences
        /// </summary>
        public static IntPtr GetWindowLongPtr(IntPtr hWnd, GwlIndex nIndex)
        {
            if (Is32BitProcess())
            {
                return (IntPtr)GetWindowLongPtr32(hWnd, (int)nIndex);
            }

            return GetWindowLongPtr64(hWnd, (int)nIndex);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LASTINPUTINFO
        {
            internal uint cbSize;
            internal uint dwTime;
        }

        [DllImport("User32.dll")]
        internal static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DestroyIcon([In] IntPtr hIcon);

        #endregion

        #region Shcore

        [DllImport("shcore")]
        internal static extern int GetDpiForMonitor(IntPtr hMonitor, Monitor_DPI_Type dpiType, ref uint dpiX, ref uint dpiY);

        [DllImport("shcore")]
        internal static extern int SetProcessDpiAwareness(int value);

        [DllImport("shcore")]
        internal static extern int GetProcessDpiAwareness(IntPtr handle, ref int value);

        internal enum Monitor_DPI_Type : int
        {
            MDT_Effective_DPI = 0,
            MDT_Angular_DPI = 1,
            MDT_Raw_DPI = 2,
            MDT_Default = MDT_Effective_DPI
        };

        #endregion

        #region Kernel32

        [DllImport("kernel32.dll")]
        internal static extern uint GetCurrentThreadId();

        [DllImport("kernel32.dll")]
        internal static extern ulong GetTickCount64();

        [DllImport("kernel32.dll")]
        internal static extern bool CloseHandle(IntPtr handle);

        #endregion

        #region Advapi32

        /// <summary>
        /// Passed to <see cref="GetTokenInformation"/> to specify what information about the token to return.
        /// </summary>
        internal enum TokenInformationClass
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin,
            TokenElevationType,
            TokenLinkedToken,
            TokenElevation,
            TokenHasRestrictions,
            TokenAccessInformation,
            TokenVirtualizationAllowed,
            TokenVirtualizationEnabled,
            TokenIntegrityLevel,
            TokenUiAccess,
            TokenMandatoryPolicy,
            TokenLogonSid,
            MaxTokenInfoClass
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TokenElevation
        {
            internal uint TokenIsElevated;
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool GetTokenInformation(IntPtr tokenHandle, TokenInformationClass tokenInformationClass, IntPtr tokenInformation, uint tokenInformationLength, out uint returnLength);


        internal static uint STANDARD_RIGHTS_READ = 0x00020000;
        internal static uint TOKEN_QUERY = 0x0008;
        internal static uint TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr processHandle, UInt32 desiredAccess, out IntPtr tokenHandle);


        #endregion

        #region Utility functions

        internal static void ThrowLastError()
        {
            Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
        }

        #endregion

        #region ImageCapture
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr MonitorFromPoint(System.Drawing.Point pt, MonitorOptions dwFlags);

        public enum MonitorOptions : uint
        {
            MONITOR_DEFAULTTONULL = 0x00000000,
            MONITOR_DEFAULTTOPRIMARY = 0x00000001,
            MONITOR_DEFAULTTONEAREST = 0x00000002
        }

        public delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip,
           MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);

        public const int GWL_EXSTYLE = -20;

        public const int WS_EX_TRANSPARENT = 0x20;

        [DllImport("user32.dll")]
        internal extern static IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        internal static extern IntPtr GetShellWindow();

        [DllImport("user32.dll")]
        internal static extern IntPtr GetWindowDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetWindow(IntPtr hWnd, int uCmd);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        internal enum SetWindowPosInsertAfter : int
        {
            HWND_BOTTOM = 1,
            HWND_TOP = 0,
            HWND_TOPMOST = -1,
            HWND_NOTOPMOST = -2,
        }

        [DllImport("user32")]
        internal static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int x,
            int y,
            int cx,
            int cy,
            uint uFlags);

        public const int GW_HWNDNEXT = 2;
        public const int GW_HWNDPREV = 3;

        [DllImport("user32.dll")]
        internal static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [StructLayout(LayoutKind.Sequential)]
        internal struct IconInfo
        {
            public bool fIcon;         // Specifies whether this structure defines an icon or a cursor. A value of TRUE specifies
            // an icon; FALSE specifies a cursor.
            public Int32 xHotspot;     // Specifies the x-coordinate of a cursor's hot spot. If this structure defines an icon, the hot
            // spot is always in the center of the icon, and this member is ignored.
            public Int32 yHotspot;     // Specifies the y-coordinate of the cursor's hot spot. If this structure defines an icon, the hot
            // spot is always in the center of the icon, and this member is ignored.
            public IntPtr hbmMask;     // (HBITMAP) Specifies the icon bitmask bitmap. If this structure defines a black and white icon,
            // this bitmask is formatted so that the upper half is the icon AND bitmask and the lower half is
            // the icon XOR bitmask. Under this condition, the height should be an even multiple of two. If
            // this structure defines a color icon, this mask only defines the AND bitmask of the icon.
            public IntPtr hbmColor;    // (HBITMAP) Handle to the icon color bitmap. This member can be optional if this
            // structure defines a black and white icon. The AND bitmask of hbmMask is applied with the SRCAND
            // flag to the destination; subsequently, the color bitmap is applied (using XOR) to the
            // destination by using the SRCINVERT flag.
        }

        #endregion

        #region AuthorEx
        [DllImport("authorex.dll")]
        internal static extern int CreateDeviceDiscoveryClass(out AuthorEx.IDeviceEnumerator enumerator, AuthorEx.IDeviceEnumeratorCallback callback);

        [DllImport("authorex.dll")]
        internal static extern int CreateCaptureControlClass(out AuthorEx.ICaptureControl control, AuthorEx.ICaptureCallback callback);

        [DllImport("authorex.dll")]
        internal static extern int CreateMP4MediaTransferClass(out AuthorEx.IMP4MediaTransfer transfer);

        [DllImport("authorex.dll")]
        internal static extern int VerifyEmbeddedSignature([MarshalAs(UnmanagedType.BStr)] string filename, bool mayBeOffline);

        [DllImport("authorex.dll")]
        internal static extern int CheckD3D(uint d3dVersion);

        internal static bool Is32BitProcess()
        {
            return IntPtr.Size == 4 /*bytes*/;
        }

        #endregion

        #region gdi32
        [DllImport("gdi32.dll")]
        internal static extern int GetClipBox(IntPtr hdc, out NativeMethods.RECT lprc);

        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc,
            int nWidth, int nHeight);

        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", EntryPoint = "SelectObject")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);

        [DllImport("gdi32")]
        internal static extern bool DeleteObject(IntPtr o);

        [DllImport("gdi32.dll", EntryPoint = "CreateDC", CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateDC(string driver, string device, string output, IntPtr initData);

        [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
        internal static extern bool DeleteDC(IntPtr hDc);

        internal enum DeviceCap
        {
            /// <summary>
            /// Device driver version
            /// </summary>
            DRIVERVERSION = 0,
            /// <summary>
            /// Device classification
            /// </summary>
            TECHNOLOGY = 2,
            /// <summary>
            /// Horizontal size in millimeters
            /// </summary>
            HORZSIZE = 4,
            /// <summary>
            /// Vertical size in millimeters
            /// </summary>
            VERTSIZE = 6,
            /// <summary>
            /// Horizontal width in pixels
            /// </summary>
            HORZRES = 8,
            /// <summary>
            /// Vertical height in pixels
            /// </summary>
            VERTRES = 10,
            /// <summary>
            /// Number of bits per pixel
            /// </summary>
            BITSPIXEL = 12,
            /// <summary>
            /// Number of planes
            /// </summary>
            PLANES = 14,
            /// <summary>
            /// Number of brushes the device has
            /// </summary>
            NUMBRUSHES = 16,
            /// <summary>
            /// Number of pens the device has
            /// </summary>
            NUMPENS = 18,
            /// <summary>
            /// Number of markers the device has
            /// </summary>
            NUMMARKERS = 20,
            /// <summary>
            /// Number of fonts the device has
            /// </summary>
            NUMFONTS = 22,
            /// <summary>
            /// Number of colors the device supports
            /// </summary>
            NUMCOLORS = 24,
            /// <summary>
            /// Size required for device descriptor
            /// </summary>
            PDEVICESIZE = 26,
            /// <summary>
            /// Curve capabilities
            /// </summary>
            CURVECAPS = 28,
            /// <summary>
            /// Line capabilities
            /// </summary>
            LINECAPS = 30,
            /// <summary>
            /// Polygonal capabilities
            /// </summary>
            POLYGONALCAPS = 32,
            /// <summary>
            /// Text capabilities
            /// </summary>
            TEXTCAPS = 34,
            /// <summary>
            /// Clipping capabilities
            /// </summary>
            CLIPCAPS = 36,
            /// <summary>
            /// Bitblt capabilities
            /// </summary>
            RASTERCAPS = 38,
            /// <summary>
            /// Length of the X leg
            /// </summary>
            ASPECTX = 40,
            /// <summary>
            /// Length of the Y leg
            /// </summary>
            ASPECTY = 42,
            /// <summary>
            /// Length of the hypotenuse
            /// </summary>
            ASPECTXY = 44,
            /// <summary>
            /// Shading and Blending caps
            /// </summary>
            SHADEBLENDCAPS = 45,

            /// <summary>
            /// Logical pixels inch in X
            /// </summary>
            LOGPIXELSX = 88,
            /// <summary>
            /// Logical pixels inch in Y
            /// </summary>
            LOGPIXELSY = 90,

            /// <summary>
            /// Number of entries in physical palette
            /// </summary>
            SIZEPALETTE = 104,
            /// <summary>
            /// Number of reserved entries in palette
            /// </summary>
            NUMRESERVED = 106,
            /// <summary>
            /// Actual color resolution
            /// </summary>
            COLORRES = 108,

            // Printing related DeviceCaps. These replace the appropriate Escapes
            /// <summary>
            /// Physical Width in device units
            /// </summary>
            PHYSICALWIDTH = 110,
            /// <summary>
            /// Physical Height in device units
            /// </summary>
            PHYSICALHEIGHT = 111,
            /// <summary>
            /// Physical Printable Area x margin
            /// </summary>
            PHYSICALOFFSETX = 112,
            /// <summary>
            /// Physical Printable Area y margin
            /// </summary>
            PHYSICALOFFSETY = 113,
            /// <summary>
            /// Scaling factor x
            /// </summary>
            SCALINGFACTORX = 114,
            /// <summary>
            /// Scaling factor y
            /// </summary>
            SCALINGFACTORY = 115,

            /// <summary>
            /// Current vertical refresh rate of the display device (for displays only) in Hz
            /// </summary>
            VREFRESH = 116,
            /// <summary>
            /// Vertical height of entire desktop in pixels
            /// </summary>
            DESKTOPVERTRES = 117,
            /// <summary>
            /// Horizontal width of entire desktop in pixels
            /// </summary>
            DESKTOPHORZRES = 118,
            /// <summary>
            /// Preferred blt alignment
            /// </summary>
            BLTALIGNMENT = 119
        }

        [DllImport("gdi32.dll")]
        internal static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        internal enum TernaryRasterOperations : uint
        {
            SRCCOPY = 0x00CC0020,
            SRCPAINT = 0x00EE0086,
            SRCAND = 0x008800C6,
            SRCINVERT = 0x00660046,
            SRCERASE = 0x00440328,
            NOTSRCCOPY = 0x00330008,
            NOTSRCERASE = 0x001100A6,
            MERGECOPY = 0x00C000CA,
            MERGEPAINT = 0x00BB0226,
            PATCOPY = 0x00F00021,
            PATPAINT = 0x00FB0A09,
            PATINVERT = 0x005A0049,
            DSTINVERT = 0x00550009,
            BLACKNESS = 0x00000042,
            WHITENESS = 0x00FF0062,
            CAPTUREBLT = 0x40000000 //only if WinVer >= 5.0.0 (see wingdi.h)
        }

        [DllImport("gdi32.dll")]
        internal static extern bool BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, TernaryRasterOperations dwRop);

        #endregion

        #region Helpers

        public static IntPtr GetWindowHwnd(Window window)
        {
            WindowInteropHelper windowHwnd = new WindowInteropHelper(window);
            return windowHwnd.Handle;
        }

        public static IntPtr GetMonitorFromWindow(IntPtr hwnd)
        {
            const int MONITOR_DEFAULTTONEAREST = 0x00000002;

            return NativeMethods.MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
        }

        #endregion
    }

    internal class AuthorEx
    {
        internal enum CaptureDeviceType
        {
            InvalidCaptureDevice = 0x00000000,
            VideoCaptureDevice = 0x00000001,
            AudioCaptureDevice = 0x00000002,
            AudioRenderDevice = 0x00000004,
            AnyCaptureDevice = VideoCaptureDevice | AudioCaptureDevice,
            AnyDevice = AnyCaptureDevice | AudioRenderDevice
        }

        internal enum VideoFormat
        {
            UnknownVideoFormat = -1,
            VideoFormatYuy2 = 0,
            VideoFormatNv12 = 1,
            VideoFormatRgb24 = 2,
            VideoFormatRgb32 = 3,
        };

        [StructLayout(LayoutKind.Sequential)]
        internal struct StreamRecordingStats
        {
            // Encoder stats
            internal long llLastReceived;
            internal long llLastEncoded;
            internal long llLastProcessed;
            internal ulong cReceived;
            internal ulong cEncoded;
            internal ulong cProcessed;
            internal ulong cbProcessed;
            internal uint cDiscontinuities;
            // Pipeline stats
            internal ulong cCaptured;
            internal ulong cRecorded;
        };

        [Guid("BA1164FC-5183-45AF-AC16-05413E12972C"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IDeviceCollection
        {
            [PreserveSig]
            uint GetVersion();
            [PreserveSig]
            uint GetCount();
            [PreserveSig]
            int GetId(uint index, [MarshalAs(UnmanagedType.BStr)] out string id);
            [PreserveSig]
            int GetFriendlyName(uint index, [MarshalAs(UnmanagedType.BStr)] out string name);
            [PreserveSig]
            int GetScore(uint index, ref float score);
        };

        [Guid("14508A70-EAA4-429D-B7E9-1A20BA55544F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IDeviceEnumerator
        {
            [PreserveSig]
            int Enumerate(CaptureDeviceType type);
            [PreserveSig]
            int GetDevices(CaptureDeviceType type, out IDeviceCollection devices);
        }

        [Guid("6E1BDC8B-4569-4E02-B870-BA7FAFA5DCC7"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IDeviceEnumeratorCallback
        {
            void OnDeviceChange();
        }

        [Guid("CE1ABBC9-79D3-4CB1-B86E-C7C8FE26CF55"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface ICaptureControl
        {
            [PreserveSig]
            int StartCapture(
                [MarshalAs(UnmanagedType.BStr)] string audioDeviceId,
                bool voiceOptimized,
                [MarshalAs(UnmanagedType.BStr)] string videoDeviceId,
                int pixelCount,
                bool preferHighElseLow,
                float aspectRatio,
                float frameRate
                );
            [PreserveSig]
            int StartRecording([MarshalAs(UnmanagedType.BStr)] string filename);
            [PreserveSig]
            int StartTranscodingFromUrl(
                [MarshalAs(UnmanagedType.BStr)] string source,
                [MarshalAs(UnmanagedType.BStr)] string filename,
                bool removeVideo);
            [PreserveSig]
            int PauseRecording();
            [PreserveSig]
            int ResumeRecording();
            [PreserveSig]
            int StopRecording();
            [PreserveSig]
            int StopCapture();
            [PreserveSig]
            int GetRecordingStats(
                out StreamRecordingStats audioStats,
                out StreamRecordingStats videoStats
                );
        };

        [Guid("37FDFE04-DF59-4555-ABF0-616764D5835C"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface ICaptureVideoControl
        {
            [PreserveSig]
            int SetPreviewLocation(int x, int y);
            [PreserveSig]
            int GetPreviewBackBufferNoRef(out IntPtr pSurface);
            [PreserveSig]
            int RenderPreview();
        };

        [Guid("795B1614-71DD-464B-9B2C-809C978542E1"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface ICaptureAudioControl
        {
            [PreserveSig]
            int GetPeak(ref float peak);
            [PreserveSig]
            int GetVolume(ref bool mute, ref float level);
            [PreserveSig]
            int SetVolume(bool mute, float level);
        };

        [Guid("87524C71-CD9B-4327-96D2-302751A11F91"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface ICaptureCallback
        {
            void OnInitialized
                (
                    uint uVideoFrameWidth,
                    uint uVideoFrameHeight,
                    int iStride,
                    VideoFormat format,
                    [MarshalAs(UnmanagedType.BStr)] string formatsInfo,
                    int voiceDspResult,
                    bool blankVideo
                );
            void OnError(int hr);
            void OnSampleTime(Int64 llRecordedFrameTime);

        }

        [Guid("E1D3A706-F753-44EC-BE3A-2559D969AE2F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IErrorInjection
        {
            [PreserveSig]
            int ErrorCondition(int error);
        }

        [Guid("B9260913-0335-4E66-8C70-0568293EAD50"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMP4MediaTransfer
        {
            [PreserveSig]
            int SetOutputFile([MarshalAs(UnmanagedType.BStr)] string outputFile);
            [PreserveSig]
            int SetSampleTimeOffset(long sampleTimeOffset);
            [PreserveSig]
            int LoadSourceMediaFile([MarshalAs(UnmanagedType.BStr)] string inputFile);
            [PreserveSig]
            int ReadSample(out long sampleTime, out long duration, out bool isVideoSample, out bool isVideoStreamEnded, out bool isAudioStreamEnded);
            [PreserveSig]
            int ProcessSample(IntPtr pImageBytes, int length);
            [PreserveSig]
            int EndWriting();
            [PreserveSig]
            uint GetVideoWidth();
            [PreserveSig]
            uint GetVideoHeight();
            [PreserveSig]
            int ConfigureOutputMedia(uint videoWidth, uint videoHeight, uint frameRateHeight, uint frameRateLow);
            [PreserveSig]
            int AppendFrame(IntPtr pImageBytes, int length, long timeStamp, long duration, out bool isAudioStreamEnded);
        };
    }

    internal class AuthorScreenCap
    {
        internal enum ScreenCaptureRecordingErrorType
        {
            NoError,
            InputAudioDeviceDisconnected, // microphone disconnected
            PlaybackAudioDeviceDisconnected, // speaker disconnected,
            AudioServiceNotRunning, // Audio service not running
            GenericAudioDeviceError, // Any errors with audio device
            GenericVideoDeviceError, // Any errors with video device
            EncodingError, //media encoding error
            UnknownError,

            LastError // Please add any new errors before this
        };

        // Add ScreenCaptureRecordingErrorHResultBase + ScreenCaptureRecordingErrorType to compute the ScreenCapture recording error HRESULT
        internal const int ScreenCaptureRecordingErrorHResultBase = -1634271232; // = (int)0x9e970000 = MAKE_HRESULT(0x01, 0x1e97, 0)

        [ComImport, Guid("EFEBD47E-056E-401D-AE28-803E7755DFD0"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IScreenCaptureCallback
        {
            void OnLog([In, MarshalAs(UnmanagedType.BStr)] string logString);
            void OnSampleTime(Int64 llRecordedFrameTime);
            void OnComplete(int hr);
            void OnRecordingError(ScreenCaptureRecordingErrorType recordingErrorType);
        }

        [ComImport, Guid("58037FDA-69A8-47DD-84B0-95EC3AF7B7D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IScreenCaptureControl
        {
            [PreserveSig]
            int Initialize
                (
                    [In]IScreenCaptureCallback callback,
                    [In, MarshalAs(UnmanagedType.BStr)]string filename,
                    int left, int right,
                    int top, int bottom,
                    bool showCursor,
                    [In, MarshalAs(UnmanagedType.BStr)]string audioInputDevice
                );
            [PreserveSig]
            int Start();
            [PreserveSig]
            int Stop();
        }

        [ComImport, Guid("99205731-EE27-4ADA-A614-4A113AF8DB09")]
        internal class ScreenCaptureClass
        {
        }

    }
}
