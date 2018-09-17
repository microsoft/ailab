// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Windows;
using SnipInsight.Controls;

namespace SnipInsight.Views
{
    /// <summary>
    /// Interaction logic for TwoButtonDialog.xaml
    /// </summary>
    public partial class TwoButtonDialog : DpiAwareWindow
    {
        public TwoButtonDialog(string message, string button1Text, string button2Text)
        {
            InitializeComponent();
            DialogMessageText.Text = message;
            Button1.Content = button1Text;
            Button2.Content = button2Text;
        }

        public void Reset() // useful if the same dialog is reused.
        {
            Button1Clicked = false;
            Button2Clicked = false;
        }

        public bool Button1Clicked { get; set; }

        public bool Button2Clicked { get; set; }

        private void Button1_OnClick(object sender, RoutedEventArgs e)
        {
            Button1Clicked = true;
            Close();
        }

        private void Button2_OnClick(object sender, RoutedEventArgs e)
        {
            Button2Clicked = true;
            Close();
        }
    }
}
