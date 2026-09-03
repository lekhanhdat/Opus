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
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter.Profile;
using AvePoint.RA.Contract.Object;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers.GoogleOne.Discovery.Google
{
    [Route("api/googleone/discovery/google/profile")]
    public class GoogleOneDiscoveryGoogleProfileApiController : GoogleOneApiBaseController
    {
        private readonly IRMDiscoveryGoogleProfileService _profileService = PlatformWindsorManager.GetService<IRMDiscoveryGoogleProfileService>();
        private readonly IRMDiscoveryGoogleProfileDataQueryService _discoveryDataQueryService = PlatformWindsorManager.GetService<IRMDiscoveryGoogleProfileDataQueryService>();

        #region Profile Info

        [HttpGet("inactive/list")]
        public Task<List<RMDiscoveryGoogleProfileDataInfo>> GetInactiveProfileInfoList([FromQuery] string organizationId)
        {
            return _profileService.GetInactiveProfileInfoListAsync(organizationId);
        }

        [HttpPost("inactive/add")]
        public Task<RAReturnMessage> AddInactiveProfileInfo([FromBody] RMDiscoveryGoogleProfileDataInfo dataInfo)
        {
            return _profileService.AddInactiveProfileInfoAsync(dataInfo);
        }

        [HttpPost("inactive/update")]
        public Task<RAReturnMessage> UpdateInactiveProfileInfo([FromBody] RMDiscoveryGoogleProfileDataInfo dataInfo)
        {
            return _profileService.UpdateInactiveProfileInfoAsync(dataInfo);
        }

        [HttpPost("inactive/delete")]
        public Task<RAReturnMessage> DeleteInactiveProfileInfo([FromBody] RMDiscoveryGoogleProfileDataInfo dataInfo)
        {
            return _profileService.DeleteInactiveProfileInfoAsync(dataInfo);
        }

        [HttpGet("rot/list")]
        public Task<List<RMDiscoveryGoogleProfileDataInfo>> GetRotProfileInfoList([FromQuery] string organizationId)
        {
            return _profileService.GetRotProfileInfoListAsync(organizationId);
        }

        [HttpPost("rot/add")]
        public Task<RAReturnMessage> AddRotProfileInfo([FromBody] RMDiscoveryGoogleProfileDataInfo dataInfo)
        {
            return _profileService.AddRotProfileInfoAsync(dataInfo);
        }

        [HttpPost("rot/update")]
        public Task<RAReturnMessage> UpdateRotProfileInfo([FromBody] RMDiscoveryGoogleProfileDataInfo dataInfo)
        {
            return _profileService.UpdateRotProfileInfoAsync(dataInfo);
        }

        [HttpPost("rot/delete")]
        public Task<RAReturnMessage> DeleteRotProfileInfo([FromBody] RMDiscoveryGoogleProfileDataInfo dataInfo)
        {
            return _profileService.DeleteRotProfileInfoAsync(dataInfo);
        }

        #endregion

        #region Data Query

        [HttpPost("inactive/optimizationnodes")]
        public async Task<RMDiscoveryNodeDataInfo> QueryInactiveOptimizationNodesData([FromBody] RMDiscoveryGoogleProfileQueryParameter queryParameter)
        {
            var result = await _discoveryDataQueryService.QueryInactiveOptimizationNodesAsync(queryParameter);
            return result.ApplyI18NForDefaultNodeName();
        }

        [HttpPost("inactive/optimizationnodes/totalaggregateinfo")]
        public Task<Dictionary<string, object>> QueryInactiveOptimizationNodeTotalAggregateInfo([FromBody] RMDiscoveryGoogleProfileQueryParameter queryParameter)
        {
            return _discoveryDataQueryService.QueryInactiveOptimizationNodeTotalAggregateInfoAsync(queryParameter);
        }

        [HttpPost("inactive/aggregate")]
        public Task<Dictionary<string, object>> QueryInactiveAggregateInfo([FromBody] RMDiscoveryGoogleProfileQueryParameter queryParameter)
        {
            return _discoveryDataQueryService.QueryInactiveAggregateInfo(queryParameter);
        }

        [HttpPost("rot/optimizationnodes")]
        public async Task<RMDiscoveryNodeDataInfo> QueryRotOptimizationNodesData([FromBody] RMDiscoveryGoogleProfileQueryParameter queryParameter)
        {
            var result = await _discoveryDataQueryService.QueryRotOptimizationNodesAsync(queryParameter);
            return result.ApplyI18NForDefaultNodeName();
        }

        [HttpPost("rot/optimizationnodes/totalaggregateinfo")]
        public Task<Dictionary<string, object>> QueryRotOptimizationNodeTotalAggregateInfo([FromBody] RMDiscoveryGoogleProfileQueryParameter queryParameter)
        {
            return _discoveryDataQueryService.QueryRotOptimizationNodeTotalAggregateInfoAsync(queryParameter);
        }

        #endregion
    }
}
