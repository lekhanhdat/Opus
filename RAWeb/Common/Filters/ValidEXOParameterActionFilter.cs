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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.AccountManager;
using AvePoint.RA.Service.Services.ControlPanel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters
{
    public class ValidEXOParameterActionFilter : BaseActionFilter
    {
        private RALogger logger = RALogger.GetInstance(typeof(ValidEXOParameterActionFilter));
        public IRMScopeRoleAssignmentDao RMScopeRoleAssignmentDao => PlatformWindsorManager.GetService<IRMScopeRoleAssignmentDao>();//Temp Method TO DO important add to common method 
        private IRuleManagerService RuleManagerService => PlatformWindsorManager.GetService<IRuleManagerService>();
        private IManualProcessManagementService ManualProcessManagementService => PlatformWindsorManager.GetService<IManualProcessManagementService>();

        private string action;
        public ValidEXOParameterActionFilter()
        {
        }
        public ValidEXOParameterActionFilter(string type)
        {
            action = type;
        }


        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var treeNode = actionContext.ActionArguments.Values.FirstOrDefault() as RMEXOTreeNode;
            if (treeNode == null || treeNode?.Level == null) 
            {
                logger.Info("treeNode or treeNode?.Level is illegal.");
                actionContext.Result = new ObjectResult("Invalid treeNode or treeNode?.Level") { StatusCode = (int)HttpStatusCode.Forbidden };
                return;
            }
            IUserService userService = new UserService();
            if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.EXOAdmin)))
            {
                if (treeNode?.Level > (int)NodeLevel.ExchangeOnlineFarm)
                {
                    string containerId = TreeNodeUtil.GetEXOContainderId(treeNode);
                    List<string> userAndGroupUserIds = await userService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                    if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupUserIds))
                    {
                        logger.Info("No access on container.");
                        actionContext.Result = new ObjectResult("Access  Denied(container)") { StatusCode = (int)HttpStatusCode.Forbidden };
                    }
                }

                if (!string.IsNullOrEmpty(action))
                {
                    if (action.Equals("ValidateSaveGroupEXOTermSetting"))
                    {
                        if (treeNode.Level == (int)NodeLevel.ExchangeOnlineMailboxGroup && treeNode.IsNullClassificationSetting)
                        {
                            var associatedRules = treeNode.Rules;
                            if (associatedRules != null && associatedRules.Count > 0)
                            {
                                try
                                {
                                    var ruleIds = associatedRules.Select(o => o.RuleId).ToList();
                                    var containerId = treeNode.Id;
                                    var securityGroupIds = SecurityTrimmingHelper.GetSecurityGroupsByContentScope(new List<string> { containerId }, SourceFlag.Exchange);
                                    var ruleContainerIds = SecurityTrimmingHelper.GetRuleScopeBySecurityGroupIds(securityGroupIds);
                                    var availableExchangeRules = await RuleManagerService.GetExchangeRulesAsync(ruleContainerIds);
                                    var availableExchangeRuleIds = availableExchangeRules.Select(o => o.RuleId).ToList();

                                    if (availableExchangeRuleIds.Count == 0)
                                    {
                                        logger.Info("No available Exchange Rules.");
                                        actionContext.Result = new ObjectResult("Access  Denied(No available exchange rules)") { StatusCode = (int)HttpStatusCode.Forbidden };
                                    }

                                    ruleIds.ForEach(o =>
                                    {
                                        if (!availableExchangeRuleIds.Contains(o.ToString()))
                                        {
                                            logger.Info("The associated rule is illegal.");
                                            actionContext.Result = new ObjectResult("Access  Denied(The associated rule is illegal)") { StatusCode = (int)HttpStatusCode.Forbidden };
                                            return;
                                        }
                                    });
                                }
                                catch (Exception ex)
                                {
                                    logger.Warn($"error occurred while ValidateSaveGroupEXOTermSetting:{ex}");
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                var isEnableManualApproval = treeNode.ApprovalType == (int)ApprovalType.ApprovalProcess;
                if (isEnableManualApproval)
                {
                    var workflowId = treeNode.WorkflowReferenceId;
                    if(workflowId == null || string.IsNullOrEmpty(workflowId.Trim()))
                    {
                        actionContext.Result = new ObjectResult("Access  Denied(The workflow id is illegal)") { StatusCode = (int)HttpStatusCode.Forbidden };
                        return;
                    }
                    var result = Guid.TryParse(workflowId, out var referenceId);
                    if (result)
                    {
                        var workflow = ManualProcessManagementService.GetWorkflow(referenceId);
                        if(workflow == null)
                        {
                            actionContext.Result = new ObjectResult("Access  Denied(The workflow id is illegal)") { StatusCode = (int)HttpStatusCode.Forbidden };
                            return;
                        }
                    }
                    else
                    {
                        actionContext.Result = new ObjectResult("Access  Denied(The workflow id is illegal)") { StatusCode = (int)HttpStatusCode.Forbidden };
                        return;
                    }
                }
            }
        }
    }
}