// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace SnipInsight.Controls.Ariadne
{
    public class AriIconLabel : Control
    {
        static AriIconLabel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AriIconLabel), new FrameworkPropertyMetadata(typeof(AriIconLabel)));
        }

        #region Label

        public string Label
        {
            get { return GetValue(LabelProperty) as string; }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(AriIconLabel), new PropertyMetadata(null));

        #endregion

        #region LabelPadding

        public Thickness LabelPadding
        {
            get { return (Thickness)GetValue(LabelPaddingProperty); }
            set { SetValue(LabelPaddingProperty, value); }
        }

        public static readonly DependencyProperty LabelPaddingProperty =
            DependencyProperty.Register("LabelPadding", typeof(Thickness), typeof(AriIconLabel), new PropertyMetadata(new Thickness()));

        #endregion

        #region TextWrapping

        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        public static readonly DependencyProperty TextWrappingProperty =
            DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(AriIconLabel), new PropertyMetadata(TextWrapping.NoWrap));

        #endregion

        #region ShowLabel

        public bool ShowLabel
        {
            get { return (bool)GetValue(ShowLabelProperty); }
            set { SetValue(ShowLabelProperty, value); }
        }

        public static readonly DependencyProperty ShowLabelProperty =
            DependencyProperty.Register("ShowLabel", typeof(bool), typeof(AriIconLabel), new PropertyMetadata(true));

        #endregion

        #region Icon

        [BindableAttribute(true)]
        public object Icon
        {
            get { return GetValue(IconProperty) as string; }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(object), typeof(AriIconLabel), new PropertyMetadata(null));

        #endregion

        #region IconPadding

        public Thickness IconPadding
        {
            get { return (Thickness)GetValue(IconPaddingProperty); }
            set { SetValue(IconPaddingProperty, value); }
        }

        public static readonly DependencyProperty IconPaddingProperty =
            DependencyProperty.Register("IconPadding", typeof(Thickness), typeof(AriIconLabel), new PropertyMetadata(new Thickness()));

        #endregion

        #region Orientation

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(AriIconLabel), new PropertyMetadata(Orientation.Vertical));

        #endregion
    }
}
