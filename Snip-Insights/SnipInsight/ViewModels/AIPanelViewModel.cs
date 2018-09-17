// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SnipInsight.AIServices.AIViewModels;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SnipInsight.ViewModels
{
    public class AIPanelViewModel : ViewModelBase
    {
        public enum AiSelected
        {
            Suggested = 0,
            ImageSearch,
            ProductSearch,
            PeopleSearch,
            PlaceSearch,
            Ocr
        }

        /// <summary>
        /// Current selected AI
        /// </summary>
        private AiSelected _currentAI = AiSelected.Suggested;

        public void ActivateButtons(AiSelected newSelection)
        {
            _currentAI = newSelection;
            RaisePropertyChanged("SuggestedInsightsVisible");
            RaisePropertyChanged("ImageToTextVisible");
            RaisePropertyChanged("SimilarImagesVisible");
            RaisePropertyChanged("ProductSearchVisible");
            RaisePropertyChanged("PeopleSearchVisible");
            RaisePropertyChanged("PlaceSearchVisible");
            ImageSearchVisibility = Visibility.Collapsed;
            ProductSearchVisibility = Visibility.Collapsed;
            PeopleSearchVisibility = Visibility.Collapsed;
            OCRVisibility = Visibility.Collapsed;
            PlaceSearchVisibility = Visibility.Collapsed;
            OCRCommand.RaiseCanExecuteChanged();
            SuggestedCommand.RaiseCanExecuteChanged();
            ImageSearchCommand.RaiseCanExecuteChanged();
            ProductSearchCommand.RaiseCanExecuteChanged();
            PeopleSearchCommand.RaiseCanExecuteChanged();
            PlaceSearchCommand.RaiseCanExecuteChanged();
            AppManager.TheBoss.MainWindow.VerticalScrollViewer.ScrollToTop();
        }

        public AIPanelViewModel()
        {
            SuggestedCommand = new RelayCommand(SuggestedCommandExecute);
            ImageSearchCommand = new RelayCommand(ImageSearchCommandExecute, ImageSearchCommandCanExecute);
            ProductSearchCommand = new RelayCommand(ProductSearchCommandExecute, ProductSearchCommandCanExecute);
            PeopleSearchCommand = new RelayCommand(PeopleSearchCommandExecute, PeopleSearchCommandCanExecute);
            PlaceSearchCommand = new RelayCommand(PlaceSearchCommandExecute, PlaceSearchCommandCanExecute);
            OCRCommand = new RelayCommand(OCRCommandExecute, OCRCommandCanExecute);
        }

        #region Properties
        public BitmapSource CapturedImage
        {
            get
            {
                return AppManager.TheBoss.ViewModel.CapturedImage;
            }
            set
            {
                AppManager.TheBoss.ViewModel.CapturedImage = value;
                RaisePropertyChanged();
            }
        }

        public bool SuggestedInsightsVisible
        {
            get { return _currentAI == AiSelected.Suggested; }
            set
            {
                if(!SuggestedInsightsVisible)
                {
                    _currentAI = AiSelected.Suggested;
                    SuggestedCommandExecute();
                }
            }
        }

        public bool ImageToTextVisible
        {
            get { return _currentAI == AiSelected.Ocr; }
            set
            {
                if (!ImageToTextVisible)
                {
                    _currentAI = AiSelected.Ocr;
                    OCRCommandExecute();
                }
            }
        }
        public bool SimilarImagesVisible
        {
            get { return _currentAI == AiSelected.ImageSearch; }
            set
            {
                if (!SimilarImagesVisible)
                {
                    _currentAI = AiSelected.ImageSearch;
                    ImageSearchCommandExecute();
                }
            }
        }
        public bool ProductSearchVisible
        {
            get { return _currentAI == AiSelected.ProductSearch; }
            set
            {
                if (!ProductSearchVisible)
                {
                    _currentAI = AiSelected.ProductSearch;
                    ProductSearchCommandExecute();
                }
            }
        }
        public bool PeopleSearchVisible
        {
            get { return _currentAI == AiSelected.PeopleSearch; }
            set
            {
                if (!PeopleSearchVisible)
                {
                    _currentAI = AiSelected.PeopleSearch;
                    PeopleSearchCommandExecute();
                }
            }
        }
        public bool PlaceSearchVisible
        {
            get { return _currentAI == AiSelected.PlaceSearch; }
            set
            {
                if (!PlaceSearchVisible)
                {
                    _currentAI = AiSelected.PlaceSearch;
                    PlaceSearchCommandExecute();
                }
            }
        }

        private Visibility _imageSearchVisibility = Visibility.Visible;

        public Visibility ImageSearchVisibility
        {
            get { return _imageSearchVisibility; }
            set
            {
                _imageSearchVisibility = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _productSearchVisibility = Visibility.Visible;

        public Visibility ProductSearchVisibility
        {
            get { return _productSearchVisibility; }
            set
            {
                _productSearchVisibility = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _oCRVisibility = Visibility.Visible;

        public Visibility OCRVisibility
        {
            get { return _oCRVisibility; }
            set
            {
                _oCRVisibility = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _peopleSearchVisibility = Visibility.Visible;

        public Visibility PeopleSearchVisibility
        {
            get { return _peopleSearchVisibility; }
            set
            {
                _peopleSearchVisibility = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _placeSearchVisibility = Visibility.Visible;

        public Visibility PlaceSearchVisibility
        {
            get { return _placeSearchVisibility; }
            set
            {
                _placeSearchVisibility = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _loadingVisibility = Visibility.Visible;

        public Visibility LoadingVisibility
        {
            get => _loadingVisibility;

            set
            {
                _loadingVisibility = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _emptyStateVisibility = Visibility.Collapsed;

        public Visibility EmptyStateVisibility
        {
            get => _emptyStateVisibility;

            set
            {
                _emptyStateVisibility = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _aiControlsVisibility = Visibility.Collapsed;

        public Visibility AIControlsVisibility
        {
            get => _aiControlsVisibility;

            set
            {
                _aiControlsVisibility = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand SuggestedCommand { get; set; }

        public RelayCommand ImageSearchCommand { get; set; }

        public RelayCommand ProductSearchCommand { get; set; }

        public RelayCommand PeopleSearchCommand { get; set; }

        public RelayCommand PlaceSearchCommand { get; set; }
        #endregion

        #region Commands

        private bool SuggestedCommandCanExecute()
        {
            // return _currentAI != AiSelected.Suggested;
            return true;
        }

        public void SuggestedCommandExecute()
        {
            ActivateButtons(AiSelected.Suggested);
            ImageSearchVisibility = Visibility.Visible;
            ProductSearchVisibility = Visibility.Visible;
            PeopleSearchVisibility = Visibility.Visible;
            OCRVisibility = Visibility.Visible;
            PlaceSearchVisibility = Visibility.Visible;
            Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.SuggestedInsightsButton, Telemetry.ViewName.AiPanel);
        }

        private bool ImageSearchCommandCanExecute()
        {
            // return (_currentAI != AiSelected.ImageSearch) &&
                // ServiceLocator.Current.GetInstance<ImageSearchViewModel>().IsVisible == Visibility.Visible;

            return ServiceLocator.Current.GetInstance<ImageSearchViewModel>().IsVisible == Visibility.Visible; ;
        }

        public void ImageSearchCommandExecute()
        {
            ActivateButtons(AiSelected.ImageSearch);
            ImageSearchVisibility = Visibility.Visible;
            Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.ImageSearchButton, Telemetry.ViewName.AiPanel);
        }

        private bool ProductSearchCommandCanExecute()
        {
            // return _currentAI != AiSelected.ProductSearch &&
                // ServiceLocator.Current.GetInstance<ProductSearchViewModel>().IsVisible == Visibility.Visible;

            return ServiceLocator.Current.GetInstance<ProductSearchViewModel>().IsVisible == Visibility.Visible;
        }

        public void ProductSearchCommandExecute()
        {
            ActivateButtons(AiSelected.ProductSearch);
            ProductSearchVisibility = Visibility.Visible;
            Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.ProductSearchButton, Telemetry.ViewName.AiPanel);
        }

        private bool PeopleSearchCommandCanExecute()
        {
            // return _currentAI != AiSelected.PeopleSearch &&
               //  ServiceLocator.Current.GetInstance<ImageAnalysisViewModel>().IsPeopleVisible == Visibility.Visible;

            return ServiceLocator.Current.GetInstance<ImageAnalysisViewModel>().IsPeopleVisible == Visibility.Visible;
        }

        public void PeopleSearchCommandExecute()
        {
            ActivateButtons(AiSelected.PeopleSearch);
            PeopleSearchVisibility = Visibility.Visible;
            Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.PeopleSearchButton,Telemetry.ViewName.AiPanel);
        }

        private bool PlaceSearchCommandCanExecute()
        {
            // return _currentAI != AiSelected.PlaceSearch &&
               // ServiceLocator.Current.GetInstance<ImageAnalysisViewModel>().IsPlaceVisible == Visibility.Visible;

            return ServiceLocator.Current.GetInstance<ImageAnalysisViewModel>().IsPlaceVisible == Visibility.Visible;
        }

        public void PlaceSearchCommandExecute()
        {
            ActivateButtons(AiSelected.PlaceSearch);
            PlaceSearchVisibility = Visibility.Visible;
            Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.PlaceSearchButton, Telemetry.ViewName.AiPanel);
        }

        public RelayCommand OCRCommand { get; set; }

        private bool OCRCommandCanExecute()
        {
            //return _currentAI != AiSelected.Ocr &&
            //    ServiceLocator.Current.GetInstance<OCRViewModel>().IsVisible == Visibility.Visible;

            return ServiceLocator.Current.GetInstance<OCRViewModel>().IsVisible == Visibility.Visible;
        }

        public void OCRCommandExecute()
        {
            ActivateButtons(AiSelected.Ocr);
            OCRVisibility = Visibility.Visible;
            Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.OCRButton, Telemetry.ViewName.AiPanel);
        }

        #endregion
    }
}
