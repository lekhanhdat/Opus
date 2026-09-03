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
using AvePoint.GCommon.Contract.Server.ControlPanel;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Common;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Service.DomainModel.DocAve6x;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.ConvertStub;
using AvePoint.Wrapper.Common;
using Cloud.Sdk.Data.AosModern;
using Media.Common.ClassicStorageApi;
using Merged18NResources.MediaServiceArchiverBackup;
using RAExportCommon;
using RecordsHotfixMaintenanceService;
using Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.ExportDecryptionIndexDB
{
    public class ExportDecryptionIndexDBJobHandler : ApplicationModelServiceBase
    {
        private static readonly AveLogger s_logger = AveLogger.GetInstance(typeof(ConvertStubJobHandler));

        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private IRMStorageDeviceInfoDao StorageDeviceDao => PlatformWindsorManager.GetService<IRMStorageDeviceInfoDao>();



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

        private Dictionary<RMStorageDeviceInfo, IXSystem> _storageDeviceDic = new Dictionary<RMStorageDeviceInfo, IXSystem>();

        private IXSystem _indexLogicalDevice;

        private MediaConfigInfo _mediaConfigInfo = new MediaConfigInfo();

        private string _jobId;

        private DateTime _jobStartTime;

        #region init
        public ExportDecryptionIndexDBJobHandler(string jobId, string siteCollectionUrlsJson)
        {
            InitMediaObject(jobId, siteCollectionUrlsJson);
            InitIndexDBStorageDevice();
            InitAndOpenCacheManager();
            if (_mediaConfigInfo.NeedExportArchiverDataList)
            {
                InitArchiveDataStorageDevice();
            }
        }

        private void InitMediaObject(string jobId, string mediaJson)
        {
            s_logger.Info($"Start to init ExportDecryptionIndexDBJobHandler.jobId:{jobId}, mediaJson:{mediaJson}");

            _jobStartTime = DateTime.UtcNow;
            if (string.IsNullOrWhiteSpace(jobId))
            {
                throw new ArgumentNullException(nameof(jobId));
            }
            _jobId = jobId;

            if (string.IsNullOrWhiteSpace(mediaJson))
            {
                throw new ArgumentNullException(nameof(mediaJson));
            }

            _mediaConfigInfo = SerializerHelper.DeserializeByJsonConvert<MediaConfigInfo>(mediaJson);

            if (_mediaConfigInfo?.SiteCollectionUrls == null || !_mediaConfigInfo.SiteCollectionUrls.Any())
            {
                throw new Exception($"siteCollectionUrlsJson is not valid json array string, source json:{mediaJson}");
            }
        }

        private void InitIndexDBStorageDevice()
        {
            WrapperConfiguration.NeedToUploadIndex = false;
            var indexDeviceDto = StorageDeviceService.GetIndexDevice();
            if (indexDeviceDto == null)
            {
                throw new Exception("RM_JS_DAM_RunJob_Failed_NoIndexDeviceSetting");
            }

            StorageDeviceManager ??= new StorageDeviceManager();

            var indexLogicalDevive = RAStorageUtil.ConvertStorageDeviceDtoToLogicalDeviceDto(indexDeviceDto);
            _indexLogicalDevice = StorageDeviceManager.Open(indexLogicalDevive.GetXRIS(PhysicalDeviceUsage.Index));
        }

        private void InitArchiveDataStorageDevice()
        {
            List<RMStorageDeviceInfo> phDtos = new List<RMStorageDeviceInfo>();
            int page = 1;
            int size = 10;
            do
            {
                phDtos = StorageDeviceDao.GetAllStorageByIsOldRecord(0, new() { PageIndex = page++, PageSize = size });
                foreach (RMStorageDeviceInfo deviceInfo in phDtos)
                {
                    try
                    {
                        IXSystem device = XFactoryCommon.InstanceSystem(deviceInfo.ConnectionString);
                        device.Open();
                        _storageDeviceDic.Add(deviceInfo, device);
                    }
                    catch (Exception ex)
                    {
                        s_logger.Error($"failed to conn the device {deviceInfo.Name}, {ex}");
                    }
                }
            }
            while (phDtos != null && phDtos.Count >= size);
        }

        public void InitAndOpenCacheManager()
        {
            IndexDatabaseHelper.isNoNeedUploadIndex = true;
            DiskInfoDto disk = new DiskInfoDto()
            {
                Path = Path.Combine(RecordsEnv.AppDomainRootFolder, "ArchiverCache", "exportDecrypeIndexDB"),
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
        #endregion

        public void Run()
        {
            foreach (var siteCollectionUrl in _mediaConfigInfo.SiteCollectionUrls)
            {
                s_logger.Info($"Start to process for site collection {siteCollectionUrl}.");

                ExportIndexDB(siteCollectionUrl);

                if (_mediaConfigInfo.NeedExportArchiverDataList)
                {
                    ExportArchiverDataList(siteCollectionUrl);
                }
            }
        }

        #region ExportIndexDB
        public void ExportIndexDB(string siteCollectionUrl)
        {
            ArchiverIndexService indexService = null;
            string currentIndexFullPath = null;
            try
            {
                indexService = OpenObjectSiteCollectionIndex(StorageDeviceService.GetIndexDevice(), siteCollectionUrl);
                s_logger.Info($"Download index db for site collection {siteCollectionUrl} successfully.");

                indexService.IndexProcessor.ChangePassword(null);
                currentIndexFullPath = indexService.IndexProcessor.GetCurrentIndexFullPath();
                s_logger.Info($"Export decryption index db for site collection {siteCollectionUrl} successfully.");
            }
            catch (Exception ex)
            {
                s_logger.Error($"Export decryption index db for site collection {siteCollectionUrl} failed.", ex);
            }
            finally
            {
                try
                {
                    indexService?.Close();
                    s_logger.Info($"End to close decryption index db for site collection {siteCollectionUrl}.");
                }
                catch (Exception ex)
                {
                    s_logger.Error($"Close decryption index db for site collection {siteCollectionUrl} failed.", ex);
                }
            }

            try
            {
                UploadDatabase(GetBlobUri(siteCollectionUrl, currentIndexFullPath), currentIndexFullPath);
                s_logger.Info($"End to upload decryption index db for site collection {siteCollectionUrl}.");
            }
            catch (Exception ex)
            {
                s_logger.Error($"Upload decryption index db for site collection {siteCollectionUrl} failed.", ex);
            }
            finally
            {
                try
                {
                    FileUtility.TryDelete(currentIndexFullPath);
                }
                catch (Exception ex)
                {
                    s_logger.Error($"TryDelete current Index db {siteCollectionUrl} failed.", ex);
                }
            }
        }

        private ArchiverIndexService OpenObjectSiteCollectionIndex(StorageDeviceDto indexDeviceDto, string siteCollectionUrl)
        {
            ArchiverBrowseInfo browseInfo = new ArchiverBrowseInfo()
            {
                IndexVolume = new ArchiverVolumeGenerator().GenerateIndexVolume(new VolumeParameter() { FarmName = string.Empty, SiteCollectionUrl = siteCollectionUrl, }),
                Path = siteCollectionUrl,
                EndTime = DateTime.MaxValue.Ticks,
                SiteUrl = siteCollectionUrl,
                TreeMode = Media.Service.DomainModel.TreeMode.SiteCollectionMode,
                IndexLogicalDevice = indexDeviceDto,
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
                ArchiverIndexService _indexService = new ArchiverIndexService()
                {
                    IndexProcessor = new IndexProcessor<ArchiverIndexProcessorParameter>(),
                    IndexSynchronizer = new IndexDatabaseSynchronizer()
                };
                _indexService.Open(openParam);
                return _indexService;
            }
            catch (Exception e)
            {
                s_logger.Error($"Open site collection index db {siteCollectionUrl} failed.", e);
                throw;
            }
        }

        private string GetBlobUri(string siteCollection, string currentIndexFullPath)
        {
            currentIndexFullPath = currentIndexFullPath.Replace("\\", "/");
            string[] pathPart = currentIndexFullPath.Split("/");
            string blobUriEndPart = "";
            if (pathPart.Length < 3)
            {
                blobUriEndPart = Guid.NewGuid().ToString() + pathPart[^1];
                s_logger.Warn($"currentIndexFullPath {currentIndexFullPath} is not valid, use random guid {blobUriEndPart} as blob uri end part.");
            }
            else
            {
                blobUriEndPart = string.Join("/", pathPart[^3], pathPart[^2], pathPart[^1]);
            }

            return string.Join("/", TenantLocalValue.LogonGroupId, "ExportDecryptIndexDB", _jobStartTime.Ticks + "_" + _jobId, blobUriEndPart);
        }

        private void UploadDatabase(string blobUri, string dbFilePath)
        {
            if (!File.Exists(dbFilePath))
            {
                throw new Exception($"db not exist, dbFilePath:{dbFilePath}, blobUri:{blobUri}");
            }
            RAStorageUtil.UploadReportBlob(blobUri, dbFilePath);
        }
        #endregion

        #region ExportArchiverDataList
        public void ExportArchiverDataList(string siteCollectionUrl)
        {
            try
            {
                ArchiverVolumeGenerator volumeGenerator = new ArchiverVolumeGenerator();
                string highName = volumeGenerator.GenerateDataVolume(new VolumeParameter() { FarmName = string.Empty, SiteCollectionUrl = siteCollectionUrl, });
                foreach (RMStorageDeviceInfo deviceInfo in _storageDeviceDic.Keys)
                {
                    try
                    {
                        List<XFileInfo> fileInfos = _storageDeviceDic[deviceInfo].ListFiles(new StorageInfo() { HighName =  highName });
                        if (fileInfos != null && fileInfos.Count > 0)
                        {
                            StringBuilder stringBuilder = new StringBuilder();
                            stringBuilder.AppendLine($"ExportArchiverDataList site collection:{siteCollectionUrl}, device name:{deviceInfo.Name}, id:{deviceInfo.Id}, type:{deviceInfo.Type}, highName:{highName}");
                            foreach (var fileInfo in fileInfos)
                            {
                                stringBuilder.AppendLine(fileInfo.FullName);
                            }
                            s_logger.Info(stringBuilder.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        s_logger.Error($"Export archiver data list for site collection {siteCollectionUrl} on device NAME: {deviceInfo.Name} ,ID: {deviceInfo.Id} .ex:{ex}.");
                    }
                }
            }
            catch (Exception ex)
            {
                s_logger.Error($"Export site collections archiver data list {siteCollectionUrl} failed.", ex);
            }
        }
        #endregion


        private class MediaConfigInfo
        {
            public bool NeedExportArchiverDataList { get; set; }

            public List<string> SiteCollectionUrls { get; set; } = new List<string>();
        }
    }
}
