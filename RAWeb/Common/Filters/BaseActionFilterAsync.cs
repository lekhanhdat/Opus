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
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.DB.SecurityTrimming;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Threading.Tasks;
using AvePoint.RA.Web.Extentions.Authorize;

namespace AvePoint.RA.Web.Common.Filters
{
    public class BaseActionFilterAsync : ActionFilterAttribute
    {
        private RALogger logger = RALogger.GetInstance(typeof(BaseActionFilterAsync));
        public override async Task OnActionExecutionAsync(ActionExecutingContext actionContext, ActionExecutionDelegate next)
        {
            await base.OnActionExecutionAsync(actionContext, next);
            RMIdentity Identity = await actionContext.HttpContext.Request.GetRMIdentityAsync();
            if (null == Identity || !Identity.IsAuthenticated)
            {
                logger.Warn($"user is not authenticated-3: {Identity?.TenantGroupId}");
                actionContext.Result = new ObjectResult("No login") { StatusCode = (int)HttpStatusCode.Forbidden };
                return;
            }

            await next();
        }
    }
}
