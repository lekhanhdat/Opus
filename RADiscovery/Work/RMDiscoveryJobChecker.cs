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
using AvePoint.RA.DB.Model.Discovery;
using Cloud.Sdk.Data.IE;
using DocumentFormat.OpenXml.Office.CustomUI;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RADiscovery.Work
{
    public class RMDiscoveryJobChecker : RMDiscoveryWorker
    {
        public async Task CheckAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var mainjob = efContext.MainJobs.FirstOrDefault(item => item.Status == RMDiscoveryJobStatus.Running);
            if (mainjob == null)
            {
                return;
            }

            await CheckTimeoutJobsAsync(mainjob);
            await CalculateJobsStatusAsync(mainjob);
            await CheckDiscoveryJobsAsync(mainjob);
        }

        public async Task CheckTimeoutJobsAsync(RMDiscoveryMainJob mainJob)
        {
            const int runningTimeoutHour = 1;
            const int pendingTimeoutHour = 3;
            var runningTimeoutTicks = DateTime.UtcNow.AddHours(0 - runningTimeoutHour).Ticks;
            var pendingTimeoutTicks = DateTime.UtcNow.AddHours(0 - pendingTimeoutHour).Ticks;
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = @"UPDATE [dbo].[RMAnalysisJobs] SET Status = 11, EndTime = @EndTime WHERE
JobId = @MainJobId AND Status = @Status AND LastModifiedTime < @LastModifiedTime";
            var effectRunningCount = await context.ExecuteNonQueryAsync(sql, 
                new SqlParameter("@MainJobId", mainJob.Id),
                new SqlParameter("@Status", RMDiscoveryJobStatus.Running), 
                new SqlParameter("@LastModifiedTime", runningTimeoutTicks));
            var effectPendingCount = await context.ExecuteNonQueryAsync(sql,
                new SqlParameter("@MainJobId", mainJob.Id),
                new SqlParameter("@Status", RMDiscoveryJobStatus.Pending), 
                new SqlParameter("@LastModifiedTime", 
                pendingTimeoutTicks));
        }

        public async Task CalculateJobsStatusAsync(RMDiscoveryMainJob mainJob)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var completingJobs = efContext.DiscoveryJobs.Where(item => item.Status == RMDiscoveryJobStatus.Completing);
            foreach(var completingJob in completingJobs)
            {
                var statusDic = await efContext.AnalysisJobs.Where(item => item.MainJobId == mainJob.Id).Select(item => item.Status).GroupBy(item => item)
                    .ToDictionaryAsync(item => item.Key, item => item.Count());
                _ = statusDic.TryGetValue(RMDiscoveryJobStatus.Finished, out var finishedCount);
                _ = statusDic.TryGetValue(RMDiscoveryJobStatus.Failed, out var failedCount);
                _ = statusDic.TryGetValue(RMDiscoveryJobStatus.Timeout, out var timeoutCount);
                completingJob.CompletedSiteCount = finishedCount + failedCount + timeoutCount;
                if(completingJob.SiteCount == completingJob.CompletedSiteCount)
                {
                    completingJob.EndTime = DateTime.UtcNow.Ticks;
                    completingJob.Status = RMDiscoveryJobStatus.Finished;
                    if(finishedCount > 0 && completingJob.SiteCount - finishedCount > 0)
                    {
                        completingJob.Status = RMDiscoveryJobStatus.Exception;
                    }
                    else if(failedCount + timeoutCount > 0)
                    {
                        completingJob.Status = RMDiscoveryJobStatus.Failed;
                    }
                    efContext.Entry(completingJob).Property(item => item.Status).IsModified = true;
                    efContext.Entry(completingJob).Property(item => item.EndTime).IsModified = true;
                    await efContext.SaveChangesAsync();
                }
            }

            if(!completingJobs.Any())
            {
                var statusDic = await efContext.DiscoveryJobs.Where(item => item.MainJobId == mainJob.Id)
                    .Select(item => item.Status)
                    .GroupBy(item => item)
                    .ToDictionaryAsync(item => item.Key, item => item.Count());
                _ = statusDic.TryGetValue(RMDiscoveryJobStatus.Pending, out var pendingCount);
                _ = statusDic.TryGetValue(RMDiscoveryJobStatus.Running, out var runningCount);
                _ = statusDic.TryGetValue(RMDiscoveryJobStatus.Completing, out var completingCount);
                _ = statusDic.TryGetValue(RMDiscoveryJobStatus.Finished, out var finishedCount);
                _ = statusDic.TryGetValue(RMDiscoveryJobStatus.Failed, out var failedCount);
                _ = statusDic.TryGetValue(RMDiscoveryJobStatus.Exception, out var exceptionCount);
                if(pendingCount + runningCount + completingCount == 0)
                {
                    mainJob.EndTime = DateTime.UtcNow.Ticks;
                    mainJob.Status = RMDiscoveryJobStatus.Finished;
                    if (finishedCount > 0 && failedCount + exceptionCount > 0)
                    {
                        mainJob.Status = RMDiscoveryJobStatus.Exception;
                    }
                    else if (failedCount + exceptionCount > 0)
                    {
                        mainJob.Status = RMDiscoveryJobStatus.Failed;
                    }
                    efContext.Entry(mainJob).Property(item => item.Status).IsModified = true;
                    efContext.Entry(mainJob).Property(item => item.EndTime).IsModified = true;
                    await efContext.SaveChangesAsync();
                }
            }
        }

        public async Task CheckDiscoveryJobsAsync(RMDiscoveryMainJob mainJob)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();

            var discoveryJobAsyncEnumerable = GetNeedCheckJobsAsync(mainJob);
            await foreach (var discoveryJob in discoveryJobAsyncEnumerable)
            {
                var checkTime = DateTime.UtcNow.Ticks; // need to get from completed job info.
                var jobs = await _ieApiClient.JobService.GetFinishedSubJobObjectIdByCompletedTimeAsync(discoveryJob.Id, discoveryJob.LastCheckedTime);
                discoveryJob.LastCheckedTime = checkTime;
                efContext.Entry(discoveryJob).Property(item => item.LastCheckedTime).IsModified = true;
                await efContext.SaveChangesAsync();
            }
        }

        public async Task CheckDiscoveryJobAsync(RMDiscoveryJob discoveryJob)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var jobInfo = await _ieApiClient.JobService.GetJobHistoryAsync(discoveryJob.Id);
            var jobStatus = jobInfo.Status;
            if (jobStatus == JobStatus.Failed)
            {
                discoveryJob.Status = RMDiscoveryJobStatus.Failed;
                discoveryJob.EndTime = DateTime.UtcNow.Ticks;
                efContext.Entry(discoveryJob).Property(item => item.Status).IsModified = true;
                efContext.Entry(discoveryJob).Property(item => item.EndTime).IsModified = true;
                await efContext.SaveChangesAsync();
                await ChangeAnalysisJobStatus(discoveryJob.MainJobId, discoveryJob.MainJobId, RMDiscoveryJobStatus.Failed);
            }

        }

        private async IAsyncEnumerable<RMDiscoveryJob> GetNeedCheckJobsAsync(RMDiscoveryMainJob mainJob)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var discoveryJobs = await efContext.DiscoveryJobs.Where(item => item.Equals(mainJob.Id)
                && (item.Status == RMDiscoveryJobStatus.Pending || item.Status == RMDiscoveryJobStatus.Running)
            ).ToListAsync();

            foreach (var discoveryJob in discoveryJobs)
            {
                var jobInfo = await _ieApiClient.JobService.GetJobHistoryAsync(discoveryJob.Id);
                var status = jobInfo.Status switch
                {
                    JobStatus.Pending => RMDiscoveryJobStatus.Pending,
                    JobStatus.Running => RMDiscoveryJobStatus.Running,
                    JobStatus.Skipped => RMDiscoveryJobStatus.Skipped,
                    JobStatus.Failed => RMDiscoveryJobStatus.Failed,
                    JobStatus.Finshed => RMDiscoveryJobStatus.Finished,
                    _ => throw new NotSupportedException("")
                };

                switch(status)
                {
                    case RMDiscoveryJobStatus.Pending:
                        continue;
                    case RMDiscoveryJobStatus.Running:
                        await SetJobStatusToRunning(discoveryJob);
                        yield return discoveryJob;
                        break;
                    case RMDiscoveryJobStatus.Failed:
                        await SetJobStatusToFailed(discoveryJob, jobInfo);
                        break;
                    case RMDiscoveryJobStatus.Exception:
                        await SetJobStatusToException(discoveryJob);
                        yield return discoveryJob;
                        break;
                    case RMDiscoveryJobStatus.Finished:
                        await SetJobStatusToFinisihed(discoveryJob);
                        break;
                }
            }
        }

        private async Task SetJobStatusToFailed(RMDiscoveryJob discoveryJob, JobInfoModel infoMode)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            discoveryJob.Status = RMDiscoveryJobStatus.Failed;
            discoveryJob.EndTime = infoMode.FinishTime?.Ticks ?? DateTime.UtcNow.Ticks;
            efContext.Entry(discoveryJob).Property(item => item.Status).IsModified = true;
            efContext.Entry(discoveryJob).Property(item => item.EndTime).IsModified = true;
            await efContext.SaveChangesAsync();
            await ChangeAnalysisJobStatus(discoveryJob.MainJobId, discoveryJob.Id, RMDiscoveryJobStatus.Failed);
        }

        private async Task SetJobStatusToFinisihed(RMDiscoveryJob discoveryJob)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            discoveryJob.Status = RMDiscoveryJobStatus.Completing;
            efContext.Entry(discoveryJob).Property(item => item.Status).IsModified = true;
            await efContext.SaveChangesAsync();
            await ChangeAnalysisJobStatus(discoveryJob.MainJobId, discoveryJob.Id, RMDiscoveryJobStatus.Pending);
        }

        private async Task SetJobStatusToRunning(RMDiscoveryJob discoveryJob)
        {
            if(discoveryJob.Status == RMDiscoveryJobStatus.Pending)
            {
                using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
                discoveryJob.Status = RMDiscoveryJobStatus.Running;
                efContext.Entry(discoveryJob).Property(item => item.Status).IsModified = true;
                await efContext.SaveChangesAsync();
            }
        }

        private async Task SetJobStatusToException(RMDiscoveryJob discoveryJob)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            discoveryJob.Status = RMDiscoveryJobStatus.Completing;
            efContext.Entry(discoveryJob).Property(item => item.Status).IsModified = true;
            await efContext.SaveChangesAsync();
        }

        private async Task ChangeAnalysisJobStatus(Guid mainJobId, Guid discoveryJobId, RMDiscoveryJobStatus status)
        {
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            var sql = @"UPDATE [dbo].[RMDiscoveryAnalysisJobs] SET Status = @Status 
WHERE JobId = @JobId AND DiscoveryJobId = @DiscoveryJobId AND Status = 1";
            var effectCount = await context.ExecuteNonQueryAsync(sql,
                new SqlParameter("@Status", status),
                new SqlParameter("@JobId", mainJobId),
                new SqlParameter("@DiscoveryJobId", discoveryJobId));
        }
    }
}
