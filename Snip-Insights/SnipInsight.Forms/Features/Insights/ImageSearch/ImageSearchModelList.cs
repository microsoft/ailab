using System.Collections.Generic;
using Newtonsoft.Json;

namespace SnipInsight.Forms.Features.Insights.ImageSearch
{
    public class ImageSearchModelList
    {
        [JsonProperty(PropertyName = "value")]
        public List<ImageSearchModel> Images { get; set; }
    }
}
