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
using AvePoint.RA.Contract.Discovery.Model.Query.Google;
using AvePoint.RA.Contract.Discovery.Model.Query;
using System.Collections.Generic;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Service.Services.Discovery.Google;
using Microsoft.AspNetCore.Mvc;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter;
using AvePoint.RA.Web.Common.Filters.GoogleDriveFilter;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Web.Common.WIF;

namespace AvePoint.RA.Web.Controllers.Discovery.Google
{
    [RMApiAuthorize(RMDiscoveryGoogleROTPermissionMask.AccessAll, preferred: false)]
    public class RMDiscoveryGoogleDataQueryApiController : BaseApiController
    {
        private readonly IRMDiscoveryGoogleDataQueryService _dataQueryService = new RMDiscoveryGoogleDataQueryService();

        [HttpPost]
        public Task<List<RMDiscoveryFileExtensionDataInfo>> QueryInactiveFileExtensions([FromBody] RMDiscoveryGoogleQueryParameter queryParameter)
        {
            return _dataQueryService.QueryInactiveFileExtensionsAsync(queryParameter);
        }

        [HttpPost]
        public Task<List<RMDiscoverySizeRangeDataInfo>> QueryInactiveSizeRanges([FromBody] RMDiscoveryGoogleQueryParameter queryParameter)
        {
            return _dataQueryService.QueryInactiveSizeRangesAsync(queryParameter);
        }

        [HttpPost]
        public Task<RMDiscoveryNodeDataInfo> QueryInactiveSummaryNodes([FromBody] RMDiscoveryGoogleQueryParameter queryParameter)
        {
            return _dataQueryService.QueryInactiveSummaryNodesAsync(queryParameter);
        }

        [HttpPost]
        public Task<Dictionary<string, object>> QueryInactiveSummaryNodeTotalAggregateInfo([FromBody] RMDiscoveryGoogleQueryParameter queryParameter)
        {
            return _dataQueryService.QueryInactiveSummaryNodeTotalAggregateInfoAsync(queryParameter);
        }

        [HttpPost]
        public Task<RMDiscoveryGoogleAggregateStatisticDataInfo> QueryInactiveAggregateInfo([FromBody] RMDiscoveryGoogleQueryParameter queryParameter)
        {
            return _dataQueryService.QueryInactiveAggregateInfoAsync(queryParameter);
        }

        [HttpPost]
        public Task<RMDiscoveryNodeDataInfo> QueryRotSummaryNodeData([FromBody] RMDiscoveryGoogleQueryParameter queryParameter)
        {
            return _dataQueryService.QueryRotSummaryNodeDataAsync(queryParameter);
        }

        [HttpPost]
        public Task<List<RMDiscoveryFileExtensionDataInfo>> QueryRotFileExtensionData([FromBody] RMDiscoveryGoogleQueryParameter queryParameter)
        {
            return _dataQueryService.QueryRotFileExtensionDataAsync(queryParameter);
        }

        [HttpPost]
        public Task<Dictionary<string, object>> QueryRotSummaryNodeTotalAggregateInfoData([FromBody] RMDiscoveryGoogleQueryParameter queryParameter)
        {
            return _dataQueryService.QueryRotSummaryNodeTotalAggregateInfoDataAsync(queryParameter);
        }

        [HttpPost]
        public Task<RMDiscoveryRotRuleDataInfo> QueryRotRuleInfoOfTree([FromBody] RMDiscoveryGoogleQueryParameter queryParameter)
        {
            return _dataQueryService.QueryRotRuleInfoOfTreeAsync(queryParameter);
        }

        [HttpPost]
        public Task<RMDiscoveryGoogleAggregateStatisticDataInfo> QueryRotAggregateInfo([FromBody] RMDiscoveryGoogleQueryParameter queryParameter)
        {
            return _dataQueryService.QueryRotAggregateInfoAsync(queryParameter);
        }
    }
}
