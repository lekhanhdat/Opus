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
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.DB.SecurityTrimming;
using Cloud.Sdk.Data.AosModern;

namespace AvePoint.RA.Service.Services.Discovery.Office365.License
{
    public class RMDiscoveryOffice365LicenseHelper
    {
        private const string S_DISCOVERY_TRIAL_LICENSE_CONTROL = "DISCOVERY_TRIAL_LICENSE_CONTROL";

        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365LicenseHelper));

        private static readonly IRMKeyValueDao s_keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private static readonly IRMDiscoveryExecutionInfoDao s_executionInfoDao = new RMDiscoveryExecutionInfoDao();
        
        private static readonly IRMSecurityTrimmingHelper _securityTrimmingHelper = PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        
        private static ITenantService s_tenantService => PlatformWindsorManager.GetService<ITenantService>();

        public static async Task<LicenseType> GetLicenseTypeAsync()
        {
            var licenseInfo = await RMAosApiClient.GetLicenseInfo(TenantLocalValue.LogonGroupId);
            s_logger.Info($"Current tenant license is [{licenseInfo.Type}].");
            return licenseInfo.Type;
        }

        public static async Task<bool> IsMeetLimitAsync()
        {
            var licenseInfo = await RMAosApiClient.GetLicenseInfo(TenantLocalValue.LogonGroupId);
            if (licenseInfo.Type == LicenseType.Trial)
            {
                var controlInfo = new RMDiscoveryOffice365TrialLicenseControlInfo();

                var setting = s_keyValueDao.GetValueByKey(S_DISCOVERY_TRIAL_LICENSE_CONTROL);
                if (setting != null && !string.IsNullOrWhiteSpace(setting.Value))
                {
                    controlInfo = JsonConvert.DeserializeObject<RMDiscoveryOffice365TrialLicenseControlInfo>(setting.Value);
                }

                var (fileTotalSize, executedCount) = await s_executionInfoDao.CalculateAllAsync(licenseInfo.Type);

                s_logger.Info($"Current tenant collect file total size [{fileTotalSize}], execute job count [{executedCount}].");

                return executedCount < 3 && fileTotalSize < controlInfo.LimitSize;
            }
            else
            {
                var discoveryLicenseInfo = licenseInfo.DiscoveryLicenseInfo;
                var (_, _, currentYearCount) = await s_executionInfoDao.CalculateAsync(licenseInfo.Type);
                return discoveryLicenseInfo.FrequencyPerYear > currentYearCount;
            }
        }

        public static async Task IncreaseConsumedFrequencyPreMonthAsync()
        {
            var aosApiClient = AosApiUtility.GetAosModerClient();
            var info = await aosApiClient.LicenseService.GetLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME);
            if (info.Type == LicenseType.Trial)
            {
                return;
            }

            if (info.Extension is CloudRecordsExtension extension)
            {
                extension.ConsumedFrequency++;
                await aosApiClient.LicenseService.UpdateLicenseExtensionAsync(new()
                {
                    LicenseId = info.Id,
                    Extension = extension
                });
            }
        }

        public static async Task DecreaseConsumedFrequencyPreMonthAsync()
        {
            var aosApiClient = AosApiUtility.GetAosModerClient();
            var info = await aosApiClient.LicenseService.GetLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME);
            if (info.Type == LicenseType.Trial)
            {
                return;
            }

            if (info.Extension is CloudRecordsExtension extension && extension.ConsumedFrequency > 0)
            {
                extension.ConsumedFrequency--;
                await aosApiClient.LicenseService.UpdateLicenseExtensionAsync(new()
                {
                    LicenseId = info.Id,
                    Extension = extension
                });
            }
        }

        public static async Task RemoveAllExecutionAsync()
        {
            await s_executionInfoDao.DeleteAllRecordsAsync();
        }

        public static async Task<bool> ClearLicenseUsageAsync()
        {
            try
            {
                var aosApiClient = AosApiUtility.GetAosModerClient();
                var info = await aosApiClient.LicenseService.GetLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME);
                if (info.Type == LicenseType.Trial)
                {
                    return true;
                }
                if (info.Extension is CloudRecordsExtension extension && extension.ConsumedFrequency > 0)
                {
                    extension.ConsumedFrequency = 0;
                    await aosApiClient.LicenseService.UpdateLicenseExtensionAsync(new()
                    {
                        LicenseId = info.Id,
                        Extension = extension
                    });
                }
                await RemoveAllExecutionAsync();
                return true;
            }
            catch (Exception e)
            {
                s_logger.Error($"Clear discovery license usage failed, error: {e}");
                return false;
            }
        }
        
        public static async Task<bool> IsAllowedToExportRowDataAsync()
        {
            return s_tenantService.CheckLicenseWithAdditionalDataSource(TenantLocalValue.LogonGroupId, PreviewFeature.DiscoveryExportRowData)
                   && await _securityTrimmingHelper.DoesUserHasThisPermissionAsync(RMDiscoveryPermissionMasks.AccessAll);
        }

        #region Discovery Duplication Data
        public static async Task<bool> IsAllowedToCleanupDiscoveryDuplicationDataAsync()
        {
            return s_tenantService.IsNewOpusTenant()
                   && await _securityTrimmingHelper.DoesUserHasThisPermissionAsync(RMDiscoveryPermissionMasks.AccessAll)
                   && s_tenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, PaidForProduct.OpusSO);
        }

        public static async Task<bool> IsAllowedToExportDuplicationDataAsync()
        {
            return s_tenantService.CheckLicenseWithAdditionalProduct(TenantLocalValue.LogonGroupId, PaidForProduct.OpusDiscovery)
                   && await _securityTrimmingHelper.DoesUserHasThisPermissionAsync(RMDiscoveryPermissionMasks.AccessAll);
        }
        #endregion
    }

    public class RMDiscoveryOffice365TrialLicenseControlInfo
    {
        public long LimitSize { get; set; } = (100L * 1024 * 1024 * 1024);
    }
}
