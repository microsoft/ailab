using System.ComponentModel;
using SnipInsight.Forms.Features.Insights;
using SnipInsight.Forms.GTK.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;

[assembly: ExportRenderer(typeof(Canvas), typeof(CanvasRenderer))]

namespace SnipInsight.Forms.GTK.Renderers
{
    public class CanvasRenderer : ViewRenderer<Canvas, Controls.Canvas>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Canvas> e)
        {
            if (e.NewElement != null)
            {
                if (this.Control == null)
                {
                    this.SetNativeControl(new Controls.Canvas());
                    this.Control.FaceSelectedCommand = this.Element.FaceSelectedCommand;
                    this.Element.SetDrawingActioner(this.Control.CanvasDrawingArea);
                    this.Control.CanvasDrawingArea.SetCanvas(this.Element);
                }

                this.SetSize();
            }

            base.OnElementChanged(e);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Canvas.FacesProperty.PropertyName)
            {
                this.Control.UpdateFaces(this.Element.Faces);
            }
            else if (e.PropertyName == Canvas.FaceSelectedCommandProperty.PropertyName)
            {
                this.Control.FaceSelectedCommand = this.Element.FaceSelectedCommand;
            }
            else if (e.PropertyName == Canvas.ImageProperty.PropertyName)
            {
                this.Control.UpdateImagePath(this.Element.Image);
            }
            else if (e.PropertyName == Canvas.DrawingModeProperty.PropertyName)
            {
                this.Control.SetDrawingMode(this.Element.DrawingMode);
            }
            else if (e.PropertyName == Canvas.DrawingColorProperty.PropertyName)
            {
                this.Control.CanvasDrawingArea.SetDrawingColor(this.Element.DrawingColor);
            }
            else if (e.PropertyName == Canvas.LineWeightProperty.PropertyName)
            {
                this.Control.CanvasDrawingArea.SetLineWeight(this.Element.LineWeight);
            }

            base.OnElementPropertyChanged(sender, e);
        }

        protected override void OnSizeAllocated(Gdk.Rectangle allocation)
        {
            this.SetSize();

            base.OnSizeAllocated(allocation);
        }

        private void SetSize()
        {
            int width = this.WidthRequest;
            int height = this.HeightRequest;

            this.Control.UpdateSize(width, height);
        }
    }
}
