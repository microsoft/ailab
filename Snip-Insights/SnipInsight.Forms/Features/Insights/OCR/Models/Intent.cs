using System.Runtime.Serialization;

namespace SnipInsight.Forms.Features.Insights.OCR.Models
{
    [DataContract]
    public class Intent
    {
        [DataMember(Name = "intent")]
        public string IntentValue { get; set; }

        [DataMember(Name = "score")]
        public double Score { get; set; }
    }
}
