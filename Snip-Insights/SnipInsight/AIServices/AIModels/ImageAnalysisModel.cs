// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SnipInsight.AIServices.AIModels
{
    public class VisualFeatureModel
    {
        List<string> Tags { get; set; }

        List<string> Captions { get; set; }
    }


    [DataContract]
    public class ImageAnalysisModel
    {
        [DataMember(Name = "categories")]
        public List<Category> Categories { get; set; }

        [DataMember(Name = "tags")]
        public List<Tag> Tags { get; set; }

        [DataMember(Name = "description")]
        public ImageDescription Description { get; set; }

        [DataMember(Name = "metadata")]
        public ImageMetadata Metadata { get; set; }

        [DataContract]
        public class Category
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "score")]
            public double Score { get; set; }

            [DataMember(Name = "detail")]
            public Detail Detail { get; set; }
        }

        [DataContract]
        public class Tag
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "confidence")]
            public double Confidence { get; set; }
        }

        [DataContract]
        public class Caption
        {
            [DataMember(Name = "text")]
            public string Text { get; set; }

            [DataMember(Name = "confidence")]
            public double Confidence { get; set; }
        }

        [DataContract]
        public class ImageDescription
        {
            [DataMember(Name = "tags")]
            public List<string> Tags { get; set; }

            [DataMember(Name = "captions")]
            public List<Caption> Captions { get; set; }
        }

        [DataContract]
        public class Celebrity
        {
            [DataMember(Name = "faceRectangle")]
            public FaceRectangle FaceRectangle { get; set; }

            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "confidence")]
            public double Confidence { get; set; }
        }

        [DataContract]
        public class Detail
        {
            [DataMember(Name = "celebrities")]
            public List<Celebrity> Celebrities { get; set; }

            [DataMember(Name = "landmarks")]
            public List<Landmark> Landmarks { get; set; }
        }

        [DataContract]
        public class Landmark
        {
            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "confidence")]
            public double Confidence { get; set; }
        }

        [DataContract]
        public class ImageMetadata
        {
            [DataMember(Name = "height")]
            public int Height { get; set; }

            [DataMember(Name = "width")]
            public int Width { get; set; }
        }
    }

    [DataContract]
    public class FaceRectangle
    {
        [DataMember(Name = "top")]
        public int Top { get; set; }

        [DataMember(Name = "left")]
        public int Left { get; set; }

        [DataMember(Name = "width")]
        public int Width { get; set; }

        [DataMember(Name = "height")]
        public int Height { get; set; }
    }
}
