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

namespace Office365GroupRetention
{
    #region using directives

    using System;
    using System.Diagnostics;
    using System.IO;

    using AvePoint.Media.Common;
    using AvePoint.Media.Core.Index;
    using AvePoint.Media.Service.ArchiverBackup;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.RA.CommonUtil;
    using AvePoint.RA.Contract.Tenant;
    using Merged18NResources.MediaServiceArchiverBackup;
    using Office365Group;
    using Storage;
    using Storage.Util;

    #endregion

    public class ExchangeIndexService
        : IIndexService<ExchangeIndexServiceOpenParameter>
    {
        private static RALogger logger = RALogger.GetInstance(typeof(ExchangeIndexService));
        private ExchangeIndexServiceOpenParameter openParam;
        private String storageInfo;

        public IDatabaseNameGenerator DatabaseNameGenerator = new ExchangeDatabaseNameGenerator();

        public IIndexDatabaseSynchronizer IndexSynchronizer { get; set; }

        public IIndexProcessor<ExchangeIndexProcessorParameter> IndexProcessor { get; set; }

        public IExchangeBackupIndexService ExchangeBackupIndexService { get; set; }

        //public BackupOptions Config { get; set; }

        public void Open(ExchangeIndexServiceOpenParameter openParam)
        {
            logger.Info("Exchange index service begin opening.");
            if (!String.IsNullOrEmpty(this.storageInfo))
            {
                openParam.StorageInfo = this.storageInfo;
            }
            this.openParam = openParam;
            if (openParam.IndexCacheDeviceSystem != null && !openParam.IndexCacheDeviceSystem.IsDirectSystem)
            {
                logger.Info("Exchange index service is working on non-local system.");
            }
            IndexSynchronizer.Initialize(openParam);
            var param = new ExchangeIndexProcessorParameter(TenantLocalValue.LogonGroupId);
            var dbInfo = new IndexDatabaseInfo(openParam);
            try
            {
                this.LoopFindIndex(openParam, param, dbInfo);
            }
            catch (FileNotFoundException e)
            {
                logger.Warn("Exchange index {0} was not found. Changing index name to index.db, details: {1}.", dbInfo.DbFileName, e.ToString());
                try
                {
                    //dbInfo.DbFileName = !openParam.UserAddress.IsNullOrEmpty() ? DatabaseNameGenerator.Generate(new ExchangeDataBaseInfo { UesrAddress = openParam.UserAddress, Is64BitProcess = !MediaEnvironment.Is64BitProcess }) : ServiceConstants.IndexDBName;
                    dbInfo.DbFileName = ServiceConstants.IndexDBName;// !string.IsNullOrEmpty(openParam.UserAddress)? string.Format("index{0}.db", BackupOptions.AgentIndexMapping[openParam.UserAddress]) : ServiceConstants.IndexDBName;
                    param.DownLoadResult = IndexSynchronizer.Download(dbInfo);
                }
                catch (FileNotFoundException ex)
                {
                    logger.Warn("Exchange index {0} was not found. Changing index name to index.db, details: {1}.", dbInfo.DbFileName, ex.ToString());
                    string catchPath = openParam.CacheSetting.Extension.Path[0].DiskInfo.Path;
                    param.DownLoadResult.IndexFullPath = Path.Combine(catchPath, openParam.IndexVolume, dbInfo.DbFileName);
                }
            }
            param.IndexWorkingSystem = (!openParam.IndexLogicalDeviceSystem.IsDirectSystem || MediaConfigInfo.CommonConfigInfo.ForceUseCache) ?
                openParam.IndexCacheDeviceSystem : openParam.IndexLogicalDeviceSystem;
            logger.Info("Start opening exchange index processor.");
            IndexProcessor.Open(param);
            logger.Info("Open exchange index service successfully.");
        }

        public void Close()
        {
            if (IndexProcessor != null)
            {
                IndexProcessor.Close();
            }
        }

        public IndexDatabaseUpLoadResult CommitDB(Boolean needRenameIndex = default(Boolean))
        {
            //String dbName = DatabaseNameGenerator.Generate(new ExchangeDataBaseInfo { UesrAddress = userAddress, Is64BitProcess = true });
            String dbName ="index.db";
            var dbInfo = new IndexDatabaseInfo(dbName, this.openParam);
            dbInfo.NeedRenameIndexName = needRenameIndex;
            logger.Info("Exchange index service start committing index: {0}.", dbName);
            IndexDatabaseUpLoadResult result = IndexSynchronizer.Upload(dbInfo);
            return result;
        }

        public IndexDatabaseUpLoadResult CommitAgentIndexFile(String fileName)
        {
            //logger.Info("Exchange index service start committing agent index: {0}.", fileName);
            ////string storageInfo = fileName.Contains("lastBackup") ? BackupDataCommunicator.lastBackupInfo : string.Empty;
            //string storageInfo = string.Empty;
            //if (fileName.Contains("lastBackup"))
            //{
            //    storageInfo = BackupDataCommunicator.lastBackupInfo;
            //}
            //else if (fileName.Contains("lastFullBackup"))
            //{
            //    storageInfo = BackupDataCommunicator.lastFullBackUpInfo;
            //}

            //var dbInfo = new IndexDatabaseInfo(fileName, storageInfo, this.openParam);
            //dbInfo.IsForceUpload = true;
            //var result = IndexSynchronizer.Upload(dbInfo);
            //return result;
            return null;
        }

        private String GenerateArchiverIndexName(ExchangeIndexServiceOpenParameter openParam)
        {
            return $"{openParam.BackupJobId}_{ServiceConstants.IndexDBName}";
        }

        private void LoopFindIndex(ExchangeIndexServiceOpenParameter openParam, ExchangeIndexProcessorParameter param, IndexDatabaseInfo dbInfo)
        {
            var userAddress = openParam.UserAddress;
            //var newIndexDBCode = BackupCommonUtility.GetAgentIndexName(userAddress, ExchangeUtility.ExchangeMailboxType.Teams, true, true);
            var newIndexDBName = openParam.IndexDatabaseName;// GenerateArchiverIndexName(openParam); //$"index{newIndexDBCode}.db";

            
            var newIndexDBStorageInfo = XConvert.FromNames(openParam.IndexVolume, newIndexDBName);
            var hasNewIndexDB = openParam.IndexLogicalDeviceSystem.FileExists(newIndexDBStorageInfo);

            //if (hasNewIndexDB)
            //{
            //    DownloadNewIndexDB();
            //    return;
            //}

            //var oldIndexDBName = $"index{BackupOptions.AgentIndexMapping[userAddress]}.db";
            //var oldIndexDBStorageInfo = XConvert.FromNames(openParam.IndexVolume, oldIndexDBName);
            //var hasOldIndexDB = openParam.IndexLogicalDeviceSystem.FileExists(oldIndexDBStorageInfo);
            //if (hasOldIndexDB)
            //{
            //    dbInfo.DbFileName = oldIndexDBName;
            //    param.DownLoadResult = IndexSynchronizer.Download(dbInfo);
            //    param.IndexWorkingSystem = (!openParam.IndexLogicalDeviceSystem.IsDirectSystem || MediaConfigInfo.CommonConfigInfo.ForceUseCache) ? openParam.IndexCacheDeviceSystem : openParam.IndexLogicalDeviceSystem;
            //    IndexProcessor.Open(param);
            //    if (ExchangeBackupIndexService.GetContainerCount() > 1 || !ExchangeBackupIndexService.HasContainter(userAddress.Remove(userAddress.LastIndexOf(BackupConstants.CONTAINTERSUFFIX)).ToMD5HashCode()))
            //    {
            //        IndexProcessor.Close();
            //        DownloadNewIndexDB();
            //    }
            //    return;
            //}

            DownloadNewIndexDB();

            void DownloadNewIndexDB()
            {
                dbInfo.DbFileName = newIndexDBName;
                param.DownLoadResult = IndexSynchronizer.Download(dbInfo);
                //BackupOptions.AgentIndexMapping[userAddress] = newIndexDBCode;
            }
        }

        //public long GetIndexDBSize(string mailboxName)
        //{
        //    try
        //    {
        //        //var indexDBName = DatabaseNameGenerator.Generate(new ExchangeDataBaseInfo { UesrAddress = mailboxName, Is64BitProcess = true });
        //        var indexDBName = string.Format("index{0}.db", BackupOptions.AgentIndexMapping[mailboxName]);
        //        var dbInfo = new IndexDatabaseInfo(indexDBName, this.openParam);
        //        var param = new IndexCacheManagerParameter()
        //        {
        //            IndexName = dbInfo.DbFileName,
        //            IndexVolume = openParam.IndexVolume,
        //            CacheSetting = openParam.CacheSetting,
        //            CacheSystem = openParam.IndexCacheDeviceSystem,
        //            StorageSystem = openParam.IndexLogicalDeviceSystem,
        //        };
        //        var info = XConvert.FromNames(param.IndexVolume, param.IndexName);
        //        return openParam.IndexCacheDeviceSystem.OpenFile(info).FileSize;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("An error occurred while getting IndexDBSize {0}.", ex.ToString());
        //        return 0;
        //    }
        //}

        public void DeleteFileForJob(ExchangeIndexServiceOpenParameter openParam)
        {
            throw new NotImplementedException();
        }

        public bool IntegrityCheck(string indexDbpath)
        {
            logger.Info("Start to check index db integrity. FilePath: {0}", indexDbpath);
            var monitor = Stopwatch.StartNew();
            try
            {
                var scanResult = Convert.ToString(IndexProcessor.ExecuteScalar("PRAGMA quick_check", null));
                var result = scanResult.Equals("OK", StringComparison.OrdinalIgnoreCase);
                logger.Info($"Finish to check index db integrity before commit db, the check result is {result}, TimeCost:{monitor.Elapsed}, indexpath:{indexDbpath}");
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while to check index db integrity. Reason: {0}.", ex);
                return false;
            }
        }
    }
}