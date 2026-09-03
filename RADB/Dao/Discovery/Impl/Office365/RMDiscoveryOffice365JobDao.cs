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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model.Discovery.Office365;
using Cloud.Sdk.Data.Cop.SMP;
using DocumentFormat.OpenXml.Office.CustomUI;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.Office365
{
    public class RMDiscoveryOffice365JobDao : IRMDiscoveryOffice365JobDao
    {

        private static readonly HashSet<RMDiscoveryJobStatus> S_PROCESSING_JOB_STATUS = new()
        {
            RMDiscoveryJobStatus.Preparing,
            RMDiscoveryJobStatus.Waiting,
            RMDiscoveryJobStatus.Pending,
            RMDiscoveryJobStatus.Running,
            RMDiscoveryJobStatus.Completing,
        };

        public async Task<(bool has, RMDiscoveryOffice365MainJob mainJobInfo)> TryGetProcessingMainJobAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var processingJob = await efContext.Office365MainJobs.FirstOrDefaultAsync(item => S_PROCESSING_JOB_STATUS.Contains(item.Status));
            return (processingJob != null, processingJob);
        }

        public async Task<(bool has, RMDiscoveryOffice365MainJob mainJobInfo)> TryGetLatestMainJobAsync(params RMDiscoveryJobType[] types)
        {
            var hasTypes = types.Any();
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var latestJob = await efContext.Office365MainJobs
                .Where(item => (!hasTypes || Enumerable.Contains(types, item.Type)))
                .OrderByDescending(item => item.StartTime).FirstOrDefaultAsync();
            return (latestJob != null, latestJob);
        }

        public async Task<(bool has, RMDiscoveryOffice365MainJob mainJob)> TryGetMainJobAsync(RMDiscoveryJobStatus status)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var processingJob = await efContext.Office365MainJobs.FirstOrDefaultAsync(item => item.Status == status);
            return (processingJob != null, processingJob);
        }

        public async Task<(bool has, RMDiscoveryOffice365MainJob mainJob)> TryGetMainJobAsync(Guid id)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var job = await efContext.Office365MainJobs.FirstOrDefaultAsync(item => item.Id == id);
            if (job != null && job.Type == RMDiscoveryJobType.None)
            {
                job.Type = RMDiscoveryJobType.Newly;
            }
            return (job != null, job);
        }

        public async Task AddOrUpdateMainJobAsync(RMDiscoveryOffice365MainJob mainJobInfo)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            mainJobInfo.LastModifiedTime = DateTime.UtcNow.Ticks;
            efContext.Office365MainJobs.AddOrUpdate(mainJobInfo);
            await efContext.SaveChangesAsync();
        }

        public async Task AddOrUpdateDiscoveryJobAsync(params RMDiscoveryOffice365DiscoveryJob[] discoveryJobs)
        {
            if (!discoveryJobs.Any())
            {
                return;
            }
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            efContext.Office365DiscoveryJobs.AddOrUpdate(discoveryJobs);
            await efContext.SaveChangesAsync();
        }

        public async Task AddOrUpdateAnalysisJobAsync(params RMDiscoveryOffice365AnalysisJob[] analysisJobs)
        {
            if (!analysisJobs.Any())
            {
                return;
            }
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            efContext.Office365AnalysisJobs.AddOrUpdate(analysisJobs);
            await efContext.SaveChangesAsync();
        }

        public async Task BatchInsertAnalysisJobAsync(List<RMDiscoveryOffice365AnalysisJob> analysisJobs)
        {
            if (analysisJobs?.Count == 0)
            {
                return;
            }

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteInsertAsync(analysisJobs);
        }

        public async Task<List<RMDiscoveryOffice365AnalysisJob>> GetTimeoutAnalysisJobsAsync(Guid mainJobId, RMDiscoveryJobStatus status, long timeout)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();

            return await efContext.Office365AnalysisJobs.Where(item =>
                item.MainJobId == mainJobId &&
                item.Status == status &&
                item.LastModifiedTime < timeout
            ).ToListAsync();
        }

        public async Task<bool> HasProcessingDiscoveryJobAsync(Guid mainJobId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.Office365DiscoveryJobs.AnyAsync(item => item.MainJobId == mainJobId && S_PROCESSING_JOB_STATUS.Contains(item.Status));
        }

        public async Task<bool> HasDiscoveryJobAsync(Guid mainJobId, params RMDiscoveryJobStatus[] jobStatus)
        {
            var jobStatusList = jobStatus.ToList();
            var hasAny = jobStatus.Any();
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.Office365DiscoveryJobs.AnyAsync(item => item.MainJobId == mainJobId && jobStatusList.Contains(item.Status));
        }

        public async Task<List<RMDiscoveryOffice365DiscoveryJob>> GetDiscoveryJobsAsync(Guid mainJobId, params RMDiscoveryJobStatus[] status)
        {
            var hasStatus = status.Any();
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.Office365DiscoveryJobs.Where(item => item.MainJobId == mainJobId && (!hasStatus || Enumerable.Contains(status, item.Status))).ToListAsync();
        }

        public async Task<List<RMDiscoveryOffice365MainJob>> GetDiscoveryJobsHangingAsync(long threshold)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();

            var runningJobs = efContext.Office365MainJobs
                .Where(x => x.Status == RMDiscoveryJobStatus.Running);

            var analysisJobs = efContext.Office365AnalysisJobs;

            return await runningJobs
                .Where(job =>
                    analysisJobs.Any(subJob =>
                        subJob.MainJobId == job.Id &&
                        subJob.Status != RMDiscoveryJobStatus.Finished
                    )
                )
                .Where(job =>
                    !analysisJobs.Any(subJob =>
                        subJob.MainJobId == job.Id &&
                        subJob.Status != RMDiscoveryJobStatus.Finished &&
                        subJob.LastModifiedTime > threshold
                    )
                )
                .ToListAsync();
        }

        public async Task<RMDiscoveryOffice365DiscoveryJob> GetDiscoveryJobAsync(Guid id)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.Office365DiscoveryJobs.FirstAsync(item => item.Id == id);
        }

        public async Task<Dictionary<RMDiscoveryJobStatus, int>> GetDiscoveryCompletedStatusAsync(Guid mainJobId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.Office365DiscoveryJobs.Where(item => item.MainJobId == mainJobId).GroupBy(item => item.Status).ToDictionaryAsync(item => item.Key, item => item.Count());
        }

        public async Task<List<RMDiscoveryOffice365AnalysisJob>> GetAnalysisJobsAsync(Guid mainJobId, int count, params RMDiscoveryJobStatus[] status)
        {
            var hasStatus = status.Any();
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.Office365AnalysisJobs.Where(item => item.MainJobId == mainJobId && (!hasStatus || Enumerable.Contains(status, item.Status)))
                .OrderByDescending(item => item.LastModifiedTime)
                .Take(count)
                .ToListAsync();
        }

        public async IAsyncEnumerable<RMDiscoveryOffice365AnalysisJob> GetAnalysisJobsWithPaginationAsync(Guid mainJobId, int pageSize, params RMDiscoveryJobStatus[] status)
        {
            var hasStatus = status.Any();
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            for (var i = 0; ; i++)
            {
                var jobs = await efContext.Office365AnalysisJobs.Where(item => item.MainJobId == mainJobId && (!hasStatus || Enumerable.Contains(status, item.Status)))
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

        public async IAsyncEnumerable<RMDiscoveryOffice365AnalysisJob> GetAnalysisJobsByDiscoveryJobWithPaginationAsync(Guid discoveryJobId, int pageSize, params RMDiscoveryJobStatus[] status)
        {
            var hasStatus = status.Any();
            for (var i = 0; ; i++)
            {
                using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
                var jobs = await efContext.Office365AnalysisJobs.Where(item => item.DiscoveryJobId == discoveryJobId)
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

        public async IAsyncEnumerable<RMDiscoveryOffice365AnalysisJob> GetAnalysisJobReportWithPaginationAsync(Guid mainJobId, int pageSize)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            for (var i = 0; ; i++)
            {
                var jobs = await efContext.Office365AnalysisJobs.Where(item => item.MainJobId == mainJobId)
                                                    .OrderByDescending(item => item.Status)
                                                    .ThenBy(item => item.Url)
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

        public async Task<int> CountAnalysisJobsByMainJobAsync(Guid mainJobId, params RMDiscoveryJobStatus[] status)
        {
            var hasStatus = status.Any();
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.Office365AnalysisJobs.Where(item => item.MainJobId == mainJobId && (!hasStatus || Enumerable.Contains(status, item.Status)))
                .CountAsync();
        }

        public async Task<(bool has, RMDiscoveryOffice365AnalysisJob analysisJob)> TryGetAnalysisJobAsync(Guid discvoeryJobId, Guid siteId, params RMDiscoveryJobStatus[] status)
        {
            var hasStatus = status.Any();
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var res = await efContext.Office365AnalysisJobs.FirstOrDefaultAsync(item => item.DiscoveryJobId == discvoeryJobId && item.SiteId == siteId && (!hasStatus || Enumerable.Contains(status, item.Status)));
            return (res != null, res);
        }

        public async Task<bool> HasProcessingAnalysisJobAsync(Guid discoveryJobId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.Office365AnalysisJobs.AnyAsync(item => item.DiscoveryJobId == discoveryJobId && S_PROCESSING_JOB_STATUS.Contains(item.Status));
        }

        public async Task<Dictionary<RMDiscoveryJobStatus, int>> GetAnalysisCompletedStatusByMainJobIdAsync(Guid mainJobId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.Office365AnalysisJobs.Where(item => item.MainJobId == mainJobId).GroupBy(item => item.Status).ToDictionaryAsync(item => item.Key, item => item.Count());
        }

        public async Task<Dictionary<RMDiscoveryJobStatus, int>> GetAnalysisCompletedStatusAsync(Guid discoveryJobId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.Office365AnalysisJobs.Where(item => item.DiscoveryJobId == discoveryJobId).GroupBy(item => item.Status).ToDictionaryAsync(item => item.Key, item => item.Count());
        }

        public async Task<int> ChangeAnalysisJobsStatusAsync(Guid discoveryJobId, RMDiscoveryJobStatus willChangeStatus, bool isEnd, RMDiscoveryJobFailedCause failedCause, params RMDiscoveryJobStatus[] beforeStatus)
        {
            var parameters = new List<SqlParameter>();
            var sql = "UPDATE [dbo].[RMAnalysisJobs] SET Status = @Status, FailedCause = @FailedCause";
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
            var sql = $"UPDATE [dbo].[RMAnalysisJobs] SET Status = @Status, FailedCause = @FailedCause WHERE DiscoveryJobId IN {DatabaseUtility.BuildInClause(discoveryJobIds)}";
            var context = await RMDiscoveryDBManager.GetContextAsync();
            var effectCount = await context.ExecuteNonQueryAsync(sql, new SqlParameter("@Status", status), new SqlParameter("@FailedCause", failedCause));
            return effectCount;
        }

        public async Task<RMDiscoveryOffice365AnalysisJob> GetAnalysisJobByIdAsync(Guid analysisJobId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.Office365AnalysisJobs.FirstAsync(item => item.Id == analysisJobId);
        }
    }
}
