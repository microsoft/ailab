using System;
using Gtk;
using SnipInsight.Forms.Common;
using SnipInsight.Forms.Features.Localization;
using SettingsFeature = SnipInsight.Forms.Features.Settings;
using XF = Xamarin.Forms;

namespace SnipInsight.Forms.GTK.Features.TopMenu
{
    public class PopupWindow : Window
    {
        private readonly VBox container;
        private readonly CheckButton enableAICheckButton;
        private readonly CheckButton copyToClipboardCheckButton;
        private readonly CheckButton autoOpenEditorCheckButton;

        public PopupWindow()
            : base(WindowType.Popup)
        {
            XF.MessagingCenter.Subscribe<Messenger>(this, Messages.SettingsUpdated, _ => this.UpdateSettings());

            this.KeepAbove = true;
            this.Decorated = false;

            this.container = new VBox(true, 0);

            this.enableAICheckButton = this.AddCheckBox(
                Resources.Config_EnableAI,
                SettingsFeature.Settings.EnableAI,
                this.ToggleEnableAI);
            this.copyToClipboardCheckButton = this.AddCheckBox(
                Resources.Config_CopyClipboard,
                SettingsFeature.Settings.CopyToClipboard,
                this.ToggleCopyToClipboard);
            this.autoOpenEditorCheckButton = this.AddCheckBox(
                Resources.Config_OpenEditor,
                SettingsFeature.Settings.AutoOpenEditor,
                this.ToggleAutoOpenEditor);

            this.Add(this.container);
            this.ShowAll();
        }

        private void ToggleEnableAI(object sender, EventArgs args) =>
            SettingsFeature.Settings.EnableAI = !SettingsFeature.Settings.EnableAI;

        private void ToggleCopyToClipboard(object sender, EventArgs args) =>
            SettingsFeature.Settings.CopyToClipboard = !SettingsFeature.Settings.CopyToClipboard;

        private void ToggleAutoOpenEditor(object sender, EventArgs args) =>
            SettingsFeature.Settings.AutoOpenEditor = !SettingsFeature.Settings.AutoOpenEditor;

        private void UpdateSettings()
        {
            if (this.enableAICheckButton.Active != SettingsFeature.Settings.EnableAI)
            {
                this.enableAICheckButton.Toggled -= this.ToggleEnableAI;
                this.enableAICheckButton.Active = SettingsFeature.Settings.EnableAI;
                this.enableAICheckButton.Toggled += this.ToggleEnableAI;
            }

            if (this.copyToClipboardCheckButton.Active != SettingsFeature.Settings.CopyToClipboard)
            {
                this.copyToClipboardCheckButton.Toggled -= this.ToggleCopyToClipboard;
                this.copyToClipboardCheckButton.Active = SettingsFeature.Settings.CopyToClipboard;
                this.copyToClipboardCheckButton.Toggled += this.ToggleCopyToClipboard;
            }

            if (this.autoOpenEditorCheckButton.Active != SettingsFeature.Settings.AutoOpenEditor)
            {
                this.autoOpenEditorCheckButton.Toggled -= this.ToggleAutoOpenEditor;
                this.autoOpenEditorCheckButton.Active = SettingsFeature.Settings.AutoOpenEditor;
                this.autoOpenEditorCheckButton.Toggled += this.ToggleAutoOpenEditor;
            }
        }

        private CheckButton AddCheckBox(string label, bool isToggled, EventHandler handler)
        {
            var checkButton = new CheckButton(label);
            checkButton.Active = isToggled;
            checkButton.Accessible.Name = label;

            checkButton.Toggled += handler;
            this.container.PackStart(checkButton, false, true, 1);

            return checkButton;
        }
    }
}
