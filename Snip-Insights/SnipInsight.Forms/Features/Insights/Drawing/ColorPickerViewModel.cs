using System;
using System.Windows.Input;
using SnipInsight.Forms.Common;
using XF = Xamarin.Forms;

namespace SnipInsight.Forms.Features.Insights.Drawing
{
    public class ColorPickerViewModel : BaseViewModel
    {
        private bool isVisible;
        private XF.Color selectedColor;
        private double lineWeight;

        private XF.Color previousColor;
        private double previousLineWeight;
        private bool isInHighLightMode;

        public ColorPickerViewModel()
        {
            this.SelectColorCommand = new XF.Command<XF.Color>(this.OnSelectColor);
            this.SelectLineWeightCommand = new XF.Command<double>(this.OnSelectLineWeight);
        }

        public ICommand SelectColorCommand { get; set; }

        public ICommand SelectLineWeightCommand { get; set; }

        public bool IsVisible
        {
            get => this.isVisible;
            set => this.SetProperty(ref this.isVisible, value);
        }

        public XF.Color SelectedColor
        {
            get => this.selectedColor;
            set => this.SetProperty(ref this.selectedColor, value);
        }

        public double LineWeight
        {
            get => this.lineWeight;
            set => this.SetProperty(ref this.lineWeight, value);
        }

        public void UndoHighLightMode()
        {
            if (this.isInHighLightMode)
            {
                this.isInHighLightMode = false;
                this.SelectedColor = this.previousColor;
                this.LineWeight = this.previousLineWeight;
            }
        }

        public void SetMarkerMode()
        {
            this.previousColor = this.SelectedColor;
            this.previousLineWeight = this.lineWeight;
            this.isInHighLightMode = true;
            this.SelectedColor = (XF.Color)XF.Application.Current.Resources["MarkerColor"];
            this.LineWeight = (double)XF.Application.Current.Resources["BrushHighlightSize"];
        }

        private void OnSelectColor(Xamarin.Forms.Color color)
        {
            this.SelectedColor = color;
        }

        private void OnSelectLineWeight(double lineWeight)
        {
            this.LineWeight = lineWeight;
        }
    }
}
