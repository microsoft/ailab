using System;
using System.Globalization;
using SnipInsight.Forms.Features.Products.Models;
using Xamarin.Forms;

namespace SnipInsight.Forms.Converters
{
    public class ProductOfferToCurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;

            if (value is ProductOffer input && input.PriceCurrency != null)
            {
                if (input.PriceCurrency.Equals("USD"))
                {
                    result = $"US $ {input.Price}";
                }
                else
                {
                    result = $"{input.PriceCurrency} {input.Price}";
                }
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
