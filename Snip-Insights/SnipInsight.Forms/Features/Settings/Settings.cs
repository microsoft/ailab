using System;
using System.IO;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using SnipInsight.Forms.Common;
using Xamarin.Forms;

namespace SnipInsight.Forms.Features.Settings
{
    /* Dear Developer,
     *
     * To reset every setting simply call at the beginning: Plugin.Settings.CrossSettings.Current.Clear();
     */
    public static class Settings
    {
        public const string DefaultLuisAPPID = "b0580bc0-6d92-436a-8191-82b5285a5a23";

        public const string DefaultEntitySearchEndPoint = "https://api.cognitive.microsoft.com/bing/v7.0";
        public const string DefaultImageAnalysisEndPoint = "https://westus.api.cognitive.microsoft.com/vision/v1.0";
        public const string DefaultImageSearchEndPoint = "https://api.cognitive.microsoft.com/bing/v7.0";
        public const string DefaultTextRecognitionEndPoint = "https://westus.api.cognitive.microsoft.com/vision/v1.0";
        public const string DefaultTranslatorEndPoint = "https://api.microsofttranslator.com";
        public const string DefaultLuisEndPoint = "https://westus.api.cognitive.microsoft.com/luis/v2.0";

        private static readonly int ScreenCaptureDelaySecondsDefault = 0;

        private static readonly string SnipsPathDefault = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Snips/");

        public static bool AutoOpenEditor
        {
            get => AppSettings.GetValueOrDefault(nameof(AutoOpenEditor), true);
            set
            {
                if (AutoOpenEditor != value)
                {
                    AppSettings.AddOrUpdateValue(nameof(AutoOpenEditor), value);
                    MessagingCenter.Send(Messenger.Instance, Messages.SettingsUpdated);
                }
            }
        }

        public static bool CopyToClipboard
        {
            get => AppSettings.GetValueOrDefault(nameof(CopyToClipboard), true);
            set
            {
                if (CopyToClipboard != value)
                {
                    AppSettings.AddOrUpdateValue(nameof(CopyToClipboard), value);
                    MessagingCenter.Send(Messenger.Instance, Messages.SettingsUpdated);
                }
            }
        }

        public static bool EnableAI
        {
            get => AppSettings.GetValueOrDefault(nameof(EnableAI), true);
            set
            {
                if (EnableAI != value)
                {
                    AppSettings.AddOrUpdateValue(nameof(EnableAI), value);
                    MessagingCenter.Send(Messenger.Instance, Messages.SettingsUpdated);
                }
            }
        }

        public static int ScreenCaptureDelaySeconds
        {
            get => AppSettings.GetValueOrDefault(nameof(ScreenCaptureDelaySeconds), ScreenCaptureDelaySecondsDefault);
            set => AppSettings.AddOrUpdateValue(nameof(ScreenCaptureDelaySeconds), value);
        }

        public static string SnipsPath
        {
            get => AppSettings.GetValueOrDefault(nameof(SnipsPath), SnipsPathDefault);
            set
            {
                if (SnipsPath != value)
                {
                    AppSettings.AddOrUpdateValue(nameof(SnipsPath), value);
                    MessagingCenter.Send(Messenger.Instance, Messages.RefreshLibrary);
                }
            }
        }

        public static string EntitySearchAPIKey
        {
            get => AppSettings.GetValueOrDefault(nameof(EntitySearchAPIKey), APIKeys.EntitySearchAPIKey);
            set => AppSettings.AddOrUpdateValue(nameof(EntitySearchAPIKey), value);
        }

        public static string ImageAnalysisAPIKey
        {
            get => AppSettings.GetValueOrDefault(nameof(ImageAnalysisAPIKey), APIKeys.ImageAnalysisAndTextRecognitionAPIKey);
            set => AppSettings.AddOrUpdateValue(nameof(ImageAnalysisAPIKey), value);
        }

        public static string ImageSearchAPIKey
        {
            get => AppSettings.GetValueOrDefault(nameof(ImageSearchAPIKey), APIKeys.ImageSearchAPIKey);
            set => AppSettings.AddOrUpdateValue(nameof(ImageSearchAPIKey), value);
        }

        public static string TextRecognitionAPIKey
        {
            get => AppSettings.GetValueOrDefault(nameof(TextRecognitionAPIKey), APIKeys.ImageAnalysisAndTextRecognitionAPIKey);
            set => AppSettings.AddOrUpdateValue(nameof(TextRecognitionAPIKey), value);
        }

        public static string TranslatorAPIKey
        {
            get => AppSettings.GetValueOrDefault(nameof(TranslatorAPIKey), APIKeys.TranslatorAPIKey);
            set => AppSettings.AddOrUpdateValue(nameof(TranslatorAPIKey), value);
        }

        public static string LuisAPIKey
        {
            get => AppSettings.GetValueOrDefault(nameof(LuisAPIKey), APIKeys.LuisAPIKey);
            set => AppSettings.AddOrUpdateValue(nameof(LuisAPIKey), value);
        }

        public static string LuisAPPID
        {
            get => AppSettings.GetValueOrDefault(nameof(LuisAPPID), DefaultLuisAPPID);
            set => AppSettings.AddOrUpdateValue(nameof(LuisAPPID), value);
        }

        public static string EntitySearchEndPoint
        {
            get => AppSettings.GetValueOrDefault(
                nameof(EntitySearchEndPoint), DefaultEntitySearchEndPoint);

            set => AppSettings.AddOrUpdateValue(nameof(EntitySearchEndPoint), value);
        }

        public static string ImageAnalysisEndPoint
        {
            get => AppSettings.GetValueOrDefault(
                nameof(ImageAnalysisEndPoint), DefaultImageAnalysisEndPoint);

            set => AppSettings.AddOrUpdateValue(nameof(ImageAnalysisEndPoint), value);
        }

        public static string ImageSearchEndPoint
        {
            get => AppSettings.GetValueOrDefault(
                nameof(ImageSearchEndPoint), DefaultImageSearchEndPoint);

            set => AppSettings.AddOrUpdateValue(nameof(ImageSearchEndPoint), value);
        }

        public static string TextRecognitionEndPoint
        {
            get => AppSettings.GetValueOrDefault(
                nameof(TextRecognitionEndPoint), DefaultTextRecognitionEndPoint);

            set => AppSettings.AddOrUpdateValue(nameof(TextRecognitionEndPoint), value);
        }

        public static string TranslatorEndPoint
        {
            get => AppSettings.GetValueOrDefault(
                nameof(TranslatorEndPoint), DefaultTranslatorEndPoint);

            set => AppSettings.AddOrUpdateValue(nameof(TranslatorEndPoint), value);
        }

        public static string LuisEndPoint
        {
            get => AppSettings.GetValueOrDefault(
                nameof(LuisEndPoint), DefaultLuisEndPoint);

            set => AppSettings.AddOrUpdateValue(nameof(LuisEndPoint), value);
        }

        private static ISettings AppSettings => CrossSettings.Current;

        public static void ClearEntitySearchAPIKey()
        {
            AppSettings.Remove(nameof(EntitySearchAPIKey));
        }

        public static void ClearImageSearchAPIKey()
        {
            AppSettings.Remove(nameof(ImageSearchAPIKey));
        }

        public static void ClearImageAnalysisAPIKey()
        {
            AppSettings.Remove(nameof(ImageAnalysisAPIKey));
        }

        public static void ClearTextRecognitionAPIKey()
        {
            AppSettings.Remove(nameof(TextRecognitionAPIKey));
        }

        public static void ClearTranslatorAPIKey()
        {
            AppSettings.Remove(nameof(TranslatorAPIKey));
        }

        public static void ClearLuisAPIKey()
        {
            AppSettings.Remove(nameof(LuisAPIKey));
        }

        public static void ClearLuisAPPID()
        {
            AppSettings.Remove(nameof(LuisAPPID));
        }

        public static void ClearEntitySearchEndPoint()
        {
            AppSettings.Remove(nameof(EntitySearchEndPoint));
        }

        public static void ClearImageAnalysisEndPoint()
        {
            AppSettings.Remove(nameof(ImageAnalysisEndPoint));
        }

        public static void ClearImageSearchEndPoint()
        {
            AppSettings.Remove(nameof(ImageSearchEndPoint));
        }

        public static void ClearTextRecognitionEndPoint()
        {
            AppSettings.Remove(nameof(TextRecognitionEndPoint));
        }

        public static void ClearTranslatorEndPoint()
        {
            AppSettings.Remove(nameof(TranslatorEndPoint));
        }

        public static void ClearLuisEndPoint()
        {
            AppSettings.Remove(nameof(LuisEndPoint));
        }
    }
}
