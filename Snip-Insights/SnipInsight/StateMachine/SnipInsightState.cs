// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace SnipInsight.StateMachine
{
    public enum SnipInsightState
    {
        Ready,
        CapturingScreen,          // User is capturing the screen
        CapturingCamera,          // User is capturing the camera
        QuickCapture,             // Capture but the picture is auto-saved and no editing option is presented to the user
        Editing,                  // User captured something and is ready for inking or recording. Editor is shown at this time.
        Recording,                // The recording is happening.
        Paused,                   // Recording is still in progress with pause
        EditingCompleted,         // A recording has been completed with Stop.
        LibraryPanelOpened,
        SettingsPanelOpened,
        Playing,
        SavingImage,
        SavingVideo,
        Deleting,
        Sharing,
        Copying,
        ImageInsights,
        AIPanelOpened,
        Exiting
    }
}
