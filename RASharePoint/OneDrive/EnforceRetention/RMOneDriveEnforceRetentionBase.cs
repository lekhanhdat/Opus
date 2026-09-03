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
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.EnforceRetention.Cache;
using AvePoint.RA.SharePoint.SPObjDiscover;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Common.Throttle;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.SharePoint.OneDrive.EnforceRetention.Cache;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.Global.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.SharePoint.OneDrive.Discover.Base;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Contract.Configurations;
using Aspose.Email.Storage.Pst;

namespace AvePoint.RA.SharePoint.OneDrive.EnforceRetention
{
    public class RMOneDriveEnforceRetentionBase : RMSPDiscoverBase
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMOneDriveEnforceRetentionBase));
        private ISPDiscover mDiscover = null;
        //get change item by spquery
        private List<AveCamlQuery> mCamlQueries = null;
        private bool needUpdateLabelState = false;
        private static int _itemsPerTask = 500;
        private static int _queryCosmosDBPageSize = 2000;
        protected int itemsPerTask
        {
            get
            {
                return _itemsPerTask;
            }
        }

        private static CallLimiter _spoCallLimiter;

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
        private RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao(true);
                }
                return _explorerDao;
            }
        }

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

        public RMOneDriveEnforceRetentionBase(AveDiscoverSite discoverSite, SPTreeNodeDto treeNode, JobContext jobContext)
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

        public void Init(ISPDiscover sPDiscover, List<AveCamlQuery> camlQueries)
        {
            mDiscover = sPDiscover;
            mCamlQueries = camlQueries;
        }

        public override async System.Threading.Tasks.Task RunNowAsync()
        {
            try
            {
                using (var performance = new PerformanceScope("RMOneDriveEnforceRetentionProcesser.RunNow"))
                {
                    using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        ThrowUtil.ThrowIfNull(DiscoverSite, $"Discover Site is null:{TreeNode?.FullPath}");
                        var webs = mDiscover.GetWebs(DiscoverSite);
                        JobContext.ReportManager.IncreaseBase(webs.LongCount());
                        foreach (var web in webs)
                        {
                            using (CheckJobStopScope jScope = new CheckJobStopScope())
                            {
                                await ProcessWebAsync(web);
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
        private string GetExceptionMessage(Exception e)
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
                using (var performance = new PerformanceScope("RMOneDriveEnforceRetentionProcesser.ProcessWeb", addToStatistics: true))
                {
                    logger.Info($"Process web:{discoverWeb?.FullUrl}");
                    
                    ArgumentCheck.CheckNotNull(discoverWeb);
                    JobContext.ReportManager.Increase();
                    if (discoverWeb?.ChangeType == Wrapper.Common.ChangeType.Delete)
                    {
                        logger.Info("skip removed web object {0} : {1}", DiscoverSite.SiteID, discoverWeb.WebID);
                        return;
                    }
                    var lists = mDiscover.GetLists(discoverWeb);
                    JobContext.ReportManager.IncreaseBase(lists.LongCount());
                    foreach (var list in lists)
                    {
                        using (CheckJobStopScope jScope = new CheckJobStopScope())
                        {
                            //ProcessList(list, discoverWeb.WebID);
                            await ProcessListV1Async(list);
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

        protected virtual bool CanProcessList(AveDiscoverList discoverList)
        {
            if (discoverList.ChangeType == Wrapper.Common.ChangeType.Delete)
            {
                logger.Info("skip removed list object {0}", discoverList?.ListId);
                return false;
            }
            if (discoverList.Name.Equals("{System Folder}"))
            {
                logger.Info("Skip the system list {0}", string.IsNullOrEmpty(discoverList?.RootFolderUrl) ? discoverList?.Name : discoverList?.RootFolderUrl);
                return false;
            }
            if (CheckIsDesignList(discoverList))
            {
                logger.Info("Skip the design list {0}", string.IsNullOrEmpty(discoverList?.RootFolderUrl) ? discoverList?.Name : discoverList?.RootFolderUrl);
                return false;
            }

            return true;
        }

        protected virtual ExplorerQueryV2Dto GetFilterOption(IAveList discoverList)
        {
            var termIds = OneDriveRetentionDataCache.Instance.TermRetentionMapping
                .Where(o => !string.IsNullOrEmpty(o.Value.OneDriveRetentionLabel))
                .Select(o => o.Key).ToList();
            return RMOneDriveQueryHelper.GetListQueryDto(discoverList.ParentWeb.Site.ID, discoverList.ID, termIds, _queryCosmosDBPageSize);
        }

        protected virtual void ProcessRecords(IAveList list, IEnumerable<Record> records)
        {
            try 
            {
                logger.Info($"Process records under List Url {list.RootFolder.Url} records count:[{records.LongCount()}]");
                JobContext.ReportManager.IncreaseBase(records.Count());

                var existingItemIds = records.Select(r => r.ItemRowId).ToList();
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
                                ProcessAveItemV1(changedItem, records.Where(r => r.ItemRowId == changedItem.ID).First().TermId, cts);
                            });
                        }
                        else
                        {
                            foreach (var changedItem in items)
                            {
                                ProcessAveItemV1(changedItem, records.Where(r => r.ItemRowId == changedItem.ID).First().TermId);
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

        /// <summary>
        /// get records from Cosmos DB first, then update SPO item
        /// </summary>
        /// <param name="discoverList"></param>
        /// <param name="webId"></param>
        public virtual async System.Threading.Tasks.Task ProcessListV1Async(AveDiscoverList discoverList)
        {
            string listPath = string.Empty;
            try
            {
                using (var performance = new PerformanceScope("RMOneDriveEnforceRetentionProcesser.ProcessListV1", $"RMOneDriveEnforceRetentionProcesser.ProcessListV1 Path:[{discoverList?.RootFolderUrl}]", true))
                {
                    logger.Info($"Process list:{discoverList?.RootFolderUrl}");
                    using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        JobContext.ReportManager.Increase();
                        if (!CanProcessList(discoverList)) return;
                        ArgumentNullException.ThrowIfNull(discoverList);
                        var list = discoverList.GetListObject();
                        listPath = WebUtil.MakeFullUrl(list.ParentWeb.Url, list.RootFolder.Url);
                        //1. get query
                        //2. query db
                        var explorerQueryV2Dto = GetFilterOption(list);

                        do
                        {
                            Tuple<IEnumerable<Record>, string> rt;
                            using (var performance0 = new PerformanceScope("RMOneDriveEnforceRetentionProcesser.SearchRecordsV2", addToStatistics: true))
                            {
                                rt = ExplorerDao.SearchRecordsV2(explorerQueryV2Dto);
                            }
                            var result = rt.Item1;
                            if (result != null && result.Count() > 0)
                            {
                                await CheckLabelExistAndThrowExceptionAsync();
                                ProcessRecords(list, result);
                            }
                            explorerQueryV2Dto.PagingInfo.PageIndex = rt.Item2;
                            explorerQueryV2Dto.PagingInfo.HasNextPage = !string.IsNullOrEmpty(rt.Item2);

                        }
                        while (explorerQueryV2Dto.PagingInfo.HasNextPage);
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

        public void InnerProcessAveItem(IAveListItem aveItem, ref Guid recordId, ref string itemName, ref string itemUrl)
        {
            var siteId = aveItem.ParentList.ParentWeb.Site.ID;
            var nodeId = aveItem.UniqueId;
            recordId = IDGenerator.GetRecordId(siteId, nodeId);
            JobContext.ReportManager.Increase();
            itemName = aveItem?.GetObjectName();
            itemUrl = aveItem.FullPath();
            logger.Info($"Process item:siteId {siteId} Node Id{nodeId} rowId {aveItem.ID}");
            Guid termId;
            if (OneDriveRetentionDataCache.Instance.GetProcessedItem(aveItem.UniqueId))
            {
                logger.Info($"Item already processed, item url:siteId {siteId} Node Id{nodeId} rowId {aveItem.ID}");
                return;
            }
            OneDriveRetentionDataCache.Instance.AddProcessedItem(aveItem.UniqueId);
            using (CheckJobStopScope stopScope = new CheckJobStopScope())
            {
                Record recordInDB = null;
                var itemId = recordId;
                WaitSPOExecuteAction(() =>
                {
                    recordInDB = ExplorerDao.ReadById(siteId, itemId);
                });
                if (recordInDB != null)
                {
                    var columnVal = recordInDB.TermId.ToString();
                    if (!string.IsNullOrEmpty(columnVal))
                    {
                        termId = Guid.Parse(columnVal);
                        TermSettingsInfo termInfo = GetTermInfo(termId);
                        
                        if (termInfo != null)
                        {
                            Guid tempRecordId = recordId;
                            WaitSPOExecuteAction(() =>
                            {
                                logger.Info($"Process item:termInfo EnforceRetention: {termInfo.EnforceRetention}.");
                                if ((termInfo.EnforceRetention & (int)EnforceRetentionType.OneDrive) == (int)EnforceRetentionType.OneDrive)
                                {
                                    ApplyComplianceTag(aveItem, tempRecordId);
                                }
                                else
                                {
                                    RemoveComplianceTag(aveItem, tempRecordId);
                                }
                            });
                        }
                        else
                        {
                            logger.Info($"TermSettingsInfo is null:columnVal:{columnVal}, itemUrl:{itemUrl}");
                        }
                    }
                    else
                    {
                        logger.Info($"invalid term format:{columnVal}, {itemUrl}");
                    }
                }
                else
                {
                    logger.Info($"item does not exist in recordsdb,{OneDriveRetentionDataCache.Instance.BCSColumnName}, {itemUrl}");
                }
            }
        }

        public void ProcessAveItemV1(IAveListItem aveItem, Guid termId, CancellationTokenSource cts = null)
        {
            if (aveItem == null) return;
            string itemName = string.Empty;
            string itemUrl = string.Empty;
            Guid recordId = Guid.Empty;
            try
            {
                using (var performance = new PerformanceScope("RMOneDriveEnforceRetentionProcesser.ProcessAveItemV1", addToStatistics: true))
                {
                    JobContext.ReportManager.Increase();
                    if (aveItem.FileSystemObjectType == AveFileSystemObjectType.Folder)
                    {
                        logger.Info($"Skip folder. Path:[{aveItem.FullPath()}]");
                        return;
                    }
                    InnerProcessAveItemV1(aveItem, termId, ref recordId, ref itemName, ref itemUrl);
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
                	JobContext.NodeLevelError = true;
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

        public void InnerProcessAveItemV1(IAveListItem aveItem, Guid termId, ref Guid recordId, ref string itemName, ref string itemUrl)
        {
            try 
            {
                var siteId = aveItem.ParentList.ParentWeb.Site.ID;
                var nodeId = aveItem.UniqueId;
                recordId = IDGenerator.GetRecordId(siteId, nodeId);
                JobContext.ReportManager.Increase();
                itemName = aveItem?.GetObjectName();
                itemUrl = aveItem.FullPath();
                logger.Info($"Process item V1:siteId {siteId} Node Id{nodeId} rowId {aveItem.ID}");
                if (OneDriveRetentionDataCache.Instance.GetProcessedItem(aveItem.UniqueId))
                {
                    logger.Info($"Item already processed, item url:siteId {siteId} Node Id{nodeId} rowId {aveItem.ID}");
                    return;
                }
                OneDriveRetentionDataCache.Instance.AddProcessedItem(aveItem.UniqueId);
                using (CheckJobStopScope stopScope = new CheckJobStopScope())
                {
                    TermSettingsInfo termInfo = GetTermInfo(termId);

                    if (termInfo != null)
                    {
                        Guid tempRecordId = recordId;
                        UpdateItemTag(aveItem, termInfo, tempRecordId);
                    }
                    else
                    {
                        logger.Info($"TermSettingsInfo is null:columnVal:{termId}, itemUrl:{itemUrl}");
                    }
                }
            }
            catch (JobStopException)
                            {
                throw new JobStopException("This Job is stopped.");
            }
        }

        private void UpdateItemTag(IAveListItem aveItem, TermSettingsInfo termInfo, Guid recordId)
        {
            WaitSPOExecuteAction(() =>
            {
                logger.Info($"Process item:termInfo EnforceRetention: {termInfo.EnforceRetention}.");
                if ((termInfo.EnforceRetention & (int)EnforceRetentionType.OneDrive) == (int)EnforceRetentionType.OneDrive)
                {
                    ApplyComplianceTag(aveItem, recordId);
                }
                else
                {
                    RemoveComplianceTag(aveItem, recordId);
                }
            });
        }

        private TermSettingsInfo GetTermInfo(Guid termId)
        {
            TermSettingsInfo result = null;

            if (!OneDriveRetentionDataCache.Instance.TermRetentionMapping.TryGetValue(termId, out result))
            {
                var tempTerm = TermDao.GetParentInhertSetting(termId);
                if (tempTerm != null)
                {
                    result = new TermSettingsInfo() { EnforceRetention = tempTerm.EnforceRetention };
                    OneDriveRetentionDataCache.Instance.AddTermRetentionObj(termId, result);
                }
                else
                {
                    logger.Warn($"item term not exist in db:{termId}");
                    //throw new Exception($"term cannot be found, termId:{termId}");
                }
            }
            return result;
        }

        private async System.Threading.Tasks.Task CheckLabelExistAndThrowExceptionAsync()
        {
            var processingLabelName = OneDriveRetentionDataCache.Instance.LabelStateInfo.CurrentLabel.Name;
            if (!OneDriveRetentionDataCache.Instance.SPSiteRetentionLables.TryGetValue(processingLabelName, out AveComplianceTagInfo tagInfo))
            {
                logger.Warn($"label not exist:{processingLabelName}");
                await JobContext.MonitorExcetionAsync(Contract.Monitor.MonitorExceptionType.LabelNotFound);
                throw new LabelNotExistException($"label not exist: {processingLabelName}");
            }
        }
        private void ApplyComplianceTag(IAveListItem item, Guid recordId)
        {
            using (var performance = new PerformanceScope("RMOneDriveEnforceRetentionProcesser.ApplyLabel", addToStatistics: true))
            {
                var processingLabelName = OneDriveRetentionDataCache.Instance.LabelStateInfo.CurrentLabel.Name;
                var previousLabelNames = OneDriveRetentionDataCache.Instance.LabelStateInfo.PreviousLabelNames;
                AveComplianceTagInfo tagInfo = null;
                var itemUrl = item.FullPath();
                var currentLabel = item.GetComplianceTagName().ToLower();
                if (IsCurrentLabelLocked(item, itemUrl, currentLabel, true))
                {
                    return;
                }
                var needApplyLabel = previousLabelNames.Count > 0 && previousLabelNames.Contains(currentLabel) && !currentLabel.Equals(processingLabelName, StringComparison.OrdinalIgnoreCase);
                //only overwrite tag of retention setting label
                var itemAppliedLabel = item.ExistComplianceTag();
                logger.Info($"ApplyComplianceTag:RowId {item.ID} processingLabelName:{processingLabelName}..currentLabel:{currentLabel}.ExistComplianceTag:{itemAppliedLabel}.");
                if (!itemAppliedLabel || needApplyLabel)
                {
                    if (OneDriveRetentionDataCache.Instance.SPSiteRetentionLables.TryGetValue(processingLabelName, out tagInfo))
                    {
                        using (var performance1 = new PerformanceScope("RMEnforceRetentionProcesser.ApplyComplianceTag", addToStatistics: true))
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
                        }
                    }
                    else
                    {
                        logger.Error($"SPLabel cannot be found:{processingLabelName}");
                        JobContext.HasErrorNode = true;
                        JobContext.NodeLevelError = true;
                        JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                        {
                            ObjectName = item.GetObjectName(),
                            SourceURL = itemUrl,
                            Status = JobDetailsStatus.Failed,
                            Action = "RM_EXO_EnforceRetention_TagLabel",
                            Comment = $"RM_JS_JM_EnforceRetention_LabelNotFound|I18NSplit|{processingLabelName}",
                        });
                    }
                }
                else
                {
                    logger.Info($"skip item:Row Id {item.ID}, compliance tag:{processingLabelName} already exist.");
                }
            }
        }

        private void RemoveComplianceTag(IAveListItem item, Guid recordId)
        {
            using (var performance = new PerformanceScope("RMOneDriveEnforceRetentionProcesser.RemoveLabel", addToStatistics: true))
            {
                var itemAppliedLabel = item.ExistComplianceTag();
                if (itemAppliedLabel)
                {
                    var processingLabelName = OneDriveRetentionDataCache.Instance.LabelStateInfo.CurrentLabel.Name;
                    var previousLabelNames = OneDriveRetentionDataCache.Instance.LabelStateInfo.PreviousLabelNames;
                    var itemUrl = item.FullPath();
                    var currentLabel = item.GetComplianceTagName().ToLower();
                    if (IsCurrentLabelLocked(item, itemUrl, currentLabel))
                    {
                        return;
                    }
                    var needRemoveLabel = previousLabelNames.Contains(currentLabel);

                    logger.Info($"RemoveComplianceTag:RowId {item.ID} processingLabelName:{processingLabelName}.currentLabel:{currentLabel}.ExistComplianceTag:{itemAppliedLabel}.");
                    //only remove tag of retention setting label
                    if (itemAppliedLabel && needRemoveLabel)
                    {
                        using (var performance1 = new PerformanceScope("RMEnforceRetentionProcesser.RemoveComplianceTag", addToStatistics: true))
                        {
                            //item.SetComplianceTag(null, false, false, false, false);
                            item.SetComplianceTagOnBulkItems(string.Empty);
                        }
                        logger.Info($"remove item label:{currentLabel}, ItemRowId:{item.ID}");
                        needUpdateLabelState = true;
                        using (var performance2 = new PerformanceScope("RMEnforceRetentionProcesser.SendReport", addToStatistics: true))
                        {
                            JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                            {
                                ObjectName = item.GetObjectName(),
                                SourceURL = itemUrl,
                                Action = "RM_EXO_EnforceRetention_RemoveLabel",
                                Status = JobDetailsStatus.Successful,
                            });
                        }
                    }
                    else
                    {
                        logger.Info($"skip item:RowId {item.ID}, compliance tag:current:{currentLabel}.");
                    }
                }
                else
                {
                    logger.Info($"skip item:RowId {item.ID}, item doesn't have a label");
                }
            }
        }

        private async System.Threading.Tasks.Task UpdateLabelStatusAsync()
        {
            var label = OneDriveRetentionDataCache.Instance.LabelStateInfo.CurrentLabel;
            var dbLabel = LabelDao.GetLabel((int)RMRetentionSourceType.OneDrive, (int)RMRetentionLabelStatus.JobProcessing);
            //清理旧的失败数据,正常应该只有一条
            LabelDao.RemoveOldFaildLabel((int)RMRetentionSourceType.OneDrive);
            if (dbLabel == null)
            {
                var tempLabel = new RMEXOLabel();
                tempLabel.LabelName = label.Name;
                tempLabel.Status = (int)RMRetentionLabelStatus.JobProcessing;
                tempLabel.Type = (int)RMRetentionSourceType.OneDrive;
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
                if (OneDriveRetentionDataCache.Instance.SPSiteRetentionLables.TryGetValue(complianceTagName, out AveComplianceTagInfo info))
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
