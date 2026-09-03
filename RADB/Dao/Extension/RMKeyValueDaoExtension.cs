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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Extension
{
    public static class RMKeyValueDaoExtension
    {
        public static int SubJobCountInMainJob
        {
            get
            {
                if (int.TryParse(RMGlobalConfiguration.AppConfig[RMAppSettingKey.SUB_JOB_COUNT_IN_MAIN_JOB], out var count))
                {
                    return count;
                }
                return 2;
            }
        }

        public static int MainJobCount
        {
            get
            {
                if (int.TryParse(RMGlobalConfiguration.AppConfig[RMAppSettingKey.MAX_JOBS_LIMIT_PER_TENANT], out var count))
                {
                    return count;
                }
                return 4;
            }
        }
        /// <summary>
        /// Check if cosmos bulk operation is enabled.
        /// </summary>
        /// <param name="dao"></param>
        /// <returns></returns>
        /// 
        public static bool IsCosmosBulkOperationEnabled(this IRMKeyValueDao dao)
        {
            var result = true;
            var key = $"{KeyNameCollection.COSMOS_BULK_OPERATION_ENABLED}{RMNameValueDto.Seprator}{RMNameValueType.CosmosBulkOperation}";
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;

        }
        /// <summary>
        /// Get the comsos bulk insert buffer size
        /// </summary>
        /// <param name="dao"></param>
        /// <returns></returns>
        public static int GetCosmosBulkInsertOperationBufferSize(this IRMKeyValueDao dao)
        {
            var result = default(int);
            var key = $"{KeyNameCollection.COSMOS_BULK_INSERT_OPERATION_BUFFER_SIZE}{RMNameValueDto.Seprator}{RMNameValueType.CosmosBulkOperation}";
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            int.TryParse(setting.Value, out result);

            return result;
        }

        public static int GetSubJobCountFromDB(this IRMKeyValueDao dao, int jobType)
        {
            var result = SubJobCountInMainJob;
            var key = $"{KeyNameCollection.JobConfigration}{RMNameValueDto.Seprator}{KeyNameCollection.JobType}{RMNameValueDto.Seprator}{jobType}{RMNameValueDto.Seprator}{KeyNameCollection.JobType_SubJobCount}";
            var setting = dao.Find(o => o.Key.Equals(key));
            if (setting == null) return result;
            int.TryParse(setting.Value, out result);

            return result;
        }

        public static int GetTotalSubJobCountFromDB(this IRMKeyValueDao dao, int jobType)
        {
            var key = $"{KeyNameCollection.JobConfigration}{RMNameValueDto.Seprator}{KeyNameCollection.JobType}{RMNameValueDto.Seprator}{jobType}{RMNameValueDto.Seprator}{KeyNameCollection.JobType_TotalSubJobCount}";
            var setting = dao.Find(o => o.Key.Equals(key));
            if (setting == null) return 0;
            int totalSubJobCount = 0;
            int.TryParse(setting.Value, out totalSubJobCount);

            return totalSubJobCount;
        }


        public static string GetSearchSiteFieldColumnName(this IRMKeyValueDao dao)
        {
            var result = RMGlobalConfiguration.AppConfig[RMAppSettingKey.UNIQUE_ID_SP_SEARCH_SITE_COLUMN];
            var key = KeyNameCollection.UniqueIdJob_SiteColumn_FieldName;
            var setting = dao.Find(o => o.Key.Equals(key));
            if (setting == null) return result;

            return result;
        }
        public static string GetSearchListFieldColumnName(this IRMKeyValueDao dao)
        {
            var result = RMGlobalConfiguration.AppConfig[RMAppSettingKey.UNIQUE_ID_SP_SEARCH_LIST_COLUMN];
            var key = KeyNameCollection.UniqueIdJob_ListColumn_FieldName;
            var setting = dao.Find(o => o.Key.Equals(key));
            if (setting == null) return result;

            return result;
        }

        public static bool IsExportDataEncryptionEnabled(this IRMKeyValueDao dao)
        {
            var result = false;
            var key = $"{KeyNameCollection.ExportDataEncryptionEnabled}{RMNameValueDto.Seprator}{RMNameValueType.ExportDataEncryption}";
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;
        }

        public static long GetMonitorLongRunningJobRange(this IRMKeyValueDao dao)
        {
            long result = new TimeSpan(1, 0, 0, 0).Ticks;
            var key = $"{KeyNameCollection.Monitor}{RMNameValueDto.Seprator}{RMNameValueType.MonitorLongRunningJobRange}";
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            long.TryParse(setting.Value, out result);

            return result;
        }

        public static long GetMonitorQueryScope(this IRMKeyValueDao dao)
        {
            long result = new TimeSpan(1, 0, 0, 0).Ticks;
            var key = $"{KeyNameCollection.Monitor}{RMNameValueDto.Seprator}{RMNameValueType.MonitorQueryRange}";
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            long.TryParse(setting.Value, out result);

            return result;
        }

        public static bool IsMonitorEnabled(this IRMKeyValueDao dao)
        {
            var result = false;
            var key = $"{KeyNameCollection.Monitor}{RMNameValueDto.Seprator}{RMNameValueType.EnableMonitor}";
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;
        }
        public static int GetMainJobCountFromDB(IRMKeyValueDao dao)
        {
            var result = MainJobCount;
            var key = $"{KeyNameCollection.JobConfigration}{RMNameValueDto.Seprator}{KeyNameCollection.JobType_MainJobCount}";
            var setting = dao.Find(o => o.Key.Equals(key));
            if (setting == null) return result;
            int.TryParse(setting.Value, out result);

            return result;
        }

        public static bool GetIsNestleCustomize(this IRMKeyValueDao dao)
        {
            var result = false;
            var key = KeyNameCollection.IsNestleCustomize;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;
        }

        public static bool GetForceUpdate(this IRMKeyValueDao dao)
        {
            var result = false;
            var key = KeyNameCollection.ForceUpdate;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;
        }
        public static bool IsEnableIntelligent(this IRMKeyValueDao dao)
        {
            var result = false;
            var key = KeyNameCollection.EnableIntelligent;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;
        }
        public static bool ForceEnableSO(this IRMKeyValueDao dao)
        {
            var result = false;
            var key = KeyNameCollection.ForceEnableSO;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;
        }

        public static bool EnableJPMCFileSystemFeature(this IRMKeyValueDao dao)
        {
            var result = false;
            var key = KeyNameCollection.EnableJPMCFileSystemFeature;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;
        }

        public static bool IsSupportMultipleGeoFeature(this IRMKeyValueDao dao)
        {
            if (!dao.EnableJPMCFileSystemFeature()) return false;
            var DCSupportedSetting = dao.GetValueByKey(KeyNameCollection.JPMCMultiGEODC);
            return !string.IsNullOrEmpty(DCSupportedSetting?.Value);
        }

        public static bool HasUpgradeVEOV3(this IRMKeyValueDao dao)
        {
            var result = false;
            var key = KeyNameCollection.HasUpgradeVEOV3;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;
        }

        public static bool EnableArchiveHoldSiteCollection(this IRMKeyValueDao dao)
        {
            var result = false;
            var key = KeyNameCollection.EnableArchiveHoldSiteCollection;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;
        }

        public static bool HasSupportTriggerNewJobPod(this IRMKeyValueDao dao)
        {
            var result = false;
            var key = KeyNameCollection.SupportTriggerNewJobPod;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;
        }

        public static bool HasSkipCheckRunningRestoreJob(this IRMKeyValueDao dao)
        {
            var result = false;
            var key = KeyNameCollection.SkipCheckRunningRestoreJob;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;
        }

        public static bool HasUpgradeTeams(this IRMKeyValueDao dao)
        {
            if (!dao.EnableTeamsFeature())
            {
                return false;
            }
            var result = false;
            var key = KeyNameCollection.HasUpgradeTeams;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;
        }

        public static bool EnableTeamsFeature(this IRMKeyValueDao dao)
        {
            var key = KeyNameCollection.EnableTeamsFeature;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return true;

            bool.TryParse(setting.Value, out var result);
            return result;
        }

        public static bool IsNewOpusTenant(this IRMKeyValueDao dao)
        {
            var key = KeyNameCollection.RunDisposalInRecords;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return true;

            bool.TryParse(setting.Value, out var result);
            return result;
        }

        public static bool EnableZeroShotFeature(this IRMKeyValueDao dao)
        {
            var result = false;
            var key = KeyNameCollection.EnableZeroShot;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;
        }

        public static bool EnableShowPredictReport(this IRMKeyValueDao dao)
        {
            var result = false;
            var key = KeyNameCollection.EnableShowPredictReport;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;
        }

        public static bool EnableAIRecommendationFeature(this IRMKeyValueDao dao)
        {
            var result = false;
            var key = KeyNameCollection.EnableAIRecommendation;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;
        }

        public static bool UseOldLogicParsing(this IRMKeyValueDao dao)
        {
            var result = false;
            var key = KeyNameCollection.UseOldLogicParsing;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;
        }

        public static bool EmbeddingFullDocument(this IRMKeyValueDao dao)
        {
            var result = false;
            var key = KeyNameCollection.EmbeddingFullDocument;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;
        }

        public static bool EnableExportExcelPreviewCsv(this IRMKeyValueDao dao)
        {
            var result = false;
            var key = KeyNameCollection.EnableExportExcelPreviewCsv;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;
        }

        public static bool HasSyncArchivedTeamsGroup(this IRMKeyValueDao dao)
        {
            var result = false;
            var key = KeyNameCollection.HasSyncArchivedTeamsGroup;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;
            bool.TryParse(setting.Value, out result);
            return result;
        }

        public static bool HasUpdateEmail4ArchivedSite(this IRMKeyValueDao dao)
        {
            var result = false;
            var key = KeyNameCollection.HasUpdateEmail4ArchivedSite;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;
            bool.TryParse(setting.Value, out result);
            return result;
        }

        public static int GetMigrationImportJobTimeOutMinutes(this IRMKeyValueDao dao)
        {
            var result = 60;
            var setting = dao.GetValueByKey(KeyNameCollection.MigrationImportJobTimeOutMinutes);
            if (setting == null) return result;

            int.TryParse(setting.Value, out result);

            return result;
        }

        public static int GetMigrationImportJobCount(this IRMKeyValueDao dao)
        {
            var result = 23;
            var setting = dao.GetValueByKey(KeyNameCollection.MigrationImportJobCount);
            if (setting == null) return result;

            int.TryParse(setting.Value, out result);

            return result;
        }

        public static int GetMigrationImportJobPackageCountCapacity(this IRMKeyValueDao dao)
        {
            var result = 250;
            var setting = dao.GetValueByKey(KeyNameCollection.MigrationImportJobPackageCountCapacity);
            if (setting == null) return result;

            int.TryParse(setting.Value, out result);

            return result;
        }

        public static int GetMigrationImportJobPackageSizeCapacity(this IRMKeyValueDao dao)
        {
            var defaultValue = 250 * 1024 * 1024;
            var setting = dao.GetValueByKey(KeyNameCollection.MigrationImportJobPackageSizeCapacity);
            if (setting == null) return defaultValue;

            int.TryParse(setting.Value, out var result);

            if (result == 0)
            {
                return defaultValue;
            }
            else
            {
                return result * 1024 * 1024;
            }
        }

        public static bool IsMigrationImportJobEnabled(this IRMKeyValueDao dao)
        {
            var result = false;
            var setting = dao.GetValueByKey(KeyNameCollection.EnableMigrationImportJob);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;
        }

        public static RMKeyValue GetMigrationImportJobSetting(this IRMKeyValueDao dao)
        {
            return dao.GetValueByKey(KeyNameCollection.EnableMigrationImportJob);
        }

        public static bool ShouldResetMajorVersionApprovalStatus(this IRMKeyValueDao dao)
        {
            var result = true;
            var setting = dao.GetValueByKey(KeyNameCollection.ResetMajorVersionApprovalStatus);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;
        }

        public static bool DisableChatBot(this IRMKeyValueDao dao)
        {
            var result = false;
            var key = KeyNameCollection.DisableChatBot;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;
        }

        public static bool EnableDiscoveryFeature(this IRMKeyValueDao dao)
        {
            var result = false;
            var key = KeyNameCollection.EnableDiscoveryFeature;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;
        }

        public static bool CompletedAvePointStorageUpgradeFor21V(this IRMKeyValueDao dao)
        {
            var key = KeyNameCollection.CompletedAvePointStorageUpgradeFor21V;
            var setting = dao.GetValueByKey(key);

            return bool.TryParse(setting?.Value, out var result) ? result : false;
        }
        public static void SetAvePointStorageUpgradeFor21VCompletedFlag(this IRMKeyValueDao dao)
        {
            dao.Create(new DB.Model.RMKeyValue
            {
                Key = KeyNameCollection.CompletedAvePointStorageUpgradeFor21V,
                Value = "true",
            });
        }

        /// <summary>
        /// Check if copy to another location is enabled for retention move operations.
        /// When enabled, source files will be kept (copy-only operation).
        /// When disabled, source files will be deleted after successful move to destination.
        /// </summary>
        /// <param name="dao">The key-value DAO instance</param>
        /// <returns>True if delete after move is enabled, false otherwise (default: false)</returns>
        public static bool IsEnableCopyToAnotherLocation(this IRMKeyValueDao dao)
        {
            var result = false; // Default: keep source files (safe default)
            var key = KeyNameCollection.EnableCopyToAnotherLocation;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;

            bool.TryParse(setting.Value, out result);

            return result;
        }

        /// <summary>
        /// Check if moving files to another storage location is enabled.
        /// When enabled, retention operations can move or copy files between storage devices.
        /// </summary>
        /// <param name="dao">The key-value DAO instance</param>
        /// <returns>True if move to another location is enabled, false otherwise (default: false)</returns>
        public static bool IsEnableMoveToAnotherLocation(this IRMKeyValueDao dao)
        {
            var settings = dao.FindListAsync(o => o.Key == KeyNameCollection.EnableMoveToAnotherLocation || o.Key == KeyNameCollection.EnableCopyToAnotherLocation).ExecuteAsyncTask();
            if (settings == null || settings.Count == 0) return false;
            foreach (var setting in settings)
            {
                _ = bool.TryParse(setting.Value, out bool value);
                if (value) return true;
            }
            return false;
        }

        /// <summary>
        /// Check if random exception throwing is enabled for testing purposes.
        /// When enabled, operations will randomly throw exceptions to simulate failures.
        /// </summary>
        /// <param name="dao">The key-value DAO instance</param>
        /// <returns>True if random exception testing is enabled, false otherwise (default: false)</returns>
        public static async Task<bool> IsEnableRandomExceptionForTestingAsync(this IRMKeyValueDao dao)
        {
            var setting = await dao.GetValueByKeyAsync(KeyNameCollection.EnableRandomExceptionForTesting);
            if (!string.IsNullOrEmpty(setting) && bool.TryParse(setting, out bool result))
                return result;
            return false;
        }

        /// <summary>
        /// Get the probability of throwing a random exception for testing purposes.
        /// </summary>
        /// <param name="dao">The key-value DAO instance</param>
        /// <param name="defaultValue">The default probability value if the setting is not found or invalid</param>
        /// <returns>The probability of throwing a random exception</returns>
        public static async Task<double> GetRandomExceptionProbabilityAsync(this IRMKeyValueDao dao, double defaultValue = 0.1)
        {
            var setting = await dao.GetValueByKeyAsync(KeyNameCollection.RandomExceptionProbability);
            if (!string.IsNullOrEmpty(setting) && double.TryParse(setting, out double result) && result >= 0 && result <= 1)
                return result;
            return defaultValue;
        }

        public static bool IsEnableCustomRetentionSettings(this IRMKeyValueDao dao)
        {
            var setting = dao.GetValueByKey(KeyNameCollection.EnableCustomRetentionSettings);
            if (bool.TryParse(setting?.Value, out var result))
            {
                return result;
            }
            return false;
        }

        /// <summary>
        /// Check if extended move action is enabled for retention operations.
        /// </summary>
        /// <param name="dao"></param>
        /// <returns>True if the feature is enabled, false otherwise (default: false)</returns>
        public static bool IsEnableExtendedMoveActionForRetention(this IRMKeyValueDao dao)
        {
            var setting = dao.GetValueByKey(KeyNameCollection.EnableExtendedMoveActionForRetention);
            if (bool.TryParse(setting?.Value, out var result))
            {
                return result;
            }
            return false;
        }

        public static string GetStorageIdForArchivedDataMigrationReport(this IRMKeyValueDao dao)
        {
            var setting = dao.GetValueByKey(KeyNameCollection.StorageIdForArchivedDataMigrationReport);
            if (Guid.TryParse(setting?.Value, out _))
            {
                return setting.Value;
            }
            return string.Empty;
        }

        public static int GetMaxRecordsPerCSVFile(this IRMKeyValueDao dao, int defaultValue = 500_000)
        {
            var setting = dao.GetValueByKey(KeyNameCollection.MaxRecordsPerCSVFile);
            if (int.TryParse(setting?.Value, out var result))
            {
                return result;
            }
            return defaultValue; // Default value
        }

        public static bool IsEnableJPMCFileSystemFeature(this IRMKeyValueDao dao)
        {
            var result = false;
            var key = KeyNameCollection.EnableJPMCFileSystemFeature;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;
            bool.TryParse(setting.Value, out result);
            return result;
        }

        public static bool IsDeleteArchivedSiteCollectionEnabled(this IRMKeyValueDao dao)
        {
            var result = false;
            var key = KeyNameCollection.EnableDeleteArchivedSiteCollection;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;
            _ = bool.TryParse(setting.Value, out result);
            return result;
        }

        /// <summary>
        /// Check if disable update smtp address in Teams Restore job.
        /// When true, Teams Restore job will not update email domain for restored Teams.
        /// Default: false (will update email domain)
        /// </summary>
        public static bool IsDisableUpdateTeamSmtpAddress(this IRMKeyValueDao dao)
        {
            var result = false;
            var key = KeyNameCollection.DisableUpdateTeamSmtpAddress;
            var setting = dao.GetValueByKey(key);
            if (setting == null) return result;
            _ = bool.TryParse(setting.Value, out result);
            return result;
        }

        public static int GetExoGraphDiscoverThreadsLimit(this IRMKeyValueDao dao, int defaultValue = 3)
        {
            var settings = dao.GetValueByKey(KeyNameCollection.ExoGraphDiscoverThreadsLimit);
            if (int.TryParse(settings?.Value, out var result))
            {
                return result;
            }
            return defaultValue;
        }

        public static int GetSaveProgressIntervalInSeconds(this IRMKeyValueDao dao, int defaultValue = 60)
        {
            var settings = dao.GetValueByKey(KeyNameCollection.SaveProgressIntervalInSeconds);
            if (int.TryParse(settings?.Value, out var result))
            {
                return result;
            }
            return defaultValue;
        }   
    }
}
