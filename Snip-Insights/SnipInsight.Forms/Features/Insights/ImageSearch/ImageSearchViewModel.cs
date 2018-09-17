using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Refit;
using SnipInsight.Forms.Common;
using Xamarin.Forms;

namespace SnipInsight.Forms.Features.Insights.ImageSearch
{
    public class ImageSearchViewModel : HideableViewModel, ILoadableWithData
    {
        private readonly IImageSearchService imageSearchService;
        private IEnumerable<ImageSearchModel> images;
        private bool hasData;

        public ImageSearchViewModel()
        {
            this.imageSearchService = RestService.For<IImageSearchService>(Settings.Settings.EntitySearchEndPoint);

            this.UpdateInsightsImageCommand = new Command<string>(this.OnUpdateInsightsImage);
        }

        public ICommand UpdateInsightsImageCommand { get; }

        public IEnumerable<ImageSearchModel> Images
        {
            get => this.images;

            set => this.SetProperty(ref this.images, value);
        }

        public bool HasData
        {
            get => this.hasData;
            set => this.SetProperty(ref this.hasData, value);
        }

        public async Task LoadAsync(Stream image, CancellationToken cancelToken)
        {
            this.HasData = false;

            if (cancelToken.IsCancellationRequested)
            {
                return;
            }

            ImageSearchModelContainer results = await RetryHelper.WrapAsync(
                this.imageSearchService.GetSimilarImagesAsync(image, Settings.Settings.ImageSearchAPIKey));

            if (results.Container != null)
            {
                this.Images = results.Container?.Images;
                this.HasData = this.Images.Any();
            }
            else
            {
                this.HasData = false;
            }

            this.IsVisible = this.HasData;
        }

        private void OnUpdateInsightsImage(string url)
        {
            Task.Run(async () => await IOHelper.SaveImageAndReturnPathAsync(url)
                     .ContinueWith(task => MessagingCenter.Send(Messenger.Instance, Messages.UpdateInsightsImage, task.Result)));
        }
    }
}
