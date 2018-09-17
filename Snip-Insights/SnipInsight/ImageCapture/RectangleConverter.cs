// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System.Windows.Markup;

namespace SnipInsight.ImageCapture
{
    public class RectangleConverter : MarkupExtension, IMultiValueConverter
    {
        private static RectangleConverter _instance;

        #region IValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2)
            {
                var width = System.Convert.ToDouble(values[0]);
                var height = System.Convert.ToDouble(values[1]);
                return new Rect(0, 0, width, height);
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion

        public RectangleConverter()
        {
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new RectangleConverter());
        }
    }
}
