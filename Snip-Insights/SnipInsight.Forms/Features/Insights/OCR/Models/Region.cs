using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SnipInsight.Forms.Features.Insights.OCR.Models
{
    [DataContract]
    public class Region
    {
        [DataMember(Name = "boundingBox")]
        public string BoundingBox { get; set; }

        [DataMember(Name = "lines")]
        public List<Line> Lines { get; set; }
    }
}
