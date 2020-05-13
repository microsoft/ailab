using NAudio.Wave;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage.Streams;

namespace Speaker.Recorder.Capture
{
    public sealed class Encoder : IDisposable
    {
        private IDirect3DDevice device;

        private Func<GraphicsCaptureItem> captureItemFactory;
        private IWaveIn waveIn;
        private BufferedWaveProvider audioProvider;
        private CaptureFrameWait frameGenerator;

        private VideoStreamDescriptor videoDescriptor;
        private AudioStreamDescriptor audioDescriptor;
        private MediaStreamSource mediaStreamSource;
        private MediaTranscoder transcoder;
        private bool isRecording;
        private bool closed = false;
        private TimeSpan audioTime;
        private Stopwatch encodingTime;

        public TimeSpan Time { get => encodingTime?.Elapsed ?? TimeSpan.Zero; }

        public Encoder(IDirect3DDevice device, Func<GraphicsCaptureItem> itemFactory, IWaveIn waveIn)
        {
            this.device = device;
            this.captureItemFactory = itemFactory;
            this.waveIn = waveIn;
            this.isRecording = false;
        }

        public Task InitializeAsync()
        {
            if (this.captureItemFactory != null)
            {
                frameGenerator = new CaptureFrameWait(this.device, this.captureItemFactory);
            }

            var audioTcs = new TaskCompletionSource<bool>();
            void FirstDataAvailable(object sender, WaveInEventArgs e)
            {
                this.waveIn.DataAvailable -= FirstDataAvailable;
                audioTcs.TrySetResult(true);
            }

            this.waveIn.DataAvailable += FirstDataAvailable;
            this.waveIn.StartRecording();
            this.audioProvider = new BufferedWaveProvider(waveIn.WaveFormat) { DiscardOnBufferOverflow = true, ReadFully = false };
            this.CreateMediaObjects(this.frameGenerator?.CurrentItem);
            return audioTcs.Task;
        }

        public IAsyncAction EncodeAsync(IRandomAccessStream stream, MediaEncodingProfile encodingProfile)
        {
            return this.EncodeInternalAsync(stream, encodingProfile).AsAsyncAction();
        }

        private async Task EncodeInternalAsync(IRandomAccessStream stream, MediaEncodingProfile encodingProfile)
        {
            if (!this.isRecording)
            {
                this.isRecording = true;
                this.waveIn.DataAvailable += OnWaveInDataAvailable;
                var transcode = await this.transcoder.PrepareMediaStreamSourceTranscodeAsync(this.mediaStreamSource, stream, encodingProfile);
                await transcode.TranscodeAsync();
            }
        }

        public void Dispose()
        {
            if (this.closed)
            {
                return;
            }
            this.closed = true;
            this.waveIn.DataAvailable -= OnWaveInDataAvailable;
            this.mediaStreamSource.Starting -= OnMediaStreamSourceStarting;
            this.mediaStreamSource.SampleRequested -= OnMediaStreamSourceSampleRequested;
            this.waveIn.StopRecording();

            if (!this.isRecording)
            {
                this.DisposeInternal();
            }

            this.isRecording = false;
        }

        private void DisposeInternal()
        {
            this.frameGenerator?.Dispose();
        }

        private void CreateMediaObjects(GraphicsCaptureItem item)
        {
            if (item != null)
            {
                int width = item.Size.Width;
                int height = item.Size.Height;
                var videoProperties = VideoEncodingProperties.CreateUncompressed(MediaEncodingSubtypes.Bgra8, (uint)width, (uint)height);
                this.videoDescriptor = new VideoStreamDescriptor(videoProperties);
            }

            var audioProperties = AudioEncodingProperties.CreatePcm((uint)this.waveIn.WaveFormat.SampleRate, (uint)this.waveIn.WaveFormat.Channels, (uint)this.waveIn.WaveFormat.BitsPerSample);
            this.audioDescriptor = new AudioStreamDescriptor(audioProperties);

            // Create our MediaStreamSource
            this.mediaStreamSource = this.videoDescriptor != null ? new MediaStreamSource(this.videoDescriptor, this.audioDescriptor) : new MediaStreamSource(this.audioDescriptor);
            this.mediaStreamSource.BufferTime = TimeSpan.FromSeconds(0);
            this.mediaStreamSource.Starting += this.OnMediaStreamSourceStarting;
            this.mediaStreamSource.SampleRequested += this.OnMediaStreamSourceSampleRequested;

            // Create our transcoder
            this.transcoder = new MediaTranscoder();
            this.transcoder.HardwareAccelerationEnabled = true;
        }

        private void OnMediaStreamSourceSampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
        {
            if (this.isRecording && !this.closed)
            {
                try
                {
                    if (args.Request.StreamDescriptor is VideoStreamDescriptor)
                    {
                        using var frame = this.frameGenerator.WaitForNewFrame();
                        if (frame == null)
                        {
                            args.Request.Sample = null;
                            this.DisposeInternal();
                            return;
                        }

                        var sample = MediaStreamSample.CreateFromDirect3D11Surface(frame.Surface, frame.SystemRelativeTime);
                        args.Request.Sample = sample;
                        frame.Dispose();
                    }
                    else
                    {
                        var bytes = this.audioProvider.BufferedBytes;
                        var audioFrameSize = this.audioProvider.BufferedDuration;
                        var audioData = ArrayPool<byte>.Shared.Rent(bytes);
                        var audioSize = this.audioProvider.Read(audioData, 0, bytes);
                        args.Request.Sample = MediaStreamSample.CreateFromBuffer(audioData.AsBuffer(0, audioSize), audioTime);
                        this.audioTime += audioFrameSize;
                        ArrayPool<byte>.Shared.Return(audioData);

                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    args.Request.Sample = null;
                    this.DisposeInternal();
                }
            }
            else
            {
                args.Request.Sample = null;
                this.DisposeInternal();
            }
        }

        private void OnMediaStreamSourceStarting(MediaStreamSource sender, MediaStreamSourceStartingEventArgs args)
        {
            if (this.frameGenerator != null)
            {
                using var frame = this.frameGenerator.WaitForNewFrame();
                this.encodingTime = Stopwatch.StartNew();
                this.audioTime = frame.SystemRelativeTime;
            }
            else
            {
                this.encodingTime = Stopwatch.StartNew();
                this.audioTime = TimeSpan.Zero;
            }

            args.Request.SetActualStartPosition(this.audioTime);
        }

        private void OnWaveInDataAvailable(object sender, WaveInEventArgs e)
        {
            this.audioProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
        }
    }
}
