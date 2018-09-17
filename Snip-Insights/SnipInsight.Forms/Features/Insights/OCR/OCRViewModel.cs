using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Refit;
using SnipInsight.Forms.Common;
using SnipInsight.Forms.Features.Insights.OCR.Models;
using SnipInsight.Forms.Features.Localization;
using SnipInsight.Forms.Features.Settings;
using Xamarin.Forms;

namespace SnipInsight.Forms.Features.Insights.OCR
{
    public class OCRViewModel : HideableViewModel, ILoadableWithData
    {
        private readonly ISettingsService settingsService;
        private readonly IOCRTextService ocrService;
        private readonly ITranslatorService translatorService;
        private readonly ILUISService luisService;

        private bool hasData;
        private string text;
        private string detectedLanguage;
        private string[] languageCodes;
        private bool translationInterfaceVisibility;
        private List<string> languages;
        private string translatedText;
        private bool datesAvailable;
        private bool emailAvailable;
        private List<DateTime> calendarDates = new List<DateTime>();
        private List<string> toAddress = new List<string>();

        private HandWrittenModel writtenModel;
        private PrintedModel printedModel;

        public OCRViewModel(ISettingsService settingsService = null)
        {
            this.settingsService = settingsService ?? DependencyService.Get<ISettingsService>();

            /*
            var handler = new HttpClientHandler
            {
                UseProxy = true,
                Proxy = new WebProxy("http://localhost:8888", false) // Or whatever Charles is set up as
            };
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri(this.settingsService.TextRecognitionEndPoint)
            };*/

            this.ocrService = RestService.For<IOCRTextService>(this.settingsService.TextRecognitionEndPoint);
            this.translatorService = RestService.For<ITranslatorService>(this.settingsService.TranslatorEndPoint);
            this.luisService = RestService.For<ILUISService>(this.settingsService.LuisEndPoint);
            this.ToggleTranslateVisibilityCommand = new Command(this.OnToggleTranslateVisibility);

            this.TranslationInterfaceVisibility = true;
        }

        public bool HasData
        {
            get => this.hasData;
            set => this.SetProperty(ref this.hasData, value);
        }

        public string Text
        {
            get => this.text;
            private set => this.SetProperty(ref this.text, value);
        }

        public string TranslatedText
        {
            get => this.translatedText;
            private set => this.SetProperty(ref this.translatedText, value);
        }

        public string DetectedLanguage
        {
            get => this.detectedLanguage;
            private set => this.SetProperty(ref this.detectedLanguage, value);
        }

        public Dictionary<string, string> LanguageCodesAndTitles { get; private set; }

        public Dictionary<string, string> LanguagesTitlesAndCodes { get; private set; }

        public List<string> Languages
        {
            get => this.languages;
            set => this.SetProperty(ref this.languages, value);
        }

        public ICommand ToggleTranslateVisibilityCommand { get; }

        public bool TranslationInterfaceVisibility
        {
            get => this.translationInterfaceVisibility;
            set => this.SetProperty(ref this.translationInterfaceVisibility, value);
        }

        public bool DatesAvailable
        {
            get => this.datesAvailable;
            set => this.SetProperty(ref this.datesAvailable, value);
        }

        public bool EmailAvailable
        {
            get => this.emailAvailable;
            set => this.SetProperty(ref this.emailAvailable, value);
        }

        public async Task LoadAsync(Stream image, CancellationToken cancelToken)
        {
            this.HasData = false;
            this.Text = string.Empty;
            this.TranslatedText = string.Empty;
            await this.LoadLanguages(cancelToken);

            var imageCopy = new MemoryStream();
            await image.CopyToAsync(imageCopy);
            imageCopy.Seek(0, SeekOrigin.Begin);
            image.Seek(0, SeekOrigin.Begin);

            this.writtenModel = await this.GetHandWrittenDataTask(image, cancelToken);

            if (cancelToken.IsCancellationRequested)
            {
                return;
            }

            this.printedModel = await this.GetPrintedDataTask(imageCopy);

            this.UpdateTexts(cancelToken);
        }

        public async Task LoadLanguages(CancellationToken cancelToken)
        {
            if (this.LanguageCodesAndTitles == null)
            {
                await this.GetLanguagesForTranslate();

                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }

                await this.GetLanguageNames();
            }
        }

        public async Task<HandWrittenModel> GetHandWrittenDataTask(Stream image, CancellationToken cancelToken)
        {
            var result = await RetryHelper.WrapAsync(
                this.ocrService.RecognizeHandWritingTextRequest(image, this.settingsService.TextRecognitionAPIKey));

            if (cancelToken.IsCancellationRequested)
            {
                return new HandWrittenModel();
            }

            string operationLocation = result.Headers.GetValues("Operation-Location").FirstOrDefault();

            var handWritingResultService = RestService.For<IHandWritingResultService>(operationLocation);

            var handWritingResult = await RetryHelper.WrapLongRetryAsync<HandWrittenModel>(
                handWritingResultService.GetHandWritingResult(this.settingsService.TextRecognitionAPIKey));

            return handWritingResult;
        }

        public async Task TranslateTo(string fromCode, string toCode)
        {
            if (!string.IsNullOrWhiteSpace(fromCode) && !string.IsNullOrWhiteSpace(toCode))
            {
                string from = this.LanguagesTitlesAndCodes[fromCode];
                string to = this.LanguagesTitlesAndCodes[toCode];
                var translatedStream = await RetryHelper.WrapAsync(
                    this.translatorService.Translate(this.Text, from, to, this.settingsService.TranslatorAPIKey));

                DataContractSerializer serializer = new DataContractSerializer(typeof(string));

                this.TranslatedText = (string)serializer.ReadObject(translatedStream);
            }
        }

        private string ProccessWrittenData()
        {
            StringBuilder result = new StringBuilder();
            if (this.writtenModel != null
                && this.writtenModel.RecognitionResult != null)
            {
                foreach (LineWritten line in this.writtenModel.RecognitionResult.Lines)
                {
                    result.Append(line.Text + "\n");
                }
            }

            return result.ToString();
        }

        private string ProccessPrintedData()
        {
            var result = new StringBuilder();

            if (this.printedModel != null)
            {
                foreach (Region region in this.printedModel.Regions)
                {
                    foreach (Line line in region.Lines)
                    {
                        foreach (Word word in line.Words)
                        {
                            result.Append(word.Text + " ");
                        }

                        result.Append("\n");
                    }
                }
            }

            return result.ToString();
        }

        private Task<PrintedModel> GetPrintedDataTask(Stream image)
        {
            return RetryHelper.WrapAsync(this.ocrService.RecognizePrintedText(
                image,
                this.settingsService.TextRecognitionAPIKey));
        }

        private async Task GetLanguagesForTranslate()
        {
            var trasnsService = RestService.For<ITranslatorService>(this.settingsService.TranslatorEndPoint);

            var stream = await this.translatorService.GetLanguagesForTranslate(this.settingsService.TranslatorAPIKey);

            DataContractSerializer serializer = new DataContractSerializer(typeof(string[]));
            this.languageCodes = (string[])serializer.ReadObject(stream);
        }

        private async Task GetLanguageNames()
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(string[]));

            var stream = new MemoryStream();

            serializer.WriteObject(stream, this.languageCodes);
            stream.Seek(0, SeekOrigin.Begin);
            var result = await this.translatorService.GetLanguageNames(stream, this.settingsService.TranslatorAPIKey);

            string[] languageNames;

            languageNames = (string[])serializer.ReadObject(result);

            this.LanguageCodesAndTitles = new Dictionary<string, string>();
            this.LanguagesTitlesAndCodes = new Dictionary<string, string>();
            this.Languages = new List<string>();

            // Load the dictionary for the combo boxes
            for (int i = 0; i < languageNames.Length; i++)
            {
                // Sorted by the language name for display
                this.LanguageCodesAndTitles.Add(this.languageCodes[i], languageNames[i]);
                this.LanguagesTitlesAndCodes.Add(languageNames[i], this.languageCodes[i]);
                this.Languages.Add(languageNames[i]);
            }
        }

        private void GetLanguage(string languageCode)
        {
            if (this.LanguageCodesAndTitles.ContainsKey(languageCode))
            {
                this.DetectedLanguage = this.LanguageCodesAndTitles[languageCode];
            }
            else
            {
                this.DetectedLanguage = Resources.Unknown;
            }
        }

        private void OnToggleTranslateVisibility(object obj)
        {
            this.TranslationInterfaceVisibility = !this.TranslationInterfaceVisibility;
        }

        private void ProcessLUISData(LUISModel luisResult)
        {
            List<string> datesFound = new List<string>();
            this.calendarDates.Clear();
            this.toAddress.Clear();

            if (luisResult != null && luisResult.Entities != null)
            {
                for (int i = 0; i < luisResult.Entities.Count; i++)
                {
                    Entity entity = luisResult.Entities[i];
                    Entity nextEntity = i + 1 < luisResult.Entities.Count ? luisResult.Entities[i + 1] : null;
                    var validYear = @"^[1-9]\d*$";

                    switch (entity.Type)
                    {
                        case "builtin.email":
                            this.toAddress.Add(entity.TheEntity);
                            this.EmailAvailable = true;
                            break;
                        case "builtin.datetimeV2.datetime":
                            datesFound.Add(entity.Resolution.Values[0].TheValue);
                            break;
                        case "builtin.datetimeV2.date":
                            if (nextEntity != null && nextEntity.Type.Equals("builtin.datetimeV2.time"))
                            {
                                datesFound.Add(entity.Resolution.Values[0].TheValue + " " + nextEntity.Resolution.Values[0].TheValue);
                                i++;
                            }
                            else if (nextEntity != null && nextEntity.Type.Equals("builtin.datetimeV2.timerange"))
                            {
                                datesFound.Add(entity.Resolution.Values[0].TheValue + " " + nextEntity.Resolution.Values[0].Start);
                                datesFound.Add(entity.Resolution.Values[0].TheValue + " " + nextEntity.Resolution.Values[0].End);
                                i++;
                            }
                            else if (nextEntity != null && !nextEntity.Type.Equals("builtin.datetimeV2.daterange"))
                            {
                                datesFound.Add(entity.Resolution.Values.Count == 2 ? entity.Resolution.Values[1].TheValue : entity.Resolution.Values[0].TheValue);
                            }

                            break;
                        case "builtin.datetimeV2.daterange":
                            Match isYear = Regex.Match(entity.Resolution.Values[0].Timex, validYear);

                            // Should not create calender event for recognized year
                            if (!isYear.Success)
                            {
                                datesFound.Add(entity.Resolution.Values[0].Start);
                                datesFound.Add(entity.Resolution.Values[0].End);
                            }

                            break;
                        case "builtin.datetimeV2.time":
                            datesFound.Add(entity.Resolution.Values[0].TheValue);
                            break;
                        case "builtin.datetimeV2.timerange":
                            datesFound.Add(entity.Resolution.Values[0].Start);
                            datesFound.Add(entity.Resolution.Values[0].End);
                            break;
                        case "builtin.datetimeV2.datetimerange":
                            datesFound.Add(entity.Resolution.Values[0].Start);
                            datesFound.Add(entity.Resolution.Values[0].End);
                            break;
                    }
                }

                this.DatesAvailable = this.ExtractDateTime(datesFound);
                this.EmailAvailable = (this.toAddress.Count > 0) ? true : false;
            }
            else
            {
                this.DatesAvailable = false;
                this.EmailAvailable = false;
            }
        }

        private bool ExtractDateTime(List<string> ocrDates)
        {
            bool extracted = false;

            foreach (string date in ocrDates)
            {
                if (DateTime.TryParse(date, out DateTime extractedDate))
                {
                    extracted = true;
                    this.calendarDates.Add(extractedDate);
                }
            }

            return extracted;
        }

        private async void UpdateTexts(CancellationToken cancelToken)
        {
            if (cancelToken.IsCancellationRequested)
            {
                return;
            }

            var writtenResult = this.ProccessWrittenData();
            var printedResult = this.ProccessPrintedData();

            if (this.printedModel.Language != Constants.UnknownLanguage && printedResult.Length > writtenResult.Length)
            {
                this.Text = printedResult;
                this.GetLanguage(this.printedModel.Language);
            }
            else
            {
                this.Text = writtenResult;
                this.GetLanguage("en");
            }

            this.HasData = !string.IsNullOrEmpty(this.Text);
            this.IsVisible = this.HasData;
            if (string.IsNullOrWhiteSpace(this.Text))
            {
                this.Text = "No Result";
            }
            else
            {
                var luisData = await RetryHelper.WrapAsync(this.luisService.Proccess(
                    this.settingsService.LuisAPPID,
                    q: this.text,
                    timezoneOffset: "0",
                    verbose: "false",
                    spellCheck: "false",
                    staging: "false",
                    apiKey: this.settingsService.LuisAPIKey));

                this.ProcessLUISData(luisData);
            }

            MessagingCenter.Send(Messenger.Instance, Messages.OCRText, this.Text);
        }
    }
}
