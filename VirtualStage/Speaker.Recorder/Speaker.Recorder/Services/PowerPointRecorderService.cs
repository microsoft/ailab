using Microsoft.Extensions.Logging;
using NAudio.Wave;
using Speaker.Recorder.Capture;
using Speaker.Recorder.Helpers;
using Speaker.Recorder.PowerPoint;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;

namespace Speaker.Recorder.Services
{
    public class PowerPointRecorderService : IDisposable
    {
        private Encoder encoder;
        private readonly PowerPointGraphicsCaptureItemsFactory graphicsCaptureItemsFactory;
        private readonly ILogger<PowerPointRecorderService> logger;
        private IRandomAccessStream randomStream;
        private IDirect3DDevice device;
        private WaveInEvent waveIn;
        private MediaEncodingProfile encodingProfile;
        private FileStream stream;

        public string PptPath { get; private set; }

        public TimeSpan Time { get => this.encoder.Time; }

        public PowerPointRecorderService(PowerPointGraphicsCaptureItemsFactory graphicsCaptureItemsFactory, ILogger<PowerPointRecorderService> logger)
        {
            this.graphicsCaptureItemsFactory = graphicsCaptureItemsFactory;
            this.logger = logger;
        }

        public async Task<PowerPointRecorderService> WithAsync(string pptPath, string recordPath)
        {
            if (this.PptPath != null)
            {
                throw new InvalidOperationException("This instance has been initialized already.");
            }

            this.PptPath = pptPath;
            await this.CreateRecord(recordPath).ConfigureAwait(false);
            return this;
        }

        private Task CreateRecord(string recordPath)
        {
            this.logger.LogInformation($"Creating recorder for {this.PptPath} in path {recordPath}");
            this.stream = File.OpenWrite(recordPath);
            this.randomStream = stream.AsRandomAccessStream();
            this.device = Direct3D11Helper.CreateDevice();

            this.waveIn = new WaveInEvent();
            this.waveIn.WaveFormat = this.waveIn.SelectBestWaveInFormat();

            this.encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);

            this.encoder = new Encoder(device, this.PptPath != null ? () => this.graphicsCaptureItemsFactory.GetOrCreateGraphicsCaptureItem(this.PptPath) : (Func<GraphicsCaptureItem>)null, waveIn);
            return this.encoder.InitializeAsync();
        }

        public async Task Start()
        {
            this.logger.LogInformation($"Starting recorder for {this.PptPath}");
            try
            {
                await this.encoder.EncodeAsync(randomStream, encodingProfile);
                this.graphicsCaptureItemsFactory.Remove(this.PptPath);
            }
            finally
            {
                this.Stop();
                this.device?.Dispose();
                this.waveIn?.Dispose();
                this.randomStream?.Dispose();
                this.stream?.Dispose();
            }
        }

        public void Stop()
        {
            this.logger.LogInformation($"Stopping record for {this.PptPath}");
            this.encoder?.Dispose();
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}
