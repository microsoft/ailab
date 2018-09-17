// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using SnipInsight.Views;
using System.Windows;
using System.Windows.Controls;

namespace SnipInsight.Controls
{
    public class RibbonGroupPanel : ContentControl, IShyControl
    {
        static RibbonGroupPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonGroupPanel), new FrameworkPropertyMetadata(typeof(RibbonGroupPanel)));
        }

        #region IsShy

        public bool IsShy
        {
            get { return (bool)GetValue(IsShyProperty); }
            set { SetValue(IsShyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsShy.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsShyProperty =
            DependencyPropertyUtilities.Register<RibbonGroupPanel, bool>("IsShy", false, (o, p) => { o.OnShyChanged(p); });

        private void OnShyChanged(bool value)
        {
            ShowLabel = !value;
        }

        #endregion

        public bool ShowLabel
        {
            get { return (bool)GetValue(ShowLabelProperty); }
            set { SetValue(ShowLabelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowLabel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowLabelProperty =
            DependencyProperty.Register("ShowLabel", typeof(bool), typeof(RibbonGroupPanel), new PropertyMetadata(true));

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Label.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(RibbonGroupPanel), new PropertyMetadata(null));

    }
}
