using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Refit;
using SnipInsight.Forms.Features.Insights.OCR.Models;

namespace SnipInsight.Forms.Features.Insights.OCR
{
    public interface IHandWritingResultService
    {
        [Headers("Content-Type: application/octet-stream")]
        [Get("")]
        Task<HandWrittenModel> GetHandWritingResult([Header("Ocp-Apim-Subscription-Key")] string apiKey);
    }
}
