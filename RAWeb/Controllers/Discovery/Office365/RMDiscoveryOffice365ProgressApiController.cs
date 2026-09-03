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
using AvePoint.RA.Contract.Discovery.Model.Query.Progress;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Service.Services.Discovery;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.Discovery.Office365
{
    [RMApiAuthorize(RMDiscoveryPermissionMasks.AccessAll, preferred: false)]
    public class RMDiscoveryOffice365ProgressApiController
    {
        private static readonly IRMDiscoveryOffice365ProgressService s_progressService = PlatformWindsorManager.GetService<IRMDiscoveryOffice365ProgressService>();

        [HttpPost]
        public Task<RMDiscoveryProgressSummaryOptimizedInfo> GetSummaryOptimizedInfo([FromBody] Guid o365TenantId)
        {
            return s_progressService.GetSummaryOptimizedInfoAsync(o365TenantId);
        }

        [HttpPost]
        public Task<RMDiscoveryProgressPaginateQueryResult<RMDiscoveryProgressContainerOptimizedInfo>> GetContainerOptimizedInfoes([FromBody] RMDiscoveryProgressPaginateInfo paginateInfo)
        {
            return s_progressService.GetContainerOptimizedInfoesAsync(paginateInfo);
        }

        [HttpPost]
        public Task<RMDiscoveryProgressPaginateQueryResult<RMDiscoveryProgressSiteOptimizedInfo>> GetSiteOptimizedInfoes([FromBody] RMDiscoveryProgressPaginateInfo paginateInfo)
        {
            return s_progressService.GetSiteOptimizedInfoesAsync(paginateInfo);
        }

        [HttpPost]
        public Task<RMDiscoveryProgressPaginateQueryResult<RMDiscoveryProgressOptimizationPlanDataInfo>> GetOptimizationPlanInfoes([FromBody] RMDiscoveryProgressPaginateInfo paginateInfo)
        {
            return s_progressService.GetOptimizationPlanInfoesAsync(paginateInfo);
        }

        [HttpGet]
        public Task<RMDiscoveryProgressOptimizationPlanDetail> GetOptimizationSettingDetail(Guid o365TenantId, Guid settingId)
        {
            return s_progressService.GetOptimizationSettingDetailAsync(o365TenantId, settingId);
        }

        [HttpGet]
        public Task<bool> RequestCancelOptimizationJob(Guid o365TenantId, Guid settingId)
        {
            return s_progressService.GetCancelJobAsync(o365TenantId, settingId);
        }

        [HttpGet]
        public Task<RMDiscoveryProjectionConfigurationInfo> GetProjectionConfigurationInfoAsync(Guid o365TenantId)
        {
            return s_progressService.GetProjectionConfigurationInfoAsync(o365TenantId);
        }

        [HttpPost]
        public Task<bool> UpdateProjectionConfigurationInfoAsync([FromBody] RMDiscoveryProjectionConfigurationInfo configurationInfo)
        {
            return s_progressService.UpdateProjectionConfigurationInfoAsync(configurationInfo);
        }
    }
}
