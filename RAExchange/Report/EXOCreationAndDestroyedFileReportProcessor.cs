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
using AvePoint.GCommon.Contract.Server.Service;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.RAExchange.Common;
using Microsoft.Graph;
using Newtonsoft.Json;
using RAArchiverCommon.DestructionCache;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Contract;
using ExchangeBackupUtility.Graph;
using ExchangeFolder = ExchangeBackupUtility.ExchangeFolder;
using ExchangeItem = ExchangeBackupUtility.ExchangeItem;

namespace AvePoint.RA.RAExchange.Report
{
    public class EXOCreationAndDestroyedFileReportProcessor : EXOReportProcessor
    {
        private RMCreationJobMessage msg = null;
        private DateTime startUtcTime;
        private DateTime endUtcTime;
        private ITermDao mTermDao;
        protected override bool IsGroupItems => true;
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
        private IArchiverTableDao mArchiverTableDao = null;
        private AzureTableConnectContract mAzureTableConnectInfo = null;
        private string mTenantGroupId = TenantLocalValue.LogonGroupId;
        private string commomErrorMessage = string.Empty;
        private const string TIME_FORMAT = "yyyy-MM-dd HH:mm";
        private Dictionary<Guid, RMRuleInfos> idRuleInfoDic = new Dictionary<Guid, RMRuleInfos>();
        private IRuleManagerService mRuleManagerService;
        private string mailboxStringId = string.Empty;
        private DestrunctionReportHelper destrunctionReportHelper = null;
        private Dictionary<int, RMAccount> cacheAllUsers = null;
        protected IRuleManagerService RuleManagerService
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
        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private IExplorerDao explorerDao = new ExplorerDao();
        public EXOCreationAndDestroyedFileReportProcessor(RMCreationJobMessage msg)
            : base(msg.JobID, (int)JobType.EXOCreateAndDestroyedFileReport)
        {
            RMProfileDto profile = ReportService.GetProfileByIdForReportJob(msg.ProfileId);
            this.msg = msg;
            var globalTimeZone = GeneralSettingConfig.FindSystemTimeZoneById(this.msg.GlobalTimeZoneId.Replace("_", " "));
            startUtcTime = TimeZoneInfo.ConvertTimeToUtc(this.msg.StartTime, globalTimeZone);
            endUtcTime = TimeZoneInfo.ConvertTimeToUtc(this.msg.EndTime.AddDays(1), globalTimeZone);//包含当天
            destrunctionReportHelper = new DestrunctionReportHelper(startUtcTime, endUtcTime);
            mArchiverTableDao = (IArchiverTableDao)PlatformWindsorManager.GetService(typeof(IArchiverTableDao));
            commomErrorMessage = "RM_TS_SS_Summary";
            cacheAllUsers = AccountDao.FindAll().ToDictionary(k => k.Id, v => v);
        }

        public override async Task ProcessAsync(ExchangeOnlineTreeNodeDto node)
        {
            try
            {
                if (node.Level == NodeLevel.ExchangeOnlineMailbox)
                {
                    Init(node);
                    if (msg.SelectCreated)
                    {
                        if (!IsSupportGraphApi)
                        { 
                            ProcessFolder(CurrentFolder);
                        }
                        else
                        {
                            ProcessFolder(ExchangeFolder);
                        }
                    }
                    if (msg.SelectDestroyed)
                    {
                        TreeManagement tm = new TreeManagement();
                        mailboxStringId = tm.GetRealMailboxStringId(node);
                        await ProcessDeletedMailsForDestoryedReportAsync(node);
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

        protected async Task ProcessDeletedMailsForDestoryedReportAsync(ExchangeOnlineTreeNodeDto node)
        {
            using (PerformanceScope scope = new PerformanceScope("EXOCreationAndDestroyedFileReportProcessor.ProcessDeletedMailsForDestoryedReport"))
            {
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        await InitRulesInfoAsync();
                        var childItems = await GetDeletedItemsAsync(node.Name, node.ID);
                        if (childItems != null && childItems.Count > 0)
                        {
                            ReportManager.IncreaseBase(childItems.Count);
                            foreach (var item in childItems)
                            {
                                ReportManager.Increase();
                                item.PartitionKey = node.Name;
                                ProcessDeletedItem(item);
                                SendJobReportDetailsForDestoryItem(item, JobDetailsStatus.Successful, "");
                            }
                        }
                        else
                        {
                            mLog.Info("An error occurred while prosess mail box, fullPath is :{0}, error message: {1}.", node.FullPath);
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
                    mLog.Error("An error occurred while prosess mail box, fullPath is :{0}, error message: {1}.", node.FullPath, e.ToString());
                }
                finally
                {

                }
            }
        }

        private async Task<List<ArchiverExchangeOnlineDto>> GetEntitiesFromArhicverTableAsync(string partKey, string mailboxId, DateTime queryStartUtcTime, DateTime queryEndUtcTime)
        {
            var mDocAveClient = new DAOAPIClientV1(true);
            mAzureTableConnectInfo = await mDocAveClient.GetArchiverDataBaseConfigAsync();
            List<ArchiverExchangeOnlineDto> infos = mArchiverTableDao.GetDeletedItemsByMailBoxId(mAzureTableConnectInfo, mTenantGroupId, partKey, mailboxId, queryStartUtcTime, queryEndUtcTime);
            return infos;
        }

        private async Task<List<ArchiverExchangeOnlineDto>> GetDeletedItemsAsync(string partKey, string mailboxId)
        {
            List<ArchiverExchangeOnlineDto> infos = new();
            try
            {
                if (TenantService.IsNewOpusTenant())
                {
                    try
                    {
                        var queryArchiverTableTimeRange = destrunctionReportHelper.GetQueryArchiverTableTimeRange();
                        if (queryArchiverTableTimeRange != null)
                        {
                            List<ArchiverExchangeOnlineDto> destroyedItemsInArchiverTable = await GetEntitiesFromArhicverTableAsync(partKey, mailboxId, queryArchiverTableTimeRange.Item1, queryArchiverTableTimeRange.Item2);
                            if (destroyedItemsInArchiverTable != null)
                            {
                                infos = infos.Concat(destroyedItemsInArchiverTable).ToList();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn($"An error while get data from archiver table, message: {ex}");
                    }

                    var queryLiteDBTimeRange = destrunctionReportHelper.GetQueryLiteDBTimeRange();
                    if (queryLiteDBTimeRange != null)
                    {
                        List<ArchiverExchangeOnlineDto> destroyedItemsInLiteDB = GetDestroyedItemFromCache(mailboxStringId, queryLiteDBTimeRange.Item1, queryLiteDBTimeRange.Item2);
                        if (destroyedItemsInLiteDB != null)
                        {
                            infos = infos.Concat(destroyedItemsInLiteDB).ToList();
                        }
                    }
                }
                else
                {
                    infos = await GetEntitiesFromArhicverTableAsync(partKey, mailboxId, startUtcTime, endUtcTime);
                }
            }
            catch (AzureTableNotExistException ex)
            {
                mLog.Error("Failed to retrieve the archived data that meets the rule settings. table not exist, Node id:[{0}], error:{1}", mailboxId, ex.ToString());
                commomErrorMessage = I18NEntity.GetString("RM_DAM_NoTable");
                throw;
            }
            catch (SqlException se)
            {
                //REC-2281
                mLog.Warn("Get destroyed items failed,error:{0}", se.ToString());
            }
            return infos;
        }

        private List<ArchiverExchangeOnlineDto> GetDestroyedItemFromCache(string mailBoxId, DateTime queryStartUtcTime, DateTime queryEndUtcTime)
        {
            List<ArchiverExchangeOnlineDto> infos = new List<ArchiverExchangeOnlineDto>();
            string filePath = String.Empty;
            using (PerformanceScope scope = new PerformanceScope("EXOCreationAndDestroyedFileReportProcessor.DownloadCacheFromStorage"))
            {
                filePath = DestructionFactory.GetInstance(mailBoxId, string.Empty).DownloadCacheFromStorage(mailBoxId, queryStartUtcTime, queryEndUtcTime);
            }
            if (!string.IsNullOrWhiteSpace(filePath) && System.IO.Directory.Exists(filePath))
            {
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(filePath);
                foreach (var file in dir.GetFiles())
                {
                    DestructionUtility destructionUtility = new DestructionUtility(file.FullName);
                    int readCount = 0;
                    int pageSize = 100;
                    int pageIndex = 0;
                    do
                    {
                        var records = destructionUtility.SelectValuesFromDB(pageIndex, pageSize);
                        pageIndex += records.Count;
                        readCount = records.Count;
                        infos.AddRange(records.ConvertAll(r => ConvertDestructionReport2ArchiverExchangeOnlineDto(r)));
                    }
                    while (readCount == 100);

                }
                try
                {
                    System.IO.Directory.Delete(filePath, true);
                }
                catch (Exception e)
                {
                    mLog.Warn($@"fail delete file,ex:{e}");
                }
            }
            else
            {
                mLog.Warn("Destruction cache file not exist.");
            }

            DestructionFactory.Dispose(mailBoxId, string.Empty);
            //过滤掉不在report time range内的数据
            return infos.Where(r => r.ArchivedTime >= queryStartUtcTime.Ticks && r.ArchivedTime <= queryEndUtcTime.Ticks).ToList();
        }

        private ArchiverExchangeOnlineDto ConvertDestructionReport2ArchiverExchangeOnlineDto(DestructionReport destructionReport)
        {
            ArchiverExchangeOnlineDto dto = new ArchiverExchangeOnlineDto();
            dto.ArchivedTime = destructionReport.ArchivedTime;
            dto.NodeID = destructionReport.NodeId;
            dto.RuleID = destructionReport.RuleID;
            dto.FullPath = destructionReport.FullPath;
            dto.JsonMeta = destructionReport.JsonMeta;
            var jsonMeta = JsonConvert.DeserializeObject<ArchiverExchangeOnlineDto>(destructionReport.JsonMeta);
            if (jsonMeta != null)
            {
                dto.TermValue = jsonMeta.TermValue;
                dto.Title = jsonMeta.Title;
            }
            return dto;
        }

        protected override void ProcessGroupItems(ExchangeFolder folder, IEnumerable<ExchangeItem> items)
        {
            GetItemsTaxonomyFieldValue(folder, items);
            foreach (var item in items)
            {
                ProcessItem(item);
            }
        }
        
        protected override void ProcessGroupItems(IExchangeFolder folder, IEnumerable<IExchangeItem> items)
        {
            GetItemsTaxonomyFieldValue(folder, items);
            foreach (var item in items)
            {
                ProcessItem(item);
            }
        }

        protected override void ProcessItem(ExchangeItem item)
        {
            using (PerformanceScope scope = new PerformanceScope("RAExchangeTermUsageReportProcessor.ProcessItem"))
            {
                mLog.Info("Process Item {0}.", item.ItemId);
                try
                {
                    BuildCreatedReport(item);
                }
                catch (JobStopException ex)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception ex)
                {
                    mLog.Warn("Report item failed. item url: {0}, Error message: {1}.", item.ItemId, ex.ToString());
                }
            }
        }
        
        protected override void ProcessItem(IExchangeItem item)
        {
            using (PerformanceScope scope = new PerformanceScope("RAExchangeTermUsageReportProcessor.ProcessItem"))
            {
                mLog.Info("Process Item {0}.", item.ItemId);
                try
                {
                    BuildCreatedReport(item);
                }
                catch (JobStopException ex)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception ex)
                {
                    mLog.Warn("Report item failed. item url: {0}, Error message: {1}.", item.ItemId, ex.ToString());
                }
            }
        }

        private void BuildCreatedReport(ExchangeItem item)
        {
            JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    if (IsValidItem(item))
                    {
                        Guid termId = Guid.Empty;
                        if (GetSingleTaxonomyFieldValue(item, out termId))
                        {
                            try
                            {
                                var tempTerm = TermDao.GetRMTermByUniqueId(termId, true);
                                if (tempTerm == null || tempTerm.IsExpired || tempTerm.IsRemoved)
                                {
                                    return;
                                }
                                detail.CreatedTime = item.Created.Ticks;
                                detail.LastModifiedTime = item.Modified.Ticks;
                                detail.FileType = "msg";
                                detail.TermName = tempTerm.Name;
                                detail.Title = item.ItemName;
                                detail.ObjectLevel = "RM_EXO_LevelType_ExchangeOnlineItem";
                                detail.OperationTime = item.SendDateUTC.Equals(DateTime.MinValue) ? string.Empty : item.SendDateUTC.Ticks.ToString();
                                detail.OperationBy = item.Sender;
                                detail.URL = mCachedNodeNameForPath + item.ItemPath + "_" + item.SendDateUTC.ToString("R");
                                detail.Operation = (int)OperationType.Created;
                                detail.Status = JobDetailsStatus.Successful;
                                ReportManager.SendJobReport(ConvertToReport(detail));
                            }
                            catch (Exception ex)
                            {
                                mLog.Error("Get term error, Term id: {0}, error: {1}", termId.ToString(), ex.ToString());
                            }
                        }
                        else
                        {
                            mLog.Info($"Cannot get term value for item : {item.ItemId}.");
                        }
                    }
                    else
                    {
                        mLog.Info($"Item : {item.ItemId} isvalid = false"); ;
                    }
                }
            }
            catch (JobStopException ex)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception ex)
            {
                detail.Status = JobDetailsStatus.Failed;
                detail.Comment = commomErrorMessage;
                ReportManager.SendJobDetail(detail);
                mLog.Warn("Build Exo Created Report failed,Item:{0}. Error: {1}.", item.ItemId, ex.ToString());
            }
        }

        private void BuildCreatedReport(IExchangeItem item)
        {
            JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    if (IsValidItem(item))
                    {
                        Guid termId = Guid.Empty;
                        if (GetSingleTaxonomyFieldValue(item, out termId))
                        {
                            try
                            {
                                var tempTerm = TermDao.GetRMTermByUniqueId(termId, true);
                                if (tempTerm == null || tempTerm.IsExpired || tempTerm.IsRemoved)
                                {
                                    return;
                                }
                                detail.CreatedTime = item.Created.Ticks;
                                detail.LastModifiedTime = item.Modified.Ticks;
                                detail.FileType = "msg";
                                detail.TermName = tempTerm.Name;
                                detail.Title = item.ItemName;
                                detail.ObjectLevel = "RM_EXO_LevelType_ExchangeOnlineItem";
                                detail.OperationTime = item.SendDateUTC.Equals(DateTime.MinValue) ? string.Empty : item.SendDateUTC.Ticks.ToString();
                                detail.OperationBy = item.Sender;
                                detail.URL = mCachedNodeNameForPath + item.ItemPath + "_" + item.SendDateUTC.ToString("R");
                                detail.Operation = (int)OperationType.Created;
                                detail.Status = JobDetailsStatus.Successful;
                                ReportManager.SendJobReport(ConvertToReport(detail));
                            }
                            catch (Exception ex)
                            {
                                mLog.Error("Get term error, Term id: {0}, error: {1}", termId.ToString(), ex.ToString());
                            }
                        }
                        else
                        {
                            mLog.Info($"Cannot get term value for item : {item.ItemId}.");
                        }
                    }
                    else
                    {
                        mLog.Info($"Item : {item.ItemId} isvalid = false"); ;
                    }
                }
            }
            catch (JobStopException ex)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception ex)
            {
                detail.Status = JobDetailsStatus.Failed;
                detail.Comment = commomErrorMessage;
                ReportManager.SendJobDetail(detail);
                mLog.Warn("Build Exo Created Report failed,Item:{0}. Error: {1}.", item.ItemId, ex.ToString());
            }
        }

        protected void ProcessDeletedItem(ArchiverExchangeOnlineDto item)
        {
            using (PerformanceScope scope = new PerformanceScope("RAExchangeTermUsageReportProcessor.ProcessDeletedItem"))
            {
                mLog.Info("Process Item {0}", item.NodeID);
                try
                {
                    BuildDestroyedReport(item);
                }
                catch (JobStopException ex)
                {
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception ex)
                {
                    mLog.Warn("Report item failed. item url: {0}, Error message: {1}.", item.NodeID, ex.ToString());
                }
            }
        }
        private async Task InitRulesInfoAsync()
        {
            using (var performance = new PerformanceScope($"Report.GetRules"))
            {
                var dbRules = await RuleManagerService.GetSimpleRecordsRulesFromDBAsync();
                if (dbRules.Count > 0)
                {
                    idRuleInfoDic = dbRules.ToDictionary(key => new Guid(key.RuleId), value => value);
                }
            }
        }

        private RMRuleInfos GetRuleInfo(Guid id)
        {
            return idRuleInfoDic.ContainsKey(id) ? idRuleInfoDic[id] : null;
        }

        private void BuildDestroyedReport(ArchiverExchangeOnlineDto item)
        {
            JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    Guid termId = Guid.Empty;
                    if (GetSingleTaxonomyFieldValue(item, out termId))
                    {
                        try
                        {
                            var tempTerm = TermDao.GetRMTermByGuId(termId);
                            if (tempTerm == null || tempTerm.IsExpired || tempTerm.IsRemoved)
                            {
                                return;
                            }

                            var recordID = IDGenerator.GetRecordId(item.PartitionKey, item.NodeID);
                            var realMailBoxGuid = GetRealMailboxGuidId(mailboxStringId);
                            var record = explorerDao.ReadById(realMailBoxGuid, recordID);
                            if (record != null && record.ManualApprovedStatus != (int)Contract.SOApproveDBStatus.Rejected)
                            {
                                if (cacheAllUsers.TryGetValue(record.ManualApprovedBy, out RMAccount approveUser))
                                {
                                    detail.ApprovedBy = approveUser.DisplayName;
                                    detail.ApprovedByUPN = approveUser.UserPrincipalName;
                                }
                                detail.RecordsId = record.RecordsId;
                            }
                            if(record?.ManualArchiveStatus == (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd)
                            {
                                if (record.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WaitingApprove || record.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WorkflowInProgress)
                                {
                                    detail.InternalApprovedStatus = (int)SOApproveDBStatus.Cancelled;
                                    detail.ApprovalStatus = (int)SOApproveDBStatus.Cancelled;
                                }
                                else
                                {
                                    detail.ApprovalStatus = record?.ManualApprovedStatus ?? 0;
                                    detail.InternalApprovedStatus = record?.ManualInternalApprovedStatus ?? 0;
                                }
                            }

                            var aspd = JsonConvert.DeserializeObject<ArchiverExchangeOnlineDto>(item.JsonMeta);
                            detail.CreatedTime = aspd.CreatedTime;
                            detail.LastModifiedTime = aspd.CDLastModifiedTime;
                            detail.FileType = "msg";
                            detail.TermName = tempTerm.Name;
                            detail.Title = aspd.Title;
                            detail.ObjectLevel = "RM_EXO_LevelType_ExchangeOnlineItem";
                            detail.OperationTime = item.ArchivedTime.Equals(DateTime.MinValue.Ticks) ? string.Empty : item.ArchivedTime.ToString();
                            //detail.OperationBy = item.Operator;
                            detail.OperationBy = "RM_RC_TimeFrame_ArchiverByRASystem";
                            detail.URL = item.FullPath;
                            detail.Operation = (int)OperationType.Destroyed;
                            detail.DisposalClass = GetRuleInfo(item.RuleID)?.DisposalClass; 
                            detail.RuleName = GetRuleInfo(item.RuleID)?.RuleName;
                            detail.Status = JobDetailsStatus.Successful;
                            ReportManager.SendJobReport(ConvertToReport(detail));
                        }
                        catch (Exception ex)
                        {
                            mLog.Error("Get term error, Term id: {0}, error: {1}", termId.ToString(), ex.ToString());
                        }
                    }
                }
            }
            catch (JobStopException ex)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (Exception ex)
            {
                detail.Status = JobDetailsStatus.Failed;
                detail.Comment = commomErrorMessage;
                ReportManager.SendJobDetail(detail);
                mLog.Warn("Build Exo Created Report failed, Error: {0}", ex.ToString());
            }
        }

        public Guid GetRealMailboxGuidId(string mailboxIdWithArchive)
        {
            if (mailboxIdWithArchive.EndsWith("(Archive)"))
            {
                mailboxIdWithArchive = mailboxIdWithArchive.Substring(0, mailboxIdWithArchive.LastIndexOf("(Archive)"));
            }

            Guid mailboxGuid;
            if (Guid.TryParse(mailboxIdWithArchive, out mailboxGuid))
            {
                return mailboxGuid;
            }
            else
            {
                throw new FormatException("The mailbox ID without '(Archive)' is not a valid GUID format.");
            }
        }

        private bool IsValidItem(ExchangeItem item)
        {
            var result = false;
            if (item.SendDateUTC >= startUtcTime && item.SendDateUTC <= endUtcTime)
            {
                result = true;
            }
            return result;
        }

        private bool IsValidItem(IExchangeItem item)
        {
            var result = false;
            if (item.SendDateUTC >= startUtcTime && item.SendDateUTC <= endUtcTime)
            {
                result = true;
            }
            return result;
        }

        protected bool GetSingleTaxonomyFieldValue(ArchiverExchangeOnlineDto item, out Guid termId)
        {
            bool result = true;
            termId = new Guid();
            if (!cachedMailTermMapping.TryGetValue(item.NodeID, out termId))
            {
                try
                {
                    var aspd = JsonConvert.DeserializeObject<ArchiverExchangeOnlineDto>(item.JsonMeta);
                    termId = aspd.TermValue;
                    if (termId != Guid.Empty)
                    {
                        cachedMailTermMapping.TryAdd(item.NodeID, termId);
                    }
                    else
                    {
                        result = false;
                    }
                }
                catch (Exception ex)
                {
                    mLog.Warn("Get single taxonomy field value failed! Item url: {0}, error message: {1}.", item.NodeID, ex.ToString());
                    result = false;
                }
            }
            return result;
        }

        private CreateAndDestroyedFileReport ConvertToReport(JMCreateAndDestroyedFileReportJobDetail detail)
        {
            CreateAndDestroyedFileReport report = new CreateAndDestroyedFileReport();
            report.Title = detail.Title;
            report.LevelStr = (int)NodeLevel.ExchangeOnlineItem;
            report.OperationTime = detail.OperationTime;
            report.OperationBy = detail.OperationBy;
            report.TermName = detail.TermName;
            report.Url = detail.URL;
            report.Operation = detail.Operation;
            report.DisposalClass = detail.DisposalClass;
            report.ApprovedBy = detail.ApprovedBy;
            report.ApprovedByUPN = detail.ApprovedByUPN;
            report.CreatedTime = detail.CreatedTime;
            report.LastModifiedTime = detail.LastModifiedTime;
            report.FileType = detail.FileType;
            report.RecordsId = detail.RecordsId;
            report.RuleName = detail.RuleName;
            report.ApprovalStatus = detail.ApprovalStatus;
            report.InternalApprovedStatus = detail.InternalApprovedStatus;
            return report;
        }

        protected override void SendJobReportDetails(ExchangeFolder folder, JobDetailsStatus status, string comments = "")
        {
            JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
            detail.ObjectLevel = "RM_EXO_LevelType_ExchangeOnlineFolder";
            detail.Title = folder.FolderName;
            detail.URL = folder.ImpersonateId + folder.DisplayFolderPath;
            detail.Status = status;
            detail.Comment = comments;
            ReportManager.SendJobDetail(detail);
        }
        
        protected override void SendJobReportDetails(IExchangeFolder folder, JobDetailsStatus status, string comments = "")
        {
            JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
            detail.ObjectLevel = "RM_EXO_LevelType_ExchangeOnlineFolder";
            detail.Title = folder.FolderName;
            detail.URL = folder.ImpersonateId + folder.DisplayFolderPath;
            detail.Status = status;
            detail.Comment = comments;
            ReportManager.SendJobDetail(detail);
        }

        protected void SendJobReportDetailsForDestoryItem(ArchiverExchangeOnlineDto item, JobDetailsStatus status, string comments = "")
        {
            JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
            var aspd = JsonConvert.DeserializeObject<ArchiverExchangeOnlineDto>(item.JsonMeta);
            detail.ObjectLevel = "RM_EXO_LevelType_ExchangeOnlineItem";
            detail.Title = aspd.Title;
            detail.OperationBy = "RM_RC_TimeFrame_ArchiverByRASystem";
            detail.URL = item.FullPath;
            detail.Status = status;
            detail.Comment = comments;
            ReportManager.SendJobDetail(detail);
        }

        protected void SendJobReportDetails(ExchangeOnlineTreeNodeDto item, JobDetailsStatus status, string comments = "")
        {
            JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
            detail.ObjectLevel = "RM_EXO_LevelType_ExchangeOnlineFolder";
            detail.Title = "Top of Information Store";
            detail.URL = item.Name;
            detail.Status = status;
            detail.Comment = comments;
            ReportManager.SendJobDetail(detail);
        }
        public static bool TryGetOperationTimeUtcTicks(CreateAndDestroyedFileReport report, out long utcTimeTicks)
        {
            utcTimeTicks = 0;
            int dtLength = 16;
            //TIME_FORMAT.Length: 16
            if (!string.IsNullOrEmpty(report.OperationTime) && report.OperationTime.Length > dtLength)
            {
                try
                {
                    DateTime dt = DateTimeUtil.ConvertStringToDateTime(report.OperationTime.Substring(0, dtLength), TIME_FORMAT);
                    var zone = GetTimeZoneInfoByDisplayName(report.OperationTime.Substring(dtLength));
                    if (zone != null)
                    {
                        dt = TimeZoneInfo.ConvertTimeToUtc(dt, zone);
                        utcTimeTicks = dt.Ticks;
                        return true;
                    }
                }
                catch(Exception e)
                {
                    mLog.Warn("TryGetOperationTimeUtcTicks error {0}", e.ToString());
                }
            }
            return false;
        }
        private static TimeZoneInfo GetTimeZoneInfoByDisplayName(string displayName)
        {
            string sourceZoneStr = displayName?.Split(' ')[0];
            if (TimeZoneInfo.Local.DisplayName.StartsWith(sourceZoneStr, StringComparison.OrdinalIgnoreCase))
            {
                return TimeZoneInfo.Local;
            }
            return null;
        }
    }

    internal enum OperationType
    {
        Created = 0,
        Destroyed = 1
    }

}

