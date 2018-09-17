using System;
using System.ComponentModel;
using Gdk;
using SnipInsight.Forms.Controls;
using SnipInsight.Forms.GTK.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;

[assembly: ExportRenderer(typeof(ToggleButton), typeof(ToggleButtonRenderer))]

namespace SnipInsight.Forms.GTK.Renderers
{
    public class ToggleButtonRenderer : ViewRenderer<ToggleButton, Gtk.ToggleButton>
    {
        private bool disposed;
        private Gtk.ToggleButton toggleButton;

        protected override void OnElementChanged(ElementChangedEventArgs<ToggleButton> e)
        {
            if (this.Control == null)
            {
                this.toggleButton = new Gtk.ToggleButton(this.Element.Text);
                this.toggleButton.Inconsistent = false;

                this.toggleButton.Toggled += this.OnToggled;

                this.Add(this.toggleButton);

                this.toggleButton.ShowAll();

                this.SetNativeControl(this.toggleButton);
            }

            if (e.NewElement != null)
            {
                this.UpdateIsToggled();
            }

            base.OnElementChanged(e);
        }

        protected override bool OnClientEvent(EventClient evnt)
        {
            return base.OnClientEvent(evnt);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ToggleButton.IsToggledProperty.PropertyName)
            {
                this.UpdateIsToggled();
            }
            else if (e.PropertyName == ToggleButton.TextProperty.PropertyName)
            {
                this.UpdateText();
            }
            else if (e.PropertyName == ToggleButton.TooltipProperty.PropertyName)
            {
                this.UpdateTooltip();
            }
            else if (e.PropertyName == ToggleButton.HeightProperty.PropertyName)
            {
                this.UpdateHeight();
            }

            base.OnElementPropertyChanged(sender, e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !this.disposed)
            {
                this.disposed = true;

                if (this.Control != null)
                {
                    this.Control.Toggled -= this.OnToggled;
                }
            }

            base.Dispose(disposing);
        }

        private void UpdateIsToggled()
        {
            if (this.toggleButton != null)
            {
                // ElementController.SetValueFromRenderer(ToggleButton.IsToggledProperty, this.toggleButton.Active);
                this.toggleButton.Toggled -= this.OnToggled;
                this.toggleButton.Active = this.Element.IsToggled;
                this.toggleButton.Toggled += this.OnToggled;
            }
        }

        private void UpdateText()
        {
            if (this.toggleButton != null)
            {
                this.toggleButton.Label = this.Element.Text;
            }
        }

        private void UpdateTooltip()
        {
            if (this.toggleButton != null)
            {
                this.toggleButton.TooltipText = this.Element.Tooltip;
            }
        }

        private void UpdateHeight()
        {
            this.Control.HeightRequest = (int)this.Element.HeightRequest;
        }

        private void OnToggled(object sender, System.EventArgs e)
        {
            var nativeToggleButton = sender as Gtk.ToggleButton;

            if (this.Element.IsKeepActive && !nativeToggleButton.Active)
            {
                nativeToggleButton.Active = true;
            }
            else
            {
                this.Element.SendIsToggledChanged();
                if (nativeToggleButton != null)
                {
                    this.Element.IsToggled = nativeToggleButton.Active;
                    this.Element.Command?.Execute(this.Element.IsToggled);
                }
            }
        }
    }
}