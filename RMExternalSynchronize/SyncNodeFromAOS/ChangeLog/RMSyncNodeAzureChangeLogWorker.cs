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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.SyncNode;
using AvePoint.RA.Contract.SyncNode.GoogleSyncNode;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.AzureTable.Model;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.SharePoint.Object;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMSynchronize.SyncNodeFromAOS.ChangeLog
{
    public class RMSyncNodeAzureChangeLogWorker
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMSyncNodeAzureChangeLogWorker));

        public const string TABLE_NAME = "ChangeLogs";

        public const string FOLDER_NAME = "SyncNodeChanges";

        public const string CREATE_TABLE_BCS_TERM_USAGE_REPORT = @"
Create table {0} (ID integer primary key autoincrement,NodeId nvarchar (500),AosId nvarchar (500),BeforeUrl nvarchar (500),Url nvarchar (500),
ContainerId nvarchar (500),ContainerName nvarchar (500),NodeLevel int,ChangeType int,IsContainer integer,O365TenantId nvarchar (500),ContentSource int,RealId nvarchar (500),MoveSourceContainerId nvarchar (500))";

        public const string INSERT_DATA_BCS_TERM_USAGE_REPORT = @"
Insert into {0} (NodeId,AosId,BeforeUrl,Url,ContainerId,ContainerName,NodeLevel,ChangeType,IsContainer,O365TenantId,ContentSource,RealId,MoveSourceContainerId)
 Values (@NodeId,@AosId,@BeforeUrl,@Url,@ContainerId,@ContainerName,@NodeLevel,@ChangeType,@IsContainer,@O365TenantId,@ContentSource,@RealId,@MoveSourceContainerId)";

        public string CREATE_TABLE_SQL = string.Format(CREATE_TABLE_BCS_TERM_USAGE_REPORT, TABLE_NAME);

        public string INSERT_DATE_SQL = string.Format(INSERT_DATA_BCS_TERM_USAGE_REPORT, TABLE_NAME);

        private readonly string _syncNodeJobId;

        private readonly string _reportFilePath;

        public static string REPORT_FOLDER
        {
            get
            {
                return RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.REPORT_TEMP_FOLDER];
            }
        }

        public RMSyncNodeAzureChangeLogWorker(string syncNodeJobId, bool isTempSub)
        {
            _syncNodeJobId = syncNodeJobId;
            _reportFilePath = GetChangeLogReportPath(TenantLocalValue.LogonGroupId, isTempSub);
        }

        public void UploadChangeLogReport(string reportFilePath)
        {
            if (!string.IsNullOrEmpty(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING]) && File.Exists(reportFilePath))
            {
                try
                {
                    s_logger.Info($"start to upload file");
                    var tenantFolderName = TenantLocalValue.LogonGroupId;
                    var blobName = new StringBuilder();
                    if (!string.IsNullOrEmpty(tenantFolderName))
                    {
                        blobName.Append(tenantFolderName).Append("/");
                    }
                    blobName.Append(FOLDER_NAME).Append("/");
                    blobName.Append(Path.GetFileName(reportFilePath));
                    RAStorageUtil.UploadChangeLogBlob(blobName.ToString(), reportFilePath);
                    s_logger.Info($"finish to upload blob name:{blobName}");
                    DeleteFile(reportFilePath);
                    s_logger.Info($"finish to delete file.");
                }
                catch (Exception ex)
                {
                    s_logger.Error($"Error uploading file {reportFilePath}: {ex.Message}");
                }
            }
            else
            {
                s_logger.Warn($"Report file path is empty or file does not exist: {reportFilePath}");
            }
        }

        public static string GetBlobFolderName()
        {
            var tenantFolderName = TenantLocalValue.LogonGroupId;
            var blobName = new StringBuilder();
            if (!string.IsNullOrEmpty(tenantFolderName))
            {
                blobName.Append(tenantFolderName).Append("/");
            }
            blobName.Append(FOLDER_NAME).Append("/");
            return blobName.ToString();
        }

        public string DownloadReports()
        {
            try
            {
                var tenantFolderName = TenantLocalValue.LogonGroupId;
                var blobName = new StringBuilder();
                blobName.Append(GetBlobFolderName());
                blobName.Append(Path.GetFileName(_reportFilePath));

                if (SQLCommond.CanConnectToReportFile(_reportFilePath))
                {
                    return _reportFilePath;
                }
                RAStorageUtil.DownloadChangeLogReport(blobName.ToString(), _reportFilePath);
            }
            catch (Exception e)
            {
                s_logger.Error($"download detail file fail, error : {e}");
            }
            return _reportFilePath;
        }

        public void DeleteStorageFile()
        {
            try
            {
                var tenantFolderName = TenantLocalValue.LogonGroupId;
                var blobName = new StringBuilder();
                if (!string.IsNullOrEmpty(tenantFolderName))
                {
                    blobName.Append(tenantFolderName).Append("/");
                }
                blobName.Append(FOLDER_NAME).Append("/");
                blobName.Append(Path.GetFileName(_reportFilePath));
                RAStorageUtil.DeleteChangeLogBlob(blobName.ToString());
            }
            catch (Exception ex)
            {
                s_logger.Error($"delete storage file faile,error:{ex}.");
            }
        }


        public void DeleteLocalFile(string reportFilePath)
        {
            try
            {
                DeleteFile(reportFilePath);
                s_logger.Info($"finish to delete file.");
            }
            catch (Exception ex)
            {
                s_logger.Error($"delete storage file faile,error:{ex}.");
            }
        }


        public void CreateTableNew()
        {
            try
            {
                CheckAndCreateDirectory(_reportFilePath);
                SQLCommond.ExecuteNonQuery(_reportFilePath, CREATE_TABLE_SQL);
                s_logger.Debug("Successfulfull to create table {0}.", TABLE_NAME);
            }
            catch (Exception ex)
            {
                s_logger.Error("failed to create table {0}.", TABLE_NAME);
                s_logger.Error(ex.ToString());
            }
        }

        public string GetChangeLogReportPath(string tenantIdentity, bool isTempSub)
        {
            var rootFolder = isTempSub ? SecurityUtils.SafeCombinePath(REPORT_FOLDER, "Temp") : REPORT_FOLDER;
            var jobReportPath = SecurityUtils.SafeCombinePath(rootFolder, tenantIdentity);
            jobReportPath = SecurityUtils.SafeCombinePath(jobReportPath, "ChangeLogs", _syncNodeJobId + ".rpt");
            return jobReportPath;
        }

        public List<SQLiteParameter> BuildSQLiteParameters(RMContainerInfoAdaption node, SourceFlag contentSource, RMSyncNodeChangeType changeType, string beforeUrl = "", string changedUrl = "")
        {
            List<SQLiteParameter> parameters =
            [
                new SQLiteParameter("NodeId", node.Id),
                new SQLiteParameter("BeforeUrl", beforeUrl),
                new SQLiteParameter("Url", changeType == RMSyncNodeChangeType.ChangeName ? changedUrl : node.Name),
                new SQLiteParameter("AosId", ""),
                new SQLiteParameter("ChangeType", (int)changeType),
                new SQLiteParameter("NodeLevel", (int)node.NodeLevel),
                new SQLiteParameter("ContainerId", ""),
                new SQLiteParameter("IsContainer", true),
                new SQLiteParameter("O365TenantId", ""),
                new SQLiteParameter("ContainerName", ""),
                new SQLiteParameter("ContentSource", (int)contentSource),
                new SQLiteParameter("RealId", ""),
                new SQLiteParameter("MoveSourceContainerId", ""),
            ];
            return parameters;
        }

        public List<SQLiteParameter> BuildSQLiteParameters(RMSiteNodeAdaption node, SourceFlag contentSource, RMSyncNodeChangeType changeType, string beforeUrl = "", string changedUrl = "")
        {
            List<SQLiteParameter> parameters =
            [
                new SQLiteParameter("NodeId", node.Id),
                new SQLiteParameter("BeforeUrl", beforeUrl),
                new SQLiteParameter("Url", changeType == RMSyncNodeChangeType.ChangeName ? changedUrl : node.Url),
                new SQLiteParameter("AosId", node.ObjectId),
                new SQLiteParameter("ChangeType", (int)changeType),
                new SQLiteParameter("NodeLevel", (int)node.NodeLevel),
                new SQLiteParameter("ContainerId", node.ContainerId),
                new SQLiteParameter("IsContainer", false),
                new SQLiteParameter("O365TenantId", node.TenantId),
                new SQLiteParameter("ContainerName", node.ContainerName),
                new SQLiteParameter("ContentSource", (int)contentSource),
                new SQLiteParameter("RealId", contentSource == SourceFlag.Teams ? node.TeamId : ""),
                new SQLiteParameter("MoveSourceContainerId", ""),
            ];
            return parameters;
        }

        public List<SQLiteParameter> BuildSQLiteParameters(RMExchangeNodeAdaption node, SourceFlag contentSource, RMSyncNodeChangeType changeType, string beforeUrl = "", string changedUrl = "")
        {
            List<SQLiteParameter> parameters =
            [
                new SQLiteParameter("NodeId", node.Id),
                new SQLiteParameter("BeforeUrl", beforeUrl),
                new SQLiteParameter("Url", changeType == RMSyncNodeChangeType.ChangeName ? changedUrl : node.EmailAddress),
                new SQLiteParameter("AosId", node.ObjectId),
                new SQLiteParameter("ChangeType", (int)changeType),
                new SQLiteParameter("NodeLevel", (int)node.NodeLevel),
                new SQLiteParameter("ContainerId", node.ContainerId),
                new SQLiteParameter("IsContainer", false),
                new SQLiteParameter("O365TenantId", node.TenantId),
                new SQLiteParameter("ContainerName", node.ContainerName),
                new SQLiteParameter("ContentSource", (int)contentSource),
                new SQLiteParameter("RealId", ""),
                new SQLiteParameter("MoveSourceContainerId", ""),
            ];
            return parameters;
        }        
        
        public List<SQLiteParameter> BuildSQLiteParameters(RMGoogleNodeAdaption node, SourceFlag contentSource, RMSyncNodeChangeType changeType, string beforeUrl = "", string changedUrl = "")
        {
            List<SQLiteParameter> parameters =
            [
                new SQLiteParameter("NodeId", node.Id),
                new SQLiteParameter("BeforeUrl", beforeUrl),
                new SQLiteParameter("Url", changeType == RMSyncNodeChangeType.ChangeName ? changedUrl : node.Name),
                new SQLiteParameter("AosId", node.ObjectId),
                new SQLiteParameter("ChangeType", (int)changeType),
                new SQLiteParameter("NodeLevel", (int)node.NodeLevel),
                new SQLiteParameter("ContainerId", node.ContainerId),
                new SQLiteParameter("IsContainer", false),
                new SQLiteParameter("O365TenantId", node.TenantId),
                new SQLiteParameter("ContainerName", node.ContainerName),
                new SQLiteParameter("ContentSource", (int)contentSource),
                new SQLiteParameter("RealId", ""),
                new SQLiteParameter("MoveSourceContainerId", ""),
            ];
            return parameters;
        }

        public List<RMSyncNodeChangeInfo> GetData(int startPage, string condition)
        {
            var selectDataSql = InitGetDataSQLString(1000, startPage, condition);
            List<RMSyncNodeChangeInfo> result = null;
            try
            {
                if (SecurityUtils.ValidateSQLiteConnectionWithBuilder(_reportFilePath, out var builder))
                {
                    using (SQLiteConnection conn = new SQLiteConnection(builder.ConnectionString))
                    {
                        conn.Open();
                        try
                        {
                            using (SQLiteCommand cmd = new SQLiteCommand(selectDataSql, conn))
                            {
                                using (SQLiteDataReader sqlReader = cmd.ExecuteReader())
                                {
                                    result = Convert(sqlReader);
                                }

                            }
                        }
                        catch (Exception e)
                        {
                            s_logger.Error(string.Format("{0},{1}", e.Message, e));
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                s_logger.Error(string.Format("Get detail failed. {0}", e));
            }
            return result;
        }

        public int GetCount(string condition)
        {
            int result = 0;
            var selectDataSql = InitGetDataCountSQLString(condition);
            try
            {
                if (SecurityUtils.ValidateSQLiteConnectionWithBuilder(_reportFilePath, out var builder))
                {
                    using (SQLiteConnection conn = new SQLiteConnection(builder.ConnectionString))
                    {
                        conn.Open();
                        try
                        {
                            using (SQLiteCommand cmd = new SQLiteCommand(selectDataSql, conn))
                            {
                                using (SQLiteDataReader sqlReader = cmd.ExecuteReader())
                                {
                                    if (sqlReader.Read())
                                    {
                                        result = int.Parse(sqlReader[0].ToString());
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            s_logger.Error(string.Format("{0},{1}", e.Message, e));
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                s_logger.Error(string.Format("Get detail failed. {0}", e));
            }
            return result;
        }

        private static void CheckAndCreateDirectory(string reportFilePath)
        {
            FileInfo reportFile = new FileInfo(reportFilePath);
            if (!reportFile.Directory.Exists)
            {
                reportFile.Directory.Create();
                s_logger.Debug("Create Directory:", reportFile.Directory.Name);
            }
        }

        private static void DeleteFile(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                s_logger.Error($"delete file faile,file path:{file},error:{ex}.");
            }
        }

        private List<RMSyncNodeChangeInfo> Convert(SQLiteDataReader sqlReader)
        {
            List<RMSyncNodeChangeInfo> result = new List<RMSyncNodeChangeInfo>();
            while (sqlReader.Read())
            {
                result.Add(ConvertToSyncNodeChangeInfo(sqlReader));
            }
            return result;
        }

        private static RMSyncNodeChangeInfo ConvertToSyncNodeChangeInfo(SQLiteDataReader sqlReader)
        {
            return new()
            {
                Id = sqlReader["NodeId"]?.ToString(),
                AosId = sqlReader["AosId"]?.ToString(),
                BeforeUrl = sqlReader["BeforeUrl"]?.ToString(),
                ChangeType = int.TryParse(sqlReader["ChangeType"]?.ToString(), out var changeType) ? (RMSyncNodeChangeType)changeType : RMSyncNodeChangeType.None,
                ContainerId = sqlReader["ContainerId"]?.ToString(),
                ContainerName = sqlReader["ContainerName"]?.ToString(),
                ContentSource = int.TryParse(sqlReader["ChangeType"]?.ToString(), out var contentSource) ? (SourceFlag)contentSource : SourceFlag.None,
                IsContainer = int.TryParse(sqlReader["IsContainer"]?.ToString(), out var isContainer) ? (isContainer == 0 ? false : true) : false,
                MoveSourceContainerId = sqlReader["MoveSourceContainerId"]?.ToString(),
                NodeLevel = int.TryParse(sqlReader["NodeLevel"]?.ToString(), out var nodeLevel) ? (NodeLevel)nodeLevel : NodeLevel.Undefined,
                O365TenantId = sqlReader["O365TenantId"]?.ToString(),
                RealId = string.IsNullOrEmpty(sqlReader["RealId"]?.ToString()) ? Guid.Empty : new Guid(sqlReader["RealId"]?.ToString()),
                Url = sqlReader["Url"]?.ToString(),
            };
        }

        private static string InitGetDataSQLString(int PageSize, int StartPage, string conditionFilter)
        {
            if (string.IsNullOrEmpty(conditionFilter))
            {
                return string.Format(JobMonitorConstants.SELECT_DATA_FROM_TABLE, TABLE_NAME, PageSize, (StartPage - 1) * PageSize);
            }
            else
            {
                return string.Format(JobMonitorConstants.SELECT_DATA_ON_CONDITION_FROM_TABLE, TABLE_NAME, conditionFilter, PageSize, (StartPage - 1) * PageSize);
            }
        }

        private static string InitGetDataCountSQLString(string conditionFilter)
        {
            if (string.IsNullOrEmpty(conditionFilter))
            {
                return string.Format(JobMonitorConstants.SELECT_DETAIL_COUNT_SQL, TABLE_NAME);
            }
            else
            {
                return string.Format(JobMonitorConstants.SELECT_DETAIL_COUNT_ON_CONDITION_SQL, TABLE_NAME, conditionFilter);
            }
        }
    }
}
