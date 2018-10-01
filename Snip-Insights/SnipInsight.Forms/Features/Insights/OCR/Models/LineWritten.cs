using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SnipInsight.Forms.Features.Insights.OCR.Models
{
    [DataContract]
    public class LineWritten
    {
        [DataMember(Name = "boundingBox")]
        public List<int> BoundingBox { get; set; }

        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "words")]
        public List<WordWritten> Words { get; set; }
    }
}
