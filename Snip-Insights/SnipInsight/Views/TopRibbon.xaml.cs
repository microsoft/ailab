// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SnipInsight.Views
{
    /// <summary>
    /// Interaction logic for TopRibbon.xaml
    /// </summary>
    public partial class TopRibbon : UserControl
    {
        public TopRibbon()
        {
            InitializeComponent();
        }
    }

	/// <summary>
	/// Inverts booleans for xaml
	/// </summary>
	public class InvertBooleanToVisibility : IValueConverter
	{
		/// <summary>
		/// Converts booleans to visibilty, for single parameter
		/// </summary>
		/// <param name="values">Current status of nagivation buttons</param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns>Returns the visible for true, collapsed otherwise</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var res = (value is bool) ? ((bool)value ? Visibility.Collapsed : Visibility.Visible) : throw new ArgumentException();
            return res;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
