// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SnipInsight.AIServices.AIModels
{
    [DataContract]
    public class PrintedModel
    {
        [DataMember(Name = "language")]
        public string Language { get; set; }

        [DataMember(Name = "orientation")]
        public string Orientation { get; set; }

        [DataMember(Name = "textAngle")]
        public double TextAngle { get; set; }

        [DataMember(Name = "regions")]
        public List<Region> Regions { get; set; }
    }

    [DataContract]
    public class Word
    {
        [DataMember(Name = "boundingBox")]
        public string BoundingBox { get; set; }

        [DataMember(Name = "text")]
        public string Text { get; set; }
    }

    [DataContract]
    public class Line
    {
        [DataMember(Name = "boundingBox")]
        public string BoundingBox { get; set; }

        [DataMember(Name = "words")]
        public List<Word> Words { get; set; }
    }

    [DataContract]
    public class Region
    {
        [DataMember(Name = "boundingBox")]
        public string BoundingBox { get; set; }

        [DataMember(Name = "lines")]
        public List<Line> Lines { get; set; }
    }
}
