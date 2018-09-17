// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Collections.Generic;
using System.Runtime.Serialization;
using GalaSoft.MvvmLight;

namespace SnipInsight.AIServices.AIModels
{

    [DataContract]
    public class ProductSearchModel : ObservableObject
    {
        /// <summary>
        /// The width of the returned image
        /// </summary>
        private double width = 0;

        /// <summary>
        /// The height of the returned image
        /// </summary>
        private double height = 0;

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "hostPageUrl")]
        public string HostPage { get; set; }

        [DataMember(Name = "thumbnailUrl")]
        public string Image { get; set; }

        [DataMember(Name = "insightsMetadata")]
        public ProductMetadata Metadata { get; set; }

        [DataMember(Name = "webSearchUrl")]
        public string WebSearchUrl { get; set; }

        [DataMember(Name = "datePublished")]
        public string DatePublished { get; set; }

        [DataMember(Name = "contentUrl")]
        public string ContentUrl { get; set; }

        [DataMember(Name = "width")]
        public double Width
        {
            get { return width; }
            set
            {
                width = value;
                RaisePropertyChanged();
            }
        }

        [DataMember(Name = "height")]
        public double Height
        {
            get { return height; }
            set
            {
                height = value;
                RaisePropertyChanged();
            }
        }
    }

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

    [DataContract]
    public class ProductSearchModelContainer
    {
        [DataMember(Name = "visuallySimilarProducts")]
        public ProductSearchModelList Container { get; set; }
    }

    [DataContract]
    public class ProductSearchModelList
    {
        [DataMember(Name = "value")]
        public List<ProductSearchModel> Products { get; set; }
    }
}