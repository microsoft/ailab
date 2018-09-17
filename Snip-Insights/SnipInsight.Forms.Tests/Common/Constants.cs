namespace SnipInsight.Forms.Tests.Common
{
    public static class Constants
    {
        public const string EntitySearchAPIKey = "--ENTER YOUR APIKEY HERE--";
        public const string ImageSearchAPIKey = "--ENTER YOUR APIKEY HERE--";
        public const string ImageAnalysisAndTextRecognitionAPIKey = "--ENTER YOUR APIKEY HERE--";
        public const string TranslatorAPIKey = "--ENTER YOUR APIKEY HERE--";
        public const string LuisAPIKey = "--ENTER YOUR APIKEY HERE--";

        public static readonly string EntitySearchEndpoint = Schema + "api.cognitive.microsoft.com/bing/v7.0";
        public static readonly string ImageAnalysisEndpoint = Schema + "northeurope.api.cognitive.microsoft.com/vision/v1.0";
        public static readonly string ImageSearchEndPoint = Schema + "api.cognitive.microsoft.com/bing/v7.0";
        public static readonly string TextRecognitionEndPoint = Schema + "northeurope.api.cognitive.microsoft.com/vision/v1.0";
        public static readonly string TranslatorEndPoint = Schema + "api.microsofttranslator.com";
        public static readonly string LUISEndPoint = Schema + "northeurope.api.cognitive.microsoft.com/luis/v2.0";

        public static readonly string LUISAppId = "b0580bc0-6d92-436a-8191-82b5285a5a23";

        private const string Schema = "https://";
    }
}
