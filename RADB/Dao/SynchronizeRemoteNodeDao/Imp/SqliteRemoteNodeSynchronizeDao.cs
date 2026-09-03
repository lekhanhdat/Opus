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
using AvePoint.RA.Common.Util;
using AvePoint.RA.DB.Core.Synchronize.DbContext.Base;
using AvePoint.RA.DB.Core.Synchronize.DbContext.SqliteQuery;
using AvePoint.RA.DB.Core.Synchronize.DbContext.Utility;
using AvePoint.RA.DB.Core.Synchronize.DbContext.Utility.RecordQuery;
using AvePoint.RA.DB.Core.Synchronize.DbManager;
using AvePoint.RA.DB.Model;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.SynchronizeDao.Imp;

public class SqliteRemoteNodeSynchronizeDao : IRemoteNodeSynchronizeDao
{
    private static ISynchronizeDbContext GetDbContext() => RMSynchronizeDbManager.GetContext();

    public async Task<IEnumerable<RMRemoteNode>> GetRemoteNodesAsync(IEnumerable<string> ids)
    {
        if(ids.IsNullOrEmpty())
        {
            return [];
        }
        await using var context = GetDbContext();

        var expressionSql = new RecordQuery
        {
            PlaceHolder = PlaceHolder.Select,
            Table = typeof(RMRemoteNode),
            Filters = [
                new QueryFilter
                {
                    ColumnName = nameof(RMRemoteNode.Id),
                    Operator = Operator.In,
                    Value = string.Join(", ", ids.Select(id => $"'{id}'"))
                }
            ]
        };
        
        return await context.ExecuteQueryAsync<RMRemoteNode>(expressionSql.BuildSqlString()).ToListAsync();
    }

    public async IAsyncEnumerable<RMRemoteNode> GetRemoteNodesAsync(string containerId, string tenantId)
    {
        await using var context = GetDbContext();
        
        var expressionSql = new RecordQuery
        {
            PlaceHolder = PlaceHolder.Select,
            Table = typeof(RMRemoteNode),
            Filters = [
                new QueryFilter
                {
                    ColumnName = nameof(RMRemoteNode.ParentId),
                    Operator = Operator.Equal,
                    Value = "@ParentId"
                },
                new QueryFilter
                {
                    ColumnName = nameof(RMRemoteNode.TenantId),
                    Operator = Operator.Equal,
                    Value = "@TenantId"
                }
            ],
            OrderBy = new QueryOrderBy
            {
                ColumnName = nameof(RMRemoteNode.Id)
            }
        };
        
        var parameters = new SQLiteParameter[]
        {
            new("@ParentId", containerId),
            new("@TenantId", tenantId)
        };
        
        var result = context.ExecuteQueryAsync<RMRemoteNode>(expressionSql.BuildSqlString(), parameters);

        await foreach (var remoteNode in result)
        {
            yield return remoteNode;
        };    
    }

    public async Task<IEnumerable<RMRemoteNode>> GetContainerNodesAsync(NodeLevel nodeLevel)
    {
        await using var context = GetDbContext();
        
        var expressionSql = new RecordQuery
        {
            PlaceHolder = PlaceHolder.Select,
            Table = typeof(RMRemoteNode),
            Filters = [
                new QueryFilter
                {
                    ColumnName = nameof(RMRemoteNode.NodeLevel),
                    Operator = Operator.Equal,
                    Value = "@NodeLevel"
                }
            ]
        };

        return await context
            .ExecuteQueryAsync<RMRemoteNode>(expressionSql.BuildSqlString(),
                new SQLiteParameter("@NodeLevel", (int)nodeLevel)).ToListAsync();

    }

    public async Task DeleteNodesAsync(IEnumerable<string> nodeIds)
    {
        if(nodeIds.IsNullOrEmpty())
        {
            return;
        }
        await using var context = GetDbContext();

        var sqlString = BuildDeleteSqlString(nodeIds, out var sqliteParameters);

        await context.ExecuteNonQueryAsync(sqlString, sqliteParameters.ToArray());
    }

    public async Task DeleteNodesByParentIdsAsync(IEnumerable<string> parentIds)
    {
        if(parentIds.IsNullOrEmpty())
        {
            return;
        }
        await using var context = GetDbContext();
        List<SQLiteParameter> expressionParams = new List<SQLiteParameter>();
        var expressionSql = new RecordQuery
        {
            PlaceHolder = PlaceHolder.Delete,
            Table = typeof(RMRemoteNode),
            Filters = [
                new QueryFilter
                {
                    ColumnName = nameof(RMRemoteNode.ParentId),
                    Operator = Operator.In,
                    Value = DatabaseUtility.BuildInClause(parentIds, out expressionParams)?.TrimStart('(')?.TrimEnd(')')
                }
            ]
        };
        var parentExpressionSql = BuildDeleteSqlString(parentIds, out var sqliteParameters);

        await context.ExecuteNonQueryAsync(expressionSql.BuildSqlString(), expressionParams.ToArray());
        await context.ExecuteNonQueryAsync(parentExpressionSql, sqliteParameters.ToArray());
    }

    public async Task AddNodesAsync(IEnumerable<RMRemoteNode> nodes)
    {
        if(nodes.IsNullOrEmpty())
        {
            return;
        }
        await using var context = GetDbContext();
        
        var expressionSql = BuildDeleteSqlString(nodes.Select(node => node.Id), out var sqliteParameters);

        await context.ExecuteNonQueryAsync(expressionSql, sqliteParameters.ToArray());

        await context.ExecuteInsertAsync(nodes);
    }

    public async Task UpdateNodesAsync(IEnumerable<RMRemoteNode> nodes)
    {
        await using var context = GetDbContext();
        var expressionSql = BuildDeleteSqlString(nodes.Select(node => node.Id), out var sqliteParameters);

        await context.ExecuteNonQueryAsync(expressionSql, sqliteParameters.ToArray());
        await context.ExecuteInsertAsync(nodes);
    }

    public async Task<bool> HasAnySites()
    {
        await using var context = GetDbContext();
        var expressionSql = new RecordQuery
        {
            PlaceHolder = PlaceHolder.Select,
            Table = typeof(RMRemoteNode),
            Limit = 1
        };
        var result = await context.ExecuteQueryAsync<RMRemoteNode>(expressionSql.BuildSqlString()).ToListAsync();
        return result.Count > 0;
    }

    private string BuildDeleteSqlString(IEnumerable<string> ids, out List<SQLiteParameter> sqliteParameters)
    {

        var expressionSql = new RecordQuery
        {
            PlaceHolder = PlaceHolder.Delete,
            Table = typeof(RMRemoteNode),
            Filters = [
                new QueryFilter
                {
                    ColumnName = nameof(RMRemoteNode.Id),
                    Operator = Operator.In,
                    Value = DatabaseUtility.BuildInClause(ids, out sqliteParameters)?.TrimStart('(').TrimEnd(')')
                }
            ]
        };
        return expressionSql.BuildSqlString();
    }
}