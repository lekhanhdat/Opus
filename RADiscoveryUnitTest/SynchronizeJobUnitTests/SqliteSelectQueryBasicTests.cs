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
using AvePoint.RA.DB.Core.Synchronize.DbContext.SqliteQuery;
using AvePoint.RA.DB.Core.Synchronize.DbContext.TypeMapper;
using AvePoint.RA.DB.Core.Synchronize.DbContext.Utility.RecordQuery;
using AvePoint.RA.DB.Core.Synchronize.DbManager;
using AvePoint.RA.DB.Model;

namespace RADiscoveryUnitTest.SynchronizeJobUnitTests;

[TestClass]
public class SqliteSelectQueryBasicTests : SynchronizeJobInitializeTest
{

    private string SelectQuery()
    {
        var tableInfo = RMSynchronizeDbTableMapper.Get(typeof(RMRemoteNode));
        var schemaName = RMSynchronizeDbManager.GetSchemaName();
        var needSelectedColumns = tableInfo.Columns.Select(item => item.Name).ToList();
        SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name);
        return
            $"SELECT {string.Join(", ", needSelectedColumns)} FROM {SecurityUtils.SanitizeSQLSchemaName(schemaName)}${SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name)}";
    }
            
    [TestMethod]
    public void  SelectQueryWithMultipleFilters_ShouldBeSuccessful()
    {
        var sql = new RecordQuery
        {
            Table = typeof(RMRemoteNode),
            PlaceHolder = PlaceHolder.Select,
            Filters =
            [
                new QueryFilter
                {
                    ColumnName = nameof(RMRemoteNode.NodeLevel),
                    Operator = Operator.Equal,
                    Value = "44"
                },
                new QueryFilter
                {
                    ColumnName = nameof(RMRemoteNode.Id),
                    Operator = Operator.Equal,
                    Value = "123"
                },
                new QueryFilter
                {
                    ColumnName = nameof(RMRemoteNode.Name),
                    Operator = Operator.Equal,
                    Value = "'Test'"
                },
            ]
        };
        var result = sql.BuildSqlString();
        var expected = $@"{SelectQuery()} 
WHERE NodeLevel = 44 AND Id = 123 AND Name = 'Test'";
        StringAssert.Contains(NormalizedString(result), NormalizedString(expected));    
    }
    
    
    [TestMethod]
    public void  SelectQueryWithOneFilter_ShouldBeSuccessful()
    {
        var sql = new RecordQuery
        {
            Table = typeof(RMRemoteNode),
            PlaceHolder = PlaceHolder.Select,
            Filters =
            [
                new QueryFilter
                {
                    ColumnName = nameof(RMRemoteNode.NodeLevel),
                    Operator = Operator.Equal,
                    Value = "44"
                }
            ]
        };
        
        var result = sql.BuildSqlString();
        var expected = $@" {SelectQuery()} 
WHERE NodeLevel = 44";
        StringAssert.Contains(NormalizedString(result), NormalizedString(expected));    
    }
    
    [TestMethod]
    public void  SelectQueryWithFilterAndOrderByDesc_ShouldBeSuccessful()
    {
        var columnName = nameof(RMRemoteNode.Id);
        var oderByDesc = true;
        var sql = new RecordQuery
        {
            Table = typeof(RMRemoteNode),
            PlaceHolder = PlaceHolder.Select,
            Filters =
            [
                new QueryFilter
                {
                    ColumnName = nameof(RMRemoteNode.NodeLevel),
                    Operator = Operator.Equal,
                    Value = "44"
                }
            ],
            OrderBy = new()
            {
                ColumnName = columnName,
                OrderByDesc = oderByDesc
            }
        };
        var result = sql.BuildSqlString();

        var expected = $@"{SelectQuery()} 
WHERE NodeLevel = 44 ORDER BY {columnName} {(oderByDesc ? "DESC" : "ASC")}";
        StringAssert.Contains(NormalizedString(result), NormalizedString(expected));    
    }
    
    [TestMethod]
    public void  SelectQueryWithFilterAndLimit_ShouldBeSuccessful()
    {
        uint limit = 1;
        var sql = new RecordQuery
        {
            Table = typeof(RMRemoteNode),
            PlaceHolder = PlaceHolder.Select,
            Filters =
            [
                new QueryFilter
                {
                    ColumnName = nameof(RMRemoteNode.NodeLevel),
                    Operator = Operator.Equal,
                    Value = "44"
                }
            ],
            Limit = limit
        };
        var result = sql.BuildSqlString();

        var expected = $@"{SelectQuery()} 
WHERE NodeLevel = 44 LIMIT {limit}";
        StringAssert.Contains(NormalizedString(result), NormalizedString(expected));    
    }
    
    [TestMethod]
    public void  SelectQueryWithFilterAndLimitAndOffSet_ShouldBeSuccessful()
    {
        uint limit = 1;
        uint offset = 5;
        var sql = new RecordQuery
        {
            Table = typeof(RMRemoteNode),
            PlaceHolder = PlaceHolder.Select,
            Filters =
            [
                new QueryFilter
                {
                    ColumnName = nameof(RMRemoteNode.NodeLevel),
                    Operator = Operator.Equal,
                    Value = "44"
                }
            ],
            Limit = limit,
            OffSet = offset
        };
        var result = sql.BuildSqlString();

        var expected = $@"{SelectQuery()} 
WHERE NodeLevel = 44 LIMIT {limit} OFFSET {offset}";
        StringAssert.Contains(NormalizedString(result), NormalizedString(expected));    
    }
}