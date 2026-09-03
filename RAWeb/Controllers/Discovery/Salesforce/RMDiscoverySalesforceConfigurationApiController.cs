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
using AvePoint.RA.Contract.Discovery.Model.Configuration.Salesforce;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Salesforce.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;

namespace AvePoint.RA.Web.Controllers.Discovery.Salesforce
{
    [RMApiAuthorize(RMDiscoverySalesforcePermissionMask.AccessAll, preferred: false)]
    public class RMDiscoverySalesforceConfigurationApiController : BaseApiController
    {
        
        private readonly IRMDiscoverySalesforceConfigurationService _configurationService = PlatformWindsorManager.GetService<IRMDiscoverySalesforceConfigurationService>();
        
        private readonly IRMDiscoverySalesforceDataQueryService _dataQueryService = PlatformWindsorManager.GetService<IRMDiscoverySalesforceDataQueryService>();


        [HttpGet]
        public Task<RMDiscoverySalesforceConfigurationInfo> GetConfigurationInfo()
        {
            return _configurationService.GetConfigurationInfoAsync();
        }

        [HttpPost]
        public Task<RAReturnMessage> AddOrUpdateNewlyConfigurationInfo([FromBody] RMDiscoverySalesforceConfigurationInfo discoveryConfigurationInfo)
        {
            return _configurationService.AddOrUpdateConfigurationInfoAsync(discoveryConfigurationInfo);
        }

        [HttpPost]
        public async Task<IActionResult> DownloadDiscoveryJobReport()
        {
            var filePath = await _configurationService.DownloadDiscoveryJobReporAsync();
            var stream = System.IO.File.Open(filePath, FileMode.Open);
            return new FileStreamResult(stream, "application/octet-stream")
            {
                FileDownloadName = $"Discovery Salesforce Job Report.csv"
            };        
        }

        [HttpGet]
        public Task<List<RMDiscoverySalesforceOrgnization>> GetOrganizations()
        {
            return _dataQueryService.GetAllOrganizations();
        }
    }
}
