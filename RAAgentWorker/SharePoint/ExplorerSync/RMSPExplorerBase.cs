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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.ExplorerSync.Cache;
using AvePoint.RA.SharePoint.SPObjDiscover;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.SharePoint.ExplorerSync.Utils;
using AvePoint.RA.SharePoint.ExplorerSync.Modes;
using System.Collections;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Common.Global.Throttle;
using AvePoint.RA.Contract.Services;
using AvePoint.Hybrid.Utility.Util;
using AvePoint.RA.SharePoint.Common.Threads;
using AvePoint.Hybrid.AgentContract.Rule;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.FileSystem.Utils;
using System.Net;
using AvePoint.GCommon.Utility;
using RAFileSystem.Utils;
using AvePoint.GCommon;

namespace AvePoint.RA.SharePoint.ExplorerSync
{
    public class RMSPExplorerBase : RMSPDiscoverBase
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(RMSPExplorerBase));
        private ISPDiscover mDiscover = null;
        private long mLastJobTicks = DateTime.MinValue.Ticks;
        private long mMainJobTicks = DateTime.MinValue.Ticks;
        private SPDiscoverType mDiscoverType = SPDiscoverType.Full;
        //private RMSPDashboardWorker dashboardWorker = null;
        private RMSPExplorerSiteLevelCache _siteCache = null;
        private RMSPSyncItem syncItem = null;
        private static CallLimiter _spoCallLimiter;
        private static CallLimiter _cosmosCallLimiter;
        private static int _itemsPerTask = 400;  //Threshold降到2000， 这里降到400
        private MemoryListCacheService<ReportDto> mTermChangedDataCache;
        private MemoryListCacheService<AvePoint.RA.Contract.Global.Object.DeleteItemDto> mDeleteItemCache;
        public IProgressService ProgressService { get; set; }
        public IReportService<JMJobDetails> JobDetailService { get; set; }
        protected int itemsPerTask
        {
            get
            {
                return _itemsPerTask;
            }
        }

        private List<Guid> unSuccessList = new List<Guid>();
        private object unSuccessListLock = new object();
        private string containerId = string.Empty;
        private List<Contract.Global.Object.NodeFlag> listNodeFlags = null;
        //private MemoryListCacheService<RecordDto> NeedSyncDataCache;

        public RMSPExplorerBase(AveDiscoverSite discoverSite, SPTreeNodeDto treeNode)
            : base(discoverSite, treeNode)
        {
            ProgressService = JobContext.Current.mProgressManager.Create();
            JobDetailService = JobContext.Current.JobDetailManager.Create();
            listNodeFlags = new List<Contract.Global.Object.NodeFlag>();
            var siteId = DiscoverSite.SiteID.ToString();
            _siteCache = RMSPExplorerDataCache.Instance.SiteLevelCache[siteId];
            syncItem = new RMSPSyncItem(_siteCache);
            var numSetting = System.Configuration.ConfigurationManager.AppSettings["SPOSyncDataItemsPerTask"];
            if (!string.IsNullOrEmpty(numSetting))
            {
                int.TryParse(numSetting, out _itemsPerTask);
            }
            //spo call limit
            var spoCallLimitPerSecond = 50;
            var spoCallLimitPerSecondStr = System.Configuration.ConfigurationManager.AppSettings["SPOSyncDataCallLimitPerSecond"];
            if (!string.IsNullOrEmpty(spoCallLimitPerSecondStr))
            {
                int.TryParse(spoCallLimitPerSecondStr, out spoCallLimitPerSecond);
            }
            _spoCallLimiter = CallLimiterFactory.CreateInstance("SPOCalllimiter", spoCallLimitPerSecond);

            //cosmos call limit
            var cosmosCallLimitPerSecond = 20;
            var cosmosCallLimitPerSecondStr = System.Configuration.ConfigurationManager.AppSettings["CosmosSyncDataCallLimitPerSecond"];
            if (!string.IsNullOrEmpty(cosmosCallLimitPerSecondStr))
            {
                int.TryParse(cosmosCallLimitPerSecondStr, out cosmosCallLimitPerSecond);
            }
            _cosmosCallLimiter = CallLimiterFactory.CreateInstance("CosmosCallLimiter", cosmosCallLimitPerSecond);
            //dashboardWorker = new RMSPDashboardWorker();
            containerId = SPTreeNodeManagement.GetGroupNode(treeNode).ID;
            mTermChangedDataCache = new MemoryListCacheService<ReportDto>();
            mDeleteItemCache = new MemoryListCacheService<AvePoint.RA.Contract.Global.Object.DeleteItemDto>();
        }

        public void Init(ISPDiscover sPDiscover, SPDiscoverType discoverType, long lastJobTicks, long mainJobTicks)
        {
            mDiscover = sPDiscover;
            mLastJobTicks = lastJobTicks;
            mMainJobTicks = mainJobTicks;
            mDiscoverType = discoverType;
        }

        public override void RunNow()
        {
            try
            {
                //using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.RunNow"))
                using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.RunNow", addToStatistics: true))
                {
                    //using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        string disposalAction = string.Empty;
                        ThrowUtil.ThrowIfNull(DiscoverSite, $"Discover Site is null:{TreeNode?.FullPath}");

                        var aveSite = DiscoverSite.Site;
                        var termInfo = GetTermInfo(aveSite.RootWeb.Properties);
                        RMRuleItemCollection rules = null;
                        SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
                        if (RMSPExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                        {
                            var newRuleCollection = RebuldSPRules(rules);
                            if (newRuleCollection.Rules.Count == 0)
                            {
                                logger.Info($"No SP rules realted to the site {aveSite.RootWeb.ServerRelativeUrl.LogBase64()}");
                            }
                            else
                            {
                                var filterEnginer = new RMSPRuleChecker(newRuleCollection);
                                itemRuleInfo = filterEnginer.CheckDisposalRule(aveSite);

                            }

                        }
                        itemRuleInfo.TermInfo = termInfo;
                        syncItem.InitTimeZone(aveSite.RootWeb.RegionalSettings.TimeZone);
                        var item = syncItem.AssembleRecord(DiscoverSite, itemRuleInfo);
                        SyncItemToDB(item);

                        var webs = mDiscover.GetWebs(DiscoverSite);
                        ProgressService.IncreaseBase(webs.LongCount());
                        foreach (var web in webs)
                        {
                            using (web)
                            {
                                ProcessWeb(web, itemRuleInfo);
                            }
                        }
                        AddSiteScope(item);
                    }
                }

            }
            //catch (JobStopException)
            //{
            //    throw new JobStopException("the job has stopped.");
            //}
            catch (Exception e)
            {
                logger.Error($"error occurred while Process Site:{TreeNode?.FullPath.LogBase64()}, ERROR:{e.ToString()}");
                JobContext.Current.HasErrorNode = true;
                //_siteCache.HasErrorNode = true;
                JobDetailService.Commit(new Contract.Global.RMWeb.JobMonitor.JMCollectionDataJobDetails()
                {
                    ObjectName = TreeNode?.Name,
                    FullPath = TreeNode?.FullPath,
                    Status = JobDetailsStatus.Failed,
                    Comment = GetExceptionMessage(e),
                    AgentName = OSInformation.HostName
                });
            }
            finally
            {
                SendListNodeFlag();
                FinalAddDeleteItemToCache();
            }
        }
        private void SendListNodeFlag()
        {
            if (listNodeFlags.Count > 0)
            {
                for (int i = 0; i < listNodeFlags.Count; i += 100)
                {
                    try
                    {
                        var nodeFlags = listNodeFlags.Skip(i).Take(100).ToList();
                        using (var performance = new AgentPerformanceScope("RMSPExplorerBase.UpdateAutoJobCollectionTime", $"RMSPExplorerBase.UpdateAutoJobCollectionTime.Count:{nodeFlags.Count}", true))
                        {
                            HybridApiClient.Instance.UpdateAutoJobCollectionTime(nodeFlags);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("An error occurred while sending list node flags. Error:{0}", e.ToString());
                    }
                }
            }
        }

        public virtual void ProcessWeb(AveDiscoverWeb discoverWeb, SyncItemRuleInfo parentItemRule)
        {
            try
            {
                //using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.ProcessWeb"))
                using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.ProcessWeb", addToStatistics: true))
                {
                    logger.Info($"Process web:{discoverWeb?.FullUrl.LogBase64()}");
                    //using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        string disposalAction = string.Empty;

                        if (discoverWeb.ChangeType == Wrapper.Common.ChangeType.Delete)
                        {
                            logger.Info("remove web object {0} : {1}", DiscoverSite.SiteID, discoverWeb.WebID);
                            RemoveSPObj(discoverWeb.WebID);
                            return;
                        }
                        var aveWeb = discoverWeb.AveWeb;
                        var termInfo = GetTermInfo(aveWeb.Properties);
                        RMRuleItemCollection rules = null;
                        SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
                        if (RMSPExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                        {
                            var newRuleCollection = RebuldSPRules(rules);
                            if (newRuleCollection.Rules.Count == 0)
                            {
                                logger.Info($"No SP rules realted to the web {aveWeb.ServerRelativeUrl.LogBase64()}");
                            }
                            else
                            {
                                var filterEnginer = new RMSPRuleChecker(newRuleCollection);
                                itemRuleInfo = filterEnginer.CheckDisposalRule(discoverWeb, parentItemRule);
                            }
                        }
                        itemRuleInfo.TermInfo = termInfo;
                        var item = syncItem.AssembleRecord(discoverWeb, itemRuleInfo);
                        SyncItemToDB(item);
                        var lists = mDiscover.GetLists(discoverWeb);
                        logger.Info("Discover list finished. Web Url:{0} Lists:{1}", discoverWeb.FullUrl.LogBase64(), string.Join(",", lists.Select(l => l.RootFolderUrl)).LogBase64());
                        ProgressService.IncreaseBase(lists.LongCount());
                        foreach (var list in lists)
                        {
                            using (list)
                            {
                                ProcessList(discoverWeb, list, itemRuleInfo, discoverWeb.WebID);
                            }
                        }
                    }
                }


            }
            //catch (JobStopException)
            //{
            //    throw new JobStopException("This Job is stopped.");
            //}
            catch (Exception e)
            {
                logger.Error($"error occurred while Process web:{discoverWeb?.FullUrl.LogBase64()}, ERROR:{e.ToString()}");
                JobContext.Current.HasErrorNode = true;
                _siteCache.HasErrorNode = true;
                JobDetailService.Commit(new Contract.Global.RMWeb.JobMonitor.JMCollectionDataJobDetails()
                {
                    ObjectName = discoverWeb?.Title,
                    FullPath = discoverWeb?.FullUrl,
                    Status = JobDetailsStatus.Failed,
                    Comment = GetExceptionMessage(e),
                    AgentName = OSInformation.HostName
                });
            }
            finally
            {
                ProgressService.Increase();
            }

        }
        public virtual void ProcessList(AveDiscoverWeb discoverWeb, AveDiscoverList discoverList, SyncItemRuleInfo parentItemRule, Guid webId)
        {
            string listPath = string.Empty;
            try
            {
                //using (var performance = new AgentPerformanceScope($"RMSPExplorerProcessor.ProcessList:{discoverList?.RootFolderUrl}"))
                using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.ProcessList", $"RMSPExplorerProcessor.ProcessList:{discoverList?.RootFolderUrl}", true))
                {
                    string disposalAction = string.Empty;
                    logger.Info($"Process list:{discoverList?.RootFolderUrl.LogBase64()}");
                    // using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {


                        if (discoverList.ChangeType == Wrapper.Common.ChangeType.Delete)
                        {
                            logger.Info("remove list object {0} : {1}", discoverWeb, discoverList.ListId);
                            RemoveSPObj(discoverList.ListId);
                            return;
                        }

                        if (NeedSkipList(discoverList))
                        {
                            return;
                        }
                        var list = discoverList.GetListObject();

                        if (!HasBCSColumn(list))
                        {
                            logger.Warn($"list does not have bcs column, list:{discoverList?.RootFolderUrl.LogBase64()}, column name:{_siteCache.BCSColumnInternalName.LogBase64()}");
                            return;
                        }

                        listPath = AvePoint.RA.Common.Global.Util.WebUtil.MakeFullUrl(list.ParentWeb.Url, list.RootFolder.Url);
                        var termInfo = GetTermInfo(list.RootFolder.Properties);
                        RMRuleItemCollection rules = null;
                        SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
                        if (RMSPExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                        {
                            var newRuleCollection = RebuldSPRules(rules);
                            if (newRuleCollection.Rules.Count == 0)
                            {
                                logger.Info($"No SP rules realted to the list {list.RootFolder.ServerRelativeUrl.LogBase64()}");
                            }
                            else
                            {
                                var filterEnginer = new RMSPRuleChecker(newRuleCollection);
                                itemRuleInfo = filterEnginer.CheckDisposalRule(discoverList, list, parentItemRule);

                            }

                        }
                        itemRuleInfo.TermInfo = termInfo;
                        var item = syncItem.AssembleRecord(discoverList, itemRuleInfo);
                        SyncItemToDB(item);

                        logger.Info($"Get items under [{list.RootFolder.ServerRelativeUrl.LogBase64()}].");
                        switch (mDiscoverType)
                        {
                            case SPDiscoverType.Full:
                                logger.Info($"Start to sync items for full discover.[{list.RootFolder.ServerRelativeUrl.LogBase64()}].");
                                int totalItemCount = SyncItemsForFullDiscover(list, parentItemRule);
                                logger.Info($"Sync items for full discover finished.[{list.RootFolder.ServerRelativeUrl.LogBase64()}]");
                                //Full job optimiz for Cardinia
                                if (totalItemCount > 10000 && !unSuccessList.Contains(list.ID)) //超过10000 Items的Library，没有失败的Item， 加入NodeFlag， 为List Incremental做准备
                                {
                                    AddListFlag(discoverList, totalItemCount);
                                }
                                break;
                            case SPDiscoverType.CAMLSearch:
                                ProcessFailedItems(list, parentItemRule);
                                logger.Info($"Start to sync items for search discover.[{list.RootFolder.ServerRelativeUrl.LogBase64()}].");
                                SyncItemsForSearchDicsover(list, parentItemRule);
                                logger.Info($"Sync items for search discover finished.[{list.RootFolder.ServerRelativeUrl.LogBase64()}]");
                                ProcessDeletedData(discoverList, list, webId);
                                break;
                            default:
                                ProcessFailedItems(list, parentItemRule);
                                logger.Info($"Get changed items under [{list.RootFolder.ServerRelativeUrl.LogBase64()}] for incremental sync job.");
                                Dictionary<string, object> changedItems = new Dictionary<string, object>();
                                using (var performance1 = new AgentPerformanceScope("RMSPExplorerProcessor.GetListChangedItems", addToStatistics: true))
                                {
                                    changedItems = discoverList.GetListChangedItems(webId, DiscoverSite.StartTime, DiscoverSite.EndTime);
                                }
                                logger.Info($"Start to sync items for incremental discover.[{list.RootFolder.ServerRelativeUrl.LogBase64()}].");
                                ProcessIncrementalChangedItems(list, changedItems, list.RootFolder.UniqueId, parentItemRule);
                                logger.Info($"Sync items for incremental discover finished.[{list.RootFolder.ServerRelativeUrl.LogBase64()}]");
                                break;
                        }
                        #region old logic
                        //if (mDiscover is SPObjDiscover.DiscoverImpl.RMSPFullDiscover)
                        //{
                        //    int totalItemCount = 0;
                        //    //Full job optimiz for Cardinia
                        //    int rowLimit = GetListViewThresholdNumber(list); // list.ParentWeb.Site.GetMaxItemsPerThrottledOperation();
                        //    bool needQueryNext = false;
                        //    AveCamlQuery query = GetQuery(list.RootFolder, rowLimit);
                        //    int startIdx = 0;
                        //    int endIdx = startIdx + rowLimit;
                        //    int maxItemId = SPCommonUtility.GetLastItemFolderId(list, list.RootFolder);
                        //    IAveListItemCollection items = null;
                        //    do
                        //    {
                        //        items = list.GetItemsForRecords(query);
                        //        //ProgressService.IncreaseBase(items.Count);
                        //        logger.Info($"Existing job process item count:[{items.Count}]");
                        //        totalItemCount += items.Count;
                        //        ProcessAveItems(items, list.RootFolder.UniqueId, parentItemRule);
                        //        startIdx = endIdx;
                        //        endIdx = startIdx + rowLimit;

                        //        needQueryNext = startIdx < maxItemId;
                        //        if (needQueryNext)
                        //        {
                        //            logger.Info($"Query for items. StartIndex:[{startIdx}] EndIndex:[{endIdx}]");
                        //            query.ViewXml = GetQueryXml(startIdx, endIdx, rowLimit);
                        //        }
                        //    }
                        //    while (needQueryNext);

                        //    if (totalItemCount > 10000 && !unSuccessList.Contains(list.ID)) //超过10000 Items的Library，没有失败的Item， 加入NodeFlag， 为List Incremental做准备
                        //    {
                        //        try
                        //        {
                        //            logger.Info($"ProcessList total item count {totalItemCount} , Large data list, add flag {discoverList?.RootFolderUrl}");
                        //            SPTreeNodeDto groupNode = SPTreeNodeManagement.GetGroupNode(TreeNode);
                        //            if (groupNode != null)
                        //            {
                        //                listNodeFlags.Add(new Contract.Global.Object.NodeFlag()
                        //                {
                        //                    NodeId = new Guid(TreeNode.SPObjectId),
                        //                    Title = discoverList.Title,
                        //                    FullPath = discoverList.RootFolderUrl,
                        //                    CollectionTime = DateTime.UtcNow.Ticks,
                        //                    GroupId = new Guid(groupNode.SPObjectId),//Debug
                        //                    ListId = discoverList.ListId,
                        //                    IsRemoved = false,
                        //                    NodeFlagType = 4
                        //                });
                        //            }
                        //            else
                        //            {
                        //                logger.Warn("Group Node is null");
                        //            }
                        //        }
                        //        catch (Exception e)
                        //        {
                        //            logger.Info($"Add list node info failed {e.ToString()}");
                        //        }
                        //    }
                        //}
                        //else
                        //{
                        //    logger.Info($"Get changed items under [{list.RootFolder.ServerRelativeUrl}] for incremental sync job.");
                        //    var changedItems = discoverList.GetListChangedItems(webId, DiscoverSite.StartTime, DiscoverSite.EndTime);
                        //    ProcessIncrementalChangedItems(list, changedItems, list.RootFolder.UniqueId, parentItemRule);
                        //}
                        #endregion
                    }
                }
            }
            //catch (JobStopException)
            //{
            //    throw new JobStopException("This Job is stopped.");
            //}
            catch (Exception e)
            {
                logger.Error($"error occurred while Process list:{discoverList?.RootFolderUrl.LogBase64()}, ERROR:{e.ToString()}");
                JobContext.Current.HasErrorNode = true;
                _siteCache.HasErrorNode = true;
                JobDetailService.Commit(new Contract.Global.RMWeb.JobMonitor.JMCollectionDataJobDetails()
                {
                    ObjectName = discoverList?.Title,
                    FullPath = listPath,
                    Status = JobDetailsStatus.Failed,
                    Comment = GetExceptionMessage(e),
                    AgentName = OSInformation.HostName
                });
            }
            finally
            {
                ProgressService.Increase();
            }
        }

        private void ProcessFailedItems(IAveList list, SyncItemRuleInfo parentItemRule)
        {
            logger.Info("Start to process failed items in azure table");
            List<RMAgentSyncFailureItem> failedItems = RMSPExplorerDataCache.Instance.LastJobFailedItems.ContainsKey(list.ID) ?
                RMSPExplorerDataCache.Instance.LastJobFailedItems[list.ID] : new List<RMAgentSyncFailureItem>();
            //SyncFailureItemDao.GetAll(TenantLocalValue.LogonGroupId, DiscoverSite.SiteID.ToString(), list.ID.ToString());
            int incItemsPerTask = failedItems.Count / 4;
            logger.Info($"Process last failed item count:[{failedItems.Count}].incItemsPerTask:[{incItemsPerTask}]");
            if (failedItems.Count > 0)
            {
                ProgressService.IncreaseBase(failedItems.Count);
                var itemIds = failedItems.Select(i => i.IntemIntId).ToList();
                for (int i = 0; i < itemIds.Count; i += 2000)
                {
                    var rowIds = itemIds.Skip(i).Take(2000).ToList();
                    IEnumerable<IAveListItem> items = GetItemsByRowIds(list, rowIds);
                    int existingItemsPerTask = items.Count() / 4;
                    CancellationTokenSource cts = null;
                    if (items.Count() > itemsPerTask)
                    {
                        cts = new CancellationTokenSource();
                        //最多起4~5个Task处理Incremental的Changed Item，Full Job Get Item默认2k，因此itemsPerTask固定，但是Incremental items 数量不固定，因此需要按照多个处理。
                        AveTenantTasks.RunParallel(items, existingItemsPerTask, cts, changedItem =>
                        {
                            ProcessFailedItemV1(list, changedItem, failedItems, parentItemRule, cts);
                        });
                    }
                    else
                    {
                        foreach (var changedItem in items)
                        {
                            ProcessFailedItemV1(list, changedItem, failedItems, parentItemRule);
                        }
                    }
                }
            }
        }
        public virtual void ProcessFailedItemV1(IAveList list, IAveListItem aveItem, List<RMAgentSyncFailureItem> failedItems, SyncItemRuleInfo parentItemRule, CancellationTokenSource cts = null)
        {
            string itemName = string.Empty;
            var failedItem = failedItems.Where(f => f.IntemIntId == aveItem.ID).FirstOrDefault();
            string itemUrl = failedItem.URL; // string.Empty;
            try
            {
                Guid parentId = new Guid(failedItem.ParentId);
                using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.ProcessFailedItem", addToStatistics: true))
                {
                    ProgressService.Increase();
                    int itemId = failedItem.IntemIntId;
                    logger.Info($"Process failed item:Id:{itemId}.");
                    itemName = aveItem?.GetObjectName();
                    itemUrl = aveItem.FullPath();
                    //using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        var termInfo = GetTermInfo(aveItem, aveItem.Fields);
                        RMRuleItemCollection rules = null;
                        SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
                        using (var performance0 = new AgentPerformanceScope("RMSPExplorerProcessor.CheckRule", addToStatistics: true))
                        {
                            if (RMSPExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                            {
                                try
                                {
                                    var newRuleCollection = RebuldSPRules(rules);
                                    if (newRuleCollection.Rules.Count == 0)
                                    {
                                        logger.Info($"No SP rules realted to the item {list.RootFolder.Url.LogBase64()}:{itemId}");
                                    }
                                    else
                                    {
                                        var filterEnginer = new RMSPRuleChecker(newRuleCollection);
                                        itemRuleInfo = filterEnginer.CheckDisposalRule(aveItem, parentItemRule);
                                    }
                                }
                                catch (Exception e)
                                {
                                    logger.Warn(e.Message, e);
                                }
                            }
                        }
                        itemRuleInfo.TermInfo = termInfo;
                        var item = syncItem.AssembleRecord(aveItem, parentId, itemRuleInfo);
                        //Record recordInDB = null;

                        //WaitCosmosExecuteAction(() =>
                        //{
                        //    recordInDB = ExplorerDao.ReadById(item.ScopeId, item.Id);
                        //});
                        //check uniqueId
                        //UpdateRecordId(item, recordInDB);
                        //item.Comment = "RM_JM_SyncFailedItemSuccess";
                        SyncItemToDB(item);
                        //this.RemoveFailureItemFromAzure(failedItem);
                    }
                }
            }
            //catch (JobStopException)
            //{
            //    cts?.Cancel();
            //    throw new JobStopException("This Job is stopped.");
            //}
            catch (Exception e)
            {
                logger.Error($"error occurred while Process aveitem:{itemUrl.LogBase64()}, ERROR:{e.ToString()}");
                bool isItemNotFound = this.isItemNotFoundError(e);
                if (!isItemNotFound)
                {
                    JobContext.Current.HasErrorNode = true;
                    JobDetailService.Commit(new Contract.Global.RMWeb.JobMonitor.JMCollectionDataJobDetails()
                    {
                        ObjectName = itemName,
                        FullPath = itemUrl,
                        Status = JobDetailsStatus.Failed,
                        Comment = GetExceptionMessage(e),
                        AgentName = OSInformation.HostName
                    });
                }
                else
                {
                    //this.RemoveFailureItemFromAzure(failedItem);
                    Guid itemId = new Guid(failedItem.ItemId);
                    if (!RMSPExplorerDataCache.Instance.SuccessSyncedFailedItemIds.Contains(itemId))
                    {
                        RMSPExplorerDataCache.Instance.SuccessSyncedFailedItemIds.Add(itemId);
                    }
                }

            }
            return;
        }

        public virtual void ProcessFailedItem(IAveList list, RMAgentSyncFailureItem failedItem, SyncItemRuleInfo parentItemRule, CancellationTokenSource cts = null)
        {
            string itemName = string.Empty;
            string itemUrl = failedItem.URL; // string.Empty;
            IAveListItem aveItem = null;
            try
            {
                Guid parentId = new Guid(failedItem.ParentId);
                using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.ProcessFailedItem", addToStatistics: true))
                {
                    ProgressService.Increase();
                    int itemId = failedItem.IntemIntId;
                    logger.Info($"Process failed item:Id:{itemId}.");

                    using (var performance0 = new AgentPerformanceScope("RMSPExplorerProcessor.GetFailedItemById", addToStatistics: true))
                    {
                        WaitSPOExecuteAction(() =>
                        {
                            aveItem = list.GetItemById(itemId);
                        });
                    }
                    itemName = aveItem?.GetObjectName();
                    itemUrl = aveItem.FullPath();
                    //using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        var termInfo = GetTermInfo(aveItem, aveItem.Fields);
                        RMRuleItemCollection rules = null;
                        SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
                        using (var performance0 = new AgentPerformanceScope("RMSPExplorerProcessor.CheckRule", addToStatistics: true))
                        {
                            if (RMSPExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                            {
                                try
                                {
                                    var newRuleCollection = RebuldSPRules(rules);
                                    if (newRuleCollection.Rules.Count == 0)
                                    {
                                        logger.Info($"No SP rules realted to the item {list.RootFolder.Url.LogBase64()}:{itemId}");
                                    }
                                    else
                                    {
                                        var filterEnginer = new RMSPRuleChecker(newRuleCollection);
                                        itemRuleInfo = filterEnginer.CheckDisposalRule(aveItem, parentItemRule);
                                    }
                                }
                                catch (Exception e)
                                {
                                    logger.Warn(e.Message, e);
                                }
                            }
                        }
                        itemRuleInfo.TermInfo = termInfo;
                        var item = syncItem.AssembleRecord(aveItem, parentId, itemRuleInfo);
                        //Record recordInDB = null;

                        //WaitCosmosExecuteAction(() =>
                        //{
                        //    recordInDB = ExplorerDao.ReadById(item.ScopeId, item.Id);
                        //});
                        //check uniqueId
                        //UpdateRecordId(item, recordInDB);
                        //item.Comment = "RM_JM_SyncFailedItemSuccess";
                        SyncItemToDB(item);
                        //this.RemoveFailureItemFromAzure(failedItem);
                    }
                }
            }
            //catch (JobStopException)
            //{
            //    cts?.Cancel();
            //    throw new JobStopException("This Job is stopped.");
            //}
            catch (Exception e)
            {
                logger.Error($"error occurred while Process aveitem:{itemUrl.LogBase64()}, ERROR:{e.ToString()}");
                bool isItemNotFound = this.isItemNotFoundError(e);
                if (!isItemNotFound)
                {
                    JobContext.Current.HasErrorNode = true;
                    JobDetailService.Commit(new Contract.Global.RMWeb.JobMonitor.JMCollectionDataJobDetails()
                    {
                        ObjectName = itemName,
                        FullPath = itemUrl,
                        Status = JobDetailsStatus.Failed,
                        Comment = GetExceptionMessage(e),
                        AgentName = OSInformation.HostName
                    });
                }
                else
                {
                    //this.RemoveFailureItemFromAzure(failedItem);
                    Guid itemId = new Guid(failedItem.ItemId);
                    if (!RMSPExplorerDataCache.Instance.SuccessSyncedFailedItemIds.Contains(itemId))
                    {
                        RMSPExplorerDataCache.Instance.SuccessSyncedFailedItemIds.Add(itemId);
                    }
                }

            }
            return;
        }

        private int SyncItemsForFullDiscover(IAveList list, SyncItemRuleInfo parentItemRule)
        {
            int totalItemCount = 0;
            int rowLimit = GetListViewThresholdNumber(list); // list.ParentWeb.Site.GetMaxItemsPerThrottledOperation();
            bool needQueryNext = false;
            AveCamlQuery query = GetQuery(list.RootFolder, rowLimit);
            int startIdx = 0;
            int endIdx = startIdx + rowLimit;
            int maxItemId = SPCommonUtility.GetLastItemFolderId(list, list.RootFolder);
            IAveListItemCollection items = null;
            do
            {
                using (var performance1 = new AgentPerformanceScope("RMSPExplorerProcessor.GetItemsForRecords", addToStatistics: true))
                {

                    items = list.GetItemsForRecords(query);
                }
                //JobContext.ReportManager.IncreaseBase(items.Count);
                logger.Info($"Existing job process item count:[{items.Count}]");
                totalItemCount += items.Count;
                ProcessAveItems(items, list.RootFolder.UniqueId, parentItemRule);
                startIdx = endIdx;
                endIdx = startIdx + rowLimit;

                needQueryNext = startIdx < maxItemId;
                if (needQueryNext)
                {
                    logger.Info($"Query for items. StartIndex:[{startIdx}] EndIndex:[{endIdx}]");
                    query.ViewXml = GetQueryXml(startIdx, endIdx, rowLimit);
                }
            }
            while (needQueryNext);
            return totalItemCount;
        }

        private void AddListFlag(AveDiscoverList discoverList, int totalItemCount)
        {
            try
            {
                logger.Info($"ProcessList total item count {totalItemCount} , Large data list, add flag {discoverList?.RootFolderUrl.LogBase64()}");
                SPTreeNodeDto groupNode = SPTreeNodeManagement.GetGroupNode(TreeNode);
                if (groupNode != null)
                {
                    listNodeFlags.Add(new Contract.Global.Object.NodeFlag()
                    {
                        NodeId = new Guid(TreeNode.SPObjectId),
                        Title = discoverList.Title,
                        FullPath = discoverList.RootFolderUrl,
                        CollectionTime = DateTime.UtcNow.Ticks,
                        GroupId = new Guid(groupNode.SPObjectId),//Debug
                        ListId = discoverList.ListId,
                        IsRemoved = false,
                        NodeFlagType = 4
                    });
                }
                else
                {
                    logger.Warn("Group Node is null");
                }
            }
            catch (Exception e)
            {
                logger.Info($"Add list node info failed {e.ToString()}");
            }
        }

        private void SyncItemsForSearchDicsover(IAveList list, SyncItemRuleInfo parentItemRule)
        {
            bool needQueryNext = false;
            int rowLimit = list.ParentWeb.Site.GetMaxItemsPerThrottledOperation();
            int maxItemId = GetLastItemId(list, list.RootFolder);

            int startIndex = 0;
            IAveListItemCollection items = null;
            DateTime startTime = DateTime.SpecifyKind(new DateTime(mLastJobTicks), DateTimeKind.Utc);
            DateTime endTime = DateTime.UtcNow;
            do
            {
                using (var queryAuto = new AgentPerformanceScope("RMSPExplorerBase.SearchQueryData", $"RMSPExplorerBase.SearchQueryData{list.RootFolder.ServerRelativeUrl} start{startIndex}", true))
                {
                    AveCamlQuery query = GetSearchDiscoverQuery(list, list.RootFolder, startTime, endTime, startIndex, startIndex + rowLimit, rowLimit);
                    //using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        items = list.GetItemsForRecords(query);
                    }
                    //JobContext.ReportManager.IncreaseBase(items.Count);
                    logger.Info($"Data sync job process folder url {list.RootFolder.ServerRelativeUrl.LogBase64()} item count:[{items.Count}], start index {startIndex}, end index {startIndex + rowLimit}");
                }
                using (var queryAuto = new AgentPerformanceScope("RMSPExplorerBase.ProcessAveItems", $"RMSPExplorerBase.ProcessAveItems{list.RootFolder.ServerRelativeUrl} count {items.Count}", true))
                {
                    ProcessAveItems(items, list.RootFolder.UniqueId, parentItemRule);
                }
                if (startIndex + rowLimit < maxItemId)
                {
                    needQueryNext = true;
                    startIndex += rowLimit;
                    logger.Info($"PagingInfo:{startIndex}");
                }
                else
                {
                    needQueryNext = false;
                }
            }
            while (needQueryNext);
        }

        private void ProcessDeletedData(AveDiscoverList discoverList, IAveList list, Guid webId)
        {
            logger.Info($"Start to process deleted data in {list.RootFolder.ServerRelativeUrl.LogBase64()}");
            try
            {
                if (mLastJobTicks != DateTime.MinValue.Ticks)
                {
                    Dictionary<string, object> changedItems = new Dictionary<string, object>();
                    using (var performance1 = new AgentPerformanceScope("RMSPExplorerProcessor.GetListChangedItems", addToStatistics: true))
                    {
                        changedItems = discoverList.GetListChangedItems(webId, new DateTime(mLastJobTicks, DateTimeKind.Utc), new DateTime(mMainJobTicks, DateTimeKind.Utc));
                    }
                    ProcessDeletedItems(list, changedItems);
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while process deleted data. Error{0}", e.ToString());
            }
            logger.Info($"Process deleted data in {list.RootFolder.ServerRelativeUrl.LogBase64()} finished.");
        }

        private void ProcessDeletedItems(IAveList list, Dictionary<string, object> changedItems)
        {
            foreach (var changeItem in changedItems)
            {
                using (var performance = new AgentPerformanceScope("RMOneDriveExplorerBase.ProcessDeletedData", addToStatistics: true))
                {
                    //using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        Dictionary<string, object> itemChangeProperties = changeItem.Value as Dictionary<string, object>;
                        int itemId = (int)itemChangeProperties["ItemId"];
                        int itemChangeType = (int)itemChangeProperties["ChangeType"];
                        var itemUniqueId = (Guid)itemChangeProperties["UniqueId"];
                        //Guid itemUniqueId = (Guid)itemChangeProperties["UniqueId"];
                        logger.Info($"Process changed item:Id:{itemId}.ChangeType:{itemChangeType}.");
                        if (itemChangeProperties.ContainsKey("Hidden") && (bool)itemChangeProperties["Hidden"])
                        {
                            logger.Info($"skip hidden item:{itemId}");
                            continue;
                        }
                        if (itemChangeType == (int)Wrapper.Common.ChangeType.Delete)
                        {
                            try
                            {
                                using (var performance1 = new AgentPerformanceScope("RMSPExplorerProcessor.GetDeletedItem", addToStatistics: true))
                                {
                                    WaitSPOExecuteAction(() =>
                                    {
                                        var aveItem = list.GetItemById(itemId);
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Info($"cannot found item object ID:{itemId}  :{ex.ToString()}");
                                var dto = new AvePoint.RA.Contract.Global.Object.DeleteItemDto()
                                {
                                    SiteId = DiscoverSite.SiteID,
                                    ListId = list.ID,
                                    ItemRowId = itemId,
                                    ItemId = itemUniqueId,
                                };
                                AddDeleteItemToCache(dto);
                                // Guid: { itemUniqueId}
                                //RemoveSPObj(itemUniqueId, itemId);
                            }
                            logger.Warn("remove view item, {0}.", itemId);
                        }
                    }
                }
            }
        }

        public AveCamlQuery GetQuery(IAveFolder folder, int rowLimit)
        {
            AveCamlQuery query = new AveCamlQuery();
            query.FolderServerRelativeUrl = folder.ServerRelativeUrl;
            query.ListItemCollectionPosition = new AveItemCollectionPosition();
            query.ViewXml = GetQueryXml(0, 0 + rowLimit, rowLimit);
            return query;
        }
        public virtual void ProcessItems(IAveList list, IEnumerable<IAveDiscoverItem> items, Guid parentId, SyncItemRuleInfo parentItemRule, List<RecordDto> dbRecords)
        {
            logger.Info($"Process item count:[{items.Count()}]");
            ProgressService.IncreaseBase(items.Count());

            if (items.Count() > itemsPerTask)
            {
                var cts = new CancellationTokenSource();
                AveTenantTasks.RunParallel(items, itemsPerTask, cts, discoverItem =>
                {
                    using (discoverItem)
                    {
                        ProcessItem(list, discoverItem, parentId, parentItemRule, cts);
                    }
                });
            }
            else
            {
                foreach (var discoverItem in items)
                {
                    using (discoverItem)
                    {
                        ProcessItem(list, discoverItem, parentId, parentItemRule);
                    }
                }
            }
        }

        public virtual void ProcessIncrementalChangedItems(IAveList list, Dictionary<string, object> changedItems, Guid parentId, SyncItemRuleInfo parentItemRule)
        {
            int incItemsPerTask = changedItems.Count() / 5;
            logger.Info($"Process incremental changed item count:[{changedItems.Count()}].incItemsPerTask:[{incItemsPerTask}]");
            //ProgressService.IncreaseBase(changedItems.Count());
            // var itemOwnerMapping = GetItemOwnerMappingForIncremental(list, changedItems);
            var changedObjects = changedItems.Values.Select(i => i as Dictionary<string, object>).Where(i => (i.ContainsKey("Hidden") && !(bool)i["Hidden"]) || !i.ContainsKey("Hidden")).ToList();
            var deleteItems = changedObjects.Where(i => (int)i["ChangeType"] == (int)Wrapper.Common.ChangeType.Delete).ToList();
            var existingItemIds = changedObjects.Where(i => (int)i["ChangeType"] != (int)Wrapper.Common.ChangeType.Delete).Select(i => (int)i["ItemId"]).ToList();
            ProgressService.IncreaseBase(changedObjects.Count);
            #region process items whose ChangeType is Delete
            int deleteItemsPerTask = deleteItems.Count / 4;
            CancellationTokenSource cts0 = null;
            if (deleteItems.Count > itemsPerTask)
            {
                cts0 = new CancellationTokenSource();
                //最多起4~5个Task处理Incremental的Changed Item，Full Job Get Item默认2k，因此itemsPerTask固定，但是Incremental items 数量不固定，因此需要按照多个处理。
                AveTenantTasks.RunParallel(deleteItems, deleteItemsPerTask, cts0, changedItem =>
                {
                    ProcessIncrementalDeleteItem(list, changedItem, cts0);
                });
            }
            else
            {
                foreach (var changedItem in deleteItems)
                {
                    ProcessIncrementalDeleteItem(list, changedItem);
                }
            }
            #endregion

            for (int i = 0; i < existingItemIds.Count; i += 2000)
            {
                var rowIds = existingItemIds.Skip(i).Take(2000).ToList();
                IEnumerable<IAveListItem> items = GetItemsByRowIds(list, rowIds);
                int existingItemsPerTask = items.Count() / 4;
                CancellationTokenSource cts = null;
                if (items.Count() > itemsPerTask)
                {
                    cts = new CancellationTokenSource();
                    //最多起4~5个Task处理Incremental的Changed Item，Full Job Get Item默认2k，因此itemsPerTask固定，但是Incremental items 数量不固定，因此需要按照多个处理。
                    AveTenantTasks.RunParallel(items, existingItemsPerTask, cts, changedItem =>
                    {
                        ProcessIncrementalChangedItemV1(list, changedItem, parentId, parentItemRule, cts);
                    });
                }
                else
                {
                    foreach (var changedItem in items)
                    {
                        ProcessIncrementalChangedItemV1(list, changedItem, parentId, parentItemRule);
                    }
                }
            }
        }

        public virtual void ProcessIncrementalDeleteItem(IAveList list, Dictionary<string, object> itemChangeProperties, CancellationTokenSource cts = null)
        {
            IAveListItem aveItem = null;
            int rowId = 0;
            try
            {
                ProgressService.Increase();
                int itemId = (int)itemChangeProperties["ItemId"];
                rowId = itemId;
                int itemChangeType = (int)itemChangeProperties["ChangeType"];
                Guid itemUniqueId = (Guid)itemChangeProperties["UniqueId"];
                logger.Info($"Process changed item:Id:{itemId}.ChangeType:{itemChangeType}.");
                if (itemChangeType == (int)Wrapper.Common.ChangeType.Delete)
                {
                    try
                    {
                        WaitSPOExecuteAction(() =>
                        {
                            aveItem = list.GetItemById(itemId);
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.Info($"cannot found item object ID:{itemId} :{ex.ToString()}");
                        var dto = new AvePoint.RA.Contract.Global.Object.DeleteItemDto()
                        {
                            SiteId = DiscoverSite.SiteID,
                            ListId = list.ID,
                            ItemRowId = itemId,
                            ItemId = itemUniqueId,
                        };
                        AddDeleteItemToCache(dto);
                        //RemoveSPObj(itemUniqueId, itemId);
                    }
                    logger.Warn("remove view item, {0}.", itemId);
                }
            }
            //catch (JobStopException)
            //{
            //    cts?.Cancel();
            //    throw new JobStopException("This Job is stopped.");
            //}
            catch (Exception e)
            {
                logger.Error($"error occurred while Process ProcessIncrementalDeleteItem:{rowId.ToString().LogBase64()}, ERROR:{e.ToString()}");
            }
        }
        [Obsolete]
        private Dictionary<Guid, string> GetItemOwnerMappingForIncremental(IAveList list, Dictionary<string, object> changedItems)
        {
            Dictionary<Guid, string> itemAndOwnerMapping = new Dictionary<Guid, string>();
            try
            {
                List<int> changedItemIds = new List<int>();
                foreach (var item in changedItems)
                {
                    var tempProperty = item.Value as Dictionary<string, object>;
                    int itemId = (int)tempProperty["ItemId"];
                    changedItemIds.Add(itemId);
                }
                var siteId = list.ParentWeb.Site.ID;
                var listId = list.ID;
                for (int i = 0; i < changedItemIds.Count; i += 100)
                {
                    var tempIds = changedItemIds.Skip(i).Take(100).ToList();
                    Dictionary<Guid, string> tempMapping = new Dictionary<Guid, string>();
                    using (var performance = new AgentPerformanceScope("RMSPExplorerBase.GetIncrementalItemOwnerMapping", $"RMSPExplorerBase.GetIncrementalItemOwnerMapping.SiteId:{siteId} ListId:{listId} ItemId Count:{tempIds.Count}", true))
                    {
                        tempMapping = HybridApiClient.Instance.GetIncrementalItemOwnerMapping(siteId, listId, tempIds);
                    }
                    foreach (var mapping in tempMapping)
                    {
                        if (!itemAndOwnerMapping.ContainsKey(mapping.Key))
                        {
                            itemAndOwnerMapping.Add(mapping.Key, mapping.Value);
                        }
                        else
                        {
                            logger.Warn("An item with the same unique id has been added. Unique Id:{0}", mapping.Key);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting item owner mapping for incremental. Error:{0}", e.ToString());
            }
            return itemAndOwnerMapping;
        }

        public virtual void ProcessAveItems(IAveListItemCollection items, Guid parentId, SyncItemRuleInfo parentItemRule)
        {
            logger.Info($"Process item count:[{items.Count}]");
            ProgressService.IncreaseBase(items.Count);

            if (items.Count > itemsPerTask)
            {
                var cts = new System.Threading.CancellationTokenSource();
                AveTenantTasks.RunParallelBatch(items, itemsPerTask, cts, item =>
                {
                    ProcessAveItemBatch(item, parentId, parentItemRule, cts);
                });
            }
            else
            {
                //foreach (var item in items)
                //{
                //    ProcessAveItem(item, parentId, parentItemRule);
                //}
                ProcessAveItemBatch(items, parentId, parentItemRule);
            }
        }
        //[Obsolete("use ProcessAveItemBatch in data sync full")]
        //public void ProcessAveItem(IAveListItem aveItem, Guid parentId, SyncItemRuleInfo parentItemRule, CancellationTokenSource cts = null)
        //{
        //    //JobContext.ReportManager.Increase();
        //    if (aveItem != null && aveItem.FileSystemObjectType == AveFileSystemObjectType.Folder)
        //    {
        //        IAveFolder folder = aveItem.Folder;
        //        if (folder != null)
        //        {
        //            ProcessAveFolder(folder, parentId, parentItemRule);
        //        }
        //        return;
        //    }
        //    string itemName = aveItem?.Name; // string.Empty;
        //    string itemUrl = aveItem?.Url; // string.Empty;
        //    Guid recordId = Guid.Empty;
        //    try
        //    {
        //        //using (var performance = new AgentPerformanceScope("SP.RMSPExplorerProcessor.ProcessAveItem"))
        //        {
        //            if (aveItem.ParentList.BaseType == AveBaseType.DocumentLibrary && NeedSkipFile(aveItem, itemName))
        //            {
        //                return;
        //            }
        //            InnerProcessAveItem(aveItem, parentId, parentItemRule, new Dictionary<Guid, string>());
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Error($"error occurred while Process aveitem:{itemUrl}, ERROR:{e.ToString()}");
        //        //JobContext.HasErrorNode = true;
        //        _siteCache.HasErrorNode = true;
        //        this.AddExceptionListDic(this.GetListId(aveItem));
        //        //JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
        //        //{
        //        //    ObjectName = itemName,
        //        //    FullPath = itemUrl,
        //        //    Status = JobDetailsStatus.Failed,
        //        //    Comment = this.GetExceptionMessage(e),
        //        //});
        //    }
        //}
        public void ProcessAveItemBatch(IEnumerable<IAveListItem> aveItems, Guid parentId, SyncItemRuleInfo parentItemRule, CancellationTokenSource cts = null)
        {
            logger.Info("Start to process items batch.");
            List<IAveListItem> folders = aveItems.Where(a => a != null && a.FileSystemObjectType == AveFileSystemObjectType.Folder).ToList();
            logger.Info("Folders count is {0}", folders.Count);
            foreach (IAveListItem item in folders)
            {
                IAveFolder folder = item.Folder;
                if (folder != null)
                {
                    try
                    {
                        ProcessAveFolder(folder, parentId, parentItemRule);
                    }
                    finally
                    {
                        ProgressService.Increase();
                    }
                }
            }
            List<IAveListItem> items = aveItems.Where(a => a != null && a.FileSystemObjectType != AveFileSystemObjectType.Folder && !(a.ParentList.BaseType == AveBaseType.DocumentLibrary && NeedSkipFile(a, a.Name))).ToList();
            logger.Info("Items count is {0}", items.Count);
            if (items.Count > 0)
            {
                //Guid siteId = DiscoverSite.SiteID;
                //List<Guid> nodeIdList = items.Select(a => a.UniqueId).ToList();
                //Dictionary<Guid, string> itemAndOwnerMapping = HybridApiClient.Instance.GetItemOwnerMapping(siteId, nodeIdList);
                //logger.Info("Manual approve mapping {0}", itemAndOwnerMapping.Count);
                //AssembleItemAndOwnerMapping(mas, nodeIdList);
                foreach (IAveListItem aveItem in items)
                {
                    string itemName = aveItem?.Name; // string.Empty;
                    string itemUrl = aveItem?.Url; // string.Empty; 
                    try
                    {
                        //using (var performance = new AgentPerformanceScope("SP.RMSPExplorerProcessor.ProcessAveItem"))
                        using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.ProcessAveItem", addToStatistics: true))
                        {
                            InnerProcessAveItem(aveItem, parentId, parentItemRule);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error($"error occurred while Process aveitem:{itemUrl}, ERROR:{e.ToString()}");
                        //JobContext.Current.HasErrorNode = true;
                        //_siteCache.HasErrorNode = true;
                        this.AddExceptionListDic(this.GetListId(aveItem));
                        JobDetailService.Commit(new Contract.Global.RMWeb.JobMonitor.JMCollectionDataJobDetails()
                        {
                            ObjectName = itemName,
                            FullPath = itemUrl,
                            Status = JobDetailsStatus.Failed,
                            Comment = this.GetExceptionMessage(e),
                            AgentName = OSInformation.HostName
                        });
                        AddFailureItem2Cache(aveItem, parentId, e);
                    }
                    finally
                    {
                        ProgressService.Increase();
                    }
                }

            }

        }
        private Dictionary<Guid, string> AssembleItemAndOwnerMapping(List<Contract.Global.Object.RMManualApprove> mas, List<Guid> allNodeIds)
        {
            Dictionary<Guid, string> tempMapping = new Dictionary<Guid, string>();
            List<Contract.Global.Object.RMManualApprove> esclates = mas.Where(a => a.WorkflowInstanceId == Guid.Empty && a.EscalateTo != null).ToList();
            foreach (Contract.Global.Object.RMManualApprove ra in esclates)
            {
                List<string> userIds = ra.EscalateTo.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                logger.Debug("Owner for node {0}, {1}, is {2}", ra.NodeId, ra.Url.LogBase64(), string.Join("|", userIds).LogBase64());
                if (!tempMapping.ContainsKey(ra.NodeId))
                {
                    tempMapping.Add(ra.NodeId, string.Join("|", userIds));
                }
            }
            List<Contract.Global.Object.RMManualApprove> workflow = mas.Where(a => a.WorkflowInstanceId != Guid.Empty).ToList();
            if (workflow.Count > 0)
            {
                Dictionary<Guid, List<string>> dictionary = HybridApiClient.Instance.GetManualNodeAndApproverMapping(DiscoverSite.SiteID, workflow.Select(a => a.NodeId).ToList());
                //RMManualApproveDao.GetManualNodeAndApproverMapping(DiscoverSite.SiteID, workflow.Select(a => a.NodeId).ToList());
                if (dictionary.Count > 0)
                {
                    List<string> tempUserIds = new List<string>();
                    foreach (var a in dictionary.Values)
                    {
                        tempUserIds.AddRange(a);
                    }
                    List<string> uniqueUserIds = tempUserIds.Where(a => a != null).Distinct().ToList();
                    List<Contract.Global.Object.RMAccount> accounts = HybridApiClient.Instance.GetUserByUserIds(uniqueUserIds);
                    foreach (KeyValuePair<Guid, List<string>> pa in dictionary)
                    {
                        if (!tempMapping.ContainsKey(pa.Key))
                        {
                            List<int> userKey = accounts.Where(a => pa.Value.Contains(a.UserId)).Select(s => s.Id).ToList();
                            logger.Debug("Owner for workflow node {0}, is {1}", pa.Key, string.Join("|", userKey).LogBase64());
                            if (userKey.Count > 0)
                            {
                                string owner = AddBeforeAndAfterSeparator(string.Join("|", userKey));
                                tempMapping.Add(pa.Key, owner);
                            }
                        }
                        else
                        {
                            logger.Warn("Node {0} has multi manual approve records", pa.Key);
                        }
                    }
                }
            }

            logger.Info("Node with manual info, count {0}", tempMapping.Count);
            foreach (Guid nodeId in allNodeIds)
            {
                if (!tempMapping.ContainsKey(nodeId))
                {
                    tempMapping.Add(nodeId, string.Empty);
                }
            }
            return tempMapping;
        }
        private string AddBeforeAndAfterSeparator(string source, string separator = "|")
        {
            if (!string.IsNullOrEmpty(source))
            {
                if (!source.StartsWith(separator))
                {
                    source = separator + source;
                }
                if (!source.EndsWith(separator))
                {
                    source = source + separator;
                }
                return source;
            }
            return string.Empty;
        }
        public void InnerProcessAveItem(IAveListItem aveItem, Guid parentId, SyncItemRuleInfo parentItemRule)
        {
            ProgressService.Increase();
            var termInfo = GetTermInfo(aveItem, aveItem.Fields);
            RMRuleItemCollection rules = null;
            SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
            var key = aveItem.DirPath();
            using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.CheckRule", addToStatistics: true))
            {
                if (!CheckParentRule(key, ref itemRuleInfo))
                {
                    if (RMSPExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                    {
                        var newRuleCollection = RebuldSPRules(rules);
                        if (newRuleCollection.Rules.Count == 0)
                        {
                            logger.Info($"No SP rules realted to the item {aveItem?.Url.LogBase64()}");
                        }
                        else
                        {
                            var filterEnginer = new RMSPRuleChecker(newRuleCollection);
                            itemRuleInfo = filterEnginer.CheckDisposalRule(aveItem, parentItemRule);
                            logger.Debug("Check rule finished. Rule id:", itemRuleInfo?.Rule?.Id);
                        }

                    }
                    else if (parentItemRule.Rule != null)
                    {
                        logger.Debug("Cannot get term rule mapping, will use parent rule. Term id:", termInfo?.UniqueId);
                        itemRuleInfo.Rule = parentItemRule.Rule;
                        itemRuleInfo.DisposalAction = parentItemRule.DisposalAction;
                    }
                }
            }
            itemRuleInfo.TermInfo = termInfo;
            //string owner = null;
            //if (itemAndOwnerMapping.ContainsKey(aveItem.UniqueId))
            //{
            //    owner = itemAndOwnerMapping[aveItem.UniqueId];
            //}
            //logger.Debug("Owner for record {0} is {1}", aveItem.UniqueId, owner);
            var item = syncItem.AssembleRecord(aveItem, parentId, itemRuleInfo);
            // RecordDto recordInDB = dbRecords.Where(r => r.ScopeId == item.ScopeId && r.Id == item.Id).FirstOrDefault();

            //set uniqueId
            // UpdateRecordId(item, recordInDB);

            SyncItemToDB(item);
        }

        public virtual void ProcessAveFolder(IAveFolder folder, Guid parentId, SyncItemRuleInfo parentItemRule)
        {
            try
            {
                //using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.ProcessAveFolder"))
                using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.ProcessAveFolder", addToStatistics: true))
                {
                    logger.Info($"Process folder:{folder?.ServerRelativeUrl.LogBase64()}");

                    ProgressService.Increase();

                    //某些Hidden Folder Discover到 但取不到AveFolder
                    if (folder == null || folder.Properties == null || folder.Item == null)
                    {
                        logger.Warn("get folder occured error, folder is :{0}", folder.ServerRelativeUrl.LogBase64());
                        return;
                    }
                    var termInfo = GetTermInfo(folder.Item, folder.Item.Fields);
                    RMRuleItemCollection rules = null;
                    SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
                    var key = folder.ServerRelativeUrl + "/";
                    if (!CheckParentRule(key, ref itemRuleInfo))
                    {
                        if (RMSPExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                        {
                            var newRuleCollection = RebuldSPRules(rules);
                            if (newRuleCollection.Rules.Count == 0)
                            {
                                logger.Info($"No SP rules realted to the folder {folder?.ServerRelativeUrl.LogBase64()}");
                            }
                            else
                            {
                                var filterEnginer = new RMSPRuleChecker(newRuleCollection);
                                itemRuleInfo = filterEnginer.CheckDisposalRule(folder, parentItemRule);

                            }
                            if (itemRuleInfo.Rule != null)
                            {
                                RMSPExplorerListLevelCache.Instance.Add(folder.ServerRelativeUrl + "/", itemRuleInfo);
                            }

                        }
                        else if (parentItemRule.Rule != null)
                        {
                            itemRuleInfo.Rule = parentItemRule.Rule;
                            itemRuleInfo.DisposalAction = parentItemRule.DisposalAction;
                        }

                    }
                    itemRuleInfo.TermInfo = termInfo;

                    var item = syncItem.AssembleRecord(folder, folder.UniqueId, itemRuleInfo);
                    SyncItemToDB(item);

                }
            }
            catch (Exception e)
            {
                logger.Error($"error occurred while Process folder:{folder?.ServerRelativeUrl.LogBase64()}, ERROR:{e.ToString()}");
                JobContext.Current.HasErrorNode = true;
                _siteCache.HasErrorNode = true;
                this.AddExceptionListDic(folder.ParentListId);
                JobDetailService.Commit(new Contract.Global.RMWeb.JobMonitor.JMCollectionDataJobDetails()
                {
                    ObjectName = folder?.Name,
                    FullPath = folder?.Url,
                    Status = JobDetailsStatus.Failed,
                    Comment = e.Message,
                    AgentName = OSInformation.HostName
                });
            }

        }

        private void AddExceptionListDic(Guid listId)
        {
            lock (unSuccessListLock)
            {
                if (!unSuccessList.Contains(listId))
                {
                    unSuccessList.Add(listId);
                }
            }
        }
        private Guid GetListId(IAveListItem aveItem)
        {
            if (aveItem != null && aveItem.ParentList != null)
            {
                return aveItem.ParentList.ID;
            }
            return Guid.Empty;
        }

        private bool CheckParentRule(string key, ref SyncItemRuleInfo rule)
        {
            bool result = false;

            if (RMSPExplorerListLevelCache.Instance.FolderRule.Keys.Any(k => key.StartsWith(k)))
            {
                var tempKey = RMSPExplorerListLevelCache.Instance.FolderRule.Keys.Where(k => key.StartsWith(k)).FirstOrDefault();
                rule = RMSPExplorerListLevelCache.Instance.FolderRule[tempKey];
                logger.Info($"folder meet parent rule: key:{key.LogBase64()}, parentKey:{tempKey.LogBase64()}");
                result = true;
            }
            return result;
        }


        private string GetExceptionMessage(Exception e)
        {
            string comment = e.Message;
            if (e is System.Reflection.TargetInvocationException)
            {
                System.Reflection.TargetInvocationException te = e as System.Reflection.TargetInvocationException;
                if (te.InnerException != null)
                {
                    comment = te.InnerException.Message;
                }
            }
            return comment;
        }
        //TODO fpwang
        public virtual void ProcessFolder(IAveList aveList, AveDiscoverFolder discoverFolder, SyncItemRuleInfo parentItemRule)
        {
            try
            {
                using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.ProcessFolder", addToStatistics: true))
                {
                    logger.Info($"Process folder:{discoverFolder?.FullUrl.LogBase64()}");
                    //using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        ProgressService.Increase();
                        //有一些Hidden Folder通过这个属性判断不出来， 优先判断Hidden属性
                        if (discoverFolder.Hidden.HasValue && discoverFolder.Hidden.Value)
                        {
                            logger.Info("skip hidden folder object {0} : {1}", aveList?.ID, discoverFolder?.FullUrl.LogBase64());
                            return;
                        }
                        if (discoverFolder.ChangeType == Wrapper.Common.ChangeType.Delete)
                        {
                            logger.Info("remove folder object {0} : {1}", aveList?.ID, discoverFolder.tp_GUID);
                            RemoveSPObj(discoverFolder.DocID);
                            return;
                        }
                        var aveFolder = discoverFolder.AveFolder;
                        //某些Hidden Folder Discover到 但取不到AveFolder
                        if (aveFolder == null || aveFolder.Properties == null || aveFolder.Item == null)
                        {
                            logger.Warn("get folder occured error, folder is :{0}", discoverFolder.FullUrl.LogBase64());
                            return;
                        }
                        var termInfo = GetTermInfo(aveFolder.Item, aveFolder.Item.Fields);
                        RMRuleItemCollection rules = null;
                        SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
                        if (RMSPExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                        {
                            var newRuleCollection = RebuldSPRules(rules);
                            if (newRuleCollection.Rules.Count == 0)
                            {
                                logger.Info($"No SP rules realted to the folder {aveFolder?.ServerRelativeUrl.LogBase64()}");
                            }
                            else
                            {
                                var filterEnginer = new RMSPRuleChecker(newRuleCollection);
                                itemRuleInfo = filterEnginer.CheckDisposalRule(discoverFolder, parentItemRule);

                            }

                        }
                        itemRuleInfo.TermInfo = termInfo;
                        var item = syncItem.AssembleRecord(discoverFolder, discoverFolder.DocID, itemRuleInfo);
                        SyncItemToDB(item);

                        string pagerInfo = string.Empty;
                        do
                        {
                            logger.Info($"Get items under [{discoverFolder.FullUrl.LogBase64()}] with pager. PagerInfo:[{pagerInfo.LogBase64()}]");
                            var items = this.mDiscover.GetItems(aveList, discoverFolder, ref pagerInfo);
                            List<RecordDto> dbRecords = new List<RecordDto>();
                            ProcessItems(aveList, items, discoverFolder.DocID, parentItemRule, dbRecords);
                        }
                        while (!string.IsNullOrEmpty(pagerInfo));

                        var folders = this.mDiscover.GetSubFolders(discoverFolder);
                        logger.Info($"Process folders under [{discoverFolder?.FullUrl.LogBase64()}] Count:[{folders.LongCount()}]");
                        ProgressService.IncreaseBase(folders.LongCount());
                        foreach (var folder in folders)
                        {
                            using (folder)
                            {
                                ProcessFolder(aveList, folder, itemRuleInfo);
                            }
                        }
                    }

                }
            }
            catch (Exception e)
            {
                logger.Error($"error occurred while Process folder:{discoverFolder?.FullUrl.LogBase64()}, ERROR:{e.ToString()}");
                JobContext.Current.HasErrorNode = true;
                _siteCache.HasErrorNode = true;
                JobDetailService.Commit(new Contract.Global.RMWeb.JobMonitor.JMCollectionDataJobDetails()
                {
                    ObjectName = discoverFolder?.LeafName,
                    FullPath = discoverFolder?.FullUrl,
                    Status = JobDetailsStatus.Failed,
                    Comment = GetExceptionMessage(e),
                    AgentName = OSInformation.HostName
                });
            }
        }

        //private void GetDBRecords(List<IAveDiscoverItem> items)
        //{
        //    if (items.Count > 0)
        //    {
        //        List<AvePoint.RA.Contract.Global.SharePoint.QueryRecordDto> queryDtos = new List<AvePoint.RA.Contract.Global.SharePoint.QueryRecordDto>();
        //        foreach (var item in items)
        //        { 
        //        }
        //    }
        //}

        public virtual void ProcessItem(IAveList list, IAveDiscoverItem discoverItem, Guid parentId, SyncItemRuleInfo parentItemRule, CancellationTokenSource cts = null)
        {
            string itemName = string.Empty;
            string itemUrl = string.Empty;
            IAveListItem aveItem = null;
            try
            {
                using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.ProcessItem", addToStatistics: true))
                {
                    //JobContext.ReportManager.Increase();
                    if (discoverItem.ID == null || (discoverItem.Hidden != null && discoverItem.Hidden == true))
                    {
                        logger.Info($"skip hidden item:{discoverItem?.FullUrl.LogBase64()}");
                        return;
                    }
                    if (discoverItem.ChangeType == Wrapper.Common.ChangeType.Delete)
                    {
                        try
                        {
                            WaitSPOExecuteAction(() =>
                            {
                                aveItem = list.GetItemById((int)discoverItem.ID);
                            });
                        }
                        catch (Exception ex)
                        {
                            var itemGuid = discoverItem.tp_GUID != Guid.Empty ? discoverItem.tp_GUID : discoverItem.DocID;
                            logger.Info($"cannot found item object ID:{discoverItem.ID} Guid:{itemGuid} :{ex.ToString()}");

                            RemoveSPObj(itemGuid, (int)discoverItem.ID);
                        }
                        logger.Warn("remove view item, {0}", discoverItem?.FullUrl.LogBase64());
                        return;
                    }
                    WaitSPOExecuteAction(() =>
                    {
                        aveItem = list.GetItemById((int)discoverItem.ID);
                    });
                    itemName = aveItem?.GetObjectName();
                    if (list.BaseType == AveBaseType.DocumentLibrary && NeedSkipFile(aveItem, itemName))
                    {
                        return;
                    }

                    itemUrl = aveItem.FullPath();
                    logger.Info($"Process item:{itemUrl}");
                    //using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {

                        var termInfo = GetTermInfo(aveItem, aveItem.Fields);
                        RMRuleItemCollection rules = null;
                        SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
                        if (RMSPExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                        {
                            var newRuleCollection = RebuldSPRules(rules);
                            if (newRuleCollection.Rules.Count == 0)
                            {
                                logger.Info($"No SP rules realted to the item {aveItem?.Url.LogBase64()}");
                            }
                            else
                            {
                                var filterEnginer = new RMSPRuleChecker(newRuleCollection);
                                itemRuleInfo = filterEnginer.CheckDisposalRule(aveItem, parentItemRule);

                            }

                        }
                        itemRuleInfo.TermInfo = termInfo;
                        var item = syncItem.AssembleRecord(aveItem, parentId, itemRuleInfo);
                        //RecordDto recordInDB = dbRecords.Where(r => r.ScopeId == item.ScopeId && r.Id == item.Id).FirstOrDefault();

                        //check uniqueId
                        //UpdateRecordId(item);

                        SyncItemToDB(item);


                    }
                }

            }
            //catch (JobStopException)
            //{
            //    cts?.Cancel();
            //    throw new JobStopException("This Job is stopped.");
            //}
            catch (Exception e)
            {
                logger.Error($"error occurred while Process aveitem:{itemUrl.LogBase64()}, ERROR:{e.ToString()}");
                //JobContext.Current.HasErrorNode = true;
                // _siteCache.HasErrorNode = true;
                JobDetailService.Commit(new Contract.Global.RMWeb.JobMonitor.JMCollectionDataJobDetails()
                {
                    ObjectName = itemName,
                    FullPath = itemUrl,
                    Status = JobDetailsStatus.Failed,
                    Comment = GetExceptionMessage(e),
                    AgentName = OSInformation.HostName
                });
            }
            return;
        }

        public virtual void ProcessIncrementalChangedItemV1(IAveList list, IAveListItem aveItem, Guid parentId, SyncItemRuleInfo parentItemRule, CancellationTokenSource cts = null)
        {
            string itemName = string.Empty;
            string itemUrl = string.Empty;
            try
            {
                using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.ProcessChangedItem", addToStatistics: true))
                {
                    ProgressService.Increase();
                    itemName = aveItem?.GetObjectName();
                    if (list.BaseType == AveBaseType.DocumentLibrary && NeedSkipFile(aveItem, itemName))
                    {
                        return;
                    }
                    itemUrl = aveItem.FullPath();
                    if (aveItem.FileSystemObjectType == AveFileSystemObjectType.Folder)
                    {
                        logger.Info($"Current list item is folder so skip it.Url:{itemUrl.LogBase64()}.Id:{aveItem.ID}.");
                        return;
                    }
                    // using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        var termInfo = GetTermInfo(aveItem, aveItem.Fields);
                        RMRuleItemCollection rules = null;
                        SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
                        using (var performance0 = new AgentPerformanceScope("RMSPExplorerProcessor.CheckRule", addToStatistics: true))
                        {
                            if (RMSPExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                            {
                                var newRuleCollection = RebuldSPRules(rules);
                                if (newRuleCollection.Rules.Count == 0)
                                {
                                    logger.Info($"No SP rules realted to the item {list.RootFolder.Url.LogBase64()}:{aveItem.ID}");
                                }
                                else
                                {
                                    var filterEnginer = new RMSPRuleChecker(newRuleCollection);
                                    itemRuleInfo = filterEnginer.CheckDisposalRule(aveItem, parentItemRule);
                                    logger.Debug("CheckDisposalRule finished. Rule id:", itemRuleInfo?.Rule?.Id);
                                }
                            }
                            else
                            {
                                logger.Warn("Cannot found terminfo from term rule mapping. Unique id:", termInfo?.UniqueId);
                            }
                        }
                        itemRuleInfo.TermInfo = termInfo;
                        string owner = null;
                        //if (itemOwnerMapping != null && itemOwnerMapping.Count > 0)
                        //{
                        //    if (itemOwnerMapping.ContainsKey(aveItem.UniqueId))
                        //    {
                        //        owner = itemOwnerMapping[aveItem.UniqueId];
                        //    }
                        //}
                        var item = syncItem.AssembleRecord(aveItem, parentId, itemRuleInfo);

                        //check uniqueId
                        //UpdateRecordId(item, recordInDB);
                        SyncItemToDB(item);
                    }
                }
            }
            //catch (JobStopException)
            //{
            //    cts?.Cancel();
            //    throw new JobStopException("This Job is stopped.");
            //}
            catch (Exception e)
            {
                logger.Error($"error occurred while Process aveitem:{itemUrl.LogBase64()}, ERROR:{e.ToString()}");
                bool isItemNotFound = this.isItemNotFoundError(e);
                if (!isItemNotFound)
                {
                    //JobContext.Current.HasErrorNode = true;
                    //_siteCache.HasErrorNode = true;
                    JobDetailService.Commit(new Contract.Global.RMWeb.JobMonitor.JMCollectionDataJobDetails()
                    {
                        ObjectName = itemName,
                        FullPath = itemUrl,
                        Status = JobDetailsStatus.Failed,
                        Comment = GetExceptionMessage(e),
                        AgentName = OSInformation.HostName
                    });
                    AddFailureItem2Cache(aveItem, parentId, e);
                }
            }
            return;
        }

        public virtual void ProcessIncrementalChangedItem(IAveList list, KeyValuePair<string, object> changeItem, Guid parentId, SyncItemRuleInfo parentItemRule, CancellationTokenSource cts = null)
        {
            string itemName = string.Empty;
            string itemUrl = string.Empty;
            IAveListItem aveItem = null;
            try
            {
                using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.ProcessChangedItem", addToStatistics: true))
                {
                    ProgressService.Increase();
                    Dictionary<string, object> itemChangeProperties = changeItem.Value as Dictionary<string, object>;
                    int itemId = (int)itemChangeProperties["ItemId"];
                    int itemChangeType = (int)itemChangeProperties["ChangeType"];
                    Guid itemUniqueId = (Guid)itemChangeProperties["UniqueId"];
                    // UniqueId: { itemUniqueId}
                    logger.Info($"Process changed item:Id:{itemId}.ChangeType:{itemChangeType}.");
                    if (itemChangeProperties.ContainsKey("Hidden") && (bool)itemChangeProperties["Hidden"])
                    {
                        logger.Info($"skip hidden item:{itemId}");
                        return;
                    }
                    if (itemChangeType == (int)Wrapper.Common.ChangeType.Delete)
                    {
                        try
                        {
                            using (var performance0 = new AgentPerformanceScope("RMSPExplorerProcessor.GetDeletedItem", addToStatistics: true))
                            {
                                WaitSPOExecuteAction(() =>
                                {
                                    aveItem = list.GetItemById(itemId);
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Info($"cannot found item object ID:{itemId}  :{ex.ToString()}");
                            var dto = new AvePoint.RA.Contract.Global.Object.DeleteItemDto()
                            {
                                SiteId = DiscoverSite.SiteID,
                                ListId = list.ID,
                                ItemRowId = itemId,
                                ItemId = itemUniqueId
                            };
                            AddDeleteItemToCache(dto);
                            // Guid: { itemUniqueId}
                            //RemoveSPObj(itemUniqueId, itemId);
                        }
                        logger.Warn("remove view item, {0}.", itemId);
                        return;
                    }
                    using (var performance0 = new AgentPerformanceScope("RMSPExplorerProcessor.GetItemById", addToStatistics: true))
                    {
                        WaitSPOExecuteAction(() =>
                        {
                            aveItem = list.GetItemById(itemId);
                        });
                    }
                    itemName = aveItem?.GetObjectName();
                    if (list.BaseType == AveBaseType.DocumentLibrary && NeedSkipFile(aveItem, itemName))
                    {
                        return;
                    }
                    itemUrl = aveItem.FullPath();
                    if (aveItem.FileSystemObjectType == AveFileSystemObjectType.Folder)
                    {
                        logger.Info($"Current list item is folder so skip it.Url:{itemUrl.LogBase64()}.Id:{itemId}.");
                        return;
                    }
                    // using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        var termInfo = GetTermInfo(aveItem, aveItem.Fields);
                        RMRuleItemCollection rules = null;
                        SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
                        using (var performance0 = new AgentPerformanceScope("RMSPExplorerProcessor.CheckRule", addToStatistics: true))
                        {
                            if (RMSPExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                            {
                                var newRuleCollection = RebuldSPRules(rules);
                                if (newRuleCollection.Rules.Count == 0)
                                {
                                    logger.Info($"No SP rules realted to the item {list.RootFolder.Url.LogBase64()}:{itemId}");
                                }
                                else
                                {
                                    var filterEnginer = new RMSPRuleChecker(newRuleCollection);
                                    itemRuleInfo = filterEnginer.CheckDisposalRule(aveItem, parentItemRule);
                                    logger.Debug("CheckDisposalRule finished. Rule id:", itemRuleInfo?.Rule?.Id);
                                }
                            }
                            else
                            {
                                logger.Warn("Cannot found terminfo from term rule mapping. Unique id:", termInfo?.UniqueId);
                            }
                        }
                        itemRuleInfo.TermInfo = termInfo;
                        string owner = null;
                        //if (itemOwnerMapping != null && itemOwnerMapping.Count > 0)
                        //{
                        //    if (itemOwnerMapping.ContainsKey(aveItem.UniqueId))
                        //    {
                        //        owner = itemOwnerMapping[aveItem.UniqueId];
                        //    }
                        //}
                        var item = syncItem.AssembleRecord(aveItem, parentId, itemRuleInfo);

                        //check uniqueId
                        //UpdateRecordId(item, recordInDB);
                        SyncItemToDB(item);
                    }
                }
            }
            //catch (JobStopException)
            //{
            //    cts?.Cancel();
            //    throw new JobStopException("This Job is stopped.");
            //}
            catch (Exception e)
            {
                logger.Error($"error occurred while Process aveitem:{itemUrl.LogBase64()}, ERROR:{e.ToString()}");
                bool isItemNotFound = this.isItemNotFoundError(e);
                if (!isItemNotFound)
                {
                    //JobContext.Current.HasErrorNode = true;
                    //_siteCache.HasErrorNode = true;
                    JobDetailService.Commit(new Contract.Global.RMWeb.JobMonitor.JMCollectionDataJobDetails()
                    {
                        ObjectName = itemName,
                        FullPath = itemUrl,
                        Status = JobDetailsStatus.Failed,
                        Comment = GetExceptionMessage(e),
                        AgentName = OSInformation.HostName
                    });
                    AddFailureItem2Cache(aveItem, parentId, e);
                }
            }
            return;
        }

        private void AddFailureItem2Cache(IAveListItem aveItem, Guid parentId, Exception e)
        {
            RMAgentSyncFailureItem failureItem = new RMAgentSyncFailureItem()
            {
                SiteId = DiscoverSite.SiteID.ToString(),
                ListId = aveItem.ParentList.ID.ToString(),
                IntemIntId = aveItem.ID,
                JobId = JobContext.Current.JobId,
                ItemId = aveItem.UniqueId.ToString(),
                ParentId = parentId.ToString(),
                WebId = aveItem.ParentList.ParentWeb.ID.ToString(),
                SourceFlag = (int)SourceFlag.SharePointOnPrem,
                SortTicks = Snowflake.Instance().GetTicks()
            };
            failureItem.URL = aveItem?.Url;
            failureItem.ObjectName = aveItem?.Name;
            failureItem.Message = this.GetExceptionMessage(e);
            RMSPExplorerDataCache.Instance.CurrentJobFailedItems.Add(failureItem);
        }

        private void AddDeleteItemToCache(AvePoint.RA.Contract.Global.Object.DeleteItemDto dto)
        {
            mDeleteItemCache.Add(dto);
            if (mDeleteItemCache.Count > ExternalUtil.TransferDataCount)
            {
                try
                {
                    var dtos = mDeleteItemCache.Take(ExternalUtil.TransferDataCount).ToList();
                    using (var performance = new AgentPerformanceScope("RMSPExplorerBase.UpdateDeletedItemsInExplorer", $"RMSPExplorerBase.UpdateDeletedItemsInExplorer.Count:{dtos.Count}", true))
                    {
                        HybridApiClient.Instance.UpdateDeletedItemsInExplorer(dtos);
                    }
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while updating deleted items. Error:{0}", e.ToString());
                }
            }
        }

        private void FinalAddDeleteItemToCache()
        {
            if (mDeleteItemCache.Count > 0)
            {
                try
                {
                    var dtos = mDeleteItemCache.TakeAll().ToList();
                    using (var performance = new AgentPerformanceScope("RMSPExplorerBase.UpdateDeletedItemsInExplorer", $"RMSPExplorerBase.UpdateDeletedItemsInExplorer.Count:{dtos.Count}", true))
                    {
                        HybridApiClient.Instance.UpdateDeletedItemsInExplorer(dtos);
                    }
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while updating deleted items. Error:{0}", e.ToString());
                }
            }
        }

        public void SyncItemToDB(RecordDto newItem)
        {
            newItem.ContainerId = containerId;
            newItem.BulkImportEnabled = JobContext.Current.BulkImportEnabled;
            newItem.BulkSize = JobContext.Current.BulkSize;
            RMSPExplorerDataCache.Instance.NeedSyncDataCache.Add(newItem);
        }

        private Contract.Global.Object.RMTermInfo GetTermInfo(IAvePropertyBag properties)
        {
            var termInfo = new Contract.Global.Object.RMTermInfo();

            if (properties.ContainsKey(RA.Common.Global.RcordsBuiltInColumn.CONTAINER_BCS_NAME))
            {
                var termId = properties[RA.Common.Global.RcordsBuiltInColumn.CONTAINER_BCS_NAME];
                if (termId != null)
                {
                    termInfo.UniqueId = new Guid(termId.ToString());
                    termInfo.Name = RMSPExplorerDataCache.Instance.Terms.ContainsKey(termInfo.UniqueId) ? RMSPExplorerDataCache.Instance.Terms[termInfo.UniqueId].Name : string.Empty;
                }
            }
            return termInfo;
        }

        private Contract.Global.Object.RMTermInfo GetTermInfo(Hashtable properties)
        {
            var termInfo = new Contract.Global.Object.RMTermInfo();

            if (properties.ContainsKey(RA.Common.Global.RcordsBuiltInColumn.CONTAINER_BCS_NAME))
            {
                var termId = properties[RA.Common.Global.RcordsBuiltInColumn.CONTAINER_BCS_NAME];
                if (termId != null)
                {
                    termInfo.UniqueId = new Guid(termId.ToString());
                    termInfo.Name = RMSPExplorerDataCache.Instance.Terms.ContainsKey(termInfo.UniqueId) ? RMSPExplorerDataCache.Instance.Terms[termInfo.UniqueId].Name : string.Empty;
                }
            }
            return termInfo;
        }

        private Contract.Global.Object.RMTermInfo GetTermInfo(IAveListItem item, IAveFieldCollection fields)
        {
            var termInfo = new Contract.Global.Object.RMTermInfo();
            using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.GetTermInfo", addToStatistics: true))
            {
                if (fields.ContainsField(_siteCache.BCSColumnInternalName))
                {
                    var termObj = item[_siteCache.BCSColumnInternalName];
                    if (termObj != null && !string.IsNullOrEmpty(termObj.ToString()))
                    {
                        var valueString = termObj.ToString().Split('|');
                        if (valueString.Length > 1)
                        {
                            termInfo.UniqueId = new Guid(valueString[1]);
                            termInfo.Name = RMSPExplorerDataCache.Instance.Terms.ContainsKey(termInfo.UniqueId) ? RMSPExplorerDataCache.Instance.Terms[termInfo.UniqueId].Name : string.Empty;
                        }
                        else
                        {
                            logger.Info($"{item.Url.LogBase64()} invalid term format:{valueString}");
                        }

                    }
                }
            }
            return termInfo;
        }

        //private Contract.Global.Object.BoardChangeType GetChangeType(RecordDto recordInDB)
        //{
        //    Contract.Global.Object.BoardChangeType result = Contract.Global.Object.BoardChangeType.None;
        //    if (recordInDB != null)
        //    {
        //        if (recordInDB.RecordStatus == (int)RMRecordStatus.Active)
        //        {
        //            result = Contract.Global.Object.BoardChangeType.Modified;
        //        }
        //        else
        //        {
        //            result = Contract.Global.Object.BoardChangeType.Add;
        //        }
        //    }
        //    else
        //    {
        //        result = Contract.Global.Object.BoardChangeType.Add;
        //    }
        //    return result;
        //}

        /// <summary>
        /// for this now ,incremental logic not support container level rule change.... to do next...
        /// </summary>
        /// <param name="site"></param>
        public void ProcessTermChangedItems(long lastScanTime, List<Guid> changedTermIds, long mainJobStartTime)
        {
            IAveSite site = DiscoverSite.Site;
            try
            {
                List<RecordDto> allRecords = new List<RecordDto>();
                var ChangeTermIds = changedTermIds;
                // GetChangedTermIds(lastScanTime);
                if (ChangeTermIds.Count > 0)
                {
                    logger.Info("Start to get term changed records. Site Id:{0} Term Ids:{1} Main job start time:{2}", site.ID, string.Join(",", changedTermIds).LogBase64(), mainJobStartTime);
                    using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.GetTermChangedRecords", addToStatistics: true))
                    {
                        logger.Info($"Total changed term count: {ChangeTermIds.Count}");
                        for (int i = 0; i < ChangeTermIds.Count; i += 1000)
                        {
                            var tempIds = ChangeTermIds.Skip(i).Take(1000).ToList();
                            logger.Info($"Query changed term from {i} to {i + 1000}");
                            var records = GetTermChangedRecords(site.ID, tempIds, mainJobStartTime);
                            if (records != null && records.Count > 0)
                            {
                                allRecords.AddRange(records);
                            }
                        }
                    }
                    //ExplorerDao.GetRecordsByTerms(site.ID, ChangeTermIds, JobContext.JobStartTime.Ticks);
                }

                if (allRecords == null || allRecords.Count == 0)
                {
                    logger.Info("No Incremental Classification change records in site  {0}", site.Url.LogBase64());
                    return;
                }
                Dictionary<Guid, List<RecordDto>> webObjs = allRecords.GroupBy(r => r.WebId).ToDictionary(g => g.Key, p => p.ToList());
                IAveWeb web = null;
                IAveList list = null;
                ProgressService.IncreaseBase(webObjs.Count);
                foreach (var webId in webObjs.Keys)
                {
                    try
                    {
                        if (web == null || !web.ID.Equals(webId))
                        {
                            web = site.OpenWeb(webId);
                            logger.Info("Process classification change web {0}", web.Url.LogBase64());
                        }
                        var listNodes = webObjs[webId].GroupBy(t => t.ListId).ToDictionary(g => g.Key, p => p.ToList());

                        foreach (var listId in listNodes.Keys)
                        {
                            try
                            {
                                if (list == null || !list.ID.Equals(listId))
                                {
                                    list = web.GetList(listId);
                                    logger.Info("Process classification change list {0}", list.RootFolder.Url.LogBase64());
                                }
                                var records = listNodes[listId];
                                var itemIntIds = records.Select(i => i.ItemRowId).ToList();
                                for (int i = 0; i < itemIntIds.Count; i += 2000)
                                {
                                    var rowIds = itemIntIds.Skip(i).Take(2000).ToList();
                                    IEnumerable<IAveListItem> items = GetItemsByRowIds(list, rowIds);
                                    int existingItemsPerTask = items.Count() / 4; 
                                    CancellationTokenSource cts = null;
                                    if (items.Count() > itemsPerTask)
                                    {
                                        cts = new CancellationTokenSource();
                                        //最多起4~5个Task处理Incremental的Changed Item，Full Job Get Item默认2k，因此itemsPerTask固定，但是Incremental items 数量不固定，因此需要按照多个处理。
                                        AveTenantTasks.RunParallel(items, existingItemsPerTask, cts, changedItem =>
                                        {
                                            RealProcessTermChangeItem(changedItem, records, cts);
                                        });
                                    }
                                    else
                                    {
                                        foreach (var changedItem in items)
                                        {
                                            RealProcessTermChangeItem(changedItem, records);
                                        }
                                    }
                                }

                                #region useless
                                //var records = listNodes[listId];
                                //var batchGetItem = records.Count > 1000;
                                //List<Guid> itemIds;
                                //if (batchGetItem)
                                //{
                                //    logger.Info("Process term change data in batch.");
                                //    itemIds = records.Select(i => i.NodeId).ToList();
                                //    int totalItemCount = 0;
                                //    //Full job optimiz for Cardinia
                                //    int rowLimit = GetListViewThresholdNumber(list); // list.ParentWeb.Site.GetMaxItemsPerThrottledOperation();
                                //    bool needQueryNext = false;
                                //    AveCamlQuery query = GetQuery(list.RootFolder, rowLimit);
                                //    int startIdx = 0;
                                //    int endIdx = startIdx + rowLimit;
                                //    int maxItemId = SPCommonUtility.GetLastItemFolderId(list, list.RootFolder);
                                //    IAveListItemCollection items = null;
                                //    do
                                //    {
                                //        //using (CheckJobStopScope jScope = new CheckJobStopScope())
                                //        {
                                //            items = list.GetItemsForRecords(query);
                                //        }

                                //        logger.Info($"Existing job process item count:[{items.Count}]");
                                //        totalItemCount += items.Count;
                                //        var tempItems = items.Where(i => itemIds.Contains(i.UniqueId)).ToList();
                                //        ProgressService.IncreaseBase(tempItems.Count);
                                //        // ProcessAveItems(items, list.RootFolder.UniqueId, parentItemRule);
                                //        ProcessTermChangeItems(tempItems, records);
                                //        startIdx = endIdx;
                                //        endIdx = startIdx + rowLimit;

                                //        needQueryNext = startIdx < maxItemId;
                                //        if (needQueryNext)
                                //        {
                                //            logger.Info($"Query for items. StartIndex:[{startIdx}] EndIndex:[{endIdx}]");
                                //            query.ViewXml = GetQueryXml(startIdx, endIdx, rowLimit);
                                //        }
                                //    }
                                //    while (needQueryNext);
                                //}
                                //else
                                //{
                                //    foreach (var itemNode in listNodes[listId])
                                //    {
                                //        //using (CheckJobStopScope stopScope = new CheckJobStopScope())
                                //        {
                                //            #region process item
                                //            IAveListItem item = null;
                                //            try
                                //            {
                                //                WaitSPOExecuteAction(() =>
                                //                {
                                //                    item = list.GetItemById((int)itemNode.ItemRowId);
                                //                });
                                //            }
                                //            catch (Exception e)
                                //            {
                                //                logger.Error("An error occurred while getting item. Item Name: {0}, Item Path: {1}, ERROR:{2}", itemNode.LeafName, itemNode.DirPath, e.ToString());
                                //                continue;
                                //            }
                                //            RealProcessTermChangeItem(item, itemNode);
                                //            #endregion
                                //        }
                                //    }
                                //}
                                #endregion
                            }
                            catch (Exception le)
                            {
                                logger.Warn("Process classification list error {0}:{1}", listId, le.ToString());

                            }
                        }
                    }
                    catch (Exception we)
                    {
                        logger.Warn("process classification web error {0}:{1}", webId, we.ToString());
                    }
                    finally
                    {
                        ProgressService.Increase();
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"error occurred while Process changed site:{site.Url.LogBase64()}, ERROR:{e.ToString()}");
                JobContext.Current.HasErrorNode = true;
                _siteCache.HasErrorNode = true;
                JobDetailService.Commit(new Contract.Global.RMWeb.JobMonitor.JMCollectionDataJobDetails()
                {
                    ObjectName = site.RootWeb.Title,
                    FullPath = site.Url,
                    Status = JobDetailsStatus.Failed,
                    Comment = GetExceptionMessage(e),
                    AgentName = OSInformation.HostName
                });
            }
            finally
            {
                FinalTermChangedRecordsInExplorer();
            }
        }

        //public virtual void ProcessTermChangeItems(List<IAveListItem> items, List<RecordDto> records)
        //{
        //    logger.Info($"Process term change item count:[{items.Count}]");
        //    CancellationTokenSource cts = null;
        //    if (items.Count > itemsPerTask)
        //    {
        //        cts = new CancellationTokenSource();
        //        AveTenantTasks.RunParallelBatch(items, itemsPerTask, cts, item =>
        //        {
        //            ProcessTermChangeItemBatch(item, records, cts);
        //        });
        //    }
        //    else
        //    {
        //        //foreach (var item in items)
        //        //{
        //        //    ProcessAveItem(item, parentId, parentItemRule);
        //        //}
        //        ProcessTermChangeItemBatch(items, records);
        //    }
        //}

        //public virtual void ProcessTermChangeItemBatch(IEnumerable<IAveListItem> items, List<RecordDto> records, CancellationTokenSource cts = null)
        //{
        //    foreach (var item in items)
        //    {
        //        RealProcessTermChangeItem(item, records.Where(i => i.NodeId == item.UniqueId).FirstOrDefault());
        //    }
        //}


        private void RealProcessTermChangeItem(IAveListItem item, List<RecordDto> itemNodes, CancellationTokenSource cts = null)
        {
            using (var performance = new AgentPerformanceScope("RMSPExplorerProcessor.RealProcessTermChangeItem", addToStatistics: true))
            {
                string itemName = string.Empty, itemUrl = string.Empty;
                var itemNode = itemNodes.Where(n => n.NodeId == item.UniqueId).FirstOrDefault();
                if (itemNode == null)
                {
                    return;
                }

                #region process item               
                try
                {
                    //WaitSPOExecuteAction(() =>
                    //{
                    //    item = list.GetItemById((int)itemNode.ItemRowId);
                    //});
                    itemName = item?.GetObjectName();
                    if (NeedSkipFile(item, itemName))
                    {
                        return;
                    }

                    itemUrl = item.FullPath();

                    logger.Info($"Process classification change item {itemUrl.LogBase64()}");
                    RMRuleItemCollection rules = null;
                    var termInfo = new Contract.Global.Object.RMTermInfo()
                    {
                        Name = itemNode.TermName,
                        UniqueId = itemNode.TermId
                    };
                    SyncItemRuleInfo ruleInfo = new SyncItemRuleInfo();
                    using (var performance0 = new AgentPerformanceScope("RMSPExplorerProcessor.CheckRule", addToStatistics: true))
                    {
                        if (RMSPExplorerDataCache.Instance.TermRuleMapping.TryGetValue(itemNode.TermId, out rules))
                        {
                            var newRuleCollection = RebuldSPRules(rules);
                            if (newRuleCollection.Rules.Count == 0)
                            {
                                logger.Info($"No SP rules realted to the item {itemNode?.DirPath.LogBase64()}");
                            }
                            else
                            {
                                var filterEnginer = new RMSPRuleChecker(newRuleCollection);
                                ruleInfo = filterEnginer.CheckDisposalRule(item);
                            }

                        }

                    }
                    using (var performance0 = new AgentPerformanceScope("RMSPExplorerProcessor.UpdateItem", addToStatistics: true))
                    {
                        if (ruleInfo.Rule != null && (itemNode.RuleLevel == 0 || itemNode.RuleLevel >= 32))
                        {
                            logger.Info($"swith item rule: {itemUrl.LogBase64()}, {itemNode.RuleId} 2 {ruleInfo.Rule?.Id}.");
                            #region change item rule
                            var ruleId = new Guid(ruleInfo.Rule.Id);
                            if (!ruleInfo.Rule.IsManualApproval)
                            {
                                itemNode.RecordOwner = string.Empty;
                            }
                            itemNode.RuleId = ruleId;
                            itemNode.RuleLevel = (int)ruleInfo.Rule.PolicyLevel;

                            itemNode.PreviosDisposalDueDate = ruleInfo.DisposalAction;
                            itemNode.DisposalDueDate = ruleInfo.DisposalAction;
                            UpdateDueDate(itemNode, ruleInfo);
                            UpdateTermChangedRecordsInExplorer(itemNode, itemName, itemUrl);
                            #endregion
                        }
                        else if (itemNode.RuleLevel == (int)GCommon.Contract.CommonFilter.PolicyLevel.Document || itemNode.RuleLevel == (int)GCommon.Contract.CommonFilter.PolicyLevel.Item || itemNode.RuleLevel == (int)GCommon.Contract.CommonFilter.PolicyLevel.List)
                        {
                            logger.Info("Empty the item rule {0}", item.Url.LogBase64());
                            #region empty item rule
                            itemNode.RuleId = Guid.Empty;
                            itemNode.RuleLevel = 0;
                            itemNode.RecordOwner = string.Empty;
                            itemNode.DisposalDueDate = string.Empty;
                            itemNode.PreviosDisposalDueDate = string.Empty;
                            UpdateTermChangedRecordsInExplorer(itemNode, itemName, itemUrl);
                            #endregion
                        }
                        else
                        {
                            logger.Info("No change item {0}", item.Url.LogBase64());
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Process classification item error. Item Name: {0}, Item Path: {1}, ERROR:{2}", itemNode.LeafName.LogBase64(), itemNode.DirPath.LogBase64(), e.ToString());
                    bool isItemNotFound = this.isItemNotFoundError(e);
                    if (!isItemNotFound)
                    {
                        //JobContext.Current.HasErrorNode = true;
                        //_siteCache.HasErrorNode = true;
                        JobDetailService.Commit(new Contract.Global.RMWeb.JobMonitor.JMCollectionDataJobDetails()
                        {
                            ObjectName = itemName,
                            FullPath = itemUrl,
                            Status = JobDetailsStatus.Failed,
                            Comment = GetExceptionMessage(e),
                            AgentName = OSInformation.HostName
                        });
                        AddFailureItem2Cache(item, itemNode.ParentId, e);
                    }
                }
                finally
                {
                    ProgressService.Increase();
                }
                #endregion

                //catch (JobStopException)
                //{
                //    throw new JobStopException("This Job is stopped.");
                //}
            }

        }

        private void FinalTermChangedRecordsInExplorer()
        {
            if (mTermChangedDataCache.Count > 0)
            {
                try
                {
                    var dtos = mTermChangedDataCache.TakeAll().ToList();
                    List<Guid> failedIds = new List<Guid>();
                    using (var performance = new AgentPerformanceScope("RMSPExplorerBase.UpdateRecordsInExplorer", $"RMSPExplorerBase.UpdateRecordsInExplorer.Count:{dtos.Count}", true))
                    {
                        failedIds = HybridApiClient.Instance.AddSPDataToExplorer(dtos.Select(r => r.RecordDto).ToList()).FailedGuids;
                    }
                    if (failedIds != null && failedIds.Count > 0)
                    {
                        JobContext.Current.HasErrorNode = true;
                    }
                    foreach (var item in dtos)
                    {
                        bool isFailed = failedIds.Contains(item.RecordDto.Id);
                        JobDetailService.Commit(new Contract.Global.RMWeb.JobMonitor.JMCollectionDataJobDetails()
                        {
                            ObjectName = item.Name,
                            FullPath = item.Url,
                            Status = isFailed ? JobDetailsStatus.Failed : JobDetailsStatus.Successful,
                            Comment = isFailed ? "RM_JM_FSFailedAddToExplorer" : string.Empty,
                            AgentName = OSInformation.HostName
                        });
                    }
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while updating records. Error:{0}", e.ToString());
                    JobContext.Current.HasErrorNode = true;
                }
            }
        }

        private List<RecordDto> GetTermChangedRecords(Guid scopeId, List<Guid> termIds, long ticks)
        {
            List<RecordDto> folderRecords = new List<RecordDto>();
            using (var performance = new AgentPerformanceScope("RMSPExplorerBase.GetRecordsByTerms", $"RMSPExplorerBase.GetRecordsByTerms.ScopeId:{scopeId}", true))
            {
                try
                {

                    long sortTicks = DateTime.MinValue.Ticks;
                    while (true)
                    {

                        var data = HybridApiClient.Instance.GetRecordsByTerms(scopeId, termIds, ticks, sortTicks, ExternalUtil.TransferDataCount);

                        //JobContext.Current.ApiClient.GetDBRecordsByFolder(folderId, FSJobCache.Instance.RootPath.ToLowerInvariant().ToMd5().ToString(), sortTicks, ExternalUtil.TransferDataCount);
                        if (data != null && data.Count > 0)
                        {
                            folderRecords.AddRange(data);
                        }
                        if (data == null || data.Count < ExternalUtil.TransferDataCount)
                        {
                            break;
                        }
                        sortTicks = data[data.Count - 1].SortTicks;
                    }
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while GetTermChangedRecords. Error:{0}", e.ToString());
                }
            }
            return folderRecords;
        }

        private void UpdateTermChangedRecordsInExplorer(RecordDto dto, string name, string url)
        {
            ReportDto reportDto = new ReportDto()
            {
                RecordDto = dto,
                Name = name,
                Url = url
            };
            mTermChangedDataCache.Add(reportDto);
            if (mTermChangedDataCache.Count > ExternalUtil.TransferDataCount)
            {
                try
                {
                    var dtos = mTermChangedDataCache.Take(ExternalUtil.TransferDataCount).ToList();
                    List<Guid> failedIds = new List<Guid>();
                    using (var performance = new AgentPerformanceScope("RMSPExplorerBase.AddSPDataToExplorer", $"RMSPExplorerBase.AddSPDataToExplorer.Count:{dtos.Count}", true))
                    {
                        failedIds = HybridApiClient.Instance.AddSPDataToExplorer(dtos.Select(r => r.RecordDto).ToList()).FailedGuids;
                    }
                    if (failedIds != null && failedIds.Count > 0)
                    {
                        JobContext.Current.HasErrorNode = true;
                    }
                    foreach (var item in dtos)
                    {
                        bool isFailed = failedIds.Contains(item.RecordDto.Id);
                        JobDetailService.Commit(new Contract.Global.RMWeb.JobMonitor.JMCollectionDataJobDetails()
                        {
                            ObjectName = item.Name,
                            FullPath = item.Url,
                            Status = isFailed ? JobDetailsStatus.Failed : JobDetailsStatus.Successful,
                            Comment = isFailed ? "RM_JM_FSFailedAddToExplorer" : string.Empty,
                            AgentName = OSInformation.HostName
                        });
                    }
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while updating records in explorer. Error:{0}", e.ToString());
                    JobContext.Current.HasErrorNode = true;
                }
            }
        }

        private bool isItemNotFoundError(Exception e)
        {
            if (e.InnerException != null && e.InnerException.GetType().FullName.Equals("Microsoft.SharePoint.Client.ServerException", StringComparison.OrdinalIgnoreCase))
            //e.InnerException is Microsoft.SharePoint.Client.ServerException)
            {
                //var ex = e.InnerException as Microsoft.SharePoint.Client.ServerException;
                if (!string.IsNullOrWhiteSpace(e.InnerException.Message) && e.InnerException.Message.Contains("Item does not exist"))
                {
                    return true;
                }
            }
            return false;
        }

        private void UpdateDueDate(RecordDto itemNode, SyncItemRuleInfo ruleInfo)
        {
            //Hold状态Record重新计算Due Date;
            if (itemNode.HoldStatus && RuleHelper.IsRemoveRule(ruleInfo.Rule, itemNode.SourceFlag))
            {
                long newDisposalDueDate = 0;
                if (AvePoint.RA.Contract.Common.DueDateUtil.ConvertStringDueDate2Long(itemNode.DisposalDueDate) == AvePoint.RA.Contract.Common.DueDateUtil.NextJob)
                {
                    newDisposalDueDate = itemNode.HoldReleaseTime;
                }
                if (AvePoint.RA.Contract.Common.DueDateUtil.ConvertStringDueDate2Long(itemNode.DisposalDueDate) > 0)
                {
                    if (AvePoint.RA.Contract.Common.DueDateUtil.ConvertStringDueDate2Long(itemNode.DisposalDueDate) > itemNode.HoldReleaseTime)
                    {
                        newDisposalDueDate = AvePoint.RA.Contract.Common.DueDateUtil.ConvertStringDueDate2Long(itemNode.DisposalDueDate);
                    }
                    else
                    {
                        newDisposalDueDate = itemNode.HoldReleaseTime;
                    }
                }
                itemNode.DisposalDueDate = AvePoint.RA.Contract.Common.DueDateUtil.ConvertLongDueDate2String(newDisposalDueDate);
            }
        }
        //get from job message
        //private List<Guid> GetChangedTermIds(long ticks)
        //{
        //    List<Guid> allTerms = new List<Guid>();
        //    try
        //    {
        //        //List<Guid> subTerms = new List<Guid>();
        //        //allTerms = RMChangeClassificationDao.GetAllChange(ticks, (int)Contract.Object.TermChangeType.TermRule);
        //        //foreach (var id in allTerms)
        //        //{
        //        //    subTerms.AddRange(TermDao.GetAllSubTermUniqueIds(id));
        //        //}
        //        //allTerms.AddRange(subTerms);
        //        //return allTerms;

        //    }
        //    catch (Exception e)
        //    {
        //        logger.Error("get change terms error {0}", e.ToString());
        //    }
        //    return allTerms;
        //}

        protected void WaitSPOExecuteAction(Action action)
        {
            WaitExecuteAction(_spoCallLimiter, action);
        }

        protected void WaitCosmosExecuteAction(Action action)
        {
            WaitExecuteAction(_cosmosCallLimiter, action);
        }

        private void WaitExecuteAction(CallLimiter callLimiter, Action action)
        {
            callLimiter.WaitCallLimitPerSecond();
            action();
        }

        private void UpdateRecordId(RecordDto recoEntity, RecordDto recordInDB)
        {
            recoEntity.RecordsId = recordInDB?.RecordsId;
        }

        private bool NeedSkipList(AveDiscoverList discoverList)
        {
            bool result = false;
            if (discoverList.Hidden.HasValue && discoverList.Hidden.Value)
            {
                logger.Info("Skip the hidden list {0}", string.IsNullOrEmpty(discoverList?.RootFolderUrl.LogBase64()) ? discoverList?.Name.LogBase64() : discoverList?.RootFolderUrl.LogBase64());
                result = true;
            }
            if (discoverList.Name.Equals("{System Folder}"))
            {
                logger.Info("Skip the system list {0}", string.IsNullOrEmpty(discoverList?.RootFolderUrl.LogBase64()) ? discoverList?.Name.LogBase64() : discoverList?.RootFolderUrl.LogBase64());
                result = true;
            }
            else if (CheckIsDesignList(discoverList))
            {
                logger.Info("Skip the design list {0}", string.IsNullOrEmpty(discoverList?.RootFolderUrl.LogBase64()) ? discoverList?.Name.LogBase64() : discoverList?.RootFolderUrl.LogBase64());
                result = true;
            }
            return result;
        }

        public bool NeedSkipFile(IAveListItem item, string objectName)
        {
            string ext = null;
            try
            {
                ext = System.IO.Path.GetExtension(objectName);
                //ext = temp.IndexOf(".") >= 0 ? ext.Substring(1) : temp;
            }
            catch (Exception e)
            {
                logger.Warn(e.Message);
                int lastIndex = objectName.LastIndexOf(".");
                ext = lastIndex >= 0 ? objectName.Substring(lastIndex, objectName.Length - lastIndex) : "";
            }
            //logger.Info("file extension of object name is {0}", ext);
            if (RMSPExplorerDataCache.Instance.ArchiverSettings.SkipFileExtensions.Contains(ext))
            {
                logger.Info("need skip file check rule action {0}:{1}", objectName.LogBase64(), ext);
                return true;
            }
            if (objectName.EndsWith("aspx") && !RMSPExplorerDataCache.Instance.ArchiverSettings.IsDeleteLinkFile)
            {
                logger.Info("need skip file check rule action maybe stub file. {0}:{1}", objectName.LogBase64(), ext);
                return true;
            }
            return false;
        }

        /// <summary>
        /// check bcs column, reset internal name(existing column)
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private bool HasBCSColumn(IAveList list)
        {
            bool result = true;
            try
            {
                if (!list.Fields.ContainsFieldWithInternalName(_siteCache.BCSColumnInternalName))
                {
                    if (_siteCache.BCSColumnInternalName != RA.Common.Global.RcordsBuiltInColumn.ITEM_BCS_NAME)
                    {
                        //existing column reset internal name
                        var bcsColumn = list.Fields.GetFieldById(_siteCache.BCSColumnID, false);
                        if (bcsColumn != null)
                        {
                            _siteCache.BCSColumnInternalName = bcsColumn.InternalName;
                            logger.Info($"reset list bcs column, list:{list.RootFolder?.ServerRelativeUrl.LogBase64()}, column name:{_siteCache.BCSColumnInternalName.LogBase64()}");
                        }
                        else
                        {
                            result = false;
                        }
                    }
                    else
                    {
                        result = false;
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error($"Get list bcs column error:{ex.ToString()}");
            }



            return result;
        }

        private void RemoveSPObj(Guid objectId, int itemRowId = 0)
        {
            var siteId = DiscoverSite.SiteID;
            using (var performance = new AgentPerformanceScope("RMSPExplorerBase.RemoveSPObjInExplorer", $"RMSPExplorerBase.RemoveSPObjInExplorer.ObjectId:{objectId} ItemRowId:{itemRowId}", true))
            {
                HybridApiClient.Instance.RemoveSPObjInExplorer(siteId, objectId, itemRowId);
            }
        }

        private RuleCollection RebuldSPRules(RMRuleItemCollection rules)
        {
            RuleCollection newRuleCol = new RuleCollection();
            Dictionary<int, Rule> newRules = new Dictionary<int, Rule>();
            int reOrder = 0;
            foreach (var order in rules.CommonRules.Rules.Keys)
            {
                if (rules.CommonRules.Rules[order].PolicyLevel != PolicyLevel.None && rules.CommonRules.Rules[order].SOFilters != null && rules.CommonRules.Rules[order].SOFilters.Count > 0)
                {
                    reOrder++;
                    var rule = rules.CommonRules.Rules[order];
                    //var DAUtil = new DAUtil();
                    //DAUtil.AddMoveToFilter(rule);
                    //var newRule = ruleAssembler.ConvertToSPRule(rule);
                    newRules.Add(order, rule);
                }
            }
            newRuleCol.Rules = newRules;
            return newRuleCol;
        }

        private void AddSiteScope(RecordDto item)
        {
            AvePoint.RA.Contract.Global.Object.RMScope site = new AvePoint.RA.Contract.Global.Object.RMScope()
            {
                FullPath = item.DirPath,
                ScopeId = item.ScopeId,
                IsRemoved = false,
                ScopeName = item.LeafName,
            };
            using (var performance = new AgentPerformanceScope("RMSPExplorerBase.AddSiteScope", addToStatistics: true))
            {
                HybridApiClient.Instance.AddSiteScope(site);
            }
        }
    }

    public class ReportDto
    {
        public RecordDto RecordDto;
        public string Name;
        public string Url;
    }



}
