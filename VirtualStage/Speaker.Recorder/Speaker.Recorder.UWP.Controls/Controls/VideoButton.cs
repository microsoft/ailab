using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Speaker.Recorder.UWP.Controls
{
    public sealed class VideoButton : RadioButton
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string), 
            typeof(VideoButton),
            new PropertyMetadata(null));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public VideoButton()
        {
            this.DefaultStyleKey = typeof(VideoButton);
        }
    }
}
