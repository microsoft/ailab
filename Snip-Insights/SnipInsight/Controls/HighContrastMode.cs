// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Diagnostics;
using System.Windows;

namespace SnipInsight.Controls
{
    public class HighContrastMode : DependencyObject
    {
        public HighContrastMode()
        {
            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
        }

        public static HighContrastMode Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HighContrastMode();
                    _instance.IsHighContrast = SystemParameters.HighContrast;
                }
                return _instance;
            }
        }

        private static HighContrastMode _instance;

        private void SystemParameters_StaticPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "HighContrast")
            {
                HighContrastMode.Instance.IsHighContrast = SystemParameters.HighContrast;
            }
        }

        #region DynamicProperty IsHighContrast

        public static readonly DependencyProperty IsHighContrastProperty = DependencyProperty.Register(
            "IsHighContrast", typeof(bool), typeof(HighContrastMode), new PropertyMetadata(false));

        public bool IsHighContrast
        {
            get { return (bool)GetValue(IsHighContrastProperty); }
            private set { SetValue(IsHighContrastProperty, value); }
        }

        #endregion
    }
}
