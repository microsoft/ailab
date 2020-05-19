using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Speaker.Recorder.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Speaker.Recorder.Services
{
    public class SessionsService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly LocalPathsHelper localPathsHelper;

        public SessionsService(LocalPathsHelper localPathsHelper, IServiceProvider serviceProvider)
        {
            this.localPathsHelper = localPathsHelper;
            this.serviceProvider = serviceProvider;
        }

        public IEnumerable<Session> GetSessions()
        {
            var directories = Directory.GetDirectories(localPathsHelper.GetBasePath());

            if (!directories.Any())
            {
                return Enumerable.Empty<Session>();
            }
            else
            {
                var sessionTasks = directories
                    .Select(x => (session: this.serviceProvider.GetRequiredService<Session>(), directory: x))
                    .Select(y => (session: y.session, init: y.session.Initialize(y.directory)));

                return sessionTasks.Where(x => x.init).Select(x => x.session);
            }
        }

        public RecordingSession StartRecordingSession
            (KinectRecorderService[] kinectRecorderServices, 
            PowerPointRecorderService powerPointRecorderService,
            string powerPointPresentationPath,
            string sessionPath,
            CancellationTokenSource cancellationTokenSource)
        {
            var recordingSession = new RecordingSession(
                kinectRecorderServices, 
                powerPointRecorderService,
                powerPointPresentationPath,
                sessionPath,
                cancellationTokenSource,
                serviceProvider.GetRequiredService<ILogger<RecordingSession>>());

            recordingSession.Start();

            return recordingSession;
        }
    }
}
