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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using RAArchiverCommon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HSMCommon
{
    public abstract class SqliteDBBase
    {

        private static AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(SqliteDBBase));
        protected string dbName;
        private string dbdirPath;
        public string dbFilePath;
        public SqliteDBBase(string tenantGroupId, string jobid)
        {
            dbdirPath = Path.Combine(BackgroundSettings.GetInstance().ArchiveTemp, tenantGroupId, jobid, "DB");
            if (!Directory.Exists(dbdirPath))
            {
                DirectoryInfo dbdir = new DirectoryInfo(dbdirPath);
                try
                {
                    dbdir.Create();
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to create database directory {dbdirPath}.", ex);
                    throw;
                }
            }
            dbName = Guid.NewGuid().ToString() + "_data.db";
            dbFilePath = Path.Combine(dbdirPath, dbName);

            InitializeDb();
        }
        public virtual SQLiteConnection GetConnection()
        {
            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = dbFilePath;
            return new SQLiteConnection(builder.ToString());
        }

        private void InitializeDb() => ExecuteWithConnection(connection =>
        {
            using (var command = connection.CreateCommand())
                CreateSchemaIfNotExists(command);
        });

        public abstract void CreateSchemaIfNotExists(IDbCommand command);

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
                    logger.Warn($@"fail set dir last accesstime utc,ex:{e}");
                }
            }
        }


    }

    public class Temp4AzureTableEntries : SqliteDBBase, IDisposable
    {
        private string tableName = "tempTable";
        private int offset = 0;
        public Temp4AzureTableEntries(string tenantGroupId, string jobid) : base(tenantGroupId, jobid)
        {

        }
        public override void CreateSchemaIfNotExists(IDbCommand command)
        {
            string query = string.Format($"CREATE TABLE IF NOT EXISTS {SecurityUtils.SanitizeSQLSchemaName(tableName)}([PartitionKey] [nvarchar](500) NOT NULL," +
                "[RowKey] [nvarchar](500) NOT NULL," +
                "[ArchiveLevel] [int]," +
                "[NodeID] [uniqueidentifier] not null," +
                "[ParentId] [uniqueidentifier] not null," +
                "[UIVersion] [int] not null," +
                "[CacheNodeType] [int]," +
                "[Status] [int]," +
                "[RuleID] [uniqueidentifier] not null," +
                "[DeleteRelatedRecords] [int]," +
                "[ScanJobID] [nvarchar](128)," +
                "[SortTicks] [nvarchar](128)," +
                "[SiteUrl] [nvarchar](2000)," +
                "[WebId] [uniqueidentifier] not null," +
                "[ListId] [uniqueidentifier] not null," +
                "[LeafName] [nvarchar](255)," +
                "[Path] [nvarchar](512)," +
                "[ScanTime] [datetime]," +
                "[LibRowID] [int]," +
                "[NodeType] [int]," +
                "[SPNodeLevel] [int]," +
                "[Level] [tinyint]," +
                "[LastModifiedTime] [bigint]);" +
                "CREATE INDEX tableIndex ON tempTable(SortTicks asc)");

            command.CommandText = query;

            command.ExecuteNonQuery();
        }
        //public void InsertValueToDB(IDbCommand command, List<ArchiveApproveReport> archiverEntities)
        //{
        //    string query = string.Format("INSERT INTO {0} VALUES {1}", tableName, SplicingInsertValueString(archiverEntities));
        //    command.CommandText = query;
        //    command.ExecuteNonQuery();
        //}
        public void InsertValueToDB(SQLiteConnection conn, IDbCommand command, List<ArchiveApproveReport> archiverEntities)
        {
            using (SQLiteTransaction tr = conn.BeginTransaction())
            {
                foreach (var archiverEn in archiverEntities)
                {
                    StringBuilder query = new StringBuilder();
                    query.Append($"INSERT INTO {SecurityUtils.SanitizeSQLSchemaName(tableName)}(PartitionKey,RowKey,ArchiveLevel,NodeID,ParentId,UIVersion,CacheNodeType,Status,RuleID,DeleteRelatedRecords,ScanJobID,SortTicks,SiteUrl,WebId,ListId,LeafName,Path,ScanTime,LibRowID,NodeType,SPNodeLevel,Level,LastModifiedTime) ");
                    //string query = string.Format("INSERT INTO {0} VALUES (@PartitionKey)", tableName, SplicingInsertValueString(archiverEntities));
                    query.Append(@"VALUES (@PartitionKey,@RowKey,@ArchiveLevel,@NodeID,@ParentId,@UIVersion,@CacheNodeType,@Status,@RuleID,
@DeleteRelatedRecords,@ScanJobID,@SortTicks,@SiteUrl,@WebId,@ListId,@LeafName,@Path,@ScanTime,@LibRowID,@NodeType,@SPNodeLevel,@Level,@LastModifiedTime)");
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
                    new SQLiteParameter("@LastModifiedTime")
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
        public List<ArchiveApproveReport> SelectValuesFromDB(IDbCommand command, int pageSize)
        {
            List<ArchiveApproveReport> archiverEntities = new List<ArchiveApproveReport>();
            string query = string.Format("SELECT [PartitionKey]," +
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
                $"[LastModifiedTime] FROM {SecurityUtils.SanitizeSQLSchemaName(tableName)} order by SortTicks limit {pageSize} offset {offset}");
            offset = offset + pageSize;
            command.CommandText = query;
            using (var sr = command.ExecuteReader())
            {
                while (sr.Read())
                {
                    ArchiveApproveReport archiverEn = new ArchiveApproveReport();
                    archiverEn.PartitionKey = sr.GetString(0);
                    archiverEn.EntityRowKey = sr.GetString(1);
                    archiverEn.ArchiveLevel = sr.GetInt32(2);
                    archiverEn.NodeId = sr.GetGuid(3).ToString();
                    archiverEn.ParentId = sr.GetGuid(4).ToString();
                    archiverEn.UIVersion = sr.GetInt32(5);
                    archiverEn.CacheNodeType = sr.GetInt32(6);
                    archiverEn.Status = (SOApproveDBStatus)sr.GetInt32(7);
                    archiverEn.RuleId = sr.GetGuid(8).ToString();
                    archiverEn.DeleteRelatedRecords = sr.GetInt32(9);
                    archiverEn.ScanJobID = sr.GetString(10);
                    archiverEn.SortTicks = sr.GetString(11);
                    archiverEn.SiteUrl = sr.GetString(12);
                    archiverEn.WebID = sr.GetGuid(13);
                    archiverEn.ListID = sr.GetGuid(14);
                    archiverEn.LeafName = sr.GetString(15);
                    archiverEn.FullPath = sr.GetString(16);
                    archiverEn.ScanTime = sr.GetInt64(17);
                    archiverEn.LibRowId = sr.GetInt32(18);
                    archiverEn.NodeType = sr.GetInt32(19);
                    archiverEn.SPNodeLevel = sr.GetInt32(20);
                    archiverEn.Level = Convert.ToByte(sr.GetInt32(21));
                    archiverEn.LastModifiedTime = sr.GetInt64(22);
                    archiverEntities.Add(archiverEn);
                }
            }
            return archiverEntities;
        }
        public void ExecuteQueryWithAction(Action<SQLiteConnection> action)
        {
            ExecuteWithConnection(action);
        }

        public void Dispose()
        {
            File.Delete(this.dbFilePath);
        }
    }

    public class DB4HSMStub : SqliteDBBase, IDisposable
    {
        private string mHSMStubTable = "HSMStubs";
        //private string mContainerMappingsTable = "ContainerMappings";
        private AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        int verifiedStatus = (int)StubExportStauts.Verified;
        int failedStatus = (int)StubExportStauts.Failed;
        public readonly static object mLock = new object();
        public DB4HSMStub(string tenantGroupId, string jobid) : base(tenantGroupId, jobid)
        {

        }
        public override void CreateSchemaIfNotExists(IDbCommand command)
        {
            string query = string.Format($"CREATE TABLE IF NOT EXISTS {SecurityUtils.SanitizeSQLSchemaName(mHSMStubTable)}(" +
                "[FileID] [nvarchar](50) not null," +
                "[FileRowID] [int] not null," +
                "[ContainerId] [nvarchar](500) NOT NULL," +
                "[FileNewID] [nvarchar](50) not null," +
                "[MD5] [nvarchar](500) NOT NULL," +
                "[ListId] [nvarchar](50) not null," +
                "[Url] [nvarchar](512)," +
                "[Size] [bigint]," +
                "[TotalSize] [bigint]," +
                "[RuleID] [nvarchar](50) not null," +
                "[Status] [int]," +
                "[AuthorID] [int]," +
                "[AuthorEmail] [nvarchar](500)," +
                "[ModifiedID] [int]," +
                "[ModifiedEmail] [nvarchar](500)," +
                "[CreateTime] [nvarchar](50)," +
                "[ModifiedTime] [nvarchar](50)," +
                "[VersionCount] [int]," +
                "[ModifiedTimeTicks] [bigint]," +
                "[TimeLastModifiedTicks] [bigint]," +
                "[IsManifestStub] [int] default 0," +
                "[StubId] [nvarchar](500)" +
                ");" +
                $"CREATE INDEX tableIndexContainerId ON {SecurityUtils.SanitizeSQLSchemaName(mHSMStubTable)}(RuleID,ListId,ContainerId);" +
                $"CREATE INDEX tableIndexFileID ON {SecurityUtils.SanitizeSQLSchemaName(mHSMStubTable)}(FileID);"
                );

            command.CommandText = query;

            command.ExecuteNonQuery();
        }

        public void InsertValueToDB(List<HSMFileMapping> stubEntities)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("DBForHSMStub.InsertValueToDB"))
            {
                ExecuteQueryWithAction(connection =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        InternalInsertValueToDB(connection, command, stubEntities);
                    }
                });
            }
        }

        private void InternalInsertValueToDB(SQLiteConnection conn, IDbCommand command, List<HSMFileMapping> stubEntities)
        {
            try
            {
                using (SQLiteTransaction tr = conn.BeginTransaction())
                {
                    StringBuilder query = new StringBuilder();
                    query.AppendLine($"INSERT INTO {SecurityUtils.SanitizeSQLSchemaName(mHSMStubTable)}(FileID,FileRowID,ContainerId,FileNewID,MD5,ListId,Url,Size,TotalSize,RuleID,Status,AuthorID,AuthorEmail,ModifiedID,ModifiedEmail,CreateTime,ModifiedTime,VersionCount,ModifiedTimeTicks,TimeLastModifiedTicks,IsManifestStub,StubId) ");
                    query.AppendLine(@"VALUES (@FileID,@FileRowID,@ContainerId,@FileNewID,@MD5,@ListId,@Url,@Size,@TotalSize,@RuleID,@Status,@AuthorID,@AuthorEmail,@ModifiedID,@ModifiedEmail,@CreateTime,@ModifiedTime,@VersionCount,@ModifiedTimeTicks,@TimeLastModifiedTicks,@IsManifestStub,@StubId)");
                    foreach (var stub in stubEntities)
                    {
                        command.Parameters.Clear();
                        command.Parameters.Add(new SQLiteParameter("@FileID", stub.ID.ToString()));
                        command.Parameters.Add(new SQLiteParameter("@FileRowID", stub.RowID));
                        command.Parameters.Add(new SQLiteParameter("@ContainerId", stub.ContainerId));
                        command.Parameters.Add(new SQLiteParameter("@FileNewID", stub.FileNewID.ToString()));
                        command.Parameters.Add(new SQLiteParameter("@MD5", stub.MD5));
                        command.Parameters.Add(new SQLiteParameter("@ListId", stub.ListID.ToString()));
                        command.Parameters.Add(new SQLiteParameter("@Url", stub.FileUrl));
                        command.Parameters.Add(new SQLiteParameter("@Size", stub.Size));
                        command.Parameters.Add(new SQLiteParameter("@TotalSize", stub.TotalSize));
                        command.Parameters.Add(new SQLiteParameter("@RuleID", stub.RuleID));
                        command.Parameters.Add(new SQLiteParameter("@Status", stub.Status));
                        command.Parameters.Add(new SQLiteParameter("@AuthorID", stub.AuthorID));
                        command.Parameters.Add(new SQLiteParameter("@AuthorEmail", stub.AuthorEmail));
                        command.Parameters.Add(new SQLiteParameter("@ModifiedID", stub.ModifiedID));
                        command.Parameters.Add(new SQLiteParameter("@ModifiedEmail", stub.ModifiedEmail));
                        command.Parameters.Add(new SQLiteParameter("@CreateTime", stub.CreateTime));
                        command.Parameters.Add(new SQLiteParameter("@ModifiedTime", stub.ModifiedTime));
                        command.Parameters.Add(new SQLiteParameter("@VersionCount", stub.VersionCount));
                        command.Parameters.Add(new SQLiteParameter("@ModifiedTimeTicks", stub.ModifiedTimeTicks));
                        command.Parameters.Add(new SQLiteParameter("@TimeLastModifiedTicks", stub.TimeLastModifiedTicks));
                        command.Parameters.Add(new SQLiteParameter("@IsManifestStub", stub.IsManifestStub ? 1 : 0));
                        command.Parameters.Add(new SQLiteParameter("@StubId", stub.StubId ?? ""));
                        command.CommandText = query.ToString();
                        command.ExecuteNonQuery();
                    }
                    tr.Commit();
                }
            }
            catch (Exception ex)
            {
                mLog.Error($"InternalInsertValueToDB failed.Message:{ex}.");
            }
        }
        /// <summary>
        /// 1.SQLite DB，多线程读没问题，多线程Update会有问题，此处控制为单线程.
        /// 2.后期如果需要可以改成SQLITE_THREADSAFE方式:https://sqlite.org/threadsafe.html
        /// </summary>
        public int UpdateRecordStatusToVerified(string fileid)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("DBForHSMStub.UpdateRecordStatusToVerified"))
            {
                lock (mLock)
                {
                    int affectedRowsCount = 0;
                    ExecuteQueryWithAction(connection =>
                    {
                        using (var command = connection.CreateCommand())
                        {
                            affectedRowsCount = InternalUpdateRecordStatusToVerified(connection, command, fileid);
                        }
                    });
                    return affectedRowsCount;
                }
            }
        }

        private int InternalUpdateRecordStatusToVerified(SQLiteConnection conn, IDbCommand command, string fileid)
        {
            int affectedRowsCount = 0;

            using (SQLiteTransaction tr = conn.BeginTransaction())
            {
                StringBuilder query = new StringBuilder();
                query.AppendLine($"UPDATE {SecurityUtils.SanitizeSQLSchemaName(mHSMStubTable)}");
                query.AppendLine(@"SET Status = @VerifiedStatus");
                query.AppendLine(@"WHERE FileID = @FileID AND Status != @FailedStatus");

                command.Parameters.Clear();
                command.Parameters.Add(new SQLiteParameter("@FileID", fileid));
                command.Parameters.Add(new SQLiteParameter("@VerifiedStatus", verifiedStatus));
                command.Parameters.Add(new SQLiteParameter("@FailedStatus", failedStatus));
                command.CommandText = query.ToString();
                affectedRowsCount = command.ExecuteNonQuery();
                tr.Commit();
            }
            return affectedRowsCount;
        }

        /// <summary>
        /// 1.SQLite DB，多线程读没问题，多线程Update会有问题，此处控制为单线程.
        /// 2.后期如果需要可以改成SQLITE_THREADSAFE方式:https://sqlite.org/threadsafe.html
        /// </summary>
        public int UpdateRecordStatus(string fileid, int status)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("DBForHSMStub.UpdateRecordStatus"))
            {
                lock (mLock)
                {
                    int affectedRowsCount = 0;
                    ExecuteQueryWithAction(connection =>
                    {
                        using (var command = connection.CreateCommand())
                        {
                            affectedRowsCount = InternalUpdateRecordStatus(connection, command, fileid, status);
                        }
                    });
                    return affectedRowsCount;
                }
            }
        }

        private int InternalUpdateRecordStatus(SQLiteConnection conn, IDbCommand command, string fileid, int status)
        {
            int affectedRowsCount = 0;
            using (SQLiteTransaction tr = conn.BeginTransaction())
            {
                StringBuilder query = new StringBuilder();
                query.AppendLine($"UPDATE {SecurityUtils.SanitizeSQLSchemaName(mHSMStubTable)}");
                query.AppendLine(@"SET Status = @Status");
                query.AppendLine(@"WHERE FileID = @FileID AND Status != 1");

                command.Parameters.Clear();
                command.Parameters.Add(new SQLiteParameter("@FileID", fileid));
                command.Parameters.Add(new SQLiteParameter("@Status", status));

                command.CommandText = query.ToString();
                affectedRowsCount = command.ExecuteNonQuery();

                tr.Commit();
            }
            return affectedRowsCount;
        }

        public List<string> GetContainerIds(string ruleId, string listId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("DBForHSMStub.GetContainerIds"))
            {
                List<string> containerIds = new List<string>();
                ExecuteQueryWithAction(connection =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        containerIds = InternalGetContainerIds(command, ruleId, listId);
                    }
                });
                return containerIds;
            }
        }

        private List<string> InternalGetContainerIds(IDbCommand command, string ruleId, string listId)
        {
            List<string> containerIds = new List<string>();
            string query = string.Format($"SELECT DISTINCT ContainerId FROM {SecurityUtils.SanitizeSQLSchemaName(mHSMStubTable)} WHERE RuleID = @RuleID AND ListId = @ListId");
            command.Parameters.Clear();
            command.Parameters.Add(new SQLiteParameter("@RuleID", ruleId));
            command.Parameters.Add(new SQLiteParameter("@ListId", listId));
            command.CommandText = query;
            using (var sr = command.ExecuteReader())
            {
                while (sr.Read())
                {
                    var containerId = sr.GetString(0);
                    containerIds.Add(containerId);
                }
            }
            return containerIds;
        }

        public List<HSMFileMapping> GetRecords(string ruleId, string listId, string containerId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("DBForHSMStub.GetRecords"))
            {
                lock (mLock)
                {
                    List<HSMFileMapping> mHSMFileMappings = new List<HSMFileMapping>();
                    ExecuteQueryWithAction(connection =>
                    {
                        using (var command = connection.CreateCommand())
                        {
                            mHSMFileMappings = InternalGetRecords(connection, command, ruleId, listId, containerId);
                        }
                    });
                    return mHSMFileMappings;
                }
            }
        }

        private List<HSMFileMapping> InternalGetRecords(SQLiteConnection conn, IDbCommand command, string ruleId,string listId,string containerId)
        {
            List<HSMFileMapping> stubs = new List<HSMFileMapping>();
            string query = string.Format($"SELECT FileID,FileRowID,FileNewID,MD5,Url,Size,TotalSize,Status,AuthorID,AuthorEmail,ModifiedID,ModifiedEmail,CreateTime,ModifiedTime,VersionCount,ModifiedTimeTicks,TimeLastModifiedTicks,IsManifestStub,StubId FROM {SecurityUtils.SanitizeSQLSchemaName(mHSMStubTable)} WHERE RuleID = @RuleID AND ListId = @ListId AND ContainerId = @ContainerId");
            command.Parameters.Clear();
            command.Parameters.Add(new SQLiteParameter("@RuleID", ruleId));
            command.Parameters.Add(new SQLiteParameter("@ListId", listId));
            command.Parameters.Add(new SQLiteParameter("@ContainerId", containerId));
            command.CommandText = query;
            using (var sr = command.ExecuteReader())
            {
                while (sr.Read())
                {
                    HSMFileMapping stub = new HSMFileMapping();
                    stub.ID = new Guid(sr.GetString(0));
                    stub.RowID = sr.GetInt32(1);
                    stub.FileNewID = new Guid(sr.GetString(2));
                    stub.MD5 = sr.GetString(3);
                    stub.FileUrl = sr.GetString(4);
                    stub.Size = sr.GetInt64(5);
                    stub.TotalSize = sr.GetInt64(6);
                    stub.Status = (StubExportStauts)sr.GetInt32(7);
                    stub.AuthorID = sr.GetInt32(8);
                    stub.AuthorEmail = sr.GetString(9);
                    stub.ModifiedID = sr.GetInt32(10);
                    stub.ModifiedEmail = sr.GetString(11);
                    stub.CreateTime = sr.GetString(12);
                    stub.ModifiedTime = sr.GetString(13);
                    stub.VersionCount = sr.GetInt32(14);
                    stub.ModifiedTimeTicks = sr.GetInt64(15);
                    stub.TimeLastModifiedTicks = sr.GetInt64(16);
                    stub.IsManifestStub = sr.GetInt32(17) == 1;
                    stub.StubId = sr.GetString(18);
                    stubs.Add(stub);
                }
            }
            return stubs;
        }

        public HSMFileMapping GetRecord(string fileId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("DBForHSMStub.GetRecords"))
            {
                lock (mLock)
                {
                    HSMFileMapping mHSMFileMappings = null;
                    ExecuteQueryWithAction(connection =>
                    {
                        using (var command = connection.CreateCommand())
                        {
                            mHSMFileMappings = InternalGetRecord(connection, command, fileId);
                        }
                    });
                    return mHSMFileMappings;
                }
            }
        }

        private HSMFileMapping InternalGetRecord(SQLiteConnection conn, IDbCommand command, string fileId)
        {
            HSMFileMapping stub = null;
            string query = string.Format($"SELECT FileID,FileRowID,FileNewID,MD5,Url,Size,TotalSize,Status,AuthorID,AuthorEmail,ModifiedID,ModifiedEmail,CreateTime,ModifiedTime,VersionCount,ModifiedTimeTicks,TimeLastModifiedTicks,IsManifestStub,StubId FROM {SecurityUtils.SanitizeSQLSchemaName(mHSMStubTable)} WHERE FileID = @FileID");
            command.Parameters.Clear();
            command.Parameters.Add(new SQLiteParameter("@FileID", fileId));
            command.CommandText = query;
            using (var sr = command.ExecuteReader())
            {
                if (sr.Read())
                {
                    stub = new HSMFileMapping();
                    stub.ID = new Guid(sr.GetString(0));
                    stub.RowID = sr.GetInt32(1);
                    stub.FileNewID = new Guid(sr.GetString(2));
                    stub.MD5 = sr.GetString(3);
                    stub.FileUrl = sr.GetString(4);
                    stub.Size = sr.GetInt64(5);
                    stub.TotalSize = sr.GetInt64(6);
                    stub.Status = (StubExportStauts)sr.GetInt32(7);
                    stub.AuthorID = sr.GetInt32(8);
                    stub.AuthorEmail = sr.GetString(9);
                    stub.ModifiedID = sr.GetInt32(10);
                    stub.ModifiedEmail = sr.GetString(11);
                    stub.CreateTime = sr.GetString(12);
                    stub.ModifiedTime = sr.GetString(13);
                    stub.VersionCount = sr.GetInt32(14);
                    stub.ModifiedTimeTicks = sr.GetInt64(15);
                    stub.TimeLastModifiedTicks = sr.GetInt64(16);
                    stub.IsManifestStub = sr.GetInt32(17) == 1;
                    stub.StubId = sr.GetString(18);
                }
            }
            return stub;
        }

        public void ExecuteQueryWithAction(Action<SQLiteConnection> action)
        {
            ExecuteWithConnection(action);
        }

        public void Dispose()
        {
            File.Delete(this.dbFilePath);
        }
    }
}
