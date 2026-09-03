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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class ArchiverDedupInfoDao : BaseDao<ArchiverDedupInfo>, IArchiverDedupInfoDao
    {
        private static IRALogger logger = new RALogger(typeof(ArchiverDedupInfoDao));

        public List<string> GetDedupSiteCollections(long dedupFrom, long dedupTo)
        {
            using (var context = GetNewContext())
            {
                return context.ArchiverDedupInfoes.AsNoTracking()
                    .Where(d => d.FirstDedupTime <= dedupTo && d.LastDedupTime >= dedupFrom)
                    .Select(d => d.SiteUrl)
                    .ToList();
            }
        }

        public List<string> GetAllDedupCollections()
        {
            using (var context = GetNewContext())
            {
                return context.ArchiverDedupInfoes.AsNoTracking()
                    .Select(d => d.SiteUrl)
                    .ToList();
            }
        }

        public void UpsertArchiverDedupInfo(string siteUrl, long minDedupTime, long maxDedupTime)
        {
            using (var context = GetNewContext())
            {
                var existsInfo = context.ArchiverDedupInfoes.AsQueryable()
                    .FirstOrDefault(d => d.SiteUrl == siteUrl);
                if(existsInfo != null)
                {
                    logger.Info($"PrevMinDedupTime: {existsInfo.FirstDedupTime}, SetMinDedupTime: {minDedupTime}, PrevMaxDedupTime: {existsInfo.LastDedupTime}, SetMaxDedupTime: {maxDedupTime}");
                    minDedupTime = Math.Min(existsInfo.FirstDedupTime, minDedupTime);
                    maxDedupTime = Math.Max(existsInfo.LastDedupTime, maxDedupTime);

                    existsInfo.FirstDedupTime = minDedupTime;
                    existsInfo.LastDedupTime = maxDedupTime;
                    existsInfo.Modified = DateTime.UtcNow.Ticks;
                    context.SaveChanges();
                }
                else
                {
                    logger.Info($"SetMinDedupTime: {minDedupTime}, SetMaxDedupTime: {maxDedupTime}");
                    context.ArchiverDedupInfoes.Add(new ArchiverDedupInfo()
                    {
                        Id = Guid.NewGuid(),
                        SiteUrl = siteUrl,
                        FirstDedupTime = minDedupTime,
                        LastDedupTime = maxDedupTime,
                        Created = DateTime.UtcNow.Ticks,
                        Modified = DateTime.UtcNow.Ticks,
                        DAOMigrated = false,
                    });
                    context.SaveChanges();
                }
            }
        }

        public async Task<int> DeleteMigratedDataAsync()
        {
            var sql = $"DELETE FROM {GetFullTableName()} WHERE DAOMigrated=1";

            using (var context = GetNewContext())
            {
                return await context.Database.ExecuteSqlCommandAsync(sql);
            }
        }

        public async Task CreateByBulkCopyAsync(IEnumerable<ArchiverDedupInfo> items, bool isMigrated)
        {
            if (items.Count() == 0)
            {
                return;
            }
            logger.Debug("Total add dedup infoes: {0}", items.Count());
            using (new PerformanceScope("Batch dedup infoes"))
            {
                var tableName = GetFullTableName();
                using (var table = ConvertToDataTable(items, isMigrated))
                {
                    table.TableName = tableName;
                    await BatchAddAsync(table, tableName);
                }
            }
        }

        private string GetFullTableName()
        {
            return $"[{GetTenantSchemaName()}].[ArchiverDedupInfoes]";
        }

        private DataTable ConvertToDataTable(IEnumerable<ArchiverDedupInfo> items, bool isMigrated)
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(Guid));
            table.Columns.Add("SiteUrl", typeof(String));
            table.Columns.Add("FirstDedupTime", typeof(Int64));
            table.Columns.Add("LastDedupTime", typeof(Int64));
            table.Columns.Add("Created", typeof(Int64));
            table.Columns.Add("Modified", typeof(Int64));
            table.Columns.Add("DAOMigrated", typeof(Boolean));

            foreach (var item in items)
            {
                var row = table.NewRow();
                row["Id"] = item.Id;
                row["SiteUrl"] = item.SiteUrl;
                row["FirstDedupTime"] = item.FirstDedupTime;
                row["LastDedupTime"] = item.LastDedupTime;
                row["Created"] = item.Created;
                row["Modified"] = item.Modified;
                row["DAOMigrated"] = isMigrated;
                table.Rows.Add(row);
            }

            return table;
        }
    }
}
