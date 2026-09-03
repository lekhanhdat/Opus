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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery.FileSystem;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.FileSystem;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.Service.Services.Tenant;
using Cloud.Sdk.Data.AosModern;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.FileSystem.License
{
    public class RMDiscoveryFSLicenseHelper
    {
        private const string COP_LICENSE_ACTIVATED_KEY = "FSDiscoveryCopLicenseActivated";

        private static readonly IRMSecurityTrimmingHelper _securityTrimmingHelper = PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();

        private static ITenantService s_tenantService => PlatformWindsorManager.GetService<ITenantService>();

        private static readonly IRMDiscoveryFSExecutionInfoDao s_executionInfoDao = new RMDiscoveryFSExecutionInfoDao();

        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMDiscoveryFSLicenseHelper));

        public static bool HasDiscoveryFileSystemLicense()
        {
            var hasCopLicense = s_tenantService.CheckLicenseWithAdditionalProduct(
                TenantLocalValue.LogonGroupId, PaidForProduct.OpusFileSystemDiscovery);
            var hasPermission = _securityTrimmingHelper.DoesUserHasThisPermissionAsync(RMDiscoveryFileSystemPermissionMask.AccessAll).GetAwaiter().GetResult();
            var hasPermissionFSAdmin = _securityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.FSAdmin).GetAwaiter().GetResult();
            if (hasCopLicense && hasPermission) return true;

            if (!s_tenantService.IsNewOpusTenant()) return false;

            var hasLegacyLicense = s_tenantService.CheckLicenseWithAdditionalDataSource(
                TenantLocalValue.LogonGroupId, PreviewFeature.FileSystemDiscovery);

            // Legacy customers (PreviewFeature) have FS IL → FSAdmin is available.
            // COP-only customers have OpusFileSystemDiscovery → FSDiscovery permission is available.
            if (hasLegacyLicense && hasPermissionFSAdmin) return true;

            return false;
        }

        /// <summary>
        /// Returns true if the customer is using only the legacy preview feature flag
        /// with no COP-based license. Used to skip scan frequency enforcement.
        /// </summary>
        public static bool IsLegacyPreviewOnlyCustomer()
        {
            var hasCopLicense = s_tenantService.CheckLicenseWithAdditionalProduct(
                TenantLocalValue.LogonGroupId, PaidForProduct.OpusFileSystemDiscovery);

            if (hasCopLicense) return false;

            return s_tenantService.CheckLicenseWithAdditionalDataSource(
                TenantLocalValue.LogonGroupId, PreviewFeature.FileSystemDiscovery);
        }

        /// <summary>
        /// Resets all FS discovery usage counters on the first activation of a COP license
        /// so that previous preview scans are not carried over to the new licensing model.
        /// </summary>
        public static async Task EnsureCopLicenseCounterResetAsync()
        {
            var keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
            var flag = keyValueDao.GetValueByKey(COP_LICENSE_ACTIVATED_KEY);
            if (flag != null) return;

            var hasCopLicense = s_tenantService.CheckLicenseWithAdditionalProduct(
                TenantLocalValue.LogonGroupId, PaidForProduct.OpusFileSystemDiscovery);
            if (!hasCopLicense) return;

            s_logger.Info("First COP license activation detected. Resetting FS discovery usage counters.");
            await ClearLicenseUsageAsync();

            await keyValueDao.UpsertAsync(COP_LICENSE_ACTIVATED_KEY, bool.TrueString);
        }

        public static async Task<LicenseType> GetLicenseTypeAsync()
        {
            var licenseInfo = await RMAosApiClient.GetLicenseInfo(TenantLocalValue.LogonGroupId);
            s_logger.Info($"Current tenant File System discovery license is [{licenseInfo.Type}].");
            return licenseInfo.Type;
        }

        public static async Task<bool> IsMeetLimitAsync()
        {
            var aosApiClient = AosApiUtility.GetAosModerClient();
            var info = await aosApiClient.LicenseService.GetLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME);

            if (info.Type == LicenseType.Trial)
            {
                s_logger.Info("File System discovery license is Trial, not meet limit.");
                return false;
            }
            s_logger.Info($"All modules: {string.Join(", ", info.Modules?.Select(m => m.Name) ?? [])}");

            var fsModule = info.Modules?.Find(item => item.Name.Equals(RecordsConstants.OPUS_MODULE_FileSystem_Discovery));
            if (fsModule == null || fsModule.ExpirationTime.Ticks <= DateTime.UtcNow.Ticks)
            {
                s_logger.Info("File System discovery module not found or expired.");
                return false;
            }

            if (info.Extension is not CloudRecordsExtension extension)
            {
                return false;
            }

            var (_, _, currentYearCount) = await s_executionInfoDao.CalculateAsync(info.Type);
            s_logger.Info($"FS discovery consumed frequency [{currentYearCount}], purchased frequency [{extension.PurchasedFrequencyForFileSystem}].");

            return extension.PurchasedFrequencyForFileSystem > currentYearCount;
        }

        public static async Task IncreaseConsumedFrequencyPerYearAsync()
        {
            var aosApiClient = AosApiUtility.GetAosModerClient();
            var info = await aosApiClient.LicenseService.GetLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME);

            if (info.Type == LicenseType.Trial) return;

            if (info.Extension is CloudRecordsExtension extension)
            {
                extension.ConsumedFrequencyForFileSystem++;
                await aosApiClient.LicenseService.UpdateLicenseExtensionAsync(new()
                {
                    LicenseId = info.Id,
                    Extension = extension
                });
            }
        }

        public static async Task DecreaseConsumedFrequencyPerYearAsync()
        {
            var aosApiClient = AosApiUtility.GetAosModerClient();
            var info = await aosApiClient.LicenseService.GetLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME);

            if (info.Type == LicenseType.Trial) return;

            if (info.Extension is CloudRecordsExtension extension && extension.ConsumedFrequencyForFileSystem > 0)
            {
                extension.ConsumedFrequencyForFileSystem--;
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

                if (info.Type == LicenseType.Trial) return true;

                if (info.Extension is CloudRecordsExtension extension && extension.ConsumedFrequencyForFileSystem > 0)
                {
                    extension.ConsumedFrequencyForFileSystem = 0;
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
                s_logger.Error($"Clear File System discovery license usage failed, error: {e}");
                return false;
            }
        }
    }
}