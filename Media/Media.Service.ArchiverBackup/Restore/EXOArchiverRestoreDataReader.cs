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
using AvePoint.Media.Core.IO;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Storage;
using AvePoint.Media.Core.IO.Input;
using Media.Common.ClassicStorageApi;
using Merged18NResources.MediaServiceArchiverBackup;
using AvePoint.GCommon;
using System.Reflection;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.GCommon.Contract.Storage.Entity;

namespace Media.Service.ArchiverBackup.Restore
{
    public class EXOArchiverRestoreDataReader : DataReaderBase<ExchangeRestoreJob>
        , IInputDataListener
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        IXConverter converter;
        IXSystem LogicalDevice;
        ExchangeRestoreJob archiverRestoreJob;
        IMediaGeneralInputStream GeneralInput;
        Dictionary<string, long> blockSizeMap = new Dictionary<string, long>();
        public ICacheService CacheManager = new CacheService();
        public override IMediaGeneralInputStream Input { get { return GeneralInput; } }
        string SoftDeleteJobIds = string.Empty;
        public IVolumeGeneratorFactory VolumeGeneratorFactory { get; set; }

        public IFileNameGeneratorFactory FileNameGeneratorFactory { get { return new FileNameGeneratorFactory(); } }

        //private SafeDictionary<string, BLOBRehydrationMapping> BLOBMappings;
        //private IArchiverIndexSubInfoDao ArchiverIndexSubInfoDao => PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();
        private IStorageDeviceManager StorageDeviceManager => PlatformWindsorManager.GetService<IStorageDeviceManager>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        //IXSystem SoftDeleteDevice;
        //BlobContainerClient sourceContainerClient;
        public void CloseDataBlock(FileType fileType, string fileName, Stream stream)
        {
            stream.Close();
        }

        public override void Dispose()
        {
            this.blockSizeMap.Clear();
            this.GeneralInput.Close();
            this.LogicalDevice.Close();
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreDataReaderCloseEnd);
        }

        public override void Open(ExchangeRestoreJob restoreJob)
        {
            this.archiverRestoreJob = restoreJob;
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreDataReaderOpenBegin);
            this.LogicalDevice = XFactoryCommon.InstanceLibrary(restoreJob.LogicalDevice.ToXRIS());
            this.LogicalDevice.Open();
            // 如果需要将meta或者content data下载到cache以便减少交叉读取数据块带来的效率损失的话，则open cache.
            this.CacheManager.Open(this.archiverRestoreJob.CacheSetting, LogicalDevice.IsDirectSystem, false);

            OpenInputStreamParameter openParam = new OpenInputStreamParameter();
            openParam.DataListener = this;
            //openParam.IsSupportAutoChangeDataBlock = (this.LogicalDevice as ClassicStorage.AbstractXSystem)?.IsSupportAutoChangeDataBlock == true;
            this.GeneralInput = InputStreamFactory.GetInputStream(openParam, out this.converter);
            this.GeneralInput.Open();
        }

        public XStream OpenDataBlock(DataBlockOpenParam param, out DataBlockOpenOutParam outParam)
        {
            IXSystem dataDevice;
            this.logger.Debug(MediaServiceArchiverBackupResource.ArchiverRestoreDataReaderOpenDataBlockBeginDebug);
            outParam = new DataBlockOpenOutParam();
            String dataVolume = this.archiverRestoreJob.DataVolume;

            var fileNameGenerator = this.FileNameGeneratorFactory.GetFileNameGenerator(ProductModule.ArchiverBackup, param.DataVersion);
            outParam.FileName = fileNameGenerator.Generate(new FileNameParameter(param));
            outParam.FileSize = this.DataBlockGetSize(param.FileType, outParam.FileName, dataVolume);
            //var filePath = Path.Combine(archiverRestoreJob.DataVolume, param.JobId);
            if (param.OpenFromCache)
            {
                dataDevice = this.CacheManager.CacheSystem;
                int retryTime = 0;
                if (param.ShouldDownloadData)
                {
                    while (true)
                    {
                        try
                        {
                            retryTime++;
                            this.CacheManager.DownloadDataFromDevice(this.converter, this.LogicalDevice, param.FileType, archiverRestoreJob.DataVolume, outParam.FileName);
                            break;
                        }
                        catch (Exception ex)
                        {
                            if (retryTime > 5)
                            {
                                throw;
                            }
                            this.logger.Warn($"Archiver OpenDataBlock OpenFromCache DownloadDataFromDevice failed and need retry,RetryTime:{retryTime}.Message:{ex}.");
                            this.CacheManager.CacheSystem.DeleteFile(this.converter.FormNames(param.FileType, archiverRestoreJob.DataVolume, outParam.FileName));
                        }
                    }
                }
            }
            else
            {
                dataDevice = this.LogicalDevice;
            }
            //this.converter.SetFileSize(param.FileType, outParam.FileSize, (this.LogicalDevice as ClassicStorage.AbstractXSystem)?.IsSupportAutoChangeDataBlock == true);
            this.logger.Info(MediaServiceArchiverBackupResource.ArchiverRestoreDataReaderDataBlockOpeningBegin, Path.Combine(dataVolume, outParam.FileName), outParam.FileSize);
            StorageInfo info = this.converter.FormNames(param.FileType, dataVolume, outParam.FileName);

            //if (BLOBMappings != null && BLOBMappings.ContainsKey(Path.Combine(info.HighName, info.LowName)))
            //{
            //    var mapped = BLOBMappings[Path.Combine(info.HighName, info.LowName)].MappedBlobInfo;
            //    logger.Info($"Mapped from [{info.HighPlusLowName}] to [{mapped.HighPlusLowName}].");
            //    info.HighName = mapped.HighName;
            //    info.LowName = mapped.LowName;
            //}
            return dataDevice.OpenStream(info, FileMode.Open);
        }
       

        private long DataBlockGetSize(FileType fileType, string fileName, string dataVolume)
        {
            long blockSize = 0;
            var fileInfo = default(XFileInfo);
            if (this.blockSizeMap.ContainsKey(Path.Combine(dataVolume, fileName).ToLower()))
            {
                blockSize = this.blockSizeMap[Path.Combine(dataVolume, fileName).ToLower()];
            }
            else
            {
                var info = this.converter.FormNames(fileType, dataVolume, fileName);
                if (this.LogicalDevice.FileExists(info))
                    fileInfo = this.LogicalDevice.OpenFile(info);
                else
                {
                    if (RecordRestoredFile.IsOrealSoftDelete())
                    {
                        logger.Info($"blob has not exsit,try to undelete it:{fileName}");
                        fileInfo = this.LogicalDevice.OpenFile(info);
                    }
                    else
                    {
                        throw new FileNotFoundException(String.Format("An error occurred in getting file {0} size in {1}.", fileName, dataVolume));
                    }
                }
                if (fileInfo != null)
                {
                    blockSize = fileInfo.FileSize;
                    this.blockSizeMap.Add(Path.Combine(dataVolume, fileName).ToLower(), blockSize);
                }
            }
            return blockSize;
        }


        public XStream OpenDataBlockForGetVersion(DataBlockOpenParam param)
        {
            IXSystem dataDevice;
            StorageInfo info = new StorageInfo();
            var fileNameGenerator = this.FileNameGeneratorFactory.GetFileNameGenerator(ProductModule.ArchiverBackup, param.DataVersion);
            info.LowName = fileNameGenerator.Generate(new FileNameParameter(param));
            info.HighName = this.archiverRestoreJob.DataVolume;
            info.Offset = 0;
            info.Length = 4;
            if (param.OpenFromCache)
            {
                dataDevice = this.CacheManager.CacheSystem;
                int retryTime = 0;
                if (param.ShouldDownloadData)
                {
                    while (true)
                    {
                        try
                        {
                            retryTime++;
                            this.CacheManager.DownloadDataFromDevice(this.converter, this.LogicalDevice, param.FileType, info.HighName, info.LowName);
                            break;
                        }
                        catch (Exception ex)
                        {
                            if (retryTime > 5)
                            {
                                throw;
                            }
                            this.logger.Warn($"Archiver OpenDataBlockForGetVersion OpenFromCache DownloadDataFromDevice failed and need retry,RetryTime:{retryTime}.Message:{ex}.");
                            this.CacheManager.CacheSystem.DeleteFile(this.converter.FormNames(param.FileType, info.HighName, info.LowName));
                        }
                    }
                }
            }
            else
            {
                dataDevice = this.LogicalDevice;
            }
            return dataDevice.OpenStream(info, FileMode.Open);
        }
    }
}
