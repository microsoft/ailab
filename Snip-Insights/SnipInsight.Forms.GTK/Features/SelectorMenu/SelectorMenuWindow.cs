using System;
using Gtk;
using SnipInsight.Forms.Common;
using SnipInsight.Forms.Features.Localization;
using SnipInsight.Forms.GTK.Common;

namespace SnipInsight.Forms.GTK.Features.SelectorMenu
{
    public class SelectorMenuWindow : Window, IUIActionAware
    {
        private HBox container;
        private Button openEditorButton;
        private Button openFolderButton;
        private Button dismissButton;

        private string currentPath;

        private int initialXPosition;
        private int initialYPosition;

        public SelectorMenuWindow()
           : base(WindowType.Toplevel)
        {
            this.Decorated = false;

            this.SetIconFromFile(Constants.IconPath);

            this.CreateInterface();

            this.AddEvents((int)Gdk.EventMask.AllEventsMask);
        }

        public event EventHandler<UIActionEventArgs> UIActionSelected;

        protected override void OnShown()
        {
            base.OnShown();

            this.KeepAbove = true;

            var width = this.Allocation.Width;
            var height = this.Allocation.Height;
            var monitorNumber = this.Screen.GetMonitorAtWindow(this.GdkWindow);
            var monitor = this.Screen.GetMonitorGeometry(monitorNumber);

            this.initialXPosition = monitor.X + monitor.Width - width;
            this.initialYPosition = (int)(monitor.Y + (monitor.Height * 0.95f) - height);

            this.Move(this.initialXPosition, this.initialYPosition);
        }

        private void CreateInterface()
        {
            this.container = new HBox(true, 5);
            this.container.HeightRequest = 80;
            this.container.WidthRequest = 350;

            this.openEditorButton = Button.NewWithLabel(Resources.Config_OpenEditor);
            this.openEditorButton.Accessible.Name = Resources.Config_OpenEditor;
            this.openEditorButton.SetSizeRequest(100, 32);
            this.openEditorButton.Clicked += (sender, e) =>
            {
                this.UIActionSelected?.Invoke(this, new UIActionEventArgs(UIActions.OpenEditor) { ImagePath = this.currentPath });
                this.HideAll();
            };

            this.container.PackStart(this.openEditorButton, false, false, 4);

            this.openFolderButton = Button.NewWithLabel(Resources.Open_Library_Folder);
            this.openFolderButton.Accessible.Name = Resources.Open_Library_Folder;
            this.openFolderButton.SetSizeRequest(100, 32);
            this.openFolderButton.Clicked += (sender, e) =>
            {
                this.UIActionSelected?.Invoke(this, new UIActionEventArgs(UIActions.OpenLibraryFolder));
                this.HideAll();
            };

            this.container.PackStart(this.openFolderButton, false, false, 4);

            this.dismissButton = Button.NewWithLabel(Resources.Dismiss);
            this.dismissButton.Accessible.Name = Resources.Dismiss;
            this.dismissButton.SetSizeRequest(100, 32);
            this.dismissButton.Clicked += (sender, e) =>
            {
                this.UIActionSelected?.Invoke(this, new UIActionEventArgs(UIActions.CloseSelectorMenuWindow));
                this.HideAll();
            };

            this.container.PackStart(this.dismissButton, false, false, 4);

            this.Add(this.container);
            this.container.ShowAll();
        }

        public void ShowWithArguments(string imagePath)
        {
            this.currentPath = imagePath;
            this.ShowAll();
        }
    }
}
