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
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters.RuleApiFilter
{
    public class ValidateRuleIdPermissionFilter : BaseActionFilter
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ValidateRuleIdPermissionFilter));

        private static readonly IRMRuleDao RuleDao = new RMRuleDao();

        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var ruleId = actionContext.ActionArguments.Values.FirstOrDefault()?.ToString();
            if(string.IsNullOrEmpty(ruleId))
            {
                return;
            }

            var container = RuleDao.GetRuleContainersByRuleId(new Guid(ruleId));
            if(container == null || container.ContainerId == Guid.Empty)
            {
                Logger.Warn($"Can't find rule container info by rule id: [{ruleId}]");
                actionContext.Result = new ObjectResult("Access Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
                return; ;
            }

            var ruleContainerIds = await SecurityTrimmingHelper.GetRuleScopeAsync();
            if (!ruleContainerIds.Any(item => item == container.ContainerId))
            {
                Logger.Warn($"Current user can't access container: [{container.ContainerId}].");
                actionContext.Result = new ObjectResult("Access Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
                return; ;
            }

            return; 
        }
    }
}