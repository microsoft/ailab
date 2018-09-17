// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommonServiceLocator;
using SnipInsight.AIServices.AIModels;

namespace SnipInsight.AIServices.AIViewModels
{
    public class ProductDynamicDisplay : BaseDynamicDisplay
    {
        /// <summary>
        /// The image control to be resized
        /// </summary>
        private ObservableCollection<ProductSearchModel> renderedProducts;

        /// <summary>
        /// The offset height to style the grid
        /// </summary>
        private const double heightOffset = 80;

        /// <summary>
        /// Populating the aspect ratio dictionary
        /// </summary>
        public override void PopulateAspectRatioDict()
        {
            var productSearchVM = ServiceLocator.Current.GetInstance<ProductSearchViewModel>();

            aspectRatios = new List<double>();
            if (productSearchVM.Products == null)
            {
                return;
            }

            renderedProducts = productSearchVM.Products;
            foreach (ProductSearchModel ip in renderedProducts)
            {
                aspectRatios.Add(GetAspectRatio(ip.Width, ip.Height));
            }

            LowerBound = double.MaxValue;
            UpperBound = double.MinValue;
        }

        /// <summary>
        /// Resizes the image row
        /// </summary>
        /// <param name="startIndex">The index of the collection to start resizing</param>
        /// <param name="numImages">The number of images to resize</param>
        /// <param name="newHeight">The new height of the image resized</param>
        protected override void SizeRow(int startIndex, int numImages, double newHeight)
        {
            for (int i = startIndex; i < renderedProducts.Count && i < startIndex + numImages; ++i)
            {
                renderedProducts[i].Width = newHeight * aspectRatios[i];
                renderedProducts[i].Height = newHeight + heightOffset;

                if (renderedProducts[i].Width < ResizeDisplayThreshhold)
                {
                    ResizeDisplayThreshhold = renderedProducts[i].Width;
                }
            }
        }
    }
}
