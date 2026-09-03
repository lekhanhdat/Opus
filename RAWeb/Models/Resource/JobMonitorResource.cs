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
    public class JobMonitorResource : BaseResource
    {
        public override List<ResourceItem> Get()
        {
            return new List<ResourceItem>()
            {
                 new ResourceItem()
                {
                    Key = ResourceKeys.JM,
                    Value = ResourceKeys.JM.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.JobMonitorEnduser,
                    SOPermission = RMSOPermissionMasks.JobMonitorEnduser,
                    DiscoveryPermission = RMDiscoveryPermissionMasks.AccessAll,
                    SalesforceDiscoveryPermission = RMDiscoverySalesforcePermissionMask.AccessAll,
                    GoogleROTDiscoveryPermission = RMDiscoveryGoogleROTPermissionMask.AccessAll,
                    FSDiscoveryPermission = RMDiscoveryFileSystemPermissionMask.AccessAll,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.JM_Index,
                    Value = ResourceKeys.JM_Index.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.JobMonitorEnduser,
                    SOPermission = RMSOPermissionMasks.JobMonitorEnduser,
                    DiscoveryPermission = RMDiscoveryPermissionMasks.AccessAll,
                    SalesforceDiscoveryPermission = RMDiscoverySalesforcePermissionMask.AccessAll,
                    GoogleROTDiscoveryPermission = RMDiscoveryGoogleROTPermissionMask.AccessAll,
                    FSDiscoveryPermission = RMDiscoveryFileSystemPermissionMask.AccessAll,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.JM_Detail,
                    Value = ResourceKeys.JM_Detail.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.JobMonitorEnduser,
                    SOPermission = RMSOPermissionMasks.JobMonitorEnduser,
                    DiscoveryPermission = RMDiscoveryPermissionMasks.AccessAll,
                    SalesforceDiscoveryPermission = RMDiscoverySalesforcePermissionMask.AccessAll,
                    GoogleROTDiscoveryPermission = RMDiscoveryGoogleROTPermissionMask.AccessAll,
                    FSDiscoveryPermission = RMDiscoveryFileSystemPermissionMask.AccessAll,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.JM_PlanDetails,
                    Value = ResourceKeys.JM_PlanDetails.ToUrl(RouterUrl_Root),
                    Permission = RMPermissionMasks.JobMonitorEnduser,
                    SOPermission = RMSOPermissionMasks.JobMonitorEnduser,
                    DiscoveryPermission = RMDiscoveryPermissionMasks.AccessAll,
                    SalesforceDiscoveryPermission = RMDiscoverySalesforcePermissionMask.AccessAll,
                    GoogleROTDiscoveryPermission = RMDiscoveryGoogleROTPermissionMask.AccessAll,
                    FSDiscoveryPermission = RMDiscoveryFileSystemPermissionMask.AccessAll,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.JM_DownloadSettings,
                    Value = ResourceKeys.JM_DownloadSettings.ToString(),
                    Permission = RMPermissionMasks.JobMonitorAdmin,
                    SOPermission = RMSOPermissionMasks.JobMonitorAdmin,
                    DiscoveryPermission = RMDiscoveryPermissionMasks.AccessAll,
                    SalesforceDiscoveryPermission = RMDiscoverySalesforcePermissionMask.AccessAll,
                    GoogleROTDiscoveryPermission = RMDiscoveryGoogleROTPermissionMask.AccessAll,
                    FSDiscoveryPermission = RMDiscoveryFileSystemPermissionMask.AccessAll,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.JM_JobQueue,
                    Value = ResourceKeys.JM_JobQueue.ToString(),
                    Permission = RMPermissionMasks.JobMonitorAdmin,
                    SOPermission = RMSOPermissionMasks.JobMonitorAdmin,
                    DiscoveryPermission = RMDiscoveryPermissionMasks.AccessAll,
                    SalesforceDiscoveryPermission = RMDiscoverySalesforcePermissionMask.AccessAll,
                    GoogleROTDiscoveryPermission = RMDiscoveryGoogleROTPermissionMask.AccessAll,
                    FSDiscoveryPermission = RMDiscoveryFileSystemPermissionMask.AccessAll,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.JM,
                    Value = ResourceKeys.JM.ToUrl(RouterUrl_Root),
                    SOPermission = RMSOPermissionMasks.RestoreCenterSearch,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.JM_Index,
                    Value = ResourceKeys.JM_Index.ToUrl(RouterUrl_Root),
                    SOPermission = RMSOPermissionMasks.RestoreCenterSearch,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.JM_Detail,
                    Value = ResourceKeys.JM_Detail.ToUrl(RouterUrl_Root),
                    SOPermission = RMSOPermissionMasks.RestoreCenterSearch,
                },
                new ResourceItem()
                {
                    Key = ResourceKeys.JM_PlanDetails,
                    Value = ResourceKeys.JM_PlanDetails.ToUrl(RouterUrl_Root),
                    SOPermission = RMSOPermissionMasks.RestoreCenterSearch,
                }
            };
        }
    }
}