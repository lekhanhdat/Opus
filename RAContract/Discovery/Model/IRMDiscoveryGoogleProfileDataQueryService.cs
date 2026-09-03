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
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter.Profile;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Discovery
{
    public interface IRMDiscoveryGoogleProfileDataQueryService
    {
        Task<RMDiscoveryNodeDataInfo> QueryInactiveOptimizationNodesAsync(RMDiscoveryGoogleProfileQueryParameter queryParameter);

        Task<Dictionary<string, object>> QueryInactiveOptimizationNodeTotalAggregateInfoAsync(RMDiscoveryGoogleProfileQueryParameter queryParameter);

        Task<Dictionary<string, object>> QueryInactiveAggregateInfo(RMDiscoveryGoogleProfileQueryParameter queryParameter);

        Task<RMDiscoveryNodeDataInfo> QueryRotOptimizationNodesAsync(RMDiscoveryGoogleProfileQueryParameter queryParameter);

        Task<Dictionary<string, object>> QueryRotOptimizationNodeTotalAggregateInfoAsync(RMDiscoveryGoogleProfileQueryParameter queryParameter);
    }
}
