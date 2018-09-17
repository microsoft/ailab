// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace SnipInsight.Views
{
    /// <summary>
    /// Interaction logic for ToastControl.xaml
    /// </summary>
    public partial class ToastControl : UserControl
    {
        private int _interval;

        public ToastControl(string message)
        {
            InitializeComponent();
            this.Notification_Message.Text = message;
            this.Loaded += new RoutedEventHandler(Control_Loaded);
            _interval = 5000;
        }

        public ToastControl(string message, int interval)
        {
            InitializeComponent();
            this.Notification_Message.Text = message;
            this.Loaded += new RoutedEventHandler(Control_Loaded);
            _interval = interval;
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

        public void Control_Loaded(object sender, RoutedEventArgs e)
        {
            Timer t = new Timer();
            t.Interval = _interval;
            t.Elapsed += new ElapsedEventHandler(t_Elapsed);
            t.AutoReset = false;
            t.Start();
        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (this.Parent as Grid != null)
                {
                    ((Grid)this.Parent).Children.Remove(this);
                }
            }), null);
        }
    }
}
