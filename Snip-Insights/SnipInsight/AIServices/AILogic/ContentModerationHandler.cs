// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using SnipInsight.AIServices.AIModels;
using SnipInsight.Properties;
using SnipInsight.Util;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace SnipInsight.AIServices.AILogic
{
    /// <summary>
    /// Class to make API request image content moderator cognitive service
    /// Parse API response and find the moderation result
    /// </summary>
    /// <see cref="https://azure.microsoft.com/en-us/services/cognitive-services/content-moderator/"/>
    internal class ContentModerationHandler
    {
        private HttpClient contentModerationClient;
        private Uri URI { get; set; }
        private String key;

        /// <summary>
        /// Number of total retries in case of request failure
        /// </summary>
        private const int RetryCount = 6;

        /// <summary>
        ///  Delay in ms between each retry
        /// </summary>
        private const int RetryDelay = 500;

        /// <summary>
        /// Constructor to initialize API key and client for http request
        /// </summary>
        /// <param name="key">API Key</param>
        /// <param name="client">HTTP client for making the call</param>
        public ContentModerationHandler(string keyFile, HttpClient client=null)
        {
            RetrieveKey(keyFile);
            contentModerationClient = client ?? new HttpClient();
            contentModerationClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
            BuildURI(keyFile);
        }

        /// <summary>
        /// Returns the result of the API call
        /// </summary>
        /// <param name="stream">Captured image used for the call</param>
        /// <param name="client">object of httpclient</param>
        /// <returns>Returns true if content was recognized as inappropriate; false otherwise</returns>
        public bool GetResult(MemoryStream stream)
        {
            try
            {
                var result = Run(stream);
                return ExtractResult(result.Content.ReadAsStringAsync().Result);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Build the URI for the API request
        /// </summary>
        private void BuildURI(string keyFile)
        {
            URI = new UriBuilder
            {
                Scheme = "https",
                Host = UserSettings.GetKey(keyFile + "Endpoint", "westus.api.cognitive.microsoft.com/contentmoderator"),
                Path = "moderate/v1.0/ProcessImage/Evaluate",
                Query = "CacheImage=true"
            }.Uri;
        }

        /// <summary>
        /// Run the call and get the response message and records telemetry event with time to complete api call and status code
        /// </summary>
        /// <param name="stream">Captured image used for the call</param>
        /// <returns>The HttpResponseMessage containing the Json result</returns>
        private HttpResponseMessage Run(MemoryStream stream)
        {
            using (var content = new StreamContent(stream))
            {
                HttpResponseMessage result = null;

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                try
                {
                    // Execute the REST API call.
                    result = RequestAndRetry(() => contentModerationClient.PostAsync(URI, content).Result);
                    result.EnsureSuccessStatusCode();
                }
                finally
                {
                    stopwatch.Stop();
                    string responseStatusCode = Telemetry.PropertyValue.NoResponse;
                    if (result != null)
                    {
                        responseStatusCode = result.StatusCode.ToString();
                    }
                    Telemetry.ApplicationLogger.Instance.SubmitApiCallEvent(Telemetry.EventName.CompleteApiCall,Telemetry.EventName.ContentModerationApi, stopwatch.ElapsedMilliseconds, responseStatusCode);
                }
                content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

                return result;
            }
        }

        /// <summary>
        /// Run the function and retry if it fails
        /// </summary>
        /// <param name="action">Action to retry in case of failure</param>
        /// <returns>The response message containing the result</returns>
        private HttpResponseMessage RequestAndRetry(Func<HttpResponseMessage> action)
        {
            int retriesLeft = RetryCount;
            int delay = RetryDelay;
            HttpResponseMessage response = null;

            while (retriesLeft > 0)
            {
                response = action();
                if ((int)response.StatusCode != 429)
                    break;

                Task.Delay(delay);
                retriesLeft--;
                delay *= 2;
            }

            return response;
        }

        /// <summary>
        /// Converts the string response to deserialized object and get data from it
        /// </summary>
        /// <param name="json">Json from the API call</param>
        /// <returns>Returns true if content was recognized as inappropriate; false otherwise</returns>
        private bool ExtractResult(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ContentModerationModel));
                ContentModerationModel model = (ContentModerationModel)ser.ReadObject(ms);
                return ParseModel(model);
            }
        }

        /// <summary>
        /// Gets the image strength from deserialized json response
        /// </summary>
        /// <param name="model">Deserialized ContentModerationModel object of API result</param>
        /// <returns>Returns true if content was recognized as inappropriate; false otherwise</returns>
        private bool ParseModel(ContentModerationModel model)
        {
            if (model.AdultClassificationScore > (1 - (double)UserSettings.ContentModerationStrength / 100))
                return true;
            if (model.RacyClassificationScore > (1 - (double)UserSettings.ContentModerationStrength / 100))
                return true;
            return false;
        }

        private void RetrieveKey(string keyFile)
        {
            key = UserSettings.GetKey(keyFile);

            if (!string.IsNullOrWhiteSpace(key))
            {
                Debug.WriteLine(keyFile + Resources.API_Key_Not_Found);
            }
        }
    }
}
