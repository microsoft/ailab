// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using SnipInsight.AIServices.AIModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using SnipInsight.Util;

namespace SnipInsight.AIServices.AILogic
{
    /// <summary>
    /// Handwritten OCR call
    /// </summary>
    class HandWrittenTextHandler : CloudService<HandWrittenModel>
    {
        /// <summary>
        /// Initalizes handler with correct endpoint
        /// </summary>
        public HandWrittenTextHandler(string keyFile): base(keyFile)
        {
            Host = UserSettings.GetKey(keyFile + "Endpoint", "westus.api.cognitive.microsoft.com/vision/v1.0");
            Endpoint = "recognizeText";
            RequestParams = "handwriting=true";
        }

        protected override string GetDefaultKey()
        {
            return APIKeys.ImageAnalysisAndTextRecognitionAPIKey;
        }

        /// Run the stream asynchronously, return the HttpResonseMessage and records telemetry event with time to complete api call and status code
        /// </summary>
        /// <param name="stream">Captured Image</param>
        /// <returns>ResponseMessage of the API request/call</returns>
        protected override async Task<HttpResponseMessage> Run(MemoryStream stream)
        {
            var result = await base.Run(stream);

            if (!result.IsSuccessStatusCode)
            {
                return null;
            }

            string operationLocation = result.Headers.GetValues("Operation-Location").FirstOrDefault();
            HttpResponseMessage response = await RequestAndRetry(() => CloudServiceClient.GetAsync(operationLocation));

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                // Pass the exception to the next level
                throw e;
            }
            finally{
                stopwatch.Stop();
                string responseStatusCode = Telemetry.PropertyValue.NoResponse;
                if (result != null)
                {
                    responseStatusCode = result.StatusCode.ToString();
                }
                Telemetry.ApplicationLogger.Instance.SubmitApiCallEvent(Telemetry.EventName.CompleteApiCall, Telemetry.EventName.HandWrittenTextApi, stopwatch.ElapsedMilliseconds, responseStatusCode);
            }

            return response;
        }
    }
}
