using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SnipInsight.Forms.Features.Insights.OCR.Models
{
    [DataContract]
    public class PrintedModel
    {
        [DataMember(Name = "language")]
        public string Language { get; set; }

        [DataMember(Name = "orientation")]
        public string Orientation { get; set; }

        [DataMember(Name = "textAngle")]
        public double TextAngle { get; set; }

        [DataMember(Name = "regions")]
        public List<Region> Regions { get; set; }
    }
}
