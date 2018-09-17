using System;
using System.Collections.Generic;
using System.Windows.Input;
using Gtk;
using SnipInsight.Forms.Features.Insights;
using SnipInsight.Forms.Features.Insights.Celebrities;
using SnipInsight.Forms.Features.Insights.Drawing;

namespace SnipInsight.Forms.GTK.Controls
{
    public class Canvas : VBox
    {
        private readonly CanvasDrawingArea canvasDrawingArea;

        public Canvas()
        {
            this.canvasDrawingArea = new CanvasDrawingArea();
            this.canvasDrawingArea.FaceSelected += this.CanvasDrawingArea_FaceSelected;

            this.Add(this.canvasDrawingArea);
        }

        public CanvasDrawingArea CanvasDrawingArea { get => this.canvasDrawingArea; }

        public ICommand FaceSelectedCommand { get; set; }

        public void UpdateFaces(IEnumerable<ImageAnalysisModel.FaceRectangle> faces)
        {
            this.canvasDrawingArea.SetFaces(faces);
        }

        public void UpdateImagePath(string imagePath)
        {
            this.canvasDrawingArea.ImagePath = imagePath;
        }

        public void UpdateSize(int width, int height)
        {
            this.canvasDrawingArea.Width = width;
            this.canvasDrawingArea.Height = height;
        }

        public override void Dispose()
        {
            base.Dispose();

            if (this.canvasDrawingArea != null)
            {
                this.canvasDrawingArea.FaceSelected -= this.CanvasDrawingArea_FaceSelected;
            }
        }

        public void SetDrawingMode(DrawingMode drawingMode)
        {
            this.canvasDrawingArea.Mode = drawingMode;
        }

        private void CanvasDrawingArea_FaceSelected(object sender, int e)
        {
            this.FaceSelectedCommand?.Execute(e);
        }
    }
}