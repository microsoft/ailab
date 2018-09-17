using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SnipInsight.Forms.Features.Insights.Celebrities
{
    [DataContract]
    public class RawNewsModel
    {
        [DataMember(Name = "value")]
        public List<NewsModel> News { get; set; }

        [DataContract]
        public class NewsImage
        {
            [DataMember(Name = "thumbnail")]
            public NewsThumbnail Thumbnail { get; set; }
        }

        [DataContract]
        public class NewsThumbnail
        {
            [DataMember(Name = "contentUrl")]
            public string URL { get; set; }
        }

        [DataContract]
        public class NewsProvider
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }
        }
    }
}
