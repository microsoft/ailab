// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Windows;
using System.Windows.Controls.Primitives;

namespace SnipInsight.Controls.Ariadne
{
    public class AriToggleSwitch : ToggleButton
    {
        static AriToggleSwitch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AriToggleSwitch), new FrameworkPropertyMetadata(typeof(AriToggleSwitch)));
        }
    }
}
