using System;
using System.ComponentModel;
using SnipInsight.Forms.Common;
using SnipInsight.Forms.Features.Insights.OCR;
using SnipInsight.Forms.Features.Library;
using Xamarin.Forms;

namespace SnipInsight.Forms.Features.Insights
{
    public partial class InsightsPage : ContentPage
    {
        private readonly IFileChooserService fileChooserService;

        public InsightsPage()
        {
            this.InitializeComponent();

            this.fileChooserService = DependencyService.Get<IFileChooserService>();
        }

        private InsightsViewModel ViewModel => this.BindingContext as InsightsViewModel;

        protected override void OnAppearing()
        {
            base.OnAppearing();

            this.ShowResults();

            MessagingCenter.Subscribe<Messenger, ImageModel>(
                this, Messages.OpenImageFromLibrary, this.OpenImageFromLibrary);

            MessagingCenter.Subscribe<Messenger>(this, Messages.InsightsResults, this.Results);
            MessagingCenter.Subscribe<Messenger>(this, Messages.InsightsNoResults, this.NoResults);

            this.ActivateSection(Section.Information);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (this.ViewModel != null)
            {
                var celebritiesAndLandmarksViewModel = this.ViewModel.CelebritiesAndLandmarksViewModel
                                                           as CelebritiesAndLandmarksViewModel;
                celebritiesAndLandmarksViewModel.PropertyChanged -= this.ManageCelebritiesAndOCRVisibilities;
                celebritiesAndLandmarksViewModel.PropertyChanged += this.ManageCelebritiesAndOCRVisibilities;

                var ocrviewmodel = this.ViewModel.OCRViewModel as OCRViewModel;
                ocrviewmodel.PropertyChanged -= this.ManageCelebritiesAndOCRVisibilities;
                ocrviewmodel.PropertyChanged += this.ManageCelebritiesAndOCRVisibilities;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            MessagingCenter.Unsubscribe<Messenger, ImageModel>(this, Messages.OpenImageFromLibrary);

            MessagingCenter.Unsubscribe<Messenger>(this, Messages.InsightsResults);
            MessagingCenter.Unsubscribe<Messenger>(this, Messages.InsightsNoResults);
        }

        private void ManageCelebritiesAndOCRVisibilities(object sender, PropertyChangedEventArgs e)
        {
            // Workaround because binding IsVisible doesn't work
            if (e.PropertyName == nameof(ILoadableWithData.HasData))
            {
                var celebritiesAndLandmarksViewModel = this.ViewModel.CelebritiesAndLandmarksViewModel
                                                      as CelebritiesAndLandmarksViewModel;

                // this.celebritiesView.IsVisible = celebritiesAndLandmarksViewModel.CelebritiesViewModel.HasData;
                this.canvas.Faces = celebritiesAndLandmarksViewModel.CelebritiesViewModel.Faces;

                // this.textUtils.IsVisible = this.ViewModel.OCRViewModel.HasData;

                // this.informationButton.IsEnabled = this.ViewModel.CelebritiesAndLandmarksViewModel.HasData;
                this.ocrButton.IsEnabled = this.ViewModel.OCRViewModel.HasData;
                this.textUtils.IsEnabled = this.ViewModel.OCRViewModel.HasData;
                this.imagesButton.IsEnabled = this.ViewModel.ImageSearchViewModel.HasData;
                this.productButton.IsEnabled = this.ViewModel.SimilarProductsViewModel.HasData;
                this.celebrityButton.IsEnabled = celebritiesAndLandmarksViewModel.CelebritiesViewModel.HasData;
                this.landmarkView.IsEnabled = celebritiesAndLandmarksViewModel.LandmarksViewModel.HasData;

                this.ActivateSection(Section.Information);
            }

            this.canvas.Reset();
        }

        private void OpenImageFromLibrary(Messenger obj, ImageModel image)
        {
            this.ViewModel.LoadImageFromLibraryCommand.Execute(image);
        }

        private void Results(Messenger obj)
        {
            this.ShowResults();
        }

        private void NoResults(Messenger obj)
        {
            this.ShowNoResults();
        }

        private void ShowResults()
        {
            this.resultsGrid.IsVisible = true;
            this.noResultsGrid.IsVisible = false;
        }

        private void ShowNoResults()
        {
            this.resultsGrid.IsVisible = false;
            this.noResultsGrid.IsVisible = true;
        }

        private void ActivateSection(Section section)
        {
            var ocrState = this.ocrButton.IsToggled;
            var celebritiesAndLandmarksViewModel = this.ViewModel.CelebritiesAndLandmarksViewModel as CelebritiesAndLandmarksViewModel;

            this.informationButton.IsToggled = section == Section.Information;

            var ocrVisible = this.ViewModel.OCRViewModel.HasData && (section == Section.Information || section == Section.OCR);

            // Why I have to do this? Why IsVisible property from VM is not working on XAML? FIX IT
            this.textUtils.IsVisible = ocrVisible;
            ((HideableViewModel)this.ViewModel.OCRViewModel).IsVisible = ocrVisible;
            this.ocrButton.IsToggled = ocrVisible;

            var imageSearchVisible = this.ViewModel.ImageSearchViewModel.HasData && (section == Section.Information || section == Section.SimilarImages);

            // Why I have to do this? Why IsVisible property from VM is not working on XAML? FIX IT
            this.similarImagesControl.IsVisible = imageSearchVisible;
            ((HideableViewModel)this.ViewModel.ImageSearchViewModel).IsVisible = imageSearchVisible;
            this.imagesButton.IsToggled = imageSearchVisible;

            var productsVisible = this.ViewModel.SimilarProductsViewModel.HasData
                && (section == Section.Information || section == Section.Product);
            ((HideableViewModel)this.ViewModel.SimilarProductsViewModel).IsVisible = productsVisible;
            this.productButton.IsToggled = productsVisible;

            var celebrityVisibility = celebritiesAndLandmarksViewModel.CelebritiesViewModel.HasData && (section == Section.Information || section == Section.Celebrity);

            // Why I have to do this? Why IsVisible property from VM is not working on XAML? FIX IT
            this.celebritiesView.IsVisible = celebrityVisibility;
            celebritiesAndLandmarksViewModel.CelebritiesViewModel.IsVisible = celebrityVisibility;
            this.celebrityButton.IsToggled = celebrityVisibility;

            var landmarkVisibility = celebritiesAndLandmarksViewModel.LandmarksViewModel.HasData
                && (section == Section.Information || section == Section.Landmark);
            celebritiesAndLandmarksViewModel.LandmarksViewModel.IsVisible = landmarkVisibility;
            this.landmarkButton.IsToggled = landmarkVisibility;
        }

        private void ToggleInfoSectionClick(object sender, EventArgs e)
        {
            this.ActivateSection(Section.Information);
        }

        private void ToggleOCRSectionClick(object sender, EventArgs e)
        {
            this.ActivateSection(Section.OCR);
        }

        private void ToggleSimilarImagesSectionClick(object sender, System.EventArgs e)
        {
            this.ActivateSection(Section.SimilarImages);
        }

        private void ToggleProductsSectionClick()
        {
            this.ActivateSection(Section.Product);
        }

        private void ToggleCelebrityNewsSectionClick(object sender, EventArgs e)
        {
            this.ActivateSection(Section.Celebrity);
        }

        private void ToggleLandmarkSectionClick()
        {
            this.ActivateSection(Section.Landmark);
        }

        private void Undo(object sender, EventArgs e)
        {
            this.canvas.Undo();
        }

        private void Redo(object sender, EventArgs e)
        {
            this.canvas.Redo();
        }

        private void Save(object sender, EventArgs e)
        {
            var filePath = this.fileChooserService.ChooseFilePath();

            if (!string.IsNullOrEmpty(filePath))
            {
                this.canvas.Save(filePath);
            }
        }

        private void Erase(object sender, EventArgs e)
        {
            this.canvas.Reset();
        }
    }
}
