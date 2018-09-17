// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Windows;
using System.Windows.Controls;

namespace SnipInsight.Controls.Ariadne
{
    [TemplateVisualState(Name = "Shy", GroupName = "ShyStates")]
    [TemplateVisualState(Name = "NotShy", GroupName = "ShyStates")]
    public abstract class AriRadioButtonBase : RadioButton, IAriControl
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            OnIsShyChanged(IsShy, false);
        }

        #region IShy

        public bool IsShy
        {
            get { return (bool)GetValue(IsShyProperty); }
            set { SetValue(IsShyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsShy.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsShyProperty =
            DependencyProperty.Register("IsShy", typeof(bool), typeof(AriRadioButtonBase), new PropertyMetadata(false, OnIsShyChangedStatic));

        protected virtual void OnIsShyChanged(bool value, bool useTransitions = true)
        {
            VisualStateManager.GoToState(this, value ? "Shy" : "NotShy", useTransitions);
        }

        private static void OnIsShyChangedStatic(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as AriRadioButtonBase;

            if (self != null)
            {
                self.OnIsShyChanged((bool)e.NewValue);
            }
        }

        #endregion

    }
}
