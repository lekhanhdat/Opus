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
using System.Linq.Expressions;
using System.Threading.Tasks;
using Aspose.Pdf.Operators;
using AvePoint.GCommon.Contract.Tree;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Google;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Label;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
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
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Service.Services.ControlPanel;
using AvePoint.RA.Service.Services.Google.AuditHandler;
using AvePoint.RA.Service.Services.RMSharePointSettings;
using AvePoint.RA.Service.Services.RMSharePointSettings.AuditHandler;
using AvePoint.RA.Service.Services.Schedule.AuditHandler;
using Newtonsoft.Json;
using RAGoogle;
using static AvePoint.RA.DB.Dao.GoogleSyncNodeDao.RMGoogleRemoteNodeDao;

namespace AvePoint.RA.Service.Services.Google
{
    [Audit]
    public class RMGoogleSettingService : BaseContentRepositorySettingsService, IRMGoogleSettingsService
    {
        private IRALogger Logger = RALogger.GetInstance(typeof(RMGoogleSettingService));

        // DI services
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();

        private IRMGoogleRemoteNodeDao RemoteNodeDao =>
            PlatformWindsorManager.GetService<IRMGoogleRemoteNodeDao>();

        private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();
        private IRecordOwnerDao RecordOwnerDao => PlatformWindsorManager.GetService<IRecordOwnerDao>();
        private IRMGoogleSettingDao RmGoogleSettingDao => PlatformWindsorManager.GetService<IRMGoogleSettingDao>();
        private ILabelDao LabelDao => PlatformWindsorManager.GetService<ILabelDao>();
        private ISecurityGroupManagementService SecurityGroupManagementService => PlatformWindsorManager.GetService<ISecurityGroupManagementService>();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        
        private ITermGroupDao TermGroupDao => PlatformWindsorManager.GetService<ITermGroupDao>();
        private ITermGroupMembershipDao TermGroupMembershipDao => PlatformWindsorManager.GetService<ITermGroupMembershipDao>();


        #region Load And Apply Setting
        public async Task<RAReturnMessage> CreateDisposeSchedule(RMGoogleTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                Guid driveId = Guid.Empty;
                if (!IsGoogleContainer(nodeSetting.Level))
                {
                    var googleDrive = GetDriveNode(nodeSetting);
                    driveId = new Guid(googleDrive.Id);
                }

                if (!CheckParentNodeDisable(nodeSetting, driveId))
                {
                    nodeSetting.DisposeScheduleInfo.Id = Guid.NewGuid().ToString();
                    var cloneNodeInfo = nodeSetting.Clone();
                    cloneNodeInfo.DisposeScheduleInfo = null;
                    cloneNodeInfo.SkipRemoveContentAndDestroyAction =
                        nodeSetting.DisposeScheduleInfo.Extentions.Equals("true", StringComparison.OrdinalIgnoreCase);
                    nodeSetting.DisposeScheduleInfo.Extentions = JsonConvert.SerializeObject(cloneNodeInfo);
                    nodeSetting.DisposeScheduleInfo.ProfileId = ScheduleService.GetProfileId(nodeSetting);
                    var schedule = await ScheduleService.CreateScheduleServiceForGoogleAsync(nodeSetting.DisposeScheduleInfo,
                        true, nodeSetting.DisplayName);
                    if (schedule == "-1")
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.ScheduleServiceFailed;
                    }
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DisableRecordsManagement;
                    result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                }
            }
            catch (Exception ex)
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = ex.Message;
                Logger.Error("Update Dispose Schedule Service Failed.ERROR:{0}", ex.Message);
                throw;
            }

            return result;
        }


        public async Task<RMGoogleTreeNode> LoadGoogleNodeSettingsAsync(RMSampleGoogleTreeNode sampleGoogleNode)
        {
            RMGoogleTreeNode configNode = new();
            configNode.CopyProperties(sampleGoogleNode);

            try
            {
                RMSampleGoogleTreeNode containerNode = sampleGoogleNode;
                while (containerNode != null &&  !IsGoogleContainer(containerNode.Level))
                {
                    containerNode = containerNode.Parent;
                }

                if (containerNode == null)
                {
                    return configNode;
                }

                RMSampleGoogleTreeNode driveNode = sampleGoogleNode;
                while (driveNode != null && !IsGoogleDrive(driveNode.Level))
                {
                    driveNode = driveNode.Parent;
                }

                Guid driveId = Guid.Empty;
                if (driveNode != null)
                {
                    driveId = new Guid(driveNode.Id);
                }

                Guid containerId = new Guid(containerNode.Id);
                var containerSetting = RemoteNodeDao.LoadGoogleSetting(containerId, Guid.Empty);
                if (containerSetting != null)
                {
                    await HandleContainerSetting(containerSetting, configNode, containerId, driveId);
                    if (IsGoogleDrive(sampleGoogleNode.Level))
                    {
                        if (containerSetting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                        {
                            configNode.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                        }
                        else
                        {
                            configNode.EnableRecordManagement = (int)EnableRecordManagementSetting.Enable;
                        }
                    }
                }

                var driveSetting = RemoteNodeDao.LoadGoogleSetting(containerId, driveId);
                // For Folder Node Level In The Future
                if (driveSetting == null)
                {
                    if (IsGoogleDrive(sampleGoogleNode.Level))
                    {
                        var container = RemoteNodeDao.LoadGoogleSetting(containerId, driveId);
                        if (container != null && !IsGoogleContainer(configNode.Level))
                        {
                            if (container.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                            {
                                configNode.EnableRecordManagement = (int)EnableRecordManagementSetting.ParentDisable;
                            }
                        }
                
                        configNode.IsCustomSetting = false;
                    }
                }
                else
                {
                    configNode.IconStatus = IconStatus.Break;
                    if (!IsGoogleContainer(sampleGoogleNode.Level))
                    {
                        configNode.IsCustomSetting = true;
                    }
                }

                if (driveSetting != null)
                {
                    await HandleDriveSetting(configNode, driveSetting, driveId);
                }
                var profileId = ScheduleService.GetProfileId(sampleGoogleNode);
                var disposeSchedule = await ScheduleService.GetScheduleAsync(profileId, ScheduleType.GoogleDisposalSchedule);

                await HandleSchedule(disposeSchedule, configNode, profileId);
                
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred when load Google Drive Setting.Error:{0}", e.ToString());
                throw;
            }

            return configNode;
        }


        private async Task HandleSchedule(ScheduleInfo disposeSchedule, RMGoogleTreeNode configNode, string profileId)
        {
            if (disposeSchedule != null)
            {
                var simplifyZoneInfo = DateTimeUtil.GetSimplifyZoneInfo(disposeSchedule.TimeZoneId);
                disposeSchedule.StartTime = string.Format($"{disposeSchedule.StartTime} {simplifyZoneInfo}");
                disposeSchedule.EndTime = string.Format($"{disposeSchedule.EndTime} {simplifyZoneInfo}");

                configNode.DisposeScheduleInfo = disposeSchedule;
                configNode.DisposeScheduleInfo.Extentions = JsonConvert
                    .DeserializeObject<RMGoogleTreeNode>(configNode.DisposeScheduleInfo.Extentions)
                    .SkipRemoveContentAndDestroyAction.ToString();
                
                configNode.IconStatus = IconStatus.Break;
            }
            else
            {
                var ancestryDisposeSchedule =
                    await ScheduleService.GetAncestryScheduleAsync(profileId, ScheduleType.GoogleDisposalSchedule);
                if (ancestryDisposeSchedule != null)
                {
                    var simplifyZoneInfo = DateTimeUtil.GetSimplifyZoneInfo(ancestryDisposeSchedule.TimeZoneId);
                    ancestryDisposeSchedule.StartTime =
                        string.Format($"{ancestryDisposeSchedule.StartTime} {simplifyZoneInfo}");
                    ancestryDisposeSchedule.EndTime =
                        string.Format($"{ancestryDisposeSchedule.EndTime} {simplifyZoneInfo}");
                    configNode.DisposeScheduleInfo = ancestryDisposeSchedule;
                    configNode.DisposeScheduleInfo.Id = "1";
                    configNode.DisposeScheduleInfo.Extentions = JsonConvert
                        .DeserializeObject<RMGoogleTreeNode>(configNode.DisposeScheduleInfo.Extentions)
                        .SkipRemoveContentAndDestroyAction.ToString();
                }
                else
                {
                    configNode.DisposeScheduleInfo = null;
                }
            }
        }

        private async Task HandleDriveSetting(RMGoogleTreeNode configNode, RMGoogleSetting driveSetting, Guid driveId)
        {
            configNode.ApplyExistType = driveSetting.ApplyExistType;
            configNode.RecordOwner =
                await RecordOwnerDao.GetRecordOwnerAccountsAsync(driveSetting.Id, RecordOwnerSettingType.GoogleDrive);
            configNode.DeployLabelMethod = (DeployLabelMethod) driveSetting.DeployLabelMethod;
            configNode.AutoClassificationRules = driveSetting.AutoClassificationRules == null
                ? null
                : SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(driveSetting
                    .AutoClassificationRules);
            await UpdateTermNameAndStatus(configNode.AutoClassificationRules, driveSetting);
            await ConvertClassificationRuleTimeZoneAsync(configNode.AutoClassificationRules);
            ConvertClassificationRuleAndOrExpression(configNode.AutoClassificationRules);
            configNode.RunAutoFullJob = driveSetting.RunAutoFullJob;
            configNode.AutoJobOption = (AutoJobOption)driveSetting.AutoJobOption == AutoJobOption.None
                ? AutoJobOption.Append
                : (AutoJobOption)driveSetting.AutoJobOption;
            configNode.IsNullClassificationSetting = driveSetting.IsNullClassificationSetting;
            configNode.EnableRecordManagement = driveSetting.EnableRecordManagement;
            configNode.IsSyncData = driveSetting.IsSyncData;
            configNode.ApprovalType = (int)driveSetting.ApprovalType;
            configNode.WorkflowReferenceId = driveSetting.WorkflowReferenceId;
            configNode.DriveId = driveId.ToString();
            configNode.TermGroupId = string.IsNullOrEmpty(driveSetting.NodeInfo)
               ? new Guid() :
               SerializerHelper.DeserializeByDataContractSerializer<RMGoogleTreeNode>(driveSetting.NodeInfo).TermGroupId;

            configNode.AITermUseType = driveSetting.AITermUseType;
            configNode.AIApprovalType = (int)driveSetting.AIApprovalType;
            configNode.AISendEMail = driveSetting.AISendEMail;
            configNode.AIReviewers = await RecordOwnerDao.GetRecordOwnerAccountsAsync(driveSetting.Id, RecordOwnerSettingType.AIGoogleDrive);
            configNode.AIThenIsDefaultTermMethod = driveSetting.AIThenIsDefaultTermMethod;
            configNode.AIThenDefaultTermId = driveSetting.AIThenDefaultTermId;
            configNode.AIThenDefaultTermName = driveSetting.AIThenDefaultTermName;
        }

    

        private async Task HandleContainerSetting(RMGoogleSetting containerSetting, RMGoogleTreeNode configNode,
            Guid containerId, Guid driveId)
        {
            configNode.IconStatus = IconStatus.Inhert;
            configNode.ApplyExistType = containerSetting.ApplyExistType;
            configNode.RecordOwner =
                await RecordOwnerDao.GetRecordOwnerAccountsAsync(containerSetting.Id,
                    RecordOwnerSettingType.GoogleDrive);
            configNode.DeployLabelMethod = (DeployLabelMethod) containerSetting.DeployLabelMethod;
            configNode.AutoClassificationRules = containerSetting.AutoClassificationRules == null
                ? null
                : SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(containerSetting
                    .AutoClassificationRules);
            await UpdateTermNameAndStatus(configNode.AutoClassificationRules, containerSetting);
            await ConvertClassificationRuleTimeZoneAsync(configNode.AutoClassificationRules);
            ConvertClassificationRuleAndOrExpression(configNode.AutoClassificationRules);
            configNode.RunAutoFullJob = containerSetting.RunAutoFullJob;
            configNode.AutoJobOption = (AutoJobOption)containerSetting.AutoJobOption == AutoJobOption.None
                ? AutoJobOption.Append
                : (AutoJobOption)containerSetting.AutoJobOption;
            configNode.EnableRecordManagement = containerSetting.EnableRecordManagement;
            configNode.IsSyncData = containerSetting.IsSyncData;
            configNode.ApprovalType = (int)containerSetting.ApprovalType;
            configNode.WorkflowReferenceId = containerSetting.WorkflowReferenceId;
            configNode.IsNullClassificationSetting = containerSetting.IsNullClassificationSetting;
            configNode.Rules = RemoteNodeDao.GetMappingRules(containerId, driveId);
            configNode.ContainerId = containerId.ToString();
            configNode.TermGroupId = string.IsNullOrEmpty(containerSetting.NodeInfo) 
                ? new Guid() : //Need load TermGroupId from existing container in next sprint.
                SerializerHelper.DeserializeByDataContractSerializer<RMGoogleTreeNode>(containerSetting.NodeInfo).TermGroupId;

            configNode.AITermUseType = containerSetting.AITermUseType;
            configNode.AIApprovalType = (int)containerSetting.AIApprovalType;
            configNode.AISendEMail = containerSetting.AISendEMail;
            configNode.AIReviewers = await RecordOwnerDao.GetRecordOwnerAccountsAsync(containerSetting.Id, RecordOwnerSettingType.AIGoogleDrive);
            configNode.AIThenIsDefaultTermMethod = containerSetting.AIThenIsDefaultTermMethod;
            configNode.AIThenDefaultTermId = containerSetting.AIThenDefaultTermId;
            configNode.AIThenDefaultTermName = containerSetting.AIThenDefaultTermName;
            configNode.HasContainerSetting = true;
        }
        private async Task UpdateTermNameAndStatus(List<ClassificationRule> autoRules, RMGoogleSetting nodeSetting)
        {
            try
            {
                if (autoRules == null)
                {
                    return;
                }
                var nodeInfo = SerializerHelper.DeserializeByDataContractSerializer<RMGoogleTreeNode>(nodeSetting.NodeInfo);
                var labels = await LabelDao.GetLabelsByIdsAsync(autoRules.Select(rule => rule.TermId).ToList());

                var termGroupIds = new List<string>();
                List<string> ggTenantsUnderNodes = nodeInfo.NodeType switch
                {
                    (int)NodeLevel.GoogleMyDriveContainer or (int)NodeLevel.GoogleSharedDriveContainer =>
                        await RemoteNodeDao.GetGoogleTenantIdsUnderContainers( new List<string> { nodeInfo.ContainerId}),

                    (int)NodeLevel.GoogleMyDrive or (int)NodeLevel.GoogleSharedDrive =>
                      RemoteNodeDao.GetGoogleTenantIdsUnderNodes(new List<string> { nodeSetting.DriveId.ToString() }, NodeLevelExpressionType.ExpressionGoogleDrive),

                    _ => new List<string>()
                };

                termGroupIds = await TermGroupMembershipDao.GetTermGroupsBySiteUrlGroupIds(ggTenantsUnderNodes);

                foreach (var autoRule in autoRules)
                {
                    if (string.IsNullOrEmpty(autoRule.TermId) || autoRule.TermId == Guid.Empty.ToString())
                    {
                        continue;
                    }

                    var existingLabel = labels.FirstOrDefault(label => label.UniqueId == new Guid(autoRule.TermId));

                    if (existingLabel == null)
                    {
                        continue;
                    }

                    var termExistingGroupId = await TermGroupDao.GetTermGroupIdByTermUniqueId(existingLabel.UniqueId);

                    autoRule.TermExistingTermGroup = termGroupIds.Count > 0 && termGroupIds.Any(id => id == termExistingGroupId);
                    autoRule.TermName = existingLabel.Name;
                    autoRule.TermIsRemoved = existingLabel.IsRemoved;
                    autoRule.TermIsDeprecated = existingLabel.IsDeprecated || TermDao.IsExpiredTerm(existingLabel.Id);
                    autoRule.TermHasNoPermission = await IsTermNoPermission(existingLabel.UniqueId);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Set auto google status error:{0}", e.ToString());
            }
        }

        private async Task<bool> IsTermNoPermission(Guid existingLabelUniqueId)
        {
            var termPermissionInfo = await SecurityGroupManagementService.GetSecurityTermObjInfoAsync(new ()
            {
                UserId = TenantLocalValue.LogonUserId,
                Level = SecurityTermLevel.TermGroup,
                FilterByContentSource = true,
                ExcludeBuiltIn = false,
                SourceFlag = SourceFlag.Google
            });
            if (termPermissionInfo.TermPermissionType == TermPermissionMethod.All)
            {
                return false;
            }

            return !await SecurityGroupManagementService.DoesUserHasPermisionToTermAsync(TenantLocalValue.LogonUserId, SecurityTermLevel.Term, [existingLabelUniqueId]);
        }

        [Audit(Module = AuditModule.GoogleDrive, Category = AuditCategory.SharePointSettings, Action = AuditAction.SaveGeneralSetting, BeforeHandler = typeof(GoogleServiceBeforeAuditHandler), AfterHandler = typeof(GoogleServiceAfterAuditHandler))]
        public async Task<RAReturnMessage> AddGoogleDriveGeneralSettingAsync(RMGoogleTreeNode settingNode)
        {
            RAReturnMessage enableResult = await AddEnableRecordsManagementSettingAsync(settingNode);
            RAReturnMessage isShowUniqueIdResult = new RAReturnMessage();
            if (settingNode.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
            {
                //isShowUniqueIdResult = await AddIsShowUniqueIdSettingAsync(groupNode);
            }

            RAReturnMessage result = new RAReturnMessage();
            if (enableResult.MessageType == RAMessageType.Failed)
            {
                result = enableResult;
            }
            else if (isShowUniqueIdResult.MessageType == RAMessageType.Failed)
            {
                result = isShowUniqueIdResult;
            }
            else
            {
                result.MessageType = RAMessageType.Successful;
            }

            return result;
        }
        [Audit(Module = AuditModule.GoogleDrive, Category = AuditCategory.SharePointSettings, Action = AuditAction.SaveLabelSetting, BeforeHandler = typeof(GoogleServiceBeforeAuditHandler), AfterHandler = typeof(GoogleServiceAfterAuditHandler))]
        public async Task<RAReturnMessage> AddLabelSettingAsync(RMGoogleTreeNode settingNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                if (CheckGoogleNodeDisable(settingNode))
                {
                    AddFilterCretiaProperty(settingNode.AutoClassificationRules);
                    SetPropertiesByNodeLevel(settingNode);
                    await RmGoogleSettingDao.AddOrUpdateCustomSettingAsync(settingNode, Guid.Empty);
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
                Logger.Warn("Save Google Node Setting DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }
        public async Task<RAReturnMessage> UpdateDisposeSchedule(RMGoogleTreeNode selectedNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                if (CheckGoogleNodeDisable(selectedNode))
                {
                    var cloneNodeInfo = selectedNode.Clone();
                    cloneNodeInfo.DisposeScheduleInfo = null;
                    cloneNodeInfo.SkipRemoveContentAndDestroyAction = selectedNode.DisposeScheduleInfo.Extentions.Equals("true", StringComparison.OrdinalIgnoreCase);
                    selectedNode.DisposeScheduleInfo.Extentions = JsonConvert.SerializeObject(cloneNodeInfo);
                    var schedule = await ScheduleService.UpdateScheduleServiceForGoogleAsync(selectedNode.DisposeScheduleInfo, selectedNode.DisplayName);
                    if (schedule == "-1")
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.ScheduleServiceFailed;
                    }
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DisableRecordsManagement;
                    result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                }
            }
            catch (Exception ex)
            {
                result.MessageType = RAMessageType.Failed;
                Logger.Error("Update Dispose Schedule Service Failed.ERROR:{0}", ex.Message);
            }

            return result;
        }
        public async Task<RAReturnMessage> DeleteDisposeSchedule(RMGoogleTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                if (CheckGoogleNodeDisable(nodeSetting))
                {
                    ScheduleService.DeleteScheduleServiceForGoogle(nodeSetting.DisposeScheduleInfo.Id, nodeSetting.DisplayName);
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DisableRecordsManagement;
                    result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                }
            }
            catch (Exception ex)
            {
                result.MessageType = RAMessageType.Failed;
                Logger.Error("Delete Collection Schedule Service Failed.ERROR:{0}", ex.Message);
            }

            return result;
        }
        [Audit(Module = AuditModule.GoogleDrive, Category = AuditCategory.SharePointSettings, Action = AuditAction.EditInheritSettingGoogle, BeforeHandler = typeof(GoogleServiceBeforeAuditHandler), AfterHandler = typeof(GoogleServiceAfterAuditHandler))]
        public async Task<RAReturnMessage> InheritParentSettingAsync(RMGoogleTreeNode selectedNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                Logger.Info("Inherit Parent Settings");
                var driveNode = GetDriveNode(selectedNode);

                await RmGoogleSettingDao.DeleteGoogleSettingAsync(new Guid(driveNode.Id));
                return result;
            }
            catch (Exception ex)
            {
                Logger.Warn("Inherit Parent Setting on node [{0}] to DB Error {1}", selectedNode?.FullPath ,ex.ToString());
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        private void SetPropertiesByNodeLevel(RMGoogleTreeNode settingNode)
        {
            string containerId = string.Empty;
            var containerNode = settingNode.GetGroupNode();
            if (containerNode != null)
            {
                containerId = containerNode.Id;
            }
            settingNode.ContainerId = containerId;

            if (IsGoogleDrive(settingNode.Level))
            {
                settingNode.DriveId = settingNode.Id;
            }
            settingNode.isEnableClassification = false;
            settingNode.DescriptionOfContainer = null;
        }

        private bool CheckGoogleNodeDisable(RMGoogleTreeNode settingNode, bool isCheckSelfNode = true)
        {
            bool checkRecordsManagement = true;
            if (isCheckSelfNode)
            {
                RMGoogleSetting googleSettingSetting = RmGoogleSettingDao.GetSettingInfoByAgentId(settingNode.Id);
                if (googleSettingSetting is { EnableRecordManagement: (int)EnableRecordManagementSetting.Disable })
                {
                    checkRecordsManagement = false;
                }
            }
            if (IsGoogleDrive(settingNode.Level))
            {
                RMGoogleSetting googleSettingSetting = RmGoogleSettingDao.GetSettingInfoByAgentId(settingNode.Id);
                if (googleSettingSetting is { EnableRecordManagement: (int)EnableRecordManagementSetting.Disable })
                {
                    checkRecordsManagement = false;
                }
            }
            return checkRecordsManagement;
        }

        private async Task<RAReturnMessage> AddEnableRecordsManagementSettingAsync(RMGoogleTreeNode settingNode)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                Guid driveId = Guid.Empty;
                RMGoogleTreeNode googleDrive = null;
                if (!IsGoogleContainer(settingNode.Level))
                {
                    googleDrive = GetDriveNode(settingNode);
                    driveId = new Guid(googleDrive.Id);
                }

                if (!CheckParentNodeDisable(settingNode, driveId, false))
                {
                    AddFilterCretiaProperty(settingNode.AutoClassificationRules, SourceFlag.Google);
                    await RmGoogleSettingDao.AddOrUpdateCustomSettingAsync(settingNode, driveId);
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.FaildType = RAFailedType.DisableRecordsManagement;
                    result.ErrorMessage = I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed");
                    return result;
                }

                string nodeProfileIdPath = ScheduleService.GetProfileId(settingNode);
                await RmGoogleSettingDao.CheckNeedRemoveDescendantsSetting(settingNode, nodeProfileIdPath);
                return result;
            }
            catch (EnableDataCollectionStatusException ex)
            {
                result.MessageType = RAMessageType.Failed;
                result.FaildType = RAFailedType.EnableInsightsDataCollection;
                result.ErrorMessage = I18NEntity.GetString("RM_EnableDataCollectionSwitch_Error_Message");
                return result;
            }
            catch (Exception ex)
            {
                Logger.Warn("Save Global Setting to DB Error {0}", ex.ToString());
                result.MessageType = RAMessageType.Failed;
                return result;
            }
        }

        private bool CheckParentNodeDisable(RMGoogleTreeNode settingNode, Guid containerId,
            bool isCheckSelfNode = true)
        {
            string scopeIdString = string.Empty;
            var isDisableRecordsManagement = false;
            if (settingNode.DisposeScheduleInfo is { JobCategory: ScheduleType.GoogleArchiveJobSchedule })
            {
                isDisableRecordsManagement = false;
            }
            else
            {
                try
                {
                    Expression<Func<RMGoogleSetting, bool>> whereLambda =
                        this.GetCheckDisableLambda(settingNode, containerId, isCheckSelfNode);
                    Logger.Debug($"CheckParentNodeDisable where lambda: {whereLambda}");
                    if (RmGoogleSettingDao.GetParentNode(whereLambda) != null)
                    {
                        isDisableRecordsManagement = true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Check Parent Node Records Management error:{0}", ex.ToString());
                }
            }


            return isDisableRecordsManagement;
        }

        private Expression<Func<RMGoogleSetting, bool>> GetCheckDisableLambda(RMGoogleTreeNode settingNode,
            Guid containerId, bool isCheckSelfNode)
        {
            Expression queryExpr = null;
            List<Expression> allExpressionList = [];
            ParameterExpression param = Expression.Parameter(typeof(RMGoogleSetting), "c");
            List<Guid> scopeIds = GetParentScopeId(settingNode, isCheckSelfNode);
            allExpressionList.Add(
                Expression4DynamicQuery.GetInExpression(typeof(RMGoogleSetting), param, "ScopeId", scopeIds));
            allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(RMGoogleSetting), param,
                "EnableRecordManagement", (int)EnableRecordManagementSetting.Disable));

            allExpressionList.Add(Expression4DynamicQuery.GetInExpression(typeof(RMGoogleSetting), param, "DriveId",
                new List<object> { containerId, Guid.Empty }));
            var containerNode = settingNode.GetGroupNode();
            if (containerNode != null)
            {
                allExpressionList.Add(Expression4DynamicQuery.GetDoubleEqualityExpression(typeof(RMGoogleSetting),
                    param, "ContainerId", new Guid(containerNode.Id)));
            }

            queryExpr = allExpressionList.Aggregate(Expression.AndAlso);
            return Expression.Lambda<Func<RMGoogleSetting, bool>>(queryExpr, param);
        }

        private List<Guid> GetParentScopeId(RMGoogleTreeNode settingNode, bool isCheckSelfNode)
        {
            List<Guid> scopeIds = [];
            if (isCheckSelfNode)
            {
                scopeIds.Add(new Guid(settingNode.Id));
            }

            while (settingNode.Parent is { Id: not null })
            {
                scopeIds.Add(new Guid(settingNode.Parent.Id));
                settingNode = settingNode.Parent;
            }

            return scopeIds;
        }

        private RMGoogleTreeNode GetDriveNode(RMGoogleTreeNode node)
        {
            while (node != null && !IsGoogleDrive(node.Level))
            {
                node = node.Parent;
            }

            return node;
        }

        #endregion

        private bool IsGoogleContainer(int level)
        {
            return level == (int)NodeLevel.GoogleMyDriveContainer || level == (int)NodeLevel.GoogleSharedDriveContainer;
        }

        private bool IsGoogleDrive(int level)
        {
            return level == (int)NodeLevel.GoogleMyDrive || level == (int)NodeLevel.GoogleSharedDrive;
        }

        #region data sync
        public async Task<RAReturnMessage> RunDataSyncJob(RMGoogleTreeNode selectedTree, JobRunBy jobRunBy)
        {
            RAReturnMessage msg = new RAReturnMessage();

            if (!GooglePermissionHelper.HasGoogleLicense())
            {
                Logger.Warn($"Don't have Google permission to execute this job, job type [{JobType.GoogleDataSynchronization}]");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
            }

            if (selectedTree == null)
            {
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = I18NEntity.GetString("Selected node is null");
                return msg;
            }

            Logger.Debug("start data sync");
  
            string id = string.Empty;
   
            if (!(await CanRunDataSync(selectedTree)))
            {
                msg.MessageType = RAMessageType.Failed;
                msg.ErrorMessage = I18NEntity.GetString("node isnt enabled record management or data sync");
                return msg;
            }
            
            try
            {

                var groupId = TenantLocalValue.LogonGroupId;
                //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                var loginName = TenantLocalValue.LogonUserEmail;

                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.GoogleDataSynchronization,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = selectedTree == null ? null : SerializerHelper.SerializeByDataContractSerializer(selectedTree)
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
                if (!string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Successful, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                Logger.Error("error occurred while ApplySettings,ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        private async Task<bool> CanRunDataSync(RMGoogleTreeNode node)
        {
            if (node.ContainerId.IsNullOrEmpty())
            {
                return false;
            }

            var containerId = new Guid(node.ContainerId);
            Guid driveId = Guid.Empty;

            if (!IsGoogleContainerOrDrive((NodeLevel)node.Level))
            {
                return false;
            }

            RMGoogleSetting setting = null;
            if (IsGoogleContainer(node.Level))
            {
                setting = await RmGoogleSettingDao.GetSettingInfo(containerId, driveId);
            }

            else if (IsGoogleDrive(node.Level))
            {
                if (node.DriveId.IsNullOrEmpty())
                {
                    setting = await RmGoogleSettingDao.GetSettingInfo(containerId, driveId);
                }
                else
                {
                    driveId = new Guid(node.DriveId);
                    setting = await RmGoogleSettingDao.GetSettingInfo(containerId, driveId);
                    if (setting == null)
                    {
                        setting = await RmGoogleSettingDao.GetSettingInfo(containerId, Guid.Empty);
                    }
                }
            }

            if (setting != null && setting.IsNullClassificationSetting == true)
            {
                Logger.Info($"Setting classification is disable");
                return false;
            }

            if (setting != null && setting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable && setting.IsSyncData)
            {
                Logger.Info($"IsEnableRecordManagement and Enable data sync:{true}");
                return true;
            }
            return false;
        }

        private bool IsGoogleContainerOrDrive(NodeLevel node)
        {
            return (IsGoogleContainer((int)node) || IsGoogleDrive((int)node));
        }

        public RAReturnMessage RunDataSyncJob(JobRunBy jobRunBy)
        {
            Logger.Debug("start data sync");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();

            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.GoogleDataSynchronization,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
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
                Logger.Error("error occurred while sync for search, ERROR:{0}", ex.ToString());
            }

            return msg;
        }
        #endregion

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

        #region Google One
        [Audit(Module = AuditModule.GoogleDrive, Category = AuditCategory.SharePointSettings, Action = AuditAction.SaveLabelSetting, BeforeHandler = typeof(GoogleServiceBeforeAuditHandler), AfterHandler = typeof(GoogleServiceAfterAuditHandler))]
        public async Task<string> AddGoogleNodeSettingsAsync(RMGoogleTreeNode settingNode, bool needCheckConflictTenant = true)
        {
            try
            {
                Guid driveId = Guid.Empty;
                if (!IsGoogleContainer(settingNode.Level))
                {
                    var googleDrive = GetDriveNode(settingNode);
                    Guid.TryParse(googleDrive.Id, out driveId);
                }

                if (CheckParentNodeDisable(settingNode, driveId, false))
                {
                    throw new Exception(I18NEntity.GetString("RM_SPS_SelectRecordsManagement_Failed"));
                }

                if (!settingNode.IsNullClassificationSetting && needCheckConflictTenant)
                {
                    await HandleGoogleTenantsUnderTermGroup(new List<RMGoogleTreeNode> { settingNode });
                }

                await PrepareSavingNodeSettings(settingNode);
                await RmGoogleSettingDao.AddOrUpdateCustomSettingAsync(settingNode, Guid.Empty);
                var nodeProfileIdPath = ScheduleService.GetProfileId(settingNode);
                await RmGoogleSettingDao.CheckNeedRemoveDescendantsSetting(settingNode, nodeProfileIdPath);
                return string.Empty;
            }
            catch (Exception ex)
            {
                Logger.Error("Save Google Node [{0}] Setting DB Error: {1}", settingNode?.FullPath, ex.ToString());

                if (ex.Message.Contains("GoogleOpusUI.error.selected-scope-conflict-term-groups"))
                    return ex.Message;

                return "GoogleOpusUI.common.save-settings-fail.title";
            }
        }

        private async Task PrepareSavingNodeSettings(RMGoogleTreeNode settingNode)
        {
            AddFilterCretiaProperty(settingNode.AutoClassificationRules);
            SetPropertiesByNodeLevel(settingNode);
            await RmGoogleSettingDao.UpdateEnableRecordManagement(settingNode);
            await HandleDisposalScheduleOnNode(settingNode);
        }

        public async Task<string> BulkAddGoogleNodeSettingsAsync(List<RMGoogleTreeNode> settingNodes)
        {
            var returnMessage = string.Empty;
            int batchSize = 15;
            try
            {
                if (!settingNodes[0].IsNullClassificationSetting)
                {
                    await HandleGoogleTenantsUnderTermGroup(settingNodes);
                }
                Logger.Info($"Start bulk saving settings, count [{settingNodes.Count}].");
                for (int i = 0; i < settingNodes.Count; i += batchSize)
                {
                    var batch = settingNodes.Skip(i).Take(batchSize);
                    foreach (var settingNode in batch)
                    {
                        Logger.Info($"Processing setting node: {settingNode.FullPath}");
                        var res = await AddGoogleNodeSettingsAsync(settingNode, false);
                        if (!res.IsNullOrWhiteSpace())
                            returnMessage = res;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while saving settings in batch, ex: {ex.Message}.");
                returnMessage = "GoogleOpusUI.common.save-settings-fail.title";
                if (ex.Message.Contains("GoogleOpusUI.error.selected-scope-conflict-term-groups"))
                {
                    returnMessage = ex.Message;
                }
            }
            return returnMessage;
        }

        public async Task<string> BulkInheritParentSettingAsync(List<RMGoogleTreeNode> selectedNodes)
        {
            int batchSize = 15;
            var returnMessages = new List<RAReturnMessage>();
            try
            {
                Logger.Info($"Start bulk inherit parent settings, count [{selectedNodes.Count}].");
                for (int i = 0; i < selectedNodes.Count; i += batchSize)
                {
                    var batch = selectedNodes.Skip(i).Take(batchSize);
                    foreach (var settingNode in batch)
                    {
                        Logger.Info($"Processing setting node: {settingNode.FullPath}");
                        var res = await InheritParentSettingAsync(settingNode);
                        returnMessages.Add(res);
                    }
                }
                return CheckHasNodeError(returnMessages);
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while inheriting parent settings in batch, ex: {ex.Message}.");
                return ex.Message;
            }
        }

        private string CheckHasNodeError(List<RAReturnMessage> returnMessages)
        {
            var messagesFailed = returnMessages
                .Where(r => r.MessageType == RAMessageType.Failed && !string.IsNullOrWhiteSpace(r.ErrorMessage)) 
                .Select(r => r.ErrorMessage)
                .Distinct();
            return messagesFailed.Any() ? string.Join("; ", messagesFailed) : string.Empty;
        }

        private async Task HandleGoogleTenantsUnderTermGroup(List<RMGoogleTreeNode> settingNodes)
        {
            try
            {
                var isEnableRecordManagement = settingNodes.All(s => s.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable);
                if (!isEnableRecordManagement)
                    return;                

                var selectedNode = settingNodes.FirstOrDefault();
                var nodeIds = settingNodes.Select(s => s.Id).Where(d => !string.IsNullOrEmpty(d)).ToList();
               
                List<string> ggTenantsUnderNodes = selectedNode.Level switch
                {
                    (int)NodeLevel.GoogleMyDriveContainer or (int)NodeLevel.GoogleSharedDriveContainer =>
                      RemoteNodeDao.GetGoogleTenantIdsUnderNodes(nodeIds, NodeLevelExpressionType.ExpressionContainers),

                    (int)NodeLevel.GoogleMyDrive or (int)NodeLevel.GoogleMyDrive =>
                      RemoteNodeDao.GetGoogleTenantIdsUnderNodes(nodeIds, NodeLevelExpressionType.ExpressionGoogleDrive),

                    _ => new List<string>()
                };

                var selectedTermGroup = TermGroupDao.GetTermGroupByGuid(selectedNode.TermGroupId);
                var ggTenantsFromAOS = await RMAosApiClient.GetGoogleTenants(TenantLocalValue.LogonGroupId);
                var conflictTenants = await TermGroupMembershipDao.GetGoogleTenantsExisted(ggTenantsUnderNodes, selectedTermGroup.UniqueId);

                if (conflictTenants.IsNotNullOrEmpty() && conflictTenants.Count >= 1)
                {
                    var tenantList = string.Join(", ", conflictTenants.Keys);
                    var termGroupList = string.Join(", ", conflictTenants.Values);
                    Logger.Warn($"Google tenants [{tenantList}] already exist in other term group(s): [{termGroupList}].");
                    throw new RMGoogleTenantConflictException("GoogleOpusUI.error.selected-scope-conflict-term-groups"); //C+ I18N key
                }

                var memberships = ggTenantsFromAOS
                     .Where(d => ggTenantsUnderNodes.Contains(d.Key))
                     .Select(d => new RMTermGroupMembership
                     {
                         TermGroupId = selectedTermGroup.UniqueId,
                         AgentGroupId = d.Key,
                         DisplayName = d.Value,
                         SiteUrl = d.Key,
                         TermStoreName = d.Value,
                         TermStoreId = Guid.Empty,
                         SiteType = SiteType.Google
                     });

                foreach (var member in memberships)
                {
                    await TermGroupMembershipDao.AddGoogleTenantInTermGroupMembership(member);
                }

                selectedTermGroup.GoogleTermSyncOption = TermSyncOption.Specified;
                await TermGroupDao.UpdateAsync(selectedTermGroup);
                settingNodes.ForEach(s => s.IsNullClassificationSetting = false);
            }
            catch(RMGoogleTenantConflictException _)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while handling term group before saving setting: {ex.Message}", ex);
            }
        }

        private async Task HandleDisposalScheduleOnNode(RMGoogleTreeNode settingNode)
        {
            var profileId = ScheduleService.GetProfileId(settingNode);
            settingNode.IsNodeProcessFromGControl = true;
            var existingScheduleInfo = await ScheduleService.GetScheduleAsync(profileId, ScheduleType.GoogleDisposalSchedule);

            if (settingNode.DisposeScheduleInfo == null)
            {
                if (existingScheduleInfo != null)
                {
                    settingNode.DisposeScheduleInfo = existingScheduleInfo;
                    await DeleteDisposeSchedule(settingNode);
                }
            }
            else
            {
                ScheduleService.ConvertScheduleByTimezone(settingNode.DisposeScheduleInfo);
                if (existingScheduleInfo == null)
                {
                    await CreateDisposeSchedule(settingNode);
                    return;
                }

                await UpdateDisposeSchedule(settingNode);
            }
        }
        #endregion
    }
}