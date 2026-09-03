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
using AvePoint.RA.Contract.Services;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace AvePoint.RA.Common.Database
{
    public static class SQLiteUtil
    {
        private const int PageSite = 5000;
        public static int ExecuteNonQuery(IRALogger logger, string dataSource, string sqlString)
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
                        state = cmd.ExecuteNonQuery();
                    }
                }
                catch (SQLiteException e)
                {
                    logger.Error("Execute sqlLite error:{0}", e.Message);
                    state = -1;
                }
                finally
                {
                    conn.Close();
                }
                return state;
            }
        }

        public static void BatchExecuteNonQueryStable(IRALogger logger, string dataSource, string sqlString, List<List<SQLiteParameter>> parametersList)
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
            }
            catch (SQLiteException e)
            {
                logger.Warn(e.Message);
            }
        }

        private static void BatchExecuteNonQuery(string dataSource, string sqlString, List<List<SQLiteParameter>> parametersList)
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + dataSource))
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
        }

       /* public static void ExecuteNonQuery(IRALogger logger, string dataSource, string sqlString, IEnumerable<SQLiteParameter> parameters)
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + dataSource))
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
        }*/

        /// <summary>
        /// 检验表是否存在
        /// </summary>
        /// <returns></returns>
        public static bool IsExistTable(string reportFilePath, string tableName)
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + reportFilePath))
            {
                conn.Open();
                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT COUNT(*) FROM sqlite_master WHERE name = @tableName";
                    cmd.Parameters.AddWithValue("@tableName", tableName);
                    //logger.Info("TABLE_NAME  : " + tableName);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0 ? true : false;
                }
            }
        }

        /// <summary>
        /// 剔除在DB中不存在的Column
        /// </summary>
        /// <param name="reportFilePath"></param>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static List<string> FilterColumns(IRALogger logger, string reportFilePath, string tableName, List<string> columnNames)
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

        public static bool CanConnectToReportFile(IRALogger logger, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + path))
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
                    logger.Error("Connect to the report failed, the report path: {0}, error message: {1}", path, ex.ToString());
                    return false;
                }
            }
        }
    }
}
