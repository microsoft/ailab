using SnipInsight.Forms.Features.Home;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace SnipInsight.Forms
{
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();

            //// Plugin.Settings.CrossSettings.Current.Clear();

            this.MainPage = new HomePage();
        }
    }
}
