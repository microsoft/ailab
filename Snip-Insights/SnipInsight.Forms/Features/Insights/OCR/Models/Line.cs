using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SnipInsight.Forms.Features.Insights.OCR.Models
{
    [DataContract]
    public class Line
    {
        [DataMember(Name = "boundingBox")]
        public string BoundingBox { get; set; }

        [DataMember(Name = "words")]
        public List<Word> Words { get; set; }
    }
}
