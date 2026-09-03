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
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.ExplorerSync.Cache;
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
using AvePoint.RA.SharePoint.ExplorerSync.Utils;
using AvePoint.RA.SharePoint.ExplorerSync.Modes;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.Common.Throttle;
using AvePoint.RA.RACommonUtility.UniqueId;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.SharePoint.ExplorerSync.Report;
using System.Linq.Expressions;
using AvePoint.RA.Contract.TaxonomyModel;
using System.Collections;
using AvePoint.RA.Contract.RMReport;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using Newtonsoft.Json;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.DB.Explorer.Bulk;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using AvePoint.RA.SharePoint.Common.CAMLHelper.General;
using AvePoint.GCommon.Utility.TransientFault;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.SharePoint.ExplorerSyncNew.Modes;
using AvePoint.RA.DB.AzureCosmosDB;
using AvePoint.RA.DB.AzureCosmosDB.Model;
using AvePoint.RA.DB.AzureCosmosDB.Concurrent;
using AvePoint.RA.SharePoint.ExplorerSyncNew;

namespace AvePoint.RA.SharePoint.ExplorerSync
{
    public class RMSPExplorerBase : RMSPDiscoverBase
    {
        private static readonly AveRetryPolicy RetryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(3, TimeSpan.FromSeconds(1)));
        private static readonly RALogger logger = RALogger.GetInstance(typeof(RMSPExplorerBase));
        private ISPDiscover mDiscover = null;
        protected RMSPExplorerSiteLevelCache _siteCache = null;
        protected RMSPSyncItem syncItem = null;
        protected static CallLimiter _spoCallLimiter;
        protected static CallLimiter _cosmosCallLimiter;
        protected static int _itemsPerTask = 400;  //Threshold降到2000， 这里降到400
        private long mLastJobTicks = DateTime.MinValue.Ticks;
        private long mMainJobTicks = DateTime.MinValue.Ticks;
        private SPDiscoverType mDiscoverType = SPDiscoverType.Full;
        protected virtual NodeFlagType nodeFlagType => NodeFlagType.ExplorerSyncLib;
        protected int itemsPerTask
        {
            get
            {
                return _itemsPerTask;
            }
        }

        private bool _isCosmosBulkOperationEnabled = false; //是否开启了批量插入数据到cosmos db
        private bool _forceUpdate = false;
        private bool _isSyncStubFile = false;

        private List<Guid> unSuccessList = new List<Guid>();
        private readonly object unSuccessListLock = new object();
        protected string containerId = string.Empty;
        protected string currentSiteId = string.Empty;
        protected string currentTeamsId = string.Empty;
        protected List<RMSPSyncFailureItem> FailureItems = new List<RMSPSyncFailureItem>();
        protected Dictionary<Guid, RMRule> mRuleCache = new Dictionary<Guid, RMRule>();
        protected Dictionary<Guid, Guid> mParentTermCache = new Dictionary<Guid, Guid>();
        protected Dictionary<Guid, string> mScopeInfoCache = new Dictionary<Guid, string>();

        protected List<RMCustomIndexMetadata> CustomIndexMetadatas = new List<RMCustomIndexMetadata>();
        protected List<RMCustomMetadataColumn> CustomMetadataColumns = new List<RMCustomMetadataColumn>();
        protected RMLifecycleSetting GroupSetting = null;
        protected RMLifecycleSetting SiteSetting = null;
        protected RMLifecycleSetting TeamsSetting = null;

        protected List<RMLifecycleSetting> _currentSiteSettings = new();
        protected SourceFlag SourceFlag { get; set; } = SourceFlag.Teams;

        protected bool IsEnableInheritParentTerm { get; set; } = false;

        protected static readonly HashSet<int> s_optionalStatus = new()
        {
            (int)RMRecordStatus.Active,
            (int)RMRecordStatus.ManualPreSync,
            (int)RMRecordStatus.Retention,
            (int)RMRecordStatus.TrainingManualSync,
            (int)RMRecordStatus.MoveOverwrite,
            (int)RMRecordStatus.Moved,
        };

        #region castle properties
        public ISyncFailureItemDao SyncFailureItemDao { set; get; } = PlatformWindsorManager.GetService<ISyncFailureItemDao>();

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

        private IRMRuleDao mRuleDao;
        public IRMRuleDao RuleDao
        {
            get { return mRuleDao ?? (IRMRuleDao)PlatformWindsorManager.GetService(typeof(IRMRuleDao)); }
            set { mRuleDao = value; }
        }
        protected static readonly IRMKeyValueDao s_keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
        protected static readonly IRMCustomIndexMetadataDao s_customIndexMetadataDao = PlatformWindsorManager.GetService<IRMCustomIndexMetadataDao>();
        protected static readonly IRMCustomMetadataColumnDao s_customMetadataColumnDao = PlatformWindsorManager.GetService<IRMCustomMetadataColumnDao>();
        protected static readonly ISharePointSettingDao s_sharePointSettingDao = PlatformWindsorManager.GetService<ISharePointSettingDao>();
        protected static readonly ITeamsSettingDao s_teamsSettingDao = PlatformWindsorManager.GetService<ITeamsSettingDao>();
        protected static readonly IRMScopeDao s_scopeDao = PlatformWindsorManager.GetService<IRMScopeDao>();

        #endregion

        public RMSPExplorerBase(AveDiscoverSite discoverSite, SPTreeNodeDto treeNode, JobContext jobContext)
            : base(discoverSite, treeNode, jobContext)
        {
            var siteId = DiscoverSite.SiteID.ToString();
            _siteCache = RMSPExplorerDataCache.Instance.SiteLevelCache[siteId];
            _ = s_keyValueDao.TryGetBoolValue(KeyNameCollection.IsEnableCustomIndexMetadata, out var isEnable);
            if (isEnable)
            {
                CustomIndexMetadatas = s_customIndexMetadataDao.GetCustomIndexMetadatasBySourceFlagAsync(SourceFlag.SharePoint).GetAwaiter().GetResult().ToList();
                CustomMetadataColumns = s_customMetadataColumnDao.GetAllCustomMetadataColumnsAsync().GetAwaiter().GetResult().ToList();
            }

            _ = s_keyValueDao.TryGetBoolValue(KeyNameCollection.IsSyncStubFile, out _isSyncStubFile);

            syncItem = new RMSPSyncItem(_siteCache, CustomIndexMetadatas, CustomMetadataColumns);

            var numSetting = RMGlobalConfiguration.AppConfig[RMAppSettingKey.SPO_SYNC_DATA_ITEMS_PER_TASK];
            if (!string.IsNullOrEmpty(numSetting))
            {
                int.TryParse(numSetting, out _itemsPerTask);
            }
            //spo call limit
            var spoCallLimitPerSecond = RMGlobalConfiguration.AppConfig.GetNumberValue(RMAppSettingKey.SPO_SYNC_DATA_CALL_LIMIT_PER_SECOND, 50);
            _spoCallLimiter = CallLimiterFactory.CreateInstance("SPOCalllimiter", spoCallLimitPerSecond);

            //cosmos call limit
            var cosmosCallLimitPerSecond = RMGlobalConfiguration.AppConfig.GetNumberValue(RMAppSettingKey.COSMOS_SYNC_DATA_CALL_LIMIT_PER_SECOND, 20); ;
            _cosmosCallLimiter = CallLimiterFactory.CreateInstance("CosmosCallLimiter", cosmosCallLimitPerSecond);
            var groupTreeNode = SPTreeNodeManagement.GetGroupNode(treeNode);
            var siteTreeNode = SPTreeNodeManagement.GetSiteCollectionNode(treeNode);

            containerId = groupTreeNode.ID;
            currentSiteId = siteTreeNode.ID;

            mRuleCache = (RuleDao.GetRulesWithoutRemovedAsync().Result).ToDictionary(r => r.RuleId);

            if (nodeFlagType != NodeFlagType.TeamsSyncLibrary)
            {
                SiteSetting = RMLifecycleSetting.FromSharePointSetting(s_sharePointSettingDao.GetSettingInfoByScope(new Guid(containerId), new Guid(currentSiteId), new Guid(siteTreeNode.SPObjectId)));
                GroupSetting = RMLifecycleSetting.FromSharePointSetting(s_sharePointSettingDao.GetSettingInfoByScope(new Guid(containerId), Guid.Empty, new Guid(groupTreeNode.SPObjectId)));
                var siteSettings = RMLifecycleSetting.FromSharePointSetting(s_sharePointSettingDao.LoadSPSettingsUnderSite(new Guid(currentSiteId)));
                IsEnableInheritParentTerm = GroupSetting.IsInheritParentTerm
                                            || (SiteSetting != null && SiteSetting.IsInheritParentTerm)
                                            || siteSettings.Any(s => s.IsInheritParentTerm);
                _currentSiteSettings = siteSettings.Where(s => s.FolderId == Guid.Empty && s.WebId != Guid.Empty).OrderByDescending(s => s.FullPath).ToList();
                SourceFlag = SourceFlag.SharePoint;
            }
        }

        /// <summary>
        /// 检查setting，如果开启了批量插入数据到cosmos db,那么会做相关的初始化操作
        /// </summary>
        private void InitCosmosBulkOperation()
        {
            var RMKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
            _isCosmosBulkOperationEnabled = true;
            //RMKeyValueDao.IsCosmosBulkOperationEnabled();
            //if (_isCosmosBulkOperationEnabled)
            //{
            var bulkSize = RMKeyValueDao.GetCosmosBulkInsertOperationBufferSize();
            if (bulkSize == default(int)) bulkSize = CosmosBulkOperator.DefualtBufferSize;
            logger.Info($"Cosmos bulk operation enabled, bulk size: {bulkSize}");
            CosmosBulkOperator.Instance.Start(bulkSize, ProcessSucceedRecord, ProcessFailedRecord);
            //}
        }

        public void Init(ISPDiscover sPDiscover, SPDiscoverType discoverType, long lastJobTicks, long mainJobTicks, bool bulkImport, bool forceUpdate)
        {
            mDiscover = sPDiscover;
            mLastJobTicks = lastJobTicks;
            mMainJobTicks = mainJobTicks;
            mDiscoverType = discoverType;
            _forceUpdate = forceUpdate;
            if (bulkImport)
            {
                InitCosmosBulkOperation();
            }
        }

        public async Task ProcessInheritedParentTermItemsAsync()
        {
            var listLevelSettings = _currentSiteSettings.Where(s => s.ListId != Guid.Empty).ToList();
            if (listLevelSettings.Count > 0)
            {
                logger.Info("Start processing inherited parent term items by list level settings.");
                await ProcessInheritedParentTermItemsByListSettingsAsync(listLevelSettings);
            }

            var processedListIds = listLevelSettings.Select(s => s.ListId).ToList();
            var webLevelSettings = _currentSiteSettings.Where(s => s.ListId == Guid.Empty && s.WebId != Guid.Empty).ToList();
            if (webLevelSettings.Count > 0)
            {
                logger.Info("Start processing inherited parent term items by web level settings.");
                await ProcessInheritedParentTermItemsByWebSettingsAsync(webLevelSettings, processedListIds);
            }

            logger.Info("Start processing inherited parent term items by site level settings.");
            var processedWebIds = webLevelSettings.Select(s => s.WebId).ToList();
            await ProcessInheritedParentTermItemsBySiteSettingsAsync(SiteSetting ?? TeamsSetting ?? GroupSetting, processedListIds, processedWebIds);
        }

        private async Task ProcessInheritedParentTermItemsByListSettingsAsync(List<RMLifecycleSetting> listSettings)
        {
            var cosmosProcessor = new RMSPCosmosDBProcessor();
            if (!await cosmosProcessor.PrepareAsync())
            {
                logger.Warn("Cosmos DB is not prepared, skip processing inherited parent term items.");
                return;
            }
            foreach (var setting in listSettings)
            {
                try
                {
                    var currentList = await cosmosProcessor.SearchItemsAsync(item =>
                        item.SourceFlag == (int)SourceFlag &&
                        item.NodeType == (int)NodeLevel.List &&
                        item.AveSiteId == setting.SiteId.ToString() &&
                        item.WebId == setting.WebId &&
                        item.ListId == setting.ListId && 
                        s_optionalStatus.Contains(item.RecordStatus)).FirstOrDefaultAsync();
                    if (currentList == null)
                    {
                        logger.Info($"List item not found in Cosmos DB for list {setting.ListId}, skip processing inherited parent term items for this list.");
                        continue;
                    }

                    if (ShouldSkipCosmosList(currentList, setting.EnableLifecycleManagementForSharePointLists))
                    {
                        logger.Info($"Skip generic list {currentList.LeafName} because lifecycle management for SharePoint lists is disabled.");
                        continue;
                    }

                    var isInheritedTerm = setting.IsInheritParentTerm;
                    await ProcessCosmosDBListItems(isInheritedTerm, cosmosProcessor, currentList);
                }
                catch (Exception ex)
                {
                    logger.Error($"Error occurred while processing inherited parent term items for list {setting.ListId} . Error: {ex}");
                }
            }
            await cosmosProcessor.WaitFinishAsync();
        }

        private static bool ShouldSkipCosmosList(Record listRecord, bool enableLifecycleForLists)
        {
            if (enableLifecycleForLists)
            {
                return false;
            }
            return !string.IsNullOrEmpty(listRecord?.DirPath) &&
                   listRecord.DirPath.Contains("/Lists/", StringComparison.OrdinalIgnoreCase);
        }

        private async Task ProcessInheritedParentTermItemsByWebSettingsAsync(List<RMLifecycleSetting> webSettings, List<Guid> processedListIds)
        {
            var cosmosProcessor = new RMSPCosmosDBProcessor();
            if (!await cosmosProcessor.PrepareAsync())
            {
                logger.Warn("Cosmos DB is not prepared, skip processing inherited parent term items.");
                return;
            }
            foreach (var setting in webSettings)
            {
                try
                {
                    var currentWeb = await cosmosProcessor.SearchItemsAsync(item =>
                        item.SourceFlag == (int)SourceFlag &&
                        item.NodeType == (int)NodeLevel.Site &&
                        item.AveSiteId == setting.SiteId.ToString() &&
                        item.WebId == setting.WebId &&
                        item.ListId == Guid.Empty &&
                        s_optionalStatus.Contains(item.RecordStatus)).FirstOrDefaultAsync();

                    var needProcessLists = cosmosProcessor.SearchItemsAsync(item =>
                        item.SourceFlag == (int)SourceFlag &&
                        item.NodeType == (int)NodeLevel.List &&
                        item.AveSiteId == setting.SiteId.ToString() &&
                        item.WebId == setting.WebId &&
                        item.ListId != Guid.Empty &&
                        s_optionalStatus.Contains(item.RecordStatus) &&
                        !processedListIds.Contains(item.ListId));

                    var isInheritedTerm = setting.IsInheritParentTerm;
                    await foreach (var listItem in needProcessLists)
                    {
                        var currentListSetting = ResolveLifecycleSetting(GetInheritedSetting(listItem.ListId, listItem.FullPath), setting);
                        var enableLists = currentListSetting?.EnableLifecycleManagementForSharePointLists ?? true;
                        if (ShouldSkipCosmosList(listItem, enableLists))
                        {
                            continue;
                        }

                        try
                        {
                            await ProcessCosmosDBListItems(isInheritedTerm, cosmosProcessor, listItem);
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Error occurred while processing inherited parent term items for list {setting.ListId} . Error: {ex}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Error occurred while processing inherited parent term items for list {setting.ListId} . Error: {ex}");
                }
            }
            await cosmosProcessor.WaitFinishAsync();
        }

        private async Task ProcessInheritedParentTermItemsBySiteSettingsAsync(RMLifecycleSetting siteSetting, List<Guid> processedListIds, List<Guid> processedWebIds)
        {
            if (siteSetting == null)
            {
                return;
            }

            var cosmosProcessor = new RMSPCosmosDBProcessor();
            if (!await cosmosProcessor.PrepareAsync())
            {
                logger.Warn("Cosmos DB is not prepared, skip processing inherited parent term items.");
                return;
            }

            try
            {
                var currentSite = await cosmosProcessor.SearchItemsAsync(item =>
                    item.SourceFlag == (int)SourceFlag &&
                    item.NodeType == (int)NodeLevel.SiteCollection &&
                    item.AveSiteId == currentSiteId &&
                    item.WebId == Guid.Empty &&
                    item.ListId == Guid.Empty &&
                    s_optionalStatus.Contains(item.RecordStatus)).FirstOrDefaultAsync();

                var needProcessLists = cosmosProcessor.SearchItemsAsync(item =>
                    item.SourceFlag == (int)SourceFlag &&
                    item.NodeType == (int)NodeLevel.List &&
                    item.AveSiteId == currentSiteId &&
                    item.WebId != Guid.Empty &&
                    item.ListId != Guid.Empty &&
                    !processedWebIds.Contains(item.WebId) &&
                    !processedListIds.Contains(item.ListId) &&
                    s_optionalStatus.Contains(item.RecordStatus));

                var isInheritedTerm = siteSetting.IsInheritParentTerm;
                await foreach (var listItem in needProcessLists)
                {
                    var currentListSetting = ResolveLifecycleSetting(GetInheritedSetting(listItem.ListId, listItem.FullPath), siteSetting);
                    var enableLists = currentListSetting?.EnableLifecycleManagementForSharePointLists ?? true;
                    if (ShouldSkipCosmosList(listItem, enableLists))
                    {
                        continue;
                    }

                    try
                    {
                        await ProcessCosmosDBListItems(isInheritedTerm, cosmosProcessor, listItem);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Error occurred while processing inherited parent term items for list {siteSetting.ListId} . Error: {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred while processing inherited parent term items for list {siteSetting.ListId} . Error: {ex}");
            }
            await cosmosProcessor.WaitFinishAsync();
        }

        private async Task ProcessCosmosDBListItems(bool isInheritedTerm, RMSPCosmosDBProcessor cosmosProcessor, Record currentList)
        {
            if(isInheritedTerm)
            {
                logger.Info($"Processing inherited parent term items for list {currentList.ListId}, setting inherited term to {currentList.TermId}.");
                var listItems = cosmosProcessor.SearchItemsAsync(item =>
                            item.SourceFlag == (int)SourceFlag &&
                            (item.NodeType == (int)NodeLevel.Item || item.NodeType == (int)NodeLevel.Document) &&
                            item.AveSiteId == currentList.AveSiteId &&
                            item.WebId == currentList.WebId &&
                            item.ListId == currentList.ListId &&
                            ((item.TermId != Guid.Empty &&
                            item.IsInheritedTerm == true &&
                            item.TermId != currentList.TermId) || item.TermId == Guid.Empty)
                            && s_optionalStatus.Contains(item.RecordStatus));

                await foreach (var listItem in listItems)
                {
                    listItem.TermId = currentList.TermId;
                    listItem.TermName = currentList.TermName;
                    listItem.IsInheritedTerm = true;
                    await cosmosProcessor.AddItemAsync(listItem);
                    JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                    {
                        ObjectName = listItem.LeafName,
                        FullPath = GetListItemFullPath(listItem),
                        Status = JobDetailsStatus.Successful,
                    });
                }
            }
            else
            {
                logger.Info($"Processing inherited parent term items for list {currentList.ListId}, clearing inherited term.");
                var listItems = cosmosProcessor.SearchItemsAsync(item =>
                            item.SourceFlag == (int)SourceFlag &&
                            (item.NodeType == (int)NodeLevel.Item || item.NodeType == (int)NodeLevel.Document) &&
                            item.AveSiteId == currentList.AveSiteId &&
                            item.WebId == currentList.WebId &&
                            item.ListId == currentList.ListId &&
                            item.TermId != Guid.Empty &&
                            item.IsInheritedTerm == true 
                            && s_optionalStatus.Contains(item.RecordStatus));

                await foreach (var listItem in listItems)
                {
                    listItem.TermId = Guid.Empty;
                    listItem.TermName = string.Empty;
                    listItem.IsInheritedTerm = false;
                    await cosmosProcessor.AddItemAsync(listItem);
                    JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                    {
                        ObjectName = listItem.LeafName,
                        FullPath = GetListItemFullPath(listItem),
                        Status = JobDetailsStatus.Successful,
                    });
                }
            }
        }

        private string GetListItemFullPath(Record listItem)
        {
            if (mScopeInfoCache.ContainsKey(listItem.ScopeId))
            {
                return WebUtil.MakeFullUrl(mScopeInfoCache[listItem.ScopeId], listItem.DirPath);
            }

            var dicMap = s_scopeDao.GetScopeInfoByIds(new List<Guid>() { listItem.ScopeId });
            if (dicMap.ContainsKey(listItem.ScopeId))
            {
                var sPath = dicMap[listItem.ScopeId];
                mScopeInfoCache[listItem.ScopeId] = sPath.FullPath;
                return WebUtil.MakeFullUrl(sPath?.FullPath, listItem.DirPath);
            }

            return string.Empty;
        }

        public override async System.Threading.Tasks.Task RunNowAsync()
        {
            try
            {
                using (var performance = new PerformanceScope("RMSPExplorerBase.RunNow", addToStatistics: true))
                {
                    using (CheckJobStopScope stopScopeAll = new CheckJobStopScope())
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
                                logger.Info($"No SP rules realted to the site {aveSite.RootWeb.ServerRelativeUrl}");
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
                        Record recordInDB = null;
                        using (var performance0 = new PerformanceScope("RMSPExplorerProcessor.GetDBRecord", addToStatistics: true))
                        {
                            RetryPolicy.ExecuteAction(() =>
                            {
                                WaitCosmosExecuteAction(() =>
                                {
                                    recordInDB = ExplorerDao.ReadById(item.ScopeId, item.Id);
                                });
                            });
                        }
                        await SyncItemToDBAsync(item, recordInDB);
                        var webs = mDiscover.GetWebs(DiscoverSite);

                        JobContext.ReportManager.IncreaseBase(webs.LongCount());
                        foreach (var web in webs)
                        {
                            using (CheckJobStopScope stopScope = new CheckJobStopScope())
                            {
                                using (web)
                                {
                                    await ProcessWebAsync(web, itemRuleInfo);
                                }
                            }
                        }
                        AddSiteScope(item);
                        if (_isCosmosBulkOperationEnabled)
                        {
                            CosmosBulkOperator.Instance.Complete();
                            CosmosBulkOperator.Instance.Reset();
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

        }
        
        public virtual async System.Threading.Tasks.Task ProcessWebAsync(AveDiscoverWeb discoverWeb, SyncItemRuleInfo parentItemRule)
        {
            try
            {
                using (var performance = new PerformanceScope("RMSPExplorerProcessor.ProcessWeb", $"RMSPExplorerProcessor.ProcessWeb:[{discoverWeb.Name}]", addToStatistics: true))
                {
                    using (CheckJobStopScope stopScopeAll = new CheckJobStopScope())
                    {
                        logger.Info($"Process web:{discoverWeb?.FullUrl}");

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
                                logger.Info($"No SP rules realted to the web {aveWeb.ServerRelativeUrl}");
                            }
                            else
                            {
                                var filterEnginer = new RMSPRuleChecker(newRuleCollection);
                                itemRuleInfo = filterEnginer.CheckDisposalRule(discoverWeb, parentItemRule);

                            }

                        }
                        itemRuleInfo.TermInfo = termInfo;
                        var item = syncItem.AssembleRecord(discoverWeb, itemRuleInfo);
                        Record recordInDB = null;
                        using (var performance0 = new PerformanceScope("RMSPExplorerProcessor.GetDBRecord", addToStatistics: true))
                        {
                            RetryPolicy.ExecuteAction(() =>
                            {
                                WaitCosmosExecuteAction(() =>
                                {
                                    recordInDB = ExplorerDao.ReadById(item.ScopeId, item.Id);
                                });
                            });
                        }
                        await SyncItemToDBAsync(item, recordInDB);
                        RMLifecycleSetting webSetting = GetInheritedSetting(discoverWeb.WebID, discoverWeb.FullUrl);
                        var lists = mDiscover.GetLists(discoverWeb);
                        var parentTermInfo = parentItemRule.TermInfo;
                        if (termInfo != null && termInfo.UniqueId != Guid.Empty)
                        {
                            parentTermInfo = termInfo;
                        }
                        else
                        {
                            if (IsEnableInheritParentTerm)
                            {
                                parentTermInfo = ResolveAncestorTerm(aveWeb, parentItemRule?.TermInfo);
                            }
                        }

                        JobContext.ReportManager.IncreaseBase(lists.LongCount());
                        foreach (var list in lists)
                        {
                            using (CheckJobStopScope stopScope = new CheckJobStopScope())
                            {
                                using (list)
                                {
                                    await ProcessListAsync(discoverWeb, list, itemRuleInfo, discoverWeb.WebID, webSetting, parentTermInfo);
                                }
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
            }
            finally
            {
                JobContext.ReportManager.Increase();
            }

        }

        private RMLifecycleSetting GetInheritedSetting(Guid scopeId ,string currentFullUrl)
        {
            if (string.IsNullOrEmpty(currentFullUrl) || _currentSiteSettings.Count == 0) return null;

            var currentSetting = _currentSiteSettings.FirstOrDefault(s => s.ScopeId == scopeId);
            if (currentSetting != null)
            {
                return currentSetting;
            }

            foreach (var entry in _currentSiteSettings)
            {
                if (currentFullUrl.StartsWith(entry.FullPath, StringComparison.OrdinalIgnoreCase))
                {
                    return entry;
                }
            }
            return null;
        }

        /// <summary>
        /// 沿父级 Web 向上查找第一个有有效 Term 的 Web；如果全部没有，则返回 fallbackTerm。
        /// </summary>
        /// <param name="currentWeb">当前 Ave Web 对象</param>
        /// <param name="fallbackTerm">回退使用的父级传入 Term</param>
        /// <returns>解析出的 Term 信息</returns>
        private RMTermInfo ResolveAncestorTerm(IAveWeb currentWeb, RMTermInfo fallbackTerm)
        {
            try
            {
                var visited = 0;
                IAveWeb cursor = currentWeb?.ParentWeb; // 从父级开始
                while (cursor != null && visited < 64) // 防止异常循环，给一个上限
                {
                    visited++;
                    RMTermInfo ti = null;
                    try
                    {
                        ti = GetTermInfo(cursor.Properties);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn($"ResolveAncestorTerm: read term failed on web {cursor?.ServerRelativeUrl}: {ex.Message}");
                    }
                    if (ti != null && ti.UniqueId != Guid.Empty)
                    {
                        return ti;
                    }
                    cursor = cursor.ParentWeb;
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"ResolveAncestorTerm unexpected error: {ex.Message}");
            }
            return fallbackTerm; // 全部未找到则回退
        }
        
        public virtual async System.Threading.Tasks.Task ProcessListAsync(AveDiscoverWeb discoverWeb, AveDiscoverList discoverList, SyncItemRuleInfo parentItemRule, Guid webId, RMLifecycleSetting webSetting, RMTermInfo parentTermInfo)
        {
            string listPath = string.Empty;
            try
            {
                using (var performance = new PerformanceScope("RMSPExplorerProcessor.ProcessList", $"RMSPExplorerProcessor.ProcessList:{discoverList?.RootFolderUrl}", addToStatistics: true))
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

                        listPath = WebUtil.MakeFullUrl(list.ParentWeb.Url, list.RootFolder.Url);
                        var listSetting = GetInheritedSetting(discoverList.ListId, listPath);
                        var effectiveSetting = ResolveLifecycleSetting(listSetting, webSetting);
                        var isGenericList = IsGenericList(list);
                        if (ShouldSkipList(list, effectiveSetting))
                        {
                            await PersistSkippedListAsync(discoverList);
                            return;
                        }

                        if (!HasBCSColumn(list))
                        {
                            logger.Warn($"list does not have bcs column, list:{discoverList?.RootFolderUrl}, column name:{_siteCache.BCSColumnInternalName}");
                            return;
                        }

                        var termInfo = GetTermInfo(list.RootFolder.Properties);
                        RMRuleItemCollection rules = null;
                        SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
                        if (RMSPExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                        {
                            var newRuleCollection = RebuldSPRules(rules);
                            if (newRuleCollection.Rules.Count == 0)
                            {
                                logger.Info($"No SP rules realted to the list {list.RootFolder.ServerRelativeUrl}");
                            }
                            else
                            {
                                var filterEnginer = new RMSPRuleChecker(newRuleCollection);
                                itemRuleInfo = filterEnginer.CheckDisposalRule(discoverList, list, parentItemRule);

                            }
                        }
                        if (termInfo != null && termInfo.UniqueId != Guid.Empty)
                        {
                            parentTermInfo = termInfo;
                        }
                        itemRuleInfo.TermInfo = termInfo;
                        var item = syncItem.AssembleRecord(discoverList, itemRuleInfo);
                        Record recordInDB = null;
                        using (var performance0 = new PerformanceScope("RMSPExplorerProcessor.GetDBRecord", addToStatistics: true))
                        {
                            RetryPolicy.ExecuteAction(() =>
                            {
                                WaitCosmosExecuteAction(() =>
                                {
                                    recordInDB = ExplorerDao.ReadById(item.ScopeId, item.Id);
                                });
                            });
                        }
                        await SyncItemToDBAsync(item, recordInDB);
                        logger.Info($"Get items under [{list.RootFolder.ServerRelativeUrl}].");
                        switch (mDiscoverType)
                        {
                            case SPDiscoverType.Full:
                                logger.Info($"Start to sync items for full discover.[{list.RootFolder.ServerRelativeUrl}].");
                                int totalItemCount = 0;
                                try
                                {
                                    totalItemCount = await SyncItemsForFullDiscoverAsync(list, parentItemRule, effectiveSetting, parentTermInfo);
                                }
                                catch (JobStopException)
                                {
                                    throw new JobStopException("This Job is stopped.");
                                }
                                catch (Exception e)
                                {
                                    await HandleErrorForSyncItemsAsync(list, discoverList, parentItemRule, e, effectiveSetting, parentTermInfo);
                                }
                                
                                var deletedItems = discoverList.GetListDeletedItems(webId);
                                if (deletedItems != null && deletedItems.Count > 0)
                                {
                                    var changedObjects = deletedItems.Values.Select(i => i as Dictionary<string, object>).Where(i => (i.ContainsKey("Hidden") && !(bool)i["Hidden"]) || !i.ContainsKey("Hidden")).ToList();
                                    var needProcessDeletedItems = changedObjects.Where(i => (int)i["ChangeType"] == (int)Wrapper.Common.ChangeType.Delete).ToList();
                                    logger.Info($"Start to process deleted items for full discover.[{list.RootFolder.ServerRelativeUrl}].");
                                    await ProcessDeletedItemsAsync(list, needProcessDeletedItems);
                                    logger.Info($"Process deleted items for full discover finished.[{list.RootFolder.ServerRelativeUrl}].");
                                }
                                logger.Info($"Sync items for full discover finished.[{list.RootFolder.ServerRelativeUrl}]");
                                //Full job optimiz for Cardinia
                                if (totalItemCount > 10000 && !unSuccessList.Contains(list.ID)) //超过10000 Items的Library，没有失败的Item， 加入NodeFlag， 为List Incremental做准备
                                {
                                    AddListFlag(discoverList, totalItemCount);
                                }
                                break;
                            case SPDiscoverType.CAMLSearch:
                                await ProcessFailedItemsAsync(list, parentItemRule, effectiveSetting, parentTermInfo);
                                logger.Info($"Start to sync items for search discover.[{list.RootFolder.ServerRelativeUrl}].");
                                try
                                {
                                    await SyncItemsForSearchDicsoverAsync(list, parentItemRule, effectiveSetting, parentTermInfo);
                                }
                                catch (JobStopException)
                                {
                                    throw new JobStopException("This Job is stopped.");
                                }
                                catch (Exception e)
                                {
                                    await HandleErrorForSyncItemsAsync(list, discoverList, parentItemRule, e, effectiveSetting, parentTermInfo);
                                }
                                logger.Info($"Sync items for search discover finished.[{list.RootFolder.ServerRelativeUrl}]");
                                ProcessDeletedData(discoverList, list, webId);
                                break;
                            default:
                                await ProcessFailedItemsAsync(list, parentItemRule, effectiveSetting, parentTermInfo);
                                logger.Info($"Get changed items under [{list.RootFolder.ServerRelativeUrl}] for incremental sync job.");
                                Dictionary<string, object> changedItems = new Dictionary<string, object>();
                                using (var performance1 = new PerformanceScope("RMSPExplorerProcessor.GetItemsForRecords", addToStatistics: true))
                                {
                                    changedItems = discoverList.GetListChangedItems(webId);
                                }

                                logger.Info($"Start to sync items for incremental discover.[{list.RootFolder.ServerRelativeUrl}].");
                                await ProcessIncrementalChangedItemsAsync(list, changedItems, list.RootFolder.UniqueId, parentItemRule, effectiveSetting, parentTermInfo);

                                logger.Info($"Sync items for incremental discover finished.[{list.RootFolder.ServerRelativeUrl}]");
                                break;
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
            }
            finally
            {
                JobContext.ReportManager.Increase();
            }
        }

        private async Task<int> SyncItemsForFullDiscoverAsync(IAveList list, SyncItemRuleInfo parentItemRule, RMLifecycleSetting listSetting, RMTermInfo parentTermInfo)
        {
            try
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
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        using (var performance = new PerformanceScope("RMSPExplorerProcessor.GetItemsForRecords", addToStatistics: true))
                        {
                            items = list.GetItemsForRecords(query);
                        }
                    }
                    // JobContext.ReportManager.IncreaseBase(items.Count);
                    logger.Info($"Existing job process item count:[{items.Count}]");
                    totalItemCount += items.Count;
                    await ProcessAveItemsAsync(items, list.RootFolder.UniqueId, parentItemRule, listSetting, parentTermInfo);
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
            catch (JobStopException)
            {
                throw new JobStopException("the job has stopped.");
            }
        }

        private async System.Threading.Tasks.Task HandleErrorForSyncItemsAsync(IAveList list, AveDiscoverList discoverList, SyncItemRuleInfo parentItemRule, Exception e, RMLifecycleSetting listSetting, RMTermInfo parentTermInfo)
        {
            string errorMessage = GetExceptionMessage(e);
            logger.Error($"An error occurred while syncing items in list:{list.Title}. ErrorMessage:{errorMessage} Error:{e.ToString()}");
            if (!string.IsNullOrWhiteSpace(errorMessage)
                && (errorMessage.Contains("The attempted operation is prohibited because it exceeds the list view threshold")
                || errorMessage.Contains("Der versuchte Vorgang ist unzulässig, weil er den Schwellenwert für die Listenansicht überschreitet")))
            {
                logger.Info($"Start to run full discover in list:{list.RootFolder.ServerRelativeUrl}");
                using (var folder = discoverList.GetRootFolderForFullDiscover())
                {
                    var itemIds = folder.GetItemIDsWithStructureForRecords();
                    var itemCount = folder.GetItemCount();
                    logger.Info($"Total item count:{itemCount} in list:{list.RootFolder.ServerRelativeUrl}");
                    if (itemCount > 0)
                    {
                        var parentId = list.RootFolder.UniqueId;
                        AvePoint.GCommon.Utility.ArgumentCheck.NotNull(itemIds, nameof(itemIds));
                        for (int i = 0; i < itemCount; i += 2000)
                        {
                            var rowIds = itemIds.Skip(i).Take(2000).ToList();
                            IEnumerable<IAveListItem> items = GetItemsByRowIds(list, rowIds);
                            JobContext.ReportManager.IncreaseBase(items.Count());
                            if (items.Count() > itemsPerTask)
                            {
                                var cts = new System.Threading.CancellationTokenSource();
                                AveTenantTasks.RunParallelBatch(items, itemsPerTask, cts, item =>
                                {
                                    ProcessAveItemBatchAsync(item, parentId, parentItemRule, listSetting, parentTermInfo, cts).Wait();
                                });
                            }
                            else
                            {
                                //foreach (var item in items)
                                //{
                                //    ProcessAveItem(item, parentId, parentItemRule);
                                //}
                                await ProcessAveItemBatchAsync(items, parentId, parentItemRule, listSetting, parentTermInfo);
                            }
                        }
                    }
                }
                logger.Info($"Run full discover in list:{list.RootFolder.ServerRelativeUrl} finish.");
            }
            else
            {
                throw e;
            }
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
                        NodeFlagType = (int)nodeFlagType
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

        private async System.Threading.Tasks.Task SyncItemsForSearchDicsoverAsync(IAveList list, SyncItemRuleInfo parentItemRule, RMLifecycleSetting listSetting, RMTermInfo parentTermInfo)
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
                    using (var queryAuto = new PerformanceScope("RMSPExplorerBase.SearchQueryData", $"RMSPExplorerBase.SearchQueryData{list.RootFolder.ServerRelativeUrl} start{startIndex}", true))
                    {
                        AveCamlQuery query = GetSearchDiscoverQuery(list, list.RootFolder, startTime, endTime, startIndex, startIndex + rowLimit, rowLimit);
                        using (CheckJobStopScope jScope = new CheckJobStopScope())
                        {
                            using (var performance = new PerformanceScope("RMSPExplorerProcessor.GetItemsForRecords", addToStatistics: true))
                            {
                                items = list.GetItemsForRecords(query);
                            }
                        }
                        //JobContext.ReportManager.IncreaseBase(items.Count);
                        logger.Info($"Data sync job process folder url {list.RootFolder.ServerRelativeUrl} item count:[{items.Count}], start index {startIndex}, end index {startIndex + rowLimit}");
                    }
                    using (var queryAuto = new PerformanceScope("RMSPExplorerBase.ProcessAveItems", $"RMSPExplorerBase.ProcessAveItems{list.RootFolder.ServerRelativeUrl} count {items.Count}", true))
                    {
                        using (CheckJobStopScope jScope = new CheckJobStopScope())
                        {
                            await ProcessAveItemsAsync(items, list.RootFolder.UniqueId, parentItemRule, listSetting, parentTermInfo);
                        }
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
                throw new JobStopException("the job has stopped.");
            }
        }

        private void ProcessDeletedData(AveDiscoverList discoverList, IAveList list, Guid webId)
        {
            logger.Info($"Start to process deleted data in {list.RootFolder.ServerRelativeUrl}");
            try
            {
                if (mLastJobTicks != DateTime.MinValue.Ticks)
                {
                    var changedItems = discoverList.GetListChangedItems(webId, new DateTime(mLastJobTicks, DateTimeKind.Utc), new DateTime(mMainJobTicks, DateTimeKind.Utc));
                    ProcessDeletedItems(list, changedItems);
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while process deleted data. Error{0}", e.ToString());
            }
            logger.Info($"Process deleted data in {list.RootFolder.ServerRelativeUrl} finished.");
        }

        private void ProcessDeletedItems(IAveList list, Dictionary<string, object> changedItems)
        {
            foreach (var changeItem in changedItems)
            {
                using (var performance = new PerformanceScope("RMSPExplorerBase.ProcessDeletedItems", addToStatistics: true))
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
                            //try
                            //{
                            //    WaitSPOExecuteAction(() =>
                            //    {
                            //        var aveItem = list.GetItemById(itemId);
                            //    });
                            //}
                            //catch (Exception ex)
                            //{
                            //    logger.Info($"cannot found item object ID:{itemId} Guid:{itemUniqueId} :{ex.ToString()}");

                            RemoveSPObj(itemUniqueId, itemId);
                            //}
                            logger.Warn($"item no longer exist, remove record from explorer.ID:{itemId} Guid:{itemUniqueId}");
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

        #region Helper: Find items whose container term UniqueId is empty
        protected System.Threading.Tasks.Task<List<IAveListItem>> GetItemsWithEmptyContainerTermAsync(
            IAveList list,
            int batchSize = 500,
            int? max = null,
            CancellationToken token = default)
        {
            List<IAveListItem> result = new List<IAveListItem>();
            try
            {
                string primaryField = _siteCache?.BCSColumnInternalName;
                if (string.IsNullOrEmpty(primaryField))
                {
                    logger.Warn("BCS / Container term internal name not set in _siteCache, skip empty term scan.");
            return System.Threading.Tasks.Task.FromResult(result);
                }

                string[] fallbackNames = new[]
                {
                    primaryField,
                    primaryField + "_0",
                    primaryField + "TaxHTField0",
                };

                AveItemCollectionPosition position = null;
                bool needQueryNext;
                int fetched = 0;
                do
                {
                    token.ThrowIfCancellationRequested();
                    needQueryNext = false;

                    string viewXml = $@"<View><ViewFields><FieldRef Name='ID'/><FieldRef Name='{primaryField}'/></ViewFields><Query><Where><Or><IsNull><FieldRef Name='{primaryField}'/></IsNull><Eq><FieldRef Name='{primaryField}'/><Value Type='Text'></Value></Eq></Or></Where></Query><RowLimit>{batchSize}</RowLimit></View>";
                    var nextPosition = position ?? new AveItemCollectionPosition();
                    AveCamlQuery caml = new AveCamlQuery
                    {
                        FolderServerRelativeUrl = list.RootFolder.ServerRelativeUrl,
                        ViewXml = viewXml,
                        ListItemCollectionPosition = nextPosition
                    };

                    IAveListItemCollection collection = null;
                    WaitSPOExecuteAction(() =>
                    {
                        collection = list.GetItems(caml);
                    });

                    if (collection == null || collection.Count == 0)
                    {
                        break;
                    }

                    foreach (IAveListItem item in collection)
                    {
                        if (item == null) continue;
                        if (max.HasValue && result.Count >= max.Value) break;

                        IAveListItem confirmedItem = ReloadItemToGetBCSColumn(item);
                        var fields = confirmedItem.ParentList.Fields;
                        RMTermInfo ti = null;
                        try
                        {
                            ti = GetTermInfo(confirmedItem, fields);
                            if (ti == null || ti.UniqueId == Guid.Empty)
                            {
                                foreach (var fn in fallbackNames)
                                {
                                    if (ti != null && ti.UniqueId != Guid.Empty) break;
                                    if (fields.ContainsField(fn) && confirmedItem.FieldValues.ContainsKey(fn))
                                    {
                                        var raw = confirmedItem.FieldValues[fn];
                                        if (raw != null)
                                        {
                                            var str = raw.ToString();
                                            if (!string.IsNullOrWhiteSpace(str) && str.Contains("|") && str.Length > 36)
                                            {
                                                ti = GetTermInfo(confirmedItem, fields);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Debug($"Second-stage term parse failed for item {confirmedItem.ID}: {ex.Message}");
                        }

                        if (ti == null || ti.UniqueId == Guid.Empty)
                        {
                            result.Add(confirmedItem);
                        }
                    }

                    fetched += collection.Count;
                    if (collection.ListItemCollectionPosition != null && !string.IsNullOrEmpty(collection.ListItemCollectionPosition.PagingInfo)
                        && (!max.HasValue || result.Count < max.Value))
                    {
                        position = collection.ListItemCollectionPosition as AveItemCollectionPosition;
                        needQueryNext = true;
                    }

                    if (max.HasValue && result.Count >= max.Value)
                    {
                        needQueryNext = false;
                    }
                } while (needQueryNext);

                logger.Info($"Empty term scan complete. Candidate fetched:{fetched}, confirmed empty:{result.Count}");
            }
            catch (OperationCanceledException)
            {
                logger.Warn("Empty term scan canceled by token.");
            }
            catch (Exception ex)
            {
                logger.Error($"Error while scanning empty term items: {ex.Message}");
            }
            return System.Threading.Tasks.Task.FromResult(result);
        }
        #endregion

        [Obsolete("use ProcessIncrementalChangedItems instead")]
        public virtual async System.Threading.Tasks.Task ProcessItemsAsync(IAveList list, IEnumerable<AveDiscoverItem> items, Guid parentId, SyncItemRuleInfo parentItemRule)
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
                        ProcessItemAsync(list, discoverItem, parentId, parentItemRule, cts).Wait();
                    }
                });
            }
            else
            {
                foreach (var discoverItem in items)
                {
                    using (discoverItem)
                    {
                        await ProcessItemAsync(list, discoverItem, parentId, parentItemRule);
                    }
                }
            }
        }

        public virtual async System.Threading.Tasks.Task ProcessIncrementalChangedItemsAsync(IAveList list, Dictionary<string, object> changedItems, Guid parentId, SyncItemRuleInfo parentItemRule, RMLifecycleSetting listSetting, RMTermInfo parentTermInfo)
        {
            try
            {
                using (CheckJobStopScope stopScopeAll = new CheckJobStopScope())
                {
                    int incItemsPerTask = changedItems.Count() / 4;
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
                            using (CheckJobStopScope stopScope1 = new CheckJobStopScope())
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
                                ProcessIncrementalChangedItemV1Async(list, changedItem, parentId, parentItemRule, uniqueIdUtil, listSetting, parentTermInfo, cts).Wait();
                            });
                        }
                        else
                        {
                            foreach (var changedItem in items)
                            {
                                using (CheckJobStopScope stopScope2 = new CheckJobStopScope())
                                {
                                    await ProcessIncrementalChangedItemV1Async(list, changedItem, parentId, parentItemRule, uniqueIdUtil, listSetting, parentTermInfo);
                                }
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

        private async System.Threading.Tasks.Task ProcessDeletedItemsAsync(IAveList list, List<Dictionary<string, object>> deletedItems)
        {
            int deleteItemsPerTask = deletedItems.Count / 4;
            CancellationTokenSource cts0 = null;
            if (deletedItems.Count > itemsPerTask)
            {
                cts0 = new CancellationTokenSource();
                //最多起4~5个Task处理Incremental的Changed Item，Full Job Get Item默认2k，因此itemsPerTask固定，但是Incremental items 数量不固定，因此需要按照多个处理。
                AveTenantTasks.RunParallel(deletedItems, deleteItemsPerTask, cts0, changedItem =>
                {
                    ProcessIncrementalDeleteItem(list, changedItem, cts0);
                });
            }
            else
            {
                foreach (var changedItem in deletedItems)
                {
                    using (CheckJobStopScope stopScope1 = new CheckJobStopScope())
                    {
                        ProcessIncrementalDeleteItem(list, changedItem);
                    }
                }
            }
        }

        public virtual async System.Threading.Tasks.Task ProcessAveItemsAsync(IAveListItemCollection items, Guid parentId, SyncItemRuleInfo parentItemRule, RMLifecycleSetting listSetting, RMTermInfo parentTermInfo)
        {
            try
            {
                logger.Info($"Process item count:[{items.Count}]");
                JobContext.ReportManager.IncreaseBase(items.Count);
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    if (items.Count > itemsPerTask)
                    {
                        var cts = new System.Threading.CancellationTokenSource();
                        AveTenantTasks.RunParallelBatch(items, itemsPerTask, cts, item =>
                        {
                            ProcessAveItemBatchAsync(item, parentId, parentItemRule, listSetting, parentTermInfo, cts).Wait();
                        });
                    }
                    else
                    {
                        //foreach (var item in items)
                        //{
                        //    ProcessAveItem(item, parentId, parentItemRule);
                        //}
                        await ProcessAveItemBatchAsync(items, parentId, parentItemRule, listSetting, parentTermInfo);
                    }
                }
            }
            catch (JobStopException)
            {
                throw new JobStopException("the job has stopped.");
            }

        }
        public IAveListItem ReloadItemToGetBCSColumn(IAveListItem item)
        {
            using (var performance = new PerformanceScope("RMSPExplorerProcessor.ReloadItemToGetBCSColumn", addToStatistics: true))
            {
                if (!item.Fields.ContainsField(_siteCache.BCSColumnInternalName) || !item.FieldValues.ContainsKey(_siteCache.BCSColumnInternalName))
                {
                    logger.Info($"Reload item to get BCS column. Item ID:[{item.ID}]");
                    return item.ParentList.GetItemById(item.ID);
                }
                else
                {
                    return item;
                }
            }
        }

        public async System.Threading.Tasks.Task ProcessAveItemBatchAsync(IEnumerable<IAveListItem> aveItems, Guid parentId, SyncItemRuleInfo parentItemRule, RMLifecycleSetting listSetting, RMTermInfo parentTermInfo, CancellationTokenSource cts = null)
        {
            try
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
                    foreach (IAveListItem tempItem in folders)
                    {
                        IAveListItem item = ReloadItemToGetBCSColumn(tempItem);
                        IAveFolder folder = item.Folder;
                        if (folder != null)
                        {
                            using (CheckJobStopScope jScope = new CheckJobStopScope())
                            {
                                await ProcessAveFolderAsync(folder, parentId, parentItemRule, idUtil);
                            }
                        }
                    }
                }

            }
            catch (JobStopException)
            {
                throw new JobStopException("the job has stopped.");
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
                foreach (IAveListItem tempAveItem in items)
                {
                    IAveListItem aveItem = ReloadItemToGetBCSColumn(tempAveItem);
                    string itemName = aveItem?.Name; // string.Empty;
                    string itemUrl = aveItem?.FullPath(); // string.Empty; 
                    try
                    {
                        using (var performance = new PerformanceScope("SP.RMSPExplorerProcessor.ProcessAveItem", addToStatistics: true))
                        {
                            await InnerProcessAveItemAsync(aveItem, parentId, parentItemRule, itemAndOwnerMapping, idUtil, listSetting, parentTermInfo);
                        }
                    }
                    catch (JobStopException)
                    {
                        throw new JobStopException("the job has stopped.");
                    }
                    catch (Exception e)
                    {
                        logger.Error($"error occurred while Process aveitem:{aveItem?.UniqueId}, ERROR:{e.ToString()}");
                        //Item 级别失败暂时不更新这两个标记
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
                        this.AddFailureItem2Cache(aveItem, parentId, e);
                    }
                }

            }

        }

        protected virtual void AddFailureItem2Cache(IAveListItem aveItem, Guid parentId, Exception e)
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
                    WebId = aveItem.ParentList.ParentWeb.ID.ToString()
                };
                failureItem.URL = aveItem?.Url;
                failureItem.ObjectName = aveItem?.Name;
                failureItem.Message = this.GetExceptionMessage(e);
                this.FailureItems.Add(failureItem);
            }
        }

        protected virtual void AddFailureItem2Cache(Record record, Exception e)
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
                    Message = e?.Message
                };
                this.FailureItems.Add(failureItem);
            }
        }

        protected virtual void AddFailureItem2Azure()
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

        public System.Threading.Tasks.Task InnerProcessAveItemAsync(IAveListItem aveItem, Guid parentId, SyncItemRuleInfo parentItemRule, Dictionary<Guid, string> itemAndOwnerMapping, UniqueIdUtil uniqueIdUtil, RMLifecycleSetting listSetting, RMTermInfo parentTermInfo)
        {
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    if (!_isSyncStubFile && aveItem.IsStubItem())
                    {
                        logger.Debug($"Current item [{aveItem?.UniqueId}] is stub file, so skipped.");
                        return Task.CompletedTask;
                    }
                    if (ShouldSkipArchivedItem(aveItem))
                    {
                        RemoveSPObj(aveItem.UniqueId, aveItem.ID);
                        logger.Info($"Current item [{aveItem?.UniqueId}] is fully archived, so skipped in sync.");
                        return Task.CompletedTask;
                    }
                    JobContext.ReportManager.Increase();
                    RMTermInfo termInfo;
                    using (var performance = new PerformanceScope("RMSPExplorerProcessor.GetTermInfo", addToStatistics: true))
                    {
                        termInfo = GetTermInfo(aveItem, aveItem.Fields);
                        if (termInfo != null && termInfo.UniqueId == Guid.Empty)
                        {
                            logger.Info($"Current item [{aveItem?.UniqueId}] termInfo is empty, try to get parent term info.");
                            var currentSetting = ResolveLifecycleSetting(listSetting);
                            if (currentSetting.IsInheritParentTerm)
                            {
                                logger.Info($"Current item [{aveItem?.UniqueId}] enable inherit parent term info.");
                                termInfo = parentTermInfo;
                                termInfo.IsInheritedTerm = true;
                            }
                        }
                        else
                        {
                            logger.Info($"Current item [{aveItem?.UniqueId}] termInfo is not empty, termInfo uniqueId is {termInfo?.UniqueId}");
                        }
                    }
                    RMRuleItemCollection rules = null;
                    SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
                    var key = aveItem.DirPath();
                    using (var performance = new PerformanceScope("RMSPExplorerProcessor.CheckParentRule", addToStatistics: true))
                    {
                        if (!CheckParentRule(key, ref itemRuleInfo))
                        {
                            if (RMSPExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                            {
                                var newRuleCollection = RebuldSPRules(rules);
                                if (newRuleCollection.Rules.Count == 0)
                                {
                                    logger.Info($"No SP rules realted to the item {aveItem?.UniqueId}");
                                }
                                else
                                {
                                    var filterEnginer = new RMSPRuleChecker(newRuleCollection);
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
                    Record item = syncItem.AssembleRecord(aveItem, parentId, itemRuleInfo, owner);
                    Record recordInDB = null;
                    using (var performance = new PerformanceScope("RMSPExplorerProcessor.GetDBRecord", addToStatistics: true))
                    {
                        WaitCosmosExecuteAction(() =>
                        {
                            recordInDB = ExplorerDao.ReadById(item.ScopeId, item.Id);
                        });
                    }
                    //check uniqueId
                    UpdateRecordId(item, recordInDB, uniqueIdUtil);
                    item.RemoveSyncFailedMetaInfo();
                    return SyncItemToDBAsync(item, recordInDB, recordInDB != null);
                }
            }
            catch (JobStopException)
            {
                throw new JobStopException("the job has stopped.");
            }
        }

        public virtual async System.Threading.Tasks.Task ProcessAveFolderAsync(IAveFolder folder, Guid parentId, SyncItemRuleInfo parentItemRule, UniqueIdUtil uniqueIdUtil)
        {
            try
            {
                using (var performance = new PerformanceScope("RMSPExplorerProcessor.ProcessAveFolder", $"RMSPExplorerProcessor.ProcessAveFolder:[{folder.Name}]", addToStatistics: true))
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
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
                            if (RMSPExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                            {
                                var newRuleCollection = RebuldSPRules(rules);
                                if (newRuleCollection.Rules.Count == 0)
                                {
                                    logger.Info($"No SP rules realted to the folder {folder?.ServerRelativeUrl}");
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
                            else if (parentItemRule != null && parentItemRule.Rule != null)
                            {
                                itemRuleInfo.Rule = parentItemRule.Rule;
                                itemRuleInfo.DisposalAction = parentItemRule.DisposalAction;
                            }

                        }
                        itemRuleInfo.TermInfo = termInfo;

                        var item = syncItem.AssembleRecord(folder, folder.UniqueId, itemRuleInfo);
                        Record recordInDB = null;
                        using (var performance0 = new PerformanceScope("RMSPExplorerProcessor.GetDBRecord", addToStatistics: true))
                        {
                            WaitCosmosExecuteAction(() =>
                            {
                                recordInDB = ExplorerDao.ReadById(item.ScopeId, item.Id);
                            });
                        }
                        //check uniqueId
                        UpdateRecordId(item, recordInDB, uniqueIdUtil);
                        await SyncItemToDBAsync(item, recordInDB);
                    }
                }
            }
            catch (JobStopException)
            {
                throw new JobStopException("the job has stopped.");
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


        protected string GetExceptionMessage(Exception e)
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
        [Obsolete]
        public virtual async System.Threading.Tasks.Task ProcessFolderAsync(IAveList aveList, AveDiscoverFolder discoverFolder, SyncItemRuleInfo parentItemRule)
        {
            try
            {
                using (var performance = new PerformanceScope("RMSPExplorerProcessor.ProcessFolder", $"RMSPExplorerProcessor.ProcessFolder:[{discoverFolder.LeafName}]", addToStatistics: true))
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
                        if (RMSPExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                        {
                            var newRuleCollection = RebuldSPRules(rules);
                            if (newRuleCollection.Rules.Count == 0)
                            {
                                logger.Info($"No SP rules realted to the folder {aveFolder?.ServerRelativeUrl}");
                            }
                            else
                            {
                                var filterEnginer = new RMSPRuleChecker(newRuleCollection);
                                itemRuleInfo = filterEnginer.CheckDisposalRule(discoverFolder, parentItemRule);

                            }

                        }
                        itemRuleInfo.TermInfo = termInfo;
                        var item = syncItem.AssembleRecord(discoverFolder, discoverFolder.DocID, itemRuleInfo);
                        await SyncItemToDBAsync(item);

                        string pagerInfo = string.Empty;
                        do
                        {
                            logger.Info($"Get items under [{discoverFolder.FullUrl}] with pager. PagerInfo:[{pagerInfo}]");
                            var items = this.mDiscover.GetItems(aveList, discoverFolder, ref pagerInfo);
                            await ProcessItemsAsync(aveList, items, discoverFolder.DocID, parentItemRule);
                        }
                        while (!string.IsNullOrEmpty(pagerInfo));

                        var folders = this.mDiscover.GetSubFolders(discoverFolder);
                        logger.Info($"Process folders under [{discoverFolder?.FullUrl}] Count:[{folders.LongCount()}]");
                        JobContext.ReportManager.IncreaseBase(folders.LongCount());
                        foreach (var folder in folders)
                        {
                            using (folder)
                            {
                                await ProcessFolderAsync(aveList, folder, itemRuleInfo);
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

        [Obsolete]
        public virtual async System.Threading.Tasks.Task ProcessItemAsync(IAveList list, AveDiscoverItem discoverItem, Guid parentId, SyncItemRuleInfo parentItemRule, CancellationTokenSource cts = null)
        {
            string itemName = string.Empty;
            string itemUrl = string.Empty;
            IAveListItem aveItem = null;
            try
            {
                using (var performance = new PerformanceScope("RMSPExplorerProcessor.ProcessItem", addToStatistics: true))
                {
                    JobContext.ReportManager.Increase();
                    var itemGuid = discoverItem.tp_GUID != Guid.Empty ? discoverItem.tp_GUID : discoverItem.DocID;
                    if (discoverItem.ID == null || (discoverItem.Hidden != null && discoverItem.Hidden == true))
                    {
                        logger.Info($"skip hidden item:{itemGuid}");
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

                            logger.Info($"cannot found item object ID:{discoverItem.ID} Guid:{itemGuid} :{ex.ToString()}");
                            RemoveSPObj(itemGuid, (int)discoverItem.ID);
                        }
                        logger.Warn("remove view item, {0}", itemGuid);
                        return;
                    }
                    WaitSPOExecuteAction(() =>
                    {
                        aveItem = list.GetItemById((int)discoverItem.ID);
                    });
                    if (!_isSyncStubFile && aveItem.IsStubItem())
                    {
                        logger.Debug($"Current item [{aveItem?.UniqueId}] is stub file, so skipped.");
                        return;
                    }
                    itemName = aveItem?.GetObjectName();
                    if (list.BaseType == AveBaseType.DocumentLibrary && NeedSkipFile(aveItem, itemName))
                    {
                        return;
                    }

                    itemUrl = aveItem.FullPath();
                    logger.Info($"Process item:{aveItem?.UniqueId}");
                    using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {

                        var termInfo = GetTermInfo(aveItem, aveItem.Fields);
                        RMRuleItemCollection rules = null;
                        SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
                        if (RMSPExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                        {
                            var newRuleCollection = RebuldSPRules(rules);
                            if (newRuleCollection.Rules.Count == 0)
                            {
                                logger.Info($"No SP rules realted to the item {aveItem?.UniqueId}");
                            }
                            else
                            {
                                var filterEnginer = new RMSPRuleChecker(newRuleCollection);
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

                        await SyncItemToDBAsync(item, recordInDB, recordInDB != null);


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
                logger.Error($"error occurred while Process aveitem:{aveItem?.UniqueId}, ERROR:{e.ToString()}");
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
                    //try
                    //{
                    //    WaitSPOExecuteAction(() =>
                    //    {
                    //        aveItem = list.GetItemById(itemId);
                    //    });
                    //}
                    //catch (Exception ex)
                    //{
                    //    logger.Info($"cannot found item object ID:{itemId} Guid:{itemUniqueId} :{ex.ToString()}");

                    RemoveSPObj(itemUniqueId, itemId);
                    //}
                    logger.Warn($"item no longer exist, remove record from explorer.ID:{itemId} Guid:{itemUniqueId}");
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

        public virtual async System.Threading.Tasks.Task ProcessIncrementalChangedItemV1Async(IAveList list, IAveListItem aveListItem, Guid parentId, SyncItemRuleInfo parentItemRule, UniqueIdUtil uniqueIdUtil, RMLifecycleSetting listSetting, RMTermInfo parentTermInfo, CancellationTokenSource cts = null)
        {
            string itemName = string.Empty;
            string itemUrl = string.Empty;
            var itemId = Guid.Empty;
            try
            {
                using (var performance = new PerformanceScope("RMSPExplorerProcessor.ProcessChangedItem", addToStatistics: true))
                {
                    JobContext.ReportManager.Increase();
                    IAveListItem aveItem = ReloadItemToGetBCSColumn(aveListItem);
                    itemName = aveItem?.GetObjectName();
                    if (list.BaseType == AveBaseType.DocumentLibrary && NeedSkipFile(aveItem, itemName))
                    {
                        return;
                    }

                    AvePoint.GCommon.Utility.ArgumentCheck.NotNull(aveItem, nameof(aveItem));
                    itemUrl = aveItem.FullPath();
                    itemId = aveItem.UniqueId;
                    if (aveItem.FileSystemObjectType == AveFileSystemObjectType.Folder)
                    {
                        IAveFolder folder = aveItem.Folder;
                        if (folder != null)
                        {
                            await ProcessAveFolderAsync(folder, parentId, parentItemRule, uniqueIdUtil);
                        }
                        logger.Info($"Current list item is folder.unque id : {itemId}, Id:{aveItem.ID}.");
                        return;
                    }
                    if (!_isSyncStubFile && aveItem.IsStubItem())
                    {
                        logger.Debug($"Current item [{aveItem?.UniqueId}] is stub file, so skipped.");
                        return;
                    }
                    if (ShouldSkipArchivedItem(aveItem))
                    {
                        RemoveSPObj(aveItem.UniqueId, aveItem.ID);
                        logger.Info($"Current item [{aveItem?.UniqueId}] is fully archived, so skipped in incremental sync.");
                        return;
                    }
                    using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        RMTermInfo termInfo;
                        using (var performance1 = new PerformanceScope("RMSPExplorerProcessor.GetTermInfo", addToStatistics: true))
                        {
                            termInfo = GetTermInfo(aveItem, aveItem.Fields);
                            if (termInfo != null && termInfo.UniqueId == Guid.Empty)
                            {
                                logger.Info($"Current item [{aveItem?.UniqueId}] termInfo is empty, try to get parent term info.");
                                var currentSetting = ResolveLifecycleSetting(listSetting);
                                if (currentSetting.IsInheritParentTerm)
                                {
                                    logger.Info($"Current item [{aveItem?.UniqueId}] enable inherit parent term info.");
                                    termInfo = parentTermInfo;
                                    termInfo.IsInheritedTerm = true;
                                }
                            }
                        }
                        RMRuleItemCollection rules = null;
                        SyncItemRuleInfo itemRuleInfo = new SyncItemRuleInfo();
                        using (var performance1 = new PerformanceScope("RMSPExplorerProcessor.CheckRule", addToStatistics: true))
                        {
                            if (RMSPExplorerDataCache.Instance.TermRuleMapping.TryGetValue(termInfo.UniqueId, out rules))
                            {
                                var newRuleCollection = RebuldSPRules(rules);
                                if (newRuleCollection.Rules.Count == 0)
                                {
                                    logger.Info($"No SP rules realted to the item {list.RootFolder.Url} Id:{aveItem.ID}");
                                }
                                else
                                {
                                    var filterEnginer = new RMSPRuleChecker(newRuleCollection);
                                    itemRuleInfo = filterEnginer.CheckDisposalRule(aveItem, parentItemRule);
                                }
                            }
                        }
                        itemRuleInfo.TermInfo = termInfo;
                        var item = syncItem.AssembleRecord(aveItem, parentId, itemRuleInfo);
                        Record recordInDB = null;

                        using (var performance1 = new PerformanceScope("RMSPExplorerProcessor.GetDBRecord", addToStatistics: true))
                        {
                            WaitCosmosExecuteAction(() =>
                            {
                                recordInDB = ExplorerDao.ReadById(item.ScopeId, item.Id);
                            });
                        }
                        //check uniqueId
                        UpdateRecordId(item, recordInDB, uniqueIdUtil);
                        item.RemoveSyncFailedMetaInfo();
                        await SyncItemToDBAsync(item, recordInDB, recordInDB != null);
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
                logger.Error($"error occurred while Process aveitem:{itemId}, ERROR:{e.ToString()}");
                bool isItemNotFound = this.isItemNotFoundError(e);
                if (!isItemNotFound)
                {
                    //Item 级别失败暂时不更新这两个标记
                    //JobContext.HasErrorNode = true;
                    //_siteCache.HasErrorNode = true;
                    JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                    {
                        ObjectName = itemName,
                        FullPath = itemUrl,
                        Status = JobDetailsStatus.Failed,
                        Comment = GetExceptionMessage(e),
                    });
                    this.AddFailureItem2Cache(aveListItem, parentId, e);
                }

            }
            return;
        }

        private async System.Threading.Tasks.Task ProcessFailedItemsAsync(IAveList list, SyncItemRuleInfo parentItemRule, RMLifecycleSetting listSetting, RMTermInfo parentTermInfo, List<SyncFailureItemEntity> failedItems = null)
        {
            logger.Info("Start to process failed items in azure table");
            if (failedItems == null)
            {
                failedItems = SyncFailureItemDao.GetAll(TenantLocalValue.LogonGroupId, DiscoverSite.SiteID.ToString(), list.ID.ToString());
            }
            int incItemsPerTask = failedItems.Count / 4;
            logger.Info($"Process last failed item count:[{failedItems.Count}].incItemsPerTask:[{incItemsPerTask}]");
            if (failedItems.Count > 0)
            {
                JobContext.ReportManager.IncreaseBase(failedItems.Count);

                UniqueIdUtil uniqueIdUtil;
                using (var performance = new PerformanceScope("RMSPExplorerProcessor.GenerateUniqueIds", addToStatistics: true))
                {
                    uniqueIdUtil = new UniqueIdUtil(TenantLocalValue.LogonGroupId, failedItems.Count);
                }

                var itemIds = failedItems.Select(i => i.ItemId).ToList();
                for (int i = 0; i < itemIds.Count; i += 2000)
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
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
                                ProcessFailedItemV1Async(list, changedItem, failedItems, parentItemRule, uniqueIdUtil, listSetting, parentTermInfo, cts).Wait();
                            });
                        }
                        else
                        {
                            foreach (var changedItem in items)
                            {
                                await ProcessFailedItemV1Async(list, changedItem, failedItems, parentItemRule, uniqueIdUtil, listSetting, parentTermInfo);
                            }
                        }
                    }

                }
            }
        }

        public virtual async System.Threading.Tasks.Task ProcessFailedItemV1Async(IAveList list, IAveListItem aveItem, List<SyncFailureItemEntity> failedItems, SyncItemRuleInfo parentItemRule, UniqueIdUtil uniqueIdUtil, RMLifecycleSetting listSetting, RMTermInfo parentTermInfo, CancellationTokenSource cts = null)
        {
            string itemName = string.Empty;
            string itemUrl = string.Empty; // string.Empty;
            var failedItem = failedItems.Where(f => f.ItemId == aveItem.ID).FirstOrDefault();
            try
            {
                itemUrl = failedItem?.FullPath;
                AvePoint.GCommon.Utility.ArgumentCheck.NotNull(failedItem, nameof(failedItem));
                Guid parentId = new Guid(failedItem.ParentId);
                using (var performance = new PerformanceScope("RMSPExplorerProcessor.ProcessFailedItem", addToStatistics: true))
                {
                    JobContext.ReportManager.Increase();
                    int itemId = failedItem.ItemId;
                    logger.Info($"Process failed item:Id:{itemId}, node id : {failedItem.NodeId}.");
                    itemName = aveItem?.GetObjectName();
                    itemUrl = aveItem.FullPath();
                    if (aveItem.FileSystemObjectType == AveFileSystemObjectType.Folder)
                    {
                        IAveFolder folder = aveItem.Folder;
                        if (folder != null)
                        {
                            await ProcessAveFolderAsync(folder, parentId, parentItemRule, uniqueIdUtil);
                        }
                        logger.Info($"Current list item is folder.Unique id:{aveItem?.UniqueId}.Id:{aveItem.ID}.");
                        this.RemoveFailureItemFromAzure(failedItem);
                        return;
                    }
                    if (!_isSyncStubFile && aveItem.IsStubItem())
                    {
                        logger.Debug($"Current item [{aveItem?.UniqueId}] is stub file, so skipped.");
                        return;
                    }
                    using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        var termInfo = GetTermInfo(aveItem, aveItem.Fields);
                        if (termInfo == null || termInfo.UniqueId == Guid.Empty)
                        {
                            logger.Info($"Current item [{aveItem?.UniqueId}] termInfo is empty, try to get parent term info.");
                            var currentSetting = ResolveLifecycleSetting(listSetting);
                            if (currentSetting.IsInheritParentTerm)
                            {
                                logger.Info($"Current item [{aveItem?.UniqueId}] enable inherit parent term info.");
                                termInfo = parentTermInfo;
                                termInfo.IsInheritedTerm = true;
                            }
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
                                    logger.Info($"No SP rules realted to the item {aveItem.UniqueId}:{itemId}");
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
                        itemRuleInfo.TermInfo = termInfo;
                        var item = syncItem.AssembleRecord(aveItem, parentId, itemRuleInfo);
                        Record recordInDB = null;

                        WaitCosmosExecuteAction(() =>
                        {
                            recordInDB = ExplorerDao.ReadById(item.ScopeId, item.Id);
                        });
                        //check uniqueId
                        UpdateRecordId(item, recordInDB, uniqueIdUtil);
                        item.AddSyncFailedMetaInfo();
                        //item.Comment = "RM_JM_SyncFailedItemSuccess";
                        await SyncItemToDBAsync(item, recordInDB, recordInDB != null);
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
                logger.Error($"error occurred while Process aveitem:{aveItem?.UniqueId}, ERROR:{e.ToString()}");
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

        public async System.Threading.Tasks.Task SyncItemToDBAsync(Record newItem, Record recordInDB = null, bool getDBRecord = true)
        {
            using (var performance = new PerformanceScope("RMSPExplorerProcessor.SyncItemToDB", addToStatistics: true))
            {
                bool result = false;
                newItem.ContainerId = containerId;
                if (recordInDB != null)
                {
                    newItem.ParentId = recordInDB.ParentId;
                }
                if (recordInDB != null && recordInDB.CheckExistAndTagDuplicateManual())
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
                    await ProcessSucceedRecord(newItem);
                }
                else
                {
                    logger.Warn($"skip to add record to db, the item already exist:{newItem?.Id}");
                }
            }
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
            JobContext.HasSuccessNode = true;
            var comment = newItem.Comment;
            var status = JobDetailsStatus.Successful;
            if (newItem.CustomColumnNotExist)
            {
                JobContext.HasErrorNode = true;
                status = JobDetailsStatus.Exception;
                comment = "RM_SPS_CustomColumnNotExist";
            }
            if (newItem.NodeType == (int)NodeLevel.Item || newItem.NodeType == (int)NodeLevel.Folder)
            {
                JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                {
                    ObjectName = newItem.LeafName,
                    FullPath = newItem.FullPath,
                    Status = status,
                    Comment = comment
                });
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

        private RMTermInfo GetTermInfo(IAvePropertyBag properties)
        {
            var termInfo = new RMTermInfo();

            if (properties.ContainsKey(RcordsBuiltInColumn.CONTAINER_BCS_NAME))
            {
                var termId = properties[RcordsBuiltInColumn.CONTAINER_BCS_NAME];
                if (termId != null)
                {
                    termInfo.UniqueId = new Guid(termId.ToString());
                    termInfo.Name = RMSPExplorerDataCache.Instance.Terms.ContainsKey(termInfo.UniqueId) ? RMSPExplorerDataCache.Instance.Terms[termInfo.UniqueId].Name : string.Empty;
                }
            }
            return termInfo;
        }

        private RMTermInfo GetTermInfo(Hashtable properties)
        {
            var termInfo = new RMTermInfo();

            if (properties.ContainsKey(RcordsBuiltInColumn.CONTAINER_BCS_NAME))
            {
                var termId = properties[RcordsBuiltInColumn.CONTAINER_BCS_NAME];
                if (termId != null)
                {
                    termInfo.UniqueId = new Guid(termId.ToString());
                    termInfo.Name = RMSPExplorerDataCache.Instance.Terms.ContainsKey(termInfo.UniqueId) ? RMSPExplorerDataCache.Instance.Terms[termInfo.UniqueId].Name : string.Empty;
                }
            }
            return termInfo;
        }

        private RMTermInfo GetTermInfo(IAveListItem item, IAveFieldCollection fields)
        {
            var termInfo = new RMTermInfo();
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
                        logger.Info($"{item.UniqueId} invalid term format:{valueString}");
                    }
                }
                else
                {
                    logger.Warn($"Item FieldValues do not contain BCS column or term is null. Item ID: [{item.ID}] Column Internal Name: [{_siteCache.BCSColumnInternalName}]");
                }
            }
            else
            {
                logger.Warn($"Item Fields do not contain BCS column. Item ID: [{item.ID}] Column Internal Name: [{_siteCache.BCSColumnInternalName}]");
            }
            return termInfo;
        }

        private IAveWeb GetOrAddWebFromCache(IAveSite site, Guid webId, Dictionary<Guid, IAveWeb> webCache, LinkedList<Guid> webCacheOrder, int webCacheLimit)
        {
            if (!webCache.TryGetValue(webId, out var web))
            {
                web = site.OpenWeb(webId);
                webCache[webId] = web;
                webCacheOrder.AddFirst(webId);
                EvictWebCacheIfNeeded(webCache, webCacheOrder, webCacheLimit);
                return web;
            }

            TouchWebCacheOrder(webCacheOrder, webId);
            return web;
        }

        private void TouchWebCacheOrder(LinkedList<Guid> webCacheOrder, Guid webId)
        {
            var node = webCacheOrder.Find(webId);
            if (node != null)
            {
                webCacheOrder.Remove(node);
                webCacheOrder.AddFirst(node);
            }
        }

        private void EvictWebCacheIfNeeded(Dictionary<Guid, IAveWeb> webCache, LinkedList<Guid> webCacheOrder, int webCacheLimit)
        {
            if (webCacheOrder.Count <= webCacheLimit)
            {
                return;
            }

            var evictId = webCacheOrder.Last.Value;
            webCacheOrder.RemoveLast();
            if (webCache.TryGetValue(evictId, out var evictWeb))
            {
                webCache.Remove(evictId);
                evictWeb?.Dispose();
            }
        }

        private void DisposeWebCache(Dictionary<Guid, IAveWeb> webCache, LinkedList<Guid> webCacheOrder)
        {
            foreach (var cachedWeb in webCache.Values)
            {
                cachedWeb?.Dispose();
            }
            webCache.Clear();
            webCacheOrder.Clear();
        }

        /// <summary>
        /// for this now ,incremental logic not support container level rule change.... to do next...
        /// </summary>
        /// <param name="site"></param>
        public void ProcessTermChangedItems(long lastScanTime)
        {
            IAveSite site = DiscoverSite.Site;
            bool startSuccess = false;
            const int webCacheLimit = 20;
            Dictionary<Guid, IAveWeb> webCache = new Dictionary<Guid, IAveWeb>();
            LinkedList<Guid> webCacheOrder = new LinkedList<Guid>();
            try
            {
                var ChangeTermIds = GetChangedTermIds(lastScanTime);
                if (ChangeTermIds.Count > 0)
                {
                    logger.Info($"Total changed term count: {ChangeTermIds.Count}");
                }

                bool hasAnyRecords = false;
                if (_isCosmosBulkOperationEnabled)
                {
                    var RMKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
                    var bulkSize = RMKeyValueDao.GetCosmosBulkInsertOperationBufferSize();
                    if (bulkSize == default(int)) bulkSize = CosmosBulkOperator.DefualtBufferSize;
                    logger.Info($"Cosmos bulk operation enabled, bulk size: {bulkSize}");
                    CosmosBulkOperator.Instance.Start(bulkSize, ProcessSucceedRecord, ProcessFailedRecord);
                    startSuccess = true;
                }

                const int termBatchSize = 100;
                const int recordPageSize = 10000;
                for (int i = 0; i < ChangeTermIds.Count; i += termBatchSize)
                {
                    using (CheckJobStopScope jScopeChangeTermIds = new CheckJobStopScope())
                    {
                        var tempIds = ChangeTermIds.Skip(i).Take(termBatchSize).ToList();
                        logger.Info($"Query changed term from {i} to {i + termBatchSize}");

                        string continuation = string.Empty;
                        do
                        {
                            var pageResult = ExplorerDao.QueryByPage(
                                r => tempIds.Contains(r.TermId)
                                    && r.ScopeId == site.ID
                                    && r.RecordStatus == (int)RMRecordStatus.Active
                                    && (r.NodeType == (int)NodeLevel.Item || r.NodeType == (int)NodeLevel.Folder)
                                    && r.CollectTime < JobContext.JobStartTime.Ticks,
                                recordPageSize,
                                continuation);

                            var pageRecords = pageResult.Item1?.ToList() ?? new List<Record>();
                            continuation = pageResult.Item2;

                            if (pageRecords.Count == 0)
                            {
                                continue;
                            }

                            hasAnyRecords = true;
                            Dictionary<Guid, List<Record>> webObjs = pageRecords.GroupBy(r => r.WebId).ToDictionary(g => g.Key, p => p.ToList());
                            IAveWeb web = null;
                            IAveList list = null;
                            JobContext.ReportManager.IncreaseBase(webObjs.Count);
                            foreach (var webId in webObjs.Keys)
                            {
                                try
                                {
                                    if (web == null || !web.ID.Equals(webId))
                                    {
                                        web = GetOrAddWebFromCache(site, webId, webCache, webCacheOrder, webCacheLimit);

                                        logger.Info("Process classification change web {0}", web.Url);
                                    }
                                    var listNodes = webObjs[webId].GroupBy(t => t.ListId).ToDictionary(g => g.Key, p => p.ToList());

                                    foreach (var listId in listNodes.Keys)
                                    {
                                        try
                                        {
                                            using (CheckJobStopScope jScopeNodes = new CheckJobStopScope())
                                            {
                                                if (list == null || !list.ID.Equals(listId))
                                                {
                                                    list = web.GetList(listId);
                                                    logger.Info("Process classification change list {0}, {1}", list.RootFolder.Url, list.ID);
                                                }

                                                var records = listNodes[listId];

                                                //老数据folder在cosmosdb中没有有itemRowId，所有需要单独处理，同时处理过后会赋值itemRowId
                                                ProcessOldFolders(list, records);

                                                var itemIntIds = records.Where(o => o.ItemRowId != 0).Select(i => i.ItemRowId).ToList();
                                                for (int j = 0; j < itemIntIds.Count; j += 2000)
                                                {
                                                    using (CheckJobStopScope jScopeIds = new CheckJobStopScope())
                                                    {
                                                        var rowIds = itemIntIds.Skip(j).Take(2000).ToList();
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
                                                                using (CheckJobStopScope jScope = new CheckJobStopScope())
                                                                {
                                                                    RealProcessTermChangeItem(changedItem, records);
                                                                }
                                                            }
                                                        }
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
                        } while (!string.IsNullOrEmpty(continuation));
                    }
                }

                if (!hasAnyRecords)
                {
                    logger.Info("No Incremental Classification change records in site  {0}", site.Url);
                    return;
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
                DisposeWebCache(webCache, webCacheOrder);

                if (_isCosmosBulkOperationEnabled && startSuccess)
                {
                    CosmosBulkOperator.Instance.Complete();
                    CosmosBulkOperator.Instance.Reset();
                }
            }
        }

        public void ProcessOldFolders(IAveList list, List<Record> records)
        {
            try
            {
                var folderLevel = (int)NodeLevel.Folder;
                var totalFolderRecords = records.Where(o => o.NodeType == folderLevel && o.ItemRowId == 0).ToList();
                for (int i = 0; i < totalFolderRecords.Count; i += 2000)
                {
                    using (CheckJobStopScope jScopeAll = new CheckJobStopScope())
                    {
                        var folderRecords = totalFolderRecords.Skip(i).Take(2000).ToList();
                        var folderItems = new List<IAveListItem>();
                        foreach (var folder in folderRecords)
                        {
                            using (var performance = new PerformanceScope("RMSPExplorerBase.GetFolderForChangeTerm", addToStatistics: true))
                            {
                                var item = list.GetFolder(folder.DirPath)?.Item;
                                if (item != null)
                                {
                                    folderItems.Add(item);
                                }
                            }
                        }
                        int existingItemsPerTask = folderItems.Count / 4;
                        CancellationTokenSource cts = null;
                        if (folderItems.Count > itemsPerTask)
                        {
                            cts = new CancellationTokenSource();
                            AveTenantTasks.RunParallel(folderItems, existingItemsPerTask, cts, changedItem =>
                            {
                                RealProcessTermChangeItem(changedItem, records, cts);
                            });
                        }
                        else
                        {
                            foreach (var changedItem in folderItems)
                            {
                                using (CheckJobStopScope jScope = new CheckJobStopScope())
                                {
                                    RealProcessTermChangeItem(changedItem, totalFolderRecords);
                                }
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
        public async System.Threading.Tasks.Task FailedItemsInSiteAsync()
        {
            IAveSite site = DiscoverSite.Site;
            if (!_siteCache.HasErrorNode)
            {
                //需要插入Flag 或者更新Flag中的时间
                if (FailureItems.Count >= 1000)
                {
                    logger.Info("More than 1000 failed items in site {0}, count {2}", site.Url, FailureItems.Count);
                    //failure 数量大于 1000， 不插入Azure Table， 
                    JobContext.HasErrorNode = true;
                    _siteCache.HasErrorNode = true;
                }
                else
                {
                    logger.Info("Failed items count{0}, in site {1}", FailureItems.Count, site.Url);
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
                logger.Info("Has error container in site {0}, ignore the fail items, count {1}", site.Url, FailureItems.Count);
            }
            List<SyncFailureItemEntity> failedItems = SyncFailureItemDao.GetAll(TenantLocalValue.LogonGroupId, DiscoverSite.SiteID.ToString());
            List<Guid> currentJobFailedItemIds = FailureItems.Select(i => new Guid(i.ItemId)).ToList();
            failedItems = failedItems.Where(i => !currentJobFailedItemIds.Contains(new Guid(i.RowKey))).ToList();
            if (failedItems.Count == 0)
            {
                return;
            }

            bool startSuccess = false;
            try
            {
                if (_isCosmosBulkOperationEnabled)
                {
                    var RMKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
                    var bulkSize = RMKeyValueDao.GetCosmosBulkInsertOperationBufferSize();
                    if (bulkSize == default(int)) bulkSize = CosmosBulkOperator.DefualtBufferSize;
                    logger.Info($"Cosmos bulk operation enabled, bulk size: {bulkSize}");
                    CosmosBulkOperator.Instance.Start(bulkSize, ProcessSucceedRecord, ProcessFailedRecord);
                    startSuccess = true;
                }
                Dictionary<string, List<SyncFailureItemEntity>> webObjs = failedItems.GroupBy(r => r.WebId).ToDictionary(g => g.Key, p => p.ToList());
                IAveWeb web = null;
                IAveList list = null;
                JobContext.ReportManager.IncreaseBase(webObjs.Count);
                foreach (var webId in webObjs.Keys)
                {
                    try
                    {
                        if (web == null || !web.ID.Equals(new Guid(webId)))
                        {
                            web = site.OpenWeb(new Guid(webId));
                            logger.Info("Process failed item for web {0}", web.Url);
                        }
                        var listNodes = webObjs[webId].GroupBy(t => t.ListId).ToDictionary(g => g.Key, p => p.ToList());
                        foreach (var listId in listNodes.Keys)
                        {
                            try
                            {
                                if (list == null || !list.ID.Equals(new Guid(listId)))
                                {
                                    list = web.GetList(new Guid(listId));
                                    logger.Info("Process failed item for list {0}, {1}", list.RootFolder.Url, list.ID);
                                }
                                var parentTermInfo = new RMTermInfo();
                                var listPath = WebUtil.MakeFullUrl(list.ParentWeb.Url, list.RootFolder.Url);
                                var listSetting = GetInheritedSetting(list.ID, listPath);
                                if (listSetting == null)
                                {
                                    logger.Warn($"List setting is null, list:{listPath}");
                                    var webSetting = GetInheritedSetting(web.ID, web.Url);
                                    listSetting = ResolveLifecycleSetting(webSetting);
                                }
                                if (listSetting != null && !listSetting.IsInheritParentTerm)
                                {
                                    parentTermInfo = GetParentTermInfo(site, web, list);
                                }
                                var listFailedItems = listNodes[listId.ToString()];
                                await ProcessFailedItemsAsync(list, null, listSetting, parentTermInfo, listFailedItems);
                            }
                            catch (JobStopException)
                            {
                                throw new JobStopException("This Job is stopped.");
                            }
                            catch (Exception le)
                            {
                                logger.Warn("Process failed item for list error {0}:{1}", listId, le.ToString());

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
                logger.Error($"error occurred while Process failed item in site:{site.Url}, ERROR:{e.ToString()}");
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
                    CosmosBulkOperator.Instance.Reset();
                }
            }
        }

        private RMTermInfo GetParentTermInfo(IAveSite site, IAveWeb web, IAveList list)
        {
            if (list != null)
            {
                try
                {
                    var listTerm = GetTermInfo(list.RootFolder.Properties);
                    if (listTerm != null && listTerm.UniqueId != Guid.Empty)
                    {
                        return listTerm;
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn($"GetParentTermInfo: read list term failed, list:{list?.ID}, error:{ex.Message}");
                }
            }

            if (web != null)
            {
                try
                {
                    var webTerm = GetTermInfo(web.Properties);
                    if (webTerm != null && webTerm.UniqueId != Guid.Empty)
                    {
                        return webTerm;
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn($"GetParentTermInfo: read web term failed, web:{web?.ID}, error:{ex.Message}");
                }

                try
                {
                    var ancestorTerm = ResolveAncestorTerm(web, new RMTermInfo());
                    if (ancestorTerm != null && ancestorTerm.UniqueId != Guid.Empty)
                    {
                        return ancestorTerm;
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn($"GetParentTermInfo: resolve ancestor web term failed, start web:{web?.ID}, error:{ex.Message}");
                }
            }

            if (site != null)
            {
                try
                {
                    var siteTerm = GetTermInfo(site.RootWeb.Properties);
                    if (siteTerm != null)
                    {
                        return siteTerm;
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn($"GetParentTermInfo: read site term failed, site:{site?.Url}, error:{ex.Message}");
                }
            }

            return new RMTermInfo();
        }

        private void RealProcessTermChangeItem(IAveListItem item, List<Record> itemNodes, CancellationTokenSource cts = null)
        {
            using (var performance = new PerformanceScope("RMSPExplorerBase.RealProcessTermChangeItem", addToStatistics: true))
            {
                string itemName = string.Empty, itemUrl = string.Empty;
                var itemNode = itemNodes.Where(i => i.NodeId == item.UniqueId).FirstOrDefault();
                if (itemNode == null)
                {
                    return;
                }
                try
                {
                    itemName = item?.GetObjectName();
                    if (NeedSkipFile(item, itemName))
                    {
                        return;
                    }
                    if (ShouldSkipArchivedItem(item))
                    {
                        RemoveSPObj(item.UniqueId, item.ID);
                        logger.Info($"Current item [{item?.UniqueId}] is fully archived, so skipped in term change sync.");
                        return;
                    }
                    itemUrl = item.FullPath();
                    using (CheckJobStopScope stopScope = new CheckJobStopScope())
                    {
                        logger.Info($"Process classification change item {item?.ID},unqiue id {item?.UniqueId}");
                        RMRuleItemCollection rules = null;
                        var termInfo = new RMTermInfo()
                        {
                            Name = itemNode.TermName,
                            UniqueId = itemNode.TermId
                        };
                        SyncItemRuleInfo ruleInfo = new SyncItemRuleInfo();
                        using (var performance0 = new PerformanceScope("RMSPExplorerBase.CheckRule", addToStatistics: true))
                        {
                            if (RMSPExplorerDataCache.Instance.TermRuleMapping.TryGetValue(itemNode.TermId, out rules))
                            {
                                var newRuleCollection = RebuldSPRules(rules);
                                if (newRuleCollection.Rules.Count == 0)
                                {
                                    logger.Info($"No SP rules realted to the item {itemNode?.Id}");
                                }
                                else
                                {
                                    var filterEnginer = new RMSPRuleChecker(newRuleCollection);
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
                        using (var performance0 = new PerformanceScope("RMSPExplorerBase.UpdateItem", addToStatistics: true))
                        {
                            if (ruleInfo.Rule != null && (itemNode.RuleLevel == 0 || itemNode.RuleLevel >= 32 || itemNode.RuleLevel == 16))
                            {
                                logger.Info($"swith item rule: {itemNode?.Id}, {itemNode.RuleId} 2 {ruleInfo.Rule?.Id}.");
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
                                if (itemNode.ItemRowId == 0)
                                {
                                    itemNode.ItemRowId = item.ID;
                                }
                                itemNode.RemoveSyncFailedMetaInfo();
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
                            else if (itemNode.RuleLevel == (int)GCommon.Contract.CommonFilter.PolicyLevel.Document || itemNode.RuleLevel == (int)GCommon.Contract.CommonFilter.PolicyLevel.Item || itemNode.RuleLevel == (int)GCommon.Contract.CommonFilter.PolicyLevel.List || itemNode.RuleLevel == (int)GCommon.Contract.CommonFilter.PolicyLevel.Folder)
                            {
                                logger.Info("Empty the item rule {0}", itemNode?.Id);
                                #region empty item rule
                                itemNode.RuleId = Guid.Empty;
                                itemNode.RuleLevel = 0;
                                itemNode.RecordOwner = string.Empty;
                                itemNode.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(string.Empty);
                                itemNode.PreviosDisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(string.Empty);
                                if (itemNode.ItemRowId == 0)
                                {
                                    itemNode.ItemRowId = item.ID;
                                }
                                itemNode.RemoveSyncFailedMetaInfo();
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
                                logger.Info("No change item {0}", itemNode?.Id);
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
                    logger.Error("Process classification item error. Item id: {0}, Item unique id: {1}, ERROR:{2}", itemNode?.Id, itemNode?.ItemId, e.ToString());
                    bool isItemNotFound = this.isItemNotFoundError(e);
                    if (!isItemNotFound)
                    {
                        JobContext.HasErrorNode = true;
                        //_siteCache.HasErrorNode = true;
                        JobContext.ReportManager.SendJobDetail(new JMCollectionDataJobDetails()
                        {
                            ObjectName = itemName,
                            FullPath = itemUrl,
                            Status = JobDetailsStatus.Failed,
                            Comment = GetExceptionMessage(e),
                        });

                        this.AddFailureItem2Cache(item, itemNode.ParentId, e);
                    }
                }
                finally
                {
                    JobContext.ReportManager.Increase();
                }
            }
        }

        private bool isItemNotFoundError(Exception e)
        {
            if (e != null && e.Message != null && e.Message.Contains("Item does not exist"))
            {
                return true;
            }
            if (e != null && e.InnerException != null)
            {
                return isItemNotFoundError(e.InnerException);
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
            using (new RA.Common.PerformanceScope("RMSPExplorerProcessor.UpdateRecordId", string.Format("RMSPExplorerProcessor.UpdateRecordId:List {0} RowId{1}", recoEntity.ListId, recoEntity.ItemRowId), true))
            {
                if (string.IsNullOrEmpty(recordsGlobalId))
                {
                    recordsGlobalId = recordInDB?.RecordsId;

                    if (string.IsNullOrEmpty(recordsGlobalId))
                    {
                        logger.Info("create new unique List {0} Id:{1}", recoEntity.ListId, recoEntity?.Id);
                        recordsGlobalId = idUtil.GenerateUniqueId();
                        //UniqueIdGenerator.GenerateUniqueId(RMSPExplorerDataCache.Instance.UniqueIdSetting);

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
                logger.Info("need skip file check rule action {0}:{1}", item?.UniqueId, ext);
                return true;
            }
            if (objectName.EndsWith("aspx") && !RMSPExplorerDataCache.Instance.ArchiverSettings.IsDeleteLinkFile)
            {
                logger.Info("need skip file check rule action maybe stub file. {0}:{1}", item?.UniqueId, ext);
                return true;
            }
            return false;
        }

        private static bool ShouldSkipArchivedItem(IAveListItem item)
        {
            return item != null
                && item.FieldValues != null
                && item.FieldValues.ContainsKey("_FileArchiveStatus")
                && item.FieldValues["_FileArchiveStatus"] != null
                && !string.IsNullOrEmpty(item.FieldValues["_FileArchiveStatus"].ToString());
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
                    if (_siteCache.BCSColumnInternalName != RcordsBuiltInColumn.ITEM_BCS_NAME)
                    {
                        //existing column reset internal name
                        var bcsColumn = list.Fields.GetFieldById(_siteCache.BCSColumnID, false);
                        if (bcsColumn != null)
                        {
                            _siteCache.BCSColumnInternalName = bcsColumn.InternalName;
                            logger.Info($"reset list bcs column, list:{list.RootFolder?.ServerRelativeUrl}, column name:{_siteCache.BCSColumnInternalName}");
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
            var recId = IDGenerator.GetRecordId(siteId, objectId);
            Record removeRecordInDB = null;
            WaitCosmosExecuteAction(() =>
            {
                removeRecordInDB = ExplorerDao.ReadById(siteId, recId);
            });
            if (removeRecordInDB != null)
            {
                int associatedCount = GetAssociatedRecordCount(removeRecordInDB);
                logger.Debug($"get {removeRecordInDB.Id} removed items related count:{associatedCount}");
                if (removeRecordInDB.RecordStatus == (int)RMRecordStatus.Active)
                {
                    ExplorerDao.UpdateRecordState(removeRecordInDB, (int)RMRecordStatus.RMDeleted);
                    logger.Info("update record state to 3,siteId: {0}, objId: {1}, itemId: {2}", siteId, objectId, itemRowId);

                }
                else
                {
                    logger.Warn("sp object already archived,siteId:{0}, objId:{1}, itemId:{2}", siteId, objectId, itemRowId);
                }
            }
        }

        private int GetAssociatedRecordCount(Record rec)
        {
            int totalCount = 0;
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
                        logger.Debug($"Get removed folder with 'StartsWith'. Folder Count:{tempFolderIds.Count}");

                        if (tempFolderIds.Count == 0)
                        {
                            return totalCount;
                        }

                        const int folderIdBatchSize = 1000;
                        for (int i = 0; i < tempFolderIds.Count; i += folderIdBatchSize)
                        {
                            var batchFolderIds = tempFolderIds.Skip(i).Take(folderIdBatchSize).ToList();
                            var batchResults = ExplorerDao.GetFilterList(
                                a => new Record
                                {
                                    Id = a.Id,
                                    TermId = a.TermId,
                                    ScopeId = a.ScopeId,
                                    RecordStatus = a.RecordStatus,
                                    DestroyedTime = a.DestroyedTime
                                },
                                s => s.ScopeId == rec.ScopeId
                                    && s.WebId == rec.WebId
                                    && s.ListId == rec.ListId
                                    && s.NodeType == (int)NodeLevel.Item
                                    && batchFolderIds.Contains(s.FolderId));

                            if (batchResults != null)
                            {
                                totalCount += batchResults.Count;
                            }
                        }
                        return totalCount;
                    case (int)NodeLevel.Item:
                        return 1;
                    default:
                        logger.Warn($"node type not supported:{rec.NodeType}, {rec.Id}");
                        break;
                }
                if (lambda != null)
                {
                    var resultList = ExplorerDao.GetFilterList(a => new Record { Id = a.Id, TermId = a.TermId, ScopeId = a.ScopeId, RecordStatus = a.RecordStatus, DestroyedTime = a.DestroyedTime }, lambda);
                    if (resultList != null)
                    {
                        totalCount = resultList.Count;
                    }
                }

            }
            return totalCount;
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
            catch (Exception e)
            {
                logger.Error($"Error occurred while adding site scope. Error:{e.ToString()}");
            }
        }

        private RMLifecycleSetting ResolveLifecycleSetting(RMLifecycleSetting listSetting, RMLifecycleSetting webSetting = null)
        {
            if (listSetting != null)
                return listSetting;
            if (webSetting != null)
                return webSetting;
            if (SiteSetting != null)
                return SiteSetting;
            if (TeamsSetting != null)
                return TeamsSetting;
            return GroupSetting;
        }

        private static bool IsDocumentLibrary(IAveList list)
        {
            return list?.BaseType == AveBaseType.DocumentLibrary ||
                list?.BaseTemplate == AveListTemplateType.DocumentLibrary;
        }

        private static bool IsGenericList(IAveList list)
        {
            return !IsDocumentLibrary(list) &&
                (list?.BaseType == AveBaseType.GenericList ||
                 list?.RootFolder?.ServerRelativeUrl?.Contains("/Lists/", StringComparison.OrdinalIgnoreCase) == true);
        }

        private static bool ShouldSkipList(IAveList list, RMLifecycleSetting setting)
        {
            return IsGenericList(list) && !(setting?.EnableLifecycleManagementForSharePointLists ?? true);
        }

        private async Task PersistSkippedListAsync(AveDiscoverList discoverList)
        {
            _siteCache.HasSkippedLifecycleList = true;
            var skippedListRecord = syncItem.AssembleRecord(discoverList, new SyncItemRuleInfo());
            await SyncItemToDBAsync(skippedListRecord, null);
        }
    }
}
