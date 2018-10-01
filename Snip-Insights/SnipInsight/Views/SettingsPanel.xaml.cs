// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using SnipInsight.Controls.Ariadne;
using SnipInsight.Util;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using System.Diagnostics;

namespace SnipInsight.Views
{
	/// <summary>
	/// Interaction logic for Settings_Panel.xaml
	/// </summary>
	public partial class SettingsPanel : UserControl
    {
        // Max Display Length for path preview
        const int maxDisplayLength = 17;

        // Location of the default folder for the library view
        private readonly string mySnipLoc;

        public SettingsPanel()
        {
            InitializeComponent();

            mySnipLoc = GetDefaultPath();

            this.UpdateSettingsControls();

            this.DataContext = AppManager.TheBoss.ViewModel;
            AppManager.TheBoss.ViewModel.InsightsVisible = UserSettings.IsAIEnabled;
        }

        public void UpdateSettingsControls()
        {
            // RunWhenWindowsStartsCheckbox.IsChecked = !UserSettings.DisableRunWithWindows;
            // ShowToolbarOnDesktopCheckbox.IsChecked = !UserSettings.DisableToolWindow;
            // ScreenCaptureKeyCombo.KeyCombo = UserSettings.ScreenCaptureShortcut;
            // QuickCaptureKeyCombo.KeyCombo = UserSettings.QuickCaptureShortcut;
            ScreenCaptureDelaySlider.Value = UserSettings.ScreenCaptureDelay;
            // ShowNotificationPostSnip.IsChecked = UserSettings.IsNotificationToastEnabled;
            OpenEditorPostSnip.IsChecked = UserSettings.IsOpenEditorPostSnip;
            // ContentModerationStrengthSlider.Value = UserSettings.ContentModerationStrength;
            // AutoTagging.IsChecked = UserSettings.IsAutoTaggingEnabled;

            UserSettings.IsAutoTaggingEnabled = false;
            UserSettings.IsNotificationToastEnabled = true;
            UserSettings.DisableRunWithWindows = true;
            UserSettings.DisableToolWindow = false;

            EnableAI.IsChecked = UserSettings.IsAIEnabled;
            EntitySearch.Text = UserSettings.GetKey(EntitySearch.Name);
            ImageAnalysis.Text = UserSettings.GetKey(ImageAnalysis.Name);
            ImageSearch.Text = UserSettings.GetKey(ImageSearch.Name);
            TextRecognition.Text = UserSettings.GetKey(TextRecognition.Name);
            Translator.Text = UserSettings.GetKey(Translator.Name);
            ContentModerator.Text = UserSettings.GetKey(ContentModerator.Name);
            LUISAppId.Text = UserSettings.GetKey(LUISAppId.Name);
            LUISKey.Text = UserSettings.GetKey(LUISKey.Name);
            EntitySearchEndpoint.Text = UserSettings.GetKey(EntitySearchEndpoint.Name);
            ImageAnalysisEndpoint.Text = UserSettings.GetKey(ImageAnalysisEndpoint.Name);
            ImageSearchEndpoint.Text = UserSettings.GetKey(ImageSearchEndpoint.Name);
            TextRecognitionEndpoint.Text = UserSettings.GetKey(TextRecognitionEndpoint.Name);
            TranslatorEndpoint.Text = UserSettings.GetKey(TranslatorEndpoint.Name);
            ContentModeratorEndpoint.Text = UserSettings.GetKey(ContentModeratorEndpoint.Name);
            LUISKeyEndpoint.Text = UserSettings.GetKey(LUISKeyEndpoint.Name);
            // UpdateButton.IsEnabled = false;
            // UpdateButtonKeys.IsEnabled = false;
            this.versionTextBlock.Text = this.GetInformationalVersion();
            UpdateLocationPreview();
        }

        public string GetInformationalVersion()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            return FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
        }

        private void OpenLink(object sender, RoutedEventArgs e)
        {
            AriLinkButton menu = (AriLinkButton)sender;
            if (menu == null || menu.Tag == null)
                return;

            string link = (string)menu.Tag;
            System.Diagnostics.Process.Start(link);
        }

        private void ScreenCaptureKeyCombo_KeyComboChanged(object sender, EventArgs e)
        {
            // UserSettings.ScreenCaptureShortcut = ScreenCaptureKeyCombo.KeyCombo;
            // AppManager.TheBoss.ToolWindow.RegisterHotKey(SnipHotKey.ScreenCapture, ScreenCaptureKeyCombo.KeyCombo);
        }

        /// <summary>
        /// Detector for a change in the hotkey to access the quick capture feature
        /// </summary>
        private void QuickCaptureKeyCombo_KeyComboChanged(object sender, EventArgs e)
        {
            // UserSettings.QuickCaptureShortcut = QuickCaptureKeyCombo.KeyCombo;
            // AppManager.TheBoss.ToolWindow.RegisterHotKey(SnipHotKey.QuickCapture, QuickCaptureKeyCombo.KeyCombo);
        }

        private void RunWhenWindowsStartsCheckbox_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                bool run = ((AriToggleSwitch)sender).IsChecked.GetValueOrDefault(false);

                using (RegistryKey runKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    string regValueName = "Snip";

                    if (run)
                    {
                        string appPath = UserSettings.AppPath;
                        if (string.IsNullOrEmpty(appPath))
                        {
                            throw new ApplicationException("AppPath reg setting was not found");
                        }

                        string regValueData = string.Format("{0} -startshy", appPath);

                        runKey.SetValue(regValueName, regValueData);
                    }
                    else
                    {
                        runKey.DeleteValue(regValueName, false);
                    }
                }

                // save the setting once the operation completes successfully so, in case of error, we show
                // the correct button state the next time the settings dialog is opened
                UserSettings.DisableRunWithWindows = !run;
            }
            catch (Exception ex)
            {
                Diagnostics.LogException(new ApplicationException("Error configuring RunWhenWindowsStarts", ex));
            }
        }

        private void ShowToolbarOnDesktopCheckbox_Clicked(object sender, RoutedEventArgs e)
        {
            // try
            // {
            //     bool show = ((AriToggleSwitch)sender).IsChecked.GetValueOrDefault(false);
               
            //     if (show)
            //     {
            //         UserSettings.DisableToolWindow = false;
            //         AppManager.TheBoss.ToolWindow.ShowToolWindow(true);
            //     }
            //     else
            //     {
            //         UserSettings.DisableToolWindow = true;
            //         AppManager.TheBoss.ToolWindow.HideToolWindow();
            //         AppManager.TheBoss.TrayIcon.Activate();
            //     }
            // }
            // catch (Exception ex)
            // {
            //     Diagnostics.LogException(new ApplicationException("Error configuring ShowToolbarOnDesktop", ex));
            // }
        }

        /// <summary>
        /// Toggle the open in editor automatically option after a snip
        /// </summary>
        private void OpenEditorCheckbox_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                UserSettings.IsOpenEditorPostSnip = ((AriToggleSwitch)sender).IsChecked.GetValueOrDefault(false);
            }
            catch (Exception ex)
            {
                Diagnostics.LogException(new ApplicationException("Error configuring OpenEditorPostSnip", ex));
            }
        }

        /// <summary>
        /// Whether the user wants to see a notification popping up after a snip
        /// </summary>
        private void NotificationCheckbox_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                UserSettings.IsNotificationToastEnabled = ((AriToggleSwitch)sender).IsChecked.GetValueOrDefault(false);
            }
            catch (Exception ex)
            {
                Diagnostics.LogException(new ApplicationException("Error configuring ShowNotification", ex));
            }
        }

        /// <summary>
        /// Whether the user wants the AI to help him with auto-tagging the screenshot captured
        /// </summary>
        private void AutotaggingCheckbox_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                UserSettings.IsAutoTaggingEnabled = ((AriToggleSwitch)sender).IsChecked.GetValueOrDefault(false);
            }
            catch (Exception ex)
            {
                Diagnostics.LogException(new ApplicationException("Error configuring auto tag", ex));
            }
        }

        /// <summary>
        /// Whether the user wants enable AI services
        /// </summary>
        private void EnableAI_Clicked(object sender, RoutedEventArgs e)
        {
            UserSettings.IsAIEnabled = ((AriToggleSwitch)sender).IsChecked.GetValueOrDefault(false);
            AppManager.TheBoss.ViewModel.InsightsVisible = UserSettings.IsAIEnabled;
        }

        /// <summary>
        /// Allow the user to choose where to save the pictures automatically
        /// </summary>
        private void ChooseFolder_Clicked(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog {
                Title = "Choose your folder location",
                IsFolderPicker = true,
                EnsureFileExists = true,
                EnsureValidNames = true
            };

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                UserSettings.CustomDirectory = dlg.FileName;
            }

            UpdateLocationPreview();
        }

        private void ScreenCaptureDelaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UserSettings.ScreenCaptureDelay = (int)e.NewValue;
        }

        /// <summary>
        /// Reset the CustomDirectory to be the default directory
        /// </summary>
        private void ResetSaveLocation(object sender, EventArgs e)
        {
            UserSettings.CustomDirectory = mySnipLoc;

            UpdateLocationPreview();
        }

        /// <summary>
        /// Update the path preview on the settings panel
        /// Also update the tooltip (full path) when hovered
        /// </summary>
        private void UpdateLocationPreview()
        {
            string path = UserSettings.CustomDirectory;

            if (path.Length > maxDisplayLength)
            {
                path = "..." + path.Substring(path.Length - maxDisplayLength, maxDisplayLength);
            }

            CurrentSaveLocation.ToolTip = UserSettings.CustomDirectory;
            CurrentSaveLocation.Text = path;
        }

        /// <summary>
        /// Default path for the save location
        /// Used by the library to view/edit the screenshots
        /// By default, the path is: C:\Users\user\Documents\My Snips
        /// </summary>
        /// <returns> The default saving path as a string</returns>
        private string GetDefaultPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Snips");
        }

        /// <summary>
        /// Strength of content moderator for sharing
        /// By default, the value is 0
        /// </summary>
        private void ContentModerationStrengthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UserSettings.ContentModerationStrength = (int)e.NewValue;
        }

        /// <summary>
        /// Set the API keys used to authenticate cognnitive services.
        /// </summary>
        private void UpdateKey(TextBox t)
        {
            string oldKey = UserSettings.GetKey(t.Name);
            string value = t.Text.ToLower().Replace("https://", "");  //strip any url specific bits.

            if (value.Equals(oldKey))
            {
                return;
            }

            UserSettings.SetKey(t.Name,value);
        }

        /// <summary>
        /// Update all API keys used by application.
        /// </summary>
        private void UpdateAllKeys(object sender, RoutedEventArgs e)
        {
            UpdateKey(EntitySearch);
            UpdateKey(ImageAnalysis);
            UpdateKey(ImageSearch);
            UpdateKey(TextRecognition);
            UpdateKey(Translator);
            UpdateKey(ContentModerator);
            UpdateKey(LUISKey);
            UpdateKey(LUISAppId);

            UpdateKey(EntitySearchEndpoint);
            UpdateKey(ImageAnalysisEndpoint);
            UpdateKey(ImageSearchEndpoint);
            UpdateKey(TextRecognitionEndpoint);
            UpdateKey(TranslatorEndpoint);
            UpdateKey(ContentModeratorEndpoint);
            UpdateKey(LUISKeyEndpoint);
            MessageBox.Show(SnipInsight.Properties.Resources.Key_Restart,
                        "Info",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
            //UpdateButton.IsEnabled = false;
            //UpdateButtonKeys.IsEnabled = false;
        }

        /// <summary>
        /// Used to let the app know that text has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Key_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            //UpdateButton.IsEnabled = true;
            //UpdateButtonKeys.IsEnabled = true;
        }

        /// <summary>
        /// Changes the settings for copy to clipboard
        /// </summary>
        private void CopyToClipboardCheckbox_Clicked(object sender, RoutedEventArgs e)
        {
            UserSettings.CopyToClipboardAfterSnip = ((AriToggleSwitch)sender).IsChecked.GetValueOrDefault(false);
        }

        /// <summary>
        /// Opens the hyperlink for the corresponding key
        /// </summary>
        private void Open_HyperLink(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.UpdateSettingsControls();
        }

        private void GeneralToggle_Click(object sender, RoutedEventArgs e)
        {
            this.GeneralTab.Visibility = Visibility.Visible;
            this.DeveloperTab.Visibility = Visibility.Collapsed;
        }

        private void DeveloperToggle_Click(object sender, RoutedEventArgs e)
        {
            this.GeneralTab.Visibility = Visibility.Collapsed;
            this.DeveloperTab.Visibility = Visibility.Visible;
        }
    }
}
