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
using AvePoint.RA.Web.Common.WIF;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc;
using AvePoint.RA.Contract.Discovery.Model;
using Newtonsoft.Json;

namespace AvePoint.RA.Web.Controllers.Discovery.Office365
{
    [RMApiAuthorize(RMDiscoveryPermissionMasks.AccessAll, preferred: false)]
    public class RMDiscoveryOffice365ProfileApiController : BaseApiController
    {
        private readonly IRMDiscoveryOffice365ProfileService _profileService = PlatformWindsorManager.GetService<IRMDiscoveryOffice365ProfileService>();

        [HttpGet]
        public Task<List<RMDiscoveryProfileDataInfo>> GetInactiveProfileInfoes(Guid o365TenantId)
        {
            return _profileService.GetInactiveProfileInfoesAsync(o365TenantId);
        }

        [HttpPost]
        public Task<RAReturnMessage> AddInactiveProfileInfo([FromBody] RMDiscoveryProfileDataInfo dataInfo)
        {
            return _profileService.AddInactiveProfileInfoAsync(dataInfo);
        }

        [HttpPost]
        public Task<RAReturnMessage> UpdateInactiveProfileInfo([FromBody] RMDiscoveryProfileDataInfo dataInfo)
        {
            return _profileService.UpdateInactiveProfileInfoAsync(dataInfo);
        }

        [HttpPost]
        public Task<RAReturnMessage> DeleteInactiveProfileInfo([FromBody] RMDiscoveryProfileDataInfo dataInfo)
        {
            return _profileService.DeleteInactiveProfileInfoAsync(dataInfo);
        }

        [HttpGet]
        public Task<List<RMDiscoveryProfileDataInfo>> GetRotProfileInfoes(Guid o365TenantId)
        {
            return _profileService.GetRotProfileInfoesAsync(o365TenantId);
        }

        [HttpPost]
        public Task<RAReturnMessage> AddRotProfileInfo([FromBody] RMDiscoveryProfileDataInfo dataInfo)
        {
            return _profileService.AddRotProfileInfoAsync(dataInfo);
        }

        [HttpPost]
        public Task<RAReturnMessage> UpdateRotProfileInfo([FromBody] RMDiscoveryProfileDataInfo dataInfo)
        {
            return _profileService.UpdateRotProfileInfoAsync(dataInfo);
        }

        [HttpPost]
        public Task<RAReturnMessage> DeleteRotProfileInfo([FromBody] RMDiscoveryProfileDataInfo dataInfo)
        {
            return _profileService.DeleteRotProfileInfoAsync(dataInfo);
        }

        [HttpPost]
        public string RunExportDataAnalysisProfileJob([FromBody] DiscoveryO365DataAnalysis o365DataAnalysis)
        {
            return JsonConvert.SerializeObject(_profileService.RunExportProfileDiscoveryDataAnalysisForOffice365Job(o365DataAnalysis));
        }
    }
}
