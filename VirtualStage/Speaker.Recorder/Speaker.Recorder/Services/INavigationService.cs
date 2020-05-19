using Microsoft.Extensions.Hosting;

namespace Speaker.Recorder.Services
{
    public interface INavigationService : IHostedService
    {
        void To<TViewModel>();

        void Back();
    }
}
