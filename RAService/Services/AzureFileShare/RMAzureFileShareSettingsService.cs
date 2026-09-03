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
using AvePoint.Hybrid.Utility.Cryptography;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.SourceTreeQuery.Model;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.AzureFileShare.AuditHandler;
using AvePoint.RA.Service.Services.RMSharePointSettings;
using AvePoint.RA.Service.Services.SourceTreeQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.AzureFileShare
{
    [Audit]
    public class RMAzureFileShareSettingsService : BaseContentRepositorySettingsService, IRMAzureFileSettingsService
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMAzureFileShareSettingsService));

        #region dao&service

        private static readonly AzureFileShareTreeQuerier Querier = new AzureFileShareTreeQuerier();

        private IRMKeyValueDao  RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        public IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();

        private IJobQueueService mJobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();

        private IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();

        private IAzureFileShareSettingDao AzureFileShareSettingDao => PlatformWindsorManager.GetService<IAzureFileShareSettingDao>();

        #endregion

        public bool Has(Guid scopeId)
        {
            return AzureFileShareSettingDao.Exist(item => item.ScopeId == scopeId);
        }

        public async System.Threading.Tasks.Task ResetSyncSettingAsync(Guid scopeId)
        {
            if(AzureFileShareSettingDao.TryGet(scopeId, out var settingInfo))
            {
                settingInfo.NeedCheckDefaultValue = false;
                settingInfo.RunAutoFullJob = false;
                await AzureFileShareSettingDao.UpdateAsync(settingInfo);
            }
        }

        public async Task<List<AzureFileSettingDto>> GetAllSettingInfoesAsync()
        {
            try
            {
                var res = new List<AzureFileSettingDto>();
                var settings = AzureFileShareSettingDao.FindAll();
                foreach (var setting in settings)
                {
                    var settingInfo = new AzureFileSettingDto();
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
                    settingInfo.SelectedNode = SerializerHelper.DeserializeByDataContractSerializer<AzureFileShareTreeNode>(setting.NodeInfo);
                    SetAzureAutoTermStatus(settingInfo.AutoClassificationRules);
                    await ConvertClassificationRuleTimeZoneAsync(settingInfo.AutoClassificationRules);
                    ConvertClassificationRuleAndOrExpression(settingInfo.AutoClassificationRules);
                    res.Add(settingInfo);
                }
                return res;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while get all setting infoes. Error: {e}");
                return new List<AzureFileSettingDto>();
            }
        }

        public async Task<(bool, AzureFileSettingDto) > TryGetSettingInfoAsync(Guid scopeId)
        {
            AzureFileSettingDto settingInfo = new AzureFileSettingDto();
            try
            {
                if (!AzureFileShareSettingDao.TryGet(scopeId, out var setting))
                {
                    return (false, settingInfo) ;
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
                SetAzureAutoTermStatus(settingInfo.AutoClassificationRules);
                await ConvertClassificationRuleTimeZoneAsync(settingInfo.AutoClassificationRules);
                ConvertClassificationRuleAndOrExpression(settingInfo.AutoClassificationRules);
                return (true, settingInfo);
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while try get setting info by [{scopeId}]. Error: {e}");
                return (false, settingInfo);
            }
        }

        public async Task<AzureFileSettingDto> LoadNodeSettingAsync(AzureFileShareTreeNode sNode)
        {
            var dto = new AzureFileSettingDto();
            if (Guid.TryParse(GetConnectionGroupId(sNode), out Guid connectionGroupId))
            {
                var GSetting = AzureFileShareSettingDao.LoadSetting(connectionGroupId, connectionGroupId);
                if (GSetting != null)
                {

                    var termScope = TermDao.GetRMTermByGuId(GSetting.TermId);
                    RMTermSet termSet = null;
                    if (GSetting.TermId == Guid.Empty)
                    {
                        termSet = TermDao.GetRMTermSetByGuid(GSetting.TermSetId);
                    }
                    sNode.IconStatus = IconStatus.Inhert;
                    var termDefaultValue = TermDao.GetRMTermByGuId(GSetting.DefaultTermId);
                    dto.TermSetId = GSetting.TermSetId;
                    dto.TermSetName = GSetting.TermSetName;
                    dto.TermId = GSetting.TermId;
                    dto.TermName = GSetting.TermName;
                    dto.IsTermRemoved = (termScope == null ? termSet?.IsRemoved : termScope?.IsRemoved) ?? false;
                    dto.TermScopeFullPath = GSetting.TermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(GSetting.TermId) : TermDao.GetTermSetNamesPathByTermSetId(GSetting.TermSetId);
                    dto.DefaultTermId = GSetting.DefaultTermId;
                    dto.DefaultTermName = termDefaultValue == null ? GSetting.DefaultTermName : termDefaultValue.Name;
                    dto.DefaultTermFullPath = GSetting.DefaultTermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(GSetting.DefaultTermId) : "";
                    dto.IsDefaultTermRemoved = termDefaultValue == null ? false : termDefaultValue.IsRemoved;
                    dto.IsDefaultTermDeprecated = termDefaultValue == null ? false : termDefaultValue.IsDeprecated || TermDao.IsExpiredTerm(termDefaultValue.Id);
                    dto.NeedCheckDefaultValue = GSetting.NeedCheckDefaultValue;
                    dto.ApplyExistType = GSetting.ApplyExistType;
                    if (GSetting.NeedCheckDefaultValue && GSetting.ApplyExistType == (int)ApplyExistingTermType.None)
                    {
                        dto.ApplyExistType = (int)ApplyExistingTermType.SkipAndKeep;
                    }
                    dto.EMailToRecordOwner = GSetting.EMailToRecordOwner;
                    //dto.RecordOwner = RecordOwnerDao.GetRecordOwnerAccounts(GSetting.Id, RecordOwnerSettingType.AzureFileShare);
                    //dto.ProfileId = GSetting.IdPath;
                    dto.IsActive = GSetting.IsActive;
                    dto.DeployTermMethod = GSetting.TermSetId == Guid.Empty ? DeployTermMethod.NoDefaultTerm : (DeployTermMethod)GSetting.DeployTermMethod;
                    dto.AutoClassificationRules = GSetting.AutoClassificationRules == null ?
                        null : SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(GSetting.AutoClassificationRules);
                    SetAzureAutoTermStatus(dto.AutoClassificationRules);
                    await ConvertClassificationRuleTimeZoneAsync(dto.AutoClassificationRules);
                    ConvertClassificationRuleAndOrExpression(dto.AutoClassificationRules);
                    dto.RunAutoFullJob = GSetting.RunAutoFullJob;
                    dto.AutoJobOption = (AutoJobOption)GSetting.AutoJobOption == AutoJobOption.None ? AutoJobOption.SkipAndKeep : (AutoJobOption)GSetting.AutoJobOption;
                    dto.ApprovalType = (int)GSetting.ApprovalType;
                    dto.WorkflowReferenceId = GSetting.WorkflowReferenceId;
                }
                //reset IsCustomSetting property
                dto.IsCustomSetting = false;
                var afSetting = AzureFileShareSettingDao.LoadSetting(new Guid(sNode.Id), connectionGroupId);
                if (afSetting == null)
                {
                    if (sNode.Level == RMNodeLevel.AzureFileShareDirectory)
                    {
                        var parentNode = sNode.Parent;
                        afSetting = LoadParentAllSeting(parentNode, connectionGroupId);
                        dto.IsCustomSetting = false;
                    }
                }
                else
                {
                    sNode.IconStatus = IconStatus.Break;
                    if (sNode.Level != RMNodeLevel.AzureFileShareGroup)//Group Level 不能有CustomSetting，
                    {
                        dto.IsCustomSetting = true;
                    }
                }

                if (afSetting != null)
                {
                    var termScope = TermDao.GetRMTermByGuId(afSetting.TermId);
                    var defaultTerm = TermDao.GetRMTermByGuId(afSetting.DefaultTermId);
                    RMTermSet termSet = null;
                    if (afSetting.TermId == Guid.Empty)
                    {
                        termSet = TermDao.GetRMTermSetByGuid(afSetting.TermSetId);
                    }

                    dto.DefaultTermId = afSetting.DefaultTermId;
                    dto.DefaultTermName = defaultTerm == null ? afSetting.DefaultTermName : defaultTerm.Name;
                    dto.DefaultTermFullPath = afSetting.DefaultTermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(afSetting.DefaultTermId) : "";
                    dto.TermId = afSetting.TermId;
                    dto.TermName = termScope == null ? afSetting.TermName : termScope.Name;
                    dto.TermScopeFullPath = afSetting.TermId != Guid.Empty ? TermDao.GetTermNamesPathByTermId(afSetting.TermId) : TermDao.GetTermSetNamesPathByTermSetId(afSetting.TermSetId);
                    dto.TermSetId = afSetting.TermSetId;
                    dto.TermSetName = afSetting.TermSetName;
                    dto.IsTermRemoved = (termScope == null ? termSet?.IsRemoved : termScope?.IsRemoved) ?? false;
                    dto.IsDefaultTermRemoved = defaultTerm != null && defaultTerm.IsRemoved;
                    dto.IsTermDeprecated = termScope != null && (termScope.IsDeprecated || TermDao.IsExpiredTerm(termScope.Id));
                    dto.IsDefaultTermDeprecated = defaultTerm != null && (defaultTerm.IsDeprecated || TermDao.IsExpiredTerm(defaultTerm.Id));
                    dto.NeedCheckDefaultValue = afSetting.NeedCheckDefaultValue;
                    dto.ApplyExistType = afSetting.ApplyExistType;
                    if (afSetting.NeedCheckDefaultValue && afSetting.ApplyExistType == (int)Contract.TaxonomyModel.ApplyExistingTermType.None)
                    {
                        dto.ApplyExistType = (int)Contract.TaxonomyModel.ApplyExistingTermType.SkipAndKeep;
                    }

                    //dto.RecordOwner = RecordOwnerDao.GetRecordOwnerAccounts(afSetting.Id, RecordOwnerSettingType.AzureFileShare);
                    dto.EMailToRecordOwner = afSetting.EMailToRecordOwner;
                    //dto.ProfileId = afSetting.IdPath;
                    dto.IsActive = afSetting.IsActive;
                    dto.DeployTermMethod = afSetting.TermSetId == Guid.Empty ? DeployTermMethod.NoDefaultTerm : (DeployTermMethod)afSetting.DeployTermMethod;
                    dto.AutoClassificationRules = afSetting.AutoClassificationRules == null ?
                        null : SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(afSetting.AutoClassificationRules);
                    SetAzureAutoTermStatus(dto.AutoClassificationRules);
                    await ConvertClassificationRuleTimeZoneAsync(dto.AutoClassificationRules);
                    ConvertClassificationRuleAndOrExpression(dto.AutoClassificationRules);
                    dto.RunAutoFullJob = afSetting.RunAutoFullJob;
                    dto.AutoJobOption = (AutoJobOption)afSetting.AutoJobOption == AutoJobOption.None ? AutoJobOption.SkipAndKeep : (AutoJobOption)afSetting.AutoJobOption;
                    dto.ApprovalType = (int)afSetting.ApprovalType;
                    dto.WorkflowReferenceId = afSetting.WorkflowReferenceId;
                }

                //var profileId = ScheduleService.GetProfileId(sNode);
                //var disposeSchedule = ScheduleService.GetSchedule(profileId, ScheduleType.FSDisposalSchedule);
                //if (disposeSchedule != null)
                //{
                //    var simplifyZoneInfo = DateTimeUtil.GetSimplifyZoneInfo(disposeSchedule.TimeZoneId);
                //    disposeSchedule.StartTime = string.Format($"{disposeSchedule.StartTime} {simplifyZoneInfo}");
                //    disposeSchedule.EndTime = string.Format($"{disposeSchedule.EndTime} {simplifyZoneInfo}");

                //    sNode.DisposeScheduleInfo = disposeSchedule;
                //    sNode.DisposeScheduleInfo.Extentions = JsonConvert.DeserializeObject<RMSPTreeNode>(sNode.DisposeScheduleInfo.Extentions).SkipRemoveContentAndDestroyAction.ToString();
                //    sNode.IconStatus = IconStatus.Break;
                //}
                //else
                //{
                //    var ancestryDisposeSchedule = ScheduleService.GetAncestrySchedule(profileId, ScheduleType.FSDisposalSchedule);
                //    if (ancestryDisposeSchedule != null)
                //    {
                //        var simplifyZoneInfo = DateTimeUtil.GetSimplifyZoneInfo(ancestryDisposeSchedule.TimeZoneId);
                //        ancestryDisposeSchedule.StartTime = string.Format($"{ancestryDisposeSchedule.StartTime} {simplifyZoneInfo}");
                //        ancestryDisposeSchedule.EndTime = string.Format($"{ancestryDisposeSchedule.EndTime} {simplifyZoneInfo}");
                //        sNode.DisposeScheduleInfo = ancestryDisposeSchedule;
                //        sNode.DisposeScheduleInfo.Id = "1";//回显先祖的schedule给假ID，防止删除schedule将先祖的删掉
                //        sNode.DisposeScheduleInfo.Extentions = JsonConvert.DeserializeObject<RMSPTreeNode>(sNode.DisposeScheduleInfo.Extentions).SkipRemoveContentAndDestroyAction.ToString();
                //    }
                //    else
                //    {
                //        sNode.DisposeScheduleInfo = null;
                //    }
                //}
            }
            dto.SelectedNode = sNode;
            return dto;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.AzureFileSaveTermSetting, BeforeHandler = typeof(AzureFileShareServiceBeforeAuditHandler), AfterHandler = typeof(AzureFileShareServiceAfterAuditHandler))]
        public async System.Threading.Tasks.Task SaveNodeSettingAsync(AzureFileSettingDto dto)
        {
            AddFilterCretiaProperty(dto.AutoClassificationRules);
            if (Guid.TryParse(GetConnectionGroupId(dto?.SelectedNode), out Guid connectionGroupId))
            {
                await AzureFileShareSettingDao.SaveSettingAsync(dto, connectionGroupId);
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.AzureFileDeactiveSetting, BeforeHandler = typeof(AzureFileShareServiceBeforeAuditHandler), AfterHandler = typeof(AzureFileShareServiceAfterAuditHandler))]
        public async System.Threading.Tasks.Task SaveActiveSettingAsync(AzureFileSettingDto dto)
        {
            if (Guid.TryParse(GetConnectionGroupId(dto?.SelectedNode), out Guid connGroupId))
            {
                await AzureFileShareSettingDao.SaveSettingAsync(dto, connGroupId);
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.AzureFileInheritSetting, BeforeHandler = typeof(AzureFileShareServiceBeforeAuditHandler), AfterHandler = typeof(AzureFileShareServiceAfterAuditHandler))]
        public void InheritParentSetting(AzureFileShareTreeNode node)
        {
            try
            {
                logger.Info("Inherit Parent Settings");
                AzureFileShareSettingDao.DeleteAzureFileShareSetting(new Guid(node.Id), new Guid(node.ContainerId));
            }
            catch (Exception ex)
            {
                logger.Warn("Inherit Parent Setting to DB Error {0}", ex.ToString());
            }
        }

        public RAReturnMessage RunDataSyncJob(AzureFileShareTreeNode selectedTreeNode)
        {
            try
            {
                var dto = new JobQueueDto
                {
                    JobType = JobType.AzureFileShareDataSynchronisation,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = TenantLocalValue.LogonUserEmail,
                    Parameters = JsonConvert.SerializeObject(selectedTreeNode)
                };

                var jobId = mJobQueueService.AddToDBJobQueue(dto);
                logger.Info($"Succeed add data sync job [{jobId}] to job queue.");
                if (!string.IsNullOrEmpty(jobId))
                {
                    return new RAReturnMessage();
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while run data sync job. Error: {e}");
            }
            return new RAReturnMessage
            {
                MessageType = RAMessageType.Failed,
                FaildType = RAFailedType.None
            };
        }

        public void RunDataSyncScheduleJob()
        {
            try
            {
                var dto = new JobQueueDto
                {
                    JobType = JobType.AzureFileShareDataSynchronisationSchedule,
                    JobRunType = JobRunBy.Schedule,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = TenantLocalValue.LogonUserEmail,
                };
                var jobId = mJobQueueService.AddToDBJobQueue(dto);
                logger.Info($"Succeed add data sync schedule job [{jobId}] to job queue.");
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while run data sync schedule job. Error: {e}");
            }
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.AzureFileRunDataSyncJob, BeforeHandler = typeof(AzureFileShareServiceBeforeAuditHandler), AfterHandler = typeof(AzureFileShareServiceAfterAuditHandler))]
        public async Task<string> RealRunDataSyncJobAsync(string jobRunByUser, string selectedNodeJson)
        {
            var selectedNode = JsonConvert.DeserializeObject<AzureFileShareTreeNode>(selectedNodeJson);

            if (RMJobService.GetRunningJobsCount(JobType.AzureFileShareDataSynchronisationSchedule) > 0)
            {
                var skippedJobId = RMJobService.CreateJobWithScopeId(JobType.AzureFileShareDataSynchronisation, jobRunByUser, selectedNode.Id);
                RMJobService.UpdateJobStatus(skippedJobId, JobStatus.Skipped, "RM_FSDataSync_JobSkip");
                return skippedJobId;
            }

            var scopeIds = new List<string>();
            var tempSelectedNode = selectedNode;
            while (tempSelectedNode != null && tempSelectedNode.Level != RMNodeLevel.Root)
            {
                scopeIds.Add(tempSelectedNode.Id);
                tempSelectedNode = tempSelectedNode.Parent;
            }

            ArgumentCheck.NotNull(selectedNode, nameof(selectedNode));
            if (CheckCurrentNodeHasRunningSyncJob(scopeIds))
            {
                var skippedJobId = RMJobService.CreateJobWithScopeId(JobType.AzureFileShareDataSynchronisation, jobRunByUser, selectedNode.Id);
                RMJobService.UpdateJobStatus(skippedJobId, JobStatus.Skipped, "RM_FSDataSync_JobSkip");
                return skippedJobId;
            }

            var jobId = RMJobService.CreateJobWithScopeId(JobType.AzureFileShareDataSynchronisation, jobRunByUser, selectedNode.Id);
            var needRunningJobNodes = new List<AzureFileShareTreeNode>();
            try
            {
                if (selectedNode.Level == RMNodeLevel.AzureFileShareGroup)
                {
                    var connections = await Querier.GetChildrenConnectionsUnderGroupAsync(selectedNode);
                    connections = connections.Where(item => !CheckCurrentNodeHasRunningSyncJob(new List<string> { item.Id }));
                    needRunningJobNodes.AddRange(connections);
                }
                else
                {
                    needRunningJobNodes.Add(selectedNode);
                }

                if (needRunningJobNodes.Count == 0)
                {
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_AzureFileShare_DataSync_NoAvailableNode");
                    return jobId;
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while assembling runnable nodes. JobId:{0} Error:{1}", jobId, e.ToString());
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                return jobId;
            }

            CreateSubJobs(jobId, JobType.AzureFileShareDataSynchronisation, needRunningJobNodes, JobRunBy.Control);

            return jobId;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings, Action = AuditAction.AzureFileRunDataSyncJob, BeforeHandler = typeof(AzureFileShareServiceBeforeAuditHandler), AfterHandler = typeof(AzureFileShareServiceAfterAuditHandler))]
        public async Task<string> RealRunDataSyncScheduleJobAsync()
        {
            if(RMJobService.GetRunningJobsCount(JobType.AzureFileShareDataSynchronisationSchedule) > 0)
            {
                var skippedJobId = RMJobService.CreateJob(JobType.AzureFileShareDataSynchronisationSchedule, "RM_TS_RunSchedule");
                RMJobService.UpdateJobStatus(skippedJobId, JobStatus.Skipped, "RM_FSDataSync_JobSkip");
                return skippedJobId;
            }

            var jobId = RMJobService.CreateJob(JobType.AzureFileShareDataSynchronisationSchedule, "RM_TS_RunSchedule");

            var needRunningJobNodes = new List<AzureFileShareTreeNode>();
            try
            {
                var settings = await GetAllSettingInfoesAsync();
                var settingNodes = settings.Select(item => item.SelectedNode)
                    .GroupBy(item => item.ContainerId)
                    .Where(item => !CheckCurrentNodeHasRunningSyncJob(new List<string> { item.Key }))
                    .SelectMany(item => item)
                    .GroupBy(item => item.ConnectionId)
                    .Where(item => !CheckCurrentNodeHasRunningSyncJob(new List<string> { item.Key.ToString() }))
                    .SelectMany(item => item)
                    .OrderBy(item => item.Level)
                    .ThenBy(item => item.FullPath);
                var runnableGroupIds = new HashSet<string>();
                var runnableConnectionIds = new HashSet<Guid>();
                var runnableScopeIds = new HashSet<string>();
                foreach (var node in settingNodes)
                {
                    if (runnableGroupIds.Contains(node.ContainerId) || runnableConnectionIds.Contains(node.ConnectionId))
                    {
                        continue;
                    }

                    if (node.Level == RMNodeLevel.AzureFileShareGroup)
                    {
                        runnableGroupIds.Add(node.Id);
                        var connections = await Querier.GetChildrenConnectionsUnderGroupAsync(node);
                        connections = connections.Where(item => !CheckCurrentNodeHasRunningSyncJob(new List<string> { item.Id }));
                        needRunningJobNodes.AddRange(connections);
                        continue;
                    }

                    if (node.Level == RMNodeLevel.AzureFileShareConnection)
                    {
                        runnableConnectionIds.Add(new Guid(node.Id));
                        needRunningJobNodes.Add(node);
                    }

                    var scopeIds = new List<string>();
                    var tempNode = node;
                    while (tempNode.Level != RMNodeLevel.AzureFileShareConnection)
                    {
                        scopeIds.Add(tempNode.Id);
                        tempNode = tempNode.Parent;
                    }

                    if (!scopeIds.Any(item => runnableScopeIds.Contains(item)) && !CheckCurrentNodeHasRunningSyncJob(scopeIds))
                    {
                        runnableScopeIds.Add(node.Id);
                        needRunningJobNodes.Add(node);
                    }
                }

                if (!needRunningJobNodes.Any())
                {
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_AzureFileShare_DataSync_NoAvailableNode");
                    return jobId;
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while assembling runnable nodes. JobId:{0} Error:{1}", jobId, e.ToString());
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, e.Message);
                return jobId;
            }
            
            CreateSubJobs(jobId, JobType.AzureFileShareDataSynchronisationSchedule, needRunningJobNodes, JobRunBy.Schedule);

            return jobId;
        }

        private bool CheckCurrentNodeHasRunningSyncJob(List<string> scopeIds)
        {
            return RMJobService.GetRunningJobsScopeId(JobType.AzureFileShareDataSynchronisation).Any(item => scopeIds.Contains(item));
        }

        private void CreateSubJobs(string jobId, JobType jobType, List<AzureFileShareTreeNode> needRunningJobNodes, JobRunBy runBy)
        {
            var subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            for (var i = 0; i < needRunningJobNodes.Count; i++)
            {
                var node = needRunningJobNodes[i];
                var subJobId = string.Format(jobId + "_{0:D3}", i);
                var subJob = new RMSubJob
                {
                    Id = subJobId,
                    ParentId = jobId,
                    StartTime = DateTime.UtcNow.Ticks,
                    JobType = (int)jobType,
                    Progress = 0,
                    Status = (int)JobStatus.Wait,
                    Weight = 100d / needRunningJobNodes.Count,
                    Runable = i < subJobCountInConfigFile ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting,
                    JobContext = new RMJobContext
                    {
                        JobId = subJobId,
                        Content = JsonConvert.SerializeObject(node)
                    },
                };
                SubJobDao.CreateJob(subJob);
                logger.Info($"Create sub job {jobType}-{subJobId} succeed.");

                if (i < subJobCountInConfigFile)
                {
                    mJobQueueService.HandleMessage(new JobQueueMessage
                    {
                        JobId = subJobId,
                        RunBy = runBy,
                        JobType = jobType,
                        CommandLine = $"{jobType} {subJobId}"
                    });
                }
            }
        }

        public RAReturnMessage RunDisposalJob(AzureFileShareTreeNode selectedTree, JobRunBy jobRunBy)
        {
            throw new NotImplementedException();
        }

        private RMAzureFileShareSetting LoadParentAllSeting(AzureFileShareTreeNode node, Guid connectionGroupId)
        {
            RMAzureFileShareSetting setting = null;
            if (node.Level == RMNodeLevel.AzureFileShareGroup)
            {
                return setting;
            }
            setting = AzureFileShareSettingDao.LoadSetting(new Guid(node.Id), new Guid(node.ContainerId));
            if (setting == null)
            {
                setting = LoadParentAllSeting(node.Parent, new Guid(node.ContainerId));
            }
            return setting;
        }

        private string GetConnectionGroupId(AzureFileShareTreeNode selectedNode)
        {
            if (selectedNode?.Level == RMNodeLevel.AzureFileShareGroup)
            {
                return selectedNode?.Id;
            }
            return selectedNode?.ContainerId;
        }

    }
}
