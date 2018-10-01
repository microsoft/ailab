// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SnipInsight.Util
{
    /// <summary>
    /// Bind a string value to visibility attribute of UI elements
    /// </summary>
    class StringToVisibility : IValueConverter
    {
        /// <summary>
        /// Convert string value to visibility
        /// </summary>
        /// <param name="value">String to be converted</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Visible if string contains value, collapsed otherwise</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string && !string.IsNullOrEmpty((string)value) ?
                Visibility.Visible :
                Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
