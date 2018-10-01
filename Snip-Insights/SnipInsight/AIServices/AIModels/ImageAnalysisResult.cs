// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace SnipInsight.AIServices.AIModels
{
    /// <summary>
    /// Image metadata from the image analysis api response
    /// </summary>
    class ImageAnalysisResult
    {
        /// <summary>
        /// Initialize metadata availability to false when creating a new object
        /// </summary>
        internal ImageAnalysisResult()
        {
            CaptionAvailable = false;
            TagsAvailable = false;
            Caption = string.Empty;
            Tags = new string[0];
        }

        /// <summary>
        /// True if caption for image is available, false otherwise
        /// </summary>
        internal bool CaptionAvailable { get; set; }

        /// <summary>
        /// True if tags for image is available, false otherwise
        /// </summary>
        internal bool TagsAvailable { get; set; }

        /// <summary>
        /// Description of the image content
        /// </summary>
        internal string Caption { get; set; }

        /// <summary>
        /// Contents of the image identified by the image analysis API
        /// </summary>
        internal string[] Tags { get; set; }
    }
}
