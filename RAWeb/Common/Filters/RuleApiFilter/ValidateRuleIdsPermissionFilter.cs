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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters.RuleApiFilter
{
    public class ValidateRuleIdsPermissionFilter : BaseActionFilter
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ValidateRuleIdsPermissionFilter));

        private static readonly IRMRuleDao RuleDao = new RMRuleDao();

        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var ruleIdsObj = actionContext.ActionArguments.Values.FirstOrDefault();
            if(!(ruleIdsObj is List<string> ruleIds))
            {
                return;
            }

            var containers = RuleDao.GetRuleContainersByRuleIds(ruleIds.ConvertAll(item => new Guid(item)));
            if(containers.Count != 1)
            {
                //Logger.Warn($"Has rule can't find in record.");
                Logger.Warn($"Can not delete rules in multiples containers.");
                actionContext.Result = new ObjectResult("Access Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
                return;
            }

            var containerIds = containers.Select(item => item.ContainerId).ToList();
            var ruleContainerIds = await SecurityTrimmingHelper.GetRuleScopeAsync();
            if(containerIds.Any(item => !ruleContainerIds.Contains(item)))
            {
                Logger.Warn($"Current user has can't access container.");
                actionContext.Result = new ObjectResult("Access Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
                return;
            }

        }
    }
}