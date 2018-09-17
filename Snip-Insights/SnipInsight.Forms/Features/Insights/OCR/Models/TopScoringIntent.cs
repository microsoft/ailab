using System.Runtime.Serialization;

namespace SnipInsight.Forms.Features.Insights.OCR.Models
{
    [DataContract]
    public class TopScoringIntent
    {
        [DataMember(Name = "intent")]
        public string Intent { get; set; }

        [DataMember(Name = "score")]
        public double Score { get; set; }
    }
}
