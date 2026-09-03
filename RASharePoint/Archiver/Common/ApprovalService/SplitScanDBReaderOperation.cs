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
using AvePoint.RA.Contract;
using AvePoint.RA.SharePoint.ArchiverCommon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Collections;
using AvePoint.RA.CommonUtil;
using AvePoint.Media.Common;
using AvePoint.RA.Contract.Tenant;


namespace AvePoint.RA.SharePoint.Archiver.Common.ApprovalService
{
    public class SplitScanDBReaderOperation : SqliteDBBase, IApprovalReportOpers
    {
        private static readonly RALogger _mLog = RALogger.GetInstance(typeof(SplitScanDBReaderOperation));

        private ScheduleConfiguration _mConfiguration;
        private string _mCurrentRuleId = EMPTY_RULE_ID;
        private Queue _mNodeCacheQueue = new Queue();
        private BriefScanDBOperation _briefInformationSqlite;
        private int _offset = 0;

        private const int READ_PAGE_COUNT = 5000;
        private const string TABLE_NAME = "ArchiverScanTable";
        private const string EMPTY_RULE_ID = null;

        private string BlobFolderUri => string.Join("/", TenantLocalValue.LogonGroupId, "SplitedDBCacheFolder");
        private string BlobFileUri => BlobFolderUri + "/" + _dbName;

        public SplitScanDBReaderOperation(ScheduleConfiguration config)
        {
            base._dbName = config.JobId + ".rpt";
            base._dbdirPath = config.ArchiveTemp;
            base._dbFilePath = SecurityUtils.SafeCombinePath(_dbdirPath, base._dbName);
            _mConfiguration = config;
            _briefInformationSqlite = BriefScanDBOperation.GetInstance(_mConfiguration);

            if (config.ArchiveJobSplitedDBInfo.IsUseSplitedDB)
            {
                DownloadDBFromBlob(BlobFileUri, _dbFilePath);
                if(_mConfiguration?.ArchiveJobSplitedDBInfo?.SplitLimit?.NotDeleteScanDBFromBlobForTest == true)
                {
                    _mLog.Info("not delete cache scan db for test");
                }
                else
                {
                    RAStorageUtil.DeleteReportBlob(BlobFileUri);
                }
            }

            if (!File.Exists(_dbFilePath)) {
                throw new Exception($"Fail get splited db that contains data,bloburi:{BlobFileUri}, db file path:{_dbFilePath}");
            }
        }



        public override void CreateSchemaIfNotExists(IDbCommand command)
        {
            string query = string.Format($"CREATE TABLE IF NOT EXISTS {TABLE_NAME}(" +
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
                $"CREATE INDEX IF NOT EXISTS SortTicksIndex ON {SecurityUtils.SanitizeSQLSchemaName(TABLE_NAME)}(SortTicks asc);" +
                $"CREATE INDEX IF NOT EXISTS NodeID ON {SecurityUtils.SanitizeSQLSchemaName(TABLE_NAME)}(NodeID asc)");

            command.CommandText = query;

            command.ExecuteNonQuery();
        }

        public void Reset(string ruleId)
        {
            if (string.IsNullOrEmpty(ruleId))
            {
                _mCurrentRuleId = EMPTY_RULE_ID;
            }
            else
            {
                _mCurrentRuleId = ruleId;
            }
            _offset = 0;
            _mNodeCacheQueue.Clear();
        }


        public List<ArchiveApproveReport> SelectValuesFromDB(string ruleId, int offset, int pageSize)
        {
            using (PerformanceScope pc = new PerformanceScope("DownloadSplitedSqliteOperation.SelectValuesFromDB"))
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
                $" FROM {SecurityUtils.SanitizeSQLSchemaName(TABLE_NAME)} Where [RuleID] in (@RuleID,@EmptyRuleID) And [Status] in (@ApprovedStatus,@CheckOptionStatus) order by SortTicks limit {pageSize} offset {offset}");

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

        public List<string> SelectRuleIdsFromDB()
        {
            using (PerformanceScope pc = new PerformanceScope("DownloadSplitedSqliteOperation.SelectRuleIdsFromDB"))
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
            string query = string.Format("SELECT DISTINCT RuleID"+
                $" FROM {SecurityUtils.SanitizeSQLSchemaName(TABLE_NAME)} Where [RuleID] <> @EmptyRuleID And [Status] in (@ApprovedStatus,@CheckOptionStatus)");

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
            using (PerformanceScope pc = new PerformanceScope("DownloadSplitedSqliteOperation.SelectDataCountFromDB"))
            {
                long count = 0;
                ExecuteWithConnection(connection =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        count = InternalSelectDataCountFromDB(command, minCacheNodeType);
                    }
                });
                return count;
            }
        }

        public Dictionary<int, long> SelectDataCountsFromDB(int minCacheNodeType = 0, string ruleId = "")
        {
            using (PerformanceScope pc = new PerformanceScope("DownloadSplitedSqliteOperation.SelectDataCountsFromDB"))
            {
                var counts = new Dictionary<int, long>();
                ExecuteWithConnection(connection =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        counts = InternalSelectDataCountsFromDB(command, minCacheNodeType, ruleId);
                    }
                });
                return counts;
            }
        }

        private long InternalSelectDataCountFromDB(IDbCommand command, int minCacheNodeType = 0)
        {
            long count = 0;
            string query = string.Format("SELECT COUNT(*) " +
                $" FROM {SecurityUtils.SanitizeSQLSchemaName(TABLE_NAME)} Where [Status] in (@ApprovedStatus,@CheckOptionStatus)");

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
                $" FROM {SecurityUtils.SanitizeSQLSchemaName(TABLE_NAME)} Where [Status] in (@ApprovedStatus,@CheckOptionStatus)");

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
            _mLog.Warn($"DownloadSplitedSqliteOperation.AddToDB should not reach");
            return;
        }

        public void AddScanReport(ArchiveApproveReport nodeEntity)
        {
        }

        public ArchiveApproveReport ReadFromDB()
        {
            lock (_mNodeCacheQueue.SyncRoot)
            {
                if (_mNodeCacheQueue.Count == 0)
                {
                    ReadFromApproveDBByPage(READ_PAGE_COUNT);
                    if (_mNodeCacheQueue.Count == 0)
                    {
                        return null;
                    }
                    else
                    {
                        return _mNodeCacheQueue.Dequeue() as ArchiveApproveReport;
                    }
                }
                else
                {
                    return _mNodeCacheQueue.Dequeue() as ArchiveApproveReport;
                }
            }
        }

        public void Dispose()
        {
            FileUtility.ForceDelete(_dbFilePath);
            if(_mConfiguration.ArchiveJobSplitedDBInfo.IsLatestSplitedDB == true)
            {
                _briefInformationSqlite.Dispose();
            }
        }

        public void ReadFromApproveDBByPage(int pageSize)
        {
            using (PerformanceScope pc = new PerformanceScope("DownloadSplitedSqliteOperation.ReadFromApproveDBByPage"))
            {
                _mLog.Info("Begin ReadFromApproveDBByPage for ScopePath:{0}.", base._dbFilePath);
                var archiverEntities = SelectValuesFromDB(_mCurrentRuleId, _offset, pageSize);
                _offset += archiverEntities.Count;
                foreach (var entity in archiverEntities)
                {
                    _mNodeCacheQueue.Enqueue(entity);
                }
                _mLog.Info("End ReadFromApproveDBByPage offset:{0} Count:{1}.", _offset, archiverEntities.Count);
            }
        }

        public List<string> GetDataRuleCollection()
        {
            using (PerformanceScope pc = new PerformanceScope("DownloadSplitedSqliteOperation.GetDataRuleCollection"))
            {
                _mLog.Info("Begin GetDataRuleCollection for ScopePath:{0}.", base._dbFilePath);
                var rules = SelectRuleIdsFromDB();
                _mLog.Info("End GetDataRuleCollection Count:{0}.", rules.Count);
                return rules;
            }
        }

        public long GetDataCount(int minCacheNodeType = 0)
        {
            using (PerformanceScope pc = new PerformanceScope("DownloadSplitedSqliteOperation.GetDataCount"))
            {
                _mLog.Info("Begin GetDataCount for ScopePath:{0}.", base._dbFilePath);
                var count = SelectDataCountFromDB(minCacheNodeType);
                _mLog.Info("End GetDataCount Count:{0}.", count);
                return count;
            }
        }

        public Dictionary<int, long> GetDataCounts(int minCacheNodeType = 0, string ruleId = "")
        {
            using PerformanceScope pc = new("DownloadSplitedSqliteOperation.GetDataCounts");
            _mLog.Info("Begin GetDataCounts for ScopePath:{0}.", base._dbFilePath);
            var counts = SelectDataCountsFromDB(minCacheNodeType, ruleId);
            _mLog.Info("End GetDataCounts Count:{0}.", counts.Count);
            return counts;
        }

        public List<Guid> ExistInScanJob(List<Guid> nodeIds)
        {
            return _briefInformationSqlite.ExistInScanJob(nodeIds);
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
            using (PerformanceScope pc = new PerformanceScope("DownloadSplitedSqliteOperation.CheckListOrFolderHasFitRuleFile"))
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
                string query = string.Format($"SELECT Count(*) FROM {SecurityUtils.SanitizeSQLSchemaName(TABLE_NAME)} where CacheNodeType = 10000 and Status = 3 and ListId = @ListId and ParentId = @ParentId and RuleID = @RuleID");
                command.Parameters.Clear();
                command.Parameters.Add(new SQLiteParameter("@ListId", listId));
                command.Parameters.Add(new SQLiteParameter("@ParentId", containerId));
                command.Parameters.Add(new SQLiteParameter("@RuleID", ruleId));
                command.CommandText = query;
                var itemsCount = Convert.ToInt32(command.ExecuteScalar());
                _mLog.Info($"CheckListOrFolderHasFitRuleFile listId:{listId}.containerId:{containerId}.ruleId:{ruleId}.itemsCount:{itemsCount}.");
                if (itemsCount > 0)
                {
                    ListOrFolderHasFitRuleFile = true;
                }
            }
            catch (Exception ex)
            {
                _mLog.Warn($"CheckListOrFolderHasFitRuleFile Error:{ex}.");
                ListOrFolderHasFitRuleFile = false;
            }
            return ListOrFolderHasFitRuleFile;
        }



        public List<ArchiveApproveReport> SelectItemVersionsWithJsonMeta(string ruleId, Guid nodeId)
        {
            using (PerformanceScope pc = new PerformanceScope("DownloadSplitedSqliteOperation.SelectValuesFromDB"))
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
                $"[JsonMeta] FROM {SecurityUtils.SanitizeSQLSchemaName(TABLE_NAME)} Where [RuleID] = @RuleID And [Status] in (@ApprovedStatus,@CheckOptionStatus) And [NodeID] = @NodeID And CacheNodeType = @CacheNodeType order by SortTicks");

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
            using (PerformanceScope pc = new PerformanceScope("DownloadSplitedSqliteOperation.SelectValuesFromDBByParentID"))
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
                $" FROM {SecurityUtils.SanitizeSQLSchemaName(TABLE_NAME)} Where [RuleID] = @RuleID And [Status] in (@ApprovedStatus,@CheckOptionStatus) And [ParentId] = @ParentId " +
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
