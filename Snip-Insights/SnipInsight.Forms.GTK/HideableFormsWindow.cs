using Gdk;
using Xamarin.Forms.Platform.GTK;

namespace SnipInsight.Forms.GTK
{
    public class HideableFormsWindow : FormsWindow
    {
        protected override bool OnDeleteEvent(Event evnt)
        {
            this.HideAll();

            return true;
        }
    }
}
