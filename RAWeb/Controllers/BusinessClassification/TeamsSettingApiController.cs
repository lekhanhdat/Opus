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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Teams;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.RMUniqueIdSettings;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.Utils;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AvePoint.RA.Web.Controllers.BusinessClassification
{
    [RMApiAuthorize(RMPermissionExtensionMasks.TeamsEndUser, preferred: false)]
    public class TeamsSettingApiController : BaseApiController
    {
        #region interface
        private ITeamsSettingTreeService _teamSettingTreeService;
        private ITeamsSettingTreeService TeamsSettingTreeService => PlatformWindsorManager.GetService(ref _teamSettingTreeService);

        private IBrowseTreeService _browseTreeService;
        private IBrowseTreeService BrowseTreeService => PlatformWindsorManager.GetService(ref _browseTreeService);

        private IRMTeamsSettingsService _teamsSettingsService;
        private IRMTeamsSettingsService RMTeamsSettingsService => PlatformWindsorManager.GetService(ref _teamsSettingsService);

        private IRMArchiverSettingsService _RMArchiverSettingsService;
        private IRMArchiverSettingsService RMArchiverSettingsService => PlatformWindsorManager.GetService(ref _RMArchiverSettingsService);

        private ITenantService _TenantService;
        private ITenantService TenantService => PlatformWindsorManager.GetService(ref _TenantService);

        private IScheduleService _ScheduleService;
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService(ref _ScheduleService);

        private IManualProcessManagementService _ManualProcessManagementService;
        private IManualProcessManagementService ManualProcessManagementService => PlatformWindsorManager.GetService(ref _ManualProcessManagementService);

        private ITaxonomyService _TaxonomyService;
        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService(ref _TaxonomyService);
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();


        private IUniqueIdSettingService _UniqueIdSettingService;
        private IUniqueIdSettingService UniqueIdSettingService => PlatformWindsorManager.GetService(ref _UniqueIdSettingService);

        private IStubSettingService _StubSettingSerivce;
        private IStubSettingService StubSettingService => PlatformWindsorManager.GetService(ref _StubSettingSerivce);

        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();

        private ISPSettingTreeService _SPSettingTreeService;
        private ISPSettingTreeService SPSettingTreeService => PlatformWindsorManager.GetService(ref _SPSettingTreeService);
        #endregion

        [HttpPost]
        //[ValidSPParameterActionFilter(ValidType = ValidType.SharePointOnline)]
        [ValidAccountHasTeamsPermissionFilter]
        public string RunCollectionJob([FromBody] RMSPTreeNode selectedTree)
        {
            return JsonConvert.SerializeObject(RMTeamsSettingsService.RunDataSyncJob(selectedTree, JobRunBy.Control));
        }

        [HttpPost]
        [ValidAccountHasTeamsPermissionFilter]
        public string RunTeamsSyncDataJob([FromBody] bool fromTimerJobPage)
        {
            return JsonConvert.SerializeObject(RMTeamsSettingsService.RunDataSyncJob(null, JobRunBy.Control));
        }

        [HttpGet]
        [ValidAccountHasTeamsPermissionFilter]
        [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess, RMSOPermissionMasks.CommonModuleAccess)]
        public bool CheckRemoteNodesIsInit()
        {
            return TenantService.GetTenantInitNodeState(TenantLocalValue.LogonGroupId) == Contract.Aos.Notification.RMInitNodeState.Synced;
        }

        [HttpPost]
        [ValidAccountHasTeamsPermissionFilter]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser, RMPermissionExtensionMasks.TeamsEndUser)]
        public string GetTeamsTreeInitData()
        {
            var farmNode = TeamsSettingTreeService.LoadFarm()[0];
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

        [ValidSampleTreeParameterFilter(ValidType = ValidType.Teams)]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser, RMPermissionExtensionMasks.TeamsEndUser)]
        public async Task<RMSPSampleTreeNode> BrowseSampleTree([FromBody] RMSPSampleTreeNode node)
        {
            node.SourceType = (int)SourceFlag.Teams;
            var returnNode = await BrowseTreeService.BrowseSPOTreeAsync(node, RMBrowseTreeNodeSourceType.Teams, true);
            TeamsSettingTreeService.TransChildrenNodeName(returnNode);
            if (node.IsArchiverTree)
            {
                RMArchiverSettingsService.LoadArchiverSettingIcon(returnNode.Children, ScheduleType.TeamsArchiveJobSchedule);
            }
            else
            {
                await RMTeamsSettingsService.LoadTeamsSettingIconAsync(returnNode.Children);
            }
            returnNode.Children?.ForEach(n => n.Parent = null);
            return returnNode;
        }

        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser, RMPermissionExtensionMasks.TeamsEndUser)]
        public async Task<RMSPSampleTreeNode> SearchContainerByPage([FromBody] RMSPSampleTreeNode node)
        {
            var returnNode = await BrowseTreeService.SearchContainerByPage(node, RMBrowseTreeNodeSourceType.Teams, true);
            TeamsSettingTreeService.TransChildrenNodeName(returnNode);
            if (node.IsArchiverTree)
            {
                RMArchiverSettingsService.LoadArchiverSettingIcon(returnNode.Children, ScheduleType.TeamsArchiveJobSchedule);
            }
            else
            {
                await RMTeamsSettingsService.LoadTeamsSettingIconAsync(returnNode.Children);
            }
            returnNode.Children?.ForEach(n => n.Parent = null);
            return returnNode;

        }

        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser, RMPermissionExtensionMasks.TeamsEndUser)]
        public async Task<SearchSiteCollectionLazyLoadResponse> SearchSiteCollectionLazyLoad([FromBody] SearchSiteCollectionLazyLoadRequest condition)
        {
            condition.SourceFlag = (int)SourceFlag.Teams;
            var response = await BrowseTreeService.SearchSiteCollectionLazyLoad(condition, checkPermission: true);
            TeamsSettingTreeService.TransChildrenNodeName(response);

            var nodeGroups = response.Children?
                .GroupBy(n => n?.Parent?.Id)
                .Select(g => g.ToList())
                .ToList();

            if (nodeGroups == null || nodeGroups.Count == 0)
            {
                return response;
            }

            if (condition.IsArchiverTree)
            {
                foreach (var group in nodeGroups)
                {
                    RMArchiverSettingsService.LoadArchiverSettingIcon(group, ScheduleType.TeamsArchiveJobSchedule);
                }
            }
            else
            {
                foreach (var group in nodeGroups)
                {
                    await RMTeamsSettingsService.LoadTeamsSettingIconAsync(group);
                }
            }
            return response;
        }

        [RMApiAuthorize(RMPermissionMasks.SPOEnduser | RMPermissionMasks.OneDriveEnduser, RMPermissionExtensionMasks.TeamsEndUser, RMSOPermissionMasks.SPOEnduser | RMSOPermissionMasks.OneDriveEnduser | RMSOPermissionMasks.TeamsEndUser, DB.SecurityTrimming.Model.PermissionJoinType.Any)]
        [ValidSampleTreeParameterFilter(ValidType = ValidType.Teams)]
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
                using (new RA.Common.PerformanceScope("BrowseSampleTree"))
                {
                    List<RMSPSampleTreeNode> children = new List<RMSPSampleTreeNode>();

                    name = node.Name;
                    using (new RA.Common.PerformanceScope($"Browse tree under node: {name}. type: {type}"))
                    {
                        children = await SPSettingTreeService.BrowseSampleTreeAsync(node, true, type, true, node.IsArchiverTree);
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
                        await RMTeamsSettingsService.LoadTeamsSettingIconAsync(children);
                    }
                    using (new RA.Common.PerformanceScope("Serialize tree nodes."))
                    {
                        result = JsonConvert.SerializeObject(children);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred when browe node.NodeName:[{0}], type: [{1}],  Error:{2}", name, type, e.ToString());
                throw;
            }
            return result;
        }

        [HttpPost]
        [ValidAccountHasTeamsPermissionFilter]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser, RMPermissionExtensionMasks.TeamsEndUser)]
        public string GetSPDesignLists()
        {
            var lists = RMTeamsSettingsService.GetDesignLists();
            return JsonConvert.SerializeObject(lists);
        }

        [HttpPost]
        [ValidAccountHasTeamsPermissionFilter]
        [ValidSPSampleTreeParameterFilter(ValidType = ValidType.Teams)]
        public async Task<string> LoadSampleNodeSettings([FromBody] RMSPSampleTreeNode node)
        {
            var settings = await RMTeamsSettingsService.LoadSampleNodeSettingsAsync(node);
            if (settings.ApprovalType == (int)ApprovalType.ApprovalProcess)
            {
                var result = Guid.TryParse(settings.WorkflowReferenceId, out var referenceId);
                if (result)
                {
                    var workflow = ManualProcessManagementService.GetWorkflow(referenceId);
                    settings.WorkflowReferenceName = workflow?.Name;
                }
            }
            //if (settings.AIApprovalType == (int)ApprovalType.ApprovalProcess)
            //{
            //    settings.AIWorkflowReferenceName = ManualProcessManagementService.GetWorkflow(new Guid(settings.AIWorkflowReferenceId))?.Name;
            //}
            return JsonConvert.SerializeObject(settings);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.Teams)]
        public async Task<string> SaveColumnSetting([FromBody] RMSPTreeNode curSetting)
        {
            var result = await RMTeamsSettingsService.AddColumnSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.Teams)]
        public async Task<string> SaveColumnSettingExistColumn([FromBody] RMSPTreeNode curSetting)
        {
            var result = await RMTeamsSettingsService.AddUsingExistColumnSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.Teams)]
        public async Task<string> SaveGeneralSetting([FromBody] RMSPTreeNode curSetting)
        {
            var result = await RMTeamsSettingsService.AddGeneralSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.Teams)]
        public async Task<string> SaveGroupLevelSetting([FromBody] RMSPTreeNode curSetting)
        {
            RAReturnMessage result = new();
            var syncUserResult = await RMTeamsSettingsService.SyncADUsersAsync(curSetting.AIReviewers);
            if (syncUserResult.MessageType == RAMessageType.Successful)
            {
                result = await RMTeamsSettingsService.AddGlobalColumnAsync(curSetting);
            }
            else
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = syncUserResult.ErrorMessage;
            }
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.Teams)]
        public async Task<string> SaveContainerLevelSetting([FromBody] RMSPTreeNode curSetting)
        {
            var result = await RMTeamsSettingsService.AddContainerTermAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.Teams, Action = "ValidateSaveTeamsTermSetting")]
        public async Task<string> SaveLoactionOwners([FromBody] RMSPTreeNode curSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            var syncUserResult = await RMTeamsSettingsService.SyncADUsersAsync(curSetting.RecordOwner);
            if (syncUserResult.MessageType == RAMessageType.Successful)
            {
                result = await RMTeamsSettingsService.AddLocationOwnersAsync(curSetting);
            }
            else
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = syncUserResult.ErrorMessage;
            }
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPCurrentSettingParameterActionFilter(ValidType = ValidType.Teams)]
        public Task<string> GetSavedTree([FromBody] CurrentSettingsInfo settingInfo)
        {
            return TaxonomyService.GetTeamsSettingSavedTreeAsync(settingInfo, true);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.Teams)]
        public async Task<string> SaveDocumentLevelSetting([FromBody] RMSPTreeNode curSetting)
        {
            RAReturnMessage result = new();
            try
            {
                var syncUserResult = await RMTeamsSettingsService.SyncADUsersAsync(curSetting.AIReviewers);
                if (syncUserResult.MessageType == RAMessageType.Successful)
                {
                    if (!curSetting.DefaultTermId.Equals(Guid.Empty) && TaxonomyService.IsOrphanedTerm(curSetting.DefaultTermId))
                    {
                        result.FaildType = RAFailedType.DefaultTermIsOrphaned;
                        result.MessageType = RAMessageType.Failed;
                    }
                    else
                    {
                        result = await RMTeamsSettingsService.AddCustomColumnAsync(curSetting);
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
        [ValidAccountHasTeamsPermissionFilter]
        [ValidSPSampleTreeParameterFilter(ValidType = ValidType.Teams)]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser)]
        public string LoadArchiverNodeSettings([FromBody] RMSPSampleTreeNode node)
        {
            ScheduleType type = ScheduleType.TeamsArchiveJobSchedule;
            var settings = RMArchiverSettingsService.LoadSampleNodeSettings(node, type);
            return JsonConvert.SerializeObject(settings);
        }

        [HttpPost]
        [ValidAccountHasTeamsPermissionFilter]
        [ValidSPSampleTreeParameterFilter(ValidType = ValidType.Teams)]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser)]
        public async Task<List<RMRuleInfos>> LoadArchiverRules([FromBody] string containerId)
        {
            var teamsRules = await RMArchiverSettingsService.GetArchiverRuleListAsync(containerId, SourceFlag.Teams);
            return teamsRules;
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.Teams)]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser)]
        public async Task<string> SaveArchiverNodeSetting([FromBody] RMSPTreeNode setting)
        {
            setting.Type = ContentSourceType.Teams;
            var result = await RMArchiverSettingsService.SaveArchiverSettingAsync(setting);
            if (setting.Rules?.Count == 0)
            {
                setting.EnableArchiverManagement = (int)EnableRecordManagementSetting.Disable;
                await RMArchiverSettingsService.SaveGeneralSettingAsync(setting);
            }
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.Teams)]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser)]
        public async Task<string> SaveArchiverGeneralSetting([FromBody] RMSPTreeNode curSetting)
        {
            curSetting.Type = ContentSourceType.Teams;
            var result = await RMArchiverSettingsService.SaveGeneralSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser)]
        [ValidSPParameterActionFilter(ValidType = ValidType.Teams)]
        public string RunArchiverJob([FromBody] RMSPTreeNode selectedTree)
        {
            return JsonConvert.SerializeObject(RMArchiverSettingsService.RunTeamsArchiverJob(selectedTree, JobRunBy.Control));
        }

        [HttpPost]
        [ValidAccountHasTeamsPermissionFilter]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser)]
        public string RunArchiverJobForImport([FromForm] IFormFile fileUp, [FromForm] string selectedTree)
        {
            try
            {
                var node = JsonConvert.DeserializeObject<RMSPTreeNode>(selectedTree);
                if (fileUp != null && node.Level == (int)NodeLevel.WebApplication)
                {
                    string fileName = fileUp.FileName;
                    Logger.Info("teams archiver import teams email file,file name :{0}.", fileName);
                    string extension = fileName.Substring(fileName.LastIndexOf(".") + 1);
                    if (extension.Equals("csv", StringComparison.OrdinalIgnoreCase))
                    {
                        node.ArchiverImportSitesUrl = ApiMessageUtil.GetArchiverImportTeamsEmailAddress(fileUp);
                        node.UserArchiverImportFile = true;
                        return JsonConvert.SerializeObject(RMArchiverSettingsService.RunTeamsArchiverJob(node, JobRunBy.Control));
                    }
                }
                Logger.Info($"sp:failed run archive import site job,node full path:{node?.FullPath}");
                return JsonConvert.SerializeObject(new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_JM_Details_Failed_UnexpectedError") });
            }
            catch (Exception e)
            {
                Logger.Error($"some thing went wrong when RunArchiverJobForImport,error:{e.ToString()}");
                return JsonConvert.SerializeObject(new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_JM_Details_Failed_UnexpectedError") });
            }
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.Teams)]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser)]
        public async Task<string> InheritSubNodeToCurrentSettings([FromBody] RMSPTreeNode curSetting)
        {
            curSetting.Type = ContentSourceType.Teams;
            var result = await RMArchiverSettingsService.InheritSubNodeToCurrentAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.Teams)]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser)]
        public async Task<string> InheritParentArchiverSettings([FromBody] RMSPTreeNode curSetting)
        {
            curSetting.Type = ContentSourceType.Teams;
            var result = await RMArchiverSettingsService.InheritParentSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.Teams)]
        public async Task<string> InheritParentSettings([FromBody] RMSPTreeNode curSetting)
        {
            var result = await RMTeamsSettingsService.InheritParentSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.Teams)]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser)]
        public bool CheckRemoteNodeHaveRunningJob([FromBody] RMSPTreeNode selectedTree)
        {
            return RMArchiverSettingsService.CheckTeamsRemoteNodeHaveRunningJob(selectedTree);
        }

        #region schedule config

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.Teams)]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser, RMPermissionExtensionMasks.TeamsEndUser)]
        public async Task<string> CreateDisposeSchedule([FromBody] RMSPTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                string teamsId = string.Empty;
                string siteId = string.Empty;
                if (nodeSetting.Level != (int)NodeLevel.WebApplication)
                {
                    var teamNode = nodeSetting.GetTeamsNode();
                    teamsId = teamNode != null ? teamNode.TeamsId : Guid.Empty.ToString();
                    if (nodeSetting.Level != (int)NodeLevel.Office365GroupEntire)
                    {
                        var siteCollectionNode = nodeSetting.GetSiteCollectionNode();
                        siteId = siteCollectionNode != null ? siteCollectionNode.SPObjectId : Guid.Empty.ToString();
                    }
                }
                if (!RMTeamsSettingsService.CheckParentNodeDisable(nodeSetting, teamsId, siteId))
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
        [ValidSPParameterActionFilter(ValidType = ValidType.Teams)]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser, RMPermissionExtensionMasks.TeamsEndUser)]
        public async Task<string> UpdateDisposeSchedule([FromBody] RMSPTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                string teamsId = string.Empty;
                string siteId = string.Empty;
                if (nodeSetting.Level != (int)NodeLevel.WebApplication)
                {
                    var teamNode = nodeSetting.GetTeamsNode();
                    teamsId = teamNode != null ? teamNode.TeamsId : Guid.Empty.ToString();
                    if (nodeSetting.Level != (int)NodeLevel.Office365GroupEntire)
                    {
                        var siteCollectionNode = nodeSetting.GetSiteCollectionNode();
                        siteId = siteCollectionNode != null ? siteCollectionNode.SPObjectId : Guid.Empty.ToString();
                    }
                }
                if (!RMTeamsSettingsService.CheckParentNodeDisable(nodeSetting, teamsId, siteId))
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
        [ValidSPParameterActionFilter(ValidType = ValidType.Teams)]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser, RMPermissionExtensionMasks.TeamsEndUser)]
        public string DeleteDisposeSchedule([FromBody] RMSPTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                string teamsId = string.Empty;
                string siteId = string.Empty;
                if (nodeSetting.Level != (int)NodeLevel.WebApplication)
                {
                    var teamNode = nodeSetting.GetTeamsNode();
                    teamsId = teamNode != null ? teamNode.TeamsId : Guid.Empty.ToString();
                    if (nodeSetting.Level != (int)NodeLevel.Office365GroupEntire)
                    {
                        var siteCollectionNode = nodeSetting.GetSiteCollectionNode();
                        siteId = siteCollectionNode != null ? siteCollectionNode.SPObjectId : Guid.Empty.ToString();
                    }
                }
                if (!RMTeamsSettingsService.CheckParentNodeDisable(nodeSetting, teamsId, siteId))
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
        [ValidSPParameterActionFilter(ValidType = ValidType.Teams)]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser, RMPermissionExtensionMasks.TeamsEndUser)]
        public string BreakDisposeSchedule([FromBody] RMSPTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                string teamsId = string.Empty;
                string siteId = string.Empty;
                if (nodeSetting.Level != (int)NodeLevel.WebApplication)
                {
                    var teamNode = nodeSetting.GetTeamsNode();
                    teamsId = teamNode != null ? teamNode.TeamsId : Guid.Empty.ToString();
                    if (nodeSetting.Level != (int)NodeLevel.Office365GroupEntire)
                    {
                        var siteCollectionNode = nodeSetting.GetSiteCollectionNode();
                        siteId = siteCollectionNode != null ? siteCollectionNode.SPObjectId : Guid.Empty.ToString();
                    }
                }
                if (!RMTeamsSettingsService.CheckParentNodeDisable(nodeSetting, teamsId, siteId))
                {
                    nodeSetting.DisposeScheduleInfo.Id = "";
                    ScheduleService.CreateNoSchedule(SettingScheduleType.Dispose, GetNodeFullPath(nodeSetting));
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

        private string GetNodeFullPath(RMSPTreeNode node)
        {
            if (node.Level == (int)NodeLevel.Office365GroupEntire || node.Level == (int)NodeLevel.SiteCollection || node.Level == (int)NodeLevel.WebApplication)
            {
                return node.FullPath;
            }
            return WebUtil.MakeFullUrl(node.GetSiteCollectionNode().FullPath, node.FullPath);
        }

        #endregion

        #region apply setting job
        [HttpGet]
        public bool CheckRunningTeamsSettingJob()
        {
            return RMTeamsSettingsService.CheckRunningTeamsSettingJob();
        }
        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.Teams)]
        public string ApplySettingsOnSelectedNode([FromBody] RMSPTreeNode selectedTree)
        {
            return JsonConvert.SerializeObject(RMTeamsSettingsService.ApplySettingsOnSelectedNode(selectedTree));
        }

        [HttpPost]
        [ValidAccountHasTeamsPermissionFilter]
        public string ApplySettings([FromBody] RunApplySettingjobParam dto)
        {
            try
            {
                if (!RMTeamsSettingsService.ExistConfiguredSettings(JobType.ApplyTeamsSettings))
                {
                    RAReturnMessage msg = new RAReturnMessage();
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_ApplySetting_NoSettings");
                    return JsonConvert.SerializeObject(msg);
                }
                var needRunNodes = new List<RMSPTreeNode>();
                if (UniqueIdSettingService.ValidTeamsUniqueIdSetting() && RMTeamsSettingsService.NeedRunUniqueIdJob(needRunNodes))
                {
                    Logger.Debug("need run unique id job.");
                    var jobId = UniqueIdSettingService.RunUniqueIDSettingScheduleJob(
                        JobRunBy.Control,
                        JobType.TeamsUniqueIDSettingFullSchedule
                        );
                    Logger.Debug("Run unique id job[{0}].", jobId);
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("Run unique id job error{0}.", ex.ToString());

            }
            return JsonConvert.SerializeObject(RMTeamsSettingsService.ApplySettings(JobRunBy.Control, dto.FromTimerJobPage, dto.RunJobMethod));
        }
        #endregion

        #region Migrate teams
        [ValidAccountHasTeamsPermissionFilter]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser, RMPermissionExtensionMasks.TeamsEndUser, preferred: false)]
        [HttpGet]
        public bool HasUpgradeTeams()
        {
            return RMKeyValueDao.HasUpgradeTeams();
        }

        [HttpPost]
        [ValidAccountHasTeamsPermissionFilter]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser, RMPermissionExtensionMasks.TeamsEndUser, preferred: false)]
        public async Task<RAReturnMessage> UpgradeTeams([FromBody]bool isUpgradeSettings)
        {
            return await RMTeamsSettingsService.UpgradeTeams(isUpgradeSettings);
        }

        #endregion

        [HttpGet]
        [ValidAccountHasTeamsPermissionFilter]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser, RMPermissionExtensionMasks.TeamsEndUser, preferred: false)]
        public BaseJobDto GetTeamsChannelConflictCheckJobInfo()
        {
            return JobMonitorService.GetLastestJobByJobType(JobType.TeamsChannelSettingConflictCheck);
        }

        [HttpPost]
        [ValidAccountHasTeamsPermissionFilter]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser, RMPermissionExtensionMasks.TeamsEndUser, preferred: false)]
        public async Task<int> CancelUpgradeTeamsNodeSetting()
        {
            return await JobMonitorService.DeleteJobByTypes([JobType.TeamsChannelSettingConflictCheck]);
        }

        [HttpPost]
        [ValidAccountHasTeamsPermissionFilter]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser, RMPermissionExtensionMasks.TeamsEndUser, preferred: false)]
        public string RunTeamsChannelSettingConflictCheckJob()
        {
            return RMTeamsSettingsService.RunTeamsChannelSettingConflictCheckJob();
        }

        [HttpPost]
        [ValidAccountHasTeamsPermissionFilter]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser, RMPermissionExtensionMasks.TeamsEndUser, preferred: false)]
        public string RunConflictSettingDetailExportJob()
        {
            return RMTeamsSettingsService.RunConflictSettingDetailExportJob();
        }

        [HttpPost]
        [ValidAccountHasTeamsPermissionFilter]
        [RMApiAuthorize(RMPermissionExtensionMasks.TeamsEndUser, preferred: false)]
        public string RunTeamsDataUpgradeJob()
        {
            return RMTeamsSettingsService.RunTeamsDataUpgradeJob();
        }

        [HttpPost]
        [ValidAccountHasTeamsPermissionFilter]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser, RMPermissionExtensionMasks.TeamsEndUser, preferred: false)]
        public string RunTeamsNodeSettingUpgradeJob()
        {
            return RMTeamsSettingsService.RunTeamsNodeSettingUpgradeJob();
        }

        [HttpPost]
        [ValidAccountHasTeamsPermissionFilter]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser, RMPermissionExtensionMasks.TeamsEndUser, preferred: false)]
        public TeamsChannelConflictQueryResult GetTeamsChannelConflictsList([FromBody] TeamsChannelConflictQueryParameter parameter)
        {
            return RMTeamsSettingsService.GetTeamsChannelConflictsList(parameter);
        }

        [HttpGet]
        [ValidAccountHasTeamsPermissionFilter]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser, RMPermissionExtensionMasks.TeamsEndUser, preferred: false)]
        public ArchiverSettingInfo GetTeamsChannelNodeSetting(Guid scopeId, string id)
        {
            return RMArchiverSettingsService.LoadChannelSampleNodeSettings(scopeId, id);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.Teams)]
        [RMApiAuthorize(RMSOPermissionMasks.TeamsEndUser)]
        public string RunSOPreScanJob([FromBody] RMSPTreeNode selectedTree)
        {
            return JsonConvert.SerializeObject(RMArchiverSettingsService.RunTeamsPreScanJobWrapper(selectedTree, JobRunBy.Control));
        }
    }
}
