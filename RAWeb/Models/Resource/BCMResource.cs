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
    public class BCMResource : BaseResource
    {
        public List<ResourceItem> Resource { get; private set; } = new List<ResourceItem> 
        {
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_ContentRepositoryManagement_UniqueId,
                    Value = ResourceKeys.BCM_ContentRepositoryManagement_UniqueId.ToString(),
                    Permission = RMPermissionMasks.ContentRepositoyAdmin,
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_ContentRepositoryManagement_Import,
                    Value = ResourceKeys.BCM_ContentRepositoryManagement_Import.ToString(),
                    Permission = RMPermissionMasks.SPOEnduser,
                    PermissionExtension = RMPermissionExtensionMasks.TeamsEndUser
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_ContentRepositoryManagement_Export,
                    Value = ResourceKeys.BCM_ContentRepositoryManagement_Export.ToString(),
                    Permission = RMPermissionMasks.ContentRepositoyEnduser
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_ContentRepositoryManagement_ExportSO,
                    Value = ResourceKeys.BCM_ContentRepositoryManagement_ExportSO.ToString(),
                    SOPermission = RMSOPermissionMasks.ContentRepositoyEnduser,
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_ContentRepositoryManagement_Classification,
                    Value = ResourceKeys.BCM_ContentRepositoryManagement_Classification.ToString(),
                    Permission = RMPermissionMasks.ContentRepositoyAdmin,
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_TermManagement_Admin,
                    Value = ResourceKeys.BCM_TermManagement_Admin.ToString(),
                    Permission = RMPermissionMasks.TermManagementAdmin,
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_ContentRepositoryManagement,
                    Value = ResourceKeys.BCM_ContentRepositoryManagement.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ContentRepositoyAdmin,
                    SOPermission = RMSOPermissionMasks.ControlPanelAdmin,
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_ContentSourcesForSharePointOnline,
                    Value = ResourceKeys.BCM_ContentSourcesForSharePointOnline.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.SPOEnduser,
                    SOPermission = RMSOPermissionMasks.SPOEnduser
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_ContentSourcesForExchangeOnline,
                    Value = ResourceKeys.BCM_ContentSourcesForExchangeOnline.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.EXOEnduser,
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_ContentSourcesForPhysicalRecords,
                    Value = ResourceKeys.BCM_ContentSourcesForPhysicalRecords.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.PhysicalAdmin,
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_ContentSourcesForFileSystem,
                    Value = ResourceKeys.BCM_ContentSourcesForFileSystem.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.FSEnduser,
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_ContentSourcesForSharePointOnPremises,
                    Value = ResourceKeys.BCM_ContentSourcesForSharePointOnPremises.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.SPOnPremEnduser,
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_ContentSourcesForOneDriveforBusiness,
                    Value = ResourceKeys.BCM_ContentSourcesForOneDriveforBusiness.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.OneDriveEnduser,
                    SOPermission = RMSOPermissionMasks.OneDriveEnduser
                },
             new ResourceItem()
                {
                    Key = ResourceKeys.BCM_ContentSourcesForAzureFiles,
                    Value = ResourceKeys.BCM_ContentSourcesForAzureFiles.ToUrl(RouterUrl_Root),
                    PermissionExtension = RMPermissionExtensionMasks.AzureFSEndUser,
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_ContentSourcesForBox,
                    Value = ResourceKeys.BCM_ContentSourcesForBox.ToUrl(RouterUrl_Root),
                    PermissionExtension = RMPermissionExtensionMasks.BoxEndUser,    //need change
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_ContentSourcesForGoogle,
                    Value = ResourceKeys.BCM_ContentSourcesForGoogle.ToUrl(RouterUrl_Root),
                    PermissionExtension = RMPermissionExtensionMasks.GoogleEndUser,    //need change
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_ContentSourcesForTeams,
                    Value = ResourceKeys.BCM_ContentSourcesForTeams.ToUrl(RouterUrl_Root),
                    PermissionExtension = RMPermissionExtensionMasks.TeamsEndUser,
                    SOPermission = RMSOPermissionMasks.TeamsEndUser
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_ContentSourcesForTeams_Switch,
                    Value = ResourceKeys.BCM_ContentSourcesForTeams_Switch.ToUrl(RouterUrl_Root),
                    PermissionExtension = RMPermissionExtensionMasks.TeamsEndUser,
                    SOPermission = RMSOPermissionMasks.TeamsEndUser
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_RecordsExplorer,
                    Value = ResourceKeys.BCM_RecordsExplorer.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.EletricRecordExplorerEnduser,
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_GlobalSearch,
                    Value = ResourceKeys.BCM_GlobalSearch.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.CommonModuleAccess
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_HybridSearch,
                    Value = ResourceKeys.BCM_HybridSearch.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.CommonModuleAccess,
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_TermManagement,
                    Value = ResourceKeys.BCM_TermManagement.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.TermManagementEnduser,
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_ContentRepositoryManagement,
                    Value = ResourceKeys.BCM_ContentRepositoryManagement.ToUrl(),
                    Permission = RMPermissionMasks.ContentRepositoyEnduser,
                    SOPermission = RMSOPermissionMasks.ContentRepositoyEnduser,
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_ContentRepositoryManagement,
                    Value = ResourceKeys.BCM_ContentRepositoryManagement.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ContentRepositoyEnduser,
                    SOPermission = RMSOPermissionMasks.ContentRepositoyEnduser,
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_FSConnGroup,
                    Value = ResourceKeys.BCM_FSConnGroup.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.FSAdmin,
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_FSConnectionDetail,
                    Value = ResourceKeys.BCM_FSConnectionDetail.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.FSAdmin,
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_FSConnectionMonitor,
                    Value = ResourceKeys.BCM_FSConnectionMonitor.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.FSAdmin,
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_AzFileShareConfigureConnection,
                    Value = ResourceKeys.BCM_AzFileShareConfigureConnection.ToUrl(RouterUrl_Root),
                    PermissionExtension = RMPermissionExtensionMasks.AzureFSAdmin,
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_BoxConfigureConnection,
                    Value = ResourceKeys.BCM_BoxConfigureConnection.ToUrl(RouterUrl_Root),
                    PermissionExtension = RMPermissionExtensionMasks.BoxAdmin,    //need change
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.BCM_GoogleConfigureConnection,
                    Value = ResourceKeys.BCM_GoogleConfigureConnection.ToUrl(RouterUrl_Root),
                    PermissionExtension = RMPermissionExtensionMasks.GoogleAdmin,
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.Explorer_SPFilter,
                    Value = ResourceKeys.Explorer_SPFilter.ToString(),
                    Permission = RMPermissionMasks.SPOEnduser,
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.Explorer_OneDriveFilter,
                    Value = ResourceKeys.Explorer_OneDriveFilter.ToString(),
                    Permission = RMPermissionMasks.OneDriveEnduser,
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.Explorer_FSFilter,
                    Value = ResourceKeys.Explorer_FSFilter.ToString(),
                    Permission = RMPermissionMasks.FSAdmin,
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.Explorer_TeamsFilter,
                    Value = ResourceKeys.Explorer_TeamsFilter.ToString(),
                    PermissionExtension = RMPermissionExtensionMasks.TeamsEndUser,
                },
              new ResourceItem()
                {
                    Key = ResourceKeys.BCM_ManageHold,
                    Value = ResourceKeys.BCM_ManageHold.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ControlPanelAdmin,
                    PermissionExtension = RMPermissionExtensionMasks.ManageHoldEndUser,
                },
               new ResourceItem()
                {
                    Key = ResourceKeys.BCM_ManageHold,
                    Value = ResourceKeys.BCM_ManageHold.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ManageHold,
                },
            new ResourceItem()
                {
                    Key = ResourceKeys.RECO_ContentSource_Tab,
                    Value = ResourceKeys.RECO_ContentSource_Tab.ToString(),
                    Permission = RMPermissionMasks.TermManagementEnduser,
                },
    };
        public override List<ResourceItem> Get()
        {
            return Resource;
        }
    }
}