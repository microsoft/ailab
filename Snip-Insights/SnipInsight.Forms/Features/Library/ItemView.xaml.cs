using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace SnipInsight.Forms.Features.Library
{
    public partial class ItemView : ContentView
    {
        public static readonly BindableProperty DeleteCommandProperty = BindableProperty.Create(
            nameof(DeleteCommand), typeof(ICommand), typeof(ItemView), null);

        public static readonly BindableProperty OpenInsightsCommandProperty = BindableProperty.Create(
            nameof(OpenInsightsCommand), typeof(ICommand), typeof(ItemView), null);

        public ItemView()
        {
            this.InitializeComponent();
        }

        public ICommand DeleteCommand
        {
            get => (ICommand)this.GetValue(DeleteCommandProperty);
            set => this.SetValue(DeleteCommandProperty, value);
        }

        public ICommand OpenInsightsCommand
        {
            get => (ICommand)this.GetValue(OpenInsightsCommandProperty);
            set => this.SetValue(OpenInsightsCommandProperty, value);
        }
    }
}
