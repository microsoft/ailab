// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SnipInsight.AIServices.AIModels
{
    [DataContract]
    public class Status
    {
        [DataMember(Name = "Code")]
        public int Code { get; set; }
        [DataMember(Name = "Description")]
        public string Description { get; set; }
        [DataMember(Name = "Exception")]
        public object Exception { get; set; }
    }

    [DataContract]
    public class ContentModerationModel
    {
        [DataMember(Name = "AdultClassificationScore")]
        public double AdultClassificationScore { get; set; }
        [DataMember(Name = "IsImageAdultClassified")]
        public bool IsImageAdultClassified { get; set; }
        [DataMember(Name = "RacyClassificationScore")]
        public double RacyClassificationScore { get; set; }
        [DataMember(Name = "IsImageRacyClassified")]
        public bool IsImageRacyClassified { get; set; }
        [DataMember(Name = "AdvancedInfo")]
        public List<object> AdvancedInfo { get; set; }
        [DataMember(Name = "Result")]
        public bool Result { get; set; }
        [DataMember(Name = "Status")]
        public Status Status { get; set; }
        [DataMember(Name = "TrackingId")]
        public string TrackingId { get; set; }
        [DataMember(Name = "CacheID")]
        public string CacheID { get; set; }
    }
}
