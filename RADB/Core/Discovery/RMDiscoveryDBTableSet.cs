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
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Core.Discovery
{
    public class RMDiscoveryDBTableSet
    {

        private readonly string _schema;

        private readonly Type _type;

        public RMDiscoveryDBTableSet(Type type, string schema)
        {
            _type = type;
            _schema = schema;
        }

        public string GetExistsTableSql()
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(_type);
            return $"SELECT TOP 1 1 FROM sysObjects WHERE id = object_id('[{SecurityUtils.SanitizeSQLSchemaName(_schema)}].[{SecurityUtils.SanitizeSQLParameterName(tableInfo.Name)}]') and xtype = 'U'";
        }

        public string GetCreateTableSql()
        {
            return GetCreateTableSql(new List<RMDiscoveryCustomColumn>());
        }

        public string GetCreateTableSql(IEnumerable<RMDiscoveryCustomColumn> customColumns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(_type);
            var keyColumn = tableInfo.Columns.First(item => item.IsKey);
            var createSql = $@"CREATE TABLE [{_schema}].[{SecurityUtils.SanitizeSQLParameterName(tableInfo.Name)}](
[{SecurityUtils.SanitizeSQLParameterName(keyColumn.Name)}] [{keyColumn.TypeName}] {(keyColumn.NeedAutoIncremental ? "IDENTITY(1,1)" : "")} NOT NULL,";

            foreach (var columnInfo in tableInfo.Columns.Where(item => !item.IsKey))
            {
                createSql += $"[{SecurityUtils.SanitizeSQLParameterName(columnInfo.Name)}] [{columnInfo.TypeName}]{columnInfo.MaxLength},";
            }

            foreach (var customColumn in customColumns)
            {
                createSql += $"[{SecurityUtils.SanitizeSQLParameterName(customColumn.Name)}] [{customColumn.DBTypeName}],";
            }


            createSql += $@"CONSTRAINT [PK_{SecurityUtils.SanitizeSQLParameterName(tableInfo.Name)}] PRIMARY KEY CLUSTERED (
    [{SecurityUtils.SanitizeSQLParameterName(keyColumn.Name)}] ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
";

            return createSql += ") ON [PRIMARY]";
        }

        public string GetSqliteExistsTableSql()
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(_type);
            return $"SELECT COUNT(1) FROM sqlite_master WHERE type = 'table' AND Name = '{SecurityUtils.SanitizeSQLParameterName(_schema)}${SecurityUtils.SanitizeSQLParameterName(tableInfo.Name)}'";
        }

        public string GetSqliteCreateTableSql()
        {
            return GetSqliteCreateTableSql(new List<RMDiscoveryCustomColumn>());
        }

        public string GetSqliteCreateTableSql(IEnumerable<RMDiscoveryCustomColumn> customColumns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(_type);
            var keyColumn = tableInfo.Columns.First(item => item.IsKey);

            var createSql = $@"CREATE TABLE {_schema}${SecurityUtils.SanitizeSQLParameterName(tableInfo.Name)} (
        {SecurityUtils.SanitizeSQLParameterName(keyColumn.Name)} INTEGER PRIMARY KEY AUTOINCREMENT,
";
            foreach (var columnInfo in tableInfo.Columns.Where(item => !item.IsKey))
            {
                createSql += $"{SecurityUtils.SanitizeSQLParameterName(columnInfo.Name)} {columnInfo.TypeName},";
            }

            foreach (var customColumn in customColumns)
            {
                createSql += $"{SecurityUtils.SanitizeSQLParameterName(customColumn.Name)} {customColumn.DBTypeName},";
            }

            createSql = createSql.TrimEnd(',');
            createSql += ")";
            return createSql;
        }

        public string GetDropTableSql()
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(_type);
            return $@"DROP TABLE IF EXISTS [{_schema}].[{SecurityUtils.SanitizeSQLParameterName(tableInfo.Name)}]";
        }

        public string GetSqliteDropTableSql()
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(_type);
            return $@"DROP TABLE IF EXISTS {_schema}${SecurityUtils.SanitizeSQLParameterName(tableInfo.Name)}";
        }

        public IEnumerable<string> GetAddIndexSql()
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(_type);
            foreach(var column in tableInfo.Columns.Where(item => item.NeedIndex))
            {
                var sql = $@"CREATE NONCLUSTERED INDEX [IX_{SecurityUtils.SanitizeSQLSchemaName(tableInfo.Name)}_{SecurityUtils.SanitizeSQLParameterName(column.Name)}] 
ON [{SecurityUtils.SanitizeSQLSchemaName(_schema)}].[{SecurityUtils.SanitizeSQLParameterName(tableInfo.Name)}]
(
    [{SecurityUtils.SanitizeSQLParameterName(column.Name)}] ASC
)";
                yield return sql;
            }
        }

        public IEnumerable<string> GetSqliteAddIndexSql()
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(_type);
            foreach (var column in tableInfo.Columns.Where(item => item.NeedIndex))
            {
                var sql = $@"CREATE INDEX idx_{SecurityUtils.SanitizeSQLSchemaName(_schema)}_{SecurityUtils.SanitizeSQLParameterName(tableInfo.Name)}_{SecurityUtils.SanitizeSQLParameterName(column.Name)} ON 
{SecurityUtils.SanitizeSQLSchemaName(_schema)}${SecurityUtils.SanitizeSQLParameterName(tableInfo.Name)}({SecurityUtils.SanitizeSQLParameterName(column.Name)})";
                yield return sql;
            }
        }

        public string GetQueryDeleteRowsBySiteId(int siteId)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(_type);
            //return $@"DELETE FROM [{_schema}]$[{SecurityUtils.SanitizeSQLParameterName(tableInfo.Name)}] WHERE [SiteId] = {siteId}";
            return $@"DELETE FROM {SecurityUtils.SanitizeSQLSchemaName(_schema)}${SecurityUtils.SanitizeSQLParameterName(tableInfo.Name)} WHERE SiteId = {siteId}";
        }
        public string GetQueryColumnsSql()
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(_type);
            return $@"SELECT COLUMN_NAME AS Name, DATA_TYPE AS TypeName
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = '{SecurityUtils.SanitizeSQLParameterName(tableInfo.Name)}' AND TABLE_SCHEMA = '{_schema}'";
        }

        public IEnumerable<string> GetAddColumnsSql(IEnumerable<RMDiscoveryColumnInfo> columns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(_type);
            foreach (var column in columns)
            {
                var sql = $@"ALTER TABLE [{_schema}].[{SecurityUtils.SanitizeSQLParameterName(tableInfo.Name)}] add {SecurityUtils.SanitizeSQLParameterName(column.Name)} {column.TypeName}";
                yield return sql;
            }
        }

        public IEnumerable<string> GetEditColumnsSql(IEnumerable<RMDiscoveryColumnInfo> columns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(_type);
            foreach(var column in columns)
            {
                var sql = $@"ALTER TABLE [{_schema}].[{SecurityUtils.SanitizeSQLParameterName(tableInfo.Name)}] alter {SecurityUtils.SanitizeSQLParameterName(column.Name)} {column.TypeName}";
                yield return sql;
            }
        }

        public IEnumerable<string> GetSetDefaultValueColumnSql(IEnumerable<RMDiscoveryColumnInfo> columns)
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(_type);
            foreach(var column in columns)
            {
                if(column.HasDefaultValue)
                {
                    var sql = $@"UPDATE [{_schema}].[{SecurityUtils.SanitizeSQLParameterName(tableInfo.Name)}]
SET [{SecurityUtils.SanitizeSQLParameterName(column.Name)}] = {column.DefaultValue}";
//                    var sql = $@"ALTER TABLE [{_schema}].[{tableInfo.Name}] 
//ADD CONSTRAINT DF_{column.Name} 
//DEFAULT {column.DefaultValue} FOR [{column.Name}]";
                    yield return sql;
                }
            }
        }

        public string GetResetIdentifierSql()
        {
            var tableInfo = RMDiscoveryDBTableManager.Get(_type);
            return $"EXEC('DBCC CHECKIDENT ('{SecurityUtils.SanitizeSQLParameterName(tableInfo.Name)}', RESEED, 0)')";
        }
    }

    public class RMDiscoveryTableInfo
    {
        public string Name { get; set; }

        public List<RMDiscoveryColumnInfo> Columns { get; set; } = new();
    }
}
