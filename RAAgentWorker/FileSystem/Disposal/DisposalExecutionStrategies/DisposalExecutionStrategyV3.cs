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
using AvePoint.RA.Common.Utils.ProtoBuf;
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using RAFileSystem.Disposal.NewLogic;
using RAFileSystem.FileSystem.BaseProcessor;
using RAFileSystem.FileSystem.Common;
using RAFileSystem.FileSystem.DataIngestion;
using RAFileSystem.FileSystem.Disposal.NewLogic.V3;
using RAFileSystem.FileSystem.Disposal.NewLogic.V3.Services;
using RAFileSystem.FileSystem.Disposal.NewLogic.V3.Workers;
using RAFileSystemCore.Common.JobHandler;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.Disposal.DisposalExecutionStrategies
{
    public class DisposalExecutionStrategyV3 : BaseDisposalExecutionStrategy, IFSExecutionStrategy
    {
        private AveLogger _logger;
        private FSJobProcessorContext _context;
        private FSDisposalChannelProvider _channelProvider;
        private DisposalReportService _reportService;
        private DisposalFileAnalyzer _fileAnalyzer;
        private CancellationToken _cancellationToken = CancellationToken.None;

        internal static RMDataIngestionOperationType OperationType = RMDataIngestionOperationType.FileSystemEnforceRunAction;
        internal static NodeLevel ClassificationLevel;
        internal static FSSettingDto CurrentSetting;

        public void Initialize(FSJobProcessorContext context, AveLogger logger)
        {
            _logger = logger;
            _context = context;
            ClassificationLevel = context.ClassificationLevel;
            CurrentSetting = context.Setting;
            FSDataDisposalV2.ClassificationLevel = context.ClassificationLevel;
            JobContext.Current.EnableFSHighPerformanceMode = true;
            ProtobufRuntimeHelper.EnsureTypeRegistered<FileSystemRecordDto>();
            var batchCapacity = Math.Max(100, ExternalUtil.TransferDataCount);
            _channelProvider = new FSDisposalChannelProvider(batchCapacity);
            var progressService = JobContext.Current.mProgressManager.Create();
            var jobDetailService = JobContext.Current.JobDetailManager.Create();

            var metadataExtractor = new FileMetadataExtractor();
            var dtoFactory = new DisposalDtoFactory(metadataExtractor);
            _reportService = new DisposalReportService(jobDetailService, progressService);
            _fileAnalyzer = new DisposalFileAnalyzer(dtoFactory, _reportService, _channelProvider);
        }

        public void RegisterConnectionGroups(FSJobProcessorContext context)
        {
        }

        public void RegisterRootStub(FSJobProcessorContext context)
        {
        }

        public void FinalizeInitialization(FSJobProcessorContext context)
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

        public void HandleMissingDirectory(FSJobProcessorContext context)
        {
            JobContext.Current.JobDetailManager.Create().Commit(new JMFSDisposalJobDetailV2
            {
                AgentName = AvePoint.GCommon.Utility.OSInformation.HostName,
                ObjectName = Path.GetFileName(context.Node.FullPath),
                SourceLocation = context.Node.FullPath,
                Status = JobDetailsStatus.Failed,
                Comment = "RM_JS_JMD_FS_JPMC_PathCanNotAccess",
                DirPath = context.Node.FullPath,
                Depth = 0,
                DetailAction = 0,
            });
        }

        public void HandleBindException(Exception exception)
        {
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
                if (_context.Top3Nodes != null && _context.Top3Nodes.Item3 != null) {
                    string connId = _context.Top3Nodes.Item3.ID;
                    bool res = JobContext.Current.ApiClient.CheckConnectionStatus(connId);
                    if (res) {
                        _logger.Info("Running disposal job, ConnectionId: {0} is Paused", connId);
                        _reportService.AddConnectionSkipReport(_context.Top3Nodes.Item3);
                        return;
                    }
                }

                if (ClassificationLevel == NodeLevel.FSFile)
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
                _logger.Error("Error occurred while running disposal job. Error: {0}", e);
            }
        }

        private async Task ExecuteFileLevelClassificationAsync()
        {
            GetAllRecords(true);
            var allFolderCache = GetDisposalDiscoverFoldersV2();
            if (allFolderCache != null && allFolderCache.Count > 0)
            {
                FSJobCache.Instance.DisposalFolderCache.AddBatch(allFolderCache.AsEnumerable());
                await CoordinateDisposalWorkersAsync();
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
            await CoordinateDisposalWorkersAsync();
        }

        private async Task CoordinateDisposalWorkersAsync()
        {
            try
            {
                _cancellationToken.ThrowIfAgentJobStopped();

                var discovery = new DisposalDiscoveryWorkerV3(_reportService, _fileAnalyzer);
                var worker = new DisposalWorkerV2(_reportService);
                var updater = new DisposalDataUpdaterV2();
                var cosmosSend = new DisposalCosmosSenderWorkerV3(_channelProvider, _reportService);
                var cosmosReceive = new DisposalCosmosReceiverWorkerV3(_channelProvider, _fileAnalyzer);

                var runningTasks = new List<Task>
                {
                    ExecuteTaskAsync("DiscoveryRun", () => discovery.RunAsync()),
                    ExecuteTaskAsync("WorkerRun", () => worker.Run()),
                    ExecuteTaskAsync("DataUpdaterRun", () => updater.Run()),
                    ExecuteTaskAsync("CosmosSend", () => cosmosSend.RunAsync()),
                    ExecuteTaskAsync("CosmosReceive", () => cosmosReceive.RunAsync())
                };

                var reportTask = ExecuteTaskAsync("ReportCollector", () => Task.Run(ProcessExecutionResults));

                using (_cancellationToken.Register(() =>
                {
                    _logger.Info("Stop signal received. Completing all disposal channels.");
                    FSJobCache.Instance.ManualInFolderToCosmos?.Writer.TryComplete();
                    FSJobCache.Instance.DiscoveryToWorker?.Writer.TryComplete();
                    FSJobCache.Instance.WorkerToUpdater?.Writer.TryComplete();
                    FSJobCache.Instance.DiscoveryToCosmos?.Writer.TryComplete();
                }))
                {
                    await Task.WhenAll(runningTasks).ConfigureAwait(false);

                    _cancellationToken.ThrowIfAgentJobStopped();

                    FSJobCache.Instance.DataIngestionDataCollector.Complete();

                    await reportTask.ConfigureAwait(false);

                    _cancellationToken.ThrowIfAgentJobStopped();

                    RunSendEmailJob();
                }
            }
            catch (AgentJobStopException)
            {
                _logger.Info("Disposal V3 job stopped via OPUS during worker coordination.");
                throw;
            }
            catch (Exception e)
            {
                _logger.Error("An error occurred while running sub threads. Error:" + e);
                throw;
            }
        }

        private void RunSendEmailJob()
        {
            _logger.Info("There is no send email serializer thread running now...");
            JobContext.Current.ApiClient.RunSendEmailJob(JobContext.Current.JobId);
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

        private void ProcessExecutionResults()
        {
            foreach (var item in FSJobCache.Instance.DataIngestionResultCollector.ReadItemExecutionResults())
            {
                _logger.Debug($"Receive ingestion result for item id: {item.Id}, " +
                              $"name: {ExternalUtil.CombinePath(item.DirPath, item.LeafName).LogBase64()}, " +
                              $"ruleName: {item.RuleName}, " +
                              $"ruleAction: {DisposalRuleUtility.GetActionString(item.RuleAction)}, " +
                              $"isSucceed: {item.Succeed}, " +
                              $"{(!item.Succeed ? $"ErrorMessage: {item.Message}" : "")}");
            }
        }
    }
}