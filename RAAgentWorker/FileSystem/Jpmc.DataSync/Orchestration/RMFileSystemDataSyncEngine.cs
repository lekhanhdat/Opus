using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Storage;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using RAFileSystem.FileSystem.Common;
using RAFileSystem.FileSystem.DataIngestion.V2;
using RAFileSystemCore.ApiClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.Jpmc.DataSync
{
    public class RMFileSystemDataSyncEngine
    {
        private const int DefaultMaxDegreeOfParallelism = 5;

        private readonly AveLogger _logger = AveLogger.GetInstance(typeof(RMFileSystemDataSyncEngine));

        private readonly RMFileSystemJobExecutionInfo _executionInfo;

        private readonly IXSystem _filesystemClient;

        private readonly HashSet<Guid> _inheritanceBreakNodes;

        private readonly HashSet<string> _jobProcessingNodes;

        private readonly Dictionary<Guid, List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule>> _termRules;

        public RMFileSystemDataSyncEngine(string messageStr)
        {
            if (string.IsNullOrWhiteSpace(messageStr))
            {
                throw new ArgumentNullException(nameof(messageStr));
            }

            var message = SerializerHelper.DeserializeByDataContractSerializer<FSJobMessage>(messageStr);
            if (message?.FSTreeNodes == null || message.FSTreeNodes.Count == 0)
            {
                throw new InvalidOperationException("[DataSync] Invalid job message: FSTreeNodes is null or empty.");
            }

            _executionInfo = BuildExecutionInfo(message);

            _filesystemClient = ExternalUtil.OpenXSystem(_executionInfo.ConnectionPath);
            _inheritanceBreakNodes = FSJobCache.Instance.ScopeSettingCache.Keys.ToHashSet();
            _jobProcessingNodes = FSJobCache.Instance.RunningJobNodeUrls?.ToHashSet();
            _termRules = FSJobCache.Instance.TermRuleMapping;
        }

        public async Task RunAsync()
        {
            var jobStartTime = DateTime.UtcNow;
            _logger.Info($"[DataSync] Job started at {jobStartTime:O} for processing directory {_executionInfo.DirectoryFullPath.LogBase64()} with info: {_executionInfo}");

            var jobProgressTracker = new RMFileSystemJobProgressTracker(_executionInfo);

            ValidateDirectoryExists(jobProgressTracker);

            var recoveryProcessor = new RMFileSystemRecoveryProcessor(_executionInfo);
            await recoveryProcessor.InitAsync().ConfigureAwait(false);
            var auditProcessor = new RMFileSystemAuditProcessor(_executionInfo);
            var classCodeProcessor = new RMFileSystemClassCodeProcessor(jobProgressTracker, recoveryProcessor, _executionInfo, _termRules);
            var uniqueIdProcessor = new RMFileSystemUniqueIdProcessor(jobProgressTracker, recoveryProcessor, _executionInfo);
            var assembler = new RMFileSystemMetadataAssembler(_executionInfo);

            try
            {
                await RegisterConnectionGroupNodesAsync(_executionInfo, assembler);

                var dataCollector = new RMDataIngestionCollector(
                    JobContext.Current.JobId,
                    RMDataIngestionOperationType.FileSystemDataSync,
                    (record, succeed) =>
                    {
                        jobProgressTracker.AddJobDetail(record, succeed);
                        if(!succeed) recoveryProcessor.AddFailedItem(record);
                    });

                var monitorTask = dataCollector.MonitorAsync();

                _logger.Info($"[DataSync] Start scanning directory {_executionInfo.DirectoryFullPath.LogBase64()} and subdirectories.");

                var scanCompletedWithoutError = true;

                try
                {

                    var directoryScanner = new RMFileSystemDirectoryScanner(jobProgressTracker, recoveryProcessor, _executionInfo, _inheritanceBreakNodes, _jobProcessingNodes, _filesystemClient);

                    await directoryScanner.ScanStreamingAsync(async (directory) => await classCodeProcessor.ProcessDirectoryAsync(directory))
                    .ParallelForEachAsync(async (directory, _) =>
                    {
                        await ProcessDirectoryPipelineAsync(
                                directory,
                                jobProgressTracker,
                                recoveryProcessor,
                                classCodeProcessor,
                                uniqueIdProcessor,
                                auditProcessor,
                                assembler,
                                dataCollector).ConfigureAwait(false);
                    }, maxDegreeOfParallelism: _executionInfo.MaxConcurrentExecutionCount ).ConfigureAwait(false);

                }
                catch (Exception ex)
                {
                    scanCompletedWithoutError = false;
                    _logger.Error($"[DataSync] An error occurred while scanning directory {_executionInfo.DirectoryFullPath.LogBase64()}. Error: {ex}");
                }
                finally
                {
                    dataCollector.Complete();
                    await monitorTask.ConfigureAwait(false);
                }

                if (scanCompletedWithoutError)
                {
                    var shouldUpdateLastSyncTime = await recoveryProcessor.SyncFailedItemsAsync().ConfigureAwait(false);
                    _logger.Info($"[DataSync] Completed scanning directory {_executionInfo.DirectoryFullPath.LogBase64()}. Should update last sync time: {shouldUpdateLastSyncTime}.");

                    if (shouldUpdateLastSyncTime)
                    {
                        TryUpdateJobTime(jobStartTime);
                    }
                }
            }
            catch (Exception ex)
            {
                jobProgressTracker.IncreaseFailedCount();
                _logger.Error($"[DataSync] An error occurred while processing directory {_executionInfo.DirectoryFullPath.LogBase64()}. Error: {ex}");
            }
            finally
            {
                jobProgressTracker.NotfiyJobStatus();
            }

            _logger.Info($"[DataSync] Completed processing directory {_executionInfo.DirectoryFullPath.LogBase64()} and its subdirectories.");
        }

        private static RMFileSystemJobExecutionInfo BuildExecutionInfo(FSJobMessage message)
        {
            var executionNode = DtoConverter.ConvertGlobalDto2FSTreeNodeDto(message.FSTreeNodes[0]);
            var topThreeLevelNodes = ExternalUtil.FindTop3LevelNodes(executionNode);

            var info = new RMFileSystemJobExecutionInfo
            {
                DirectoryId = executionNode.FullPath.ToLowerInvariant().ToMd5(),
                DirectoryFullPath = executionNode.FullPath,
                DirectoryRelativePath = executionNode.FullPath.Substring(topThreeLevelNodes.Item3.FullPath.Length).Trim('\\'),
                DirectoryParentId = executionNode.FullPath.TrimEnd('\\').EqualsIgnoreCase(topThreeLevelNodes.Item3.FullPath.TrimEnd('\\')) ? new Guid(topThreeLevelNodes.Item2.ID) : executionNode.Parent.FullPath.Trim('\\').ToLowerInvariant().ToMd5(),
                DirectoryParentFullPath = executionNode.Parent.FullPath.Trim('\\'),
                RootId = topThreeLevelNodes.Item1.ID,
                RootPath = topThreeLevelNodes.Item1.Name,
                ConnectionGroupId = topThreeLevelNodes.Item2.ID,
                ConnectionGroupPath = topThreeLevelNodes.Item2.Name,
                ConnectionId = topThreeLevelNodes.Item3.ID,
                ConnectionPath = topThreeLevelNodes.Item3.FullPath.TrimEnd('\\'),
                ExecutionType = message.FSJobType,
                LastScanTime = message.IBStartTime,
                EnabledRecordManagement = HybridApiClient.Instance.LoadFSNodeEnableRecordManagement(new Guid(executionNode.ID)),
            };

            var maxConcurrentLimit = HyperHybridAPIClient.Instance.GetMaxConcurrentExecutionAsync().GetAwaiter().GetResult();
            info.MaxConcurrentExecutionCount = maxConcurrentLimit > 0 ? maxConcurrentLimit : DefaultMaxDegreeOfParallelism;

            var uniqueIdSetting = HybridApiClient.Instance.GetUniqueIdSetting();
            if (uniqueIdSetting != null)
            {
                info.UniqueIdSetting = new RMFileSystemUniqueIdSetting
                {
                    Actived = uniqueIdSetting.IsActived,
                    Stored = uniqueIdSetting.IsStored,
                    Prefix = uniqueIdSetting.Prefix
                };
            }

            if (message.ClassCodeDto != null)
            {
                info.ClassCodeInfo = new RMFileSystemClassCode
                {
                    Id = message.ClassCodeDto.TermId,
                    Name = message.ClassCodeDto.ClassCode,
                    CountryCode = message.ClassCodeDto.CountryCode,
                    RetentionType = message.ClassCodeDto.RetentionType,
                    StartDate = message.ClassCodeDto.StartDate,
                    PolicyValueUnit = message.ClassCodeDto.PolicyValueUnit,
                    PolicyValueNumber = message.ClassCodeDto.PolicyValueNumber
                };
            }

            return info;
        }

        private void ValidateDirectoryExists(RMFileSystemJobProgressTracker jobProgressTracker)
        {
            var storageInfo = new StorageInfo { HighName = _executionInfo.DirectoryRelativePath };
            if (!_filesystemClient.DirectoryExists(storageInfo))
            {
                var logPath = _executionInfo.DirectoryFullPath.LogBase64();
                _logger.Error($"[DataSync] The directory {logPath} does not exist in the file system. Please check if the directory has been deleted or moved.");

                jobProgressTracker.AddCannotAccessJobDetail();
                throw new FileNotFoundException($"The directory {logPath} does not exist in the file system. Please check if the directory has been deleted or moved.");
            }
        }

        public async Task RegisterConnectionGroupNodesAsync(RMFileSystemJobExecutionInfo executionInfo, RMFileSystemMetadataAssembler metadataAssembler)
        {
            try
            {
                if (!Guid.TryParse(executionInfo.RootId, out var rootGuid) || !Guid.TryParse(executionInfo.ConnectionGroupId, out var groupGuid))
                {
                    _logger.Warn("[DataSync] Invalid RootId ({0}) or ConnectionGroupId ({1}). Operation skipped.",
                        executionInfo.RootId, executionInfo.ConnectionGroupId);
                    return;
                }

                var groupNodeIds = new List<Guid> { rootGuid, groupGuid };
                var existingRecords = await HyperHybridAPIClient.Instance.QueryFileSystemRecordsAsync(Guid.Empty.ToString(), groupNodeIds);

                var existingRecordsLookup = existingRecords.Select(r => r.NodeId).ToHashSet();

                var nodeConfigurations = new (Guid NodeId, Func<RMFileSystemJobExecutionInfo, FileSystemRecordDto> AssembleFunc)[]
                {
                    (rootGuid, metadataAssembler.AssembleRootRecordInfo),
                    (groupGuid, metadataAssembler.AssembleGroupRecordInfo)
                };

                var recordsToUpsert = nodeConfigurations
                    .Where(nodeConfig => !existingRecordsLookup.Contains(nodeConfig.NodeId))
                    .Select(nodeConfig => nodeConfig.AssembleFunc(executionInfo))
                    .ToList();

                if (recordsToUpsert.Any())
                {
                    await HyperHybridAPIClient.Instance.UpsertFileSystemRecordsAsync(recordsToUpsert);
                    _logger.Info($"[DataSync] Registered {recordsToUpsert.Count} connection group nodes for RootId: {executionInfo.RootId}, ConnectionGroupId: {executionInfo.ConnectionGroupId}.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"[DataSync] An error occurred while registering connection group nodes. Error: {ex}");
                throw;
            }
        }

        private void TryUpdateJobTime(DateTime jobStartTime)
        {
            try
            {
                HybridApiClient.Instance.UpdateJobTime(new RMFileSystemJobTimeReferenceDto
                {
                    LastJobTime = jobStartTime,
                    Path = _executionInfo.DirectoryFullPath,
                    ScopeId = _executionInfo.DirectoryId
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"[DataSync] An error occurred while updating last sync time for directory {_executionInfo.DirectoryFullPath.LogBase64()}. Error: {ex}");
            }
        }

        private async Task ProcessDirectoryPipelineAsync(
            RMFileSystemDirectoryMetadata directory,
            RMFileSystemJobProgressTracker progressTracker,
            RMFileSystemRecoveryProcessor recoveryProcessor,
            RMFileSystemClassCodeProcessor classCodeProcessor,
            RMFileSystemUniqueIdProcessor uniqueIdProcessor,
            RMFileSystemAuditProcessor auditProcessor,
            RMFileSystemMetadataAssembler assembler,
            RMDataIngestionCollector dataCollector)
        {
            progressTracker.IncreseBaseProgress(1);

            if (!await classCodeProcessor.ProcessDirectoryAsync(directory).ConfigureAwait(false)) return;
            
            await using var dataIngestionWriter = dataCollector.CreateWriter(directory.Id.ToString());
           
            if (!directory.IsHidden && (directory.HasChanged || directory.IsPriorFailure))
            {
                if (!await uniqueIdProcessor.ProcessDirectoryAsync(directory).ConfigureAwait(false)) return;
                await auditProcessor.ProcessDirectoryAsync(directory).ConfigureAwait(false);

                var directoryRecord = assembler.AssembleDirectoryRecordInfo(directory);
                if (!await dataIngestionWriter.WriteAsync(directoryRecord).ConfigureAwait(false))
                {
                    recoveryProcessor.AddFailedItem(directoryRecord);
                    progressTracker.AddJobDetail(directoryRecord, false);
                }
            }

            await ProcessFileBatchesAsync(directory,
                progressTracker,
                recoveryProcessor,
                classCodeProcessor,
                uniqueIdProcessor,
                auditProcessor,
                assembler,
                dataIngestionWriter).ConfigureAwait(false);
        }

        private async Task ProcessFileBatchesAsync(
            RMFileSystemDirectoryMetadata directory,
            RMFileSystemJobProgressTracker progressTracker,
            RMFileSystemRecoveryProcessor recoveryProcessor,
            RMFileSystemClassCodeProcessor classCodeProcessor,
            RMFileSystemUniqueIdProcessor uniqueIdProcessor,
            RMFileSystemAuditProcessor auditProcessor,
            RMFileSystemMetadataAssembler assembler,
            RMDataIngestionDataWriter dataIngestionWriter)
        {
            var fileScanner = new RMFileSystemFileScanner(progressTracker, recoveryProcessor, _executionInfo, directory, _filesystemClient);

            await foreach (var fileBatch in fileScanner.ScanStreamingAsync().ConfigureAwait(false))
            {
                progressTracker.IncreseBaseProgress(fileBatch.Count);

                var classCodeSuccessFiles = classCodeProcessor.ProcessFiles(fileBatch);
                _logger.Info($"[DataSync] Processed ClassCode for {classCodeSuccessFiles.Count} files under {directory.FullPath.LogBase64()}.");

                var uniqueIdSuccessFiles = await uniqueIdProcessor.ProcessFilesAsync(classCodeSuccessFiles).ConfigureAwait(false);
                _logger.Info($"[DataSync] Processed UniqueID for {uniqueIdSuccessFiles.Count} files under {directory.FullPath.LogBase64()}.");

                await auditProcessor.ProcessFilesAsync(uniqueIdSuccessFiles).ConfigureAwait(false);

                var fileRecords = uniqueIdSuccessFiles.ConvertAll(file => assembler.AssembleFileRecordInfo(file));
                var failedRecords = await dataIngestionWriter.WriteAsync(fileRecords).ConfigureAwait(false);
                recoveryProcessor.AddFailedItems(failedRecords);
                progressTracker.AddJobDetails(failedRecords, false);
            }
        }
    }
}
