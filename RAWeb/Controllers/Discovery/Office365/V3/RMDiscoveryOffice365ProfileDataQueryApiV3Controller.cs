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
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter.Profile;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Service.Services.Discovery.Office365;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Web.Common;
using System.Collections.Generic;

namespace AvePoint.RA.Web.Controllers.Discovery.Office365.V3
{
    [RMApiAuthorize(RMDiscoveryPermissionMasks.AccessAll, preferred: false)]
    public class RMDiscoveryOffice365ProfileDataQueryApiV3Controller : BaseApiController
    {
        private static readonly IRMDiscoveryOffice365ProfileDataQueryService s_discoveryDataQueryService = new RMDiscoveryOffice365ProfileDataQueryService();

        [HttpPost]
        public Task<RMDiscoveryNodeDataInfo> QueryInactiveOptimizationNodesData([FromBody] RMDiscoveryOffice365ProfileQueryParameter queryParameter)
        {
            return s_discoveryDataQueryService.QueryInactiveV3OptimizationNodesAsync(queryParameter);
        }

        [HttpPost]
        public Task<Dictionary<string, object>> QueryInactiveOptimizationNodeTotalAggregateInfo([FromBody] RMDiscoveryOffice365ProfileQueryParameter queryParameter)
        {
            return s_discoveryDataQueryService.QueryInactiveV3OptimizationNodeTotalAggregateInfoAsync(queryParameter);
        }
    }
}
