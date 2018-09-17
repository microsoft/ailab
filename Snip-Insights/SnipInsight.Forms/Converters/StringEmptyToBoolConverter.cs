using System;
using System.Globalization;
using Xamarin.Forms;

namespace SnipInsight.Forms.Converters
{
    public class StringEmptyToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result;

            var input = value as string;

            result = string.IsNullOrWhiteSpace(input);

            if (parameter != null)
            {
                result = !result;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
