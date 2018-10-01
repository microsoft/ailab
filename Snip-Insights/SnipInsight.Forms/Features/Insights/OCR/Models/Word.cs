using System.Runtime.Serialization;

namespace SnipInsight.Forms.Features.Insights.OCR.Models
{
    [DataContract]
    public class Word
    {
        [DataMember(Name = "boundingBox")]
        public string BoundingBox { get; set; }

        [DataMember(Name = "text")]
        public string Text { get; set; }
    }
}
