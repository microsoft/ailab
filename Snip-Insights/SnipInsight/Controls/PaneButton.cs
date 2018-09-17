// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SnipInsight.Controls
{
    /// <summary>
    /// Custom button for panes to allow two images for the Default and MouseOver states,
    /// and background highlight for Pressed and Disabled states.
    /// </summary>
    public class PaneButton : Button
    {
        /// <summary>
        /// Ctor
        /// </summary>
        static PaneButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PaneButton), new FrameworkPropertyMetadata(typeof(PaneButton)));
        }

        /// <summary>
        /// /////////////////////////////////////////////////////////////
        /// </summary>
        public ImageSource DefaultImage
        {
            get { return (ImageSource)GetValue(DefaultImageProperty); }
            set { SetValue(DefaultImageProperty, value); }
        }
        public static readonly DependencyProperty DefaultImageProperty =
            DependencyProperty.Register("DefaultImage", typeof(ImageSource), typeof(PaneButton), new PropertyMetadata(null));

        /// <summary>
        /// /////////////////////////////////////////////////////////////
        /// </summary>
        public ImageSource MouseOverImage
        {
            get { return (ImageSource)GetValue(MouseOverImageProperty); }
            set { SetValue(MouseOverImageProperty, value); }
        }
        public static readonly DependencyProperty MouseOverImageProperty =
            DependencyProperty.Register("MouseOverImage", typeof(ImageSource), typeof(PaneButton), new PropertyMetadata(null));

        /// <summary>
        /// /////////////////////////////////////////////////////////////
        /// </summary>
        public Color BackgroundPressed
        {
            get { return (Color)GetValue(BackgroundPressedProperty); }
            set { SetValue(BackgroundPressedProperty, value); }
        }
        public static readonly DependencyProperty BackgroundPressedProperty =
            DependencyProperty.Register("BackgroundPressed", typeof(Color), typeof(PaneButton), new PropertyMetadata(Color.FromArgb(0x70, 0xFF, 0xFF, 0xFF)));

        /// <summary>
        /// /////////////////////////////////////////////////////////////
        /// </summary>
        public Color BackgroundDisabled
        {
            get { return (Color)GetValue(BackgroundDisabledProperty); }
            set { SetValue(BackgroundDisabledProperty, value); }
        }
        public static readonly DependencyProperty BackgroundDisabledProperty =
            DependencyProperty.Register("BackgroundDisabled", typeof(Color), typeof(PaneButton), new PropertyMetadata(Color.FromArgb(0x70, 0xFF, 0xFF, 0xFF)));

    }
}
