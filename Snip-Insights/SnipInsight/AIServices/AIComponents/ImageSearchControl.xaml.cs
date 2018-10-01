// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Windows;
using System.Windows.Controls;
using CommonServiceLocator;
using SnipInsight.AIServices.AIViewModels;

namespace SnipInsight.AIServices.AIComponents
{
    /// <summary>
    /// Interaction logic for ImageSearch.xaml
    /// </summary>
    public partial class ImageSearchControl : UserControl
    {
        /// <summary>
        /// Reference to image search vm
        /// </summary>
        private ImageSearchViewModel ImageSearchVM = ServiceLocator.Current.GetInstance<ImageSearchViewModel>();

        /// <summary>
        /// View for the Image Search
        /// </summary>
        public ImageSearchControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Passes the width of the panel to the bounded VM
        /// </summary>
        private void ImageGallerySizeChanged(object sender, SizeChangedEventArgs e)
        {
            ImageSearchVM.CurrentWrapPanelWidth = ImageDisplayPanel.ActualWidth;
        }
    }
}
