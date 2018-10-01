// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using SnipInsight.StateMachine;
using SnipInsight.Util;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SnipInsight.Views
{
    /// <summary>
    /// Interaction logic for NotificationWindow.xaml
    /// Show up a toast to notify that the image was captured and saved
    /// Allow for a quick editor access
    /// </summary>
    public partial class NotificationWindow : Window
    {

        const int offsetTop = 220;
        const int offsetLeft = 10;

        public NotificationWindow()
        {
            InitializeComponent();

            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;

            // Toast window position, top right of the screen.
            this.Left = desktopWorkingArea.Right - this.Width - offsetLeft;
            this.Top = desktopWorkingArea.Bottom - offsetTop;

            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                var workingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
                var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
                var corner = transform.Transform(new Point(workingArea.Right, workingArea.Top));
            }));
        }

        /// <summary>
        /// Open the editor when the window is clicked
        /// </summary>
        protected void OpenInEditor(object sender, EventArgs e)
        {
            AppManager.TheBoss.ViewModel.SelectedPackage = AppManager.TheBoss.ViewModel.Packages[0];
            AppManager.TheBoss.ViewModel.AIAlreadyRan = true;
            AppManager.TheBoss.ViewModel.StateMachine.Fire(SnipInsightTrigger.LoadImageFromLibrary);
            this.Close();
        }

        /// <summary>
        /// Close the window without handling
        /// </summary>
        private void CloseWindow(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            this.Close();
        }

        /// <summary>
        /// Open save folder location
        /// </summary>
        private void OpenFolder(object sender, MouseButtonEventArgs e)
        {
            Process.Start(UserSettings.CustomDirectory);
            this.Close();
        }
    }
}
