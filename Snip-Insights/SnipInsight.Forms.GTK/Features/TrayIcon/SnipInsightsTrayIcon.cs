using System;
using Gdk;
using Gtk;
using SnipInsight.Forms.Common;
using SnipInsight.Forms.Features.Localization;
using SnipInsight.Forms.GTK.Common;

namespace SnipInsight.Forms.GTK.Features.TrayIcon
{
    public class SnipInsightsTrayIcon : StatusIcon, IUIActionAware
    {
        private readonly Menu popupMenu;

        public SnipInsightsTrayIcon()
            : base(new Pixbuf(Constants.IconPath))
        {
            this.popupMenu = new Menu();

            this.CreateMenuItem(Resources.TrayIcon_ContextMenuItem_NewCapture, UIActions.Snipping);
            this.CreateMenuItem(Resources.TrayIcon_ContextMenuItem_Library, UIActions.Library);
            this.CreateMenuItem(Resources.TrayIcon_ContextMenuItem_Settings, UIActions.Settings);
            this.CreateMenuItem(Resources.TrayIcon_ContextMenuItem_Exit, UIActions.Exit);
        }

        public event EventHandler<UIActionEventArgs> UIActionSelected;

        protected override void OnPopupMenu(uint button, uint activate_time)
        {
            base.OnPopupMenu(button, activate_time);

            this.popupMenu.ShowAll();
            this.popupMenu.Popup();
        }

        private void CreateMenuItem(string label, UIActions uiAction)
        {
            var item = new MenuItem(label);
            item.Activated += (sender, e) => this.UIActionSelected?.Invoke(this, new UIActionEventArgs(uiAction));

            this.popupMenu.Add(item);
            this.popupMenu.Add(new SeparatorMenuItem());
        }
    }
}
