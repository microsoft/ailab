// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using SnipInsight.Util;
using Stateless;

namespace SnipInsight.StateMachine
{
    public class StateMachine : StateMachine<SnipInsightState, SnipInsightTrigger>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly Dictionary<string, Action> _actions;

        private SnipInsightState _preCaptureState = SnipInsightState.Ready; // State that was there before entering capture state. We are interested in Editing, EditingCompleted, and LibraryPanelOpened states only.

        public StateMachine(Dictionary<string, Action> actions)
            : base(SnipInsightState.Ready)
        {
            _actions = actions;
#if DEBUG
            _actions = new Dictionary<string, Action>();
            foreach (var entry in actions)
            {
                _actions.Add(entry.Key, WrapAction(entry.Key, entry.Value));
            }
#endif

            ConfigureUnhandledTriggers();

            ConfigureReadyState();
            ConfigureQuickCapture();
            ConfigureCapturingScreenState();
            ConfigureEditingState();
            ConfigureEditingCompletedState();
            ConfigureLibraryPanelOpenedState();
            ConfigureSettingsPanelOpenedState();
            ConfigureSavingImageState();
            ConfigureDeletingState();
            ConfigureSharingState();
            ConfigureCopyingState();
            ConfigureExitingState();

            OnTransitioned
                    (
                      t =>
                      {
                          Telemetry.ApplicationLogger.Instance.SubmitStateTransitionEvent(Telemetry.EventName.StateTransition, t.Source, t.Trigger, t.Destination);
                          OnPropertyChanged("State");
                          CommandManager.InvalidateRequerySuggested();
                      }
                    );

            //used to debug commands and UI components
#if DEBUG
            OnTransitioned
              (
                // ReSharper disable once LocalizableElement
                (t) => Console.WriteLine("StateMachine: {0} -> Trig({2}) -> {1}", t.Source, t.Destination, t.Trigger)
              );
#endif
        }

        public Action WrapAction(string actionKey, Action action)
        {
            return () =>
            {
                Console.WriteLine("ExecutingAction: " + actionKey);
                action();
            };
        }

        private void ConfigureUnhandledTriggers()
        {
            OnUnhandledTrigger((state, trigger) =>
            {
#if DEBUG
                // ReSharper disable once LocalizableElement
                Console.WriteLine("StateMachine: Unhandled Trigger Encountered. s:{0} t:{1}]", state, trigger);
#endif
            });
        }

        private StateConfiguration ConfigureReadyState()
        {
            return Configure(SnipInsightState.Ready)
                .OnEntry(tr =>
                {
                    // Clean and Clear old data if any.
                    _actions[ActionNames.CleanFiles]();
                    _actions[ActionNames.ClearOldImageData]();
                    _actions[ActionNames.ShowToolWindowShy]();
                    if (tr.Trigger == SnipInsightTrigger.ImageCaptureCancelled && _preCaptureState == SnipInsightState.Editing)
                    {
                        _actions[ActionNames.RestoreImage]();
                    }
                    else if (tr.Trigger == SnipInsightTrigger.ImageCaptureCancelled && _preCaptureState == SnipInsightState.LibraryPanelOpened)
                    {
                        _actions[ActionNames.RestoreLibrary]();
                    }
                    else if (tr.Trigger == SnipInsightTrigger.ImageCaptureCancelled && _preCaptureState == SnipInsightState.SettingsPanelOpened)
                    {
                        _actions[ActionNames.RestoreSettings]();
                    }
                    else
                    {
                        _actions[ActionNames.HideMainWindow]();
                    }
                })
                .OnExit(_actions[ActionNames.CloseFirstRunWindow])
                .Permit(SnipInsightTrigger.Exit, SnipInsightState.Exiting)
                .Permit(SnipInsightTrigger.RestoreImage, SnipInsightState.Editing)
                .Permit(SnipInsightTrigger.RestoreLibrary, SnipInsightState.LibraryPanelOpened)
                .Permit(SnipInsightTrigger.RestoreSettings, SnipInsightState.SettingsPanelOpened)
                .Permit(SnipInsightTrigger.CaptureScreen, SnipInsightState.CapturingScreen)
                .Permit(SnipInsightTrigger.ShowLibraryPanel, SnipInsightState.LibraryPanelOpened)
                .Permit(SnipInsightTrigger.ShowSettingsPanel, SnipInsightState.SettingsPanelOpened)
                .Permit(SnipInsightTrigger.LoadImageFromLibrary, SnipInsightState.Editing)
                .Permit(SnipInsightTrigger.QuickSnip, SnipInsightState.QuickCapture);
        }

        private StateConfiguration ConfigureCapturingScreenState()
        {
            return Configure(SnipInsightState.CapturingScreen)
                .OnEntry(tr =>
                {
                    _preCaptureState = tr.Source;
                    _actions[ActionNames.SaveMainWindowState]();
                    _actions[ActionNames.CreateMainWindow](); // Image stored needs to adjust other dependent sizes. So, create it.
                    _actions[ActionNames.InitializeCaptureImage]();
                    _actions[ActionNames.HideMainWindow]();
                    _actions[ActionNames.HideToolWindow]();
                    _actions[ActionNames.StartCaptureScreen]();
                })
                .OnExit(_actions[ActionNames.CloseImageCapture])
                .Permit(SnipInsightTrigger.Exit, SnipInsightState.Exiting)
                .Permit(SnipInsightTrigger.ImageCaptureCancelled, SnipInsightState.Ready)
                .Permit(SnipInsightTrigger.ImageCaptured, SnipInsightState.Editing)
                .Permit(SnipInsightTrigger.QuickSnip, SnipInsightState.Editing);
        }

        /// <summary>
        /// Add the initial configuration and permissions for the QuickSnip
        /// Describe the next possible steps and setup the environnement
        /// </summary>
        /// <returns>
        /// The configuration for the current state
        /// </returns>
        private StateConfiguration ConfigureQuickCapture()
        {
            return Configure(SnipInsightState.QuickCapture)
                .OnEntry(tr =>
                {
                    _preCaptureState = tr.Source;
                    _actions[ActionNames.SaveMainWindowState]();
                    _actions[ActionNames.CreateMainWindow](); // Image stored needs to adjust other dependent sizes. So, create it.
                    _actions[ActionNames.StartQuickCapture]();
                    _actions[ActionNames.HideMainWindow]();
                    _actions[ActionNames.HideToolWindow]();
                    _actions[ActionNames.StartCaptureScreen]();
                })
                .OnExit(_actions[ActionNames.CloseImageCapture])
                .Permit(SnipInsightTrigger.Exit, SnipInsightState.Exiting)
                .Permit(SnipInsightTrigger.ImageCaptureCancelled, SnipInsightState.Ready)
                .Permit(SnipInsightTrigger.QuickSnip, SnipInsightState.Editing);
        }

        private StateConfiguration ConfigureEditingState()
        {
            return Configure(SnipInsightState.Editing)
                .OnEntry(tr =>
                {
                    if (tr.Trigger == SnipInsightTrigger.QuickSnip)
                    {
                        _actions[ActionNames.SaveImage](); // User might have inked. So, save it again.
                        _actions[ActionNames.CleanFiles](); // Clean so that orig image is cleanedup.
                        Fire(SnipInsightTrigger.EditingWindowClosed);
                        return;
                    }

                    _actions[ActionNames.CreateMainWindow]();
                    _actions[ActionNames.ShowToolWindowShy]();
                    switch (tr.Trigger)
                    {
                        case SnipInsightTrigger.Redo:
                            _actions[ActionNames.Redo]();
                            break;
                        case SnipInsightTrigger.LoadImageFromLibrary:
                            _actions[ActionNames.CleanFiles]();
                            _actions[ActionNames.ClearOldImageData](); // First clear old image if any.
                            _actions[ActionNames.LoadImageFromLibary]();
                            break;
                    }
                    _actions[ActionNames.HideLibraryPanel](); // When we come to this state, it is always to show content and not the library.
                    if (tr.Trigger == SnipInsightTrigger.RestoreImage)
                    {
                        _actions[ActionNames.RestoreMainWindow]();
                    }
                    else
                    {
                        _actions[ActionNames.ShowMainWindow]();
                    }
                    if (tr.Trigger != SnipInsightTrigger.LoadImageFromLibrary) // If we load from lib, no need to save. Avoids loop.
                    {
                        _actions[ActionNames.SaveImage]();
                    }

					// Feature Out: Uncomment if you want to use the Editor Tour feature
                    //_actions[ActionNames.ShowEditorWindowTour]();

                    if (tr.Trigger == SnipInsightTrigger.ImageCaptured && UserSettings.CopyToClipboardAfterSnip)
                    {
                        _actions[ActionNames.ShowImageCapturedToastMessage]();
                    }

                    if (tr.Trigger == SnipInsightTrigger.ImageCaptured)
                    {
                        _actions[ActionNames.RunAllInsights]();
                    }
                })
                .OnExit(tr =>
                {
                    _actions[ActionNames.SaveImage](); // User might have inked. So, save it again.
                    _actions[ActionNames.CleanFiles](); // Clean so that orig image is cleanedup.
                })
                .Permit(SnipInsightTrigger.Exit, SnipInsightState.Exiting)
                .Permit(SnipInsightTrigger.CaptureScreen, SnipInsightState.CapturingScreen)
                .Permit(SnipInsightTrigger.Save, SnipInsightState.SavingImage)
                .Permit(SnipInsightTrigger.Copy, SnipInsightState.Copying)
                .Permit(SnipInsightTrigger.ShareEmail, SnipInsightState.Sharing)
                .Permit(SnipInsightTrigger.ShareSendToOneNote, SnipInsightState.Sharing)
                .Permit(SnipInsightTrigger.EditingWindowClosed, SnipInsightState.Ready)
                .Permit(SnipInsightTrigger.ShowLibraryPanel, SnipInsightState.LibraryPanelOpened)
                .Permit(SnipInsightTrigger.ShowSettingsPanel, SnipInsightState.SettingsPanelOpened);
        }

        private StateConfiguration ConfigureLibraryPanelOpenedState()
        {
            return Configure(SnipInsightState.LibraryPanelOpened)
                .OnEntry(tr =>
                {
                    _actions[ActionNames.ShowToolWindowShy]();
                    if (tr.Trigger == SnipInsightTrigger.RestoreLibrary)
                    {
                        _actions[ActionNames.RestoreMainWindow]();
                    }
                    else
                    {
                        _actions[ActionNames.ShowMainWindow]();
                    }
                    _actions[ActionNames.ShowLibraryPanel]();

                })
                .OnExit(tr =>
                {
                    _actions[ActionNames.HideLibraryPanel]();
                })
                .Permit(SnipInsightTrigger.CaptureScreen, SnipInsightState.CapturingScreen)
                .Permit(SnipInsightTrigger.LoadImageFromLibrary, SnipInsightState.Editing)
                .Permit(SnipInsightTrigger.LoadPackageFromLibrary, SnipInsightState.EditingCompleted)
                .PermitReentry(SnipInsightTrigger.ShowLibraryPanel)
                .Permit(SnipInsightTrigger.ShowSettingsPanel, SnipInsightState.SettingsPanelOpened)
                .Permit(SnipInsightTrigger.EditingWindowClosed, SnipInsightState.Ready)
                .Permit(SnipInsightTrigger.Exit, SnipInsightState.Exiting);
        }

        private StateConfiguration ConfigureSettingsPanelOpenedState()
        {
            return Configure(SnipInsightState.SettingsPanelOpened)
                .OnEntry(tr =>
                {
                    _actions[ActionNames.ShowToolWindowShy]();
                    if (tr.Trigger == SnipInsightTrigger.RestoreSettings)
                    {
                        _actions[ActionNames.RestoreMainWindow]();
                    }
                    else
                    {
                        _actions[ActionNames.ShowMainWindow]();
                    }
                    _actions[ActionNames.ShowSettingsPanel]();

                })
                .OnExit(tr =>
                {
                    _actions[ActionNames.HideSettingsPanel]();
                })
                .Permit(SnipInsightTrigger.CaptureScreen, SnipInsightState.CapturingScreen)
                .Permit(SnipInsightTrigger.LoadImageFromLibrary, SnipInsightState.Editing)
                .Permit(SnipInsightTrigger.LoadPackageFromLibrary, SnipInsightState.EditingCompleted)
                .Permit(SnipInsightTrigger.ShowLibraryPanel, SnipInsightState.LibraryPanelOpened)
                .PermitReentry(SnipInsightTrigger.ShowSettingsPanel)
                .Permit(SnipInsightTrigger.EditingWindowClosed, SnipInsightState.Ready)
                .Permit(SnipInsightTrigger.Exit, SnipInsightState.Exiting);
        }

        private StateConfiguration ConfigureEditingCompletedState()
        {
            return Configure(SnipInsightState.EditingCompleted)
                .OnEntry(tr =>
                {
                    if (tr.Trigger != SnipInsightTrigger.DeletionFailed) // If not failed, we keep the old image/recording as it is. Otherwise, clean old image.
                    {
                        _actions[ActionNames.CleanFiles]();
                    }

                    if (tr.Trigger == SnipInsightTrigger.RestorePackage)
                    {
                        _actions[ActionNames.RestoreMainWindow]();
                    }
                    _actions[ActionNames.ShowSharePanel]();
                })
                .OnExit(tr =>
                {
                    if (tr.Destination != SnipInsightState.Sharing) // To avoid glitch. We will come back to this state after sharing is done.
                    {
                        _actions[ActionNames.HideSharePanel]();
                    }
                })
                .Permit(SnipInsightTrigger.Exit, SnipInsightState.Exiting)
                .Permit(SnipInsightTrigger.CaptureScreen, SnipInsightState.CapturingScreen)
                .Permit(SnipInsightTrigger.Save, SnipInsightState.SavingVideo)
                .Permit(SnipInsightTrigger.Copy, SnipInsightState.Copying)
                .Permit(SnipInsightTrigger.Redo, SnipInsightState.Editing)
                .Permit(SnipInsightTrigger.Delete, SnipInsightState.Deleting)
                .Permit(SnipInsightTrigger.EditingWindowClosed, SnipInsightState.Ready)
                .Permit(SnipInsightTrigger.ShowLibraryPanel, SnipInsightState.LibraryPanelOpened)
                .Permit(SnipInsightTrigger.ShowSettingsPanel, SnipInsightState.SettingsPanelOpened);
        }

        private StateConfiguration ConfigureSavingImageState()
        {
            return Configure(SnipInsightState.SavingImage)
                .OnEntry(tr =>
                {
                    if (tr.IsReentry) return;
                    switch (tr.Source)
                    {
                        case SnipInsightState.Editing:
                            _actions[ActionNames.SaveImageWithDialog]();
                            break;
                    }
                })
                .Permit(SnipInsightTrigger.Exit, SnipInsightState.Exiting)
                .Permit(SnipInsightTrigger.SavingImageWithDialogCompleted, SnipInsightState.Editing)
                .PermitReentry(SnipInsightTrigger.EditingWindowClosed); // Save should not allow close of main window.
        }

        private StateConfiguration ConfigureDeletingState()
        {
            return Configure(SnipInsightState.Deleting)
                .OnEntry(tr =>
                {
                    if (tr.IsReentry) return;
                    _actions[ActionNames.CleanFiles](); // Clear before performing delete operation to ensure that images are deleted if it has package.
                    _actions[ActionNames.Delete]();
                })
                .Permit(SnipInsightTrigger.Exit, SnipInsightState.Exiting)
                .Permit(SnipInsightTrigger.LoadImageFromLibrary, SnipInsightState.Editing)
                .Permit(SnipInsightTrigger.DeletionFailed, SnipInsightState.EditingCompleted)
                .Permit(SnipInsightTrigger.DeletionCancelled, SnipInsightState.EditingCompleted)
                .PermitReentry(SnipInsightTrigger.EditingWindowClosed); // Delete should not allow close of main window.
        }

        private StateConfiguration ConfigureCopyingState()
        {
            return Configure(SnipInsightState.Copying)
                .OnEntry(tr =>
                {
                    if (tr.IsReentry) return;
                    _actions[ActionNames.CopyWithImage]();
                })
                .Permit(SnipInsightTrigger.Exit, SnipInsightState.Exiting)
                .Permit(SnipInsightTrigger.CopyingWithImageCompleted, SnipInsightState.Editing)
                .PermitReentry(SnipInsightTrigger.EditingWindowClosed); // Copy should finish before closing main window.
        }

        private StateConfiguration ConfigureSharingState()
        {
            return Configure(SnipInsightState.Sharing)
                .OnEntry(tr =>
                {
                    if (tr.IsReentry) return;
                    ProcessShareAction(tr);
                })
                .Permit(SnipInsightTrigger.Exit, SnipInsightState.Exiting)
                .Permit(SnipInsightTrigger.SharingWithImageCompleted, SnipInsightState.Editing)
                .Permit(SnipInsightTrigger.SharingWithPublishCompleted, SnipInsightState.EditingCompleted)
                .PermitReentry(SnipInsightTrigger.EditingWindowClosed); // Share should finish before letting main window to close.
        }

        private void ProcessShareAction(Transition tr)
        {
            switch (tr.Trigger)
            {
                case SnipInsightTrigger.ShareLink:
                    {
                        switch (tr.Source)
                        {
                            case SnipInsightState.EditingCompleted:
                                {
                                    _actions[ActionNames.ShareLinkWithPublish]();
                                    break;
                                }
                        }
                        break;
                    }
                case SnipInsightTrigger.ShareEmbed:
                    {
                        switch (tr.Source)
                        {
                            case SnipInsightState.EditingCompleted:
                                {
                                    _actions[ActionNames.ShareEmbedWithPublish]();
                                    break;
                                }
                        }
                        break;
                    }
                case SnipInsightTrigger.ShareEmail:
                    {
                        switch (tr.Source)
                        {
                            case SnipInsightState.Editing:
                                {
                                    _actions[ActionNames.ShareEmailWithImage]();
                                    break;
                                }
                            case SnipInsightState.EditingCompleted:
                                {
                                    _actions[ActionNames.ShareEmailWithPublish]();
                                    break;
                                }
                        }
                        break;
                    }
                case SnipInsightTrigger.ShareSendToOneNote:
                    {
                        switch (tr.Source)
                        {
                            case SnipInsightState.Editing:
                                {
                                    _actions[ActionNames.ShareSendToOneNoteWithImage]();
                                    break;
                                }
                            case SnipInsightState.EditingCompleted:
                                {
                                    _actions[ActionNames.ShareSendToOneNoteWithPublish]();
                                    break;
                                }
                        }
                        break;
                    }
            }
        }

        private StateConfiguration ConfigureExitingState()
        {
            return Configure(SnipInsightState.Exiting)
                .OnEntry(tr =>
                {
                    _actions[ActionNames.CleanFiles]();
                    _actions[ActionNames.ClearOldImageData]();
                    _actions[ActionNames.Exit]();
                });
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
