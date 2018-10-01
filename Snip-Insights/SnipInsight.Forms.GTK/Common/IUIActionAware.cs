using System;

namespace SnipInsight.Forms.GTK.Common
{
    public interface IUIActionAware
    {
        event EventHandler<UIActionEventArgs> UIActionSelected;
    }
}
