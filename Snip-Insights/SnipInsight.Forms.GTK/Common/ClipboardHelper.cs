using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnipInsight.Forms.GTK.Common
{
    public static class ClipboardHelper
    {
        public static void CopyToClipboard(string text)
        {
            Gtk.Clipboard clipboard = Gtk.Clipboard.Get(Gdk.Atom.Intern("CLIPBOARD", false));
            clipboard.Text = text;
        }
    }
}
