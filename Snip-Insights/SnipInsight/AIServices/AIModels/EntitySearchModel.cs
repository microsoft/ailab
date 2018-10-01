// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace SnipInsight.AIServices.AIModels
{
    public class CelebrityModel
    {
        public string Name { get; set; }

        public string Image { get; set; }

        public string URL { get; set; }

        public string Description { get; set; }

        public ObservableCollection<NewsModel> News { get; set; }
    }

    public class LandmarkModel
    {
        public string Name { get; set; }

        public string Image { get; set; }

        public string URL { get; set; }

        public string Description { get; set; }

        public string PostalCode { get; set; }

        public string Telephone { get; set; }

        public string Address { get; set; }
    }

    [DataContract]
    public class NewsModel
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "url")]
        public string URL { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "datePublished")]
        public string DatePublished { get; set; }

        [DataMember(Name = "image")]
        public NewsImage Image { get; set; }

        [DataMember(Name = "provider")]
        public List<NewsProvider> Provider { get; set; }
    }

    [DataContract]
    public class EntitySearchModel
    {
        [DataMember(Name = "entities")]
        public EntityList Entities { get; set; }

        [DataContract]
        public class EntityList
        {
            [DataMember(Name = "value")]
            public List<Entity> List { get; set; }
        }

        [DataContract]
        public class Entity
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "webSearchUrl")]
            public string URL { get; set; }

            [DataMember(Name = "image")]
            public ImageData Image { get; set; }

            [DataMember(Name = "description")]
            public string Description { get; set; }
        }

        [DataContract]
        public class ImageData
        {
            [DataMember(Name = "hostPageUrl")]
            public string URL { get; set; }
        }
    }

    [DataContract]
    public class RawNewsModel
    {
        [DataMember(Name = "value")]
        public List<NewsModel> News { get; set; }
    }

    [DataContract]
    public class NewsImage
    {
        [DataMember(Name = "thumbnail")]
        public NewsThumbnail Thumbnail { get; set; }
    }

    [DataContract]
    public class NewsThumbnail
    {
        [DataMember(Name = "contentUrl")]
        public string URL { get; set; }
    }

    [DataContract]
    public class NewsProvider
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}
