// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using CommonServiceLocator;
using SnipInsight.AIServices.AIViewModels;
using System.Windows.Controls;

namespace SnipInsight.AIServices.AIComponents
{
    /// <summary>
    /// Interaction logic for ProductSearchControl.xaml
    /// </summary>
    public partial class ProductSearchControl : UserControl
    {
        /// <summary>
        /// Reference to product search vm
        /// </summary>
        private ProductSearchViewModel ProductSearchVM = ServiceLocator.Current.GetInstance<ProductSearchViewModel>();

        public ProductSearchControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Passes the width of the panel to the bounded VM
        /// </summary>
        private void ProductGallerySizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            ProductSearchVM.CurrentWrapPanelWidth = ProductDisplayPanel.ActualWidth;
        }
    }
}
