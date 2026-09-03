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
using AvePoint.RA.SharePoint.ArchiverCommon;
using RAGoogle.Archive.Scan.Base;
using RAGoogle.Common;
using System.Collections;
using System.Data;
using System.Data.SQLite;
using System.Text;

namespace RAGoogle.Archive.ApprovalService
{
    internal class AveSqliteOperation : SqliteDBBase, IApprovalReportOpers
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(GoogleScannerBase));
        const string EMPTYRULEID = null;
        private GoogleConfiguration mConfiguration;
        private string mCurrentRuleId = EMPTYRULEID;
        private string tableName = "ArchiverScanTable";
        private Queue mNodeCacheQueue = new Queue();
        private int readPageCount = 5000;//一次从SQLite中查出5000个记录。

        private int offset = 0;

        public AveSqliteOperation(GoogleConfiguration config) : base(config.ArchiveTemp, config.ScanDBName)
        {
            mConfiguration = config;

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
                //"[SiteUrl] [nvarchar](2000)," +
                //"[WebId] [uniqueidentifier] not null," +
                //"[ListId] [uniqueidentifier] not null," +
                "[LeafName] [nvarchar](255)," +
                "[Path] [nvarchar](512)," +
                "[ScanTime] [datetime]," +
                //"[LibRowID] [int]," +
                "[NodeType] [int]," +
                "[SPNodeLevel] [int]," +
                //"[Level] [tinyint]," +
                "[LastModifiedTime] [bigint]," +
                "[DoDelete] [Boolean]," +
                "[Size] [bigint]," +
                "[TermId][nvarchar](128)," +
                "[JsonMeta] [nvarchar](4000));" +
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
                    query.Append($"INSERT INTO {SecurityUtils.SanitizeSQLSchemaName(tableName)}(RowKey,ArchiveLevel,NodeID,ParentId,UIVersion,CacheNodeType,Status,RuleID,DeleteRelatedRecords,ScanJobID,SortTicks,LeafName,Path,ScanTime,NodeType,SPNodeLevel,LastModifiedTime,DoDelete,Size,TermId,JsonMeta) ");
                    //string query = string.Format("INSERT INTO {0} VALUES (@PartitionKey)", tableName, SplicingInsertValueString(archiverEntities));
                    query.Append(@"VALUES (@RowKey,@ArchiveLevel,@NodeID,@ParentId,@UIVersion,@CacheNodeType,@Status,@RuleID,
@DeleteRelatedRecords,@ScanJobID,@SortTicks,@LeafName,@Path,@ScanTime,@NodeType,@SPNodeLevel,@LastModifiedTime,@DoDelete,@Size,@TermId,@JsonMeta)");
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
                    //new SQLiteParameter("@SiteUrl"),
                    //new SQLiteParameter("@WebId"),
                    //new SQLiteParameter("@ListId"),
                    new SQLiteParameter("@LeafName"),
                    new SQLiteParameter("@Path"),
                    new SQLiteParameter("@ScanTime"),
                    //new SQLiteParameter("@LibRowID"),
                    new SQLiteParameter("@NodeType"),
                    new SQLiteParameter("@SPNodeLevel"),
                    //new SQLiteParameter("@Level"),
                    new SQLiteParameter("@LastModifiedTime"),
                    new SQLiteParameter("@DoDelete"),
                    new SQLiteParameter("@Size"),
                    new SQLiteParameter("@TermId"),
                    new SQLiteParameter("@JsonMeta")
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
                    //parameters[12].Value = archiverEn.SiteUrl;
                    //parameters[13].Value = archiverEn.WebID;
                    //parameters[14].Value = archiverEn.ListID;
                    parameters[12].Value = archiverEn.LeafName;
                    parameters[13].Value = archiverEn.FullPath;
                    parameters[14].Value = archiverEn.ScanTime;
                    //parameters[18].Value = archiverEn.LibRowId;
                    parameters[15].Value = archiverEn.NodeType;
                    parameters[16].Value = archiverEn.SPNodeLevel;
                    //parameters[21].Value = archiverEn.Level;
                    parameters[17].Value = archiverEn.LastModifiedTime;
                    parameters[18].Value = archiverEn.DoDelete;
                    parameters[19].Value = archiverEn.DocumentSize;
                    parameters[20].Value = archiverEn.TermId;
                    parameters[21].Value = archiverEn.JsonMeta;

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


        public List<ArchiveApproveReport> SelectValuesFromDB(string ruleId, int offset, int pageSize)
        {
            using (PerformanceScope pc = new PerformanceScope("AveSqliteOperation.SelectValuesFromDB"))
            {
                List<ArchiveApproveReport> reports = new List<ArchiveApproveReport>();
                ExecuteWithConnection(connection =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        reports = InternalSelectValuesFromDB(command, ruleId, offset, pageSize);
                    }
                });
                return reports;
            }
        }

        private List<ArchiveApproveReport> InternalSelectValuesFromDB(IDbCommand command, string ruleId, int offset, int pageSize)
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
                //"[SiteUrl]," +
                //"[WebId]," +
                //"[ListId]," +
                "[LeafName]," +
                "[Path]," +
                "[ScanTime]," +
                //"[LibRowID]," +
                "[NodeType]," +
                "[SPNodeLevel]," +
                //"[Level]," +
                "[LastModifiedTime]," +
                "[DoDelete]," +
                $"[Size],[TermId],[JsonMeta] FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} Where [RuleID] in (@RuleID,@EmptyRuleID) And [Status] in (@ApprovedStatus,@CheckOptionStatus) order by SortTicks limit {pageSize} offset {offset}");

            SQLiteParameter[] parameters = new SQLiteParameter[]
            {
                new SQLiteParameter("@RuleID",ruleId),
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
                    //archiverEn.SiteUrl = sr.GetString(11);
                    //archiverEn.WebID = sr.GetGuid(12);
                    //archiverEn.ListID = sr.GetGuid(13);
                    archiverEn.LeafName = sr.GetString(11);
                    archiverEn.FullPath = sr.GetString(12);
                    archiverEn.ScanTime = sr.GetInt64(13);
                    //archiverEn.LibRowId = sr.GetInt32(17);
                    archiverEn.NodeType = sr.GetInt32(14);
                    archiverEn.SPNodeLevel = sr.GetInt32(15);
                    //archiverEn.Level = Convert.ToByte(sr.GetInt32(20));
                    archiverEn.LastModifiedTime = sr.GetInt64(16);
                    archiverEn.DoDelete = sr.GetBoolean(17);
                    archiverEn.DocumentSize = sr.GetInt64(18);
                    archiverEn.TermId = sr.IsDBNull(19) ? string.Empty : sr.GetString(19);
                    archiverEn.JsonMeta = sr.IsDBNull(20) ? string.Empty : sr.GetString(20);
                    archiverEntities.Add(archiverEn);
                }
            }
            return archiverEntities;
        }

        public List<string> SelectRuleIdsFromDB()
        {
            using (PerformanceScope pc = new PerformanceScope("AveSqliteOperation.SelectRuleIdsFromDB"))
            {
                List<string> reports = new List<string>();
                ExecuteWithConnection(connection =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        reports = InternalSelectRuleIdsFromDB(command);
                    }
                });
                return reports;
            }
        }

        private List<string> InternalSelectRuleIdsFromDB(IDbCommand command)
        {
            List<string> ruleids = new List<string>();
            string query = string.Format("SELECT DISTINCT RuleID" +
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

        public long SelectDataCountFromDB()
        {
            using (PerformanceScope pc = new PerformanceScope("AveSqliteOperation.SelectDataCountFromDB"))
            {
                long count = 0;
                ExecuteWithConnection(connection =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        count = InternalSelectDataCountFromDB(command);
                    }
                });
                return count;
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

        private long InternalSelectDataCountFromDB(IDbCommand command)
        {
            long count = 0;
            string query = string.Format("SELECT COUNT(*) " +
                $" FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} Where [Status] in (@ApprovedStatus,@CheckOptionStatus)");

            SQLiteParameter[] parameters = new SQLiteParameter[]
            {
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
                    count = sr.GetInt64(0);
                }
            }
            return count;
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
            var ticks = Snowflake.Instance().GetTicks();
            nodeEntity.EntityRowKey = mConfiguration.JobId + "_" + ticks;
            nodeEntity.SortTicks = ticks.ToString();
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

        public List<string> GetDataRuleCollection()
        {
            using (PerformanceScope pc = new PerformanceScope("SOArchiverAzureDBWorker.GetDataRuleCollection"))
            {
                mLog.Info("Begin GetDataRuleCollection for ScopePath:{0}.", _dbFilePath);
                var rules = SelectRuleIdsFromDB();
                mLog.Info("End GetDataRuleCollection Count:{0}.", rules.Count);
                return rules;
            }
        }

        public long GetDataCount()
        {
            using (PerformanceScope pc = new PerformanceScope("SOArchiverAzureDBWorker.GetDataCount"))
            {
                mLog.Info("Begin GetDataCount for ScopePath:{0}.", _dbFilePath);
                var count = SelectDataCountFromDB();
                mLog.Info("End GetDataCount Count:{0}.", count);
                return count;
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
    }
}