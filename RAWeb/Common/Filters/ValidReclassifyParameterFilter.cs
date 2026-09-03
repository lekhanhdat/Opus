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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters
{
    public class ValidReclassifyParameterFilter : BaseActionFilter
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ValidReclassifyParameterFilter));

        public static ISecurityGroupManagementService SecurityGroupManagementService => PlatformWindsorManager.GetService<ISecurityGroupManagementService>();

        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var parameter = actionContext.ActionArguments.Values.FirstOrDefault();

            if (parameter == null)
            {
                Logger.Warn($"The parameter is empty.");
                return;
            }

            if (!(parameter is ChangeTermDto changeTermDto))
            {
                Logger.Warn("The parameter is not match ChangeTermDto.");
                return;
            }

            var termInfo = changeTermDto.TermInfo;
            if (termInfo == null)
            {
                Logger.Warn("The parameter TermInfo is NULL.");
                return;
            }

            if (!changeTermDto.CanReclassifyAllTerm)
            {
                var hasPermission = await SecurityGroupManagementService.DoesUserHasPermisionToTermAsync(TenantLocalValue.LogonUserId, Contract.RMWeb.CP.SecurityTermLevel.Term, new List<Guid> { termInfo.UniqueId });
                if (!hasPermission)
                {
                    Logger.Warn($"No permission to term [{termInfo.UniqueId}].");
                    actionContext.Result = new ObjectResult("No permission to term.") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return;
                }
            }

            return;
        }
    }
}