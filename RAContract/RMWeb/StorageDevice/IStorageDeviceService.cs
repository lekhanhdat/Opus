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
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.Contract.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.StorageDevice
{
    public interface IStorageDeviceService
    {
        Task<RAReturnMessage> CreateStorageDeviceAsync(StorageDeviceDto dto, EntityObjectPermissionType permission);
        Task<GOReturnMessage> CreateStorageDeviceAsyncForGoogleOne(StorageDeviceDto dto, EntityObjectPermissionType permission);
        System.Threading.Tasks.Task BatchCreateStorageDevicesAsync(IEnumerable<StorageDeviceDto> storages);
        Task<RAReturnMessage> UpdateStorageDeviceAsync(StorageDeviceDto dto);
        System.Threading.Tasks.Task UpdateAveStorageFor21VAsync(string storageId, string connStr);
        Task<GOReturnMessage> UpdateStorageDeviceAsyncForGoogleOne(StorageDeviceDto dto);

        Task<StorageDeviceResult> GetAllStorageDeviceByIsOldRecordAsync(int isOldRecord,StorageDeviceResult pageInfobool);
        StorageDeviceDto GetStorageDeviceById(string id, bool includeData = false, bool needDecryptSecert = false);
        StorageDeviceDto GetStorageDeviceByDAOStoragePolicyId(string id);
        Task<RAReturnMessage> DeleteStorageDevicesAsync(List<string> ids);
        /// <summary>
        /// 只有在 Cloud Archiver Migration Job里可以调用此方法来清理历史数据
        /// </summary>
        Task<int> DeleteMigratedStorageDevicesAsync();
        Task<RAReturnMessage> CheckAzureRegion(string accessPoint, string accountName, string storageDeviceId);
        string GetAzureRegionOfDataCenter();
        string GetAzureAccessPointUrl(string accessPoint, string accountName);
        Task<RAReturnMessage> ValidateAndCreateStorageDeviceAsync(StorageDeviceDto dto, EntityObjectPermissionType permission);
        Task<GOReturnMessage> ValidateAndCreateStorageDeviceAsyncForGoogleOne(StorageDeviceDto dto, EntityObjectPermissionType permission);
        System.Threading.Tasks.Task CheckCanDeleteStorageAsync(List<string> ids, RAReturnMessage result);
        RAReturnMessage ValidateStorageDeviceSpace(StorageDeviceDto dto);
        GOReturnMessage ValidateStorageDeviceSpaceForGoogleOne(StorageDeviceDto dto);

        Task<RAReturnMessage> SetUsingDeviceByIdAsync(string id, SettingProfilesType profileType, string profileName = "",bool isCompliantExport = false);
        StorageDeviceDto GetIndexDevice(bool needDecrypt = true);
        StorageDeviceDto GetIndexDeviceForMigrationJob();
        StorageDeviceDto GetExportDevice();
        Task<DevicesResult> GetStorageIdAndNameAsync(bool IsFilter);
        Task<List<StorageDeviceUIDto>> GetAllStorageDeviceNotPagedAsync();
        Task<List<StorageDeviceUIDto>> GetStorageDevicesIncludeGGNotPagedAsync();
        Task<List<StorageDeviceDto>> GetAllAsync();
        Task<List<StorageDeviceDto>> GetAllAvePointStorageAsync();
        StorageDeviceDto GetStorageDeviceByName(string name);
        System.Threading.Tasks.Task UpdateLastArchivedTimeAsync(string id, long lastArchivedTime);
        System.Threading.Tasks.Task CreateDefaultStorageDeviceAsync();
        string GetDefaultStorageConnectionString();
        long GetArchiverStorageGBSize();
        long GetAOSPArchiverStorageGBSize();
        Task<List<StorageDeviceDto>> GetSystemStorageAsync();

        bool IsDisableRetentionPeriodLimitation();
        string GetStorageDeviceNameById(string id);
        bool ValidateExportStorageInfo(string id);
        bool ValidateExportGoogleStorageInfo(string id);

        Task<double> GetAllArchiverStorageGBSizeAsync(string storageId, IEnumerable<string> excludedJobPrefixes = null, CancellationToken cancellationToken = default);
        System.Threading.Tasks.Task UpgradeAvePointStorageToManagedIdentityAsync();
    }
}
