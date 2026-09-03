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
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Google;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter;
using AvePoint.RA.Service.Services.Discovery.Google;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers.GoogleOne.Discovery.Google
{
    [Route("api/googleone/discovery/google/dataquery")]
    public class GoogleOneDiscoveryGoogleDataQueryApiController : GoogleOneApiBaseController
    {
        private readonly IRMDiscoveryGoogleDataQueryService _dataQueryService = new RMDiscoveryGoogleDataQueryService();

        [HttpPost("inactive/fileextensions")]
        public async Task<string> QueryInactiveFileExtensions([FromBody] RMDiscoveryGoogleQueryParameter queryParameter)
        {
            var fileExtensions = await _dataQueryService.QueryInactiveFileExtensionsAsync(queryParameter);
            return JsonConvert.SerializeObject(fileExtensions);
        }

        [HttpPost("inactive/sizeranges")]
        public async Task<string> QueryInactiveSizeRanges([FromBody] RMDiscoveryGoogleQueryParameter queryParameter)
        {
            var sizeRanges = await _dataQueryService.QueryInactiveSizeRangesAsync(queryParameter);
            return JsonConvert.SerializeObject(sizeRanges);

        }

        [HttpPost("inactive/summary/nodes")]
        public async Task<RMDiscoveryNodeDataInfo> QueryInactiveSummaryNodes([FromBody] RMDiscoveryGoogleQueryParameter queryParameter)
        {
            var result = await _dataQueryService.QueryInactiveSummaryNodesAsync(queryParameter);
            return result.ApplyI18NForDefaultNodeName();
        }

        [HttpPost("inactive/summary/node/total/aggregateinfo")]
        public async Task<Dictionary<string, object>> QueryInactiveSummaryNodeTotalAggregateInfo([FromBody] RMDiscoveryGoogleQueryParameter queryParameter)
        {
            return await _dataQueryService.QueryInactiveSummaryNodeTotalAggregateInfoAsync(queryParameter);
        }

        [HttpPost("inactive/aggregateinfo")]
        public async Task<RMDiscoveryGoogleAggregateStatisticDataInfo> QueryInactiveAggregateInfo([FromBody] RMDiscoveryGoogleQueryParameter queryParameter)
        {
            return await _dataQueryService.QueryInactiveAggregateInfoAsync(queryParameter);
        }

        [HttpPost("rot/aggregateinfo")]
        public async Task<RMDiscoveryGoogleAggregateStatisticDataInfo> QueryRotAggregateInfo([FromBody] RMDiscoveryGoogleQueryParameter queryParameter)
        {
            return await _dataQueryService.QueryRotAggregateInfoAsync(queryParameter);
        }

        [HttpPost("rot/summary/node/total/aggregateinfo")]
        public async Task<Dictionary<string, object>> QueryRotSummaryNodeTotalAggregateInfoData([FromBody] RMDiscoveryGoogleQueryParameter queryParameter)
        {
            return await _dataQueryService.QueryRotSummaryNodeTotalAggregateInfoDataAsync(queryParameter);
        }

        [HttpPost("rot/fileextensions")]
        public async Task<string> QueryRotFileExtensionData([FromBody] RMDiscoveryGoogleQueryParameter queryParameter)
        {
            var response = await _dataQueryService.QueryRotFileExtensionDataAsync(queryParameter);
            return  JsonConvert.SerializeObject(response);
        }

        [HttpPost("rot/rule/infooftree")]
        public async Task<RMDiscoveryRotRuleDataInfo> QueryRotRuleInfoOfTree([FromBody] RMDiscoveryGoogleQueryParameter queryParameter)
        {
            return await _dataQueryService.QueryRotRuleInfoOfTreeAsync(queryParameter);
        }

        [HttpPost("rot/summary/nodes")]
        public async Task<RMDiscoveryNodeDataInfo> QueryRotSummaryNodeData([FromBody] RMDiscoveryGoogleQueryParameter queryParameter)
        {
            var result = await _dataQueryService.QueryRotSummaryNodeDataAsync(queryParameter);
            return result.ApplyI18NForDefaultNodeName();
        }
    }
}
