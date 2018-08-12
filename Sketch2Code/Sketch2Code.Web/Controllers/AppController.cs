using Sketch2Code.Core;
using Sketch2Code.Core.Services.Interfaces;
using Sketch2Code.Web.Models;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Drawing;
using Sketch2Code.Core.Helpers;
using System.Text;
using System.Linq;
using Sketch2Code.Web.Helpers;
using Sketch2Code.Core.BoxGeometry;
using System.Net;

namespace Sketch2Code.Web.Controllers
{

    public class AppController : BaseController
    {
        IObjectDetectionAppService _objectDetectionAppService;
        public AppController()
        {
            _objectDetectionAppService = new ObjectDetectionAppService();
        }
        public async Task<ActionResult> Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Step2()
        {
            byte[] buffer = null;
            HttpPostedFileBase file = Request.Files["imageData"];
            
            //file or base64 content
            if (file == null) //check if image is submitted as base64
            {
                var image = Request["imageData"];
                if (!String.IsNullOrWhiteSpace(image))
                {
                    buffer = Convert.FromBase64String(image);
                }
                else
                {
                    throw new ApplicationException("No file or base64 content found");
                }
            }
            else
            {
                buffer = new byte[file.ContentLength];
                using (var ms = new MemoryStream())
                {
                    await file.InputStream.CopyToAsync(ms);
                    buffer = ms.ToArray();
                }
            }

            //Optimize size and quality for best performance
            buffer = ImageHelper.OptimizeImage(buffer, 100, 40);

            //generate correlationid
            var correlationID = Guid.NewGuid().ToString();

            //save the file on the storage
            var objectDetector = new ObjectDetectionAppService();
            await objectDetector.SaveResults(buffer, correlationID, "original.png");

            //Jump directly to progress
            return View("step3", new ContentViewModel { CorrelationId= correlationID });
        }

        [HttpPost]
        public async Task<ActionResult> Step2FromSample()
        {
            string Url =  Request.Url.Scheme + "://" + Request.Url.Authority + "/Content/img/sampledesigns/" + Request.Url.Segments[Request.Url.Segments.Length - 1] + ".jpg";

            byte[] buffer;
            var webRequest = WebRequest.Create(Url);

            using (var response = webRequest.GetResponse())
            using (var content = response.GetResponseStream())
            {
                var img = Image.FromStream(content);
                MemoryStream ms = new MemoryStream();
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                buffer = ms.ToArray();
            }
            
            //Optimize size and quality for best performance
            buffer = ImageHelper.OptimizeImage(buffer, 100, 40);

            //generate correlationid
            var correlationID = Guid.NewGuid().ToString();

            //save the file on the storage
            var objectDetector = new ObjectDetectionAppService();
            await objectDetector.SaveResults(buffer, correlationID, "original.png");

            //Jump directly to progress
            return View("step3", new ContentViewModel { CorrelationId = correlationID });
        }

        [HttpPost]
        public async Task<ActionResult> Step3()
        {
            var image = Request["imageData"];
            if (String.IsNullOrWhiteSpace(image))
                throw new ApplicationException("No file or base64 content found");

            var model = new ContentViewModel { ImageData = image };
            return View(model);
        }
        [HttpPost]        
        public async Task<JsonResult> Upload()
        {
            //var image = Request["imageData"];
            string correlationId = Request["correlationId"];

            //return await callBackEndFunction(image);
            return await callBackEnd(correlationId);
        }

        private async Task<JsonResult> callBackEndFunction(string image)
        {
            var url = ConfigurationManager.AppSettings["Sketch2CodeAppFunctionEndPoint"];

            if (String.IsNullOrWhiteSpace(image))
                throw new ApplicationException("No file or base64 content found");

            using (HttpClient client = new HttpClient())
            {
                var content = new ByteArrayContent(Convert.FromBase64String(image));
                HttpResponseMessage respon = await client.PostAsync(url, content);
                var responseBody = await respon.Content.ReadAsStringAsync();
                var jsonResponse = await Task.Run(() => JsonConvert.DeserializeObject<string>(responseBody));
                string correlationID = jsonResponse;
                return Json(new { id = correlationID });
            }
        }

        public async Task<JsonResult> callBackEnd(string correlationId)
        {
            //Pass byte array to object detector app service 
            var objectDetector = new ObjectDetectionAppService();
            byte[] content = await objectDetector.GetFile(correlationId, "original.png");
            var html = "";

            var result = await objectDetector.GetPredictionAsync(content);

            if (result != null)
            {
                byte[] jsonContent = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result));
                await saveResults(correlationId, objectDetector, content, result, jsonContent);

                var groupBox = await objectDetector.CreateGroupBoxAsync(result);
                html = this.RenderViewToString2<GroupBox>("/Views/Layout/RawGroupBox2.cshtml", groupBox);
                await saveResults2(correlationId, objectDetector, groupBox, html);
            }

            return Json(new { id = correlationId, generatedHtml=html });
        }

        private static async Task saveResults(string correlationId, ObjectDetectionAppService objectDetector, byte[] content, System.Collections.Generic.IList<Core.Entities.PredictedObject> result, byte[] jsonContent)
        {
            //TODO: Skip this once we generate online slices
            await objectDetector.SaveResults(result, correlationId);

            await objectDetector.SaveResults(content.DrawRectangle(result), correlationId, "predicted.png");

            //TODO: Store this as Text
            await objectDetector.SaveResults(jsonContent, correlationId, "results.json");
            
        }

        private static async Task saveResults2(string correlationId, ObjectDetectionAppService objectDetector, GroupBox groupBox, string html)
        {
            //TODO: Store this as Text
            await objectDetector.SaveResults(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(groupBox)), correlationId, "groups.json");

            await objectDetector.SaveHtmlResults(html, correlationId, "generated.html");
        }

        public async Task<ActionResult> Step4(string id)
        {
            byte[] data = await _objectDetectionAppService.GetFile(id, "generated.html");
            string html = Encoding.UTF8.GetString(data);
            var model = new GeneratedHtmlModel { Html = html, FolderId = id };
            return View(model);
        }

        public async Task<ActionResult> Step5(string id)
        {
            byte[] data = await _objectDetectionAppService.GetFile(id, "generated.html");
            string html = Encoding.UTF8.GetString(data);

            //If generated HTML image is done pass it to the view
            //Check if the file is there in the storage
            var model = new GeneratedHtmlModel { Html = html, FolderId=id };
            return View(model);
        }

        public async Task<ActionResult> TakeSnapshot()
        {
            return View();
        }
        public async Task<ActionResult> Details(string id)
        {

                var model = new PredictionDetailsViewModel();

                model.Detail = await _objectDetectionAppService.GetPredictionAsync(id);
                
                if (model.Detail?.PredictedObjects != null)
                {
                    model.Name = id;
                }

                return View("Details", model);

        }
        public async Task<FileContentResult> File(string container, string fileName)
        {
            var fileContent = await this._objectDetectionAppService.GetFile(container, fileName);
            return new FileContentResult(fileContent, "image/png");
        }

        public async Task<ActionResult> Loader1()
        {
            return View();
        }
        public async Task<ActionResult> Loader2()
        {
            return View();
        }

        
        [HttpPost]
        public async Task<JsonResult> SaveFile()
        {
            var data = Request["imgBase64"];
            var folderId = Request["folderId"];

            byte[] content = Convert.FromBase64String(data);

            var objectDetector = new ObjectDetectionAppService();
            await objectDetector.SaveResults(content, folderId, "html.png");

            return Json(new { folderId = folderId });
        }
    }
}