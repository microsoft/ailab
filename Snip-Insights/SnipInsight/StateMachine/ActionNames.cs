// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace SnipInsight.StateMachine
{
    public static class ActionNames
    {
        public const string CreateMainWindow = "CreateMainWindow";
        public const string ShowMainWindow = "ShowMainWindow";
        public const string HideMainWindow = "HideMainWindow";
        public const string CloseMainWindow = "CloseMainWindow"; // Hide + EditingWindowsClosedTrigger
        public const string ShowToolWindow = "ShowToolWindow";
        public const string ShowToolWindowShy = "ShowToolWindowShy";
        public const string HideToolWindow = "HideToolWindow";
        public const string ShowSharePanel = "ShowSharePanel";
        public const string HideSharePanel = "HideSharePanel";
        public const string CloseFirstRunWindow = "CloseFirstRunWindow";
        public const string InitializeCaptureImage = "InitializeCaptureImage";
        public const string ShowLibraryPanel = "ShowLibraryPanel";
        public const string HideLibraryPanel = "HideLibraryPanel";
        public const string ShowSettingsPanel = "ShowSettingsPanel";
        public const string HideSettingsPanel = "HideSettingsPanel";
        public const string StartCaptureScreen  = "StartCaptureScreen";
        public const string StartCaptureCamera  = "StartCaptureCamera";
        public const string StartQuickCapture = "StartQuickCapture"; // To save a picture without opening the editor
        public const string StartWhiteboard = "StartWhiteboard";
        public const string CloseImageCapture = "CloseImageCapture";
        public const string StartWhiteboardForCurrentWindow = "StartWhiteboardForCurrentWindow";
        public const string PrepareRecording = "PrepareRecording";
        public const string Record = "Record";
        public const string Pause = "Pause";
        public const string Stop = "Stop";
        public const string StartPlay = "StartPlay";
        public const string StopPlay = "StopPlay";
        public const string Exit = "Exit";
        public const string SaveImage = "SaveImage";
        public const string SaveImageWithDialog = "SaveImageWithDialog";
        public const string SaveVideoWithDialog = "SaveVideoWithDialog";
        public const string ShareLinkWithPublish= "ShareLinkWithPublish";
        public const string ShareEmbedWithPublish = "ShareEmbedWithPublish";
        public const string ShareEmailWithPublish = "ShareEmailWithPublish";
        public const string ShareSendToOneNoteWithPublish = "ShareSendToOneNoteWithPublish";
        public const string ShareEmailWithImage = "ShareEmailWithImage";
        public const string ShareSendToOneNoteWithImage = "ShareSendToOneNoteWithImage";
        public const string CopyWithImage = "CopyWithImage";
        public const string CopyWithPublish = "CopyWithPublish";
        public const string ClearOldImageData = "ClearOldImageData";
        public const string Redo = "Redo";
        public const string Delete = "Delete";
        public const string DeleteLibraryItems = "DeleteLibraryItems";
        public const string CleanFiles = "CleanFiles";
        public const string OpenMediaCapture = "OpenMediaCapture";
        public const string CloseMediaCapture = "CloseMediaCapture";
        public const string ShowImageCapturedToastMessage = "ShowImageCapturedToastMessage";
        public const string ShowMicrophoneOptions = "ShowMicrophoneOptions";
        public const string LoadImageFromLibary = "LoadImageFromLibary";
        public const string LoadPackageFromLibary = "LoadPackageFromLibary";
        public const string SelectLatestLibItem = "SelectLatestLibItem";
        public const string RestoreImage = "RestoreImage"; // To set trigger to go back to Editing state after screen capture cancel.
        public const string RestorePackage = "RestorePackage"; // To set trigger to go back to EditingCompleted state after screen capture cancel.
        public const string RestoreLibrary = "RestoreLibrary"; // To set trigger to go back to Library state after screen capture cancel.
        public const string RestoreSettings = "RestoreSettings"; // To set trigger to go back to Settings state after screen capture cancel.
        public const string RestoreMainWindow = "RestoreMainWindow";
        public const string SaveMainWindowState = "SaveMainWindowState";
        public const string ShowEditorWindowTour = "ShowEditorWindowTour";
        public const string StopEditorWindowTour = "StopEditorWindowTour";
        public const string DoImageInsights = "DoImageInsights";
        public const string ShowImageResultsWindow = "ShowImageResultsWindow";
        public const string CreateImageResultsWindow = "CreateImageResultsWindow";
        public const string DoCompVisionAnalysis = "DoCompVisionAnalysis";
        public const string ShowAIPanel = "ShowAIPanel";
        public const string HideAIPanel = "HideAIPanel";
        public const string OpenAIPanel = "OpenAIPanel";
		public const string RunAllInsights = "RunAllInsights";
	}
}
