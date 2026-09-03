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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Impl.AOSP
{
    public class RMDiscoveryAOSPJobDao : IRMDiscoveryAOSPJobDao
    {
        private static readonly HashSet<RMDiscoveryJobStatus> S_PROCESSING_JOB_STATUS = new()
        {
            RMDiscoveryJobStatus.Preparing,
            RMDiscoveryJobStatus.Waiting,
            RMDiscoveryJobStatus.Pending,
            RMDiscoveryJobStatus.Running,
            RMDiscoveryJobStatus.Completing,
        };

        public async Task<(bool has, RMDiscoveryAOSPMainJob mainJobInfo)> TryGetLatestMainJobAsync(string o365TenantId, params RMDiscoveryJobType[] types)
        {
            var hasTypes = types.Any();
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var latestJob = await efContext.AOSPMainJobs
                .Where(item => (!hasTypes || Enumerable.Contains(types, item.Type)) && item.O365TenantId == o365TenantId)
                .OrderByDescending(item => item.StartTime).FirstOrDefaultAsync();
            return (latestJob != null, latestJob);
        }

        public async Task<Dictionary<RMDiscoveryJobStatus, int>> GetAnalysisCompletedStatusByMainJobIdAsync(Guid mainJobId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.AOSPAnalysisJobs.Where(item => item.MainJobId == mainJobId).GroupBy(item => item.Status).ToDictionaryAsync(item => item.Key, item => item.Count());
        }

        public async Task<(bool has, RMDiscoveryAOSPMainJob mainJobInfo)> TryGetProcessingMainJobAsync()
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var processingJob = await efContext.AOSPMainJobs.FirstOrDefaultAsync(item => S_PROCESSING_JOB_STATUS.Contains(item.Status));
            return (processingJob != null, processingJob);
        }

        public async Task<(bool has, RMDiscoveryAOSPMainJob mainJobInfo)> TryGetProcessingMainJobAsync(string o365TenantId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var processingJob = await efContext.AOSPMainJobs.FirstOrDefaultAsync(item => S_PROCESSING_JOB_STATUS.Contains(item.Status) && o365TenantId == item.O365TenantId);
            return (processingJob != null, processingJob);
        }

        public async Task AddOrUpdateMainJobAsync(RMDiscoveryAOSPMainJob mainJobInfo)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            mainJobInfo.LastModifiedTime = DateTime.UtcNow.Ticks;
            efContext.AOSPMainJobs.AddOrUpdate(mainJobInfo);
            await efContext.SaveChangesAsync();
        }

        public async Task<(bool has, RMDiscoveryAOSPMainJob mainJob)> TryGetMainJobAsync(RMDiscoveryJobStatus status)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var processingJob = await efContext.AOSPMainJobs.FirstOrDefaultAsync(item => item.Status == status);
            return (processingJob != null, processingJob);
        }

        public async Task<(bool has, List<RMDiscoveryAOSPMainJob> mainJobs)> TryGetMainJobsAsync(RMDiscoveryJobStatus status)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var processingJobs= await efContext.AOSPMainJobs.Where(item => item.Status == status).ToListAsync();
            return (processingJobs != null && processingJobs.Count != 0, processingJobs);
        }

        public async Task<(bool has, RMDiscoveryAOSPMainJob mainJob)> TryGetMainJobAsync(Guid id)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var job = await efContext.AOSPMainJobs.FirstOrDefaultAsync(item => item.Id == id);
            if (job != null && job.Type == RMDiscoveryJobType.None)
            {
                job.Type = RMDiscoveryJobType.Newly;
            }
            return (job != null, job);
        }

        public async Task AddOrUpdateDiscoveryJobAsync(params RMDiscoveryAOSPDiscoveryJob[] discoveryJobs)
        {
            if (!discoveryJobs.Any())
            {
                return;
            }
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            efContext.AOSPDiscoveryJobs.AddOrUpdate(discoveryJobs);
            await efContext.SaveChangesAsync();
        }


        public async Task BatchInsertAnalysisJobAsync(List<RMDiscoveryAOSPAnalysisJob> analysisJobs)
        {
            if (analysisJobs?.Count == 0)
            {
                return;
            }

            await using var context = await RMDiscoveryDBManager.GetContextAsync();
            await context.ExecuteInsertAsync(analysisJobs);
        }

        public async Task<bool> HasDiscoveryJobAsync(Guid mainJobId, params RMDiscoveryJobStatus[] jobStatus)
        {
            var jobStatusList = jobStatus.ToList();
            var hasAny = jobStatus.Any();
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.AOSPDiscoveryJobs.AnyAsync(item => item.MainJobId == mainJobId && jobStatusList.Contains(item.Status));
        }

        public async Task<List<RMDiscoveryAOSPAnalysisJob>> GetTimeoutAnalysisJobsAsync(Guid mainJobId, RMDiscoveryJobStatus status, long timeout)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();

            return await efContext.AOSPAnalysisJobs.Where(item =>
                item.MainJobId == mainJobId &&
                item.Status == status &&
                item.LastModifiedTime < timeout
            ).ToListAsync();
        }

        public async Task AddOrUpdateAnalysisJobAsync(params RMDiscoveryAOSPAnalysisJob[] analysisJobs)
        {
            if (!analysisJobs.Any())
            {
                return;
            }
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            efContext.AOSPAnalysisJobs.AddOrUpdate(analysisJobs);
            await efContext.SaveChangesAsync();
        }

        public async Task<List<RMDiscoveryAOSPDiscoveryJob>> GetDiscoveryJobsAsync(Guid mainJobId, params RMDiscoveryJobStatus[] status)
        {
            var hasStatus = status.Any();
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.AOSPDiscoveryJobs.Where(item => item.MainJobId == mainJobId && (!hasStatus || Enumerable.Contains(status, item.Status))).ToListAsync();
        }

        public async Task<bool> HasProcessingAnalysisJobAsync(Guid discoveryJobId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.AOSPAnalysisJobs.AnyAsync(item => item.DiscoveryJobId == discoveryJobId && S_PROCESSING_JOB_STATUS.Contains(item.Status));
        }

        public async Task<Dictionary<RMDiscoveryJobStatus, int>> GetAnalysisCompletedStatusAsync(Guid discoveryJobId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.AOSPAnalysisJobs.Where(item => item.DiscoveryJobId == discoveryJobId).GroupBy(item => item.Status).ToDictionaryAsync(item => item.Key, item => item.Count());
        }

        public async Task<string> GetAnalysisErrorCommentLatestAsync(Guid discoveryJobId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var analysisFailedLastJob = await efContext.AOSPAnalysisJobs.Where(item => item.DiscoveryJobId == discoveryJobId && item.Status == RMDiscoveryJobStatus.Failed).OrderByDescending(_ => _.EndTime).FirstOrDefaultAsync();
            return analysisFailedLastJob?.Comment ?? string.Empty;
        }

        public async Task<bool> HasProcessingDiscoveryJobAsync(Guid mainJobId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.AOSPDiscoveryJobs.AnyAsync(item => item.MainJobId == mainJobId && S_PROCESSING_JOB_STATUS.Contains(item.Status));
        }

        public async Task<(bool has, RMDiscoveryAOSPAnalysisJob analysisJob)> TryGetAnalysisJobAsync(Guid discvoeryJobId, Guid siteId, params RMDiscoveryJobStatus[] status)
        {
            var hasStatus = status.Any();
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var res = await efContext.AOSPAnalysisJobs.FirstOrDefaultAsync(item => item.DiscoveryJobId == discvoeryJobId && item.SiteId == siteId && (!hasStatus || Enumerable.Contains(status, item.Status)));
            return (res != null, res);
        }

        public async Task<int> ChangeAnalysisJobsStatusAsync(Guid discoveryJobId, RMDiscoveryJobStatus willChangeStatus, bool isEnd, RMDiscoveryJobFailedCause failedCause, params RMDiscoveryJobStatus[] beforeStatus)
        {
            var parameters = new List<SqlParameter>();
            var sql = "UPDATE [dbo].[RMAOSPAnalysisJobs] SET Status = @Status, FailedCause = @FailedCause";
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

        public async Task<int> ChangeAnalysisJobsStatusAsync(RMDiscoveryJobStatus status, RMDiscoveryJobFailedCause failedCause, string comment, params Guid[] discoveryJobIds)
        {
            if (!discoveryJobIds.Any())
            {
                return 0;
            }
            var sql = $"UPDATE [dbo].[RMAOSPAnalysisJobs] SET Status = @Status, FailedCause = @FailedCause, Comment = @Comment WHERE DiscoveryJobId IN {DatabaseUtility.BuildInClause(discoveryJobIds)}";
            var context = await RMDiscoveryDBManager.GetContextAsync();
            var effectCount = await context.ExecuteNonQueryAsync(sql, new SqlParameter("@Status", status), new SqlParameter("@FailedCause", failedCause), new SqlParameter("@Comment", comment));
            return effectCount;
        }

        public async Task<RMDiscoveryAOSPAnalysisJob> GetAnalysisJobByIdAsync(Guid analysisJobId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.AOSPAnalysisJobs.FirstAsync(item => item.Id == analysisJobId);
        }

        public async IAsyncEnumerable<RMDiscoveryAOSPAnalysisJob> GetAnalysisJobsByDiscoveryJobWithPaginationAsync(Guid discoveryJobId, int pageSize, params RMDiscoveryJobStatus[] status)
        {
            var hasStatus = status.Any();
            for (var i = 0; ; i++)
            {
                using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
                var jobs = await efContext.AOSPAnalysisJobs.Where(item => item.DiscoveryJobId == discoveryJobId)
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

        public async IAsyncEnumerable<RMDiscoveryAOSPAnalysisJob> GetAnalysisJobsWithPaginationAsync(Guid mainJobId, int pageSize, params RMDiscoveryJobStatus[] status)
        {
            var hasStatus = status.Any();
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            for (var i = 0; ; i++)
            {
                var jobs = await efContext.AOSPAnalysisJobs.Where(item => item.MainJobId == mainJobId && (!hasStatus || Enumerable.Contains(status, item.Status)))
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

        public async Task<Dictionary<RMDiscoveryJobStatus, int>> GetDiscoveryCompletedStatusAsync(Guid mainJobId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            return await efContext.AOSPDiscoveryJobs.Where(item => item.MainJobId == mainJobId).GroupBy(item => item.Status).ToDictionaryAsync(item => item.Key, item => item.Count());
        }

        public async Task<string> GetDiscoveryErrorCommentLatest(Guid mainJobId)
        {
            using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
            var discoveryFailedLastJob = await efContext.AOSPDiscoveryJobs.Where(item => item.MainJobId == mainJobId && (item.Status == RMDiscoveryJobStatus.Failed || item.Status == RMDiscoveryJobStatus.Exception)).OrderByDescending(_ => _.EndTime).FirstOrDefaultAsync();
            return discoveryFailedLastJob?.Comment ?? string.Empty;
        }
    }
}
