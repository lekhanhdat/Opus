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
using System.Text;
using AvePoint.GCommon.Contract.CentralAdmin.Object;

namespace AvePoint.GCommon.Contract.AveModuleContract
{
    public enum JobTypes : int
    {
        BackupJob = 1,
        CASearchJob = 2,
        CAJob = 3,
        BackupJobFB = 4,
        BackupJobIB = 5,
        BackupJobDB = 6,
        ContentManagerJob = 7,
        RestoreJob = 8,
        Replicator = 9,
        PRBackupJobFB = 10,
        PRBackupJobIB = 11,
        PRBackupJobDB = 12,
        PRRestoreJob = 13,
        SOStubRetentionExtender = 15,
        SOConvertStubToContent = 16,
        DesignManagerJob = 17,
        SOConfigStubDB = 18,
        SOExtenderScheduled = 19,
        RCCollectorJob = 20,
        DeploymentManagerJob = 21,
        FrontendDeployment = 22,
        SoluctionCenter = 23,
        ArchiverScan = 24,
        LogManager = 25,
        #region CM improt & exprot
        //这两个JobType在CM中并未使用
        ContentManagerImportJob = 26,
        ContentManagerExportJob = 27,
        #endregion
        ArchiverRestore = 28,
        ArchiverBackup = 29,
        ArchiverMergeIndex = 30,
        PRMaintenanceJob = 31,
        MetadataService = 32,
        RPRealTime = 33,
        RPConflict = 34,
        ArchiverRetention = 35,
        JobPruning = 36,
        ConnectorSync = 37,
        PRJobRentention = 38,
        LanguageTranslater = 39,
        FileMigrationJob = 40,
        SPMigration07_10 = 41,
        EndUserArchiverBackup = 42,
        eRoomMigrationJob = 43,
        LivelinkMigrationJob = 44,
        NotesMigrationJob = 45,
        EndUserMergeIndex = 46,
        ExtenderDataUpgrade = 47,
        GranularRetention = 48,
        ArchiverDeleteDataCollection = 49,
        UpgradeImportData = 50,
        ArchiverUpgradeData = 51,
        VaultScanJob = 52,
        ArchiverApproveAlert = 53,
        ArchiverEmailAlert = 54,
        ReplicatorImportPlan = 55,
        PublicFolderMigration = 56,
        FileMigrationGenerateExcelFile = 57,
        ArchiverFullTextIndexJob = 58,
        UpgradeSolutionData = 59,
        EndUserRestore = 60,
        ArchiverIncrementalScan = 61,
        PRNAMigrationDbAndIndex = 62,
        FarmRebuildJob = 63,
        PRJobRetentionForSN = 64,
        VaultExportJob = 65,
        PRNAMigrationDb = 67,
        PRNAMigrationIndex = 68,
        LogManagerByJob = 69,
        #region 70--79 eDiscovery占用
        EDContentSourceJob = 70,
        EDSearchJob = 71,
        EDDownloadSearchResult = 72,
        EDHoldJob = 73,
        EDReleaseJob = 74,
        EDRealTimeJob = 75,
        EDSyncJob = 76,
        EDExportJob = 77,
        EDExtention6 = 78,
        EDExtention7 = 79,
        #endregion
        DeploymentManagerUpload = 81,
        #region SPMigration
        SPMigration07Export = 80,
        SPMigration07_10_Import = 90,
        SPMigration07_13_Import = 91,
        SPMigration10_13_Import = 92,

        SPMigration10_13Remote = 94,
        SPMigration07_13Remote = 95,
        SPMigration07_13 = 112,
        SPMigration10_13 = 113,
        SPMigration10Export = 114,

        #region SP2016
        SPMigration13Export = 520,
        SPMigration07_16_Import = 521,
        SPMigration10_16_Import = 522,
        SPMigration13_16_Import = 523,
        SPMigration07_16 = 524,
        SPMigration10_16 = 525,
        SPMigration13_16 = 526,
        SPMigration13_13Remote = 527,
        #endregion

        #endregion
        #region Other module run Granular Backup&Restore job type
        CMBackupJob = 85,
        ReplicatorBackupJob = 86,
        DPMBackupJob = 87,
        CMRestoreJob = 84,
        ReplicatorRestoreJob = 88,
        DPMRestoreJob = 89,
        #endregion
        EBSStubUpgrade = 99,
        PRDataManagerIndex = 100,
        EndUserArchiverSyncJob = 116,
        ArchiverRetentionApprovalExport = 117,
        ArchiverApprovalExport = 118,
        ArchiverTestJob = 119,
        QuickPlaceMigrationJob = 110,
        DocumentumMigrationJob = 111,
        SRMAnalyzeSqlBackup = 120,
        SRMRestoreFromSQLBackup = 121,
        HASyncJobFB = 122,
        HASyncJobIB = 123,
        HAFailoverJob = 124,
        HAFallbackJob = 125,
        GranularEndUserRestore = 126,
        SOStorageReport = 127,
        SOMoveBlobToolJob = 128,
        HAPreScan = 129,
        SOExportLocation = 130,
        ArchiverVEOMergeJob = 131,
        //connector
        ConnectorSyncNow = 137,
        ConnectorReportJob = 138,
        //Replicator 
        RPHealthCheckJob = 139,
        //Granular
        GranularAdHocBackupJob = 140,
        GranularOOPRestoreJob = 141,
        ReportCollector = 142,

        GranularSyncDataJob = 143,
        PlatformSyncDataJob = 144,
        //DPM
        DMCompareReport = 150,
        DMSPAppUpdate = 151,
        DMSPAPPPushUpdate = 152,
        SOExtenderScheduledIncremental = 153,
        ArchiverLifecycleBackup = 155,

        FSArchiverScan = 160,
        FSArchiverBackupFull = 161,
        FSArchiverBackupInc = 162,
        FSArchiverTestJob = 163,
        FSArchiverFullTextIndex = 164,
        FSArchiverDownloadJob = 165,
        FSArchiverMergeIndex = 166,
        FSArchiverScanInc = 167,

        PhysicalArchiver = 168,

        #region 200-299 RC Used
        RCStorageTrendsJob = 200,
        RCCheckOutDocumentsJob = 201,
        RCSiteCollectionLoadTimeJob = 202,
        RCSearchUsageJob = 203,
        RCUserStorageSizeJob = 204,
        RCDiskSpaceMonitoringJob = 205,
        RCUsageCollectorJob = 206,
        RCSiteVisitorsAndActivityJob = 207,
        RCSiteActivityRankingJob = 208,
        RCPageTrafficJob = 209,
        RCActiveUsersJob = 210,
        RCIISLogJob = 211,
        RCDownloadRankingJob = 212,
        RCFailedLoginAttemptsJob = 213,
        RCReferrersJob = 214,
        RCBlobCalculatorJob = 215,
        RCBlobGenerateRawDataJob = 216,
        RCWorkflowStatusJob = 217,
        RCContentTypeUsageJob = 218,
        RCSharePointAlertJob = 219,
        RCLastAccessedTimeJob = 220,
        RCTermStorageChangesJob = 221,
        RCConfigurationReportsJob = 222,
        RCBestPracticeReportsJob = 223,
        RCAuditControllerRetrieveJob = 224,
        RCAuditControllerApplyJob = 225,
        RCAuditPruningJob = 226,
        RCAuditRestoreJob = 227,
        RCAuditorReportsJob = 228,
        RCContentTypeChangesJob = 229,
        RCUserLifecycleJob = 230,
        RCListAccessJob = 231,
        RCItemLifecycleJob = 232,
        RCSiteActionsJob = 233,
        RCListDeletionJob = 234,
        RCPermissionChangesJob = 235,
        RCCustomizedReportJob = 236,

        RCActiveUsersExportJob = 237,
        RCBlobCalculatorExportJob = 238,
        RCBlobRawDataExportJob = 239,
        RCCheckOutDocumentsExportJob = 240,
        RCContentContributorExportJob = 241,
        RCContentTypeUsageExportJob = 242,
        RCSiteCollectionComparisonExportJob = 243,
        RCDiskSpaceMonitoringExportJob = 244,
        RCDocAveAuditExportJob = 245,
        RCDownloadRankingExportJob = 246,
        RCFailedLoginExportJob = 247,
        RCJobPerformanceExportJob = 248,
        RCLastAccessTimeExportJob = 249,
        RCLoadTimeExportJob = 250,
        RCPageTrafficExportJob = 251,
        RCReferrersExportJob = 252,
        RCSiteActivityRankingExportJob = 253,
        RCSearchUsageExportJob = 254,
        RCSiteVisitorsAndActivityExportJob = 255,
        RCSPAlertsExportJob = 256,
        RCSPServiceExportJob = 257,
        RCStorageTrendsExportJob = 258,
        RCUserStorageSizeExportJob = 259,
        RCWorkflowStatusExportJob = 260,
        RCTermStoreChangesExportJob = 261,
        RCBlogActivityExportJob = 262,
        RCWebPartUsageCollectorJob = 266,
        RCLastAccessTimeDocumentLevelJob = 272,
        #endregion

        #region 300-399 CA Used

        #region Farm Level

        CAFarmNewWebAppJob = 300,
        CAFarmAdminSearchJob = 301,
        CAFarmSearchDuplicateFileJob = 302,
        CAFarmSecuritySearchJob = 303,
        CAFarmCloneUserPermissionJob = 304,
        CAFarmImportConfigurationFileJob = 305,
        CAFarmDeadAccountCleanerJob = 306,

        #endregion

        #region Web App Level

        CAWebAppAdminSearchJob = 310,
        CAWebAppDeleteOrphanSiteJob = 311,
        CAWebAppSearchWebPartJob = 312,
        CAWebAppSearchDuplicateFileJob = 313,
        CAWebAppSecuritySearchJob = 314,
        CAWebAppCloneUserPermissionJob = 315,
        CAWebAppDeadAccountCleanerJob = 316,

        #endregion

        #region Site Collection Level

        CASiteCollectionMoveSiteCollectionJob = 320,
        CASiteCollectionAdminSearchJob = 321,
        CASiteCollectionCheckBrokenLinkJob = 322,
        CASiteCollectionSearchWebPartJob = 323,
        CASiteCollectionSearchDuplicateFileJob = 324,
        CASiteCollectionSecuritySearchJob = 325,
        CASiteCollectionCloneUserPermissionJob = 326,
        CASiteCollectionCloneSitePermissionJob = 327,
        CASiteCollectionStopInheritingPermissionsJob = 328,
        CASiteCollectionDeadAccountCleanerJob = 329,

        #endregion

        #region Site Level

        CASiteAdminSearchJob = 331,
        CASiteCheckBrokenLinkJob = 332,
        CASiteSearchWebPartJob = 333,
        CASiteSearchDuplicateFileJob = 334,
        CASiteSecuritySearchJob = 370,
        CASiteCloneUserPermissionsJob = 335,
        CASiteCloneSitePermissionJob = 336,
        CASiteStopInheritingPermissionsJob = 337,
        CASiteDeadAccountCleanerJob = 338,

        #endregion

        #region List Level

        CAListAdminSearchJob = 340,
        CAListSecuritySearchJob = 341,
        CAListCloneUserPermissionJob = 342,
        CAListCloneListLibraryPermissionJob = 343,
        CAListStopInheritingPermissionsJob = 344,
        CAListInheritPermissionsJob = 345,

        #endregion

        #region Folder Level

        CAFolderAdminSearchJob = 350,
        CAFolderSecuritySearchJob = 351,
        CAFolderCloneUserPermissionJob = 352,
        CAFolderCloneFolderPermissionJob = 353,
        CAFolderStopInheritingPermissionsJob = 354,
        CAFolderInheritPermissionsJob = 355,

        #endregion

        #region Item Level

        CAItemAdminSearchJob = 360,
        CAItemSecuritySearchJob = 361,
        CAItemChangeMetadataJob = 363,

        #endregion

        #region 以下类型Job不区分Level

        CADeleteTempPermissionJob = 371,
        CAOfflineExportReportJob = 372,
        CAProfileJob = 373,
        /// <summary>
        /// PE Job中只有Auditor类型的Rule
        /// </summary>
        CAOnlyAuditorRulePEJob = 374,
        CAOnlyScanRulePEJob = 375,
        CAEventReceiverPEJob = 376,

        #endregion

        #region restore from live DB

        SSDMRestoreFromLiveDBJob = 380,

        #endregion

        #region farm repair

        PRFarmRepairJob = 381,

        #endregion

        #region analyze VHD backup

        SSDMAnalyzeVHDBackup = 385,

        #endregion

        #endregion

        HealthAnalyzer = 400,

        #region ssdm delete temp db
        SSDMJobRetention = 401,
        #endregion
        #region VM
        VMBackupJobFB = 402,
        VMBackupJobIB = 403,
        VMBackupJobDB = 404,
        VMRestore = 405,
        VMDataManager = 406,
        VMJobRetention = 407,
        #endregion
        RPDeploymentJob = 450,

        #region farm rebuild OOP
        PRFarmCloneJob = 470,
        #endregion

        #region farm rebuild with VM
        PRFarmRebuildWithVMJob = 471,
        #endregion

        #region SMSP Provision
        PRStorageProvisionJob = 472,

        PRSnapMirrorProvisionJob = 473,

        PRSnapMirrorDiscoverJob = 474,
        #endregion SMSP Provision

        HAPreFailoverJob = 475,

        #region High Speed Migration take 500-510
        FileHighSpeedMigrationJob = 500,
        FileHighSpeedMigrationGenerateExcelJob = 501,
        LivelinkHighSpeedMigrationJob = 502,
        DocumentumHighSpeedMigrationJob = 503,
        eRoomHighSpeedMigrationJob = 504,
        NotesHighSpeedMigrationJob = 505,
        #endregion

        #region SharePoint High Speed Migration take 510-520
        SharePoint07HighSpeedMigrationImportJob = 510,
        SharePoint07HighSpeedMigrationExportJob = 511,
        SharePoint07HighSpeedMigrationOnlineJob = 512,
        SharePoint10HighSpeedMigrationImportJob = 513,
        SharePoint10HighSpeedMigrationExportJob = 514,
        SharePoint10HighSpeedMigrationOnlineJob = 515,
        SharePoint13HighSpeedMigrationImportJob = 516,
        SharePoint13HighSpeedMigrationExportJob = 517,
        SharePoint13HighSpeedMigrationOnlineJob = 518,
        #endregion

        PRBackupJobFBforSMSP = 529,
        PRRestoreJobforSMSP = 530,
        PRMaintenanceJobforSMSP = 531,
        PRNAMigrationDbforSMSP = 532,
        PRNAMigrationIndexforSMSP = 533,
        FarmRebuildJobforSMSP = 534,
        PRFarmRepairJobforSMSP = 535,
        PRFarmRebuildWithVMJobforSMSP = 536,
        PRFarmCloneJobforSMSP = 537,
        LicenseManager = 538,
        Office365AutoScan = 540,
        #region Records Job Type 541 - 549
        //Records Job
        RecordsDataSync = 541,
        RecordsSharepointSetting = 542,
        RecordsUniqueID = 543,
        RecordsDisposalReport = 544,
        RecordsDestructionReport = 545,
        RecordsTermUsageReport = 546,
        RecordsFSDataSync = 547,
        RecordsMove = 548,
        RecordsForceRetention = 549,
        #endregion

        #region SP2019 550-559
        SPMigration16Export = 550,
        SPMigration07_19_Import = 551,
        SPMigration10_19_Import = 552,
        SPMigration13_19_Import = 553,
        SPMigration16_19_Import = 554,
        SPMigration07_19 = 555,
        SPMigration10_19 = 556,
        SPMigration13_19 = 557,
        SPMigration16_19 = 558,
        SPMigration16_13Remote = 559,
        #endregion

        #region SharePoint High Speed Migration 2016 take 560-562
        SharePoint16HighSpeedMigrationExportJob = 560,
        SharePoint16HighSpeedMigrationImportJob = 561,
        SharePoint16HighSpeedMigrationOnlineJob = 562,
        #endregion

        #region REcords FS 570~590

        RecordsFSReclassify = 570, 
        RecordsFSFolderHold = 571,
        #endregion

        #region New Physical Job
        PhysicalExplorerTimer = 580,
        RecordsAvailableSpaceReport = 581,
        #endregion

        #region SP 07 10 13 16 Import to Online 600~610
        SPMigration07_Remote_Import = 600,
        SPMigration10_Remote_Import = 601,
        SPMigration13_Remote_Import = 602,
        SPMigration16_Remote_Import = 603,
        #endregion

    }

    public class JobTypeAgentTypeMappings
    {
        private static Dictionary<JobTypes, List<string>> Mappings { get; set; }

        private static object Loker = new object();

        public static void InitMappings()
        {
            lock (Loker)
            {
                if (Mappings == null)
                {
                    Mappings = new Dictionary<JobTypes, List<string>>();

                    Mappings[JobTypes.RecordsSharepointSetting] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.RecordsDataSync] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.RecordsFSDataSync] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.RecordsUniqueID] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.RecordsDisposalReport] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.RecordsDestructionReport] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.RecordsTermUsageReport] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.RecordsAvailableSpaceReport] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.RecordsMove] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.RecordsForceRetention] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.PhysicalArchiver] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.PhysicalExplorerTimer] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.RecordsFSReclassify] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.RecordsFSFolderHold] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.FSArchiverScan] = new List<string>() { AgentTypes.AGENT_TYPE_FILE_SYSTEM_ARCHIVER };
                    Mappings[JobTypes.FSArchiverBackupFull] = new List<string>() { AgentTypes.AGENT_TYPE_FILE_SYSTEM_ARCHIVER };
                    Mappings[JobTypes.FSArchiverBackupInc] = new List<string>() { AgentTypes.AGENT_TYPE_FILE_SYSTEM_ARCHIVER };
                    Mappings[JobTypes.FSArchiverTestJob] = new List<string>() { AgentTypes.AGENT_TYPE_FILE_SYSTEM_ARCHIVER };
                    Mappings[JobTypes.ArchiverBackup] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.ArchiverLifecycleBackup] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.ArchiverMergeIndex] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.ArchiverRestore] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.ArchiverDeleteDataCollection] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.ArchiverScan] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.ArchiverIncrementalScan] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.ArchiverApprovalExport] = new List<string> { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.ArchiverTestJob] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.EndUserArchiverBackup] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.EndUserMergeIndex] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.EndUserRestore] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.EndUserArchiverSyncJob] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.ArchiverApproveAlert] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.ArchiverEmailAlert] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.ArchiverVEOMergeJob] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    Mappings[JobTypes.ArchiverRetentionApprovalExport] = new List<string>() { AgentTypes.AGENT_TYPE_ARCHIVER };
                    #region Granular Backup
                    Mappings[JobTypes.BackupJob] = new List<string>() { AgentTypes.AGENT_TYPE_SITE_LEVEL, AgentTypes.AGENT_TYPE_ITEM_LEVEL, AgentTypes.AGENT_TYPE_SUBSITE_LEVEL };
                    Mappings[JobTypes.BackupJobDB] = new List<string>() { AgentTypes.AGENT_TYPE_SITE_LEVEL, AgentTypes.AGENT_TYPE_ITEM_LEVEL, AgentTypes.AGENT_TYPE_SUBSITE_LEVEL };
                    Mappings[JobTypes.BackupJobFB] = new List<string>() { AgentTypes.AGENT_TYPE_SITE_LEVEL, AgentTypes.AGENT_TYPE_ITEM_LEVEL, AgentTypes.AGENT_TYPE_SUBSITE_LEVEL };
                    Mappings[JobTypes.BackupJobIB] = new List<string>() { AgentTypes.AGENT_TYPE_SITE_LEVEL, AgentTypes.AGENT_TYPE_ITEM_LEVEL, AgentTypes.AGENT_TYPE_SUBSITE_LEVEL };
                    Mappings[JobTypes.GranularAdHocBackupJob] = new List<string>() { AgentTypes.AGENT_TYPE_SITE_LEVEL, AgentTypes.AGENT_TYPE_ITEM_LEVEL, AgentTypes.AGENT_TYPE_SUBSITE_LEVEL };
                    Mappings[JobTypes.GranularSyncDataJob] = new List<string>() { AgentTypes.AGENT_TYPE_SITE_LEVEL, AgentTypes.AGENT_TYPE_ITEM_LEVEL, AgentTypes.AGENT_TYPE_SUBSITE_LEVEL };
                    #endregion
                    #region CA
                    foreach (JobTypes caJobType in Enum.GetValues(typeof(JobTypes)))
                    {
                        if (Enum.IsDefined(typeof(CAJobTypes), (int)caJobType))
                        {
                            Mappings[caJobType] = new List<string> { AgentTypes.AGENT_TYPE_SMS };
                        }
                    }
                    #endregion
                    Mappings[JobTypes.ConnectorSync] = new List<string>() { AgentTypes.AGENT_TYPE_CONNECTOR };
                    Mappings[JobTypes.ConnectorSyncNow] = new List<string>() { AgentTypes.AGENT_TYPE_CONNECTOR };
                    Mappings[JobTypes.ConnectorReportJob] = new List<string>() { AgentTypes.AGENT_TYPE_CONNECTOR };
                    Mappings[JobTypes.ContentManagerExportJob] = new List<string>() { AgentTypes.AGENT_TYPE_CONTENT_MANAGER2010 };
                    Mappings[JobTypes.ContentManagerImportJob] = new List<string>() { AgentTypes.AGENT_TYPE_CONTENT_MANAGER2010 };
                    Mappings[JobTypes.ContentManagerJob] = new List<string>() { AgentTypes.AGENT_TYPE_CONTENT_MANAGER2010 };
                    Mappings[JobTypes.DeploymentManagerJob] = new List<string>() { AgentTypes.AGENT_TYPE_DEPLOYMENT_SITE_LEVEL };
                    Mappings[JobTypes.DesignManagerJob] = new List<string>() { AgentTypes.AGENT_TYPE_DEPLOYMENT_SITE_LEVEL };
                    Mappings[JobTypes.FrontendDeployment] = new List<string>() { AgentTypes.AGENT_TYPE_DEPLOYMENT_SITE_LEVEL };
                    Mappings[JobTypes.MetadataService] = new List<string>() { AgentTypes.AGENT_TYPE_DEPLOYMENT_SITE_LEVEL };
                    Mappings[JobTypes.DMCompareReport] = new List<string>() { AgentTypes.AGENT_TYPE_DEPLOYMENT_SITE_LEVEL };
                    Mappings[JobTypes.DMSPAppUpdate] = new List<string>() { AgentTypes.AGENT_TYPE_DEPLOYMENT_SITE_LEVEL };
                    Mappings[JobTypes.DMSPAPPPushUpdate] = new List<string>() { AgentTypes.AGENT_TYPE_DEPLOYMENT_SITE_LEVEL };
                    Mappings[JobTypes.PRBackupJobDB] = new List<string>() { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.PRBackupJobFB] = new List<string>() { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.PRBackupJobIB] = new List<string>() { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.PRFarmRepairJob] = new List<string>() { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.PRFarmRebuildWithVMJob] = new List<string>() { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.PRJobRentention] = new List<string>() { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.PRMaintenanceJob] = new List<string>() { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.PlatformSyncDataJob] = new List<string>() { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.PRRestoreJob] = new List<string>() { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.RCCollectorJob] = new List<string>() { AgentTypes.AGENT_TYPE_REPORT_CENTER };
                    Mappings[JobTypes.Replicator] = new List<string>() { AgentTypes.AGENT_TYPE_REPLICATOR };
                    #region Granular Restore
                    Mappings[JobTypes.RestoreJob] = new List<string>() { AgentTypes.AGENT_TYPE_SITE_LEVEL, AgentTypes.AGENT_TYPE_ITEM_LEVEL, AgentTypes.AGENT_TYPE_SUBSITE_LEVEL };
                    Mappings[JobTypes.GranularOOPRestoreJob] = new List<string>() { AgentTypes.AGENT_TYPE_SITE_LEVEL, AgentTypes.AGENT_TYPE_ITEM_LEVEL, AgentTypes.AGENT_TYPE_SUBSITE_LEVEL };
                    Mappings[JobTypes.GranularEndUserRestore] = new List<string>() { AgentTypes.AGENT_TYPE_SITE_LEVEL, AgentTypes.AGENT_TYPE_ITEM_LEVEL, AgentTypes.AGENT_TYPE_SUBSITE_LEVEL };
                    #endregion
                    Mappings[JobTypes.RPConflict] = new List<string>() { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.RPRealTime] = new List<string>() { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.SOConvertStubToContent] = new List<string>() { AgentTypes.AGENT_TYPE_REAL_TIME_ARCHIVE, AgentTypes.AGENT_TYPE_CONNECTOR };
                    Mappings[JobTypes.SOExtenderScheduled] = new List<string>() { AgentTypes.AGENT_TYPE_REAL_TIME_ARCHIVE };
                    Mappings[JobTypes.SOStorageReport] = new List<string>() { AgentTypes.AGENT_TYPE_REAL_TIME_ARCHIVE, AgentTypes.AGENT_TYPE_CONNECTOR };
                    Mappings[JobTypes.SOExportLocation] = new List<string>() { AgentTypes.AGENT_TYPE_REAL_TIME_ARCHIVE, AgentTypes.AGENT_TYPE_CONNECTOR };
                    Mappings[JobTypes.SOExtenderScheduledIncremental] = new List<string>() { AgentTypes.AGENT_TYPE_REAL_TIME_ARCHIVE };
                    Mappings[JobTypes.SoluctionCenter] = new List<string>() { AgentTypes.AGENT_TYPE_DEPLOYMENT_SITE_LEVEL };
                    Mappings[JobTypes.SOStubRetentionExtender] = new List<string>() { AgentTypes.AGENT_TYPE_REAL_TIME_ARCHIVE, AgentTypes.AGENT_TYPE_CONNECTOR };
                    #region SharePoint Migration
                    Mappings[JobTypes.SPMigration07_10] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_07_10 };
                    Mappings[JobTypes.SPMigration07_13] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_07_13 };
                    Mappings[JobTypes.SPMigration10_13] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };
                    Mappings[JobTypes.SPMigration07_16] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_07_13 };
                    Mappings[JobTypes.SPMigration10_16] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };
                    Mappings[JobTypes.SPMigration13_16] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };
                    Mappings[JobTypes.SPMigration07Export] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_07_10 };
                    Mappings[JobTypes.SPMigration10Export] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_07_10 };
                    Mappings[JobTypes.SPMigration13Export] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_07_10 };
                    Mappings[JobTypes.SPMigration07_10_Import] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_07_10 };
                    Mappings[JobTypes.SPMigration07_13_Import] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_07_10 };
                    Mappings[JobTypes.SPMigration10_13_Import] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_07_10 };
                    Mappings[JobTypes.SPMigration07_16_Import] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_07_10 };
                    Mappings[JobTypes.SPMigration10_16_Import] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_07_10 };
                    Mappings[JobTypes.SPMigration13_16_Import] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_07_10 };
                    Mappings[JobTypes.SPMigration07_13Remote] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_07_13 };
                    Mappings[JobTypes.SPMigration10_13Remote] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };
                    Mappings[JobTypes.SPMigration13_13Remote] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };

                    Mappings[JobTypes.SPMigration16Export] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };
                    Mappings[JobTypes.SPMigration07_19_Import] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };
                    Mappings[JobTypes.SPMigration10_19_Import] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };
                    Mappings[JobTypes.SPMigration13_19_Import] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };
                    Mappings[JobTypes.SPMigration16_19_Import] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };
                    Mappings[JobTypes.SPMigration07_19] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };
                    Mappings[JobTypes.SPMigration10_19] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };
                    Mappings[JobTypes.SPMigration13_19] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };
                    Mappings[JobTypes.SPMigration16_19] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };
                    Mappings[JobTypes.SPMigration16_13Remote] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };

                    Mappings[JobTypes.SPMigration07_Remote_Import] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_07_13 };
                    Mappings[JobTypes.SPMigration10_Remote_Import] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };
                    Mappings[JobTypes.SPMigration13_Remote_Import] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };
                    Mappings[JobTypes.SPMigration16_Remote_Import] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };

                    Mappings[JobTypes.SharePoint07HighSpeedMigrationOnlineJob] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_07_13 };
                    Mappings[JobTypes.SharePoint10HighSpeedMigrationOnlineJob] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };
                    Mappings[JobTypes.SharePoint13HighSpeedMigrationOnlineJob] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };
                    Mappings[JobTypes.SharePoint16HighSpeedMigrationOnlineJob] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };
                    Mappings[JobTypes.SharePoint07HighSpeedMigrationExportJob] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_07_13 };
                    Mappings[JobTypes.SharePoint10HighSpeedMigrationExportJob] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };
                    Mappings[JobTypes.SharePoint13HighSpeedMigrationExportJob] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };
                    Mappings[JobTypes.SharePoint16HighSpeedMigrationExportJob] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };
                    Mappings[JobTypes.SharePoint07HighSpeedMigrationImportJob] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_07_13 };
                    Mappings[JobTypes.SharePoint10HighSpeedMigrationImportJob] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };
                    Mappings[JobTypes.SharePoint13HighSpeedMigrationImportJob] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };
                    Mappings[JobTypes.SharePoint16HighSpeedMigrationImportJob] = new List<string>() { AgentTypes.AGENT_TYPE_MIGRATION_10_13 };
                    #endregion

                    Mappings[JobTypes.ExtenderDataUpgrade] = new List<string>() { AgentTypes.AGENT_TYPE_REAL_TIME_ARCHIVE, AgentTypes.AGENT_TYPE_CONNECTOR };
                    Mappings[JobTypes.EBSStubUpgrade] = new List<string>() { AgentTypes.AGENT_TYPE_REAL_TIME_ARCHIVE, AgentTypes.AGENT_TYPE_CONNECTOR };
                    Mappings[JobTypes.EDContentSourceJob] = new List<string>() { AgentTypes.AGENT_TYPE_EDISCOVERY };
                    Mappings[JobTypes.PRNAMigrationDbAndIndex] = new List<string> { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.VaultScanJob] = new List<string>() { AgentTypes.AGENT_TYPE_COMPLIANCE_VAULT };
                    Mappings[JobTypes.VaultExportJob] = new List<string>() { AgentTypes.AGENT_TYPE_COMPLIANCE_VAULT };
                    #region Other module run Granular Backup&Restore job type
                    Mappings[JobTypes.CMBackupJob] = new List<string>() { AgentTypes.AGENT_TYPE_CONTENT_MANAGER2010 };
                    Mappings[JobTypes.ReplicatorBackupJob] = new List<string>() { AgentTypes.AGENT_TYPE_REPLICATOR };
                    Mappings[JobTypes.DPMBackupJob] = new List<string>() { AgentTypes.AGENT_TYPE_DEPLOYMENT_SITE_LEVEL };
                    Mappings[JobTypes.CMRestoreJob] = new List<string>() { AgentTypes.AGENT_TYPE_CONTENT_MANAGER2010 };
                    Mappings[JobTypes.ReplicatorRestoreJob] = new List<string>() { AgentTypes.AGENT_TYPE_REPLICATOR };
                    Mappings[JobTypes.DPMRestoreJob] = new List<string>() { AgentTypes.AGENT_TYPE_DEPLOYMENT_SITE_LEVEL };
                    #endregion
                    #region - eDiscovery job type -
                    Mappings[JobTypes.EDHoldJob] = new List<string> { AgentTypes.AGENT_TYPE_EDISCOVERY };
                    Mappings[JobTypes.EDExportJob] = new List<string> { AgentTypes.AGENT_TYPE_EDISCOVERY };
                    Mappings[JobTypes.EDReleaseJob] = new List<string> { AgentTypes.AGENT_TYPE_EDISCOVERY };
                    Mappings[JobTypes.EDSearchJob] = new List<string> { AgentTypes.AGENT_TYPE_EDISCOVERY };
                    Mappings[JobTypes.EDSyncJob] = new List<string> { AgentTypes.AGENT_TYPE_EDISCOVERY };
                    Mappings[JobTypes.EDDownloadSearchResult] = new List<string> { AgentTypes.AGENT_TYPE_EDISCOVERY };
                    //                    Mappings[JobTypes.EDSearchArchiveJob] = new List<string> { AgentTypes.AGENT_TYPE_EDISCOVERY };
                    #endregion
                    Mappings[JobTypes.SRMAnalyzeSqlBackup] = new List<string>() { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.SRMRestoreFromSQLBackup] = new List<string>() { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.SSDMRestoreFromLiveDBJob] = new List<string>() { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.SSDMAnalyzeVHDBackup] = new List<string>() { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.SSDMJobRetention] = new List<string>() { AgentTypes.AGENT_TYPE_PR_CONTROL };
                    Mappings[JobTypes.HAFailoverJob] = new List<string>() { AgentTypes.AGENT_TYPE_HIGH_AVAILABILITY_CONTROL };
                    Mappings[JobTypes.HAFallbackJob] = new List<string>() { AgentTypes.AGENT_TYPE_HIGH_AVAILABILITY_CONTROL };
                    Mappings[JobTypes.HASyncJobFB] = new List<string>() { AgentTypes.AGENT_TYPE_HIGH_AVAILABILITY_CONTROL };
                    Mappings[JobTypes.HASyncJobIB] = new List<string>() { AgentTypes.AGENT_TYPE_HIGH_AVAILABILITY_CONTROL };
                    Mappings[JobTypes.HAPreScan] = new List<string>() { AgentTypes.AGENT_TYPE_HIGH_AVAILABILITY_CONTROL };
                }

            }

        }

        public static List<string> GetAgentTypes(int jobType)
        {
            InitMappings();

            return Mappings[(JobTypes)jobType];
        }
    }
}

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    /// <summary>
    /// 此枚举中JobType定义和JobTypes.cs文件中的定义保持一致, 修改任意其中一个文件则需要同时修改另一个文件
    /// </summary>
    public enum CAJobTypes : int
    {
        /// <summary>
        /// 原始JobType, 6.2之前只有以下两种JobType
        /// </summary>
        CASearchJob = 2,
        /// <summary>
        /// 原始JobType, 6.2之前只有以下两种JobType
        /// </summary>
        CAJob = 3,

        #region 300-399 CA Used

        #region Farm Level

        CAFarmNewWebAppJob = 300,
        CAFarmAdminSearchJob = 301,
        CAFarmSearchDuplicateFileJob = 302,
        CAFarmSecuritySearchJob = 303,
        CAFarmCloneUserPermissionJob = 304,
        CAFarmImportConfigurationFileJob = 305,
        CAFarmDeadAccountCleanerJob = 306,

        #endregion

        #region Web App Level

        CAWebAppAdminSearchJob = 310,
        CAWebAppDeleteOrphanSiteJob = 311,
        CAWebAppSearchWebPartJob = 312,
        CAWebAppSearchDuplicateFileJob = 313,
        CAWebAppSecuritySearchJob = 314,
        CAWebAppCloneUserPermissionJob = 315,
        CAWebAppDeadAccountCleanerJob = 316,

        #endregion

        #region Site Collection Level

        CASiteCollectionMoveSiteCollectionJob = 320,
        CASiteCollectionAdminSearchJob = 321,
        CASiteCollectionCheckBrokenLinkJob = 322,
        CASiteCollectionSearchWebPartJob = 323,
        CASiteCollectionSearchDuplicateFileJob = 324,
        CASiteCollectionSecuritySearchJob = 325,
        CASiteCollectionCloneUserPermissionJob = 326,
        CASiteCollectionCloneSitePermissionJob = 327,
        CASiteCollectionStopInheritingPermissionsJob = 328,
        CASiteCollectionDeadAccountCleanerJob = 329,
        CASiteCollectionImportConfigurationFileJob = 330,
        CASiteCollectionDeleteSiteCollectionJob = 339,
        #endregion

        #region Site Level

        CASiteAdminSearchJob = 331,
        CASiteCheckBrokenLinkJob = 332,
        CASiteSearchWebPartJob = 333,
        CASiteSearchDuplicateFileJob = 334,
        CASiteSecuritySearchJob = 370,
        CASiteCloneUserPermissionsJob = 335,
        CASiteCloneSitePermissionJob = 336,
        CASiteStopInheritingPermissionsJob = 337,
        CASiteDeadAccountCleanerJob = 338,

        #endregion

        #region List Level

        CAListAdminSearchJob = 340,
        CAListSecuritySearchJob = 341,
        CAListCloneUserPermissionJob = 342,
        CAListCloneListLibraryPermissionJob = 343,
        CAListStopInheritingPermissionsJob = 344,
        CAListInheritPermissionsJob = 345,

        #endregion

        #region Folder Level

        CAFolderAdminSearchJob = 350,
        CAFolderSecuritySearchJob = 351,
        CAFolderCloneUserPermissionJob = 352,
        CAFolderCloneFolderPermissionJob = 353,
        CAFolderStopInheritingPermissionsJob = 354,
        CAFolderInheritPermissionsJob = 355,

        #endregion

        #region Item Level

        CAItemAdminSearchJob = 360,
        CAItemSecuritySearchJob = 361,
        CAItemChangeMetadataJob = 363,

        #endregion

        #region 以下类型Job不区分Level

        CADeleteTempPermissionJob = 371,
        CAOfflineExportReportJob = 372,
        CAProfileJob = 373,
        /// <summary>
        /// PE Job中只有Auditor类型的Rule
        /// </summary>
        CAOnlyAuditorRulePEJob = 374,
        CAOnlyScanRulePEJob = 375,
        CAEventReceiverPEJob = 376,

        #endregion

        #endregion
    }

    /// <summary>
    /// 此枚举中的值与CAJobTypes枚举中的定义保持完全一致
    /// </summary>
    public enum CASearchJobTypes : int
    {
        /// <summary>
        /// 原始JobType, 6.2之前只有以下两种JobType
        /// </summary>
        CASearchJob = 2,

        CAFarmAdminSearchJob = 301,
        CAFarmSecuritySearchJob = 303,
        CAWebAppAdminSearchJob = 310,
        CAWebAppSecuritySearchJob = 314,
        CASiteCollectionAdminSearchJob = 321,
        CASiteCollectionSecuritySearchJob = 325,
        CASiteAdminSearchJob = 331,
        CASiteSecuritySearchJob = 370,
        CAListAdminSearchJob = 340,
        CAListSecuritySearchJob = 341,
        CAFolderAdminSearchJob = 350,
        CAFolderSecuritySearchJob = 351,
        CAItemAdminSearchJob = 360,
        CAItemSecuritySearchJob = 361,
    }
}