using Microsoft.Azure.Storage.DataMovement;
using System;

namespace Speaker.Recorder.Services
{
    /// <summary>
    /// A helper class to record progress reported by data movement library.
    /// </summary>
    public class ProgressRecorder : IProgress<TransferStatus>
    {
        private long latestBytesTransferred;
        private long latestNumberOfFilesTransferred;
        private long latestNumberOfFilesSkipped;
        private long latestNumberOfFilesFailed;
        private long bytesTotal;

        public event EventHandler<double> UpdateUploadProgress;

        public ProgressRecorder(long bytesTotal)
        {
            this.bytesTotal = bytesTotal;
        }

        public void Report(TransferStatus progress)
        {
            var transferSpeedBytes = progress.BytesTransferred - this.latestBytesTransferred;
            this.latestBytesTransferred = progress.BytesTransferred;
            this.latestNumberOfFilesTransferred = progress.NumberOfFilesTransferred;
            this.latestNumberOfFilesSkipped = progress.NumberOfFilesSkipped;
            this.latestNumberOfFilesFailed = progress.NumberOfFilesFailed;

            UpdateUploadProgress?.Invoke(this, this.latestBytesTransferred);
        }

        public override string ToString()
        {
            return string.Format("Transferred bytes: {0}; Transfered: {1}; Skipped: {2}, Failed: {3}",
                this.latestBytesTransferred,
                this.latestNumberOfFilesTransferred,
                this.latestNumberOfFilesSkipped,
                this.latestNumberOfFilesFailed);
        }
    }
}
