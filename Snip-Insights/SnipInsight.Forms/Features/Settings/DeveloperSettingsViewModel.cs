using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using SnipInsight.Forms.Features.Localization;
using Xamarin.Forms;

namespace SnipInsight.Forms.Features.Settings
{
    public class DeveloperSettingsViewModel
    {
        public DeveloperSettingsViewModel()
        {
            this.SaveCommand = new Command(this.OnSave);
        }

        public ICommand SaveCommand { get; }

        public string EntitySearchAPIKey
        {
            get => GetValueOrEmptyIfDefault(Settings.EntitySearchAPIKey, APIKeys.EntitySearchAPIKey);
            set => SetWithCustomDefaultPolicy(value, Settings.EntitySearchAPIKey, APIKeys.EntitySearchAPIKey, () => Settings.EntitySearchAPIKey = value, () => Settings.ClearEntitySearchAPIKey());
        }

        public string ImageAnalysisAPIKey
        {
            get => GetValueOrEmptyIfDefault(Settings.ImageAnalysisAPIKey, APIKeys.ImageAnalysisAndTextRecognitionAPIKey);
            set => SetWithCustomDefaultPolicy(value, Settings.ImageAnalysisAPIKey, APIKeys.ImageAnalysisAndTextRecognitionAPIKey, () => Settings.ImageAnalysisAPIKey = value, () => Settings.ClearImageAnalysisAPIKey());
        }

        public string ImageSearchAPIKey
        {
            get => GetValueOrEmptyIfDefault(Settings.ImageSearchAPIKey, APIKeys.ImageSearchAPIKey);
            set => SetWithCustomDefaultPolicy(value, Settings.ImageSearchAPIKey, APIKeys.ImageSearchAPIKey, () => Settings.ImageSearchAPIKey = value, () => Settings.ClearImageSearchAPIKey());
        }

        public string TextRecognitionAPIKey
        {
            get => GetValueOrEmptyIfDefault(Settings.TextRecognitionAPIKey, APIKeys.ImageAnalysisAndTextRecognitionAPIKey);
            set => SetWithCustomDefaultPolicy(value, Settings.TextRecognitionAPIKey, APIKeys.ImageAnalysisAndTextRecognitionAPIKey, () => Settings.TextRecognitionAPIKey = value, () => Settings.ClearTextRecognitionAPIKey());
        }

        public string TranslatorAPIKey
        {
            get => GetValueOrEmptyIfDefault(Settings.TranslatorAPIKey, APIKeys.TranslatorAPIKey);
            set => SetWithCustomDefaultPolicy(value, Settings.TranslatorAPIKey, APIKeys.TranslatorAPIKey, () => Settings.TranslatorAPIKey = value, () => Settings.ClearTranslatorAPIKey());
        }

        public string LuisAPIKey
        {
            get => GetValueOrEmptyIfDefault(Settings.LuisAPIKey, APIKeys.LuisAPIKey);
            set => SetWithCustomDefaultPolicy(value, Settings.LuisAPIKey, APIKeys.LuisAPIKey, () => Settings.LuisAPIKey = value, () => Settings.ClearLuisAPIKey());
        }

        public string LuisAPPID
        {
            get => GetValueOrEmptyIfDefault(Settings.LuisAPPID, Settings.DefaultLuisAPPID);
            set => SetWithCustomDefaultPolicy(value, Settings.LuisAPPID, Settings.DefaultLuisAPPID, () => Settings.LuisAPPID = value, () => Settings.ClearLuisAPPID());
        }

        public string EntitySearchEndPoint
        {
            get => GetValueOrEmptyIfDefault(Settings.EntitySearchEndPoint, Settings.DefaultEntitySearchEndPoint);
            set => SetWithCustomDefaultPolicy(value, Settings.EntitySearchEndPoint, Settings.DefaultEntitySearchEndPoint, () => Settings.EntitySearchEndPoint = value, () => Settings.ClearEntitySearchEndPoint());
        }

        public string ImageAnalysisEndPoint
        {
            get => GetValueOrEmptyIfDefault(Settings.ImageAnalysisEndPoint, Settings.DefaultImageAnalysisEndPoint);
            set => SetWithCustomDefaultPolicy(value, Settings.ImageAnalysisEndPoint, Settings.DefaultImageAnalysisEndPoint, () => Settings.ImageAnalysisEndPoint = value, () => Settings.ClearImageAnalysisEndPoint());
        }

        public string ImageSearchEndPoint
        {
            get => GetValueOrEmptyIfDefault(Settings.ImageSearchEndPoint, Settings.DefaultImageSearchEndPoint);
            set => SetWithCustomDefaultPolicy(value, Settings.ImageSearchEndPoint, Settings.DefaultImageSearchEndPoint, () => Settings.ImageSearchEndPoint = value, () => Settings.ClearImageSearchEndPoint());
        }

        public string TextRecognitionEndPoint
        {
            get => GetValueOrEmptyIfDefault(Settings.TextRecognitionEndPoint, Settings.DefaultTextRecognitionEndPoint);
            set => SetWithCustomDefaultPolicy(value, Settings.TextRecognitionEndPoint, Settings.DefaultTextRecognitionEndPoint, () => Settings.TextRecognitionEndPoint = value, () => Settings.ClearTextRecognitionEndPoint());
        }

        public string TranslatorEndPoint
        {
            get => GetValueOrEmptyIfDefault(Settings.TranslatorEndPoint, Settings.DefaultTranslatorEndPoint);
            set => SetWithCustomDefaultPolicy(value, Settings.TranslatorEndPoint, Settings.DefaultTranslatorEndPoint, () => Settings.TranslatorEndPoint = value, () => Settings.ClearTranslatorEndPoint());
        }

        public string LuisEndPoint
        {
            get => GetValueOrEmptyIfDefault(Settings.LuisEndPoint, Settings.DefaultLuisEndPoint);
            set => SetWithCustomDefaultPolicy(value, Settings.LuisEndPoint, Settings.DefaultLuisEndPoint, () => Settings.LuisEndPoint = value, () => Settings.ClearLuisEndPoint());
        }

        private static string GetValueOrEmptyIfDefault(string val, string def)
        {
            if (val == def)
            {
                return string.Empty;
            }

            return val;
        }

        private static void SetWithCustomDefaultPolicy(string newVal, string current, string def, Action setVal, Action clear)
        {
            if (current == newVal)
            {
                return;
            }

            if (string.IsNullOrEmpty(newVal) || newVal == def)
            {
                clear();
            }
            else
            {
                setVal();
            }
        }

        private void OnSave(object obj)
        {
            Task.Factory.StartNew(
                async () =>
                {
                    await Application.Current.MainPage.DisplayAlert(
                        Resources.KeyComboPicker_Updated, Resources.Key_Restart, Resources.Ok);
                },
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
