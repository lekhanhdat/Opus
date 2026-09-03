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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Common.Throttle;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Bulk;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.UniqueId;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Discover;
using AvePoint.RA.SharePoint.ExplorerSync.Cache;
using AvePoint.RA.SharePoint.ExplorerSync.Modes;
using AvePoint.RA.SharePoint.ExplorerSync.Report;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.SharePoint.Object;
using AvePoint.RA.SharePoint.OneDrive.OneDriveExplorerSync;
using AvePoint.RA.SharePoint.OneDriveExplorerSync.Cache;
using AvePoint.RA.SharePoint.OneDriveExplorerSync.Report;
using AvePoint.RA.SharePoint.OneDriveExplorerSync.Utils;
using AvePoint.RA.SharePoint.RMSharePointColumn;
using AvePoint.RA.SharePoint.SPObjDiscover;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ServerFilterPolicy = AvePoint.GCommon.Contract.Server.Common.Profile.Object;

namespace AvePoint.RA.SharePoint.OneDriveExplorerSync
{
    public class RMOneDriveExplorerBase : RMSPDiscoverBase
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(RMOneDriveExplorerBase));
        private ISPDiscover mDiscover = null;
        private RMSPExplorerSiteLevelCache _siteCache = null;
        private RMOneDriveSyncItem syncItem = null;
        private static CallLimiter _spoCallLimiter;
        private static CallLimiter _cosmosCallLimiter;
        private static int _itemsPerTask = 400;  //Threshold降到2000， 这里降到400
        private bool _isCosmosBulkOperationEnabled = false; //是否开启了批量插入数据到cosmos db
        private bool _forceUpdate = false;
        private bool _isSyncStubFile = false;
        protected int itemsPerTask
        {
            get
            {
                return _itemsPerTask;
            }
        }

        private List<Guid> unSuccessList = new List<Guid>();
        private readonly object unSuccessListLock = new object();
        private string containerId = string.Empty;
        private RMOneDriveSetting mSiteLevelSetting;
        private Guid mSettingSiteId = Guid.Empty;
        private AveObjectModelFactory mAveObjectModelFactory;
        private IAveSite mAveSite;
        private bool needUpdateLabelState = false;
        private long mLastScanTime = DateTime.MinValue.Ticks;
        private long mMainJobStartTime = DateTime.MinValue.Ticks;
        private List<RMOneDriveSetting> mAllSettingsUnderSite;
        private SPDiscoverType mDiscoverType = SPDiscoverType.Full;
        private List<RMSPSyncFailureItem> FailureItems = new List<RMSPSyncFailureItem>();
        private Dictionary<Guid, RMRule> mRuleCache = new Dictionary<Guid, RMRule>();
        private static int smartAutoCacheitemsPerTask = 50;
        #region castle properties
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
        private ITermDao mTermDao;
        protected ITermDao TermDao
        {
            get
            {
                if (mTermDao == null)
                {
                    mTermDao = (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));
                }
                return mTermDao;
            }
        }
        private IRMChangeClassificationDao mRMChangeClassification;
        protected IRMChangeClassificationDao RMChangeClassificationDao
        {
            get
            {
                if (mRMChangeClassification == null)
                {
                    mRMChangeClassification = (IRMChangeClassificationDao)PlatformWindsorManager.GetService(typeof(IRMChangeClassificationDao));
                }
                return mRMChangeClassification;
            }

        }
        private IRMNodeFlagDao _rMNodeFlagDao;
        public IRMNodeFlagDao RMNodeFlagDao
        {
            get { return _rMNodeFlagDao ?? (IRMNodeFlagDao)PlatformWindsorManager.GetService(typeof(IRMNodeFlagDao)); }
            set { _rMNodeFlagDao = value; }
        }
        private IRMScopeDao _rmScopeDao;
        public IRMScopeDao RMScopeDao
        {
            get
            {
                if (_rmScopeDao == null)
                {
                    _rmScopeDao = (IRMScopeDao)PlatformWindsorManager.GetService(typeof(IRMScopeDao));
                }
                return _rmScopeDao;
            }
        }
        public IRMManualApproveDao RMManualApproveDao { set; get; } = PlatformWindsorManager.GetService<IRMManualApproveDao>();
        public IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private IOneDriveSettingDao mOneDriveSettingDao;
        protected IOneDriveSettingDao OneDriveSettingDao
        {
            get
            {
                if (mOneDriveSettingDao == null)
                {
                    mOneDriveSettingDao = (IOneDriveSettingDao)PlatformWindsorManager.GetService(typeof(IOneDriveSettingDao));
                }
                return mOneDriveSettingDao;
            }
        }
        protected static IRMReportManager ReportManager
        {
            get
            {
                return ReportMangerFactory.Instance.ReportManager;
            }
        }

        private IRMEXOLabelDao _labelDao;
        public IRMEXOLabelDao LabelDao
        {
            get { return _labelDao ?? (IRMEXOLabelDao)PlatformWindsorManager.GetService(typeof(IRMEXOLabelDao)); }
            set { _labelDao = value; }
        }

        public ISyncFailureItemDao SyncFailureItemDao { set; get; } = PlatformWindsorManager.GetService<ISyncFailureItemDao>();
        private IRMRuleDao mRuleDao;
        public IRMRuleDao RuleDao
        {
            get { return mRuleDao ?? (IRMRuleDao)PlatformWindsorManager.GetService(typeof(IRMRuleDao)); }
            set { mRuleDao = value; }
        }
        private static readonly IRMKeyValueDao s_keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
        #endregion

        public RMOneDriveExplorerBase(AveDiscoverSite discoverSite, SPTreeNodeDto treeNode, JobContext jobContext, RMOneDriveSetting setting, Guid settingSiteId, IAveSite aveSite, AveObjectModelFactory aveObjectModelFactory, long lastScanTime, long mainJobStartTime, List<RMOneDriveSetting> allSettingsUnderSite)
            : base(discoverSite, treeNode, jobContext)
        {
            var siteId = DiscoverSite.SiteID.ToString();
            _siteCache = RMOneDriveExplorerDataCache.Instance.SiteLevelCache[siteId];
            syncItem = new RMOneDriveSyncItem(_siteCache);
            mSiteLevelSetting = setting;
            mSettingSiteId = settingSiteId;
            mAveObjectModelFactory = aveObjectModelFactory;
            mAveSite = aveSite;
            mLastScanTime = lastScanTime;
            mMainJobStartTime = mainJobStartTime;
            mAllSettingsUnderSite = allSettingsUnderSite;
            var numSetting = RMGlobalConfiguration.AppConfig[RMAppSettingKey.SPO_SYNC_DATA_ITEMS_PER_TASK];
            if (!string.IsNullOrEmpty(numSetting))
            {
                int.TryParse(numSetting, out _itemsPerTask);
            }
            //spo call limit
            var spoCallLimitPerSecond = RMGlobalConfiguration.AppConfig.GetNumberValue(RMAppSettingKey.SPO_SYNC_DATA_CALL_LIMIT_PER_SECOND, 50);
            _spoCallLimiter = CallLimiterFactory.CreateInstance("SPOCalllimiter", spoCallLimitPerSecond);

            //cosmos call limit
            var cosmosCallLimitPerSecond = RMGlobalConfiguration.AppConfig.GetNumberValue(RMAppSettingKey.COSMOS_SYNC_DATA_CALL_LIMIT_PER_SECOND, 20);
            _cosmosCallLimiter = CallLimiterFactory.CreateInstance("CosmosCallLimiter", cosmosCallLimitPerSecond);
            containerId = SPTreeNodeManagement.GetGroupNode(treeNode).ID;

            mRuleCache = (RuleDao.GetRulesWithoutRemovedAsync().Result).ToDictionary(r => r.RuleId);
            _ = s_keyValueDao.TryGetBoolValue(KeyNameCollection.IsSyncStubFile, out _isSyncStubFile);
            //InitCosmosBulkOperation();
        }

        private void InitCosmosBulkOperation()
        {
            var RMKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
            _isCosmosBulkOperationEnabled = true;
            //RMKeyValueDao.IsCosmosBulkOperationEnabled();
            //if (_isCosmosBulkOperationEnabled)
            {
                var bulkSize = RMKeyValueDao.GetCosmosBulkInsertOperationBufferSize();
                if (bulkSize == default(int)) bulkSize = CosmosBulkOperator.DefualtBufferSize;
                logger.Info($"Cosmos bulk operation enabled, bulk size: {bulkSize}");
                CosmosBulkOperator.Instance.Start(bulkSize, ProcessSucceedRecord, ProcessFailedRecord);
            }
        }

        public void Init(ISPDiscover sPDiscover, SPDiscoverType discoverType, bool bulkImport, bool forceUpdate)
        {
            mDiscover = sPDiscover;
            mDiscoverType = discoverType;
            _forceUpdate = forceUpdate;
            if (bulkImport)
            {
                InitCosmosBulkOperation();
            }
        }

        public override async System.Threading.Tasks.Task RunNowAsync()
        {
            try
            {
                using (var performance = new PerformanceScope("RMOneDriveExplorerBase.RunNow", addToStatistics: true))
                {
                    using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        string disposalAction = string.Empty;
                        ThrowUtil.ThrowIfNull(DiscoverSite, $"Discover Site is null:{TreeNode?.FullPath}");

                        var aveSite = DiscoverSite.Site;
                        var termInfo = GetTermInfo(aveSite.RootWeb.Properties);
                        RMRuleItemCollection rules = null;
                        SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
                        if (RMOneDriveExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                        {
                            var newRuleCollection = RebuldSPRules(rules);
                            if (newRuleCollection.Rules.Count == 0)
                            {
                                logger.Info($"No SP rules realted to the site {aveSite.RootWeb.ServerRelativeUrl}");
                            }
                            else
                            {
                                var filterEnginer = new RMOneDriveRuleChecker(newRuleCollection);
                                itemRuleInfo = filterEnginer.CheckDisposalRule(aveSite);
                            }
                        }
                        itemRuleInfo.TermInfo = termInfo;
                        syncItem.InitTimeZone(aveSite.RootWeb.RegionalSettings.TimeZone);
                        var item = syncItem.AssembleRecord(DiscoverSite, itemRuleInfo);
                        SyncItemToDB(item);

                        var webs = mDiscover.GetWebs(DiscoverSite);
                        JobContext.ReportManager.IncreaseBase(webs.LongCount());
                        foreach (var web in webs)
                        {
                            using (web)
                            {
                                await ProcessWebAsync(web, itemRuleInfo);
                            }
                        }
                        AddSiteScope(item);
                        if (_isCosmosBulkOperationEnabled)
                        {
                            CosmosBulkOperator.Instance.Complete();
                            CosmosBulkOperator.Instance.Reset();
                        }
                        if (!_siteCache.HasErrorNode)
                        {
                            //需要插入Flag 或者更新Flag中的时间
                            if (FailureItems.Count >= 1000)
                            {
                                logger.Info("More than 1000 failed items in site {0}, count {2}", aveSite.Url, FailureItems.Count);
                                //failure 数量大于 1000， 不插入Azure Table， 
                                JobContext.HasErrorNode = true;
                                _siteCache.HasErrorNode = true;
                            }
                            else
                            {
                                logger.Info("Failed items count{0}, in site {1}", FailureItems.Count, aveSite.Url);
                                //将失败的Item插入Azure Table， 下次Job再处理
                                AddFailureItem2Azure();
                                //如果存在失败数据， Job状态不能是Finish
                                if (FailureItems.Count > 0)
                                {
                                    JobContext.HasErrorNode = true;
                                }
                            }
                        }
                        else
                        {
                            //HasErrorNode， 不会更新Flag， 也不需要单独处理此次失败的Item。
                            logger.Info("Has error container in site {0}, ignore the fail items, count {1}", aveSite.Url, FailureItems.Count);
                        }
                    }
                }

            }
            catch (JobStopException)
            {
                throw new JobStopException("the job has stopped.");
            }
            catch (Exception e)
            {
                logger.Error($"error occurred while Process Site:{TreeNode?.FullPath}, ERROR:{e.ToString()}");
                JobContext.HasErrorNode = true;
                _siteCache.HasErrorNode = true;
                JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                {
                    ObjectName = TreeNode?.Name,
                    FullPath = TreeNode?.FullPath,
                    Status = JobDetailsStatus.Failed,
                    Comment = GetExceptionMessage(e),
                });
            }
            finally
            {
                if (needUpdateLabelState)
                {
                    await RMOneDriveRetentionDataCache.Instance.AddLabelHistoryAsync();
                }
            }
        }



        public virtual async System.Threading.Tasks.Task ProcessWebAsync(AveDiscoverWeb discoverWeb, SyncItemRuleInfo parentItemRule)
        {
            bool useUniqueSetting = false;
            Guid uniqueSettingScopeId = Guid.Empty;
            bool hasError = false;
            try
            {
                using (var performance = new PerformanceScope("RMOneDriveExplorerBase.ProcessWeb", $"RMOneDriveExplorerBase.ProcessWeb:[{discoverWeb.Name}]", addToStatistics: true))
                {
                    logger.Info($"Process web:{discoverWeb?.FullUrl}");
                    string disposalAction = string.Empty;
                    if (discoverWeb.ChangeType == Wrapper.Common.ChangeType.Delete)
                    {
                        logger.Info("remove web object {0} : {1}", DiscoverSite.SiteID, discoverWeb.WebID);
                        RemoveSPObj(discoverWeb.WebID);
                        return;
                    }
                    RMOneDriveSetting webLevelSetting = mAllSettingsUnderSite.Count > 0 ? GetWebLevelSetting(discoverWeb.AveWeb) : null;
                    if (webLevelSetting == null)
                    {
                        webLevelSetting = mSiteLevelSetting;
                    }
                    else
                    {
                        useUniqueSetting = true;
                        uniqueSettingScopeId = webLevelSetting.ScopeId;
                        logger.Info($"Web has unique seting:{discoverWeb?.FullUrl}");
                    }
                    //TODO Need Derek Review
                    //if (!webLevelSetting.IsSyncData)
                    //{
                    //    logger.Info("Web level setting doesn't enable data sync.");
                    //    return;
                    //}

                    if (webLevelSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                    {
                        logger.Info("Web level setting doesn't enable record management.");
                        return;
                    }
                    var aveWeb = discoverWeb.AveWeb;
                    var termInfo = GetTermInfo(aveWeb.Properties);
                    RMRuleItemCollection rules = null;
                    SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
                    if (RMOneDriveExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                    {
                        var newRuleCollection = RebuldSPRules(rules);
                        if (newRuleCollection.Rules.Count == 0)
                        {
                            logger.Info($"No SP rules realted to the web {aveWeb.ServerRelativeUrl}");
                        }
                        else
                        {
                            var filterEnginer = new RMOneDriveRuleChecker(newRuleCollection);
                            itemRuleInfo = filterEnginer.CheckDisposalRule(discoverWeb, parentItemRule);
                        }
                    }
                    itemRuleInfo.TermInfo = termInfo;
                    var item = syncItem.AssembleRecord(discoverWeb, itemRuleInfo);
                    SyncItemToDB(item);
                    var lists = mDiscover.GetLists(discoverWeb);
                    JobContext.ReportManager.IncreaseBase(lists.LongCount());
                    foreach (var list in lists)
                    {
                        using (list)
                        {
                            using (CheckJobStopScope stopScope = new CheckJobStopScope())
                            {
                                await ProcessListAsync(discoverWeb, list, itemRuleInfo, discoverWeb.WebID, webLevelSetting);
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
                logger.Error($"error occurred while Process web:{discoverWeb?.FullUrl}, ERROR:{e.ToString()}");
                JobContext.HasErrorNode = true;
                _siteCache.HasErrorNode = true;
                JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                {
                    ObjectName = discoverWeb?.Title,
                    FullPath = discoverWeb?.FullUrl,
                    Status = JobDetailsStatus.Failed,
                    Comment = GetExceptionMessage(e),
                });
                hasError = true;
            }
            finally
            {
                JobContext.ReportManager.Increase();
                if (useUniqueSetting && !hasError)
                {
                    await OneDriveSettingDao.SetSettingJobTimeAsync(uniqueSettingScopeId, mSettingSiteId);
                }
            }
        }

        private RMOneDriveSetting GetWebLevelSetting(IAveWeb web)
        {
            var setting = mAllSettingsUnderSite.Where(s => s.ScopeId == web.ID && s.WebId == web.ID).FirstOrDefault();
            if (setting != null)
            {
                return setting;
            }
            else
            {

                if (web.IsRootWeb)
                {
                    return null;
                }
                else
                {
                    return GetWebLevelSetting(web.ParentWeb);
                }
            }
        }
        public virtual async System.Threading.Tasks.Task ProcessListAsync(AveDiscoverWeb discoverWeb, AveDiscoverList discoverList, SyncItemRuleInfo parentItemRule, Guid webId, RMOneDriveSetting webLevelSetting)
        {
            string listPath = string.Empty;
            bool useUniqueSetting = false;
            Guid uniqueSettingScopeId = Guid.Empty;
            bool hasError = false;
            try
            {
                using (var performance = new PerformanceScope("RMOneDriveExplorerBase.ProcessList", $"RMOneDriveExplorerBase.ProcessList:{discoverList?.RootFolderUrl}", addToStatistics: true))
                {
                    string disposalAction = string.Empty;
                    logger.Info($"Process list:{discoverList?.RootFolderUrl}");
                    using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {

                        AvePoint.GCommon.Utility.ArgumentCheck.NotNull(discoverList, nameof(discoverList));
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

                        RMOneDriveSetting listLevelSetting = OneDriveSettingDao.LoadOneDriveSetting(list.ID, mSettingSiteId);
                        if (listLevelSetting == null)
                        {
                            listLevelSetting = webLevelSetting;
                        }
                        else
                        {
                            useUniqueSetting = true;
                            uniqueSettingScopeId = listLevelSetting.ScopeId;
                            logger.Info($"List has unique setting:{discoverList?.RootFolderUrl}");
                        }
                        //TODO Need Derek Review
                        //if (!listLevelSetting.IsSyncData)
                        //{
                        //    logger.Info("List level setting doesn't enable data sync.");
                        //    return;
                        //}

                        if (listLevelSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                        {
                            logger.Info("List level setting doesn't enable record management.");
                            return;
                        }
                        logger.Info($"Full Path:{listLevelSetting.FullPath} DeployMethod:{listLevelSetting.DeployTermMethod.ToString()}");
                        //if (!HasBCSColumn(list))
                        //{
                        //    logger.Warn($"list does not have bcs column, list:{discoverList?.RootFolderUrl}, column name:{_siteCache.BCSColumnInternalName}");
                        //    return;
                        //}

                        listPath = WebUtil.MakeFullUrl(list.ParentWeb.Url, list.RootFolder.Url);
                        var termInfo = GetTermInfo(list.RootFolder.Properties);
                        RMRuleItemCollection rules = null;
                        SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
                        if (RMOneDriveExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                        {
                            var newRuleCollection = RebuldSPRules(rules);
                            if (newRuleCollection.Rules.Count == 0)
                            {
                                logger.Info($"No SP rules realted to the list {list.RootFolder.ServerRelativeUrl}");
                            }
                            else
                            {
                                var filterEnginer = new RMOneDriveRuleChecker(newRuleCollection);
                                itemRuleInfo = filterEnginer.CheckDisposalRule(discoverList, list, parentItemRule);

                            }

                        }
                        itemRuleInfo.TermInfo = termInfo;
                        var item = syncItem.AssembleRecord(discoverList, itemRuleInfo);
                        SyncItemToDB(item);

                        logger.Info($"Get items under [{list.RootFolder.ServerRelativeUrl}].");

                        List<string> excludePath = OneDriveSettingDao.GetFolderSettingUnderList(list.ID, mSettingSiteId).Select(f => WebUtil.MakeServerRelativeUrl(f.FullPath)).ToList();
                        excludePath = excludePath.Where(p => p.StartsWith(list.RootFolder.ServerRelativeUrl) && p != list.RootFolder.ServerRelativeUrl).ToList();
                        ProcessFailedItems(list, parentItemRule);
                        if (NeedRunFullDiscover(listLevelSetting, mDiscover))
                        {
                            logger.Info($"Start to sync items for full discover.[{list.RootFolder.ServerRelativeUrl}].");
                            int totalItemCount = SyncItemsForFullDiscover(list, list.RootFolder, parentItemRule, excludePath, listLevelSetting);
                            logger.Info($"Sync items for full discover finished.[{list.RootFolder.ServerRelativeUrl}]");
                            //Full job optimiz for Cardinia
                            if (totalItemCount > 10000 && !unSuccessList.Contains(list.ID)) //超过10000 Items的Library，没有失败的Item， 加入NodeFlag， 为List Incremental做准备
                            {
                                AddListFlag(discoverList, totalItemCount);
                            }
                            ProcessListDeletedData(discoverList, list, webId);
                        }
                        else if (NeedRunSearchDiscover(mLastScanTime))
                        {
                            logger.Info($"Start to sync items for search discover.[{list.RootFolder.ServerRelativeUrl}].");
                            SyncItemsForSearchDicsover(list, list.RootFolder, parentItemRule, excludePath, listLevelSetting);
                            logger.Info($"Sync items for search discover finished.[{list.RootFolder.ServerRelativeUrl}]");
                            ProcessListDeletedData(discoverList, list, webId);
                        }
                        else
                        {
                            logger.Info($"Get changed items under [{list.RootFolder.ServerRelativeUrl}] for incremental sync job.");
                            Dictionary<string, object> changedItems = new Dictionary<string, object>();
                            using (var performance1 = new PerformanceScope("RMOneDriveExplorerBase.GetItemsForRecords", addToStatistics: true))
                            {
                                changedItems = discoverList.GetListChangedItems(webId, new DateTime(mLastScanTime, DateTimeKind.Utc), new DateTime(mMainJobStartTime, DateTimeKind.Utc));
                            }
                            logger.Info($"Start to sync items for incremental discover.[{list.RootFolder.ServerRelativeUrl}].");
                            ProcessIncrementalChangedItems(list, changedItems, list.RootFolder.UniqueId, parentItemRule, excludePath, listLevelSetting);
                            logger.Info($"Sync items for incremental discover finished.[{list.RootFolder.ServerRelativeUrl}]");

                        }
                        #region old logic
                        //if (NeedRunFullDiscover(listLevelSetting, mDiscover))
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
                        //        using (CheckJobStopScope jScope = new CheckJobStopScope())
                        //        {
                        //            items = list.GetItemsForRecords(query);
                        //        }
                        //        JobContext.ReportManager.IncreaseBase(items.Count);
                        //        logger.Info($"Existing job process item count:[{items.Count}]");
                        //        totalItemCount += items.Count;
                        //        ProcessAveItems(items, list.RootFolder.UniqueId, parentItemRule, excludePath, listLevelSetting);
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
                        //        AddListFlag(discoverList, totalItemCount);
                        //    }

                        //    try
                        //    {
                        //        if (mLastScanTime != DateTime.MinValue.Ticks)
                        //        {
                        //            logger.Info($"Start to process deleted data in {list.RootFolder.ServerRelativeUrl}");
                        //            var changedItems = discoverList.GetListChangedItems(webId, new DateTime(mLastScanTime, DateTimeKind.Utc), new DateTime(mMainJobStartTime, DateTimeKind.Utc));
                        //            ProcessDeletedItems(list, changedItems);
                        //            logger.Info($"Process deleted data in {list.RootFolder.ServerRelativeUrl} finished.");
                        //        }
                        //    }
                        //    catch (Exception e)
                        //    {
                        //        logger.Warn("An error occurred while updating deleted data in list. Url:{0} Error:{1}", listPath, e.ToString());
                        //    }
                        //}
                        //else
                        //{
                        //    logger.Info($"Get changed items under [{list.RootFolder.ServerRelativeUrl}] for incremental sync job.");
                        //    var changedItems = discoverList.GetListChangedItems(webId, new DateTime(mLastScanTime, DateTimeKind.Utc), new DateTime(mMainJobStartTime, DateTimeKind.Utc));
                        //    ProcessIncrementalChangedItems(list, changedItems, list.RootFolder.UniqueId, parentItemRule, excludePath, listLevelSetting);
                        //}
                        #endregion
                        await ProcessFoldersWithUniqueSettingAsync(list, discoverList, webId, parentItemRule);
                    }
                }
            }
            catch (JobStopException)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                logger.Error($"error occurred while Process list:{discoverList?.RootFolderUrl}, ERROR:{e.ToString()}");
                JobContext.HasErrorNode = true;
                _siteCache.HasErrorNode = true;
                JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                {
                    ObjectName = discoverList?.Title,
                    FullPath = listPath,
                    Status = JobDetailsStatus.Failed,
                    Comment = GetExceptionMessage(e),
                });
                hasError = true;
            }
            finally
            {
                JobContext.ReportManager.Increase();
                if (useUniqueSetting && !hasError)
                {
                    await OneDriveSettingDao.SetSettingJobTimeAsync(uniqueSettingScopeId, mSettingSiteId);
                }
            }
        }

        private bool NeedRunSearchDiscover(long lastJobTimeTicks)
        {
            var lastJobTime = DateTime.SpecifyKind(new DateTime(lastJobTimeTicks), DateTimeKind.Utc);
            return lastJobTime.AddDays(59) < DateTime.UtcNow;
        }

        private int SyncItemsForFullDiscover(IAveList list, IAveFolder folder, SyncItemRuleInfo parentItemRule, List<string> excludePath, RMOneDriveSetting spSetting)
        {
            int totalItemCount = 0;
            int rowLimit = GetListViewThresholdNumber(list); // list.ParentWeb.Site.GetMaxItemsPerThrottledOperation();
            bool needQueryNext = false;
            AveCamlQuery query = GetQuery(folder, rowLimit);
            int startIdx = 0;
            int endIdx = startIdx + rowLimit;
            int maxItemId = SPCommonUtility.GetLastItemFolderId(list, folder);
            IAveListItemCollection items = null;
            do
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    using (var performance = new PerformanceScope("RMOneDriveExplorerBase.GetItemsForRecords", addToStatistics: true))
                    {
                        items = list.GetItemsForRecords(query);
                    }
                }
                //JobContext.ReportManager.IncreaseBase(items.Count);
                logger.Info($"Existing job process item count:[{items.Count}]");
                totalItemCount += items.Count;
                ProcessAveItems(items, list.RootFolder.UniqueId, parentItemRule, excludePath, spSetting);
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
                logger.Info($"ProcessList total item count {totalItemCount} , Large data list, add flag {discoverList?.RootFolderUrl}");
                AvePoint.GCommon.Utility.ArgumentCheck.NotNull(discoverList, nameof(discoverList));
                SPTreeNodeDto groupNode = SPTreeNodeManagement.GetGroupNode(TreeNode);
                if (groupNode != null)
                {
                    RMNodeFlagDao.AddListFlagInfo(new RMNodeFlag()
                    {
                        NodeId = new Guid(TreeNode.SPObjectId),
                        Title = discoverList.Title,
                        FullPath = discoverList.RootFolderUrl,
                        CollectionTime = DateTime.UtcNow.Ticks,
                        GroupId = new Guid(groupNode.SPObjectId),//Debug
                        ListId = discoverList.ListId,
                        IsRemoved = false,
                        NodeFlagType = (int)NodeFlagType.OneDriveExplorerSyncLib
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

        private void SyncItemsForSearchDicsover(IAveList list, IAveFolder folder, SyncItemRuleInfo parentItemRule, List<string> excludePath, RMOneDriveSetting spSetting)
        {
            bool needQueryNext = false;
            int rowLimit = list.ParentWeb.Site.GetMaxItemsPerThrottledOperation();
            int maxItemId = GetLastItemId(list, folder);

            int startIndex = 0;
            IAveListItemCollection items = null;
            DateTime startTime = DateTime.SpecifyKind(new DateTime(mLastScanTime), DateTimeKind.Utc);
            DateTime endTime = DateTime.UtcNow;
            do
            {
                using (var queryAuto = new PerformanceScope("RMSPExplorerBase.SearchQueryData", $"RMSPExplorerBase.SearchQueryData{folder.ServerRelativeUrl} start{startIndex}", true))
                {
                    AveCamlQuery query = GetSearchDiscoverQuery(list, folder, startTime, endTime, startIndex, startIndex + rowLimit, rowLimit);
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        using (var performance = new PerformanceScope("RMSPExplorerBase.GetItemsForRecords", addToStatistics: true))
                        {
                            items = list.GetItemsForRecords(query);
                        }
                    }
                    //JobContext.ReportManager.IncreaseBase(items.Count);
                    logger.Info($"Data sync job process folder url {folder.ServerRelativeUrl} item count:[{items.Count}], start index {startIndex}, end index {startIndex + rowLimit}");
                }
                using (var queryAuto = new PerformanceScope("RMSPExplorerBase.ProcessAveItems", $"RMSPExplorerBase.ProcessAveItems{folder.ServerRelativeUrl} count {items.Count}", true))
                {
                    ProcessAveItems(items, list.RootFolder.UniqueId, parentItemRule, excludePath, spSetting);
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

        private void ProcessListDeletedData(AveDiscoverList discoverList, IAveList list, Guid webId)
        {
            logger.Info($"Start to process deleted data in {list.RootFolder.ServerRelativeUrl}");
            if (mLastScanTime != DateTime.MinValue.Ticks)
            {
                try
                {
                    Dictionary<string, object> changedItems = new Dictionary<string, object>();
                    using (var performance = new PerformanceScope("RMOneDriveExplorerBase.GetListChangedItems", addToStatistics: true))
                    {
                        changedItems = discoverList.GetListChangedItems(webId, new DateTime(mLastScanTime, DateTimeKind.Utc), new DateTime(mMainJobStartTime, DateTimeKind.Utc));
                    }
                    ProcessDeletedItems(list, changedItems);
                }
                catch (JobStopException)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while process deleted data. Error{0}", e.ToString());
                }
            }
            logger.Info($"Process deleted data in {list.RootFolder.ServerRelativeUrl} finished.");
        }

        private void ProcessDeletedItems(IAveList list, Dictionary<string, object> changedItems)
        {
            foreach (var changeItem in changedItems)
            {
                using (var performance = new PerformanceScope("RMOneDriveExplorerBase.ProcessDeletedItems", addToStatistics: true))
                {
                    using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        Dictionary<string, object> itemChangeProperties = changeItem.Value as Dictionary<string, object>;
                        int itemId = (int)itemChangeProperties["ItemId"];
                        int itemChangeType = (int)itemChangeProperties["ChangeType"];
                        Guid itemUniqueId = (Guid)itemChangeProperties["UniqueId"];
                        logger.Info($"Process changed item:Id:{itemId}.UniqueId:{itemUniqueId}.ChangeType:{itemChangeType}.");
                        if (itemChangeProperties.ContainsKey("Hidden") && (bool)itemChangeProperties["Hidden"])
                        {
                            logger.Info($"skip hidden item:{itemId}");
                            continue;
                        }
                        if (itemChangeType == (int)Wrapper.Common.ChangeType.Delete)
                        {
                            try
                            {
                                using (var performance0 = new PerformanceScope("RMOneDriveExplorerBase.GetDeletedItem", addToStatistics: true))
                                {
                                    WaitSPOExecuteAction(() =>
                                    {
                                        var aveItem = list.GetItemById(itemId);
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Info($"cannot found item object ID:{itemId} Guid:{itemUniqueId} :{ex.ToString()}");

                                RemoveSPObj(itemUniqueId, itemId);
                            }
                            logger.Warn("remove view item, {0}.", itemId);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 判断当前是否需要按照Full进行discover，以下情况需要按照Full进行discover，确保可以查询出所有数据：
        ///1.当前节点使用的setting用了auto classification，并且勾选了了select all或者criteria中使用了older than条件
        ///2.当前节点使用的setting用了use default，并且勾选了select all
        ///3.如果以上均为使用，再判断LastScanTime是否为DateTime.MinValue，如果是，则走full
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="mDiscover"></param>
        /// <returns></returns>
        private bool NeedRunFullDiscover(RMOneDriveSetting setting, ISPDiscover mDiscover)
        {
            if (setting.DeployTermMethod == (int)DeployTermMethod.UseAutoClassification)
            {
                if (setting.RunAutoFullJob)
                {
                    logger.Info("Current setting is RunAutoFullJob. ScopeId:{0}", setting.ScopeId);
                    return true;
                }

                if (HasAutoOlderThanRule(setting.AutoClassificationRules))
                {
                    logger.Info("Current setting has older than rule. ScopeId:{0}", setting.ScopeId);
                    return true;
                }
            }
            else if (setting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm)
            {
                if (setting.NeedCheckDefaultValue)
                {
                    logger.Info("Current setting is NeedCheckDefaultValue. ScopeId:{0}", setting.ScopeId);
                    return true;
                }
            }
            else if (setting.DeployTermMethod == (int)DeployTermMethod.UseIntelligenceClassification && setting.AITermUseType == ArtificialIntelligenceTermUseType.ApplyTerm)
            {
                if (setting.RunAutoFullJob)
                {
                    logger.Info("Current setting is run ai full job. ScopeId:{0}", setting.ScopeId);
                    return true;
                }
            }

            if (mLastScanTime == DateTime.MinValue.Ticks)
            {
                logger.Info("Last scan time is DateTime.MinValue, will run full job. ScopeId:{0}", setting.ScopeId);
                return true;
            }

            logger.Info("Current node is using incremental. ScopeId:{0}", setting.ScopeId);
            return false;
        }

        private bool HasAutoOlderThanRule(string autoRulesStr)
        {
            List<ClassificationRule> autoRules = SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(autoRulesStr);
            foreach (var autoRule in autoRules)
            {
                if (!autoRule.IsDefaultRule)
                {
                    foreach (var filterGroup in autoRule.FilterGroups)
                    {
                        if (filterGroup.Filters.Any(f => f.Condition == ArchiverFilterCondition.OlderThan))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private async System.Threading.Tasks.Task ProcessFoldersWithUniqueSettingAsync(IAveList list, AveDiscoverList discoverList, Guid webId, SyncItemRuleInfo parentItemRule)
        {
            int rowLimit = GetListViewThresholdNumber(list);
            var folderSettings = OneDriveSettingDao.GetFolderSettingUnderList(list.ID, mSettingSiteId).Where(p => WebUtil.MakeServerRelativeUrl(p.FullPath).StartsWith(list.RootFolder.ServerRelativeUrl) && WebUtil.MakeServerRelativeUrl(p.FullPath) != list.RootFolder.ServerRelativeUrl).ToList();
            logger.Info("Process folders with unique settings. Count:{0}", folderSettings?.Count);

            bool hasInitRoorFolder = false;
            Dictionary<Guid, AveDiscoverFolder> folderCache = new Dictionary<Guid, AveDiscoverFolder>();
            AvePoint.GCommon.Utility.ArgumentCheck.NotNull(folderSettings, nameof(folderSettings));
            foreach (var folderSetting in folderSettings)
            {
                using (var performance = new PerformanceScope("RMSPExplorerBase.ProcessFoldersWithUniqueSetting", $"RMSPExplorerBase.ProcessFoldersWithUniqueSetting.{folderSetting.FullPath}", addToStatistics: true))
                {
                    bool hasError = false;
                    try
                    {
                        using (CheckJobStopScope stopScope = new CheckJobStopScope())
                        {
                            if (folderSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                            {
                                logger.Info("Folder level setting doesn't enable record management.");
                                continue;
                            }
                            logger.Info("Start to process folder. Full path:{0} DeployMethod:{1}", folderSetting.FullPath, folderSetting.DeployTermMethod.ToString());
                            IAveFolder folder = list.GetFolder(WebUtil.MakeServerRelativeUrl(folderSetting.FullPath));
                            var folderExcludePath = folderSettings.Select(f => WebUtil.MakeServerRelativeUrl(f.FullPath)).ToList();
                            folderExcludePath = folderExcludePath.Where(p => p.StartsWith(folder.ServerRelativeUrl) && p != folder.ServerRelativeUrl).ToList();

                            if (NeedRunFullDiscover(folderSetting, mDiscover))
                            {
                                logger.Info($"Start to sync items for full discover.[{folder.ServerRelativeUrl}].");
                                try
                                {
                                    SyncItemsForFullDiscover(list, folder, parentItemRule, folderExcludePath, folderSetting);
                                }
                                catch (JobStopException)
                                {
                                    throw new JobStopException("This Job is stopped.");
                                }
                                catch (Exception e)
                                {
                                    HandleFolderSyncError(list, folder, discoverList, parentItemRule, folderExcludePath, folderSetting, folderCache, e, ref hasInitRoorFolder);
                                }
                                logger.Info($"Sync items for full discover finished.[{folder.ServerRelativeUrl}]");
                                ProcessFolderDeletedData(list, discoverList, folder, webId);
                            }
                            else if (NeedRunSearchDiscover(mLastScanTime))
                            {
                                logger.Info($"Start to sync items for search discover.[{folder.ServerRelativeUrl}].");
                                try
                                {
                                    SyncItemsForSearchDicsover(list, folder, parentItemRule, folderExcludePath, folderSetting);
                                }
                                catch (JobStopException)
                                {
                                    throw new JobStopException("This Job is stopped.");
                                }
                                catch (Exception e)
                                {
                                    HandleFolderSyncError(list, folder, discoverList, parentItemRule, folderExcludePath, folderSetting, folderCache, e, ref hasInitRoorFolder);
                                }
                                logger.Info($"Sync items for search discover finished.[{folder.ServerRelativeUrl}]");
                                ProcessFolderDeletedData(list, discoverList, folder, webId);
                            }
                            else
                            {
                                logger.Info($"Get changed items under [{folder.ServerRelativeUrl}] for incremental sync job.");
                                Dictionary<string, object> changedItems = new Dictionary<string, object>();
                                using (var performance1 = new PerformanceScope("RMOneDriveExplorerBase.GetFolderChangedItems", addToStatistics: true))
                                {
                                    changedItems = discoverList.GetFolderAndSubFolderChangedItems(webId, folder.UniqueId, new DateTime(mLastScanTime, DateTimeKind.Utc), new DateTime(mMainJobStartTime, DateTimeKind.Utc));
                                }
                                logger.Info($"Start to sync items for incremental discover.[{folder.ServerRelativeUrl}].");
                                ProcessIncrementalChangedItems(list, changedItems, list.RootFolder.UniqueId, parentItemRule, folderExcludePath, folderSetting);
                                logger.Info($"Sync items for incremental discover finished.[{folder.ServerRelativeUrl}]");

                            }
                            #region old logic
                            //if (NeedRunFullDiscover(folderSetting, mDiscover))
                            //{
                            //    int totalItemCount = 0;
                            //    //Full job optimiz for Cardinia
                            //    // list.ParentWeb.Site.GetMaxItemsPerThrottledOperation();
                            //    bool needQueryNext = false;
                            //    AveCamlQuery query = GetQuery(folder, rowLimit);
                            //    int startIdx = 0;
                            //    int endIdx = startIdx + rowLimit;
                            //    int maxItemId = SPCommonUtility.GetLastItemFolderId(list, list.RootFolder);
                            //    IAveListItemCollection items = null;
                            //    try
                            //    {
                            //        do
                            //        {
                            //            using (CheckJobStopScope jScope = new CheckJobStopScope())
                            //            {
                            //                items = list.GetItemsForRecords(query);
                            //            }
                            //            JobContext.ReportManager.IncreaseBase(items.Count);
                            //            logger.Info($"Existing job process item count:[{items.Count}]");
                            //            totalItemCount += items.Count;
                            //            ProcessAveItems(items, list.RootFolder.UniqueId, parentItemRule, folderExcludePath, folderSetting);
                            //            startIdx = endIdx;
                            //            endIdx = startIdx + rowLimit;
                            //            needQueryNext = startIdx < maxItemId;
                            //            if (needQueryNext)
                            //            {
                            //                logger.Info($"Query for items. StartIndex:[{startIdx}] EndIndex:[{endIdx}]");
                            //                query.ViewXml = GetQueryXml(startIdx, endIdx, rowLimit);
                            //            }
                            //        }
                            //        while (needQueryNext);
                            //    }
                            //    catch (JobStopException)
                            //    {
                            //        throw new JobStopException("This Job is stopped.");
                            //    }
                            //    catch (Exception e)
                            //    {
                            //        logger.Error("An error occurred while processing folder with unique setting. Folder url:{0} Error:{1}", folderSetting.FullPath, e.ToString());
                            //        if (e.Message.Contains("The attempted operation is prohibited because it exceeds the list view threshold"))
                            //        {
                            //            if (!hasInitRoorFolder)
                            //            {
                            //                logger.Info("Start to init folder with structure.");
                            //                var rootFolder = discoverList.GetRootFolder(true);
                            //                AddDiscoverFolderToCache(rootFolder, folderCache);
                            //                logger.Info("Init folder with structure finished. Count:{0}", folderCache.Count);
                            //                hasInitRoorFolder = true;
                            //            }

                            //            if (folderCache.ContainsKey(folder.UniqueId))
                            //            {
                            //                logger.Info("Get items with spquery failed, try to get all items.");
                            //                var discoverFolder = folderCache[folder.UniqueId];
                            //                var discoverItems = discoverFolder.GetItemsWithStructure();//memory leak?
                            //                foreach (var tempItems in discoverItems)
                            //                {
                            //                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                            //                    {
                            //                        var itemList = tempItems.Select(i => i.CurrentItem).ToList();
                            //                        ProcessAveItems(itemList.AsEnumerable(), list.RootFolder.UniqueId, parentItemRule, folderExcludePath, folderSetting);
                            //                    }
                            //                }
                            //            }
                            //            else
                            //            {
                            //                logger.Info("Cannot find dicover folder from cache. Folder url:{0}", folderSetting.FullPath);
                            //                throw;
                            //            }
                            //        }
                            //        else
                            //        {
                            //            throw;
                            //        }
                            //    }

                            //    try
                            //    {
                            //        if (mLastScanTime != DateTime.MinValue.Ticks)
                            //        {
                            //            logger.Info($"Start to process deleted data in {folder.ServerRelativeUrl}");
                            //            var changedItems = discoverList.GetFolderChangedItems(webId, folder.UniqueId, new DateTime(mLastScanTime, DateTimeKind.Utc), new DateTime(mMainJobStartTime, DateTimeKind.Utc));
                            //            ProcessDeletedItems(list, changedItems);
                            //            logger.Info($"Process deleted data in {folder.ServerRelativeUrl} finished.");
                            //        }
                            //    }
                            //    catch (JobStopException)
                            //    {
                            //        throw new JobStopException("This Job is stopped.");
                            //    }
                            //    catch (Exception e)
                            //    {
                            //        logger.Warn("An error occurred while updating deleted data in folder. Url:{0} Error:{1}", WebUtil.MakeServerRelativeUrl(folderSetting.FullPath), e.ToString());
                            //    }
                            //}
                            //else
                            //{
                            //    var changedItems = discoverList.GetFolderChangedItems(webId, folder.UniqueId, new DateTime(mLastScanTime, DateTimeKind.Utc), new DateTime(mMainJobStartTime, DateTimeKind.Utc));
                            //    ProcessIncrementalChangedItems(list, changedItems, list.RootFolder.UniqueId, parentItemRule, folderExcludePath, folderSetting);
                            //}
                            #endregion
                            logger.Info("Process folder finished. Full path:{0}", folderSetting.FullPath);
                        }
                    }
                    catch (JobStopException)
                    {
                        throw new JobStopException("This Job is stopped.");
                    }
                    catch (Exception e)
                    {
                        logger.Error("An error occurred while process folder with unique setting. Full path:{0} Error:{1}", folderSetting.FullPath, e.ToString());
                        JobContext.HasErrorNode = true;
                        _siteCache.HasErrorNode = true;
                        JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                        {
                            ObjectName = WebUtil.MakeServerRelativeUrl(folderSetting.FullPath),
                            FullPath = folderSetting.FullPath,
                            Status = JobDetailsStatus.Failed,
                            Comment = GetExceptionMessage(e),
                        });
                        hasError = true;
                    }
                    finally
                    {
                        if (!hasError)
                        {
                           await OneDriveSettingDao.SetSettingJobTimeAsync(folderSetting.ScopeId, mSettingSiteId);
                        }
                    }
                }
            }
        }

        private void HandleFolderSyncError(IAveList list, IAveFolder folder, AveDiscoverList discoverList, SyncItemRuleInfo parentItemRule, List<string> folderExcludePath, RMOneDriveSetting folderSetting, Dictionary<Guid, AveDiscoverFolder> folderCache, Exception e, ref bool hasInitRoorFolder)
        {
            string errorMessage = GetExceptionMessage(e);
            logger.Error($"An error occurred while processing folder with unique setting. Folder url:{folderSetting.FullPath} ErrorMessage:{errorMessage} Error:{e.ToString()}");         
            if (!string.IsNullOrWhiteSpace(errorMessage)
                && (errorMessage.Contains("The attempted operation is prohibited because it exceeds the list view threshold")
                || errorMessage.Contains("Der versuchte Vorgang ist unzulässig, weil er den Schwellenwert für die Listenansicht überschreitet")))                
            {
                if (!hasInitRoorFolder)
                {
                    logger.Info("Start to init folder with structure.");
                    var rootFolder = discoverList.GetRootFolder(true);
                    AddDiscoverFolderToCache(rootFolder, folderCache);
                    logger.Info("Init folder with structure finished. Count:{0}", folderCache.Count);
                    hasInitRoorFolder = true;
                }

                if (folderCache.ContainsKey(folder.UniqueId))
                {
                    logger.Info("Get items with spquery failed, try to get all items.");
                    var discoverFolder = folderCache[folder.UniqueId];
                    var discoverItems = discoverFolder.GetItemsWithStructureForArchiver();//memory leak?
                    foreach (var tempItems in discoverItems)
                    {
                        using (CheckJobStopScope jScope = new CheckJobStopScope())
                        {                           
                            var rowIds = tempItems.Where(i => i.ID != null).Select(i => (int)i.ID).ToList();
                            IEnumerable<IAveListItem> items = GetItemsByRowIds(list, rowIds);                           
                            ProcessAveItems(items, list.RootFolder.UniqueId, parentItemRule, folderExcludePath, folderSetting);
                        }
                    }
                }
                else
                {
                    logger.Info("Cannot find dicover folder from cache. Folder url:{0}", folderSetting.FullPath);
                    throw e;
                }
            }
            else
            {
                throw e;
            }
        }

        private void ProcessFolderDeletedData(IAveList list, AveDiscoverList discoverList, IAveFolder folder, Guid webId)
        {
            try
            {
                if (mLastScanTime != DateTime.MinValue.Ticks)
                {
                    logger.Info($"Start to process deleted data in {folder.ServerRelativeUrl}");
                    Dictionary<string, object> changedItems = new Dictionary<string, object>();
                    using (var performance = new PerformanceScope("RMOneDriveExplorerBase.GetFolderChangedItems", addToStatistics: true))
                    {
                        changedItems = discoverList.GetFolderChangedItems(webId, folder.UniqueId, new DateTime(mLastScanTime, DateTimeKind.Utc), new DateTime(mMainJobStartTime, DateTimeKind.Utc));
                    }
                    ProcessDeletedItems(list, changedItems);
                    logger.Info($"Process deleted data in {folder.ServerRelativeUrl} finished.");
                }
            }
            catch (JobStopException)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while updating deleted data in folder. Url:{0} Error:{1}", folder.ServerRelativeUrl, e.ToString());
            }
        }

        private void AddDiscoverFolderToCache(AveDiscoverFolder folder, Dictionary<Guid, AveDiscoverFolder> folderCache)
        {
            if (!folderCache.ContainsKey(folder.AveFolder.UniqueId))
            {
                folderCache.Add(folder.AveFolder.UniqueId, folder);
            }

            foreach (var subFolders in folder.GetFoldersWithStructure(false))
            {
                foreach (var subFolder in subFolders)
                {
                    AddDiscoverFolderToCache(subFolder, folderCache);
                }
            }
        }

        //private void ProcessDataWithSPQuery(Guid remoteSiteId, IAveList list,IAveFolder folder, RMSharePointSetting setting, DateTime startTime, DateTime endTime )
        //{
        //    logger.Info($"Start to process auto classification. Path:[{folder?.ServerRelativeUrl}]");
        //    List<string> excludePath = SharePointSettingDao.GetFolderSettingUnderList(list.ID, remoteSiteId).Select(f => WebUtil.MakeServerRelativeUrl(f.FullPath)).ToList();
        //    excludePath = excludePath.Where(p => p.StartsWith(folder.ServerRelativeUrl) && p != folder.ServerRelativeUrl).ToList();

        //    List<ClassificationRule> autoRules = SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(setting.AutoClassificationRules);
        //    //Dictionary<Guid, IAveTerm> aveTerms = GetAveTerms(list, autoRules);
        //    Dictionary<string, Guid> ruleTermIdMapping = new Dictionary<string, Guid>();
        //    RuleCollection ruleCollection = GetRuleCollection(autoRules, ref ruleTermIdMapping);
        //    RuleManagement ruleManagement = new RuleManagement(ruleCollection);
        //    bool needQueryNext = false;
        //    int rowLimit = list.ParentWeb.Site.GetMaxItemsPerThrottledOperation();
        //    int maxItemId = GetListItemMaxId(list.RootFolder);

        //    int startIndex = 0;
        //    IAveListItemCollection items = null;
        //    do
        //    {
        //        AveCamlQuery query = GetAutoClassificationQuery(list, folder, setting, startTime, endTime, startIndex, startIndex + rowLimit, rowLimit);
        //        items = list.GetItemsForRecords(query);
        //        ReportManager.IncreaseBase(items.Count);
        //        logger.Info($"AutoJob process folder url {folder.ServerRelativeUrl} item count:[{items.Count}], start index {startIndex}, end index {startIndex + rowLimit}");

        //        //hasError = AutoSetValues(items, list, excludePath, aveTaxField, records, setting, ruleManagement, ruleTermIdMapping, aveTerms, configSiteSetting);
        //        if (startIndex + rowLimit < maxItemId)
        //        {
        //            needQueryNext = true;
        //            startIndex += rowLimit;
        //            logger.Info($"PagingInfo:{startIndex}");
        //        }
        //        else
        //        {
        //            needQueryNext = false;
        //        }
        //    }
        //    while (needQueryNext);
        //    logger.Info($"Finish to process auto classification. Path:[{folder?.ServerRelativeUrl}]");
        //}
        public static RuleCollection GetRuleCollection(List<ClassificationRule> autoRules, ref Dictionary<string, Guid> termRuleMapping)
        {
            List<Rule> rules = new List<Rule>();
            List<SOFilterPolicy> soFilters;
            foreach (var autoRule in autoRules)
            {
                if (autoRule.IsDefaultRule)
                {
                    if (autoRule.NoDefaultTerm)
                    {
                        termRuleMapping.Add(Guid.Empty.ToString(), Guid.Empty);
                    }
                    else
                    {
                        termRuleMapping.Add(Guid.Empty.ToString(), new Guid(autoRule.TermId));
                    }
                }
                else
                {
                    soFilters = new List<SOFilterPolicy>();
                    int sequenceNo = 0;
                    ConvertToSOFilters(autoRule.FilterGroups, ref sequenceNo, ref soFilters);
                    List<FilterPolicy> filerPolicies = ConvertSOFiletrPolicyToFilterPolicy(soFilters);
                    string andOrExpressionStr = GetGroupsAndOrExpression(autoRule.FilterGroups, ArchiverFilterCombineMode.And);
                    logger.Info("AndOr Expression:{0}", andOrExpressionStr);
                    Rule soRule = ConvertToSORule(autoRule, soFilters, filerPolicies, andOrExpressionStr);
                    rules.Add(soRule);

                    termRuleMapping.Add(soRule.Id, new Guid(autoRule.TermId));
                }
            }

            RuleCollection ruleCol = new RuleCollection() { Rules = new Dictionary<int, Rule>() };
            for (int i = 0; i < rules.Count; i++)
            {
                ruleCol.Rules.Add(i, rules[i]);
            }
            return ruleCol;
        }
        public static string GetGroupAndOrExpression(FilterGroup filterGroup)
        {
            string groupAndOrExpression = string.Empty;

            string filtersExpression = GetFiltersAndOrExpression(filterGroup.Filters);
            groupAndOrExpression = filtersExpression;

            if (filterGroup.FilterGroups != null && filterGroup.FilterGroups.Count > 0)
            {
                string groupsResult = GetGroupsAndOrExpression(filterGroup.FilterGroups, filterGroup.CombineMode);
                groupAndOrExpression += " " + filterGroup.CombineMode.ToString() + " " + groupsResult;
            }

            if (filterGroup.Filters.Count == 1 && filterGroup.FilterGroups.Count == 0)
            {
                //do nothing
            }
            else
            {
                groupAndOrExpression = "(" + groupAndOrExpression + ")";
            }
            return groupAndOrExpression;
        }
        public static string GetGroupsAndOrExpression(List<FilterGroup> filterGroups, ArchiverFilterCombineMode combineMode)
        {
            string result = string.Empty;
            for (int i = 0; i < filterGroups.Count; i++)
            {
                string groupResult = GetGroupAndOrExpression(filterGroups[i]);
                if (i == 0)
                {
                    result = groupResult;
                }
                else
                {
                    result += " " + combineMode.ToString() + " " + groupResult;
                }
            }
            return result;
        }
        public static List<FilterPolicy> ConvertSOFiletrPolicyToFilterPolicy(List<SOFilterPolicy> soFilters)
        {
            List<FilterPolicy> filerPolicies = new List<FilterPolicy>();
            foreach (var filter in soFilters)
            {
                FilterPolicy filterPolicy = new FilterPolicy();
                if (filter.Condition == PolicyCondition.Exactly || filter.Condition == PolicyCondition.Equals)
                {
                    filterPolicy.Condition = PolicyCondition.Equals;
                }
                else
                {
                    filterPolicy.Condition = filter.Condition;
                }
                filterPolicy.Level = filter.Level;
                filterPolicy.Rule = filter.Rule;
                filterPolicy.RuleType = filter.RuleType;
                filterPolicy.SequenceNo = filter.SequenceNo;
                filterPolicy.Value = filter.Value;

                filerPolicies.Add(filterPolicy);
            }
            return filerPolicies;
        }
        public static Rule ConvertToSORule(ClassificationRule autoRule, List<SOFilterPolicy> soFilters, List<FilterPolicy> filerPolicies, string andOrStr)
        {
            Rule rule = new Rule();
            rule.Id = Guid.NewGuid().ToString();
            rule.SOFilters = soFilters;
            rule.Filters = filerPolicies;
            rule.PolicyLevel = (PolicyLevel)autoRule.RuleLevel;
            rule.Order = autoRule.RuleOrder;
            rule.ProfileType = ServerFilterPolicy.ProfileType.ArchiverRule;
            rule.IncludeNew = "1";
            //rule.AndOrExpression = GetAndOrExpression(soFilters, autoRule.RuleLevel);
            rule.AndOrExpression = new Dictionary<PolicyLevel, string>() { { autoRule.RuleLevel, andOrStr } };
            return rule;
        }
        public static string GetFiltersAndOrExpression(List<RuleFilter> filters)
        {
            //string AndOrExpression = "(";
            string AndOrExpression = string.Empty;
            for (int i = 0; i < filters.Count; i++)
            {
                RuleFilter filter = filters[i];
                if (i == filters.Count - 1)
                {
                    AndOrExpression += string.Format("{0}", filter.SequenceNo);
                }
                else
                {
                    AndOrExpression += string.Format("{0} {1} ", filter.SequenceNo, filter.CombineMode == ArchiverFilterCombineMode.And ? "And" : "Or");
                }
            }
            //AndOrExpression += ")";
            return AndOrExpression;
        }
        public static void ConvertToSOFilters(List<FilterGroup> filterGroups, ref int sequenceNo, ref List<SOFilterPolicy> soFilters)
        {
            foreach (var filterGroup in filterGroups)
            {
                foreach (var raFilter in filterGroup.Filters)
                {
                    sequenceNo++;
                    SOFilterPolicy soFilter = BuildSOFilter(raFilter, sequenceNo);
                    soFilters.Add(soFilter);
                }
                ConvertToSOFilters(filterGroup.FilterGroups, ref sequenceNo, ref soFilters);
            }
        }
        public static SOFilterPolicy BuildSOFilter(RuleFilter filter, int sequenceNo)
        {
            ArchiverRuleFilter arFilter = new ArchiverRuleFilter();
            arFilter.CombineMode = filter.CombineMode;
            //arFilter.SequenceNo = filter.SequenceNo;
            arFilter.SequenceNo = sequenceNo;
            arFilter.Level = filter.Level;
            arFilter.Condition = filter.Condition;
            arFilter.RuleType = filter.RuleType;
            if (!string.IsNullOrEmpty(filter.filterName))
            {
                arFilter.RuleName = filter.filterName;
            }
            //arFilter.Dto.Rule = arFilter.RuleBase;
            if (arFilter.RuleType == ArchiverFilterRuleType.ModifiedTime || arFilter.RuleType == ArchiverFilterRuleType.CreatedTime
         || arFilter.RuleType == ArchiverFilterRuleType.LastAccessedTime || arFilter.RuleType == ArchiverFilterRuleType.DateTimeColumn || arFilter.RuleType == ArchiverFilterRuleType.DateTimeCustomProperty)
            {
                string startDayLightSaving = filter.StartTimeInfo == null ? "true" : filter.StartTimeInfo.IsDayLightSaving.ToString();
                string endDayLightSaving = filter.EndTimeInfo == null ? "true" : filter.EndTimeInfo.IsDayLightSaving.ToString();
                if (arFilter.Condition == ArchiverFilterCondition.FromTo)
                {

                    DateTime startUtcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                    DateTime endUtcTime = arFilter.SetDateTime(filter.Value2, filter.EndTimeInfo.TimeZoneId, endDayLightSaving, true);
                    if (DateTime.Parse(filter.Value1) >= DateTime.Parse(filter.Value2))
                    {
                        //throw new InvalidArgumentException(Messages.Get("start_date_after_end_date"));
                        throw new Exception("");
                    }
                    arFilter.Value1 = startUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    arFilter.Value2 = endUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                }
                else if (arFilter.Condition == ArchiverFilterCondition.Before)
                {
                    // ValidateValueCount(value, 3);
                    DateTime utcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                    arFilter.Value1 = utcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                }
                else if (arFilter.Condition == ArchiverFilterCondition.OlderThan)
                {
                    //ValidateValueCount(value, 1);
                    //SetValueForOlderThan(value[0]);
                    arFilter.Value1 = filter.Value1;
                    arFilter.Value1Unit = filter.Value1Unit;
                }
            }
            else
            {
                arFilter.Value1 = filter.Value1;
                if (filter.RuleType == ArchiverFilterRuleType.DocumentSize || filter.RuleType == ArchiverFilterRuleType.SiteCollectionSizeTrigger
                    || filter.RuleType == ArchiverFilterRuleType.Size)
                {
                    arFilter.Value1Unit = filter.Value1Unit;
                    arFilter.Value2Unit = filter.Value2Unit;
                }
                arFilter.Value2 = filter.Value2;
            }
            return arFilter.Dto;
        }
        protected static int GetListItemMaxId(IAveFolder folder)
        {
            AveCamlQuery query = new AveCamlQuery();

            query.ViewXml = "<View Scope='RecursiveAll'><Query><OrderBy><FieldRef Ascending='FALSE' Name='ID' /></OrderBy></Query><RowLimit>1</RowLimit></View>";

            query.FolderServerRelativeUrl = folder.ServerRelativeUrl;
            var items = folder.ParentList.GetItemsForRecords(query);
            if (items.Count <= 0) return 0;
            int maxId = items[0].ID;
            return maxId;
        }
        private int GetListViewThresholdNumber(IAveList list)
        {
            int rowLimit = list.ParentWeb.Site.GetMaxItemsPerThrottledOperation();
            if (rowLimit > 2000)
            {
                logger.Info("Threshold number is over 2000, limit it to 2000");
                return 2000;
            }
            return rowLimit;
        }
        public AveCamlQuery GetQuery(IAveFolder folder, int rowLimit)
        {
            AveCamlQuery query = new AveCamlQuery();
            query.FolderServerRelativeUrl = folder.ServerRelativeUrl;
            query.ListItemCollectionPosition = new AveItemCollectionPosition();
            query.ViewXml = GetQueryXml(0, 0 + rowLimit, rowLimit);
            return query;
        }
        public virtual void ProcessItems(IAveList list, IEnumerable<AveDiscoverItem> items, Guid parentId, SyncItemRuleInfo parentItemRule)
        {
            logger.Info($"Process item count:[{items.Count()}]");
            JobContext.ReportManager.IncreaseBase(items.Count());

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

        public virtual void ProcessIncrementalChangedItems(IAveList list, Dictionary<string, object> changedItems, Guid parentId, SyncItemRuleInfo parentItemRule, List<string> excludePath, RMOneDriveSetting spSetting)
        {
            try
            {
                int incItemsPerTask = changedItems.Count() / 5;
                logger.Info($"Process incremental changed item count:[{changedItems.Count()}].incItemsPerTask:[{incItemsPerTask}]");

                var changedObjects = changedItems.Values.Select(i => i as Dictionary<string, object>).Where(i => (i.ContainsKey("Hidden") && !(bool)i["Hidden"]) || !i.ContainsKey("Hidden")).ToList();
                var deleteItems = changedObjects.Where(i => (int)i["ChangeType"] == (int)Wrapper.Common.ChangeType.Delete).ToList();
                var existingItemIds = changedObjects.Where(i => (int)i["ChangeType"] != (int)Wrapper.Common.ChangeType.Delete).Select(i => (int)i["ItemId"]).ToList();
                JobContext.ReportManager.IncreaseBase(changedObjects.Count);
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
                        using (CheckJobStopScope jScope = new CheckJobStopScope())
                        {
                            ProcessIncrementalDeleteItem(list, changedItem);
                        }
                    }
                }
                #endregion

                UniqueIdUtil uniqueIdUtil;
                using (var performance = new PerformanceScope("RMSPExplorerProcessor.GenerateUniqueIds", addToStatistics: true))
                {
                    uniqueIdUtil = new UniqueIdUtil(TenantLocalValue.LogonGroupId, existingItemIds.Count);
                }

                RMMLAutoSmartItemsCache.Instance.Init(ProcessIncrementalSmartAutoCacheAveItemsAction);

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
                            ProcessIncrementalChangedItemV1(list, changedItem, parentId, parentItemRule, excludePath, spSetting, uniqueIdUtil, cts);
                        });
                    }
                    else
                    {
                        foreach (var changedItem in items)
                        {
                            ProcessIncrementalChangedItemV1(list, changedItem, parentId, parentItemRule, excludePath, spSetting, uniqueIdUtil);
                        }
                    }
                }

                if (RMMLAutoSmartItemsCache.Instance.NeedProcessCache)
                {
                    RMMLAutoSmartItemsCache.Instance.SetFinished();
                    RMMLAutoSmartItemsCache.Instance.Dispose();
                }
            }
            catch (JobStopException)
            {
                throw new JobStopException("the job has stopped.");
            }
           
        }

        public void ProcessIncrementalSmartAutoCacheAveItemsAction(List<AutoSmartCacheItemInfo> cacheItems)
        {
            var totalCount = cacheItems?.Count;
            if (totalCount > 0)
            {
                logger.Info($"it's need process auto smart cache changed items, count:{totalCount}");
                var odCacheItems = cacheItems.ConvertAll(o => (OneDriveAutoSmartCacheItemInfo)o);
                var aveItems = odCacheItems.Select(o => o.AveItem).ToList();
                var spSetting = odCacheItems.Select(o => o.SpSetting).First();
                RMOneDriveMachineLearningUtility.StartPredictTerm(GetNeedPredictItems(aveItems), mSettingSiteId, spSetting.SiteGroupId);
                if (totalCount > smartAutoCacheitemsPerTask)
                {
                    var cts = new System.Threading.CancellationTokenSource();
                    AveTenantTasks.RunParallel(odCacheItems, itemsPerTask, cts, item =>
                    {
                        ProcessIncrementalSmartAutoCacheOneItemAsync(item.AveList, item.AveItem, item.ParentId, item.ParentItemRule, item.ExcludePath, item.SpSetting, item.IdUtil, cts).Wait();
                    });
                }
                else
                {
                    foreach (var item in odCacheItems)
                    {
                        ProcessIncrementalSmartAutoCacheOneItemAsync(item.AveList, item.AveItem, item.ParentId, item.ParentItemRule, item.ExcludePath, item.SpSetting, item.IdUtil).Wait();
                    }
                }
            }
        }

        public virtual async System.Threading.Tasks.Task ProcessIncrementalSmartAutoCacheOneItemAsync(IAveList list, IAveListItem aveItem, Guid parentId, SyncItemRuleInfo parentItemRule, List<string> excludePath, RMOneDriveSetting spSetting, UniqueIdUtil idUtil, CancellationTokenSource cts = null)
        {
            string itemName = string.Empty;
            string itemUrl = string.Empty;
            Guid termId = Guid.Empty;
            string termName = string.Empty;
            try
            {
                using (var performance = new PerformanceScope("RMOneDriveExplorerBase.ProcessIncrementalSmartAutoCacheOneItem", addToStatistics: true))
                {
                    JobContext.ReportManager.Increase();
                    if (NeedSkip(aveItem, excludePath))
                    {
                        logger.Info("item need skip.");
                        return;
                    }

                    if (!_isSyncStubFile && aveItem.IsStubItem())
                    {
                        logger.Debug($"Current item [{aveItem?.UniqueId}] is stub file, so skipped.");
                        return;
                    }
                    itemName = aveItem?.GetObjectName();
                    AvePoint.GCommon.Utility.ArgumentCheck.NotNull(aveItem, nameof(aveItem));
                    itemUrl = aveItem.FullPath();
                    using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        Record recordInDB = null;
                        Record aiRecord = null;
                        var scopeId = mAveSite.ID;
                        var itemGuid = IDGenerator.GetRecordId(mAveSite.ID, aveItem.UniqueId);
                        using (var performance1 = new PerformanceScope("RMSPExplorerProcessor.GetDBRecord", addToStatistics: true))
                        {
                            WaitCosmosExecuteAction(() =>
                            {
                                recordInDB = ExplorerDao.ReadById(scopeId, itemGuid);
                            });
                        }

                        RMTermInfo termInfo;
                        using (var performance2 = new PerformanceScope("RMSPExplorerProcessor.GetTermInfo", addToStatistics: true))
                        {
                            (termId, termName, aiRecord) = await AssignPredictTermAsync(aveItem, spSetting, recordInDB, termId, termName, aiRecord);
                            termInfo = new RMTermInfo { UniqueId = termId, Name = termName };
                        }
                        RMRuleItemCollection rules = null;
                        SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
                        using (var performance1 = new PerformanceScope("RMSPExplorerProcessor.CheckRule", addToStatistics: true))
                        {
                            if (RMOneDriveExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                            {
                                var newRuleCollection = RebuldSPRules(rules);
                                if (newRuleCollection.Rules.Count == 0)
                                {
                                    logger.Info($"No SP rules realted to the item {list.RootFolder.Url}:{aveItem.ID}");
                                }
                                else
                                {
                                    var filterEnginer = new RMOneDriveRuleChecker(newRuleCollection);
                                    itemRuleInfo = filterEnginer.CheckDisposalRule(aveItem, parentItemRule);
                                }
                            }
                        }
                        itemRuleInfo.TermInfo = termInfo;
                        var item = syncItem.AssembleRecord(aveItem, parentId, itemRuleInfo);

                        //check uniqueId
                        UpdateRecordId(item, recordInDB, idUtil);
                        UpdateAIPredictInfo(item, aiRecord, recordInDB);
                        bool labelNotExist = UpdateLabel(aveItem, termInfo.UniqueId, item.Id, recordInDB);
                        SyncItemToDB(item, recordInDB, labelNotExist, recordInDB != null);
                    }
                }
                var predictResultFail = RMMLPredictHelper.GetPredictRequestFailCache(aveItem.UniqueId);
                if (predictResultFail != null)
                {
                    JobContext.HasErrorNode = true;
                }
            }
            catch (JobStopException)
            {
                cts?.Cancel();
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                logger.Error($"error occurred while Process smart auto change aveitem:{itemUrl}, ERROR:{e.ToString()}");
                bool isItemNotFound = this.isItemNotFoundError(e);
                if (!isItemNotFound)
                {
                    JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                    {
                        ObjectName = itemName,
                        FullPath = itemUrl,
                        Status = JobDetailsStatus.Failed,
                        Comment = GetExceptionMessage(e),
                    });
                    this.AddFailureItem2Cache(aveItem, parentId, termId, e);
                }
            }
            return;
        }

        public virtual void ProcessIncrementalDeleteItem(IAveList list, Dictionary<string, object> itemChangeProperties, CancellationTokenSource cts = null)
        {
            IAveListItem aveItem = null;
            int rowId = 0;
            try
            {
                JobContext.ReportManager.Increase();
                int itemId = (int)itemChangeProperties["ItemId"];
                rowId = itemId;
                int itemChangeType = (int)itemChangeProperties["ChangeType"];
                Guid itemUniqueId = (Guid)itemChangeProperties["UniqueId"];
                logger.Info($"Process changed item:Id:{itemId}.UniqueId:{itemUniqueId}.ChangeType:{itemChangeType}.");
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
                        logger.Info($"cannot found item object ID:{itemId} Guid:{itemUniqueId} :{ex.ToString()}");

                        RemoveSPObj(itemUniqueId, itemId);
                    }
                    logger.Warn("remove view item, {0}.", itemId);
                }
            }
            catch (JobStopException)
            {
                cts?.Cancel();
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                logger.Error($"error occurred while Process ProcessIncrementalDeleteItem:{rowId}, ERROR:{e.ToString()}");
            }
        }

        public virtual void ProcessAveItems(IEnumerable<IAveListItem> items, Guid parentId, SyncItemRuleInfo parentItemRule, List<string> excludePath, RMOneDriveSetting spSetting)
        {
            logger.Info($"Process item count:[{items.Count()}]");
            JobContext.ReportManager.IncreaseBase(items.Count());
            RMMLAutoSmartItemsCache.Instance.Init(ProcessSmartAutoCacheAveItemsAction);
            if (items.Count() > itemsPerTask)
            {
                var cts = new System.Threading.CancellationTokenSource();
                AveTenantTasks.RunParallelBatch(items, itemsPerTask, cts, item =>
                {
                    ProcessAveItemBatch(item, parentId, parentItemRule, excludePath, spSetting, cts);
                });
            }
            else
            {
                //foreach (var item in items)
                //{
                //    ProcessAveItem(item, parentId, parentItemRule);
                //}
                ProcessAveItemBatch(items, parentId, parentItemRule, excludePath, spSetting);
            }

            if (RMMLAutoSmartItemsCache.Instance.NeedProcessCache)
            {
                RMMLAutoSmartItemsCache.Instance.SetFinished();
                RMMLAutoSmartItemsCache.Instance.Dispose();
            }
        }

        private void ProcessSmartAutoCacheAveItemsAction(List<AutoSmartCacheItemInfo> cacheItems)
        {
            var totalCount = cacheItems?.Count;
            if (totalCount > 0)
            {
                logger.Info($"it's need process auto smart cache items, count:{totalCount}");
                var odCacheItems = cacheItems.ConvertAll(o => (OneDriveAutoSmartCacheItemInfo)o);
                var aveItems = odCacheItems.Select(o => o.AveItem).ToList();
                var spSetting = odCacheItems.Select(o => o.SpSetting).First();
                RMOneDriveMachineLearningUtility.StartPredictTerm(GetNeedPredictItems(aveItems), mSettingSiteId, spSetting.SiteGroupId);
                if (totalCount > smartAutoCacheitemsPerTask)
                {
                    var cts = new System.Threading.CancellationTokenSource();
                    AveTenantTasks.RunParallel(odCacheItems, smartAutoCacheitemsPerTask, cts, item =>
                    {
                        ProcessIncrementalSmartAutoCacheOneItemAsync(item.AveList, item.AveItem, item.ParentId, item.ParentItemRule, item.ExcludePath, item.SpSetting, item.IdUtil, cts).Wait();
                    });
                }
                else
                {
                    foreach (var item in odCacheItems)
                    {
                        ProcessIncrementalSmartAutoCacheOneItemAsync(item.AveList, item.AveItem, item.ParentId, item.ParentItemRule, item.ExcludePath, item.SpSetting, item.IdUtil).Wait();
                    }
                }
            }
        }

        public async System.Threading.Tasks.Task ProcessSmartAutoAveItemBatchAsync(IEnumerable<IAveListItem> items, Guid parentId, SyncItemRuleInfo parentItemRule, List<string> excludePath, RMOneDriveSetting spSetting, CancellationTokenSource cts = null)
        {
            var itemCount = items.Count();
            logger.Info($"Start to process smart auto items batch, count: {itemCount}");
            if (itemCount > 0)
            {
                Guid siteId = DiscoverSite.SiteID;
                Dictionary<Guid, string> itemAndOwnerMapping = new Dictionary<Guid, string>();
                using (var performance = new PerformanceScope("RMSPExplorerProcessor.AssembleItemAndOwnerMapping", addToStatistics: true))
                {
                    List<Guid> nodeIdList = items.Select(a => a.UniqueId).ToList();
                    List<RMManualApprove> mas = RMManualApproveDao.GetManualApproveByNodes(siteId, nodeIdList);
                    logger.Info("Manual approve count {0}", mas.Count);
                    itemAndOwnerMapping = AssembleItemAndOwnerMappingNew(mas, nodeIdList);
                }

                UniqueIdUtil idUtil = null;
                using (var performance = new PerformanceScope("RMSPExplorerProcessor.GenerateUniqueIds", addToStatistics: true))
                {
                    idUtil = new UniqueIdUtil(TenantLocalValue.LogonGroupId, itemCount);
                }
                foreach (IAveListItem aveItem in items)
                {
                    JobContext.ReportManager.Increase();
                    if (NeedSkip(aveItem, excludePath))
                    {
                        continue;
                    }
                    string itemName = aveItem?.Name;
                    string itemUrl = aveItem?.FullPath();
                    Guid termId = Guid.Empty;
                    try
                    {
                        using (var performance = new PerformanceScope("RMOneDriveExplorerBase.InnerProcessSmartAutoCacheAveItem", addToStatistics: true))
                        {
                            termId = await InnerProcessSmartAutoCacheAveItemAsync(aveItem, parentId, parentItemRule, itemAndOwnerMapping, spSetting, termId, idUtil);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error($"error occurred while Process smart auto aveitem:{itemUrl}, ERROR:{e.ToString()}");
                        this.AddExceptionListDic(this.GetListId(aveItem));
                        JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                        {
                            ObjectName = itemName,
                            FullPath = itemUrl,
                            Status = JobDetailsStatus.Failed,
                            Comment = this.GetExceptionMessage(e),
                        });
                        this.AddFailureItem2Cache(aveItem, parentId, termId, e);
                    }
                }
            }
        }


        [Obsolete("use ProcessAveItemBatch in data sync full")]
        public void ProcessAveItem(IAveListItem aveItem, Guid parentId, SyncItemRuleInfo parentItemRule, RMOneDriveSetting setting, CancellationTokenSource cts = null)
        {
            JobContext.ReportManager.Increase();
            if (aveItem != null && aveItem.FileSystemObjectType == AveFileSystemObjectType.Folder)
            {
                IAveFolder folder = aveItem.Folder;
                if (folder != null)
                {
                    ProcessAveFolder(folder, parentId, parentItemRule, null);
                }
                return;
            }
            string itemName = aveItem?.Name; // string.Empty;
            string itemUrl = aveItem?.Url; // string.Empty;
            Guid recordId = Guid.Empty;
            Guid termId = Guid.Empty;
            try
            {
                using (var performance = new PerformanceScope("SP.RMOneDriveExplorerProcessor.ProcessAveItem"))
                {
                    AvePoint.GCommon.Utility.ArgumentCheck.NotNull(aveItem, nameof(aveItem));
                    if (aveItem.ParentList.BaseType == AveBaseType.DocumentLibrary && NeedSkipFile(aveItem, itemName))
                    {
                        return;
                    }
                    InnerProcessAveItem(aveItem, parentId, parentItemRule, new Dictionary<Guid, string>(), setting, ref termId, null);
                }
            }
            catch (Exception e)
            {
                logger.Error($"error occurred while Process aveitem:{itemUrl}, ERROR:{e.ToString()}");
                JobContext.HasErrorNode = true;
                _siteCache.HasErrorNode = true;
                this.AddExceptionListDic(this.GetListId(aveItem));
                JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                {
                    ObjectName = itemName,
                    FullPath = itemUrl,
                    Status = JobDetailsStatus.Failed,
                    Comment = this.GetExceptionMessage(e),
                });
            }
        }

        private bool NeedSkip(IAveListItem item, List<string> excludePaths)
        {
            if (excludePaths != null)
            {
                string itemPath = item["FileRef"].ToString();
                foreach (var excludePath in excludePaths)
                {
                    if (itemPath.StartsWith(excludePath) && itemPath != excludePath)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        public void ProcessAveItemBatch(IEnumerable<IAveListItem> aveItems, Guid parentId, SyncItemRuleInfo parentItemRule, List<string> excludePath, RMOneDriveSetting spSetting, CancellationTokenSource cts = null)
        {
            logger.Info("Start to process items batch.");
            List<IAveListItem> folders = aveItems.Where(a => a != null && a.FileSystemObjectType == AveFileSystemObjectType.Folder).ToList();
            logger.Info("Folders count is {0}", folders.Count);
            if (folders.Count > 0)
            {
                UniqueIdUtil idUtil = null;
                using (var performance = new PerformanceScope("RMSPExplorerProcessor.GenerateUniqueIds", addToStatistics: true))
                {
                    idUtil = new UniqueIdUtil(TenantLocalValue.LogonGroupId, folders.Count);
                }
                foreach (IAveListItem item in folders)
                {
                    IAveFolder folder = item.Folder;
                    if (!NeedSkip(item, excludePath))
                    {
                        if (folder != null)
                        {
                            ProcessAveFolder(folder, parentId, parentItemRule, idUtil);
                        }
                    }
                }
            }
            
            List<IAveListItem> items = aveItems.Where(a => a != null && a.FileSystemObjectType != AveFileSystemObjectType.Folder && !(a.ParentList.BaseType == AveBaseType.DocumentLibrary && NeedSkipFile(a, a.Name))).ToList();
            logger.Info("Items count is {0}", items.Count);
            if (items.Count > 0)
            {
                Guid siteId = DiscoverSite.SiteID;
                Dictionary<Guid, string> itemAndOwnerMapping = new Dictionary<Guid, string>();
                using (var performance = new PerformanceScope("RMSPExplorerProcessor.AssembleItemAndOwnerMapping", addToStatistics: true))
                {
                    List<Guid> nodeIdList = items.Select(a => a.UniqueId).ToList();
                    List<RMManualApprove> mas = RMManualApproveDao.GetManualApproveByNodes(siteId, nodeIdList);
                    logger.Info("Manual approve count {0}", mas.Count);

                    itemAndOwnerMapping = AssembleItemAndOwnerMappingNew(mas, nodeIdList);
                }

                

                UniqueIdUtil idUtil = null;
                using (var performance = new PerformanceScope("RMSPExplorerProcessor.GenerateUniqueIds", addToStatistics: true))
                {
                    idUtil = new UniqueIdUtil(TenantLocalValue.LogonGroupId, items.Count);
                }
                foreach (IAveListItem aveItem in items)
                {
                    JobContext.ReportManager.Increase();
                    if (NeedSkip(aveItem, excludePath))
                    {
                        continue;
                    }
                    //if (IsRemoteItem(aveItem))
                    //{
                    //    logger.Info("Current item is remote item, will not sync it to db. Id:{0}", aveItem.ID);
                    //}
                    string itemName = aveItem?.Name; // string.Empty;
                    string itemUrl = aveItem?.FullPath(); // string.Empty; 
                    Guid termId = Guid.Empty;
                    try
                    {
                        using (var performance = new PerformanceScope("RMOneDriveExplorerBase.InnerProcessAveItem", addToStatistics: true))
                        {
                            using (CheckJobStopScope jScope = new CheckJobStopScope())
                            {
                                InnerProcessAveItem(aveItem, parentId, parentItemRule, itemAndOwnerMapping, spSetting, ref termId, idUtil);
                            } 
                        }
                    }
                    catch (JobStopException)
                    {
                        throw new JobStopException("the job has stopped.");
                    }
                    catch (Exception e)
                    {
                        logger.Error($"error occurred while Process aveitem:{itemUrl}, ERROR:{e.ToString()}");
                        //JobContext.HasErrorNode = true;
                        //_siteCache.HasErrorNode = true;
                        this.AddExceptionListDic(this.GetListId(aveItem));
                        JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                        {
                            ObjectName = itemName,
                            FullPath = itemUrl,
                            Status = JobDetailsStatus.Failed,
                            Comment = this.GetExceptionMessage(e),
                        });
                        this.AddFailureItem2Cache(aveItem, parentId, termId, e);
                    }
                }
            }
        }

        //private bool IsRemoteItem(IAveListItem item)
        //{
        //    try
        //    {
        //        if (item.Properties != null && item.Properties.ContainsKey("vti_a2od_ismountpoint") && Convert.ToBoolean(item.Properties["vti_a2od_ismountpoint"]))
        //        {
        //            return true;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Warn("An error occurred while checking if item is remote item. Error:{0}", e.ToString());
        //    }
        //    return false;
        //}

        public bool IsBlockEditAndDeleteRecord(IAveListItem item)
        {
            return IsBlockEditAndDeleteRecord(GetHoldAndRecordStatus(item));
        }

        public bool IsBlockEditAndDeleteRecord(int holdAndRecordStatus)
        {
            return ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.RecordMask) != 0L) && ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.EditBlockedMask) != 0L) && ((holdAndRecordStatus & (int)HoldAndRecordStatusMask.DeleteBlockedMask) != 0L);
        }

        private static int GetHoldAndRecordStatus(IAveListItem item)
        {
            int result = 0;
            try
            {
                if ((GetBoolIprPropertyCore(item.ParentList, "ecm_ListFieldsReadyForIPR")) || IsHoldOrRecordsEnabled(item.ParentList))
                {
                    try
                    {
                        if (item.Fields.Contains(HoldRecordStatus))
                        {
                            object obj2 = item[HoldRecordStatus];
                            if ((obj2 != null) && !int.TryParse(obj2.ToString(), out result))
                            {
                                result = 0;
                            }
                        }
                    }
                    catch (ArgumentException)
                    {
                        result = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn(string.Format("An error occur in get hold and declare status, reason : {0}.", ex.ToString()));
            }
            return result;
        }
        internal static Guid HoldRecordStatus
        {
            get
            {
                return new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E");
            }
        }
        private static bool GetBoolIprPropertyCore(IAveList list, string propName)
        {
            bool? nullable = null;
            if (list != null && list.RootFolder != null && list.RootFolder.Properties != null)
            {
                object obj = list.RootFolder.Properties[propName];
                if (obj != null) nullable = new bool?(obj.ToString().Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase));
            }
            return (nullable == true);
        }
        private static bool IsHoldOrRecordsEnabled(IAveList list)
        {
            if (list == null || list.Fields == null)
            {
                throw new ArgumentNullException("list");
            }
            if (list.Fields.Contains(new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")))
            {
                return (list.Fields[new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")] != null);
            }
            else
            {
                return false;
            }
        }
        internal enum HoldAndRecordStatusMask
        {
            EditBlockedMask = 1, //只要不允许编辑, 这位值就为1, 包括Hold 和 Block edit and delete
            RecordMask = 0x10, //Record 文件，这位值 就是1 ， 包含Block edit and delete， block delete
            DeleteBlockedMask = 0x100,//只要不允许删除，这位值就为1, 包括 Hold， block edit and delete， block delete
            HoldMask = 0x1000, //Hold 文件，这位值就是 1， 
        }
        private RMTermInfo AssignTerm(IAveListItem item, RMOneDriveSetting setting, Record dbRecord, Guid parentId, SyncItemRuleInfo parentItemRule, List<string> excludePath, UniqueIdUtil idUtil, ref Record aiRecord)
        {
            if (!NeedAssignTerm(setting, item.ParentList))
            {
                if (dbRecord != null)
                {
                    return new RMTermInfo() { UniqueId = dbRecord.TermId, Name = dbRecord.TermName };
                }
                else
                {
                    return new RMTermInfo();
                }
            }
            if (IsBlockEditAndDeleteRecord(item))
            {
                logger.Info("Item is Block Edit and delete {0}", item.Name);
                //*****ReportService.Commit(new SPSettingJobReportEntry(item.Name, item.Url, "",
                //string.Empty, "RM_SS_ApplyExist", JobReportDetailStatus.Skipped, "RM_SS_ItemBlockEditAndDelete"));
                if (!setting.IncludeDeclaredRecords)
                {
                    if (dbRecord != null)
                    {
                        return new RMTermInfo() { UniqueId = dbRecord.TermId, Name = dbRecord.TermName };
                    }
                    else
                    {
                        return new RMTermInfo();
                    }
                }
            }
            if (NeedCheckExistingRecord(setting))
            {
                if (dbRecord != null && dbRecord.TermId != Guid.Empty)
                {
                    logger.Info("Record already has a term, no need to assign trem. Id:{0}", item.ID);
                    return new RMTermInfo() { UniqueId = dbRecord.TermId, Name = dbRecord.TermName };
                }
            }

            Guid termid = setting.DefaultTermId;
            string termName = string.Empty;
            if (setting.DeployTermMethod == (int)DeployTermMethod.UseAutoClassification)
            {
                List<Rule> autoRules = RMOneDriveExplorerDataCache.Instance.AutoRuleCollections[setting.ScopeId];
                var ruleManagement = new RuleManagement(GetRuleCollection(autoRules));
                var itemRuleInfo = ruleManagement.CheckItemCriteria(item.UniqueId, item);
                string key = string.Empty;
                if (itemRuleInfo != null)
                {
                    key = setting.ScopeId.ToString() + "_" + itemRuleInfo.Id;
                }
                else
                {
                    key = setting.ScopeId.ToString() + "_" + Guid.Empty.ToString();
                }

                //Auto Ai
                if (itemRuleInfo == null && setting.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault)
                {
                    RMMLAutoSmartItemsCache.Instance.ProcessItem(new OneDriveAutoSmartCacheItemInfo {
                        AveList = item.ParentList,
                        AveItem = item,
                        ParentId = parentId,
                        ParentItemRule = parentItemRule,
                        ExcludePath = excludePath,
                        SpSetting = setting,
                        IdUtil = idUtil,
                    });
                    return null;
                }
                else
                {
                    termid = RMOneDriveExplorerDataCache.Instance.AutoRuleIdTermIdMapping[key];
                }
            }

            //Use AI
            if (setting.DeployTermMethod == (int)DeployTermMethod.UseIntelligenceClassification && setting.AITermUseType == ArtificialIntelligenceTermUseType.ApplyTerm)
            {
                RMMLAutoSmartItemsCache.Instance.ProcessItem(new OneDriveAutoSmartCacheItemInfo
                {
                    AveList = item.ParentList,
                    AveItem = item,
                    ParentId = parentId,
                    ParentItemRule = parentItemRule,
                    ExcludePath = excludePath,
                    SpSetting = setting,
                    IdUtil = idUtil,
                });
                return null;
            }

            if (RMOneDriveExplorerDataCache.Instance.Terms.ContainsKey(termid))
            {
                var tempTerm = RMOneDriveExplorerDataCache.Instance.Terms[termid];
                var termInvalid = false;
                if (tempTerm == null || tempTerm.IsDeprecated || tempTerm.IsRemoved)
                {
                    termInvalid = true;
                }
                AvePoint.GCommon.Utility.ArgumentCheck.NotNull(tempTerm, nameof(tempTerm));
                if (tempTerm.TermExpirationFrom != 0 || tempTerm.TermExpirationTo != 0)
                {
                    if (DateTime.UtcNow.Ticks < tempTerm.TermExpirationFrom || (tempTerm.TermExpirationTo != 0 && DateTime.UtcNow.Ticks > tempTerm.TermExpirationTo))
                    {
                        termInvalid = true;
                    }
                }
                if (termInvalid)
                {
                    logger.Warn("Term is invalid [{0}].", termid);
                    throw new Exception("RM_FS_DisposalDetail_TermIsInvalid" + I18NEntity.Separator + tempTerm.Name);
                }
                termName = RMOneDriveExplorerDataCache.Instance.Terms[termid].Name;
            }
            else
            {
                logger.Warn("Cannot find the term with id [{0}] from the cache.", termid);
            }

            if (termid == Guid.Empty && dbRecord != null && dbRecord.TermId != Guid.Empty)
            {
                termid = dbRecord.TermId;
            }
            if (string.IsNullOrWhiteSpace(termName) && dbRecord != null && !string.IsNullOrWhiteSpace(dbRecord.TermName))
            {
                termName = dbRecord.TermName;
            }
            return new RMTermInfo() { UniqueId = termid, Name = termName };
        }


        private async System.Threading.Tasks.Task<(Guid, string, Record)> AssignPredictTermAsync(IAveListItem item, RMOneDriveSetting setting, Record dbRecord, Guid termid, string termName, Record aiRecord)
        {
            var predictTerm = RMOneDriveMachineLearningUtility.GetFilePredictTerm(item.UniqueId);
            if (predictTerm != null)
            {
                if (setting.AIApprovalType == DB.Model.ApprovalType.None)
                {
                    termid = predictTerm.Id;
                    termName = predictTerm.Name;
                    logger.Info($"use ai predict term, itemId: [{item.UniqueId}], termid: [{predictTerm.Id}]");
                    //标记AI直接打的Term
                    AppendAIAutoApplyInfo(ref aiRecord, predictTerm);

                }
                else if (setting.AIApprovalType == DB.Model.ApprovalType.RecordOwners)
                {
                    if (predictTerm.AutoApply)
                    {
                        //当前数据使用的Term，开启了AutoApply，需要在CosmosDB中标记是直接打的
                        termid = predictTerm.Id;
                        termName = predictTerm.Name;
                        logger.Info($"use ai predict term, because term is auto apply, itemId: [{item.UniqueId}], termid: [{predictTerm.Id}]");
                        AppendAIAutoApplyInfo(ref aiRecord, predictTerm);
                    }
                    else
                    {
                        logger.Info($"it is ai manual data, itemId: [{item.UniqueId}], predict termid: [{predictTerm.Id}]");
                        //标记数据是Manual状态的数据
                        termid = Guid.Empty;
                        termName = string.Empty;
                        var recordOwners = await RMMachineLearningReviewerUtility.GetRecordOwnersAsync(setting.Id, RecordOwnerSettingType.AIOneDrive);
                        if (setting.AISendEMail)
                        { 
                            RMMLManualApprovalEmailSender.AddNeedSendEmailUserId(recordOwners);
                        }
                        AppendAIManualInfo(ref aiRecord, predictTerm, recordOwners);
                    }
                }
            }
            else
            {
                //AI预测Term没有结果时处理逻辑
                if (setting.AIThenIsDefaultTermMethod)
                {
                    termid = setting.AIThenDefaultTermId;
                    termName = setting.AIThenDefaultTermName;
                    logger.Info($"use ai then default term, itemId: [{item.UniqueId}], termid: [{setting.AIThenDefaultTermId}]");
                }
                else
                {
                    termid = Guid.Empty;
                    termName = string.Empty;
                    logger.Info($"skip to append predict term info, itemId:{item?.UniqueId}");
                }
            }

            if (dbRecord != null)
            {
                if (termid == Guid.Empty && dbRecord.TermId != Guid.Empty)
                {
                    termid = dbRecord.TermId;
                }
                if (string.IsNullOrWhiteSpace(termName) && !string.IsNullOrWhiteSpace(dbRecord.TermName))
                {
                    termName = dbRecord.TermName;
                }
            }

            return (termid, termName, aiRecord);

        }


        /// <summary>
        /// 获取支持预测的Documents
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private List<IAveListItem> GetNeedPredictItems(IEnumerable<IAveListItem> items)
        {
            List<IAveListItem> result = items.Where(a => a != null && a.FileSystemObjectType != AveFileSystemObjectType.Folder && !(a.ParentList.BaseType == AveBaseType.DocumentLibrary && NeedSkipFile(a, a.Name))).ToList();
            return result;
        }

        private void UpdateAIPredictInfo(Record targetRecord, Record aiRecord, Record dbRecord)
        {
            if (aiRecord == null)
            {
                return;
            }
            bool hasValueChanged = false;
            if (dbRecord != null)
            {
                hasValueChanged = dbRecord.PredictTermId != aiRecord.PredictTermId || dbRecord.MLUnderReview != aiRecord.MLUnderReview
                                || dbRecord.MLClassificationType != aiRecord.MLClassificationType || dbRecord.MLApprovalStatus != aiRecord.MLApprovalStatus
                                || dbRecord.TrainingModelId != aiRecord.TrainingModelId;
            }

            if (dbRecord == null || hasValueChanged)
            {
                targetRecord.PredictTermId = aiRecord.PredictTermId;
                targetRecord.PredictTermScore = aiRecord.PredictTermScore;
                targetRecord.PredictTime = DateTime.UtcNow.Ticks;
                targetRecord.MLUnderReview = aiRecord.MLUnderReview;
                targetRecord.MLClassificationType = aiRecord.MLClassificationType;
                targetRecord.MLReviewer = aiRecord.MLReviewer;
                targetRecord.MLApprovalStatus = aiRecord.MLApprovalStatus;
                targetRecord.MLEscalateFrom = aiRecord.MLEscalateFrom;
                targetRecord.MLEscalatedComment = aiRecord.MLEscalatedComment;
                targetRecord.TrainingModelId = aiRecord.TrainingModelId;
            }
            else 
            {
                targetRecord.PredictTermId = dbRecord.PredictTermId;
                targetRecord.PredictTermScore = dbRecord.PredictTermScore;
                targetRecord.PredictTime = DateTime.UtcNow.Ticks;
                targetRecord.MLUnderReview = dbRecord.MLUnderReview;
                targetRecord.MLClassificationType = dbRecord.MLClassificationType;
                targetRecord.MLReviewer = dbRecord.MLReviewer;
                targetRecord.MLApprovalStatus = dbRecord.MLApprovalStatus;
                targetRecord.MLEscalateFrom = dbRecord.MLEscalateFrom;
                targetRecord.MLEscalatedComment = dbRecord.MLEscalatedComment;
                targetRecord.TrainingModelId = dbRecord.TrainingModelId;
            }
        }

        private void AppendAIAutoApplyInfo(ref Record record, MLTermDto predictTerm)
        {
            //AI直接打Term的数据信息，或者Setting开启Manual，但是训练Term开启了AutoApply，都视为AutoApply, 走Manual流程
            if (record == null)
            {
                record = new Record();
            }
            record.PredictTermId = predictTerm.Id;
            record.PredictTermScore = predictTerm.PredictTermScore;
            record.PredictTime = DateTime.UtcNow.Ticks;
            record.PredictTermScore = predictTerm.PredictTermScore;
            record.MLUnderReview = (int)RMMLUnderReview.DirectAssign;
            record.MLClassificationType = (int)RMMLClassificationType.AutoClassfied;
            record.MLApprovalStatus = (int)RMMLApprovalStatus.None;
            record.TrainingModelId = RMMLPredictHelper.DefaultTrainingModeId;
            record.MLEscalateFrom = 0;
            record.MLEscalatedComment = "";
        }

        private void AppendAIManualInfo(ref Record record, MLTermDto predictTerm, int[] reviewers)
        {
            //Setting开启Manual，保存AI预测信息到DB，走Manual流程  
            if (record == null)
            {
                record = new Record();
            }
            record.PredictTermId = predictTerm.Id;
            record.PredictTermScore = predictTerm.PredictTermScore;
            record.PredictTime = DateTime.UtcNow.Ticks;
            record.MLUnderReview = (int)RMMLUnderReview.IsManual;
            //等在ManualReview页面操作后去修改此值为ManualClassified
            record.MLClassificationType = (int)RMMLClassificationType.None; 
            record.MLReviewer = reviewers;
            record.MLApprovalStatus = (int)RMMLApprovalStatus.WaitingApprove;
            record.TrainingModelId = RMMLPredictHelper.DefaultTrainingModeId;
            record.MLEscalateFrom = 0;
            record.MLEscalatedComment = "";
        }

        private Dictionary<Guid, string> AssembleItemAndOwnerMappingNew(List<RMManualApprove> mas, List<Guid> allNodeIds)
        {
            Dictionary<Guid, string> tempMapping = new Dictionary<Guid, string>();
            foreach (var ma in mas)
            {
                var ownerIds = RMManualApproveDao.GetManualApproveOwnerIds(ma);
                if (!tempMapping.ContainsKey(ma.NodeId))
                {
                    tempMapping.Add(ma.NodeId, string.Join("|", ownerIds) + "|");
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

        public void InnerProcessAveItem(IAveListItem aveItem, Guid parentId, SyncItemRuleInfo parentItemRule, Dictionary<Guid, string> itemAndOwnerMapping, RMOneDriveSetting setting, ref Guid termId, UniqueIdUtil uniqueIdUtil)
        {
            JobContext.ReportManager.Increase();
            if (!_isSyncStubFile && aveItem.IsStubItem())
            {
                logger.Debug($"Current item [{aveItem?.UniqueId}] is stub file, so skipped.");
                return;
            }
            Record recordInDB = null;
            Record aiRecord = null;
            var scopeId = mAveSite.ID;
            var itemId = IDGenerator.GetRecordId(mAveSite.ID, aveItem.UniqueId);
            using (var performance = new PerformanceScope("RMSPExplorerProcessor.GetDBRecord", addToStatistics: true))
            {
                WaitCosmosExecuteAction(() =>
                {
                    recordInDB = ExplorerDao.ReadById(scopeId, itemId);
                });
            }
            RMTermInfo termInfo;
            using (var performance = new PerformanceScope("RMSPExplorerProcessor.GetTermInfo", addToStatistics: true))
            {
                termInfo = AssignTerm(aveItem, setting, recordInDB, parentId, parentItemRule,  null, uniqueIdUtil, ref aiRecord);
            }
            if (termInfo == null) return;
            termId = termInfo.UniqueId;
            //var termInfo = GetTermInfo(aveItem, aveItem.Fields);
            RMRuleItemCollection rules = null;
            SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
            var key = aveItem.DirPath();
            using (var performance = new PerformanceScope("RMSPExplorerProcessor.CheckParentRule", addToStatistics: true))
            {
                if (!CheckParentRule(key, ref itemRuleInfo))
                {
                    if (RMOneDriveExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                    {
                        var newRuleCollection = RebuldSPRules(rules);
                        if (newRuleCollection.Rules.Count == 0)
                        {
                            logger.Info($"No SP rules realted to the item {aveItem?.Url}");
                        }
                        else
                        {
                            var filterEnginer = new RMOneDriveRuleChecker(newRuleCollection);
                            itemRuleInfo = filterEnginer.CheckDisposalRule(aveItem, parentItemRule);
                        }
                    }
                    else if (parentItemRule.Rule != null)
                    {
                        itemRuleInfo.Rule = parentItemRule.Rule;
                        itemRuleInfo.DisposalAction = parentItemRule.DisposalAction;
                    }
                }
            }
            itemRuleInfo.TermInfo = termInfo;
            string owner = null;
            if (itemAndOwnerMapping.ContainsKey(aveItem.UniqueId))
            {
                owner = itemAndOwnerMapping[aveItem.UniqueId];
            }
            logger.Debug("Owner for record {0} is {1}", aveItem.UniqueId, owner);
            var item = syncItem.AssembleRecord(aveItem, parentId, itemRuleInfo, owner);

            //check uniqueId
            UpdateRecordId(item, recordInDB, uniqueIdUtil);
            UpdateAIPredictInfo(item, aiRecord, recordInDB);
            bool labelNotExist = UpdateLabel(aveItem, termInfo.UniqueId, item.Id, recordInDB);

            SyncItemToDB(item, recordInDB, labelNotExist, recordInDB != null);
        }

        public async System.Threading.Tasks.Task<Guid> InnerProcessSmartAutoCacheAveItemAsync(IAveListItem aveItem, Guid parentId, SyncItemRuleInfo parentItemRule, Dictionary<Guid, string> itemAndOwnerMapping, RMOneDriveSetting setting,  Guid termId, UniqueIdUtil uniqueIdUtil)
        {
            JobContext.ReportManager.Increase();
            Record recordInDB = null;
            Record aiRecord = null;
            var scopeId = mAveSite.ID;
            var itemId = IDGenerator.GetRecordId(mAveSite.ID, aveItem.UniqueId);
            using (var performance = new PerformanceScope("RMSPExplorerProcessor.GetDBRecord", addToStatistics: true))
            {
                WaitCosmosExecuteAction(() =>
                {
                    recordInDB = ExplorerDao.ReadById(scopeId, itemId);
                });
            }
            RMTermInfo termInfo;
            Guid termid = setting.DefaultTermId;
            string termName = string.Empty;

            using (var performance = new PerformanceScope("RMSPExplorerProcessor.GetTermInfo", addToStatistics: true))
            {
                (termId, termName, aiRecord) = await AssignPredictTermAsync(aveItem, setting, recordInDB, termId, termName, aiRecord);
                termInfo = new RMTermInfo { UniqueId = termId, Name = termName };
            }
            RMRuleItemCollection rules = null;
            SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
            var key = aveItem.DirPath();
            using (var performance = new PerformanceScope("RMSPExplorerProcessor.CheckParentRule", addToStatistics: true))
            {
                if (!CheckParentRule(key, ref itemRuleInfo))
                {
                    if (RMOneDriveExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                    {
                        var newRuleCollection = RebuldSPRules(rules);
                        if (newRuleCollection.Rules.Count == 0)
                        {
                            logger.Info($"No SP rules realted to the item {aveItem?.Url}");
                        }
                        else
                        {
                            var filterEnginer = new RMOneDriveRuleChecker(newRuleCollection);
                            itemRuleInfo = filterEnginer.CheckDisposalRule(aveItem, parentItemRule);
                        }
                    }
                    else if (parentItemRule.Rule != null)
                    {
                        itemRuleInfo.Rule = parentItemRule.Rule;
                        itemRuleInfo.DisposalAction = parentItemRule.DisposalAction;
                    }
                }
            }
            itemRuleInfo.TermInfo = termInfo;
            string owner = null;
            if (itemAndOwnerMapping.ContainsKey(aveItem.UniqueId))
            {
                owner = itemAndOwnerMapping[aveItem.UniqueId];
            }
            logger.Debug("Owner for record {0} is {1}", aveItem.UniqueId, owner);
            var item = syncItem.AssembleRecord(aveItem, parentId, itemRuleInfo, owner);

            //check uniqueId
            UpdateRecordId(item, recordInDB, uniqueIdUtil);
            UpdateAIPredictInfo(item, aiRecord, recordInDB);
            bool labelNotExist = UpdateLabel(aveItem, termInfo.UniqueId, item.Id, recordInDB);
            SyncItemToDB(item, recordInDB, labelNotExist, recordInDB != null);
            return termid;
        }


        private bool NeedAssignTerm(RMOneDriveSetting setting, IAveList list)
        {
            if (setting.DeployTermMethod == (int)DeployTermMethod.UseAutoClassification && list.BaseType == AveBaseType.DocumentLibrary)
            {
                return true;
            }

            if (setting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm)
            {
                return true;
            }

            if (setting.DeployTermMethod == (int)DeployTermMethod.UseIntelligenceClassification && setting.AITermUseType == ArtificialIntelligenceTermUseType.ApplyTerm)
            {
                return true;
            }
            return false;
        }

        private bool UpdateLabel(IAveListItem aveItem, Guid termId, Guid recordId, Record dbRecord)
        {
            using (var performance = new PerformanceScope("RMSPExplorerProcessor.UpdateLabel", addToStatistics: true))
            {
                bool labelNotExist = false;
                if (termId != Guid.Empty)
                {
                    //term id改变时才操作label
                    try
                    {
                        TermSettingsInfo termInfo = GetTermInfo(termId);
                        if (termInfo != null)
                        {
                            WaitSPOExecuteAction(() =>
                            {
                                if ((termInfo.EnforceRetention & (int)EnforceRetentionType.OneDrive) == (int)EnforceRetentionType.OneDrive)
                                {
                                    labelNotExist = ApplyComplianceTag(aveItem, recordId, termInfo, termId, dbRecord);
                                }
                                else
                                {
                                    if (dbRecord != null && dbRecord.TermId != termId)
                                    {
                                        //var previousTermInfo = GetTermInfo(dbRecord.TermId);
                                        //if (previousTermInfo != null)
                                        //{
                                        //    if ((previousTermInfo.EnforceRetention & (int)EnforceRetentionType.OneDrive) == (int)EnforceRetentionType.OneDrive)
                                        //    {
                                        RemoveComplianceTag(aveItem, recordId);
                                        //}
                                    }
                                }
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error("An error occurred while updating retention label. Item url:{0} Error:{1}", aveItem.FullPath(), e.ToString());
                    }
                }
                else
                {
                    //term id改变时才操作label
                    if (dbRecord != null && dbRecord.TermId != Guid.Empty)
                    {
                        //var previousTermInfo = GetTermInfo(dbRecord.TermId);
                        //if (previousTermInfo != null)
                        //{
                        //    if ((previousTermInfo.EnforceRetention & (int)EnforceRetentionType.OneDrive) == (int)EnforceRetentionType.OneDrive)
                        //    {
                        RemoveComplianceTag(aveItem, recordId);
                        //    }
                        //}
                    }
                }
                return labelNotExist;
            }
        }

        private TermSettingsInfo GetTermInfo(Guid termId)
        {
            TermSettingsInfo result = null;

            if (!RMOneDriveRetentionDataCache.Instance.TermRetentionMapping.TryGetValue(termId, out result))
            {
                var tempTerm = TermDao.GetParentInhertSetting(termId);
                if (tempTerm != null)
                {
                    result = new TermSettingsInfo() { EnforceRetention = tempTerm.EnforceRetention, OneDriveRetentionLabel = tempTerm.OneDriveRetentionLabel };
                    RMOneDriveRetentionDataCache.Instance.AddTermRetentionObj(termId, result);
                }
                else
                {
                    logger.Warn($"item term not exist in db:{termId}");
                    //throw new Exception($"term cannot be found, termId:{termId}");
                }
            }
            return result;
        }

        private bool ApplyComplianceTag(IAveListItem item, Guid recordId, TermSettingsInfo termInfo, Guid termId, Record dbRecord)
        {
            bool labelNotExist = false;
            using (var performance = new PerformanceScope("RMOneDriveExplorerBase.ApplyComplianceTag", addToStatistics: true))
            {
                var processingLabelName = RMOneDriveRetentionDataCache.Instance.LabelStateInfo.CurrentLabel.Name;
                var previousLabelNames = RMOneDriveRetentionDataCache.Instance.LabelStateInfo.PreviousLabelNames;
                AveComplianceTagInfo tagInfo = null;
                var itemUrl = item.FullPath();
                var currentLabel = item.GetComplianceTagName();
                // bool needApplyLabel = (!string.IsNullOrEmpty(previousLabelName) && currentLabel == previousLabelName && currentLabel != processingLabelName);


                logger.Info($"ApplyComplianceTag:RowId {item.ID} .currentLabel:{currentLabel}. processing lable:{processingLabelName}");
                if (NeedApplyLabel(item, termInfo, recordId, termId, dbRecord))
                {
                    if (RMOneDriveRetentionDataCache.Instance.SPSiteRetentionLables.TryGetValue(processingLabelName, out tagInfo))
                    {
                        using (var performance1 = new PerformanceScope("RMOneDriveExplorerBase.SetComplianceTag", addToStatistics: true))
                        {
                            //item.SetComplianceTag(tagInfo.TagName, tagInfo.BlockDelete, tagInfo.BlockEdit, tagInfo.IsEventTag, tagInfo.SuperLock);
                            item.SetComplianceTagOnBulkItems(tagInfo.TagName);
                        }

                        needUpdateLabelState = true;
                        logger.Info($"add item label:{processingLabelName}, Item RowId:{item.ID}");

                        //using (var performance2 = new PerformanceScope("SP.RMEnforceRetentionProcesser.sendReport"))
                        //{
                        //    JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                        //    {
                        //        ObjectName = item.GetObjectName(),
                        //        SourceURL = itemUrl,
                        //        Action = "RM_EXO_EnforceRetention_TagLabel",
                        //        Status = JobDetailsStatus.Successful,
                        //    });
                        //}
                    }
                    else
                    {
                        logger.Error($"SPLabel cannot be found:{processingLabelName}");
                        JobContext.HasErrorNode = true;
                        JobContext.NodeLevelError = true;
                        //AddFaildLabel(recordId);
                        labelNotExist = true;
                        //JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                        //{
                        //    ObjectName = item.GetObjectName(),
                        //    FullPath = itemUrl,
                        //    Status = JobDetailsStatus.Failed,
                        //    Comment = $"RM_JS_JM_EnforceRetention_LabelNotFound|I18NSplit|{processingLabelName}",
                        //});
                        //throw new Exception($"Label cannot be found, label name:{processingLabelName}");
                    }
                }
                else
                {
                    logger.Info($"skip item:Row Id {item.ID}, compliance tag:{processingLabelName} already exist.");
                }
            }
            return labelNotExist;
        }

        //以下下情况会给数据打Label
        //1.数据在cosmos db中没有记录，并且数据没有Label
        //2.数据在cosmos db中有记录，但是db中的term id和当前term id不一致
        private bool NeedApplyLabel(IAveListItem item, TermSettingsInfo termInfo, Guid recordId, Guid termId, Record dbRecord)
        {
            bool applyLabel = false;
            var processingLabelName = RMOneDriveRetentionDataCache.Instance.LabelStateInfo.CurrentLabel.Name;
            var previousLabelNames = RMOneDriveRetentionDataCache.Instance.LabelStateInfo.PreviousLabelNames;
            var currentLabel = item.GetComplianceTagName().ToLower();
            if (dbRecord == null)
            {
                if (!item.ExistComplianceTag())
                {
                    applyLabel = true;
                }
            }
            else
            {
                if (dbRecord.TermId != termId && (!item.ExistComplianceTag()
                    || (previousLabelNames.Count > 0 && previousLabelNames.Contains(currentLabel) && !currentLabel.Equals(processingLabelName, StringComparison.OrdinalIgnoreCase))))
                {
                    applyLabel = true;
                }
            }
            return applyLabel;
        }

        private void RemoveComplianceTag(IAveListItem item, Guid recordId)
        {
            using (var performance = new PerformanceScope("RMOneDriveExplorerBase.RemoveComplianceTag", addToStatistics: true))
            {
                try
                {
                    if (item.ExistComplianceTag())
                    {
                        var previousLabelNames = RMOneDriveRetentionDataCache.Instance.LabelStateInfo.PreviousLabelNames;
                        var currentLabel = item.GetComplianceTagName().ToLower();
                        var itemUrl = item.FullPath();
                        var needRemoveLabel = previousLabelNames.Contains(currentLabel);
                        logger.Info($"RemoveComplianceTag:RowId {item.ID}.currentLabel:{currentLabel}.");
                        //only remove tag of retention setting label
                        if (needRemoveLabel)
                        {
                            using (var performance1 = new PerformanceScope("RMOneDriveExplorerBase.UnSetComplianceTag", addToStatistics: true))
                            {
                                //item.SetComplianceTag(null, false, false, false, false);
                                item.SetComplianceTagOnBulkItems(string.Empty);
                            }
                            logger.Info($"remove item label:{currentLabel}, ItemRowId:{item.ID}");
                            //needUpdateLabelState = true;
                            //using (var performance2 = new PerformanceScope("SP.RMEnforceRetentionProcesser.sendReport"))
                            //{
                            //    JobContext.ReportManager.SendJobDetail(new JMEnforceRetentionJobDetail()
                            //    {
                            //        ObjectName = item.GetObjectName(),
                            //        SourceURL = itemUrl,
                            //        Action = "RM_EXO_EnforceRetention_RemoveLabel",
                            //        Status = JobDetailsStatus.Successful,
                            //    });
                            //}
                        }
                        else
                        {
                            logger.Info($"skip item:RowId {item.ID}, compliance tag:current:{currentLabel}.");
                        }
                    }
                    else
                    {
                        logger.Info($"skip item:RowId {item.ID}, item doesn't have  a label.");
                    }
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while removing label,ItemRowId:{0} error:{1}", item.ID, e.ToString());
                }
            }
        }

        public virtual void ProcessAveFolder(IAveFolder folder, Guid parentId, SyncItemRuleInfo parentItemRule, UniqueIdUtil idUtil)
        {
            try
            {
                using (var performance = new PerformanceScope("RMOneDriveExplorerBase.ProcessAveFolder", $"RMOneDriveExplorerBase.ProcessAveFolder:[{folder.Name}]", addToStatistics: true))
                {
                    logger.Info($"Process folder:{folder?.ServerRelativeUrl}");

                    JobContext.ReportManager.Increase();

                    //某些Hidden Folder Discover到 但取不到AveFolder
                    if (folder == null || folder.Properties == null || folder.Item == null)
                    {
                        logger.Warn("get folder occured error, folder is :{0}", folder.ServerRelativeUrl);
                        return;
                    }
                    var termInfo = GetTermInfo(folder.Item, folder.Item.Fields);
                    RMRuleItemCollection rules = null;
                    SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
                    var key = folder.ServerRelativeUrl + "/";
                    if (!CheckParentRule(key, ref itemRuleInfo))
                    {
                        if (RMOneDriveExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                        {
                            var newRuleCollection = RebuldSPRules(rules);
                            if (newRuleCollection.Rules.Count == 0)
                            {
                                logger.Info($"No SP rules realted to the folder {folder?.ServerRelativeUrl}");
                            }
                            else
                            {
                                var filterEnginer = new RMOneDriveRuleChecker(newRuleCollection);
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
                    Record recordInDB = null;
                    using (var performance0 = new PerformanceScope("RMOneDriveExplorerBase.GetDBRecord", addToStatistics: true))
                    {
                        WaitCosmosExecuteAction(() =>
                        {
                            recordInDB = ExplorerDao.ReadById(item.ScopeId, item.Id);
                        });
                    }
                    //check uniqueId
                    UpdateRecordId(item, recordInDB, idUtil);
                    item.KeepTermInfo(recordInDB);
                    SyncItemToDB(item);
                }
            }
            catch (Exception e)
            {
                logger.Error($"error occurred while Process folder:{folder?.ServerRelativeUrl}, ERROR:{e.ToString()}");
                JobContext.HasErrorNode = true;
                _siteCache.HasErrorNode = true;
                if (folder == null)
                {
                    logger.Error("Folder is null");
                }
                else
                {
                    this.AddExceptionListDic(folder.ParentListId);
                }
                JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                {
                    ObjectName = folder?.Name,
                    FullPath = folder?.Item?.FullPath(),
                    Status = JobDetailsStatus.Failed,
                    Comment = e.Message,
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

            if (RMSPExplorerListLevelCache.Instance.TryGetRuleByPrefix(key, out var parentRule, out var tempKey))
            {
                rule = parentRule;
                logger.Info($"folder meet parent rule: key:{key}, parentKey:{tempKey}");
                result = true;
            }
            return result;
        }


        private string GetExceptionMessage(Exception e)
        {
            bool getLATError = e.InnerException != null && !string.IsNullOrWhiteSpace(e.InnerException.Message) && e.InnerException.Message.StartsWith("The site do not meet the conditions.", StringComparison.OrdinalIgnoreCase);
            // "RM_SPS_LastAccessTimeQueryException" : e.Message;
            string comment = string.Empty;
            if (getLATError)
            {
                comment = "RM_SPS_LastAccessTimeQueryException";
            }
            else
            {
                comment = e.Message;
                if (e is System.Reflection.TargetInvocationException)
                {
                    System.Reflection.TargetInvocationException te = e as System.Reflection.TargetInvocationException;
                    if (te.InnerException != null)
                    {
                        comment = te.InnerException.Message;
                    }
                }
            }
            return comment;
        }
        //TODO fpwang
        [Obsolete("No use")]
        public virtual void ProcessFolder(IAveList aveList, AveDiscoverFolder discoverFolder, SyncItemRuleInfo parentItemRule)
        {
            try
            {
                using (var performance = new PerformanceScope("RMOneDriveExplorerBase.ProcessFolder", $"RMOneDriveExplorerBase.ProcessFolder:[{discoverFolder.LeafName}]", addToStatistics: true))
                {
                    logger.Info($"Process folder:{discoverFolder?.FullUrl}");
                    using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        JobContext.ReportManager.Increase();
                        //有一些Hidden Folder通过这个属性判断不出来， 优先判断Hidden属性
                        if (discoverFolder.Hidden.HasValue && discoverFolder.Hidden.Value)
                        {
                            logger.Info("skip hidden folder object {0} : {1}", aveList?.ID, discoverFolder?.FullUrl);
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
                            logger.Warn("get folder occured error, folder is :{0}", discoverFolder.FullUrl);
                            return;
                        }
                        var termInfo = GetTermInfo(aveFolder.Item, aveFolder.Item.Fields);
                        RMRuleItemCollection rules = null;
                        SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
                        if (RMOneDriveExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                        {
                            var newRuleCollection = RebuldSPRules(rules);
                            if (newRuleCollection.Rules.Count == 0)
                            {
                                logger.Info($"No SP rules realted to the folder {aveFolder?.ServerRelativeUrl}");
                            }
                            else
                            {
                                var filterEnginer = new RMOneDriveRuleChecker(newRuleCollection);
                                itemRuleInfo = filterEnginer.CheckDisposalRule(discoverFolder, parentItemRule);

                            }

                        }
                        itemRuleInfo.TermInfo = termInfo;
                        var item = syncItem.AssembleRecord(discoverFolder, discoverFolder.DocID, itemRuleInfo);
                        SyncItemToDB(item);

                        string pagerInfo = string.Empty;
                        do
                        {
                            logger.Info($"Get items under [{discoverFolder.FullUrl}] with pager. PagerInfo:[{pagerInfo}]");
                            var items = this.mDiscover.GetItems(aveList, discoverFolder, ref pagerInfo);
                            ProcessItems(aveList, items, discoverFolder.DocID, parentItemRule);
                        }
                        while (!string.IsNullOrEmpty(pagerInfo));

                        var folders = this.mDiscover.GetSubFolders(discoverFolder);
                        logger.Info($"Process folders under [{discoverFolder?.FullUrl}] Count:[{folders.LongCount()}]");
                        JobContext.ReportManager.IncreaseBase(folders.LongCount());
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
            catch (JobStopException)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                logger.Error($"error occurred while Process folder:{discoverFolder?.FullUrl}, ERROR:{e.ToString()}");
                JobContext.HasErrorNode = true;
                _siteCache.HasErrorNode = true;
                JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                {
                    ObjectName = discoverFolder?.LeafName,
                    FullPath = discoverFolder?.FullUrl,
                    Status = JobDetailsStatus.Failed,
                    Comment = GetExceptionMessage(e),
                });
            }

        }

        public virtual void ProcessItem(IAveList list, AveDiscoverItem discoverItem, Guid parentId, SyncItemRuleInfo parentItemRule, CancellationTokenSource cts = null)
        {
            string itemName = string.Empty;
            string itemUrl = string.Empty;
            IAveListItem aveItem = null;
            try
            {
                using (var performance = new PerformanceScope("RMOneDriveExplorerProcessor.ProcessItem"))
                {
                    JobContext.ReportManager.Increase();
                    if (discoverItem.ID == null || (discoverItem.Hidden != null && discoverItem.Hidden == true))
                    {
                        logger.Info($"skip hidden item:{discoverItem?.FullUrl}");
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
                        logger.Warn("remove view item, {0}", discoverItem?.FullUrl);
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

                    if (!_isSyncStubFile && aveItem.IsStubItem())
                    {
                        logger.Debug($"Current item [{aveItem?.UniqueId}] is stub file, so skipped.");
                        return;
                    }
                    itemUrl = aveItem.FullPath();
                    logger.Info($"Process item:{itemUrl}");
                    using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {

                        var termInfo = GetTermInfo(aveItem, aveItem.Fields);
                        RMRuleItemCollection rules = null;
                        SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
                        if (RMOneDriveExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                        {
                            var newRuleCollection = RebuldSPRules(rules);
                            if (newRuleCollection.Rules.Count == 0)
                            {
                                logger.Info($"No SP rules realted to the item {aveItem?.Url}");
                            }
                            else
                            {
                                var filterEnginer = new RMOneDriveRuleChecker(newRuleCollection);
                                itemRuleInfo = filterEnginer.CheckDisposalRule(aveItem, parentItemRule);

                            }

                        }
                        itemRuleInfo.TermInfo = termInfo;
                        var item = syncItem.AssembleRecord(aveItem, parentId, itemRuleInfo);
                        Record recordInDB = null;

                        WaitCosmosExecuteAction(() =>
                        {
                            recordInDB = ExplorerDao.ReadById(item.ScopeId, item.Id);
                        });
                        //check uniqueId
                        UpdateRecordId(item, recordInDB, null);

                        SyncItemToDB(item, recordInDB);


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
                JobContext.HasErrorNode = true;
                _siteCache.HasErrorNode = true;
                JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                {
                    ObjectName = itemName,
                    FullPath = itemUrl,
                    Status = JobDetailsStatus.Failed,
                    Comment = GetExceptionMessage(e),
                });
            }
            return;
        }

        public virtual void ProcessIncrementalChangedItemV1(IAveList list, IAveListItem aveItem, Guid parentId, SyncItemRuleInfo parentItemRule, List<string> excludePath, RMOneDriveSetting spSetting, UniqueIdUtil idUtil, CancellationTokenSource cts = null)
        {
            string itemName = string.Empty;
            string itemUrl = string.Empty;
            Guid termId = Guid.Empty;

            try
            {
                using (var performance = new PerformanceScope("RMOneDriveExplorerBase.ProcessIncrementalChangedItem", addToStatistics: true))
                {
                    JobContext.ReportManager.Increase();
                    if (NeedSkip(aveItem, excludePath))
                    {
                        return;
                    }
                    itemName = aveItem?.GetObjectName();
                    if (list.BaseType == AveBaseType.DocumentLibrary && NeedSkipFile(aveItem, itemName))
                    {
                        return;
                    }

                    if (!_isSyncStubFile && aveItem.IsStubItem())
                    {
                        logger.Debug($"Current item [{aveItem?.UniqueId}] is stub file, so skipped.");
                        return;
                    }
                    AvePoint.GCommon.Utility.ArgumentCheck.NotNull(aveItem, nameof(aveItem));
                    itemUrl = aveItem.FullPath();
                    if (aveItem.FileSystemObjectType == AveFileSystemObjectType.Folder)
                    {
                        IAveFolder folder = aveItem.Folder;
                        if (folder != null)
                        {
                            ProcessAveFolder(folder, parentId, parentItemRule, idUtil);
                        }
                        logger.Info($"Current list item is folder.Url:{itemUrl}.Id:{aveItem.ID}.");
                        return;
                    }
                    using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        Record recordInDB = null;
                        Record aiRecord = null;
                        var scopeId = mAveSite.ID;
                        var itemGuid = IDGenerator.GetRecordId(mAveSite.ID, aveItem.UniqueId);
                        using (var performance1 = new PerformanceScope("RMSPExplorerProcessor.GetDBRecord", addToStatistics: true))
                        {
                            WaitCosmosExecuteAction(() =>
                            {
                                recordInDB = ExplorerDao.ReadById(scopeId, itemGuid);
                            });
                        }
                        var termInfo = AssignTerm(aveItem, spSetting, recordInDB, parentId, parentItemRule, excludePath, idUtil, ref aiRecord);
                        if (termInfo == null) return;
                        termId = termInfo.UniqueId;
                        RMRuleItemCollection rules = null;
                        SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
                        using (var performance1 = new PerformanceScope("RMSPExplorerProcessor.CheckRule", addToStatistics: true))
                        {
                            if (RMOneDriveExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                            {
                                var newRuleCollection = RebuldSPRules(rules);
                                if (newRuleCollection.Rules.Count == 0)
                                {
                                    logger.Info($"No SP rules realted to the item {list.RootFolder.Url}:{aveItem.ID}");
                                }
                                else
                                {
                                    var filterEnginer = new RMOneDriveRuleChecker(newRuleCollection);
                                    itemRuleInfo = filterEnginer.CheckDisposalRule(aveItem, parentItemRule);
                                }
                            }
                        }
                        itemRuleInfo.TermInfo = termInfo;
                        var item = syncItem.AssembleRecord(aveItem, parentId, itemRuleInfo);

                        //check uniqueId
                        UpdateRecordId(item, recordInDB, idUtil);
                        UpdateAIPredictInfo(item, aiRecord, recordInDB);
                        bool labelNotExist = UpdateLabel(aveItem, termInfo.UniqueId, item.Id, recordInDB);

                        SyncItemToDB(item, recordInDB, labelNotExist, recordInDB != null);
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
                    //JobContext.HasErrorNode = true;
                    //_siteCache.HasErrorNode = true;
                    JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                    {
                        ObjectName = itemName,
                        FullPath = itemUrl,
                        Status = JobDetailsStatus.Failed,
                        Comment = GetExceptionMessage(e),
                    });
                    this.AddFailureItem2Cache(aveItem, parentId, termId, e);
                }
            }
            return;
        }

        private bool NeedCheckExistingRecord(RMOneDriveSetting setting)
        {
            bool needCheck = false;
            if (setting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm)
            {
                if (!setting.NeedCheckDefaultValue)
                {
                    needCheck = true;
                }
                else
                {
                    if (setting.ApplyExistType != (int)ApplyExistingTermType.OverWrite)
                    {
                        needCheck = true;
                    }
                }
            }
            else if (setting.DeployTermMethod == (int)DeployTermMethod.UseAutoClassification)
            {
                if (setting.AutoJobOption != (int)AutoJobOption.Override)
                {
                    needCheck = true;
                }
            }
            else if (setting.DeployTermMethod == (int)DeployTermMethod.UseIntelligenceClassification && setting.AITermUseType == ArtificialIntelligenceTermUseType.ApplyTerm)
            {
                if (setting.AutoJobOption != (int)AutoJobOption.Override)
                {
                    needCheck = true;
                }
            }
            return needCheck;
        }

        private void ProcessFailedItems(IAveList list, SyncItemRuleInfo parentItemRule)
        {
            try 
            {
                logger.Info("Start to process failed items in azure table");
                List<SyncFailureItemEntity> failedItems = SyncFailureItemDao.GetAllByDataSource(TenantLocalValue.LogonGroupId, DiscoverSite.SiteID.ToString(), list.ID.ToString(), (int)FailureSourceType.OneDriveDataSync);
                int incItemsPerTask = failedItems.Count / 4;
                logger.Info($"Process last failed item count:[{failedItems.Count}].incItemsPerTask:[{incItemsPerTask}]");
                if (failedItems.Count > 0)
                {
                    JobContext.ReportManager.IncreaseBase(failedItems.Count);
                    List<Guid> nodeIdList = failedItems.Select(n => new Guid(n.RowKey)).ToList();
                    List<RMManualApprove> mas = RMManualApproveDao.GetManualApproveByNodes(DiscoverSite.SiteID, nodeIdList);
                    logger.Info("Manual approve count {0}", mas.Count);
                    Dictionary<Guid, string> itemAndOwnerMapping = AssembleItemAndOwnerMappingNew(mas, nodeIdList);
                    UniqueIdUtil idUtil = null;
                    using (var performance = new PerformanceScope("RMSPExplorerProcessor.GenerateUniqueIds", addToStatistics: true))
                    {
                        idUtil = new UniqueIdUtil(TenantLocalValue.LogonGroupId, failedItems.Count);
                    }

                    var itemIds = failedItems.Select(i => i.ItemId).ToList();
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
                                ProcessFailedItemV1(list, changedItem, failedItems, parentItemRule, itemAndOwnerMapping, idUtil, cts);
                            });
                        }
                        else
                        {
                            foreach (var changedItem in items)
                            {
                                ProcessFailedItemV1(list, changedItem, failedItems, parentItemRule, itemAndOwnerMapping, idUtil);
                            }
                        }
                    }
                }
            }
            catch (JobStopException)
            {
                throw new JobStopException("the job has stopped.");
            }
            
        }

        public virtual void ProcessFailedItemV1(IAveList list, IAveListItem aveItem, List<SyncFailureItemEntity> failedItems, SyncItemRuleInfo parentItemRule, Dictionary<Guid, string> itemAndOwnerMapping, UniqueIdUtil idUtil, CancellationTokenSource cts = null)
        {
            string itemName = string.Empty;
            var failedItem = failedItems.Where(f => f.ItemId == aveItem.ID).FirstOrDefault();
            string itemUrl = failedItem?.FullPath; // string.Empty;
            try
            {
                AvePoint.GCommon.Utility.ArgumentCheck.NotNull(failedItem, nameof(failedItem));
                Guid parentId = new Guid(failedItem.ParentId);
                using (var performance = new PerformanceScope("RMSPExplorerProcessor.ProcessFailedItem", addToStatistics: true))
                {
                    JobContext.ReportManager.Increase();
                    if (!_isSyncStubFile && aveItem.IsStubItem())
                    {
                        logger.Debug($"Current item [{aveItem?.UniqueId}] is stub file, so skipped.");
                        return;
                    }
                    int itemId = failedItem.ItemId;
                    logger.Info($"Process failed item:Id:{itemId}, full path:{failedItem.FullPath}.");
                    itemName = aveItem?.GetObjectName();
                    itemUrl = aveItem.FullPath();
                    using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        Guid termId = new Guid(failedItem.TermId);
                        var termInfo = new RMTermInfo() { UniqueId = termId };
                        if (RMSPExplorerDataCache.Instance.Terms.ContainsKey(termId))
                        {
                            termInfo.Name = RMSPExplorerDataCache.Instance.Terms[termId].Name;
                        }
                        RMRuleItemCollection rules = null;
                        SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
                        if (RMSPExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                        {
                            try
                            {
                                var newRuleCollection = RebuldSPRules(rules);
                                if (newRuleCollection.Rules.Count == 0)
                                {
                                    logger.Info($"No SP rules realted to the item {list.RootFolder.Url}:{itemId}");
                                }
                                else
                                {
                                    var filterEnginer = new RMOneDriveRuleChecker(newRuleCollection);
                                    itemRuleInfo = filterEnginer.CheckDisposalRule(aveItem, parentItemRule);
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Warn(e.Message, e);
                            }
                        }
                        itemRuleInfo.TermInfo = termInfo;
                        string owner = null;
                        if (itemAndOwnerMapping.ContainsKey(aveItem.UniqueId))
                        {
                            owner = itemAndOwnerMapping[aveItem.UniqueId];
                        }
                        logger.Debug("Owner for record {0} is {1}", aveItem.UniqueId, owner);
                        var item = syncItem.AssembleRecord(aveItem, parentId, itemRuleInfo, owner);
                        Record recordInDB = null;

                        WaitCosmosExecuteAction(() =>
                        {
                            recordInDB = ExplorerDao.ReadById(item.ScopeId, item.Id);
                        });
                        //check uniqueId
                        UpdateRecordId(item, recordInDB, idUtil);

                        bool labelNotExist = UpdateLabel(aveItem, termInfo.UniqueId, item.Id, recordInDB);
                        //item.Comment = "RM_JM_SyncFailedItemSuccess";
                        SyncItemToDB(item, recordInDB, labelNotExist, recordInDB != null);
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
                    JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                    {
                        ObjectName = itemName,
                        FullPath = itemUrl,
                        Status = JobDetailsStatus.Failed,
                        Comment = GetExceptionMessage(e),
                    });
                }
                else
                {
                    this.RemoveFailureItemFromAzure(failedItem);
                }

            }
            return;
        }

        private void AddFailureItem2Cache(IAveListItem aveItem, Guid parentId, Guid termId, Exception e)
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
                    ParentId = parentId.ToString(),
                    WebId = aveItem.ParentList.ParentWeb.ID.ToString(),
                    TermId = termId.ToString()
                };
                failureItem.URL = aveItem?.Url;
                failureItem.ObjectName = aveItem?.Name;
                failureItem.Message = this.GetExceptionMessage(e);
                this.FailureItems.Add(failureItem);
            }
        }

        private void AddFailureItem2Cache(Record record, Exception e)
        {
            if (this.FailureItems.Count <= 1000)
            {
                RMSPSyncFailureItem failureItem = new RMSPSyncFailureItem()
                {
                    SiteId = record.ScopeId.ToString(),
                    ListId = record.ListId.ToString(),
                    IntemIntId = record.ItemRowId,
                    JobId = JobContext.SubJobId,
                    ItemId = record.ItemId.ToString(),
                    ParentId = record.FolderId.ToString(),
                    WebId = record.WebId.ToString(),
                    URL = record.FullPath,
                    ObjectName = record.LeafName,
                    Message = e?.Message,
                    TermId = record.TermId.ToString()
                };
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
                        entity.DataSource = (int)FailureSourceType.OneDriveDataSync;
                        failureEntities.Add(entity);
                    }
                    logger.Debug($"Add entity to azure, list count: {failureEntities.Count}");
                    SyncFailureItemDao.Add(TenantLocalValue.LogonGroupId, failureEntities);
                }
            }
            catch (Exception e)
            {
                JobContext.HasErrorNode = true;
                _siteCache.HasErrorNode = true;
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

        public void SyncItemToDB(Record newItem, Record recordInDB = null, bool labelNotExist = false, bool getDBRecord = true)
        {
            using (var performance = new PerformanceScope("RMSPExplorerProcessor.SyncItemToDB", addToStatistics: true))
            {
                bool result = false;
                newItem.ContainerId = containerId;
                newItem.labelNotExist = labelNotExist;
                if (recordInDB != null)
                {
                    newItem.ParentId = recordInDB.ParentId;
                }
                if (recordInDB.CheckExistAndTagDuplicateManual())
                {
                    newItem.KeepOldManualColumn(recordInDB);
                }
                if (_isCosmosBulkOperationEnabled)
                {
                    Add2BulkOperationQueue(newItem, recordInDB, getDBRecord);
                    return;
                }
                RMRule tempRule = null;
                if (newItem.RuleId != Guid.Empty && mRuleCache != null && mRuleCache.ContainsKey(newItem.RuleId))
                {
                    tempRule = mRuleCache[newItem.RuleId];
                }
                WaitCosmosExecuteAction(() =>
                {
                    result = ExplorerDao.AddOrUpdateRecord(newItem, _forceUpdate, tempRule);
                });
                if (result)
                {
                    logger.Info($"add record to db success {newItem.ListId}:{newItem.ItemRowId}:{newItem.ItemId}:{newItem.ContainerId}");
                    //only report item
                    if (newItem.NodeType == (int)NodeLevel.Item || newItem.NodeType == (int)NodeLevel.Folder)
                    {
                        if (labelNotExist)
                        {
                            JobContext.HasErrorNode = true;
                            JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                            {
                                ObjectName = newItem.LeafName,
                                FullPath = newItem.FullPath,
                                Status = JobDetailsStatus.Failed,
                                Comment = "RM_OneDrive_DataSync_LabelNotExist"
                            });
                        }
                        else
                        {
                            JobContext.HasSuccessNode = true;
                            JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                            {
                                ObjectName = newItem.LeafName,
                                FullPath = newItem.FullPath,
                                Status = JobDetailsStatus.Successful,
                                Comment = newItem.Comment
                            });
                        }
                    }
                }
                else
                {
                    logger.Warn($"skip to add record to db, the item already exist:{newItem?.Id}");
                }
            }
        }

        private RMTermInfo GetTermInfo(IAvePropertyBag properties)
        {
            var termInfo = new RMTermInfo();

            //if (properties.ContainsKey(RcordsBuiltInColumn.CONTAINER_BCS_NAME))
            //{
            //    var termId = properties[RcordsBuiltInColumn.CONTAINER_BCS_NAME];
            //    if (termId != null)
            //    {
            //        termInfo.UniqueId = new Guid(termId.ToString());
            //        termInfo.Name = RMOneDriveExplorerDataCache.Instance.Terms.ContainsKey(termInfo.UniqueId) ? RMOneDriveExplorerDataCache.Instance.Terms[termInfo.UniqueId].Name : string.Empty;
            //    }
            //}
            return termInfo;
        }

        private RMTermInfo GetTermInfo(Hashtable properties)
        {
            var termInfo = new RMTermInfo();

            //if (properties.ContainsKey(RcordsBuiltInColumn.CONTAINER_BCS_NAME))
            //{
            //    var termId = properties[RcordsBuiltInColumn.CONTAINER_BCS_NAME];
            //    if (termId != null)
            //    {
            //        termInfo.UniqueId = new Guid(termId.ToString());
            //        termInfo.Name = RMOneDriveExplorerDataCache.Instance.Terms.ContainsKey(termInfo.UniqueId) ? RMOneDriveExplorerDataCache.Instance.Terms[termInfo.UniqueId].Name : string.Empty;
            //    }
            //}
            return termInfo;
        }

        private void Add2BulkOperationQueue(Record newItem, Record dbRecord = null, bool getDBRecord = true)
        {
            if (dbRecord != null)
            {
                RMRule tempRule = null;
                if (newItem.RuleId != Guid.Empty && mRuleCache != null && mRuleCache.ContainsKey(newItem.RuleId))
                {
                    tempRule = mRuleCache[newItem.RuleId];
                }
                if (ExplorerDao.NeedUpdateRecord(newItem, _forceUpdate, dbRecord, tempRule))
                {
                    CosmosBulkOperator.Instance.Add(newItem);
                }
            }
            else
            {
                if (getDBRecord)
                {
                    RMRule tempRule = null;
                    if (newItem.RuleId != Guid.Empty && mRuleCache != null && mRuleCache.ContainsKey(newItem.RuleId))
                    {
                        tempRule = mRuleCache[newItem.RuleId];
                    }
                    if (ExplorerDao.NeedUpdateRecord(newItem, _forceUpdate, tempRule))
                    {
                        CosmosBulkOperator.Instance.Add(newItem);
                    }
                }
                else
                {
                    CosmosBulkOperator.Instance.Add(newItem);
                }
            }
        }

        private async System.Threading.Tasks.Task ProcessSucceedRecord(Record newItem)
        {
            logger.Info($"add record to db success {newItem.ListId}:{newItem.ItemRowId}:{newItem.ItemId}:{newItem.ContainerId}");
            //only report item
            if (newItem.NodeType == (int)NodeLevel.Item || newItem.NodeType == (int)NodeLevel.Folder)
            {
                if (newItem.labelNotExist)
                {
                    JobContext.HasErrorNode = true;
                    JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                    {
                        ObjectName = newItem.LeafName,
                        FullPath = newItem.FullPath,
                        Status = JobDetailsStatus.Failed,
                        Comment = "RM_OneDrive_DataSync_LabelNotExist"
                    });
                }
                else
                {
                    JobContext.HasSuccessNode = true;
                    JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                    {
                        ObjectName = newItem.LeafName,
                        FullPath = newItem.FullPath,
                        Status = JobDetailsStatus.Successful,
                        Comment = newItem.Comment
                    });
                }
            }
            ProcessManualDuplicateData(newItem);
        }
        private void ProcessManualDuplicateData(Record newItem)
        {
            //先运行manual review job scan进来的数据createdate为0, 需要remove.
            if (newItem.hasDuplicate)
            {
                try
                {
                    logger.Info($"remove old manual data:{newItem.Id}");
                    ExplorerDao.Delete(0, newItem.Id);
                }
                catch (Exception ex)
                {
                    logger.Error($"error occurred while remove old manual data, ERROR: {ex.ToString()}");
                }

            }
        }
        private void ProcessFailedRecord(Record record, Exception ex)
        {
            logger.Warn($"Failed to add record to db, the item id:{record?.Id}");
            AvePoint.GCommon.Utility.ArgumentCheck.NotNull(record, nameof(record));
            JobContext.HasErrorNode = true;
            JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
            {
                ObjectName = record.LeafName,
                FullPath = record.FullPath,
                Status = JobDetailsStatus.Failed,
                Comment = ex?.Message
            });

            if (record.NodeType == (int)NodeLevel.Item)
            {
                AddFailureItem2Cache(record, ex);
            }
            else
            {
                RMSPExplorerDataCache.Instance.SiteLevelCache[record.ScopeId.ToString()].HasErrorNode = true;
            }
        }

        private RMTermInfo GetTermInfo(IAveListItem item, IAveFieldCollection fields)
        {
            var termInfo = new RMTermInfo();

            //if (fields.ContainsField(_siteCache.BCSColumnInternalName))
            //{
            //    var termObj = item[_siteCache.BCSColumnInternalName];
            //    if (termObj != null && !string.IsNullOrEmpty(termObj.ToString()))
            //    {
            //        var valueString = termObj.ToString().Split('|');
            //        if (valueString.Length > 1)
            //        {
            //            termInfo.UniqueId = new Guid(valueString[1]);
            //            termInfo.Name = RMOneDriveExplorerDataCache.Instance.Terms.ContainsKey(termInfo.UniqueId) ? RMOneDriveExplorerDataCache.Instance.Terms[termInfo.UniqueId].Name : string.Empty;
            //        }
            //        else
            //        {
            //            logger.Info($"{item.Url} invalid term format:{valueString}");
            //        }

            //    }
            //}
            return termInfo;
        }

      



        /// <summary>
        /// for this now ,incremental logic not support container level rule change.... to do next...
        /// </summary>
        /// <param name="site"></param>
        public void ProcessTermChangedItems(long lastScanTime)
        {
            IAveSite site = DiscoverSite.Site;
            bool startSuccess = false;
            try
            {
                List<Record> allRecords = new List<Record>();
                var ChangeTermIds = GetChangedTermIds(lastScanTime);
                if (ChangeTermIds.Count > 0)
                {
                    using (var performance0 = new PerformanceScope("RMOneDriveExplorerBase.GetRecordsByTerms", addToStatistics: true))
                    {
                        logger.Info($"Total changed term count: {ChangeTermIds.Count}");
                        for (int i = 0; i < ChangeTermIds.Count; i += 1000)
                        {
                            var tempIds = ChangeTermIds.Skip(i).Take(1000).ToList();
                            logger.Info($"Query changed term from {i} to {i + 1000}");
                            var records = ExplorerDao.GetRecordsByTerms(site.ID, tempIds, JobContext.JobStartTime.Ticks);
                            if (records != null && records.Count > 0)
                            {
                                allRecords.AddRange(records);
                            }
                        }
                    }
                }

                if (allRecords == null || allRecords.Count == 0)
                {
                    logger.Info("No Incremental Classification change records in site  {0}", site.Url);
                    return;
                }
                if (_isCosmosBulkOperationEnabled)
                {
                    var RMKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
                    var bulkSize = RMKeyValueDao.GetCosmosBulkInsertOperationBufferSize();
                    if (bulkSize == default(int)) bulkSize = CosmosBulkOperator.DefualtBufferSize;
                    logger.Info($"Cosmos bulk operation enabled, bulk size: {bulkSize}");
                    CosmosBulkOperator.Instance.Start(bulkSize, ProcessSucceedRecord, ProcessFailedRecord);
                    startSuccess = true;
                }
                Dictionary<Guid, List<Record>> webObjs = allRecords.GroupBy(r => r.WebId).ToDictionary(g => g.Key, p => p.ToList());
                IAveWeb web = null;
                IAveList list = null;
                JobContext.ReportManager.IncreaseBase(webObjs.Count);
                foreach (var webId in webObjs.Keys)
                {
                    try
                    {
                        if (web == null || !web.ID.Equals(webId))
                        {
                            web = site.OpenWeb(webId);
                            logger.Info("Process classification change web {0}", web.Url);
                        }
                        var listNodes = webObjs[webId].GroupBy(t => t.ListId).ToDictionary(g => g.Key, p => p.ToList());

                        foreach (var listId in listNodes.Keys)
                        {
                            try
                            {
                                if (list == null || !list.ID.Equals(listId))
                                {
                                    list = web.GetList(listId);
                                    logger.Info("Process classification change list {0}", list.RootFolder.Url);
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
                            }
                            catch (JobStopException)
                            {
                                throw new JobStopException("This Job is stopped.");
                            }
                            catch (Exception le)
                            {
                                logger.Warn("Process classification list error {0}:{1}", listId, le.ToString());

                            }
                        }
                    }
                    catch (JobStopException)
                    {
                        throw new JobStopException("This Job is stopped.");
                    }
                    catch (Exception we)
                    {
                        logger.Warn("process classification web error {0}:{1}", webId, we.ToString());
                    }
                    finally
                    {
                        JobContext.ReportManager.Increase();
                    }
                }
            }
            catch (JobStopException)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                logger.Error($"error occurred while Process changed site:{site.Url}, ERROR:{e.ToString()}");
                JobContext.HasErrorNode = true;
                _siteCache.HasErrorNode = true;
                JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                {
                    ObjectName = site.RootWeb.Title,
                    FullPath = site.Url,
                    Status = JobDetailsStatus.Failed,
                    Comment = GetExceptionMessage(e),
                });
            }
            finally
            {
                if (_isCosmosBulkOperationEnabled && startSuccess)
                {
                    CosmosBulkOperator.Instance.Complete();
                }
            }
        }

        private void RealProcessTermChangeItem(IAveListItem item, List<Record> itemNodes, CancellationTokenSource cts = null)
        {
            string itemName = string.Empty, itemUrl = string.Empty;
            var itemNode = itemNodes.Where(i => i.NodeId == item.UniqueId).FirstOrDefault();
            if (itemNode == null)
            {
                return;
            }
            using (var performance = new PerformanceScope("RMOneDriveExplorerBase.ProcessTermChangedItem", addToStatistics: true))
            {
                #region process item
                //IAveListItem item = null;                   
                Guid termId = Guid.Empty;
                try
                {
                    //using (var performance0 = new PerformanceScope("RMOneDriveExplorerBase.GetItemById", addToStatistics: true))
                    //{
                    //    WaitSPOExecuteAction(() =>
                    //    {
                    //        item = list.GetItemById((int)itemNode.ItemRowId);
                    //    });
                    //}
                    itemName = item?.GetObjectName();
                    if (NeedSkipFile(item, itemName))
                    {
                        return;
                    }

                    itemUrl = item.FullPath();
                    using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        logger.Info($"Process classification change item {itemUrl}");
                        RMRuleItemCollection rules = null;
                        var termInfo = new RMTermInfo()
                        {
                            Name = itemNode.TermName,
                            UniqueId = itemNode.TermId
                        };
                        termId = termInfo.UniqueId;
                        SyncItemRuleInfo ruleInfo = new SyncItemRuleInfo();
                        using (var performance0 = new PerformanceScope("RMOneDriveExplorerBase.CheckRule", addToStatistics: true))
                        {
                            if (RMOneDriveExplorerDataCache.Instance.TermRuleMapping.TryGetValue(itemNode.TermId, out rules))
                            {
                                var newRuleCollection = RebuldSPRules(rules);
                                if (newRuleCollection.Rules.Count == 0)
                                {
                                    logger.Info($"No SP rules realted to the item {itemNode?.DirPath}");
                                }
                                else
                                {
                                    var filterEnginer = new RMOneDriveRuleChecker(newRuleCollection);
                                    if (item.FileSystemObjectType == AveFileSystemObjectType.Folder)
                                    {
                                        ruleInfo = filterEnginer.CheckDisposalRule(item.Folder);
                                    }
                                    else
                                    {
                                        ruleInfo = filterEnginer.CheckDisposalRule(item);
                                    }
                                }

                            }
                        }
                        using (var performance0 = new PerformanceScope("RMOneDriveExplorerBase.UpdateItem", addToStatistics: true))
                        {
                            if (ruleInfo.Rule != null && (itemNode.RuleLevel == 0 || itemNode.RuleLevel >= 32))
                            {
                                logger.Info($"swith item rule: {itemUrl}, {itemNode.RuleId} 2 {ruleInfo.Rule?.Id}.");
                                #region change item rule
                                var ruleId = new Guid(ruleInfo.Rule.Id);
                                if (!ruleInfo.Rule.IsManualApproval)
                                {
                                    itemNode.RecordOwner = string.Empty;
                                }
                                itemNode.RuleId = ruleId;
                                itemNode.RuleLevel = (int)ruleInfo.Rule.PolicyLevel;

                                itemNode.PreviosDisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(ruleInfo.DisposalAction);
                                itemNode.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(ruleInfo.DisposalAction);
                                UpdateDueDate(itemNode, ruleInfo);
                                if (_isCosmosBulkOperationEnabled)
                                {
                                    itemNode.FullPath = itemUrl;
                                    CosmosBulkOperator.Instance.Add(itemNode);
                                }
                                else
                                {
                                    ExplorerDao.Upsert(itemNode);
                                    JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails() { ObjectName = itemName, FullPath = itemUrl, Status = JobDetailsStatus.Successful });
                                }
                                #endregion
                            }
                            else if (itemNode.RuleLevel == (int)GCommon.Contract.CommonFilter.PolicyLevel.Document || itemNode.RuleLevel == (int)GCommon.Contract.CommonFilter.PolicyLevel.Item || itemNode.RuleLevel == (int)GCommon.Contract.CommonFilter.PolicyLevel.List)
                            {
                                logger.Info("Empty the item rule {0}", item.Url);
                                #region empty item rule
                                itemNode.RuleId = Guid.Empty;
                                itemNode.RuleLevel = 0;
                                itemNode.RecordOwner = string.Empty;
                                itemNode.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(string.Empty);
                                itemNode.PreviosDisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(string.Empty);
                                if (_isCosmosBulkOperationEnabled)
                                {
                                    itemNode.FullPath = itemUrl;
                                    CosmosBulkOperator.Instance.Add(itemNode);
                                }
                                else
                                {
                                    ExplorerDao.Upsert(itemNode);
                                    JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails() { ObjectName = itemName, FullPath = itemUrl, Status = JobDetailsStatus.Successful });
                                }

                                #endregion
                            }
                            else
                            {
                                logger.Info("No change item {0}", item.Url);
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
                    logger.Error("Process classification item error. Item id: {0}, Item Path: {1}, ERROR:{2}", itemNode?.Id, itemNode?.DirPath, e.ToString());
                    bool isItemNotFound = this.isItemNotFoundError(e);
                    if (!isItemNotFound)
                    {
                        //JobContext.HasErrorNode = true;
                        // _siteCache.HasErrorNode = true;
                        JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                        {
                            ObjectName = itemName,
                            FullPath = itemUrl,
                            Status = JobDetailsStatus.Failed,
                            Comment = GetExceptionMessage(e),
                        });
                        this.AddFailureItem2Cache(item, itemNode?.ParentId ?? Guid.Empty, termId, e);
                    }
                }
                #endregion
            }

        }


        private bool isItemNotFoundError(Exception e)
        {
            if (e.InnerException != null && e.InnerException is Microsoft.SharePoint.Client.ServerException)
            {
                var ex = e.InnerException as Microsoft.SharePoint.Client.ServerException;
                if (ex.Message.Contains("Item does not exist"))
                {
                    return true;
                }
            }
            return false;
        }

        private void UpdateDueDate(Record itemNode, SyncItemRuleInfo ruleInfo)
        {
            //Hold状态Record重新计算Due Date;
            if (itemNode.HoldStatus && RuleHelper.IsRemoveRule(ruleInfo.Rule, itemNode.SourceFlag))
            {
                long newDisposalDueDate = 0;
                if (itemNode.DisposalDueDate == DueDateUtil.NextJob)
                {
                    newDisposalDueDate = itemNode.HoldReleaseTime;
                }
                if (itemNode.DisposalDueDate > 0)
                {
                    if (itemNode.DisposalDueDate > itemNode.HoldReleaseTime)
                    {
                        newDisposalDueDate = itemNode.DisposalDueDate;
                    }
                    else
                    {
                        newDisposalDueDate = itemNode.HoldReleaseTime;
                    }
                }
                itemNode.DisposalDueDate = newDisposalDueDate;
            }
        }

        private List<Guid> GetChangedTermIds(long ticks)
        {
            List<Guid> allTerms = new List<Guid>();
            try
            {
                List<Guid> subTerms = new List<Guid>();
                allTerms = RMChangeClassificationDao.GetAllChange(ticks, (int)Contract.Object.TermChangeType.TermRule);
                foreach (var id in allTerms)
                {
                    subTerms.AddRange(TermDao.GetAllSubTermUniqueIds(id));
                }
                allTerms.AddRange(subTerms);
                return allTerms;
            }
            catch (Exception e)
            {
                logger.Error("get change terms error {0}", e.ToString());
            }
            return allTerms;
        }

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

        private void UpdateRecordId(Record recoEntity, Record recordInDB, UniqueIdUtil idUtil)
        {
            string recordsGlobalId = recoEntity.RecordsId;
            using (new RA.Common.PerformanceScope("RMOneDriveExplorerProcessor.UpdateRecordId", string.Format("RMOneDriveExplorerProcessor.SetUniqueId:List {0} RowId{1}", recoEntity.ListId, recoEntity.ItemRowId), true))
            {
                if (string.IsNullOrEmpty(recordsGlobalId))
                {
                    recordsGlobalId = recordInDB?.RecordsId;

                    if (string.IsNullOrEmpty(recordsGlobalId))
                    {
                        logger.Info("create new unique List {0}Id:{1}", recoEntity.ListId, recoEntity?.Id);
                        recordsGlobalId = idUtil.GenerateUniqueId();
                        //UniqueIdGenerator.GenerateUniqueId(RMOneDriveExplorerDataCache.Instance.UniqueIdSetting);

                    }
                }
                recoEntity.RecordsId = recordsGlobalId;
            }
        }

        private bool NeedSkipList(AveDiscoverList discoverList)
        {
            bool result = false;
            if (discoverList.Hidden.HasValue && discoverList.Hidden.Value)
            {
                logger.Info("Skip the hidden list {0}", string.IsNullOrEmpty(discoverList?.RootFolderUrl) ? discoverList?.Name : discoverList?.RootFolderUrl);
                result = true;
            }
            if (discoverList.Name.Equals("{System Folder}"))
            {
                logger.Info("Skip the system list {0}", string.IsNullOrEmpty(discoverList?.RootFolderUrl) ? discoverList?.Name : discoverList?.RootFolderUrl);
                result = true;
            }
            else if (CheckIsDesignList(discoverList))
            {
                logger.Info("Skip the design list {0}", string.IsNullOrEmpty(discoverList?.RootFolderUrl) ? discoverList?.Name : discoverList?.RootFolderUrl);
                result = true;
            }
            else if (discoverList.GetListObject().BaseType != AveBaseType.DocumentLibrary)
            {
                logger.Info("Skip this list {0}, not a document library.", string.IsNullOrEmpty(discoverList?.RootFolderUrl) ? discoverList?.Name : discoverList?.RootFolderUrl);
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
            if (RMOneDriveExplorerDataCache.Instance.ArchiverSettings.SkipFileExtensions.Contains(ext))
            {
                logger.Info("need skip file check rule action {0}:{1}", objectName, ext);
                return true;
            }
            if (objectName.EndsWith("aspx") && !RMOneDriveExplorerDataCache.Instance.ArchiverSettings.IsDeleteLinkFile)
            {
                logger.Info("need skip file check rule action maybe stub file. {0}:{1}", objectName, ext);
                return true;
            }
            return false;
        }

      

        private void RemoveSPObj(Guid objectId, int itemRowId = 0)
        {
            using (var performance1 = new PerformanceScope("RMSPExplorerProcessor.RemoveSPObj", addToStatistics: true))
            {
                var siteId = DiscoverSite.SiteID;
                var recId = IDGenerator.GetRecordId(siteId, objectId);
                Record removeRecordInDB = null;
                WaitCosmosExecuteAction(() =>
                {
                    removeRecordInDB = ExplorerDao.ReadById(siteId, recId);
                });
                if (removeRecordInDB != null)
                {
                    List<Guid> subFolderIds = new List<Guid>();
                    List<Record> tempRecords = GetAssociatedRecords(removeRecordInDB, ref subFolderIds);
                    logger.Debug($"get {removeRecordInDB.DirPath} removed items related count:{tempRecords.Count}");
                    if (removeRecordInDB.RecordStatus == (int)RMRecordStatus.Active)
                    {
                        ExplorerDao.UpdateRecordState(removeRecordInDB, (int)RMRecordStatus.RMDeleted, subFolderIds);
                        logger.Info("update record state to 3,siteId: {0}, objId: {1}, itemId: {2}", siteId, objectId, itemRowId);

                    }
                    else
                    {
                        logger.Warn("sp object already archived,siteId:{0}, objId:{1}, itemId:{2}", siteId, objectId, itemRowId);
                    }
                }
            }
        }

        private List<Record> GetAssociatedRecords(Record rec, ref List<Guid> folderIds)
        {
            List<Record> results = new List<Record>();
            if (rec != null)
            {
                Expression<Func<Record, bool>> lambda = null;
                switch (rec.NodeType)
                {
                    case (int)NodeLevel.SiteCollection:
                        lambda = s => s.ScopeId == rec.ScopeId;
                        break;
                    case (int)NodeLevel.Site:
                        lambda = s => s.ScopeId == rec.ScopeId && s.WebId == rec.WebId && s.NodeType == (int)NodeLevel.Item;
                        break;
                    case (int)NodeLevel.List:
                        lambda = s => s.ScopeId == rec.ScopeId && s.WebId == rec.WebId && s.ListId == rec.ListId && s.NodeType == (int)NodeLevel.Item;
                        break;
                    case (int)NodeLevel.Folder:
                        //Get all folder id list under current folder...

                        var tempFolderIds = ExplorerDao.GetAllSubFolderUnderFolder(rec);
                        logger.Debug($"get removed folder count:{tempFolderIds.Count}");
                        folderIds = tempFolderIds;
                        lambda = s => s.ScopeId == rec.ScopeId && s.WebId == rec.WebId && s.ListId == rec.ListId && s.NodeType == (int)NodeLevel.Item && tempFolderIds.Contains(s.FolderId);
                        break;
                    case (int)NodeLevel.Item:
                        results.Add(rec);
                        return results;
                    default:
                        logger.Warn($"node type not supported:{rec.NodeType}, {rec.DirPath}");
                        break;
                }
                if (lambda != null)
                {
                    results = ExplorerDao.GetFilterList(a => new Record { Id = a.Id, TermId = a.TermId, ScopeId = a.ScopeId, RecordStatus = a.RecordStatus, DestroyedTime = a.DestroyedTime }, lambda).ToList();
                }

            }
            return results;
        }

        private RuleCollection RebuldSPRules(RMRuleItemCollection rules)
        {
            RuleCollection newRuleCol = new RuleCollection();
            Dictionary<int, Rule> newRules = new Dictionary<int, Rule>();
            int reOrder = 0;
            foreach (var order in rules.CommonRules.Rules.Keys)
            {
                if (rules.CommonRules.Rules[order].PolicyLevel != PolicyLevel.None && rules.CommonRules.Rules[order].OneDriveRule != null && rules.CommonRules.Rules[order].OneDriveRule.SOFilters != null && rules.CommonRules.Rules[order].OneDriveRule.SOFilters.Count > 0)
                {
                    reOrder++;

                    var commonRule = rules.CommonRules.Rules[order];
                    var rule = commonRule.OneDriveRule;
                    rule.Id = commonRule.Id;
                    //var DAUtil = new DAUtil();
                    //DAUtil.AddMoveToFilter(rule);
                    //var newRule = ruleAssembler.ConvertToSPRule(rule);
                    newRules.Add(order, rule);
                }
            }

            newRuleCol.Rules = newRules;
            return newRuleCol;
        }


        private RuleCollection GetRuleCollection(List<Rule> rules)
        {
            RuleCollection newRuleCol = new RuleCollection();
            Dictionary<int, Rule> newRules = new Dictionary<int, Rule>();
            int reOrder = 0;
            foreach (var rule in rules)
            {
                if (rule.OneDriveRule.PolicyLevel != PolicyLevel.None && rule.OneDriveRule.SOFilters != null && rule.OneDriveRule.SOFilters.Count > 0)
                {
                    reOrder++;

                    //var DAUtil = new DAUtil();
                    //DAUtil.AddMoveToFilter(rule);
                    //var newRule = ruleAssembler.ConvertToSPRule(rule);
                    newRules.Add(reOrder, rule.OneDriveRule);
                }
            }
            newRuleCol.Rules = newRules;
            return newRuleCol;
        }

        private void AddSiteScope(Record item)
        {
            try
            {
                RMScope site = new RMScope()
                {
                    FullPath = item.DirPath,
                    ScopeId = item.ScopeId,
                    IsRemoved = false,
                    ScopeName = item.LeafName,
                };
                RMScopeDao.AddOrUpateSiteScope(site);
            }
            catch(Exception e)
            {
                logger.Error($"Error occurred while adding site scope. Error:{e.ToString()}");
            }
        }
    }


}
