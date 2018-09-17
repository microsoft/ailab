// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using CommonServiceLocator;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell;
using SnipInsight.AIServices;
using SnipInsight.AIServices.AILogic;
using SnipInsight.AIServices.AIModels;
using SnipInsight.ClipboardUtils;
using SnipInsight.Conversion;
using SnipInsight.EmailController;
using SnipInsight.ImageCapture;
using SnipInsight.Package;
using SnipInsight.Properties;
using SnipInsight.SendTo;
using SnipInsight.StateMachine;
using SnipInsight.Util;
using SnipInsight.ViewModels;
using SnipInsight.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SnipInsight
{
    internal class AppManager : IDisposable
    {
        #region Enumerations

        /// <summary>
        /// Enumeration for each of the navbar buttons
        /// </summary>
        private enum NavBarButtons
        {
            Editor,
            AIPanel,
            Library,
            Settings,
        }

        #endregion

        static internal AppManager TheBoss { get; } = new AppManager();

        #region Member variables
        readonly SnipInsightsManager _snipInsightsManager;
        IImageCaptureManager _imageCapture;
        Mutex _singleInstanceMutex;
        readonly string _singleInstanceMutexName = string.Format("{0}_SingleInstanceMutex_2E07A16C-8329-4577-94CA-3318635DBFDD", Assembly.GetExecutingAssembly().GetName().Name);
        EventWaitHandle _instanceLaunchedMonitorEvent;
        readonly string _instanceLaunchedMonitorEventName = string.Format("{0}_InstanceLaunchedMonitorEvent_B84FC2F6-3AEA-486D-99CC-70886D00941E", Assembly.GetExecutingAssembly().GetName().Name);
        ManualResetEvent _backgroundTaskStopEvent;

        // cmd line
        string[] _cmdLineArgs;
        bool _startShy;

        private bool _loadedSnipInsights;

        /// <summary>
        /// Stores the currently disabled button
        /// </summary>
        private NavBarButtons? disabledNavButton = null;
        private ContentModerationHandler contentModerationHandler = new ContentModerationHandler("ContentModerator");
        #endregion

        #region Properties

        internal SnipInsightViewModel ViewModel
        {
            get;
            private set;
        }

        internal AIManager AiCoreManager
        {
            get;
            private set;
        }

        internal MainWindow MainWindow
        {
            get;
            private set;
        }

        internal ToolWindow ToolWindow
        {
            get;
            private set;
        }

        internal FirstRunWindow FirstRunWindow
        {
            get;
            private set;
        }

        internal TrayIcon TrayIcon { get; private set; }

        /// <summary>
        /// Image metadata used by application, obtained from image analysis API
        /// </summary>
        internal ImageAnalysisResult ImageMetadata { get; set; }

        #endregion

        AppManager()
        {
            EnsureValidUserConfig();

            Resources.Culture = System.Globalization.CultureInfo.CurrentCulture;
            Dictionary<string, Action> actions = new Dictionary<string, Action>
            {
                { ActionNames.CreateMainWindow, WrapException(CreateMainWindow)},
                { ActionNames.ShowMainWindow, WrapException(ShowMainWindow)},
                { ActionNames.HideMainWindow, WrapException(HideMainWindow)},
                { ActionNames.CloseMainWindow, WrapException(CloseMainWindow)},
                { ActionNames.RestoreImage, WrapException(RestoreImage)},
                { ActionNames.RestoreLibrary, WrapException(RestoreLibrary)},
                { ActionNames.RestoreSettings, WrapException(RestoreSettings)},
                { ActionNames.RestoreMainWindow, WrapException(RestoreMainWindow)},
                { ActionNames.ShowToolWindow, WrapException(ShowToolWindow)},
                { ActionNames.ShowToolWindowShy, WrapException(ShowToolWindowShy)},
                { ActionNames.HideToolWindow, WrapException(HideToolWindow)},
                { ActionNames.CloseFirstRunWindow, WrapException(CloseFirstRunWindow)},
                { ActionNames.InitializeCaptureImage, WrapException(InitializeCaptureImage)},
                { ActionNames.StartCaptureScreen, WrapException(StartCaptureImage)},
                { ActionNames.ShowLibraryPanel, WrapAsyncException(ShowLibraryPanel)},
                { ActionNames.HideLibraryPanel, WrapException(HideLibraryPanel)},
                { ActionNames.ShowSettingsPanel, WrapException(ShowSettingsPanel)},
                { ActionNames.HideSettingsPanel, WrapException(HideSettingsPanel)},
                { ActionNames.StartQuickCapture, WrapException(InitializeQuickCapture)},
                { ActionNames.CloseImageCapture, WrapException(CloseImageCapture)},
                { ActionNames.Exit, WrapException(OnExit)},
                { ActionNames.SaveImage, WrapException(OnSaveImage)},
                { ActionNames.SaveImageWithDialog, WrapException(OnSaveImageWithDialog)},
                { ActionNames.ShareEmailWithImage, WrapException(OnShareEmailWithImage)},
                { ActionNames.ShareSendToOneNoteWithImage, WrapException(OnShareSendToOneNoteWithImage)},
                { ActionNames.CopyWithImage, WrapException(OnCopyWithImage)},
                { ActionNames.ClearOldImageData, WrapException(ClearOldImageData)},
                { ActionNames.Delete, WrapException(OnDelete)},
                { ActionNames.DeleteLibraryItems, WrapAsyncException(OnDeleteLibraryItems)},
                { ActionNames.CleanFiles, WrapException(OnCleanFiles)},
                { ActionNames.ShowImageCapturedToastMessage, WrapException(ShowImageCapturedToastMessage)},
                { ActionNames.LoadImageFromLibary, WrapException(LoadImageFromLibrary)},
                { ActionNames.SaveMainWindowState, WrapException(SaveMainWindowState)},
                { ActionNames.ShowEditorWindowTour, WrapException(ShowEditorWindowTour)},
                { ActionNames.OpenAIPanel, WrapException(OnShowHideAIPanel)},
                { ActionNames.RunAllInsights, WrapException(RunAllInsights)}
            };

            ViewModel = new SnipInsightViewModel(actions)
            {
                EraserCommand = new DelegateCommand(OnEraser),
                EraseAllCommand = new DelegateCommand(OnEraseAll),
                UndoCommand = new DelegateCommand(OnUndo),
                RedoCommand = new DelegateCommand(OnRedo),
                ToggleLibraryCommand = new DelegateCommand(OnShowHideLibrary),
                ToggleSettingsCommand = new DelegateCommand(OnShowHideSettings),
                ToggleEditorCommand = new DelegateCommand(OnShowHideEditor),
                ToggleAIPanelCommand = new DelegateCommand(OnShowHideAIPanel),

                // Commands for action buttons in ai panel
                RestoreImageCommand = new DelegateCommand(OnRestoreImage),
                SaveImageCommand = new DelegateCommand(OnSaveImageWithDialog),
                CopyImageCommand = new DelegateCommand(OnCopyWithImage),
                ShareImageEmailCommand = new DelegateCommand(OnShareEmailWithImage),
                ShareImageSendToOneNoteCommand = new DelegateCommand(OnShareSendToOneNoteWithImage),
                RefreshAICommand = new DelegateCommand(OnRefreshAI)
            };

            AiCoreManager = new AIManager();

            _snipInsightsManager = new SnipInsightsManager();

            _snipInsightsManager.ImageSaved += _snipInsightsManager_ImageSaved;
            _snipInsightsManager.ImageDeleted += _snipInsightsManager_ImageDeleted;

            _backgroundTaskStopEvent = new ManualResetEvent(false);

            TrayIcon = new TrayIcon();

            ImageMetadata = new ImageAnalysisResult();
        }

        private static void EnsureValidUserConfig()
        {
            // Attempt to open the user's user.config file, or delete it file if we fail to open it.
            // This is necessary because it is possible for the user.config file to get corrupted.
            try
            {
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            }
            catch (ConfigurationErrorsException e)
            {
                if (!string.IsNullOrEmpty(e.Filename) && e.Filename.EndsWith("user.config"))
                {
                    try
                    {
                        File.Delete(e.Filename);
                    }
                    catch
                    { }
                }
            }
        }

        void _snipInsightsManager_ImageDeleted(object sender, PackageArgs e)
        {
            var matchedItem = ViewModel.Packages.FirstOrDefault(x => x.Url == e.PackageUrl);
            if (matchedItem != null)
            {
                ViewModel.Packages.Remove(matchedItem);
                matchedItem.Dispose();
                ViewModel.SelectedPackage = null; // This could be pointing to some other content than the currently saved one. Reset it.
            }
            else
            {
                e.Thumbnail.Dispose();
            }
        }

        void _snipInsightsManager_ImageSaved(object sender, PackageArgs e)
        {
            // Save should be a new one and NOT an existing one. If new one, consume it
            if (ViewModel.Packages.FirstOrDefault(x => x.Url == e.PackageUrl) == null)
            {
                var link = new SnipInsightLink
                {
                    Url = e.PackageUrl,
                    ImageStream = e.Thumbnail,
                    Duration = e.Duration,
                    HasMedia = e.HasMedia,
                    LastWriteTime = DateTime.Now,
                    HasPackage = false
                };
                ViewModel.Packages.Insert(0, link);
                ViewModel.SelectedPackage = null; // This could be pointing to some other content than the currently saved one. Reset it.
            }
            else
            {
                e.Thumbnail.Dispose();
            }
        }

        public Action WrapException(Action action)
        {
            return () =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Debug.Fail("There was an exception when calling an action. Ex Message = ", ex.ToString());
                    Diagnostics.LogException(ex);
                }
            };
        }

        public Action WrapAsyncException(Func<Task> asyncAction)
        {
            return async () =>
            {
                try
                {
                    await asyncAction();
                }
                catch (Exception ex)
                {
                    Debug.Fail("There was an exception when calling an action. Ex Message = ", ex.ToString());
                    Diagnostics.LogException(ex);
                }
            };
        }

        /// <summary>
        /// Used when there is no registry entry for appid. May be set to id from the first appid file from package folder.
        /// </summary>
        internal string _defaultAppIdForRegistry = Guid.NewGuid().ToString();

        internal string DefaultAppIdForRegisry
        {
            get
            {
                return _defaultAppIdForRegistry;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AppManager()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            IDisposable imageCaptureDispose = _imageCapture as IDisposable;
            if (imageCaptureDispose != null)
            {
                imageCaptureDispose.Dispose();
                _imageCapture = null;
            }
            if (_singleInstanceMutex != null)
            {
                _singleInstanceMutex.Close();
                _singleInstanceMutex = null;
            }
            if (_instanceLaunchedMonitorEvent != null)
            {
                _instanceLaunchedMonitorEvent.Close();
                _instanceLaunchedMonitorEvent = null;
            }
            if (_backgroundTaskStopEvent != null)
            {
                _backgroundTaskStopEvent.Close();
                _backgroundTaskStopEvent = null;
            }
            if (TrayIcon != null)
            {
                TrayIcon.Dispose();
                TrayIcon = null;
            }
        }

        internal void Run(string[] args)
        {
            if (InstanceAlreadyRunning())
            {
                // signal the already running instance that a new instance was launched
                using (EventWaitHandle instanceLaunchedMonitorEvent = new EventWaitHandle(false, EventResetMode.AutoReset, _instanceLaunchedMonitorEventName))
                {
                    instanceLaunchedMonitorEvent.Set();
                }

                this.Dispose();
                Application.Current.Shutdown();
            }
            else
            {
                StartMonitoringForNewInstances();
                var updated = UpdateVersion();
                ProcessCmdLine(args);
                ShowToolWindow(!_startShy);
                RegisterHotKeys();
                // Feature Out: Uncomment if you want to use the fist time cards feature
                //ShowFirstRunWindow(updated);
            }
        }

        bool InstanceAlreadyRunning()
        {
            bool acquiredOwnership;

            _singleInstanceMutex = new Mutex(true, _singleInstanceMutexName, out acquiredOwnership);

            return !acquiredOwnership;
        }

        void StartMonitoringForNewInstances()
        {
            new Thread(MonitorForNewInstancesThreadProc)
            {
                IsBackground = true // allow the process to terminate if this thread is still running
            }
            .Start();
        }

        void MonitorForNewInstancesThreadProc()
        {
            try
            {
                _instanceLaunchedMonitorEvent = new EventWaitHandle(false, EventResetMode.AutoReset, _instanceLaunchedMonitorEventName);

                while (true)
                {
                    WaitHandle[] eventsToWaitOn = new WaitHandle[] { _instanceLaunchedMonitorEvent, _backgroundTaskStopEvent };

                    int signaledEventIndex = WaitHandle.WaitAny(eventsToWaitOn);

                    switch (signaledEventIndex)
                    {
                        case 0:
                            // _instanceLaunchedMonitorEvent has been signaled, open the ToolWindow
                            if (Application.Current != null)
                                Application.Current.Dispatcher.Invoke(ShowToolWindow);
                            break;
                        case 1:
                            // _backgroundTaskStopEvent has been signaled, exit
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                Diagnostics.LogLowPriException(ex);
            }
        }

        string GetCmdLineArgsString()
        {
            StringBuilder cmdLineArgsSb = new StringBuilder();

            foreach (string arg in _cmdLineArgs)
            {
                cmdLineArgsSb.Append(string.Format("{0} ", arg));
            }

            return cmdLineArgsSb.ToString().TrimEnd();
        }

        internal void RestartApp(bool killRunningInstance = false)
        {
            // release the single instance mutex so the new process can acquire ownership
            _singleInstanceMutex.Close();
            _singleInstanceMutex = null;

            StopBackgroundTasks();

            // start the new process with the existing cmd line
            // launch from the install dir, not necessarily where the current process is running from
            string processPath = UserSettings.AppPath;
            string processArgs = GetCmdLineArgsString();
            Process.Start(processPath, processArgs);

            ExitApp(killRunningInstance);
        }

        internal void ExitApp(bool killRunningInstance = false)
        {
            try
            {
                if (killRunningInstance)
                {
                    Process.GetCurrentProcess().Kill();
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() => ViewModel.StateMachine.Fire(StateMachine.SnipInsightTrigger.Exit));
                }
            }
            catch (Exception ex)
            {
                Diagnostics.LogLowPriException(ex);
            }
        }

        void StopBackgroundTasks()
        {
            if (_backgroundTaskStopEvent != null)
            {
                _backgroundTaskStopEvent.Set();
            }
        }

        private bool UpdateVersion()
        {
            var appVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (String.Equals(appVersion, UserSettings.Version))
            {
                return false;
            }

            UserSettings.IsAIEnabled = false;
            ViewModel.InsightsVisible = UserSettings.IsAIEnabled;
            UserSettings.Version = appVersion;
            Telemetry.ApplicationLogger.Instance.SubmitEvent(Telemetry.EventName.VersionChange);
            return true;
        }

        private void ShowFirstRunWindow(bool versionUpdated)
        {
            bool showFirstRun = versionUpdated && !UserSettings.DisableFirstRun;

            if (showFirstRun)
            {
                FirstRunWindow firstRunWindow = new FirstRunWindow();
                firstRunWindow.Show();
                FirstRunWindow = firstRunWindow;
                firstRunWindow.Closed += (sender, args) => { FirstRunWindow = null; };

                UserSettings.DisableFirstRun = true; // only show first run on the 'first run'
            }
        }

        internal void ShowToolWindow()
        {
            ShowToolWindow(true);
        }

        internal void ShowToolWindowShy()
        {
            ShowToolWindow(false);
        }

        private void ShowToolWindow(bool isOpen)
        {
            if (ToolWindow == null)
            {
                ToolWindow = new ToolWindow();
            }

            ToolWindow.ShowToolWindow(isOpen);
        }

        internal void HideToolWindow()
        {
            if (ToolWindow != null)
            {
                ToolWindow.HideToolWindow();
            }
        }

        internal void CloseFirstRunWindow()
        {
            var firstRunWindow = FirstRunWindow;
            if (firstRunWindow != null)
            {
                firstRunWindow.CloseWindow();
            }
        }

        internal void CreateMainWindow()
        {
            if (MainWindow == null)
            {
                MainWindow = new MainWindow();
                MainWindow.Title = Resources.WindowTitle_Main;
                MainWindow.Loaded += OnLoaded;
                ViewModel.Mode = Mode.Capturing;
            }
        }

        internal void ShowMainWindow()
        {
            var state = WindowState.Normal;
            if (MainWindow != null && MainWindow.WindowState != WindowState.Minimized)
            {
                state = MainWindow.WindowState;
            }
            ShowMainWindowInternal(state);
        }

        internal void ShowMainWindowInternal(WindowState? state = null)
        {
            CreateMainWindow();
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                    new Action(delegate()
                                    {
                                        MainWindow.Show();

                                        if (state.HasValue)
                                        {
                                            MainWindow.WindowState = state.Value;
                                        }

                                        MainWindow.Activate();
                                    }));
        }

        internal void ShowEditorWindowTour()
        {
            bool showTour = !UserSettings.DisableEditorWindowTour;

			if (showTour && MainWindow != null)
			{
				UserSettings.DisableEditorWindowTour = true;

                MainWindow.ShowEditorTour();
			}
		}

        internal void StopEditorWindowTour()
        {
            if (MainWindow != null)
            {
                bool isRunning = MainWindow.StopEditorTour();
                if (isRunning)
                {
                    // They didn't finish to re-enable for next time
                    UserSettings.DisableEditorWindowTour = false;
                }
            }
        }

        internal void ShowImageCapturedToastMessage()
        {
            ToastControl toast = new ToastControl(Resources.Message_CopiedToClipboard, 1000);
            toast.ShowInMainWindow();
        }

        internal void CloseMainWindow()
        {
            if (MainWindow != null)
            {
                SwitchNavButton(null);
                ViewModel.AIEnable = false;
                ViewModel.EditorEnable = false;

                HideMainWindow();
                ViewModel.StateMachine.Fire(SnipInsightTrigger.EditingWindowClosed);
            }
        }

        internal void HideMainWindow()
        {
            if (MainWindow != null)
            {
                MainWindow.Hide();
            }
        }

        void ProcessCmdLine(string[] args)
        {
            _cmdLineArgs = args;

            foreach (string argRaw in args)
            {
                string arg = argRaw;

                if (arg.StartsWith("-") || arg.StartsWith("/"))
                {
                    arg = arg.Substring(1);

                    if (string.Compare(arg, "startshy", true) == 0)
                    {
                        _startShy = true;
                    }
                }
            }
        }

        async Task ShowLibraryPanel()
        {
            if (MainWindow != null)
            {
                ViewModel.AIEnable = true;
                SwitchNavButton(NavBarButtons.Library);

                MainWindow.OnShowLibrary();
                MainWindow.SizeToContent = SizeToContent.Manual;
                ShowMainWindowInternal();

                if (!_loadedSnipInsights)
                {
                    _loadedSnipInsights = true;
                    await StartLoadingSnipInsights();
                }
            }
        }

        void ShowSettingsPanel()
        {
            // TODO: Make a shortcut for this panel
            if (MainWindow != null)
            {
                MainWindow.OnShowSettings();
                // To Disable the Setting button when setting button is
                //pressed to show enabled overlay
                ViewModel.AIEnable = true;
                SwitchNavButton(NavBarButtons.Settings);
                ShowMainWindowInternal();
            }
        }

        void HideSettingsPanel()
        {
            if (MainWindow != null)
            {
                MainWindow.OnHideSettings();
            }
        }

        private async Task StartLoadingSnipInsights()
        {
            ViewModel.Packages.Clear();
            List<FileInfo> files = _snipInsightsManager.GetAllSnipInsightFileInfos();
            const int batchSize = 25;
            List<FileInfo> batch;
            int count = 0;
            do
            {
                batch = files.GetRange(count, Math.Min(batchSize, files.Count - count));
                if (batch.Count > 0)
                {
                    // Wait for one batch to finish before loading other batches.
                    await StartLoadingSnipInsightsBatch(batch).ConfigureAwait(false);
                }
                count += batch.Count;
            } while (count < files.Count && batch.Count > 0);
        }

        private async Task StartLoadingSnipInsightsBatch(List<FileInfo> files)
        {
            var tasks = new Collection<Task<PackageData>>();
            foreach (var file in files)
            {
                tasks.Add(LoadPackagesAsync(file));
            }
            var packagesData = await Task.WhenAll(tasks).ConfigureAwait(false);

            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var packageData in packagesData.Where(x => x != null))
                    {
                        LoadSnipInsightLinkFromPackageData(packageData);
                    }
                });
            }
        }

        private void LoadSnipInsightLinkFromPackageData(PackageData packageData)
        {
            if (packageData != null)
            {
                var SnipInsightLink = new SnipInsightLink
                {
                    Url = packageData.Url,
                    ImageStream = packageData.Thumbnail,
                    Duration = packageData.Duration,
                    LastWriteTime = packageData.LastWriteTime,
                    HasMedia = packageData.HasMedia,
                    HasPackage = packageData.IsPackage,
                    MixId = packageData.MixId
                };
                ViewModel.Packages.Add(SnipInsightLink);
                packageData.Thumbnail = null;
                packageData.Dispose();
            }
        }

        private async Task<PackageData> LoadPackagesAsync(FileInfo file)
        {
            try
            {
                var packageData = await _snipInsightsManager.GetPackageDataAsync(file);
                return packageData;
            }
            catch (Exception ex)
            {
                Diagnostics.LogLowPriException(ex);
                return null;
            }
        }

        void HideLibraryPanel()
        {
            if (MainWindow != null)
            {
                MainWindow.OnHideLibrary();
            }
        }

        #region Actions
        public void OnEraser()
        {
            ViewModel.InkModeRequested = System.Windows.Controls.InkCanvasEditingMode.EraseByStroke;
            ResetEditorButtons(EditorTools.Eraser);
        }

        void OnEraseAll()
        {
            try
            {
                MainWindow.acetateLayer.InkCanvas.EraseAll();
                ViewModel.HasInk = false;
            }
            catch (Exception ex)
            {
                Diagnostics.ReportException(ex);
            }
        }

        /// <summary>
        /// Undo most recent edit action performed.
        /// </summary>
        void OnUndo()
        {
            try
            {
                MainWindow.acetateLayer.InkCanvas.Undo();
            }
            catch (Exception ex)
            {
                Diagnostics.ReportException(ex);
            }
        }

        /// <summary>
        /// Redo the most recent action undone.
        /// </summary>
        void OnRedo()
        {
            try
            {
                MainWindow.acetateLayer.InkCanvas.Redo();
            }
            catch (Exception ex)
            {
                Diagnostics.ReportException(ex);
            }
        }

        internal void OnExit()
        {
            try
            {
                StopBackgroundTasks();

                if (ToolWindow != null)
                {
                    ToolWindow.ToolWindowClosedBySystem = true;
                    ToolWindow.Close();
                }
                ToolWindow = null;
                if (MainWindow != null)
                {
                    MainWindow.MainWindowClosedBySystem = true;
                    MainWindow.Close();
                    MainWindow = null;
                }

                Dispose();
            }
            catch (Exception ex)
            {
                Diagnostics.ReportException(ex);
            }
        }

        private void OnDelete()
        {
            Delete(ViewModel.SavedSnipInsightFile, ViewModel.SavedCaptureImage, ViewModel.SavedInkedImage, true);
        }

        private async Task OnDeleteLibraryItems()
        {
            await DeleteLibraryItemsAsync();
        }

        public Task DeleteLibraryItemsAsync()
        {
            return DeleteAsync(ViewModel.SelectedLibraryItems, true, false);
        }

         public void Delete(string SnipInsightFile, string savedCaptureImage, string savedInkedImage, bool raiseOutcomeTrigger)
        {
            try
            {
                int deletedIndex = GetCurrentContentIndexInLibrary(SnipInsightFile, savedCaptureImage, savedInkedImage);
                Debug.Assert(deletedIndex != -1);
                var deletedLink = ViewModel.Packages[deletedIndex];

                var deleteConfirmed = ShowDeleteConfirmation(1, deletedLink.MixId != null);

                if (!deleteConfirmed)
                {
                    if (raiseOutcomeTrigger)
                    {
                        ViewModel.StateMachine.Fire(SnipInsightTrigger.DeletionCancelled);
                    }
                    return;
                }

                var success = DeleteCore(SnipInsightFile, savedCaptureImage, savedInkedImage);

                if (success)
                {
                    SelectPackageAfterDelete(deletedIndex, raiseOutcomeTrigger);
                }
                else
                {
                    Diagnostics.LogTrace("Deletion failed.");
                    ToastControl toast = new ToastControl(Resources.Delete_Failed);
                    toast.ShowInMainWindow();
                    if (raiseOutcomeTrigger)
                    {
                        ViewModel.StateMachine.Fire(SnipInsightTrigger.DeletionFailed);
                    }
                }
            }
            catch (Exception ex)
            {
                Diagnostics.ReportException(ex);
                ToastControl toast = new ToastControl(Resources.Delete_Failed);
                toast.ShowInMainWindow();
                ViewModel.StateMachine.Fire(SnipInsightTrigger.DeletionFailed);
            }
        }

        public async Task DeleteAsync(IEnumerable<SnipInsightLink> items, bool showConfirmation, bool raiseOutcomeTrigger)
        {
            ProgressControl progressMessage = null;

            // Filter out items that are already in the process of deleting!
            items = items.Where(i => i.DeletionPending == false).ToArray();

            int itemCount = items.Count();
            int failedCount = 0;

            if (itemCount == 0)
            {
                // There is nothing to do
                return;
            }

            try
            {
                // Mark all items as pending deletion
                foreach (var item in items)
                {
                    item.DeletionPending = true;
                }

                // Confirmation
                bool hasMixIds = items.Any(i => i.MixId != null);

                if (showConfirmation)
                {
                    bool deleteConfirmed = ShowDeleteConfirmation(itemCount, hasMixIds);

                    if (!deleteConfirmed) // No was clicked.
                    {
                        // Restore pending items
                        foreach (var item in items)
                        {
                            item.DeletionPending = false;
                        }

                        if (raiseOutcomeTrigger)
                        {
                            ViewModel.StateMachine.Fire(SnipInsightTrigger.DeletionCancelled);
                        }
                        return;
                    }
                }

                //
                // Do the actual delete
                //

                if (itemCount > 5 || hasMixIds)
                {
                    // Only bother with the Progress Bar if we think it might be slow...
                    // A large number or we need to call the server...

                    progressMessage = new ProgressControl();
                    progressMessage.ShowInMainWindow();
                }

                // If the Selected Package was deleted, we'll keep track of it's location
                int deletedSelectedPackageIndex = -1;

                int processedCount = 0;

                foreach (var item in items)
                {
                    var deletedIndex = GetCurrentContentIndexInLibrary(item);

                    var success = DeleteCore(item);

                    processedCount++;

                    if (progressMessage != null)
                    {
                        progressMessage.SetProgress(processedCount / itemCount);
                    }

                    if (success == true)
                    {
                        if (ViewModel.SelectedPackage != null && ViewModel.SelectedPackage.Url == item.Url)
                        {
                            // We just deleted the Selected Package! Remember this so we can choose a replacement
                            deletedSelectedPackageIndex = deletedIndex;
                            ViewModel.SelectedPackage = null;
                        }
                    }
                    else
                    {
                        failedCount++;

                        // Restore the item so the user can try again
                        item.DeletionPending = false;
                    }
                }

                if (progressMessage != null)
                {
                    progressMessage.Dismiss();
                    progressMessage = null;
                }

                //
                // Restore Selected Package
                //

                if (deletedSelectedPackageIndex != -1)
                {
                    // We deleted the SelectedPackage, so we need to restore it to something...
                    SelectPackageAfterDelete(deletedSelectedPackageIndex, raiseOutcomeTrigger);
                }
                else
                {
                    if (raiseOutcomeTrigger)
                    {
                        // We don't currently have a state for DeletionCompleted, but Cancelled should
                        // leave us in the same place
                        ViewModel.StateMachine.Fire(SnipInsightTrigger.DeletionCancelled);
                    }
                }

                //
                // Report any failures
                //

                if (failedCount > 0)
                {
                    Diagnostics.LogTrace(string.Format("Deletion failed for {0} item(s).", failedCount));
                    ToastControl toast = new ToastControl(string.Format(Resources.Delete_Failed_List, failedCount));
                    toast.ShowInMainWindow();
                    if (raiseOutcomeTrigger)
                    {
                        ViewModel.StateMachine.Fire(SnipInsightTrigger.DeletionFailed);
                    }

                }
            }
            catch (Exception ex)
            {
                if (progressMessage != null)
                {
                    progressMessage.Dismiss();
                    progressMessage = null;
                }

                Diagnostics.ReportException(ex);
                ToastControl toast = new ToastControl(Resources.Delete_Failed);
                toast.ShowInMainWindow();
                ViewModel.StateMachine.Fire(SnipInsightTrigger.DeletionFailed);
            }
            finally
            {
                //
                // Restore all items.
                // This is okay because those that were deleted should already be out of the list by now.
                //

                foreach (var item in items)
                {
                    item.DeletionPending = false;
                }

            }
        }

        private bool ShowDeleteConfirmation(int itemCount, bool containsUploadedMixes)
        {
            string message;

            if (itemCount == 1)
            {
                message = Resources.Confirm_Delete;
            }
            else
            {
                message = string.Format(Resources.Confirm_Delete_List, itemCount);
            }

            var twoButtonDialog = new TwoButtonDialog(message, "Yes", "No");
            twoButtonDialog.Owner = MainWindow;

            twoButtonDialog.ShowDialog();

            return !twoButtonDialog.Button2Clicked;
        }

        private int GetCurrentContentIndexInLibrary(SnipInsightLink link)
        {
            var packages = ViewModel.Packages;

            for (int i = 0; i < packages.Count; i++)
            {
                if (link.Url == ViewModel.Packages[i].Url)
                {
                    return i;
                }
            }

            return -1;
        }

        private int GetCurrentContentIndexInLibrary(string SnipInsightFile, string savedCaptureImage, string savedInkedImage)
        {
            if (string.IsNullOrEmpty(SnipInsightFile) &&
                string.IsNullOrEmpty(savedInkedImage) &&
                string.IsNullOrEmpty(savedCaptureImage))
            {
                return -1;
            }
            for (int i = 0; i < ViewModel.Packages.Count; i++)
            {
                if (!string.IsNullOrEmpty(SnipInsightFile) && ViewModel.Packages[i].Url == SnipInsightFile)
                {
                    return i;
                }
                if (!string.IsNullOrEmpty(savedInkedImage) && ViewModel.Packages[i].Url == savedInkedImage)
                {
                    return i;
                }
                if (!string.IsNullOrEmpty(savedCaptureImage) && ViewModel.Packages[i].Url == savedCaptureImage)
                {
                    return i;
                }
            }
            return -1;
        }


        //TODO: Delete packages
        private void SelectPackageAfterDelete(int deletedIndex, bool raiseOutcomeTrigger)
        {
            // Whatever was in deletedIndex was deleted and should now have the next item from the lib or if last item was deleted, it is outside lib.
            if (deletedIndex >= ViewModel.Packages.Count)
            {
                deletedIndex = ViewModel.Packages.Count - 1;
            }
            if (deletedIndex == -1)
            {
                if (raiseOutcomeTrigger)
                {
                    ViewModel.StateMachine.Fire(SnipInsightTrigger.WhiteboardForCurrentWindow);
                }
            }
            else
            {
                var currentLink = ViewModel.Packages[deletedIndex];
                ViewModel.SelectedPackage = currentLink;
                if (raiseOutcomeTrigger)
                {
                    if (currentLink.HasPackage)
                    {
                        ViewModel.StateMachine.Fire(SnipInsightTrigger.LoadPackageFromLibrary);
                    }
                    else
                    {
                        ViewModel.StateMachine.Fire(SnipInsightTrigger.LoadImageFromLibrary);
                    }
                }
            }
        }

        private void OnAfterHidePanel()
        {
            if (ViewModel.SelectedPackage == null)
            {
                if (ViewModel.Packages.Count > 0)
                {
                    ViewModel.SelectedPackage = ViewModel.Packages[0];
                }
                else
                {
                    ViewModel.StateMachine.Fire(SnipInsightTrigger.WhiteboardForCurrentWindow);
                    return;
                }
            }

            if (Path.GetExtension(ViewModel.SelectedPackage.Url) == ".mixp")
            {
                ViewModel.StateMachine.Fire(SnipInsightTrigger.LoadPackageFromLibrary);
            }
            else
            {
                ViewModel.StateMachine.Fire(SnipInsightTrigger.LoadImageFromLibrary);
            }
        }

        private void OnBeforeShowPanel()
        {
            // If current image is whitEimage with no saved file, then do not select anything. Else, select what is currently loaded.
            if (ViewModel.IsWhiteboardImage && ViewModel.SavedInkedImage == null && ViewModel.SavedSnipInsightFile == null)
            {
                ViewModel.SelectedPackage = null;
            }
            else
            {
                // The curent image can be one that is currently captured and worked on OR an item from library. Find matching one in packages.
                // If user loads a lib item and then capure a new one, the selectedpackage is pointing to lib item. So, we need to select the
                // item matching current capture. If item was loaded from library, then SelectedPackage will match but no harm to find match.
                var currentUrl = ViewModel.SavedSnipInsightFile ??
                                 (ViewModel.SavedInkedImage ?? ViewModel.SavedCaptureImage);
                ViewModel.SelectedPackage = ViewModel.Packages.FirstOrDefault(x => x.Url == currentUrl);
            }
        }

        /// <summary>
        /// Changes the state to show the library panel
        /// </summary>
        void OnShowHideLibrary()
        {
            OnBeforeShowPanel();
            ViewModel.StateMachine.Fire(SnipInsightTrigger.ShowLibraryPanel);
        }

        /// <summary>
        /// Changes the state to show the settings panel
        /// </summary>
        void OnShowHideSettings()
        {
            OnBeforeShowPanel();
            ViewModel.StateMachine.Fire(SnipInsightTrigger.ShowSettingsPanel);
        }

        /// <summary>
        /// Changes the state to show the editor
        /// </summary>
        internal void OnShowHideEditor()
        {
            SwitchNavButton(NavBarButtons.AIPanel);
            OnAfterHidePanel();
        }

        /// <summary>
        /// Changes the state to show the AI panel
        /// </summary>
        internal void OnShowHideAIPanel()
        {
            OnBeforeShowPanel();
            ViewModel.StateMachine.Fire(SnipInsightTrigger.ShowAIPanel);
        }

        /// <summary>
        /// Replace the highlighted button of the current panel selected
        /// </summary>
        /// <param name="button">NavBarButtons enum indicating the button to be set</param>
        void SwitchNavButton(NavBarButtons? button)
        {
            ReEnableNavBar(disabledNavButton, true);
            disabledNavButton = button;
            ReEnableNavBar(disabledNavButton, false);
        }

        /// <summary>
        /// Re-enables the previously disabled navbar button
        /// </summary>
        /// <param name="button">NavBarButtons enum indicating the button to be changed</param>
        /// <param name="value">True to enable a button, false to disable</param>
        void ReEnableNavBar(NavBarButtons? button, bool value)
        {
            switch(button)
            {
                case NavBarButtons.AIPanel:
                    ViewModel.AIEnable = value;
                    break;
                case NavBarButtons.Editor:
                    ViewModel.EditorEnable = value;
                    break;
                case NavBarButtons.Library:
                    ViewModel.LibraryEnable = value;
                    break;
                case NavBarButtons.Settings:
                    ViewModel.SettingsEnable = value;
                    break;
                default:
                    break;
            }
        }

        void LoadImageFromLibrary()
        {
            if (!string.IsNullOrEmpty(ViewModel.RestoreImageUrl))
            {
                ViewModel.InkedImage = null;
                ViewModel.CapturedImage = new BitmapImage(new Uri(ViewModel.SelectedImageUrl));
                ViewModel.SavedCaptureImage = ViewModel.RestoreImageUrl;
                AiCoreManager.ImageBytes = BitmapToStream(ViewModel.CapturedImage).GetBuffer();
                ViewModel.IsWhiteboardImage = false;
            }
            //TODO: Delete package
            else if (ViewModel.SelectedPackage != null)
            {
                ViewModel.InkedImage = null;
                ViewModel.CapturedImage = SnipInsightLink.CreateBitmapSource(ViewModel.SelectedPackage.ImageStream);
                ViewModel.SavedCaptureImage = ViewModel.SelectedPackage.Url;
                AiCoreManager.ImageBytes = BitmapToStream(ViewModel.CapturedImage).GetBuffer();

                // Since we don't store whiteboard image itself in file system, the images from lib should never be whiteboard image.
                ViewModel.IsWhiteboardImage = false;
                ViewModel.RestoreImageUrl = string.Empty;
                if (!ViewModel.AIAlreadyRan)
                {
                    RunAllInsights();
                }
                ViewModel.AIAlreadyRan = false;
            }
            ViewModel.Mode = Mode.Captured;
            MainWindow.SetInsightVisibility(Visibility.Visible);
        }

        void OnCleanFiles()
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(ViewModel.SavedSnipInsightFile))
                {
                    if (!String.IsNullOrWhiteSpace(ViewModel.SavedCaptureImage))
                    {
                        _snipInsightsManager.DeleteImage(ViewModel.SavedCaptureImage);
                    }
                    if (!String.IsNullOrWhiteSpace(ViewModel.SavedInkedImage))
                    {
                        _snipInsightsManager.DeleteImage(ViewModel.SavedInkedImage);
                    }
                }
                else
                {
                    if (!String.IsNullOrWhiteSpace(ViewModel.SavedInkedImage))
                    {
                        if (!String.IsNullOrWhiteSpace(ViewModel.SavedCaptureImage))
                        {
                            _snipInsightsManager.DeleteImage(ViewModel.SavedCaptureImage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Deletion is best-effort
                Diagnostics.LogLowPriException(ex);
            }
        }


        public bool DeleteCore(SnipInsightLink SnipInsight)
        {
            bool isPackage = Path.GetExtension(SnipInsight.Url) == ".mixp";

            if (isPackage)
            {
                return DeleteCore(SnipInsight.Url, null, null);
            }
            else
            {
                // I don't know how to detect if we have ink or not.
                return DeleteCore(null, SnipInsight.Url, null);
            }
        }


        /// <summary>
        /// Delete files core.
        /// </summary>
        /// <returns></returns>
        public bool DeleteCore(string SnipInsightFile, string savedCaptureImage, string savedInkedImage)
        {
            // delete the captured image.
            if (!string.IsNullOrWhiteSpace(savedCaptureImage))
            {
                try
                {
                    _snipInsightsManager.DeleteImage(savedCaptureImage);
                }
                catch (Exception ex)
                {
                    Diagnostics.LogLowPriException(ex);
                    return false;
                }
            }
            // delete the inked image.
            if (!string.IsNullOrWhiteSpace(savedInkedImage))
            {
                try
                {
                    _snipInsightsManager.DeleteImage(savedInkedImage);
                }
                catch (Exception ex)
                {
                    Diagnostics.LogLowPriException(ex);
                    return false;
                }
            }
            return true;
        }

        internal void OnSaveImage()
        {
            try
            {
                if (!string.IsNullOrEmpty(ViewModel.RestoreImageUrl))
                {
                    ViewModel.SelectedPackage = ViewModel.Packages[0];
                }
                // if captured image is already not saved, save it.
                if (ViewModel.CapturedImage != null && !ViewModel.IsWhiteboardImage && string.IsNullOrEmpty(ViewModel.SavedCaptureImage))
                {
                    // Save the image.
                    using (MemoryStream captured = BitmapToStream(ViewModel.CapturedImage))
                    {
                        ViewModel.SavedCaptureImage = _snipInsightsManager.SaveImage(captured);
                    }
                }
                // if acetate layer has ink -> save it.
                // else delete the existing inked image.
                if (MainWindow.acetateLayer.HasInk())
                {
                    if (MessageBoxResult.No == MessageBox.Show(Resources.Commit_Changes,
                        "Confirm",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question))
                    {
                        DeleteInkedImage();
                    }
                    else
                    {
                        SaveInkedImage();
                        string path = ViewModel.SavedInkedImage ?? ViewModel.SavedCaptureImage;
                        ViewModel.CapturedImage = new BitmapImage(new Uri(path));
                        ViewModel.SavedInkedImage = null;
                        ViewModel.SavedCaptureImage = path;
                        ViewModel.SelectedPackage = ViewModel.Packages[0];
                    }
                }
                else
                {
                    DeleteInkedImage();
                }
                MainWindow.AcetateLayer.InkCanvas.Clear();
            }
            catch (Exception ex)
            {
                Diagnostics.ReportException(ex);
            }
        }

        void OnSaveImageWithDialog()
        {
            Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.SaveImageButton, Telemetry.ViewName.ActionRibbon);
            string[] validPictureExtensions = { ".png", ".jpg", ".jpeg", ".bmp"};
            var invalidChars = string.Format(@"[{0}]+", (new string(Path.GetInvalidFileNameChars())));

            try
            {
                if (ViewModel.CapturedImage == null && ViewModel.InkedImage == null)
                {
                    return;
                }

                // This is just a static image (possibly with ink)
                SaveFileDialog dlg = new SaveFileDialog
                {
                    DefaultExt = ".png",
                    Filter = "PNG image|*.png|JPEG image|*.jpg;*.jpeg|Bitmap image|*.bmp",
                    FileName = string.Format(
                        "snip_{0} {1}",
                        DateTimeOffset.Now.ToString(Resources.Culture.DateTimeFormat.ShortDatePattern),
                        DateTimeOffset.Now.ToString(Resources.Culture.DateTimeFormat.ShortTimePattern))
                };

                if (UserSettings.IsAutoTaggingEnabled && ImageMetadata.CaptionAvailable)
                    dlg.FileName = string.Concat(dlg.FileName, "_", ImageMetadata.Caption);

                dlg.FileName = Regex.Replace(dlg.FileName, invalidChars, "-");

                if (dlg.FileName.Length > 255)
                {
                    dlg.FileName = dlg.FileName.Substring(0, 255);
                }

                var lastDirectory = Settings.Default.LastSaveImageDirectory;

                if (!string.IsNullOrWhiteSpace(lastDirectory) && Directory.Exists(lastDirectory))
                {
                    dlg.InitialDirectory = lastDirectory;
                }
                else
                {
                    dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                }

                if (dlg.ShowDialog() == true)
                {
                    Settings.Default.LastSaveImageDirectory = Path.GetDirectoryName(dlg.FileName);
                    Settings.Default.Save();

                    if (!validPictureExtensions.Contains(Path.GetExtension(dlg.FileName).ToLowerInvariant()))
                    {
                        MessageBox.Show(string.Format("File saved in unrecognized image type: {0}", Path.GetExtension(dlg.FileName)), "Warning");
                    }

                    WriteNamedFile(dlg.FileName);
                }
            }
            catch (Exception ex)
            {
                Diagnostics.ReportException(ex);
            }
            finally
            {
                ViewModel.StateMachine.Fire(SnipInsightTrigger.SavingImageWithDialogCompleted);
            }
        }

        /// <summary>
        /// Write image into a file as per user choice in save dialog
        /// </summary>
        /// <param name="filePathName">string containing full path, name and extension</param>
        private void WriteNamedFile(string filePathName)
        {
            if (MainWindow.AcetateLayer.HasInk())
            {
                ViewModel.InkedImage = PictureConverter.GenerateSnapshot(MainWindow.contentImage, MainWindow.acetateLayer.InkCanvas);
            }

            BitmapSource bitmap = ViewModel.InkedImage ?? ViewModel.CapturedImage;
            string extension = Path.GetExtension(filePathName).ToLowerInvariant();

            using (var stream = File.OpenWrite(filePathName))
            {
                switch (extension)
                {
                    case ".jpg":
                    case ".jpeg":
                        PictureConverter.SaveToJpg(bitmap, stream);
                        break;
                    case ".bmp":
                        PictureConverter.SaveToBmp(bitmap, stream);
                        break;
                    case ".png":
                    default:
                        PictureConverter.SaveToPng(bitmap, stream);
                        break;
                }
            }

            if (UserSettings.IsAutoTaggingEnabled && (extension == ".jpg" || extension == ".jpeg"))
            {
                WriteMetadata(filePathName);
            }
        }

        /// <summary>
        /// Write available metadata to files.
        /// </summary>
        /// <param name="filePathName">string containing full path, name and extension</param>
        private void WriteMetadata(string filePathName)
        {
            using (var file = ShellFile.FromFilePath(filePathName))
            {
                // If metadata is available, add it
                if (ImageMetadata.TagsAvailable)
                    file.Properties.System.Keywords.Value = ImageMetadata.Tags;

                if (ImageMetadata.CaptionAvailable)
                    file.Properties.System.Title.Value = ImageMetadata.Caption;
            }
        }

        /// <summary>
        /// Returns the result of user response for content moderation warning
        /// </summary>
        /// <returns>true if sharing is to be blocked, false if sharing is allowed</returns>
        bool IsBlockedByContentModeration()
        {
            bool warning = false;
            if (UserSettings.ContentModerationStrength == 0)
            {
                return false;
            }
            else if (UserSettings.ContentModerationStrength == 100)
            {
                warning = true;
            }

            using (MemoryStream stream = BitmapToStream(ViewModel.CapturedImage))
            {
                if (warning || contentModerationHandler.GetResult(stream))
                {
                    var result = MessageBox.Show(Resources.ShareModerateWarning, "Warning", MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                    if (result == MessageBoxResult.No)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        void OnShareEmailWithImage()
        {
            Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.SaveImageEmailButton, Telemetry.ViewName.ActionRibbon);
            try
            {
                if (IsBlockedByContentModeration())
                {
                    return;
                }

                OnSaveImage();

                string file = ViewModel.SavedCaptureImage;
                if (!String.IsNullOrWhiteSpace(ViewModel.SavedInkedImage))
                {
                    file = ViewModel.SavedInkedImage;
                }

                string subject = Resources.Sharing_Snip;
                // TODO: Fix the attachment name
                string attachmentName = "Capture.png";
                if (UserSettings.IsAutoTaggingEnabled && ImageMetadata.CaptionAvailable)
                {
                    subject = string.Format(Resources.Sharing_Snip_Name, ImageMetadata.Caption);
                    attachmentName = string.Format("{0}.png",ImageMetadata.Caption);
                    if (attachmentName.Length > 255)
                    {
                        attachmentName = attachmentName.Substring(0, 255);
                    }
                }

                // Try and open email client with attachment.
                if (!EmailManager.OpenEmailClientWithEmbeddedImage(file, subject, attachmentName, "image/png"))
                {
                    File.Delete(file);
                    bool success;
                    // Do Work
                    if (ViewModel.InkedImage != null)
                    {
                        success = ClipboardManager.Copy(ViewModel.InkedImage);
                    }
                    else if (ViewModel.CapturedImage != null)
                    {
                        success = ClipboardManager.Copy(ViewModel.CapturedImage);
                    }
                    else
                    {
                        throw new InvalidOperationException("no image to share");
                    }

                    if (success)
                    {
                        string mailToImageFormat = "mailto:?subject=" + subject + "&body=Capture copied to clipboard. please paste here";
                        Process.Start(mailToImageFormat);
                    }
                    else
                    {
                        ToastControl toast = new ToastControl(Resources.Message_CopyToClipboardFailed);
                        toast.ShowInMainWindow();
                    }
                }
            }
            catch (Exception ex)
            {
                Diagnostics.ReportException(ex);
            }
            finally
            {
                // Trigger
                ViewModel.StateMachine.Fire(SnipInsightTrigger.SharingWithImageCompleted);
            }
        }

        // TODO: Check which OneNote function is used
        void OnShareSendToOneNoteWithImage()
        {
            Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.ShareImageSendToOneNoteButton, Telemetry.ViewName.ActionRibbon);
            try
            {
                if (IsBlockedByContentModeration())
                {
                    return;
                }

                OnSaveImage();

                string imageFilePath = ViewModel.SavedCaptureImage;
                if (!String.IsNullOrWhiteSpace(ViewModel.SavedInkedImage))
                {
                    imageFilePath = ViewModel.SavedInkedImage;
                }

                using (OneNoteManager oneNoteMgr = new OneNoteManager())
                {
                    bool? success = oneNoteMgr.InsertSnip(imageFilePath);

                    if (success.HasValue)
                    {
                        ToastControl toast = new ToastControl(success.Value ? Properties.Resources.Message_SendToOneNote_Succeeded : Properties.Resources.Message_SendToOneNote_Failed, 3000);
                        toast.ShowInMainWindow();
                    }
                }
            }
            finally
            {
                // Trigger
                ViewModel.StateMachine.Fire(SnipInsightTrigger.SharingWithImageCompleted);
            }
        }

        /// <summary>
        /// Reload the actual snipped/edited image to the panel content
        /// </summary>
        void OnRestoreImage()
        {
            Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.RestoreImageButton, Telemetry.ViewName.ActionRibbon);
            ImageLoader.LoadFromUrl(new Uri(ViewModel.SavedCaptureImage)).ContinueWith(t =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ServiceLocator.Current.GetInstance<AIPanelViewModel>().CapturedImage = t.Result;
                    ViewModel.SavedCaptureImage = ViewModel.RestoreImageUrl;
                    ViewModel.RestoreImageUrl = string.Empty;
                    ViewModel.SelectedImageUrl = string.Empty;
                    RunAllInsights();
                });

            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        /// <summary>
        /// Save edits and make the API calls to refresh AI
        /// </summary>
        void OnRefreshAI()
        {
            Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.RefreshAICommandButton, Telemetry.ViewName.ActionRibbon);
            OnSaveImage();
            RunAllInsights();
        }

        void OnCopyWithImage()
        {
            Telemetry.ApplicationLogger.Instance.SubmitButtonClickEvent(Telemetry.EventName.CopyImageButton, Telemetry.ViewName.ActionRibbon);
            try
            {
                // copy to clipboard.
                bool success;
                // Do Work
                if (MainWindow.acetateLayer.HasInk())
                {
                    ViewModel.InkedImage = PictureConverter.GenerateSnapshot(MainWindow.contentImage, MainWindow.acetateLayer.InkCanvas);
                    success = ClipboardManager.Copy(ViewModel.InkedImage);
                }
                else if (ViewModel.CapturedImage != null)
                {
                    success = ClipboardManager.Copy(ViewModel.CapturedImage);
                }
                else
                {
                    throw new InvalidOperationException("no image to copy");
                }
                ToastControl toast = new ToastControl(success ? Resources.Message_CopiedToClipboard : Resources.Message_CopyToClipboardFailed);
                toast.ShowInMainWindow();
            }
            catch (Exception ex)
            {
                Diagnostics.ReportException(ex);
            }
            finally
            {
                // Trigger
                ViewModel.StateMachine.Fire(SnipInsightTrigger.CopyingWithImageCompleted);
            }
        }
        #endregion

        #region Events
        void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow.Loaded -= OnLoaded;
                MainWindow.Closed += OnClosed;
            }
            catch (Exception ex)
            {
                Diagnostics.ReportException(ex);
            }
        }

        private void OnClosed(object sender, EventArgs e)
        {
            var mainWindow = MainWindow;
        }
        #endregion

        #region ImageCapture

        /// <summary>
        /// Prepare the capture manager for the snip
        /// </summary>
        void SetupImageCaptureManager()
        {
            var imageCaptureDispose = _imageCapture as IDisposable;
            if (imageCaptureDispose != null)
            {
                imageCaptureDispose.Dispose();
            }

            _imageCapture = new ImageCaptureManager();
        }

        /// <summary>
        /// Initialize a regular shot with a post-snip editor
        /// </summary>
        void InitializeCaptureImage()
        {
            SetupImageCaptureManager();
            _imageCapture.CaptureCompleted += ImageCaptureCompleted;
        }

        /// <summary>
        /// Initialize a quick snip with no editor
        /// </summary>
        void InitializeQuickCapture()
        {
            SetupImageCaptureManager();
            _imageCapture.CaptureCompleted += QuickCaptureCompleted;
        }

        void SaveMainWindowState()
        {
            if (MainWindow != null)
            {
                _mainWindowVisibilityBeforeCapture = MainWindow.Visibility;
                _mainWindowStateBeforeCapture = MainWindow.WindowState;
            }
        }

        private Visibility _mainWindowVisibilityBeforeCapture;
        private WindowState _mainWindowStateBeforeCapture;

        internal void RestoreMainWindow()
        {
            ShowMainWindowInternal(_mainWindowStateBeforeCapture);
        }

        internal void RestoreImage()
        {
            if (ViewModel.IsWhiteboardImage && ViewModel.InkedImage == null)
            {
                ViewModel.StateMachine.Fire(SnipInsightTrigger.RestoreWhiteboard);
                return;
            }
            if (ViewModel.Packages.Count > 0)
            {
                if (ViewModel.SelectedPackage == null) // Indicated that current content was not loaded from library
                {
                    ViewModel.SelectedPackage = ViewModel.Packages[0]; // Go back to the latest capture. This matches what the user was manipulating last.
                }
                LoadImageFromLibrary();
                ViewModel.StateMachine.Fire(SnipInsightTrigger.RestoreImage);
            }
        }

        internal void RestoreLibrary()
        {
            ViewModel.StateMachine.Fire(SnipInsightTrigger.RestoreLibrary);
        }

        internal void RestoreSettings()
        {
            ViewModel.StateMachine.Fire(SnipInsightTrigger.RestoreSettings);
        }

        void StartCaptureImage()
        {
            if (_imageCapture != null)
            {
                int delay = UserSettings.ScreenCaptureDelay;
                if (delay > 0)
                {
                    Thread.Sleep(delay * 1000);
                }
                _imageCapture.StartCapture();
            }
        }

        void ClearOldImageData()
        {
            var viewModel = ViewModel;
            if (viewModel != null)
            {
                viewModel.CapturedImage = null;
                viewModel.InkedImage = null;
                viewModel.SavedInkedImage = null;
                viewModel.SavedCaptureImage = null;
                viewModel.SavedSnipInsightFile = null;
                viewModel.HasInk = false;
            }
            if (MainWindow != null)
            {
                MainWindow.acetateLayer.InkCanvas.Clear();
            }
        }

        /// <summary>
        /// General behaviour no matter the type of screenshot
        /// </summary>
        private bool GeneralCaptureCompleted(ImageCaptureEventArgs e)
        {
            // Set the image on window.content
            if (e.Image == null)
            {
                ViewModel.StateMachine.Fire(SnipInsightTrigger.ImageCaptureCancelled);
                return false;
            }

            ClearOldImageData();
            ViewModel.CapturedImage = e.Image;
            AiCoreManager.ImageBytes = BitmapToStream(e.Image).GetBuffer();

            ViewModel.IsWhiteboardImage = false;
            ViewModel.RestoreImageUrl = string.Empty;
            MainWindow.SetInsightVisibility(Visibility.Visible);

            ViewModel.EditorEnable = true;
            ViewModel.AIEnable = true;
            SwitchNavButton(NavBarButtons.AIPanel);

            return true;
        }

        /// <summary>
        /// Take a screenshot of the screen but do not open the editor
        /// </summary>
        private void QuickCaptureCompleted(object sender, ImageCaptureEventArgs e)
        {
            if (!GeneralCaptureCompleted(e))
            {
                return;
            }

            if (UserSettings.IsNotificationToastEnabled)
            {
                // Post QuickSnip Toast
                NotificationWindow toastNotification = new NotificationWindow();
                toastNotification.Show();
            }

            // Fire The QuickSnip trigger
            ViewModel.StateMachine.Fire(SnipInsightTrigger.QuickSnip);
        }

        /// <summary>
        /// Take a screenshot of the screen and open the editor if specified
        /// </summary>
        private void ImageCaptureCompleted(object sender, ImageCaptureEventArgs e)
        {
            if (UserSettings.IsOpenEditorPostSnip && GeneralCaptureCompleted(e))
            {
                ViewModel.StateMachine.Fire(SnipInsightTrigger.ImageCaptured);
            }
            else
            {
                QuickCaptureCompleted(sender, e);
            }
        }

        internal void CloseImageCapture()
        {
            IDisposable imageCaptureDispose = _imageCapture as IDisposable;
            if (imageCaptureDispose != null)
            {
                imageCaptureDispose.Dispose();
                _imageCapture = null;
            }
        }


        MemoryStream BitmapToStream(BitmapSource image)
        {
            MemoryStream capturedImage = PictureConverter.SaveToPng(image, new MemoryStream());
            capturedImage.Position = 0;
            return capturedImage;
        }

        #endregion

        #region AI Panel
        public enum EditorTools
        {
            Pen = 1,
            Highlighter = 2,
            Eraser = 3,
        }
        public void ResetEditorButtons(EditorTools button)
        {
            if (button != EditorTools.Eraser)
            {
                ViewModel.EraserChecked = false;
            }
            if (button != EditorTools.Highlighter)
            {
                ViewModel.HighlighterChecked = false;
            }
            if (button != EditorTools.Pen)
            {
                ViewModel.PenChecked = false;
            }
            ViewModel.penSelected = false;
        }

		/// <summary>
		/// Runs all the image insights
		/// </summary>
		internal void RunAllInsights()
		{
			SwitchNavButton(NavBarButtons.AIPanel);
            if (UserSettings.IsAIEnabled)
            {
                AiCoreManager.ImageBytes = BitmapToStream(ViewModel.CapturedImage).GetBuffer();
                AiCoreManager.RunAllAsyncCalls();

                // Resets the scroll viewer back to the top after insights refresh
                AppManager.TheBoss.MainWindow.VerticalScrollViewer.ScrollToTop();
            }
		}
        #endregion

        /// <summary>
        /// The return value for operations (especialy service related) to help us give better error information.
        /// </summary>
        public class OperationResult
        {
            /// <summary>
            ///  Indicates the outcome of the operation.
            /// </summary>
            public bool Succeeded { get; set; }
        }

        #region Share
        void Publish(Action<OperationResult> continueWith, string failureMessageForToast, bool embedCodeNeeded = false)
        {
            if (continueWith == null)
            {
                throw new ArgumentNullException("continueWith");
            }
            if (string.IsNullOrWhiteSpace(ViewModel.SavedSnipInsightFile) || !File.Exists(ViewModel.SavedSnipInsightFile))
            {
                ToastControl toast = new ToastControl("File not found!");
                toast.ShowInMainWindow();
                continueWith(new OperationResult { Succeeded = false });
            };
        }
        #endregion

        #region Save
        /// <summary>
        /// Save inked image from the canvas.
        /// </summary>
        private void SaveInkedImage()
        {
            if (MainWindow == null || MainWindow.contentImage == null)
            {
                return;
            }
            // Generate the snapshot always.
            ViewModel.InkedImage = PictureConverter.GenerateSnapshot(MainWindow.contentImage, MainWindow.acetateLayer.InkCanvas);
            // delete the old image
            if (!String.IsNullOrWhiteSpace(ViewModel.SavedInkedImage))
            {
                _snipInsightsManager.DeleteImage(ViewModel.SavedInkedImage);
            }
            // save the inked image
            using (MemoryStream ms = new MemoryStream())
            {
                PictureConverter.SaveToPng(ViewModel.InkedImage, ms);
                ms.Position = 0;
                ViewModel.SavedInkedImage = _snipInsightsManager.SaveImage(ms);
            }
        }
        #endregion

        #region Delete
        private void DeleteInkedImage()
        {
            // delete the old image.
            ViewModel.InkedImage = null;
            if (!String.IsNullOrWhiteSpace(ViewModel.SavedInkedImage))
            {
                _snipInsightsManager.DeleteImage(ViewModel.SavedInkedImage);
            }
            ViewModel.SavedInkedImage = null;
        }
        #endregion

        #region KeyBoard
        /// <summary>
        /// Register the hotkeys at the start of the application
        /// </summary>
        private void RegisterHotKeys()
        {
            try
            {
                ToolWindow.RegisterHotKey(SnipHotKey.ScreenCapture, UserSettings.ScreenCaptureShortcut);
                ToolWindow.RegisterHotKey(SnipHotKey.QuickCapture, UserSettings.QuickCaptureShortcut);
                ToolWindow.HotKeyPressed += ToolWindow_HotKeyPressed;
            }
            catch (Exception ex)
            {
                Diagnostics.LogException(ex);
            }
        }

        /// <summary>
        /// Handler for the hotkey press detection. Fire the correct trigger.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ToolWindow_HotKeyPressed(object sender, HotKeyPressedEventArgs e)
        {
            switch (e.KeyPressed)
            {
                case SnipHotKey.ScreenCapture:
                    ViewModel.StateMachine.Fire(SnipInsightTrigger.CaptureScreen);
                    break;
                case SnipHotKey.QuickCapture:
                    ViewModel.StateMachine.Fire(SnipInsightTrigger.QuickSnip);
                    break;
            }
        }
        #endregion
    }
}