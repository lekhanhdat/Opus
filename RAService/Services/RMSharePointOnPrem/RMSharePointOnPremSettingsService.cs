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
using AvePoint.GCommon.Utility.TransientFault;
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Global.JobMessage;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.Contract.RMWeb.SingalR;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RACommonUtility.SharePointOnPrem;
using AvePoint.RA.Service.Services.Explorer.AuditHandler;
using AvePoint.RA.Service.Services.RMFileSystemSettings;
using AvePoint.RA.Service.Services.RMSharePointSettings;
using AvePoint.RA.Service.Services.RMSharePointSettings.AuditHandler;
using AvePoint.RA.Service.Services.RMSharePointTaxonomy.AuditHandler;
using AvePoint.RA.Service.Services.SharePointSetting.AuditHandler;
using AvePoint.RA.SharePoint.Discover;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Global = AvePoint.RA.Contract.Global.Object;

namespace AvePoint.RA.Service.Services.RMSharePointOnPrem
{
    [Audit]
    public class RMSharePointOnPremSettingsService : BaseContentRepositorySettingsService, IRMSharePointOnPremSettingsService
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RMSharePointOnPremSettingsService));
        private static readonly AveRetryPolicy RetryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(12, TimeSpan.FromSeconds(10)));
        private IUniqueIdSettingService UniqueIdSettingService => PlatformWindsorManager.GetService<IUniqueIdSettingService>();
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();
        private IHybridBrowserService HybridBrowserService => PlatformWindsorManager.GetService<IHybridBrowserService>();
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();

        private ISharePointOnPremiseSettingDao SharePointOnPremiseSettingDao => PlatformWindsorManager.GetService<ISharePointOnPremiseSettingDao>();
        private IRecordOwnerDao RecordOwnerDao => PlatformWindsorManager.GetService<IRecordOwnerDao>();


        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService<ITaxonomyService>();
        private IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IJobMonitorDao JobMonitorDao => PlatformWindsorManager.GetService<IJobMonitorDao>();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private IRMScopeRoleAssignmentDao RMScopeRoleAssignmentDao => PlatformWindsorManager.GetService<IRMScopeRoleAssignmentDao>();
        private IRMScheduleDao RMScheduleDao => PlatformWindsorManager.GetService<IRMScheduleDao>();
        private IRMSharePointOnPremBrowseService RMSharePointOnPremBrowseService => PlatformWindsorManager.GetService<IRMSharePointOnPremBrowseService>();
        //private ISharePointSettingDao SharePointSettingDao => PlatformWindsorManager.GetService<ISharePointSettingDao>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private ITermRuleAssociationDao TermRuleInfos => PlatformWindsorManager.GetService<ITermRuleAssociationDao>();
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();

        private IHybridSharePointOnPremWorkerService HybridSharePointWorkerService => PlatformWindsorManager.GetService<IHybridSharePointOnPremWorkerService>();
        public IRMNodeFlagDao RMNodeFlagDao => PlatformWindsorManager.GetService<IRMNodeFlagDao>();
        private IUniqueIdSettingDao UniqueIdSettingDao => PlatformWindsorManager.GetService<IUniqueIdSettingDao>();
        protected IRMChangeClassificationDao RMChangeClassificationDao => PlatformWindsorManager.GetService<IRMChangeClassificationDao>();
        private IRMSharePointOnPremBrowseService SharePointOnPremBrowseService => PlatformWindsorManager.GetService<IRMSharePointOnPremBrowseService>();
        private IRMSharePointSettingsService RMSPSettingsService => PlatformWindsorManager.GetService<IRMSharePointSettingsService>();
        private DB.Explorer.Dao.IExplorerDao explorerDao = new DB.Explorer.Dao.CosmosImp.ExplorerDao(true);
        private ISignalRService SignalRService => PlatformWindsorManager.GetService<ISignalRService>();
        private ISPSettingTreeService RMSPTreeService => PlatformWindsorManager.GetService<ISPSettingTreeService>();

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
        private IRMKeyValueDao  RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        #region apply setting job
        public RAReturnMessage ApplySettings(JobRunBy jobRunBy, bool fromTimerJobPage, RunApplySettingMethod runJobMethod)
        {
            Logger.Debug("start ApplySettings on premise.");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            if (runJobMethod == RunApplySettingMethod.UpdatedScope)
            {
                var updatedScopeCount = 0;
                var settings = SharePointOnPremiseSettingDao.LoadRunJobSetting();
                updatedScopeCount = settings.Count;
                msg.Extension = updatedScopeCount.ToString();
                if (updatedScopeCount == 0)
                {
                    //选择updated scope run job，如果settings count为0直接返回，不起job
                    msg.Extsion1 = I18NEntity.GetString("RM_JS_SPS_NoUpdatedScope");
                    return msg;
                }
				msg.Extsion1 = string.Format(I18NEntity.GetString("RM_JS_SPS_Msg_RunJobNodes"), updatedScopeCount);
				if (updatedScopeCount == 1)
                {
					msg.Extsion1 = string.Format(I18NEntity.GetString("RM_JS_SPS_Msg_RunJobSingleNode"), updatedScopeCount);
				}
            }

            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = jobRunBy == JobRunBy.Schedule ? "RM_TS_RunSchedule" : TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.SPOnPremApplySetting,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = string.Format("{0} {1}", fromTimerJobPage, Convert.ToInt32(runJobMethod))
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred while ApplySettings for SharePoint on premise,ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        public async Task<bool> NeedRunUniqueIdJobAsync(List<RMSPTreeNode> needRunNodes = null)
        {
            bool result = false;
            try
            {
                var needRunJobNodes = SharePointOnPremiseSettingDao.LoadShowUniqueIdSetting();
                foreach (var nodeInfo in needRunJobNodes)
                {
                    var setting = CloneSetting(nodeInfo);
                    if (setting.NodeInfo == null)
                    {
                        Logger.Info("no change, nodeinfo null.Id:{0}", setting.ScopeId);
                        continue;
                    }
                    var node = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(setting.NodeInfo);

                    if (node.Level == (int)NodeLevel.WebApplication)
                    {
                        var group = await SharePointOnPremClient.GetLocalSiteCollectionsByWebAppIdAsync(node.SPObjectId);
                        if (group == null)
                        {
                            Logger.Info($"can not find the group:{node?.FullPath}.");
                            continue;
                        }

                        Guid groupId = Guid.Empty;
                        Guid.TryParse(node.SPObjectId, out groupId);

                        if (!RMNodeFlagDao.IsNodeFlagExist(groupId, Guid.Empty, (int)NodeFlagType.UniqueId))
                        {
                            if (needRunNodes != null)
                            {
                                needRunNodes.Add(node);
                            }
                            else
                            {
                                needRunNodes = new List<RMSPTreeNode>();
                                needRunNodes.Add(node);
                            }
                            Logger.Info("need run unique id node:{0}", node.FullPath);
                            result = true;
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Error("error occurred while check unique id,ERROR:{0}", ex.ToString());
            }
            return result;
        }


        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.ApplySharePointSettingSPOnPrem, BeforeHandler = typeof(RMTermSyncBeforeAuditHandler), AfterHandler = typeof(RMTermSyncAfterAuditHandler))]
        public async Task<string> RealRunApplySettingJobAsync(JobRunBy jobRunBy, string jobRunByUser, bool fromTimerJobPage, RunApplySettingMethod runJobMethod)
        {
            string jobId = string.Empty;
            //起Job，判断是前台起Job还是Schedule起的Job
            List<string> runningJobs = RMJobService.GetRunningSharePointOnPremiseSettingJob();

            //bool isSkip = runningJobs.Any(j => j != jobId);
            try
            {
                if (runningJobs.Count == 0)
                {
                    jobId = await StartApplySettingJobAsync(jobRunBy, jobRunByUser, JobType.SPOnPremApplySetting, runJobMethod);
                }
                else
                {
                    //TO DO for skipped jobs, how to set container id?
                    var settings = GetSPSettings(jobRunBy, runJobMethod);
                    if (settings.IsNullOrEmpty())
                    {
                        Logger.Warn("No sharepoint on premise setting node found.");
                        throw new Exception("No sharepoint setting node found.");
                    }
                    bool hasAvailableNode = false;
                    foreach (var setting in settings)
                    {
                        RMSPTreeNode node = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(setting.NodeInfo);
                        if (node == null)
                        {
                            Logger.Warn("Node info in {0} is null or empty", setting.FullPath);
                            continue;
                        }
                        var containerId = GetSPContainerId(node);
                        var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
                        if (!IsSPAdmin(account.UserId))
                        {
                            List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(account.UserId);
                            if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupUserIds))
                            {
                                Logger.Info($"current user doesn't have permission on container. Container Id:{containerId}");
                                continue;
                            }
                        }
                        var fullPah = node.Level == (int)NodeLevel.WebApplication ? node.Name  : GetSPContainerName(node);
                        jobId = CreateApplySettingJob(jobRunBy, jobRunByUser, containerId, null, fullPah);
                        RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SS_JobSkip");
                        Logger.Info(I18NEntity.GetString("RM_SS_JobSkip"));
                        hasAvailableNode = true;
                        break;
                    }
                    if (!hasAvailableNode)
                    {
                        jobId = CreateApplySettingJob(jobRunBy, jobRunByUser);
                        RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SP_NoAvailableNodeError");
                        Logger.Warn($"Has no available node for current user. JobId:{jobId}");
                    }
                }
            }
            catch (Exception e)
            {
                if (string.IsNullOrWhiteSpace(jobId))
                {
                    jobId = CreateApplySettingJob(jobRunBy, jobRunByUser);
                }
                if (e.Message == I18NEntity.GetString("RM_SP_NoAvailableSettingError"))
                {
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SP_NoAvailableSettingError");
                }
                else
                {
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SP_CreateJobError");
                }

                Logger.Error("real run apply sp on premise setting job error: {0}", e.ToString());
            }

            return jobId;
        }

        public string RunSharepointSettingsScheduleJob(JobRunBy jobRunBy)
        {
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.SPOnPremApplySettingSchedule,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred while RunSharepointSettingsScheduleJob,ERROR:{0}", ex.ToString());
            }

            return id;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.ApplySharePointSettingSPOnPrem, BeforeHandler = typeof(RMTermSyncBeforeAuditHandler), AfterHandler = typeof(RMTermSyncAfterAuditHandler))]
        public async Task<string> RealSharepointSettingsScheduleJobAsync(JobRunBy jobRunBy, string jobRunByUser, bool fromTimerJobPage)
        {
            string jobId = string.Empty;

            #region old logic
            //获取节点上正在运行的job 如果有其他运行的job job Skip            
            #endregion
            List<string> runningJobs = RMJobService.GetRunningSharePointOnPremiseSettingJob();
            if (runningJobs.Count == 0)
            {
                //StartSettingsJob(JobType.SharePointScheduleSetting, jobId, jobRunBy);
                jobId = await StartApplySettingJobAsync(jobRunBy, jobRunByUser, JobType.SPOnPremApplySettingSchedule, RunApplySettingMethod.Auto);
            }
            else
            {
                jobId = RMJobService.CreateJob(Contract.JobMonitor.JobType.SPOnPremApplySettingSchedule, string.IsNullOrEmpty(jobRunByUser) ? "RM_TS_RunSchedule" : jobRunByUser);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SS_JobSkip");
                //StartSettingsJob(JobType.SharePointScheduleSetting, jobId);
                Logger.Info("CustomSetting job or GlobalSetting job or InheritSetting job has job running,so shedule job is skip");
            }

            return jobId;
        }

        public string GetApplySettingJobMessage(string jobId)
        {
            string message = string.Empty;
            try
            {
                Logger.Debug("Start to get apply setting job message. Job Id:" + jobId);
                var subJob = SubJobDao.GetSubJob(jobId, true);
                if (subJob.JobType == (int)JobType.SPOnPremApplySetting || subJob.JobType == (int)JobType.SPOnPremApplySettingSchedule)
                {

                    var nodes = SerializerHelper.DeserializeByDataContractSerializer<List<RMSPTreeNode>>(subJob.JobContext.Settings);
                    var groupMappings = SerializerHelper.DeserializeByDataContractSerializer<Dictionary<Guid, RMSharePointOnPremiseSetting>>(subJob.JobContext.Content);
                    var allSettings = SharePointOnPremiseSettingDao.LoadSharePointSettings(GetGroupId(nodes[0]));
                    ApplySettingJobMessage jobMessage = new ApplySettingJobMessage();
                    jobMessage.TreeNodes = AssembleTreeNodes(nodes);
                    jobMessage.GroupSettingMapping = AssembleGroupSettingMapping(groupMappings);
                    jobMessage.AllSettings = AssembleSPSettings(allSettings);
                    message = SerializerHelper.SerializeByDataContractSerializer(jobMessage);
                }
                else
                {
                    Logger.Warn("Invalid job type, type:" + subJob.JobType);
                }
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred while getting apply setting job message, error:{0}", e.ToString());
            }
            return message;
        }

        private Guid GetGroupId(RMSPTreeNode node)
        {
            if (node.Level != (int)NodeLevel.WebApplication)
            {
                return GetGroupId(node.Parent);
            }
            else
            {
                return new Guid(node.Id);
            }
        }

        private List<Global.RMSPTreeNode> AssembleTreeNodes(List<RMSPTreeNode> nodes)
        {
            List<Global.RMSPTreeNode> treeNodes = nodes.ConvertAll(n => RMDtoConverter.ConvertRMSPTreeNode2GlobalDto(n));
            return treeNodes;
        }

        private List<Global.RMSharePointOnPremiseSetting> AssembleSPSettings(List<RMSharePointOnPremiseSetting> spSettings)
        {
            var globalSettings = spSettings.ConvertAll(s => ConvertRMSharePointSetting2GlobalDto(s));
            return globalSettings;
        }

        public static Global.RMSharePointOnPremiseSetting ConvertRMSharePointSetting2GlobalDto(RMSharePointOnPremiseSetting spSetting)
        {
            Global.RMSharePointOnPremiseSetting setting = new Global.RMSharePointOnPremiseSetting()
            {
                ApplyExistType = spSetting.ApplyExistType,
                AutoJobOption = spSetting.AutoJobOption,
                ColumnName = spSetting.ColumnName,
                ColumnRequired = spSetting.ColumnRequired,
                DefaultTermId = spSetting.DefaultTermId,
                DefaultTermName = spSetting.DefaultTermName,
                DeployTermMethod = spSetting.DeployTermMethod,
                Description = spSetting.Description,
                DescriptionOfContainer = spSetting.DescriptionOfContainer,
                EMailToRecordOwner = spSetting.EMailToRecordOwner,
                EnableRecordManagement = spSetting.EnableRecordManagement,
                EnableRelatedRecords = spSetting.EnableRelatedRecords,
                ExistColumnName = spSetting.ExistColumnName,
                // FieldId = spSetting.f,
                FolderId = spSetting.FolderId,
                FullPath = spSetting.FullPath,
                //HaveConfigSetting = spSetting.HaveConfigSetting,
                Id = spSetting.Id,
                IncludeDeclaredRecords = spSetting.IncludeDeclaredRecords,
                IsDisplyaTermPath = spSetting.IsDisplyaTermPath,
                isEnableClassification = spSetting.IsEnableContainerLevelTerm,
                isFailedConfigClassification = spSetting.IsFailedConfigClassification,
                isFailedConfigMetaDataColumn = spSetting.IsFailedConfigMetaDataColumn,
                IsSyncData = spSetting.IsSyncData,
                IsUsingExistColumnName = spSetting.IsUsingExistColumnName,
                IsShowUniqueId = spSetting.IsShowUniqueId,
                ListId = spSetting.ListId,
                NeedCheckDefaultValue = spSetting.NeedCheckDefaultValue,
                ScopeId = spSetting.ScopeId,
                RunAutoFullJob = spSetting.RunAutoFullJob,
                SetDocLevelTermForExistColumn = spSetting.SetDocLevelTermForExistColumn,
                SiteGroupId = spSetting.SiteGroupId,
                SiteId = spSetting.SiteId,
                TermId = spSetting.TermId,
                TermName = spSetting.TermName,
                TermIdOfContainer = spSetting.TermIdOfContainer,
                TermNameOfContainer = spSetting.TermNameOfContainer,
                TermSetId = spSetting.TermSetId,
                TermSetName = spSetting.TermSetName,
                TermStoreId = spSetting.TermStoreId,
                WebId = spSetting.WebId,
                //NodeInfo = spSetting.NodeInfo
                //IsRunning = spSetting.IsRunning
            };
            if (!string.IsNullOrWhiteSpace(spSetting.NodeInfo))
            {
                var nodeInfo = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(spSetting.NodeInfo);
                var localNodeInfo = RMDtoConverter.ConvertRMSPTreeNode2GlobalDto(nodeInfo);
                setting.NodeInfo = SerializerHelper.SerializeByDataContractSerializer(localNodeInfo);
            }
            if (spSetting.AutoClassificationRules != null)
            {
                var oldAutoRules = SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(spSetting.AutoClassificationRules);
                setting.AutoClassificationRules = SerializerHelper.SerializeByDataContractSerializer(oldAutoRules.ConvertAll(a => RMDtoConverter.ConvertClassificationRule2GlobalDto(a)));
            }
            return setting;
        }

        private Dictionary<Guid, Global.RMSharePointOnPremiseSetting> AssembleGroupSettingMapping(Dictionary<Guid, RMSharePointOnPremiseSetting> mappings)
        {
            Dictionary<Guid, Global.RMSharePointOnPremiseSetting> newMapping = new Dictionary<Guid, Global.RMSharePointOnPremiseSetting>();
            foreach (var mapping in mappings)
            {
                newMapping.Add(mapping.Key, ConvertRMSharePointSetting2GlobalDto(mapping.Value));
            }
            return newMapping;
        }

        private string CreateApplySettingJob(JobRunBy runBy, string jobRunByUser, string containerId = null, string scopedId = null, string fullPath = null)
        {
            string jobId = string.Empty;
            if (runBy == JobRunBy.Control)
            {
                jobId = RMJobService.CreateJob(JobType.SPOnPremApplySetting, jobRunByUser, containerId, scopedId, fullPath);
                Logger.Info("Begin control Apply Job {0}", jobId);
            }
            else if (runBy == JobRunBy.Schedule)
            {
                jobId = RMJobService.CreateJob(JobType.SPOnPremApplySetting, "RM_TS_RunSchedule", containerId, scopedId, fullPath);
                Logger.Info("Begin schedule Apply Job {0}", jobId);
            }
            else
            {
                jobId = RMJobService.CreateJob(JobType.SPOnPremApplySetting, jobRunByUser, containerId, scopedId, fullPath);
                Logger.Info("Begin default Sync Job {0}", jobId);
            }
            return jobId;
        }

        private bool IsSPAdmin(string userId)
        {
            //return UserService.DoesUserHasThisPermission(TenantLocalValue.LogonGroupId, userId, RMPermissionMasks.SPOAdmin);
            return true;
        }

        private string GetSPContainerId(RMSPTreeNode selectedNode)
        {
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                return selectedNode.Id;
            }
            else
            {
                return GetSPContainerId(selectedNode.Parent);
            }
        }

        private string GetSPContainerName(RMSPTreeNode selectedNode)
        {
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                return selectedNode.Name;
            }
            else
            {
                return GetSPContainerName(selectedNode.Parent);
            }
        }
        private string GetSPContainerFullName(RMSPTreeNode selectedNode)
        {
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                return selectedNode.FullPath;
            }
            else
            {
                return GetSPContainerFullName(selectedNode.Parent);
            }
        }
        private List<RMSharePointOnPremiseSetting> GetSPSettings(JobRunBy runBy, RunApplySettingMethod runJobMethod)
        {
            List<RMSharePointOnPremiseSetting> allSettings = null;
            if (runBy == JobRunBy.Control)
            {
                switch (runJobMethod)
                {
                    case RunApplySettingMethod.UpdatedScope:
                        allSettings = SharePointOnPremiseSettingDao.LoadRunJobSetting();
                        break;
                    case RunApplySettingMethod.AllScope:
                        Logger.Info("apply full sharepoint setting job");
                        allSettings = SharePointOnPremiseSettingDao.LoadAllSetting();
                        break;
                    case RunApplySettingMethod.Auto:
                        //Part job by node.
                        allSettings = SharePointOnPremiseSettingDao.LoadRunJobSetting();
                        if (allSettings.Count == 0)
                        {
                            Logger.Info("apply full sharepoint setting job");
                            allSettings = SharePointOnPremiseSettingDao.LoadAllSetting();
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                //Full job
                allSettings = SharePointOnPremiseSettingDao.LoadAllSetting();
            }
            return allSettings;
        }

        private async Task<string> StartApplySettingJobAsync(JobRunBy runBy, string jobRunByUser, JobType jobType, RunApplySettingMethod runJobMethod)
        {
            //Get settings jobs
            //browser tree start sub job..
            //Create sub job detail..
            List<RMSharePointOnPremiseSetting> allSettings = GetSPSettings(runBy, runJobMethod);
            string jobId = string.Empty;

            if (allSettings.IsNullOrEmpty())
            {
                Logger.Warn("No sharepoint setting node found.");
                throw new Exception(I18NEntity.GetString("RM_SP_NoAvailableSettingError"));
            }
            Dictionary<Guid, RMSharePointOnPremiseSetting> gruopSetingMap = new Dictionary<Guid, RMSharePointOnPremiseSetting>();
            Dictionary<Guid, int> nodeSettingMap = new Dictionary<Guid, int>();
            var excludeSiteNodes = SharePointOnPremiseSettingDao.LoadExcludeSiteCollectionSetting();
            Dictionary<Guid, int> applyExistScopes = new Dictionary<Guid, int>();
            List<Guid> ExcludeSiteIds = excludeSiteNodes.Select(t => t.ScopeId).ToList();
            //List<SPTreeNodeDto> subJobNodes = new List<SPTreeNodeDto>();
            List<RMSPTreeNode> availableSites = new List<RMSPTreeNode>();
            Dictionary<Guid, List<RMSPTreeNode>> settingGroup = new Dictionary<Guid, List<RMSPTreeNode>>();
            var account = await AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail);
            Dictionary<string, string> emptyContainers = new Dictionary<string, string>();
            foreach (RMSharePointOnPremiseSetting setting in allSettings)
            {
                if (AvePoint.RA.RACommonUtility.SharePointOnPrem.SharePointOnPremClient.GetLocalWebApplicationById(setting.SiteGroupId.ToString()) == null)
                {
                    Logger.Warn($"Can't find the group: [{setting.SiteGroupId}] in database.");
                    continue;
                }
                if (setting.SiteId != Guid.Empty && AvePoint.RA.RACommonUtility.SharePointOnPrem.SharePointOnPremClient.GetLocalSiteCollectionById(setting.SiteId.ToString()) == null)
                {
                    Logger.Warn($"Can't find the node: [{setting.FullPath}] in database.");
                    continue;
                }
                RMSPTreeNode node = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(setting.NodeInfo);
                if (node == null)
                {
                    Logger.Warn("Node info in {0} is null or empty", setting.FullPath);
                    continue;
                }
                //will use common method later
                var containerId = GetSPContainerId(node);
                var isAdmin = IsSPAdmin(account.UserId);
                if (!isAdmin)
                {
                    List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(account.UserId);
                    if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupUserIds))
                    {
                        Logger.Info($"current user doesn't have permission on container. Container Id:{containerId}");
                        continue;
                    }
                }
                List<RMSPTreeNode> nodes = new List<RMSPTreeNode>();
                if (node.Level == (int)NodeLevel.WebApplication)
                {
                    List<RMSPTreeNode> sites = (await RMSharePointOnPremBrowseService.BrowseAsync(RMDtoConverter.ConvertRMTree2SPTree(node))).ConvertAll(n => RMDtoConverter.ConvertSPTree2RMTree(n));
                    var totalSiteCount = sites.Count;
                    var hasCustomSiteCount = 0;

                    Logger.Info("Group:{0} site collection count is {1}", node.Name, sites.Count);
                    if (sites.Count > 0)
                    {
                        foreach (RMSPTreeNode siteNode in sites)
                        {
                            if (ExcludeSiteIds.Contains(new Guid(siteNode.SPObjectId)))
                            {
                                Logger.Info("Exclude SiteId {0}", siteNode.SPObjectId);
                                hasCustomSiteCount++;
                            }
                            else
                            {
                                nodes.Add(siteNode);
                            }
                            if (!gruopSetingMap.ContainsKey(new Guid(node.Id)))
                            {
                                gruopSetingMap.Add(new Guid(node.Id), setting);
                            }
                        }
                    }
                    else
                    {
                        if (!emptyContainers.ContainsKey(containerId))
                        {
                            emptyContainers.Add(containerId, GetSPContainerName(node));
                        }
                    }
                    if (totalSiteCount > 0 && totalSiteCount == hasCustomSiteCount)
                    {
                        //update group node setting
                        await SharePointOnPremiseSettingDao.SetSettingJobTimeAsync(new Guid(node.Id), false, false);
                    }
                }
                else
                {
                    nodes.Add(node);
                }
                if (nodes.Count > 0)
                {
                    if (settingGroup.ContainsKey(setting.SiteGroupId))
                    {
                        settingGroup[setting.SiteGroupId].AddRange(nodes);
                    }
                    else
                    {
                        settingGroup.Add(setting.SiteGroupId, nodes);
                    }
                }
            }
            if (settingGroup.Count > 0)
            {
                //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
                int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
                foreach (var group in settingGroup)
                {
                    jobId = CreateApplySettingJob(runBy, jobRunByUser, group.Key.ToString());
                    var parallelSubJobCount = subJobCountInConfigFile * await HybridSharePointWorkerService.GetAgentCountAsync(group.Value[0].FarmId);
                    if (parallelSubJobCount == 0)
                    {
                        Logger.Error("No available agent server. Set main job failed.");
                        RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_FSNoAvailableAgent");
                        continue;
                    }
                    SeperateSubJobForApplySetting(group.Value, gruopSetingMap, jobId, runBy, jobType, parallelSubJobCount);
                }
            }
            else
            {
                if (emptyContainers.Count > 0)
                {
                    foreach (var container in emptyContainers)
                    {
                        jobId = CreateApplySettingJob(runBy, jobRunByUser, container.Key);
                        RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, $"RM_SP_NoSiteCollectionUnderGroup{I18NEntity.Separator}{container.Value}");
                    }
                }
                else
                {
                    Logger.Warn("No sharepoint setting node group found.");
                    throw new Exception(I18NEntity.GetString("RM_SP_NoAvailableSettingError"));
                }
            }

            return jobId;
        }

        private int GetNodeCountInSubJob(int totalCount)
        {
            if (totalCount <= 100)
            {
                return 5;
            }
            else if (100 < totalCount && totalCount <= 200)
            {
                return 10;
            }
            else if (200 < totalCount && totalCount <= 500)
            {
                return 20;
            }
            else if (500 < totalCount && totalCount <= 1000)
            {
                return 50;
            }
            else
            {
                return 100;
            }
        }
        private void SeperateSubJobForApplySetting(List<RMSPTreeNode> availableSites, Dictionary<Guid, RMSharePointOnPremiseSetting> gruopSetingMap, string jobId, JobRunBy runBy, JobType jobType, int parallelSubJobCount)
        {
            List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();
            Dictionary<string, List<RMSPTreeNode>> folderLevelSubJobDic = GroupFolderLevelNodeForSubJob(availableSites);
            Dictionary<string, List<RMSPTreeNode>> aboveListLevelSubJobDic = GroupAboveListLevelNodeForSubJob(availableSites);
            Dictionary<int, List<RMSPTreeNode>> subJobNodeDic = new Dictionary<int, List<RMSPTreeNode>>();
            int count = 0;
            foreach (KeyValuePair<string, List<RMSPTreeNode>> pa in aboveListLevelSubJobDic)
            {
                tempList.AddRange(pa.Value);
                if (tempList.Count >= GetNodeCountInSubJob(pa.Value.Count))
                {
                    count++;
                    var temp = new List<RMSPTreeNode>();
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
            foreach (var folderGroup in folderLevelSubJobDic)
            {
                count++;
                subJobNodeDic.Add(count, folderGroup.Value);
                //var foldeNodes = folderGroup.Value;
                //for (int i = 0; i < foldeNodes.Count; i += 50)
                //{
                //    var tempNode = foldeNodes.Skip(i).Take(50).ToList();
                //    count++;
                //    subJobNodeDic.Add(count, tempNode);
                //}
            }
            SubJobDao.UpdateSubJobCount(jobId, count);
            Logger.Info("Sub job count for {0} is {1}", jobId, count);
            //int subJobCount = availableSites.Count % RMGlobalConfiguration.AppConfig.NODE_COUNT_IN_SUB_JOB == 0 ? availableSites.Count / RMGlobalConfiguration.AppConfig.NODE_COUNT_IN_SUB_JOB : availableSites.Count / RMGlobalConfiguration.AppConfig.NODE_COUNT_IN_SUB_JOB + 1;
            //SubJobDao.UpdateSubJobCount(jobId, subJobCount);

            int currentSubjobIndex = 0;
            foreach (KeyValuePair<int, List<RMSPTreeNode>> pa in subJobNodeDic)
            {
                string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, count, pa.Value, currentSubjobIndex < parallelSubJobCount, gruopSetingMap);
                Logger.Debug("Create and queue sub job {0}", subJobId);
                if (currentSubjobIndex < parallelSubJobCount)
                {
                    HybridSharePointWorkerService.StartSPJob(new Hybrid.Contract.RecordsJobArgs()
                    {
                        JobId = subJobId,
                        JobType = AvePoint.Hybrid.Contract.JobType.SharePointOnPremApplySetting,
                        TenantId = TenantLocalValue.LogonGroupId,
                        FarmId = pa.Value[0].FarmId
                    });
                }
                currentSubjobIndex++;
            }
        }

        private Dictionary<string, List<RMSPTreeNode>> GroupFolderLevelNodeForSubJob(List<RMSPTreeNode> treeNodes)
        {
            Dictionary<string, List<RMSPTreeNode>> result = new Dictionary<string, List<RMSPTreeNode>>();
            foreach (RMSPTreeNode node in treeNodes)
            {
                if (node.Level > (int)NodeLevel.List)
                {
                    string listFullPath = this.GetParentListFullPath(node);
                    if (!result.ContainsKey(listFullPath))
                    {
                        result.Add(listFullPath, new List<RMSPTreeNode>());
                    }
                    result[listFullPath].Add(node);
                }
            }
            return result;
        }

        private Dictionary<string, List<RMSPTreeNode>> GroupAboveListLevelNodeForSubJob(List<RMSPTreeNode> treeNodes)
        {
            Dictionary<string, List<RMSPTreeNode>> result = new Dictionary<string, List<RMSPTreeNode>>();
            foreach (RMSPTreeNode node in treeNodes)
            {
                if (node.Level <= (int)NodeLevel.List)
                {
                    if (!result.ContainsKey(node.FullPath))
                    {
                        result.Add(node.FullPath, new List<RMSPTreeNode>());
                    }
                    result[node.FullPath].Add(node);
                }
            }
            return result;
        }

       
        private string GetParentListFullPath(RMSPTreeNode node)
        {
            try
            {
                if (node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Library)
                {
                    return node.FullPath;
                }
                else
                {
                    return GetParentListFullPath(node.Parent);
                }
            }
            catch (Exception e)
            {
                Logger.Debug(e.Message, e);
                return Guid.NewGuid().ToString();
            }
        }

        private string CreateSubJob(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<RMSPTreeNode> tempList, bool sendNow, Dictionary<Guid, RMSharePointOnPremiseSetting> gruopSetingMap = null)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            string farmId = tempList[0].FarmId;
            var subJob = new RMSubJob() { Id = subJobId, FarmId = farmId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(tempList) };
            if (gruopSetingMap != null)
            {
                subJob.JobContext.Content = SerializerHelper.SerializeByDataContractSerializer(gruopSetingMap);
            }
            SubJobDao.CreateJob(subJob);
            Logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} ", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }
        #endregion

        #region data sync job
        public async Task<RAReturnMessage> RunDataSyncJobAsync(RMSPTreeNode selectedTree, JobRunBy jobRunBy)
        {
            Logger.Debug("start sp on premise data sync");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();


            //selectedTree is null start by Timer Page run now;
            //selectedTree is not null start by Content Repository Management;
            if (selectedTree != null)
            {
                if (!await IsExistCanRunJobNodesAsync(selectedTree))
                {
                    msg.MessageType = RAMessageType.Failed;
                    //此处的提示信息与EXO使用同一个
                    msg.ErrorMessage = I18NEntity.GetString("RM_JM_EXO_SyncData_NoSC");
                    return msg;
                }
            }

            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.SPOnPremDataSync,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = selectedTree == null ? null : SerializerHelper.SerializeByDataContractSerializer(selectedTree)
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred while ApplySettings,ERROR:{0}", ex.ToString());
            }

            return msg;
        }
        /// <summary>
        /// 验证:是否存在可以运行Job的节点
        /// </summary>
        /// <param name="selectedTree"></param>
        /// <returns></returns>
        private async Task<bool> IsExistCanRunJobNodesAsync(RMSPTreeNode selectedTree)
        {
            if (selectedTree != null)
            {
                if (IsEnableRecordManagement(selectedTree) && await IsHaveAvailableNodesAsync(selectedTree))
                {
                    return true;
                }
            }
            return false;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunCollectionJob4SPOnPrem, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public Task<string> RealRunDataSyncJobAsync(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.SPOnPremDataSync;
            if (string.IsNullOrEmpty(param))
            {
                return RunSPDataSyncJobAllSettingNodeAsync(jobRunBy, jobRunByUser, jobType);
            }
            else
            {
                RMSPTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(param);
                return RunDataSyncJobBySelectdNodeAsync(jobRunByUser, jobType, selectedNode);
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.RunCollectionJob4SPOnPrem, BeforeHandler = typeof(RMTermSyncBeforeAuditHandler), AfterHandler = typeof(RMTermSyncAfterAuditHandler))]
        public async Task<string> RealRunSPDataSyncScheduleJobAsync(JobRunBy jobRunBy, string jobRunByUser = null)
        {
            JobType jobType = jobRunBy == JobRunBy.Control ? JobType.SPOnPremDataSync : JobType.SPOnPremDataSyncSchedule;
            jobRunByUser = GetJobRunByUser(jobRunBy, jobRunByUser);
            //Skip if a schedule job is running
            List<string> runningJobIds = RMJobService.GetRunningJobs(JobType.SPOnPremDataSyncSchedule);
            if (!runningJobIds.IsNullOrEmpty())
            {
                Logger.Info("Current running scheduled on premise data sync job:{0}", string.Join(", ", runningJobIds.ToArray()));

                string jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "Skipped this job. A SharePoint On Premise Data Synchronization job is already running.");
                return jobId;
            }
            else
            {
                return await RunSPDataSyncJobAllSettingNodeAsync(jobRunBy, jobRunByUser, jobType);
            }
        }

        private static string GetJobRunByUser(JobRunBy jobRunBy, string jobRunByUser)
        {
            if (jobRunBy == JobRunBy.Control)
            {
                jobRunByUser = string.IsNullOrEmpty(jobRunByUser) ? TenantLocalValue.LogonUserEmail : jobRunByUser;
            }
            else
            {
                jobRunByUser = "RM_TS_RunSchedule";
            }

            return jobRunByUser;
        }

        private async Task<string> RunSPDataSyncJobAllSettingNodeAsync(JobRunBy jobRunBy, string jobRunByUser, JobType jobType)
        {
            //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            string jobId = string.Empty;
            jobId = RMJobService.CreateJob(jobType, jobRunByUser);
            List<RMSPTreeNode> availableNode = new List<RMSPTreeNode>();
            var allSetting = SharePointOnPremiseSettingDao.LoadAllSetting().Where(s => s.IsSyncData);

            if (allSetting.IsNullOrEmpty())
            {
                Logger.Warn("There is no site collection setting enable sync data into Explorer.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_NoIsSyncSCUnderGroup");
                return jobId;
            }

            try
            {
                foreach (var setting in allSetting)
                {
                    RMSPTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(setting.NodeInfo);
                    if (selectedNode.Level == (int)NodeLevel.WebApplication || selectedNode.Level == (int)NodeLevel.SiteCollection)
                    {
                        var tempNodes = await this.AssembleSyncDataRunnableNodeAsync(selectedNode);
                        foreach (var node in tempNodes)
                        {
                            if (!availableNode.Select(n => n.Id).ToList().Contains(node.Id))
                            {
                                availableNode.Add(node);
                            }
                        }
                    }
                }
                if (availableNode.IsNullOrEmpty())
                {
                    Logger.Warn("No available sc to run");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_NoSCUnderGroupBySchedule");
                    return jobId;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error occurred while get available node for data sync job. ERROR:{0}", ex.ToString());
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, ex.Message);
                return jobId;
            }
            
            Dictionary<string, List<RMSPTreeNode>> farmNodeGroup = new Dictionary<string, List<RMSPTreeNode>>();
            foreach (var node in availableNode)
            {
                if (string.IsNullOrWhiteSpace(node.FarmId))
                {
                    Logger.Warn("Node farm id is null, node id:{0}", node.Id);
                    continue;
                }

                if (farmNodeGroup.ContainsKey(node.FarmId))
                {
                    farmNodeGroup[node.FarmId].Add(node);
                }
                else
                {
                    farmNodeGroup.Add(node.FarmId, new List<RMSPTreeNode>() { node });
                }
            }

            int totalSubJobCount = 0;
            foreach (var nodes in farmNodeGroup.Values)
            {
                int tempSubJobCount = nodes.Count % RMGlobalConfiguration.AppConfig.NodeCountInSubJob == 0 ? nodes.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob : nodes.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob + 1;
                totalSubJobCount += tempSubJobCount;
            }
            //int subJobCount = availableNode.Count % RMGlobalConfiguration.AppConfig.NODE_COUNT_IN_SUB_JOB == 0 ? availableNode.Count / RMGlobalConfiguration.AppConfig.NODE_COUNT_IN_SUB_JOB : availableNode.Count / RMGlobalConfiguration.AppConfig.NODE_COUNT_IN_SUB_JOB + 1;
            SubJobDao.UpdateSubJobCount(jobId, totalSubJobCount);
            Logger.Debug("Sub job count:{0}", totalSubJobCount);
            jobType = JobType.SPOnPremDataSync;
            int currentSubjobIndex = 0;
            List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();
            foreach (var group in farmNodeGroup)
            {
                foreach (var site in group.Value)
                {
                    tempList.Add(site);
                    if (tempList.Count == RMGlobalConfiguration.AppConfig.NodeCountInSubJob)
                    {
                        string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, totalSubJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile);
                        Logger.Debug("Create and queue sub job {0}", subJobId);
                        if (currentSubjobIndex < subJobCountInConfigFile)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                        {
                            HybridSharePointWorkerService.StartSPJob(new Hybrid.Contract.RecordsJobArgs()
                            {
                                JobId = subJobId,
                                JobType = AvePoint.Hybrid.Contract.JobType.SharePointOnPremDataSync,
                                TenantId = TenantLocalValue.LogonGroupId,
                                FarmId = tempList[0].FarmId
                            });
                        }
                        tempList.Clear();
                        currentSubjobIndex++;
                    }
                }
                if (tempList.Count > 0)
                {
                    string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, totalSubJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile);
                    Logger.Debug("Create and queue sub job {0}", subJobId);
                    if (currentSubjobIndex < subJobCountInConfigFile) //一次只发两个子job, 后续在JobInfoUpdater中触发
                    {
                        HybridSharePointWorkerService.StartSPJob(new Hybrid.Contract.RecordsJobArgs()
                        {
                            JobId = subJobId,
                            JobType = AvePoint.Hybrid.Contract.JobType.SharePointOnPremDataSync,
                            TenantId = TenantLocalValue.LogonGroupId,
                            FarmId = tempList[0].FarmId
                        });
                    }
                    tempList.Clear();
                    currentSubjobIndex++;
                }
            }
            return jobId;
        }

        public string GetDataSyncJobMessage(string jobId)
        {
            string message = string.Empty;
            try
            {
                Logger.Debug("Start to get data sync job message. Job Id:" + jobId);
                var subJob = SubJobDao.GetSubJob(jobId, true);
                if (subJob.JobType == (int)JobType.SPOnPremDataSync || subJob.JobType == (int)JobType.SPOnPremDataSyncSchedule)
                {
                    var mainJob = JobMonitorDao.GetJob(subJob.ParentId);
                    var nodes = SerializerHelper.DeserializeByDataContractSerializer<List<RMSPTreeNode>>(subJob.JobContext.Settings);
                    DataSyncJobMessage jobMessage = new DataSyncJobMessage();
                    jobMessage.TreeNodes = AssembleTreeNodes(nodes);
                    jobMessage.MainJobStartTime = mainJob.StartTime;
                    jobMessage.Rules = AssembleRules().ToDictionary(r => new Guid(r.Id));
                    jobMessage.Terms = AssembleTermInfos().ToDictionary(t => t.UniqueId);
                    jobMessage.TermAndRulesMapping = AssembleTermRuleMapping();
                    jobMessage.SiteInformationDic = AssembleSiteInformationDic(nodes);
                    jobMessage.ArchiverSetting = AssembleArchiverSetting();
                    bool isCosmosBulkOperationEnabled = RMKeyValueDao.IsCosmosBulkOperationEnabled();
                    if (isCosmosBulkOperationEnabled)
                    {
                        jobMessage.BulkImportEnabled = true;
                        var bulkSize = RMKeyValueDao.GetCosmosBulkInsertOperationBufferSize();
                        if (bulkSize == default(int))
                        {
                            bulkSize = DB.Explorer.Bulk.CosmosBulkOperator.DefualtBufferSize;
                        }
                        Logger.Info($"Cosmos bulk operation enabled, bulk size: {bulkSize}");
                        jobMessage.BulkSize = bulkSize;
                    }
                    message = SerializerHelper.SerializeByDataContractSerializer(jobMessage);
                }
                else
                {
                    Logger.Warn("Invalid job type, type:" + subJob.JobType);
                }
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred while getting data sync job message, error:{0}", e.ToString());
            }
            return message;
        }

        public RAReturnMessage RunSPDataSyncScheduleJob(JobRunBy jobRunBy)
        {
            Logger.Debug("start all data sync");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : I18NEntity.GetString("RM_TS_RunSchedule");
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.SPOnPremDataSyncSchedule,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred while SP DataSync,ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        private AvePoint.RA.Contract.Global.Object.SOArchiverSettings AssembleArchiverSetting()
        {
            return new AvePoint.RA.Contract.Global.Object.SOArchiverSettings()
            {
                IsDeleteRecord = false,
                IsDeleteLinkFile = false,
                SkipFileExtensions = new string[] { ".aspx", ".js", ".css" }
            };
        }

        private Dictionary<string, SiteInfo> AssembleSiteInformationDic(List<RMSPTreeNode> nodes)
        {
            Dictionary<string, SiteInfo> siteInfos = new Dictionary<string, SiteInfo>();
            foreach (var node in nodes)
            {
                var siteUrl = node.FullPath;
                var lastScanTime = RMNodeFlagDao.GetCollectionTime((int)NodeFlagType.ExplorerSync, new Guid(node.Parent.SPObjectId), new Guid(node.SPObjectId));
                var groupLevelSetting = SharePointOnPremiseSettingDao.GetGroupLevelSetting(node.Parent.FullPath, new Guid(node.Parent.SPObjectId));
                var columnName = groupLevelSetting.IsUsingExistColumnName ? groupLevelSetting.ExistColumnName : groupLevelSetting.ColumnName;
                var allTerms = RMChangeClassificationDao.GetAllChange(lastScanTime, (int)Contract.Object.TermChangeType.TermRule);
                List<Guid> subTerms = new List<Guid>();
                foreach (var id in allTerms)
                {
                    subTerms.AddRange(TermDao.GetAllSubTermUniqueIds(id));
                }
                allTerms.AddRange(subTerms);
                siteInfos.Add(siteUrl, new SiteInfo()
                {
                    LastScanTime = lastScanTime,
                    BCSColumnName = columnName,
                    ChangedTermIds = allTerms
                });
            }
            return siteInfos;
        }

        private List<AvePoint.RA.Contract.Global.Object.Rule> AssembleRules()
        {
            var fsRules = AgentRuleUtil.FilterRuleWithDataSource(RuleManagerService.GetRulesFromRecords(), Contract.Explorer.SourceFlag.SharePointOnPrem);
            var globalRules = fsRules.ConvertAll(r => RMDtoConverter.ConvertRule2GlobalDto(r));
            return globalRules;
        }

        private Dictionary<Guid, Contract.Global.Object.RMRuleItemCollection> AssembleTermRuleMapping()
        {
            var spRules = AgentRuleUtil.FilterRuleWithDataSource(RuleManagerService.GetRulesFromRecords(), Contract.Explorer.SourceFlag.SharePointOnPrem);
            if (spRules.Count == 0)
            {
                return new Dictionary<Guid, Global.RMRuleItemCollection>();
                //throw new Exception("No available rules");
            }
            //转换所有的Rule为Global Rule
            var globalRules = spRules.ConvertAll(r => RMDtoConverter.ConvertRule2GlobalDto(r));
            //获取所有Onpremise Rule
            var globalSPLocalRules = globalRules.Where(r => r.SPLocalRule != null);
            //转换Rule.SPLocalRule为Rule，AgentCheck用的是Rule本身，而不是SPLocalRule
            var globalSORules = globalSPLocalRules.ToList().ConvertAll(r => RMDtoConverter.ConvertGlbalSPLocalRule2GlobalRule(r));

            var commonLocalSPRules = spRules.Where(r => r.SPLocalRule != null).ToList();
            var commonSORules = commonLocalSPRules.ConvertAll(r => RMDtoConverter.ConvertGCommonSPLocalRule2GCommonRule(r));
            var termAndRulesMapping = new DAUtil().GetTermAndRuleMappingsForDataSync(DateTime.UtcNow, commonSORules);//Init Term Rule Settings
            return RMDtoConverter.ConvertTermAndRuleMappings2GlobalDto(termAndRulesMapping);
        }

        private List<AvePoint.RA.Contract.Global.Object.RMTermInfo> AssembleTermInfos()
        {
            var terms = TermDao.GetAllTermsForce();
            var termInfos = terms.ConvertAll(t => ConvertRMTerm2TermInfoDto(t));
            return termInfos;
        }

        private AvePoint.RA.Contract.Global.Object.RMTermInfo ConvertRMTerm2TermInfoDto(RMTerm term)
        {
            AvePoint.RA.Contract.Global.Object.RMTermInfo termInfo = new AvePoint.RA.Contract.Global.Object.RMTermInfo()
            {
                Id = term.Id,
                Name = term.Name,
                // Type = (AvePoint.RA.Contract.Global.Object.RMTermType)term.Type,
                UniqueId = term.UniqueId
            };
            return termInfo;
        }

        private async Task<string> RunDataSyncJobBySelectdNodeAsync(string jobRunByUser, JobType jobType, RMSPTreeNode selectedNode)
        {
            string jobId = string.Empty;
            //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            jobId = RMJobService.CreateJob(JobType.SPOnPremDataSync, jobRunByUser, GetSPContainerId(selectedNode));
            List<RMSPTreeNode> availableNode = new List<RMSPTreeNode>();

            try
            {
                availableNode = await this.AssembleSyncDataRunnableNodeAsync(selectedNode);
                if (availableNode.IsNullOrEmpty())
                {
                    Logger.Warn("No available sc to run");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_NoSCUnderGroup");
                    return jobId;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error occurred while get available node for data sync job. ERROR:{0}", ex.ToString());
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, ex.Message);
                return jobId;
            }
            
            int subJobCount = availableNode.Count % RMGlobalConfiguration.AppConfig.NodeCountInSubJob == 0 ? availableNode.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob : availableNode.Count / RMGlobalConfiguration.AppConfig.NodeCountInSubJob + 1;
            SubJobDao.UpdateSubJobCount(jobId, subJobCount);

            int currentSubjobIndex = 0;
            List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();
            foreach (RMSPTreeNode site in availableNode)
            {
                tempList.Add(site);
                if (tempList.Count == RMGlobalConfiguration.AppConfig.NodeCountInSubJob)
                {
                    string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile);
                    if (currentSubjobIndex < subJobCountInConfigFile)  //一次只发两个子job, 后续在JobInfoUpdater中触发
                    {
                        HybridSharePointWorkerService.StartSPJob(new Hybrid.Contract.RecordsJobArgs()
                        {
                            JobId = subJobId,
                            JobType = AvePoint.Hybrid.Contract.JobType.SharePointOnPremDataSync,
                            TenantId = TenantLocalValue.LogonGroupId,
                            FarmId = tempList[0].FarmId
                        });
                    }
                    tempList.Clear();
                    currentSubjobIndex++;
                }
            }
            if (tempList.Count > 0)
            {
                string subJobId = CreateSubJob(jobId, currentSubjobIndex, jobType, subJobCount, tempList, currentSubjobIndex < subJobCountInConfigFile);
                if (currentSubjobIndex < subJobCountInConfigFile) //一次只发两个子job, 后续在JobInfoUpdater中触发
                {
                    HybridSharePointWorkerService.StartSPJob(new Hybrid.Contract.RecordsJobArgs()
                    {
                        JobId = subJobId,
                        JobType = AvePoint.Hybrid.Contract.JobType.SharePointOnPremDataSync,
                        TenantId = TenantLocalValue.LogonGroupId,
                        FarmId = tempList[0].FarmId
                    });
                }
                tempList.Clear();
            }
            return jobId;
        }

        private bool IsEnableRecordManagement(RMSPTreeNode selectedTree)
        {
            Guid siteId = Guid.NewGuid();
            Guid siteGroupId = Guid.NewGuid();
            RMSharePointOnPremiseSetting setting = null;

            //当前只有两个类型的结点可以启动Sync Job: 一类是Group,一类是SiteCollection
            int cnt = 6;
            do
            {
                switch ((NodeLevel)selectedTree.Level)
                {
                    case NodeLevel.WebApplication:
                        {
                            siteId = Guid.Empty;
                            siteGroupId = Guid.Parse(selectedTree.SPObjectId);
                            break;
                        }
                    case NodeLevel.SiteCollection:
                        {
                            siteId = Guid.Parse(selectedTree.SPObjectId);
                            siteGroupId = selectedTree.SiteGroupId;
                            break;
                        }
                }
                setting = SharePointOnPremiseSettingDao.GetSettingInfoByScope(siteGroupId, siteId, Guid.Parse(selectedTree.SPObjectId));
                selectedTree = selectedTree.Parent;
            }
            while (setting == null && selectedTree != null && cnt-- > 0);

            if (setting == null || setting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
            {
                Logger.Info($"IsEnableRecordManagement:setting==null:{setting == null}");
                return false;
            }
            Logger.Info($"IsEnableRecordManagement:{true}");
            return true;
        }

        private async Task<bool> IsHaveAvailableNodesAsync(RMSPTreeNode selectedTree)
        {
            List<RMSPTreeNode> lstAvailableNodes = await AssembleSyncDataRunnableNodeAsync(selectedTree);
            if (lstAvailableNodes == null || lstAvailableNodes.Count() <= 0)
            {
                return false;
            }
            return true;
        }

        private async Task<List<RMSPTreeNode>> AssembleSyncDataRunnableNodeAsync(RMSPTreeNode selectedNode)
        {
            List<RMSPTreeNode> availableNode = new List<RMSPTreeNode>();
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                //List<RMSPTreeNode> sites = RMSPTreeService.Browse(selectedNode);
                List<RMSPTreeNode> sites = (await RMSharePointOnPremBrowseService.BrowseAsync(RMDtoConverter.ConvertRMTree2SPTree(selectedNode))).ConvertAll(n => RMDtoConverter.ConvertSPTree2RMTree(n));
                if (sites.IsNullOrEmpty())
                {
                    return availableNode;
                }
                await this.LoadSPSettingAsync(sites);
                foreach (RMSPTreeNode site in sites)
                {
                    if (site.IsSyncData && site.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)//RECO-3282  RECO-3268
                    //if (!site.IsCustomSetting && site.IsSyncData)   //去掉CustomSetting的节点
                    {
                        availableNode.Add(site);
                    }
                }
            }
            else
            {
                if (ValidateSiteExist(selectedNode))
                {
                    availableNode.Add(selectedNode);
                }
                else
                {
                    Logger.Info("Site collection not exist, site:{0}", selectedNode.Name);
                }
            }
            return availableNode;
        }

        private bool ValidateSiteExist(RMSPTreeNode selectedNode)
        {
            AvePoint.RA.Contract.SharePoint.OnPrem.RMSiteCollection site = null;
            try
            {
                //DAOAPIClientV1 client = new DAOAPIClientV1();
                //testMailbox = client.GetExchangeNodeById(dbNodeInfo.Id);
                site = AvePoint.RA.RACommonUtility.SharePointOnPrem.SharePointOnPremClient.GetLocalSiteCollectionById(selectedNode.Id);
            }
            catch (Exception e)
            {
                Logger.Error("get sp node error:{0}", e.ToString());
            }
            return site != null ? true : false;
        }

       
        public async System.Threading.Tasks.Task LoadSPSettingAsync(List<RMSPTreeNode> nodes)
        {
            try
            {
                foreach (var node in nodes)
                {
                    bool ownSetting = true;
                    var groupNode = GetGroupNode(node);
                    Guid groupId = Guid.Empty;
                    string GlobalColumnName = string.Empty;
                    bool folderDisable = false;
                    if (groupNode != null)
                    {
                        groupId = new Guid(groupNode.SPObjectId);
                    }
                    var GSetting = SharePointOnPremiseSettingDao.LoadSharePointSetting(groupId, Guid.Empty);
                    if (GSetting != null)
                    {
                        GlobalColumnName = GSetting.ColumnName;
                        var termScope = TermDao.GetRMTermByGuId(GSetting.DefaultTermId);
                        var containerTerm = TermDao.GetRMTermByGuId(GSetting.TermIdOfContainer);

                        node.ColumnName = GlobalColumnName;
                        node.ExistColumnName = GSetting.ExistColumnName;
                        node.IsUsingExistColumnName = GSetting.IsUsingExistColumnName;
                        node.TermNameOfContainer = containerTerm == null ? GSetting.TermNameOfContainer : containerTerm.Name;
                        node.TermSetName = GSetting.TermSetName;
                        node.DefaultTermName = termScope == null ? GSetting.DefaultTermName : termScope.Name;
                        node.DefaultTermNameFullPath = termScope == null ? GSetting.DefaultTermName : TermDao.GetTermFullPathByTermId(GSetting.DefaultTermId);
                        node.IsDisplyaTermPath = GSetting.IsDisplyaTermPath;
                        node.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(GSetting.Id, RecordOwnerSettingType.SharePoint);
                        node.IsDefaultTermRemoved = termScope == null ? false : termScope.IsRemoved;
                        node.IsDefaultTermDeprecated = termScope == null ? false : termScope.IsDeprecated || TermDao.IsExpiredTerm(termScope.Id);
                        node.isFailedConfigClassification = GSetting.IsFailedConfigClassification;
                        node.isFailedConfigMetaDataColumn = GSetting.IsFailedConfigMetaDataColumn;
                        node.IsClassificationTermRemoved = containerTerm == null ? false : containerTerm.IsRemoved;
                        node.IsClassificationTermDeprecated = containerTerm == null ? false : containerTerm.IsDeprecated || TermDao.IsExpiredTerm(containerTerm.Id);
                        node.EnableRelatedRecords = GSetting.EnableRelatedRecords;
                        node.EnableRecordManagement = GSetting.EnableRecordManagement;
                        node.isEnableClassification = GSetting.IsEnableContainerLevelTerm;
                        node.IsSyncData = GSetting.IsSyncData;
                    }
                    var siteNode = GetSiteCollectionNode(node);
                    Guid siteId = Guid.Empty;
                    if (siteNode != null)
                    {
                        siteId = new Guid(siteNode.SPObjectId);
                    }
                    var SPSetting = SharePointOnPremiseSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), siteId);
                    if (SPSetting != null && (SPSetting.TermIdOfContainer != Guid.Empty || SPSetting.TermId != Guid.Empty || SPSetting.EnableRecordManagement == (int)EnableRecordManagementSetting.Disable))
                    {
                        node.HasCustomSetting = true;
                    }
                    else
                    {
                        node.HasCustomSetting = false;
                    }

                    if (SPSetting != null)
                    {
                        node.IsCustomSetting = true;
                    }
                    if (node.Level == (int)NodeLevel.Folder)// site,list disable, all folder disable
                    {
                        var pNode = LoadFolderParentSeting(node, siteId);
                        if (pNode != null && pNode.EnableRecordManagement == (int)EnableRecordManagementSetting.ParentDisable)
                        {
                            if (SPSetting != null)
                            {
                                SPSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                            }
                            folderDisable = true;
                        }
                    }

                    if (SPSetting == null)
                    {
                        if (node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Site || node.Level == (int)NodeLevel.Folder)
                        {
                            SPSetting = LoadParentSeting(node.Parent, siteId);
                            if (SPSetting != null && node.Level != (int)NodeLevel.WebApplication)
                            {
                                if (SPSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable || folderDisable)
                                {
                                    SPSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                                }

                            }
                        }
                    }
                    //else
                    //{
                    //    node.IsCustomSetting = true;
                    //}



                    if (SPSetting != null)
                    {
                        var termScope = TermDao.GetRMTermByGuId(SPSetting.TermId);
                        var defaultTerm = TermDao.GetRMTermByGuId(SPSetting.DefaultTermId);
                        var containerTerm = TermDao.GetRMTermByGuId(SPSetting.TermIdOfContainer);

                        node.ColumnName = GlobalColumnName;
                        node.Description = SPSetting.Description;
                        node.DefaultTermId = SPSetting.DefaultTermId;
                        node.DefaultTermName = defaultTerm == null ? SPSetting.DefaultTermName : defaultTerm.Name;
                        node.DefaultTermNameFullPath = defaultTerm == null ? SPSetting.DefaultTermName : TermDao.GetTermFullPathByTermId(SPSetting.DefaultTermId);
                        node.TermId = SPSetting.TermId;
                        node.TermName = termScope == null ? SPSetting.TermName : termScope.Name;
                        node.TermNameFullPath = termScope == null ? SPSetting.TermName : TermDao.GetTermFullPathByTermId(SPSetting.TermId);
                        node.TermSetId = SPSetting.TermSetId;
                        node.TermSetName = SPSetting.TermSetName;
                        node.IsTermRemoved = termScope == null ? false : termScope.IsRemoved;
                        node.IsDefaultTermRemoved = defaultTerm == null ? false : defaultTerm.IsRemoved;
                        node.IsTermDeprecated = termScope == null ? false : termScope.IsDeprecated || TermDao.IsExpiredTerm(termScope.Id);
                        node.IsDefaultTermDeprecated = defaultTerm == null ? false : defaultTerm.IsDeprecated || TermDao.IsExpiredTerm(defaultTerm.Id);
                        node.DescriptionOfContainer = SPSetting.DescriptionOfContainer;
                        node.TermIdOfContainer = SPSetting.TermIdOfContainer;
                        node.TermNameOfContainer = containerTerm == null ? SPSetting.TermNameOfContainer : containerTerm.Name;
                        node.isEnableClassification = SPSetting.IsEnableContainerLevelTerm;
                        node.EnableRecordManagement = SPSetting.EnableRecordManagement;
                        //node.IsEnableHoldPhyical = SPSetting.hold;
                        node.isFailedConfigClassification = SPSetting.IsFailedConfigClassification;
                        node.isFailedConfigMetaDataColumn = SPSetting.IsFailedConfigMetaDataColumn;
                        node.IsClassificationTermRemoved = containerTerm == null ? false : containerTerm.IsRemoved;
                        node.IsClassificationTermDeprecated = containerTerm == null ? false : containerTerm.IsDeprecated || TermDao.IsExpiredTerm(containerTerm.Id);
                        node.ExistColumnName = SPSetting.ExistColumnName;
                        node.IsUsingExistColumnName = SPSetting.IsUsingExistColumnName;
                        node.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(SPSetting.Id, RecordOwnerSettingType.SharePoint);
                        node.EMailToRecordOwner = SPSetting.EMailToRecordOwner;
                        node.IsDisplyaTermPath = SPSetting.IsDisplyaTermPath;
                        node.EnableRelatedRecords = SPSetting.EnableRelatedRecords;
                        node.IsSyncData = SPSetting.IsSyncData;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred when load SharePointSetting.Error:{0}", e.ToString());
                throw;
            }
        }

        public RMSharePointOnPremiseSetting LoadParentSeting(RMSPTreeNode node, Guid siteId)
        {
            RMSharePointOnPremiseSetting SPSetting = null;

            if (node.Level == (int)NodeLevel.WebApplication)
            {
                return SPSetting;
            }

            if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site || node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Folder)
            {
                SPSetting = SharePointOnPremiseSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), siteId);
            }


            if (SPSetting == null)
            {
                SPSetting = LoadParentSeting(node.Parent, siteId);
            }

            return SPSetting;
        }
        #endregion

        #region Save SharePoint Settings Action to RMDB, Apply Action to run job.
        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditSPOnPremColumnSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddColumnSettingAsync(RMSPTreeNode groupNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                Logger.Info("Set SharePoint Column Setting");
                result.MessageType = RAMessageType.Successful;
                if (groupNode.IsShowUniqueId)
                {
                    UniqueIdSetting curUniqueIdSetting = UniqueIdSettingService.LoadingUniqueIdSetting();
                    if (curUniqueIdSetting == null || !curUniqueIdSetting.IsActived)
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.UniqueIdSettingIsEmpty;
                        return result;
                    }
                }
                if (!CheckParentNodeDisable(groupNode, Guid.Empty.ToString()))
                {
                    if (!groupNode.IsUsingExistColumnName)
                    {
                        SharePointOnPremiseSettingDao.UpdateBCSColumnName(groupNode.SiteGroupId, groupNode.ColumnName, groupNode.Description, groupNode.ColumnRequired);
                        await SharePointOnPremiseSettingDao.AddOrUpdateGlobalSettingAsync(groupNode);
                    }
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DisableRecordsManagement;
                    result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                    return result;
                }
                return result;
            }
            catch (Exception ex)
            {
                Logger.Warn("Save Global Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditSPOnPremConLevelSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddContainerTermAsync(RMSPTreeNode containerNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                Logger.Info("Set Container SharePoint Setting");
                var settingNode = containerNode;
                if (containerNode.Level == (int)NodeLevel.WebApplication)
                {
                    if (!CheckParentNodeDisable(settingNode, Guid.Empty.ToString()))
                    {
                        await SharePointOnPremiseSettingDao.AddOrUpdateGlobalSettingAsync(containerNode);
                    }
                    else
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.DisableRecordsManagement;
                        result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                        return result;
                    }
                }
                else
                {
                    Logger.Info("Set Container SharePoint Setting, current node save term as group : {0}", containerNode.FullPath);
                    RMSPTreeNode siteCollectionNode = null;
                    siteCollectionNode = GetSiteCollectionNode(settingNode);
                    if (!CheckParentNodeDisable(settingNode, siteCollectionNode.SPObjectId))
                    {
                        SetPropertiesByNodeLevel(settingNode, siteCollectionNode);
                        await SharePointOnPremiseSettingDao.AddOrUpdateCustomSettingAsync(settingNode, new Guid(siteCollectionNode.SPObjectId));
                    }
                    else
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.DisableRecordsManagement;
                        result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                        return result;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Logger.Warn("Save Custom Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditSPOnPremDocLevelSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddCustomColumnAsync(RMSPTreeNode customNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                Logger.Info("Set Custom SharePoint Setting");
                var settingNode = customNode;
                RMSPTreeNode siteCollectionNode = null;

                siteCollectionNode = GetSiteCollectionNode(settingNode);
                if (!CheckParentNodeDisable(settingNode, siteCollectionNode.SPObjectId))
                {
                    SetPropertiesByNodeLevel(settingNode, siteCollectionNode);
                    AddFilterCretiaProperty(settingNode.AutoClassificationRules);
                    await SharePointOnPremiseSettingDao.AddOrUpdateCustomSettingAsync(settingNode, new Guid(siteCollectionNode.SPObjectId));
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DisableRecordsManagement;
                    result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                    return result;
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.Warn("Save Custom Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        public async Task<RAReturnMessage> AddEnableColumnSettingAsync(RMSPTreeNode settingNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                if (settingNode.Level == (int)NodeLevel.WebApplication)
                {
                    SharePointOnPremiseSettingDao.UpdateBCSColumnName(settingNode.SiteGroupId, settingNode.ColumnName, settingNode.Description, settingNode.ColumnRequired);
                    await SharePointOnPremiseSettingDao.AddOrUpdateGlobalSettingAsync(settingNode);
                }
                else
                {
                    RMSPTreeNode siteCollectionNode = GetSiteCollectionNode(settingNode);
                    if (!CheckParentNodeDisable(settingNode, siteCollectionNode.SPObjectId, false))
                    {

                        SetPropertiesByNodeLevel(settingNode, siteCollectionNode);
                        AddFilterCretiaProperty(settingNode.AutoClassificationRules);
                        await SharePointOnPremiseSettingDao.AddOrUpdateCustomSettingAsync(settingNode, new Guid(siteCollectionNode.SPObjectId));
                    }
                    else
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.DisableRecordsManagement;
                        result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                        return result;
                    }
                }
                string nodeProfileIdPath = ScheduleService.GetProfileId(settingNode);
                SharePointOnPremiseSettingDao.CheckNeedRemoveDescendantsSetting(settingNode, nodeProfileIdPath);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Warn("Save Global Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditSPOnPremDocLevelSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddGlobalColumnAsync(RMSPTreeNode groupNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                Logger.Info("Set Global SharePoint Setting");
                if (!CheckParentNodeDisable(groupNode, Guid.Empty.ToString()))
                {
                    if (!groupNode.IsUsingExistColumnName || (groupNode.IsUsingExistColumnName && groupNode.SetDocLevelTermForExistColumn))
                    {
                        AddFilterCretiaProperty(groupNode.AutoClassificationRules);
                        //SharePointSettingDao.UpdateBCSColumnName(groupNode.SiteGroupId, groupNode.ColumnName);
                        await SharePointOnPremiseSettingDao.AddOrUpdateGlobalSettingAsync(groupNode);
                    }
                    //else
                    //{
                    //    SharePointSettingDao.AddOrUpdateGlobalSettingUsingExistColumn(groupNode);
                    //}
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DisableRecordsManagement;
                    result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                    return result;
                }
                return result;
            }
            catch (Exception ex)
            {
                Logger.Warn("Save Global Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        public async Task<RAReturnMessage> AddIsSyncSettingAsync(RMSPTreeNode settingNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                if (settingNode.Level == (int)NodeLevel.WebApplication)
                {
                    if (!CheckParentNodeDisable(settingNode, Guid.Empty.ToString()))
                    {
                        SharePointOnPremiseSettingDao.UpdateBCSColumnName(settingNode.SiteGroupId, settingNode.ColumnName, settingNode.Description, settingNode.ColumnRequired);
                        await SharePointOnPremiseSettingDao.AddOrUpdateGlobalSettingAsync(settingNode);
                    }
                    else
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.DisableRecordsManagement;
                        result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                        return result;
                    }
                }
                else
                {
                    RMSPTreeNode siteCollectionNode = GetSiteCollectionNode(settingNode);
                    if (!CheckParentNodeDisable(settingNode, siteCollectionNode.SPObjectId))
                    {

                        SetPropertiesByNodeLevel(settingNode, siteCollectionNode);
                        AddFilterCretiaProperty(settingNode.AutoClassificationRules);
                        await SharePointOnPremiseSettingDao.AddOrUpdateCustomSettingAsync(settingNode, new Guid(siteCollectionNode.SPObjectId));
                    }
                    else
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.DisableRecordsManagement;
                        result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                        return result;
                    }
                }
                //SharePointSettingDao.RemoveDescendantsSetting(settingNode);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Warn("Save Global Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.GeneralSetting4SPOnPrem, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddSPOnPremGeneralSettingAsync(RMSPTreeNode settingNode)
        {
            RAReturnMessage enableResult = await AddEnableColumnSettingAsync(settingNode);
            RAReturnMessage isSyncResult = new RAReturnMessage();
            if (settingNode.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
            {
                isSyncResult = await AddIsSyncSettingAsync(settingNode);
            }
            RAReturnMessage result = new RAReturnMessage();
            if (enableResult.MessageType == RAMessageType.Failed)
            {
                result = enableResult;
            }
            else if (isSyncResult.MessageType == RAMessageType.Failed)
            {
                result = isSyncResult;
            }
            else
            {
                result.MessageType = RAMessageType.Successful;
            }
            return result;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditSPOnPremLocationOwnersSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddLocationOwnersAsync(RMSPTreeNode locationNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                Logger.Info("Set Location Owners On-Prem SharePoint Setting");
                var settingNode = locationNode;
                if (locationNode.Level == (int)NodeLevel.WebApplication)
                {
                    if (!CheckParentNodeDisable(settingNode, Guid.Empty.ToString()))
                    {
                        await SharePointOnPremiseSettingDao.AddOrUpdateGlobalSettingAsync(locationNode);
                    }
                    else
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.DisableRecordsManagement;
                        result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                        return result;
                    }
                }
                else
                {
                    Logger.Info("Set Location Owners On-Prem SharePoint Setting, current node save term as group : {0}", locationNode.FullPath);
                    RMSPTreeNode siteCollectionNode = null;
                    siteCollectionNode = GetSiteCollectionNode(settingNode);
                    if (!CheckParentNodeDisable(settingNode, siteCollectionNode.SPObjectId))
                    {
                        SetPropertiesByNodeLevel(settingNode, siteCollectionNode);
                        await SharePointOnPremiseSettingDao.AddOrUpdateCustomSettingAsync(settingNode, new Guid(siteCollectionNode.SPObjectId));
                    }
                    else
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.DisableRecordsManagement;
                        result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                        return result;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Logger.Warn("Save Location Owners Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }


        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditSPOnPremColumnSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> AddUsingExistColumnSettingAsync(RMSPTreeNode groupNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                Logger.Info("Begin save global using column name settings {0}:{1}", groupNode.FullPath, groupNode.ExistColumnName);
                result.MessageType = RAMessageType.Successful;
                if (groupNode.IsShowUniqueId)
                {
                    UniqueIdSetting curUniqueIdSetting = UniqueIdSettingService.LoadingUniqueIdSetting();
                    if (curUniqueIdSetting == null || !curUniqueIdSetting.IsActived)
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.UniqueIdSettingIsEmpty;
                        return result;
                    }
                }
                if (!CheckParentNodeDisable(groupNode, Guid.Empty.ToString()))
                {
                    await SharePointOnPremiseSettingDao.AddOrUpdateGlobalSettingUsingExistColumnAsync(groupNode);
                    Logger.Info("using column name add or update global serring succes,group node:{0}", groupNode.Name);
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DisableRecordsManagement;
                    result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                    return result;
                }
                return result;
            }
            catch (Exception e)
            {
                Logger.Warn("using column name add or update global serring occur error,group node:{0},info:{1}", groupNode.Name, e.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        public bool CheckParentNodeDisable(RMSPTreeNode settingNode, string SPObjectId, bool isCheckSelfNode = true)
        {
            string scopeIdString = string.Empty;
            var isDisableRecordsManagement = false;
            try
            {
                Expression<Func<RMSharePointOnPremiseSetting, bool>> whereLambda = this.GetCheckDisableLambda(settingNode, SPObjectId, isCheckSelfNode);
                Logger.Debug($"CheckParentNodeDisable where lambda: {whereLambda}");
                if (SharePointOnPremiseSettingDao.GetParentNode(whereLambda) != null)
                {
                    isDisableRecordsManagement = true;
                }

            }
            catch (Exception ex)
            {
                Logger.Error("Check Parent Node Records Management error:{0}", ex.ToString());
            }
            return isDisableRecordsManagement;
        }

        public List<string> GetDesignLists()
        {
            List<string> results = new List<string>();
            try
            {
                string configFilePath = System.AppDomain.CurrentDomain.BaseDirectory + "Config\\DesignLists\\DesignLists.config";
                XmlDocument doc = new XmlDocument();
                doc.Load(configFilePath);
                foreach (var node in doc.GetElementsByTagName("List"))
                {
                    XmlElement xe = (XmlElement)node;
                    results.Add(xe.GetAttribute("url") + xe.GetAttribute("serverTemplate"));
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("Get Design Lists config file error {0}", ex.ToString());
            }
            return results;
        }

        public RMSPTreeNode GetSiteCollectionNode(RMSPTreeNode node)
        {
            while (node != null && node.Level != (int)NodeLevel.SiteCollection)
            {
                node = node.Parent;
            }
            return node;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditSPOnPremInheritSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> InheritParentSettingAsync(RMSPTreeNode node)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                Logger.Info("Inherit Parent Settings");
                var siteCollectionNode = GetSiteCollectionNode(node);

                await SharePointOnPremiseSettingDao.DeleteSharePointSettingAsync(new Guid(node.SPObjectId), new Guid(siteCollectionNode.SPObjectId));
                CleanParentNodeSetting(node);
                //Update the parent node setting to inherit settings. to do next.
                return result;
            }
            catch (Exception ex)
            {
                Logger.Warn("Inherit Parent Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        public async System.Threading.Tasks.Task LoadSPSettingIconAsync(List<RMSPSampleTreeNode> nodes)
        {
            try
            {
                if (nodes.Count > 0)
                {
                    RMSPSampleTreeNode tempNode = nodes[0];
                    if (tempNode.Level == (int)NodeLevel.Farm)
                    {
                        return;
                    }
                    RMSPSampleTreeNode groupNode = tempNode;
                    if (groupNode.Level != (int)NodeLevel.WebApplication)
                    {
                        while (groupNode.Level != (int)NodeLevel.WebApplication && groupNode != null)
                        {
                            groupNode = groupNode.Parent;
                        }

                        Guid groupId = Guid.Empty;
                        if (groupNode != null)
                        {
                            groupId = new Guid(groupNode.SPObjectId);
                        }
                        var gsSetting = SharePointOnPremiseSettingDao.LoadSharePointSetting(groupId, Guid.Empty);
                        var allSchedules = await ScheduleService.GetScheduleByTypeServiceAsync(ScheduleType.SPOnPremDisposalSchedule);
                        List<string> allSchedulesProfilesId = new List<string>();
                        if (allSchedules != null && allSchedules.Count != 0)
                        {
                            allSchedulesProfilesId = allSchedules.Select(s => s.ProfileId).ToList();
                        }

                        var allSettings = new Dictionary<string, RMSharePointOnPremiseSetting>();
                        var settings = SharePointOnPremiseSettingDao.LoadSharePointSettings(groupId).OrderBy(item => item.Id);
                        foreach (var setting in settings)
                        {
                            var key = setting.ScopeId.ToString() + setting.SiteId.ToString();
                            if (!allSettings.ContainsKey(key))
                            {
                                allSettings.Add(key, setting);
                            }
                        }
                        foreach (var node in nodes)
                        {
                            ArgumentCheck.NotNull(node, nameof(node));
                            var siteNode = node;
                            while (siteNode != null && siteNode.Level != (int)NodeLevel.SiteCollection)
                            {
                                siteNode = siteNode.Parent;
                            }
                            RMSharePointOnPremiseSetting csSetting = null;
                            var settingKey = node?.SPObjectId + siteNode?.SPObjectId;
                            if (allSettings.TryGetValue(settingKey, out csSetting))
                            {
                                node.IconStatus = IconStatus.Break;
                                continue;
                            }
                            var profileId = ScheduleService.GetProfileId(node);
                            if (allSchedulesProfilesId.Contains(profileId))
                            {
                                node.IconStatus = IconStatus.Break;
                                continue;
                            }
                            if (gsSetting != null)
                            {
                                node.IconStatus = IconStatus.Inhert;
                                continue;
                            }
                            node.IconStatus = IconStatus.NoSet;
                        }
                    }
                    else
                    {
                        foreach (var selfGroupNode in nodes)
                        {
                            var selfGSSetting = SharePointOnPremiseSettingDao.LoadSharePointSetting(new Guid(selfGroupNode.SPObjectId), Guid.Empty);
                            if (selfGSSetting == null)
                            {
                                selfGroupNode.IconStatus = IconStatus.NoSet;
                            }
                            else
                            {
                                selfGroupNode.IconStatus = IconStatus.Break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred when load SharePointSetting Icon.Error:{0}", e.ToString());
                throw;
            }
        }

        public async Task<RMSPTreeNode> LoadSampleNodeSettingsAsync(RMSPSampleTreeNode sNode)
        {
            var configNode = new RMSPTreeNode();
            configNode.IconStatus = IconStatus.NoSet;
            #region copy node properties
            configNode.Id = sNode.Id;
            configNode.FarmId = sNode.FarmId;
            configNode.FarmName = sNode.FarmName;
            configNode.Name = sNode.Name;
            configNode.Title = sNode.Title;
            configNode.FullPath = sNode.FullPath;
            configNode.Level = sNode.Level;
            configNode.NodeType = sNode.NodeType;
            configNode.SPType = sNode.SPType;
            configNode.SPObjectId = sNode.SPObjectId;
            configNode.SPVersion = sNode.SPVersion;
            configNode.Expanded = sNode.Expanded;
            configNode.ChildrenCount = sNode.ChildrenCount;
            configNode.CheckNumber = sNode.CheckNumber;
            configNode.Hidden = sNode.Hidden;
            configNode.TemplateId = sNode.TemplateId;
            configNode.BposInfo = sNode.BposInfo;
            #endregion

            try
            {
                RMSPSampleTreeNode groupNode = sNode;
                //TODO
                while (groupNode.Level != (int)NodeLevel.WebApplication && groupNode != null)
                {
                    groupNode = groupNode.Parent;
                }
                if (groupNode == null)
                {
                    return configNode;
                }
                //var groupNode = GetGroupNode(configNode);
                Guid groupId = Guid.Empty;
                bool ownSetting = true;
                bool folderDisable = false;
                string GlobalColumnName = string.Empty;
                string GlobalColumnNameDesc = string.Empty;
                if (groupNode != null)
                {
                    groupId = new Guid(groupNode.SPObjectId);
                }
                var GSetting = SharePointOnPremiseSettingDao.LoadSharePointSetting(groupId, Guid.Empty);
                if (GSetting != null)
                {
                    configNode.IconStatus = IconStatus.Inhert;
                    GlobalColumnName = GSetting.ColumnName;
                    GlobalColumnNameDesc = GSetting.Description;
                    var termDefaultValue = TermDao.GetRMTermByGuId(GSetting.DefaultTermId);
                    var containerTerm = TermDao.GetRMTermByGuId(GSetting.TermIdOfContainer);

                    var termScope = TermDao.GetRMTermByGuId(GSetting.TermId);
                    RMTermSet termSet = null;
                    if (GSetting.TermId == Guid.Empty)
                    {
                        termSet = TermDao.GetRMTermSetByGuid(GSetting.TermSetId);
                    }
                    configNode.ColumnName = GlobalColumnName;
                    configNode.ColumnRequired = GSetting.ColumnRequired == null ? true : (bool)GSetting.ColumnRequired;
                    configNode.Description = GlobalColumnNameDesc;
                    configNode.ExistColumnName = GSetting.ExistColumnName;
                    configNode.IsUsingExistColumnName = GSetting.IsUsingExistColumnName;
                    configNode.SetDocLevelTermForExistColumn = GSetting.SetDocLevelTermForExistColumn;
                    configNode.TermNameOfContainer = containerTerm == null ? GSetting.TermNameOfContainer : containerTerm.Name;
                    configNode.TermIdOfContainer = GSetting.TermIdOfContainer;
                    configNode.ContainerTermFullPath = GSetting.TermIdOfContainer != Guid.Empty ? TermDao.GetTermNamesPathByTermId(GSetting.TermIdOfContainer) : "";
                    configNode.isEnableClassification = GSetting.IsEnableContainerLevelTerm;
                    configNode.DescriptionOfContainer = GSetting.DescriptionOfContainer;
                    configNode.IsTermRemoved = (termScope == null ? termSet?.IsRemoved : termScope?.IsRemoved) ?? false;
                    configNode.TermSetId = GSetting.TermSetId;
                    configNode.TermSetName = GSetting.TermSetName;
                    configNode.TermId = GSetting.TermId;
                    configNode.TermName = GSetting.TermName;
                    configNode.DefaultTermId = GSetting.DefaultTermId;
                    configNode.DefaultTermName = termDefaultValue == null ? GSetting.DefaultTermName : termDefaultValue.Name;
                    configNode.TermScopeFullPath = GSetting.TermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(GSetting.TermId) : TermDao.GetTermSetNamesPathByTermSetId(GSetting.TermSetId);
                    configNode.DefaultTermFullPath = GSetting.DefaultTermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(GSetting.DefaultTermId) : "";

                    //configNode.DefaultTermNameFullPath = termDefaultValue == null ? GSetting.DefaultTermName : TermDao.GetTermFullPathByTermId(GSetting.DefaultTermId);
                    configNode.IsDisplyaTermPath = GSetting.IsDisplyaTermPath;
                    configNode.IsShowUniqueId = GSetting.IsShowUniqueId == null ? true : (bool)GSetting.IsShowUniqueId;
                    configNode.IsDefaultTermRemoved = termDefaultValue == null ? false : termDefaultValue.IsRemoved;
                    configNode.IsDefaultTermDeprecated = termDefaultValue == null ? false : termDefaultValue.IsDeprecated || TermDao.IsExpiredTerm(termDefaultValue.Id);
                    configNode.isFailedConfigClassification = GSetting.IsFailedConfigClassification;
                    configNode.isFailedConfigMetaDataColumn = GSetting.IsFailedConfigMetaDataColumn;
                    configNode.IsClassificationTermRemoved = containerTerm == null ? false : containerTerm.IsRemoved;
                    configNode.IsClassificationTermDeprecated = containerTerm == null ? false : containerTerm.IsDeprecated || TermDao.IsExpiredTerm(containerTerm.Id);
                    configNode.NeedCheckDefaultValue = GSetting.NeedCheckDefaultValue;
                    configNode.ApplyExistType = GSetting.ApplyExistType;
                    configNode.ApprovalType = (int)GSetting.ApprovalType;
                    configNode.WorkflowReferenceId = GSetting.WorkflowReferenceId;
                    if (GSetting.NeedCheckDefaultValue && GSetting.ApplyExistType == (int)Contract.TaxonomyModel.ApplyExistingTermType.None)
                    {
                        configNode.ApplyExistType = (int)Contract.TaxonomyModel.ApplyExistingTermType.None;
                    }
                    configNode.EnableRelatedRecords = GSetting.EnableRelatedRecords;
                    configNode.EMailToRecordOwner = GSetting.EMailToRecordOwner;
                    //configNode.RecordOwner = GetSettingRecordOnwers(GSetting.Id, SourceType.SharePoint);
                    configNode.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(GSetting.Id, RecordOwnerSettingType.SharePointOnPremise);
                    configNode.SiteGroupId = GSetting.SiteGroupId;
                    //configNode.ProfileId = GSetting.IdPath;
                    configNode.DeployTermMethod = GSetting.TermSetId == Guid.Empty ? DeployTermMethod.NoDefaultTerm : (DeployTermMethod)GSetting.DeployTermMethod;
                    if (GSetting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm && GSetting.DefaultTermId == Guid.Empty)
                    {
                        configNode.DeployTermMethod = DeployTermMethod.NoDefaultTerm;
                    }
                    configNode.AutoClassificationRules = GSetting.AutoClassificationRules == null ?
                        null : SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(GSetting.AutoClassificationRules);
                    SetAutoTermStatus(configNode.AutoClassificationRules);
                    await ConvertClassificationRuleTimeZoneAsync(configNode.AutoClassificationRules);
                    ConvertClassificationRuleAndOrExpression(configNode.AutoClassificationRules);
                    configNode.RunAutoFullJob = GSetting.RunAutoFullJob;
                    configNode.AutoJobOption = (AutoJobOption)GSetting.AutoJobOption == AutoJobOption.None ? AutoJobOption.SkipAndKeep : (AutoJobOption)GSetting.AutoJobOption;
                    //configNode.EnableRecordManagement = GSetting.EnableRecordManagement;
                    configNode.IncludeDeclaredRecords = GSetting.IncludeDeclaredRecords;
                    if (sNode.Level == (int)NodeLevel.SiteCollection || sNode.Level == (int)NodeLevel.Site || sNode.Level == (int)NodeLevel.List || sNode.Level == (int)NodeLevel.Folder)
                    {
                        if (GSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                        {
                            configNode.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                        }
                        else
                        {
                            configNode.EnableRecordManagement = (int)EnableRecordManagementSetting.Enable;
                        }
                    }
                    configNode.isEnableClassification = GSetting.IsEnableContainerLevelTerm;
                    configNode.IsSyncData = GSetting.IsSyncData;

                    //SetDisposeJob(configNode, GSetting.DisposalJobId1);
                    //SetCollectionJob(configNode, GSetting.CollectionJobId1);
                }
                RMSPSampleTreeNode siteNode = sNode;
                while (siteNode != null && siteNode.Level != (int)NodeLevel.SiteCollection)
                {
                    siteNode = siteNode.Parent;
                }

                Guid siteId = Guid.Empty;
                if (siteNode != null)
                {
                    siteId = new Guid(siteNode.SPObjectId);
                }
                var spSetting = SharePointOnPremiseSettingDao.LoadSharePointSetting(new Guid(sNode.SPObjectId), siteId);
                if (configNode.Level == (int)NodeLevel.Folder)// site,list disable, all folder disable
                {
                    var pNode = LoadFolderParentSeting(sNode, siteId);
                    if (pNode != null && pNode.EnableRecordManagement == (int)EnableRecordManagementSetting.ParentDisable)
                    {
                        if (spSetting != null)
                        {
                            spSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                        }
                        folderDisable = true;
                    }
                }

                if (spSetting == null)
                {
                    if (sNode.Level == (int)NodeLevel.List || sNode.Level == (int)NodeLevel.Site || sNode.Level == (int)NodeLevel.Folder)
                    {
                        spSetting = LoadSampleNodeParentSeting(sNode.Parent, siteId);
                        if (spSetting != null && configNode.Level != (int)NodeLevel.WebApplication)
                        {
                            if (spSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable || folderDisable)
                            {
                                spSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                            }
                        }
                        configNode.IsCustomSetting = false;
                    }
                }
                else
                {
                    configNode.IconStatus = IconStatus.Break;
                    if (sNode.Level != (int)NodeLevel.WebApplication)//Group Level 不能有CustomSetting，
                    {
                        configNode.IsCustomSetting = true;
                    }
                }

                if (spSetting != null)
                {
                    var termScope = TermDao.GetRMTermByGuId(spSetting.TermId);
                    var defaultTerm = TermDao.GetRMTermByGuId(spSetting.DefaultTermId);
                    var containerTerm = TermDao.GetRMTermByGuId(spSetting.TermIdOfContainer);
                    RMTermSet termSet = null;
                    if (spSetting.TermId == Guid.Empty)
                    {
                        termSet = TermDao.GetRMTermSetByGuid(spSetting.TermSetId);
                    }

                    configNode.ColumnName = GlobalColumnName;
                    configNode.Description = GlobalColumnNameDesc;
                    configNode.ColumnRequired = spSetting.ColumnRequired == null ? true : (bool)spSetting.ColumnRequired;
                    configNode.DefaultTermId = spSetting.DefaultTermId;
                    configNode.DefaultTermName = defaultTerm == null ? spSetting.DefaultTermName : defaultTerm.Name;
                    configNode.TermScopeFullPath = spSetting.TermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(spSetting.TermId) : TermDao.GetTermSetNamesPathByTermSetId(spSetting.TermSetId);
                    configNode.DefaultTermFullPath = spSetting.DefaultTermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(spSetting.DefaultTermId) : "";
                    //configNode.DefaultTermNameFullPath = defaultTerm == null ? spSetting.DefaultTermName : TermDao.GetTermFullPathByTermId(spSetting.DefaultTermId);
                    configNode.TermId = spSetting.TermId;
                    configNode.TermName = termScope == null ? spSetting.TermName : termScope.Name;
                    //configNode.TermNameFullPath = termScope == null ? spSetting.TermName : TermDao.GetTermFullPathByTermId(spSetting.TermId);
                    configNode.TermSetId = spSetting.TermSetId;
                    configNode.TermSetName = spSetting.TermSetName;
                    configNode.IsTermRemoved = (termScope == null ? termSet?.IsRemoved : termScope?.IsRemoved) ?? false;
                    configNode.IsDefaultTermRemoved = defaultTerm == null ? false : defaultTerm.IsRemoved;
                    configNode.IsTermDeprecated = termScope == null ? false : termScope.IsDeprecated || TermDao.IsExpiredTerm(termScope.Id);
                    configNode.IsDefaultTermDeprecated = defaultTerm == null ? false : defaultTerm.IsDeprecated || TermDao.IsExpiredTerm(defaultTerm.Id);
                    configNode.DescriptionOfContainer = spSetting.DescriptionOfContainer;
                    configNode.TermIdOfContainer = spSetting.TermIdOfContainer;
                    configNode.TermNameOfContainer = containerTerm == null ? spSetting.TermNameOfContainer : containerTerm.Name;
                    configNode.ContainerTermFullPath = spSetting.TermIdOfContainer != Guid.Empty ? TermDao.GetTermNamesPathByTermId(spSetting.TermIdOfContainer) : "";
                    configNode.isEnableClassification = spSetting.IsEnableContainerLevelTerm;
                    configNode.EnableRecordManagement = spSetting.EnableRecordManagement;
                    configNode.isFailedConfigClassification = spSetting.IsFailedConfigClassification;
                    configNode.isFailedConfigMetaDataColumn = spSetting.IsFailedConfigMetaDataColumn;
                    configNode.IsClassificationTermRemoved = containerTerm == null ? false : containerTerm.IsRemoved;
                    configNode.IsClassificationTermDeprecated = containerTerm == null ? false : containerTerm.IsDeprecated || TermDao.IsExpiredTerm(containerTerm.Id);
                    configNode.IsDisplyaTermPath = spSetting.IsDisplyaTermPath;
                    configNode.NeedCheckDefaultValue = spSetting.NeedCheckDefaultValue;
                    configNode.ApplyExistType = spSetting.ApplyExistType;
                    configNode.ApprovalType = (int)spSetting.ApprovalType;
                    configNode.WorkflowReferenceId = spSetting.WorkflowReferenceId;

                    if (spSetting.NeedCheckDefaultValue && spSetting.ApplyExistType == (int)Contract.TaxonomyModel.ApplyExistingTermType.None)
                    {
                        configNode.ApplyExistType = (int)Contract.TaxonomyModel.ApplyExistingTermType.SkipAndKeep;
                    }

                    configNode.EnableRelatedRecords = spSetting.EnableRelatedRecords;
                    configNode.RecordOwner = await RecordOwnerDao.GetRecordOwnerAccountsAsync(spSetting.Id, RecordOwnerSettingType.SharePointOnPremise);
                    configNode.EMailToRecordOwner = spSetting.EMailToRecordOwner;
                    configNode.IsSyncData = spSetting.IsSyncData;
                    configNode.DeployTermMethod = spSetting.TermSetId == Guid.Empty ? DeployTermMethod.NoDefaultTerm : (DeployTermMethod)spSetting.DeployTermMethod;
                    if (spSetting.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm && spSetting.DefaultTermId == Guid.Empty)
                    {
                        configNode.DeployTermMethod = DeployTermMethod.NoDefaultTerm;
                    }
                    configNode.AutoClassificationRules = spSetting.AutoClassificationRules == null ?
                        null : SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(spSetting.AutoClassificationRules);
                    SetAutoTermStatus(configNode.AutoClassificationRules);
                    await ConvertClassificationRuleTimeZoneAsync(configNode.AutoClassificationRules);
                    ConvertClassificationRuleAndOrExpression(configNode.AutoClassificationRules);
                    configNode.RunAutoFullJob = spSetting.RunAutoFullJob;
                    configNode.AutoJobOption = (AutoJobOption)spSetting.AutoJobOption == AutoJobOption.None ? AutoJobOption.SkipAndKeep : (AutoJobOption)spSetting.AutoJobOption;
                    configNode.IncludeDeclaredRecords = spSetting.IncludeDeclaredRecords;
                }

                if (string.IsNullOrEmpty(configNode.ColumnName))
                {
                    configNode.ColumnRequired = true;
                }

                var profileId = ScheduleService.GetProfileId(sNode);
                var disposeSchedule = await ScheduleService.GetScheduleAsync(profileId, ScheduleType.SPOnPremDisposalSchedule);
                if (disposeSchedule != null)
                {
                    var simplifyZoneInfo = DateTimeUtil.GetSimplifyZoneInfo(disposeSchedule.TimeZoneId);
                    disposeSchedule.StartTime = string.Format($"{disposeSchedule.StartTime} {simplifyZoneInfo}");
                    disposeSchedule.EndTime = string.Format($"{disposeSchedule.EndTime} {simplifyZoneInfo}");
                    configNode.DisposeScheduleInfo = disposeSchedule;
                    configNode.DisposeScheduleInfo.Extentions = JsonConvert.DeserializeObject<RMSPTreeNode>(configNode.DisposeScheduleInfo.Extentions).SkipRemoveContentAndDestroyAction.ToString();
                    //configNode.IsCustomSetting = true;
                    configNode.IconStatus = IconStatus.Break;
                    //if (!configNode.IsCustomSetting && configNode.Level != (int)NodeLevel.WebApplication)
                    //{
                    //    configNode.DisposeScheduleInfo.Id = "1";
                    //}
                }
                else
                {
                    var ancestryDisposeSchedule = await ScheduleService.GetAncestryScheduleAsync(profileId, ScheduleType.SPOnPremDisposalSchedule);
                    if (ancestryDisposeSchedule != null)
                    {
                        var simplifyZoneInfo = DateTimeUtil.GetSimplifyZoneInfo(ancestryDisposeSchedule.TimeZoneId);
                        ancestryDisposeSchedule.StartTime = string.Format($"{ancestryDisposeSchedule.StartTime} {simplifyZoneInfo}");
                        ancestryDisposeSchedule.EndTime = string.Format($"{ancestryDisposeSchedule.EndTime} {simplifyZoneInfo}");
                        configNode.DisposeScheduleInfo = ancestryDisposeSchedule;
                        configNode.DisposeScheduleInfo.Id = "1";//回显先祖的schedule给假ID，防止删除schedule将先祖的删掉
                        configNode.DisposeScheduleInfo.Extentions = JsonConvert.DeserializeObject<RMSPTreeNode>(configNode.DisposeScheduleInfo.Extentions).SkipRemoveContentAndDestroyAction.ToString();
                    }
                    else
                    {
                        configNode.DisposeScheduleInfo = null;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred when load SharePointSetting.Error:{0}", e.ToString());
                throw;
            }
            return configNode;
        }
        #endregion

        #region Privete Method
        private void SetPropertiesByNodeLevel(RMSPTreeNode settingNode, RMSPTreeNode siteCollectionNode)
        {
            if (settingNode.Level == (int)NodeLevel.Folder)
            {
                settingNode.FolderId = new Guid(settingNode.SPObjectId);
                settingNode.WebId = new Guid(GetWebNode(settingNode).SPObjectId);//set Web Id
                settingNode.ListId = new Guid(GetListNode(settingNode).SPObjectId);//set List Id
                settingNode.isEnableClassification = false;
                settingNode.DescriptionOfContainer = null;
                settingNode.TermIdOfContainer = Guid.Empty;
                settingNode.TermNameOfContainer = null;
                settingNode.FullPath = WebUtil.MakeFullUrl(siteCollectionNode.FullPath, settingNode.FullPath);
            }
            if (settingNode.Level == (int)NodeLevel.List || settingNode.Level == (int)NodeLevel.Library)
            {
                settingNode.ListId = new Guid(settingNode.SPObjectId);
                settingNode.WebId = new Guid(settingNode.Parent.Parent.SPObjectId);//set Web Id
            }
            else if (settingNode.Level == (int)NodeLevel.Site)
            {
                settingNode.WebId = new Guid(settingNode.SPObjectId);
            }
            var groupNode = GetGroupNode(settingNode);
            Guid groupId = Guid.Empty;
            if (groupNode != null)
            {
                groupId = new Guid(groupNode.SPObjectId);
                settingNode.SiteGroupId = groupId;
            }
            var GSetting = SharePointOnPremiseSettingDao.LoadSharePointSetting(groupId, Guid.Empty);
            if (GSetting != null)
            {
                settingNode.IsUsingExistColumnName = GSetting.IsUsingExistColumnName;
            }
        }

        private RMSPTreeNode GetGroupNode(RMSPTreeNode node)
        {
            if (node.Level != (int)NodeLevel.WebApplication)
            {
                while (node.Level != (int)NodeLevel.SiteCollection)
                {
                    node = node.Parent;
                }
                return node.Parent;
            }
            else
            {
                return node;
            }
        }
        private RMSPTreeNode GetListNode(RMSPTreeNode node)
        {
            while (node.Level != (int)NodeLevel.List)
            {
                node = node.Parent;
            }
            return node;
        }
        private RMSPTreeNode GetWebNode(RMSPTreeNode node)
        {
            while (node.Level != (int)NodeLevel.Site)
            {
                node = node.Parent;
            }
            return node;
        }

        private void CleanParentNodeSetting(RMSPTreeNode node)
        {
            do
            {
                if (SharePointOnPremiseSettingDao.CleanSettingJobTime(node))
                {
                    break;
                }
                node = node.Parent;
            }
            while (node != null);
        }

        private RMSharePointOnPremiseSetting LoadFolderParentSeting(RMSPSampleTreeNode node, Guid siteId)
        {
            RMSharePointOnPremiseSetting SPSetting = null;

            if (node.Level == (int)NodeLevel.WebApplication)
            {
                SPSetting = SharePointOnPremiseSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), Guid.Empty);
                return SPSetting;
            }

            if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site || node.Level == (int)NodeLevel.List)
            {
                SPSetting = SharePointOnPremiseSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), siteId);
            }

            if (SPSetting == null)
            {
                SPSetting = LoadFolderParentSeting(node.Parent, siteId);
                if (SPSetting != null)
                {
                    if (SPSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                    {
                        SPSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                    }

                }
            }

            return SPSetting;
        }
        private RMSharePointOnPremiseSetting LoadFolderParentSeting(RMSPTreeNode node, Guid siteId)
        {
            RMSharePointOnPremiseSetting SPSetting = null;

            if (node.Level == (int)NodeLevel.WebApplication)
            {
                SPSetting = SharePointOnPremiseSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), Guid.Empty);
                return SPSetting;
            }

            if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site || node.Level == (int)NodeLevel.List)
            {
                SPSetting = SharePointOnPremiseSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), siteId);
            }

            if (SPSetting == null)
            {
                SPSetting = LoadFolderParentSeting(node.Parent, siteId);
                if (SPSetting != null)
                {
                    if (SPSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                    {
                        SPSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                    }

                }
            }

            return SPSetting;
        }

        private RMSharePointOnPremiseSetting LoadSampleNodeParentSeting(RMSPSampleTreeNode node, Guid siteId)
        {
            RMSharePointOnPremiseSetting SPSetting = null;

            if (node.Level == (int)NodeLevel.WebApplication)
            {
                return SPSetting;
            }

            if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.Site || node.Level == (int)NodeLevel.List || node.Level == (int)NodeLevel.Folder)
            {
                SPSetting = SharePointOnPremiseSettingDao.LoadSharePointSetting(new Guid(node.SPObjectId), siteId);
            }


            if (SPSetting == null)
            {
                SPSetting = LoadSampleNodeParentSeting(node.Parent, siteId);
            }

            return SPSetting;
        }

        private Expression<Func<RMSharePointOnPremiseSetting, bool>> GetCheckDisableLambda(RMSPTreeNode settingNode, string SPObjectId, bool isCheckSelfNode = true)
        {
            Expression queryExpr = null;
            List<Expression> allExpressionList = new List<Expression>();
            ParameterExpression param = Expression.Parameter(typeof(RMSharePointOnPremiseSetting), "c");
            List<Expression> nodeIdExpressionList = new List<Expression>();
            List<Guid> scopeIds = GetParentScopeId(settingNode, isCheckSelfNode);
            allExpressionList.Add(Expression4DynamicQuery.GetInExpression(typeof(RMSharePointOnPremiseSetting), param, "ScopeId", scopeIds));
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(RMSharePointOnPremiseSetting), param, "EnableRecordManagement", (int)EnableRecordManagementSetting.Disable));
            if (SPObjectId == null || SPObjectId == "")
            {
                SPObjectId = Guid.Empty.ToString();
            }
            allExpressionList.Add(Expression4DynamicQuery.GetInExpression(typeof(RMSharePointOnPremiseSetting), param, "SiteId", new List<object> { new Guid(SPObjectId), Guid.Empty }));
            queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
            return Expression.Lambda<Func<RMSharePointOnPremiseSetting, bool>>(queryExpr, param);
        }
        private List<Guid> GetParentScopeId(RMSPTreeNode settingNode, bool isCheckSelfNode)
        {
            List<Guid> scopeIds = new List<Guid>();
            if (isCheckSelfNode)
            {
                scopeIds.Add(new Guid(settingNode.SPObjectId));
            }
            while (settingNode.Parent != null && settingNode.Parent.SPObjectId != null)
            {
                scopeIds.Add(new Guid(settingNode.Parent.SPObjectId));
                settingNode = settingNode.Parent;
            }
            return scopeIds;
        }
        #endregion

        #region Enforce Rule Action Job

        public string RunOnPremiseEnforceRuleActionScheduleJob(RMSPTreeNode selectedTree, JobRunBy jobRunBy)
        {
            Logger.Debug("Start OnPremise SP Enforce Rule Action Schedule Job.");
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.SPOnPremEnforceRuleAction,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = selectedTree == null ? null : SerializerHelper.SerializeByDataContractSerializer(selectedTree)
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                Logger.Error("An error occurred while OnPremise SP Enforce Rule Action Schedule Job,ERROR:{0}.", ex.ToString());
            }
            return id;
        }

        public async Task<string> GetEnforceRuleActionJobMessageAsync(string jobId)
        {
            EnforceRuleActionJobMessage jobMessage = new EnforceRuleActionJobMessage();
            try
            {
                Logger.Debug("Start to get onpremise enforce rule action job message. Job Id:{0}.",jobId);
                var subJob = SubJobDao.GetSubJob(jobId, true);
                if (subJob.JobType == (int)JobType.SPOnPremEnforceRuleAction)
                {
                    var nodes = SerializerHelper.DeserializeByDataContractSerializer<List<RMSPTreeNode>>(subJob.JobContext.Settings);
                    //var groupMappings = SerializerHelper.DeserializeByDataContractSerializer<Dictionary<Guid, RMSharePointOnPremiseSetting>>(subJob.JobContext.Content);
                    var allSettings = SharePointOnPremiseSettingDao.LoadSharePointSettings(GetGroupIdForOnpremiseEnforceRuleActionJob(nodes[0]));
                    jobMessage.TreeNodes = AssembleTreeNodes(nodes);
                    //jobMessage.GroupSettingMapping = AssembleGroupSettingMapping(groupMappings);
                    jobMessage.AllSettings = AssembleSPSettings(allSettings);
                    jobMessage.AllTerms = TaxonomyService.GetAllTermsForce();
                    jobMessage.AllTermSets = TaxonomyService.GetAllTermSetsForce();
                    jobMessage.AllTermSetMemberships = TaxonomyService.GetAllTermSetMemberShipsForce();
                    var spRules = AgentRuleUtil.FilterRuleWithDataSource(RuleManagerService.GetRulesFromRecords(), Contract.Explorer.SourceFlag.SharePointOnPrem);
                    if (spRules.Count == 0)
                    {
                        throw new Exception("No available rules");
                    }
                    if (jobMessage.AllTerms.Count == 0)
                    {
                        throw new Exception("No available terms");
                    }
                    //转换所有的Rule为Global Rule
                    var globalRules = spRules.ConvertAll(r => RMDtoConverter.ConvertRule2GlobalDto(r));
                    //获取所有Onpremise Rule
                    var globalSPLocalRules = globalRules.Where(r => r.SPLocalRule != null);
                    //转换Rule.SPLocalRule为Rule，AgentCheck用的是Rule本身，而不是SPLocalRule
                    var globalSORules = globalSPLocalRules.ToList().ConvertAll(r => RMDtoConverter.ConvertGlbalSPLocalRule2GlobalRule(r));
                    jobMessage.AllRecordsRule = SerializerHelper.SerializeByDataContractSerializer(globalSORules);
                    jobMessage.TermIDRuleIDMapping = await GetTermRuleMappingAsync();
                    if (jobMessage.TermIDRuleIDMapping.Count == 0)
                    {
                        throw new Exception("No available term rules");
                    }
                    var commonLocalSPRules = spRules.Where(r => r.SPLocalRule != null).ToList();
                    var commonSORules = commonLocalSPRules.ConvertAll(r => RMDtoConverter.ConvertGCommonSPLocalRule2GCommonRule(r));
                    var termAndRulesMapping = new DAUtil().GetTermAndRuleMappings(DateTime.UtcNow, commonSORules);//Init Term Rule Settings
                    jobMessage.TermAndRulesMapping = RMDtoConverter.ConvertTermAndRuleMappings2GlobalDto(termAndRulesMapping);
                    //无论是Group节点还是SC/Site/List Run Job，均获取当前Group所有打破继承信息.
                    jobMessage.BreakTreeNodeUrls = OnpremiseSPBuildBreakTreeNode(GetGroupNode(nodes.FirstOrDefault()));
                    jobMessage.RunningJobNodeUrls = OnpremiseSPBuildRunningJobNode(GetGroupNode(nodes.FirstOrDefault()), JobType.SPOnPremEnforceRuleAction, jobId);
                    var generalSetting = await GetGeneralSettingModelAsync();
                    if (generalSetting != null)
                    {
                        jobMessage.GeneralSettingModel = SerializerHelper.SerializeByDataContractSerializer(generalSetting);
                        jobMessage.TimeFormat = DateTimeUtil.GetAllStaticTimeZones().Where(x => x.Id == GeneralSettingConfig.GetTimeZoneInforById(generalSetting.TimeZoneId).Id).FirstOrDefault()?.DisplayName;
                    }
                }
                else
                {
                    Logger.Warn("Invalid job type, type:{0}.", subJob.JobType);
                }
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred while getting onpremise enforce rule action job message, error:{0}.", e.ToString());
            }
            return SerializerHelper.SerializeByDataContractSerializer(jobMessage);
        }

        private async Task<GeneralSettingModel> GetGeneralSettingModelAsync()
        {
            var timeSetting = await GeneralSettingService.GetGeneralSettingAsync();
            return timeSetting;
        }

        private List<string> OnpremiseSPBuildBreakTreeNode(RMSPTreeNode tree)
        {
            List<string> breakNodeUrls = new List<string>();
            var parentId = ScheduleService.GetProfileId(tree) + "|";
            var treeNodes = RMScheduleDao.GetDisposalBreakNodes(parentId);
            foreach (var item in treeNodes)
            {
                var node = JsonConvert.DeserializeObject<RMSPTreeNode>(item);
                if (node.Level == (int)NodeLevel.WebApplication)
                {
                    continue;
                }
                string url = EncodeUtil.EncryptBySHA1(node.FullPath.ToLowerInvariant());
                if (!breakNodeUrls.Contains(url))
                {
                    breakNodeUrls.Add(url);
                }
            }
            return breakNodeUrls;
        }

        private List<string> OnpremiseSPBuildRunningJobNode(RMSPTreeNode tree, JobType type, string currentJobId)
        {
            List<string> runningJobNodeUrls = new List<string>();
            try
            {
                List<JobType> jobTypes = new List<JobType>();
                //if (type == JobType.FSDataSynchronization || type == JobType.FSDataSynchronizationSchedule)
                //{
                //    jobTypes.Add(JobType.FSDataSynchronization);
                //    jobTypes.Add(JobType.FSDataSynchronizationSchedule);
                //}
                if (type == JobType.SPOnPremEnforceRuleAction || type == JobType.SPOnPremEnforceRuleActionSchedule)
                {
                    jobTypes.Add(JobType.SPOnPremEnforceRuleAction);
                    jobTypes.Add(JobType.SPOnPremEnforceRuleActionSchedule);
                }

                var subJobs = SubJobDao.GetRunningAgentJob(jobTypes)
                            .Where(j => j.String1.StartsWith(tree.FullPath) && !j.Id.Equals(currentJobId)).OrderByDescending(j => j.String1).ToList();
                foreach (var subJob in subJobs)
                {
                    var context = SubJobDao.GetSubJob(subJob.Id, true)?.JobContext;
                    if (context != null)
                    {
                        RMFSTreeNode node = SerializerHelper.DeserializeByDataContractSerializer<List<RMFSTreeNode>>(context.Settings).First();
                        string url = EncodeUtil.EncryptBySHA1(node.FullPath.ToLowerInvariant());
                        if (!runningJobNodeUrls.Contains(url))
                        {
                            runningJobNodeUrls.Add(url);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Warn("An error occurred while getting OnpremiseSPBuildRunningJobNode. Error:{0}", e.ToString());
            }
            return runningJobNodeUrls;
        }

        private async Task<Dictionary<Guid, List<Guid>>> GetTermRuleMappingAsync()
        {
            Dictionary<Guid, List<Guid>> mapping = new Dictionary<Guid, List<Guid>>();

            Dictionary<int, Guid> termIdUniqueIdMapping = TaxonomyService.GetAllTermsForce().ToDictionary(t => t.Id, t => t.UniqueId);

            Dictionary<int, List<Guid>> termRuleMapping = TaxonomyService.GetTermRuleMapping();


            ITermSetMembershipDao membershipDao = new TermSetMembershipDao();
            Dictionary<int, List<int>> memberships = (await membershipDao.FindListWithColumnsAsync(c => new { c.TermId, c.ParentTermId }, e => !e.IsRemoved))
                .GroupBy(t => t.ParentTermId, v => v.TermId)
                .ToDictionary(t => t.Key, v => v.ToList());

            foreach (var pId in memberships.Keys.OrderBy(k => k))
            {
                if (termRuleMapping.ContainsKey(pId))
                {
                    memberships[pId].ForEach(cId =>
                    {
                        if (!termRuleMapping.ContainsKey(cId))
                        {
                            termRuleMapping[cId] = termRuleMapping[pId];
                        }
                    });
                }
            }
            foreach (var termId in termRuleMapping.Keys)
            {
                if (termIdUniqueIdMapping.ContainsKey(termId))
                {
                    Guid termGuid = termIdUniqueIdMapping[termId];
                    mapping[termGuid] = termRuleMapping[termId];
                }
            }
            return mapping;
        }

        public RAReturnMessage RunOnpremiseEnforceRuleActionJob(RMSPTreeNode selectedTree, JobRunBy jobRunBy)
        {
            Logger.Debug("Start OnPremise SP Enforce Rule Action Job.");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            //selectedTree is null start by Timer Page run now;
            //selectedTree is not null start by Content Repository Management;
            if (selectedTree != null)
            {
                //if (!IsExistCanRunJobNodesForDisposal(selectedTree))
                //{
                //    msg.MessageType = RAMessageType.Failed;
                //    msg.ErrorMessage = I18NEntity.GetString("RM_JM_OnPremise_EnforceRuleAction_NoSC");//RM_JM_FS_Disposal_NoSC
                //    return msg;
                //}
            }
            if (selectedTree != null)
            {
                RMSPTreeNode siteCollectionNode = this.GetSiteCollectionNode(selectedTree);
                if (this.CheckParentNodeDisable(selectedTree, siteCollectionNode == null ? Guid.Empty.ToString() : siteCollectionNode.SPObjectId))
                {
                    msg.MessageType = RAMessageType.Failed;
                    msg.FaildType = RAFailedType.DisableRecordsManagement;
                    msg.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_RunJob_Failed");
                    return msg;
                }
            }
            if (TermRuleInfos.GetTermWithRule().Count == 0)
            {
                Logger.Error(I18NEntity.GetString("RM_JS_DAM_Physical_RunJob_Failed_NoRules"));
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_RunJob_Failed_NoRules");
                return msg;
            }
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.SPOnPremEnforceRuleAction,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = selectedTree == null ? null : SerializerHelper.SerializeByDataContractSerializer(selectedTree)
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                Logger.Error("An error occurred while OnPremise SP Enforce Rule Action Job,ERROR:{0}.", ex.ToString());
            }
            return msg;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.DisposalActivityManagement, Action = AuditAction.RunSPOnPremDisposalJob, 
            AfterHandler = typeof(DisposalActivityManagementAfterAuditHandler))]
        public Task<string> RealRunOnpremiseEnforceRuleActionJobAsync(string jobRunByUser, JobRunBy jobRunBy, string param)
        {
            JobType jobType = JobType.SPOnPremEnforceRuleAction;
            if (string.IsNullOrEmpty(param))
            {
                Logger.Error("Param is null when run OnPremise SP Enforce Rule Action Job.");
                throw new Exception("Param is null when run OnPremise SP Enforce Rule Action Job.");
            }
            else
            {
                RMSPTreeNode selectedNode = SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(param);
                return RunOnpremiseEnforceRuleActionJobBySelectdNodeAsync(jobRunBy, jobRunByUser, jobType, selectedNode);
            }
        }
        public Task<string> RealRunOnpremiseEnforceRuleActionJobForApprovalAsync(string jobRunByUser, JobRunBy jobRunBy)
        {
            return RunOnpremiseEnforceRuleActionJobBySelectdNodeForApprovalAsync(jobRunBy, jobRunByUser, JobType.SPOnPremEnforceRuleAction);
        }
        /// <summary>
        /// OnPremise SP Enforce Rule Action Job目前不支持Control Panel->Schedule Settings设置整体的Schedule/整体的Run Now，因此不需要考虑多Group，多Farm的情况。只需要考虑最高Group节点Run Job即可。
        /// </summary>
        /// <param name="jobRunByUser"></param>
        /// <param name="jobType"></param>
        /// <param name="selectedNode"></param>
        /// <returns></returns>
        public async Task<string> RunOnpremiseEnforceRuleActionJobBySelectdNodeAsync(JobRunBy jobRunBy, string jobRunByUser, JobType jobType, RMSPTreeNode selectedNode)
        {
            string jobId = string.Empty;
            List<RMSPTreeNode> availableSites = new List<RMSPTreeNode>();
            if (selectedNode.Level == (int)NodeLevel.WebApplication)
            {
                SPTreeMessage message = new SPTreeMessage()
                {
                    Node = RMDtoConverter.ConvertRMTree2SPTree(selectedNode)
                };
                var breakTreeNodeUrls = OnpremiseSPBuildBreakTreeNode(selectedNode);
                List<RMSPTreeNode> sites = (await AvePoint.RA.RACommonUtility.SharePointOnPrem.SharePointOnPremClient.BrowseAsync(message)).NodeList.ConvertAll(n => RMDtoConverter.ConvertSPTree2RMTree(n));
                var totalSiteCount = sites.Count;
                Logger.Info("OnpremiseEnforceRuleAction Group:{0} site collection count is {1}.", selectedNode.Name, sites.Count);
                if (sites.Count > 0)
                {
                    foreach (RMSPTreeNode siteNode in sites)
                    {
                        if (IsBreakInheritNode(breakTreeNodeUrls, siteNode.FullPath))
                        {
                            Logger.Info("Current site IsBreakInheritNode {0}.", siteNode.FullPath);
                        }
                        else
                        {
                            siteNode.ParentId = message.Node.ID;
                            availableSites.Add(siteNode);
                        }
                    }
                }
            }
            else
            {
                availableSites.Add(selectedNode);
            }
            Logger.Info("OnpremiseEnforceRuleAction Group:{0} available site collection count is {1}.", selectedNode.Name, availableSites.Count);
            //每个Agent最多运行当前类型的exe的数量，默认值2.不会check当前机器有多少exe，只是控制当前job。如果有多个节点在同时运行job，机器可能会出现N*2个exe.
            //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            var fullPah = selectedNode.Level == (int)NodeLevel.WebApplication ? selectedNode.Name : selectedNode.FullPath;
            if (fullPah.StartsWith("/") && selectedNode.Level != (int)NodeLevel.WebApplication)
            {
                var containerFullPath = GetSPContainerFullName(selectedNode);
                fullPah = containerFullPath + fullPah.Substring(1);
            }
            jobId = CreateOnpremiseEnforceRuleActionJob(jobRunBy, jobRunByUser, selectedNode.Id, null, fullPah);
            if (HasRunningJobOnNode(jobType, selectedNode, jobId))
            {
                JobMonitorService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_FSDisposal_JobSkip");
                return jobId;
            }
            if(!availableSites.Any())
            {
                Logger.Error("availableSite is null.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "");
                return jobId;
            }
            var parallelSubJobCount = subJobCountInConfigFile * await HybridSharePointWorkerService.GetAgentCountAsync(availableSites[0].FarmId);
            if (parallelSubJobCount == 0)
            {
                Logger.Error("No available agent server.Set main job failed.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_OnPremiseEnforceRuleActionNoAvailableAgent");
                return jobId;
            }
            SeperateSubJobForOnpremiseEnforceRuleAction(availableSites, jobId, jobType, parallelSubJobCount, selectedNode.Id);
            return jobId;
        }
        public async Task<string> RunOnpremiseEnforceRuleActionJobBySelectdNodeForApprovalAsync(JobRunBy jobRunBy, string jobRunByUser, JobType jobType)
        {
            try
            {
                string jobId = string.Empty;
                RMSPTreeNode selectedNode = new RMSPTreeNode();
               
                var farmNode = RMSPTreeService.LoadFarm()[0];
                var children = await SharePointOnPremBrowseService.BrowseTreeAsync(farmNode, true);
                foreach (var node in children)
                {
                    List<RMSPTreeNode> availableSites = new List<RMSPTreeNode>();
                    selectedNode = await RMSPSettingsService.LoadSampleNodeSettingsAsync(node);
                    SPTreeMessage message = new SPTreeMessage()
                    {
                        Node = RMDtoConverter.ConvertRMTree2SPTree(selectedNode)
                    };
                    List<RMSPTreeNode> sites = (await AvePoint.RA.RACommonUtility.SharePointOnPrem.SharePointOnPremClient.BrowseAsync(message)).NodeList.ConvertAll(n => RMDtoConverter.ConvertSPTree2RMTree(n));
                    var totalSiteCount = sites.Count;
                    Logger.Info("OnpremiseEnforceRuleAction Group:{0} site collection count is {1}.", selectedNode.Name, sites.Count);
                    if (sites.Count > 0)
                    {
                        foreach (RMSPTreeNode siteNode in sites)
                        {
                            bool exsitApprovalData = explorerDao.Exist(r => r.ManualApprovedStatus == (int)Contract.SOApproveDBStatus.Approved && r.ManualSiteUrl.Equals(siteNode.FullPath,StringComparison.OrdinalIgnoreCase));
                            Logger.Info($"OnpremiseEnforceRuleAction Group site full path is :{siteNode.FullPath}");
                            if (exsitApprovalData)
                            {
                                siteNode.ParentId = message.Node.ID;
                                availableSites.Add(siteNode);
                            }
                        }
                        Logger.Info("OnpremiseEnforceRuleAction Group:{0} available site collection count is {1}.", selectedNode.Name, availableSites.Count);
                        //每个Agent最多运行当前类型的exe的数量，默认值2.不会check当前机器有多少exe，只是控制当前job。如果有多个节点在同时运行job，机器可能会出现N*2个exe.
                        //int subJobCountInConfigFile = RMGlobalConfiguration.AppConfig.GetSubJobCount(new Guid(TenantLocalValue.LogonGroupId), (int)jobType);
                        int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);

                        if (HasRunningJobOnNode(jobType, selectedNode, jobId))
                        {
                            Logger.Error("has running job for thie node");
                            continue;
                        }
                        if (!availableSites.Any())
                        {
                            Logger.Error("availableSite is null.");
                            continue;
                        }
                        var parallelSubJobCount = subJobCountInConfigFile;// * await HybridSharePointWorkerService.GetAgentCountAsync(availableSites[0].FarmId);
                        if (parallelSubJobCount == 0)
                        {
                            Logger.Error("No available agent server.Set main job failed.");
                            continue;
                        }
                        jobId = CreateOnpremiseEnforceRuleActionJob(jobRunBy, jobRunByUser, jobType.ToString(), null, node.Name);
                        SeperateSubJobForOnpremiseEnforceRuleAction(availableSites, jobId, jobType, parallelSubJobCount, selectedNode.Id);
                    }
                }
                return jobId;

            }
            catch (Exception e)
            {
                Logger.Error("An error occurred while run OnpremiseEnforceRuleActionJobBySelectdNodeForApprovalAsync. Error:{0}.", e.ToString());
                return string.Empty;
            }
        }
        private bool HasRunningJobOnNode(JobType type, RMSPTreeNode treeNode, string jobId)
        {
            try
            {
                List<JobType> jobTypes = new List<JobType>();
                if (type == JobType.SPOnPremEnforceRuleAction || type == JobType.SPOnPremEnforceRuleActionSchedule)
                {
                    jobTypes.Add(JobType.SPOnPremEnforceRuleAction);
                    jobTypes.Add(JobType.SPOnPremEnforceRuleActionSchedule);
                }

                switch (treeNode.Level)
                {
                    //group级别有job在运行，再次在group级别运行job，job会skip
                    case (int)NodeLevel.WebApplication:
                    case (int)NodeLevel.SiteCollection:
                    case (int)NodeLevel.Site:
                    case (int)NodeLevel.List:
                    case (int)NodeLevel.Folder:
                    default:
                        var groupJobs = JobMonitorService.GetRunningJobs(jobTypes, treeNode.Id).Where(j => !j.Id.Equals(jobId)).ToList();
                        if (groupJobs.Count > 0)
                        {
                            Logger.Debug("Has running job on group:{0}. Job ids:{1}.", treeNode.FullPath, string.Join(",", groupJobs.Select(x => x.Id).ToList()));
                        }
                        return groupJobs.Count > 0;
                        //connection级别运行job，检查connection上是否有正在运行的job
                        //var connectionJobs = SubJobDao.GetRunningAgentJob(jobTypes).Where(j => j.String1 == treeNode.Id).ToList();
                        //if (connectionJobs != null && connectionJobs.Count > 0)
                        //{
                        //    Logger.Debug("Has running job on current node:{0}. Job ids:{1}.", treeNode.FullPath, string.Join(",", connectionJobs));
                        //}
                        //return connectionJobs.Count > 0;
                }
            }
            catch (Exception e)
            {
                Logger.Warn("An error occurred while checking if has job running on node. Error:{0}.", e.ToString());
            }
            return false;
        }

        private bool IsBreakInheritNode(List<string> breakTreeNodeUrls, string url)
        {
            string sha1Url = EncodeUtil.EncryptBySHA1(url);
            if (breakTreeNodeUrls != null && breakTreeNodeUrls.Contains(sha1Url))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 1.Onpremise Disposal Job目前最大范围只能在Group Node Run Job(目前支持Group/SC/Site/List)，因此分发SubJob无需考虑多个Group的情况，无需分Farm，分Group处理.
        /// 2.分Job的时候会获取当前有多少可用的SP Agent(通过FarmID获取)，默认每个Agent最大SubJobCount为2，每个SubJob最大NodeCount为5。举例如下：
        /// 当前有两个SP Agent Server可用，那么当前Disposal Job最大运行的exe数量为SubJobCount2*AgentCount2=4个，最大处理SC数量为运行的exe数量4* 最大NodeCount5 = 20个.
        /// 超过这些SubJob的节点都会waiting，等之前的SubJob结束后会继续起后续未waiting的SubJob.
        /// </summary>
        private void SeperateSubJobForOnpremiseEnforceRuleAction(List<RMSPTreeNode> availableSites, string jobId, JobType jobType, int parallelSubJobCount, string scopeId)
        {
            List<RMSPTreeNode> tempList = new List<RMSPTreeNode>();
            Dictionary<int, List<RMSPTreeNode>> subJobNodeDic = new Dictionary<int, List<RMSPTreeNode>>();
            int count = 0;
            foreach (var site in availableSites)
            {
                tempList.Add(site);
                if (tempList.Count >= RMGlobalConfiguration.AppConfig.NodeCountInSubJob)//每个exe包含的RMSPTreeNode数.默认值5
                {
                    count++;
                    var temp = new List<RMSPTreeNode>();
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
            Logger.Info("OnpremiseEnforceRuleAction Sub job count for {0} is {1}.", jobId, count);
            //int subJobCount = availableSites.Count % RMGlobalConfiguration.AppConfig.NODE_COUNT_IN_SUB_JOB == 0 ? availableSites.Count / RMGlobalConfiguration.AppConfig.NODE_COUNT_IN_SUB_JOB : availableSites.Count / RMGlobalConfiguration.AppConfig.NODE_COUNT_IN_SUB_JOB + 1;
            //SubJobDao.UpdateSubJobCount(jobId, subJobCount);

            int currentSubjobIndex = 0;
            foreach (KeyValuePair<int, List<RMSPTreeNode>> pa in subJobNodeDic)
            {
                string subJobId = CreateSubJobOnpremiseEnforceRuleAction(jobId, currentSubjobIndex, jobType, count, pa.Value, currentSubjobIndex < parallelSubJobCount, scopeId);
                Logger.Debug("Create and queue OnpremiseEnforceRuleAction sub job {0}.", subJobId);
                if (currentSubjobIndex < parallelSubJobCount)
                {
                    HybridSharePointWorkerService.StartSPJob(new Hybrid.Contract.RecordsJobArgs()
                    {
                        JobId = subJobId,
                        JobType = AvePoint.Hybrid.Contract.JobType.SharePointOnPremEnforceRuleAction,
                        TenantId = TenantLocalValue.LogonGroupId,
                        FarmId = pa.Value[0].FarmId
                    });
                }
                currentSubjobIndex++;
            }
        }

        private string CreateSubJobOnpremiseEnforceRuleAction(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, List<RMSPTreeNode> tempList, bool sendNow, string scopeId, Dictionary<Guid, RMSharePointOnPremiseSetting> gruopSetingMap = null)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            string farmId = tempList[0].FarmId;
            var subJob = new RMSubJob() { Id = subJobId, FarmId = farmId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount, String1 = scopeId };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(tempList) };
            //if (gruopSetingMap != null)
            //{
            //    subJob.JobContext.Content = SerializerHelper.SerializeByDataContractSerializer(gruopSetingMap);
            //}
            SubJobDao.CreateJob(subJob);
            Logger.Info("Create OnpremiseEnforceRuleAction sub job {0} sucessfull, type {1}, weight {2}.", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }


        private string CreateOnpremiseEnforceRuleActionJob(JobRunBy runBy, string jobRunByUser, string scopeId, string containerId = null,string fullPath = null)
        {
            string jobId = string.Empty;
            if (runBy == JobRunBy.Control)
            {
                jobId = RMJobService.CreateJobWithScopeId(JobType.SPOnPremEnforceRuleAction, jobRunByUser, scopeId, containerId, fullPath);
                Logger.Info("Begin control SPOnPremEnforceRuleAction Job {0}.", jobId);
            }
            else if (runBy == JobRunBy.Schedule)
            {
                jobId = RMJobService.CreateJobWithScopeId(JobType.SPOnPremEnforceRuleAction, "RM_TS_RunSchedule", scopeId, containerId, fullPath);
                Logger.Info("Begin schedule SPOnPremEnforceRuleAction Job {0}.", jobId);
            }
            else
            {
                jobId = RMJobService.CreateJobWithScopeId(JobType.SPOnPremEnforceRuleAction, jobRunByUser, scopeId, containerId, fullPath);
                Logger.Info("Begin default SPOnPremEnforceRuleAction Job {0}.", jobId);
            }
            return jobId;
        }

        private Guid GetGroupIdForOnpremiseEnforceRuleActionJob(RMSPTreeNode node)
        {
            if (node.Level != (int)NodeLevel.WebApplication)
            {
                if (node.Level == (int)NodeLevel.SiteCollection && !string.IsNullOrEmpty(node.ParentId))
                {
                    return new Guid(node.ParentId);
                }
                else
                {
                    return GetGroupIdForOnpremiseEnforceRuleActionJob(node.Parent);
                }
            }
            else
            {
                return new Guid(node.Id);
            }
        }
        #endregion

        public Task<bool> CheckHasAvailableAgentAsync()
        {
            return HybridBrowserService.CheckHasAvailableAgentAsync(Hybrid.Contract.Object.SourceType.SharePoint);
        }

        private RMSharePointOnPremiseSetting CloneSetting(RMSharePointOnPremiseSetting setting)
        {
            string xml = SerializerHelper.SerializeByDataContractSerializer(setting);
            RMSharePointOnPremiseSetting result = SerializerHelper.DeserializeByDataContractSerializer<RMSharePointOnPremiseSetting>(xml);
            return result;
        }

        public string GetUniqueIdSettingJobMessage(string jobId)
        {
            string message = string.Empty;
            try
            {
                Logger.Debug("Start to get unique id setting job message. Job Id:" + jobId);
                var subJob = SubJobDao.GetSubJob(jobId, true);
                if (subJob.JobType == (int)JobType.SPOnPremUniqueIDSettingFullSchedule || subJob.JobType == (int)JobType.SPOnPremUniqueIDSettingIncrementalSchedule)
                {
                    var mainJob = JobMonitorDao.GetJob(subJob.ParentId);
                    var nodes = SerializerHelper.DeserializeByDataContractSerializer<List<RMSPTreeNode>>(subJob.JobContext.Settings);
                    UniqueIdSettingJobMessage jobMessage = new UniqueIdSettingJobMessage
                    {
                        TreeNodes = AssembleTreeNodes(nodes),
                        MainJobStartTime = mainJob.StartTime,
                        SiteInformationDic = AssembleSiteInforDic(nodes),
                        CurUniqueIdSetting = Convert2GRMUniqueIdSetting(UniqueIdSettingDao.LoadingUniqueIdSetting()),
                        SiteGroupEnableUniqueIdDic = AssembleSiteGroupEnableUniqueIdDic(nodes),
                        WebEnableSettings = AssembleWebEnableSettings(nodes),
                        SiteEnableSettings = AssembleSiteEnableSettings(nodes)
                    };
                    message = SerializerHelper.SerializeByDataContractSerializer(jobMessage);
                }
                else
                {
                    Logger.Warn("Invalid job type, type:" + subJob.JobType);
                }
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred while getting unique id setting job message, error:{0}", e.ToString());
            }
            return message;
        }

        private Global.GRMUniqueIdSetting Convert2GRMUniqueIdSetting(RMUniqueIdSetting setting)
        {
            var gSetting = new Global.GRMUniqueIdSetting();
            if (setting != null)
            {
                gSetting.Name = setting.Name;
                gSetting.Prefix = setting.Prefix;
                gSetting.IsActived = setting.IsActived;
                gSetting.Id = setting.Id;
                gSetting.OverrideSPPrefix = setting.OverrideSPPrefix;
            }
            return gSetting;
        }

        private Dictionary<Guid, bool> AssembleSiteGroupEnableUniqueIdDic(List<RMSPTreeNode> nodes)
        {
            Dictionary<Guid, bool> dic = new Dictionary<Guid, bool>();
            foreach (var node in nodes)
            {
                var groupId = new Guid(node.Parent.SPObjectId);
                var groupLevelSetting = SharePointOnPremiseSettingDao.GetGroupLevelSetting(node.Parent.FullPath, groupId);
                if (groupLevelSetting != null && !dic.ContainsKey(groupId))
                {
                    dic.Add(groupId, groupLevelSetting.IsShowUniqueId);
                }
            }
            return dic;
        }

        private List<WebEnableSetting> AssembleWebEnableSettings(List<RMSPTreeNode> nodes)
        {
            List<WebEnableSetting> webSettings = new List<WebEnableSetting>();
            foreach (var site in nodes)
            {
                var groupId = new Guid(site.Parent.SPObjectId);
                var siteId = new Guid(site.SPObjectId);
                var mapping = SharePointOnPremiseSettingDao.GetWebEnableManagementSettingInfo(groupId, siteId);
                foreach (var item in mapping)
                {
                    webSettings.Add(new WebEnableSetting
                    {
                        GroupId = groupId,
                        SiteId = siteId,
                        WebId = item.Key,
                        EnableRecordsManagement = item.Value
                    });
                }
            }
            return webSettings;
        }

        private List<SiteEnableSetting> AssembleSiteEnableSettings(List<RMSPTreeNode> nodes)
        {
            List<SiteEnableSetting> siteSettings = new List<SiteEnableSetting>();
            foreach (var site in nodes)
            {
                var siteId = new Guid(site.SPObjectId);
                var groupId = new Guid(site.Parent.SPObjectId);
                var setting = SharePointOnPremiseSettingDao.GetSiteLevelSetting(site.FullPath, siteId);
                if (setting == null)
                {
                    setting = SharePointOnPremiseSettingDao.GetGroupLevelSetting(site.Parent.FullPath, groupId);
                }
                siteSettings.Add(new SiteEnableSetting
                {
                    GroupId = groupId,
                    SiteId = siteId,
                    EnableRecordsManagement = setting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable
                });
            }
            return siteSettings;
        }

        private Dictionary<string, SiteInfo> AssembleSiteInforDic(List<RMSPTreeNode> nodes)
        {
            Dictionary<string, SiteInfo> siteInfos = new Dictionary<string, SiteInfo>();
            foreach (var node in nodes)
            {
                var siteUrl = node.FullPath;
                var lastScanTime = RMNodeFlagDao.GetCollectionTime((int)NodeFlagType.UniqueId, new Guid(node.Parent.SPObjectId), new Guid(node.SPObjectId));
                siteInfos.Add(siteUrl, new SiteInfo()
                {
                    LastScanTime = lastScanTime
                });
            }
            return siteInfos;
        }

        #region global search
        public string GetGlobalSearchActionJobMessage(string jobId)
        {
            string message = string.Empty;
            try
            {
                Logger.Debug("Start to get global search action job message. Job Id:" + jobId);
                var subJob = SubJobDao.GetSubJob(jobId, true);
                if (subJob.JobType == (int)JobType.GlobalSearchAction)
                {

                    var dto = SerializerHelper.DeserializeByDataContractSerializer<GlobalSearchActionDto>(subJob.JobContext.Content);
                    GlobalSearchActionJobMessage jobMessage = new GlobalSearchActionJobMessage();
                    jobMessage.JobId = jobId;
                    jobMessage.Action = (AvePoint.RA.Contract.Global.Explorer.GlobalSearchAction)dto.Action;
                    jobMessage.ActionExtension = AssembleActionExtension(dto.Action, dto.ActionExtension);
                    message = SerializerHelper.SerializeByDataContractSerializer(jobMessage);
                }
                else
                {
                    Logger.Warn("Invalid job type, type:" + subJob.JobType);
                }
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred while getting global search action job message, error:{0}", e.ToString());
            }
            return message;
        }

        private object AssembleActionExtension(GlobalSearchAction action, object extension)
        {
            object realExtension = null;
            switch (action)
            {
                case GlobalSearchAction.DeclareRecords:
                case GlobalSearchAction.UnDeclareRecords:
                    realExtension = extension;
                    break;
                case GlobalSearchAction.Reclassify:
                    var changeTermDto = SerializerHelper.DeserializeByDataContractSerializer<AvePoint.RA.Contract.Object.RealTime.ChangeTermOption>(extension.ToString());
                    ChangeTermOption option = new ChangeTermOption()
                    {
                        OverWriteSubFiles = changeTermDto.OverWriteSubFiles,
                        SourceSPOnPremRecordIds = changeTermDto.SourceSPOnPremRecordIds,
                        TargetTermId = changeTermDto.TargetTermId,
                        TargetTermName = changeTermDto.TargetTermName,
                        TargetTermUniqueId = changeTermDto.TargetTermUniqueId,
                        LogonUser = changeTermDto.LogonUser,
                        Comment = changeTermDto.Comment,
                    };
                    realExtension = SerializerHelper.SerializeByDataContractSerializer(option);
                    break;
            }

            return realExtension;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.DeclareSPOOnPreAsRecord, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<(Contract.Object.RealTime.RecordsReturnMessage,string)> SPOnPremDeclaredItemRecordsAsync(List<Guid> ids, bool isDeclared)
        {
            string declaredTempJobId;
            Contract.Object.RealTime.RecordsReturnMessage message = new Contract.Object.RealTime.RecordsReturnMessage() { ResultType = Contract.Object.RealTime.ResultType.Success };
            declaredTempJobId = "UD" + Guid.NewGuid().ToString();
            AvePoint.RA.Contract.Global.JobMessage.OnPremRealtimeJobMessage jobMessage = new AvePoint.RA.Contract.Global.JobMessage.OnPremRealtimeJobMessage();
            jobMessage.JobId = declaredTempJobId;
            jobMessage.Action = isDeclared ? AvePoint.RA.Contract.Global.JobMessage.RealTimeAction.Declare : AvePoint.RA.Contract.Global.JobMessage.RealTimeAction.UnDeclare;

            jobMessage.DeclareIds = ids;
            jobMessage.DeclaredBy = WebUtil.LogOnUserName;
            try
            {
                await SendRealtimeJobToAgentAsync(jobMessage);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
                message.ResultType = Contract.Object.RealTime.ResultType.Failed;
            }
            return (message, declaredTempJobId);
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.UndeclareSPOOnPreAsRecord, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<(Contract.Object.RealTime.RecordsReturnMessage, string)> SPOnPremUnDeclaredItemRecordsAsync(List<Guid> ids, bool isDeclared)
        {
            string declaredTempJobId;
            Contract.Object.RealTime.RecordsReturnMessage message = new Contract.Object.RealTime.RecordsReturnMessage() { ResultType = Contract.Object.RealTime.ResultType.Success };
            declaredTempJobId = "UD" + Guid.NewGuid().ToString();
            AvePoint.RA.Contract.Global.JobMessage.OnPremRealtimeJobMessage jobMessage = new AvePoint.RA.Contract.Global.JobMessage.OnPremRealtimeJobMessage();
            jobMessage.JobId = declaredTempJobId;
            jobMessage.Action = isDeclared ? AvePoint.RA.Contract.Global.JobMessage.RealTimeAction.Declare : AvePoint.RA.Contract.Global.JobMessage.RealTimeAction.UnDeclare;

            jobMessage.DeclareIds = ids;
            jobMessage.DeclaredBy = WebUtil.LogOnUserName;
            try
            {
                await SendRealtimeJobToAgentAsync(jobMessage);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
                message.ResultType = Contract.Object.RealTime.ResultType.Failed;
            }
            return (message,declaredTempJobId);
        }

        private async System.Threading.Tasks.Task SendRealtimeJobToAgentAsync(AvePoint.RA.Contract.Global.JobMessage.OnPremRealtimeJobMessage jobMessage)
        {
            var batchId = Guid.NewGuid();
            var farmId = jobMessage.Action == Contract.Global.JobMessage.RealTimeAction.ChangeTerm ?
                GetFarmId(jobMessage.ChangeTermOption.SourceSPOnPremRecordIds.FirstOrDefault()) :
                GetFarmId(jobMessage.DeclareIds.FirstOrDefault());
            Logger.Info("Begin get proxy");
            var proxy = RetryPolicy.ExecuteAction(() => RASignalRAgentProxy.GetProxy());
            Logger.Info("End get proxy");

            var agents = await SignalRService.GetAgentsByFarmIdAsync(TenantLocalValue.LogonGroupId, farmId);
            Logger.Info($"Farm: [{farmId}] all available agent count: [{agents.Count}]");
            var agent = agents.FirstOrDefault();
            Logger.Info($"Farm: [{farmId}] used agent: [{agent?.AgentId}].");


            var args = new SharePointOnPremRealtimeJobArgs
            {
                BatchId = batchId.ToString(),
                Message = SerializerHelper.SerializeByDataContractSerializer(jobMessage)
            };

            var result = await proxy.InvokeOneAgentAysnc<SharePointOnPremRealtimeJobExecute, SharePointOnPremRealtimeJobArgs, SharePointOnPremRealtimeJobResult>(agent, new SharePointOnPremRealtimeJobExecute { MethodArgs = args });

            if (result.Result == SharePointOnPremRealtimeJobResultEnum.Failed)
            {
                Logger.Error($"Process sharepoint on-prem realtime job failed. Error: {result.Message}");
            }
        }

        private string GetFarmId(Guid id)
        {
            var record = ExplorerDao.GetRecordByIds(new List<Guid>() { id });
            var siteId = record.FirstOrDefault()?.AveSiteId;
            var site = AvePoint.RA.RACommonUtility.SharePointOnPrem.SharePointOnPremClient.GetLocalSiteCollectionById(siteId);
            return site.FarmId;
        }


        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.Explorer, Action = AuditAction.ChangeTerm, BeforeHandler = typeof(ExplorerBeforeAuditHandler), AfterHandler = typeof(ExplorerAfterAuditHandler))]
        public async Task<(Contract.Object.RealTime.RecordsReturnMessage, string)> UpdateOnPremTermsAsync(AvePoint.RA.Contract.Object.RealTime.ChangeTermOption changeTermInfo)
        {
            string updateTermTempJobId;
            Contract.Object.RealTime.RecordsReturnMessage message = new Contract.Object.RealTime.RecordsReturnMessage() { ResultType = Contract.Object.RealTime.ResultType.Success };
            updateTermTempJobId = "UT" + Guid.NewGuid().ToString();
            AvePoint.RA.Contract.Global.JobMessage.OnPremRealtimeJobMessage jobMessage = new AvePoint.RA.Contract.Global.JobMessage.OnPremRealtimeJobMessage();
            jobMessage.JobId = updateTermTempJobId;
            jobMessage.Action = AvePoint.RA.Contract.Global.JobMessage.RealTimeAction.ChangeTerm;
            jobMessage.ChangeTermOption = new AvePoint.RA.Contract.Global.JobMessage.ChangeTermOption()
            {
                SourceSPOnPremRecordIds = changeTermInfo.SourceSPOnPremRecordIds,
                TargetTermId = changeTermInfo.TargetTermId,
                TargetTermName = changeTermInfo.TargetTermName,
                TargetTermUniqueId = changeTermInfo.TargetTermUniqueId,
                OverWriteSubFiles = changeTermInfo.OverWriteSubFiles,
                LogonUser = WebUtil.LogOnUserName,
                Comment = changeTermInfo.Comment
            };
            try
            {
                await SendRealtimeJobToAgentAsync(jobMessage);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
                message.ResultType = Contract.Object.RealTime.ResultType.Failed;
            }
            return (message, updateTermTempJobId);
        }
        #endregion
    }
}
