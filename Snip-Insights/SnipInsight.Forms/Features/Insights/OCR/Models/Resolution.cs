using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SnipInsight.Forms.Features.Insights.OCR.Models
{
        [DataContract]
        public class Resolution
        {
            [DataMember(Name = "values")]
            public List<Value> Values { get; set; }
        }
}
