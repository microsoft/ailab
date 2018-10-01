// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using SnipInsight.Properties;
using System;
using System.IO;

namespace SnipInsight.Util
{
	internal class AppUsageLogger : IDisposable
    {
        const string LogType = "usage";
        internal AppUsageLogger(string langName, string appName, string appVersion, string applicationOS, string envOS, string osBitness, string processBitness, bool itInstall)
            : base()
        {
            _lock = new object();
            _lastLogged = DateTime.UtcNow;

            _logFile = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\SnipUsages.txt";
            _uploadLogFile = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\SnipUsagesUpload.txt";

            try
            {
                MergeUploadLogToLog();
            }
            catch (Exception)
            {

            }
            _langName = langName;
            _sessionId = Guid.NewGuid().ToString();
            _appName = appName;
            _appVersion = appVersion;
            _os = string.Format("{0} ({1})", envOS, osBitness);
            _process = processBitness;
            _itInstall = itInstall;

            const int UploadDelay = 29567;
            const int IdleThreshold = 5107;
            _logUploader = new LogUploader(Settings.Default.DiagsReportUri, UserSettings.RequestId, LogType, IdleThreshold, UploadDelay,
                this.GetUploadLogContent, this.UploadSuccessCallback, this.UploadFailureCallback);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_logUploader != null)
                {
                    _logUploader.Dispose();
                    _logUploader = null;
                }
            }
        }

        internal void Exception(Exception ex, int severity)
        {

            bool success = true;
            DateTime requestTime = DateTime.MinValue;

            lock (_lock)
            {
                try
                {
                    _lastLogged = requestTime = DateTime.UtcNow;
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch
                // ReSharper restore EmptyGeneralCatchClause
                {
                    success = false;
                }
            }

            if (success)
            {
                _logUploader.Queue(requestTime);
            }
        }

        private string GetUploadLogContent(ref DateTime contentTime)
        {
            lock(_lock)
            {
                try
                {
                    if (File.Exists(_logFile))
                    {
                        File.Move(_logFile, _uploadLogFile);
                        contentTime = _lastLogged;
                        // Trim the file to keep only the last lines
                        string[] allLines = File.ReadAllLines(_uploadLogFile);
                        if (allLines.Length > _sMaxLines)
                        {
                            string[] trimmedLines = new string[_sMaxLines];
                            Array.Copy(allLines, allLines.Length - _sMaxLines, trimmedLines, 0, _sMaxLines);
                            File.WriteAllLines(_logFile, trimmedLines);
                            return string.Join("\r\n", trimmedLines);
                        }
                        else
                        {
                            return string.Join("\r\n", allLines);
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Diagnostics.LogLowPriException(ex);
                    return null;
                }
            }
        }

        private void UploadFailureCallback()
        {
            // merge upload log with
            lock (_lock)
            {
                try
                {
                    Diagnostics.LogTrace("Usage log upload failed.");
                    MergeUploadLogToLog();
                }
                catch (Exception ex)
                {
                    Diagnostics.LogLowPriException(ex);
                }
            }
        }

        private void MergeUploadLogToLog()
        {
            if (!File.Exists(_logFile))
            {
                if (File.Exists(_uploadLogFile))
                {
                    File.Move(_uploadLogFile, _logFile);
                }
            }
            else
            {
                using (Stream inputStream = File.OpenRead(_logFile))
                using (Stream outputStream = new FileStream(_uploadLogFile, FileMode.Append, FileAccess.Write, FileShare.None))
                {
                    inputStream.CopyTo(outputStream);
                }
                File.Delete(_logFile);
                File.Move(_uploadLogFile, _logFile);
            }
        }

        private void UploadSuccessCallback()
        {
            // delete upload log
            lock(_lock)
            {
                try
                {
                    if (File.Exists(_uploadLogFile))
                    {
                        File.Delete(_uploadLogFile);
                    }
                }
                catch (Exception ex)
                {
                    Diagnostics.LogLowPriException(ex);
                }
            }
        }

        private static int _sMaxLines = 512;

        private string _langName;
        private string _uploadLogFile;
        private string _sessionId;
        private object _lock;
        private DateTime _lastLogged;
        private LogUploader _logUploader;
        private string _logFile;
        private string _appName;
        private string _appVersion;
        private string _os;
        private string _process;
        private bool _itInstall;
    }
}
