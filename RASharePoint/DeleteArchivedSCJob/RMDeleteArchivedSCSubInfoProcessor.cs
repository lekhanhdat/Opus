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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Media.Common;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Service.DomainModel.DocAve6x;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.SharePoint.ArchiverCommon;
using Merged18NResources.MediaServiceArchiverBackup;
using Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.DeleteArchivedSCJob
{
    public class RMDeleteArchivedSCSubInfoProcessor
    {
        private static readonly AveLogger _logger = AveLogger.GetInstance(typeof(RMDeleteArchivedSCSubInfoProcessor));

        private readonly IStorageDeviceService _storageDeviceService = PlatformWindsorManager.GetService<IStorageDeviceService>();
        private readonly IArchiverIndexSubInfoDao _archiveIndexSubInfoDao = PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();

        // class global field
        private RMDeleteArchivedSCJobReportManager _reportManager;
        private RMDeleteArchivedSCStubManager _stubManager;
        private RMDeleteArchivedSCFullTextIndexManager _fullTextIndexManager;

        // field from Init
        private string _siteCollectionUrl;
        private IXSystem _indexLogicalDevice;
        private IDeleteArchivedSCIndexService _deleteArchivedSCIndexService;
        private string _indexVolume = string.Empty;
        private string _dataVolume = string.Empty;

        // internal managed fields
        private Dictionary<string, StorageDeviceDto> _storageDeviceCache = [];
        private readonly int _batchIndexSize = 2000;
        private IXSystem _dataLogicalDevice;

        public RMDeleteArchivedSCSubInfoProcessor(
            RMDeleteArchivedSCJobReportManager reportManager,
            RMDeleteArchivedSCStubManager stubManager,
            RMDeleteArchivedSCFullTextIndexManager fullTextIndexManager
            )
        {
            _reportManager = reportManager;
            _stubManager = stubManager;
            _fullTextIndexManager = fullTextIndexManager;
        }

        public async Task InitAsync(string siteCollectionUrl, IXSystem indexLogicalDevice, IDeleteArchivedSCIndexService deleteArchivedSCIndexService)
        {
            _siteCollectionUrl = siteCollectionUrl;
            _indexLogicalDevice = indexLogicalDevice;
            _deleteArchivedSCIndexService = deleteArchivedSCIndexService;

            var volumeGenerator = new ArchiverVolumeGenerator();
            var VolumeParam = new VolumeParameter() { FarmName = string.Empty, SiteCollectionUrl = _siteCollectionUrl, };
            _indexVolume = volumeGenerator.GenerateIndexVolume(VolumeParam);
            _dataVolume = volumeGenerator.GenerateDataVolume(VolumeParam);
        }

        public async Task<(bool hasFailed, long totalMediaDataSize)> ProcessAsync(bool isFileLevelMode, string archivedJobId)
        {
            var subInfoes = _archiveIndexSubInfoDao.GetSubInfoesBySubJobId(archivedJobId);
            if (subInfoes.IsNullOrEmpty())
            {
                _logger.Info($"No sub info found for archived job: {archivedJobId}.");
                return (false, 0);
            }

            _logger.Info($"Found {subInfoes.Count} sub info for archived job: {archivedJobId}, isFileLevelMode: {isFileLevelMode}.");
            _reportManager.IncreaseBase(subInfoes.Count);

            StorageDeleteResult deleteDataResult = new();
            StorageDeleteResult deleteIndexResult = new();
            var totalDeletedMediaDataSize = 0L;
            var hasFailed = false;
            foreach (var subInfo in subInfoes)
            {
                try
                {
                    _logger.Info($"Start to process JobId: {subInfo.JobId}, StorageId: {subInfo.StorageInfo} , CurrentStorageId: {subInfo.CurrentStorageId}.");
                    var isFailed = await ProcessBySubInfoes(subInfo, deleteDataResult, deleteIndexResult, isFileLevelMode);
                    hasFailed |= isFailed;
                    if (!isFailed)
                    {
                        totalDeletedMediaDataSize += subInfo.MediaDataSize;
                        _logger.Info($"Finished processing JobId: {subInfo.JobId}, totalDeletedMediaDataSize: {subInfo.MediaDataSize}.");
                    }
                    else
                    {
                        _logger.Warn($"Finished processing JobId: {subInfo.JobId} is Failed. MediaDataSize: {subInfo.MediaDataSize} will not be included in total size.");
                    }
                    _reportManager.IncreaseProgress();
                }
                catch (Exception e)
                {
                    _logger.Error($"Error occurred while processing sub info for JobId: {subInfo.JobId}, StorageId: {subInfo.StorageInfo}. {e}");
                    _reportManager.IncreaseProgress();
                }
            }

            _logger.Info($"Finished processing all sub info for archived job: {archivedJobId}. hasFailed: {hasFailed}, totalDeletedMediaDataSize: {totalDeletedMediaDataSize}.");
            return (hasFailed, totalDeletedMediaDataSize);
        }

        // file level mode: query index record => delete file => create job detail report one by one
        // block level mode: find all content files on device to delete => query index record to create job detail report
        public async Task<bool> ProcessBySubInfoes(ArchiverIndexSubInfoContract subInfo, StorageDeleteResult deleteDataResult, StorageDeleteResult deleteIndexResult, bool isFileLevelMode)
        {
            var hasFailed = false;
            var contentFilePrefix = $"{subInfo.JobId}_content_";
            var metaFilePrefix = $"{subInfo.JobId}_meta_";
            try
            {
                if (!_storageDeviceCache.TryGetValue(subInfo.CurrentStorageId, out StorageDeviceDto value))
                {
                    value = _storageDeviceService.GetStorageDeviceById(subInfo.CurrentStorageId, needDecryptSecert: true);
                    _logger.Info($"Load storage device from service for storage id: {subInfo.CurrentStorageId}, storage name: {value.Name}, type: {(StorageDeviceType)value.Type}.");
                    _storageDeviceCache.Add(subInfo.CurrentStorageId, value);
                }

                hasFailed = !HandleDAOMigratedStorageInfo(subInfo);
                if (hasFailed)
                {
                    _logger.Warn($"Failed to handle DAO migrated storage info for JobId: {subInfo.JobId}, StorageId: {subInfo.StorageInfo}. Will skip processing this sub info to avoid wrong deletion.");
                    return hasFailed;
                }

                StorageInfo subJobIndexStorageInfo = XConvert.FromNames(_indexVolume, subInfo.JobId + "_" + ServiceConstants.IndexDBName);
                var indexFile = _indexLogicalDevice.OpenFile(subJobIndexStorageInfo);
                if (indexFile == null || !indexFile.Exists)
                {
                    _logger.Warn($"Sub job index file does not exist for JobId: {subInfo.JobId}, StorageId: {subInfo.StorageInfo}. StorageInfo: {subJobIndexStorageInfo.HighPlusLowName}");
                }
                else
                {
                    _logger.Info($"Sub job index file exists for JobId: {subInfo.JobId}, StorageId: {subInfo.StorageInfo}. StorageInfo: {subJobIndexStorageInfo.HighPlusLowName}, Size: {indexFile.FileSize}");
                }

                var LogicalDevice = ArchiverTypeConvert.ConvertStorageDeviceDtoToLogicalDeviceDto(value);
                _dataLogicalDevice = XFactory.InstanceSystem(LogicalDevice.GetXRIS(PhysicalDeviceUsage.Data)[0]);
                _dataLogicalDevice.Open();

                var tempDeletedDataSize = 0L; // for the case delete failed in block level mode
                if (!isFileLevelMode)
                {
                    // block level mode
                    var tempFileList = this._dataLogicalDevice.ListFiles(XConvert.FromNames(_dataVolume, null));
                    var fileList = tempFileList.FindAll(file => file.LowName.StartsWith(contentFilePrefix, StringComparison.OrdinalIgnoreCase));
                    _logger.Info($"Need delete blobs count : {fileList.Count}, prefix: {contentFilePrefix}");
                    fileList.ForEach(item =>
                    {
                        var info = XConvert.FromNames(item.HighName, item.Name);
                        try
                        {
                            _logger.Info($"Start to delete device content: {info.HighPlusLowName}. SubSubJobId:{subInfo.JobId}.");
                            deleteDataResult = this._dataLogicalDevice.DeleteFile(info);
                            //ChangeLorealBlobFromPreviousVersionToDelete(info);
                            if (deleteDataResult.IsDeleted)
                            {
                                var tempSize = deleteDataResult.DeletedFileSize > 0
                                    ? deleteDataResult.DeletedFileSize
                                    : item.FileSize > 0 ? item.FileSize : item.Length;
                                tempDeletedDataSize += Math.Max(tempSize, 0);
                            }
                        }
                        catch (Exception ex)
                        {
                            hasFailed = true;
                            _logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceWarn, info.LowName, ex.ToString());
                            _reportManager.AddFailedDetail(info.HighPlusLowName, subInfo.JobId, item.FileSize, LogicalDevice.Name, "StorageOptimization.Service_D1CF4044-9105-4506-A7A2-3C2AB7E36EBB");
                        }
                    });

                    if (hasFailed)
                    {
                        // if delete content file failed, will not process index record
                        _logger.Warn($"Delete content file failed for JobId: {subInfo.JobId}, CurrentStorageId: {subInfo.CurrentStorageId}. Will not process index record and delete sub info. DeletedDataSize: {tempDeletedDataSize}.");
                        return hasFailed;
                    }
                }

                var pageOffset = 0;
                while (true)
                {
                    var searchResult = _deleteArchivedSCIndexService.GetDeletingIndexesBySubInfo(subInfo.StorageInfo, subInfo.JobId, _batchIndexSize, pageOffset);
                    if (searchResult == null || searchResult.Count == 0)
                    {
                        _logger.Warn("No search results found after search.");
                        break;
                    }

                    _logger.Info($"Found files with jobId: {subInfo.JobId}, storageId: {subInfo.StorageInfo}. Count:{searchResult.Count}, offSet: {pageOffset}");
                    pageOffset += searchResult.Count;

                    if (!isFileLevelMode)
                    {
                        // block level mode
                        foreach (var basicIndex in searchResult)
                        {
                            await DeleteArchivedAsync(basicIndex);
                            _reportManager.AddSucceedDetail(_reportManager.GetFullPath(basicIndex.ExtraInfo, basicIndex.Url), subInfo.JobId, basicIndex.ContentLength, LogicalDevice.Name);
                        }
                    }
                    else
                    {
                        // file level mode
                        foreach (var basicIndex in searchResult)
                        {
                            var info = XConvert.FromNames(_dataVolume, contentFilePrefix + basicIndex.ContentDataFileNumber + ".dat");
                            _logger.Info($"Start to delete device content: {info.HighPlusLowName}. ModifiedTime:{new DateTime(basicIndex.ModifyTime)}. SubSubJobId:{basicIndex.JobId}.");
                            try
                            {
                                deleteDataResult = this._dataLogicalDevice.DeleteFile(info);

                                var delSize = Math.Max(deleteDataResult.DeletedFileSize, 0);
                                if (delSize == 0)
                                {
                                    delSize = basicIndex.ContentLength;
                                }

                                await DeleteArchivedAsync(basicIndex);
                                _reportManager.AddSucceedDetail(_reportManager.GetFullPath(basicIndex.ExtraInfo, basicIndex.Url), subInfo.JobId, delSize, LogicalDevice.Name);
                            }
                            catch (Exception ex)
                            {
                                hasFailed = true;
                                _logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceWarn, info.LowName, ex.ToString());
                                _reportManager.AddFailedDetail(_reportManager.GetFullPath(basicIndex.ExtraInfo, basicIndex.Url), subInfo.JobId, basicIndex.ContentLength, LogicalDevice.Name, "StorageOptimization.Service_D1CF4044-9105-4506-A7A2-3C2AB7E36EBB");
                            }
                        }
                    }
                }

                _logger.Info($"Finished deleting content files for JobId: {subInfo.JobId}, StorageId: {subInfo.StorageInfo}. DeletedDataSize: {tempDeletedDataSize}, hasFailed: {hasFailed}. Start to delete meta blocks, sub job index file and sub info record.");

                if (!hasFailed)
                {
                    hasFailed = !DeleteMetaBlocks(subInfo.JobId, metaFilePrefix, LogicalDevice.Name);
                }

                if (!hasFailed && indexFile != null && indexFile.Exists)
                {
                    hasFailed = !DeleteSubJobIndexFile(subInfo, deleteIndexResult, subJobIndexStorageInfo);
                }

                if (!hasFailed)
                {
                    _logger.Info($"Successfully deleted all data for JobId: {subInfo.JobId}, StorageId: {subInfo.StorageInfo}, CurrentStorageId: {subInfo.CurrentStorageId}. MediaDataSize: {subInfo.MediaDataSize}. Start to delete sub info record in database.");
                    _archiveIndexSubInfoDao.DeleteByKey(subInfo.Id);
                }
            }
            catch (Exception e)
            {
                hasFailed = true;
                _logger.Error($"Error occurred while processing sub info: {subInfo.Id}, JobId: {subInfo.JobId}, CurrentStorageId: {subInfo.CurrentStorageId}. {e}");
            }
            finally
            {
                _dataLogicalDevice.Close();
            }

            return hasFailed;
        }

        private bool HandleDAOMigratedStorageInfo(ArchiverIndexSubInfoContract subInfo)
        {
            // JobId 如果 AR 开头的，表示是从DAO migration 过来的数据
            if (subInfo.JobId.StartsWith("AR") || (subInfo.JobId.StartsWith("EA") && subInfo.DAOMigrated))
            {
                // DAO migrated 的备份数据，需要用DAO 的StoragePolicyId去级联删除，所以在这替换成 DAOStoragePolicyId
                if (!_storageDeviceCache.TryGetValue(subInfo.StorageInfo, out StorageDeviceDto daoStorage))
                {
                    daoStorage = _storageDeviceService.GetStorageDeviceById(subInfo.StorageInfo, needDecryptSecert: true);
                    _logger.Info($"Load dao storage device from service for storage id: {subInfo.StorageInfo}, storage name: {daoStorage.Name}, type: {(StorageDeviceType)daoStorage.Type}, DAOStoragePolicyId: {daoStorage.DAOStoragePolicyId}.");
                    _storageDeviceCache.Add(subInfo.StorageInfo, daoStorage);
                }

                if (string.IsNullOrEmpty(daoStorage.DAOStoragePolicyId))
                {
                    _logger.Error($"SOCan't find DAOStoragePolicyId from the archiver sub index's storage. SubInfoId: {subInfo.Id}, storage id: {subInfo.StorageInfo}, JobId: {subInfo.JobId}");
                    return false; // if can't find DAOStoragePolicyId for DAO job, return failed to avoid wrong deletion
                }
                else
                {
                    _logger.Info($"Replace storage info with DAOStoragePolicyId for JobId: {subInfo.JobId}. Original StorageInfo: {subInfo.StorageInfo}, DAOStoragePolicyId: {daoStorage.DAOStoragePolicyId}.");
                    subInfo.StorageInfo = daoStorage.DAOStoragePolicyId;
                    return true;
                }
            }

            return true;
        }

        private async Task DeleteArchivedAsync(ArchiverBasicIndex basicIndex)
        {
            try
            {
                await _stubManager.ProcessDeleteStub(basicIndex);
            }
            catch (Exception e)
            {
                _logger.Error($"Error occurred while deleting stub for index: {basicIndex.Id}, JobId: {basicIndex.JobId}. {e}");
            }

            try
            {
                await _fullTextIndexManager.DeleteAsync(basicIndex);
            }
            catch (Exception e)
            {
                _logger.Error($"Error occurred while deleting full text index for index: {basicIndex.Id}, JobId: {basicIndex.JobId}. {e}");
            }
        }

        private bool DeleteMetaBlocks(string jobId, string metaFilePrefix, string storageName)
        {
            var allSucceeded = true;
            try
            {

                var tempFileList = this._dataLogicalDevice.ListFiles(XConvert.FromNames(_dataVolume, null));
                var fileList = tempFileList.FindAll(file => file.LowName.StartsWith(metaFilePrefix, StringComparison.OrdinalIgnoreCase));
                _logger.Info($"Need delete meta blocks count : {fileList.Count}, storageName: {storageName}");
                StorageDeleteResult deleteDataResult = new();
                foreach (var item in fileList)
                {
                    var info = XConvert.FromNames(item.HighName, item.Name);
                    //_logger.Info($"Start to delete device meta: {info.HighPlusLowName}. SubSubJobId:{jobId}.");
                    try
                    {
                        deleteDataResult = this._dataLogicalDevice.DeleteFile(info);
                        var tempSize = deleteDataResult.DeletedFileSize;
                        if (deleteDataResult.IsDeleted && tempSize <= 0)
                        {
                            // some storage does not return the DeletedFileSize
                            tempSize = item.FileSize > 0 ? item.FileSize : Math.Max(item.Length, 0);
                        }
                        _logger.Info($"Finish deleting meta file: {info.HighPlusLowName}, SubSubJobId:{jobId}, DeletedFileSize: {tempSize} bytes.");
                        //_reportManager.AddSucceedDetail(info.HighPlusLowName, jobId, tempSize, storageName);
                    }
                    catch (Exception ex)
                    {
                        allSucceeded = false;
                        _logger.Error(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceWarn, info.LowName, ex.ToString());
                        _reportManager.AddFailedDetail(info.HighPlusLowName, jobId, deleteDataResult.DeletedFileSize, storageName, "StorageOptimization.Service_D1CF4044-9105-4506-A7A2-3C2AB7E36EBB");

                    }
                }
                return allSucceeded;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error occurred while deleting meta blocks: {jobId} in storage: {storageName}. {ex}");
                return false;
            }
        }

        private bool DeleteSubJobIndexFile(ArchiverIndexSubInfoContract subInfo, StorageDeleteResult deleteIndexResult, StorageInfo subJobIndexStorageInfo)
        {
            //StorageInfo storageInfo = XConvert.FromNames(_indexVolume, subInfo.JobId + "_" + ServiceConstants.IndexDBName);
            try
            {
                deleteIndexResult = _indexLogicalDevice.DeleteFile(subJobIndexStorageInfo);
                _logger.Info($"Finish deleting sub job index file {subJobIndexStorageInfo.HighPlusLowName} for job: {subInfo.JobId}. IsDeleted: {deleteIndexResult.IsDeleted}, DeletedFileSize: {deleteIndexResult.DeletedFileSize} bytes.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Warn(MediaServiceArchiverBackupResource.RetentionServiceDeleteDataFromDeviceWarn, subJobIndexStorageInfo.LowName, ex.ToString());
                return false;
            }

        }
    }
}
