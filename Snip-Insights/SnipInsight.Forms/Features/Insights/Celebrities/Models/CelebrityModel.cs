using System.Collections.ObjectModel;

namespace SnipInsight.Forms.Features.Insights.Celebrities
{
    public class CelebrityModel
    {
        public string Name { get; set; }

        public string Image { get; set; }

        public string URL { get; set; }

        public string Description { get; set; }

        public ObservableCollection<NewsModel> News { get; set; }
    }
}
