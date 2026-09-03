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
using AvePoint.RA.SharePoint.ArchiverCommon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.IO;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Common.Util;
using AvePoint.Media.Common;

namespace AvePoint.RA.SharePoint.Archiver.Common.ApprovalService
{
    internal class BriefScanDBOperation : SqliteDBBase, IApprovalReportOpers
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(BriefScanDBOperation));

        private const string BRIEF_TABLE_NAME = "BriefInfos";

        private const string FAIL_PROCESS_TABLE_NAME = "FailProcessNodes";

        #region instance
        private readonly static object _instanceLock = new object();

        private static BriefScanDBOperation _instance;

        public static BriefScanDBOperation GetInstance(ScheduleConfiguration config)
        {
            lock (_instanceLock)
            {
                if (_instance == null)
                {
                    _instance = new BriefScanDBOperation(config);
                }
                return _instance;
            }
        }

        private BriefScanDBOperation(ScheduleConfiguration config)
        {
            CreateDataBaseIfNotExist(config.ArchiveTemp, "Brief_" + config.ScanDBName);
        }
        #endregion


        public override void CreateSchemaIfNotExists(IDbCommand command)
        {
            string query = string.Format(
                @$"CREATE TABLE IF NOT EXISTS {BRIEF_TABLE_NAME}( 
                [RowKey] [nvarchar](500) NOT NULL, 
                [ArchiveLevel] [int], 
                [NodeID] [uniqueidentifier] not null); 
                CREATE INDEX IF NOT EXISTS NodeID ON {SecurityUtils.SanitizeSQLSchemaName(BRIEF_TABLE_NAME)}(NodeID asc); 
                
                CREATE TABLE IF NOT EXISTS {FAIL_PROCESS_TABLE_NAME}( 
                [RowKey] [nvarchar](500) NOT NULL, 
                [NodeID] [uniqueidentifier] not null);
                CREATE INDEX IF NOT EXISTS NodeID ON {SecurityUtils.SanitizeSQLSchemaName(FAIL_PROCESS_TABLE_NAME)}(NodeID asc);
                ");

            command.CommandText = query;
            command.ExecuteNonQuery();
        }

        public void Reset(string ruleId)
        {
            
        }

        public void InsertValueToDB(List<ArchiveApproveReport> archiverEntities)
        {
            using (PerformanceScope pc = new PerformanceScope("BriefInfoSqliteOperation.InsertValueToDB"))
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

        private void InternalInsertValueToDB(SQLiteConnection conn, IDbCommand command, List<ArchiveApproveReport> archiverEntities)
        {
            using (SQLiteTransaction tr = conn.BeginTransaction())
            {
                foreach (var archiverEn in archiverEntities)
                {
                    StringBuilder query = new StringBuilder();
                    query.Append($"INSERT INTO {SecurityUtils.SanitizeSQLSchemaName(BRIEF_TABLE_NAME)}(RowKey,NodeID) ");
                    query.Append(@"VALUES (@RowKey,@NodeID)");
                    List<SQLiteParameter> parameters = [
                    new SQLiteParameter("@RowKey"),
                    new SQLiteParameter("@NodeID"),
                   ];
                    parameters[0].Value = archiverEn.EntityRowKey;
                    parameters[1].Value = archiverEn.NodeId;

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

        public void InsertFailProcessedNodeToDB(params ArchiveApproveReport[] archiverEntities)
        {
            using (PerformanceScope pc = new PerformanceScope("BriefInfoSqliteOperation.InsertFailProcessedNodeToDB"))
            {
                ExecuteWithConnection(connection =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        InternalInsertFailProcessedNodeToDB(connection, command, archiverEntities);
                    }
                });
            }
        }

        private void InternalInsertFailProcessedNodeToDB(SQLiteConnection conn, IDbCommand command, ArchiveApproveReport[] archiverEntities)
        {
            using (SQLiteTransaction tr = conn.BeginTransaction())
            {
                foreach (var archiverEn in archiverEntities)
                {
                    StringBuilder query = new StringBuilder();
                    query.Append($"INSERT INTO {SecurityUtils.SanitizeSQLSchemaName(FAIL_PROCESS_TABLE_NAME)}(RowKey,NodeID) ");
                    query.Append(@"VALUES (@RowKey,@NodeID)");
                    List<SQLiteParameter> parameters = [
                    new SQLiteParameter("@RowKey"),
                    new SQLiteParameter("@NodeID"),
                   ];
                    parameters[0].Value = archiverEn.EntityRowKey;
                    parameters[1].Value = archiverEn.NodeId;

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

        public bool NodeIsFailProcessed(ArchiveApproveReport archiverEntitie)
        {
            bool res = false;
            using (PerformanceScope pc = new PerformanceScope("BriefInfoSqliteOperation.NodeIsFailProcessed"))
            {
                ExecuteWithConnection(connection =>
                {
                    using (var command = connection.CreateCommand())
                    {
                       res = InternalNodeIsFillProcessed(command, archiverEntitie);
                    }
                });
            }
            return res;
        }


        private bool InternalNodeIsFillProcessed(IDbCommand command, ArchiveApproveReport entity)
        {
            string query = string.Format("SELECT COUNT(*) " +
                $" FROM {SecurityUtils.SanitizeSQLSchemaName(FAIL_PROCESS_TABLE_NAME)} Where [RowKey] = @RowKey");

            command.Parameters.Add(new SQLiteParameter("@RowKey", entity.EntityRowKey));
            command.CommandText = query;
            using var sr = command.ExecuteReader();
            sr.Read();
            long count = sr.GetInt64(0);
            return count > 0;
        }

        public List<ArchiveApproveReport> SelectValuesFromDB(string ruleId, int offset, int pageSize)
        {
            mLog.Warn($"BriefInfoSqliteOperation.SelectValuesFromDB should not reach");
            return default;
        }

        
        public List<string> SelectRuleIdsFromDB()
        {
            mLog.Warn($"BriefInfoSqliteOperation.SelectRuleIdsFromDB should not reach");
            return default;
        }

        public long SelectDataCountFromDB()
        {
            mLog.Warn($"BriefInfoSqliteOperation.SelectDataCountFromDB should not reach");
            return default;
        }

        public List<Guid> SelectExistingItemByNodeIds(List<Guid> nodeIds)
        {
            List<Guid> ids = new List<Guid>();
            using (PerformanceScope pc = new PerformanceScope("BriefInfoSqliteOperation.SelectExistingItemByNodeIds"))
            {
                for (int i = 0; i < nodeIds.Count; i += 50)
                {
                    var tempIds = nodeIds.Skip(i).Take(50);
                    ExecuteWithConnection(connection =>
                    {
                        using (var command = connection.CreateCommand())
                        {
                            string query = @$"SELECT NodeId FROM {SecurityUtils.SanitizeSQLSchemaName(BRIEF_TABLE_NAME)} WHERE NodeId in {DatabaseUtility.BuildInClause<Guid>(tempIds)}";
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

        public void AddToDB(ArchiveApproveReport nodeEntity, bool hasReported)
        {
            InsertValueToDB(new List<ArchiveApproveReport>() { nodeEntity });
        }

        public void AddScanReport(ArchiveApproveReport nodeEntity)
        {

        }

        public ArchiveApproveReport ReadFromDB()
        {
            mLog.Warn($"BriefInfoSqliteOperation.ReadFromDB should not reach");
            return default;
        }

        public void Dispose()
        {
            lock (_instanceLock)
            {
                BriefScanDBOperation._instance = null;
            }
            FileUtility.ForceDelete(_dbFilePath);
        }

        public void ReadFromApproveDBByPage(int pageSize)
        {

        }

        public List<string> GetDataRuleCollection()
        {
            mLog.Warn($"BriefInfoSqliteOperation.GetDataRuleCollection should not reach");
            return default;
        }

        public long GetDataCount(int minCacheNodeType = 0)
        {
            mLog.Warn($"BriefInfoSqliteOperation.GetDataCount should not reach");
            return default;
        }

        public Dictionary<int, long> GetDataCounts(int minCacheNodeType = 0, string ruleId = "")
        {
            mLog.Warn($"BriefInfoSqliteOperation.GetDataCounts should not reach");
            return default;
        }

        public List<Guid> ExistInScanJob(List<Guid> nodeIds)
        {
            List<Guid> ids;
            using (PerformanceScope pc = new PerformanceScope("BriefInfoSqliteOperation.ExistInScanJob"))
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
            mLog.Warn($"BriefInfoSqliteOperation.CheckListOrFolderHasFitRuleFile should not reach");
            return default;
        }

        public List<ArchiveApproveReport> SelectItemVersionsWithJsonMeta(string ruleId, Guid nodeId)
        {
            mLog.Warn($"BriefInfoSqliteOperation.SelectItemVersionsWithJsonMeta should not reach");
            return default;
        }

        
        public List<ArchiveApproveReport> SelectItemsByParentWithJsonMeta(string ruleId, string parentNodeId)
        {
            mLog.Warn($"BriefInfoSqliteOperation.SelectItemsByParentWithJsonMeta should not reach");
            return default;
        }

        public void Flush()
        {
        }
    }
}