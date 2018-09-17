using System.Threading.Tasks;
using Gdk;
using Gtk;
using SnipInsight.Forms.GTK.Common;
using SettingsFeature = SnipInsight.Forms.Features.Settings;
using XF = Xamarin.Forms;

namespace SnipInsight.Forms.GTK.Features.Snipping
{
    public class SnippingArea : DrawingArea
    {
        private const int SizeTooltipMarginInPixels = 2;
        private const int SizeTooltipPaddingInPixels = 6;

        private static readonly double[] SelectionRectangleDashesPattern = { 4, 4 };

        private readonly Pixbuf screenshot;
        private readonly XF.Color accentColor;
        private readonly XF.Color inverseSelectionColor;
        private readonly XF.Color textColor;
        private readonly XF.Color tooltipBackgroundColor;

        private double x0;
        private double y0;
        private double x1;
        private double y1;
        private Cairo.Rectangle? croppingRectangle;
        private int windowWidth = -1;
        private int windowHeight = -1;

        public SnippingArea(Pixbuf screenshot)
        {
            this.screenshot = screenshot;

            var resources = XF.Application.Current.Resources;
            this.accentColor = (XF.Color)resources["AccentColor"];
            this.inverseSelectionColor = (XF.Color)resources["SnippingInverseSelectionColor"];
            this.textColor = (XF.Color)resources["SnippingTextColor"];
            this.tooltipBackgroundColor = (XF.Color)resources["SnippingTooltipBackgroundColor"];

            this.AddEvents((int)EventMask.AllEventsMask);

            this.ReleasePressedXY();
        }

        protected override bool OnButtonPressEvent(EventButton evnt)
        {
            if (evnt.Button == CairoHelpers.LeftMouseButton)
            {
                this.x0 = evnt.X;
                this.y0 = evnt.Y;
            }

            return base.OnButtonPressEvent(evnt);
        }

        protected override bool OnButtonReleaseEvent(EventButton evnt)
        {
            if (evnt.Button == CairoHelpers.LeftMouseButton)
            {
                var width = this.x1 - this.x0;
                var height = this.y1 - this.y0;

                if (width > 0 && height > 0)
                {
                    this.croppingRectangle = new Cairo.Rectangle(this.x0, this.y0, width, height);
                }
                else
                {
                    this.croppingRectangle = new Cairo.Rectangle(0, 0, this.windowWidth, this.windowHeight);
                }

                this.ReleasePressedXY();
                this.QueueDraw();
            }

            return base.OnButtonReleaseEvent(evnt);
        }

        protected override bool OnExposeEvent(EventExpose evnt)
        {
            if (this.windowWidth < 0 && this.windowHeight < 0)
            {
                this.GdkWindow.GetSize(out this.windowWidth, out this.windowHeight);
            }

            using (var context = CairoHelper.Create(this.GdkWindow))
            {
                this.Draw(context);
            }

            return base.OnExposeEvent(evnt);
        }

        protected override bool OnMotionNotifyEvent(EventMotion evnt)
        {
            this.x1 = evnt.X;
            this.y1 = evnt.Y;

            if (this.x1 < 0)
            {
                this.x1 = 0;
            }

            if (this.x1 > this.windowWidth)
            {
                this.x1 = this.windowWidth;
            }

            if (this.y1 < 0)
            {
                this.y1 = 0;
            }

            if (this.y1 > this.windowHeight)
            {
                this.y1 = this.windowHeight;
            }

            this.QueueDraw();

            return base.OnMotionNotifyEvent(evnt);
        }

        protected override void OnRealized()
        {
            base.OnRealized();

            var path = System.IO.Path.Combine("Resources", "Images", "Crosshair.png");
            var crosshairPixbuf = new Pixbuf(path);
            this.GdkWindow.Cursor = new Cursor(Display.Default, crosshairPixbuf, 16, 16);
        }

        private void Crop(Cairo.Rectangle rectangle)
        {
            var normalizedRectangle = CairoHelpers.NormalizeRectangle(rectangle);
            var croppedScreenshot = new Pixbuf(
                this.screenshot,
                (int)normalizedRectangle.X,
                (int)normalizedRectangle.Y,
                (int)normalizedRectangle.Width,
                (int)normalizedRectangle.Height);

            if (croppedScreenshot == null)
            {
                return;
            }

            var snippingWindow = this.Parent as SnippingWindow;

            var imagePath = ScreenshotHelpers.SaveAndReturnPath(croppedScreenshot);

            if (!string.IsNullOrEmpty(imagePath))
            {
                if (SettingsFeature.Settings.CopyToClipboard)
                {
                    ScreenshotHelpers.CopyToClipboard(this, croppedScreenshot);
                }

                this.Close(snippingWindow);
                snippingWindow.UpdateInsightsImage(imagePath);
                snippingWindow.OpenInsights();
            }
            else
            {
                this.Close(snippingWindow);

                var md = new MessageDialog(snippingWindow.Toplevel as Gtk.Window, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, string.Empty);
                md.Text = "Error ocurred during snip. Please try again.";
                md.Response += (o, args) =>
                {
                    md.Destroy();
                };

                md.Show();
            }
        }

        private void Close(SnippingWindow snippingWindow)
        {
            snippingWindow.CloseEveryWindow();
            snippingWindow.ShowTopMenu();
        }

        private void Draw(Cairo.Context context)
        {
            CairoHelper.SetSourcePixbuf(context, this.screenshot, 0, 0);
            context.Paint();

            if (this.x0 >= 0 && this.y0 >= 0)
            {
                CairoHelpers.DrawInverseSelectionRectangles(
                    context,
                    this.inverseSelectionColor,
                    this.x0,
                    this.y0,
                    this.x1,
                    this.y1,
                    this.windowWidth,
                    this.windowHeight);
                CairoHelpers.DrawSelectionRectangle(
                    context,
                    SelectionRectangleDashesPattern,
                    this.accentColor,
                    this.x0,
                    this.y0,
                    this.x1,
                    this.y1);
                CairoHelpers.DrawSizeTooltip(
                    context,
                    this.tooltipBackgroundColor,
                    this.textColor,
                    this.x0,
                    this.y0,
                    this.x1,
                    this.y1,
                    SizeTooltipPaddingInPixels,
                    SizeTooltipMarginInPixels);
            }
            else if (this.croppingRectangle != null)
            {
                this.Crop(this.croppingRectangle.Value);
                this.croppingRectangle = null;
            }
            else
            {
                context.SetSourceRGBA(
                    this.inverseSelectionColor.R,
                    this.inverseSelectionColor.G,
                    this.inverseSelectionColor.B,
                    this.inverseSelectionColor.A);
                context.Rectangle(0, 0, this.windowWidth, this.windowHeight);
                context.Fill();
            }
        }

        private void ReleasePressedXY() => this.x0 = this.y0 = -1;
    }
}
