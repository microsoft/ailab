using System.Runtime.Serialization;

namespace SnipInsight.Forms.Features.Insights.OCR.Models
{
    [DataContract]
    public class HandWrittenModel
    {
        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "recognitionResult")]
        public RecognitionResult RecognitionResult { get; set; }
    }
}
