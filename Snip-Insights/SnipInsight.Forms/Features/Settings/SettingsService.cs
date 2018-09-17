using Xamarin.Forms;

[assembly: Dependency(typeof(SnipInsight.Forms.Features.Settings.SettingsService))]

namespace SnipInsight.Forms.Features.Settings
{
    public class SettingsService : ISettingsService
    {
        public string ImageAnalysisEndPoint => Settings.ImageAnalysisEndPoint;

        public string ImageAnalysisAPIKey => Settings.ImageAnalysisAPIKey;

        public string EntitySearchEndPoint => Settings.EntitySearchEndPoint;

        public string EntitySearchAPIKey => Settings.EntitySearchAPIKey;

        public string ImageSearchAPIKey => Settings.ImageSearchAPIKey;

        public string TextRecognitionEndPoint => Settings.TextRecognitionEndPoint;

        public string TextRecognitionAPIKey => Settings.TextRecognitionAPIKey;

        public string TranslatorEndPoint => Settings.TranslatorEndPoint;

        public string TranslatorAPIKey => Settings.TranslatorAPIKey;

        public string LuisEndPoint => Settings.LuisEndPoint;

        public string LuisAPPID => Settings.LuisAPPID;

        public string LuisAPIKey => Settings.LuisAPIKey;
    }
}
