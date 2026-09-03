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
using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model.Query.AOSP.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Query.AOSP;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Service.Services.Discovery.AOSP;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers.AOSPDisocvery
{

    [Route("api/discoveryDataQuery/[action]")]
    [ApiController]
    //[APIScopeFilter(ContractConstants.RecordsPublicScope)]
    public class DiscoveryAOSPDataQueryApiController : RAWebApiBase
    {
        private static readonly IRMDiscoveryAOSPDataQueryService s_dataQueryService = new RMDiscoveryAOSPDataQueryService();

        [HttpPost]
        public Task<List<RMDiscoveryFileExtensionDataInfo>> QueryInactiveFileExtensions([FromBody] RMDiscoveryAOSPQueryParameter queryParameter)
        {
            return s_dataQueryService.QueryInactiveFileExtensionsAsync(queryParameter);
        }

        [HttpPost]
        public Task<List<RMDiscoverySizeRangeDataInfo>> QueryInactiveSizeRanges([FromBody] RMDiscoveryAOSPQueryParameter queryParameter)
        {
            return s_dataQueryService.QueryInactiveSizeRangesAsync(queryParameter);
        }

        [HttpPost]
        public Task<RMDiscoveryAOSPAggregateStatisticDataInfo> QueryInactiveAggregateInfo([FromBody] RMDiscoveryAOSPQueryParameter queryParameter)
        {
            return s_dataQueryService.QueryInactiveAggregateInfo(queryParameter);
        }

        [HttpPost]
        public Task<RMDiscoveryNodeDataInfo> QueryInactiveAndRotSiteNodes([FromBody] RMDiscoveryAOSPQueryParameter queryParameter)
        {
            return s_dataQueryService.QueryInactiveAndRotSiteNodesAsync(queryParameter);
        }
        [HttpPost]
        public Task<List<RMDiscoveryNodeDataSizeInfo>> QuerySiteArchivedSizeInfo(RMDiscoveryAOSPQueryParameter queryParameter)
        {
            return s_dataQueryService.QuerySiteArchiveSizeInfo(queryParameter);
        }

        [HttpPost]
        public Task<List<RMDiscoveryFileExtensionDataInfo>> QueryRotFileExtensionData([FromBody] RMDiscoveryAOSPQueryParameter queryParameter)
        {
            return s_dataQueryService.QueryRotFileExtensionDataAsync(queryParameter);
        }

        [HttpPost]
        public Task<RMDiscoveryRotRuleDataInfo> QueryRotRuleInfoOfTree([FromBody] RMDiscoveryAOSPQueryParameter queryParameter)
        {
            return s_dataQueryService.QueryRotRuleInfoOfTreeAsync(queryParameter);
        }

        [HttpPost]
        public Task<RMDiscoveryAOSPAggregateStatisticDataInfo> QueryRotAggregateInfo([FromBody] RMDiscoveryAOSPQueryParameter queryParameter)
        {
            return s_dataQueryService.QueryRotAggregateInfoAsync(queryParameter);
        }
    }
}
