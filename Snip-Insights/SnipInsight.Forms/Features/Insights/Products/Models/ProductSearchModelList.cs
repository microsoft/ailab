using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SnipInsight.Forms.Features.Products.Models
{
    [DataContract]
    public class ProductSearchModelList
    {
        [DataMember(Name = "value")]
        public List<ProductModel> Products { get; set; }
    }
}