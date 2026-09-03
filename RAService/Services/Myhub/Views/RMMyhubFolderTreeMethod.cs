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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.MyHub.Items.Views;
using AvePoint.RA.Contract.MyHub.Model.QueryRequest.Views;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.MyHub.NewMethods
{
    public class RMMyhubFolderTreeMethod
    {
        private RMMyhubQueryRecordsMethod _recordStore;
        private RMMyhubQueryRecordsMethod RecordStore => _recordStore ??= new RMMyhubQueryRecordsMethod();
        public (string sql, List<SqlParameter> parameters) BuildQuery(RMMyhubTreeChildFolderQueryInfo queryInfo)
        {
            var sql = BuildFolderSelectSql();
            var sqlParameters = BaseSqlParameters(queryInfo.PartitionKeyId);

            if (queryInfo.RootFolderId != Guid.Empty)
            {
                sql += " AND c.nodeId = @RootFolderId";
                AddParameter(sqlParameters, "@RootFolderId", queryInfo.RootFolderId);
            }

            if (queryInfo.ParentId != Guid.Empty)
            {
                sql += " AND c.parentId = @ParentId";
                AddParameter(sqlParameters, "@ParentId", queryInfo.ParentId);
            }
            sql += " ORDER BY c.leafName";
            return (sql, sqlParameters);
        }

        public string IsHasChildrenSql()
        {
            var sql = @"SELECT VALUE
COUNT(1) FROM c
WHERE c.sourceFlag = @sourceFlag
AND c.recordStatus=@statuses
AND c.parentId = @ParentId
AND c.nodeType=@NodeType
AND c.l2PartitionKey = @l2PartitionKey
";
            return sql;
        }

        public List<SqlParameter> IsHasChildrenSqlParameters(Guid parentId, string partitionKeyId)
        {
            var sqlParameters = new List<SqlParameter>
            {
                new SqlParameter("@sourceFlag", (int)SourceFlag.FileSystem),
                new SqlParameter("@NodeType",(int)NodeLevel.FSFolder),
                new SqlParameter("@l2PartitionKey",partitionKeyId.ToLowerInvariant()),
                new SqlParameter("@statuses", (int)RMRecordStatus.Active),
                new SqlParameter("@ParentId", parentId)
            };
            return sqlParameters;
        }

        private static string BuildFolderSelectSql()
        {
            return @"SELECT VALUE {
    ""Id"": c.nodeId,
    ""NodeId"":c.nodeId,
    ""ParentId"": c.parentId,
    ""PartitionKeyId"": c.l2PartitionKey,
    ""Name"": c.leafName,
    ""Path"": c.dirPath,
    ""ClassCode"": c.classCode,
    ""CountryCode"":c.countryCode,
    ""RetentionType"":c.retentionType,
    ""Size"":c.jpmcFileSize,
    ""PendingDisposal"":c.manual_approvedStatus,
    ""RecordId"": c.recordsId,
    ""EndDate"":c.endTime,
    ""StartDate"":c.startDate
} 
FROM c
WHERE c.sourceFlag = @sourceFlag
AND c.nodeType = @nodeType
AND c.recordStatus=@statuses
AND c.l2PartitionKey=@l2PartitionKey
AND IS_DEFINED(c.recordsId)
AND NOT IS_NULL(c.recordsId)";
        }

        private static void AddParameter(List<SqlParameter> sqlParameters, string name, object value)
        {
            sqlParameters.Add(new SqlParameter(name, value));
        }

        private List<SqlParameter> BaseSqlParameters(string partitionKeyId)
        {
            var sqlParameters = new List<SqlParameter>
            {
                new SqlParameter("@sourceFlag", (int)SourceFlag.FileSystem),
                new SqlParameter("@nodeType", (int)NodeLevel.FSFolder),
                new SqlParameter("@statuses", (int)RMRecordStatus.Active),
                new SqlParameter("@l2PartitionKey", partitionKeyId.ToLowerInvariant())
            };
            return sqlParameters;
        }

        public (string sql, List<SqlParameter> parameters) BuildBatchHasChildrenSql(List<Guid> parentIds, string partitionKeyId)
        {
            if (parentIds == null || parentIds.Count == 0)
                return ("SELECT VALUE []", new List<SqlParameter>());

            var idConditions = new List<string>();
            var parameters = new List<SqlParameter>
    {
        new SqlParameter("@sourceFlag", (int)SourceFlag.FileSystem),
        new SqlParameter("@nodeType", (int)NodeLevel.FSFolder),
        new SqlParameter("@l2PartitionKey", partitionKeyId.ToLowerInvariant()),
        new SqlParameter("@statuses", (int)RMRecordStatus.Active),
    };

            for (int i = 0; i < parentIds.Count; i++)
            {
                var paramName = $"@parentId_{i}";
                idConditions.Add(paramName);
                parameters.Add(new SqlParameter(paramName, parentIds[i]));
            }

            var idInClause = string.Join(", ", idConditions);

            var sql = $@"
        SELECT 
            c.parentId,
            COUNT(1) AS ChildCount
        FROM c
        WHERE c.sourceFlag = @sourceFlag
        AND c.recordStatus = @statuses
        AND c.nodeType = @nodeType
        AND c.l2PartitionKey = @l2PartitionKey
        AND c.parentId IN ({idInClause})
        GROUP BY c.parentId
    ";

            return (sql, parameters);
        }

        public async Task<Dictionary<Guid, bool>> GetBatchHasChildrenAsync(List<Guid> parentIds, string partitionKeyId)
        {
            var result = new Dictionary<Guid, bool>();

            if (parentIds == null || parentIds.Count == 0)
                return result;

            foreach (var id in parentIds)
            {
                result[id] = false;
            }

            var (sql, parameters) = BuildBatchHasChildrenSql(parentIds, partitionKeyId);
            var rows = await RecordStore.QueryAllAsync<ChildCountResult>(sql, parameters);

            foreach (var row in rows)
            {
                var parentId = row.parentId;
                var childCount = row.ChildCount;
                result[parentId] = childCount > 0;
            }

            return result;
        }
        public class ChildCountResult
        {
            public Guid parentId { get; set; }
            public int ChildCount { get; set; }
        }
    }
}
