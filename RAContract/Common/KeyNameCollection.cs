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

namespace AvePoint.RA.Contract.Common
{
    public class KeyNameCollection
    {
        public const string AppManagementClientId = "appmanagementclientid";
        public const string DefaultCertificateId = "DefaultCertificateId";
        public const string COSMOS_BULK_OPERATION_ENABLED = "CosmosBulkInsertEnabled";
        public const string COSMOS_BULK_INSERT_OPERATION_BUFFER_SIZE = "CosmosBulkInsertBufferSize";
        public const string COSMOS_SCHEMA_VERSION = "CosmosSchemaVersion";
        public const string UniqueIdJob_SiteColumn_FieldName = "UniqueIdJobSearchSiteColumnFieldName";
        public const string UniqueIdJob_ListColumn_FieldName = "UniqueIdJobSearchListColumnFieldName";
        public const string MigrationImportJobTimeOutMinutes = "MigrationImportJobTimeOutMinutes";
        public const string MigrationImportJobCount = "MigrationImportJobCount";
        public const string MigrationImportJobPackageCountCapacity = "MigrationImportJobPackageCountCapacity";
        public const string MigrationImportJobPackageSizeCapacity = "MigrationImportJobPackageSizeCapacity";
        public const string EnableMigrationImportJob = "EnableMigrationImportJob";
        public const string ResetMajorVersionApprovalStatus = "ResetMajorVersionApprovalStatus";
        public const string JobConfigration = "JobConfiguration";
        public const string JobType = "JobType";
        public const string JobType_SubJobCount = "SubJobCount";
        public const string JobType_TotalSubJobCount = "TotalSubJobCount";
        public const string ExportDataEncryptionEnabled = "ExportDataEncryptionEnabled";
        public const string Monitor = "Monitor";
        public const string JobType_MainJobCount = "MainJobCount";
        public const string API_Rate_Limits = "APIRateLimit";
        public const string AES_Encrypt = "AesEncryption";
        public const string MachineLearning = "MachineLearning";
        public const string IsNestleCustomize = "IsNestleCustomize";
        public const string DisableArchiver = "DisableArchiver";
        public const string DisableRecords = "DisableRecords";
        public const string ForceUpdate = "ForceUpdate";
        public const string EnableIntelligent = "EnableIntelligent";
        public const string ForceEnableSO = "ForceEnableSO";
        public const string HasUpgradeVEOV3 = "HasUpgradeVEOV3";
        public const string EnableArchiveHoldSiteCollection = "EnableArchiveHoldSiteCollection";
        public const string SkipCheckRunningRestoreJob = "SkipCheckRunningRestoreJob";
        public const string HasUpgradeTeams = "HasUpgradeTeams";
        public const string EnableTeamsFeature = "EnableTeamsFeature";
        public const string HasSyncArchivedTeamsGroup = "HasSyncArchivedTeamsGroup";
        public const string HasUpdateEmail4ArchivedSite = "HasUpdateEmail4ArchivedSite";
        public const string HasUpgradeTeamsSettings = "HasUpgradeTeamsSettings";
        public const string HasUpgradeTeamsData = "HasUpgradeTeamsData";
        public const string EnableZeroShot = "EnableZeroShot";
        public const string EnableAIRecommendation = "EnableAIRecommendation";
        public const string EnableShowPredictReport = "EnableShowPredictReport";
        public const string EnableExportExcelPreviewCsv = "EnableExportExcelPreviewCsv";
        public const string UseOldLogicParsing = "UseOldLogicParsing";
        public const string DisableChatBot = "DisableChatBot";
        public const string EmbeddingFullDocument = "EmbeddingFullDocument";
        public const string EnableDiscoveryFeature = "EnableDiscovery";

        public const string EnableNewJobController = "EnableNewJobController";
        public const string CpuUsageForOneJob = "CpuUsageForOneJob";
        public const string MBMemoryUsageForOneJob = "MBMemoryUsageForOneJob";
        public const string IoUsageLimitPercentage = "IoUsageLimitPercentage";   

        public const string IsEnableCustomIndexMetadata = "IsEnableCustomIndexMetadata";
        public const string IsSyncStubFile = "IsSyncStubFile";
        public const string IsSCBlackListForEdiscovery = "IsSCBlackListForEdiscovery";
        public const string EnableFileSystemHighPerformanceMode = "ENABLE_HIGH_PERFORMANCE_FILESYSTEM";
        public const string JPMCUpgradeSetting = "Upgrade_Setting_JPMC";
        public const string EnableJPMCFileSystemFeature = "ENABLE_JPMC_FILE_SYSTEM_FEATURE";
        public const string JPMCMultiGEODC = "JPMC_MULTI_GEO_DC";
        public const string JPMCMultiGEOMainDC = "JPMC_MULTI_GEO_MAIN_DC";
        public const string CompletedAvePointStorageUpgradeFor21V = "CompletedAvePointStorageUpgradeFor21V";
        public const string SyncArchivedSiteInfo = "SyncArchivedSiteInfo";
        public const string SharepointidAndCountRatio = "SharepointidAndCountRatio";
        public const string SPQueryRowLimit = "SPQueryRowLimit";

        public const string MakeRestoreFailed = "MakeRestoreFailed";

        public const string EnableCopyToAnotherLocation = "EnableCopyToAnotherLocation";
        public const string EnableMoveToAnotherLocation = "EnableMoveToAnotherLocation";
        public const string EnableRandomExceptionForTesting = "EnableRandomExceptionForTesting";
        public const string RandomExceptionProbability = "RandomExceptionProbability";

        public const string RunDisposalInRecords = "RunDisposalInRecords";

        public const string EnableCustomRetentionSettings = "EnableCustomRetentionSettings";
        public const string UploadedCustomRetentionSettingsFileName = "UploadedCustomRetentionSettingsFileName";

        public const string EnableExtendedMoveActionForRetention = "EnableExtendedMoveActionForRetention";
        public const string StorageIdForArchivedDataMigrationReport = "STORAGE_ID_FOR_ARCHIVED_DATA_MIGRATION_REPORT";

        public const string MaxRecordsPerCSVFile = "MAX_RECORDS_PER_CSV_FILE";

        public const string ExoGraphDiscoverThreadsLimit = "EXO_GRAPH_DISCOVER_THREADS_LIMIT";

        public const string IsNewFullTextIndex = "IsNewFullTextIndex";
        public const string SupportDataIngestionJob = "SUPPORT_DATA_INGESTION_JOB";
        public const string SupportTriggerNewJobPod = "SUPPORT_TRIGGER_NEW_JOB_POD";
        public const string EnableDeleteArchivedSiteCollection = "EnableDeleteArchivedSiteCollection";
        public const string DisableUpdateTeamSmtpAddress = "DisableUpdateTeamSmtpAddress";
        public const string EnableFolderPath = "EnableFolderPath";
        public const string SyncFailedCommonTable = "SyncFailedCommonTable";

        public const string SaveProgressIntervalInSeconds = "SAVE_PROGRESS_INTERVAL_IN_SECONDS";
    }
}
