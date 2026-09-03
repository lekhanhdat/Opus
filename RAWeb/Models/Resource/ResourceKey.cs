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

namespace AvePoint.RA.Web.Models
{
    public enum ResourceKeys
    {
        Home = 1,
        Index = 2,

        //Control Panel
        CP = 100,
        CP_Index = 101,
        CP_StorageSettings = 102,
        CP_AccountManagement = 103,
        CP_AgentManagement = 104,
        CP_GeneralSetting = 105,
        CP_ExportSettings = 106,
        CP_EmailTemplate = 107,
        CP_EditEmailTemplate = 108,
        CP_TimerJobSettings = 109,
        JM_DownloadSettings = 110,
        CP_Schedule_Settings_On_Prem = 111,
        CP_StubSettings = 112,
        CP_SuperUserConfiguration = 113,
        CP_EndUserRestoreSettings = 114,
        Reco_CP_Schedule_Settings = 115,
		CP_CreateEmailTemplate = 116,
		CP_JobNotificationSettings = 117,
        CP_ExportSettings_CompliantExports = 118,
        CP_Multi_GEOSettings = 119,

		CP_CSDApiKeyManagement = 199,

        //Job Monitor
        JM = 200,
        JM_Index,
        JM_Detail,
        JM_PlanDetails,
        JM_JobQueue,

        //RDM: Business Rule Management
        RDM = 300,
        RDM_RuleManagement,
        RDM_RuleManagementOld,
        RDM_CreateRule,
        RDM_EditRule,
        RDM_ManualApprovalReview,
        RDM_MAProcessesManagement,
        RDM_ViewWorkFlow,
        RDM_CreateWorkFlow,
        RDM_ManualApprovalReviews,
        RDM_ApprovalSetting,

        //BCM: 
        BCM = 400,
        BCM_RecordsExplorer,
        BCM_GlobalSearch,
        BCM_HybridSearch,
        BCM_ManageHold,
        BCM_TermManagement,
        BCM_ContentRepositoryManagement,
        BCM_ContentSourcesForSharePointOnline,
        BCM_ContentSourcesForExchangeOnline,
        BCM_ContentSourcesForPhysicalRecords,
        BCM_ContentSourcesForFileSystem,
        BCM_ContentSourcesForSharePointOnPremises,
        BCM_ContentSourcesForOneDriveforBusiness,
        BCM_ContentSourcesForAzureFiles,
        BCM_FSConnGroup,
        BCM_FSConnectionDetail,
        BCM_FSConnectionMonitor,
        BCM_ContentRepositoryManagement_UniqueId,
        BCM_ContentRepositoryManagement_Import,
        BCM_TermManagement_Admin,
        BCM_AzFileShareConfigureConnection,
        BCM_ContentRepositoryManagement_Classification,
        RECO_ContentSource_Tab,
        BCM_ContentSourcesForBox,
        BCM_BoxConfigureConnection,
        BCM_ContentSourcesForGoogle,
        BCM_GoogleConfigureConnection,
        BCM_ContentRepositoryManagement_Export,
        BCM_ContentSourcesForTeams,
        FileAnalysis_Discovery_ConfigurationFSConfigConnection,
        BCM_ContentSourcesForTeams_Switch,
        BCM_ContentRepositoryManagement_ExportSO,

        //Report Center: 
        RC = 500,
        RC_Dashboard,
        RC_DueDisposalReport_Management,
        RC_DueDisposalReport_Profile,
        RC_DueDisposalReport_ShowReport,
        RC_DueDisposalReport_ViewDetail,
        RC_TimeFrameFileReport_Management,
        RC_TimeFrameFileReport_Profile,
        RC_TimeFrameFileReport_ShowReport,
        RC_TimeFrameFileReport_ViewDetail,
        RC_TermUsageReport_Management,
        RC_TermUsageReport_Profile,
        RC_TermUsageReport_ShowReport,
        RC_TermUsageReport_ViewDetail,
        RC_AvailableSpaceReport_Management,
        RC_AvailableSpaceReport_Profile,
        RC_AvailableSpaceReport_ShowReport,
        RC_AvailableSpaceReport_ViewDetail,
        RC_RuleUsageReport_Management,
        RC_AuditReport_Management,
        RC_ActionReport_Management,
        RC_DueDisposalReport_Create,
        RC_DueDisposalReport_Edit,
        RC_CreateAndDestryoedReport_Create,
        RC_CreateAndDestryoedReport_Edit,
        RC_TermUsageReport_Create,
        RC_TermUsageReport_Edit,
        RC_ActionAuditReport_Management,
        RC_ActionAuditReport_Profile,
        RC_ActionAuditReport_ShowReport,
        RC_ActionAuditReport_ViewDetail,
        RC_ActionAuditReport_Create,
        RC_ActionAuditReport_Edit,
        RC_StorageOptimizationReport_Management,
        RC_StorageOptimizationReport_Profile,
        RC_StorageOptimizationReport_ShowReport,
        RC_RestoreReport_Management,
        RC_RestoreReport_Profile,
        RC_RestoreReport_ShowReport,
        RC_RestoreReport_ViewDetail,
        RC_RestoreReport_Create,
        RC_RestoreReport_Edit,
        RC_ExportSiteMetricsReport_Generate,

        //Physical
        PRM = 600,
        PRM_LocationManagement,
        PRM_PhysicalRecordsBulkImport,
        PRM_TemplateManagement,
        PRM_RecordsManagement,
        PRM_BarcodeManagement,
        PRM_BarcodeManagement_Create,
        PRM_BarcodeManagement_Edit,
        PRM_BarcodeManagement_EditDefault,
        PRM_BarcodeTemplate,
        PRM_RecordsExplorer,
        PRM_EditTemplate,
        PRM_CreateTemplate,
        PRM_MyRequest,
        PRM_ManageHold,
        PRM_GlobalSearch,
        PRM_ImportHPRM,
        PRM_CreateTemplateSuite,
        PRM_EditTemplateSuite,
        PRM_FolderTemplateManagement,
        PRM_RecordTemplateManagement,
        PRM_SetAccessControl,
        PRM_BoxCreationRequest,
        PRM_FolderCreationRequest,
        PRM_FolderLoanRequest,
        PRM_FolderLoanReturn,
        PRM_MoveRequest,

        Test = 700,
        Test_Timer,

        Source_SP = 800,
        Source_EXO,
        Source_Phy,
        Source_FS,
        Source_LSP,
        Source_OneDrive,
        Source_AzureFile,
        Explorer_SPFilter = 900,
        Explorer_FSFilter,
        Explorer_OneDriveFilter,
        Explorer_TeamsFilter,
        Source_Box,
        Source_Google,
        Source_Salesforce,
        Source_Teams,

        DC = 1000,
        DC_Download,

        RelatedRecords = 1100,

        //Customize Connector
        Connector = 1200,
        Connector_Index = 1201,
        Connector_CreateOrEdit = 1202,

        //My Task
        MT = 1300,
		MT_PickListForLoanRequests = 1301,
		MT_PickListForDestruction = 1302,
		MT_MachineLearningReview = 1303,
        MT_PickListForMovement = 1304,

		//Machine Learning
		ML = 1400,
		ML_MachineLearning = 1401,

        //Archiver
        Archiver_ContentSource_Tab = 1500,
        Archiver_RestoreCenter = 1501,
        Archiver_CP_Schedule_Settings = 1502,
        Archiver_Discovery_Optimization_RunJob = 1503,
        Archiver_Export_Index = 1504,
        Archiver_RestoreCenter_Search = 1505,
        Archiver_RestoreCenter_SearchAndExport = 1506,
        Archiver_RestoreCenter_FullControl = 1507,
        Archiver_RestoreCenter_Discovery = 1508,

        //Analysis
        FileAnalysis_Discovery = 1600,
        FileAnalysis_Discovery_Configuration = 1601,
        FileAnalysis_Discovery_RunJob = 1602,
        FileAnalysis_Discovery_Finish = 1603,
        FileAnalysis_InactiveOptimization = 1604,
        FileAnalysis_ROTOptimization = 1605,
        FileAnalysis_Progress = 1606,
        FileAnalysis_PlanProfile = 1607,
        FileAnalysis_PlanView = 1608
    }
}
