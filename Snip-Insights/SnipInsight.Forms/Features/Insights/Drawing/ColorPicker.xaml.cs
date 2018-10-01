using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XF = Xamarin.Forms;

namespace SnipInsight.Forms.Features.Insights.Drawing
{
    public partial class ColorPicker : ContentView
    {
        private List<Button> colorButtons;

        private List<Button> weightButtons;

        public ColorPicker()
        {
            this.InitializeComponent();

            this.colorButtons = new List<Button>();
            this.colorButtons.Add(this.blackButton);
            this.colorButtons.Add(this.greenButton);
            this.colorButtons.Add(this.yellowButton);
            this.colorButtons.Add(this.blueButton);
            this.colorButtons.Add(this.redButton);

            this.weightButtons = new List<Button>();
            this.weightButtons.Add(this.line1Button);
            this.weightButtons.Add(this.line2Button);
            this.weightButtons.Add(this.line3Button);
            this.weightButtons.Add(this.line4Button);
        }

        private ColorPickerViewModel ViewModel => this.BindingContext as ColorPickerViewModel;

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            this.ViewModel.PropertyChanged -= this.ViewModel_PropertyChanged;
            this.ViewModel.PropertyChanged += this.ViewModel_PropertyChanged;

            this.ViewModel.SelectedColor = XF.Color.Black;
            this.ViewModel.LineWeight = (double)XF.Application.Current.Resources["BrushLargeSize"];
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.ViewModel.SelectedColor))
            {
                this.UpdateColorButtons(this.ViewModel.SelectedColor);
            }
            else if (e.PropertyName == nameof(this.ViewModel.LineWeight))
            {
                this.UpdateLineWeightButtons(this.ViewModel.LineWeight);
            }
        }

        private void UpdateColorButtons(Color selectedColor)
        {
            var button = this.colorButtons.FirstOrDefault(b => b.BackgroundColor == selectedColor);
            if (button != null)
            {
                this.ResetColorButtonsBorders();

                button.BorderWidth = 3;
            }
        }

        private void UpdateLineWeightButtons(double lineWeight)
        {
            var button = this.weightButtons.FirstOrDefault(b => b.CommandParameter.Equals(lineWeight));

            if (button != null)
            {
                this.ResetLineWeightButtonsBorders();

                button.BorderWidth = 3;
            }
        }

        private void ResetColorButtonsBorders()
        {
            this.blackButton.BorderWidth = 0;
            this.redButton.BorderWidth = 0;
            this.yellowButton.BorderWidth = 0;
            this.greenButton.BorderWidth = 0;
            this.blueButton.BorderWidth = 0;
        }

        private void ResetLineWeightButtonsBorders()
        {
            this.line1Button.BorderWidth = 0;
            this.line2Button.BorderWidth = 0;
            this.line3Button.BorderWidth = 0;
            this.line4Button.BorderWidth = 0;
        }
    }
}