using System;
using Gtk;
using SnipInsight.Forms.Common;
using SnipInsight.Forms.Features.Localization;
using SnipInsight.Forms.GTK.Common;

namespace SnipInsight.Forms.GTK.Features.TopMenu
{
    public class TopMenuWindow : Window, IUIActionAware
    {
        private HBox container;
        private Image logoImage;
        private Button snipButton;
        private Button toggleButton;
        private Button libraryButton;
        private Button settingsButton;
        private Button closeButton;
        private PopupWindow popupWindow;
        private int initialXPosition;
        private int initialYPosition;
        private int currentX;
        private int currentY;
        private bool windowsIsBeingPressed = false;
        private int offsetX;
        private int offsetY;

        public TopMenuWindow()
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
            this.initialYPosition = (int)(monitor.Y + (monitor.Height * 0.1));

            this.currentX = this.initialXPosition;
            this.currentY = this.initialYPosition;

            this.Move(this.initialXPosition, this.initialYPosition);
        }

        protected override bool OnButtonPressEvent(Gdk.EventButton evnt)
        {
            this.windowsIsBeingPressed = true;

            if (evnt.Button == CairoHelpers.LeftMouseButton)
            {
                this.offsetX = (int)evnt.X;
                this.offsetY = (int)evnt.Y;
            }

            return base.OnButtonPressEvent(evnt);
        }

        protected override bool OnButtonReleaseEvent(Gdk.EventButton evnt)
        {
            this.windowsIsBeingPressed = false;
            return base.OnButtonReleaseEvent(evnt);
        }

        protected override bool OnMotionNotifyEvent(Gdk.EventMotion evnt)
        {
            if (this.windowsIsBeingPressed && evnt.Type == Gdk.EventType.MotionNotify)
            {
                var x = (int)evnt.X;
                var y = (int)evnt.Y;

                this.currentX += x - this.offsetX;
                this.currentY += y - this.offsetY;

                this.Move(this.currentX, this.currentY);
            }

            return base.OnMotionNotifyEvent(evnt);
        }

        private void CreateInterface()
        {
            this.container = new HBox(false, 0);

            var logo = new Gdk.Pixbuf(Constants.LogoPath, 32, 32, true);
            this.logoImage = new Image(logo);
            this.logoImage.SetPadding(4, 4);
            this.container.PackStart(this.logoImage, false, false, 4);

            this.snipButton = Button.NewWithLabel(Resources.TopMenuWindow_Snip);
            this.snipButton.Accessible.Name = "Snip";
            this.snipButton.SetSizeRequest(64, 32);
            this.snipButton.Clicked += (sender, e) =>
             {
                 this.popupWindow?.Hide();
                 this.UIActionSelected?.Invoke(this, new UIActionEventArgs(UIActions.Snipping));
             };

            this.container.PackStart(this.snipButton, false, false, 4);

            this.toggleButton = this.CreateButton("Toggle Button", "Toggle.png", xPadding: 0, yPadding: 0);
            this.toggleButton.Clicked += this.ShowToggles;

            this.settingsButton = this.CreateButton("Settings Button", "Settings.png", UIActions.Settings, xPadding: 5, yPadding: 5);

            this.libraryButton = this.CreateButton(
                "Library Button",
                "Library.png",
                UIActions.Library,
                imageHeight: 18,
                xPadding: 5,
                yPadding: 7);

            this.closeButton = this.CreateButton(
                "Close Button",
                "Close.png",
                UIActions.Exit,
                imageWidth: 16,
                imageHeight: 16,
                xPadding: 5,
                lateralPadding: 10,
                alignEnd: true);

            this.Add(this.container);
            this.container.ShowAll();
        }

        private Button CreateButton(
            string name,
            string path,
            UIActions? uiAction = null,
            int imageWidth = 22,
            int imageHeight = 22,
            int xPadding = 0,
            int yPadding = 0,
            uint lateralPadding = 4,
            bool alignEnd = false)
        {
            var imagePath = System.IO.Path.Combine("Resources", "Images", path);
            var pixBuf = new Gdk.Pixbuf(imagePath, imageWidth, imageHeight, false);

            var image = new Image(pixBuf);
            image.SetPadding(xPadding, yPadding);
            var button = new Button
            {
                BorderWidth = 0,
            };

            button.Accessible.Name = name;

            button.Add(image);

            button.WidthRequest = 32;
            button.HeightRequest = 32;

            if (uiAction != null)
            {
                button.Clicked += (sender, e) =>
                {
                    this.popupWindow?.Hide();
                    this.UIActionSelected?.Invoke(this, new UIActionEventArgs(uiAction.Value));
                };
            }

            if (alignEnd)
            {
                this.container.PackEnd(button, false, false, lateralPadding);
            }
            else
            {
                this.container.PackStart(button, false, false, lateralPadding);
            }

            return button;
        }

        private void ShowToggles(object sender, EventArgs e)
        {
            this.GetPosition(out int x, out int y);

            var height = ((Button)sender).Allocation.Size.Height;

            if (this.popupWindow == null)
            {
                this.popupWindow = new PopupWindow();
            }

            this.popupWindow.Move(x + this.snipButton.Allocation.X + this.snipButton.Allocation.Width, y + height);
            this.popupWindow.Present();
        }
    }
}