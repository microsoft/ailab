using System.Runtime.Serialization;

namespace SnipInsight.Forms.Features.Insights.OCR.Models
{
    [DataContract]
    public class Entity
    {
        [DataMember(Name = "entity")]
        public string TheEntity { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "startIndex")]
        public int StartIndex { get; set; }

        [DataMember(Name = "endIndex")]
        public int EndIndex { get; set; }

        [DataMember(Name = "resolution")]
        public Resolution Resolution { get; set; }
    }
}
