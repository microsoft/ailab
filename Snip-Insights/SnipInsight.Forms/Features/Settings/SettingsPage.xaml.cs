using System;
using SnipInsight.Forms.Common;
using Xamarin.Forms;

namespace SnipInsight.Forms.Features.Settings
{
    public partial class SettingsPage
    {
        public SettingsPage()
        {
            this.InitializeComponent();

            this.BindingContextChanged += this.SettingsPage_BindingContextChanged;

            MessagingCenter.Subscribe<Messenger>(this, Messages.IAEnabled, this.IAEnabled);
        }

        private void SettingsPage_BindingContextChanged(object sender, EventArgs e)
        {
            if (this.BindingContext != null)
            {
                this.AddSettingsTab();
            }
        }

        private void AddSettingsTab()
        {
            var settingsViewModel = (SettingsViewModel)this.BindingContext;

            var generalPage = new GeneralSettingsPage()
            {
                BindingContext = settingsViewModel.GeneralSettingsViewModel,
            };

            var developerPage = new DeveloperSettingsPage()
            {
                BindingContext = settingsViewModel.DeveloperSettingsViewModel,
            };

            this.Children.Clear();

            this.Children.Add(generalPage);

            if (Settings.EnableAI)
            {
                this.Children.Add(developerPage);
            }
        }

        private void IAEnabled(Messenger obj)
        {
            this.AddSettingsTab();
        }
    }
}
