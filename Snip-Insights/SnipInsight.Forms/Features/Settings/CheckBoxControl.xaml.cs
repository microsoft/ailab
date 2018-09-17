using Xamarin.Forms;

namespace SnipInsight.Forms.Features.Settings
{
    public partial class CheckBoxControl : ContentView
    {
        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text), typeof(string), typeof(CheckBoxControl), null);

        public static readonly BindableProperty SettingProperty =
            BindableProperty.Create(nameof(Setting), typeof(bool), typeof(CheckBoxControl), false, BindingMode.TwoWay);

        public CheckBoxControl()
        {
            this.InitializeComponent();
        }

        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        public bool Setting
        {
            get { return (bool)this.GetValue(SettingProperty); }
            set { this.SetValue(SettingProperty, value); }
        }
    }
}