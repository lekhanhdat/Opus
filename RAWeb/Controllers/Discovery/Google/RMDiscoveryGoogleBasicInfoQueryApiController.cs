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
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Service.Services.Discovery.Google;
using AvePoint.RA.Web.Common;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using AvePoint.RA.Contract.Discovery.Model.Query.Google;
using AvePoint.RA.Web.Common.Filters.GoogleDriveFilter;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Web.Common.WIF;

namespace AvePoint.RA.Web.Controllers.Discovery.Google
{
    [RMApiAuthorize(RMDiscoveryGoogleROTPermissionMask.AccessAll, preferred: false)]
    public class RMDiscoveryGoogleBasicInfoQueryApiController : BaseApiController
    {
        private readonly IRMDiscoveryGoogleBasicInfoQueryService _basicInfoQueryService = new RMDiscoveryGoogleBasicInfoQueryService();

        [HttpGet]
        public Task<List<RMDiscoveryGoogleOrganizationDataInfo>> GetOrganizationInfoes()
        {
            return _basicInfoQueryService.GetOrganizationInfoesAsync();
        }

        [HttpGet]
        public Task<List<RMDiscoveryFileExtensionDataInfo>> GetFileExtensions(string organizationId)
        {
            return _basicInfoQueryService.GetFileExtensionsAsync(organizationId);
        }

        [HttpGet]
        public Task<List<RMDiscoveryWithoutInDateDataInfo>> GetWithoutInDateList()
        {
            return _basicInfoQueryService.GetWithoutInDateListAsync();
        }

        [HttpGet]
        public Task<List<RMDiscoverySizeRangeDataInfo>> GetSizeRangeList()
        {
            return _basicInfoQueryService.GetSizeRangeListAsync();
        }

        [HttpGet]
        public Task<List<RMDiscoveryTableColumnInfo>> GetInactiveTableColumns()
        {
            return _basicInfoQueryService.GetInactiveTableColumnsAsync();
        }

        [HttpGet]
        public Task<RMDiscoverySummaryStatisticalDataInfo> GetSummaryStatisticalDataInfo(string organizationId)
        {
            return _basicInfoQueryService.GetSummaryStaticalDataInfoAsync(organizationId);
        }

        [HttpGet]
        public Task<RMDiscoveryRotRuleDataInfo> GetRotRuleDataInfo()
        {
            return _basicInfoQueryService.GetRotRuleDataInfoAsync();
        }

        [HttpGet]
        public Task<List<RMDiscoveryRuleDataInfo>> GetRotRuleInfoes()
        {
            return _basicInfoQueryService.GetRotRuleInfeosAsync();
        }

        [HttpGet]
        public Task<bool> GetRotEnable()
        {
            return _basicInfoQueryService.GetRotEnableAsync();
        }
    }
}
