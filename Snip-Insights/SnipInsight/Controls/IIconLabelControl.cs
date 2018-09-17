// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.ComponentModel;
using System.Windows;

namespace SnipInsight.Controls
{
    public class IIconLabelControl
    {
        [TypeConverterAttribute(typeof(LengthConverter))]
        public double IconHeight { get; set; }

        [TypeConverterAttribute(typeof(LengthConverter))]
        public double IconWidth { get; set; }

        string Label { get; set; }
    }
}
