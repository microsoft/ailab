using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Refit;
using SnipInsight.Forms.Features.Insights.OCR.Models;

namespace SnipInsight.Forms.Features.Insights.OCR
{
    public interface ITranslatorService
    {
        /*[Headers("Content-Type: application/octet-stream")]
        [Post("/recognizeText?handwriting=true")]
        Task<HttpResponseMessage> RecognizeHandWritingTextRequest([Body]Stream image, [Header("Ocp-Apim-Subscription-Key")] string apiKey);

        [Multipart]
        [Post("/ocr?language=unk&detectOrientation=true")]
        Task<PrintedModel> RecognizePrintedText(Stream image, [Header("Ocp-Apim-Subscription-Key")] string apiKey);
        */

        [Headers("Content-Type: text/xml")]
        [Post("/v2/Http.svc/GetLanguageNames?locale=en")]
        Task<Stream> GetLanguageNames([Body]Stream languagecodes, [Header("Ocp-Apim-Subscription-Key")] string apiKey);

        [Get("/v2/Http.svc/GetLanguagesForTranslate?locale=en")]
        Task<Stream> GetLanguagesForTranslate([Header("Ocp-Apim-Subscription-Key")] string apiKey);

        [Get("/v2/Http.svc/Translate")]
        Task<Stream> Translate(string text, string from, string to, [Header("Ocp-Apim-Subscription-Key")] string apiKey);
    }
}
