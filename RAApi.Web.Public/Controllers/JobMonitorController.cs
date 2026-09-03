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
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.RA.Api.Contract;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.Common.JobService;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.RACommonUtility.JobControl.JPMC;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridAgentScope)]
    [RMAgentApiPerformanceLogger]
    public class JobMonitorController : RAWebApiBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(JobMonitorController));

        private IJobInfoUpdater _JobInfoUpdate;
        private IJobInfoUpdater JobInfoUpdate => PlatformWindsorManager.GetService(ref _JobInfoUpdate);

        private IJobDetailService _JobDetailService;
        private IJobDetailService JobDetailService => PlatformWindsorManager.GetService(ref _JobDetailService);

        private IJobMonitorService _JobMonitorService;
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService(ref _JobMonitorService);
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IAllocatedJobWeightServices AllocatedJobWeightServices => PlatformWindsorManager.GetService<IAllocatedJobWeightServices>();

        [HttpPost]
        public void DeleteJobById([FromBody] string jobid)
        {
            JobMonitorService.DeleteJobsAsync(new List<string>() { jobid });
        }

        [HttpPost]
        public void DeleteJobForAgentById([FromBody] string jobid)
        {
            JobMonitorService.DeleteJobsForAgentAsync(new List<string>() { jobid });
        }

        [HttpPost]
        public bool UpdateJobProgress([FromBody] HBJobStatusInfo hBJobStatusInfo)
        {
            try
            {
                using (new PerformanceScope("excute update job Progress"))
                {
                    JobInfoUpdate.UpdateJobProgress(hBJobStatusInfo.JobId, hBJobStatusInfo.Progress);
                }

            }
            catch (Exception ex)
            {
                logger.Error($"excute update job progress api exception:{ex.ToString()}");
            }
            return false;
        }

        [HttpPost]
        public void UpdateJobState([FromBody] HBJobStatusInfo hBJobStatusInfo)
        {
            try
            {
                using (new PerformanceScope("excute update job state"))
                {
                    JobInfoUpdate.UpdateJobState(hBJobStatusInfo.JobId, hBJobStatusInfo.State, hBJobStatusInfo.Comment);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"excute update job state api exception:{ex.ToString()}");
            }

        }

        [HttpPost]
        public IActionResult SendReport([FromBody] HBReportFileInfo reportInfo)
        {

            using (new PerformanceScope("excute upload job report"))
            {
                var result = JobDetailService.SendReport(reportInfo);
                return Ok(result);
            }
        }
    }
}

