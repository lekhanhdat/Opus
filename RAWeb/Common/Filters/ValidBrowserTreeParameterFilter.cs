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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Service.Services.AccountManager;
using AvePoint.RA.Web.Extentions.Authorize;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RATeams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters
{
    public class ValidSampleTreeParameterFilter : BaseActionFilter
    {
        private RALogger logger = RALogger.GetInstance(typeof(ValidSampleTreeParameterFilter));
        public ValidType ValidType { get; set; }

        private IRMScopeRoleAssignmentDao RMScopeRoleAssignmentDao => PlatformWindsorManager.GetService<IRMScopeRoleAssignmentDao>();
        private IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        public ValidSampleTreeParameterFilter()
        {
        }
        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var parameterObj = actionContext.ActionArguments.Values.FirstOrDefault();
            var treeNode = parameterObj as RMSPSampleTreeNode;
            if (treeNode == null)
            {
                treeNode = SerializerHelper.DeserializeByJsonConvert<RMSPSampleTreeNode>(parameterObj?.ToString());
            }

            var validTypeUseILMasks = new List<ValidType> { ValidType.SharePointOnline, ValidType.OneDrive };
            var validTypeUseILExtensionMasks = new List<ValidType> { ValidType.Teams };

            var (validILMasks, validILExtensionMasks, validSOMasks) = GetValidMasks(ValidType);

            IUserService userService = new UserService();

            if (!((validTypeUseILMasks.Contains(ValidType) && await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(validILMasks))
                || (validTypeUseILExtensionMasks.Contains(ValidType) && await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(validILExtensionMasks)))
                && !(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(validSOMasks)))
            {
                if (treeNode.Level >= (int)NodeLevel.WebApplication)
                {
                    string containerId = GetContainerId(treeNode);
                    List<string> userAndGroupUserIds = await userService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                    if (TeamsPermissionHelper.HasUpgradeTeamsFeature() && containerId == "41cfe969-e07b-45cb-a7d0-b022f967e929")
                    {
                        var contentSourcePermission = RMScopeRoleAssignmentDao.GetSourceFlagsByUser(userAndGroupUserIds);
                        if (!contentSourcePermission.Contains((int)SourceFlag.Teams))
                        {
                            logger.Info("No access on container.");
                            actionContext.Result = new ObjectResult("Access  Denied(container)") { StatusCode = (int)HttpStatusCode.Forbidden };
                        }
                    }
                    else if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupUserIds))
                    {
                        logger.Info("No access on container.");
                        actionContext.Result = new ObjectResult("Access  Denied(container)") { StatusCode = (int)HttpStatusCode.Forbidden };
                    }
                }
            }
        }

        private string GetContainerId(RMSPSampleTreeNode treeNode)
        {
            if (treeNode == null)
            {
                return string.Empty;
            }

            if (treeNode.Level == (int)NodeLevel.WebApplication)
            {
                return treeNode.Id;
            }

            if (treeNode.Parent == null)
            {
                return string.Empty;
            }

            return GetContainerId(treeNode.Parent);
        }

    }

    public class ValidSPTreeParameterFilter : BaseActionFilter
    {
        public ValidType ValidType { get; set; }
        private RALogger logger = RALogger.GetInstance(typeof(ValidSPTreeParameterFilter));

        public RMScopeRoleAssignmentDao RMScopeRoleAssignmentDao = new RMScopeRoleAssignmentDao();
        public ValidSPTreeParameterFilter()
        {
        }
        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var validTypeUseILMasks = new List<ValidType> { ValidType.SharePointOnline, ValidType.OneDrive };
            var validTypeUseILExtensionMasks = new List<ValidType> { ValidType.Teams };
            var parameterObj = actionContext.ActionArguments.Values.FirstOrDefault();
            var treeNode = parameterObj as RMSPTreeNode;
            if (treeNode == null)
            {
                treeNode = SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(parameterObj?.ToString());
            }

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
                if (treeNode.Level >= (int)NodeLevel.WebApplication)
                {
                    string containerId = TreeNodeUtil.GetSPContainderId(treeNode);
                    List<string> userAndGroupUserIds = await userService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                    if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupUserIds))
                    {
                        logger.Info("No access on container.");
                        actionContext.Result = new ObjectResult("Access  Denied(container)") { StatusCode = (int)HttpStatusCode.Forbidden };
                    }
                }
            }
        }
    }

    public class ValidEXOTreeParameterFilter : BaseActionFilter
    {
        private RALogger logger = RALogger.GetInstance(typeof(ValidEXOTreeParameterFilter));
        public RMScopeRoleAssignmentDao RMScopeRoleAssignmentDao = new RMScopeRoleAssignmentDao();
        public ValidEXOTreeParameterFilter()
        {
        }
        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var treeNode = actionContext.ActionArguments.Values.FirstOrDefault() as RMSampleEXOTreeNode;

            IUserService userService = new UserService();
            if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOAdmin)))
            {
                if (treeNode?.Level > (int)NodeLevel.ExchangeOnlineFarm)
                {
                    string containerId = GetContainerId(treeNode);
                    List<string> userAndGroupUserIds = await userService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                    if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupUserIds))
                    {
                        logger.Info("No access on container.");
                        actionContext.Result = new ObjectResult("Access  Denied(container)") { StatusCode = (int)HttpStatusCode.Forbidden };
                    }
                }
            }
        }
        private string GetContainerId(RMSampleEXOTreeNode treeNode)
        {
            if (treeNode.Level == (int)NodeLevel.ExchangeOnlineMailboxGroup)
            {
                return treeNode.Id;
            }
            else
            {
                return GetContainerId(treeNode.Parent);
            }
        }
    }

    public class ValidSPSampleTreeParameterFilter : BaseActionFilter
    {
        private RALogger logger = RALogger.GetInstance(typeof(ValidSPSampleTreeParameterFilter));
        public ValidType ValidType { get; set; }
        public RMScopeRoleAssignmentDao RMScopeRoleAssignmentDao = new RMScopeRoleAssignmentDao();
        private IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        public ValidSPSampleTreeParameterFilter()
        {
        }
        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var validTypeUseILMasks = new List<ValidType> { ValidType.SharePointOnline, ValidType.OneDrive };
            var validTypeUseILExtensionMasks = new List<ValidType> { ValidType.Teams };
            var treeNode = actionContext.ActionArguments.Values.FirstOrDefault() as RMSPSampleTreeNode;
            var (validILMasks, validILExtensionMasks, validSOMasks) = GetValidMasks(ValidType);
            IUserService userService = new UserService();
            if (!((validTypeUseILMasks.Contains(ValidType) && await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(validILMasks))
                || (validTypeUseILExtensionMasks.Contains(ValidType) && await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(validILExtensionMasks)))
                && !(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(validSOMasks)))
            {
                if (treeNode?.Level >= (int)NodeLevel.WebApplication)
                {
                    string containerId = GetContainerId(treeNode);
                    List<string> userAndGroupUserIds = await userService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                    if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupUserIds))
                    {
                        logger.Info("No access on container.");
                        actionContext.Result = new ObjectResult("Access  Denied(container)") { StatusCode = (int)HttpStatusCode.Forbidden };
                    }
                }
            }
        }
        private string GetContainerId(RMSPSampleTreeNode treeNode)
        {
            if (treeNode == null)
            {
                return string.Empty;
            }

            if (treeNode.Level == (int)NodeLevel.WebApplication)
            {
                return treeNode.Id;
            }

            if (treeNode.Parent == null)
            {
                return string.Empty;
            }

            return GetContainerId(treeNode.Parent);
        }
    }


    public class ValidCurrentEXOSettingsParameterFilter : BaseActionFilter
    {
        private RALogger logger = RALogger.GetInstance(typeof(ValidCurrentEXOSettingsParameterFilter));
        public RMScopeRoleAssignmentDao RMScopeRoleAssignmentDao = new RMScopeRoleAssignmentDao();
        public ValidCurrentEXOSettingsParameterFilter()
        {
        }
        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var treeNode = actionContext.ActionArguments.Values.FirstOrDefault() as CurrentSettingsInfo;
            IUserService userService = new UserService();
            if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.EXOAdmin)))
            {
                var containerId = treeNode?.GroupId;
                List<string> userAndGroupUserIds = await userService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(containerId), userAndGroupUserIds))
                {
                    logger.Info("No access on container.");
                    actionContext.Result = new ObjectResult("Access  Denied(container)") { StatusCode = (int)HttpStatusCode.Forbidden };
                }
            }
        }
    }
}