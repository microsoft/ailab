namespace SnipInsight.Forms.Features.Settings
{
    public interface ISettingsService
    {
        string ImageAnalysisEndPoint { get; }

        string ImageAnalysisAPIKey { get; }

        string EntitySearchEndPoint { get; }

        string EntitySearchAPIKey { get; }

        string ImageSearchAPIKey { get; }

        string TextRecognitionEndPoint { get; }

        string TextRecognitionAPIKey { get; }

        string TranslatorEndPoint { get; }

        string TranslatorAPIKey { get; }

        string LuisEndPoint { get; }

        string LuisAPPID { get; }

        string LuisAPIKey { get; }
    }
}
