using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using SnipInsight.Forms.Common;
using SnipInsight.Forms.GTK.Common;
using SnipInsight.Forms.GTK.Features.SelectorMenu;
using SnipInsight.Forms.GTK.Features.Snipping;
using SnipInsight.Forms.GTK.Features.TopMenu;
using SnipInsight.Forms.GTK.Features.TrayIcon;
using Xamarin.Forms.Platform.GTK.Controls;
using Xamarin.Forms.Platform.GTK.Extensions;
using SettingsFeature = SnipInsight.Forms.Features.Settings;
using XF = Xamarin.Forms;

namespace SnipInsight.Forms.GTK
{
    public class Program
    {
        private static HideableFormsWindow formsWindow;
        private static TopMenuWindow topMenuWindow;
        private static SelectorMenuWindow selectorMenuWindow;

        [STAThread]
        public static void Main(string[] args)
        {
#if MACOS
            MacOSScreenshotHelpers.InitializeXamarinMacEnvironmentUnderMacOS();
#endif
            LoadDarkTheme();

            Application.Init();

            CreateSelectorMenuWindow();
            CreateAndShowTopMenuWindow();

            GLib.Idle.Add(() =>
            {
                CreateAndShowTrayIcon();
                return false;
            });

            GLib.Idle.Add(() =>
            {
                CreateFormsWindow();
                return false;
            });

            Application.Run();
        }

        private static void CreateSelectorMenuWindow()
        {
            selectorMenuWindow = new SelectorMenuWindow();
            selectorMenuWindow.SetIconFromFile(Constants.IconPath);
            selectorMenuWindow.UIActionSelected += UIActionSelected;
        }

        private static void CreateAndShowTopMenuWindow()
        {
            topMenuWindow = new TopMenuWindow();
            topMenuWindow.SetIconFromFile(Constants.IconPath);
            topMenuWindow.UIActionSelected += UIActionSelected;

            topMenuWindow.ShowAll();
        }

        private static void CreateAndShowTrayIcon()
        {
            var trayIcon = new SnipInsightsTrayIcon
            {
                Visible = true
            };
            trayIcon.UIActionSelected += UIActionSelected;
        }

        private static void CreateFormsWindow()
        {
            XF.Forms.Init();

            var app = new App();

            var color = (XF.Color)XF.Application.Current.Resources["WindowDarkBackgroundColor"];
            ListView.DefaultSelectionColor = color.ToGtkColor();

            formsWindow = new HideableFormsWindow
            {
                WindowPosition = WindowPosition.Center,
                DefaultWidth = 1000
            };
            formsWindow.SetIconFromFile(Constants.IconPath);
            formsWindow.LoadApplication(app);
            formsWindow.SetApplicationTitle(Constants.Title);
        }

        private static void LoadDarkTheme()
        {
            var filename = "gtkrc-dark";

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                filename = "gtkrc-dark.Windows";
            }

            var themeAbsolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Themes/{filename}");
            Rc.Parse(themeAbsolutePath);
        }

        private static void OpenHome()
        {
            formsWindow.ShowAll();
        }

        private static void OpenSnipping()
        {
            formsWindow.HideAll();
            topMenuWindow.HideAll();
            selectorMenuWindow.HideAll();

            Task.Factory.StartNew(
                async () =>
            {
                var seconds = SettingsFeature.Settings.ScreenCaptureDelaySeconds;

                if (seconds == 0)
                {
                    // Give UI some time to hide top menu
                    await Task.Delay(TimeSpan.FromSeconds(0.25f));
                }
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(seconds));
                }

                SnippingWindow.ShowInEveryMonitor(UIActionSelected);
            },
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        private static void UIActionSelected(object sender, UIActionEventArgs args)
        {
            switch (args.UIAction)
            {
                case UIActions.Snipping:
                    OpenSnipping();
                    break;
                case UIActions.Insights:
                    if (SettingsFeature.Settings.AutoOpenEditor)
                    {
                        OpenHome();
                        XF.MessagingCenter.Send(Messenger.Instance, Messages.OpenInsights);
                    }

                    break;
                case UIActions.InsightsImage:
                    if (SettingsFeature.Settings.AutoOpenEditor)
                    {
                        XF.MessagingCenter.Send(Messenger.Instance, Messages.UpdateInsightsImage, args.ImagePath);
                    }
                    else
                    {
                        selectorMenuWindow.ShowWithArguments(args.ImagePath);
                    }

                    break;
                case UIActions.Library:
                    OpenHome();
                    XF.MessagingCenter.Send(Messenger.Instance, Messages.OpenLibrary);
                    break;
                case UIActions.Settings:
                    OpenHome();
                    XF.MessagingCenter.Send(Messenger.Instance, Messages.OpenSettings);
                    break;
                case UIActions.Exit:
                    Application.Quit();
                    break;
                case UIActions.TopMenu:
                    topMenuWindow.ShowAll();
                    break;
                case UIActions.OpenEditor:
                    OpenHome();
                    XF.MessagingCenter.Send(Messenger.Instance, Messages.OpenInsights);
                    XF.MessagingCenter.Send(Messenger.Instance, Messages.UpdateInsightsImage, args.ImagePath);
                    break;
                case UIActions.OpenLibraryFolder:
                    XF.MessagingCenter.Send(Messenger.Instance, Messages.OpenLibraryFolder);
                    break;
                case UIActions.CloseSelectorMenuWindow:
                    selectorMenuWindow.HideAll();
                    break;
                default:
                    break;
            }
        }
    }
}
