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
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.SecurityTrimming;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Threading.Tasks;
using AvePoint.RA.Web.Extentions.Authorize;
namespace AvePoint.RA.Web.Common.Filters
{
    public class BaseActionFilter : ActionFilterAttribute
    {
        private RALogger logger = RALogger.GetInstance(typeof(BaseActionFilter));
        public IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            RMIdentity Identity = await context.HttpContext.Request.GetRMIdentityAsync();

            if (!context.ModelState.IsValid)
            {
                logger.Warn($"Current model state is invalid: {Identity?.TenantGroupId}");
                context.Result = new ObjectResult("Model state is invalid") { StatusCode = (int)HttpStatusCode.BadRequest };
            }

            if (null == Identity || !Identity.IsAuthenticated)
            {
                logger.Warn($"user is not authenticated-4: {Identity?.TenantGroupId}");
                context.Result = new ObjectResult("No login") { StatusCode = (int)HttpStatusCode.Forbidden };
            }
            else
            {
                await OnActionAuthenticatedAsync(context);
            }

            await base.OnActionExecutionAsync(context, next);
        }

        protected virtual Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            return Task.CompletedTask;
        }

        public (RMPermissionMasks, RMPermissionExtensionMasks, RMSOPermissionMasks) GetValidMasks(ValidType validType)
        {
            var validILMasks = RMPermissionMasks.SPOAdmin;
            var validILExtensionMasks = RMPermissionExtensionMasks.None;
            var validSOMasks = RMSOPermissionMasks.SPOAdmin;
            if (validType == ValidType.OneDrive)
            {
                validILMasks = RMPermissionMasks.OneDriveAdmin;
                validSOMasks = RMSOPermissionMasks.OneDriveAdmin;
            }
            switch(validType)
            {
                case ValidType.Teams:
                    validILExtensionMasks = RMPermissionExtensionMasks.TeamsAdmin;
                    validSOMasks = RMSOPermissionMasks.TeamsAdmin;
                    validILMasks = RMPermissionMasks.None;
                    break;
                default:
                    break;
            }
            return (validILMasks, validILExtensionMasks, validSOMasks);
        }
    }
}