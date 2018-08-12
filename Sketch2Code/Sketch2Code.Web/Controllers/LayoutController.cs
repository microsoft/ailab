using Sketch2Code.Core;
using Sketch2Code.Core.Helpers;
using Sketch2Code.Core.Services.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Sketch2Code.Web.Helpers;
using Sketch2Code.Core.BoxGeometry;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;

namespace Sketch2Code.Web.Controllers
{
    public class LayoutController : BaseController
    {
        IObjectDetectionAppService _objectDetectionAppService;
        public LayoutController()
        {
            _objectDetectionAppService = new ObjectDetectionAppService();
        }

        public async Task<ActionResult> Details(string id)
        {
            var objects = await _objectDetectionAppService.GetPredictionAsync(id);
            var model = objects.PredictedObjects.Where(p => p.ClassName!=Controls.Table).ToList();
            return View(model);
        }

        public async Task<ActionResult> GroupBox(string id)
        {
            var objects = await _objectDetectionAppService.GetPredictionAsync(id);

            return View(objects.GroupBox.FirstOrDefault());
        }

        public async Task<FileResult> Result(string id)
        {
            var objects = await _objectDetectionAppService.GetPredictionAsync(id);
            var result = this.RenderViewToString<GroupBox>("/Views/Layout/RawGroupBox.cshtml", objects.GroupBox.FirstOrDefault());
            var buffer = Encoding.UTF8.GetBytes(result);

            return File(buffer, System.Net.Mime.MediaTypeNames.Text.Html, "result.html");
        }

        public async Task<ActionResult> RawGroupBox(string id)
        {
            var objects = await _objectDetectionAppService.GetPredictionAsync(id);
            return View(objects.GroupBox.FirstOrDefault());
        }
    }
}
