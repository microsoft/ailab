using Sketch2Code.AI;
using Sketch2Code.Core;
using Sketch2Code.Core.Services.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sketch2Code.Core.Helpers;
using OpenCvSharp;
using Sketch2Code.Core.Entities;
using System.Drawing;
using System.Diagnostics;

namespace Sketch2Code.Tests
{
    [TestClass]
    public class ApplicationTests
    {
        IObjectDetectionAppService _detectorService;
        public ApplicationTests()
        {
            _detectorService = new ObjectDetectionAppService();
        }

        [TestMethod]
        public void ShouldGetPredictedObjects()
        {
            using (var file = File.OpenRead(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "original.png")))
            {
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    var results = _detectorService.GetPredictionAsync(ms.ToArray()).Result;
                    var img = file.DrawRectangle(results);
                    File.WriteAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), DateTime.Now.Ticks.ToString() + ".jpg"), img);
                    //_detectorService.SaveResults(results, Guid.NewGuid().ToString()).Wait();
                    Assert.IsNotNull(results);
                    Assert.IsTrue(results.Any());
                }
            }
        }

        [TestMethod]
        public void ShouldGetPredictedObjectsFromMetadata()
        {
            string folderId = "3c205322-d6a0-4144-98e1-59c248a2850b";
            var result = _detectorService.GetPredictionAsync(folderId).Result;

            var groupBox = _detectorService.CreateGroupBoxAsync(result.PredictedObjects).Result;


            Assert.IsNotNull(result);
            Assert.IsTrue(result.PredictedObjects.Any());
        }

        [TestMethod]
        public void ShouldGetListOfPredictions()
        {
            var list = _detectorService.GetPredictionsAsync().Result;

            Assert.IsNotNull(list);
            Assert.IsTrue(list.Any());
        }

        [TestMethod]
        public void ShouldGetLayoutBorders()
        {
            var maxHeight = 1944;
            var folderID = "e85957c7-b19b-4563-a786-0f6d3b5d6519";
            var prediction = _detectorService.GetPredictionAsync(folderID).Result;
            var original  = _detectorService.GetPredictionAsync(folderID).Result;
            var wthreshold = 0.5D;

            prediction.PredictedObjects.All((p) =>
            {
                p.BoundingBox.Top = maxHeight - 60;
                p.BoundingBox.Height = 1;
                return true;
            });

            var predicted = prediction.PredictedObjects.ToList();
            predicted.RemoveAll(p => p.Name != "slice_Label_7" && p.Name != "slice_TextBox_12");

            var maxWidthBox = predicted.OrderBy(p => p.BoundingBox.Width).Last();
            var objectList = predicted.Select((p) =>
            {
                bool intersects = p.BoundingBox.Rectangle.IntersectsWith(maxWidthBox.BoundingBox.Rectangle);
                var overlappingArea = Rectangle.Intersect(maxWidthBox.BoundingBox.Rectangle, p.BoundingBox.Rectangle);
                return new
                {
                    Object = p,
                    Shape = p.BoundingBox.Rectangle,
                    intersectsWithMaxRectangle = intersects,
                    overlapRegion = overlappingArea,
                    overlappingPercentage = Convert.ToDouble(overlappingArea.Width) / Convert.ToDouble(p.BoundingBox.Width)
                };
            }).ToList();

            //Me zumbo los que no estan contenidos en la region maxima
            objectList.RemoveAll(o => o.overlappingPercentage < wthreshold);
            var filteredPredictions = objectList.Select(o => o.Object).ToList();

            var filtered_fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), $"{folderID}_filtered.png");
            var fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), $"{folderID}.png");


            var image = prediction.OriginalImage.DrawRectangle(predicted);
            var filtered_image = prediction.OriginalImage.DrawRectangle(filteredPredictions);

            File.WriteAllBytes(fileName, image);
            File.WriteAllBytes(filtered_fileName, filtered_image);

            var rectangleImg = Cv2.ImRead(fileName, ImreadModes.Color);
            var filtered_rectangleImg = Cv2.ImRead(fileName, ImreadModes.Color);

            Cv2.NamedWindow("rectangles", WindowMode.FreeRatio);
            Cv2.ImShow("rectangles", rectangleImg);

            Cv2.NamedWindow("filtered_rectangles", WindowMode.FreeRatio);
            Cv2.ImShow("filtered_rectangles", filtered_rectangleImg);

            Cv2.WaitKey(0);
            Cv2.DestroyAllWindows();

        }

     
        
    }

    

}
