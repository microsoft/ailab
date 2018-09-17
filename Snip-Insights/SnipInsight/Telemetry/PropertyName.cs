// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace SnipInsight.Telemetry
{
    public static class PropertyName
    {
        //Common Property Names for All Events
        public const string VersionNumber = "VersionNumber";
        public const string MACAddress = "MACAddress";

        //StateTransition Event Property Names
        public const string StateTransitionSource = "StateTransitionSource";
        public const string StateTransitionTrigger = "StateTransitionTrigger";
        public const string StateTransitionDestination = "StateTransitionDestination";

        //Api Call Event Properties
        public const string ApiCalled = "ApiCalled";
        public const string ApiResponseStatus = "ApiResponseStatus";
        public const string TimeToComplete = "TimeToComplete(ms)";

        //Button Click Event Properties
        public const string ButtonName = "ButtonName";
        public const string ViewName = "ViewName";
    }
}
