using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SnipInsight.Forms.Features.Insights.Celebrities
{
    [DataContract]
    public class EntitySearchModel
    {
        [DataMember(Name = "entities")]
        public EntityList Entities { get; set; }

        [DataContract]
        public class EntityList
        {
            [DataMember(Name = "value")]
            public List<Entity> List { get; set; }
        }

        [DataContract]
        public class Entity
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "webSearchUrl")]
            public string URL { get; set; }

            [DataMember(Name = "image")]
            public ImageData Image { get; set; }

            [DataMember(Name = "description")]
            public string Description { get; set; }
        }

        [DataContract]
        public class ImageData
        {
            [DataMember(Name = "hostPageUrl")]
            public string URL { get; set; }
        }
    }
}
