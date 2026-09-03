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
using AvePoint.RA.Contract.Discovery.Model.Query.Progress;
using AvePoint.RA.Contract.Discovery.Model;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using AvePoint.RA.Service.Services.Discovery.AOSP;

namespace AvePoint.RA.Api.Web.Controllers.AOSPDisocvery
{
    [Route("api/discoveryProgress/[action]")]
    [ApiController]
    //[APIScopeFilter(ContractConstants.RecordsPublicScope)]
    public class DiscoveryAOSPProgressApiControllercs : RAWebApiBase
    {
        private static readonly IRMDiscoveryAOSPProgressService s_progressService = new RMDiscoveryAOSPProgressService();

        [HttpPost]
        public Task<RMDiscoveryProgressSummaryOptimizedInfo> GetSummaryOptimizedInfo([FromBody] Guid o365TenantId)
        {
            return s_progressService.GetSummaryOptimizedInfoAsync(o365TenantId);
        }


        [HttpGet]
        public Task<RMDiscoveryProjectionConfigurationInfo> GetProjectionConfigurationInfo(Guid o365TenantId)
        {
            return s_progressService.GetProjectionConfigurationInfoAsync(o365TenantId);
        }

        [HttpPost]
        public Task<bool> UpdateProjectionConfigurationInfo([FromBody]RMDiscoveryProjectionConfigurationInfo configurationInfo)
        {
            return s_progressService.UpdateProjectionConfigurationInfoAsync(configurationInfo);
        }
    }
}
