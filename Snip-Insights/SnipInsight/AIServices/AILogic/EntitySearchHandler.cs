// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using SnipInsight.AIServices.AIModels;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using SnipInsight.Util;

namespace SnipInsight.AIServices.AILogic
{
    /// <summary>
    /// Backend logic to access API endpoint for Bing Entity Search service
    /// </summary>
    class EntitySearchHandler : CloudService<EntitySearchModel>
    {
        /// <summary>
        ///  Constructor that get implemented based on CloudService
        ///  when passing in the api key and http client
        /// </summary>
        /// <param name="key"> API key for Bing Entity Search </param>
        public EntitySearchHandler(string keyFile): base(keyFile)
        {
            Host = UserSettings.GetKey(keyFile + "Endpoint", "api.cognitive.microsoft.com/bing/v7.0");
            Endpoint = "entities";
        }

        /// <summary>
        /// Returns the result of the API call
        /// </summary>
        /// <param name="stream">Captured image used for the call</param>
        /// <param name="client">object of httpclient to be used for the request</param>
        /// <returns>Data extracted from successful API response, default in case of failure</returns>
        public async Task<EntitySearchModel> GetResult(string entityName)
        {
            try
            {
                var result = await Run(entityName);
                return ExtractResult(await result.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + URI);
                return default(EntitySearchModel);
            }
        }

        protected override string GetDefaultKey()
        {
            return APIKeys.EntitySearchAPIKey;
        }

        /// <summary>
        /// Run the API call and get the response message and records telemetry event with time to complete api call and status code
        /// </summary>
        /// <param name="entityNameStream">Name of entity to be used for the call</param>
        /// <returns>The HttpResponseMessage containing the Json result</returns>
        protected async Task<HttpResponseMessage> Run(string entityName)
        {
            // Construct the uri to perform Bing Entity Search
            RequestParams = "mkt=en-us&q=" + System.Net.WebUtility.UrlEncode(entityName);
            BuildURI();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            HttpResponseMessage response = null;
            try
            {
                // Execute the REST API GET call asynchronously
                response = await RequestAndRetry(() => CloudServiceClient.GetAsync(URI));
                response.EnsureSuccessStatusCode();
            }
            finally
            {
                stopwatch.Stop();
                string responseStatusCode = Telemetry.PropertyValue.NoResponse;
                if (response != null)
                {
                    responseStatusCode = response.StatusCode.ToString();
                }
                Telemetry.ApplicationLogger.Instance.SubmitApiCallEvent(Telemetry.EventName.CompleteApiCall,Telemetry.EventName.EntitySearchApi, stopwatch.ElapsedMilliseconds, responseStatusCode);
            }

            return response;
        }
    }
}
