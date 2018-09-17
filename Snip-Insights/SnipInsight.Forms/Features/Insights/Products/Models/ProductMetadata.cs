using System.Runtime.Serialization;

namespace SnipInsight.Forms.Features.Products.Models
{
    [DataContract]
    public class ProductMetadata
    {
        [DataMember(Name = "aggregateOffer")]
        public ProductOffer Offer { get; set; }

        [DataMember(Name = "shoppingSourcesCount")]
        public int ShoppingSourcesCount { get; set; }

        [DataMember(Name = "recipeSourcesCount")]
        public int RecipeSourcesCount { get; set; }
    }
}