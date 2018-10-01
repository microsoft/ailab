using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Gtk;
using SnipInsight.Forms.Features.Localization;
using SnipInsight.Forms.GTK.Common;

namespace SnipInsight.Forms.GTK.Features.Snipping
{
    public class SnippingWindow : Window, IUIActionAware
    {
        private static List<Window> windows;

        private SnippingWindow(Gdk.Rectangle monitor, Gdk.Pixbuf screenshot)
            : base(WindowType.Toplevel)
        {
            this.Title = Resources.Choose_an_area;
            this.Decorated = false;
            this.DefaultSize = new Gdk.Size(monitor.Width, monitor.Height);

            this.Move(monitor.X, monitor.Y);

            var snippingArea = new SnippingArea(screenshot);
            this.Add(snippingArea);

            this.KeyPressEvent += this.CloseOnEscape;
        }

        public event EventHandler<UIActionEventArgs> UIActionSelected;

        public static void ShowInEveryMonitor(Action<object, UIActionEventArgs> handler)
        {
            windows = new List<Window>();
#if MACOS
            var xamarinMacScreenshots = new List<(string path, CoreGraphics.CGRect bounds)>();
            xamarinMacScreenshots = MacOSScreenshotHelpers.Take();
#endif

            var displayManager = Gdk.DisplayManager.Get();
            var displays = displayManager.ListDisplays();

            foreach (var display in displays)
            {
                for (int i = 0; i < display.NScreens; i++)
                {
                    var screen = display.GetScreen(i);
                    var monitors = screen.NMonitors;

                    for (int j = 0; j < monitors; j++)
                    {
                        var monitor = screen.GetMonitorGeometry(j);
                        Gdk.Pixbuf screenshot;

#if MACOS
                        screenshot = FirstMatchingScreenshot(monitor, xamarinMacScreenshots);

                        if (screenshot.Width != monitor.Width || screenshot.Height != monitor.Height)
                        {
                            var scaled = screenshot.ScaleSimple(
                            monitor.Width,
                            monitor.Height,
                            Gdk.InterpType.Bilinear);
                            screenshot = scaled;
                        }
#else
                        screenshot = ScreenshotHelpers.Take(monitor);
#endif
                        CreateSnippingWindow(monitor, screenshot, handler);
                    }
                }
            }
        }

        public void CloseEveryWindow()
        {
            foreach (var window in windows)
            {
                window.Destroy();
            }
        }

        public void OpenInsights()
        {
            this.UIActionSelected?.Invoke(this, new UIActionEventArgs(UIActions.Insights));
        }

        public void ShowTopMenu()
        {
            this.UIActionSelected?.Invoke(this, new UIActionEventArgs(UIActions.TopMenu));
        }

        public void UpdateInsightsImage(string imagePath)
        {
            var args = new UIActionEventArgs(UIActions.InsightsImage) { ImagePath = imagePath };
            this.UIActionSelected?.Invoke(this, args);
        }

        private static void CreateSnippingWindow(
            Gdk.Rectangle monitor, Gdk.Pixbuf screenshot, Action<object, UIActionEventArgs> handler)
        {
            var snippingWindow = new SnippingWindow(monitor, screenshot);
            snippingWindow.UIActionSelected += (sender, args) => handler(sender, args);
            snippingWindow.ShowAll();

            windows.Add(snippingWindow);
        }

#if MACOS
        private static Gdk.Pixbuf FirstMatchingScreenshot(
            Gdk.Rectangle monitor, List<(string path, CoreGraphics.CGRect bounds)> xamarinMacScreenshots)
        {
#if DEBUG
            Console.WriteLine($"Monitor: {monitor}");

            foreach (var item in xamarinMacScreenshots)
            {
                Console.WriteLine($"Bound: {item.bounds}");
            }
#endif

            var match = xamarinMacScreenshots.First(
            screenshot => screenshot.bounds.Width == monitor.Width && screenshot.bounds.Height == monitor.Height);

            return new Gdk.Pixbuf(match.path);
         }
#endif

        private void CloseOnEscape(object o, KeyPressEventArgs args)
        {
            if (args.Event.Key == Gdk.Key.Escape)
            {
                this.CloseEveryWindow();
            }

            this.ShowTopMenu();
        }
    }
}
