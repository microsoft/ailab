using System.Collections.Generic;
using System.Runtime.Serialization;
using static SnipInsight.Forms.Features.Insights.Celebrities.RawNewsModel;

namespace SnipInsight.Forms.Features.Insights.Celebrities
{
    [DataContract]
    public class NewsModel
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "url")]
        public string URL { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "datePublished")]
        public string DatePublished { get; set; }

        [DataMember(Name = "image")]
        public NewsImage Image { get; set; }

        [DataMember(Name = "provider")]
        public List<NewsProvider> Provider { get; set; }
    }
}
