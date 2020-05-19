using Microsoft.Extensions.Logging;
using Speaker.Recorder.Kinect;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Speaker.Recorder.Services
{
    public class KinectRecorderService : IDisposable
    {
        private TaskCompletionSource<bool> recordTcs;
        private readonly KinectCaptureProvidersFactory kinectCaptureProvidersFactory;
        private readonly ILogger<KinectRecorderService> logger;
        private KinectCaptureProvider provider;
        private Microsoft.Azure.Kinect.Sensor.Record.Recorder recording;
        private TimeSpan? firstFrameTime;
        private bool exposureFixed;
        private readonly object recordingLock = new object();

        public object ProviderKey { get; private set; }

        public TimeSpan FixExposure { get; set; } = TimeSpan.FromSeconds(2);

        public KinectRecorderService(KinectCaptureProvidersFactory kinectCaptureProvidersFactory, ILogger<KinectRecorderService> logger)
        {
            this.kinectCaptureProvidersFactory = kinectCaptureProvidersFactory;
            this.logger = logger;
        }

        public async Task<KinectRecorderService> WithAsync(object key, string filePath)
        {
            if (this.ProviderKey != null)
            {
                throw new InvalidOperationException("This instance has been initialized already.");
            }

            this.ProviderKey = key;
            await this.CreateRecording(filePath).ConfigureAwait(false);
            return this;
        }

        private async Task CreateRecording(string filePath)
        {
            this.logger.LogInformation($"Creating recorder for {this.ProviderKey} in path {filePath}");
            this.provider = this.kinectCaptureProvidersFactory.GetOrCreateProvider(this.ProviderKey);
            this.recording = await this.provider.GetRecorder(filePath).ConfigureAwait(false);
            this.provider.CaptureArrived += this.ProviderHoldCameraStartedHandler;
            this.recording.WriteHeader();
        }

        private void ProviderHoldCameraStartedHandler(object sender, Microsoft.Azure.Kinect.Sensor.Capture e)
        {
            // This handler holds the cameras started
        }

        public Task Start()
        {
            if (this.recordTcs != null)
            {
                throw new Exception("The recording has been started yet.");
            }

            this.logger.LogInformation($"Starting recorder for {this.ProviderKey}");
            this.recordTcs = new TaskCompletionSource<bool>();
            this.provider.CaptureArrived += Provider_CaptureArrived;
            this.provider.IsConnectedChanged += Provider_IsConnectedChanged;
            return recordTcs.Task;
        }

        private void Provider_IsConnectedChanged(object sender, EventArgs e)
        {
            this.logger.LogInformation($"Provider for {this.ProviderKey} IsConnected changed to {this.provider.IsConnected}");
            if (this.provider.IsConnected != true)
            {
                this.recordTcs.TrySetException(new Exception("The Azure Kinect device has been disconnected."));
                this.Stop();
            }
        }

        private void Provider_CaptureArrived(object sender, Microsoft.Azure.Kinect.Sensor.Capture e)
        {
            try
            {
                if (e.Color != null)
                {
                    if (this.firstFrameTime == null)
                    {
                        this.firstFrameTime = e.Color.DeviceTimestamp;
                    }

                    if (this.FixExposure > TimeSpan.Zero && !this.exposureFixed && (e.Color.DeviceTimestamp - this.firstFrameTime.Value) > this.FixExposure)
                    {
                        var exposureValue = e.Color.Exposure;
                        this.provider.SetExposure(exposureValue);
                        this.exposureFixed = true;
                    }
                }

                lock (this.recordingLock)
                {
                    this.recording?.WriteCapture(e);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(ex, $"Error handling capture arrived in recording for {this.ProviderKey}");
            }
        }

        public void Stop()
        {
            this.logger.LogInformation($"Stopping record for {this.ProviderKey}");
            this.provider.CaptureArrived -= Provider_CaptureArrived;
            this.provider.CaptureArrived -= ProviderHoldCameraStartedHandler;
            this.provider.SetExposure(null);
            this.kinectCaptureProvidersFactory.Remove(ProviderKey);

            lock (this.recordingLock)
            {
                var recording = Interlocked.Exchange(ref this.recording, null);
                recording?.Flush();
                recording?.Dispose();
            }

            this.recordTcs?.TrySetResult(true);
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}
