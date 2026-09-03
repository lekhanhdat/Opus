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
using AvePoint.RA.Contract.Discovery.Model.Query.FileSystem;
using AvePoint.RA.Contract.Discovery.Model.Query.FileSystem.Parameter;
using AvePoint.RA.Service.Services.Discovery.FileSystem;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters.FileSystem;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.Discovery.FileSystem
{
    //[RMApiAuthorize(RMDiscoveryFileSystemROTPermissionMask.AccessAll, preferred: false)]
    [ValidFileSystemDiscoveryPermissionFilter]
    public class RMDiscoveryFSDataQueryApiController : BaseApiController
    {
        private readonly IRMDiscoveryFSDataQueryService _dataQueryService = new RMDiscoveryFSDataQueryService();

        [HttpPost]
        public Task<List<RMDiscoveryFileExtensionDataInfo>> QueryInactiveFileExtensions([FromBody] RMDiscoveryFSQueryParameter queryParameter)
        {
            return _dataQueryService.QueryInactiveFileExtensionsAsync(queryParameter);
        }

        [HttpPost]
        public Task<List<RMDiscoverySizeRangeDataInfo>> QueryInactiveSizeRanges([FromBody] RMDiscoveryFSQueryParameter queryParameter)
        {
            return _dataQueryService.QueryInactiveSizeRangesAsync(queryParameter);
        }

        [HttpPost]
        public Task<RMDiscoveryNodeDataInfo> QueryInactiveSummaryNodes([FromBody] RMDiscoveryFSQueryParameter queryParameter)
        {
            return _dataQueryService.QueryInactiveSummaryNodesAsync(queryParameter);
        }

        [HttpPost]
        public Task<Dictionary<string, object>> QueryInactiveSummaryNodeTotalAggregateInfo([FromBody] RMDiscoveryFSQueryParameter queryParameter)
        {
            return _dataQueryService.QueryInactiveSummaryNodeTotalAggregateInfoAsync(queryParameter);
        }

        [HttpPost]
        public Task<RMDiscoveryFSAggregateStatisticDataInfo> QueryInactiveAggregateInfo([FromBody] RMDiscoveryFSQueryParameter queryParameter)
        {
            return _dataQueryService.QueryInactiveAggregateInfoAsync(queryParameter);
        }

        [HttpPost]
        public Task<RMDiscoveryNodeDataInfo> QueryRotSummaryNodeData([FromBody] RMDiscoveryFSQueryParameter queryParameter)
        {
            return _dataQueryService.QueryRotSummaryNodeDataAsync(queryParameter);
        }

        [HttpPost]
        public Task<List<RMDiscoveryFileExtensionDataInfo>> QueryRotFileExtensionData([FromBody] RMDiscoveryFSQueryParameter queryParameter)
        {
            return _dataQueryService.QueryRotFileExtensionDataAsync(queryParameter);
        }

        [HttpPost]
        public Task<Dictionary<string, object>> QueryRotSummaryNodeTotalAggregateInfoData([FromBody] RMDiscoveryFSQueryParameter queryParameter)
        {
            return _dataQueryService.QueryRotSummaryNodeTotalAggregateInfoDataAsync(queryParameter);
        }

        [HttpPost]
        public Task<RMDiscoveryRotRuleDataInfo> QueryRotRuleInfoOfTree([FromBody] RMDiscoveryFSQueryParameter queryParameter)
        {
            return _dataQueryService.QueryRotRuleInfoOfTreeAsync(queryParameter);
        }

        [HttpPost]
        public Task<RMDiscoveryFSAggregateStatisticDataInfo> QueryRotAggregateInfo([FromBody] RMDiscoveryFSQueryParameter queryParameter)
        {
            return _dataQueryService.QueryRotAggregateInfoAsync(queryParameter);
        }
    }
}
