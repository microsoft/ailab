using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace SnipInsight.Forms.Controls
{
    public class ToggleButton : Button
    {
        public static readonly BindableProperty TooltipProperty =
            BindableProperty.Create(nameof(Tooltip), typeof(string), typeof(ToggleButton), string.Empty);

        public static readonly BindableProperty IsToggledProperty =
            BindableProperty.Create(nameof(IsToggled), typeof(bool), typeof(ToggleButton), false);

        public static readonly BindableProperty IsKeepActiveProperty =
            BindableProperty.Create(nameof(IsKeepActive), typeof(bool), typeof(ToggleButton), false);

        public event EventHandler IsToggledChanged;

        public string Tooltip
        {
            get { return (string)this.GetValue(TooltipProperty); }
            set { this.SetValue(TooltipProperty, value); }
        }

        public bool IsKeepActive
        {
            get
            {
                return (bool)this.GetValue(IsKeepActiveProperty);
            }

            set
            {
                 this.SetValue(IsKeepActiveProperty, value);
            }
        }

        public bool IsToggled
        {
            get
            {
                return (bool)this.GetValue(IsToggledProperty);
            }

            set
            {
                if (value != this.IsToggled)
                {
                    this.SetValue(IsToggledProperty, value);
                }
            }
        }

        public void SendIsToggledChanged()
        {
            this.IsToggledChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}