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

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.Media.Core.IO;
    using AvePoint.Media.Core.IO.Input;
    using AvePoint.Media.Service.DomainModel;
    using Storage;
    using global::Media.Common.ClassicStorageApi;
    using AvePoint.RA.Common;
    using AvePoint.RA.Contract.Services;
    using AvePoint.GCommon.Contract.Storage.Entity;
    using Azure.Storage.Blobs;
    using System.Linq;
    using RAFileSystem.FileSystem.FileSystem.Restore.Common;


    #endregion using directives

    [AveCodeReview(
    "2012/8/2",
    "dwxue@avepoint.com",
    "yjhuo@avepoint.com",
    new string[] { },
    null,
    true)]
    public class ArchiverRestoreDataReader
        : DataReaderBase<ArchiverRestoreJob>
        , IInputDataListener
    {
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        IXConverter converter;
        IXSystem LogicalDevice;
        ArchiverRestoreJob archiverRestoreJob;
        IMediaGeneralInputStream GeneralInput;
        Dictionary<string, long> blockSizeMap = new Dictionary<string, long>();
        private SafeDictionary<string, BLOBRehydrationMapping> BLOBMappings;
        public override IMediaGeneralInputStream Input { get { return GeneralInput; } }
        string SoftDeleteJobIds = string.Empty;


        public IFileNameGeneratorFactory FileNameGeneratorFactory { get { return new FileNameGeneratorFactory(); } }


        IXSystem SoftDeleteDevice;
        BlobContainerClient sourceContainerClient;
        public override void Open(ArchiverRestoreJob restoreJob)
        {
            this.archiverRestoreJob = restoreJob;
            this.logger.Info("Archiver Restore DataReader Open Begin");
            this.LogicalDevice = restoreJob.LogicalDeviceSystem;

            OpenInputStreamParameter openParam = new OpenInputStreamParameter();
            openParam.DataListener = this;
            openParam.IsSupportAutoChangeDataBlock = this.LogicalDevice.IsSupportAutoChangeDataBlock;
            this.GeneralInput = InputStreamFactory.GetInputStream(openParam, out this.converter);
            this.GeneralInput.Open();
        }

        public override void Dispose()
        {
            this.blockSizeMap.Clear();
            this.GeneralInput.Close();
            this.LogicalDevice.Close();
            this.logger.Info("ArchiverRestore DataReader Close End");
        }

        #region InputDataListener Methods

        public void CloseDataBlock(FileType fileType, string fileName, System.IO.Stream stream)
        {
            stream.Close();
        }


        public XStream OpenDataBlock(DataBlockOpenParam param, out DataBlockOpenOutParam outParam)
        {
            this.logger.Debug("Archiver Restore Data Reader Open Data Block Begin Debug");
            outParam = new DataBlockOpenOutParam();
            String dataVolume = this.archiverRestoreJob.DataVolume;

            var fileNameGenerator = this.FileNameGeneratorFactory.GetFileNameGenerator();
            outParam.FileName = fileNameGenerator.Generate(new FileNameParameter(param));
            outParam.FileSize = this.DataBlockGetSize(param.FileType, outParam.FileName, dataVolume);
            this.converter.SetFileSize(param.FileType, outParam.FileSize, this.LogicalDevice.IsSupportAutoChangeDataBlock);
            this.logger.Info($"ArchiverRestoreDataReaderDataBlockOpeningBegin,Path.Combine(dataVolume, outParam.FileName):{Path.Combine(dataVolume.LogBase64(), outParam.FileName.LogBase64())},outParam.FileSize:{outParam.FileSize}");
            StorageInfo info = this.converter.FormNames(param.FileType, dataVolume, outParam.FileName);

            if (BLOBMappings != null && BLOBMappings.ContainsKey(Path.Combine(info.HighName, info.LowName)))
            {
                var mapped = BLOBMappings[Path.Combine(info.HighName, info.LowName)].MappedBlobInfo;
                logger.Info($"Mapped from [{info.HighPlusLowName.LogBase64()}] to [{mapped.HighPlusLowName.LogBase64()}].");
                info.HighName = mapped.HighName;
                info.LowName = mapped.LowName;
            }
            return this.LogicalDevice.OpenStream(info, FileMode.Open);
        }
        
        private string GetSubsubJobIdFromFileName(string fileName)
        {
            int index = 0;
            if (fileName.Contains("_content_"))
            {
                index = fileName.IndexOf("_content_", StringComparison.OrdinalIgnoreCase);
            }
            else if (fileName.Contains("_meta_"))
            {
                index = fileName.IndexOf("_meta_", StringComparison.OrdinalIgnoreCase);
            }
            if (index > 0)
            {
                return fileName.Substring(0, index);
            }
            return fileName;
        }
        public static LogicalDeviceDto ConvertStorageDeviceDtoToLogicalDeviceDto(StorageDeviceDto storageDevice)
        {
            var physical = new PhysicalDeviceDto()
            {
                Id = storageDevice.Id,
                ConnectionString = storageDevice.ConnectionString,
                ModifyTime = storageDevice.ModifyTime,
                Type = storageDevice.Type,
                IsSystemStorage = storageDevice.Id == RecordsConstants.AVEPOINT_DEFAULT_STORAGEID || storageDevice.IsSystemStorage
            };

            var logical = new LogicalDeviceDto();
            logical.Name = storageDevice.Name;
            logical.PhysicalDrives = new List<PhysicalDeviceDto>
            {
                physical
            };
            return logical;
        }
        private AbstractXSystem ValidStorage(AbstractXSystem storage)
        {
            if (string.IsNullOrEmpty(storage.XriString))
            {
                var storageLibrary = storage as XLibrary;
                if (storageLibrary == null)
                {
                    logger.Error($"Index storage system not a XLibrary.");
                }
                storage = storageLibrary?.SubSystems?.FirstOrDefault() as AbstractXSystem;
                if (storage == null)
                {
                    logger.Error($"Index storage system's connection string is null.");
                }
            }
            return storage;
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
                if (fileInfo != null)
                {
                    blockSize = fileInfo.FileSize;
                    this.blockSizeMap.Add(Path.Combine(dataVolume, fileName).ToLower(), blockSize);
                }
            }
            return blockSize;
        }

        #endregion InputDataListener Methods


    }
}