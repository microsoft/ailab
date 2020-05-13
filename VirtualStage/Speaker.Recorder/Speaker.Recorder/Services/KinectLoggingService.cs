using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Speaker.Recorder.Services
{
    public class KinectLoggingService : IHostedService
    {
        private readonly ILogger logger;

        public KinectLoggingService(ILogger<KinectLoggingService> logger)
        {
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Microsoft.Azure.Kinect.Sensor.Logger.LogMessage += Logger_LogMessage;
            Microsoft.Azure.Kinect.Sensor.Record.RecordLogger.LogMessage += RecordLogger_LogMessage;
            return Task.CompletedTask;
        }

        private void Logger_LogMessage(Microsoft.Azure.Kinect.Sensor.LogMessage obj)
        {
            this.logger.Log(this.GetLevel(obj.LogLevel), obj.Message);
        }

        private void RecordLogger_LogMessage(Microsoft.Azure.Kinect.Sensor.LogMessage obj)
        {
            this.logger.Log(this.GetLevel(obj.LogLevel), obj.Message);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Microsoft.Azure.Kinect.Sensor.Logger.LogMessage -= Logger_LogMessage;
            Microsoft.Azure.Kinect.Sensor.Record.RecordLogger.LogMessage -= RecordLogger_LogMessage;
            return Task.CompletedTask;
        }

        private Microsoft.Extensions.Logging.LogLevel GetLevel(Microsoft.Azure.Kinect.Sensor.LogLevel logLevel)
        {
            return logLevel switch
            {
                Microsoft.Azure.Kinect.Sensor.LogLevel.Critical => LogLevel.Critical,
                Microsoft.Azure.Kinect.Sensor.LogLevel.Error => LogLevel.Error,
                Microsoft.Azure.Kinect.Sensor.LogLevel.Warning => LogLevel.Warning,
                Microsoft.Azure.Kinect.Sensor.LogLevel.Information => LogLevel.Information,
                Microsoft.Azure.Kinect.Sensor.LogLevel.Trace => LogLevel.Trace,
                _ => LogLevel.None,
            };
        }
    }
}
