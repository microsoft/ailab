using SnipInsight.Forms.Common;
using Xamarin.Forms;

namespace SnipInsight.Forms.Features.Home
{
    public partial class HomePage
    {
        public HomePage()
        {
            this.InitializeComponent();
        }

        public HomeViewModel ViewModel => this.BindingContext as HomeViewModel;

        protected override void OnAppearing()
        {
            base.OnAppearing();

            MessagingCenter.Subscribe<Messenger>(this, Messages.OpenInsights, this.OpenInsights);
            MessagingCenter.Subscribe<Messenger>(this, Messages.OpenLibrary, this.OpenLibrary);
            MessagingCenter.Subscribe<Messenger>(this, Messages.OpenSettings, this.OpenSettings);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            MessagingCenter.Unsubscribe<Messenger>(this, Messages.OpenInsights);
            MessagingCenter.Unsubscribe<Messenger>(this, Messages.OpenLibrary);
            MessagingCenter.Unsubscribe<Messenger>(this, Messages.OpenSettings);
        }

        private void OpenInsights(Messenger obj)
        {
            this.RefreshLibrary();
            this.SelectedItem = this.insightsTab.BindingContext;
        }

        private void OpenLibrary(Messenger obj)
        {
            this.RefreshLibrary();
            this.SelectedItem = this.libraryTab.BindingContext;
        }

        private void OpenSettings(Messenger obj)
        {
            this.RefreshLibrary();
            this.SelectedItem = this.settingsTab.BindingContext;
        }

        private void RefreshLibrary()
        {
            this.ViewModel.LibraryViewModel.Initialize();
        }
    }
}
