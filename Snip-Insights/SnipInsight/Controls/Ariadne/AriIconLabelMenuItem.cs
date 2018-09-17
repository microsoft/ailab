// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Windows;
using System.Windows.Controls;

namespace SnipInsight.Controls.Ariadne
{
    public class AriIconLabelMenuItem : AriMenuItemBase
    {
        public AriIconLabelMenuItem()
        {
            ToolTipService.SetPlacement(this, System.Windows.Controls.Primitives.PlacementMode.Bottom);
            ToolTipService.SetInitialShowDelay(this, 1000);
        }

        static AriIconLabelMenuItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AriIconLabelMenuItem), new FrameworkPropertyMetadata(typeof(AriIconLabelMenuItem)));
        }

        #region Label

        public string Label
        {
            get { return GetValue(LabelProperty) as string; }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(AriIconLabelMenuItem), new PropertyMetadata(null, OnLabelChangedStatic));

        protected virtual void OnLabelChanged(string value)
        {
            Header = value;
            RecomputeToolTip();
            SetValue(System.Windows.Automation.AutomationProperties.HelpTextProperty, value);
        }

        private static void OnLabelChangedStatic(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as AriIconLabelMenuItem;

            if (self != null)
            {
                self.OnLabelChanged(e.NewValue as string);
            }
        }

        #endregion
    }
}
