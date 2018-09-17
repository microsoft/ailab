// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SnipInsight.Properties;
using SnipInsight.Util;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace SnipInsight.AIServices.AIViewModels
{
    public class InsightsPermissionsViewModel : ViewModelBase
    {
        public InsightsPermissionsViewModel()
        {
            TurnOnAIAnalysisCommand = new RelayCommand(TurnOnAIAnalysisCommandExecute);
            OpenPrivatePolicyInBrowserCommand = new RelayCommand(OpenPrivatePolicyInBrowserCommandExecute);
            OpenLearnMoreInBrowserCommand = new RelayCommand(OpenLearnMoreInBrowserCommandExecute);
        }

        #region Commands
        /// <summary>
        /// Turns on the setting for using ai analysis, disables visibility for permissions and enables the insights visible
        /// </summary>
        public RelayCommand TurnOnAIAnalysisCommand { get; set; }

        /// <summary>
        /// Opens the web browser and navigates to the microsoft privacy policy page
        /// </summary>
        public RelayCommand OpenPrivatePolicyInBrowserCommand { get; set; }

        /// <summary>
        /// Opens the web browser and navigates to the microsoft cognitive services page
        /// </summary>
        public RelayCommand OpenLearnMoreInBrowserCommand { get; set; }

        /// <summary>
        /// Turns on the setting for using ai analysis, disables visibility for permissions and enables the insights visible
        /// </summary>
        private void TurnOnAIAnalysisCommandExecute()
        {
            UserSettings.IsAIEnabled = true;
            AppManager.TheBoss.ViewModel.InsightsVisible = UserSettings.IsAIEnabled;
            AppManager.TheBoss.RunAllInsights();
        }

        /// <summary>
        /// Opens the web browser and navigates to the microsoft privacy policy page
        /// </summary>
        private void OpenPrivatePolicyInBrowserCommandExecute()
        {
            GoToUrl("https://go.microsoft.com/fwlink/?LinkId=521839");
        }

        /// <summary>
        /// Opens the web browser and navigates to the microsoft cognitive services page
        /// </summary>
        private void OpenLearnMoreInBrowserCommandExecute()
        {
            GoToUrl("https://azure.microsoft.com/en-us/services/cognitive-services/");
        }
        #endregion

        #region Helper Methods

        /// <summary>
        /// Navigates to the given url, throws a Win32 exception if there
        /// is no internet browser found on the computer
        /// </summary>
        private void GoToUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Win32Exception)
            {
                MessageBox.Show(Resources.No_Browser);
            }
        }
        #endregion
    }
}
