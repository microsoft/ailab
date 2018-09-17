using System.Runtime.Serialization;

namespace SnipInsight.Forms.Features.Products.Models
{
    [DataContract]
    public class ProductRating
    {
        [DataMember(Name = "ratingValue")]
        public double RatingValue { get; set; }

        [DataMember(Name = "bestRating")]
        public double BestRating { get; set; }

        [DataMember(Name = "ratingCount")]
        public int RatingCount { get; set; }
    }
}