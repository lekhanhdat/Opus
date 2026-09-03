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




using Amazon.Util.Internal;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Common;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.DB.Core;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.StorageOptimization.Schedule.Common;
using AvePoint.Wrapper.Common;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;
using RAExportCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using static Microsoft.Office.Project.Server.Library.QueueConstants;

namespace AvePoint.RA.SharePoint.Archiver
{
    
    internal class ScanDBOperation : SqliteDBBase, IApprovalReportOpers
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(ArchiverSharePointScanner));
        private static readonly object _sqliteGlobalLock = new object();
        const string EMPTYRULEID = null;
        private string mJobId;
        private string mCurrentRuleId = EMPTYRULEID;
        private string tableName = "ArchiverScanTable";
        private Queue mNodeCacheQueue = new Queue();
        private int readPageCount = 5000;//一次从SQLite中查出5000个记录。

        private int offset = 0;

        public ScanDBOperation(ScheduleConfiguration config) : base(config.ArchiveTemp, config.ScanDBName)
        {
            mJobId = config.JobId;
        }

        public ScanDBOperation(string dirPath, string name, string jobId) : base(dirPath, name)
        {
            mJobId = jobId;
        }


        public override void CreateSchemaIfNotExists(IDbCommand command)
        {
            string query = string.Format($"CREATE TABLE IF NOT EXISTS {tableName}(" +
                "[RowKey] [nvarchar](500) NOT NULL," +
                "[ArchiveLevel] [int]," +
                "[NodeID] [uniqueidentifier] not null," +
                "[ParentId] [uniqueidentifier] not null," +
                "[UIVersion] [int] not null," +
                "[CacheNodeType] [int]," +
                "[Status] [int]," +
                "[RuleID] [uniqueidentifier]," +
                "[DeleteRelatedRecords] [int]," +
                "[ScanJobID] [nvarchar](128)," +
                "[SortTicks] [nvarchar](128)," +
                "[SiteUrl] [nvarchar](2000)," +
                "[WebId] [uniqueidentifier] not null," +
                "[ListId] [uniqueidentifier] not null," +
                "[LeafName] [nvarchar](255)," +
                "[Path] [nvarchar](512)," +
                "[ScanTime] [bigint]," +
                "[LibRowID] [int]," +
                "[NodeType] [int]," +
                "[SPNodeLevel] [int]," +
                "[Level] [tinyint]," +
                "[LastModifiedTime] [bigint]," +
                "[DoDelete] [Boolean]," +
                "[Size] [bigint]," +
                "[JsonMeta] [nvarchar](4000)," +
                "[ManifestDocumentSnapshot] [nvarchar]," +
                "[IsRepeatProcess] [Boolean]);" +
                $"CREATE INDEX IF NOT EXISTS SortTicksIndex ON {SecurityUtils.SanitizeSQLSchemaName(tableName)}(SortTicks asc);" +
                $"CREATE INDEX IF NOT EXISTS NodeID ON {SecurityUtils.SanitizeSQLSchemaName(tableName)}(NodeID asc)");

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

        public void InsertValueToDB(List<ArchiveApproveReport> archiverEntities)
        {
            using (PerformanceScope pc = new PerformanceScope("AveSqliteOperation.InsertValueToDB"))
            {
                lock (_sqliteGlobalLock)
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

        private void InternalInsertValueToDB(SQLiteConnection conn, IDbCommand command, List<ArchiveApproveReport> archiverEntities)
        {
            using (SQLiteTransaction tr = conn.BeginTransaction())
            {
                foreach (var archiverEn in archiverEntities)
                {
                    StringBuilder query = new StringBuilder();
                    query.Append($"INSERT INTO {SecurityUtils.SanitizeSQLSchemaName(tableName)}(RowKey,ArchiveLevel,NodeID,ParentId,UIVersion,CacheNodeType,Status,RuleID,DeleteRelatedRecords,ScanJobID,SortTicks,SiteUrl,WebId,ListId,LeafName,Path,ScanTime,LibRowID,NodeType,SPNodeLevel,Level,LastModifiedTime,DoDelete,Size,JsonMeta,IsRepeatProcess,ManifestDocumentSnapshot) ");
                    //string query = string.Format("INSERT INTO {0} VALUES (@PartitionKey)", tableName, SplicingInsertValueString(archiverEntities));
                    query.Append(@"VALUES (@RowKey,@ArchiveLevel,@NodeID,@ParentId,@UIVersion,@CacheNodeType,@Status,@RuleID,
@DeleteRelatedRecords,@ScanJobID,@SortTicks,@SiteUrl,@WebId,@ListId,@LeafName,@Path,@ScanTime,@LibRowID,@NodeType,@SPNodeLevel,@Level,@LastModifiedTime,@DoDelete,@Size,@JsonMeta,@IsRepeatProcess,@ManifestDocumentSnapshot)");
                    SQLiteParameter[] parameters = {
                    new SQLiteParameter("@PartitionKey"),
                    new SQLiteParameter("@RowKey"),
                    new SQLiteParameter("@ArchiveLevel"),
                    new SQLiteParameter("@NodeID"),
                    new SQLiteParameter("@ParentId"),
                    new SQLiteParameter("@UIVersion"),
                    new SQLiteParameter("@CacheNodeType"),
                    new SQLiteParameter("@Status"),
                    new SQLiteParameter("@RuleID"),
                    new SQLiteParameter("@DeleteRelatedRecords"),
                    new SQLiteParameter("@ScanJobID"),
                    new SQLiteParameter("@SortTicks"),
                    new SQLiteParameter("@SiteUrl"),
                    new SQLiteParameter("@WebId"),
                    new SQLiteParameter("@ListId"),
                    new SQLiteParameter("@LeafName"),
                    new SQLiteParameter("@Path"),
                    new SQLiteParameter("@ScanTime"),
                    new SQLiteParameter("@LibRowID"),
                    new SQLiteParameter("@NodeType"),
                    new SQLiteParameter("@SPNodeLevel"),
                    new SQLiteParameter("@Level"),
                    new SQLiteParameter("@LastModifiedTime"),
                    new SQLiteParameter("@DoDelete"),
                    new SQLiteParameter("@Size"),
                    new SQLiteParameter("@JsonMeta"),
                    new SQLiteParameter("@IsRepeatProcess"),
                    new SQLiteParameter("@ManifestDocumentSnapshot")
                };
                    parameters[0].Value = archiverEn.PartitionKey;
                    parameters[1].Value = archiverEn.EntityRowKey;
                    parameters[2].Value = archiverEn.ArchiveLevel;
                    parameters[3].Value = archiverEn.NodeId;
                    parameters[4].Value = archiverEn.ParentId;
                    parameters[5].Value = archiverEn.UIVersion;
                    parameters[6].Value = archiverEn.CacheNodeType;
                    parameters[7].Value = archiverEn.Status;
                    parameters[8].Value = archiverEn.RuleId;
                    parameters[9].Value = archiverEn.DeleteRelatedRecords;
                    parameters[10].Value = archiverEn.ScanJobID;
                    parameters[11].Value = archiverEn.SortTicks;
                    parameters[12].Value = archiverEn.SiteUrl;
                    parameters[13].Value = archiverEn.WebID;
                    parameters[14].Value = archiverEn.ListID;
                    parameters[15].Value = archiverEn.LeafName;
                    parameters[16].Value = archiverEn.FullPath;
                    parameters[17].Value = archiverEn.ScanTime;
                    parameters[18].Value = archiverEn.LibRowId;
                    parameters[19].Value = archiverEn.NodeType;
                    parameters[20].Value = archiverEn.SPNodeLevel;
                    parameters[21].Value = archiverEn.Level;
                    parameters[22].Value = archiverEn.LastModifiedTime>0? archiverEn.LastModifiedTime: archiverEn.Modified;
                    parameters[23].Value = archiverEn.DoDelete;
                    parameters[24].Value = archiverEn.DocumentSize;
                    parameters[25].Value = archiverEn.JsonMeta;
                    parameters[26].Value = archiverEn.IsRepeatProcess;
                    parameters[27].Value = SerializerHelper.SerializeByJsonSerializer(archiverEn.ManifestDocumentSnapshot);

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


        public List<ArchiveApproveReport> SelectUnRepeatProcessedValuesFromDB(int offset, int pageSize)
        {
            using (PerformanceScope pc = new PerformanceScope("AveSqliteOperation.SelectUnRepeatProcessedValuesFromDB"))
            {
                List<ArchiveApproveReport> reports = new List<ArchiveApproveReport>();
                lock (_sqliteGlobalLock)
                {
                    ExecuteWithConnection(connection =>
                    {
                        using (var command = connection.CreateCommand())
                        {
                            reports = InternalSelectValuesFromDB(command, " where IsRepeatProcess = 0 ", null, offset, pageSize);
                        }
                    });
                }
                return reports;
            }
        }

        public List<ArchiveApproveReport> SelectValuesFromDB(string ruleId, int offset, int pageSize)
        {
            using (PerformanceScope pc = new PerformanceScope("AveSqliteOperation.SelectValuesFromDB"))
            {
                List<ArchiveApproveReport> reports = new List<ArchiveApproveReport>();
                lock (_sqliteGlobalLock)
                {
                    ExecuteWithConnection(connection =>
                    {
                        using (var command = connection.CreateCommand())
                        {
                            string whereSql = "Where[RuleID] in (@RuleID, @EmptyRuleID) And[Status] in (@ApprovedStatus, @CheckOptionStatus)";
                            SQLiteParameter[] parameters = new SQLiteParameter[]
                            {
                                new SQLiteParameter("@RuleID",ruleId),
                                new SQLiteParameter("@EmptyRuleID",Guid.Empty.ToString()),
                                new SQLiteParameter("@ApprovedStatus",(int)SOApproveDBStatus.Approved),
                                new SQLiteParameter("@CheckOptionStatus",(int)SOApproveDBStatus.CheckOption),
                            };
                            reports = InternalSelectValuesFromDB(command, whereSql, parameters, offset, pageSize);
                        }
                    });
                }
                return reports;
            }
        }

        public List<ArchiveApproveReport> SelectValuesFromDB(int offset, int pageSize)
        {
            using (PerformanceScope pc = new PerformanceScope("AveSqliteOperation.SelectValuesFromDB"))
            {
                List<ArchiveApproveReport> reports = new List<ArchiveApproveReport>();
                lock (_sqliteGlobalLock)
                {
                    ExecuteWithConnection(connection =>
                    {
                        using (var command = connection.CreateCommand())
                        {
                            reports = InternalSelectValuesFromDB(command, string.Empty, null, offset, pageSize);
                        }
                    });
                }
                return reports;
            }
        }

        private List<ArchiveApproveReport> InternalSelectValuesFromDB(IDbCommand command, string whereSql, SQLiteParameter[] parameters, int offset, int pageSize)
        {
            List<ArchiveApproveReport> archiverEntities = new List<ArchiveApproveReport>();
            string query = string.Format("SELECT " +
                "[RowKey]," +
                "[ArchiveLevel]," +
                "[NodeID]," +
                "[ParentId]," +
                "[UIVersion]," +
                "[CacheNodeType]," +
                "[Status]," +
                "[RuleID]," +
                "[DeleteRelatedRecords]," +
                "[ScanJobID]," +
                "[SortTicks]," +
                "[SiteUrl]," +
                "[WebId]," +
                "[ListId]," +
                "[LeafName]," +
                "[Path]," +
                "[ScanTime]," +
                "[LibRowID]," +
                "[NodeType]," +
                "[SPNodeLevel]," +
                "[Level]," +
                "[LastModifiedTime]," +
                "[DoDelete]," +
                "[Size]," +
                "[IsRepeatProcess]," +
                "[ManifestDocumentSnapshot]" +
                $" FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} {whereSql} order by SortTicks limit {pageSize} offset {offset}");

            if(parameters != null)
            {
                foreach (var para in parameters)
                {
                    command.Parameters.Add(para);
                }
            }
            command.CommandText = query;
            using (var sr = command.ExecuteReader())
            {
                while (sr.Read())
                {
                    ArchiveApproveReport archiverEn = new ArchiveApproveReport();
                    archiverEn.EntityRowKey = sr.GetString(0);
                    archiverEn.ArchiveLevel = sr.GetInt32(1);
                    archiverEn.NodeId = sr.GetGuid(2).ToString();
                    archiverEn.ParentId = sr.GetGuid(3).ToString();
                    archiverEn.UIVersion = sr.GetInt32(4);
                    archiverEn.CacheNodeType = sr.GetInt32(5);
                    archiverEn.Status = (SOApproveDBStatus)sr.GetInt32(6);
                    archiverEn.RuleId = sr.GetString(7);
                    archiverEn.DeleteRelatedRecords = sr.GetInt32(8);
                    //archiverEn.ScanJobID = sr.GetString(9);
                    archiverEn.SortTicks = sr.GetString(10);
                    archiverEn.SiteUrl = sr.GetString(11);
                    archiverEn.WebID = sr.GetGuid(12);
                    archiverEn.ListID = sr.GetGuid(13);
                    archiverEn.LeafName = sr.GetString(14);
                    archiverEn.FullPath = sr.GetString(15);
                    archiverEn.ScanTime = sr.GetInt64(16);
                    archiverEn.LibRowId = sr.GetInt32(17);
                    archiverEn.NodeType = sr.GetInt32(18);
                    archiverEn.SPNodeLevel = sr.GetInt32(19);
                    archiverEn.Level = Convert.ToByte(sr.GetInt32(20));
                    archiverEn.LastModifiedTime = sr.GetInt64(21);
                    archiverEn.DoDelete = sr.GetBoolean(22);
                    archiverEn.DocumentSize = sr.GetInt64(23);
                    archiverEn.IsRepeatProcess = sr.GetBoolean(24);
                    string manifestDocumentSnapshot = sr.GetString(25);
                    archiverEn.ManifestDocumentSnapshot = string.IsNullOrEmpty(manifestDocumentSnapshot) || manifestDocumentSnapshot.EqualsIgnoreCase("null") ? null : SerializerHelper.DeserializeByJsonSerializer<ManifestDocumentSnapshot>(manifestDocumentSnapshot);
                    archiverEntities.Add(archiverEn);
                }
            }
            return archiverEntities;
        }

        public void UpdateNodesToNotNeedProcess()
        {
            lock (_sqliteGlobalLock)
            {
                ExecuteWithConnection(connection =>
                {
                    InternalUpdateNodesToNotNeedProcess(connection);
                });
            }
        }

        public void InternalUpdateNodesToNotNeedProcess(SQLiteConnection conn)
        {
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    using (var command = conn.CreateCommand())
                    {
                        string updateQuery = $@"
                    UPDATE {SecurityUtils.SanitizeSQLSchemaName(tableName)}
                    SET IsRepeatProcess = 1 where IsRepeatProcess = 0";
                        command.CommandText = updateQuery;
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void UpdateNodesToNeedProcess(IEnumerable<String> parentNodeRowKeys)
        {
            lock (_sqliteGlobalLock)
            {
                ExecuteWithConnection(connection =>
                {
                    InternalUpdateNodesToNeedProcess(connection, parentNodeRowKeys);
                });
            }
        }

        public void InternalUpdateNodesToNeedProcess(SQLiteConnection conn, IEnumerable<String> parentNodeRowKeys)
        {
            if (!parentNodeRowKeys.Any())
            {
                mLog.Warn("not any parent node was get");
                return;
            }

            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    string updateQuery = $@"
                    UPDATE {SecurityUtils.SanitizeSQLSchemaName(tableName)}
                    SET IsRepeatProcess = 0
                    WHERE RowKey IN {DatabaseUtility.BuildInClause(parentNodeRowKeys, out List<SQLiteParameter> parameters)}";

                    using (var command = new SQLiteCommand(updateQuery, conn, transaction))
                    {
                        command.Parameters.AddRange(parameters.ToArray());
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }


        public List<string> SelectRuleIdsFromDB()
        {
            using (PerformanceScope pc = new PerformanceScope("AveSqliteOperation.SelectRuleIdsFromDB"))
            {
                List<string> reports = new List<string>();
                lock (_sqliteGlobalLock)
                {
                    ExecuteWithConnection(connection =>
                    {
                        using (var command = connection.CreateCommand())
                        {
                            reports = InternalSelectRuleIdsFromDB(command);
                        }
                    });
                }
                return reports;
            }
        }

        private List<string> InternalSelectRuleIdsFromDB(IDbCommand command)
        {
            List<string> ruleids = new List<string>();
            string query = string.Format("SELECT DISTINCT RuleID"+
                $" FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} Where [RuleID] <> @EmptyRuleID And [Status] in (@ApprovedStatus,@CheckOptionStatus)");

            SQLiteParameter[] parameters = new SQLiteParameter[]
            {
                new SQLiteParameter("@EmptyRuleID",Guid.Empty.ToString()),
                new SQLiteParameter("@ApprovedStatus",(int)SOApproveDBStatus.Approved),
                new SQLiteParameter("@CheckOptionStatus",(int)SOApproveDBStatus.CheckOption),
            };
            foreach (var para in parameters)
            {
                command.Parameters.Add(para);
            }
            command.CommandText = query;
            using (var sr = command.ExecuteReader())
            {
                while (sr.Read())
                {
                    ruleids.Add(sr.GetString(0));
                }
            }
            return ruleids;
        }

        public long SelectDataCountFromDB(int minCacheNodeType = 0)
        {
            using (PerformanceScope pc = new PerformanceScope("AveSqliteOperation.SelectDataCountFromDB"))
            {
                long count = 0;
                lock (_sqliteGlobalLock)
                {
                    ExecuteWithConnection(connection =>
                    {
                        using (var command = connection.CreateCommand())
                        {
                            count = InternalSelectDataCountFromDB(command, minCacheNodeType);
                        }
                    });
                }
                return count;
            }
        }

        public Dictionary<int, long> SelectDataCountsFromDB(int minCacheNodeType = 0, string ruleId = "")
        {
            using (PerformanceScope pc = new PerformanceScope("AveSqliteOperation.SelectDataCountsFromDB"))
            {
                var counts = new Dictionary<int, long>();
                lock (_sqliteGlobalLock)
                {
                    ExecuteWithConnection(connection =>
                    {
                        using (var command = connection.CreateCommand())
                        {
                            counts = InternalSelectDataCountsFromDB(command, minCacheNodeType, ruleId);
                        }
                    });
                }
                return counts;
            }
        }

        public List<Guid> SelectExistingItemByNodeIds(List<Guid> nodeIds)
        {
            List<Guid> ids = new List<Guid>();
            using (PerformanceScope pc = new PerformanceScope("AveSqliteOperation.SelectExistingItemByNodeIds"))
            {

                for (int i = 0; i < nodeIds.Count; i += 50)
                {
                    var tempIds = nodeIds.Skip(i).Take(50);
                    lock (_sqliteGlobalLock)
                    {
                        ExecuteWithConnection(connection =>
                        {
                            using (var command = connection.CreateCommand())
                            {
                                string query = @$"SELECT NodeId FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} WHERE NodeId in {DatabaseUtility.BuildInClause<Guid>(tempIds)}";
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
                }
                return ids;
            }
        }

        private long InternalSelectDataCountFromDB(IDbCommand command, int minCacheNodeType = 0)
        {
            long count = 0;
            string query = string.Format("SELECT COUNT(*) " +
                $" FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} Where [Status] in (@ApprovedStatus,@CheckOptionStatus)");

            List<SQLiteParameter> parameters = new List<SQLiteParameter>
            {
                new SQLiteParameter("@ApprovedStatus",(int)SOApproveDBStatus.Approved),
                new SQLiteParameter("@CheckOptionStatus",(int)SOApproveDBStatus.CheckOption),
            };
            if (minCacheNodeType > 0)
            {
                query += " AND [CacheNodeType] >= @MinCacheNodeType";
                parameters.Add(new SQLiteParameter("@MinCacheNodeType", minCacheNodeType));
            }
            foreach (var para in parameters)
            {
                command.Parameters.Add(para);
            }
            command.CommandText = query;
            using (var sr = command.ExecuteReader())
            {
                while (sr.Read())
                {
                    count=sr.GetInt64(0);
                }
            }
            return count;
        }

        private Dictionary<int, long> InternalSelectDataCountsFromDB(IDbCommand command, int minCacheNodeType = 0, string ruleId = "")
        {
            var counts = new Dictionary<int, long>();
            string query = string.Format("SELECT CacheNodeType, COUNT(*) " +
                $" FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} Where [Status] in (@ApprovedStatus,@CheckOptionStatus)");

            List<SQLiteParameter> parameters = new List<SQLiteParameter>
            {
                new("@ApprovedStatus", (int)SOApproveDBStatus.Approved),
                new("@CheckOptionStatus", (int)SOApproveDBStatus.CheckOption),
            };
            if (minCacheNodeType > 0)
            {
                query += " AND [CacheNodeType] >= @MinCacheNodeType";
                parameters.Add(new SQLiteParameter("@MinCacheNodeType", minCacheNodeType));
            }
            if (!string.IsNullOrEmpty(ruleId))
            {
                query += " AND [RuleID] = @RuleId";
                parameters.Add(new SQLiteParameter("@RuleId", ruleId));
            }
            query += " GROUP BY [CacheNodeType]";
            foreach (var para in parameters)
            {
                command.Parameters.Add(para);
            }
            command.CommandText = query;
            using (var sr = command.ExecuteReader())
            {
                while (sr.Read())
                {
                    int cacheNodeType = sr.GetInt32(0);
                    long count = sr.GetInt64(1);
                    counts[cacheNodeType] = count;
                }
            }
            return counts;
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

        public void AddToDB(ArchiveApproveReport nodeEntity, bool hasReported)
        {
            if (string.IsNullOrWhiteSpace(nodeEntity.EntityRowKey))
            {
                var ticks = Snowflake.Instance().GetTicks();
                nodeEntity.EntityRowKey = mJobId + "_" + ticks;
                nodeEntity.SortTicks = ticks.ToString();
            }
            
            if (string.IsNullOrEmpty(nodeEntity.RuleId))
            {
                nodeEntity.RuleId = Guid.Empty.ToString();
            }
            InsertValueToDB(new List<ArchiveApproveReport>() { nodeEntity });
        }

        public void AddScanReport(ArchiveApproveReport nodeEntity)
        {
        }

        public ArchiveApproveReport ReadFromDB()
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
                        return mNodeCacheQueue.Dequeue() as ArchiveApproveReport;
                    }
                }
                else
                {
                    return mNodeCacheQueue.Dequeue() as ArchiveApproveReport;
                }
            }
        }

        public void Dispose()
        {
            FileUtility.ForceDelete(base._dbFilePath);
        }

        public void ReadFromApproveDBByPage(int pageSize)
        {
            using (PerformanceScope pc = new PerformanceScope("SOArchiverAzureDBWorker.ReadFromApproveDBByPage"))
            {
                mLog.Info("Begin ReadFromApproveDBByPage for ScopePath:{0}.", base._dbFilePath);
                var archiverEntities = SelectValuesFromDB(mCurrentRuleId, offset, pageSize);
                offset += archiverEntities.Count;
                foreach (var entity in archiverEntities)
                {
                    mNodeCacheQueue.Enqueue(entity);
                }
                mLog.Info("End ReadFromApproveDBByPage offset:{0} Count:{1}.", offset, archiverEntities.Count);
            }
        }

        public List<string> GetDataRuleCollection()
        {
            using (PerformanceScope pc = new PerformanceScope("SOArchiverAzureDBWorker.GetDataRuleCollection"))
            {
                mLog.Info("Begin GetDataRuleCollection for ScopePath:{0}.", base._dbFilePath);
                var rules = SelectRuleIdsFromDB();
                mLog.Info("End GetDataRuleCollection Count:{0}.", rules.Count);
                return rules;
            }
        }

        public long GetDataCount(int minCacheNodeType = 0)
        {
            using (PerformanceScope pc = new PerformanceScope("SOArchiverAzureDBWorker.GetDataCount"))
            {
                mLog.Info("Begin GetDataCount for ScopePath:{0}.", base._dbFilePath);
                var count = SelectDataCountFromDB(minCacheNodeType);
                mLog.Info("End GetDataCount Count:{0}.", count);
                return count;
            }
        }

        public Dictionary<int, long> GetDataCounts(int minCacheNodeType = 0, string ruleId = "")
        {
            using PerformanceScope pc = new("SOArchiverAzureDBWorker.GetDataCounts");
            mLog.Info("Begin GetDataCounts for ScopePath:{0}.", base._dbFilePath);
            var counts = SelectDataCountsFromDB(minCacheNodeType, ruleId);
            mLog.Info("End GetDataCounts Count:{0}.", counts.Count);
            return counts;
        }

        public List<Guid> ExistInScanJob(List<Guid> nodeIds)
        {
            List<Guid> ids;
            using (PerformanceScope pc = new PerformanceScope("SOArchiverAzureDBWorker.ExistInScanJob"))
            {
                mLog.Info("Begin ExistInScanJob for ScopePath:{0}.", base._dbFilePath);
                ids = SelectExistingItemByNodeIds(nodeIds);
                mLog.Info("End ExistInScanJob Count:{0}.", ids.Count);
                return ids;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listId">List的Unique ID</param>
        /// <param name="containerId">当前需要查询的List/Folder的UniqueId，需要注意的是，RootFolder下的数据，ArchiverDB存储的ParentID为GUID.Empty，因此查询List Root Folder下数据时，此属性值需要赋值为Guid.Empty</param>
        /// <param name="ruleId">当前RuleID</param>
        /// <returns></returns>
        public bool CheckListOrFolderHasFitRuleFile(Guid listId, string containerId, string ruleId)
        {
            using (PerformanceScope pc = new PerformanceScope("SOArchiverAzureDBWorker.CheckListOrFolderHasFitRuleFile"))
            {
                bool hasFitRuleFile = false;

                ExecuteWithConnection(connection =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        hasFitRuleFile = CheckListOrFolderHasFitRuleFile(command, listId, containerId, ruleId);
                    }
                });
                return hasFitRuleFile;
            }
        }

        private bool CheckListOrFolderHasFitRuleFile(IDbCommand command, Guid listId, string containerId, string ruleId)
        {
            bool ListOrFolderHasFitRuleFile = false;
            try
            {
                string query = string.Format($"SELECT Count(*) FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} where CacheNodeType = 10000 and Status = 3 and ListId = @ListId and ParentId = @ParentId and RuleID = @RuleID");
                command.Parameters.Clear();
                command.Parameters.Add(new SQLiteParameter("@ListId", listId));
                command.Parameters.Add(new SQLiteParameter("@ParentId", containerId));
                command.Parameters.Add(new SQLiteParameter("@RuleID", ruleId));
                command.CommandText = query;
                var itemsCount = Convert.ToInt32(command.ExecuteScalar());
                mLog.Info($"CheckListOrFolderHasFitRuleFile listId:{listId}.containerId:{containerId}.ruleId:{ruleId}.itemsCount:{itemsCount}.");
                if (itemsCount > 0)
                {
                    ListOrFolderHasFitRuleFile = true;
                }
            }
            catch (Exception ex)
            {
                mLog.Warn($"CheckListOrFolderHasFitRuleFile Error:{ex}.");
                ListOrFolderHasFitRuleFile = false;
            }
            return ListOrFolderHasFitRuleFile;
        }



        public List<ArchiveApproveReport> SelectItemVersionsWithJsonMeta(string ruleId, Guid nodeId)
        {
            using (PerformanceScope pc = new PerformanceScope("AveSqliteOperation.SelectValuesFromDB"))
            {
                List<ArchiveApproveReport> reports = new List<ArchiveApproveReport>();
                ExecuteWithConnection(connection =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        reports = InternalSelectItemVersionsWithJsonMeta(command, ruleId, nodeId);
                    }
                });
                return reports;
            }
        }

        private List<ArchiveApproveReport> InternalSelectItemVersionsWithJsonMeta(IDbCommand command, string ruleId, Guid nodeId)
        {
            List<ArchiveApproveReport> archiverEntities = new List<ArchiveApproveReport>();
            string query = string.Format("SELECT " +
                "[NodeID]," +
                "[ListId]," +
                "[Path]," +
                $"[JsonMeta] FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} Where [RuleID] = @RuleID And [Status] in (@ApprovedStatus,@CheckOptionStatus) And [NodeID] = @NodeID And CacheNodeType = @CacheNodeType order by SortTicks");

            SQLiteParameter[] parameters = new SQLiteParameter[]
            {
                new SQLiteParameter("@RuleID",ruleId),
                new SQLiteParameter("@NodeID",nodeId.ToString()),
                new SQLiteParameter("@ApprovedStatus",(int)SOApproveDBStatus.Approved),
                new SQLiteParameter("@CheckOptionStatus",(int)SOApproveDBStatus.CheckOption),
                new SQLiteParameter("@CacheNodeType",(int)CacheNodeType.Item),
            };
            foreach (var para in parameters)
            {
                command.Parameters.Add(para);
            }
            command.CommandText = query;
            using (var sr = command.ExecuteReader())
            {
                while (sr.Read())
                {
                    ArchiveApproveReport archiverEn = new ArchiveApproveReport();
                    archiverEn.NodeId = sr.GetGuid(0).ToString();
                    archiverEn.ListID = sr.GetGuid(1);
                    archiverEn.FullPath = sr.GetString(2);
                    archiverEn.JsonMeta = sr.GetString(3);
                    archiverEntities.Add(archiverEn);
                }
            }
            return archiverEntities;
        }

        public List<ArchiveApproveReport> SelectItemsByParentWithJsonMeta(string ruleId, string parentNodeId)
        {
            using (PerformanceScope pc = new PerformanceScope("AveSqliteOperation.SelectValuesFromDBByParentID"))
            {
                List<ArchiveApproveReport> reports = new List<ArchiveApproveReport>();
                ExecuteWithConnection(connection =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        reports = InternalSelectItemsByParentWithJsonMeta(command, ruleId, parentNodeId);
                    }
                });
                return reports;
            }
        }

        private List<ArchiveApproveReport> InternalSelectItemsByParentWithJsonMeta(IDbCommand command, string ruleId, string parentNodeId)
        {
            List<ArchiveApproveReport> archiverEntities = new List<ArchiveApproveReport>();
            string query = string.Format("SELECT " +
                "[NodeID]," +
                "[ListId]," +
                "[Path]," +
                "[JsonMeta]," +
                "[CacheNodeType]" +
                $" FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} Where [RuleID] = @RuleID And [Status] in (@ApprovedStatus,@CheckOptionStatus) And [ParentId] = @ParentId " +
                $"And CacheNodeType >= @CacheNodeTypeFolder And CacheNodeType <= @CacheNodeTypeItem order by SortTicks");

            SQLiteParameter[] parameters =
            [
                new SQLiteParameter("@RuleID", ruleId),
                new SQLiteParameter("@ParentId", parentNodeId),
                new SQLiteParameter("@ApprovedStatus", (int)SOApproveDBStatus.Approved),
                new SQLiteParameter("@CheckOptionStatus", (int)SOApproveDBStatus.CheckOption),
                new SQLiteParameter("@CacheNodeTypeFolder", (int)CacheNodeType.Folder),
                new SQLiteParameter("@CacheNodeTypeItem", (int)CacheNodeType.Item),
            ];
            foreach (var para in parameters)
            {
                command.Parameters.Add(para);
            }
            command.CommandText = query;
            using (var sr = command.ExecuteReader())
            {
                while (sr.Read())
                {
                    ArchiveApproveReport archiverEn = new ArchiveApproveReport();
                    archiverEn.NodeId = sr.GetGuid(0).ToString();
                    archiverEn.ListID = sr.GetGuid(1);
                    archiverEn.FullPath = sr.GetString(2);
                    archiverEn.JsonMeta = sr.GetString(3);
                    archiverEn.CacheNodeType = sr.GetInt32(4);
                    archiverEntities.Add(archiverEn);
                }
            }
            return archiverEntities;
        }

        public void Flush()
        {
            
        }
    }
}