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

using Aspose.Pdf;
using Aspose.Pdf.Operators;
using AvePoint.Common.Portal;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Core.IO;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.Service.JobMonitor;
using AvePoint.RA.Service.Services.Archiver;
using AvePoint.RA.Service.Services.RMReport;
using AvePoint.RA.Service.Services.Settings.AuditHandler;
using AvePoint.RA.Web.Controllers.PhysicalDevice;
using Cloud.Sdk.Data.Dao;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using Media.Common.ClassicStorageApi;
using Microsoft.Azure.Cosmos;
using Microsoft.Graph;
using Newtonsoft.Json;
using Storage;
using Storage.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Util;
using static AvePoint.GCommon.Utility.I18N.ContextValues.Configuration;
using static AvePoint.RA.RACommonUtility.Common.CommonUtilityForSpecialTenant;
using StorageDeviceType = AvePoint.GCommon.Contract.Storage.Entity.StorageDeviceType;

namespace AvePoint.RA.Service.Services.StorageDevice
{
    [Audit]
    public class StorageDeviceService : RMServiceBase, IStorageDeviceService
    {
        private IRMStorageDeviceInfoDao StorageDeviceDao => PlatformWindsorManager.GetService<IRMStorageDeviceInfoDao>();
        private ISettingProfilesDao SettingProfileDao => PlatformWindsorManager.GetService<ISettingProfilesDao>();
        public IArchiverIndexSubInfoDao ArchiverIndexSubInfoDao => PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();
        private IJobMonitorService RMJobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();
        private IGlobalStorageSettingDao GlobalStorageSettingDao => PlatformWindsorManager.GetService<IGlobalStorageSettingDao>();
        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IEXOArchiverIndexSubInfoDao EXOArhciverSubInfo => PlatformWindsorManager.GetService<IEXOArchiverIndexSubInfoDao>();

        private RALogger logger = RALogger.GetInstance(typeof(RMReportService));
        private static Dictionary<string, HashSet<IPNetwork>> azureRegionDic;
        private static string azureRegionFileUrl;
        private static DateTime azureRegionRefreashTime = new DateTime();
        private IXSystem deviceSystem;
        private IRuleManagerService mRuleManagerService;
        protected IRuleManagerService RuleManagerService
        {
            get
            {
                if (mRuleManagerService == null)
                {
                    mRuleManagerService = (IRuleManagerService)PlatformWindsorManager.GetService(typeof(IRuleManagerService));
                }
                return mRuleManagerService;
            }
        }
        public readonly string DEFAULTSTORAGENAME = "AvePoint Storage";

        public async Task<RAReturnMessage> CreateStorageDeviceAsync(StorageDeviceDto dto, EntityObjectPermissionType permission)
        {
            string id = string.Empty;
            RAReturnMessage creatStatus = new RAReturnMessage() { MessageType=RAMessageType.Successful,ErrorMessage=string.Empty};
            try
            {
                dto.ModifyTime = DateTime.UtcNow.Ticks;
                dto.Status = RMConstants.STORAGE_NEW_DATA_TYPE;
                id = StorageDeviceDao.Create(StorageDeviceConvert.ConvertStorageDeviceDto(dto)).Id.ToString();
                dto.Id = id;
                //await this.DoRunAsync(dto);
                return creatStatus;
            }
            catch (Exception e)
            {
                logger.Error($"CreateStorageDeviceAsync error :{e}");
                creatStatus.MessageType = RAMessageType.Failed;
                creatStatus.ErrorMessage = I18NEntity.GetString("RM_AR_Storage_Unknow_ErrorMessage");
                throw;
            }
        }

        public async Task<GOReturnMessage> CreateStorageDeviceAsyncForGoogleOne(StorageDeviceDto dto, EntityObjectPermissionType permission)
        {
            string id = string.Empty;
            GOReturnMessage createStatus = new GOReturnMessage() { MessageType = RAMessageType.Successful, ErrorMessage = string.Empty };
            try
            {
                dto.ModifyTime = DateTime.UtcNow.Ticks;
                dto.Status = RMConstants.STORAGE_NEW_DATA_TYPE;
                id = StorageDeviceDao.Create(StorageDeviceConvert.ConvertStorageDeviceDto(dto)).Id.ToString();
                dto.Id = id;
                RMStorageDeviceInfo info = StorageDeviceDao.GetStorageDevicesById(new Guid(id));
                createStatus.storageIdAndName = new StorageIdAndName { Id = info.Id.ToString(), Name = info.Name , Type = info.Type };
                return createStatus;
            }
            catch (Exception e)
            {
                logger.Error($"CreateStorageDeviceAsync error :{e}");
                createStatus.MessageType = RAMessageType.Failed;
                createStatus.ErrorMessage = I18NEntity.GetString("RM_AR_Storage_Unknow_ErrorMessage");
                throw;
            }
        }


        public async System.Threading.Tasks.Task BatchCreateStorageDevicesAsync(IEnumerable<StorageDeviceDto> storages)
        {
            logger.Info($"{storages.Count()} Storage Devices will be created.");
            var domains = storages.Select(async i =>
            {
                i.ModifyTime = DateTime.UtcNow.Ticks;
                i.Status = RMConstants.STORAGE_NEW_DATA_TYPE;
                if (await LicenseHelperService.IsCloudArchivingByos() && (i.IsSystemStorage || i.IsAveStorage))
                {
                    logger.Info($"Because Cloud Archiving is a BYOS license, default storage:[{i.Name}] is set to unavailable state.");
                    //i.Status = RMConstants.STORAGE_OLD_DATA_TYPE;
                }
                return StorageDeviceConvert.ConvertStorageDeviceDto(i);
            });
            var count = await StorageDeviceDao.BatchCreateAsync(await Task.WhenAll(domains));
            logger.Info($"{count} Storage Device created.");
        }

        // Update AvePoint Storage for 21V migrated tenants
        public async Task UpdateAveStorageFor21VAsync(string storageId, string connStr)
        {
            StorageDeviceDto oldDto = this.GetStorageDeviceById(storageId);
            oldDto.Id = null;
            oldDto.Status = RMConstants.STORAGE_OLD_DATA_TYPE;
            oldDto.BackupPhysicalDeviceId = storageId;
            StorageDeviceDao.Create(oldDto);

            var newDto = oldDto;
            newDto.Id = storageId;
            newDto.Status = RMConstants.STORAGE_NEW_DATA_TYPE;
            newDto.ModifyTime = DateTime.UtcNow.Ticks;
            newDto.ConnectionString = connStr;
            oldDto.BackupPhysicalDeviceId = "";
            await UpdateStorageDeviceInfoAsync(newDto);
        }

        public async Task<RAReturnMessage> UpdateStorageDeviceAsync(StorageDeviceDto dto)
        {
            string id = string.Empty;
            RAReturnMessage UpdateStatus = new RAReturnMessage() { MessageType=RAMessageType.Successful,ErrorMessage=string.Empty};
            try
            {
                StorageDeviceDto oldDto = this.GetStorageDeviceById(dto.Id);

                if (dto.Id.Equals(RecordsConstants.AVEPOINT_DEFAULT_STORAGEID, StringComparison.OrdinalIgnoreCase) || dto.IsSystemStorage)
                {
                    oldDto.SetupDataRetention = dto.SetupDataRetention;
                    oldDto.ArchiveRetentionRules = dto.ArchiveRetentionRules;
                    oldDto.ModifyTime = DateTime.UtcNow.Ticks;
                    id = await UpdateStorageDeviceInfoAsync(oldDto);
                }
                else
                {
                    oldDto.Id = null;
                    oldDto.Status = RMConstants.STORAGE_OLD_DATA_TYPE;
                    oldDto.BackupPhysicalDeviceId = dto.Id;
                    //oldDto.ObjectInfo = null;
                    StorageDeviceDao.Create(oldDto);
                    dto.ModifyTime = DateTime.UtcNow.Ticks;
                    dto.DAOMigrated = oldDto.DAOMigrated;
                    dto.DAOLogicalDeviceId = oldDto.DAOLogicalDeviceId;
                    dto.DAOPhysicalDeviceId = oldDto.DAOPhysicalDeviceId;
                    dto.DAOStoragePolicyId = oldDto.DAOStoragePolicyId;
                    dto.LastArchivedTime = oldDto.LastArchivedTime;
                    id = await UpdateStorageDeviceInfoAsync(dto);
                    //id = physicalDeviceDao.Update(dto);
                    //await this.DoRunAsync(dto);
                }
                return UpdateStatus;
            }
            catch (Exception e)
            {
                UpdateStatus.MessageType = RAMessageType.Failed;
                UpdateStatus.ErrorMessage = I18NEntity.GetString("RM_AR_Storage_Unknow_ErrorMessage");
                throw;
            }
        }

        public async Task<GOReturnMessage> UpdateStorageDeviceAsyncForGoogleOne(StorageDeviceDto dto)
        {
            string id = string.Empty;
            GOReturnMessage UpdateStatus = new GOReturnMessage() { MessageType = RAMessageType.Successful, ErrorMessage = string.Empty };
            try
            {
                StorageDeviceDto oldDto = this.GetStorageDeviceById(dto.Id);

                if (dto.Id.Equals(RecordsConstants.AVEPOINT_DEFAULT_STORAGEID, StringComparison.OrdinalIgnoreCase) || dto.IsSystemStorage)
                {
                    oldDto.SetupDataRetention = dto.SetupDataRetention;
                    oldDto.ArchiveRetentionRules = dto.ArchiveRetentionRules;
                    oldDto.ModifyTime = DateTime.UtcNow.Ticks;
                    id = await UpdateStorageDeviceInfoAsync(oldDto);
                }
                else
                {
                    oldDto.Id = null;
                    oldDto.Status = RMConstants.STORAGE_OLD_DATA_TYPE;
                    oldDto.BackupPhysicalDeviceId = dto.Id;
                    //oldDto.ObjectInfo = null;
                    StorageDeviceDao.Create(oldDto);
                    dto.ModifyTime = DateTime.UtcNow.Ticks;
                    dto.DAOMigrated = oldDto.DAOMigrated;
                    dto.DAOLogicalDeviceId = oldDto.DAOLogicalDeviceId;
                    dto.DAOPhysicalDeviceId = oldDto.DAOPhysicalDeviceId;
                    dto.DAOStoragePolicyId = oldDto.DAOStoragePolicyId;
                    dto.LastArchivedTime = oldDto.LastArchivedTime;
                    id = await UpdateStorageDeviceInfoAsync(dto);
                }
                if(id != null)
                {
                    RMStorageDeviceInfo info = StorageDeviceDao.GetStorageDevicesById(new Guid(id));
                    UpdateStatus.storageIdAndName = new StorageIdAndName { Id = info.Id.ToString(), Name = info.Name , Type = info.Type};
                    return UpdateStatus;
                }
                return UpdateStatus;
            }
            catch (Exception e)
            {
                UpdateStatus.MessageType = RAMessageType.Failed;
                UpdateStatus.ErrorMessage = I18NEntity.GetString("RM_AR_Storage_Unknow_ErrorMessage");
                throw;
            }
        }

        public async Task<StorageDeviceResult> GetAllStorageDeviceByIsOldRecordAsync(int isOldRecord, StorageDeviceResult pageInfo)
        {
            await GeneralSettingService.SaveUsingSecurityProfileAsync();
            if (TenantService.IsNewOpusTenant())
            {
                //SaveAllSecurityProfile();
                SettingProfileDto indexDto = new SettingProfileDto()
                {
                    Type = (int)SettingProfilesType.IndexDevice,
                    Name = "UsingIndexDevice"
                };
                List<RMStorageDeviceInfo> phDtos = StorageDeviceDao.GetAllStorageByIsOldRecord(isOldRecord, pageInfo);
                foreach (var pro in phDtos)
                {
                    //Move to migration job
                    //if (await LicenseHelperService.IsCloudArchivingByos() && pro.IsSystemStorage && pro.DAOMigrated.GetValueOrDefault())
                    //{
                    //    logger.Warn($"Cloud archiving Byos, hidden archiving default storage:{pro.Name}");
                    //    continue;
                    //}
                    pageInfo.StorageDeviceUIDtosList.Add(StorageDeviceConvert.ConvertStorageDeviceDtoToUIDto(StorageDeviceConvert.ConvertStorageDeviceInfoDto(pro,true)));
                }
                var indexDDto = SettingProfileDao.Load(indexDto);
                if (indexDDto != null)
                {
                    var tempDto = StorageDeviceConvert.ConvertSettingProfileToIndexDeviceDto(indexDDto);
                    pageInfo.IndexDeviceId = tempDto.Settings;
                }
                else
                {
                    pageInfo.IndexDeviceId = null;
                }
                return pageInfo;
            }
            else
            {
                var client = new DAOAPIClientV1();
                var daoStoragePolicy = client.GetAllStoragePolicy();
                if (await LicenseHelperService.IsCloudArchivingByos())
                {
                    daoStoragePolicy = daoStoragePolicy.Where(s => s.Name.ToLowerInvariant() != RMConstants.DEFAULT_STORAGE_POLICY.ToLowerInvariant()).ToList();
                }
                foreach (var pro in daoStoragePolicy)
                {
                    StorageDeviceUIDto uiDto = new StorageDeviceUIDto();
                    if (string.IsNullOrEmpty(pageInfo.SearchValue) || (!string.IsNullOrEmpty(pageInfo.SearchValue) && pro.Name.Contains(pageInfo.SearchValue, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        uiDto.Id = pro.Id;
                        uiDto.Name = pro.Name;
                        pageInfo.StorageDeviceUIDtosList.Add(uiDto);
                    }
                }
                var globalSetting = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();
                if (daoStoragePolicy.Select(s => s.Id.ToLowerInvariant()).ToList().Contains(globalSetting.StoragePolicyId.ToString().ToLowerInvariant()))
                {
                    pageInfo.IndexDeviceId = globalSetting.StoragePolicyId.ToString();
                }
                pageInfo.TotalNumber = pageInfo.StorageDeviceUIDtosList.Count;
                return pageInfo;
            }
        }

        public StorageDeviceDto GetIndexDevice(bool needDecrypt = true)
        {
            SettingProfileDto indexDto = new SettingProfileDto()
            {
                Type = (int)SettingProfilesType.IndexDevice,
                Name = "UsingIndexDevice"
            };
            var indexSetting = SettingProfileDao.Load(indexDto);
            if (indexSetting != null)
            {
                var indexStorage = this.GetStorageDeviceById(indexSetting.Settings, needDecryptSecret: needDecrypt);
                return indexStorage;
            }
            else
            {
                return null;
            }
        }
        public StorageDeviceDto GetIndexDeviceForMigrationJob()
        {
            SettingProfileDto indexDto = new SettingProfileDto()
            {
                Type = (int)SettingProfilesType.IndexDevice,
                Name = "UsingIndexDevice"
            };
            var indexSetting = SettingProfileDao.Load(indexDto);
            if (indexSetting != null && Guid.TryParse(indexSetting.Settings, out var storageId))
            {

                var rm = StorageDeviceDao.Find(s => s.Id == storageId);
                if (rm != null)
                {
                    return StorageDeviceConvert.ConvertStorageDeviceInfoDto(rm);
                }
                else
                {
                    logger.Warn($"Cannot find storage by {storageId}");
                }
            }
            return null;
        }
        public StorageDeviceDto GetExportDevice()
        {
            SettingProfileDto indexDto = new SettingProfileDto()
            {
                Type = (int)SettingProfilesType.ExportLocationDevice,
                Name = "UsingExportLocationDevice"
            };
            var indexSetting = SettingProfileDao.Load(indexDto);
            if (indexSetting != null)
            {
                var indexStorage = this.GetStorageDeviceById(indexSetting.Settings);
                return indexStorage;
            }
            else
            {
                return null;
            }
        }
        public async Task<List<StorageDeviceUIDto>> GetAllStorageDeviceNotPagedAsync()
        {
            List<StorageDeviceUIDto> result = new List<StorageDeviceUIDto>();
            await GeneralSettingService.SaveUsingSecurityProfileAsync();
            if (TenantService.IsNewOpusTenant())
            {
                //SaveAllSecurityProfile();
                List<RMStorageDeviceInfo> phDtos = await StorageDeviceDao.GetStoragesDeviceByFilterAsync(false);
                foreach (var pro in phDtos)
                {
                    result.Add(StorageDeviceConvert.ConvertStorageDeviceDtoToUIDto(StorageDeviceConvert.ConvertStorageDeviceInfoDto(pro,true)));
                }
            }
            else
            {
                var client = new DAOAPIClientV1();
                var daoStoragePolicy = client.GetAllStoragePolicy();
                foreach (var pro in daoStoragePolicy)
                {
                    StorageDeviceUIDto uiDto = new StorageDeviceUIDto();
                    uiDto.Id = pro.Id;
                    uiDto.Name = pro.Name;
                    result.Add(uiDto);
                }
            }
            return result;

        }
        
        public async Task<List<StorageDeviceUIDto>> GetStorageDevicesIncludeGGNotPagedAsync()
        {
            List<StorageDeviceUIDto> result = new List<StorageDeviceUIDto>();
            await GeneralSettingService.SaveUsingSecurityProfileAsync();
            if (TenantService.IsNewOpusTenant())
            {
                //SaveAllSecurityProfile();
                List<RMStorageDeviceInfo> phDtos = await StorageDeviceDao.GetGoogleStoragesDeviceAsync();
                foreach (var pro in phDtos)
                {
                    result.Add(StorageDeviceConvert.ConvertStorageDeviceDtoToUIDto(StorageDeviceConvert.ConvertStorageDeviceInfoDto(pro,true)));
                }
            }
            else
            {
                var client = new DAOAPIClientV1();
                var daoStoragePolicy = client.GetAllStoragePolicy();
                foreach (var pro in daoStoragePolicy)
                {
                    StorageDeviceUIDto uiDto = new StorageDeviceUIDto();
                    uiDto.Id = pro.Id;
                    uiDto.Name = pro.Name;
                    result.Add(uiDto);
                }
            }
            return result;

        }

        public StorageDeviceDto GetStorageDeviceByDAOStoragePolicyId(string id)
        {
            try
            {
                StorageDeviceDto dto = new StorageDeviceDto();
                var rm = StorageDeviceDao.Find(s => s.Status == RMConstants.STORAGE_NEW_DATA_TYPE && s.DAOStoragePolicyId == id);
                if (rm != null)
                {
                    dto = StorageDeviceConvert.ConvertStorageDeviceInfoDto(rm);
                }
                else
                {
                    dto = null;
                    logger.Warn($"Cannot find storage by DAOStoragePolicyId:{id}");
                }

                return dto;
            }
            //EH_2
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw;
            }
        }

        public StorageDeviceDto GetStorageDeviceById(string id, bool includeData = false, bool needDecryptSecret = false)
        {
            try
            {
                StorageDeviceDto dto = new StorageDeviceDto();
                if (TenantService.IsNewOpusTenant())
                {
                    var rm = StorageDeviceDao.Find(s => s.Id.ToString().Equals(id, StringComparison.OrdinalIgnoreCase));
                    if (rm != null)
                    {
                        dto = StorageDeviceConvert.ConvertStorageDeviceInfoDto(rm,includeData);
                        if(needDecryptSecret)
                        {
                            dto.ConnectionString = DecryptGoogleStorageSecret(dto);
                        }
                    }
                    else
                    {
                        dto = null;
                        logger.Warn($"Cannot find storage by {id}");
                    }
                    //if (includeData)
                    //{
                    //    this.RefreshStorageDevice(new StorageDeviceState() { StorageDevice = dto, AutoResetEvent = null });
                    //}
                }
                else 
                {
                    var client = new DAOAPIClientV1();
                    var daoStoragePolicy = client.GetAllStoragePolicy();
                    foreach (var pro in daoStoragePolicy)
                    {
                        if (pro.Id == id)
                        {
                            dto.Id = pro.Id;
                            dto.Name = pro.Name;
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(dto.Id))
                    {
                        logger.Warn($"Can not find storage by {id}");
                    }
                }
                return dto;
            }
            //EH_2
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw;
            }
        }

        public StorageDeviceDto GetStorageDeviceByName(string name)
        {
            StorageDeviceDto dto = new StorageDeviceDto();
            if (TenantService.IsNewOpusTenant())
            {
                var rm = StorageDeviceDao.Find(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && s.Status == 0);
                if (rm == null)
                {
                    return null;
                }
                dto = StorageDeviceConvert.ConvertStorageDeviceInfoDto(rm);
            }
            else
            {
                var client = new DAOAPIClientV1();
                var daoStoragePolicy = client.GetAllStoragePolicy();
                foreach (var pro in daoStoragePolicy)
                {
                    if (pro.Name == name)
                    {
                        dto.Id = pro.Id;
                        dto.Name = pro.Name;
                        break;
                    }
                }
            }
            return dto;
        }
        public async System.Threading.Tasks.Task UpdateLastArchivedTimeAsync(string id, long lastArchivedTime)
        {
            var dto = StorageDeviceDao.Find(s => s.Id.ToString().Equals(id, StringComparison.OrdinalIgnoreCase));
            if (dto != null)
            {
                dto.LastArchivedTime = lastArchivedTime;
                await StorageDeviceDao.UpdateAsync(dto);
            }
        }
        public async Task<List<StorageDeviceDto>> GetAllAsync()
        {
            List<StorageDeviceDto> storageDtos = new List<StorageDeviceDto>();
            try
            {
                var storages = (await StorageDeviceDao.FindListAsync(s => s.Status == 0)).ToList();
                storages.ForEach(s =>
                {
                    storageDtos.Add(StorageDeviceConvert.ConvertStorageDeviceInfoDto(s));
                });

            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw;
            }
            return storageDtos;
        }

        public async Task<List<StorageDeviceDto>> GetAllAvePointStorageAsync()
        {
            List<StorageDeviceDto> storageDtos = new List<StorageDeviceDto>();
            try
            {
                var defaultStorageId = Guid.Parse(RecordsConstants.AVEPOINT_DEFAULT_STORAGEID);
                var storages = await StorageDeviceDao.FindListAsync(s => s.Status == 0 && (s.Id == defaultStorageId || s.IsSystemStorage));
                storages.ForEach(s =>
                {
                    storageDtos.Add(StorageDeviceConvert.ConvertStorageDeviceInfoDto(s));
                });

            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw;
            }
            return storageDtos;
        }

        public async Task<List<StorageDeviceDto>> GetSystemStorageAsync()
        {
            List<StorageDeviceDto> storageDtos = new List<StorageDeviceDto>();
            try
            {
                var storages = (await StorageDeviceDao.FindListAsync(s => s.IsSystemStorage && s.Status == 0)).ToList();
                storages.ForEach(s =>
                {
                    storageDtos.Add(StorageDeviceConvert.ConvertStorageDeviceInfoDto(s));
                });

            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw;
            }
            return storageDtos;
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.StorageDeviceSettings, Action = AuditAction.StorageDeviceSetIndexDevice, BeforeHandler = typeof(ArchiverSettingsBeforeAuditHandler), AfterHandler = typeof(ArchiverSettingsAfterAuditHandler))]
        public async Task<RAReturnMessage> SetUsingDeviceByIdAsync(string id, SettingProfilesType profileType, string profileName = "", bool isCompliantExport = false)
        {
            RAReturnMessage result = new RAReturnMessage() { MessageType = RAMessageType.Successful, ErrorMessage = string.Empty };
            //((int)CreateOrEditStatus.Failed).ToString()
            SettingProfileDto indexDto = new SettingProfileDto()
            {
                Id = Guid.NewGuid().ToString(),
            };
            switch (profileType)
            {
                case SettingProfilesType.IndexDevice:
                    indexDto.Name = "UsingIndexDevice";
                    indexDto.Type = (int)SettingProfilesType.IndexDevice;
                    break;
                case SettingProfilesType.ExportLocationDevice:
                    indexDto.Name = "UsingExportLocationDevice";
                    indexDto.Type = (int)SettingProfilesType.ExportLocationDevice;
                    break;
                default:
                    break;
            }
            //index file save in default storage, index device ≈ default storage
            if (SettingProfilesType.IndexDevice == profileType)
            {
                var setting = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();
                setting.StoragePolicyId = new Guid(id);
                setting.StoragePolicyName = profileName;
                await GlobalStorageSettingDao.SaveOrUpdateAsync(setting);
            }

            indexDto.Settings = id;
            var returnId = await SettingProfileDao.UpdateAsync(indexDto);
            if (string.IsNullOrEmpty(returnId))
            {
                result.MessageType = RAMessageType.Failed;
                //return ((int)CreateOrEditStatus.Failed).ToString();
                return result;
            }
            else
            {
                return result;
            }
        }
        public async Task<bool> IsDuplicateStorageDeviceNameAsync(StorageDeviceDto dto, bool isType = false)
        {
            int count = 0;
            count = (await StorageDeviceDao.FindListAsync(s => s.Name == dto.Name && s.Status == RMConstants.STORAGE_NEW_DATA_TYPE && s.Id.ToString() == dto.Id)).ToList().Count;
            return count != 0;
        }
        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.StorageDeviceSettings, Action = AuditAction.StorageDeviceDelete, BeforeHandler = typeof(ArchiverSettingsBeforeAuditHandler), AfterHandler = typeof(ArchiverSettingsAfterAuditHandler))]
        public async Task<RAReturnMessage> DeleteStorageDevicesAsync(List<string> ids)
        {
            logger.Info("DeleteStorageDevicesAsync");
            RAReturnMessage result = new RAReturnMessage() { MessageType=RAMessageType.Successful};
            try
            {
                await CheckCanDeleteStorageAsync(ids, result);
                if (result.MessageType == RAMessageType.Failed)
                {
                    return result;
                }
                foreach (string id in ids)
                {
                    await this.DeletePhysicalDeviceAsync(id);
                }
                return result;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                //logger.Log(EventSources.DocAveControlService, EventCategorys.DocAveControlService.ControlPanel_StorageConfiguration_PhysicalDevice, new EventIds.Configuration.Profile.DeleteProfileFailedEventMessage(string.Join(",", ids.ToArray()), ContextValues.Configuration.Profile.ProfileType.ControlPanel_PhysicalDevice, e));
                //result.Add(((int)CreateOrEditStatus.Failed).ToString());
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = I18NEntity.GetString("RM_AR_Storage_Unknow_ErrorMessage");
                return result;
            }
        }

        public Task<int> DeleteMigratedStorageDevicesAsync()
        {
            return StorageDeviceDao.DeleteMigratedStorageDevicesAsync();
        }


        public async System.Threading.Tasks.Task CheckCanDeleteStorageAsync(List<string> ids, RAReturnMessage result)
        {
            var allStorage = await GetAllStorageDeviceNotPagedAsync();
            List<string> notDeleteDevice = new List<string>();
            List<string> includeNotDeleteDeviceName = new List<string>();
            StringBuilder includeNotDeleteDeviceString = new StringBuilder();
            foreach (var storage in allStorage)
            {
                if (storage.SetupDataRetention)
                {
                    foreach (var rerule in storage.ArchiveRetentionRules)
                    {
                        if (!string.IsNullOrEmpty(rerule.MoveDeviceId))
                        {
                            if (!includeNotDeleteDeviceName.Contains(storage.Name))
                            {
                                includeNotDeleteDeviceName.Add(storage.Name);
                                includeNotDeleteDeviceString.Append(storage.Name + ',');
                            }
                            notDeleteDevice.Add(rerule.MoveDeviceId);
                            logger.Info($"CheckCanDeleteStorageAsync ArchiveRetentionRules using storage {rerule.MoveDeviceId}");
                        }
                    }
                }
            }

            try
            {
                //判断是否有move index job在运行，如果有运行的job, 目的端id不能被删除
                List<JobType> indexJobTypes = new List<JobType>() { JobType.ArchiverMoveIndex };
                var mIndexJobs = RMJobMonitorService.GetRunningJobs(indexJobTypes);
                foreach (var job in mIndexJobs)
                {
                    logger.Info($"CheckCanDeleteStorageAsync move index job using storage {job.ScopeId}");
                    notDeleteDevice.Add(job.ScopeId);

                    var temp = allStorage.Where(s => s.Id.Equals(job.ScopeId, StringComparison.CurrentCultureIgnoreCase));
                    if (temp != null && temp.FirstOrDefault() != null)
                    {
                        result.MessageType = RAMessageType.Failed;
                        result.ErrorMessage = string.Format(I18NEntity.GetString("RM_AR_StorageHasRanArchiverJob_Delete_ErrorMessage"), temp.FirstOrDefault().Name);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"CheckCanDeleteStorageAsync move index job error {e}");
            }

            var indexDDto = SettingProfileDao.LoadByType(SettingProfilesType.IndexDevice);
            if (indexDDto != null)
            {
                var tempDto = StorageDeviceConvert.ConvertSettingProfileToIndexDeviceDto(indexDDto);
                notDeleteDevice.Add(tempDto.Settings);
            }

            List<string> notDeletedId = new List<string>();
            foreach (string id in ids)
            {
                var storage = GetStorageDeviceById(id);
                if (notDeleteDevice.Contains(id))
                {
                    result.MessageType = RAMessageType.Failed;
                    result.ErrorMessage = string.Format(I18NEntity.GetString("RM_AR_Storage_Delete_ErrorMessage"), storage.Name, includeNotDeleteDeviceString.ToString().TrimEnd(','));
                    return;
                }
                var exportLocation = SettingProfileDao.LoadByType(SettingProfilesType.ExportLocationDevice);
                string exportStorageId = exportLocation == null ? string.Empty : exportLocation.Settings;
                if (exportStorageId == id)
                {
                    result.MessageType = RAMessageType.Failed;
                    result.ErrorMessage = string.Format(I18NEntity.GetString("RM_AR_ExportStorage_Delete_ErrorMessage"), storage.Name);
                    return;
                }
                var hasRanArchiverJobDevice = ArchiverIndexSubInfoDao.FindAll().Where(a => a.StorageId == id || a.CurrentStorageId == id).ToList();
                if (hasRanArchiverJobDevice != null && hasRanArchiverJobDevice.Count > 0)
                {
                    result.MessageType = RAMessageType.Failed;
                    result.ErrorMessage = string.Format(I18NEntity.GetString("RM_AR_StorageHasRanArchiverJob_Delete_ErrorMessage"), storage.Name);
                    return;
                }
                var allRules = RuleManagerService.GetRulesFromRecords();

                // all rules that can use storage for exporting or moving content in action or in export before action option
                // SPO, OD, EXO, Phy, FS, Teams
                var temp = allRules
                        .Where(r =>
                            IsMatchRule(r, id) ||
                            IsMatchRule(r.OneDriveRule, id) ||
                            IsMatchRule(r.EXORule, id) ||
                            IsMatchRule(r.PhysicalRule, id) ||
                            IsMatchRule(r.FSRule, id) ||
                            IsMatchRule(r.TeamsRule, id)
                        )
                        .Select(r => r.Name)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (temp != null && temp.Count > 0)
                {
                    result.MessageType = RAMessageType.Failed;
                    var names = string.Join(", ", temp);
                    result.ErrorMessage = string.Format(I18NEntity.GetString("RM_AR_StorageRule_Delete_ErrorMessage"), storage.Name, names);
                    return;
                }
            }
        }

        private bool IsMatchRule(Rule rule, string id)
        {
            return rule?.SOFilters?.Count > 0 &&
                   (rule.StoragePolicyId?.Equals(id, StringComparison.OrdinalIgnoreCase) == true
                   || rule.ExportInfo?.exportLocationId?.Equals(id, StringComparison.OrdinalIgnoreCase) == true);
        }

        public async System.Threading.Tasks.Task DeletePhysicalDeviceAsync(string id)
        {
            string name = string.Empty;
            try
            {
                if (id.Equals(RecordsConstants.AVEPOINT_DEFAULT_STORAGEID, StringComparison.OrdinalIgnoreCase))
                {
                    logger.Error("The default storage cannot be deleted.");
                }
                else
                {
                    StorageDeviceDto oldDto = this.GetStorageDeviceById(id);
                    name = oldDto.Name;
                    oldDto.Status = RMConstants.STORAGE_OLD_DATA_TYPE;
                    await UpdateStorageDeviceInfoAsync(oldDto);
                }
                //physicalDeviceDao.Update(oldDto);
                //this.StartExecuteSendCacheInfo();
                //logger.Log(EventSources.DocAveControlService, EventCategorys.DocAveControlService.ControlPanel_StorageConfiguration_PhysicalDevice, new EventIds.Configuration.Profile.DeleteProfileSuccessfullyEventMessage(name, ContextValues.Configuration.Profile.ProfileType.ControlPanel_PhysicalDevice));
            }
            catch (Exception ex)
            {
                //logger.Log(EventSources.DocAveControlService, EventCategorys.DocAveControlService.ControlPanel_StorageConfiguration_PhysicalDevice, new EventIds.Configuration.Profile.DeleteProfileFailedEventMessage(name, ContextValues.Configuration.Profile.ProfileType.ControlPanel_PhysicalDevice, ex));
                logger.Error($"DeletePhysicalDeviceAsync error {ex}");
            }
        }

        public async Task<RAReturnMessage> CheckAzureRegion(string accessPoint, string accountName, string storageDeviceId)
        {
            RAReturnMessage status = new RAReturnMessage() { MessageType = RAMessageType.Successful, ErrorMessage = string.Empty };
            try
            {
                if (RMGlobalConfiguration.EnvSetting.IsGCPEnvironment)
                {
                    logger.Info("GCP environment skip to check azure region");
                    return status;
                }
                StorageDeviceDto dto = GetStorageDeviceById(storageDeviceId);
                if (dto != null && accessPoint == dto.mCurrentXRI.Params["accesspoint"] && accountName == dto.mCurrentXRI.Params["name"])
                {
                    return status;
                }
                IPAddress host = ParseHost(accessPoint, status, accountName);
                if (status.MessageType != RAMessageType.Successful)
                {
                    return status;
                }
                await InitAzureRegionDic();
                ValidAzureRegion(status, host);
            }
            catch (Exception ex)
            {
                logger.Error(@$"Fail check Valid azure region,ex:{ex}");
                status.MessageType = RAMessageType.Exception;
                status.ErrorMessage = I18NEntity.GetString("RM_AR_Storage_Unknow_ErrorMessage");
            }
            return status;
        }

        public string GetAzureRegionOfDataCenter()
        {
            return StorageManagerUtil.GetAzureRegionOfDataCenter();
        }

        private void ValidAzureRegion(RAReturnMessage status, IPAddress host)
        {
            string azureRegion = StorageManagerUtil.GetAzureRegionOfDataCenter();

            if (azureRegionDic.ContainsKey(azureRegion.Trim()))
            {
                foreach (IPNetwork iPNetwork in azureRegionDic[azureRegion])
                {
                    if (iPNetwork.Contains(host))
                    {
                        status.MessageType = RAMessageType.Successful;
                        return;
                    }
                }
            }
            status.MessageType = RAMessageType.Failed;
            status.ErrorMessage = I18NEntity.GetString("RM_AR_Storage_DC_Unmatch_WarnMessage");
        }

        public string GetAzureAccessPointUrl(string accessPoint, string accountName)
        {
            if (accessPoint.Equals("https://blob.core.windows.net", StringComparison.CurrentCultureIgnoreCase))
            {
                accessPoint = "https://" + accountName + ".blob.core.windows.net";
            }
            return accessPoint;
        }

        private IPAddress ParseHost(string accessPoint, RAReturnMessage status, string accountName)
        {
            try
            {
                if(accessPoint.Equals("https://blob.core.windows.net", StringComparison.CurrentCultureIgnoreCase))
                {
                    accessPoint = "https://"+accountName+".blob.core.windows.net";
                }
                if (accessPoint.StartsWith("https://"))
                {
                    accessPoint = accessPoint.Substring("https://".Length);
                }
                return Dns.GetHostAddresses(accessPoint).First();
            }
            catch (Exception e)
            {
                logger.Error(@$"Fail parse accessPoint host,ex:{e}");
                status.MessageType = RAMessageType.Failed;
                status.ErrorMessage = I18NEntity.GetString("RM_AR_Storage_DC_Unmatch_WarnMessage");
                return null;
            }
        }



        private static async Task InitAzureRegionDic()
        {
            if ((DateTime.Now - azureRegionRefreashTime) < new TimeSpan(1, 0, 0)
                            && !string.IsNullOrWhiteSpace(azureRegionFileUrl) && !(azureRegionDic == null))
            {
                return;
            }
            string enviromentName = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME];
            string azurePageUrl = StorageManagerUtil.GetDownloadAzureIpRangesPageUrlByEnviroment(enviromentName);
            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            var clientHelper = HttpClientHelper.CreateWithRemoteCertificateValidation(null);
            HttpResponseMessage azurePageResponseMeaasge = await clientHelper.GetAsync(new HttpRequestMessage(HttpMethod.Get, azurePageUrl), cancellationTokenSource.Token);
            string azurePageHtml = await azurePageResponseMeaasge.Content.ReadAsStringAsync();
            int startIndex = azurePageHtml.IndexOf("https://download");
            string downloadUrl = azurePageHtml.Substring(startIndex, azurePageHtml.IndexOf(".json", startIndex) + 5 - azurePageHtml.IndexOf("https://download"));
            if (downloadUrl.Equals(azureRegionFileUrl) && !(azureRegionDic == null))
            {
                return;
            }
            HttpResponseMessage downloadResponseMessage = await clientHelper.GetAsync(new HttpRequestMessage(HttpMethod.Get, downloadUrl), cancellationTokenSource.Token);
            string azureRegionContext = await downloadResponseMessage.Content.ReadAsStringAsync();
            azureRegionDic = GetAzureRegionDic(azureRegionContext);
            azureRegionFileUrl = downloadUrl;
            azureRegionRefreashTime = DateTime.Now;
        }

        private static Dictionary<string, HashSet<IPNetwork>> GetAzureRegionDic(string azureRegionContext)
        {
            Dictionary<string, HashSet<IPNetwork>> res = new Dictionary<string, HashSet<IPNetwork>>();
            var download = SerializerHelper.DeserializeByJsonConvert<AzureRegionDetils>(azureRegionContext);
            if (download != null)
            {
                var items = download.Values.Where(s => s.Name.StartsWith("Storage.", StringComparison.OrdinalIgnoreCase)
                && s.Properties.SystemService.Equals("AzureStorage", StringComparison.OrdinalIgnoreCase));
                if (items.Any())
                {
                    foreach (var item in items)
                    {
                        res[item.Properties.Region] = new HashSet<IPNetwork>();
                        if (!item.Properties.AddressPrefixes.IsNullOrEmpty())
                        {
                            foreach (var v in item.Properties.AddressPrefixes)
                            {
                                res[item.Properties.Region].Add(IPNetwork.Parse(v));
                            }
                        }
                    }
                }
            }
            return res;
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.StorageDeviceSettings, Action = AuditAction.StorageDeviceCreate, BeforeHandler = typeof(ArchiverSettingsBeforeAuditHandler), AfterHandler = typeof(ArchiverSettingsAfterAuditHandler))]
        public async Task<RAReturnMessage> ValidateAndCreateStorageDeviceAsync(StorageDeviceDto dto, EntityObjectPermissionType permission)
        {
            try
            {
                RAReturnMessage status = new RAReturnMessage() { MessageType = RAMessageType.Successful, ErrorMessage = string.Empty};
                if (dto.ArchiveRetentionRules != null && dto.ArchiveRetentionRules.Count == 1)
                {
                    logger.Info("this storage has only one retention rule,need to valid keep data type");
                    if (ValidEnableFileLevelBackup(dto.ArchiveRetentionRules[0]))
                    {
                        status.MessageType = RAMessageType.Failed;
                        status.ErrorMessage = I18NEntity.GetString("Can not save success because fault param!");
                        return status;
                    }
                }
                if (dto.ArchiveRetentionRules != null && dto.ArchiveRetentionRules.Count > 0)
                {
                    bool hasMoveRule = dto.ArchiveRetentionRules.Any(r => r.IsMove);
                    if (hasMoveRule && !KeyValueDao.IsEnableMoveToAnotherLocation())
                    {
                        status.MessageType = RAMessageType.Failed;
                        status.ErrorMessage = I18NEntity.GetString("RM_Retention_MoveToAnotherLocationDisabled");
                        return status;
                    }
                }
                if (TenantService.IsNewOpusTenant())
                {
                    logger.Info("Storage Device Name : " + dto.Name);
                    if (!ValidateStorageDeviceName(dto))
                    {
                        status.MessageType= RAMessageType.Failed;
                        status.ErrorMessage = I18NEntity.GetString("RM_AR_Stub_Name_ErrorMessage"); ;
                        return status;
                    }
                    var mdto = this.GetStorageDeviceById(dto.Id);
                    if (dto.Id != null && mdto != null)
                    {
                        if (dto.IsSystemStorage)
                        {
                            // System storage uses Managed Identity, skip connection validation
                            logger.Info($"[ValidateAndCreate] System storage detected, skip ValidateStorageDeviceSpace.");
                        }
                        else if (mdto.Password != null)
                        {
                            for (int i = 0; i < mdto.Password.Count; i++)
                            {
                                if (!mdto.Password[i].Equals(dto.Password[i]))
                                {
                                    EncryptPasswordForValidation(dto);
                                    status = ValidateStorageDeviceSpace(dto); //need revert
                                }
                                else
                                {
                                    status = ValidateStorageDeviceSpace(dto);//need revert
                                }
                            }
                        }
                        else
                        {
                            status = ValidateStorageDeviceSpace(dto); //need revert
                        }
                    }
                    else
                    {
                        EncryptPasswordForValidation(dto);
                        status = ValidateStorageDeviceSpace(dto);//need revert
                    }
                    if (!string.IsNullOrEmpty(dto.EncryptionProfileId))
                    {
                        var profileIds = PortalUtil.GetSecurityProfilesSummary(AvePoint.RA.Contract.Tenant.TenantLocalValue.LogonGroupId).SecurityProfiles;
                        List<string> ids = new List<string>();
                        foreach (var temp in profileIds)
                        {
                            ids.Add(temp.Id);
                        }
                        if (ids.Contains(dto.EncryptionProfileId))
                        {
                            PortalUtil.UpdateSecurityProfileInUse(dto.EncryptionProfileId, true);
                        }
                        else
                        {
                            status.MessageType=RAMessageType.Failed;
                            return status;
                        }
                    }
                    if (status.MessageType==RAMessageType.Successful)
                    {
                        dto.AuditId = dto.Id;
                        if (dto.Id == null || mdto == null)
                        {
                            status = await this.CreateStorageDeviceAsync(dto, permission);
                            //logger.Info(ControlPanelResource.PhysicalDeviceService_Create_physicl_device_successful);
                        }
                        else
                        {
                            status = await this.UpdateStorageDeviceAsync(dto);
                            //logger.Info(ControlPanelResource.PhysicalDeviceService_Update_physicl_device_successful);
                        }
                    }
                    return status;
                }
                else
                {
                    status.MessageType=RAMessageType.Failed;
                    return status;
                }
            }
            //EH_2
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                if (dto.Type == (int)StorageDeviceType.Google && (e is FormatException || e.Message.Contains("PKCS8 data must be contained within '-----BEGIN PRIVATE KEY-----' and '-----END PRIVATE KEY-----")))
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_AR_Storage_Private_ID_Incorrect") };
                }
                //logger.Log(EventSources.DocAveControlService, EventCategorys.DocAveControlService.ControlPanel_StorageConfiguration_PhysicalDevice, new EventIds.Configuration.Profile.ModifyProfileFailedEventMessage(dto.Name, ContextValues.Configuration.Profile.ProfileType.ControlPanel_PhysicalDevice, e));
                //return new List<string>() { "", ((int)XSystemHealth.AuthenticationFailed).ToString() };
                return  new RAReturnMessage(){ MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_AR_Storage_Unknow_ErrorMessage") };
            }
        }
        public async Task<GOReturnMessage> ValidateAndCreateStorageDeviceAsyncForGoogleOne(StorageDeviceDto dto, EntityObjectPermissionType permission)
        {
            try
            {
                GOReturnMessage status = new GOReturnMessage() { MessageType = RAMessageType.Successful, ErrorMessage = string.Empty };
                if (dto.ArchiveRetentionRules != null && dto.ArchiveRetentionRules.Count == 1)
                {
                    logger.Info("this storage has only one retention rule,need to valid keep data type");
                    if (ValidEnableFileLevelBackup(dto.ArchiveRetentionRules[0]))
                    {
                        status.MessageType = RAMessageType.Failed;
                        status.ErrorMessage = I18NEntity.GetString("Can not save success because fault param!");
                        return status;
                    }
                }
                if (TenantService.IsNewOpusTenant())
                {
                    logger.Info("Storage Device Name : " + dto.Name);
                    if (!ValidateStorageDeviceName(dto))
                    {
                        status.MessageType = RAMessageType.Failed;
                        status.ErrorMessage = I18NEntity.GetString("RM_AR_Stub_Name_ErrorMessage"); ;
                        return status;
                    }
                    if (dto.Id != null)
                    {
                        var mdto = this.GetStorageDeviceById(dto.Id);
                        if (mdto.Password != null)
                        {
                            for (int i = 0; i < mdto.Password.Count; i++)
                            {
                                if (!mdto.Password[i].Equals(dto.Password[i]))
                                {
                                    EncryptPasswordForValidation(dto);
                                    status = ValidateStorageDeviceSpaceForGoogleOne(dto); //need revert
                                }
                                else
                                {
                                    status = ValidateStorageDeviceSpaceForGoogleOne(dto);//need revert
                                }
                            }
                        }
                        else
                        {
                            status = ValidateStorageDeviceSpaceForGoogleOne(dto); //need revert
                        }
                    }
                    else
                    {
                        EncryptPasswordForValidation(dto);
                        status = ValidateStorageDeviceSpaceForGoogleOne(dto);//need revert
                    }
                    if (!string.IsNullOrEmpty(dto.EncryptionProfileId))
                    {
                        var profileIds = PortalUtil.GetSecurityProfilesSummary(AvePoint.RA.Contract.Tenant.TenantLocalValue.LogonGroupId).SecurityProfiles;
                        List<string> ids = new List<string>();
                        foreach (var temp in profileIds)
                        {
                            ids.Add(temp.Id);
                        }
                        if (ids.Contains(dto.EncryptionProfileId))
                        {
                            PortalUtil.UpdateSecurityProfileInUse(dto.EncryptionProfileId, true);
                        }
                        else
                        {
                            status.MessageType = RAMessageType.Failed;
                            return status;
                        }
                    }
                    if (status.MessageType == RAMessageType.Successful)
                    {
                        dto.AuditId = dto.Id;
                        if (dto.Id == null)
                        {
                            status = await this.CreateStorageDeviceAsyncForGoogleOne(dto, permission);
                        }
                        else
                        {
                            status = await this.UpdateStorageDeviceAsyncForGoogleOne(dto);
                        }
                        await TryToSetDefaultIndex(dto);
                    }
                    return status;
                }
                else
                {
                    status.MessageType = RAMessageType.Failed;
                    return status;
                }
            }
            //EH_2
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                if (dto.Type == (int)StorageDeviceType.Google && (e is FormatException || e.Message.Contains("PKCS8 data must be contained within '-----BEGIN PRIVATE KEY-----' and '-----END PRIVATE KEY-----")))
                {
                    return new GOReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_AR_Storage_Private_ID_Incorrect") };
                }
                return new GOReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_AR_Storage_Unknow_ErrorMessage") };
            }
        }
        private async Task TryToSetDefaultIndex(StorageDeviceDto dto)
        {
            try
            {
                logger.Info($"try to get storage device by id {dto.Id}");
                var storageDto = this.GetStorageDeviceById(dto.Id);
                if (storageDto != null)
                {
                    logger.Info($"try to get index device.");
                    var currentIndex = this.GetIndexDevice();
                    if (currentIndex == null)
                    {
                        logger.Info($"not found index device.");
                        await this.SetUsingDeviceByIdAsync(storageDto.Id, SettingProfilesType.IndexDevice, storageDto.Name);
                        logger.Info($"set index device by id {dto.Id} successful.");
                    }
                }
            }
            catch(Exception ex)
            {                 
                logger.Error($"TryToSetDefaultIndex error : {ex}");
            }
        }
        private bool ValidEnableFileLevelBackup(RetentionRule ArchiveRetentionRule)
        {
            if (ArchiveRetentionRule != null && ArchiveRetentionRule.RetentionDataTimeType == KeepDateType.ModifiedTime)
            {
                if (int.TryParse(KeyValueDao.GetValueByKey(RMKeyValuesConstants.ArchiverBackupOutputStreamLevel)?.Value, out var outputStreamLevel))
                {
                    if (outputStreamLevel != (int)OutputStreamLevel.FileLevel)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
        private bool ValidateStorageDeviceName(StorageDeviceDto dto)
        {
            var temp = GetStorageDeviceByName(dto.Name);
            if (temp == null || temp.Id == dto.Id)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task<bool> IsInArchiverMigrating()
        {
            try
            {
                bool.TryParse(KeyValueDao.GetValueByKey("RunDisposalInRecords")?.Value, out bool isNewOpus);
                if (isNewOpus)
                {
                    return false;
                }
                var runningJobs = RMJobMonitorService.GetRunningJobs(JobType.CloudArchiverMigration);
                if (runningJobs.Count > 0)
                {
                    return true;
                }

                var client = new DAOAPIClientV1(true);
                var isInMigrating = await client.IsArchiverMigrating();
                logger.Info($"get is mgrating from DAO : {isInMigrating}");

                return isInMigrating;
            }
            catch (Exception ex)
            {
                logger.Error($"check tenant is in migrating error: {ex}");
            }
            return false;
        }

        public async System.Threading.Tasks.Task CreateDefaultStorageDeviceAsync()
        {
            try
            {
                var storage = GetStorageDeviceById(RecordsConstants.AVEPOINT_DEFAULT_STORAGEID);
                //If AvePoint Storage has already been migrated from Dao, Opus will no longer create AvePoint Storage.
                if (storage == null)
                {
                    var isInMigrating = await IsInArchiverMigrating();
                    if(isInMigrating)
                    {
                        logger.Info($"Tenant is in mgrating. skip to create default storage device");
                        return;
                    }
                    var migrationSystemStorage = (await GetSystemStorageAsync()).FirstOrDefault();
                    if (migrationSystemStorage != null)
                    {
                        logger.Warn($"Get System Storage, Name:[{migrationSystemStorage?.Name}], so AvePoint Storage will no longer be created.");
                        storage = migrationSystemStorage;
                    }
                }
                if (storage == null || string.IsNullOrEmpty(storage.Id))
                {
                    var isGcpEnv = RMGlobalConfiguration.EnvSetting.IsGCPEnvironment;
#if DEBUG
                    string defaultConnectionString = CommonUtilityForSpecialTenant.GetStorageConnectionStringFromConfigFile(StorageStringType.DefaultStorage);
                    if (string.IsNullOrEmpty(defaultConnectionString))
                    {
                        throw new ArgumentNullException("DEFAULT_STORAGE_CONNECTION_STRING");
                    }
#endif

                    StorageDeviceDto defaultStorageDeviceDto = new StorageDeviceDto();
                    defaultStorageDeviceDto.Type = isGcpEnv ? 14 : 403;// 403 : Azure storage type, 14 : Google storage type
                    defaultStorageDeviceDto.Name = DEFAULTSTORAGENAME;
                    defaultStorageDeviceDto.Id = RecordsConstants.AVEPOINT_DEFAULT_STORAGEID;
                    defaultStorageDeviceDto.ConnectionString = GetDefaultStorageConnectionString();
                    defaultStorageDeviceDto.SetupDataRetention = false;
                    defaultStorageDeviceDto.ArchiveRetentionRules = new List<RetentionRule>() { new RetentionRule() { DeleteTheData = true, ArchiveDateUnit = DateUnit.Week } };
                    defaultStorageDeviceDto.IsUsingDevice = true;
                    //if (isGcpEnv)
                    //    defaultStorageDeviceDto.ConnectionString = DecryptGoogleStorageSecret(defaultStorageDeviceDto, true);
                    if (!isGcpEnv)
                    {
                        EncryptPasswordForValidation(defaultStorageDeviceDto);
                    }
                    var status = await this.CreateStorageDeviceAsync(defaultStorageDeviceDto, EntityObjectPermissionType.FullPermission);
                    logger.Info($"CreateDefaultStorageDeviceAsync successful.");
                }
            }
            catch(Exception e)
            {
                logger.Error($"CreateDefaultStorageDeviceAsync error : {e}");
            }
        }

        public string GetDefaultStorageConnectionString()
        {
            string defaultConnectionString = CommonUtilityForSpecialTenant.GetStorageConnectionStringFromConfigFile(StorageStringType.DefaultStorage);
            if (RMGlobalConfiguration.EnvSetting.IsGCPEnvironment)
            {
                return AzureUtil.GetGoogleConnectionBuilderString(defaultConnectionString, TenantLocalValue.LogonGroupId);
            }
            return AzureUtil.GetConnectionBuilderString(defaultConnectionString, TenantLocalValue.LogonGroupId);
        }

        private IDictionary<string, string> ParseStringIntoSettings(string connectionString)
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>();
            string[] array = connectionString.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < array.Length; i++)
            {
                string[] array2 = array[i].Split(new char[1] { '=' }, 2);
                if (array2.Length != 2)
                {
                    logger.Warn("Settings must be of the form \"name=value\".");
                    return null;
                }

                if (dictionary.ContainsKey(array2[0]))
                {
                    logger.Warn(string.Format(CultureInfo.InvariantCulture, "Duplicate setting '{0}' found.", array2[0]));
                    return null;
                }

                dictionary.Add(array2[0], array2[1]);
            }

            return dictionary;
        }

        public long GetArchiverStorageGBSize()
        {
            long mailboxSize = EXOArhciverSubInfo.GetArchiverStorageGBSize();
            long spSize = ArchiverIndexSubInfoDao.GetArchiverStorageGBSize();
            long totalSize = mailboxSize + spSize;
            this.logger.Info($"GetArchiverStorageGBSize mailboxSize:{mailboxSize},spSize:{spSize},totalSize:{totalSize}");
            return totalSize;
        }

        public long GetAOSPArchiverStorageGBSize()
        {
            long spSize = ArchiverIndexSubInfoDao.GetAOSPArchiverStorageGBSize();
            long totalSize = spSize;
            this.logger.Info($"GetAOSPArchiverStorageGBSize spSize:{spSize},totalSize:{totalSize}");
            return totalSize;
        }

        private void EncryptPasswordForValidation(StorageDeviceDto mDto)
        {
            if (mDto != null && mDto.Password != null && mDto.Password.Count > 0)
            {
                if (!string.IsNullOrEmpty(mDto.Id) && IsSixStar(mDto))
                {
                    return;
                }
                mDto.UpdatePassword(EncryptPassword(mDto.Password));
                mDto.IsEncryptPassword = true;
            }
        }

        private List<string> EncryptPassword(List<string> passwords)
        {
            List<string> newPasswords = new List<string>();
            for (int i = 0; i < passwords.Count; i++)
            {
                if (!passwords[i].EndsWith(RMConstants.PASSWORD_RETURN_VALUE, StringComparison.OrdinalIgnoreCase))
                {
                    string[] keyValue = passwords[i].Split(new char[] { '=' });
                    if (!keyValue[0].EndsWith("tokensecret"))
                    {
                        keyValue[1] = PhysicalDeviceDto.XRIUtil.ValueEncode(Crypto.WrapKey(PhysicalDeviceDto.XRIUtil.ValueDecode(keyValue[1])));
                    }
                    newPasswords.Add(keyValue[0] + "=" + keyValue[1]);
                }
                else
                {
                    newPasswords.Add(passwords[i]);
                }
            }
            return newPasswords;
        }
        private bool IsSixStar(StorageDeviceDto dto)
        {
            return dto.Password.All(p => SecurityUtils.IsDefaultRMConstantsReturnValue(p));
        }

        public RAReturnMessage ValidateStorageDeviceSpace(StorageDeviceDto dto)
        {
            RAReturnMessage error = new RAReturnMessage() { MessageType = RAMessageType.Successful, ErrorMessage = string.Empty };
            this.deviceSystem = XFactoryCommon.InstanceSystem(DecryptGoogleStorageSecret(dto));
            this.deviceSystem.Open();
            var result = this.deviceSystem.Validate();
            if (result.SystemHealth == XSystemHealth.AvailableAndNotFull
             || result.SystemHealth == XSystemHealth.Available)
            {
                if (result.TotalFreeSpace > 1024 * 1024 * 1024)  //>1g
                {
                    this.logger.Info($"Validate {dto.mCurrentXRI.VIM} successfully.");
                }
                else
                {
                    this.logger.Info($"Validate {dto.mCurrentXRI.VIM} successfully,but the total free space is not enough 1gb");
                    //errorList.Add(((int)CreateOrEditStatus.Failed).ToString());
                    error.MessageType=RAMessageType.Failed;
                    error.ErrorMessage = I18NEntity.GetString("RM_AR_Storage_SpaceNotEnough_ErrorMessage");
                }
            }
            else
            {
                error.MessageType = RAMessageType.Failed;
                error.ErrorMessage = I18NEntity.GetString("RM_AR_Storage_Account_ErrorMessage");
            }
            return error;
        }

        public GOReturnMessage ValidateStorageDeviceSpaceForGoogleOne(StorageDeviceDto dto)
        {
            GOReturnMessage error = new GOReturnMessage() { MessageType = RAMessageType.Successful, ErrorMessage = string.Empty };
            this.deviceSystem = XFactoryCommon.InstanceSystem(DecryptGoogleStorageSecret(dto));
            this.deviceSystem.Open();
            var result = this.deviceSystem.Validate();
            if (result.SystemHealth == XSystemHealth.AvailableAndNotFull
             || result.SystemHealth == XSystemHealth.Available)
            {
                if (result.TotalFreeSpace > 1024 * 1024 * 1024)  //>1g
                {
                    this.logger.Info($"Validate {dto.mCurrentXRI.VIM} successfully.");
                }
                else
                {
                    this.logger.Info($"Validate {dto.mCurrentXRI.VIM} successfully,but the total free space is not enough 1gb");
                    //errorList.Add(((int)CreateOrEditStatus.Failed).ToString());
                    error.MessageType = RAMessageType.Failed;
                    error.ErrorMessage = I18NEntity.GetString("RM_AR_Storage_SpaceNotEnough_ErrorMessage");
                }
            }
            else
            {
                error.MessageType = RAMessageType.Failed;
                error.ErrorMessage = I18NEntity.GetString("RM_AR_Storage_Account_ErrorMessage");
            }
            return error;
        }
        private string DecryptGoogleStorageSecret(StorageDeviceDto dto)
        {
            string begin = "-----BEGIN PRIVATE KEY-----";
            string end = "-----END PRIVATE KEY-----";
            if (dto.mCurrentXRI != null && dto.mCurrentXRI.VIM == "google_vim" && !dto.IsSystemStorage)
            {
                string[] keyValue = dto.Password[0].Split(new char[] { '=' });
                if (!keyValue[0].EndsWith("tokensecret") && !(keyValue[1].StartsWith(begin) && keyValue[1].Contains(end)))
                {
                    keyValue[1] = PhysicalDeviceDto.XRIUtil.ValueEncode(Crypto.UnWrapKey(PhysicalDeviceDto.XRIUtil.ValueDecode(keyValue[1])));
                }
                dto.IsEncryptPassword = false;
                return dto.ConnectionString.Replace(dto.Password[0], keyValue[0] + "=" + keyValue[1]);
            }
            return dto.ConnectionString;
        }
        public async System.Threading.Tasks.Task UpgradeAvePointStorageToManagedIdentityAsync()
        {
            try
            {
                if (RMGlobalConfiguration.EnvSetting.IsDevEnvironment)
                {
                    logger.Info($"[UpgradeMI] Dev environment detected, skip.");
                    return;
                }

                var isGcpEnv = RMGlobalConfiguration.EnvSetting.IsGCPEnvironment;
                if (isGcpEnv)
                {
                    logger.Info($"[UpgradeMI] GCP environment detected, skip.");
                    return;
                }

                // Get all storages need to upgrade: default ID + migration storages
                var storagesToUpgrade = new List<StorageDeviceDto>();

                // Case 1: get by fixed ID
                var defaultStorage = GetStorageDeviceById(RecordsConstants.AVEPOINT_DEFAULT_STORAGEID);
                if (defaultStorage != null && !string.IsNullOrEmpty(defaultStorage.ConnectionString))
                {
                    storagesToUpgrade.Add(defaultStorage);
                }

                // Case 2: get migration storages
                var migrationStorages = await GetSystemStorageAsync();
                if (migrationStorages?.Count > 0)
                {
                    storagesToUpgrade.AddRange(migrationStorages);
                }

                if (storagesToUpgrade.Count == 0)
                {
                    logger.Info($"[UpgradeMI] No storage found to upgrade, skip.");
                    return;
                }

                foreach (var storage in storagesToUpgrade)
                {
                    var nameMatch = Regex.Match(storage.ConnectionString,
                        @"[&?]name=([^&]+)", RegexOptions.IgnoreCase);
                    if (!nameMatch.Success)
                    {
                        logger.Warn($"[UpgradeMI] Storage [{storage.Id}] cannot parse account name, skip.");
                        continue;
                    }
                    var accountName = nameMatch.Groups[1].Value;

                    var secretMatch = Regex.Match(storage.ConnectionString,
                        @"secret=([^&]*)", RegexOptions.IgnoreCase);
                    if (!secretMatch.Success || string.IsNullOrEmpty(secretMatch.Groups[1].Value))
                    {
                        logger.Info($"[UpgradeMI] Storage [{storage.Id}] already Managed Identity, skip.");
                        continue;
                    }

                    // Convert to Managed Identity
                    var newConnString = Regex.Replace(
                        storage.ConnectionString,
                        @"accesspoint=https://blob\.core\.windows\.net",
                        $"accesspoint=https://{accountName}.blob.core.windows.net",
                        RegexOptions.IgnoreCase);

                    newConnString = Regex.Replace(
                        newConnString,
                        @"secret=[^&]*",
                        "secret=",
                        RegexOptions.IgnoreCase);

                    // Backup old record
                    var oldStorage = JsonConvert.DeserializeObject<StorageDeviceDto>(
                        JsonConvert.SerializeObject(storage));
                    oldStorage.Id = null;
                    oldStorage.Status = RMConstants.STORAGE_OLD_DATA_TYPE;
                    oldStorage.BackupPhysicalDeviceId = storage.Id;
                    StorageDeviceDao.Create(oldStorage);

                    // Update new record
                    storage.ConnectionString = newConnString;
                    storage.ModifyTime = DateTime.UtcNow.Ticks;
                    await UpdateStorageDeviceInfoAsync(storage);

                    logger.Info($"[UpgradeMI] Upgraded successfully. storageId={storage.Id}, accountName={accountName}");
                }
            }
            catch (Exception e)
            {
                logger.Error($"[UpgradeMI] UpgradeAvePointStorageToManagedIdentityAsync error: {e}");
            }
        }        
        //private async System.Threading.Tasks.Task DoRunAsync(object obj)
        //{
        //    StorageDeviceDto dto = obj as StorageDeviceDto;
        //    //由于这个方法在Save的时候进行调用，并且在Save的时候对密码进行了加密，
        //    //因此此处代码虽然传递的是Physical Device对象，但是仍然会通过这个对象的Id重新到数据库里面重新将这个对象取出。
        //    dto = this.GetStorageDeviceById(dto.Id);
        //    if (dto != null)
        //    {
        //        try
        //        {
        //            string str = dto.BuildValidateXRI();
        //            IXSystem xSystem = XFactoryCommon.InstanceSystem(str);
        //            //bool IsUpdateDeviceMode = false;  //这个属性判断，是否修改了DeviceMode
        //            //dto.DeviceMode = RMConstants.STORAGE_DEVICE_DATA_ONLINE;
        //            await UpdateStorageDeviceInfoAsync(dto);
        //            //logger.Debug(ControlPanelResource.ControlPanel_Save_physical_device_info_successfully_);//"Save physical device info successfully."
        //            //logger.Debug(string.Format(ControlPanelResource.PhysicalDeviceServiceUsedSpaceTotalSpace, dto.Extension.UsedSpace, dto.Extension.TotalSpace));
        //        }
        //        catch (Exception ex)
        //        {
        //            //logger.Error(ControlPanelResource.ControlPanel_An_error_occured_while_executing_the_operation_, ex);//"An error occured while executing the operation."
        //        }
        //    }
        //}
        public async Task<DevicesResult> GetStorageIdAndNameAsync(bool IsFilter)
        {
            var result = new DevicesResult() { StorageIdAndNameList = new List<StorageIdAndName>() };
            var storageFilterResult= await StorageDeviceDao.GetStoragesDeviceByFilterAsync(IsFilter);
            result.StorageIdAndNameList = new List<StorageIdAndName>();
            foreach (var storage in storageFilterResult)
            {
                result.StorageIdAndNameList.Add(new StorageIdAndName() { Id = storage.Id.ToString(), Name = storage.Name, Type = storage.Type });
            }
            result.EnableMoveToAnotherLocation = KeyValueDao.IsEnableMoveToAnotherLocation();
            return result;
        }
        private Task<string> UpdateStorageDeviceInfoAsync(StorageDeviceDto dto)
        {
            return StorageDeviceDao.UpdateAsync(dto);
        }

        public PhysicalDeviceLicenseResult GetStorageDeviceLicense()
        {
            throw new NotImplementedException();
        }

        public List<StorageDeviceDto> GetAllStorageDeviceByIsOldRecord(int isOldRecord, bool needToUpdateData = false)
        {
            throw new NotImplementedException();
        }

        public bool IsDisableRetentionPeriodLimitation()
        {
            if (bool.TryParse(KeyValueDao.GetValueByKey("DisableRetentionPeriodLimitation")?.Value, out var disableRetentionLimitation))
            {
                return disableRetentionLimitation;
            }
            return false;
        }

        public string GetStorageDeviceNameById(string id)
        {
           return StorageDeviceDao.Find(s => s.Id.ToString().Equals(id, StringComparison.OrdinalIgnoreCase)).Name;
        }

        public bool ValidateExportStorageInfo(string id)
        {
            var globalStorage = StorageDeviceDao.GetStorageDevicesById(new Guid(id));
            return globalStorage is { Type: (int) StorageDeviceType.SFTP or (int) StorageDeviceType.CloudAzure };
        }
        public bool ValidateExportGoogleStorageInfo(string id)
        {
            var globalStorage = StorageDeviceDao.GetStorageDevicesById(new Guid(id));
            return globalStorage is { Type: (int)StorageDeviceType.SFTP or (int)StorageDeviceType.CloudAzure or (int)StorageDeviceType.Google };
        }

        public async Task<double> GetAllArchiverStorageGBSizeAsync(string storageId, IEnumerable<string> excludedJobPrefixes = null, CancellationToken cancellationToken = default)
        {
            double mailboxSize = await EXOArhciverSubInfo.GetArchiverStorageGBSizeAsync(storageId, cancellationToken);
            double spSize = await ArchiverIndexSubInfoDao.GetAllArchiverStorageGBSizeAsync(storageId, excludedJobPrefixes, cancellationToken);
            double totalSize = mailboxSize + spSize;
            this.logger.Info($"GetArchiverStorageGBSize mailboxSize:{mailboxSize},spSize:{spSize},totalSize:{totalSize}");
            return totalSize;
        }
    }
}
