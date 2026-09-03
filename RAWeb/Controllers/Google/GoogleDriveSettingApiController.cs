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
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Google;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.Service.TermManagement;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AvePoint.RA.Web.Controllers.Google;

[RMApiAuthorize(RMPermissionExtensionMasks.GoogleAdmin, preferred: false)]
public class GoogleDriveSettingApiController : BaseApiController
{
    public IBrowseTreeService BrowseTreeService => PlatformWindsorManager.GetService<IBrowseTreeService>();

    private IRMRemoteGoogleNodeService RemoteGoogleNodeService => PlatformWindsorManager.GetService<IRMRemoteGoogleNodeService>();

    private IManualProcessManagementService ManualProcessManagementService => PlatformWindsorManager.GetService<IManualProcessManagementService>();

    private IRMGoogleSettingsService GoogleSettingsService => PlatformWindsorManager.GetService<IRMGoogleSettingsService>();
    
    private IRMGoogleJobService GoogleJobService => PlatformWindsorManager.GetService<IRMGoogleJobService>();
    private IScheduleService RMScheduleService => PlatformWindsorManager.GetService<IScheduleService>();
    private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();
    private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

    private IRMSecurityTrimmingHelper _SecurityTrimmingHelper;
    private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService(ref _SecurityTrimmingHelper);

    private IRuleManagerService _RuleManagerService;
    private IRuleManagerService RuleManagerService => PlatformWindsorManager.GetService(ref _RuleManagerService);
    
    private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();

    public IRMSecurityGroupDao SecurityGroupDao => PlatformWindsorManager.GetService<IRMSecurityGroupDao>();

    [HttpPost]
    public async Task<RMSampleGoogleTreeNode> BrowseSampleTree([FromBody] RMSampleGoogleTreeNode node)
    {
        var returnNode = await BrowseTreeService.BrowseGoogleDriveTreeAsync(node, false);
        return returnNode;
    }

    [HttpPost]
    public async Task<string> BrowseSampleTreeForRule([FromBody] RMSampleGoogleTreeNode parentNode)
    {
        List<RMSampleGoogleTreeNode> children = (await BrowseTreeService.BrowseGoogleDriveTreeForRuleAsync(parentNode, false)).Children;
        parentNode.Children = null;
        children?.ForEach(child =>
        {
            child.Parent = parentNode;
            child.ParentId = parentNode.Id;
        });
        return JsonConvert.SerializeObject(children);
    }

    [HttpPost]
    public async Task<string> BrowseSampleTreeForFullLevel([FromBody] RMSampleGoogleTreeNode parentNode)
    {
        List<RMSampleGoogleTreeNode> children = (await BrowseTreeService.BrowseGoogleDriveTreeForFullLevelAsync(parentNode, false)).Children;
        parentNode.Children = null;
        children?.ForEach(child =>
        {
            child.Parent = parentNode;
            child.ParentId = parentNode.Id;
        });
        return JsonConvert.SerializeObject(children);
    }

    [HttpGet]
    public RMSampleGoogleTreeNode GetGoogleDriveRootNode()
    {
        var result = RemoteGoogleNodeService.LoadGoogleDriveRoot()[0];
        if (result.Children != null)
        {
            result.Children = null;
        }
        return result;       
    }
    
    [HttpGet]
    public bool CheckTenantKindWithM365AndGoogleLicense()
    {
        if (!TenantService.IsNewOpusTenant() && LicenseHelperService.HasOpusILLicense &&
            LicenseHelperService.HasOpusGoogleLicense)
        {
            return true;
        }
        return false;
    }

    #region Load & Save Node Settings

    [HttpPost]
    public async Task<string> LoadGoogleNodeSettings([FromBody] RMSampleGoogleTreeNode node)
    {
        var settings = await GoogleSettingsService.LoadGoogleNodeSettingsAsync(node);
        if(settings.ApprovalType == (int)ApprovalType.ApprovalProcess)
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
    public async Task<string> SaveGeneralSetting([FromBody] RMGoogleTreeNode curSetting)
    {
        var result = await GoogleSettingsService.AddGoogleDriveGeneralSettingAsync(curSetting);
        return JsonConvert.SerializeObject(result);
    }

    [HttpPost]
    public async Task<List<RMRuleInfos>> GetAvailableRuleList([FromBody] string containerId)
    {
        List<RMRuleInfos> listRuleFromDA = [];
        List<RMRuleInfos> availableRules = [];
        try
        {
            Logger.Info("Get Google Drive Rules from Records ");
            using (PerformanceScope scope = new("setting rules"))
            {
                var userAndGroupIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                var securityGroups = SecurityGroupDao.GetSecurityGroups(userAndGroupIds);
                var securityGroupIds = SecurityTrimmingHelper.GetSecurityGroupsByContentScope(securityGroups, SourceFlag.Google, false).Select(group => group.Id).ToList();
                var ruleContainerIds = SecurityTrimmingHelper.GetRuleScopeBySecurityGroupIds(securityGroupIds);
                listRuleFromDA = await RuleManagerService.GetGoogleRulesAsync(ruleContainerIds);
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
    public async Task<string> CreateDisposeSchedule([FromBody] RMGoogleTreeNode nodeSetting)
    {
        var result = await GoogleSettingsService.CreateDisposeSchedule(nodeSetting);
        return JsonConvert.SerializeObject(result);
    }
    
    [HttpPost]
    public async Task<string> UpdateDisposeSchedule([FromBody] RMGoogleTreeNode nodeSetting)
    {
        var result = await GoogleSettingsService.UpdateDisposeSchedule(nodeSetting);
        return JsonConvert.SerializeObject(result);
    }
    
    [HttpPost]
    public async Task<string> DeleteDisposeSchedule([FromBody] RMGoogleTreeNode nodeSetting)
    {
        var result = await GoogleSettingsService.DeleteDisposeSchedule(nodeSetting);
        return JsonConvert.SerializeObject(result);
    }

    
    [HttpPost]
    public async Task<string> SaveLabelSetting([FromBody] RMGoogleTreeNode curSetting)
    {
        RAReturnMessage result = new();
        try
        {
            var syncUserResult = await GoogleSettingsService.SyncADUsersAsync(curSetting.AIReviewers);
            if (syncUserResult.MessageType == RAMessageType.Successful)
            {
                result = await GoogleSettingsService.AddLabelSettingAsync(curSetting);
                if (curSetting.Rules?.Count == 0 && curSetting.IsNullClassificationSetting)
                {
                    curSetting.EnableRecordManagement = (int)EnableRecordManagementSetting.Disable;
                    await GoogleSettingsService.AddGoogleDriveGeneralSettingAsync(curSetting);
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
            Logger.Error("Save Google Settings Failed.ERROR:{0}", ex.Message);
        }
        return JsonConvert.SerializeObject(result);
    }

    [HttpPost]
    public async Task<string> InheritParentSettings([FromBody] RMGoogleTreeNode curSetting)
    {
        var result = await GoogleSettingsService.InheritParentSettingAsync(curSetting);
        return JsonConvert.SerializeObject(result);
    }
    
    #endregion

    #region data sync
    [HttpPost]
    public async Task<string> RunCollectionJob([FromBody] RMGoogleTreeNode node)
    {
        return JsonConvert.SerializeObject(await GoogleSettingsService.RunDataSyncJob(node, JobRunBy.Control));
    }
    #endregion

    #region apply setting
    [HttpPost]
    public string ApplySettingOnSelectedNode([FromBody] RMGoogleTreeNode selectedNode)
    {
        return JsonConvert.SerializeObject(GoogleJobService.ApplySettingsOnSelectedNode(selectedNode));
    }

    [HttpPost]
    public string ApplySettings([FromBody] RunApplySettingjobParam param)
    {
        return JsonConvert.SerializeObject(GoogleJobService.ApplySettings(JobRunBy.Control, param.FromTimerJobPage,
            param.RunJobMethod));
    }
    #endregion

    #region run enforce rule
    [HttpPost]
    public string RecordsDisposal([FromBody] RMGoogleTreeNode selectedNode)
    {
        return JsonConvert.SerializeObject(GoogleJobService.RunRecordsDisposalJob(selectedNode));
    }

    [HttpPost]
    public string BreakDisposeSchedule([FromBody] RMGoogleTreeNode selectedNode)
    {
        RAReturnMessage result = new RAReturnMessage();
        try
        {
            result.MessageType = RAMessageType.Successful;
            RMScheduleService.CreateNoSchedule(SettingScheduleType.Dispose);
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
    [HttpPost]
    public string RunSyncDataJob(bool fromTimerJobPage)
    {
        return JsonConvert.SerializeObject(GoogleSettingsService.RunDataSyncJob(JobRunBy.Control));
    }
    [HttpPost]
    [ValidScheduleSettingActionFilter]
    [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMPermissionExtensionMasks.GoogleAdmin)]
    public Task<string> CreateSchedule([FromBody] ScheduleInfo info)
    {
        info.Id = Guid.NewGuid().ToString();
        return RMScheduleService.CreateScheduleServiceAsync(info);
    }
    [HttpPost]
    [ValidScheduleSettingActionFilter]
    [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin ,RMPermissionExtensionMasks.GoogleAdmin)]
    public Task<string> UpdateScheduleService([FromBody] ScheduleInfo info)
    {
        return RMScheduleService.UpdateScheduleServiceAsync(info);
    }
    [HttpPost]
    [ValidScheduleSettingActionFilter]
    [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMPermissionExtensionMasks.GoogleAdmin)]
    public void DeleteScheduleService([FromBody] string Id)
    {
        RMScheduleService.DeleteScheduleService(Id);
    }
}