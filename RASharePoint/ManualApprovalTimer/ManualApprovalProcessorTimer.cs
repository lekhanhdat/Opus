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
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.SharePoint.OnPrem;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.Workflow;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.SharePointOnPrem;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Wrapper.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.RA.SharePoint.ManualApprovalTimer
{
    public class ManualApprovalProcessorTimer
    {
        private static readonly IRALogger mLogger = RALogger.GetInstance(typeof(ManualApprovalProcessorTimer));
        private BaseJobDto mJobInfo;
        private DAOAPIClient mDocAveClient;
        private AzureTableConnectContract mAzureTableConnectInfo;
        private string mTenantGroupId = TenantLocalValue.LogonGroupId;
        private string mCommomErrorMessage;
        private Dictionary<Guid, ManualRuleInfo> mCacheRuleInfo = new Dictionary<Guid, ManualRuleInfo>();
        //private List<JMManualApprovalJobDetails> mJobDetails = new List<JMManualApprovalJobDetails>();
        private Dictionary<string, string> workflowXamlDic = new Dictionary<string, string>();
        private JobContext JobContext;

        private Dictionary<Guid, IAveSite> mAveSiteCollections = new Dictionary<Guid, IAveSite>();
        private Dictionary<Guid, RMSPTreeNode> mSiteTreeNodes = new Dictionary<Guid, RMSPTreeNode>();
        private Dictionary<Guid, RMSPTreeNode> mAllSiteGroupTreeNode = null;
        private Dictionary<Guid, RMSampleEXOTreeNode> mAllEXOGroupTreeNode = null;
        private HashSet<Guid> mUnavailableSiteIds = new HashSet<Guid>();
        private Dictionary<Guid, Guid> daoRecordSiteIdMapping = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, Guid> daoRecordGroupIdMapping = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, Guid> daoRecordMailBoxIdMapping = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, Guid> daoRecordMailGroupIdMapping = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, List<Guid>> spLocalSiteIdMapping = new Dictionary<Guid, List<Guid>>();
        private string fsAzureTableConnectStr;
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

        #region interface
        private IDisposalReviewWFService mDisposalReviewWFService;
        protected IDisposalReviewWFService DisposalReviewWFService
        {
            get
            {
                if (mDisposalReviewWFService == null)
                {
                    mDisposalReviewWFService = (IDisposalReviewWFService)PlatformWindsorManager.GetService(typeof(IDisposalReviewWFService));
                }
                return mDisposalReviewWFService;
            }
        }
        private IManualProcessManagementService mManualProcessManagementService;
        protected IManualProcessManagementService ManualProcessManagementService
        {
            get
            {
                if (mManualProcessManagementService == null)
                {
                    mManualProcessManagementService = (IManualProcessManagementService)PlatformWindsorManager.GetService(typeof(IManualProcessManagementService));
                }
                return mManualProcessManagementService;
            }
        }
        //ManualProcessManagementService
        private IRuleManagerService mRuleManagerService;
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

        private IManualApprovalService mManualApprovalService;
        protected IManualApprovalService ManualApprovalService
        {
            get
            {
                if (mManualApprovalService == null)
                {
                    mManualApprovalService = (IManualApprovalService)PlatformWindsorManager.GetService(typeof(IManualApprovalService));
                }
                return mManualApprovalService;
            }
        }

        //private IJobMonitorService mJobService;
        //protected IJobMonitorService JobService
        //{
        //    get
        //    {
        //        if (mJobService == null)
        //        {
        //            mJobService = (IJobMonitorService)PlatformWindsorManager.GetService(typeof(IJobMonitorService));
        //        }
        //        return mJobService;
        //    }
        //}

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

        private IAccountDao mAccountDao;
        protected IAccountDao AccountDao
        {
            get
            {
                if (mAccountDao == null)
                {
                    mAccountDao = (IAccountDao)PlatformWindsorManager.GetService(typeof(IAccountDao));
                }
                return mAccountDao;
            }
        }

        private IRecordOwnerDao mRecordOwnerDao;
        protected IRecordOwnerDao RecordOwnerDao
        {
            get
            {
                if (mRecordOwnerDao == null)
                {
                    mRecordOwnerDao = (IRecordOwnerDao)PlatformWindsorManager.GetService(typeof(IRecordOwnerDao));
                }
                return mRecordOwnerDao;
            }
        }

        //private IJobDetailService mJobDetailService;
        //protected IJobDetailService JobDetailService
        //{
        //    get
        //    {
        //        if (mJobDetailService == null)
        //        {
        //            mJobDetailService = (IJobDetailService)PlatformWindsorManager.GetService(typeof(IJobDetailService));
        //        }
        //        return mJobDetailService;
        //    }
        //}

        private ISPSettingTreeService mSPTreeService;
        private ISPSettingTreeService RMSPTreeService
        {
            get
            {
                if (mSPTreeService == null)
                {
                    mSPTreeService = (ISPSettingTreeService)PlatformWindsorManager.GetService(typeof(ISPSettingTreeService));
                }
                return mSPTreeService;
            }
        }
        private ISharePointSettingDao mSPSettingDao;
        protected ISharePointSettingDao SPSettingDao
        {
            get
            {
                if (mSPSettingDao == null)
                {
                    mSPSettingDao = (ISharePointSettingDao)PlatformWindsorManager.GetService(typeof(ISharePointSettingDao));
                }
                return mSPSettingDao;
            }
        }
        private IPhysicalRecordSettingDao mPhysicalRecordSettingDao;
        protected IPhysicalRecordSettingDao PhysicalRecordSettingDao
        {
            get
            {
                if (mPhysicalRecordSettingDao == null)
                {
                    mPhysicalRecordSettingDao = (IPhysicalRecordSettingDao)PlatformWindsorManager.GetService(typeof(IPhysicalRecordSettingDao));
                }
                return mPhysicalRecordSettingDao;
            }
        }
        private IEXOSettingDao mEXOSettingDao;
        protected IEXOSettingDao EXOSettingDao
        {
            get
            {
                if (mEXOSettingDao == null)
                {
                    mEXOSettingDao = (IEXOSettingDao)PlatformWindsorManager.GetService(typeof(IEXOSettingDao));
                }
                return mEXOSettingDao;
            }
        }

        private IWorkflowInstanceDao mWorkflowInstance;
        protected IWorkflowInstanceDao WorkflowInstanceDao
        {
            get
            {
                if (mWorkflowInstance == null)
                {
                    mWorkflowInstance = (IWorkflowInstanceDao)PlatformWindsorManager.GetService(typeof(IWorkflowInstanceDao));
                }
                return mWorkflowInstance;
            }

        }

        private IRMEmailItemDao mEmailItemDao;
        protected IRMEmailItemDao EmailItemDao
        {
            get
            {
                if (mEmailItemDao == null)
                {
                    mEmailItemDao = (IRMEmailItemDao)PlatformWindsorManager.GetService(typeof(IRMEmailItemDao));
                }
                return mEmailItemDao;
            }

        }

        private IFileSystemSettingDao mFileSystemSettingDao;
        protected IFileSystemSettingDao FileSystemSettingDao
        {
            get
            {
                if (mFileSystemSettingDao == null)
                {
                    mFileSystemSettingDao = (IFileSystemSettingDao)PlatformWindsorManager.GetService(typeof(IFileSystemSettingDao));
                }
                return mFileSystemSettingDao;
            }
        }

        private ISharePointOnPremiseSettingDao mSharePointOnPremiseSettingDao;
        protected ISharePointOnPremiseSettingDao SharePointOnPremiseSettingDao
        {
            get
            {
                if (mSharePointOnPremiseSettingDao == null)
                {
                    mSharePointOnPremiseSettingDao = (ISharePointOnPremiseSettingDao)PlatformWindsorManager.GetService(typeof(ISharePointOnPremiseSettingDao));
                }
                return mSharePointOnPremiseSettingDao;
            }
        }
        private IOneDriveSettingDao mOneDriveSettingDao;
        public IOneDriveSettingDao OneDriveSettingDao
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

        #endregion
        public ManualApprovalProcessorTimer(string jobId)
        {
            mJobInfo = new BaseJobDto() { Id = jobId, JobType = (int)JobType.ManualApprovalTimer };

            DAOAPIClientV1 Client1 = new DAOAPIClientV1();
            mAzureTableConnectInfo = Client1.GetArchiverDataBaseConfig();
            fsAzureTableConnectStr = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
            JobContext = JobContext.GetInstance(jobId, Contract.JobMonitor.JobType.TermSynchronization);
            JobContext.ReportManager.IncreaseBase(1);
            JobContext.ReportManager.StartUpdateJobProgress();
        }

        public void RunJob()
        {
            var hasError = false;
            var exoHasError = false;
            var hasSucceed = false;
            List<string> userIds = new List<string>();
            List<string> spUserIds = new List<string>();
            List<string> exoUserIds = new List<string>();
            List<string> phUserIds = new List<string>();
            List<string> fsUserIds = new List<string>();
            List<string> spLocalUserIds = new List<string>();
            //JobService.UpdateJobProgress(mJobInfo.Id, 1);

            JobContext.ReportManager.Increase();
            if (SPSettingDao.CountAll() != 0)
            {
                mLogger.Info($"Start process sharepoint online manual approval job.");
                ProcessSP(ref hasError, ref hasSucceed, ref spUserIds);
                userIds.AddRange(spUserIds);
            }
            if (OneDriveSettingDao.CountAll() != 0)
            {
                mLogger.Info($"Start process onedrive manual approval job.");
                ProcessOneDrive(ref hasError, ref hasSucceed, ref spUserIds);
                userIds.AddRange(spUserIds);
            }
            if (EXOSettingDao.CountAll() != 0)
            {
                mLogger.Info($"Start process exchange online manual approval job.");
                ProcessEXO(ref exoHasError, ref hasSucceed, ref exoUserIds);
                userIds.AddRange(exoUserIds);
            }
            if (PhysicalRecordSettingDao.CountAll() != 0)
            {
                mLogger.Info($"Start process physical record manual approval job.");
                ProcessPhysicalRecord(ref exoHasError, ref hasSucceed, ref phUserIds);
                userIds.AddRange(phUserIds);
            }
            if (FileSystemSettingDao.CountAll() != 0)
            {
                mLogger.Info($"Start process file system manual approval job.");
                ProcessFS(ref exoHasError, ref hasSucceed, ref fsUserIds);
                userIds.AddRange(fsUserIds);
            }
            if (SharePointOnPremiseSettingDao.CountAll() != 0)
            {
                mLogger.Info($"Start process sharepoint on-prem manual approval job.");
                ProcessSPLocal(ref exoHasError, ref hasSucceed, ref spLocalUserIds);
                userIds.AddRange(spLocalUserIds);
            }
            SendEmailForWorkFlowManualItems(userIds);
            //if (mJobDetails.Count > 0)
            //{
            //    JobDetailService.UpdateJobDetails(mJobDetails, mJobInfo);
            //    mJobDetails.Clear();
            //}
            //JobDetailService.UploadJobDetailsAndReport(mJobInfo);
            if (hasError || exoHasError)
            {
                if (hasSucceed)
                {
                    JobContext.ReportManager.SetJobFinished(JobStatus.FinishWithException, "RM_JMD_ContentDueSummary");
                    //JobService.UpdateJobStatus(mJobInfo.Id, JobStatus.FinishWithException, "RM_JMD_ContentDueSummary");
                }
                else
                {
                    JobContext.ReportManager.SetJobFinished(JobStatus.Failed, "RM_JMD_ContentDueSummary");
                    //JobService.UpdateJobStatus(mJobInfo.Id, JobStatus.Failed,"RM_JMD_ContentDueSummary");
                }
            }
            else
            {
                JobContext.ReportManager.SetJobFinished(JobStatus.Finished);
                //JobService.UpdateJobStatus(mJobInfo.Id, JobStatus.Finished);
            }
        }

        public void ProcessSP(ref bool hasError, ref bool hasSucceed, ref List<string> spUserIds)
        {
            List<UserInfo> sendEmailUsers = new List<UserInfo>();
            //向sql db中存入archiver table中MA的数据。
            IEnumerable<ManualExportReportInfo> reports = null;
            var cacheWorkflowIds = new List<Guid>();
            var sourceFlag = SourceFlag.SharePoint;
            try
            {
                //Check db [RMRecordOwners] is empty 
                RMSPTreeNode farmNode = RMSPTreeService.LoadFarm()[0];
                mAllSiteGroupTreeNode = RMSPTreeService.Browse(farmNode, false, RMBrowseTreeNodeSourceType.SharepointOnline).ToDictionary(t => new Guid(t.Id));
                reports = ManualApprovalService.GetManualExportReports(mAzureTableConnectInfo, mTenantGroupId, sourceFlag);
                var roGroups = ManualApprovalService.GetReportsManagement(reports, ref daoRecordGroupIdMapping, ref daoRecordSiteIdMapping);
                JobContext.ReportManager.IncreaseBase(reports.Count());
                foreach (var report in reports)
                {
                    try
                    {
                        mLogger.Info($"Report info, Site Url: [{report.SiteUrl}], Web Id: [{report.WebID}], List Id: [{report.ListID}]. Level: [{report.ObjectLevel}].");
                        JobContext.ReportManager.Increase();
                        var settingInfo = GetSettingInfoForSharePointOnline(report);
                        var ruleInfo = GetRuleInfo(report.RuleID);
                        if (ruleInfo == null)
                        {
                            ManualApprovalService.MarkApprovalingObjectsToExportedStatus(mAzureTableConnectInfo, mTenantGroupId, report.PartKey, report.RowKey);
                            SendJobReportDetails(report, JobDetailsStatus.Failed, string.Empty, "RM_RDM_Rule_RuleIsDeleted");
                            hasError = true;
                            continue;
                        }

                        var userIds = new List<int>();
                        List<string> ownerNames = new List<string>();

                        #region local

                        if (settingInfo != null && settingInfo.ApprovalType != DB.Model.ApprovalType.None)
                        {
                            mLogger.Info($"Use settings manual approval config. Approval Type: [{settingInfo.ApprovalType}], Is send email to owner: [{settingInfo.EMailToRecordOwner}].");
                            if (settingInfo.ApprovalType == DB.Model.ApprovalType.ApprovalProcess)
                            {
                                var localRuleInfo = new ManualRuleInfo
                                {
                                    WorkflowId = settingInfo.WorkflowReferenceId,
                                    IsSendEmailToOwner = settingInfo.EMailToRecordOwner,
                                    Criteria = ruleInfo.Criteria,
                                    RuleName = ruleInfo.RuleName,
                                };
                                StartWorkflow(report, localRuleInfo, sourceFlag, ref hasError, ref hasSucceed);
                                var workflowRefrenceId = Guid.Parse(settingInfo.WorkflowReferenceId);
                                if (settingInfo.EMailToRecordOwner && !cacheWorkflowIds.Contains(workflowRefrenceId))
                                {
                                    cacheWorkflowIds.Add(workflowRefrenceId);
                                }
                                continue;
                            }
                            else if (settingInfo.ApprovalType == DB.Model.ApprovalType.RecordOwners)
                            {
                                var group = roGroups.Find(item => item.SPSettingId == settingInfo.Id);
                                var owners = new List<RecordOwnerDto>();
                                if (settingInfo.EMailToRecordOwner)
                                {
                                    AddToEmailUser(sendEmailUsers, out userIds, out owners, group);
                                }
                                else
                                {
                                    userIds = group.Owners.Select(s => s.LnkId).Distinct().ToList();
                                    owners = group.Owners;
                                }
                                ownerNames.AddRange(owners.Select(item => item.DisplayName));
                            }
                        }
                        #endregion
                        #region Rule Management
                        else
                        {
                            mLogger.Info($"Use rule management manual approval config. Use workflow: [{!string.IsNullOrEmpty(ruleInfo.WorkflowId)}], Is send email to owner: [{ruleInfo.IsSendEmailToOwner}]");
                            #region Start Workflow logic
                            if (!string.IsNullOrEmpty(ruleInfo.WorkflowId))
                            {
                                List<string> recordsOwner = new List<string>();
                                StartWorkflow(report, ruleInfo, sourceFlag, ref hasError, ref hasSucceed);
                                var workflowRefrenceId = Guid.Parse(ruleInfo.WorkflowId);
                                if (ruleInfo.IsSendEmailToOwner && !cacheWorkflowIds.Contains(workflowRefrenceId))
                                {
                                    cacheWorkflowIds.Add(workflowRefrenceId);
                                }
                                continue;
                            }
                            #endregion

                            if (ruleInfo.IsSendEmailToOwner)
                            {
                                foreach (var user in ruleInfo.Users)
                                {
                                    if (!sendEmailUsers.Any(s => s.UserId == user.UserId))
                                    {
                                        sendEmailUsers.Add(user);
                                    }
                                }
                            }

                            if (ruleInfo.Users != null)
                            {
                                foreach (var user in ruleInfo.Users)
                                {
                                    var dbUser = AccountDao.Find(s => s.UserId == user.UserId && s.IsRemoved == 0);
                                    if (dbUser == null)
                                    {
                                        AccountDao.Create(new RMAccount()
                                        {
                                            DisplayName = user.DisplayName,
                                            UserId = user.UserId,
                                            UserPrincipalName = user.UserPrincipalName,
                                            ObjectType = RMActiveDirectoryObjectType.User
                                        });
                                        userIds.Add(AccountDao.Find(s => s.UserId == user.UserId && s.IsRemoved == 0).Id);
                                    }
                                    else
                                    {
                                        userIds.Add(dbUser.Id);
                                    }
                                }
                                ownerNames.AddRange(ruleInfo.Users.Select(s => s.DisplayName));
                            }
                        }
                        #endregion

                        report.RuleInfo = ruleInfo;
                        if (report.ObjectLevel == RMReportObjectLevel.SiteCollection)
                        {
                            report.ContentType = "RM_JS_Rule_ObjectLevel_SiteCollection";
                        }

                        if (userIds?.Count == 0)
                        {
                            mLogger.Warn("no records owner set,partKey:{0},rowKey:{1}, Rule Id:{2}", report.PartKey, report.RowKey, report.RuleID);
                            SendJobReportDetails(report, JobDetailsStatus.Failed, string.Empty, $"RM_MA_NoRecordOwner{I18NEntity.Separator}{ruleInfo.RuleName}");
                            hasError = true;
                            continue;
                        }

                        var maItem = ConvertReportToEntity(report);
                        var escalateTo = string.Empty;
                        userIds = userIds.Distinct().ToList();
                        foreach (var id in userIds)
                        {
                            escalateTo += id + "|";
                        }

                        maItem.EscalateTo = escalateTo;
                        maItem.SourceFlag = (int)sourceFlag;
                        ManualApproveDao.SaveManualApproveItem(maItem);

                        try
                        {
                            var successCount = ExplorerDao.UpdateRecordOwner(maItem.SiteId, maItem.NodeId, escalateTo);
                            if (successCount > 0)
                            {
                                mLogger.Info($"success to update records owner:{maItem?.Id}, {escalateTo}");
                            }
                        }
                        catch (Exception ex)
                        {
                            mLogger.Error("update explorer data record owner error:{0}", ex.ToString());
                        }

                        ManualApprovalService.MarkApprovalingObjectsToExportedStatus(mAzureTableConnectInfo, mTenantGroupId, report.PartKey, report.RowKey);
                        ownerNames = ownerNames.Distinct().ToList();
                        SendJobReportDetails(report, JobDetailsStatus.Successful, string.Join(";", ownerNames));
                        hasSucceed = true;
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("error occurred while export waiting for approval data, partkey:{0}, rowKey:{1}, ERROR:{2}", report?.PartKey, report?.RowKey, ex);
                        SendJobReportDetails(report, JobDetailsStatus.Failed, string.Empty, ex.Message);
                        hasError = true;
                    }


                }
            }
            //catch (JobStopException ex)
            //{
            //    throw new JobStopException("This Job is stopped.");
            //}
            catch (AzureTableNotExistException ex)
            {
                mLogger.Error("error occurred while get manual approval data,table not exist,ERROR:{0}.", ex);
                mCommomErrorMessage = I18NEntity.GetString("RM_MA_NoTable");
            }

            #region Send Email

            spUserIds = ManualCache.Instance.TryGetOwnerIds(cacheWorkflowIds);
            foreach (var user in sendEmailUsers)
            {
                if (!spUserIds.Contains(user.UserId))
                {
                    spUserIds.Add(user.UserId);
                }
            }
            #endregion
            #region Check whether the data has been archiverd updates the status of the data in RADB
            try
            {
                //检查sql中approved数据，是否被archive
                var approveDatas = ManualApproveDao.GetAllApproveOrRejectedData(sourceFlag);
                foreach (var approveData in approveDatas)
                {
                    if (!reports.Any(s => s.PartKey == approveData.PartKey && s.RowKey == approveData.RowKey))
                    {
                        //继续查找 static 表，
                        try
                        {
                            var destoryItem = ManualApprovalService.GetDestoryItem(mAzureTableConnectInfo, mTenantGroupId, approveData.SiteId.ToString(), approveData.NodeId, approveData.Version);
                            if (destoryItem != null)
                            {
                                if (destoryItem.Status == SOApproveDBStatus.Archived || destoryItem.Status == SOApproveDBStatus.Rejected)
                                {
                                    approveData.ActionStatus = (int)Contract.Schedule.ActionStatus.Archiverd;
                                    var aspd = JsonConvert.DeserializeObject<ArchiverSharePointDto>(destoryItem.JsonMeta);
                                    approveData.ActionTime = aspd.ArchivedTime.Ticks;
                                    ManualApproveDao.SaveManualApprove(approveData);
                                    var recordOwners = !string.IsNullOrEmpty(approveData.EscalateTo) ? ManualApprovalService.GesEscalateUsers(approveData.EscalateTo) : "";
                                    SendJobReportDetails(approveData, JobDetailsStatus.Successful, recordOwners);
                                    hasSucceed = true;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            mLogger.Warn("Import item {0} error {1}", approveData.Url, e.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Error("Import error {0}", e.ToString());
            }
            #endregion
        }

        public void ProcessFS(ref bool hasError, ref bool hasSucceed, ref List<string> fsUserIds)
        {
            List<UserInfo> sendEmailUsers = new List<UserInfo>();
            IEnumerable<ManualExportReportInfo> reports = null;
            var cacheWorkflowIds = new List<Guid>();
            try
            {
                reports = ManualApprovalService.GetManualExportReportsForFS(fsAzureTableConnectStr, mTenantGroupId);
                var scopeIds = reports.Select(o => new Guid(o.ScopeID)).Distinct().ToList();
                var roGroups = FileSystemSettingDao.GetRecordOwners(scopeIds);
                JobContext.ReportManager.IncreaseBase(reports.Count());
                foreach (var report in reports)
                {
                    try
                    {
                        JobContext.ReportManager.Increase();

                        var settingInfo = GetSettingInfoForFileSystem(report);
                        var ruleInfo = GetRuleInfo(report.RuleID);
                        if (ruleInfo == null)
                        {
                            ManualApprovalService.MarkApprovalingObjectsToExportedStatusForFS(fsAzureTableConnectStr, mTenantGroupId, report.PartKey, report.RowKey);
                            SendJobReportDetails(report, JobDetailsStatus.Failed, string.Empty, "RM_RDM_Rule_RuleIsDeleted");
                            hasError = true;
                            continue;
                        }

                        var userIds = new List<int>();
                        List<string> ownerNames = new List<string>();

                        if (settingInfo.ApprovalType != DB.Model.ApprovalType.None)
                        {
                            mLogger.Info($"Use settings manual approval config. Approval Type: [{settingInfo.ApprovalType}], Is send email to owner: [{settingInfo.EMailToRecordOwner}].");
                            if (settingInfo.ApprovalType == DB.Model.ApprovalType.ApprovalProcess)
                            {
                                var localRuleInfo = new ManualRuleInfo
                                {
                                    FSWorkflowId = settingInfo.WorkflowReferenceId,
                                    FSIsSendEmailToOwner = settingInfo.EMailToRecordOwner,
                                    FSCriteria = ruleInfo.FSCriteria,
                                    RuleName = ruleInfo.RuleName,
                                };
                                StartWorkflow(report, localRuleInfo, SourceFlag.FileSystem, ref hasError, ref hasSucceed);
                                var workflowRefrenceId = Guid.Parse(settingInfo.WorkflowReferenceId);
                                if (settingInfo.EMailToRecordOwner && !cacheWorkflowIds.Contains(workflowRefrenceId))
                                {
                                    cacheWorkflowIds.Add(workflowRefrenceId);
                                }
                                continue;
                            }
                            else if (settingInfo.ApprovalType == DB.Model.ApprovalType.RecordOwners)
                            {
                                var group = roGroups.Find(item => item.SPSettingId == settingInfo.Id);
                                var owners = new List<RecordOwnerDto>();
                                if (settingInfo.EMailToRecordOwner)
                                {
                                    AddToEmailUser(sendEmailUsers, out userIds, out owners, group);
                                }
                                else
                                {
                                    userIds = group.Owners.Select(s => s.LnkId).Distinct().ToList();
                                    owners = group.Owners;
                                }
                                ownerNames.AddRange(owners.Select(item => item.DisplayName));
                            }
                        }
                        else
                        {
                            mLogger.Info($"Use rule management manual approval config. Use workflow: [{!string.IsNullOrEmpty(ruleInfo.FSWorkflowId)}], Is send email to owner: [{ruleInfo.FSIsSendEmailToOwner}]");

                            #region Start Workflow logic
                            if (!string.IsNullOrEmpty(ruleInfo.FSWorkflowId))
                            {
                                List<string> recordsOwner = new List<string>();
                                StartWorkflow(report, ruleInfo, SourceFlag.FileSystem, ref hasError, ref hasSucceed);
                                var workflowRefrenceId = Guid.Parse(ruleInfo.FSWorkflowId);
                                if (ruleInfo.FSIsSendEmailToOwner && !cacheWorkflowIds.Contains(workflowRefrenceId))
                                {
                                    cacheWorkflowIds.Add(workflowRefrenceId);
                                }
                                continue;
                            }
                            #endregion

                            if (ruleInfo.FSIsSendEmailToOwner)
                            {
                                foreach (var user in ruleInfo.FSUsers)
                                {
                                    if (!sendEmailUsers.Any(s => s.UserId == user.UserId))
                                    {
                                        sendEmailUsers.Add(user);
                                    }
                                }
                            }

                            if (ruleInfo.FSUsers != null)
                            {
                                foreach (var user in ruleInfo.FSUsers)
                                {
                                    var dbUser = AccountDao.Find(s => s.UserId == user.UserId && s.IsRemoved == 0);
                                    if (dbUser == null)
                                    {
                                        AccountDao.Create(new RMAccount()
                                        {
                                            DisplayName = user.DisplayName,
                                            UserId = user.UserId,
                                            UserPrincipalName = user.UserPrincipalName,
                                            ObjectType = RMActiveDirectoryObjectType.User
                                        });
                                        userIds.Add(AccountDao.Find(s => s.UserId == user.UserId && s.IsRemoved == 0).Id);
                                    }
                                    else
                                    {
                                        userIds.Add(dbUser.Id);
                                    }
                                }
                                ownerNames.AddRange(ruleInfo.FSUsers.Select(s => s.DisplayName));
                            }
                        }

                        report.RuleInfo = ruleInfo;
                        if (userIds?.Count == 0)
                        {
                            mLogger.Warn("no records owner set, rowKey:{0}, Rule Id:{1}", report.NodeID, report.RuleID);
                            SendJobReportDetails(report, JobDetailsStatus.Failed, string.Empty, $"RM_MA_NoRecordOwner{I18NEntity.Separator}{ruleInfo.RuleName}");
                            hasError = true;
                            continue;
                        }

                        var maItem = ConvertReportToEntityForFS(report);
                        var escalateTo = string.Empty;
                        userIds = userIds.Distinct().ToList();
                        foreach (var id in userIds)
                        {
                            escalateTo += id + "|";
                        }

                        maItem.EscalateTo = escalateTo;
                        maItem.SourceFlag = (int)SourceFlag.FileSystem;
                        ManualApproveDao.SaveManualApproveForFS(maItem);
                        try
                        {
                            //fs适用此方法
                            ExplorerDao.UpdateRecordOwnerForPhysical(maItem.NodeId, escalateTo);
                        }
                        catch (Exception ex)
                        {
                            mLogger.Error("update explorer data record owner error:{0}", ex.ToString());
                        }

                        ManualApprovalService.MarkApprovalingObjectsToExportedStatusForFS(fsAzureTableConnectStr, mTenantGroupId, report.PartKey, report.RowKey);
                        //if (ruleInfo.FSUsers != null)
                        //{
                        //    ownerNames.AddRange(ruleInfo.FSUsers.Select(s => s.DisplayName));
                        //}
                        ownerNames = ownerNames.Distinct().ToList();
                        //Save Rule FSCriteria info to Criteria for Job Detail.
                        if (report.RuleInfo != null)
                        {
                            report.RuleInfo.Criteria = report.RuleInfo.FSCriteria;
                        }
                        SendJobReportDetails(report, JobDetailsStatus.Successful, string.Join(";", ownerNames));
                        hasSucceed = true;
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("error occurred while export waiting for approval data, rowKey:{0}, ERROR:{1}", report?.RowKey, ex.ToString());
                        SendJobReportDetails(report, JobDetailsStatus.Failed, string.Empty, ex.Message);
                        hasError = true;
                    }
                }
            }
            catch (AzureTableNotExistException ex)
            {
                mLogger.Error("error occurred while get manual approval data,table not exist,ERROR:{0}.", ex.ToString());
                mCommomErrorMessage = I18NEntity.GetString("RM_MA_NoTable");
            }

            #region Send Email

            fsUserIds = ManualCache.Instance.TryGetOwnerIds(cacheWorkflowIds);
            foreach (var user in sendEmailUsers)
            {
                if (!fsUserIds.Contains(user.UserId))
                {
                    fsUserIds.Add(user.UserId);
                }
            }
            #endregion
            #region Check whether the data has been archiverd updates the status of the data in RADB
            try
            {
                //检查sql中approved数据，是否被archive
                var approveDatas = ManualApproveDao.GetAllApproveOrRejectedData(SourceFlag.FileSystem);
                foreach (var approveData in approveDatas)
                {
                    if (!reports.Any(s => s.PartKey == approveData.PartKey && s.RowKey == approveData.RowKey))
                    {
                        //继续查找 static 表，
                        try
                        {
                            var destoryItem = ManualApprovalService.GetDestoryItemForFS(fsAzureTableConnectStr, mTenantGroupId, approveData.PartKey, approveData.RowKey);
                            if (destoryItem != null)
                            {
                                if (destoryItem.Status == SOApproveDBStatus.Archived || destoryItem.Status == SOApproveDBStatus.Rejected)
                                {
                                    approveData.ActionStatus = (int)Contract.Schedule.ActionStatus.Archiverd;
                                    approveData.ActionTime = destoryItem.ArchivedTime;
                                    ManualApproveDao.SaveManualApproveForFS(approveData);
                                    var recordOwners = !string.IsNullOrEmpty(approveData.EscalateTo) ? ManualApprovalService.GesEscalateUsers(approveData.EscalateTo) : "";
                                    SendJobReportDetails(approveData, JobDetailsStatus.Successful, recordOwners);
                                    hasSucceed = true;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            mLogger.Warn("Import item {0} error {1}", approveData.Url, e.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Error("Import error {0}", e.ToString());
            }
            #endregion
        }

        public void ProcessPhysicalRecord(ref bool hasError, ref bool hasSucceed, ref List<string> phUserIds)
        {
            #region Sync data to manual table
            List<UserInfo> sendEmailUsers = new List<UserInfo>();
            IEnumerable<ManualExportReportInfo> waitingApproveItems = null;
            var cacheWorkflowIds = new List<Guid>();
            try
            {
                waitingApproveItems = ManualApprovalService.GetManualExportReportsForPhysical();
                var roGroups = ManualApprovalService.GetRecordOwnerGroupForPhysical(waitingApproveItems);
                JobContext.ReportManager.IncreaseBase(waitingApproveItems.Count());
                foreach (var waitingApproveItem in waitingApproveItems)
                {
                    try
                    {
                        JobContext.ReportManager.Increase();

                        var settingInfo = GetSettingInfoForPhysicalRecords(waitingApproveItem);
                        var ruleInfo = GetRuleInfo(waitingApproveItem.RuleID);
                        if (ruleInfo == null)
                        {
                            ManualApprovalService.MarkToExportedStatusForPhysical(waitingApproveItem.NodeID);
                            SendJobReportDetails(waitingApproveItem, JobDetailsStatus.Failed, string.Empty, "RM_RDM_Rule_RuleIsDeleted");
                            hasError = true;
                            continue;
                        }

                        var userIds = new List<int>();
                        List<string> ownerNames = new List<string>();

                        if (settingInfo.ApprovalType != DB.Model.ApprovalType.None)
                        {
                            mLogger.Info($"Use settings manual approval config. Approval Type: [{settingInfo.ApprovalType}], Is send email to owner: [{settingInfo.EMailToRecordOwner}].");
                            if (settingInfo.ApprovalType == DB.Model.ApprovalType.ApprovalProcess)
                            {
                                var localRuleInfo = new ManualRuleInfo
                                {
                                    PhysicalCriteria = ruleInfo?.PhysicalCriteria,
                                    PhyWorkflowId = settingInfo.WorkflowReferenceId,
                                    PhysicalIsSendEmailToOwner = settingInfo.EMailToRecordOwner,
                                    RuleName = ruleInfo.RuleName,
                                };
                                StartWorkflow(waitingApproveItem, localRuleInfo, SourceFlag.Physical, ref hasError, ref hasSucceed);
                                var workflowRefrenceId = Guid.Parse(settingInfo.WorkflowReferenceId);
                                if (settingInfo.EMailToRecordOwner && !cacheWorkflowIds.Contains(workflowRefrenceId))
                                {
                                    cacheWorkflowIds.Add(workflowRefrenceId);
                                }
                                continue;
                            }
                            else if (settingInfo.ApprovalType == DB.Model.ApprovalType.RecordOwners)
                            {
                                var group = roGroups.Find(item => item.SPSettingId == settingInfo.Id);
                                var owners = new List<RecordOwnerDto>();
                                if (settingInfo.EMailToRecordOwner)
                                {
                                    AddToEmailUser(sendEmailUsers, out userIds, out owners, group);
                                }
                                else
                                {
                                    userIds = group.Owners.Select(s => s.LnkId).Distinct().ToList();
                                    owners = group.Owners;
                                }
                                ownerNames.AddRange(owners.Select(item => item.DisplayName));
                            }
                        }
                        else
                        {
                            mLogger.Info($"Use rule management manual approval config. Use workflow: [{!string.IsNullOrEmpty(ruleInfo.PhyWorkflowId)}], Is send email to owner: [{ruleInfo.PhysicalIsSendEmailToOwner}]");

                            #region Start Workflow logic
                            if (!string.IsNullOrEmpty(ruleInfo.PhyWorkflowId))
                            {
                                StartWorkflow(waitingApproveItem, ruleInfo, SourceFlag.Physical, ref hasError, ref hasSucceed);
                                var workflowRefrenceId = Guid.Parse(ruleInfo.PhyWorkflowId);
                                if (ruleInfo.PhysicalIsSendEmailToOwner && !cacheWorkflowIds.Contains(workflowRefrenceId))
                                {
                                    cacheWorkflowIds.Add(workflowRefrenceId);
                                }
                                continue;
                            }
                            #endregion

                            if (ruleInfo.PhysicalIsSendEmailToOwner)
                            {
                                foreach (var user in ruleInfo.PhysicalUsers)
                                {
                                    if (!sendEmailUsers.Any(s => s.UserId == user.UserId))
                                    {
                                        sendEmailUsers.Add(user);
                                    }
                                }
                            }

                            if (ruleInfo.PhysicalUsers != null)
                            {
                                foreach (var user in ruleInfo.PhysicalUsers)
                                {
                                    var dbUser = AccountDao.Find(s => s.UserId == user.UserId && s.IsRemoved == 0);
                                    if (dbUser == null)
                                    {
                                        AccountDao.Create(new RMAccount()
                                        {
                                            DisplayName = user.DisplayName,
                                            UserId = user.UserId,
                                            UserPrincipalName = user.UserPrincipalName,
                                            ObjectType = RMActiveDirectoryObjectType.User
                                        });
                                        userIds.Add(AccountDao.Find(s => s.UserId == user.UserId && s.IsRemoved == 0).Id);
                                    }
                                    else
                                    {
                                        userIds.Add(dbUser.Id);
                                    }
                                }
                                ownerNames.AddRange(ruleInfo.PhysicalUsers.Select(s => s.DisplayName));
                            }
                        }

                        waitingApproveItem.RuleInfo = ruleInfo;
                        if (userIds?.Count == 0)
                        {
                            mLogger.Warn("no records owner set,NodeId:{0}, Rule Id:{1}", waitingApproveItem.NodeID, waitingApproveItem.RuleID);
                            SendJobReportDetails(waitingApproveItem, JobDetailsStatus.Failed, string.Empty, string.Format(I18NEntity.GetString("RM_MA_HaveNotRecordOwner"), ruleInfo.RuleName));
                            hasError = true;
                            continue;
                        }

                        var maItem = ConvertReportToEntityForPhysical(waitingApproveItem);
                        var escalateTo = string.Empty;
                        userIds = userIds.Distinct().ToList();
                        foreach (var id in userIds)
                        {
                            escalateTo += id + "|";
                        }

                        maItem.EscalateTo = escalateTo;
                        maItem.SourceFlag = (int)SourceFlag.Physical;
                        ManualApproveDao.SaveManualApproveForPhysical(maItem);
                        try
                        {
                            ExplorerDao.UpdateRecordOwnerForPhysical(maItem.NodeId, escalateTo);
                        }
                        catch (Exception ex)
                        {
                            mLogger.Error("update explorer data record owner error:{0}", ex.ToString());
                        }
                        ManualApprovalService.MarkToExportedStatusForPhysical(maItem.NodeId);
                        ownerNames = ownerNames.Distinct().ToList();
                        //Save Rule PhysicalCriteria info to Criteria for Job Detail.
                        if (waitingApproveItem.RuleInfo != null)
                        {
                            waitingApproveItem.RuleInfo.Criteria = waitingApproveItem.RuleInfo.PhysicalCriteria;
                        }
                        SendJobReportDetails(waitingApproveItem, JobDetailsStatus.Successful, string.Join(";", ownerNames));
                        hasSucceed = true;
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("error occurred while export waiting for approval data, partkey:{0}, rowKey:{1}, ERROR:{2}", waitingApproveItem?.PartKey, waitingApproveItem?.RowKey, ex.ToString());
                        SendJobReportDetails(waitingApproveItem, JobDetailsStatus.Failed, string.Empty, ex.Message);
                        hasError = true;
                    }

                }
            }
            catch (AzureTableNotExistException ex)
            {
                mLogger.Error("error occurred while get manual approval data,table not exist,ERROR:{0}.", ex.ToString());
                mCommomErrorMessage = I18NEntity.GetString("RM_MA_NoTable");
            }
            #endregion

            #region Send Email
            // TODO sendEmailUsers
            //if (sendEmailUsers != null && sendEmailUsers.Count > 0)
            //{
            //    foreach (var user in sendEmailUsers)
            //    {
            //        EmailMessageDto emailDto = new EmailMessageDto();
            //        emailDto.DetailMap = new Dictionary<string, object>();
            //        emailDto.DetailMap.Add(DetailKey.To.ToString(), user.DisplayName);
            //        emailDto.Receivers = user.UserPrincipalName;
            //        emailDto.JobId = mJobInfo.Id;
            //        MailUtil.SendSyncEmail(emailDto);
            //    }
            //}
            phUserIds = ManualCache.Instance.TryGetOwnerIds(cacheWorkflowIds);
            foreach (var user in sendEmailUsers)
            {
                if (!phUserIds.Contains(user.UserId))
                {
                    phUserIds.Add(user.UserId);
                }
            }
            #endregion

            #region Check whether the data has been archiverd updates the status of the data in RADB
            try
            {
                //检查sql中approved数据，是否被archive
                var approveDatas = ManualApproveDao.GetAllApproveOrRejectedData(SourceFlag.Physical);
                foreach (var approveData in approveDatas)
                {
                    //if (!waitingApproveItems.Any(s => s.NodeID == approveData.NodeId))
                    //{
                    //继续查找 static 表，
                    try
                    {
                        var destoryItem = ManualApprovalService.GetPhysicalRecord(approveData.NodeId);
                        if (destoryItem != null)
                        {
                            if (destoryItem.Status == SOApproveDBStatus.Archived || destoryItem.Status == SOApproveDBStatus.WaitingApprove)
                            {
                                approveData.ActionStatus = (int)Contract.Schedule.ActionStatus.Archiverd;
                                approveData.ActionTime = destoryItem.ArchivedTime;
                                ManualApproveDao.SaveManualApproveForPhysical(approveData);
                                var recordOwners = !string.IsNullOrEmpty(approveData.EscalateTo) ? ManualApprovalService.GesEscalateUsers(approveData.EscalateTo) : "";
                                SendJobReportDetails(approveData, JobDetailsStatus.Successful, recordOwners);
                                hasSucceed = true;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        mLogger.Warn("Import item {0} error {1}", approveData.Url, e.ToString());
                    }
                    //}
                }
            }
            catch (Exception e)
            {
                mLogger.Error("Import error {0}", e.ToString());
            }
            #endregion
        }

        public void ProcessSPLocal(ref bool hasError, ref bool hasSucceed, ref List<string> spLocalUserIds)
        {
            List<UserInfo> sendEmailUsers = new List<UserInfo>();
            IEnumerable<ManualExportReportInfo> reports = null;
            List<RMWebApplication> siteGroupnodes = null;
            var cacheWorkflowIds = new List<Guid>();
            try
            {
                siteGroupnodes = SharePointOnPremClient.GetAllLocalWebApplications();
                reports = ManualApprovalService.GetManualExportReportsForSPOnPrem(fsAzureTableConnectStr, mTenantGroupId);
                var roGroups = ManualApprovalService.GetReportsManagementForSPLocal(reports);
                JobContext.ReportManager.IncreaseBase(reports.Count());
                foreach (var report in reports)
                {
                    try
                    {
                        JobContext.ReportManager.Increase();

                        var settingInfo = GetSettingInfoForSPOnPrem(report);
                        var ruleInfo = GetRuleInfo(report.RuleID);
                        if (ruleInfo == null)
                        {
                            ManualApprovalService.MarkApprovalingObjectsToExportedStatusForSPOnPrem(fsAzureTableConnectStr, mTenantGroupId, report.PartKey, report.RowKey);
                            SendJobReportDetails(report, JobDetailsStatus.Failed, string.Empty, "RM_RDM_Rule_RuleIsDeleted");
                            hasError = true;
                            continue;
                        }

                        var userIds = new List<int>();
                        List<string> ownerNames = new List<string>();

                        if (settingInfo.ApprovalType != DB.Model.ApprovalType.None)
                        {
                            mLogger.Info($"Use settings manual approval config. Approval Type: [{settingInfo.ApprovalType}], Is send email to owner: [{settingInfo.EMailToRecordOwner}].");
                            if (settingInfo.ApprovalType == DB.Model.ApprovalType.ApprovalProcess)
                            {
                                var localRuleInfo = new ManualRuleInfo
                                {
                                    SPLocalWorkflowId = settingInfo.WorkflowReferenceId,
                                    SPLocalIsSendEmailToOwner = settingInfo.EMailToRecordOwner,
                                    SPLocalCriteria = ruleInfo.SPLocalCriteria,
                                    RuleName = ruleInfo.RuleName,
                                };
                                StartWorkflow(report, localRuleInfo, SourceFlag.SharePointOnPrem, ref hasError, ref hasSucceed);
                                var workflowRefrenceId = Guid.Parse(settingInfo.WorkflowReferenceId);
                                if (settingInfo.EMailToRecordOwner && !cacheWorkflowIds.Contains(workflowRefrenceId))
                                {
                                    cacheWorkflowIds.Add(workflowRefrenceId);
                                }
                                continue;
                            }
                            else if (settingInfo.ApprovalType == DB.Model.ApprovalType.RecordOwners)
                            {
                                var group = roGroups.Find(item => item.SPSettingId == settingInfo.Id);
                                var owners = new List<RecordOwnerDto>();
                                if (settingInfo.EMailToRecordOwner)
                                {
                                    AddToEmailUser(sendEmailUsers, out userIds, out owners, group);
                                }
                                else
                                {
                                    userIds = group.Owners.Select(s => s.LnkId).Distinct().ToList();
                                    owners = group.Owners;
                                }
                                ownerNames.AddRange(owners.Select(item => item.DisplayName));
                            }
                        }
                        else
                        {
                            mLogger.Info($"Use rule management manual approval config. Use workflow: [{!string.IsNullOrEmpty(ruleInfo.SPLocalWorkflowId)}], Is send email to owner: [{ruleInfo.SPLocalIsSendEmailToOwner}]");

                            #region Start Workflow logic
                            if (!string.IsNullOrEmpty(ruleInfo.SPLocalWorkflowId))
                            {
                                List<string> recordsOwner = new List<string>();
                                StartWorkflow(report, ruleInfo, SourceFlag.SharePointOnPrem, ref hasError, ref hasSucceed);
                                var workflowRefrenceId = Guid.Parse(ruleInfo.SPLocalWorkflowId);
                                if (ruleInfo.SPLocalIsSendEmailToOwner && !cacheWorkflowIds.Contains(workflowRefrenceId))
                                {
                                    cacheWorkflowIds.Add(workflowRefrenceId);
                                }
                                continue;
                            }
                            #endregion

                            if (ruleInfo.SPLocalIsSendEmailToOwner)
                            {
                                foreach (var user in ruleInfo.SPLocalUsers)
                                {
                                    if (!sendEmailUsers.Any(s => s.UserId == user.UserId))
                                    {
                                        sendEmailUsers.Add(user);
                                    }

                                }

                            }
                            if (ruleInfo.SPLocalUsers != null)
                            {
                                foreach (var user in ruleInfo.SPLocalUsers)
                                {

                                    var dbUser = AccountDao.Find(s => s.UserId == user.UserId && s.IsRemoved == 0);
                                    if (dbUser != null)
                                    {
                                        userIds.Add(dbUser.Id);
                                    }
                                }
                                ownerNames.AddRange(ruleInfo.SPLocalUsers.Select(s => s.DisplayName));
                            }
                        }

                        report.RuleInfo = ruleInfo;
                        if (report.ObjectLevel == RMReportObjectLevel.SiteCollection)
                        {
                            report.ContentType = "RM_JS_Rule_ObjectLevel_SiteCollection";
                        }

                        if (userIds?.Count == 0)
                        {
                            mLogger.Warn("no records owner set,partKey:{0},rowKey:{1}, Rule Id:{2}", report.PartKey, report.RowKey, report.RuleID);
                            SendJobReportDetails(report, JobDetailsStatus.Failed, string.Empty, $"RM_MA_NoRecordOwner{I18NEntity.Separator}{ruleInfo.RuleName}");
                            hasError = true;
                            continue;
                        }

                        var maItem = ConvertReportToEntityForSPOnPrem(report);
                        var escalateTo = string.Empty;
                        userIds = userIds.Distinct().ToList();
                        foreach (var id in userIds)
                        {
                            escalateTo += id + "|";
                        }

                        maItem.EscalateTo = escalateTo;
                        maItem.SourceFlag = (int)SourceFlag.SharePointOnPrem;
                        ManualApproveDao.SaveManualApproveItem(maItem);
                        try
                        {
                            var successCount = ExplorerDao.UpdateRecordOwner(report.SiteID, maItem.NodeId, escalateTo);
                            if (successCount > 0)
                            {
                                mLogger.Info($"success to update records owner:{maItem?.Id}, {escalateTo}");
                            }
                        }
                        catch (Exception ex)
                        {
                            mLogger.Error("update explorer data record owner error:{0}", ex.ToString());
                        }
                        ManualApprovalService.MarkApprovalingObjectsToExportedStatusForSPOnPrem(fsAzureTableConnectStr, mTenantGroupId, report.PartKey, report.RowKey);
                        ownerNames = ownerNames.Distinct().ToList();
                        report.RuleInfo.Criteria = report.RuleInfo?.SPLocalCriteria;
                        SendJobReportDetails(report, JobDetailsStatus.Successful, string.Join(";", ownerNames));
                        hasSucceed = true;
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("error occurred while export waiting for approval data, partkey:{0}, rowKey:{1}, ERROR:{2}", report?.PartKey, report?.RowKey, ex.ToString());
                        SendJobReportDetails(report, JobDetailsStatus.Failed, string.Empty, ex.Message);
                        hasError = true;
                    }


                }
            }
            catch (AzureTableNotExistException ex)
            {
                mLogger.Error("error occurred while get manual approval data,table not exist,ERROR:{0}.", ex.ToString());
                mCommomErrorMessage = I18NEntity.GetString("RM_MA_NoTable");
            }

            #region Send Email

            spLocalUserIds = ManualCache.Instance.TryGetOwnerIds(cacheWorkflowIds);
            foreach (var user in sendEmailUsers)
            {
                if (!spLocalUserIds.Contains(user.UserId))
                {
                    spLocalUserIds.Add(user.UserId);
                }
            }
            #endregion
            #region Check whether the data has been archiverd updates the status of the data in RADB
            try
            {
                //检查sql中approved数据，是否被archive
                var approveDatas = ManualApproveDao.GetAllApproveOrRejectedData(SourceFlag.SharePointOnPrem);
                foreach (var approveData in approveDatas)
                {
                    //if (!reports.Any(s => s.PartKey == approveData.PartKey && s.RowKey == approveData.RowKey))
                    {
                        //继续查找 static 表，
                        try
                        {
                            var destoryItem = ManualApprovalService.GetDestoryItemForSPOnPrem(fsAzureTableConnectStr, mTenantGroupId, approveData.SiteId.ToString(), approveData.NodeId, approveData.Version);
                            if (destoryItem != null)
                            {
                                if (destoryItem.Status == SOApproveDBStatus.Archived || destoryItem.Status == SOApproveDBStatus.Rejected)
                                {
                                    approveData.ActionStatus = (int)Contract.Schedule.ActionStatus.Archiverd;
                                    var aspd = JsonConvert.DeserializeObject<OnPremiseArchiverSharePointDto>(destoryItem.JsonMeta);
                                    approveData.ActionTime = aspd.ArchivedTime.Ticks;
                                    ManualApproveDao.SaveManualApprove(approveData);
                                    var recordOwners = !string.IsNullOrEmpty(approveData.EscalateTo) ? ManualApprovalService.GesEscalateUsers(approveData.EscalateTo) : "";
                                    SendJobReportDetails(approveData, JobDetailsStatus.Successful, recordOwners);
                                    hasSucceed = true;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            mLogger.Warn("Import item {0} error {1}", approveData.Url, e.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Error("Import error {0}", e.ToString());
            }
            #endregion
        }

        public void ProcessOneDrive(ref bool hasError, ref bool hasSucceed, ref List<string> spUserIds)
        {
            List<UserInfo> sendEmailUsers = new List<UserInfo>();
            //向sql db中存入archiver table中MA的数据。
            IEnumerable<ManualExportReportInfo> reports = null;
            var cacheWorkflowIds = new List<Guid>();
            var sourceFlag = SourceFlag.OneDrive;
            try
            {
                //Check db [RMRecordOwners] is empty 
                RMSPTreeNode farmNode = RMSPTreeService.LoadFarm()[0];
                mAllSiteGroupTreeNode = RMSPTreeService.Browse(farmNode, false, RMBrowseTreeNodeSourceType.SkyDrivePro).ToDictionary(t => new Guid(t.Id));
                reports = ManualApprovalService.GetManualExportReports(mAzureTableConnectInfo, mTenantGroupId, sourceFlag);
                var roGroups = ManualApprovalService.GetReportsManagementForOneDrive(reports, ref daoRecordGroupIdMapping, ref daoRecordSiteIdMapping);
                JobContext.ReportManager.IncreaseBase(reports.Count());
                foreach (var report in reports)
                {
                    try
                    {
                        mLogger.Info($"Report info, Site Url: [{report.SiteUrl}], Web Id: [{report.WebID}], List Id: [{report.ListID}]. Level: [{report.ObjectLevel}].");
                        JobContext.ReportManager.Increase();
                        var settingInfo = GetSettingInfoForOneDrive(report);
                        var ruleInfo = GetRuleInfo(report.RuleID);
                        if (ruleInfo == null)
                        {
                            ManualApprovalService.MarkApprovalingObjectsToExportedStatus(mAzureTableConnectInfo, mTenantGroupId, report.PartKey, report.RowKey);
                            SendJobReportDetails(report, JobDetailsStatus.Failed, string.Empty, "RM_RDM_Rule_RuleIsDeleted");
                            hasError = true;
                            continue;
                        }

                        var userIds = new List<int>();
                        List<string> ownerNames = new List<string>();

                        #region local

                        if (settingInfo != null && settingInfo.ApprovalType != DB.Model.ApprovalType.None)
                        {
                            mLogger.Info($"Use settings manual approval config. Approval Type: [{settingInfo.ApprovalType}], Is send email to owner: [{settingInfo.EMailToRecordOwner}].");
                            if (settingInfo.ApprovalType == DB.Model.ApprovalType.ApprovalProcess)
                            {
                                var localRuleInfo = new ManualRuleInfo
                                {
                                    OneDriveWorkflowId = settingInfo.WorkflowReferenceId,
                                    OneDriveIsSendEmailToOwner = settingInfo.EMailToRecordOwner,
                                    OneDriveCriteria = ruleInfo.OneDriveCriteria,
                                    RuleName = ruleInfo.RuleName,
                                };
                                StartWorkflow(report, localRuleInfo, sourceFlag, ref hasError, ref hasSucceed);
                                var workflowRefrenceId = Guid.Parse(settingInfo.WorkflowReferenceId);
                                if (settingInfo.EMailToRecordOwner && !cacheWorkflowIds.Contains(workflowRefrenceId))
                                {
                                    cacheWorkflowIds.Add(workflowRefrenceId);
                                }
                                continue;
                            }
                            else if (settingInfo.ApprovalType == DB.Model.ApprovalType.RecordOwners)
                            {
                                var group = roGroups.Find(item => item.SPSettingId == settingInfo.Id);
                                var owners = new List<RecordOwnerDto>();
                                if (settingInfo.EMailToRecordOwner)
                                {
                                    AddToEmailUser(sendEmailUsers, out userIds, out owners, group);
                                }
                                else
                                {
                                    userIds = group.Owners.Select(s => s.LnkId).Distinct().ToList();
                                    owners = group.Owners;
                                }
                                ownerNames.AddRange(owners.Select(item => item.DisplayName));
                            }
                        }
                        #endregion
                        #region Rule Management
                        else
                        {
                            mLogger.Info($"Use rule management manual approval config. Use workflow: [{!string.IsNullOrEmpty(ruleInfo.OneDriveWorkflowId)}], Is send email to owner: [{ruleInfo.OneDriveIsSendEmailToOwner}]");
                            #region Start Workflow logic
                            if (!string.IsNullOrEmpty(ruleInfo.OneDriveWorkflowId))
                            {
                                List<string> recordsOwner = new List<string>();
                                StartWorkflow(report, ruleInfo, sourceFlag, ref hasError, ref hasSucceed);
                                var workflowRefrenceId = Guid.Parse(ruleInfo.OneDriveWorkflowId);
                                if (ruleInfo.OneDriveIsSendEmailToOwner && !cacheWorkflowIds.Contains(workflowRefrenceId))
                                {
                                    cacheWorkflowIds.Add(workflowRefrenceId);
                                }
                                continue;
                            }
                            #endregion

                            if (ruleInfo.OneDriveIsSendEmailToOwner)
                            {
                                foreach (var user in ruleInfo.OneDriveUsers)
                                {
                                    if (!sendEmailUsers.Any(s => s.UserId == user.UserId))
                                    {
                                        sendEmailUsers.Add(user);
                                    }
                                }
                            }

                            if (ruleInfo.OneDriveUsers != null)
                            {
                                foreach (var user in ruleInfo.OneDriveUsers)
                                {
                                    var dbUser = AccountDao.Find(s => s.UserId == user.UserId && s.IsRemoved == 0);
                                    if (dbUser == null)
                                    {
                                        AccountDao.Create(new RMAccount()
                                        {
                                            DisplayName = user.DisplayName,
                                            UserId = user.UserId,
                                            UserPrincipalName = user.UserPrincipalName,
                                            ObjectType = RMActiveDirectoryObjectType.User
                                        });
                                        userIds.Add(AccountDao.Find(s => s.UserId == user.UserId && s.IsRemoved == 0).Id);
                                    }
                                    else
                                    {
                                        userIds.Add(dbUser.Id);
                                    }
                                }
                                ownerNames.AddRange(ruleInfo.OneDriveUsers.Select(s => s.DisplayName));
                            }
                        }
                        #endregion

                        report.RuleInfo = ruleInfo;
                        if (report.ObjectLevel == RMReportObjectLevel.SiteCollection)
                        {
                            report.ContentType = "RM_JS_Rule_ObjectLevel_SiteCollection";
                        }

                        if (userIds?.Count == 0)
                        {
                            mLogger.Warn("no records owner set,partKey:{0},rowKey:{1}, Rule Id:{2}", report.PartKey, report.RowKey, report.RuleID);
                            SendJobReportDetails(report, JobDetailsStatus.Failed, string.Empty, $"RM_MA_NoRecordOwner{I18NEntity.Separator}{ruleInfo.RuleName}");
                            hasError = true;
                            continue;
                        }

                        var maItem = ConvertReportToEntityForOneDrive(report);
                        var escalateTo = string.Empty;
                        userIds = userIds.Distinct().ToList();
                        foreach (var id in userIds)
                        {
                            escalateTo += id + "|";
                        }

                        maItem.EscalateTo = escalateTo;
                        maItem.SourceFlag = (int)sourceFlag;
                        ManualApproveDao.SaveManualApproveItem(maItem);

                        try
                        {
                            var successCount = ExplorerDao.UpdateRecordOwner(maItem.SiteId, maItem.NodeId, escalateTo);
                            if (successCount > 0)
                            {
                                mLogger.Info($"success to update records owner:{maItem?.Id}, {escalateTo}");
                            }
                        }
                        catch (Exception ex)
                        {
                            mLogger.Error("update explorer data record owner error:{0}", ex.ToString());
                        }

                        ManualApprovalService.MarkApprovalingObjectsToExportedStatus(mAzureTableConnectInfo, mTenantGroupId, report.PartKey, report.RowKey);
                        ownerNames = ownerNames.Distinct().ToList();
                        report.RuleInfo.Criteria = report.RuleInfo?.OneDriveCriteria;
                        SendJobReportDetails(report, JobDetailsStatus.Successful, string.Join(";", ownerNames));
                        hasSucceed = true;
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("error occurred while export waiting for approval data, partkey:{0}, rowKey:{1}, ERROR:{2}", report?.PartKey, report?.RowKey, ex);
                        SendJobReportDetails(report, JobDetailsStatus.Failed, string.Empty, ex.Message);
                        hasError = true;
                    }


                }
            }
            catch (AzureTableNotExistException ex)
            {
                mLogger.Error("error occurred while get manual approval data,table not exist,ERROR:{0}.", ex);
                mCommomErrorMessage = I18NEntity.GetString("RM_MA_NoTable");
            }

            #region Send Email

            spUserIds = ManualCache.Instance.TryGetOwnerIds(cacheWorkflowIds);
            foreach (var user in sendEmailUsers)
            {
                if (!spUserIds.Contains(user.UserId))
                {
                    spUserIds.Add(user.UserId);
                }
            }
            #endregion
            #region Check whether the data has been archiverd updates the status of the data in RADB
            try
            {
                //检查sql中approved数据，是否被archive
                var approveDatas = ManualApproveDao.GetAllApproveOrRejectedData(sourceFlag);
                foreach (var approveData in approveDatas)
                {
                    if (!reports.Any(s => s.PartKey == approveData.PartKey && s.RowKey == approveData.RowKey))
                    {
                        //继续查找 static 表，
                        try
                        {
                            var destoryItem = ManualApprovalService.GetDestoryItem(mAzureTableConnectInfo, mTenantGroupId, approveData.SiteId.ToString(), approveData.NodeId, approveData.Version);
                            if (destoryItem != null)
                            {
                                if (destoryItem.Status == SOApproveDBStatus.Archived || destoryItem.Status == SOApproveDBStatus.Rejected)
                                {
                                    approveData.ActionStatus = (int)Contract.Schedule.ActionStatus.Archiverd;
                                    var aspd = JsonConvert.DeserializeObject<ArchiverSharePointDto>(destoryItem.JsonMeta);
                                    approveData.ActionTime = aspd.ArchivedTime.Ticks;
                                    ManualApproveDao.SaveManualApprove(approveData);
                                    var recordOwners = !string.IsNullOrEmpty(approveData.EscalateTo) ? ManualApprovalService.GesEscalateUsers(approveData.EscalateTo) : "";
                                    SendJobReportDetails(approveData, JobDetailsStatus.Successful, recordOwners);
                                    hasSucceed = true;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            mLogger.Warn("Import item {0} error {1}", approveData.Url, e.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Error("Import error {0}", e.ToString());
            }
            #endregion
        }
        
        /// <summary>
        /// 把workflow第一部 manual approve的user加到发送manual appove timer更新emailitem表中
        /// </summary>
        /// <param name="userIds"></param>
        public void SendEmailForWorkFlowManualItems(List<string> userIds)
        {
            mLogger.Info("Begin send waitting for appove email to workflow users");
            Dictionary<string, List<Guid>> userIdsAndInstanceIds = new Dictionary<string, List<Guid>>();
            userIdsAndInstanceIds = EmailItemDao.GetAllWaittingUserAndInstanceId();
            userIds.AddRange(userIdsAndInstanceIds.Keys);
            if (userIds.Count > 0)
            {
                userIds = userIds.Distinct().ToList();
            }
            List<string> sendSuccessUserId = new List<string>();
            if (userIds.Count == 0)
            {
                mLogger.Info("No user id find to send email for workflow users");
                return;
            }
            var accounts = AccountDao.GetUserByUserIds(userIds);
            foreach (var user in accounts)
            {
                if (user != null)
                {
                    EmailMessageDto emailDto = new EmailMessageDto();
                    emailDto.DetailMap = new Dictionary<string, object>();
                    emailDto.DetailMap.Add(DetailKey.From.ToString(), I18NEntity.GetString("RM_TS_RunSchedule"));
                    emailDto.DetailMap.Add(DetailKey.To.ToString(), user.DisplayName);
                    emailDto.Receivers = user.UserPrincipalName;
                    bool isSendSuccess = MailUtil.SendSyncEmail(emailDto);
                    if (isSendSuccess)
                    {
                        sendSuccessUserId.Add(user.UserId);
                    }
                }
            }
            if (sendSuccessUserId.Count > 0)
            {
                try
                {
                    mLogger.Info("Send email complete and update instance status");
                    List<Guid> changeStatusItems = new List<Guid>();
                    foreach (string userId in sendSuccessUserId)
                    {
                        if (userIdsAndInstanceIds.ContainsKey(userId))
                        {
                            changeStatusItems.AddRange(userIdsAndInstanceIds[userId]);
                        }
                    }
                    EmailItemDao.UpdateWorkflowManualItem(changeStatusItems);
                }
                catch (Exception ex)
                {
                    mLogger.Warn($"An error while update EmailItem table, {ex}");
                }
            }
        }
        public void ProcessEXO(ref bool hasError, ref bool hasSucceed, ref List<string> exoUserIds)
        {
            List<UserInfo> sendEmailUsers = new List<UserInfo>();
            IEnumerable<ManualExportReportInfo> reports = null;
            var cacheWorkflowIds = new List<Guid>();
            try
            {
                RMSampleEXOTreeNode root = RMSPTreeService.LoadExchangeRoot()[0];
                mAllEXOGroupTreeNode = RMSPTreeService.BrowseSampleExchangeTree(root).ToDictionary(t => new Guid(t.Id));
                reports = ManualApprovalService.GetManualExportReportsForEXO(mAzureTableConnectInfo, mTenantGroupId);
                var roGroups = ManualApprovalService.GetReportsManagementForEXO(reports, ref daoRecordMailGroupIdMapping, ref daoRecordMailBoxIdMapping);
                JobContext.ReportManager.IncreaseBase(reports.Count());
                foreach (var report in reports)
                {
                    try
                    {
                        JobContext.ReportManager.Increase();

                        var settingInfo = GetSettingInfoForExchangeOnline(report);
                        var ruleInfo = GetRuleInfo(report.RuleID);
                        if (ruleInfo == null)
                        {
                            ManualApprovalService.MarkApprovalingObjectsToExportedStatus(mAzureTableConnectInfo, mTenantGroupId, report.PartKey, report.RowKey, SourceFlag.Exchange);
                            SendJobReportDetails(report, JobDetailsStatus.Failed, string.Empty, "RM_RDM_Rule_RuleIsDeleted");
                            hasError = true;
                            continue;
                        }

                        var userIds = new List<int>();
                        List<string> ownerNames = new List<string>();

                        if (settingInfo.ApprovalType != DB.Model.ApprovalType.None)
                        {
                            mLogger.Info($"Use settings manual approval config. Approval Type: [{settingInfo.ApprovalType}], Is send email to owner: [{settingInfo.EMailToRecordOwner}].");
                            if (settingInfo.ApprovalType == DB.Model.ApprovalType.ApprovalProcess)
                            {
                                var localRuleInfo = new ManualRuleInfo
                                {
                                    EXOWorkflowId = settingInfo.WorkflowReferenceId,
                                    EXOIsSendEmailToOwner = settingInfo.EMailToRecordOwner,
                                    EXOCriteria = ruleInfo.EXOCriteria,
                                    RuleName = ruleInfo.RuleName,
                                };
                                StartWorkflow(report, localRuleInfo, SourceFlag.Exchange, ref hasError, ref hasSucceed);
                                var workflowRefrenceId = Guid.Parse(settingInfo.WorkflowReferenceId);
                                if (settingInfo.EMailToRecordOwner && !cacheWorkflowIds.Contains(workflowRefrenceId))
                                {
                                    cacheWorkflowIds.Add(workflowRefrenceId);
                                }
                                continue;
                            }
                            else if (settingInfo.ApprovalType == DB.Model.ApprovalType.RecordOwners)
                            {
                                var group = roGroups.Find(item => item.SPSettingId == settingInfo.Id);
                                var owners = new List<RecordOwnerDto>();
                                if (settingInfo.EMailToRecordOwner)
                                {
                                    AddToEmailUser(sendEmailUsers, out userIds, out owners, group);
                                }
                                else
                                {
                                    userIds = group.Owners.Select(s => s.LnkId).Distinct().ToList();
                                    owners = group.Owners;
                                }
                                ownerNames.AddRange(owners.Select(item => item.DisplayName));
                            }
                        }
                        else
                        {
                            mLogger.Info($"Use rule management manual approval config. Use workflow: [{!string.IsNullOrEmpty(ruleInfo.EXOWorkflowId)}], Is send email to owner: [{ruleInfo.EXOIsSendEmailToOwner}]");

                            #region Start Workflow logic
                            if (!string.IsNullOrEmpty(ruleInfo.EXOWorkflowId))
                            {
                                StartWorkflow(report, ruleInfo, SourceFlag.Exchange, ref hasError, ref hasSucceed);
                                var workflowRefrenceId = Guid.Parse(ruleInfo.EXOWorkflowId);
                                if (ruleInfo.EXOIsSendEmailToOwner && !cacheWorkflowIds.Contains(workflowRefrenceId))
                                {
                                    cacheWorkflowIds.Add(workflowRefrenceId);
                                }
                                continue;
                            }
                            #endregion

                            if (ruleInfo.EXOIsSendEmailToOwner)
                            {
                                foreach (var user in ruleInfo.EXOUsers)
                                {
                                    if (!sendEmailUsers.Any(s => s.UserId == user.UserId))
                                    {
                                        sendEmailUsers.Add(user);
                                    }
                                }
                            }

                            if (ruleInfo.EXOUsers != null)
                            {
                                foreach (var user in ruleInfo.EXOUsers)
                                {
                                    var dbUser = AccountDao.Find(s => s.UserId == user.UserId && s.IsRemoved == 0);
                                    if (dbUser == null)
                                    {
                                        AccountDao.Create(new RMAccount()
                                        {
                                            DisplayName = user.DisplayName,
                                            UserId = user.UserId,
                                            UserPrincipalName = user.UserPrincipalName,
                                            ObjectType = RMActiveDirectoryObjectType.User
                                        });
                                        userIds.Add(AccountDao.Find(s => s.UserId == user.UserId && s.IsRemoved == 0).Id);
                                    }
                                    else
                                    {
                                        userIds.Add(dbUser.Id);
                                    }
                                }
                                ownerNames.AddRange(ruleInfo.EXOUsers.Select(s => s.DisplayName));
                            }
                        }

                        report.RuleInfo = ruleInfo;
                        if (report.ObjectLevel == RMReportObjectLevel.ExchangeOnlineItem)
                        {
                            //report.LeafName = string.Empty;
                            report.ContentType = "RM_JS_Rule_ObjectLevel_ExchangeOnlineItem";
                        }

                        if (userIds?.Count == 0)
                        {
                            mLogger.Warn("no records owner set,partKey:{0},rowKey:{1}, Rule Id:{2}", report.PartKey, report.RowKey, report.RuleID);
                            SendJobReportDetails(report, JobDetailsStatus.Failed, string.Empty, $"RM_MA_HaveNotRecordOwner{I18NEntity.Separator}{ruleInfo.RuleName}");
                            hasError = true;
                            continue;
                        }

                        var maItem = ConvertReportToEntityForEXO(report);
                        var escalateTo = string.Empty;
                        userIds = userIds.Distinct().ToList();
                        foreach (var id in userIds)
                        {
                            escalateTo += id + "|";
                        }

                        maItem.EscalateTo = escalateTo;
                        maItem.SourceFlag = (int)SourceFlag.Exchange;
                        ManualApproveDao.SaveManualApproveItem(maItem);
                        try
                        {
                            var successCount = ExplorerDao.UpdateRecordOwner(maItem.SiteId, maItem.NodeId, escalateTo);
                            if (successCount > 0)
                            {
                                mLogger.Info($"success to update records owner:{maItem?.Id}, {escalateTo}");
                            }
                        }
                        catch (Exception ex)
                        {
                            mLogger.Error("update explorer data record owner error:{0}", ex.ToString());
                        }
                        ManualApprovalService.MarkApprovalingObjectsToExportedStatus(mAzureTableConnectInfo, mTenantGroupId, report.PartKey, report.RowKey, SourceFlag.Exchange);
                        ownerNames = ownerNames.Distinct().ToList();
                        //Save Rule EXOCriteria info to Criteria for Job Detail.
                        if (report.RuleInfo != null)
                        {
                            report.RuleInfo.Criteria = report.RuleInfo.EXOCriteria;
                        }
                        SendJobReportDetails(report, JobDetailsStatus.Successful, string.Join(";", ownerNames));
                        hasSucceed = true;
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("error occurred while export waiting for approval data, partkey:{0}, rowKey:{1}, ERROR:{2}", report?.PartKey, report?.RowKey, ex.ToString());
                        SendJobReportDetails(report, JobDetailsStatus.Failed, string.Empty, ex.Message);
                        hasError = true;
                    }

                }
            }
            //catch (JobStopException ex)
            //{
            //    throw new JobStopException("This Job is stopped.");
            //}
            catch (AzureTableNotExistException ex)
            {
                mLogger.Error("error occurred while get manual approval data,table not exist,ERROR:{0}.", ex.ToString());
                mCommomErrorMessage = I18NEntity.GetString("RM_MA_NoTable");
            }

            #region Send Email
            // TODO sendEmailUsers
            //if (sendEmailUsers != null && sendEmailUsers.Count > 0)
            //{
            //    foreach (var user in sendEmailUsers)
            //    {
            //        EmailMessageDto emailDto = new EmailMessageDto();
            //        emailDto.DetailMap = new Dictionary<string, object>();
            //        emailDto.DetailMap.Add(DetailKey.To.ToString(), user.DisplayName);
            //        emailDto.Receivers = user.UserPrincipalName;
            //        emailDto.JobId = mJobInfo.Id;
            //        MailUtil.SendSyncEmail(emailDto);
            //    }
            //}

            exoUserIds = ManualCache.Instance.TryGetOwnerIds(cacheWorkflowIds); ;
            foreach (var user in sendEmailUsers)
            {
                if (!exoUserIds.Contains(user.UserId))
                {
                    exoUserIds.Add(user.UserId);
                }
            }
            #endregion

            #region Check whether the data has been archiverd updates the status of the data in RADB
            try
            {
                //检查sql中approved数据，是否被archive
                var approveDatas = ManualApproveDao.GetAllApproveOrRejectedData(SourceFlag.Exchange);
                foreach (var approveData in approveDatas)
                {
                    if (!reports.Any(s => s.PartKey == approveData.PartKey && s.RowKey == approveData.RowKey))
                    {
                        //继续查找 static 表，
                        try
                        {
                            var destoryItem = ManualApprovalService.GetDestoryItemForEXO(mAzureTableConnectInfo, mTenantGroupId, approveData.PartKey, approveData.RowKey);
                            if (destoryItem != null)
                            {
                                if (destoryItem.Status == SOApproveDBStatus.Archived || destoryItem.Status == SOApproveDBStatus.Rejected)
                                {
                                    approveData.ActionStatus = (int)Contract.Schedule.ActionStatus.Archiverd;
                                    approveData.ActionTime = destoryItem.ArchivedTime;
                                    ManualApproveDao.SaveManualApprove(approveData);
                                    var recordOwners = !string.IsNullOrEmpty(approveData.EscalateTo) ? ManualApprovalService.GesEscalateUsers(approveData.EscalateTo) : "";
                                    SendJobReportDetails(approveData, JobDetailsStatus.Successful, recordOwners);
                                    hasSucceed = true;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            mLogger.Warn("Import item {0} error {1}", approveData.Url, e.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Error("Import error {0}", e.ToString());
            }
            #endregion
        }

        private RMSharePointOnPremiseSetting GetSettingInfoForSPOnPrem(ManualExportReportInfo reportInfo)
        {
            var needGetParentLevelSettingInfo = false;

            var siteUrl = reportInfo.SiteUrl;

            if (reportInfo.ObjectLevel != RMReportObjectLevel.SiteCollection && reportInfo.ObjectLevel != RMReportObjectLevel.Site && reportInfo.ObjectLevel != RMReportObjectLevel.List)
            {
                var folderId = reportInfo.ObjectLevel == RMReportObjectLevel.Folder ? reportInfo.NodeID : reportInfo.ParentID;
                var settingInfo = SharePointOnPremiseSettingDao.Find(item => item.ScopeId == folderId);
                if (settingInfo != null)
                {
                    return settingInfo;
                }

                if (!string.IsNullOrEmpty(reportInfo.ServerRelativeUrl) && !reportInfo.ServerRelativeUrl.StartsWith("/"))
                {
                    reportInfo.ServerRelativeUrl = "/" + reportInfo.ServerRelativeUrl;
                }
                var folderFullPath = reportInfo.Path.Substring(0, reportInfo.Path.LastIndexOf("/"));
                mLogger.Info("ma process folder full path :{0}", folderFullPath);

                var parentFolderIds = SharePointOnPremClient.GetRootParentIdsFromFolder(siteUrl, reportInfo.WebID.ToString(), reportInfo.ListID.ToString(), folderFullPath);
                foreach (var parentId in parentFolderIds)
                {
                    settingInfo = SharePointOnPremiseSettingDao.Find(item => item.ScopeId == new Guid(parentId));
                    if (settingInfo != null)
                    {
                        return settingInfo;
                    }
                }

                needGetParentLevelSettingInfo = true;
            }

            if (needGetParentLevelSettingInfo || reportInfo.ObjectLevel == RMReportObjectLevel.List)
            {
                var settingInfo = SharePointOnPremiseSettingDao.Find(item => item.ScopeId == reportInfo.ListID);
                if (settingInfo != null)
                {
                    return settingInfo;
                }

                needGetParentLevelSettingInfo = true;
            }

            if (needGetParentLevelSettingInfo || reportInfo.ObjectLevel == RMReportObjectLevel.Site)
            {
                var parentWebIds = SharePointOnPremClient.GetRootParentIdsFromWeb(siteUrl, reportInfo.WebID.ToString());
                foreach (var parentId in parentWebIds)
                {
                    var settingInfo = SharePointOnPremiseSettingDao.Find(item => item.ScopeId == new Guid(parentId));
                    if (settingInfo != null)
                    {
                        return settingInfo;
                    }
                }

                needGetParentLevelSettingInfo = true;
            }

            if (needGetParentLevelSettingInfo || reportInfo.ObjectLevel == RMReportObjectLevel.SiteCollection)
            {
                var setttingInfo = SharePointOnPremiseSettingDao.Find(item => item.ScopeId == reportInfo.RegistedSiteId);
                if (setttingInfo != null)
                {
                    return setttingInfo;
                }
            }

            return SharePointOnPremiseSettingDao.Find(item => item.ScopeId == reportInfo.SiteGroupID);
        }

        private RMFileSystemSetting GetSettingInfoForFileSystem(ManualExportReportInfo reportInfo)
        {
            return FileSystemSettingDao.Find(item => item.ScopeId == new Guid(reportInfo.ScopeID));
        }

        private RMPhysicalRecordSetting GetSettingInfoForPhysicalRecords(ManualExportReportInfo reportInfo)
        {
            return PhysicalRecordSettingDao.Find(item => item.LocationUniqueId == reportInfo.TopLocationID);
        }

        private RMExchangeOnlineSetting GetSettingInfoForExchangeOnline(ManualExportReportInfo reportInfo)
        {
            if (daoRecordMailBoxIdMapping.TryGetValue(reportInfo.MailBoxID, out var mailboxId))
            {
                var mailboxSettingInfo = EXOSettingDao.Find(item => item.ScopeId == mailboxId);
                if (mailboxSettingInfo != null)
                {
                    return mailboxSettingInfo;
                }
            }

            if (daoRecordMailGroupIdMapping.TryGetValue(reportInfo.SiteGroupID, out var mailGroupId))
            {
                var mailboxGroupSettingInfo = EXOSettingDao.Find(item => item.ScopeId == mailGroupId);
                if (mailboxGroupSettingInfo != null)
                {
                    return mailboxGroupSettingInfo;
                }
            }
            return null;
        }

        private RMSharePointSetting GetSettingInfoForSharePointOnline(ManualExportReportInfo reportInfo)
        {
            var needGetParentLevelSettingInfo = false;

            var site = GetAveSite(reportInfo);


            // folder 和 folder 以下級別
            if (reportInfo.ObjectLevel != RMReportObjectLevel.SiteCollection && reportInfo.ObjectLevel != RMReportObjectLevel.Site && reportInfo.ObjectLevel != RMReportObjectLevel.List)
            {
                if (!string.IsNullOrEmpty(reportInfo.ServerRelativeUrl) && !reportInfo.ServerRelativeUrl.StartsWith("/"))
                {
                    reportInfo.ServerRelativeUrl = "/" + reportInfo.ServerRelativeUrl;
                }
                var folderServerRelativeUrl = reportInfo.ServerRelativeUrl.Contains("\\") ? reportInfo.ServerRelativeUrl.Substring(0, reportInfo.ServerRelativeUrl.IndexOf("\\")) : reportInfo.ServerRelativeUrl;
                var web = site.OpenWeb(reportInfo.WebID);
                var list = web.GetList(reportInfo.ListID);
                var folder = list.GetFolder(folderServerRelativeUrl);
                var folderRootId = list.RootFolder.UniqueId;
                while (folder.UniqueId != Guid.Empty && folder.UniqueId != folderRootId)
                {
                    var folderSettingInfo = SPSettingDao.Find(item => item.ScopeId == folder.UniqueId);
                    if (folderSettingInfo != null)
                    {
                        return folderSettingInfo;
                    }
                    folder = folder.ParentFolder;
                }

                needGetParentLevelSettingInfo = true;
            }

            if (needGetParentLevelSettingInfo || reportInfo.ObjectLevel == RMReportObjectLevel.List)
            {
                if (daoRecordSiteIdMapping.TryGetValue(reportInfo.RegistedSiteId, out var siteId) && daoRecordGroupIdMapping.TryGetValue(reportInfo.SiteGroupID, out var recordGroupId))
                {
                    var listSettingInfo = SPSettingDao.Find(item => item.SiteGroupId == recordGroupId && item.SiteId == siteId && item.WebId == reportInfo.WebID && item.ScopeId == reportInfo.ListID);
                    if (listSettingInfo != null)
                    {
                        return listSettingInfo;
                    }
                }
                needGetParentLevelSettingInfo = true;
            }

            if (needGetParentLevelSettingInfo || reportInfo.ObjectLevel == RMReportObjectLevel.Site)
            { 
                if(daoRecordSiteIdMapping.TryGetValue(reportInfo.RegistedSiteId, out var siteId) && daoRecordGroupIdMapping.TryGetValue(reportInfo.SiteGroupID, out var recordGroupId))
                {
                    var web = site.OpenWeb(reportInfo.WebID);
                    var tempWeb = web;
                    var rootWebId = site.RootWeb.ID;

                    RMSharePointSetting webSettingInfo = null;

                    while (tempWeb.ID != Guid.Empty && tempWeb.ID != rootWebId)
                    {
                        webSettingInfo = SPSettingDao.Find(item => item.SiteGroupId == recordGroupId && item.SiteId == siteId && item.ScopeId == tempWeb.ID);
                        if (webSettingInfo != null)
                        {
                            return webSettingInfo;
                        }

                        tempWeb = tempWeb.ParentWeb;
                    }

                    webSettingInfo = SPSettingDao.Find(item => item.SiteGroupId == recordGroupId && item.SiteId == siteId && item.ScopeId == rootWebId);
                    if(webSettingInfo != null)
                    {
                        return webSettingInfo;
                    }
                } 
                needGetParentLevelSettingInfo = true;
            }

            if (needGetParentLevelSettingInfo || reportInfo.ObjectLevel == RMReportObjectLevel.SiteCollection)
            {
                if (daoRecordSiteIdMapping.TryGetValue(reportInfo.RegistedSiteId, out var siteId) && daoRecordGroupIdMapping.TryGetValue(reportInfo.SiteGroupID, out var recordGroupId))
                {
                    var siteSettingInfo = SPSettingDao.Find(item => item.SiteGroupId == recordGroupId && item.ScopeId == siteId);
                    if (siteSettingInfo != null)
                    {
                        return siteSettingInfo;
                    }
                }
            }

            if (daoRecordGroupIdMapping.TryGetValue(reportInfo.SiteGroupID, out var groupId))
            {
                return SPSettingDao.Find(item => item.ScopeId == groupId);
            }
            return null;
        }

        private RMOneDriveSetting GetSettingInfoForOneDrive(ManualExportReportInfo reportInfo)
        {
            var needGetParentLevelSettingInfo = false;

            var site = GetAveSite(reportInfo);

            // folder 和 folder 以下級別
            if (reportInfo.ObjectLevel != RMReportObjectLevel.SiteCollection && reportInfo.ObjectLevel != RMReportObjectLevel.Site && reportInfo.ObjectLevel != RMReportObjectLevel.List)
            {
                if (!string.IsNullOrEmpty(reportInfo.ServerRelativeUrl) && !reportInfo.ServerRelativeUrl.StartsWith("/"))
                {
                    reportInfo.ServerRelativeUrl = "/" + reportInfo.ServerRelativeUrl;
                }
                var folderServerRelativeUrl = reportInfo.ServerRelativeUrl.Contains("\\") ? reportInfo.ServerRelativeUrl.Substring(0, reportInfo.ServerRelativeUrl.IndexOf("\\")) : reportInfo.ServerRelativeUrl;
                var web = site.OpenWeb(reportInfo.WebID);
                var list = web.GetList(reportInfo.ListID);
                var folder = list.GetFolder(folderServerRelativeUrl);
                var folderRootId = list.RootFolder.UniqueId;
                while (folder.UniqueId != Guid.Empty && folder.UniqueId != folderRootId)
                {
                    var folderSettingInfo = OneDriveSettingDao.Find(item => item.ScopeId == folder.UniqueId);
                    if (folderSettingInfo != null)
                    {
                        return folderSettingInfo;
                    }
                    folder = folder.ParentFolder;
                }

                needGetParentLevelSettingInfo = true;
            }

            if (needGetParentLevelSettingInfo || reportInfo.ObjectLevel == RMReportObjectLevel.List)
            {
                if (daoRecordSiteIdMapping.TryGetValue(reportInfo.RegistedSiteId, out var siteId) && daoRecordGroupIdMapping.TryGetValue(reportInfo.SiteGroupID, out var recordGroupId))
                {
                    var listSettingInfo = OneDriveSettingDao.Find(item => item.SiteGroupId == recordGroupId && item.SiteId == siteId && item.WebId == reportInfo.WebID && item.ScopeId == reportInfo.ListID);
                    if (listSettingInfo != null)
                    {
                        return listSettingInfo;
                    }
                }
                needGetParentLevelSettingInfo = true;
            }

            if (needGetParentLevelSettingInfo || reportInfo.ObjectLevel == RMReportObjectLevel.Site)
            {
                if (daoRecordSiteIdMapping.TryGetValue(reportInfo.RegistedSiteId, out var siteId) && daoRecordGroupIdMapping.TryGetValue(reportInfo.SiteGroupID, out var recordGroupId))
                {
                    var web = site.OpenWeb(reportInfo.WebID);
                    var tempWeb = web;
                    var rootWebId = site.RootWeb.ID;

                    RMOneDriveSetting webSettingInfo = null;

                    while (tempWeb.ID != Guid.Empty && tempWeb.ID != rootWebId)
                    {
                        webSettingInfo = OneDriveSettingDao.Find(item => item.SiteGroupId == recordGroupId && item.SiteId == siteId && item.ScopeId == tempWeb.ID);
                        if (webSettingInfo != null)
                        {
                            return webSettingInfo;
                        }

                        tempWeb = tempWeb.ParentWeb;
                    }

                    webSettingInfo = OneDriveSettingDao.Find(item => item.SiteGroupId == recordGroupId && item.SiteId == siteId && item.ScopeId == rootWebId);
                    if (webSettingInfo != null)
                    {
                        return webSettingInfo;
                    }
                }
                needGetParentLevelSettingInfo = true;
            }

            if (needGetParentLevelSettingInfo || reportInfo.ObjectLevel == RMReportObjectLevel.SiteCollection)
            {
                if (daoRecordSiteIdMapping.TryGetValue(reportInfo.RegistedSiteId, out var siteId) && daoRecordGroupIdMapping.TryGetValue(reportInfo.SiteGroupID, out var recordGroupId))
                {
                    var siteSettingInfo = OneDriveSettingDao.Find(item => item.SiteGroupId == recordGroupId && item.ScopeId == siteId);
                    if (siteSettingInfo != null)
                    {
                        return siteSettingInfo;
                    }
                }
            }

            if (daoRecordGroupIdMapping.TryGetValue(reportInfo.SiteGroupID, out var groupId))
            {
                return OneDriveSettingDao.Find(item => item.ScopeId == groupId);
            }
            return null;
        }

        private void StartWorkflow(ManualExportReportInfo waitingData, ManualRuleInfo rule, SourceFlag source, ref bool hasError, ref bool hasSucceed)
        {
            try
            {
                RMManualApprove manualItem = null;
                waitingData.RuleInfo = rule;
                List<AccountDto> recordOwners = new List<AccountDto>();
                switch (source)
                {
                    case SourceFlag.SharePoint:
                        if (waitingData.ObjectLevel == RMReportObjectLevel.SiteCollection)
                        {
                            waitingData.ContentType = "RM_JS_Rule_ObjectLevel_SiteCollection";
                        }
                        manualItem = ConvertReportToEntity(waitingData);
                        manualItem.SourceFlag = (int)source;
                        //创建Workflow instance
                        manualItem.WorkflowInstanceId = CreateWorkflowInstance(waitingData, rule.WorkflowId, rule.IsSendEmailToOwner);
                        //SPO保存数据到ManualApprove表
                        ManualApproveDao.SaveManualApproveItem(manualItem);
                        //更新数据导出状态为True
                        ManualApprovalService.MarkApprovalingObjectsToExportedStatus(mAzureTableConnectInfo, mTenantGroupId, waitingData.PartKey, waitingData.RowKey);

                        recordOwners = ManualCache.Instance.TryGetWorkflowOwner(Guid.Parse(rule.WorkflowId));
                        UpdateRecordOwner(manualItem, recordOwners);
                        break;
                    case SourceFlag.OneDrive:
                        if (waitingData.ObjectLevel == RMReportObjectLevel.SiteCollection)
                        {
                            waitingData.ContentType = "RM_JS_Rule_ObjectLevel_SiteCollection";
                        }
                        manualItem = ConvertReportToEntityForOneDrive(waitingData);
                        manualItem.SourceFlag = (int)source;
                        //创建Workflow instance
                        manualItem.WorkflowInstanceId = CreateWorkflowInstance(waitingData, rule.OneDriveWorkflowId, rule.OneDriveIsSendEmailToOwner);
                        //SPO保存数据到ManualApprove表
                        ManualApproveDao.SaveManualApproveItem(manualItem);
                        //更新数据导出状态为True
                        ManualApprovalService.MarkApprovalingObjectsToExportedStatus(mAzureTableConnectInfo, mTenantGroupId, waitingData.PartKey, waitingData.RowKey);
                        waitingData.RuleInfo.Criteria = rule.OneDriveCriteria;
                        recordOwners = ManualCache.Instance.TryGetWorkflowOwner(Guid.Parse(rule.OneDriveWorkflowId));
                        UpdateRecordOwner(manualItem, recordOwners);
                        break;
                    case SourceFlag.SharePointOnPrem:
                        manualItem = ConvertReportToEntityForSPOnPrem(waitingData);
                        manualItem.SourceFlag = (int)source;
                        manualItem.WorkflowInstanceId = CreateWorkflowInstance(waitingData, rule.SPLocalWorkflowId, rule.SPLocalIsSendEmailToOwner);
                        ManualApproveDao.SaveManualApproveItem(manualItem);
                        ManualApprovalService.MarkApprovalingObjectsToExportedStatusForSPOnPrem(fsAzureTableConnectStr, mTenantGroupId, waitingData.PartKey, waitingData.RowKey);
                        waitingData.RuleInfo.Criteria = waitingData.RuleInfo.SPLocalCriteria;
                        recordOwners = ManualCache.Instance.TryGetWorkflowOwner(Guid.Parse(rule.SPLocalWorkflowId));
                        UpdateRecordOwner(manualItem, recordOwners);
                        break;
                    case SourceFlag.Physical:
                        manualItem = ConvertReportToEntityForPhysical(waitingData);
                        manualItem.SourceFlag = (int)SourceFlag.Physical;
                        manualItem.WorkflowInstanceId = CreateWorkflowInstance(waitingData, rule.PhyWorkflowId, rule.PhysicalIsSendEmailToOwner);
                        //Physical保存数据到ManualApprove表
                        ManualApproveDao.SaveManualApproveForPhysical(manualItem);
                        //更新数据导出状态为True
                        ManualApprovalService.MarkToExportedStatusForPhysical(manualItem.NodeId);
                        waitingData.RuleInfo.Criteria = rule.PhysicalCriteria;
                        recordOwners = ManualCache.Instance.TryGetWorkflowOwner(Guid.Parse(rule.PhyWorkflowId));
                        ExplorerDao.UpdateRecordOwnerForPhysical(waitingData.NodeID, string.Join("|", recordOwners.Select(u => u.Id).Distinct().ToList()));
                        break;
                    case SourceFlag.Exchange:
                        if (waitingData.ObjectLevel == RMReportObjectLevel.ExchangeOnlineItem)
                        {
                            waitingData.ContentType = "RM_JS_Rule_ObjectLevel_ExchangeOnlineItem";
                        }
                        manualItem = ConvertReportToEntityForEXO(waitingData);
                        manualItem.SourceFlag = (int)SourceFlag.Exchange;
                        manualItem.WorkflowInstanceId = CreateWorkflowInstance(waitingData, rule.EXOWorkflowId, rule.EXOIsSendEmailToOwner);
                        //EXO保存数据到ManualApprove表
                        ManualApproveDao.SaveManualApproveItem(manualItem);
                        //更新数据导出状态为True
                        ManualApprovalService.MarkApprovalingObjectsToExportedStatus(mAzureTableConnectInfo, mTenantGroupId, waitingData.PartKey, waitingData.RowKey, SourceFlag.Exchange);
                        waitingData.RuleInfo.Criteria = waitingData.RuleInfo.EXOCriteria;

                        recordOwners = ManualCache.Instance.TryGetWorkflowOwner(Guid.Parse(rule.EXOWorkflowId));
                        UpdateRecordOwner(manualItem, recordOwners);
                        break;
                    case SourceFlag.FileSystem:
                        manualItem = ConvertReportToEntityForFS(waitingData);
                        manualItem.SourceFlag = (int)SourceFlag.FileSystem;
                        manualItem.WorkflowInstanceId = CreateWorkflowInstance(waitingData, rule.FSWorkflowId, rule.FSIsSendEmailToOwner);
                        //FS保存数据到ManualApprove表
                        ManualApproveDao.SaveManualApproveForFS(manualItem);
                        //更新数据导出状态为True
                        ManualApprovalService.MarkApprovalingObjectsToExportedStatusForFS(fsAzureTableConnectStr, mTenantGroupId, waitingData.PartKey, waitingData.RowKey);
                        waitingData.RuleInfo.Criteria = waitingData.RuleInfo.FSCriteria;

                        recordOwners = ManualCache.Instance.TryGetWorkflowOwner(Guid.Parse(rule.FSWorkflowId));
                        ExplorerDao.UpdateRecordOwnerForPhysical(waitingData.NodeID, string.Join("|", recordOwners.Select(u => u.Id).Distinct().ToList()));
                        break;
                }
                SendJobReportDetails(waitingData, JobDetailsStatus.Successful, string.Join(";", recordOwners.Select(o => o.DisplayName).Distinct()));
                hasSucceed = true;
            }
            catch (Exception ex)
            {
                mLogger.Error($"error occurred while export waiting for approval data, NodeId:{waitingData.NodeID}, ERROR:{ex.ToString()}");
                SendJobReportDetails(waitingData, JobDetailsStatus.Failed, "", ex.Message);
                hasError = true;
            }
        }

        private void UpdateRecordOwner(RMManualApprove manualItem, List<AccountDto> recordOwners)
        {
            try
            {
                ExplorerDao.UpdateRecordOwner(manualItem.SiteId, manualItem.NodeId, string.Join("|", recordOwners.Select(u => u.Id).Distinct().ToList()));
            }
            catch (Exception ex)
            {
                mLogger.Error($"An error when update record owner for cosmosdb, id:{manualItem.RowKey}, id:{manualItem?.Id}, message:{ex}");
            }
        }

        private List<string> GetReviewUserById(Guid instanceId)
        {
            try
            {
                var userIds = WorkflowInstanceDao.GetReviewUserIdsByWFInstanceId(instanceId);
                var users = AccountDao.GetUserByUserIds(userIds);
                return users.Select(u => u.DisplayName).ToList();
            }
            catch (Exception ex)
            {
                mLogger.Error($"get record owner error:{ex.ToString()}");
            }
            return new List<string>();

        }

        private Guid CreateWorkflowInstance(ManualExportReportInfo manualData, string workflowReferenceId, bool isSendEmailToReviewers)
        {
            var workflowInfo = ManualProcessManagementService.GetWorkflow(Guid.Parse(workflowReferenceId));
            var request = new DisposalReviewRequestInfo()
            {
                RequestId = manualData.NodeID,
                DefinitionId = workflowInfo.Id,
                IsSendEmail = isSendEmailToReviewers,
                ActionBy = "RM_TS_RunSchedule"
            };
            return DisposalReviewWFService.StartWorkflow(request, workflowInfo.XamlStr);
        }

        //private string GetWorkflowXamlInfo(string id)
        //{
        //    var workflowXaml = "";
        //    if (!workflowXamlDic.ContainsKey(id))
        //    {
        //        var workflowDto = ManualProcessManagementService.LoadProcess(new Guid(id));
        //        workflowXaml = XamlBuilder.BuildXaml(workflowDto);
        //        workflowXamlDic[id] = workflowXaml;
        //    }
        //    else
        //    {
        //        workflowXaml = workflowXamlDic[id];
        //    }
        //    return workflowXaml;
        //}

        private void GetRecordOwnerFromSPS(List<UserInfo> sendEmailUsers, List<RecordOwnerGroupDto> roGroups, ManualExportReportInfo report, out List<int> ownerIds, out List<RecordOwnerDto> owners)
        {
            ownerIds = null;
            owners = null;
            bool isNotGetContainerLevel = false;

            //Folder和Folder级别以下的

            if (report.ObjectLevel != RMReportObjectLevel.SiteCollection && report.ObjectLevel != RMReportObjectLevel.Site && report.ObjectLevel != RMReportObjectLevel.List)
            {
                isNotGetContainerLevel = true;//用来标识需要获取的是否为Container级别的Owner
                //Folder级别ID是自身ID，Folder以下级别ID是parentID
                var folderId = report.ObjectLevel == RMReportObjectLevel.Folder ? report.NodeID : report.ParentID;
                if (Guid.Empty != folderId)
                {
                    var group = ProcessReportInFolderLevel(folderId, roGroups);
                    if (group != null)
                    {
                        AddToEmailUser(sendEmailUsers, out ownerIds, out owners, group);
                        return;
                    }
                    else
                    {
                        var tempSite = GetAveSite(report);
                        if (tempSite != null)
                        {
                            var tempWeb = tempSite.OpenWeb(report.WebID);
                            var tempList = tempWeb.GetList(report.ListID);
                            try
                            {
                                if (!string.IsNullOrEmpty(report.ServerRelativeUrl) && !report.ServerRelativeUrl.StartsWith("/"))
                                {
                                    report.ServerRelativeUrl = "/" + report.ServerRelativeUrl;
                                }
                                mLogger.Info("ma process folder report.ServerRelativeUrl:{0} ", report.ServerRelativeUrl);
                                var folderServerRelativeUrl = report.ServerRelativeUrl.Contains("\\") ? report.ServerRelativeUrl.Substring(0, report.ServerRelativeUrl.IndexOf("\\")) : report.ServerRelativeUrl;
                                mLogger.Info("ma process folder folderServerRelativeUrl:{0}", folderServerRelativeUrl);
                                bool isRootFolder = report.ObjectLevel == RMReportObjectLevel.Folder && tempList.RootFolder.ServerRelativeUrl.Equals(folderServerRelativeUrl, StringComparison.OrdinalIgnoreCase);
                                if (!isRootFolder)
                                {
                                    if (report.ObjectLevel == RMReportObjectLevel.Folder)
                                    {
                                        group = ProcessReportInFolderLevel(report.ParentID, roGroups);
                                        if (group != null)
                                        {
                                            AddToEmailUser(sendEmailUsers, out ownerIds, out owners, group);
                                            return;
                                        }
                                    }

                                    var tempFolder = tempList.GetFolder(folderServerRelativeUrl);//TODO report.Path = "/sites/linw01/AIRM\\123.docx"
                                    while (tempFolder.UniqueId != tempList.RootFolder.UniqueId && tempFolder.UniqueId != Guid.Empty)
                                    {
                                        group = ProcessReportInFolderLevel(tempFolder.UniqueId, roGroups);
                                        if (group != null)
                                        {
                                            AddToEmailUser(sendEmailUsers, out ownerIds, out owners, group);
                                            return;
                                        }
                                        else
                                        {
                                            tempFolder = tempFolder.ParentFolder;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                mLogger.Error("Somthing went wrong, report item path: {0}, error msg: {1}.", report.Path, ex.ToString());
                                return;
                            }
                        }

                    }
                }
            }


            //List和List级别以下的
            if (isNotGetContainerLevel || (isNotGetContainerLevel = report.ObjectLevel == RMReportObjectLevel.List))
            //if (report.ObjectLevel != RMReportObjectLevel.SiteCollection && report.ObjectLevel != RMReportObjectLevel.Site)
            {
                //isNotGetContainerLevel = true;
                if (daoRecordSiteIdMapping.ContainsKey(report.RegistedSiteId))
                {
                    var recordSiteNodeId = daoRecordSiteIdMapping[report.RegistedSiteId];
                    var group = roGroups.Where(g => g.ListId == report.ListID && g.SiteId == recordSiteNodeId && g.ScopeId == g.ListId).FirstOrDefault();
                    if (group != null)
                    {
                        AddToEmailUser(sendEmailUsers, out ownerIds, out owners, group);
                        return;
                    }
                }
            }
            if (isNotGetContainerLevel || (isNotGetContainerLevel = report.ObjectLevel == RMReportObjectLevel.Site))
            {
                var group = ProcessReportInWebLevel(report.WebID, roGroups);
                if (group != null)
                {
                    AddToEmailUser(sendEmailUsers, out ownerIds, out owners, group);
                    return;
                }
                else
                {
                    try
                    {
                        bool isRootWeb = report.ObjectLevel == RMReportObjectLevel.Site && report.SiteUrl.Equals(report.Path, StringComparison.OrdinalIgnoreCase);
                        if (!isRootWeb)
                        {
                            Guid tempWebId = report.WebID;
                            if (report.ObjectLevel == RMReportObjectLevel.Site)
                            {
                                tempWebId = report.ParentID;
                                group = ProcessReportInWebLevel(tempWebId, roGroups);
                                if (group != null)
                                {
                                    AddToEmailUser(sendEmailUsers, out ownerIds, out owners, group);
                                    return;
                                }
                            }
                            else
                            {
                                var site = GetAveSite(report);
                                if (site != null)
                                {
                                    var tempWeb = site.OpenWeb(tempWebId);
                                    while (!tempWeb.IsRootWeb)
                                    {
                                        tempWebId = tempWeb.ParentWebId;
                                        group = ProcessReportInWebLevel(tempWebId, roGroups);
                                        if (group != null)
                                        {
                                            AddToEmailUser(sendEmailUsers, out ownerIds, out owners, group);
                                            return;
                                        }
                                        else
                                        {
                                            tempWeb = site.OpenWeb(tempWebId);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("Somthing went wrong, report item path: {0}, error msg: {1}.", report.Path, ex.ToString());
                        return;
                    }
                }
            }
            if (isNotGetContainerLevel || (isNotGetContainerLevel = report.ObjectLevel == RMReportObjectLevel.SiteCollection))
            {
                if (daoRecordSiteIdMapping.ContainsKey(report.RegistedSiteId))
                {
                    var recordSiteNodeId = daoRecordSiteIdMapping[report.RegistedSiteId];
                    var group = roGroups.Where(g => g.SiteId == recordSiteNodeId && g.ScopeId == g.SiteId).FirstOrDefault();
                    if (group != null)
                    {
                        AddToEmailUser(sendEmailUsers, out ownerIds, out owners, group);
                        return;
                    }
                }
            }
            if (daoRecordGroupIdMapping.ContainsKey(report.SiteGroupID))
            {
                var recordGroupId = daoRecordGroupIdMapping[report.SiteGroupID];
                var groupSetting = roGroups.Where(g => g.SiteGroupId == recordGroupId && g.ScopeId == g.SiteGroupId).FirstOrDefault();
                if (groupSetting != null)
                {
                    AddToEmailUser(sendEmailUsers, out ownerIds, out owners, groupSetting);
                    return;
                }
            }
        }

        private void GetRecordOwnerFromSPLocalSetting(List<UserInfo> sendEmailUsers, List<RecordOwnerGroupDto> roGroups, ManualExportReportInfo report, out List<int> ownerIds, out List<RecordOwnerDto> owners)
        {
            ownerIds = null;
            owners = null;
            bool isNotGetContainerLevel = false;
            var recordSiteId = report.RegistedSiteId;
            var siteUrl = report.SiteUrl;
            //Folder和Folder级别以下的
            if (report.ObjectLevel != RMReportObjectLevel.SiteCollection && report.ObjectLevel != RMReportObjectLevel.Site && report.ObjectLevel != RMReportObjectLevel.List)
            {
                isNotGetContainerLevel = true;//用来标识需要获取的是否为Container级别的Owner
                //Folder级别ID是自身ID，Folder以下级别ID是parentID
                var folderId = report.ObjectLevel == RMReportObjectLevel.Folder ? report.NodeID : report.ParentID;
                if (Guid.Empty != folderId)
                {
                    var group = ProcessReportInFolderLevel(folderId, roGroups);
                    if (group != null)
                    {
                        AddToEmailUser(sendEmailUsers, out ownerIds, out owners, group);
                        return;
                    }
                    else
                    {
                        var webId = report.WebID;
                        var listId = report.ListID;
                        try
                        {
                            if (!string.IsNullOrEmpty(report.ServerRelativeUrl) && !report.ServerRelativeUrl.StartsWith("/"))
                            {
                                report.ServerRelativeUrl = "/" + report.ServerRelativeUrl;
                            }
                            //mLogger.Info("ma process folder report.ServerRelativeUrl:{0} ", report.ServerRelativeUrl);
                            //var folderServerRelativeUrl = report.ServerRelativeUrl.Contains("\\") ? report.ServerRelativeUrl.Substring(0, report.ServerRelativeUrl.IndexOf("\\")) : report.ServerRelativeUrl;

                            var folderFullPath = report.Path.Substring(0, report.Path.LastIndexOf("/"));
                            mLogger.Info("ma process folder full path :{0}", folderFullPath);

                            if (report.ObjectLevel == RMReportObjectLevel.Folder)
                            {
                                group = ProcessReportInFolderLevel(report.ParentID, roGroups);
                                if (group != null)
                                {
                                    AddToEmailUser(sendEmailUsers, out ownerIds, out owners, group);
                                    return;
                                }
                            }

                            try
                            {
                                var parentFolderIds = SharePointOnPremClient.GetRootParentIdsFromFolder(siteUrl, webId.ToString(), listId.ToString(), folderFullPath);
                                if (parentFolderIds != null)
                                {
                                    foreach (var parentFolderId in parentFolderIds)
                                    {
                                        group = ProcessReportInFolderLevel(new Guid(parentFolderId), roGroups);
                                        if (group != null)
                                        {
                                            AddToEmailUser(sendEmailUsers, out ownerIds, out owners, group);
                                            return;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                mLogger.Info($"Failed to get parent folders ids, Error: {ex}");
                            }
                        }
                        catch (Exception ex)
                        {
                            mLogger.Error("Somthing went wrong, report item path: {0}, error msg: {1}.", report.Path, ex.ToString());
                            return;
                        }
                    }
                }
            }

            //List和List级别以下的
            if (isNotGetContainerLevel || (isNotGetContainerLevel = report.ObjectLevel == RMReportObjectLevel.List))
            //if (report.ObjectLevel != RMReportObjectLevel.SiteCollection && report.ObjectLevel != RMReportObjectLevel.Site)
            {
                //isNotGetContainerLevel = true;
                var recordSiteNodeId = report.RegistedSiteId;
                var group = roGroups.Where(g => g.ListId == report.ListID && g.SiteId == recordSiteNodeId && g.ScopeId == g.ListId).FirstOrDefault();
                if (group != null)
                {
                    AddToEmailUser(sendEmailUsers, out ownerIds, out owners, group);
                    return;
                }
            }
            if (isNotGetContainerLevel || (isNotGetContainerLevel = report.ObjectLevel == RMReportObjectLevel.Site))
            {
                var group = ProcessReportInWebLevel(report.WebID, roGroups);
                if (group != null)
                {
                    AddToEmailUser(sendEmailUsers, out ownerIds, out owners, group);
                    return;
                }
                else
                {
                    try
                    {
                        var tempWebId = report.ParentID;
                        if (report.ObjectLevel == RMReportObjectLevel.Site)
                        {
                            group = ProcessReportInWebLevel(tempWebId, roGroups);
                            if (group != null)
                            {
                                AddToEmailUser(sendEmailUsers, out ownerIds, out owners, group);
                                return;
                            }
                        }

                        try
                        {
                            var parentWebIds = SharePointOnPremClient.GetRootParentIdsFromWeb(siteUrl, tempWebId.ToString());
                            foreach (var parentWebId in parentWebIds)
                            {
                                group = ProcessReportInWebLevel(new Guid(parentWebId), roGroups);
                                if (group != null)
                                {
                                    AddToEmailUser(sendEmailUsers, out ownerIds, out owners, group);
                                    return;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            mLogger.Info($"Failed to get parent webs ids, Error: {ex}");
                        }
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("Somthing went wrong, report item path: {0}, error msg: {1}.", report.Path, ex.ToString());
                        return;
                    }
                }
            }
            if (isNotGetContainerLevel || (isNotGetContainerLevel = report.ObjectLevel == RMReportObjectLevel.SiteCollection))
            {
                var recordSiteNodeId = report.RegistedSiteId;
                var group = roGroups.Where(g => g.SiteId == recordSiteNodeId && g.ScopeId == g.SiteId).FirstOrDefault();
                if (group != null)
                {
                    AddToEmailUser(sendEmailUsers, out ownerIds, out owners, group);
                    return;
                }
            }
            var recordGroupId = report.SiteGroupID;
            var groupSetting = roGroups.Where(g => g.SiteGroupId == recordGroupId && g.ScopeId == g.SiteGroupId).FirstOrDefault();
            if (groupSetting != null)
            {
                AddToEmailUser(sendEmailUsers, out ownerIds, out owners, groupSetting);
                return;
            }
        }

        private void GetRecordOwnerFromEXOSetting(List<UserInfo> sendEmailUsers, List<RecordOwnerGroupDto> roGroups, ManualExportReportInfo report, out List<int> ownerIds, out List<RecordOwnerDto> owners)
        {
            ownerIds = null;
            owners = null;
            if (daoRecordMailBoxIdMapping.ContainsKey(report.MailBoxID))
            {
                var recordMailBoxId = daoRecordMailBoxIdMapping[report.MailBoxID];
                var currentSetting = roGroups.Where(g => g.ScopeId == recordMailBoxId).FirstOrDefault();
                if (currentSetting != null)
                {
                    AddToEmailUser(sendEmailUsers, out ownerIds, out owners, currentSetting);
                    return;
                }
            }
            var daoMailGroupId = report.SiteGroupID;
            if (daoRecordMailGroupIdMapping.ContainsKey(daoMailGroupId))
            {
                var recordMailGroupId = daoRecordMailGroupIdMapping[daoMailGroupId];
                var groupSetting = roGroups.Where(g => g.ScopeId == recordMailGroupId).FirstOrDefault();
                if (groupSetting != null)
                {
                    AddToEmailUser(sendEmailUsers, out ownerIds, out owners, groupSetting);
                    return;
                }
            }
        }

        private void GetRecordOwnerFromPhysicalSetting(List<UserInfo> sendEmailUsers, List<RecordOwnerGroupDto> roGroups, ManualExportReportInfo report, out List<int> ownerIds, out List<RecordOwnerDto> owners)
        {
            ownerIds = null;
            owners = null;
            //var currentSetting = roGroups.Where(g => g.ScopeId == report.MailBoxID).FirstOrDefault();
            //if (currentSetting != null)
            //{
            //    AddToEmailUser(sendEmailUsers, out ownerIds, out owners, currentSetting);
            //    return;
            //}
            var groupSetting = roGroups.Where(g => g.ScopeId == report.TopLocationID).FirstOrDefault();
            if (groupSetting != null)
            {
                AddToEmailUser(sendEmailUsers, out ownerIds, out owners, groupSetting);
                return;
            }
        }

        private void GetRecordOwnerFromFileSystem(List<UserInfo> sendEmailUsers, List<RecordOwnerGroupDto> roGroups, ManualExportReportInfo report, out List<int> ownerIds, out List<RecordOwnerDto> owners)
        {
            ownerIds = null;
            owners = null;
            var groupSetting = roGroups.Where(g => g.ScopeId == new Guid(report.ScopeID)).FirstOrDefault();
            if (groupSetting != null)
            {
                AddToEmailUser(sendEmailUsers, out ownerIds, out owners, groupSetting);
                return;
            }
        }

        private static void AddToEmailUser(List<UserInfo> sendEmailUsers, out List<int> ownerIds, out List<RecordOwnerDto> owners, RecordOwnerGroupDto group)
        {
            ownerIds = group.Owners.Select(s => s.LnkId).Distinct().ToList();
            owners = group.Owners;
            if (group.MailToOwner)
            {
                group.Owners.ForEach(o =>
                {
                    if (sendEmailUsers.Any(s => s.UserId == o.ObjectId))
                    {

                    }
                    else
                    {
                        sendEmailUsers.Add(new UserInfo()
                        {
                            DisplayName = o.DisplayName,
                            Email = o.UserPrincipalName,
                            UserPrincipalName = o.UserPrincipalName,
                            UserId = o.ObjectId,
                            //InviteType = o.Type == AccountType.Group ? InviteType.Group : InviteType.User
                        });
                    }
                });
            }
        }


        private RecordOwnerGroupDto ProcessReportInFolderLevel(Guid folder, List<RecordOwnerGroupDto> roGroups)
        {
            return roGroups.Where(g => g.FolderId == folder && g.FolderId == g.ScopeId).FirstOrDefault();
        }
        private RecordOwnerGroupDto ProcessReportInWebLevel(Guid webId, List<RecordOwnerGroupDto> roGroups)
        {
            return roGroups.Where(g => g.WebId == webId && g.WebId == g.ScopeId).FirstOrDefault();
        }

        private IAveSite GetAveSite(ManualExportReportInfo report)
        {
            IAveSite site = null;
            try
            {
                if (!mUnavailableSiteIds.Contains(report.RegistedSiteId))
                {
                    if (!mAveSiteCollections.TryGetValue(report.RegistedSiteId, out site))
                    {
                        RMSPTreeNode siteNode = null;
                        var registedGroupId = report.SiteGroupID;
                        if(daoRecordSiteIdMapping.TryGetValue(report.RegistedSiteId, out var recordSiteId))
                        {
                            if (!mSiteTreeNodes.TryGetValue(recordSiteId, out siteNode))
                            {
                                mLogger.Info($"Can't find siteNode from mSiteTreeNodes.");
                                RMSPTreeNode groupNode;
                                if (daoRecordGroupIdMapping.TryGetValue(registedGroupId, out Guid recordGroupUniqueId))
                                {
                                    mLogger.Info($"Get record sharepoint online group id from daoRecordGroupIdMapping. group id: [{recordGroupUniqueId}].");
                                    if (mAllSiteGroupTreeNode.TryGetValue(recordGroupUniqueId, out groupNode))
                                    {
                                        mLogger.Info("Get records sharepoint online group node from mAllSiteGroupTreeNode.");
                                        foreach (var child in RMSPTreeService.Browse(groupNode))
                                        {
                                            var siteId = new Guid(child.Id);
                                            if (StringComparer.OrdinalIgnoreCase.Equals(report.SiteUrl, child.FullPath))
                                            {
                                                siteNode = child;
                                            }
                                            mSiteTreeNodes[siteId] = child;
                                        }
                                        mAllSiteGroupTreeNode.Remove(recordGroupUniqueId);
                                    }
                                }
                            }
                        }
                        else
                        {
                            mSiteTreeNodes.Remove(report.RegistedSiteId);
                        }
                        if (siteNode != null)
                        {
                            var factory = MultiAppUtil.CreateAveObjectModelFactory(report.SiteUrl, PoolUserUtil.GetAveBPOSAccountInfo(siteNode.BposInfo, report.SiteUrl), AveContextKind.ClientObjectModel);
                            site = factory.CreateSite();
                            mAveSiteCollections[report.RegistedSiteId] = site;

                        }
                    }
                    if (site == null)
                    {
                        mUnavailableSiteIds.Add(report.RegistedSiteId);
                        mLogger.Warn("Site not found or unavailable, siteId: {0}, siteUrl: {1}.", report.RegistedSiteId, report.SiteUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                mUnavailableSiteIds.Add(report.RegistedSiteId);
                mLogger.Error("Get site failed, siteId: {0}, siteUrl: {1}, error msg: {2}.", report.RegistedSiteId, report.SiteUrl, ex.ToString());
            }

            return site;
        }

        private string GetObjectLevelI18NStr(RMReportObjectLevel level)
        {
            string result = string.Empty;
            switch (level)
            {
                case RMReportObjectLevel.PhysicalBox:
                    result = I18NEntity.GetString("RM_Common_ObjectLevel_PhysicalBox");
                    break;
                case RMReportObjectLevel.PhysicalFile:
                    result = I18NEntity.GetString("RM_Common_ObjectLevel_PhysicalFile");
                    break;
                case RMReportObjectLevel.Document:
                case RMReportObjectLevel.SiteCollection:
                case RMReportObjectLevel.Site:
                case RMReportObjectLevel.List:
                case RMReportObjectLevel.Item:
                case RMReportObjectLevel.PhysicalRecord:
                case RMReportObjectLevel.Folder:
                case RMReportObjectLevel.Attachment:
                case RMReportObjectLevel.ExchangeOnlineItem:
                default:
                    result = level.ToString();
                    break;
            }
            return result;
        }

        private string ConvertObjectLevelToString(RMReportObjectLevel level)
        {
            switch (level)
            {
                case RMReportObjectLevel.Document: return "RM_JS_Rule_ObjectLevel_Document";
                case RMReportObjectLevel.SiteCollection: return "RM_JS_Rule_ObjectLevel_SiteCollection";
                case RMReportObjectLevel.Site: return "RM_JS_Rule_ObjectLevel_Site";
                case RMReportObjectLevel.List: return "RM_JS_Rule_ObjectLevel_List";
                case RMReportObjectLevel.Item: return "RM_JS_Rule_ObjectLevel_Item";
                case RMReportObjectLevel.Folder: return "RM_JS_Rule_ObjectLevel_Folder";
                case RMReportObjectLevel.ExchangeOnlineItem: return "RM_JS_Rule_ObjectLevel_ExchangeOnlineItem";
                case RMReportObjectLevel.PhysicalBox: return "RM_Common_ObjectLevel_PhysicalBox";
                case RMReportObjectLevel.PhysicalFile: return "RM_JS_Rule_ObjectLevel_PhysicalFile";
                case RMReportObjectLevel.PhysicalRecord: return "RM_JS_Rule_ObjectLevel_PhysicalRecord";
                case RMReportObjectLevel.FSFile: return "RM_JS_Rule_ObjectLevel_Document";
                default: return "";
            }
        }

        private void SendJobReportDetails(ManualExportReportInfo report, JobDetailsStatus status, string recordOwner, string comments = "")
        {
            JMManualApprovalJobDetails detail = new JMManualApprovalJobDetails()
            {
                TitleOrName = report.LeafName,
                Url = report.Path,
                ObjectLevel = ConvertObjectLevelToString(report.ObjectLevel),
                Action = ManualApprovalAction.Export.ToString(),
                RuleCriteria = report.RuleInfo?.Criteria,
                RecordOwner = recordOwner,
                Status = status,
                Comment = comments,
            };
            if (report.Status == SOApproveDBStatus.Approved)
            {
                detail.ApprovalStatus = "RM_DAM_ManualApproval_ApprovedStatus";
            }
            else if (report.Status == SOApproveDBStatus.Rejected)
            {
                detail.ApprovalStatus = "RM_DAM_ManualApproval_RejectedStatus";
            }
            else if (report.Status == SOApproveDBStatus.WaitingApprove)
            {
                detail.ApprovalStatus = "RM_DAM_ManualApproval_WaitingApproveStatus";
            }
            //mJobDetails.Add(detail);
            //if (mJobDetails.Count >= 5000)
            //{
            //    JobDetailService.UpdateJobDetails(mJobDetails, mJobInfo);
            //    mJobDetails.Clear();
            //}
            JobContext.ReportManager.SendJobDetail(detail);
        }

        private void SendJobReportDetails(RMManualApprove data, JobDetailsStatus status, string recordOwner, string comments = "")
        {
            JMManualApprovalJobDetails detail = new JMManualApprovalJobDetails()
            {
                TitleOrName = data.LeafName,
                Url = data.Url,
                RuleCriteria = data.Criteria,
                //ObjectLevel = ((RMReportObjectLevel)data.ObjectLevel).ToString(),
                ObjectLevel = ConvertObjectLevelToString((RMReportObjectLevel)data.ObjectLevel),
                //ApprovalStatus = data,
                Action = ManualApprovalAction.Import.ToString(),
                RecordOwner = recordOwner,
                Status = status,
                Comment = comments,
            };
            if (data.Status == (int)SOApproveDBStatus.Approved)
            {
                detail.ApprovalStatus = "RM_DAM_ManualApproval_ApprovedStatus";
            }
            else if (data.Status == (int)SOApproveDBStatus.Rejected)
            {
                detail.ApprovalStatus = "RM_DAM_ManualApproval_RejectedStatus";
            }
            else if (data.Status == (int)SOApproveDBStatus.WaitingApprove)
            {
                detail.ApprovalStatus = "RM_DAM_ManualApproval_WaitingApproveStatus";
            }
            //mJobDetails.Add(detail);
            //if (mJobDetails.Count >= 5000)
            //{
            //    JobDetailService.UpdateJobDetails(mJobDetails, mJobInfo);
            //    mJobDetails.Clear();
            //}
            JobContext.ReportManager.SendJobDetail(detail);
        }
        private ManualRuleInfo GetRuleInfo(string ruleIdStr)
        {
            ManualRuleInfo cacheRule = null;
            try
            {
                var ruleId = new Guid(ruleIdStr);
                if (!mCacheRuleInfo.ContainsKey(ruleId))
                {
                    var rule = RuleManagerService.LoadRule(ruleIdStr);
                    cacheRule = new ManualRuleInfo()
                    {
                        RuleId = rule.RuleId,
                        RuleName = rule.RuleName,
                        Criteria = string.Join(" ", rule.RuleCretias),
                        EXOCriteria = rule.EXORule == null ? string.Empty : string.Join(" ", rule.EXORule.RuleCretias),
                        PhysicalCriteria = rule.PhysicalRule == null ? string.Empty : string.Join(" ", rule.PhysicalRule.RuleCretias),
                        FSCriteria = rule.FSRule == null ? string.Empty : string.Join(" ", rule.FSRule.RuleCretias),
                        SPLocalCriteria = rule.SPLocalRule == null ? string.Empty : string.Join(" ", rule.SPLocalRule.RuleCretias),
                        OneDriveCriteria = rule.OneDriveRule == null? string.Empty: string.Join(" ", rule.OneDriveRule.RuleCretias),
                        IsSendEmailToOwner = rule.IsSendEmailToOwner,
                        EXOIsSendEmailToOwner = rule.EXORule == null ? false : rule.EXORule.IsSendEmailToOwner,
                        PhysicalIsSendEmailToOwner = rule.PhysicalRule == null ? false : rule.PhysicalRule.IsSendEmailToOwner,
                        FSIsSendEmailToOwner = rule.FSRule == null ? false : rule.FSRule.IsSendEmailToOwner,
                        SPLocalIsSendEmailToOwner = rule.SPLocalRule == null ? false : rule.SPLocalRule.IsSendEmailToOwner,
                        OneDriveIsSendEmailToOwner = rule.OneDriveRule == null? false: rule.OneDriveRule.IsSendEmailToOwner,
                        WorkflowId = rule.WorkflowId,
                        EXOWorkflowId = rule.EXORule == null ? "" : rule.EXORule.WorkflowId,
                        PhyWorkflowId = rule.PhysicalRule == null ? "" : rule.PhysicalRule.WorkflowId,
                        FSWorkflowId = rule.FSRule == null ? "" : rule.FSRule.WorkflowId,
                        SPLocalWorkflowId = rule.SPLocalRule == null ? "" : rule.SPLocalRule.WorkflowId,
                        OneDriveWorkflowId = rule.OneDriveRule == null ? "" : rule.OneDriveRule.WorkflowId,
                        DisposalClass = rule.DisposalClass
                    };

                    cacheRule.Users = RuleManagerService.Convert2RecordOwnerInfos(rule.Users);
                    if (rule.EXORule != null)
                    {
                        cacheRule.EXOUsers = RuleManagerService.Convert2RecordOwnerInfos(rule.EXORule.Users);
                    }
                    if (rule.PhysicalRule != null)
                    {
                        cacheRule.PhysicalUsers = RuleManagerService.Convert2RecordOwnerInfos(rule.PhysicalRule.Users);
                    }
                    if (rule.FSRule != null)
                    {
                        cacheRule.FSUsers = RuleManagerService.Convert2RecordOwnerInfos(rule.FSRule.Users);
                    }
                    if (rule.SPLocalRule != null)
                    {
                        cacheRule.SPLocalUsers = RuleManagerService.Convert2RecordOwnerInfos(rule.SPLocalRule.Users);
                    }
                    if (rule.OneDriveRule != null)
                    {
                        cacheRule.OneDriveUsers = RuleManagerService.Convert2RecordOwnerInfos(rule.OneDriveRule.Users);
                    }
                    mCacheRuleInfo[ruleId] = cacheRule;
                }
                else
                {
                    cacheRule = mCacheRuleInfo[ruleId];
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("error occurred while get manual report ruleInfo,ruleId:{0},ERROR:{1}", ruleIdStr, ex.ToString());
            }
            return cacheRule;
        }
        private RMManualApprove ConvertReportToEntity(ManualExportReportInfo reportDetails)
        {
            var aspd = JsonConvert.DeserializeObject<ArchiverSharePointDto>(reportDetails.JsonMeta);
            var entity = new RMManualApprove();
            entity.ObjectLevel = (int)reportDetails.ObjectLevel;
            entity.LeafName = reportDetails.LeafName;
            entity.Url = reportDetails.Path;
            entity.Status = (int)reportDetails.Status;
            entity.ArchiveLevel = reportDetails.ArchiveLevel;
            entity.Version = GetVersion(reportDetails.UIVersion);
            entity.ContentType = reportDetails.ContentType;
            entity.ModifiedBy = reportDetails.ModifiedBy;
            entity.CreatedBy = reportDetails.CreatedBy;
            entity.RuleId = reportDetails.RuleID;
            entity.RuleName = reportDetails.RuleInfo?.RuleName;
            entity.Criteria = reportDetails.RuleInfo?.Criteria;
            entity.PartKey = reportDetails.PartKey;
            entity.RowKey = reportDetails.RowKey;
            entity.ActionStatus = (int)Contract.Schedule.ActionStatus.None;
            entity.CollectionTime = DateTime.UtcNow.Ticks;
            entity.ActionTime = 0;
            entity.SiteId = aspd.SiteId;
            entity.NodeId = reportDetails.NodeID;
            entity.WorkflowInstanceId = reportDetails.WorkflowInstanceId;
            entity.IsRelatedRecords = reportDetails.HasRelatedDocument > 0;
            entity.RelatedRecordsAction = reportDetails.DeleteRelatedRecords;
            entity.DisposalClass = GetRuleInfo(reportDetails.RuleID)?.DisposalClass;
            if (!string.IsNullOrEmpty(reportDetails.RelatedRecordInfo))
            {
                try
                {
                    var sourceUrlValue = reportDetails.RelatedRecordInfo;
                    var utility = new RelatedRecordsUtility();
                    List<ReportRelatedRecords> reportRelatedRecords = new List<ReportRelatedRecords>();
                    var relatedInfos = utility.GetRelatedPropertiesBySPColumnValue(sourceUrlValue);
                    relatedInfos.ForEach(r =>
                    {
                        if (r.SourceFlag == (int)SourceFlag.Physical)
                        {
                            var url = $"/Root/PRM/RecordsExplorer/?uniqueId={r.recId}";
                            reportRelatedRecords.Add(new ReportRelatedRecords() { Name = r.recId, Url = url });
                        }
                        //None 表示SP 老数据
                        else if (r.SourceFlag == (int)SourceFlag.SharePoint || r.SourceFlag == (int)SourceFlag.All)
                        {
                            var relatedItemUrl = GetItemFullPath(r.SiteUrl, r.url);
                            reportRelatedRecords.Add(new ReportRelatedRecords() { Name = r.name, Url = relatedItemUrl });
                        }
                    });

                    //XmlDocument xmlDoc = new XmlDocument();
                    ////sourceUrlValue = HttpUtility.UrlDecode(sourceUrlValue);//??
                    //sourceUrlValue = sourceUrlValue.Replace("&#58;", ":");
                    //xmlDoc.LoadXml(sourceUrlValue);
                    //foreach (var ele in xmlDoc.GetElementsByTagName("a"))
                    //{
                    //    XmlElement element = ele as XmlElement;
                    //    var relatedObjString = element.GetAttribute("rel");
                    //    JavaScriptSerializer jss = new JavaScriptSerializer();
                    //    RMRelatedItemInfo relatedObj = jss.Deserialize<RMRelatedItemInfo>(relatedObjString);
                    //    var relatedItemUrl = HttpUtility.UrlDecode(element.GetAttribute("href"));
                    //    //string url = string.Empty;
                    //    //if (relatedObj.url == null)
                    //    //{
                    //    //    if (!element.GetAttribute("href").StartsWith(relatedObj.SiteUrl))//parmDic["SiteUrl"]))
                    //    //    {
                    //    //        var webServerRelativeUrl = relatedObj.WebServerRelativeUrl;
                    //    //        url = element.GetAttribute("href").Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
                    //    //        url = relatedObj.SiteUrl + "/" + url;
                    //    //    }
                    //    //    relatedObj.url = url;
                    //    //}
                    //    relatedObj.url = relatedItemUrl;
                    //    if (relatedObj.SourceFlag == (int)SourceFlag.Physical)
                    //    {
                    //        var url = $"/Root/PRM/RecordsExplorer/?uniqueId={relatedObj.recId}";
                    //        reportRelatedRecords.Add(new ReportRelatedRecords() { Name = relatedObj.recId, Url = url });
                    //    }
                    //    //None 表示SP 老数据
                    //    else if (relatedObj.SourceFlag == (int)SourceFlag.SharePoint || relatedObj.SourceFlag == (int)SourceFlag.None)
                    //    {
                    //        StringBuilder stringBuilder = new StringBuilder(512);
                    //        var siteUri = new Uri(relatedObj.SiteUrl);
                    //        stringBuilder.Append("https:");
                    //        stringBuilder.Append("//");
                    //        stringBuilder.Append(siteUri.Host);
                    //        reportRelatedRecords.Add(new ReportRelatedRecords() { Name = relatedObj.name, Url = stringBuilder.ToString() + relatedObj.url });
                    //    }
                    //}
                    entity.RelatedRecords = SerializerHelper.SerializeToXmlString(reportRelatedRecords);
                }
                catch (Exception e)
                {
                    mLogger.Warn("get related record info error{0}", e.ToString());
                }
            }
            return entity;
        }

        private RMManualApprove ConvertReportToEntityForSPOnPrem(ManualExportReportInfo reportDetails)
        {
            var aspd = JsonConvert.DeserializeObject<OnPremiseArchiverSharePointDto>(reportDetails.JsonMeta);
            var entity = new RMManualApprove
            {
                ObjectLevel = (int)reportDetails.ObjectLevel,
                LeafName = reportDetails.LeafName,
                Url = reportDetails.Path,
                Status = (int)reportDetails.Status,
                ArchiveLevel = reportDetails.ArchiveLevel,
                Version = GetVersion(reportDetails.UIVersion),
                ContentType = reportDetails.ContentType,
                ModifiedBy = reportDetails.ModifiedBy,
                CreatedBy = reportDetails.CreatedBy,
                RuleId = reportDetails.RuleID,
                RuleName = reportDetails.RuleInfo?.RuleName,
                Criteria = reportDetails.RuleInfo?.SPLocalCriteria,
                PartKey = reportDetails.PartKey,
                RowKey = reportDetails.RowKey,
                ActionStatus = (int)Contract.Schedule.ActionStatus.None,
                CollectionTime = DateTime.UtcNow.Ticks,
                ActionTime = 0,
                SiteId = new Guid(aspd.SiteId),
                NodeId = reportDetails.NodeID,
                WorkflowInstanceId = reportDetails.WorkflowInstanceId,
                //entity.IsRelatedRecords = reportDetails.HasRelatedDocument > 0;
                //entity.RelatedRecordsAction = reportDetails.DeleteRelatedRecords;
                DisposalClass = GetRuleInfo(reportDetails.RuleID)?.DisposalClass
            };
            return entity;
        }

        private RMManualApprove ConvertReportToEntityForEXO(ManualExportReportInfo reportDetails)
        {
            var aspd = JsonConvert.DeserializeObject<ExchangeOnlineTableEntity>(reportDetails.JsonMeta);
            var entity = new RMManualApprove();
            entity.ObjectLevel = (int)reportDetails.ObjectLevel;
            entity.LeafName = reportDetails.LeafName;
            entity.Url = reportDetails.Path;
            entity.Status = (int)reportDetails.Status;
            entity.ArchiveLevel = reportDetails.ArchiveLevel;
            entity.Version = GetVersion(reportDetails.UIVersion);
            entity.ContentType = reportDetails.ContentType;
            entity.ModifiedBy = reportDetails.ModifiedBy;
            entity.CreatedBy = reportDetails.CreatedBy;
            entity.RuleId = reportDetails.RuleID;
            entity.RuleName = reportDetails.RuleInfo?.RuleName;
            entity.Criteria = reportDetails.RuleInfo?.EXOCriteria;
            entity.PartKey = reportDetails.PartKey;
            entity.RowKey = reportDetails.RowKey;
            entity.ActionStatus = (int)Contract.Schedule.ActionStatus.None;
            entity.CollectionTime = DateTime.UtcNow.Ticks;
            entity.ActionTime = 0;
            entity.SiteId = reportDetails.MailBoxID;
            entity.NodeId = reportDetails.NodeID;
            entity.DisposalClass = GetRuleInfo(reportDetails.RuleID)?.DisposalClass;
            entity.IsRelatedRecords = false;
            entity.RelatedRecordsAction = reportDetails.DeleteRelatedRecords;
            return entity;
        }

        private RMManualApprove ConvertReportToEntityForPhysical(ManualExportReportInfo reportDetails)
        {
            //var aspd = JsonConvert.DeserializeObject<ExchangeOnlineTableEntity>(reportDetails.JsonMeta);
            var entity = new RMManualApprove();
            entity.ObjectLevel = (int)reportDetails.ObjectLevel;
            entity.LeafName = reportDetails.LeafName;
            entity.Url = reportDetails.Path;
            entity.Status = (int)reportDetails.Status;
            entity.ArchiveLevel = reportDetails.ArchiveLevel;
            entity.Version = GetVersion(reportDetails.UIVersion);
            entity.ContentType = reportDetails.ContentType;
            entity.ModifiedBy = reportDetails.ModifiedBy;
            entity.CreatedBy = reportDetails.CreatedBy;
            entity.RuleId = reportDetails.RuleID;
            entity.RuleName = reportDetails.RuleInfo?.RuleName;
            entity.Criteria = reportDetails.RuleInfo?.PhysicalCriteria;
            //entity.PartKey = reportDetails.PartKey;
            //entity.RowKey = reportDetails.RowKey;
            entity.ActionStatus = (int)Contract.Schedule.ActionStatus.None;
            entity.CollectionTime = DateTime.UtcNow.Ticks;
            entity.ActionTime = 0;
            //entity.SiteId = reportDetails.MailBoxID;
            entity.NodeId = reportDetails.NodeID;
            entity.DisposalClass = GetRuleInfo(reportDetails.RuleID)?.DisposalClass;
            entity.IsRelatedRecords = reportDetails.HasRelatedDocument > 0;
            entity.RelatedRecordsAction = reportDetails.DeleteRelatedRecords;
            if (!string.IsNullOrEmpty(reportDetails.RelatedRecordInfo))
            {
                try
                {
                    List<ReportRelatedRecords> reportRelatedRecords = new List<ReportRelatedRecords>();
                    var relatedRecordInfos = SerializerHelper.DeserializeFromXmlString<List<RMRelatedItemInfo>>(reportDetails.RelatedRecordInfo);
                    foreach (var relatedRecordInfo in relatedRecordInfos)
                    {
                        if (relatedRecordInfo.SourceFlag == (int)SourceFlag.Physical)
                        {
                            var url = $"/Root/PRM/RecordsExplorer/?uniqueId={relatedRecordInfo.recId}";
                            reportRelatedRecords.Add(new ReportRelatedRecords() { Name = relatedRecordInfo.recId, Url = url });
                        }
                        //None 表示SP 老数据
                        else if (relatedRecordInfo.SourceFlag == (int)SourceFlag.SharePoint || relatedRecordInfo.SourceFlag == (int)SourceFlag.None || relatedRecordInfo.SourceFlag == (int)SourceFlag.All)
                        {
                            reportRelatedRecords.Add(new ReportRelatedRecords() { Name = relatedRecordInfo.name, Url = relatedRecordInfo.url });
                        }
                    }
                    entity.RelatedRecords = SerializerHelper.SerializeToXmlString(reportRelatedRecords);
                }
                catch (Exception e)
                {
                    mLogger.Warn("get related record info error{0}", e.ToString());
                }
            }
            return entity;
        }

        private RMManualApprove ConvertReportToEntityForFS(ManualExportReportInfo reportDetails)
        {

            var entity = new RMManualApprove
            {
                NodeId = reportDetails.NodeID,
                SiteId = new Guid(reportDetails.ScopeID),
                PartKey = reportDetails.PartKey,
                RowKey = reportDetails.RowKey,
                LeafName = reportDetails.LeafName,
                ObjectLevel = (int)reportDetails.ObjectLevel,
                Url = reportDetails.Path,
                Status = (int)reportDetails.Status,
                RuleId = reportDetails.RuleID,
                RuleName = reportDetails.RuleInfo?.RuleName,
                Criteria = reportDetails.RuleInfo?.FSCriteria,
                ModifiedBy = reportDetails.ModifiedBy,
                CreatedBy = reportDetails.CreatedBy,
                IsRelatedRecords = reportDetails.HasRelatedDocument > 0,
                RelatedRecordsAction = reportDetails.DeleteRelatedRecords,
                ActionStatus = (int)Contract.Schedule.ActionStatus.None,
                CollectionTime = DateTime.UtcNow.Ticks,
                ActionTime = 0,
                DisposalClass = GetRuleInfo(reportDetails.RuleID)?.DisposalClass
            };

            if (!string.IsNullOrEmpty(reportDetails.RelatedRecordInfo))
            {
                try
                {
                    List<ReportRelatedRecords> reportRelatedRecords = new List<ReportRelatedRecords>();
                    var relatedRecordInfos = SerializerHelper.DeserializeFromXmlString<List<RMRelatedItemInfo>>(reportDetails.RelatedRecordInfo);
                    foreach (var relatedRecordInfo in relatedRecordInfos)
                    {
                        var url = $"{relatedRecordInfo.name}";//可能需要和relatedRecordInfo.url组装一下
                        reportRelatedRecords.Add(new ReportRelatedRecords() { Name = relatedRecordInfo.id.ToString(), Url = relatedRecordInfo.url });
                    }
                    entity.RelatedRecords = SerializerHelper.SerializeToXmlString(reportRelatedRecords);
                }
                catch (Exception e)
                {
                    mLogger.Warn("fs get related record info error{0}", e.ToString());
                }
            }
            return entity;
        }

        private RMManualApprove ConvertReportToEntityForOneDrive(ManualExportReportInfo reportDetails)
        {
            var aspd = JsonConvert.DeserializeObject<ArchiverSharePointDto>(reportDetails.JsonMeta);
            var entity = new RMManualApprove
            {
                ObjectLevel = (int)reportDetails.ObjectLevel,
                LeafName = reportDetails.LeafName,
                Url = reportDetails.Path,
                Status = (int)reportDetails.Status,
                ArchiveLevel = reportDetails.ArchiveLevel,
                Version = GetVersion(reportDetails.UIVersion),
                ContentType = reportDetails.ContentType,
                ModifiedBy = reportDetails.ModifiedBy,
                CreatedBy = reportDetails.CreatedBy,
                RuleId = reportDetails.RuleID,
                RuleName = reportDetails.RuleInfo?.RuleName,
                Criteria = reportDetails.RuleInfo?.OneDriveCriteria,
                PartKey = reportDetails.PartKey,
                RowKey = reportDetails.RowKey,
                ActionStatus = (int)Contract.Schedule.ActionStatus.None,
                CollectionTime = DateTime.UtcNow.Ticks,
                ActionTime = 0,
                SiteId = aspd.SiteId,
                NodeId = reportDetails.NodeID,
                //WorkflowInstanceId = reportDetails.WorkflowInstanceId,
                //IsRelatedRecords = reportDetails.HasRelatedDocument > 0,
                //RelatedRecordsAction = reportDetails.DeleteRelatedRecords,
                DisposalClass = GetRuleInfo(reportDetails.RuleID)?.DisposalClass
            };
            //if (!string.IsNullOrEmpty(reportDetails.RelatedRecordInfo))
            //{
            //    try
            //    {
            //        var sourceUrlValue = reportDetails.RelatedRecordInfo;
            //        var utility = new RelatedRecordsUtility();
            //        List<ReportRelatedRecords> reportRelatedRecords = new List<ReportRelatedRecords>();
            //        var relatedInfos = utility.GetRelatedPropertiesBySPColumnValue(sourceUrlValue);
            //        relatedInfos.ForEach(r =>
            //        {
            //            if (r.SourceFlag == (int)SourceFlag.Physical)
            //            {
            //                var url = $"/Root/PRM/RecordsExplorer/?uniqueId={r.recId}";
            //                reportRelatedRecords.Add(new ReportRelatedRecords() { Name = r.recId, Url = url });
            //            }
            //            //None 表示SP 老数据
            //            else if (r.SourceFlag == (int)SourceFlag.SharePoint || r.SourceFlag == (int)SourceFlag.All)
            //            {
            //                StringBuilder stringBuilder = new StringBuilder(512);
            //                var siteUri = new Uri(r.SiteUrl);
            //                stringBuilder.Append("https:");
            //                stringBuilder.Append("//");
            //                stringBuilder.Append(siteUri.Host);
            //                reportRelatedRecords.Add(new ReportRelatedRecords() { Name = r.name, Url = stringBuilder.ToString() + r.url });
            //            }
            //        });
            //        entity.RelatedRecords = SerializerHelper.SerializeToXmlString(reportRelatedRecords);
            //    }
            //    catch (Exception e)
            //    {
            //        mLogger.Warn("get related record info error{0}", e.ToString());
            //    }
            //}
            return entity;
        }
        private string GetVersion(int uiversion)
        {
            var version = string.Empty;
            if (uiversion > 0)
            {
                int majorVers = uiversion / 512;
                int minorVers = uiversion % 512;
                version = string.Format("{0}.{1}", majorVers, minorVers);
            }
            return version;
        }

        private string GetItemFullPath(string siteUrl, string itemUrl)
        {
            if (itemUrl.StartsWith("http:") || itemUrl.StartsWith("https:"))
            {
                return itemUrl;
            }
            var stringBuilder = new StringBuilder(512);
            var siteUri = new Uri(siteUrl);
            stringBuilder.Append("https:");
            stringBuilder.Append("//");
            stringBuilder.Append(siteUri.Host);
            return stringBuilder.ToString() + itemUrl;
        }
    }

    class ManualCache
    {
        private IManualApprovalService mManualApprovalService;
        protected IManualApprovalService ManualApprovalService
        {
            get
            {
                if (mManualApprovalService == null)
                {
                    mManualApprovalService = (IManualApprovalService)PlatformWindsorManager.GetService(typeof(IManualApprovalService));
                }
                return mManualApprovalService;
            }
        }
        private IAccountDao mAccountDao;
        protected IAccountDao AccountDao
        {
            get
            {
                if (mAccountDao == null)
                {
                    mAccountDao = (IAccountDao)PlatformWindsorManager.GetService(typeof(IAccountDao));
                }
                return mAccountDao;
            }
        }
        static object loceker = new object();
        private static ManualCache _instance = null;
        public static ManualCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (loceker)
                    {
                        if (_instance == null)
                        {
                            _instance = new ManualCache();
                        }
                    }
                }
                return _instance;
            }
        }
        public Dictionary<Guid, List<AccountDto>> WorkflowOwners = new Dictionary<Guid, List<AccountDto>>();

        public List<AccountDto> TryGetWorkflowOwner(Guid workflowId)
        {
            List<AccountDto> accountDtos = new List<AccountDto>();
            if (!WorkflowOwners.ContainsKey(workflowId))
            {
                accountDtos = ManualApprovalService.GetUserIdsForManualJob(null, Guid.Empty);
                WorkflowOwners[workflowId] = accountDtos;
            }
            else
            {
                accountDtos = WorkflowOwners[workflowId];
            }
            return accountDtos;
        }



        public List<string> TryGetOwnerIds(List<Guid> workflowIds)
        {
            List<string> accountIds = new List<string>();
            foreach (var item in WorkflowOwners)
            {
                if (workflowIds.Contains(item.Key))
                {
                    accountIds.AddRange(item.Value.Select(u => u.UserId));
                }
            }

            return accountIds.Distinct().ToList();
        }
    }
}
