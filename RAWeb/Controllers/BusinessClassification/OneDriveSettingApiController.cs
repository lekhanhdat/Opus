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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.RuleManagement;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static AvePoint.RA.Contract.Object.RMSPTreeNode;
using System.Threading.Tasks;
using RMSPTreeNode = AvePoint.RA.Contract.Object.RMSPTreeNode;
using AvePoint.RA.Web.Common.Utils;
using Microsoft.AspNetCore.Http;

namespace AvePoint.RA.Web.Controllers.SharePointSettings
{
    [RMApiAuthorize(RMPermissionMasks.OneDriveEnduser, preferred: false)]
    public class OneDriveSettingApiController : BaseApiController
    {
        #region Interface
        private ISPSettingTreeService _RMSPTreeService;
        private ISPSettingTreeService RMSPTreeService => PlatformWindsorManager.GetService(ref _RMSPTreeService);
        private IRMOneDriveSettingsService _RMOneDriveSettingsService;
        private IRMOneDriveSettingsService RMOneDriveSettingsService => PlatformWindsorManager.GetService(ref _RMOneDriveSettingsService);
        private IRMSharePointSettingsService _RMSPSService;
        private IRMSharePointSettingsService RMSPSService => PlatformWindsorManager.GetService(ref _RMSPSService);
        private ITaxonomyService _TaxonomyService;
        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService(ref _TaxonomyService);
        private IScheduleService _ScheduleService;
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService(ref _ScheduleService);
      
        private ITenantService _TenantService;
        private ITenantService TenantService => PlatformWindsorManager.GetService(ref _TenantService);
        private IRMJobService _RMJobService;
        private IRMJobService RMJobService => PlatformWindsorManager.GetService(ref _RMJobService);
        private IManualProcessManagementService _ManualProcessManagementService;
        private IManualProcessManagementService ManualProcessManagementService => PlatformWindsorManager.GetService(ref _ManualProcessManagementService);
        private IBrowseTreeService _BrowseTreeService;
        private IBrowseTreeService BrowseTreeService => PlatformWindsorManager.GetService(ref _BrowseTreeService);

        private IRMSecurityTrimmingHelper _SecurityTrimmingHelper;
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService(ref _SecurityTrimmingHelper);

        private IRuleManagerService _RuleManagerService;
        private IRuleManagerService RuleManagerService => PlatformWindsorManager.GetService(ref _RuleManagerService);

        private IRMArchiverSettingsService _RMArchiverSettingsService;
        private IRMArchiverSettingsService RMArchiverSettingsService => PlatformWindsorManager.GetService(ref _RMArchiverSettingsService);
        #endregion


        #region Browse
        [RMApiAuthorize(RMPermissionMasks.OneDriveEnduser, RMSOPermissionMasks.OneDriveEnduser)]
        [ValidSampleTreeParameterFilter(ValidType = ValidType.OneDrive)]
        public Task<string> BrowseOneDriveTree([FromBody] RMSPSampleTreeNode node)
        {
            return BrowseSampleTreeAsync(node, RMBrowseTreeNodeSourceType.SkyDrivePro);
        }

        [ValidSampleTreeParameterFilter(ValidType = ValidType.OneDrive)]
        [RMApiAuthorize(RMPermissionMasks.OneDriveEnduser, RMSOPermissionMasks.OneDriveEnduser)]
        public async Task<RMSPSampleTreeNode> BrowseOneDriveTreePaged([FromBody] RMSPSampleTreeNode node)
        {
            node.SourceType = (int)SourceFlag.OneDrive;
            var returnNode = await BrowseTreeService.BrowseSPOTreeAsync(node, RMBrowseTreeNodeSourceType.SkyDrivePro, true);
            RMSPTreeService.TransChildrenNodeName(returnNode);
            if (node.IsArchiverTree)
            {
                RMArchiverSettingsService.LoadArchiverSettingIcon(returnNode.Children, ScheduleType.OneDriveArchiveJobSchedule);
            }
            else
            {
                await RMOneDriveSettingsService.LoadSettingIconAsync(returnNode.Children);
            }
            returnNode.Children?.ForEach(n => n.Parent = null);
            return returnNode;
        }

        [RMApiAuthorize(RMPermissionMasks.OneDriveEnduser, RMSOPermissionMasks.OneDriveEnduser)]
        public async Task<RMSPSampleTreeNode> SearchContainerByPage([FromBody] RMSPSampleTreeNode node)
        {
            return await BrowseTreeService.SearchContainerByPage(node, RMBrowseTreeNodeSourceType.SkyDrivePro, true);
        }

        [RMApiAuthorize(RMPermissionMasks.OneDriveEnduser, RMSOPermissionMasks.OneDriveEnduser)]
        public async Task<SearchSiteCollectionLazyLoadResponse> SearchSiteCollectionLazyLoad([FromBody] SearchSiteCollectionLazyLoadRequest condition)
        {
            condition.SourceFlag = (int)SourceFlag.OneDrive;
            var response = await BrowseTreeService.SearchSiteCollectionLazyLoad(condition, checkPermission: true);
            RMSPTreeService.TransChildrenNodeName(new RMSPSampleTreeNode { Children = response.Children });
            if (condition.IsArchiverTree)
            {
                RMArchiverSettingsService.LoadArchiverSettingIcon(response.Children, ScheduleType.OneDriveArchiveJobSchedule);
            }
            else
            {
                await RMOneDriveSettingsService.LoadSettingIconAsync(response.Children);
            }
            response.Children?.ForEach(n => n.Parent = null);
            return response;
        }

        [RMApiAuthorize(RMPermissionMasks.SPOEnduser | RMPermissionMasks.OneDriveEnduser, RMPermissionExtensionMasks.TeamsEndUser, RMSOPermissionMasks.SPOEnduser | RMSOPermissionMasks.OneDriveEnduser | RMSOPermissionMasks.TeamsEndUser, DB.SecurityTrimming.Model.PermissionJoinType.Any)]
        [ValidSampleTreeParameterFilter(ValidType = ValidType.OneDrive)]
        public Task<string> BrowseAllTree([FromBody] RMSPSampleTreeNode node)
        {
            if (node.IsTeams)
            {
                return BrowseSampleTreeAsync(node, RMBrowseTreeNodeSourceType.Teams);
            }
            else
            {
                if (node.IsEnableTeams)
                {
                    return BrowseSampleTreeAsync(node, RMBrowseTreeNodeSourceType.SPAndOD);
                }
                else
                {
                    return BrowseSampleTreeAsync(node, RMBrowseTreeNodeSourceType.All);
                }
            }
        }

        private async Task<string> BrowseSampleTreeAsync(RMSPSampleTreeNode node, RMBrowseTreeNodeSourceType type)
        {
            string result = string.Empty;
            string name = string.Empty;
            try
            {
                using (new RA.Common.PerformanceScope("BrowseOneDriveTree"))
                {
                    List<RMSPSampleTreeNode> children = new List<RMSPSampleTreeNode>();
                    name = node.Name;
                    using (new RA.Common.PerformanceScope($"Browse tree under node: {name}."))
                    {
                        children = await RMSPTreeService.BrowseSampleTreeAsync(node, true, type, true, node.IsArchiverTree);

                        foreach (var child in children)
                        {
                            if (child.BposInfo == null || child.BposInfo.UserAccountInfo == null)
                            {
                                continue;
                            }
                            var accountInfo = child.BposInfo.UserAccountInfo;
                            accountInfo.Domain = string.Empty;
                            accountInfo.Username = string.Empty;
                            accountInfo.AppId = string.Empty;
                            accountInfo.AppClientId = string.Empty;
                            accountInfo.AppCertSecret = string.Empty;
                            //accountInfo.AppCertContent = string.Empty;
                            accountInfo.AppCertSecretContent = string.Empty;
                        }
                    }
                    using (new RA.Common.PerformanceScope("Load tree node setting icon."))
                    {
                        await RMOneDriveSettingsService.LoadSettingIconAsync(children);
                    }

                    using (new RA.Common.PerformanceScope("Serialize tree nodes."))
                    {
                        result = JsonConvert.SerializeObject(children);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred when browe node.NodeName:[{0}] Error:{1}", name, e.ToString());
                throw;
            }
            return result;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.OneDriveEnduser, RMSOPermissionMasks.OneDriveEnduser)]
        public string GetSPDesignLists()
        {
            var lists = RMSPSService.GetDesignLists();
            return JsonConvert.SerializeObject(lists);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.OneDriveEnduser, RMSOPermissionMasks.OneDriveEnduser)]
        public string GetSPTreeInitData()
        {
            var farmNode = RMSPTreeService.LoadFarm()[0];
            if (farmNode == null || string.IsNullOrEmpty(farmNode.Id))
            {
                Logger.Warn("Farm node is null.Please refresh page.");
            }
            else
            {
                if (farmNode.Children != null)
                {
                    farmNode.Children = null;
                }
            }
            return JsonConvert.SerializeObject(farmNode);
        }
        #endregion

        #region Load & Save Node Settings

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.OneDrive)]
        public async Task<string> LoadSampleNodeSettings([FromBody] RMSPTreeNode node)
        {
            var settings = await RMOneDriveSettingsService.LoadNodeSettingAsync(node);
            if (settings.ApprovalType == (int)ApprovalType.ApprovalProcess)
            {
                var result = Guid.TryParse(settings.WorkflowReferenceId, out var referenceId);
                if (result)
                {
                    var workflow = ManualProcessManagementService.GetWorkflow(referenceId);
                    settings.WorkflowReferenceName = workflow?.Name;
                }
            }
            return JsonConvert.SerializeObject(settings);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.OneDrive)]
        [RMApiAuthorize(RMSOPermissionMasks.OneDriveEnduser)]
        public string LoadArchiverNodeSettings([FromBody] RMSPSampleTreeNode node)
        {
            ScheduleType type = ScheduleType.OneDriveArchiveJobSchedule;
            var settings = RMArchiverSettingsService.LoadSampleNodeSettings(node, type);
            return JsonConvert.SerializeObject(settings);
        }
        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.OneDrive)]
        [RMApiAuthorize(RMSOPermissionMasks.OneDriveEnduser)]
        public async Task<List<RMRuleInfos>> LoadArchiverRules([FromBody] string containerId)
        {
            var settings = await RMArchiverSettingsService.GetArchiverRuleListAsync(containerId, SourceFlag.OneDrive);
            return settings;
        }
        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.OneDrive)]
        public async Task<string> SaveEnableColumnSetting([FromBody] RMSPTreeNode curSetting)
        {
            var result = await RMOneDriveSettingsService.AddEnableRecordsManagementSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.OneDrive)]
        public async Task<string> SaveIsShowUniquedIdSetting([FromBody] RMSPTreeNode curSetting)
        {
            var result = await RMOneDriveSettingsService.AddIsShowUniqueIdSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.OneDrive, Action = "ValidateSaveOneDriveTermSetting")]
        public async Task<string> SaveDocumentLevelSetting([FromBody] RMSPTreeNode curSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                var syncUserResult = await RMSPSService.SyncADUsersAsync(curSetting.AIReviewers);
                if (syncUserResult.MessageType == RAMessageType.Successful)
                {
                    if (!curSetting.IsNullClassificationSetting && !curSetting.DefaultTermId.Equals(Guid.Empty) && TaxonomyService.IsOrphanedTerm(curSetting.DefaultTermId))
                    {
                        result.FaildType = RAFailedType.DefaultTermIsOrphaned;
                        result.MessageType = RAMessageType.Failed;
                    }
                    else
                    {
                        result = await RMOneDriveSettingsService.AddTermSettingAsync(curSetting);
                        if (curSetting.Rules?.Count == 0 && curSetting.IsNullClassificationSetting)
                        {
                            curSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.Disable;
                            await RMOneDriveSettingsService.AddOneDriveGeneralSettingAsync(curSetting);
                        }
                    }
                }
                else
                {
                    result.MessageType = RAMessageType.Failed;
                    result.ErrorMessage = syncUserResult.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                result.MessageType = RAMessageType.Failed;
                Logger.Error("Save SharePoint Settings Failed.ERROR:{0}", ex.Message);
            }
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.OneDrive, Action = "ValidateSaveSPTermSetting")]
        public async Task<string> SaveLoactionOwners([FromBody] RMSPTreeNode curSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            var syncUserResult = await RMSPSService.SyncADUsersAsync(curSetting.RecordOwner);
            if (syncUserResult.MessageType == RAMessageType.Successful)
            {
                result = await RMOneDriveSettingsService.AddLocationOwnersAsync(curSetting);
            }
            else
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = syncUserResult.ErrorMessage;
            }
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.OneDrive)]
        public async Task<string> InheritParentSettings([FromBody] RMSPTreeNode curSetting)
        {
            var result = await RMOneDriveSettingsService.InheritParentSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }
        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.OneDrive)]
        [RMApiAuthorize(RMSOPermissionMasks.OneDriveEnduser)]
        public async Task<string> InheritParentArchiverSettings([FromBody] RMSPTreeNode curSetting)
        {
            curSetting.Type = ContentSourceType.OneDrive;
            var result = await RMArchiverSettingsService.InheritParentSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.OneDrive)]
        [RMApiAuthorize(RMSOPermissionMasks.OneDriveEnduser)]
        public async Task<string> InheritSubNodeToCurrentSettings([FromBody] RMSPTreeNode curSetting)
        {
            curSetting.Type = ContentSourceType.OneDrive;
            var result = await RMArchiverSettingsService.InheritSubNodeToCurrentAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.OneDrive)]
        public async Task<string> SaveGeneralSetting([FromBody] RMSPTreeNode curSetting)
        {
            var result = await RMOneDriveSettingsService.AddOneDriveGeneralSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidPermissionFilter(RMPermissionMasks.RuleManagementEnduser)]
        public async Task<List<RMRuleInfos>> GetAvailableRuleList([FromBody] string containerId)
        {
            List<RMRuleInfos> listRuleFromDA = new List<RMRuleInfos>();
            List<RMRuleInfos> availableRules = new List<RMRuleInfos>();
            try
            {
                Logger.Info("Get OneDrive Rules from Records ");
                using (PerformanceScope scope = new PerformanceScope("setting rules"))
                {
                    var securityGroupIds = SecurityTrimmingHelper.GetSecurityGroupsByContentScope(new List<string> { containerId }, SourceFlag.OneDrive);
                    var ruleContainerIds = SecurityTrimmingHelper.GetRuleScopeBySecurityGroupIds(securityGroupIds);
                    listRuleFromDA = await RuleManagerService.GetRulesByDataSourceAsync((int)SourceFlag.OneDrive, ruleContainerIds);
                    var associateAvailableRule = await RuleManagerService.GetSimpleRulesFromDBAsync(ruleContainerIds);
                    var availableRuleIds = associateAvailableRule.Select(r => r.RuleId).ToList();
                    availableRules = listRuleFromDA.Where(r => availableRuleIds.Contains(r.RuleId)).ToList();
                }
                Logger.Info("Rule count {0}", listRuleFromDA.Count);
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred while get rules:{0}", ex.ToString());
            }

            return availableRules;
        }
        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.OneDrive)]
        [RMApiAuthorize(RMSOPermissionMasks.OneDriveEnduser)]
        public async Task<string> SaveArchiverNodeSetting([FromBody] RMSPTreeNode setting)
        {
            setting.Type = ContentSourceType.OneDrive;
            var result = await RMArchiverSettingsService.SaveArchiverSettingAsync(setting);
            if (setting.Rules?.Count == 0)
            {
                setting.EnableArchiverManagement = (int)EnableRecordManagementSetting.Disable;
                await RMArchiverSettingsService.SaveGeneralSettingAsync(setting);
            }
            return JsonConvert.SerializeObject(result);
        }
        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.OneDrive)]
        [RMApiAuthorize(RMSOPermissionMasks.OneDriveEnduser)]
        public async Task<string> SaveArchiverGeneralSetting([FromBody] RMSPTreeNode curSetting)
        {
            curSetting.Type = ContentSourceType.OneDrive;
            var result = await RMArchiverSettingsService.SaveGeneralSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }
        #endregion

        #region Dispose Schedule
        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.OneDrive)]
        [RMApiAuthorize(RMPermissionMasks.OneDriveEnduser, RMSOPermissionMasks.OneDriveEnduser)]
        public async Task<string> UpdateDisposeSchedule([FromBody] RMSPTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                string objectId = Guid.Empty.ToString();
                if (nodeSetting.Level != (int)NodeLevel.WebApplication)
                {
                    RMSPTreeNode siteCollectionNode = RMOneDriveSettingsService.GetSiteCollectionNode(nodeSetting);
                    objectId = siteCollectionNode.SPObjectId;
                }
                if (!RMOneDriveSettingsService.CheckParentNodeDisable(nodeSetting, objectId))
                {

                    var cloneNodeInfo = nodeSetting.Clone();
                    cloneNodeInfo.DisposeScheduleInfo = null;
                    cloneNodeInfo.SkipRemoveContentAndDestroyAction = nodeSetting.DisposeScheduleInfo.Extentions.Equals("true", StringComparison.OrdinalIgnoreCase);
                    nodeSetting.DisposeScheduleInfo.Extentions = JsonConvert.SerializeObject(cloneNodeInfo);
                    var schedule = await ScheduleService.UpdateScheduleServiceAsync(nodeSetting.DisposeScheduleInfo, GetNodeFullPath(nodeSetting));
                    if (schedule == "-1")
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.FaildType = RAFailedType.ScheduleServiceFailed;
                    }
                    //else
                    //{
                    //    mRMSPSettingsService.AddNodeSettingDisposeSchedule(nodeSetting);
                    //}
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
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.OneDrive)]
        [RMApiAuthorize(RMPermissionMasks.OneDriveEnduser, RMSOPermissionMasks.OneDriveEnduser)]
        public async Task<string> CreateDisposeSchedule([FromBody] RMSPTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                string objectId = Guid.Empty.ToString();
                if (nodeSetting.Level != (int)NodeLevel.WebApplication)
                {
                    RMSPTreeNode siteCollectionNode = RMOneDriveSettingsService.GetSiteCollectionNode(nodeSetting);
                    objectId = siteCollectionNode.SPObjectId;
                }
                if (!RMOneDriveSettingsService.CheckParentNodeDisable(nodeSetting, objectId))
                {
                    nodeSetting.DisposeScheduleInfo.Id = Guid.NewGuid().ToString();
                    var cloneNodeInfo = nodeSetting.Clone();
                    cloneNodeInfo.DisposeScheduleInfo = null;
                    cloneNodeInfo.SkipRemoveContentAndDestroyAction = nodeSetting.DisposeScheduleInfo.Extentions.Equals("true", StringComparison.OrdinalIgnoreCase);
                    nodeSetting.DisposeScheduleInfo.Extentions = JsonConvert.SerializeObject(cloneNodeInfo);
                    nodeSetting.DisposeScheduleInfo.ProfileId = ScheduleService.GetProfileId(nodeSetting);
                    var schedule = await ScheduleService.CreateScheduleServiceAsync(nodeSetting.DisposeScheduleInfo, true, GetNodeFullPath(nodeSetting));
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
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.OneDrive)]
        [RMApiAuthorize(RMPermissionMasks.OneDriveEnduser, RMSOPermissionMasks.OneDriveEnduser)]
        public string DeleteDisposeSchedule([FromBody] RMSPTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                string objectId = Guid.Empty.ToString();
                if (nodeSetting.Level != (int)NodeLevel.WebApplication)
                {
                    RMSPTreeNode siteCollectionNode = RMOneDriveSettingsService.GetSiteCollectionNode(nodeSetting);
                    objectId = siteCollectionNode.SPObjectId;
                }
                if (!RMOneDriveSettingsService.CheckParentNodeDisable(nodeSetting, objectId))
                {
                    ScheduleService.DeleteScheduleService(nodeSetting.DisposeScheduleInfo.Id, GetNodeFullPath(nodeSetting));
                    //mRMSPSettingsService.AddNodeSettingDisposeSchedule(nodeSetting, true);
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
                Logger.Error("Delete Collection Schedule Service Failed.ERROR:{0}", ex.Message);
                throw;
            }
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.OneDrive)]
        [RMApiAuthorize(RMPermissionMasks.OneDriveEnduser, RMSOPermissionMasks.OneDriveEnduser)]
        public string BreakDisposeSchedule([FromBody] RMSPTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                string objectId = Guid.Empty.ToString();
                if (nodeSetting.Level != (int)NodeLevel.WebApplication)
                {
                    RMSPTreeNode siteCollectionNode = RMOneDriveSettingsService.GetSiteCollectionNode(nodeSetting);
                    objectId = siteCollectionNode.SPObjectId;
                }
                if (!RMOneDriveSettingsService.CheckParentNodeDisable(nodeSetting, objectId))
                {
                    nodeSetting.DisposeScheduleInfo.Id = "";
                    ScheduleService.CreateNoSchedule(SettingScheduleType.OneDriveDisposal, GetNodeFullPath(nodeSetting));
                    //mRMSPSettingsService.AddNodeSettingDisposeSchedule(nodeSetting);
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
                Logger.Error("Break Collection Schedule Service Failed.ERROR:{0}", ex.Message);
                throw;
            }
            return JsonConvert.SerializeObject(result);
        }

        #endregion

        #region Tool Method

        public string GetNodeFullPath(RMSPTreeNode node)
        {
            if (node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.WebApplication)
            {
                return node.FullPath;
            }
            return WebUtil.MakeFullUrl(node.GetSiteCollectionNode().FullPath, node.FullPath);
        }

        #endregion

        #region Run Job
        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.OneDriveEnduser)]
        [ValidSPParameterActionFilter(ValidType = ValidType.OneDrive)]
        public string RunArchiverJob([FromBody] RMSPTreeNode selectedTree)
        {
            return JsonConvert.SerializeObject(RMArchiverSettingsService.RunArchiverJob(selectedTree, JobRunBy.Control));
        }
        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.OneDriveEnduser)]
        public string RunArchiverJobForImport([FromForm] IFormFile fileUp, [FromForm] string selectedTree)
        {
            try
            {
                var node = JsonConvert.DeserializeObject<RMSPTreeNode>(selectedTree);
                if (fileUp != null && node.Level == (int)NodeLevel.WebApplication)
                {
                    string fileName = fileUp.FileName;
                    Logger.Info("od archiver import sites url file,file name :{0}.", fileName);
                    string extension = fileName.Substring(fileName.LastIndexOf(".") + 1);
                    if (extension.Equals("csv", StringComparison.OrdinalIgnoreCase))
                    {
                        node.ArchiverImportSitesUrl = ApiMessageUtil.GetArchiverImportSitesUrl(fileUp);
                        node.UserArchiverImportFile = true;
                        return JsonConvert.SerializeObject(RMArchiverSettingsService.RunArchiverJob(node, JobRunBy.Control));
                    }
                }
                Logger.Info($"od:failed run archive import site job,node full path:{node?.FullPath}");
                return JsonConvert.SerializeObject(new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_JM_Details_Failed_UnexpectedError") });
            }
            catch (Exception e)
            {
                Logger.Error($"some thing went wrong when RunArchiverJobForImport,error:{e.ToString()}");
                return JsonConvert.SerializeObject(new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_JM_Details_Failed_UnexpectedError") });
            }
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.OneDriveEnduser)]
        [ValidSPParameterActionFilter(ValidType = ValidType.OneDrive)]
        public string RunSOPreScanJob([FromBody] RMSPTreeNode selectedTree)
        {
            return JsonConvert.SerializeObject(RMArchiverSettingsService.RunODPreScanJobWrapper(selectedTree, JobRunBy.Control));
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.OneDrive)]
        public string RunCollectionJob([FromBody] RMSPTreeNode selectedTree)
        {
            return JsonConvert.SerializeObject(RMOneDriveSettingsService.RunDataSyncJob(selectedTree, JobRunBy.Control));
        }

        [HttpPost]
        public string RunSPSyncDataJob([FromBody] bool fromTimerJobPage)
        {
            return JsonConvert.SerializeObject(RMOneDriveSettingsService.RunDataSyncJob(null, JobRunBy.Control));
        }

        [HttpPost]
        [ValidSPTreeParameterFilter(ValidType = ValidType.OneDrive)]
        public async Task<RAReturnMessage> RunJob([FromBody] string node)
        {
            RMSPTreeNode selectedNode = null;
            try
            {
                selectedNode = SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(node);
                //var selectedNode = SPTreeCacheUtil.GetNodeById(spObjectId, RAModule.Common);
                Logger.Info("Run job Node FullPath:[{0}].", selectedNode?.FullPath);
                if (TenantService.IsNewOpusTenant())
                {
                    return RMOneDriveSettingsService.RunRecordsDisposalJob(selectedNode, JobRunBy.Control);
                }
                else
                {
                    return await RMJobService.RunOneDriveNowAsync(selectedNode, JobRunBy.Control);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Failed to run OneDrive job. NodeSPObjectId:[{0}] Error:{1}.", selectedNode?.SPObjectId, e.ToString());
                throw;
            }
        }
        #endregion

        #region Term
        /// <summary>
        /// 编辑setting时，还原已经保存的tree结构
        /// 
        /// 此处算法：保存的只是一个展开的并被选择的节点，其他展开的节点不被保存。根据此节点反查各层父级节点。展示出tree。展示tree的过程中会
        /// 把被选中的节点的兄弟节点显示出来
        /// </summary>
        /// <param name="settingInfo"></param>
        /// <returns></returns>       
        [HttpPost]
        [RACodeReview("Allen Yin")]
        [ValidSPCurrentSettingParameterActionFilter(ValidType = ValidType.OneDrive)]
        public Task<string> GetSavedTree([FromBody] CurrentSettingsInfo settingInfo)
        {
            return TaxonomyService.GetOneDriveSettingSavedTreeAsync(settingInfo, true);
        }
        #endregion

    }
}