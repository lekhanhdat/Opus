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
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model.DataIngestion;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.DataIngestion.Impl
{
    public class RMDataIngestionJobDao : IRMDataIngestionJobDao
    {
        public async Task AddOrUpdateAsync(RMDataIngestionJob job)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            context.DataIngestionJobs.AddOrUpdate(job);
            await context.SaveChangesAsync();
        }

        public async Task AddOrUpdateAsync(IEnumerable<RMDataIngestionJob> jobs)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            context.DataIngestionJobs.AddOrUpdate(jobs.ToArray());
            await context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(string id)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var job = await context.DataIngestionJobs.FirstOrDefaultAsync(item => item.Id == id);
            if (job == null)
            {
                return false;
            }

            context.DataIngestionJobs.Remove(job);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<RMDataIngestionJob> GetByIdAsync(string id)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return await context.DataIngestionJobs.FirstOrDefaultAsync(item => item.Id == id);
        }

        public async Task<bool> IsJobRunning(RMDataIngestionType type)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return await context.DataIngestionJobs.AnyAsync(item => item.IngestionType == type && (item.Status == JobStatus.Wait || item.Status == JobStatus.InProgress));
        }

        public async Task<List<RMDataIngestionJob>> GetByStatusAsync(params JobStatus[] status)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var statusList = status?.ToList() ?? new List<JobStatus>();
            var hasStatus = statusList.Any();
            return await context.DataIngestionJobs
                .Where(item => !hasStatus || statusList.Contains(item.Status))
                .ToListAsync();
        }

        public async Task<bool> UpdateStatusAsync(string id, JobStatus status, long modifiedTime)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var job = await context.DataIngestionJobs.FirstOrDefaultAsync(item => item.Id == id);
            if (job == null)
            {
                return false;
            }

            job.Status = status;
            job.ModifiedTime = modifiedTime;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<int> UpdateStatusToTimeoutAsync(long modifiedTimeUpperBound)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var jobs = await context.DataIngestionJobs
                .Where(item => item.ModifiedTime < modifiedTimeUpperBound && item.Status != JobStatus.Timeout)
                .ToListAsync();

            if (!jobs.Any())
            {
                return 0;
            }

            foreach (var job in jobs)
            {
                job.Status = JobStatus.Timeout;
            }

            await context.SaveChangesAsync();
            return jobs.Count;
        }

        public async Task<bool> UpdateModifiedTimeAsync(string id, long modifiedTime)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var job = await context.DataIngestionJobs.FirstOrDefaultAsync(item => item.Id == id);
            if (job == null)
            {
                return false;
            }

            job.ModifiedTime = modifiedTime;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<RMDataIngestionJob> GetExistingJobByUniqueId(string uniqueId)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return await context.DataIngestionJobs.FirstOrDefaultAsync(item => item.UniqueId == uniqueId && (item.Status == JobStatus.InProgress || item.Status == JobStatus.Wait));
        }
    }
}
