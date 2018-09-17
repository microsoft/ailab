// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using SnipInsight.AIServices.AIViewModels;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SnipInsight.AIServices.AIComponents
{
    /// <summary>
    /// Interaction logic for OCRControl.xaml
    /// </summary>
    public partial class OCRControl : UserControl
    {
        public OCRControl()
        {
            InitializeComponent();
        }

    }

    /// <summary>
    /// Converter class to bind visibility to a list of boolean parameters
    /// </summary>
    public class MultiBooleanToVisibility : IMultiValueConverter
    {
        /// <summary>
        /// Converts enabled property of navigation buttons to clip visibility.
        /// </summary>
        /// <param name="values">Current status of nagivation buttons</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>Visible if at least one button is disabled, otherwise hidden</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (object oj in values)
            {
                if ((Visibility)oj == Visibility.Visible)
                {
                    return Visibility.Visible;
                }
            }

            return Visibility.Collapsed;
        }

        /// <summary>
        /// Not Implemented, does not need to be implementeds but required to be overriden as part of converter interface
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetTypes"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}