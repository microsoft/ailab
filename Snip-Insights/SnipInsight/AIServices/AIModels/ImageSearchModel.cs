// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Collections.Generic;
using System.Runtime.Serialization;
using GalaSoft.MvvmLight;

namespace SnipInsight.AIServices.AIModels
{
    [DataContract]
    public class ImageSearchModel : ObservableObject
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

        [DataMember(Name = "webSearchUrl")]
        public string WebSearchUrl { get; set; }

        [DataMember(Name = "thumbnailUrl")]
        public string Image { get; set; }

        [DataMember(Name = "datePublished")]
        public string DatePublished { get; set; }

        [DataMember(Name = "contentUrl")]
        public string URL { get; set; }

        [DataMember(Name = "hostPageUrl")]
        public string HostPageUrl { get; set; }

        [DataMember(Name = "contentSize")]
        public string ContentSize { get; set; }

        [DataMember(Name = "hostPageDisplayUrl")]
        public string HostPageDisplayUrl { get; set; }

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
        public double Height {
            get { return height; }
            set
            {
                height = value;
                RaisePropertyChanged();
            }
        }
    }

    [DataContract]
    public class ImageSearchModelContainer
    {
        [DataMember(Name = "visuallySimilarImages")]
        public ImageSearchModelList Container { get; set; }
    }

    [DataContract]
    public class ImageSearchModelList
    {
        [DataMember(Name = "value")]
        public List<ImageSearchModel> Images { get; set; }
    }
}
