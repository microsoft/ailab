using System;
using System.Runtime.Serialization;

namespace SnipInsight.Forms.Features.Products.Models
{
    [DataContract]
    public class ProductModel
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "hostPageUrl")]
        public string HostPage { get; set; }

        [DataMember(Name = "thumbnailUrl")]
        public string Image { get; set; }

        [DataMember(Name = "insightsMetadata")]
        public ProductMetadata Metadata { get; set; }

        [DataMember(Name = "webSearchUrl")]
        public string WebSearchUrl { get; set; }

        [DataMember(Name = "datePublished")]
        public DateTime DatePublished { get; set; }

        [DataMember(Name = "contentUrl")]
        public string ContentUrl { get; set; }

        [DataMember(Name = "width")]
        public double Width { get; set; }

        [DataMember(Name = "height")]
        public double Height { get; set; }
    }
}