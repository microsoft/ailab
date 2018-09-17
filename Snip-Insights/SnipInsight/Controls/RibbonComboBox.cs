// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Windows;
using System.Windows.Controls;

namespace SnipInsight.Controls
{
    public class RibbonComboBox : ComboBox, IShyControl
    {
        public bool IsShy
        {
            get { return (bool)GetValue(IsShyProperty); }
            set { SetValue(IsShyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsShy.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsShyProperty =
            DependencyProperty.Register("IsShy", typeof(bool), typeof(RibbonComboBox), new PropertyMetadata(false));

    }
}
