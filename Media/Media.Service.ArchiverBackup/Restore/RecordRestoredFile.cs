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
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Service.DomainModel.DocAve6x;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Object.Extension;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.AzureTable;
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery;
using LiteDB;
using Microsoft.ProjectServer.Client;
using RazorEngine.Compilation.ImpromptuInterface.Dynamic;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.MSAzure;

namespace Media.Service.ArchiverBackup.Restore
{
    public class RecordRestoredFile
    {
        private static IRALogger mLog = new RALogger(typeof(RecordRestoredFile));
        private const string STORAGE_CONTAINER_NAME = "opus-sqlite-database-container";
        private const string RESTORE_FOLDER_NAME = "delete_archived_data";
        private static readonly string STORAGE_CONNECTION_STRING = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
        private static string TenantId => TenantLocalValue.LogonGroupId;
        private static IRestoredSitesInfoDao RestoredSitesInfoDao => PlatformWindsorManager.GetService<IRestoredSitesInfoDao>();
        //ILicenseHelperService
        private static ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();
        private static IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private static SQLiteConnection _connection;
        private static bool? IsOrealSoft;
        private static string dbName;
        private static string SiteId;
        private static string StoragePath;
        private static bool ExportJob;
        private static string RestoreSetting;
        private static bool IsSoFileLevel;
        private static string CleanRestoredOption;
        private static bool CleanRestoredOptionEnabled;
        private static string SiteUrl;
        private static string SourceSiteUrl;
        private static string SourceSiteUrlReplaceChars;
        private static string TargetPath;
        private static bool EnableDeleteRestoredDataFeature;
        private static readonly Object _object =new();
        private static bool IsUseSqliteSaveRestoredDatas;
        public static Guid CurrentWebId;
        public static void InitContext(string dbname,string siteId,string storagePath,bool exportJob,string restoreSetting,string cleanRestoredOption,bool isSoFileLevel,bool useIndexDb)
        {
            try
            {
                EnableDeleteRestoredDataFeature = LicenseHelperService.IsEnableDeleteRestoreDataFeature();
                IsSoFileLevel = isSoFileLevel;
                ExportJob = exportJob;
                CleanRestoredOption = cleanRestoredOption;
                IsUseSqliteSaveRestoredDatas = useIndexDb;
                if (string.IsNullOrEmpty(CleanRestoredOption))
                {
                    mLog.Info("clean restored option is empty");
                    CleanRestoredOptionEnabled = false;
                }
                else
                {
                    var option = SerializerHelper.DeserializeByDataContractSerializer<CleanRestoredItemsExtension>(CleanRestoredOption);
                    if (option != null)
                    {
                        mLog.Info($"clean restored option enabled is {option.EnableDelArchivedData}");
                        CleanRestoredOptionEnabled = option.EnableDelArchivedData;
                    }
                    else
                    {
                        mLog.Info("clean restored option is null,can not deserialize");
                        CleanRestoredOptionEnabled = false;
                    }
                }
                
                if (!EnableDeleteRestoredDataFeature || ExportJob || !IsSoFileLevel || !CleanRestoredOptionEnabled)
                {
                    mLog.Info($"this is recenter export job,skip InitContext,isRecenterExport:{ExportJob},IsSoFileLevel:{IsSoFileLevel}");
                }
                else
                {
                    if (!IsUseSqliteSaveRestoredDatas)
                    {
                        mLog.Warn("this is not use index db save restored datas");
                    }
                    else
                    {
                        mLog.Info($"start init restored items Context,dbname:{dbname},siteId:{siteId},storagePath;{storagePath})");
                        dbName = dbname;
                        RestoreSetting = restoreSetting;
                        InitRestoreTableInfo(siteId, storagePath);
                        CreateDatabase();
                        var dbPath = GetDBPath();
                        _connection = new SQLiteConnection($"DataSource={dbPath};Version=3;Pooling=False;");
                        _connection.Open();
                        using var sqlCommand = _connection.CreateCommand();
                        sqlCommand.CommandText = CreateTableSql();
                        sqlCommand.ExecuteNonQuery();
                        mLog.Info($"finish init restored items Context");
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Error($"init restored items Context failed,error:{e}");
            }
        }
        private static void InitRestoreTableInfo(string siteId,string storagePath)
        {
            SiteId = siteId;
            StoragePath = storagePath;
        }
        public static void InitSiteUrl(string siteUrl,string sourceUrl)
        {
            SourceSiteUrl = sourceUrl;
            SourceSiteUrlReplaceChars = SourceSiteUrl.ToLower().Replace('/', '_').Replace('\\', '_').Replace('#', '_').Replace('?', '_').Replace('&', '_').Replace('=', '_').Replace('+', '_');
            SiteUrl = siteUrl;
            ArchiverVolumeGenerator pathGenerator = new ArchiverVolumeGenerator();
            TargetPath = pathGenerator.GenerateSitePath(sourceUrl);
        }
        private static string CreateTableSql()
        {
            string result = "create table RestoredItems(id INTEGER PRIMARY KEY AUTOINCREMENT," +
                "SiteId nvarchar(500)," +
                "StorageId nvarchar(500)," +
                "StoragePath nvarchar(500)," +
                "JobId nvarchar(500)," +
                "COL_ID nvarchar(500)," +
                "ItemPathMd5 nvarchar(500)," +
                "RestoreSetting nvarchar," +
                "CleanRestoredOption nvarchar," +
                "RestoredUrl nvarchar," +
                "RestoredTimeTicks long," +
                "RestoredSiteUrl nvarchar" +
                ")";
            return result;
        }
        public static void InsertIntoTable(string storageId, string COL_ID, string ItemPathMd5, string JobId, string restoreUrl)
        {
            try
            {
                if (!EnableDeleteRestoredDataFeature || ExportJob || !CleanRestoredOptionEnabled)
                {
                    mLog.Info($"this is recenter export job,skip InsertIntoTable");
                }
                else
                {
                    lock (_object)
                    {
                        if (!IsUseSqliteSaveRestoredDatas)
                        {
                            RMRecordStorageAzureTableContext.NeedDeleteArchivedDataList.Add(new AvePoint.RA.DB.AzureTable.Model.RMNeedDeleteArchivedDataTableEntity
                            {
                                SiteId = SiteId,
                                WebId = CurrentWebId.ToString(),
                                JobId = JobId,
                                RestoredUrl = restoreUrl,
                                RestoredTicks = DateTime.UtcNow.Ticks,
                                RestoredSiteUrl = SiteUrl,
                                PartitionKey = SourceSiteUrlReplaceChars,
                                RowKey = $"{DateTime.UtcNow.Ticks}_{COL_ID}",
                                BasicIndexId = COL_ID
                            })
                            .GetAwaiter().GetResult();
                        }
                        else
                        {
                            string query = "INSERT INTO RestoredItems (SiteId, StorageId, StoragePath, JobId, COL_ID, ItemPathMd5, RestoreSetting, CleanRestoredOption, RestoredUrl, RestoredTimeTicks, RestoredSiteUrl) VALUES (@SiteId, @StorageId, @StoragePath, @JobId, @COL_ID, @ItemPathMd5, @RestoreSetting, @CleanRestoredOption, @RestoredUrl, @RestoredTimeTicks, @RestoredSiteUrl)";
                            mLog.Info($"start Insert Into RestoredItems Table, query: {query}");
                            using var sqlCommand = _connection.CreateCommand();
                            sqlCommand.CommandText = query;
                            sqlCommand.Parameters.AddWithValue("@SiteId", SiteId);
                            sqlCommand.Parameters.AddWithValue("@StorageId", storageId);
                            sqlCommand.Parameters.AddWithValue("@StoragePath", StoragePath);
                            sqlCommand.Parameters.AddWithValue("@JobId", JobId);
                            sqlCommand.Parameters.AddWithValue("@COL_ID", COL_ID);
                            sqlCommand.Parameters.AddWithValue("@ItemPathMd5", ItemPathMd5);
                            sqlCommand.Parameters.AddWithValue("@RestoreSetting", RestoreSetting);
                            sqlCommand.Parameters.AddWithValue("@CleanRestoredOption", CleanRestoredOption);
                            sqlCommand.Parameters.AddWithValue("@RestoredUrl", restoreUrl);
                            sqlCommand.Parameters.AddWithValue("@RestoredTimeTicks", DateTime.UtcNow.Ticks);
                            sqlCommand.Parameters.AddWithValue("@RestoredSiteUrl", SiteUrl);
                            sqlCommand.ExecuteNonQuery();
                            mLog.Info($"finish Insert Into RestoredItems Table");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Error($"failed insert into RestoredItems table, error: {e}");
            }
        }
        private static void CreateDatabase()
        {
            try
            {
                var dbPath = GetDBPath();
#if DEBUG
                if (File.Exists(dbPath))
                {
                    File.Delete(dbPath);
                }
#endif
                if (File.Exists(dbPath))
                {
                    return;
                }

                EnsureDBFolderPath();
                SQLiteConnection.CreateFile(dbPath);
            }
            catch(Exception e) 
            {
                mLog.Error($"error occurd when create restored item db,error:{e}");
            }
        }

        public static async System.Threading.Tasks.Task UploadRestoredDBToStorageAsync()
        {
            try
            {
                if (!EnableDeleteRestoredDataFeature || ExportJob || !CleanRestoredOptionEnabled)
                {
                    mLog.Info($"this is recenter export job,skip UploadRestoredDBToStorageAsync");
                }
                else
                {
                    if (!IsUseSqliteSaveRestoredDatas)
                    {
                        AddOrUpdateRestoredSite();
                        return;
                    }
                    else
                    {
                        var dbPath = GetDBPath();
                        var containerClient = StorageUtil.GetContainerClient(STORAGE_CONNECTION_STRING, STORAGE_CONTAINER_NAME);
                        await containerClient.CreateIfNotExistsAsync();
                        var blobClient = containerClient.GetBlobClient(SecurityUtils.SafeCombinePath(TenantId.ToString().ToLower(), RESTORE_FOLDER_NAME, dbName));
                        await blobClient.DeleteIfExistsAsync();
                        _connection.Close();
                        _connection.Dispose();
                        using (var fileStream = new FileStream(dbPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            await containerClient.UploadBlobAsync(SecurityUtils.SafeCombinePath(TenantId.ToString().ToLower(), RESTORE_FOLDER_NAME, TargetPath, dbName), fileStream);
                        }
                        File.Delete(dbPath);
                        AddOrUpdateRestoredSite();
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Error($"something went wrong when upload restored db to storage,error:{e}");
            }
        }
        private static void AddOrUpdateRestoredSite()
        {
            if (string.IsNullOrEmpty(SourceSiteUrl))
            {
                mLog.Error($"Skip to add or update restore site, because SiteURL is empty.");
                return;
            }
            var restoredSiteInfo = RestoredSitesInfoDao.GetInfoByUrl(SourceSiteUrl);
            if (restoredSiteInfo != null)
            {
                restoredSiteInfo.LatestRestoreTime = DateTime.UtcNow.Ticks;
                if (string.IsNullOrEmpty(restoredSiteInfo.SiteId))
                {
                    restoredSiteInfo.SiteId = SiteId ?? "";
                }
                RestoredSitesInfoDao.AddOrUpdateRestoredSite(restoredSiteInfo);
            }
            else
            {
                var insertRestoredSiteInfo = new RestoredSitesInfo();
                insertRestoredSiteInfo.Id = Guid.NewGuid();
                insertRestoredSiteInfo.SiteUrl = SourceSiteUrl;
                insertRestoredSiteInfo.SiteId = SiteId ?? "";
                insertRestoredSiteInfo.LatestRestoreTime = DateTime.UtcNow.Ticks;
                RestoredSitesInfoDao.AddOrUpdateRestoredSite(insertRestoredSiteInfo);
            }
        }
        private static string GetDBPath()
        {
            return SecurityUtils.SafeCombinePath(Environment.CurrentDirectory, STORAGE_CONTAINER_NAME, TenantId.ToString().ToLower(), dbName);
        }

        private static void EnsureDBFolderPath()
        {
            var dbFolderPath = SecurityUtils.SafeCombinePath(Environment.CurrentDirectory, STORAGE_CONTAINER_NAME);
            if (!Directory.Exists(dbFolderPath))
            {
                Directory.CreateDirectory(dbFolderPath);
            }

            dbFolderPath = SecurityUtils.SafeCombinePath(dbFolderPath, TenantId.ToString().ToLower());
            if (!Directory.Exists(dbFolderPath))
            {
                Directory.CreateDirectory(dbFolderPath);
            }
        }
        public static bool IsOrealSoftDelete()
        {
            if (IsOrealSoft != null)
            {
                return (bool)IsOrealSoft;
            }
            var realDeleteRetentionDatas = RMKeyValueDao.GetValueByKey("RealDeleteAzureRetentionDatas");
            if (realDeleteRetentionDatas != null)
            {
                bool result;
                if (bool.TryParse(realDeleteRetentionDatas.Value, out result) && result)
                {
                    IsOrealSoft = true;
                }
                else
                {
                    IsOrealSoft = false;
                }
            }
            else
            {
                IsOrealSoft = false;
            }
            return (bool)IsOrealSoft;
        }
    }
}
