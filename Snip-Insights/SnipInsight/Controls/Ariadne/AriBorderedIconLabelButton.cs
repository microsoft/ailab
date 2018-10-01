// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Windows;

namespace SnipInsight.Controls.Ariadne
{
    public class AriBorderedIconLabelButton : AriIconLabelButton
    {
        static AriBorderedIconLabelButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AriBorderedIconLabelButton), new FrameworkPropertyMetadata(typeof(AriBorderedIconLabelButton)));
        }
    }
}
