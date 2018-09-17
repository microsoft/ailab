using SnipInsight.Forms.Common;

namespace SnipInsight.Forms.Features.Settings
{
    public class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel()
        {
            this.GeneralSettingsViewModel = new GeneralSettingsViewModel();
            this.DeveloperSettingsViewModel = new DeveloperSettingsViewModel();
        }

        public GeneralSettingsViewModel GeneralSettingsViewModel { get; }

        public DeveloperSettingsViewModel DeveloperSettingsViewModel { get; }
    }
}
