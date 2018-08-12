using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Sketch2Code.AI;
using Sketch2Code.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Westwind.RazorHosting;
using Sketch2Code.Core.Helpers;
using System.Globalization;
using Microsoft.Extensions.Logging;
using System.Configuration;
using Freezer.Core;
using Microsoft.ApplicationInsights;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.DataContracts;

namespace Sketch2Code.Api
{
    public static class ObjectDetection
    {
        [FunctionName("detection")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            //Get image byte array from request
            var content = await req.Content.ReadAsByteArrayAsync();

            if (content == null)
                return req.CreateResponse(HttpStatusCode.BadRequest, "Request content is empty.");
            else
                log.Info($"found binary content->Size: {content.Length} bytes");

            //Use correlationId to identify each session and results
            var correlationID = req.GetCorrelationId().ToString();

            //Pass byte array to object detector app service 
            var objectDetector = new ObjectDetectionAppService();
            var result = await objectDetector.GetPredictionAsync(content);

            if (result != null)
            {
                await objectDetector.SaveResults(result, correlationID);
                await objectDetector.SaveResults(content, correlationID, "original.png");
                await objectDetector.SaveResults(content.DrawRectangle(result), correlationID, "predicted.png");
                byte[] jsonContent = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result));
                await objectDetector.SaveResults(jsonContent, correlationID, "results.json");
                var groupBox = await objectDetector.CreateGroupBoxAsync(result);
                await objectDetector.SaveResults(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(groupBox)), correlationID, "groups.json");
            }
            
            return req.CreateResponse(HttpStatusCode.OK, correlationID);
        }
    }
}
