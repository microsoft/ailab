using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Refit;
using SnipInsight.Forms.Common;
using SnipInsight.Forms.Features.Localization;
using SnipInsight.Forms.Features.Settings;

namespace SnipInsight.Forms.Features.Insights.Celebrities
{
    public class CelebritiesViewModel : HideableViewModel, IHasData
    {
        private readonly ISettingsService settingsService;
        private readonly IEntitySearchService entitySearchService;

        private ObservableCollection<CelebrityModel> celebrities;
        private CelebrityModel celebrity;
        private IEnumerable<ImageAnalysisModel.FaceRectangle> faces;
        private bool hasData;
        private string newsTitle;

        public CelebritiesViewModel(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
            this.entitySearchService = RestService.For<IEntitySearchService>(this.settingsService.EntitySearchEndPoint);

            this.celebrities = new ObservableCollection<CelebrityModel>();
        }

        public ObservableCollection<CelebrityModel> Celebrities
        {
            get => this.celebrities;
            set => this.SetProperty(ref this.celebrities, value);
        }

        public CelebrityModel Celebrity
        {
            get => this.celebrity;
            set => this.SetProperty(ref this.celebrity, value);
        }

        public IEnumerable<ImageAnalysisModel.FaceRectangle> Faces
        {
            get => this.faces;
            set => this.SetProperty(ref this.faces, value);
        }

        public bool HasData
        {
            get => this.hasData;
            set => this.SetProperty(ref this.hasData, value);
        }

        public string NewsTitle
        {
            get => this.newsTitle;
            set => this.SetProperty(ref this.newsTitle, value);
        }

        public void ChangeCelebrity(int index)
        {
            this.Celebrity = this.celebrities[index];
            this.NewsTitle = string.Format(Resources.NewsAboutFormat, this.celebrity.Name);
        }

        public Task LoadAsync(ImageAnalysisModel imageAnalysisModel, CancellationToken cancelToken)
        {
            this.HasData = false;
            if (cancelToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancelToken);
            }

            return this.GetCelebritiesAsync(imageAnalysisModel);
        }

        private async Task GetCelebritiesAsync(ImageAnalysisModel imageAnalysisModel)
        {
            this.Celebrities.Clear();
            var facesList = new List<ImageAnalysisModel.FaceRectangle>();
            this.HasData = false;

            foreach (var category in imageAnalysisModel.Categories)
            {
                if (category.Detail == null || category.Detail.Celebrities == null)
                {
                    continue;
                }

                foreach (var eachCelebrity in category.Detail.Celebrities)
                {
                    var entitySearchModel = await RetryHelper.WrapAsync(
                        this.entitySearchService.GetEntities(
                            eachCelebrity.Name, this.settingsService.EntitySearchAPIKey));

                    if (entitySearchModel.Entities == null)
                    {
                        continue;
                    }

                    var entry = entitySearchModel.Entities.List.FirstOrDefault();

                    if (entry == null)
                    {
                        continue;
                    }

                    var celebrityModel = new CelebrityModel
                    {
                        Name = entry.Name,
                        Image = entry.Image.URL,
                        URL = entry.URL,
                        Description = entry.Description
                    };

                    var rawNewsModel = await this.entitySearchService.SearchNews(
                        eachCelebrity.Name, this.settingsService.ImageSearchAPIKey);

                    celebrityModel.News = new ObservableCollection<NewsModel>(rawNewsModel.News);

                    foreach (var newsModel in celebrityModel.News)
                    {
                        newsModel.DatePublished = newsModel.DatePublished.Substring(0, 10);
                        newsModel.Description = newsModel.Description;
                    }

                    this.Celebrities.Add(celebrityModel);
                    facesList.Add(eachCelebrity.FaceRectangle);
                }
            }

            if (this.celebrities.Any())
            {
                this.HasData = true;
                this.ChangeCelebrity(0);
                this.Faces = facesList;
            }
        }
    }
}
