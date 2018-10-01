using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Refit;
using SnipInsight.Forms.Common;
using SnipInsight.Forms.Features.Insights.Celebrities;
using SnipInsight.Forms.Features.Insights.Landmarks.Models;
using SnipInsight.Forms.Features.Settings;

namespace SnipInsight.Forms.Features.Insights.Landmarks
{
    public class LandmarksViewModel : HideableViewModel, IHasData
    {
        private readonly ISettingsService settingsService;
        private readonly IEntitySearchService entitySearchService;

        private ObservableCollection<LandmarkModel> landmarks;
        private LandmarkModel landmark;
        private bool hasData;

        public LandmarksViewModel(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
            this.entitySearchService = RestService.For<IEntitySearchService>(this.settingsService.EntitySearchEndPoint);
            this.landmarks = new ObservableCollection<LandmarkModel>();
            this.hasData = false;
        }

        public LandmarkModel Landmark
        {
            get => this.landmark;
            set => this.SetProperty(ref this.landmark, value);
        }

        public ObservableCollection<LandmarkModel> Landmarks
        {
            get => this.landmarks;
            set => this.SetProperty(ref this.landmarks, value);
        }

        public bool HasData
        {
            get => this.hasData;
            set => this.SetProperty(ref this.hasData, value);
        }

        public Task LoadAsync(ImageAnalysisModel imageAnalysisModel, CancellationToken cancelToken)
        {
            this.HasData = false;
            if (cancelToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancelToken);
            }

            return this.GetLandmarksAsync(imageAnalysisModel);
        }

        private async Task GetLandmarksAsync(ImageAnalysisModel imageAnalysisModel)
        {
            this.Landmarks.Clear();
            this.HasData = false;
            this.IsVisible = false;

            foreach (var category in imageAnalysisModel.Categories)
            {
                if (category.Detail == null || category.Detail.Landmarks == null)
                {
                    continue;
                }

                foreach (var eachLandmark in category.Detail.Landmarks)
                {
                    var entitySearchModel = await RetryHelper.WrapAsync(
                        this.entitySearchService.GetEntities(
                            eachLandmark.Name, this.settingsService.EntitySearchAPIKey));

                    if (entitySearchModel.Entities == null)
                    {
                        continue;
                    }

                    var entry = entitySearchModel.Entities.List.FirstOrDefault();

                    if (entry == null)
                    {
                        continue;
                    }

                    var landmarkModel = new LandmarkModel
                    {
                        Name = entry.Name,
                        Image = entry.Image.URL,
                        URL = entry.URL,
                        Description = entry.Description
                    };

                    this.Landmarks.Add(landmarkModel);
                }
            }

            if (this.landmarks.Any())
            {
                this.HasData = true;
                this.IsVisible = true;

                this.Landmark = this.landmarks.First();
            }
        }
    }
}
