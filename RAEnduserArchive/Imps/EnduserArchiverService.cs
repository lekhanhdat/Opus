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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.OnPremiseSharePoint;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.SharePointOnPrem;
using AvePoint.RA.RAPhysical.API;
using AvePoint.RA.RAPhysical.Disposal;
using AvePoint.RA.RAPhysical.Disposal.PhysicalDisposalActionImps;
using AvePoint.RA.SharePoint.Archiver;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using RAArchiverCommon;
using RAArchiverCommon.DestructionCache;
using RAExportCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAEnduserArchive.Imps
{
    public class RelativeDataArchiverService : IRelativeDataArchiverService
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(PhyscialDisposalAction));
        private IExplorerDao ExplorerDao = new ExplorerDao();
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

        private IRecordLoanAllianceDao mRecordLoanAllianceDao;
        public IRecordLoanAllianceDao RecordLoanAllianceDao
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

        private IRecordsHistoryService RecordsHistoryService => PlatformWindsorManager.GetService<IRecordsHistoryService>();

        private ConcurrentDictionary<string, bool> DeleteStatusCache = new ConcurrentDictionary<string, bool>();

        private Rule CurrentRule = null;
        /// <summary>
        /// all realted data archived success, return true
        /// </summary>
        /// <param name="currentRule"></param>
        /// <param name="recordRelatedValue"></param>
        /// <param name="sourceFlag"></param>
        /// <returns></returns>
        public bool DeleteRelatedData(Rule currentRule, Guid nodeId, string recordRelatedValue, int sourceFlag, bool isRAJob, string jobid, JobRunBy jobRunBy = JobRunBy.Control)
        {
            bool success = true;
            this.CurrentRule = currentRule;
            var relatedItems = RelatedRecordsUtility.GetRelatedProperties(recordRelatedValue);
            List<AvePoint.RA.Contract.RMRelatedRecord.RMRelatedItemInfo> spItemInfos = new List<AvePoint.RA.Contract.RMRelatedRecord.RMRelatedItemInfo>();
            foreach (var itemInfo in relatedItems)
            {
                try
                {
                    if (sourceFlag == (int)SourceFlag.Physical && itemInfo.SiteUrl != null)
                    {
                        SOArchiverJobInfoStatistics.Instance.InitInstance(jobid, itemInfo.SiteUrl, Contract.JobMonitor.JobType.PhysicalRecordsDisposal, itemInfo.SiteId.ToString());
                    }
                    //老数据并没有存SourceFlag， 所以使用0 记录
                    if (itemInfo.SourceFlag == (int)SourceFlag.SharePoint || itemInfo.SourceFlag == 0)
                    {
                        #region spo
                        //if (Configuration.soArchiverQueryWorker != null && Configuration.soArchiverQueryWorker.TryGetCurrentVersionInTable(itemInfo.id))
                        //{
                        //    mLog.Info("Current related item fit rule in this job so skip it.FilePath:{0}.RuleID:{1}.", itemInfo.url, itemInfo.id);
                        //    continue;
                        //}
                        //string jobMetadata = string.Empty;
                        //string jobId = callProcess.GenerateJobId(ArchiveConstants.EndUserJob);
                        //string planId = "PLAN" + callProcess.GeneratePlanId();
                        //var relativeDataContract = request.GetEndUserArchiverContract(itemInfo, currentRule.Id, jobMetadata);
                        //var msg = Configuration.CloneArchiveMessageFromCurrentJob();
                        //msg.Action = ArchiverAction.ENDUSER_ARCHIVER_BACKUP_JOB_REQUEST;
                        //msg.EndUserArchiverMetaData = relativeDataContract.MetaData;
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
                        //start multiple threads to archive spo data
                        spItemInfos.Add(itemInfo);
                        if (string.IsNullOrWhiteSpace(currentRule.StoragePolicyId) || currentRule.StoragePolicyId.Equals(Guid.Empty.ToString()))
                        {
                            logger.Info("Will use default storage.");
                        }
                    }
                    else if (itemInfo.SourceFlag == (int)SourceFlag.Physical)
                    {
                        PhyscialDisposalAction action = new PhyscialDisposalAction(DateTime.UtcNow, jobid, jobRunBy);
                        //PhyBox = 9300,PhyFile = 9400,PhyRecord = 9500,
                        if (itemInfo.NodeType == 9400)
                        {
                            var r = ExplorerDao.GetPhysicalRecordById(itemInfo.id);
                            var file = new PhysicalFile(r);
                            if (!IsRecordsHold(file, DateTime.UtcNow.Ticks))
                            {
                                var actionAudit = action.DisposalFile(file, currentRule, sourceFlag == (int)SourceFlag.Physical ? SendPhysicalJobDetail : SendSPJobDetail, false);
                                if (actionAudit != null)
                                {
                                    RecordsHistoryService.AddPhysicalAudit([actionAudit]);
                                }
                            }
                            else
                            {
                                logger.Info("Current physical folder IsRecordsHold,DirPath:{0}.", file.Id);
                                if (sourceFlag == (int)SourceFlag.Physical)
                                {
                                    SendPhysicalJobDetail(file.Name, file.DirPath, PhysicalDisposalActionType.Disposal, string.Empty, "RM_Common_ObjectLevel_PhysicalFile", JobDetailsStatus.Skipped, "RM_PRM_Disposal_SkipHoldFolder");
                                }
                                else
                                {
                                    SendSPJobDetail(file.Name, file.DirPath, PhysicalDisposalActionType.Disposal, String.Empty, "RM_Common_ObjectLevel_PhysicalFile", JobDetailsStatus.Skipped, "RM_PRM_Disposal_SkipHoldFolder");
                                }
                            }
                        }
                        //当前不会关联box ，所以写在这为了以后可能用到
                        else if (itemInfo.NodeType == 9300)
                        {
                            var box = new PhysicalBox(itemInfo.id);
                            action.DisposalBox(box, currentRule, sourceFlag == (int)SourceFlag.Physical ? SendPhysicalJobDetail : SendSPJobDetail);
                        }
                        else if (itemInfo.NodeType == 9500)
                        {
                            var r = ExplorerDao.GetPhysicalRecordById(itemInfo.id);
                            var record = new PhysicalRecord(r);
                            if (!IsRecordsHold(record.ParentFile, DateTime.UtcNow.Ticks))
                            {
                                action.DisposalRecord(record, currentRule, false);
                                if (sourceFlag == (int)SourceFlag.Physical)
                                {
                                    SendPhysicalJobDetail(record.Name, record.DirPath, PhysicalDisposalActionType.Disposal, String.Empty, "RM_Common_ObjectLevel_PhysicalRecord", JobDetailsStatus.Successful, "");
                                }
                                else
                                {
                                    SendSPJobDetail(record.Name, record.DirPath, PhysicalDisposalActionType.Disposal, String.Empty, "RM_Common_ObjectLevel_PhysicalRecord", JobDetailsStatus.Successful, "");
                                }
                            }
                            else
                            {
                                logger.Info("Current physical folder IsRecordsHold,DirPath:{0}.", record.Id);
                                if (sourceFlag == (int)SourceFlag.Physical)
                                {
                                    SendPhysicalJobDetail(record.Name, record.DirPath, PhysicalDisposalActionType.Disposal, String.Empty, "RM_Common_ObjectLevel_PhysicalRecord", JobDetailsStatus.Skipped, "RM_PRM_Disposal_SkipHoldRecord");
                                }
                                else
                                {
                                    SendSPJobDetail(record.Name, record.DirPath, PhysicalDisposalActionType.Disposal, String.Empty, "RM_Common_ObjectLevel_PhysicalRecord", JobDetailsStatus.Skipped, "RM_PRM_Disposal_SkipHoldRecord");
                                }
                            }
                        }
                    }
                    else if (itemInfo.SourceFlag == (int)SourceFlag.SharePointOnPrem)
                    {
                        using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverDeletion.DeleteRelatedSPOnprem"))
                        {
                            logger.Info("start delete related onprem data");
                            var r = ExplorerDao.GetRecordByIds(new List<Guid>() { HashCodeHelper.StringHash(itemInfo.SiteId.ToString().ToLowerInvariant() + itemInfo.id.ToString().ToLowerInvariant()) });
                            if (r != null && r.Count > 0)
                            {
                                var record = r.FirstOrDefault();
                                if (record.HoldStatus && record.HoldReleaseTime > DateTime.UtcNow.Ticks)
                                {
                                    logger.Info($"do not delete related onprem data,it is hold,{itemInfo.id}");
                                    SendSPOnpremJobDetail(itemInfo.name, itemInfo.url, PhysicalDisposalActionType.Disposal, String.Empty, "RM_MA_Document", JobDetailsStatus.Skipped, "RM_FS_ReportSkip_OnHold");
                                    continue;
                                }
                            }
                            var result = SharePointOnPremClient.DisposeSPItems(new Hybrid.Contract.SignalR.SharePointOnPremDisposalArgs()
                            {
                                SiteUrl = itemInfo.SiteUrl,
                                SiteId = itemInfo.SiteId,
                                WebId = itemInfo.WebId,
                                ListId = itemInfo.ListId,
                                ItemId = itemInfo.id,
                            }).GetAwaiter().GetResult();
                            if (result.IsSuccess)
                            {
                                logger.Info($"delete related onprem data success,id:{itemInfo.id},will update record");
                                if (r != null && r.Count > 0)
                                {
                                    var record = r.FirstOrDefault();
                                    ExplorerDao.UpdateRecordState(record, (int)RMRecordStatus.Destroyed);
                                }
                                SendSPOnpremJobDetail(itemInfo.name, itemInfo.url, PhysicalDisposalActionType.Disposal, String.Empty, "RM_MA_Document", JobDetailsStatus.Successful);
                            }
                            else
                            {
                                logger.Warn($"delete related onprem data failed,id:{itemInfo.id},will not update record,message:{result.ErrorMessage}");
                                SendSPOnpremJobDetail(itemInfo.name, itemInfo.url, PhysicalDisposalActionType.Disposal, String.Empty, "RM_MA_Document", JobDetailsStatus.Failed, result.ErrorMessage);
                                success = false;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"Error occurred while deleting related data. Id:{itemInfo.id} DataSource;{itemInfo.SourceFlag} Error:{e.ToString()}"); ;
                    success = false;
                    break;
                }
            }

            if (success && spItemInfos.IsNotNullOrEmpty())
            {
                using (AveAppendableTaskExecutor taskExecutor = new AveAppendableTaskExecutor(5))
                {
                    taskExecutor.StartExecute();
                    foreach (var itemInfo in spItemInfos)
                    {
                        taskExecutor.AddTask(async () =>
                        {
                            try
                            {
                                await DisposalSPDataAsync(itemInfo, currentRule, nodeId, isRAJob, jobid, sourceFlag);
                            }
                            catch(Exception e)
                            {
                                logger.Error(e.ToString());
                            }
                        });
                    }
                    if (!taskExecutor.WaitForAllTasks(1000 * 60 * 30))
                    {
                        //todo: handle timeout
                        success = false;
                        logger.Error($"Time out exception.");
                    }
                }
                success = !DeleteStatusCache.Any(v => !v.Value);
            }
            return success;
        }
        public List<OnPremRelatedResult> DeleteSPOnpremRelatedPhysicalData(Rule currentRule,  string recordRelatedValue,string jobid, JobRunBy jobRunBy = JobRunBy.Control)
        {
            bool success = true;
            this.CurrentRule = currentRule;
            var relatedItems = RelatedRecordsUtility.GetRelatedProperties(recordRelatedValue);
            List<OnPremRelatedResult> relatedResult = new List<OnPremRelatedResult>();
            List<AvePoint.RA.Contract.RMRelatedRecord.RMRelatedItemInfo> spItemInfos = new List<AvePoint.RA.Contract.RMRelatedRecord.RMRelatedItemInfo>();
            foreach (var itemInfo in relatedItems)
            {
                try
                {
                    //if (sourceFlag == (int)SourceFlag.Physical && itemInfo.SiteUrl != null)
                    //{
                    //    SOArchiverJobInfoStatistics.Instance.InitInstance(jobid, itemInfo.SiteUrl, Contract.JobMonitor.JobType.PhysicalRecordsDisposal, itemInfo.SiteId.ToString());
                    //}
                    if (itemInfo.SourceFlag == (int)SourceFlag.Physical)
                    {
                        PhyscialDisposalAction action = new PhyscialDisposalAction(DateTime.UtcNow, jobid, jobRunBy);
                        //PhyBox = 9300,PhyFile = 9400,PhyRecord = 9500,
                        if (itemInfo.NodeType == 9400)
                        {
                            var r = ExplorerDao.GetPhysicalRecordById(itemInfo.id);
                            var file = new PhysicalFile(r);
                            if (!IsRecordsHold(file, DateTime.UtcNow.Ticks))
                            {
                                var actionAudit = action.DisposalFile(relatedResult, file, currentRule, SendRelatedJobDetail, false);
                                if (actionAudit != null)
                                {
                                    RecordsHistoryService.AddPhysicalAudit([actionAudit]);
                                }
                            }
                            else
                            {
                                logger.Info("Current Related physical folder IsRecordsHold,DirPath:{0}.", file.Id);
                                SendRelatedJobDetail(relatedResult, file.Name, file.DirPath, "RM_Common_ObjectLevel_PhysicalFile", JobDetailsStatus.Skipped, "RM_PRM_Disposal_SkipHoldFolder");
                            }
                        }
                        else if (itemInfo.NodeType == 9500)
                        {
                            var r = ExplorerDao.GetPhysicalRecordById(itemInfo.id);
                            var record = new PhysicalRecord(r);
                            if (!IsRecordsHold(record.ParentFile, DateTime.UtcNow.Ticks))
                            {
                                action.DisposalRecord(record, currentRule, false);
                                SendRelatedJobDetail(relatedResult, record.Name, record.DirPath, "RM_Common_ObjectLevel_PhysicalRecord", JobDetailsStatus.Successful);
                            }
                            else
                            {
                                logger.Info("Current Related physical folder IsRecordsHold,DirPath:{0}.", record.Id);
                                SendRelatedJobDetail(relatedResult, record.Name, record.DirPath, "RM_Common_ObjectLevel_PhysicalRecord", JobDetailsStatus.Skipped, "RM_PRM_Disposal_SkipHoldRecord");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"Error Related occurred while deleting related data. Id:{itemInfo.id} DataSource;{itemInfo.SourceFlag} Error:{e.ToString()}"); ;
                    success = false;
                    break;
                }
            }

            //if (success)
            //{
            //    SOArchiverJobInfoStatistics.Instance.IsDeleteArchiveAction = true;
            //    SOArchiverJobInfoStatistics.Instance.SaveInfoToDB();
            //}
            return relatedResult;
        }
        public bool IsRecordsHold(IPhysicalBox box, long ticks)
        {
            bool IsRecordsHold = false;
            logger.Info("IsRecordsHold.");
            try
            {
                //List<RMRecordAlliance> rMRecordAlliances = GetAllRMRecordAlliance();
                //int disposalCount = rMRecordAlliances.Count(a => a.HoldReleaseTime > ticks && ids.Any(temp => temp == a.RecordsId));
                if (box.HoldStatus && box.HoldReleaseTime > ticks)
                {
                    return true;
                }
                List<Guid> ids = new List<Guid>() { box.Id };
                List<RMRecordLoanAlliance> loanAlliances = GetPhyRecordAllianceByIds(ids);
                int loanCount = loanAlliances.Count;
                return loanCount > 0;
            }
            catch (Exception ex)
            {
                logger.Info("Failed IsRecordsHold.Message:{0}. ", ex.ToString());
            }
            return IsRecordsHold;
        }


        public bool IsRecordsHold(IPhysicalFile file, long ticks)
        {
            bool IsRecordsHold = false;
            logger.Info("IsRecordsHold.");
            try
            {
                //List<RMRecordAlliance> rMRecordAlliances = GetAllRMRecordAlliance();
                //int disposalCount = rMRecordAlliances.Count(a => a.HoldReleaseTime > ticks && ids.Any(temp => temp == a.RecordsId));
                if (file.HoldStatus && file.HoldReleaseTime > ticks
                    || (file.ParentBox != null && file.ParentBox.HoldStatus && file.ParentBox.HoldReleaseTime > ticks))
                {
                    return true;
                }
                List<Guid> ids = new List<Guid>();
                ids.Add(file.Id);
                if (file.ParentBox != null)
                {
                    ids.Add(file.ParentBox.Id);
                }
                List<RMRecordLoanAlliance> loanAlliances = GetPhyRecordAllianceByIds(ids);
                int loanCount = loanAlliances.Count;
                return loanCount > 0;
            }
            catch (Exception ex)
            {
                logger.Info("Failed IsRecordsHold.Message:{0}. ", ex.ToString());
            }
            return IsRecordsHold;
        }
        public List<RMRecordLoanAlliance> GetPhyRecordAllianceByIds(List<Guid> ids)
        {
            logger.Info("GetPhyRecordAllianceByIds.");
            List<RMRecordLoanAlliance> loanAlliances = new List<RMRecordLoanAlliance>();
            loanAlliances = RecordLoanAllianceDao.GetPhyRecordAllianceByIds(ids);
            return loanAlliances.Where(a => ids.Any(temp => temp == a.RecordsId)).ToList();
        }

        public void UpdloadDestructionCache()
        {
            try
            {
                DestructionFactory.UploadToStorage();
            }
            catch (Exception e)
            {
                logger.Error($"Error occurred while uploading destrunction cache. Error:{e.ToString()}");
            }
        }

        //disposal single sp data
        private async System.Threading.Tasks.Task DisposalSPDataAsync(AvePoint.RA.Contract.RMRelatedRecord.RMRelatedItemInfo info, Rule rule, Guid nodeId, bool isRAJob, string jobid, int sourceFlag)
        {
            //delete failed 
            var contract = GetRelativeDataArchiverContract(info, rule, string.Empty);
            var relatedId = info.ItemUrl;
            if (contract == null)
            {
                DeleteStatusCache[relatedId] = false;
                return;
            }
            try
            {
                DisposalActivityManagementProcessor disposalActivityManagement = new DisposalActivityManagementProcessor();
                DeleteStatusCache[relatedId] = await disposalActivityManagement.RelativeDataBackupAsync(contract, isRAJob, jobid, sourceFlag);
            }
            catch (Exception e)
            {
                DeleteStatusCache[relatedId] = false;
                logger.Error($"Error occurred while end user backup item. Path:{info.ItemUrl} Error:{e.ToString()}");
            }
        }

        public void SendPhysicalJobDetail(string name, string originPath, string ruleName, PhysicalDisposalActionType action, string destinationPath, string ItemType, JobDetailsStatus status, string comment = "")
        {
            this.SendPhysicalJobDetail(name, originPath, action, destinationPath, ItemType, status, comment);
        }

        public void SendPhysicalJobDetail(string name, string originPath, PhysicalDisposalActionType action, string destinationPath, string ItemType, JobDetailsStatus status, string comment = "")
        {
            ReportManager.SendJobDetail(new JMPhysicalDisposalJobDetails()
            {
                ObjectName = name,
                FullPath = originPath,
                ActionType = GetI18NActionType(action),
                RuleName = CurrentRule?.Name,
                DestinationPath = destinationPath,
                ItemType = ItemType,
                Status = status,
                Comment = comment
            });
        }

        public void SendSPJobDetail(string name, string originPath, string ruleName, PhysicalDisposalActionType action, string destinationPath, string ItemType, JobDetailsStatus status, string comment = "")
        {
            this.SendSPJobDetail(name, originPath, action, destinationPath, ItemType, status, comment);
        }
        public void SendRelatedJobDetail(List<OnPremRelatedResult> relatedResult, string name, string dirPath, string ItemType, JobDetailsStatus status, string comment = "")
        {
            relatedResult.Add(new OnPremRelatedResult()
            {
                Name = name,
                DirPath = GetDirPath(dirPath),
                Message = comment,
                DetailsStatus= status,
                ObjectLevel = ItemType
            });
        }
        private string GetDirPath(string dirPath)
        {
            string i18nKey = "RM_SPS_Location_RootNode";
            var i18nRootName = I18NEntity.GetString(i18nKey);
            if (!string.IsNullOrEmpty(dirPath) && dirPath.StartsWith(i18nKey))
            {
                dirPath = i18nRootName + dirPath.Substring(i18nKey.Length);
            }
            return dirPath;
        }
        public void SendSPJobDetail(string name, string originPath, PhysicalDisposalActionType action, string destinationPath, string ItemType, JobDetailsStatus status, string comment = "")
        {
            //JMArchiverActionJobDetails mArchiverActionJobDetails = new JMArchiverActionJobDetails();
            //mArchiverActionJobDetails.SourceLocation = url;
            //mArchiverActionJobDetails.Size = nodeSize.ToString();
            //mArchiverActionJobDetails.RuleName = rulename;
            //mArchiverActionJobDetails.Status = status;
            //mArchiverActionJobDetails.Level = ArchiverTypeConvert.ConvertNodeLevelToI18n(cacheNodeType);
            //mArchiverActionJobDetails.ActionTab = (int)ActionTab.Action;
            //mArchiverActionJobDetails.Action = keepData;
            //mArchiverActionJobDetails.FinishTime = DateTime.UtcNow.Ticks;
            ReportManager.SendJobDetail(new JMArchiverActionJobDetails()
            {
                SourceLocation = originPath,
                Size = "0",
                RuleName = CurrentRule?.Name,
                Status = status,
                Level = ItemType,
                ActionTab = (int)ActionTab.Action,
                //Action = "Delete",
                FinishTime = DateTime.UtcNow.Ticks,
                Comment = comment
            });
        }
        public void SendSPOnpremJobDetail(string name, string originPath, PhysicalDisposalActionType action, string destinationPath, string ItemType, JobDetailsStatus status, string comment = "")
        {
            ReportManager.SendJobDetail(new JMPhysicalDisposalJobDetails()
            {
                ObjectName = name,
                FullPath = originPath,
                RuleName = CurrentRule?.Name,
                Status = status,
                ItemType = ItemType,
                //Level = ItemType,
                ActionType = "RM_JMD_PD_DisposalAction_Dispose",
                //Action = "Delete",
                //FinishTime = DateTime.UtcNow.Ticks,
                Comment = comment
            });
        }
        private string GetI18NActionType(PhysicalDisposalActionType action)
        {
            string result = string.Empty;
            switch (action)
            {
                case PhysicalDisposalActionType.Pending:
                    result = "RM_JMD_PD_DisposalAction_Pending";
                    break;
                case PhysicalDisposalActionType.Disposal:
                    result = "RM_JMD_PD_DisposalAction_Dispose";
                    break;
                case PhysicalDisposalActionType.Move:
                    result = "RM_JMD_PD_DisposalAction_Move";
                    break;
                default:
                    result = action.ToString();
                    break;
            }
            return result;
        }

        internal RelativeDataArchiverContract GetRelativeDataArchiverContract(RMRelatedItemInfo info, Rule rule, string metaData)
        {
            logger.Info("start get end user archive job for contract");
            RelativeDataArchiverContract relativeDataContract = null;
            try
            {
                relativeDataContract = GetRelativeDataArchiveContract(info, rule, metaData);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error in archiving the related item. Exception: {0}", ex.Message));
            }
            return relativeDataContract;
        }

        private RelativeDataArchiverContract GetRelativeDataArchiveContract(RMRelatedItemInfo info, Rule rule, string metaData)
        {
            logger.Info("get archive contract");
            RelativeDataArchiverContract relativeDataContract = GetRelativeDataArchiveBackupContract(info, rule, metaData);
            switch (info.level)
            {
                case SORelativeDataArchiverNodeLevel.Document:
                    {
                        relativeDataContract.NodeLevel = (int)ArchiveLevel.Document;
                        relativeDataContract.FullPath = info.url;
                        break;
                    }
                case SORelativeDataArchiverNodeLevel.Item:
                    {
                        relativeDataContract.NodeLevel = (int)ArchiveLevel.Item;
                        relativeDataContract.FullPath = info.url;
                        break;
                    }
                //case SOEndUserArchiverNodeLevel.Multifiles:
                //    {
                //        relativeDataContract.NodeLevel = (int)ArchiveLevel.List;
                //        relativeDataContract.FullPath = SOContextObject.SOList.FullPath;
                //        break;
                //    }
                default:
                    logger.Error(string.Format("Get contract error: {0}", info.level.ToString()));
                    break;
            }
            return relativeDataContract;
        }

        private RelativeDataArchiverContract GetRelativeDataArchiveBackupContract(RMRelatedItemInfo info, Rule rule, string metaData)
        {
            RelativeDataArchiverContract relativeDataContract = new RelativeDataArchiverContract();
            //Regist farm name is hard code 
            relativeDataContract.FarmName = "Remote Farm 2013";
            relativeDataContract.SiteId = info.SiteId.ToString();
            relativeDataContract.SiteUrl = info.SiteUrl;
            relativeDataContract.RuleId = rule.Id;
            relativeDataContract.Rule = rule;
            relativeDataContract.MetaData = GetRequestMetadata(info, metaData);
            //Agent 直接另起进程完成Job，不需要Agent Address
            //relativeDataContract.AgentAddress = AveEnv.AgentAddress;
            relativeDataContract.NodeId = string.Empty;
            relativeDataContract.NodeName = string.Empty;
            return relativeDataContract;
        }

        private string GetRequestMetadata(RMRelatedItemInfo info, string metaData)
        {
            string metadata = string.Empty;
            #region Build XML Tree
            SORelativeDataArchiveBackupRequest relativeDataArchiveBackupRequest = null;
            switch (info.level)
            {
                case SORelativeDataArchiverNodeLevel.Item:
                case SORelativeDataArchiverNodeLevel.Document:
                    {
                        relativeDataArchiveBackupRequest = GetItemArchiveRequest(info, metaData);
                        break;
                    }
                default:
                    break;
            }
            #endregion
            metadata = SerializerHelper.SerializeToXmlString<SORelativeDataArchiveBackupRequest>(relativeDataArchiveBackupRequest);
            return metadata;
        }

        private SORelativeDataArchiveBackupRequest GetItemArchiveRequest(RMRelatedItemInfo info, string metaData)
        {
            SORelativeDataArchiveBackupRequest relativeDataArchiveBackupRequest = new SORelativeDataArchiveBackupRequest()
            {
                SiteCollectionId = info.SiteId.ToString(),
                SiteCollectionUrl = info.SiteUrl,
                WebId = info.WebId.ToString(),
                ListId = info.ListId.ToString(),
                FolderId = info.FolderId.ToString(),
                LeafName = info.name,
                ItemId = info.id.ToString(),
                DocLibRowId = info.DocLibRowId,
                Path = AveUrlUtility.GetServerRelativeUrl(info.url),
                ParentFolderIsRootFolder = info.ParentFolderIsRootFolder,
                CurrentLevel = info.level.ToString(),
                //ItemLastModifiedTime = item.Web.RegionalSettings.TimeZone.LocalTimeToUTC(((DateTime)item["Modified"]))
                WebServerRelatedUrl = info.WebServerRelativeUrl,
                ListUrl = info.ListUrl,
                FolderUrl = info.FolderUrl
            };
            return relativeDataArchiveBackupRequest;
        }
    }


}
