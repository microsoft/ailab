using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Refit;
using SnipInsight.Forms.Common;
using SnipInsight.Forms.Features.Products.Models;
using Xamarin.Forms;

namespace SnipInsight.Forms.Features.Insights.Products
{
    public class SimilarProductsViewModel : HideableViewModel, ILoadableWithData
    {
        private bool hasData;
        private ISimilarProductsService productsService;
        private List<ProductModel> products;

        public SimilarProductsViewModel()
        {
            this.productsService = RestService.For<ISimilarProductsService>(Settings.Settings.ImageSearchEndPoint);

            this.UpdateInsightsImageCommand = new Command<string>(this.OnUpdateInsightsImage);

            this.hasData = false;
        }

        public ICommand UpdateInsightsImageCommand { get; }

        public bool HasData
        {
            get => this.hasData;

            internal set => this.SetProperty(ref this.hasData, value);
        }

        public List<ProductModel> Products
        {
            get => this.products;
            private set => this.SetProperty(ref this.products, value);
        }

        public async Task LoadAsync(Stream imageStream, CancellationToken cancelToken)
        {
            if (cancelToken.IsCancellationRequested)
            {
                return;
            }

            var similarProducts = await RetryHelper.WrapAsync(
                this.productsService.GetSimilar(imageStream, Settings.Settings.ImageSearchAPIKey));
            if (similarProducts.Container != null)
            {
                this.Products = similarProducts.Container.Products;
                this.HasData = true;
            }
            else
            {
                this.HasData = false;
            }

            this.IsVisible = this.hasData;
        }

        private void OnUpdateInsightsImage(string url)
        {
            Task.Run(async () => await IOHelper.SaveImageAndReturnPathAsync(url)
                     .ContinueWith(task => MessagingCenter.Send(Messenger.Instance, Messages.UpdateInsightsImage, task.Result)));
        }
    }
}
