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
using AvePoint.RA.Contract.Discovery.Model.Query.Office365;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Query.Progress;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Service.Services.Discovery;
using AvePoint.RA.Service.Services.Discovery.Office365;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.Discovery.Office365
{
    [RMApiAuthorize(RMDiscoveryPermissionMasks.AccessAll, preferred: false)]
    public class RMDiscoveryOffice365DataQueryApiController : BaseApiController
    {
        private static readonly IRMDiscoveryOffice365DataQueryService s_discoveryDataQueryService = new RMDiscoveryOffice365DataQueryService();

        #region Inactive
        [HttpPost]
        public Task<List<RMDiscoveryFileExtensionDataInfo>> QueryInactiveFileExtensions([FromBody] RMDiscoveryOffice365QueryParameter queryParameter)
        {
            return s_discoveryDataQueryService.QueryInactiveFileExtensionsAsync(queryParameter);
        }

        [HttpPost]
        public Task<List<RMDiscoverySizeRangeDataInfo>> QueryInactiveSizeRanges([FromBody] RMDiscoveryOffice365QueryParameter queryParameter)
        {
            return s_discoveryDataQueryService.QueryInactiveSizeRangesAsync(queryParameter);
        }

        [HttpPost]
        public Task<RMDiscoveryOffice365AggregateStatisticDataInfo> QueryInactiveAggregateInfo([FromBody] RMDiscoveryOffice365QueryParameter queryParameter)
        {
            return s_discoveryDataQueryService.QueryInactiveAggregateInfo(queryParameter);
        }

        [HttpPost]
        public Task<RMDiscoveryNodeDataInfo> QueryInactiveSummaryNodesData([FromBody] RMDiscoveryOffice365QueryParameter queryParameter)
        {
            return s_discoveryDataQueryService.QueryInactiveSummaryNodesAsync(queryParameter);
        }

        [HttpPost]
        public Task<Dictionary<string, object>> QueryInactiveSummaryNodeTotalAggregateInfo([FromBody] RMDiscoveryOffice365QueryParameter queryParameter)
        {
            return s_discoveryDataQueryService.QueryInactiveSummaryNodeTotalAggregateInfoAsync(queryParameter);
        }

        [HttpPost]
        public Task<Dictionary<string, object>> QueryInactiveOptimizationNodeTotalAggregateInfo([FromBody] RMDiscoveryOffice365QueryParameter queryParameter)
        {
            return s_discoveryDataQueryService.QueryInactiveOptimizationNodeTotalAggregateInfoAsync(queryParameter);
        }

        [HttpPost]
        public Task<RMDiscoveryNodeDataInfo> QueryInactiveOptimizationNodesData([FromBody] RMDiscoveryOffice365QueryParameter queryParameter)
        {
            return s_discoveryDataQueryService.QueryInactiveOptimizationNodesAsync(queryParameter);
        }
        #endregion

        #region ROT

        [HttpPost]
        public Task<List<RMDiscoveryFileExtensionDataInfo>> QueryROTFileExtensions([FromBody] RMDiscoveryOffice365QueryParameter queryParameter)
        {
            return s_discoveryDataQueryService.QueryRotFileExtensionsAsync(queryParameter);
        }

        [HttpPost]
        public Task<RMDiscoveryOffice365AggregateStatisticDataInfo> QueryROTTotalData([FromBody] RMDiscoveryOffice365QueryParameter queryParameter)
        {
            return s_discoveryDataQueryService.QueryRotAggregateInfoAsync(queryParameter);
        }

        [HttpPost]
        public Task<RMDiscoveryNodeDataInfo> QueryRotSummaryNodes([FromBody] RMDiscoveryOffice365QueryParameter queryParameter)
        {
            return s_discoveryDataQueryService.QueryRotSummaryNodesAsync(queryParameter);
        }

        [HttpPost]
        public Task<RMDiscoveryNodeDataInfo> QueryRotOptmizationNodes([FromBody] RMDiscoveryOffice365QueryParameter queryParameter)
        {
            return s_discoveryDataQueryService.QueryRotOptmizationNodesAsync(queryParameter);
        }

        [HttpPost]
        public Task<Dictionary<string, object>> QueryRotSummaryNodeTotalAggregateInfo([FromBody] RMDiscoveryOffice365QueryParameter queryParameter)
        {
            return s_discoveryDataQueryService.QueryRotSummaryNodeTotalAggregateInfoAsync(queryParameter);
        }

        [HttpPost]
        public Task<Dictionary<string, object>> QueryRotOptimizationNodeTotalAggregateInfo([FromBody] RMDiscoveryOffice365QueryParameter queryParameter)
        {
            return s_discoveryDataQueryService.QueryRotOptimizationNodeTotalAggregateInfoAsync(queryParameter);
        }


        [HttpPost]
        public Task<RMDiscoveryRotRuleDataInfo> QueryTreeRotRuleInfo([FromBody] RMDiscoveryOffice365QueryParameter queryParameter)
        {
            return s_discoveryDataQueryService.QueryTreeRotRuleInfoAsync(queryParameter);
        }

        #endregion

        [HttpPost]
        public Task<RMDiscoveryNodeDataInfo> QueryRotV3SummaryNodeData([FromBody] RMDiscoveryOffice365QueryParameter queryParameter)
        {
            return s_discoveryDataQueryService.QueryRotV3SummaryNodeDataAsync(queryParameter);
        }

        [HttpPost]
        public Task<List<RMDiscoveryFileExtensionDataInfo>> QueryRotV3FileExtensionData([FromBody] RMDiscoveryOffice365QueryParameter queryParameter)
        {
            return s_discoveryDataQueryService.QueryRotV3FileExtensionDataAsync(queryParameter);
        }

        [HttpPost]
        public Task<Dictionary<string, object>> QueryRotV3SummaryNodeTotalAggregateInfoData([FromBody] RMDiscoveryOffice365QueryParameter queryParameter)
        {
            return s_discoveryDataQueryService.QueryRotV3SummaryNodeTotalAggregateInfoDataAsync(queryParameter);
        }

        [HttpPost]
        public Task<RMDiscoveryRotRuleDataInfo> QueryRotV3RuleInfoOfTree([FromBody] RMDiscoveryOffice365QueryParameter queryParameter)
        {
            return s_discoveryDataQueryService.QueryRotV3RuleInfoOfTreeAsync(queryParameter);
        }

        [HttpPost]
        public Task<RMDiscoveryOffice365AggregateStatisticDataInfo> QueryRotV3AggregateInfo([FromBody] RMDiscoveryOffice365QueryParameter queryParameter)
        {
            return s_discoveryDataQueryService.QueryRotV3AggregateInfoAsync(queryParameter);
        }
    }
}
