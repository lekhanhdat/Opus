using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Common.Tracking.Performance;
using AvePoint.RA.Common.Utils.ProtoBuf;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Protobuf;
using AvePoint.RA.Contract.Tenant;
using Azure.Storage.Blobs;
using ProtoBuf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.DataIngestion.V2
{
    public class RMDataIngestionCollector
    {
        private const int DEFAULT_MAX_RECORD_PER_FILE = 10_000;

        private static readonly TimeSpan PollingInterval = TimeSpan.FromMinutes(1);

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDataIngestionCollector));

        private readonly string _jobId;

        private readonly RMDataIngestionOperationType _operationType;

        private readonly Action<FileSystemRecordDto, bool> _onNotify;

        private readonly int _maxRecordPerFile;

        private readonly string _folderPath;

        private readonly ConcurrentDictionary<string, RMDataIngestionMessageSendReceipt> _activeReceipts
            = new ConcurrentDictionary<string, RMDataIngestionMessageSendReceipt>();

        private volatile bool _isCompleted;

        public RMDataIngestionCollector(
            string jobId,
            RMDataIngestionOperationType operationType,
            Action<FileSystemRecordDto, bool> onNotify,
            int maxRecordPerFile = DEFAULT_MAX_RECORD_PER_FILE
            )
        {
            _jobId = jobId ?? throw new ArgumentNullException(nameof(jobId));
            _operationType = operationType;
            _onNotify = onNotify;
            _maxRecordPerFile = maxRecordPerFile > 0 ? maxRecordPerFile : DEFAULT_MAX_RECORD_PER_FILE;

            _folderPath = Path.Combine(Path.GetTempPath(), "DataIngestion", TenantAgentInfo.AgentId ?? "DefaultAgent");
            if (!Directory.Exists(_folderPath))
            {
                Directory.CreateDirectory(_folderPath);
            }
            ProtobufRuntimeHelper.EnsureTypeRegistered<RMDataIngestionAgentWorkItemExecutionResult>();
            ProtobufRuntimeHelper.EnsureTypeRegistered<FileSystemRecordDto>();
        }

        public RMDataIngestionDataWriter CreateWriter(string mark)
        {
            return new RMDataIngestionDataWriter(
                _jobId,
                _operationType,
                _folderPath,
                mark,
                _maxRecordPerFile,
                UploadAsync);
        }

        public void Complete()
        {
            _isCompleted = true;
        }

        public async Task MonitorAsync()
        {
            _logger.Info($"[DataIngestion] Starting execution monitor for job {_jobId}, operation {_operationType}");

            while (!_isCompleted || !_activeReceipts.IsEmpty)
            {
                var activeFiles = _activeReceipts.Keys.ToList();

                if (activeFiles.Count == 0)
                {
                    await Task.Delay(PollingInterval).ConfigureAwait(false);
                    continue;
                }

                foreach (var filePath in activeFiles)
                {
                    if (!_activeReceipts.TryGetValue(filePath, out var receipt)) continue;

                    try
                    {
                        var executionResult = HybridApiClient.Instance.DataIngestionGetExecutionResult(_jobId, receipt.MessageId);
                        if (executionResult == null ||
                           (executionResult.Status != RMDataIngestionStatus.Succeed && executionResult.Status != RMDataIngestionStatus.Failed))
                        {
                            continue;
                        }

                        if (executionResult.Status == RMDataIngestionStatus.Failed)
                        {
                            await HandleFailedMessageAsync(filePath, receipt).ConfigureAwait(false);
                        }
                        else
                        {
                            await HandleSucceededMessageAsync(filePath, receipt, executionResult).ConfigureAwait(false);
                        }

                        _activeReceipts.TryRemove(filePath, out _);
                        SafeDeleteLocalFile(filePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"[DataIngestion] Error monitoring execution result for file {filePath}, message {receipt.MessageId}", ex);
                    }
                }

                await Task.Delay(PollingInterval).ConfigureAwait(false);
            }

            _logger.Info($"[DataIngestion] Monitor completed for job {_jobId}, operation {_operationType}");
        }

        private async Task UploadAsync(string filePath)
        {
            using var performanceScope = RMPerformanceMonitor.Scope("Data Ingestion Upload");

            try
            {
                var blobReference = performanceScope.Step("Generate Blob Reference", () => HybridApiClient.Instance.DataIngestionGenerateBlobReference(new RMDataIngestionBlobNamingContext
                {
                    UniqueId = _jobId,
                    IngestionType = RMDataIngestionType.AgentWork,
                    OperationType = _operationType,
                    BlobType = RMDataIngestionBlobType.Source
                }));

                if (blobReference == null || string.IsNullOrEmpty(blobReference.SasUri))
                {
                    string errorMsg = $"[DataIngestion] Failed to generate blob reference for job {_jobId} and operation {_operationType}";
                    _logger.Error(errorMsg);
                    throw new InvalidOperationException(errorMsg);
                }

                var blobClient = new BlobClient(new Uri(blobReference.SasUri));
                await performanceScope.StepAsync("Upload to Blob", async () => await blobClient.UploadAsync(filePath).ConfigureAwait(false)).ConfigureAwait(false);

                _logger.Info($"[DataIngestion] Successfully uploaded file {filePath} to blob {blobReference.BlobName} for job {_jobId}");

                var receipt = performanceScope.Step("Send Message", () => HybridApiClient.Instance.DataIngestionSendMessage(new RMDataIngestionMessageDto
                {
                    Id = Guid.NewGuid(),
                    UniqueId = _jobId,
                    IngestionType = RMDataIngestionType.AgentWork,
                    OperationType = _operationType,
                    SourceBlobName = blobReference.BlobName,
                    CreatedTime = DateTime.UtcNow.Ticks
                }));

                if (receipt == null)
                {
                    string errorMsg = $"[DataIngestion] Failed to send ingestion message for job {_jobId} and operation {_operationType}";
                    _logger.Error(errorMsg);
                    throw new InvalidOperationException(errorMsg);
                }

                _logger.Info($"[DataIngestion] Successfully sent ingestion message with message id {receipt.MessageId}");
                _activeReceipts.TryAdd(filePath, receipt);
            }
            catch (Exception ex)
            {
                _logger.Error($"[DataIngestion] Error uploading file {filePath} for job {_jobId} and operation {_operationType}", ex);
                performanceScope.MarkFaulted();
                using var fileStream = File.OpenRead(filePath);
                var items = Serializer.DeserializeItems<FSDataIngestion<FileSystemRecordDto>>(fileStream, PrefixStyle.Base128, 1);
                foreach (var item in items)
                {
                    _onNotify(item.Item, false);
                }
            }
        }

        private async Task HandleFailedMessageAsync(string filePath, RMDataIngestionMessageSendReceipt receipt)
        {
            _logger.Error($"[DataIngestion] Ingestion message {receipt.MessageId} failed for file {filePath}.");
            if (!File.Exists(filePath)) return;

            using var fileStream = File.OpenRead(filePath);
            var items = Serializer.DeserializeItems<FSDataIngestion<FileSystemRecordDto>>(fileStream, PrefixStyle.Base128, 1);
            foreach (var item in items)
            {
                _onNotify(item.Item, false);
            }
        }

        private async Task HandleSucceededMessageAsync(string filePath, RMDataIngestionMessageSendReceipt receipt, RMDataIngestionExecutionResult executionResult)
        {
            _logger.Info($"[DataIngestion] Ingestion message {receipt.MessageId} succeeded.");
            var sasUri = HybridApiClient.Instance.DataIngestionGenerateBlobSasUri(RMDataIngestionType.AgentWork, executionResult.ResultBlobName);
            var blobClient = new BlobClient(new Uri(sasUri));

            HashSet<Guid> failedNodeIds = new HashSet<Guid>();
            using (var streamReader = await blobClient.OpenReadAsync())
            {
                var items = Serializer.DeserializeItems<RMDataIngestionAgentWorkItemExecutionResult>(streamReader, PrefixStyle.Base128, 1);
                foreach (var item in items)
                {
                    failedNodeIds.Add(item.Id);
                }
            }

            if (File.Exists(filePath))
            {
                using (var fileStream = File.OpenRead(filePath))
                {
                    var items = Serializer.DeserializeItems<FSDataIngestion<FileSystemRecordDto>>(fileStream, PrefixStyle.Base128, 1);
                    foreach (var item in items)
                    {
                        bool isSuccess = !failedNodeIds.Contains(item.Item.NodeId);
                        _onNotify(item.Item, isSuccess);
                    }
                }
            }

            HybridApiClient.Instance.DeleteBlobByName(new RMDataIngestionBlobDto
            {
                BlobName = executionResult.ResultBlobName,
                IngestionType = RMDataIngestionType.AgentWork
            });
        }

        private void SafeDeleteLocalFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.Warn($"[DataIngestion] Failed to delete temporary file {filePath}: {ex.Message}");
            }
        }
    }
}

