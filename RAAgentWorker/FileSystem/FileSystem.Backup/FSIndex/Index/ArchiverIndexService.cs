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




namespace AvePoint.Media.Service.ArchiverBackup
{
    #region using directives
    using AvePoint.GCommon;
    using AvePoint.Media.Common;
    using AvePoint.Media.Core.Index;
    using AvePoint.Media.Service.DomainModel;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using Storage;
    using AvePoint.Common;
    using AvePoint.RA.Common;
    using AvePoint.RA.CommonUtil;
    using AvePoint.GCommon.Utility;
    using AvePoint.GCommon.Utility.Exceptions;
    using AvePoint.Wrapper.Common;
    using AvePoint.Media.Storage.Util;
    using AvePoint.RA.FileSystem.Collect;

    #endregion

    public class ArchiverIndexService : IIndexService<ArchiverIndexServiceOpenParameter>
    {
        private RALogger logger = RALogger.GetInstance(typeof(ArchiverIndexService));
        ArchiverIndexServiceOpenParameter indexServiceOpenParam;

        public IIndexProcessor<ArchiverIndexProcessorParameter> IndexProcessor = new IndexProcessor<ArchiverIndexProcessorParameter>();
        public IIndexDatabaseSynchronizer IndexSynchronizer = new IndexDatabaseSynchronizer();
        public IIndexCacheManager IndexCacheManager { get; set; }

        public void Open(ArchiverIndexServiceOpenParameter openParam)
        {
            this.indexServiceOpenParam = openParam;
            openParam.IndexDatabaseName = this.GenerateArchiverIndexName(openParam);
            ArchiverIndexMutex archiverIndexMutex = new ArchiverIndexMutex(openParam.IndexVolume + openParam.IndexDatabaseName);
            var gotLock = archiverIndexMutex.WaitAsync(openParam.WaitIndexLockerTimeOutInMs).GetAwaiter().GetResult();
            if (!gotLock)
            {
                throw new Exception($"Failed to get lock to open archiver index db: {openParam.IndexVolume}");
            }

            try
            {
                IndexSynchronizer.Initialize(openParam);
                var param = this.GetIndexProcessorParameter(openParam);
                this.logger.Info($"Get DB see master key");
                this.IndexProcessor.Open(param);

                //RAWebLocalCacheReleaser.RecordCacheFile(param.DownLoadResult.IndexFullPath);
            }
            finally
            {
                archiverIndexMutex.Release();
            }
        }

        public void DeleteFileForJob(ArchiverIndexServiceOpenParameter openParam)
        {
            this.indexServiceOpenParam = openParam;
            openParam.IndexDatabaseName = this.GenerateArchiverIndexName(openParam);
            this.DeleteFile(openParam);
        }

        private String GenerateArchiverIndexName(ArchiverIndexServiceOpenParameter openParam)
        {
            var indexDatabaseName = default(String);
            switch (openParam.TreeMode)
            {
                case TreeMode.JobMode:
                    indexDatabaseName = openParam.BackupJobId + "_" + ServiceConstants.IndexDBName;
                    break;
                case TreeMode.SiteCollectionMode:
                    indexDatabaseName = ServiceConstants.IndexDBName;
                    break;
                default:
                    throw new Exception($"Archiver Restore Browser Open Mode Exception:{openParam.TreeMode}");
            }
            return indexDatabaseName;
        }

        private ArchiverIndexProcessorParameter GetIndexProcessorParameter(ArchiverIndexServiceOpenParameter openParam)
        {
            IndexDatabaseDownLoadResult indexDownLoadInfo;
            var realIndexDeviceSystem = (openParam.IndexCacheDeviceSystem != null) ? openParam.IndexCacheDeviceSystem : openParam.IndexLogicalDeviceSystem;
            var logicalStorageInfo = XConvert.FromNames(openParam.IndexVolume, openParam.IndexDatabaseName, openParam.StorageInfo);
            if (openParam.IndexLogicalDeviceSystem.FileExists(logicalStorageInfo))
            {
                if (string.IsNullOrEmpty(FSJobCache.RestoreInstance.FSRestoreCachePath))
                {
                    var dbInfo = new IndexDatabaseInfo(openParam);
                    indexDownLoadInfo = this.IndexSynchronizer.Download(dbInfo);
                }
                else
                {
                    string indexFullPath = SecurityUtils.SafeCombinePath(FSJobCache.RestoreInstance.FSRestoreCachePath, SecurityUtils.SafeCombinePath(openParam.IndexVolume, openParam.IndexDatabaseName));
                    if (!File.Exists(indexFullPath))
                    {
                        var dbInfo = new IndexDatabaseInfo(openParam);
                        indexDownLoadInfo = this.IndexSynchronizer.Download(dbInfo);
                    }
                    else
                    {
                        logger.Info("Index file already exists in cache, no need to download");
                        indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Cached, indexFullPath);
                    }
                }
                //    indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Cached, SecurityUtils.SafeCombinePath(realIndexDeviceSystem.SystemLocation, SecurityUtils.SafeCombinePath(openParam.IndexVolume, openParam.IndexDatabaseName)));
            }
            else
            {
                if (openParam.IsNeedCreateNewIndex)
                    indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Nonexistent, Path.Combine(realIndexDeviceSystem.SystemLocation, Path.Combine(openParam.IndexVolume, openParam.IndexDatabaseName)));
                else throw new Exception("Archiver IndexService Open Index Can Not Found Exception");
            }
            realIndexDeviceSystem.OpenDirectory(XConvert.FromNames(openParam.IndexVolume, string.Empty), FileMode.Create);
            ArchiverIndexProcessorParameter indexProcessorParameter = new ArchiverIndexProcessorParameter();
            indexProcessorParameter.DownLoadResult = indexDownLoadInfo;
            indexProcessorParameter.IndexWorkingSystem = realIndexDeviceSystem;
            indexProcessorParameter.IsNeedCheckIntegrity = openParam.IsNeedCheckIntegrity;
            indexProcessorParameter.DBPassWord = openParam.DBPassWord;
            return indexProcessorParameter;
        }

        public void DeleteFile(ArchiverIndexServiceOpenParameter openParam)
        {
            var dbInfo = new IndexDatabaseInfo(openParam);
            this.IndexSynchronizer.DeleteFile(dbInfo);
        }

        public IndexDatabaseUpLoadResult UploadSubIndexToRealDevice()
        {
            this.IndexProcessor.Close();
            var dbName = this.indexServiceOpenParam.IndexDatabaseName;
            var dbInfo = new IndexDatabaseInfo(dbName, this.indexServiceOpenParam);
            var uploadResult = new IndexDatabaseUpLoadResult();
            if (this.indexServiceOpenParam.IndexCacheDeviceSystem != null)
            {
                IndexSynchronizer.Initialize(indexServiceOpenParam);
                var result = IndexSynchronizer.Upload(dbInfo);
                return result;
            }
            return uploadResult;
        }

        public void Close()
        {
            this.IndexProcessor.Close();
            if (WrapperConfiguration.NeedToUploadIndex)
            {
                logger.Info("this job need to uplaod index");
                var dbInfo = new IndexDatabaseInfo(ServiceConstants.IndexDBName, null);
                this.IndexSynchronizer.Upload(dbInfo);
            }
            this.logger.Info("Archiver Index Service Close End");
        }
    }
}