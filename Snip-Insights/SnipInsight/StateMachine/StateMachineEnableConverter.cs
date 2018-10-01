// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace SnipInsight.StateMachine
{
    public class StateMachineEnableConverter : IValueConverter
    {
        /// <summary>
        /// Converts state machine's state property to boolean for a UI component.
        /// </summary>
        /// <param name="value">The current state</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">SnipInsightState enum value or a comma seperated string of state enum values. e..g SnipInsightState.Recording or 'Recording, Recorded'</param>
        /// <param name="culture">Ignored.</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = value != null ? value.ToString() : string.Empty;
            var targetStates = parameter.ToString().Split(',');

            return targetStates.Contains(state);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
