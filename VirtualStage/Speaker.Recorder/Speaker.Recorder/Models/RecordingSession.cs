using Microsoft.Extensions.Logging;
using Speaker.Recorder.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Speaker.Recorder.Models
{
    public class RecordingSession
    {
        private readonly KinectRecorderService[] kinectRecorders;
        private readonly PowerPointRecorderService powerPointRecorder;
        private readonly string powerPointPresentationPath;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly ILogger<RecordingSession> logger;
        private readonly string sessionBasePath;

        public Task[] RecordingTasks { get; private set; }

        public DateTime Started { get; private set; }

        public TimeSpan Duration { get; private set; }

        public RecordingSession(KinectRecorderService[] kinectRecorders,
            PowerPointRecorderService powerPointRecorder,
            string powerPointPresentationPath,
            string sessionBasePath,
            CancellationTokenSource cancellationTokenSource,
            ILogger<RecordingSession> logger)
        {
            this.kinectRecorders = kinectRecorders;
            this.powerPointRecorder = powerPointRecorder;
            this.powerPointPresentationPath = powerPointPresentationPath;
            this.cancellationTokenSource = cancellationTokenSource;
            this.logger = logger;
            this.sessionBasePath = sessionBasePath;
        }

        public void Start()
        {
            logger.LogInformation($"Starting a recording ");
            var dstFile = string.Empty;
            if (this.powerPointPresentationPath != null)
            {
                // Copy slides to session folder. Rename file without changing extension
                try
                {
                    var slidesExtension = Path.GetExtension(this.powerPointPresentationPath);
                    dstFile = Path.Combine(this.sessionBasePath, $"{LocalPathsHelper.PowerPointSlidesFileNameWithoutExtension}{slidesExtension}");
                    File.Copy(this.powerPointPresentationPath, dstFile);
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Exception copying PPT slides {powerPointPresentationPath} to {dstFile}");
                    File.AppendAllText(Path.Combine(this.sessionBasePath, "log.txt"), $"[{DateTime.Now}] Exception copying PPT slides to session\n{e}\n");
                }
            }

            var _ = this.cancellationTokenSource.Token.Register(this.InternalStopRecorders);
            this.RecordingTasks = this.kinectRecorders.Select(x => x.Start()).Concat(new[] { this.powerPointRecorder?.Start() }).Where(x => x != null).ToArray();
        }

        private void InternalStopRecorders()
        {
            foreach (var item in this.kinectRecorders)
            {
                try
                {
                    item.Stop();
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Exception trying to stop a {nameof(KinectRecorderService)} {item?.ProviderKey}");
                    File.AppendAllText(Path.Combine(this.sessionBasePath, "log.txt"), $"[{DateTime.Now}] Exception trying to stop\n{e}\n");
                }
            }

            try
            {
                this.powerPointRecorder.Stop();
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Exception trying to stop a {nameof(PowerPointRecorderService)} {powerPointRecorder?.PptPath}");
                File.AppendAllText(Path.Combine(this.sessionBasePath, "log.txt"), $"[{DateTime.Now}] Exception trying to stop\n{e}\n");
            }
        }

        public void Stop()
        {
            logger.LogInformation("Stopping recording");
            this.cancellationTokenSource.Cancel();
        }
    }
}