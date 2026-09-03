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
using AvePoint.RA.Cache.Services;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Cache;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Google;
using AvePoint.RA.Contract.Google.Model;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.GoogleSyncNodeDao.Contract;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.RuleManagement;
using AvePoint.RA.Service.Services.Archiver;
using AvePoint.RA.Service.Services.Google.AuditHandler;
using AvePoint.RA.Service.Services.TermManagement.AuditHandler;
using Newtonsoft.Json;
using RAExportCommon;
using RAGoogle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using RAGoogle.Util;

namespace AvePoint.RA.Service.Services.Google
{
    [Audit]
    public class RMGoogleJobService : IRMGoogleJobService
    {

        private readonly IRALogger logger = RALogger.GetInstance(typeof(RMGoogleJobService));

        // DI services
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        private IRMRemoteGoogleNodeService RemoteGoogleNodeService => PlatformWindsorManager.GetService<IRMRemoteGoogleNodeService>();
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();

        // DI Dao
        private ITermGroupMembershipDao TermGroupMembershipDao => PlatformWindsorManager.GetService<ITermGroupMembershipDao>();
        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        private IRMScopeRoleAssignmentDao ScopeRoleAssignmentDao => PlatformWindsorManager.GetService<IRMScopeRoleAssignmentDao>();
        private IRMGoogleSettingDao GoogleSettingDao => PlatformWindsorManager.GetService<IRMGoogleSettingDao>();
        private IRMGoogleRemoteNodeDao GoogleRemoteNodeDao => PlatformWindsorManager.GetService<IRMGoogleRemoteNodeDao>();
        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IRMSettingJobDao SettingJobDao => PlatformWindsorManager.GetService<IRMSettingJobDao>();
        private IRMScheduleDao ScheduleDao => PlatformWindsorManager.GetService<IRMScheduleDao>();
        private ITermGroupDao TermGroupDao => PlatformWindsorManager.GetService<ITermGroupDao>();
        private ITermRuleAssociationDao TermRuleAssociationDao => PlatformWindsorManager.GetService<ITermRuleAssociationDao>();
        private IRMRunningJobRuleMappingDao RMRunningJobRuleMappingDao => PlatformWindsorManager.GetService<IRMRunningJobRuleMappingDao>();
        public IRuleManagerService RuleManagerService => PlatformWindsorManager.GetService<IRuleManagerService>();
        private static IRMMLTrainingModelDao TrainingModelDao => PlatformWindsorManager.GetService<IRMMLTrainingModelDao>();
        
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();

        private readonly string _runBySchedule = "RM_TS_RunSchedule";

        private BaseJobDto baseJobDto;
        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        //private readonly Dictionary<string, string> _emptyContainers = new();
        //private readonly Dictionary<Guid, List<RMGoogleTreeNode>> _containerSettingGroup = new();
        private readonly Dictionary<string, RMGoogleSetting> _settingNodeMapping = new();
        #region Init job message queue
        public RAReturnMessage ApplySettings(JobRunBy jobRunBy, bool fromTimerJobPage, RunApplySettingMethod runJobMethod)
        {
            logger.Debug("Start ApplySettings on all node, path: {0}");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                bool hasGControlLicense = TenantService.HasInitGControlPlatForm().Result;

                if (!GooglePermissionHelper.HasGoogleLicense() && !hasGControlLicense)
                {
                    logger.Warn($"Don't have Google permission and google control license to execute this job, job type [{JobType.GoogleApplySettings}]");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }

                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                if (runJobMethod == RunApplySettingMethod.UpdatedScope)
                {
                    var updatedScopeCount = 0;
                    var settings = GoogleSettingDao.GetRunJobSetting();
                    updatedScopeCount = settings.Count;
                    msg.Extension = updatedScopeCount.ToString();
                    if (updatedScopeCount == 0)
                    {
                        msg.Extsion1 = I18NEntity.GetString("RM_JS_SPS_NoUpdatedScope");
                        return msg;
                    }
                    msg.Extsion1 = string.Format(I18NEntity.GetString("RM_JS_SPS_Msg_RunJobNodes"), updatedScopeCount);
                    if (updatedScopeCount == 1)
                    {
                        msg.Extsion1 = string.Format(I18NEntity.GetString("RM_JS_SPS_Msg_RunJobSingleNode"), updatedScopeCount);
                    }
                }
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.GoogleApplySettings,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = jobRunBy == JobRunBy.Schedule ? _runBySchedule : loginName,
                    Parameters = string.Format("{0},{1}", fromTimerJobPage, Convert.ToInt32(runJobMethod))
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while ApplySettingOnSelectedNode, ERROR:{0}", ex.ToString());
            }
            return msg;
        }
        public RAReturnMessage ApplySettingsOnSelectedNode(RMGoogleTreeNode node)
        {
            logger.Debug("Start ApplySettings on selected node, path: {0}");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.GoogleApplySettings,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = string.Format("{0},{1},{2},{3},{4}", false, Convert.ToInt32(RunApplySettingMethod.SelectedNode), node.Id, GetTreeNodeDriveId(node), node.FullPath)
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while ApplySettingOnSelectedNode, ERROR:{0}", ex.ToString());
            }
            return msg;
        }

        public RAReturnMessage RunRecordsDisposalJob(RMGoogleTreeNode node)
        {
            logger.Debug("Start ApplySettings on selected node, path: {0}");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                if (!GooglePermissionHelper.HasGoogleLicense())
                {
                    logger.Warn($"Don't have Google permission to execute this job, job type [{JobType.GoogleRecordsDisposal}]");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }

                if (node == null)
                {
                    logger.Error("Failed to add the run disposal job to the job queue. Google Tree Node is null");
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_JM_FS_Disposal_NoSC");
                    return msg;
                }
                
                var indexDevice = StorageDeviceService.GetIndexDevice();
                if (indexDevice == null)
                {
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_AR_RunEnforceRuleActionJob_Failed_NoIndexDeviceSetting");
                    return msg;
                }

                var dto = new JobQueueDto
                {
                    JobType = JobType.GoogleRecordsDisposal,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = TenantLocalValue.LogonUserEmail,
                    Parameters = JsonConvert.SerializeObject(node)
                };

                var jobId = JobQueueService.AddToDBJobQueue(dto);
                if (string.IsNullOrEmpty(jobId))
                {
                    logger.Error("Failed to add the run disposal job to the job queue. Job ID is null or empty.");
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
                else
                {
                    logger.Info($"Successfully added the run disposal job [{jobId}] to the job queue.");
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while running enforce run action job, ERROR:{0}", ex.ToString());
            }
            return msg;
        }

        #endregion

        #region real-run
        // to-do: audit handler
        [Audit(Module = AuditModule.GoogleDrive, Category = AuditCategory.SharePointSettings, Action = AuditAction.GoogleApplySettings, BeforeHandler = typeof(GoogleServiceBeforeAuditHandler), AfterHandler = typeof(GoogleServiceAfterAuditHandler))]
        public async Task<string> RealRunApplySettingJobAsync(JobRunBy jobRunBy, string jobRunByUser, bool fromTimerJobPage, RunApplySettingMethod runJobMethod, string scopeId = null, string driveId = null, string fullPath = null)
        {
            string jobId = string.Empty;
            // Check if having any apply setting jobs are running
            List<string> runningJobs = JobMonitorService.GetRunningJobs(JobType.GoogleApplySettings);
            try
            {
                logger.Info($"RealRunApplySettingJobAsync: runningJobs.Count: {runningJobs.Count}, jobRunBy: {jobRunBy}, runJobMethod: {runJobMethod}, scopeId: {scopeId}");
                if (runningJobs.Count == 0)
                {
                    // Create job
                    jobId = await ProcessCreateApplySettingJobAsync(jobRunBy, jobRunByUser, JobType.GoogleApplySettings, runJobMethod, scopeId, driveId, fullPath);
                }
                else
                {
                    // Get setting
                    var settings = GetGoogleSettings(jobRunBy, runJobMethod, scopeId, driveId);
                    if (settings.IsNullOrEmpty())
                    {
                        logger.Warn("No Google setting node found.");
                        throw new Exception("No Google setting node found.");
                    }
                    bool hasAvailableNode = false;
                    foreach (var setting in settings)
                    {
                        RMGoogleTreeNode node = SerializerHelper.DeserializeByDataContractSerializer<RMGoogleTreeNode>(setting.NodeInfo);
                        if (node == null)
                        {
                            logger.Warn("Node info in {0} is null", setting.ScopeId);
                            continue;
                        }
                        var containerId = GetGoogleContainerId(node);

                        if (jobRunBy != JobRunBy.Schedule)
                        {
                            var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                            if (!await IsGoogleAdminAsync(account.UserId))
                            {
                                List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(account.UserId);
                                if (!ScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupUserIds))
                                {
                                    logger.Info($"current user doesn't have permission on container. Container Id : {containerId}");
                                    continue;
                                }
                            }
                        }

                        jobId = CreateGoogleJob(jobRunBy, jobRunByUser, JobType.GoogleApplySettings, containerId, scopeId, fullPath);
                        JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SS_JobSkip");
                        logger.Info(I18NEntity.GetString("RM_SS_JobSkip"));
                        hasAvailableNode = true;
                        break;
                    }
                    if (!hasAvailableNode)
                    {
                        jobId = CreateGoogleJob(jobRunBy, jobRunByUser, JobType.GoogleApplySettings, string.Empty, scopeId, fullPath);
                        JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_GoogleDrive_RunAction_NoAvailableNode");
                        logger.Warn($"Has no available node for current user. JobId:{jobId}");
                    }
                }
            }
            catch (Exception e)
            {
                if (string.IsNullOrWhiteSpace(jobId))
                {
                    jobId = CreateGoogleJob(jobRunBy, jobRunByUser, JobType.GoogleApplySettings, string.Empty, scopeId, fullPath);
                }
                if (e.Message == I18NEntity.GetString("RM_SP_NoAvailableSettingError"))
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SP_NoAvailableSettingError");
                }
                else if (e.Message == I18NEntity.GetString("RM_JM_Summary_DisableRecordManagementError"))
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JM_Summary_DisableRecordManagementError");
                }
                else if (e.Message == I18NEntity.GetString("RM_JM_JS_NoInhertDriveError"))
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_JM_JS_NoInhertDriveError");
                }
                else
                {
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SP_CreateJobError");
                }
                logger.Error("real run apply Google setting job error: {0}", e.ToString());
            }

            return jobId;
        }

        [Audit(Module = AuditModule.GoogleDrive, Category = AuditCategory.SharePointSettings, Action = AuditAction.GoogleDataSynchronization, BeforeHandler = typeof(GoogleServiceBeforeAuditHandler), AfterHandler = typeof(GoogleServiceAfterAuditHandler))]
        public async Task<string> RealRunDataSyncJobAsync(JobRunBy jobRunBy, string jobRunByUser, string parameters)
        {
            string jobId = string.Empty;

            try
            {
                if (parameters.IsNullOrEmpty())
                {
                    // create 1 main job for schedule sync and run now from timer page
                    logger.Info($"Start running Google data sync job for all setting node. JobRunBy: {jobRunBy}, jobRunByUser: {jobRunByUser}");
                    return await RunGoogleDataSyncJobAllSettingNodeAsync(jobRunBy, jobRunByUser, JobType.GoogleDataSynchronization);
                }
                RMGoogleTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMGoogleTreeNode>(parameters);
                bool hasJobRunning = JobMonitorService.HasRunningArchiverJobOnScope([JobType.GoogleDataSynchronization], selectedNode.Id);
                if (!hasJobRunning)
                {
                    jobId = await ProcessCreateDataSyncJobAsync(jobRunBy, jobRunByUser, selectedNode);
                }
                else
                {
                    jobId = CreateGoogleJob(jobRunBy, jobRunByUser, JobType.GoogleDataSynchronization, selectedNode.ContainerId, selectedNode.Id, selectedNode.FullPath);
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_GoogleDrive_DataSync_AnotherNodeRunning");
                    logger.Warn($"This drive node is running for current user. JobId:{jobId}");
                }
            }
            catch (Exception e)
            {
                if (string.IsNullOrWhiteSpace(jobId))
                {
                    jobId = CreateGoogleJob(jobRunBy, jobRunByUser, JobType.GoogleDataSynchronization);
                }
                JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SP_CreateJobError");
                logger.Error("real run Google data synchronization job error: {0}", e.ToString());
            }

            return jobId;
        }

        [Audit(Module = AuditModule.GoogleDrive, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunGoogleDisposalJob, BeforeHandler = typeof(GoogleServiceBeforeAuditHandler), AfterHandler = typeof(GoogleServiceAfterAuditHandler))]
        public async Task<string> RealRunRecordsDisposalJobAsync(JobRunBy jobRunBy, string jobRunByUser, string selectedNodeJson)
        {
            string jobId = string.Empty;
            try
            {
                RMGoogleTreeNode selectedNode = JsonConvert.DeserializeObject<RMGoogleTreeNode>(selectedNodeJson);
                var hasJobRunning = JobMonitorService.HasRunningArchiverJobOnScope([JobType.GoogleRecordsDisposal], selectedNode.Id);
                if (!hasJobRunning)
                {
                    // Create job
                    jobId = await ProcessCreateRecordsDisposalJobAsync(jobRunBy, jobRunByUser, selectedNode);
                }
                else
                {
                    logger.Warn($"Current node has job running on. {selectedNode.Id}");
                    jobId = JobMonitorService.CreateJobWithScopeId(JobType.GoogleRecordsDisposal, jobRunByUser, selectedNode.Id, GetGoogleContainerId(selectedNode), selectedNode.FullPath);
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict"); 
                    return jobId;
                }
            }
            catch (Exception e)
            {
                logger.Error("real run google enforce rule action job error: {0}", e.ToString());
                JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SP_CreateJobError");
            }

            return jobId;
        }

        #endregion

        #region Process Apply Settings
        private async Task<string> ProcessCreateApplySettingJobAsync(JobRunBy jobRunBy, string jobRunByUser, JobType jobType, RunApplySettingMethod runJobMethod, string scopeId = null, string driveId = null, string fullPath = null)
        {
            // Get settings jobs
            List<RMGoogleSetting> allSettings;
            using (var performance = new PerformanceScope("ProcessApplySettings"))
            {
                allSettings = GetGoogleSettings(jobRunBy, runJobMethod, scopeId, driveId);
            }

            if (allSettings.IsNullOrEmpty())
            {
                logger.Warn("No Google setting node found.");
                throw new Exception(I18NEntity.GetString("RM_SP_NoAvailableSettingError"));
            }

            string jobId = string.Empty;
            List<RMGoogleSetting> driveLevelSettings;
            using (var performance = new PerformanceScope("GoogleApplySetting.GetDriveNodeSettings"))
            {
                driveLevelSettings = GoogleSettingDao.GetDriveNodeLevelSettings();
            }
            List<NodeCacheInfo> excludeNodesCache = [];
            // validate and cache the drive node level setting
            List<string> settingDriveLevelIds = [];
            List<RMSampleGoogleTreeNode> driveTreeNodeList = GoogleRemoteNodeDao.GetGoogleDrives(
                driveLevelSettings.Select(setting => setting.DriveId.ToString()));

            foreach (var setting in driveLevelSettings)
            {
                if (!ValidateDriveSettingCache(excludeNodesCache, setting.DriveId.ToString(), setting.ContainerId.ToString(), driveTreeNodeList))
                {
                    continue;
                }
                settingDriveLevelIds.Add(setting.ScopeId.ToString());
            }

            Dictionary<string, string> emptyContainers = new();

            Dictionary<Guid, List<RMGoogleTreeNode>> containerSettingGroup = new();
            Dictionary<string, RMGoogleSetting> settingNodeMapping = new();
            List<RMSampleGoogleTreeNode> containerTreeNodeList = GoogleRemoteNodeDao.GetGoogleContainers(
                allSettings.Select(setting => setting.ContainerId.ToString()));
            List<string> nodeIds = [.. driveTreeNodeList.Select(driveNode => driveNode.Id), .. containerTreeNodeList.Select(containerNode => containerNode.Id)];
            allSettings = allSettings.Where(setting => nodeIds.Contains(setting.ScopeId.ToString())).ToList();
            foreach (RMGoogleSetting setting in allSettings)
            {

                if (setting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable && runJobMethod == RunApplySettingMethod.SelectedNode)
                {
                    logger.Error($"The current node setting does not enable record management. Setting ID: {setting.Id}");
                    throw new Exception(I18NEntity.GetString("RM_JM_Summary_DisableRecordManagementError"));
                }

                //Validate and cache settings
                if (!ValidateContainerSettingCache(excludeNodesCache, setting.ContainerId.ToString(), containerTreeNodeList))
                {
                    await GoogleSettingDao.SetSettingJobTimeWithContainerIdAsync(setting.ContainerId, setting.ScopeId);
                    continue;
                }

                // Get and check node info
                RMGoogleTreeNode node = SerializerHelper.DeserializeByDataContractSerializer<RMGoogleTreeNode>(setting.NodeInfo);
                if (node == null)
                {
                    logger.Warn("Node info in {0} is null or empty", setting.Id);
                    continue;
                }
                var containerId = GetGoogleContainerId(node);

                if (jobRunBy != JobRunBy.Schedule)
                {
                    var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                    if (!await IsGoogleAdminAsync(account.UserId))
                    {
                        List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(account.UserId);
                        if (!ScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupUserIds))
                        {
                            logger.Info($"current user doesn't have permission on container. Container Id:{containerId}");
                            continue;
                        }
                    }
                }

                // Process child node if the node info is in container level
                List<RMGoogleTreeNode> inheritChildNodes = new List<RMGoogleTreeNode>();
                if (IsGoogleContainer(node.Level))
                {
                    using (var performance = new PerformanceScope("InitContainerSettings", $"InitContainerSettings{node.Id}"))
                    {
                        List<RMGoogleTreeNode> drives = await RemoteGoogleNodeService.BrowserRMTreeAsync(node);
                        var totalDriveCount = drives.Count;
                        var hasCustomDriveCount = 0;

                        logger.Info("Container:{0} drive count is {1}", node.Name, drives.Count);
                        if (drives.Count > 0)
                        {
                            foreach (RMGoogleTreeNode driveNode in drives)
                            {
                                if (settingDriveLevelIds.Contains(driveNode.Id))
                                {
                                    logger.Info("DriveId has custom setting {0}, skipped.", driveNode.Id);
                                    hasCustomDriveCount++;
                                }
                                else
                                {
                                    inheritChildNodes.Add(driveNode);
                                }

                                if (!settingNodeMapping.ContainsKey(node.Id))
                                {
                                    settingNodeMapping.Add(node.Id, setting);
                                }
                            }
                        }
                        else
                        {
                            if (!emptyContainers.ContainsKey(containerId))
                            {
                                emptyContainers.Add(containerId, GetGoogleContainerName(node));
                            }
                        }
                        if (totalDriveCount == hasCustomDriveCount)
                        {
                            //update group node setting
                            await GoogleSettingDao.SetSettingJobTimeWithContainerIdAsync(setting.ContainerId, setting.ScopeId);
                        }
                    }
                }
                else
                {
                    if (setting.ContainerId.ToString() == containerId)
                    {
                        if (settingDriveLevelIds.Contains(node.Id))
                        {
                            inheritChildNodes.Add(node);
                        }
                    }
                }
                // Group the nodes that do not have custom setting, inherit from parent (container)
                if (inheritChildNodes.Count > 0)
                {
                    var isZeroShotMode = KeyValueDao.EnableZeroShotFeature() && TrainingModelDao.GetDefaultModel()?.Mode == TrainingMode.ZeroShot;
                    foreach (var child in inheritChildNodes) 
                    {
                        child.PredictionModeType = isZeroShotMode ? PredictionModeType.ZeroShot : PredictionModeType.MLTraining;
                    }
                    if (containerSettingGroup.ContainsKey(setting.ContainerId))
                    {
                        containerSettingGroup[setting.ContainerId].AddRange(inheritChildNodes);
                    }
                    else
                    {
                        containerSettingGroup.Add(setting.ContainerId, inheritChildNodes);
                    }
                }
            }
            if (containerSettingGroup.Count > 0)
            {
                foreach (var group in containerSettingGroup)
                {
                    var parentGroupSetting = allSettings.FirstOrDefault(g => group.Key == g.ContainerId);

                    var objectName = fullPath.IsNullOrEmpty() ? parentGroupSetting.FullPath : fullPath;
                    jobId = CreateGoogleJob(jobRunBy, jobRunByUser, JobType.GoogleApplySettings, group.Key.ToString(), group.Key.ToString(), objectName);

                    if (parentGroupSetting.IsNullClassificationSetting)
                    {
                        logger.Warn($"Google Drive Job is skip, because container is null classification setting. google node ids: {string.Join(",", group.Value.Select(m => m.Id))}");

                        switch (parentGroupSetting.FullPath)
                        {
                            case "Default_ Google_ SharedDrive_ Group":
                                parentGroupSetting.FullPath = I18NEntity.GetString("RM_GoogleSharedDrive_Default_Container");
                                break;
                            case "Default_ GoogleUser_ Group":
                                parentGroupSetting.FullPath = I18NEntity.GetString("RM_GoogleUser_Default_Container");
                                break;
                        }

                        JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, $"RM_EXO_GroupIsRuleSettingAndSkipApplySetting{I18NEntity.Separator}{parentGroupSetting.FullPath}");
                    }
                    else
                    {
                        SeparateSubJobForApplySetting(group.Value, settingNodeMapping, jobId, jobRunBy, jobType);
                    }

                    #region Store job settings to db.
                    var settingsPerContainer = allSettings.Where(s => s.ContainerId == group.Key).ToList();
                    logger.Info("Begin store job setting, JobId: {0}, Drive Container: {1} Setting Count: {2}.", jobId, group.Key, settingsPerContainer.Count);
                    var isExist = SettingJobDao.GetRMSettingJob(item => item.Id == jobId && item.JobType == (int)jobType) != null;
                    if (!isExist)
                    {
                        RMSettingJobInfo settingJobInfo = new RMSettingJobInfo
                        {
                            Id = jobId,
                            JobType = (int)JobType.GoogleApplySettings,
                            JobInfos = SerializerHelper.SerializeByDataContractSerializer(settingsPerContainer),
                        };

                        SettingJobDao.AddRMSettingJob(settingJobInfo);
                    }
                    logger.Info("Finishing stored job setting, JobId: {0}, Drive Container: {1} Setting Count: {2}.", jobId, group.Key, settingsPerContainer.Count);
                    #endregion
                }
            }
            else
            {
                if (emptyContainers.Count > 0)
                {
                    foreach (var container in emptyContainers)
                    {
                        jobId = CreateGoogleJob(jobRunBy, jobRunByUser, JobType.GoogleApplySettings, container.Key, container.Key, container.Value);
                        JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, $"RM_JM_JS_NoDriveUnderGroup{I18NEntity.Separator}{container.Value}");
                    }
                }
                else
                {
                    logger.Warn("No google setting node group found.");
                    throw new Exception(I18NEntity.GetString("RM_JM_JS_NoInhertDriveError"));
                }
            }
            return jobId;
        }

        #endregion

        #region process run enforce rule
        private async Task<string> ProcessCreateRecordsDisposalJobAsync(JobRunBy jobRunBy, string jobRunByUser, RMGoogleTreeNode selectedNode)
        {
            string jobId = string.Empty;
            using (var performance = new PerformanceScope("CreateRecordsDisposalJob", $"RecordsDisposalOnNode{selectedNode.Id}"))
            {
                List<RMGoogleTreeNode> availableNodes = await GetAvailableEnforceRuleNodesAsync(selectedNode);
                if (availableNodes.IsNullOrEmpty())
                {
                    jobId = JobMonitorService.CreateJobWithScopeId(JobType.GoogleRecordsDisposal, jobRunByUser, selectedNode.Id, GetGoogleContainerId(selectedNode), selectedNode.FullPath);
                    logger.Warn("No available nodes to run record disposal job");
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, $"RM_JM_JS_NoDriveUnderGroup{I18NEntity.Separator}{selectedNode.FullPath}");
                    return jobId;
                }

                var runningDriveIds = await JobMonitorService.GetRunningDriveNodeIds([JobType.GoogleRecordsDisposal]);
                availableNodes = availableNodes.Where(n => !runningDriveIds.Contains(n.ObjectId)).ToList();
                if (availableNodes.Count == 0)
                {
                    jobId = JobMonitorService.CreateJobWithScopeId(JobType.GoogleRecordsDisposal, jobRunByUser, selectedNode.Id, GetGoogleContainerId(selectedNode), selectedNode.FullPath);
                    logger.Warn($"Current has job running on same scope.will skip job");
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                    return jobId;
                }
                jobId = JobMonitorService.CreateJobWithScopeId(JobType.GoogleRecordsDisposal, jobRunByUser, selectedNode.Id, GetGoogleContainerId(selectedNode),selectedNode.FullPath, GoogleTreeNodeUtil.GenerateArchiveJobMonitorExtension(selectedNode,TreeMode.LifeGDrive));
                List<JobType> indexJobTypes = new List<JobType>() { JobType.ArchiverMoveIndex };
                var mIndexJobs = JobMonitorService.GetRunningJobs(indexJobTypes);

                if (mIndexJobs.Count > 0)
                {
                    //has move index job, need skip.
                    logger.Warn("Current has move index job running.");
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                    return jobId;
                }
                SeparateSubJobForDisposal(availableNodes, jobId, jobRunBy, JobType.GoogleRecordsDisposal);
                RMRunningJobRuleMappingDao.AddJobRuleMapping(TenantLocalValue.LogonGroupId, jobId, GetGGRules(selectedNode));
            }

            return jobId;
        }

        private List<Guid> GetGGRules(RMGoogleTreeNode tree)
        {
            if (tree.IsNullClassificationSetting && tree.Rules?.Count > 0)
            {
                return tree.Rules.Select(r => r.RuleId).Distinct().ToList();
            }

            List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule> rules = RuleManagerService.GetRulesFromRecords();
            List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule> ggRules = rules.AsQueryable().Where(r => r.GoogleDriveRule != null && r.GoogleDriveRule.SOFilters != null && r.GoogleDriveRule.SOFilters.Count != 0).ToList();
            return TermRuleAssociationDao.GetTermWithRuleLevel(tree.Level, ggRules).Select(t => t.RuleId).Distinct().ToList();
        }

        private async Task<List<RMGoogleTreeNode>> GetAvailableEnforceRuleNodesAsync(RMGoogleTreeNode selectedNode)
        {
            List<RMGoogleTreeNode> availableNodes = [];
            if (IsGoogleContainer(selectedNode.Level))
            {
                List<RMGoogleTreeNode> drives = await RemoteGoogleNodeService.BrowserRMTreeAsync(selectedNode);
                drives.ForEach(drive => drive.IsNodeProcessFromGControl = selectedNode.IsNodeProcessFromGControl);
                List<string> breakScheduleNodes = [];
                logger.Info("Container:{0} drive count is {1}", selectedNode.Id, drives.Count);

                // Get child nodes have schedule setting
                string parentId = ScheduleService.GetProfileId(selectedNode) + "|";
                var treeNodes = ScheduleDao.GetDisposalBreakNodes(parentId);
                var settingInforDrives = GoogleSettingDao.GetSettingInforDrive(Guid.Parse(selectedNode.Id));
                foreach (var item in treeNodes)
                {
                    var node = JsonConvert.DeserializeObject<RMGoogleTreeNode>(item);
                    if (IsGoogleContainer(node.Level))
                    {
                        continue;
                    }
                    breakScheduleNodes.Add(node.Id);
                }
                // await LoadGoogleSettingsAsync(drives, selectedNode);
                if (drives.Count > 0 && selectedNode.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
                {
                    foreach (RMGoogleTreeNode driveNode in drives)
                    {
                        var driveSetting = settingInforDrives.FirstOrDefault(drive => drive.DriveId == Guid.Parse(driveNode.DriveId));

                        if (!breakScheduleNodes.Contains(driveNode.Id)
                            && (driveSetting == null || driveSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable))
                        {
                            availableNodes.Add(driveNode);
                        }
                    }
                }
            }
            else
            {
                if (IsDriveNodeExist(selectedNode)) 
                {
                    availableNodes.Add(selectedNode);
                }
                else
                {
                    logger.Warn("Node does not exist, node {0}", selectedNode.Id);
                }
            }

            return availableNodes;
        }
        #endregion

        #region Process Data sync

        private async Task<string> ProcessCreateDataSyncJobAsync(JobRunBy jobRunBy, string jobRunByUser, RMGoogleTreeNode selectedNode)
        {
            string jobId = CreateGoogleJob(jobRunBy, jobRunByUser, JobType.GoogleDataSynchronization, selectedNode.ContainerId, selectedNode.Id, selectedNode.FullPath);
            using (var performance = new PerformanceScope("CreateDataSyncJob", $"DataSyncOnNode{selectedNode.Id}"))
            {
                if (!selectedNode.IsSyncData || selectedNode.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                {
                    logger.Warn("No available nodes to run data sync job");
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_JS_JD_DisableRecordManagement_Or_HasOwnSetting");
                    return jobId;
                }
                List<RMGoogleTreeNode> availableNodes = await GetAvailableDataSyncNodesAsync(selectedNode);
                if (availableNodes.IsNullOrEmpty())
                {
                    logger.Warn("No available nodes to run data sync job");
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, $"RM_JM_JS_NoDriveUnderGroup{I18NEntity.Separator}{selectedNode.FullPath}");
                    return jobId;
                }

                SeparateSubJob(availableNodes, jobId, jobRunBy, JobType.GoogleDataSynchronization);
            }

            return jobId;
        }

        private async Task<List<RMGoogleTreeNode>> GetAvailableDataSyncNodesAsync(RMGoogleTreeNode selectedNode)
        {
            if (selectedNode.Level is (int)NodeLevel.GoogleMyDrive or (int)NodeLevel.GoogleSharedDrive)
            {
                return [selectedNode];
            }
            var nodes = await RemoteGoogleNodeService.BrowserRMTreeAsync(selectedNode);

            var unSyncableNodeIds = new HashSet<string>(GoogleSettingDao.GetUnSyncableNodeIdsByContainerId(new(selectedNode.Id)));
            nodes.RemoveAll(n => unSyncableNodeIds.Contains(n.Id));
            return nodes;
        }
        #endregion

        #region Import Label from Google
        public async Task<RAReturnMessage> RunImportGoogleTermStructure(JobRunBy jobRunBy, RMGoogleTermGroupSetting setting)
        {
            RAReturnMessage result = new();
            try
            {
                var googleTenantsExistInOtherTermGroup =
                    await TermGroupMembershipDao.GetGoogleTenantsExisted(setting.GoogleTenants.Keys.ToList(), Guid.Parse(setting.TermGroupId));
                if (googleTenantsExistInOtherTermGroup.IsNotNullOrEmpty())
                {
                    result.MessageType = RAMessageType.Failed;
                    result.ErrorMessage = JsonConvert.SerializeObject(googleTenantsExistInOtherTermGroup);
                    result.Extension = "ExistedGoogleTenants";
                    return result;
                }
                await TermGroupDao.UpdateGoogleTermGroupSettingAsync(setting);
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ImportGoogleTermStructure,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = setting.TermGroupId,
                };
                var id = JobQueueService.AddToDBJobQueue(jqDto);
                await SecurityTrimmingHelper.RemovePermissionCacheAsync();
                RedisCacheService.CacheProvider.KeyDel(CacheKeyPrefix.SecurityTermCacheKeyPrefix + TenantLocalValue.LogonGroupId);
                return result;
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while RunImportTermFromGoogle,ERROR:{0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = ex.Message;
                result.Extension = "Exception";
                return result;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TermManagement, Action = AuditAction.ImportGoogleTerm, AfterHandler = typeof(TermManagementAfterAuditHandler))]
        public string RealRunImportGoogleTermJob(JobRunBy jobRunBy, string jobRunByUser, string termGroupId)
        {
            string jobId = string.Empty;

            if (jobRunBy == JobRunBy.Control)
            {
                jobId = JobMonitorService.CreateJob(JobType.ImportGoogleTermStructure, jobRunByUser);
                logger.Info("Begin control Import Google Term Job {0}", jobId);
            }
            baseJobDto = new BaseJobDto() { Id = jobId, JobType = (int)JobType.ImportGoogleTermStructure };
            List<BaseJobDto> importJobs = JobMonitorService.GetRunningJobs([JobType.ImportGoogleTermStructure]);

            bool isSkip = false;
            if (importJobs != null && importJobs.Count > 0)
            {
                var otherImportJobs = importJobs.Where(j => !j.Id.Equals(jobId)).ToList();
                if (otherImportJobs != null && otherImportJobs.Count > 0)
                {
                    isSkip = true;
                }
            }
            if (!isSkip)
            {
                JobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = jobId,
                    JobType = JobType.ImportGoogleTermStructure,
                    CommandLine = string.Format("{0} {1} {2}", JobType.ImportGoogleTermStructure, jobId, termGroupId),
                });
            }
            else
            {
                JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_ImportTerm_JobSkip");
                logger.Info(I18NEntity.GetString("RM_ImportTerm_JobSkip"));
            }
            return jobId;
        }
        #endregion

        #region private methods

        private string GetTreeNodeDriveId(RMGoogleTreeNode node)
        {
            if (IsGoogleContainer(node.Level))
            {
                return Guid.Empty.ToString();
            }
            else
            {
                // to-do: handle folder level in future
                return node.DriveId;
            }
        }

        private bool IsDriveNodeExist(RMGoogleTreeNode selectedNode)
        {
            RMSampleGoogleTreeNode node = null;
            try
            {
                node = GoogleRemoteNodeDao.GetGoogleDriveById(selectedNode.Id);
            }
            catch (Exception ex)
            {
                logger.Error("get google drive node error. Error: {0}", ex.ToString());
            }
            return node != null ? true : false;
        }

        //private async Task LoadGoogleSettingsAsync(List<RMGoogleTreeNode> nodes, RMGoogleTreeNode selectedNode)
        //{
        //    try
        //    {
        //        logger.Info($"Begin to load google settings for node: {selectedNode.Id} Child nodes count:{nodes.Count}");
        //        using (var performance = new PerformanceScope("RMGoogleJobService.LoadGoogleSettings"))
        //        {
        //            var setting = GetGoogleSettings(JobRunBy.Control, RunApplySettingMethod.SelectedNode, selectedNode.Id, selectedNode.DriveId).FirstOrDefault();
        //            List<RMGoogleSetting> driveLevelSettings = [];
        //            using (var performance0 = new PerformanceScope("GoogleApplySetting.GetDriveNodeSettings"))
        //            {
        //                driveLevelSettings = GoogleSettingDao.GetDriveNodeLevelSettings();
        //            }
        //            if (setting != null)
        //            {
        //                foreach (var node in nodes)
        //                {
        //                    ArgumentCheck.NotNull(node, nameof(node));
        //                    var scopeId = new Guid(node.Id);
        //                    var driveId = new Guid(GetTreeNodeDriveId(node));
        //                    var driveSetting = driveLevelSettings.Where(s => s.ScopeId == scopeId && s.DriveId == driveId).FirstOrDefault();
        //                    if (driveSetting == null && setting != null)
        //                    {
        //                        node.LabelId = new Guid(setting.LabelId);
        //                        node.LabelName = setting.LabelName;
        //                        node.DefaultLabelId = new Guid(setting.DefaultLabelId);
        //                        node.DefaultLabelName = setting.DefaultLabelName;
        //                        node.DeployLabelMethod = (DeployLabelMethod)setting.DeployLabelMethod;
        //                        node.ApprovalType = (int)setting.ApprovalType;
        //                        node.AutoJobOption = (AutoJobOption)setting.AutoJobOption;
        //                        node.AutoClassificationRules = SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(setting.AutoClassificationRules);
        //                        node.EnableRecordManagement = setting.EnableRecordManagement;
        //                        node.IsSyncData = setting.IsSyncData;
        //                        node.RunAutoFullJob = setting.RunAutoFullJob;
        //                        node.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(setting.Id, RecordOwnerSettingType.GoogleDrive);
        //                    }
        //                    else
        //                    {
        //                        if (driveSetting != null)
        //                        {
        //                            node.IsCustomSetting = true;
        //                            node.LabelId = new Guid(driveSetting.LabelId);
        //                            node.LabelName = driveSetting.LabelName;
        //                            node.DefaultLabelId = new Guid(driveSetting.DefaultLabelId);
        //                            node.DefaultLabelName = driveSetting.DefaultLabelName;
        //                            node.DeployLabelMethod = (DeployLabelMethod)driveSetting.DeployLabelMethod;
        //                            node.ApprovalType = (int)driveSetting.ApprovalType;
        //                            node.AutoJobOption = (AutoJobOption)driveSetting.AutoJobOption;
        //                            node.AutoClassificationRules = SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(driveSetting.AutoClassificationRules);
        //                            node.EnableRecordManagement = driveSetting.EnableRecordManagement;
        //                            node.IsSyncData = driveSetting.IsSyncData;
        //                            node.RunAutoFullJob = driveSetting.RunAutoFullJob;
        //                            node.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(driveSetting.Id, RecordOwnerSettingType.GoogleDrive);
        //                        }
        //                    }
        //                }
        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("An error occurred when loading GoogleSetting. Error:{0}", ex.ToString());
        //        throw;
        //    }
        //}

        private List<RMGoogleSetting> GetGoogleSettings(JobRunBy jobRunBy, RunApplySettingMethod runJobMethod, string scopeId = null, string driveId = null)
        {
            List<RMGoogleSetting> allSettings = null;
            if (jobRunBy == JobRunBy.Control)
            {
                switch (runJobMethod)
                {
                    case RunApplySettingMethod.UpdatedScope:
                        allSettings = GoogleSettingDao.GetRunJobSetting();
                        break;
                    case RunApplySettingMethod.AllScope:
                        logger.Info("apply full google setting job");
                        allSettings = GoogleSettingDao.GetAllSettings();
                        break;
                    case RunApplySettingMethod.Auto:
                        allSettings = GoogleSettingDao.GetRunJobSetting();
                        if (allSettings.Count == 0)
                        {
                            logger.Info("apply full google setting job");
                            allSettings = GoogleSettingDao.GetAllSettings();
                        }
                        break;
                    case RunApplySettingMethod.SelectedNode:
                        if (scopeId == null || scopeId.Equals(Guid.Empty.ToString()))
                        {
                            throw new Exception("Scope id is null");
                        }
                        logger.Info("Apply setting on selected node, ScopeId :{0} DriveId:{1}", scopeId, driveId);
                        var googleContainer = GoogleRemoteNodeDao.GetGoogleContainerById(scopeId);
                        string containerId = string.Empty;
                        if (googleContainer != null)
                        {
                            containerId = scopeId;
                        }
                        else
                        {
                            var drive = GoogleRemoteNodeDao.GetGoogleDriveById(scopeId);
                            containerId = drive?.ParentId;
                        }
                        var setting = GoogleSettingDao.GetSettingInfoByScope(new Guid(containerId), new Guid(scopeId), new Guid(driveId));
                        logger.Info("Get setting of selected node successfully, exist:{0}", setting != null);
                        if (setting != null)
                        {
                            allSettings = new List<RMGoogleSetting>() { setting };
                        }
                        else
                        {
                            var containerSetting = GoogleSettingDao.GetSettingInfoByScope(new Guid(containerId), new Guid(containerId), Guid.Empty);
                            if (containerSetting != null) allSettings = new List<RMGoogleSetting>() { containerSetting };
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                allSettings = GoogleSettingDao.GetAllSettings();
            }

            if (allSettings != null)
            {
                logger.Info("Load Google settings finished. Count:{0}", allSettings.Count);
            }

            return allSettings;
        }

        private Task<bool> IsGoogleAdminAsync(string userId)
        {
            return TenantUtil.RunUnderTenantAsync(new TenantContext(TenantLocalValue.LogonGroupId, userId, TenantLocalValue.LogonUserEmail),
            () =>
            {
                return SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.GoogleAdmin);
            });
        }

        private string GetGoogleContainerId(RMGoogleTreeNode selectedNode)
        {
            if (selectedNode == null)
            {
                return string.Empty;
            }
            if (IsGoogleContainer(selectedNode.Level))
            {
                return selectedNode.Id;
            }
            else
            {
                return GetGoogleContainerId(selectedNode.Parent);
            }
        }

        private string GetGoogleContainerName(RMGoogleTreeNode selectedNode)
        {
            if (IsGoogleContainer(selectedNode.Level))
            {
                return DefaultSecurityContainerNameHelper.GetI18NName(selectedNode.DisplayName);
            }
            else
            {
                return GetGoogleContainerName(selectedNode.Parent);
            }
        }

        #endregion

        #region create job
        private string CreateGoogleJob(JobRunBy jobRunBy, string jobRunByUser, JobType jobType, string containerId = null, string scopeId = null, string fullPath = null)
        {
            if (scopeId == null || scopeId.Equals(Guid.Empty.ToString()))
            {
                logger.Error("ScopeId is null or empty.");
            }

            if (!string.IsNullOrEmpty(scopeId))
            {
                var node = GoogleRemoteNodeDao.LoadGoogleSetting(string.IsNullOrEmpty(containerId) ? Guid.Empty : new Guid(containerId), new Guid(scopeId));
                if (node != null && fullPath.StartsWith("/"))
                {
                    fullPath = node.FullPath;
                }
            }

            string jobId = string.Empty;
            if (jobRunBy == JobRunBy.Control)
            {
                jobId = JobMonitorService.CreateJob(jobType, jobRunByUser, containerId, scopeId, fullPath);
                logger.Info("Begin control Google job. JobId:{0}, JobType: {1}", jobId, jobType);
            }
            else if (jobRunBy == JobRunBy.Schedule)
            {
                jobId = JobMonitorService.CreateJob(jobType, "RM_TS_RunSchedule", containerId, scopeId, fullPath);
                logger.Info("Begin Google job. JobId:{0}, JobType: {1}", jobId, jobType);
            }
            else
            {
                jobId = JobMonitorService.CreateJob(jobType, jobRunByUser, containerId, scopeId, fullPath);
                logger.Info("Begin default Google Job. JobId:{0}, JobType: {1}", jobId, jobType);
            }
            return jobId;
        }

        #endregion

        #region Create sub job
        private void SeparateSubJobForDisposal(List<RMGoogleTreeNode> needRunNodes, string jobId, JobRunBy jobRunBy, JobType jobType)
        {
            int subJobCountInConfigFile = KeyValueDao.GetSubJobCountFromDB((int)jobType);
            //int nodeCountInSubJob = RMGlobalConfiguration.AppConfig.NodeCountInSubJob;
            //List<List<RMGoogleTreeNode>> tempGroupNodes = new();
            int subJobCount = needRunNodes.Count;
            //for (int i = 0; i < needRunNodes.Count; i += nodeCountInSubJob)
            //{
            //    tempGroupNodes.Add(needRunNodes.GetRange(i, Math.Min(nodeCountInSubJob, needRunNodes.Count - i)));
            //    subJobCount++;
            //}
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);
            logger.Info("Sub job count for [{0}] is [{1}]", jobId, subJobCount);

            int currentSubjobIndex = 0;
            using (var subJob = new PerformanceScope("AddSubJob", $"AddSubJob{jobId}:{subJobCount}"))
            {
                foreach (var tempNode in needRunNodes)
                {
                    var tempNodes = new List<RMGoogleTreeNode>() { tempNode };
                    string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempNodes, currentSubjobIndex < subJobCountInConfigFile);
                    logger.Debug("Create and queue sub job {0}", subJobId);
                    if (currentSubjobIndex < subJobCountInConfigFile)
                    {
                        logger.Debug("Start sub job {0}", subJobId);
                        JobQueueService.HandleMessage(new JobQueueMessage()
                        {
                            JobId = subJobId,
                            RunBy = jobRunBy,
                            JobType = jobType,
                            CommandLine = string.Format("{0} {1}", jobType, subJobId),
                        });
                    }
                    currentSubjobIndex++;
                }
            }

        }
        private void SeparateSubJob(List<RMGoogleTreeNode> needRunNodes, string jobId, JobRunBy jobRunBy, JobType jobType)
        {
            int subJobCountInConfigFile = KeyValueDao.GetSubJobCountFromDB((int)jobType);
            int nodeCountInSubJob = RMGlobalConfiguration.AppConfig.NodeCountInSubJob;
            List<List<RMGoogleTreeNode>> tempGroupNodes = new();
            int subJobCount = 0;
            for (int i = 0; i < needRunNodes.Count; i += nodeCountInSubJob)
            {
                tempGroupNodes.Add(needRunNodes.GetRange(i, Math.Min(nodeCountInSubJob, needRunNodes.Count - i)));
                subJobCount++;
            }
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);
            logger.Info("Sub job count for [{0}] is [{1}]", jobId, subJobCount);

            int currentSubjobIndex = 0;
            using (var subJob = new PerformanceScope("AddSubJob", $"AddSubJob{jobId}:{subJobCount}"))
            {
                foreach (List<RMGoogleTreeNode> tempNodes in tempGroupNodes)
                {
                    string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempNodes, currentSubjobIndex < subJobCountInConfigFile);
                    logger.Debug("Create and queue sub job {0}", subJobId);
                    if (currentSubjobIndex < subJobCountInConfigFile)
                    {
                        logger.Debug("Start sub job {0}", subJobId);
                        JobQueueService.HandleMessage(new JobQueueMessage()
                        {
                            JobId = subJobId,
                            RunBy = jobRunBy,
                            JobType = jobType,
                            CommandLine = string.Format("{0} {1}", jobType, subJobId),
                        });
                    }
                    currentSubjobIndex++;
                }
            }

        }

        private void SeparateSubJobForApplySetting(List<RMGoogleTreeNode> inheritSettingNodes, Dictionary<string, RMGoogleSetting> settingNodeMapping, string jobId, JobRunBy jobRunBy, JobType jobType)
        {
            int subJobCountInConfigFile = KeyValueDao.GetSubJobCountFromDB((int)jobType);
            List<RMGoogleTreeNode> tempList = new List<RMGoogleTreeNode>();

            // Suport for folder nodes later
            Dictionary<string, List<RMGoogleTreeNode>> groupInheritNodes = inheritSettingNodes.GroupBy(t => t.DriveId).ToDictionary(group => group.Key, group => group.ToList());
            var orderGroup = groupInheritNodes.OrderBy(a => a.Value.Count);
            Dictionary<int, List<RMGoogleTreeNode>> subJobNodeDic = new();
            int count = 0;

            foreach (KeyValuePair<string, List<RMGoogleTreeNode>> gn in orderGroup)
            {
                tempList.AddRange(gn.Value);
                if (tempList.Count >= RMGlobalConfiguration.AppConfig.NodeCountInSubJob)
                {
                    count++;
                    var temp = new List<RMGoogleTreeNode>();
                    temp.AddRange(tempList);
                    subJobNodeDic.Add(count, temp);
                    tempList.Clear();
                }
            }
            if (tempList.Count > 0)
            {
                count++;
                subJobNodeDic.Add(count, tempList);
            }
            SubJobDao.UpdateSubJobCount(jobId, count);
            logger.Info("Sub job count for [{0}] is [{1}]", jobId, count);

            int currentSubjobIndex = 0;
            using (var subJob = new PerformanceScope("AddSubJob", $"AddSubJob{jobId}:{count}"))
            {
                foreach (KeyValuePair<int, List<RMGoogleTreeNode>> dic in subJobNodeDic)
                {

                    string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, count, dic.Value, currentSubjobIndex < subJobCountInConfigFile, settingNodeMapping);
                    logger.Debug("Create and queue sub job {0}", subJobId);
                    if (currentSubjobIndex < subJobCountInConfigFile)
                    {
                        logger.Debug("Start sub job {0}", subJobId);
                        JobQueueService.HandleMessage(new JobQueueMessage()
                        {
                            JobId = subJobId,
                            RunBy = jobRunBy,
                            JobType = jobType,
                            CommandLine = string.Format("{0} {1}", jobType, subJobId),
                        });
                    }
                    currentSubjobIndex++;
                }
            }
        }

        private string CreateSubJob(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<RMGoogleTreeNode> tempList, bool sendNow, Dictionary<string, RMGoogleSetting> groupSettingMap = null)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId };
            if (groupSettingMap.IsNotNullOrEmpty())
            {
                subJob.JobContext.Settings = SerializerHelper.SerializeByDataContractSerializer(groupSettingMap);
            }
            if (tempList != null)
            {
                subJob.JobContext.Content = SerializerHelper.SerializeByDataContractSerializer(tempList);
            }
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }

        #endregion

        private bool ValidateDriveSettingCache(List<NodeCacheInfo> excludeNodeCache, string driveId, string containerId, List<RMSampleGoogleTreeNode> driveTreeNodeList)
        {
            bool isAvailable = true;
            NodeCacheInfo nodeInfo = new()
            {
                ScopeId = driveId,
                ContainerId = containerId
            };

            if (nodeInfo.NodeExistingInCache(excludeNodeCache))
            {
                if (!nodeInfo.NodeIsValid(excludeNodeCache))
                {
                    logger.Warn($"Drive is null or has been move to other container [{driveId}]. Will not add to exclude list.");
                    isAvailable = false;
                }
            }
            else
            {
                var drive = driveTreeNodeList.Find(node => node.Id == driveId);
                if (drive == null || !drive.ParentId.Equals(containerId, StringComparison.OrdinalIgnoreCase))
                {
                    if (!nodeInfo.NodeExistingInCache(excludeNodeCache))
                    {
                        nodeInfo.AddNode2Cache(excludeNodeCache);
                    }
                    logger.Warn($"Drive is null or has been move to other container [{driveId}]. Will not add to exclude list.");
                    isAvailable = false;
                }
                if (!nodeInfo.NodeExistingInCache(excludeNodeCache))
                {
                    nodeInfo.IsValid = true;
                    nodeInfo.AddNode2Cache(excludeNodeCache);
                }

            }
            return isAvailable;
        }

        private bool ValidateContainerSettingCache(List<NodeCacheInfo> excludeNodeCache, string containerId, List<RMSampleGoogleTreeNode> containerTreeNodeList)
        {
            bool isAvailable = true;
            NodeCacheInfo nodeInfo = new NodeCacheInfo()
            {
                ScopeId = containerId,
                ContainerId = containerId
            };
            if (nodeInfo.NodeExistingInCache(excludeNodeCache))
            {
                if (!nodeInfo.NodeIsValid(excludeNodeCache))
                {
                    logger.Warn($"Can't find the container: [{containerId}] in database");
                    isAvailable = false;
                }
            }
            else
            {
                var container = containerTreeNodeList.Find(node => node.Id == containerId);
                if (container == null)
                {
                    if (!nodeInfo.NodeExistingInCache(excludeNodeCache))
                    {
                        nodeInfo.AddNode2Cache(excludeNodeCache);
                    }
                    logger.Warn($"Can't find the container: [{containerId}] in database.");
                    isAvailable = false;
                }
                else if (!nodeInfo.NodeExistingInCache(excludeNodeCache))
                {
                    nodeInfo.IsValid = true;
                    nodeInfo.AddNode2Cache(excludeNodeCache);
                }

            }
            return isAvailable;
        }
        #region Schedule Job
        public async Task<RAReturnMessage> RunEnforceRuleActionScheduleJobAsync(RMGoogleTreeNode selectedNode, JobRunBy jobRunBy)
        {
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                // check permission
                bool hasGControlLicense = await TenantService.HasInitGControlPlatForm();

                if (!GooglePermissionHelper.HasGoogleLicense() && !hasGControlLicense)
                {
                    logger.Warn($"Don't have Google permission and google control license to execute this job, job type [{JobType.GoogleRecordsDisposal}]");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }

                if (selectedNode == null)
                {
                    logger.Error("Failed to add the run disposal schedule job to the job queue. Google Tree Node is null.");
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_GoogleDrive_RunAction_NoAvailableNode");
                    return msg;
                }
                
                if (IsGoogleDrive(selectedNode.Level) && !IsDriveNodeExist(selectedNode))
                {
                    logger.Warn($"Node does not exist, node id {selectedNode.Id}");
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_GoogleDrive_RunAction_NoAvailableNode");
                    return msg;
                }

                var dto = new JobQueueDto
                {
                    JobType = JobType.GoogleRecordsDisposal,
                    JobRunType = JobRunBy.Schedule,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = "RM_TS_RunSchedule",
                    Parameters = JsonConvert.SerializeObject(selectedNode)
                };

                var jobId = JobQueueService.AddToDBJobQueue(dto);
                if (string.IsNullOrEmpty(jobId))
                {
                    logger.Error("Failed to add the run disposal schedule job to the job queue. Job ID is null or empty.");
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
                else
                {
                    logger.Info($"Successfully added the run disposal schedule job [{jobId}] to the job queue.");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while running the run disposal schedule job. Error: {ex}");
            }
            return msg;
        }

        public async Task<RAReturnMessage> RunEnforceRuleActionScheduleJob(RMGoogleTreeNode selectedNode, JobRunBy jobRunBy)
        {
            RAReturnMessage msg = new RAReturnMessage();
            string jobId = string.Empty;
            try
            {
                if (!GooglePermissionHelper.HasGoogleLicense())
                {
                    logger.Warn($"Don't have Google permission to execute this job, job type [{JobType.GoogleRecordsDisposal}]");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }

                if (selectedNode == null)
                {
                    logger.Error("Failed to add the run disposal schedule job to the job queue. Google Tree Node is null.");
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_GoogleDrive_RunAction_NoAvailableNode");
                    return msg;
                }

                if (IsGoogleDrive(selectedNode.Level) && !IsDriveNodeExist(selectedNode))
                {
                    logger.Warn($"Node does not exist, node id {selectedNode.Id}, node name {selectedNode.Name}");
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_GoogleDrive_RunAction_NoAvailableNode");
                    return msg;
                }
                var hasJobRunning = JobMonitorService.HasRunningArchiverJobOnScope([JobType.GoogleRecordsDisposal], selectedNode.Id);
                if (!hasJobRunning)
                {
                    // Create job
                    await ProcessCreateRecordsDisposalJobAsync(jobRunBy, _runBySchedule, selectedNode);
                }
                else
                {
                    logger.Warn($"Current node has job running on. {selectedNode.Id}");
                    jobId = JobMonitorService.CreateJobWithScopeId(JobType.GoogleRecordsDisposal, _runBySchedule, selectedNode.Id, GetGoogleContainerId(selectedNode), selectedNode.FullPath);
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_GoogleDrive_RunAction_NoAvailableNode");
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_GoogleDrive_RunAction_NoAvailableNode");
                    return msg;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while running the run disposal schedule job. Error: {ex}");
            }
            return msg;
        }

        private async Task<string> RunGoogleDataSyncJobAllSettingNodeAsync(JobRunBy jobRunBy, string jobRunByUser, JobType jobType)
        {
            string jobId = string.Empty;
            List<string> runningJobIds = JobMonitorService.GetRunningJobs(jobType);
            if (!runningJobIds.IsNullOrEmpty())
            {
                logger.Info("Current running scheduled data sync job:{0}", string.Join(", ", runningJobIds.ToArray()));

                jobId = JobMonitorService.CreateJob(jobType, jobRunByUser, fullPath: "Global Data Sync Job");
                JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SS_JobSkip");
                return jobId;
            }

            int subJobCountInConfigFile = KeyValueDao.GetSubJobCountFromDB((int)jobType);
            jobId = JobMonitorService.CreateJob(jobType, jobRunByUser, fullPath: "Global Data Sync Job");
            List<RMGoogleTreeNode> availableNode = [];
            var allSetting = GoogleSettingDao.GetAllSettings();

            if (allSetting.IsNullOrEmpty())
            {
                logger.Warn("There is no setting node enable sync data into Explorer.");
                JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_NoIsSyncDrive");
                return jobId;
            }
            Dictionary<string, string> googleDriveRuleSettingContainers = new();

            var processingNodeId = allSetting.Select(s => s.ScopeId.ToString()).ToList();
            var enableNullClassificationGroupIds = allSetting.Where(s => s.ContainerId == s.ScopeId && s.IsNullClassificationSetting).Select(s => s.ContainerId.ToString()).ToList();

            foreach (var setting in allSetting)
            {
                if (!setting.IsSyncData || setting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                {
                    logger.Warn($"GoogleSetting [{setting.Id}] can not run sync. IsSyncData: {setting.IsSyncData}, EnableRecordManagement: {setting.EnableRecordManagement}");
                    continue;
                }

                var container = GoogleRemoteNodeDao.GetGoogleContainerById(setting.ContainerId.ToString());
                if (container == null)
                {
                    logger.Warn($"Can't find the container: [{setting.ContainerId}] in database.");
                    continue;
                }
                RMGoogleTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMGoogleTreeNode>(setting.NodeInfo);
                if (enableNullClassificationGroupIds.IsNotNullOrEmpty() && enableNullClassificationGroupIds.Contains(container.Id))
                {
                    logger.Info("Google drive group enable null classification, drive:{0}", selectedNode.Id);
                    if (!googleDriveRuleSettingContainers.ContainsKey(container.Id))
                    {
                        googleDriveRuleSettingContainers.Add(container.Id, container.DisplayName);
                    }
                    continue;
                }

                if (IsGoogleDrive(selectedNode.Level))
                {
                    var drive = GoogleRemoteNodeDao.GetGoogleDriveById(selectedNode.Id);
                    if (drive == null)
                    {
                        logger.Info("Drive not exist, id:{0}", selectedNode.Id);
                        continue;
                    }

                    if (!drive.ParentId.Equals(setting.ContainerId.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        logger.Info("Drive has been moved to other container, drive:{0}", selectedNode.Id);
                        continue;
                    }

                    availableNode.Add(selectedNode);
                    _settingNodeMapping.TryAdd(setting.ScopeId.ToString(), setting);
                    continue;
                }

                if (IsGoogleContainer(selectedNode.Level))
                {
                    List<RMGoogleTreeNode> drives = await RemoteGoogleNodeService.BrowserRMTreeAsync(selectedNode);
                    if (drives.IsNullOrEmpty())
                    {
                        logger.Info($"Cannot find any drive under container {selectedNode.Id}");
                        continue;
                    }
                    foreach (var drive in drives)
                    {
                        if (processingNodeId.Contains(drive.Id))
                        {
                            continue;
                        }

                        if (!availableNode.Select(n => n.Id).ToList().Contains(drive.Id))
                        {
                            availableNode.Add(drive);
                        }
                        _settingNodeMapping.TryAdd(setting.ScopeId.ToString(), setting);
                    }
                }
            }

            if (availableNode.IsNullOrEmpty())
            {
                if (googleDriveRuleSettingContainers.IsNotNullOrEmpty())
                {
                    logger.Warn($"Google drive container enable null classification. Skip run job. Container ids:{string.Join(',', googleDriveRuleSettingContainers.Keys)}");
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Finished, $"RM_EXO_GroupIsRuleSettingAndSkipApplySetting{I18NEntity.Separator}{string.Join(',', googleDriveRuleSettingContainers.Values)}");
                }
                else
                {
                    logger.Warn("No available drive to run");
                    JobMonitorService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_NoDriveUnderContainerBySchedule");
                }

                return jobId;
            }

            int subJobCount = availableNode.Count % RMGlobalConfiguration.AppConfig.NodeCountInSubJob == 0 ? availableNode.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob : availableNode.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob + 1;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);

            int currentSubjobIndex = 0;
            List<RMGoogleTreeNode> tempList = [];

            foreach (var node in availableNode)
            {
                tempList.Add(node);
                if (tempList.Count == RMGlobalConfiguration.AppConfig.NodeCountInSubJob)
                {
                    string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile, _settingNodeMapping);
                    if (currentSubjobIndex < subJobCountInConfigFile)
                    {
                        JobQueueService.HandleMessage(new JobQueueMessage()
                        {
                            JobId = subJobId,
                            RunBy = jobRunBy,
                            JobType = jobType,
                            CommandLine = string.Format("{0} {1}", jobType, subJobId),
                        });
                    }
                    tempList.Clear();
                    currentSubjobIndex++;
                }
            }
            if (tempList.Count > 0)
            {
                string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile, _settingNodeMapping);
                if (currentSubjobIndex < subJobCountInConfigFile)
                {
                    JobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = jobRunBy,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", jobType, subJobId),
                    });
                }
                tempList.Clear();
            }
            return jobId;
        }

        private bool IsGoogleContainer(int level)
        {
            return level == (int)NodeLevel.GoogleMyDriveContainer || level == (int)NodeLevel.GoogleSharedDriveContainer;
        }

        private bool IsGoogleDrive(int level)
        {
            return level == (int)NodeLevel.GoogleMyDrive || level == (int)NodeLevel.GoogleSharedDrive;
        }

        public RAReturnMessage RunDataSyncJob(JobRunBy jobRunBy, string jobRunByUser = "")
        {
            logger.Debug("start data sync");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();

            try
            {
                bool hasGControlLicense = TenantService.HasInitGControlPlatForm().Result;

                if (!GooglePermissionHelper.HasGoogleLicense() && !hasGControlLicense)
                {   
                    logger.Warn($"Don't have Google permission and google control license to execute this job, job type [{JobType.GoogleDataSynchronization}]");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }

                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.GoogleDataSynchronization,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = string.IsNullOrEmpty(jobRunByUser) ? loginName : jobRunByUser,
                    Parameters = null
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while sync for search, ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        #endregion
    }
}
