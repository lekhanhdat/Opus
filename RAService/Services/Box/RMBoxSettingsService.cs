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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspose.Words.XAttr;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Schedule;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Box.AuditHandler;
using AvePoint.RA.Service.Services.RMSharePointSettings;
using AvePoint.RA.Service.Services.Schedule;
using AvePoint.RA.Service.Services.Settings.AuditHandler;
using Newtonsoft.Json;
using RABox;
using RAExportCommon;

namespace AvePoint.RA.Service.Services.Box
{
    [Audit]
    public class RMBoxSettingsService : BaseContentRepositorySettingsService, IRMBoxSettingsService
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMBoxSettingsService));

        private static readonly RMBoxBrowser BoxBrowser = new RMBoxBrowser();
        public IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IBoxSettingDao BoxSettingDao => PlatformWindsorManager.GetService<IBoxSettingDao>();
        private IRecordOwnerDao RecordOwnerDao => PlatformWindsorManager.GetService<IRecordOwnerDao>();
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        private IBrowserBoxTreeService BrowserBoxTreeService => PlatformWindsorManager.GetService<IBrowserBoxTreeService>();
        private DB.Explorer.Dao.IExplorerDao explorerDao = new DB.Explorer.Dao.CosmosImp.ExplorerDao(true);
        public bool EnqueueDataSyncJob(BoxTreeNode treeNode)
        {
            try
            {
                if (treeNode == null)
                {
                    logger.Error("Failed to add data sync job to the job queue. BoxTreeNode is null.");
                    return false;
                }

                var dto = new JobQueueDto
                {
                    JobType = JobType.BoxDataSynchronisation,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = TenantLocalValue.LogonUserEmail,
                    Parameters = JsonConvert.SerializeObject(treeNode)
                };

                var jobId = JobQueueService.AddToDBJobQueue(dto);
                if (!string.IsNullOrEmpty(jobId))
                {
                    logger.Info($"Successfully added data sync job [{jobId}] to the job queue.");
                    return true;
                }
                else
                {
                    logger.Error("Failed to add data sync job to the job queue. Job ID is null or empty.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while running the data sync job. Error: {ex}");
                return false;
            }
        }

        public void EnqueueDataSyncScheduleJob(bool isFromTimerPage)
        {
            try
            {
                var dto = new JobQueueDto
                {
                    JobType = JobType.BoxDataSynchronisationSchedule,
                    JobRunType = isFromTimerPage ? JobRunBy.Control : JobRunBy.Schedule,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = isFromTimerPage ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule",
                };

                var jobId = JobQueueService.AddToDBJobQueue(dto);
                if (!string.IsNullOrEmpty(jobId))
                {
                    logger.Info($"Successfully added data sync schedule job [{jobId}] to the job queue.");
                }
                else
                {
                    logger.Error("Failed to add data sync schedule job to the job queue. Job ID is null or empty.");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while adding the data sync schedule job to the job queue. Error: {ex}");
            }
        }


        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.BoxRunDataSyncJob, BeforeHandler = typeof(BoxServiceBeforeAuditHandler), AfterHandler = typeof(BoxServiceAfterAuditHandler))]
        public async Task<string> RealRunDataSyncJobAsync(string jobRunByUser, string selectedNodeJson)
        {
            var selectedNode = JsonConvert.DeserializeObject<BoxTreeNode>(selectedNodeJson);

            ArgumentCheck.NotNull(selectedNode, nameof(selectedNode));

            if (CheckRunningDataSyncScheduleJobs())
            {
                return SkipJob(selectedNode, jobRunByUser);
            }

            var scopeIds = GetScopeIds(selectedNode);

            if (CheckRunningJobInScope(scopeIds))
            {
                return SkipJob(selectedNode, jobRunByUser);
            }

            var jobId = RMJobService.CreateJobWithScopeId(JobType.BoxDataSynchronisation, jobRunByUser, selectedNode.Id);
            var needRunningJobNodes = new List<BoxTreeNode>();
            try
            {
                needRunningJobNodes = GetNeedRunningJobNodes(selectedNode);

                if (needRunningJobNodes.Count == 0)
                {
                    SkipJob(jobId);
                    return jobId;
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while assembling runnable nodes. JobId:{0} Error:{1}", jobId, e.ToString());
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                return jobId;
            }

            CreateSubJobs(jobId, JobType.BoxDataSynchronisation, needRunningJobNodes, JobRunBy.Control);

            return jobId;
        }

        private bool CheckRunningDataSyncScheduleJobs()
        {
            return RMJobService.GetRunningJobsCount(JobType.BoxDataSynchronisationSchedule) > 0;
        }

        private string SkipJob(BoxTreeNode selectedNode, string jobRunByUser, JobType jobType = JobType.BoxDataSynchronisation)
        {
            string skippedJobId;
            if (jobType == JobType.BoxRecordsDisposal)
            {
                skippedJobId = RMJobService.CreateJobWithScopeId(jobType, jobRunByUser, selectedNode.Id, selectedNode.ContainerId, selectedNode.FullPath);
            }
            else
            {
                skippedJobId = RMJobService.CreateJobWithScopeId(jobType, jobRunByUser, selectedNode.Id);
            }
            RMJobService.UpdateJobStatus(skippedJobId, JobStatus.Skipped, jobType == JobType.BoxDataSynchronisation ? "RM_BoxDataSync_JobSkip" : "RM_BoxRunAction_JobSkip");
            return skippedJobId;
        }

        private List<string> GetScopeIds(BoxTreeNode selectedNode)
        {
            var scopeIds = new List<string>();
            var tempSelectedNode = selectedNode;
            while (tempSelectedNode != null && tempSelectedNode.Level != RMNodeLevel.Root)
            {
                scopeIds.Add(tempSelectedNode.Id);
                tempSelectedNode = tempSelectedNode.Parent;
            }
            return scopeIds;
        }

        private bool CheckRunningJobInScope(List<string> scopeIds, JobType jobType = JobType.BoxDataSynchronisation)
        {
            var runningJobs = RMJobService.GetRunningJobs(new List<JobType> { jobType });
            return runningJobs.Any(item => scopeIds.Contains(item.ScopeId));
        }

        private List<BoxTreeNode> GetNeedRunningJobNodes(BoxTreeNode selectedNode, JobType jobType = JobType.BoxDataSynchronisation)
        {
            var needRunningJobNodes = new List<BoxTreeNode>();
            List<BoxTreeNode> filteredNodes;

            switch (selectedNode.Level)
            {
                case RMNodeLevel.BoxConnectionGroup:
                    filteredNodes = BoxBrowser.GetConnectionNode(selectedNode)
                                      .Where(node => NeedCheckScheduleInfo(node, jobType))
                                      .SelectMany(node => BoxBrowser.GetBoxUserNode(node)
                                                                    .Where(userNode => NeedCheckScheduleInfo(userNode, jobType) &&
                                                                                       !CheckRunningJobInScope(new List<string> { userNode.Id }, jobType)))
                                      .ToList();
                    foreach (var node in filteredNodes)
                    {
                        node.StartJobNodeLevel = RMNodeLevel.BoxConnectionGroup;
                    }
                    needRunningJobNodes.AddRange(filteredNodes);
                    break;
                case RMNodeLevel.BoxConnection:
                    filteredNodes = BoxBrowser.GetBoxUserNode(selectedNode)
                                      .Where(node => NeedCheckScheduleInfo(node, jobType) && node.ConnectionId == selectedNode.Id &&
                                                     !CheckRunningJobInScope(new List<string> { node.Id }, jobType))
                                      .ToList();
                    foreach (var node in filteredNodes)
                    {
                        node.StartJobNodeLevel = RMNodeLevel.BoxConnection;
                    }
                    needRunningJobNodes.AddRange(filteredNodes);
                    break;
                case RMNodeLevel.BoxUser:
                    selectedNode.StartJobNodeLevel = RMNodeLevel.BoxUser;
                    needRunningJobNodes.Add(selectedNode);
                    break;
                default:
                    selectedNode.StartJobNodeLevel = RMNodeLevel.BoxFolder;
                    needRunningJobNodes.Add(selectedNode);
                    break;
            }
            return needRunningJobNodes;
        }

        private bool NeedCheckScheduleInfo(BoxTreeNode currentNode, JobType jobType)
        {
            return jobType != JobType.BoxRecordsDisposal || GetScheduleInfo(currentNode) == null;
        }

        private void SkipJob(string jobId, JobType jobType = JobType.BoxDataSynchronisation)
        {
            RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, jobType == JobType.BoxDataSynchronisation ? "RM_Box_DataSync_NoAvailableNode" : "RM_Box_RunAction_NoAvailableNode");
        }

        private void CreateSubJobs(string jobId, JobType jobType, List<BoxTreeNode> needRunningJobNodes, JobRunBy runBy)
        {
            var subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            int nodeCountInSubJob = RMGlobalConfiguration.AppConfig.NodeCountInSubJob;
            var groupedNodesByConnectionId = needRunningJobNodes.GroupBy(node => node.ConnectionId).ToDictionary(group => group.Key, group => group.ToList());
            int currentSubjobIndex = 0;

            int totalSubJobCount = groupedNodesByConnectionId.Sum(group => group.Value.Count > nodeCountInSubJob ? (group.Value.Count / nodeCountInSubJob) + (group.Value.Count % nodeCountInSubJob == 0 ? 0 : 1) : 1);

            SubJobDao.UpdateSubJobCount(jobId, totalSubJobCount);

            foreach (var group in groupedNodesByConnectionId)
            {
                int nodeIndex = 0;
                while (nodeIndex < group.Value.Count)
                {
                    List<BoxTreeNode> nodesForCurrentSubJob = group.Value.Skip(nodeIndex).Take(nodeCountInSubJob).ToList();

                    string subJobId = $"{jobId}_{currentSubjobIndex:D3}";

                    var subJob = new RMSubJob
                    {
                        Id = subJobId,
                        ParentId = jobId,
                        StartTime = DateTime.UtcNow.Ticks,
                        JobType = (int)jobType,
                        Progress = 0,
                        Status = (int)JobStatus.Wait,
                        Weight = 100d / totalSubJobCount,
                        Runable = currentSubjobIndex < subJobCountInConfigFile ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting,
                        JobContext = new RMJobContext
                        {
                            JobId = subJobId,
                            Content = JsonConvert.SerializeObject(nodesForCurrentSubJob)
                        },
                        String1 = $"{nodesForCurrentSubJob.First().ContainerId}/{nodesForCurrentSubJob.First().ConnectionId}"
                    };

                    SubJobDao.CreateJob(subJob);
                    logger.Info($"Create sub job {jobType}-{subJobId} succeed.");

                    if (currentSubjobIndex < subJobCountInConfigFile)
                    {
                        JobQueueService.HandleMessage(new JobQueueMessage
                        {
                            JobId = subJobId,
                            RunBy = runBy,
                            JobType = jobType,
                            CommandLine = $"{jobType} {subJobId}"
                        });
                    }
                    currentSubjobIndex++;
                    nodeIndex += nodeCountInSubJob;
                }
            }
        }


        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.TimerJobSettings, Action = AuditAction.BoxRunDataSyncJob, BeforeHandler = typeof(BoxServiceBeforeAuditHandler), AfterHandler = typeof(BoxServiceAfterAuditHandler))]
        public async Task<string> RealRunDataSyncScheduleJobAsync(string jobRunByUser)
        {
            // check permission
            if (!(new BoxPermissionManager().HasBoxLicense()))
                return string.Empty;

            if (CheckRunningDataSyncScheduleJobs())
            {
                return SkipRunningScheduleJob(jobRunByUser);
            }

            var jobId = RMJobService.CreateJob(JobType.BoxDataSynchronisationSchedule, jobRunByUser);
            var settings = await GetSettingInfoAsync();
            var needRunningJobNodes = new List<BoxTreeNode>();

            try
            {
                foreach (var item in settings)
                {
                    needRunningJobNodes.AddRange(GetNeedRunningJobNodes(item.SelectedNode, JobType.BoxDataSynchronisationSchedule));
                }

                if (!needRunningJobNodes.Any())
                {
                    return SkipNoAvailableNodes(jobId);
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while assembling runnable nodes. JobId:{0} Error:{1}", jobId, e.ToString());
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                return jobId;
            }
            
            CreateSubJobs(jobId, JobType.BoxDataSynchronisationSchedule, needRunningJobNodes, JobRunBy.Schedule);
            return jobId;
        }

        private string SkipRunningScheduleJob(string jobRunByUser)
        {
            var skippedJobId = RMJobService.CreateJob(JobType.BoxDataSynchronisationSchedule, jobRunByUser);
            RMJobService.UpdateJobStatus(skippedJobId, JobStatus.Skipped, "RM_BoxDataSync_JobSkip");
            return skippedJobId;
        }

        private string SkipNoAvailableNodes(string jobId)
        {
            RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Box_DataSync_NoAvailableNode");
            return jobId;
        }

        public async Task<List<BoxSettingDto>> GetSettingInfoAsync()
        {
            try
            {
                var groupSettings = new List<BoxSettingDto>();
                var settings = await BoxSettingDao.FindListAsync(s => s.ConnectionGroupId == new Guid(s.ScopeId));

                foreach (var setting in settings)
                {
                    var settingInfo = new BoxSettingDto();
                    if (setting != null)
                    {
                        var termDefaultValue = TermDao.GetRMTermByGuId(setting.DefaultTermId);
                        settingInfo.ScopeId = setting.ScopeId;
                        settingInfo.TermSetId = setting.TermSetId;
                        settingInfo.TermSetName = setting.TermSetName;
                        settingInfo.TermId = setting.TermId;
                        settingInfo.TermName = setting.TermName;
                        settingInfo.TermScopeFullPath = setting.TermId != Guid.Empty ?
                            TermDao.GetTermNamesPathByTermId(setting.TermId) :
                            TermDao.GetTermSetNamesPathByTermSetId(setting.TermSetId);
                        settingInfo.DefaultTermId = setting.DefaultTermId;
                        settingInfo.DefaultTermName = termDefaultValue?.Name ?? setting.DefaultTermName;
                        settingInfo.DefaultTermFullPath = setting.DefaultTermId == Guid.Empty ? "" : TermDao.GetTermNamesPathByTermId(setting.DefaultTermId);
                        settingInfo.IsDefaultTermRemoved = termDefaultValue == null || termDefaultValue.IsRemoved;
                        settingInfo.IsDefaultTermDeprecated = termDefaultValue == null || termDefaultValue.IsDeprecated || TermDao.IsExpiredTerm(termDefaultValue.Id);
                        settingInfo.NeedCheckDefaultValue = setting.NeedCheckDefaultValue;
                        settingInfo.ApplyExistType = setting.ApplyExistType;
                        if (setting.NeedCheckDefaultValue && setting.ApplyExistType == (int)ApplyExistingTermType.None)
                        {
                            settingInfo.ApplyExistType = (int)ApplyExistingTermType.SkipAndKeep;
                        }
                        settingInfo.DeployTermMethod = setting.TermSetId == Guid.Empty ? DeployTermMethod.NoDefaultTerm : (DeployTermMethod)setting.DeployTermMethod;
                        settingInfo.AutoClassificationRules = setting.AutoClassificationRules == null ? null : SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(setting.AutoClassificationRules);
                        settingInfo.SelectedNode = SerializerHelper.DeserializeByDataContractSerializer<BoxTreeNode>(setting.NodeInfo);
                        SetBoxAutoTermStatus(settingInfo.AutoClassificationRules);
                        await ConvertClassificationRuleTimeZoneAsync(settingInfo.AutoClassificationRules);
                        ConvertClassificationRuleAndOrExpression(settingInfo.AutoClassificationRules);
                        settingInfo.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(setting.Id, RecordOwnerSettingType.Box);
                        settingInfo.ApprovalType = (int)setting.ApprovalType;
                        settingInfo.WorkflowReferenceId = setting.WorkflowReferenceId;
                        settingInfo.EMailToRecordOwner = setting.EMailToRecordOwner;
                        groupSettings.Add(settingInfo);
                    }
                }
                
                return groupSettings;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while get all setting infoes. Error: {e}");
                return null;
            }
        }

        public async Task<(bool, BoxSettingDto)> TryGetSettingInfoAsync(string scopeId, string containerId, string connectionId = "", string userId = "")
        {
            BoxSettingDto settingInfo = new BoxSettingDto();
            try
            {
                if (!BoxSettingDao.TryGet(scopeId, containerId, connectionId, userId, out var setting))
                {
                    return (false, settingInfo);
                }
                var termDefaultValue = TermDao.GetRMTermByGuId(setting.DefaultTermId);

                settingInfo.ScopeId = setting.ScopeId;
                settingInfo.TermSetId = setting.TermSetId;
                settingInfo.TermSetName = setting.TermSetName;
                settingInfo.TermId = setting.TermId;
                settingInfo.TermName = setting.TermName;
                settingInfo.TermScopeFullPath = setting.TermId != Guid.Empty ?
                    TermDao.GetTermNamesPathByTermId(setting.TermId) :
                    TermDao.GetTermSetNamesPathByTermSetId(setting.TermSetId);
                settingInfo.DefaultTermId = setting.DefaultTermId;
                settingInfo.DefaultTermName = termDefaultValue?.Name ?? setting.DefaultTermName;
                settingInfo.DefaultTermFullPath = setting.DefaultTermId == Guid.Empty ? "" : TermDao.GetTermNamesPathByTermId(setting.DefaultTermId);
                settingInfo.IsDefaultTermRemoved = termDefaultValue == null || termDefaultValue.IsRemoved;
                settingInfo.IsDefaultTermDeprecated = termDefaultValue == null || termDefaultValue.IsDeprecated || TermDao.IsExpiredTerm(termDefaultValue.Id);
                settingInfo.NeedCheckDefaultValue = setting.NeedCheckDefaultValue;
                settingInfo.ApplyExistType = setting.ApplyExistType;
                settingInfo.RunAutoFullJob = setting.RunAutoFullJob;
                if (setting.NeedCheckDefaultValue && setting.ApplyExistType == (int)ApplyExistingTermType.None)
                {
                    settingInfo.ApplyExistType = (int)ApplyExistingTermType.SkipAndKeep;
                }
                settingInfo.DeployTermMethod = setting.TermSetId == Guid.Empty ? DeployTermMethod.NoDefaultTerm : (DeployTermMethod)setting.DeployTermMethod;
                settingInfo.AutoClassificationRules = setting.AutoClassificationRules == null ? null : SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(setting.AutoClassificationRules);
                settingInfo.AutoJobOption = (AutoJobOption)setting.AutoJobOption;
                SetBoxAutoTermStatus(settingInfo.AutoClassificationRules);
                await ConvertClassificationRuleTimeZoneAsync(settingInfo.AutoClassificationRules);
                ConvertClassificationRuleAndOrExpression(settingInfo.AutoClassificationRules);
                settingInfo.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(setting.Id, RecordOwnerSettingType.Box);
                settingInfo.ApprovalType = (int)setting.ApprovalType;
                settingInfo.WorkflowReferenceId = setting.WorkflowReferenceId;
                settingInfo.EMailToRecordOwner = setting.EMailToRecordOwner;
                return (true, settingInfo);
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while try get setting info by [{scopeId}]. Error: {e}");
                return (false, settingInfo);
            }
        }

        private void SetBoxAutoTermStatus(List<ClassificationRule> autoRules)
        {
            try
            {
                if (autoRules == null)
                {
                    return;
                }
                foreach (var autoRule in autoRules)
                {
                    if (string.IsNullOrEmpty(autoRule.TermId) || autoRule.TermId == Guid.Empty.ToString())
                    {
                        continue;
                    }
                    var term = TermDao.GetRMTermByGuId(new Guid(autoRule.TermId));
                    autoRule.TermIsDeprecated = term.IsDeprecated || TermDao.IsExpiredTerm(term.Id);
                    autoRule.TermIsRemoved = term.IsRemoved;
                }
            }
            catch (Exception e)
            {
                logger.Error("Set auto term status error:{0}", e.ToString());
            }
        }

        public async Task ResetSyncSettingAsync(string scopeId, string containerId, string connectionId = "", string userId = "")
        {
            if (BoxSettingDao.TryGet(scopeId, containerId, connectionId, userId, out var settingInfo))
            {
                settingInfo.NeedCheckDefaultValue = false;
                settingInfo.ApplyExistType = 0;
                settingInfo.RunAutoFullJob = false;
                await BoxSettingDao.UpdateAsync(settingInfo);
            }
        }
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.BoxSaveTermSetting, BeforeHandler = typeof(BoxServiceBeforeAuditHandler), AfterHandler = typeof(BoxServiceAfterAuditHandler))]
        public async Task SaveNodeSettingAsync(BoxSettingDto dto)
        {
            if (dto is null)
            {
                throw new ArgumentNullException(nameof(dto), $"The box setting is null or empty. Unable to save node setting.");
            }
            if (dto.SelectedNode is null)
            {
                throw new ArgumentNullException(nameof(dto.SelectedNode), $"The node is null or empty. Unable to save node setting.");
            }
            AddFilterCretiaProperty(dto.AutoClassificationRules);
            if (Guid.TryParse(dto.SelectedNode.ContainerId, out _))
            {
                try
                {
                    await BoxSettingDao.UpdateOrCreateSettingAsync(dto);
                }
                catch (Exception ex)
                {
                    logger.Error($"An error occurred while saving the node setting. Error: {ex}");
                }
            }
            else
            {
                throw new Exception("Cannot convert connection group id from string to Guid.");
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.BoxDeactiveSetting, BeforeHandler = typeof(BoxServiceBeforeAuditHandler), AfterHandler = typeof(BoxServiceAfterAuditHandler))]
        public async Task SaveActiveSettingAsync(BoxSettingDto dto)
        {
            if (dto is null)
            {
                throw new ArgumentNullException(nameof(dto), $"The box setting is null or empty. Unable to save active setting.");
            }
            if (dto.SelectedNode is null)
            {
                throw new ArgumentNullException(nameof(dto.SelectedNode), $"The node is null or empty. Unable to save active setting.");
            }
            if (Guid.TryParse(dto.SelectedNode.ContainerId, out _))
            {
                try
                {
                    await BoxSettingDao.UpdateOrCreateSettingAsync(dto);
                }
                catch (Exception ex)
                {
                    logger.Error($"An error occurred while saving the active setting. Error: {ex}");
                }
            }
            else
            {
                throw new Exception("Cannot convert connection group id from string to Guid.");
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.BoxInheritSetting, BeforeHandler = typeof(BoxServiceBeforeAuditHandler), AfterHandler = typeof(BoxServiceAfterAuditHandler))]
        public async Task InheritParentSettingAsync(BoxTreeNode node)
        {
            if (node is null)
            {
                throw new ArgumentNullException(nameof(node), $"The node is null or empty. Unable to inherit parent setting.");
            }
            try
            {
                logger.Info("Inherit parent settings");
                await BoxSettingDao.DeleteSettingAsync(node.Id, new Guid(node.ContainerId));
                ScheduleType type = ScheduleType.BoxDisposalSchedule;
                var profileId = ScheduleService.GetProfileId(node);
                ScheduleService.DeleteSchedules(type, profileId);
            }
            catch (Exception ex)
            {
                logger.Warn("Inherit parent setting to DB Error {0}", ex.ToString());
            }
        }

        public async Task<BoxSettingDto> LoadNodeSettingAsync(BoxTreeNode node)
        {
            if (node is null)
            {
                throw new ArgumentNullException(nameof(node), $"The node is null or empty. Unable to load node setting.");
            }
            var dto = new BoxSettingDto();
            var connectionGroupId = node.Level == RMNodeLevel.BoxConnectionGroup ? node.Id : node.ContainerId;
            RMBoxSetting gSetting = BoxSettingDao.GetSettingByScopeIdAndGroupId(node.ContainerId.ToString(), connectionGroupId);
            if (gSetting != null)
            {
                var termScope = TermDao.GetRMTermByGuId(gSetting.TermId);
                var termDefaultValue = TermDao.GetRMTermByGuId(gSetting.DefaultTermId);
                RMTermSet termSet = null;
                if (gSetting.TermId == Guid.Empty)
                {
                    termSet = TermDao.GetRMTermSetByGuid(gSetting.TermSetId);
                }
                node.IconStatus = IconStatus.Inhert;
                dto.TermSetId = gSetting.TermSetId;
                dto.TermId = gSetting.TermId;
                dto.DefaultTermId = gSetting.DefaultTermId;
                dto.DefaultTermName = termDefaultValue == null ? gSetting.DefaultTermName : termDefaultValue.Name;
                dto.DefaultTermFullPath = gSetting.DefaultTermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(gSetting.DefaultTermId) : "";
                dto.TermSetName = gSetting.TermSetName;
                dto.TermName = gSetting.TermName;
                dto.IsTermRemoved = (termScope == null ? termSet?.IsRemoved : termScope?.IsRemoved) ?? false;
                dto.IsDefaultTermRemoved = termDefaultValue == null ? false : termDefaultValue.IsRemoved;
                dto.IsTermDeprecated = termScope != null && (termScope.IsDeprecated || TermDao.IsExpiredTerm(termScope.Id));
                dto.IsDefaultTermDeprecated = termDefaultValue == null ? false : termDefaultValue.IsDeprecated || TermDao.IsExpiredTerm(termDefaultValue.Id);
                dto.NeedCheckDefaultValue = gSetting.NeedCheckDefaultValue;
                dto.IsActive = gSetting.IsActive;
                dto.ApplyExistType = gSetting.ApplyExistType;
                if (gSetting.NeedCheckDefaultValue && gSetting.ApplyExistType == (int)ApplyExistingTermType.None)
                {
                    dto.ApplyExistType = (int)ApplyExistingTermType.SkipAndKeep;
                }
                dto.AutoClassificationRules = gSetting.AutoClassificationRules == null ?
                    null : SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(gSetting.AutoClassificationRules);
                SetBoxAutoTermStatus(dto.AutoClassificationRules);
                await ConvertClassificationRuleTimeZoneAsync(dto.AutoClassificationRules);
                ConvertClassificationRuleAndOrExpression(dto.AutoClassificationRules);
                dto.DeployTermMethod = gSetting.TermSetId == Guid.Empty ? DeployTermMethod.NoDefaultTerm : (DeployTermMethod)gSetting.DeployTermMethod;
                dto.RunAutoFullJob = gSetting.RunAutoFullJob;
                dto.AutoJobOption = (AutoJobOption)gSetting.AutoJobOption == AutoJobOption.None ? AutoJobOption.SkipAndKeep : (AutoJobOption)gSetting.AutoJobOption;
                dto.TermScopeFullPath = gSetting.TermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(gSetting.TermId) : TermDao.GetTermSetNamesPathByTermSetId(gSetting.TermSetId);
                dto.ScopeId = gSetting.ScopeId;
                dto.EMailToRecordOwner = gSetting.EMailToRecordOwner;
                dto.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(gSetting.Id, RecordOwnerSettingType.Box);
                dto.ApprovalType = (int)gSetting.ApprovalType;
                dto.WorkflowReferenceId = gSetting.WorkflowReferenceId;
            }
            dto.IsCustomSetting = false;
            var bSetting = BoxSettingDao.GetSettingByScopeIdAndGroupId(node.Id, connectionGroupId);
            if (bSetting == null)
            {
                if (node.Parent != null && node.Parent.Level != RMNodeLevel.Root)
                {
                    var parentNode = node.Parent;
                    bSetting = LoadNodeParentSetting(parentNode);
                    dto.IsCustomSetting = false;
                }
            }
            else
            {
                node.IconStatus = IconStatus.Break;
                if (node.Level != RMNodeLevel.BoxConnectionGroup)
                {
                    dto.IsCustomSetting = true;
                }
            }
            if (bSetting != null)
            {
                var termScope = TermDao.GetRMTermByGuId(bSetting.TermId);
                var defaultTerm = TermDao.GetRMTermByGuId(bSetting.DefaultTermId);
                RMTermSet termSet = null;
                if (bSetting.TermId == Guid.Empty)
                {
                    termSet = TermDao.GetRMTermSetByGuid(bSetting.TermSetId);
                }
                dto.TermSetId = bSetting.TermSetId;
                dto.TermId = bSetting.TermId;
                dto.DefaultTermId = bSetting.DefaultTermId;
                dto.DefaultTermName = defaultTerm == null ? bSetting.DefaultTermName : defaultTerm.Name;
                dto.DefaultTermFullPath = bSetting.DefaultTermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(bSetting.DefaultTermId) : "";
                dto.TermSetName = bSetting.TermSetName;
                dto.TermName = termScope == null ? bSetting.TermName : termScope.Name;
                dto.IsTermRemoved = (termScope == null ? termSet?.IsRemoved : termScope?.IsRemoved) ?? false;
                dto.IsDefaultTermRemoved = defaultTerm != null && defaultTerm.IsRemoved;
                dto.IsTermDeprecated = termScope != null && (termScope.IsDeprecated || TermDao.IsExpiredTerm(termScope.Id));
                dto.IsDefaultTermDeprecated = defaultTerm != null && (defaultTerm.IsDeprecated || TermDao.IsExpiredTerm(defaultTerm.Id));
                dto.NeedCheckDefaultValue = bSetting.NeedCheckDefaultValue;
                dto.IsActive = bSetting.IsActive;
                dto.ApplyExistType = bSetting.ApplyExistType;
                if (bSetting.NeedCheckDefaultValue && bSetting.ApplyExistType == (int)ApplyExistingTermType.None)
                {
                    dto.ApplyExistType = (int)ApplyExistingTermType.SkipAndKeep;
                }
                dto.AutoClassificationRules = bSetting.AutoClassificationRules == null ?
                    null : SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(bSetting.AutoClassificationRules);
                SetBoxAutoTermStatus(dto.AutoClassificationRules);
                await ConvertClassificationRuleTimeZoneAsync(dto.AutoClassificationRules);
                ConvertClassificationRuleAndOrExpression(dto.AutoClassificationRules);
                dto.DeployTermMethod = bSetting.TermSetId == Guid.Empty ? DeployTermMethod.NoDefaultTerm : (DeployTermMethod)bSetting.DeployTermMethod;
                dto.RunAutoFullJob = bSetting.RunAutoFullJob;
                dto.AutoJobOption = (AutoJobOption)bSetting.AutoJobOption == AutoJobOption.None ? AutoJobOption.SkipAndKeep : (AutoJobOption)bSetting.AutoJobOption;
                dto.TermScopeFullPath = bSetting.TermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(bSetting.TermId) : TermDao.GetTermSetNamesPathByTermSetId(bSetting.TermSetId);
                dto.ScopeId = bSetting.ScopeId;
                dto.EMailToRecordOwner = bSetting.EMailToRecordOwner;
                dto.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(bSetting.Id, RecordOwnerSettingType.Box);
                dto.ApprovalType = (int)bSetting.ApprovalType;
                dto.WorkflowReferenceId = bSetting.WorkflowReferenceId;
            }
            var profileId = ScheduleService.GetProfileId(node);
            var disposeSchedule = await ScheduleService.GetScheduleAsync(profileId, ScheduleType.BoxDisposalSchedule);
            if (disposeSchedule != null)
            {
                var simplifyZoneInfo = DateTimeUtil.GetSimplifyZoneInfo(disposeSchedule.TimeZoneId);
                disposeSchedule.StartTime = string.Format($"{disposeSchedule.StartTime} {simplifyZoneInfo}");
                disposeSchedule.EndTime = string.Format($"{disposeSchedule.EndTime} {simplifyZoneInfo}");
                dto.DisposeScheduleInfo = disposeSchedule;
                //dto.IsCustomSetting = true;
                node.IconStatus = IconStatus.Break;
            }
            else
            {
                var ancestryDisposeSchedule = await ScheduleService.GetAncestryScheduleAsync(profileId, ScheduleType.BoxDisposalSchedule);
                if (ancestryDisposeSchedule != null)
                {
                    var simplifyZoneInfo = DateTimeUtil.GetSimplifyZoneInfo(ancestryDisposeSchedule.TimeZoneId);
                    ancestryDisposeSchedule.StartTime = string.Format($"{ancestryDisposeSchedule.StartTime} {simplifyZoneInfo}");
                    ancestryDisposeSchedule.EndTime = string.Format($"{ancestryDisposeSchedule.EndTime} {simplifyZoneInfo}");
                    dto.DisposeScheduleInfo = ancestryDisposeSchedule;
                    dto.DisposeScheduleInfo.Id = "1";
                }
                else
                {
                    dto.DisposeScheduleInfo = null;
                }
            }
            dto.SelectedNode = node;
            return dto;
        }

        private RMBoxSetting LoadNodeParentSetting(BoxTreeNode node)
        {
            RMBoxSetting setting = null;
            if (node.Level == RMNodeLevel.BoxConnectionGroup)
            {
                return setting;
            }
            setting = BoxSettingDao.GetSettingByScopeIdAndGroupId(node.Id, node.ContainerId);
            if (setting == null)
            {
                setting = LoadNodeParentSetting(node.Parent);
            }
            return setting;
        }

        public RAReturnMessage EnqueueRunRecordsDisposalJob(BoxTreeNode treeNode)
        {
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                if (treeNode == null)
                {
                    logger.Error("Failed to add the run disposal job to the job queue. BoxTreeNode is null.");
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_JM_FS_Disposal_NoSC");
                    return msg;
                }

                var dto = new JobQueueDto
                {
                    JobType = JobType.BoxRecordsDisposal,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = TenantLocalValue.LogonUserEmail,
                    Parameters = JsonConvert.SerializeObject(treeNode)
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
                logger.Error($"An error occurred while running the run disposal job. Error: {ex}");
            }
            return msg;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.BoxRunDisposalJob, BeforeHandler = typeof(BoxServiceBeforeAuditHandler), AfterHandler = typeof(BoxServiceAfterAuditHandler))]
        public async Task<string> RealRunBoxRecordsDisposalJobAsync(string jobRunByUser, string selectedNodeJson)
        {
            var selectedNode = JsonConvert.DeserializeObject<BoxTreeNode>(selectedNodeJson);

            ArgumentCheck.NotNull(selectedNode, nameof(selectedNode));

            var scopeIds = GetScopeIds(selectedNode);

            if (CheckRunningJobInScope(scopeIds, JobType.BoxRecordsDisposal))
            {
                return SkipJob(selectedNode, jobRunByUser, JobType.BoxRecordsDisposal);
            }

            var jobId = RMJobService.CreateJobWithScopeId(JobType.BoxRecordsDisposal, jobRunByUser, selectedNode.Id, selectedNode.ContainerId, selectedNode.FullPath);

            var needRunningJobNodes = new List<BoxTreeNode>();
            try
            {
                needRunningJobNodes = GetNeedRunningJobNodes(selectedNode, JobType.BoxRecordsDisposal);

                if (needRunningJobNodes.Count == 0)
                {
                    SkipJob(jobId, JobType.BoxRecordsDisposal);
                    return jobId;
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while assembling runnable nodes. JobId:{0} Error:{1}", jobId, e.ToString());
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                return jobId;
            }

            CreateSubJobs(jobId, JobType.BoxRecordsDisposal, needRunningJobNodes, JobRunBy.Control);

            return jobId;
        }
        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.TimerJobSettings, Action = AuditAction.ApprovalProcessConfig, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public async Task<string> RealRunBoxRecordsDisposalJobForApprovalAsync(string jobRunByUser)
        {
            try
            {
                var rootNode = await BrowserBoxTreeService.GetRootNode();
                var realRootNode = AvePoint.RA.Browser.Browser.Box.BoxBrowser.ConvertToBoxTreeNode(rootNode);
                var contract = AvePoint.RA.Browser.Browser.Box.BoxBrowser.ConvertToBoxBrowserContract(realRootNode);
                var children = await BrowserBoxTreeService.GetChildrenWithSettingIcon(contract);
                var groupNodes = children.ConvertAll(child => AvePoint.RA.Browser.Browser.Box.BoxBrowser.ConvertToBoxTreeNode(child));
                List<BoxTreeNode> realNeedRunJobNodes = new List<BoxTreeNode>();
                foreach (var node in groupNodes)
                {
                    var selectedNode = node;
                    ArgumentCheck.NotNull(selectedNode, nameof(selectedNode));

                    var scopeIds = GetScopeIds(selectedNode);
                    var needRunningJobNodes = GetNeedRunningJobNodes(selectedNode, JobType.BoxRecordsDisposal);
                    foreach (var needRunningJobNode in needRunningJobNodes)
                    {
                        bool exsitApproval = explorerDao.Exist(e => e.ManualApprovedStatus == (int)Contract.SOApproveDBStatus.Approved && e.ManualArchiveStatus == (int)ActionStatus.None && e.ContainerId.Equals(needRunningJobNode.ConnectionId,StringComparison.OrdinalIgnoreCase));
                        if (exsitApproval)
                        {
                            needRunningJobNode.IsProcessApprovalDatasOnly = true;
                            realNeedRunJobNodes.Add(needRunningJobNode);
                        }
                        else
                        {
                            logger.Info($"No approval box record found for the user.name:{needRunningJobNode.DisplayName}");
                        }
                    }

                }
                if (realNeedRunJobNodes.Count == 0)
                {
                    logger.Info($"No approval box record found for the user.continue process");
                    return string.Empty;
                }
                var jobId = RMJobService.CreateJobWithScopeId(JobType.BoxRecordsDisposal, jobRunByUser, "RM_BOX_Virtual_Container", null, JobType.BoxRecordsDisposal.ToString());
                CreateSubJobs(jobId, JobType.BoxRecordsDisposal, realNeedRunJobNodes, JobRunBy.Control);
                return jobId;
            }
            catch(Exception ex)
            {
                logger.Error($"An error occurred while running the run box disposal job. Error: {ex}");
                return string.Empty;
            }
        }

        public RAReturnMessage RunBoxEnforceRuleActionScheduleJob(BoxSettingDto boxSetting, JobRunBy jobRunBy)
        {
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                // check permission
                if(!(new BoxPermissionManager().HasBoxLicense()))
                    return msg;

                if (boxSetting.SelectedNode == null)
                {
                    logger.Error("Failed to add the run disposal schedule job to the job queue. BoxTreeNode is null.");
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_Box_RunAction_NoAvailableNode");
                    return msg;
                }

                var dto = new JobQueueDto
                {
                    JobType = JobType.BoxRecordsDisposal,
                    JobRunType = JobRunBy.Schedule,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = "RM_TS_RunSchedule",
                    Parameters = JsonConvert.SerializeObject(boxSetting.SelectedNode)
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

        public ScheduleInfo GetScheduleInfo(BoxTreeNode node)
        {
            ScheduleInfo disposeScheduleInfo = null;

            var profileId = ScheduleService.GetProfileId(node);

            var disposeSchedule = ScheduleService.GetScheduleAsync(profileId, ScheduleType.BoxDisposalSchedule).Result;
            if (disposeSchedule != null)
            {
                var simplifyZoneInfo = DateTimeUtil.GetSimplifyZoneInfo(disposeSchedule.TimeZoneId);
                disposeSchedule.StartTime = string.Format($"{disposeSchedule.StartTime} {simplifyZoneInfo}");
                disposeSchedule.EndTime = string.Format($"{disposeSchedule.EndTime} {simplifyZoneInfo}");
                disposeScheduleInfo = disposeSchedule;
            }

            return disposeScheduleInfo;
        }

        public ScheduleInfo GetScheduleInfo(List<Guid> ids)
        {
            ScheduleInfo disposeScheduleInfo = null;

            var profileId = ScheduleService.GetProfileId(ids);

            var disposeSchedule = ScheduleService.GetScheduleAsync(profileId, ScheduleType.BoxDisposalSchedule).Result;
            if (disposeSchedule != null)
            {
                var simplifyZoneInfo = DateTimeUtil.GetSimplifyZoneInfo(disposeSchedule.TimeZoneId);
                disposeSchedule.StartTime = string.Format($"{disposeSchedule.StartTime} {simplifyZoneInfo}");
                disposeSchedule.EndTime = string.Format($"{disposeSchedule.EndTime} {simplifyZoneInfo}");
                disposeScheduleInfo = disposeSchedule;
            }

            return disposeScheduleInfo;
        }

        public async Task<bool> SyncADUsersAsync(List<ToUserInfo> users)
        {
            var result = true;
            try
            {
                if (users != null && users.Count > 0)
                {
                    await UserService.SyncUsersAsync(TenantLocalValue.LogonGroupId, users);
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }
    }
}
