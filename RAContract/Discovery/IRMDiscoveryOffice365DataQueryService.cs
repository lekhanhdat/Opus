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
using AvePoint.RA.Contract.Discovery.Model.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Discovery.Model.Query.Progress;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365;

namespace AvePoint.RA.Contract.Discovery
{
    public interface IRMDiscoveryOffice365DataQueryService
    {
        Task<List<RMDiscoveryFileExtensionDataInfo>> QueryInactiveFileExtensionsAsync(RMDiscoveryOffice365QueryParameter queryParameter);

        Task<List<RMDiscoverySizeRangeDataInfo>> QueryInactiveSizeRangesAsync(RMDiscoveryOffice365QueryParameter queryParameter);

        Task<RMDiscoveryNodeDataInfo> QueryInactiveSummaryNodesAsync(RMDiscoveryOffice365QueryParameter queryParameter);

        Task<RMDiscoveryNodeDataInfo> QueryInactiveOptimizationNodesAsync(RMDiscoveryOffice365QueryParameter queryParameter);

        Task<Dictionary<string, object>> QueryInactiveSummaryNodeTotalAggregateInfoAsync(RMDiscoveryOffice365QueryParameter queryParameter);

        Task<Dictionary<string, object>> QueryInactiveOptimizationNodeTotalAggregateInfoAsync(RMDiscoveryOffice365QueryParameter queryParameter);

        Task<RMDiscoveryOffice365AggregateStatisticDataInfo> QueryInactiveAggregateInfo(RMDiscoveryOffice365QueryParameter queryParameter);

        Task<List<RMDiscoveryFileExtensionDataInfo>> QueryRotFileExtensionsAsync(RMDiscoveryOffice365QueryParameter queryParameter);

        Task<RMDiscoveryNodeDataInfo> QueryRotSummaryNodesAsync(RMDiscoveryOffice365QueryParameter queryParameter);

        Task<RMDiscoveryNodeDataInfo> QueryRotOptmizationNodesAsync(RMDiscoveryOffice365QueryParameter queryParameter);

        Task<Dictionary<string, object>> QueryRotSummaryNodeTotalAggregateInfoAsync(RMDiscoveryOffice365QueryParameter queryParameter);

        Task<Dictionary<string, object>> QueryRotOptimizationNodeTotalAggregateInfoAsync(RMDiscoveryOffice365QueryParameter queryParameter);

        Task<RMDiscoveryOffice365AggregateStatisticDataInfo> QueryRotAggregateInfoAsync(RMDiscoveryOffice365QueryParameter queryParameter);

        Task<RMDiscoveryRotRuleDataInfo> QueryTreeRotRuleInfoAsync(RMDiscoveryOffice365QueryParameter queryParameter);

        Task<RMDiscoveryNodeDataInfo> QueryRotV3SummaryNodeDataAsync(RMDiscoveryOffice365QueryParameter queryParameter);

        Task<List<RMDiscoveryFileExtensionDataInfo>> QueryRotV3FileExtensionDataAsync(RMDiscoveryOffice365QueryParameter queryParameter);

        Task<Dictionary<string, object>> QueryRotV3SummaryNodeTotalAggregateInfoDataAsync(RMDiscoveryOffice365QueryParameter queryParameter);

        Task<RMDiscoveryRotRuleDataInfo> QueryRotV3RuleInfoOfTreeAsync(RMDiscoveryOffice365QueryParameter queryParameter);

        Task<RMDiscoveryOffice365AggregateStatisticDataInfo> QueryRotV3AggregateInfoAsync(RMDiscoveryOffice365QueryParameter queryParameter);

        Task<RMDiscoveryRotCategoryDataInfo> QueryRotV3CategoryDataAsync(Guid o365TenantId);

        #region V3
        Task<RMDiscoveryNodeDataInfo> QueryInactiveV3SummaryNodesAsync(RMDiscoveryOffice365QueryParameter queryParameter);

        Task<Dictionary<string, object>> QueryInactiveV3SummaryNodeTotalAggregateInfoAsync(RMDiscoveryOffice365QueryParameter queryParameter);
        #endregion
    }
}
