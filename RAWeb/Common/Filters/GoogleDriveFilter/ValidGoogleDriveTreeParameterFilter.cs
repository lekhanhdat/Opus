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
using System.Net;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Service.Services.AccountManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AvePoint.RA.Web.Common.Filters.GoogleDriveFilter;

public class ValidGoogleDriveTreeParameterFilter : BaseActionFilter
{
    private RALogger logger = RALogger.GetInstance(typeof(ValidEXOTreeParameterFilter));
    public RMScopeRoleAssignmentDao RMScopeRoleAssignmentDao = new RMScopeRoleAssignmentDao();

    public ValidGoogleDriveTreeParameterFilter()
    {
    }

    protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
    {
        var treeNode = actionContext.ActionArguments.Values.FirstOrDefault() as RMSampleGoogleTreeNode;

        IUserService userService = new UserService();
        if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.GoogleAdmin)))
        {
            if (treeNode?.Level > (int)NodeLevel.GoogleMyDrive)
            {
                string containerId = GetContainerId(treeNode);
                List<string> userAndGroupUserIds =
                    await userService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupUserIds))
                {
                    logger.Info("No access on container.");
                    actionContext.Result = new ObjectResult("Access  Denied(container)")
                        { StatusCode = (int)HttpStatusCode.Forbidden };
                }
            }
        }
    }
    private string GetContainerId(RMSampleGoogleTreeNode treeNode)
    {
        if (treeNode.Level == (int)NodeLevel.WebApplication)
        {
            return treeNode.Id;
        }
        else
        {
            return GetContainerId(treeNode.Parent);
        }
    }
}