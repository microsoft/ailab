using SnipInsight.Forms.Features.Home;
using SnipInsight.Forms.Features.Insights;

namespace SnipInsight.Forms.Common
{
    public static class ViewModelsFactory
    {
        static ViewModelsFactory()
        {
            HomeViewModel = new HomeViewModel();
        }

        public static HomeViewModel HomeViewModel { get; set; }
    }
}
