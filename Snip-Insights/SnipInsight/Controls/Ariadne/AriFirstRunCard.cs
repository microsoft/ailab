// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Windows;
using System.Windows.Controls;

namespace SnipInsight.Controls.Ariadne
{
    public class AriFirstRunCard : Control
    {
        static AriFirstRunCard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AriFirstRunCard), new FrameworkPropertyMetadata(typeof(AriFirstRunCard)));
        }

        #region Image

        public object Image
        {
            get { return GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(object), typeof(AriFirstRunCard), new PropertyMetadata(null));

        #endregion

        #region Heading

        public string Heading
        {
            get { return GetValue(HeadingProperty) as string; }
            set { SetValue(HeadingProperty, value); }
        }

        public static readonly DependencyProperty HeadingProperty =
            DependencyProperty.Register("Heading", typeof(string), typeof(AriFirstRunCard), new PropertyMetadata(null));

        #endregion

        #region Message

        public string Message
        {
            get { return GetValue(MessageProperty) as string; }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(AriFirstRunCard), new PropertyMetadata(null));

        #endregion

        public void FlyOn()
        {
            Visibility = Visibility.Visible;
            IsEnabled = true;
        }

        public void FlyOff()
        {
            IsEnabled = false;
        }
    }
}
