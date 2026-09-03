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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.GraphApi.UsageReport;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Google;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Salesforce;
using AvePoint.RA.Contract.O365Tenant;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.ArchivedFullTextIndex;
using AvePoint.RA.DB.Dao.ArchivedFullTextIndex.Impl;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.FileSystem;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.FileSystem;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Impl.Salesforce;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Dao.Discovery.Salesforce;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.ArchivedFullTextIndex;
using AvePoint.RA.Service.Services.StorageDevice;
using Cloud.Sdk.AosModern;
using Cloud.Sdk.Data.Aos.License;
using Cloud.Sdk.Data.AosModern;
using RAGoogle.Services;
using RASalesforce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Dao.Discovery.FileSystem;
using AvePoint.RA.DB.Dao.Discovery.Impl.FileSystem;

namespace AvePoint.RA.Service.RMTasks
{
    public class UpdateAosStatisticsSizeExecutor : ITaskExecutor
    {
        private RALogger mLogger = RALogger.GetInstance(typeof(UpdateAosStatisticsSizeExecutor));
        public ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();

        private readonly IRMDiscoveryExecutionInfoDao _executionInfoDao = new RMDiscoveryExecutionInfoDao();
        
        private readonly IRMDiscoverySalesforceExecutionInfoDao _salesforceExecutionInfoDao = new RMDiscoverySalesforceExecutionInfoDao();
        private static readonly IRMDiscoveryGoogleExecutionInfoDao _googleROTExecutionInfoDao = new RMDiscoveryGoogleExecutionInfoDao();
        private static readonly IRMDiscoveryFSExecutionInfoDao _fsExecutionInfoDao = new RMDiscoveryFSExecutionInfoDao();

        private readonly IRMTenantDiscoveryDBInfoDao _tenantInfoDao = new RMTenantDiscoveryDBInfoDao();

        private readonly IRMDiscoveryOffice365TenantDao _discoveryO365TenantDao = new RMDiscoveryOffice365TenantDao();

        private readonly IRMDiscoveryOffice365NodeDao _discoveryNodeDao = new RMDiscoveryOffice365NodeDao();
        private readonly IRMDiscoveryGoogleOrganizationInfoDao _organizationInfoDao = new RMDiscoveryGoogleOrganizationInfoDao();

        private readonly IRMDiscoveryConfigurationDao _configurationDao = new RMDiscoveryConfigurationDao();

        private readonly IRMKeyValueDao _keyValueDao = new RMKeyValueDao();

        private RMArchivedFullTextIndexCategoryManagement _indexCategoryManagement;

        private readonly IRMArchivedFullTextIndexDao _archivedFullTextIndexDao = new RMArchivedFullTextIndexDao();

        private IRestoreSearchService _restoreSearchService => PlatformWindsorManager.GetService<IRestoreSearchService>();
        public Task ExecutorAsync(TaskBase task)
        {
            try
            {
                var tInfos = TenantService.GetAllAvailableTenantInfo();
                foreach (var tInfo in tInfos)
                {
                    TenantUtil.RunUnderTenant(tInfo.TenantId, tInfo.RegisterEmail, () =>
                    {
                        UpdateSizeToAOS();
                        UpdateAOSPSizeToAOS();
                    });
                }
            }
            catch(Exception e)
            {
                mLogger.Error($"something went wrong when update size to AOS ,error:{e.ToString()}");
            }
            return System.Threading.Tasks.Task.CompletedTask;
        }
        public void UpdateSizeToAOS()
        {
            try
            {
                IRMJobSizeAndCountStatisticsDao mRMJobSizeAndCountStatisticsDao = PlatformWindsorManager.GetService<IRMJobSizeAndCountStatisticsDao>();
                var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
                var info = client.LicenseService.GetLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME).GetAwaiter().GetResult();
                if (info.Extension is Cloud.Sdk.Data.AosModern.CloudRecordsExtension)
                {
                    var size = StorageDeviceService.GetArchiverStorageGBSize();
                    Cloud.Sdk.Data.AosModern.CloudRecordsExtension extension = info.Extension as Cloud.Sdk.Data.AosModern.CloudRecordsExtension;
                    if (extension.SaleType == Cloud.Sdk.Data.AosModern.SaleType.PrePaidConsumption)
                    {
                        DateTime currentDate = DateTime.UtcNow;
                        int resetDay = 1;
                        try
                        {
                            var keyValue = _keyValueDao.GetValueByKey("RestoreJobStatisticsResetDay");
                            if (keyValue != null && !string.IsNullOrEmpty(keyValue.Value) && int.TryParse(keyValue.Value, out int day))
                            {
                                int daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
                                resetDay = day > daysInMonth ? daysInMonth : day;
                            }
                        }
                        catch (Exception ex)
                        {
                            mLogger.Warn($"Failed to get RestoreJobStatisticsResetDay from RMKeyValues, fallback to 1st day. Error: {ex}");
                            resetDay = 1;
                        }
                        if (currentDate.Day == resetDay)
                        {
                            mRMJobSizeAndCountStatisticsDao.UpdateRestoreJobStatisticsStatusAsync().GetAwaiter().GetResult();
                        }
                        var deleteSizeList = mRMJobSizeAndCountStatisticsDao.GetAllDeleteJobStatisticsAsync().GetAwaiter().GetResult();
                        var restoreSizeList = mRMJobSizeAndCountStatisticsDao.GetAllRestoreJobStatisticsAsync().GetAwaiter().GetResult();
                        int deleteSize = CaculateGBSize(deleteSizeList);
                        int restoreSize = CaculateGBSize(restoreSizeList);
                        extension.DeletionUsageCapacity = deleteSize;
                        extension.ConsumedRestoreCapacity = restoreSize;
                        extension.ConsumedAvepointStorageCapacity = (int)size;
                        mLogger.Info($"update size to aos,size:DeletionUsageCapacity:{deleteSize}gb,ConsumedRestoreCapacity:{restoreSize}gb,ConsumedAvepointStorageCapacity:{size}gb");
                    }

                    var opusSOLicenseModule = info.Modules.FirstOrDefault(item => item.Name.Equals(RecordsConstants.OPUS_MODULE_DISCOVERY_NAME));

                    if(_tenantInfoDao.IsInitTenantDiscoveryDBInfoAsync().GetAwaiter().GetResult() && RMDiscoveryDBManager.CheckOffice365TablesExistsAsync().GetAwaiter().GetResult())
                    {
                        if (info.Type != LicenseType.Trial && opusSOLicenseModule?.ExpirationTime.Ticks > DateTime.UtcNow.Ticks)
                        {
                            var (_, executedCount, currentYearCount) = _executionInfoDao.CalculateAsync(info.Type).GetAwaiter().GetResult();
                            extension.ConsumedFrequency = currentYearCount;
                            var usageSize = CacalateO365TenantStorageUsageSize(client).GetAwaiter().GetResult();
                            client.LicenseService.UpdateModuleNumberAsync(new()
                            {
                                Product = RecordsConstants.RECORDS_APPLICATION_NAME,
                                ModuleName = RecordsConstants.OPUS_MODULE_DISCOVERY_NAME,
                                ObjectNumber = (int)usageSize
                            }).GetAwaiter().GetResult();
                        }
                    }

                    var opusSalesforceLicenseModule = info.Modules.FirstOrDefault(item => item.Name.Equals(RecordsConstants.OPUS_MODULE_Salesforce_Discovery));

                    if(_tenantInfoDao.IsInitTenantDiscoveryDBInfoAsync().GetAwaiter().GetResult() && RMDiscoveryDBManager.CheckSalesforceTablesExistsAsync().GetAwaiter().GetResult())
                    {
                        if (info.Type != LicenseType.Trial && opusSalesforceLicenseModule?.ExpirationTime.Ticks > DateTime.UtcNow.Ticks)
                        {
                            //mLogger.Info($"Current consumed frequency for salesforce: {extension.ConsumedFrequencyForSalesforce}");
                            var (_, _, currentYearCount) = _salesforceExecutionInfoDao.CalculateAsync(info.Type).GetAwaiter().GetResult();
                            //extension.ConsumedFrequencyForSalesforce = currentYearCount;
                            //mLogger.Info($"New consumed frequency for salesforce: {extension.ConsumedFrequencyForSalesforce}");
                            var usageSize = CaculateSalesforceOrganizationStorageUsageSize().GetAwaiter().GetResult();
                            client.LicenseService.UpdateModuleNumberAsync(new()
                            {
                                Product = RecordsConstants.RECORDS_APPLICATION_NAME,
                                ModuleName = RecordsConstants.OPUS_MODULE_Salesforce_Discovery,
                                ObjectNumber = (int)usageSize
                            }).GetAwaiter().GetResult();
                        }
                    }

                    var opusGoogleROTLicenseModule = info.Modules.FirstOrDefault(item => item.Name.Equals(RecordsConstants.OPUS_MODULE_Google_WorkSpace_Discovery));

                    if (_tenantInfoDao.IsInitTenantDiscoveryDBInfoAsync().GetAwaiter().GetResult() && RMDiscoveryDBManager.CheckGoogleTablesExistsAsync().GetAwaiter().GetResult())
                    {
                        if (info.Type != LicenseType.Trial && opusGoogleROTLicenseModule?.ExpirationTime.Ticks > DateTime.UtcNow.Ticks)
                        {
                            mLogger.Info($"Current consumed frequency for google: {extension.ConsumedFrequencyForGoogleWorkspace}");
                            var (_, _, currentYearCount) = _googleROTExecutionInfoDao.CalculateAsync(info.Type).GetAwaiter().GetResult();
                            extension.ConsumedFrequencyForGoogleWorkspace = currentYearCount;
                            mLogger.Info($"New consumed frequency for google: {extension.ConsumedFrequencyForGoogleWorkspace}");
                            var usageSize = CalculateGoogleOrganizationStorageUsageSize(client).GetAwaiter().GetResult();
                            client.LicenseService.UpdateModuleNumberAsync(new()
                            {
                                Product = RecordsConstants.RECORDS_APPLICATION_NAME,
                                ModuleName = RecordsConstants.OPUS_MODULE_Google_WorkSpace_Discovery,
                                ObjectNumber = (int)usageSize
                            }).GetAwaiter().GetResult();
                        }
                    }

                    var opusFSLicenseModule = info.Modules.FirstOrDefault(item => item.Name.Equals(RecordsConstants.OPUS_MODULE_FileSystem_Discovery));

                    if (_tenantInfoDao.IsInitTenantDiscoveryDBInfoAsync().GetAwaiter().GetResult() && RMDiscoveryDBManager.CheckFileSystemTablesExistsAsync().GetAwaiter().GetResult())
                    {
                        if (info.Type != LicenseType.Trial && opusFSLicenseModule?.ExpirationTime.Ticks > DateTime.UtcNow.Ticks)
                        {
                            mLogger.Info($"Current consumed frequency for file system: {extension.ConsumedFrequencyForFileSystem}");
                            var (_, _, currentYearCount) = _fsExecutionInfoDao.CalculateAsync(info.Type).GetAwaiter().GetResult();
                            extension.ConsumedFrequencyForFileSystem = currentYearCount;
                            mLogger.Info($"New consumed frequency for file system: {extension.ConsumedFrequencyForFileSystem}");
                            var usageSize = CalculateFileSystemStorageUsageSize();
                            client.LicenseService.UpdateModuleNumberAsync(new()
                            {
                                Product = RecordsConstants.RECORDS_APPLICATION_NAME,
                                ModuleName = RecordsConstants.OPUS_MODULE_FileSystem_Discovery,
                                ObjectNumber = (int)usageSize
                            }).GetAwaiter().GetResult();
                        }
                    }

                    if (extension.EnableContentSearch || _restoreSearchService.IsEnableFullTextIndexSearch())
                    {
                        _indexCategoryManagement = new RMArchivedFullTextIndexCategoryManagement();
                        var (minArchiverTime, maxArchiverTime) = (0L, 0L);
                        var isNewFullTextIndexKeyValue = _keyValueDao.GetValueByKey(KeyNameCollection.IsNewFullTextIndex);
                        if (isNewFullTextIndexKeyValue != null && bool.TryParse(isNewFullTextIndexKeyValue.Value, out var isNewFullTextIndex) && isNewFullTextIndex)
                        {
                            (minArchiverTime, maxArchiverTime) = _archivedFullTextIndexDao.GetMinMaxArchiverTimeBySiteUrlsV1Async([]).GetAwaiter().GetResult();
                        }
                        else
                        {
                            (minArchiverTime, maxArchiverTime) = _archivedFullTextIndexDao.GetMinMaxArchiverTimeBySiteUrlsAsync([]).GetAwaiter().GetResult();
                        }
                        var indexSize = _indexCategoryManagement.GetCategorySizeByArchiverTimeRangeAsync(minArchiverTime, maxArchiverTime).GetAwaiter().GetResult();
                        long oneGB = 1024L * 1024 * 1024;
                        int indexSizeInGB = indexSize == 0 ? 0 : (int)((indexSize + oneGB - 1) / oneGB);
                        extension.ConsumedIndexSize = indexSizeInGB;
                        mLogger.Info($"Willing to update the index size to AOS, consumed index size Bytes/GB: [{indexSize} ,{indexSizeInGB}] .");
                    }

                    if (client.LicenseService.UpdateLicenseExtensionAsync(new ()
                    {
                        LicenseId = info.Id,
                        Extension = extension
                    }).GetAwaiter().GetResult())
                    {
                        mLogger.Info("update size to AOS success!");
                    }
                    else
                    {
                        mLogger.Warn("update size to AOS failed!");
                    }
                    UpdateStorageSize(size).GetAwaiter().GetResult();
                }
            }
            catch (Exception e)
            {
                mLogger.Warn($"update size to AOS failed!,error :{e.ToString()}");
            }
        }

        public void UpdateAOSPSizeToAOS()
        {
            try
            {
                IRMJobSizeAndCountStatisticsDao mRMJobSizeAndCountStatisticsDao = PlatformWindsorManager.GetService<IRMJobSizeAndCountStatisticsDao>();
                var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
                var info = client.LicenseService.GetLicenseAsync(ProductInfo.PartnerStorageOptimization.Name).GetAwaiter().GetResult();
                if (info != null && info.Extension is Cloud.Sdk.Data.AosModern.PartnerStorageOptimizationExtension)
                {
                    var extension = info.Extension as Cloud.Sdk.Data.AosModern.PartnerStorageOptimizationExtension;
                    DateTime currentDate = DateTime.UtcNow;
                    if (currentDate.Day == 1)
                    {
                        mRMJobSizeAndCountStatisticsDao.UpdateAOSPRestoreJobStatisticsStatusAsync().GetAwaiter().GetResult();
                    }
                    var size = StorageDeviceService.GetAOSPArchiverStorageGBSize();
                    var deleteSizeList = mRMJobSizeAndCountStatisticsDao.GetAOSPDeleteJobStatisticsAsync().GetAwaiter().GetResult();
                    var restoreSizeList = mRMJobSizeAndCountStatisticsDao.GetAOSPRestoreJobStatisticsAsync().GetAwaiter().GetResult();
                    var deleteSize = CaculateGBSize(deleteSizeList);
                    var restoreSize = CaculateGBSize(restoreSizeList);
                    extension.DeletionUsageCapacity = deleteSize;
                    extension.ConsumedRestoreCapacity = restoreSize;
                    mLogger.Info($"update AOSP size to aos,size:DeletionUsageCapacity:{deleteSize}gb,ConsumedRestoreCapacity:{restoreSize}gb,ConsumedAvepointStorageCapacity:{size}gb");

                    if (client.LicenseService.UpdateLicenseExtensionAsync(new()
                    {
                        LicenseId = info.Id,
                        Extension = extension
                    }).GetAwaiter().GetResult())
                    {
                        mLogger.Info("update AOSP size to AOS success!");
                    }
                    else
                    {
                        mLogger.Warn("update AOSP size to AOS failed!");
                    }

                    UpdateAOSPStorageSize(size).GetAwaiter().GetResult();
                }
            }
            catch (Exception e)
            {
                mLogger.Warn($"update AOSP size to AOS failed!,error :{e}");
            }
        }

        private async Task<long> CacalateO365TenantStorageUsageSize(AosModernApiTenantClient client)
        {
            var totalSize = 0L;
            var discoveredO365Tenants = await _discoveryO365TenantDao.GetAllAsync();
            foreach(var discoveryO365Tenant in discoveredO365Tenants)
            {
                try
                {
                    mLogger.Info($"Start calculate tenant [{discoveryO365Tenant.UniqueId}] storage usage size.");
                    var usageReportManager = new RMGraphUsageReportManager(discoveryO365Tenant.UniqueId.ToString());

                    var containers = await _discoveryNodeDao.GetAllDiscoveryContainersAsync(discoveryO365Tenant.UniqueId);
                    var contentSources = containers.Select(item => item.ContentSource).ToHashSet();
                    foreach (var contentSource in contentSources)
                    {
                        var reportInfoes = await usageReportManager.GetUsageReportsAsync(contentSource, RMGraphUsageReportPeriod.Day7);
                        if(reportInfoes.Any())
                        {
                            totalSize += reportInfoes.First().Size;
                        }
                    }

                    mLogger.Info($"End calculate tenant [{discoveryO365Tenant.UniqueId}] storage usage size.");
                }
                catch(Exception e)
                {
                    mLogger.Info($"An error occurred while calculate tenant [{discoveryO365Tenant.UniqueId}] storage usage size. Error: {e}");
                }
            }

            return totalSize / 1024 / 1024 / 1024;
        }

        private async Task<long> CalculateGoogleOrganizationStorageUsageSize(AosModernApiTenantClient client)
        {
            var totalSize = 0L;
            var discoveryGoogleOrganizations = await _organizationInfoDao.GetAllAsync();

            foreach (var discoveryGoogleOrganization in discoveryGoogleOrganizations.ToHashSet())
            {
                try
                {
                    mLogger.Info($"Start calculate google organization [{discoveryGoogleOrganization.OrganizationId}] storage usage size.");

                    var  googleAppProfile = RMAosApiClient.GetGoogleAppProfile(TenantLocalValue.LogonGroupId, discoveryGoogleOrganization.OrganizationId, true);
                    
                    mLogger.Info($"Using Google app profile {googleAppProfile.ProfileName}");
                    
                    GoogleActivityService service = new(googleAppProfile);
                    DateTime startTime = DateTime.UtcNow.AddDays(-3);
                    var usageReport = await service.GetCustomerDriveReportUsageAsync(startTime) ?? 0;
                    totalSize += usageReport;
                    
                    mLogger.Info($"End calculate google organization [{discoveryGoogleOrganization.OrganizationId}] storage usage size.");

                }
                catch(Exception e)
                {
                    mLogger.Info($"An error occurred while calculate google organization [{discoveryGoogleOrganization.OrganizationId}] storage usage size. Error: {e}");
                }
            }

            return totalSize / 1024;
        }
        private async Task<long> CaculateSalesforceOrganizationStorageUsageSize()
        {
            var totalSize = 0L;
            var organizations = (await _configurationDao.GetAsync<RMDiscoverySalesforceScopeInfo>(RMDiscoveryConfigurationType.SalesforceNewlyScope)).Organizations;
            foreach(var organization in organizations)
            {
                try
                {
                    mLogger.Info($"Start calculate organization [{organization.Name}]-[{organization.Email}] storage usage size.");

                    
                   var _salesforceService = new SalesforceService(TenantLocalValue.LogonGroupId, organization.Id).Build();

                   var storageLimit = await _salesforceService.GetStorageLimitProxyAsync();
                   
                   var dataTotalSize = storageLimit.GetDataStorageTotal() * 1024 * 1024;
                   var fileTotalSize = storageLimit.GetFileStorageTotal() * 1024 * 1024;

                   totalSize += dataTotalSize + fileTotalSize;

                    mLogger.Info($"End calculate organization [{organization.Name}]-[{organization.Email}] storage usage size.");
                }
                catch(Exception e)
                {
                    mLogger.Info($"An error occurred while calculate organization [{organization.Name}]-[{organization.Email}] storage usage size. Error: {e}");
                }
            }

            return totalSize / 1024 / 1024 / 1024;
        }

        private async Task UpdateStorageSize(long size)
        {
            try
            {
                var licenseInfo = await RMAosApiClient.GetLicenseInfo(TenantLocalValue.LogonGroupId);

                if (licenseInfo.AdditionalProduct.HasFlag(PaidForProduct.OpusSO) && licenseInfo.StorageLicenseInfo != null)
                {
                    Cloud.Sdk.Data.AosModern.LicenseObjectsNumberInfo model = new Cloud.Sdk.Data.AosModern.LicenseObjectsNumberInfo();
                    model.Product = Cloud.Sdk.Data.AosModern.ProductInfo.AvePointRecords.Name;
                    model.ModuleName = Cloud.Sdk.Data.AosModern.LicenseModuleName.OpusStorageOptimization.Name;
                    //GB
                    model.ObjectNumber = (int)size;

                    var updated = await AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId).LicenseService.UpdateModuleNumberAsync(model);

                    Cloud.Sdk.Data.AosModern.CloudRecordsExtension recordsExtension = new Cloud.Sdk.Data.AosModern.CloudRecordsExtension();
                    //var updated = await AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId).LicenseService.UpdateLicenseExtensionAsync(model);
                    mLogger.Info($"update size to AOS is :{size} GB");
                }
            }
            catch (Exception e)
            {
                mLogger.Error("error occurred while updating actualUserSeat,ERROR:{0}", e.ToString());
            }
        }

        private async Task UpdateAOSPStorageSize(long size)
        {
            try
            {
                var model = new LicenseObjectsNumberInfo();
                model.Product = ProductInfo.PartnerStorageOptimization.Name;
                model.ModuleName = LicenseModuleName.PartnerStorageOptimization.Name;
                //GB
                model.ObjectNumber = (int)size;
                var updated = await AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId).LicenseService.UpdateModuleNumberAsync(model);
                mLogger.Info($"update size to AOS is :{size} GB");
                
            }
            catch (Exception e)
            {
                mLogger.Error("error occurred while updating AOSP actualUserSeat,ERROR:{0}", e.ToString());
            }
        }

        private int CaculateGBSize(List<RMJobSizeAndCountStatistics> sizeList)
        {
            long realSize=0;
            int sizeOfGB = 0;
            foreach (var temp in sizeList)
            {
                realSize += temp.Size;
            }
            sizeOfGB = (int)(realSize / (1024 * 1024 * 1024));
            return sizeOfGB;
        }

        private long CalculateFileSystemStorageUsageSize()
        {
            try
            {
                mLogger.Info("Start calculate file system storage usage size.");

                if (!RMDiscoveryDBManager.CheckFileSystemTablesExistsAsync().GetAwaiter().GetResult())
                {
                    mLogger.Info("File system tables not exists, skip calculate.");
                    return 0;
                }

                using var efContext = RMDiscoveryDBManager.GetEFContextAsync().GetAwaiter().GetResult();
                var totalSize = efContext.FSExecutionInfoList.Any()
                    ? efContext.FSExecutionInfoList.Sum(item => item.FileTotalSize)
                    : 0L;

                mLogger.Info($"End calculate file system storage usage size: [{totalSize}] bytes.");
                return totalSize / 1024 / 1024 / 1024;
            }
            catch (Exception e)
            {
                mLogger.Warn($"Failed to calculate file system storage usage size. Error: {e}");
                return 0;
            }
        }
    }
}
