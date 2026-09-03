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
using System.Data;
using System.Data.Common;
using System.Linq;
using AvePoint.RA.CommonUtil;
using RACommon.SQLiteDatabase.Extensions;


using Util;

public class SQLiteDatabaseHelper(String filePath, String? password = null) : ISQLiteDatabaseHelper
{
    private static readonly RALogger logger = RALogger.GetInstance(typeof(SQLiteDatabaseHelper));

    private readonly SQLiteExecutor sqliteDatabaseHelper = new(filePath, password);

    private bool isOpen;

    public void BatchExecuteNonQuery(Dictionary<String, Dictionary<String, Object>> commandInfoList) =>
        ExecuteWithTransaction((transaction) =>
            commandInfoList.ForEach(parameter => sqliteDatabaseHelper.ExecuteNonQuery(parameter.Key, parameter.Value, transaction)));

    public void ExecuteNonQuery(String commandText, Dictionary<String, Object> parameters) =>
        sqliteDatabaseHelper.ExecuteNonQuery(commandText, parameters);

    public void ExecuteNonQuery<T>(List<T> entities)
        where T : IInsertable =>
        ExecuteWithTransaction((transaction) =>
            entities.ForEach(entity =>
                sqliteDatabaseHelper.ExecuteNonQuery(
                    entity.ToInsertCommand(),
                    entity.GenerateInsertDatabaseParameters(),
                    transaction)));

    public void ExecuteNonQueryWithCommandPrepare<T>(List<T> entities)
        where T : IInsertable =>
        throw new NotImplementedException();

    public List<T> QueryForAllClass<T>(String commandText, Dictionary<String, Object> parameters)
        where T : class
    {
        var result = new List<T>();
        sqliteDatabaseHelper.ExecuteReader(reader => result.Add((reader as DbDataReader)!.ToM<T>()), commandText, parameters);
        return result;
    }

    public List<T> QuerySingleField<T>(String commandText, Dictionary<String, Object> parameters = null!)
        where T : class
    {
        var result = new List<T>();
        sqliteDatabaseHelper.ExecuteReader(reader => result.Add((reader.GetValue(0) as T)!), commandText, parameters);
        return result;
    }

    public DataTable QueryDataTable(String commandText, Dictionary<String, Object> parameters)
    {
        var result = new DataTable();
        var first = true;
        sqliteDatabaseHelper.ExecuteReader(reader =>
        {
            var row = result.NewRow();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (first)
                {
                    result.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
                }
                row[reader.GetName(i)] = reader.GetValue(i);
            }
            result.Rows.Add(row);
            first = false;
        }, commandText, parameters);
        return result;
    }

    public List<T> Query<T>(String commandText, Dictionary<String, Object> parameters)
        where T : IInsertable
    {
        var result = new List<T>();
        sqliteDatabaseHelper.ExecuteReader(reader => result.Add((reader as DbDataReader)!.ToEntity<T>()), commandText, parameters);
        return result;
    }

    [Obsolete("Same with ExecuteReader<T>, do not use IEnumerable lazy loading to reduce the result data, but to input an exact sql command")]
    public IEnumerable<T> QueryEnumerable<T>(String commandText, Dictionary<String, Object> parameters)
        where T : IInsertable =>
        Query<T>(commandText, parameters);

    public object ExecuteScalar(string commandText, Dictionary<String, Object> parameters = null!) =>
        sqliteDatabaseHelper.ExecuteScalar<Object>(commandText, parameters);

    public DbDataReader GetExecuteReader(String commandText, Dictionary<String, Object> parameters) =>
        throw new NotImplementedException();

    [Obsolete("Same with QuerySingleField<T>, do not use IEnumerable lazy loading to reduce the result data, but to input an exact sql command")]
    public IEnumerable<T> QuerySingleFieldEnumerable<T>(String commandText, Dictionary<String, Object> parameters = null!)
        where T : class =>
        QuerySingleField<T>(commandText, parameters);

    [Obsolete("In order to be compatible with the old implementation, a variable control switch is defined")]
    public Boolean IsOpen => isOpen;

    [Obsolete("In order to be compatible with the old implementation, a variable control switch is defined")]
    public void Open(string connectionString) => isOpen = true;

    public void Close()
    {
        isOpen = false;
        sqliteDatabaseHelper.Close();
    }

    private void ExecuteWithTransaction(Action<TransactionContext> action)
    {
        TransientFaultHandler.Process(() =>
        {
            var transaction = sqliteDatabaseHelper.CreateTransactionContext();
            try
            {
                action(transaction);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while batch executing in transaction, error: {0}.", ex);
                transaction.Rollback();
                throw;
            }
            finally
            {
                transaction?.Dispose();
            }
        });
    }
}
