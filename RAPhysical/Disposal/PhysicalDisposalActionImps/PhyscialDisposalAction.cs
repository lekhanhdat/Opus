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
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon.Contract.CommonFilter.Rules;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.OnPremiseSharePoint;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RAPhysical.API;
using AvePoint.RA.RAPhysical.Common;
using AvePoint.RA.RAPhysical.ExplorerMove;
using AvePoint.Wrapper.Common;
using DocumentFormat.OpenXml.Office2010.Excel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RAManualApprovalCommon.Archiver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAContract = AvePoint.GCommon.Contract.StorageOptimization.Object;

namespace AvePoint.RA.RAPhysical.Disposal.PhysicalDisposalActionImps
{
    //TO DO ylgu... Set status with query.like empty file under pending box action.
    public class PhyscialDisposalAction : IPhysicalDisposalAction
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(PhyscialDisposalAction));
        private IRecordAllianceDao mIRecordAllianceDao;
        protected IRecordAllianceDao RecordAllianceDao
        {
            get
            {
                if (mIRecordAllianceDao == null)
                {
                    mIRecordAllianceDao = (IRecordAllianceDao)PlatformWindsorManager.GetService(typeof(IRecordAllianceDao));
                }
                return mIRecordAllianceDao;
            }
        }
        private IRecordLoanAllianceDao mRecordLoanAllianceDao;
        protected IRecordLoanAllianceDao RecordLoanAllianceDao
        {
            get
            {
                if (mRecordLoanAllianceDao == null)
                {
                    mRecordLoanAllianceDao = (IRecordLoanAllianceDao)PlatformWindsorManager.GetService(typeof(IRecordLoanAllianceDao));
                }
                return mRecordLoanAllianceDao;
            }
        }

        private IRMScopePermissionDao mRMScopePermissionDao;
        protected IRMScopePermissionDao RMScopePermissionDao
        {
            get
            {
                if (mRMScopePermissionDao == null)
                {
                    mRMScopePermissionDao = (IRMScopePermissionDao)PlatformWindsorManager.GetService(typeof(IRMScopePermissionDao));
                }
                return mRMScopePermissionDao;
            }
        }

        private ITemplateManagementService mTemplateManagementService;
        protected ITemplateManagementService TemplateManagementService
        {
            get
            {
                if (mTemplateManagementService == null)
                {
                    mTemplateManagementService = (ITemplateManagementService)PlatformWindsorManager.GetService(typeof(ITemplateManagementService));
                }
                return mTemplateManagementService;
            }
        }

        private IRMTemplateDao mRMTemplateDao;
        protected IRMTemplateDao RMTemplateDao
        {
            get
            {
                if (mRMTemplateDao == null)
                {
                    mRMTemplateDao = (IRMTemplateDao)PlatformWindsorManager.GetService(typeof(IRMTemplateDao));
                }
                return mRMTemplateDao;
            }
        }

        private IExplorerDao ExplorerDao = new ExplorerDao();
        private IRelativeDataArchiverService EnduserArchiverAction => PlatformWindsorManager.GetService<IRelativeDataArchiverService>();

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

        private IRecordsHistoryService RecordsHistoryService => PlatformWindsorManager.GetService<IRecordsHistoryService>();

        //public IRMBoardCacheDao BoardCacheDao { get; set; }
        private DateTime mRunJobTime;

        private string jobId;

        private JobRunBy jobRunBy;

        RMPhysicalExplorerMoveUtility RMPhysicalExplorer;
        public bool HasMoveFailed()
        {
            return RMPhysicalExplorer.HasFailedNode;
        }

        public bool HasMoveSuccess()
        {
            return RMPhysicalExplorer.HasSuccessNode;
        }
        //1:active 2, archived, 3 delete, 4 moved, 5 overwrited(Move job destination file can be overwrited), add enum for this?        
        public PhyscialDisposalAction(DateTime runJobTime, string jobId = null, JobRunBy jobRunBy = JobRunBy.Control)
        {
            mRunJobTime = runJobTime;
            RMPhysicalExplorer = new RMPhysicalExplorerMoveUtility(true, jobRunBy);
            this.jobId = jobId;
            this.jobRunBy = jobRunBy;
        }
        /// <summary>
        /// currently not use batch update , consider imps batch if necessary
        /// </summary>
        /// <param name="box"></param>
        /// <param name="rule"></param>
        /// <summary>
        /// currently not use batch update , consider imps batch if necessary
        /// </summary>
        /// <param name="box"></param>
        /// <param name="rule"></param>
        public PhysicalRecordActionAudit DisposalBox(IPhysicalBox box, Rule rule, SendReportHandler SendReportHandler)
        {
            using (PerformanceScope pc0 = new PerformanceScope("PhyscialDisposalAction.DisposalBox"))
            {
                bool hasHoldOrLoanedFile = false;
                var actionAudits = new List<PhysicalRecordActionAudit>();
                var filesInBox = box.GetFiles(f => f.RecordStatus != (int)RMRecordStatus.Destroyed && f.RecordStatus != (int)RMRecordStatus.RMDeleted && f.RecordStatus != (int)RMRecordStatus.MoveOverwrite);
                var isRuleLastestFolderDisposalDueDateRule = this.IsLastestFolderDisposalDueDateRule(rule);
                hasHoldOrLoanedFile = IsSkipDetroyBox(filesInBox);
                filesInBox.ForEach(f =>
                {
                    if (isRuleLastestFolderDisposalDueDateRule)
                    {
                        HandleFolderWithLastestFolderDisposalDueDateRule(f, hasHoldOrLoanedFile, rule, SendReportHandler);
                    }
                    else
                    {
                        bool isHoldOrLoaned = ExplorerDao.IsRecordsHold(new List<Guid> { f.Id }, mRunJobTime.Ticks) || RecordLoanAllianceDao.IsRecordsLoan(new List<Guid> { f.Id }, mRunJobTime.Ticks);
                        if (!isHoldOrLoaned)
                        {
                            DisposalFile(f, rule, SendReportHandler, includeDeleteBlock: true);
                        }
                        else
                        {
                            SendReportHandler(f.Name, f.DirPath, rule.Name, PhysicalDisposalActionType.Disposal, string.Empty, "RM_Common_ObjectLevel_PhysicalFile",
                                JobDetailsStatus.Skipped, "RM_PRM_Disposal_SkipHoldFolder");
                            hasHoldOrLoanedFile = true;
                        }
                    }
                });
                RecordsHistoryService.AddPhysicalAudit(actionAudits);

                if (!hasHoldOrLoanedFile)
                {
                    box.RuleId = new Guid(rule.Id);
                    box.DisposalStatus = (int)SOApproveDBStatus.Archived;
                    box.RecordStatus = (int)RMRecordStatus.Destroyed;
                    box.DisposalActionTime = DateTime.UtcNow.Ticks;
                    box.DisposalDueDate = DateTime.UtcNow.Ticks;
                    var statusField = new ChoiceColumnValue() { Value = box.RecordStatus.ToString(), Name = I18NEntity.GetString("RM_PRM_PRE_Column_Status_Destroyed") };
                    box[MetaInfo.StatusId] = JsonConvert.SerializeObject(statusField);
                    //cancel hold
                    box.HoldStatus = false;
                    box.HoldType = 0;
                    box.HoldReleaseTime = 0;
                    box.HoldId = string.Empty;
                    box.HoldBy = string.Empty;
                    if (rule.PhysicalRule != null && rule.PhysicalRule.IsManualApproval)
                    {
                        box.ManualArchiveStatus = (int)Contract.Schedule.ActionStatus.Archiverd;
                    }
                    using (PerformanceScope pc = new PerformanceScope("PhyscialDisposalAction.BoxUpdate"))
                    {
                        box.Update(isUpdateManualProperties: true);
                    }
                    if (rule.PhysicalRule.IsManualApproval)
                    {
                        AddApproveHistory(box.Id);
                    }
                    logger.Info($"Disposal Box: {box.Id} ruleId: {rule.Id}");
                    SendReportHandler(box.Name, box.DirPath, rule.Name, PhysicalDisposalActionType.Disposal, String.Empty, "RM_Common_ObjectLevel_PhysicalBox", JobDetailsStatus.Successful);
                    var result = RecordsHistoryService.BuildPhysicalActionAuditForJob(box.Id, PhysicalActionType.Disposal, false, jobRunBy);
                    return result;
                }
                else
                {
                    logger.Info($"Skip Box: [{box.Id}] Reson:{"RM_PRM_Disposal_hasHoldFolder"}");
                    SendReportHandler(box.Name, box.DirPath, rule.Name, PhysicalDisposalActionType.Disposal, String.Empty, "RM_Common_ObjectLevel_PhysicalBox", JobDetailsStatus.Skipped, "RM_PRM_Disposal_hasHoldFolder");
                }
                return null;
            }
        }

        private bool IsRuleLastestFolderDisposalDueDateRule(IEnumerable<IPhysicalFile> folders, Rule rule)
        {
            var hasRuleLastestFolderDisposalDueDateRule = rule.PhysicalRule.Filters.Any(r => r.Rule is LastestFolderDisposalDueDateRule);
            
            return hasRuleLastestFolderDisposalDueDateRule;
        }


        private bool IsLastestFolderDisposalDueDateRule(Rule rule)
        {
            var hasRuleLastestFolderDisposalDueDateRule = rule.PhysicalRule.Filters.Any(r => r.Rule is LastestFolderDisposalDueDateRule);
            
            return hasRuleLastestFolderDisposalDueDateRule;
        }

        private bool IsSkipDetroyBox(IEnumerable<IPhysicalFile> folders)
        {
            return folders.Any(f => IsHoldOrLoaned(f.Id));
        }
        private string GetMessageDetail(IPhysicalFile currentFolder)
        {
            if (IsHoldOrLoaned(currentFolder.Id))
            {
                return "RM_PRM_Disposal_SkipHoldFolder";
            }
            return "RM_PRM_Disposal_hasHoldFolder";
        }

        private void HandleFolderWithLastestFolderDisposalDueDateRule(IPhysicalFile f, bool isSkipDetroyBox,Rule rule, SendReportHandler SendReportHandler)
        {
            if (isSkipDetroyBox)
            {
                var message = GetMessageDetail(f);
                logger.Info($"Skip Destroy box with rule is lastest folder disposal date. Folder {f.Name} is {message}.");
                SendReportHandler(f.Name, f.DirPath, rule.Name, PhysicalDisposalActionType.Disposal, string.Empty, "RM_Common_ObjectLevel_PhysicalFile", JobDetailsStatus.Skipped, message);
            }
            else
            {
                DisposalFile(f, rule, SendReportHandler, includeDeleteBlock: true);
            }
        }
        private bool IsHoldOrLoaned(Guid id) => ExplorerDao.IsRecordsHold(new List<Guid> { id }, mRunJobTime.Ticks) || RecordLoanAllianceDao.IsRecordsLoan(new List<Guid> { id }, mRunJobTime.Ticks);
        private void AddApproveHistory(Guid id)
        {
            var rec = GetRecord(id);
            rec.ManualApprovedStatus = (int)SOApproveDBStatus.Approved;
            var mainJobId = jobId.Split("_").First();
            PhysicalArchiverManualAction mManualAction = new PhysicalArchiverManualAction(mainJobId);
            mManualAction.ProcessApprovedOrRejectedRecord(rec);
        }
        private Record GetRecord(Guid id)
        {
            return ExplorerDao.GetFirstOrDefault(r => r.Id == id);
        }

        public PhysicalRecordActionAudit DisposalFile(IPhysicalFile file, Rule rule, SendReportHandler SendReportHandler, bool needRelatedRecord = true, bool includeDeleteBlock = true)
        {
            using (PerformanceScope pc0 = new PerformanceScope("PhyscialDisposalAction.DisposalFile", addToStatistics: true))
            {
                //string jobIds = string.Empty;
                bool deleteRelated = false;
                if (needRelatedRecord && rule.PhysicalRule != null
                    &&
                    (
                        //是否删除Related根据当前Rule状态走
                        //1.当前Rule不是Manual，勾选Related，按照当前Rule走
                        //2.当前Rule是Manual+DB中Physical是Delete状态时才删除 
                        (rule.PhysicalRule.RelatedRecordOption == RelatedRecordOption.Both && !rule.PhysicalRule.IsManualApproval)
                        ||
                        (rule.PhysicalRule.IsManualApproval /*&& rule.PhysicalRule.RelatedRecordOption == RelatedRecordOption.Both*/ && file.DeleteRelatedRecords == 1)
                    )
                )
                {
                    logger.Info("Current physical folder DisposeRelatedItems.Path:{0}.RelatedRecordInfo:{1}.", file.Id, file.RelatedRecords);
                    using (PerformanceScope pc = new PerformanceScope("PhyscialDisposalAction.DisposeRelatedItemsForArchiveAndRemove", addToStatistics: true))
                    {
                        //TODO Derek Related
                        //jobIds = DisposalRelatedItemUtility.DisposeRelatedItemsForArchiveAndRemove(callProcess, config, rule, file.RelatedRecordInfo, SendReportHandler);
                        deleteRelated = EnduserArchiverAction.DeleteRelatedData(RuleManagerService.ConvertToPhysicalRule(rule), file.Id, file.RelatedRecords, (int)SourceFlag.Physical, true, this.jobId);
                        logger.Info($"Delete related status is {deleteRelated}");
                    }
                    if (deleteRelated)
                    {
                        //RelatedPostActionObject relatedPostActionObject = new RelatedPostActionObject()
                        //{
                        //    PhysicalFile = file,
                        //    PhysicalRule = rule,
                        //    SendReportHandler = SendReportHandler,
                        //    RelatedPostAction = RealDisposalFile,
                        //    RelatedJobIds = jobIds
                        //};
                        //relatedKeyValuePairs.Add(jobIds, relatedPostActionObject);
                        return RealDisposalFile(file, rule, SendReportHandler);

                    }
                    else
                    {
                        logger.Info("Current Physical object has SP related object doesn't delete.Physical DirPath:{0}.", file.Id);
                        SendReportHandler(file.Name, file.DirPath, rule.Name, PhysicalDisposalActionType.Disposal, string.Empty, "RM_Common_ObjectLevel_PhysicalFile",
                         JobDetailsStatus.Failed, "StorageOptimization13_SOARRelatedRecordDeleteFailed");
                    }
                    return null;
                }
                return RealDisposalFile(file, rule, SendReportHandler);
            }
        }
        public PhysicalRecordActionAudit DisposalFile(List<OnPremRelatedResult> relatedResults, IPhysicalFile file, Rule rule, SendRelatedReportHandler SendRelatedReportHandler, bool needRelatedRecord = true)
        {
            using (PerformanceScope pc0 = new PerformanceScope("PhyscialDisposalAction.DisposalFile.SendRelatedReportHandler", addToStatistics: true))
            {
                return RealDisposalRelatedFile(relatedResults, file, rule, SendRelatedReportHandler);
            }
        }
        public string DisposeRelatedItemsForArchiveAndRemove(Rule currentRule, string recordRelatedValue, SendReportHandler SendReportHandler)
        {
            StringBuilder jobIds = new StringBuilder();
            // var request = new EndUserRequest();
            var relatedItems = RelatedRecordsUtility.GetRelatedProperties(recordRelatedValue);
            foreach (var itemInfo in relatedItems)
            {
                //老数据并没有存SourceFlag， 所以使用0 记录
                if (itemInfo.SourceFlag == (int)SourceFlag.SharePoint || itemInfo.SourceFlag == 0)
                {
                    #region sp logic
                    //if (Configuration.soArchiverQueryWorker != null && Configuration.soArchiverQueryWorker.TryGetCurrentVersionInTable(itemInfo.id))
                    //{
                    //    mLog.Info("Current related item fit rule in this job so skip it.FilePath:{0}.RuleID:{1}.", itemInfo.url, itemInfo.id);
                    //    continue;
                    //}
                    //string jobMetadata = string.Empty;
                    //string jobId = callProcess.GenerateJobId(ArchiveConstants.EndUserJob);
                    //string planId = "PLAN" + callProcess.GeneratePlanId();
                    //var endUserContract = request.GetEndUserArchiverContract(itemInfo, currentRule.Id, jobMetadata);
                    //var msg = Configuration.CloneArchiveMessageFromCurrentJob();
                    //msg.Action = ArchiverAction.ENDUSER_ARCHIVER_BACKUP_JOB_REQUEST;
                    //msg.EndUserArchiverMetaData = endUserContract.MetaData;
                    //msg.SubJobId = jobId + "_000";
                    //msg.RunDAOArchiverJobProduct = 1;
                    //msg.Job.Id = jobId;
                    //msg.Job.PlanId = planId;
                    //msg.Job.Scope = itemInfo.url;
                    //if (msg.ArchiverBackupRequest == null)
                    //{
                    //    msg.ArchiverBackupRequest = new GCommon.Contract.Media.TCPRequest.Backup.ArchiverBackupRequest();
                    //}
                    //msg.ArchiverBackupRequest.PlanId = planId;
                    //msg.ArchiverBackupRequest.ParentJobId = jobId;
                    //if (msg.ArchiverBackupRequest.IndexLogicalDevice == null)
                    //{
                    //    msg.ArchiverBackupRequest.IndexLogicalDevice = msg.PhysicalRecordsLogicalDevice;
                    //}
                    //msg.ArchiverBackupRequest.Rules = new Dictionary<string, GCommon.Contract.StorageOptimization.Object.Rule>();
                    ////Physical Related SP, StoragePolicyDto is null and need get encrypt/compress
                    //currentRule = AddRecordsGlobalStorageSettingsToPhysicalRule(currentRule, msg.RecordsGlobalStorageSettingsDto);
                    //msg.ArchiverBackupRequest.Rules.Add(currentRule.Id, currentRule);
                    //msg.ArchiverBackupRequest.ArchiverSiteInfoDto = new GCommon.Contract.Storage.Entity.ArchiverSiteInfoDto();
                    //var remoteNodeInfo = Configuration.GetRemoteNodeInfo(itemInfo.SiteUrl);
                    //if (remoteNodeInfo == null)
                    //{
                    //    mLog.Info("AOS RemoteNodeInfo is null when connect AOS.");
                    //    continue;
                    //}
                    //msg.ArchiverBackupRequest.ArchiverSiteInfoDto.SiteUrl = itemInfo.SiteUrl;
                    //msg.ArchiverBackupRequest.ArchiverSiteInfoDto.NewSiteUrl = itemInfo.SiteUrl;
                    ////Set site id here, but the site id may be wrong after backup and restore job
                    ////DAOAPIClient client = new DAOAPIClient(msg.TenantGroupId, msg.TenantGroupOwner);
                    //Guid mDAOSiteID = Guid.Empty;
                    //string mDAOGroupID = string.Empty;
                    //var daoSite = Configuration.GetRemoteSiteCollectionByDAO(itemInfo.SiteUrl);
                    ////Configuration.isRAJob ? Configuration.GetRemoteSiteCollectionByRecords(itemInfo.SiteUrl) : Configuration.GetRemoteSiteCollectionByDAO(itemInfo.SiteUrl);
                    //if (daoSite != null)
                    //{
                    //    mDAOSiteID = new Guid(daoSite.id);
                    //    mDAOGroupID = daoSite.parentId;
                    //    mLog.Info("DAO SiteID:{0},AOS SiteID:{1}.", mDAOSiteID, remoteNodeInfo.SiteID);
                    //    mLog.Info("DAO GroupID:{0},AOS GroupID:{1}.", mDAOGroupID, remoteNodeInfo.GroupId);
                    //}
                    //else
                    //{
                    //    mLog.Info("Can't get DAO SiteID:{0},AOS SiteID:{1}.", mDAOSiteID, remoteNodeInfo.SiteID);
                    //    mLog.Info("Can't get DAO GroupID:{0},AOS GroupID:{1}.", mDAOGroupID, remoteNodeInfo.GroupId);
                    //}
                    //msg.ArchiverBackupRequest.ArchiverSiteInfoDto.SiteId = mDAOSiteID.ToString();
                    //msg.ArchiverBackupRequest.ArchiverSiteInfoDto.WebApplicationUrl = remoteNodeInfo.GroupName;
                    //msg.ArchiverBackupRequest.ArchiverSiteInfoDto.NewWebApplicationUrl = remoteNodeInfo.GroupName;
                    //msg.ArchiverBackupRequest.ArchiverSiteInfoDto.WebApplicationId = mDAOGroupID;
                    //if (msg.ScheduledConfigs == null)
                    //{
                    //    msg.ScheduledConfigs = new List<RuleNodeContract>();
                    //    RuleNodeContract ruleNodeContract = new RuleNodeContract();
                    //    msg.ScheduledConfigs.Add(ruleNodeContract);
                    //}
                    //msg.ScheduledConfigs[0].RuleCollection.Rules = new Dictionary<int, GCommon.Contract.StorageOptimization.Object.Rule>();
                    //msg.ScheduledConfigs[0].RuleCollection.Rules.Add(1, currentRule);
                    //if (msg.ScheduledConfigs[0].BposInfo == null)
                    //{
                    //    msg.ScheduledConfigs[0].BposInfo = new GCommon.Contract.CentralAdmin.Object.BposInfo();
                    //    msg.ScheduledConfigs[0].BposInfo.UserAccountInfo = new GCommon.Contract.CentralAdmin.Object.BposUserAccountInfo();
                    //}
                    //msg.ScheduledConfigs[0].BposInfo.UserAccountInfo.Username = remoteNodeInfo.UserName;
                    //msg.ScheduledConfigs[0].BposInfo.UserAccountInfo.Domain = remoteNodeInfo.DomainName;
                    //msg.ScheduledConfigs[0].BposInfo.UserAccountInfo.TenantId = remoteNodeInfo.TenantId;
                    //msg.ScheduledConfigs[0].BposInfo.UserAccountInfo.AdminUrl = remoteNodeInfo.AdminUrl;
                    //msg.ScheduledConfigs[0].BposInfo.UserAccountInfo.AppCertContent = remoteNodeInfo.AppCertContent;
                    //msg.ScheduledConfigs[0].BposInfo.UserAccountInfo.AppCertSecretContent = remoteNodeInfo.AppCertSecretContent;
                    //msg.ScheduledConfigs[0].BposInfo.UserAccountInfo.AppCertSecret = remoteNodeInfo.AppCertSecret;
                    //msg.ScheduledConfigs[0].BposInfo.UserAccountInfo.AppClientId = remoteNodeInfo.AppClientId;
                    //msg.ScheduledConfigs[0].BposInfo.UserAccountInfo.AppId = remoteNodeInfo.AppId;
                    //msg.ScheduledConfigs[0].BposInfo.ConnectionType = remoteNodeInfo.BposConnectionType;
                    //msg.ScheduledConfigs[0].BposInfo.SiteUrl = itemInfo.SiteUrl;
                    //msg.ScheduledConfigs[0].BposInfo.TenantGroupId = msg.TenantGroupId;
                    //msg.ScheduledConfigs[0].SiteId = mDAOSiteID.ToString();
                    //msg.ScheduledConfigs[0].SiteUrl = itemInfo.SiteUrl;
                    //msg.ScheduledConfigs[0].FullPath = itemInfo.SiteUrl;
                    //msg.ScheduledConfigs[0].WebAppId = itemInfo.SiteId.ToString();
                    //mLog.Info("End User Job ArchiverMessage:" + jobId + ".  " + SerializerHelper.SerializeByDataContractSerializer(msg));
                    //if (!CheckRelatedRecordExist(msg))
                    //{
                    //    mLog.Warn("Related record:{0} does not exist in Share Point, skip this record.", itemInfo.url);
                    //    continue;
                    //}
                    //string archiveMsgFolder = AveEnv.AgentTempFolder + "\\" + msg.SubJobId;
                    //string msgFileName = "jobInfo.dat";
                    //string archiveMessagePath = archiveMsgFolder + "\\" + msgFileName;
                    //callProcess.WriteArchiveMsgToLocal(archiveMsgFolder, msgFileName, msg);
                    //mLog.Info("Current Agent machine SPStorageOptimizationMessageCenter.exe count is:{0}.", Process.GetProcessesByName("SPStorageOptimizationMessageCenter").Count());
                    //while (Process.GetProcessesByName("SPStorageOptimizationMessageCenter").Count() > 10)
                    //{
                    //    mLog.Info("Current machine SPStorageOptimizationMessageCenter.exe count over 10 and wait processor.");
                    //    Thread.Sleep(30 * 1000);
                    //}
                    //callProcess.StartSOMessageCenterProcess(ArchiveConstants.EndUserJob, archiveMessagePath);
                    //if (!string.IsNullOrEmpty(msg.SubJobId))
                    //{
                    //    jobIds.Append(msg.SubJobId);
                    //    jobIds.Append(";");
                    //}
                    //else
                    //{
                    //    throw new Exception("Cannot start remove related item job");
                    //}
                    #endregion
                }
                else if (itemInfo.SourceFlag == (int)SourceFlag.Physical)
                {
                    PhyscialDisposalAction action = new PhyscialDisposalAction(DateTime.UtcNow, null, this.jobRunBy);
                    //PhyBox = 9300,PhyFile = 9400,PhyRecord = 9500,
                    if (itemInfo.NodeType == 9400)
                    {
                        var r = ExplorerDao.GetPhysicalRecordById(itemInfo.id);
                        var file = new PhysicalFile(r);
                        if (!ExplorerDao.IsRecordsHold(new List<Guid>() { file.Id }, DateTime.UtcNow.Ticks))
                        {
                            action.DisposalFile(file, currentRule, SendReportHandler, false);
                        }
                        else
                        {
                            logger.Info("Current physical folder IsRecordsHold,DirPath:{0}.", file.Id);
                            SendReportHandler(file.Name, file.DirPath, "", PhysicalDisposalActionType.Disposal, string.Empty, "RM_Common_ObjectLevel_PhysicalFile",
                          JobDetailsStatus.Skipped, "RM_PRM_Disposal_SkipHoldFolder");
                        }
                    }
                    //当前不会关联box ，所以写在这为了以后可能用到
                    else if (itemInfo.NodeType == 9300)
                    {
                        var box = new PhysicalBox(itemInfo.id);
                        action.DisposalBox(box, currentRule, SendReportHandler);
                    }
                    else if (itemInfo.NodeType == 9500)
                    {
                        var r = ExplorerDao.GetPhysicalRecordById(itemInfo.id);
                        var record = new PhysicalRecord(r);
                        if (!ExplorerDao.IsRecordsHold(new List<Guid>() { record.ParentFile.Id }, DateTime.UtcNow.Ticks))
                        {
                            action.DisposalRecord(record, currentRule, false);
                            SendReportHandler(record.Name, record.DirPath, "", PhysicalDisposalActionType.Disposal, String.Empty, "RM_Common_ObjectLevel_PhysicalRecord", JobDetailsStatus.Successful, "");
                        }
                        else
                        {
                            logger.Info("Current physical folder IsRecordsHold,DirPath:{0}.", record.Id);
                            SendReportHandler(record.Name, record.DirPath, "", PhysicalDisposalActionType.Disposal, String.Empty, "RM_Common_ObjectLevel_PhysicalRecord", JobDetailsStatus.Skipped, "RM_PRM_Disposal_SkipHoldRecord");
                        }
                    }
                }
            }
            return jobIds.ToString().TrimEnd(';');
        }

        public PhysicalRecordActionAudit RealDisposalFile(IPhysicalFile file, Rule rule, SendReportHandler SendReportHandler)
        {
            using (PerformanceScope pc0 = new PerformanceScope("PhyscialDisposalAction.RealDisposalFile", addToStatistics: true))
            {
                var recordAudits = new List<PhysicalRecordActionAudit>();
                file.Records.ForEach(r => DisposalRecord(r, rule));
                RecordsHistoryService.AddPhysicalAudit(recordAudits);
                file.RuleId = new Guid(rule.Id);
                file.DisposalStatus = (int)SOApproveDBStatus.Archived;
                file.RecordStatus = (int)RMRecordStatus.Destroyed;
                file.DisposalActionTime = DateTime.UtcNow.Ticks;
                file.DisposalDueDate = DateTime.UtcNow.Ticks;
                var statusField = new ChoiceColumnValue() { Value = file.RecordStatus.ToString(), Name = I18NEntity.GetString("RM_PRM_PRE_Column_Status_Destroyed") };
                file[MetaInfo.StatusId] = JsonConvert.SerializeObject(statusField);
                //cancel hold
                file.HoldStatus = false;
                file.HoldType = 0;
                file.HoldReleaseTime = 0;
                file.HoldId = string.Empty;
                file.HoldBy = string.Empty;
                if (rule.PhysicalRule != null && rule.PhysicalRule.IsManualApproval)
                {
                    file.ManualArchiveStatus = (int)Contract.Schedule.ActionStatus.Archiverd;
                }

                using (PerformanceScope pc = new PerformanceScope("PhyscialDisposalAction.FolderUpdate", addToStatistics: true))
                {
                    file.Update(isUpdateManualProperties: true);
                }
                logger.Info($"Disposal File: {file.Id}. ruleId: {rule.Id}.RelatedRecordInfo:{file.RelatedRecords}.");
                //TODO Derek Related
                var utility = new RelatedRecordsUtility();
                var relatedInfos = RelatedRecordsUtility.GetRelatedProperties(file.RelatedRecords);
                using (PerformanceScope pc = new PerformanceScope("PhyscialDisposalAction.RemoveRelateColumnValue"))
                {
                    foreach (var relatedInfo in relatedInfos)
                    {
                        if (relatedInfo.SourceFlag == (int)SourceFlag.SharePointOnPrem)
                        {
                            logger.Info($"the folder related sponprem data has deleted,no need to remove related column value,id:{relatedInfo.id}");
                            continue;
                        }
                        utility.RemoveRelateColumnValue(relatedInfo, file.Id);
                    }
                }
                if (!string.IsNullOrEmpty(file.RelatedRecords))
                {
                    file.RelatedRecords = string.Empty;
                    file.RelatedRecordsCount = 0;
                    file.Update(true);
                }
                if (rule.PhysicalRule != null && rule.PhysicalRule.IsManualApproval)
                {
                    AddApproveHistory(file.Id);
                }
                SendReportHandler(file.Name, file.DirPath, rule.Name, PhysicalDisposalActionType.Disposal, String.Empty, "RM_Common_ObjectLevel_PhysicalFile", JobDetailsStatus.Successful);
                if (rule.PhysicalRule != null && rule.PhysicalRule.IsDeleteParentBox && file.ParentBox != null)
                {
                    var filesInBox = file.ParentBox.GetFiles(f => f.RecordStatus != (int)RMRecordStatus.Destroyed && f.RecordStatus != (int)RMRecordStatus.RMDeleted && f.RecordStatus != (int)RMRecordStatus.MoveOverwrite);
                    if (filesInBox.Count == 0)
                    {
                        file.ParentBox.RuleId = new Guid(rule.Id);
                        file.ParentBox.DisposalStatus = (int)SOApproveDBStatus.Archived;
                        file.ParentBox.RecordStatus = (int)RMRecordStatus.Destroyed;
                        file.ParentBox.DisposalActionTime = DateTime.UtcNow.Ticks;
                        file.ParentBox.DisposalDueDate = DateTime.UtcNow.Ticks;
                        file.ParentBox[MetaInfo.StatusId] = JsonConvert.SerializeObject(statusField);
                        //cancel hold
                        file.ParentBox.HoldStatus = false;
                        file.ParentBox.HoldType = 0;
                        file.ParentBox.HoldReleaseTime = 0;
                        file.ParentBox.HoldId = string.Empty;
                        file.ParentBox.HoldBy = string.Empty;
                        var actionAudit = RecordsHistoryService.BuildPhysicalActionAuditForJob(file.ParentBox.Id, PhysicalActionType.Disposal, false, jobRunBy);
                        RecordsHistoryService.AddPhysicalAudit([actionAudit]);
                        using (PerformanceScope pc = new PerformanceScope("PhyscialDisposalAction.BoxUpdate", addToStatistics: true))
                        {
                            file.ParentBox.Update();
                        }
                        logger.Info($"Disposal Box: {file.ParentBox.Id} ruleId: {rule.Id}");
                        SendReportHandler(file.ParentBox.Name, file.ParentBox.DirPath, rule.Name, PhysicalDisposalActionType.Disposal, string.Empty, "RM_Common_ObjectLevel_PhysicalBox", JobDetailsStatus.Successful);
                    }
                }
                var result = RecordsHistoryService.BuildPhysicalActionAuditForJob(file.Id, PhysicalActionType.Disposal, false, jobRunBy);
                return result;
            }
        }
        public PhysicalRecordActionAudit RealDisposalRelatedFile(List<OnPremRelatedResult> relatedResults, IPhysicalFile file, Rule rule, SendRelatedReportHandler SendReportHandler)
        {
            using (PerformanceScope pc0 = new PerformanceScope("PhyscialDisposalAction.RealRelatedDisposalFile", addToStatistics: true))
            {
                var recordAudits = new List<PhysicalRecordActionAudit>();
                file.Records.ForEach(r => DisposalRecord(r, rule));
                RecordsHistoryService.AddPhysicalAudit(recordAudits);
                file.RuleId = new Guid(rule.Id);
                file.DisposalStatus = (int)SOApproveDBStatus.Archived;
                file.RecordStatus = (int)RMRecordStatus.Destroyed;
                file.DisposalActionTime = DateTime.UtcNow.Ticks;
                file.DisposalDueDate = DateTime.UtcNow.Ticks;
                var statusField = new ChoiceColumnValue() { Value = file.RecordStatus.ToString(), Name = I18NEntity.GetString("RM_PRM_PRE_Column_Status_Destroyed") };
                file[MetaInfo.StatusId] = JsonConvert.SerializeObject(statusField);
                //cancel hold
                file.HoldStatus = false;
                file.HoldType = 0;
                file.HoldReleaseTime = 0;
                file.HoldId = string.Empty;
                file.HoldBy = string.Empty;
                using (PerformanceScope pc = new PerformanceScope("PhyscialDisposalAction.FolderUpdate", addToStatistics: true))
                {
                    file.Update(isUpdateManualProperties: true);
                }
                logger.Info($"Disposal Related File: {file.Id}. ruleId: {rule.Id}.RelatedRecordInfo:{file.RelatedRecords}.");
                //TODO Derek Related
                var utility = new RelatedRecordsUtility();
                var relatedInfos = RelatedRecordsUtility.GetRelatedProperties(file.RelatedRecords);
                using (PerformanceScope pc = new PerformanceScope("PhyscialDisposalAction.RemoveRelateColumnValue"))
                {
                    foreach (var relatedInfo in relatedInfos)
                    {
                        utility.RemoveRelateColumnValue(relatedInfo, file.Id);
                    }
                }
                if (!string.IsNullOrEmpty(file.RelatedRecords))
                {
                    file.RelatedRecords = string.Empty;
                    file.RelatedRecordsCount = 0;
                    file.Update(true);
                }
                if (rule.PhysicalRule != null && rule.PhysicalRule.IsManualApproval)
                {
                    AddApproveHistory(file.Id);
                }
                SendReportHandler(relatedResults,file.Name, file.DirPath, "RM_Common_ObjectLevel_PhysicalFile", JobDetailsStatus.Successful);
                var result = RecordsHistoryService.BuildPhysicalActionAuditForJob(file.Id, PhysicalActionType.Disposal, false, jobRunBy);
                return result;
            }
        }
        public void DisposalRecord(IPhysicalRecord record, Rule rule, bool needRelatedRecord = true)
        {
            using (PerformanceScope pc0 = new PerformanceScope("PhyscialDisposalAction.DisposalRecord", addToStatistics: true))
            {
                //if (needRelatedRecord && rule.PhysicalRule != null && rule.PhysicalRule.RelatedRecordOption == RelatedRecordOption.Both)
                //{
                //    string jobIds = DisposalRelatedItemUtility.DisposeRelatedItems(callProcess, config, rule, record.RelatedRecordInfo, null);
                //}
                record.RuleId = new Guid(rule.Id);
                record.DisposalStatus = (int)SOApproveDBStatus.Archived;
                record.RecordStatus = (int)RMRecordStatus.Destroyed;
                record.DisposalActionTime = DateTime.UtcNow.Ticks;
                var statusField = new ChoiceColumnValue() { Value = record.RecordStatus.ToString(), Name = I18NEntity.GetString("RM_PRM_PRE_Column_Status_Destroyed") };
                record[MetaInfo.StatusId] = JsonConvert.SerializeObject(statusField);
                using (PerformanceScope pc = new PerformanceScope("PhyscialDisposalAction.RecordUpdate", addToStatistics: true))
                {
                    record.Update();
                }
                logger.Info($"Disposal Record: {record.Id}. ruleId: {rule.Id}.RelatedRecordInfo:{record.RelatedRecords}.");
                //TODO Derek
                var utility = new RelatedRecordsUtility();
                var relatedInfos = RelatedRecordsUtility.GetRelatedProperties(record.RelatedRecords);
                using (PerformanceScope pc = new PerformanceScope("PhyscialDisposalAction.RemoveRecordRelateColumnValue"))
                {
                    foreach (var relatedInfo in relatedInfos)
                    {
                        utility.RemoveRelateColumnValue(relatedInfo, record.Id);
                    }
                }
                if (!string.IsNullOrEmpty(record.RelatedRecords))
                {
                    record.RelatedRecords = string.Empty;
                    record.RelatedRecordsCount = 0;
                    using (PerformanceScope pc = new PerformanceScope("PhyscialDisposalAction.RecordUpdate", addToStatistics: true))
                    {
                        record.Update(true);
                    }
                }
            }
        }

        public void EmptyBoxRuleInfo(IPhysicalBox box)
        {
            using (PerformanceScope pc = new PerformanceScope("PhyscialDisposalAction.EmptyBoxRuleInfo", addToStatistics: true))
            {
                box.Files.ForEach(f => EmptyFileRuleInfo(f));
                box.RuleId = Guid.Empty;
                //box.RecordStatus = 1;
                box.DisposalStatus = (int)SOApproveDBStatus.None;
                box.DisposalActionTime = 0;
                box.Update();
            }
            logger.Info($"Empty ruleinfo box dir: {box.Id}");
        }

        public void EmptyFileRuleInfo(IPhysicalFile file)
        {
            using (PerformanceScope pc = new PerformanceScope("PhyscialDisposalAction.EmptyFileRuleInfo", addToStatistics: true))
            {
                file.Records.ForEach(f => EmptyRecordRuleInfo(f));
                file.RuleId = Guid.Empty;
                //file.RecordStatus = 1;
                file.DisposalStatus = (int)SOApproveDBStatus.None;
                file.DisposalActionTime = 0;
                file.Update();
            }
            logger.Info($"Empty ruleinfo file dir: {file.Id}");
        }

        public void EmptyRecordRuleInfo(IPhysicalRecord record)
        {
            using (PerformanceScope pc = new PerformanceScope("PhyscialDisposalAction.EmptyRecordRuleInfo", addToStatistics: true))
            {
                record.RuleId = Guid.Empty;
                //record.RecordStatus = 1;
                record.DisposalStatus = (int)SOApproveDBStatus.None;
                record.DisposalActionTime = 0;
                record.Update();
            }
            logger.Info($"Empty ruleinfo record dir: {record.Id}");
        }

        private AvePoint.RA.Contract.Object.RealTime.PhysicalMoveHoldConflictOption ConvertHoldConflictOption2lMoveHoldConflictOption(PhysicalHoldConflictOption physicalHoldConflict)
        {
            return physicalHoldConflict switch
            {
                PhysicalHoldConflictOption.UseDesDefinedHoldSetting => AvePoint.RA.Contract.Object.RealTime.PhysicalMoveHoldConflictOption.UseDest,
                PhysicalHoldConflictOption.CompareHoldSetting => AvePoint.RA.Contract.Object.RealTime.PhysicalMoveHoldConflictOption.UseLongest,
                _ => AvePoint.RA.Contract.Object.RealTime.PhysicalMoveHoldConflictOption.None
            };
        }

        private AvePoint.RA.Contract.Object.RealTime.NameConflictOption ConvertHoldConflictOption2lMoveHoldConflictOption(DAContract.ConflictOption conflictOption)
        {
            return conflictOption switch
            {
                DAContract.ConflictOption.Overwrite => AvePoint.RA.Contract.Object.RealTime.NameConflictOption.Overwrite,
                DAContract.ConflictOption.Skip => AvePoint.RA.Contract.Object.RealTime.NameConflictOption.Skip,
                DAContract.ConflictOption.AppendByName => AvePoint.RA.Contract.Object.RealTime.NameConflictOption.Rename,
                _ => AvePoint.RA.Contract.Object.RealTime.NameConflictOption.Skip
            };
        }

        public async System.Threading.Tasks.Task MoveBoxAsync(IPhysicalBox box, Guid locationId, string ruleName, DAContract.ConflictOption conflictOption, SendReportHandler SendReportHandler, PhysicalHoldConflictOption physicalHoldConflict)
        {
            using (PerformanceScope pc0 = new PerformanceScope("PhyscialDisposalAction.MoveBox", addToStatistics: true))
            {
                RMPhysicalExplorer.InitDestinationSetting(new Contract.Object.RealTime.PhysicalMoveOption()
                {
                    HoldConflictOption = ConvertHoldConflictOption2lMoveHoldConflictOption(physicalHoldConflict),
                    LocationId = locationId.ToString(),
                    NameConflictOption = ConvertHoldConflictOption2lMoveHoldConflictOption(conflictOption)
                });
                await RMPhysicalExplorer.MoveBoxAsync(box, ruleName);
            }
        }


        public async System.Threading.Tasks.Task MoveFileAsync(IPhysicalFile file, Guid boxId, Guid locationId, string fullPath, string ruleName, DAContract.ConflictOption conflictOption, SendReportHandler SendReportHandler, PhysicalHoldConflictOption physicalHoldConflict)
        {
            using (PerformanceScope pc = new PerformanceScope("PhyscialDisposalAction.MoveFile", addToStatistics: true))
            {
                RMPhysicalExplorer.InitDestinationSetting(new Contract.Object.RealTime.PhysicalMoveOption()
                {
                    HoldConflictOption = ConvertHoldConflictOption2lMoveHoldConflictOption(physicalHoldConflict),
                    LocationId = locationId.ToString(),
                    NameConflictOption = ConvertHoldConflictOption2lMoveHoldConflictOption(conflictOption),
                    BoxId = boxId.ToString()
                });
                string destinationPath = boxId != Guid.Empty ? fullPath : string.Empty;
                await RMPhysicalExplorer.MoveFileAsync(file, boxId, destinationPath, ruleName);
            }
        }

        public void PendingBox(IPhysicalBox box, Rule rule, SendReportHandler SendReportHandler)
        {
            using (PerformanceScope pc0 = new PerformanceScope("PhyscialDisposalAction.PendingBox", addToStatistics: true))
            {
                box.Files.ForEach(f => EmptyFileRuleInfo(f));//box.Files.ForEach(f => PendingFile(f, rule));
                //box.RuleId = new Guid(rule.Id);
                //box.DisposalStatus = (int)SOApproveDBStatus.WaitingApprove;
                //box.ExportToManual = false;
                ////box.RecordStatus = 1;
                //box.DisposalActionTime = 0;
                //box.Update();
                logger.Info($"Pending Box: {box.Id} ruleId:{rule.Name}");

                SendReportHandler(box.Name, box.DirPath, rule.Name, PhysicalDisposalActionType.Pending, String.Empty, "RM_Common_ObjectLevel_PhysicalBox", JobDetailsStatus.Successful);
            }
        }

        public void PendingFile(IPhysicalFile file, Rule rule, SendReportHandler SendReportHandler)
        {
            using (PerformanceScope pc = new PerformanceScope("PhyscialDisposalAction.PendingFile", addToStatistics: true))
            {
                file.Records.ForEach(r => EmptyRecordRuleInfo(r));//file.Records.ForEach(r => PendingRecord(r, rule));
                //file.RuleId = new Guid(rule.Id);
                //file.DisposalStatus = (int)SOApproveDBStatus.WaitingApprove;
                //file.ExportToManual = false;
                //file.RecordStatus = 1;
                //if (rule.PhysicalRule.IsManualApproval && rule.PhysicalRule.RelatedRecordOption == RelatedRecordOption.Both)
                //{
                //    file.DeleteRelatedRecords = (int)RelatedRecordOption.Both;
                //}
                //file.DisposalActionTime = 0;
                //using (PerformanceScope pc0 = new PerformanceScope("PhyscialDisposalAction.FolderUpdate", addToStatistics: true))
                //{
                //    file.Update();
                //}
                logger.Info($"Pending file :{file.Id} ruleId:{rule.Name}");

                SendReportHandler(file.Name, file.DirPath, rule.Name, PhysicalDisposalActionType.Pending, String.Empty, "RM_Common_ObjectLevel_PhysicalFile", JobDetailsStatus.Successful);
            }
        }

        public void PendingRecord(IPhysicalRecord record, Rule rule)
        {
            using (PerformanceScope pc = new PerformanceScope("PhyscialDisposalAction.PendingRecord", addToStatistics: true))
            {
                record.RuleId = new Guid(rule.Id);
                record.DisposalStatus = (int)SOApproveDBStatus.WaitingApprove;
                record.ExportToManual = false;
                //record.RecordStatus = 1;
                if (rule.PhysicalRule.IsManualApproval && rule.PhysicalRule.RelatedRecordOption == RelatedRecordOption.Both)
                {
                    record.DeleteRelatedRecords = (int)RelatedRecordOption.Both;
                }
                record.DisposalActionTime = 0;
                using (PerformanceScope pc0 = new PerformanceScope("PhyscialDisposalAction.RecordUpdate", addToStatistics: true))
                {
                    record.Update();
                }
                logger.Info($"Pending Record :{record.Id} ruleId:{rule.Name}");
            }
        }

        public void CalculateDisposalDateForFolder(IPhysicalFile folder, PhysicalRuleEngine engine, ObjectInfoBase fileFilterObj, SendReportHandler sendReportHandler)
        {
            string disposalDueDate = string.Empty;

            var rule = engine.CheckDueDisposalRule(folder, fileFilterObj, ref disposalDueDate);
            if(rule != null)
            {
                var disposalDueDateLong = DueDateUtil.ConvertStringDueDate2Long(disposalDueDate);
                folder.DisposalDueDate = disposalDueDateLong;
                folder.PreviousDisposalDueDate = disposalDueDateLong;
                folder.RuleId = new Guid(rule.Id);
            }

            using (var pc = new PerformanceScope("CalculateDisposalDateForFolder.FolderUpdate", addToStatistics: true))
            {
                folder.Update();
            }
            //var actionAudit = RecordsHistoryService.BuildPhysicalActionAuditForJob(folder.Id, PhysicalActionType.CalculateDisposalDate, false, jobRunBy);
            //RecordsHistoryService.AddPhysicalAudit([actionAudit]);
            sendReportHandler(folder.Name, folder.DirPath, rule.Name, PhysicalDisposalActionType.CalculateDisposalDate, String.Empty, "RM_Common_ObjectLevel_PhysicalFile", JobDetailsStatus.Successful);
            logger.Info($"Update Record: {folder.Id} successful");
        }

        //public void Dispose()
        //{
        //    if (relatedKeyValuePairs.Count > 0)
        //    {
        //        using (AveAppendableTaskExecutor taskExecutor = new AveAppendableTaskExecutor(20))
        //        {
        //            taskExecutor.StartExecute();
        //            foreach (var item in relatedKeyValuePairs)
        //            {
        //                taskExecutor.AddTask(() =>
        //                {
        //                    RelatedPostAction(item.Key, item.Value);
        //                });
        //            }
        //            logger.Info($"Add items to task executor finished.");
        //            if (!taskExecutor.WaitForAllTasks(1000 * 60 * 30))
        //            {
        //                //todo: handle timeout
        //                logger.Error($"Time out exception.");
        //            }
        //            logger.Info($"Finish Related Post Action.");
        //        }
        //    }
        //}

        //TODO Derek
        //public void RelatedPostAction(string jobIds, RelatedPostActionObject relatedPostActionObject)
        //{
        //    bool needDeleteSourceFile = true;
        //    var jobIdList = (jobIds.Split(';').ToList().Where(r => !string.IsNullOrEmpty(r))).ToList();
        //    logger.Info("Current Physical object has SP Related Record Job,ID Collection:{0}.DirPath:{1}.", jobIds, relatedPostActionObject.PhysicalFile.Id);
        //    List<string> needRemoveJobID = new List<string>();
        //    int retryCount = 120;
        //    while (retryCount > 0)
        //    {
        //        logger.Info("needRemoveJobID count is:{0}.", needRemoveJobID.Count);
        //        foreach (string jobId in needRemoveJobID)
        //        {
        //            jobIdList.Remove(jobId);
        //        }
        //        needRemoveJobID.Clear();
        //        if (jobIdList.Count == 0)
        //        {
        //            logger.Info("jobIdList count is 0.");
        //            break;
        //        }
        //        foreach (string jobId in jobIdList)
        //        {
        //            var jobState = LoadJobStatus(jobId);
        //            if ((jobState == JobStates.InProgress || jobState == JobStates.Waiting))
        //            {
        //                logger.Info(string.Format("Job id is : {0}, status is : {1},continue current foreach.", jobId, jobState.ToString()));
        //                continue;
        //            }
        //            else if (jobState == JobStates.Finished)
        //            {
        //                logger.Info(string.Format("Delete sp related item job : {0} is finished.", jobId));
        //            }
        //            else
        //            {
        //                needDeleteSourceFile = false;
        //                //有一个删除related document job 失败，就return，不删除这个document,先打出一些必要的log，然后return
        //                if (jobState == JobStates.Failed || jobState == JobStates.FinishedException || jobState == JobStates.Skiped || jobState == JobStates.Stopped)
        //                {
        //                    logger.Warn("The related item deleted failed, skip delete the current document,JobID:{0}, JobState:{1}.", jobId, jobState.ToString());
        //                }
        //            }
        //            needRemoveJobID.Add(jobId);
        //            EndUserJobReortOperation jobReport = new EndUserJobReortOperation(jobId);
        //            List<JobDetail> jobDetails = jobReport.GetReports();
        //            foreach (JobDetail jobdetail in jobDetails)
        //            {
        //                //only add delete detail to physical.
        //                if (jobdetail.Remark12 == "Delete")
        //                {
        //                    JobDetailsStatus status = JobDetailsStatus.Succeed;
        //                    switch (jobdetail.Status)
        //                    {
        //                        case 0:
        //                            status = JobDetailsStatus.Succeed;
        //                            jobdetail.Message = "StorageOptimization13_SOARRelatedRecordDeleteSuccess";

        //                            break;
        //                        case 1:
        //                            status = JobDetailsStatus.Failed;
        //                            jobdetail.Message = "StorageOptimization13_SOARRelatedRecordDeleteFailed";
        //                            break;
        //                        case 2:
        //                            status = JobDetailsStatus.Skipped;
        //                            jobdetail.Message = "StorageOptimization13_SOARRelatedRecordDeleteSkipped";
        //                            break;
        //                        default:
        //                            break;
        //                    }
        //                    if (Convert.ToInt32(Enum.Parse(typeof(CacheNodeType), jobdetail.Type)) == 10000)
        //                    {
        //                        string realUrl = jobdetail.SrcURL.Replace('\\', '/');
        //                        relatedPostActionObject.SendReportHandler(realUrl.Substring(realUrl.LastIndexOf('/') + 1), realUrl, PhysicalDisposalActionType.Disposal, "", Convert.ToInt32(Enum.Parse(typeof(CacheNodeType), jobdetail.Type)), status, jobdetail.Message);
        //                    }
        //                }
        //            }
        //        }
        //        Thread.Sleep(15 * 1000);
        //        retryCount--;
        //    }
        //    if (!needDeleteSourceFile)
        //    {
        //        logger.Info("Current Physical ojbect has SP related object doesn't delete.Physical DirPath:{0}.", relatedPostActionObject.PhysicalFile.Id);
        //        relatedPostActionObject.SendReportHandler(relatedPostActionObject.PhysicalFile.Name, relatedPostActionObject.PhysicalFile.DirPath, PhysicalDisposalActionType.Disposal, String.Empty, (int)PhysicalNodeLevel.PhysicalFile, JobDetailsStatus.Failed);
        //    }
        //    else
        //    {
        //        relatedPostActionObject.RelatedPostAction(relatedPostActionObject.PhysicalFile, relatedPostActionObject.PhysicalRule, relatedPostActionObject.SendReportHandler);
        //    }
        //}
    }

    public class MetaInfo
    {
        public const string StatusId = "eb4e9ab7-c939-425b-9e29-235236c9ce5b";
        public const string HomelocationId = "d2568d7d-4891-46d2-8eb2-2e8c032a41bf";
        public const string Classification = "aedcf21f-dfdb-41d3-935a-5c5859187754";
        public const string NameOrTitleId = "de5e99cb-4fb4-4e25-b732-a1dce71dd048";
    }
}
