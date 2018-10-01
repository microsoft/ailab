// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace SnipInsight.Util
{
    public class FormatStringConverter : IValueConverter
    {
        public string FormatString { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var formatString = this.FormatString ?? parameter as String;
            Debug.Assert(
                !string.IsNullOrEmpty(formatString),
                "FormatStringConverter requires providing a format string as either the FormatString property or the converter parameter");
            return string.Format(formatString, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}