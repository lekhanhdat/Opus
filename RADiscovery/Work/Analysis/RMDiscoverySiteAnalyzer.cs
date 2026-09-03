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
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Model.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RADiscovery.Work.Analysis
{
    public class RMDiscoverySiteAnalyzer : RMDiscoveryWorker
    {
        private readonly IRMDiscoveryRuleInfoDao _ruleDao = new RMDiscoveryRuleInfoDao();

        private readonly Guid _jobId;

        public RMDiscoverySiteAnalyzer(string jobId) : base() 
        { 
            _jobId = new Guid(jobId);
        }

        public async Task RunAsync()
        {
            var canNext = await PreProcessAsync();
            if (!canNext)
            {
                return;
            }

            var tokenSource = new CancellationTokenSource();
            _ = UpdateJobStatusAsync(tokenSource.Token);

            

            tokenSource.Cancel(false);
        }

        private async Task<bool> PreProcessAsync()
        {
            using var context = await RMDiscoveryDBManager.GetEFContextAsync();
            var analysisJobInfo = context.AnalysisJobs.First(item => item.Id == _jobId);
            if(analysisJobInfo.Status != RMDiscoveryJobStatus.Running)
            {
                throw new Exception($"Current job [{analysisJobInfo.Id}] is running.");
            }

            analysisJobInfo.Status = RMDiscoveryJobStatus.Running;
            context.Entry(analysisJobInfo).Property(item => item.Status).IsModified = true;
            await context.SaveChangesAsync();
            return true;
        }

        private async Task ProcessInactive(List<RMDiscoveryRuleInfo> inactiveRules)
        {
            var sql = $"filter()";
        }

        private async Task UpdateJobStatusAsync(CancellationToken cancellationToken)
        {
            using var context = await RMDiscoveryDBManager.GetEFContextAsync();
            var analysisJobInfo = context.AnalysisJobs.First(item => item.Id == _jobId);

            while(!cancellationToken.IsCancellationRequested)
            {
                analysisJobInfo.LastModifiedTime = DateTime.UtcNow.Ticks;
                context.Entry(analysisJobInfo).Property(item => item.Status).IsModified = true;
                await context.SaveChangesAsync();
                await Task.Delay(TimeSpan.FromMinutes(10));
            }
        }


    }
}
