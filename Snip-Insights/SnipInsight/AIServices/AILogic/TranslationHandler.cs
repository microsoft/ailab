// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using SnipInsight.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace SnipInsight.AIServices.AILogic
{
    /// <summary>
    /// Handle the translation service
    /// </summary>
    public class TranslationHandler: CloudService<string>
    {
        /// <summary>
        /// Language codes for the translation
        /// </summary>
        private static string[] languageCodes;
        private string detectedLanguage = string.Empty;

        public TranslationHandler(string keyFile): base(keyFile)
        {
            Host = "api.microsofttranslator.com";

            LanguageCodesAndTitles = new SortedDictionary<string, string>(
                Comparer<string>.Create((a, b) => string.Compare(a, b, true))
                );

            Task.Run(async () =>
            {
                try
                {
                    await GetLanguagesForTranslate();
                    await GetLanguageNames();

                    TranslatorEnable = true;
                }
                catch (WebException e)
                {
                    Diagnostics.LogException(e);

                    TranslatorEnable = false;
                }
            });
        }

        /// <summary>
        /// Enable translation if languages loaded
        /// </summary>
        public bool TranslatorEnable { get; set; }

        /// <summary>
        /// Maps the language codes to their full name
        /// </summary>
        public SortedDictionary<string, string> LanguageCodesAndTitles { get; set; }

        /// <summary>
        /// Translate the text and return the translated text and records telemetry
        /// </summary>
        /// <param name="textToTranslate">Text to translate</param>
        /// <returns>Translated text</returns>
        public async Task<string> GetResult(string textToTranslate, string fromLanguage, string toLanguage)
        {
            //TODO: After Refactoring Log results status and api run time
            Telemetry.ApplicationLogger.Instance.SubmitApiCallEvent(Telemetry.EventName.CompleteApiCall, Telemetry.EventName.TranslationApi, -1, "N/A");

            Endpoint = "/v2/Http.svc/Translate";
            RequestParams = string.Format("text={0}&from={1}&to={2}",
                HttpUtility.UrlEncode(textToTranslate),
                fromLanguage,
                toLanguage);

            BuildURI();

            var webRequest = WebRequest.Create(URI);
            webRequest.Headers.Add("Ocp-Apim-Subscription-Key", Key);

            try
            {
                WebResponse response = await webRequest.GetResponseAsync();

                using (StreamReader translatedStream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8")))
                {
                    String stringText = translatedStream.ReadToEnd();
                    int startPos = stringText.IndexOf(">")+1;

                    return stringText.Substring(startPos, stringText.IndexOf("<", startPos) - startPos);
                }
            }
            catch (Exception e) when (e is WebException || e is XmlException )
            {
                Diagnostics.LogException(e);

                return textToTranslate;
            }
        }

        /// <summary>
        /// Get the languages for the translation
        /// </summary>
        private async Task GetLanguagesForTranslate()
        {
            Endpoint = "/v2/Http.svc/GetLanguagesForTranslate";
            RequestParams = "scope=text";
            BuildURI();

            WebRequest request = WebRequest.Create(URI);
            request.Headers.Add("Ocp-Apim-Subscription-Key", this.Key);

            WebResponse response = await request.GetResponseAsync();

            using (Stream stream = response.GetResponseStream())
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(string[]));
                languageCodes = (string[])serializer.ReadObject(stream);
            }
        }

        /// <summary>
        /// Populate the array of string with a list of language codes
        /// </summary>
        private async Task GetLanguageNames()
        {
            Endpoint = "/v2/Http.svc/GetLanguageNames";
            RequestParams = "locale=en";
            BuildURI();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URI);
            request.Headers.Add("Ocp-Apim-Subscription-Key", Key);
            request.ContentType = "text/xml";
            request.Method = "POST";

            DataContractSerializer serializer = new DataContractSerializer(typeof(string[]));
            using (Stream stream = request.GetRequestStream())
            {
                serializer.WriteObject(stream, languageCodes);
            }

            // Read and parse the XML response
            var response = await request.GetResponseAsync();

            string[] languageNames;
            using (Stream stream = response.GetResponseStream())
            {
                languageNames = (string[])serializer.ReadObject(stream);
            }

            // Load the dictionary for the combo box
            for (int i = 0; i < languageNames.Length; i++)
            {
                //Sorted by the language name for diaplay
                LanguageCodesAndTitles.Add(languageNames[i], languageCodes[i]);
            }
        }

        protected override string GetDefaultKey()
        {
            return APIKeys.TranslatorAPIKey;
        }
    }
}
