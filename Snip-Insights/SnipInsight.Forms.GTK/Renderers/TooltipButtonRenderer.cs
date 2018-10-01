using System;
using System.ComponentModel;
using SnipInsight.Forms.Controls;
using SnipInsight.Forms.GTK.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;
using Xamarin.Forms.Platform.GTK.Renderers;

[assembly: ExportRenderer(typeof(TooltipButton), typeof(TooltipButtonRenderer))]

namespace SnipInsight.Forms.GTK.Renderers
{
    public class TooltipButtonRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                this.UpdateTooltip();
            }
        }

        private void UpdateTooltip()
        {
            if (this.Control != null)
            {
                this.Control.TooltipText = (this.Element as TooltipButton).Tooltip;
            }
        }
    }
}