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
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Web.Common.WIF;
using AvePoint.RA.Web.Common;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AvePoint.RA.Service.Services.Discovery.FileSystem;
using AvePoint.RA.Web.Common.Filters.FileSystem;

namespace AvePoint.RA.Web.Controllers.Discovery.FS
{
    //[RMApiAuthorize(RMPermissionMasks.FSAdmin, preferred: false)] //Add FS Discovery License
    [ValidFileSystemDiscoveryPermissionFilter]
    public class RMDiscoveryFSJobManagementApiController : BaseApiController
    {
        private readonly IRMDiscoveryFSJobManagementService _jobManagementService = new RMDiscoveryFSJobManagementService();

        [HttpGet]
        public Task<RMDiscoveryLatestJobInfo> GetLatest()
        {
            return _jobManagementService.GetLatestAsync();
        }
    }
}
