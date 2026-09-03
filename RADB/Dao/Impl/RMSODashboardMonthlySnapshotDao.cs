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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMSODashboardMonthlySnapshotDao : BaseDao<RMSODashboardMonthlySnapshot>, IRMSODashboardMonthlySnapshotDao
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMSODashboardMonthlySnapshotDao));

        public async Task<List<RMSODashboardMonthlySnapshot>> GetByStartPeriodAsync(string startPeriod)
        {
            using var context = GetNewContext();
            try
            {
                var monthlyTotals = await BuildQuery(context.RMSODashboardMonthlySnapshots.AsNoTracking(), startPeriod)
                    .GroupBy(snapshot => snapshot.Period)
                    .Select(group => new
                    {
                        Period = group.Key,
                        SpoArchivedSize = group.Sum(snapshot => snapshot.SpoArchivedSize),
                        OdArchivedSize = group.Sum(snapshot => snapshot.OdArchivedSize),
                        SpoDestroyedFromArchiveSize = group.Sum(snapshot => snapshot.SpoDestroyedFromArchiveSize),
                        OdDestroyedFromArchiveSize = group.Sum(snapshot => snapshot.OdDestroyedFromArchiveSize),
                        SpoDestroyedFromLiveSize = group.Sum(snapshot => snapshot.SpoDestroyedFromLiveSize),
                        OdDestroyedFromLiveSize = group.Sum(snapshot => snapshot.OdDestroyedFromLiveSize),
                    })
                    .OrderBy(snapshot => snapshot.Period)
                    .ToListAsync();

                return monthlyTotals.Select(snapshot => new RMSODashboardMonthlySnapshot
                {
                    Period = snapshot.Period,
                    SpoArchivedSize = snapshot.SpoArchivedSize,
                    OdArchivedSize = snapshot.OdArchivedSize,
                    SpoDestroyedFromArchiveSize = snapshot.SpoDestroyedFromArchiveSize,
                    OdDestroyedFromArchiveSize = snapshot.OdDestroyedFromArchiveSize,
                    SpoDestroyedFromLiveSize = snapshot.SpoDestroyedFromLiveSize,
                    OdDestroyedFromLiveSize = snapshot.OdDestroyedFromLiveSize,
                    Id = null,
                    O365TenantId = null,
                    CreatedTime = 0,
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error($"GetByStartPeriodAsync failed, startPeriod: {startPeriod}, exception: {ex}");
                throw;
            }
        }

        public async Task UpsertMonthlySnapshotAsync(
            string tenantId,
            string period,
            long spoArchivedSize,
            long odArchivedSize,
            long spoDestroyedFromArchiveSize,
            long odDestroyedFromArchiveSize,
            long spoDestroyedFromLiveSize,
            long odDestroyedFromLiveSize)
        {
            using var context = this.GetNewContext();
            var existing = await context.RMSODashboardMonthlySnapshots
                .FirstOrDefaultAsync(x => x.O365TenantId == tenantId && x.Period == period);

            if (existing != null)
            {
                existing.SpoArchivedSize += spoArchivedSize;
                existing.OdArchivedSize += odArchivedSize;
                existing.SpoDestroyedFromArchiveSize += spoDestroyedFromArchiveSize;
                existing.OdDestroyedFromArchiveSize += odDestroyedFromArchiveSize;
                existing.SpoDestroyedFromLiveSize += spoDestroyedFromLiveSize;
                existing.OdDestroyedFromLiveSize += odDestroyedFromLiveSize;
                context.RMSODashboardMonthlySnapshots.AddOrUpdate(existing);
            }
            else
            {
                var snapshot = new RMSODashboardMonthlySnapshot
                {
                    Id = Guid.NewGuid().ToString(),
                    O365TenantId = tenantId,
                    Period = period,
                    SpoArchivedSize = spoArchivedSize,
                    OdArchivedSize = odArchivedSize,
                    SpoDestroyedFromArchiveSize = spoDestroyedFromArchiveSize,
                    OdDestroyedFromArchiveSize = odDestroyedFromArchiveSize,
                    SpoDestroyedFromLiveSize = spoDestroyedFromLiveSize,
                    OdDestroyedFromLiveSize = odDestroyedFromLiveSize,
                    CreatedTime = DateTime.UtcNow.Ticks
                };
                context.RMSODashboardMonthlySnapshots.Add(snapshot);
            }

            await context.SaveChangesAsync();
        }

        private IQueryable<RMSODashboardMonthlySnapshot> BuildQuery(IQueryable<RMSODashboardMonthlySnapshot> source, string startPeriod)
        {
            var query = source;
            if (!string.IsNullOrWhiteSpace(startPeriod))
            {
                query = query.Where(x => string.Compare(x.Period, startPeriod) >= 0);
            }

            return query.OrderBy(x => x.Period);
        }
    }
}