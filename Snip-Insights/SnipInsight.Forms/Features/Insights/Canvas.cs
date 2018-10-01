using System;
using System.Collections.Generic;
using System.Windows.Input;
using SnipInsight.Forms.Features.Insights.Celebrities;
using SnipInsight.Forms.Features.Insights.Drawing;
using Xamarin.Forms;

namespace SnipInsight.Forms.Features.Insights
{
    public class Canvas : View
    {
        public static readonly BindableProperty FacesProperty = BindableProperty.Create(
            nameof(Faces),
            typeof(IEnumerable<ImageAnalysisModel.FaceRectangle>),
            typeof(Canvas));

        public static readonly BindableProperty FaceSelectedCommandProperty = BindableProperty.Create(
            nameof(FaceSelectedCommand),
            typeof(ICommand),
            typeof(Canvas));

        public static readonly BindableProperty ImageProperty = BindableProperty.Create(
            nameof(Image),
            typeof(string),
            typeof(Canvas));

        public static readonly BindableProperty DrawingModeProperty = BindableProperty.Create(
           nameof(DrawingMode),
           typeof(DrawingMode),
           typeof(Canvas),
           defaultValue: DrawingMode.None);

        public static readonly BindableProperty CanUndoProperty = BindableProperty.Create(
          nameof(CanUndo),
          typeof(bool),
          typeof(Canvas),
          defaultValue: false);

        public static readonly BindableProperty CanRedoProperty = BindableProperty.Create(
         nameof(CanRedo),
         typeof(bool),
         typeof(Canvas),
         defaultValue: false);

        public static readonly BindableProperty DrawingColorProperty = BindableProperty.Create(
         nameof(DrawingColor),
         typeof(Color),
         typeof(Canvas),
         defaultValue: Color.Black);

        public static readonly BindableProperty LineWeightProperty = BindableProperty.Create(
         nameof(LineWeight),
         typeof(double),
         typeof(Canvas),
         defaultValue: 1d);

        private IDrawingActions drawer;

        public IEnumerable<ImageAnalysisModel.FaceRectangle> Faces
        {
            get { return (IEnumerable<ImageAnalysisModel.FaceRectangle>)this.GetValue(FacesProperty); }
            set { this.SetValue(FacesProperty, value); }
        }

        public ICommand FaceSelectedCommand
        {
            get { return (ICommand)this.GetValue(FaceSelectedCommandProperty); }
            set { this.SetValue(FaceSelectedCommandProperty, value); }
        }

        public string Image
        {
            get { return (string)this.GetValue(ImageProperty); }
            set { this.SetValue(ImageProperty, value); }
        }

        public DrawingMode DrawingMode
        {
            get { return (DrawingMode)this.GetValue(DrawingModeProperty); }
            set { this.SetValue(DrawingModeProperty, value); }
        }

        public bool CanUndo
        {
            get { return (bool)this.GetValue(CanUndoProperty); }
            set { this.SetValue(CanUndoProperty, value); }
        }

        public bool CanRedo
        {
            get { return (bool)this.GetValue(CanRedoProperty); }
            set { this.SetValue(CanRedoProperty, value); }
        }

        public Color DrawingColor
        {
            get { return (Color)this.GetValue(DrawingColorProperty); }
            set { this.SetValue(DrawingColorProperty, value); }
        }

        public double LineWeight
        {
            get { return (double)this.GetValue(LineWeightProperty); }
            set { this.SetValue(LineWeightProperty, value); }
        }

        public void SetDrawingActioner(IDrawingActions drawer)
        {
            this.drawer = drawer;
        }

        public void Undo()
        {
            this.drawer.Undo();
        }

        public void Redo()
        {
            this.drawer.Redo();
        }

        public void Save(string filePath)
        {
            this.drawer.Save(filePath);
        }

        public void Reset()
        {
            this.CanRedo = false;
            this.CanUndo = false;
            this.drawer.Reset();
        }
    }
}
