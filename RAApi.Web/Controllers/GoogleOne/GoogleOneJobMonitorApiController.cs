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

using Microsoft.AspNetCore.Mvc;
using AvePoint.Api.Service.Implement;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using System.Threading.Tasks;
using System;
using AvePoint.Api.Service.Interface;
using AvePoint.RA.Common;

namespace AvePoint.RA.Api.Web.Controllers.GoogleOne
{
    [Route("api/googleone/jobmonitor")]
    public class GoogleOneJobMonitorApiController : GoogleOneApiBaseController
    {
        public IGoogleOneJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IGoogleOneJobMonitorService>();


        [HttpPost("summary")]
        public async Task<JMJobSummary> GetJobSummary([FromBody] string id)
        {
            try
            {
                return await JobMonitorService.GetJobSummaryAsync(id);
            }
            catch (Exception e)
            {
                Console.WriteLine($"GetJobSummary error: {e}");
            }
            return null;
        }

        [HttpPost("details")]
        public async Task<String> GetJobDetails([FromBody] JMDetailsQuery queryModel)
        {
            return await JobMonitorService.GetJobDetailsAsync(queryModel);
        }

        [HttpPost("summary/statistics")]
        public async Task<JMJobDetails> GetJobSummaryStatistics([FromBody] string id)
        {
            return await JobMonitorService.GetJobSummaryStatisticsAsync(id);
        }
    }
}
