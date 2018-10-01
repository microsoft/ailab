using Newtonsoft.Json;

namespace SnipInsight.Forms.Features.Insights.ImageSearch
{
    public class ImageSearchModelContainer
    {
        [JsonProperty(PropertyName = "visuallySimilarImages")]
        public ImageSearchModelList Container { get; set; }
    }
}
