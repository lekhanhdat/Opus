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
using AvePoint.RA.Contract.FileSystemRegister.JPMC;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class FSConnectionRelatedJobInfoDao : BaseDao<FSConnectionRelatedJobInfo>, IFSConnectionRelatedJobInfoDao
    {
        public async Task AddOrUpdateRelatedJobAsync(FSConnectionRelatedJobInfo job)
        {
            if (job == null) throw new ArgumentNullException(nameof(job));

            using var context = RMDBContextManager.GetNewDBContext();
            if(string.IsNullOrEmpty(job.ConnectionGroupName))
            {
                var groupName = await context.FSConnectionGroup
                    .Where(j => j.Id == job.ConnectionGroupId)
                    .Select(g => g.Name)
                    .FirstOrDefaultAsync();
                job.ConnectionGroupName = groupName;
            }
            context.FSConnectionRelatedJobInfoes.AddOrUpdate(job);
            await context.SaveChangesAsync();
        }

        public async Task<List<string>> GetAllConnGroupNameByConnectionIDAsync(Guid connectionId)
        {
            if (connectionId == Guid.Empty) throw new ArgumentException("Connection ID cannot be empty.", nameof(connectionId));
           
            using var ctx = RMDBContextManager.GetNewDBContext();
            
            var baseQuery = from conn in ctx.FSConnectionRelatedJobInfoes
                            join monitor in ctx.JobMonitors
                            on conn.JobId equals monitor.Id
                            where conn.ConnectionId == connectionId
                            select conn.ConnectionGroupName;
            
            return await baseQuery.Distinct().ToListAsync();
        }

        public async Task<List<string>> GetAllConnPathByConnectionIdAsync(Guid connectionId)
        {
            if (connectionId == Guid.Empty)
                throw new ArgumentException("Connection ID cannot be empty.", nameof(connectionId));

            using var ctx = RMDBContextManager.GetNewDBContext();

            var result = await (
                from conn in ctx.FSConnectionRelatedJobInfoes
                join job in ctx.JobMonitors
                on conn.JobId equals job.Id
                where conn.ConnectionId == connectionId
                select new
                {
                    conn.ConnectionPath,
                    conn.FolderPath
                }).ToListAsync();

            return result.SelectMany(x => new[]
                {
                    x.ConnectionPath,
                    x.FolderPath
                })
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();
        }

        public async Task<bool> UpdateRelatedJobExecutionInfoAsync(string mainJobId)
        {
            using var context = RMDBContextManager.GetNewDBContext();

            var jobMonitor = await context.JobMonitors.FirstOrDefaultAsync(j => j.Id == mainJobId);

            if (jobMonitor == null) return false;

            var connRelatedJobs = await context.FSConnectionRelatedJobInfoes.Where(j => j.JobId == mainJobId).ToListAsync();

            if (connRelatedJobs.Count == 0) return false;

            const int batchSize = 100;

            for (int i = 0; i < connRelatedJobs.Count; i += batchSize)
            {
                int end = Math.Min(i + batchSize, connRelatedJobs.Count);

                for (int j = i; j < end; j++)
                {
                    var job = connRelatedJobs[j];
                    job.JobRunBy = jobMonitor.UserName;
                    job.StartTime = jobMonitor.StartTime;
                    job.EndTime = jobMonitor.EndTime;
                }
                await context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<(int totalCount, List<FSConnectionRelatedJobInfo>)> QueryConnectionMonitorPagerAsync(Expression<Func<FSConnectionRelatedJobInfo, bool>> whereLambda, FSConnectionMonitorQueryPager pager)
        {
            var defaultResult = (0, new List<FSConnectionRelatedJobInfo>());

            var sortDirection = pager.Order.IsDesc ? SortDirectionEnum.Descending : SortDirectionEnum.Ascending;

            using var ctx = RMDBContextManager.GetNewDBContext();

            var baseQuery = from conn in ctx.FSConnectionRelatedJobInfoes
                            join monitor in ctx.JobMonitors
                            on conn.JobId equals monitor.Id 
                            where monitor.EndTime > 0
                            select conn;

            if (whereLambda != null)
                baseQuery = baseQuery.Where(whereLambda);

            int totalCount = await baseQuery.CountAsync();

            if (totalCount == 0) return defaultResult;

            var finalQuery = baseQuery.SortBy(pager.Order.ColumnName, sortDirection);

            if (pager.PageIndex < 0 || pager.PageSize < 0)
            {
                return defaultResult;
            }

            var data = await finalQuery.Skip(pager.PageIndex * pager.PageSize).Take(pager.PageSize).ToListAsync();

            return (totalCount, data);
        }
    }
}
