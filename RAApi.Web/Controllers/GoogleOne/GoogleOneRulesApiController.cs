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
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Schedule;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.DocAve;
using System.Collections.Generic;
using System.Linq;
using AvePoint.RA.Service.ControlPanel;
using AvePoint.RA.Service.Services.RuleManagement;
using Newtonsoft.Json;
using AvePoint.RA.Service.RuleManagement;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Service.Services.ControlPanel;
using AvePoint.RA.Contract.RMWeb.Rule;
using Cloud.sdk.Data.Opus.GoogleOne.Common;

namespace AvePoint.RA.Api.Web.Controllers.GoogleOne;

[Route("api/googleone/rule")]
public class GoogleOneRulesApiController : GoogleOneApiBaseController
{
    private RALogger logger = RALogger.GetInstance(typeof(GoogleOneRulesApiController));

    private IRuleManagerService RuleService => PlatformWindsorManager.GetService<IRuleManagerService>();
    private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
    private IGlobalSettingService GlobalSettingService => PlatformWindsorManager.GetService<IGlobalSettingService>();
    private IRuleContainerService RuleContainerService => PlatformWindsorManager.GetService<IRuleContainerService>();
    private IRuleManagerService RuleManagerService => PlatformWindsorManager.GetService<RuleManagerService>();

    private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();

    public static readonly Guid RECORD_DEFAULT_CONTAINER_ID = new Guid("C01A98AD-0D33-477B-A846-43AD41DDEE55");
    private IManualProcessManagementService ManualProcessManagementService => PlatformWindsorManager.GetService<IManualProcessManagementService>();

    [HttpPost("get")]
    public async Task<string> GetRuleById([FromBody] string ruleId)
    {
        using var performance = new PerformanceScope("GoogleOneRulesApiController.GetRuleByID");
        try
        {
            logger.Info("get rule by id normal,id:{0}", ruleId);
            RMRuleInfos rule = await RuleService.LoadRuleAsync(ruleId, true);
            return JsonConvert.SerializeObject(rule);
        }
        catch (Exception ex)
        {
            logger.Info($"get rule by id faild,id:{ruleId},msg:{ex}");
            return string.Empty;
        }
    }

    [HttpPost("create")]
    public async Task<string> CreateRule([FromBody] RMRuleInfos ruleInfo)
    {
        try
        {
            ruleInfo.ContainerId = RECORD_DEFAULT_CONTAINER_ID;
            AddTimeZoneIdForRuleTimeCriterias(ruleInfo.GoogleDriveRule.RuleFilters);
            

            RAReturnMessage result = await RuleService.CreateRuleInDAAsync(ruleInfo);
            if (result.MessageType == RAMessageType.Failed)
            {
                logger.Error("create rule faild,RuleName:{1},ERROR:{0}.", result.ErrorMessage, ruleInfo.RuleName);
                if (result.ErrorMessage == I18NEntity.GetString("RM_JS_RDM_CreateRule_Validation_EqualCopyName"))
                {
                    return I18NEntity.GetString("RM_JS_RDM_CreateRule_Validation_EqualCopyName");

                }
                else
                {
                    return string.Format(I18NEntity.GetString("RM_JS_Common_ErrorInOperateRule"), ruleInfo.RuleName);
                }
            }
            else
            {
                //enable manual approval, create manula approval job schedule
                if ((ruleInfo.GoogleDriveRule != null && ruleInfo.GoogleDriveRule.EnableManualApproval))
                {
                    await ScheduleService.CreateCustomScheduleAsync(false, ScheduleType.ManualApprovalEmailSchedule);
                }

                if (TenantService.IsNewOpusTenant() && ruleInfo.ModelType == RuleModel.Records)
                {
                    //enable manual approval, create manula approval job schedule
                    if ((ruleInfo.GoogleDriveRule != null && ruleInfo.GoogleDriveRule.EnableManualApproval))
                    {
                        await ScheduleService.CreateCustomScheduleAsync(false, ScheduleType.ManualApprovalScheduleTimer);
                    }
                }
                else
                {
                    //enable manual approval, create manula approval job schedule
                    if (ruleInfo.GoogleDriveRule != null && ruleInfo.GoogleDriveRule.EnableManualApproval)
                    {
                        await ScheduleService.CreateCustomScheduleAsync(false, ScheduleType.ManualApprovalScheduleTimer);
                    }
                }
            }

            logger.Info("create rule success,RuleName:{0}.", ruleInfo.RuleName);
            return string.Empty;
        }
        catch (Exception ex)
        {
            logger.Error("an error occurred while create rule(c),rule name:{1},ERROR:{0}", ex.ToString(), ruleInfo.RuleName);
            return string.Format(I18NEntity.GetString("RM_JS_Common_ErrorInOperateRule"), ruleInfo.RuleName);
        }

    }

    [HttpPost("edit")]
    public async Task<string> EditRule([FromBody] RMRuleInfos ruleInfo)
    {
        try
        {
            AddTimeZoneIdForRuleTimeCriterias(ruleInfo.GoogleDriveRule.RuleFilters);

            RMRuleInfos rule = await RuleService.LoadRuleAsync(ruleInfo.RuleId);
            rule.RuleName = ruleInfo.RuleName;
            rule.Description = ruleInfo.Description;
            rule.GoogleDriveRule = ruleInfo.GoogleDriveRule;
            rule.DisposalClass = ruleInfo.DisposalClass;
            RAReturnMessage result = await RuleService.ModifyRuleInDAAsync(rule);
            if (result.MessageType == RAMessageType.Failed)
            {
                logger.Info("edit rule faild,RuleName:{1},ERROR:{0}.", result.ErrorMessage, ruleInfo.RuleName);
                if (TenantService.IsNewOpusTenant())
                {
                    return string.Format(I18NEntity.GetString("RM_JS_Common_ErrorInOperateRule"), result.ErrorMessage);
                }
                else
                {
                    return string.Format(I18NEntity.GetString("RM_JS_Common_FromDocaveMsg"), result.ErrorMessage);
                }
            }
            else
            {
                //enable manual approval, create manula approval job schedule
                if ((ruleInfo.GoogleDriveRule != null && ruleInfo.GoogleDriveRule.EnableManualApproval))
                {
                    await ScheduleService.CreateCustomScheduleAsync(false, ScheduleType.ManualApprovalEmailSchedule);
                }

                if (TenantService.IsNewOpusTenant() && ruleInfo.ModelType == RuleModel.Records)
                {
                    //enable manual approval, create manula approval job schedule
                    if ((ruleInfo.GoogleDriveRule != null && ruleInfo.GoogleDriveRule.EnableManualApproval))
                    {
                        await ScheduleService.CreateCustomScheduleAsync(false, ScheduleType.ManualApprovalScheduleTimer);
                    }
                }
                else
                {
                    //enable manual approval, create manula approval job schedule
                    if (ruleInfo.GoogleDriveRule != null && ruleInfo.GoogleDriveRule.EnableManualApproval)
                    {
                        await ScheduleService.CreateCustomScheduleAsync(false, ScheduleType.ManualApprovalScheduleTimer);
                    }
                }
            }
            logger.Info("edit rule success.RuleName:{0},RuleId:{1}", ruleInfo.RuleName, ruleInfo.RuleId);
            return string.Empty;
        }
        catch (Exception ex)
        {
            logger.Error("an error occurred while edit rule(c),rule name:{1}, ERROR:{0}", ex.ToString(), ruleInfo.RuleName);
            return string.Format(I18NEntity.GetString("RM_JS_RDM_EditRule_MessageInfo_Faild"), ex.Message);
        }
    }

    [HttpPost("delete")]
    public async Task<string> DeleteRules([FromBody] List<string> ruleIds)
    {
        try
        {
            logger.Info("begin to delete rule(s).");
            RAReturnMessage result = await RuleService.DeleteRulesAsync(ruleIds);
            if (result.MessageType == RAMessageType.Failed)
            {
                logger.Info("delete rule faild,RuleName:{1},ERROR:{0}.", result.ErrorMessage, ruleIds?.FirstOrDefault());
                if (result.ErrorMessage.Contains(I18NEntity.GetString("RM_JS_RDM_EditRule_UsedByJob")))
                {
                    return I18NEntity.GetString("RM_JS_RDM_EditRule_UsedByJob");
                }
                else
                {
                    return I18NEntity.GetString("RM_AR_Storage_Unknow_ErrorMessage");
                }
            }
            return string.Empty;
        }
        catch (Exception ex)
        {
            logger.Error("an error occurred while delete rules,ERROR:{0}", ex.ToString());
            return I18NEntity.GetString("RM_AR_Storage_Unknow_ErrorMessage");
        }
    }

    [HttpPost("search")]
    public async Task<string> GetSearchRuleDatas([FromBody] RulePageRequestModel requestModel)
    {
        List<RMRuleInfos> rules = [];
        try
        {
            using PerformanceScope scope = new("Outer get all rules");
            rules = await RuleService.GetSearchRuleAsync(requestModel);
        }
        catch (Exception ex)
        {
            logger.Error("error occurred while get rules:{0}", ex.ToString());
        }

        return JsonConvert.SerializeObject(rules);
    }

    [HttpPost("getavailablerules")]
    public async Task<List<RMRuleInfos>> GetAvailableRuleList()
    {
        try
        {
            using (PerformanceScope scope = new PerformanceScope("setting rules"))
            {
                return await RuleService.GetSearchRuleAsync(new()
                {
                    PageIndex = 0,
                    PageSize = 99999
                });

            }
        }
        catch (Exception ex)
        {
            logger.Error("error occurred while get rules:{0}", ex.ToString());
        }

        return new();
    }
    private void AddTimeZoneIdForRuleTimeCriterias(List<RuleFilter> ruleFilters)
    {
        foreach (var rf in ruleFilters)
        {
            if (rf.RuleType == ArchiverFilterRuleType.CreatedTime || rf.RuleType == ArchiverFilterRuleType.ModifiedTime || rf.RuleType == ArchiverFilterRuleType.DateTimeLabelProperty)
            {
                if (rf.StartTimeInfo != null)
                    rf.StartTimeInfo.TimeZoneId = TenantLocalValue.TimezoneId;

                if (rf.EndTimeInfo != null)
                    rf.EndTimeInfo.TimeZoneId = TenantLocalValue.TimezoneId;
            }
        }
    }

}