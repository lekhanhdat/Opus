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
using AvePoint.GCommon.Utility;
using AvePoint.RA.DB.Core.Synchronize.DbContext.Base;
using AvePoint.RA.DB.Core.Synchronize.DbContext.TypeMapper;

namespace AvePoint.RA.DB.Core.Synchronize.DbContext;

public class RMSynchonizeSqliteDbTableSet : ISynchronizeDbTableSet
{
    public Type Type { get; private set; }
    
    public string Schema { get; private set; }

    public RMSynchonizeSqliteDbTableSet(Type type, string schema)
    {
        Type = type;
        Schema = schema;
    }
    
    public string GetExistsTableSql()
    {
        var tableInfo = RMSynchronizeDbTableMapper.Get(Type);
        return
            $"SELECT COUNT(1) FROM sqlite_master WHERE type = 'table' AND Name = '{SecurityUtils.SanitizeSQLParameterName(Schema)}${SecurityUtils.SanitizeSQLParameterName(tableInfo.Name)}'";
    }

    public string GetCreateTableSql()
    {
        var tableInfo = RMSynchronizeDbTableMapper.Get(Type);
        var keyColumn = tableInfo.Columns.First(item => item.IsKey);

        var createSql = $@"CREATE TABLE {Schema}${SecurityUtils.SanitizeSQLParameterName(tableInfo.Name)} (
        {SecurityUtils.SanitizeSQLParameterName(keyColumn.Name)} NVARCHAR PRIMARY KEY NOT NULL,
";
        foreach (var columnInfo in tableInfo.Columns.Where(item => !item.IsKey))
        {
            createSql += $"{SecurityUtils.SanitizeSQLParameterName(columnInfo.Name)} {columnInfo.TypeName},";
        }

        createSql = createSql.TrimEnd(',');
        createSql += ")";
        return createSql;
    }

    public string GetDropTableSql()
    {
        var tableInfo = RMSynchronizeDbTableMapper.Get(Type);
        return $@"DROP TABLE IF EXISTS [{Schema}].[{SecurityUtils.SanitizeSQLParameterName(tableInfo.Name)}]";
    }

    public IEnumerable<string> GetAddIndexSql()
    {
        var tableInfo = RMSynchronizeDbTableMapper.Get(Type);
        foreach (var column in tableInfo.Columns.Where(item => item.NeedIndex))
        {
            var sql =
                $@"CREATE INDEX idx_{SecurityUtils.SanitizeSQLSchemaName(Schema)}_{SecurityUtils.SanitizeSQLParameterName(tableInfo.Name)}_{SecurityUtils.SanitizeSQLParameterName(column.Name)} ON 
{SecurityUtils.SanitizeSQLSchemaName(Schema)}${SecurityUtils.SanitizeSQLParameterName(tableInfo.Name)}({SecurityUtils.SanitizeSQLParameterName(column.Name)})";
            yield return sql;
        }
    }
}