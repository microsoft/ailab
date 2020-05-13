using System;
using System.Collections.Generic;
using System.Text;

namespace Speaker.Recorder.ViewModels.Base
{
    public interface IPausableViewModel
    {
        void Pause();

        void Resume();
    }
}
