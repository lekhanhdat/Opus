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
using AvePoint.RA.Contract.Salesforce;
using AvePoint.RA.Contract.Salesforce.Model;
using AvePoint.RA.Service.Services.Discovery;
using AvePoint.RA.Web.Common;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Service.Services.Discovery.Salesforce;
using AvePoint.RA.Web.Common.WIF;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Web.Common.Filters;

namespace AvePoint.RA.Web.Controllers.Discovery.Salesforce
{
    [RMApiAuthorize(RMDiscoverySalesforcePermissionMask.AccessAll, preferred: false)]
    [APIValidateSalesforceParameterFilter]
    public class RMDiscoverySalesforceDataQueryApiController : BaseApiController
    {
        private readonly IRMDiscoverySalesforceDataQueryService _sfDiscoveryDataQueryService = PlatformWindsorManager.GetService<IRMDiscoverySalesforceDataQueryService>();
        [HttpGet]
        public Task<RMDiscoverySalesforceSummaryStatisticalDataInfo> GetSummaryStatisticalDataInfo(string? organizationId)
        {
            return _sfDiscoveryDataQueryService.GetSummaryStaticalDataInfoAsync(organizationId);
        }

        [HttpPost]
        public Task<RMDiscoverySalesforceAggregateStatisticDataInfo> QueryInactiveAggregateInfo([FromBody] RMDiscoverySalesforceQueryParameter salesforceQueryParameter)
        {
            return _sfDiscoveryDataQueryService.QueryInactiveAggregateInfo(salesforceQueryParameter);
        }
        #region File analysis
        [HttpPost]
        public Task<List<RMDiscoveryFileExtensionDataInfo>> QueryInactiveFileExtensions([FromBody] RMDiscoverySalesforceQueryParameter salesforceQueryParameter)
        {
            return _sfDiscoveryDataQueryService.QueryInactiveFileExtensionsAsync(salesforceQueryParameter);
        }
        [HttpPost]
        public Task<List<RMDiscoverySizeRangeDataInfo>> QueryInactiveSizeRanges([FromBody] RMDiscoverySalesforceQueryParameter salesforceQueryParameter)
        {
            return _sfDiscoveryDataQueryService.QueryInactiveSizeRangesAsync(salesforceQueryParameter);
        }
        #endregion
        #region Data analysis
        [HttpPost]
        public Task<List<RMSFObjectSelected>> SearchObjectAsync([FromBody] RMDiscoverySalesforceQueryParameter salesforceQueryParameter)
        {
            return _sfDiscoveryDataQueryService.GetObjectByName(salesforceQueryParameter);
        }
        [HttpPost]
        public Task<RMDiscoveryNodeDataInfo> QueryAnalysis([FromBody] RMDiscoverySalesforceQueryParameter salesforceQueryParameter)
        {
            return _sfDiscoveryDataQueryService.QueryAnalysis(salesforceQueryParameter);
        }
        [HttpPost]
        public Task<Dictionary<string, object>> QueryInactiveSummaryObjectTotalInfo([FromBody] RMDiscoverySalesforceQueryParameter salesforceQueryParameter)
        {
            return _sfDiscoveryDataQueryService.QueryInactiveSummaryObjectTotalInfo(salesforceQueryParameter);
        }
        [HttpPost]
        public Task<List<RMDiscoverySalesforceYearlyData>> QueryFigureDataInfo([FromBody] RMDiscoverySalesforceQueryParameter salesforceQueryParameter)
        {
            return _sfDiscoveryDataQueryService.QueryFigureDataInfo(salesforceQueryParameter);
        }
        #endregion
        
        [HttpGet]
        public Task<List<RMDiscoveryWithoutInDateDataInfo>> GetWithoutInDateList()
        {
            return _sfDiscoveryDataQueryService.GetWithoutInDateListAsync();
        }
    }
}
