using Sketch2Code.Core;
using Sketch2Code.Core.Services.Interfaces;
using Sketch2Code.Web.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Sketch2Code.Web.Controllers
{
    public class TemplateController : BaseController
    {
        IObjectDetectionAppService _objectDetectionAppService;
        public TemplateController()
        {
            _objectDetectionAppService = new ObjectDetectionAppService();
        }
        // GET: Template
        public async Task<ActionResult> Index()
        {
            var list = (await _objectDetectionAppService.GetPredictionsAsync()).ToList();
            list = list.Take(15).ToList();
            var model = list.ToList().ConvertAll((container) =>
            {
                return new PredictionViewModel(container.Name, container.Properties.LastModified?.DateTime);
            });
            return View(model);
        }
        public ActionResult Uploader()
        {
            return View();
        }
        public ActionResult Prueba()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Uploader(ContentViewModel model)
        {
            byte[] buffer = null;
            HttpPostedFileBase file = Request.Files["ImageData"];
            var url = ConfigurationManager.AppSettings["Sketch2CodeAppFunctionEndPoint"];

            //file or base64 content
            if (file == null) //check if image is submitted as base64
            {
                var image = Request["ImageData"];
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

            using (HttpClient client = new HttpClient())
            {
                var content = new ByteArrayContent(buffer);
                HttpResponseMessage respon = await client.PostAsync(url, content);
                var responseBody = await respon.Content.ReadAsStringAsync();
                var jsonResponse = await Task.Run(() => JsonConvert.DeserializeObject<string>(responseBody));
                string correlationID = jsonResponse;
                return RedirectToAction("GroupBox", "Layout", new { id = correlationID });
            }
        }
        public async Task<ActionResult> Details(string id)
        {
            var model = new PredictionDetailsViewModel();

            model.Detail = await _objectDetectionAppService.GetPredictionAsync(id);

            if (model.Detail?.PredictedObjects != null && model.Detail.PredictedObjects.Any())
            {
                model.Name = id;
            }

            return View(model);
        }
        public async Task<FileContentResult> File(string container, string fileName)
        {
            var fileContent = await this._objectDetectionAppService.GetFile(container, fileName);
            return new FileContentResult(fileContent, "image/png");
        }
        public async Task<FileContentResult> FileJson(string container)
        {

            var fileContent = await this._objectDetectionAppService.GetFile(container, "results.json");
            return new FileContentResult(fileContent, "application/json");
        }
    }
}