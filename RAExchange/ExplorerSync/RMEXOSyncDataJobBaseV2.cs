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
using AvePoint.RA.Common.Lock;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.Contract.SharePoint.CustomIndexMetadata;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Bulk;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.UniqueId;
using AvePoint.RA.RAExchange.Common;
using AvePoint.RA.RAExchange.Discover;
using AvePoint.RA.RAExchange.Discover.DiscoverImplV2;
using AvePoint.RA.RAExchange.Disposal.Common;
using AvePoint.RA.RAExchange.ExplorerSync;
using AvePoint.Records.Core.Utilities.Extensions;
using AvePoint.Wrapper.Common;
using ExchangeBackupUtility.Graph;
using ExchangeCommonWrapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArgumentCheck = AvePoint.GCommon.Utility.ArgumentCheck;

namespace AvePoint.RA.RAExchange.RMCollectionData
{
    public class RMEXOSyncDataJobBaseV2 : RMEXODiscoverBaseV2, IEXOSyncDataJob
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMEXOSyncDataJobBaseV2));

        protected RuleManagement RuleManagement = null;
        protected Guid GroupId = Guid.Empty;
        protected Guid AOSMailboxId = Guid.Empty;

        protected JobManagement JobManagement = null;
        private IBatchDiscoverV2 discover = null;
        protected bool useBatchUpdate = false;
        private bool _isCosmosBulkOperationEnabled = false; //是否开启了批量插入数据到cosmos db
        protected List<Record> cacheList = new List<Record>();
        //这个属性记录了job 开始时间，用来做change term 逻辑，SP 逻辑就是这样。 PS 能不能先处理change term，那样就不用记录时间了?
        private DateTime JobStartTime = DateTime.MinValue;

        private List<Rule> allRulesList = null;
        private Dictionary<Guid, RMRuleItemCollection> TermAndRulesMapping;
        private Dictionary<Guid, string> ReviewedUserIdsAndNodeIdMapping;
        private Dictionary<Guid, string> TermIdAndNameMapping;
        private RMUniqueIdSetting uniqueIdSetting = null;
        private static MemoryLocker _memoryLocker = new MemoryLocker();
        private string containerId = string.Empty;
        private Dictionary<Guid, List<RMEXOSyncFailureItem>> FailureItems = new Dictionary<Guid, List<RMEXOSyncFailureItem>>();
        private Dictionary<Guid, EXONodeFlag> folderFlags = new Dictionary<Guid, EXONodeFlag>();
        private Dictionary<Guid, IExchangeFolder> mSubFolders = [];
        private Dictionary<Guid, RMRule> mRuleCache = new Dictionary<Guid, RMRule>();
        private readonly object mLock = new object();
        private int itemsPerTask = 100;

        public ISyncFailureItemDao SyncFailureItemDao { set; get; } = PlatformWindsorManager.GetService<ISyncFailureItemDao>();
        private IEXONodeFlagDao mEXONodeInfoDao;

        // Index metadata column
        private List<RMCustomIndexMetadata> _customIndexMetadatas = new List<RMCustomIndexMetadata>();
        private List<RMCustomMetadataColumn> _customMetadataColumns = new List<RMCustomMetadataColumn>();

        protected static readonly IRMCustomIndexMetadataDao _customIndexMetadataDao = PlatformWindsorManager.GetService<IRMCustomIndexMetadataDao>();
        protected static readonly IRMCustomMetadataColumnDao _customMetadataColumnDao = PlatformWindsorManager.GetService<IRMCustomMetadataColumnDao>();

        protected static readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        protected IEXONodeFlagDao EXONodeInfoDao
        {
            get
            {
                if (mEXONodeInfoDao == null)
                {
                    mEXONodeInfoDao = new EXONodeFlagDao();
                }
                return mEXONodeInfoDao;
            }
        }

        private IExplorerQueryService mExplorerQueryService;
        public IExplorerQueryService ExplorerQueryService
        {
            get
            {
                if (mExplorerQueryService == null)
                {
                    mExplorerQueryService = (IExplorerQueryService)PlatformWindsorManager.GetService(typeof(IExplorerQueryService));
                }
                return mExplorerQueryService;
            }
        }

        private IExplorerDao _explorerDao;
        protected IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new ExplorerDao(true);
                }
                return _explorerDao;
            }
        }

        private ITermRuleAssociationDao termRuleAssociationDao;
        protected ITermRuleAssociationDao TermRuleInfos
        {
            get
            {
                if (termRuleAssociationDao == null)
                {
                    termRuleAssociationDao = new TermRuleAssociationDao();
                }
                return termRuleAssociationDao;
            }
        }

        private ITermDao mTermDao;
        protected ITermDao TermDao
        {
            get
            {
                if (mTermDao == null)
                {
                    mTermDao = new TermDao();
                }
                return mTermDao;
            }
        }

        private IRuleManagerService mRuleManagerService;
        public IRuleManagerService RuleManagerService
        {
            get
            {
                if (mRuleManagerService == null)
                {
                    mRuleManagerService = (IRuleManagerService)PlatformWindsorManager.GetService(typeof(IRuleManagerService));
                }
                return mRuleManagerService;
            }
        }

        private IExplorerService mExplorerService;
        protected IExplorerService ExplorerService
        {
            get
            {
                if (mExplorerService == null)
                {
                    mExplorerService = (IExplorerService)PlatformWindsorManager.GetService(typeof(IExplorerService));
                }
                return mExplorerService;
            }
        }

        private IRMManualApproveDao mManualApproveDao;

        protected IRMManualApproveDao ManualApproveDao
        {
            get
            {
                if (mManualApproveDao == null)
                {
                    mManualApproveDao = (IRMManualApproveDao)PlatformWindsorManager.GetService(typeof(IRMManualApproveDao));
                }
                return mManualApproveDao;
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

        private IRMBoardCacheDao mBoardCacheDao;
        protected IRMBoardCacheDao BoardCacheDao
        {
            get
            {
                if (mBoardCacheDao == null)
                {
                    mBoardCacheDao = (IRMBoardCacheDao)PlatformWindsorManager.GetService(typeof(IRMBoardCacheDao));
                }
                return mBoardCacheDao;
            }

        }

        private IRMClassificationHistoryDao mClassificationHistoryDao;
        protected IRMClassificationHistoryDao ClassificationHistoryDao
        {
            get
            {
                if (mClassificationHistoryDao == null)
                {
                    mClassificationHistoryDao = (IRMClassificationHistoryDao)PlatformWindsorManager.GetService(typeof(IRMClassificationHistoryDao));
                }
                return mClassificationHistoryDao;
            }

        }

        private IRMRuleDao mRuleDao;
        public IRMRuleDao RuleDao
        {
            get { return mRuleDao ?? (IRMRuleDao)PlatformWindsorManager.GetService(typeof(IRMRuleDao)); }
            set { mRuleDao = value; }
        }

        private SemaphoreSlim _semaphore = new SemaphoreSlim(3, 3);

        public RMEXOSyncDataJobBaseV2(ExchangeOnlineTreeNodeDto treeNode, JobManagement jobManagement)
            : base(treeNode)
        {
            JobManagement = jobManagement;
            JobStartTime = DateTime.UtcNow;
            containerId = GetGroupNode(treeNode).ID;
        }

        private void InitCosmosBulkOperation()
        {
            var RMKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
            _isCosmosBulkOperationEnabled = RMKeyValueDao.IsCosmosBulkOperationEnabled();
            if (_isCosmosBulkOperationEnabled)
            {
                var bulkSize = RMKeyValueDao.GetCosmosBulkInsertOperationBufferSize();
                if (bulkSize == default(int)) bulkSize = CosmosBulkOperator.DefualtBufferSize;
                logger.Info($"Cosmos bulk operation enabled, bulk size: {bulkSize}");
                CosmosBulkOperator.Instance.Start(bulkSize, ProcessSucceedRecordAsync, ProcessFailedRecord);
            }
        }

        public void SetDiscoverObject(IBatchDiscoverV2 discover)
        {
            this.discover = discover;
        }

        public virtual async System.Threading.Tasks.Task RunNowAsync()
        {
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    Init();
                    InitCosmosBulkOperation();
                    await LoadCustomIndexMetadataAsync();
                    using (var performance = new PerformanceScope("EXO.RMEXODataSync.GetRulesFromRecords", "", true))
                    {
                        allRulesList = RuleManagerService.GetRulesFromRecords();
                    }
                    using (var performance = new PerformanceScope("EXO.RMEXODataSync.GetTermAndRuleMappings", "", true))
                    {
                        TermAndRulesMapping = GetTermAndRuleMappings();
                    }
                    using (var performance = new PerformanceScope("EXO.RMEXODataSync.GetTermIdAndNameMapping", "", true))
                    {
                        TermIdAndNameMapping = TermDao.GetExistingTermIdAndNameMapping();
                    }
                    using (var performance = new PerformanceScope("EXO.RMEXODataSync.ReviewSetting", "", true))
                    {
                        ReviewedUserIdsAndNodeIdMapping = ManualApproveDao.GetLastReviewedUserIdsByScope(AOSMailboxId);
                    }
                    using (var performance = new PerformanceScope("EXO.RMEXODataSync.GetRulesWithoutRemoved", "", true))
                    {
                        mRuleCache = (await RuleDao.GetRulesWithoutRemovedAsync()).ToDictionary(r => r.RuleId);
                    }
                    var mUniqueIdSettingDao = (IUniqueIdSettingDao)PlatformWindsorManager.GetService(typeof(IUniqueIdSettingDao));
                    uniqueIdSetting = mUniqueIdSettingDao.LoadingUniqueIdSetting();
                    //跑Job之前，就获取一下当前mailbox 的上次sync 时间，这样才能正确处理Term Rule 变化的case
                    DateTime collectionTime = DateTime.MinValue;
                    collectionTime = GetStartDate(TreeManagement.GetMailboxNode(TreeNodeDto).ID);
                    if (collectionTime != DateTime.MinValue)
                    {
                        logger.Info("Start to process failed items.");
                        await ProcessFailedItemsAsync(CurrentFolder);
                    }
                    await ProcessFolderAsync(CurrentFolder);
                    if (_isCosmosBulkOperationEnabled)
                    {
                        CosmosBulkOperator.Instance.Complete();
                        CosmosBulkOperator.Instance.Reset();
                    }
                    await ProcessChangedTermAsync(CurrentFolder, collectionTime.Ticks);

                    //同步完records后最后更新失败的item
                    foreach (var nodeCache in folderFlags)
                    {
                        var folderId = nodeCache.Key;
                        var currentFolderFailedItems = FailureItems.ContainsKey(folderId) ? FailureItems[folderId] : new List<RMEXOSyncFailureItem>();
                        if (currentFolderFailedItems.Count >= 200)
                        {
                            logger.Info("More than 200 failed items in folder {0}, count {1}", folderId, currentFolderFailedItems.Count);
                            JobManagement.HasErrorNode = true;
                            FailureItems.Remove(folderId);
                        }
                        else
                        {
                            logger.Info("Failed items count{0}, in folder {1}", currentFolderFailedItems.Count, folderId);
                            if (currentFolderFailedItems.Count > 0)
                            {
                                JobManagement.HasErrorNode = true;
                                AddFailureItem2Azure(currentFolderFailedItems);
                            }
                            EXONodeInfoDao.AddEXONodeInfo(nodeCache.Value);
                        }
                    }
                }
            }
            catch (JobStopException)
            {
                logger.Info("Job Stopped");
                throw new JobStopException("This Job is stopped.");
            }
            finally
            {
                if (_isCosmosBulkOperationEnabled)
                {
                    CosmosBulkOperator.Instance.Reset();
                }
            }
        }

        private async System.Threading.Tasks.Task ProcessFailedItemsAsync(IExchangeFolder folder)
        {
            try
            {
                List<SyncFailureItemEntity> failedItems = SyncFailureItemDao.GetAllByDataSource(TenantLocalValue.LogonGroupId, AOSMailboxId.ToString(), (int)FailureSourceType.ExchangeDataSync);
                if (failedItems.Count > 0)
                {
                    int incItemsPerTask = failedItems.Count / 4;
                    logger.Info($"Process last failed item count:[{failedItems.Count}].incItemsPerTask:[{incItemsPerTask}]");
                    List<IExchangeItem> items = [];
                    IEnumerable<string> notExistIds = [];
                    using (var performance = new PerformanceScope("EXO.RMEXODataSync.GetFailedItems", $"EXO.RMEXODataSync.GetFailedItems.Count:{failedItems.Count}", true))
                    {
                        var result = folder.GetItemsByIds(failedItems.Select(item => new FailedItemEntity() { Id = item.NodeId }).ToList());
                        items = result.Item1;
                        notExistIds = result.Item2.Select(item => item.Id);
                    }

                    var deleteItems = failedItems.Where(i => notExistIds.Contains(i.NodeId)).ToList();
                    foreach (var deleteItem in deleteItems)
                    {
                        this.RemoveFailureItemFromAzure(deleteItem);
                    }
                    JobManagement.ReportManager.IncreaseBase(items.Count);
                    UniqueIdUtil idUtil = null;
                    using (var performance = new PerformanceScope("RMEXOSyncDataJobBase.GenerateUniqueIds", addToStatistics: true))
                    {
                        idUtil = new UniqueIdUtil(TenantLocalValue.LogonGroupId, items.Count);
                    }
                    if (items.Count > itemsPerTask)
                    {
                        var cts = new CancellationTokenSource();
                        //最多起4~5个Task处理Incremental的Changed Item，Full Job Get Item默认2k，因此itemsPerTask固定，但是Incremental items 数量不固定，因此需要按照多个处理。
                        AveTenantTasks.RunParallel(items, incItemsPerTask, cts, failedItem =>
                        {
                            ProcessFailedItemAsync(failedItem, failedItems.Where(i => i.NodeId == failedItem.ItemId).FirstOrDefault(), idUtil, cts).Wait();
                        });
                    }
                    else
                    {
                        foreach (var failedItem in items)
                        {
                            using (CheckJobStopScope jScope = new CheckJobStopScope())
                            {
                                await ProcessFailedItemAsync(failedItem, failedItems.Where(i => i.NodeId == failedItem.ItemId).FirstOrDefault(), idUtil);
                            }
                           
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
                logger.Error("An error occurred while processing failed items. Error:{0}", e.ToString());
            }
        }

        public virtual async System.Threading.Tasks.Task ProcessFailedItemAsync(IExchangeItem item, SyncFailureItemEntity failedItem, UniqueIdUtil idUtil, CancellationTokenSource cts = null)
        {
            try
            {
                using (new RA.Common.PerformanceScope(string.Format("EXO.RMEXOSyncDataJobBase.ProcessFailedItem", "", true)))
                {
                    if (item.TryGetExtendProperty(ExtendProperty.Term, out string value))
                    {
                        string dueDisposalTime = string.Empty;
                        Rule rs = null;
                        RMRuleItemCollection rules = null;
                        Guid termId;
                        if (Guid.TryParse(value, out termId))
                        {
                            if (!TermExistInTermManagement(termId))
                            {
                                logger.Info($"Term id no longer exists in term management for item : {item.ItemId}. Current term id:{termId}");
                                return;
                            }
                            if (TermAndRulesMapping.TryGetValue(termId, out rules))
                            {
                                if (rules == null || rules.CommonRules == null || rules.CommonRules.Rules.Count == 0)
                                {
                                    logger.Warn($"No rules realted to the item {item.ItemId}.");
                                }
                                else
                                {
                                    var newRuleCol = RebuldDARules(rules);
                                    if (newRuleCol.Rules.Count == 0)
                                    {
                                        logger.Info($"No DA rules realted to the item: {item.ItemId}.");
                                        //return null;
                                    }
                                    RuleManagement ruleManagement = new RuleManagement(newRuleCol);

                                    rs = ruleManagement.CheckItemCriteria(item);
                                    //文件已经符合Rule，直接获取action 以及due date
                                    if (rs != null)
                                    {
                                        int disposalAction = rs.KeepDataOption;
                                        dueDisposalTime = "RDM_RecordsExporer_Status_NextJob";
                                    }
                                    else
                                    {
                                        //文件不符合Rule，就需要判断一下以后符合Rule 的时间
                                        rs = ruleManagement.GetDueDisposalRule(item, ref dueDisposalTime);
                                    }
                                }
                            }
                            Record recordInDB;
                            using (var performance0 = new PerformanceScope("RMEXOSyncDataJobBase.GetDBRecord", addToStatistics: true))
                            {
                                recordInDB = GetRecordsByNodeIds(AOSMailboxId, new List<Guid>() { item.ItemId.ToMd5() }).FirstOrDefault();
                                    //ExplorerDao.GetRecordByIds(new List<Guid>() { IDGenerator.GetRecordId(MailboxAddress, item.ItemId) }).FirstOrDefault();
                            }
                            var recoInfo = GenerateRecordInfo(item, termId, rs, dueDisposalTime, recordInDB, idUtil);
                            await AddRecordToDBAsync(recoInfo, recordInDB, recordInDB != null);
                        }
                        else
                        {
                            logger.Info($"Cannot get term id for item : {item.ItemId}.");
                        }
                    }
                    else
                    {
                        logger.Info($"Item : {item.ItemId} does not have term value, so we don't add it to explorer db.");
                    }
                    RemoveFailureItemFromAzure(failedItem);
                }
            }
            catch (Exception e)
            {
                logger.Error($"error occurred while Process aveitem:{item.ItemId}, ERROR:{e.ToString()}");
                EXOCommonUtil.AddDetailsForSyncDataJob(item, MailboxAddress, JobDetailsStatus.Failed, e.Message);
            }
            finally
            {
                JobManagement.ReportManager.Increase();
            }
        }


        /// <summary>
        /// 1.兼容旧数据升级逻辑，先用DAOTreeNodeID获取DB中旧记录，如果有则用旧记录并删除旧记录.
        /// 2.新数据，直接通过AOSMailboxID和AOSObjectId取对应Mailbox记录
        /// </summary>
        protected DateTime GetStartDate(string mailboxId)
        {
            var mAOSEXONodeFlag = EXONodeInfoDao.GetEXONodeInfoByAOSMailboxIdAndObjectId(AOSMailboxId, GroupId, (int)NodeFlagType.ExplorerSync, AOSObjectId);
            if (mAOSEXONodeFlag != null)
            {
                DateTime collectionTime = new DateTime(mAOSEXONodeFlag.CollectionTime);
                logger.Info($"Current get CollectionTime:{collectionTime} by AOSMailboxId when EXO sync data.AOSMailboxId:{AOSMailboxId}.DAOTreeNodeID:{mailboxId}.GroupId:{GroupId}.AOSObjectId:{AOSObjectId}.");
                return collectionTime;
            }
            else
            {
                var mEXONodeFlag = EXONodeInfoDao.GetEXONodeInfo(new Guid(mailboxId), GroupId, (int)NodeFlagType.ExplorerSync);
                if (mEXONodeFlag != null)
                {
                    DateTime collectionTime = new DateTime(mEXONodeFlag.CollectionTime);
                    logger.Info($"Current get CollectionTime by DAOTreeNodeID when EXO sync data.CollectionTime:{collectionTime}.DAOTreeNodeID:{mailboxId}.GroupId:{GroupId}.");
                    //EXONodeFlagDao.DeleteEXONodeInfo(new Guid(mailboxId), groupId, (int)NodeFlagType.EnforceRetention);
                    return collectionTime;
                }
                else
                {
                    logger.Info($"Current CollectionTime can not be get by DAOTreeNodeID & AOSMailboxId when EXO sync data.AOSMailboxId:{AOSMailboxId}.DAOTreeNodeID:{mailboxId}.GroupId:{GroupId}.AOSObjectId:{AOSObjectId}.");
                    return DateTime.MinValue;
                }
            }
        }

        public override void Init()
        {
            base.Init();
            GroupId = new Guid(TreeManagement.GetGroupNode(TreeNodeDto).ID);
            AOSMailboxId = new Guid(base.MailboxGuid);

            _semaphore = new(MaxBackupItemsThreads, MaxBackupItemsThreads);
        }

        private async System.Threading.Tasks.Task ProcessChangedTermAsync(IExchangeFolder currentFolder, long CollectionTime)
        {
            bool startSuccess = false;
            try
            {
                var changes = ExplorerService.GetChangeTermIds(CollectionTime);
                List<BaseRecordDto> allRecords = new List<BaseRecordDto>(); ;
                if (changes.Count > 0)
                {
                    //此处没有调用ExplorerService.GetObjectDatas ，因为不需要获取account 之类的操作，后期可以考虑调整
                    using (new PerformanceScope("GetEXORecordsByTerms", $"GetEXORecordsByTerms{AOSMailboxId}", true))
                    {
                        logger.Info($"Total changed term count: {changes.Count}");
                        for (int i = 0; i < changes.Count; i += 1000)
                        {
                            var tempIds = changes.Skip(i).Take(1000).ToList();
                            logger.Info($"Query changed term from {i} to {i + 1000}");
                            var records = await QueryTermChangeRecordsAsync(AOSMailboxId, tempIds, JobStartTime.Ticks, MailboxAddress);
                            if (records != null && records.Count > 0)
                            {
                                allRecords.AddRange(records);
                            }
                        }
                        //ExplorerDao.GetEXORecordsByTerms(AOSMailboxId, changes, JobStartTime.Ticks, MailboxAddress);
                    }
                }

                if (allRecords == null || allRecords.Count == 0)
                {
                    logger.Info($"No Incremental Classification change records in mailbox : {AOSMailboxId}");
                    return;
                }
                if (_isCosmosBulkOperationEnabled)
                {
                    var RMKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
                    var bulkSize = RMKeyValueDao.GetCosmosBulkInsertOperationBufferSize();
                    if (bulkSize == default(int)) bulkSize = CosmosBulkOperator.DefualtBufferSize;
                    logger.Info($"Cosmos bulk operation enabled, bulk size: {bulkSize}");
                    CosmosBulkOperator.Instance.Start(bulkSize, ProcessSucceedRecordAsync, ProcessFailedRecord);
                    startSuccess = true;
                }
                UniqueIdUtil idUtil = null;
                using (var performance = new PerformanceScope("RMEXOSyncDataJobBase.GenerateUniqueIds", addToStatistics: true))
                {
                    idUtil = new UniqueIdUtil(TenantLocalValue.LogonGroupId, allRecords.Count);
                }

                //group by folder
                var folderItems = allRecords.GroupBy(t => t.FolderId).ToDictionary(g => g.Key, p => p.ToList());
                if (folderItems.Any(f => f.Value.Count > 1000))
                {
                    using (var performance = new PerformanceScope("RMEXOSyncDataJobBase.GetAllSubFoldersDeep", addToStatistics: true))
                    {
                        foreach (var folder in currentFolder.GetAllSubFoldersDeep())
                        {
                            Guid folderId = folder.FolderId.ToMd5();
                            if (!mSubFolders.ContainsKey(folderId))
                            {
                                mSubFolders.Add(folderId, folder);
                            }
                        }
                    }
                }
                foreach (var items in folderItems)
                {
                    if (items.Value.Count > 1000)
                    {
                        if (mSubFolders.ContainsKey(items.Key))
                        {
                            using (CheckJobStopScope jScope = new CheckJobStopScope())
                            {
                                var folder = mSubFolders[items.Key];
                                ProcessTermChangeGroupedItems(folder, items.Value, idUtil);
                            }
                        }
                    }
                    else
                    {
                        foreach (var record in items.Value)
                        {
                            try
                            {
                                using (CheckJobStopScope jScope = new CheckJobStopScope())
                                {
                                    //此种方式实例化的Item对象，没有Item Fullpath，需要另外赋值
                                    ArgumentCheck.NotNull(record, nameof(record));
                                    var item = CurrentFolder.GetItemById(record?.ExternalId);
                                    item.ItemPath = record.DirPath;
                                    await ProcessTermChangeItemAsync(item, idUtil);
                                }
                            }
                            catch (JobStopException)
                            {
                                throw new JobStopException("the job has stopped.");
                            }
                            catch (Exception ex)
                            {
                                logger.Error($"Error in process record : {record?.DirPath} in ProcessChangedTerm method, reason : {ex.ToString()}.");
                            }
                        }
                    }
                }
                //foreach (var record in allRecords)
                //{
                //    try
                //    {
                //        //此种方式实例化的Item对象，没有Item Fullpath，需要另外赋值
                //        var item = CurrentFolder.GetItemById(record?.ExternalId);
                //        item.ItemPath = record.DirPath;
                //        ProcessItem(item, idUtil);
                //    }
                //    catch (Exception ex)
                //    {
                //        logger.Error($"Error in process record : {record.DirPath} in ProcessChangedTerm method, reason : {ex.ToString()}.");
                //    }
                //}
            }
            catch (JobStopException)
            {
                logger.Info("Job Stopped");
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception ex)
            {
                logger.Error($"Error in ProcessChangedTerm, reason : {ex.ToString()}.");
                throw ex;
            }
            finally
            {
                if (_isCosmosBulkOperationEnabled && startSuccess)
                {
                    CosmosBulkOperator.Instance.Complete();
                }
            }
        }

        private void ProcessTermChangeGroupedItems(IExchangeFolder folder, List<BaseRecordDto> records, UniqueIdUtil idUtil)
        {
            var logonGroupId = TenantLocalValue.LogonGroupId;
            var logonUserEmail = TenantLocalValue.LogonUserEmail;
            bool isJobStopped = false;
            using (AveAppendableTaskExecutor taskExecutor = new AveAppendableTaskExecutor(MaxBackupItemsThreads))
            {
                taskExecutor.StartExecute();
                using (var performance = new PerformanceScope("EXO.RMEXODataSync.ProcessTermChangeGroupedItems", "", true))
                {
                    IEnumerable<IExchangeItemGroup> exchangeItems = null;
                    using (var performance1 = new PerformanceScope("EXO.RMEXODataSync.GetGroupItems", "", true))
                    {
                        IBatchDiscoverV2 fullDiscover = new FullDiscoverV2();
                        exchangeItems = fullDiscover.GetGroupedItems(folder, null);
                    }

                    foreach (var itemGroup in exchangeItems)
                    {
                        using (CheckJobStopScope jScope = new CheckJobStopScope())
                        {
                            taskExecutor.AddTask(() =>
                            {
                                try
                                {
                                    using CheckJobStopScope jScope = new();
                                    TenantLocalValue.LogonGroupId = logonGroupId;
                                    TenantLocalValue.LogonUserEmail = logonUserEmail;
                                    JobManagement.ReportManager.Increase();
                                    logger.Info($"Begin processing term change items, items count is : {itemGroup.ItemsCount}.");
                                    ProcessTermChangeItemsByGroupAsync(itemGroup, records, idUtil).Wait();
                                }
                                catch (JobStopException)
                                {
                                    isJobStopped = true;
                                }
                            });
                        }
                    }

                    logger.Info($"Add items to task executor finished.");
                    if (!taskExecutor.WaitForAllTasks(Timeout.Infinite))
                    {
                        //todo: handle timeout
                        logger.Error($"Time out exception.");
                    }
                }
            }
            if (isJobStopped)
            {
                throw new JobStopException("This Job is stopped.");
            }
            logger.Info($"ProcessItems finish.");
        }

        protected async System.Threading.Tasks.Task ProcessTermChangeItemAsync(IExchangeItem item, UniqueIdUtil idUtil)
        {
            using (new PerformanceScope("RMEXOSyncDataJobBase.ProcessItem", addToStatistics: true))
            {
                logger.Info($"Begin processing item : {item.ItemId}.");
                var status = AvePoint.RA.Contract.RMWeb.JobMonitor.JobDetailsStatus.Successful;
                var comment = string.Empty;
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope()) 
                    {
                        if (item.TryGetExtendProperty(ExtendProperty.Term, out var termIdString))
                        {
                            string dueDisposalTime = string.Empty;
                            Rule rs = null;
                            RMRuleItemCollection rules = null;
                            Guid termId;
                            if (Guid.TryParse(termIdString, out termId))
                            {
                                if (!TermExistInTermManagement(termId))
                                {
                                    logger.Info($"Term id no longer exists in term management for item : {item.ItemId}. Current term id:{termId}");
                                    return;
                                }
                                if (TermAndRulesMapping.TryGetValue(termId, out rules))
                                {
                                    if (rules == null || rules.CommonRules == null || rules.CommonRules.Rules.Count == 0)
                                    {
                                        logger.Warn($"No rules realted to the item {item.ItemId}.");
                                    }
                                    else
                                    {
                                        var newRuleCol = RebuldDARules(rules);
                                        if (newRuleCol.Rules.Count == 0)
                                        {
                                            logger.Info($"No DA rules realted to the item: {item.ItemId}.");
                                            //return null;
                                        }
                                        RuleManagement ruleManagement = new RuleManagement(newRuleCol);
                                        rs = ruleManagement.CheckItemCriteria(item);
                                        //文件已经符合Rule，直接获取action 以及due date
                                        if (rs != null)
                                        {
                                            int disposalAction = rs.KeepDataOption;
                                            dueDisposalTime = "RDM_RecordsExporer_Status_NextJob";
                                        }
                                        else
                                        {
                                            //文件不符合Rule，就需要判断一下以后符合Rule 的时间
                                            rs = ruleManagement.GetDueDisposalRule(item, ref dueDisposalTime);
                                        }
                                    }
                                }
                                //ReadById 方法，在多线程或者并发情况，Cosmos DB 会有很长时间才能返回结果.此方法单个执行，会存在效率问题
                                //目前没有单个处理Item 的方法，理论上ProcessItem方法不会被调用，暂时保留ExplorerDao.ReadById的调用。如果使用ProcessItem方法，需要处理效率问题
                                var recordInDB = ExplorerDao.GetFirstOrDefault(e => e.ScopeId == AOSMailboxId && e.NodeId == item.ItemId.ToMd5());

                                if (rs != null)
                                {
                                    logger.Info($"swith item rule: {recordInDB.Id}, {recordInDB.RuleId} 2 {rs?.Id}.");
                                    #region change item rule
                                    var ruleId = new Guid(rs.Id);
                                    if (!rs.IsManualApproval)
                                    {
                                        recordInDB.RecordOwner = string.Empty;
                                    }
                                    recordInDB.RuleId = ruleId;
                                    recordInDB.RuleLevel = (int)rs.PolicyLevel;

                                    recordInDB.PreviosDisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(dueDisposalTime);
                                    recordInDB.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(dueDisposalTime);
                                    UpdateDueDate(recordInDB, rs);
                                    if (_isCosmosBulkOperationEnabled)
                                    {
                                        CosmosBulkOperator.Instance.Add(recordInDB);
                                    }
                                    else
                                    {
                                        ExplorerDao.Upsert(recordInDB);
                                        await ProcessSucceedRecordAsync(recordInDB);
                                    }

                                    #endregion
                                }
                                else if (recordInDB.RuleId != Guid.Empty && rs == null)
                                {
                                    logger.Info("Empty the item rule {0}", recordInDB.Id);
                                    #region empty item rule
                                    recordInDB.RuleId = Guid.Empty;
                                    recordInDB.RuleLevel = 0;
                                    recordInDB.RecordOwner = string.Empty;
                                    recordInDB.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(string.Empty);
                                    recordInDB.PreviosDisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(string.Empty);
                                    if (_isCosmosBulkOperationEnabled)
                                    {
                                        CosmosBulkOperator.Instance.Add(recordInDB);
                                    }
                                    else
                                    {
                                        ExplorerDao.Upsert(recordInDB);
                                        await ProcessSucceedRecordAsync(recordInDB);
                                    }

                                    #endregion
                                }
                                else
                                {
                                    logger.Info("No change item {0}", recordInDB.Id);
                                }
                            }
                            else
                            {
                                logger.Info($"Cannot get term id for item : {item.ItemId}.");
                            }

                        }
                        else
                        {
                            logger.Info($"Item : {item.ItemId} does not have term value, so we don't add it to explorer db.");
                        }

                    }
                }
                catch (JobStopException)
                {
                    logger.Info("Job Stopped");
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception ex)
                {
                    JobManagement.HasErrorNode = true;
                    status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed;
                    logger.Error($"An error occur in ProcessItem, item id {item?.ItemId}, reason : {ex.ToString()}.");
                    JobManagement.ReportManager.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMEXODataSyncJobDetails()
                    {
                        ObjectName = item.ItemName,
                        FullPath = MailboxAddress + item.ItemPath + "_" + item.SendDateUTC.ToString("R"),
                        ItemType = JobReportUtility.ConvertItemTypeForDetails(NodeLevel.ExchangeOnlineItem),
                        Status = status,
                        Comment = ex.Message,
                    });
                }
            }
        }

        private void UpdateDueDate(Record itemNode, Rule rule)
        {
            //Hold状态Record重新计算Due Date;
            if (itemNode.HoldStatus && IsRemoveRule(rule))
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

        private bool IsRemoveRule(Rule tempRule)
        {
            //var result = false;
            ////if (tempRule != null && tempRule.EXORule != null && tempRule.EXORule.KeepDataOption == 0)
            ////RECO-3972
            ////current rule is exo rule.
            //if (tempRule != null && tempRule.KeepDataOption == 0)
            //{
            //    result = true;
            //}
            if (tempRule != null)
            {
                int action = RuleHelper.GetOperationTypeForEXO(tempRule);
                if (action == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private async System.Threading.Tasks.Task ProcessTermChangeItemsByGroupAsync(IExchangeItemGroup itemGroup, List<BaseRecordDto> records, UniqueIdUtil idUtil)
        {
            foreach (var item in itemGroup.Items)
            {
                var itemId = IDGenerator.GetRecordId(MailboxAddress, item.ItemId);
                if (records.Where(r => r.Id == itemId).FirstOrDefault() != null)
                {
                    await ProcessTermChangeItemAsync(item, idUtil);
                }
            }
        }

        private async Task<List<BaseRecordDto>> QueryTermChangeRecordsAsync(Guid mailboxId, List<Guid> changeTermIds, long ticks, string emailAddress)
        {
            var pagingInfo = new ExplorerPagingInfo()
            {
                PageSize = 200,
                PageIndex = ""
            };
            ExplorerPagingInfo pageInfo;
            ExplorerQueryV2Dto queryDto = BuildTermChangeItemFilter(mailboxId, changeTermIds, ticks, emailAddress);
            queryDto.PagingInfo = pagingInfo;
            List<BaseRecordDto> records = new List<BaseRecordDto>();
            do
            {
                var result = await ExplorerQueryService.QueryDataListWithoutTotalDirectlyAsync(queryDto);
                if (result != null && result.Datas != null && result.Datas.Count > 0)
                {
                    records.AddRange(result.Datas);
                    logger.Debug($"Got {result.Datas.Count} change term records.");
                }
                pageInfo = result?.PagingInfo;
            }
            while (pageInfo != null && pageInfo.HasNextPage);
            logger.Info("Total term changed records count:{0}", records.Count);
            return records;
        }

        private ExplorerQueryV2Dto BuildTermChangeItemFilter(Guid mailboxId, List<Guid> changeTermIds, long ticks, string emailAddress)
        {
            ExplorerQueryV2Dto explorerQueryV2Dto = new ExplorerQueryV2Dto();
            DateTime collectionTime = DateTime.SpecifyKind(new DateTime(ticks), DateTimeKind.Utc);
            explorerQueryV2Dto.QueryOption = new ExplorerQueryOptionV2()
            {
                FilterOption = new ExplorerFilterOptionV2()
                {
                    TermIds = changeTermIds,
                    ScopeId = mailboxId.ToString(),
                    CollectionDateInfo = new DateInfo()
                    {
                        Condition = DateCondition.Before,
                        Value1 = collectionTime.ToString(),
                        TimeZoneId = "UTC"
                    },
                    MailboxAddress = emailAddress,
                    Status = new List<RMRecordStatus>() { RMRecordStatus.Active },
                    NodeTypes = new List<Contract.RMWeb.Tree.Base.RMNodeLevel>() { Contract.RMWeb.Tree.Base.RMNodeLevel.ExchangeOnlineItem },
                    SourceFlags = new List<SourceFlag>() { SourceFlag.Exchange }
                }
            };
            return explorerQueryV2Dto;
        }

        private async System.Threading.Tasks.Task ProcessFolderAsync(IExchangeFolder folder)
        {
            logger.Info($"Begin processing folder : {folder.FolderId}.");
            //此处用GetItems 的值更合理，但是很多getitems是异步的，没有办法获取所有值
            JobManagement.ReportManager.IncreaseBase(folder.ItemsCount);
            using (var performance = new PerformanceScope("EXO.RMEXODataSync.ProcessFolder", "", true))
            {
                try
                {
                    foreach (var mFolder in GetFolders(folder))
                    {
                        using (CheckJobStopScope jScope = new CheckJobStopScope())
                        {
                            await ProcessFolderAsync(mFolder);
                        }
                    }

                    DateTime collectionTime = DateTime.UtcNow;
                    ProcessGroupedItems(folder);

                    var nodeInfo = EXONodeInfoDao.GetEXONodeInfo(folder.FolderId.ToMd5(), GroupId, (int)NodeFlagType.ExplorerSync);
                    discover.GetGroupedDeleteItems(folder, nodeInfo?.ItemSyncState);
                    var deleteItemIds = discover.GetDeleteItemIds();

                    logger.Info($"start process deleted items, deleted items count is {deleteItemIds.Count}.");
                    foreach (var item in deleteItemIds)
                    {
                        using (var performance1 = new PerformanceScope("EXO.RMEXODataSync.ProcessDeleteItem", "", true))
                        {
                            using (CheckJobStopScope jScope = new CheckJobStopScope())
                            {
                                await ProcessDeletedItemAsync(item);
                            }
                        }
                    }
                    using (var performance1 = new PerformanceScope("EXO.RMEXODataSync.GenerateCurrentItemSyncState", "", true))
                    {
                        folder.GenerateCurrentItemSyncState();
                    }

                    Guid folderId = folder.FolderId.ToMd5();
                    folderFlags.Add(folderId, GenerateNodeFlag(folder, collectionTime));   
                }
                catch (JobStopException)
                {
                    logger.Info("Job Stopped");
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception ex)
                {
                    JobManagement.HasErrorNode = true;
                    logger.Error($"Error in process folder : {folder.DisplayFolderPath}, reason : {ex.ToString()}.");
                    JobManagement.ReportManager.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMEXODataSyncJobDetails()
                    {
                        ObjectName = folder.FolderName,
                        FullPath = MailboxAddress + folder.DisplayFolderPath,
                        ItemType = JobReportUtility.ConvertItemTypeForDetails(NodeLevel.ExchangeFolder),
                        Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed,
                    });
                }
            }
        }

        private void AddFailureItem2Cache(IExchangeItem exoItem, IExchangeFolder folder, Exception e)
        {
            RMEXOSyncFailureItem failureItem = new RMEXOSyncFailureItem() { MailboxId = AOSMailboxId.ToString(), JobId = JobManagement.SubJobId };
            failureItem.URL = MailboxAddress + exoItem.ItemPath + "_" + exoItem.SendDateUTC.ToString("R");
            failureItem.NodeId = exoItem.ItemId;
            failureItem.ObjectName = exoItem?.ItemName;
            failureItem.Message = this.GetExceptionMessage(e);
            Guid folderId = folder.FolderId.ToMd5();
            lock (mLock)
            {
                if (FailureItems.ContainsKey(folderId))
                {
                    if (FailureItems[folderId].Count <= 200)
                    {
                        FailureItems[folderId].Add(failureItem);
                    }
                }
                else
                {
                    FailureItems.TryAdd(folderId, new List<RMEXOSyncFailureItem>() { failureItem });
                }
            }
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

        private void AddFailureItem2Azure(List<RMEXOSyncFailureItem> items)
        {
            try
            {
                if (items.Count > 0)
                {
                    List<SyncFailureItemEntity> failureEntities = new List<SyncFailureItemEntity>();
                    foreach (var item in items)
                    {
                        SyncFailureItemEntity entity = new SyncFailureItemEntity(item.MailboxId, item.NodeId.ToMd5().ToString());
                        entity.DataSource = (int)FailureSourceType.ExchangeDataSync;
                        entity.JobId = item.JobId;
                        entity.NodeId = item.NodeId;
                        entity.FullPath = item.URL;
                        failureEntities.Add(entity);
                    }
                    logger.Debug($"Add entity to azure, list count: {failureEntities.Count}");
                    SyncFailureItemDao.Add(TenantLocalValue.LogonGroupId, failureEntities);
                }
            }
            catch (Exception e)
            {
                JobManagement.HasErrorNode = true;
                logger.Error(e.Message, e);
            }
        }

        private void RemoveFailureItemFromAzure(SyncFailureItemEntity entity)
        {
            try
            {
                logger.Debug($"Remove entity from azure, scope id: {entity.PartitionKey}, item Id:{entity.NodeId}");
                SyncFailureItemDao.Remove(TenantLocalValue.LogonGroupId, entity);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
        }

        private void ProcessGroupedItems(IExchangeFolder folder)
        {
            var logonGroupId = TenantLocalValue.LogonGroupId;
            var logonUserEmail = TenantLocalValue.LogonUserEmail;
            bool isJobStopped = false;
            using (AveAppendableTaskExecutor taskExecutor = new AveAppendableTaskExecutor(MaxBackupItemsThreads))
            {
                taskExecutor.StartExecute();
                using (var performance = new PerformanceScope("EXO.RMEXODataSync.ProcessGroupedItems", "", true))
                {
                    IEnumerable<IExchangeItemGroup> exchangeItems = null;
                    using (var performance1 = new PerformanceScope("EXO.RMEXODataSync.GetGroupItems", "", true))
                    {
                        var filter = GenerateSearchFilter(folder);
                        exchangeItems = discover.GetGroupedItems(folder, filter);
                    }
                    foreach (var itemGroup in exchangeItems)
                    {
                        using (CheckJobStopScope jScope = new CheckJobStopScope())
                        {
                            taskExecutor.AddTask(() =>
                            {
                                try
                                {
                                    using CheckJobStopScope jScope = new();
                                    TenantLocalValue.LogonGroupId = logonGroupId;
                                    TenantLocalValue.LogonUserEmail = logonUserEmail;
                                    logger.Info($"Begin processing items, items count is : {itemGroup.ItemsCount}.");
                                    ProcessItemsAsync(itemGroup, folder).Wait();
                                }
                                catch (JobStopException)
                                {
                                    isJobStopped = true;
                                }
                                catch (Exception ex)
                                {
                                    logger.Error(ex.ToString());
                                }
                            });    
                        }
                    }
                }
                logger.Info($"Add items to task executor finished.");
                if (!taskExecutor.WaitForAllTasks(Timeout.Infinite))
                {
                    //todo: handle timeout
                    logger.Error($"Time out exception.");
                }
            }
            if (isJobStopped)
            {
                throw new JobStopException("This Job is stopped.");
            }
            logger.Info($"ProcessItems finish.");
        }

        private Microsoft.Exchange.WebServices.Data.SearchFilter GenerateSearchFilter(IExchangeFolder folder)
        {
            DateTime collectionTime = DateTime.MinValue;
            var nodeInfo = EXONodeInfoDao.GetEXONodeInfo(folder.FolderId.ToMd5(), GroupId, (int)NodeFlagType.ExplorerSync);
            if (nodeInfo != null)
            {
                collectionTime = DateTime.SpecifyKind(new DateTime(nodeInfo.CollectionTime), DateTimeKind.Utc);
            }
            return collectionTime != DateTime.MinValue ? new Microsoft.Exchange.WebServices.Data.SearchFilter.IsGreaterThanOrEqualTo(Microsoft.Exchange.WebServices.Data.ItemSchema.LastModifiedTime, collectionTime) : null;
        }

        private async System.Threading.Tasks.Task ProcessItemsAsync(IExchangeItemGroup itemGroup, IExchangeFolder folder, int retryCount = 0)
        {
            List<IExchangeItem> nonePropertyItems = new List<IExchangeItem>();
            try
            {
                logger.Info($"Begin process grouped item, item count: {itemGroup.ItemsCount}.");
                var records = new List<Record>();
                using (new RA.Common.PerformanceScope("EXO.RMEXOSyncDataJobBase.ProcessItems.GetRecordsByIds", $"RMEXOSyncDataJobBase.GetRecordsByIds.Count:{itemGroup.Items.Count()}", true))
                {
                    records = GetRecordsByNodeIds(AOSMailboxId, itemGroup.Items.Select(i => i.ItemId.ToMd5()).ToList());
                    logger.Info($"Get {records.Count} records from db.");
                }
                UniqueIdUtil idUtil = null;
                using (var performance = new PerformanceScope("RMEXOSyncDataJobBase.GenerateUniqueIds", addToStatistics: true))
                {
                    idUtil = new UniqueIdUtil(TenantLocalValue.LogonGroupId, itemGroup.Items.Count());
                }
                foreach (var item in itemGroup.Items)
                {
                    try
                    {
                        using (new RA.Common.PerformanceScope("EXO.RMEXOSyncDataJobBase.ProcessItem", addToStatistics: true))
                        {
                            string value;
                            if (item.TryGetExtendProperty(ExtendProperty.Term, out value))
                            {
                                string dueDisposalTime = string.Empty;
                                Rule rs = null;
                                RMRuleItemCollection rules = null;
                                Guid termId;

                                if (Guid.TryParse(value, out termId))
                                {
                                    if (!TermExistInTermManagement(termId))
                                    {
                                        logger.Info($"Term id no longer exists in term management for item : {item.ItemId}. Current term id:{termId}");
                                        continue;
                                    }
                                    using (new RA.Common.PerformanceScope("EXO.RMEXOSyncDataJobBase.CheckRule", addToStatistics: true))
                                    {
                                        if (TermAndRulesMapping.TryGetValue(termId, out rules))
                                        {
                                            if (rules == null || rules.CommonRules == null || rules.CommonRules.Rules.Count == 0)
                                            {
                                                logger.Warn($"No rules realted to the item {item.ItemId}.");
                                            }
                                            else
                                            {
                                                var newRuleCol = RebuldDARules(rules);
                                                if (newRuleCol.Rules.Count == 0)
                                                {
                                                    logger.Info($"No DA rules realted to the item: {item.ItemId}.");
                                                    //return null;
                                                }
                                                RuleManagement ruleManagement = new RuleManagement(newRuleCol);

                                                using (new RA.Common.PerformanceScope("EXO.RMEXOSyncDataJobBase.CheckItemCriteria", addToStatistics: true))
                                                {
                                                    rs = ruleManagement.CheckItemCriteria(item);
                                                }
                                                //文件已经符合Rule，直接获取action 以及due date
                                                if (rs != null)
                                                {
                                                    int disposalAction = rs.KeepDataOption;
                                                    dueDisposalTime = "RDM_RecordsExporer_Status_NextJob";
                                                }
                                                else
                                                {
                                                    using (new RA.Common.PerformanceScope("EXO.RMEXOSyncDataJobBase.GetDueDisposalRule", addToStatistics: true))
                                                    {
                                                        //文件不符合Rule，就需要判断一下以后符合Rule 的时间
                                                        rs = ruleManagement.GetDueDisposalRule(item, ref dueDisposalTime);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    Record recordInDB;
                                    using (var performance0 = new PerformanceScope("RMEXOSyncDataJobBase.GetDBRecord", addToStatistics: true))
                                    {
                                        recordInDB = records.FirstOrDefault(r => r.NodeId == item.ItemId.ToMd5());
                                    }
                                    var recoInfo = GenerateRecordInfo(item, termId, rs, dueDisposalTime, recordInDB, idUtil);

                                    await AddRecordToDBAsync(recoInfo, recordInDB, recordInDB != null);
                                }
                                else
                                {
                                    logger.Info($"Cannot get term id for item : {item.ItemId}.");
                                }

                            }
                            else
                            {
                                logger.Info($"Item : {item.ItemId} does not have term value, so we don't add it to explorer db.");
                                nonePropertyItems.Add(item);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        AddFailureItem2Cache(item, folder, ex);
                        //JobManagement.HasErrorNode = true;
                        logger.Error($"An error occur in ProcessItem, item id {item?.ItemId}, reason : {ex.ToString()}.");
                        EXOCommonUtil.AddDetailsForSyncDataJob(item, MailboxAddress, JobDetailsStatus.Failed, ex.Message);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"Error occurred while sync item in folder:{folder.FolderId} Error:{e.ToString()}");
                if (itemGroup.Items != null && itemGroup.Items.Count() > 0)
                {
                    foreach (var item in itemGroup.Items)
                    {
                        AddFailureItem2Cache(item, folder, e);
                        AddFailedDetail(item, e.Message);
                    }
                }
            }
            finally
            {
                if (nonePropertyItems.Count > 0)
                {
                    try
                    {
                        JobManagement.ReportManager.Increase(itemGroup.ItemsCount - nonePropertyItems.Count);
                        logger.Info($"Retry count is:{retryCount} None property item count:{nonePropertyItems.Count}");
                        if (retryCount < 2)
                        {
                            await ProcessItemsAsync(new IExchangeItemGroup(nonePropertyItems.AsEnumerable()), folder, ++retryCount);
                        }
                        else
                        {
                            foreach (var item in nonePropertyItems)
                            {
                                AddFailureItem2Cache(item, folder, new Exception("Term property not found"));
                                AddFailedDetail(item, "Term property not found");
                            }
                            JobManagement.ReportManager.Increase(nonePropertyItems.Count);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Error occurred while retry process items.Folder:{folder?.FolderId} Error:{e.ToString()}");
                    }
                }
                else
                {
                    JobManagement.ReportManager.Increase(itemGroup.ItemsCount);
                }
            }
        }

        private void AddFailedDetail(IExchangeItem item, string errorMessage)
        {
            EXOCommonUtil.AddDetailsForSyncDataJob(item, MailboxAddress, JobDetailsStatus.Failed, errorMessage);
        }

        protected async System.Threading.Tasks.Task ProcessItemAsync(IExchangeItem item, UniqueIdUtil idUtil)
        {
            using (new PerformanceScope("RMEXOSyncDataJobBase.ProcessItem", addToStatistics: true))
            {
                logger.Info($"Begin processing item : {item.ItemId}.");
                var status = AvePoint.RA.Contract.RMWeb.JobMonitor.JobDetailsStatus.Successful;
                var comment = string.Empty;
                try
                {
                    if (item.TryGetExtendProperty(ExtendProperty.Term, out var termIdString))
                    {
                        string dueDisposalTime = string.Empty;
                        Rule rs = null;
                        RMRuleItemCollection rules = null;
                        Guid termId;
                        if (Guid.TryParse(termIdString, out termId))
                        {
                            if (!TermExistInTermManagement(termId))
                            {
                                logger.Info($"Term id no longer exists in term management for item : {item.ItemId}. Current term id:{termId}");
                                return;
                            }
                            if (TermAndRulesMapping.TryGetValue(termId, out rules))
                            {
                                if (rules == null || rules.CommonRules == null || rules.CommonRules.Rules.Count == 0)
                                {
                                    logger.Warn($"No rules realted to the item {item.ItemId}.");
                                }
                                else
                                {
                                    var newRuleCol = RebuldDARules(rules);
                                    if (newRuleCol.Rules.Count == 0)
                                    {
                                        logger.Info($"No DA rules realted to the item: {item.ItemId}.");
                                        //return null;
                                    }
                                    RuleManagement ruleManagement = new RuleManagement(newRuleCol);

                                    rs = ruleManagement.CheckItemCriteria(item);
                                    //文件已经符合Rule，直接获取action 以及due date
                                    if (rs != null)
                                    {
                                        int disposalAction = rs.KeepDataOption;
                                        dueDisposalTime = "RDM_RecordsExporer_Status_NextJob";
                                    }
                                    else
                                    {
                                        //文件不符合Rule，就需要判断一下以后符合Rule 的时间
                                        rs = ruleManagement.GetDueDisposalRule(item, ref dueDisposalTime);
                                    }
                                }
                            }
                            //ReadById 方法，在多线程或者并发情况，Cosmos DB 会有很长时间才能返回结果.此方法单个执行，会存在效率问题
                            //目前没有单个处理Item 的方法，理论上ProcessItem方法不会被调用，暂时保留ExplorerDao.ReadById的调用。如果使用ProcessItem方法，需要处理效率问题
                            var recordInDB = ExplorerDao.GetFirstOrDefault(e => e.ScopeId == AOSMailboxId && e.NodeId == item.ItemId.ToMd5());
                            var recoInfo = GenerateRecordInfo(item, termId, rs, dueDisposalTime, recordInDB, idUtil);
                            await AddRecordToDBAsync(recoInfo, recordInDB, recordInDB != null);
                        }
                        else
                        {
                            logger.Info($"Cannot get term id for item : {item.ItemId}.");
                        }

                    }
                    else
                    {
                        logger.Info($"Item : {item.ItemId} does not have term value, so we don't add it to explorer db.");
                    }
                }
                catch (Exception ex)
                {
                    JobManagement.HasErrorNode = true;
                    status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed;
                    logger.Error($"An error occur in ProcessItem, item id {item?.ItemId}, reason : {ex.ToString()}.");
                    JobManagement.ReportManager.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMEXODataSyncJobDetails()
                    {
                        ObjectName = item.ItemName,
                        FullPath = MailboxAddress + item.ItemPath + "_" + item.SendDateUTC.ToString("R"),
                        ItemType = JobReportUtility.ConvertItemTypeForDetails(NodeLevel.ExchangeOnlineItem),
                        Status = status,
                        Comment = ex.Message,
                    });
                }
            }
        }

        private bool TermExistInTermManagement(Guid termId)
        {
            return TermIdAndNameMapping.ContainsKey(termId);
        }

        protected async System.Threading.Tasks.Task ProcessDeletedItemAsync(string itemId)
        {
            var id = IDGenerator.GetRecordId(MailboxAddress, itemId);
            var scopeId = AOSMailboxId;
            try
            {
                //Get previous term id before update if needed... 
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    var dbRec = ExplorerDao.ReadById(scopeId, id);
                    if (dbRec != null)
                    {
                        ExplorerDao.UpdateRecordState(scopeId, id, 3);
                        logger.Info($"update record state to 3 for item id : {itemId}.");
                        //Incremental Logic for Dashboard Collection job...
                        using (new RA.Common.PerformanceScope(string.Format("CollectionData.Exchange.ProcessIncrementalTermUsagesForDeleteRecord")))
                        {
                            await ProcessIncrementalTermUsagesForDeleteRecordAsync(dbRec, JobManagement.SubJobId);
                        }
                        using (new RA.Common.PerformanceScope(string.Format("CollectionData.Exchange.ProcessIncrementalDataOfDaysForDeleteRecord")))
                        {
                            await ProcessIncrementalDataOfDaysForDeleteRecordAsync(dbRec, JobManagement.SubJobId);
                        }
                        using (new RA.Common.PerformanceScope(string.Format("CollectionData.Exchange.ProcessIncrementalTotalsForDeleteRecord")))
                        {
                            await ProcessIncrementalTotalsForDeleteRecordAsync(dbRec, JobManagement.SubJobId);
                        }
                    }
                }
            }
            catch (JobStopException)
            {
                logger.Info("Job Stopped");
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception ex)
            {
                logger.Warn($"Cannot update state for item id : {itemId}, reason : {ex.ToString()}.");
            }
        }

        private List<Record> GetRecordsByNodeIds(Guid scopeId, List<Guid> nodeIds)
        {
            var records = new List<Record>();
            try
            {
                records = ExplorerDao.QueryAll(r => r.ScopeId == scopeId && nodeIds.Contains(r.NodeId)).ToList();
            }
            catch (Exception ex)
            {
                logger.Warn($"Cannot get records by ids, scope id is : {scopeId.ToString()}, reason : {ex.ToString()}.");
            }
            return records;
        }

        private async System.Threading.Tasks.Task AddRecordToDBAsync(Record recoEntity, Record dbRec, bool getDBRecord = true)
        {
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    using (var performance0 = new PerformanceScope("RMEXOSyncDataJobBase.AddRecordToDB", addToStatistics: true))
                    {
                        //Get previous term id before update if needed...
                        var currentDbTermId = Guid.Empty;

                        var existRecord = false;
                        if (dbRec.CheckExistAndTagDuplicateManual())
                        {
                            recoEntity.KeepOldManualColumn(dbRec);
                        }
                        if (dbRec != null && dbRec.RecordStatus == (int)RMRecordStatus.Active)
                        {
                            existRecord = true;
                            currentDbTermId = dbRec.TermId;
                        }
                        recoEntity.ContainerId = containerId;
                        if (_isCosmosBulkOperationEnabled)
                        {
                            Add2BulkOperationQueue(recoEntity, dbRec, getDBRecord);
                            return;
                        }
                        //Dong Xie:此方法底层仍然调用了ReadById 方法, 浪费了一部分性能。目前Data Sync 功能外围已经读取过了，所以知道Record 是否存在，此处仍然有优化效率的空间
                        RMRule tempRule = null;
                        if (recoEntity.RuleId != Guid.Empty && mRuleCache != null && mRuleCache.ContainsKey(recoEntity.RuleId))
                        {
                            tempRule = mRuleCache[recoEntity.RuleId];
                        }
                        var operationResult = ExplorerDao.AddOrUpdateRecord(recoEntity, false, tempRule);
                        await ProcessSucceedRecordAsync(recoEntity);
                        //Incremental Logic for Dashboard Collection job...
                        if (operationResult)
                        {
                            if (!existRecord)
                            {
                                using (new RA.Common.PerformanceScope(string.Format("CollectionData.Exchange.ProcessIncrementalTermUsagesForAdd")))
                                {
                                    await ProcessIncrementalTermUsagesForAddAsync(recoEntity, JobManagement.SubJobId);
                                }
                                using (new RA.Common.PerformanceScope(string.Format("CollectionData.Exchange.ProcessIncrementalDataOfDaysForAdd")))
                                {
                                    ProcessIncrementalDataOfDaysForAdd(recoEntity, JobManagement.SubJobId);
                                }
                                using (new RA.Common.PerformanceScope(string.Format("CollectionData.Exchange.ProcessIncrementalTotalsForAdd")))
                                {
                                    await ProcessIncrementalTotalsForAddAsync(recoEntity, JobManagement.SubJobId);
                                }
                            }
                            else
                            {
                                using (new RA.Common.PerformanceScope(string.Format("CollectionData.Exchange.ProcessIncrementalTermUsagesForUpdate")))
                                {
                                    await ProcessIncrementalTermUsagesForUpdateAsync(recoEntity, currentDbTermId, JobManagement.SubJobId);
                                }
                            }
                        }
                        return;
                    }
                }
            }
            catch (JobStopException)
            {
                throw new JobStopException("the job has stopped.");
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
                if (ExplorerDao.NeedUpdateRecord(newItem, false, dbRecord, tempRule))
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
                    if (ExplorerDao.NeedUpdateRecord(newItem, false, tempRule))
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

        private async System.Threading.Tasks.Task ProcessSucceedRecordAsync(Record item)
        {
            logger.Info($"add record to db success, the item id:{item?.Id}");
            JobManagement.HasSuccessNode = true;
            //only report item
            ArgumentCheck.NotNull(item, nameof(item));
            JobManagement.ReportManager.SendJobDetail(new JMEXODataSyncJobDetails()
            {
                ObjectName = item.LeafName,
                FullPath = string.Format(AvePoint.RA.Common.RecordsConstants.EXOLocationFormat, item.EmailAddress, item.DirPath, new DateTime(item.TimeCreated).ToString("R")),
                ItemType = JobReportUtility.ConvertItemTypeForDetails(NodeLevel.ExchangeOnlineItem),
                Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Successful,
                Comment = string.Empty,
            }); 
            ProcessManualDuplicateData(item);
        }

        private void ProcessManualDuplicateData(Record newItem)
        {
            //先运行manual review job scan进来的数据createdate为0, 需要remove, 重新生成一条数据.
            if (newItem.hasDuplicate)
            {
                try
                {
                    //manual 数据能拿到的Id是通过下面方式生成的.
                    var manualId = IDGenerator.GetRecordId(newItem.EmailAddress, newItem.ItemId.ToString());
                    logger.Info($"remove old manual data:{newItem.Id}, manual Id:{manualId}");
                    ExplorerDao.Delete(0, manualId);
                }
                catch (Exception ex)
                {
                    logger.Error($"error occurred while remove old manual data, ERROR: {ex.ToString()}");
                }

            }
        }

        private void ProcessFailedRecord(Record item, Exception ex)
        {
            logger.Warn($"Failed to add record to db, the item id:{item?.Id}");
            ArgumentCheck.NotNull(item, nameof(item));
            JobManagement.HasErrorNode = true;
            JobManagement.ReportManager.SendJobDetail(new AvePoint.RA.Contract.RMWeb.JobMonitor.JMEXODataSyncJobDetails()
            {
                ObjectName = item.LeafName,
                FullPath = string.Format(AvePoint.RA.Common.RecordsConstants.EXOLocationFormat, item.EmailAddress, item.DirPath, new DateTime(item.TimeCreated).ToString("R")),
                ItemType = JobReportUtility.ConvertItemTypeForDetails(NodeLevel.ExchangeOnlineItem),
                Status = Contract.RMWeb.JobMonitor.JobDetailsStatus.Failed,
                Comment = ex.Message,
            }); ;

            if (item.NodeType == (int)NodeLevel.ExchangeOnlineItem)
            {
                AddFailureItem2Cache(item, ex);
            }
        }

        private void AddFailureItem2Cache(Record item, Exception e)
        {
            try
            {
                RMEXOSyncFailureItem failureItem = new RMEXOSyncFailureItem() { MailboxId = AOSMailboxId.ToString(), JobId = JobManagement.SubJobId };
                failureItem.URL = string.Format(AvePoint.RA.Common.RecordsConstants.EXOLocationFormat, item.EmailAddress, item.DirPath, new DateTime(item.TimeCreated).ToString("R"));
                failureItem.NodeId = item.ExternalId;
                failureItem.ObjectName = item?.LeafName;
                failureItem.Message = this.GetExceptionMessage(e);
                Guid folderId = item.FolderId;
                lock (mLock)
                {
                    if (FailureItems.ContainsKey(folderId))
                    {
                        if (FailureItems[folderId].Count <= 200)
                        {
                            FailureItems[folderId].Add(failureItem);
                        }
                    }
                    else
                    {
                        FailureItems.TryAdd(folderId, new List<RMEXOSyncFailureItem>() { failureItem });
                    }
                }
            }
            catch (Exception ex)
            {
                JobManagement.HasErrorNode = true;
                logger.Error(ex.Message, ex);
            }
        }

        private Record GenerateRecordInfo(IExchangeItem item, Guid termId, Rule disposalRule, string dueDisposalTime, Record recordInDB, UniqueIdUtil idUtil)
        {
            using (new RA.Common.PerformanceScope(string.Format("EXO.RMEXOSyncDataJobBase.GenerateRecordInfo"), addToStatistics: true))
            {
                Record recoEntity = new Record();
                try
                {
                    _semaphore.WaitAsync().ExecuteAsyncTask();
                    var itemId = item.ItemId.ToMd5();
                    RecordMetaInfo metaInfo = new RecordMetaInfo
                    {
                        FileSize = item.ItemSize,
                        AttachmentNames = item.AttachmentNames,
                    };
                    var jsonStr = string.Empty;
                    using (new RA.Common.PerformanceScope("EXO.RMEXOSyncDataJobBase.GenerateRecordInfo.SerializeMetaInfo"))
                    {
                        jsonStr = JsonConvert.SerializeObject(metaInfo);
                    }
                    recoEntity = new Record()
                    {
                        #region init records entity
                        Id = IDGenerator.GetRecordId(MailboxAddress, item.ItemId),
                        ScopeId = AOSMailboxId,
                        NodeId = itemId,
                        DirPath = item.ItemPath,
                        FullPath = item.ItemPath,
                        LeafName = item.ItemName,
                        ExtensionForFile = "msg",//Confirm in Demo with Moses, we use msg here
                        AveSiteId = AOSMailboxId.ToString(),
                        WebId = Guid.Empty,
                        ListId = Guid.Empty,
                        ItemId = itemId,
                        CollectTime = DateTime.UtcNow.Ticks,
                        TimeCreated = item.SendDateUTC.Ticks,
                        NodeType = (int)NodeLevel.ExchangeOnlineItem,
                        TermId = termId,
                        TermName = TermIdAndNameMapping.ContainsKey(termId) ? TermIdAndNameMapping[termId] : string.Empty,
                        FolderId = item.ParentFolderId.ToMd5(),//to do next validate folder id
                                                               //folderRowId = aveItem.Folder.Item.ID,
                        MetaInfo = jsonStr,
                        HoldStatus = false,
                        RelatedRecords = "",
                        RelatedRecordsCount = 0,
                        SourceFlag = (int)SourceFlag.Exchange,
                        CreatedBy = item.SenderDisplayName, //item.Sender,
                        ModifiedBy = item.ModifiedBy,
                        DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(dueDisposalTime),
                        PreviosDisposalDueDate = DueDateUtil.ConvertStringDueDate2Long(dueDisposalTime),
                        DeclareAsRecord = false,
                        TimeModified = item.Modified.Ticks,
                        ItemRowId = 0,
                        RuleId = disposalRule != null ? new Guid(disposalRule.Id) : Guid.Empty,
                        RuleLevel = disposalRule != null ? (int)disposalRule.PolicyLevel : 0,
                        RecordStatus = (int)RMRecordStatus.Active,
                        //RecordOwner = ManualApproveDao.GetLastReviewedUserIds(AOSMailboxId, itemId),
                        RecordOwner = ReviewedUserIdsAndNodeIdMapping.ContainsKey(itemId) ? ReviewedUserIdsAndNodeIdMapping[itemId] : string.Empty,
                        ExternalId = item.ItemId,
                        EmailAddress = MailboxAddress,
                        SendTo = item.DisplayTo,
                        #endregion
                    };
                    using (new RA.Common.PerformanceScope(string.Format("EXO.RMEXOSyncDataJobBase.GenerateRecordInfo.GetItemUnique"), addToStatistics: true))
                    {
                        string recordsGlobalId = string.Empty;
                        using (new RA.Common.PerformanceScope(string.Format("EXO.RMEXOSyncDataJobBase.GenerateRecordInfo.GetItemUnique.ReadById")))
                        {
                            recordsGlobalId = recordInDB?.RecordsId;
                        }

                        if (string.IsNullOrEmpty(recordsGlobalId))
                        {
                            logger.Info("create new unique Id:{0}.", itemId);
                            recordsGlobalId = idUtil.GenerateUniqueId();
                            //UniqueIdGenerator.GenerateUniqueId(this.uniqueIdSetting);
                        }
                        recoEntity.RecordsId = recordsGlobalId;
                        recoEntity.CustomColumnDic = GetEXOCustomMetadata(item, recoEntity);
                    }
                    return recoEntity;
                }
                catch (Exception ex)
                {
                    logger.Error($"Error occurred while generate record info for item id: {item?.ItemId}, reason: {ex.ToString()}.");
                    throw;
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        private EXONodeFlag GenerateNodeFlag(IExchangeFolder folder, DateTime collectionTime)
        {
            EXONodeFlag nodeFlag = new EXONodeFlag();
            nodeFlag.CollectionTime = collectionTime.Ticks;
            nodeFlag.EmailAdress = folder.Mailbox.MailboxAddress;
            nodeFlag.AOSEmailboxId = AOSMailboxId;
            nodeFlag.FolderSyncState = folder.FolderSyncState;
            nodeFlag.FullPath = folder.DisplayFolderPath;
            nodeFlag.GroupId = GroupId;
            nodeFlag.IsRemoved = false;
            nodeFlag.ItemSyncState = folder.ItemSyncState;
            nodeFlag.NodeFlagType = (int)NodeFlagType.ExplorerSync;
            nodeFlag.NodeId = folder.IsRootFolder ? AOSMailboxId : folder.FolderId.ToMd5();
            nodeFlag.Title = folder.FolderName;
            nodeFlag.AOSObjectId = AOSObjectId;
            return nodeFlag;
        }

        private Dictionary<Guid, RMRuleItemCollection> GetTermAndRuleMappings()
        {
            List<RMTermRuleAssociation> trAssociations = TermRuleInfos.GetTermWithRule();
            Dictionary<int, List<Guid>> termRules = new Dictionary<int, List<Guid>>();
            foreach (var termId in trAssociations.Select(a => a.TermId).Distinct())
            {
                var rules = trAssociations
                    .Where(a => a.TermId == termId)
                    .OrderBy(a => a.RuleOrder)
                    .Select(a => a.RuleId)
                    .ToList();
                if (rules.Count > 0)
                {
                    termRules.Add(termId, rules);
                }
            }

            var termRuleMappings = new Dictionary<Guid, RMRuleItemCollection>();
            Dictionary<Guid, Rule> allRules = allRulesList.ToDictionary(r => new Guid(r.Id));//get rule from DA//RuleService.GetRulesFromDA().ToDictionary(r => new Guid(r.Id));
            var allHasRuleTerms = TermDao.GetRMTermsByTermIds(termRules.Keys.ToArray());
            foreach (var term in allHasRuleTerms)
            {
                if (term.IsRemoved)
                {
                    continue;
                }
                RuleCollection commonRules = new RuleCollection() { Rules = new Dictionary<int, Rule>() };
                List<RMRuleItem> rmRules = new List<RMRuleItem>();

                Rule rule;
                var ruleIds = termRules[term.Id];
                int reOrder = 0;
                for (int idx = 0; idx < ruleIds.Count; idx++)
                {
                    if (allRules.TryGetValue(ruleIds[idx], out rule))
                    {
                        if (rule.PolicyLevel != PolicyLevel.None)
                        {
                            reOrder++;
                            var ruleOBj = CloneSameRuleObject(rule);
                            commonRules.Rules.Add(reOrder, ruleOBj);
                        }
                    }
                }

                var refTerms = new List<RMTerm>();
                TermDao.GetAllInheritTermsByRootTerm(term.Id, ref refTerms);
                foreach (var refTerm in refTerms)
                {
                    RMRuleItemCollection tempRC;
                    if (!termRuleMappings.TryGetValue(refTerm.UniqueId, out tempRC))
                    {
                        tempRC = new RMRuleItemCollection
                        {
                            TermId = refTerm.UniqueId,
                            TermName = refTerm.Name
                        };
                        termRuleMappings.Add(refTerm.UniqueId, tempRC);
                    }

                    tempRC.CommonRules = commonRules;
                    tempRC.Rules = rmRules;

                }
            }

            return termRuleMappings;
        }

        public Rule CloneSameRuleObject(Rule rule)
        {
            string xml = SerializerHelper.SerializeByDataContractSerializer(rule);
            Rule result = SerializerHelper.DeserializeByDataContractSerializer<Rule>(xml);
            return result;
        }

        private RuleCollection RebuldDARules(RMRuleItemCollection rules)
        {
            RuleCollection newRuleCol = new RuleCollection();
            Dictionary<int, Rule> newRules = new Dictionary<int, Rule>();
            int reOrder = 0;
            foreach (var order in rules.CommonRules.Rules.Keys)
            {
                if (rules.CommonRules.Rules[order].PolicyLevel != PolicyLevel.None && rules.CommonRules.Rules[order].EXORule != null && rules.CommonRules.Rules[order].EXORule.SOFilters != null && rules.CommonRules.Rules[order].EXORule.SOFilters.Count > 0)
                {
                    reOrder++;
                    var commonRule = rules.CommonRules.Rules[order];
                    var rule = commonRule.EXORule;
                    rule.Id = commonRule.Id;
                    //var newRule = ruleAssembler.ConvertToSPRule(rule);
                    newRules.Add(order, rule);
                }
            }
            newRuleCol.Rules = newRules;
            return newRuleCol;
        }

        #region Dashboard Sync Incremental Logic
        private async System.Threading.Tasks.Task ProcessIncrementalTermUsagesForAddAsync(Record rec, string subJobId)
        {
            try
            {
                if (rec.TermId != null && rec.TermId != Guid.Empty)
                {
                    var type = 1;
                    lock (_memoryLocker.GetLocker($"{rec.TermId}_{subJobId}_{type}"))
                    {
                        var tempCache = BoardCacheDao.GetFilterList(s => new { Id = s.Id, TermId = s.TermId, Size = s.Size }, d => d.TermId == rec.TermId && d.SubJobId == subJobId && d.Type == type).FirstOrDefault();
                        if (tempCache != null)
                        {
                            _= BoardCacheDao.UpdateAsync(new RMBoardCache()
                            {
                                Id = tempCache.Id,
                                TermId = tempCache.TermId,
                                Size = tempCache.Size + 1,
                                SubJobId = subJobId,
                                Type = type
                            }).Result;
                        }
                        else
                        {
                            BoardCacheDao.Create(new RMBoardCache()
                            {
                                TermId = rec.TermId,
                                Size = 1,
                                SubJobId = subJobId,
                                Type = type
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Process incremental term usages for add failed. Error: {0}", ex.ToString());
            }
        }

        private void ProcessIncrementalDataOfDaysForAdd(Record rec, string subJobId)
        {
            try
            {
                if (rec.TimeCreated != 0)
                {
                    var date = ConvertToShortTime(rec.TimeCreated);
                    var dater = ConvertDateTimeToTicks(date);
                    var type = 2;
                    lock (_memoryLocker.GetLocker($"{dater}_{subJobId}_{type}"))
                    {
                        var tempCache = BoardCacheDao.GetFilterList(s => new { Id = s.Id, Dater = s.Dater, Size = s.Size }, d => d.Dater == dater && d.SubJobId == subJobId && d.Type == type).FirstOrDefault();
                        if (tempCache != null)
                        {
                            _=BoardCacheDao.UpdateAsync(new RMBoardCache()
                            {
                                Id = tempCache.Id,
                                Dater = tempCache.Dater,
                                Size = tempCache.Size + 1,
                                SubJobId = subJobId,
                                Type = type
                            }).Result;
                        }
                        else
                        {
                            BoardCacheDao.Create(new RMBoardCache()
                            {
                                Dater = dater,
                                Size = 1,
                                SubJobId = subJobId,
                                Type = type
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Process incremental DataOfDays for add failed. Error: {0}", ex.ToString());
            }
        }

        private async System.Threading.Tasks.Task ProcessIncrementalTotalsForAddAsync(Record rec, string subJobId)
        {
            try
            {
                var type = 4;
                lock (_memoryLocker.GetLocker($"{subJobId}_{type}"))
                {
                    var tempCache = BoardCacheDao.GetFilterList(s => new { Id = s.Id, Size = s.Size }, d => d.SubJobId == subJobId && d.Type == type).FirstOrDefault();
                    if (tempCache != null)
                    {
                        _=BoardCacheDao.UpdateAsync(new RMBoardCache()
                        {
                            Id = tempCache.Id,
                            Size = tempCache.Size + 1,
                            SubJobId = subJobId,
                            Type = type
                        }).Result;
                    }
                    else
                    {
                        BoardCacheDao.Create(new RMBoardCache()
                        {
                            Size = 1,
                            SubJobId = subJobId,
                            Type = type
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Process incremental totals for add failed. Error: {0}", ex.ToString());
            }
        }

        private async System.Threading.Tasks.Task ProcessIncrementalTermUsagesForUpdateAsync(Record rec, Guid currentDbTermId, string subJobId)
        {
            //需要判断TermId是否发生了变化...
            //当前TermId == DB中的TermId, 需要判断本次收集Job时， Record是否存在History Reclassify的操作. 若存在说明有做过Reclassify相关操作, 需要找到原始Term并-1处理, 不存在则不处理
            //若当前TermId != DB中的TermId, 需要到Reclassify操作对应的临时表中找最原始的TermId，并将该TermId的Size - 1, 当前Record关联的TermId的Size + 1, 
            //若Reclassify操作对应的临时表没有该记录相关信息, 则将该Record在Explorer DB中的TermId的Size - 1.
            //找到后删除该Record相关的Reclassify操作记录;
            try
            {
                var previousTermId = Guid.Empty;
                var currentTermId = rec.TermId;
                var tempHistories = await ClassificationHistoryDao.FindListAsync(d => d.RecordId == rec.Id);
                var tempHistory = tempHistories.OrderBy(j => j.OperationTime).FirstOrDefault();
                if (tempHistory != null)
                {
                    previousTermId = tempHistory.PreviousTermId;
                    //Delete Classification History
                    ClassificationHistoryDao.BatchDelete(tempHistories);
                }
                else
                {
                    if (currentTermId != currentDbTermId)
                    {
                        previousTermId = currentDbTermId;
                    }
                }
                if (previousTermId != Guid.Empty)
                {

                    var type = 1;
                    lock (_memoryLocker.GetLocker($"{previousTermId}_{subJobId}_{type}"))
                    {
                        //Previous Term - 1
                        var tempCache1 = BoardCacheDao.GetFilterList(s => new { Id = s.Id, TermId = s.TermId, Size = s.Size }, d => d.TermId == previousTermId && d.SubJobId == subJobId && d.Type == type).FirstOrDefault();
                        if (tempCache1 != null)
                        {
                             _=BoardCacheDao.UpdateAsync(new RMBoardCache()
                            {
                                Id = tempCache1.Id,
                                TermId = tempCache1.TermId,
                                Size = tempCache1.Size - 1,
                                SubJobId = subJobId,
                                Type = type
                            }).Result;
                        }
                        else
                        {
                            BoardCacheDao.Create(new RMBoardCache()
                            {
                                TermId = previousTermId,
                                Size = -1,
                                SubJobId = subJobId,
                                Type = type
                            });
                        }
                    }

                    lock (_memoryLocker.GetLocker($"{currentTermId}_{subJobId}_{type}"))
                    {
                        //Current Term + 1
                        var tempCache2 = BoardCacheDao.GetFilterList(s => new { Id = s.Id, TermId = s.TermId, Size = s.Size }, d => d.TermId == currentTermId && d.SubJobId == subJobId && d.Type == type).FirstOrDefault();
                        if (tempCache2 != null)
                        {
                            _=BoardCacheDao.UpdateAsync(new RMBoardCache()
                            {
                                Id = tempCache2.Id,
                                TermId = tempCache2.TermId,
                                Size = tempCache2.Size + 1,
                                SubJobId = subJobId,
                                Type = type
                            }).Result;
                        }
                        else
                        {
                            BoardCacheDao.Create(new RMBoardCache()
                            {
                                TermId = currentTermId,
                                Size = 1,
                                SubJobId = subJobId,
                                Type = type
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Process incremental term usages for update failed. Error: {0}", ex.ToString());
            }
        }

        private async System.Threading.Tasks.Task ProcessIncrementalTermUsagesForDeleteRecordAsync(Record rec, string subJobId)
        {
            try
            {
                //数据经过多次Classify操作后删除，再收集会如何??
                var previousTermId = Guid.Empty;
                var tempHistories = await ClassificationHistoryDao.FindListAsync(d => d.RecordId == rec.Id);
                var tempHistory = tempHistories.OrderBy(j => j.OperationTime).FirstOrDefault();
                if (tempHistory != null)
                {
                    previousTermId = tempHistory.PreviousTermId;
                    //Delete Classification History
                    ClassificationHistoryDao.BatchDelete(tempHistories);
                }
                else
                {
                    previousTermId = rec.TermId;
                }
                //Previous Term - 1
                var tempCache = BoardCacheDao.GetFilterList(s => new { Id = s.Id, TermId = s.TermId, Size = s.Size }, d => d.TermId == previousTermId && d.SubJobId == subJobId && d.Type == 1).FirstOrDefault();
                if (tempCache != null)
                {
                    await BoardCacheDao.UpdateAsync(new RMBoardCache()
                    {
                        Id = tempCache.Id,
                        TermId = tempCache.TermId,
                        Size = tempCache.Size - 1,
                        SubJobId = subJobId,
                        Type = 1
                    });
                }
                else
                {
                    BoardCacheDao.Create(new RMBoardCache()
                    {
                        TermId = previousTermId,
                        Size = -1,
                        SubJobId = subJobId,
                        Type = 1
                    });
                }
            }
            catch (Exception ex)
            {
                logger.Error("Process incremental term usages for delete record failed. Error: {0}", ex.ToString());
            }
        }

        private async System.Threading.Tasks.Task ProcessIncrementalDataOfDaysForDeleteRecordAsync(Record rec, string subJobId)
        {
            try
            {
                //若当前Exploer DB中的记录的Record Status为Archiverd状态, 则将更新前的DestroyedTime属性转换为ConverToShortTime的Ticks作为Dater添加到临时表中，Type为3 (若同Dater记录存在， 原Size + 1，否则记录1)
                if (rec.RecordStatus == (int)RMRecordStatus.Destroyed || rec.RecordStatus == (int)RMRecordStatus.Moved)
                {
                    var date = ConvertToShortTime(rec.DestroyedTime);
                    var dater = ConvertDateTimeToTicks(date);
                    var tempCache = BoardCacheDao.GetFilterList(s => new { Id = s.Id, Dater = s.Dater, Size = s.Size }, d => d.Dater == dater && d.SubJobId == subJobId && d.Type == 3).FirstOrDefault();
                    if (tempCache != null)
                    {
                        await BoardCacheDao.UpdateAsync(new RMBoardCache()
                        {
                            Id = tempCache.Id,
                            Dater = tempCache.Dater,
                            Size = tempCache.Size + 1,
                            SubJobId = subJobId,
                            Type = 3
                        });
                    }
                    else
                    {
                        BoardCacheDao.Create(new RMBoardCache()
                        {
                            Dater = dater,
                            Size = 1,
                            SubJobId = subJobId,
                            Type = 3
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Process incremental DataOfDays for delete record failed. Error: {0}", ex.ToString());
            }
        }

        private async System.Threading.Tasks.Task ProcessIncrementalTotalsForDeleteRecordAsync(Record rec, string subJobId)
        {
            try
            {
                //若当前Exploer DB中记录的Record Status为Active状态, Type为4的Size - 1, 
                //若Status为Archive状态, Type为4的Size - 1， Type为5的Size + 1
                if (rec.RecordStatus == (int)RMRecordStatus.Active || rec.RecordStatus == (int)RMRecordStatus.Destroyed || rec.RecordStatus == (int)RMRecordStatus.Moved)
                {
                    var tempCache1 = BoardCacheDao.GetFilterList(s => new { Id = s.Id, Size = s.Size }, d => d.SubJobId == subJobId && d.Type == 4).FirstOrDefault();
                    if (tempCache1 != null)
                    {
                        await BoardCacheDao.UpdateAsync(new RMBoardCache()
                        {
                            Id = tempCache1.Id,
                            Size = tempCache1.Size - 1,
                            SubJobId = subJobId,
                            Type = 4
                        });
                    }
                    else
                    {
                        BoardCacheDao.Create(new RMBoardCache()
                        {
                            Size = -1,
                            SubJobId = subJobId,
                            Type = 4
                        });
                    }
                    if (rec.RecordStatus == (int)RMRecordStatus.Destroyed || rec.RecordStatus == (int)RMRecordStatus.Moved)
                    {
                        var tempCache2 = BoardCacheDao.GetFilterList(s => new { Id = s.Id, Size = s.Size }, d => d.SubJobId == subJobId && d.Type == 5).FirstOrDefault();
                        if (tempCache2 != null)
                        {
                            await BoardCacheDao.UpdateAsync(new RMBoardCache()
                            {
                                Id = tempCache2.Id,
                                Size = tempCache2.Size + 1,
                                SubJobId = subJobId,
                                Type = 5
                            });
                        }
                        else
                        {
                            BoardCacheDao.Create(new RMBoardCache()
                            {
                                Size = 1,
                                SubJobId = subJobId,
                                Type = 5
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Process incremental totals for delete record failed. Error: {0}", ex.ToString());
            }
        }

        private string ConvertToShortTime(long ticks)
        {
            var time = new DateTime(ticks);
            return time.ToString("d");
        }
        private long ConvertDateTimeToTicks(string timeString)
        {
            DateTime time = Convert.ToDateTime(timeString);
            return time.Ticks;
        }
        #endregion

        public static ExchangeOnlineTreeNodeDto GetGroupNode(ExchangeOnlineTreeNodeDto node)
        {
            while (node != null && (node.Level != NodeLevel.ExchangeOnlineMailboxGroup && node.Level != NodeLevel.ExchangeOnlineO365GroupGroup))
            {
                node = node.Parent;
            }
            return node;
        }


        #region Index metadata column

        private async System.Threading.Tasks.Task LoadCustomIndexMetadataAsync()
        {
            try
            {
                if (!_keyValueDao.TryGetBoolValue(AvePoint.RA.Contract.Common.KeyNameCollection.IsEnableCustomIndexMetadata, out var isEnabled) || !isEnabled)
                {
                    logger.Info("Custom index metadata is disabled. Skipping load.");
                    return;
                }

                _customIndexMetadatas = (await _customIndexMetadataDao.GetCustomIndexMetadatasBySourceFlagAsync(SourceFlag.Exchange)).ToList();
                _customMetadataColumns = (await _customMetadataColumnDao.GetAllCustomMetadataColumnsAsync()).ToList();
                logger.Info($"Loaded {_customIndexMetadatas.Count} custom index metadata mappings for Exchange.");
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to load custom index metadata. Error: {ex}");
            }
        }

        private Dictionary<string, CustomColumn> GetEXOCustomMetadata(IExchangeItem item, Record record)
        {
            var dic = new Dictionary<string, CustomColumn>();
            if (_customIndexMetadatas == null || _customIndexMetadatas.Count == 0)
            {
                return dic;
            }

            logger.Debug($"Start extracting {_customIndexMetadatas.Count} custom columns for item: [{record.ItemId}]");

            foreach (var mapping in _customIndexMetadatas)
            {
                try
                {
                    var columnInfo = _customMetadataColumns.FirstOrDefault(c => c.UniqueId == mapping.TargetColumnId);
                    if (columnInfo == null)
                    {
                        logger.Warn($"Target column not found for mapping: {mapping.SourceColumnName}");
                        continue;
                    }

                    var value = GetGraphItemPropertyValue(item, mapping.SourceColumnName);
                    if (value == null)
                    {
                        logger.Warn($"Cannot get value for Graph column [{mapping.SourceColumnName}].");
                        record.CustomColumnNotExist = true;
                        continue;
                    }

                    logger.Debug($"Successfully extracted [{mapping.SourceColumnName}] for item: [{record.ItemId}]");

                    dic[columnInfo.UniqueId.ToString()] = BuildCustomColumn(columnInfo, mapping.SourceColumnName, value);
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to get custom column [{mapping.SourceColumnName}]. Error: {ex}");
                    record.CustomColumnNotExist = true;
                }
            }

            logger.Debug($"Finished extracting. Successfully mapped {dic.Count} columns for item: [{record.ItemId}]");

            return dic;
        }

        private object GetGraphItemPropertyValue(IExchangeItem item, string sourceColumnName)
        {
            switch (sourceColumnName.ToLowerInvariant().Trim())
            {
                case "attachment" or "has attachment" or "hasattachment" or "hasattach":
                    return item.HasAttach;
                case "size" or "itemsize":
                    return (object)item.ItemSize;
                case "sent time" or "sent" or "senddateutc":
                    return item.SendDateUTC;
                //case "received time" or "received":
                //    return item.Received;
                case "created date" or "created":
                    return item.Created;
                case "from" or "sender":
                    return item.SenderEmailAddress;
                case "cc" or "displaycc":
                    return item.DisplayCc;
                case "importance":
                    return ConvertImportanceToString(item.Importance);
                case "retention label" or "retentionlabel":
                    return item.RetentionLabel;
            }

            var props = item.GetProperties();
            var dictKey = MapSourceColumnToPropertiesKey(sourceColumnName);
            if (dictKey != null && props.TryGetValue(dictKey, out var value) && !string.IsNullOrEmpty(value))
            {
                return value;
            }

            return null;
        }

        private static string MapSourceColumnToPropertiesKey(string sourceColumnName) =>
            sourceColumnName.ToLowerInvariant().Trim() switch
            {
                "subject" => "Subject",
                "conversation" or "conversationtopic" => "Conversation",
                "from" or "sender" or "fromemail" => "From",
                "to" or "displayto" => "To",
                "cc" or "displaycc" => "Cc",
                "recipient name" or "recipientname" => "Recipient Name",
                "email account" or "emailaccount" => "Email Account",
                "received representing name" or "receivedrepresentingname" => "Received Representing Name",
                "sensitivity" => "Sensitivity",
                "importance" => "Importance",
                "flag status" or "flagstatus" => "Flag Status",
                "flag start date" or "flagstartdate" or "start date" => "Start Date",
                "flag due date" or "flagduedate" or "due date" => "Due Date",
                "size" or "itemsize" => "Size",
                "sent time" or "sent" or "senddateutc" => "Sent",
                "received time" or "received" => "Received",
                "created date" or "created" => "Created",
                _ => null
            };

        private static string ConvertImportanceToString(int importance) => importance switch
        {
            0 => "Low",
            1 => "Normal",
            2 => "High",
            _ => importance.ToString()
        };

        private CustomColumn BuildCustomColumn(RMCustomMetadataColumn column, string sourceColumnName, object value)
        {
            var customColumn = new CustomColumn();
            switch (column.ColumnType)
            {
                case CustomColumnType.SingleText:
                    customColumn.Value = value?.ToString() ?? string.Empty;
                    customColumn.Value_Array = customColumn.Value.ExplorerAnalyzeBuiltInColumn();
                    return customColumn;

                case CustomColumnType.Number:
                    if (!double.TryParse(value.ToString(), out var numberValue))
                    {
                        throw new Exception($"Cannot parse Number value for column [{sourceColumnName}].");
                    }
                    customColumn.Value = numberValue.ToString();
                    customColumn.Number = numberValue;
                    customColumn.Value_Array = customColumn.Value.ExplorerAnalyzeBuiltInColumn();
                    return customColumn;

                case CustomColumnType.YesOrNo:
                    bool boolValue;
                    if (value is bool b)
                    {
                        boolValue = b;
                    }
                    else if (!bool.TryParse(value.ToString(), out boolValue))
                    {
                        throw new Exception($"Cannot parse YesOrNo value for column [{sourceColumnName}].");
                    }
                    customColumn.Value = boolValue.ToString();
                    customColumn.YesOrNo = boolValue ? "Yes" : "No";
                    return customColumn;

                case CustomColumnType.DateTime:
                    DateTime dateTimeValue;
                    if (value is DateTime dt)
                    {
                        dateTimeValue = dt.Kind == DateTimeKind.Utc ? dt : dt.ToUniversalTime();
                    }
                    else if (value is string dateStr
                        && DateTime.TryParse(dateStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsed)
                        && parsed != DateTime.MinValue)
                    {
                        dateTimeValue = parsed.Kind == DateTimeKind.Utc ? parsed : parsed.ToUniversalTime();
                    }
                    else
                    {
                        throw new Exception($"Cannot parse DateTime value for column [{sourceColumnName}].");
                    }
                    var timeColumn = new DateTimeColumnValue() { Date = dateTimeValue, TimeZoneId = "UTC" };
                    customColumn.Value = JsonConvert.SerializeObject(timeColumn);
                    customColumn.Date = dateTimeValue;
                    customColumn.TimeZoneId = "UTC";
                    return customColumn;

                default:
                    return customColumn;
            }
        }

        #endregion
    }
}
