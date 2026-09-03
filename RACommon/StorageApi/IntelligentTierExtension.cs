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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Storage.Cloud.Azure;
using Storage;
using AvePoint.Application.StorageApiModern;
using Microsoft.Extensions.Caching.Memory;

using Storage.Cloud.Amazon;
using Storage.Cloud.Google;
using AvePoint.RA.CommonUtil;

namespace Microsoft365Backup.CommonUtil
{
    public enum DataAccessFrequency
    {
        Frequently = 0,
        Infrequently = 1,
        Rarely = 2
    }

    /// <summary>
    /// This extension will help you to get a sutiable data access tier for your storage system.
    /// You need to specific your data type, and the extension will help you to get a suitable tier for your data.
    /// This is mainly serve the cloud storage,since they have different access tier for different purpose.
    /// Azure: Hot for frequently access, cold for infrequently access.
    /// Amazon: Standard for frequently access, GlacierInstantRetrieval for infrequently access.
    /// Google: Standard for frequently access, Archive for infrequently access.
    /// </summary>
    public static class IntelligentTierExtension
    {
        private const int maxSizeLimit = 64;
        private static readonly RALogger logger = RALogger.GetInstance(typeof(IntelligentTierExtension));
        private static readonly MemoryCache azureDeviceSupportSetTierCache = new(new MemoryCacheOptions() { SizeLimit = maxSizeLimit, ExpirationScanFrequency = TimeSpan.FromMinutes(1) });

        public static StorageInfo ConvertToIntelligentTierInfo(this IXSystem storageSystem,StorageInfo storageInfo, DataAccessFrequency dataType)
        {
            return GetStorageInfo(storageSystem, storageInfo.HighName, storageInfo.LowName, dataType, storageInfo.MetaInfos);
        }

        public static StorageInfo GetStorageInfo(this IXSystem storageSystem, string highName, string lowName, DataAccessFrequency dataType, Dictionary<string, string> metaInfos = null)
        {
            var storageInfo = storageSystem.StorageType switch
            {
                XStorageType.Azure when IsAzureStorageV2(storageSystem) => GetAzureStorageInfo(highName, lowName, dataType),
                XStorageType.Amazon => GetAmazonCloudInfo(highName, lowName, dataType),
                XStorageType.GoogleCloud => GetGoogleCloudInfo(highName, lowName, dataType),
                _ => new StorageInfo { HighName = highName, LowName = lowName }
            };
            if (metaInfos != null)
            {
                foreach (var metaInfo in metaInfos)
                {
                    storageInfo.MetaInfos[metaInfo.Key] = metaInfo.Value;
                }
            }
            return storageInfo;
        }

        /// <summary>
        /// this need to set when process startup, and used for azure storage to handle some special cases that some data center don't support cold tier.
        /// This function is under control, if it is cold, use cold, otherwise use cool.
        /// </summary>
        /// <returns></returns>
        private static AccessTierType GetAzureDataBlobTier()
        {
            return AvePoint.Application.MediaStorageApi.StorageApiSettings.AzureBackupDataBlobTier switch
            {
                AzureBlobTier.Cool => AccessTierType.Cool,
                AzureBlobTier.Cold => AccessTierType.Cold,
                _ => AccessTierType.Cold
            };
        }

        private static GoogleCloudInfo GetGoogleCloudInfo(string highName, string lowName, DataAccessFrequency dataType)
        {
            var tier = dataType switch
            {
                DataAccessFrequency.Frequently => GoogleStorageClass.Standard,
                DataAccessFrequency.Infrequently => GoogleStorageClass.Archive,
                DataAccessFrequency.Rarely => GoogleStorageClass.Archive,
                _ => throw new NotSupportedException(dataType.ToString())
            };
            return new GoogleCloudInfo(highName, lowName) { StorageClass = tier };
        }

        private static AmazonCloudInfo GetAmazonCloudInfo(string highName, string lowName, DataAccessFrequency dataType)
        {
            var tier = dataType switch
            {
                DataAccessFrequency.Frequently => StorageClassType.Standard,
                DataAccessFrequency.Infrequently => StorageClassType.GlacierInstantRetrieval,
                DataAccessFrequency.Rarely => StorageClassType.DeepArchive,
                _ => throw new NotSupportedException(dataType.ToString())
            };
            return new AmazonCloudInfo(highName, lowName) { StorageClassTier = tier };
        }

        private static AzureCloudInfo GetAzureStorageInfo(string highName, string lowName, DataAccessFrequency dataType)
        {
            var tier = dataType switch
            {
                DataAccessFrequency.Frequently => AccessTierType.Hot,
                DataAccessFrequency.Infrequently => GetAzureDataBlobTier(),
                DataAccessFrequency.Rarely => AccessTierType.Archive,
                _ => throw new NotSupportedException(dataType.ToString())
            };
            return new AzureCloudInfo(highName, lowName) { FileTierType = tier };
        }

        private static bool IsAzureStorageV2(this IXSystem storageSystem)
        {
            if (storageSystem == null)
            {
                return false;
            }
            try
            {
                if (storageSystem.StorageType == XStorageType.Azure)
                {
                    var systemPath = storageSystem.ToXSystem().SystemPath;
                    if (systemPath != null && (azureDeviceSupportSetTierCache?.TryGetValue(systemPath, out bool result) ?? false))
                    {
                        return result;
                    }

                    var props = storageSystem.GetStorageAccountProps();

                    var azureAccountProps = props.AccountProps as AzureAccountProps;

                    logger.Info($"The current storage information: AccountKind is {azureAccountProps.AccountKind} and SkuName is {azureAccountProps.SkuName}");

                    if ((azureAccountProps.AccountKind == AzureStorageAccountKind.BlobStorage || azureAccountProps.AccountKind == AzureStorageAccountKind.StorageV2)
                        && !azureAccountProps.SkuName.StartsWith("premium", StringComparison.OrdinalIgnoreCase))
                    {
                        if (systemPath != null)
                            CacheCheckResult(systemPath, true);
                        return true;
                    }
                    if (systemPath != null)
                        CacheCheckResult(systemPath, false);
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while getting azure version. {0}", ex);
                return false;
            }

            void CacheCheckResult(string key, bool value)
            {
                if (azureDeviceSupportSetTierCache?.Count == maxSizeLimit)
                    azureDeviceSupportSetTierCache?.Compact(0.5);
                azureDeviceSupportSetTierCache?.Set(key, value,
                                new MemoryCacheEntryOptions()
                                {
                                    SlidingExpiration = TimeSpan.FromHours(10),
                                    Size = 1,
                                });
            }
        }
    }
}
