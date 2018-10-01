using System.Threading.Tasks;
using Refit;
using SnipInsight.Forms.Features.Insights.Products;
using SnipInsight.Forms.Tests.Common;
using Xunit;

namespace SnipInsight.Forms.Tests.Features.Products
{
    public class SimilarProductsTests
    {
        private ISimilarProductsService service = RestService.For<ISimilarProductsService>(Constants.ImageSearchEndPoint);

        [Fact]
        public async Task SimilarProductsServiceReturnsResults()
        {
            var image = ImageHelper.GetPlaymobilImage();
            var result = await this.service.GetSimilar(image, Constants.ImageSearchAPIKey);

            Assert.NotEqual(0, result.Container.Products.Count);
        }

        [Fact]
        public async Task SimilarProductsServiceReturnsNull()
        {
            var result = await this.service.GetSimilar(ImageHelper.GetHarryPotterImage(), Constants.ImageSearchAPIKey);
            Assert.Null(result.Container);
        }
    }
}
