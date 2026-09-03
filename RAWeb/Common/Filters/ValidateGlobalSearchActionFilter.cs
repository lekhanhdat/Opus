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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Service.Services.AccountManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters
{
    public class ValidateGlobalSearchActionFilter : BaseActionFilter
    {
        private RALogger logger = RALogger.GetInstance(typeof(ValidateGlobalSearchActionFilter));
        
        public IRMScopeRoleAssignmentDao RMScopeRoleAssignmentDao  => PlatformWindsorManager.GetService<IRMScopeRoleAssignmentDao>();
        public ValidateGlobalSearchActionFilter()
        {
        }
        protected override async Task OnActionAuthenticatedAsync(ActionExecutingContext actionContext)
        {
            var dto = actionContext.ActionArguments.Values.FirstOrDefault() as GlobalSearchActionDto;
            IUserService userService = new UserService();
            if (!(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.EletricRecordExplorerAdmin)))
            {
                if (!(await ValidatePermissionAsync(dto, userService)))
                {
                    logger.Info("Access denied for global search action.");
                    actionContext.Result = new ObjectResult("Access  Denied For Global Search Action.") { StatusCode = (int)HttpStatusCode.Forbidden };
                }
            }
        }

        private async Task<bool> ValidatePermissionAsync(GlobalSearchActionDto globalSearchActionDto, IUserService userService)
        {
            bool hasPermission = false;
            switch (globalSearchActionDto.Action)
            {
                case GlobalSearchAction.AccessControl:
                    if (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.PhysicalEndUser))
                    {
                        hasPermission = true;
                    }
                    break;
                case GlobalSearchAction.DeclareRecords:
                case GlobalSearchAction.UnDeclareRecords:
                    hasPermission = await ValidateDeclarePermissionAsync(userService, globalSearchActionDto);
                    break;
                case GlobalSearchAction.MoveTo:
                    hasPermission = await ValidateMovetoPremissionAsync(userService, globalSearchActionDto);
                    break;
                case GlobalSearchAction.Reclassify:
                    hasPermission = await ValidateReclassifyPermissionAsync(userService, globalSearchActionDto);
                    break;
                case GlobalSearchAction.PhysicalBulkUpdate:
                    hasPermission = await ValidatePhysicalBulkUpdatePermissionAsync(userService, globalSearchActionDto);
                    break;
            }
            return hasPermission;
        }

        private async Task<bool> ValidateDeclarePermissionAsync(IUserService userService, GlobalSearchActionDto globalSearchActionDto)
        {
            bool hasPermission = false;
            switch ((SourceFlag)globalSearchActionDto.SourceFlag)
            {
                case SourceFlag.SharePoint:
                    if (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOEnduser))
                    {
                        hasPermission = true;
                    }
                    break;
                case SourceFlag.SharePointOnPrem:
                    if (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOnPremEnduser))
                    {
                        hasPermission = true;
                    }
                    break;
                case SourceFlag.OneDrive:
                    if (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.OneDriveEnduser))
                    {
                        hasPermission = true;
                    }
                    break;
                case SourceFlag.Teams:
                    if (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.TeamsEndUser))
                    {
                        hasPermission = true;
                    }
                    break;
            }
            return hasPermission;
        }

        private async Task<bool> ValidateReclassifyPermissionAsync(IUserService userService, GlobalSearchActionDto globalSearchActionDto)
        {
            bool hasPermission = false;
            switch ((SourceFlag)globalSearchActionDto.SourceFlag)
            {
                case SourceFlag.SharePoint:
                    if (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOEnduser))
                    {
                        hasPermission = true;
                    }
                    break;
                case SourceFlag.Exchange:
                    if (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.EXOEnduser))
                    {
                        hasPermission = true;
                    }
                    break;
                case SourceFlag.Physical:
                    if (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.PhysicalAdmin))
                    {
                        hasPermission = true;
                    }
                    break;
                case SourceFlag.FileSystem:
                    if (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.FSAdmin))
                    {
                        if (!globalSearchActionDto.ForceDiscoverAll)
                        {
                            ChangeTermDto changeTermDto = JsonConvert.DeserializeObject<ChangeTermDto>(globalSearchActionDto.ActionExtension.ToString());
                            List<string> userAndGroupUserIds = await userService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                            hasPermission = ValidateIds(changeTermDto.FSRecordIds, userAndGroupUserIds, SourceFlag.FileSystem);
                        }
                        else
                        {
                            hasPermission = true;
                        }
                    }
                    break;
                case SourceFlag.SharePointOnPrem:
                    if (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOnPremEnduser))
                    {
                        hasPermission = true;
                    }
                    break;
                case SourceFlag.OneDrive:
                    if (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.OneDriveEnduser))
                    {
                        hasPermission = true;
                    }
                    break;
                case SourceFlag.AzureFileShare:
                    if(await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.AzureFSEndUser))
                    {
                        hasPermission = true;
                    }
                    break;
                case SourceFlag.Box:
                    if (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.BoxEndUser))
                    {
                        hasPermission = true;
                    }
                    break;
                case SourceFlag.Google:
                    if (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.GoogleEndUser))
                    {
                        hasPermission = true;
                    }
                    break;
                case SourceFlag.Teams:
                    if (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.TeamsEndUser))
                    {
                        hasPermission = true;
                    }
                    break;
            }
            return hasPermission;
        }

        private async Task<bool> ValidatePhysicalBulkUpdatePermissionAsync(IUserService userService, GlobalSearchActionDto globalSearchActionDto)
        {
            return await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.PhysicalAdmin);
        }


        private async Task<bool> ValidateMovetoPremissionAsync(IUserService userService, GlobalSearchActionDto globalSearchActionDto)
        {
            bool hasPermission = false;
            switch ((SourceFlag)globalSearchActionDto.SourceFlag)
            {
                case SourceFlag.SharePoint:
                    if (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOEnduser))
                    {
                        MoveToDto moveDto = JsonConvert.DeserializeObject<MoveToDto>(globalSearchActionDto.ActionExtension.ToString());
                        List<string> userAndGroupUserIds = await userService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                        hasPermission = ValidateIds(moveDto.SourceRecords.Select(r => r.Id).ToList(), userAndGroupUserIds, (SourceFlag)globalSearchActionDto.SourceFlag);
                        if (hasPermission)
                        {
                            hasPermission = await ValidateDestinationAsync(moveDto, userService);
                        }
                    }
                    break;
                case SourceFlag.OneDrive:
                    if (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.OneDriveEnduser))
                    {
                        MoveToDto moveDto = JsonConvert.DeserializeObject<MoveToDto>(globalSearchActionDto.ActionExtension.ToString());
                        List<string> userAndGroupUserIds = await userService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                        hasPermission = ValidateIds(moveDto.SourceRecords.Select(r => r.Id).ToList(), userAndGroupUserIds, (SourceFlag)globalSearchActionDto.SourceFlag);
                        if (hasPermission)
                        {
                            hasPermission = await ValidateDestinationAsync(moveDto, userService);
                        }
                    }
                    break;
                case SourceFlag.Teams:
                    if (await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.TeamsEndUser))
                    {
                        MoveToDto moveDto = JsonConvert.DeserializeObject<MoveToDto>(globalSearchActionDto.ActionExtension.ToString());
                        List<string> userAndGroupUserIds = await userService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                        hasPermission = ValidateIds(moveDto.SourceRecords.Select(r => r.Id).ToList(), userAndGroupUserIds, (SourceFlag)globalSearchActionDto.SourceFlag);
                        if (hasPermission)
                        {
                            hasPermission = await ValidateDestinationAsync(moveDto, userService);
                        }
                    }
                    break;
            }
            return hasPermission;
        }

        private async Task<bool> ValidateDestinationAsync(MoveToDto moveDto, IUserService userService)
        {
            RemoteSiteCollection site = null;
            if (moveDto.DestMode == Contract.RMWeb.DestMode.SharePoint)
            {
                if (moveDto.IsSpecifyLocation)
                {
                    site = RABrowserClient.GetRemoteSiteCollectionByListUrl(moveDto.LocationPath);
                }
                else
                {
                    var siteCollNode = GetSiteCollectionNode(moveDto.SPTree);
                    site = RABrowserClient.GetRemoteSiteCollectionById(siteCollNode.SPObjectId);
                }
            }
            if (site == null)
            {
                logger.Warn("Cannot find site, site url:{0}", moveDto.IsSpecifyLocation ? moveDto.LocationPath : moveDto.SPTree.FullPath);
                return false;
            }
            else
            {

                List<string> userAndGroupUserIds = await userService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(site.parentId), userAndGroupUserIds))
                {
                    logger.Info($"Current user doesn't have permission on container. Container Id:{site.parentId}.DesUrl:{(moveDto.IsSpecifyLocation ? moveDto.LocationPath : moveDto.SPTree.FullPath)}.");
                    return false;
                }
            }

            return true;
        }

        public RMSPTreeNode GetSiteCollectionNode(RMSPTreeNode node)
        {
            while (node.Level != (int)NodeLevel.SiteCollection)
            {
                node = node.Parent;
            }
            return node;
        }

        private bool ValidateIds(List<Guid> recordIds, List<string> userAndGroupUserIds, SourceFlag flag)
        {
            ExplorerDao ExplorerDao = new ExplorerDao();
            List<Record> allRecord = ExplorerDao.GetRecordByIds(recordIds);
            var nonSPRecords = allRecord.Where(r => r.SourceFlag != (int)flag).ToList();
            if (nonSPRecords.Count > 0)
            {
                logger.Info("Contains invalid data.");
                return false;
            }

            if (flag == SourceFlag.SharePoint || flag == SourceFlag.Exchange || flag == SourceFlag.OneDrive || flag == SourceFlag.Teams)
            {
                var nonUpgradeData = allRecord.Where(r => string.IsNullOrWhiteSpace(r.ContainerId)).ToList();
                if (nonUpgradeData.Count > 0)
                {
                    logger.Info("Contains non-upgrade data.");
                    return false;
                }

                List<string> containerIds = allRecord.Select(r => r.ContainerId).Distinct().ToList();
                if (containerIds.Count > 0 && !RMScopeRoleAssignmentDao.ValidateContainerIdPermission(containerIds, userAndGroupUserIds))
                {
                    logger.Info($"No access on container");
                    return false;
                }
            }

            return true;
        }
    }
}