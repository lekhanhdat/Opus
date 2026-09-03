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
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Media.Core.IO.Input;
using AvePoint.Media.Core.IO;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Service.Services.ArchivedFullTextIndex.Work.Util;
using Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.Media.Service.DomainModel;
using Media.Common.ClassicStorageApi;
using AvePoint.Media.Service.DomainModel.DocAve6x;
using System.IO;
using Storage.Cloud.Azure;
using AvePoint.RA.Contract.ArchivedFullTextIndex;
using AvePoint.GCommon.Utility;
using Util.AI.Text.Extractor;
using SkiaSharp;
using ZXing;
using System.Threading;
using LiteDB;
using AvePoint.RA.DB.Dao;
using Newtonsoft.Json;
using AvePoint.RA.Common.RAProcess.Extractor;
using AvePoint.Metadata;

namespace AvePoint.RA.Service.Services.ArchivedFullTextIndex.Work
{
    public class RMArchivedFullTextIndexDataManager : IInputDataListener
    {
        private const int S_READ_ITEM_PAGE_SIZE = 1000;

        private const string S_AFTI_LIMIT_CONFIGURATION = "AFTI_LIMIT_CONFIGURATION";

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMArchivedFullTextIndexDataManager));

        private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private readonly IStorageDeviceService _storageDeviceService = PlatformWindsorManager.GetService<IStorageDeviceService>();

        private readonly IArchiverIndexSubInfoDao _archiverIndexSubInfoDao = PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();

        private readonly Dictionary<int, long> _blockSizeCache = [];

        private readonly ArchiverVolumeGenerator _volumeGenerator;

        private readonly Extractor _extractor;

        private readonly RMArchivedFullTextIndexDBManager _indexDBManager;

        private readonly RMArchivedFullTextIndexSiteManager _siteManager;

        private readonly RMArchivedFullTextIndexSyncJobManager _syncJobManager;

        private readonly ArchiverIndexSubInfoContract _jobInfo;

        private readonly DataEncryptionInfo _encryptionInfo;

        private bool _noLimit;

        private long _fileSizeLimit = 50 * 1024 * 1024;

        private int _letterCountLimit = 1_000_000;

        private int _threadCountLimit = 10;

        private int _extractTimeoutMinutes = 6;

        private string _fileFolderPath;

        private IXSystem _dataStorageDevice;

        private string _dataStorageVolume;

        private IMediaGeneralInputStream _dataStorageInputer;

        private IXConverter _dataStorageConverter;

        private LogicalDeviceDto _logicalDevice;

        private bool _isArchiveTier;

        public RMArchivedFullTextIndexDataManager(
            RMArchivedFullTextIndexDBManager indexDBManager,
            RMArchivedFullTextIndexSiteManager siteManager,
            RMArchivedFullTextIndexSyncJobManager syncJobManager,
            ArchiverIndexSubInfoContract jobInfo,
            LogicalDeviceDto logicalDevice)
        {
            _volumeGenerator = new();
            _extractor = new();
            _indexDBManager = indexDBManager;
            _siteManager = siteManager;
            _syncJobManager = syncJobManager;
            _jobInfo = jobInfo;
            _encryptionInfo = RMArchivedFullTextIndexSecurityUtil.GetEncryptionInfo(jobInfo);
            _logicalDevice = logicalDevice;
        }

        public void Open()
        {
            InitConfiguration();
            InitStorage();

            if (_logicalDevice == null)
            {
                var storageDeviceDto = _storageDeviceService.GetStorageDeviceById(string.IsNullOrEmpty(_jobInfo.CurrentStorageId) ? _jobInfo.StorageInfo : _jobInfo.CurrentStorageId, needDecryptSecert: true);
                var _logicalDevice = new LogicalDeviceDto
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
            }

            _dataStorageVolume = _volumeGenerator.GenerateDataVolume(new()
            {
                FarmName = "",
                SiteCollectionUrl = _siteManager.SiteUrl
            });
            _dataStorageDevice = XFactoryCommon.InstanceLibrary(_logicalDevice.ToXRIS());
            _dataStorageDevice.Open();
            _logger.Info($"The site [{_siteManager.SiteUrl}] storage device of data block has been open.");

            _dataStorageInputer = new FormatedInputStream(new()
            {
                DataListener = this,
                IsSupportAutoChangeDataBlock = (_dataStorageDevice as Media.ClassicStorage.AbstractXSystem)?.IsSupportAutoChangeDataBlock == true
            });
            _dataStorageConverter = (IXConverter)_dataStorageInputer;

            _dataStorageInputer = new EncryptedFormatedInputStream(_dataStorageInputer);
            _dataStorageInputer = new CompressedFormatedInputStream(_dataStorageInputer);
            _dataStorageInputer.Open();

            var encryptionInfoDic = new Dictionary<string, DataEncryptionInfo>();
            if (_encryptionInfo != null)
            {
                encryptionInfoDic.Add(_jobInfo.JobId, _encryptionInfo);
            }
            _dataStorageInputer.SetEncryptionInfos(encryptionInfoDic, this.GetEncryptionInfoAsync);

            _isArchiveTier = CheckDataBlockIsArchiverTier();

            _logger.Info($"The site [{_siteManager.SiteUrl}] data block storage inputer has been open.");
        }

        private DataEncryptionInfo GetEncryptionInfoAsync(string backupSubJobId)
        {
            var (_, subIndexInfo) = _archiverIndexSubInfoDao.TryGetSubInfoByJobIdAsync(backupSubJobId).GetAwaiter().GetResult();
            if (subIndexInfo != null)
            {
                return RMArchivedFullTextIndexSecurityUtil.GetEncryptionInfo(subIndexInfo);
            }
            else
            {
                _logger.Warn($"Could not get encryption info by : {backupSubJobId}");
                return null;
            }
        }

        private void InitConfiguration()
        {
            try
            {
                var setting = _keyValueDao.GetValueByKey(S_AFTI_LIMIT_CONFIGURATION);
                if (setting != null && !string.IsNullOrWhiteSpace(setting.Value))
                {
                    var obj = JsonConvert.DeserializeObject<Dictionary<string, object>>(setting.Value);
                    if (obj != null
                        && obj.TryGetValue("noLimit", out var rawNoLimit)
                        && bool.TryParse(rawNoLimit?.ToString(), out var noLimit))
                    {
                        _noLimit = noLimit;
                    }

                    if (!_noLimit
                        && obj != null
                        && obj.TryGetValue("file_size_limit", out var rawFileSizeLimit)
                        && long.TryParse(rawFileSizeLimit?.ToString(), out var fileSizeLimit))
                    {
                        _fileSizeLimit = fileSizeLimit;
                    }

                    if (!_noLimit
                        && obj != null
                        && obj.TryGetValue("letter_count_limit", out var rawLetterCountLimit)
                        && int.TryParse(rawLetterCountLimit?.ToString(), out var letterCountLimit))
                    {
                        _letterCountLimit = letterCountLimit;
                    }

                    if (obj != null
                        && obj.TryGetValue("thread_count_limit", out var rawThreadCount)
                        && int.TryParse(rawThreadCount?.ToString(), out var threadCountLimit))
                    {
                        _threadCountLimit = threadCountLimit;
                    }

                    if (obj != null
                        && obj.TryGetValue("extract_timeout_minutes", out var rawExtractTimeout))
                    {
                        if (int.TryParse(rawExtractTimeout?.ToString(), out var extractTimeoutMinutes)
                            && extractTimeoutMinutes > 0)
                        {
                            _extractTimeoutMinutes = extractTimeoutMinutes;
                        }
                    }
                }

                _logger.Info(
                    $"Archived full text index limits: noLimit [{_noLimit}], " +
                    $"file size limit [{(_noLimit ? "unlimited" : _fileSizeLimit.ToString())}], " +
                    $"letter count limit [{(_noLimit ? "unlimited" : _letterCountLimit.ToString())}], " +
                    $"thread count limit [{_threadCountLimit}], " +
                    $"extract timeout minutes [{_extractTimeoutMinutes}].");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while init configuration. Error: {e}");
            }
        }

        private void InitStorage()
        {
            var eDiscoveryFolderPath = SecurityUtils.SafeCombinePath(Environment.CurrentDirectory, "e_discovery");
            if (!Directory.Exists(eDiscoveryFolderPath))
            {
                Directory.CreateDirectory(eDiscoveryFolderPath);
            }

            var jobFolderPath = SecurityUtils.SafeCombinePath(eDiscoveryFolderPath, _jobInfo.JobId);
            if (!Directory.Exists(jobFolderPath))
            {
                Directory.CreateDirectory(jobFolderPath);
            }

            _fileFolderPath = SecurityUtils.SafeCombinePath(jobFolderPath, "cache_files");
            if (!Directory.Exists(_fileFolderPath))
            {
                Directory.CreateDirectory(_fileFolderPath);
            }
        }

        public async IAsyncEnumerable<RMArchivedFullTextIndexDataInfo> ReadAsync()
        {
            var extractor = new RMArchivedFullTextIndexContentExtractor(
                _letterCountLimit,
                _threadCountLimit,
                _noLimit,
                TimeSpan.FromMinutes(_extractTimeoutMinutes));
            var readTask = ReadItemsAsync(extractor);

            await foreach (var item in extractor.GetAllDataAsync())
            {
                yield return item;
            }

            await readTask;
            var succeed = extractor.GetResult();
            _syncJobManager.IncreseProgress(succeed);
            _siteManager.IncreseProgress(succeed);
        }

        private async Task ReadItemsAsync(RMArchivedFullTextIndexContentExtractor extractor)
        {
            var items = _indexDBManager.Read(_jobInfo.JobId, S_READ_ITEM_PAGE_SIZE);

            foreach(var (item, treeNode) in items)
            {
                try
                {
                    await ProcessItemAsync(item, treeNode, extractor);
                }
                catch (Exception e)
                {
                    _logger.Error($"An error occurred while read and analysis item [{item.Url}]. Error: {e}");
                }
            }

            extractor.SetNoDataNeedsExtract();
            await extractor.CompleteAsync();
        }

        private async Task ProcessItemAsync(
            ArchiverBasicIndex item,
            TreeNode treeNode,
            RMArchivedFullTextIndexContentExtractor extractor)
        {
            var fileName = item.Name.Split(":").First();
            var fileType = Path.GetExtension(fileName).Replace(".", "");
            var dataInfo = BuildDataInfo(item, treeNode, fileType);

            if (_isArchiveTier)
            {
                await extractor.AddDataAsync(dataInfo);
                return;
            }

            if (_extractor.IsSupportType(fileType) && (_noLimit || item.ContentLength <= _fileSizeLimit))
            {
                await HandleExtractableItemAsync(item, fileName, dataInfo, extractor);
                return;
            }

            if (_extractor.IsSupportType(fileType) && !_noLimit && item.ContentLength > _fileSizeLimit)
            {
                _logger.Warn($"The file [{item.Url}] content length [{item.ContentLength}] exceeds [{_fileSizeLimit}] limit. Skip extract content.");
            }

            await HandleMetadataOnlyItemAsync(item, fileName, dataInfo, extractor);
        }

        private RMArchivedFullTextIndexDataInfo BuildDataInfo(ArchiverBasicIndex item, TreeNode treeNode, string fileType)
        {
            return new RMArchivedFullTextIndexDataInfo
            {
                IndexDBUniqueId = item.Id,
                SiteId = _jobInfo.SiteUniqueId,
                Name = item.Name,
                FullPath = item.Url,
                FriendlyFullPath = _indexDBManager.GetFriendlyFullPath(item),
                SiteUrl = item.SitePath,
                FileSize = item.ContentLength,
                ArchiverTime = item.ArchiveTime,
                CreateTime = item.CreateTime,
                ModifiedTime = item.ModifyTime,
                UIVersion = item.Version,
                PathMd5 = item.PathMD5,
                ParentPathMd5 = item.ParentPathMD5,
                Author = item.Author,
                Editor = item.Editor,
                ArchiverJobId = item.JobId,
                NodeLevel = item.Name.Contains(":") ? 256 : 64,
                FileType = fileType,
                TreeNode = SerializerHelper.SerializeByDataContractSerializer(treeNode),
                IsCurrentVersion = !item.Name.Contains(":"),
                AccessTierType = item.FlagExtend,
                TypeInIndex = item.Type,
            };
        }

        private async Task HandleExtractableItemAsync(
            ArchiverBasicIndex item,
            string fileName,
            RMArchivedFullTextIndexDataInfo dataInfo,
            RMArchivedFullTextIndexContentExtractor extractor)
        {
            var (succeed, hasContent, filePath, metadataInfo) = TryReadMetadataAndContent(item, fileName, true);
            dataInfo.MetadataInfo = metadataInfo;
            UpdateProgress(succeed);

            if (succeed && hasContent)
            {
                await extractor.AddNeedExtractDataAsync(dataInfo, filePath);
            }
        }

        private async Task HandleMetadataOnlyItemAsync(
            ArchiverBasicIndex item,
            string fileName,
            RMArchivedFullTextIndexDataInfo dataInfo,
            RMArchivedFullTextIndexContentExtractor extractor)
        {
            var (succeed, _, _, metadataInfo) = TryReadMetadataAndContent(item, fileName, false);
            UpdateProgress(succeed);
            dataInfo.MetadataInfo = metadataInfo;
            await extractor.AddDataAsync(dataInfo);
        }

        private void UpdateProgress(bool succeed)
        {
            _syncJobManager.IncreseProgress(succeed);
            _siteManager.IncreseProgress(succeed);
        }

        public List<RMArchivedFullTextIndexDataInfo> ReadRelateOldItems(RMArchivedFullTextIndexDataInfo dataInfo)
        {
            var dataList = _indexDBManager.ReadRelateOldDataList(dataInfo.IndexDBUniqueId, dataInfo.PathMd5);
            return dataList.ConvertAll(MapToRelatedItem);
        }

        private RMArchivedFullTextIndexDataInfo MapToRelatedItem(ArchiverBasicIndex item)
        {
            return new RMArchivedFullTextIndexDataInfo
            {
                IndexDBUniqueId = item.Id,
                SiteId = _jobInfo.SiteUniqueId,
                Name = item.Name,
                FullPath = item.Url,
                SiteUrl = item.SitePath,
                FileSize = item.ContentLength,
                ArchiverTime = item.ArchiveTime,
                CreateTime = item.CreateTime,
                ModifiedTime = item.ModifyTime,
                UIVersion = item.Version,
                PathMd5 = item.PathMD5,
                ParentPathMd5 = item.ParentPathMD5,
                Author = item.Author,
                Editor = item.Editor,
                ArchiverJobId = item.JobId,
                NodeLevel = item.Name.Contains(":") ? 256 : 64,
                AccessTierType = item.FlagExtend,
                TypeInIndex = item.Type,
            };
        }

        public void ReadEnd()
        {
            _dataStorageDevice?.Dispose();
        }

        public XStream OpenDataBlock(DataBlockOpenParam param, out DataBlockOpenOutParam outParam)
        {
            outParam = new DataBlockOpenOutParam
            {
                FileName = GenerateFileName(param.FileType, param.JobId, param.FileNumber)
            };
            var info = _dataStorageConverter.FormNames(param.FileType, _dataStorageVolume, outParam.FileName);
            var filePathHashCode = Path.Combine(_dataStorageVolume, outParam.FileName).ToLower().GetHashCode();
            if (!_blockSizeCache.ContainsKey(filePathHashCode))
            {
                _blockSizeCache[filePathHashCode] = _dataStorageDevice.OpenFile(info).FileSize;
            }
            outParam.FileSize = _blockSizeCache[filePathHashCode];
            _dataStorageConverter.SetFileSize(param.FileType, outParam.FileSize, (_dataStorageDevice as Media.ClassicStorage.AbstractXSystem)?.IsSupportAutoChangeDataBlock == true);

            info = _dataStorageConverter.FormNames(param.FileType, _dataStorageVolume, outParam.FileName);
            return _dataStorageDevice.OpenStream(info, FileMode.Open);
        }

        public XStream OpenDataBlockForGetVersion(DataBlockOpenParam param)
        {
            var info = new StorageInfo
            {
                LowName = GenerateFileName(param.FileType, param.JobId, param.FileNumber),
                HighName = _dataStorageVolume,
                Offset = 0,
                Length = 4
            };
            return _dataStorageDevice.OpenStream(info, FileMode.Open);
        }

        public void CloseDataBlock(FileType fileType, string fileName, Stream stream)
        {
            stream.Close();
        }

        private bool CheckDataBlockIsArchiverTier()
        {
            if (_indexDBManager.TryGetFirst(_jobInfo.JobId, out var item))
            {
                var fileInfo = _dataStorageDevice.OpenFile(new StorageInfo { HighName = _dataStorageVolume, LowName = GenerateFileName(FileType.Content, _jobInfo.JobId, item.CurrentItemContentDataStartFileNumber) });
                if (fileInfo is AzureCloudInfo azureFile)
                {
                    if (azureFile.FileTierType == AccessTierType.Archive)
                    {
                        _logger.Warn($"The data block of job [{_jobInfo.JobId}] [{_siteManager.SiteUrl}] is archive tier, skipped it.");
                        return true;
                    }
                }
            }

            return false;
        }

        private (bool Succeed, bool HasContent, string FilePath, string metadataInfo) TryReadMetadataAndContent(ArchiverBasicIndex item, string fileName, bool needContent)
        {
            item.IsRestoreToFS = false;
            var metadataInfo = string.Empty;
            var filePath = string.Empty;
            _dataStorageInputer.NextItem(item);
            try
            {
                metadataInfo = ReadMetadataPart1Info();
                filePath = _dataStorageInputer.HasContent && needContent
                    ? ReadContentToFile(fileName)
                    : string.Empty;

                ReadMetadataPart2();

                return (true, !string.IsNullOrWhiteSpace(filePath), filePath, metadataInfo);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while read item [{item.Url}] content. Error: {e}");
                if (e.Message.Contains("Permission is required to open this file"))
                {
                    _logger.Warn($"The item [{item.Url}] has been encrypt by IRM, skip extract content.");
                    return (true, false, string.Empty, metadataInfo);
                }
                return (false, false, string.Empty, "");
            }
            finally
            {
                _dataStorageInputer.EndItem();
            }
        }

        private string ReadMetadataPart1Info()
        {
            if (!_dataStorageInputer.HasMetaDataPart1)
            {
                return string.Empty;
            }

            _dataStorageInputer.BeginRead(FileType.MetaData);

            var contentData = new byte[1048576];
            var byteList = new List<byte>();
            var readLen = 0;
            while ((readLen = _dataStorageInputer.ReadMetaDataPart1(contentData, 0, contentData.Length)) > 0)
            {
                byteList.AddRange(contentData.Take(readLen));
            }

            var metadataInfo = BuildMetadataInfo(byteList);

            _dataStorageInputer.EndRead(FileType.MetaData);
            return metadataInfo;
        }

        private string BuildMetadataInfo(List<byte> byteList)
        {
            var byteArray = byteList.Skip(20).ToArray();
            using var streamReader = new StreamReader(new MemoryStream(byteArray));
            using var metadataReader = new AveMemoryMetadataReader(streamReader);
            var metadata = metadataReader.TryReadMetadata(AveMetadataType.DocData);
            var data = metadata.GetMetadata<Dictionary<string, object>>();
            var needRemovedKeys = new List<string> { "#tp_ContentTypeId", "File_x0020_Type", "#tp_ID", "Created", "Author", "Modified", "Editor", "#tp_ModerationStatus", "#tp_Level", "#tp_IsCurrentVersion", "#AppEditor", "#tp_UIVersion", "#tp_UIVersionString", "#tp_ItemOrder", "#tp_GUID", "GeoLoc", "CountryOrRegion", "State", "City", "Street", "DispName" };
            needRemovedKeys.ForEach(needRemovedKey => data.Remove(needRemovedKey));
            var metadataInfo = string.Empty;
            foreach (var value in data.Values)
            {
                if (value is string strValue && !string.IsNullOrWhiteSpace(strValue))
                {
                    metadataInfo += $"{value} ";
                }
            }

            return metadataInfo;
        }

        private string ReadContentToFile(string fileName)
        {
            var filePath = SecurityUtils.SafeCombinePath(_fileFolderPath, $"{Guid.NewGuid()}_{fileName}");
            using (var fileStream = File.Open(filePath, FileMode.OpenOrCreate))
            {
                var contentData = new byte[1048576];

                _dataStorageInputer.BeginRead(FileType.Content);
                var readLen = 0;
                while ((readLen = _dataStorageInputer.ReadContent(contentData, 0, contentData.Length)) > 0)
                {
                    fileStream.Write(contentData, 0, readLen);
                }

                _dataStorageInputer.EndRead(FileType.Content);
            }

            return filePath;
        }

        private void ReadMetadataPart2()
        {
            if (!_dataStorageInputer.HasMetaDataPart2)
            {
                return;
            }

            _dataStorageInputer.BeginRead(FileType.MetaData);
            var contentData = new byte[1048576];
            var readLen = 0;
            while ((readLen = _dataStorageInputer.ReadMetaDataPart2(contentData, 0, contentData.Length)) > 0)
            {
                //fileStream.Write(contentData, 0, readLen);
            }
            _dataStorageInputer.EndRead(FileType.MetaData);
        }

        private static string GenerateFileName(FileType fileType, string jobId, long fileNumber)
        {
            if (fileType == FileType.Content)
            {
                return jobId + "_content_" + fileNumber + ".dat";
            }
            return jobId + "_meta_" + fileNumber + ".dat";
        }
    }
}
