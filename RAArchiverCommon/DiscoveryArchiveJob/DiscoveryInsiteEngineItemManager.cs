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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.SharePoint.ArchiverCommon;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using RAArchiverCommon.DestructionCache;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Duende.IdentityModel.OidcConstants;

namespace RAArchiverCommon.DiscoveryArchiveJob
{
    public class DiscoveryInsiteEngineItemManager : SqliteDBBase, IDisposable
    {
        private static AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(DiscoveryInsiteEngineItemManager));

        private static string CACHE_FODER_PATH = SecurityUtils.SafeCombinePath(System.Environment.CurrentDirectory, "DiscoveryScanTemp");

        private const string DB_NAME_PREFIX = "DiscovertyTemp";

        private const string DB_SUFFIX = ".rpt";

        private const string TABLE_NAME = "Items";

        private const string QEURY_SQL_BASE = $"SELECT [Id],[FullUrl],[ModifiedTime],[FileSize],[ItemId],[ListId],[WebId],[ItemUniqueId] FROM {TABLE_NAME} ";

        private const string GET_COUNT_SQL_BASE = $"SELECT Count(*) FROM {TABLE_NAME} ";

        private BlockingCollection<RMDiscoveryFileDataInfo> _queue = new BlockingCollection<RMDiscoveryFileDataInfo>();

        private string _dbName;

        private Thread _executeThread;

        private volatile Exception _executeThreadExcpetion;

        private volatile int _executeThreadEndFlag;

        private readonly object _locker = new object();

        public int ItemCountOfWaitInsert => _queue.Count;

        public bool ExecuteIsRunning => _executeThread.IsAlive && _executeThreadEndFlag == default;

        public static DiscoveryInsiteEngineItemManager GetInstance()
        {
            return new DiscoveryInsiteEngineItemManager(CACHE_FODER_PATH, DB_NAME_PREFIX + DateTime.Now.Ticks + DB_SUFFIX);
        }

        public long GetCoutOfData()
        {
            long res = 0;
            using (PerformanceScope pc = new PerformanceScope("DiscoveryInsiteEngineItemManager.GetCoutOfData", addToStatistics: true))
            {
                ExecuteWithConnectionNoUpateDicLastAccessTime(connection =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = GET_COUNT_SQL_BASE;
                        using (var sr = command.ExecuteReader())
                        {
                            sr.Read();
                            res = sr.GetInt64(0);
                        }
                    }
                });
            }
            return res;
        }

        private DiscoveryInsiteEngineItemManager(string dirPath, string name) : base(dirPath, name)
        {
            _dbName = name;
            _executeThread = new Thread(DoInsert) 
            { 
                IsBackground = true
            };
            _executeThread.Start();
        }


        public override void CreateSchemaIfNotExists(IDbCommand command)
        {
            string query = string.Format($"CREATE TABLE IF NOT EXISTS {SecurityUtils.SanitizeSQLSchemaName(TABLE_NAME)}(" +
                "[Id] [nvarchar](500) NOT NULL," +
                "[FullUrl] [nvarchar](500)," +
                "[ModifiedTime] [bigint] not null," +
                "[FileSize] [bigint] not null," +
                "[ItemId] [int] NOT NULL," +
                "[ListId] [nvarchar](500)," +
                "[WebId] [nvarchar](500)," +
                "[ItemUniqueId] [nvarchar](500) NOT NULL);" +
                $"CREATE INDEX itemUniqueIdIndex ON {TABLE_NAME}(ItemUniqueId);" +
                $"CREATE INDEX itemIdIndex ON {TABLE_NAME}(ItemId);");

            command.CommandText = query;

            command.ExecuteNonQuery();
        }

        public void DoInsert()
        {
            try
            {
                while (!_queue.IsCompleted)
                {
                    if (!_queue.Any())
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                    int count = _queue.Count;
                    for (int page = 0, size = 500; page * size < count; page++)
                    {
                        int insertCount = Math.Min(500, count - (page * size));
                        InsertValueToDB(_queue.Take(insertCount));
                        for (int i = 0; i < insertCount; i++)
                        {
                            _queue.Take();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lock (_locker)
                {
                    _executeThreadExcpetion = ex;
                }
                logger.Error(@$"Fail insert data into discovery item temp db:{_dbName}, ex:{ex}");
            }
            finally
            {
                lock (_locker)
                {
                    _executeThreadEndFlag = 1;
                }
                _queue?.Dispose();
            }
        }

        public void InsertValue(IEnumerable<RMDiscoveryFileDataInfo> datas)
        {
            while(ItemCountOfWaitInsert > 2000 && ExecuteIsRunning)
            {
                logger.Info($"Cached insite engine file is too many, count:{ItemCountOfWaitInsert}, will sleep 100 ms");
                Thread.Sleep(100);
            }

            foreach (var data in datas)
            {
                _queue.Add(data);
            }
        }

        public void InsertValueToDB(IEnumerable<RMDiscoveryFileDataInfo> destructionReports)
        {
            using (PerformanceScope pc = new PerformanceScope("DiscoveryInsiteEngineItemManager.InsertValueToDB", addToStatistics: true))
            {
                ExecuteWithConnectionNoUpateDicLastAccessTime(connection =>
                {
                    InternalInsertValueToDB(connection, destructionReports);
                });
            }
        }

        private void InternalInsertValueToDB(SQLiteConnection conn, IEnumerable<RMDiscoveryFileDataInfo> dataInfos)
        {
            using (SQLiteTransaction tr = conn.BeginTransaction())
            {
                using (var command = conn.CreateCommand())
                {
                    foreach (var dataInfo in dataInfos)
                    {
                        StringBuilder query = new StringBuilder();
                        query.Append($"INSERT INTO {SecurityUtils.SanitizeSQLSchemaName(TABLE_NAME)}" +
                                              $"(Id,FullUrl,ModifiedTime,FileSize,ItemId,ListId,WebId,ItemUniqueId) ");
                        query.Append(@"VALUES (@Id,@FullUrl,@ModifiedTime,@FileSize,@ItemId,@ListId,@WebId,@ItemUniqueId)");
                        SQLiteParameter[] parameters = {
                            new SQLiteParameter{ ParameterName = "@Id" , Value = dataInfo.Id},
                            new SQLiteParameter{ ParameterName = "@FullUrl" , Value = dataInfo.FullUrl},
                            new SQLiteParameter{ ParameterName = "@ModifiedTime" , Value = dataInfo.ModifiedTime.Ticks},
                            new SQLiteParameter{ ParameterName = "@FileSize" , Value = dataInfo.FileSize},
                            new SQLiteParameter{ ParameterName = "@ItemId" , Value = dataInfo.ItemId},
                            new SQLiteParameter{ ParameterName = "@ListId" , Value = dataInfo.ListId == null ? DBNull.Value : dataInfo.ListId},
                            new SQLiteParameter{ ParameterName = "@WebId" , Value = dataInfo.WebId == null ? DBNull.Value : dataInfo.WebId},
                            new SQLiteParameter{ ParameterName = "@ItemUniqueId" , Value = dataInfo.ItemUniqueId},
                            };
                        foreach (var para in parameters)
                        {
                            command.Parameters.Add(para);
                        }
                        command.CommandText = query.ToString();
                        command.ExecuteNonQuery();
                    }
                    tr.Commit();
                }
            }
        }

        public List<RMDiscoveryFileDataInfo> SelectValuesFromDBByItemUniqueIds(params string[] ids)
        {
            using (PerformanceScope pc = new PerformanceScope("DiscoveryInsiteEngineItemManager.SelectValuesFromDBByItemUniqueIds", addToStatistics: true))
            {
                List<RMDiscoveryFileDataInfo> reports = new List<RMDiscoveryFileDataInfo>();
                ExecuteWithConnectionNoUpateDicLastAccessTime(connection =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        reports.AddRange(InterSelectValuesByItemUniqueIds(command, ids));
                    }
                });
                return reports;
            }
        }

        private List<RMDiscoveryFileDataInfo> InterSelectValuesByItemUniqueIds(SQLiteCommand command,params string[] ids)
        {
            string query = QEURY_SQL_BASE + $"where ItemUniqueId in {DatabaseUtility.BuildInClause(ids, out List<SQLiteParameter> parameters)}";
            command.CommandText = query;
            command.Parameters.AddRange(parameters.ToArray());
            using (var sr = command.ExecuteReader())
            {
                return ParseResultFromReader(sr);
            }
        }

        public List<RMDiscoveryFileDataInfo> SelectValuesFromDBByItemIds(params int[] itemIds)
        {
            using (PerformanceScope pc = new PerformanceScope("DiscoveryInsiteEngineItemManager.SelectValuesFromDBByItemIds", addToStatistics: true))
            {
                List<RMDiscoveryFileDataInfo> reports = new List<RMDiscoveryFileDataInfo>();
                ExecuteWithConnectionNoUpateDicLastAccessTime(connection =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        reports.AddRange(InterSelectValuesByItemIds(command, itemIds));
                    }
                });
                return reports;
            }
        }

        private List<RMDiscoveryFileDataInfo> InterSelectValuesByItemIds(SQLiteCommand command,params int[] ItemIds)
        {
            string query = QEURY_SQL_BASE + $"where ItemId in {DatabaseUtility.BuildInClause(ItemIds, out List<SQLiteParameter> parameters)}";
            command.CommandText = query;
            command.Parameters.AddRange(parameters.ToArray());
            using (var sr = command.ExecuteReader())
            {
                return ParseResultFromReader(sr);
            }
        }

        public List<RMDiscoveryFileDataInfo> PageSelectValuesFromDB(int pageIndex, int pageSize)
        {
            using (PerformanceScope pc = new PerformanceScope("DiscoveryInsiteEngineItemManager.PageSelectValuesFromDB", addToStatistics: true))
            {
                List<RMDiscoveryFileDataInfo> reports = new List<RMDiscoveryFileDataInfo>();
                ExecuteWithConnectionNoUpateDicLastAccessTime(connection =>
                {
                   using (var command = connection.CreateCommand())
                    {
                        reports.AddRange(InternalSelectValuesFromDB4NewSchema(command, pageIndex * pageSize, pageSize));
                    }
                });
                return reports;
            }
        }

        private List<RMDiscoveryFileDataInfo> InternalSelectValuesFromDB4NewSchema(IDbCommand command, int offset, int pageSize)
        {;
            string query = QEURY_SQL_BASE + $" limit {pageSize} offset {offset}";
            command.CommandText = query;
            using (var sr = command.ExecuteReader())
            {
                return ParseResultFromReader(sr);
            }
        }

        private List<RMDiscoveryFileDataInfo> ParseResultFromReader(IDataReader reader)
        {
            List<RMDiscoveryFileDataInfo> res = new List<RMDiscoveryFileDataInfo>();
            while (reader.Read())
            {
                RMDiscoveryFileDataInfo info = new RMDiscoveryFileDataInfo();
                info.Id = reader.GetString(0);
                info.FullUrl = reader.GetString(1);
                info.ModifiedTime = new DateTime(reader.GetInt64(2));
                info.FileSize = reader.GetInt64(3);
                info.ItemId = reader.GetInt32(4);
                info.ListId = reader.IsDBNull(5) ? null : reader.GetString(5);
                info.WebId = reader.IsDBNull(6) ? null : reader.GetString(6);
                info.ItemUniqueId = reader.GetString(7);
                res.Add(info);
            }
            return res;
        }

        public void WaitInsertFinish()
        {
            _queue.CompleteAdding();
            while (_executeThread.IsAlive && _executeThreadEndFlag == default)
            {
                Thread.Sleep(100);
            }
            if(_executeThreadExcpetion != null)
            {
                throw new Exception(@$"Fail insert data into discovery item temp db:{_dbName}", _executeThreadExcpetion);
            }
        }

        public void Dispose()
        {
            File.Delete(CACHE_FODER_PATH  + System.IO.Path.DirectorySeparatorChar + _dbName);
        }



    }
}
