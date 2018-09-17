// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;

namespace SnipInsight.Util
{
    public enum SnipHotKey
    {
        QuickCapture = 0,
        ScreenCapture = 1,
        Library = 2,
    }

    internal class HotKeyPressedEventArgs : EventArgs
    {
        internal SnipHotKey KeyPressed { get; set; }
    }
}
