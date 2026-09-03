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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Media.Service.DomainModel;
using ArchiverSiteMasterIndex = AvePoint.RA.DB.Model.ArchiverSiteMasterIndex;
namespace RADownloadCentre.IndexExport
{
    public static class ConvertExportDto
    {
        public static List<SettingProfileExportDto> ConvertSettingProfiles(Dictionary<string, string> key)
        {
            if (key == null || key.Count <= 0)
                return new List<SettingProfileExportDto>();
            var result = new List<SettingProfileExportDto>();
            foreach (var item in key)
            {
                result.Add(new SettingProfileExportDto
                {
                    Name = item.Key,
                    Settings = item.Value
                });
            }
            return result;
        }

        public static List<ArchiverSiteMasterIndexExportDto> ConvertSiteMasterIndexes(List<ArchiverSiteMasterIndex> siteMasterIndexs, Dictionary<string, int> subJobSourceFlagMapping)
        {
            List<ArchiverSiteMasterIndexExportDto> result = new();
            foreach (var siteMaster in siteMasterIndexs)
            {
                result.Add(new ArchiverSiteMasterIndexExportDto
                {
                    Id = siteMaster.Id,
                    ArchiverTime = siteMaster.ArchiverTime,
                    JobId = siteMaster.JobId,
                    SiteURL = siteMaster.SiteURL,
                    SiteId = siteMaster.SiteId,
                    SourceFlag = subJobSourceFlagMapping.TryGetValue(siteMaster.JobId, out var sFlag) ? sFlag : 1,
                    GroupMailboxAddress = siteMaster.GroupMailboxAddress,
                    O365TenantId = siteMaster.O365TenantId
                });
            }
            return result;
        }

        public static List<ArchiverIndexSubInfoExportDto> ConvertIndexSubInfoes(List<ArchiverIndexSubInfoContract> indexSubInfoes)
        {
            List<ArchiverIndexSubInfoExportDto> result = new();
            foreach (var indexSubInfo in indexSubInfoes)
            {
                result.Add(new ArchiverIndexSubInfoExportDto
                {
                    Id = indexSubInfo.Id,
                    JobId = indexSubInfo.JobId,
                    StorageId = indexSubInfo.StorageInfo,
                    CurrentStorageId = indexSubInfo.CurrentStorageId,
                    DataEncryptionDynamicKey = indexSubInfo.DataEncryptionDynamicKey,
                    DataEncryptionType = indexSubInfo.DataEncryptionType,
                    SubJobId = indexSubInfo.SubJobId,
                });
            }
            return result;
        }
        public static List<RMStorageDeviceInfoExportDto> ConvertStorageDeviceInfoes(List<StorageDeviceDto> storageDevices)
        {
            List<RMStorageDeviceInfoExportDto> result = new();
            foreach (var storageDevice in storageDevices)
            {
                result.Add(new RMStorageDeviceInfoExportDto
                {
                    Id = storageDevice.Id,
                    Name = storageDevice.Name,
                    Type = storageDevice.Type,
                    ModifiedTime = storageDevice.ModifyTime,
                    ConnectionString = storageDevice.ConnectionString,
                });
            }
            return result;
        }

        public static List<CommonSiteMasterIndexExportDto> ConvertCommonSiteMasterIndex(List<AvePoint.RA.DB.Model.CommonSiteMasterIndex> commonSiteMasterIndexes)
        {
            List<CommonSiteMasterIndexExportDto> result = new();
            foreach (var commonSiteMasterIndex in commonSiteMasterIndexes)
            {
                result.Add(new CommonSiteMasterIndexExportDto
                {
                    Id = commonSiteMasterIndex.Id,
                    ArchiverTime = commonSiteMasterIndex.ArchiverTime,
                    JobId = commonSiteMasterIndex.JobId,
                    SiteURL = commonSiteMasterIndex.SiteURL,
                    StorageId = commonSiteMasterIndex.StorageId,
                    IndexStorageId = commonSiteMasterIndex.IndexStorageId,
                    SiteGroupId = commonSiteMasterIndex.SiteGroupId,
                    TeamId = commonSiteMasterIndex.TeamId,
                    SiteId = commonSiteMasterIndex.SiteId,
                    SPVersion = commonSiteMasterIndex.SPVersion,
                    MergeIndexState = commonSiteMasterIndex.MergeIndexState,
                    JobState = commonSiteMasterIndex.JobState,
                    StorageInfo = commonSiteMasterIndex.StorageInfo,
                    Extension = commonSiteMasterIndex.Extension,
                    Flag = commonSiteMasterIndex.Flag,
                    DAOMigrated = commonSiteMasterIndex.DAOMigrated,
                    BackupFileType = commonSiteMasterIndex.BackupFileType,
                    DuplicateStatus = commonSiteMasterIndex.DuplicateStatus,
                    DataType = commonSiteMasterIndex.DataType,
                    O365TenantId = commonSiteMasterIndex.O365TenantId,
                });
            }
            return result;
        }
    }
}
