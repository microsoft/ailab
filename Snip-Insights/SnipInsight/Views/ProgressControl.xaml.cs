// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Windows.Controls;

namespace SnipInsight.Views
{
    /// <summary>
    /// Interaction logic for ProgressControl.xaml
    /// </summary>
    public partial class ProgressControl : UserControl
    {
        public ProgressControl(string message = null)
        {
            InitializeComponent();
            if (message != null)
                Notification_Message.Text = message;
        }

        public void ShowInMainWindow()
        {
            var mainWindow = AppManager.TheBoss.MainWindow;
            if (mainWindow != null)
            {
                mainWindow.rootGrid.Children.Add(this);
                Grid.SetRow(this, 1);
                Grid.SetZIndex(this, 1);
            }
        }

        public void SetProgress(double percentCompleted)
        {
            this.Progress_Bar.Value = percentCompleted;
        }

        public void Dismiss()
        {
            if (this.Parent as Grid != null)
            {
                ((Grid)this.Parent).Children.Remove(this);
            }
        }
    }
}
