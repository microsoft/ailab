// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.ComponentModel;
using System.Windows;

namespace SnipInsight.Views
{
    public class HighContrastHelper : DependencyObject
    {
        #region Singleton pattern

        private HighContrastHelper()
        {
            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
        }

        private static HighContrastHelper _instance;

        public static HighContrastHelper Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = new HighContrastHelper();
                return _instance;
            }
        }

        #endregion

        public void ApplyCurrentTheme()
        {
            if (!SystemParameters.HighContrast) return;
            var windowbrush = SystemColors.WindowBrush;

            if (windowbrush.Color.R == 255 && windowbrush.Color.G == 255 && windowbrush.Color.B == 255)
            {
                HighContrastHelper.Instance.IsHighContrastLight = true;
            }
            else
            {
                HighContrastHelper.Instance.IsHighContrastLight = false;
            }
        }

        void SystemParameters_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "HighContrast") return;
            HighContrastHelper.Instance.IsHighContrast = SystemParameters.HighContrast;
            ApplyCurrentTheme();
        }

        #region DP IsHighContrast, IsHighContrastLight

        public static readonly DependencyProperty IsHighContrastProperty = DependencyProperty.Register(
            "IsHighContrast",
            typeof(bool),
            typeof(HighContrastHelper),
            new PropertyMetadata(
                false
                ));

        public bool IsHighContrast
        {
            get { return (bool)GetValue(IsHighContrastProperty); }
            private set { SetValue(IsHighContrastProperty, value); }
        }

        public static readonly DependencyProperty IsHighContrastLightProperty = DependencyProperty.Register(
            "IsHighContrastLight",
            typeof(bool),
            typeof(HighContrastHelper),
            new PropertyMetadata(
                false
                ));

        public bool IsHighContrastLight
        {
            get { return (bool)GetValue(IsHighContrastLightProperty); }
            private set { SetValue(IsHighContrastLightProperty, value); }
        }

        #endregion

    }
}
