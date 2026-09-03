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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.Rule;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters.RuleApiFilter
{

    public enum ContainerPermissionFilterType 
    {
        ContainerId,
        RuleParameter,
        RMRuleInfos,
        RuleContainerDto
    }


    public class ValidateRuleContainerPermissionFilter : BaseActionFilter
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ValidateRuleContainerPermissionFilter));

        public ContainerPermissionFilterType FilterType { get; }

        public ValidateRuleContainerPermissionFilter(ContainerPermissionFilterType type)
        {
            FilterType = type;
        }

        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var ruleParamObj = actionContext.ActionArguments.Values.FirstOrDefault();
            if(!TryGetContainerId(ruleParamObj, out var containerId))
            {
                return; 
            }

            var ruleContainerIds = await SecurityTrimmingHelper.GetRuleScopeAsync();
            if(!ruleContainerIds.Any(item => item == containerId))
            {
                Logger.Warn($"Current user can't access container: [{containerId}].");
                actionContext.Result = new ObjectResult("Access Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
                return; 
            }

            return;
        }

        private bool TryGetContainerId(object parameterObj, out Guid containerId)
        {
            containerId = Guid.Empty;

            if(parameterObj == null)
            {
                return false;
            }

            if(FilterType == ContainerPermissionFilterType.ContainerId)
            {
                return Guid.TryParse(parameterObj.ToString(), out containerId);
            }
            else if(FilterType == ContainerPermissionFilterType.RuleParameter)
            {
                if (!(parameterObj is RuleParameter ruleParam))
                {
                    return false;
                }
                containerId = ruleParam.ContainerId;
                return true;
            }
            else if(FilterType == ContainerPermissionFilterType.RMRuleInfos)
            {
                if (!(parameterObj is RMRuleInfos ruleInfo))
                {
                    return false;
                }
                containerId = ruleInfo.ContainerId;
                return true;
            }
            else if (FilterType == ContainerPermissionFilterType.RuleContainerDto)
            {
                if (!(parameterObj is RuleContainerDto ruleContainerDto))
                {
                    return false;
                }
                if (ruleContainerDto.ContainerId == Guid.Empty)
                {
                    return false;
                }
                containerId = ruleContainerDto.ContainerId;
                return true;
            }

            return false;
        }


    }
}