using System.Windows.Input;
using Xamarin.Forms;

namespace SnipInsight.Forms.Features.Insights.Products
{
    public partial class ProductItemTemplate : ContentView
    {
        public static readonly BindableProperty SelectCommandProperty = BindableProperty.Create(
            nameof(SelectCommand), typeof(ICommand), typeof(ProductItemTemplate), null);

        public static readonly BindableProperty NavigateCommandProperty = BindableProperty.Create(
            nameof(NavigateCommand), typeof(ICommand), typeof(ProductItemTemplate), null);

        public ProductItemTemplate()
        {
            this.InitializeComponent();
        }

        public ICommand SelectCommand
        {
            get => (ICommand)this.GetValue(SelectCommandProperty);
            set => this.SetValue(SelectCommandProperty, value);
        }

        public ICommand NavigateCommand
        {
            get => (ICommand)this.GetValue(NavigateCommandProperty);
            set => this.SetValue(NavigateCommandProperty, value);
        }
    }
}