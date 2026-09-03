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
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Core.Discovery.DBManager.SQLite;
using AvePoint.RA.DB.Model.Discovery;
using Cloud.Sdk.Telemetry.Data.Alita;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Core.Discovery.Context.SQLite
{
    public class RMDiscoveryOffice365SQLiteDBContext : IDisposable
    {
        public int Timeout { get; set; } = 60 * 10;

        private readonly SQLiteConnection _connection;

        public RMDiscoveryOffice365SQLiteDBContext(string connectionString)
        {
            _connection = new SQLiteConnection($"DataSource={connectionString};Version=3");
            _connection.Open();
        }

        public async Task<RMDiscoveryDataCollection> ExecuteQueryAsync(string sql, params SQLiteParameter[] parameters)
        {
            sql = sql.Replace(" Rule ", " [Rule] ");
            using var command = GetSqlCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(parameters);
            await using var reader = await command.ExecuteReaderAsync();
            var dataList = new List<Dictionary<string, RMDiscoveryTableDataFieldInfo>>();
            while (await reader.ReadAsync())
            {
                var fieldCount = reader.FieldCount;
                var row = new Dictionary<string, RMDiscoveryTableDataFieldInfo>(fieldCount);
                for (var i = 0; i < fieldCount; i++)
                {
                    var fieldType = reader.GetFieldType(i);
                    var fieldName = reader.GetName(i);
                    var fieldValue = reader.GetValue(i);
                    row.Add(fieldName, new(fieldType, fieldName, fieldValue));
                }
                dataList.Add(row);
            }

            return new(dataList);
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, params SQLiteParameter[] parameters)
        {
            sql = sql.Replace(" Rule ", " [Rule] ");
            using var command = GetSqlCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(parameters);
            return await command.ExecuteNonQueryAsync();
        }

        public Task<int> ExecuteInsertAsync<T>(List<T> items, Guid o365TenantId) where T : RMDiscoveryDBTable
        {
            return ExecuteInsertAsync(items, RMDiscoveryOffice365SQLiteDBManager.GetSchemaName(o365TenantId));
        }

        public async Task<int> ExecuteInsertAsync<T>(List<T> items, string schema) where T : RMDiscoveryDBTable
        {
            if (items == null || items.Count == 0)
            {
                return 0;
            }

            var type = typeof(T);
            var tableInfo = RMDiscoveryDBTableManager.Get(type);

            var autoIncrementalKeyColumn = tableInfo.Columns.FirstOrDefault(item => item.IsKey && item.NeedAutoIncremental);

            var properties = type.GetProperties().Where(item => item.GetAttribute<NotMappedAttribute>() == null && item.Name != autoIncrementalKeyColumn.Name).ToList();

            List<(string ColumnName, Type ColumnType)> columnInfoes =
                properties
                .Select(item => (item.Name, item.PropertyType))
                .Concat(items.First().CustomColumns.Select(item => (item.Name, item.ValueType)))
                .ToList();

            var columnSql = "(";
            var valueSql = "(";
            foreach (var columnInfo in columnInfoes)
            {
                columnSql += $"{columnInfo.ColumnName},";
                valueSql += $"@{columnInfo.ColumnName},";
            }

            columnSql = columnSql.TrimEnd(',') + ")";
            valueSql = valueSql.TrimEnd(',') + ")";

            var sql = $@"INSERT INTO {SecurityUtils.SanitizeSQLParameterName(schema)}${SecurityUtils.SanitizeSQLParameterName(tableInfo.Name)} 
{columnSql} VALUES {valueSql}";

            for (var i = 0; i < items.Count; i += 1000)
            {
                var batchItems = items.Skip(i).Take(1000).ToList();
                using (var transaction = await _connection.BeginTransactionAsync())
                {
                    using (var command = GetSqlCommand())
                    {
                        command.CommandText = sql;
                        foreach (var (ColumnName, ColumnType) in columnInfoes)
                        {
                            command.Parameters.AddWithValue($"@{ColumnName}", GetDefaultValue(ColumnType));
                        }

                        foreach (var batchItem in batchItems)
                        {
                            foreach (var property in properties)
                            {
                                command.Parameters[$"@{property.Name}"].Value = property.GetValue(batchItem);
                            }

                            foreach (var customColumn in batchItem.CustomColumns)
                            {
                                command.Parameters[$"@{customColumn.Name}"].Value = customColumn.Value;
                            }

                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    await transaction.CommitAsync();
                }
            }

            return items.Count;
        }

        public async Task<T> ExecuteScalarAsync<T>(string sql, params SqlParameter[] parameters)
        {
            using var command = GetSqlCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(parameters);
            var res = await command.ExecuteScalarAsync();
            if (res == null)
            {
                return default;
            }
            return (T)res;
        }

        private static object GetDefaultValue(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        private SQLiteCommand GetSqlCommand()
        {
            var command = _connection.CreateCommand();
            command.CommandTimeout = Timeout;
            return command;
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }
    }
}
