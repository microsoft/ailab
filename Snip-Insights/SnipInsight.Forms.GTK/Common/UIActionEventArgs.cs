using System;

namespace SnipInsight.Forms.GTK.Common
{
    public class UIActionEventArgs : EventArgs
    {
        public UIActionEventArgs(UIActions uiAction)
        {
            this.UIAction = uiAction;
        }

        public string ImagePath { get; set; }

        public UIActions UIAction { get; private set; }
    }
}