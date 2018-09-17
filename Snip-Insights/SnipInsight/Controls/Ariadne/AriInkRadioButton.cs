// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Windows;
using System.Windows.Media;

namespace SnipInsight.Controls.Ariadne
{
    public class AriInkRadioButton : AriRadioButtonBase
    {
        static AriInkRadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AriInkRadioButton), new FrameworkPropertyMetadata(typeof(AriInkRadioButton)));
        }

        #region Ink

        public Brush Ink
        {
            get { return GetValue(InkProperty) as Brush; }
            set { SetValue(InkProperty, value); }
        }

        public static readonly DependencyProperty InkProperty =
            DependencyProperty.Register("Ink", typeof(Brush), typeof(AriInkRadioButton), new PropertyMetadata(Brushes.Black));

        #endregion

        #region Label

        public string Label
        {
            get { return GetValue(LabelProperty) as string; }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(AriInkRadioButton), new PropertyMetadata(null, OnLabelChangedStatic));

        protected virtual void OnLabelChanged(string value)
        {
            if (ToolTip == null)
            {
                ToolTip = value;
            }

            SetValue(System.Windows.Automation.AutomationProperties.HelpTextProperty, value);
        }

        private static void OnLabelChangedStatic(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as AriInkRadioButton;

            if (self != null)
            {
                self.OnLabelChanged(e.NewValue as string);
            }
        }

        #endregion
    }
}
