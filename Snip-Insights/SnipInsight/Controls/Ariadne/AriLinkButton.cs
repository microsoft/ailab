// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Windows;
using System.Windows.Controls;

namespace SnipInsight.Controls.Ariadne
{
    public class AriLinkButton : Button
    {
        static AriLinkButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AriLinkButton), new FrameworkPropertyMetadata(typeof(AriLinkButton)));
        }
    }
}
