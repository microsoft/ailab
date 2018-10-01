using System.Threading.Tasks;
using Moq;
using Refit;
using SnipInsight.Forms.Features.Insights;
using SnipInsight.Forms.Features.Settings;
using SnipInsight.Forms.Tests.Common;
using Xunit;

namespace SnipInsight.Forms.Tests.Features.Insights.Celebrities
{
    public class CelebritiesViewModelTests
    {
        [Fact]
        public async Task LoadAsyncFillsCelebritiesInTest()
        {
            var settingsServiceMock = this.MockSettingsService();
            var viewModel = new CelebritiesAndLandmarksViewModel(settingsServiceMock.Object);
            var image = ImageHelper.GetHarryPotterImage();
            var isInconclusive = false;

            try
            {
                await viewModel.LoadAsync(image, CancelationHelper.CancelToken);
            }
            catch (ApiException exception) when ((int)exception.StatusCode == 429)
            {
                isInconclusive = true;
            }

            if (!isInconclusive)
            {
                Assert.NotEmpty(viewModel.CelebritiesViewModel.Celebrities);
            }
        }

        private Mock<ISettingsService> MockSettingsService()
        {
            var settingsServiceMock = new Mock<ISettingsService>();
            settingsServiceMock.SetupGet(service => service.ImageAnalysisEndPoint)
                               .Returns(Constants.ImageAnalysisEndpoint);
            settingsServiceMock.SetupGet(service => service.ImageAnalysisAPIKey)
                               .Returns(APIKeys.ImageAnalysisAndTextRecognitionAPIKey);
            settingsServiceMock.SetupGet(service => service.EntitySearchEndPoint)
                               .Returns(Constants.EntitySearchEndpoint);
            settingsServiceMock.SetupGet(service => service.EntitySearchAPIKey)
                               .Returns(APIKeys.EntitySearchAPIKey);
            settingsServiceMock.SetupGet(service => service.ImageSearchAPIKey)
                               .Returns(APIKeys.ImageSearchAPIKey);

            return settingsServiceMock;
        }
    }
}
