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
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public static class SQLCommond
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(SQLCommond));
        private const int PageSite = 5000;
        private const int defaultCommandTimeout = 60 * 5;
        public static int ExecuteNonQuery(string dataSource, string sqlString)
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + dataSource))
            {
                int state = 0;
                try
                {
                    conn.Open();
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = sqlString;
                        cmd.CommandTimeout = defaultCommandTimeout;
                        state = cmd.ExecuteNonQuery();
                    }
                }
                catch (SQLiteException e)
                {
                    logger.Error($"Execute sqlLite error,report file path:{dataSource} ,sql string:{sqlString} ,error message:{e.Message}");
                    state = -1;
                }
                finally
                {
                    conn.Close();
                }
                return state;
            }
        }

        public static bool BatchExecuteNonQueryStable(string dataSource, string sqlString, List<List<SQLiteParameter>> parametersList)
        {
            try
            {
                while (parametersList.Count > 0)
                {
                    int pageSize = parametersList.Count > PageSite ? PageSite : parametersList.Count;
                    List<List<SQLiteParameter>> param = parametersList.GetRange(0, pageSize);

                    BatchExecuteNonQuery(dataSource, sqlString, param);

                    parametersList.RemoveRange(0, pageSize);
                }
                return true;
            }
            catch (SQLiteException e)
            {
                logger.Warn($"Batch execute nonQuery stable fail,data source :{dataSource}, sqlString :{sqlString}, error message:{e.Message}");
                return false;
            }
        }

        private static void BatchExecuteNonQuery(string dataSource, string sqlString, List<List<SQLiteParameter>> parametersList)
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + dataSource))
            {
                try
                {
                    conn.Open();
                    SQLiteTransaction transaction = conn.BeginTransaction();
                    try
                    {

                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            foreach (List<SQLiteParameter> parameters in parametersList)
                            {
                                cmd.CommandText = sqlString;
                                cmd.Parameters.AddRange(parameters.ToArray<SQLiteParameter>());
                                cmd.ExecuteNonQuery();
                            }
                            if (parametersList.Count > 0)
                            {
                                transaction.Commit();
                            }
                        }
                    }
                    catch (SQLiteException)
                    {
                        transaction.Rollback();
                        throw;
                    }
                    finally
                    {
                        transaction.Dispose();
                    }
                   
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        /*public static void ExecuteNonQuery(string dataSource, string sqlString, IEnumerable<SQLiteParameter> parameters)
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + dataSource))
            {
                try
                {
                    conn.Open();
                    SQLiteTransaction transaction = conn.BeginTransaction();
                    try
                    {

                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = sqlString;
                            cmd.Parameters.AddRange(parameters.ToArray());
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (SQLiteException ex)
                    {
                        logger.Error(ex.StackTrace);
                        transaction.Rollback();
                        throw;
                    }
                }
                finally
                {
                    conn.Close();
                }
            }
        }*/

        /// <summary>
        /// 检验表是否存在
        /// </summary>
        /// <returns></returns>
        public static bool IsExistTable(string reportFilePath,string tableName)
        {
            if (SecurityUtils.ValidateSQLiteConnectionWithBuilder(reportFilePath, out var builder))
            {
                using (SQLiteConnection conn = new SQLiteConnection(builder.ConnectionString))
                {
                    conn.Open();
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"SELECT COUNT(*) FROM sqlite_master WHERE name = @tableName";
                        cmd.Parameters.AddWithValue("@tableName", tableName);
                        return Convert.ToInt32(cmd.ExecuteScalar()) > 0 ? true : false;
                    }
                }
            }
            return false;
        }

        public static (bool ExactMatch, List<string> ExistColumns, List<string> MissingColumns) CheckColumnsExistenceInTable(string reportFilePath, string tableName, List<string> columnsToCheck)
        {
            if (!SecurityUtils.ValidateSQLiteConnectionWithBuilder(reportFilePath, out var builder)) 
            {
                return (false, [], columnsToCheck);
            }
            string sql = $"PRAGMA table_info({tableName})";

            List<string> existingColumns = [];
            List<string> missingColumns = [];
            HashSet<string> tempColumns = new(StringComparer.OrdinalIgnoreCase);
            bool exactMatch = true;

            using (SQLiteConnection conn = new(builder.ConnectionString))
            {
                conn.Open();
                using SQLiteCommand command = new(sql, conn);
                using SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string currentColumnName = reader["name"].ToString();
                    tempColumns.Add(currentColumnName);
                }
            }

            foreach (string columnName in columnsToCheck)
            {
                if (tempColumns.Contains(columnName))
                {
                    existingColumns.Add(columnName);
                    continue;
                }

                exactMatch = false;
                missingColumns.Add(columnName);
            }

            return (exactMatch, existingColumns, missingColumns);
        }

        public static bool CheckOneColumnExistenceInTable(string reportFilePath, string tableName, string columnToCheck)
        {
            if (!SecurityUtils.ValidateSQLiteConnectionWithBuilder(reportFilePath, out var builder))
            {
                return false;
            }
            string sql = $"PRAGMA table_info({SecurityUtils.SanitizeSQLSchemaName(tableName)})";

            using (SQLiteConnection conn = new(builder.ConnectionString))
            {
                conn.Open();
                using SQLiteCommand command = new(sql, conn);
                using SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string currentColumnName = reader["name"].ToString();
                    
                    if (string.Equals(currentColumnName, columnToCheck, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 剔除在DB中不存在的Column
        /// </summary>
        /// <param name="reportFilePath"></param>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static List<string> FilterColumns(string reportFilePath, string tableName, List<string> columnNames)
        {
            var newColumnNames = new List<string>();
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + reportFilePath))
                {
                    conn.Open();
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        foreach (var colName in columnNames)
                        {
                            cmd.CommandText = $"SELECT COUNT(*) FROM sqlite_master WHERE name = @tableName AND sql LIKE @colName";
                            cmd.Parameters.AddWithValue("@tableName", tableName);
                            cmd.Parameters.AddWithValue("@colName", $"%{colName}%");
                            if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                            {
                                newColumnNames.Add(colName);
                            }
                            else
                            {
                                logger.Info($"rpt file not contains {colName} column.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"An error occurred in FilterColumns, message:{ex.Message}");
                return columnNames;
            }
            return newColumnNames;
        }

        public static bool CanConnectToReportFile(string path)
        {
            if (SecurityUtils.ValidateSQLiteConnectionWithBuilder(path, out var builder))
            {
                using (SQLiteConnection conn = new SQLiteConnection(builder.ConnectionString))
                {
                    try
                    {
                        conn.Open();
                        SQLiteCommand cmd = conn.CreateCommand();
                        cmd.CommandText = @"SELECT COUNT(*) FROM sqlite_master";
                        var rst = Convert.ToInt32(cmd.ExecuteScalar()) > 0 ? true : false;
                        cmd.Dispose();
                        return rst;
                    }
                    catch (Exception ex)
                    {
                        logger.Warn($"Connect to the report failed,path:{path}, error message: {ex}");
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            return false;
        }

        public static long GetRowCount(string dataSource, string tableName)
        {
            if (!SecurityUtils.ValidateSQLiteConnectionWithBuilder(dataSource, out var builder))
            {
                return 0;
            }
            using SQLiteConnection conn = new(builder.ConnectionString);
            try
            {
                conn.Open();
                using SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT COUNT(*) FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)}";
                cmd.CommandTimeout = defaultCommandTimeout;
                return Convert.ToInt64(cmd.ExecuteScalar());
            }
            catch (SQLiteException ex)
            {
                logger.Warn($"GetRowCount failed, table: {tableName}, error: {ex.Message}");
                return 0;
            }
            finally
            {
                conn.Close();
            }
        }

        public static async Task<List<T>> ExecuteQueryAsync<T> (string dataSource, string sqlString, params SQLiteParameter[] parameters) where T : class
        {
            List<T> result = new();
            using (SQLiteConnection conn = new($"Data Source={dataSource}"))
            {
                try
                {
                    await conn.OpenAsync();
                    using SQLiteCommand cmd = conn.CreateCommand();
                    cmd.CommandText = sqlString;
                    cmd.CommandTimeout = defaultCommandTimeout;
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        T item = Activator.CreateInstance<T>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var property = typeof(T)
                                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                .Where(p => p.Name == reader.GetName(i))
                                .OrderByDescending(p => p.DeclaringType == typeof(T))
                                .FirstOrDefault();
                            if (property != null && !Convert.IsDBNull(reader[i]))
                            {
                                var value = reader[i];
                                var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                                if (targetType == typeof(DateTime))
                                {
                                    value = new DateTime((long)value, DateTimeKind.Utc);
                                }
                                else if (targetType.IsEnum)
                                {
                                    value = Enum.ToObject(targetType, value);
                                }
                                else
                                {
                                    value = Convert.ChangeType(value, targetType);
                                }
                                property.SetValue(item, value);
                            }
                        }
                        result.Add(item);
                    }
                }
                catch (SQLiteException e)
                {
                    logger.Error($"Execute SQLite error, report file path: {dataSource}, error message: {e.Message}");
                }
                finally
                {
                    await conn.CloseAsync();
                }
            }
            return result;
        }
    }
}
