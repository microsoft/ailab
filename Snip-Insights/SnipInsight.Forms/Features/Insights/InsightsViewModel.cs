using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using SnipInsight.Forms.Common;
using SnipInsight.Forms.Features.Insights.Drawing;
using SnipInsight.Forms.Features.Insights.ImageSearch;
using SnipInsight.Forms.Features.Insights.OCR;
using SnipInsight.Forms.Features.Insights.Products;
using SnipInsight.Forms.Features.Library;
using SnipInsight.Forms.Features.Settings;
using Xamarin.Forms;

namespace SnipInsight.Forms.Features.Insights
{
    public class InsightsViewModel : BaseViewModel
    {
        private DrawingMode drawingMode;
        private string imagePath;
        private CancellationTokenSource cancelationSource;

        public InsightsViewModel()
        {
            this.ChangeCelebrityCommand = new Command<int>(this.OnChangeCelebrity);
            this.LoadImageFromLibraryCommand = new Command<ImageModel>(this.OnLoadImageFromLibrary);

            this.CanvasTappedCommand = new Command(this.OnCanvasTapped);
            this.CopyToClipboardCommand = new Command(this.OnCopyToClipBoard, this.CanExecuteImageCommands);
            this.PencilCommand = new Command(this.OnPencil, this.CanExecuteImageCommands);
            this.HighLightCommand = new Command(this.OnHighLight, this.CanExecuteImageCommands);
            this.RefreshCommand = new Command(this.OnRefresh, this.CanExecuteImageCommands);
            this.SetEraserModeCommand = new Command(this.OnSetEraserMode, this.CanExecuteImageCommands);

            this.ImageSearchViewModel = new ImageSearchViewModel();
            this.CelebritiesAndLandmarksViewModel = new CelebritiesAndLandmarksViewModel();
            this.SimilarProductsViewModel = new SimilarProductsViewModel();
            this.OCRViewModel = new OCRViewModel();
            this.ColorPickerViewModel = new ColorPickerViewModel();
            this.DrawingMode = DrawingMode.None;

            MessagingCenter.Subscribe<Messenger, string>(
                this, Messages.UpdateInsightsImage, (_, imagePath) => this.UpdateImagePath(imagePath));

            MessagingCenter.Subscribe<Messenger>(this, Messages.SettingsUpdated, _ => this.RaiseSettingsPropertyChanged());
        }

        public ICommand ChangeCelebrityCommand { get; private set; }

        public ICommand LoadImageFromLibraryCommand { get; private set; }

        public ICommand CopyToClipboardCommand { get; private set; }

        public ICommand CanvasTappedCommand { get; private set; }

        public ICommand PencilCommand { get; private set; }

        public ICommand HighLightCommand { get; private set; }

        public ICommand RefreshCommand { get; private set; }

        public ICommand SetEraserModeCommand { get; private set; }

        public ILoadableWithData CelebritiesAndLandmarksViewModel { get; }

        public ILoadableWithData ImageSearchViewModel { get; }

        public ILoadableWithData SimilarProductsViewModel { get; }

        public ILoadableWithData OCRViewModel { get; }

        public ColorPickerViewModel ColorPickerViewModel { get; }

        public bool EnableAI
        {
            get => Settings.Settings.EnableAI;
            set
            {
                if (Settings.Settings.EnableAI != value)
                {
                    Settings.Settings.EnableAI = value;
                    this.OnPropertyChanged(nameof(this.EnableAI));
                }
            }
        }

        public DrawingMode DrawingMode
        {
            get => this.drawingMode;
            set => this.SetProperty(ref this.drawingMode, value);
        }

        public string ImagePath
        {
            get => this.imagePath;
            private set
            {
                this.SetProperty(ref this.imagePath, value);
                ((Command)this.PencilCommand).ChangeCanExecute();
                ((Command)this.HighLightCommand).ChangeCanExecute();
                ((Command)this.CopyToClipboardCommand).ChangeCanExecute();
                ((Command)this.RefreshCommand).ChangeCanExecute();
                ((Command)this.SetEraserModeCommand).ChangeCanExecute();
            }
        }

        public void UpdateImagePath(string imagePath)
        {
            this.ImagePath = imagePath;

            this.RunInsights();
        }

        private void RaiseSettingsPropertyChanged()
        {
            this.OnPropertyChanged(nameof(this.EnableAI));
        }

        private void OnChangeCelebrity(int index)
        {
            var viewModel = this.CelebritiesAndLandmarksViewModel as CelebritiesAndLandmarksViewModel;
            viewModel.CelebritiesViewModel.ChangeCelebrity(index);
        }

        private void OnLoadImageFromLibrary(ImageModel image)
        {
            if (image == null || string.IsNullOrWhiteSpace(image.Path))
            {
                return;
            }

            this.UpdateImagePath(image.Path);
        }

        private async Task RunAllInsightsAsync()
        {
            this.IsBusy = true;

            byte[] imageData = this.ReadImage();

            if (imageData != null)
            {
                if (this.cancelationSource != null)
                {
                    this.cancelationSource.Cancel();
                }

                this.cancelationSource = new CancellationTokenSource();

                TaskHelper.Run(this.OCRViewModel.LoadAsync(new MemoryStream(imageData), this.cancelationSource.Token));

                var imageSearchTask = this.ImageSearchViewModel.LoadAsync(new MemoryStream(imageData), this.cancelationSource.Token);
                var celebritiesTask = this.CelebritiesAndLandmarksViewModel.LoadAsync(new MemoryStream(imageData), this.cancelationSource.Token);
                var similarProductTask = this.SimilarProductsViewModel.LoadAsync(new MemoryStream(imageData), this.cancelationSource.Token);
                var runAllTask = Task.WhenAll(celebritiesTask, imageSearchTask, similarProductTask);
                var result = await this.InvokeWithErrorHandling(runAllTask);

                if (!result)
                {
                    MessagingCenter.Send<Messenger>(Messenger.Instance, Messages.InsightsNoResults);
                }
                else
                {
                    MessagingCenter.Send<Messenger>(Messenger.Instance, Messages.InsightsResults);
                }
            }

            this.IsBusy = false;
        }

        // TODO move to a Service or similar
        private byte[] ReadImage()
        {
            byte[] imageData = null;

            imageData = File.ReadAllBytes(this.ImagePath);

            return imageData;
        }

        private void OnCopyToClipBoard(object obj)
        {
            MessagingCenter.Send(Messenger.Instance, Messages.CopyTextToClipboard);
        }

        private void OnCanvasTapped(object parameter)
        {
            this.ColorPickerViewModel.IsVisible = false;
        }

        private void OnPencil(object parameter)
        {
            if (this.DrawingMode == DrawingMode.Drawing)
            {
                this.ColorPickerViewModel.IsVisible = !this.ColorPickerViewModel.IsVisible;
                this.ColorPickerViewModel.UndoHighLightMode();
            }
            else
            {
                this.DrawingMode = DrawingMode.Drawing;
            }
        }

        private void OnHighLight(object parameter)
        {
            this.DrawingMode = DrawingMode.Drawing;
            this.ColorPickerViewModel.SetMarkerMode();
        }

        private void OnRefresh(object obj)
        {
            this.RunInsights();
        }

        private void OnSetEraserMode(object obj)
        {
            this.DrawingMode = DrawingMode.Erasing;
        }

        private void RunInsights()
        {
            if (Settings.Settings.EnableAI)
            {
                this.RunAllInsightsAsync().ConfigureAwait(false);
            }
        }

        private bool CanExecuteImageCommands(object arg)
        {
            return !string.IsNullOrWhiteSpace(this.ImagePath);
        }
    }
}
