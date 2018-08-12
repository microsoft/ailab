using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sketch2Code.AI
{
    public class ObjectDetector : CustomVisionClient
    {

        public ObjectDetector()
            : base(ConfigurationManager.AppSettings["ObjectDetectionTrainingKey"],
                   ConfigurationManager.AppSettings["ObjectDetectionPredictionKey"],
                   ConfigurationManager.AppSettings["ObjectDetectionProjectName"])
        {

        }

        public ObjectDetector(string trainingKey, string predictionKey, string projectName) 
            : base(trainingKey, predictionKey, projectName)
        {

        }

        public async Task<Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models.ImagePrediction> GetDetectedObjects(byte[] image)
        {
            using (var endpoint = new PredictionEndpoint() { ApiKey = this._predictionApiKey })
            {
                using (var ms = new MemoryStream(image))
                {
                    return await endpoint.PredictImageAsync(this._project.Id, ms);
                }
            }
        }

        public async Task<List<String>> GetText(byte[] image)
        {
            var list = new List<String>();
            try
            {
                using (var ms = new MemoryStream(image))
                {
                    var operation = await _visionClient.CreateHandwritingRecognitionOperationAsync(ms);
                    var result = await _visionClient.GetHandwritingRecognitionOperationResultAsync(operation);

                    while (result.Status != Microsoft.ProjectOxford.Vision.Contract.HandwritingRecognitionOperationStatus.Succeeded)
                    {
                        if (result.Status == Microsoft.ProjectOxford.Vision.Contract.HandwritingRecognitionOperationStatus.Failed)
                            return new List<string>(new string[] { "Text prediction failed" });

                        await Task.Delay(Convert.ToInt32(ConfigurationManager.AppSettings["ComputerVisionDelay"]));

                        result = await _visionClient.GetHandwritingRecognitionOperationResultAsync(operation);
                    }
                    list = result.RecognitionResult.Lines.SelectMany(l => l.Words?.Select(w => w.Text)).ToList();
                }
            }
            catch (ClientException ex)
            {
                list.Add($"Text prediction failed: {ex.Error.Message}. Id: {ex.Error.Code}.");
            }
            return list;
        }

        public async Task<HandwritingTextLine[]> GetTextRecognition(byte[] image)
        {
            try
            {
                using (var ms = new MemoryStream(image))
                {
                    var operation = await _visionClient.CreateHandwritingRecognitionOperationAsync(ms);
                    var result = await _visionClient.GetHandwritingRecognitionOperationResultAsync(operation);

                    while (result.Status != Microsoft.ProjectOxford.Vision.Contract.HandwritingRecognitionOperationStatus.Succeeded)
                    {
                        if (result.Status == Microsoft.ProjectOxford.Vision.Contract.HandwritingRecognitionOperationStatus.Failed)
                        {
                            return null;
                        }

                        await Task.Delay(Convert.ToInt32(ConfigurationManager.AppSettings["ComputerVisionDelay"]));
                        result = await _visionClient.GetHandwritingRecognitionOperationResultAsync(operation);
                    }

                    return result.RecognitionResult.Lines;

                }
            }
            catch (ClientException ex)
            {
                return null;
            }
        }
    }
}

