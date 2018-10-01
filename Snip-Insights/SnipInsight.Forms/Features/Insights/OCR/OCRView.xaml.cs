using System.Windows.Input;
using Xamarin.Forms;

namespace SnipInsight.Forms.Features.Insights.OCR
{
    public partial class OCRView : ContentView
    {
        public static readonly BindableProperty CopyCommandProperty = BindableProperty.Create(
            nameof(CopyCommand), typeof(ICommand), typeof(OCRView), null);

        public OCRView()
        {
            this.InitializeComponent();
        }

        public ICommand CopyCommand
        {
            get => (ICommand)this.GetValue(CopyCommandProperty);
            set => this.SetValue(CopyCommandProperty, value);
        }

        private async void To_Picker_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            await this.DoTranslation();
        }

        private async void From_Picker_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            await this.DoTranslation();
        }

        private async System.Threading.Tasks.Task DoTranslation()
        {
            var viewModel = (OCRViewModel)this.BindingContext;
            await viewModel.TranslateTo((string)this.pickerTranslateFrom.SelectedItem, (string)this.pickerTranslateTo.SelectedItem);
        }
    }
}