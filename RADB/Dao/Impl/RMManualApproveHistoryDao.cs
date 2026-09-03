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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.AzureTable;
using AvePoint.RA.DB.AzureTable.Model;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMManualApproveHistoryDao : BaseDao<RMManualApproveHistory>, IRMManualApproveHistoryDao
    {
        public Task<int> DeleteExpiredDatasAsync(long expiredTicks)
        {
            return BatchDeleteAsync(item => item.ArchivedTime <= expiredTicks);
        }

        public Task<int> DeleteAllAsync()
        {
            return BatchDeleteAsync(item => true);
        }

        public IEnumerable<List<RMManualApproveHistory>> GetExpiredDatas(long expiredTicks, int limit = 1000)
        {
            using (var context = GetNewContext())
            {
                var count = 0;
                var pageIndex = 0;
                do
                {
                    var result = context.ManualApproveHistory
                        .Where(item => item.ArchivedTime <= expiredTicks)
                        .Skip(limit * pageIndex++)
                        .Take(limit)
                        .ToList();
                    count = result.Count;
                    yield return result;
                } while (count == limit);
            }
        }

        /*public async Task<int> Count(ManualApprovalHistoryDBQueryDefinition queryDefinition)
        {
            using var context = GetNewContext();
            var predicateSql = BuildPredicateSql(queryDefinition.SqlDefinitons);
            var parameters = BuildSqlParameters(queryDefinition.SqlDefinitons);
            var clonedPrarameters = parameters.Select(i => ((ICloneable)i).Clone()).ToArray();
            var sql = $@"
SELECT COUNT(1) FROM {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.[RMManualApproveHistories]
WHERE {predicateSql}";

            var result = await context.Database.SqlQuery<int>(sql, clonedPrarameters).FirstOrDefaultAsync();
            return result;
        }*/

        public async Task<IEnumerable<RMManualApproveHistory>> QueryItems(ManualApprovalHistoryDBQueryDefinition queryDefinition)
        {
            using var context = GetNewContext();
            var pageIndex = queryDefinition.PageIndex - 1;
            var pageSize = queryDefinition.PageSize;
            var predicateSql = BuildPredicateSql(queryDefinition.SqlDefinitons);
            var parameters = BuildSqlParameters(queryDefinition.SqlDefinitons);
            var clonedPrarameters = parameters.Select(i => ((ICloneable)i).Clone()).ToArray();
            SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
            var sql = $@"
SELECT * FROM {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.[RMManualApproveHistories]
WHERE {predicateSql} ORDER BY ActionTime DESC
OFFSET {pageIndex * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY
";

            var result = await context.Database.SqlQuery<RMManualApproveHistory>(sql, clonedPrarameters).ToListAsync();
            return result;
        }

        public async Task<List<RMManualApproveHistory>> QueryItemsForMigration(int pageIndex, int pageSize)
        {
            using var context = GetNewContext();
            var sql = $@"
SELECT * FROM {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.[RMManualApproveHistories]
ORDER BY ID
OFFSET {pageIndex * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY
";
            context.Database.CommandTimeout = 60 * 10;
            var result = await context.Database.SqlQuery<RMManualApproveHistory>(sql).ToListAsync();
            return result;
        }

        public static string BuildPredicateSql(List<ManualApprovalSqlDefintion> sqlDefinitions)
        {
            var sqls = sqlDefinitions.Select(item => item.Sql).ToList();
            sqls.Add("IsRemoved = 0");
            return string.Join(" AND ", sqls);
        }

        public static SqlParameter[] BuildSqlParameters(List<ManualApprovalSqlDefintion> sqlDefinitions)
        {
            return sqlDefinitions.SelectMany(item => item.Parameter).ToArray();
        }

        public int ExecuteSqlCommand(Func<string, string> executeSql, params object[] parameters)
        {
            using var context = GetNewContext();
            context.Database.CommandTimeout = 600;
            SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
            var sql = executeSql(context.SchemaName);
            return context.Database.ExecuteSqlCommand(sql, parameters);
        }

        public async Task<int> DeleteOldestDatasForJob(int count)
        {
            using var context = GetNewContext();
            var pageSize = 1000;
            var pageCount = (count - 1) / pageSize;
            var pageArr = new int[pageCount];
            Array.Fill(pageArr, pageSize);
            pageArr[^1] = count - pageCount * pageSize;

            var willDeleteItems = new List<RMManualApproveHistory>();

            for (var i = 0; i < pageArr.Length; i++)
            {
                var takeCount = pageArr[i];
                var items = await context.ManualApproveHistory
                    .OrderBy(item => item.ActionTime)
                    .Skip(i * pageSize)
                    .Take(takeCount)
                    .ToListAsync();
                willDeleteItems.AddRange(items);
            }

            context.ManualApproveHistory.RemoveRange(willDeleteItems);
            return await context.SaveChangesAsync();
        }

        public bool Add(RMManualApproveHistory entity)
        {
            using var context = GetNewContext();
            context.ManualApproveHistory.Add(entity);
            return context.SaveChanges() > 0;
        }
    }
}
