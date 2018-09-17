using System;
using System.ComponentModel;
using System.Linq;
using SnipInsight.Forms.Common;
using SnipInsight.Forms.Controls;
using SnipInsight.Forms.GTK.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;

[assembly: ExportRenderer(typeof(FileButton), typeof(FileButtonRenderer))]

namespace SnipInsight.Forms.GTK.Renderers
{
    public class FileButtonRenderer : ViewRenderer<FileButton, Gtk.FileChooserButton>
    {
        private bool disposed;
        private Gtk.FileChooserButton fileChooserButton;

        protected override void OnElementChanged(ElementChangedEventArgs<FileButton> e)
        {
            if (this.Control == null)
            {
                this.fileChooserButton = new Gtk.FileChooserButton(string.Empty, Gtk.FileChooserAction.Open);
                this.Add(this.fileChooserButton);

                this.fileChooserButton.ShowAll();

                this.fileChooserButton.FileSet += this.FileChooserButton_FileSet;

                // This handles folders choosen within the dialog
                this.fileChooserButton.CurrentFolderChanged += this.FileChooserButton_CurrentFolderChanged;

                // And this those selected within the drop-down menu
                this.fileChooserButton.SelectionChanged += this.FileChooserButton_CurrentFolderChanged;
                this.SetNativeControl(this.fileChooserButton);
            }

            if (e.NewElement != null)
            {
                this.UpdateTitle();
                this.UpdateFileAction();
                this.UpdateCurrentFolder();
                this.UpdateShowHidden();
            }

            base.OnElementChanged(e);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == FileButton.TitleProperty.PropertyName)
            {
                this.UpdateTitle();
            }
            else if (e.PropertyName == FileButton.FileActionProperty.PropertyName)
            {
                this.UpdateFileAction();
            }
            else if (e.PropertyName == FileButton.CurrentFolderProperty.PropertyName)
            {
                this.UpdateCurrentFolder();
            }
            else if (e.PropertyName == FileButton.ShowHiddenProperty.PropertyName)
            {
                this.UpdateShowHidden();
            }

            base.OnElementPropertyChanged(sender, e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !this.disposed)
            {
                this.disposed = true;
            }

            base.Dispose(disposing);
        }

        private void UpdateTitle()
        {
            if (this.fileChooserButton != null)
            {
                this.fileChooserButton.Title = this.Element.Title;
            }
        }

        private void UpdateFileAction()
        {
            if (this.fileChooserButton != null)
            {
                var fileAction = this.Element.FileAction;

                switch (fileAction)
                {
                    case FileButtonAction.Open:
                        this.fileChooserButton.Action = Gtk.FileChooserAction.Open;
                        break;
                    case FileButtonAction.CreateFolder:
                        this.fileChooserButton.Action = Gtk.FileChooserAction.CreateFolder;
                        break;
                    case FileButtonAction.Save:
                        this.fileChooserButton.Action = Gtk.FileChooserAction.Save;
                        break;
                    case FileButtonAction.SelectFolder:
                        this.fileChooserButton.Action = Gtk.FileChooserAction.SelectFolder;
                        break;
                }
            }
        }

        private void UpdateCurrentFolder()
        {
            if (this.fileChooserButton != null)
            {
                this.fileChooserButton.SetCurrentFolder(this.Element.CurrentFolder);
            }
        }

        private void UpdateShowHidden()
        {
            if (this.fileChooserButton != null)
            {
                this.fileChooserButton.ShowHidden = this.Element.ShowHidden;
            }
        }

        private void FileChooserButton_FileSet(object sender, EventArgs e)
        {
            var button = (Gtk.FileChooserButton)sender;
            var filepath = button.Filename;

            if (this.IsAValidFile(filepath))
            {
                this.Element.SelectedFile = filepath;
            }
            else
            {
                button.UnselectFilename(filepath);
                this.Element.SelectedFile = string.Empty;
            }
        }

        private void FileChooserButton_CurrentFolderChanged(object sender, EventArgs e)
        {
            var button = (Gtk.FileChooserButton)sender;
            this.Element.CurrentFolder = button.Filename;
        }

        private bool IsAValidFile(string filepath)
        {
            var valid = false;
            var extension = System.IO.Path.GetExtension(filepath).ToLowerInvariant();

            if (Constants.ValidExtensions.Contains(extension))
            {
                valid = true;
            }

            return valid;
        }
    }
}