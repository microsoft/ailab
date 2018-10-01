// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using SnipInsight.Controls.Ariadne;
using SnipInsight.Util;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace SnipInsight.Views
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : AriModernWindow
    {
        readonly AppManager _manager;
        TimeSpan _lastCompositionTargetRender;
        TimeSpan _lastCompositionTargetSlowFreq;
        const double c_compositionTargetSlowPeriod = 193; // slow frequency updates, in milliseconds
        const int destructionTimerTimeinMins = 2;

        public MainWindow()
        {
            _manager = AppManager.TheBoss;
            InitializeComponent();
            DataContext = _manager.ViewModel;

            this.Loaded += OnLoaded;

            InputBindings.Add(new KeyBinding(
                AppManager.TheBoss.ViewModel.UndoCommand,
                new KeyGesture(Key.Z, ModifierKeys.Control)));
            InputBindings.Add(new KeyBinding(
                AppManager.TheBoss.ViewModel.RedoCommand,
                new KeyGesture(Key.Y, ModifierKeys.Control)));
        }

        internal void OnLoaded(object sender, RoutedEventArgs e)
        {
            LayoutUtilities.RestoreWindowLocation(this, UserSettings.MainWindowLocation);

            // Both acetate layer and media capture should use the same Timer instance. The timer instancce is global in app manager and never destroyed.
            CompositionTarget.Rendering += OnRendering;
            this.Loaded -= OnLoaded;
        }

        internal AcetateLayer AcetateLayer { get { return acetateLayer; } }

        public bool MainWindowClosedBySystem { get; set; }

        void OnRendering(object sender, EventArgs e)
        {
            try
            {
                if (!IsLoaded) // We register only in OnLoaded. No harm to check as well.
                {
                    return;
                }

                RenderingEventArgs args = (RenderingEventArgs)e;
                // We may be called back twice for the same frame
                if (_lastCompositionTargetRender == args.RenderingTime)
                {
                    return;
                }
                _lastCompositionTargetRender = args.RenderingTime;

                // Low frequency updates
                if ((_lastCompositionTargetRender - _lastCompositionTargetSlowFreq).TotalMilliseconds > c_compositionTargetSlowPeriod)
                {
                    _lastCompositionTargetSlowFreq = _lastCompositionTargetRender;
                }
            }
            catch (Exception ex)
            {
                Diagnostics.LogLowPriException(ex);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UserSettings.MainWindowLocation = LayoutUtilities.SaveWindowLocationToString(this);

            if (!MainWindowClosedBySystem)
            {
                e.Cancel = true;
                AppManager.TheBoss.CloseMainWindow();
            }
        }
        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            CompositionTarget.Rendering -= OnRendering; // No harm if we never registered (loaded)
        }

        #region Window Resizing

        private readonly double ThresholdToMaximize = .9; // If we are filling more than this percentage, just maximize.

        private void OptimizeSizeAndPosition(double contentWidth, double contentHeight)
        {
            if (WindowState == WindowState.Maximized)
            {
                // If Maximized, don't change...
                return;
            }

            //
            // Estimate Chrome requirements
            //

            double estimatedChromeHeight = CaptionBarHeight
                                           + contentGrid.Margin.Top + contentGrid.Margin.Bottom
                                           + 12.0; // A little buffer for padding and to be safe

            double estimatedChromeWidth = contentGrid.Margin.Left
                                        + contentGrid.Margin.Right
                                        + 8.0; // A little padding for the sides and to be safe

            //
            // Estimate Window dimensions required for unscaled content
            //

            double estimatedWindowWidth = contentWidth + estimatedChromeWidth;
            double estimatedWindowHeight = contentHeight + estimatedChromeHeight;

            //
            // Decide the best window size given the content and available working area
            //

            var workingArea = LayoutUtilities.GetWorkingAreaInVirtualPixels(this);

            if ((estimatedWindowWidth > workingArea.Item3 * ThresholdToMaximize)
                || (estimatedWindowHeight > workingArea.Item4 * ThresholdToMaximize))
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                if (estimatedWindowHeight <= this.Height && estimatedWindowWidth <= this.Width)
                {
                    // Just quit if the window size isn't increasing...
                    return;
                }

                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                        new Action(delegate ()
                        {
                            WindowState = WindowState.Normal;

                            if (estimatedWindowHeight > this.Height)
                            {
                                this.Height = estimatedWindowHeight;
                            }

                            if (estimatedWindowWidth > this.Width)
                            {
                                this.Width = estimatedWindowWidth;
                            }

                            LayoutUtilities.PositionWindowInsideCanvas(this, workingArea.Item1, workingArea.Item2, workingArea.Item3, workingArea.Item4);
                        })
                    );
                }
            }
        }

        #endregion

        #region Key Handler

        private void AriModernWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    if (!AppManager.TheBoss.ViewModel.LibraryEnable &&
                        AppManager.TheBoss.ViewModel.DeleteLibraryItemsCommand.CanExecute(null))
                    {
                        AppManager.TheBoss.ViewModel.DeleteLibraryItemsCommand.Execute(null);

                        e.Handled = true;
                    }
                    break;
            }
        }

        #endregion

        #region Library Panel

        private LibraryPanel _libraryPanel;
        private DispatcherTimer _libraryPanelSelfDestructTimer;

        public void OnShowLibrary()
        {
            // Kill off the timer immediately so it doesn't fire
            // while we are loading.
            DestroyLibrarySelfDestructTimer();

            if (_libraryPanel == null)
            {
                LibraryPanel newPanel = new LibraryPanel();

                Binding binding = new Binding("Packages");
                binding.Mode = BindingMode.OneWay;
                newPanel.SetBinding(LibraryPanel.PackagesSourceProperty, binding);

                LibraryPanelContainer.Children.Add(newPanel);

                _libraryPanel = newPanel;
            }

            _libraryPanel.SetInitialFocus();
        }

        public void OnHideLibrary()
        {
            CreateLibrarySelfDestructTimer();
        }

        private void CreateLibrarySelfDestructTimer()
        {
            TimeSpan duration = TimeSpan.FromMinutes(destructionTimerTimeinMins);

            DestroyLibrarySelfDestructTimer();

            _libraryPanelSelfDestructTimer = new DispatcherTimer(duration, DispatcherPriority.Normal, OnLibrarySelfDestruct, Dispatcher);
            _libraryPanelSelfDestructTimer.Start();
        }

        private void DestroyLibrarySelfDestructTimer()
        {
            if (_libraryPanelSelfDestructTimer != null)
            {
                _libraryPanelSelfDestructTimer.IsEnabled = false;
                _libraryPanelSelfDestructTimer.Stop();
                _libraryPanelSelfDestructTimer = null;
            }
        }

        private void OnLibrarySelfDestruct(object sender, EventArgs e)
        {
            try
            {
                if (_libraryPanel != null)
                {
                    var panel = _libraryPanel;

                    _libraryPanel = null;

                    LibraryPanelContainer.Children.Remove(panel);

                    Diagnostics.LogTrace("LibraryPanel: Removed from the visual tree.");
                }
            }
            catch (Exception ex)
            {
                Diagnostics.LogException(ex);
            }
        }

        #endregion

        #region Settings Panel

        private SettingsPanel _settingsPanel;

        internal void OnShowSettings()
        {
            if (_settingsPanel == null)
            {
                SettingsPanel newPanel = new SettingsPanel();

                SettingsPanelContainer.Children.Add(newPanel);

                _settingsPanel = newPanel;
            }
        }

        internal void OnHideSettings()
        {
            if (_settingsPanel != null)
            {
                var panel = _settingsPanel;

                _settingsPanel = null;

                SettingsPanelContainer.Children.Remove(panel);
            }
        }

        #endregion

        #region AIPanel Panel
        public void SetInsightVisibility (Visibility visibility)
        {
            TopRib.InsightsToggle.Visibility = visibility;
        }
        #endregion

        #region Tour

        private EditorWindowTourPanel _activeEditorTour;

        public void ShowEditorTour()
        {
            try
            {
                if (_activeEditorTour != null)
                {
                    // We are already running, so leave
                }

                EditorWindowTourPanel tour = new EditorWindowTourPanel();

                tour.SetValue(Grid.RowProperty, 1);
                tour.SetValue(Grid.RowSpanProperty, 2);

                tour.Completed += Tour_Completed;

                rootGrid.Children.Add(tour);

                _activeEditorTour = tour;

                tour.Start();
            }
            catch (Exception ex)
            {
                // If anything goes wrong, restore the Ribbon
                //clipRibbon.IsHitTestVisible = true;

                Diagnostics.LogException(ex);
            }
        }

        public bool StopEditorTour()
        {
            if (_activeEditorTour != null)
            {
                _activeEditorTour.Stop();

                _activeEditorTour = null;

                return true;
            }

            return false;
        }

        private void Tour_Completed(object sender, EventArgs e)
        {
            _activeEditorTour = null;
        }

        #endregion

        #region FaceRectangles
        /// <summary>
        /// Show the face rectangles on the celebrities
        /// </summary>
        private void ShowFaceRectangles(object sender, RoutedEventArgs e)
        {
            if (AppManager.TheBoss.ViewModel.CelebritiesCanvas != null)
            {
                AppManager.TheBoss.ViewModel.CelebritiesCanvas.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Hide the face rectangles on the celebrities
        /// </summary>
        private void HideFaceRectangles(object sender, RoutedEventArgs e)
        {
            if (AppManager.TheBoss.ViewModel.CelebritiesCanvas != null)
            {
                AppManager.TheBoss.ViewModel.CelebritiesCanvas.Visibility = Visibility.Collapsed;
            }
        }
        #endregion
    }

    /// <summary>
    /// Converter class to bind visibility to a list of boolean parameters
    /// </summary>
    public class InvertMultiBooleanToVisibility : IMultiValueConverter
    {
        /// <summary>
        /// Converts enabled property of navigation buttons to clip visibility.
        /// </summary>
        /// <param name="values">Current status of nagivation buttons</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Visible if at least one button is disabled, otherwise hidden</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.OfType<bool>().Any(b => b == false) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
			throw new NotImplementedException();
        }
    }

    public class ViewBoxConstantFontSizeConverter : IValueConverter
    {
        /// <summary>
        /// Preserves the size of the object based on
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double)) return null;
            double d = (double)value;
            return 32*(d/250);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
