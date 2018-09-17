// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;

namespace SnipInsight.AIServices.AIViewModels
{
    public abstract class BaseDynamicDisplay
    {
        /// <summary>
        /// The width of the panel which the images resized to
        /// </summary>
        public double CenterBound = 0;

        /// <summary>
        /// The thresh hold which we resize the display
        /// </summary>
        protected double ResizeDisplayThreshhold = double.MaxValue;

        /// <summary>
        /// An array storing the aspect ratios of the controls to be analyzed
        /// </summary>
        protected List<double> aspectRatios;

        /// <summary>
        /// The lower bound of needing another resize
        /// </summary>
        protected double LowerBound = double.MaxValue;

        /// <summary>
        /// The upper bound of needing another resize
        /// </summary>
        protected double UpperBound = double.MinValue;

        /// <summary>
        /// Creates a control gallery, similar to the Bing Image Search Website
        /// </summary>
        /// <param name="maxHeight">The maximum height for the resized images</param>
        /// <param name="panelWidth">The panel width for the resized images</param>
        public void CreateControlGallery(double maxHeight, double panelWidth)
        {
            if (IsBetween(panelWidth, LowerBound, UpperBound))
            {
                return;
            }

            int incrementIndex = 0;

            for (int i = 0; i < aspectRatios.Count; i += incrementIndex)
            {
                Tuple<int, double> controlResults = GetNumControls(i, maxHeight, panelWidth);
                SizeRow(i, controlResults.Item1, controlResults.Item2);
                incrementIndex = controlResults.Item1;
            }

            LowerBound = panelWidth - (ResizeDisplayThreshhold);
            UpperBound = panelWidth + (ResizeDisplayThreshhold);
            CenterBound = panelWidth;
        }

        /// <summary>
        /// Populates the aspect ratio dictionary
        /// </summary>
        public abstract void PopulateAspectRatioDict();

        /// <summary>
        /// Sets the controls in the row to the given
        /// </summary>
        /// <param name="startIndex">The start index of image to be resized</param>
        /// <param name="numControls">The number of images to be resized</param>
        /// <param name="newHeight">The new height of the images</param>
        protected abstract void SizeRow(int startIndex, int numControls, double newHeight);

        /// <summary>
        /// Gets the aspect ratio of the image
        /// </summary>
        /// <param name="width">The width of the image</param>
        /// <param name="height">The height of the image</param>
        /// <returns>Image aspect ratio</returns>
        protected double GetAspectRatio(double width, double height)
        {
            return (height <= 0) ? 0 : width / height;
        }

        /// <summary>
        /// Gets the number of controls to be put into a row
        /// </summary>
        /// <param name="startIndex">The start index of the controls</param>
        /// <param name="maxHeight">The max height of the row</param>
        /// <param name="resizeWidth">The width to be resized to</param>
        /// <returns>The number of controls in the row in Item1, and the height of the controls in Item2</returns>
        private Tuple<int, double> GetNumControls(int startIndex, double maxHeight, double resizeWidth)
        {
            int numImages = 0;
            double height = 0;
            double sumRatios = 0;

            // Finds the optimal number of images and height for the row
            for (int i = startIndex; i < aspectRatios.Count && startIndex + numImages <= aspectRatios.Count; ++i)
            {
                ++numImages;
                sumRatios += aspectRatios[i];
                height = FindHeight(sumRatios, resizeWidth);

                if (height <= maxHeight)
                {
                    break;
                }

                if (i == aspectRatios.Count - 1)
                {
                    height = maxHeight;
                    break;
                }
            }

            Tuple<int, double> imageInfo = new Tuple<int, double>(numImages, height);
            return imageInfo;
        }

        /// <summary>
        /// Finds the height of the row based on the sum of the Aspect ratios of the photos
        /// </summary>
        /// <param name="sumRatios"> The sum of the aspect ratios in the row</param>
        /// <param name="resizeWidth"> The width of the row</param>
        /// <returns>Height of the row</returns>
        private double FindHeight(double sumRatios, double resizeWidth)
        {
            return (sumRatios <= 0) ? 0 : resizeWidth / sumRatios;
        }

        /// <summary>
        /// Returns true if num is between lower and upper inclusive, false otherwise
        /// </summary>
        /// <param name="num">num to be evaluated</param>
        /// <param name="lower">lower bound</param>
        /// <param name="upper">upper bound</param>
        /// <returns></returns>
        private bool IsBetween(double num, double lower, double upper)
        {
            return lower <= num && num <= upper;
        }
    }
}
