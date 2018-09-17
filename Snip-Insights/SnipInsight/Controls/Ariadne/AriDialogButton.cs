// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Windows;

namespace SnipInsight.Controls.Ariadne
{
    public class AriDialogButton : AriButtonBase
    {
        static AriDialogButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AriDialogButton), new FrameworkPropertyMetadata(typeof(AriDialogButton)));
        }

    }
}
