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
using Aspose.Slides.Effects;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.DB.SecurityTrimming.Model;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Service.Services.StorageDevice;
using AvePoint.RA.Service.Services.Tenant;
using Castle.Core.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ControlPanel
{
    public class LicenseHelperService : RMServiceBase, ILicenseHelperService
    {
        private static IRALogger logger = RALogger.GetInstance(typeof(LicenseHelperService));
        private ITenantInfoDao TenantDao => PlatformWindsorManager.GetService<ITenantInfoDao>();
        private IRMKeyValueDao _keyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        public string CustomerId => TenantLocalValue.LogonGroupId;
        public async Task<RMAosLicenseInfo> GetLicenseInfoFromAOS()
        {
            return await RMCacheManager.Cache.TryGetAsync<RMAosLicenseInfo>(IRMCache.Keys.License_Tenant_Info, async () =>
            {
                return await RMAosApiClient.GetLicenseInfo(CustomerId);

            }, TimeSpan.FromMinutes(3));
        }
        public bool HasOpusSOLicense
        {
            get 
            {
                return RMCacheManager.Cache.TryGetAsync<bool>(IRMCache.Keys.License_Tenant_Info + $"DB_{PaidForProduct.OpusSO}", () =>
                {
                    var hasLicense = false;
                    hasLicense =  TenantDao.CheckAdditionalProduct(CustomerId, (long)PaidForProduct.OpusSO);
                    return System.Threading.Tasks.Task.FromResult(hasLicense);
                }, TimeSpan.FromMinutes(3)).Result;
            }
        }

        public bool HasOpusILLicense
        {
            get
            {
                return RMCacheManager.Cache.TryGetAsync<bool>(IRMCache.Keys.License_Tenant_Info + $"DB_{PaidForProduct.OpusIL}", () =>
                {
                    var hasLicense = false;
                    hasLicense = TenantDao.CheckAdditionalProduct(CustomerId, (long)PaidForProduct.OpusIL);
                    return System.Threading.Tasks.Task.FromResult(hasLicense);
                }, TimeSpan.FromMinutes(3)).Result;
            }
        }

        public bool HasOpusDiscoveryLicense
        {
            get
            {
                return RMCacheManager.Cache.TryGetAsync<bool>(IRMCache.Keys.License_Tenant_Info + $"DB_{PaidForProduct.OpusDiscovery}", () =>
                {
                    var hasLicense = false;
                    hasLicense = TenantDao.CheckAdditionalProduct(CustomerId, (long)PaidForProduct.OpusDiscovery);
                    return System.Threading.Tasks.Task.FromResult(hasLicense);
                }, TimeSpan.FromMinutes(3)).Result;
            }
        }

        public bool HasOpusGoogleLicense
        {
            get
            {
                return RMCacheManager.Cache.TryGetAsync<bool>(IRMCache.Keys.License_Tenant_Info + $"DB_{PaidForProduct.OpusGoogle}", () =>
                {
                    var hasLicense = false;
                    hasLicense = TenantDao.CheckAdditionalProduct(CustomerId, (long)PaidForProduct.OpusGoogle);
                    return System.Threading.Tasks.Task.FromResult(hasLicense);
                }, TimeSpan.FromMinutes(3)).Result;
            }
        }

        public bool HasOpusSalesforceDiscoveryLicense
        {
            get
            {
                return RMCacheManager.Cache.TryGetAsync<bool>(IRMCache.Keys.License_Tenant_Info + $"DB_{PaidForProduct.OpusSalesforceDiscovery}", () =>
                {
                    var hasLicense = false;
                    hasLicense = TenantDao.CheckAdditionalProduct(CustomerId, (long)PaidForProduct.OpusSalesforceDiscovery);
                    return System.Threading.Tasks.Task.FromResult(hasLicense);
                }, TimeSpan.FromMinutes(3)).Result;
            }
        }

        public bool HasOpusSPILOrSOLicense
        {
            get
            {
                return HasOpusILLicense || HasOpusSOLicense;
            }
        }

        public bool HasGoogleControlLicense
        {
            get
            {
                return RMCacheManager.Cache.TryGetAsync<bool>(IRMCache.Keys.License_Tenant_Info + $"DB_{PaidForProduct.GoogleControl}", async () =>
                {
                    var hasLicense = await RMAosApiClient.CheckControlPlusLicense(CustomerId);
                    return hasLicense;
                }, TimeSpan.FromMinutes(3)).Result;
            }
        }

        public bool HasOpusGoogleROTDiscoveryLicense
        {
            get
            {
                return RMCacheManager.Cache.TryGetAsync<bool>(IRMCache.Keys.License_Tenant_Info + $"DB_{PaidForProduct.OpusGoogleWorkspaceDiscovery}", () =>
                {
                    var hasLicense = false;
                    hasLicense = TenantDao.CheckAdditionalProduct(CustomerId, (long)PaidForProduct.OpusGoogleWorkspaceDiscovery);
                    return System.Threading.Tasks.Task.FromResult(hasLicense);
                }, TimeSpan.FromMinutes(3)).Result;
            }
        }

        public bool HasOpusFileSystemDiscoveryLicense
        {
            get
            {
                return RMCacheManager.Cache.TryGetAsync<bool>(
                    IRMCache.Keys.License_Tenant_Info + $"DB_{PaidForProduct.OpusFileSystemDiscovery}",
                    () =>
                    {
                        var hasLicense = TenantDao.CheckAdditionalProduct(CustomerId, (long)PaidForProduct.OpusFileSystemDiscovery);
                        if (!hasLicense)
                        {
                            // Check legacy PreviewFeature
                            hasLicense = TenantService.CheckLicenseWithAdditionalDataSource(CustomerId, PreviewFeature.FileSystemDiscovery);
                        }
                        return System.Threading.Tasks.Task.FromResult(hasLicense);
                    },
                    TimeSpan.FromMinutes(3)).Result;
            }
        }

        public async Task<bool> IsNewOpus(bool checkTenantExist = false, bool useCache = false)
        {
            if (!useCache)
            {
                return await RealCheckIsNewOpusAsync(checkTenantExist);
            }

            return await RMCacheManager.Cache.TryGetAsync<bool>(
                IRMCache.Keys.Tenant_IsNewOpus, 
                async () =>
                {
                    return await RealCheckIsNewOpusAsync(checkTenantExist);
                }, 
                TimeSpan.FromMinutes(3));
        }
        private async Task<bool> RealCheckIsNewOpusAsync(bool checkTenantExist)
        {
            if (checkTenantExist && !(await TenantDao.CheckIfExistTenantInfoAsync(CustomerId)))
            {
                logger.Info("start get lincense from aos");
                var licenseInfo = await GetLicenseInfoFromAOS();
                logger.Info("finish get lincense from aos");
                return licenseInfo.AdditionalProduct.HasFlag(PaidForProduct.OpusSO) && !await ExistArchiverOldData(licenseInfo);
            }

            bool.TryParse(await _keyValueDao.GetValueByKeyAsync("RunDisposalInRecords"), out bool result);

            logger.Info($"RunDisposalInRecords : {result}");
            return result;
        }

        public async Task<bool> UpdateLicense(bool isInit, bool disableSO = false, bool isMigrationJob = false) 
        {
            var licenseInfo = await GetLicenseInfoFromAOS();
            if (!await UpdateSOLicense(licenseInfo, isInit, disableSO) || isMigrationJob) 
            {
                TenantDao.AddOrUpdateTenantLinkedModules(CustomerId, licenseInfo);
            }
            return true;
        }
        private async Task<bool> UpdateSOLicense(RMAosLicenseInfo licenseInfo, bool isInit, bool disableSO) 
        {
            var licenseUpdated = false;
            if (licenseInfo.AdditionalProduct.HasFlag(PaidForProduct.OpusSO))
            {
                var runDisposalInRecords = _keyValueDao.TryGetBoolValue("RunDisposalInRecords", out var tempVal) && tempVal;
                if (!runDisposalInRecords && isInit && !await ExistArchiverOldData(licenseInfo))
                {
                    runDisposalInRecords = !disableSO;
                    _keyValueDao.Save(new RMKeyValue() { Key = "RunDisposalInRecords", Value = runDisposalInRecords.ToString() });
                    _keyValueDao.Save(new RMKeyValue() { Key = "UpgradeOpusUtcTimeTicks", Value = DateTime.UtcNow.Ticks.ToString() });
                    logger.Info($"init RunDisposalInRecords.");
                }
                if (isInit && runDisposalInRecords)
                {
                    logger.Info($"init tenant use opus so.");
                    await InitAveStorage(licenseInfo);
                }
                if (!await IsNewOpus())
                {
                    RMAosLicenseInfo newLicenseInfo = new RMAosLicenseInfo()
                    {
                        AdditionalDataSource = licenseInfo.AdditionalDataSource,
                        AdditionalProduct = licenseInfo.AdditionalProduct.Remove(PaidForProduct.OpusSO),
                        EnableAutoClassification = licenseInfo.EnableAutoClassification
                    };
                    TenantDao.AddOrUpdateTenantLinkedModules(CustomerId, newLicenseInfo);
                    licenseUpdated = true;
                    logger.Info($"init tenant remove opus so.");
                }
                
            }
            if (isInit && !licenseInfo.AdditionalProduct.HasFlag(PaidForProduct.OpusIL) && 
                !licenseInfo.AdditionalProduct.HasFlag(PaidForProduct.OpusSO) &&
                licenseInfo.AdditionalProduct.HasFlag(PaidForProduct.OpusGoogle))
            {
                _keyValueDao.Save(new RMKeyValue() { Key = "RunDisposalInRecords", Value = "True" });
                
            }
            if (isInit && (licenseInfo.AdditionalProduct.HasFlag(PaidForProduct.OpusGoogle) || HasGoogleControlLicense))
            {
                logger.Info($"init default storage.");
                await InitAveStorage(licenseInfo);
            }
            return licenseUpdated;
        }
        private async Task InitAveStorage(RMAosLicenseInfo licenseInfo) 
        {
            try
            {
                if (licenseInfo.StorageLicenseInfo != null && !licenseInfo.StorageLicenseInfo.Byos)
                {
                    //Check and create AvePoint Storage
                    logger.Info("AvePoint Storage license.");
                    await StorageDeviceService.CreateDefaultStorageDeviceAsync();
                }
            }
            catch (Exception e)
            {
                logger.Error($"CreateDefaultStorageDeviceAsync in timer error : {e}");
            }
        }
        public async Task<bool> IsAvePointStorage() 
        {
            var licenseInfo = await GetLicenseInfoFromAOS();
            if ((licenseInfo.AdditionalProduct.HasFlag(PaidForProduct.OpusSO) 
                || licenseInfo.AdditionalProduct.HasFlag(PaidForProduct.OpusGoogle)
                || HasGoogleControlLicense) && await IsNewOpus()) 
            {
                return licenseInfo.StorageLicenseInfo != null && !licenseInfo.StorageLicenseInfo.Byos;
            }
            return false;
        }

        public async Task<bool> IsCloudArchivingByos()
        {
            var licenseInfo = await GetLicenseInfoFromAOS();
            var arl = licenseInfo.RelatedProductLicenses.FirstOrDefault(r => r.ProductType == RelatedProductType.CloudArchiving);
            if (arl != null)
            {
                return arl.Byos;
            }
            return true;
        }
        public async Task<bool> ForceEnableSO() 
        {
            try
            {
                logger.Info("start check ForceEnableSO");
                return await RMCacheManager.Cache.TryGetAsync(IRMCache.Keys.ForceEnableSO, () =>
                {
                    return System.Threading.Tasks.Task.FromResult(_keyValueDao.ForceEnableSO());
                }, TimeSpan.FromSeconds(5));

            }
            catch (Exception e)
            {
                logger.Error($"Checking tenant is 'EnableAutoClassfication' error:{e}");
            }
            return false;
        }
        public async Task<bool> IsEnableMaestroAI() 
        {
            try
            {
                // var envName = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME];
                // var isGCP = ContractConstants.ENVIRONMENT_NAME_GCP.EqualsIgnoreCase(envName);
                // if (isGCP) { return false; }

                return await RMCacheManager.Cache.TryGetAsync(IRMCache.Keys.EnableMaestroAI, async () =>
                {
                    if (_keyValueDao.IsEnableIntelligent())
                    {
                        logger.Info("use db controll maestroAI.");
                        return true;
                    }
                    return await TenantDao.IsEnableIntelligent(CustomerId);
                }, TimeSpan.FromMinutes(3));
                
            }
            catch (Exception e)
            {
                logger.Error($"Checking tenant is 'EnableAutoClassfication' error:{e}");
            }
            return false;
        }

        public bool IsEnableDeleteRestoreDataFeature()
        {
            var res = true;
            res &= (_keyValueDao.TryGetBoolValue(RMKeyValuesConstants.EnableDeleteRestoredDataFeature, out var enabled) && enabled);
            //var key = _keyValueDao.GetValueByKey(RMKeyValuesConstants.ArchiverBackupOutputStreamLevel);//0 filelevel,4096 datablock
            //res &= (int.TryParse(key?.Value, out int result) && (result == 0));
            //Remove BYOS limit
            //res &= (RMAosApiClient.GetLicenseInfo(TenantLocalValue.LogonGroupId).Result.StorageLicenseInfo?.Byos ?? false);
            return res;
        }

        public async Task<RAReturnMessage> ValidateLicense() 
        {
            RAReturnMessage message = new RAReturnMessage()
            {
                MessageType = RAMessageType.Successful,
            };
            logger.Info($"[SsoLogin] begin to validate license info.");
            var licenseInfo = await GetLicenseInfoFromAOS();
            if (licenseInfo.AdditionalProduct.Has(PaidForProduct.OpusDiscovery) 
                || licenseInfo.AdditionalProduct.Has(PaidForProduct.OpusSalesforceDiscovery) 
                || licenseInfo.AdditionalProduct.Has(PaidForProduct.OpusGoogleWorkspaceDiscovery)
                || licenseInfo.AdditionalProduct.Has(PaidForProduct.OpusFileSystemDiscovery))
            {
                return message;
            }
            var isNewOpus = await IsNewOpus();
            if (isNewOpus)
            {
                if (!licenseInfo.AdditionalProduct.HasFlag(PaidForProduct.OpusSO) && !licenseInfo.AdditionalProduct.Has(PaidForProduct.OpusGoogle))
                {
                    message.MessageType = RAMessageType.Failed;
                    message.FaildType = RAFailedType.LicenseDoesNotAllowLogin;
                    logger.Info($"opus so has no SO License.");
                    return message;
                }
            }
            else 
            {
                if (licenseInfo.IsOnlySOLicense())
                {
                    message.MessageType = RAMessageType.Failed;
                    message.FaildType = RAFailedType.UseCloudArchiving;
                    logger.Info($"old records only SO License.");
                    return message;
                }
                else if (licenseInfo.ArchiverLicenseExpired())
                {
                    message.MessageType = RAMessageType.Failed;
                    message.FaildType = RAFailedType.CloudArchiverLicenseExpired;
                    logger.Info($"old records archiver License expired.");
                    return message;
                }
            }
            return message;
        }
     
        private static async Task<bool> ExistArchiverOldData(RMAosLicenseInfo license)
        {
            var result = false;
            try
            {
                var client = new DAOAPIClientV1(true);
                //call dao api
                if (await client.CloudArchiverEnabled())
                {
                    result = true;
                    logger.Info("The tenant has cloud archiver old data.");
                }
            }
            catch (Exception ex)
            {
                logger.Info($"check tenant cloud archiver old data error:{ex.ToString()}.");
            }
            
            return result;
        }

        public async Task<long> GetUpgradeOpusTime()
        {
            return await RMCacheManager.Cache.TryGetAsync<long>(IRMCache.Keys.Tenant_UpgradeOpusTime, async () =>
            {
                var key = _keyValueDao.GetValueByKey("UpgradeOpusUtcTimeTicks");
                long.TryParse(key?.Value, out long res);
                return res;
            }, TimeSpan.FromMinutes(3));
        }

        public bool CheckAdditionalDataSource(PaidForModule module)
        {
            return TenantDao.CheckAdditionalDataSource(CustomerId, (long)module);
        }
    }
}
