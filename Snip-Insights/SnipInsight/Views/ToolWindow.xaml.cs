// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using SnipInsight.Properties;
using SnipInsight.Util;
using SnipInsight.Controls;
using SnipInsight.Controls.Ariadne;
using DpiScale = SnipInsight.Util.DpiScale;

namespace SnipInsight.Views
{
    internal delegate void HotKeyPressedEventHandler(object sender, HotKeyPressedEventArgs e);

    public partial class ToolWindow : DpiAwareWindow
    {
        enum DockState
        {
            Top,
            Left,
            Right,
            Bottom,
            Middle,
        }
        DockState _currentDockState = DockState.Top;

        private Storyboard _activeStoryboard;
        private bool _isAnimating; // Forbid dragging the tool window while it is growing/shrinking
        private bool _isRepositioning; // Suppress processing of LocationChanged events when changing window position programmatically
        private HwndSource _source;
        private IntPtr _handle;
        private bool _dragging;
        private Point _startPoint;
        internal event HotKeyPressedEventHandler HotKeyPressed;
        private Dictionary<SnipHotKey, KeyCombo> _monitoringHotKeys = new Dictionary<SnipHotKey, KeyCombo>();

        public bool ToolWindowClosedBySystem { get; set; }

        public ToolWindow()
        {
            InitializeComponent();
            CopyClipboardToggle.IsChecked = UserSettings.CopyToClipboardAfterSnip;
            AIEnableToggle.IsChecked = UserSettings.IsAIEnabled;
            OpenEditorToggle.IsChecked = UserSettings.IsOpenEditorPostSnip;

            DataContext = AppManager.TheBoss.ViewModel;
        }

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = false;

            switch (msg)
            {
                case (int)SnipInsight.Util.NativeMethods.WindowMsg.WM_HOTKEY:
                    if (HotKeyPressed != null)
                    {
                        HotKeyPressed(this, new HotKeyPressedEventArgs { KeyPressed = (SnipHotKey)(wParam.ToInt32()) });
                        handled = true;
                    }
                    break;
            }

            return (IntPtr)0;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            try
            {
                _handle = new WindowInteropHelper(this).Handle;
                _source = HwndSource.FromHwnd(_handle);
                _source.AddHook(WindowProc);
            }
            catch (Exception ex)
            {
                Diagnostics.LogException(ex);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_source != null)
            {
                _source.RemoveHook(WindowProc);
                _source = null;
            }

            foreach (SnipHotKey key in _monitoringHotKeys.Keys.ToList())
            {
                UnregisterHotKey(key);
            }

            if (!ToolWindowClosedBySystem)
            {
                AppManager.TheBoss.ViewModel.StateMachine.Fire(StateMachine.SnipInsightTrigger.Exit);
            }
            base.OnClosed(e);
        }

        #region HotKeys
        internal void RegisterHotKey(SnipHotKey key, KeyCombo keyCombo)
        {
            try
            {
                // Register Key.None means remove it
                if (keyCombo == null || keyCombo.Key == Key.None)
                {
                    UnregisterHotKey(key);
                    return;
                }

                if (_monitoringHotKeys.ContainsKey(key))
                {
                    UnregisterHotKey(key);
                }
                NativeMethods.RegisterHotKey(_handle, (int)key, keyCombo.KeyModifier, keyCombo.VirtualKeyCode);
                _monitoringHotKeys.Add(key, keyCombo);
            }
            catch (Exception ex)
            {
                Diagnostics.LogException(ex);
            }
        }

        internal void UnregisterHotKey(SnipHotKey key)
        {
            try
            {
                if (_monitoringHotKeys.ContainsKey(key))
                {
                    NativeMethods.UnregisterHotKey(_handle, (int)key);
                    _monitoringHotKeys.Remove(key);
                }
            }
            catch (Exception ex)
            {
                Diagnostics.LogException(ex);
            }
        }
        #endregion

        #region IsOpen

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            private set { SetValue(IsOpenProperty, value); }
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(ToolWindow), new PropertyMetadata(true, OnIsOpenChangedStatic));

        protected virtual void OnIsOpenChanged(bool value, bool useTransitions = true)
        {
            Storyboard s = null;
            switch (_currentDockState)
            {
                case DockState.Top:
                    if (value)
                        s = (Storyboard)TryFindResource("GrowStoryboardTop") as Storyboard;
                    else
                        s = (Storyboard)TryFindResource("ShrinkStoryboardTop") as Storyboard;
                    break;
                case DockState.Left:
                    if (value)
                        s = (Storyboard)TryFindResource("GrowStoryboardLeft") as Storyboard;
                    else
                        s = (Storyboard)TryFindResource("ShrinkStoryboardLeft") as Storyboard;
                    break;
                case DockState.Right:
                    if (value)
                        s = (Storyboard)TryFindResource("GrowStoryboardRight") as Storyboard;
                    else
                        s = (Storyboard)TryFindResource("ShrinkStoryboardRight") as Storyboard;
                    break;
            }

            if (s != null)
            {
                if (!value)
                {
                    IsFullyOpen = false;
                }

                if (_activeStoryboard != null)
                {
                    // Stop the active storyboard so that its Completed event doesn't fire. This ensures
                    // that our _isAnimating bool accurately tracks whether an animation is on-going.
                    _activeStoryboard.Stop();
                }
                _isAnimating = true;
                _activeStoryboard = s;

                s.Begin();
            }
            else
            {
                // We are floating, always allow clicks
                IsFullyOpen = true;
            }
        }

        private static void OnIsOpenChangedStatic(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //var self = d as ToolWindow;

            //if (self != null)
            //{
            //    self.OnIsOpenChanged((bool)e.NewValue);
            //}
        }

        private void AfterGrowStoryboard(object sender, EventArgs e)
        {
            IsFullyOpen = true;
            _isAnimating = false;
            _activeStoryboard = null;
        }

        private void AfterShrinkStoryboard(object sender, EventArgs e)
        {
            _isAnimating = false;
            _activeStoryboard = null;
        }

        private void RecalculateIsOpen()
        {
            IsOpen = true;
            //IsOpen = IsOpenByMouseOver
            //|| IsOpenByTimer
            //|| IsOpenByPenOrTouch;
        }

        private bool _isOpenByTimer;

        public bool IsOpenByTimer
        {
            get { return _isOpenByTimer; }
            set
            {
                if (value != _isOpenByTimer)
                {
                    _isOpenByTimer = value;
                    RecalculateIsOpen();

                    if (value)
                    {
                        _autoCloseTimer.Start();
                    }
                    else
                    {
                        _autoCloseTimer.Stop();
                    }
                }
            }
        }

        public void RestartOpenByTimer()
        {
            IsOpenByTimer = false;
            IsOpenByTimer = true;
        }

        private bool _isOpenByMouseOver;

        private bool IsOpenByMouseOver
        {
            get { return _isOpenByMouseOver; }
            set
            {
                if (value != _isOpenByMouseOver)
                {
                    _isOpenByMouseOver = value;
                    RecalculateIsOpen();
                }
            }
        }

        private bool _isOpenByPenOrTouch;

        private bool IsOpenByPenOrTouch
        {
            get { return _isOpenByPenOrTouch; }
            set
            {
                if (value != _isOpenByPenOrTouch)
                {
                    _isOpenByPenOrTouch = value;
                    RecalculateIsOpen();
                }
            }
        }

        readonly System.Timers.Timer _autoCloseTimer = new System.Timers.Timer(5000) { AutoReset = false }; // 5 sec.

        public void AutoCloseTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (Application.Current != null)
                Application.Current.Dispatcher.Invoke(() => { IsOpenByTimer = false; });
        }

        #endregion

        #region IsFullyOpen

        private bool _isFullyOpen;


        public bool IsFullyOpen
        {
            get
            {
                return _isFullyOpen;
            }

            set
            {
                if (value != _isFullyOpen)
                {
                    _isFullyOpen = value;
                    OnIsFullyOpenChanged();
                }
            }
        }

        private void OnIsFullyOpenChanged()
        {
            RecalculateIfClickIsAvailable();
        }

        private void RecalculateIfClickIsAvailable()
        {
            Root.IsHitTestVisible = IsFullyOpen
                                    && !IsWaitingForStylusSwipeToFinish;

            //Debug.WriteLine("    IsFullyOpen = " + IsFullyOpen.ToString());
            //Debug.WriteLine("    IsWaitingForStylusSwipeToFinish = " + IsWaitingForStylusSwipeToFinish.ToString());
            //Debug.WriteLine("    IsHitTestVisible = " + Root.IsHitTestVisible.ToString());
        }

        #endregion

        #region IsWaitingForStylusSwipeToFinish

        private bool _isWaitingForStylusSwipeToFinish;

        public bool IsWaitingForStylusSwipeToFinish
        {
            get
            {
                return _isWaitingForStylusSwipeToFinish;
            }

            set
            {
                if (value != _isWaitingForStylusSwipeToFinish)
                {
                    _isWaitingForStylusSwipeToFinish = value;
                    OnIsWaitingForStylusSwipeToFinish();
                }
            }
        }

        private void OnIsWaitingForStylusSwipeToFinish()
        {
            RecalculateIfClickIsAvailable();
        }

        #endregion

        #region Show/Hide

        internal void ShowToolWindow(bool isOpen, bool force = false)
        {
            if (force || !UserSettings.DisableToolWindow)
            {
                IsOpen = isOpen;

                Storyboard s = (Storyboard)TryFindResource("ShowStoryboard") as Storyboard;

                if (s != null)
                {
                    Opacity = 0;
                    s.Begin();
                }
                else
                {
                    Opacity = 1;
                }
                IsOpenByTimer = isOpen;
                Show();
            }

            // a hack to get window handle created so hotkey hook can be initialized when user disabled the tool window
            if (_source == null && UserSettings.DisableToolWindow)
            {
                Opacity = 0;
                Show();
                Hide();
                Opacity = 1;
            }
        }

        internal void HideToolWindow()
        {
            IsFullyOpen = false;

            // Make sure we are Collapsed
            IsOpenByMouseOver = false;
            IsOpenByPenOrTouch = false;
            IsOpenByTimer = false;

            Hide();
            Opacity = 0;

            // Close toggle panel and uncheck the toggle panel button when tool window is closed
            TogglePanel.IsChecked = false;
        }

        #endregion

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            IsOpenByMouseOver = true;

            // Disable the timer, if it's running
            IsOpenByTimer = false;
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            IsOpenByMouseOver = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //_autoCloseTimer.Elapsed += AutoCloseTimerElapsed;
            PositionWindow();
            Closed += (o, args) =>
            {
                //_autoCloseTimer.Elapsed -= AutoCloseTimerElapsed;
            };

            // Added functionality where toggle panel window will move with the tool window
            Window ToolWindow = Window.GetWindow(TogglePanel);

            if (null != ToolWindow)
            {
                ToolWindow.LocationChanged += delegate (object sender2, EventArgs args)
                {
                    var offset = MyPopup.HorizontalOffset;

                    // "bump" the offset to cause the popup to reposition itself
                    // on its own
                    MyPopup.HorizontalOffset = offset + 1;
                    MyPopup.HorizontalOffset = offset;
                };
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PositionWindow();
        }

        private void PositionWindow()
        {
            _isRepositioning = true;   // suppress processing of LocationChanged events

            // Default tool position
            LayoutUtilities.PositionWindowOnPrimaryWorkingAreaWithOffsetIngnoringSystemDPI(this, HorizontalAlignment.Right, VerticalAlignment.Top, 0.0f, 100.0f);

            // Now, raise LocationChanged event to do final repositioning
            this.OnLocationChanged(new EventArgs());
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.HideToolWindow(); // keep running in the system tray
            AppManager.TheBoss.TrayIcon.Activate();
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Store the mouse position
            this._startPoint = e.GetPosition(null);
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (!_isRepositioning)
            {
                // Position was changed by Windows, e.g. due to screen resizing or app reloading
                // Snap to screen boundaries
                _isRepositioning = true;
                var scale = LayoutUtilities.GetDpiScale(this);
                scale = DpiUtilities.GetSystemScale();
                scale = SystemScale;
                var screen = System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point((int)Left, (int)Top));
                Left = Math.Min(Math.Max(Left, screen.WorkingArea.Left / scale.X), screen.WorkingArea.Right / scale.X - ActualWidth);
                Top = Math.Min(Math.Max(Top, screen.WorkingArea.Top / scale.Y), screen.WorkingArea.Bottom / scale.Y - ActualHeight);

                // Make sure docking state is correct
                var dockState = ToolsDockState(scale, screen);
                Trace.WriteLine("Window position changed, new position: (" + Left + ", " + Top + "), Current dock state:" + _currentDockState + ", Dock state for position:" + dockState);
                if (dockState != _currentDockState)
                {
                    // Adjust tool window position to match the old dock state
                    switch (_currentDockState)
                    {
                        case DockState.Top:
                            Top = screen.WorkingArea.Top / scale.Y;
                            break;
                        case DockState.Left:
                            Left = screen.WorkingArea.Left / scale.X;
                            break;
                        case DockState.Right:
                            Left = screen.WorkingArea.Right / scale.X - ActualWidth;
                            break;
                        case DockState.Bottom:
                            Top = screen.WorkingArea.Bottom / scale.Y - ActualHeight;
                            break;
                        case DockState.Middle:
                            Left = (screen.WorkingArea.Left + screen.WorkingArea.Right) / 2 / scale.X - ActualWidth / 2;
                            Top = (screen.WorkingArea.Top + screen.WorkingArea.Bottom) / 2 / scale.Y - ActualHeight / 2;
                            break;
                    }
                    Trace.WriteLine("Adjusted dockState:" + _currentDockState + ", adjusted position: (" + Left + ", " + Top + ")");
                }
                _isRepositioning = false;

                Settings.Default.ToolDocking = _currentDockState.ToString();
                Settings.Default.ToolPosition = new System.Drawing.Point((int)Left, (int)Top);
                Settings.Default.Save();
            }
        }

        /// <summary>
        /// Detect if the tools are docked at the edge of the screen
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="screen"></param>
        /// <returns></returns>
        private DockState ToolsDockState(DpiScale scale, System.Windows.Forms.Screen screen)
        {
            if ((int)Math.Round(Top * scale.Y) <= screen.WorkingArea.Top)
                return DockState.Top;
            else if ((int)Math.Round(Left * scale.X) <= screen.WorkingArea.Left)
                return DockState.Left;
            else if ((int)Math.Round((Left + ActualWidth) * scale.X) >= screen.WorkingArea.Right)
                return DockState.Right;
            else if ((int)Math.Round((Top + ActualHeight) * scale.Y) >= screen.WorkingArea.Bottom)
                return DockState.Bottom;
            return DockState.Middle;
        }

        private void AdjustOrientation(DockState dockState)
        {
            // Apply changes
            UpdateLayout();
        }

        private void Self_StylusDown(object sender, StylusDownEventArgs e)
        {
            if (!IsFullyOpen)
            {
                // Enable an open by pen or touch when the style
                // clicks down while we are closed
                IsOpenByPenOrTouch = true;
                IsWaitingForStylusSwipeToFinish = true;
                CaptureStylus();
                e.Handled = true;
            }
        }

        private void Self_StylusUp(object sender, StylusEventArgs e)
        {
            if (IsWaitingForStylusSwipeToFinish)
            {
                IsWaitingForStylusSwipeToFinish = false;
                ReleaseStylusCapture();
            }
        }

        private void Self_Deactivated(object sender, EventArgs e)
        {
            // Enable a soft dismiss for touch when the window is
            // deactivated

            IsOpenByPenOrTouch = false;
        }

        /// <summary>
        /// Show tool window when user click/double click on taskbar
        /// </summary>
        private void Window_Activated(object sender, EventArgs e)
        {
            if (IsOpen)
                return;

            ShowToolWindow(true);
        }

        #region Toggle Panel
        /// <summary>
        /// Sets the settings for whether to copy to clipboard automatically when captured or annotated/modified
        /// </summary>
        private void CopyClipboard_Clicked(object sender, RoutedEventArgs e)
        {
            UserSettings.CopyToClipboardAfterSnip = ((AriToggleSwitch)sender).IsChecked.GetValueOrDefault(false);
        }
        #endregion

        /// <summary>
        /// Sets the open editor setting in the tool window
        /// </summary>
        private void OpenEditor_Clicked(object sender, RoutedEventArgs e)
        {
            UserSettings.IsOpenEditorPostSnip = ((AriToggleSwitch)sender).IsChecked.GetValueOrDefault(false);
        }

        /// <summary>
        /// Sets the enable ai setting in the tool window
        /// </summary>
        private void EnableAI_Clicked(object sender, RoutedEventArgs e)
        {
            UserSettings.IsAIEnabled = ((AriToggleSwitch)sender).IsChecked.GetValueOrDefault(false);
            AppManager.TheBoss.ViewModel.InsightsVisible = UserSettings.IsAIEnabled;
        }

        /// <summary>
        /// Event trigger to move onto the ai toggle if popup is open
        /// </summary>
        private void LostKeyboardFocus_Event(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (MyPopup.IsOpen)
            {
                AIEnableToggle.Focus();
            }
        }

        /// <summary>
        /// Event trigger to move to the settings button once we move past the toggles
        /// </summary>
        private void LostKeyboardFocus_Event_Popup(object sender, KeyboardFocusChangedEventArgs e)
        {
            SettingsButton.Focus();
        }

        private void AriIcon_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                    Application.Current.MainWindow.Top = 3;
                }

                this.DragMove();
            }
        }
    }
}
