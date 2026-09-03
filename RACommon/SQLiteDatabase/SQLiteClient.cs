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

namespace RACommon.SQLiteDatabase;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;

using Dapper;

using Util;


using RACommon.SQLiteDatabase.Core;
using AvePoint.RA.CommonUtil;
using System.Threading.Tasks;

public class SQLiteClient(String filePath, String? password = null, bool withWal = true) : IDisposable, IAsyncDisposable
{
    private static readonly RALogger logger = RALogger.GetInstance(typeof(SQLiteClient));

    private readonly SQLiteExecutor executor = new(filePath, password, withWal);

    public async Task ExecuteNonQueryAsync(String commandText, Dictionary<String, Object>? parameters = null) =>
        await executor.ExecuteNonQueryAsync(commandText, parameters);

    public async Task BatchExecuteNonQueryAsync(List<(String CommandText, Dictionary<String, Object>? Parameters)> commandInfos)
    {
        await ExecuteWithTransactionAsync(async (transaction) =>
        {
            foreach (var commandInfo in commandInfos)
            {
                await executor.ExecuteNonQueryAsync(commandInfo.CommandText, commandInfo.Parameters, transaction);
            }
        });
    }

    public async Task BatchExecuteNonQueryAsync(String commandText, IEnumerable<Dictionary<String, Object>>? parameters = null) =>
        await executor.BatchExecuteNonQueryAsync(commandText, parameters);

    public async Task InsertAsync<T>(T obj) where T : notnull
    {
        await TransientFaultHandler.ProcessAsync(async () =>
        {
            var sql = Mapper.GetInsertSql(typeof(T));
            using var connection = await executor.GetConnectionAsync();
            await connection.ExecuteAsync(sql, obj);
        });
    }

    public async Task BatchInsertAsync<T>(List<T> list) where T : notnull
    {
        await ExecuteWithTransactionAsync(async (connection, transaction) =>
        {
            var sql = Mapper.GetInsertSql(typeof(T));
            foreach (var obj in list)
            {
                await connection.ExecuteAsync(sql, obj, transaction);
            }
        });
    }


    public async Task UpdateAsync<T>(T obj) where T : notnull
    {
        await TransientFaultHandler.ProcessAsync(async () =>
        {
            var sql = Mapper.GetUpdateSql(typeof(T));
            using var connection = await executor.GetConnectionAsync();
            await connection.ExecuteAsync(sql, obj);
        });
    }

    public async Task BatchUpdateAsync<T>(List<T> list) where T : notnull
    {
        await ExecuteWithTransactionAsync(async (connection, transaction) =>
        {
            var sql = Mapper.GetUpdateSql(typeof(T));
            foreach (var obj in list)
            {
                await connection.ExecuteAsync(sql, obj, transaction);
            }
        });
    }

    public async Task UpsertAsync<T>(T obj) where T : notnull
    {
        await TransientFaultHandler.ProcessAsync(async () =>
        {
            var sql = Mapper.GetUpsertSql(typeof(T));
            using var connection = await executor.GetConnectionAsync();
            await connection.ExecuteAsync(sql, obj);
        });
    }

    public async Task BatchUpsertAsync<T>(List<T> list) where T : notnull
    {
        await ExecuteWithTransactionAsync(async (connection, transaction) =>
        {
            var sql = Mapper.GetUpsertSql(typeof(T));
            foreach (var obj in list)
            {
                await connection.ExecuteAsync(sql, obj, transaction);
            }
        });
    }

    public async Task DeleteAsync<T>(T obj) where T : notnull
    {
        await TransientFaultHandler.ProcessAsync(async () =>
        {
            var sql = Mapper.GetDeleteSql(typeof(T));
            using var connection = await executor.GetConnectionAsync();
            await connection.ExecuteAsync(sql, obj);
        });
    }


    public async Task BatchDeleteAsync<T>(List<T> list) where T : notnull
    {
        await ExecuteWithTransactionAsync(async (connection, transaction) =>
        {
            var sql = Mapper.GetDeleteSql(typeof(T));
            foreach (var obj in list)
            {
                await connection.ExecuteAsync(sql, obj, transaction);
            }
        });
    }


    public async Task<List<T>> QueryAsync<T>(String? commandText = null, Dictionary<String, Object>? parameters = null) where T : notnull
    {
        if (commandText.IsNullOrEmpty())
        {
            commandText = $"SELECT * FROM {Mapper.GetTableName(typeof(T))}";
        }

        return await TransientFaultHandler.ProcessAsync(async () =>
        {
            Mapper.SetTypeMap(typeof(T));
            var param = parameters?.Count > 0 ? new DynamicParameters() : null;
            parameters?.ForEach(p => param!.Add(p.Key, p.Value));
            using var connection = await executor.GetConnectionAsync();
            return (await connection.QueryAsync<T>(commandText, param)).ToList();
        });
    }

    public AsyncPageable<T> QueryPageAsync<T>(String? commandText = null, Dictionary<String, Object>? parameters = null, Int32? pageSize = 500) where T : notnull
    {
        if (commandText.IsNullOrEmpty())
        {
            commandText = $"SELECT * FROM {Mapper.GetTableName(typeof(T))}";
        }

        var offset = 0;
        var pageSql = "LIMIT {0}, {1}";

        return new AsyncPageable<T>(async sql =>
        {
            var result = await QueryAsync<T>(sql, parameters);
            return new Page<T>(result.Count >= pageSize ? $"{commandText} {pageSql.FormatWith(++offset * pageSize, pageSize)}" : null, result);
        }, $"{commandText} {pageSql.FormatWith(offset, pageSize)}");
    }

    public async Task<Object> ExecuteScalarAsync(String commandText, Dictionary<String, Object>? parameters = null) =>
        await executor.ExecuteScalarAsync<Object>(commandText, parameters);

    public async ValueTask CloseAsync() => await executor.CloseAsync();

    public void Dispose() => executor.Close();

    public async ValueTask DisposeAsync() => await CloseAsync();

    private async Task ExecuteWithTransactionAsync(Func<TransactionContext, Task> func)
    {
        await TransientFaultHandler.ProcessAsync(async () =>
        {
            using var transaction = await executor.CreateTransactionContextAsync();
            try
            {
                await func(transaction);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while batch executing in transaction, error: {0}.", ex);
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    private async Task ExecuteWithTransactionAsync(Func<DbConnection, DbTransaction, Task> func)
    {
        await TransientFaultHandler.ProcessAsync(async () =>
        {
            using var connection = await executor.GetConnectionAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                await func(connection, transaction);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while batch executing in transaction, error: {0}.", ex);
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
}