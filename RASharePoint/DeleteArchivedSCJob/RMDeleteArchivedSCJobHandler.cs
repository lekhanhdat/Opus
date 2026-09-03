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
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Server.ControlPanel;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Common;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Service.DomainModel.DocAve6x;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.DBLocker;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Common;
using Merged18NResources.MediaServiceArchiverBackup;
using RecordsHotfixMaintenanceService;
using Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.DeleteArchivedSCJob
{
    public class RMDeleteArchivedSCJobHandler
    {
        private static readonly AveLogger _logger = AveLogger.GetInstance(typeof(RMDeleteArchivedSCJobHandler));

        private readonly IArchiverSiteMasterIndexDao _archiverSiteMasterIndexDao = PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        private readonly IStorageDeviceService _storageDeviceService = PlatformWindsorManager.GetService<IStorageDeviceService>();

        private StorageDeviceDto _indexDeviceDto;
        private IXSystem _indexLogicalDevice;
        public IStorageDeviceManager StorageDeviceManager { get; set; }
        public ICacheService CacheManager { get; set; }
        private IMCacheSettingService _cacheSettingService;
        public IMCacheSettingService CacheSettingService
        {
            get
            {
                if (_cacheSettingService == null)
                {
                    _cacheSettingService = new CacheSettingService();
                    return _cacheSettingService;
                }
                else
                {
                    return _cacheSettingService;
                }
            }
        }
        private ArchiverIndexService _indexService;
        public IIndexService<ArchiverIndexServiceOpenParameter> IndexService
        {
            get
            {
                if (_indexService == null)
                {
                    _indexService = new ArchiverIndexService()
                    {
                        IndexProcessor = new IndexProcessor<ArchiverIndexProcessorParameter>(),
                        IndexSynchronizer = new IndexDatabaseSynchronizer()
                    };
                    return _indexService;
                }
                else
                {
                    return _indexService;
                }
            }
            set { }
        }
        private IDeleteArchivedSCIndexService _deleteArchivedSCIndexService { get; set; }
        private string _indexVolume = string.Empty;

        private string _siteUrl;
        private JobContext _jobContext = null;
        private string _jobId = string.Empty;
        private JobType _jobType;

        private SiteCollectionNodesInfo _siteCollectionNodesInfo;

        private RMDeleteArchivedSCJobReportManager _reportManager;
        private RMDeleteArchivedSCStubManager _stubManager;
        private RMDeleteArchivedSCSubInfoProcessor _subInfoProcessor;
        private RMDeleteArchivedSCFullTextIndexManager _fullTextIndexManager;
        private RMDeleteArchivedSCSizeInfoManager _sizeInfoManager;

        public RMDeleteArchivedSCJobHandler(string jobId, JobType jobType)
        {
            _jobType = jobType;
            _jobId = jobId;
            _jobContext = JobContext.GetInstance(_jobId, _jobType);
            _reportManager = new RMDeleteArchivedSCJobReportManager(_jobContext.ReportManager);
            _stubManager = new(_reportManager);
            _fullTextIndexManager = new(_reportManager);
            _sizeInfoManager = new(_reportManager);
            _subInfoProcessor = new(_reportManager, _stubManager, _fullTextIndexManager);
        }

        public async Task InitAsync()
        {
            _reportManager.Init();
            _siteCollectionNodesInfo = SerializerHelper.DeserializeByDataContractSerializer<SiteCollectionNodesInfo>(_jobContext.JobContextSetting);
            _siteUrl = _siteCollectionNodesInfo.SiteUrl;
        }

        public async Task RunAsync()
        {
            var errorMessage = string.Empty;
            try
            {
                await InitAsync();
                _logger.Info($"Start to run delete archived site collection job for site: {_siteUrl}");
                using var _ = await SampleDBLocker.Get4IndexDBUpdater(_siteUrl, _siteCollectionNodesInfo.SPObjectId, _jobId);

                Open();
                await _stubManager.InitAsync(_siteUrl, _deleteArchivedSCIndexService);
                await _subInfoProcessor.InitAsync(_siteUrl, _indexLogicalDevice, _deleteArchivedSCIndexService);
                _logger.Info($"Finish initialization for delete archived site collection job for site: {_siteUrl}");

                // need query by batch ?
                var masterIndexes = _archiverSiteMasterIndexDao.GetAllSiteCollectionNodsInfoByUrl(_siteUrl);
                if (masterIndexes.IsNullOrEmpty())
                {
                    _logger.Error($"No master index found for site: {_siteUrl}.");
                    _reportManager.AddFailedDetail(_siteUrl, string.Empty, 0, string.Empty, "RM_JM_JD_DeleteArchivedSC_Comment_NoArchivedHistory");
                    throw new Exception("RM_JM_JD_DeleteArchivedSC_Comment_NoArchivedHistory");
                }
                string groupMailboxAddress = masterIndexes.FirstOrDefault()?.GroupMailboxAddress ?? "";

                _logger.Info($"Found master index count: {masterIndexes.Count} for site: {_siteUrl}, groupMailboxAddress: {groupMailboxAddress}");
                _reportManager.IncreaseBase(masterIndexes.Count);

                await _sizeInfoManager.InitAsync(_siteUrl, groupMailboxAddress);

                var hasFailed = false;
                foreach (var masterIndex in masterIndexes)
                {
                    _logger.Info($"Start to process master with JobId: {masterIndex.JobId}, BackupFileType: {(BackupFileType)masterIndex.BackupFileType}, MergeState: {(MergeIndexState)masterIndex.MergeIndexState}, isDAOMigrated: {masterIndex.DAOMigrated}");
                    try
                    {
                        var isFileLevelMode = masterIndex.BackupFileType != (int)BackupFileType.DataBlock;
                        var result = await _subInfoProcessor.ProcessAsync(isFileLevelMode, masterIndex.JobId);

                        _logger.Info($"Finish processing master with JobId: {masterIndex.JobId}. hasFailed: {result.hasFailed}, TotalMediaDataSize: {result.totalMediaDataSize} bytes.");

                        if (!result.hasFailed)
                        {
                            await _sizeInfoManager.UpdateSizeInfoAsync(masterIndex.JobId, result.totalMediaDataSize);
                            _archiverSiteMasterIndexDao.DeleteByKey(masterIndex.Id);
                        }
                        else
                        {
                            hasFailed |= result.hasFailed;
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Error occurred while processing master index: {masterIndex.Id}, JobId: {masterIndex.JobId}. {e}");
                        hasFailed = true;
                    }
                    finally
                    {
                        _reportManager.IncreaseProgress();
                    }
                }

                _logger.Info($"Finish processing all master indexes for site: {_siteUrl}");

                try
                {
                    await _fullTextIndexManager.FlushDataAsync();
                    _logger.Info($"Finish flushing full text index data for site: {_siteUrl}.");

                    await _stubManager.FlushAsync();
                    _logger.Info($"Finish flushing stub manager for site: {_siteUrl}.");

                    await _sizeInfoManager.CommitSizeChangesAsync(hasFailed);
                    _logger.Info($"Finish committing size info changes for site: {_siteUrl}. hasFailed: {hasFailed}");
                }
                catch (Exception e)
                {
                    _logger.Error($"Error occurred while cleanup manager for site: {_siteUrl}. {e}");
                }

                if (!hasFailed)
                {
                    StorageInfo storageInfo = XConvert.FromNames(_indexVolume, ServiceConstants.IndexDBName);
                    var deleteIndexResult = _indexLogicalDevice.DeleteFile(storageInfo);
                    //if (deleteIndexResult.IsDeleted && deleteIndexResult.DeletedFileSize > 0)
                    //{
                    //    _reportManager.AddSucceedDetail(storageInfo.HighPlusLowName, "", deleteIndexResult.DeletedFileSize, _indexDeviceDto.Name);
                    //}
                    _logger.Info($"Finish deleting index file for site: {_siteUrl}. IsDeleted: {deleteIndexResult.IsDeleted}, DeletedFileSize: {deleteIndexResult.DeletedFileSize} bytes.");
                }

                await _fullTextIndexManager.WaitAllAsync();
            }
            catch (Exception e)
            {
                _logger.Error($"Error occurred while running delete archived site collection job for site: {_siteUrl}. {e}");
                _reportManager.ErrorMessage = "RM_SS_CommonErrorMessage";
            }
            finally
            {
                IndexService.Close();
                _indexLogicalDevice.Close();
                _reportManager.Finish();
            }
        }

        #region Index Service related private methods
        private void Open()
        {
            _indexDeviceDto = _storageDeviceService.GetIndexDevice();
            if (_indexDeviceDto == null)
            {
                throw new Exception("RM_JS_DAM_RunJob_Failed_NoIndexDeviceSetting");
            }

            StorageDeviceManager ??= new StorageDeviceManager();

            var indexLogicalDevive = RAStorageUtil.ConvertStorageDeviceDtoToLogicalDeviceDto(_indexDeviceDto);
            _indexLogicalDevice = StorageDeviceManager.Open(indexLogicalDevive.GetXRIS(PhysicalDeviceUsage.Index));
            InitAndOpenCacheManager();

            OpenObjectSiteCollectionIndex(_siteUrl);
        }

        public void InitAndOpenCacheManager()
        {
            IndexDatabaseHelper.isNoNeedUploadIndex = true;
            DiskInfoDto disk = new DiskInfoDto()
            {
                Path = Path.Combine(RecordsEnv.AppDomainRootFolder, "ArchiverCache", "deleteArchivedSC"),
                Type = DeviceType.LocalPath,
                Password = null,
                UserName = string.Empty,
                Usage = null
            };

            var cacheSetting = new CacheSettingDto
            {
                Extension = new CacheSettingExtension { Path = new List<PathMap>() { new PathMap() { DiskInfo = disk } } },
                LimitFreeSpace = 1024 * 1024 * 1024,//1 GB
            };

            CacheManager = PlatformWindsorManager.GetService<ICacheService>();
            CacheManager.Open(cacheSetting, _indexLogicalDevice.IsDirectSystem);
        }

        private void OpenObjectSiteCollectionIndex(string siteCollectionUrl)
        {
            var volumeGenerator = new ArchiverVolumeGenerator();
            var VolumeParam = new VolumeParameter() { FarmName = string.Empty, SiteCollectionUrl = siteCollectionUrl, };
            _indexVolume = volumeGenerator.GenerateIndexVolume(VolumeParam);

            ArchiverBrowseInfo browseInfo = new ArchiverBrowseInfo()
            {
                IndexVolume = _indexVolume,
                Path = siteCollectionUrl,
                EndTime = DateTime.MaxValue.Ticks,
                SiteUrl = siteCollectionUrl,
                TreeMode = TreeMode.SiteCollectionMode,
                IndexLogicalDevice = _indexDeviceDto,
                CacheSetting = CacheSettingService.GetBrowserCacheInfo(),
            };
            var openParam = new ArchiverIndexServiceOpenParameter(browseInfo, CacheManager.CacheSystem, _indexLogicalDevice)
            {
                WaitIndexLockerTimeOutInMs = 3000,
                IndexDatabaseName = ServiceConstants.IndexDBName,
                CacheSetting = browseInfo.CacheSetting
            };
            try
            {
                IndexService.Open(openParam);

                _deleteArchivedSCIndexService = new DeleteArchivedSCIndexService()
                {
                    HeadAndBodyService = new ArchiverHeadAndBodyIndexService()
                    {
                        IndexProcessor = _indexService.IndexProcessor
                    }
                };

                var expectedCount = _deleteArchivedSCIndexService.GetDeletingIndexesCountForDeleteArchivedSC();
                _reportManager.IncreaseBase(expectedCount);
            }
            catch (Exception e)
            {
                if (e is IndexCanNotFoundException || e.Message.Equals(MediaServiceArchiverBackupResource.ArchiverIndexServiceOpenIndexCanNotFoundException))
                {
                    _reportManager.AddFailedDetail(siteCollectionUrl, string.Empty, 0, _indexDeviceDto.Name, "RM_JM_JD_DeleteArchivedSC_Comment_IndexNotFound");
                }
                throw;
            }
        }
        #endregion
    }
}
