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
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Service.Services.Discovery.FileSystem;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters.FileSystem;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.Discovery.FileSystem
{
    //[RMApiAuthorize(RMDiscoveryFileSystemROTPermissionMask.AccessAll, preferred: false)]
    [ValidFileSystemDiscoveryPermissionFilter]
    public class RMDiscoveryFSBasicInfoQueryApiController : BaseApiController
    {
        private readonly IRMDiscoveryFSBasicInfoQueryService _basicInfoQueryService = new RMDiscoveryFSBasicInfoQueryService();

        [HttpGet]
        public Task<List<RMDiscoveryFileExtensionDataInfo>> GetFileExtensions()
        {
            return _basicInfoQueryService.GetFileExtensionsAsync();
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
        public Task<RMDiscoverySummaryStatisticalDataInfo> GetSummaryStatisticalDataInfo()
        {
            return _basicInfoQueryService.GetSummaryStaticalDataInfoAsync();
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
