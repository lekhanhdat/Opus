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
using AvePoint.RA.Api.Web.Filters;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Service.Services.Discovery.AOSP;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Tenant;

namespace AvePoint.RA.Api.Web.Controllers.AOSPDisocvery
{
    [Route("api/discoveryJobManagement/[action]")]
    [ApiController]
    //[APIScopeFilter(ContractConstants.RecordsPublicScope)]
    public class DiscoveryAOSPJobManagementApiController
    {
        private static readonly IRMDiscoveryAOSPJobManagentService s_jobManagementService = new RMDiscoveryAOSPJobManagentService();
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        [HttpGet]
        public Task<RMDiscoveryAOSPLatestJobInfo> GetJobInfo(string o365TenantId)
        {
            return s_jobManagementService.GetJobInfoAsync(o365TenantId);
        }


        [HttpGet]
        public Task<RMDiscoveryAOSPLatestJobInfo> GetJobInfoByMainJobId(string o365TenantId, string mainJobId)
        {
            return s_jobManagementService.GetJobInfoByMainJobIdAsync(o365TenantId, mainJobId);
        }

        [HttpGet]
        public Task<AOSPJMItemInfo> GetJobInfoByJobId(string jobId, string o365TenantId)
        {
            if (string.IsNullOrWhiteSpace(o365TenantId))
            {
                return JobMonitorService.GetAOSPJobAsync(jobId);
            }

            return JobMonitorService.GetAOSPJobAsync(jobId, new Guid(o365TenantId));
        }

        [HttpPost]
        public Task<JMAOSPDetailsResult> GetJobDetailInfoByPager(JMDetailsQuery queryModel)
        {
            return JobMonitorService.GetAOSPJobDetailsAsync(queryModel);
        }

        [HttpGet]
        public Task<bool> HasAOSPOptimizedJob()
        {
            var isExist= TenantService.CheckTenantExist(TenantLocalValue.LogonGroupId);
            if (!isExist)
            {
                return Task.FromResult(false);
            }
            var hasRunningDSOJob = JobMonitorService.GetRunningJobsCount(JobType.DiscoveryAOSPOptimization);
            return Task.FromResult(hasRunningDSOJob > 0);
        }    
    }
}
