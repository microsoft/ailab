// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using SnipInsight.Controls;
using SnipInsight.Controls.Ariadne;

namespace SnipInsight.Views
{
    /// <summary>
    /// Interaction logic for FirstRunWindow.xaml
    /// </summary>
    public partial class FirstRunWindow : DpiAwareWindow
    {
        readonly TimeSpan timerInterval = TimeSpan.FromSeconds(6);
        DispatcherTimer timer;

        public FirstRunWindow()
        {
            InitializeComponent();
        }

        public void CloseWindow()
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer = new DispatcherTimer();
            timer.Interval = timerInterval;
            timer.Tick += timer_Tick;

            // MoveNext starts the timer
            MoveNext();
            LayoutUtilities.PositionWindowOnPrimaryWorkingArea(this, HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Prevent double close
            this.Deactivated -= Window_Deactivated;

            Close();
        }

        void Window_Deactivated(object sender, EventArgs e)
        {
            // Removing the Soft Dismiss for now. We don't think this is necessarily helpful.
            //Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MoveNext();
        }

        void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            timer.Stop();
        }

        void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            MoveNext();
        }

        #region Cards

        private int currentCardIndex = -1;
        AriFirstRunCard currentCard = null;
        Ellipse currentBall = null;

        private void MoveNext()
        {
            MoveTo((currentCardIndex + 1) % 4);
        }

        private void MovePrevious()
        {
            MoveTo((currentCardIndex - 1) % 4);
        }

        private void MoveTo(int index)
        {
            if (index == currentCardIndex)
            {
                return;
            }

            switch (index)
            {
                case 0:
                    MoveTo(Card0, Ball0);
                    break;
                case 1:
                    MoveTo(Card1, Ball1);
                    break;
                case 2:
                    MoveTo(Card2, Ball2);
                    break;
                case 3:
                    MoveTo(Card3, Ball3);
                    break;
                default:
                    // if we've moved off the ends, we don't
                    // want to save the new position
                    return;
            }

            currentCardIndex = index;
        }

        private void MoveTo(AriFirstRunCard newCard, Ellipse newBall)
        {
            if (currentBall != null)
            {
                currentBall.Fill = new SolidColorBrush(Color.FromRgb(201, 201, 201));
            }

            if (currentCard != null)
            {
                currentCard.IsEnabled = false;
            }

            newBall.Fill = new SolidColorBrush(Color.FromRgb(104, 98, 209));
            newCard.IsEnabled = true;

            currentCard = newCard;
            currentBall = newBall;
            timer.Stop();
            timer.Start();
        }

        private void Ball0_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MoveTo(0);
            e.Handled = true;
        }

        private void Ball1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MoveTo(1);
            e.Handled = true;
        }

        private void Ball2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MoveTo(2);
            e.Handled = true;
        }

        private void Ball3_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MoveTo(3);
            e.Handled = true;
        }

        #endregion

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // The Tool Window should never be null, but just to be extra safe...
            if (AppManager.TheBoss.ToolWindow != null)
            {
                AppManager.TheBoss.ToolWindow.RestartOpenByTimer();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                case Key.Enter:
                case Key.Right:
                case Key.Down:
                case Key.PageDown:
                    MoveNext();
                    e.Handled = true;
                    break;
                case Key.Left:
                case Key.Up:
                case Key.PageUp:
                    MovePrevious();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    Close();
                    e.Handled = true;
                    break;
            }
        }
    }
}
