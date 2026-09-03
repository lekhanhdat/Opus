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
using AvePoint.GCommon.Utility;
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.FileSystem;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Stubs;
using AvePoint.RA.FileSystem.Utils;
using RAFileSystem.FileSystem.Common;
using RAFileSystem.FileSystem.DataIngestion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.DataSync.V2
{
    internal class FSDataCollectorV2 : IScheduleJobWorker
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private FSDataSyncChannelProvider _channelProvider;
        private RMDataIngestionDataCollector _ingestionDataCollector;
        private RMDataIngestionExecutionResultCollector _ingestionExecutionResultCollector;
        private RMDataIngestionPersistor _ingestionPersistor;
        private CancellationTokenSource _cts;

        internal static NodeLevel ClassificationLevel = 0;
        internal static FileSystemUniqueIdDto UniqueIdSetting;

        public void Bind(string msgStr)
        {
            try
            {
                JobContext.Current.EnableFSHighPerformanceMode = true;
                _cts = new CancellationTokenSource();
                _channelProvider = new FSDataSyncChannelProvider();
                FSJobMessage msg = SerializerHelper.DeserializeByDataContractSerializer<FSJobMessage>(msgStr);
                JobContext.Current.JobMessage = msgStr;
                JobContext.Current.EnableFSHighPerformanceMode = true;
                ClassificationLevel = (NodeLevel)msg.ClassificationLevel;
                UniqueIdSetting = JobContext.Current.ApiClient.GetUniqueIdSetting();
                logger.Info("Init classification level:{0}", ClassificationLevel);
                AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto node = DtoConverter.ConvertGlobalDto2FSTreeNodeDto(msg.FSTreeNodes[0]);  //for now, the sub job can only process one connection.
                System.Tuple<AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto, AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto, AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto> top3Nodes = ExternalUtil.FindTop3LevelNodes(DtoConverter.ConvertGlobalDto2FSTreeNodeDto(msg.FSTreeNodes[0]));
                string path = top3Nodes.Item3.FullPath;
                logger.Debug("The root location is {0}", top3Nodes?.Item3?.ID);
                FSJobCache.Instance.RootPath = path.TrimEnd('\\');
                FSJobCache.Instance.RecordOwner = msg.RecordOwner;
                FSJobCache.Instance.AveConnectionId = new Guid(top3Nodes.Item3.ID);
                JobContext.Current.BulkImportEnabled = msg.BulkImportEnabled;
                JobContext.Current.BulkSize = msg.BulkSize;
                FSJobCache.Instance.classCodeInfoDtoOnNode = msg.ClassCodeDto;
                IXSystem _system = ExternalUtil.OpenXSystem(FSJobCache.Instance.RootPath);
                _channelProvider.WriteBatchToAnalyzerAsync(new List<Stub>
                {
                    new FSConnectionGroupsStub { FullPath = top3Nodes.Item1.Name, SelfId = new Guid(top3Nodes.Item1.ID) },
                    new FSConnectionGroupStub { FullPath = top3Nodes.Item2.Name, SelfId = new Guid(top3Nodes.Item2.ID), ParentId = new Guid(top3Nodes.Item1.ID) }
                }, _cts.Token);
                FSJobCache.Instance.RunJobScopePath = node.FullPath;
                string highName = node.FullPath.Substring(path.Length).Trim('\\');
                StorageInfo dirInfo = new StorageInfo() { HighName = highName };
                Guid settingScopeId = QueryScopeTermIdSetting(node);
                var setting = FSJobCache.Instance.ScopeSettingCache[settingScopeId];
                if (_system.DirectoryExists(dirInfo))
                {
                    XDirectoryInfo dir = _system.OpenDirectory(dirInfo, FileMode.Open);
                    Guid parentId = string.IsNullOrEmpty(dirInfo.HighName) ?
                        new Guid(top3Nodes.Item2.ID)  //level3  connection
                        : ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, Path.GetDirectoryName(ExternalUtil.CombinePath(dir.HighName, dir.LowName))).ToLowerInvariant().ToMd5();  //sub folder
                    string fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, dir.HighName, dir.LowName);
                    string termName = FSJobCache.Instance.Terms.ContainsKey(setting.DefaultTermId) ? FSJobCache.Instance.Terms[setting.DefaultTermId].Name : null;
                    Stub rootStub = new FSFolderStub() { MediaObj = dir, FullPath = fullPath, SelfId = fullPath.ToLowerInvariant().ToMd5(), ParentId = parentId, ScopeSettingId = settingScopeId, TermId4Folder = GetTermId4Folder(msg, setting), TermName4Folder = termName };
                    rootStub.Depth = 0;
                    _channelProvider.WriteToDiscoverAsync(rootStub, _cts.Token);
                    _channelProvider.IncreaseDiscoveryCount();
                    FSJobCache.Instance.JobController.InitJob(setting, rootStub.FullPath.ToLowerInvariant().ToMd5(), rootStub.FullPath, msg, dir.Name);
                    JobContext.Current.mProgressManager.Create().IncreaseBase(3);
                    _ingestionPersistor = new RMDataIngestionPersistor(JobContext.Current.JobId);
                    _ingestionExecutionResultCollector = new RMDataIngestionExecutionResultCollector(JobContext.Current.JobId, RMDataIngestionOperationType.FileSystemDataSync, _ingestionPersistor);
                    _ingestionDataCollector = new RMDataIngestionDataCollector(_ingestionExecutionResultCollector, _ingestionPersistor);
                }
                else
                {
                    JobContext.Current.JobDetailManager.Create().Commit(new FSDataSyncJobReportDetailV2()
                    {
                        AgentName = OSInformation.HostName,
                        ObjectName = Path.GetFileName(node.FullPath),
                        FullPath = node.FullPath,
                        Status = JobDetailsStatus.Failed,
                        Comment = "RM_JS_JMD_FS_PathCanNotAccess",
                        Depth = 0,
                        DirPath = node.FullPath
                    });
                    JobContext.Current.HasErrorNode = true;
                    FSJobCache.Instance.FailedCount++;
                    throw new FileNotFoundException("We cannot open the Dir" + node.FullPath);
                }
            }
            catch (Exception ex)
            {
                _cts.Cancel();
                _channelProvider.SetCompleteAll();
                logger.Error("Failed to initialize the file system from the tree node dto.  Exception:{0}", ex.ToString());
                throw;
            }
        }

        public void Run()
        {
            logger.Info("Start FS data synchronization job.");
            FetchLastSyncFailedItemIds();
            CoordinateDataSyncWorkersAsync().GetAwaiter().GetResult();
            ProcessSyncFailedItems();
            CleanupJobContext();
            NotifyManagerFinalStatus();
            logger.Info("Finished FS data synchronization job.");
        }

        private async Task CoordinateDataSyncWorkersAsync()
        {
            var discoverWorkers = ExecuteWorkers(ConfigUtils.DISCOVERY_AND_ANALYZE_THREAD_COUNT, () => new FSDiscoveryWorker(_channelProvider, _cts.Token), w => w.RunAsync());
            var analyzerWorkers = ExecuteWorkers(ConfigUtils.DISCOVERY_AND_ANALYZE_THREAD_COUNT, () => new FSAnalysisWorker(_channelProvider, _cts.Token), w => w.RunAsync());
            var persistWorkers = ExecuteWorkers(ConfigUtils.PERSIST_AND_REPORT_THREAD_COUNT, () => new FSPersistWorker(_channelProvider, _ingestionDataCollector, _ingestionExecutionResultCollector, _cts.Token), w => w.RunAsync());
            var reportWorkers = ExecuteWorkers(ConfigUtils.PERSIST_AND_REPORT_THREAD_COUNT, () => new FSReportWorker(_channelProvider, _ingestionExecutionResultCollector, _cts.Token), w => w.RunAsync());
            logger.Info("All workers started for data synchronization job. Discoverers: {0}, Analyzers: {1}, Persisters: {2}, Reporters: {3}",
                discoverWorkers.Count, analyzerWorkers.Count, persistWorkers.Count, reportWorkers.Count);
            
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
                }
                catch (Exception ex)
                {
                    logger.Error("An error occurred while workers performing data synchronization job. Ex: {0}", ex);
                    _cts.Cancel();
                    throw;
                }
                finally
                {
                    await _channelProvider.WaitForAllReadersCompletedAsync().ConfigureAwait(false);
                    logger.Info("All workers completed for data synchronization job.");
                }
            }
        }

        private void CompleteIngestionDataCollector()
        {
            try
            {
                logger.Info("Start completing ingestion data collector.");
                _ingestionDataCollector.Complete();
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while completing ingestion data collector. Ex: {0}", ex);
            }
        }

        private List<Task> ExecuteWorkers<IFSDataWorker>(int workerCount, Func<IFSDataWorker> factory, Func<IFSDataWorker, Task> runAsync)
        {
            if (workerCount <= 0) workerCount = 1;
            var tasks = new List<Task>(workerCount);
            for (int i = 0; i < workerCount; i++)
            {
                var worker = factory();
                tasks.Add(Task.Run(() => runAsync(worker)));
            }
            return tasks;
        }

        private void ProcessSyncFailedItems()
        {
            RemoveFailedItemFromAzure();
            var failedCount = FSJobCache.Instance.FailedItems.Count;
            var throttling = FSJobCache.Instance.FailedItemThrottling;

            if (failedCount > throttling - 1)
            {
                JobContext.Current.HasErrorNode = true;
                logger.Warn($"Has more than {throttling} failed item in job, will not update last job time.");
                return;
            }

            if (failedCount > 0)
            {
                AddFailedItemsToAzure();
                JobContext.Current.HasErrorNode = true;
            }

            FSJobCache.Instance.JobController.StoreJobTime();
        }

        private void CleanupJobContext()
        {
            try
            {
                JobContext.Current.Cleanup();
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while cleaning up. Error: " + e);
                FSJobCache.Instance.FailedCount++;
                JobContext.Current.HasErrorNode = true;
            }
        }

        private void NotifyManagerFinalStatus()
        {
            var context = JobContext.Current;
            var cache = FSJobCache.Instance;

            int status = CalculateFinalJobStatus(
                context.AllErrorNode,
                context.HasErrorNode,
                cache.SuccessCount,
                cache.FailedCount
            );

            context.JobSummaryService.NotifyManager(status, context.JobId);
        }

        private int CalculateFinalJobStatus(bool allError, bool hasError, int success, int failed)
        {
            if (allError)
                return (int)JobStatus.Failed;

            if (hasError)
                return (int)JobStatus.FinishWithException;

            if (success > 0 && failed > 0)
                return (int)JobStatus.FinishWithException;

            if (success == 0 && failed > 0)
                return (int)JobStatus.Failed;

            return (int)JobStatus.Finished;
        }

        private void FetchLastSyncFailedItemIds()
        {
            long sortTicks = 0;
            int pageSize = ExternalUtil.TransferDataCount;
            string scopeId = FSJobCache.Instance.RunJobScopePath.ToLowerInvariant().ToMd5().ToString();
            HashSet<Guid> failedItemIds = new HashSet<Guid>();
            try
            {
                do
                {
                    List<RMAgentSyncFailureItem> data = new List<RMAgentSyncFailureItem>();
                    using (new AgentPerformanceScope("FSDicover.FindSyncFailedItems", addToStatistics: true))
                    {
                        data = JobContext.Current.ApiClient.FindSyncFailedItems((int)SourceFlag.FileSystem, scopeId, sortTicks, pageSize);
                    }
                    if (data != null && data.Count > 0)
                    {
                        FSJobCache.Instance.LastJobFailedItems.AddRange(data);
                        var itemIds = data.Select(d => d.NodeId).ToList();
                        foreach (var id in itemIds)
                        {
                            Guid tempId;
                            if (Guid.TryParse(id, out tempId))
                            {
                                if (!failedItemIds.Contains(tempId))
                                {
                                    failedItemIds.Add(tempId);
                                }
                            }
                        }
                    }
                    if (data == null || data.Count < ExternalUtil.TransferDataCount)
                    {
                        break;
                    }
                    sortTicks = data[data.Count - 1].SortTicks;
                } while (true);

                FSJobCache.Instance.LastJobFailedItemIds = failedItemIds.ToList();
                logger.Info($"Get failed items in last job, count:{failedItemIds.Count}");
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while FindSyncFailedItems, error:{e.ToString()}");
            }
        }

        private void RemoveFailedItemFromAzure()
        {
            try
            {
                var notExistItemIds = GetNotExistItemIds();
                notExistItemIds.UnionWith(FSJobCache.Instance.SuccessItemIdsInLastJobFailedItems);
                var successItems = FSJobCache.Instance.LastJobFailedItems.Where(i => notExistItemIds.Contains(new Guid(i.NodeId))).ToList();
                if (successItems.Count > 0)
                {
                    for (int i = 0; i < successItems.Count; i += ExternalUtil.TransferDataCount)
                    {
                        var temp = successItems.Skip(i).Take(ExternalUtil.TransferDataCount).ToList();
                        using (new AgentPerformanceScope("FSDicover.RemoveSuccessItemsInAzure", addToStatistics: true))
                        {
                            JobContext.Current.ApiClient.RemoveSuccessItemsInAzure(temp);
                        }
                    }
                }
                logger.Info($"Remove success items in azure, count:{successItems.Count}");
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while RemoveFailedItemInAzure, error:{e.ToString()}");
            }
        }

        private HashSet<Guid> GetNotExistItemIds()
        {
            var result = new HashSet<Guid>();
            try
            {
                var system = ExternalUtil.OpenXSystem(FSJobCache.Instance.RootPath);
                var items = FSJobCache.Instance.LastJobFailedItems;
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];

                    if (!system.FileExists(new StorageInfo("", item.URL)))
                    {
                        if (Guid.TryParse(item.NodeId, out Guid nodeId))
                        {
                            result.Add(nodeId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while getting not exist items. Error: {ex}");
            }
            return result;
        }

        private void AddFailedItemsToAzure()
        {
            try
            {
                for (int i = 0; i < FSJobCache.Instance.FailedItems.Count; i += ExternalUtil.TransferDataCount)
                {
                    var temp = FSJobCache.Instance.FailedItems.Skip(i).Take(ExternalUtil.TransferDataCount).ToList();
                    using (new AgentPerformanceScope("FSDicover.AddSyncFailedItems", addToStatistics: true))
                    {
                        JobContext.Current.ApiClient.AddSyncFailedItems(temp);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while AddSyncFailedItems, error:{e.ToString()}");
            }
        }

        private Guid GetTermId4Folder(FSJobMessage msg, FSSettingDto setting)
        {
            Guid folderTermId = Guid.Empty;
            if (setting.NeedCheckDefaultValue || string.IsNullOrWhiteSpace(msg.FolderTermId))
            {
                folderTermId = setting.DefaultTermId;
            }
            else
            {
                if (Guid.TryParse(msg.FolderTermId, out Guid termid))
                {
                    folderTermId = termid;
                }
                else
                {
                    folderTermId = setting.DefaultTermId;
                }
            }
            logger.Info($"Get folder termId:{folderTermId}");
            return folderTermId;
        }

        private static Guid QueryScopeTermIdSetting(AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto node)
        {
            Guid scopeId = node.Level == NodeLevel.FSFolder ? node.FullPath.ToLowerInvariant().ToMd5() : new Guid(node.ID);
            if (FSJobCache.Instance.ScopeSettingCache.ContainsKey(scopeId)) return scopeId;
            if (node.Parent != null) return QueryScopeTermIdSetting(node.Parent);
            return Guid.Empty;
        }
    }
}
