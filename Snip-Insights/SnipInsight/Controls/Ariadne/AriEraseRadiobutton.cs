// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Windows;

namespace SnipInsight.Controls.Ariadne
{
    public class AriEraseRadioButton : AriRadioButtonBase
    {
        static AriEraseRadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AriEraseRadioButton), new FrameworkPropertyMetadata(typeof(AriEraseRadioButton)));
        }

        #region Label

        public string Label
        {
            get { return GetValue(LabelProperty) as string; }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(AriEraseRadioButton), new PropertyMetadata(null, OnLabelChangedStatic));

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
            var self = d as AriEraseRadioButton;

            if (self != null)
            {
                self.OnLabelChanged(e.NewValue as string);
            }
        }

        #endregion
    }
}
