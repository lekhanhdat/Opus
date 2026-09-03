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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RAExchange.Common;
using AvePoint.RA.RAExchange.Discover;
using AvePoint.Records.Core.Utilities.Extensions;
using AvePoint.Wrapper.Common;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using ExchangeBackupUtility.Graph;
using ExchangeFolder = ExchangeBackupUtility.ExchangeFolder;
using ExchangeItemBulkHelper = ExchangeBackupUtility.ExchangeItemBulkHelper;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.RAExchange.Disposal.Common;

namespace AvePoint.RA.RAExchange.EnforceRetention
{
    public class EXOEnforceRetentionProcessor
    {
        protected static readonly RALogger mLog = RALogger.GetInstance(typeof(EXOEnforceRetentionProcessor));
        protected List<ExchangeOnlineTreeNodeDto> nodes = new List<ExchangeOnlineTreeNodeDto>();
        protected string mCachedNodeNameForPath = string.Empty;
        protected readonly static object obj = new object();
        //EXOLabel for this job.
        protected string mGlobalCurrentEXOLabelName = string.Empty;
        protected int mGlobalCurrentEXOLabelId = 0;
        protected Guid mGlobalCurrentEXOLabelGuid = Guid.Empty;
        //EXOLabel for previous job.
        protected bool mGlobalEXOLabelProcessed = false;
        protected List<int> mGlobalPreviousEXOLabelIds = null;
        protected List<Guid> mGlobalPreviousEXOLabelGuids = null;

        protected bool mJobHasException = false;
        protected bool mJobHasStopped = false;
        protected bool mNeedFullJobForMailBox = false;
        protected bool mNeedIncrementalJobForMailBox = false;
        private List<Guid> mFailedFolderList = new List<Guid>();
        /// <summary>
        /// 旧的ID，可能是DAOTreeNodeID，也可能是GUID的AOS MailboxID(经过特殊处理满足Records GUID格式需求的ID)
        /// </summary>
        protected Guid AOSMailboxId = Guid.Empty;
        /// <summary>
        /// AOS AOS真正的Mailbox Object ID，类型为String
        /// </summary>
        protected string AOSObjectId = string.Empty;
        protected string MailboxAddress = string.Empty;
        protected Guid groupId = Guid.Empty;
        protected int MaxBackupItemsThreads { get; private set; } = 25;
        private static Semaphore mWorkerThreads = new Semaphore(2, 2);
        protected IExchangeFolder CurrentFolder { get; set; }
        protected Dictionary<string, Guid> RetentionLabel = null;
        private IRMSubJobDao SubJobDao { set; get; }
        private IBatchDiscoverV2 searchDiscover = null;
        private IBatchDiscoverV2 incrementalDiscover = null;
        private RMEXODiscoverHelper discoverHelper = null;
        private IRMReportManager mReportManger;

        private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
        protected bool IsSupportGraphApi { get; set; }

        public IRMReportManager ReportManager
        {
            get
            {
                if (mReportManger == null)
                {
                    mReportManger = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManger;
            }
        }
        private IEXONodeFlagDao mEXONodeFlagDao;
        protected IEXONodeFlagDao EXONodeFlagDao
        {
            get
            {
                if (mEXONodeFlagDao == null)
                {
                    mEXONodeFlagDao = (IEXONodeFlagDao)PlatformWindsorManager.GetService(typeof(IEXONodeFlagDao));
                }
                return mEXONodeFlagDao;
            }
        }
        public ITermDao mTermDao;
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
        public IRMEXOLabelDao mEXOLabelDao;
        protected IRMEXOLabelDao EXOLabelDao
        {
            get
            {
                if (mEXOLabelDao == null)
                {
                    mEXOLabelDao = (IRMEXOLabelDao)PlatformWindsorManager.GetService(typeof(IRMEXOLabelDao));
                }
                return mEXOLabelDao;
            }
        }
        public EXOEnforceRetentionProcessor(string jobId)
        {
            ReportMangerFactory.Instance.Init(jobId, JobType.EnforceRetention);
            InitFromConfig();
            ReportManager.StartUpdateJobProgress();
            mGlobalPreviousEXOLabelIds = new List<int>();
            mGlobalPreviousEXOLabelGuids = new List<Guid>();
            //上次的Label信息
            var tempProcessedLabels = EXOLabelDao.GetLabelByStatusAndType((int)RMRetentionLabelStatus.Previous, (int)RMRetentionSourceType.Exchange);
            if (tempProcessedLabels != null && tempProcessedLabels.Count > 0)
            {
                mGlobalEXOLabelProcessed = true;
                mGlobalPreviousEXOLabelIds = tempProcessedLabels.Select(t => t.Id).ToList();
                mGlobalPreviousEXOLabelGuids = tempProcessedLabels.Select(t => t.LabelId).ToList();
            }

            //本次的Label信息, 优先取中间状态
            var tempCurrentLabel = EXOLabelDao.GetLabel((int)RMRetentionSourceType.Exchange, (int)RMRetentionLabelStatus.JobProcessing);
            if (tempCurrentLabel != null)
            {
                mGlobalCurrentEXOLabelId = tempCurrentLabel.Id;
                mGlobalCurrentEXOLabelName = tempCurrentLabel.LabelName;
            }
            else
            {
                tempCurrentLabel = EXOLabelDao.GetLabel((int)RMRetentionSourceType.Exchange, (int)RMRetentionLabelStatus.FromGUI);
                if (tempCurrentLabel != null)
                {
                    mGlobalCurrentEXOLabelId = tempCurrentLabel.Id;
                    mGlobalCurrentEXOLabelName = tempCurrentLabel.LabelName;
                }
            }

            //不论是新修改的Label信息, 亦或是上次跑完的Label信息, 至少拿到Label信息才能继续往下执行.
            if (mGlobalCurrentEXOLabelId != 0)
            {
                SubJobDao = (IRMSubJobDao)PlatformWindsorManager.GetService(typeof(IRMSubJobDao));
                RMSubJob subJobWithContext = SubJobDao.GetSubJob(jobId, true);

                List<RMEXOTreeNode> tempList = SerializerHelper.DeserializeByDataContractSerializer<List<RMEXOTreeNode>>(subJobWithContext.JobContext.Settings);
                tempList.ForEach(node => nodes.Add(RMDtoConverter.ConvertRMExchangeTree2TreeNodeDto(node)));

                discoverHelper = new RMEXODiscoverHelper();
            }
        }

        private void InitFromConfig()
        {
            try
            {
                //this.EnableBulkGenerateItems = bool.Parse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.EXO_ENABLE_BULK_GENERATE_ITEMS]);
                this.MaxBackupItemsThreads = int.Parse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.EXO_DISCOVER_THREADS_LIMIT]);
                //this.MaxBulkItemsCount = int.Parse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.EXO_BULK_ITEMS_COUNT_LIMIT]);
                //this.MaxBulkItemSize = int.Parse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.EXO_BULK_ITEMS_SIZE_LIMIT]);
            }
            catch (Exception ex)
            {
                mLog.Error($"An exception occurred while trying to get the configuration, reason:{ex.ToString()}. Set the value to default.");
                //this.MaxRestoreItemsThreads = 2;
                //this.MinRestoreItemsThreads = 1;
                //this.MaxTotalSizeOnDownload = 20;
                //this.EnableBulkGenerateItems = true;
                //this.MaxBulkItemsCount = 50;
                //this.MaxBulkItemSize = 20;
                MaxBackupItemsThreads = 3;
                //this.SetApplicationImpersonation = true;
                //this.EWSMonitorMode = 3;
                //this.EWSMonitorInterval = 300;
            }
        }

        public void RunNow()
        {
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    //不论是新修改的Label信息, 亦或是上次跑完的Label信息, 至少拿到Label信息才能继续往下执行.
                    if (mGlobalCurrentEXOLabelId != 0)
                    {
                        if (nodes != null && nodes.Count > 0)
                        {
                            foreach (var node in nodes)
                            {
                                try
                                {
                                    mCachedNodeNameForPath = node.Name;
                                    Process(node);
                                }
                                catch (JobStopException ex)
                                {
                                    mJobHasStopped = true;
                                    throw new JobStopException("This Job is stopped.");
                                }
                                catch (Exception ex)
                                {
                                    mJobHasException = true;
                                    AddFailedDetailForMailbox(mCachedNodeNameForPath, mCachedNodeNameForPath, JobDetailsStatus.Failed, ex.Message);
                                    mLog.Error($"Error in process node: {node?.Name}, reason: {ex.ToString()}.");
                                }
                            }
                        }
                        else
                        {
                            mLog.Info("Tree node is null.");
                        }
                    }
                    else
                    {
                        mLog.Info("None global exo label setting.");
                    }
                }
            }
            catch (JobStopException ex)
            {
                mJobHasStopped = true;
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while run exchange online enforce retention job. Exception: [{0}]", e.ToString());
                throw;
            }
            finally
            {
                var finalStatus = JobStatus.Finished;
                if (mJobHasException)
                {
                    finalStatus = JobStatus.FinishWithException;
                }
                if (mJobHasStopped)
                {
                    finalStatus = JobStatus.Stopped;
                }
                ReportManager.SetJobFinished(finalStatus);
            }
        }

        public void Process(ExchangeOnlineTreeNodeDto node)
        {
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    if (node.Level == NodeLevel.ExchangeOnlineMailbox)
                    {
                        Init(node);
                        groupId = new Guid(TreeManagement.GetGroupNode(node).ID);
                        long lastJobStartTime = DateTime.MinValue.Ticks;
                        lastJobStartTime = GetCollectionTimeNew(node);
                        EXOEnforceRetentionDataCache.Instance.CacheTermChange(lastJobStartTime);
                        //AOSMailboxId = new Guid(TreeManagement.GetMailboxNode(node).ID);
                        ExtendedPropertyDefinition extendedPropertyDefinition = new ExtendedPropertyDefinition(TermColumnInfo.WellKnowTermColumnGuid, TermColumnInfo.WellKnowTermColumnId, MapiPropertyType.String);
                        var searchFilter = new SearchFilter.Exists(extendedPropertyDefinition);
                        if (EXOEnforceRetentionDataCache.Instance.TermDeclarationMapping.Count > 0)
                        {
                            mLog.Info("Run retention full job for mailbox:{0}", node.FullPath);
                            mNeedFullJobForMailBox = true;
                            searchDiscover = EXODiscoverFactoryV2.CreateFactory(EXODiscoverType.Search, NodeFlagType.EnforceRetention, Guid.Empty, searchFilter);
                        }
                        else if (lastJobStartTime != DateTime.MinValue.Ticks)
                        {
                            mLog.Info("Run retention incremental job for mailbox:{0}", node.FullPath);
                            mNeedIncrementalJobForMailBox = true;
                            incrementalDiscover = EXODiscoverFactoryV2.CreateFactory(EXODiscoverType.Search, NodeFlagType.EnforceRetention, Guid.Empty, searchFilter);
                        }
                        ProcessFolder(CurrentFolder);
                        EXOEnforceRetentionDataCache.Instance.ClearData();
                    }
                }
            }
            catch (JobStopException ex)
            {
                mLog.Info("Job Stopped");
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception ex)
            {
                mLog.Error("An error occurred while farm process. Name: [{0}], error message : {1}.", node?.Name, ex.ToString());
                throw;
            }
            finally
            {
                mNeedFullJobForMailBox = false;
                mNeedIncrementalJobForMailBox = false;
            }
        }

        private void AddFailedDetailForMailbox(string name, string fullPath, JobDetailsStatus status, string comments = "")
        {
            JMEnforceRetentionJobDetail detail = new JMEnforceRetentionJobDetail();
            detail.ObjectName = name;
            detail.SourceURL = fullPath;
            detail.Status = status;
            detail.Comment = EXOCommonUtil.ProcessJobDetailMessage(comments, JobDetailsStatus.Failed);
            if (!detail.Comment.Equals("RM_Aos_CustomApp_Permission") && !detail.Comment.Equals("RM_EXODisposal_Exception_TimeOut"))
            {
                detail.Comment = "RM_Connector_InsertDatasFailed";
            }
            ReportManager.SendJobDetail(detail);
        }
       

        /// <summary>
        /// Enforce Retention外围Incremental逻辑和Data Sync/Apply Setting 逻辑不一致。
        /// Data Sync/Apply Setting会把所有Mailbox 节点都写入到EXONodeFlag表，会根据具体的Node查询记录。
        /// Enforce Retention目前只会把有数据的Mailbox Folder写入到EXONodeFlag表，然后仅通过MailboxID查询，查出所有记录的第一条当做Mailbox的记录。
        /// 1.新数据，直接通过AOSMailboxID和AOSObjectId取对应Mailbox记录.        
        /// 2.兼容旧数据升级逻辑，先用DAOTreeNodeID获取DB中旧记录，如果有则用旧记录并删除旧记录.
        /// </summary>
        protected long GetCollectionTime(string mailboxId)
        {

            long collectionTime = EXONodeFlagDao.GetCollectionTime((int)NodeFlagType.EnforceRetention, AOSMailboxId, MailboxAddress);
            if (collectionTime != DateTime.MinValue.Ticks)
            {
                mLog.Info($"Current get CollectionTime:{collectionTime} by AOSMailboxId when EXO enforce retention.AOSMailboxId:{AOSMailboxId}.MailboxAddress:{MailboxAddress}.");
                return collectionTime;
            }
            else
            {
                var mEXONodeFlag = EXONodeFlagDao.GetEXONodeInfo(new Guid(mailboxId), groupId, (int)NodeFlagType.EnforceRetention);
                if (mEXONodeFlag != null)
                {
                    collectionTime = EXONodeFlagDao.GetCollectionTime((int)NodeFlagType.EnforceRetention, new Guid(mailboxId), MailboxAddress);
                    mLog.Info($"Current get CollectionTime by DAOTreeNodeID when EXO enforce retention.CollectionTime:{collectionTime}.DAOTreeNodeID:{mailboxId}.MailboxAddress:{MailboxAddress}.");
                    return collectionTime;
                }
                else
                {
                    mLog.Info($"Current CollectionTime can not be get by DAOTreeNodeID & AOSMailboxId when EXO enforce retention.AOSMailboxId:{AOSMailboxId}.DAOTreeNodeID:{mailboxId}.groupId:{groupId}.AOSObjectId:{AOSObjectId}.MailboxAddress:{MailboxAddress}.");
                    return DateTime.MinValue.Ticks;
                }
            }
        }

        private long GetCollectionTimeNew(ExchangeOnlineTreeNodeDto treeNode)
        {
            TreeManagement treeManagement = new TreeManagement();
            string AOSObjectId = treeManagement.GetAOSObjectId(treeNode);
            string AOSMailboxId = treeManagement.GetRealMailboxGuid(treeNode);
            var mailboxId = new Guid(TreeManagement.GetMailboxNode(treeNode).ID);
            var mAOSEXONodeFlag = EXONodeFlagDao.GetEXONodeInfoByAOSMailboxIdAndObjectId(new Guid(AOSMailboxId), groupId, (int)NodeFlagType.EnforceRetention, AOSObjectId);
            if (mAOSEXONodeFlag != null)
            {
                DateTime collectionTime = new DateTime(mAOSEXONodeFlag.CollectionTime);
                mLog.Info($"Current get CollectionTime:{collectionTime} by AOSMailboxId when EXO sync data processer.AOSMailboxId:{AOSMailboxId}.DAOTreeNodeID:{mailboxId}.groupId:{groupId}.AOSObjectId:{AOSObjectId}.");
                return mAOSEXONodeFlag.CollectionTime;
            }
            else
            {
                var mEXONodeFlag = EXONodeFlagDao.GetEXONodeInfo(mailboxId, groupId, (int)NodeFlagType.EnforceRetention);
                if (mEXONodeFlag != null)
                {
                    DateTime collectionTime = new DateTime(mEXONodeFlag.CollectionTime);
                    mLog.Info($"Current get CollectionTime by DAOTreeNodeID when EXO sync data processer.CollectionTime:{collectionTime}.DAOTreeNodeID:{mailboxId}.groupId:{groupId}.");
                    //EXONodeFlagDao.DeleteEXONodeInfo(new Guid(mailboxId), groupId, (int)NodeFlagType.EnforceRetention);
                    return mEXONodeFlag.CollectionTime;
                }
                else
                {
                    mLog.Info($"Current CollectionTime can not be get by DAOTreeNodeID & AOSMailboxId when EXO sync data processer.AOSMailboxId:{AOSMailboxId}.groupId:{groupId}.AOSObjectId:{AOSObjectId}.");
                    return DateTime.MinValue.Ticks;
                }
            }
        }

        private void Init(ExchangeOnlineTreeNodeDto TreeNodeDto)
        {
            using (PerformanceScope scope = new PerformanceScope("EXOEnforceRetentionProcessor.Init"))
            {
                TreeManagement tm = new TreeManagement();
                MailboxAddress = TreeManagement.GetMailboxNode(TreeNodeDto).Name;
                IsSupportGraphApi = EXOGraphApiResolver.ShouldUseGraph(_keyValueDao, MailboxAddress, tm.GetRealMailboxStringId(TreeNodeDto), TreeNodeDto);
                var mailboxGuid = tm.GetRealMailboxGuid(TreeNodeDto);
                CurrentFolder = tm.GetExchangeFolderFromTreeNodeV2(TreeNodeDto, mailboxGuid, IsSupportGraphApi);
                if (RetentionLabel != null)
                {
                    RetentionLabel.Clear();
                }
                RetentionLabel = CurrentFolder.GetRetentionLabelDic();
                AOSMailboxId = new Guid(tm.GetRealMailboxGuid(TreeNodeDto));
                AOSObjectId = tm.GetAOSObjectId(TreeNodeDto);

                if (IsSupportGraphApi)
                {
                    this.MaxBackupItemsThreads = _keyValueDao.GetExoGraphDiscoverThreadsLimit();
                    mLog.Info($"Graph API is enabled for mailbox {TreeNodeDto.ID}, set MaxBackupItemsThreads to {MaxBackupItemsThreads} based on configuration.");
                }
            }
        }

        protected void ProcessFolder(IExchangeFolder folder)
        {
            using (PerformanceScope scope = new PerformanceScope("EXOEnforceRetentionProcessor.ProcessFolder",addToStatistics:true))
            {
                try
                {
                    using CheckJobStopScope jScope = new();
                    var childFolders = GetFolders(folder);
                    if (childFolders != null && childFolders.Count > 0)
                    {
                        //ReportManager.IncreaseBase(childFolders.Count);
                        foreach (var mFolder in childFolders)
                        {
                            ProcessFolder(mFolder);
                            //ReportManager.Increase();
                        }
                    }
                    var logonGroupId = TenantLocalValue.LogonGroupId;
                    var logonUserEmail = TenantLocalValue.LogonUserEmail;
                    //ExchangeItemBulkHelper bulkHelper = new ExchangeItemBulkHelper(folder, "");
                    var hasItems = false;
                    bool isJobStopped = false;
                    if (mNeedFullJobForMailBox)
                    {
                        //mLog.Info("Dove Debug_Use Full");
                        using (AveAppendableTaskExecutor fullTaskExecutor = new AveAppendableTaskExecutor(MaxBackupItemsThreads))
                        {
                            fullTaskExecutor.StartExecute();
                            foreach (var itemGroup in searchDiscover.GetGroupedItems(folder))
                            {
                                ReportManager.IncreaseBase(itemGroup.ItemsCount);
                                //mLog.Info("Dove Debug_Full Discover item count: {0}, time: {1}", itemGroup.ItemsCount, DateTime.UtcNow.Ticks);
                                //foreach (var item in itemGroup.Items)
                                //{
                                //    mLog.Info("Dove Debug_Full Discover item: {0}", mCachedNodeNameForPath + item.ItemPath);
                                //}
                                hasItems = true;
                                fullTaskExecutor.AddTask(() =>
                                {
                                    try
                                    {
                                        using CheckJobStopScope jScope = new();
                                        ProcessItems(folder, itemGroup, logonGroupId, logonUserEmail, true);
                                    }
                                    catch (JobStopException)
                                    {
                                        isJobStopped = true;
                                    }
                                });
                            }
                            if (!fullTaskExecutor.WaitForAllTasks(Timeout.Infinite))
                            {
                                //todo: handle timeout
                                mLog.Error($"Full Task Executor Opertation Time out exception.");
                            }
                        }
                        if (isJobStopped)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                    }
                    if (mNeedIncrementalJobForMailBox)
                    {
                        //mLog.Info("Dove Debug_Use Incremental");
                        using (AveAppendableTaskExecutor incTaskExecutor = new AveAppendableTaskExecutor(MaxBackupItemsThreads))
                        {
                            incTaskExecutor.StartExecute();
                            var filter = GenerateSearchFilter(folder);
                            var groupItems = incrementalDiscover.GetGroupedItems(folder, filter);
                            foreach (var itemGroup in groupItems)
                            {
                                //mLog.Info("Dove Debug_Incremental Discover item count: {0}, time: {1}", itemGroup.Items.Count(), DateTime.UtcNow.Ticks);
                                //foreach (var item in itemGroup.Items)
                                //{
                                //    mLog.Info("Dove Debug_Incremental Discover item: {0}", mCachedNodeNameForPath + item.ItemPath);
                                //}
                                hasItems = true;
                                incTaskExecutor.AddTask(() =>
                                {
                                    try
                                    {
                                        using CheckJobStopScope jScope = new();
                                        ProcessItems(folder, itemGroup, logonGroupId, logonUserEmail, false);
                                    }
                                    catch (JobStopException)
                                    {
                                        isJobStopped = true;
                                    }
                                });
                            }
                            if (!incTaskExecutor.WaitForAllTasks(Timeout.Infinite))
                            {
                                //todo: handle timeout
                                mLog.Error($"Incremental Task Executor Opertation Time out exception.");
                            }
                        }
                        if (isJobStopped)
                        {
                            throw new JobStopException("This Job is stopped.");
                        }
                    }
                    // if (hasItems)
                    //{
                    //只使用SeachDiscover的情况下, 需要计算Item Sync Sate
                    //Thread.Sleep(1500);
                    folder.GenerateCurrentItemSyncState();
                    //跑过Job, 才加Flag

                    if (!mFailedFolderList.Contains(folder.FolderId.ToMd5()))
                    {
                        EXONodeFlagDao.AddEXONodeInfo(GenerateNodeFlag(folder));
                    }
                    else
                    {
                        mLog.Info("Current folder has failed items, will not update last job time. Folder id:{0}", folder?.FolderId);
                    }
                    //}
                    //else
                    //{
                    //    mLog.Info("No items under current mailbox folder. Folder url: {0}", folder.ImpersonateId + folder.DisplayFolderPath);
                    //}
                    
                }
                catch (JobStopException ex)
                {
                    mLog.Info("Job Stopped");
                    throw;
                }
                catch (Exception e)
                {
                    mJobHasException = true;
                    AddFailedDetailForMailbox(folder.FolderName, folder.DisplayFolderPath, JobDetailsStatus.Failed, e.Message);
                    mLog.Error("An error occurred while prosess mail box, fullPath is :{0}, error message: {1}.", folder.DisplayFolderPath, e.ToString());
                }
            }
        }

        private void AddFailedFolder(Guid folderId)
        {
            if (!mFailedFolderList.Contains(folderId))
            {
                mFailedFolderList.Add(folderId);
            }
        }

        private SearchFilter GenerateSearchFilter(IExchangeFolder folder)
        {
            DateTime collectionTime = DateTime.MinValue;
            var nodeInfo = EXONodeFlagDao.GetEXONodeInfo(folder.FolderId.ToMd5(), groupId, (int)NodeFlagType.EnforceRetention);
            if (nodeInfo != null)
            {
                collectionTime = DateTime.SpecifyKind(new DateTime(nodeInfo.CollectionTime), DateTimeKind.Utc);
            }
            return collectionTime != DateTime.MinValue ? new SearchFilter.IsGreaterThan(ItemSchema.LastModifiedTime, collectionTime) : null;
        }

        private void ProcessItems(IExchangeFolder folder, IExchangeItemGroup itemGroup, string logonGroupId, string logonUserEmail, bool isFull)
        {
            Guid folderId = folder.FolderId.ToMd5();
            try
            {
                IExchangeItemBulkHelper bulkHelper = GetExchangeItemBulkHelper(folder);
                TenantLocalValue.LogonGroupId = logonGroupId;
                TenantLocalValue.LogonUserEmail = logonUserEmail;
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope()) 
                    {
                        LoadItemsPro(folder, itemGroup.Items);
                    }
                }
                catch (JobStopException)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    mLog.Error($"Error occurred while getting term property. Folder: {folder.FolderId}. Error: {e.ToString()}");
                    if (itemGroup.Items != null && itemGroup.ItemsCount > 0)
                    {
                        foreach (var item in itemGroup.Items)
                        {
                            SendJobReportDetails(item, JobDetailsStatus.Failed, false, "RM_Connector_InsertDatasFailed");
                        }
                    }
                    throw;
                }
                var addLabelItems = new List<IExchangeItem>();
                var removeLabelItems = new List<IExchangeItem>();
                foreach (var item in itemGroup.Items)
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        ReportManager.Increase();
                        var result = false;
                        if (isFull)
                        {
                            result = ProcessItemFull(item, folderId);
                        }
                        else
                        {
                            result = ProcessItemInc(item, folderId);
                        }
                        if (result)
                        {
                            if (item.IsLabelExist())
                            {
                                //mLog.Info("Dove Debug_Add Label item: {0}", mCachedNodeNameForPath + item.ItemPath);
                                addLabelItems.Add(item);
                            }
                            else
                            {
                                //mLog.Info("Dove Debug_Remove Label item: {0}", mCachedNodeNameForPath + item.ItemPath);
                                removeLabelItems.Add(item);
                            }
                        }
                        else
                        {
                            mLog.Info("Skip item: {0}.", item.ItemId);
                            //mLog.Debug("Skip Report Item {0}.", item.ItemPath);
                        }
                    }
                }
                var addLabelItemCount = 0;
                var removeLabelItemCount = 0;
                if (addLabelItems.Count > 0)
                {
                    try
                    {
                        ReportManager.IncreaseBase(addLabelItems.Count);
                        mWorkerThreads.WaitOne();
                        using PerformanceScope scope = new("EXOEnforceRetentionProcessor.ProcessItems.BatchUpdateAddLabel");
                        var result = bulkHelper.BatchUpdateExchangeItem(addLabelItems);
                        foreach (var item in result)
                        {
                            var eItem = itemGroup.Items.Where(t => t.ItemId == item.Key).FirstOrDefault();
                            if (!item.Value.IsFailed)
                            {
                                mLog.Info("start send job detail with status successful");
                                SendJobReportDetails(eItem, JobDetailsStatus.Successful, true, "");
                                //mLog.Info("Dove Debug_Add Label item success. Item: {0}", mCachedNodeNameForPath + eItem.ItemPath);
                                addLabelItemCount++;
                            }
                            else
                            {
                                AddFailedFolder(folderId);
                                mJobHasException = true;
                                SendJobReportDetails(eItem, JobDetailsStatus.Failed, true, item.Value.ErrorMessage);
                                //mLog.Info("Dove Debug_Add Label item failed. Item: {0}", mCachedNodeNameForPath + eItem.ItemPath);
                            }
                            ReportManager.Increase();
                        }
                    }
                    finally
                    {
                        mWorkerThreads.Release();
                    }
                }
                if (removeLabelItems.Count > 0)
                {
                    try
                    {
                        ReportManager.IncreaseBase(removeLabelItems.Count);
                        mWorkerThreads.WaitOne();
                        using PerformanceScope scope = new("EXOEnforceRetentionProcessor.ProcessItems.BatchUpdateRemoveLabel");
                        var result = bulkHelper.BatchUpdateExchangeItem(removeLabelItems);
                        foreach (var item in result)
                        {
                            var eItem = itemGroup.Items.Where(t => t.ItemId == item.Key).FirstOrDefault();
                            if (!item.Value.IsFailed)
                            {
                                SendJobReportDetails(eItem, JobDetailsStatus.Successful, false, "");
                                //mLog.Info("Dove Debug_Remove Label item success. Item: {0}", mCachedNodeNameForPath + eItem.ItemPath);
                                removeLabelItemCount++;
                            }
                            else
                            {
                                mJobHasException = true;
                                AddFailedFolder(folderId);
                                SendJobReportDetails(eItem, JobDetailsStatus.Failed, false, item.Value.ErrorMessage);
                                //mLog.Info("Dove Debug_Remove Label item failed. Item: {0}", mCachedNodeNameForPath + eItem.ItemPath);
                            }
                            ReportManager.Increase();
                        }
                    }
                    finally
                    {
                        mWorkerThreads.Release();
                    }
                }
                if (addLabelItemCount > 0 || removeLabelItemCount > 0)
                {
                    lock (obj)
                    {
                        //有成功记录, 更新中间状态
                        var tempLabel = EXOLabelDao.GetLabel((int)RMRetentionSourceType.Exchange, (int)RMRetentionLabelStatus.JobProcessing);
                        if (tempLabel == null)
                        {
                            var exoLabel = new RMEXOLabel();
                            exoLabel.LabelName = mGlobalCurrentEXOLabelName;
                            exoLabel.Status = 2;
                            exoLabel.Type = 0;
                            exoLabel.LabelId = mGlobalCurrentEXOLabelGuid;
                            exoLabel.SavedTime = DateTime.UtcNow.Ticks;
                            EXOLabelDao.Create(exoLabel);
                        }
                        else
                        {
                            var exoLabel = new RMEXOLabel();
                            exoLabel.Id = tempLabel.Id;
                            exoLabel.LabelName = mGlobalCurrentEXOLabelName;
                            exoLabel.Status = 2;
                            exoLabel.Type = 0;
                            exoLabel.LabelId = mGlobalCurrentEXOLabelGuid;
                            exoLabel.SavedTime = DateTime.UtcNow.Ticks;
                            var result = EXOLabelDao.UpdateAsync(exoLabel).Result;
                        }
                    }
                }
            }
            catch (JobStopException)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception ex)
            {
                mJobHasException = true;
                AddFailedFolder(folderId);
                mLog.Warn("Items count: {0} proccessed error.", itemGroup.Items.Count());
                mLog.Error(ex.ToString());
            }
        }

        protected void LoadItemsPro(IExchangeFolder folder, IEnumerable<IExchangeItem> items)
        {
            IExchangeItemBulkHelper bulkHelper = GetExchangeItemBulkHelper(folder);
            bulkHelper.LoadExtendProperties(items, false);
        }

        private IExchangeItemBulkHelper GetExchangeItemBulkHelper(IExchangeFolder folder)
        {
            IExchangeItemBulkHelper bulkHelper = IsSupportGraphApi ?
                new ExchangeGraphItemBulkHelper(CurrentFolder.MailBoxId, folder.FolderId, CurrentFolder.GetCredential()) :
                new ExchangeItemBulkHelper(CurrentFolder as ExchangeFolder);
            return bulkHelper;
        }

        protected bool ProcessItemFull(IExchangeItem item, Guid folderId)
        {
            var result = false;
            using (PerformanceScope scope = new PerformanceScope("EXOEnforceRetentionProcessor.ProcessItemFull", addToStatistics: true))
            {
                mLog.Info("Process Item: {0}.", item.ItemId);
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        Guid termId = Guid.Empty;
                        string value = string.Empty;
                        if (item.TryGetExtendProperty(ExtendProperty.Term, out value))
                        {
                            termId = new Guid(value);
                            if (EXOEnforceRetentionDataCache.Instance.TermDeclarationMapping.ContainsKey(termId))
                            {
                                //Add Cache
                                EXOEnforceRetentionDataCache.Instance.AddProcessedItem(item.ItemId.ToMd5());
                                //Do declare
                                var tempTerm = EXOEnforceRetentionDataCache.Instance.TermDeclarationMapping[termId];
                                if ((tempTerm.EnforceRetention & (int)EnforceRetentionType.Exchange) == (int)EnforceRetentionType.Exchange)
                                {
                                    if (RetentionLabel.TryGetValue(mGlobalCurrentEXOLabelName, out mGlobalCurrentEXOLabelGuid))
                                    {
                                        //Label不存在, 或当前存在的Label是上一次Retention Job使用的Label并且同时本次为新Label.
                                        if (!item.IsLabelExist() || (mGlobalEXOLabelProcessed && item.CanUpdateLabel(mGlobalPreviousEXOLabelGuids) && item.ApplyedLabelId() != mGlobalCurrentEXOLabelGuid))
                                        {
                                            try
                                            {
                                                mLog.Info("Tag Item {0}, {1}", item.ItemId, mGlobalCurrentEXOLabelName);
                                                item.TagLabel(mGlobalCurrentEXOLabelGuid);
                                                result = true;
                                                //SendJobReportDetails(item, JobDetailsStatus.Successful, true, "");
                                            }
                                            catch (Exception ex)
                                            {
                                                SendJobReportDetails(item, JobDetailsStatus.Failed, true, ex.ToString());
                                                mJobHasException = true;
                                                AddFailedFolder(folderId);
                                                mLog.Warn($"Tag exchange item failed, item id : {item?.ItemId}, reason : {ex.ToString()}.");
                                            }
                                        }
                                        else
                                        {
                                            SendJobReportDetails(item, JobDetailsStatus.Skipped, true, "RM_JM_EXORetention_Skip_LabelConflict");
                                            mLog.Debug($"Same or another confict label already exist, item id : {item?.ItemId}.");
                                        }
                                    }
                                    else
                                    {
                                        SendJobReportDetails(item, JobDetailsStatus.Failed, true, "RM_JM_EXORetention_Skip_LabelIsNotExistOnExchangeServer");
                                        mJobHasException = true;
                                        AddFailedFolder(folderId);
                                        mLog.Warn($"Tag exchange item failed, mailbox: {mCachedNodeNameForPath}, item id: {item?.ItemId}, reason: Label is not exist on exchange mailbox.");
                                    }
                                }
                                else
                                {
                                    if (mGlobalEXOLabelProcessed && item.CanUpdateLabel(mGlobalPreviousEXOLabelGuids))
                                    {
                                        try
                                        {
                                            mLog.Info("Remove Item label {0}, {1}", item.ItemId, mGlobalPreviousEXOLabelGuids);
                                            item.RemoveLabel();
                                            result = true;
                                            //SendJobReportDetails(item, JobDetailsStatus.Successful, false, "");
                                        }
                                        catch (Exception ex)
                                        {
                                            SendJobReportDetails(item, JobDetailsStatus.Failed, false, ex.ToString());
                                            mJobHasException = true;
                                            AddFailedFolder(folderId);
                                            mLog.Warn($"Exchange item remove tag failed, item id : {item?.ItemId}, reason : {ex.ToString()}.");
                                        }
                                    }
                                    else
                                    {
                                        SendJobReportDetails(item, JobDetailsStatus.Skipped, false, "RM_JM_EXORetention_Skip_LabelIsNotExistOnMailItem");
                                        mLog.Debug($"Not valid lable found, or label is already removed, item id : {item?.ItemId}");
                                    }
                                }
                            }
                            else
                            {
                                mLog.Debug("Skip Report Item {0}, Unknown term id {1} in cached changes terms.", item.ItemId, termId.ToString());
                                //SendJobReportDetails(item, JobDetailsStatus.Skipped, true, "I18NTodo_Unknown term id.");
                            }
                        }
                        else
                        {
                            mLog.Debug("Skip Report Item {0}, no valid term associated with current mail.", item.ItemId);
                            EXOEnforceRetentionDataCache.Instance.AddProcessedItem(item.ItemId.ToMd5());
                            //SendJobReportDetails(item, JobDetailsStatus.Skipped, true, "RM_JM_EXORetention_Skip_TermIsInvalid");
                        }
                    }
                }
                catch (JobStopException ex)
                {
                    mLog.Info("Job Stopped");
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception ex)
                {
                    mLog.Warn("Process item failed. item url: {0}, Error message: {1}.", item.ItemId, ex.ToString());
                }
            }
            return result;
        }

        protected bool ProcessItemInc(IExchangeItem item, Guid folderId)
        {
            var result = false;
            using (PerformanceScope scope = new PerformanceScope("EXOEnforceRetentionProcessor.ProcessItemInc", addToStatistics: true))
            {
                mLog.Info("Process Item: {0}.", item.ItemId);
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        if (EXOEnforceRetentionDataCache.Instance.GetProcessedItem(item.ItemId.ToMd5()))
                        {
                            mLog.Info("Item already processed and added in cache, item id:{0}", item.ItemId);
                            return result;
                        }
                        Guid termId = Guid.Empty;
                        string value = string.Empty;
                        if (item.TryGetExtendProperty(ExtendProperty.Term, out value))
                        {
                            termId = new Guid(value);
                            var dbt = TermDao.GetParentInhertSetting(termId);
                            if (dbt != null)
                            {
                                var tempTerm = new TermSettingsInfo() { EnforceRetention = dbt.EnforceRetention };
                                //Do declare
                                if ((tempTerm.EnforceRetention & (int)EnforceRetentionType.Exchange) == (int)EnforceRetentionType.Exchange)
                                {
                                    if (RetentionLabel.TryGetValue(mGlobalCurrentEXOLabelName, out mGlobalCurrentEXOLabelGuid))
                                    {
                                        //Label不存在, 或当前存在的Label是上一次Retention Job使用的Label并且同时本次为新Label.
                                        if (!item.IsLabelExist() || (mGlobalEXOLabelProcessed && item.CanUpdateLabel(mGlobalPreviousEXOLabelGuids) && item.ApplyedLabelId() != mGlobalCurrentEXOLabelGuid))
                                        {
                                            try
                                            {
                                                mLog.Info("Tag Item {0}, {1}.", item.ItemId, mGlobalCurrentEXOLabelName);
                                                item.TagLabel(mGlobalCurrentEXOLabelGuid);
                                                result = true;
                                                //SendJobReportDetails(item, JobDetailsStatus.Successful, true, "");
                                            }
                                            catch (Exception ex)
                                            {
                                                SendJobReportDetails(item, JobDetailsStatus.Failed, true, ex.ToString());
                                                mJobHasException = true;
                                                AddFailedFolder(folderId);
                                                mLog.Warn($"Tag exchange item failed, item id : {item?.ItemId}, reason : {ex.ToString()}.");
                                            }
                                        }
                                        else
                                        {
                                            SendJobReportDetails(item, JobDetailsStatus.Skipped, true, "RM_JM_EXORetention_Skip_LabelConflict");
                                            mLog.Debug($"Same or another confict label already exist, item id : {item?.ItemId}");
                                        }
                                    }
                                    else
                                    {
                                        SendJobReportDetails(item, JobDetailsStatus.Failed, true, "RM_JM_EXORetention_Skip_LabelIsNotExistOnExchangeServer");
                                        mJobHasException = true;
                                        AddFailedFolder(folderId);
                                        mLog.Warn($"Tag exchange item failed, mailbox: {mCachedNodeNameForPath}, item id: {item?.ItemId}, reason: Label is not exist on exchange mailbox.");
                                    }
                                }
                                else
                                {
                                    if (mGlobalEXOLabelProcessed && item.CanUpdateLabel(mGlobalPreviousEXOLabelGuids))
                                    {
                                        try
                                        {
                                            mLog.Info("Remove Item label {0}, {1}.", item.ItemId, mGlobalPreviousEXOLabelGuids);
                                            item.RemoveLabel();
                                            result = true;
                                            //SendJobReportDetails(item, JobDetailsStatus.Successful, false, "");
                                        }
                                        catch (Exception ex)
                                        {
                                            SendJobReportDetails(item, JobDetailsStatus.Failed, false, ex.ToString());
                                            mJobHasException = true;
                                            AddFailedFolder(folderId);
                                            mLog.Warn($"Exchange item remove tag failed, item id : {item?.ItemId}, reason : {ex.ToString()}.");
                                        }
                                    }
                                    else
                                    {
                                        SendJobReportDetails(item, JobDetailsStatus.Skipped, false, "RM_JM_EXORetention_Skip_LabelIsNotExistOnMailItem");
                                        mLog.Warn($"Not valid lable found, or label is already removed, item id : {item?.ItemId}");
                                    }
                                }
                            }
                            else
                            {
                                mLog.Debug("Skip Report Item {0}, Unknown term id {1}.", item.ItemId, termId.ToString());
                                //SendJobReportDetails(item, JobDetailsStatus.Skipped, true, "RM_JM_EXORetention_Skip_TermIsInvalid");
                            }
                        }
                        else
                        {
                            mLog.Debug("Skip Report Item {0}, no valid term associated with current mail.", item.ItemId);
                            //SendJobReportDetails(item, JobDetailsStatus.Skipped, true, "RM_JM_EXORetention_Skip_TermIsInvalid");
                        }
                    }
                }
                catch (JobStopException ex)
                {
                    mLog.Info("Job Stopped");
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception ex)
                {
                    mLog.Warn("Process item failed. item url: {0}, Error message: {1}.", item.ItemId, ex.ToString());
                }
            }
            return result;
        }

        private List<IExchangeFolder> GetFolders(IExchangeFolder folder)
        {
            if (folder == null)
            {
                return new List<IExchangeFolder>();
            }
            else
            {
                return folder.GetAllSubFolders().Where(f => f.FolderType == "IPF.Note").ToList();
            }
        }

        private void SendJobReportDetails(IExchangeItem item, JobDetailsStatus status, bool doOrUndo, string comments = "")
        {
            mLog.Info("send job deatil with status is : " + status);
            JMEnforceRetentionJobDetail detail = new JMEnforceRetentionJobDetail();
            detail.ObjectName = item.ItemName;
            detail.Action = doOrUndo ? "RM_EXO_EnforceRetention_TagLabel" : "RM_EXO_EnforceRetention_RemoveLabel";
            detail.SourceURL = mCachedNodeNameForPath + item.ItemPath + "_" + item.SendDateUTC.ToString("R");
            detail.Status = status;
            detail.Comment = comments;
            ReportManager.SendJobDetail(detail);
        }

        private EXONodeFlag GenerateNodeFlag(IExchangeFolder folder)
        {
            EXONodeFlag nodeFlag = new EXONodeFlag();
            nodeFlag.CollectionTime = DateTime.UtcNow.Ticks;
            nodeFlag.EmailAdress = folder.Mailbox.MailboxAddress;
            nodeFlag.AOSEmailboxId = AOSMailboxId;
            nodeFlag.FolderSyncState = folder.FolderSyncState;
            nodeFlag.FullPath = folder.DisplayFolderPath;
            nodeFlag.GroupId = groupId;
            nodeFlag.IsRemoved = false;
            nodeFlag.ItemSyncState = folder.ItemSyncState;
            nodeFlag.NodeFlagType = (int)NodeFlagType.EnforceRetention;
            nodeFlag.NodeId = folder.IsRootFolder ? AOSMailboxId : folder.FolderId.ToMd5();
            nodeFlag.Title = folder.FolderName;
            nodeFlag.AOSObjectId = AOSObjectId;
            return nodeFlag;
        }
    }
}
