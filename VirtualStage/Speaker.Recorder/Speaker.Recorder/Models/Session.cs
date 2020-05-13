using Microsoft.Azure.Storage.DataMovement;
using Microsoft.Extensions.Logging;
using Speaker.Recorder.ViewModels.Base;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Speaker.Recorder.Services
{
    public class Session : NotifyPropertyObject, IDisposable
    {

        private readonly UploadManager uploadManager;
        private readonly LocalPathsHelper localPathsHelper;
        private readonly ILogger<Session> logger;
        private Stopwatch speedStopwatch;
        private double lastSpeedTransferedSize;
        private double transferedSize;
        private StorageFolder storageFolder;
        private bool isInitilized;
        private CancellationTokenSource uploadCancellation;

        public string Id { get; private set; }

        public string LocalRecordingFolderPath { get; private set; }

        public string[] KinectVideoPaths { get; private set; }

        public string PresentationVideoPath => localPathsHelper.GetPowerPointRecordingPath(Id);

        public string PresentationSlidesPath => localPathsHelper.GetPowerPointSlidesPath(Id);

        public long TotalBytes { get; private set; }

        public DateTime Created { get; private set; }

        private DateTime uploaded;

        public DateTime Uploaded
        {
            get => uploaded;
            private set => SetProperty(ref uploaded, value);
        }

        private TimeSpan duration;
        public TimeSpan Duration
        {
            get => duration;
            private set => SetProperty(ref duration, value);
        }

        private bool isBusy;

        public bool IsBusy
        {
            get => isBusy;
            private set => SetProperty(ref isBusy, value);
        }

        private bool isUploaded;
        public bool IsUploaded
        {
            get => isUploaded;
            private set
            {
                SetProperty(ref isUploaded, value);
            }
        }

        private bool isUploading;
        public bool IsUploading
        {
            get => isUploading;
            private set
            {
                SetProperty(ref isUploading, value);
            }
        }

        private bool isUploadPaused;
        public bool IsUploadPaused
        {
            get => isUploadPaused;
            private set
            {
                SetProperty(ref isUploadPaused, value);
            }
        }

        private bool canBeUploaded;
        public bool CanBeUploaded
        {
            get => canBeUploaded;
            private set
            {
                SetProperty(ref canBeUploaded, value);
            }
        }

        private bool isFailed;
        public bool IsFailed
        {
            get => isFailed;
            private set
            {
                SetProperty(ref isFailed, value);
            }
        }

        private double uploadPercentage;

        public double UploadPercentage
        {
            get => uploadPercentage;
            private set
            {
                SetProperty(ref uploadPercentage, value);
            }
        }
        private double uploadSpeed;

        public double UploadSpeedMbps
        {
            get => uploadSpeed;
            private set
            {
                SetProperty(ref uploadSpeed, value);
            }
        }

        public Session(UploadManager uploadManager, LocalPathsHelper localPathsHelper, ILogger<Session> logger)
        {
            this.uploadManager = uploadManager;
            this.localPathsHelper = localPathsHelper;
            this.logger = logger;
        }

        /// <summary>
        /// Initializes synchronously some basic information about the session
        /// Starts in a fire-and-forget fashion the loading of more data
        /// </summary>
        /// <param name="localRecordingFolderPath"></param>
        /// <returns></returns>
        public bool Initialize(string localRecordingFolderPath)
        {
            this.IsBusy = true;

            this.CanBeUploaded = this.uploadManager.IsUploadAvailable;
            this.LocalRecordingFolderPath = localRecordingFolderPath;
            this.Id = new DirectoryInfo(localRecordingFolderPath).Name;

            this.KinectVideoPaths = Directory.GetFiles(this.LocalRecordingFolderPath, LocalPathsHelper.KinectRecordFilePattern);

            if (this.KinectVideoPaths.Length == 0)
            {
                return false;
            }

            // Check if all files exists
            foreach (var kinectVideoPath in this.KinectVideoPaths)
            {
                if (!File.Exists(kinectVideoPath))
                {
                    IsBusy = false;
                    return false;
                }
            }

            // Load basic metadata
            this.Created = File.GetCreationTime(this.KinectVideoPaths.FirstOrDefault());

            // Get total bytes size     
            var dirListOptions = new EnumerationOptions()
            {
                RecurseSubdirectories = false,
            };
            var filesInDir = Directory.GetFiles(this.LocalRecordingFolderPath, UploadManager.FileUploadSearchPattern, dirListOptions);
            this.TotalBytes = filesInDir.Select(x => new FileInfo(x).Length).Sum();

            // Further initialization
            _ = this.InitializeMetadataAsync();

            return true;
        }

        public async Task InitializeMetadataAsync()
        {
            var uploadCheckTask = this.CheckUploadStatusAsync();

            try
            {
                this.storageFolder = await StorageFolder.GetFolderFromPathAsync(this.LocalRecordingFolderPath);
                var videoLengthsTasks = this.KinectVideoPaths.Concat(new[] { this.PresentationVideoPath })
                    .Where(x => x != null)
                    .Select(x => this.GetVideoDuration(x))
                    .ToArray();
                await Task.WhenAll(videoLengthsTasks);

                this.Duration = videoLengthsTasks.Select(x => x.Result).Where(x => x > TimeSpan.Zero).Min();

                // Await upload check task to catch exceptions.
                await uploadCheckTask;

                if (!this.IsUploaded && this.CanBeUploaded && uploadManager.ExistsUploadFile(this))
                {
                    this.IsUploading = true;
                    this.IsUploadPaused = true;
                }

                this.isInitilized = true;
            }
            catch (Exception e)
            {
                logger.LogWarning(e, $"Error initiating asynchronous Session {this.Id} in {nameof(InitializeMetadataAsync)}");
                this.IsFailed = true;
                File.AppendAllText(Path.Combine(this.LocalRecordingFolderPath, "log.txt"), $"[{DateTime.Now}] Can't load \n{e}\n");
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        private async Task<TimeSpan> GetVideoDuration(string videoFilePath)
        {
            var videoItem = await this.storageFolder.TryGetItemAsync(Path.GetFileName(videoFilePath));
            if (videoItem != null && videoItem.IsOfType(StorageItemTypes.File) && videoItem is StorageFile videoFile)
            {
                VideoProperties videoProperties;
                int retries = 0;
                do
                {
                    videoProperties = await videoFile.Properties.GetVideoPropertiesAsync();
                    if (videoProperties.Duration > TimeSpan.Zero)
                    {
                        return TimeSpan.FromSeconds(Math.Floor(videoProperties.Duration.TotalSeconds));
                    }

                    await Task.Delay(TimeSpan.FromSeconds(0.3));
                    retries++;
                } while (retries < 10);
            }

            logger.LogWarning($"Can't get the duration of the video {videoFilePath} of Session {this.Id}");
            return TimeSpan.Zero;
        }

        public async Task CheckUploadStatusAsync()
        {
            var uploaded = await uploadManager.GetUploadedDateTime(this);
            this.IsUploaded = uploaded.HasValue;
            this.Uploaded = uploaded.GetValueOrDefault();
        }

        public async Task UploadAsync()
        {
            logger.LogInformation($"Start a session {this.Id} upload");

            try
            {
                if (!this.isInitilized)
                {
                    this.IsBusy = true;
                    this.IsFailed = false;
                    await this.CheckUploadStatusAsync();
                    this.IsBusy = false;
                    return;
                }

                if (this.IsFailed)
                {
                    this.IsBusy = true;
                    this.IsFailed = false;
                    await this.CheckUploadStatusAsync();
                    this.IsBusy = false;
                }

                if (this.CanUpload())
                {
                    this.speedStopwatch = new Stopwatch();
                    this.transferedSize = 0;
                    this.lastSpeedTransferedSize = 0;
                    this.UploadSpeedMbps = 0;
                    var uploadCancellation = new CancellationTokenSource();
                    this.uploadCancellation = uploadCancellation;
                    var progress = new ProgressRecorder(this.TotalBytes);
                    progress.UpdateUploadProgress += (e, progress) =>
                    {
                        this.transferedSize = progress;
                        var uploadProgress = progress / this.TotalBytes;
                        this.IsBusy = uploadProgress <= 0;
                        if (!this.speedStopwatch.IsRunning)
                        {
                            this.lastSpeedTransferedSize = this.transferedSize;
                            this.speedStopwatch.Restart();
                        }

                        this.UploadPercentage = uploadProgress;
                    };

                    this.IsBusy = true;
                    if (!this.isUploadPaused)
                    {
                        this.UploadPercentage = 0;
                    }

                    this.TransferSpeedLoop(uploadCancellation.Token);

                    this.IsUploadPaused = false;
                    this.IsUploading = true;
                    try
                    {
                        if (await uploadManager.UploadSessionAsync(this, progress, this.uploadCancellation.Token))
                        {
                            this.IsUploaded = true;
                            this.IsUploading = false;
                            this.Uploaded = DateTime.Now;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, $"Error trying to upload a file of session {this.Id}");
                        this.IsFailed = true;
                    }
                    finally
                    {
                        uploadCancellation.Cancel();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, $"Error trying to upload a file in {nameof(UploadAsync)} of session {this.Id}");
                this.IsBusy = false;
                this.IsFailed = true;
            }
        }

        private async void TransferSpeedLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(800);
                var elapsed = this.speedStopwatch.Elapsed.TotalSeconds;
                if (elapsed > 0)
                {
                    var size = this.transferedSize - this.lastSpeedTransferedSize;
                    var speed = ((size * 8) / (1024 * 1024)) / elapsed;
                    this.UploadSpeedMbps = speed;
                }
            }
        }

        public async Task PauseUploadAsync()
        {
            logger.LogTrace($"Trying to pause a session {this.Id}");
            if (this.CanPauseUpload())
            {
                logger.LogInformation($"Pausing a session {this.Id} upload");
                var cancellation = this.uploadCancellation;
                this.uploadCancellation = null;
                if (cancellation != null)
                {
                    this.IsBusy = true;
                    await Task.Run(() => { cancellation?.Cancel(); });
                    this.uploadCancellation = null;
                    this.IsUploadPaused = true;
                    this.IsBusy = false;
                }
            }
        }

        public async Task StopUploadAsync()
        {
            logger.LogTrace($"Trying to stop a session {this.Id}");
            if (this.CanStopUpload())
            {
                logger.LogInformation($"Stopping a session {this.Id} upload");
                var cancellation = this.uploadCancellation;
                this.uploadCancellation = null;
                if (cancellation != null)
                {
                    this.IsBusy = true;
                    await Task.Run(() => { cancellation?.Cancel(); });
                    this.uploadManager.RemoveUploadStatus(this);
                    this.IsUploaded = false;
                    this.IsUploading = false;
                    this.IsUploadPaused = false;
                    this.IsBusy = false;
                }
            }
        }

        public bool CanUpload()
        {
            return !this.IsBusy && !this.IsUploaded;
        }

        public bool CanPauseUpload()
        {
            return !this.IsBusy && this.IsUploading && !this.IsUploadPaused;
        }

        public bool CanStopUpload()
        {
            return !this.IsBusy && (this.IsUploading || this.IsUploadPaused);
        }

        public void Dispose()
        {
            _ = Task.Run(() => { this.uploadCancellation?.Cancel(); });
        }
    }
}
