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
using AvePoint.RA.Contract.Discovery.Model.Configuration.AOSP;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Service.Services.Discovery.AOSP;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using AvePoint.RA.Api.Web.Common;

namespace AvePoint.RA.Api.Web.Controllers.AOSPDisocvery
{
    [Route("api/discoveryOptimization/[action]")]
    [ApiController]
    //[APIScopeFilter(ContractConstants.RecordsPublicScope)]
    public class DiscoveryAOSPOptimizationApiController : RAWebApiBase
    {
        private readonly IRMDiscoveryAOSPOptimizationService _aospOptimizationService = new RMDiscoveryAOSPOptimizationService();

        [HttpPost]
        public Task<RMDiscoveryReturnMessage> SaveOptimizationSetting([FromBody] RMDiscoveryAOSPOptimizationSetting setting)
        {
            return _aospOptimizationService.SaveOptimizationSettingAsync(setting);
        }

        [HttpPost]
        public Task<RAReturnMessage> UpdateArchiveProfileRetention([FromBody] RMDiscoveryAOSPArchiveProfileRetentionRequest request)
        {
            return _aospOptimizationService.UpdateArchiveProfileRetentionAsync(request);
        }

        [HttpPost]
        public Task<RAReturnMessage> DeleteArchiveProfile([FromBody] RMDiscoveryAOSPArchiveProfileDeleteRequest request)
        {
            var archiveProfileIds = request?.ArchiveProfileIds ?? new System.Collections.Generic.List<string>();
            if (string.IsNullOrWhiteSpace(request?.ArchiveProfileId) == false)
            {
                archiveProfileIds.Insert(0, request.ArchiveProfileId);
            }

            return _aospOptimizationService.DeleteArchiveProfileAsync(archiveProfileIds);
        }

        [HttpGet]
        public Task<RAReturnMessage> RunRetentionJob()
        {
            return _aospOptimizationService.RunRetentionJob();
        }
    }
}
