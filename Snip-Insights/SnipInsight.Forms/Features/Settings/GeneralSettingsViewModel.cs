using System.Collections;
using System.Linq;
using SnipInsight.Forms.Common;
using SnipInsight.Forms.Features.Localization;
using Xamarin.Forms;

namespace SnipInsight.Forms.Features.Settings
{
    public class GeneralSettingsViewModel : BaseViewModel
    {
        private readonly IList screenCaptureDelays;

        public GeneralSettingsViewModel()
        {
            this.screenCaptureDelays = Enumerable.Range(0, 6)
                .Select(delay => string.Format(Resources.Settings_Delay_FormatString, delay))
                .ToList();

            MessagingCenter.Subscribe<Messenger>(this, Messages.SettingsUpdated, _ => this.RaisePropertyChanged());
        }

        public bool AutoOpenEditor
        {
            get => Settings.AutoOpenEditor;
            set
            {
                if (Settings.AutoOpenEditor != value)
                {
                    Settings.AutoOpenEditor = value;
                    this.OnPropertyChanged(nameof(this.AutoOpenEditor));
                }
            }
        }

        public bool CopyToClipboard
        {
            get => Settings.CopyToClipboard;
            set
            {
                if (Settings.CopyToClipboard != value)
                {
                    Settings.CopyToClipboard = value;
                    this.OnPropertyChanged(nameof(this.CopyToClipboard));
                }
            }
        }

        public bool EnableAI
        {
            get => Settings.EnableAI;
            set
            {
                if (Settings.EnableAI != value)
                {
                    Settings.EnableAI = value;
                    this.OnPropertyChanged(nameof(this.EnableAI));

                    MessagingCenter.Send(Messenger.Instance, Messages.IAEnabled);
                }
            }
        }

        public IList ScreenCaptureDelays => this.screenCaptureDelays;

        public int ScreenCaptureDelaySeconds
        {
            get => Settings.ScreenCaptureDelaySeconds;
            set
            {
                if (Settings.ScreenCaptureDelaySeconds != value)
                {
                    Settings.ScreenCaptureDelaySeconds = value;
                    this.OnPropertyChanged(nameof(this.ScreenCaptureDelaySeconds));
                }
            }
        }

        public string SnipsPath
        {
            get => Settings.SnipsPath;
            set
            {
                if (Settings.SnipsPath != value)
                {
                    Settings.SnipsPath = value;
                    this.OnPropertyChanged(nameof(this.SnipsPath));
                }
            }
        }

        private void RaisePropertyChanged()
        {
            this.OnPropertyChanged(nameof(this.AutoOpenEditor));
            this.OnPropertyChanged(nameof(this.CopyToClipboard));
            this.OnPropertyChanged(nameof(this.EnableAI));
        }
    }
}
