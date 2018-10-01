// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using SnipInsight.Properties;
using System;
using System.Globalization;
using System.Windows;

namespace SnipInsight.Util
{
	internal static class Diagnostics
    {

        const string AppName = "SnipInsight";
        static Diagnostics()
        {
            string langName = CultureInfo.CurrentCulture.Name;
            string applicationOS = Environment.OSVersion.VersionString;
            string envOS = Environment.OSVersion.ToString();
            string osBitness = GetBitnessString(Environment.Is64BitOperatingSystem);
            string processBitness = GetBitnessString(Environment.Is64BitProcess);
            bool itInstall = false;

            _diagnosticsLogger = new AppDiagnosticsLogger(langName, AppName, Version, applicationOS, envOS, osBitness, processBitness, itInstall);
            _usageLogger = new AppUsageLogger(langName, AppName, Version, applicationOS, envOS, osBitness, processBitness, itInstall);
        }

        internal static string Version
        {
            get { return _sVersion; }
        }

        internal static bool IsSilent { get; set; }

        /// <summary>
        /// Helper method to log trace information
        /// </summary>
        /// <param name="description">The string representation of the info to be logged</param>
        /// <param name="uploadNow">Whether to upload log immediately</param>
        internal static void LogTrace(string description)
        {
            _diagnosticsLogger.Info(description);
        }

        /// <summary>
        /// Helper method to report a severe exception
        /// </summary>
        /// <param name="ex">The exception that occurred</param>
        /// <returns></returns>
        internal static void LogException(Exception ex)
        {
            try
            {
                const int Severity = 2;
                _diagnosticsLogger.Exception(ex, Severity);
                _usageLogger.Exception(ex, Severity);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
            }
        }

        /// <summary>
        /// Helper method to report a non-severe exception
        /// </summary>
        /// <param name="ex">The exception that occurred</param>
        /// <returns></returns>
        internal static void LogLowPriException(Exception ex)
        {
            try
            {
                const int Severity = 3;
                _diagnosticsLogger.Exception(ex, Severity);
                _usageLogger.Exception(ex, Severity);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
            }
        }

        internal static void ReportException(Exception ex)
        {
            const int Severity = 1;
            try
            {
                _usageLogger.Exception(ex, Severity);
                string text = _diagnosticsLogger.Exception(ex, 1);

                if (!IsSilent)
                {
#if DEBUG
                    MessageBox.Show(AppManager.TheBoss.MainWindow, text, Resources.Exception_Dialog_Title, MessageBoxButton.OK);
#else
                    MessageBox.Show(AppManager.TheBoss.MainWindow, Resources.Exception_Dialog_Text, Resources.Exception_Dialog_Title, MessageBoxButton.OK);
#endif
                }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
            }
        }

        internal static string GetDiagnosticsLog()
        {
            return _diagnosticsLogger.GetLogFile();
        }

        private static string GetBitnessString(bool is64bit)
        {
            return is64bit ? "64-bit" : "32-bit";
        }

        static string _sVersion = "1.0";/*((AssemblyFileVersionAttribute)(Assembly.GetExecutingAssembly().GetCustomAttribute(typeof(AssemblyFileVersionAttribute)))).Version;*/
        static AppDiagnosticsLogger _diagnosticsLogger;
        static AppUsageLogger _usageLogger;
    }
}
