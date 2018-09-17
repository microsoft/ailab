using System.Diagnostics;
using System.Reflection;

namespace SnipInsight.Forms.Features.Settings
{
    public partial class GeneralSettingsPage
    {
        public GeneralSettingsPage()
        {
            this.InitializeComponent();

            this.VersionTextBlock.Text = this.GetInformationVersion();
            this.ProductVersionTextBlock.Text = this.GetProductInformationVersion();
        }

        public string GetInformationVersion()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            return FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
        }

        public string GetProductInformationVersion()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            return FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
        }
    }
}
