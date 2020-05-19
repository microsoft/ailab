using Microsoft.Toolkit.Win32.UI.XamlHost;
using Speaker.Recorder.Helpers;
using System;
using System.Diagnostics;

namespace Speaker.Recorder
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            NvApiLoader.TryLoad();

            var splashBackground = new System.Windows.SplashScreen("Resources/Splash_Screen.png");
            splashBackground.Show(false, !Debugger.IsAttached);

            var uwpApp = new UWP.App();
            var application = new App();
            application.InitializeComponent();
            application.Loaded += (object sender, EventArgs e) =>
            {
                splashBackground.Close(TimeSpan.FromMilliseconds(300));
            };
            application.Run();
        }
    }
}
