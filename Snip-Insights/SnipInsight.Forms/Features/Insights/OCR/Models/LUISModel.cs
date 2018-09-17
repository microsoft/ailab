using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SnipInsight.Forms.Features.Insights.OCR.Models
{
    [DataContract]
    public partial class LUISModel
    {
        [DataMember(Name = "query")]
        public string Query { get; set; }

        [DataMember(Name = "topScoringIntent")]
        public TopScoringIntent TheTopScoringIntent { get; set; }

        [DataMember(Name = "intents")]
        public List<Intent> Intents { get; set; }

        [DataMember(Name = "entities")]
        public List<Entity> Entities { get; set; }
    }
}
