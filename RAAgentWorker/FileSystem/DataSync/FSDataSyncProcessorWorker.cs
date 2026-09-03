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
using System;
using System.Collections.Generic;
using System.Linq;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility.PerformanceScope;
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.FileSystem;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.DataSync.V2;
using AvePoint.RA.FileSystem.Utils;
using RAFileSystem.FileSystem.BaseProcessor;
using RAFileSystem.FileSystem.Common;
using RAFileSystem.FileSystem.DataSync.Utils;
using FSTreeNodeDto = AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto;

namespace RAFileSystem.FileSystem.DataSync
{
    internal sealed class FSDataSyncProcessorWorker : FSProcessorWorkerBase
    {
        private readonly AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FSDataSyncProcessorWorker(IFSExecutionStrategy strategy)
            : base(strategy)
        {
        }

        protected override FSJobProcessorContext CreateContext(
            FSJobMessage message,
            FSTreeNodeDto node,
            Tuple<FSTreeNodeDto, FSTreeNodeDto, FSTreeNodeDto> top3Nodes,
            string rootPath,
            IXSystem system,
            StorageInfo directoryInfo,
            Guid settingScopeId,
            FSSettingDto setting,
            NodeLevel classificationLevel)
        {
            return new FSDataSyncJobContext(message, node, top3Nodes, rootPath, system, directoryInfo, settingScopeId, setting, classificationLevel);
        }

        protected override void BeforeExecute()
        {
            using (new AgentPerformanceScope("FSDataSyncProcessor.BeforeExecute", addToStatistics: true))
            {
                FetchLastSyncFailedItemIds();
            }
        }

        protected override void AfterExecute()
        {
            using (new AgentPerformanceScope("FSDataSyncProcessor.AfterExecute", addToStatistics: true))
            {
                FSJobCacheV2.Instance.SyncBackToFSJobCache();
                ProcessSyncFailedItems();
            }
        }

        protected override string GetJobStartMessage()
        {
            return "Start FS data synchronization job.";
        }

        protected override string GetJobFinishMessage()
        {
            return "Finished FS data synchronization job.";
        }

        private void ProcessSyncFailedItems()
        {
            using (new AgentPerformanceScope("FSDataSyncProcessor.ProcessSyncFailedItems", addToStatistics: true))
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
        }

        private void FetchLastSyncFailedItemIds()
        {
            using (new AgentPerformanceScope("FSDataSyncProcessor.FetchLastSyncFailedItemIds", addToStatistics: true))
            {
                long sortTicks = 0;
                var pageSize = ExternalUtil.TransferDataCount;
                var scopeId = FSJobCache.Instance.RunJobScopePath.ToLowerInvariant().ToMd5().ToString();
                var failedItemIds = new List<Guid>();
                try
                {
                    do
                    {
                        var data = new List<RMAgentSyncFailureItem>();
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
                            if (Guid.TryParse(id, out Guid tempId) && !failedItemIds.Contains(tempId))
                            {
                                failedItemIds.Add(tempId);
                            }
                        }
                    }

                    if (data == null || data.Count < ExternalUtil.TransferDataCount)
                    {
                        break;
                    }

                    sortTicks = data[data.Count - 1].SortTicks;
                } while (true);

                FSJobCache.Instance.LastJobFailedItemIds = failedItemIds;
                logger.Info($"Get failed items in last job, count:{failedItemIds.Count}");
                }
                catch (Exception ex)
                {
                    logger.Error($"An error occurred while FindSyncFailedItems, error:{ex}");
                }
            }
        }

        private void RemoveFailedItemFromAzure()
        {
            using (new AgentPerformanceScope("FSDataSyncProcessor.RemoveFailedItemFromAzure", addToStatistics: true))
            {
                try
                {
                    var notExistItemIds = GetNotExistItemIds();
                    notExistItemIds = notExistItemIds.Concat(FSJobCache.Instance.SuccessItemIdsInLastJobFailedItems).ToList();
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
                catch (Exception ex)
                {
                    logger.Error($"An error occurred while RemoveFailedItemInAzure, error:{ex}");
                }
            }
        }

        private List<Guid> GetNotExistItemIds()
        {
            using (new AgentPerformanceScope("FSDataSyncProcessor.GetNotExistItemIds", addToStatistics: true))
            {
                var result = new HashSet<Guid>();
                try
                {
                    var system = ExternalUtil.OpenXSystem(FSJobCache.Instance.RootPath);
                    var items = FSJobCache.Instance.LastJobFailedItems;
                    for (int i = 0; i < items.Count; i++)
                    {
                        var item = items[i];
                        if (!system.FileExists(new StorageInfo("", item.URL)) && Guid.TryParse(item.NodeId, out Guid nodeId))
                        {
                            result.Add(nodeId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"An error occurred while getting not exist items. Error: {ex}");
                }

                return new List<Guid>(result);
            }
        }

        private void AddFailedItemsToAzure()
        {
            using (new AgentPerformanceScope("FSDataSyncProcessor.AddFailedItemsToAzure", addToStatistics: true))
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
                catch (Exception ex)
                {
                    logger.Error($"An error occurred while AddSyncFailedItems, error:{ex}");
                }
            }
        }
    }
}
