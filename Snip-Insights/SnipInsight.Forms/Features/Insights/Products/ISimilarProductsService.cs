using System.IO;
using System.Threading.Tasks;
using Refit;
using SnipInsight.Forms.Features.Products.Models;

namespace SnipInsight.Forms.Features.Insights.Products
{
    public interface ISimilarProductsService
    {
        [Multipart]
        [Post("/images/details?modules=SimilarProducts&mkt=en-us")]
        Task<SimilarProductsModel> GetSimilar(Stream image, [Header("Ocp-Apim-Subscription-Key")] string apiKey);
    }
}