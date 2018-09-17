using System;
using System.Globalization;
using Xamarin.Forms;

namespace SnipInsight.Forms.Converters
{
    public class NotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result;

            if (value is bool input)
            {
                result = !input;
            }
            else
            {
                result = false;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
