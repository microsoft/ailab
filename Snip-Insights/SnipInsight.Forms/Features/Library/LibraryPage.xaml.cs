using Xamarin.Forms;

namespace SnipInsight.Forms.Features.Library
{
    public partial class LibraryPage : ContentPage
    {
        public LibraryPage()
        {
            this.InitializeComponent();
        }

        private LibraryViewModel ViewModel => this.BindingContext as LibraryViewModel;

        protected override void OnAppearing()
        {
            base.OnAppearing();

            this.ViewModel.Initialize();
        }
    }
}
