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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.Service.Services.StorageDevice;

namespace AvePoint.RA.ArchiverMigration.ArchiverMigration
{
    internal class StorageMigrationService
    {
        protected RALogger logger = RALogger.GetInstance(typeof(StorageMigrationService));
        private DAOAPIClientV1 DAOApiClient;
        private Dictionary<string, List<StorageDeviceDto?>> storagePolicyIdMappings = new Dictionary<string, List<StorageDeviceDto?>>();
        private Dictionary<string, List<StorageDeviceDto?>> logicalDeviceIdMappings = new Dictionary<string, List<StorageDeviceDto?>>();
        private Dictionary<string, List<StorageDeviceDto?>> physicalDeviceIdMappings = new Dictionary<string, List<StorageDeviceDto?>>();
        private Dictionary<string, int> existsRepeatStorageNames = new Dictionary<string, int>();

        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();


        public StorageMigrationService(DAOAPIClientV1 daoApiClient)
        {
            DAOApiClient = daoApiClient;
        }

        public async Task<List<StorageDeviceDto>> LoadStoragesFromDaoStoragePoliciesAsync()
        {
            var existsStorages = StorageDeviceService.GetAllAsync().GetAwaiter().GetResult();
            foreach (var item in existsStorages)
            {
                existsRepeatStorageNames[item.Name] = 0;
            }

            var migrateStorageDevices = await GetAllStoragesFromDaoAsync();
            await CacheAllStorages(migrateStorageDevices);
            return migrateStorageDevices;
        }

        public string GetStorageDeviceIdByDAOStoragePolicyId(string id)
        {
            return GetStorageDeviceByDAOStoragePolicyId(id)?.Id ?? Guid.Empty.ToString();
        }

        public StorageDeviceDto? GetStorageDeviceByDAOStoragePolicyId(string id)
        {
            List<StorageDeviceDto> results = null;
            if (storagePolicyIdMappings.TryGetValue(id.ToLower(), out results))
            {
                var storageDevice = results.FirstOrDefault();
                if(storageDevice == null)
                {
                    logger.Warn($"Cann't find target storage device for Storage Policy: {id}");
                }
                return storageDevice;
            }
            else
            {
                logger.Warn($"Couldn't find target storage device for Storage Policy: {id}");
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logicalDeviceId"></param>
        /// <returns>(isNew, storageDevice, isFromCache)</returns>
        public async Task<(bool, StorageDeviceDto?, bool)> GetStorageDeviceByDAOLogicalDeviceIdAsync(string logicalDeviceId)
        {
            List<StorageDeviceDto?>? matchDevices = null;
            if (logicalDeviceIdMappings.TryGetValue(logicalDeviceId.ToLower(), out matchDevices) && matchDevices.Count > 0)
            {
                return (false, matchDevices.FirstOrDefault(), true);
            }
            else
            {
                logger.Warn($"Couldn't find target storage device for Logical Device: {logicalDeviceId}");
                var physicalDeviceId = await GetFirstPhysicalIdInLogicalDeviceAsync(logicalDeviceId);
                if (string.IsNullOrEmpty(physicalDeviceId))
                {
                    logicalDeviceIdMappings[logicalDeviceId] = new List<StorageDeviceDto?>() { null };
                    logger.Error($"Couldn't get physical device id for Logical Device: {logicalDeviceId}");
                    return (false, null, false);
                }

                return GetStorageDeviceByDAOPhysicalDeviceId(physicalDeviceId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="physicalDeviceId"></param>
        /// <returns>(isNew, storageDevice, isFromCache)</returns>
        public (bool, StorageDeviceDto?, bool) GetStorageDeviceByDAOPhysicalDeviceId(string physicalDeviceId)
        {
            List<StorageDeviceDto?>? matchDevices = null;
            if (physicalDeviceIdMappings.TryGetValue(physicalDeviceId.ToLower(), out matchDevices) && matchDevices.Count > 0)
            {
                var storageDevice = matchDevices.FirstOrDefault();
                logger.Info($"The physical device {physicalDeviceId} of the logical device had a matched StorageDevice: {storageDevice?.Id}");
                return (false, storageDevice, true);
            }
            else
            {
                //var storageDevice = await GetStorageByPhysicalDeviceAsync(physicalDeviceId);
                //if (storageDevice == null)
                //{
                //    physicalDeviceIdMappings[physicalDeviceId] = new List<StorageDeviceDto?>() { null };
                    logger.Error($"Couldn't assembly new StorageDevice by physical device: {physicalDeviceId}");
                    return (false, null, true);
                //}

                //logger.Info($"The physical device: {physicalDeviceId}, assembly a new StorageDevice: {storageDevice.Id}");
                //return (true, storageDevice, false);
            }
        }


        private async Task CacheAllStorages(List<StorageDeviceDto> migrateStorageDevices)
        {
            var moveRetentionRules = new List<GCommon.Contract.Storage.Entity.RetentionRule>();
            foreach (var migrateStorage in migrateStorageDevices)
            {
                logger.Info($"Storage : {migrateStorage.Name} - {migrateStorage.DAOStoragePolicyId} | {migrateStorage.DAOLogicalDeviceId} | {migrateStorage.DAOPhysicalDeviceId}");
                if (migrateStorage.ArchiveRetentionRules != null)
                {
                    foreach (var retentionRule in migrateStorage.ArchiveRetentionRules)
                    {
                        if(retentionRule.IsMove && !string.IsNullOrEmpty(retentionRule.MoveDeviceId))
                        {
                            moveRetentionRules.Add(retentionRule);
                        }
                    }
                }

                List<StorageDeviceDto?>? devices = null;
                if (!string.IsNullOrEmpty(migrateStorage.DAOStoragePolicyId))
                {
                    if (storagePolicyIdMappings.TryGetValue(migrateStorage.DAOStoragePolicyId.ToLower(), out devices))
                    {
                        devices.Add(migrateStorage);
                    }
                    else
                    {
                        devices = new List<StorageDeviceDto?>() { migrateStorage };
                        storagePolicyIdMappings[migrateStorage.DAOStoragePolicyId.ToLower()] = devices;
                    }
                }

                if (!string.IsNullOrEmpty(migrateStorage.DAOLogicalDeviceId))
                {
                    if (logicalDeviceIdMappings.TryGetValue(migrateStorage.DAOLogicalDeviceId.ToLower(), out devices))
                    {
                        devices.Add(migrateStorage);
                    }
                    else
                    {
                        devices = new List<StorageDeviceDto?>() { migrateStorage };
                        logicalDeviceIdMappings[migrateStorage.DAOLogicalDeviceId.ToLower()] = devices;
                    }
                }

                if (!string.IsNullOrEmpty(migrateStorage.DAOPhysicalDeviceId))
                {
                    if (physicalDeviceIdMappings.TryGetValue(migrateStorage.DAOPhysicalDeviceId.ToLower(), out devices))
                    {
                        devices.Add(migrateStorage);
                    }
                    else
                    {
                        devices = new List<StorageDeviceDto?>() { migrateStorage };
                        physicalDeviceIdMappings[migrateStorage.DAOPhysicalDeviceId.ToLower()] = devices;
                    }
                }
            }

            TryRenameIfNameExists(migrateStorageDevices);

            ReOrderAllCachedStorages();

            //List<StorageDeviceDto> moveDevices = new List<StorageDeviceDto>();
            foreach (var moveRetentionRule in moveRetentionRules)
            {
                (bool isNew, StorageDeviceDto? storage, _) = await GetStorageDeviceByDAOLogicalDeviceIdAsync(moveRetentionRule.MoveDeviceId);
                moveRetentionRule.MoveDeviceId = storage?.Id ?? Guid.Empty.ToString();
                //if (isNew && storage != null)
                //{
                //    moveDevices.Add(storage);
                //}
            }
            //migrateStorageDevices.AddRange(moveDevices);
        }

        private void ReOrderAllCachedStorages()
        {
            var allMappings = new List<Dictionary<string, List<StorageDeviceDto?>>>() { 
                storagePolicyIdMappings, logicalDeviceIdMappings, physicalDeviceIdMappings 
            };
            foreach (var mappings in allMappings)
            {
                foreach (var key in mappings.Keys)
                {
                    var storages = mappings[key];
                    mappings[key] = ReOrderByName(storages);
                }
            }
        }

        private List<StorageDeviceDto?> ReOrderByName(IEnumerable<StorageDeviceDto?> storages)
        {
            if (storages.Any(s => s == null) && storages.Any(s => s != null))
            {
                storages = storages.Where(s => s != null);
            }
            return storages.OrderBy(storage => storage?.Name).ToList();
        }

        private void TryRenameIfNameExists(List<StorageDeviceDto> migrateStorageDevices)
        {
            foreach (var storage in migrateStorageDevices)
            {
                TryRenameIfNameExists(storage);

                logger.Info($"Migrate storage: {storage.Name} | {storage.Id}, StoragePolicyId: {storage.DAOStoragePolicyId}, LogicalDeviceId: {storage.DAOLogicalDeviceId}, PhysicalDeviceId: {storage.DAOPhysicalDeviceId}");
            }
        }
        public void TryRenameIfNameExists(StorageDeviceDto storage)
        {
            if (existsRepeatStorageNames.TryGetValue(storage.Name, out int repeatCount))
            {
                repeatCount++;
                var finalName = $"{storage.Name}_{repeatCount}";
                logger.Info($"Storage Device name exists. StorageId: {storage.Id}, Name from {storage.Name} to {finalName}");
                storage.Name = finalName;
            }

            existsRepeatStorageNames[storage.Name] = 0;
        }

        #region get data from DAO

        private async Task<List<StorageDeviceDto>> GetAllStoragesFromDaoAsync()
        {
            return await DAOApiClient.GetArchiverMigrationDataAsync<List<StorageDeviceDto>>((service) =>
            {
                return service.GetAllStorages();
            });
        }

        private async Task<string> GetFirstPhysicalIdInLogicalDeviceAsync(string logicalId)
        {
            return await DAOApiClient.GetArchiverMigrationDataAsync<string>((service) =>
            {
                return service.GetFirstPhysicalIdInLogicalDevice(logicalId);
            });
        }

        #endregion
    }
}
