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


namespace Sketch2Code.Tests
{
    [TestClass]
    public class ApplicationTest
    {
        IObjectDetectionAppService _detectorService;

        public ApplicationTest()
        {
            _detectorService = new ObjectDetectionAppService();
        }

        [TestMethod]
        public void DownloadFile()
        {
            var im = _detectorService.GetFile("7dda94c5-edd8-470d-ad02-ebe9ba7334ba", "slices/slice_dropdown_0.png").Result;
            File.WriteAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), DateTime.Now.Ticks.ToString() + ".jpg"), im);
            Assert.IsNotNull(im);
        }
    }
}
