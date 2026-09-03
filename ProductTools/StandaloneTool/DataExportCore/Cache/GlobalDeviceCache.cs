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
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Media.Core.Index;
using AvePoint.Media.Service;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using DataExportCore.Utils;
using Media.Common.ClassicStorageApi;
using RecordsHotfixMaintenanceService;
using Storage;
using System.Collections.Concurrent;

namespace DataExportCore.Cache
{
    public static class GlobalDeviceCache
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(GlobalDeviceCache));
        // device cache
        private static IStorageDeviceManager StorageManager = new StorageDeviceManager();

        private static ConcurrentDictionary<String, IXSystem> StorageDevices = []; //StoragePolicyId 

        private static ConcurrentDictionary<String, LogicalDeviceDto> storageDtos = [];

        private static ConcurrentDictionary<String, String> FailedStorageIds = [];

        public static IXSystem IndexDevice;
        public static IXSystem ExportCacheSetting;
        public static ICacheService CacheManager;
        private static IXSystem? AvepointMappingStorage;
        private static IXSystem? TargetStorage;

        public static void InitGlobalDeviceCaches(Dictionary<string, LogicalDeviceDto> localDevices, IXSystem? avepointMappingStorage, IXSystem? targetStorage)
        {
            AvepointMappingStorage = avepointMappingStorage;
            TargetStorage = targetStorage;
            InitLogicalDeviceCache(localDevices);
            InitIndexDevice();
            InitAndOpenCacheManager();
            InitExportCacheSettings();
        }

        public static IXSystem GetDestinationDevice()
        {
            if (TargetStorage == null)
            {
                logger.Error("Target Storage is null");
                throw new Exception("Can not found target storage device");
            }
            return TargetStorage;
        }

        public static void InitIndexDevice()
        {
            if (string.IsNullOrEmpty(GlobalCache.IndexDeviceId) || GlobalCache.IndexDeviceId == Guid.Empty.ToString())
            {
                throw new Exception("IndexDeviceIdIsNull");
            }

            try
            {
                IndexDevice = GetDeviceById(GlobalCache.IndexDeviceId, true);
            }
            catch (Exception e)
            {
                logger.Error($"Open index device failed. Ex: {e}");
                throw;
            }
        }

        public static void InitLogicalDeviceCache(Dictionary<string, LogicalDeviceDto> StorageDevices)
        {
            storageDtos = new(StorageDevices, StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsDeviceExist(string policyId)
        {
            return storageDtos.ContainsKey(policyId);
        }

        public static bool IsDeviceAPStorage(string policyId)
        {
            return policyId.Equals(ExportUtility.AVEPOINT_STORAGE_ID, StringComparison.OrdinalIgnoreCase) || storageDtos.TryGetValue(policyId, out var deviceDto) && deviceDto.PhysicalDrives.First().IsSystemStorage;
        }

        public static IXSystem GetDeviceById(string policyId, bool isIndexDevice = false)
        {
            logger.Info($"Get Device with policyId: {policyId}, isIndexDevice: {isIndexDevice}");

            if (policyId.Equals(ExportUtility.AVEPOINT_STORAGE_ID, StringComparison.OrdinalIgnoreCase))
            {
                if (AvepointMappingStorage == null)
                {
                    logger.Error("Avepoint Mapping Storage Is Null.");
                    throw new Exception("Avepoint Mapping Storage Is Null");
                }

                logger.Info("Change to using AvepointMappingStorage.");
                return AvepointMappingStorage;
            }

            if (StorageDevices.TryGetValue(policyId, out var device))
            {
                return device;
            }

            if (storageDtos.TryGetValue(policyId, out var deviceDto))
            {
                try
                {
                    if (deviceDto.PhysicalDrives.Count == 0)
                    {
                        throw new Exception($"No physical drives found for policyId: {policyId}");
                    }

                    if (deviceDto.PhysicalDrives.First().IsSystemStorage)
                    {
                        if (AvepointMappingStorage == null)
                        {
                            logger.Error("Avepoint Mapping Storage Is Null.");
                            throw new Exception("Avepoint Mapping Storage Is Null");
                        }

                        logger.Info("Change to using AvepointMappingStorage.");
                        return StorageDevices[policyId] = AvepointMappingStorage;
                    }

                    IXSystem isystem;
                    if (isIndexDevice)
                    {
                        logger.Info("Start opening index device");
                        isystem = XFactoryCommon.InstanceSystem(deviceDto.PhysicalDrives.First().ConnectionString);
                        isystem.Open();
                        logger.Info("Index device opened successfully.");
                    }
                    else
                    {
                        logger.Info("Start opening logical device");
                        isystem = StorageManager.Open(deviceDto.ToXRIS());
                        logger.Info("Logical device opened successfully.");
                    }
                    StorageDevices[policyId] = isystem;
                    return isystem;
                }
                catch (Exception e)
                {
                    logger.Error($"Failed to open device for policyId: {policyId}. Exception: {e.Message}");
                    var storageTypeString = ((StorageDeviceType)deviceDto.PhysicalDrives.First().Type).ToString();
                    FailedStorageIds.TryAdd(policyId, storageTypeString);
                    throw new ManagedException(ErrorType.CannotOpenDevice, new[] { policyId, storageTypeString ?? StorageDeviceType.None.ToString() });
                }
            }
            else
            {
                logger.Warn($"Device with policyId: {policyId} not found.");
                throw new ManagedException(ErrorType.DeviceNotFound, new[] { policyId });
            }
        }

        public static bool IsStorageOpenFailed(string policyId, out string? type)
        {
            return FailedStorageIds.TryGetValue(policyId, out type);
        }


        // encryption info cache
        private static ConcurrentDictionary<String, ArchiverIndexSubInfoContract?> IndexSubInfoCache = new ConcurrentDictionary<String, ArchiverIndexSubInfoContract?>(); //SubjobId 

        public static DataEncryptionInfo? GetEncryptionInfoBySubJobId(string subjobId)
        {
            if (IndexSubInfoCache.TryGetValue(subjobId, out var indexSubInfo) && indexSubInfo != null)
            {
                return indexSubInfo.DataEncryptionInfo;
            }
            else
            {
                logger.Error($"An error occurred when getting the Data Encryption Info in {subjobId}");
                throw new ManagedException(ErrorType.SubJobNotFound);
            }
        }

        public static string GetCurrentStoragePolicyIdBySubjobId(string subjobId)
        {
            if (IndexSubInfoCache.TryGetValue(subjobId, out var indexSubInfo) && indexSubInfo != null)
            {
                return string.IsNullOrEmpty(indexSubInfo.CurrentStorageId) || !IsDeviceExist(indexSubInfo.CurrentStorageId) ? indexSubInfo.StorageInfo : indexSubInfo.CurrentStorageId;
            }
            else
            {
                logger.Error($"An error occurred when getting the current storage in {subjobId}");
                throw new ManagedException(ErrorType.SubJobNotFound);
            }
        }

        public static string GetMailBoxCurrentStoragePolicyIdBySubJobId(string subjobId)
        {
            var indexSubInfo = IndexSubInfoCache.Where(_ => subjobId.StartsWith(_.Key)).Select(_ => _.Value).FirstOrDefault();
            if (indexSubInfo != null)
            {
                return string.IsNullOrEmpty(indexSubInfo.CurrentStorageId) || !IsDeviceExist(indexSubInfo.CurrentStorageId) ? indexSubInfo.StorageInfo : indexSubInfo.CurrentStorageId;
            }
            else
            {
                logger.Error($"An error occurred when getting the current storage in {subjobId}");
                throw new ManagedException(ErrorType.SubJobNotFound);
            }
        }

        public static DataEncryptionInfo? GetMailBoxEncryptionInfoBySubJobId(string subjobId)
        {
            var indexSubInfo = IndexSubInfoCache.Where(_ => subjobId.StartsWith(_.Key)).Select(_ => _.Value).FirstOrDefault();
            if (indexSubInfo != null)
            {
                return indexSubInfo.DataEncryptionInfo;
            }
            else
            {
                logger.Error($"An error occurred when getting the Data Encryption Info in {subjobId}");
                throw new ManagedException(ErrorType.SubJobNotFound);
            }
        }

        public static void AddIndexSubInfo(string subjobId, ArchiverIndexSubInfoContract? indexSubInfo)
        {
            if (!IndexSubInfoCache.TryAdd(subjobId, indexSubInfo))
            {
                logger.Warn($"Subjob with id [{subjobId}] existed in the cache.");
            }
        }

        public static void InitExportCacheSettings()
        {
            DiskInfoDto disk = new DiskInfoDto()
            {
                Path = GlobalCache.ExportLocation,
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

            ExportCacheSetting = XFactoryCommon.InstanceLibrary(cacheSetting.ConvertToMediaLogicalDeviceDto().ToXRIS());
            ExportCacheSetting.Open();
        }


        // init vs open cache

        public static void InitAndOpenCacheManager()
        {
            IndexDatabaseHelper.isNoNeedUploadIndex = true;
            DiskInfoDto disk = new DiskInfoDto()
            {
                Path = Path.Combine(RecordsEnv.AppDomainRootFolder, "ArchiverCache", "restore"),
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
            CacheManager.Open(cacheSetting, IndexDevice.IsDirectSystem);
        }
    }
}
