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
    public class ControlPanelResource : BaseResource
    {
        public override List<ResourceItem> Get()
        {
            return new List<ResourceItem>()
            {
                 new ResourceItem()
                {
                    Key = ResourceKeys.CP,
                    Value = ResourceKeys.CP.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ControlPanelAdmin,
                    SOPermission = RMSOPermissionMasks.ControlPanelAdmin,
                    DiscoveryPermission = RMDiscoveryPermissionMasks.AccessAll,
                    SalesforceDiscoveryPermission = RMDiscoverySalesforcePermissionMask.AccessAll,
                    GoogleROTDiscoveryPermission = RMDiscoveryGoogleROTPermissionMask.AccessAll,
                    FSDiscoveryPermission = RMDiscoveryFileSystemPermissionMask.AccessAll,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.CP_Index,
                    Value = ResourceKeys.CP_Index.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ControlPanelAdmin,
                    SOPermission = RMSOPermissionMasks.ControlPanelAdmin,
                    DiscoveryPermission = RMDiscoveryPermissionMasks.AccessAll,
                    SalesforceDiscoveryPermission = RMDiscoverySalesforcePermissionMask.AccessAll,
                    GoogleROTDiscoveryPermission = RMDiscoveryGoogleROTPermissionMask.AccessAll,
                    FSDiscoveryPermission = RMDiscoveryFileSystemPermissionMask.AccessAll,

                },
                new ResourceItem()
                {
                    Key = ResourceKeys.CP_StorageSettings,
                    Value = ResourceKeys.CP_StorageSettings.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ControlPanelAdmin,
                    SOPermission = RMSOPermissionMasks.ControlPanelAdmin,
                    PermissionExtension = RMPermissionExtensionMasks.GoogleAdmin
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.CP_Multi_GEOSettings,
                    Value = ResourceKeys.CP_Multi_GEOSettings.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ControlPanelAdmin,
                    SOPermission = RMSOPermissionMasks.ControlPanelAdmin,
                    PermissionExtension = RMPermissionExtensionMasks.GoogleAdmin
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.CP_StubSettings,
                    Value = ResourceKeys.CP_StubSettings.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ControlPanelAdmin,
                    SOPermission = RMSOPermissionMasks.ControlPanelAdmin
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.CP_AccountManagement,
                    Value = ResourceKeys.CP_AccountManagement.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ControlPanelAdmin,
                    SOPermission = RMSOPermissionMasks.ControlPanelAdmin,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.CP_AgentManagement,
                    Value = ResourceKeys.CP_AgentManagement.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.FSAdmin | RMPermissionMasks.ControlPanelAdmin,
                    FSDiscoveryPermission = RMDiscoveryFileSystemPermissionMask.AccessAll,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.CP_AgentManagement,
                    Value = ResourceKeys.CP_AgentManagement.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.SPOnPremAdmin | RMPermissionMasks.ControlPanelAdmin,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.CP_JobNotificationSettings,
                    Value = ResourceKeys.CP_JobNotificationSettings.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ControlPanelAdmin,
                    SOPermission = RMSOPermissionMasks.ControlPanelAdmin,
                    DiscoveryPermission = RMDiscoveryPermissionMasks.AccessAll,
                    SalesforceDiscoveryPermission = RMDiscoverySalesforcePermissionMask.AccessAll,
                    GoogleROTDiscoveryPermission = RMDiscoveryGoogleROTPermissionMask.AccessAll,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.CP_GeneralSetting,
                    Value = ResourceKeys.CP_GeneralSetting.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ControlPanelAdmin,
                    SOPermission = RMSOPermissionMasks.ControlPanelAdmin,
                    DiscoveryPermission = RMDiscoveryPermissionMasks.AccessAll,
                    SalesforceDiscoveryPermission = RMDiscoverySalesforcePermissionMask.AccessAll,
                    GoogleROTDiscoveryPermission = RMDiscoveryGoogleROTPermissionMask.AccessAll,
                    FSDiscoveryPermission = RMDiscoveryFileSystemPermissionMask.AccessAll,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.CP_ExportSettings,
                    Value = ResourceKeys.CP_ExportSettings.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ControlPanelAdmin,
                    SOPermission = RMSOPermissionMasks.ControlPanelAdmin,
                    PermissionExtension = RMPermissionExtensionMasks.GoogleAdmin
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.CP_ExportSettings_CompliantExports,
                    Value = ResourceKeys.CP_ExportSettings_CompliantExports.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ControlPanelAdmin,
                    SOPermission = RMSOPermissionMasks.ControlPanelAdmin,
                    PermissionExtension = RMPermissionExtensionMasks.GoogleAdmin
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.CP_EmailTemplate,
                    Value = ResourceKeys.CP_EmailTemplate.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ControlPanelAdmin,
                    SOPermission = RMSOPermissionMasks.ControlPanelAdmin,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.CP_EditEmailTemplate,
                    Value = ResourceKeys.CP_EditEmailTemplate.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ControlPanelAdmin,
                    SOPermission = RMSOPermissionMasks.ControlPanelAdmin,
                },
				 new ResourceItem()
				{
					Key = ResourceKeys.CP_CreateEmailTemplate,
					Value = ResourceKeys.CP_CreateEmailTemplate.ToUrl(RouterUrl_Root),
					Permission = RMPermissionMasks.ControlPanelAdmin,
					SOPermission = RMSOPermissionMasks.ControlPanelAdmin,
                },
				new ResourceItem()
                {
                    Key = ResourceKeys.CP_TimerJobSettings,
                    Value = ResourceKeys.CP_TimerJobSettings.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ControlPanelAdmin,
                    SOPermission = RMSOPermissionMasks.ControlPanelAdmin,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.CP_Schedule_Settings_On_Prem,
                    Value = ResourceKeys.CP_Schedule_Settings_On_Prem.ToString(),
                    Permission = RMPermissionMasks.SPOnPremAdmin,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.CP_CSDApiKeyManagement,
                    Value = ResourceKeys.CP_CSDApiKeyManagement.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.ControlPanelAdmin,
                    IsCSDResource = true
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.Reco_CP_Schedule_Settings,
                    Value = ResourceKeys.Reco_CP_Schedule_Settings.ToString(),
                    Permission = RMPermissionMasks.EletricRecordExplorerAdmin,
                },
            };
        }
    }
}