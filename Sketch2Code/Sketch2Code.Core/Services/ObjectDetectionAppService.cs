using Sketch2Code.AI;
using Sketch2Code.Core.Entities;
using Sketch2Code.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sketch2Code.Core.Helpers;
using System.IO;
using System.Drawing;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Sketch2Code.Core.BoxGeometry;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using System.Diagnostics;
using Microsoft.ProjectOxford.Vision.Contract;

namespace Sketch2Code.Core
{
    public class ObjectDetectionAppService : IObjectDetectionAppService
    {
        ObjectDetector _detectorClient;
        CloudBlobClient _cloudBlobClient;
        int predicted_index = 0;

        public ObjectDetectionAppService(ObjectDetector detectorClient, ILogger logger)
        {
            _detectorClient = detectorClient;
            _detectorClient.Initialize();
            var account = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureWebJobsStorage"]);
            _cloudBlobClient = account.CreateCloudBlobClient();
        }
        public ObjectDetectionAppService() : this(new ObjectDetector(),
            new LoggerFactory().CreateLogger<ObjectDetectionAppService>())
        {
        }
        public ObjectDetectionAppService(ILogger logger) : this(new ObjectDetector(), logger)
        {
        }
        public async Task<IList<PredictedObject>> GetPredictionAsync(byte[] data)
        {
           var list = new List<PredictedObject>();

           Image image = buildAndVerifyImage(data);

           ImagePrediction prediction = await _detectorClient.GetDetectedObjects(data);

           HandwritingTextLine[] result = await this._detectorClient.GetTextRecognition(data);

            if (prediction != null)
            {
                if (prediction.Predictions != null && prediction.Predictions.Any())
                {
                    var predictions = prediction.Predictions.ToList();

                    removePredictionsUnderProbabilityThreshold(predictions);

                    list = predictions.ConvertAll<PredictedObject>((p) =>
                    {
                        return buildPredictedObject(p, image, data);
                    });

                    removeUnusableImages(list);

                    if (result != null)
                    { 
                        foreach (var predictedObject in list)
                        {
                            assignPredictedText2(predictedObject, result);
                        }
                    }
                }
            }

            return list;
        }

        private static void removePredictionsUnderProbabilityThreshold(List<PredictionModel> predictions)
        {
            var Probability = Convert.ToInt32(ConfigurationManager.AppSettings["Probability"]);

            predictions.RemoveAll(p => p.Probability < (Probability / 100D));
        }

        private async Task assignPredictedText(PredictedObject predictedObject)
        {
            //Exclude images from non predictable classes
            var nonPredictableClasses = new string[] { Controls.Image, Controls.Paragraph, Controls.TextBox };

            bool okHeight = predictedObject.BoundingBox.Height <= 3200 && predictedObject.BoundingBox.Height >= 40;
            bool okWidth = predictedObject.BoundingBox.Width <= 3200 && predictedObject.BoundingBox.Width >= 40;
            bool predictable = !nonPredictableClasses.Contains(predictedObject.ClassName);

            if (okHeight && okWidth && predictable)
            {
                var result = await this._detectorClient.GetText(predictedObject.SlicedImage);
                predictedObject.Text = result;
                await Task.Delay(Convert.ToInt32(ConfigurationManager.AppSettings["ComputerVisionDelay"]));
            }
        }

        private void assignPredictedText2(PredictedObject predictedObject, HandwritingTextLine[] textLines)
        {
            predictedObject.Text = new List<string>();

            for (int i = 0; i < textLines.Length; i++)
            {
                //if areas are 100% overlapping assign every textline
                Overlap ovl = new Overlap();
                Entities.BoundingBox b = new Entities.BoundingBox();


                int min_x = textLines[i].Polygon.Points.Min(p => p.X);
                int min_y = textLines[i].Polygon.Points.Min(p => p.Y);

                int max_x = textLines[i].Polygon.Points.Max(p => p.X);
                int max_y = textLines[i].Polygon.Points.Max(p => p.Y);

                b.Left = min_x;
                b.Top = min_y;
                b.Width = max_x - min_x;
                b.Height = max_y - min_y;

                //If boxes overlaps more than 50% we decide they are the same thing
                if (ovl.OverlapArea(predictedObject.BoundingBox, b) > 0.5)
                {
                    for(int j = 0; j < textLines[i].Words.Length; j++)
                    { 
                        predictedObject.Text.Add(textLines[i].Words[j].Text);
                    }
                }
            }
        }

        private void removeUnusableImages(List<PredictedObject> list)
        {
            //Remove images with size over 4mb
            list.RemoveAll(img => img.SlicedImage.Length >= 4 * 1024 * 1024);

            //Exclude images outside of this range  40x40 - 3200x3200
            list.RemoveAll(p => p.BoundingBox.Height > 3200 || p.BoundingBox.Height < 40);
            list.RemoveAll(p => p.BoundingBox.Width > 3200 || p.BoundingBox.Width < 40);
        }

        private PredictedObject buildPredictedObject(PredictionModel p, Image image, byte[] data)
        {
            PredictedObject predictedObject = new PredictedObject();

            predictedObject.BoundingBox.Top = p.BoundingBox.Top * image.Height;
            predictedObject.BoundingBox.Height = p.BoundingBox.Height * image.Height;
            predictedObject.BoundingBox.Left = p.BoundingBox.Left * image.Width;
            predictedObject.BoundingBox.Width = p.BoundingBox.Width * image.Width;
            predictedObject.BoundingBox.TopNorm = p.BoundingBox.Top;
            predictedObject.BoundingBox.LeftNorm = p.BoundingBox.Left;
            predictedObject.BoundingBox.MaxHeight = image.Height;
            predictedObject.BoundingBox.MaxWidth = image.Width;
            predictedObject.ClassName = p.TagName;
            predictedObject.Probability = p.Probability;

            predictedObject.SlicedImage = data.SliceImage(predictedObject.BoundingBox.Left,
            predictedObject.BoundingBox.Top, predictedObject.BoundingBox.Width, predictedObject.BoundingBox.Height);

            predictedObject.Name = ($"slice_{predictedObject.ClassName}_{predicted_index}");
            predictedObject.FileName = ($"slices/{predictedObject.Name}.png");

            predicted_index++;

            return predictedObject;
        }

        private Image buildAndVerifyImage(byte[] data)
        {
            double imageWidth = 0;
            double imageHeight = 0;
            Image img;

            using (var ms = new MemoryStream(data))
            {
                img = Image.FromStream(ms);

                imageWidth = img.Width;
                imageHeight = img.Height;

                if ((imageWidth == 0) || (imageHeight == 0))
                {
                    throw new InvalidOperationException("Invalid image dimensions");
                }
            }

            return img;
        }

        public async Task SaveResults(IList<PredictedObject> predictedObjects, string id)
        {
            if (_cloudBlobClient == null) throw new InvalidOperationException("blobClient is null");
            var slices_container = $"{id}/slices";

            for (int i = 0; i < predictedObjects.Count; i++)
            {
                PredictedObject result = (PredictedObject)predictedObjects[i];
                await this.SaveResults(result.SlicedImage, slices_container, $"{result.Name}.png");
            }
        }
        public async Task SaveResults(byte[] file, string container, string fileName)
        {
            CloudBlobContainer theContainer = null;

            if (_cloudBlobClient == null) throw new InvalidOperationException("blobClient is null");

            var segments = container.Split(@"/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            var rootContainerPath = segments.First();

            var relativePath = String.Join(@"/", segments.Except(new string[] { rootContainerPath }));

            theContainer = _cloudBlobClient.GetContainerReference($"{rootContainerPath}");

            await theContainer.CreateIfNotExistsAsync();
            var permission = new BlobContainerPermissions();
            permission.PublicAccess = BlobContainerPublicAccessType.Blob;
            await theContainer.SetPermissionsAsync(permission);
            if (relativePath != rootContainerPath)
            {
                fileName = Path.Combine(relativePath, fileName);
            }
            var blob = theContainer.GetBlockBlobReference(fileName);
            await blob.UploadFromByteArrayAsync(file, 0, file.Length);
        }

        public async Task SaveHtmlResults(string html, string container, string fileName)
        {
            CloudBlobContainer theContainer = null;

            if (_cloudBlobClient == null) throw new InvalidOperationException("blobClient is null");

            var segments = container.Split(@"/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            var rootContainerPath = segments.First();

            var relativePath = String.Join(@"/", segments.Except(new string[] { rootContainerPath }));

            theContainer = _cloudBlobClient.GetContainerReference($"{rootContainerPath}");

            await theContainer.CreateIfNotExistsAsync();
            var permission = new BlobContainerPermissions();
            permission.PublicAccess = BlobContainerPublicAccessType.Blob;
            await theContainer.SetPermissionsAsync(permission);
            if (relativePath != rootContainerPath)
            {
                fileName = Path.Combine(relativePath, fileName);
            }
            var blob = theContainer.GetBlockBlobReference(fileName);
            await blob.UploadTextAsync(html);
        }
        public async Task<PredictionDetail> GetPredictionAsync(string folderId)
        {
            if (String.IsNullOrWhiteSpace(folderId))
                throw new ArgumentNullException("folderId");

            var blobContainer = _cloudBlobClient.GetContainerReference(folderId);
            bool exists = await blobContainer.ExistsAsync();
            if (!exists)
                throw new DirectoryNotFoundException($"Container {folderId} does not exist");

            var groupsBlob = blobContainer.GetBlockBlobReference("groups.json");

            var detail = new PredictionDetail();

            detail.OriginalImage = await this.GetFile(folderId, "original.png");
            detail.PredictionImage = await this.GetFile(folderId, "predicted.png");
            detail.PredictedObjects = await this.GetFile<IList<PredictedObject>>(folderId, "results.json");
            var groupBox = await this.GetFile<GroupBox>(folderId, "groups.json");
            detail.GroupBox = new List<GroupBox> { groupBox };

            return detail;
        }
        public async Task<IList<CloudBlobContainer>> GetPredictionsAsync()
        {
            return await Task.Run(() => _cloudBlobClient.ListContainers().Where(l => l.Name != "azure-webjobs-hosts")
                .OrderByDescending(c => c.Properties.LastModified).ToList());
        }
        public async Task<byte[]> GetFile(string container, string file)
        {
            var blobcontainer = _cloudBlobClient.GetContainerReference(container);
            if (!await blobcontainer.ExistsAsync())
            {
                throw new ApplicationException($"container {container} does not exist");
            }
            var blob = blobcontainer.GetBlobReference(file);
            if (!await blob.ExistsAsync())
            {
                throw new ApplicationException($"file {file} does not exist in container {container}");
            }
            using (var ms = new MemoryStream())
            {
                await blob.DownloadToStreamAsync(ms);
                return ms.ToArray();
            }
        }
        public async Task<T> GetFile<T>(string container, string file)
        {
            var data = await this.GetFile(container, file);
            if (data == null) return default(T);
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
        }
        public async Task<GroupBox> CreateGroupBoxAsync(IList<PredictedObject> predictedObjects)
        {
            var result = await Task.Run(() =>
            {
                //Project each prediction into its bounding box
                foreach (var p in predictedObjects)
                    p.BoundingBox.PredictedObject = p;

                var list = predictedObjects.Select(p => p.BoundingBox).ToList();

                //Execute BoxGeometry methods
                BoxGeometry.Geometry g = new BoxGeometry.Geometry();
                g.RemoveOverlapping(list);

                BoxGeometry.GroupBox root = g.BuildGroups(list);
                return root;
            });

            return result;
        }
    }
}
