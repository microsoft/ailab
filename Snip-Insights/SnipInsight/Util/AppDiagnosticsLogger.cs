// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using SnipInsight.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SnipInsight.Util
{
    internal class AppDiagnosticsLogger : IDisposable
    {
        const string LogType = "diagnostics";
        internal AppDiagnosticsLogger(string langName, string appName, string appVersion, string applicationOS, string envOS, string osBitness, string processBitness, bool itInstall)
            : base()
        {
            _lock = new object();
            _lastLogged = DateTime.UtcNow;

            _logFile = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Snip.txt";
            _logHeaderWritten = false;
            _logHeader = string.Format("Language Locale: {0} Name: {1} Version: {2} Started: {3:s} OS: {4} ITInstall: {5}\r\nVersion2: OS: {6} ({7}), Process: ({8})\r\n\r\n",
                langName,
                appName,
                appVersion,
                _lastLogged,
                applicationOS,
                itInstall,
                envOS,
                osBitness,
                processBitness);

            const int UploadDelay = 301303;
            const int IdleThreshold = 4973;
            LogUploader.RetrieveContentDelegate retrieveContentDelegate = new LogUploader.RetrieveContentDelegate(this.GetUploadLogContent);
            _logUploader = new LogUploader(Settings.Default.DiagsReportUri, UserSettings.RequestId, LogType, IdleThreshold, UploadDelay,
                this.GetUploadLogContent, null, null);
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

        /// <summary>
        /// Log info into log
        /// </summary>
        /// <param name="description">The string representation of the info to be logged</param>
        /// <returns></returns>
        internal void Info(string description)
        {
            bool success = true;
            DateTime requestTime = DateTime.MinValue;

            lock (_lock)
            {
                try
                {
                    _lastLogged = requestTime = DateTime.UtcNow;
                    string logInfo = String.Format("{0:s}: {1}\r\n", requestTime, description);

                    using (StreamWriter writer = File.AppendText(_logFile))
                    {
                        if (_logHeaderWritten)
                        {
                            Debug.WriteLineIf(System.Diagnostics.Debugger.IsLogging(), logInfo);
                            writer.Write(logInfo);
                        }
                        else
                        {
                            // Include the header information the first time
                            writer.Write(_logHeader);
                            writer.Write(logInfo);
                            _logHeaderWritten = true;
                        }
                    }
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

        internal string Exception(Exception ex, int severity)
        {
            string text = MessageFromException(ex, severity);
            Info(text);
            return text;
        }

        private string GetUploadLogContent(ref DateTime contentTime)
        {
            lock (_lock)
            {
                try
                {
                    contentTime = _lastLogged;
                    // Trim the file to keep only the last lines
                    string[] allLines = File.ReadAllLines(_logFile);
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
                // ReSharper disable EmptyGeneralCatchClause
                catch
                // ReSharper restore EmptyGeneralCatchClause
                {
                    return null;
                }
            }
        }

        internal string GetLogFile()
        {
            return _logFile;
        }

        static string MessageFromException(Exception ex, int severity)
        {
            // Create human decipherable message
            StringBuilder text = new StringBuilder();

            // header
            text.AppendLine(string.Format("[exception]({0})-({1})-({2})", Diagnostics.Version, ex.HResult, severity));

            text.AppendLine(ex.Message);
            text.AppendLine(ex.StackTrace);
            if (ex.InnerException != null)
            {
                text.AppendLine("-----");
                text.AppendLine(ex.InnerException.Message);
                text.AppendLine(ex.InnerException.StackTrace);
            }

            // footer
            text.Append("[/exception]");

            return text.ToString();
        }

        private static int _sMaxLines = 5000;

        private object _lock;
        private DateTime _lastLogged;
        private LogUploader _logUploader;
        private string _logFile;
        private bool _logHeaderWritten;
        private string _logHeader;

    }
}
