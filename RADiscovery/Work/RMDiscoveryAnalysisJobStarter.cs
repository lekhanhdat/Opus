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
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.DB.Core.Discovery;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RADiscovery.Work
{
    public class RMDiscoveryAnalysisJobStarter : RMDiscoveryWorker
    {
        private const int MAX_JOB_COUNT = 10;

        public async Task StartAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var mainjob = efContext.MainJobs.FirstOrDefault(item => item.Status == RMDiscoveryJobStatus.Running);
            if (mainjob == null)
            {
                return;
            }

            var runningJobCount = await efContext.AnalysisJobs.Where(item => item.MainJobId == mainjob.Id && item.Status == RMDiscoveryJobStatus.Running).CountAsync();
            var canRunJobCount = runningJobCount - MAX_JOB_COUNT;
            var jobs = await efContext.AnalysisJobs.Where(item => item.MainJobId == mainjob.Id && item.Status == RMDiscoveryJobStatus.Running)
                .OrderBy(item => item.StartTime)
                .Take(canRunJobCount)
                .ToListAsync();
            foreach(var job in jobs) 
            { 
                
            }
        }
    }
}
