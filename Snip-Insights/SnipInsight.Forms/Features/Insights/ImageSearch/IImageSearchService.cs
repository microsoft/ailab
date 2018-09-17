using System.IO;
using System.Threading.Tasks;
using Refit;

namespace SnipInsight.Forms.Features.Insights.ImageSearch
{
    public interface IImageSearchService
    {
        [Multipart]
        [Post("/images/details?modules=SimilarImages")]
        Task<ImageSearchModelContainer> GetSimilarImagesAsync(Stream image, [Header("Ocp-Apim-Subscription-Key")] string apiKey);
    }
}
