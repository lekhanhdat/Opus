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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.RA.DB.Core.Synchronize.DbContext.Base;
using AvePoint.RA.DB.Core.Synchronize.DbContext.TypeMapper;
using AvePoint.RA.DB.Core.Synchronize.DbManager;
using AvePoint.RA.DB.Model;
using System.Data.SQLite;
using System.Reflection;
using AvePoint.RA.DB.Core.Discovery.Context;


namespace AvePoint.RA.DB.Core.Synchronize.DbContext;

public class SqliteSynchronizeDbContext : ISynchronizeDbContext
{
    public int Timeout { get; set; } = 60 * 10;
    
    private SQLiteConnection _connection;

    public async Task OpenAsync(string connectionString)
    {
        _connection = new SQLiteConnection($"DataSource={connectionString};Version=3");
        await _connection.OpenAsync();
    }

    public async Task<T> ExecuteScalarAsync<T>(string sql, params SQLiteParameter[] parameters)
    {
        await using var command = GetSqlCommand();
        command.CommandText = sql;
        command.Parameters.AddRange(parameters);
        var res = await command.ExecuteScalarAsync();
        if (res == null)
        {
            return default;
        }
        return (T)res;
    }

    public async Task<int> ExecuteNonQueryAsync(string sql, params SQLiteParameter[] parameters)
    {
        await using var command = GetSqlCommand();
        command.CommandText = sql;
        command.Parameters.AddRange(parameters);
        return await command.ExecuteNonQueryAsync();
    }

    public async IAsyncEnumerable<T> ExecuteQueryAsync<T>(string sql, params SQLiteParameter[] parameters)
    {
        await using var command = GetSqlCommand();
        command.CommandText = sql;
        command.Parameters.AddRange(parameters);
        if(!sql.Contains("LIMIT") && !sql.Contains("OFFSET"))
        {
            sql += " LIMIT @PageSize OFFSET @OFFSET";
        }
        for (var i = 0; ; i += 10_000)
        {
            var dataCollection = await ExecuteQueryAsync(sql,
            [
                .. parameters,
                new SQLiteParameter("@PageSize", 10_000),
                new SQLiteParameter("@OFFSET", i)
            ]);

            var dataList = dataCollection.ToTableList<T>();
            foreach (var data in dataList)
            {
                yield return data;
            }

            if (dataList.Count < 10_000)
            {
                break;
            }
        }
    }

    public async Task<RMSynchronizeDataCollection> ExecuteQueryAsync(string sql, params SQLiteParameter[] parameters)
    {
        await using var command = GetSqlCommand();
        command.CommandText = sql;
        command.Parameters.AddRange(parameters);
        await using var reader = await command.ExecuteReaderAsync();
        var dataList = new List<Dictionary<string, RMSynchronizeTableDataFieldInfo>>();
        while (await reader.ReadAsync())
        {
            var fieldCount = reader.FieldCount;
            var row = new Dictionary<string, RMSynchronizeTableDataFieldInfo>(fieldCount);
            for (var i = 0; i < fieldCount; i++)
            {
                var fieldType = reader.GetFieldType(i);
                var fieldName = reader.GetName(i);
                var fieldValue = reader.GetValue(i);
                row.Add(fieldName, new(fieldType, fieldName, fieldValue));
            }
            dataList.Add(row);
        }

        return  new(dataList);    
    }

    public async Task<int> ExecuteInsertAsync<T>(IEnumerable<T> items)
    {
        if (items == null || !items.Any())
        {
            return 0;
        }

        var schema = RMSynchronizeDbManager.GetSchemaName();

        var type = typeof(T);
        var tableInfo = RMSynchronizeDbTableMapper.Get(typeof(T));
        
        var properties = type.GetProperties().Where(item =>
            item.GetAttribute<NotMappedAttribute>() == null).ToList();

        List<(string ColumnName, Type ColumnType)> columnInfos =
            properties
                .Select(item => (item.Name, item.PropertyType))
                .ToList();

        var columnSql = "(";
        var valueSql = "(";
        foreach (var columnInfo in columnInfos)
        {
            columnSql += $"{columnInfo.ColumnName},";
            valueSql += $"@{columnInfo.ColumnName},";
        }

        columnSql = columnSql.TrimEnd(',') + ")";
        valueSql = valueSql.TrimEnd(',') + ")";

        var sql =
            $@"INSERT INTO {SecurityUtils.SanitizeSQLParameterName(schema)}${SecurityUtils.SanitizeSQLParameterName(tableInfo.Name)} 
{columnSql} VALUES {valueSql}";

        for (var i = 0; i < items.Count(); i += 1000)
        {
            await using var transaction = await _connection.BeginTransactionAsync();
            try
            {
                var batchItems = items.Skip(i).Take(1000).ToList();
                await using (var command = GetSqlCommand())
                {
                    command.CommandText = sql;
                    foreach (var (columnName, columnType) in columnInfos)
                    {
                        command.Parameters.AddWithValue($"@{columnName}", GetDefaultValue(columnType));
                    }

                    foreach (var batchItem in batchItems)
                    {
                        foreach (var property in properties)
                        {
                            command.Parameters[$"@{property.Name}"].Value = property.GetValue(batchItem);
                        }


                        await command.ExecuteNonQueryAsync();
                    }
                }

                await transaction.CommitAsync();
            }catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error inserting batch of {typeof(T).Name}: {ex.Message}", ex);
            }
        }

        return items.Count();
    }

    public List<ISynchronizeDbTableSet> GetTableSets(string schemaName)
    {
        return [
            new RMSynchonizeSqliteDbTableSet(typeof(RMRemoteNode), schemaName),
            new RMKeyValueSqliteDbTableSet(typeof(RMKeyValue), schemaName)
        ];
    }

    public void CreateDatabase(string dbPath)
    {
        SQLiteConnection.CreateFile(dbPath);
    }

    private static object GetDefaultValue(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    private SQLiteCommand GetSqlCommand()
    {
        var command = _connection.CreateCommand();
        command.CommandTimeout = Timeout;
        return command;
    }

    public async Task CloseAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
    }
    
    public async ValueTask DisposeAsync()
    {
        await CloseAsync();
    }
}

