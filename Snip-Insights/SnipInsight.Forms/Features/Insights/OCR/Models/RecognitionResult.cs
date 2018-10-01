using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SnipInsight.Forms.Features.Insights.OCR.Models
{
    [DataContract]
    public class RecognitionResult
    {
        [DataMember(Name = "lines")]
        public List<LineWritten> Lines { get; set; }
    }
}
