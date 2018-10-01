// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using SnipInsight.AIServices.AIModels;
using SnipInsight.Util;

namespace SnipInsight.AIServices.AILogic
{
    /// <summary>
    /// Printed Text OCR call
    /// </summary>
    class PrintedTextHandler : CloudService<PrintedModel>
    {
        /// <summary>
        /// Initalizes handler with correct endpoint
        /// </summary>
        public PrintedTextHandler(string keyFile) : base(keyFile)
        {
            Host = UserSettings.GetKey(keyFile + "Endpoint", "westus.api.cognitive.microsoft.com/vision/v1.0");
            Endpoint = "ocr";
            RequestParams = "language=unk&detectOrientation=true";
        }

        protected override string GetDefaultKey()
        {
            return APIKeys.ImageAnalysisAndTextRecognitionAPIKey;
        }
    }
}
