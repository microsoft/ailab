using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Refit;
using SnipInsight.Forms.Common;
using SnipInsight.Forms.Features.Insights.Celebrities;
using SnipInsight.Forms.Features.Insights.Landmarks;
using SnipInsight.Forms.Features.Settings;
using Xamarin.Forms;

namespace SnipInsight.Forms.Features.Insights
{
    public class CelebritiesAndLandmarksViewModel : BaseViewModel, ILoadableWithData
    {
        private readonly ISettingsService settingsService;
        private readonly IImageAnalysisService imageAnalysisService;
        private readonly CelebritiesViewModel celebritiesViewModel;
        private readonly LandmarksViewModel landmarksViewModel;

        public CelebritiesAndLandmarksViewModel(ISettingsService settingsService = null)
        {
            this.settingsService = settingsService ?? DependencyService.Get<ISettingsService>();
            this.imageAnalysisService = RestService.For<IImageAnalysisService>(
                this.settingsService.ImageAnalysisEndPoint);

            this.celebritiesViewModel = new CelebritiesViewModel(this.settingsService);
            this.landmarksViewModel = new LandmarksViewModel(this.settingsService);
        }

        public CelebritiesViewModel CelebritiesViewModel => this.celebritiesViewModel;

        public LandmarksViewModel LandmarksViewModel => this.landmarksViewModel;

        public bool HasData => this.celebritiesViewModel.HasData || this.landmarksViewModel.HasData;

        public async Task LoadAsync(Stream image, CancellationToken cancelToken)
        {
            var imageAnalysisModel = await this.imageAnalysisService.Analyze(
                image,
                this.settingsService.ImageAnalysisAPIKey);

            Task[] tasks = new[]
            {
                this.celebritiesViewModel.LoadAsync(imageAnalysisModel, cancelToken),
                this.landmarksViewModel.LoadAsync(imageAnalysisModel, cancelToken)
            };

            await Task.WhenAll(tasks);

            this.OnPropertyChanged(nameof(this.HasData));
        }
    }
}
