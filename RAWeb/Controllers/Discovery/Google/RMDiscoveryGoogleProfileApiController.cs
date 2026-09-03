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
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model.Profile;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters.GoogleDriveFilter;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.Discovery.Google
{
    [RMApiAuthorize(RMDiscoveryGoogleROTPermissionMask.AccessAll, preferred: false)]
    public class RMDiscoveryGoogleProfileApiController : BaseApiController
    {
        private readonly IRMDiscoveryGoogleProfileService _profileService = PlatformWindsorManager.GetService<IRMDiscoveryGoogleProfileService>();

        [HttpGet]
        public Task<List<RMDiscoveryGoogleProfileDataInfo>> GetInactiveProfileInfoList(string organizationId)
        {
            return _profileService.GetInactiveProfileInfoListAsync(organizationId);
        }

        [HttpPost]
        public Task<RAReturnMessage> AddInactiveProfileInfo([FromBody] RMDiscoveryGoogleProfileDataInfo dataInfo)
        {
            return _profileService.AddInactiveProfileInfoAsync(dataInfo);
        }

        [HttpPost]
        public Task<RAReturnMessage> UpdateInactiveProfileInfo([FromBody] RMDiscoveryGoogleProfileDataInfo dataInfo)
        {
            return _profileService.UpdateInactiveProfileInfoAsync(dataInfo);
        }

        [HttpPost]
        public Task<RAReturnMessage> DeleteInactiveProfileInfo([FromBody] RMDiscoveryGoogleProfileDataInfo dataInfo)
        {
            return _profileService.DeleteInactiveProfileInfoAsync(dataInfo);
        }

        [HttpGet]
        public Task<List<RMDiscoveryGoogleProfileDataInfo>> GetRotProfileInfoList(string organizationId)
        {
            return _profileService.GetRotProfileInfoListAsync(organizationId);
        }

        [HttpPost]
        public Task<RAReturnMessage> AddRotProfileInfo([FromBody] RMDiscoveryGoogleProfileDataInfo dataInfo)
        {
            return _profileService.AddRotProfileInfoAsync(dataInfo);
        }

        [HttpPost]
        public Task<RAReturnMessage> UpdateRotProfileInfo([FromBody] RMDiscoveryGoogleProfileDataInfo dataInfo)
        {
            return _profileService.UpdateRotProfileInfoAsync(dataInfo);
        }

        [HttpPost]
        public Task<RAReturnMessage> DeleteRotProfileInfo([FromBody] RMDiscoveryGoogleProfileDataInfo dataInfo)
        {
            return _profileService.DeleteRotProfileInfoAsync(dataInfo);
        }
    }
}
