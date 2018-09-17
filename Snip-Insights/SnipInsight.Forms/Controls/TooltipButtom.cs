using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace SnipInsight.Forms.Controls
{
    public class TooltipButton : Button
    {
        public static readonly BindableProperty TooltipProperty =
            BindableProperty.Create(nameof(Tooltip), typeof(string), typeof(TooltipButton), string.Empty);

        public string Tooltip
        {
            get { return (string)this.GetValue(TooltipProperty); }
            set { this.SetValue(TooltipProperty, value); }
        }
    }
}