using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using SnipInsight.Forms.Common;
using SnipInsight.Forms.Extensions;
using SnipInsight.Forms.Features.Localization;
using Xamarin.Forms;

namespace SnipInsight.Forms.Features.Library
{
    public class LibraryViewModel : BaseViewModel
    {
        private readonly ILibraryService libraryService;
        private readonly ICommand deleteCommand;
        private readonly ICommand openInsightsCommand;

        private ObservableCollection<IGrouping<string, ImageModel>> images;

        public LibraryViewModel()
        {
            this.libraryService = DependencyService.Get<ILibraryService>();

            this.deleteCommand = new Command<ImageModel>(this.Delete);
            this.openInsightsCommand = new Command<ImageModel>(this.OpenInsights);

            this.Images = new ObservableCollection<IGrouping<string, ImageModel>>();

            MessagingCenter.Subscribe<Messenger>(Messenger.Instance, Messages.RefreshLibrary, _ => this.Initialize());
        }

        public ObservableCollection<IGrouping<string, ImageModel>> Images
        {
            get => this.images;
            private set => this.SetProperty(ref this.images, value);
        }

        public ICommand DeleteCommand => this.deleteCommand;

        public ICommand OpenInsightsCommand => this.openInsightsCommand;

        public void Initialize()
        {
            this.LoadPathsAsync().ConfigureAwait(false);
        }

        private void Delete(ImageModel image)
        {
            Task.Factory.StartNew(
                async () =>
            {
                var result = await Application.Current.MainPage.DisplayActionSheet(
                    Resources.Confirm_Delete, Resources.No, Resources.Yes);

                if (result == Resources.Yes)
                {
                    this.libraryService.Delete(image);
                    await this.LoadPathsAsync();
                }
            },
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        private string GetGroup(string path)
        {
            var now = DateTime.Now;
            var time = this.libraryService.GetLastWriteTime(path);
            string group;

            if (time.Year == now.Year)
            {
                if (time.Month == now.Month && time.Day == now.Day)
                {
                    group = Resources.Today;
                }
                else if (time >= now.AddDays(-7).Date)
                {
                    group = Resources.Last_7_days;
                }
                else
                {
                    group = time.ToString("MMMM");
                }
            }
            else
            {
                group = time.ToString("MMMM yyyy");
            }

            return group;
        }

        private async Task LoadPathsAsync()
        {
            this.IsBusy = true;

            var everyImage = await this.libraryService.LoadAllAsync();
            List<IGrouping<string, ImageModel>> groupedPaths = everyImage
                .GroupBy(image => this.GetGroup(image.Path))
                .OrderByDescending(group => this.libraryService.GetLastWriteTime(group.First().Path))
                .ToList();

            this.Images.Clear();
            this.Images.AddRange(groupedPaths);

            this.IsBusy = false;
        }

        private void OpenInsights(ImageModel image)
        {
            MessagingCenter.Send(Messenger.Instance, Messages.OpenImageFromLibrary, image);
            MessagingCenter.Send(Messenger.Instance, Messages.OpenInsights);
        }
    }
}
