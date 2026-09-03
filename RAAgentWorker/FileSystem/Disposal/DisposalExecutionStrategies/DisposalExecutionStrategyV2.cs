/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Media.Storage;
using AvePoint.RA.Common.Utils.ProtoBuf;
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Collect.NewLogic;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using RAFileSystem.Disposal.NewLogic;
using RAFileSystem.FileSystem.BaseProcessor;
using RAFileSystem.FileSystem.Common;
using RAFileSystem.FileSystem.DataIngestion;
using RAFileSystemCore.Common.JobHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using Task = System.Threading.Tasks.Task;

namespace RAFileSystem.FileSystem.Disposal.DisposalExecutionStrategies
{
    internal class DisposalExecutionStrategyV2 : BaseDisposalExecutionStrategy, IFSExecutionStrategy
    {
        private AveLogger _logger;
        private FSJobProcessorContext _context;
        private CancellationToken _cancellationToken = CancellationToken.None;

        internal static RMDataIngestionOperationType OperationType = RMDataIngestionOperationType.FileSystemEnforceRunAction;

        private static Channel<T> CreateBounded<T>(int capacity, bool singleWriter = false, bool singleReader = false) =>
            Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleWriter = singleWriter,
                SingleReader = singleReader
            });

        public void Initialize(FSJobProcessorContext context, AveLogger logger)
        {
            _logger = logger;
            _context = context;
            FSDataDisposalV2.ClassificationLevel = context.ClassificationLevel;
            FSDataDisposalV2.currentSetting = context.Setting;
            JobContext.Current.EnableFSHighPerformanceMode = true;
        }

        public void RegisterConnectionGroups(FSJobProcessorContext context)
        {
            // V2 does not register connection groups to AnalyzerCache
        }

        public void RegisterRootStub(FSJobProcessorContext context)
        {
            // V2 does not register root stub to caches
        }

        public void FinalizeInitialization(FSJobProcessorContext context)
        {
            InitializeChannels();
            InitializeDataIngestion();
        }

        public void HandleMissingDirectory(FSJobProcessorContext context)
        {
            JobContext.Current.JobDetailManager.Create().Commit(new JMFSDisposalJobDetailV2
            {
                AgentName = AvePoint.GCommon.Utility.OSInformation.HostName,
                ObjectName = Path.GetFileName(context.Node.FullPath),
                SourceLocation = context.Node.FullPath,
                Status = JobDetailsStatus.Failed,
                Comment = "RM_JS_JMD_FS_PathCanNotAccess",
                DirPath = context.Node.FullPath,
                Depth = 0,
                DetailAction = 0,
            });
        }

        public void HandleBindException(Exception exception)
        {
            // V2 channels are on FSJobCache and will be cleaned up automatically
        }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            return ExecuteAsync();
        }

        public async Task ExecuteAsync()
        {
            try
            {
                if (FSDataDisposalV2.ClassificationLevel == NodeLevel.FSFile)
                {
                    await ExecuteFileLevelClassificationAsync();
                }
                else
                {
                    await ExecuteFolderLevelClassificationAsync();
                }
            }
            catch (AgentJobStopException)
            {
                throw;
            }
            catch (Exception e)
            {
                FSJobCache.Instance.FailedCount++;
                _logger.Error($"Error occurred while running disposal job. Error:{e.ToString()}");
            }
        }

        private async Task ExecuteFileLevelClassificationAsync()
        {
            GetAllRecords();
            var allFolderCache = GetDisposalDiscoverFolders();
            if (allFolderCache != null && allFolderCache.Count > 0)
            {
                FSJobCache.Instance.DisposalFolderCache.AddBatch(allFolderCache.AsEnumerable());
                await StartSubThreadsAsync().ConfigureAwait(false);
            }
            else
            {
                _logger.Warn("No available folder path, skip running job.");
            }
        }

        private async Task ExecuteFolderLevelClassificationAsync()
        {
            var allExceptFolderCache = GetAllFolders(_context);
            FSJobCache.Instance.DisposalDifferentFolderCache.AddRange(allExceptFolderCache.AsEnumerable());
            await StartSubThreadsAsync().ConfigureAwait(false);
        }

        private async Task StartSubThreadsAsync()
        {
            try
            {
                _cancellationToken.ThrowIfAgentJobStopped();

                var discovery = new DisposalDiscoverV2();
                var worker = new DisposalWorkerV2();
                var updater = new DisposalDataUpdaterV2();

                var runningTasks = new List<Task>
                {
                    ExecuteTaskAsync("DiscoveryRun", () => discovery.Run()),
                    ExecuteTaskAsync("WorkerRun", () => worker.Run()),
                    ExecuteTaskAsync("DataUpdaterRun", () => updater.Run()),
                    ExecuteTaskAsync("CosmosSend", () => discovery.RunSendRecordsToCosmos()),
                    ExecuteTaskAsync("CosmosReceive", () => discovery.RunGetRecordsFromCosmos()),
                };

                var reportTask = ExecuteTaskAsync("ReportCollector", () => Task.Run(() => ProcessExecutionResults()));

                await Task.WhenAll(runningTasks).ConfigureAwait(false);

                _cancellationToken.ThrowIfAgentJobStopped();

                FSJobCache.Instance.DataIngestionDataCollector.Complete();

                await reportTask.ConfigureAwait(false);

                _cancellationToken.ThrowIfAgentJobStopped();

                RunSendEmailJob();
            }
            catch (AgentJobStopException)
            {
                _logger.Info("Disposal V2 job stopped via OPUS during worker coordination.");
                throw;
            }
            catch (Exception e)
            {
                _logger.Error("An error occurred while running sub threads. Error:" + e);
                throw;
            }
        }

        private void ProcessExecutionResults()
        {
            foreach (var item in FSJobCache.Instance.DataIngestionResultCollector.ReadItemExecutionResults())
            {
                _logger.Debug($"Receive ingestion result for item id: {item.Id}, " +
                    $"name: {ExternalUtil.CombinePath(item.DirPath, item.LeafName).LogBase64()}, " +
                    $"ruleName: {item.RuleName}, " +
                    $"ruleAction: {GetActionString(item.RuleAction)}, " +
                    $"isSucceed: {item.Succeed}, " +
                    $"{(!item.Succeed ? $"ErrorMessage: {item.Message}" : "")}");
            }
        }

        private Task ExecuteTaskAsync(string taskName, Func<Task> action)
        {
            return Task.Run(async () =>
            {
                try
                {
                    _logger.Info($"[Task Start] {taskName}");
                    await action();
                    _logger.Info($"[Task Success] {taskName}");
                }
                catch (Exception ex)
                {
                    _logger.Error($"[Task Failed] {taskName}. Error: {ex}");
                    throw;
                }
            });
        }

        private static string GetActionString(int action)
        {
            switch (action)
            {
                case (int)RuleAction.ArchiveAndRemove:
                    return "RM_FS_DisposalAction_Remove";
                case (int)RuleAction.MoveAndDeclare:
                    return "RM_FS_DisposalAction_Move";
                case (int)RuleAction.None:
                case (int)RuleAction.ArchiveAndKeep:
                case (int)RuleAction.ExportOnly:
                default:
                    return string.Empty;
            }
        }

        private void RunSendEmailJob()
        {
            _logger.Info("There is no send email serializer thread running now...");
            JobContext.Current.ApiClient.RunSendEmailJob(JobContext.Current.JobId);
        }

        private void InitializeChannels()
        {
            ProtobufRuntimeHelper.EnsureTypeRegistered<FileSystemRecordDto>();
            var batchCapacity = Math.Max(100, ExternalUtil.TransferDataCount);
            FSJobCache.Instance.DiscoveryToWorker = CreateBounded<(FSAzureTableEntityDto, FileSystemRecordDto)>(batchCapacity);
            FSJobCache.Instance.WorkerToUpdater = CreateBounded<(FSAzureTableEntityDto, FileSystemRecordDto)>(batchCapacity);
            FSJobCache.Instance.DiscoveryToCosmos = CreateBounded<FSAzureTableEntityDto>(batchCapacity);
            FSJobCache.Instance.ManualInFolderToCosmos = CreateBounded<FSAzureTableEntityDto>(batchCapacity);
        }

        private void InitializeDataIngestion()
        {
            var ingestionPersistor = new RMDataIngestionPersistor(JobContext.Current.JobId);
            FSJobCache.Instance.DataIngestionResultCollector = new RMDataIngestionExecutionResultCollector(
                JobContext.Current.JobId, OperationType, ingestionPersistor);
            FSJobCache.Instance.DataIngestMessageExtensionManager = new RMDataIngestMessageExtensionManager();
            FSJobCache.Instance.DataIngestionDataCollector = new RMDataIngestionDataCollector(
                FSJobCache.Instance.DataIngestionResultCollector,
                FSJobCache.Instance.DataIngestMessageExtensionManager,
                ingestionPersistor);
        }
    }
}