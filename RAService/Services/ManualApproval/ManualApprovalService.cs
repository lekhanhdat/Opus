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

using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.RetentionDisposal;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.Workflow;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Service.Services.ManualApproval.AuditHandler;
using AvePoint.Records.Core.Utilities.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Xml;
using AvePoint.Common;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Contract;
using System.Threading.Tasks;
using AvePoint.RA.DB.AzureTable.Model;
using RABox.Converters;

namespace AvePoint.RA.Service.Services.ManualApproval
{
    [Audit]
    public class ManualApprovalService : IManualApprovalService
    {
        private RALogger mLogger = RALogger.GetInstance(typeof(ManualApprovalService));

        private ISharePointSettingDao SharePointSettingDao => PlatformWindsorManager.GetService<ISharePointSettingDao>();

        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();

        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private IUserWrapperService UserWrapperService => PlatformWindsorManager.GetService<IUserWrapperService>();

        private IJobQueueService mJobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IManualProcessManagementService ManualProcessManagementService => PlatformWindsorManager.GetService<IManualProcessManagementService>();
        private IRMManualApproveDao ManualApproveDao { get; set; }

        private IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();

        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();

        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();

        private ITenantInfoDao TenantInfoDao => PlatformWindsorManager.GetService<ITenantInfoDao>();

        private IRMRuleDao RuleDao => PlatformWindsorManager.GetService<IRMRuleDao>();



        protected IWorkflowInstanceDao WorkflowInstanceDao => PlatformWindsorManager.GetService<IWorkflowInstanceDao>();
        protected IArchiverTableDao ArchiverTableDao => PlatformWindsorManager.GetService<IArchiverTableDao>();

        private IRMLocationDao RMLocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();
        private IPhysicalRecordSettingDao PhysicalRecordSettingDao => PlatformWindsorManager.GetService<IPhysicalRecordSettingDao>();
        private RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
                }
                return _explorerDao;
            }
        }
        private IRMWorkflowDefinitionDao RMWorkflowDefinitionDao => PlatformWindsorManager.GetService<IRMWorkflowDefinitionDao>();

        private IRMWorkflowSiteOwnersDao WorkflowSiteOwnersDao => PlatformWindsorManager.GetService<IRMWorkflowSiteOwnersDao>();

        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();

        private const string ARCHIVER_XML_NODE_METADATA = "MetaData";
        private const string ARCHIVER_XML_NODE_NAME = "Name";
        private const string ARCHIVER_XML_NODE_VALUE = "Value";
        private const string SP_FIELD_CONTENTTYPE_NAME = "content type";
        private const string SP_FIELD_MODIFIEDBY_NAME = "editor";
        private const string SP_FIELD_CREATEDBY_NAME = "author";
        private const string FS_XML_NODE_PROPERTY = "Property";
        private const string FS_FIELD_CREATEDBY_NAME = "CreatedBy";
        private const string FS_FIELD_MODIFIEDBY_NAME = "ModifiedBy";

        private string[] mColumnNames = new string[] { "LeafName", "Status", "ContentType", "ModifiedBy", "CreatedBy", "RuleId", "EscalateFrom", "EscalateTo", "ApprovedBy", "SourceFlag", "DisposalClass" };
        public string serverUrl { get; set; }
        private Dictionary<Guid, Guid> daoRecordSiteIdMapping = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, Guid> daoRecordGroupIdMapping = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, Guid> daoRecordMailGroupIdMapping = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, Guid> daoRecordMailBoxIdMapping = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, Guid> daoRecordOneDriveSiteIdMapping = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, Guid> daoRecordOneDriveGroupIdMapping = new Dictionary<Guid, Guid>();
        private IJobMonitorService mJobService;
        protected IJobMonitorService RMJobService
        {
            get
            {
                if (mJobService == null)
                {
                    mJobService = (IJobMonitorService)PlatformWindsorManager.GetService(typeof(IJobMonitorService));
                }
                return mJobService;
            }
        }
        
        public bool IsNeedUpgradeLoading()
        {
            //var hasMessage = mJobQueueService.GetMessagesCount(TenantLocalValue.LogonGroupId, JobType.ManualApprovalTimer) > 0;
            //var hasRunningJob = JobMonitorService.GetRunningJobsCount(JobType.ManualApprovalTimer) > 0;
            //var needUpgrade = TenantInfoDao.NeedUpgradeManualData(TenantLocalValue.LogonGroupId);
            //return needUpgrade && !hasMessage && !hasRunningJob;
            var needUpgrade = TenantInfoDao.NeedUpgradeManualData(TenantLocalValue.LogonGroupId);
            if(!needUpgrade)
            {
                return false;
            }
            var hasWorkflowData = ManualApproveDao.HasWorkflowData();
            return hasWorkflowData;
        }

        public void UpgradeManualApprovalDataJob()
        {
            ManualApproveDao = new RMManualApproveDao();
            try
            {
                var needUpgrade = TenantInfoDao.NeedUpgradeManualData(TenantLocalValue.LogonGroupId);
                if (!needUpgrade)
                {
                    return;
                }

                var hasData = ManualApproveDao.HasData();
                if (!hasData)
                {
                    mLogger.Info($"Current tenant dons't use manual review. Upgrade skipped.");
                    TenantInfoDao.UpdateManualDataUpgradeStatusToSuccessful(TenantLocalValue.LogonGroupId);
                    return;
                }

                var hasWorkflowData = ManualApproveDao.HasWorkflowData();
                if (!hasWorkflowData)
                {
                    mLogger.Info($"Current tenant dons't has workflow data. Upgrade skipped. Next manual approval schedule execute upgrade.");
                    return;
                }

                RunManualApprovalTimerJob(JobRunBy.Schedule);
            }
            catch (Exception e)
            {
                mLogger.Error($"An error occurred while run upgrade manual approval data job. Error: {e}");
            }
        }

        public string RunManualApprovalJob(JobRunBy jobRunBy)
        {
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ManualApproval,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                id = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                mLogger.Error("error occurred while RunManualApprovalJob,ERROR:{0}", ex.ToString());
            }

            return id;
        }

        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ManualApproval, Action = AuditAction.RunManualApproval, AfterHandler = typeof(ManualApprovalAfterAuditHandler))]
        public string RealRunManualApprovalJob(JobRunBy jobRunBy, string jobRunByUser)
        {
            string jobId = string.Empty;
            JobType jobType = JobType.ManualApproval;
            //起Job，判断是前台起Job还是Schedule起的Job
            if (jobRunBy == JobRunBy.Control)
            {
                jobId = JobMonitorService.CreateJob(jobType, jobRunByUser);
                mLogger.Info("Begin control run job {0}", jobId);
            }
            else if (jobRunBy == JobRunBy.Schedule)
            {
                jobId = JobMonitorService.CreateJob(jobType, "RM_TS_RunSchedule");
                mLogger.Info("Begin schedule run Job {0}", jobId);
            }
            else
            {
                jobId = JobMonitorService.CreateJob(jobType, jobRunByUser);
                mLogger.Info("Begin default run Job {0}", jobId);
            }

            //查询当前还没有结束的 Job
            List<string> runningJobs = JobMonitorService.GetRunningJobs(jobType);

            //Job一次只能同时运行一个，所以判断当前起的Job是否要Skip掉
            bool isSkip = runningJobs.Any(j => j != jobId);
            if (!isSkip)
            {
                mJobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = jobId,
                    RunBy = jobRunBy,
                    JobType = jobType,
                    CommandLine = string.Format("{0} {1}", jobType, jobId),
                });
            }
            else
            {
                mLogger.Info(I18NEntity.GetString("RM_SYNC_JobSkip")); //RM_SYNC_JobSkip to do
                JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SYNC_JobSkip");
            }

            return jobId;
        }
        
        public RAReturnMessage RunApprovedOrRejectedJob(ManualReviewJobQuery query) {
            RAReturnMessage returnMessage = new RAReturnMessage();
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                var accountId = TenantLocalValue.LogonUserId;
                query.UserId = accountId;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ManualApprovalOrRejectJob,
                    Parameters = SerializerHelper.SerializeByJsonSerializer(query),
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                string a = SerializerHelper.SerializeByJsonSerializer(query);
                ManualReviewJobQuery m = SerializerHelper.DeserializeByJsonSerializer<ManualReviewJobQuery>(jqDto.Parameters);
                returnMessage.MessageType = RAMessageType.Successful;
                returnMessage.Extension = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                mLogger.Error("error occurred while RunApprovedOrRejectedJob,ERROR:{0}", ex.ToString());
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = ex.Message;
            }
            return returnMessage;
        }
        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ManualApprovalTimer, Action = AuditAction.RunManualApproveOrReject, AfterHandler = typeof(ManualApprovalAfterAuditHandler))]
        public string RealRunApprovedOrRejectedJob(string query) {
            string jobId = string.Empty;
            string jobRunByUser = TenantLocalValue.LogonUserEmail;
            try
            {
                var jobType = JobType.ManualApprovalOrRejectJob;
                jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                SubJobDao.UpdateSubJobCount(jobId, 1);
                var subJobId = CreateSubJob(jobId, 0, jobType, JobStatus.InProgress, 1, query);
                List<string> runningJobs = JobMonitorService.GetRunningJobs(JobType.ManualApprovalOrRejectJob);
                bool isSkip = runningJobs.Any(j => j != jobId);
                if (!isSkip) 
                {
                    mJobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = JobRunBy.Control,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", jobType, subJobId),
                    });
                }
                else
                {
                    mLogger.Info(I18NEntity.GetString("RM_SYNC_JobSkip")); //RM_SYNC_JobSkip to do
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SYNC_JobSkip");
                }
            }
            catch (Exception ex)
            {
                mLogger.Error($"Error in RealRunApprovedOrRejectedJob, reason : {ex.ToString()}.");
            }
            return jobId;
        }
        private string CreateSubJob(string jobId, int currentSubjobIndex, JobType jobType, JobStatus jobState, int subJobCount, string jobMessage, string string1 = null)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob()
            {
                Id = subJobId,
                ParentId = jobId,
                StartTime = DateTime.UtcNow.Ticks,
                JobType = (int)jobType,
                Progress = 0,
                Status = (int)jobState,
                Weight = 100d / subJobCount,
                String1 = string1,
                LastUpdateTime = DateTime.UtcNow.Ticks
            };
            if (jobState == JobStatus.Wait)
            {
                subJob.Runable = RecordsConstants.SubJob_Runnable_CanRun;
            }
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Content = jobMessage };
            SubJobDao.CreateJob(subJob);
            mLogger.Info("Create sub job {0} sucessfull, type {1}, weight {2}, state {3}, string1 {4} ", subJob.Id, subJob.JobType, subJob.Weight, subJob.Status, string1);
            return subJobId;
        }

        public IEnumerable<ManualExportReportInfo> GetManualExportReports(AzureTableConnectContract connectionInfo, string tenantGroupId, SourceFlag source)
        {
            var datas = ArchiverTableDao.GetWaitingApprovalDatas(connectionInfo, tenantGroupId, source);
            return datas.Select(e => ConvertToManualExportReport(e));
        }

        public List<ManualExportReportInfo> GetManualExportReportsForEXO(AzureTableConnectContract connectionInfo, string tenantGroupId)
        {
            var datas = ArchiverTableDao.GetWaitingApprovalDatasForEXO(connectionInfo, tenantGroupId);
            return datas.Select(e => ConvertToManualExportReportForEXO(e)).ToList();
        }

        public List<ManualExportReportInfo> GetManualExportReportsForPhysical()
        {
            var datas = ExplorerDao.GetWaitingApproveItemForPhysical();
            return datas.Select(e => ConvertToManualExportReportForPhysical(e)).ToList();
        }

        public List<ManualExportReportInfo> GetManualExportReportsForFS(string fsAzureTableConnectStr, string tenantGroupId)
        {
            var datas = ArchiverTableDao.GetWaitingApprovalDatasForFS(fsAzureTableConnectStr, tenantGroupId);
            return datas.Select(e => ConvertToManualExportReportForFS(e)).ToList();
        }

        public List<ManualExportReportInfo> GetManualExportReportsForSPOnPrem(string connectString, string tenantGroupId)
        {
            var datas = ArchiverTableDao.GetWaitingApprovalDatasForSPOnPrem(connectString, tenantGroupId);
            return datas.Select(e => ConvertToManualExportReportForSPOnPrem(e)).ToList();
        }

        private string GetAuditXMLString(RMManualApprove item, string action, string operateBy)
        {
            List<ReviewAudits> audits = new List<ReviewAudits>();
            ReviewAudits audit = new ReviewAudits();
            audit.ReviewTime = DateTime.UtcNow.Ticks.ToString();
            audit.ReviewBy = operateBy;
            audit.Action = action;// approveOrReject.Equals(SOApproveDBStatus.Approved) ? "Approved" : "Rejected";
            if (!string.IsNullOrEmpty(item.Audits))
            {
                audits = SerializerHelper.DeserializeFromXmlString<List<ReviewAudits>>(item.Audits);
            }
            audits.Add(audit);
            return SerializerHelper.SerializeToXmlString(audits);
        }

        public List<RecordOwnerGroupDto> GetReportsManagement(IEnumerable<ManualExportReportInfo> reports, ref Dictionary<Guid, Guid> groupIdMapping, ref Dictionary<Guid, Guid> siteIdMapping)
        {
            var recordGroupIds = new List<Guid>();
            var recordSiteIds = new List<Guid>();
            foreach (var item in reports)
            {
                AddDaoRecordSPNodeIdCache(item);
            }
            if (daoRecordGroupIdMapping.Count > 0)
            {
                groupIdMapping = daoRecordGroupIdMapping;
                recordGroupIds = daoRecordGroupIdMapping.Values.Distinct().ToList();
                AddNodeIdCacheLog(daoRecordGroupIdMapping, SourceFlag.SharePoint);

            }
            if (daoRecordSiteIdMapping.Count > 0)
            {
                siteIdMapping = daoRecordSiteIdMapping;
                recordSiteIds = daoRecordSiteIdMapping.Values.Distinct().ToList();
                AddNodeIdCacheLog(daoRecordSiteIdMapping, SourceFlag.SharePoint);
            }
            return SharePointSettingDao.GetRecordOwners(recordGroupIds, recordSiteIds);
        }

        public List<RecordOwnerGroupDto> GetReportsManagementForSPLocal(IEnumerable<ManualExportReportInfo> reports)
        {
            var groupIds = reports.Select(o => o.SiteGroupID).Distinct().ToList();
            var siteIds = reports.Select(o => o.RegistedSiteId).Distinct().ToList();
            return SharePointSettingDao.GetRecordOwnersForSPLocal(groupIds, siteIds);
        }

        public List<RecordOwnerGroupDto> GetReportsManagementForOneDrive(IEnumerable<ManualExportReportInfo> reports, ref Dictionary<Guid, Guid> groupIdMapping, ref Dictionary<Guid, Guid> siteIdMapping)
        {
            var recordGroupIds = new List<Guid>();
            var recordSiteIds = new List<Guid>();
            foreach (var item in reports)
            {
                AddDaoRecordOneDriveNodeIdCache(item);
            }
            if (daoRecordOneDriveGroupIdMapping.Count > 0)
            {
                groupIdMapping = daoRecordOneDriveGroupIdMapping;
                recordGroupIds = daoRecordOneDriveGroupIdMapping.Values.Distinct().ToList();
                AddNodeIdCacheLog(daoRecordOneDriveGroupIdMapping, SourceFlag.OneDrive);

            }
            if (daoRecordOneDriveSiteIdMapping.Count > 0)
            {
                siteIdMapping = daoRecordOneDriveSiteIdMapping;
                recordSiteIds = daoRecordOneDriveSiteIdMapping.Values.Distinct().ToList();
                AddNodeIdCacheLog(daoRecordOneDriveSiteIdMapping, SourceFlag.OneDrive);
            }
            return SharePointSettingDao.GetRecordOwnersForOneDrive(recordGroupIds, recordSiteIds);
        }

        private void AddDaoRecordOneDriveNodeIdCache(ManualExportReportInfo item)
        {
            var daoSiteId = item.RegistedSiteId;
            var daoGroupId = item.SiteGroupID;
            if (!daoRecordOneDriveSiteIdMapping.ContainsKey(daoSiteId))
            {
                var recordSiteNode = RABrowserClient.GetRemoteSiteCollectionByUrl(item.SiteUrl);
                if (recordSiteNode != null)
                {
                    daoRecordOneDriveSiteIdMapping.Add(daoSiteId, new Guid(recordSiteNode.id));
                    if (!daoRecordOneDriveGroupIdMapping.ContainsKey(daoGroupId))
                    {
                        daoRecordOneDriveGroupIdMapping.Add(daoGroupId, new Guid(recordSiteNode.parentId));
                    }
                }
            }
        }

        private void AddDaoRecordSPNodeIdCache(ManualExportReportInfo item)
        {
            var daoSiteId = item.RegistedSiteId;
            var daoGroupId = item.SiteGroupID;
            if (!daoRecordSiteIdMapping.ContainsKey(daoSiteId))
            {
                var recordSiteNode = RABrowserClient.GetRemoteSiteCollectionByUrl(item.SiteUrl);
                if (recordSiteNode != null)
                {
                    daoRecordSiteIdMapping.Add(daoSiteId, new Guid(recordSiteNode.id));
                    if (!daoRecordGroupIdMapping.ContainsKey(daoGroupId))
                    {
                        daoRecordGroupIdMapping.Add(daoGroupId, new Guid(recordSiteNode.parentId));
                    }
                }
            }
        }

        private void AddNodeIdCacheLog(Dictionary<Guid, Guid> nodeIdDic, SourceFlag source)
        {
            foreach (var key in nodeIdDic.Keys)
            {
                mLogger.Info($"Datasource:{source}, dao node id:[{key}], record node Id:[{nodeIdDic[key]}]");
            }
        }
        private Guid GetRootLocationId(Guid currentLocationId)
        {
            var curLocation = RMLocationDao.GetLocationByUniqueId(currentLocationId);
            var locationIds = curLocation.DirPath.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            bool isRoot = locationIds.Count == 1;
            if (isRoot)
            {
                return currentLocationId;
            }
            else
            {
                var rootLocation = RMLocationDao.GetLocationById(Convert.ToInt32(locationIds[1]));
                return rootLocation.UniqueId;
            }
        }

        public List<RecordOwnerGroupDto> GetRecordOwnerGroupForPhysical(IEnumerable<ManualExportReportInfo> manualItems)
        {
            Dictionary<Guid, Guid> locationMapping = new Dictionary<Guid, Guid>();
            foreach (var item in manualItems)
            {
                var topLocationId = GetRootLocationId(item.LocationID);
                item.TopLocationID = topLocationId;
                if (!locationMapping.ContainsKey(item.LocationID))
                {
                    locationMapping.Add(item.LocationID, topLocationId);
                }
            }
            return PhysicalRecordSettingDao.GetRecordOwners(locationMapping.Values.Distinct().ToList());
        }

        public List<RecordOwnerGroupDto> GetReportsManagementForEXO(IEnumerable<ManualExportReportInfo> reports, ref Dictionary<Guid, Guid> mailGroupIdMapping, ref Dictionary<Guid, Guid> mailBoxIdMapping)
        {
            var parentIds = new List<Guid>();
            var currentNodeIds = new List<Guid>();
            foreach (var item in reports)
            {
                AddDaoRecordExchangeNodeIdCache(item);
            }

            if (daoRecordMailGroupIdMapping.Count > 0)
            {
                mailGroupIdMapping = daoRecordMailGroupIdMapping;
                parentIds = daoRecordMailGroupIdMapping.Values.Distinct().ToList();
                AddNodeIdCacheLog(daoRecordMailGroupIdMapping, SourceFlag.Exchange);
            }
            if (daoRecordMailBoxIdMapping.Count > 0)
            {
                mailBoxIdMapping = daoRecordMailBoxIdMapping;
                currentNodeIds = daoRecordMailBoxIdMapping.Values.Distinct().ToList();
                AddNodeIdCacheLog(daoRecordMailBoxIdMapping, SourceFlag.Exchange);
            }
            return SharePointSettingDao.GetRecordOwnersForEXO(parentIds, currentNodeIds);
        }

        private void AddDaoRecordExchangeNodeIdCache(ManualExportReportInfo item)
        {
            var daoMailBoxId = item.MailBoxID;
            var daoMailGroupId = item.SiteGroupID;
            if (!daoRecordMailBoxIdMapping.ContainsKey(daoMailBoxId))
            {
                var recordMailBox = RABrowserClient.GetExchangeNodeByMailBox(item.SiteUrl);
                if (recordMailBox != null)
                {
                    daoRecordMailBoxIdMapping.Add(daoMailBoxId, new Guid(recordMailBox.ID));
                    if (!daoRecordMailGroupIdMapping.ContainsKey(daoMailGroupId))
                    {
                        daoRecordMailGroupIdMapping.Add(daoMailGroupId, new Guid(recordMailBox.ParentId));
                    }
                }
            }
        }

        public string GetSPOnlineADGroupName(string tenantId, string objectId)
        {
            var group = UserWrapperService.GetGroupByObjectId(tenantId, objectId);
            if (group != null)
            {
                return group.DisplayName;
            }
            else
            {
                return null;
            }
        }

        public Account GetSPOnlineADUser(string tenantId, string objectId)
        {
            return UserWrapperService.GetUserByObjectId(tenantId, objectId);
        }

        public HashSet<string> GetAllUserEMailsFromGroup(string tenantId, string objectID)
        {
            return UserWrapperService.GetAllUserEMailsFromGroup(tenantId, objectID);
        }

        public void UpdateGroupRecordOwner(RecordOwnerDto owner)
        {
            SharePointSettingDao.UpdateRecordOwnerUserPrincipalName(owner);
        }

        public async System.Threading.Tasks.Task MarkApprovalingObjectsToExportedStatusAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, Dictionary<string, List<string>> rowkeysGroupBySite)
        {
            foreach (var rowkeys in rowkeysGroupBySite)
            {
                await ArchiverTableDao.UpdateItemsToExportedStatusAsync(connectionInfo, tenantGroupId, rowkeys.Key.ToString(), rowkeys.Value);
            }
        }

        public async System.Threading.Tasks.Task MarkApprovalingObjectsToNotExportedStatusAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, Dictionary<string, List<string>> rowkeysGroupBySite)
        {
            foreach (var rowkeys in rowkeysGroupBySite)
            {
                await ArchiverTableDao.UpdateItemsToNotExportedStatusAsync(connectionInfo, tenantGroupId, rowkeys.Key, rowkeys.Value);
            }
        }

        public async System.Threading.Tasks.Task MarkApprovalingObjectsToApprovedStatusAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, Dictionary<string, List<string>> rowkeysGroupBySite)
        {
            foreach (var rowkeys in rowkeysGroupBySite)
            {
                await ArchiverTableDao.UpdateItemsToApprovedStatusAsync(connectionInfo, tenantGroupId, rowkeys.Key, rowkeys.Value);
            }
        }

        public void MarkApprovalingObjectsToRejectedStatus(AzureTableConnectContract connectionInfo, string tenantGroupId, Dictionary<string, List<string>> rowkeysGroupBySite)
        {
            foreach (var rowkeys in rowkeysGroupBySite)
            {
                ArchiverTableDao.DeleteItemsByRowKey(connectionInfo, tenantGroupId, rowkeys.Key, rowkeys.Value);
            }
        }
        private RMReportObjectLevel GetObjectLevelForPhysical(RMNodeType rmNodeType)
        {
            RMReportObjectLevel rmReportLevel;
            switch (rmNodeType)
            {
                case RMNodeType.PhyBox:
                    rmReportLevel = RMReportObjectLevel.PhysicalBox;
                    break;
                case RMNodeType.PhyFile:
                    rmReportLevel = RMReportObjectLevel.PhysicalFile;
                    break;
                default:
                    rmReportLevel = (RMReportObjectLevel)0;
                    break;
            }
            return rmReportLevel;
        }
        private ManualExportReportInfo ConvertToManualExportReportForPhysical(Record entity)
        {
            var info = new ManualExportReportInfo()
            {
                LeafName = entity.LeafName,
                LocationID = entity.LocationId,
                RuleID = entity.RuleId.ToString(),
                ScopeID = entity.ScopeId.ToString(),
                Status = (SOApproveDBStatus)entity.DisposalStatus,
                ObjectLevel = GetObjectLevelForPhysical((RMNodeType)entity.NodeType),
                NodeID = entity.Id,
                ArchivedTime = entity.DestroyedTime,
                CreatedBy = entity.CreatedBy,
                ModifiedBy = entity.ModifiedBy,
                Path = GetFullPathForPhysical(entity),
                ExportToRECO = entity.ExportToRECO,
                RecordStatus = (RMRecordStatus)entity.RecordStatus,
                HasRelatedDocument = entity.RelatedRecordsCount,
                DeleteRelatedRecords = entity.DeleteRelatedRecords,
                RelatedRecordInfo = entity.RelatedRecords
            };
            return info;
        }
        private string GetFullPathForPhysical(Record entity)
        {
            return $"{ExplorerService.GetPhysicalObjectFullPath(entity.Id)}/{entity.LeafName}";
        }
       /* private string GetPhysicalBoxPath(Guid boxId)
        {
            string result = string.Empty;
            try
            {
                var box = ExplorerDao.QueryAll(r => r.Id == boxId).FirstOrDefault();
                if(box != null)
                {
                    result = GetPhysicalLocationPath((Guid)(box?.LocationId));
                }
                if (!string.IsNullOrEmpty(result))
                {
                    result += string.Format($"/{box?.LeafName}");
                }
            }
            catch (Exception e)
            {
                mLogger.Error($"Get Path by box id: [{boxId}], error: {e.ToString()}");
            }
            return result;
        }*/
       /* private string GetPhysicalLocationPath(Guid locationId)
        {
            var result = string.Empty;
            try
            {
                var tempLocation = RMLocationDao.GetLocationByUniqueId(locationId);
                if (tempLocation != null)
                {
                    result = string.Format($"{tempLocation.PathForDisplay}/{tempLocation.Name}");
                }
            }
            catch (Exception ex)
            {
                mLogger.Error($"Get Path by location id: [{locationId}], error: {ex.ToString()}");
            }
            return result;
        }*/

        private ManualExportReportInfo ConvertToManualExportReport(ArchiverTableEntity entity)
        {
            var aspd = JsonConvert.DeserializeObject<ArchiverSharePointDto>(entity.JsonMeta);
            var objectLevel = GetObjectLevel(entity);
            var leafName = objectLevel == RMReportObjectLevel.SiteCollection || objectLevel == RMReportObjectLevel.Site ? aspd.SiteTitle : aspd.LeafName;
            var info = new ManualExportReportInfo()
            {
                PartKey = entity.PartitionKey,
                LeafName = leafName,
                SiteGroupID = aspd.SiteGroupId,
                SiteID = aspd.SiteId,
                RegistedSiteId = aspd.RegistedSiteId,
                WebID = aspd.WebId,
                ListID = aspd.ListId,
                NodeID = entity.NodeID,
                ParentID = entity.ParentID,
                RowKey = entity.RowKey,
                SiteUrl = aspd.SiteUrl,
                ArchiveLevel = entity.ArchiveLevel,
                Level = entity.CacheNodeType,
                RuleID = entity.RuleID.ToString(),
                ScanJobId = entity.ScanJobID,
                ScopeID = entity.ScopeID.ToString(),
                Status = (SOApproveDBStatus)entity.Status,
                UIVersion = entity.UIVersion,
                ObjectLevel = objectLevel,
                JsonMeta = entity.JsonMeta,
                DeleteRelatedRecords = entity.DeleteRelatedRecords,
                HasRelatedDocument = entity.HasRelatedDocument,
                RelatedRecordInfo = entity.RelatedRecordInfo,
                RetentionStatus = entity.SourceFlag == (int)SourceFlag.LifecycleRetention ? 1 : 0
            };
            if (!string.IsNullOrEmpty(aspd.Metadata))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(aspd.Metadata);
                XmlNode root = doc.SelectSingleNode(ARCHIVER_XML_NODE_METADATA);
                foreach (XmlNode node in root.ChildNodes)
                {
                    string fieldName = node.Attributes[ARCHIVER_XML_NODE_NAME].Value;
                    string fieldValue = node.Attributes[ARCHIVER_XML_NODE_VALUE].Value;
                    switch (fieldName)
                    {
                        case SP_FIELD_CONTENTTYPE_NAME:
                            info.ContentType = fieldValue;
                            break;
                        case SP_FIELD_CREATEDBY_NAME:
                            info.CreatedBy = fieldValue;
                            break;
                        case SP_FIELD_MODIFIEDBY_NAME:
                            info.ModifiedBy = fieldValue;
                            break;
                    }
                }
            }
            info.Path = GetFullPath(aspd, info.ObjectLevel);
            info.ServerRelativeUrl = aspd.Path;
            return info;
        }

        private ManualExportReportInfo ConvertToManualExportReportForEXO(ArchiverExchangeOnlineDto entity)
        {
            var aspd = JsonConvert.DeserializeObject<ArchiverExchangeOnlineDto>(entity.JsonMeta);
            var info = new ManualExportReportInfo()
            {
                PartKey = entity.PartitionKey,
                LeafName = aspd.Title,
                SiteGroupID = string.IsNullOrEmpty(entity.MailBoxGroupID) ? Guid.Empty : new Guid(entity.MailBoxGroupID),
                SiteID = Guid.Empty,
                RegistedSiteId = Guid.Empty,
                WebID = Guid.Empty,
                ListID = Guid.Empty,
                NodeID = entity.NodeID.ToMd5(),
                ParentID = entity.ParentID.ToMd5(),
                RowKey = entity.RowKey,
                SiteUrl = GetMailBoxUrl(entity.FullPath),
                ArchiveLevel = entity.ArchiveLevel,
                ArchivedTime = entity.ArchivedTime,
                Level = entity.CacheNodeType,
                RuleID = entity.RuleID.ToString(),
                ScanJobId = entity.ScanJobID,
                Status = (SOApproveDBStatus)entity.Status,
                ObjectLevel = RMReportObjectLevel.ExchangeOnlineItem,
                JsonMeta = entity.JsonMeta,
                DeleteRelatedRecords = entity.DeleteRelatedRecords,
                HasRelatedDocument = entity.HasRelatedDocument,
                RelatedRecordInfo = entity.RelatedRecordInfo,
                //MailBoxID = string.IsNullOrEmpty(entity.MailBoxID) ? Guid.Empty : new Guid(entity.MailBoxID),
                CreatedBy = aspd.SendFrom,
                ModifiedBy = aspd.ModifiedBy
            };
            var mailboxId = entity.MailBoxID;
            if(mailboxId.IndexOf("(Archive)") != -1)
            {
                mailboxId = mailboxId.Substring(0, mailboxId.IndexOf("(Archive)"));
            }
            info.MailBoxID = string.IsNullOrEmpty(mailboxId) ? Guid.Empty : new Guid(mailboxId);
            info.Path = entity.FullPath;
            info.ServerRelativeUrl = string.Empty;
            return info;
        }

        private ManualExportReportInfo ConvertToManualExportReportForSPOnPrem(OnPremiseSPTableEntity entity)
        {
            var aspd = JsonConvert.DeserializeObject<OnPremiseArchiverSharePointDto>(entity.JsonMeta);
            var objectLevel = RMReportObjectLevel.Item;
            var leafName = aspd.LeafName;
            var info = new ManualExportReportInfo()
            {
                PartKey = entity.PartitionKey,
                LeafName = leafName,
                SiteGroupID = aspd.SiteGroupId,
                SiteID = new Guid(aspd.SiteId),
                RegistedSiteId = new Guid(aspd.RegistedSiteId),
                WebID = aspd.WebId,
                ListID = aspd.ListId,
                NodeID = entity.NodeID,
                ParentID = entity.ParentID,
                RowKey = entity.RowKey,
                SiteUrl = aspd.SiteUrl,
                ArchiveLevel = entity.ArchiveLevel,
                Level = entity.CacheNodeType,
                RuleID = entity.RuleID.ToString(),
                ScanJobId = entity.ScanJobID,
                ScopeID = entity.ScopeID.ToString(),
                Status = (SOApproveDBStatus)entity.Status,
                UIVersion = entity.UIVersion,
                ObjectLevel = objectLevel,
                JsonMeta = entity.JsonMeta,
                //DeleteRelatedRecords = entity.DeleteRelatedRecords,
                //HasRelatedDocument = entity.HasRelatedDocument,
                //RelatedRecordInfo = entity.RelatedRecordInfo
            };
            if (!string.IsNullOrEmpty(aspd.Metadata))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(aspd.Metadata);
                XmlNode root = doc.SelectSingleNode(ARCHIVER_XML_NODE_METADATA);
                foreach (XmlNode node in root.ChildNodes)
                {
                    string fieldName = node.Attributes[ARCHIVER_XML_NODE_NAME].Value;
                    string fieldValue = node.Attributes[ARCHIVER_XML_NODE_VALUE].Value;
                    switch (fieldName)
                    {
                        case SP_FIELD_CONTENTTYPE_NAME:
                            info.ContentType = fieldValue;
                            break;
                        case SP_FIELD_CREATEDBY_NAME:
                            info.CreatedBy = fieldValue;
                            break;
                        case SP_FIELD_MODIFIEDBY_NAME:
                            info.ModifiedBy = fieldValue;
                            break;
                    }
                }
            }
            info.Path = GetFullPathForSPOnPrem(aspd, info.ObjectLevel);
            info.ServerRelativeUrl = aspd.Path;
            return info;
        }

        private string GetMailBoxUrl(string mailItemUrl)
        {
            if (!string.IsNullOrEmpty(mailItemUrl))
            {
                return mailItemUrl.Split('\\').ToList()[0];
            }
            return "";
        }

        private ManualExportReportInfo ConvertToManualExportReportForFS(FileSystemTableEntity entity)
        {
            var info = new ManualExportReportInfo()
            {
                PartKey = entity.PartitionKey,
                RowKey = entity.RowKey,
                LeafName = entity.LowName,
                NodeID = new Guid(entity.RowKey),//文件路径md5
                ParentID = entity.ParentID, //parent folder md5
                Level = entity.NodeLevel,
                RuleID = entity.RuleId.ToString(),
                ScopeID = entity.CurrentSettingId != Guid.Empty ? entity.CurrentSettingId.ToString() : entity.ScopeID.ToString(),
                Status = (SOApproveDBStatus)entity.Status,
                ObjectLevel = RMReportObjectLevel.FSFile,
                ArchivedTime = entity.AchiveTime.Ticks,
                ExportToRECO = entity.MovedToApprovalTable,
                DeleteRelatedRecords = entity.DisposalAction ? 1 : 0,
                RelatedRecordInfo = entity.RelatedRecordInfo,
                //Path = GetFSItemFullPath(entity.HighName, entity.LowName)
                Path = entity.FullPath
            };

            if (!string.IsNullOrEmpty(entity.Property))
            {
                //处理扩展属性逻辑
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(entity.Property);
                XmlNode root = doc.SelectSingleNode(FS_XML_NODE_PROPERTY);
                foreach (XmlNode node in root.ChildNodes)
                {
                    string fieldName = node.Attributes[ARCHIVER_XML_NODE_NAME].Value;
                    string fieldValue = node.Attributes[ARCHIVER_XML_NODE_VALUE]?.Value;
                    switch (fieldName)
                    {
                        case FS_FIELD_CREATEDBY_NAME:
                            info.CreatedBy = fieldValue;
                            break;
                        case FS_FIELD_MODIFIEDBY_NAME:
                            info.ModifiedBy = fieldValue;
                            break;
                    }
                }
            }
            return info;
        }

        public string GetFSItemFullPath(string highName, string lowName)
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(highName))
            {
                sb.AppendFormat("{0}", highName);
            }
            if (!string.IsNullOrEmpty(lowName))
            {
                sb.AppendFormat("\\{0}", lowName);
            }
            return sb.ToString();
        }

        private string GetFullPath(ArchiverSharePointDto info, RMReportObjectLevel objectLevel)
        {
            string fullPathUrl = string.Empty;
            try
            {
                if (info.Path.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                    || info.Path.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase)
                    || objectLevel == RMReportObjectLevel.SiteCollection)
                {
                    fullPathUrl = info.Path;
                }
                else
                {
                    var level = objectLevel;

                    if (level == RMReportObjectLevel.Site || level == RMReportObjectLevel.Folder || level == RMReportObjectLevel.List || level == RMReportObjectLevel.Item)
                    {
                        fullPathUrl = WebUtil.MakeFullUrl(info.SiteUrl, info.Path);
                    }

                    else if (level == RMReportObjectLevel.Attachment)
                    {
                        try
                        {
                            string baseUrl = info.SiteUrl.Length > 8 ? info.SiteUrl.IndexOf('/', 8) > 0 ? info.SiteUrl.Substring(0, info.SiteUrl.IndexOf('/', 8)) : info.SiteUrl : string.Empty;
                            int indexRealName = info.LeafName.IndexOf(':');
                            int id = 0;
                            string realName = string.Empty;
                            string listServerRelatedUrl = string.Empty;
                            id = Convert.ToInt32(info.LeafName.Substring(0, info.LeafName.IndexOfAny(new char[] { '_', '.' })));
                            realName = info.LeafName.Substring(indexRealName + 1);
                            string list = "Lists/";
                            int listUrlLength = info.Path.IndexOf(list, StringComparison.OrdinalIgnoreCase) + list.Length;
                            string listUrl = info.Path.Substring(0, listUrlLength);
                            string subUrl = info.Path.Substring(listUrlLength);
                            //sites/gaoxinqu/Lists/Tasks/aaa/bbb/ccc\5_.000  -> subfold's item attachment
                            int index = subUrl.IndexOf('/');
                            if (index == -1)
                            {
                                listServerRelatedUrl = (listUrl + subUrl.Substring(0, subUrl.IndexOf('\\'))).TrimStart('/');
                            }
                            else
                            {
                                //sites/gaoxinqu/Lists/Tasks\1_.000  -> rootfold's attachment
                                listServerRelatedUrl = (listUrl + subUrl.Substring(0, index)).TrimStart('/');
                            }
                            fullPathUrl = baseUrl + @"/" + listServerRelatedUrl + @"/Attachments/" + id + @"/" + realName;
                        }
                        catch (Exception ex)
                        {
                            mLogger.Warn("Error in Get Attachment Full Url" + ex.ToString());
                            fullPathUrl = info.Path;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(fullPathUrl))
                {
                    fullPathUrl = fullPathUrl.Replace("\\", "/");
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("get full path error, node id:{0}, url:{1} error:{2}", info?.NodeID, info?.SiteUrl, ex.ToString());
            }

            return fullPathUrl;
        }

        private string GetFullPathForSPOnPrem(OnPremiseArchiverSharePointDto info, RMReportObjectLevel objectLevel)
        {
            string fullPathUrl = "";
            if (info.Path.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) || info.Path.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
            {
                fullPathUrl = info.Path;
            }
            else
            {
                fullPathUrl = WebUtil.MakeFullUrl(info.SiteUrl, info.Path);
            }
            return fullPathUrl;
        }

        private RMReportObjectLevel GetObjectLevel(ArchiverTableEntity entity)
        {
            RMReportObjectLevel level = RMReportObjectLevel.Item;
            if (entity.CacheNodeType == (int)CacheNodeType.SiteCollection)
            {
                level = RMReportObjectLevel.SiteCollection;
            }
            else if ((entity.CacheNodeType >= (int)CacheNodeType.Web) && (entity.CacheNodeType < (int)CacheNodeType.List))
            {
                level = RMReportObjectLevel.Site;
            }
            else if (entity.CacheNodeType == (int)CacheNodeType.List)
            {
                level = RMReportObjectLevel.List;
            }
            else if ((entity.CacheNodeType > (int)CacheNodeType.List) && (entity.CacheNodeType < (int)CacheNodeType.Item))
            {
                level = RMReportObjectLevel.Folder;
            }
            else if (entity.CacheNodeType == (int)CacheNodeType.Item || entity.CacheNodeType == (int)CacheNodeType.ItemVersion)
            {
                level = RMReportObjectLevel.Item;
            }
            else if (entity.CacheNodeType == (int)CacheNodeType.Attachment)
            {
                level = RMReportObjectLevel.Attachment;
            }

            return level;
        }


        private Expression<Func<T, P>> GetExpressionBody<T, P>(ParameterExpression param, string propName)
        {
            PropertyInfo property = typeof(T).GetProperty(propName);
            Expression propertyAccess = param;
            propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
            var body = Expression.Lambda<Func<T, P>>(propertyAccess, param);
            return body;
        }
        public async Task<string> GetAllFilterListAsync()
        {
            Dictionary<int, Dictionary<string, string>> allFilters = new Dictionary<int, Dictionary<string, string>>();
            using (new PerformanceScope("get all filters"))
            {
                //var needFilterColIdx = new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10 };
                var needFilterColIdx = new int[] { 3, 4, 5, 6, 7, 8, 9, 10 };
                var lambda = await GetManualReviewQueryLambdaAsync();
                var accounts = (await AccountDao.FindListAsync(s => s.IsRemoved == 0)).ToDictionary(a => a.Id.ToString(), o => o.DisplayName);
                foreach (var idx in needFilterColIdx)
                {

                    var filterName = mColumnNames[idx];
                    if (idx == 5)//rule
                    {
                        var rules = await RuleDao.GetRulesWithoutRemovedAsync();
                        var nameDic = rules.ToDictionary(item => item.RuleId.ToString(), item => item.RuleName);
                        //var nameDic = new Dictionary<string, string>();
                        //var filters = ManualApproveDao.GetFilterList(s => new { RuleId = s.RuleId, RuleName = s.RuleName }, lambda);
                        //foreach (var f in filters)
                        //{
                        //    nameDic.Add(f.RuleId, f.RuleName);
                        //}
                        allFilters[idx] = nameDic;
                    }
                    else if (idx == 3)
                    {
                        allFilters[idx] = accounts;
                    }
                    else if (idx == 4)
                    {
                        allFilters[idx] = accounts;
                    }
                    else if (idx == 6)//Escalate From
                    {
                        var nameDic = new Dictionary<string, string>();
                        //var filters = ManualApproveDao.GetFilterList(s => s.EscalateFrom, lambda);
                        //if (filters != null)
                        //{
                        //    foreach (var f in filters)
                        //    {
                        //        if (f != null)
                        //        {
                        //            if (!nameDic.ContainsKey(f))
                        //            {
                        //                try
                        //                {
                        //                    var uid = int.Parse(f);
                        //                    nameDic.Add(f, AccountDao.Find(s => s.Id == uid).DisplayName);
                        //                }
                        //                catch (Exception e)
                        //                {
                        //                    mLogger.Warn("get escalate error:{0}, uid: {1}", e.ToString(), f);
                        //                }
                        //            }
                        //        }
                        //    }
                        //}
                        allFilters[idx] = accounts;
                    }
                    else if (idx == 7)//Records Owner
                    {
                        //var nameDic = new Dictionary<string, string>();
                        //var filters = ManualApproveDao.GetOwnersFilterList(s => s.EscalateTo, lambda);
                        allFilters[idx] = accounts;
                    }
                    else if (idx == 8)//ApprovedBy
                    {
                        //var nameDic = new Dictionary<string, string>();
                        //var filters = ManualApproveDao.GetFilterList(s => s.ApprovedBy, lambda);
                        //if (filters != null)
                        //{
                        //    foreach (var f in filters)
                        //    {
                        //        if (f != null)
                        //        {
                        //            if (!nameDic.ContainsKey(f))
                        //            {
                        //                try
                        //                {
                        //                    var uid = int.Parse(f);
                        //                    nameDic.Add(f, AccountDao.Find(s => s.Id == uid).DisplayName);
                        //                }
                        //                catch (Exception e)
                        //                {
                        //                    mLogger.Warn("get escalate error:{0}, uid: {1}", e.ToString(), f);
                        //                }
                        //            }
                        //        }
                        //    }
                        //}
                        allFilters[idx] = accounts;
                    }
                    else if (idx == 9)
                    {
                        bool CheckLicense(SourceFlag sourceFlag)
                        {
                            if (sourceFlag == SourceFlag.FileSystem)
                            {
                                return TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.FileSystem);
                            }
                            else if (sourceFlag == SourceFlag.SharePointOnPrem)
                            {
                                return TenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.RoleAssignments.PaidForModule.SharePointOnPrem);
                            }
                            return true;
                        }

                        //var nameDic = new Dictionary<string, string>();
                        var sources = new List<SourceFlag>
                        {
                            SourceFlag.SharePoint,
                            SourceFlag.Exchange,
                            SourceFlag.Physical,
                            SourceFlag.FileSystem,
                            SourceFlag.OneDrive,
                            SourceFlag.SharePointOnPrem,
                        };
                        sources = sources.Where(item => CheckLicense(item)).ToList();
                        var nameDic = sources.ToDictionary(item => Convert.ToInt32(item).ToString(), item => GetSourceFlagString(Convert.ToInt32(item)));
                        //var filters = ManualApproveDao.GetFilterList(s => s.SourceFlag, lambda);
                        //if (filters != null)
                        //{
                        //    foreach (var f in filters)
                        //    {
                        //        if (!nameDic.ContainsKey(f.ToString()))
                        //        {
                        //            try
                        //            {
                        //                nameDic.Add(f.ToString(), GetSourceFlagString(f));
                        //            }
                        //            catch (Exception e)
                        //            {
                        //                mLogger.Warn("get source error:{0}", e.ToString());
                        //            }
                        //        }
                        //    }
                        //}
                        allFilters[idx] = nameDic;
                    }
                    else
                    {
                        allFilters[idx] = GetFilterList<string>(idx, filterName, lambda);
                    }
                }
            }
           
            return JsonConvert.SerializeObject(allFilters);
        }

        public string GetSourceFlagString(int s)
        {
            switch (s)
            {
                case (int)SourceFlag.SharePoint:
                    return I18NEntity.GetString("RM_JS_Common_ReportType_SharePoint");
                case (int)SourceFlag.Exchange:
                    return I18NEntity.GetString("RM_JS_Common_ReportType_Exchange");
                case (int)SourceFlag.Physical:
                    return I18NEntity.GetString("RM_JS_Common_ReportType_Physical");
                case (int)SourceFlag.FileSystem:
                    return I18NEntity.GetString("RM_JS_SPS_TabLabel_FS");
                case (int)SourceFlag.SharePointOnPrem:
                    return I18NEntity.GetString("RM_JS_SPS_TabLabel_SPLocal");
                case (int)SourceFlag.OneDrive:
                    return I18NEntity.GetString("RM_JS_SPS_TabLabel_OneDrive");
                default:
                    return I18NEntity.GetString("None");
            }
        }

        private async Task<Expression> GetUserAndGroupLambdaAsync(ParameterExpression param)
        {
            Expression userAndgroupExpression = null;
            //var currentUser = LoginService.GetCurrentUserInfo();
            //RMSessionStore.GetLogonUserInfo()
            var accountId = TenantLocalValue.LogonUserId;
            if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(Contract.RoleAssignments.RMPermissionMasks.ManualReviewAdmin)))
            {
                AccountDto user = UserService.GetUserOrGroup(accountId);
                if (user == null)
                {
                    throw new Exception("current user not found.");
                }
                var userAndGroups = await UserService.GetUserGroupsAsync(accountId);
                userAndGroups.Add(user);
                var exps = userAndGroups.Select(g => Expression4DynamicQuery.GetContainsExpression(typeof(RMManualApprove), param, "EscalateTo", "|" + g.Id + "|"));
                userAndgroupExpression = exps.Aggregate(Expression.OrElse);

            }

            return userAndgroupExpression;
        }

        private async Task<Expression> GetUserAndGroupLambdaInJobAsync(ParameterExpression param,string accountId)
        {
            Expression userAndgroupExpression = null;

            if (!(await TenantUtil.RunUnderTenantAsync(new Contract.Tenant.TenantContext(TenantLocalValue.LogonGroupId, accountId, TenantLocalValue.LogonUserEmail),
                        () => {
                            return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.ManualReviewAdmin);
                        })))
            {
                AccountDto user = UserService.GetUserOrGroup(accountId);
                if (user == null)
                {
                    throw new Exception("current user not found.");
                }
                var userAndGroups = await UserService.GetUserGroupsAsync(accountId);
                userAndGroups.Add(user);
                var exps = userAndGroups.Select(g => Expression4DynamicQuery.GetContainsExpression(typeof(RMManualApprove), param, "EscalateTo", "|" + g.Id + "|"));
                userAndgroupExpression = exps.Aggregate(Expression.OrElse);

            }
            return userAndgroupExpression;
        }

        public delegate Dictionary<string, string> FilterPutInDicDelegate<T>(List<T> list);
        private Dictionary<string, string> GetFilterList<T>(int idx, string filterName, Expression<Func<RMManualApprove, bool>> lambda) where T : class
        {
            var nameDic = new Dictionary<string, string>();
            try
            {
                ParameterExpression param = Expression.Parameter(typeof(RMManualApprove), "c");
                var selectLambda = GetExpressionBody<RMManualApprove, T>(param, filterName);
                var list = ManualApproveDao.GetFilterList(selectLambda, lambda);
                foreach (var i in list)
                {
                    if (i != null)
                    {
                        if (!string.IsNullOrEmpty(i.ToString()))
                        {
                            nameDic.Add(i.ToString(), i.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Error("Get filter name Error. filter name:{0}, Message:{1}.", filterName, e.ToString());
            }
            return nameDic;
        }

        public async Task<string> GetReportJobDatasAsync(ManualReviewQuery query)
        {
            var pager = query;

            var globalTimeZoneId = (await GeneralSettingService.GetGeneralSettingAsync()).TimeZoneId;
            TimeZoneInfo localZone = GeneralSettingConfig.FindSystemTimeZoneById(globalTimeZoneId);
            if (query.StartTime.HasValue)
            {
                query.StartTime = TimeZoneInfo.ConvertTimeToUtc(query.StartTime.Value, localZone);
            }
            if (query.EndTime.HasValue)
            {
                query.EndTime = TimeZoneInfo.ConvertTimeToUtc(query.EndTime.Value, localZone);
            }
            if (pager.SortBy == "RecordOwner")
            {
                pager.SortBy = "EscalateTo";
            }
            if (pager.SortBy == "CreatedTime")
            {
                pager.SortBy = "CollectionTime";
            }

            Expression queryExpr = null;
            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(RMManualApprove), "c");
            int totalCount;
            List<Expression> normalStatusExps = new List<Expression>();
            var enableFilterStatus = false;
            var isAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(Contract.RoleAssignments.RMPermissionMasks.ManualReviewAdmin);
            //用OR合并一个Filter选的多个值的表达式
            if (pager.FilterInfos != null)
            {
                foreach (var f in pager.FilterInfos)
                {
                    var stringValues = f.Value.Select(s => (s as object).ToString()).ToList();
                    IEnumerable<Expression> exps = null;
                    if (f.Key == 7)//recordOwner is escalateTo in db
                    {
                        var curExps = new List<Expression>();
                        
                        var escalateToExps = stringValues.Select(c => Expression4DynamicQuery.GetContainsExpression(typeof(RMManualApprove), param, mColumnNames[f.Key], "|" + c + "|"));
                        curExps.AddRange(escalateToExps);

                        //var userIntIds = stringValues.ConvertAll(item => Convert.ToInt32(item));
                        //var userIds = UserService.GetUsersByIds(userIntIds).Select(item => item.UserId).ToList();
                        //var userAndGroupIds = userIds.ConvertAll(item => UserService.GetUserAndGroupUserIds(item)).SelectMany(item => item).ToList();
                        //var siteOwnerWorkflowExps = GetWfInstanceExpressionsBySiteOwners(userAndGroupIds, FilterWorkflowStatus.All, param);
                        //if(siteOwnerWorkflowExps != null)
                        //{
                        //    curExps.Add(siteOwnerWorkflowExps);
                        //}
                        //var workflowIds = RMWorkflowDefinitionDao.GetInstances(userAndGroupIds).Select(item => item.Id).ToList();
                        //var workflowExps = Expression4DynamicQuery.GetInExpression(typeof(RMManualApprove), param, "WorkflowInstanceId", workflowIds);
                        //curExps.Add(workflowExps);

                        exps = curExps;
                    }
                    else
                    {

                        if (f.Key == 1)//status is filter condition
                        {
                            enableFilterStatus = true;
                            var normalApprovalStatus = stringValues.Except(new List<string> { ((int)SOApproveDBStatus.WorkflowInProgress).ToString(), ((int)SOApproveDBStatus.WorkflowComplete).ToString() }).ToList();
                            //if (normalApprovalStatus.Count == stringValues.Count)
                            //{
                            //    //status filter中不含有workflow状态选项
                            //    showWorkflowData = false;
                            //}
                            //else
                            //{
                            //    filterWorkflowStatus = ResetFilterWorkflowStatus(stringValues);
                            //}

                            if (normalApprovalStatus.Count > 0)
                            {
                                //stauts is waiting/approved/rejected
                                normalStatusExps = normalApprovalStatus.Select(c => Expression4DynamicQuery.GetEqualExpression(typeof(RMManualApprove), param, mColumnNames[f.Key], c)).ToList();
                            }
                        }
                        else
                        {
                            exps = stringValues.Select(c => Expression4DynamicQuery.GetEqualExpression(typeof(RMManualApprove), param, mColumnNames[f.Key], c)).ToList();
                        }
                    }

                    if (exps != null)
                    {
                        var filterExpression = exps.Aggregate(Expression.OrElse);
                        allExpressionList.Add(filterExpression);
                    }
                }
            }

            if (!string.IsNullOrEmpty(pager.SearchValue))
            {
                try
                {
                    var exps = pager.SearcheKeys.Select(searchKey => Expression4DynamicQuery.GetContainsExpression(typeof(RMManualApprove), param, searchKey, pager.SearchValue));
                    var searchExpression = exps.Aggregate(Expression.OrElse);
                    allExpressionList.Add(searchExpression);
                }
                catch (Exception ex)
                {
                    mLogger.Warn("{0}", ex.Message.ToString());
                }
            }
            List<RMManualApprove> dbResult = new List<RMManualApprove>();
            ManualApprovalReviewResult responseResult = new ManualApprovalReviewResult();
            Expression userAndgroupExpression = null;
            try
            {
                if (!isAdmin)
                {
                    userAndgroupExpression = await GetUserAndGroupLambdaAsync(param);
                }
                
            }
            catch (Exception e)
            {
                mLogger.Error("get user and groups error:{0}", e.ToString());
                return JsonConvert.SerializeObject(responseResult);
            }

            List<Expression> filterStatusExps = new List<Expression>();

            //暂时不支持Filter workflow Status条件，开启时删除此行
            //filterWorkflowStatus = !isAdmin ? FilterWorkflowStatus.All : FilterWorkflowStatus.None;

            if (enableFilterStatus)
            {
                if (normalStatusExps.Count > 0)
                {
                    var statusCondition = normalStatusExps.Aggregate(Expression.OrElse);
                    if (isAdmin)
                    {
                        filterStatusExps.Add(statusCondition);
                    }
                    else
                    {
                        //endUser
                        //var reviewerCondition = GetReviewCondition(userAndgroupExpression, param);
                        var reviewerAndStatusCondition = new List<Expression> { statusCondition, userAndgroupExpression };
                        filterStatusExps.Add(reviewerAndStatusCondition.Aggregate(Expression.AndAlso));
                    }
                }
            }
            else
            {
                if (!isAdmin)
                {
                    //var reviewerCondition = GetReviewCondition(userAndgroupExpression, param);
                    filterStatusExps.Add(userAndgroupExpression);
                }
            }

            if (filterStatusExps.Count > 0)
            {
                allExpressionList.Add(filterStatusExps.Aggregate(Expression.OrElse));
            }

            if (allExpressionList.Count > 0)
            {
                //将多个Filter和search都用AND合并 目前将EscalateTo条件也添加至此处
                queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
                var lambda = Expression.Lambda<Func<RMManualApprove, bool>>(queryExpr, param);
                Stopwatch timer = new Stopwatch();
                timer.Start();
                dbResult = ManualApproveDao.GetAllData(pager.viewTab, pager.CurrentPage, pager.PageSize, out totalCount, pager.SortBy, pager.isAscending, pager.StartTime, pager.EndTime, lambda);
                timer.Stop();
                mLogger.Info("Get Manual Approve Review Data Take Milliseconds:{0}ms. Lambda is:{1}", timer.ElapsedMilliseconds, lambda.ToString());
            }
            else
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();
                dbResult = ManualApproveDao.GetAllData(pager.viewTab, pager.CurrentPage, pager.PageSize, out totalCount, pager.SortBy, pager.isAscending, pager.StartTime, pager.EndTime);
                timer.Stop();
                mLogger.Info("Get Manual Approve Review Data Take Milliseconds:{0}ms.", timer.ElapsedMilliseconds);
            }

            var wfInstanceIdStatusDic = new Dictionary<Guid, RMWorkflowStatus>();
            var details = new List<ManualApprovalReviewDetails>();
            responseResult.Details = details;
            responseResult.TotalNumber = totalCount;
            if (dbResult != null)
            {
                var instanceIds = dbResult.Where(d => d.WorkflowInstanceId != Guid.Empty).Select(d => d.WorkflowInstanceId).ToList();
                CacheInstanceInfo(wfInstanceIdStatusDic, instanceIds);
                GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
                //var daoRule = RuleService.GetRulesFromDA();
                
                foreach (var r in dbResult)
                {
                    var job = r;
                    var escalateFromUser = I18NEntity.GetString("RM_JS_Common_Pending");
                    var approvedByUser = string.Empty;
                    var recordOwners = string.Empty;
                    if (r.EscalateFrom != null)
                    {
                        try
                        {
                            var uid = int.Parse(r.EscalateFrom);
                            escalateFromUser = AccountDao.Find(s => s.Id == uid).DisplayName;
                        }
                        catch (Exception e)
                        {
                            mLogger.Warn("get escalate error:{0}, uid: {1}", e.ToString(), r.EscalateFrom);
                        }
                    }
                    recordOwners = await GetReviewUserAsync(r);
                    if (r.ApprovedBy != null)
                    {
                        try
                        {
                            var uid = int.Parse(r.ApprovedBy);
                            approvedByUser = AccountDao.Find(s => s.Id == uid).DisplayName;
                        }
                        catch (Exception e)
                        {
                            mLogger.Warn("get approvedBy error:{0}, uid: {1}", e.ToString(), r.ApprovedBy);
                        }
                    }
                    if (r.ContentType == "Item")
                    {
                        r.Url = WebUtil.GetListItemRealPath(r.Url);
                    }

                    var rStatus = r.Status;
                    if (r.WorkflowInstanceId != Guid.Empty)
                    {
                        if (wfInstanceIdStatusDic.Count > 0 && wfInstanceIdStatusDic.ContainsKey(r.WorkflowInstanceId))
                        {
                            switch (wfInstanceIdStatusDic[r.WorkflowInstanceId])
                            {
                                case RMWorkflowStatus.Running:
                                    rStatus = (int)SOApproveDBStatus.WorkflowInProgress;
                                    break;
                                case RMWorkflowStatus.Completed:
                                    rStatus = (int)SOApproveDBStatus.WorkflowComplete;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    details.Add(new ManualApprovalReviewDetails()
                    {
                        Id = r.Id,
                        LeafName = r.LeafName,
                        Type = GetManualItemType(r),
                        Url = r.Url,
                        Status = rStatus,
                        ContentType = r.ContentType,
                        ModifiedBy = r.ModifiedBy,
                        CreatedBy = r.CreatedBy,
                        RuleName = r.RuleName,
                        RuleId = r.RuleId,
                        Criteria = I18NEntity.ReplaceI18NKey(r.Criteria, "RM_JS_", new string[] { ",", " " }),//GetI18NRuleCriteria(r.RuleId, r.Criteria, daoRule, (RMReportObjectLevel)r.ObjectLevel),
                        PartKey = r.PartKey,
                        RowKey = r.RowKey,
                        EscalateFrom = escalateFromUser,
                        RecordOwner = recordOwners,
                        ApprovedBy = approvedByUser,
                        Comments = r.Comment,
                        CreatedTime = GeneralSettingService.ConvertTiksToDateTime(gls, r.CollectionTime, true).SimplifyFormatTime,
                        RelatedRecordsList = r.RelatedRecords != null ? SerializerHelper.DeserializeFromXmlString<List<ReportRelatedRecords>>(r.RelatedRecords) : new List<ReportRelatedRecords>(),
                        RelatedRecordsAction = r.RelatedRecordsAction,
                        SourceFlag = r.SourceFlag,
                        DisposalClass = r.DisposalClass
                    });
                }
            }
            return JsonConvert.SerializeObject(responseResult);    
        }
        public async Task<QueryResult> GetReportJobDatasInJobAsync(ManualReviewQuery query,string accountId,int pageIndex,int pageSize)
        {
            var pager = query;

            var globalTimeZoneId = (await GeneralSettingService.GetGeneralSettingAsync()).TimeZoneId;
            TimeZoneInfo localZone = GeneralSettingConfig.FindSystemTimeZoneById(globalTimeZoneId);
            if (query.StartTime.HasValue)
            {
                query.StartTime = TimeZoneInfo.ConvertTimeToUtc(query.StartTime.Value, localZone);
            }
            if (query.EndTime.HasValue)
            {
                query.EndTime = TimeZoneInfo.ConvertTimeToUtc(query.EndTime.Value, localZone);
            }
            if (pager.SortBy == "RecordOwner")
            {
                pager.SortBy = "EscalateTo";
            }
            if (pager.SortBy == "CreatedTime")
            {
                pager.SortBy = "CollectionTime";
            }

            Expression queryExpr = null;
            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(RMManualApprove), "c");
            int totalCount;
            List<Expression> normalStatusExps = new List<Expression>();
            var enableFilterStatus = false;
            var isAdmin = await TenantUtil.RunUnderTenant(new Contract.Tenant.TenantContext(TenantLocalValue.LogonGroupId, accountId, TenantLocalValue.LogonUserEmail),
                        () => {
                            return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.ManualReviewAdmin);
                        });
            //用OR合并一个Filter选的多个值的表达式
            if (pager.FilterInfos != null)
            {
                foreach (var f in pager.FilterInfos)
                {
                    var stringValues = f.Value.Select(s => (s as object).ToString()).ToList();
                    IEnumerable<Expression> exps = null;
                    if (f.Key == 7)//recordOwner is escalateTo in db
                    {
                        exps = stringValues.Select(c => Expression4DynamicQuery.GetContainsExpression(typeof(RMManualApprove), param, mColumnNames[f.Key], c + "|"));
                    }
                    else
                    {

                        if (f.Key == 1)//status is filter condition
                        {
                            enableFilterStatus = true;
                            var normalApprovalStatus = stringValues.Except(new List<string> { ((int)SOApproveDBStatus.WorkflowInProgress).ToString(), ((int)SOApproveDBStatus.WorkflowComplete).ToString() }).ToList();
                            if (normalApprovalStatus.Count > 0)
                            {
                                //stauts is waiting/approved/rejected
                                normalStatusExps = normalApprovalStatus.Select(c => Expression4DynamicQuery.GetEqualExpression(typeof(RMManualApprove), param, mColumnNames[f.Key], c)).ToList();
                            }
                        }
                        else
                        {
                            exps = stringValues.Select(c => Expression4DynamicQuery.GetEqualExpression(typeof(RMManualApprove), param, mColumnNames[f.Key], c)).ToList();
                        }
                    }

                    if (exps != null)
                    {
                        var filterExpression = exps.Aggregate(Expression.OrElse);
                        allExpressionList.Add(filterExpression);
                    }
                }
            }

            if (!string.IsNullOrEmpty(pager.SearchValue))
            {
                try
                {
                    var exps = pager.SearcheKeys.Select(searchKey => Expression4DynamicQuery.GetContainsExpression(typeof(RMManualApprove), param, searchKey, pager.SearchValue));
                    var searchExpression = exps.Aggregate(Expression.OrElse);
                    allExpressionList.Add(searchExpression);
                }
                catch (Exception ex)
                {
                    mLogger.Warn("{0}", ex.Message.ToString());
                }
            }
            List<RMManualApprove> dbResult = new List<RMManualApprove>();
            QueryResult queryResult = new QueryResult();
            ManualApprovalReviewResult responseResult = new ManualApprovalReviewResult();
            Expression userAndgroupExpression = null;
            try
            {
                userAndgroupExpression = await GetUserAndGroupLambdaInJobAsync(param,accountId);
            }
            catch (Exception e)
            {
                mLogger.Error("get user and groups error:{0}", e.ToString());
                return null;
            }

            List<Expression> filterStatusExps = new List<Expression>();

            //暂时不支持Filter workflow Status条件，开启时删除此行

            if (enableFilterStatus)
            {
                if (normalStatusExps.Count > 0)
                {
                    var statusCondition = normalStatusExps.Aggregate(Expression.OrElse);
                    if (isAdmin)
                    {
                        filterStatusExps.Add(statusCondition);
                    }
                    else
                    {
                        //endUser
                        //var reviewerCondition = GetReviewConditionInJob(userAndgroupExpression, param,accountId);
                        var reviewerAndStatusCondition = new List<Expression> { statusCondition, userAndgroupExpression };
                        filterStatusExps.Add(reviewerAndStatusCondition.Aggregate(Expression.AndAlso));
                    }
                }
            }
            else
            {
                if (!isAdmin)
                {
                    //var reviewerCondition = GetReviewConditionInJob(userAndgroupExpression, param,accountId);
                    filterStatusExps.Add(userAndgroupExpression);
                }
            }

            if (filterStatusExps.Count > 0)
            {
                allExpressionList.Add(filterStatusExps.Aggregate(Expression.OrElse));
            }

            if (allExpressionList.Count > 0)
            {
                using (new PerformanceScope("ApproveOrRejectProcess","GetAllDatasWithExp"))
                {
                    //将多个Filter和search都用AND合并 目前将EscalateTo条件也添加至此处
                    queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
                    var lambda = Expression.Lambda<Func<RMManualApprove, bool>>(queryExpr, param);
                    Stopwatch timer = new Stopwatch();
                    timer.Start();
                    queryResult = ManualApproveDao.GetAllDataInJob(pager.viewTab, out totalCount, pageIndex, pageSize, pager.StartTime, pager.EndTime, lambda);
                    timer.Stop();
                    mLogger.Info("Get Manual Approve Review Data Take Milliseconds:{0}ms. Lambda is:{1}", timer.ElapsedMilliseconds, lambda.ToString());
                }   
            }
            else
            {
                using (new PerformanceScope("ApproveOrRejectProcess", "GetAllDatasWithoutExp"))
                {
                    Stopwatch timer = new Stopwatch();
                    timer.Start();
                    queryResult = ManualApproveDao.GetAllDataInJob(pager.viewTab, out totalCount, pageIndex, pageSize, pager.StartTime, pager.EndTime);
                    timer.Stop();
                    mLogger.Info("Get Manual Approve Review Data Take Milliseconds:{0}ms.", timer.ElapsedMilliseconds);
                }   
            }
            return queryResult;
        }

        public async System.Threading.Tasks.Task UpgradeManualReviewDataForEscalateAsync() 
        {
            var index = 1;
            var pageSize = 100;
            var totalCount = 0;
            var needUpgrage = ManualApproveDao.GetDatasByPager(1, 1, ref totalCount, m => m.WorkflowInstanceId != Guid.Empty && m.EscalateTo == "").Count() > 0;
            if (!needUpgrage) return;
            mLogger.Info($"begin to upgarde manual reivew:{totalCount}.");

            var tempDatas = ManualApproveDao.GetDatasByPager(index, pageSize, ref totalCount);
            await ProcessUpgradeManualItemAsync(tempDatas);
            while (totalCount - index * pageSize > 0)
            {
                index++;
                var manualItems = ManualApproveDao.GetDatasByPager(index, pageSize, ref totalCount);
                await ProcessUpgradeManualItemAsync(manualItems);
            }
            mLogger.Info($"success to upgarde manual reivew items.");
        }
        private async System.Threading.Tasks.Task ProcessUpgradeManualItemAsync(List<RMManualApprove> items)
        {
            foreach (var item in items)
            {
                if (item.WorkflowInstanceId != Guid.Empty && string.IsNullOrEmpty(item.EscalateTo))
                {
                    var userIds = WorkflowInstanceDao.GetReviewUserIdsByManualInfo(item);
                    var uIds = (await AccountDao.GetUserByUserIdsAsync(userIds)).Select(u => u.Id).ToList();
                    var owners = "|" + string.Join("|", uIds) + "|";
                    item.EscalateTo = owners;
                }
                else if (!string.IsNullOrEmpty(item.EscalateTo) && !item.EscalateTo.StartsWith("|")) 
                {
                    item.EscalateTo = "|" + item.EscalateTo;
                }
            }
            ManualApproveDao.BatchUpdate(items);
        }

      

        //private Expression GetReviewCondition(Expression userAndgroupExpression, ParameterExpression param)
        //{
        //    var reviewerCondition = new List<Expression>();
        //    if (userAndgroupExpression != null)
        //    {
        //        reviewerCondition.Add(userAndgroupExpression);
        //    }
        //    reviewerCondition.AddRange(GetWfInstanceExpressions(FilterWorkflowStatus.None, param, true));

        //    return reviewerCondition.Count > 0 ? reviewerCondition.Aggregate(Expression.OrElse) : null;
        //}
        //private Expression GetReviewConditionInJob(Expression userAndgroupExpression, ParameterExpression param,string accountId)
        //{
        //    var reviewerCondition = new List<Expression>();
        //    if (userAndgroupExpression != null)
        //    {
        //        reviewerCondition.Add(userAndgroupExpression);
        //    }
        //    reviewerCondition.AddRange(GetWfInstanceExpressionsInJob(FilterWorkflowStatus.None, param, true,accountId));

        //    return reviewerCondition.Count > 0 ? reviewerCondition.Aggregate(Expression.OrElse) : null;
        //}
        private async Task<string> GetReviewUserAsync(RMManualApprove data)
        {
            string recordOwners = string.Empty;
            try
            {
                //recordOwnerIds包含两部分User:1 workflow reviewer;2 escalateTo中User
                var recordOwnerIds = new List<int>();
                //if (data.WorkflowInstanceId != Guid.Empty)
                //{
                //    var reviewerIds = WorkflowInstanceDao.GetReviewUserIdsByManualInfo(data);
                //    var reviewerAccountIds = AccountDao.GetUserByUserIds(reviewerIds).Select(u => u.Id).ToList();
                //    recordOwnerIds.AddRange(reviewerAccountIds);
                //}
                if (!string.IsNullOrEmpty(data.EscalateTo))
                {
                    var escalateToIds = data.EscalateTo.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var escalateToAccountIds = escalateToIds.ConvertAll(u => { return int.Parse(u); });
                    recordOwnerIds.AddRange(escalateToAccountIds);
                }
                var users = await AccountDao.GetUserByIdsAsync(recordOwnerIds.Distinct().ToList());
                var userNames = users.Select(u => u.DisplayName).ToList();
                recordOwners = string.Join(";", userNames);
            }
            catch (Exception ex)
            {
                mLogger.Error($"get record owner error:{ex.ToString()}");
            }
            return recordOwners;

        }
        //private List<string> GetCurrentUserAndGroupIds()
        //{
        //    List<string> userAndGroupIds = new List<string>();
        //    var currentUser = LoginService.GetCurrentUserInfo(); //RMSessionStore.GetLogonUserInfo()
        //    var userId = currentUser.AccountId;
        //    AccountDto user = UserService.GetUserOrGroup(userId);
        //    if (user == null)
        //    {
        //        throw new Exception("current user not found.");
        //    }
        //    userAndGroupIds.Add(userId);
        //    var userAndGroups = UserService.GetUserGroups(userId);
        //    if (userAndGroups != null && userAndGroups.Count > 0)
        //    {
        //        userAndGroups.ForEach((item) =>
        //        {
        //            userAndGroupIds.Add(item.UserId);
        //        });
        //    }
        //    return userAndGroupIds;
        //}

        //private List<string> GetCurrentUserAndGroupIdsInJob(string accoundId)
        //{
        //    List<string> userAndGroupIds = new List<string>();
        //    var userId = accoundId;
        //    AccountDto user = UserService.GetUserOrGroup(userId);
        //    if (user == null)
        //    {
        //        throw new Exception("current user not found.");
        //    }
        //    userAndGroupIds.Add(userId);
        //    var userAndGroups = UserService.GetUserGroups(userId);
        //    if (userAndGroups != null && userAndGroups.Count > 0)
        //    {
        //        userAndGroups.ForEach((item) =>
        //        {
        //            userAndGroupIds.Add(item.UserId);
        //        });
        //    }
        //    return userAndGroupIds;
        //}

        public void CacheInstanceInfo(Dictionary<Guid, RMWorkflowStatus> wfInstanceIdStatusDic, List<Guid> instanceIds)
        {
            var instances = RMWorkflowDefinitionDao.GetInstances(instanceIds);
            if (instances != null && instances.Count > 0)
            {
                foreach (var item in instances)
                {
                    if (!wfInstanceIdStatusDic.ContainsKey(item.Id))
                    {
                        wfInstanceIdStatusDic.Add(item.Id, item.Status);
                    }
                }
            }
        }

        //private FilterWorkflowStatus ResetFilterWorkflowStatus(List<string> filterValues)
        //{
        //    var status = FilterWorkflowStatus.None;
        //    var uiInProgress = (int)SOApproveDBStatus.WorkflowInProgress;
        //    var uiComplete = (int)SOApproveDBStatus.WorkflowComplete;
        //    if (filterValues.Contains(uiInProgress.ToString()) && filterValues.Contains(uiComplete.ToString()))
        //    {
        //        status = FilterWorkflowStatus.All;
        //    }
        //    else if (filterValues.Contains(uiInProgress.ToString()))
        //    {
        //        status = FilterWorkflowStatus.Inprogress;
        //    }
        //    else if (filterValues.Contains(uiComplete.ToString()))
        //    {
        //        status = FilterWorkflowStatus.Complete;
        //    }
        //    return status;
        //}

        //private Expression GetWfInstanceExpressionsBySiteOwners(List<string> userAndGroupIds, FilterWorkflowStatus status, ParameterExpression param)
        //{
        //    var siteOwnerUsedWorkflowDefinitions = WorkflowSiteOwnersDao.FindList(item => userAndGroupIds.Contains(item.OwnerId));

        //    if (siteOwnerUsedWorkflowDefinitions == null || siteOwnerUsedWorkflowDefinitions.Count == 0)
        //    {
        //        return null;
        //    }

        //    var workflowDefinitionIds = siteOwnerUsedWorkflowDefinitions.Select(item => item.DefinitionId).ToList();
        //    var workflowInstances = RMWorkflowDefinitionDao.GetInstancesByHasSiteOwnersReviewerTypeDefinition(workflowDefinitionIds, userAndGroupIds);

        //    if (workflowInstances == null || workflowInstances.Count == 0)
        //    {
        //        return null;
        //    }

        //    if (status != FilterWorkflowStatus.All)
        //    {
        //        workflowInstances = workflowInstances.Where(item => item.Status == (RMWorkflowStatus)(int)status).ToList();
        //    }

        //    var workflowInstanceIds = workflowInstances.Select(item => item.Id);
        //    //var workflowInstanceExpressions = workflowInstanceIds.Select(c => Expression4DynamicQuery.GetEqualExpression(typeof(RMManualApprove), param, "WorkflowInstanceId", c)).ToList();
        //    var workflowInstanceExpressions = Expression4DynamicQuery.GetInExpression(typeof(RMManualApprove), param, "WorkflowInstanceId", workflowInstanceIds);
        //    //var workflowSiteExpressions = siteOwnerUsedWorkflowDefinitions.Select(item => item.SiteId).Select(c => Expression4DynamicQuery.GetEqualExpression(typeof(RMManualApprove), param, "SiteId", c)).ToList();
        //    var workflowSiteExpressions = Expression4DynamicQuery.GetInExpression(typeof(RMManualApprove), param, "SiteId", siteOwnerUsedWorkflowDefinitions.Select(item => item.SiteId));
        //    //var instanceExpression = workflowInstanceExpressions.Aggregate(Expression.OrElse);
        //    //var siteExpression = workflowSiteExpressions.Aggregate(Expression.OrElse);
        //    return new List<Expression> { workflowInstanceExpressions, workflowSiteExpressions }.Aggregate(Expression.AndAlso);
        //}

        //private List<Expression> GetWfInstanceExpressions(FilterWorkflowStatus status, ParameterExpression param, bool isEndUser)
        //{
        //    List<Expression> instanceExps = new List<Expression>();
        //    try
        //    {
        //        List<RMWorkflowInstance> instances = null;
        //        if (isEndUser)
        //        {
        //            //enduser
        //            var userAndGroupIds = GetCurrentUserAndGroupIds();
        //            instances = RMWorkflowDefinitionDao.GetInstances(userAndGroupIds);
        //            status = status == FilterWorkflowStatus.None ? FilterWorkflowStatus.All : status;
        //            var expression = GetWfInstanceExpressionsBySiteOwners(userAndGroupIds, status, param);
        //            if (expression != null)
        //            {
        //                instanceExps.Add(expression);
        //            }
        //        }
        //        else
        //        {
        //            //RM
        //            if (FilterWorkflowStatus.None != status)
        //            {
        //                var instanceIds = ManualApproveDao.GetAllInstanceIds();
        //                instances = RMWorkflowDefinitionDao.GetInstances(instanceIds);
        //            }
        //        }
        //        instanceExps.AddRange(GetWorkflowInstanceExpression(instances, param, status));
        //    }
        //    catch (Exception e)
        //    {
        //        mLogger.Error("An error occured when generate workflow instance query conditions, message:{0}", e.ToString());
        //    }
        //    return instanceExps;
        //}
        //private List<Expression> GetWfInstanceExpressionsInJob(FilterWorkflowStatus status, ParameterExpression param, bool isEndUser,string accountId)
        //{
        //    List<Expression> instanceExps = new List<Expression>();
        //    try
        //    {
        //        List<RMWorkflowInstance> instances = null;
        //        if (isEndUser)
        //        {
        //            //enduser
        //            var userAndGroupIds = GetCurrentUserAndGroupIdsInJob(accountId);
        //            instances = RMWorkflowDefinitionDao.GetInstances(userAndGroupIds);
        //            status = status == FilterWorkflowStatus.None ? FilterWorkflowStatus.All : status;
        //            var expression = GetWfInstanceExpressionsBySiteOwners(userAndGroupIds, status, param);
        //            if (expression != null)
        //            {
        //                instanceExps.Add(expression);
        //            }
        //        }
        //        else
        //        {
        //            //RM
        //            if (FilterWorkflowStatus.None != status)
        //            {
        //                var instanceIds = ManualApproveDao.GetAllInstanceIds();
        //                instances = RMWorkflowDefinitionDao.GetInstances(instanceIds);
        //            }
        //        }
        //        instanceExps.AddRange(GetWorkflowInstanceExpression(instances, param, status));
        //    }
        //    catch (Exception e)
        //    {
        //        mLogger.Error("An error occured when generate workflow instance query conditions, message:{0}", e.ToString());
        //    }
        //    return instanceExps;
        //}

        //public List<Expression> GetWorkflowInstanceExpression(List<RMWorkflowInstance> instances, ParameterExpression param, FilterWorkflowStatus status)
        //{
        //    List<Expression> instanceExps = new List<Expression>();
        //    if (instances != null && instances.Count > 0)
        //    {
        //        var instanceIds = new List<Guid>();
        //        if (status == FilterWorkflowStatus.None || status == FilterWorkflowStatus.All)
        //        {
        //            instanceIds = instances.Select(s => s.Id).ToList();
        //        }
        //        else
        //        {
        //            instanceIds = instances.Where(s => s.Status == (RMWorkflowStatus)(int)status).Select(s => s.Id).ToList();
        //        }
        //        var workflowInstanceExpressions = Expression4DynamicQuery.GetInExpression(typeof(RMManualApprove), param, "WorkflowInstanceId", instanceIds);
        //        instanceExps.Add(workflowInstanceExpressions);
        //        //instanceExps = instanceIds.Select(c => Expression4DynamicQuery.GetEqualExpression(typeof(RMManualApprove), param, "WorkflowInstanceId", c)).ToList();
        //    }
        //    return instanceExps;
        //}
        public string RunManualApprovalTimerJob(JobRunBy jobRunBy)
        {
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ManualApprovalTimer,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                id = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                mLogger.Error("error occurred while RunManualApprovalJob,ERROR:{0}", ex.ToString());
            }

            return id;
        }

        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ManualApprovalTimer, Action = AuditAction.RunManualApproval, AfterHandler = typeof(ManualApprovalAfterAuditHandler))]
        public string RealRunManualApprovalTimerJob(JobRunBy jobRunBy, string jobRunByUser, string preGenerated = "")
        {
            string jobId = string.Empty;
            JobType jobType = JobType.ManualApprovalTimer;
            //起Job，判断是前台起Job还是Schedule起的Job
            if (jobRunBy == JobRunBy.Control)
            {
                if (string.IsNullOrEmpty(preGenerated))
                {
                    jobId = JobMonitorService.CreateJob(jobType, jobRunByUser);
                }
                else
                {
                    jobId = preGenerated;
                }
                mLogger.Info("Begin control run job {0}", jobId);
            }
            else if (jobRunBy == JobRunBy.Schedule)
            {
                jobId = JobMonitorService.CreateJob(jobType, "RM_TS_RunSchedule");
                mLogger.Info("Begin schedule run Job {0}", jobId);
            }
            else
            {
                jobId = JobMonitorService.CreateJob(jobType, jobRunByUser);
                mLogger.Info("Begin default run Job {0}", jobId);
            }

            //查询当前还没有结束的 Job
            List<string> runningJobs = JobMonitorService.GetRunningJobs(jobType);

            //Job一次只能同时运行一个，所以判断当前起的Job是否要Skip掉
            bool isSkip = runningJobs.Any(j => j != jobId);
            if (!isSkip)
            {
                mJobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = jobId,
                    RunBy = jobRunBy,
                    JobType = jobType,
                    CommandLine = string.Format("{0} {1}", jobType, jobId),
                });
            }
            else
            {
                mLogger.Info(I18NEntity.GetString("RM_MA_Job_Skip")); //RM_SYNC_JobSkip to do
                JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_MA_Job_Skip");
            }

            return jobId;
        }

        public async System.Threading.Tasks.Task MarkApprovalingObjectsToExportedStatusAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, string partionKey, string rowKey, SourceFlag sourceFlag = SourceFlag.SharePoint)
        {
            await ArchiverTableDao.UpdateItemsToExportedStatusAsync(connectionInfo, tenantGroupId, partionKey, new List<string>() { rowKey }, sourceFlag);
        }

        public async System.Threading.Tasks.Task MarkApprovalingObjectsToExportedStatusForFSAsync(string fsAzureTableConnectStr, string tenantGroupId, string partionKey, string rowKey)
        {
            await ArchiverTableDao.UpdateItemsToExportedStatusForFSAsync(fsAzureTableConnectStr, tenantGroupId, partionKey, new List<string>() { rowKey });
        }

        public async System.Threading.Tasks.Task MarkApprovalingObjectsToExportedStatusForSPOnPremAsync(string connectionInfo, string tenantGroupId, string partionKey, string rowKey)
        {
            await ArchiverTableDao.UpdateItemsToExportedStatusForSPOnPremAsync(connectionInfo, tenantGroupId, partionKey, new List<string>() { rowKey });
        }

        public void MarkToExportedStatusForPhysical(Guid physicalItemId)
        {
            ExplorerDao.UpdateItemToExportStatus(physicalItemId);
        }

        public void MarkToExportedStatusForBox(Guid recordId)
        {
            ExplorerDao.UpdateItemToExportStatus(recordId);
        }

        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ManualApprovalTimer, Action = AuditAction.MarkToApproved, AfterHandler = typeof(ManualApprovalAfterAuditHandler), BeforeHandler = typeof(ManualApprovalBeforeAuditHandler))]
        public Task<RAReturnMessage> MarkToApprovedStatusAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, List<int> ids)
        {
            return MarkToApprovedOrRejectedStatusAsync(connectionInfo, tenantGroupId, ids, true);
        }
        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ManualApprovalTimer, Action = AuditAction.MarkToApproved, AfterHandler = typeof(ManualApprovalAfterAuditHandler), BeforeHandler = typeof(ManualApprovalBeforeAuditHandler))]
        public Task<List<List<RAReturnMessage>>> MarkToApprovedStatusInJobAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, List<int> ids, string accountId)
        {
            return MarkToApprovedOrRejectedStatusInJobAsync(connectionInfo, tenantGroupId, ids, true, accountId);
        }
        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ManualApprovalTimer, Action = AuditAction.MarkToRejected, AfterHandler = typeof(ManualApprovalAfterAuditHandler), BeforeHandler = typeof(ManualApprovalBeforeAuditHandler))]
        public Task<RAReturnMessage> MarkToRejectedStatusAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, List<int> ids)
        {
            return MarkToApprovedOrRejectedStatusAsync(connectionInfo, tenantGroupId, ids, false);
        }
        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ManualApprovalTimer, Action = AuditAction.MarkToRejected, AfterHandler = typeof(ManualApprovalAfterAuditHandler), BeforeHandler = typeof(ManualApprovalBeforeAuditHandler))]
        public Task<List<List<RAReturnMessage>>> MarkToRejectedStatusInJobAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, List<int> ids,string accountId)
        {
            return MarkToApprovedOrRejectedStatusInJobAsync(connectionInfo, tenantGroupId, ids, false, accountId);
        }

        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ManualApprovalTimer, Action = AuditAction.MarkToExtend, AfterHandler = typeof(ManualApprovalAfterAuditHandler), BeforeHandler = typeof(ManualApprovalBeforeAuditHandler))]
        public async Task<RAReturnMessage> MarkToExtendStatusAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, List<int> ids, long extendDispositionCustomTime, string extendDispositionComment)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            RAReturnMessage msg = new RAReturnMessage();
            msg.MessageType = RAMessageType.Successful;
            var approvedBy = string.Empty;
            var accountId = TenantLocalValue.LogonUserId;
            var loginUser = AccountDao.Find(s => s.UserId == accountId);
            if (loginUser != null)
            {
                approvedBy = loginUser.Id.ToString();
            }
            List<RMManualApprove> items = await ManualApproveDao.FindListAsync(s => ids.Contains(s.Id));
            if (!ValidateWorkflowItems(items))
            {
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = I18NEntity.GetString("RM_RR_ErrorUseProcessCompletedItem");
                return msg;
            }
            ArgumentCheck.NotNull(loginUser, nameof(loginUser));
            foreach (RMManualApprove item in items)
            {
                //Extend什么都没改，Workflow不需要更新,confirmed with Bruce Yang.
                item.Status = GetApproveStatus(extendDispositionCustomTime);
                item.ApprovedBy = string.Empty;//approvedBy;
                item.Audits = GetAuditXMLString(item, "RM_JS_MA_ApproveStatus_Extend", loginUser.DisplayName);
                item.ExtendDispositionCustomTime = extendDispositionCustomTime;
                item.ExtendDispositionComment = extendDispositionComment;
                if (item.PartKey == null)
                {
                    item.PartKey = string.Empty;
                }
            }
            ManualApproveDao.BatchUpdate(items);
            timer.Stop();
            mLogger.Info("mark count {0} extend status take {1} ms.", ids.Count, timer.ElapsedMilliseconds);
            return msg;
        }

        private async Task<RAReturnMessage> MarkToApprovedOrRejectedStatusAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, List<int> ids, bool isApprove)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            RAReturnMessage msg = new RAReturnMessage();
            msg.MessageType = RAMessageType.Successful;
            var approvedBy = string.Empty;
            var accountId = TenantLocalValue.LogonUserId;
            var loginUser = AccountDao.Find(s => s.UserId == accountId && s.IsRemoved == 0);
            if (loginUser != null)
            {
                approvedBy = loginUser.Id.ToString();
            }
            List<RMManualApprove> items = await ManualApproveDao.FindListAsync(s => ids.Contains(s.Id));
            if (!ValidateWorkflowItems(items))
            {
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = I18NEntity.GetString("RM_RR_ErrorUseProcessCompletedItem");
                return msg;
            }
            var fsAzureTableConnectStr = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
            Dictionary<string, List<RMManualApprove>> pkMappingRowItems = new Dictionary<string, List<RMManualApprove>>();
            ArgumentCheck.NotNull(loginUser, nameof(loginUser));
            foreach (RMManualApprove item in items)
            {
                if (item.WorkflowInstanceId != Guid.Empty)
                {

                    var historyItem = ConvertManualApprovalToHistoryData(item, isApprove, approvedBy);

                    //workflow item logic
                    var request = new DisposalReviewRequestInfo()
                    {
                        RequestId = item.NodeId,
                        InstanceId = item.WorkflowInstanceId,
                        ArchiverTableConnInfo = connectionInfo,
                        TenantGroupId = tenantGroupId,
                        PartionKey = item.PartKey,
                        RowKey = item.RowKey,
                        Source = (SourceFlag)item.SourceFlag,
                        Action = isApprove ? DisposalReviewActionEnum.Approve : DisposalReviewActionEnum.Reject,
                        ActionBy = loginUser.DisplayName,
                        ActionUserId = approvedBy
                    };
                    ExecuteWorkflow(request, item);
                    
                    AddWorkflowManualDataToHistory(historyItem, item.WorkflowInstanceId);
                    continue;
                }
                //update in db
                item.Status = (int)(isApprove ? SOApproveDBStatus.Approved : SOApproveDBStatus.Rejected);
                item.ApprovedBy = approvedBy;
                item.Audits = GetAuditXMLString(item, isApprove ? "RM_JS_MA_ApproveStatus_Approved" : "RM_JS_MA_ApproveStatus_Rejected", loginUser.DisplayName);

                if (item.PartKey == null)
                {
                    item.PartKey = string.Empty;
                }

                //group by partKey
                if (!pkMappingRowItems.ContainsKey(item.PartKey))
                {
                    pkMappingRowItems.Add(item.PartKey, new List<RMManualApprove>() { item });
                }
                else
                {
                    if (pkMappingRowItems[item.PartKey] == null)
                    {
                        pkMappingRowItems[item.PartKey] = new List<RMManualApprove>();
                    }
                    pkMappingRowItems[item.PartKey].Add(item);
                }
            }
            ManualApproveDao.BatchUpdate(items);

            foreach (KeyValuePair<string, List<RMManualApprove>> kv in pkMappingRowItems)
            {
                string partionKey = kv.Key;
                List<RMManualApprove> rowItems = kv.Value;
                foreach (var rowItem in rowItems)
                {
                    try
                    {
                        ArchiverTableEntity item = null;
                        ArchiverExchangeOnlineDto mailItem = null;
                        FileSystemTableEntity fsItem = null;
                        OnPremiseSPTableEntity spLocalItem = null;
                        if(rowItem == null)
                        {
                            continue;
                        }
                        if (rowItem.SourceFlag == (int)SourceFlag.Exchange)
                        {
                            #region EXO
                            using (PerformanceScope scope = new PerformanceScope("MarkToApprovedOrRejectedStatus.GetArchiverItem"))
                            {
                                mailItem = ArchiverTableDao.GetArchiverItemForEXO(connectionInfo, tenantGroupId, partionKey, rowItem.RowKey);
                            }
                            if (mailItem != null)
                            {
                                await ArchiverTableDao.UpdateItemStatusForEXOAsync(connectionInfo, tenantGroupId, partionKey, rowItem.RowKey, isApprove);
                            }
                            else
                            {
                                msg.MessageType = RAMessageType.Failed;
                                msg.ErrorMessage = I18NEntity.GetString("RM_JS_MA_ItemDisposal");
                                //get item from static table
                                var dbItem = ManualApproveDao.Find(s => s.PartKey == partionKey && s.RowKey == rowItem.RowKey);
                                var destoryItem = GetDestoryItemForEXO(connectionInfo, tenantGroupId, partionKey, rowItem.RowKey);
                                if (destoryItem != null)
                                {
                                    if (destoryItem.Status == SOApproveDBStatus.Archived || destoryItem.Status == SOApproveDBStatus.Rejected)
                                    {
                                        dbItem.ActionStatus = (int)Contract.Schedule.ActionStatus.Archiverd;
                                        dbItem.ActionTime = destoryItem.ArchivedTime;
                                        dbItem.Status = items.Where(s => s.Id == dbItem.Id).Select(s => s.Status).First();
                                        ManualApproveDao.SaveManualApprove(dbItem);
                                    }
                                }
                            }
                            #endregion
                        }
                        else if (rowItem.SourceFlag == (int)SourceFlag.Physical)
                        {
                            #region Physical
                            var physicalItem = GetPhysicalRecord(rowItem.NodeId);
                            //item has been destoryed
                            if (physicalItem.Status == SOApproveDBStatus.Archived //Approved
                                || (physicalItem.Status == SOApproveDBStatus.WaitingApprove && !physicalItem.ExportToRECO) //Rejected
                                || physicalItem.RecordStatus == RMRecordStatus.Missing
                                || physicalItem.RecordStatus == RMRecordStatus.RMDeleted
                                || physicalItem.RecordStatus == RMRecordStatus.Destroyed)
                            {
                                msg.MessageType = RAMessageType.Failed;
                                msg.ErrorMessage = I18NEntity.GetString("RM_JS_MA_ItemDisposalForPhysical");

                                var dbItem = ManualApproveDao.Find(s => s.Id == rowItem.Id);
                                dbItem.ActionStatus = (int)Contract.Schedule.ActionStatus.Archiverd;
                                dbItem.ActionTime = physicalItem.ArchivedTime;
                                dbItem.Status = items.Where(s => s.Id == dbItem.Id).Select(s => s.Status).First();
                                ManualApproveDao.SaveManualApproveForPhysical(dbItem);
                            }
                            //update physical item in cosmos db
                            else
                            {
                                ExplorerDao.UpdateApproveStatus(rowItem.NodeId, isApprove ? SOApproveDBStatus.Approved : SOApproveDBStatus.Rejected);
                            }
                            #endregion
                        }
                        else if (rowItem.SourceFlag == (int)SourceFlag.FileSystem)
                        {
                            #region FS
                            using (PerformanceScope scope = new PerformanceScope("MarkToApprovedOrRejectedStatus.GetArchiverItem"))
                            {
                                fsItem = ArchiverTableDao.GetArchiverItemForFS(fsAzureTableConnectStr, tenantGroupId, partionKey, rowItem.RowKey);
                            }
                            if (fsItem != null)
                            {
                                using (PerformanceScope scope = new PerformanceScope("MarkToApprovedOrRejectedStatus.UpdateItemsToApprovedStatus"))
                                {
                                    await ArchiverTableDao.UpdateItemStatusForFSAsync(fsAzureTableConnectStr, tenantGroupId, partionKey, fsItem.RowKey, isApprove);
                                }
                            }
                            else
                            {
                                msg.MessageType = RAMessageType.Failed;
                                msg.ErrorMessage = I18NEntity.GetString("RM_JS_MA_ItemDisposal");
                                //get item from static table
                                var dbItem = ManualApproveDao.Find(s => s.PartKey == partionKey && s.RowKey == rowItem.RowKey);
                                var destoryItem = GetDestoryItemForFS(fsAzureTableConnectStr, tenantGroupId, partionKey, fsItem?.RowKey);
                                if (destoryItem != null)
                                {
                                    if (destoryItem.Status == SOApproveDBStatus.Archived || destoryItem.Status == SOApproveDBStatus.Rejected)
                                    {
                                        dbItem.ActionStatus = (int)Contract.Schedule.ActionStatus.Archiverd;
                                        dbItem.ActionTime = destoryItem.ArchivedTime;
                                        dbItem.Status = items.Where(s => s.Id == dbItem.Id).Select(s => s.Status).First();
                                        ManualApproveDao.SaveManualApproveForFS(dbItem);
                                    }
                                }
                            }
                            #endregion
                        }
                        else if (rowItem.SourceFlag == (int)SourceFlag.SharePointOnPrem)
                        {
                            #region SharePointOnPrem
                            using (PerformanceScope scope = new PerformanceScope("MarkToApprovedOrRejectedStatus.GetArchiverItem"))
                            {
                                spLocalItem = ArchiverTableDao.GetArchiverItemForSPOnPrem(fsAzureTableConnectStr, tenantGroupId, partionKey, rowItem.RowKey);
                            }
                            if (spLocalItem != null)
                            {
                                using (PerformanceScope scope = new PerformanceScope("MarkToApprovedOrRejectedStatus.UpdateItemsToApprovedStatus"))
                                {
                                    await ArchiverTableDao.UpdateItemStatusForSPOnPremAsync(fsAzureTableConnectStr, tenantGroupId, partionKey, spLocalItem.NodeID, isApprove);
                                }
                            }
                            else
                            {
                                msg.MessageType = RAMessageType.Failed;
                                msg.ErrorMessage = I18NEntity.GetString("RM_JS_MA_ItemDisposal");
                                //get item from static table
                                var dbItem = ManualApproveDao.Find(s => s.PartKey == partionKey && s.RowKey == rowItem.RowKey);
                                var destoryItem = GetDestoryItemForSPOnPrem(fsAzureTableConnectStr, tenantGroupId, dbItem.SiteId.ToString(), dbItem.NodeId, dbItem.Version);
                                if (destoryItem != null)
                                {
                                    if (destoryItem.Status == SOApproveDBStatus.Archived || destoryItem.Status == SOApproveDBStatus.Rejected)
                                    {
                                        dbItem.ActionStatus = (int)Contract.Schedule.ActionStatus.Archiverd;
                                        var aspd = JsonConvert.DeserializeObject<OnPremiseArchiverSharePointDto>(destoryItem.JsonMeta);
                                        dbItem.ActionTime = aspd.ArchivedTime.Ticks;
                                        dbItem.Status = items.Where(s => s.Id == dbItem.Id).Select(s => s.Status).First();
                                        ManualApproveDao.SaveManualApprove(dbItem);
                                    }
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            #region SPO & OneDrive
                            using (PerformanceScope scope = new PerformanceScope("MarkToApprovedOrRejectedStatus.GetArchiverItem"))
                            {
                                item = ArchiverTableDao.GetArchiverItem(connectionInfo, tenantGroupId, partionKey, rowItem.RowKey);
                            }
                            if (item != null)
                            {
                                using (PerformanceScope scope = new PerformanceScope("MarkToApprovedOrRejectedStatus.UpdateItemsToApprovedStatus"))
                                {
                                    await ArchiverTableDao.UpdateItemStatusAsync(connectionInfo, tenantGroupId, partionKey, item.NodeID, isApprove);
                                }
                            }
                            else
                            {
                                msg.MessageType = RAMessageType.Failed;
                                msg.ErrorMessage = I18NEntity.GetString("RM_JS_MA_ItemDisposal");
                                //get item from static table
                                var dbItem = ManualApproveDao.Find(s => s.PartKey == partionKey && s.RowKey == rowItem.RowKey);
                                var destoryItem = GetDestoryItem(connectionInfo, tenantGroupId, dbItem.SiteId.ToString(), dbItem.NodeId, dbItem.Version);
                                if (destoryItem != null)
                                {
                                    if (destoryItem.Status == SOApproveDBStatus.Archived || destoryItem.Status == SOApproveDBStatus.Rejected)
                                    {
                                        dbItem.ActionStatus = (int)Contract.Schedule.ActionStatus.Archiverd;
                                        var aspd = JsonConvert.DeserializeObject<ArchiverSharePointDto>(destoryItem.JsonMeta);
                                        dbItem.ActionTime = aspd.ArchivedTime.Ticks;
                                        dbItem.Status = items.Where(s => s.Id == dbItem.Id).Select(s => s.Status).First();
                                        ManualApproveDao.SaveManualApprove(dbItem);
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                    catch (Exception e)
                    {
                        mLogger.Warn("approve item in archiver table error:{0}", e.ToString());
                    }
                }
            }
            timer.Stop();
            mLogger.Info("mark count {0} to {2} status take {1} ms", ids.Count, timer.ElapsedMilliseconds, isApprove ? "approved" : "reject"); //approve or reject
            return msg;
        }

        private async Task<List<List<RAReturnMessage>>> MarkToApprovedOrRejectedStatusInJobAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, List<int> ids, bool isApprove, string accountId)
        {
            List<List<RAReturnMessage>> resultList = new List<List<RAReturnMessage>>();
            List<RAReturnMessage> successItem = new List<RAReturnMessage>();
            List<RAReturnMessage> failedItem = new List<RAReturnMessage>();
            Stopwatch timer = new Stopwatch();
            RAReturnMessage msg = new RAReturnMessage();
            timer.Start();
            var approvedBy = string.Empty;
            var loginUser = AccountDao.Find(s => s.UserId == accountId && s.IsRemoved == 0);
            if (loginUser != null)
            {
                approvedBy = loginUser.Id.ToString();
            }
            List<RMManualApprove> items = await ManualApproveDao.FindListAsync(s => ids.Contains(s.Id));
            using(new PerformanceScope("ApproveOrRejectProcess", "ValidateWorkflowItems"))
            {
                if (!ValidateWorkflowItems(items))
                {
                    RAReturnMessage Nmsg = new RAReturnMessage();
                    Nmsg.MessageType = RAMessageType.Failed;
                    Nmsg.ErrorMessage = I18NEntity.GetString("RM_RR_ErrorUseProcessCompletedItem");
                    failedItem.Add(Nmsg);
                    resultList.Add(failedItem);
                    resultList.Add(successItem);
                    return resultList;
                }
            }
            var fsAzureTableConnectStr = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
            Dictionary<string, List<RMManualApprove>> pkMappingRowItems = new Dictionary<string, List<RMManualApprove>>();
            using (new PerformanceScope("ApproveOrRejectProcess","ProceessItems"))
            {
                ArgumentCheck.NotNull(loginUser, nameof(loginUser));
                foreach (RMManualApprove item in items)
                {
                    if (item.WorkflowInstanceId != Guid.Empty)
                    {
                        try
                        {
                            var historyItem = ConvertManualApprovalToHistoryData(item, isApprove, approvedBy);

                            //workflow item logic
                            var request = new DisposalReviewRequestInfo()
                            {
                                RequestId = item.NodeId,
                                InstanceId = item.WorkflowInstanceId,
                                ArchiverTableConnInfo = connectionInfo,
                                TenantGroupId = tenantGroupId,
                                PartionKey = item.PartKey,
                                RowKey = item.RowKey,
                                Source = (SourceFlag)item.SourceFlag,
                                Action = isApprove ? DisposalReviewActionEnum.Approve : DisposalReviewActionEnum.Reject,
                                ActionBy = loginUser.DisplayName,
                                ActionUserId = approvedBy
                            };
                            ExecuteWorkflow(request, item);

                            AddWorkflowManualDataToHistory(historyItem, item.WorkflowInstanceId);

                        } catch (Exception e)
                        {
                            RAReturnMessage failedMesg = new RAReturnMessage();
                            failedMesg.Extsion1 = item;
                            failedMesg.ErrorMessage = e.Message;
                            failedItem.Add(failedMesg);
                            continue;
                        }
                    }
                    item.Status = (int)(isApprove ? SOApproveDBStatus.Approved : SOApproveDBStatus.Rejected);
                    item.ApprovedBy = approvedBy;
                    item.Audits = GetAuditXMLString(item, isApprove ? "RM_JS_MA_ApproveStatus_Approved" : "RM_JS_MA_ApproveStatus_Rejected", loginUser.DisplayName);
                    using (new PerformanceScope("ApproveOrRejectProcess", "UpdateItems"))
                    {
                        try
                        {
                            if (item.SourceFlag == (int)SourceFlag.Exchange)
                            {
                                await UpdateInEXOAsync(connectionInfo, tenantGroupId, item, isApprove, items);
                            }
                            else if (item.SourceFlag == (int)SourceFlag.Physical)
                            {
                                UpdateInPhysical(item, isApprove, items);
                            }
                            else if (item.SourceFlag == (int)SourceFlag.FileSystem)
                            {
                                await UpdateInFSAsync(fsAzureTableConnectStr, tenantGroupId, item, isApprove, items);
                            }
                            else if (item.SourceFlag == (int)SourceFlag.SharePointOnPrem)
                            {
                                await UpdateInSPOnPremAsync(fsAzureTableConnectStr, tenantGroupId, item, isApprove, items);
                            }
                            else
                            {
                                await UpdateInSPOAndODAsync(connectionInfo, tenantGroupId, item, isApprove, items);
                            }
                        }
                        catch (Exception e)
                        {
                            mLogger.Warn("approve item in archiver table error:{0}", e.ToString());
                            throw e;
                        }
                    }  
                    //update in db
                    RAReturnMessage successMesg = new RAReturnMessage();
                    successMesg.Extsion1 = item;
                    successItem.Add(successMesg);
                    timer.Stop();
                    mLogger.Info("mark count {0} to {2} status take {1} ms", ids.Count, timer.ElapsedMilliseconds, isApprove ? "approved" : "reject"); //approve or reject
                }
            }
            ManualApproveDao.BatchUpdate(items);
            resultList.Add(failedItem);
            resultList.Add(successItem);
            return resultList;
        }
        private async System.Threading.Tasks.Task UpdateInEXOAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, RMManualApprove item, bool isApprove, List<RMManualApprove> items)
        {
            ArchiverExchangeOnlineDto mailItem = null;
            using (PerformanceScope scope = new PerformanceScope("MarkToApprovedOrRejectedStatus.GetArchiverItem"))
            {
                mailItem = ArchiverTableDao.GetArchiverItemForEXO(connectionInfo, tenantGroupId, item.PartKey, item.RowKey);
            }
            if (mailItem != null)
            {
                await ArchiverTableDao.UpdateItemStatusForEXOAsync(connectionInfo, tenantGroupId, item.PartKey, item.RowKey, isApprove);
            }
            else
            {
                mLogger.Error("An error occurred,error message is {0}", I18NEntity.GetString("RM_JS_MA_ItemDisposal"));
                //get item from static table
                var dbItem = ManualApproveDao.Find(s => s.PartKey == item.PartKey && s.RowKey == item.RowKey);
                var destoryItem = GetDestoryItemForEXO(connectionInfo, tenantGroupId, item.PartKey, item.RowKey);
                if (destoryItem != null)
                {
                    if (destoryItem.Status == SOApproveDBStatus.Archived || destoryItem.Status == SOApproveDBStatus.Rejected)
                    {
                        dbItem.ActionStatus = (int)Contract.Schedule.ActionStatus.Archiverd;
                        dbItem.ActionTime = destoryItem.ArchivedTime;
                        dbItem.Status = items.Where(s => s.Id == dbItem.Id).Select(s => s.Status).First();
                        ManualApproveDao.SaveManualApprove(dbItem);
                    }
                }
            }
        }
        private void UpdateInPhysical(RMManualApprove item,bool isApprove, List<RMManualApprove> items)
        {
            var physicalItem = GetPhysicalRecord(item.NodeId);
            //item has been destoryed
            if (physicalItem.Status == SOApproveDBStatus.Archived //Approved
                || (physicalItem.Status == SOApproveDBStatus.WaitingApprove && !physicalItem.ExportToRECO) //Rejected
                || physicalItem.RecordStatus == RMRecordStatus.Missing
                || physicalItem.RecordStatus == RMRecordStatus.RMDeleted
                || physicalItem.RecordStatus == RMRecordStatus.Destroyed)
            {
                mLogger.Error("An error occurred,error message is {0}", I18NEntity.GetString("RM_JS_MA_ItemDisposalForPhysical"));
                var dbItem = ManualApproveDao.Find(s => s.Id == item.Id);
                dbItem.ActionStatus = (int)Contract.Schedule.ActionStatus.Archiverd;
                dbItem.ActionTime = physicalItem.ArchivedTime;
                dbItem.Status = items.Where(s => s.Id == dbItem.Id).Select(s => s.Status).First();
                ManualApproveDao.SaveManualApproveForPhysical(dbItem);
            }
            //update physical item in cosmos db
            else
            {
                ExplorerDao.UpdateApproveStatus(item.NodeId, isApprove ? SOApproveDBStatus.Approved : SOApproveDBStatus.Rejected);
            }
        }
        private async System.Threading.Tasks.Task UpdateInFSAsync(string fsAzureTableConnectStr, string tenantGroupId, RMManualApprove item, bool isApprove, List<RMManualApprove> items)
        {
            FileSystemTableEntity fsItem = null;
            using (PerformanceScope scope = new PerformanceScope("MarkToApprovedOrRejectedStatus.GetArchiverItem"))
            {
                fsItem = ArchiverTableDao.GetArchiverItemForFS(fsAzureTableConnectStr, tenantGroupId, item.PartKey, item.RowKey);
            }
            if (fsItem != null)
            {
                using (PerformanceScope scope = new PerformanceScope("MarkToApprovedOrRejectedStatus.UpdateItemsToApprovedStatus"))
                {
                    await ArchiverTableDao.UpdateItemStatusForFSAsync(fsAzureTableConnectStr, tenantGroupId, item.PartKey, fsItem.RowKey, isApprove);
                }
            }
            else
            {
                mLogger.Error("An error occurred,error message is {0}", I18NEntity.GetString("RM_JS_MA_ItemDisposal"));
                //get item from static table
                var dbItem = ManualApproveDao.Find(s => s.PartKey == item.PartKey && s.RowKey == item.RowKey);
                ArgumentCheck.NotNull(fsItem,nameof(fsItem));
                var destoryItem = GetDestoryItemForFS(fsAzureTableConnectStr, tenantGroupId, item.PartKey, fsItem.RowKey);
                if (destoryItem != null)
                {
                    if (destoryItem.Status == SOApproveDBStatus.Archived || destoryItem.Status == SOApproveDBStatus.Rejected)
                    {
                        dbItem.ActionStatus = (int)Contract.Schedule.ActionStatus.Archiverd;
                        dbItem.ActionTime = destoryItem.ArchivedTime;
                        dbItem.Status = items.Where(s => s.Id == dbItem.Id).Select(s => s.Status).First();
                        ManualApproveDao.SaveManualApproveForFS(dbItem);
                    }
                }
            }
        }
        private async System.Threading.Tasks.Task UpdateInSPOnPremAsync(string fsAzureTableConnectStr,string tenantGroupId, RMManualApprove item, bool isApprove, List<RMManualApprove> items) 
        {
            OnPremiseSPTableEntity spLocalItem = null;
            using (PerformanceScope scope = new PerformanceScope("MarkToApprovedOrRejectedStatus.GetArchiverItem"))
            {
                spLocalItem = ArchiverTableDao.GetArchiverItemForSPOnPrem(fsAzureTableConnectStr, tenantGroupId, item.PartKey, item.RowKey);
            }
            if (spLocalItem != null)
            {
                using (PerformanceScope scope = new PerformanceScope("MarkToApprovedOrRejectedStatus.UpdateItemsToApprovedStatus"))
                {
                    await ArchiverTableDao.UpdateItemStatusForSPOnPremAsync(fsAzureTableConnectStr, tenantGroupId, item.PartKey, spLocalItem.NodeID, isApprove);
                }
            }
            else
            {
                mLogger.Error("An error occurred,error message is {0}", I18NEntity.GetString("RM_JS_MA_ItemDisposal"));
                //get item from static table
                var dbItem = ManualApproveDao.Find(s => s.PartKey == item.PartKey && s.RowKey == item.RowKey);
                var destoryItem = GetDestoryItemForSPOnPrem(fsAzureTableConnectStr, tenantGroupId, dbItem.SiteId.ToString(), dbItem.NodeId, dbItem.Version);
                if (destoryItem != null)
                {
                    if (destoryItem.Status == SOApproveDBStatus.Archived || destoryItem.Status == SOApproveDBStatus.Rejected)
                    {
                        dbItem.ActionStatus = (int)Contract.Schedule.ActionStatus.Archiverd;
                        var aspd = JsonConvert.DeserializeObject<OnPremiseArchiverSharePointDto>(destoryItem.JsonMeta);
                        dbItem.ActionTime = aspd.ArchivedTime.Ticks;
                        dbItem.Status = items.Where(s => s.Id == dbItem.Id).Select(s => s.Status).First();
                        ManualApproveDao.SaveManualApprove(dbItem);
                    }
                }
            }
        }
        private async System.Threading.Tasks.Task UpdateInSPOAndODAsync(AzureTableConnectContract connectionInfo,string tenantGroupId, RMManualApprove item, bool isApprove, List<RMManualApprove> items)
        {
            ArchiverTableEntity Archiveritem = null;
            using (PerformanceScope scope = new PerformanceScope("MarkToApprovedOrRejectedStatus.GetArchiverItem"))
            {
                Archiveritem = ArchiverTableDao.GetArchiverItem(connectionInfo, tenantGroupId, item.PartKey, item.RowKey);
            }
            if (Archiveritem != null)
            {
                using (PerformanceScope scope = new PerformanceScope("MarkToApprovedOrRejectedStatus.UpdateItemsToApprovedStatus"))
                {
                    await ArchiverTableDao.UpdateItemStatusAsync(connectionInfo, tenantGroupId, item.PartKey, Archiveritem.NodeID, isApprove);
                }
            }
            else
            {
                mLogger.Error("An error occurred,error message is {0}", I18NEntity.GetString("RM_JS_MA_ItemDisposal"));
                //get item from static table
                var dbItem = ManualApproveDao.Find(s => s.PartKey == item.PartKey && s.RowKey == item.RowKey);
                var destoryItem = GetDestoryItem(connectionInfo, tenantGroupId, dbItem.SiteId.ToString(), dbItem.NodeId, dbItem.Version);
                if (destoryItem != null)
                {
                    if (destoryItem.Status == SOApproveDBStatus.Archived || destoryItem.Status == SOApproveDBStatus.Rejected)
                    {
                        dbItem.ActionStatus = (int)Contract.Schedule.ActionStatus.Archiverd;
                        var aspd = JsonConvert.DeserializeObject<ArchiverSharePointDto>(destoryItem.JsonMeta);
                        dbItem.ActionTime = aspd.ArchivedTime.Ticks;
                        dbItem.Status = items.Where(s => s.Id == dbItem.Id).Select(s => s.Status).First();
                        ManualApproveDao.SaveManualApprove(dbItem);
                    }
                }
            }
        }
        private RMManualApprove ConvertManualApprovalToHistoryData(RMManualApprove manualApprove, bool isApprove, string approvedBy)
        {
            var ownerIds = ManualApproveDao.GetManualApproveOwnerIds(manualApprove);
            var escalateTo = "|" + string.Join("|", ownerIds) + "|";
            return new RMManualApprove
            {
                ObjectLevel = manualApprove.ObjectLevel,
                SourceFlag = manualApprove.SourceFlag,
                LeafName = manualApprove.LeafName,
                Url = manualApprove.Url,
                Status = (int)(isApprove ? SOApproveDBStatus.Approved : SOApproveDBStatus.Rejected),
                ArchiveLevel = manualApprove.ArchiveLevel,
                Version = manualApprove.Version,
                ContentType = manualApprove.ContentType,
                ModifiedBy = manualApprove.ModifiedBy,
                CreatedBy = manualApprove.CreatedBy,
                ApprovedBy = approvedBy,
                RuleName = manualApprove.RuleName,
                RuleId = manualApprove.RuleId,
                Criteria = manualApprove.Criteria,
                PartKey = manualApprove.PartKey,
                RowKey = manualApprove.RowKey,
                ActionStatus = (int)Contract.Schedule.ActionStatus.Archiverd,
                CollectionTime = manualApprove.CollectionTime,
                ActionTime = manualApprove.ActionTime,
                SiteId = manualApprove.SiteId,
                NodeId = manualApprove.NodeId,
                EscalateFrom = manualApprove.EscalateFrom,
                EscalateTo = escalateTo,
                Comment = manualApprove.Comment,
                Audits = manualApprove.Audits,
                RelatedRecords = manualApprove.RelatedRecords,
                RelatedRecordsAction = manualApprove.RelatedRecordsAction,
                IsRelatedRecords = manualApprove.IsRelatedRecords,
                WorkflowInstanceId = Guid.Empty,
                DisposalClass = manualApprove.DisposalClass,
                ExtendDispositionCustomTime = manualApprove.ExtendDispositionCustomTime,
                ExtendDispositionComment = manualApprove.ExtendDispositionComment,
            };
        }

        private void AddWorkflowManualDataToHistory(RMManualApprove historyManualApproval, Guid workflowInstanceId)
        {
            try
            {
                var instance = RMWorkflowDefinitionDao.GetInstances(new List<Guid> { workflowInstanceId }).First();
                if(instance.Status == RMWorkflowStatus.Completed)
                {
                    mLogger.Info($"The workflow: [{workflowInstanceId}] is completed. Need run manual job add to history");
                    return;
                }

                ManualApproveDao.Create(historyManualApproval);
            }
            catch(Exception e)
            {
                mLogger.Error($"An error occurred while add manual: [{historyManualApproval.Id}] to history failed. Error: {e}");
            }
        }

        private int GetApproveStatus(long extendDispositionCustomTime)
        {
            int status = 0;

            if (extendDispositionCustomTime != 0)
            {
                status = (int)SOApproveDBStatus.WaitingApprove;
            }
            else
            {
                status = (int)SOApproveDBStatus.Rejected;
            }
            return status;
        }


        private bool ValidateWorkflowItems(List<RMManualApprove> items)
        {
            var result = true;
            var instanceIds = items.Where(o => o.WorkflowInstanceId != Guid.Empty).Select(o => o.WorkflowInstanceId).ToList();
            if (instanceIds.Count > 0)
            {
                var instances = RMWorkflowDefinitionDao.GetInstances(instanceIds);
                if (instances.Any(o => o.Status == RMWorkflowStatus.Completed))
                {
                    result = false;//workflow状态是完成的不允许再approve/reject
                }
            }
            return result;
        }

        public async System.Threading.Tasks.Task AddWorkflowHistoryAsync(DisposalReviewRequestInfo req)
        {
            var item = ManualApproveDao.Find(s => s.WorkflowInstanceId == req.InstanceId);
            var actionString = "";
            switch (req.Action)
            {
                case DisposalReviewActionEnum.Approve:
                    actionString = "RM_JS_MA_ApproveStatus_Approved";
                    break;
                case DisposalReviewActionEnum.Reject:
                    actionString = "RM_JS_MA_ApproveStatus_Rejected";
                    break;
                default:
                    break;
            }
            item.Audits = GetAuditXMLString(item, actionString, req.ActionBy);
            item.ApprovedBy = req.ActionUserId;
            await ManualApproveDao.UpdateAsync(item);
        }

        private void ExecuteWorkflow(DisposalReviewRequestInfo request, RMManualApprove item)
        {

            try
            {
                var instance = RMWorkflowDefinitionDao.GetWorkflowInstanceAsync(item.WorkflowInstanceId);
                if (instance != null)
                {
                    throw new Exception("obsoleted");
                    //var xaml = GetWorkflowXamlInfo(instance.DefinitionId);
                    //DisposalReviewWFService.Resume(request, xaml, instance.CurStepName);
                    //mLogger.Info($"Success to resume workflow, NodeId:{item.NodeId}.");
                }
            }
            catch (Exception ex)
            {
                mLogger.Error($"An error occured when resume workflow, NodeId:{item.NodeId}, message:{ex.ToString()}");
            }
        }

        //private string GetWorkflowXamlInfo(Guid workflowId)
        //{
        //    var workflowXaml = "";
        //    if (!workflowXamlDic.ContainsKey(workflowId))
        //    {
        //        var workflowDto = ManualProcessManagementService.LoadProcess(workflowId);
        //        workflowXaml = XamlBuilder.BuildXaml(workflowDto);
        //        workflowXamlDic[workflowId] = workflowXaml;
        //    }
        //    else
        //    {
        //        workflowXaml = workflowXamlDic[workflowId];
        //    }
        //    return workflowXaml;
        //}

        /// <summary>
        /// Workflow最后一步修改数据在Archiver Table或者CosmosDB中的状态
        /// </summary>
        /// <param name="req"></param>
        private async System.Threading.Tasks.Task UpdateArchiverTableManualItemStatusAsync(DisposalReviewRequestInfo req)
        {
            var isApprove = req.Action == DisposalReviewActionEnum.Approve;
            var connectionInfo = req.ArchiverTableConnInfo;
            var tenantGroupId = req.TenantGroupId;
            var partionKey = req.PartionKey;
            var rowKey = req.RowKey;
            var nodeId = req.RequestId;
            var fsAzureTableConnectStr = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
            switch (req.Source)
            {
                case SourceFlag.SharePoint:
                case SourceFlag.OneDrive:
                    var item = ArchiverTableDao.GetArchiverItem(connectionInfo, tenantGroupId, partionKey, rowKey);
                    if (item != null)
                    {
                        await ArchiverTableDao.UpdateItemStatusAsync(connectionInfo, tenantGroupId, partionKey, nodeId, isApprove);
                    }
                    break;
                case SourceFlag.SharePointOnPrem:
                    var spLocalitem = ArchiverTableDao.GetArchiverItemForSPOnPrem(fsAzureTableConnectStr, tenantGroupId, partionKey, rowKey);
                    if (spLocalitem != null)
                    {
                        await ArchiverTableDao.UpdateItemStatusForSPOnPremAsync(fsAzureTableConnectStr, tenantGroupId, partionKey, nodeId, isApprove);
                    }
                    break;
                case SourceFlag.Exchange:
                    var mailItem = ArchiverTableDao.GetArchiverItemForEXO(connectionInfo, tenantGroupId, partionKey, rowKey);
                    if (mailItem != null)
                    {
                        await ArchiverTableDao.UpdateItemStatusForEXOAsync(connectionInfo, tenantGroupId, partionKey, rowKey, isApprove);
                    }
                    break;
                case SourceFlag.Physical:
                    UpdatePhyManualItemCosmosDBStatus(nodeId, isApprove);
                    break;
                case SourceFlag.FileSystem:
                    var fsItem = ArchiverTableDao.GetArchiverItemForFS(fsAzureTableConnectStr, tenantGroupId, partionKey, rowKey);
                    if (fsItem != null)
                    {
                        await ArchiverTableDao.UpdateItemStatusForFSAsync(fsAzureTableConnectStr, tenantGroupId, partionKey, rowKey, isApprove);
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// workflow中设置的Reviewer把数据Escalate其他人，当数据Approve or Reject后需要把Escalate相关信息
        /// </summary>
        /// <param name="instanceId"></param>
        public async System.Threading.Tasks.Task ClearWorkflowEscalateInfoAsync(Guid instanceId)
        {
            var item = ManualApproveDao.Find(s => s.WorkflowInstanceId == instanceId);
            if (item != null)
            {
                item.EscalateFrom = "";
                item.EscalateTo = "";
                await ManualApproveDao.UpdateAsync(item);
            }
        }

        private async System.Threading.Tasks.Task UpdateManualApproveItemStatusAsync(DisposalReviewRequestInfo req)
        {
            var item = ManualApproveDao.Find(t => t.WorkflowInstanceId == req.InstanceId);
            if (item != null)
            {
                item.Status = (int)(req.Action == DisposalReviewActionEnum.Approve ? SOApproveDBStatus.Approved : SOApproveDBStatus.Rejected);
                item.ApprovedBy = req.ActionUserId;
                await ManualApproveDao.UpdateAsync(item);
            }
        }

        public async System.Threading.Tasks.Task UpdateWorkflowItemFinalStatusAsync(DisposalReviewRequestInfo req)
        {
            await UpdateManualApproveItemStatusAsync(req);
            await UpdateArchiverTableManualItemStatusAsync(req);
        }

        public void UpdatePhyManualItemCosmosDBStatus(Guid nodeId, bool isApprove)
        {
            var physicalItem = GetPhysicalRecord(nodeId);

            if (physicalItem.Status == SOApproveDBStatus.Archived //Approved
                || (physicalItem.Status == SOApproveDBStatus.WaitingApprove && !physicalItem.ExportToRECO) //Rejected
                || physicalItem.RecordStatus == RMRecordStatus.Missing
                || physicalItem.RecordStatus == RMRecordStatus.RMDeleted
                || physicalItem.RecordStatus == RMRecordStatus.Destroyed)
            {
                //item has been destoryed || rejected
            }
            //update physical item in cosmos db
            else
            {
                ExplorerDao.UpdateApproveStatus(nodeId, isApprove ? SOApproveDBStatus.Approved : SOApproveDBStatus.Rejected);
            }
        }

        #region Export Report
        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ManualApprovalTimer, Action = AuditAction.ExportHistory, AfterHandler = typeof(ManualApprovalAfterAuditHandler), BeforeHandler = typeof(ManualApprovalBeforeAuditHandler))]
        public async System.Threading.Tasks.Task GenerateReportForManualApprovalReviewingAsync(string folderPath, string fileName, string sheetName, string serverUrl)
        {
            this.serverUrl = serverUrl;
            string reportFilePath = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(folderPath, fileName + ".xlsx");
            string[][] datas = null;
            int countOfOneSheet = 65535;
            bool isAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(Contract.RoleAssignments.RMPermissionMasks.ManualReviewAdmin);
            List<RMManualApprove> dbResults = ManualApproveDao.GetExportData(isAdmin, TenantLocalValue.LogonUserId);
            List<RMManualApprove> tempList = new List<RMManualApprove>();
            int termInfoTotalCount = dbResults == null ? 0 : dbResults.Count;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            try
            {
                if (termInfoTotalCount > 0)
                {
                    ArgumentCheck.NotNull(dbResults, nameof(dbResults));
                    for (int i = 1; i < dbResults.Count + 1; i++)
                    {
                        if (tempList.Count > 0 && tempList.Count % countOfOneSheet == 0)
                        {
                            tempList.Add(dbResults[i - 1]);
                            tempList = await InsertDataToExcelAsync(reportFilePath, tempList, i, countOfOneSheet, sheetName);

                        }
                        else
                        {
                            tempList.Add(dbResults[i - 1]);
                        }
                    }
                    if (tempList.Count > 0)
                    {
                        await InsertDataToExcelAsync(reportFilePath, tempList, termInfoTotalCount, countOfOneSheet, sheetName);
                    }
                }
                else
                {
                    datas = new string[1][];
                    datas[0] = new string[] { I18NEntity.GetString("RM_Common_NoReport") };
                    ReportUtil.CreateExcel(reportFilePath, sheetName + tempList.Count / countOfOneSheet, datas);
                }

            }
            catch (Exception e)
            {
                mLogger.Warn($"An error has occurred when GenerateReportForManualApprovalReviewing, message:{e.Message}");
            }
        }

        public async Task<List<RMManualApprove>> InsertDataToExcelAsync(string reportFilePath, List<RMManualApprove> tempList, int currentInsertCount, int maxCountOfOneSheet, string sheetName)
        {
            string[][] datas = new string[currentInsertCount + 1][];
            datas = AssembleMaReviewInfoHeaderTittle(datas);
            datas = await ConvertMaReviewInfoToArrayAsync(tempList, datas);
            if (currentInsertCount <= maxCountOfOneSheet)
            {
                ReportUtil.CreateExcel(reportFilePath, sheetName, datas);
                tempList.Clear();
            }
            else
            {
                ReportUtil.InsertWorksheet(reportFilePath, sheetName + tempList.Count / maxCountOfOneSheet, datas);
                tempList.Clear();
            }
            return tempList;
        }
        public string[][] AssembleMaReviewInfoHeaderTittle(string[][] datas)
        {
            var rowIndex = 0;
            var colIndex = 0;
            datas[rowIndex] = new string[14];
            datas[rowIndex][colIndex++] = I18NEntity.GetString("RM_JS_BCM_Explorer_Datagrid_Source");
            datas[rowIndex][colIndex++] = I18NEntity.GetString("RM_JS_MA_Grid_Title");
            datas[rowIndex][colIndex++] = I18NEntity.GetString("RM_JS_JMD_Grid_Type");
            datas[rowIndex][colIndex++] = I18NEntity.GetString("RM_JS_MA_Grid_ApprovalStatus");
            datas[rowIndex][colIndex++] = I18NEntity.GetString("RM_JS_MA_Grid_ModifiedBy");
            datas[rowIndex][colIndex++] = I18NEntity.GetString("RM_JS_MA_Grid_CreatedBy");
            datas[rowIndex][colIndex++] = I18NEntity.GetString("RM_JS_MA_Grid_CreatedTime");
            datas[rowIndex][colIndex++] = I18NEntity.GetString("RM_JS_MA_Grid_Rule");
            datas[rowIndex][colIndex++] = I18NEntity.GetString("RM_JS_MA_Grid_RelatedRecords");
            datas[rowIndex][colIndex++] = I18NEntity.GetString("RM_JS_MA_Grid_RelatedRecordsAction");
            datas[rowIndex][colIndex++] = I18NEntity.GetString("RM_MA_Grid_EscalateOrReassignFrom");
            datas[rowIndex][colIndex++] = I18NEntity.GetString("RM_JS_MA_Grid_ApprovedBy");
            datas[rowIndex][colIndex++] = I18NEntity.GetString("RM_JS_Rule_DisposalClass_Title");
            datas[rowIndex][colIndex++] = I18NEntity.GetString("RM_JS_MA_Grid_Comment");
            return datas;
        }
        public async Task<string[][]> ConvertMaReviewInfoToArrayAsync(List<RMManualApprove> infos, string[][] datas)
        {
            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            int rowCount = 1;
            foreach (RMManualApprove info in infos)
            {
                var colIndex = 0;
                datas[rowCount] = new string[14];
                datas[rowCount][colIndex++] = GetI18NOfSourceFlag(info.SourceFlag);
                datas[rowCount][colIndex++] = info.LeafName;
                datas[rowCount][colIndex++] = GetManualItemType(info);
                datas[rowCount][colIndex++] = SOApproveDBStatusToString((SOApproveDBStatus)info.Status);
                datas[rowCount][colIndex++] = !string.IsNullOrEmpty(info.ModifiedBy) ? info.ModifiedBy : "";
                datas[rowCount][colIndex++] = !string.IsNullOrEmpty(info.CreatedBy) ? info.CreatedBy : "";
                datas[rowCount][colIndex++] = GeneralSettingService.ConvertTiksToDateTime(gls, info.CollectionTime, true).FormaTime;
                datas[rowCount][colIndex++] = !string.IsNullOrEmpty(info.RuleName) ? info.RuleName : "";

                var escalateFromUser = I18NEntity.GetString("RM_JS_Common_Pending");
                if (info.EscalateFrom != null)
                {
                    try
                    {
                        var uid = int.Parse(info.EscalateFrom);
                        escalateFromUser = AccountDao.Find(s => s.Id == uid).DisplayName;
                    }
                    catch (Exception e)
                    {
                        mLogger.Warn("get escalate error:{0}, uid: {1}", e.ToString(), info.EscalateFrom);
                    }
                }
                var approvedByUser = string.Empty;
                if (info.ApprovedBy != null)
                {
                    try
                    {
                        var uid = int.Parse(info.ApprovedBy);
                        approvedByUser = AccountDao.Find(s => s.Id == uid).DisplayName;
                    }
                    catch (Exception e)
                    {
                        mLogger.Warn("get approvedBy error:{0}, uid: {1}", e.ToString(), info.ApprovedBy);
                    }
                }


                if (!string.IsNullOrEmpty(info.RelatedRecords))
                {
                    StringBuilder sBuilder = new StringBuilder();
                    var reportRelatedRecords = SerializerHelper.DeserializeFromXmlString<List<ReportRelatedRecords>>(info.RelatedRecords);
                    foreach (var rProp in reportRelatedRecords)
                    {
                        if (!string.IsNullOrEmpty(rProp.Url) && rProp.Url.StartsWith("/Root/PRM/RecordsExplorer"))//physical data
                        {
                            rProp.Url = serverUrl + rProp.Url;
                        }
                        sBuilder.AppendFormat("{0}:\n{1}\n", rProp.Name, rProp.Url);
                    }
                    datas[rowCount][colIndex++] = sBuilder.ToString();
                }
                else
                {
                    datas[rowCount][colIndex++] = string.Empty;
                }

                if (info.RelatedRecordsAction == 0)
                {
                    datas[rowCount][colIndex++] = I18NEntity.GetString("RM_JS_RDM_RelatedRecordsAction_None");
                }
                else if (info.RelatedRecordsAction == 1)
                {
                    datas[rowCount][colIndex++] = I18NEntity.GetString("RM_JS_RDM_RelatedRecordsAction_Both");
                }
                else
                {
                    datas[rowCount][colIndex++] = string.Empty;
                }
                datas[rowCount][colIndex++] = escalateFromUser;
                datas[rowCount][colIndex++] = approvedByUser;
                datas[rowCount][colIndex++] = info.DisposalClass;
                datas[rowCount][colIndex++] = !string.IsNullOrEmpty(info.Comment) ? info.Comment : "";
                rowCount++;
            }
            return datas;
        }
        #endregion
        public string SOApproveDBStatusToString(SOApproveDBStatus status)
        {
            return I18NEntity.GetString($"RM_JS_MA_ApproveStatus_{status.ToString()}");
        }

        public async Task<string> GesEscalateUsersAsync(string userIdString)
        {
            var dispalyNames = new List<string>();
            var uids = userIdString.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

            var intUids = uids.Select(s => int.Parse(s));
            var owners = await AccountDao.FindListAsync(s => intUids.Contains(s.Id));
            foreach (var o in owners)
            {
                dispalyNames.Add(o.DisplayName);
            }
            return string.Join(",", dispalyNames);
        }

        public ManualExportReportInfo GetDestoryItem(AzureTableConnectContract connectionInfo, string tenantGroupId, string partitionKey, Guid nodeId, string version, bool isRetention = false)
        {
            var destoryItem = ArchiverTableDao.GetDestroyItem(connectionInfo, tenantGroupId, partitionKey, nodeId, version, isRetention);
            if (destoryItem == null)
            {
                return null;
            }
            return ConvertToManualExportReport(destoryItem);
        }

        public ManualExportReportInfo GetDestoryItemForEXO(AzureTableConnectContract connectionInfo, string tenantGroupId, string partitionKey, string rowKey)
        {
            var destoryItem = ArchiverTableDao.GetDestroyItemForEXO(connectionInfo, tenantGroupId, partitionKey, rowKey);
            if (destoryItem == null)
            {
                return null;
            }
            return ConvertToManualExportReportForEXO(destoryItem);
        }

        public ManualExportReportInfo GetDestoryItemForFS(string fsAzureTableConnectStr, string tenantGroupId, string partitionKey, string rowKey)
        {
            var destoryItem = ArchiverTableDao.GetDestroyItemForFS(fsAzureTableConnectStr, tenantGroupId, partitionKey, rowKey);
            if (destoryItem == null)
            {
                return null;
            }
            return ConvertToManualExportReportForFS(destoryItem);
        }

        public ManualExportReportInfo GetDestoryItemForSPOnPrem(string connectionInfo, string tenantGroupId, string partitionKey, Guid nodeId, string version)
        {
            var destoryItem = ArchiverTableDao.GetDestroyItemForSPOnPrem(connectionInfo, tenantGroupId, partitionKey, nodeId, version);
            if (destoryItem == null)
            {
                return null;
            }
            return ConvertToManualExportReportForSPOnPrem(destoryItem);
        }
        public ManualExportReportInfo GetPhysicalRecord(Guid id)
        {
            var destoryItem = ExplorerDao.QueryAll(s => s.Id == id).FirstOrDefault();
            if (destoryItem == null)
            {
                return null;
            }
            return ConvertToManualExportReportForPhysical(destoryItem);
        }

        public ManualExportReportInfo GetBoxRecord(Guid id)
        {
            var destoryItem = ExplorerDao.QueryAll(s => s.Id == id).FirstOrDefault();
            if (destoryItem == null)
            {
                return null;
            }
            return destoryItem.ConvertToManualExportReportForBox();
        }


        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.ManualApprovalTimer, Action = AuditAction.ChangeAction, AfterHandler = typeof(ManualApprovalAfterAuditHandler), BeforeHandler = typeof(ManualApprovalBeforeAuditHandler))]
        public async Task<RAReturnMessage> SubmitChangeActionSettingAsync(AzureTableConnectContract connectionInfo, string tenantGroupId, List<ChangedItems> changedItems, AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption relatedRecordAction)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            RAReturnMessage msg = new RAReturnMessage();
            msg.MessageType = RAMessageType.Successful;
            var changedItemsBySource = changedItems.GroupBy(c => c.SourceFlag).ToDictionary(key => key.Key, value => value);
            foreach (var sourceInfo in changedItemsBySource)
            {
                var ids = sourceInfo.Value.Select(r => r.id).ToList();
                List<RMManualApprove> items = await ManualApproveDao.FindListAsync(s => ids.Contains(s.Id));
                ManualApproveDao.UpdateManualApproveDisposalAction(ids, relatedRecordAction);
                if (sourceInfo.Key == (int)SourceFlag.SharePoint)
                {
                    Dictionary<string, List<string>> pkMappingRowKeys = new Dictionary<string, List<string>>();
                    foreach (RMManualApprove item in items)
                    {
                        if (!pkMappingRowKeys.ContainsKey(item.PartKey))
                        {
                            pkMappingRowKeys.Add(item.PartKey, new List<string>() { item.RowKey });
                        }
                        else
                        {
                            pkMappingRowKeys[item.PartKey].Add(item.RowKey);
                        }
                    }
                    foreach (KeyValuePair<string, List<string>> kv in pkMappingRowKeys)
                    {
                        string partionKey = kv.Key;
                        List<string> rowKeys = kv.Value;
                        foreach (var rowKey in rowKeys)
                        {
                            try
                            {
                                var item = ArchiverTableDao.GetArchiverItem(connectionInfo, tenantGroupId, partionKey, rowKey);
                                if (item != null)
                                {
                                    await ArchiverTableDao.UpdateItemDisposalActionAsync(connectionInfo, tenantGroupId, partionKey, item.NodeID, relatedRecordAction);
                                }
                                else
                                {
                                    msg.MessageType = RAMessageType.Failed;
                                    msg.ErrorMessage = I18NEntity.GetString("RM_JS_MA_ItemDisposal");
                                    //get item from static table
                                    var dbItem = ManualApproveDao.Find(s => s.PartKey == partionKey && s.RowKey == rowKey);
                                    var destoryItem = GetDestoryItem(connectionInfo, tenantGroupId, dbItem.SiteId.ToString(), dbItem.NodeId, dbItem.Version);
                                    if (destoryItem != null)
                                    {
                                        if (destoryItem.Status == SOApproveDBStatus.Archived || destoryItem.Status == SOApproveDBStatus.Rejected)
                                        {
                                            dbItem.ActionStatus = (int)Contract.Schedule.ActionStatus.Archiverd;
                                            var aspd = JsonConvert.DeserializeObject<ArchiverSharePointDto>(destoryItem.JsonMeta);
                                            dbItem.ActionTime = aspd.ArchivedTime.Ticks;
                                            dbItem.RelatedRecordsAction = items.Where(s => s.Id == dbItem.Id).Select(s => s.RelatedRecordsAction).First();
                                            ManualApproveDao.SaveManualApprove(dbItem);
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                mLogger.Warn("change action item in archiver table error:{0}", e.ToString());
                            }
                        }
                    }
                }
                else if (sourceInfo.Key == (int)SourceFlag.FileSystem)
                {
                    var fsAzureTableConnectStr = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
                    Dictionary<string, List<string>> pkMappingRowKeys = new Dictionary<string, List<string>>();
                    foreach (RMManualApprove item in items)
                    {
                        if (!pkMappingRowKeys.ContainsKey(item.PartKey))
                        {
                            pkMappingRowKeys.Add(item.PartKey, new List<string>() { item.RowKey });
                        }
                        else
                        {
                            pkMappingRowKeys[item.PartKey].Add(item.RowKey);
                        }
                    }
                    foreach (KeyValuePair<string, List<string>> kv in pkMappingRowKeys)
                    {
                        string partionKey = kv.Key;
                        List<string> rowKeys = kv.Value;
                        foreach (var rowKey in rowKeys)
                        {
                            try
                            {
                                var item = ArchiverTableDao.GetArchiverItemForFS(fsAzureTableConnectStr, tenantGroupId, partionKey, rowKey);
                                if (item != null)
                                {
                                    await ArchiverTableDao.UpdateItemDisposalActionForFSAsync(fsAzureTableConnectStr, tenantGroupId, partionKey, rowKey, relatedRecordAction);
                                }
                                else
                                {
                                    msg.MessageType = RAMessageType.Failed;
                                    msg.ErrorMessage = I18NEntity.GetString("RM_JS_MA_ItemDisposal");
                                    //get item from static table
                                    var dbItem = ManualApproveDao.Find(s => s.PartKey == partionKey && s.RowKey == rowKey && s.ActionStatus == (int)Contract.Schedule.ActionStatus.None);
                                    var destoryItem = GetDestoryItemForFS(fsAzureTableConnectStr, tenantGroupId, partionKey, rowKey);
                                    if (destoryItem != null)
                                    {
                                        if (destoryItem.Status == SOApproveDBStatus.Archived || destoryItem.Status == SOApproveDBStatus.Rejected)
                                        {
                                            dbItem.ActionStatus = (int)Contract.Schedule.ActionStatus.Archiverd;
                                            dbItem.ActionTime = destoryItem.ArchivedTime;
                                            dbItem.RelatedRecordsAction = items.Where(s => s.Id == dbItem.Id).Select(s => s.RelatedRecordsAction).First();
                                            ManualApproveDao.SaveManualApproveForFS(dbItem);
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                mLogger.Warn("change action item in archiver table error:{0}", e.ToString());
                            }
                        }
                    }
                }
                else if (sourceInfo.Key == (int)SourceFlag.Physical)
                {
                    List<Guid> nodeIds = new List<Guid>();
                    foreach (RMManualApprove item in items)
                    {
                        if (!nodeIds.Contains(item.NodeId))
                        {
                            nodeIds.Add(item.NodeId);
                        }
                    }
                    var allRecords = ExplorerDao.GetRecordByIds(nodeIds);
                    foreach (var record in allRecords)
                    {
                        try
                        {
                            if ((record.RecordStatus == (int)RMRecordStatus.Active || record.RecordStatus == (int)RMRecordStatus.Closed)
                                && (record.DisposalStatus == (int)SOApproveDBStatus.WaitingApprove || record.DisposalStatus == (int)SOApproveDBStatus.Rejected || record.DisposalStatus == (int)SOApproveDBStatus.Approved))
                            {
                                record.DeleteRelatedRecords = (int)relatedRecordAction;
                                ExplorerDao.UpdatePhysicalRecord(record, true);
                            }
                            else
                            {
                                msg.MessageType = RAMessageType.Failed;
                                msg.ErrorMessage = I18NEntity.GetString("RM_JS_MA_ItemDisposal");
                                //get item from static table
                                if (record.DisposalStatus == (int)SOApproveDBStatus.Archived || record.DisposalStatus == (int)SOApproveDBStatus.Rejected)
                                {
                                    var dbItem = ManualApproveDao.Find(s => s.NodeId == record.NodeId);
                                    dbItem.ActionStatus = (int)Contract.Schedule.ActionStatus.Archiverd;
                                    dbItem.ActionTime = record.DestroyedTime;
                                    dbItem.RelatedRecordsAction = items.Where(s => s.Id == dbItem.Id).Select(s => s.RelatedRecordsAction).First();
                                    ManualApproveDao.SaveManualApprove(dbItem);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            mLogger.Warn("change action item in archiver table error:{0}", e.ToString());
                        }
                    }
                }
            }
            timer.Stop();
            mLogger.Info("mark count {0} to change action take {1} ms", changedItems.Count, timer.ElapsedMilliseconds);
            return msg;
        }

        private async Task<Expression<Func<RMManualApprove, bool>>> GetManualReviewQueryLambdaAsync()
        {
            Expression<Func<RMManualApprove, bool>> lambda = null;
            Expression queryExpr = null;
            List<Expression> reviewerExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(RMManualApprove), "c");

            Expression userAndgroupExpression = await GetUserAndGroupLambdaAsync(param);
            if (userAndgroupExpression != null)
            {
                reviewerExpressionList.Add(userAndgroupExpression);
            }

            //List<RMWorkflowInstance> instances = null;

            //if (!SecurityTrimmingHelper.DoesUserHasThisPermission(Contract.RoleAssignments.RMPermissionMasks.ManualReviewAdmin))
            //{
            //    var userAndGroupIds = GetCurrentUserAndGroupIds();
            //    instances = RMWorkflowDefinitionDao.GetInstances(userAndGroupIds);
            //    var instanceExps = GetWorkflowInstanceExpression(instances, param, FilterWorkflowStatus.All);
            //    if (instanceExps.Count > 0)
            //    {
            //        reviewerExpressionList.Add(instanceExps.Aggregate(Expression.OrElse));
            //    }
            //    var siteOwnerExpression = GetWfInstanceExpressionsBySiteOwners(userAndGroupIds, FilterWorkflowStatus.All, param);
            //    if (siteOwnerExpression != null)
            //    {
            //        reviewerExpressionList.Add(siteOwnerExpression);
            //    }
            //}

            if (reviewerExpressionList.Count > 0)
            {
                queryExpr = reviewerExpressionList.Aggregate(Expression.OrElse);
                lambda = Expression.Lambda<Func<RMManualApprove, bool>>(queryExpr, param);
                mLogger.Info("Get Manual Approve Review Data:{0}", lambda.ToString());
            }

            return lambda;
        }

        private async Task<Expression<Func<RMManualApprove, bool>>> GetManualReviewQueryLambdaAsync(SourceFlag flag)
        {
            Expression<Func<RMManualApprove, bool>> lambda = null;
            Expression queryExpr = null;
            List<Expression> reviewerExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(RMManualApprove), "c");

            var flagExpression = Expression4DynamicQuery.GetEqualExpression(typeof(RMManualApprove), param, "SourceFlag", 4);

            Expression userAndgroupExpression = await GetUserAndGroupLambdaAsync(param);
            if (userAndgroupExpression != null)
            {
                reviewerExpressionList.Add(userAndgroupExpression);
            }

            //List<RMWorkflowInstance> instances = null;

            //if (!SecurityTrimmingHelper.DoesUserHasThisPermission(Contract.RoleAssignments.RMPermissionMasks.ManualReviewAdmin))
            //{
            //    var userAndGroupIds = GetCurrentUserAndGroupIds();
            //    instances = RMWorkflowDefinitionDao.GetInstances(userAndGroupIds);
            //    var instanceExps = GetWorkflowInstanceExpression(instances, param, FilterWorkflowStatus.All);
            //    if (instanceExps.Count > 0)
            //    {
            //        reviewerExpressionList.Add(instanceExps.Aggregate(Expression.OrElse));
            //    }
            //    var siteOwnerExpression = GetWfInstanceExpressionsBySiteOwners(userAndGroupIds, FilterWorkflowStatus.All, param);
            //    if (siteOwnerExpression != null)
            //    {
            //        reviewerExpressionList.Add(siteOwnerExpression);
            //    }
            //}

            if (reviewerExpressionList.Count > 0)
            {
                queryExpr = reviewerExpressionList.Aggregate(Expression.OrElse);
                queryExpr = new List<Expression> { flagExpression, queryExpr }.Aggregate(Expression.AndAlso);
                lambda = Expression.Lambda<Func<RMManualApprove, bool>>(queryExpr, param);
                mLogger.Info("Get Manual Approve Review Data:{0}", lambda.ToString());
            }
            else
            {
                lambda = Expression.Lambda<Func<RMManualApprove, bool>>(flagExpression, param);
                mLogger.Info("Get Manual Approve Review Data:{0}", lambda.ToString());
            }

            return lambda;
        }

        public async Task<string> GetAllDataInfoAsync()
        {
            var lambda = await GetManualReviewQueryLambdaAsync();
            var result = ManualApproveDao.GetTabInfo(lambda);
            return JsonConvert.SerializeObject(result);
        }

        public async Task<string> GetAllDataInfoAsync(SourceFlag flag)
        {
            var lambda = await GetManualReviewQueryLambdaAsync(flag);
            var result = ManualApproveDao.GetTabInfo(lambda);
            return JsonConvert.SerializeObject(result);
        }


        //public bool CheckArchiveTableExist()
        //{
        //    DAOAPIClientV1 mDocAveClient = new DAOAPIClientV1();
        //    AzureTableConnectContract connectionInfo = mDocAveClient.GetArchiverDataBaseConfig();
        //    return ArchiverTableDao.CheckArchiveTableExist(connectionInfo, TenantLocalValue.LogonGroupId);
        //}

        public string GetI18NOfSourceFlag(int sourceFlag)
        {
            var str = "";
            switch (sourceFlag)
            {
                case (int)SourceFlag.SharePoint:
                    str = I18NEntity.GetString("RM_JS_Common_ReportType_SharePoint");
                    break;
                case (int)SourceFlag.Exchange:
                    str = I18NEntity.GetString("RM_JS_Common_ReportType_Exchange");
                    break;
                case (int)SourceFlag.Physical:
                    str = I18NEntity.GetString("RM_JS_Common_ReportType_Physical");
                    break;
                case (int)SourceFlag.FileSystem:
                    str = I18NEntity.GetString("RM_JS_SPS_TabLabel_FS");
                    break;
                case (int)SourceFlag.SharePointOnPrem:
                    str = I18NEntity.GetString("RM_JS_SPS_TabLabel_SPLocal");
                    break;
                case (int)SourceFlag.OneDrive:
                    str = I18NEntity.GetString("RM_JS_SPS_TabLabel_OneDrive");
                    break;
                default:
                    break;
            }
            return str;
        }

        public async System.Threading.Tasks.Task UpdateRecordOwnerAsync(Guid instanceId)
        {
            try
            {
                if (ManualApproveDao.Exist(s => s.WorkflowInstanceId == instanceId))
                {
                    var item = ManualApproveDao.Find(s => s.WorkflowInstanceId == instanceId);
                    var userIds = WorkflowInstanceDao.GetReviewUserIdsByManualInfo(item);
                    var uIds = (await AccountDao.GetUserByUserIdsAsync(userIds)).Select(u => u.Id).ToList();
                    var owners = "|" + string.Join("|", uIds) + "|";
                    var successCount = 0;
                    if (item.ObjectLevel == (int)RMNodeLevel.FSFile)
                    {
                        successCount = ExplorerDao.UpdateRecordOwnerForFS(item.NodeId, owners);
                    }
                    else
                    {
                        successCount = ExplorerDao.UpdateRecordOwner(item.SiteId, item.NodeId, owners);
                    }
                    if (successCount > 0)
                    {
                        mLogger.Info($"success to update records owner:{item?.Id}, {owners}");
                    }
                    item.EscalateTo = owners;
                    await ManualApproveDao.UpdateAsync(item);
                }

            }
            catch (Exception ex)
            {
                mLogger.Error($"errror occurred while update record owner, instanceId:{instanceId}, ERROR: {ex.ToString()}");
            }

        }

        public List<string> SendEmailForWorkflows(List<Guid> ids)
        {
            var userIds = new List<string>();
            foreach (var id in ids)
            {
                var wf = ManualProcessManagementService.GetWorkflow(id);
                var beginStepNode = wf.Content.WorkflowNodes.Where(w => w.NodeType == WorkflowNodeType.BeginDisposalReview).FirstOrDefault();
                if (beginStepNode != null)
                {
                    var stepUserIds = RMWorkflowDefinitionDao.GetReviewerIdsByStepId(beginStepNode.Id);
                    if (stepUserIds != null && stepUserIds.Count > 0)
                    {
                        foreach (var stepUserId in stepUserIds)
                        {
                            if (!userIds.Contains(stepUserId))
                            {
                                userIds.Add(stepUserId);
                            }
                        }
                    }
                }
            }
            return userIds;
            //SendEmailToUsers(userIds, I18NEntity.GetString("RM_TS_RunSchedule"));
        }

        public async Task<List<AccountDto>> GetUserIdsForManualJobAsync(WorkflowDefinitionDto workflowDefinition, Guid siteId)
        {
            List<string> userIds = new List<string>();
            var beginStepNode = workflowDefinition.Content.WorkflowNodes.Where(w => w.NodeType == WorkflowNodeType.BeginDisposalReview).FirstOrDefault();
            if (beginStepNode != null)
            {
                if (beginStepNode.ReviewerType == WorkflowReviewerType.SiteOwners)
                {
                    var users = await WorkflowSiteOwnersDao.FindListAsync(item => item.DefinitionId == workflowDefinition.Id.ToString() && item.SiteId == siteId && !item.IsSPGroup);
                    userIds.AddRange(users.Select(item => item.OwnerId));
                }
                else if(beginStepNode.ReviewerType == WorkflowReviewerType.SharePointGroup)
                {
                    var groupName = beginStepNode.GroupName.Trim();
                    var users = await WorkflowSiteOwnersDao.FindListAsync(item => item.DefinitionId == workflowDefinition.Id.ToString() && item.SiteId == siteId && item.IsSPGroup && item.GroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase));
                    userIds.AddRange(users.Select(item => item.OwnerId));
                }
                else
                {
                    var stepUserIds = RMWorkflowDefinitionDao.GetReviewerIdsByStepId(beginStepNode.Id);
                    if (stepUserIds != null && stepUserIds.Count > 0)
                    {
                        foreach (var stepUserId in stepUserIds)
                        {
                            if (!userIds.Contains(stepUserId))
                            {
                                userIds.Add(stepUserId);
                            }
                        }
                    }
                }
            }
            return (await AccountDao.GetUserByUserIdsAsync(userIds)).ConvertAll(o => Convert2AccountDto(o));
        }

        private AccountDto Convert2AccountDto(RMAccount mAccount)
        {
            return new AccountDto()
            {
                Id = mAccount.Id,
                UserId = mAccount.UserId,
                DisplayName = mAccount.DisplayName,
            };
        }
        public async Task<RAReturnMessage> SyncADUsersAsync(List<ToUserInfo> users)
        {
            var returnMessage = new RAReturnMessage();
            try
            {
                if (users != null && users.Count > 0)
                {
                    await UserService.SyncUsersAsync(TenantLocalValue.LogonGroupId, users);
                }
            }
            catch (Exception ex)
            {
                returnMessage.ErrorMessage = I18NEntity.GetString("RM_RegisterUser_Error_Message");
                returnMessage.MessageType = RAMessageType.Failed;
            }
            return returnMessage;
        }

        public string GetManualItemType(RMManualApprove r)
        {
            var typeString = string.Empty;
            if (r.ObjectLevel == (int)RMReportObjectLevel.Item)
            {
                if (r.ArchiveLevel == (int)CacheNodeType.Item)
                {
                    typeString = I18NEntity.GetString("RM_RDM_RecordDetails_DataType_SPItem");
                }
                else
                {
                    typeString = Path.GetExtension(r.LeafName);
                    if (typeString.Length > 0 && typeString[0] == '.')
                    {
                        typeString = typeString.Substring(1);
                    }
                }
            }
            else
            {
                switch ((RMReportObjectLevel)r.ObjectLevel)
                {
                    case RMReportObjectLevel.SiteCollection:
                        typeString = I18NEntity.GetString("RM_JS_Rule_ObjectLevel_SiteCollection");
                        break;
                    case RMReportObjectLevel.Site:
                        typeString = I18NEntity.GetString("RM_JS_Rule_ObjectLevel_Site");
                        break;
                    case RMReportObjectLevel.List:
                        typeString = I18NEntity.GetString("RM_Common_ObjectLevel_List");
                        break;
                    case RMReportObjectLevel.Folder:
                        typeString = I18NEntity.GetString("RM_Common_ObjectLevel_Folder");
                        break;
                    case RMReportObjectLevel.PhysicalBox:
                        typeString = I18NEntity.GetString("RM_Common_ObjectLevel_PhysicalBox");
                        break;
                    case RMReportObjectLevel.PhysicalFile:
                        typeString = I18NEntity.GetString("RM_Common_ObjectLevel_PhysicalFile");
                        break;
                    case RMReportObjectLevel.PhysicalRecord:
                        typeString = I18NEntity.GetString("RM_PRM_PRE_TableItemType_Record");
                        break;
                    case RMReportObjectLevel.ExchangeOnlineItem:
                        typeString = I18NEntity.GetString("RM_JS_Rule_ObjectLevel_ExchangeOnlineItem");
                        break;
                    case RMReportObjectLevel.FSFile:
                        typeString = Path.GetExtension(r.LeafName);
                        if (typeString.Length > 0 && typeString[0] == '.')
                        {
                            typeString = typeString.Substring(1);
                        }
                        break;
                    default:
                        break;
                }
            }
            return typeString;
        }
    }
}