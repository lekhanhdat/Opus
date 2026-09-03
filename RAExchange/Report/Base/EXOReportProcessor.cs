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
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.DB.Dao;
using System.Collections.Generic;
using System;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Common.Util;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.RAExchange.Authorization;
using ExchangeUtility;
using System.Linq;
using Microsoft.Exchange.WebServices.Data;
using AvePoint.RA.RAExchange.Common;
using System.Threading;
using AvePoint.RA.Contract.Tenant;
using System.Collections.Concurrent;
using AvePoint.RA.RAExchange.Discover;
using AvePoint.Wrapper.Common;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.DB.Dao.Impl;
using System.Threading.Tasks;
using AvePoint.RA.I18N.Core;
using ExchangeBackupUtility.Graph;
using ExchangeFolder = ExchangeBackupUtility.ExchangeFolder;
using ExchangeItem = ExchangeBackupUtility.ExchangeItem;
using ExchangeItemBulkHelper = ExchangeBackupUtility.ExchangeItemBulkHelper;
using Task = System.Threading.Tasks.Task;
using AvePoint.RA.DB.Dao.Extension;
using SerializerHelper = AvePoint.RA.Common.Global.Utils.SerializerHelper;
using AvePoint.RA.Contract.RMWeb.ReportCenter;

namespace AvePoint.RA.RAExchange.Report
{
    public abstract class EXOReportProcessor
    {
        protected static readonly RALogger mLog = RALogger.GetInstance(typeof(EXOReportProcessor));
        protected List<ExchangeOnlineTreeNodeDto> nodes = new List<ExchangeOnlineTreeNodeDto>();
        protected ConcurrentDictionary<string, Guid> cachedMailTermMapping = new ConcurrentDictionary<string, Guid>();
        private IBatchDiscover discover = null;
        private IBatchDiscoverV2 discoverV2 = null;
        private RMEXODiscoverHelper discoverHelper = null;

        protected int maxThreadCount = 25;
        protected int itemsPerTask = 1000; // items count per task, default value=1000;
        protected int itemsPerGroup = 40;

        protected void SetItemsPerTask(int value)
        {
            if (itemsPerTask != value)
            {
                itemsPerTask = value;
                mLog.Info($"rewrite EXOItemsPerTask : {itemsPerTask}");
            }
            
        }
        private IRMReportManager mReportManger;
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
        private IRMReportService mReportService;
        protected IRMReportService ReportService
        {
            get
            {
                if (mReportService == null)
                {
                    mReportService = (IRMReportService)PlatformWindsorManager.GetService(typeof(IRMReportService));
                }
                return mReportService;
            }
        }
        //private IGeneralSettingService mGeneralSettingService;
        //protected IGeneralSettingService GeneralSettingService
        //{
        //    get
        //    {
        //        if (mGeneralSettingService == null)
        //        {
        //            mGeneralSettingService = (IGeneralSettingService)PlatformWindsorManager.GetService(typeof(IGeneralSettingService));
        //        }
        //        return mGeneralSettingService;
        //    }
        //}
        private IRMSubJobDao SubJobDao { set; get; }
        private IEXOSettingDao EXOSettingDao { set; get; }
        private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
        protected ExchangeFolder CurrentFolder { get; set; }
        protected IExchangeFolder ExchangeFolder { get; set; }
        protected string mCachedNodeNameForPath = string.Empty;
        protected bool mJobHasException = false;
        protected bool mJobHasStopped = false;
        /// <summary>
        /// GUID的AOS MailboxID(经过特殊处理满足Records GUID格式需求的ID)
        /// </summary>
        protected Guid aosMailboxId = Guid.Empty;
        protected Guid DAOTreeNodeID = Guid.Empty;
        protected string ContainerId;
        protected string MailboxAddress;
        protected bool IsSupportGraphApi { get; set; }
        protected string ReportProfileId { get; set; }
        protected string ReportJobId { get; set; }

        protected EXOReportProcessor(string jobId, int jobType, bool IsOrphanedTermReport = false)
        {
            var numSetting = RMGlobalConfiguration.AppConfig[RMAppSettingKey.EXO_DISCOVER_ITEMS_PER_TASK];
            if (!string.IsNullOrEmpty(numSetting))
            {
                int.TryParse(numSetting, out itemsPerTask);
            }
            mLog.Info($"EXOItemsPerTask : {itemsPerTask}");
            var threadCount = RMGlobalConfiguration.AppConfig[RMAppSettingKey.EXO_DISCOVER_THREADS_LIMIT];
            if (!string.IsNullOrEmpty(threadCount))
            {
                int.TryParse(threadCount, out maxThreadCount);
            }
            mLog.Info($"MaxThreadCount : {maxThreadCount}");
            //ReportMangerFactory.Instance.Init(jobId, (JobType)jobType, true);
            ReportManager.StartUpdateJobProgress();

            SubJobDao = (IRMSubJobDao)PlatformWindsorManager.GetService(typeof(IRMSubJobDao));
            EXOSettingDao = (IEXOSettingDao)PlatformWindsorManager.GetService(typeof(IEXOSettingDao));
            RMSubJob subJobWithContext = SubJobDao.GetSubJob(jobId, true);

            List<RMEXOTreeNode> tempList = SerializerHelper.DeserializeByDataContractSerializer<List<RMEXOTreeNode>>(subJobWithContext.JobContext.Settings);
            //tempList.ForEach(node => nodes.Add(RMDtoConverter.ConvertRMExchangeTree2TreeNodeDto(node)));
            tempList.ForEach(node =>
            {
                var mailBoxSetting = EXOSettingDao.LoadSharePointSetting(new Guid(node.Id), new Guid(node.Id));
                if (mailBoxSetting == null)
                {
                    mailBoxSetting = EXOSettingDao.LoadSharePointSetting(new Guid(node.Parent.Id), Guid.Empty);
                }
                if (mailBoxSetting != null && mailBoxSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                {
                    nodes.Add(RMDtoConverter.ConvertRMExchangeTree2TreeNodeDto(node));
                }
            });
        }

        public virtual void SetDiscoverObject(RMEXODiscoverHelper discoverHelper, IBatchDiscover discover)
        {
            this.discoverHelper = discoverHelper;
            this.discover = discover;
        }

        public virtual bool CheckRunReportJobIsPrepared(out string message)
        {
            message = "";
            return true;
        }
        public virtual async System.Threading.Tasks.Task RunReportJobAsync()
        {
            string failedMessage = "";
            if (!CheckRunReportJobIsPrepared(out failedMessage))
            {
                ReportManager.SetJobFinished(JobStatus.Failed, failedMessage);
                return;
            }
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    if (nodes != null && nodes.Count > 0)
                    {
                        foreach (var node in nodes)
                        {
                            try
                            {
                                mCachedNodeNameForPath = node.Name;
                                await ProcessAsync(node);
                            }
                            catch (Exception ex)
                            {
                                mJobHasException = true;
                                mLog.Error($"Error in process node:{node.FullPath}, reason : {ex.ToString()}.");
                            }
                        }
                    }
                    else
                    {
                        mLog.Info("Tree node is null.");
                    }
                }
            }
            catch (JobStopException ex)
            {
                mJobHasStopped = true;
            }
            catch (Exception e)
            {
                mLog.Error($"Error in RunReportJob, reason : {e.ToString()}.");
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
                RMProfileDto ReportProfile = ReportService.GetProfileByIdForReportJob(ReportProfileId);
                if (ReportProfile?.ScheduleId != null && (finalStatus == JobStatus.Finished || finalStatus == JobStatus.FinishWithException))
                {
                    var jobIdReal = ReportJobId?.Split('_')[0];
                    var exportModel = new ExportReportCommonModel
                    {
                        ReportJobType = ((int)ReportProfile.Type).ToString(),
                        ReportJobId = jobIdReal,
                        ProfileName = ReportProfile.ProfileName,
                        ProfileId = ReportProfile.Id.ToString(),
                    };
                    var reportParameters = SerializerHelper.SerializeByJsonConvert(exportModel);
                    ReportService.RunExportReportJob(reportParameters);
                    mLog.Info("Started scheduled EXO due-disposal report export. JobId:{0}, ProfileId:{1}", jobIdReal, ReportProfile.Id);
                }
            }
        }

        public virtual async System.Threading.Tasks.Task ProcessAsync(ExchangeOnlineTreeNodeDto node)
        {
            try
            {
                if (node.Level == NodeLevel.ExchangeOnlineMailbox)
                {
                    Init(node);
                    if (!IsSupportGraphApi)
                    {
                        ProcessFolder(CurrentFolder);
                    }
                    else
                    {
                        ProcessFolder(ExchangeFolder);
                    }
                }
            }
            catch (JobStopException ex)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Microsoft.Kiota.Abstractions.ApiException ex)
            {

                if (ex.ResponseStatusCode == (int)System.Net.HttpStatusCode.Unauthorized || ex.ResponseStatusCode == (int)System.Net.HttpStatusCode.Forbidden)
                {
                    SendJobReportDetails(node, JobDetailsStatus.Failed, "RM_Aos_CustomApp_Permission");
                    mLog.Error($"Access is denied for mailbox '{node.Name}'. The current user may not have permissions in AOS. Error: {ex}");
                    throw;
                }

            }
            catch (Exception ex)
            {
                SendJobReportDetails(node, JobDetailsStatus.Failed, "RM_EXO_ReportCenter_NoUserExists");
                mLog.Error("An error occurred while farm process. Name: [{0}], error message : {1}.", node.Name, ex.ToString());
                throw;
            }
        }

        public void Init(ExchangeOnlineTreeNodeDto TreeNodeDto)
        {
            try 
            {
                using (PerformanceScope scope = new PerformanceScope("EXOReportProcessor.Init"))
                {
                    TreeManagement tm = new TreeManagement();
                    aosMailboxId = new Guid(tm.GetRealMailboxGuid(TreeNodeDto));
                    var mailboxNode = TreeManagement.GetMailboxNode(TreeNodeDto);
                    var mailboxGuid = tm.GetRealMailboxGuid(TreeNodeDto);
                    MailboxAddress = mailboxNode.Name;
                    IsSupportGraphApi = EXOGraphApiResolver.ShouldUseGraph(_keyValueDao, MailboxAddress, tm.GetRealMailboxStringId(TreeNodeDto), TreeNodeDto);
                    if (!IsSupportGraphApi)
                    {
                        CurrentFolder = tm.GetExchangeFolderFromTreeNode(TreeNodeDto);
                    }
                    else
                    {
                        ExchangeFolder = tm.GetExchangeFolderFromTreeNodeV2(TreeNodeDto,aosMailboxId.ToString(), IsSupportGraphApi);
                    }
                    DAOTreeNodeID = new Guid(mailboxNode.ID);
                    ContainerId = GetContainerNode(TreeNodeDto)?.ID;

                    ExtendedPropertyDefinition extendedPropertyDefinition = new ExtendedPropertyDefinition(TermColumnInfo.WellKnowTermColumnGuid, TermColumnInfo.WellKnowTermColumnId, MapiPropertyType.String);
                    var searchFilter = new SearchFilter.Exists(extendedPropertyDefinition);

                    if (!IsSupportGraphApi)
                    {
                        this.discoverHelper = new Discover.RMEXODiscoverHelper();
                        this.discover = EXODiscoverFactory.CreateFactory(this.discoverHelper, EXODiscoverType.Search, NodeFlagType.ExplorerSync, Guid.Empty, searchFilter);
                    }
                    else
                    {
                        discoverV2 = EXODiscoverFactoryV2.CreateFactory(EXODiscoverType.Search, NodeFlagType.ExplorerSync, Guid.Empty, searchFilter);
                    }

                    if (IsSupportGraphApi)
                    {
                        this.maxThreadCount = _keyValueDao.GetExoGraphDiscoverThreadsLimit();
                        mLog.Info($"Graph API is enabled for mailbox {TreeNodeDto.ID}, set MaxBackupItemsThreads to {maxThreadCount} based on configuration.");
                    }
                }
            }
            catch (Exception ex) 
            {
                mLog.Info($"Converted to a Guid error, it may be that the current user has been deleted in Aos:{ex}");
                throw;
            }
        }

        private ExchangeOnlineTreeNodeDto GetContainerNode(ExchangeOnlineTreeNodeDto treeNodeDto)
        {
            if (treeNodeDto == null) return null;
            if(treeNodeDto.Level == NodeLevel.ExchangeOnlineMailboxGroup)
            {
                return treeNodeDto;
            }
            return GetContainerNode(treeNodeDto.Parent);
        }

        protected virtual void ProcessFolder(ExchangeFolder folder)
        {
            using (PerformanceScope scope = new PerformanceScope("RAExchangeReportProcessor.ProcessFolder"))
            {
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        var childFolders = GetFolders(folder);
                        if (childFolders != null && childFolders.Count > 0)
                        {
                            ReportManager.IncreaseBase(childFolders.Count);
                            foreach (var mFolder in childFolders)
                            {
                                ReportManager.Increase();
                                ProcessFolder(mFolder);
                            }
                        }
                        //如果启用了分组功能，就走进If 下方逻辑；如果不启用分组，就走else 逻辑
                        if (IsGroupItems)
                        {
                            
                            var logonGroupId = TenantLocalValue.LogonGroupId;
                            var logonUserEmail = TenantLocalValue.LogonUserEmail;
                            bool isJobStopped = false;
                            using (AveAppendableTaskExecutor taskExecutor = new AveAppendableTaskExecutor(maxThreadCount))
                            {
                                taskExecutor.StartExecute();
                                var hasItems = false;
                                foreach (var itemGroup in discover.GetGroupedItems(folder))
                                {
                                    hasItems = true;
                                    taskExecutor.AddTask(() =>
                                    {
                                        try
                                        {
                                            using CheckJobStopScope jScope = new();
                                            TenantLocalValue.LogonGroupId = logonGroupId;
                                            TenantLocalValue.LogonUserEmail = logonUserEmail;
                                            ProcessGroupItems(folder, itemGroup.Items);
                                        }
                                        catch (JobStopException)
                                        {
                                            isJobStopped = true;
                                        }
                                    });
                                }
                                if(hasItems)
                                {
                                    SendJobReportDetails(folder, JobDetailsStatus.Successful, "");
                                }
                                else
                                {
                                    SendJobReportDetails(folder, JobDetailsStatus.Skipped, string.Empty);
                                    mLog.Info("No items under current mailbox folder. Folder url: {0}.", folder.ImpersonateId + folder.DisplayFolderPath);
                                }
                                mLog.Info($"Add items to task executor finished.");
                                if (!taskExecutor.WaitForAllTasks(Timeout.Infinite))
                                {
                                    //todo: handle timeout
                                    mLog.Error($"Time out exception.");
                                }
                            }
                            if (isJobStopped)
                            {
                                throw new JobStopException("This Job is stopped.");
                            }
                        }
                        else
                        {
                            var childItems = GetItems(folder);
                            if (childItems != null && childItems.Count > 0)
                            {
                                ReportManager.IncreaseBase(childItems.Count);
                                SendJobReportDetails(folder, JobDetailsStatus.Successful, "");
                                if (childItems.Count > itemsPerTask)
                                {
                                    var cts = new CancellationTokenSource();
                                    RunMultiThreadsReport(folder, childItems, itemsPerTask, cts);
                                    return;
                                }
                                foreach (var item in childItems)
                                {
                                    ReportManager.Increase();
                                    ProcessItem(item);
                                }
                            }
                            else
                            {
                                SendJobReportDetails(folder, JobDetailsStatus.Skipped, string.Empty);
                                mLog.Info("No items under current mailbox folder. Folder url: {0}.", folder.ImpersonateId + folder.DisplayFolderPath);
                            }
                        }
                    }
                }
                catch (JobStopException ex)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception e)
                {
                    mJobHasException = true;
                    SendJobReportDetails(folder, JobDetailsStatus.Failed, "RM_JM_Details_Failed_UnexpectedError");
                    mLog.Error("An error occurred while prosess mail box, fullPath is :{0}, error message: {1}.", folder.DisplayFolderPath, e.ToString());
                }
                finally
                {
                    cachedMailTermMapping.Clear();
                }
            }
        }
        protected virtual void ProcessFolder(IExchangeFolder folder)
        {
            using PerformanceScope scope = new PerformanceScope("RAExchangeReportProcessor.ProcessIExchangeFolder");
            try
            {
                using CheckJobStopScope jScope = new();
                var childFolders = GetFolders(folder);
                if (childFolders is { Count: > 0 })
                {
                    ReportManager.IncreaseBase(childFolders.Count);
                    foreach (var childFolder in childFolders)
                    {
                        ReportManager.Increase();
                        ProcessFolder(childFolder);
                    }
                }
                if (IsGroupItems)
                {
                    var logonGroupId = TenantLocalValue.LogonGroupId;
                    var logonUserEmail = TenantLocalValue.LogonUserEmail;
                    bool isJobStopped = false;
                    using (AveAppendableTaskExecutor taskExecutor = new(maxThreadCount))
                    {
                        taskExecutor.StartExecute();
                        var hasItems = false;
                        foreach (var itemGroup in discoverV2.GetGroupedItems(folder))
                        {
                            hasItems = true;
                            taskExecutor.AddTask(() =>
                            {
                                try
                                {
                                    using CheckJobStopScope jScope = new();
                                    TenantLocalValue.LogonGroupId = logonGroupId;
                                    TenantLocalValue.LogonUserEmail = logonUserEmail;
                                    ProcessGroupItems(folder, itemGroup.Items);
                                }
                                catch (JobStopException)
                                {
                                    isJobStopped = true;
                                }
                            });
                        }
                        if (hasItems)
                        {
                            SendJobReportDetails(folder, JobDetailsStatus.Successful, "");
                        }
                        else
                        {
                            SendJobReportDetails(folder, JobDetailsStatus.Skipped, string.Empty);
                            mLog.Info("No items under current mailbox graph folder. Folder url: {0}.", folder.ImpersonateId + folder.DisplayFolderPath);
                        }
                        mLog.Info($"Add graph items to task executor finished.");
                        if (!taskExecutor.WaitForAllTasks(Timeout.Infinite))
                        {
                            //todo: handle timeout
                            mLog.Error($"Time out exception.");
                        }
                    }
                    if (isJobStopped)
                    {
                        throw new JobStopException("This Job is stopped.");
                    }
                }
                else
                {
                    var childItems = GetItems(folder);
                    if (childItems != null && childItems.Count > 0)
                    {
                        ReportManager.IncreaseBase(childItems.Count);
                        SendJobReportDetails(folder, JobDetailsStatus.Successful, "");
                        if (childItems.Count > itemsPerTask)
                        {
                            var cts = new CancellationTokenSource();
                            RunMultiThreadsReport(folder, childItems, itemsPerTask, cts);
                            return;
                        }
                        foreach (var item in childItems)
                        {
                            ReportManager.Increase();
                            ProcessItem(item);
                        }
                    }
                    else
                    {
                        SendJobReportDetails(folder, JobDetailsStatus.Skipped, string.Empty);
                        mLog.Info("No items under current mailbox folder. Folder url: {0}.", folder.ImpersonateId + folder.DisplayFolderPath);
                    }
                }
            }
            catch (JobStopException ex)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception e)
            {
                mJobHasException = true;
                SendJobReportDetails(folder, JobDetailsStatus.Failed, "RM_JM_Details_Failed_UnexpectedError");
                mLog.Error("An error occurred while prosess mail box, fullPath is :{0}, error message: {1}.", folder.DisplayFolderPath, e.ToString());
            }
            finally
            {
                cachedMailTermMapping.Clear();
            }
        }
        
        protected void RunMultiThreadsReport(ExchangeFolder folder, List<ExchangeItem> items, int itemsPerTask, CancellationTokenSource cts)
        {
            var currentGroupId = TenantLocalValue.LogonGroupId;
            var currentUserId = TenantLocalValue.LogonUserId;
            var currentUserType = TenantLocalValue.AccountType;
            var displayName = TenantLocalValue.DisplayName;
            var currentUserName = TenantLocalValue.LogonUserEmail;
            var currentPrincipal = Thread.CurrentPrincipal;

            var partioner = Partitioner.Create(0, items.Count, itemsPerTask);
            System.Threading.Tasks.Parallel.ForEach(partioner, (range, loopState) =>
            {
                TenantLocalValue.LogonGroupId = currentGroupId;
                TenantLocalValue.LogonUserId = currentUserId;
                TenantLocalValue.LogonUserEmail = currentUserName;
                TenantLocalValue.AccountType = currentUserType;
                TenantLocalValue.DisplayName = displayName;
                TenantLocalValue.CurrentCulture = null;
                Thread.CurrentPrincipal = currentPrincipal;

                var startPos = range.Item1;
                var endPos = range.Item2;
                mLog.Info($"enter new thread. startPos: {startPos}, endPos : {endPos}");
                if (IsGroupItems)
                {
                    var groupsCount = (endPos - startPos + itemsPerGroup - 1) / itemsPerGroup;

                    for (var j = 0; j < groupsCount; j++)
                    {
                        var skipCount = startPos + itemsPerGroup * j;
                        var takecount = (skipCount + itemsPerGroup >= endPos) ? endPos - skipCount : itemsPerGroup;
                        var groupItems = items.Skip(skipCount).Take(takecount).ToList();
                        //mLog.Info($"groupsCount : {groupsCount}, skipCount : {skipCount}, take count: {takecount}, real grouped item count : {groupItems.Count}");
                        ProcessGroupItems(folder, groupItems);
                    }
                }
                else
                {
                    for (var j = startPos; j < endPos; j++)
                    {
                        ReportManager.Increase();
                        ProcessItem(items[j]);
                    }
                }
            });
        }
        
        protected void RunMultiThreadsReport(IExchangeFolder folder, List<IExchangeItem> items, int itemsPerTask, CancellationTokenSource cts)
        {
            var currentGroupId = TenantLocalValue.LogonGroupId;
            var currentUserId = TenantLocalValue.LogonUserId;
            var currentUserType = TenantLocalValue.AccountType;
            var displayName = TenantLocalValue.DisplayName;
            var currentUserName = TenantLocalValue.LogonUserEmail;
            var currentPrincipal = Thread.CurrentPrincipal;

            var partioner = Partitioner.Create(0, items.Count, itemsPerTask);
            System.Threading.Tasks.Parallel.ForEach(partioner, (range, loopState) =>
            {
                TenantLocalValue.LogonGroupId = currentGroupId;
                TenantLocalValue.LogonUserId = currentUserId;
                TenantLocalValue.LogonUserEmail = currentUserName;
                TenantLocalValue.AccountType = currentUserType;
                TenantLocalValue.DisplayName = displayName;
                TenantLocalValue.CurrentCulture = null;
                Thread.CurrentPrincipal = currentPrincipal;

                var startPos = range.Item1;
                var endPos = range.Item2;
                mLog.Info($"enter new thread. startPos: {startPos}, endPos : {endPos}");
                if (IsGroupItems)
                {
                    var groupsCount = (endPos - startPos + itemsPerGroup - 1) / itemsPerGroup;

                    for (var j = 0; j < groupsCount; j++)
                    {
                        var skipCount = startPos + itemsPerGroup * j;
                        var takecount = (skipCount + itemsPerGroup >= endPos) ? endPos - skipCount : itemsPerGroup;
                        var groupItems = items.Skip(skipCount).Take(takecount).ToList();
                        //mLog.Info($"groupsCount : {groupsCount}, skipCount : {skipCount}, take count: {takecount}, real grouped item count : {groupItems.Count}");
                        ProcessGroupItems(folder, groupItems);
                    }
                }
                else
                {
                    for (var j = startPos; j < endPos; j++)
                    {
                        ReportManager.Increase();
                        ProcessItem(items[j]);
                    }
                }
            });
        }

        protected abstract void ProcessItem(ExchangeItem item);
        protected abstract void ProcessItem(IExchangeItem item);
        protected virtual bool IsGroupItems => false;

        protected virtual void ProcessGroupItems(ExchangeFolder folder, IEnumerable<ExchangeItem> items)
        {
            throw new NotImplementedException();
        }
        
        protected virtual void ProcessGroupItems(IExchangeFolder folder, IEnumerable<IExchangeItem> items)
        {
            throw new NotImplementedException();
        }

        protected List<ExchangeFolder> GetFolders(ExchangeFolder folder)
        {
            if (folder == null)
            {
                return new List<ExchangeFolder>();
            }
            else
            {
                return folder.GetAllSubFolders().Where(f => f.FolderType == "IPF.Note").ToList();
            }
        }
        
        protected List<IExchangeFolder> GetFolders(IExchangeFolder folder)
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

        protected List<ExchangeItem> GetItems(ExchangeFolder folder)
        {
            return folder.GetAllItems();
        }
        
        protected List<IExchangeItem> GetItems(IExchangeFolder folder)
        {
            return folder.GetAllItemsUnderFolder().GetAwaiter().GetResult();
        }

        protected IEnumerable<Tuple<ExchangeItem, bool, Guid>> GetItemsTaxonomyFieldValue(ExchangeFolder folder, IEnumerable<ExchangeItem> items)
        {
            using (PerformanceScope scope = new PerformanceScope("EXOReportProcessor.GetItemsTaxonomyFieldValue"))
            {
                var result = new List<Tuple<ExchangeItem, bool, Guid>>();
                var items2 = new List<ExchangeItem>();
                foreach(var item in items)
                {
                    var termId = Guid.Empty;
                    if (cachedMailTermMapping.TryGetValue(item.ItemId, out termId))
                    {
                        result.Add(Tuple.Create<ExchangeItem, bool, Guid>(item, true, termId));
                    }
                    else
                    {
                        items2.Add(item);
                    }
                }
                //var items2 = items.Where(o => !cachedMailTermMapping.Keys.Contains(o.ItemId));

                var helper = new ExchangeItemBulkHelper(folder, string.Empty);
                var idDefinition = new ExtendedPropertyDefinition(TermColumnInfo.WellKnowTermColumnGuid, TermColumnInfo.WellKnowTermColumnId, MapiPropertyType.String);
                var sensitivityDef = new ExtendedPropertyDefinition(DefaultExtendedPropertySet.InternetHeaders, "msip_labels", MapiPropertyType.String);
                List<ExtendedPropertyDefinition> tempDefinition = new List<ExtendedPropertyDefinition>();
                tempDefinition.Add(idDefinition);
                tempDefinition.Add(sensitivityDef);
                helper.LoadExtendProperties(items2, tempDefinition.ToArray());
                foreach(var item in items2)
                {
                    string value = string.Empty;
                    if (item.TryGetProperty(idDefinition, out value))
                    {
                        var termId = new Guid(value);
                        cachedMailTermMapping.TryAdd(item.ItemId, termId);
                        result.Add(Tuple.Create<ExchangeItem, bool, Guid>(item, true, termId));
                    }
                    else
                    {
                        result.Add(Tuple.Create<ExchangeItem, bool, Guid>(item, false, Guid.Empty));
                    }
                }
                return result;
            }
        }
        protected IEnumerable<Tuple<IExchangeItem, bool, Guid>> GetItemsTaxonomyFieldValue(IExchangeFolder folder, IEnumerable<IExchangeItem> items)
        {
            using PerformanceScope scope = new ("EXOReportProcessor.GetGraphItemsTaxonomyFieldValue");
            var result = new List<Tuple<IExchangeItem, bool, Guid>>();
            var items2 = new List<IExchangeItem>();
            foreach(var item in items)
            {
                var termId = Guid.Empty;
                if (cachedMailTermMapping.TryGetValue(item.ItemId, out termId))
                {
                    result.Add(Tuple.Create(item, true, termId));
                }
                else
                {
                    items2.Add(item);
                }
            }
            
            foreach(var item in items2)
            {
                string value = string.Empty;
                if (item.TryGetExtendProperty(ExtendProperty.Term, out value))
                {
                    var termId = new Guid(value);
                    cachedMailTermMapping.TryAdd(item.ItemId, termId);
                    result.Add(Tuple.Create(item, true, termId));
                }
                else
                {
                    result.Add(Tuple.Create(item, false, Guid.Empty));
                }
            }
            return result;
        }
        protected bool GetSingleTaxonomyFieldValue(ExchangeItem item, out Guid termId)
        {
            using (PerformanceScope scope0 = new PerformanceScope("EXOReportProcessor.GetSingleTaxonomyFieldValue"))
            {
                bool result = true;
                termId = new Guid();
                if (!cachedMailTermMapping.TryGetValue(item.ItemId, out termId))
                {
                    try
                    {
                        var idDefinition = new ExtendedPropertyDefinition(TermColumnInfo.WellKnowTermColumnGuid, TermColumnInfo.WellKnowTermColumnId, MapiPropertyType.String);
                        List<ExtendedPropertyDefinition> tempDefinition = new List<ExtendedPropertyDefinition>();
                        tempDefinition.Add(idDefinition);
                        var tempResult = item.LoadExtendProperties(tempDefinition.ToArray());

                        if (tempResult != null && tempResult.Count > 0)
                        {
                            if (tempResult[idDefinition] != null)
                            {
                                termId = new Guid(tempResult[idDefinition].ToString());
                                cachedMailTermMapping.TryAdd(item.ItemId, termId);
                            }
                            else
                            {
                                mLog.Warn("Get single taxonomy field value null! Item url: {0}", item.ItemId);
                                result = false;
                            }
                        }
                        else
                        {
                            result = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn("Get single taxonomy field value failed! Item url: {0}, error message: {1}.", item.ItemId, ex.ToString());
                        result = false;
                    }
                }
                return result;
            }
        }
        protected bool GetSingleTaxonomyFieldValue(IExchangeItem item, out Guid termId)
        {
            using PerformanceScope scope0 = new PerformanceScope("EXOReportProcessor.GetSingleTaxonomyFieldValue");
            bool result = true;
            termId = Guid.Empty;
            if (!cachedMailTermMapping.TryGetValue(item.ItemId, out termId))
            {
                try
                {
                    if (item.TryGetExtendProperty(ExtendProperty.Term, out var itemTermId))
                    {
                        if (itemTermId.IsNotNullOrEmpty())
                        {
                            termId = new Guid(itemTermId);
                            cachedMailTermMapping.TryAdd(item.ItemId, termId);
                        }
                        else
                        {
                            mLog.Warn("Get single taxonomy field value null! Item url: {0}", item.ItemId);
                            result = false;
                        }
                    }
                    else
                    {
                        result = false;
                    }
                }
                catch (Exception ex)
                {
                    mLog.Warn("Get single taxonomy field value failed! Item url: {0}, error message: {1}.", item.ItemId, ex.ToString());
                    result = false;
                }
            }
            return result;
        }

        protected virtual void SendJobReportDetails(ExchangeFolder folder, JobDetailsStatus status, string comments = "")
        {
            JMReportJobDetails detail = new JMReportJobDetails();
            detail.Type = "RM_EXO_LevelType_ExchangeOnlineFolder";
            detail.TitleOrName = folder.FolderName;
            detail.Url = folder.ImpersonateId + folder.DisplayFolderPath;
            detail.Status = status;
            detail.Comment = comments;
            ReportManager.SendJobDetail(detail);
        }
        
        protected virtual void SendJobReportDetails(IExchangeFolder folder, JobDetailsStatus status, string comments = "")
        {
            JMReportJobDetails detail = new JMReportJobDetails();
            detail.Type = "RM_EXO_LevelType_ExchangeOnlineFolder";
            detail.TitleOrName = folder.FolderName;
            detail.Url = folder.ImpersonateId + folder.DisplayFolderPath;
            detail.Status = status;
            detail.Comment = comments;
            ReportManager.SendJobDetail(detail);
        }

        protected void SendJobReportDetails(ExchangeOnlineTreeNodeDto item, JobDetailsStatus status, string comments = "")
        {
            JMReportJobDetails detail = new()
            {
                Type = "RM_EXO_LevelType_ExchangeOnlineFolder",
                TitleOrName = "Top of Information Store",
                Url = item.Name,
                Status = status,
                Comment = comments
            };
            ReportManager.SendJobDetail(detail);
        }
    }
}
