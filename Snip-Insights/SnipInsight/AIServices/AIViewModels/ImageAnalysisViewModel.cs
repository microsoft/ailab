// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SnipInsight.AIServices.AILogic;
using SnipInsight.AIServices.AIModels;
using SnipInsight.Properties;
using SnipInsight.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SnipInsight.AIServices.AIViewModels
{
    public class ImageAnalysisViewModel : ViewModelBase
    {
        private ImageAnalysisHandler _analysisHandler;
        private ObservableCollection<LandmarkModel> _landmark;

        private CelebrityModel _celebrity;

        private Visibility _isPeopleVisible = Visibility.Collapsed;
        private Visibility _isPlaceVisible = Visibility.Collapsed;

        private SolidColorBrush unselectedStrokeBrush = new SolidColorBrush(new Color() { R = 255, G = 255, B = 255, A = 64 });
        private SolidColorBrush selectedStrokeBrush = new SolidColorBrush(new Color() { R = 102, G = 228, B = 196, A = 255 });

        /// <summary>
        /// Command for navigating to url on celebrity panel
        /// </summary>
        public RelayCommand NavigateToCelebrityUrlCommand { get; set; }

        /// <summary>
        /// Command for navigating to url on landmark panel
        /// </summary>
        public RelayCommand NavigateToLandmarkUrlCommand { get; set; }


        /// <summary>
        /// Command for navigating to url on news panel
        /// </summary>
        public RelayCommand<NewsModel> NavigateToNewsUrlCommand { get; set; }

		/// <summary>
		/// Celebrity recognized by the API
		/// </summary>
        public CelebrityModel Celebrity
        {
            get => _celebrity;
            set
            {
                _celebrity = value;
                RaisePropertyChanged();
            }
        }

		/// <summary>
		/// Landmark recognized by the API
		/// </summary>
        public ObservableCollection<LandmarkModel> Landmarks
        {
            get => _landmark;
            set
            {
                _landmark = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Whether or not the people panel should be visible
        /// </summary>
        public Visibility IsPeopleVisible
        {
            get => _isPeopleVisible;

            set
            {
                _isPeopleVisible = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Whether or not the place panel should be visible
        /// </summary>
        public Visibility IsPlaceVisible
        {
            get => _isPlaceVisible;

            set
            {
                _isPlaceVisible = value;
                RaisePropertyChanged();
            }
        }

		/// <summary>
		/// Constructor for the Image Analysis view model
		/// </summary>
        public ImageAnalysisViewModel()
        {
            _analysisHandler = new ImageAnalysisHandler("ImageAnalysis");

            NavigateToCelebrityUrlCommand = new RelayCommand(NavigateToCelebrityUrlCommandExecute);
            NavigateToLandmarkUrlCommand = new RelayCommand(NavigateToLandmarkUrlCommandExecute);
            NavigateToNewsUrlCommand = new RelayCommand<NewsModel> (NavigateToNewsUrlCommandExecute);
        }

		/// <summary>
		/// Navigate to the celebrity description detail page
		/// </summary>
        private void NavigateToCelebrityUrlCommandExecute()
        {
            try
            {
                Process.Start(Celebrity.URL);
            }
            catch (Win32Exception)
            {
                MessageBox.Show(Resources.No_Browser);
            }
        }

		/// <summary>
		/// Navigate to the landmark description detail page
		/// </summary>
		private void NavigateToLandmarkUrlCommandExecute()
        {
            try
            {
                Process.Start(Landmarks[0].URL);
            }
            catch (Win32Exception)
            {
                MessageBox.Show(Resources.No_Browser);
            }
        }

        /// <summary>
        /// Command for navigating to url on news panel
        /// </summary>
        private void NavigateToNewsUrlCommandExecute(NewsModel newsModel)
        {
            try
            {
                Process.Start(newsModel.URL);
            }
            catch (Win32Exception)
            {
                MessageBox.Show(Resources.No_Browser);
            }
        }

		/// <summary>
		/// Retrieve the informations about the celebrity from the cloud services
		/// </summary>
		/// <param name="model">Model containing the json image analysis data</param>
		/// <returns></returns>
        private async Task GetCelebrities(ImageAnalysisModel model)
        {
            EntitySearchHandler entityHandler = new EntitySearchHandler("EntitySearch");
            NewsHandler newsHandler = new NewsHandler("ImageSearch");

            Celebrity = null;
            IsPeopleVisible = Visibility.Collapsed;
            Canvas celebrities = new Canvas();

            try
            {
                foreach (ImageAnalysisModel.Category category in model.Categories)
                {
                    foreach (ImageAnalysisModel.Celebrity celebrity in category.Detail.Celebrities)
                    {
                        var celebrityModel = await entityHandler.GetResult(celebrity.Name);

                        if (celebrityModel.Entities != null)
                        {
                            var entry = celebrityModel.Entities.List.FirstOrDefault();

                            if (entry != null)
                            {
                                Rectangle rect = new Rectangle();
                                var celebModel = new CelebrityModel
                                {
                                    Name = entry.Name,
                                    Image = entry.Image.URL,
                                    URL = entry.URL,
                                    Description = entry.Description
                                };

                                var result = await newsHandler.GetResult(celebrity.Name);

                                celebModel.News = new ObservableCollection<NewsModel>(result.News);

                                foreach(NewsModel newsModel in celebModel.News)
                                {
                                    newsModel.DatePublished = newsModel.DatePublished.Substring(0, 10);
                                    newsModel.Description = newsModel.Description.Substring(0, 150) + "...";
                                }

                                rect.Tag = celebModel;

                                rect.Width = celebrity.FaceRectangle.Width;
                                rect.Height = celebrity.FaceRectangle.Height;
                                //rect.Stroke = Brushes.Transparent;
                                rect.Stroke = unselectedStrokeBrush;
                                rect.StrokeThickness = 2;
                                rect.Fill = Brushes.Transparent;
                                rect.ToolTip = entry.Name;
                                rect.MouseDown += CelebritySelected;
                                rect.MouseEnter += ShowCelebrityRectangle;
                                rect.MouseLeave += HideCelebrityRectangle;

                                celebrities.Children.Add(rect);
                                Canvas.SetLeft(rect, celebrity.FaceRectangle.Left);
                                Canvas.SetTop(rect, celebrity.FaceRectangle.Top);
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(Resources.Exception_at_celebrities + e.Message);
            }

            if (celebrities.Children.Count > 0)
            {
                IsPeopleVisible = Visibility.Visible;
                var rect = celebrities.Children[0] as Rectangle;
                rect.Stroke = selectedStrokeBrush;
                Celebrity = rect.Tag as CelebrityModel;
            }

            ServiceLocator.Current.GetInstance<AIPanelViewModel>().PeopleSearchCommand.RaiseCanExecuteChanged();

            AppManager.TheBoss.ViewModel.CelebritiesCanvas = celebrities;
        }

		/// <summary>
		/// Retrieve the informations about the landmark from the cloud services
		/// </summary>
		/// <param name="model">Model containing the json image analysis data</param>
		/// <returns></returns>
		private async Task GetLandmarks(ImageAnalysisModel model)
        {
            ObservableCollection<LandmarkModel> landmarkList = new ObservableCollection<LandmarkModel>();

            EntitySearchHandler entityHandler = new EntitySearchHandler("EntitySearch");

            try
            {
                foreach (ImageAnalysisModel.Category category in model.Categories)
                {
                    foreach (ImageAnalysisModel.Landmark landmark in category.Detail.Landmarks)
                    {
                        var landmarkModel = await entityHandler.GetResult(landmark.Name);

                        var entry = landmarkModel.Entities.List.FirstOrDefault();

                        if (entry != null)
                        {
                            landmarkList.Add(new LandmarkModel
                            {
                                Name = entry.Name,
                                Image = entry.Image.URL,
                                URL = entry.URL,
                                Description = entry.Description,
                                PostalCode = null,
                                Telephone = null,
                                Address = null
                            });
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(Resources.Exception_at_landmark + e.Message);
            }

            IsPlaceVisible = landmarkList.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            ServiceLocator.Current.GetInstance<AIPanelViewModel>().PlaceSearchCommand.RaiseCanExecuteChanged();

            Landmarks = landmarkList;
        }

		/// <summary>
		/// Update the metadata for saving or sharing the image
		/// </summary>
		/// <param name="model">Model containing the json image analysis data</param>
		/// <returns></returns>
		private void UpdateMetadata(ImageAnalysisModel model)
        {
            ImageAnalysisResult result = new ImageAnalysisResult();

            if (model == null || model.Description == null)
            {
                return;
            }

            var caption = model.Description.Captions.FirstOrDefault();

            if (caption != null)
            {
                result.Caption = caption.Text;
                result.CaptionAvailable = true;
            }

            if (model.Description.Tags.Count > 0)
            {
                result.Tags = model.Description.Tags.ToArray();
                result.TagsAvailable = true;
            }

            AppManager.TheBoss.ImageMetadata = result;
        }

		/// <summary>
		/// Analyse the image
		/// </summary>
		/// <param name="imageStream">Image used for the analysis</param>
		/// <returns>A task containing the success or failure of the operation</returns>
        public async Task LoadAnalysis(MemoryStream imageStream)
        {
            IsPeopleVisible = Visibility.Collapsed;
            IsPlaceVisible = Visibility.Collapsed;

            var model = await _analysisHandler.GetResult(imageStream);

            // Create the celebrities models
            await GetCelebrities(model);

            // Create the landmarks models
            await GetLandmarks(model);

            UpdateMetadata(model);
        }

		/// <summary>
		/// Change the selected celebrity based on the user's choice
		/// </summary>
        private void CelebritySelected(object sender, MouseButtonEventArgs e)
        {
            var rect = sender as Rectangle;

            Celebrity = rect.Tag as CelebrityModel;

            var par = rect.Parent as Canvas;
            foreach (var obj in par.Children)
            {
                var otherRect = obj as Rectangle;
                otherRect.Stroke = unselectedStrokeBrush;
            }

            rect.Stroke = selectedStrokeBrush;
        }

		/// <summary>
		/// Show the hovered celebrity
		/// </summary>
        private void ShowCelebrityRectangle(object sender, MouseEventArgs e)
        {
            var rect = sender as Rectangle;

            var celeb = rect.Tag as CelebrityModel;

            if (celeb != Celebrity)
            {
                rect.Stroke = unselectedStrokeBrush;
            }
        }

		/// <summary>
		/// Hide the face rectangle on mouse leave
		/// </summary>
        private void HideCelebrityRectangle(object sender, MouseEventArgs e)
        {
            var rect = sender as Rectangle;
            var celeb = rect.Tag as CelebrityModel;

            if (celeb != Celebrity)
            {
                rect.Stroke = unselectedStrokeBrush;
            }
        }
    }
}
