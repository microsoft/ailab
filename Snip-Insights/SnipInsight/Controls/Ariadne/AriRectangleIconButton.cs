// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Windows;

namespace SnipInsight.Controls.Ariadne
{
    public class AriRectangleIconButton : AriIconLabelButton
    {
        static AriRectangleIconButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AriRectangleIconButton), new FrameworkPropertyMetadata(typeof(AriRectangleIconButton)));
        }
    }
}
