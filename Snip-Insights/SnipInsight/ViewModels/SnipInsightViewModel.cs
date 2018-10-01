// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using SnipInsight.Package;
using SnipInsight.Properties;
using SnipInsight.StateMachine;
using SnipInsight.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SnipInsight.ViewModels
{
    public enum Mode
    {
        Initializing,
        Capturing,
        Captured,
        Recording,
        PausedRecording,
        Saving,
        Stopped,
        Playing,
        Exporting
    }

    //
    // Since UI properties have dependencies on state properties, make
    // sure that any changes keep these relationship consistent
    //

    public sealed class SnipInsightViewModel : INotifyPropertyChanged
    {
        static SnipInsightViewModel()
        {
            // Ensure all static member variables are initialized at once

        }

        internal SnipInsightViewModel(Dictionary<string, Action> actions)
        {
            StateMachine = new StateMachine.StateMachine(actions);

            CaptureCommand = StateMachine.CreateCommand(SnipInsightTrigger.CaptureScreen);
            QuickCaptureCommand = StateMachine.CreateCommand(SnipInsightTrigger.QuickSnip);
            PhotoCommand = StateMachine.CreateCommand(SnipInsightTrigger.CaptureCamera);
            WhiteboardCommand = StateMachine.CreateCommand(SnipInsightTrigger.Whiteboard);
            WhiteboardForCurrentWindowCommand = StateMachine.CreateCommand(SnipInsightTrigger.WhiteboardForCurrentWindow);
            RecordCommand = StateMachine.CreateCommand(SnipInsightTrigger.Record);
            PauseCommand = StateMachine.CreateCommand(SnipInsightTrigger.Pause);
            StopCommand = StateMachine.CreateCommand(SnipInsightTrigger.Stop);
            PlayCommand = StateMachine.CreateCommand(SnipInsightTrigger.TogglePlayStop);
            ExitCommand = StateMachine.CreateCommand(SnipInsightTrigger.Exit);
            ShareLinkCommand = StateMachine.CreateCommand(SnipInsightTrigger.ShareLink);
            ShareEmbedCommand = StateMachine.CreateCommand(SnipInsightTrigger.ShareEmbed);
            ShareEmailCommand = StateMachine.CreateCommand(SnipInsightTrigger.ShareEmail);
            ShareSendToOneNoteCommand = StateMachine.CreateCommand(SnipInsightTrigger.ShareSendToOneNote);
            SaveCommand = StateMachine.CreateCommand(SnipInsightTrigger.Save);
            CopyCommand = StateMachine.CreateCommand(SnipInsightTrigger.Copy);
            DeleteCommand = StateMachine.CreateCommand(SnipInsightTrigger.Delete);
            ShowMainWindowCommand = StateMachine.CreateCommand(SnipInsightTrigger.ShowMainWindow); // Tool Window used to show main window for lib.
            ShowLibraryCommand = StateMachine.CreateCommand(SnipInsightTrigger.ShowLibraryPanel);
            ShowSettingsCommand = StateMachine.CreateCommand(SnipInsightTrigger.ShowSettingsPanel);
            MicrophoneOptionsCommand = StateMachine.CreateCommand(SnipInsightTrigger.ShowMicrophoneOptions);
            DeleteLibraryItemsCommand = new DelegateCommand(actions[ActionNames.DeleteLibraryItems]);
            DoImageInsightsCommand = StateMachine.CreateCommand(SnipInsightTrigger.DoImageInsights);
            ShowImageResultsWindowCommand = StateMachine.CreateCommand(SnipInsightTrigger.ShowImageResultsWindow);
            ShowAIPanelCommand = StateMachine.CreateCommand(SnipInsightTrigger.ShowAIPanel);
            HideAIPanelCommand = StateMachine.CreateCommand(SnipInsightTrigger.HideAIPanel);

            CloseMainWindowCommand = new DelegateCommand(actions[ActionNames.CloseMainWindow]); // Hide + EditingWindowClosed Trigger

            Assembly assembly = Assembly.GetEntryAssembly();
            var version = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
            this.appVersion = "Snip Insights " + version;
        }

        public StateMachine.StateMachine StateMachine { get; private set; }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Commands
        public ICommand ShowMainWindowCommand { get; set; }

        public ICommand CloseMainWindowCommand { get; set; }

        public ICommand CaptureCommand { get; set; }
        public ICommand PhotoCommand { get; set; }
        public ICommand WhiteboardCommand { get; set; }
        public ICommand WhiteboardForCurrentWindowCommand { get; set; }
        public ICommand RecordCommand { get; set; }
        public ICommand PauseCommand { get; set; }
        public ICommand StopCommand { get; set; }
        public ICommand PlayCommand { get; set; }
        public ICommand ExitCommand { get; set; }
        public ICommand EraserCommand { get; set; }
        public ICommand EraseAllCommand { get; set; }
        public ICommand UndoCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand ShareLinkCommand { get; set; }
        public ICommand ShareEmbedCommand { get; set; }
        public ICommand ShareEmailCommand { get; set; }
        public ICommand ShareSendToOneNoteCommand { get; set; }
        public ICommand CopyCommand { get; set; }
        public ICommand RedoCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand DeleteLibraryItemsCommand { get; set; }

        public ICommand ToggleLibraryCommand { get; set; }



        public ICommand ShowLibraryCommand { get; set; }
        public ICommand MicrophoneOptionsCommand { get; set; }

        public ICommand ToggleSettingsCommand { get; set; }
        public ICommand ShowSettingsCommand { get; set; }

        public ICommand QuickCaptureCommand { get; set; }

        public ICommand ShowImageResultsWindowCommand { get; set; }

        public ICommand DoImageInsightsCommand { get; set; }

        public ICommand ShowAIPanelCommand { get; set; }
        public ICommand HideAIPanelCommand { get; set; }
        public ICommand ToggleEditorCommand { get; set; }
        public ICommand ToggleAIPanelCommand { get; set; }
        public ICommand SaveImageCommand { get; set; }
        public ICommand RestoreImageCommand { get; set; }
        public ICommand CopyImageCommand { get; set; }
        public ICommand ShareImageEmailCommand { get; set; }
        public ICommand ShareImageSendToOneNoteCommand { get; set; }
        public ICommand RefreshAICommand { get; set; }
        #endregion

        #region Ink properties

        public DrawingAttributes InkDrawingAttributes
        {
            get
            {
                return _inkAttributes;
            }
        }
        readonly DrawingAttributes _inkAttributes = new DrawingAttributes()
        {
            Color = Colors.Black,
            IsHighlighter = false,
            Width = 5,
            Height = 5
        };

        public InkCanvasEditingMode InkModeRequested
        {
            get
            {
                return _inkModeRequested;
            }
            set
            {
                // Always notify of a mode prop set
                _inkModeRequested = value;
                OnPropertyChanged();
            }
        }
        InkCanvasEditingMode _inkModeRequested = InkCanvasEditingMode.Ink;

        #endregion

        #region State properties

        public Mode Mode
        {
            get { return _mode; }
            set
            {
                if (_mode != value)
                {
                    _mode = value;
                    OnPropertyChanged();
                    OnPropertyChanged("CaptureEnabled");
                    OnPropertyChanged("EraserEnabled");
                    OnPropertyChanged("RecordEnabled");
                    OnPropertyChanged("PlayEnabled");
                    OnPropertyChanged("RecordingInProgress");
                    OnPropertyChanged("RecordingNotInProgress");
                    OnPropertyChanged("RecordingOrPaused");
                    OnPropertyChanged("DeviceSelectionEnabled");
                    OnPropertyChanged("MicrophoneLevelEnabled");
                    OnPropertyChanged("AcceptingInk");
                    OnPropertyChanged("MainWindowTitle");
                    OnPropertyChanged("InsightsVisible");
                }
            }
        }
        Mode _mode = Mode.Initializing;

        public void SetRecordingTime(ulong timer)
        {
            // we only care about seconds granularity
            timer /= 1000;
            TimeSpan value = new TimeSpan((Int64)timer * TimeSpan.TicksPerSecond);
            if (_recordingTime != value)
            {
                _recordingTime = value;
                OnPropertyChanged("MainWindowTitle");
            }
        }
        TimeSpan _recordingTime;

        #endregion

        #region UI elements properties
        BitmapSource _capturedImage;
        BitmapSource _similarImage1;
        BitmapSource _similarImage2;
        BitmapSource _similarImage3;
        BitmapSource _inkedImage;
        String _ocrTextResults;
        TextBox _ocrTextBox;
        Canvas _celebritiesCanvas;

        Size _canvasSize = new Size(800, 480);

        public Size CanvasSize
        {
            get { return _canvasSize; }
            private set
            {
                if (_canvasSize != value)
                {
                    _canvasSize = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Captured image.
        /// </summary>
        public BitmapSource CapturedImage
        {
            get { return _capturedImage; }
            set
            {
                if (!Equals(_capturedImage, value))
                {
                    _capturedImage = value;
                    if (_capturedImage != null)
                    {
                        var virtualPixelWidth = _capturedImage.PixelWidth / _capturedImage.DpiX * 96.0;
                        var virtualPixelHeight = _capturedImage.PixelHeight / _capturedImage.DpiY * 96.0;
                        CanvasSize = new Size(virtualPixelWidth, virtualPixelHeight);
                    }

                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// Gets and sets first similar image
        /// </summary>
        public BitmapSource SimilarImage1
        {
            get { return _similarImage1; }
            set
            {
                if (!Equals(_similarImage1, value))
                {
                    _similarImage1 = value;
                    if (_similarImage1 != null)
                    {
                        var virtualPixelWidth = _similarImage1.PixelWidth / _similarImage1.DpiX * 96.0;
                        var virtualPixelHeight = _similarImage1.PixelHeight / _similarImage1.DpiY * 96.0;
                        CanvasSize = new Size(virtualPixelWidth, virtualPixelHeight);
                    }

                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets and sets second similar image
        /// </summary>
        public BitmapSource SimilarImage2
        {
            get { return _similarImage2; }
            set
            {
                if (!Equals(_similarImage2, value))
                {
                    _similarImage2 = value;
                    if (_similarImage2 != null)
                    {
                        var virtualPixelWidth = _similarImage2.PixelWidth / _similarImage2.DpiX * 96.0;
                        var virtualPixelHeight = _similarImage2.PixelHeight / _similarImage2.DpiY * 96.0;
                        CanvasSize = new Size(virtualPixelWidth, virtualPixelHeight);
                    }

                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets and sets third similar image
        /// </summary>
        public BitmapSource SimilarImage3
        {
            get { return _similarImage3; }
            set
            {
                if (!Equals(_similarImage3, value))
                {
                    _similarImage3 = value;
                    if (_similarImage3 != null)
                    {
                        var virtualPixelWidth = _similarImage3.PixelWidth / _similarImage3.DpiX * 96.0;
                        var virtualPixelHeight = _similarImage3.PixelHeight / _similarImage3.DpiY * 96.0;
                        CanvasSize = new Size(virtualPixelWidth, virtualPixelHeight);
                    }

                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets and sets OCR text string
        /// </summary>
        public String OCRTextResults
        {
            get { return _ocrTextResults; }
            set
            {
                if (!Equals(_ocrTextResults, value))
                {
                    _ocrTextResults = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets and sets OCR text box
        /// </summary>
        public TextBox OCRTextBox
        {
            get { return _ocrTextBox; }
            set
            {
                if (!Equals(_ocrTextBox, value))
                {
                    _ocrTextBox = value;
                    OnPropertyChanged();
                }
            }
        }


        public Canvas CelebritiesCanvas
        {
            get => _celebritiesCanvas;

            set
            {
                _celebritiesCanvas = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// Image capture with static ink overlayed.
        /// </summary>
        public BitmapSource InkedImage
        {
            get { return _inkedImage; }
            set
            {
                _inkedImage = value;
            }
        }

        public string MainWindowTitle
        {
            get
            {
                switch (_mode)
                {
                    case Mode.Recording:
                    case Mode.PausedRecording:
                    case Mode.Saving:
                        return _recordingTime.ToString("g");
                    default:
                        return Resources.WindowTitle_Main;
                }
            }
        }

        public bool CaptureEnabled
        {
            get { return _mode == Mode.Capturing; }
        }


        private bool _hasInk;
        public bool HasInk
        {
            get { return _hasInk; }
            set
            {
                if (_hasInk != value)
                {
                    _hasInk = value;
                    OnPropertyChanged();
                    OnPropertyChanged("CanShare");
                }
            }
        }

        public bool CanShare
        {
            get { return !IsWhiteboardImage || HasInk; }
        }

        /// <summary>
        /// Defines whether the editor button on the navbar is enabled
        /// </summary>
        private bool _editorEnable = false;

        public bool EditorEnable
        {
            get { return _editorEnable; }
            set
            {
                _editorEnable = value;
                OnPropertyChanged("EditorEnable");
            }
        }
        #endregion

        #region Saved Data Properties
        // These need to be reset for a new capture.
        public string SavedCaptureImage { get; set; }

        public string SavedInkedImage { get; set; }

        public string SavedSnipInsightFile { get; set; }

        private bool _isWhiteboardImage;

        public bool IsWhiteboardImage
        {
            get { return _isWhiteboardImage; }
            set
            {
                if (_isWhiteboardImage != value)
                {
                    _isWhiteboardImage = value;
                    OnPropertyChanged();
                    OnPropertyChanged("CanShare");
                }
            }
        }

        #endregion

        #region Library Panel Properties

        private IList _selectedLibraryItemsList;
        private int _selectedLibraryItemsCount;
        private SnipInsightLink _selectedPackage;

        private ObservableCollection<SnipInsightLink> _packages = new ObservableCollection<SnipInsightLink>();

        public ObservableCollection<SnipInsightLink> Packages
        {
            get { return _packages; }
        }

        public SnipInsightLink SelectedPackage
        {
            get { return _selectedPackage; }
            set
            {
                if (!Equals(_selectedPackage, value))
                {
                    _selectedPackage = value;
                    OnPropertyChanged("SelectedPackage");
                }
            }
        }

        internal IList SelectedLibraryItemsList
        {
            get { return _selectedLibraryItemsList; }
            set
            {
                if (_selectedLibraryItemsList != value)
                {
                    _selectedLibraryItemsList = value;

                    OnPropertyChanged("SelectedLibraryItems");
                }

                SelectedLibraryItemsCount = value != null ? value.Count : 0;
            }
        }

        public IEnumerable<SnipInsightLink> SelectedLibraryItems
        {
            get
            {
                if (_selectedLibraryItemsList != null)
                {
                    return _selectedLibraryItemsList.OfType<SnipInsightLink>();
                }
                else
                {
                    return Enumerable.Empty<SnipInsightLink>();
                }
            }
        }

        public int SelectedLibraryItemsCount
        {
            get { return _selectedLibraryItemsCount; }
            private set
            {
                if (value != _selectedLibraryItemsCount)
                {
                    bool hasSelectedItemsChanged = (value == 0 || _selectedLibraryItemsCount == 0);

                    _selectedLibraryItemsCount = value;

                    OnPropertyChanged("SelectedLibraryItemsCount");

                    if (hasSelectedItemsChanged)
                    {
                        OnPropertyChanged("HasSelectedLibraryItems");
                    }
                }
            }
        }

        public bool HasSelectedLibraryItems
        {
            get { return _selectedLibraryItemsCount != 0; }
        }

        /// <summary>
        /// Defines whether the library button on the navbar is enabled
        /// </summary>
        private bool _libraryEnable = true;
        public bool LibraryEnable
        {
            get { return _libraryEnable; }
            set
            {
                _libraryEnable = value;
                if (value == true)
                {
                    // Clear the selected items
                    SelectedLibraryItemsList = null;
                }

                OnPropertyChanged("LibraryEnable");
            }
        }

        #endregion

        #region Settings Panel Properties

        /// <summary>
        /// Defines whether the settings button on the navbar is enabled
        /// </summary>
        private bool _settingsEnable = true;

        /// <summary>
        /// Defines whether we open editor post snip
        /// </summary>
        private bool isOpenEditorPostSnip = UserSettings.IsOpenEditorPostSnip;

        /// <summary>
        /// Defines wheter we copy to clipboard post snip
        /// </summary>
        private bool copyClipboardPostSnip = UserSettings.CopyToClipboardAfterSnip;

        /// <summary>
        /// String for the app version
        /// </summary>
        private string appVersion;

        public bool SettingsEnable
        {
            get { return _settingsEnable; }
            set
            {
                _settingsEnable = value;
                OnPropertyChanged("SettingsEnable");
            }
        }

        /// <summary>
        /// Defines whether we open editor post snip
        /// </summary>
        public bool IsOpenEditorPostSnip
        {
            get { return isOpenEditorPostSnip; }
            set
            {
                isOpenEditorPostSnip = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Defines wheter we copy to clipboard post snip
        /// </summary>
        public bool CopyClipboardPostSnip
        {
            get { return copyClipboardPostSnip; }
            set
            {
                copyClipboardPostSnip = value;
                OnPropertyChanged();
            }
        }

        public string AppVersion
        {
            get { return appVersion; }
            set
            {
                appVersion = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region AI Panel Properties

        /// <summary>
        /// Defines whether AI panel button is enabled
        /// </summary>
        private bool _aiEnable = false;

        /// <summary>
        /// Defines if insights are visible
        /// </summary>
        private bool insightsVisible = UserSettings.IsAIEnabled;

        /// <summary>
        /// Defines if the eraser button is checked
        /// </summary>
        private bool eraserChecked = false;

        /// <summary>
        /// Defines if the highlighter button is checked
        /// </summary>
        private bool highlighterChecked = false;

        /// <summary>
        /// Defines if the pen button is checked
        /// </summary>
        private bool penChecked = false;

        /// <summary>
        /// Defines if the pen button is checked
        /// </summary>
        public bool penSelected = true;

        public bool AIEnable
        {
            get { return _aiEnable; }
            set
            {
                _aiEnable = value;
                OnPropertyChanged("AIEnable");
            }
        }

        /// <summary>
        /// Defines if insights are visible
        /// </summary>
        public bool InsightsVisible
        {
            get { return insightsVisible; }
            set
            {
                insightsVisible = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Defines if the eraser button is checked
        /// </summary>
        public bool EraserChecked
        {
            get { return eraserChecked; }
            set
            {
                eraserChecked = value;
                penSelected = !eraserChecked;
                if (eraserChecked)
                {
                    AppManager.TheBoss.OnEraser();
                }
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Defines if the highlighter button is checked
        /// </summary>
        public bool HighlighterChecked
        {
            get { return highlighterChecked; }
            set
            {
                highlighterChecked = value;
                penSelected = !highlighterChecked;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Defines if the pen button is checked
        /// </summary>
        public bool PenChecked
        {
            get { return penChecked; }
            set
            {
                penChecked = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region AI Properties
        public List<string> ImageTags { get; set; }
        public string ImageCaption { get; set; }
        public string SelectedImageUrl { get; set; }

        private string _restoreImageUrl = "";

        /// <summary>
        /// URL of image selected from AI
        /// </summary>
        public string RestoreImageUrl
        {
            get
            {
                return _restoreImageUrl;
            }
            set
            {
                // Always notify of a mode prop set
                _restoreImageUrl = value;
                OnPropertyChanged();
            }
        }

        public bool AIAlreadyRan
        {
            get;
            set;
        }
        #endregion
    }
}
