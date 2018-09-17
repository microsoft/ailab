using SnipInsight.Forms.Common;
using SnipInsight.Forms.Features.Insights;
using SnipInsight.Forms.Features.Library;
using SnipInsight.Forms.Features.Settings;

namespace SnipInsight.Forms.Features.Home
{
    public class HomeViewModel : BaseViewModel
    {
        public HomeViewModel()
        {
            this.InsightsViewModel = new InsightsViewModel();
            this.LibraryViewModel = new LibraryViewModel();
            this.SettingsViewModel = new SettingsViewModel();
        }

        public InsightsViewModel InsightsViewModel { get; }

        public LibraryViewModel LibraryViewModel { get; }

        public SettingsViewModel SettingsViewModel { get; }
    }
}
