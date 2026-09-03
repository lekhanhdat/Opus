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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMFSAuditDao : BaseDao<RMFSAudit>, IRMFSAuditDao
    {
        private const string TableName = "RMFSAudits";
        private static readonly HashSet<string> ValidOrderColumns = new(StringComparer.OrdinalIgnoreCase)
        {
            nameof(RMFSAudit.ExecutedTime),
            nameof(RMFSAudit.AuditType),
            nameof(RMFSAudit.AuditLevel),
            nameof(RMFSAudit.ExecutedBy)
        };

        private RALogger _logger = RALogger.GetInstance(typeof(RMFSAuditDao));


        public async Task BulkInsertAsync(IReadOnlyList<FSAuditRecord> records)
        {
            if (records == null || records.Count == 0) return;
            _logger.Info($"Start bulk insert {records.Count} FSAudit records.");
            using (new PerformanceScope("Bulk insert FSAudit records"))
            {
                var tableName = GetFullTableName();
                using var dataTable = BuildDataTable(tableName, records);
                await BatchAddAsync(dataTable, tableName, 60);
            }
        }

        public async Task<RMFSAudit> InsertAsync(FSAuditRecord record)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            var entity = MapRecordToEntity(record);
            return await CreateAsync(entity);
        }


        public async Task InsertBatchAsync(List<FSAuditRecord> records)
        {
            if (records == null || records.Count == 0) throw new ArgumentException("Records cannot be null or empty.", nameof(records));
            var entities = records.Select(MapRecordToEntity).ToList();
            await BatchCreateAsync(entities);
        }

        public Task<(List<RMFSAudit> Items, int TotalCount)> QueryAsync(FSAuditQueryParam queryParam)
        {
            if (queryParam == null) throw new ArgumentNullException(nameof(queryParam));
            return QueryAsync(null, queryParam);
        }

        public async Task<(List<RMFSAudit> Items, int TotalCount)> QueryAsync(Expression<Func<RMFSAudit, bool>> filterExpression, FSAuditQueryParam queryParam)
        {
            if (queryParam == null) throw new ArgumentNullException(nameof(queryParam));

            var sortDirection = GetSortDirection(queryParam);
            var sortColumn = GetSortColumn(queryParam);

            using var context = GetNewContext();

            var baseQuery = context.Set<RMFSAudit>().AsNoTracking().AsQueryable();

            if (filterExpression != null)
            {
                baseQuery = baseQuery.Where(filterExpression);
            }

            int totalCount = await baseQuery.CountAsync();

            if (totalCount == 0)
            {
                return (new List<RMFSAudit>(), 0);
            }

            var orderedQuery = baseQuery.SortBy(sortColumn, sortDirection);

            var data = await orderedQuery
                .Skip((queryParam.PageIndex - 1) * queryParam.PageSize)
                .Take(queryParam.PageSize)
                .ToListAsync();

            return (data, totalCount);
        }

        private static string GetSortColumn(FSAuditQueryParam queryParam)
        {
            var columnName = queryParam.Order?.ColumnName;
            if (!string.IsNullOrEmpty(columnName) && ValidOrderColumns.Contains(columnName))
            {
                return columnName;
            }
            return nameof(RMFSAudit.ExecutedTime);
        }

        private static SortDirectionEnum GetSortDirection(FSAuditQueryParam queryParam)
        {
            var isDesc = queryParam.Order?.IsDesc ?? queryParam.IsDesc;
            return isDesc ? SortDirectionEnum.Descending : SortDirectionEnum.Ascending;
        }

        private static RMFSAudit MapRecordToEntity(FSAuditRecord record)
        {
            return new RMFSAudit
            {
                AuditType = record.AuditType,
                AuditLevel = record.AuditLevel,
                Content = record.Content,
                ClientIP = record.ClientIP,
                ExecutedBy = record.UserName,
                ExecutedTime = record.ActionTimeUtc,
                Status = record.Status,
                ObjectName = record.ObjectName,
                ConnectionGroupId = record.ConnectionGroupId,
                ConnectionId = record.ConnectionId,
                ItemId = record.ItemId,
                FullPath = record.CurrentPath,
                PreviousPath = record.PreviousPath,
            };
        }

        private static DataTable BuildDataTable(string tableName, IReadOnlyList<FSAuditRecord> records)
        {
            var table = new DataTable(tableName);
            table.Columns.Add(nameof(RMFSAudit.AuditType), typeof(int));
            table.Columns.Add(nameof(RMFSAudit.AuditLevel), typeof(int));
            table.Columns.Add(nameof(RMFSAudit.Content), typeof(string));
            table.Columns.Add(nameof(RMFSAudit.ClientIP), typeof(string));
            table.Columns.Add(nameof(RMFSAudit.ExecutedBy), typeof(string));
            table.Columns.Add(nameof(RMFSAudit.ExecutedTime), typeof(long));
            table.Columns.Add(nameof(RMFSAudit.Status), typeof(int));
            table.Columns.Add(nameof(RMFSAudit.ObjectName), typeof(string));
            table.Columns.Add(nameof(RMFSAudit.ConnectionGroupId), typeof(string));
            table.Columns.Add(nameof(RMFSAudit.ConnectionId), typeof(string));
            table.Columns.Add(nameof(RMFSAudit.ItemId), typeof(string));
            table.Columns.Add(nameof(RMFSAudit.FullPath), typeof(string));
            table.Columns.Add(nameof(RMFSAudit.PreviousPath), typeof(string));

            table.BeginLoadData();
            for (int i = 0; i < records.Count; i++)
            {
                var record = records[i];
                var row = table.NewRow();
                row[0] = record.AuditType;
                row[1] = record.AuditLevel;
                row[2] = record.Content ?? (object)DBNull.Value;
                row[3] = record.ClientIP ?? (object)DBNull.Value;
                row[4] = record.UserName ?? (object)DBNull.Value;
                row[5] = record.ActionTimeUtc;
                row[6] = record.Status;
                row[7] = record.ObjectName ?? (object)DBNull.Value;
                row[8] = record.ConnectionGroupId ?? (object)DBNull.Value;
                row[9] = record.ConnectionId ?? (object)DBNull.Value;
                row[10] = record.ItemId ?? (object)DBNull.Value;
                row[11] = record.CurrentPath ?? (object)DBNull.Value;
                row[12] = record.PreviousPath ?? (object)DBNull.Value;
                table.Rows.Add(row);
            }
            table.EndLoadData();

            return table;
        }

        private string GetFullTableName() => $"[{SecurityUtils.SanitizeSQLSchemaName(GetTenantSchemaName())}].[{TableName}]";

        public List<FSAuditType> FetchAllAuditTypes()
        {
            var excludedTypes = new List<FSAuditType>
            {
                FSAuditType.RunFSRestoreJob,
                //FSAuditType.MyhubClassify,
            };
            return Enum.GetValues<FSAuditType>().Except(excludedTypes).ToList();
        }

        public List<string> FetchAllAuditUsers()
        {
            using (var context = GetNewContext())
            {
                return context.Set<RMFSAudit>().AsNoTracking().Where(x=> x.ExecutedBy != null).Select(audit => audit.ExecutedBy).Distinct().ToList();
            }
        }
    }
}