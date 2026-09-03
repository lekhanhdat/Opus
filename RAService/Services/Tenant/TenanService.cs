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
using AvePoint.GCommon.Utility;
using AvePoint.Media.Service;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.AzureService;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Encryption;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.RMMachineLearning;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.CosmosDBControl;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Service.Services.StorageDevice;
using AvePoint.RA.VectorDataCenter.Storage;
using DocumentFormat.OpenXml.Drawing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Util;
using static AvePoint.RA.RACommonUtility.Common.CommonUtilityForSpecialTenant;
using RALogger = AvePoint.RA.CommonUtil.RALogger;
using Task = System.Threading.Tasks.Task;

namespace AvePoint.RA.Service.Services.Tenant
{
    public class TenantService : RMServiceBase, ITenantService
    {
        #region private member

        private RALogger logger = RALogger.GetInstance(typeof(TenantService));
        private static readonly int DefaultStorageQuota = 100; //GB
        private static readonly int DefaultGroupDBQuota = 1; //GB
        private readonly static object TenantInitLock = new object();

        private static readonly List<string> DeletedTables = new()
        {
            "ManualApproveHistories",
            "RECODataSyncFailure",
            "RECORecordsHistory",
            "SOFSArchiverDB",
            "SOOnPremiseSPArchiverDB",
            "SOStaticFSArchiverDB",
            "SOStaticOnPremiseSPArchiverDB",
            "NeedDeleteArchivedDataList",
            "RECOInheritTermFailure",
            "RECOPhysicalRecordsActionAudit",
            "RECORDReturnLoanDataHistory",
            "RECOConflictChannelSetting",
            "RMRunningJobRuleMapping",
            "RMStubFileRecords",
            "DataIngestionMessageList",
            "DataIngestionExecuteResultList",
            "RECOPhysicalRecordsMoveData"
        };

        #endregion private member

        #region public member
        private ITenantInfoDao TenantInfoDao => PlatformWindsorManager.GetService<ITenantInfoDao>();

        private IRMTenantUpgradeInfoDao TenantUpgradeInfoDao => PlatformWindsorManager.GetService<IRMTenantUpgradeInfoDao>();
        private ISecurityProfileDao SecurityProfileDao => PlatformWindsorManager.GetService<ISecurityProfileDao>();
        private IRMRemoteNodeService RemoteNodeService => PlatformWindsorManager.GetService<IRMRemoteNodeService>();
        private ITrainingScopeService TrainingScopeService => PlatformWindsorManager.GetService<ITrainingScopeService>();
        private IRMKeyValueDao _keyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IRMFunctionSettingDao RMFunctionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private IStorageDeviceManager StorageDeviceManager => new StorageDeviceManager();
        private IDBInfoDao DBInfoDao => PlatformWindsorManager.GetService<IDBInfoDao>();
        private IGeneralSettingDao GeneralSettingDao => PlatformWindsorManager.GetService<IGeneralSettingDao>();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();

        #endregion
        private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();
        private IRMScheduleDao RMScheduleDao => PlatformWindsorManager.GetService<IRMScheduleDao>();
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private IRMTenantDiscoveryDBInfoDao TenantDiscoveryDBDao = new RMTenantDiscoveryDBInfoDao();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private readonly RMTenantVectorPostgreMappingDao _mappingDao = new RMTenantVectorPostgreMappingDao();

        private readonly RMTenantVectorCosmosMappingDao _mappingDaoCosmosDb = new RMTenantVectorCosmosMappingDao();

        /// <summary>
        /// 临时字段用于记录是否升级了Cosmos数据
        /// </summary>
        public int IsExplorerDataMoved(string tenantId)
        {
            return TenantInfoDao.CheckIfExplorerDataMoved(tenantId);
        }

        public bool CheckTenantExist(string tenantId)
        {
            return TenantInfoDao.CheckIfExistTenantInfo(tenantId);
        }

        public bool CheckTenantIsAvailable(string tenantId)
        {
            return TenantInfoDao.CheckTenantIsAvailable(tenantId);
        }

        public void ChangeAccountStatus(string tenantId, TenantStatus status)
        {
            TenantInfoDao.ChangeAccountStatus(tenantId, status);
        }

        public async Task<bool> InitTenantAsync()
        {
            bool isNew = false;
            string tenantId = TenantLocalValue.LogonGroupId;
            string registerEmail = TenantLocalValue.LogonUserEmail;
            try
            {
                using (PerformanceScope scope = new PerformanceScope($"Init tenant"))
                {
                    if (!TenantInfoDao.CheckIfExistTenantInfo(tenantId))
                    {
                        isNew = true;
                        logger.Info("Init tenant info: Id {0}", tenantId);
                        await InitTenantInfoAsync(tenantId, registerEmail, false);
                        await UserService.SyncLogonUserGroupAsync(TenantLocalValue.LogonUserId);
                        await AddUpgradeTeamsFlagForNewTenant();
                    }
                    else
                    {
                        if (TenantInfoDao.CheckIfExistAOSPTenantInfo(tenantId))
                        {
                            RemoteNodeService.CreateSyncAllNodesJob();
                            await UserService.SyncLogonUserGroupAsync(TenantLocalValue.LogonUserId);
                            TenantInfoDao.UpdateAOSPToOpusTenantInfo(TenantLocalValue.LogonGroupId);
                        }
                        logger.Info("Tenant info already exists,Id:{0}", tenantId);
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error("error occurred while init tenant,ERROR:{0}", ex.ToString());
                throw ex;
            }
            return isNew;

        }

        public async Task<bool> InitAOSPTenantAsync(string logonUserName)
        {
            bool isNew = false;
            string tenantId = TenantLocalValue.LogonGroupId;
            string registerEmail = logonUserName;
            TenantLocalValue.LogonUserEmail = logonUserName;
            try
            {
                using (PerformanceScope scope = new PerformanceScope($"Init tenant"))
                {
                    if (!TenantInfoDao.CheckIfExistTenantInfo(tenantId))
                    {
                        isNew = true;
                        logger.Info("Init tenant info: Id {0}", tenantId);
                        await InitTenantInfoAsync(tenantId, registerEmail, true);
                        await AddUpgradeTeamsFlagForNewTenant();
                    }
                    else
                    {
                        logger.Info("Tenant info already exists,Id:{0}", tenantId);
                        await CheckAndUpdateAOSPTenantAsync();
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error("error occurred while init tenant,ERROR:{0}", ex.ToString());
                throw ex;
            }
            return isNew;

        }

        public async Task<bool> InitMultiGeoTenantAsync(MultiGeoStatus multiGeoStatus)
        {
            bool isNew = false;
            string tenantId = TenantLocalValue.LogonGroupId;
            string registerEmail = TenantLocalValue.LogonUserEmail;
            try
            {
                using (PerformanceScope scope = new PerformanceScope($"Init Multi Geo tenant"))
                {
                    if (!TenantInfoDao.CheckIfExistTenantInfo(tenantId))
                    {
                        isNew = true;
                        logger.Info("Init tenant info: Id {0}", tenantId);
                        await InitMultiGeoTenantInfoAsync(tenantId, registerEmail, multiGeoStatus);
                        await UserService.SyncLogonUserGroupAsync(TenantLocalValue.LogonUserId);
                        await AddUpgradeTeamsFlagForNewTenant();
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error("error occurred while init tenant,ERROR:{0}", ex.ToString());
                throw ex;
            }
            return isNew;

        }

        public async Task CheckAndUpdateAOSPTenantAsync()
        {
            try
            {
                var tenantId = TenantLocalValue.LogonGroupId;
                var tenantInfo = TenantInfoDao.GetTenantInfo(tenantId);
                if (tenantInfo != null && tenantInfo.Status != TenantStatus.Normal)
                {
                    var client = AosApiUtility.GetAospApiClient();
                    var aospLicense = await client.CustomerService.CheckIsJobLicenseAvailable(tenantId, 1);
                    if (aospLicense.IsLicenseAvailable)
                    {
                        TenantInfoDao.UpdateStatus(tenantId, TenantStatus.Normal);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while check and update aosp tenant,ERROR:{0}", ex);
            }
        }

        public bool NeedUpgradeRemoteNodeForAosId(string tenantId)
        {
            return TenantInfoDao.NeedUpgradeRemoteNodeForAosId(tenantId);
        }

        public async Task InitKeyForMultiGeoTenant(InitMultiGeoTenantInfo tenantInfo)
        {
            await _keyValueDao.SaveOrUpdateAsync(new RMKeyValue() { Key = "RunDisposalInRecords", Value = "True" });
            await _keyValueDao.SaveOrUpdateAsync(new RMKeyValue() { Key = KeyNameCollection.EnableJPMCFileSystemFeature, Value = "true" });
            await _keyValueDao.SaveOrUpdateAsync(new RMKeyValue() { Key = "JPMC_UPGRADE_STATUS", Value = "3" });
            await RMFunctionSettingDao.AddOrUpdateSettingInfoAsync(Contract.FunctionSetting.FunctionSettingType.EnableMultiGEOFeature, "True");
            if (!string.IsNullOrEmpty(tenantInfo.JPMCMultiGeoDC))
            {
                await _keyValueDao.SaveOrUpdateAsync(new RMKeyValue() { Key = KeyNameCollection.JPMCMultiGEODC, Value = JsonConvert.DeserializeObject<string>(tenantInfo.JPMCMultiGeoDC) });
            }
            if (!string.IsNullOrEmpty(tenantInfo.JPMCMultiGeoMainDC))
            {
                await _keyValueDao.SaveOrUpdateAsync(new RMKeyValue() { Key = KeyNameCollection.JPMCMultiGEOMainDC, Value = tenantInfo.JPMCMultiGeoMainDC });
            }
            if(!string.IsNullOrEmpty(tenantInfo.EnableTeamsFeature))
            {
                await _keyValueDao.SaveOrUpdateAsync(new RMKeyValue() { Key = KeyNameCollection.EnableTeamsFeature, Value = tenantInfo.EnableTeamsFeature });
            }
            if (!string.IsNullOrEmpty(tenantInfo.HasUpgradeTeams))
            { 
                await _keyValueDao.SaveOrUpdateAsync(new RMKeyValue() { Key = KeyNameCollection.HasUpgradeTeams, Value = tenantInfo.HasUpgradeTeams });
            }
            if(tenantInfo.AdminAccountInfo != null && tenantInfo.AdminAccountInfo.Count > 0)
            {
                await UserService.SyncAdminAccountForMultiGeoTenantOtherDCAsync(tenantInfo.AdminAccountInfo);
            }
            if(!string.IsNullOrEmpty(tenantInfo.EnableFolderPath))
            {
                await _keyValueDao.SaveOrUpdateAsync(new RMKeyValue() { Key = KeyNameCollection.EnableFolderPath, Value = tenantInfo.EnableFolderPath });
            }
        }

        public void UpdateContainersUpgradeStatusToSuccessful(string tenantId)
        {
            TenantInfoDao.UpdateContainersUpgradeStatusToSuccessful(tenantId);
        }

        public List<TenantInfoDto> GetAllAvailableTenantInfo()
        {
            return TenantInfoDao.GetAllAvailableTenantInfo();
        }
        public List<TenantInfoDto> GetAllTenantInfo()
        {
            return TenantInfoDao.GetAllTenantInfo();
        }

        public List<TenantInfoDto> GetTenantInfoByTenantStatusAndMultiGeoStatus(int tenantStatus, int MultiGeoStatus)
        {
            return TenantInfoDao.GetTenantInfoByTenantStatusAndMultiGeoStatus(tenantStatus, MultiGeoStatus);
        }

        public string GetRegisterEmailByTenantId(string tenantId)
        {
            return TenantInfoDao.GetRegisterEmailByTenantId(tenantId);
        }
        public List<TenantInfoDto> GetPenddingForSyncNodesTenants()
        {
            return TenantInfoDao.GetPenddingForSyncNodesTenants();
        }
        public List<TenantInfoDto> GetSyncingNodesTenants()
        {
            return TenantInfoDao.GetSyncingNodesTenants();
        }

        public RMInitNodeState GetTenantInitNodeState(string tenantId)
        {
            return GetTenantInitNodeState(TenantInfoDao.GetTenantInitNodeState(tenantId));
        }
        public RMInitNodeState GetTenantInitNodeState(int initState)
        {
            if (initState >= (int)RMInitNodeState.Synced)
            {
                return RMInitNodeState.Synced;
            }
            else if (initState >= (int)RMInitNodeState.Syncing)
            {
                return RMInitNodeState.Syncing;
            }
            else if (initState >= (int)RMInitNodeState.SyncFailed)
            {
                return RMInitNodeState.SyncFailed;
            }
            else
            {
                return RMInitNodeState.None;
            }
        }
        public RMInitNodeState GetTenantInitNodeState(string tenantId, out RMDependTypeForInitNode dependType)
        {
            var initState = TenantInfoDao.GetTenantInitNodeState(tenantId);
            dependType = (initState & (int)RMDependTypeForInitNode.DAO) == (int)RMDependTypeForInitNode.DAO
                ? RMDependTypeForInitNode.DAO : RMDependTypeForInitNode.AOS;
            return GetTenantInitNodeState(initState);
        }

        public string GetEncryptionKey(string tenantGroupId)
        {
            throw new NotImplementedException();
        }

        public TenantInfoDto GetTenantInfo(string tenantGroupId)
        {
            return TenantInfoDao.GetTenantInfo(tenantGroupId);
        }

        public Task<TenantStatus?> TryGetTenantStatusAsync(string tenantId)
        {
            return TenantInfoDao.TryGetTenantStatusAsync(tenantId);
        }

        public void UpdateEncryptionInfoByGroupId(string groupId, string key)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAllTenantLicenseInfoAsync()
        {
            var tenants = TenantInfoDao.GetAllTenantInfo();
            List<string> availableTenantIds = new List<string>();
            List<string> unavailableTenantIds = new List<string>();
            bool hasFaild = false;
            foreach (var tenant in tenants)
            {
                try
                {
                    await TenantUtil.RunUnderTenantAsync(tenant.TenantId, tenant.RegisterEmail,
                        async () =>
                        {
                            try
                            {
                                //no async
                                if (tenant.IsUsedForAOSP)
                                {
                                    var aospClient = AosApiUtility.GetAospApiClient();
                                    var isAvaliable = await aospClient.CustomerService.CheckIsJobLicenseAvailable(tenant.TenantId, 1);
                                    if (!isAvaliable.IsLicenseAvailable)
                                    {
                                        unavailableTenantIds.Add(tenant.TenantId);
                                    }
                                    else
                                    {
                                        await InitDefaultStorageDeviceForAOSPAsync();
                                        availableTenantIds.Add(tenant.TenantId);
                                    }
                                }
                                else
                                {
                                    var licenseInfo = RMAosApiClient.GetLicenseInfo(tenant.TenantId).Result;
                                    if (licenseInfo.Enable)
                                    {
                                        availableTenantIds.Add(tenant.TenantId);
                                    }
                                    else
                                    {
                                        var aospClient = AosApiUtility.GetAospApiClient();
                                        var isAvaliable = await aospClient.CustomerService.CheckIsJobLicenseAvailable(tenant.TenantId, 1);
                                        if (isAvaliable.IsLicenseAvailable)
                                        {
                                            availableTenantIds.Add(tenant.TenantId);
                                        }
                                        else
                                        {
                                            unavailableTenantIds.Add(tenant.TenantId);
                                        }
                                    }

                                    await LicenseHelperService.UpdateLicense(false);

                                    await InitDefaultStorageDeviceAsync(licenseInfo);
                                    await StorageDeviceService.UpgradeAvePointStorageToManagedIdentityAsync();
                                    await GenerateDisposalJobScheduleForProcessApprovalDataAsync();
                                }
                            }
                            catch(Exception e)
                            {

                            }
                        });
                }
                catch (Exception ex)
                {
                    hasFaild = true;
                    logger.Error($"check license error,{tenant.TenantId}: {ex}");
                }
            }

            logger.Info($"Available tenants {availableTenantIds.Count}. {string.Join(',', availableTenantIds)}");
            logger.Info($"Unavailable tenants {unavailableTenantIds.Count}. {string.Join(',', unavailableTenantIds)}");
            if (!hasFaild)
            {
                TenantInfoDao.UpdateStatus(unavailableTenantIds);
            }
        }

        private async Task InitDefaultStorageDeviceAsync(RMAosLicenseInfo licenseInfo)
        {
            try
            {
                if ((licenseInfo.AdditionalProduct.HasFlag(PaidForProduct.OpusSO) && IsNewOpusTenant())
                    || await HasInitGControlPlatForm())
                {
                    if (licenseInfo.StorageLicenseInfo != null && !licenseInfo.StorageLicenseInfo.Byos)
                    {
                        //Check and create AvePoint Storage
                        logger.Info("AvePoint Storage license.");
                        await StorageDeviceService.CreateDefaultStorageDeviceAsync();
                    }
                    var schedule = RMScheduleDao.GetScheduleByType(ScheduleType.AdjustSizeSchedule);
                    if (schedule == null || schedule.Count == 0)
                    {
                        logger.Info("start create AdjustSizeSchedule.");
                        var generalSetting = GeneralSettingService.GetGeneralSettingAsync();
                        await RMScheduleDao.CreateScheduleAsync(new RMSchedule()
                        {
                            Id = Guid.NewGuid().ToString(),
                            StartTime = DateTime.UtcNow.AddMinutes(10).Ticks,
                            NoSchedule = false,
                            TimeZoneId = (await generalSetting).TimeZoneId,
                            EndType = (int)EndType.NoEnd,
                            Interval = 52,//one year
                            IntervalType = (int)IntervalType.Weekly,
                            JobCategory = (int)ScheduleType.AdjustSizeSchedule,
                            NextTime = DateTime.UtcNow.AddMinutes(10).Ticks,
                            OccurrencesTotal = 1,
                            Occurrences = 0,
                            IsDaylightSaving = false,
                            IsRemoved = false,
                        });
                        logger.Info("finish create AdjustSizeSchedule.");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"CreateDefaultStorageDeviceAsync in timer error : {e}");
            }
        }

        private async Task InitDefaultStorageDeviceForAOSPAsync()
        {
            try
            {
                //Check and create AvePoint Storage
                logger.Info("Create default storage device for AOSP.");
                await StorageDeviceService.CreateDefaultStorageDeviceAsync();

                var schedule = RMScheduleDao.GetScheduleByType(ScheduleType.AdjustSizeSchedule);
                if (schedule == null || schedule.Count == 0)
                {
                    logger.Info("start create AdjustSizeSchedule.");
                    var generalSetting = GeneralSettingService.GetGeneralSettingAsync();
                    await RMScheduleDao.CreateScheduleAsync(new RMSchedule()
                    {
                        Id = Guid.NewGuid().ToString(),
                        StartTime = DateTime.UtcNow.AddMinutes(10).Ticks,
                        NoSchedule = false,
                        TimeZoneId = (await generalSetting).TimeZoneId,
                        EndType = (int)EndType.NoEnd,
                        Interval = 52,//one year
                        IntervalType = (int)IntervalType.Weekly,
                        JobCategory = (int)ScheduleType.AdjustSizeSchedule,
                        NextTime = DateTime.UtcNow.AddMinutes(10).Ticks,
                        OccurrencesTotal = 1,
                        Occurrences = 0,
                        IsDaylightSaving = false,
                        IsRemoved = false,
                    });
                    logger.Info("finish create AdjustSizeSchedule.");
                }
                logger.Info("Success to create default storage device for AOSP.");
            }
            catch (Exception e)
            {
                logger.Error($"CreateDefaultStorageDeviceAsync in timer error : {e}");
            }
        }

        private async Task GenerateDisposalJobScheduleForProcessApprovalDataAsync()
        {
            try
            {
                if (IsNewOpusTenant())
                {
                    var schedule = RMScheduleDao.GetScheduleByType(ScheduleType.ApprovalProcessJob);
                    if (schedule == null || schedule.Count == 0)
                    {
                        logger.Info("start create ApprovalProcessJob.");
                        var generalSetting = GeneralSettingService.GetGeneralSettingAsync();
                        var nightTime = TimeZoneInfo.ConvertTimeToUtc(GenerateMidnightTime((await generalSetting).TimeZoneId)).AddMinutes(10).Ticks;
                        logger.Info($"approve process next time is:{nightTime}");
                        await RMScheduleDao.CreateScheduleAsync(new RMSchedule()
                        {
                            Id = Guid.NewGuid().ToString(),
                            StartTime = nightTime,
                            NoSchedule = false,
                            TimeZoneId = (await generalSetting).TimeZoneId,
                            EndType = (int)EndType.NoEnd,
                            Interval = 1,
                            IntervalType = (int)IntervalType.Daily,
                            JobCategory = (int)ScheduleType.ApprovalProcessJob,
                            NextTime = nightTime,
                            OccurrencesTotal = 1,
                            Occurrences = 0,
                            IsDaylightSaving = false,
                            IsRemoved = false,
                        });
                        logger.Info("finish create ApprovalProcessJob.");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error($"GenerateDisposalJobScheduleForProcessApprovalDataAsync in timer error : {e}");
            }
        }
        private DateTime GenerateMidnightTime(string timeZoneId)
        {

            /* Fortify Issue Type: Insecure Randomness 
            * Sink Details:  this class UpdateDashboardNextRunTimeAsync
            * Ignore Reason: random用于生成下次执行时间
            */
            Random random = new Random((int)DateTime.Now.Ticks);
            var hour = random.Next(-2, 2);
            hour = hour < 0 ? hour + 24 : hour;
            var min = random.Next(0, 59);
            var second = random.Next(0, 59);

            var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId));

            var startTime = localNow;
            if (hour <= localNow.Hour)
            {
                startTime = localNow.AddDays(1);
            }

            return new DateTime(startTime.Year, startTime.Month, startTime.Day, hour, min, second);
        }
        public void AddOrUpdateTenantLinkedModules(string customerId, RMAosLicenseInfo licenseInfo)
        {
            try
            {
                TenantInfoDao.AddOrUpdateTenantLinkedModules(customerId, licenseInfo);
            }
            catch (Exception ex)
            {
                logger.Error($"update tenant linked module {customerId}, {licenseInfo.AdditionalDataSource}, ERROR:{ex.ToString()}.");
            }

        }

        public void UpdateSyncNodeState(string tenantId, RMInitNodeState state)
        {
            TenantInfoDao.UpdateSyncNodeState(tenantId, state);
        }
        public void UpdateSyncSAState(string tenantId, RMInitNodeState state)
        {
            TenantInfoDao.UpdateSyncSAState(tenantId, state);
        }

        public void UpdateMultiGeoStatus(string tenantId, MultiGeoStatus status)
        {
            TenantInfoDao.UpdateMultiGeoStatus(tenantId, status);
        }

        public bool CheckLicenseWithAdditionalDataSource(string customerId, PaidForModule module)
        {
            return TenantInfoDao.CheckTenantIsAvailable(customerId) && TenantInfoDao.CheckAdditionalDataSource(customerId, (long)module);
        }

        public bool CheckLicenseWithAdditionalDataSource(string customerId, PreviewFeature previewFeature)
        {
            if (long.TryParse(RMKeyValueDao.GetValueByKey(RMKeyValuesConstants.PreviewFeature)?.Value, out var module))
            {
                if (((PreviewFeature)module & previewFeature) == previewFeature)
                {
                    return TenantInfoDao.CheckTenantIsAvailable(customerId);
                }
            }
            return false;
        }

        public bool CheckLicenseWithAdditionalProduct(string customerId, PaidForProduct product)
        {
            return TenantInfoDao.CheckTenantIsAvailable(customerId) && TenantInfoDao.CheckAdditionalProduct(customerId, (long)product);
        }

        public bool IsOldEncryption(string tenantId)
        {
            var key = GetEncryptionKey(tenantId);
            return key == EncodeUtil.EntropyKey;
        }

        public bool IsNewOpusTenant()
        {
            var key = _keyValueDao.GetValueByKey("RunDisposalInRecords");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }

        public long GetUpgradeOpusTimeTicks()
        {
            var key = _keyValueDao.GetValueByKey("UpgradeOpusUtcTimeTicks");
            long.TryParse(key?.Value, out long result);
            return result;
        }

        public bool IsCustomizationAppTenant()
        {
            var value = _keyValueDao.GetValueByKey("JPMC_Customization");
            var result = !string.IsNullOrEmpty(value?.Value);
            return result;
        }

        public FileExtentionsConfig GetFileExtentionsConfig()
        {
            try
            {
                var key = _keyValueDao.GetValueByKey("FileExtentionsConfig");
                if (key != null && !string.IsNullOrEmpty(key.Value))
                {
                    return JsonConvert.DeserializeObject<FileExtentionsConfig>(key.Value);
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"An error while GetFileExtentionsConfig, {ex}");
            }
            return null;
        }

        public int GetExportResultLimit()
        {
            var key = _keyValueDao.GetValueByKey("ExportResultLimit");
            _ = int.TryParse(key?.Value, out int result);
            return result;
        }

        public int GetTimeoutPeriodForWaitingJob()
        {
            var key = _keyValueDao.GetValueByKey("TimeoutPeriodForWaitingJob");
            _ = int.TryParse(key?.Value, out int result);
            return result;
        }

        public async Task AddUpgradeTeamsFlagForNewTenant()
        {
            var keyValue = _keyValueDao.GetValueByKey("HasUpgradeTeams");
            if(keyValue != null && bool.TryParse(keyValue.Value, out var hasUpgradeTeams) && hasUpgradeTeams)
            {
                return;
            }

            await _keyValueDao.SaveOrUpdateAsync(new RMKeyValue()
            {
                Key = "HasUpgradeTeams",
                Value = "True"
            });
        }

        public Task<bool> UpdateInitGControlPlatformStatus()
        {
            return TenantInfoDao.UpdateInitStatusForGControlPlatform(TenantLocalValue.LogonGroupId);
        }

        public async Task<bool> HasInitGControlPlatForm()
        {
#if DEBUG
            return true;
#endif
            if (!LicenseHelperService.HasGoogleControlLicense)
                return false;

            var existingTenant = TenantInfoDao.GetTenantInfo(TenantLocalValue.LogonGroupId);

            if (existingTenant == null) return false;

            //Fallback to check tenant already initialized for GControlPlatform before
            if (!existingTenant.IsInitForGControlPlatform) 
            {
                await TenantInfoDao.UpdateInitStatusForGControlPlatform(TenantLocalValue.LogonGroupId);
            }
            return true;
        }

        public MultiGeoStatus IsMultiGeoTenantInitialized()
        {
            var tenantInfo = GetTenantInfo(TenantLocalValue.LogonGroupId);
            return tenantInfo == null ? MultiGeoStatus.NotInit : (MultiGeoStatus)tenantInfo.MultiGeoStatus;
        }

        public async Task<bool> UpdateMultiGeoTenantInitStatus(MultiGeoStatus multiGeoStatus)
        {
            return await TenantInfoDao.UpdateMultiGeoTenantInitStatus(TenantLocalValue.LogonGroupId, multiGeoStatus);
        }

        private async System.Threading.Tasks.Task InitMultiGeoTenantInfoAsync(string tenantId, string registerEmail, MultiGeoStatus multiGeoStatus)
        {
#if DEBUG
            while (File.Exists("C:\\InitTenant.sleep"))
            {
                await System.Threading.Tasks.Task.Delay(2000);
            }
#endif

            var tenantInfo = new TenantInfoDto
            {
                TenantId = tenantId,
                RegisterEmail = registerEmail,
                StorageQuota = DefaultStorageQuota,
                DBQuota = DefaultGroupDBQuota,
                EncryptionKey = "NewKey",
                Status = TenantStatus.Provisioning,
                StorageSetting = string.Empty,
                SyncNodeState = (int)RMDependTypeForInitNode.AOS,
                IsUsedForAOSP = false,
                //SyncSAState = (int)RMDependTypeForInitNode.AOS,
                MultiGeoStatus = (int)multiGeoStatus
            };
            TenantInfoDao.CreateTenantInfo(tenantInfo);
            try
            {
                InitSecurityProfile(tenantId);
                await InitTenantDBAsync(tenantId, registerEmail);
                TenantInfoDao.UpdateStatus(tenantId, TenantStatus.Normal);

                #region Need read upgrade option from the configuration file
                //TenantUpgradeInfoDao.Create(tenantId, TenantUpgradeConfig.UpgradeOptions);
                #endregion
                //RemoteO365AccountService.SyncAllServiceAccountsFromAOS();
            }
            catch (Exception ex)
            {
                RMDBContextManager.DisposeCurrentTenantMapping(tenantId);
                TenantInfoDao.DeleteTenantInfo(tenantId);
                logger.Error("error occurred while init tenant,ERROR:{0}", ex.ToString());
                throw;
            }
        }

        private async System.Threading.Tasks.Task InitTenantInfoAsync(string tenantId, string registerEmail, bool isUsedForAOSP)
        {
#if DEBUG
            while (File.Exists("C:\\InitTenant.sleep"))
            {
                await System.Threading.Tasks.Task.Delay(2000);
            }
#endif

            var tenantInfo = new TenantInfoDto
            {
                TenantId = tenantId,
                RegisterEmail = registerEmail,
                StorageQuota = DefaultStorageQuota,
                DBQuota = DefaultGroupDBQuota,
                EncryptionKey = "NewKey",
                Status = TenantStatus.Provisioning,
                StorageSetting = string.Empty,
                SyncNodeState = (int)RMDependTypeForInitNode.AOS,
                IsUsedForAOSP = isUsedForAOSP,
                //SyncSAState = (int)RMDependTypeForInitNode.AOS,
            };
            TenantInfoDao.CreateTenantInfo(tenantInfo);
            try
            {
                InitSecurityProfile(tenantId);
                await InitTenantDBAsync(tenantId, registerEmail);
                TenantInfoDao.UpdateStatus(tenantId, TenantStatus.Normal);

                #region Need read upgrade option from the configuration file
                //TenantUpgradeInfoDao.Create(tenantId, TenantUpgradeConfig.UpgradeOptions);
                #endregion

                if (!isUsedForAOSP)
                {
                    await UserService.SyncTenantOnwerAsync();
                    RemoteNodeService.CreateSyncAllNodesJob();
                }
                //RemoteO365AccountService.SyncAllServiceAccountsFromAOS();
            }
            catch (Exception ex)
            {
                RMDBContextManager.DisposeCurrentTenantMapping(tenantId);
                TenantInfoDao.DeleteTenantInfo(tenantId);
                logger.Error("error occurred while init tenant,ERROR:{0}", ex.ToString());
                throw;
            }
        }

        private System.Threading.Tasks.Task InitTenantDBAsync(string tenantGroupId, string registerEmail)
        {

            var tenantDB = CreateTenantDBIfNotExist();

            var sqlObjName = EscapeSqlObjectName(registerEmail);
            var userName = string.Format("u#{0}", sqlObjName);
            var schemaName = string.Format("s#{0}", sqlObjName);
            var suffix = GetUniqieSuffix(tenantGroupId, schemaName);
            userName = userName + suffix;
            schemaName = schemaName + suffix;
            TenantInfoDao.UpdateTenantDBInfo(tenantGroupId, tenantDB, userName, schemaName);
            RMDBInitializer.UpgradTenantDBModel();
            return RMDBInitializer.InitDBAsync();
        }



        private string CreateTenantDBIfNotExist()
        {
            var tenantDB = TenantInfoDao.GetAvailableTenantDB(DefaultGroupDBQuota);
            if (null == tenantDB)
            {
                tenantDB = CreateNewDB();
                DatabaseUtility.LastTimeCreated = DateTime.UtcNow;
                logger.Info("create db success,dbname:{0}.", tenantDB);
            }
            return tenantDB;
        }

        private string CreateNewDB()
        {
            string dbName = string.Empty;
            try
            {
                lock (TenantInitLock)
                {
                    dbName = TenantInfoDao.GetAvailableTenantDB(DefaultGroupDBQuota);
                    if (string.IsNullOrEmpty(dbName))
                    {
                        dbName = GetTenantDBName();

                        TenantInfoDao.CreateTenantDB(dbName);
                        AddDatabaseToFailoverGroupWithRetry(dbName);
                    }
                }

            }
            catch (Exception ex)
            {
                dbName = string.Empty;
                logger.Error("error occurred while create tenant db,ERROR:{0}", ex.ToString());
                throw;
            }

            return dbName;
        }

        private void AddDatabaseToFailoverGroupWithRetry(string dbName)
        {
            if (string.IsNullOrWhiteSpace(dbName))
            {
                throw new ArgumentException("Database name is required.", nameof(dbName));
            }

            const int maxAttempts = 3;
            Exception lastException = null;
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    var added = FailoverGroupService.AddDatabasesToFailoverGroup(dbName);
                    if (added)
                    {
                        logger.Info("Added database to failover group successfully. DbName:{0}, Attempt:{1}", dbName, attempt);
                        return;
                    }

                    logger.Warn("Failed to add database to failover group (returned false). DbName:{0}, Attempt:{1}", dbName, attempt);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    logger.Warn("Failed to add database to failover group (exception). DbName:{0}, Attempt:{1}, Error:{2}", dbName, attempt, ex);
                }

                if (attempt < maxAttempts)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(2 * attempt));
                }
            }

            if (lastException != null)
            {
                throw lastException;
            }
        }

        private string GetUniqieSuffix(string tenantGroupId, string userName)
        {
            var finallyOwnerName = userName;
            var index = 0;
            while (TenantInfoDao.IsUserNameExist(tenantGroupId, finallyOwnerName))
            {
                index++;
                finallyOwnerName = userName + index;
            }
            return index == 0 ? "" : Convert.ToString(index);
        }

        private string EscapeSqlObjectName(string accountName)
        {
            if (accountName == null)
            {
                return null;
            }
            var schemaName = new StringBuilder();
            var accountNameChars = accountName.ToCharArray();
            foreach (var c in accountNameChars)
            {
                if (Char.IsLetter(c) || Char.IsNumber(c))
                {
                    schemaName.Append(c);
                }
                else
                {
                    schemaName.Append('#');
                }
            }
            return schemaName.ToString();
        }

        private string GetTenantDBName()
        {
            var dbNumber = TenantInfoDao.GetTenantDBCount() + 1;
            return string.Format("{0}_{1}_{2}", "recol_tenant", dbNumber, DateTime.UtcNow.ToString("yyMMddHHmmss"));
        }

        private void InitSecurityProfile(string tenantId)
        {
            logger.Info("begin to init security Profile,tenantId:{0}.", tenantId);
            var currentProfile = RMAosApiClient.GetCurrentAppliedSecurityProfile(tenantId);

            logger.Info("init security Profile,currentProfileName: {0}.", currentProfile?.Name);
            RA.Common.Global.Utils.ArgumentCheck.NotNull(currentProfile, nameof(currentProfile));
            var currentEncryption = new RADataEncryptionProfile()
            {
                TenantId = tenantId,
                AosSecurityProfileId = currentProfile.Id,
                Name = currentProfile.Name
            };
            SecurityProfileDao.AddProfile(currentEncryption);

        }

        public void SyncTenantOwner(string groupId)
        {
            try
            {
                var owner = RMAosApiClient.GetTenantInfo(groupId);
                var tenantInfo = TenantInfoDao.GetTenantInfo(groupId);
                if (!owner.UserPrincipalName.Equals(tenantInfo.RegisterEmail))
                {
                    TenantInfoDao.UpdateTenantOwner(groupId, owner.UserPrincipalName);
                }
            }
            catch (Exception)
            {
                logger.Error("error occurred while sync owner");
            }

        }

        /// <summary>
        /// validate account for user login by API 
        /// </summary>
        /// <param name="email">login user email</param>
        /// <param name="tenantId"></param>
        /// <param name="ownerEmail"></param>
        /// <returns></returns>
        public Boolean ValidateAccountByEmail(string email, ref string tenantId, ref string ownerEmail)
        {
            try
            {
                var spTenantId = WebUtil.GetOffice365tenantIdByUserName(email);
                var tenantGroupIds = RMAosApiClient.GetTenantGroupId(spTenantId);
                if (tenantGroupIds != null && tenantGroupIds.Count > 0)
                {

                    tenantId = tenantGroupIds[0];
                    var tenantInfo = TenantInfoDao.GetTenantInfo(tenantId);
                    if (tenantInfo != null && tenantInfo.Status == TenantStatus.Normal)
                    {
                        ownerEmail = tenantInfo.RegisterEmail;
                        return RMAosApiClient.IsCustomerLicenseAvailable(tenantId);
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error("validate user {0},error: {1}", email, ex.ToString());
                return false;
            }
            return false;

        }

        /// <summary>
        /// validate account for user login by API 
        /// </summary>
        /// <param name="email">login user email</param>
        /// <param name="tenantId"></param>
        /// <param name="ownerEmail"></param>
        /// <returns></returns>
        public Boolean ValidateAccountByTenantId(string tenantId, ref string ownerEmail)
        {
            bool result = false;
            try
            {
                if (TenantInfoDao.CheckTenantIsAvailable(tenantId))
                {
                    var tenantInfo = TenantInfoDao.GetTenantInfo(tenantId);
                    if (tenantInfo != null)
                    {
                        ownerEmail = tenantInfo.RegisterEmail;
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("validate user {0},error: {1}", ownerEmail, ex.ToString());
            }
            return result;

        }

        public async Task<bool> DeleteExpiredTenantAsync(string tenantId)
        {
            bool result = false;
            try
            {
                if (!TenantInfoDao.CheckTenantIsAvailable(tenantId))
                {
                    result = DeleteCosmosDB(tenantId);
                    result &= await TrainingScopeService.DeleteAIRelatedResourcesAsync(tenantId);
                    result &= await DeleteStorage(tenantId);
                    result &= DeleteDBInfo(tenantId);
                    result &= await DeleteTableAsync(tenantId);
                    result &= await DeleteVectorDb(tenantId);
                }

            }
            catch (Exception ex)
            {
                result = false;
                logger.Error($"error occurred while delete tenant:{ex.ToString()}");

            }
            return result;
        }

        private bool DeleteDBInfo(string tenantId)
        {
            bool result = false;
            try
            {
                var tenantInfo = TenantInfoDao.GetTenantInfo(tenantId);

                TenantInfoDao.DeleteTenantDBSchema(tenantInfo.DBName, string.Empty, tenantInfo.SchemaName);

                DBInfoDao.RemoveExplorerDBMapping(tenantInfo.TenantId);
                GeneralSettingDao.DeleteGeneralSettingByUser(tenantId);
                SecurityProfileDao.DeleteProfile(tenantId);
                TenantInfoDao.DeleteTenantInfo(tenantId);
                TenantUpgradeInfoDao.Delete(tenantId);
                TenantDiscoveryDBDao.TryRemoveTenantDiscoveryDBInfoAsync(tenantId).GetAwaiter().GetResult();
                result = true;
                logger.Info("success to remove control db info:{0}", tenantInfo.TenantId);

            }
            catch (Exception ex)
            {
                logger.Info("error occurred while remove db info:{0}", ex.ToString());
            }
            return result;
        }

        private async Task<bool> DeleteTableAsync(string tenantId)
        {
            var result = false;
            try
            {
                var connectStr = RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
                if (string.IsNullOrEmpty(connectStr))
                {
                    logger.Error("args table connect is not valid.");
                    return result;
                }
                foreach (var table in DeletedTables)
                {
                    await AzureUtil.DeleteTableAsync(connectStr, table + $"{tenantId.Replace("-", "")}");
                    result = true;
                    logger.Info("success to remove table :{0}", table + $"{tenantId.Replace("-", "")}");
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while remove azure table:{0}", ex.ToString());
            }
            return result;
        }

        private async Task<bool> DeleteStorage(string tenantId)
        {
            bool result = false;
            try
            {
                var connectStr = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];

                var containerName = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.REPORT_CONTAINER_NAME];

                if (string.IsNullOrEmpty(connectStr) || string.IsNullOrEmpty(containerName))
                {
                    logger.Error("args storage account is not valid.");
                    return result;
                }

                AzureUtil.DeleteBlobs(connectStr, containerName, tenantId);
                logger.Info("success to remove report file:{0}", tenantId);

                try
                {
                    string defaultConnectionString = CommonUtilityForSpecialTenant.GetStorageConnectionStringFromConfigFile(StorageStringType.DefaultStorage);
                    if (RMGlobalConfiguration.EnvSetting.IsGCPEnvironment)
                    {

                        logger.Info($"remove GCP default storage {tenantId} ");
                        var storageAPI = StorageDeviceManager.Open(new List<string>() { defaultConnectionString });
                        if (storageAPI.DirectoryExists(new Storage.StorageInfo(tenantId, "")))
                        {
                            var deleteResult = storageAPI.DeleteDirectory(new Storage.StorageInfo(tenantId, ""));
                            logger.Info($"remove google api {tenantId} {deleteResult.IsDeleted} {deleteResult.Message}");
                        }

                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(defaultConnectionString))
                        {
                            await AzureUtil.DeleteContainer(defaultConnectionString, tenantId);
                            logger.Info($"Success to remove default storage container: [{tenantId}]");
                        }
                        else
                        {
                            logger.Error($"Failed to remove default storage container :  [{tenantId}]");
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"Failed to remove default storage container :  [{tenantId}], error : {e}");
                    throw;
                }
                result = true;
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while remove storage blob:{0}", ex.ToString());
            }
            return result;

        }

        private async Task<bool> DeleteVectorDb(string tenantId)
        {
            try
            {
                var envName = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME];
                var isGCP = ContractConstants.ENVIRONMENT_NAME_GCP.Contains(envName?.ToLower());
                if (isGCP) // delete PostgreSQL
                {
                    var dbName = _mappingDao.GetOrCreateDatabaseName(tenantId, false);
                    if (!string.IsNullOrEmpty(dbName))
                    {
                        IVectorStore _vectorStore = new PostgresVectorStore(false);
                        logger.Warn($"Current tenant: [{tenantId}] is hard deleted. Need to execute deletion schema");
                        await _vectorStore.DropVectorDbIfExist(dbName, $"s_{SanitizeIdentifier(tenantId)}");
                        _mappingDao.DeleteMapping(tenantId);
                    }
                }
                else // delete CosmosDb
                {
                    var (dbName, containerName) = _mappingDaoCosmosDb.GetOrCreateDatabaseAndContainerName(new Guid(tenantId), false);
                    if (!string.IsNullOrEmpty(dbName))
                    {
                        IVectorStore _vectorStore = new CosmosDbVectorStore(false);
                        logger.Warn($"Current tenant: [{tenantId}] is hard deleted. Need to execute deletion document");
                        await _vectorStore.DropVectorDbIfExist(dbName, containerName);
                        _mappingDaoCosmosDb.DeleteMapping(tenantId);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to delete vector db :  [{tenantId}], error : {ex.Message}");
                return false;
            }
        }

        private bool DeleteCosmosDB(string tenantId)
        {
            bool result = true;
            try
            {
                if (RMCosmosDBIndependentController.IsEnabledIndependent(tenantId))
                {
                    result &= DeleteExplorerCosmosDataIfExists(tenantId, false, "independent");
                    result &= DeleteExplorerCosmosDataIfExists(tenantId, true, "normal");
                }
                else
                {
                    result = DeleteExplorerCosmosDataIfExists(tenantId, true, "normal");
                }

                logger.Info($"success to remove cosmos data:{tenantId}");
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while remove cosmos db info:{0}", ex.ToString());
                result = false;
            }
            return result;
        }

        private bool DeleteExplorerCosmosDataIfExists(string tenantId, bool usingNormalDatabase, string dbType)
        {
            try
            {
                var dbName = usingNormalDatabase
                    ? DBInfoDao.GetDBNameByNormalTenantId(tenantId)
                    : DBInfoDao.GetIdependentDBNameByTenantId(tenantId);

                if (string.IsNullOrEmpty(dbName))
                {
                    logger.Info($"Skip deleting {dbType} cosmos data because mapping does not exist. Tenant:{tenantId}");
                    return true;
                }

                var explorerDao = usingNormalDatabase
                    ? new ExplorerDao(useJpmcNoramlDB: true)
                    : new ExplorerDao();

                var deleted = explorerDao.DeleteExplorerData(tenantId);
                if (deleted)
                {
                    logger.Info($"success to remove {dbType} cosmos data:{tenantId}");
                }
                else
                {
                    logger.Warn($"failed to remove {dbType} cosmos data:{tenantId}");
                }

                return deleted;
            }
            catch (Exception ex)
            {
                logger.Error($"error occurred while remove {dbType} cosmos db info:{tenantId}, error:{ex}");
                return false;
            }
        }

        /*private string GetRECOTableName(string tenantGroupId)
        {
            return string.Concat(_RECOTablePrefix, tenantGroupId.Replace("-", string.Empty));
        }*/

        public bool IsCSDTenant()
        {
            try
            {
                return TenantInfoDao.IsEnableCSD(TenantLocalValue.LogonGroupId);
            }
            catch (Exception e)
            {
                logger.Error($"Checking tenant is 'EnableCSD' error:{e.ToString()}");
                return false;
            }
        }
        private string SanitizeIdentifier(string identifier)
        {
            var sanitized = identifier.ToLower().Replace("-", "_");
            return Regex.Replace(sanitized, @"[^a-z0-9_]", "");
        }
    }
}
