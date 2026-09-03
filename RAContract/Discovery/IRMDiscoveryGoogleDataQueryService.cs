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
using AvePoint.RA.Contract.Discovery.Model.Query.Google;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter;

namespace AvePoint.RA.Contract.Discovery
{
    public interface IRMDiscoveryGoogleDataQueryService
    {
        Task<List<RMDiscoveryFileExtensionDataInfo>> QueryInactiveFileExtensionsAsync(RMDiscoveryGoogleQueryParameter queryParameter);

        Task<List<RMDiscoverySizeRangeDataInfo>> QueryInactiveSizeRangesAsync(RMDiscoveryGoogleQueryParameter queryParameter);

        Task<RMDiscoveryNodeDataInfo> QueryInactiveSummaryNodesAsync(RMDiscoveryGoogleQueryParameter queryParameter);

        Task<Dictionary<string, object>> QueryInactiveSummaryNodeTotalAggregateInfoAsync(RMDiscoveryGoogleQueryParameter queryParameter);

        Task<RMDiscoveryGoogleAggregateStatisticDataInfo> QueryInactiveAggregateInfoAsync(RMDiscoveryGoogleQueryParameter queryParameter);

        Task<RMDiscoveryNodeDataInfo> QueryRotSummaryNodeDataAsync(RMDiscoveryGoogleQueryParameter queryParameter);

        Task<List<RMDiscoveryFileExtensionDataInfo>> QueryRotFileExtensionDataAsync(RMDiscoveryGoogleQueryParameter queryParameter);

        Task<Dictionary<string, object>> QueryRotSummaryNodeTotalAggregateInfoDataAsync(RMDiscoveryGoogleQueryParameter queryParameter);

        Task<RMDiscoveryRotRuleDataInfo> QueryRotRuleInfoOfTreeAsync(RMDiscoveryGoogleQueryParameter queryParameter);

        Task<RMDiscoveryGoogleAggregateStatisticDataInfo> QueryRotAggregateInfoAsync(RMDiscoveryGoogleQueryParameter queryParameter);
    }
}
