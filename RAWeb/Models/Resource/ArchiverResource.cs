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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using System.Collections.Generic;

namespace AvePoint.RA.Web.Models.Resource
{
    public class ArchiverResource : BaseResource
    {
        public List<ResourceItem> Resource { get; private set; } = new List<ResourceItem>
        {
               new ResourceItem()
               {
                    Key = ResourceKeys.CP_SuperUserConfiguration,
                    Value = ResourceKeys.CP_SuperUserConfiguration.ToUrl(RouterUrl_Root),
                    SOPermission = RMSOPermissionMasks.ControlPanelAdmin
               },
               new ResourceItem()
               {
                    Key = ResourceKeys.CP_EndUserRestoreSettings,
                    Value = ResourceKeys.CP_EndUserRestoreSettings.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ControlPanelAdmin,
                    SOPermission = RMSOPermissionMasks.ControlPanelAdmin
               },
               new ResourceItem()
               {
                    Key = ResourceKeys.Archiver_ContentSource_Tab,
                    Value = ResourceKeys.Archiver_ContentSource_Tab.ToString(),
                    SOPermission = RMSOPermissionMasks.ContentRepositoyEnduser
               },
               new ResourceItem()
               {
                    Key = ResourceKeys.Archiver_RestoreCenter,
                    Value = ResourceKeys.Archiver_RestoreCenter.ToUrl(GResources.RouterUrl_Root),
                    Permission = RMPermissionMasks.SPOEnduser & RMPermissionMasks.OneDriveEnduser,
                    SOPermission = RMSOPermissionMasks.ContentRepositoyEnduser,
                    PermissionExtension = RMPermissionExtensionMasks.TeamsEndUser,
               },
               new ResourceItem()
               {
                    Key = ResourceKeys.Archiver_RestoreCenter,
                    Value = ResourceKeys.Archiver_RestoreCenter.ToUrl(GResources.RouterUrl_Root),
                    SOPermission = RMSOPermissionMasks.RestoreCenterSearch,
                    PermissionExtension = RMPermissionExtensionMasks.GoogleEndUser,
               },
               new ResourceItem()
               {
                    Key = ResourceKeys.Archiver_RestoreCenter_Search,
                    Value = ResourceKeys.Archiver_RestoreCenter_Search.ToString(),
                    SOPermission = RMSOPermissionMasks.RestoreCenterSearch
               },
               new ResourceItem()
               {
                    Key = ResourceKeys.Archiver_RestoreCenter,
                    Value = ResourceKeys.Archiver_RestoreCenter.ToUrl(GResources.RouterUrl_Root),
                    Permission = RMPermissionMasks.FSEnduser,
                    PermissionExtension = RMPermissionExtensionMasks.GoogleEndUser,
               },

               //new ResourceItem()
               //{
               //     Key = ResourceKeys.Archiver_RestoreCenter_Search,
               //     Value = ResourceKeys.Archiver_RestoreCenter_Search.ToString(),
               //     SOPermission = RMSOPermissionMasks.ContentRepositoyEnduser,
               //     PermissionExtension = RMPermissionExtensionMasks.RestoreCenterAccess,
               //     SubPermission = RMSubPermissionMasks.RestoreCenterSearch,
               //},
               //new ResourceItem()
               //{
               //     Key = ResourceKeys.Archiver_RestoreCenter_SearchAndExport,
               //     Value = ResourceKeys.Archiver_RestoreCenter_SearchAndExport.ToString(),
               //     SOPermission = RMSOPermissionMasks.ContentRepositoyEnduser,
               //     PermissionExtension = RMPermissionExtensionMasks.RestoreCenterAccess,
               //     SubPermission = RMSubPermissionMasks.RestoreCenterExport,
               //},
               //new ResourceItem()
               //{
               //     Key = ResourceKeys.Archiver_RestoreCenter_FullControl,
               //     Value = ResourceKeys.Archiver_RestoreCenter_FullControl.ToString(),
               //     SOPermission = RMSOPermissionMasks.ContentRepositoyEnduser,
               //     PermissionExtension = RMPermissionExtensionMasks.RestoreCenterAccess,
               //     SubPermission = RMSubPermissionMasks.RestoreCenterFullControl,
               //},
               new ResourceItem()
               {
                    Key = ResourceKeys.Archiver_CP_Schedule_Settings,
                    Value = ResourceKeys.Archiver_CP_Schedule_Settings.ToString(),
                    SOPermission = RMSOPermissionMasks.ControlPanelAdmin,
                    PermissionExtension = RMPermissionExtensionMasks.GoogleEndUser,
               },
        };
        public override List<ResourceItem> Get()
        {
            return Resource;
        }
    }
}
