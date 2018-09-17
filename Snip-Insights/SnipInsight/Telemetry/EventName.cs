// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace SnipInsight.Telemetry
{
    public static class EventName
    {
        public const string SnipApplicationInitialized = "SnipApplicationInitialized";
        public const string StateTransition = "StateTransition";
        public const string SingleApiCall = "SingleApiCall";
        public const string CompleteApiCall = "CompleteApiCall";
        public const string ButtonClick = "ButtonClick";
        public const string VersionChange = "VersionChange";

        //Cognitive Services Api Call Event types
        public const string CelebrityRecognitionApi = "CelebrityRecognitionApi";
        public const string ContentModerationApi = "ContentModerationApi";
        public const string EntitySearchApi = "EntitySearchApi";
        public const string HandWrittenTextApi = "HandWrittenTextApi";
        public const string ImageAnalysisApi = "ImageAnalysisApi";
        public const string ImageSearchApi = "ImageSearchApi";
        public const string LandmarkRecognitionApi = "LandmarkRecognitionApi";
        public const string PrintTextApi= "PrintTextApi";
        public const string ProductSearchApi = "ProductSearchApi";
        public const string TranslationApi = "TranslationApi";

        //Api Panel Button Clicks
        public const string SuggestedInsightsButton = "SuggestedInsightsButton";
        public const string ProductSearchButton = "ProductSearchButton";
        public const string PeopleSearchButton = "PeopleSearchButton";
        public const string PlaceSearchButton = "PlaceSearchButton";
        public const string OCRButton = "OCRButton";
        public const string ImageSearchButton = "ImageSearchButton";

        //Action Ribbon Button Clicks
        public const string RestoreImageButton = "RestoreImageButton";
        public const string CopyImageButton = "CopyImageButton";
        public const string SaveImageButton = "SaveImageButton";
        public const string SaveImageEmailButton = "SaveImageEmailButton";
        public const string ShareImageSendToOneNoteButton = "ShareImageSendToOneNoteButton";
        public const string RefreshAICommandButton = "RefrestAICommandButton";

        //Clip Ribbon Button Clicks
        public const string PauseButton = "PauseButton";
        public const string StopButton = "StopButton";
        public const string PlayButton = "PlayButton";

        public const string PenSize1Button = "PenSize1Button";
        public const string PenSize3Button = "PenSize3Button";
        public const string PenSize5Button = "PenSize5Button";
        public const string PenSize7Button = "PenSize7Button";
        public const string PenSize9Button = "PenSize9Button";

        public const string BlackColorToggle = "BlackColorToggle";
        public const string RedColorToggle = "RedColorToggle";
        public const string YellowColorToggle = "YellowColorToggle";
        public const string GreenColorToggle = "GreenColorToggle";
        public const string BlueColorToggle = "BlueColorToggle";
    }
}
