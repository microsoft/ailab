// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SnipInsight.AIServices.AILogic;
using SnipInsight.AIServices.AIModels;
using SnipInsight.ClipboardUtils;
using SnipInsight.EmailController;
using SnipInsight.Properties;
using SnipInsight.Util;
using SnipInsight.ViewModels;
using SnipInsight.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace SnipInsight.AIServices.AIViewModels
{
    /// <summary>
    /// ViewModel for the OCR, simple string
    /// The result is a string containing the text
    /// </summary>
    public class OCRViewModel : ViewModelBase
    {
        private HandWrittenTextHandler writtenHandler;
        private PrintedTextHandler printedHandler;
        private TranslationHandler translationHandler;
        private LUISInsights luisInsights;
        private string text;
        private string printedResult = string.Empty;
        private const string unknownLanguage = "unk";

        public bool DatesAvailable { get; private set; }
        public bool EmailAvailable { get; private set; }

        /// <summary>
        /// List of DateTime object to be used to create calendar event.
        /// </summary>
        private List<DateTime> calendarDates = new List<DateTime>();

        /// <summary>
        /// List of email addresses to be used to send new email.
        /// </summary>
        private List<string> toAddress = new List<string>();

        /// <summary>
        /// Constructor for the OCR View Model, initialize the commands
        /// </summary>
        public OCRViewModel()
        {
            writtenHandler = new HandWrittenTextHandler("TextRecognition");
            printedHandler = new PrintedTextHandler("TextRecognition");
            translationHandler = new TranslationHandler("Translator");
            luisInsights = new LUISInsights("LUISKey");

            ToggleTranslatorCommand = new RelayCommand(ToggleTranslatorCommandExecute);
            OpenCalendarEventCommand = new RelayCommand(OpenCalendarEventCommandExecute);
            SendNewEmailCommand = new RelayCommand(SendNewEmailCommandExecute);
            CopyTextCommand = new RelayCommand(CopyTextCommandExecute);
        }

        public async Task LoadText(MemoryStream writtenStream, MemoryStream printedStream)
        {
            TranslatedText = String.Empty;
            ToLanguage = String.Empty;

            IsVisible = Visibility.Collapsed;

            Task<HandWrittenModel> writtenTask = writtenHandler.GetResult(writtenStream);
            Task<PrintedModel> printedTask = printedHandler.GetResult(printedStream);

            await Task.WhenAll(writtenTask, printedTask);
            CompareResult(writtenTask.Result, printedTask.Result);

            CultureInfo ci = CultureInfo.InstalledUICulture;
            ToLanguage = ci.TwoLetterISOLanguageName;

            if (!string.IsNullOrEmpty(printedResult))
            {
                await GetLUISInsights(printedResult);
            }

            CalendarButtonVisibility = DatesAvailable ? Visibility.Visible : Visibility.Collapsed;
            EmailButtonVisibility = EmailAvailable ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Detected language marked in the from language dropdown
        /// </summary>
        public string DetectedLanguage { get; set; }

        private string fromLanguage;

        /// <summary>
        /// selected item in the from language dropdown
        /// </summary>
        public string FromLanguage
        {
            get
            {
                return fromLanguage;
            }
            set
            {
                fromLanguage = value;
                RaisePropertyChanged();
                RunTranslation();
            }
        }

        private string toLanguage;

        /// <summary>
        /// selected item in the to language dropdown
        /// </summary>
        public string ToLanguage
        {
            get
            {
                return toLanguage;
            }
            set
            {
                toLanguage = value;
                RaisePropertyChanged();
                RunTranslation();
            }
        }

        private string translatedText;

        /// <summary>
        /// selected item in the to language dropdown
        /// </summary>
        public string TranslatedText
        {
            get
            {
                return translatedText;
            }
            set
            {
                translatedText = value;
                RaisePropertyChanged();
            }
        }

        private bool translatorEnable = false;

        /// <summary>
        /// Enable/disable toggle translator button
        /// </summary>
        public bool TranslatorEnable
        {
            get
            {
                return translatorEnable;
            }
            set
            {
                translatorEnable = value;
                RaisePropertyChanged();
            }
        }

        private Visibility translationVisibility = Visibility.Collapsed;

        /// <summary>
        /// Make the translation textblock visible
        /// </summary>
        public Visibility TranslationVisibility
        {
            get { return translationVisibility; }
            set
            {
                translationVisibility = value;
                RaisePropertyChanged();
            }
        }

        private Visibility ocrSingleBoxVisibility = Visibility.Visible;

        /// <summary>
        /// Visibility for the Single OCR Text Block
        /// </summary>
        public Visibility OCRSingleBoxVisibility
        {
            get { return ocrSingleBoxVisibility; }
            set
            {
                ocrSingleBoxVisibility = value;
                RaisePropertyChanged();
            }
        }

        private Visibility calendarButtonVisibility = Visibility.Collapsed;

        /// <summary>
        /// Visibility for the "Create New Event" button
        /// </summary>
        public Visibility CalendarButtonVisibility
        {
            get { return calendarButtonVisibility; }
            set
            {
                calendarButtonVisibility = value;
                RaisePropertyChanged();
            }
        }

        private Visibility emailButtonVisibility = Visibility.Collapsed;

        /// <summary>
        /// Visibility for the "Open New Email" button
        /// </summary>
        public Visibility EmailButtonVisibility
        {
            get { return emailButtonVisibility; }
            set
            {
                emailButtonVisibility = value;
                RaisePropertyChanged();
            }
        }

        private Dictionary<string, string> languages;

        /// <summary>
        /// List of language for the translator dropdown
        /// </summary>
        public Dictionary<string, string> Languages
        {
            get { return languages; }
            set
            {
                languages = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Toggle the visibility of the translate
        /// </summary>
        public RelayCommand ToggleTranslatorCommand { get; set; }

        /// <summary>
        /// Toggle "Create Calendar Event" button depending on
        /// whether date and time is available.
        /// </summary>
        public RelayCommand OpenCalendarEventCommand { get; set; }

        /// <summary>
        /// Toggle "Send New Email" button depending on
        /// whether email address is available.
        /// </summary>
        public RelayCommand SendNewEmailCommand { get; set; }

        /// <summary>
        /// Toggle the visibility of copy
        /// </summary>
        public RelayCommand CopyTextCommand { get; set; }

        /// <summary>
        /// Execute the translate command
        /// </summary>
        private void ToggleTranslatorCommandExecute()
        {
            int change = (int)TranslationVisibility;
            TranslationVisibility = OCRSingleBoxVisibility;
            OCRSingleBoxVisibility = (Visibility)change;
        }

        /// <summary>
        /// Text to be displayed on screen
        /// </summary>
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Opens a calendar event by creating and opening an ics file.
        /// </summary>
        private void OpenCalendarEventCommandExecute()
        {
            CreateCalendarEvent();
        }

        /// <summary>
        /// Sends new email by creating and opening an eml file.
        /// </summary>
        private void SendNewEmailCommandExecute()
        {
            OpenNewEmail();
        }

        /// <summary>
        /// Copy text in OCR result into clipboard
        /// </summary>
        private void CopyTextCommandExecute()
        {
            bool copiedText = TranslationVisibility == Visibility.Visible ? ClipboardManager.Copy(TranslatedText) : ClipboardManager.Copy(Text);
            ToastControl toast = new ToastControl(copiedText ? Resources.Message_CopiedToClipboard : Resources.Message_CopyToClipboardFailed);
            toast.ShowInMainWindow();
        }

        /// <summary>
        /// Compare and set the better result of the text recognition to view model
        /// </summary>
        /// <param name="handWrittenText">Data extracted from the API call</param>
        /// <param name="printedText">Data extracted from the API call</param>
        private void CompareResult(HandWrittenModel writtenModel, PrintedModel printedModel)
        {
            StringBuilder result = new StringBuilder();

            try
            {
                foreach (LineWritten line in writtenModel.RecognitionResult.Lines)
                {
                    result.Append(line.Text + "\n");
                }
            }
            catch (Exception)
            { }

            string writtenResult = result.ToString() ?? string.Empty;

            result.Clear();

            try
            {
                foreach (Region region in printedModel.Regions)
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
            catch (Exception)
            { }

            printedResult = result.ToString() ?? string.Empty;

            if (printedModel.Language != unknownLanguage && printedResult.Length > writtenResult.Length)
            {
                Text = printedResult;
                DetectedLanguage = printedModel.Language;
            }
            else
            {
                Text = writtenResult;
                DetectedLanguage = "en";
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                Text = "No Result";
            }
            else
            {
                TranslatorEnable = translationHandler.TranslatorEnable;

                if (TranslatorEnable)
                {
                    Languages = PopulateLanguageMenus();
                }

                IsVisible = Visibility.Visible;
            }

            ServiceLocator.Current.GetInstance<AIPanelViewModel>().OCRCommand.RaiseCanExecuteChanged();
        }

        private Visibility _isVisible = Visibility.Collapsed;

        public Visibility IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;

                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Populate the languages combobox and set the detected language
        /// </summary>
        /// <returns>Dictionary of language names and codes</returns>
        public Dictionary<string, string> PopulateLanguageMenus()
        {
            Dictionary<string, string> languages = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> language in translationHandler.LanguageCodesAndTitles)
            {
                //Reversed to fetch detected language code.
                languages.Add(language.Value, language.Key);
            }

            if (!string.IsNullOrEmpty(DetectedLanguage) && !DetectedLanguage.Equals("unk"))
            {
                languages[DetectedLanguage] = languages[DetectedLanguage] + " (Detected)";
            }

            FromLanguage = DetectedLanguage;
            return languages;
        }

        /// <summary>
        /// Checks if the result from OCR text recognition contains valid entities,
        /// such as date/time, email address using LUIS.
        /// </summary>
        /// <param name="ocrResult"> string of text returned from OCR text recognition </param>
        private async Task GetLUISInsights(string ocrText)
        {
            List<string> datesFound = new List<string>();

            CalendarButtonVisibility = Visibility.Collapsed;
            EmailButtonVisibility = Visibility.Collapsed;
            calendarDates.Clear();
            toAddress.Clear();

            try
            {
                var luisResult = await luisInsights.GetResult(ocrText);

                for (int i = 0; i < luisResult.Entities.Count; i++)
                {
                    LUISModel.Entity entity = luisResult.Entities[i];
                    LUISModel.Entity nextEntity = i + 1 < luisResult.Entities.Count ? luisResult.Entities[i + 1] : null;
                    var validYear = @"^[1-9]\d*$";

                    switch (entity.Type)
                    {
                        case "builtin.email":
                            toAddress.Add(entity.TheEntity);
                            EmailAvailable = true;
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

                DatesAvailable = ExtractDateTime(datesFound);
                EmailAvailable = (toAddress.Count > 0) ? true : false;
            }
            catch (WebException e)
            {
                Debug.WriteLine(e.Message);
                DatesAvailable = false;
                EmailAvailable = false;
            }
        }

        /// <summary>
        /// Temporarily store information in EML file (email in file form).
        /// Open the EML file with the default mail client to send out a new email.
        /// </summary>
        /// <returns> whether a new email is opened successfully </returns>
        private void OpenNewEmail()
        {
            try
            {
                using (MailMessage message = new MailMessage())
                {
                    // create a new email message with toAddress.
                    message.From = new MailAddress("youremail@example.com");
                    for (int i = 0; i < toAddress.Count; i++)
                    {
                        message.To.Add(new MailAddress(toAddress[i]));
                    }
                    message.IsBodyHtml = true;

                    // Get the temp EML file.
                    string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".eml");
                    while (File.Exists(tempFile))
                    {
                        tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".eml");
                    }

                    // save the message to disk and open it.
                    if (EmailManager.SaveMessage(message, tempFile))
                    {
                        Process.Start(tempFile);
                    }
                    else if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
            }
            catch (Exception ex)
            {
                Diagnostics.LogException(ex);
            }
        }

        /// <summary>
        /// Convert collected information - date and time - into a ics file.
        /// </summary>
        public void CreateCalendarEvent()
        {
            if (calendarDates.Count == 0)
            {
                return;
            }

            DateTime dateStart = calendarDates[0];
            DateTime dateEnd = calendarDates.Count >= 2 ? calendarDates[1] : calendarDates[0];

            string DateFormat = "yyyyMMddTHHmmssZ";
            string now = DateTime.Now.ToUniversalTime().ToString(DateFormat);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("PRODID:-//Compnay Inc//Product Application//EN");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("METHOD:PUBLISH");
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine("DTSTART:" + dateStart.ToUniversalTime().ToString(DateFormat));
            sb.AppendLine("DTEND:" + dateEnd.ToUniversalTime().ToString(DateFormat));
            sb.AppendLine("DTSTAMP:" + now);
            sb.AppendLine("UID:" + Guid.NewGuid());
            sb.AppendLine("CREATED:" + now);
            sb.AppendLine("LAST-MODIFIED:" + now);
            sb.AppendLine("SEQUENCE:0");
            sb.AppendLine("STATUS:CONFIRMED");
            sb.AppendLine("SUMMARY:" + Resources.Create_New_Event);
            sb.AppendLine("TRANSP:OPAQUE");
            sb.AppendLine("END:VEVENT");
            sb.AppendLine("END:VCALENDAR");

            Task result = CreateAndOpenICSFile(sb);
            result.ContinueWith((t) =>
            {
                if (t.Exception != null)
                {
                    Debug.WriteLine(t.Exception.Message);
                }
            });
        }

        /// <summary>
        /// Attempts to extract date and time from the given list of string
        /// </summary>
        /// <param name="ocrDates"> list of date and time recognized using OCR </param>
        /// <returns> true if at least one date is parsed successfully, false otherwise </returns>
        private bool ExtractDateTime(List<string> ocrDates)
        {
            bool extracted = false;

            foreach (string date in ocrDates)
            {
                if (DateTime.TryParse(date, out DateTime extractedDate))
                {
                    extracted = true;
                    calendarDates.Add(extractedDate);
                }
            }

            return extracted;
        }

        /// <summary>
        /// Create the ics file, save it to user-defined file path,
        /// and open the ics file.
        /// </summary>
        /// <param name="sb"> contains all information needed to create ics file </param>
        private async Task CreateAndOpenICSFile(StringBuilder sb)
        {
            // Generate an unique ics file name using current date and time
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string fileName = string.Format(
                "event_{0} {1}",
                DateTimeOffset.Now.ToString(Resources.Culture.DateTimeFormat.ShortDatePattern),
                DateTimeOffset.Now.ToString(Resources.Culture.DateTimeFormat.ShortTimePattern));

            // Replace invalid characters in filename with "-"
            var invalidChars = string.Format(@"[{0}]+", (new string(Path.GetInvalidFileNameChars())));
            fileName = Regex.Replace(fileName, invalidChars, "-");
            string icsFileName = string.Concat(folderPath, "\\", fileName, ".ics");

            File.WriteAllText(@icsFileName, sb.ToString());
            using (Stream file = File.OpenWrite(icsFileName))
            {
                using (StreamWriter ics = new StreamWriter(file))
                {
                    await ics.WriteAsync(sb.ToString());
                }
            }

            Process myProcess = new Process();
            myProcess.StartInfo.FileName = @icsFileName;
            myProcess.StartInfo.Arguments = "";
            myProcess.Exited += new EventHandler((sender, e) => {
                // TODO: monitor process of exit code (for telemetry)
            });
            myProcess.Start();
        }

        /// <summary>
        /// Run the translation with the update languages
        /// </summary>
        private void RunTranslation()
        {
            if (!string.IsNullOrEmpty(toLanguage) && !fromLanguage.Equals(toLanguage))
            {
                Task.Run(async () => TranslatedText = await translationHandler.GetResult(Text,
                    FromLanguage,
                    ToLanguage));
            }
            else
            {
                TranslatedText = Text;
            }
        }
    }
}
