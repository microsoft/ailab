using Speaker.Recorder.Models;
using System.Collections.Generic;

namespace Speaker.Recorder.Services
{
    public interface ISessionsService
    {
        public RecordingSession StartRecordingSession(KinectRecorderService kinectRecorder, PowerPointRecorderService powerPointRecorder);

        public IEnumerable<Session> GetSessions();
    }
}