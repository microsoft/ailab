// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Windows;
using System.Windows.Controls;

namespace SnipInsight.Controls.Ariadne
{
    public class AriIcon : Control
    {
        static AriIcon()
        {
            FocusableProperty.OverrideMetadata(typeof(AriIcon), new FrameworkPropertyMetadata(false));
        }
    }
}
