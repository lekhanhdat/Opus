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
using AvePoint.RA.Contract.RoleAssignments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AvePoint.RA.Web.Models.Resource
{
    public class CommonResource : BaseResource
    {
        public override List<ResourceItem> Get()
        {
            return new List<ResourceItem>()
            {
                 new ResourceItem()
                {
                    Key = ResourceKeys.Home,
                    Value = ResourceKeys.Home.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.CommonModuleAccess,
                    SOPermission = RMSOPermissionMasks.CommonModuleAccess,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.Index,
                    Value = ResourceKeys.Index.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.CommonModuleAccess,
                    SOPermission = RMSOPermissionMasks.CommonModuleAccess,
                },

                new ResourceItem()
                {
                    Key = ResourceKeys.Source_SP,
                    Value = ResourceKeys.Source_SP.ToString(),
                    Permission = RMPermissionMasks.SPOEnduser,
                    SOPermission = RMSOPermissionMasks.SPOEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.Source_EXO,
                    Value = ResourceKeys.Source_EXO.ToString(),
                    Permission = RMPermissionMasks.EXOEnduser,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.Source_FS,
                    Value = ResourceKeys.Source_FS.ToString(),
                    Permission = RMPermissionMasks.FSAdmin,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.Source_Phy,
                    Value = ResourceKeys.Source_Phy.ToString(),
                    Permission = RMPermissionMasks.PhysicalAdmin,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.Source_LSP,
                    Value = ResourceKeys.Source_LSP.ToString(),
                    Permission = RMPermissionMasks.SPOnPremEnduser,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.Source_OneDrive,
                    Value = ResourceKeys.Source_OneDrive.ToString(),
                    Permission = RMPermissionMasks.OneDriveEnduser,
                    SOPermission = RMSOPermissionMasks.OneDriveEnduser
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.Source_AzureFile,
                    Value = ResourceKeys.Source_AzureFile.ToString(),
                    PermissionExtension = RMPermissionExtensionMasks.AzureFSAdmin,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.Source_Box,
                    Value = ResourceKeys.Source_Box.ToString(),
                    PermissionExtension = RMPermissionExtensionMasks.BoxAdmin, //need change
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.Source_Google,
                    Value = ResourceKeys.Source_Google.ToString(),
                    PermissionExtension = RMPermissionExtensionMasks.GoogleAdmin,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.Source_Teams,
                    Value = ResourceKeys.Source_Teams.ToString(),
                    PermissionExtension = RMPermissionExtensionMasks.TeamsEndUser,
                    SOPermission = RMSOPermissionMasks.TeamsEndUser
                },
            };
        }
    }
}