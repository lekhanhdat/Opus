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
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;

//using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.ObjectModel.ClientOM
{
    public class LastAccessTimeSqliteDBUtility : IDisposable
    {
        public static LastAccessTimeSqliteDBUtility instance;
        private static readonly object padlock = new object();
        protected string dbName;
        private string dbdirPath;
        public string dbFilePath;
        private string tableName = string.Empty;
        private bool hasDownLoadSASFile = false;
        private bool hasCurrentSCTable = false;
        private const string TEMPFOLDERNAME = "LATReport";
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(LastAccessTimeSqliteDBUtility));

        private LastAccessTimeSqliteDBUtility(string tenantGroupId, string latSASString, string tableName)
        {
            var timer = Stopwatch.StartNew();
            string currentJobGuid = Guid.NewGuid().ToString();
            this.tableName = tableName;
            dbdirPath = SecurityUtils.SafeCombinePath(SecurityUtils.SafeCombinePath(AveEnv.AgentTempFolder, TEMPFOLDERNAME), tenantGroupId, "CloudInsightsLATDB_" + currentJobGuid);
            if (!Directory.Exists(dbdirPath))
            {
                mLogger.Info($"Temp Path:{dbdirPath}");
                DirectoryInfo dbdir = new DirectoryInfo(dbdirPath);
                try
                {
                    dbdir.Create();
                }
                catch (Exception e)
                {
                    mLogger.Error($"LastAccessTimeSqliteDBUtility Create DirectoryInfo failed.dbdirPath:{dbdirPath}.Message:{e.ToString()}.");
                }
            }
            dbName = tableName + "_" + currentJobGuid + "_lat.db";
            dbFilePath = SecurityUtils.SafeCombinePath(dbdirPath, dbName);
            mLogger.Info($"LastAccessTimeSqliteDBUtility dbFilePath:{dbFilePath}.");
            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(latSASString, dbFilePath);
                    hasDownLoadSASFile = true;
                    mLogger.Info($"LastAccessTimeSqliteDBUtility success DownloadFile.");
                }
            }
            catch (Exception ex)
            {
                mLogger.Error($"LastAccessTimeSqliteDBUtility failed DownloadFile.Message:{ex.ToString()}.");
            }
            timer.Stop();
            mLogger.Info($"download time count: {timer.ElapsedMilliseconds} ms");
            CheckCurrentSiteCollectionTableExist();
        }

        public static LastAccessTimeSqliteDBUtility GetInstance(string tenantGroupId, string latSASString, string tableName)
        {
            if (instance == null)
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new LastAccessTimeSqliteDBUtility(tenantGroupId, latSASString, tableName);
                    }
                }
            }
            return instance;
        }

        public virtual SQLiteConnection GetConnection()
        {
            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = dbFilePath;
            return new SQLiteConnection(builder.ToString());
        }

        public DateTime SelectItemLastAccessedTimeFromSqliteDB(IDbCommand command, Guid itemId)
        {
            DateTime itemLAT = DateTime.MinValue;
            //SAS File没有下载成功，不执行Query
            if (hasDownLoadSASFile && hasCurrentSCTable)
            {
                string query = string.Format($"SELECT [LastAccessedTime] FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} WHERE [ItemId] = @ItemId");
                command.CommandText = query;
                var itemIdPs = new SQLiteParameter("@ItemId");
                itemIdPs.Value = itemId.ToString();
                command.Parameters.Add(itemIdPs);
                using (var sr = command.ExecuteReader())
                {
                    if (sr.Read())
                    {
                        itemLAT = sr.GetDateTime(0);
                        return itemLAT;
                    }
                }
            }
            return itemLAT;
        }

        public Dictionary<string, DateTime> SelectItemsLastAccessedTimeFromSqliteDB(IDbCommand command, List<string> itemIds)
        {
            Dictionary<string, DateTime> keyValuePairs = new Dictionary<string, DateTime>();
            // 假设 itemIds 是 List<string> 类型（要查询的条件集合）
            if (itemIds == null || itemIds.Count == 0)
            {
                // 处理空集合的情况（如直接返回，避免无效查询）
                return keyValuePairs;
            }
            // 1. 动态生成参数占位符（@p0, @p1, ..., @pn）
            var paramNames = new List<string>();
            for (int i = 0; i < itemIds.Count; i++)
            {
                paramNames.Add($"@p{i}"); // 生成参数名，如 @p0, @p1...
            }
            // 2. 构建带动态参数的 SQL 语句
            string sqlQuery = $"SELECT [ItemID],[LastAccessedTime] FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} WHERE [ItemId] IN ({string.Join(",", paramNames)})";
            if (hasDownLoadSASFile && hasCurrentSCTable)
            {
                command.CommandText = sqlQuery;
                // 3. 为每个参数添加对应的值
                for (int i = 0; i < itemIds.Count; i++)
                {
                    // 创建参数（指定参数名和值）
                    SQLiteParameter param = new SQLiteParameter(paramNames[i], itemIds[i]);
                    // 添加到命令参数集合
                    command.Parameters.Add(param);
                }
                // 4. 执行查询
                using (var sr = command.ExecuteReader())
                {
                    while (sr.Read())
                    {
                        // 建议通过列名获取值（更健壮，避免列顺序变化导致错误）
                        if (!sr.IsDBNull(sr.GetOrdinal("ItemID")) && !sr.IsDBNull(sr.GetOrdinal("LastAccessedTime")))
                        {
                            string itemId = sr.GetString(sr.GetOrdinal("ItemID"));
                            DateTime dateTime = sr.GetDateTime(sr.GetOrdinal("LastAccessedTime"));
                            keyValuePairs.Add(itemId, dateTime);
                        }
                    }
                }
            }
            return keyValuePairs;
        }

        public void ExecuteQueryWithAction(Action<SQLiteConnection> action)
        {
            ExecuteWithConnection(action);
        }

        protected void ExecuteWithConnection(Action<SQLiteConnection> action)
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                action(connection);
                try
                {
                    Directory.SetLastAccessTimeUtc(dbdirPath, DateTime.UtcNow);
                }
                catch (Exception e)
                {
                    mLogger.Warn($@"fail set dir last accesstime utc,ex:{e}");
                }
            }
        }

        public static void ClearInstance()
        {
            if (instance != null)
            { 
                instance = null;
            }
        }
        private void CheckCurrentSiteCollectionTableExist()
        {
            try
            {
                if (hasDownLoadSASFile)
                {
                    using (var connection = GetConnection())
                    {
                        connection.Open();
                        using (var command = connection.CreateCommand())
                        {
                            string query = string.Format($"SELECT count(*) FROM sqlite_master WHERE type='table' AND name = @TableName");
                            command.CommandText = query;
                            var itemIdPs = new SQLiteParameter("@TableName");
                            itemIdPs.Value = tableName;
                            command.Parameters.Add(itemIdPs);
                            using (var sr = command.ExecuteReader())
                            {
                                while (sr.Read())
                                {
                                    if (sr.GetInt32(0) > 0)
                                    {
                                        mLogger.Info($"Current Site Collection table exist:{tableName}.");
                                        hasCurrentSCTable = true;
                                    }
                                    else
                                    {
                                        mLogger.Info($"Current Site Collection table does not exist:{tableName}.");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Error($"CheckCurrentSiteCollectionTableExist failed.Message:{ex.ToString()}.");
            }
        }

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(dbdirPath) && Directory.Exists(dbdirPath))
            {
                try
                {
                    Directory.Delete(dbdirPath, true);
                }
                catch (Exception e)
                {
                    mLogger.Warn($"Failed to delete temp folder {dbdirPath} error {e}");
                }
            }
        }
    }
}
