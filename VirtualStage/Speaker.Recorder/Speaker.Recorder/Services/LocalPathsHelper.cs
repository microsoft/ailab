using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Speaker.Recorder.Services
{
    public class LocalPathsHelper
    {
        public const string PowerPointSlidesFileNameWithoutExtension = "slides";

        public static string KinectRecordFilePattern => "kinect*.mkv";

        private string KinectRecordFileName => "kinect{0}.mkv";

        private string PowerPointRecordFileName => "presentation.mp4";

        private string RootFolder { get; }

        public LocalPathsHelper(IConfiguration configuration)
        {
            this.RootFolder = configuration.GetValue("Data:RecordingFolder", Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Recorded Sessions"));
            Directory.CreateDirectory(RootFolder);
        }

        public string GetBasePath()
        {
            return this.RootFolder;
        }

        public string GetSessionPath(string sessionIdentifier)
        {
            var sessionFolder = Path.Combine(this.RootFolder, sessionIdentifier);
            Directory.CreateDirectory(sessionFolder);

            return sessionFolder;
        }

        public string GetKinectRecordingPath(string sessionIdentifier, int deviceIndex)
        {
            return Path.Combine(GetSessionPath(sessionIdentifier), string.Format(KinectRecordFileName, deviceIndex));
        }

        public string GetPowerPointRecordingPath(string sessionIdentifier)
        {
            return Path.Combine(GetSessionPath(sessionIdentifier), PowerPointRecordFileName);
        }

        public string GetPowerPointSlidesPath(string sessionIdentifier)
        {
            var sessionPath = GetSessionPath(sessionIdentifier);
            return Directory.GetFiles(sessionPath, "*.ppt*").FirstOrDefault();
        }
    }
}
