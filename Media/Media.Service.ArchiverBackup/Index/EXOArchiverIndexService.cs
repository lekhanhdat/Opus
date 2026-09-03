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
using AvePoint.GCommon.Utility.Exceptions;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Common;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.Wrapper.Common;
using Merged18NResources.MediaServiceArchiverBackup;
using Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Service.ArchiverBackup.Index
{
    public class EXOArchiverIndexService:IIndexService<ExchangeIndexServiceOpenParameter>
    {
        private RALogger logger = RALogger.GetInstance(typeof(ArchiverIndexService));
        ExchangeIndexServiceOpenParameter indexServiceOpenParam;
        public ArchiverIndexService _IndexService { get; set; }
        public IIndexProcessor<EXOArchiverIndexProcessorParameter> IndexProcessor = new IndexProcessor<EXOArchiverIndexProcessorParameter>();
        public IIndexDatabaseSynchronizer IndexSynchronizer =new IndexDatabaseSynchronizer();
        public IIndexCacheManager IndexCacheManager => PlatformWindsorManager.GetService<IIndexCacheManager>();

        public void Open(ExchangeIndexServiceOpenParameter openParam)
        {
            this.indexServiceOpenParam = openParam;
            openParam.IndexDatabaseName = this.GenerateArchiverIndexName(openParam);
            ArchiverIndexMutex archiverIndexMutex = new ArchiverIndexMutex(openParam.IndexVolume + openParam.IndexDatabaseName);
            var gotLock = archiverIndexMutex.WaitAsync(openParam.WaitIndexLockerTimeOutInMs).GetAwaiter().GetResult();
            if (!gotLock)
            {
                throw new OpenIndexDbTimeoutException($"Failed to get lock to open archiver index db: {openParam.IndexVolume}");
            }

            try
            {
                IndexSynchronizer.Initialize(openParam);
                var param = this.GetIndexProcessorParameter(openParam);
                this.logger.Info($"Get DB see master key by {openParam.TenantGroupId}");
                this.IndexProcessor.Open(param);

                RAWebLocalCacheReleaser.RecordCacheFile(param.DownLoadResult.IndexFullPath);
            }
            finally
            {
                archiverIndexMutex.Release();
            }
        }

        public void DeleteFileForJob(ExchangeIndexServiceOpenParameter openParam)
        {
            this.indexServiceOpenParam = openParam;
            openParam.IndexDatabaseName = this.GenerateArchiverIndexName(openParam);
            this.DeleteFile(openParam);
        }

        private String GenerateArchiverIndexName(ExchangeIndexServiceOpenParameter openParam)
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
                    throw new Exception(string.Format(MediaServiceArchiverBackupResource.ArchiverRestoreBrowserOpenModeException, openParam.TreeMode));
            }
            return indexDatabaseName;
        }

        private EXOArchiverIndexProcessorParameter GetIndexProcessorParameter(ExchangeIndexServiceOpenParameter openParam)
        {
            if (MediaConfigInfo.CommonConfigInfo == null)
            {
                MediaConfigInfo.CommonConfigInfo = PlatformWindsorManager.GetService<CommonConfigInfo>();
            }
            IndexDatabaseDownLoadResult indexDownLoadInfo;
            var realIndexDeviceSystem = (openParam.IndexCacheDeviceSystem != null && MediaConfigInfo.CommonConfigInfo.ForceUseCache) ? openParam.IndexCacheDeviceSystem : openParam.IndexLogicalDeviceSystem;
            var logicalStorageInfo = XConvert.FromNames(openParam.IndexVolume, openParam.IndexDatabaseName, openParam.StorageInfo);
            if (openParam.IndexLogicalDeviceSystem.FileExists(logicalStorageInfo))
            {
                if (MediaConfigInfo.CommonConfigInfo.ForceUseCache && openParam.IndexCacheDeviceSystem != null)
                {
                    var dbInfo = new IndexDatabaseInfo(openParam);
                    indexDownLoadInfo = this.IndexSynchronizer.Download(dbInfo);
                }
                else
                    indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Cached, SecurityUtils.SafeCombinePath(realIndexDeviceSystem.SystemLocation, SecurityUtils.SafeCombinePath(openParam.IndexVolume, openParam.IndexDatabaseName)));
            }
            else
            {
                if (openParam.IsNeedCreateNewIndex)
                    indexDownLoadInfo = new IndexDatabaseDownLoadResult(IndexDatabaseStatus.Nonexistent, SecurityUtils.SafeCombinePath(realIndexDeviceSystem.SystemLocation, SecurityUtils.SafeCombinePath(openParam.IndexVolume, openParam.IndexDatabaseName)));
                else throw new IndexCanNotFoundException(MediaServiceArchiverBackupResource.ArchiverIndexServiceOpenIndexCanNotFoundException);
            }
            realIndexDeviceSystem.OpenDirectory(XConvert.FromNames(openParam.IndexVolume, string.Empty), FileMode.Create);
            IdentityManager.IdentityMode = IdentityMode.Process;
            EXOArchiverIndexProcessorParameter indexProcessorParameter = new EXOArchiverIndexProcessorParameter(IdentityManager.IdentityContent);
            indexProcessorParameter.DownLoadResult = indexDownLoadInfo;
            indexProcessorParameter.IndexWorkingSystem = realIndexDeviceSystem;
            indexProcessorParameter.IsNeedCheckIntegrity = openParam.IsNeedCheckIntegrity;
            return indexProcessorParameter;
        }

        public void DeleteFile(ExchangeIndexServiceOpenParameter openParam)
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
            if (this.indexServiceOpenParam.IndexCacheDeviceSystem != null && MediaConfigInfo.CommonConfigInfo.ForceUseCache)
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
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverIndexServiceCloseEnd);
        }
    }
}
