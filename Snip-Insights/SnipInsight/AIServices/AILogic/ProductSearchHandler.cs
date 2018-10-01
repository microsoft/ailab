// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using SnipInsight.AIServices.AIModels;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using SnipInsight.Util;

namespace SnipInsight.AIServices.AILogic
{
    /// <summary>
    /// Handler for the product search API
    /// Takes a json API model in param and returns a list of products
    /// </summary>
    class ProductSearchHandler : CloudService<ProductSearchModelContainer>
    {
        /// <summary>
        /// Expected number of results for highest confidence on response
        /// </summary>
        private const double ConfidenceThreshold = 10.0;

        /// <summary>
        /// Normalize confidence of result to avoid dominance in suggested search
        /// </summary>
        private const double ConfidenceOffset = 0.1;

        /// <summary>
        /// Initalizes handler with correct endpoint
        /// </summary>
        public ProductSearchHandler(string keyFile) : base(keyFile)
        {
            Host = UserSettings.GetKey(keyFile + "Endpoint", "api.cognitive.microsoft.com/bing/v7.0");
            Endpoint = "images/details";
            RequestParams = "modules=SimilarProducts";

            BuildURI();
        }

        protected override string GetDefaultKey()
        {
            return APIKeys.ImageSearchAPIKey;
        }

        /// <summary>
        /// Run the HTTP call to get a list of similar products
        /// </summary>
        /// <param name="stream">Captured Image</param>
        /// <returns>The HTTP response for the call</returns>
        protected override async Task<HttpResponseMessage> Run(MemoryStream stream)
        {
            var strContent = new StreamContent(stream);
            strContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { FileName = "AnyNameWorks" };

            var content = new MultipartFormDataContent();
            content.Add(strContent);

            // Execute the REST API call.
            var result = await RequestAndRetry(() => CloudServiceClient.PostAsync(URI, content));
            result.EnsureSuccessStatusCode();
            return result;
        }
    }
}