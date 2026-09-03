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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.RACommonUtility.Encryption;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan
{
    internal class CGDBReader : IScanDBReader
    {
        private static readonly AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public static CGDBReader instance;
        private string _mCGDBConnectionString;
        private string _mSiteSummaryDBConnectionString;
        private string _mSiteSummaryTableName;
        private string mCurrentSCCGDBTableName;
        private bool _mSiteSummaryTableCanConnect;
        private static int CommandTimeOut = 1800;
        private const int SQLTIMEOUT = 180;
        private static readonly object padlock = new object();
        private string mSiteId = string.Empty;
        private string mSiteUrl = string.Empty;
        private string mDecryptDBPassword = string.Empty;
        private List<FailedUpdateCGObject> failedUpdateCGObjects = null;
        private RMAesEncryptorWrapper AesEncryptorWrapper => new();

        public string MCurrentSCCGDBTableName
        {
            get { return mCurrentSCCGDBTableName; }
        }
        public string MCurrentSiteSummaryTableName
        {
            get { return _mSiteSummaryTableName; }
        }
        public bool MCurrentSiteSummaryTableCanConnect
        {
            get { return _mSiteSummaryTableCanConnect; }
        }

        private CGDBReader(ArchiverExtendSettingDto archiverExtendSetting, string siteId, string siteUrl)
        {
            failedUpdateCGObjects = new List<FailedUpdateCGObject>();
            mSiteId = siteId;
            mSiteUrl = siteUrl;
            Init(archiverExtendSetting, siteUrl);
        }

        public static CGDBReader GetInstance(ArchiverExtendSettingDto archiverExtendSetting, string siteId, string siteUrl)
        {
            if (instance == null)
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new CGDBReader(archiverExtendSetting, siteId, siteUrl);
                    }
                }
            }
            return instance;
        }

        /// <summary>
        /// 拼接获取CG DB Connection String
        /// </summary>
        private void InitSiteSummaryDBConnectionString(ArchiverExtendSettingDto archiverExtendSetting)
        {
            if (GCommon.Utility.SecurityUtils.ValidateSQLConnectionStringWithBuilder(archiverExtendSetting.CGDatabaseConnection,out var sqlConnBuilder))
            {
                sqlConnBuilder.ConnectTimeout = SQLTIMEOUT;
                //var sqlConnBuilder = new SqlConnectionStringBuilder(archiverExtendSetting.CGDatabaseConnection) { ConnectTimeout = SQLTIMEOUT };
                try
                {
                    sqlConnBuilder.Password = AesEncryptorWrapper.Decrypt(sqlConnBuilder.Password);
                    mDecryptDBPassword = sqlConnBuilder.Password;
                    sqlConnBuilder.InitialCatalog = archiverExtendSetting.SiteSummaryDBName;
                }
                catch (Exception e)
                {
                    mLog.Error("Decrypt SiteSummary DB Connection String error : {0}", e.ToString());
                }
                _mSiteSummaryDBConnectionString = sqlConnBuilder.ConnectionString;
                _mSiteSummaryTableName = archiverExtendSetting.SiteSummaryTableName;
                mLog.Info($"Success init SiteSummaryTable,DBName:{archiverExtendSetting.SiteSummaryDBName}.tableName:{_mSiteSummaryTableName}.");
            }
        }
        public void UpdateStatus(string siteId, Guid itemId, BackupRestoreStatus Status)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan.UpdateStatus"))
            {
                int status = 0;
                try
                {
                    if (mCurrentSCCGDBTableName != null)
                    {
                        Stopwatch sw = Stopwatch.StartNew();
                        using (var sqlConn = new SqlConnection(_mCGDBConnectionString))
                        {
                            using (var sqlComm = sqlConn.CreateCommand())
                            {
                                sqlComm.CommandTimeout = CommandTimeOut;
                                sqlComm.Parameters.AddWithValue("@siteId", siteId);
                                sqlComm.Parameters.AddWithValue("@itemId", itemId.ToString());
                                //0 not processed,1 success,2 faild,3 skip
                                switch (Status)
                                {
                                    case BackupRestoreStatus.Succeed:
                                        status = 1;
                                        break;
                                    case BackupRestoreStatus.Failed:
                                        status = 2;
                                        break;
                                    case BackupRestoreStatus.Skipped:
                                        status = 3;
                                        break;
                                    case BackupRestoreStatus.UnProcess:
                                        status = 99;
                                        break;
                                    case BackupRestoreStatus.UnKnown:
                                    default:
                                        status = 0;
                                        break;
                                }
                                sqlComm.Parameters.AddWithValue("@status", status);
                                SecurityUtils.SanitizeSQLSchemaName(mCurrentSCCGDBTableName);
                                sqlComm.CommandText = string.Format("UPDATE [{0}] SET Status=@status WHERE ItemID=@itemId AND SiteID=@siteId AND (Status = 0 OR Status = 2)", mCurrentSCCGDBTableName);
                                sqlConn.Open();
                                sqlComm.ExecuteNonQuery();
                            }
                        }
                        sw.Stop();
                        mLog.Info($"CBDBReader success UpdateStatus:siteId:{siteId},itemId:{itemId}.Time:{sw.Elapsed}.");
                    }
                }
                catch (Exception ex)
                {
                    mLog.Warn($"CBDBReader failed UpdateStatus:siteId:{siteId},itemId:{itemId}.status:{status}.Message:{ex}.");
                    failedUpdateCGObjects.Add(new FailedUpdateCGObject() { SiteId = siteId, ItemId = itemId, Status = status, ArchiveSize = 0 });
                }
            }
        }
        /// <summary>
        /// 1.从SiteSummary表读取当前SC对应的DB和Table
        /// 2.实例化CGDBConnectionString
        /// </summary>
        private void GetCurrentSCSummaryInfoBySiteUrl(ArchiverExtendSettingDto archiverExtendSetting, string siteUrl)
        {
            if (CheckTableExist(_mSiteSummaryDBConnectionString, _mSiteSummaryTableName))
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan.GetCurrentSCSummaryInfoBySiteUrl"))
                {
                    using (var sqlConn = new SqlConnection(_mSiteSummaryDBConnectionString))
                    {
                        using (var sqlComm = sqlConn.CreateCommand())
                        {
                            sqlComm.CommandTimeout = CommandTimeOut;
                            sqlComm.Parameters.AddWithValue("@siteUrl", siteUrl);
                            sqlComm.CommandText = $"SELECT [DB Name],[Table Name] From [{SecurityUtils.SanitizeSQLParameterName(_mSiteSummaryTableName)}] Where [Site Collection URL] = @siteUrl";
                            sqlConn.Open();
                            using (var sr = sqlComm.ExecuteReader())
                            {
                                int i = 0;
                                while (sr.Read())
                                {
                                    i++;
                                    if (GCommon.Utility.SecurityUtils.ValidateSQLConnectionStringWithBuilder(archiverExtendSetting.CGDatabaseConnection,out var sqlConnBuilder))
                                    {
                                        sqlConnBuilder.ConnectTimeout = SQLTIMEOUT;
                                        sqlConnBuilder.Password = mDecryptDBPassword;
                                        sqlConnBuilder.InitialCatalog = sr.GetValue(0).ToString();
                                        _mCGDBConnectionString = sqlConnBuilder.ConnectionString;
                                        mCurrentSCCGDBTableName = SecurityUtils.SanitizeSQLParameterName(sr.GetValue(1).ToString());
                                        mLog.Info($"Success init CGDB & Table,DBName:{sr.GetValue(0).ToString()}.tableName:{sr.GetValue(1).ToString()}.");
                                    }
                                    if (i > 0)
                                    {
                                        break;
                                    }
                                }
                                CheckTableExist(_mCGDBConnectionString, mCurrentSCCGDBTableName);
                            }
                        }
                    }
                }
            }
            else
            {
                mLog.Error($"Can't find SiteSummaryTable,tableName:{_mSiteSummaryTableName}.");
            }
        }

        /// <summary>
        /// 判断Table是否在当前Database中存在
        /// </summary>
        private bool CheckTableExist(string dbConnectionString, string tableName)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan.CheckTableExist"))
            {
                try
                {
                    using (var sqlConn = new SqlConnection(dbConnectionString))
                    {
                        using (var sqlComm = sqlConn.CreateCommand())
                        {
                            sqlComm.CommandTimeout = CommandTimeOut;
                            sqlComm.Parameters.AddWithValue("@tableName", SecurityUtils.SanitizeSQLSchemaName(tableName));
                            sqlComm.CommandText = string.Format("IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME = @tableName) BEGIN select 1 end ELSE BEGIN select 0 end");
                            sqlConn.Open();
                            using (var sr = sqlComm.ExecuteReader())
                            {
                                while (sr.Read())
                                {
                                    if (sr.GetValue(0).ToString() == "1")
                                    {
                                        _mSiteSummaryTableCanConnect = true;
                                        return true;
                                    }
                                    else
                                    {
                                        _mSiteSummaryTableCanConnect = false;
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) 
                {
                    _mSiteSummaryTableCanConnect = false;
                    mLog.Error($"Can't connect SiteSummaryTable,tableName:{_mSiteSummaryTableName}.Message:{ex.ToString()}.");
                }
                return false;
            }
        }

        public void Init(ArchiverExtendSettingDto archiverExtendSetting, string siteUrl)
        {
            InitSiteSummaryDBConnectionString(archiverExtendSetting);
            GetCurrentSCSummaryInfoBySiteUrl(archiverExtendSetting, siteUrl);
        }
        public void UpdateStatusAndArchiveSize(string siteId, Guid itemId, BackupRestoreStatus Status, long archiveSize, DateTime executiondate)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan.UpdateStatusAndArchiveSize"))
            {
                int status = 0;
                try
                {
                    if (mCurrentSCCGDBTableName != null)
                    {
                        Stopwatch sw = Stopwatch.StartNew();
                        using (var sqlConn = new SqlConnection(_mCGDBConnectionString))
                        {
                            using (var sqlComm = sqlConn.CreateCommand())
                            {
                                sqlComm.CommandTimeout = CommandTimeOut;
                                sqlComm.Parameters.AddWithValue("@siteId", siteId);
                                sqlComm.Parameters.AddWithValue("@itemId", itemId.ToString());
                                //0 not processed,1 success,2 faild,3 skip
                                switch (Status)
                                {
                                    case BackupRestoreStatus.Succeed:
                                        status = 1;
                                        break;
                                    case BackupRestoreStatus.Failed:
                                        archiveSize = 0;
                                        status = 2;
                                        break;
                                    case BackupRestoreStatus.Skipped:
                                        archiveSize = 0;
                                        status = 3;
                                        break;
                                    case BackupRestoreStatus.UnKnown:
                                    default:
                                        status = 0;
                                        break;
                                }
                                //throw new Exception("UpdateStatusAndArchiveSize Exception");
                                sqlComm.Parameters.AddWithValue("@status", status);
                                sqlComm.Parameters.AddWithValue("@ArchiveSize", archiveSize);
                                if(status == 1)
                                {
                                    sqlComm.Parameters.AddWithValue("@executiondate", executiondate);
                                    sqlComm.CommandText = string.Format("UPDATE [{0}] SET Status=@status,ArchiveSize=@ArchiveSize,executiondate=@executiondate WHERE ItemID=@itemId AND SiteID=@siteId", SecurityUtils.SanitizeSQLSchemaName(mCurrentSCCGDBTableName));
                                }
                                else
                                {
                                    sqlComm.CommandText = string.Format("UPDATE [{0}] SET Status=@status,ArchiveSize=@ArchiveSize WHERE ItemID=@itemId AND SiteID=@siteId", SecurityUtils.SanitizeSQLSchemaName(mCurrentSCCGDBTableName));
                                }
                                sqlConn.Open();
                                sqlComm.ExecuteNonQuery();
                            }
                        }
                        sw.Stop();
                        mLog.Info($"CBDBReader success UpdateStatusAndArchiveSize:siteId:{siteId},itemId:{itemId}.Time:{sw.Elapsed}.");
                    }
                }
                catch (Exception ex)
                {
                    mLog.Warn($"CBDBReader failed UpdateStatusAndArchiveSize:siteId:{siteId},itemId:{itemId}.status:{status}.archiveSize:{archiveSize}.Message:{ex}.");
                    failedUpdateCGObjects.Add(new FailedUpdateCGObject() { SiteId = siteId, ItemId = itemId, Status = status, ArchiveSize = archiveSize });
                }
            }
        }

        public void FinalProcessFailedUpdateObject()
        {
            if (failedUpdateCGObjects != null && failedUpdateCGObjects.Count > 0)
            {
                foreach (var failedUpdateCGObject in failedUpdateCGObjects)
                {
                    try
                    {
                        Stopwatch sw = Stopwatch.StartNew();
                        using (var sqlConn = new SqlConnection(_mCGDBConnectionString))
                        {
                            using (var sqlComm = sqlConn.CreateCommand())
                            {
                                sqlComm.CommandTimeout = CommandTimeOut;
                                sqlComm.Parameters.AddWithValue("@siteId", failedUpdateCGObject.SiteId);
                                sqlComm.Parameters.AddWithValue("@itemId", failedUpdateCGObject.ItemId.ToString());
                                sqlComm.Parameters.AddWithValue("@status", failedUpdateCGObject.Status);
                                sqlComm.Parameters.AddWithValue("@ArchiveSize", failedUpdateCGObject.ArchiveSize);
                                sqlComm.CommandText = string.Format("UPDATE [{0}] SET Status=@status,ArchiveSize=@ArchiveSize WHERE ItemID=@itemId AND SiteID=@siteId AND (Status = 0 OR Status = 2)", SecurityUtils.SanitizeSQLSchemaName(mCurrentSCCGDBTableName));
                                sqlConn.Open();
                                sqlComm.ExecuteNonQuery();
                            }
                        }
                        sw.Stop();
                        mLog.Info($"CBDBReader success FinalProcessFailedUpdateObject:siteId:{failedUpdateCGObject.SiteId},itemId:{failedUpdateCGObject.ItemId}.status:{failedUpdateCGObject.Status}.archiveSize:{failedUpdateCGObject.ArchiveSize}.Time:{sw.Elapsed}.");
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn($"CBDBReader failed FinalProcessFailedUpdateObject:siteId:{failedUpdateCGObject.SiteId},itemId:{failedUpdateCGObject.ItemId}.status:{failedUpdateCGObject.Status}.archiveSize:{failedUpdateCGObject.ArchiveSize}.Message:{ex}.");
                    }
                }
                failedUpdateCGObjects.Clear();
            }
            else
            {
                mLog.Info($"FinalProcessFailedUpdateObject no failed CGObjects.");
            }
        }

        // "IF EXISTS(SELECT* FROM [CGTest].[dbo].[CGTest1] where[SiteID] = 'B301F2D8-B9FB-46D3-BF8E-6F4473ADE69B') BEGIN select 1 end ELSE BEGIN select 0 end"
        public List<DBFileInfo> GetFilesInfo(Guid siteid, Guid webid, Guid listid, int ruleOrder)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan.GetFilesInfo"))
            {
                List<DBFileInfo> files = new List<DBFileInfo>();
                using (var sqlConn = new SqlConnection(_mCGDBConnectionString))
                {
                    using (var sqlComm = sqlConn.CreateCommand())
                    {
                        sqlComm.CommandTimeout = CommandTimeOut;
                        sqlComm.Parameters.AddWithValue("@siteid", siteid);
                        sqlComm.Parameters.AddWithValue("@webid", webid);
                        sqlComm.Parameters.AddWithValue("@listid", listid);
                        sqlComm.Parameters.AddWithValue("@isArchived", ruleOrder);
                        sqlComm.Parameters.AddWithValue("@InitialStatus", 0);
                        sqlComm.Parameters.AddWithValue("@FailedStatus", 2);
                        sqlComm.CommandText = $"SELECT [SiteID],[WebID],[ListID],[ItemID],[FullPath],[ListItemId],[FileSize],[StorageSize] From [{SecurityUtils.SanitizeSQLParameterName(mCurrentSCCGDBTableName)}] with (NOLOCK) Where SiteID = @siteid AND WebID=@webid AND ListID=@listid AND IsArchive=@isArchived AND (Status=@InitialStatus OR Status=@FailedStatus)";
                        sqlConn.Open();
                        using (var sr = sqlComm.ExecuteReader())
                        {
                            while (sr.Read())
                            {
                                DBFileInfo fileInfo = new DBFileInfo();
                                fileInfo.itemId = sr.GetGuid(3);
                                fileInfo.url = sr.GetString(4);
                                fileInfo.fullPath = sr.GetString(4);
                                fileInfo.ID = sr.GetInt32(5);
                                fileInfo.webId = sr.GetGuid(1);
                                fileInfo.listId = sr.GetGuid(2);
                                try
                                {
                                    fileInfo.Size = sr.GetInt64(6);
                                }
                                catch (Exception ex)
                                {
                                    fileInfo.Size = 0;
                                    mLog.Warn($"Size is null, assign a value of 0,error message:{ex.ToString()}.");
                                }
                                try
                                {
                                    fileInfo.StorageSize = sr.GetInt64(7);
                                }
                                catch (Exception ex)
                                {
                                    fileInfo.StorageSize = 0;
                                    mLog.Warn($"StorageSize is null, assign a value of 0,error message:{ex.ToString()}.");
                                }
                                files.Add(fileInfo);
                            }
                        }
                    }
                }
                mLog.Info($"CBDBReader GetFilesInfo success:siteId:{siteid}.webid:{webid}.listid:{listid}.ruleOrder:{ruleOrder}.FilesInfoCount:{files.Count}.");
                return files;
            }
        }

        public List<DBFileInfo> GetUnProcessedFileInfo(Guid siteid, int ruleOrder)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan.GetUnProcessedFileInfo"))
            {
                List<DBFileInfo> files = new List<DBFileInfo>();
                using (var sqlConn = new SqlConnection(_mCGDBConnectionString))
                {
                    using (var sqlComm = sqlConn.CreateCommand())
                    {
                        sqlComm.CommandTimeout = CommandTimeOut;
                        sqlComm.Parameters.AddWithValue("@siteid", siteid);
                        sqlComm.Parameters.AddWithValue("@isArchived", ruleOrder);
                        sqlComm.Parameters.AddWithValue("@InitialStatus", 0);
                        sqlComm.CommandText = string.Format("SELECT [SiteID],[WebID],[ListID],[ItemID],[FullPath],[ListItemId],[FileSize],[StorageSize] From [{0}] with (NOLOCK) Where SiteID = @siteid AND IsArchive=@isArchived AND Status=@InitialStatus", SecurityUtils.SanitizeSQLSchemaName(mCurrentSCCGDBTableName));
                        sqlConn.Open();
                        using (var sr = sqlComm.ExecuteReader())
                        {
                            while (sr.Read())
                            {
                                DBFileInfo fileInfo = new DBFileInfo();
                                fileInfo.itemId = sr.GetGuid(3);
                                fileInfo.url = sr.GetString(4);
                                fileInfo.fullPath = sr.GetString(4);
                                fileInfo.ID = sr.GetInt32(5);
                                fileInfo.webId = sr.GetGuid(1);
                                fileInfo.listId = sr.GetGuid(2);
                                try
                                {
                                    fileInfo.Size = sr.GetInt64(6);
                                }
                                catch (Exception ex)
                                {
                                    fileInfo.Size = 0;
                                    mLog.Warn($"Size is null, assign a value of 0,error message:{ex.ToString()}.");
                                }
                                try
                                {
                                    fileInfo.StorageSize = sr.GetInt64(7);
                                }
                                catch (Exception ex)
                                {
                                    fileInfo.StorageSize = 0;
                                    mLog.Warn($"StorageSize is null, assign a value of 0,error message:{ex.ToString()}.");
                                }
                                files.Add(fileInfo);
                            }
                        }
                    }
                }
                mLog.Info($"CBDBReader GetUnProcessedFileInfo success:siteId:{siteid}.ruleOrder:{ruleOrder}.UnProcessedFileInfoCount:{files.Count}.");
                return files;
            }
        }

        public List<Guid> GetListIds(Guid siteid, Guid webid, int ruleOrder)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan.GetListIds"))
            {
                List<Guid> lists = new List<Guid>();
                using (var sqlConn = new SqlConnection(_mCGDBConnectionString))
                {
                    using (var sqlComm = sqlConn.CreateCommand())
                    {
                        sqlComm.CommandTimeout = CommandTimeOut;
                        sqlComm.Parameters.AddWithValue("@siteid", siteid);
                        sqlComm.Parameters.AddWithValue("@webid", webid);
                        sqlComm.Parameters.AddWithValue("@isArchived", ruleOrder);
                        sqlComm.Parameters.AddWithValue("@InitialStatus", 0);
                        sqlComm.Parameters.AddWithValue("@FailedStatus", 2);
                        sqlComm.CommandText = $"SELECT DISTINCT ListID From [{SecurityUtils.SanitizeSQLParameterName(mCurrentSCCGDBTableName)}] with (NOLOCK) Where SiteID = @siteid AND WebID = @webid AND IsArchive=@isArchived AND (Status=@InitialStatus OR Status=@FailedStatus)";
                        sqlConn.Open();
                        using (var sr = sqlComm.ExecuteReader())
                        {
                            while (sr.Read())
                            {
                                lists.Add(sr.GetGuid(0));
                            }
                        }
                    }
                }
                mLog.Info($"CBDBReader GetListIds success:siteId:{siteid}.webid:{webid}.ruleOrder:{ruleOrder}.ListIdsCount:{lists.Count}.");
                return lists;
            }
        }

        public List<Guid> GetWebIds(Guid siteid, int ruleOrder)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan.GetWebIds"))
            {
                List<Guid> lists = new List<Guid>();
                using (var sqlConn = new SqlConnection(_mCGDBConnectionString))
                {
                    using (var sqlComm = sqlConn.CreateCommand())
                    {
                        sqlComm.CommandTimeout = CommandTimeOut;
                        sqlComm.Parameters.AddWithValue("@siteid", siteid);
                        sqlComm.Parameters.AddWithValue("@isArchived", ruleOrder);
                        sqlComm.Parameters.AddWithValue("@InitialStatus", 0);
                        sqlComm.Parameters.AddWithValue("@FailedStatus", 2);
                        sqlComm.CommandText = $@"SELECT DISTINCT WebID From [{SecurityUtils.SanitizeSQLSchemaName(mCurrentSCCGDBTableName)}] with (NOLOCK) Where SiteID = @siteid AND IsArchive=@isArchived AND (Status=@InitialStatus OR Status=@FailedStatus)";
                        sqlConn.Open();
                        using (var sr = sqlComm.ExecuteReader())
                        {
                            while (sr.Read())
                            {
                                lists.Add(sr.GetGuid(0));
                            }
                        }
                    }
                }
                mLog.Info($"CBDBReader GetWebIds success:siteId:{siteid}.ruleOrder:{ruleOrder}.WebIdsCount:{lists.Count}.");
                return lists;
            }
        }

        public void UpdateCGDBUnCorrectData(long cgDBId, int listItemId, int condition, string fileFullPath, string fileName, string listId)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan.UpdateCGDBUnCorrectData"))
            {
                try
                {
                    if (mCurrentSCCGDBTableName != null)
                    {
                        Stopwatch sw = Stopwatch.StartNew();
                        using (var sqlConn = new SqlConnection(_mCGDBConnectionString))
                        {
                            using (var sqlComm = sqlConn.CreateCommand())
                            {
                                sqlComm.CommandTimeout = CommandTimeOut;
                                switch (condition)
                                {
                                    //文件路径有问题，但是文件存在
                                    case 0:
                                        sqlComm.Parameters.AddWithValue("@fileFullPath", fileFullPath);
                                        sqlComm.Parameters.AddWithValue("@listItemId", listItemId);
                                        sqlComm.Parameters.AddWithValue("@listId", listId);
                                        sqlComm.Parameters.AddWithValue("@id", cgDBId);
                                        sqlComm.CommandText = string.Format(@"UPDATE  [{0}]  SET FullPath = @fileFullPath,ListItemId = @listItemId,ListId = @listId WHERE Id =  @id", SecurityUtils.SanitizeSQLSchemaName(mCurrentSCCGDBTableName));

                                        break;
                                    //文件不存在了
                                    case 1:
                                        sqlComm.Parameters.AddWithValue("@id", cgDBId);
                                        sqlComm.CommandText = string.Format(@"UPDATE  [{0}]  SET Status = 99 WHERE Id =  @id", SecurityUtils.SanitizeSQLSchemaName(mCurrentSCCGDBTableName));
                                        break;
                                    //文件最后修改时间scan之后有改动，改动时间未超过3个月
                                    case 2:
                                        sqlComm.Parameters.AddWithValue("@id", cgDBId);
                                        sqlComm.CommandText = string.Format(@"UPDATE  [{0}]  SET Status = 90 WHERE Id =  @id", SecurityUtils.SanitizeSQLSchemaName(mCurrentSCCGDBTableName));
                                        break;
                                    //文件名字scan之后有改动，改动时间超过3个月
                                    case 3:
                                        sqlComm.Parameters.AddWithValue("@fileFullPath", fileFullPath);
                                        sqlComm.Parameters.AddWithValue("@listItemId", listItemId);
                                        sqlComm.Parameters.AddWithValue("@listId", listId);
                                        sqlComm.Parameters.AddWithValue("@id", cgDBId);
                                        sqlComm.Parameters.AddWithValue("@ItemName", fileName);
                                        sqlComm.CommandText = string.Format(@"UPDATE  [{0}]  SET FullPath = @fileFullPath,ItemName = @ItemName,ListItemId = @listItemId,ListId = @listId WHERE Id =  @id", SecurityUtils.SanitizeSQLSchemaName(mCurrentSCCGDBTableName));
                                        break;
                                    //文件名字scan之后有改动，改动时间未超过3个月
                                    case 4:
                                        sqlComm.Parameters.AddWithValue("@fileFullPath", fileFullPath);
                                        sqlComm.Parameters.AddWithValue("@listItemId", listItemId);
                                        sqlComm.Parameters.AddWithValue("@listId", listId);
                                        sqlComm.Parameters.AddWithValue("@id", cgDBId);
                                        sqlComm.Parameters.AddWithValue("@ItemName", fileName);
                                        sqlComm.CommandText = string.Format(@"UPDATE  [{0}]  SET FullPath = @fileFullPath,ItemName = @ItemName,Status = 90,ListItemId = @listItemId,ListId = @listId WHERE Id =  @id", SecurityUtils.SanitizeSQLSchemaName(mCurrentSCCGDBTableName));
                                        break;
                                    //ListItemId为零
                                    case 5:
                                        sqlComm.Parameters.AddWithValue("@listItemId", listItemId);
                                        sqlComm.Parameters.AddWithValue("@id", cgDBId);
                                        sqlComm.CommandText = string.Format(@"UPDATE  [{0}]  SET ListItemId = @listItemId WHERE Id =  @id", SecurityUtils.SanitizeSQLSchemaName(mCurrentSCCGDBTableName));
                                        break;
                                    default:
                                        break;
                                }
                                sqlConn.Open();
                                sqlComm.ExecuteNonQuery();
                            }
                        }
                        sw.Stop();
                        mLog.Info($"CBDBReader success UpdateCGDBUnCorrectData.listItemId:{listItemId}.Time:{sw.Elapsed}.");
                    }
                }
                catch (Exception ex)
                {
                    mLog.Warn($"CBDBReader failed UpdateCGDBUnCorrectData:listItemId:{listItemId}.Message:{ex}.");
                }
            }
        }

        public List<DBFileInfo> GetFilesInfoForFixCGFullPath(Guid siteid)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan.GetFilesInfoForFixCGFullPath"))
            {
                List<DBFileInfo> files = new List<DBFileInfo>();
                using (var sqlConn = new SqlConnection(_mCGDBConnectionString))
                {
                    using (var sqlComm = sqlConn.CreateCommand())
                    {
                        sqlComm.CommandTimeout = CommandTimeOut;
                        sqlComm.Parameters.AddWithValue("@siteid", siteid);
                        sqlComm.CommandText = string.Format("SELECT [WebId],[ListId],[ItemID],[FullPath],[ItemName],[ListItemId],[ID],[FileType] From [{0}] with (NOLOCK) Where SiteID = @siteid AND (Status = 0 or Status = 2) and IsArchive != 0 and IsArchive != 90 and IsArchive != 99", SecurityUtils.SanitizeSQLSchemaName(mCurrentSCCGDBTableName));
                        sqlConn.Open();
                        using (var sr = sqlComm.ExecuteReader())
                        {
                            while (sr.Read())
                            {
                                try
                                {
                                    DBFileInfo fileInfo = new DBFileInfo();
                                    fileInfo.webId = sr.GetGuid(0);
                                    fileInfo.listId = sr.GetGuid(1);
                                    fileInfo.itemId = sr.GetGuid(2);
                                    fileInfo.fullPath = sr.GetString(3);
                                    fileInfo.url = sr.GetString(3);
                                    fileInfo.fileName = sr.GetString(4);
                                    fileInfo.ID = sr.GetInt32(5);
                                    fileInfo.CGDBID = sr.GetInt64(6);
                                    fileInfo.fileType = sr.GetString(7);
                                    files.Add(fileInfo);
                                }
                                catch (Exception ex)
                                {
                                    mLog.Warn($"CBDBReader GetFilesInfoForFixCGFullPath failed.message:{ex}.");
                                }
                            }
                        }
                    }
                }
                mLog.Info($"CBDBReader GetFilesInfoForFixCGFullPath success:siteId:{siteid}.FilesInfoCount:{files.Count}.");
                return files;
            }
        }


        //public int GetListItemsCount()
        //{
        //    int count = 0;
        //    using (var sqlConn = new SqlConnection(_mCGDBConnectionString))
        //    {
        //        using (var sqlComm = sqlConn.CreateCommand())
        //        {
        //            sqlComm.CommandTimeout = CommandTimeOut;
        //            sqlComm.Parameters.AddWithValue("@siteid", mSiteId);
        //            sqlComm.Parameters.AddWithValue("@isArchived", 0);
        //            sqlComm.Parameters.AddWithValue("@InitialStatus", 0);
        //            sqlComm.Parameters.AddWithValue("@FailedStatus", 2);
        //            sqlComm.CommandText = string.Format("SELECT count(*) From [{0}] with (NOLOCK) Where SiteID = @siteid AND IsArchive!=@isArchived AND (Status=@InitialStatus OR Status=@FailedStatus)", mCurrentSCCGDBTableName);
        //            sqlConn.Open();
        //            var sr = sqlComm.ExecuteScalar();
        //            count = (int)sr;
        //        }
        //    }
        //    mLog.Info($"CBDBReader GetListItemsCount success:mSiteId:{mSiteId}.ListItemsCount:{count}.");
        //    return count;
        //}

        public void Dispose()
        {
        }

        //#region Old logic method code
        //private bool GetTableName(string tempTableName, string siteId)
        //{
        //    using (AvePerformanceScope pc = new AvePerformanceScope("AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan.GetTableName"))
        //    {
        //        if (CheckExistsSiteIdColume(tempTableName))
        //        {
        //            using (var sqlConn = new SqlConnection(_mCGDBConnectionString))
        //            {
        //                using (var sqlComm = sqlConn.CreateCommand())
        //                {
        //                    sqlComm.CommandTimeout = CommandTimeOut;
        //                    sqlComm.Parameters.AddWithValue("@siteId", siteId);
        //                    sqlComm.CommandText = string.Format("IF EXISTS(SELECT SiteID FROM [{0}] where[SiteID] = @siteId) BEGIN select 1 end ELSE BEGIN select 0 end", tempTableName);
        //                    sqlConn.Open();
        //                    using (var sr = sqlComm.ExecuteReader())
        //                    {
        //                        while (sr.Read())
        //                        {
        //                            if (sr.GetValue(0).ToString() == "1")
        //                            {
        //                                return true;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            return false;
        //        }
        //        return false;
        //    }
        //}
        //private bool CheckExistsSiteIdColume(string tempTableName)
        //{
        //    using (var sqlConn = new SqlConnection(_mCGDBConnectionString))
        //    {
        //        using (var sqlComm = sqlConn.CreateCommand())
        //        {
        //            sqlComm.CommandTimeout = CommandTimeOut;
        //            sqlComm.CommandText = string.Format("IF COL_LENGTH('{0}','SiteID') IS NOT NULL BEGIN select 1 end ELSE BEGIN select 0 end", tempTableName);
        //            sqlConn.Open();
        //            using (var sr = sqlComm.ExecuteReader())
        //            {
        //                while (sr.Read())
        //                {
        //                    if (sr.GetValue(0).ToString() == "1")
        //                    {
        //                        return true;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return false;
        //}
        //#endregion
    }

    class FailedUpdateCGObject
    {
        public string SiteId { get; set; }
        public Guid ItemId { get; set; }
        public int Status { get; set; }
        public long ArchiveSize { get; set; }
    }
}
