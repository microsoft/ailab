using System.Runtime.Serialization;

namespace SnipInsight.Forms.Features.Products.Models
{
    [DataContract]
    public class SimilarProductsModel
    {
        [DataMember(Name = "visuallySimilarProducts")]
        public ProductSearchModelList Container { get; set; }
    }
}