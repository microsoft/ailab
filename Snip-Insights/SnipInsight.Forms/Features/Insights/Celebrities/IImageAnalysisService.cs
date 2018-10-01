using System.IO;
using System.Threading.Tasks;
using Refit;

namespace SnipInsight.Forms.Features.Insights.Celebrities
{
    public interface IImageAnalysisService
    {
        [Headers("Content-Type: application/octet-stream")]
        [Post("/analyze?visualFeatures=Tags,Description&language=en&details=Celebrities,Landmarks")]
        Task<ImageAnalysisModel> Analyze([Body] Stream image, [Header("Ocp-Apim-Subscription-Key")] string apiKey);
    }
}
