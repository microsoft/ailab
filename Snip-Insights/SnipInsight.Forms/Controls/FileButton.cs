using Xamarin.Forms;

namespace SnipInsight.Forms.Controls
{
    public class FileButton : ContentView
    {
        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(
                                    nameof(Title),
                                    typeof(string),
                                    typeof(FileButton),
                                    string.Empty,
                                    BindingMode.TwoWay);

        public static readonly BindableProperty FileActionProperty =
            BindableProperty.Create(
                                    nameof(FileAction),
                                    typeof(FileButtonAction),
                                    typeof(FileButton),
                                    FileButtonAction.Open,
                                    BindingMode.TwoWay);

        public static readonly BindableProperty CurrentFolderProperty =
            BindableProperty.Create(
                                    nameof(CurrentFolder),
                                    typeof(string),
                                    typeof(FileButton),
                                    string.Empty,
                                    BindingMode.TwoWay);

        public static readonly BindableProperty ShowHiddenProperty =
            BindableProperty.Create(
                                    nameof(ShowHidden),
                                    typeof(bool),
                                    typeof(FileButton),
                                    false,
                                    BindingMode.TwoWay);

        public static readonly BindableProperty SelectedFileProperty =
            BindableProperty.Create(
                                    nameof(SelectedFile),
                                    typeof(string),
                                    typeof(FileButton),
                                    string.Empty,
                                    BindingMode.TwoWay);

        public string Title
        {
            get { return (string)this.GetValue(TitleProperty); }
            set { this.SetValue(TitleProperty, value); }
        }

        public string SelectedFile
        {
            get { return (string)this.GetValue(SelectedFileProperty); }
            set { this.SetValue(SelectedFileProperty, value); }
        }

        public bool ShowHidden
        {
            get { return (bool)this.GetValue(ShowHiddenProperty); }
            set { this.SetValue(ShowHiddenProperty, value); }
        }

        public string CurrentFolder
        {
            get { return (string)this.GetValue(CurrentFolderProperty); }
            set { this.SetValue(CurrentFolderProperty, value); }
        }

        public FileButtonAction FileAction
        {
            get { return (FileButtonAction)this.GetValue(FileActionProperty); }
            set { this.SetValue(FileActionProperty, value); }
        }
    }
}