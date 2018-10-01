using System;
using System.Diagnostics;
using Gdk;
using Gtk;
using SnipInsight.Forms.Common;
using SnipInsight.Forms.Features.Library;
using SettingsFeature = SnipInsight.Forms.Features.Settings;
using XF = Xamarin.Forms;

namespace SnipInsight.Forms.GTK.Features.Snipping
{
    public static class ScreenshotHelpers
    {
        public static void CopyToClipboard(DrawingArea drawingArea, Pixbuf screenshot)
        {
            var clipboard = drawingArea.GetClipboard(Gdk.Selection.Clipboard);

            // FIXME macOS: can't paste directly on Outlook or Word —can on Gimp
            clipboard.Image = screenshot;
        }

        public static string SaveAndReturnPath(Pixbuf screenshot)
        {
            var libraryService = XF.DependencyService.Get<ILibraryService>();
            var date = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            var path = System.IO.Path.Combine(
                SettingsFeature.Settings.SnipsPath,
                $"capture{date}.{Constants.ScreenshotExtension}");

            try
            {
                // FIXME macOS: some colors are wrong
                screenshot.Save(path, Constants.ScreenshotExtension);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            try
            {
                screenshot.Save(path, Constants.ScreenshotExtension);
                return path;
            }
            catch (GLib.GException)
            {
                //// : Couldn't allocate memory for loading JPEG file
            }

            return string.Empty;
        }

        public static Pixbuf Take(Rectangle monitor)
        {
            var rootWindow = Screen.Default.RootWindow;
            var screenshot = Pixbuf.FromDrawable(
                rootWindow, rootWindow.Colormap, monitor.X, monitor.Y, 0, 0, monitor.Width, monitor.Height);

            return screenshot;
        }
    }
}
