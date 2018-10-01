using System.Runtime.Serialization;

namespace SnipInsight.Forms.Features.Insights.OCR.Models
{
    [DataContract]
    public class Value
    {
        [DataMember(Name = "timex")]
        public string Timex { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "start")]
        public string Start { get; set; }

        [DataMember(Name = "end")]
        public string End { get; set; }

        [DataMember(Name = "value")]
        public string TheValue { get; set; }
    }
}
