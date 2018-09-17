using System.Linq;
using System.Threading.Tasks;
using Moq;
using Refit;
using SnipInsight.Forms.Features.Insights.OCR;
using SnipInsight.Forms.Features.Settings;
using SnipInsight.Forms.Tests.Common;
using Xunit;

namespace SnipInsight.Forms.Tests.Features.Products
{
    public class TextTests
    {
        private readonly Mock<ISettingsService> settingsServiceMock = MockSettingsService();
        private IOCRTextService service = RestService.For<IOCRTextService>(Constants.TextRecognitionEndPoint);

        [Fact]
        public async Task GetHandWritenTakeLong()
        {
            var ocrViewModel = new OCRViewModel(this.settingsServiceMock.Object);
            var image = ImageHelper.GetHandWritenImage2();

            var handWritingResult = await ocrViewModel.GetHandWrittenDataTask(image, CancelationHelper.CancelToken);

            Assert.Equal("Running", handWritingResult.Status);
        }

        [Fact]
        public async Task GetTextCorrect()
        {
            var image = ImageHelper.GetHandWritenImage();

            var result = await Forms.Common.RetryHelper.WrapAsync(
                this.service.RecognizePrintedText(image, APIKeys.ImageAnalysisAndTextRecognitionAPIKey));

            Assert.True(result.Regions[0].Lines[0].Words.Any());
        }

        [Fact]
        public async Task LanguagesLoadSucces()
        {
            var ocrViewModel = new OCRViewModel(this.settingsServiceMock.Object);

            await ocrViewModel.LoadLanguages(CancelationHelper.CancelToken);

            Assert.True(ocrViewModel.LanguageCodesAndTitles.Any());
        }

        private static Mock<ISettingsService> MockSettingsService()
        {
            var settingsServiceMock = new Mock<ISettingsService>();
            settingsServiceMock.SetupGet(service => service.TextRecognitionEndPoint)
                               .Returns(Constants.TextRecognitionEndPoint);
            settingsServiceMock.SetupGet(service => service.TextRecognitionAPIKey)
                               .Returns(Constants.ImageAnalysisAndTextRecognitionAPIKey);
            settingsServiceMock.SetupGet(service => service.TranslatorEndPoint)
                               .Returns(Constants.TranslatorEndPoint);
            settingsServiceMock.SetupGet(service => service.TranslatorAPIKey)
                               .Returns(Constants.TranslatorAPIKey);
            settingsServiceMock.SetupGet(service => service.LuisEndPoint)
                               .Returns(Constants.LUISEndPoint);
            settingsServiceMock.SetupGet(service => service.LuisAPPID)
                               .Returns(Constants.LUISAppId);
            settingsServiceMock.SetupGet(service => service.LuisAPIKey)
                               .Returns(Constants.LuisAPIKey);

            return settingsServiceMock;
        }
    }
}
