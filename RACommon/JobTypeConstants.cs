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
using AvePoint.RA.Contract.JobMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common
{
    public class JobTypeConstants
    {
        public const int SOArchivedSiteReportPageType = 8001;

        public static List<int> SPReportTypes = new List<int>()
        {
            (int)JobType.ItemsFilesDueDisposal,
            (int)JobType.CreateAndDestroyedFileReport,
            (int)JobType.BCSTermUsageReport,
            (int)JobType.OrphanedTermReport,
            (int)JobType.RetiredTermReport,
            (int)JobType.RestoreReport,
            (int)JobType.SPOActionAuditReport,
            (int)JobType.ArchivedSiteReport,
            (int)JobType.ImportSPSetting
        };

        public static List<int> SOArchivedSiteReportTypes = new List<int>()
        {
            (int)JobType.ArchivedSiteReport,
            (int)JobType.OneDriveArchivedSiteReport,
            (int)JobType.TeamsArchivedSiteReport,
            (int)JobType.GoogleArchivedSiteReport
        };

        public static List<int> EXOReportTypes = new List<int>()
        {
            (int)JobType.EXOItemsFilesDueDisposalReport,
            (int)JobType.EXOCreateAndDestroyedFileReport,
            (int)JobType.EXOTermUsageReport,
            (int)JobType.EXOOrphanedTermUsageReport,
            (int)JobType.EXORetiredTermUsageReport
        };

        public static List<int> PhysicalReportTypes = new List<int>()
        {
            (int)JobType.PhysicalItemsFilesDueDisposalReport,
            (int)JobType.PhysicalCreateAndDestroyedFileReport,
            (int)JobType.PhysicalTermUsageReport,
            (int)JobType.PhysicalOrphanedTermUsageReport,
            (int)JobType.PhysicalRetiredTermUsageReport,
            (int)JobType.AvailableSpaceReport
        };

        public static List<int> FSReportTypes = new List<int>()
        {
            (int)JobType.FSItemsFilesDueDisposal,
            (int)JobType.FSCreateAndDestroyedFileReport,
            (int)JobType.FSBCSTermUsageReport,
            (int)JobType.FSOrphanedTermReport,
            (int)JobType.FSRetiredTermReport
        };

        public static List<int> OneDriveReportTypes = new List<int>()
        {
            (int)JobType.OneDriveTermUsageReport,
            (int)JobType.OneDriveItemsFilesDueDisposalReport,
            (int)JobType.OneDriveOrphanedTermUsageReport,
            (int)JobType.OneDriveRetiredTermUsageReport,
            (int)JobType.OneDriveCreateAndDestroyedFileReport,
            (int)JobType.OneDriverRestoreReport,
            (int)JobType.OneDriveActionAuditReport
            ,(int)JobType.OneDriveArchivedSiteReport
        };

        public static List<int> SOSPReportTypes = new List<int>()
        {
            (int)JobType.RestoreReport,
            (int)JobType.SPOActionAuditReport,
            (int)JobType.ArchivedSiteReport,
        };
 

        public static List<int> SOOneDriveReportTypes = new List<int>()
        {
            (int)JobType.OneDriverRestoreReport,
            (int)JobType.OneDriveActionAuditReport,
            (int)JobType.OneDriveArchivedSiteReport,
        };

        public static List<int> SOTeamsReportTypes = new List<int>()
        {
            (int)JobType.TeamsRestoreReport,
            (int)JobType.TeamsActionAuditReport,
            (int)JobType.TeamsArchivedSiteReport,
        };

        public static List<int> SPOnPremReportTypes = new List<int>()
        {
            (int)JobType.SPOnPremItemsFilesDueDisposal,
            (int)JobType.SPOnPremCreateAndDestroyedFileReport,
            (int)JobType.SPOnPremBCSTermUsageReport,
            (int)JobType.SPOnPremOrphanedTermReport,
            (int)JobType.SPOnPremRetiredTermReport,
        };


        public static List<int> RestoreJobTypes = new List<int>()
        {
            (int)JobType.ArchiverRestore,
            (int)JobType.ArchiverOutPlaceRestore,
            (int)JobType.ExportRestoreCenterSeachResult,
            (int)JobType.FSArchiverRestore,
            (int)JobType.TeamsArchiverRestore,
            (int)JobType.TeamsOutPlaceRestore,
            (int)JobType.MailBoxArchiverRestore,
            (int)JobType.ArchiverToSpoRestore,
            (int)JobType.FSMyHubDashboard,
            (int)JobType.StubArchiverRestore,
            (int)JobType.M365InPlaceArchiverRestore,
        };

        public static List<int> FSJobTypes = new List<int>()
        {
            (int)JobType.FSDataSynchronization,
            (int)JobType.FSDisposal,
            //(int)JobType.FSDataSynchronizationSchedule,
            (int)JobType.FSDisposalSchedule,
            (int)JobType.FSDisposalByClassCode,
            (int)JobType.FSFolderChangeTerm,
            (int)JobType.FSFolderManageHold,
            (int)JobType.FSDashBoard,
            (int)JobType.FSArchiverRestore,
            (int)JobType.ApplyClassCode
        };

        public static List<int> GoogleAllRelatedJobTypes = new List<int>()
        {
            (int)JobType.GoogleApplySettings,
            (int)JobType.GoogleArchiverBackup,
            (int)JobType.GoogleArchiverRestore,
            (int)JobType.GoogleArchiverRetention,
            (int)JobType.GoogleBCSTermUsageReport,
            (int)JobType.GoogleCreateAndDestroyedFileReport,
            (int)JobType.GoogleDataSynchronization,
            (int)JobType.GoogleItemsFilesDueDisposalReport,
            (int)JobType.GoogleLabelSyncToGoogle,
            (int)JobType.GoogleOrphanedTermUsageReport,
            (int)JobType.GoogleRecordsDisposal,
            (int)JobType.GoogleRestoreReport,
            (int)JobType.GoogleArchivedSiteReport,
            (int)JobType.GoogleRetiredTermUsageReport,
            (int)JobType.ImportGoogleTermStructure,
            (int)JobType.GoogleArchiverBackup,
            (int)JobType.GoogleArchiverBackup,
            (int)JobType.GoogleArchiverBackup,
            (int)JobType.SyncSecurityContainer,
        };
        
        public static List<JobType> ArchiverIndexConflictJobTypes = new List<JobType>()
        {
            JobType.ArchiverRetention, 
            JobType.ArchiverMoveIndex, 
            //JobType.ArchiverRestore, 
            //JobType.ArchiverOutPlaceRestore, 
            JobType.RMArchiverBackup,
            JobType.CleanUpDuplicateDatas,
            JobType.RMEndUserArchiverBackup,
            JobType.SpecifySitesArchiverBackup, 
            JobType.RecordsDisposal, 
            JobType.OneDriveRecordsDisposal, 
            JobType.DiscoverOptimization,
            JobType.DiscoveryPlanProOptimization,
            JobType.ArchiverByHSMXml,
            JobType.DiscoveryAOSPOptimization, 
            JobType.ArchiverFullTextIndex, 
            //JobType.StubOopRestore, 
            JobType.DeleteRestoredData, 
            JobType.ArchiverDeduplication,
            JobType.DeleteOrphanDatas,
            JobType.ConvertStub,
            JobType.TeamsArchiverBackup,
            JobType.SpecifyTeamsArchiverBackup,
            JobType.TeamsRecordsDisposal,
            JobType.TeamsArchiverRetention,
            JobType.EXOArchiverRetention,
            JobType.GoogleArchiverRetention,
            JobType.DeleteArchivedSiteCollection,
        };
        public static List<JobType> ArchiverIndexWriteConflictJobTypes = new List<JobType>()
        {
            JobType.ArchiverRetention,
            JobType.ArchiverMoveIndex,
            JobType.CleanUpDuplicateDatas,
            JobType.RMArchiverBackup,
            JobType.RMEndUserArchiverBackup,
            JobType.SpecifySitesArchiverBackup,
            JobType.RecordsDisposal,
            JobType.OneDriveRecordsDisposal,
            JobType.DiscoverOptimization,
            JobType.DiscoveryPlanProOptimization,
            JobType.ArchiverByHSMXml,
            JobType.DiscoveryAOSPOptimization,
            JobType.DeleteRestoredData,
            JobType.ArchiverDeduplication,
            JobType.DeleteOrphanDatas,
            JobType.ConvertStub,
            JobType.TeamsArchiverBackup,
            JobType.SpecifyTeamsArchiverBackup,
            JobType.TeamsRecordsDisposal,
            JobType.TeamsArchiverRetention,
            JobType.EXOArchiverRetention,
            JobType.GoogleArchiverRetention,
            JobType.DeleteArchivedSiteCollection,
        };
        public static List<JobType> JobLevelConflictJobTypes = new List<JobType>()
        {
            JobType.ArchiverMoveIndex,
            JobType.ArchiverDeduplication
        };

        public static List<JobType> NoNeedToCheckVEORuleJobType = new List<JobType>()
        {
            JobType.CleanUpDuplicateDatas,
        };

        public static List<JobType> NeedToCheckVEORuleJobType = new List<JobType>() 
        { 
            JobType.RMArchiverBackup,
            JobType.RMEndUserArchiverBackup,
            JobType.SpecifySitesArchiverBackup,
            JobType.SpecifyTeamsArchiverBackup,
            JobType.RecordsDisposal,
            JobType.OneDriveRecordsDisposal,
            JobType.EXORecordsDisposal,
            JobType.TeamsRecordsDisposal,
            JobType.TeamsArchiverBackup
        };

        public static List<JobType> ArchiveSiteConflictType = new List<JobType>()
        {
            JobType.RMArchiverBackup,
            JobType.RMEndUserArchiverBackup,
            JobType.SpecifySitesArchiverBackup,
            JobType.RecordsDisposal, 
            JobType.OneDriveRecordsDisposal,
            JobType.DiscoverOptimization,
            JobType.DiscoveryPlanProOptimization,
            JobType.ArchiverByHSMXml,
            JobType.DiscoveryAOSPOptimization,
            JobType.ArchiverRetention,
            JobType.DeleteRestoredData,
            JobType.DeleteOrphanDatas,
            JobType.ConvertStub,
            JobType.TeamsArchiverBackup,
            JobType.SpecifyTeamsArchiverBackup,
            JobType.TeamsRecordsDisposal,
            JobType.DeleteArchivedSiteCollection,
        };

        public static List<JobType> NeedCheckInSiteCollectinMethod = new List<JobType>()
        {
            JobType.RMArchiverBackup,
            JobType.RMEndUserArchiverBackup,
            JobType.SpecifySitesArchiverBackup,
            JobType.RecordsDisposal,
            JobType.OneDriveRecordsDisposal,
            JobType.DiscoverOptimization,
            JobType.DiscoveryPlanProOptimization,
            JobType.ArchiverByHSMXml,
            JobType.DiscoveryAOSPOptimization,
            JobType.ArchiverRetention,
            JobType.DeleteRestoredData,
            JobType.DeleteOrphanDatas,
            JobType.ConvertStub,
            JobType.DeleteArchivedSiteCollection,
        };

        public static List<JobType> NeedCheckInTeamMethod = new List<JobType>()
        {
            JobType.ConvertStub,
            JobType.TeamsArchiverBackup,
            JobType.SpecifyTeamsArchiverBackup,
            JobType.TeamsRecordsDisposal,
        };

        public static List<JobType> ArchiveTeamsConflictType = new List<JobType>()
        {
            JobType.TeamsArchiverBackup,
            JobType.TeamsRecordsDisposal,
            JobType.SpecifyTeamsArchiverBackup,
            JobType.TeamsArchiverRetention,
            JobType.ConvertStub,
            JobType.DiscoverOptimization,
            JobType.DiscoveryPlanProOptimization,
            JobType.ArchiverByHSMXml,
            JobType.ArchiverRetention,
            JobType.SpecifySitesArchiverBackup,
            JobType.RMEndUserArchiverBackup,
            JobType.DiscoveryAOSPOptimization,
            JobType.DeleteRestoredData,
            JobType.RMArchiverBackup
        };
        public static List<JobType> FSArchiveConflictType = new List<JobType>()
        {
            JobType.FSDisposal,
            JobType.FSDisposalSchedule,
            JobType.FSDisposalByClassCode,
            JobType.FSRetain,
            JobType.ArchiverMoveIndex,
            JobType.FSArchiverRestore
        };
        public static List<JobType> FSDisposalConflictType = new List<JobType>()
        {
            JobType.FSRetain,
            JobType.ArchiverMoveIndex,
        };

        public static List<JobType> SOBackupjobTypes = new List<JobType>()
        {
            JobType.RMArchiverBackup,
            JobType.RMEndUserArchiverBackup,
            JobType.SpecifySitesArchiverBackup,
            JobType.RecordsDisposal,
            JobType.OneDriveRecordsDisposal,
            JobType.DiscoverOptimization,
            JobType.DiscoveryPlanProOptimization,
            JobType.ArchiverByHSMXml,
            JobType.TeamsArchiverBackup,
            JobType.CleanUpDuplicateDatas,
            JobType.SpecifyTeamsArchiverBackup,
            JobType.TeamsRecordsDisposal,
            JobType.DiscoveryAOSPOptimization,
            JobType.MailBoxBackup
        };
        public static List<int> ArchiverJobTypes = new List<int>()
        {
            (int)JobType.RMArchiverBackup,
            (int)JobType.RMEndUserArchiverBackup,
            (int)JobType.SpecifySitesArchiverBackup,
            (int)JobType.SpecifyTeamsArchiverBackup,
            (int)JobType.ArchiverRestore,
            (int)JobType.ArchiverToSpoRestore,
            (int)JobType.ArchiverMoveIndex,
            (int)JobType.ArchiverRetention,
            (int)JobType.ArchiverRetentionSimulate,
            (int)JobType.VeoMerge,
            (int)JobType.ArchiverOutPlaceRestore,
            (int)JobType.SOPreScan,
            (int)JobType.MigrationDisposalActivityManagement,
            (int)JobType.CloudArchiverMigration,
            (int)JobType.MigrationArchiverRetention,
            (int)JobType.MigrationArchiverRestore,
            (int)JobType.DiscoverOptimization,
            (int)JobType.DiscoveryPlanProOptimization,
            (int)JobType.ArchiverByHSMXml,
            (int)JobType.CleanUpDuplicateDatas,
            (int)JobType.DiscoveryAOSPOptimization,
            (int)JobType.StubOopRestore,
            (int)JobType.AOSPRestore,
            (int)JobType.ExportAdvanceSeachResult,
            (int)JobType.DiscoveryPreScan,
            (int)JobType.DiscoveryPlanProScan,
            (int)JobType.ArchiverDeduplication,
            (int)JobType.DeleteOrphanDatas,
            (int)JobType.MigrationArchiverFileLevelRetention,
            (int)JobType.ExportRestoreCenterSeachResult,
            (int)JobType.TeamsArchiverRestore,
            (int)JobType.TeamsOutPlaceRestore,
            (int)JobType.MailBoxArchiverRestore,
            (int)JobType.TeamsArchiverRetention,
            (int)JobType.EXOArchiverRetention,
            (int)JobType.TeamsArchiverBackup,
            (int)JobType.TeamsPreScan,
            (int)JobType.GoogleArchiverRestore,
            (int)JobType.TeamsNodeSettingUpgrade,
            (int)JobType.TeamsDataUpgrade,
            (int)JobType.ConflictSettingDetailExport,
            (int)JobType.GoogleArchiverRetention,
            (int)JobType.DeclaredRecordsMigration,
            (int)JobType.StubDisposal,
            (int)JobType.DeleteArchivedSiteCollection,
            (int)JobType.StubArchiverRestore,
            (int)JobType.M365InPlaceArchiverRestore,
        };

        public static List<int> ArchiverSpecialJobTypes = new List<int>()
        {
            (int)JobType.SyncNodesFromAOS,
            (int)JobType.Dashboard,
            (int)JobType.ArchiverExport,
            (int)JobType.ArchiverDeduplicationReport,
            (int)JobType.ExportToLocation,
            (int)JobType.DownloadJobReports,
            (int)JobType.ArchiverFullTextIndex,
            (int)JobType.DiscoveryJobV2,
            (int)JobType.DiscoveryJobV3,
            (int)JobType.DiscoveryJobV4,
            (int)JobType.DiscoveryJobV5,
            (int)JobType.DiscoveryAOSPJob,
            (int)JobType.SFDiscoveryJob,
            (int)JobType.DiscoveryProfileJob,
            (int)JobType.DeleteRestoredData,
            (int)JobType.DiscoveryExportRowDataJob,
            (int)JobType.DiscoveryExportO365Profile,
            (int)JobType.DiscoveryExportDuplicationReport,
            (int)JobType.DiscoveryImportExcludeSCList,
            (int)JobType.DiscoveryExportExcludeSCList,
        };

        public static List<int> PhysicalJobTypes = new List<int>()
        {
            (int)JobType.PhysicalDisposal,
            (int)JobType.PhysicalFolderSynchronization,
            (int)JobType.PhysicalSetPermission,
            (int)JobType.PhysicalExportBarcode,
            (int)JobType.PhysicalLoanBox,
            (int)JobType.PhysicalReturnBox,
            (int)JobType.PhysicalReturnBox,
            (int)JobType.PhysicalLoanPick,
            (int)JobType.PhysicalDestructionPick,
            (int)JobType.PhysicalReturnHistoryExport,
            (int)JobType.PhysicalMovePickExportJob,
            (int)JobType.PhysicalMoveDataJob,
        };


        public static List<int> SPOnPremJobTypes = new List<int>()
        {
            (int)JobType.SPOnPremApplySetting,
            (int)JobType.SPOnPremDataSync,
            (int)JobType.SPOnPremEnforceRuleAction,
            (int)JobType.SPOnPremEnforceRuleActionSchedule,
            (int)JobType.SPOnPremDashBoard,
        };

        public static List<int> AzureFileShareJobTypes = new List<int>
        {
            (int)JobType.AzureFileShareDataSynchronisation,
            (int)JobType.AzureFileShareDataSynchronisationSchedule
        };

        public static List<int> BoxJobTypes = new List<int>
        {
            (int)JobType.BoxDataSynchronisation,
            (int)JobType.BoxDataSynchronisationSchedule,
            (int)JobType.BoxRecordsDisposal,
        };
        public static List<int> GoogleJobTypes = new List<int>
        {
            (int)JobType.GoogleDataSynchronization,
            (int)JobType.GoogleApplySettings,
            (int)JobType.GoogleRecordsDisposal,
            (int)JobType.GoogleArchiverRestore,
        };
        //需要验证container id的job类型
        public static List<int> WithContainerIdJobTypes = new List<int>()
        {
            (int)JobType.ApplySharePointSettings,
            (int)JobType.EXOApplySetting,
            (int)JobType.DataSynchronisation,
            (int)JobType.DisposalActivityManagement,
            (int)JobType.EXODataSynchronisation,
            (int)JobType.OneDriveDataSynchronisation,
            (int)JobType.RecordsDisposal,
            (int)JobType.EXORecordsDisposal,
            (int)JobType.OneDriveRecordsDisposal,
            (int)JobType.RMArchiverBackup,
            (int)JobType.RMEndUserArchiverBackup,
            (int)JobType.SpecifySitesArchiverBackup,
            (int)JobType.SpecifyTeamsArchiverBackup,
            (int)JobType.ArchiverRestore,
            (int)JobType.ArchiverToSpoRestore,
            (int)JobType.ArchiverOutPlaceRestore,
            (int)JobType.SOPreScan,
            (int)JobType.DiscoverOptimization,
            (int)JobType.ArchiverByHSMXml,
            (int)JobType.DiscoveryAOSPOptimization,
            (int)JobType.DiscoveryPreScan,
            (int)JobType.StubOopRestore,
            (int)JobType.AOSPRestore,
            (int)JobType.ApprovalProcessArchive,
            (int)JobType.DeleteInvalidRecords,
            (int)JobType.TeamsArchiverBackup,
            (int)JobType.TeamsRecordsDisposal,
            (int)JobType.TeamsArchiverRestore,
            (int)JobType.TeamsOutPlaceRestore,
            (int)JobType.MailBoxArchiverRestore,
            (int)JobType.TeamsPreScan,
            (int)JobType.PhysicalRecordsDisposal,
            (int)JobType.StubArchiverRestore,
            (int)JobType.M365InPlaceArchiverRestore,
        };

        public static List<int> SpecialJobTypes = new List<int>()
        {
            (int)JobType.ExportToLocation,
            (int)JobType.GlobalSearchAction,
            (int)JobType.ExportSearchResult,
            (int)JobType.ExplorerOfflineSearch,
            (int)JobType.ManualApprovalOrRejectJob,
            (int)JobType.ManualExportRecordsForReviewDatasJob,
            (int)JobType.ManualImportUnderReviewDatasJob,
            (int)JobType.ManualFolderViewActions,
            (int)JobType.DeleteInvalidRecords,
            (int)JobType.ExportReportDetails,
            (int)JobType.MachineLearningReviewApprove,
            (int)JobType.MachineLearningReviewReclassify,
            (int)JobType.MachineLearningExportReportJob,
            (int)JobType.ManualExportHistoryDatasJob,
            (int)JobType.PhysicalLoanPickExportJob,
            (int)JobType.PhysicalDestructionPickExportJob,
            (int)JobType.PhysicalMovePickExportJob,
            (int)JobType.VeoMerge,
            (int)JobType.ImportPhysicalRecords,
            (int)JobType.PhysicalBulkInsertExport,
            (int)JobType.PhysicalBulkEditExport,
            (int)JobType.DownloadJobReports,
            (int)JobType.ExportFSSetting,
            (int)JobType.ExportSPSetting,
            (int)JobType.ImportSCMapping,
            (int)JobType.ExportSCMapping,
            (int)JobType.ImportSCWhitelist,
            (int)JobType.ExportSCWhitelist,
            (int)JobType.ImportSCBlacklist,
            (int)JobType.ExportSCBlacklist,
            (int)JobType.ConvertStub,
            (int)JobType.ExportTeamsSOSetting,
            (int)JobType.ExportSPSOSetting,
            (int)JobType.ExportRestoreCenterSeachResult,
            (int)JobType.DownloadRCCReport,
            (int)JobType.ExportHoldRecords,
            (int)JobType.ImportHoldRecords,
            (int)JobType.ImportWorkspaceHold
        };

        public static List<int> TermUsageJobTypes = new List<int>()
        {
            (int)JobType.BCSTermUsageReport,
            (int)JobType.OrphanedTermReport,
            (int)JobType.RetiredTermReport,
            (int)JobType.EXOTermUsageReport,
            (int)JobType.EXOOrphanedTermUsageReport,
            (int)JobType.EXORetiredTermUsageReport,
            (int)JobType.PhysicalTermUsageReport,
            (int)JobType.PhysicalOrphanedTermUsageReport,
            (int)JobType.PhysicalRetiredTermUsageReport,
            (int)JobType.FSBCSTermUsageReport,
            (int)JobType.FSOrphanedTermReport,
            (int)JobType.FSRetiredTermReport,
            (int)JobType.OneDriveTermUsageReport,
            (int)JobType.OneDriveOrphanedTermUsageReport,
            (int)JobType.OneDriveRetiredTermUsageReport,
            (int)JobType.SPOnPremBCSTermUsageReport,
            (int)JobType.SPOnPremOrphanedTermReport,
            (int)JobType.SPOnPremRetiredTermReport,
            (int)JobType.BoxBCSTermUsageReport,
            (int)JobType.BoxOrphanedTermUsageReport,
            (int)JobType.BoxRetiredTermUsageReport,
            (int)JobType.GoogleBCSTermUsageReport,
            (int)JobType.GoogleOrphanedTermUsageReport,
            (int)JobType.GoogleRetiredTermUsageReport,
             (int)JobType.TeamsBCSTermUsageReport,
            (int)JobType.TeamsOrphanedTermUsageReport,
            (int)JobType.TeamsRetiredTermUsageReport,
        };


        public static List<int> ContentDueReportJobTypes = new List<int>()
        {
            (int)JobType.EXOItemsFilesDueDisposalReport,
            (int)JobType.FSItemsFilesDueDisposal,
            (int)JobType.ItemsFilesDueDisposal,
            (int)JobType.OneDriveItemsFilesDueDisposalReport,
            (int)JobType.PhysicalItemsFilesDueDisposalReport,
            (int)JobType.SPOnPremItemsFilesDueDisposal,
            (int)JobType.BoxItemsFilesDueDisposalReport,
            (int)JobType.GoogleItemsFilesDueDisposalReport,
            (int)JobType.TeamsItemsFilesDueDisposalReport,
        };

        public static List<int> CreationReportJobTypes = new List<int>()
        {
            (int)JobType.CreateAndDestroyedFileReport,
            (int)JobType.EXOCreateAndDestroyedFileReport,
            (int)JobType.FSCreateAndDestroyedFileReport,
            (int)JobType.OneDriveCreateAndDestroyedFileReport,
            (int)JobType.PhysicalCreateAndDestroyedFileReport,
            (int)JobType.SPOnPremCreateAndDestroyedFileReport,
            (int)JobType.BoxCreateAndDestroyedFileReport,
            (int)JobType.GoogleCreateAndDestroyedFileReport,
            (int)JobType.TeamsCreateAndDestroyedFileReport,        
        };

        public static List<int> AvaliableSpaceReportJobTypes = new List<int>()
        {
            (int)JobType.AvailableSpaceReport
        };

        public static List<int> ActionAuditReportJobTypes = new List<int>()
        {
            (int)JobType.SPOActionAuditReport,
            (int)JobType.OneDriveActionAuditReport,
            (int)JobType.TeamsActionAuditReport,
        };

        public static List<int> ReviewersJobTypes = new List<int>(){
            (int)JobType.MachineLearningReviewReclassify,
            (int)JobType.MachineLearningReviewApprove,
            (int)JobType.MachineLearningExportReportJob,
            (int)JobType.ManualImportUnderReviewDatasJob,
            (int)JobType.ManualFolderViewActions,
            (int)JobType.ManualExportRecordsForReviewDatasJob,
            (int)JobType.ManualApprovalOrRejectJob,
            (int)JobType.ManualExportHistoryDatasJob,
            (int)JobType.DeleteInvalidRecords,
            (int)JobType.DownloadJobReports,
            (int)JobType.GlobalSearchAction,
            (int)JobType.FSMyHubDashboard,
            (int)JobType.DownloadRCCReport,
            (int)JobType.ApplyClassCode
        };

        public static List<int> MigrationDisposalJobTypes = new() {
            (int)JobType.ArchiverScan,
            (int)JobType.ArchiverBackup,
            (int)JobType.ExchangeArchiverScan,
            (int)JobType.ExchangeArchiverBackup,
            (int)JobType.PhysicalDisposal
        };

        public static List<int> RestoreReportJobTypes = new()
        {
            (int)JobType.RestoreReport,
            (int)JobType.OneDriverRestoreReport,
            (int)JobType.TeamsRestoreReport
        };

        public static List<int> ArchivedSiteReportJobTypes = new()
        {
            (int)JobType.ArchivedSiteReport,
            (int)JobType.OneDriveArchivedSiteReport,
            (int)JobType.TeamsArchivedSiteReport,
            (int)JobType.GoogleArchivedSiteReport
        };

        public static List<int> BoxReportTypes = new List<int>()
        {
            (int)JobType.BoxItemsFilesDueDisposalReport,
            (int)JobType.BoxCreateAndDestroyedFileReport,
            (int)JobType.BoxBCSTermUsageReport,
            (int)JobType.BoxOrphanedTermUsageReport,
            (int)JobType.BoxRetiredTermUsageReport,
        };
        public static List<int> GoogleReportTypes = new List<int>()
        {
            (int)JobType.GoogleCreateAndDestroyedFileReport,
            (int)JobType.GoogleItemsFilesDueDisposalReport,
            (int)JobType.GoogleBCSTermUsageReport,
            (int)JobType.GoogleOrphanedTermUsageReport,
            (int)JobType.GoogleRetiredTermUsageReport,
            (int)JobType.GoogleRestoreReport,
            (int)JobType.GoogleArchivedSiteReport,
        };

        public static List<int> TeamsReportTypes = new List<int>()
        {
            (int)JobType.TeamsCreateAndDestroyedFileReport,
            (int)JobType.TeamsItemsFilesDueDisposalReport,
            (int)JobType.TeamsBCSTermUsageReport,
            (int)JobType.TeamsOrphanedTermUsageReport,
            (int)JobType.TeamsRetiredTermUsageReport,
            (int)JobType.TeamsRestoreReport,
            (int)JobType.ExportTeamsSetting,
            (int)JobType.ImportTeamsSetting
        };

        public static List<int> RestoreOnlyPermissionJobTypes = new()
        {
            (int)JobType.ArchiverRestore,
            (int)JobType.ArchiverOutPlaceRestore,
            (int)JobType.ExportRestoreCenterSeachResult,
            (int)JobType.DownloadJobReports,
            (int)JobType.FSArchiverRestore,
            (int)JobType.TeamsArchiverRestore,
            (int)JobType.MailBoxArchiverRestore,
            (int)JobType.TeamsOutPlaceRestore,
            (int)JobType.ArchiverToSpoRestore,
            (int)JobType.StubArchiverRestore,
            (int)JobType.M365InPlaceArchiverRestore,
        };

        #region Teams job types
        public static List<int> TeamsJobTypes = new List<int>
        {
            (int)JobType.TeamsDataSynchronisation,
            (int)JobType.TeamsDataSynchronisationSchedule,
            (int)JobType.ApplyTeamsSettings,
            (int)JobType.TeamsRecordsDisposal,
            (int)JobType.SpecifyTeamsArchiverBackup,
            (int)JobType.TeamsArchiverBackup,
        };

        public static List<int> TeamsReportJobTypes = new List<int>()
        {
        };
        #endregion
    }
}
