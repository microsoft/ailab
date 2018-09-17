// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using SnipInsight.Util;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SnipInsight.AIServices
{
    /// <summary>
    /// Logic for the API calls of the Azure CloudServices
    /// </summary>
    /// <typeparam name="TModel">Model used to represent the data from the API response</typeparam>
    /// <typeparam name="TResult">Result type that should be displayed to the screen</typeparam>
    public class CloudService<TModel>
    {
        /// <summary>
        /// Client used for the request
        /// </summary>
        protected HttpClient CloudServiceClient;

        protected String Host { get; set; }

        protected String Endpoint { get; set; }

        protected String RequestParams { get; set; }

        /// <summary>
        /// URI of the request
        /// </summary>
        protected Uri URI { get; set; }

        protected String Key { get; set; }

        /// <summary>
        /// API Identifier
        /// </summary>
        protected String ServiceName { get; set; }

        /// <summary>
        /// Indicates the confidence in the result, ranges from 0.0 to 1.0
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Generic base class for all the AI azure calss
        /// </summary>
        /// <param name="key">API Key</param>
        /// <param name="client">HTTP client for making the call</param>
        protected CloudService(string keyFile, HttpClient client = null)
        {
            RetrieveKey(keyFile);

            Host = "westcentralus.api.cognitive.microsoft.com";
            CloudServiceClient = client ?? new HttpClient();
            CloudServiceClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Key);
        }

        /// <summary>
        /// Number of total retries in case of request failure
        /// </summary>
        private const int _retryCount = 6;

        /// <summary>
        ///  Delay in ms between each retry
        /// </summary>
        private const int _retryDelay = 500;

        /// <summary>
        /// Returns the result of the API call
        /// </summary>
        /// <param name="stream">Captured image used for the call</param>
        /// <param name="client">object of httpclient to be used for the request</param>
        /// <returns>Data extracted from successful API response, default in case of failure</returns>
        public async Task<TModel> GetResult(MemoryStream stream)
        {
            Confidence = 0.0;

            try
            {
                var result = await Run(stream);
                return ExtractResult(await result.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return default(TModel);
            }
        }

        /// <summary>
        /// Run the call and get the response message
        /// </summary>
        /// <param name="stream">Captured image used for the call</param>
        /// <returns>The HttpResponseMessage containing the Json result</returns>
        protected virtual async Task<HttpResponseMessage> Run(MemoryStream stream)
        {
            if (URI == null)
            {
                this.BuildURI();
            }

            using (var content = new StreamContent(stream))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // Execute the REST API call.
                var result = await RequestAndRetry(() => CloudServiceClient.PostAsync(URI, content));

                return result;
            }
        }

        /// <summary>
        /// Run the function and retry if it fails
        /// </summary>
        /// <param name="action">Action to retry in case of failure</param>
        /// <returns>The response message containing the result</returns>
        protected async Task<HttpResponseMessage> RequestAndRetry(Func<Task<HttpResponseMessage>> action)
        {
            int retriesLeft = _retryCount;
            int delay = _retryDelay;
            HttpResponseMessage response = null;

            while (retriesLeft > 0)
            {
                response = await action();
                if ((int)response.StatusCode != 429)
                    break;

                await Task.Delay(delay);
                retriesLeft--;
                delay *= 2;
            }

            return response;
        }

        /// <summary>
        /// Extract useful information from the json result in http response message.
        /// </summary>
        /// <param name="json">Json from the API call</param>
        /// <returns>Result in a format specific to each call</returns>
        protected virtual TModel ExtractResult(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return default(TModel);
            }

            return JsonConvert.DeserializeObject<TModel>(json);
        }

        /// <summary>
        /// Build the URI for the API request
        /// </summary>
        protected void BuildURI()
        {
            URI = new UriBuilder
            {
                Scheme = "https",
                Host = this.Host,
                Path = this.Endpoint,
                Query = this.RequestParams
            }.Uri;
        }

        private void RetrieveKey(string keyFile)
        {
            Key = UserSettings.GetKey(keyFile);

            if (string.IsNullOrWhiteSpace(Key))
            {
                Key = GetDefaultKey();
                Debug.WriteLine(keyFile+" API Key not configured in setting using default: "+ Key); ;
            }
        }

        protected virtual string GetDefaultKey()
        {
            return string.Empty;
        }
    }
}
