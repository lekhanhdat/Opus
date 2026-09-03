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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.SourceTreeQuery.Model;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Service.Services.AccountManager;
using AvePoint.RA.Web.Extentions.Authorize;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters.SourceTreeNodeFilters
{
    public abstract class SourceTreeNodePermissionFilter : BaseActionFilter
    {

        private static readonly RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly IUserService UserService = new UserService();

        private static readonly IRMScopeRoleAssignmentDao ScopeRoleAssignmentDao = new RMScopeRoleAssignmentDao();

        protected abstract SourceFlag Source { get; }

        protected abstract RMPermissionMasks AdminPermissionMasks { get; }

        protected abstract RMPermissionExtensionMasks AdminExtensionPermissionMasks { get; }

        protected abstract PermissionMaskMode Mode { get; }

        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var nodeObject = actionContext.ActionArguments.Values.FirstOrDefault();
            if(nodeObject == null)
            {
                Logger.Warn($"The [{Source}] query request node parameter is empty.");
                return;
            }

            if (!(nodeObject is SourceTreeNode node))
            {
                var type = nodeObject.GetType();
                node = type.GetProperty("Node").GetValue(nodeObject) as SourceTreeNode;
            }
            
            if(node.Level == Contract.RMWeb.Tree.Base.RMNodeLevel.Root)
            {
                return;
            }

            if (Mode == PermissionMaskMode.Normal && await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(AdminPermissionMasks))
            {
                return;
            }

            if (Mode == PermissionMaskMode.Extension && await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(AdminExtensionPermissionMasks))
            {
                return;
            }

            var containerId = node.ContainerId;
            var userAndGroupIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
            if (!ScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupIds))
            {
                Logger.Warn($"The current user no access [{Source}] container: [{containerId}].");
                actionContext.Result = new ObjectResult("Access  Denied(container)") { StatusCode = (int)HttpStatusCode.Forbidden };
            }
        }
    }

    public enum PermissionMaskMode
    {
        Normal = 0,
        Extension = 1,
    }
}