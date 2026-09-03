import RouterUrls from '../Constants/RouterUrls';

class SiteMapLink {
    constructor(text, url, icon) {
        this.text = text;
        this.url = url;
        this.icon = icon;
    }
}

const SiteMapLinks = {
    Home: new SiteMapLink(RMResx.RM_DSB_PageTitle, RouterUrls.Home),
    //Control Panel
    CP: new SiteMapLink(RMResx.RM_Nav_Settings, RouterUrls.CP_Index),
    CP_GeneralSetting: new SiteMapLink(RMResx.RM_GS_Title, RouterUrls.CP_GeneralSetting),
    CP_StorageSettings: new SiteMapLink(RMResx.RM_JS_CP_StorageSetting, RouterUrls.CP_StorageSettings),
    CP_ExportSettings: new SiteMapLink(RMResx.RM_ES_Title, RouterUrls.CP_ExportSettings),
    CP_ExportSettings_CompliantExports: new SiteMapLink(RMResx.RM_ES_CompliantExport_Title, RouterUrls.CP_ExportSettings_CompliantExports),
    CP_DashboardSettings: new SiteMapLink(RMResx.RM_JS_CP_DS_Title, RouterUrls.CP_DashboardSettings),
    CP_TimerJobSettings: new SiteMapLink(RMResx.RM_CP_TimerJob, RouterUrls.CP_TimerJobSettings),
    CP_EmailTemplate: new SiteMapLink(RMResx.RM_CP_EmailTemplateManagement, RouterUrls.CP_EmailTemplate),
    CP_EditEmailTemplate: new SiteMapLink(RMResx.RM_CP_EditEmailTemplate, RouterUrls.CP_EditEmailTemplate),
    CP_CreateEmailTemplate: new SiteMapLink(RMResx.RM_JS_CP_EamilTemplate_CreateTemplate, RouterUrls.CP_CreateEmailTemplate),
    CP_AccountManagement: new SiteMapLink(RMResx.RM_CP_AccountManagement, RouterUrls.CP_AccountManagement),
    CP_AgentManagement: new SiteMapLink(RMResx.RM_CP_Agent_Management, RouterUrls.CP_AgentManagement),
    CP_CSDApiKeyManagement: new SiteMapLink(RMResx.RM_CP_CSDAK_Management, RouterUrls.CP_CSDApiKeyManagement),
    CP_StubSettings: new SiteMapLink(RMResx.RM_AR_CP_StubSettings, RouterUrls.CP_StubSettings),
    CP_EndUserRestore: new SiteMapLink(RMResx.RM_AR_CP_RestoreSetting, RouterUrls.CP_EndUserRestore),
    CP_JobNotification: new SiteMapLink(RMResx.RM_AR_CP_JobNotification, RouterUrls.CP_JobNotification),
    CP_MultiGeo: new SiteMapLink(RMResx.RM_AR_CP_Multi_Geo, RouterUrls.CP_MultiGeo),
    //Job Monitor
    JM: new SiteMapLink(RMResx.RM_JS_JM_Title, RouterUrls.JM_Index),
    JM_DETAIL: new SiteMapLink(RMResx.RM_JM_DetailsTitle, RouterUrls.JM_Detail),
    JM_PLAN_DETAIL: new SiteMapLink(RMResx.RM_JS_JM_PlanDetails, RouterUrls.JM_PlanDetail),
    //RDM
    RDM_RuleManagement: new SiteMapLink(RMResx.RM_Nav_MAN_Rules, RouterUrls.RDM_RuleManagement),
    RDM_CreateRule: new SiteMapLink(RMResx.RM_JS_Common_Create, RouterUrls.RDM_CreateRule),
    RDM_EditRule: new SiteMapLink(RMResx.RM_JS_Common_Edit, RouterUrls.RDM_EditRule),
    RDM_ManualApprovalReview: new SiteMapLink(RMResx.RM_DAM_ManualApprovalReview, RouterUrls.RDM_ManualApprovalReview),
    RDM_WorkFlowManagement: new SiteMapLink(RMResx.RM_Nav_MAN_ApprovalProcesses, RouterUrls.RDM_WorkFlowManagement),
    RDM_ViewWorkFlow: new SiteMapLink(RMResx.RM_RDM_WorkFlow_ViewDetail, RouterUrls.RDM_ViewWorkFlow),
    RDM_CreateWorkFlow: new SiteMapLink(RMResx.RM_RDM_CreateWorkFlow, RouterUrls.RDM_CreateWorkFlow),
    RDM_EditWorkFlow: new SiteMapLink(RMResx.RM_RDM_EditWorkFlow, RouterUrls.RDM_CreateWorkFlow),
    //BCM
    BCM_RecordsExplorer: new SiteMapLink(RMResx.RM_BCM_PageTitle_Explorer, RouterUrls.BCM_RecordsExplorer),
    BCM_TermManagement: new SiteMapLink(RMResx.RM_TM_TermsLabel, RouterUrls.BCM_TermManagement),
    BCM_ContentRepositoryManagement: new SiteMapLink(RMResx.RM_SPS_SharePointSettings, RouterUrls.BCM_ContentRepositoryManagement),
    BCM_ContentRepositoryManagement_SPO: new SiteMapLink(RMResx.RM_JS_SPS_TabLabel_SP, RouterUrls.BCM_ContentRepositoryManagement_SPO),
    BCM_ContentRepositoryManagement_EXO: new SiteMapLink(RMResx.RM_JS_SPS_TabLabel_EXO, RouterUrls.BCM_ContentRepositoryManagement_EXO),
    BCM_ContentRepositoryManagement_Phy: new SiteMapLink(RMResx.RM_JS_SPS_TabLabel_Physical, RouterUrls.BCM_ContentRepositoryManagement_Phy),
    BCM_ContentRepositoryManagement_FS: new SiteMapLink(RMResx.RM_JS_SPS_TabLabel_FS, RouterUrls.BCM_ContentRepositoryManagement_FS),
    BCM_ContentRepositoryManagement_OD: new SiteMapLink(RMResx.RM_JS_SPS_TabLabel_OneDrive, RouterUrls.BCM_ContentRepositoryManagement_OD),
    BCM_ContentRepositoryManagement_LSP: new SiteMapLink(RMResx.RM_JS_SPS_TabLabel_SPLocal, RouterUrls.BCM_ContentRepositoryManagement_LSP),
    BCM_ContentRepositoryManagement_AF: new SiteMapLink(RMResx.RM_JS_SPS_TabLabel_AF, RouterUrls.BCM_ContentRepositoryManagement_AF),
    BCM_ContentRepositoryManagement_Box: new SiteMapLink(RMResx.RM_JS_SPS_TabLabel_Box, RouterUrls.BCM_ContentRepositoryManagement_Box),
    BCM_ContentRepositoryManagement_GoogleDrive: new SiteMapLink(RMResx.RM_JS_SPS_TabLabel_GoogleDrive, RouterUrls.BCM_ContentRepositoryManagement_GoogleDrive),
    BCM_ContentRepositoryManagement_Teams: new SiteMapLink(RMResx.RM_JS_SPS_TabLabel_Teams, RouterUrls.BCM_ContentRepositoryManagement_Teams),
    BCM_ContentRepositoryManagement_Teams_Switch: new SiteMapLink(RMResx.RM_JS_SPS_TabLabel_SwitchToTeams, RouterUrls.BCM_ContentRepositoryManagement_Teams_Switch),

    //RC
    RC_AvailableSpaceReport: new SiteMapLink(RMResx.RM_Nav_RC_AvailableSpace, RouterUrls.RC_AvailableSpaceReportManagement),
    RC_AvailableSpaceReportProfile: new SiteMapLink(RMResx.RM_JS_Common_Create, RouterUrls.RC_AvailableSpaceReportProfile),
    RC_TermUsageReport: new SiteMapLink(RMResx.RM_Nav_RC_TermUsage, RouterUrls.RC_TermUsageReportManagement),
    RC_TermUsageReportProfile: new SiteMapLink(RMResx.RM_JS_Common_Create, RouterUrls.RC_TermUsageReportProfile),
    RC_CreationAndDestructionReport: new SiteMapLink(RMResx.RM_Nav_RC_CreationandDestruction, RouterUrls.RC_CreationAndDestructionReport),
    RC_DueDisposalReportManagement: new SiteMapLink(RMResx.RM_Nav_RC_ContentDueforAction, RouterUrls.RC_DueDisposalReportManagement),
    RC_RuleUsageReportManagement: new SiteMapLink(RMResx.RM_Nav_RC_RuleUsage, RouterUrls.RC_RuleUsageReportManagement),
    RC_AuditReportManagement: new SiteMapLink(RMResx.RM_Nav_RC_AdministratorAudit, RouterUrls.RC_AuditReportManagement),
    RC_ActionAuditReportManagement: new SiteMapLink(RMResx.RM_Nav_RC_ActionAudit, RouterUrls.RC_ActionAuditReportManagement),
    RC_StorageOptimizationReportManagement: new SiteMapLink(RMResx.RM_Nav_RC_SOReport, RouterUrls.RC_StorageOptimizationReportManagement),
    RC_RestoreReportManagement: new SiteMapLink(RMResx.RM_Nav_RC_RestoreReport, RouterUrls.RC_RestoreReportManagement),
    //PRM
    PRM_LocationManagement: new SiteMapLink(RMResx.RM_Nav_PR_LocationManager, RouterUrls.PRM_LocationManagement),
    PRM_LocationSynchronisation: new SiteMapLink(RMResx.RM_PRM_LS_PageTitle, RouterUrls.PRM_LocationSynchronisation),
    PRM_UpdateRecordLocation: new SiteMapLink(RMResx.RM_URL_PageTitle, RouterUrls.PRM_UpdateRecordLocation),
    PRM_ContainerSize: new SiteMapLink(RMResx.RM_CZ_PageTitle, RouterUrls.PRM_ContainerSize),
    PRM_PhysicalRecordsBulkImport: new SiteMapLink(RMResx.RM_PRM_PhysicalRecordsImport_PageTitle, RouterUrls.PRM_PhysicalRecordsBulkImport),
    PRM_TemplateManagement: new SiteMapLink(RMResx.RM_Nav_PR_TemplateManager, RouterUrls.PRM_TemplateManagement),
    PRM_RecordsManagement: new SiteMapLink(RMResx.RM_PRM_TM_Records_Template, RouterUrls.PRM_TemplateManagement),
    PRM_BarcodeManagement: new SiteMapLink(RMResx.RM_PRM_TM_Barcode_Template, RouterUrls.PRM_BarcodeManagement),
    PRM_BarcodeManagement_Create: new SiteMapLink(RMResx.RM_PRM_TM_Barcode_Template_Create, RouterUrls.PRM_BarcodeManagement_Create),
    PRM_BarcodeManagement_Edit: new SiteMapLink(RMResx.RM_PRM_TM_Barcode_Template_Edit, RouterUrls.PRM_BarcodeManagement_Edit),
    PRM_BarcodeManagement_EditDefault: new SiteMapLink(RMResx.RM_PRM_TM_Barcode_Template_EditDefault, RouterUrls.PRM_BarcodeManagement_EditDefault),
    PRM_EditTemplate: new SiteMapLink(RMResx.RM_PRM_TM_EditTemplate_PageTitle, RouterUrls.PRM_EditTemplate),
    PRM_RecordsExplorer: new SiteMapLink(RMResx.RM_PRM_RecordsExplorer_PageTitle, RouterUrls.PRM_RecordsExplorer),

    //PRM_GlobalSearch: new SiteMapLink(RMResx.RM_HS_HybridSearchTitle, RouterUrls.PRM_GlobalSearch+"/?source=4"),
    PRM_HybridSearch: new SiteMapLink(RMResx.RM_Nav_Search, RouterUrls.PRM_HybridSearch),

    PRM_RequestForReview:new SiteMapLink(RMResx.RM_Nav_MT_RequestForReview,RouterUrls.PRM_MyRequest),
    PRM_Search:new SiteMapLink(RMResx.RM_Nav_Search,RouterUrls.PRM_HybridSearch),
    PRM_MyRequest: new SiteMapLink(RMResx.RM_Nav_MT_RequestForReview, RouterUrls.PRM_MyRequest),
    PRM_RequestManagement: new SiteMapLink(RMResx.RM_Nav_MT_RequestForReview, RouterUrls.PRM_MyRequest),
    PRM_ManageHold: new SiteMapLink(RMResx.RM_JS_RDM_Hold_ManageHoldTitle, RouterUrls.PRM_ManageHold),
    PRM_CreateTemplateSuite: new SiteMapLink(RMResx.RM_PRM_TM_Btn_NewSuite, RouterUrls.PRM_CreateTemplateSuite),
    PRM_EditTemplateSuite: new SiteMapLink(RMResx.RM_PRM_TM_EditTemplateSuite_PageTitle, RouterUrls.PRM_EditTemplateSuite),
    PRM_BoxTemplate: new SiteMapLink(RMResx.RM_PRM_TM_BoxTemplateManagement_PageTitle, RouterUrls.PRM_BoxTemplateManagement),
    PRM_FolderTemplate: new SiteMapLink(RMResx.RM_PRM_TM_FolderTemplateManagement_PageTitle, RouterUrls.PRM_FolderTemplateManagement),
    PRM_RecordTemplate: new SiteMapLink(RMResx.RM_PRM_TM_RecordTemplateManagement_PageTitle, RouterUrls.PRM_RecordTemplateManagement),
    PRM_BarcodeTemplate: new SiteMapLink(RMResx.RM_PRM_BarcodeTemplate, RouterUrls.PRM_BarcodeTemplate),
    BCM_ContentRepositoryManagement1: new SiteMapLink(RMResx.RM_SPS_SharePointSettings, RouterUrls.BCM_ContentRepositoryManagement1),
    BCM_FSConnGroup: new SiteMapLink(RMResx.RM_FS_Register_PageTitle, RouterUrls.BCM_FSConnGroup),
    BCM_FSConnection_JobMonitor: new SiteMapLink(RMResx.RM_FS_Connection_JobMonitor_PageTitle, RouterUrls.BCM_FSConnection_JobMonitor),
    BCM_FSConnection_JobDetails: new SiteMapLink(RMResx.RM_FS_Connection_JobDetails_PageTitle, RouterUrls.BCM_FSConnection_JobDetails),
    BCM_AzFileConnGroup: new SiteMapLink(RMResx.RM_AF_Register_PageTitle_Link, RouterUrls.BCM_AzFileShareConfigureConnection),
    BCM_BoxConnGroup: new SiteMapLink(RMResx.RM_AF_Register_PageTitle_Link, RouterUrls.BCM_BoxConfigureConnection),

    //Download Center
    DC: new SiteMapLink(RMResx.RM_JS_DC_Title, RouterUrls.DC_Download),

    //Connector
    Connector: new SiteMapLink(RMResx.RM_Connector_Title, RouterUrls.Connector),
    // Connector_CreateOrEdit: new SiteMapLink("Connector", RouterUrls.Connector_CreateOrEdit),

    //My Task
    MT_PickListForLoanRequests: new SiteMapLink(RMResx.RM_MT_PickList_LoanRequests, RouterUrls.MT_PickListForLoanRequests),
    MT_PickListForDestruction: new SiteMapLink(RMResx.RM_MT_PickList_Destruction, RouterUrls.MT_PickListForDestruction),
    MT_PickListForMovement: new SiteMapLink(RMResx.RM_MT_PickList_Movement, RouterUrls.MT_PickListForMovement),
    MT_MachineLearningReview: new SiteMapLink(RMResx.RM_MT_MachineLearningReview, RouterUrls.MT_MachineLearningReview),

    //Machine Learning
    MT_MachineLearning: new SiteMapLink(RMResx.RM_ML_MachineLearning, RouterUrls.ML_MachineLearning),

    //Archive Restore Center
    Archiver_RestoreCenter: new SiteMapLink(RMResx.RM_AR_RC_Title, RouterUrls.Archiver_RestoreCenter),

    // File Analysis
    FA_Discovery: new SiteMapLink(RMResx.RM_FA_Discovery, RouterUrls.FA_Discovery),
    FA_Discovery_FS: new SiteMapLink(RMResx.RM_FA_Discovery, RouterUrls.FA_Discovery_Configuration_FS),
    FA_Discovery_FS_ConfigConnection: new SiteMapLink(RMResx.RM_FA_Discovery_ConfigConnection, RouterUrls.FA_Discovery_Configuration_FSConfigConnection),
    FA_Inactive: new SiteMapLink(RMResx.RM_FA_Inactive, RouterUrls.FA_Inactive),
    FA_ROT: new SiteMapLink(RMResx.RM_FA_ROT, RouterUrls.FA_ROT),
    FA_Plan_Profile: new SiteMapLink(RMResx.RM_FA_Plan_Profile, RouterUrls.FA_Plan_Profile),
    FA_Plan_ViewPlan: new SiteMapLink("^^View detail plan", RouterUrls.FA_Plan_PlanView),
    FA_Discovery_Progress: new SiteMapLink(RMResx.RM_FA_Progress, RouterUrls.FA_Discovery_Progress),
};

export default SiteMapLinks;