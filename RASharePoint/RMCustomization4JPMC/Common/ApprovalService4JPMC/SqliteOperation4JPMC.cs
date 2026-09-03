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
using AvePoint.RA.Contract;
using AvePoint.RA.DB.Core;
using AvePoint.RA.SharePoint.Archiver;
using AvePoint.RA.SharePoint.ArchiverCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace AvePoint.RA.SharePoint.RMCustomization4JPMC.Common.ApprovalService4JPMC
{
    internal class SqliteOperation4JPMC : SqliteDBBase, IApprovalReportOpers4JPMC
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(ArchiverSharePointScanner));
        private static readonly object _sqliteGlobalLock = new object();
        const string EMPTYRULEID = null;
        private ScheduleConfiguration mConfiguration;
        private string mCurrentRuleId = EMPTYRULEID;
        private string tableName = "SiteMetricsScanTable";
        private Queue mNodeCacheQueue = new Queue();
        private int readPageCount = 5000;//一次从SQLite中查出5000个记录。
        private int offset = 0;

        public SqliteOperation4JPMC(ScheduleConfiguration config) : base(config.ArchiveTemp, config.ScanDBName)
        {
            mConfiguration = config;
        }

        public override void CreateSchemaIfNotExists(IDbCommand command)
        {
            string query = string.Format($"CREATE TABLE IF NOT EXISTS {SecurityUtils.SanitizeSQLSchemaName(tableName)}(" +
                "[NodeID] [uniqueidentifier] not null," +
                "[ParentId] [uniqueidentifier] not null," +
                "[CacheNodeType] [int]," +
                "[RuleID] [uniqueidentifier]," +
                "[ScanJobID] [nvarchar](128)," +
                "[SortTicks] [nvarchar](128)," +
                "[ScanTime] [nvarchar](40)," +
                "[WebId] [uniqueidentifier] not null," +
                "[ListId] [uniqueidentifier] not null," +
                "[LibRowID] [int]," +
                "[SPNodeLevel] [int]," +
                "[LastModifiedTime] [bigint]," +
                "[Size] [bigint]," +
                "[ClassCode] [nvarchar](500)," +
                "[CountryCode] [nvarchar](500)," +
                "[RecordStatus] [nvarchar](500));" +
                $"CREATE INDEX IF NOT EXISTS SortTicksIndex ON {tableName}(SortTicks asc);" +
                $"CREATE INDEX IF NOT EXISTS NodeID ON {tableName}(NodeID asc)");

            command.CommandText = query;

            command.ExecuteNonQuery();
        }

        public void Reset(string ruleId)
        {
            if (string.IsNullOrEmpty(ruleId))
            {
                mCurrentRuleId = EMPTYRULEID;
            }
            else
            {
                mCurrentRuleId = ruleId;
            }
            offset = 0;
            mNodeCacheQueue.Clear();
        }

        public static bool IsEmptyRule(string RuleId)
        {
            return RuleId == EMPTYRULEID;
        }

        public void InsertValueToDB(List<ArchiveApproveReport4JPMC> archiverEntities)
        {
            using (PerformanceScope pc = new PerformanceScope("AveSqliteOperation.InsertValueToDB"))
            {
                lock(_sqliteGlobalLock)
                {
                    ExecuteWithConnection(connection =>
                    {
                        using (var command = connection.CreateCommand())
                        {
                            InternalInsertValueToDB(connection, command, archiverEntities);
                        }
                    });
                }
            }
        }

        private void InternalInsertValueToDB(SQLiteConnection conn, IDbCommand command, List<ArchiveApproveReport4JPMC> archiverEntities)
        {
            using (SQLiteTransaction tr = conn.BeginTransaction())
            {
                foreach (var archiverEn in archiverEntities)
                {
                    StringBuilder query = new StringBuilder();
                    query.Append($"INSERT INTO {SecurityUtils.SanitizeSQLSchemaName(tableName)}(NodeID,ParentId,CacheNodeType,RuleID,ScanJobID,SortTicks,ScanTime,WebId,ListId,LibRowID,SPNodeLevel,LastModifiedTime,Size,ClassCode,CountryCode,RecordStatus) ");
                    query.Append(@"VALUES (@NodeID,@ParentId,@CacheNodeType,@RuleID,@ScanJobID,@SortTicks,@ScanTime,@WebId,@ListId,@LibRowID,@SPNodeLevel,@LastModifiedTime,@Size,@ClassCode,@CountryCode,@RecordStatus)");
                    SQLiteParameter[] parameters = {
                        new SQLiteParameter("@NodeID"),
                        new SQLiteParameter("@ParentId"),
                        new SQLiteParameter("@CacheNodeType"),
                        new SQLiteParameter("@RuleID"),
                        new SQLiteParameter("@ScanJobID"),
                        new SQLiteParameter("@SortTicks"),
                        new SQLiteParameter("@ScanTime"),
                        new SQLiteParameter("@WebId"),
                        new SQLiteParameter("@ListId"),
                        new SQLiteParameter("@LibRowID"),
                        new SQLiteParameter("@SPNodeLevel"),
                        new SQLiteParameter("@LastModifiedTime"),
                        new SQLiteParameter("@Size"),
                        new SQLiteParameter("@ClassCode"),
                        new SQLiteParameter("@CountryCode"),
                        new SQLiteParameter("@RecordStatus")
                    };
                    parameters[0].Value = archiverEn.NodeId;
                    parameters[1].Value = archiverEn.ParentId;
                    parameters[2].Value = archiverEn.CacheNodeType;
                    parameters[3].Value = archiverEn.RuleId;
                    parameters[4].Value = archiverEn.ScanJobID;
                    parameters[5].Value = archiverEn.SortTicks;
                    parameters[6].Value = FormatScanTimeUtc(archiverEn.ScanTime);
                    parameters[7].Value = archiverEn.WebID;
                    parameters[8].Value = archiverEn.ListID;
                    parameters[9].Value = archiverEn.LibRowId;
                    parameters[10].Value = archiverEn.SPNodeLevel;
                    parameters[11].Value = archiverEn.LastModifiedTime;
                    parameters[12].Value = archiverEn.DocumentSize;
                    parameters[13].Value = archiverEn.ClassCode;
                    parameters[14].Value = archiverEn.CountryCode;
                    parameters[15].Value = archiverEn.RecordStatus;

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


        public List<ArchiveApproveReport4JPMC> SelectValuesFromDB(string ruleId, int offset, int pageSize)
        {
            using (PerformanceScope pc = new PerformanceScope("SqliteOperation4JPMC.SelectValuesFromDB"))
            {
                List<ArchiveApproveReport4JPMC> reports = new List<ArchiveApproveReport4JPMC>();
                lock(_sqliteGlobalLock)
                {
                    ExecuteWithConnection(connection =>
                    {
                        using (var command = connection.CreateCommand())
                        {
                            reports = InternalSelectValuesFromDB(command, ruleId, offset, pageSize);
                        }
                    });
                }
                return reports;
            }
        }

        public List<ArchiveApproveReport4JPMGroupBy> SelectValuesFromDBGroupByColumns(string ruleId, string listId)
        {
            List<ArchiveApproveReport4JPMGroupBy> archiverEntities = new List<ArchiveApproveReport4JPMGroupBy>();
            ExecuteWithConnection(connection =>
            {
                using (var command = connection.CreateCommand())
                {
                    var listIdQuery = "";
                    if (!string.IsNullOrEmpty(listId))
                    {
                        listIdQuery = $"AND [ListId] = @ListId ";
                    }
                    string query = $"SELECT RecordStatus,ClassCode,CountryCode,count(*) " +
                        $"FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} " +
                        $"WHERE [RuleID] = @RuleID " +
                        $"{listIdQuery}" +
                        $" GROUP BY RecordStatus,ClassCode,CountryCode " +
                        $"ORDER BY SortTicks";
                    SQLiteParameter[] parameters = [new("@RuleID", ruleId)];
                    foreach (var para in parameters)
                    {
                        command.Parameters.Add(para);
                    }
                    if (!string.IsNullOrEmpty(listId))
                    {
                        command.Parameters.Add(new SQLiteParameter("@ListId",listId));
                    }
                    command.CommandText = query;
                    using (var sr = command.ExecuteReader())
                    {
                        while (sr.Read())
                        {
                            ArchiveApproveReport4JPMGroupBy archiverEn = new ArchiveApproveReport4JPMGroupBy();
                            var idx = 0;
                            archiverEn.RecordStatus = sr[idx++].ToString();
                            archiverEn.ClassCode = sr[idx++].ToString();
                            archiverEn.CountryCode = sr[idx++].ToString();
                            archiverEn.TotalCount = sr.IsDBNull(idx) ? 0L : sr.GetInt64(idx++);
                            archiverEntities.Add(archiverEn);
                        }
                    }
                }
            });
            return archiverEntities;
        }
        
        public List<ArchiveApproveReport4JPMTotalSize> SelectTotalSizeFromDB(string ruleId, string listId)
        {
            List<ArchiveApproveReport4JPMTotalSize> archiverEntities = new List<ArchiveApproveReport4JPMTotalSize>();
            ExecuteWithConnection(connection =>
            {
                using (var command = connection.CreateCommand())
                {
                    var listIdQuery = "";
                    if (!string.IsNullOrEmpty(listId))
                    {
                        listIdQuery = $"AND [ListId] = @ListId ";
                    }
                    string query = $"SELECT SUM(Size),count(*) " +
                        $"FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} " +
                        $"WHERE [RuleID] = @RuleID AND ClassCode<>'' AND CountryCode<>'' AND RecordStatus<>'' " +
                        $"{listIdQuery}";
                    SQLiteParameter[] parameters = [new("@RuleID", ruleId)];
                    foreach (var para in parameters)
                    {
                        command.Parameters.Add(para);
                    }
                    if (!string.IsNullOrEmpty(listId))
                    {
                        command.Parameters.Add(new SQLiteParameter("@ListId",listId));
                    }
                    command.CommandText = query;
                    using (var sr = command.ExecuteReader())
                    {
                        while (sr.Read())
                        {
                            ArchiveApproveReport4JPMTotalSize archiverEn = new ArchiveApproveReport4JPMTotalSize();
                            var idx = 0;
                            archiverEn.TotalSize = string.IsNullOrEmpty(sr[idx].ToString()) ? 0 : sr.GetInt64(idx++);
                            archiverEn.TotalCount = string.IsNullOrEmpty(sr[idx].ToString()) ? 0 : sr.GetInt64(idx++);
                            archiverEntities.Add(archiverEn);
                        }
                    }
                }
            });
            return archiverEntities;
        }

        private List<ArchiveApproveReport4JPMC> InternalSelectValuesFromDB(IDbCommand command, string ruleId, int offset, int pageSize)
        {
            List<ArchiveApproveReport4JPMC> archiverEntities = new List<ArchiveApproveReport4JPMC>();
            string query = string.Format("SELECT NodeID,ParentId,CacheNodeType,RuleID,ScanJobID,SortTicks,ScanTime,WebId,ListId" +
                ",LibRowID,SPNodeLevel,LastModifiedTime,Size,ClassCode,CountryCode,RecordStatus" +
                $" FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} Where [RuleID] = @RuleID " +
                $"order by SortTicks limit {pageSize} offset {offset}");

            SQLiteParameter[] parameters = [new("@RuleID", ruleId)];
            foreach (var para in parameters)
            {
                command.Parameters.Add(para);
            }
            command.CommandText = query;
            using (var sr = command.ExecuteReader())
            {
                while (sr.Read())
                {
                    ArchiveApproveReport4JPMC archiverEn = new ArchiveApproveReport4JPMC();
                    var idx = 0;
                    archiverEn.NodeId = sr.GetGuid(idx++).ToString();
                    archiverEn.ParentId = sr.GetGuid(idx++).ToString();
                    archiverEn.CacheNodeType = sr.GetInt32(idx++);
                    archiverEn.RuleId = sr.GetString(idx++);
                    archiverEn.ScanJobID = sr.GetString(idx++);
                    archiverEn.SortTicks = sr.GetString(idx++);
                    archiverEn.ScanTime = ParseScanTimeTicks(sr.GetString(idx++));
                    archiverEn.WebID = sr.GetGuid(idx++).ToString();
                    archiverEn.ListID = sr.GetGuid(idx++).ToString();
                    archiverEn.LibRowId = sr.GetInt32(idx++);
                    archiverEn.SPNodeLevel = sr.GetInt32(idx++);
                    archiverEn.LastModifiedTime = sr.GetInt64(idx++);
                    archiverEn.DocumentSize = sr.GetInt64(idx++);
                    archiverEn.ClassCode = sr.GetString(idx++);
                    archiverEn.CountryCode = sr.GetString(idx++);
                    archiverEn.RecordStatus = sr.GetString(idx++);
                    archiverEntities.Add(archiverEn);
                }
            }
            return archiverEntities;
        }

        private static string FormatScanTimeUtc(long scanTimeTicks)
        {
            if (scanTimeTicks <= 0)
            {
                return string.Empty;
            }

            try
            {
                return new DateTime(scanTimeTicks, DateTimeKind.Utc).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
            }
            catch
            {
                return string.Empty;
            }
        }

        private static long ParseScanTimeTicks(string scanTimeText)
        {
            if (string.IsNullOrWhiteSpace(scanTimeText))
            {
                return 0;
            }

            if (DateTime.TryParse(scanTimeText, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
            {
                return parsed.ToUniversalTime().Ticks;
            }

            return 0;
        }


        public List<Guid> SelectExistingItemByNodeIds(List<Guid> nodeIds)
        {
            List<Guid> ids = new List<Guid>();
            using (PerformanceScope pc = new PerformanceScope("AveSqliteOperation.SelectExistingItemByNodeIds"))
            {

                for (int i = 0; i < nodeIds.Count; i += 50)
                {
                    var tempIds = nodeIds.Skip(i).Take(50);
                    ExecuteWithConnection(connection =>
                    {
                        using (var command = connection.CreateCommand())
                        {
                            string query = @$"SELECT NodeId FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} WHERE NodeId in {DatabaseUtility.BuildInClause(tempIds)}";
                            command.CommandText = query;
                            using (var sr = command.ExecuteReader())
                            {
                                while (sr.Read())
                                {
                                    var id = sr.GetGuid(0);
                                    if (!ids.Contains(id))
                                    {
                                        ids.Add(id);
                                    }
                                }
                            }
                        }
                    });
                }
                return ids;
            }
        }


        public void Close(SQLiteConnection sqliteConnection, SQLiteCommand sqliteCommand)
        {
            if (null != sqliteConnection)
            {
                sqliteConnection.Close();
                sqliteConnection.Dispose();
                sqliteConnection = null;
            }
            if (null != sqliteCommand)
            {
                sqliteCommand.Dispose();
                sqliteCommand = null;
            }
        }

        public void AddToDB(ArchiveApproveReport4JPMC nodeEntity, bool hasReported)
        {
            var ticks = Snowflake.Instance().GetTicks();
            nodeEntity.SortTicks = ticks.ToString();
            if (string.IsNullOrEmpty(nodeEntity.RuleId))
            {
                nodeEntity.RuleId = Guid.Empty.ToString();
            }
            lock (_sqliteGlobalLock)
            {
                ExecuteWithConnection(connection =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"SELECT COUNT(1) FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} WHERE NodeID = @NodeID";
                        command.Parameters.Add(new SQLiteParameter("@NodeID", nodeEntity.NodeId));
                        var exists = Convert.ToInt32(command.ExecuteScalar()) > 0;

                        command.Parameters.Clear();

                        if (exists)
                        {
                            command.CommandText = $"UPDATE {SecurityUtils.SanitizeSQLSchemaName(tableName)} SET ParentId=@ParentId,CacheNodeType=@CacheNodeType,RuleID=@RuleID,ScanJobID=@ScanJobID,SortTicks=@SortTicks,ScanTime=@ScanTime,WebId=@WebId,ListId=@ListId,LibRowID=@LibRowID,SPNodeLevel=@SPNodeLevel,LastModifiedTime=@LastModifiedTime,Size=@Size,ClassCode=@ClassCode,CountryCode=@CountryCode,RecordStatus=@RecordStatus WHERE NodeID=@NodeID";
                        }
                        else
                        {
                            command.CommandText = $"INSERT INTO {SecurityUtils.SanitizeSQLSchemaName(tableName)}(NodeID,ParentId,CacheNodeType,RuleID,ScanJobID,SortTicks,ScanTime,WebId,ListId,LibRowID,SPNodeLevel,LastModifiedTime,Size,ClassCode,CountryCode,RecordStatus) VALUES (@NodeID,@ParentId,@CacheNodeType,@RuleID,@ScanJobID,@SortTicks,@ScanTime,@WebId,@ListId,@LibRowID,@SPNodeLevel,@LastModifiedTime,@Size,@ClassCode,@CountryCode,@RecordStatus)";
                        }

                        command.Parameters.Add(new SQLiteParameter("@NodeID", nodeEntity.NodeId));
                        command.Parameters.Add(new SQLiteParameter("@ParentId", nodeEntity.ParentId));
                        command.Parameters.Add(new SQLiteParameter("@CacheNodeType", nodeEntity.CacheNodeType));
                        command.Parameters.Add(new SQLiteParameter("@RuleID", nodeEntity.RuleId));
                        command.Parameters.Add(new SQLiteParameter("@ScanJobID", nodeEntity.ScanJobID));
                        command.Parameters.Add(new SQLiteParameter("@SortTicks", nodeEntity.SortTicks));
                        command.Parameters.Add(new SQLiteParameter("@ScanTime", FormatScanTimeUtc(nodeEntity.ScanTime)));
                        command.Parameters.Add(new SQLiteParameter("@WebId", nodeEntity.WebID));
                        command.Parameters.Add(new SQLiteParameter("@ListId", nodeEntity.ListID));
                        command.Parameters.Add(new SQLiteParameter("@LibRowID", nodeEntity.LibRowId));
                        command.Parameters.Add(new SQLiteParameter("@SPNodeLevel", nodeEntity.SPNodeLevel));
                        command.Parameters.Add(new SQLiteParameter("@LastModifiedTime", nodeEntity.LastModifiedTime));
                        command.Parameters.Add(new SQLiteParameter("@Size", nodeEntity.DocumentSize));
                        command.Parameters.Add(new SQLiteParameter("@ClassCode", nodeEntity.ClassCode));
                        command.Parameters.Add(new SQLiteParameter("@CountryCode", nodeEntity.CountryCode));
                        command.Parameters.Add(new SQLiteParameter("@RecordStatus", nodeEntity.RecordStatus));

                        command.ExecuteNonQuery();
                    }
                });
            }
        }

        public void AddScanReport(ArchiveApproveReport4JPMC nodeEntity)
        {
        }

        public ArchiveApproveReport4JPMC ReadFromDB()
        {
            lock (mNodeCacheQueue.SyncRoot)
            {
                if (mNodeCacheQueue.Count == 0)
                {
                    ReadFromApproveDBByPage(readPageCount);
                    if (mNodeCacheQueue.Count == 0)
                    {
                        return null;
                    }
                    else
                    {
                        return mNodeCacheQueue.Dequeue() as ArchiveApproveReport4JPMC;
                    }
                }
                else
                {
                    return mNodeCacheQueue.Dequeue() as ArchiveApproveReport4JPMC;
                }
            }
        }

        public void Dispose()
        {
            try
            {
                mLog.Info($"Scan db file size is:{new FileInfo(this._dbFilePath).Length}");
            }
            catch (Exception e)
            {
                mLog.Warn($"Get file size error: {e}");
            }
            mLog.Info($"Delete file:{this._dbFilePath}");
            System.IO.File.Delete(this._dbFilePath);
        }

        public void ReadFromApproveDBByPage(int pageSize)
        {
            using (PerformanceScope pc = new PerformanceScope("SOArchiverAzureDBWorker.ReadFromApproveDBByPage"))
            {
                mLog.Info("Begin ReadFromApproveDBByPage for ScopePath:{0}.", _dbFilePath);
                var archiverEntities = SelectValuesFromDB(mCurrentRuleId, offset, pageSize);
                offset += archiverEntities.Count;
                foreach (var entity in archiverEntities)
                {
                    mNodeCacheQueue.Enqueue(entity);
                }
                mLog.Info("End ReadFromApproveDBByPage offset:{0} Count:{1}.", offset, archiverEntities.Count);
            }
        }

        public List<Guid> ExistInScanJob(List<Guid> nodeIds)
        {
            List<Guid> ids;
            using (PerformanceScope pc = new PerformanceScope("SOArchiverAzureDBWorker.ExistInScanJob"))
            {
                mLog.Info("Begin ExistInScanJob for ScopePath:{0}.", _dbFilePath);
                ids = SelectExistingItemByNodeIds(nodeIds);
                mLog.Info("End ExistInScanJob Count:{0}.", ids.Count);
                return ids;
            }
        }

        public List<ArchiveApproveReport4JPMGroupBy> ReadFromApproveDBGroupByColumns(string ruleId, string listId)
        {
            using (PerformanceScope pc = new PerformanceScope("SOArchiverAzureDBWorker.ReadFromApproveDBGroupByColumns"))
            {
                mLog.Info("Begin ReadFromApproveDBGroupByColumns for ScopePath:{0}.", _dbFilePath);
                return SelectValuesFromDBGroupByColumns(ruleId, listId);
            }
        }

        public List<ArchiveApproveReport4JPMTotalSize> ReadFromApproveDBTotalSize(string ruleId, string listId)
        {
            using (PerformanceScope pc = new PerformanceScope("SOArchiverAzureDBWorker.ReadFromApproveDBTotalSize"))
            {
                mLog.Info("Begin SelectValuesFromDBGroupBy for ScopePath:{0}.", _dbFilePath);
                return SelectTotalSizeFromDB(ruleId, listId);
            }
        }

        public void DeleteByNodeIds(List<Guid> nodeIds)
        {
            if (nodeIds == null || nodeIds.Count == 0)
            {
                return;
            }

            var ids = nodeIds.Where(id => id != Guid.Empty).Distinct().ToList();
            if (ids.Count == 0)
            {
                return;
            }

            using (PerformanceScope pc = new PerformanceScope("SqliteOperation4JPMC.DeleteByNodeIds"))
            {
                lock (_sqliteGlobalLock)
                {
                    ExecuteWithConnection(connection =>
                    {
                        using (var command = connection.CreateCommand())
                        using (var transaction = connection.BeginTransaction())
                        {
                            command.Transaction = transaction;
                            for (int i = 0; i < ids.Count; i += 50)
                            {
                                var batch = ids.Skip(i).Take(50);
                                command.CommandText = $"DELETE FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} WHERE NodeID in {DatabaseUtility.BuildInClause(batch)}";
                                command.Parameters.Clear();
                                command.ExecuteNonQuery();
                            }

                            transaction.Commit();
                        }
                    });
                }
            }
        }

        public void DeleteByParentIds(List<Guid> parentIds)
        {
            if (parentIds == null || parentIds.Count == 0)
            {
                mLog.Info("DeleteByParentIds skipped because input parentIds is empty.");
                return;
            }

            var ids = parentIds.Where(id => id != Guid.Empty).Distinct().ToList();
            if (ids.Count == 0)
            {
                mLog.Info("DeleteByParentIds skipped because all input parentIds are empty GUIDs.");
                return;
            }

            using (PerformanceScope pc = new PerformanceScope("SqliteOperation4JPMC.DeleteByParentIds"))
            {
                mLog.Info("DeleteByParentIds started. Initial parent count:{0}.", ids.Count);
                lock (_sqliteGlobalLock)
                {
                    ExecuteWithConnection(connection =>
                    {
                        var processedIds = new HashSet<Guid>();
                        var currentParentIds = ids;
                        var level = 0;

                        while (currentParentIds.Count > 0)
                        {
                            level++;
                            mLog.Info("DeleteByParentIds level:{0}, current node count:{1}.", level, currentParentIds.Count);
                            var childIds = SelectNodeIdsByParentIds(connection, currentParentIds);
                            mLog.Info("DeleteByParentIds level:{0}, discovered child node count:{1}.", level, childIds.Count);
                            DeleteByNodeIdsInternal(connection, currentParentIds);

                            foreach (var id in currentParentIds)
                            {
                                processedIds.Add(id);
                            }

                            currentParentIds = childIds.Where(id => !processedIds.Contains(id)).ToList();
                        }

                        mLog.Info("DeleteByParentIds finished. Processed node count:{0}, levels:{1}.", processedIds.Count, level);
                    });
                }
            }
        }

        private List<Guid> SelectNodeIdsByParentIds(SQLiteConnection connection, List<Guid> parentIds)
        {
            var results = new List<Guid>();
            if (parentIds == null || parentIds.Count == 0)
            {
                return results;
            }

            using (var command = connection.CreateCommand())
            {
                for (int i = 0; i < parentIds.Count; i += 50)
                {
                    var batch = parentIds.Skip(i).Take(50);
                    command.CommandText = $"SELECT NodeID FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} WHERE ParentId in {DatabaseUtility.BuildInClause(batch)}";
                    command.Parameters.Clear();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                results.Add(reader.GetGuid(0));
                            }
                        }
                    }
                }
            }

            return results.Distinct().ToList();
        }

        private void DeleteByNodeIdsInternal(SQLiteConnection connection, List<Guid> nodeIds)
        {
            var ids = nodeIds?.Where(id => id != Guid.Empty).Distinct().ToList();
            if (ids == null || ids.Count == 0)
            {
                return;
            }

            using (var command = connection.CreateCommand())
            using (var transaction = connection.BeginTransaction())
            {
                command.Transaction = transaction;
                for (int i = 0; i < ids.Count; i += 50)
                {
                    var batch = ids.Skip(i).Take(50);
                    command.CommandText = $"DELETE FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} WHERE NodeID in {DatabaseUtility.BuildInClause(batch)}";
                    command.Parameters.Clear();
                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }

        public void DeleteByWebId(Guid webId)
        {
            using (PerformanceScope pc = new PerformanceScope("SqliteOperation4JPMC.DeleteByWebId"))
            {
                lock (_sqliteGlobalLock)
                {
                    ExecuteWithConnection(connection =>
                    {
                        using (var command = connection.CreateCommand())
                        {
                            var normalizedWebId = webId.ToString("D");
                            command.CommandText = $"DELETE FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} WHERE lower(WebId) = lower(@WebId)";
                            command.Parameters.Add(new SQLiteParameter("@WebId", normalizedWebId));
                            var deletedRows = command.ExecuteNonQuery();
                            mLog.Info("DeleteByWebId deleted rows:{0}, webId:{1}.", deletedRows, normalizedWebId);
                        }
                    });
                }
            }
        }

        public void DeleteByList(Guid webId, Guid listId)
        {
            using (PerformanceScope pc = new PerformanceScope("SqliteOperation4JPMC.DeleteByList"))
            {
                lock (_sqliteGlobalLock)
                {
                    ExecuteWithConnection(connection =>
                    {
                        using (var command = connection.CreateCommand())
                        {
                            var normalizedListId = listId.ToString("D");
                            command.CommandText = $"DELETE FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} WHERE lower(ListId) = lower(@ListId)";
                            command.Parameters.Add(new SQLiteParameter("@ListId", normalizedListId));
                            var normalizedWebId = webId.ToString("D");
                            if (webId != Guid.Empty)
                            {
                                command.CommandText += " AND lower(WebId) = lower(@WebId)";
                                command.Parameters.Add(new SQLiteParameter("@WebId", normalizedWebId));
                            }
                            var deletedRows = command.ExecuteNonQuery();
                            mLog.Info("DeleteByList deleted rows:{0}, listId:{1}, webId:{2}.", deletedRows, normalizedListId, normalizedWebId);
                        }
                    });
                }
            }
        }
    }
}