using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.Sensor.Record;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Speaker.Recorder.Kinect
{
    public class KinectCaptureProvider : IDisposable
    {
        public event EventHandler IsConnectedChanged;

        private ManualResetEventSlim captureEvent = new ManualResetEventSlim(false);
        private event EventHandler<Microsoft.Azure.Kinect.Sensor.Capture> captureArrived;

        public event EventHandler<Microsoft.Azure.Kinect.Sensor.Capture> CaptureArrived
        {
            add
            {
                this.captureArrived += value;
                if (this.captureArrived?.GetInvocationList().Length == 1)
                {
                    this.device?.StartCameras(this.GetDeviceConfiguration());
                    this.captureEvent.Set();
                }
            }
            remove
            {
                this.captureArrived -= value;
                if (!this.disposed && (this.captureArrived == null || this.captureArrived.GetInvocationList().Length == 0))
                {
                    this.captureEvent.Reset();
                    this.device?.StopCameras();
                }
            }
        }

        private readonly string playbackPath;
        private readonly Playback playback;
        private readonly TimeSpan fpsTime;
        private readonly int deviceIndex = -1;
        private Device device;
        private bool disposed;
        private int seekCount;
        private bool? isConnected;
        private readonly object deviceLock = new object();
        private IConfiguration configuration;
        private SemaphoreSlim deviceCreationEvent;

        public bool? IsConnected
        {
            get => this.isConnected;
            private set
            {
                if (this.isConnected != value)
                {
                    this.isConnected = value;
                    this.IsConnectedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public KinectCaptureProvider(int deviceIndex, IConfiguration configuration)
        {
            this.configuration = configuration;
            this.deviceCreationEvent = new SemaphoreSlim(0);
            this.deviceIndex = deviceIndex;
            Task.Run(this.OpenDevice);
        }

        private async void OpenDevice()
        {
            while (this.device == null && !this.disposed)
            {
                try
                {
                    lock (this.deviceLock)
                    {
                        if (!this.disposed)
                        {
                            var deviceCount = Device.GetInstalledCount();
                            if (deviceIndex < deviceCount)
                            {
                                this.device = Device.Open(deviceIndex);
                                if (this.captureArrived != null)
                                {
                                    this.device.StartCameras(this.GetDeviceConfiguration());
                                }

                                this.SetExposure(null);
                                this.IsConnected = true;
                                var renderThread = new Thread(this.RenderThread);
                                renderThread.Start();
                                this.deviceCreationEvent.Release();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }

                if (this.device == null)
                {
                    this.IsConnected = false;
                    await Task.Delay(1500);
                }
            }
        }

        public KinectCaptureProvider(string playbackPath)
        {
            try
            {
                this.playbackPath = playbackPath;
                this.playback = Playback.Open(playbackPath);
                this.fpsTime = this.playback.RecordConfiguration.CameraFPS switch
                {
                    FPS.FPS5 => TimeSpan.FromMilliseconds(1000 / 5),
                    FPS.FPS15 => TimeSpan.FromMilliseconds(1000 / 15),
                    FPS.FPS30 => TimeSpan.FromMilliseconds(1000 / 30),
                    _ => TimeSpan.FromMilliseconds(1000),
                };
                this.IsConnected = true;
                var renderThread = new Thread(this.RenderThread);
                renderThread.Start();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                this.IsConnected = false;
            }
        }

        internal void SetExposure(TimeSpan? exposureValue)
        {
            if (this.device != null && !this.disposed)
            {
                var exposureMicroSeconds = (exposureValue?.TotalMilliseconds ?? 0) * 1000;
                this.device.SetColorControl(ColorControlCommand.ExposureTimeAbsolute, exposureValue == null ? ColorControlMode.Auto : ColorControlMode.Manual, (int)exposureMicroSeconds);
            }
        }

        private DeviceConfiguration GetDeviceConfiguration()
        {
            if (this.deviceIndex >= 0)
            {
                var deviceConfiguration = new DeviceConfiguration
                {
                    CameraFPS = FPS.FPS30,
                    ColorFormat = ImageFormat.ColorMJPG,
                    ColorResolution = ColorResolution.R1080p,
                    DepthMode = DepthMode.NFOV_Unbinned,
                    SynchronizedImagesOnly = true,
                };
                configuration.GetSection("Kinect:Device").Bind(deviceConfiguration);
                return deviceConfiguration;
            }
            else
            {
                return new DeviceConfiguration
                {
                    CameraFPS = this.playback.RecordConfiguration.CameraFPS,
                    ColorFormat = this.playback.RecordConfiguration.ColorFormat,
                    ColorResolution = this.playback.RecordConfiguration.ColorResolution,
                    DepthMode = this.playback.RecordConfiguration.DepthMode,
                    DepthDelayOffColor = this.playback.RecordConfiguration.DepthDelayOffColor,
                    WiredSyncMode = this.playback.RecordConfiguration.WiredSyncMode,
                };
            }
        }

        private void RenderThread(object obj)
        {
            var processTime = new Stopwatch();
            while ((this.device != null || this.playback != null) && !this.disposed)
            {
                this.captureEvent.Wait();
                if (this.disposed)
                {
                    continue;
                }

                processTime.Restart();

                try
                {
                    using var capture = this.device != null ? this.device.GetCapture() : this.playback.GetNextCapture();
                    if (!this.disposed)
                    {
                        if (this.playback != null && capture == null)
                        {
                            this.playback.Seek(TimeSpan.Zero);
                            this.seekCount++;
                            continue;
                        }

                        if (this.playback != null)
                        {
                            if (capture.Color != null)
                            {
                                capture.Color.DeviceTimestamp += this.playback.RecordingLength * this.seekCount;
                            }

                            if (capture.Depth != null)
                            {
                                capture.Depth.DeviceTimestamp += this.playback.RecordingLength * this.seekCount;
                            }

                            if (capture.IR != null)
                            {
                                capture.IR.DeviceTimestamp += this.playback.RecordingLength * this.seekCount;
                            }
                        }

                        this.captureArrived?.Invoke(this, capture);
                        processTime.Stop();
                        if (!this.disposed)
                        {
                            if (this.playback != null && processTime.Elapsed < this.fpsTime)
                            {
                                Thread.Sleep(this.fpsTime - processTime.Elapsed);
                            }
                        }
                    }
                }
                catch (AzureKinectException)
                {
                    if (this.device != null && !this.disposed)
                    {
                        try
                        {
                            this.device?.Dispose();
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e);
                        }
                        this.device = null;
                        this.OpenDevice();
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }

            try
            {
                this.device?.Dispose();
                this.playback?.Dispose();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public void Dispose()
        {
            this.disposed = true;
            this.captureEvent.Set();
        }

        public KinectImageProvider For(KinectCaptureImage image)
        {
            return new KinectImageProvider(this, image);
        }

        public async Task<Microsoft.Azure.Kinect.Sensor.Record.Recorder> GetRecorder(string filePath)
        {
            if (this.deviceIndex >= 0)
            {
                await this.deviceCreationEvent.WaitAsync().ConfigureAwait(false);
                this.deviceCreationEvent.Release();
                return Microsoft.Azure.Kinect.Sensor.Record.Recorder.Create(filePath, this.device, this.GetDeviceConfiguration());
            }
            else
            {
                return Microsoft.Azure.Kinect.Sensor.Record.Recorder.Create(filePath, null, this.GetDeviceConfiguration());
            }
        }
    }
}
