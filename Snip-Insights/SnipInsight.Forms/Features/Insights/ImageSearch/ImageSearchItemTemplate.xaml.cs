using System.Windows.Input;
using Xamarin.Forms;

namespace SnipInsight.Forms.Features.Insights.ImageSearch
{
    public partial class ImageSearchItemTemplate : ContentView
    {
        public static readonly BindableProperty SelectCommandProperty = BindableProperty.Create(
            nameof(SelectCommand), typeof(ICommand), typeof(ImageSearchItemTemplate), null);

        public ImageSearchItemTemplate()
        {
            this.InitializeComponent();
        }

        public ICommand SelectCommand
        {
            get => (ICommand)this.GetValue(SelectCommandProperty);
            set => this.SetValue(SelectCommandProperty, value);
        }
    }
}