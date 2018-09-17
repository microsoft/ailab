// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Windows;
using System.Windows.Controls;
using SnipInsight.Util;

namespace SnipInsight.Controls.Ariadne
{
    public class AriSuperTip : ContentControl
    {
        public event EventHandler AfterFadeIn;

        static AriSuperTip()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AriSuperTip), new FrameworkPropertyMetadata(typeof(AriSuperTip)));
        }

        public void FadeIn()
        {
            Opacity = 0;
            Visibility = Visibility.Visible;

            var s = AnimationUtilities.CreateFadeInStoryboard(this, 500);

            // Delay by half a second to decrease the overlap of two tooltips
            s.BeginTime = TimeSpan.FromSeconds(1);

            s.Completed += (sender, evt) =>
                {
                    if (AfterFadeIn != null)
                    {
                        AfterFadeIn(this, EventArgs.Empty);
                    }
                };

            s.Begin();
        }

        public void FadeOut()
        {
            AnimationUtilities.FadeOutAndRemove(this, 500);
        }
    }
}
