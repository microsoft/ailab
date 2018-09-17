// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using SnipInsight.AIServices.AIViewModels;

namespace SnipInsight.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<ProductSearchViewModel>();
            SimpleIoc.Default.Register<AIPanelViewModel>();
            SimpleIoc.Default.Register<ImageSearchViewModel>();
            SimpleIoc.Default.Register<ImageAnalysisViewModel>();
            SimpleIoc.Default.Register<OCRViewModel>();
            SimpleIoc.Default.Register<InsightsPermissionsViewModel>();
        }

        public ProductSearchViewModel ProductSearchLoc
        {
            get => ServiceLocator.Current.GetInstance<ProductSearchViewModel>();
        }

        public ImageSearchViewModel ImageSearchLoc
        {
            get => ServiceLocator.Current.GetInstance<ImageSearchViewModel>();
        }

        public AIPanelViewModel AIPanelLoc
        {
            get => ServiceLocator.Current.GetInstance<AIPanelViewModel>();
        }

        public ImageAnalysisViewModel ImageAnalysisLoc
        {
            get => ServiceLocator.Current.GetInstance<ImageAnalysisViewModel>();
        }

        public OCRViewModel OCRLoc
        {
            get => ServiceLocator.Current.GetInstance<OCRViewModel>();
        }

        public InsightsPermissionsViewModel InsightsPermissionsLoc
        {
            get => ServiceLocator.Current.GetInstance<InsightsPermissionsViewModel>();
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}


/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:SnipInsight.ViewModels"
                           x:Key="Locator" />
  </Application.Resources>

  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/
