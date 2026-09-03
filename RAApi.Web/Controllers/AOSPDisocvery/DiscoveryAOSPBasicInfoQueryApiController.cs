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
using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.AOSP;
using AvePoint.RA.Service.Services.Discovery.AOSP;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers.AOSPDisocvery
{

    [Route("api/discoveryBasicInfoQuery/[action]")]
    [ApiController]
    //[APIScopeFilter(ContractConstants.RecordsPublicScope)]
    public class DiscoveryAOSPBasicInfoQueryApiController : RAWebApiBase
    {
        private static readonly IRMDiscoveryAOSPBasicInfoQueryService s_basicInfoQueryService = new RMDiscoveryAOSPBaiscInfoQueryService();

        [HttpGet]
        public Task<List<RMDiscoveryAOSPTenantDataInfo>> GetO365TenantInfoes()
        {
            return s_basicInfoQueryService.GetO365TenantInfoesAsync();
        }

        [HttpGet]
        public Task<List<RMDiscoveryFileExtensionDataInfo>> GetFileExtensions(string o365TenantId)
        {
            return s_basicInfoQueryService.GetFileExtensionsAsync(o365TenantId);
        }

        [HttpGet]
        public Task<List<RMDiscoveryWithoutInDateDataInfo>> GetWithoutInDateList(string o365TenantId)
        {
            return s_basicInfoQueryService.GetWithoutInDateListAsync(o365TenantId);
        }

        [HttpGet]
        public Task<List<RMDiscoverySizeRangeDataInfo>> GetSizeRangeList(string o365TenantId)
        {
            return s_basicInfoQueryService.GetSizeRangeListAsync(o365TenantId);
        }

        [HttpGet]
        public Task<List<RMDiscoveryTableColumnInfo>> GetInactiveTableColumns(string o365TenantId)
        {
            return s_basicInfoQueryService.GetInactiveTableColumnsAsync(o365TenantId);
        }

        [HttpGet]
        public Task<RMDiscoverySummaryStatisticalDataInfo> GetSummaryStatisticalDataInfo(string o365TenantId)
        {
            return s_basicInfoQueryService.GetSummaryStaticalDataInfoAsync(new Guid(o365TenantId));
        }

        [HttpGet]
        public Task<bool> GetRotEnable(string o365TenantId)
        {
            return s_basicInfoQueryService.GetRotEnableAsync(o365TenantId);
        }

        [HttpGet]
        public Task<RMDiscoveryRotRuleDataInfo> GetRotRuleDataInfo(string o365TenantId)
        {
            return s_basicInfoQueryService.GetRotRuleDataInfoAsync(o365TenantId);
        }
    }
}
