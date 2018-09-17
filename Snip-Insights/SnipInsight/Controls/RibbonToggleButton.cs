// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace SnipInsight.Controls
{
    public class RibbonToggleButton : ToggleButton, IShyControl
    {
        /////////////////////////////////////////////
        /// State images.
        /////////////////////////////////////////////
        static RibbonToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonToggleButton), new FrameworkPropertyMetadata(typeof(RibbonToggleButton)));
        }

        public ImageSource DefaultToggleImage
        {
            get { return (ImageSource)GetValue(DefaultToggleImageProperty); }
            set { SetValue(DefaultToggleImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefaultToggleImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultToggleImageProperty =
            DependencyProperty.Register("DefaultToggleImage", typeof(ImageSource), typeof(RibbonToggleButton), new PropertyMetadata(null));

        public ImageSource DisabledToggleImage
        {
            get { return (ImageSource)GetValue(DisabledToggleImageProperty); }
            set { SetValue(DisabledToggleImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisabledToggleImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisabledToggleImageProperty =
            DependencyProperty.Register("DisabledToggleImage", typeof(ImageSource), typeof(RibbonToggleButton), new PropertyMetadata(null));

        public ImageSource PressedToggleImage
        {
            get { return (ImageSource)GetValue(PressedToggleImageProperty); }
            set { SetValue(PressedToggleImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PressedToggleImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PressedToggleImageProperty =
            DependencyProperty.Register("PressedToggleImage", typeof(ImageSource), typeof(RibbonToggleButton), new PropertyMetadata(null));

        public ImageSource MouseOverToggleImage
        {
            get { return (ImageSource)GetValue(MouseOverToggleImageProperty); }
            set { SetValue(MouseOverToggleImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MouseOverToggleImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseOverToggleImageProperty =
            DependencyProperty.Register("MouseOverToggleImage", typeof(ImageSource), typeof(RibbonToggleButton), new PropertyMetadata(null));

        /////////////////////////////////////////////
        // Checked state content (text or anything else styled appropriately)
        /////////////////////////////////////////////
        public object CheckedContent
        {
            get { return GetValue(CheckedContentProperty); }
            set { SetValue(CheckedContentProperty, value); }
        }
        public static readonly DependencyProperty CheckedContentProperty =
            DependencyProperty.Register("CheckedContent", typeof(object), typeof(RibbonToggleButton), new PropertyMetadata(null));

        public ImageSource CheckedDefaultToggleImage
        {
            get { return (ImageSource)GetValue(CheckedDefaultToggleImageProperty); }
            set { SetValue(CheckedDefaultToggleImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DefaultToggleImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CheckedDefaultToggleImageProperty =
            DependencyProperty.Register("CheckedDefaultToggleImage", typeof(ImageSource), typeof(RibbonToggleButton), new PropertyMetadata(null));

        public ImageSource CheckedPressedToggleImage
        {
            get { return (ImageSource)GetValue(CheckedPressedToggleImageProperty); }
            set { SetValue(CheckedPressedToggleImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PressedToggleImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CheckedPressedToggleImageProperty =
            DependencyProperty.Register("CheckedPressedToggleImage", typeof(ImageSource), typeof(RibbonToggleButton), new PropertyMetadata(null));

        public ImageSource CheckedMouseOverToggleImage
        {
            get { return (ImageSource)GetValue(CheckedMouseOverToggleImageProperty); }
            set { SetValue(CheckedMouseOverToggleImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MouseOverToggleImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CheckedMouseOverToggleImageProperty =
            DependencyProperty.Register("CheckedMouseOverToggleImage", typeof(ImageSource), typeof(RibbonToggleButton), new PropertyMetadata(null));

        public bool IsShy
        {
            get { return (bool)GetValue(IsShyProperty); }
            set { SetValue(IsShyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsShy.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsShyProperty =
            DependencyProperty.Register("IsShy", typeof(bool), typeof(RibbonToggleButton), new PropertyMetadata(false));
    }
}
