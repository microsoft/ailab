using Microsoft.Toolkit.Win32.UI.XamlHost;

namespace Speaker.Recorder.UWP
{
    public sealed partial class App : XamlApplication
    {
        public App()
        {
            this.InitializeComponent();
        }

        public void Include()
        {
            var t1 = typeof(Microsoft.Xaml.Interactivity.Behavior);
            var t2 = typeof(Microsoft.Xaml.Interactions.Core.CallMethodAction);
        }
    }
}
