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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Common.Throttle;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using AvePoint.RA.SharePoint.EnforceRetention.Cache;
using AvePoint.RA.SharePoint.ExplorerSync.Modes;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.SharePoint.SPObjDiscover;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using AvePoint.Wrapper.Restore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.EnforceRetention
{
    public class RMEnforceRetentionBase : RMSPDiscoverBase
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMEnforceRetentionBase));
        protected ISPDiscover mDiscover = null;
        protected bool needUpdateLabelState = false;
        protected static int _itemsPerTask = 1000;
        protected long mLastJobTicks = DateTime.MinValue.Ticks;
        protected SPDiscoverType mDiscoverType = SPDiscoverType.Full;
        protected List<RMSPSyncFailureItem> FailureItems = new List<RMSPSyncFailureItem>();
        public ISyncFailureItemDao SyncFailureItemDao { set; get; } = PlatformWindsorManager.GetService<ISyncFailureItemDao>();
        protected int itemsPerTask
        {
            get
            {
                return _itemsPerTask;
            }
        }

        protected static CallLimiter _spoCallLimiter;

        protected void WaitSPOExecuteAction(Action action)
        {
            WaitExecuteAction(_spoCallLimiter, action);
        }
        private void WaitExecuteAction(CallLimiter callLimiter, Action action)
        {
            callLimiter.WaitCallLimitPerSecond();
            action();
        }


        #region Castle Properties

        private ITermDao _termDao;
        public ITermDao TermDao
        {
            get { return _termDao ?? (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao)); }
            set { _termDao = value; }
        }

        private IRMEXOLabelDao _labelDao;
        public IRMEXOLabelDao LabelDao
        {
            get { return _labelDao ?? (IRMEXOLabelDao)PlatformWindsorManager.GetService(typeof(IRMEXOLabelDao)); }
            set { _labelDao = value; }
        }

        #endregion

        public RMEnforceRetentionBase(AveDiscoverSite discoverSite, SPTreeNodeDto treeNode, JobContext jobContext)
            : base(discoverSite, treeNode, jobContext)
        {
            var numSetting = RMGlobalConfiguration.AppConfig[RMAppSettingKey.SPO_SYNC_DATA_ITEMS_PER_TASK];
            if (!string.IsNullOrEmpty(numSetting))
            {
                int.TryParse(numSetting, out _itemsPerTask);
            }

            var spoCallLimitPerSecond = 30;
            var spoCallLimitPerSecondStr = RMGlobalConfiguration.AppConfig[RMAppSettingKey.SPO_SYNC_DATA_CALL_LIMIT_PER_SECOND];
            if (!string.IsNullOrEmpty(spoCallLimitPerSecondStr))
            {
                int.TryParse(spoCallLimitPerSecondStr, out spoCallLimitPerSecond);
            }
            _spoCallLimiter = CallLimiterFactory.CreateInstance("SPOCalllimiter", spoCallLimitPerSecond);
            WrapperConfiguration.WrapperConfigurationForBPOS.IncludeVersionForPerformance = false;
        }

        public void Init(ISPDiscover sPDiscover, SPDiscoverType discoverType, long lastJobTicks)
        {
            mDiscover = sPDiscover;
            mLastJobTicks = lastJobTicks;
            mDiscoverType = discoverType;
        }

        public override async System.Threading.Tasks.Task RunNowAsync()
        {
            try
            {
                using (var performance = new PerformanceScope("RMEnforceRetentionProcesser.RunNow"))
                {
                    ThrowUtil.ThrowIfNull(DiscoverSite, $"Discover Site is null:{TreeNode?.FullPath}");
                    var webs = mDiscover.GetWebs(DiscoverSite);
                    JobContext.ReportManager.IncreaseBase(webs.LongCount());
                    foreach (var web in webs)
                    {
                        using (CheckJobStopScope stopScope = new CheckJobStopScope())
                        {
                            await ProcessWebAsync(web);
                        }
                    }

                    if (!JobContext.NodeLevelError)
                    {
                        //需要插入Flag 或者更新Flag中的时间
                        if (FailureItems.Count > 1000)
                        {
                            logger.Info("More than 1000 failed items in site {0}, count {2}", TreeNode?.FullPath, FailureItems.Count);
                            //failure 数量大于 1000， 不插入Azure Table， 
                            JobContext.HasErrorNode = true;
                            JobContext.NodeLevelError = true;
                        }
                        else
                        {
                            logger.Info("Failed items count{0}, in site {1}", FailureItems.Count, TreeNode?.FullPath);
                            //将失败的Item插入Azure Table， 下次Job再处理
                            AddFailureItem2Azure();
                            //如果存在失败数据， Job状态不能是Finish
                            if (FailureItems.Count > 0)
                            {
                                JobContext.HasErrorNode = true;
                            }
                        }
                    }
                }

            }
            catch (JobStopException)
            {
                throw new JobStopException("the job has stopped.");
            }
            catch (LabelNotExistException ex)
            {
                throw new LabelNotExistException(ex.Message);
            }
            catch (Exception e)
            {
                logger.Error($"error occurred while Process Site:{TreeNode?.FullPath}, ERROR:{e.ToString()}");
                JobContext.HasErrorNode = true;
                JobContext.NodeLevelError = true;
                JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                {
                    ObjectName = TreeNode?.Name,
                    SourceURL = TreeNode?.FullPath,
                    Status = JobDetailsStatus.Failed,
                    Comment = GetExceptionMessage(e),
                });

            }
            finally
            {
                await FinallyUpdateAsync();
            }


        }
        protected string GetExceptionMessage(Exception e)
        {
            string comment = e.Message;
            if (e is System.Reflection.TargetInvocationException)
            {
                if (e.InnerException != null)
                {
                    comment = e.InnerException.Message;
                }
            }
            return comment;
        }
        public virtual async System.Threading.Tasks.Task ProcessWebAsync(AveDiscoverWeb discoverWeb)
        {
            try
            {
                using (var performance = new PerformanceScope("RMEnforceRetentionProcesser.ProcessWeb", addToStatistics: true))
                {
                    logger.Info($"Process web:{discoverWeb?.FullUrl}");                   
                    ArgumentCheck.CheckNotNull(discoverWeb);
                    JobContext.ReportManager.Increase();
                    if (discoverWeb.ChangeType == Wrapper.Common.ChangeType.Delete)
                    {
                        logger.Info("skip removed web object {0} : {1}", DiscoverSite.SiteID, discoverWeb.WebID);
                        return;
                    }
                    var lists = mDiscover.GetLists(discoverWeb);
                    JobContext.ReportManager.IncreaseBase(lists.LongCount());
                    foreach (var list in lists)
                    {
                        using (CheckJobStopScope stopScope = new CheckJobStopScope())
                        {
                            await ProcessListAsync(list, discoverWeb.WebID);
                        }
                    }
                }


            }
            catch (JobStopException)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (LabelNotExistException ex)
            {
                throw new LabelNotExistException(ex.Message);
            }
            catch (Microsoft.SharePoint.Client.ServerException ex)
            {
                logger.Error($"ServerException occurred while Process web:{discoverWeb?.FullUrl}, ErrorCode:{ex?.ServerErrorCode}, ErrorType:{ex?.ServerErrorTypeName}, ERROR:{ex.ToString()}");
            }
            catch (Exception e)
            {
                logger.Error($"error occurred while Process web:{discoverWeb?.FullUrl}, ERROR:{e.ToString()}");
                if (e.InnerException != null && e.InnerException is Microsoft.SharePoint.Client.ServerException)
                {
                    var ex = e.InnerException as Microsoft.SharePoint.Client.ServerException;
                    if (!ex.Message.Contains("File Not Found"))
                    {
                        JobContext.HasErrorNode = true;
                        JobContext.NodeLevelError = true;
                        JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                        {
                            ObjectName = discoverWeb?.Title,
                            SourceURL = discoverWeb?.FullUrl,
                            Status = JobDetailsStatus.Failed,
                            Comment = GetExceptionMessage(e),
                        });
                    }
                }
                else
                {
                    JobContext.HasErrorNode = true;
                    JobContext.NodeLevelError = true;
                    JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                    {
                        ObjectName = discoverWeb?.Title,
                        SourceURL = discoverWeb?.FullUrl,
                        Status = JobDetailsStatus.Failed,
                        Comment = GetExceptionMessage(e),
                    });
                }


            }
        }
        public virtual void ProcessDiscoverItems(IAveList list, IEnumerable<AveDiscoverItem> items)
        {
            logger.Info($"Process item count:[{items.Count()}]");
            JobContext.ReportManager.IncreaseBase(items.Count());

            if (items.Count() > itemsPerTask)
            {
                var cts = new System.Threading.CancellationTokenSource();
                AveTenantTasks.RunParallel(items, itemsPerTask, cts, item =>
                {
                    ProcessDiscoverItem(list, item, cts);
                });
            }
            else
            {
                foreach (var item in items)
                {
                    ProcessDiscoverItem(list, item);
                }
            }
        }

        public virtual void ProcessIncrementalChangedItems(IAveList list, Dictionary<string, object> changedItems)
        {
            try
            {
                int incItemsPerTask = changedItems.Count() / 5;
                logger.Info($"Process incremental changed item count:[{changedItems.Count()}].incItemsPerTask:[{incItemsPerTask}]");
                var changedObjects = changedItems.Values.Select(i => i as Dictionary<string, object>).Where(i => (i.ContainsKey("Hidden") && !(bool)i["Hidden"]) || !i.ContainsKey("Hidden")).ToList();
                //var deleteItems = changedObjects.Where(i => (int)i["ChangeType"] == (int)Wrapper.Common.ChangeType.Delete).ToList();
                var existingItemIds = changedObjects.Where(i => (int)i["ChangeType"] != (int)Wrapper.Common.ChangeType.Delete).Select(i => (int)i["ItemId"]).ToList();
                logger.Info($"Existing item count:{existingItemIds.Count}");
                JobContext.ReportManager.IncreaseBase(existingItemIds.Count);
                for (int i = 0; i < existingItemIds.Count; i += 2000)
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
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
                                ProcessIncrementalChangedItemV1(list, changedItem, cts);
                            });
                        }
                        else
                        {
                            foreach (var changedItem in items)
                            {
                                ProcessIncrementalChangedItemV1(list, changedItem);
                            }
                        }
                    }
                }
            }
            catch (JobStopException)
            {
                throw new JobStopException("This Job is stopped.");
            }
        }

        public virtual void ProcessAveItems(IAveList list, IAveListItemCollection items)
        {
            try 
            {
                logger.Info($"Process List Url {list.RootFolder.Url} item count:[{items.Count}]");
                JobContext.ReportManager.IncreaseBase(items.Count);

                if (items.Count > itemsPerTask)
                {
                    var cts = new System.Threading.CancellationTokenSource();
                    AveTenantTasks.RunParallel(items, itemsPerTask, cts, item =>
                    {
                        ProcessAveItem(item, cts);
                    });
                }
                else
                {
                    foreach (var item in items)
                    {
                        ProcessAveItem(item);
                    }
                }
            }
            catch (JobStopException)
            {
                throw new JobStopException("This Job is stopped.");
            }
        }

        public virtual async System.Threading.Tasks.Task ProcessListAsync(AveDiscoverList discoverList, Guid webId)
        {
            string listPath = string.Empty;
            try
            {
                using (var performance = new PerformanceScope("RMEnforceRetentionProcesser.ProcessList", $"RMEnforceRetentionProcesser.ProcessList Path:[{discoverList?.RootFolderUrl}]", true))
                {
                    logger.Info($"Process list:{discoverList?.RootFolderUrl}");
                    using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        JobContext.ReportManager.Increase();
                        ArgumentCheck.CheckNotNull(discoverList);
                        if (discoverList.ChangeType == Wrapper.Common.ChangeType.Delete)
                        {
                            logger.Info("skip removed list object {0}", discoverList?.ListId);
                            return;
                        }
                        if (discoverList.Name.Equals("{System Folder}"))
                        {
                            logger.Info("Skip the system list {0}", string.IsNullOrEmpty(discoverList?.RootFolderUrl) ? discoverList?.Name : discoverList?.RootFolderUrl);
                            return;
                        }
                        var list = discoverList.GetListObject();
                        if (CheckIsDesignList(discoverList))
                        {
                            logger.Info("Skip the design list {0}", string.IsNullOrEmpty(discoverList?.RootFolderUrl) ? discoverList?.Name : discoverList?.RootFolderUrl);
                            return;
                        }
                        if (!HasBCSColumn(list))
                        {
                            logger.Warn($"list does not have bcs column, list:{discoverList?.RootFolderUrl}, column name:{RetentionDataCache.Instance.BCSColumnInternalName}");
                            return;
                        }
                        listPath = WebUtil.MakeFullUrl(list.ParentWeb.Url, list.RootFolder.Url);
                        ProcessFailedItems(list);

                        switch (mDiscoverType)
                        {
                            case SPDiscoverType.Full:
                                await ProcessItemsForFullJobAsync(list);
                                break;
                            case SPDiscoverType.CAMLSearch:
                                ProcessItemsForSearchDiscover(list);
                                break;
                            case SPDiscoverType.Incremental:
                            default:
                                await ProcessItemsForIncrementalJobAsync(list, discoverList, webId);
                                break;
                        }
                    }
                }

            }
            catch (JobStopException)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (LabelNotExistException ex)
            {
                throw new LabelNotExistException(ex.Message);
            }
            catch (Exception e)
            {
                logger.Error($"error occurred while Process list:{discoverList?.RootFolderUrl}, ERROR:{e.ToString()}");
                JobContext.HasErrorNode = true;
                JobContext.NodeLevelError = true;
                JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                {
                    ObjectName = discoverList?.Title,
                    SourceURL = listPath,
                    Status = JobDetailsStatus.Failed,
                    Comment = GetExceptionMessage(e),
                });
            }

        }
        protected async System.Threading.Tasks.Task ProcessItemsForFullJobAsync(IAveList list)
        {
            try
            {
                bool needQueryNext = false;
                int maxItemId = GetLastItemId(list, list.RootFolder);
                int startIdx = 0;
                int lastIdx = startIdx + MaxItemsPerThrottledOperation;
                var camlManagers = GetCAMLManager();

                logger.Info($"Get items under [{list.RootFolder.ServerRelativeUrl}]");
                IAveListItemCollection items = null;
                do
                {
                    var hasItems = false;
                    int currentItemMaxId = 0;
                    logger.Info($"StartIndex:[{startIdx}] LastIndex:[{lastIdx}] MaxItemId:[{maxItemId}]");
                    using (var performance0 = new PerformanceScope("RMEnforceRetentionProcesser.ProcessAveItems", addToStatistics: true))
                    {
                        foreach (var cm in camlManagers)
                        {
                            using (CheckJobStopScope jScope = new CheckJobStopScope())
                            {
                                var condition1 = new QueryCondition(Types.JoinTypes.And, SPColumnConstants.SP_ID, Types.FieldTypes.Integer, Types.QueryTypes.Gt, startIdx.ToString());
                                var condition2 = new QueryCondition(Types.JoinTypes.And, SPColumnConstants.SP_ID, Types.FieldTypes.Integer, Types.QueryTypes.Leq, (lastIdx).ToString());
                                cm.QueryGroup.AddGroup(new QueryGroup(Types.JoinTypes.And, null, new List<QueryCondition>() { condition1, condition2 }));
                                AveCamlQuery query = new AveCamlQuery();
                                cm.ScopeType = Types.ScopeTypes.Recursive;
                                query.LoadAllItems = false;
                                query.FolderServerRelativeUrl = list.RootFolder.ServerRelativeUrl;
                                query.ListItemCollectionPosition = new AveItemCollectionPosition();
                                cm.RowLimit = MaxItemsPerThrottledOperation;
                                string queryXml = cm.GetFullCAML();
                                query.ViewXml = queryXml;
                                logger.Debug($"query items by pager, start: {startIdx}, end: {lastIdx} :{queryXml}.");
                                using (var performance1 = new PerformanceScope("RMEnforceRetentionProcesser.GetItemsForRecords", addToStatistics: true))
                                {
                                    items = list.GetItemsForRecords(query);
                                }
                                if (items.Count > 0)
                                {
                                    await CheckLabelExistAndThrowExceptionAsync();
                                    ProcessAveItems(list, items);
                                    int tempCurrentItemMaxId = items.Max(i => i.ID);
                                    currentItemMaxId = tempCurrentItemMaxId > currentItemMaxId ? tempCurrentItemMaxId : currentItemMaxId;
                                    hasItems = true;
                                }
                            }
                        } 
                    }
                    startIdx = currentItemMaxId > startIdx ? currentItemMaxId : startIdx;
                    if (!hasItems)
                    {
                        startIdx = lastIdx;
                    }
                    int endIdx = startIdx + MaxItemsPerThrottledOperation;
                    lastIdx = endIdx;
                    needQueryNext = startIdx < maxItemId;
                    if (needQueryNext)
                    {
                        logger.Info($"Query Next");
                        camlManagers = GetCAMLManager();
                    }
                    else
                    {
                        logger.Info($"Query finished.");
                    }
                }
                while (needQueryNext);
            }
            catch(JobStopException)
            {
                throw new JobStopException("This Job is stopped.");
            }
        }

        protected async System.Threading.Tasks.Task ProcessItemsForIncrementalJobAsync(IAveList list, AveDiscoverList discoverList, Guid webId)
        {
            logger.Info($"Get changed items under [{list.RootFolder.ServerRelativeUrl}] for incremental enforce retention job.");
            Dictionary<string, object> changedItems = new Dictionary<string, object>();
            using (var performance0 = new PerformanceScope("RMEnforceRetentionProcesser.GetListChangedItems", addToStatistics: true))
            {
                changedItems = discoverList.GetListChangedItems(webId);
            }
            if (changedItems.Count > 0)
            {   
                await CheckLabelExistAndThrowExceptionAsync();
            }
            ProcessIncrementalChangedItems(list, changedItems);
        }

        protected void ProcessItemsForSearchDiscover(IAveList list)
        {
            try 
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
                    using (var queryAuto = new PerformanceScope("RMEnforceRetentionBase.SearchQueryData", $"RMEnforceRetentionBase.SearchQueryData{list.RootFolder.ServerRelativeUrl} start{startIndex}", true))
                    {
                        AveCamlQuery query = GetSearchDiscoverQuery(list, list.RootFolder, startTime, endTime, startIndex, startIndex + rowLimit, rowLimit);
                        using (CheckJobStopScope jScope = new CheckJobStopScope())
                        {
                            using (var performance = new PerformanceScope("RMEnforceRetentionBase.GetItemsForRecords", addToStatistics: true))
                            {
                                items = list.GetItemsForRecords(query);
                            }
                        }
                        //JobContext.ReportManager.IncreaseBase(items.Count);
                        logger.Info($"Process items in folder url {list.RootFolder.ServerRelativeUrl} item count:[{items.Count}], start index {startIndex}, end index {startIndex + rowLimit}");
                    }
                    using (var queryAuto = new PerformanceScope("RMEnforceRetentionBase.ProcessAveItems", $"RMEnforceRetentionBase.ProcessAveItems{list.RootFolder.ServerRelativeUrl} count {items.Count}", true))
                    {
                        ProcessAveItems(list, items);
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
            catch (JobStopException)
            {
                throw new JobStopException("This Job is stopped.");
            }
        }
        public virtual void ProcessFolder(IAveList aveList, AveDiscoverFolder discoverFolder)
        {
            try
            {
                using (var performance = new PerformanceScope($"SP.RMEnforceRetentionProcesser.ProcessFolder Path:[{discoverFolder?.FullUrl}]"))
                {
                    logger.Info($"Process folder:{discoverFolder?.FullUrl}");
                    using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        JobContext.ReportManager.Increase();
                        ArgumentCheck.CheckNotNull(discoverFolder);
                        if (discoverFolder.ChangeType == Wrapper.Common.ChangeType.Delete)
                        {
                            logger.Info("skip removed folder object {0} : {1}", aveList?.ID, discoverFolder.tp_GUID);
                            return;
                        }
                        if (discoverFolder.Hidden.HasValue && discoverFolder.Hidden.Value)
                        {
                            logger.Info("skip hidden folder object {0} : {1}", aveList?.ID, discoverFolder?.FullUrl);
                            return;
                        }
                        string pagerInfo = string.Empty;
                        do
                        {
                            logger.Info($"Get items under [{discoverFolder?.FullUrl}] with pager. PagerInfo:[{pagerInfo}]");
                            var items = this.mDiscover.GetItems(aveList, discoverFolder, ref pagerInfo);
                            ProcessDiscoverItems(aveList, items);
                        }
                        while (!string.IsNullOrEmpty(pagerInfo));

                        var folders = this.mDiscover.GetSubFolders(discoverFolder);
                        logger.Info($"Process folders under [{discoverFolder?.FullUrl}] Count:[{folders.LongCount()}]");
                        JobContext.ReportManager.IncreaseBase(folders.LongCount());
                        foreach (var folder in folders)
                        {
                            using (folder)
                            {
                                ProcessFolder(aveList, folder);
                            }
                        }
                    }
                }


            }
            catch (JobStopException)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                logger.Error($"error occurred while Process folder:{discoverFolder?.FullUrl}, ERROR:{e.ToString()}");
                JobContext.HasErrorNode = true;
                JobContext.NodeLevelError = true;
                JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                {
                    ObjectName = discoverFolder?.LeafName,
                    SourceURL = discoverFolder?.FullUrl,
                    Status = JobDetailsStatus.Failed,
                    Comment = GetExceptionMessage(e),
                });
            }

        }

        public void ProcessAveItem(IAveListItem aveItem, CancellationTokenSource cts = null)
        {
            string itemName = string.Empty;
            string itemUrl = string.Empty;
            Guid recordId = Guid.Empty;
            try
            {
                using (var performance = new PerformanceScope("RMEnforceRetentionProcesser.ProcessAveItem", addToStatistics: true))
                {
                    JobContext.ReportManager.Increase();
                    if (aveItem.FileSystemObjectType == AveFileSystemObjectType.Folder)
                    {
                        logger.Info($"Skip folder. Path:[{aveItem.FullPath()}]");
                        return;
                    }
                    InnerProcessAveItem(aveItem, ref recordId, ref itemName, ref itemUrl);
                }
            }
            catch (JobStopException)
            {
                cts?.Cancel();
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                logger.Error($"error occurred while Process aveitem:{itemUrl}, ERROR:{e.ToString()}");
                bool isItemNotFound = this.isItemNotFoundError(e);
                if (!isItemNotFound)
                {
                    JobContext.HasErrorNode = true;
                    this.AddFailureItem2Cache(aveItem, e);
                    JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                    {
                        ObjectName = itemName,
                        SourceURL = itemUrl,
                        Status = JobDetailsStatus.Failed,
                        Comment = GetExceptionMessage(e),
                    });
                }
            }

        }
            
        public virtual void InnerProcessAveItem(IAveListItem aveItem, ref Guid recordId, ref string itemName, ref string itemUrl)
        {
            var siteId = aveItem.ParentList.ParentWeb.Site.ID;  
            var nodeId = aveItem.UniqueId;
            recordId = IDGenerator.GetRecordId(siteId, nodeId);
            JobContext.ReportManager.Increase();
            itemName = aveItem?.GetObjectName();
            itemUrl = aveItem.FullPath();
            logger.Info($"Process item:siteId {siteId} Node Id{nodeId} rowId {aveItem.ID}");
            Guid termId;
            if (RetentionDataCache.Instance.GetProcessedItem(aveItem.UniqueId))
            {
                logger.Info($"Item already processed, item url:siteId {siteId} Node Id{nodeId} rowId {aveItem.ID}");
                return;
            }

            RetentionDataCache.Instance.AddProcessedItem(aveItem.UniqueId);
            using (CheckJobStopScope stopScope = new CheckJobStopScope())
            {
                var obj = aveItem.FieldValues.ContainsKey(RetentionDataCache.Instance.BCSColumnInternalName) ? aveItem.FieldValues[RetentionDataCache.Instance.BCSColumnInternalName] : null;
                if (obj != null)
                {
                    var columnVal = obj.ToString();
                    if (columnVal.Split('|').Length > 1)
                    {
                        var termIdStr = obj.ToString().Split('|')[1];
                        termId = Guid.Parse(termIdStr);
                        TermSettingsInfo termInfo = GetTermInfo(termId);
                        if (termInfo != null)
                        {
                            Guid tempRecordId = recordId;
                            WaitSPOExecuteAction(() =>
                            {
                                if ((termInfo.EnforceRetention & (int)EnforceRetentionType.SharePoint) == (int)EnforceRetentionType.SharePoint)
                                {
                                    ApplyComplianceTag(aveItem, tempRecordId);
                                }
                                else
                                {
                                    RemoveComplianceTag(aveItem, tempRecordId);
                                }
                            });

                        }
                    }
                    else
                    {
                        logger.Info($"invalid term format:{columnVal}, {itemUrl}");
                    }
                }
                else
                {
                    logger.Info($"item does not have bcs column,{RetentionDataCache.Instance.BCSColumnInternalName}, {itemUrl}");
                }
            }
        }

        public virtual void ProcessDiscoverItem(IAveList list, AveDiscoverItem discoverItem, CancellationTokenSource cts = null)
        {
            string itemName = string.Empty;
            string itemUrl = string.Empty;
            Guid recordId = Guid.Empty;
            IAveListItem aveItem = null;
            try
            {
                using (var performance = new PerformanceScope("SP.RMEnforceRetentionProcesser.ProcessItem", addToStatistics: true))
                {
                    JobContext.ReportManager.Increase();
                    if (discoverItem.ID == null || (discoverItem.Hidden != null && discoverItem.Hidden == true))
                    {
                        logger.Info($"skip hidden item:{discoverItem?.FullUrl}");
                        return;
                    }
                    if (discoverItem.ChangeType == Wrapper.Common.ChangeType.Delete)
                    {
                        //try
                        //{
                        //    WaitSPOExecuteAction(() =>
                        //    {
                        //        aveItem = list.GetItemById((int)discoverItem.ID);
                        //    });
                        //}
                        //catch (Exception ex)
                        //{
                        //    logger.Info("skip removed item object {0} : {1} :{2}", DiscoverSite.SiteID, discoverItem.ID, ex.ToString());
                        //}
                        logger.Warn("skip removed view item, {0}", discoverItem?.FullUrl);
                        return;
                    }
                    WaitSPOExecuteAction(() =>
                    {
                        aveItem = list.GetItemById((int)discoverItem.ID);
                    });
                    if (aveItem.FileSystemObjectType == AveFileSystemObjectType.Folder)
                    {
                        logger.Info($"Skip folder. Path:[{aveItem.FullPath()}]");
                        return;
                    }
                    InnerProcessAveItem(aveItem, ref recordId, ref itemName, ref itemUrl);
                }

            }
            catch (JobStopException)
            {
                cts?.Cancel();
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                logger.Error($"error occurred while Process aveitem:{itemUrl}, ERROR:{e.ToString()}");
                bool isItemNotFound = this.isItemNotFoundError(e);
                if (!isItemNotFound)
                {
                    JobContext.HasErrorNode = true;
                    this.AddFailureItem2Cache(aveItem, e);
                    JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                    {
                        ObjectName = itemName,
                        SourceURL = itemUrl,
                        Status = JobDetailsStatus.Failed,
                        Comment = GetExceptionMessage(e),
                    });
                }
            }
            return;
        }

        public virtual void ProcessIncrementalChangedItemV1(IAveList list, IAveListItem aveItem, CancellationTokenSource cts = null)
        {
            string itemName = string.Empty;
            string itemUrl = string.Empty;
            Guid recordId = Guid.Empty;
            try
            {
                using (var performance = new PerformanceScope("RMEnforceRetentionProcesser.ProcessIncrementalChangedItemV1", addToStatistics: true))
                {
                    JobContext.ReportManager.Increase();

                    if (aveItem.FileSystemObjectType == AveFileSystemObjectType.Folder)
                    {
                        logger.Info($"Skip folder. Path:[{aveItem.FullPath()}]");
                        return;
                    }
                    InnerProcessAveItem(aveItem, ref recordId, ref itemName, ref itemUrl);
                }
            }
            catch (JobStopException)
            {
                cts?.Cancel();
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                logger.Error($"error occurred while Process aveitem:{itemUrl}, ERROR:{e.ToString()}");
                bool isItemNotFound = this.isItemNotFoundError(e);
                if (!isItemNotFound)
                {
                    JobContext.HasErrorNode = true;
                    this.AddFailureItem2Cache(aveItem, e);
                    JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                    {
                        ObjectName = itemName,
                        SourceURL = itemUrl,
                        Status = JobDetailsStatus.Failed,
                        Comment = GetExceptionMessage(e),
                    });
                }
            }
            return;
        }
        protected void ProcessFailedItems(IAveList list)
        {
            try
            {
                logger.Info("Start to process failed items in azure table");
                List<SyncFailureItemEntity> failedItems = SyncFailureItemDao.GetAllByDataSource(TenantLocalValue.LogonGroupId, DiscoverSite.SiteID.ToString(), list.ID.ToString(), (int)FailureSourceType.SharePointEnforceRetention);
                int incItemsPerTask = failedItems.Count / 4;
                logger.Info($"Process last failed item count:[{failedItems.Count}].incItemsPerTask:[{incItemsPerTask}]");
                if (failedItems.Count > 0)
                {
                    JobContext.ReportManager.IncreaseBase(failedItems.Count);

                    if (failedItems.Count > itemsPerTask)
                    {
                        var cts = new CancellationTokenSource();
                        //最多起4~5个Task处理Incremental的Changed Item，Full Job Get Item默认2k，因此itemsPerTask固定，但是Incremental items 数量不固定，因此需要按照多个处理。
                        AveTenantTasks.RunParallel(failedItems, incItemsPerTask, cts, failedItem =>
                        {
                            ProcessFailedItem(list, failedItem, cts);
                        });
                    }
                    else
                    {
                        foreach (var failedItem in failedItems)
                        {
                            ProcessFailedItem(list, failedItem);
                        }
                    }
                }
            }
            catch(JobStopException)
            {
                throw new JobStopException("the job has stopped.");
            }
            
        }
        public virtual void ProcessFailedItem(IAveList list, SyncFailureItemEntity failedItem, CancellationTokenSource cts = null)
        {
            string itemName = string.Empty;
            string itemUrl = failedItem?.FullPath; // string.Empty;
            Guid recordId = Guid.Empty;
            IAveListItem aveItem = null;
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope()) 
                {
                    ArgumentCheck.CheckNotNull(failedItem);
                    Guid parentId = new Guid(failedItem.ParentId);
                    using (var performance = new PerformanceScope("RMEnforceRetentionBase.ProcessFailedItem", addToStatistics: true))
                    {
                        JobContext.ReportManager.Increase();
                        int itemId = failedItem.ItemId;
                        logger.Info($"Process failed item:Id:{itemId}, full path:{failedItem.FullPath}.");

                        WaitSPOExecuteAction(() =>
                        {
                            aveItem = list.GetItemById(itemId);
                        });
                        InnerProcessAveItem(aveItem, ref recordId, ref itemName, ref itemUrl);
                        this.RemoveFailureItemFromAzure(failedItem);

                    }
                }
            }
            catch (JobStopException)
            {
                cts?.Cancel();
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                logger.Error($"error occurred while Process aveitem:{itemUrl}, ERROR:{e.ToString()}");
                bool isItemNotFound = this.isItemNotFoundError(e);
                if (!isItemNotFound)
                {
                    JobContext.HasErrorNode = true;
                    JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                    {
                        ObjectName = itemName,
                        SourceURL = itemUrl,
                        Status = JobDetailsStatus.Failed,
                        Comment = GetExceptionMessage(e),
                    });
                    this.AddFailureItem2Cache(aveItem, e);
                }
                else
                {
                    this.RemoveFailureItemFromAzure(failedItem);
                }

            }
            return; 
        }

        protected virtual List<CAMLManager> GetCAMLManager() 
        {
            var changedTermIds = RetentionDataCache.Instance.TermRetentionMapping.Keys.ToList();
            var removeLabelTermIds = RetentionDataCache.Instance.TermRetentionMapping.Where(t => (t.Value.EnforceRetention & (int)EnforceRetentionType.SharePoint) != (int)EnforceRetentionType.SharePoint).ToDictionary(t => t.Key, o => o.Value);
            if (removeLabelTermIds.Count > 0)
            {
                logger.Info("int caml query include remove label action.");
                return CAMLManagerUtil.BuildCAMLMangager(DiscoverSite.SiteID, changedTermIds, RetentionDataCache.Instance.BCSColumnInternalName);
            }
            else 
            {
                logger.Info("int caml query for apply label.");
                return CAMLManagerUtil.BuildCAMLMangagerForRetention(DiscoverSite.SiteID, changedTermIds, RetentionDataCache.Instance.LabelStateInfo.CurrentLabel?.Name, RetentionDataCache.Instance.BCSColumnInternalName);
            }
            
        }
        private void AddFailureItem2Cache(IAveListItem aveItem, Exception e)
        {
            if (this.FailureItems.Count <= 1000)
            {
                RMSPSyncFailureItem failureItem = new RMSPSyncFailureItem()
                {
                    SiteId = DiscoverSite.SiteID.ToString(),
                    ListId = aveItem.ParentList.ID.ToString(),
                    IntemIntId = aveItem.ID,
                    JobId = JobContext.SubJobId,
                    ItemId = aveItem.UniqueId.ToString(),
                    ParentId = aveItem.ParentList.ID.ToString(),
                    WebId = aveItem.ParentList.ParentWeb.ID.ToString(),
                };
                failureItem.URL = aveItem?.Url;
                failureItem.ObjectName = aveItem?.Name;
                failureItem.Message = this.GetExceptionMessage(e);
                this.FailureItems.Add(failureItem);
            }
        }

        private void AddFailureItem2Azure()
        {
            try
            {
                if (this.FailureItems.Count > 0)
                {
                    List<SyncFailureItemEntity> failureEntities = new List<SyncFailureItemEntity>();
                    foreach (RMSPSyncFailureItem item in this.FailureItems)
                    {
                        SyncFailureItemEntity entity = new SyncFailureItemEntity(item.SiteId, item.ItemId);
                        entity.ListId = item.ListId;
                        entity.JobId = item.JobId;
                        entity.ParentId = item.ParentId;
                        entity.WebId = item.WebId;
                        entity.ItemId = item.IntemIntId;
                        entity.FullPath = item.URL;
                        entity.TermId = item.TermId;
                        entity.DataSource = (int)FailureSourceType.SharePointEnforceRetention;
                        failureEntities.Add(entity);
                    }
                    logger.Debug($"Add entity to azure, list count: {failureEntities.Count}");
                    SyncFailureItemDao.Add(TenantLocalValue.LogonGroupId, failureEntities);
                }
            }
            catch (Exception e)
            {
                JobContext.HasErrorNode = true;
                logger.Error(e.Message, e);
            }
        }

        private void RemoveFailureItemFromAzure(SyncFailureItemEntity entity)
        {
            try
            {
                logger.Debug($"Remove entity from azure, list ID: {entity.ListId}, item Id:{entity.ItemId}");
                SyncFailureItemDao.Remove(TenantLocalValue.LogonGroupId, entity);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
        }
        protected virtual TermSettingsInfo GetTermInfo(Guid termId)
        {
            TermSettingsInfo result = null;

            if (!RetentionDataCache.Instance.TermRetentionMapping.TryGetValue(termId, out result))
            {
                var tempTerm = TermDao.GetParentInhertSetting(termId);
                if (tempTerm != null)
                {
                    result = new TermSettingsInfo() { EnforceRetention = tempTerm.EnforceRetention };
                    RetentionDataCache.Instance.AddTermRetentionObj(termId, result);
                }
                else
                {
                    logger.Warn($"item term not exist in db:{termId}");
                    //throw new Exception($"term cannot be found, termId:{termId}");
                }
            }
            return result;
        }

        /// <summary>
        /// check bcs column, reset internal name(existing column)
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        protected virtual bool HasBCSColumn(IAveList list)
        {
            bool result = true;
            try
            {
                if (!list.Fields.ContainsFieldWithInternalName(RetentionDataCache.Instance.BCSColumnInternalName))
                {
                    if (RetentionDataCache.Instance.BCSColumnInternalName != RcordsBuiltInColumn.ITEM_BCS_NAME)
                    {
                        //existing column reset internal name
                        var bcsColumn = list.Fields.GetFieldById(RetentionDataCache.Instance.BCSColumnID, false);
                        if (bcsColumn != null)
                        {
                            RetentionDataCache.Instance.BCSColumnInternalName = bcsColumn.InternalName;
                            logger.Info($"reset list bcs column, list:{list.RootFolder?.ServerRelativeUrl}, column name:{RetentionDataCache.Instance.BCSColumnInternalName}");
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

        protected virtual void ApplyComplianceTag(IAveListItem item, Guid recordId)
        {
            using (var performance = new PerformanceScope("RMEnforceRetentionProcesser.ApplyLabel", addToStatistics: true))
            {
                var processingLabelName = RetentionDataCache.Instance.LabelStateInfo.CurrentLabel.Name;
                var previousLabelNames = RetentionDataCache.Instance.LabelStateInfo.PreviousLabelNames;
                AveComplianceTagInfo tagInfo = null;
                var itemUrl = item.FullPath();
                var currentLabel = item.GetComplianceTagName().ToLower();
                if (IsCurrentLabelLocked(item, itemUrl, currentLabel, true))
                {
                    return;
                }
                var needApplyLabel = string.IsNullOrEmpty(currentLabel) || previousLabelNames.Count > 0 && previousLabelNames.Contains(currentLabel) && !currentLabel.Equals(processingLabelName, StringComparison.OrdinalIgnoreCase);
                //only overwrite tag of retention setting label
                logger.Info($"ApplyComplianceTag:RowId {item.ID} processingLabelName:{processingLabelName}, currentLabel:{currentLabel}.");
                if (needApplyLabel)
                {
                    if (RetentionDataCache.Instance.SPSiteRetentionLables.TryGetValue(processingLabelName, out tagInfo))
                    {
                        using (var performance1 = new PerformanceScope("SP.RMEnforceRetentionProcesser.ApplyComplianceTag", addToStatistics: true))
                        {
                            //item.SetComplianceTag(tagInfo.TagName, tagInfo.BlockDelete, tagInfo.BlockEdit, tagInfo.IsEventTag, tagInfo.SuperLock);
                            item.SetComplianceTagOnBulkItems(tagInfo.TagName);
                        }

                        needUpdateLabelState = true;
                        logger.Info($"add item label:{processingLabelName}, Item RowId:{item.ID}");
                        JobContext.HasSuccessNode = true;
                        using (var performance2 = new PerformanceScope("RMEnforceRetentionProcesser.SendReport", addToStatistics: true))
                        {
                            JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                            {
                                ObjectName = item.GetObjectName(),
                                SourceURL = itemUrl,
                                Action = "RM_EXO_EnforceRetention_TagLabel",
                                Status = JobDetailsStatus.Successful,
                            });
                            JobContext.HasSuccessNode = true;
                        }
                    }
                    else
                    {
                        logger.Error($"SPLabel cannot be found:{processingLabelName}");
                        JobContext.HasErrorNode = true;
                        JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                        {
                            ObjectName = item.GetObjectName(),
                            SourceURL = itemUrl,
                            Status = JobDetailsStatus.Failed,
                            Action = "RM_EXO_EnforceRetention_TagLabel",
                            Comment = $"RM_JS_JM_EnforceRetention_LabelNotFound|I18NSplit|{processingLabelName}",
                        });
                        //throw new Exception($"Label cannot be found, label name:{processingLabelName}");
                    }
                }
                else
                {
                    logger.Info($"skip item:Row Id {item.ID}, compliance tag:{processingLabelName} already exist.");
                    if (!previousLabelNames.Contains(currentLabel))
                    {
                        JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                        {
                            ObjectName = item.GetObjectName(),
                            SourceURL = itemUrl,
                            Status = JobDetailsStatus.Skipped,
                            Action = "RM_EXO_EnforceRetention_TagLabel",
                            Comment = $"RM_JS_JM_EnforceRetention_LabelAlreadyExist|I18NSplit|{processingLabelName}",
                        });
                        JobContext.HasSuccessNode = true;
                    }
                }
            }
        }


        protected virtual void RemoveComplianceTag(IAveListItem item, Guid recordId)
        {
            using (var performance = new PerformanceScope("RMEnforceRetentionProcesser.RemoveLabel", addToStatistics: true))
            {
                var processingLabelName = RetentionDataCache.Instance.LabelStateInfo.CurrentLabel.Name;
                var previousLabelNames = RetentionDataCache.Instance.LabelStateInfo.PreviousLabelNames;
                var itemUrl = item.FullPath();
                var currentLabel = item.GetComplianceTagName().ToLower();
                if (IsCurrentLabelLocked(item, itemUrl, currentLabel))
                {
                    return;
                }
                var needRemoveLabel = !string.IsNullOrEmpty(currentLabel) && previousLabelNames.Contains(currentLabel);
                logger.Info($"RemoveComplianceTag:RowId {item.ID} processingLabelName:{processingLabelName}, currentLabel:{currentLabel}.");
                //only remove tag of retention setting label
                if (needRemoveLabel)
                {
                    using (var performance1 = new PerformanceScope("RMEnforceRetentionProcesser.RemoveComplianceTag", addToStatistics: true))
                    {
                        //item.SetComplianceTag(null, false, false, false, false);
                        item.SetComplianceTagOnBulkItems(string.Empty);
                    }
                    logger.Info($"remove item label:{currentLabel}, ItemRowId:{item.ID}");
                    needUpdateLabelState = true;
                    JobContext.HasSuccessNode = true;
                    using (var performance2 = new PerformanceScope("RMEnforceRetentionProcesser.SendReport", addToStatistics: true))
                    {
                        JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                        {
                            ObjectName = item.GetObjectName(),
                            SourceURL = itemUrl,
                            Action = "RM_EXO_EnforceRetention_RemoveLabel",
                            Status = JobDetailsStatus.Successful,
                        });
                        JobContext.HasSuccessNode = true;
                    }
                }
                else
                {
                    logger.Info($"skip item:RowId {item.ID}, compliance tag:current:{currentLabel}.");
                    if (!previousLabelNames.Contains(currentLabel)){
                        JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                        {
                            ObjectName = item.GetObjectName(),
                            SourceURL = itemUrl,
                            Action = "RM_EXO_EnforceRetention_RemoveLabel",
                            Status = JobDetailsStatus.Skipped,
                            Comment = $"RM_JS_JM_EnforceRetention_LabelNoNeedRemove|I18NSplit|{currentLabel}"
                        });
                        JobContext.HasSuccessNode = true;
                    }
                }
            }
        }

        protected virtual async System.Threading.Tasks.Task UpdateLabelStatusAsync()
        {
            var label = RetentionDataCache.Instance.LabelStateInfo.CurrentLabel;
            var dbLabel = LabelDao.GetLabel((int)RMRetentionSourceType.SharePoint, (int)RMRetentionLabelStatus.JobProcessing);
            //清理旧的失败数据,正常应该只有一条
            LabelDao.RemoveOldFaildLabel((int)RMRetentionSourceType.SharePoint);
            if (dbLabel == null)
            {
                var tempLabel = new RMEXOLabel();
                tempLabel.LabelName = label.Name;
                tempLabel.Status = (int)RMRetentionLabelStatus.JobProcessing;
                tempLabel.Type = (int)RMRetentionSourceType.SharePoint;
                tempLabel.LabelId = label.LabelId;
                tempLabel.SavedTime = DateTime.UtcNow.Ticks;
                LabelDao.Create(tempLabel);
            }
            else
            {
                dbLabel.LabelName = label.Name;
                dbLabel.LabelId = label.LabelId;
                dbLabel.SavedTime = DateTime.UtcNow.Ticks;
                await LabelDao.UpdateAsync(dbLabel);
            }
        }


        private bool isItemNotFoundError(Exception e)
        {
            if (e != null && e.Message != null && e.Message.Contains("Item does not exist"))
            {
                return true;
            }
            ArgumentCheck.CheckNotNull(e);
            if (e?.InnerException != null)
            {
                return isItemNotFoundError(e.InnerException);
            }
            return false;
        }
        private async System.Threading.Tasks.Task FinallyUpdateAsync()
        {
            try
            {
                //更新label状态
                if (needUpdateLabelState)
                {
                    await UpdateLabelStatusAsync();
                }

            }
            catch (Exception ex)
            {
                logger.Error($"update label faild:{ex.ToString()}");
            }

        }

        protected virtual async System.Threading.Tasks.Task CheckLabelExistAndThrowExceptionAsync()
        {
            var processingLabelName = RetentionDataCache.Instance.LabelStateInfo.CurrentLabel.Name;
            if (!RetentionDataCache.Instance.SPSiteRetentionLables.TryGetValue(processingLabelName, out AveComplianceTagInfo tagInfo))
            {
                logger.Warn($"label not exist:{processingLabelName}");
                await JobContext.MonitorExcetionAsync(Contract.Monitor.MonitorExceptionType.LabelNotFound);
                throw new LabelNotExistException($"The label cannot be found, label name: {processingLabelName}");
            }
        }

        protected virtual bool IsCurrentLabelLocked(IAveListItem item, string itemUrl, string currentLabel, bool isApply = false)
        {
            if (!string.IsNullOrEmpty(currentLabel))
            {
                var currentLabelInfo = item.GetComplianceInfo();
                if (currentLabelInfo != null && currentLabelInfo.TagPolicyHold && currentLabelInfo.TagPolicyRecord && IsRecordTypeComplianceTag(item.Web.Site, currentLabelInfo.ComplianceTag))
                {
                    logger.Info($"ApplyComplianceTag:RowId {item.ID}, currentLabel:{currentLabel} is a locked record rentention label. Skip this item");
                    JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                    {
                        ObjectName = item.GetObjectName(),
                        SourceURL = itemUrl,
                        Status = JobDetailsStatus.Skipped,
                        Action = isApply ? "RM_EXO_EnforceRetention_TagLabel" : "RM_EXO_EnforceRetention_RemoveLabel",
                        Comment = $"RM_JS_JM_EnforceRetention_CurrentLabelLocked|I18NSplit|{currentLabel}",
                    });
                    return true;
                }
            }

            return false;
        }

        protected bool IsRecordTypeComplianceTag(IAveSite site, string complianceTagName)
        {
            try
            {
                if (RetentionDataCache.Instance.SPSiteRetentionLables.TryGetValue(complianceTagName, out AveComplianceTagInfo info))
                {
                    if (info.BlockDelete && info.BlockEdit)
                    {
                        return true;
                    }
                }
                else
                {
                    logger.Warn($"Unable get complianceTag info from site avaliable compliance tags by tag name:{complianceTagName}, site url:{site.Url}");
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.Error($"Fail get complianceTag info from site avaliable compliance tags by tag name:{complianceTagName}, site url:{site.Url}, ex:{ex}");
                throw;
            }
        }

    }
}
