// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using CommonServiceLocator;
using SnipInsight.AIServices.AIViewModels;
using SnipInsight.ViewModels;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SnipInsight.AIServices
{
    /// <summary>
    /// Manage the  API calls asynchronously
    /// Controls what gets displayed into the panel
    /// Secure the keys and feed the data to the handlers
    /// </summary>
    class AIManager
    {
        public Byte[] ImageBytes { get; set; }

        /// <summary>
        /// Run all the Azure API calls asynchronously.
        /// </summary>
        internal void RunAllAsyncCalls()
        {
            var AIViewModel = ServiceLocator.Current.GetInstance<AIPanelViewModel>();

            AIViewModel.AIControlsVisibility = Visibility.Collapsed;
            AIViewModel.EmptyStateVisibility = Visibility.Collapsed;
            AIViewModel.LoadingVisibility = Visibility.Visible;

            AppManager.TheBoss.ViewModel.CelebritiesCanvas = null;

            // If we decide to go back to the previously used panel after each snip
            // Then removing this line would allow it
            // AIViewModel.SuggestedCommand.Execute(null);
            AIViewModel.SuggestedInsightsVisible = true;

            MemoryStream imageSearchStream = new MemoryStream(ImageBytes);
            var imageSearchVM = ServiceLocator.Current.GetInstance<ImageSearchViewModel>();
            var imageSearchTask = imageSearchVM.LoadImages(imageSearchStream);

            MemoryStream productSearchStream = new MemoryStream(ImageBytes);
            var productSearchVM = ServiceLocator.Current.GetInstance<ProductSearchViewModel>();
            var productSearchTask = productSearchVM.LoadProducts(productSearchStream);

            MemoryStream imageAnalysisStream = new MemoryStream(ImageBytes);
            var imageAnalysisVM = ServiceLocator.Current.GetInstance<ImageAnalysisViewModel>();
            var imageAnalysisTask = imageAnalysisVM.LoadAnalysis(imageAnalysisStream);

            MemoryStream writtenStream = new MemoryStream(ImageBytes);
            MemoryStream printedStream = new MemoryStream(ImageBytes);
            var ocrVM = ServiceLocator.Current.GetInstance<OCRViewModel>();
            var ocrTask = ocrVM.LoadText(writtenStream, printedStream);

            var completionTask = Task.WhenAll(imageSearchTask, productSearchTask, imageAnalysisTask, ocrTask);

            completionTask.ContinueWith(t =>
            {
                AIViewModel.LoadingVisibility = Visibility.Collapsed;

                if (LookForEmptyState())
                {
                    AIViewModel.EmptyStateVisibility = Visibility.Visible;
                }
                else
                {
                    AIViewModel.AIControlsVisibility = Visibility.Visible;
                }
            });
        }

        /// <summary>
        /// Check if we should display the empty state panel or not
        /// </summary>
        private bool LookForEmptyState()
        {
            /// Temporary solution to display the empty state in case of no result.
            /// Will be changed to work with the Task at a later date but this
            /// Workaround does the job without being detrimental for now.
            return ServiceLocator.Current.GetInstance<OCRViewModel>().IsVisible == Visibility.Collapsed &&
                ServiceLocator.Current.GetInstance<ImageSearchViewModel>().IsVisible == Visibility.Collapsed &&
                ServiceLocator.Current.GetInstance<ProductSearchViewModel>().IsVisible == Visibility.Collapsed &&
                ServiceLocator.Current.GetInstance<ImageAnalysisViewModel>().IsPeopleVisible == Visibility.Collapsed &&
                ServiceLocator.Current.GetInstance<ImageAnalysisViewModel>().IsPlaceVisible == Visibility.Collapsed;
        }
    }
}