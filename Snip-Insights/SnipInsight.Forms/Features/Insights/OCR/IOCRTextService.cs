using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Refit;
using SnipInsight.Forms.Features.Insights.OCR.Models;

namespace SnipInsight.Forms.Features.Insights.OCR
{
    public interface IOCRTextService
    {
        [Headers("Content-Type: application/octet-stream")]
        [Post("/recognizeText?handwriting=true")]
        Task<HttpResponseMessage> RecognizeHandWritingTextRequest([Body]Stream image, [Header("Ocp-Apim-Subscription-Key")] string apiKey);

        [Multipart]
        [Post("/ocr?language=unk&detectOrientation=true")]
        Task<PrintedModel> RecognizePrintedText(Stream image, [Header("Ocp-Apim-Subscription-Key")] string apiKey);
    }
}
