using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cairo;
using Gdk;
using Gtk;
using SnipInsight.Forms.Common;
using SnipInsight.Forms.Features.Insights.Celebrities;
using SnipInsight.Forms.Features.Insights.Drawing;
using SnipInsight.Forms.GTK.Common;
using SnipInsight.Forms.GTK.Features.Drawing;
using SnipInsight.Forms.GTK.Features.Snipping;
using Xamarin.Forms;
using XF = Xamarin.Forms;

namespace SnipInsight.Forms.GTK.Controls
{
    public class CanvasDrawingArea : DrawingArea, IDrawingActions
    {
        private readonly Cairo.Color backgroundColor;
        private readonly Cairo.Color strokeColor;

        private Cairo.Color drawingColor;
        private double drawingLineWeight;

        private IEnumerable<Cairo.Rectangle> faces;
        private int height;
        private Forms.Features.Insights.Canvas formsCanvas;

        private string imagePath;
        private int indexFaceOver = -1;
        private double ratio;
        private int width;
        private List<DrawedPath> completedPath;
        private DrawedPath inProgressPath;
        private bool userIsDrawing;
        private Gdk.Size pixbufSize;
        private List<DrawedPath> undonedPath;

        private string lastOCRtext = string.Empty;

        public CanvasDrawingArea()
        {
            this.QueueResize();

            var windowDarkBackgroundColor = (XF.Color)XF.Application.Current.Resources["WindowDarkBackgroundColor"];
            this.backgroundColor = new Cairo.Color(
                windowDarkBackgroundColor.R, windowDarkBackgroundColor.G, windowDarkBackgroundColor.B);

            var accentColor = (XF.Color)XF.Application.Current.Resources["AccentColor"];
            this.strokeColor = new Cairo.Color(accentColor.R, accentColor.G, accentColor.B);

            this.drawingColor = new Cairo.Color(0, 0, 0);
            this.drawingLineWeight = (double)XF.Application.Current.Resources["BrushLargeSize"];

            this.AddEvents((int)EventMask.AllEventsMask);

            this.completedPath = new List<DrawedPath>();
            this.undonedPath = new List<DrawedPath>();
            this.inProgressPath = new DrawedPath();

            MessagingCenter.Subscribe<Messenger>(this, Messages.CopyToClipboard, this.OnCopyClipboard);
            MessagingCenter.Subscribe<Messenger>(this, Messages.CopyTextToClipboard, this.OnTextCopyClipboard);
            MessagingCenter.Subscribe<Messenger, string>(this, Messages.OCRText, this.OnOCRText);
        }

        public event EventHandler<int> FaceSelected;

        public int Height
        {
            get => this.height;
            set
            {
                this.height = value;
                this.QueueDraw();
            }
        }

        public string ImagePath
        {
            get => this.imagePath;

            set
            {
                this.imagePath = value;
                this.faces = null;
            }
        }

        public int Width
        {
            get => this.width;
            set
            {
                this.width = value;
                this.QueueDraw();
            }
        }

        public DrawingMode Mode { get; internal set; }

        public void SetDrawingColor(XF.Color xamfColor)
        {
            this.drawingColor = new Cairo.Color(xamfColor.R, xamfColor.G, xamfColor.B, xamfColor.A);
        }

        public void SetLineWeight(double lineWeight)
        {
            this.drawingLineWeight = lineWeight;
        }

        public void SetFaces(IEnumerable<ImageAnalysisModel.FaceRectangle> faces)
        {
            this.faces = faces.Select(face => new Cairo.Rectangle(
                face.Left * this.ratio,
                face.Top * this.ratio,
                face.Width * this.ratio,
                face.Height * this.ratio));
            this.QueueDraw();
        }

        public void SetCanvas(Forms.Features.Insights.Canvas canvas)
        {
            this.formsCanvas = canvas;
        }

        public void Undo()
        {
            if (this.completedPath.Any())
            {
                this.undonedPath.Add(this.completedPath.Last());
                this.completedPath.RemoveAt(this.completedPath.Count - 1);

                this.UpdateCanUnDoRedo();
            }

            this.QueueDraw();
        }

        public void Redo()
        {
            if (this.undonedPath.Any())
            {
                this.completedPath.Add(this.undonedPath.Last());
                this.undonedPath.RemoveAt(this.undonedPath.Count - 1);

                this.UpdateCanUnDoRedo();
            }

            this.QueueDraw();
        }

        public void Save(string filePath)
        {
            Pixbuf pixbuf = this.GeneratePixBufWithPaths();

            pixbuf.Save(filePath, Constants.ScreenshotExtension);
        }

        public void Reset()
        {
            this.completedPath.Clear();
            this.QueueDraw();
        }

        protected override bool OnButtonPressEvent(EventButton evnt)
        {
            if (evnt.Button == CairoHelpers.LeftMouseButton)
            {
                this.SelectFace(evnt.X, evnt.Y);

                if (this.Mode == DrawingMode.Drawing)
                {
                    this.StartDrawing();
                }
            }

            return base.OnButtonPressEvent(evnt);
        }

        protected override bool OnButtonReleaseEvent(EventButton evnt)
        {
            if (evnt.Button == CairoHelpers.LeftMouseButton)
            {
                switch (this.Mode)
                {
                    case DrawingMode.None:
                        break;
                    case DrawingMode.Drawing:
                        this.EndDrawing();
                        break;
                    case DrawingMode.Erasing:
                        var scaledPoint = new PointD(evnt.X / this.pixbufSize.Width, evnt.Y / this.pixbufSize.Height);
                        this.EraseNearestPathTo(scaledPoint);
                        break;
                    default:
                        break;
                }

                this.ClearFaceOverAndQueueDraw();
            }

            return base.OnButtonReleaseEvent(evnt);
        }

        protected override bool OnExposeEvent(EventExpose evnt)
        {
            using (var context = CairoHelper.Create(this.GdkWindow))
            {
                this.Draw(context);
            }

            return true;
        }

        protected override bool OnMotionNotifyEvent(EventMotion evnt)
        {
            this.ProcessDrawingMode(evnt);

            if (this.faces != null)
            {
                var isFaceMatched = false;

                for (int i = 0; i < this.faces.Count(); i++)
                {
                    var face = this.faces.ElementAt(i);

                    if (CairoHelpers.IsInside(evnt.X, evnt.Y, face))
                    {
                        this.indexFaceOver = i;
                        this.QueueDraw();
                        isFaceMatched = true;
                        break;
                    }
                }

                if (!isFaceMatched)
                {
                    this.ClearFaceOverAndQueueDraw();
                }
            }

            return base.OnMotionNotifyEvent(evnt);
        }

        private void OnOCRText(Messenger sender, string ocrText)
        {
            this.lastOCRtext = ocrText;
        }

        private void UpdateCanUnDoRedo()
        {
            this.formsCanvas.CanUndo = this.completedPath.Any();
            this.formsCanvas.CanRedo = this.undonedPath.Any();
        }

        private void ProcessDrawingMode(EventMotion evnt)
        {
            if (this.Mode == DrawingMode.Drawing)
            {
                if (this.userIsDrawing
                    && evnt.X < this.pixbufSize.Width
                    && evnt.Y < this.pixbufSize.Height)
                {
                    var x = evnt.X / this.pixbufSize.Width;
                    var y = evnt.Y / this.pixbufSize.Height;

                    var newPoint = new PointD(x, y);

                    this.inProgressPath.Points.Add(newPoint);

                    this.QueueDraw();

                    this.formsCanvas.CanUndo = true;
                }
            }
        }

        private void ClearFaceOverAndQueueDraw()
        {
            this.indexFaceOver = -1;
            this.QueueDraw();
        }

        private void Draw(Context context)
        {
            context.Rectangle(0, 0, this.width, this.height);
            context.SetSourceRGB(this.backgroundColor.R, this.backgroundColor.G, this.backgroundColor.B);
            context.Fill();

            if (this.DrawImage(context))
            {
                if (this.userIsDrawing)
                {
                    this.DrawSnipPath(context, this.inProgressPath);
                }
                else
                {
                    this.DrawFacesRectangles(context);
                }

                this.DrawStrokes(context);
            }
        }

        private bool DrawImage(Context context)
        {
            bool res = true;
            if (!string.IsNullOrWhiteSpace(this.imagePath))
            {
                try
                {
                    using (var pixbuf = new Pixbuf(this.imagePath))
                    {
                        var size = CairoHelpers.GetAspectFitSize(pixbuf.Width, pixbuf.Height, this.width, this.height);
                        this.ratio = size.Width / (double)pixbuf.Width;

                        using (var pixbufScaled = pixbuf.ScaleSimple(size.Width, size.Height, InterpType.Bilinear))
                        {
                            CairoHelper.SetSourcePixbuf(context, pixbufScaled, 0, 0);

                            this.pixbufSize = new Gdk.Size(pixbufScaled.Width, pixbufScaled.Height);
                            context.Paint();
                        }
                    }
                }
                catch (Exception ex)
                {
                    res = false;
                    Debug.WriteLine(ex.ToString());
                }
            }

            return res;
        }

        private void DrawFacesRectangles(Context context)
        {
            if (this.indexFaceOver >= 0
                && this.faces != null)
            {
                // TODO rewrite with CairoHelper.SetSourceColor()
                context.SetSourceRGB(this.strokeColor.R, this.strokeColor.G, this.strokeColor.B);
                var face = this.faces.ElementAt(this.indexFaceOver);
                context.Rectangle(face);
                context.Stroke();
            }
        }

        private void DrawStrokes(Context context)
        {
            foreach (var snipPath in this.completedPath)
            {
                this.DrawSnipPath(context, snipPath);
            }
        }

        private void DrawSnipPath(Context context, DrawedPath snipPath)
        {
            context.Save();

            var color = snipPath.Color;
            context.SetSourceRGBA(color.R, color.G, color.B, color.A);
            context.Scale(this.pixbufSize.Width, this.pixbufSize.Height);
            context.LineWidth = snipPath.LineWeight;
            context.LineCap = LineCap.Round;

            foreach (var point in snipPath.Points)
            {
                context.LineTo(point);
            }

            context.Stroke();

            context.Restore();
        }

        private void OnCopyClipboard(Messenger messenger)
        {
            var screenshot = this.GeneratePixBufWithPaths();

            ScreenshotHelpers.CopyToClipboard(this, screenshot);
        }

        private void OnTextCopyClipboard(Messenger messenger)
        {
            if (!string.IsNullOrEmpty(this.lastOCRtext))
            {
                ClipboardHelper.CopyToClipboard(this.lastOCRtext);
            }
        }

        private Pixbuf GeneratePixBufWithPaths()
        {
            var width = this.pixbufSize.Width;
            var height = this.pixbufSize.Height;

            var rootWindow = Screen.Default.RootWindow;
            Gdk.Pixmap pixmap = new Gdk.Pixmap(rootWindow, width, height, rootWindow.Depth);

            using (var context = CairoHelper.Create(pixmap))
            {
                this.Draw(context);
            }

            var pixbuf = Pixbuf.FromDrawable(pixmap, rootWindow.Colormap, 0, 0, 0, 0, width, height);
            return pixbuf;
        }

        private void StartDrawing()
        {
            this.userIsDrawing = true;
            this.inProgressPath = new DrawedPath()
            {
                Color = this.drawingColor,
                LineWeight = this.drawingLineWeight
            };
        }

        private void EndDrawing()
        {
            this.userIsDrawing = false;
            this.completedPath.Add(this.inProgressPath);
        }

        private void SelectFace(double x, double y)
        {
            if (this.faces != null)
            {
                for (int i = 0; i < this.faces.Count(); i++)
                {
                    var face = this.faces.ElementAt(i);

                    if (CairoHelpers.IsInside(x, y, face))
                    {
                        this.FaceSelected?.Invoke(this, i);
                        break;
                    }
                }
            }
        }

        private void EraseNearestPathTo(PointD point)
        {
            if (this.completedPath != null
                && this.completedPath.Any())
            {
                var minimumDiscante = double.MaxValue;
                var indexToRemove = -1;
                var eraseOffset = 0.05;

                for (int i = 0; i < this.completedPath.Count; i++)
                {
                    var path = this.completedPath[i];
                    foreach (var pathPoint in path.Points)
                    {
                        var distance = CairoHelpers.GetDistance(point, pathPoint);

                        if (distance < eraseOffset
                            && distance < minimumDiscante)
                        {
                            indexToRemove = i;
                            minimumDiscante = distance;
                        }
                    }
                }

                if (indexToRemove >= 0)
                {
                    this.undonedPath.Add(this.completedPath[indexToRemove]);
                    this.completedPath.RemoveAt(indexToRemove);
                }
            }

            this.UpdateCanUnDoRedo();
        }
    }
}