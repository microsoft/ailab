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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace SnipInsight.AIServices.AIViewModels
{
    /// <summary>
    /// ViewModel for the Image Search, simple array string
    /// The result is a string containing the text
    /// </summary>
    public class ProductSearchViewModel : ViewModelBase
    {
        public ProductSearchViewModel()
        {
            _handler = new ProductSearchHandler("ImageSearch");

            // Commands initialization
            ProductSelected = new RelayCommand<ProductSearchModel>(ProductSelectedExecute, ProductSelectedCanExecute);
            NavigateToUrl = new RelayCommand<ProductSearchModel>(NavigateToUrlExecute, NavigateToUrlCanExecute);
        }

        /// <summary>
        /// Current wrap panel width
        /// </summary>
        private double currentWrapPanelWidth = 0;

        /// <summary>
        /// Object used for dynamic image resizing
        /// </summary>
        private readonly ProductDynamicDisplay DynamicProductResizer = new ProductDynamicDisplay();

        /// <summary>
        /// Max height of images in the panel
        /// </summary>
        private const double ImageMaxHeight = 175;

        /// <summary>
        /// The defined width for wrap panel
        /// </summary>
        private double wrapPanelDefinedWidth;

        private ObservableCollection<ProductSearchModel> products;

        /// <summary>
        /// The current width for wrap panel
        /// </summary>
        public double CurrentWrapPanelWidth
        {
            get { return currentWrapPanelWidth; }
            set
            {
                currentWrapPanelWidth = value;
                if (currentWrapPanelWidth >= DynamicProductResizer.CenterBound)
                {
                    WrapPanelDefinedWidth = currentWrapPanelWidth;
                }
                DynamicProductResizer.CreateControlGallery(ImageMaxHeight, currentWrapPanelWidth);
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

        /// <summary>
        /// List of the products displayed on screen
        /// </summary>
        public ObservableCollection<ProductSearchModel> Products
        {
            get { return products; }
            set
            {
                products = value;
                RaisePropertyChanged();
            }
        }

        public async Task LoadProducts(MemoryStream imageStream)
        {
            Products = null;

            IsVisible = Visibility.Collapsed;

            var model = await _handler.GetResult(imageStream);

            if (model != null && model.Container != null && model.Container.Products != null)
            {
                Products = new ObservableCollection<ProductSearchModel>(model.Container.Products);
                IsVisible = Visibility.Visible;
                DynamicProductResizer.PopulateAspectRatioDict();
                DynamicProductResizer.CreateControlGallery(ImageMaxHeight, currentWrapPanelWidth);
            }

            ServiceLocator.Current.GetInstance<AIPanelViewModel>().ProductSearchCommand.RaiseCanExecuteChanged();
        }

        #region Commands
        public RelayCommand<ProductSearchModel> ProductSelected { get; set; }

        public RelayCommand<ProductSearchModel> NavigateToUrl { get; set; }
        #endregion

        #region CommandsCanExecute
        private bool ProductSelectedCanExecute(ProductSearchModel model)
        {
            return true;
        }

        private bool NavigateToUrlCanExecute(ProductSearchModel model)
        {
            return true;
        }
        #endregion

        #region CommandsExecute
        private void ProductSelectedExecute(ProductSearchModel model)
        {
            SnipInsightViewModel viewModel = AppManager.TheBoss.ViewModel;
            var aiPanelVM = ServiceLocator.Current.GetInstance<AIPanelViewModel>();

            AppManager.TheBoss.OnSaveImage();

            if (string.IsNullOrEmpty(viewModel.RestoreImageUrl))
            {
                viewModel.RestoreImageUrl = viewModel.SavedCaptureImage;
            }

            ImageLoader.LoadFromUrl(new Uri(model.ContentUrl)).ContinueWith(t =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    aiPanelVM.CapturedImage = t.Result;
                    viewModel.SelectedImageUrl = model.ContentUrl;
                    AppManager.TheBoss.RunAllInsights();
                });
            }, TaskContinuationOptions.OnlyOnRanToCompletion);

            ImageLoader.LoadFromUrl(new Uri(model.ContentUrl)).ContinueWith(t =>
            {
                MessageBox.Show(Resources.Image_Not_Loaded);
            }, TaskContinuationOptions.NotOnRanToCompletion);
        }

        private void NavigateToUrlExecute(ProductSearchModel model)
        {
            try
            {
                Process.Start(model.HostPage);
            }
            catch (Win32Exception CaughtException)
            {
                MessageBox.Show(Resources.No_Browser);
                Console.WriteLine(CaughtException.Message);
            }
        }
        #endregion

        private ProductSearchHandler _handler;

        private Visibility _isVisible = Visibility.Collapsed;

        public Visibility IsVisible
        {
            get => _isVisible;

            set
            {
                _isVisible = value;

                RaisePropertyChanged();
            }
        }
    }
}