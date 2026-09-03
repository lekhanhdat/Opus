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
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.PerformanceScope;
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Stubs;
using RAFileSystem.FileSystem.BaseProcessor;
using RAFileSystem.FileSystem.DataIngestion;
using RAFileSystem.FileSystem.DataSync.Utils;
using RAFileSystem.FileSystem.DataSync.V2;
using RAFileSystemCore.Common.JobHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.DataSync.DataSyncExecutionStrategies
{
    internal class DataSyncExecutionStrategyV2 : IFSExecutionStrategy
    {
        private AveLogger _logger;
        private FSDataSyncChannelProvider _channelProvider;
        //private RMDataIngestionDataCollector _ingestionDataCollector;
        //private RMDataIngestionExecutionResultCollector _ingestionExecutionResultCollector;
        //private RMDataIngestionPersistor _ingestionPersistor;
        private List<RMDataIngestionDataCollector> rMDataIngestionDataCollectors = new List<RMDataIngestionDataCollector>();
        private CancellationTokenSource _cts;
        private FSJobProcessorContext _context;
        public void Initialize(FSJobProcessorContext context, AveLogger logger)
        {
            using (new AgentPerformanceScope("DataSyncStrategy.Initialize", addToStatistics: true))
            {
                _logger = logger;
                _context = context;
                var dataSyncContext = context as FSDataSyncJobContext;
                if (dataSyncContext == null)
                {
                    throw new ArgumentException("Data sync strategy requires a data sync job context.", nameof(context));
                }
                FSDataCollectorV2.ClassificationLevel = dataSyncContext.ClassificationLevel;
                FSDataCollectorV2.UniqueIdSetting = JobContext.Current.ApiClient.GetUniqueIdSetting();
                JobContext.Current.EnableFSHighPerformanceMode = true;
                _cts = new CancellationTokenSource();
                _channelProvider = new FSDataSyncChannelProvider();
            }
        }

        public void RegisterConnectionGroups(FSJobProcessorContext context)
        {
            using (new AgentPerformanceScope("DataSyncStrategy.RegisterConnectionGroups", addToStatistics: true))
            {
                try
                {
                    var topNodes = new List<Stub>
                    {
                        new FSConnectionGroupsStub
                        {
                            FullPath = context.Top3Nodes.Item1.Name,
                            SelfId = new Guid(context.Top3Nodes.Item1.ID)
                        },
                        new FSConnectionGroupStub
                        {
                            FullPath = context.Top3Nodes.Item2.Name,
                            SelfId = new Guid(context.Top3Nodes.Item2.ID),
                            ParentId = new Guid(context.Top3Nodes.Item1.ID)
                        }
                    };

                    var syncedRecords = JobContext.Current.ApiClient.QueryFileSystemRecords(Guid.Empty.ToString(), topNodes.Select(n => n.SelfId).ToList());
                    for (int i = 0; i < topNodes.Count; i++)
                    {
                        var dbRecord = syncedRecords.FirstOrDefault(r => r.NodeId == topNodes[i].SelfId);
                        if (dbRecord != null)
                        {
                            topNodes[i].DBRecord = dbRecord;
                        }
                    }

                    _channelProvider.WriteBatchToAnalyzerAsync(topNodes, _cts.Token);
                }
                catch (Exception ex)
                {
                    _logger.Error("An error occurred while registering connection groups for data synchronization job. Ex: {0}", ex);
                    throw;
                }
            }
        }

        public void RegisterRootStub(FSJobProcessorContext context)
        {
            _channelProvider.WriteToDiscoverAsync(context.RootStub, _cts.Token);
            _channelProvider.IncreaseDiscoveryCount();
        }

        public (RMDataIngestionDataCollector, RMDataIngestionExecutionResultCollector) FinalizeInitializationV2(FSJobProcessorContext context)
        {
            using (new AgentPerformanceScope("DataSyncStrategy.FinalizeInitialization", addToStatistics: true))
            {
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                var ingestionPersistor = new RMDataIngestionPersistor(JobContext.Current.JobId);
                var ingestionExecutionResultCollector = new RMDataIngestionExecutionResultCollector(JobContext.Current.JobId, RMDataIngestionOperationType.FileSystemDataSync, ingestionPersistor);
                var ingestionDataCollector = new RMDataIngestionDataCollector(ingestionExecutionResultCollector, ingestionPersistor);
                rMDataIngestionDataCollectors.Add(ingestionDataCollector);
                return (ingestionDataCollector, ingestionExecutionResultCollector);
            }
        }
        public void FinalizeInitialization(FSJobProcessorContext context)
        {

        }
        public void HandleMissingDirectory(FSJobProcessorContext context)
        {
            JobContext.Current.JobDetailManager.Create().Commit(new FSDataSyncJobReportDetailV2
            {
                AgentName = OSInformation.HostName,
                ObjectName = Path.GetFileName(context.Node.FullPath),
                FullPath = context.Node.FullPath,
                Status = JobDetailsStatus.Failed,
                Comment = "RM_JS_JMD_FS_JPMC_PathCanNotAccess",
                Depth = 0,
                DirPath = context.Node.FullPath
            });
            JobContext.Current.HasErrorNode = true;
            FSJobCache.Instance.FailedCount++;
        }

        public void HandleBindException(Exception exception)
        {
            if (_cts != null)
            {
                _cts.Cancel();
            }
            if (_channelProvider != null)
            {
                _channelProvider.SetCompleteAll();
            }
        }

        public Task ExecuteAsync()
        {
            return CoordinateDataSyncWorkersAsync(CancellationToken.None);
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using (cancellationToken.Register(() => _cts.Cancel()))
            {
                try
                {
                    await CoordinateDataSyncWorkersAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (AgentJobStopException)
                {
                    _logger.Info("DataSync job stopped by user request. Cleaning up.");
                    _channelProvider.SetCompleteAll();
                    throw;
                }
            }
        }

        private async Task CoordinateDataSyncWorkersAsync(CancellationToken externalToken)
        {
            using (new AgentPerformanceScope("DataSyncStrategy.CoordinateWorkers", addToStatistics: true))
            {
                var discoverWorkers = ExecuteWorkers(ConfigUtils.DISCOVERY_AND_ANALYZE_THREAD_COUNT, () => new FSDiscoveryWorker(_channelProvider, _cts.Token), w => w.RunAsync());
                var analyzerWorkers = ExecuteWorkers(ConfigUtils.DISCOVERY_AND_ANALYZE_THREAD_COUNT, () => new FSAnalysisWorker(_channelProvider, _cts.Token), w => w.RunAsync());
                var persistWorkers = new List<Task>();
                var reportWorkers = new List<Task>();
                for (int i = 0; i < ConfigUtils.PERSIST_AND_REPORT_THREAD_COUNT; i++)
                {
                    var (ingestionDataCollector, ingestionExecutionResultCollector) = FinalizeInitializationV2(_context);
                    var persistWorker = new FSPersistWorker(_channelProvider, ingestionDataCollector, ingestionExecutionResultCollector, _cts.Token);
                    persistWorkers.Add(Task.Run(() => persistWorker.RunAsync()));
                    var reportWorker = new FSReportWorker(_channelProvider, ingestionExecutionResultCollector, _cts.Token);
                    reportWorkers.Add(Task.Run(() => reportWorker.RunAsync()));
                }
                _logger.Info("All workers started for data synchronization job. Discoverers: {0}, Analyzers: {1}, Persisters: {2}, Reporters: {3}", discoverWorkers.Count, analyzerWorkers.Count, persistWorkers.Count, reportWorkers.Count);

                using (_cts.Token.Register(() => _channelProvider.SetCompleteAll()))
                {

                    try
                    {
                        await _channelProvider.WaitToCompletePipelineAsync(
                            discoverWorkers,
                            analyzerWorkers,
                            persistWorkers,
                            reportWorkers,
                            CompleteIngestionDataCollector).ConfigureAwait(false);

                        externalToken.ThrowIfAgentJobStopped();
                    }
                    catch (AgentJobStopException)
                    {
                        _logger.Info("DataSync pipeline stopped during worker coordination.");
                        _cts.Cancel();
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("An error occurred while workers performing data synchronization job. Ex: {0}", ex);
                        _cts.Cancel();
                        throw;
                    }
                    finally
                    {
                        if (!externalToken.IsCancellationRequested)
                        {
                            try
                            {
                                await _channelProvider.WaitForAllReadersCompletedAsync().ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                _logger.Warn("WaitForAllReadersCompletedAsync encountered an error. Error: {0}", ex.Message);
                            }
                        }
                        _logger.Info("All workers completed for data synchronization job.");
                    }
                }
            }
        }

        private void CompleteIngestionDataCollector()
        {
            using (new AgentPerformanceScope("DataSyncStrategy.CompleteIngestionDataCollector", addToStatistics: true))
            {
                try
                {
                    _logger.Info("Start completing ingestion data collector.");
                    foreach (var collector in rMDataIngestionDataCollectors)
                    {
                        collector.Complete();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("An error occurred while completing ingestion data collector. Ex: {0}", ex);
                }
            }
        }

        private List<Task> ExecuteWorkers<IFSDataWorker>(int workerCount, Func<IFSDataWorker> factory, Func<IFSDataWorker, Task> runAsync)
        {
            if (workerCount <= 0)
            {
                workerCount = 1;
            }
            var tasks = new List<Task>(workerCount);
            for (int i = 0; i < workerCount; i++)
            {
                var worker = factory();
                tasks.Add(Task.Run(() => runAsync(worker)));
            }
            return tasks;
        }
    }
}
