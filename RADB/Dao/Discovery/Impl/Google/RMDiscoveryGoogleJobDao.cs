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
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Office365;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Google
{
    public class RMDiscoveryGoogleJobDao : IRMDiscoveryGoogleJobDao
    {
        private static readonly HashSet<RMDiscoveryJobStatus> S_PROCESSING_JOB_STATUS = new()
        {
            RMDiscoveryJobStatus.Preparing,
            RMDiscoveryJobStatus.Waiting,
            RMDiscoveryJobStatus.Pending,
            RMDiscoveryJobStatus.Running,
            RMDiscoveryJobStatus.Completing,
        };
        public async Task<(bool has, RMDiscoveryGoogleMainJob mainJobInfo)> TryGetProcessingMainJobAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var processingJob = await efContext.GoogleMainJobs.FirstOrDefaultAsync(item => S_PROCESSING_JOB_STATUS.Contains(item.Status));
            return (processingJob != null, processingJob);
        }

        public async Task AddOrUpdateMainJobAsync(RMDiscoveryGoogleMainJob mainJobInfo)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            mainJobInfo.LastModifiedTime = DateTime.UtcNow.Ticks;
            efContext.GoogleMainJobs.AddOrUpdate(mainJobInfo);
            await efContext.SaveChangesAsync();
        }

        public async Task<(bool has, RMDiscoveryGoogleMainJob mainJob)> TryGetMainJobAsync(RMDiscoveryJobStatus status)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var processingJob = await efContext.GoogleMainJobs.FirstOrDefaultAsync(item => item.Status == status);
            return (processingJob != null, processingJob);
        }

        public async Task<(bool has, RMDiscoveryGoogleMainJob mainJob)> TryGetMainJobAsync(Guid jobId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var job = await efContext.GoogleMainJobs.FirstOrDefaultAsync(item => item.Id == jobId);
            if (job != null && job.Type == RMDiscoveryJobType.None)
            {
                job.Type = RMDiscoveryJobType.Newly;
            }
            return (job != null, job);
        }

        public async Task<List<RMDiscoveryGoogleAnalysisJob>> GetTimeoutAnalysisJobsAsync(Guid mainJobId, RMDiscoveryJobStatus status, long timeout)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();

            return await efContext.GoogleAnalysisJobs.Where(item =>
                item.MainJobId == mainJobId &&
                item.Status == status &&
                item.LastModifiedTime < timeout
            ).ToListAsync();
        }

        public async Task AddOrUpdateAnalysisJobAsync(params RMDiscoveryGoogleAnalysisJob[] analysisJobs)
        {
            if (!analysisJobs.Any())
            {
                return;
            }
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            efContext.GoogleAnalysisJobs.AddOrUpdate(analysisJobs);
            await efContext.SaveChangesAsync();
        }

        public async Task<bool> HasDiscoveryJobAsync(Guid mainJobId, params RMDiscoveryJobStatus[] jobStatus)
        {
            var jobStatusList = jobStatus.ToList();
            var hasAny = jobStatus.Any();
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.GoogleDiscoveryJobs.AnyAsync(item => item.MainJobId == mainJobId && Enumerable.Contains(jobStatusList, item.Status));
        }

        public async Task<List<RMDiscoveryGoogleDiscoveryJob>> GetDiscoveryJobsAsync(Guid mainJobId, params RMDiscoveryJobStatus[] status)
        {
            var hasStatus = status.Any();
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.GoogleDiscoveryJobs.Where(item => item.MainJobId == mainJobId && (!hasStatus || Enumerable.Contains(status, item.Status))).ToListAsync();
        }

        public async Task<List<RMDiscoveryGoogleAnalysisJob>> GetAnalysisJobsAsync(Guid mainJobId, int count, params RMDiscoveryJobStatus[] status)
        {
            var hasStatus = status.Any();
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.GoogleAnalysisJobs.Where(item => item.MainJobId == mainJobId && (!hasStatus || Enumerable.Contains(status, item.Status)))
                .OrderByDescending(item => item.LastModifiedTime)
                .Take(count)
                .ToListAsync();
        }

        public async IAsyncEnumerable<RMDiscoveryGoogleAnalysisJob> GetAnalysisJobsWithPaginationAsync(Guid mainJobId, int pageSize, params RMDiscoveryJobStatus[] status)
        {
            var hasStatus = status.Any();
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            for (var i = 0; ; i++)
            {
                var jobs = await efContext.GoogleAnalysisJobs.Where(item => item.MainJobId == mainJobId && (!hasStatus || Enumerable.Contains(status, item.Status)))
                                                    .OrderByDescending(item => item.Id)
                                                    .Skip(pageSize * i)
                                                    .Take(pageSize)
                                                    .ToListAsync();
                foreach (var job in jobs)
                {
                    yield return job;
                }

                if (jobs.Count < pageSize)
                {
                    break;
                }
            }
        }

        public async IAsyncEnumerable<RMDiscoveryGoogleAnalysisJob> GetAnalysisJobsByDiscoveryJobWithPaginationAsync(Guid discoveryJobId, int pageSize, params RMDiscoveryJobStatus[] status)
        {
            var hasStatus = status.Any();
            for (var i = 0; ; i++)
            {
                using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
                var jobs = await efContext.GoogleAnalysisJobs.Where(item => item.DiscoveryJobId == discoveryJobId)
                                                    .OrderByDescending(item => item.Id)
                                                    .Skip(pageSize * i)
                                                    .Take(pageSize)
                                                    .ToListAsync();
                foreach (var job in jobs.Where(item => !hasStatus || Enumerable.Contains(status, item.Status)).ToList())
                {
                    yield return job;
                }

                if (jobs.Count < pageSize)
                {
                    break;
                }
            }
        }

        public async IAsyncEnumerable<RMDiscoveryGoogleAnalysisJob> GetAnalysisJobReportWithPaginationAsync(Guid mainJobId, int pageSize)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            for (var i = 0; ; i++)
            {
                var jobs = await efContext.GoogleAnalysisJobs.Where(item => item.MainJobId == mainJobId)
                                                    .OrderByDescending(item => item.Status)
                                                    .ThenBy(item => item.DriveName)
                                                    .Skip(pageSize * i)
                                                    .Take(pageSize)
                                                    .ToListAsync();
                foreach (var job in jobs)
                {
                    yield return job;
                }

                if (jobs.Count < pageSize)
                {
                    break;
                }
            }
        }

        public async Task<Dictionary<RMDiscoveryJobStatus, int>> GetAnalysisCompletedStatusAsync(Guid discoveryJobId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.GoogleAnalysisJobs.Where(item => item.DiscoveryJobId == discoveryJobId).GroupBy(item => item.Status).ToDictionaryAsync(item => item.Key, item => item.Count());
        }

        public async Task<bool> HasProcessingAnalysisJobAsync(Guid discoveryJobId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.GoogleAnalysisJobs.AnyAsync(item => item.DiscoveryJobId == discoveryJobId && S_PROCESSING_JOB_STATUS.Contains(item.Status));
        }

        public async Task<bool> HasProcessingDiscoveryJobAsync(Guid mainJobId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.GoogleDiscoveryJobs.AnyAsync(item => item.MainJobId == mainJobId && S_PROCESSING_JOB_STATUS.Contains(item.Status));
        }

        public async Task AddOrUpdateDiscoveryJobAsync(params RMDiscoveryGoogleDiscoveryJob[] discoveryJobs)
        {
            if (!discoveryJobs.Any())
            {
                return;
            }
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            efContext.GoogleDiscoveryJobs.AddOrUpdate(discoveryJobs);
            await efContext.SaveChangesAsync();
        }

        public async Task<(bool has, RMDiscoveryGoogleAnalysisJob analysisJob)> TryGetAnalysisJobAsync(Guid discoveryJobId, string driveId, params RMDiscoveryJobStatus[] status)
        {
            var hasStatus = status.Any();
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var res = await efContext.GoogleAnalysisJobs.FirstOrDefaultAsync(item => item.DiscoveryJobId == discoveryJobId && item.DriveId == driveId.ToString() && (!hasStatus || Enumerable.Contains(status, item.Status)));
            return (res != null, res);
        }

        public async Task<int> ChangeAnalysisJobsStatusAsync(Guid discoveryJobId, RMDiscoveryJobStatus willChangeStatus, bool isEnd, RMDiscoveryJobFailedCause failedCause, params RMDiscoveryJobStatus[] beforeStatus)
        {
            var parameters = new List<SqlParameter>();
            var sql = "UPDATE [dbo].[RMGoogleAnalysisJobs] SET Status = @Status, FailedCause = @FailedCause";
            parameters.Add(new SqlParameter("@Status", willChangeStatus));
            parameters.Add(new SqlParameter("@FailedCause", failedCause));
            if (isEnd)
            {
                sql += $", EndTime = @EndTime";
                parameters.Add(new SqlParameter("@EndTime", DateTime.UtcNow.Ticks));
            }
            sql += " WHERE DiscoveryJobId = @DiscoveryJobId";
            parameters.Add(new SqlParameter("@DiscoveryJobId", discoveryJobId));
            if (beforeStatus.Any())
            {
                var inClauseParamName = DatabaseUtility.BuildInClause(beforeStatus.ConvertAll(item => (int)item), out var paramList);
                parameters.AddRange(paramList);
                sql += $" AND Status IN {inClauseParamName}";
            }

            using var context = await RMDiscoveryDBManager.GetEFContextAsync();
            var effectCount = await context.Database.ExecuteSqlCommandAsync(sql, parameters.ToArray());
            return effectCount;
        }

        public async Task<int> ChangeAnalysisJobsStatusAsync(RMDiscoveryJobStatus status, RMDiscoveryJobFailedCause failedCause, params Guid[] discoveryJobIds)
        {
            if (!discoveryJobIds.Any())
            {
                return 0;
            }
            var sql = $"UPDATE [dbo].[RMGoogleAnalysisJobs] SET Status = @Status, FailedCause = @FailedCause WHERE DiscoveryJobId IN {DatabaseUtility.BuildInClause(discoveryJobIds)}";
            var context = await RMDiscoveryDBManager.GetContextAsync();
            var effectCount = await context.ExecuteNonQueryAsync(sql, new SqlParameter("@Status", status), new SqlParameter("@FailedCause", failedCause));
            return effectCount;
        }

        public async Task BatchInsertAnalysisJobAsync(List<RMDiscoveryGoogleAnalysisJob> analysisJobs)
        {
            if (analysisJobs?.Count == 0)
            {
                return;
            }
            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteInsertAsync(analysisJobs);
        }

        public async Task<(bool has, RMDiscoveryGoogleMainJob mainJobInfo)> TryGetLatestMainJobAsync(params RMDiscoveryJobType[] types)
        {
            var hasTypes = types.Any();
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var latestJob = await efContext.GoogleMainJobs
                .Where(item => (!hasTypes || Enumerable.Contains(types, item.Type)))
                .OrderByDescending(item => item.StartTime).FirstOrDefaultAsync();
            return (latestJob != null, latestJob);
        }

        public async Task<Dictionary<RMDiscoveryJobStatus, int>> GetDiscoveryCompletedStatusAsync(Guid mainJobId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.GoogleDiscoveryJobs.Where(item => item.MainJobId == mainJobId).GroupBy(item => item.Status).ToDictionaryAsync(item => item.Key, item => item.Count());
        }


        public async Task<Dictionary<RMDiscoveryJobStatus, int>> GetAnalysisCompletedStatusByMainJobIdAsync(Guid mainJobId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.GoogleAnalysisJobs.Where(item => item.MainJobId == mainJobId).GroupBy(item => item.Status).ToDictionaryAsync(item => item.Key, item => item.Count());
        }
    }
}
