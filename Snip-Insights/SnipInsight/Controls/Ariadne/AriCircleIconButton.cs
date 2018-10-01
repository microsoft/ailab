// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Windows;

namespace SnipInsight.Controls.Ariadne
{
    public class AriCircleIconButton : AriIconLabelButton
    {
        static AriCircleIconButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AriCircleIconButton), new FrameworkPropertyMetadata(typeof(AriCircleIconButton)));
        }
    }
}
