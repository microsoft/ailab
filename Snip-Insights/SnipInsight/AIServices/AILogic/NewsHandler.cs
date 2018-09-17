// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using SnipInsight.AIServices.AIModels;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SnipInsight.Util;

namespace SnipInsight.AIServices.AILogic
{
    class NewsHandler: CloudService<RawNewsModel>
    {
        /// <summary>
        ///  Constructor that get implemented based on CloudService
        ///  when passing in the api key and http client
        /// </summary>
        /// <param name="key"> API key for Bing News Search </param>
        public NewsHandler(string keyFile): base(keyFile)
        {
            Host = UserSettings.GetKey(keyFile + "Endpoint", "api.cognitive.microsoft.com/bing/v7.0");
            Endpoint = "news/search";
        }

        /// <summary>
        /// Returns the result of the API call
        /// </summary>
        /// <param name="string">Captured name used for the call</param>
        /// <returns>Data extracted from successful API response, default in case of failure</returns>
        public async Task<RawNewsModel> GetResult(string entityName)
        {
            try
            {
                var result = await Run(entityName);
                return ExtractResult(await result.Content.ReadAsStringAsync());
            }
            catch (WebException e)
            {
                Debug.WriteLine(e.Message + URI);
                return default(RawNewsModel);
            }
        }

        protected override string GetDefaultKey()
        {
            return APIKeys.ImageSearchAPIKey;
        }

        /// <summary>
        /// Run the API call and get the response message and records telemetry event with time to complete api call and status code
        /// </summary>
        /// <param name="entityName">Name of entity to be used for the call</param>
        /// <returns>The HttpResponseMessage containing the Json result</returns>
        protected async Task<HttpResponseMessage> Run(string entityName)
        {
            // Construct the uri to perform Bing Entity Search
            RequestParams = "q=" + System.Net.WebUtility.UrlEncode(entityName);
            BuildURI();

            HttpResponseMessage response = null;
            // Execute the REST API GET call asynchronously and
            // await for non-blocking API call to Bing Entity Search
            response = await RequestAndRetry(() => CloudServiceClient.GetAsync(URI));
            response.EnsureSuccessStatusCode();

            return response;
        }
    }
}
