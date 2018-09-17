// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SnipInsight.AIServices.AIModels
{
    [DataContract]
    public class HandWrittenModel
    {
        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "recognitionResult")]
        public RecognitionResult RecognitionResult { get; set; }
    }

    [DataContract]
    public class WordWritten
    {
        [DataMember(Name = "boundingBox")]
        public List<int> BoundingBox { get; set; }

        [DataMember(Name = "text")]
        public string Text { get; set; }
    }

    [DataContract]
    public class LineWritten
    {
        [DataMember(Name = "boundingBox")]
        public List<int> BoundingBox { get; set; }

        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "words")]
        public List<WordWritten> Words { get; set; }
    }

    [DataContract]
    public class RecognitionResult
    {
        [DataMember(Name = "lines")]
        public List<LineWritten> Lines { get; set; }
    }
}
