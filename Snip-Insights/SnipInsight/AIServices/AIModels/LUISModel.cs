// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SnipInsight.AIServices.AIModels
{
    [DataContract]
    public class LUISModel
    {
        [DataMember(Name = "query")]
        public string Query { get; set; }
        [DataMember(Name = "topScoringIntent")]
        public TopScoringIntent TheTopScoringIntent { get; set; }
        [DataMember(Name = "intents")]
        public List<Intent> Intents { get; set; }
        [DataMember(Name = "entities")]
        public List<Entity> Entities { get; set; }

        [DataContract]
        public class TopScoringIntent
        {
            [DataMember(Name = "intent")]
            public string Intent { get; set; }
            [DataMember(Name = "score")]
            public double Score { get; set; }
        }

        [DataContract]
        public class Intent
        {
            [DataMember(Name = "intent")]
            public string IntentValue { get; set; }
            [DataMember(Name = "score")]
            public double Score { get; set; }
        }

        [DataContract]
        public class Value
        {
            [DataMember(Name = "timex")]
            public string Timex { get; set; }
            [DataMember(Name = "type")]
            public string Type { get; set; }
            [DataMember(Name = "start")]
            public string Start { get; set; }
            [DataMember(Name = "end")]
            public string End { get; set; }
            [DataMember(Name = "value")]
            public string TheValue { get; set; }
        }

        [DataContract]
        public class Resolution
        {
            [DataMember(Name = "values")]
            public List<Value> Values { get; set; }
        }

        [DataContract]
        public class Entity
        {
            [DataMember(Name = "entity")]
            public string TheEntity { get; set; }
            [DataMember(Name = "type")]
            public string Type { get; set; }
            [DataMember(Name = "startIndex")]
            public int StartIndex { get; set; }
            [DataMember(Name = "endIndex")]
            public int EndIndex { get; set; }
            [DataMember(Name = "resolution")]
            public Resolution Resolution { get; set; }
        }
    }
}
