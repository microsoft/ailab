using System.Threading.Tasks;
using Refit;

namespace SnipInsight.Forms.Features.Insights.Celebrities
{
    public interface IEntitySearchService
    {
        [Get("/entities/?q={entityName}&mkt=en-us")]
        Task<EntitySearchModel> GetEntities(string entityName, [Header("Ocp-Apim-Subscription-Key")] string apiKey);

        [Get("/news/search/?q={entityName}")]
        Task<RawNewsModel> SearchNews(string entityName, [Header("Ocp-Apim-Subscription-Key")] string apiKey);
    }
}
