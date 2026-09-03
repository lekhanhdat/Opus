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
    public class PhysicalResource: BaseResource
    {
        public override List<ResourceItem> Get()
        {
            return new List<ResourceItem>() 
            {
                 new ResourceItem()
                {
                    Key = ResourceKeys.PRM_LocationManagement,
                    Value = ResourceKeys.PRM_LocationManagement.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.PhysicalAdmin,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.PRM_TemplateManagement,
                    Value = ResourceKeys.PRM_TemplateManagement.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.PhysicalAdmin,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.PRM_RecordsManagement,
                    Value = ResourceKeys.PRM_RecordsManagement.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.PhysicalAdmin,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.PRM_BarcodeManagement,
                    Value = ResourceKeys.PRM_BarcodeManagement.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.PhysicalAdmin,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.PRM_BarcodeManagement_Create,
                    Value = ResourceKeys.PRM_BarcodeManagement_Create.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.PhysicalAdmin,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.PRM_BarcodeManagement_Edit,
                    Value = ResourceKeys.PRM_BarcodeManagement_Edit.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.PhysicalAdmin,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.PRM_BarcodeManagement_EditDefault,
                    Value = ResourceKeys.PRM_BarcodeManagement_EditDefault.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.PhysicalAdmin,
                },
                 new ResourceItem()
                {
                    Key = ResourceKeys.PRM_BarcodeTemplate,
                    Value = ResourceKeys.PRM_BarcodeTemplate.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.PhysicalAdmin,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.PRM_PhysicalRecordsBulkImport,
                    Value = ResourceKeys.PRM_PhysicalRecordsBulkImport.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.PhysicalAdmin,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.PRM_RecordsExplorer,
                    Value = ResourceKeys.PRM_RecordsExplorer.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.PhysicalEndUser,
                },
                 new ResourceItem()
                {
                    Key = ResourceKeys.PRM_RecordsExplorer,
                    Value = ResourceKeys.PRM_RecordsExplorer.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ManageHold,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.PRM_EditTemplate,
                    Value = ResourceKeys.PRM_EditTemplate.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.PhysicalAdmin,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.PRM_CreateTemplate,
                    Value = ResourceKeys.PRM_CreateTemplate.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.PhysicalAdmin,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.PRM_MyRequest,
                    Value = ResourceKeys.PRM_MyRequest.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.PhysicalEndUser,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.PRM_ManageHold,
                    Value = ResourceKeys.PRM_ManageHold.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ControlPanelAdmin,
                    PermissionExtension = RMPermissionExtensionMasks.ManageHoldEndUser
                },
                   new ResourceItem()
                {
                    Key = ResourceKeys.PRM_ManageHold,
                    Value = ResourceKeys.PRM_ManageHold.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ManageHold,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.PRM_GlobalSearch,
                    Value = ResourceKeys.PRM_GlobalSearch.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.CommonModuleAccess,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.PRM_ImportHPRM,
                    Value = ResourceKeys.PRM_ImportHPRM.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.PhysicalAdmin,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.PRM_CreateTemplateSuite,
                    Value = ResourceKeys.PRM_CreateTemplateSuite.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.PhysicalAdmin,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.PRM_EditTemplateSuite,
                    Value = ResourceKeys.PRM_EditTemplateSuite.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.PhysicalAdmin,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.PRM_FolderTemplateManagement,
                    Value = ResourceKeys.PRM_FolderTemplateManagement.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.PhysicalAdmin,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.PRM_RecordTemplateManagement,
                    Value = ResourceKeys.PRM_RecordTemplateManagement.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.PhysicalAdmin,
                },
                 new ResourceItem()
                {
                    Key = ResourceKeys.PRM_SetAccessControl,
                    Value = ResourceKeys.PRM_SetAccessControl.ToString(),
                    Permission = RMPermissionMasks.PhysicalEndUser,
                    SubPermission = RMSubPermissionMasks.PhysicalAccessControl,
                },
                 new ResourceItem()
                {
                    Key = ResourceKeys.PRM_BoxCreationRequest,
                    Value = ResourceKeys.PRM_BoxCreationRequest.ToString(),
                    Permission = RMPermissionMasks.PhysicalEndUser,
                    SubPermission = RMSubPermissionMasks.PhysicalBoxCreationRequest,
                },
                 new ResourceItem()
                {
                    Key = ResourceKeys.PRM_FolderCreationRequest,
                    Value = ResourceKeys.PRM_FolderCreationRequest.ToString(),
                    Permission = RMPermissionMasks.PhysicalEndUser,
                    SubPermission = RMSubPermissionMasks.PhysicalFolderCreationRequest,
                },
                 new ResourceItem()
                {
                    Key = ResourceKeys.PRM_FolderLoanRequest,
                    Value = ResourceKeys.PRM_FolderLoanRequest.ToString(),
                    Permission = RMPermissionMasks.PhysicalEndUser,
                    SubPermission = RMSubPermissionMasks.PhysicalFolderLoanRequest,
                },
                 new ResourceItem()
                {
                    Key = ResourceKeys.PRM_FolderLoanReturn,
                    Value = ResourceKeys.PRM_FolderLoanReturn.ToString(),
                    Permission = RMPermissionMasks.PhysicalEndUser,
                    SubPermission = RMSubPermissionMasks.PhysicalFolderLoanReturn,
                },
                 new ResourceItem()
                {
                    Key = ResourceKeys.PRM_MoveRequest,
                    Value = ResourceKeys.PRM_MoveRequest.ToString(),
                    Permission = RMPermissionMasks.PhysicalEndUser,
                    SubPermission = RMSubPermissionMasks.PhysicalMoveRequest,
                }
            };
        }
    }
}