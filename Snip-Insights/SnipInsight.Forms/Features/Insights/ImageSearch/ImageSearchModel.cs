using Newtonsoft.Json;

namespace SnipInsight.Forms.Features.Insights.ImageSearch
{
    public class ImageSearchModel
    {
        [JsonProperty(PropertyName = "contentUrl")]
        public string URL { get; set; }
    }
}
