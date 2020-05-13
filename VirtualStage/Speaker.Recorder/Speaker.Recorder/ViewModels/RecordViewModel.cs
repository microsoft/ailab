using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Speaker.Recorder.Models;
using Speaker.Recorder.Services;
using Speaker.Recorder.ViewModels.Base;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Speaker.Recorder.ViewModels
{
    public class RecordViewModel : BaseViewModel, IPausableViewModel, IDisposable
    {
        private bool roomDialogIsShowing;
        private bool isRecording;
        private bool presentationSavedIsShowing;
        private string selectedPpt;
        private bool isKinectReady;
        private bool isPptReady;
        private CancellationTokenSource recordingCancellationTokenSource;
        private CancellationTokenSource deviceCountCancellationTokenSource;
        private TimeSpan recordDuration;
        private string kinectRecordFile;
        private int kinectDeviceIndex = -1;
        private int kinectDeviceCount;
        private bool disposed;

        private readonly INavigationService navigationService;
        private readonly LocalPathsHelper localPathsHelper;
        private readonly SessionsService sessionsService;
        private readonly Func<KinectRecorderService> kinectRecordFactory;
        private readonly Func<PowerPointRecorderService> powerPointRecordFactory;
        private readonly IConfiguration configuration;
        private readonly ILogger<RecordViewModel> logger;
        private RecordingSession recordingSession;

        public TimeSpan MinRecordTime { get; }

        public int KinectDeviceCount
        {
            get => kinectDeviceCount;
            set => SetProperty(ref kinectDeviceCount, value);
        }

        public string KinectRecordFile
        {
            get => kinectRecordFile;
            set => SetProperty(ref kinectRecordFile, value);
        }

        public int KinectDeviceIndex
        {
            get => kinectDeviceIndex;
            set => SetProperty(ref kinectDeviceIndex, value);
        }

        public string SelectedPpt
        {
            get => selectedPpt;
            set
            {
                if (SetProperty(ref selectedPpt, value))
                {
                    this.OnPropertyChanged(nameof(IsReady));
                    this.RecordCommand.OnCanExecuteChanged();
                }
            }
        }

        public bool IsPptReady
        {
            get => isPptReady;
            set
            {
                if (SetProperty(ref isPptReady, value))
                {
                    this.OnPropertyChanged(nameof(IsReady));
                    this.RecordCommand.OnCanExecuteChanged();
                }
            }
        }

        public bool IsKinectReady
        {
            get => isKinectReady;
            set
            {
                if (SetProperty(ref isKinectReady, value))
                {
                    this.OnPropertyChanged(nameof(IsReady));
                    this.RecordCommand.OnCanExecuteChanged();
                }
            }
        }

        public bool IsReady
        {
            get => this.IsKinectReady;
        }

        public bool IsRecording
        {
            get => isRecording;
            set
            {
                if (SetProperty(ref isRecording, value))
                {
                    this.OnPropertyChanged(nameof(StayOutIsShowing));
                    this.OnPropertyChanged(nameof(YouCanStartIsShowing));
                    this.GoToPlayerCommand.OnCanExecuteChanged();
                    this.LoadPowerPointCommand.OnCanExecuteChanged();
                }
            }
        }

        public TimeSpan RecordDuration
        {
            get => recordDuration;
            set
            {
                if (SetProperty(ref recordDuration, value))
                {
                    this.OnPropertyChanged(nameof(StayOutIsShowing));
                    this.OnPropertyChanged(nameof(YouCanStartIsShowing));
                    this.StopRecordCommand.OnCanExecuteChanged();
                }
            }
        }

        public bool RoomDialogIsShowing
        {
            get => roomDialogIsShowing;
            set => SetProperty(ref roomDialogIsShowing, value);
        }

        private TimeSpan StayOutMessageDuration => this.configuration.GetValue("Kinect:Record:StayOutMessageTime", TimeSpan.FromSeconds(5));

        public bool StayOutIsShowing
        {
            get => this.IsRecording && this.RecordDuration >= TimeSpan.FromSeconds(0) && this.RecordDuration < StayOutMessageDuration;
        }

        public bool YouCanStartIsShowing
        {
            get => this.IsRecording && this.RecordDuration >= StayOutMessageDuration
                && this.RecordDuration < StayOutMessageDuration + this.configuration.GetValue("Kinect:Record:YouCanStartMessageTime", TimeSpan.FromSeconds(3));
        }

        public bool PresentationSavedIsShowing
        {
            get => presentationSavedIsShowing;
            set
            {
                if (SetProperty(ref presentationSavedIsShowing, value))
                {
                    this.StopRecordCommand.OnCanExecuteChanged();
                }
            }
        }

        public RelayCommand GoToPlayerCommand { get; }

        public RelayCommand LoadPowerPointCommand { get; }

        public ICommand CancelEmptyDialogCommand { get; }

        public RelayCommand StopRecordCommand { get; }

        public ICommand AcceptEmptyDialogCommand { get; }

        public RelayCommand RecordCommand { get; }

        public RecordViewModel(INavigationService navigationService,
            LocalPathsHelper localPathsHelper,
            SessionsService sessionsService,
            Func<KinectRecorderService> kinectRecordFactory,
            Func<PowerPointRecorderService> powerPointRecordFactory,
            IConfiguration configuration,
            ILogger<RecordViewModel> logger)
        {
            this.KinectRecordFile = configuration.GetValue<string>("Kinect:PlaybackFile", null);
            if (this.KinectRecordFile == null)
            {
                this.KinectDeviceIndex = configuration.GetValue("Kinect:DeviceIndex", 0);
            }

            this.MinRecordTime = configuration.GetValue("Kinect:Record:MinRecordTime", TimeSpan.FromSeconds(10));

            this.navigationService = navigationService;
            this.localPathsHelper = localPathsHelper;
            this.sessionsService = sessionsService;
            this.kinectRecordFactory = kinectRecordFactory;
            this.powerPointRecordFactory = powerPointRecordFactory;
            this.configuration = configuration;
            this.logger = logger;
            this.GoToPlayerCommand = new RelayCommand(() => this.navigationService?.To<PlayerViewModel>(), () => !this.IsRecording);
            this.RecordCommand = new RelayCommand(() => this.RoomDialogIsShowing = true, () => this.IsReady);
            this.StopRecordCommand = new RelayCommand(() => this.recordingSession?.Stop(), () => this.RecordDuration > this.MinRecordTime && !this.PresentationSavedIsShowing);
            this.LoadPowerPointCommand = new RelayCommand(this.LoadPowerPoint, () => !this.IsRecording);
            this.CancelEmptyDialogCommand = new RelayCommand(() => this.RoomDialogIsShowing = false);
            this.AcceptEmptyDialogCommand = new RelayCommand(this.AceptsEmptyDialog);

            this.Resume();
        }

        public void Pause()
        {
            this.deviceCountCancellationTokenSource?.Cancel();
        }

        public void Resume()
        {
            if (this.KinectRecordFile == null)
            {
                this.KinectDeviceCountUpdater();
            }
        }

        private async void KinectDeviceCountUpdater()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            Interlocked.Exchange(ref this.deviceCountCancellationTokenSource, cancellationTokenSource)?.Cancel();
            try
            {
                while (!this.disposed)
                {
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    this.KinectDeviceCount = Microsoft.Azure.Kinect.Sensor.Device.GetInstalledCount();
                    if (this.KinectDeviceCount <= this.KinectDeviceIndex)
                    {
                        this.KinectDeviceIndex = 0;
                    }

                    await Task.Delay(1300, cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogDebug("Device count loop cancelled.");
            }
        }

        private void LoadPowerPoint()
        {
            logger.LogInformation("Load PowerPoint");
            var fileDialog = new OpenFileDialog
            {
                DefaultExt = ".pptx",
                Filter = "PowerPoint file (*.pptx;*.ppt)|*.pptx;*.ppt",
                Multiselect = false,
                CheckPathExists = true,
            };

            if (fileDialog.ShowDialog() == true)
            {
                this.SelectedPpt = fileDialog.FileName;
                logger.LogInformation($"Selected PowerPoint: {this.SelectedPpt}");
            }
        }

        private async void AceptsEmptyDialog()
        {
            this.RoomDialogIsShowing = false;
            this.IsRecording = true;

            var sessionIdentifier = DateTime.Now.ToString("yyyy-dd-M-HH-mm-ss");
            var basePath = localPathsHelper.GetBasePath();
            var sessionPath = localPathsHelper.GetSessionPath(sessionIdentifier);
            var presentationPath = localPathsHelper.GetPowerPointRecordingPath(sessionIdentifier);
            this.recordingCancellationTokenSource = new CancellationTokenSource();

            KinectRecorderService[] kinectRecords = null;
            try
            {
                if (this.KinectDeviceCount > 0)
                {
                    kinectRecords = new KinectRecorderService[this.KinectDeviceCount];
                    for (int i = 0; i < this.KinectDeviceCount; i++)
                    {
                        var kinectPath = localPathsHelper.GetKinectRecordingPath(sessionIdentifier, i);
                        kinectRecords[i] = await kinectRecordFactory().WithAsync(i, kinectPath);
                    }
                }
                else
                {
                    var kinectPath = localPathsHelper.GetKinectRecordingPath(sessionIdentifier, 0);
                    kinectRecords = new KinectRecorderService[]
                    {
                        await this.kinectRecordFactory().WithAsync(this.KinectRecordFile, kinectPath)
                    };
                }

                foreach (var item in kinectRecords)
                {
                    item.FixExposure = this.configuration.GetValue("Kinect:Record:ExposureFixTime", TimeSpan.Zero);
                }

                using var powerPointRecord = await powerPointRecordFactory().WithAsync(this.SelectedPpt, presentationPath);

                this.recordingSession = sessionsService.StartRecordingSession(
                    kinectRecords,
                    powerPointRecord,
                    this.selectedPpt,
                    sessionPath,
                    this.recordingCancellationTokenSource);

                while (!this.recordingCancellationTokenSource.IsCancellationRequested)
                {
                    var task =
                        await Task.WhenAny(this.recordingSession.RecordingTasks.Concat(new[] { Task.Delay(200) }));
                    await task;
                    this.RecordDuration = powerPointRecord.Time;
                }

                this.PresentationSavedIsShowing = true;
            }
            catch (Exception e)
            {
                logger.LogError(e.Message, $"{nameof(AceptsEmptyDialog)}");
                File.AppendAllText(Path.Combine(basePath, "log.txt"), $"[{DateTime.Now}] Exception recording\n{e}\n");
            }
            finally
            {
                for (int i = 0; i < (kinectRecords?.Length ?? 0); i++)
                {
                    kinectRecords[i]?.Dispose();
                }

                this.recordingCancellationTokenSource?.Dispose();
                this.recordingCancellationTokenSource = null;

                await Task.Delay(this.configuration.GetValue("Kinect:Record:PresentationSavedMessageTime", TimeSpan.FromSeconds(3)));
                this.RecordDuration = TimeSpan.Zero;
                this.PresentationSavedIsShowing = false;
                this.IsRecording = false;
            }
        }

        public void Dispose()
        {
            this.disposed = true;
            this.recordingCancellationTokenSource?.Cancel();
        }
    }
}
