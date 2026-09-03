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
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using System.Threading.Tasks;
using System;
using AvePoint.RA.DB.Dao;
using Cloud.Sdk.Data.AosModern;
using Newtonsoft.Json;
using AvePoint.RA.Contract.ControlPlus;

namespace AvePoint.RA.Service.Services.Discovery.Google.License
{
    public class RMDiscoveryGoogleLicenseHelper
    {
        private const string S_DISCOVERY_TRIAL_LICENSE_CONTROL = "DISCOVERY_GOOGLE_TRIAL_LICENSE_CONTROL";
        
        private static readonly IRMDiscoveryGoogleExecutionInfoDao s_executionInfoDao = new RMDiscoveryGoogleExecutionInfoDao();
        
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMDiscoveryGoogleLicenseHelper));
        
        private static readonly IRMKeyValueDao s_keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        public static async Task<LicenseType> GetLicenseTypeAsync()
        {
            var licenseInfo = await RMAosApiClient.GetLicenseInfo(TenantLocalValue.LogonGroupId);
            s_logger.Info($"Current tenant google ROT license is [{licenseInfo.Type}].");
            return licenseInfo.Type;
        }

        public static async Task<bool> IsMeetLimitAsync()
        {
            var licenseInfo = await RMAosApiClient.GetLicenseInfo(TenantLocalValue.LogonGroupId);
            if (licenseInfo.Type == LicenseType.Trial)
            {
                var controlInfo = new RMDiscoveryGoogleDiscoveryTrialLicenseControlInfo();

                var setting = s_keyValueDao.GetValueByKey(S_DISCOVERY_TRIAL_LICENSE_CONTROL);
                if (setting != null && !string.IsNullOrWhiteSpace(setting.Value))
                {
                    controlInfo = JsonConvert.DeserializeObject<RMDiscoveryGoogleDiscoveryTrialLicenseControlInfo>(setting.Value);
                }

                var (fileTotalSize, executedCount) = await s_executionInfoDao.CalculateAllAsync(licenseInfo.Type);

                s_logger.Info($" google tenant collect file total size [{fileTotalSize}], execute job count [{executedCount}].");

                return executedCount < 3 && fileTotalSize < controlInfo.LimitSize;
            }
            var discoveryLicenseInfo = licenseInfo.GoogleROTDiscoveryLicenseInfo;
            var (_, _, currentYearCount) = await s_executionInfoDao.CalculateAsync(licenseInfo.Type);
            return discoveryLicenseInfo.FrequencyPerYear > currentYearCount;
        }

        public static async Task IncreaseConsumedFrequencyPerYearAsync()
        {

            if (await IsFromGControlWithoutDiscoveryLicenseAsync())
            {
                return ;
            }

            var aosApiClient = AosApiUtility.GetAosModerClient();
            var info = await aosApiClient.LicenseService.GetLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME);
            if (info.Type == LicenseType.Trial)
            {
                return;
            }

            if (info.Extension is CloudRecordsExtension extension)
            {
                extension.ConsumedFrequencyForGoogleWorkspace++;
                var result = await aosApiClient.LicenseService.UpdateLicenseExtensionAsync(new()
                {
                    LicenseId = info.Id,
                    Extension = extension
                });
            }
        }

        public static async Task DecreaseConsumedFrequencyPerYearAsync()
        {

            if (await IsFromGControlWithoutDiscoveryLicenseAsync())
            {
                return;
            }

            var aosApiClient = AosApiUtility.GetAosModerClient();
            var info = await aosApiClient.LicenseService.GetLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME);
            if (info.Type == LicenseType.Trial)
            {
                return;
            }
            if (info.Extension is CloudRecordsExtension extension && extension.ConsumedFrequencyForGoogleWorkspace > 0)
            {
                extension.ConsumedFrequencyForGoogleWorkspace--;
                var result = await aosApiClient.LicenseService.UpdateLicenseExtensionAsync(new()
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
                if (info.Extension is CloudRecordsExtension extension && extension.ConsumedFrequencyForGoogleWorkspace > 0)
                {
                    extension.ConsumedFrequencyForGoogleWorkspace = 0;
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
                s_logger.Error($"Clear google rot discovery license usage failed, error: {e}");
                return false;
            }
        }
        public static async Task<bool> IsFromGControlWithoutDiscoveryLicenseAsync()
        {
            if (TenantLocalValue.RequesterType == RequesterTypeEnum.OpusControlPlus)
            {
                var licenseInfo = await RMAosApiClient.GetLicenseInfo(TenantLocalValue.LogonGroupId);

                if (licenseInfo?.GoogleROTDiscoveryLicenseInfo == null)
                {
                    s_logger.Info($"Tenant {TenantLocalValue.LogonGroupId} running job from Google Control without Opus Discovery license.");
                    return true;
                }
            }

            return false;
        }
    }
    
    public class RMDiscoveryGoogleDiscoveryTrialLicenseControlInfo
    {
        public long LimitSize { get; set; } = (100L * 1024 * 1024 * 1024);
    }
}
