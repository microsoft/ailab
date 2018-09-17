using System.Threading.Tasks;
using Refit;
using SnipInsight.Forms.Features.Insights.Celebrities;
using SnipInsight.Forms.Tests.Common;
using Xunit;

namespace SnipInsight.Forms.Tests.Features.Insights.Celebrities
{
    public class APIServicesTests
    {
        private const string EntityName = "Toy Story";

        [Fact]
        public async Task ImageAnalysisServiceAnalyzeReturnsResults()
        {
            var imageAnalysisService = RestService.For<IImageAnalysisService>(Constants.ImageAnalysisEndpoint);
            var image = ImageHelper.GetToyStoryImage();

            var result = await imageAnalysisService.Analyze(image, APIKeys.ImageAnalysisAndTextRecognitionAPIKey);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ImageAnalysisServiceGetEntitiesReturnsResults()
        {
            var entitySearchService = RestService.For<IEntitySearchService>(Constants.EntitySearchEndpoint);

            var result = await entitySearchService.GetEntities(EntityName, APIKeys.EntitySearchAPIKey);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ImageAnalysisServiceSearchNewsReturnsResults()
        {
            var entitySearchService = RestService.For<IEntitySearchService>(Constants.EntitySearchEndpoint);

            var result = await entitySearchService.SearchNews(EntityName, APIKeys.ImageSearchAPIKey);

            Assert.NotNull(result);
        }
    }
}
