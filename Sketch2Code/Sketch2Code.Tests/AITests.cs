using Sketch2Code.AI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using Sketch2Code.AI.Management;
using System.Configuration;
using System.Net;
using System.Text;
using System.Collections.Generic;

namespace Sketch2Code.Tests
{
    [TestClass]
    public class AITests
    {
        ObjectDetector _detector;
        ObjectDetector _targetDetector;

        CustomVisionManager _customVisionManager;
        CustomVisionManager _targetDetectorVisionManager;

        public AITests()
        {
            _detector = new ObjectDetector(ConfigurationManager.AppSettings["ObjectDetectionTrainingKey"],
                                           ConfigurationManager.AppSettings["ObjectDetectionPredictionKey"],
                                           ConfigurationManager.AppSettings["ObjectDetectionProjectName"]);

            _targetDetector = new ObjectDetector(ConfigurationManager.AppSettings["TargetObjectDetectionTrainingKey"],
                                           ConfigurationManager.AppSettings["TargetObjectDetectionPredictionKey"],
                                           ConfigurationManager.AppSettings["TargetObjectDetectionProjectName"]);
            try
            {
                _detector.Initialize();
                _targetDetector.Initialize();
            }
            catch
            {
                //Go on
            }
            _customVisionManager = new CustomVisionManager(_detector);
            _targetDetectorVisionManager = new CustomVisionManager(_targetDetector);
        }
        [TestMethod]
        public void ShouldPredictImage()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "full.jpg");
            byte[] image = null;

            using (var fs = File.OpenRead(path))
            {
                using (var ms = new MemoryStream())
                {
                    fs.CopyTo(ms);
                    image = ms.ToArray();
                }
            }

            var result = _detector.GetDetectedObjects(image).Result;
        }
        [TestMethod]
        public void ShouldGetTextFromImage()
        {
            using (var file = File.OpenRead(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "full.jpg")))
            {
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    var text = _detector.GetText(ms.ToArray()).Result;
                    Assert.IsNotNull(text);
                    Assert.IsTrue(text.Any());
                }
            }
        }
        [TestMethod]
        public void ShouldExportResults()
        {
            //_detector.TrainingApi.ExportIterationWithHttpMessagesAsync("", "", "");
            var origin_guid = new Guid("751e5e0b-2447-4aa5-b242-176aca9f94c8");
            var originTrainingApiKey = "52fad44c9d264790a059a3a1ae307eb7";
            var origin_guid_iteration = new Guid("ba3fbeec-171f-4be0-905e-b6c1be9d63dc");
            var destinationTrainingApiKey = "4f1697a47b224d82bef1e93687b52935";

            var originTraining = new TrainingApi() { ApiKey = originTrainingApiKey };
            var destinationTraining = new TrainingApi() { ApiKey = destinationTrainingApiKey };

            var destination_guid = new Guid("19883999-6a28-4bd9-a7b9-6d461e60c263");
            var destination_guid_iteration = new Guid("ba3fbeec-171f-4be0-905e-b6c1be9d63dc");

            var taggedImages = originTraining.GetTaggedImages(origin_guid, origin_guid_iteration);

            var tags = taggedImages.SelectMany(t => t.Tags).Distinct();

            //foreach(var tag in taggedImages)
            //{



            //    //Create tag
            //    destinationTraining.CreateImageTags(destination_guid, new ImageTagCreateBatch {
            //        Tags =
            //    });    



            //    //Create regions
            //}
        }
        #region Management Tests
        [TestMethod]
        public void ShouldCreateProject()
        {
            string projectName = ConfigurationManager.AppSettings["ObjectDetectionProjectName"];
            var project = _customVisionManager.CreateProject(projectName).Result;
            Assert.IsNotNull(project);
        }
        [TestMethod]
        public void ShouldGetProject()
        {
            string projectName = ConfigurationManager.AppSettings["ObjectDetectionProjectName"];
            var project = _customVisionManager.GetProject(projectName).Result;
            Assert.IsNotNull(project);
        }
        [TestMethod]
        public void ShouldGetTagList()
        {
            string projectName = ConfigurationManager.AppSettings["TargetObjectDetectionProjectName"];
            var project = _targetDetectorVisionManager.GetProject(projectName).Result;

            var tags = _targetDetectorVisionManager.GetTags(project).Result;
            Assert.IsNotNull(tags);
        }
        [TestMethod]
        public void ShouldCopyDatasetToTarget()
        {
            string projectName = ConfigurationManager.AppSettings["ObjectDetectionProjectName"];
            string targetProjectName = ConfigurationManager.AppSettings["TargetObjectDetectionProjectName"];

            var project = _customVisionManager.GetProject(projectName).Result;
            var targetProject = _targetDetectorVisionManager.GetProject(targetProjectName).Result;

            var images = _customVisionManager.GetImagesForTraining(project).Result;
            _targetDetectorVisionManager.CreateDataset(targetProject, images).Wait();

            Assert.IsNotNull(images);

        }
        [TestMethod]
        public void ShouldSaveDataset()
        {
            string projectName = ConfigurationManager.AppSettings["ObjectDetectionProjectName"];

            var project = _customVisionManager.GetProject(projectName).Result;

            var images = _customVisionManager.GetImagesForTraining(project).Result;
            var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "sketch2code model");
            var imagePath = System.IO.Path.Combine(path, "images");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (!Directory.Exists(imagePath))
                Directory.CreateDirectory(imagePath);
            var returnImages = new List<Image>();

            foreach (var image in images)
            {
                using (var client = new WebClient())
                {
                    var filePath = System.IO.Path.Combine(imagePath, $"{image.Id.ToString()}.png");
                    File.WriteAllBytes(filePath, client.DownloadData(image.ImageUri));
                    returnImages.Add(new Image(image.Id, image.Created, image.Width, image.Height, null, null, image.Tags, image.Regions));
                }
            }

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(returnImages);
            File.WriteAllBytes(System.IO.Path.Combine(path, "dataset.json"), Encoding.UTF8.GetBytes(json));
            Assert.IsNotNull(images);

        }
        #endregion
    }
}
