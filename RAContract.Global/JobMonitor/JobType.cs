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
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.JobMonitor
{
    //TODO:[DataContract]
    public enum JobType
    {
        None = -1,
        TermSynchronization = 0,
        ItemsFilesDueDisposal = 1,
        BCSTermUsageReport = 2,
        SharePointGlobalSetting = 3,
        SharePointCustomSetting = 4,
        SharePointInheritSetting = 5,
        OrphanedTermReport = 6,
        SharePointScheduleSetting = 7,//SharePoint Setting Schedule job include new logic
        PhysicalTermSynchronization = 8,
        PhysicalFolderSynchronization = 9,
        TermDeletion = 10,
        UpdateLocation = 11,
        ImportPhysicalRecords = 12,
        CreateAndDestroyedFileReport = 13,
        AvailableSpaceReport = 14,
        ManualApproval = 15,
        ManualApprovalLocationTest = 16,
        ManualApprovalOrRejectJob = 17,
        ApplySharePointSettings = 18,// to do next Validate Job Type
        RetiredTermReport = 19,
        DisposalActivityManagement = 20,
        RestoreReport = 21,
        ArchiverScan = 24,
        MigrationArchiverRestore = 28,
        ArchiverBackup = 29,
        ExportToLocation = 30,
        ImportRecordsRelated = 32,
        TrimRecordsDeletion = 33,
        MigrationArchiverRetention = 35,
        //40 ~ 50 UniqueIDSetting
        UniqueIDSettingFullSchedule = 40,
        UniqueIDSettingIncrementalSchedule = 41,

        ReportAfterDataSync = 49,
        CollectionDataFull = 50,  //从Onpre Merge来的废弃jobType， 目前使用它重新计算Dashboard
        DataSynchronisation = 501,//TODO CollectionDataFull-->DataSynchronisation
        CollectionDataIncremental = 51,
        ManualApprovalTimer = 52,
        EnforceRetention = 53,
        OldEnforceRetention = 54,
        //60-70 Records Explorer Update.
        UpdateTerms = 60,
        DeclaredRecords = 61,
        UndeclaredRecords = 62,
        //VW Decalre/Undeclare Job
        ActionOnly = 80,
        ConnectorTimer = 81,
        ExportSiteMetrics = 82,

        ImportTermStructure = 100,
        ImportSPSetting = 101,
        ImportFSSetting = 102,
        ExportTermStructure = 103,
        ImportSCMapping = 104,
        ExportSCMapping = 105,
        ImportSCWhitelist = 106,
        ExportSCWhitelist = 107,
        ImportSCBlacklist = 108,
        ExportSCBlacklist = 109,
        All = 111,
        MigrationArchiverFileLevelRetention = 126,

        SyncNodesFromAOS = 200,

        SwitchSecurityProfile = 210,

        RecordsExplorerMove = 1005,
        GlobalSearchAction = 1006,
        Dashboard = 1007,
        ExportSearchResult = 1008,
        ExplorerOfflineSearch = 1020,
        ExportReportDetails = 1021,
        DownloadJobReports = 1022,
        ExportFSSetting = 1023,
        ExportSPSetting = 1024,
        // Internal COP variant of download job reports (hidden from end-user job monitor)
        DownloadJobReportsForCOP = 1025,
        ExportSPSOSetting = 1026,
        ApplyClassCode = 1027,
        DownloadRCCReport = 1028,
        SharePointSiteMetricsReport = 1029,

        #region EXO Job Type
        ExchangeArchiverScan = 124,
        ExchangeArchiverBackup = 125,
        EXOApplySetting = 2000,
        EXODataSynchronisation = 2001,
        EXOItemsFilesDueDisposalReport = 2100,
        EXOTermUsageReport = 2101,
        EXOOrphanedTermUsageReport = 2102,
        EXORetiredTermUsageReport = 2103,
        EXOCreateAndDestroyedFileReport = 2104,
        EXOApplySettingSchedule = 2105,


        EXOEnforceRetention = 2153,

        SPDataSynchronisationSchedule = 3000,
        EXODataSynchronisationSchedule = 3001,
        EXOArchiverRetention = 3002,
        #endregion
        #region New Physical Disposal
        PhysicalDisposal = 4000,
        PhysicalExplorerTimer = 4001,
        PhysicalItemsFilesDueDisposalReport = 4100,
        PhysicalTermUsageReport = 4101,
        PhysicalOrphanedTermUsageReport = 4102,
        PhysicalRetiredTermUsageReport = 4103,
        PhysicalCreateAndDestroyedFileReport = 4104,
        PhysicalExportBarcode = 4105,
        PhysicalSetPermission = 4106,
        PhysicalLoanBox = 4107,
        PhysicalReturnBox = 4108,
        PhysicalReturnHistoryExport = 4109,

        PhysicalLoanPick = 4110,
        PhysicalDestructionPick = 4111,
        PhysicalLoanPickExportJob = 4112,
        PhysicalDestructionPickExportJob = 4113,
        /// <summary>
        /// Physical导入Zip
        /// </summary>
        PhysicalBulkInsertExport = 4114,
        /// <summary>
        /// Physical导出Zip
        /// </summary>
        PhysicalBulkEditExport = 4115,
        ExportHoldRecords = 4116,
        ImportHoldRecords = 4117,
        ImportWorkspaceHold = 4118,

        PhysicalTemplateImport = 4200,
        PhysicalMoveDataJob = 4201,
        PhysicalMovePickExportJob = 4202,
        //New Physcial Disposal Report job.
        #endregion
        #region fs 5000 ~ 5499
        FSDataSynchronization = 5000,
        FSDataSynchronizationSchedule = 5001,
        FSDisposal = 5002,
        FSDisposalByClassCode = 5202,
        FSDisposalSchedule = 5003,
        FSItemsFilesDueDisposal = 5004,
        FSCreateAndDestroyedFileReport = 5006,
        FSBCSTermUsageReport = 5010,
        FSOrphanedTermReport = 5011,
        FSRetiredTermReport = 5012,
        FSDashBoard = 5100,
        FSFolderChangeTerm = 5200,
        FSFolderManageHold = 5201,
        FSMyHubDashboard = 5203,
        #endregion

        #region SharePoint OnPrem 5500 ~ 5999
        SPOnPremScanLocalNodes = 5500,
        SPOnPremDataSync = 5503,
        SPOnPremDataSyncSchedule = 5504,
        SPOnPremApplySetting = 5505,
        SPOnPremApplySettingSchedule = 5506,
        SPOnPremEnforceRuleAction = 5507,
        SPOnPremEnforceRuleActionSchedule = 5508,

        SPOnPremItemsFilesDueDisposal = 5510,
        SPOnPremCreateAndDestroyedFileReport = 5511,
        SPOnPremBCSTermUsageReport = 5512,
        SPOnPremOrphanedTermReport = 5513,
        SPOnPremRetiredTermReport = 5514,
        SPOnPremUniqueIDSettingFullSchedule = 5515,
        SPOnPremUniqueIDSettingIncrementalSchedule = 5516,
        SPOnPremTermSynchronization = 5600,
        SPOnPremTermSynchronizationSchedule = 5601,
        SPOnPremDashBoard = 5602,
        #endregion


        SyncSecurityContainer = 6000,
        OneDriveEnforceRetention = 6001,
        OneDriveDataSynchronisation = 6010,
        OneDriveDataSynchronisationSchedule = 6011,
        OneDriveTermUsageReport = 6100,
        OneDriveOrphanedTermUsageReport = 6101,
        OneDriveRetiredTermUsageReport = 6102,
        OneDriveItemsFilesDueDisposalReport = 6103,
        OneDriveCreateAndDestroyedFileReport = 6104,


        DisposalReport = 6200,
        TermUsageReport = 6201,
        CreateAndDestroyedReport = 6202,
        TenantUpgrade = 6105,
        ManualApprovalEmailSchedule = 6106,
        ManualHistoriesUpgrade = 6108,
        ManualExportHistoryDatasJob = 6109,
        ManualExportRecordsForReviewDatasJob = 6110,
        ManualImportUnderReviewDatasJob = 6111,
        OneDriverRestoreReport = 6113,
        GenerateRestoreReport = 6114,

        ManualFolderViewActions = 6300,

        #region Azure File Share

        AzureFileShareDataSynchronisation = 7000,
        AzureFileShareDataSynchronisationSchedule = 7001,

        #endregion

        #region 8000- 8019 ActionAuditReport
        SPOActionAuditReport = 8000,
        OneDriveActionAuditReport = 8019,
        #endregion

        #region 8020- 8099 Archiver
        RecordsDisposal = 8020, //run disposal job by records         
        OneDriveRecordsDisposal = 8021,
        EXORecordsDisposal = 8022,
        PhysicalRecordsDisposal = 8023,
        ArchiverRestore = 8024,
        RMArchiverBackup = 8025,
        ArchiverMoveIndex = 8026,
        ArchiverRetention = 8027,

        VeoMerge = 8028,
        ArchiverExport = 8029,
        MoveDataTier = 8030,
        SOPreScan = 8031,
        ArchiverOutPlaceRestore = 8032,
        DiscoverOptimization = 8033,
        RebuildStub = 8034,
        RebuildIndex = 8035,
        StubOopRestore = 8036,
        ApprovalProcessArchive = 8037,
        AdjustStorageSize = 8038,
        DeleteRestoredData = 8039,
        ExportIndex = 8040,
        ExportAdvanceSeachResult = 8041,
        ArchiverDeduplication = 8042,
        ArchiverDeduplicationReport = 8043,
        DeleteOrphanDatas = 8044,
        SpecifySitesArchiverBackup = 8045,
        DiscoveryAOSPOptimization = 8046,
        CleanUpDuplicateDatas = 8047,
        DeleteInvalidRecords = 8048,
        DiscoveryPlanProOptimization = 8049,
        RebuildEncryptKeyValue = 8055,
        RebuildSOJobReport = 8056,
        SimulateRestore = 8057,
        ExportRestoreCenterSeachResult = 8058,
        FSArchiverRestore = 8059,
        FSRetain = 8060,
        AOSPRestore = 8061,
        RebuildDeDupForWPPMigration = 8062,
        ArchiverRetentionSimulate = 8063,
        FSRetainSimulate = 8064,
        ArchiverRetentionSimulateMain = 8065, //Virtual
        ExportDecryptIndexDB = 8066,
        MultiSiteCollectionRestore = 8067,
        BaseArchiveJobIdMultiRestore = 8068,
        RMEndUserArchiverBackup = 8069,
        ArchiverFullMoveRetention = 8070,
        BuildRunningJobReport = 8080,
        PreviewRestore = 8081,
        #endregion

        #region 8100- 8110 Artificial Intelligence
        MachineLearningTraining = 8100,
        MachineLearningAnalyse = 8101,
        MachineLearningReviewReclassify = 8102,
        MachineLearningReviewApprove = 8103,
        MachineLearningExportReportJob = 8104,
        #endregion

        #region 8111 ~ 8112 Send email job

        SendEmailJob = 8111,

        #endregion

        #region 8111- 8200 Migration Archiver Job

        MigrationDisposalActivityManagement = 8120,
        MigrationArchiverScan = 8124,
        MigrationArchiverBackup = 8129,
        #endregion

        #region 9000~10000 Upgrade

        SharePointOnlineDeletionSyncUpgrade = 9000,
        CosmosDBDirtyDataDeleteUpgrade = 9001,
        ManualFileSystemUpgrade = 9002,

        CloudArchiverMigration = 9100,
        // Maintenance
        JobMonitorArchive = 9110,
        #endregion

        #region 10000 ~ 10100
        DiscoveryJob = 10001,
        DiscoveryOptimizationCalculate = 10002,
        DiscoveryProjection = 10003,
        DiscoveryReCalculate = 10004,
        DiscoveryJobV2 = 10005,
        DiscoveryJobV3 = 10006,
        DiscoveryProfileJob = 10007,
        DiscoveryPreScan = 10008,
        SFDiscoveryJob = 10009,
        DiscoveryJobV4 = 10011,
        DiscoveryGoogleJobV1 = 10012,
        DiscoveryGoogleProfileJob = 10013,
        DiscoveryAOSPJob = 10014,
        DiscoveryExportO365Profile = 10015,
        DiscoveryExportRowDataJob = 10016,
        DiscoveryAOSPOptimizationCalculate = 10017,
        DiscoveryFileSystemV1 = 10018,
        DiscoveryAnalysisFileSystemV1 = 10019,
        DiscoveryExportDuplicationReport = 10020,

        DiscoveryJobV5 = 10021,
        DiscoveryImportExcludeSCList = 10022,
        DiscoveryExportExcludeSCList = 10023,
        DiscoveryDalJob = 10024,
        DiscoveryPlanProScan = 10025,
        #endregion

        #region Archiver Full Text Index
        ArchiverFullTextIndex = 10010,
        #endregion

        #region 10100 ~ 10200 Box
        BoxDataSynchronisation = 10100,
        BoxDataSynchronisationSchedule = 10101,
        BoxItemsFilesDueDisposalReport = 10102,
        BoxCreateAndDestroyedFileReport = 10103,
        BoxBCSTermUsageReport = 10104,
        BoxOrphanedTermUsageReport = 10105,
        BoxRetiredTermUsageReport = 10106,
        BoxRecordsDisposal = 10107,
        #endregion


        #region 10200 ~ 10300 Google
        ImportGoogleTermStructure = 10200,
        GoogleLabelSyncToGoogle = 10201,
        GoogleDataSynchronization = 10202,
        GoogleApplySettings = 10203,
        GoogleRecordsDisposal = 10204,
        GoogleCreateAndDestroyedFileReport = 10205,
        GoogleItemsFilesDueDisposalReport = 10208,
        GoogleBCSTermUsageReport = 10209,
        GoogleOrphanedTermUsageReport = 10210,
        GoogleRetiredTermUsageReport = 10211,
        GoogleArchiverRetention = 10212,
        GoogleArchiverRestore = 10213,
        GoogleRestoreReport = 10214,
        GoogleArchiverBackup = 10215,
        #endregion

        #region 10300 ~ 10400 Teams
        ApplyTeamsSettings = 10300,
        TeamsRecordsDisposal = 10301,
        TeamsArchiverBackup = 10302,
        TeamsDataSynchronisation = 10303,
        TeamsDataSynchronisationSchedule = 10304,
        TeamsCreateAndDestroyedFileReport = 10305,
        TeamsItemsFilesDueDisposalReport = 10306,
        TeamsBCSTermUsageReport = 10307,
        TeamsOrphanedTermUsageReport = 10308,
        TeamsRetiredTermUsageReport = 10309,
        TeamsActionAuditReport = 10310,
        TeamsRestoreReport = 10311,
        TeamsArchiverRestore = 10312,
        TeamsScheduleSetting = 10313,
        TeamsEnforceRetention = 10314,
        TeamsUniqueIDSettingFullSchedule = 10315,
        TeamsUniqueIDSettingIncrementalSchedule = 10316,
        ExportTeamsSetting = 10317,
        ImportTeamsSetting = 10318,
        TeamsArchiverRetention = 10319,
        MailBoxArchiverRestore = 10321,
        TeamsPreScan = 10322,
        MailBoxBackup = 10323,
        TeamsChannelSettingConflictCheck = 10331,
        TeamsNodeSettingUpgrade = 10332,
        ConflictSettingDetailExport = 10333,
        TeamsDataUpgrade = 10334,
        ExportTeamsSOSetting = 10335,
        SpecifyTeamsArchiverBackup = 10336,
        TeamsOutPlaceRestore = 10337,
        #endregion

        //JobNotification
        JobNotification = 11000,
        ConvertStub = 11100,
        ArchiverByHSMXml = 11111,
        DeclaredRecordsMigration = 11200,
        StubDisposal = 11201,
        ArchiverToSpoRestore = 11202,
        DeleteArchivedSiteCollection = 11203,

        MultiGeoMainDCSyncCommonData = 11300,
        MultiGeoOtherDCSyncCommonData = 11301,

        APStorageCostEvaluation = 11400,

        #region Advanced Restore
        StubArchiverRestore = 11500,
        M365InPlaceArchiverRestore = 11501,
        #endregion

        #region Archived Sites Report Profiles
        ArchivedSiteReport = 11600,
        OneDriveArchivedSiteReport = 11601,
        TeamsArchivedSiteReport = 11602,
        GoogleArchivedSiteReport = 11603,
        #endregion

        MigrateDataCosmosDbForJPMC = 15001,

        DispatchedJob = 99998,

        DataIngestion = 99999,
    }
    //For time frame report
    [DataContract]
    public enum TimeRangeType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        CurrentWeek = 1,
        [EnumMember]
        CurrentMonth = 2,
        [EnumMember]
        Last3Month = 3,
        [EnumMember]
        Last6Month = 4,
        [EnumMember]
        Custom = 5,
    }
    public enum BCSSettingFailedType
    {
        None = 0,
        AddBCSColumnFailed = 1,
        AddBCSPropertyFailed = 2,
        AddPhysicalPropertyFailed = 4,
        DelBCSColumnFailed = 5,
    }

    [Flags]
    public enum ArchiverMigrationJobStatus
    {
        None = 0,
        PreparingDownloadReportBlob = 1,
    }

}
