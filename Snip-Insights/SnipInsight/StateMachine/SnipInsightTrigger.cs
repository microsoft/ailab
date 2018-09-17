// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Diagnostics.CodeAnalysis;

namespace SnipInsight.StateMachine
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum SnipInsightTrigger
    {
        CaptureScreen,
        QuickSnip,
        CaptureCamera,
        Whiteboard,
        Editor,

        WhiteboardForCurrentWindow, // whiteboard to fit current window size rather than full screen size.

        Record,
        Pause,
        Stop,
        TogglePlayStop,
        Redo,

        EditingWindowClosed,
        ToolWindowClosed,
        Exit,

        ShareLink,
        ShareEmbed,
        ShareEmail,
        ShareSendToOneNote,

        Copy,
        Save,
        Delete,

        ImageCaptured,
        ImageCaptureCancelled,

        SavingImageWithDialogCompleted,
        SavingVideoWithDialogCompleted,

        DeletionFailed,
        DeletionCancelled,

        SharingWithPublishCompleted,
        SharingWithImageCompleted,

        CopyingWithImageCompleted,
        CopyingWithPublishCompleted,

        ShowMainWindow,
        HideMainWindow,

        ShowLibraryPanel,
        LoadImageFromLibrary,
        LoadPackageFromLibrary,

        ShowSettingsPanel,

        ShowMicrophoneOptions,

        RestoreImage,
        RestorePackage,
        RestoreLibrary,
        RestoreSettings,
        RestoreWhiteboard,

        DoImageInsights,
        ShowImageResultsWindow,

        ShowAIPanel,
        HideAIPanel
    }
}
