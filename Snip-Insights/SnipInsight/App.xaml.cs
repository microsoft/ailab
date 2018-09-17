// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using SnipInsight.ViewModels;
using System.Windows;

namespace SnipInsight
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
    {
        void Application_Startup(object sender, StartupEventArgs e)
        {
            ViewModelLocator locator = (ViewModelLocator)Application.Current.Resources["Locator"];

            Util.UserSettings.DisableToolWindow = false;
            Util.UserSettings.AppPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            Telemetry.ApplicationLogger.Instance.Initialize();
            Telemetry.ApplicationLogger.Instance.SubmitEvent(Telemetry.EventName.SnipApplicationInitialized);

            AppManager.TheBoss.Run(e.Args);
        }
    }
}
