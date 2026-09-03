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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Service.DomainModel.DocAve6x;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Media.Common.ClassicStorageApi;
using RAExportCommon;
using Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.DeleteArchivedData.Archived
{
    public class RMArchivedDataBlockManager
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMArchivedDataBlockManager));

        private static readonly Dictionary<string, RMArchivedFileLevelDataBlockOperator> s_operators = [];

        public static RMArchivedFileLevelDataBlockOperator GetFileLevelDataOperator(string storageId)
        {
            if (!s_operators.TryGetValue(storageId, out var dataOperator))
            {
                s_logger.Info($"The storage [{storageId}] operator no found in cache. Init it.");
                dataOperator = new RMArchivedFileLevelDataBlockOperator(storageId);
                s_operators[storageId] = dataOperator;
            }

            return dataOperator;
        }

        public static void CloseOperators()
        {
            foreach (var entry in s_operators)
            {
                entry.Value.Close();
            }

            s_operators.Clear();
        }
    }

    public class RMArchivedFileLevelDataBlockOperator
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMArchivedFileLevelDataBlockOperator));

        private readonly IStorageDeviceService _storageDeviceService = PlatformWindsorManager.GetService<IStorageDeviceService>();

        private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private readonly IRMStorageDeviceInfoDao _storageDeviceDao = PlatformWindsorManager.GetService<IRMStorageDeviceInfoDao>();

        private readonly ArchiverVolumeGenerator _volumeGenerator = new();

        private readonly string _storageId;

        private IXSystem _dataStorageDevice;

        private bool _isLorealSoftDelete;

        private BlobContainerClient _sourceContainerClient;

        public RMArchivedFileLevelDataBlockOperator(string storageId)
        {
            _storageId = storageId;
            Open();
            _isLorealSoftDelete = IsEnabledRealDelete();
        }

        private void Open()
        {
            var storageDeviceDto = _storageDeviceService.GetStorageDeviceById(_storageId, needDecryptSecert: true);
            var dataDeviceDto = new LogicalDeviceDto
            {
                PhysicalDrives = new()
                {
                    new PhysicalDeviceDto()
                    {
                        Id = storageDeviceDto.Id,
                        ConnectionString = storageDeviceDto.ConnectionString,
                        ModifyTime = storageDeviceDto.ModifyTime,
                        Type = storageDeviceDto.Type,
                    }
                }
            };
            //_dataStorageDevice = XFactoryCommon.InstanceLibrary(dataDeviceDto.ToXRIS());
            _dataStorageDevice = XFactory.InstanceSystem(dataDeviceDto.GetXRIS(PhysicalDeviceUsage.Data)[0]);
            _dataStorageDevice.Open();
            _logger.Info($"The storage [{_storageId}] has been opened.");
        }

        public bool TryDeleteDataBlockIfExists(ArchiverBasicIndex item, out long dataSize)
        {
            dataSize = 0;
            try
            {
                if (item.ContentLength == 0)
                {
                    _logger.Warn($"[DataBlockZeroSize] The site [{item.SitePath}] item [{item.PathMD5}] data block size is zero. Skipped it.");
                    return true;
                }

                var dataVolume = _volumeGenerator.GenerateDataVolume(new()
                {
                    FarmName = "",
                    SiteCollectionUrl = item.SitePath,
                });
                var dataFileName = (item.DuplicateStatus > 0 ? item.DedupSourceFileJobId : item.JobId) + "_content_" + item.ContentDataFileNumber + ".dat";
                var storageInfo = new StorageInfo(dataVolume, dataFileName);
                var exists = _dataStorageDevice.FileExists(storageInfo);
                if(!exists)
                {
                    _logger.Error($"The site [{item.SitePath}] item [{item.PathMD5}] data block not found.");
                    return true;
                }

                var deleteRes = _dataStorageDevice.DeleteFile(storageInfo);
                ChangeLorealBlobFromPreviousVersionToDelete(storageInfo);
                if (!deleteRes.IsDeleted)
                {
                    _logger.Info($"The site [{item.SitePath}] item [{item.PathMD5}] data delete failed. Exception type [{deleteRes.DeleteExceptionType}]. Error message: [{deleteRes.Message}].");
                    return false;
                }

                _logger.Info($"The site [{item.SitePath}] item [{item.PathMD5}] data has been deleted. Data size [{deleteRes.DeletedFileSize}].");

                dataSize = deleteRes.DeletedFileSize;
                return true;
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while delete data, site [{item.SitePath}] item [{item.PathMD5}]. Error: {e}");
                return false;
            }
        }

        private void ChangeLorealBlobFromPreviousVersionToDelete(StorageInfo info)
        {
            if (_isLorealSoftDelete)
            {
                var source = _dataStorageDevice as AbstractXSystem;
                if (source != null && source.StorageType == XStorageType.Azure)
                {
                    if (_sourceContainerClient == null)
                    {
                        _sourceContainerClient = RAStorageUtil.GetBlobContainerClientByStorageXRI(source.ConnectionString);
                    }
                    string blobName = info.HighPlusLowName.Replace(@"\", @"/");
                    _logger.Info($"ChangeLorealBlobFromPreviousVersionToDelete.Blob name: {info.HighPlusLowName}.blobName:{blobName}.");
                    var blobClient = _sourceContainerClient.GetBlobClient(blobName);
                    // List all versions of the blob
                    List<string> blobVersions = new List<string>();
                    foreach (BlobItem blobItem in _sourceContainerClient.GetBlobs(BlobTraits.None, BlobStates.Version, prefix: blobName, default))
                    {
                        _logger.Info($"ChangeLorealBlobFromPreviousVersionToDelete.Blob name: {blobItem.Name}, Version ID: {blobItem.VersionId}.Version Delete:{blobItem.Deleted}.");
                        blobVersions.Add(blobItem.VersionId);
                    }
                    foreach (var blobVersion in blobVersions)
                    {
                        blobClient.WithVersion(blobVersion).DeleteIfExistsAsync();
                        _logger.Info($"ChangeLorealBlobFromPreviousVersionToDelete.Success delete blob version.Version ID: {blobVersion}.");
                    }
                }
                else
                {
                    throw new FileNotFoundException(String.Format("3An error occurred in getting file {0}.", info.HighPlusLowName));
                }
            }
        }

        private bool IsEnabledRealDelete()
        {
            var realDeleteRetentionDatas = _keyValueDao.GetValueByKey("RealDeleteAzureRetentionDatas");
            if (realDeleteRetentionDatas != null)
            {
                bool result;
                if (bool.TryParse(realDeleteRetentionDatas.Value, out result) && result)
                {
                    if (string.Equals(_storageId, RecordsConstants.AVEPOINT_DEFAULT_STORAGEID, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.Warn("this storage is avepoint storage that can not delete datas when the action is soft delete");
                        return false;
                    }
                    else
                    {
                        var storageInfo = _storageDeviceDao.GetStorageDevicesById(new Guid(_storageId));
                        if (storageInfo != null && storageInfo.Type == (int)StorageDeviceType.CloudAzure)
                        {
                            _logger.Info($"this storage is azure storage and soft delete,will real delete datas");
                            return true;
                        }
                        else
                        {
                            _logger.Info($"this storage is not azure storage,so skip delete datas when soft delete,storage id:{_storageId},type:{storageInfo?.Type}");
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public void Close()
        {
            _dataStorageDevice.Close();
            _logger.Info($"The storage [{_storageId}] has been closed.");
        }
    }
}
