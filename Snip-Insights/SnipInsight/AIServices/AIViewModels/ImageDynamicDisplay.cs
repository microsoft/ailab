// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using CommonServiceLocator;
using SnipInsight.AIServices.AIModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SnipInsight.AIServices.AIViewModels
{
    public class ImageDynamicDisplay : BaseDynamicDisplay
    {
        /// <summary>
        /// The image control to be resized
        /// </summary>
        private ObservableCollection<ImageSearchModel> renderedImages;

        /// <summary>
        /// Populating the aspect ratio dictionary
        /// </summary>
        public override void PopulateAspectRatioDict()
        {
            var imageSearchVM = ServiceLocator.Current.GetInstance<ImageSearchViewModel>();

            aspectRatios = new List<double>();
            if (imageSearchVM == null || imageSearchVM.Images == null)
            {
                return;
            }

            renderedImages = imageSearchVM.Images;
            foreach (ImageSearchModel ic in renderedImages)
            {
                aspectRatios.Add(GetAspectRatio(ic.Width, ic.Height));
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
            for (int i = startIndex; i < renderedImages.Count && i < startIndex + numImages; ++i)
            {
                renderedImages[i].Width = newHeight * aspectRatios[i];
                renderedImages[i].Height = newHeight;

                if (renderedImages[i].Width < ResizeDisplayThreshhold)
                {
                    ResizeDisplayThreshhold = renderedImages[i].Width;
                }
            }
        }
    }
}
