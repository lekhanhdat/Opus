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
using AngleSharp.Io;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using AvePoint.RA.Web.Extentions.Authorize;
using AvePoint.RA.Web.Models.ControlPanel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using static AvePoint.GCommon.Utility.I18N.EventIds.Configuration;
using static AvePoint.RA.Contract.Object.RMSPTreeNode;
using System.Threading.Tasks;
using AvePoint.RA.Common.Security;
using System.Linq;
using AvePoint.RA.Web.Common.Utils;
using Aspose.Words.XAttr;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Contract.Configurations;
using System.Text.RegularExpressions;
using AvePoint.GCommon;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.SharePoint.CustomIndexMetadata;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.Teams;
namespace AvePoint.RA.Web.Controllers.SharePointSettings
{
    [RMApiAuthorize(RMPermissionMasks.SPOEnduser, preferred: false)]
    public class SPSettingApiController : BaseApiController
    {
        #region Interface
        private ISPSettingTreeService _SPSettingTreeService;
        private ISPSettingTreeService SPSettingTreeService => PlatformWindsorManager.GetService(ref _SPSettingTreeService);
        private IRMSharePointSettingsService _RMSPSettingsService;
        private IRMSharePointSettingsService RMSPSettingsService => PlatformWindsorManager.GetService(ref _RMSPSettingsService);
        private IRMArchiverSettingsService _RMArchiverSettingsService;
        private IRMArchiverSettingsService RMArchiverSettingsService => PlatformWindsorManager.GetService(ref _RMArchiverSettingsService);
        private IUniqueIdSettingService _UniqueIdSettingService;
        private IUniqueIdSettingService UniqueIdSettingService => PlatformWindsorManager.GetService(ref _UniqueIdSettingService);
        private ITaxonomyService _TaxonomyService;
        private ITaxonomyService TaxonomyService => PlatformWindsorManager.GetService(ref _TaxonomyService);
        private IScheduleService _ScheduleService;
        private IScheduleService ScheduleService => PlatformWindsorManager.GetService(ref _ScheduleService);
        private IAOSUserWrapperService _UserWrapperService;
        private IAOSUserWrapperService UserWrapperService => PlatformWindsorManager.GetService(ref _UserWrapperService);
        private ITenantService _TenantService;
        private ITenantService TenantService => PlatformWindsorManager.GetService(ref _TenantService);

        private IManualProcessManagementService _ManualProcessManagementService;
        private IManualProcessManagementService ManualProcessManagementService => PlatformWindsorManager.GetService(ref _ManualProcessManagementService);
        private IBrowseTreeService _BrowseTreeService;
        private IBrowseTreeService BrowseTreeService => PlatformWindsorManager.GetService(ref _BrowseTreeService);
        private IStubSettingService _StubSettingSerivce;
        private IStubSettingService StubSettingService => PlatformWindsorManager.GetService(ref _StubSettingSerivce);
        private IDeclaredRecordsMigrationService _DeclaredRecordsMigrationSerivce;
        private IDeclaredRecordsMigrationService DeclaredRecordsMigrationService => PlatformWindsorManager.GetService(ref _DeclaredRecordsMigrationSerivce);
        private IExplorerQueryService _ExplorerQueryService;
        private IExplorerQueryService ExplorerQueryService => PlatformWindsorManager.GetService(ref _ExplorerQueryService);
        #endregion


        #region Browse

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.SPOEnduser, RMSOPermissionMasks.SPOEnduser)]
        public RMSPSampleTreeNode GetSPFarmNode()
        {
            RMSPSampleTreeNode farmNode = null;
            try
            {
                farmNode = SPSettingTreeService.LoadFarmSampleTree()[0];
                if (farmNode == null || farmNode.Id.Equals(System.Guid.Empty))
                {
                    Logger.Warn("sharepoint farm node is null.Please refresh page.");
                }
                else
                {
                    if (farmNode.Children != null)
                    {
                        //删除Children属性，避免以后convert to SPTree时出现死循环
                        farmNode.Children = null;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred when get sp farm node.Error:{0}", e.ToString());
            }
            return farmNode;
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess, RMSOPermissionMasks.CommonModuleAccess)]
        public bool CheckRemoteNodesIsInit()
        {
            return TenantService.GetTenantInitNodeState(TenantLocalValue.LogonGroupId) == Contract.Aos.Notification.RMInitNodeState.Synced;
        }
        

        [ValidSampleTreeParameterFilter(ValidType = ValidType.SharePointOnline)]
        [RMApiAuthorize(RMPermissionMasks.SPOEnduser, RMSOPermissionMasks.SPOEnduser)]
        public async Task<RMSPSampleTreeNode> BrowseSampleTree([FromBody] RMSPSampleTreeNode node)
        {
            node.SourceType = (int)SourceFlag.SharePoint;
            var returnNode = await BrowseTreeService.BrowseSPOTreeAsync(node, RMBrowseTreeNodeSourceType.SharepointOnline, true);
            SPSettingTreeService.TransChildrenNodeName(returnNode);
            if (node.IsArchiverTree)
            {
                RMArchiverSettingsService.LoadArchiverSettingIcon(returnNode.Children, ScheduleType.SPArchiveJobSchedule);
            }
            else
            {
                await RMSPSettingsService.LoadSPSettingIconAsync(returnNode.Children);
            }
            returnNode.Children?.ForEach(n => n.Parent = null);
            return returnNode;
        }

        [RMApiAuthorize(RMPermissionMasks.SPOEnduser, RMSOPermissionMasks.SPOEnduser)]
        public async Task<RMSPSampleTreeNode> SearchContainerByPage([FromBody] RMSPSampleTreeNode node)
        {
            var returnNode = await BrowseTreeService.SearchContainerByPage(node, RMBrowseTreeNodeSourceType.SharepointOnline, true);
            SPSettingTreeService.TransChildrenNodeName(returnNode);
            if (node.IsArchiverTree)
            {
                RMArchiverSettingsService.LoadArchiverSettingIcon(returnNode.Children, ScheduleType.SPArchiveJobSchedule);
            }
            else
            {
                await RMSPSettingsService.LoadSPSettingIconAsync(returnNode.Children);
            }
            returnNode.Children?.ForEach(n => n.Parent = null);
            return returnNode;
        }

        [RMApiAuthorize(RMPermissionMasks.SPOEnduser, RMSOPermissionMasks.SPOEnduser)]
        public async Task<SearchSiteCollectionLazyLoadResponse> SearchSiteCollectionLazyLoad([FromBody] SearchSiteCollectionLazyLoadRequest condition)
        {
            condition.SourceFlag = (int)SourceFlag.SharePoint;
            var response = await BrowseTreeService.SearchSiteCollectionLazyLoad(condition, checkPermission: true);
            SPSettingTreeService.TransChildrenNodeName(new RMSPSampleTreeNode { Children = response.Children });
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
                    await RMSPSettingsService.LoadSPSettingIconAsync(group);
                }
            }

            return response;
        }

        [RMApiAuthorize(RMPermissionMasks.SPOEnduser | RMPermissionMasks.OneDriveEnduser, RMPermissionExtensionMasks.TeamsEndUser, RMSOPermissionMasks.SPOEnduser | RMSOPermissionMasks.OneDriveEnduser | RMSOPermissionMasks.TeamsEndUser, DB.SecurityTrimming.Model.PermissionJoinType.Any)]
        [ValidSampleTreeParameterFilter(ValidType = ValidType.SharePointOnline)]
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

        [RMApiAuthorize(RMPermissionMasks.SPOEnduser | RMPermissionMasks.OneDriveEnduser, RMSOPermissionMasks.SPOEnduser | RMSOPermissionMasks.OneDriveEnduser, DB.SecurityTrimming.Model.PermissionJoinType.Any)]
        [ValidSampleTreeParameterFilter(ValidType = ValidType.SharePointOnline)]
        public Task<string> BrowseSPAndODTree([FromBody] RMSPSampleTreeNode node)
        {
            return BrowseSampleTreeAsync(node, RMBrowseTreeNodeSourceType.SPAndOD);
        }

        [RMApiAuthorize(RMPermissionMasks.SPOEnduser | RMPermissionMasks.OneDriveEnduser, RMSOPermissionMasks.SPOEnduser | RMSOPermissionMasks.OneDriveEnduser, DB.SecurityTrimming.Model.PermissionJoinType.Any)]
        public Task<string> BrowseSPAndODSuggestion([FromBody] ExplorerQueryV3Dto dto)
        {
            return BrowseSuggestionAsync(dto);
        }

        private async Task<string> BrowseSuggestionAsync(ExplorerQueryV3Dto dto)
        {
            var queryResult = await ExplorerQueryService.QueryAdvancedDataListWithTotalAsync(dto, true);
            var response = new
            {
                pagingInfo = new
                {
                    queryResult.PagingInfo?.PageIndex,
                    queryResult.PagingInfo?.PageSize,
                    queryResult.PagingInfo?.Total,
                    queryResult.PagingInfo?.HasNextPage
                },
                datas = queryResult.Datas?.Select(x => new { x.Id, x.ListId, x.SourceFlag, x.DirPath, x.FullPath, x.AveSiteId })
            };
            return JsonConvert.SerializeObject(response);
        }

        //[ValidSampleTreeParameterFilter]
        //public string BrowseOneDriveSampleTree([FromBody] string node)
        //{
        //    return BrowseSampleTree(node, RMBrowseTreeNodeSourceType.SkyDrivePro);
        //}

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
                    }
                    using (new RA.Common.PerformanceScope("Load tree node setting icon."))
                    {
                        await RMSPSettingsService.LoadSPSettingIconAsync(children);
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
        [RMApiAuthorize(RMPermissionMasks.SPOEnduser, RMPermissionExtensionMasks.TeamsEndUser, RMSOPermissionMasks.SPOEnduser | RMSOPermissionMasks.TeamsEndUser, AvePoint.RA.DB.SecurityTrimming.Model.PermissionJoinType.Any)]
        public string GetSPDesignLists()
        {
            var lists = RMSPSettingsService.GetDesignLists();
            return JsonConvert.SerializeObject(lists);
        }
        #endregion

        #region Load & Save Node Settings

        [HttpPost]
        [ValidSPSampleTreeParameterFilter(ValidType = ValidType.SharePointOnline)]
        public async Task<string> LoadSampleNodeSettings([FromBody] RMSPSampleTreeNode node)
        {
            var settings = await RMSPSettingsService.LoadSampleNodeSettingsAsync(node);
            if(settings.ApprovalType == (int)ApprovalType.ApprovalProcess)
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
        [RMApiAuthorize(RMPermissionMasks.SPOEnduser, RMSOPermissionMasks.SPOEnduser)]
        public async Task<string> LoadChannelNodeSettings([FromBody]TeamsChannelConflictSetting parameter)
        {
            var setting = await RMSPSettingsService.LoadSampleNodeSettingsByScopeId(new Guid(parameter.ScopeId),int.Parse(parameter.Id));
            if (setting.ApprovalType == (int)ApprovalType.ApprovalProcess)
            {
                var result = Guid.TryParse(setting.WorkflowReferenceId, out var referenceId);
                if (result)
                {
                    var workflow = ManualProcessManagementService.GetWorkflow(referenceId);
                    setting.WorkflowReferenceName = workflow?.Name;
                }
            }
            return JsonConvert.SerializeObject(setting);
        }

        [HttpPost]
        [ValidSPSampleTreeParameterFilter(ValidType = ValidType.SharePointOnline)]
        [RMApiAuthorize(RMSOPermissionMasks.SPOEnduser)]
        public string LoadArchiverNodeSettings([FromBody] RMSPSampleTreeNode node)
        {
            ScheduleType type = ScheduleType.SPArchiveJobSchedule;
            var settings = RMArchiverSettingsService.LoadSampleNodeSettings(node, type);
            return JsonConvert.SerializeObject(settings);
        }
        [HttpPost]
        [ValidSPSampleTreeParameterFilter(ValidType = ValidType.SharePointOnline)]
        [RMApiAuthorize(RMSOPermissionMasks.SPOEnduser)]
        public async Task<List<RMRuleInfos>> LoadArchiverRules([FromBody] string containerId)
        {
            var settings = await RMArchiverSettingsService.GetArchiverRuleListAsync(containerId, SourceFlag.SharePoint);
            return settings;
        }
        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.SharePointOnline)]
        public async Task<string> SaveEnableColumnSetting([FromBody] RMSPTreeNode curSetting)
        {
            var result = await RMSPSettingsService.AddEnableColumnSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.SharePointOnline)]
        public async Task<string> SaveColumnSettingExistColumn([FromBody] RMSPTreeNode curSetting)
        {
            var result = await RMSPSettingsService.AddUsingExistColumnSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.SharePointOnline)]
        [RMApiAuthorize(RMSOPermissionMasks.SPOEnduser)]
        public async Task<string> SaveArchiverNodeSetting([FromBody] RMSPTreeNode setting)
        {
            setting.Type = ContentSourceType.SharePoint;
            var result = await RMArchiverSettingsService.SaveArchiverSettingAsync(setting);
            if (setting.Rules?.Count == 0)
            {
                setting.EnableArchiverManagement = (int)EnableRecordManagementSetting.Disable;
                await RMArchiverSettingsService.SaveGeneralSettingAsync(setting);
            }
            return JsonConvert.SerializeObject(result);
        }
        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.SharePointOnline)]
        [RMApiAuthorize(RMSOPermissionMasks.SPOEnduser | RMSOPermissionMasks.OneDriveEnduser, DB.SecurityTrimming.Model.PermissionJoinType.Any)]

        public bool CheckRemoteNodeHaveRunningJob([FromBody] RMSPTreeNode selectedTree)
        {
            return RMArchiverSettingsService.CheckRemoteNodeHaveRunningJob(selectedTree, [JobType.RMArchiverBackup]);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.SharePointOnline)]
        public async Task<string> SaveColumnSetting([FromBody] RMSPTreeNode curSetting)
        {
            var result = await RMSPSettingsService.AddColumnSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.SharePointOnline)]
        public async Task<string> SaveIsSyncDataSetting([FromBody] RMSPTreeNode curSetting)
        {
            var result = await RMSPSettingsService.AddIsSyncSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.SharePointOnline)]
        public async Task<string> SaveGroupLevelSetting([FromBody] RMSPTreeNode curSetting)
        {
            RAReturnMessage result = new();
            var syncUserResult = await RMSPSettingsService.SyncADUsersAsync(curSetting.AIReviewers);
            if (syncUserResult.MessageType == RAMessageType.Successful)
            {
                result = await RMSPSettingsService.AddGlobalColumnAsync(curSetting);
            }
            else
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = syncUserResult.ErrorMessage;
            }
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.SharePointOnline)]
        public async Task<string> SaveDocumentLevelSetting([FromBody] RMSPTreeNode curSetting)
        {
            RAReturnMessage result = new();
            try
            {
                var syncUserResult = await RMSPSettingsService.SyncADUsersAsync(curSetting.AIReviewers);
                if (syncUserResult.MessageType == RAMessageType.Successful)
                {
                if (!curSetting.DefaultTermId.Equals(Guid.Empty) && TaxonomyService.IsOrphanedTerm(curSetting.DefaultTermId))
                {
                    result.FaildType = RAFailedType.DefaultTermIsOrphaned;
                    result.MessageType = RAMessageType.Failed;
                }
                else
                {
                    result = await RMSPSettingsService.AddCustomColumnAsync(curSetting);
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
        [ValidSPParameterActionFilter(ValidType = ValidType.SharePointOnline)]
        public async Task<string> SaveContainerLevelSetting([FromBody] RMSPTreeNode curSetting)
        {
            var result = await RMSPSettingsService.AddContainerTermAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.SharePointOnline, Action = "ValidateSaveSPTermSetting")]
        public async Task<string> SaveLoactionOwners([FromBody] RMSPTreeNode curSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            var syncUserResult = await RMSPSettingsService.SyncADUsersAsync(curSetting.RecordOwner);
            if (syncUserResult.MessageType == RAMessageType.Successful)
            {
                result = await RMSPSettingsService.AddLocationOwnersAsync(curSetting);
            }
            else
            {
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = syncUserResult.ErrorMessage;
            }
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.SharePointOnline)]
        public async Task<string> InheritParentSettings([FromBody] RMSPTreeNode curSetting)
        {
            var result = await RMSPSettingsService.InheritParentSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.SharePointOnline)]
        [RMApiAuthorize(RMSOPermissionMasks.SPOEnduser)]
        public async Task<string> InheritParentArchiverSettings([FromBody] RMSPTreeNode curSetting)
        {
            curSetting.Type = ContentSourceType.SharePoint;
            var result = await RMArchiverSettingsService.InheritParentSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.SharePointOnline)]
        [RMApiAuthorize(RMSOPermissionMasks.SPOEnduser)]
        public async Task<string> InheritSubNodeToCurrentSettings([FromBody] RMSPTreeNode curSetting)
        {
            curSetting.Type = ContentSourceType.SharePoint;
            var result = await RMArchiverSettingsService.InheritSubNodeToCurrentAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.SharePointOnline)]
        public async Task<string> SaveGeneralSetting([FromBody] RMSPTreeNode curSetting)
        {
            var result = await RMSPSettingsService.AddGeneralSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }
        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.SharePointOnline)]
        [RMApiAuthorize(RMSOPermissionMasks.SPOEnduser)]
        public async Task<string> SaveArchiverGeneralSetting([FromBody] RMSPTreeNode curSetting)
        {
            curSetting.Type = ContentSourceType.SharePoint;
            var result = await RMArchiverSettingsService.SaveGeneralSettingAsync(curSetting);
            return JsonConvert.SerializeObject(result);
        }
        #endregion

        #region Dispose Schedule
        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.SharePointOnline)]
        [RMApiAuthorize(RMPermissionMasks.SPOEnduser, RMSOPermissionMasks.SPOEnduser)]
        public async Task<string> UpdateDisposeSchedule([FromBody] RMSPTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                string objectId = Guid.Empty.ToString();
                if (nodeSetting.Level != (int)NodeLevel.WebApplication)
                {
                    RMSPTreeNode siteCollectionNode = RMSPSettingsService.GetSiteCollectionNode(nodeSetting);
                    objectId = siteCollectionNode.SPObjectId;
                }
                if (!RMSPSettingsService.CheckParentNodeDisable(nodeSetting, objectId))
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
        [ValidSPParameterActionFilter(ValidType = ValidType.SharePointOnline)]
        [RMApiAuthorize(RMPermissionMasks.SPOEnduser, RMSOPermissionMasks.SPOEnduser)]
        public async Task<string> CreateDisposeSchedule([FromBody] RMSPTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                string objectId = Guid.Empty.ToString();
                if (nodeSetting.Level != (int)NodeLevel.WebApplication)
                {
                    RMSPTreeNode siteCollectionNode = RMSPSettingsService.GetSiteCollectionNode(nodeSetting);
                    objectId = siteCollectionNode.SPObjectId;
                }
                if (!RMSPSettingsService.CheckParentNodeDisable(nodeSetting, objectId))
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
        [ValidSPParameterActionFilter(ValidType = ValidType.SharePointOnline)]
        [RMApiAuthorize(RMPermissionMasks.SPOEnduser, RMSOPermissionMasks.SPOEnduser)]
        public string DeleteDisposeSchedule([FromBody] RMSPTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                string objectId = Guid.Empty.ToString();
                if (nodeSetting.Level != (int)NodeLevel.WebApplication)
                {
                    RMSPTreeNode siteCollectionNode = RMSPSettingsService.GetSiteCollectionNode(nodeSetting);
                    objectId = siteCollectionNode.SPObjectId;
                }
                if (!RMSPSettingsService.CheckParentNodeDisable(nodeSetting, objectId))
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
        [ValidSPParameterActionFilter(ValidType = ValidType.SharePointOnline)]
        [RMApiAuthorize(RMPermissionMasks.SPOEnduser, RMSOPermissionMasks.SPOEnduser)]
        public string BreakDisposeSchedule([FromBody] RMSPTreeNode nodeSetting)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                result.MessageType = RAMessageType.Successful;
                string objectId = Guid.Empty.ToString();
                if (nodeSetting.Level != (int)NodeLevel.WebApplication)
                {
                    RMSPTreeNode siteCollectionNode = RMSPSettingsService.GetSiteCollectionNode(nodeSetting);
                    objectId = siteCollectionNode.SPObjectId;
                }
                if (!RMSPSettingsService.CheckParentNodeDisable(nodeSetting, objectId))
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
        public string ApplySettings([FromBody] RunApplySettingjobParam dto)
        {
            try
            {

                if (!RMSPSettingsService.ExistConfiguredSettings(JobType.ApplySharePointSettings))
                {
                    RAReturnMessage msg = new RAReturnMessage();
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_ApplySetting_NoSettings");
                    return JsonConvert.SerializeObject(msg);
                }
                var needRunNodes = new List<RMSPTreeNode>();
                if (UniqueIdSettingService.ValidUniqueIdSetting() && RMSPSettingsService.NeedRunUniqueIdJob(needRunNodes))
                {
                    Logger.Debug("need run unique id job.");
                    var jobId = UniqueIdSettingService.RunUniqueIDSettingScheduleJob(
                        JobRunBy.Control,
                        JobType.UniqueIDSettingFullSchedule
                        //needRunNodes, //TODO xwwang run uniqueid job by Group node.
                        //fromTimerJobPage ? "RM_TS_RunSchedule" : TenantLocalValue.LogonUserEmail
                        );
                    //mUniqueIdSettingService.RunUniqueIDSettingScheduleJob(JobRunBy.Schedule, Contract.JobMonitor.JobType.UniqueIDSettingIncrementalSchedule);
                    Logger.Debug("Run unique id job[{0}].", jobId);
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("Run unique id job error{0}.", ex.ToString());

            }
            return JsonConvert.SerializeObject(RMSPSettingsService.ApplySettings(JobRunBy.Control, dto.FromTimerJobPage, dto.RunJobMethod));
        }

        [HttpPost]
        public string RunSpecifySitesArchiverBackup([FromBody] List<string> siteUrls)
        {
            var result = RMArchiverSettingsService.RunSpecifySitesArchiverBackupJob(siteUrls);
            return result.Extension;
        }

        [HttpPost]
        public RMEndUserArchiveReturnMessage RunEndUserStorageOptimizationJob([FromBody] Api.Contract.EndUserArchiveRequestParam request)
        {
            return RMArchiverSettingsService.RunEndUserArchiverBackupJob(request);
        }

        [HttpPost]
        public string RunSpecifyTeamsArchiverBackup([FromBody] List<string> teamIdList)
        {
            var result = RMArchiverSettingsService.RunSpecifyTeamsArchiverBackupJob(teamIdList);
            return result.Extension;
        }

        [HttpGet]
        public bool CheckRunningSharePointSettingJob()
        {
            return RMSPSettingsService.CheckRunningSharePointSettingJob();
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.SharePointOnline)]
        public string ApplySettingsOnSelectedNode([FromBody] RMSPTreeNode selectedTree)
        {
            return JsonConvert.SerializeObject(RMSPSettingsService.ApplySettingsOnSelectedNode(selectedTree));
        }

        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.SharePointOnline)]
        public string RunCollectionJob([FromBody] RMSPTreeNode selectedTree)
        {
            return JsonConvert.SerializeObject(RMSPSettingsService.RunDataSyncJob(selectedTree, JobRunBy.Control));
        }
        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.SharePointOnline)]
        [RMApiAuthorize(RMSOPermissionMasks.SPOEnduser)]
        public string RunArchiverJob([FromBody] RMSPTreeNode selectedTree)
        {
            return JsonConvert.SerializeObject(RMArchiverSettingsService.RunArchiverJob(selectedTree, JobRunBy.Control));
        }
        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.SPOEnduser)]
        public string RunArchiverJobForImport([FromForm] IFormFile fileUp, [FromForm] string selectedTree)
        {
            try
            {
                var node = JsonConvert.DeserializeObject<RMSPTreeNode>(selectedTree);
                if (fileUp != null && node.Level == (int)NodeLevel.WebApplication)
                {
                    string fileName = fileUp.FileName;
                    Logger.Info("sp archiver import sites url file,file name :{0}.", fileName);
                    string extension = fileName.Substring(fileName.LastIndexOf(".") + 1);
                    if (extension.Equals("csv", StringComparison.OrdinalIgnoreCase))
                    {
                        node.ArchiverImportSitesUrl = ApiMessageUtil.GetArchiverImportSitesUrl(fileUp);
                        node.UserArchiverImportFile = true;
                        return JsonConvert.SerializeObject(RMArchiverSettingsService.RunArchiverJob(node, JobRunBy.Control));
                    }
                }
                Logger.Info($"sp:failed run archive import site job,node full path:{node?.FullPath}");
                return JsonConvert.SerializeObject(new RAReturnMessage() { MessageType = RAMessageType.Failed,ErrorMessage = I18NEntity.GetString("RM_JM_Details_Failed_UnexpectedError") });
            }
            catch(Exception e)
            {
                Logger.Error($"some thing went wrong when RunArchiverJobForImport,error:{e.ToString()}");
                return JsonConvert.SerializeObject(new RAReturnMessage() { MessageType = RAMessageType.Failed,ErrorMessage = I18NEntity.GetString("RM_JM_Details_Failed_UnexpectedError") });
            }
        }
        [HttpPost]
        [ValidSPParameterActionFilter(ValidType = ValidType.SharePointOnline)]
        [RMApiAuthorize(RMSOPermissionMasks.SPOEnduser)]
        public string RunSOPreScanJob([FromBody] RMSPTreeNode selectedTree)
        {
            return JsonConvert.SerializeObject(RMArchiverSettingsService.RunSOPreScanJobWrapper(selectedTree, JobRunBy.Control));
        }

        [HttpPost]
        public string RunSPSyncDataJob([FromBody]bool fromTimerJobPage)
        {
            return JsonConvert.SerializeObject(RMSPSettingsService.RunDataSyncJob(null, JobRunBy.Control));
        }


        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ContentRepositoyAdmin, RMSOPermissionMasks.ContentRepositoyAdmin)]
        public RAReturnMessage RunConvertStubJob([FromBody] ConvertStubDto jobInfo)
        {
            if (!TenantService.IsNewOpusTenant())
            {
                return new RAReturnMessage() { MessageType = RAMessageType.Failed };
            }
            return StubSettingService.RunConvertStubJob(jobInfo);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ContentRepositoyAdmin, RMSOPermissionMasks.ContentRepositoyAdmin)]
        public async Task<RAReturnMessage> RunDeclaredRecordsMigrationJob([FromBody] DeclaredRecordsMigrationDto jobInfo)
        {
            if (!TenantService.IsNewOpusTenant())
            {
                return new RAReturnMessage() { MessageType = RAMessageType.Failed };
            }
            return await DeclaredRecordsMigrationService.RunDeclaredRecordsMigrationJob(jobInfo);
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
        [ValidSPCurrentSettingParameterActionFilter(ValidType = ValidType.SharePointOnline)]
        public Task<string> GetSavedTree([FromBody] CurrentSettingsInfo settingInfo)
        {
            return TaxonomyService.GetSPSettingSavedTreeAsync(settingInfo, true);
        }
        #endregion

        #region Related App

        [HttpPost]
        //[Microsoft.AspNetCore.Mvc.TypeFilter(typeof(ValidateAntiForgeryTokenFilterAttribute))]
        //[FileDownloadFilter]
        public IActionResult DownloadRelatedAppOld()
        {
            try
            {
                //var filepath = JobReportUtility.DownloadCSVTemplateToFile();
                string templateName = "AvePointRelatedRecords.app";
                var filepath = Path.Combine(WebUtil.GetInstallPath(), "Config", templateName);
                if (string.IsNullOrEmpty(TenantLocalValue.RecordsUrl))
                {
                    if (string.Equals(RMSSOHelper.RecoHostUrl, "https://eugovrecords.avepointonlineservices.com", StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Info("records gov app {0}", RMSSOHelper.RecoHostUrl);
                        filepath = Path.Combine(WebUtil.GetInstallPath(), "Config", "GOVApp", templateName);
                    }
                }
                else
                {
                    if (string.Equals(TenantLocalValue.RecordsUrl, "https://eugovrecords.avepointonlineservices.com/account/logon", StringComparison.OrdinalIgnoreCase))
                    {
                        Logger.Info("records gov app {0}", TenantLocalValue.RecordsUrl);
                        filepath = Path.Combine(WebUtil.GetInstallPath(), "Config", "GOVApp", templateName);
                    }
                }
                var memoryStream = new MemoryStream();
                using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {
                    stream.CopyTo(memoryStream);
                }
                memoryStream.Position = 0;
                return File(memoryStream, GetContentType(filepath), Path.GetFileName(filepath));
            }
            catch
            {
                return new StatusCodeResult((int)HttpStatusCode.NoContent);
            }
        }


        private static void CreateDirectoryIfNotExist(string filePath)
        {
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
        }

        private static void CreateDirectory(string filePath)
        {
            if (Directory.Exists(filePath))
            {
                Directory.Delete(filePath, true);
            }
            CreateDirectoryIfNotExist(filePath);
        }

        public IActionResult DownloadRelatedApp()
        {
            try
            {
                var tempAppFilePath = "";
                string appFileName = "related-records-app.sppkg";
                var appFolderPath = "Config";
                var appFilePath = Path.Combine(WebUtil.GetInstallPath(), appFolderPath, appFileName);
                //build base temp folder
                var tempBaseFolder = Path.Combine(WebUtil.GetInstallPath(), "Temp", appFolderPath);
                CreateDirectory(tempBaseFolder);

                //unzip app
                var unZipFolder = Path.Combine(tempBaseFolder, "opus-customization");
                CreateDirectoryIfNotExist(unZipFolder);
                ZipUtil.UnZipFile(appFilePath, unZipFolder);
                Logger.Info($"Succcessfully unzip folder {appFilePath} to {unZipFolder}");

                string packagedTime = null;
                var clientSideAssetsFiles = Directory.GetFiles(Path.Combine(unZipFolder, "ClientSideAssets"), "*", SearchOption.AllDirectories);
                clientSideAssetsFiles = clientSideAssetsFiles.Where(filePath => filePath.Contains("related-records-command-set")).ToArray();
                foreach (var clientSideAssetsFilePath in clientSideAssetsFiles)
                {
                    if (Path.GetExtension(clientSideAssetsFilePath) != ".js")
                    {
                        continue;
                    }
                    var fileContentt = System.IO.File.ReadAllText(clientSideAssetsFilePath);

                    void ReplaceAppConfig(string repalceKey, string replaceValue)
                    {
                        Regex extractConfigSiteUrlRegex = new($"{repalceKey}\\s?:\\s?\\\".*?\\\"", RegexOptions.None, TimeSpan.FromMinutes(3));
                        var configSiteUrlSettings = extractConfigSiteUrlRegex.Match(fileContentt);
                        Logger.Info($"Find {repalceKey} value: {configSiteUrlSettings.Value}");
                        fileContentt = fileContentt.Replace(configSiteUrlSettings.Value, $"{repalceKey}:\"{replaceValue}\"");
                    }
                    ReplaceAppConfig("opusApiUrl", RMGlobalConfiguration.AppConfig[RMAppSettingKey.PUBLIC_RECO_API_URL]);
                    ReplaceAppConfig("aosLoginAppId", RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_LOGIN_APP_ID]);
                    ReplaceAppConfig("aosCustomerId", TenantLocalValue.LogonGroupId);

                    Regex extractPackagedTimeRegex = new("packagedTime:\\\"(?<packagedTime>\\S*?)\\\"", RegexOptions.None, TimeSpan.FromMinutes(3));
                    var match = extractPackagedTimeRegex.Match(fileContentt);
                    System.IO.File.WriteAllText(clientSideAssetsFilePath, fileContentt);
                    packagedTime ??= match.Groups.GetValueOrDefault("packagedTime")?.Value;
                    Logger.Info($"{appFileName} get packaged time is:{packagedTime}");
                }

                var manifestFilePath = Path.Combine(unZipFolder, "AppManifest.xml");
                var manifestFileContent = System.IO.File.ReadAllText(manifestFilePath);
                manifestFileContent = manifestFileContent.Replace("ResourceId=\"c4763714-72c1-4746-a68e-a17bcf7ad292\"", $"ResourceId=\"{RMGlobalConfiguration.AppConfig[RMAppSettingKey.AOS_LOGIN_APP_ID]}\"");
                System.IO.File.WriteAllText(manifestFilePath, manifestFileContent);

                tempAppFilePath = Path.Combine(tempBaseFolder, appFileName);
                if (System.IO.File.Exists(tempAppFilePath))
                {
                    System.IO.File.Delete(tempAppFilePath);
                }
                ZipUtil.ZipFolder(unZipFolder, tempAppFilePath);
                Logger.Info($"Succcessfully zip folder {tempAppFilePath} to {unZipFolder}");

                Directory.Delete(unZipFolder, recursive: true);

                var memoryStream = new MemoryStream();
                using (var stream = new FileStream(tempAppFilePath, FileMode.Open, FileAccess.Read))
                {
                    stream.CopyTo(memoryStream);
                }
                memoryStream.Position = 0;
                return File(memoryStream, GetContentType(tempAppFilePath), Path.GetFileName(tempAppFilePath));
            }
            catch
            {
                return new StatusCodeResult((int)HttpStatusCode.NoContent);
            }
        }

        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            if (!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }

            return contentType;
        }

        #endregion

        #region Other Pages

        /// <summary>
        /// 当前台页面利用;强制匹配一个用户时，调用此方法
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess)] //TODO xwwang
        [HttpGet]
        public SingleUser ValidateSingleUser(string key, string elementId)
        {
            SingleUser user = new SingleUser
            {
                User = UserWrapperService.SearchSingleAccount(TenantLocalValue.LogonGroupId, key),
                ResolveID = elementId
            };
            return user;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.SPOEnduser, RMPermissionExtensionMasks.TeamsEndUser, RMSOPermissionMasks.SPOEnduser | RMSOPermissionMasks.TeamsEndUser, AvePoint.RA.DB.SecurityTrimming.Model.PermissionJoinType.Any)]
        public string GetSPTreeInitData()
        {
            var farmNode = SPSettingTreeService.LoadFarm()[0];
            if (farmNode == null || string.IsNullOrEmpty(farmNode.Id))
            {
                Logger.Warn("Farm node is null.Please refresh page.");
            }
            else
            {
                if (farmNode.Children != null)
                {
                    //删除Children属性，避免以后convert to SPTree时出现死循环
                    farmNode.Children = null;
                }
            }

            return JsonConvert.SerializeObject(farmNode);
        }

        #endregion

        #region Custom index metadata

        [HttpPost]
        [ValidCustomMetadataParameterActionFilter("SaveOrUpdateCustomColumns")]
        public Task<RAReturnMessage> SaveCustomMetadataColumns([FromBody] List<CustomMetadataColumnInfo> customIndexMetadatas)
        {
            return RMSPSettingsService.AddOrUpdateCustomMetadataColumnAsync(customIndexMetadatas);
        }

        [HttpGet]
        public Task<List<CustomMetadataColumnInfo>> GetCustomMetadataColumns()
        {
            return RMSPSettingsService.GetAllCustomMetadataColumnInfoAsync();
        }        
        
        [HttpGet]
        public Task<List<CustomMetadataColumnInfo>> GetInUsedCustomMetadataColumns()
        {
            return RMSPSettingsService.GetInUsedCustomMetadataColumnInfoAsync();
        }

        [HttpPost]
        [ValidCustomMetadataParameterActionFilter("SaveOrUpdateCustomMetadatas")]
        public Task<RAReturnMessage> SaveCustomIndexMetadatas([FromBody] CustomIndexMetadataInfo customIndexMetadatas)
        {
            return RMSPSettingsService.AddOrUpdateCustomIndexMetadataAsync(customIndexMetadatas, SourceFlag.SharePoint);
        }

        [HttpGet]
        public Task<CustomIndexMetadataInfo> GetCustomIndexMetadatas()
        {
            return RMSPSettingsService.GetAllCustomIndexMetadataAsync();
        }

        [HttpGet]
        public Task<CustomIndexMetadataInfo> GetCustomIndexMetadatasBySourceFlag()
        {
            return RMSPSettingsService.GetCustomIndexMetadatasBySourceFlagAsync(SourceFlag.SharePoint);
        }

        #endregion
    }
}
