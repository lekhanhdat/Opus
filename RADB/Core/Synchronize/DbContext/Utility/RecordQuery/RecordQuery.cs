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
using System;
using System.Collections.Generic;
using System.Linq;
using AvePoint.RA.DB.Core.Synchronize.DbContext.SqliteQuery;
using AvePoint.RA.DB.Core.Synchronize.DbContext.TypeMapper;

namespace AvePoint.RA.DB.Core.Synchronize.DbContext.Utility.RecordQuery;

public record RecordQuery
{
    public required PlaceHolder PlaceHolder { get; init; }

    public required Type Table { get; init; }

    public IEnumerable<QueryFilter> Filters { get; init; } = [];

    public QueryOrderBy OrderBy { get; init; }

    public uint Limit { get; init; }

    public uint OffSet { get; init; }

    public string BuildSqlString()
    {
        var tableInfo = RMSynchronizeDbTableMapper.Get(Table);

        var actionStatement = BuildAction();
        
        var whereStatement = BuildWhereClause(tableInfo);
        
        whereStatement.ForEach(item => actionStatement.Add(item));
        
        var orderByStatement = BuildOrderBy(tableInfo);
        
        actionStatement.Add(orderByStatement);
        
        actionStatement.Add(new LimitSqlite
        {
            Limit = Limit,
        })
        .Add(new OffsetSqlite
        {
            Offset = OffSet,
        });
        
        return actionStatement.BuildSql();
    }

    private SqliteSqlBuilder BuildOrderBy(RMSynchronizeTableInfo tableInfo)
    {
        var columnName = tableInfo.Columns.FirstOrDefault(f=>f.Name.EqualsIgnoreCase(OrderBy?.ColumnName));

        return new OrderBySqlite
        {
            OrderByDesc = OrderBy?.OrderByDesc ?? false,
            OrderByKeyword = columnName?.Name
        };
    }
    
    private List<SqliteSqlBuilder> BuildWhereClause(RMSynchronizeTableInfo tableInfo)
    {
        List<SqliteSqlBuilder> queryFilters = [];
        queryFilters.AddRange(
            from filter in Filters 
            let field = tableInfo.Columns.FirstOrDefault(f => f.Name.EqualsIgnoreCase(filter.ColumnName)) 
            where field is not null 
            select new WhereSqlite
            {
                Condition = BuildCondition(field.Name,filter.Operator,  filter.Value)
            }
        );
        return queryFilters;
    }
    
    private string BuildCondition(string fieldName, Operator op, string value)
    {
        return op switch
        {
            Operator.In => $"{fieldName} {GetOperatorString(op)} ({value})",
            _ => $"{fieldName} {GetOperatorString(op)} {value}"
        };
    }

    private SqliteSqlBuilder BuildAction()
    {
        SqliteSqlBuilder actionStatement = PlaceHolder switch
        {
            PlaceHolder.Select => new SelectSqlite(),
            PlaceHolder.Delete => new DeleteSqlite(),
            _ => throw new ArgumentOutOfRangeException(nameof(PlaceHolder), PlaceHolder, null)
        };
        actionStatement.Table = Table;
        return actionStatement;
    }
    
    private static string GetOperatorString(Operator op)
    {
        return op switch
        {
            Operator.Equal => "=",
            Operator.NotEqual => "<>",
            Operator.GreaterThan => ">",
            Operator.LessThan => "<",
            Operator.GreaterThanOrEqual => ">=",
            Operator.LessThanOrEqual => "<=",
            Operator.In => "IN",
            Operator.Is => "IS",
            _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
        };
    }
}

public enum Operator
{
    Equal,
    NotEqual,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual,
    In,
    Is
}