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
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Web.Common.WIF;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Service.Services.Discovery.Salesforce;
using AvePoint.RA.Contract.RMWeb;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [Route("api/discovery/salesforce/[action]")]
    [ApiController]
    [APIScopeFilter(ContractConstants.RecordsPublicScope)]
    [APIValidateSaleforceHeadersFilter]
    public class DiscoverySalesforceApiController : RAWebApiBase
    {
        private readonly IRMDiscoverySalesforceDataQueryService _sfDiscoveryDataQueryService = new RMDiscoverySalesforceDataQueryService();
        private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();
        [HttpGet]
        public Task<RMDiscoverySalesforceSummaryStatisticalDataInfo> GetSummaryStatisticalDataInfo(string? organizationId)
        {
            return _sfDiscoveryDataQueryService.GetSummaryStaticalDataInfoAsync(organizationId);
        }

        [HttpPost]
        public Task<RMDiscoverySalesforceAggregateStatisticDataInfo> QueryInactiveAggregateInfo([FromBody] RMDiscoverySalesforceQueryParameter queryParameter)
        {
            return _sfDiscoveryDataQueryService.QueryInactiveAggregateInfo(queryParameter);
        }
        #region File analysis
        [HttpPost]
        public Task<List<RMDiscoveryFileExtensionDataInfo>> QueryInactiveFileExtensions([FromBody] RMDiscoverySalesforceQueryParameter queryParameter)
        {
            return _sfDiscoveryDataQueryService.QueryInactiveFileExtensionsAsync(queryParameter);
        }
        [HttpPost]
        public Task<List<RMDiscoverySizeRangeDataInfo>> QueryInactiveSizeRanges([FromBody] RMDiscoverySalesforceQueryParameter queryParameter)
        {
            return _sfDiscoveryDataQueryService.QueryInactiveSizeRangesAsync(queryParameter);
        }
        #endregion
        #region Data analysis
        [HttpPost]
        public Task<List<RMSFObjectSelected>> SearchObject([FromBody] RMDiscoverySalesforceQueryParameter queryParameter)
        {
            return _sfDiscoveryDataQueryService.GetObjectByName(queryParameter);
        }
        [HttpPost]
        public Task<RMDiscoveryNodeDataInfo> QueryAnalysis([FromBody] RMDiscoverySalesforceQueryParameter queryParameter)
        {
            return _sfDiscoveryDataQueryService.QueryAnalysis(queryParameter);
        }
        [HttpPost]
        public Task<Dictionary<string, object>> QueryInactiveSummaryObjectTotalInfo([FromBody] RMDiscoverySalesforceQueryParameter queryParameter)
        {
            return _sfDiscoveryDataQueryService.QueryInactiveSummaryObjectTotalInfo(queryParameter);
        }
        [HttpPost]
        public Task<List<RMDiscoverySalesforceYearlyData>> QueryFigureDataInfo([FromBody] RMDiscoverySalesforceQueryParameter queryParameter)
        {
            return _sfDiscoveryDataQueryService.QueryFigureDataInfo(queryParameter);
        }
        #endregion
        [HttpGet]
        public async Task<List<RMDiscoverySalesforceOrgnization>> GetSalesForceOrganizations()
        {
            return await _sfDiscoveryDataQueryService.GetAllOrganizations();
        }
        [HttpGet]
        public Task<List<RMDiscoveryWithoutInDateDataInfo>> GetWithoutInDateList()
        {
            return _sfDiscoveryDataQueryService.GetWithoutInDateListAsync();
        }
        [HttpGet]
        public async Task<bool> GetLicense()
        {
            return await Task.FromResult(LicenseHelperService.HasOpusSalesforceDiscoveryLicense);
        }
    }
}
