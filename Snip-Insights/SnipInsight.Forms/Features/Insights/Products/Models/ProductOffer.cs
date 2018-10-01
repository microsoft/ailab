using System.Runtime.Serialization;

namespace SnipInsight.Forms.Features.Products.Models
{
    [DataContract]
    public class ProductOffer
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "priceCurrency")]
        public string PriceCurrency { get; set; }

        [DataMember(Name = "aggregateRating")]
        public ProductRating Rating { get; set; }

        [DataMember(Name = "lowPrice")]
        public double Price { get; set; }

        [DataMember(Name = "offerCount")]
        public int OfferCount { get; set; }
    }
}