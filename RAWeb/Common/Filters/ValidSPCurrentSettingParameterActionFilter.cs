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
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Service.Services.AccountManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters
{
    public class ValidSPCurrentSettingParameterActionFilter : BaseActionFilter
    {
        private RALogger logger = RALogger.GetInstance(typeof(ValidSPCurrentSettingParameterActionFilter));
        public ValidType ValidType { get; set; }
        public IRMScopeRoleAssignmentDao RMScopeRoleAssignmentDao => PlatformWindsorManager.GetService<IRMScopeRoleAssignmentDao>();//Temp Method TO DO important add to common method 
        public ValidSPCurrentSettingParameterActionFilter()
        {
        }
        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var validTypeUseILMasks = new List<ValidType> { ValidType.SharePointOnline, ValidType.OneDrive };
            var validTypeUseILExtensionMasks = new List<ValidType> { ValidType.Teams };
            var setting = actionContext.ActionArguments.Values.FirstOrDefault() as CurrentSettingsInfo;
            RMPermissionMasks validMask = RMPermissionMasks.SPOAdmin;
            RMPermissionExtensionMasks validExtensionMask = RMPermissionExtensionMasks.TeamsAdmin;
            if (ValidType == ValidType.OneDrive)
            {
                validMask = RMPermissionMasks.OneDriveAdmin;
            }
            IUserService userService = new UserService();
            if (!((validTypeUseILMasks.Contains(ValidType) && await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(validMask))
                || (validTypeUseILExtensionMasks.Contains(ValidType) && await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(validExtensionMask))))
            {
                var nodes = setting?.spTreeNodes;
                if (nodes != null)
                {
                    List<string> userAndGroupUserIds = await userService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                    foreach (var node in nodes)
                    {
                        if (node.Level >= (int)NodeLevel.WebApplication)
                        {
                            string containerId = TreeNodeUtil.GetSPContainderId(node);
                            if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupUserIds))
                            {
                                logger.Warn($"Current user has no access on container.");
                                actionContext.Result = new ObjectResult("Access  Denied(container)") { StatusCode = (int)HttpStatusCode.Forbidden };
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}