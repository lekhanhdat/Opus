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
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Service.JobMonitor;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.Archiver
{
    [RMApiAuthorize(RMPermissionMasks.JobMonitorEnduser, RMSOPermissionMasks.JobMonitorEnduser, RMDiscoveryPermissionMasks.AccessAll, RMDiscoverySalesforcePermissionMask.AccessAll, RMDiscoveryGoogleROTPermissionMask.AccessAll, RMDiscoveryFileSystemPermissionMask.AccessAll ,preferred: false)]
    public class CleanupApiController : BaseApiController
    {
        private IArchiverSiteMasterIndexService _SiteMasterIndexService;
        private IArchiverSiteMasterIndexService SiteMasterIndexService => PlatformWindsorManager.GetService(ref _SiteMasterIndexService);
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public Task<string> QueryPager([FromBody] JMPager pager)
        {
            return SiteMasterIndexService.GetFailedJobsDataAsync(pager);
        }
    }
}
