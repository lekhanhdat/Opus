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
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model.ArchivedFullTextIndex;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.ArchivedFullTextIndex.Impl
{
    public class RMArchivedFullTextIndexDao : IRMArchivedFullTextIndexDao
    {
        private const int QueryBatchSize = 1000;

        public async Task AddOrUpdateLatestSyncTimeAsync(long latestSyncTime)
        {
            var functionSettingDao = PlatformWindsorManager.GetService<IRMFunctionSettingDao>();
            await functionSettingDao.AddOrUpdateSettingInfoAsync(FunctionSettingType.ArchivedDataFullTextIndexLatestSyncTime, latestSyncTime.ToString());
        }

        public async Task<List<RMArchivedDataFullTextIndexSiteInfoesV1>> GetSiteInfoesBySiteUrlsV1Async(IEnumerable<string> siteUrls)
        {
            var result = new List<RMArchivedDataFullTextIndexSiteInfoesV1>();
            var urlList = siteUrls?.Where(url => !string.IsNullOrWhiteSpace(url)).Distinct().ToList() ?? new List<string>();
            if (urlList.Count == 0)
            {
                return result;
            }

            using var context = RMDBContextManager.GetNewDBContext();
            foreach (var batch in BatchStringList(urlList, QueryBatchSize))
            {
                var batchResult = await context.FullTextIndexSiteInfoesV1
                    .AsNoTracking()
                    .Where(item => batch.Contains(item.SiteUrl))
                    .ToListAsync();
                result.AddRange(batchResult);
            }

            return result;
        }

        public async Task<(long MinArchiverTime, long MaxArchiverTime)> GetMinMaxArchiverTimeBySiteUrlsAsync(IEnumerable<string> siteUrls)
        {
            var urlList = siteUrls?.Where(url => !string.IsNullOrWhiteSpace(url)).Distinct().ToList() ?? new List<string>();
            long? min = null;
            long? max = null;

            if (urlList.Count == 0)
            {
                using var context = RMDBContextManager.GetNewDBContext();
                var result = await context.FullTextIndexSiteInfoes
                    .Where(item => item.MinArchiverTime != 0 || item.MaxArchiverTime != 0)
                    .GroupBy(_ => 1)
                    .Select(group => new
                    {
                        Min = (long?)group.Min(item => item.MinArchiverTime),
                        Max = (long?)group.Max(item => item.MaxArchiverTime)
                    })
                    .FirstOrDefaultAsync();

                return (result?.Min ?? 0, result?.Max ?? 0);
            }

            using var dbContext = RMDBContextManager.GetNewDBContext();
            foreach (var batch in BatchStringList(urlList, QueryBatchSize))
            {
                var batchResult = await dbContext.FullTextIndexSiteInfoes
                    .AsNoTracking()
                    .Where(item => batch.Contains(item.SiteUrl))
                    .Where(item => item.MinArchiverTime != 0 || item.MaxArchiverTime != 0)
                    .GroupBy(_ => 1)
                    .Select(group => new
                    {
                        Min = (long?)group.Min(item => item.MinArchiverTime),
                        Max = (long?)group.Max(item => item.MaxArchiverTime)
                    })
                    .FirstOrDefaultAsync();

                if (batchResult == null)
                {
                    continue;
                }

                if (batchResult.Min.HasValue)
                {
                    min = min.HasValue ? Math.Min(min.Value, batchResult.Min.Value) : batchResult.Min.Value;
                }

                if (batchResult.Max.HasValue)
                {
                    max = max.HasValue ? Math.Max(max.Value, batchResult.Max.Value) : batchResult.Max.Value;
                }
            }

            return (min ?? 0, max ?? 0);
        }

        public async Task<(long MinArchiverTime, long MaxArchiverTime)> GetMinMaxArchiverTimeBySiteUrlsV1Async(IEnumerable<string> siteUrls, bool isBlacklistMode = false)
        {
            var urlList = siteUrls?.Where(url => !string.IsNullOrWhiteSpace(url)).Distinct().ToList() ?? new List<string>();
            if (urlList.Count == 0)
            {
                using var context = RMDBContextManager.GetNewDBContext();
                var result = await context.FullTextIndexSiteInfoesV1
                    .Where(item => item.MinArchiverTime != 0 || item.MaxArchiverTime != 0)
                    .GroupBy(_ => 1)
                    .Select(group => new
                    {
                        Min = (long?)group.Min(item => item.MinArchiverTime),
                        Max = (long?)group.Max(item => item.MaxArchiverTime)
                    })
                    .FirstOrDefaultAsync();

                return (result?.Min ?? 0, result?.Max ?? 0);
            }

            if (isBlacklistMode)
            {
                using var context = RMDBContextManager.GetNewDBContext();
                var query = context.FullTextIndexSiteInfoesV1.AsQueryable();
                foreach (var batch in BatchStringList(urlList, QueryBatchSize))
                {
                    query = query.Where(item => !batch.Contains(item.SiteUrl));
                }

                var result = await query
                    .AsNoTracking()
                    .Where(item => item.MinArchiverTime != 0 || item.MaxArchiverTime != 0)
                    .GroupBy(_ => 1)
                    .Select(group => new
                    {
                        Min = (long?)group.Min(item => item.MinArchiverTime),
                        Max = (long?)group.Max(item => item.MaxArchiverTime)
                    })
                    .FirstOrDefaultAsync();

                return (result?.Min ?? 0, result?.Max ?? 0);
            }

            long? min = null;
            long? max = null;

            using var dbContext = RMDBContextManager.GetNewDBContext();
            foreach (var batch in BatchStringList(urlList, QueryBatchSize))
            {
                var batchResult = await dbContext.FullTextIndexSiteInfoesV1
                    .AsNoTracking()
                    .Where(item => batch.Contains(item.SiteUrl))
                    .Where(item => item.MinArchiverTime != 0 || item.MaxArchiverTime != 0)
                    .GroupBy(_ => 1)
                    .Select(group => new
                    {
                        Min = (long?)group.Min(item => item.MinArchiverTime),
                        Max = (long?)group.Max(item => item.MaxArchiverTime)
                    })
                    .FirstOrDefaultAsync();

                if (batchResult == null)
                {
                    continue;
                }

                if (batchResult.Min.HasValue)
                {
                    min = min.HasValue ? Math.Min(min.Value, batchResult.Min.Value) : batchResult.Min.Value;
                }

                if (batchResult.Max.HasValue)
                {
                    max = max.HasValue ? Math.Max(max.Value, batchResult.Max.Value) : batchResult.Max.Value;
                }
            }

            return (min ?? 0, max ?? 0);
        }

        public async Task<List<RMArchivedDataFullTextIndexJobInfoesV1>> GetSiteJobInfoesV1(long siteId, params JobStatus[] status)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var statusList = status.ToList();
            var hasAny = status.Any();
            var res = await context.FullTextIndexJobInfoesV1.Where(item =>
                item.FullTextIndexSiteId == siteId &&
                (!hasAny || statusList.Contains(item.Status))).ToListAsync();
            return res;
        }

        public async Task<List<RMArchivedDataFullTextIndexJobInfoesV1>> GetJobInfoesBySiteUrlsV1Async(IEnumerable<string> siteUrls, params JobStatus[] status)
        {
            var result = new List<RMArchivedDataFullTextIndexJobInfoesV1>();
            var urlList = siteUrls?.Where(url => !string.IsNullOrWhiteSpace(url)).Distinct().ToList() ?? new List<string>();
            if (urlList.Count == 0)
            {
                return result;
            }

            var statusList = status.ToList();
            var hasAny = status.Any();
            using var context = RMDBContextManager.GetNewDBContext();
            foreach (var batch in BatchStringList(urlList, QueryBatchSize))
            {
                var batchResult = await context.FullTextIndexJobInfoesV1
                    .AsNoTracking()
                    .Where(item => batch.Contains(item.SiteUrl) && (!hasAny || statusList.Contains(item.Status)))
                    .ToListAsync();
                result.AddRange(batchResult);
            }

            return result;
        }

        public async Task<List<RMArchivedDataFullTextIndexJobInfoesV1>> GetJobInfoesBySiteIdsV1Async(IEnumerable<long> siteIds, params JobStatus[] status)
        {
            var result = new List<RMArchivedDataFullTextIndexJobInfoesV1>();
            var idList = siteIds?.Distinct().ToList() ?? new List<long>();
            if (idList.Count == 0)
            {
                return result;
            }

            var statusList = status.ToList();
            var hasAny = status.Any();
            using var dbContext = RMDBContextManager.GetNewDBContext();
            foreach (var batch in BatchLongList(idList, QueryBatchSize))
            {
                var batchResult = await dbContext.FullTextIndexJobInfoesV1
                    .AsNoTracking()
                    .Where(item => batch.Contains(item.FullTextIndexSiteId) && (!hasAny || statusList.Contains(item.Status)))
                    .ToListAsync();
                result.AddRange(batchResult);
            }

            return result;
        }

        private static IEnumerable<List<string>> BatchStringList(List<string> items, int batchSize)
        {
            for (var i = 0; i < items.Count; i += batchSize)
            {
                yield return items.Skip(i).Take(batchSize).ToList();
            }
        }

        private static IEnumerable<List<long>> BatchLongList(List<long> items, int batchSize)
        {
            for (var i = 0; i < items.Count; i += batchSize)
            {
                yield return items.Skip(i).Take(batchSize).ToList();
            }
        }

        public async Task<(bool Has, RMArchivedDataFullTextIndexSiteInfoesV1 SiteInfo)> TryGetSiteInfoV1Async(string siteUrl)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var res = await context.FullTextIndexSiteInfoesV1.FirstOrDefaultAsync(item => item.SiteUrl == siteUrl);
            return (res != null, res);
        }

        public async Task AddOrUpdateSiteInfoAsync(RMArchivedDataFullTextIndexSiteInfoesV1 siteInfo)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            context.FullTextIndexSiteInfoesV1.AddOrUpdate(siteInfo);
            await context.SaveChangesAsync();
        }

        public async Task<(bool Has, RMArchivedDataFullTextIndexJobInfoesV1 JobInfo)> TryGetJobInfoV1Async(string archiverSubJobId)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var res = await context.FullTextIndexJobInfoesV1.FirstOrDefaultAsync(item => item.ArchiverJobId == archiverSubJobId);
            return (res != null, res);
        }

        public async Task<RMArchivedDataFullTextIndexJobInfoesV1> GetJobInfoByIdV1Async(long id)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var res = await context.FullTextIndexJobInfoesV1.FirstAsync(item => item.Id == id);
            return res;
        }

        public async Task AddOrUpdateJobInfoAsync(params RMArchivedDataFullTextIndexJobInfoesV1[] jobInfoes)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            context.FullTextIndexJobInfoesV1.AddOrUpdate(jobInfoes);
            await context.SaveChangesAsync();
        }

        public async Task AddOrUpdateEDiscoveryJobInfoAsync(params RMArchivedDataFullTextIndexEDiscoveryJobInfoesV1[] jobInfoes)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            context.FullTextIndexEDiscoveryJobInfoesV1.AddOrUpdate(jobInfoes);
            await context.SaveChangesAsync();
        }

        public async Task<long> GetSiteLatestArchivedTimeAsync(string siteUrl)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var jobInfo = await context.FullTextIndexJobInfoes.Where(item => item.SiteUrl == siteUrl)
                .OrderByDescending(item => item.ArchiverTime)
                .FirstOrDefaultAsync();
            return jobInfo?.ArchiverTime ?? DateTime.UtcNow.Ticks;
        }

        public async Task<long> GetSiteLatestArchivedTimeV1Async(string siteUrl)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var jobInfo = await context.FullTextIndexJobInfoesV1.Where(item => item.SiteUrl == siteUrl)
                .OrderByDescending(item => item.ArchiverTime)
                .FirstOrDefaultAsync();
            return jobInfo?.ArchiverTime ?? DateTime.UtcNow.Ticks;
        }

        public async Task<long> GetLatestArchivedTimeAsync()
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var jobInfo = await context.FullTextIndexJobInfoes
                .OrderByDescending(item => item.ArchiverTime)
                .FirstOrDefaultAsync();
            return jobInfo?.ArchiverTime ?? DateTime.UtcNow.Ticks;
        }

        public async Task<long> GetLatestArchivedTimeV1Async()
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var jobInfo = await context.FullTextIndexJobInfoesV1
                .OrderByDescending(item => item.ArchiverTime)
                .FirstOrDefaultAsync();
            return jobInfo?.ArchiverTime ?? DateTime.UtcNow.Ticks;
        }
    }
}
