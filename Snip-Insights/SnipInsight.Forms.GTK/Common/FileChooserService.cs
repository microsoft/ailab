using Gtk;
using SnipInsight.Forms.Common;
using SnipInsight.Forms.Features.Library;
using SnipInsight.Forms.Features.Localization;
using Xamarin.Forms;
using SettingsFeature = SnipInsight.Forms.Features.Settings;

[assembly: Dependency(typeof(SnipInsight.Forms.GTK.Common.FileChooserService))]

namespace SnipInsight.Forms.GTK.Common
{
    public class FileChooserService : IFileChooserService
    {
        public string ChooseFilePath()
        {
            string result = string.Empty;
            var renderer = Xamarin.Forms.Platform.GTK.Platform.GetRenderer(Xamarin.Forms.Application.Current.MainPage) as Gtk.Widget;

            var filechooser = new Gtk.FileChooserDialog(
                                                        Resources.Insights_Save_File,
                                                        renderer.Toplevel as Gtk.Window,
                                                        Gtk.FileChooserAction.Save,
                                                        Resources.Insights_Cancel_Save,
                                                        ResponseType.Cancel,
                                                        Resources.Insights_Save,
                                                        ResponseType.Accept);

            filechooser.CurrentName = string.Format($"{Resources.Insights_Save_Default_File_Name}.{Constants.ScreenshotExtension}");

            var libraryService = DependencyService.Get<ILibraryService>();

            filechooser.SetCurrentFolder(SettingsFeature.Settings.SnipsPath);

            if (filechooser.Run() == (int)ResponseType.Accept)
            {
                result = filechooser.Filename;
            }

            filechooser.Destroy();

            return result;
        }
    }
}
