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
using System.Linq;
using System.Text;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Core;
using AvePoint.RA.Common.Util;
using AvePoint.Media.Common;
using AvePoint.RA.Service.Services.JobMonitor.Util;
using AvePoint.RA.Contract.Tenant;
using Amazon.S3.Model.Internal.MarshallTransformations;
using AvePoint.GCommon.Contract.AveModuleContract;

namespace AvePoint.RA.SharePoint.Archiver.Common.ApprovalService
{
    public class SplitScanDBWriterOperation : SqliteDBBase, IApprovalReportOpers
    {
        private static RALogger _mLog = RALogger.GetInstance(typeof(SplitScanDBWriterOperation));

        private readonly static HashSet<int> ALLOW_SPLITED_DB_LEVEL = [(int)CacheNodeType.Item, (int)CacheNodeType.Folder];

        private const string TABLE_NAME = "ArchiverScanTable";

        private int _matchRuleNodeLimit = 1000000;
        private int _matchRuleNodeCount = 0;
        private int _dbIndex = 1;

        private ScheduleConfiguration _mConfiguration;
        private BriefScanDBOperation _briefInformationSqlite;
        private ScanDBOperation _containerCacheSqlite;

        private string BlobFolderUri => string.Join("/", TenantLocalValue.LogonGroupId, "SplitedDBCacheFolder");
        private string BlobFileUri => BlobFolderUri + "/" + _dbName;


        public SplitScanDBWriterOperation(ScheduleConfiguration config)
        {
            _briefInformationSqlite = BriefScanDBOperation.GetInstance(config);
            _containerCacheSqlite = new ScanDBOperation(config.ArchiveTemp, Guid.NewGuid().ToString()+".rpt", config.JobId);
            _dbdirPath = config.ArchiveTemp;
            _mConfiguration = config;
            if(config.ArchiveJobSplitedDBInfo.SplitLimit != null)
            {
                _matchRuleNodeLimit = config.ArchiveJobSplitedDBInfo.SplitLimit.MatchRuleNodeLimit;
            }
            CreateDateBase();
        }

        private void UploadDataBase()
        {
            if (_matchRuleNodeCount > 0)
            {
                UploadDatabase(BlobFileUri, _dbFilePath);
                _mConfiguration.ArchiveJobSplitedDBInfo.SplitedSubsubjobids.Enqueue(string.Format(_mConfiguration.JobId + "{0:D3}", _dbIndex));
                _matchRuleNodeCount = 0;
            }
            FileUtility.ForceDelete(_dbFilePath);
        }

        private void SwitchToNextDataBase()
        {
            _dbIndex++;
            CreateDateBase();
        }

        private void CreateDateBase()
        {
            _dbName = string.Format(_mConfiguration.JobId + "{0:D3}", _dbIndex) + ".rpt";
            _dbFilePath = SecurityUtils.SafeCombinePath(_dbdirPath, _dbName);

            CreateDataBaseIfNotExist(_dbdirPath, _dbName);
        }

        private void CopyAllContainerNode()
        {
            JobDetailHelper.MergeJobDetails(TABLE_NAME, _containerCacheSqlite.DBFilePath, _dbFilePath);
        }

        private void CopyUnRepeatProcessedContainerNodes()
        {
            int page = 0;
            int size = 1000;
            List<ArchiveApproveReport> containerNodes = new List<ArchiveApproveReport>();
            do
            {
                containerNodes = _containerCacheSqlite.SelectUnRepeatProcessedValuesFromDB(page++, size);
                InsertValueToDB(containerNodes);
            } while (containerNodes.Count >= size);            
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
            _mLog.Warn($"SplitAndUploadSqliteOperation.Reset should not reach");
        }

        public void InsertValueToDB(List<ArchiveApproveReport> archiverEntities)
        {
            using (PerformanceScope pc = new PerformanceScope("SplitAndUploadSqliteOperation.InsertValueToDB"))
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
                    query.Append($"INSERT INTO {SecurityUtils.SanitizeSQLSchemaName(TABLE_NAME)}(RowKey,ArchiveLevel,NodeID,ParentId,UIVersion,CacheNodeType,Status,RuleID,DeleteRelatedRecords,ScanJobID,SortTicks,SiteUrl,WebId,ListId,LeafName,Path,ScanTime,LibRowID,NodeType,SPNodeLevel,Level,LastModifiedTime,DoDelete,Size,JsonMeta,IsRepeatProcess,ManifestDocumentSnapshot) ");
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
                    parameters[22].Value = archiverEn.LastModifiedTime;
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


        public List<ArchiveApproveReport> SelectValuesFromDB(string ruleId, int offset, int pageSize)
        {
            _mLog.Warn($"SplitAndUploadSqliteOperation.SelectValuesFromDB should not reach");
            return default;
        }

        public List<string> SelectRuleIdsFromDB()
        {
            _mLog.Warn($"SplitAndUploadSqliteOperation.SelectRuleIdsFromDB should not reach");
            return default;
        }

        

        public long SelectDataCountFromDB()
        {
            _mLog.Warn($"SplitAndUploadSqliteOperation.SelectDataCountFromDB should not reach");
            return default;
        }

        public List<Guid> SelectExistingItemByNodeIds(List<Guid> nodeIds)
        {
            _mLog.Warn($"SplitAndUploadSqliteOperation.SelectExistingItemByNodeIds should not reach");
            return default;
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

        public void UpdateContainerNodeProcessStatus(ArchiveApproveReport nodeEntity)
        {
            using (PerformanceScope pc = new PerformanceScope("SplitAndUploadSqliteOperation.UpdateParentsWithoutRecursion"))
            {
                if (string.IsNullOrWhiteSpace(nodeEntity?.FullPath))
                {
                    throw new Exception("node entiry full path is null");
                }
                List<ArchiveApproveReport> parentNodes = null;
                ExecuteWithConnection(connection =>
                {
                    parentNodes = GetAllParentNodes(connection, nodeEntity);
                });

                ValidParentNodes(parentNodes, nodeEntity);

                _containerCacheSqlite.UpdateNodesToNotNeedProcess();
                _containerCacheSqlite.UpdateNodesToNeedProcess(parentNodes.Select(node => node.EntityRowKey));
            }
        }


        public static void ValidParentNodes(List<ArchiveApproveReport> parentNodes, ArchiveApproveReport currentNode)
        {
            string cacheNodeForLog = BuildNodesInfoStr(currentNode);
            string parentNodesForLog = BuildNodesInfoStr(parentNodes.ToArray());
            _mLog.Info($"Valid parent node relationShip, parent nodes:{parentNodesForLog}, currentNode:{cacheNodeForLog}");

            List<ArchiveApproveReport> sortedNodes = parentNodes.OrderByDescending(node => long.Parse(node.SortTicks)).ToList();
            foreach(ArchiveApproveReport mayParentNode in sortedNodes)
            {
                if(!ValidIsParentNode(mayParentNode, currentNode))
                {
                    //parentNodes.Remove(mayParentNode);
                }
                else
                {
                    currentNode = mayParentNode;
                }
            }

            if(currentNode.CacheNodeType > (int)CacheNodeType.SiteCollection)
            {
                _mLog.Error($"Fail valid parent node relationShip, parent nodes:{parentNodesForLog}, currentNode:{cacheNodeForLog}");
                throw new Exception($"Fail valid parent node relationShip, parent nodes:{parentNodesForLog}, currentNode:{cacheNodeForLog}");
            }
        }


        public static string BuildNodesInfoStr(params ArchiveApproveReport[] nodes)
        {
            StringBuilder res = new StringBuilder();
            foreach( ArchiveApproveReport node in nodes)
            {
                res.AppendLine(BuildNodeInfoStr(node));
            }
            return res.ToString();
        }

        public static string BuildNodeInfoStr(ArchiveApproveReport node)
        {
            var res = new StringBuilder();
            res.Append($"Parameters values: ");
            res.Append($"PartitionKey={node.PartitionKey}, ");
            res.Append($"EntityRowKey={node.EntityRowKey}, ");
            res.Append($"ArchiveLevel={node.ArchiveLevel}, ");
            res.Append($"NodeId={node.NodeId}, ");
            res.Append($"ParentId={node.ParentId}, ");
            res.Append($"UIVersion={node.UIVersion}, ");
            res.Append($"CacheNodeType={node.CacheNodeType}, ");
            res.Append($"Status={node.Status}, ");
            res.Append($"RuleId={node.RuleId}, ");
            res.Append($"DeleteRelatedRecords={node.DeleteRelatedRecords}, ");
            res.Append($"ScanJobID={node.ScanJobID}, ");
            res.Append($"SortTicks={node.SortTicks}, ");
            res.Append($"SiteUrl={node.SiteUrl}, ");
            res.Append($"WebID={node.WebID}, ");
            res.Append($"ListID={node.ListID}, ");
            res.Append($"LeafName={node.LeafName}, ");
            res.Append($"FullPath={node.FullPath}, ");
            res.Append($"ScanTime={node.ScanTime}, ");
            res.Append($"LibRowId={node.LibRowId}, ");
            res.Append($"NodeType={node.NodeType}, ");
            res.Append($"SPNodeLevel={node.SPNodeLevel}, ");
            res.Append($"Level={node.Level}, ");
            res.Append($"LastModifiedTime={node.LastModifiedTime}, ");
            res.Append($"DoDelete={node.DoDelete}, ");
            res.Append($"DocumentSize={node.DocumentSize}, ");
            res.Append($"IsRepeatProcess={node.IsRepeatProcess}");
            return res.ToString();
        }


        public static bool ValidIsParentNode(ArchiveApproveReport parent, ArchiveApproveReport current)
        {
            if(!current.ParentId.ToString().Equals(parent.NodeId, StringComparison.OrdinalIgnoreCase))
            {
                _mLog.Info($"Fail valid parent node relationShip, current node id:{current.NodeId} path:{current.FullPath} node level{current.CacheNodeType}, parent node id:{parent.NodeId} path:{parent.FullPath} node level{parent.CacheNodeType}");
                return false;
            }
            else if (Guid.Empty.ToString().Equals(current.ParentId, StringComparison.OrdinalIgnoreCase))
            {
                string parentPath = FormatNodePath(parent.FullPath, parent.SiteUrl);
                string currentPath = FormatNodePath(current.FullPath, current.SiteUrl);
                if (!currentPath.StartsWith(parentPath, StringComparison.OrdinalIgnoreCase))
                {
                    _mLog.Info($"Fail valid parent node relationShip, current node rowId:{current.EntityRowKey} path:{current.FullPath}  node level{current.CacheNodeType}, parent node id:{parent.EntityRowKey} path:{parent.FullPath} node level{parent.CacheNodeType}");
                    return false;
                }
            }
            return true;
        }

        public static string FormatNodePath(string path, string scUrl)
        {
            path = path.Replace('\\', '/').Trim('/') + '/';
            scUrl = scUrl.Replace('\\', '/').Trim('/') + '/';
            if (!path.StartsWith(scUrl, StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }
            else
            {
                Uri uri = new Uri(path);
                string AbsolutePath = uri.AbsolutePath.Replace('\\', '/').Trim('/') + '/';
                if (path.EndsWith(AbsolutePath, StringComparison.OrdinalIgnoreCase))
                {
                    return AbsolutePath;
                }
                else
                {
                    return Uri.UnescapeDataString(AbsolutePath);
                }
            }
        }


        private List<ArchiveApproveReport> GetAllParentNodes(SQLiteConnection conn, ArchiveApproveReport nodeEntity)
        {
            List<ArchiveApproveReport> parentNodes = new List<ArchiveApproveReport>();

            int page = 0;
            int size = 500;
            string nodePath = FormatNodePath(nodeEntity.FullPath, nodeEntity.SiteUrl);
            string siteCollectionUrl = FormatNodePath(nodeEntity.SiteUrl, nodeEntity.SiteUrl);

            List<ArchiveApproveReport> containers = new ();
            do
            {
                containers = _containerCacheSqlite.SelectValuesFromDB(page++ * size, size);
                foreach(ArchiveApproveReport container in containers)
                {
                    string path = FormatNodePath(container.FullPath, nodeEntity.SiteUrl);
                    if (nodePath.StartsWith(path, StringComparison.OrdinalIgnoreCase) || path.Equals(siteCollectionUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        parentNodes.Add(container);
                    }
                }
            } while (containers.Count >= size);

            return parentNodes;
        }

        public void AddToDB(ArchiveApproveReport nodeEntity, bool hasReported)
        {
            if (string.IsNullOrWhiteSpace(nodeEntity.EntityRowKey))
            {
                var ticks = Snowflake.Instance().GetTicks();
                nodeEntity.EntityRowKey = _mConfiguration.JobId + "_" + ticks;
                nodeEntity.SortTicks = ticks.ToString();
            }

            if (_matchRuleNodeLimit <= _matchRuleNodeCount && ALLOW_SPLITED_DB_LEVEL.Contains(nodeEntity.CacheNodeType))
            {
                CopyUnRepeatProcessedContainerNodes();
                UploadDataBase();

                UpdateContainerNodeProcessStatus(nodeEntity);
                SwitchToNextDataBase();
            }

            if (string.IsNullOrWhiteSpace(nodeEntity.RuleId))
            {
                nodeEntity.RuleId = Guid.Empty.ToString();
            }
            else
            {
                _matchRuleNodeCount++;
            }
            
            _briefInformationSqlite.AddToDB(nodeEntity, hasReported);

            if (IsContainerLevel(nodeEntity.CacheNodeType))
            {
                _containerCacheSqlite.AddToDB(nodeEntity, hasReported);
            }
            else
            {
                InsertValueToDB(new List<ArchiveApproveReport>() { nodeEntity });
            }
        }

        

        private bool IsContainerLevel(int cacheNodeType)
        {
            return cacheNodeType < (int)CacheNodeType.Item;
        }

        public void AddScanReport(ArchiveApproveReport nodeEntity)
        {
        }

        public ArchiveApproveReport ReadFromDB()
        {
            _mLog.Warn($"SplitAndUploadSqliteOperation.ReadFromDB should not reach");
            return default;
        }

        public void Dispose()
        {
            if (_matchRuleNodeCount > 0)
            {
                CopyAllContainerNode();
            }
            UploadDataBase();
            _containerCacheSqlite.Dispose();
        }

        public void ReadFromApproveDBByPage(int pageSize)
        {
            _mLog.Warn($"SplitAndUploadSqliteOperation.ReadFromApproveDBByPage should not reach");
        }

        public List<string> GetDataRuleCollection()
        {
            _mLog.Warn($"SplitAndUploadSqliteOperation.GetDataRuleCollection should not reach");
            return default;
        }

        public long GetDataCount(int minCacheNodeType = 0)
        {
            _mLog.Warn($"SplitAndUploadSqliteOperation.GetDataCount should not reach");
            return default;
        }

        public Dictionary<int, long> GetDataCounts(int minCacheNodeType = 0, string ruleId = "")
        {
            _mLog.Warn($"SplitAndUploadSqliteOperation.GetDataCounts should not reach");
            return default;
        }

        public List<Guid> ExistInScanJob(List<Guid> nodeIds)
        {
            _mLog.Warn($"SplitAndUploadSqliteOperation.ExistInScanJob should not reach");
            return default;
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
            _mLog.Warn($"SplitAndUploadSqliteOperation.CheckListOrFolderHasFitRuleFile should not reach");
            return default;
        }


        public List<ArchiveApproveReport> SelectItemVersionsWithJsonMeta(string ruleId, Guid nodeId)
        {
            _mLog.Warn($"SplitAndUploadSqliteOperation.SelectItemVersionsWithJsonMeta should not reach");
            return default;
        }

        public List<ArchiveApproveReport> SelectItemsByParentWithJsonMeta(string ruleId, string parentNodeId)
        {
            _mLog.Warn($"SplitAndUploadSqliteOperation.SelectItemsByParentWithJsonMeta should not reach");
            return default;
        }

        public bool CheckListOrFolderHasFitRuleFile(IDbCommand command, Guid listId, string containerId, string ruleId)
        {
            _mLog.Warn($"SplitAndUploadSqliteOperation.CheckListOrFolderHasFitRuleFile should not reach");
            return default;
        }

        public void Flush()
        {
            if (_matchRuleNodeCount > 0)
            {
                CopyAllContainerNode();
            }
            UploadDataBase();
        }
    }
}