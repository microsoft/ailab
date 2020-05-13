using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.DataMovement;
using System.Threading;
using System.IO;
using Speaker.Recorder.Services.IdentificationService;
using System.Net;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Polly;
using Microsoft.Extensions.Logging;

namespace Speaker.Recorder.Services
{
    public class UploadManager : IDisposable
    {
        public const string FileUploadSearchPattern = "*";

        private int uploadRetries;
        private Task<CloudBlobContainer> cloudBlobContainerTask;
        private readonly CancellationTokenSource disposeCancellation;
        private readonly string StorageConnectionString;
        private readonly ILogger<UploadManager> logger;
        private readonly string GlobalIdentifier;

        public bool IsUploadAvailable { get; private set; }

        public UploadManager(IIdentificationService identificationService, IConfiguration configuration, ILogger<UploadManager> logger)
        {
            this.disposeCancellation = new CancellationTokenSource();
            this.StorageConnectionString = configuration.GetValue<string>("Data:AzureBlobConnection");
            this.GlobalIdentifier = identificationService.GetSanitizedIdentifier();
            this.logger = logger;
            var connections = configuration.GetValue("Data:DefaultConnectionLimit", Environment.ProcessorCount - 2);
            ServicePointManager.DefaultConnectionLimit = Math.Max(connections, 1);

            this.uploadRetries = configuration.GetValue("Data:UploadRetries", 10);

            this.cloudBlobContainerTask = this.InitializeContainerAsync();
        }

        private async Task<CloudBlobContainer> InitializeContainerAsync()
        {
            if (string.IsNullOrEmpty(this.StorageConnectionString) || !CloudStorageAccount.TryParse(this.StorageConnectionString, out var account))
            {
                this.IsUploadAvailable = false;
                this.logger.LogError($"The storage account connectionstring is not valid.");
                throw new InvalidOperationException("The storage account connectionstring is not valid.");
            }

            this.IsUploadAvailable = true;
            this.logger.LogInformation($"Initializing Cloud storage for {GlobalIdentifier} in {account.BlobEndpoint}");
            CloudBlobClient blobClient = account.CreateCloudBlobClient();
            blobClient.DefaultRequestOptions.MaximumExecutionTime = TimeSpan.FromSeconds(10);
            blobClient.DefaultRequestOptions.ServerTimeout = TimeSpan.FromSeconds(10);
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(GlobalIdentifier);
            if (await blobContainer.CreateIfNotExistsAsync().ConfigureAwait(false))
            {
                this.logger.LogInformation($"The Cloud storage for {GlobalIdentifier} did not exists and was created");
            }
            return blobContainer;
        }

        public bool ExistsUploadFile(Session session)
        {
            var journalFile = new FileInfo(this.GetJournalFileForSession(session.LocalRecordingFolderPath));
            return journalFile.Exists && journalFile.Length > 0;
        }

        public async Task<DateTime?> GetUploadedDateTime(Session session)
        {
            if (this.IsUploadAvailable)
            {
                try
                {
                    var localSessionDirectory = session.LocalRecordingFolderPath;
                    var localFiles = Directory.GetFiles(localSessionDirectory);

                    if (localFiles.Any())
                    {
                        var blobDirectoryReference = await GetDirectoryBlobAsync(session.Id).ConfigureAwait(false);
                        var blobListing = blobDirectoryReference.ListBlobs(blobListingDetails: BlobListingDetails.Metadata).OfType<CloudBlockBlob>().ToList();

                        if (blobListing.Any())
                        {
                            bool allFilesAreInRemoteFolder = localFiles.All(x => blobListing.Any(y => Path.GetFileName(y.Name) == Path.GetFileName(x)));
                            if (allFilesAreInRemoteFolder)
                            {
                                return blobListing.Min(x => x.Properties.LastModified ?? DateTimeOffset.MinValue).ToLocalTime().DateTime;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    this.logger.LogWarning(e, $"The uploaded info for session {session.Id} can not be obtained.");
                }
            }

            return null;
        }

        public async Task<bool> UploadSessionAsync(Session session, IProgress<TransferStatus> progressHandler, CancellationToken cancellationToken)
        {
            Task<bool> UploadSessionTry(Context context, CancellationToken cancellationToken)
            {
                this.logger.LogInformation($"Uploading session {session.Id}, count {context.Count}");
                return this.InternalUploadSessionAsync(session, progressHandler, cancellationToken);
            }

            var retry = Policy.Handle<Exception>().RetryAsync(this.uploadRetries);
            return await retry.ExecuteAsync(UploadSessionTry, new Context(), cancellationToken).ConfigureAwait(false);
        }

        private async Task<bool> InternalUploadSessionAsync(Session session, IProgress<TransferStatus> progressHandler, CancellationToken cancellationToken)
        {
            var srcDirectoryPath = session.LocalRecordingFolderPath;
            var dstBlob = await GetDirectoryBlobAsync(session.Id).ConfigureAwait(false);
            var journalFile = this.GetJournalFileForSession(srcDirectoryPath);

            using var journalStream = await this.GetJournalStream(session, journalFile).ConfigureAwait(false);
            using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.disposeCancellation.Token);
            using var reg = linkedCancellation.Token.Register(() =>
            {
                this.logger.LogDebug($"The transfer has been cancelled and the journal file for session {session.Id} will be saved");
                journalStream.Position = 0;
                using var journal = File.Create(journalFile);
                journalStream.CopyTo(journal);
            });

            try
            {
                var options = new UploadDirectoryOptions()
                {
                    SearchPattern = FileUploadSearchPattern,
                    Recursive = false,
                    BlobType = BlobType.BlockBlob
                };
                var context = new DirectoryTransferContext(journalStream)
                {
                    ProgressHandler = progressHandler,
                    LogLevel = Microsoft.Azure.Storage.LogLevel.Informational
                };

                var transferTcs = new TaskCompletionSource<bool>();
                context.FileFailed += (sender, e) =>
                {
                    this.logger.LogInformation(e.Exception, $"Transfer fails. {e.Source} -> {e.Destination}.");
                    transferTcs.TrySetException(e.Exception);
                    if (!linkedCancellation.IsCancellationRequested)
                    {
                        linkedCancellation.Cancel();
                    }
                };
                context.FileSkipped += (sender, e) =>
                {
                    this.logger.LogInformation($"Transfer skips. {e.Source} -> {e.Destination}.");
                };
                context.FileTransferred += (sender, e) =>
                {
                    this.logger.LogInformation(e.Exception, $"Transfer succeds. {e.Source} -> {e.Destination}.");
                };

                // Start the upload
                var trasferStatus = await TransferManager.UploadDirectoryAsync(srcDirectoryPath, dstBlob, options, context, linkedCancellation.Token).ConfigureAwait(false);
                transferTcs.TrySetResult(true);
                await transferTcs.Task.ConfigureAwait(false);
            }
            catch (OperationCanceledException e)
            {
                this.logger.LogInformation(e, $"The transfer {session.Id} has been cancelled");
            }
            catch (InvalidOperationException e) when (e.Message.Contains("Only one transfer is allowed with stream journal."))
            {
                this.logger.LogWarning(e, $"The transfer {session.Id} has failed");
                File.Delete(journalFile);
                throw;
            }
            catch (Exception e)
            {
                linkedCancellation.Cancel();
                this.logger.LogWarning(e, $"The transfer {session.Id} has failed");
                throw;
            }

            if (!linkedCancellation.IsCancellationRequested)
            {
                File.Delete(journalFile);
                return true;
            }

            return false;
        }

        private async Task<Stream> GetJournalStream(Session session, string journalFile)
        {
            var journalStream = new MemoryStream();
            if (File.Exists(journalFile))
            {
                this.logger.LogDebug($"The journal file for session {session.Id} exists and will be used to resume the upload");
                using var current = File.OpenRead(journalFile);
                await current.CopyToAsync(journalStream).ConfigureAwait(false);
                journalStream.Position = 0;
            }

            return journalStream;
        }

        private string GetJournalFileForSession(string srcDirectoryPath)
        {
            var journalDir = Directory.CreateDirectory(Path.Combine(srcDirectoryPath, "upload"));
            var journalFile = Path.Combine(journalDir.FullName, ".journal");
            return journalFile;
        }

        private async Task<CloudBlobDirectory> GetDirectoryBlobAsync(string sessionIdentifier)
        {
            var blobContainer = await this.cloudBlobContainerTask.ConfigureAwait(false);
            return blobContainer.GetDirectoryReference(sessionIdentifier);
        }

        public void RemoveUploadStatus(Session session)
        {
            var srcDirectoryPath = session.LocalRecordingFolderPath;
            string journalFile = this.GetJournalFileForSession(srcDirectoryPath);
            File.Delete(journalFile);
        }

        public void Dispose()
        {
            this.disposeCancellation.Cancel();
        }
    }

}
