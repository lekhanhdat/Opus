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
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Query.Google;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers.GoogleOne.Discovery.Google
{
    [Route("api/googleone/discovery/google/basicinfo")]
    public class GoogleOneDiscoveryGoogleBasicInfoApiController : GoogleOneApiBaseController
    {
        private readonly IRMDiscoveryGoogleBasicInfoQueryService _basicInfoQueryService = PlatformWindsorManager.GetService<IRMDiscoveryGoogleBasicInfoQueryService>();

        [HttpGet("organizations")]
        public Task<List<RMDiscoveryGoogleOrganizationDataInfo>> GetOrganizationInfos()
        {
            return _basicInfoQueryService.GetOrganizationInfoesAsync();
        }

        [HttpGet("fileextensions")]
        public Task<List<RMDiscoveryFileExtensionDataInfo>> GetFileExtensions([FromQuery] string organizationId)
        {
            return _basicInfoQueryService.GetFileExtensionsAsync(organizationId);
        }

        [HttpGet("withoutindates")]
        public Task<List<RMDiscoveryWithoutInDateDataInfo>> GetWithoutInDateList()
        {
            return _basicInfoQueryService.GetWithoutInDateListAsync();
        }

        [HttpGet("sizeranges")]
        public Task<List<RMDiscoverySizeRangeDataInfo>> GetSizeRangeList()
        {
            return _basicInfoQueryService.GetSizeRangeListAsync();
        }

        [HttpGet("inactivetablecolumns")]
        public Task<List<RMDiscoveryTableColumnInfo>> GetInactiveTableColumns()
        {
            return _basicInfoQueryService.GetInactiveTableColumnsAsync();
        }

        [HttpGet("statisticaldata/summary")]
        public Task<RMDiscoverySummaryStatisticalDataInfo> GetSummaryStatisticalDataInfo(string organizationId)
        {
            return _basicInfoQueryService.GetSummaryStaticalDataInfoAsync(organizationId);
        }

        [HttpGet("rot/enable")]
        public Task<bool> GetRotEnable()
        {
            return _basicInfoQueryService.GetRotEnableAsync();
        }

        [HttpGet("rot/rules")]
        public Task<List<RMDiscoveryRuleDataInfo>> GetRotRuleInfoes()
        {
            return _basicInfoQueryService.GetRotRuleInfeosAsync();
        }

        [HttpGet("rot/rules/data")]
        public Task<RMDiscoveryRotRuleDataInfo> GetRotRuleDataInfo()
        {
            return _basicInfoQueryService.GetRotRuleDataInfoAsync();
        }
    }
}
