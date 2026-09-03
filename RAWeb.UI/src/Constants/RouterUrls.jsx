import { DiscoveryDataSource } from "../Components/DiscoveryAndAnalysis/Discovery/AnalysisConfigurator/Constants";

const RouterUrl_Root = "/Root";

const RouterUrls = {
    Root: RouterUrl_Root,
    Home: RouterUrl_Root + "/Home",
    TestTree: RouterUrl_Root + "/Home/TestTree",
    //Control Panel
    CP: RouterUrl_Root + "/CP",
    CP_Index: RouterUrl_Root + "/CP/Index",
    CP_DocAveConnection: RouterUrl_Root + "/CP/DocAveConnection",
    CP_StorageSettings: RouterUrl_Root + "/CP/StorageSettings",
    CP_Authentication: RouterUrl_Root + "/CP/Authentication",
    CP_AccountManagement: RouterUrl_Root + "/CP/AccountManagement",
    CP_AgentManagement: RouterUrl_Root + "/CP/AgentManagement",
    CP_CSDApiKeyManagement: RouterUrl_Root + "/CP/CSDApiKeyManagement",
    CP_GeneralSetting: RouterUrl_Root + "/CP/GeneralSetting",
    CP_ExportSettings: RouterUrl_Root + "/CP/ExportSettings",
    CP_ExportSettings_CompliantExports: RouterUrl_Root + "/CP/ExportSettings/CompliantExports",
    CP_DashboardSettings: RouterUrl_Root + "/CP/DashboardSettings",
    CP_EmailTemplate: RouterUrl_Root + "/CP/EmailTemplate",
    CP_EditEmailTemplate: RouterUrl_Root + "/CP/EditEmailTemplate",
    CP_CreateEmailTemplate: RouterUrl_Root + "/CP/CreateEmailTemplate",
    CP_StubSettings: RouterUrl_Root + "/CP/StubSettings",
    CP_EndUserRestore: RouterUrl_Root + "/CP/EndUserRestoreSetting",
    CP_JobNotification: RouterUrl_Root + "/CP/JobNotificationSetting",
    CP_MultiGeo: RouterUrl_Root + "/CP/Multi/GEOSettings",
    // CP_MultiGeo: RouterUrl_Root + "/CP/JobNotificationSetting",
    // CP_Connector: RouterUrl_Root + "/CP/Connector",
    // CP_Connector_CreateOrEdit: RouterUrl_Root + "/CP/Connector/CreateOrEdit",
    //Job Monitor
    JM: RouterUrl_Root + "/JM",
    JM_Index: RouterUrl_Root + "/JM/Index",
    JM_Detail: RouterUrl_Root + "/JM/Detail",
    JM_PlanDetail: RouterUrl_Root + "/JM/PlanDetails",

    CP_TimerJobSettings: RouterUrl_Root + "/CP/TimerJobSettings",
    //RDM: Business Rule Management
    RDM: RouterUrl_Root + '/RDM',
    RDM_RuleManagement: RouterUrl_Root + '/RDM/RuleManagement',
    RDM_CreateRule: RouterUrl_Root + '/RDM/CreateRule',
    RDM_EditRule: RouterUrl_Root + '/RDM/EditRule',
    RDM_ManualApprovalReview:RouterUrl_Root + '/RDM/ManualApprovalReview',
    RDM_ManualApprovalReview_Old:RouterUrl_Root + '/RDM/ManualApprovalReviews',
    RDM_WorkFlowManagement: RouterUrl_Root + '/RDM/MAProcessesManagement',
    RDM_ViewWorkFlow:RouterUrl_Root + '/RDM/ViewWorkFlow',
    RDM_CreateWorkFlow:RouterUrl_Root + '/RDM/CreateWorkFlow',
    //BCM: 
    BCM: RouterUrl_Root + '/BCM',
    BCM_RecordsExplorer: RouterUrl_Root + '/BCM/RecordsExplorer',
    BCM_ManageHold: RouterUrl_Root + '/BCM/ManageHold',
    BCM_TermManagement: RouterUrl_Root + '/BCM/TermManagement',
    BCM_ContentRepositoryManagement_Old: '/BCM/ContentRepositoryManagement',
    BCM_ContentRepositoryManagement: RouterUrl_Root + '/BCM/ContentRepositoryManagement',
    BCM_ContentRepositoryManagement_SPO: RouterUrl_Root + '/BCM/ContentSourcesForSharePointOnline',
    BCM_ContentRepositoryManagement_EXO: RouterUrl_Root + '/BCM/ContentSourcesForExchangeOnline',
    BCM_ContentRepositoryManagement_Phy: RouterUrl_Root + '/BCM/ContentSourcesForPhysicalRecords',
    BCM_ContentRepositoryManagement_FS: RouterUrl_Root + '/BCM/ContentSourcesForFileSystem',
    BCM_ContentRepositoryManagement_LSP: RouterUrl_Root + '/BCM/ContentSourcesForSharePointOnPremises',
    BCM_ContentRepositoryManagement_OD: RouterUrl_Root + '/BCM/ContentSourcesForOneDriveforBusiness',
    BCM_ContentRepositoryManagement_AF: RouterUrl_Root + '/BCM/ContentSourcesForAzureFiles',
    BCM_ContentRepositoryManagement_Box: RouterUrl_Root + '/BCM/ContentSourcesForBox',
    BCM_ContentRepositoryManagement_GoogleDrive: RouterUrl_Root + '/BCM/ContentSourcesForGoogle',
    BCM_ContentRepositoryManagement_Teams: RouterUrl_Root + '/BCM/ContentSourcesForTeams',
    BCM_ContentRepositoryManagement_Teams_Switch: RouterUrl_Root + '/BCM/ContentSourcesForTeams/Switch',
    //RC: 
    RC: RouterUrl_Root + '/RC',
    RC_DueDisposalReportManagement: RouterUrl_Root + '/RC/DueDisposalReport/Management',
    RC_DueDisposalReportProfile: RouterUrl_Root + '/RC/DueDisposalReport/Profile',
    RC_DueDisposalReportCreate: RouterUrl_Root + '/RC/DueDisposalReport/Create',
    RC_DueDisposalReportEdit: RouterUrl_Root + '/RC/DueDisposalReport/Edit',
    RC_CreateAndDestryoedReportCreate: RouterUrl_Root + '/RC/CreateAndDestryoedReport/Create',
    RC_CreateAndDestryoedReportEdit: RouterUrl_Root + '/RC/CreateAndDestryoedReport/Edit',
    RC_TermUsageReportCreate: RouterUrl_Root + '/RC/TermUsageReport/Create',
    RC_TermUsageReportEdit: RouterUrl_Root + '/RC/TermUsageReport/Edit',
    RC_DueDisposalShowReport: RouterUrl_Root + '/RC/DueDisposalReport/ShowReport',
    RC_DueDisposalReportViewDetail: RouterUrl_Root + '/RC/DueDisposalReport/ViewDetail',
    RC_CreationAndDestructionReport: RouterUrl_Root + '/RC/TimeFrameFileReport/Management',
    RC_CreationAndDestructionProfile: RouterUrl_Root + '/RC/TimeFrameFileReport/Profile',
    RC_CreationAndDestructionShowReport: RouterUrl_Root + '/RC/TimeFrameFileReport/ShowReport',
    RC_CreationAndDestructionViewDetail: RouterUrl_Root + '/RC/TimeFrameFileReport/ViewDetail',
    RC_TermUsageReportManagement: RouterUrl_Root + '/RC/TermUsageReport/Management',
    RC_TermUsageReportProfile: RouterUrl_Root + '/RC/TermUsageReport/Profile',
    RC_TermUsageShowReport: RouterUrl_Root + '/RC/TermUsageReport/ShowReport',
    RC_TermUsageReportViewDetail: RouterUrl_Root + '/RC/TermUsageReport/ViewDetail',
    RC_AvailableSpaceReportManagement: RouterUrl_Root + '/RC/AvailableSpaceReport/Management',
    RC_AvailableSpaceReportProfile: RouterUrl_Root+'/RC/AvailableSpaceReport/Profile',
    RC_AvailableSpaceReportShowReport: RouterUrl_Root+'/RC/AvailableSpaceReport/ShowReport',
    RC_AvailableSpaceReportDetail: RouterUrl_Root + '/RC/AvailableSpaceReport/ViewDetail',
    RC_RuleUsageReportManagement: RouterUrl_Root + '/RC/RuleUsageReport/Management',
    RC_AuditReportManagement: RouterUrl_Root + '/RC/AuditReport/Management',
    RC_ActionAuditReportManagement: RouterUrl_Root + '/RC/ActionAuditReport/Management',
    RC_ActionAuditReportProfile: RouterUrl_Root + '/RC/ActionAuditReport/Profile',
    RC_ActionAuditReportShowReport: RouterUrl_Root+'/RC/ActionAuditReport/ShowReport',
    RC_ActionAuditReportDetail: RouterUrl_Root + '/RC/ActionAuditReport/ViewDetail',
    RC_StorageOptimizationReportManagement: RouterUrl_Root + '/RC/StorageOptimizationReport/Management',
    RC_StorageOptimizationReportProfile: RouterUrl_Root + '/RC/StorageOptimizationReport/Profile',
    RC_StorageOptimizationReportShowReport: RouterUrl_Root + '/RC/StorageOptimizationReport/ShowReport',
    RC_RestoreReportManagement: RouterUrl_Root + '/RC/RestoreReport/Management',
    RC_RestoreReportProfile: RouterUrl_Root + '/RC/RestoreReport/Profile',
    RC_RestoreReportCreate: RouterUrl_Root + '/RC/RestoreReport/Create',
    RC_RestoreReportEdit: RouterUrl_Root + '/RC/RestoreReport/Edit',
    RC_RestoreShowReport: RouterUrl_Root + '/RC/RestoreReport/ShowReport',
    RC_RestoreReportViewDetail: RouterUrl_Root + '/RC/RestoreReport/ViewDetail',

    //PRM:
    PRM: RouterUrl_Root + "/PRM",
    PRM_LocationManagement: RouterUrl_Root + "/PRM/LocationManagement",
    PRM_LocationSynchronisation:
        RouterUrl_Root + "/PRM/LocationSynchronisation",
    PRM_UpdateRecordLocation: RouterUrl_Root + "/PRM/UpdateRecordLocation",
    PRM_ContainerSize: RouterUrl_Root + "/PRM/ContainerSize",
    PRM_PhysicalRecordsBulkImport:
        RouterUrl_Root + "/PRM/PhysicalRecordsBulkImport",
    PRM_TemplateManagement: RouterUrl_Root + "/PRM/TemplateManagement",
    PRM_BarcodeManagement: RouterUrl_Root + "/PRM/BarcodeManagement",
    PRM_BarcodeManagement_Create: RouterUrl_Root + "/PRM/BarcodeManagement/Create",
    PRM_BarcodeManagement_Edit: RouterUrl_Root + "/PRM/BarcodeManagement/Edit",
    PRM_BarcodeManagement_EditDefault: RouterUrl_Root + "/PRM/BarcodeManagement/EditDefault",
    PRM_RecordsExplorer: RouterUrl_Root + "/PRM/RecordsExplorer",
    PRM_EditTemplate: RouterUrl_Root + "/PRM/EditTemplate",
    PRM_CreateTemplate: RouterUrl_Root + "/PRM/CreateTemplate",
    PRM_Test: RouterUrl_Root + "/PRM/TestR",
    PRM_MyRequest: RouterUrl_Root + "/PRM/MyRequest",
    PRM_ManageHold: RouterUrl_Root + "/PRM/ManageHold",

    PRM_HybridSearch: RouterUrl_Root + "/BCM/HybridSearch",

    PRM_ImportHPRM: RouterUrl_Root + "/PRM/ImportHPRM",
    PRM_CreateTemplateSuite: RouterUrl_Root + "/PRM/CreateTemplateSuite",
    PRM_EditTemplateSuite: RouterUrl_Root + "/PRM/EditTemplateSuite",
    PRM_FolderTemplateManagement:
        RouterUrl_Root + "/PRM/FolderTemplateManagement",
    PRM_RecordTemplateManagement:
        RouterUrl_Root + "/PRM/RecordTemplateManagement",
    PRM_BarcodeTemplate: RouterUrl_Root + "/PRM/BarcodeTemplate",

    //FS URL For Dev
    BCM_FSConnGroup: RouterUrl_Root + "/BCM/FSConnGroup",
    BCM_FSConnection_JobMonitor: RouterUrl_Root + "/BCM/FSConnectionMonitor",
    BCM_FSConnection_JobDetails: RouterUrl_Root + "/BCM/FSConnectionDetail",
    BCM_ContentRepositoryManagement1: "/BCM/ContentRepositoryManagement",

    BCM_AzFileShareConfigureConnection: RouterUrl_Root + "/BCM/AzFileShareConfigureConnection",

    //Box
    BCM_BoxConfigureConnection: RouterUrl_Root + "/BCM/BoxConfigureConnection",

    //Download Center
    DC: RouterUrl_Root + "/DC",
    DC_Download: RouterUrl_Root + "/DC/Download",

    //MyTest:
    Test: RouterUrl_Root + "/Test",
    Test_Timer: RouterUrl_Root + "/Test/Timer",

    //Customize Connector
    Connector: RouterUrl_Root + "/Connector",
    Connector_Index: RouterUrl_Root + "/Connector/Index",
    Connector_CreateOrEdit: RouterUrl_Root + "/Connector/CreateOrEdit",

    //My Task
    MT_PickListForLoanRequests: RouterUrl_Root + "/MT/PickListForLoanRequests",
    MT_PickListForDestruction: RouterUrl_Root + "/MT/PickListForDestruction",
    MT_PickListForMovement: RouterUrl_Root + "/MT/PickListForMovement",
    MT_MachineLearningReview: RouterUrl_Root + "/MT/MachineLearningReview",

    //Machine Learning
    ML_MachineLearning: RouterUrl_Root + "/ML/MachineLearning",

    //Archive Restore Center
    Archiver_RestoreCenter: RouterUrl_Root + "/Archiver/RestoreCenter",

    // File Analysis
    FA: RouterUrl_Root + "/FileAnalysis",
    FA_Discovery: RouterUrl_Root + "/FileAnalysis/Discovery",
    FA_Discovery_Configuration: RouterUrl_Root + "/FileAnalysis/Discovery/Configuration",
    FA_Discovery_Configuration_FS: RouterUrl_Root + "/FileAnalysis/Discovery/Configuration?dataSource=" + DiscoveryDataSource.FileSystem,
    FA_Discovery_Configuration_FSConfigConnection: RouterUrl_Root + "/FileAnalysis/Discovery/ConfigurationFSConfigConnection",
    FA_Discovery_RunJob: RouterUrl_Root + "/FileAnalysis/Discovery/RunJob",
    FA_Discovery_Finish: RouterUrl_Root + "/FileAnalysis/Discovery/Finish",
    FA_Inactive: RouterUrl_Root + "/FileAnalysis/InactiveOptimization",
    FA_ROT: RouterUrl_Root + "/FileAnalysis/ROTOptimization",
    FA_Plan_Profile: RouterUrl_Root + "/FileAnalysis/PlanProfile",
    FA_Plan_PlanView: RouterUrl_Root + "/FileAnalysis/PlanView",
    FA_Discovery_Progress: RouterUrl_Root + "/FileAnalysis/Progress"
};
export default RouterUrls;
