// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SnipInsight.AIServices.AILogic;
using SnipInsight.AIServices.AIModels;
using SnipInsight.ImageCapture;
using SnipInsight.Properties;
using SnipInsight.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace SnipInsight.AIServices.AIViewModels
{
    /// <summary>
    /// ViewModel for the Image Search, simple array string
    /// The result is a string containing the text
    /// </summary>
    public class ImageSearchViewModel : ViewModelBase
    {
        public ImageSearchViewModel()
        {
            _handler = new ImageSearchHandler("ImageSearch");

            // Commands initialization
            ImageSelected = new RelayCommand<ImageSearchModel>(ImageSelectedExecute, ImageSelectedCanExecute);
        }

        /// <summary>
        /// Current wrap panel width
        /// </summary>
        private double currentWrapPanelWidth = 0;

        /// <summary>
        /// Object used for dynamic image resizing
        /// </summary>
        private readonly ImageDynamicDisplay DynamicImageResizer = new ImageDynamicDisplay();

        /// <summary>
        /// Max height of images in the panel
        /// </summary>
        private const double ImageMaxHeight = 175;

        /// <summary>
        /// The defined width for wrap panel
        /// </summary>
        private double wrapPanelDefinedWidth;

        private ObservableCollection<ImageSearchModel> _images;

        /// <summary>
        /// Image list for the Reverse Search
        /// </summary>
        public ObservableCollection<ImageSearchModel> Images
        {
            get { return _images; }
            set
            {
                _images = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// The current width for wrap panel
        /// </summary>
        public double CurrentWrapPanelWidth
        {
            get { return currentWrapPanelWidth; }
            set
            {
                currentWrapPanelWidth = value;
                if(currentWrapPanelWidth >= DynamicImageResizer.CenterBound)
                {
                    WrapPanelDefinedWidth = currentWrapPanelWidth;
                }
                DynamicImageResizer.CreateControlGallery(ImageMaxHeight, currentWrapPanelWidth);
            }
        }

        /// <summary>
        /// The defined width for wrap panel
        /// </summary>
        public double WrapPanelDefinedWidth
        {
            get { return currentWrapPanelWidth; }
            set
            {
                wrapPanelDefinedWidth = value;
                RaisePropertyChanged();
            }
        }

        public async Task LoadImages(MemoryStream imageStream)
        {
            Images = null;
            IsVisible = Visibility.Collapsed;

            var model = await _handler.GetResult(imageStream);

            if (model != null && model.Container != null && model.Container.Images != null)
            {
                Images = new ObservableCollection<ImageSearchModel>(model.Container.Images);
                IsVisible = model.Container.Images.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                DynamicImageResizer.PopulateAspectRatioDict();
                DynamicImageResizer.CreateControlGallery(ImageMaxHeight, currentWrapPanelWidth);
            }

            ServiceLocator.Current.GetInstance<AIPanelViewModel>().ImageSearchCommand.RaiseCanExecuteChanged();
        }

        #region Commands
        public RelayCommand<ImageSearchModel> ImageSelected { get; set; }
        #endregion

        #region CommandsCanExecute

        private bool ImageSelectedCanExecute(ImageSearchModel model)
        {
            return true;
        }
        #endregion

        #region CommandsExecute

        /// <summary>
        /// Load the selected image main panel for further opertaion.
        /// </summary>
        /// <param name="model">Details of the image from the AI result to be loaded</param>
        private void ImageSelectedExecute(ImageSearchModel model)
        {
            SnipInsightViewModel viewModel = AppManager.TheBoss.ViewModel;
            var aiPanelVM = ServiceLocator.Current.GetInstance<AIPanelViewModel>();

            AppManager.TheBoss.OnSaveImage();

            if (string.IsNullOrEmpty(viewModel.RestoreImageUrl))
            {
                viewModel.RestoreImageUrl = viewModel.SavedCaptureImage;
            }

            ImageLoader.LoadFromUrl(new Uri(model.URL)).ContinueWith(t =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    aiPanelVM.CapturedImage = t.Result;
                    viewModel.SelectedImageUrl = model.URL;
                    AppManager.TheBoss.RunAllInsights();
                });
             }, TaskContinuationOptions.OnlyOnRanToCompletion);

            ImageLoader.LoadFromUrl(new Uri(model.URL)).ContinueWith(t =>
            {
                MessageBox.Show(Resources.Image_Not_Loaded);
            }, TaskContinuationOptions.NotOnRanToCompletion);
        }
        #endregion

        private ImageSearchHandler _handler;

        private Visibility _isVisible = Visibility.Collapsed;

        public Visibility IsVisible {
            get => _isVisible;

            set
            {
                _isVisible = value;

                RaisePropertyChanged();
            }
        }
    }
}