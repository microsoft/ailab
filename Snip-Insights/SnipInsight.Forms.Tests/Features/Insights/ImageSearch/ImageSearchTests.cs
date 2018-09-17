using System.Threading.Tasks;
using Refit;
using SnipInsight.Forms.Features.Insights.ImageSearch;
using SnipInsight.Forms.Tests.Common;
using Xunit;

namespace SnipInsight.Forms.Tests.Features.Insights.ImageSearch
{
    public class ImageSearchTests
    {
        [Fact]
        public async Task ImageSearchReturnsResults()
        {
            var imageAnalysisService = RestService.For<IImageSearchService>(Constants.EntitySearchEndpoint);
            var image = ImageHelper.GetCatImage();

            var result = await imageAnalysisService.GetSimilarImagesAsync(image, Constants.ImageSearchAPIKey);

            Assert.NotNull(result);
        }
    }
}
