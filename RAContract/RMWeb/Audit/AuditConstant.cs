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
namespace AvePoint.RA.Contract.RMWeb.Audit
{
    using System;
    using System.ComponentModel;
    /// <summary>
    /// 添加新的Action时，要注意：不可以修改之前定义好的枚举对应的int值，否则会引起老数据显示错误的问题。
    /// Description中为界面上显示对应词条的Key，
    /// 通过扩展方法ToDescription得到界面显示词条，例如： ((AuditAction)300).ToDescription()
    /// </summary>
    public enum AuditAction
    {
        [Description("RM_RC_Audit_Unknown")]
        Unknown = 0,

        #region Control Panel (1 ~ 999)
        //Global Settings (1 ~ 50)
        [Description("RM_RC_Audit_Action_ConfigureGlobalsettings")]
        ConfigureGlobalsettings = 1,

        //Workflow (51 ~99)
        [Description("RM_RC_Audit_Action_NewWorkflow")]
        CreateWorkflow = 51,
        [Description("RM_RC_Audit_Action_EditWorkflow")]
        EditWorkflow = 52,
        [Description("RM_RC_Audit_Action_DeleteWorkflow")]
        DeleteWorkflow = 53,

        //DocAve Connection (100 ~ 199)
        [Description("RM_RC_Audit_Action_ConfigureDocAveConnection")]
        ConfigureDocAveConnection = 100,

        //General Settings and Export Settings (200 ~ 299)
        [Description("RM_RC_Audit_Action_ConfigureGeneralSetting")]
        ConfigureGeneralSetting = 200,
        [Description("RM_RC_Audit_Action_ConfigureExportSetting")]
        ConfigureExportSetting = 230,
        [Description("RM_RC_Audit_Action_DeleteExportSetting")]
        DeleteExportSetting = 231,
        [Description("RM_RC_Audit_Action_ConfigureDashboardSetting")]
        ConfigureDashboardSetting = 232,
        [Description("RM_RC_Audit_Action_GenerateExportEncryptionKey")]
        GenerateExportEncryptionKey = 233,
        [Description("RM_ES_CompliantExport_Title")]
        CompliantExport = 234,

        //Authentication Management (300 ~ 399)
        [Description("RM_RC_Audit_Action_SetDefaultAuthenticationMode")]
        SetDefaultAuthenticationMode = 300,
        [Description("RM_RC_Audit_Action_EnableAuthenticationMode")]
        EnableAuthenticationMode = 301,
        [Description("RM_RC_Audit_Action_DisableAuthenticationMode")]
        DisableAuthenticationMode = 302,
        [Description("RM_RC_Audit_Action_AddADDomain")]
        AddADDomain = 303,
        [Description("RM_RC_Audit_Action_EditADDomain")]
        EditADDomain = 304,
        [Description("RM_RC_Audit_Action_DeleteADDomain")]
        DeleteADDomain = 305,
        [Description("RM_RC_Audit_Action_EnableADDomain")]
        EnableADDomain = 306,
        [Description("RM_RC_Audit_Action_DisableADDomain")]
        DisableADDomain = 307,

        //Account Management (400 ~ 449)
        [Description("RM_RC_Audit_Action_AddADAccount")]
        AddADAccount = 400,
        [Description("RM_RC_Audit_Action_EditLocalAccount")]
        EditLocalAccount = 401,
        [Description("RM_RC_Audit_Action_DeleteAccount")]
        DeleteAccount = 402,
        //Security Group (450 ~ 499)
        [Description("RM_RC_Audit_Action_CreateSecurityGroup")]
        CreateSecurityGroup = 450,
        [Description("RM_RC_Audit_Action_EditSecurityGroup")]
        EditSecurityGroup = 451,
        [Description("RM_RC_Audit_Action_DeleteSecurityGroup")]
        DeleteSecurityGroup = 452,

        [Description("RM_RC_Audit_Action_EditEmailTempalte")]
        EditEmailTempalte = 501,

        [Description("RM_RC_Audit_Action_CreateEmailTempalte")]
        CreateEmailTemplate = 502,

        [Description("RM_RC_Audit_Action_DeleteEmailTempalte")]
        DeleteEmailTemplate = 503,

        #endregion

        //App Management (600 ~ 699)
        [Description("RM_RC_Audit_Action_AddClientId")]
        AddClientId = 600,
        [Description("RM_RC_Audit_Action_EditClientId")]
        EditClientId = 601,
        [Description("RM_RC_Audit_Action_RegisterAgent")]
        RegisterAgent = 603,
        [Description("RM_RC_Audit_Action_EditAgent")]
        EditAgent = 604,
        [Description("RM_RC_Audit_Action_EnableAgent")]
        EnableAgent = 605,
        [Description("RM_RC_Audit_Action_DisableAgent")]
        DisableAgent = 606,
        [Description("RM_RC_Audit_Action_DeleteAgent")]
        DeleteAgent = 607,
        [Description("RM_RC_Audit_Action_DownloadAgentConfigFile")]
        DownloadAgentConfigFile = 608,
        [Description("RM_RC_Audit_Action_DownloadCertificate")]
        DownloadCertficate = 609,
        [Description("RM_RC_Audit_Action_SetAsDefaultCertificate")]
        SetAsDefaultCertificate = 610,
        [Description("RM_RC_Audit_Action_DeleteCertificate")]
        DeleteCertificate = 611,
        [Description("RM_RC_Audit_Action_UpdateCertificate2Agents")]
        UpdateCertificate2Agents = 612,
        [Description("RM_CP_AM_Certificate_CreateBtn")]
        CreateCertificate = 613,
        [Description("RM_RC_Audit_Action_UpgradeAgent")]
        UpgradeAgent = 614,


        [Description("RM_RC_Audit_Action_CSDAddApiKey")]
        CSDAddApiKey = 997,
        [Description("RM_RC_Audit_Action_CSDEditApiKey")]
        CSDEditApiKey = 998,
        [Description("RM_RC_Audit_Action_CSDDeleteApiKey")]
        CSDDeleteApiKey = 999,

        #region Retention and Disposal Management (1000 ~ 1999)
        //Rule Management (1000 ~ 1099)
        [Description("RM_RC_Audit_Action_CreateRule")]
        CreateRule = 1000,
        [Description("RM_RC_Audit_Action_EditRule")]
        EditRule = 1001,
        [Description("RM_RC_Audit_Action_DeleteRule")]
        DeleteRule = 1002,
        [Description("RM_RC_Audit_Action_CreateRuleContainer")]
        CreateRuleContainer = 1003,
        [Description("RM_RC_Audit_Action_EditRuleContainer")]
        EditRuleContainer = 1004,
        [Description("RM_RC_Audit_Action_DeleteRuleContainer")]
        DeleteRuleContainer = 1005,

        //Disposal Activity Management (1100 ~ 1199)
        [Description("RM_RC_Audit_Action_RunDisposalJob")]
        RunDisposalJob = 1100,
        [Description("RM_RC_Audit_Action_ConfigureDisposalJob")]
        ConfigureDisposalJobSchedule = 1101,

        //Manual Approval(1200 ~ 1299)
        [Description("RM_RC_Audit_Action_ConfigureManualApproval")]
        ConfigureManualApproval = 1200,
        [Description("RM_RC_Audit_Action_RunManualApproval")]
        RunManualApproval = 1201,

        [Description("RM_RC_Audit_Action_MarkToApproved")]
        MarkToApproved = 1202,

        [Description("RM_RC_Audit_Action_MarkToRejected")]
        MarkToRejected = 1203,
        [Description("RM_MA_Escalate")]
        EscalateTo = 1204,
        [Description("RM_RC_Audit_ManualApproveExportHistory")]
        ExportHistory = 1205,
        [Description("RM_RC_Audit_ManualApproveChangeAction")]
        ChangeAction = 1206,
        [Description("RM_RC_Audit_Action_MarkToExtend")]
        MarkToExtend = 1207,
        [Description("RM_MA_Reassign")]
        ReassignTo = 1208,
        [Description("RM_RC_Audit_Action_RunManualApproveOrReject")]
        RunManualApproveOrReject = 1209,
        [Description("RM_RC_Audit_Action_RestoreExtend")]
        RestoreExtend = 1210,
        [Description("RM_RC_Audit_Action_RunExportHistroyJob")]
        RunExportHistoryJob = 1211,
        [Description("RM_RC_Audit_Action_ResetManualWorkflow")]
        ResetManualWorkflow = 1212,
        [Description("RM_RC_Audit_Action_RunExportRecordsForReviewJob")]
        RunExportRecordsForReviewJob = 1213,
        [Description("RM_RC_Audit_Action_RunImportUnderReviewJob")]
        RunImportUnderReviewJob = 1214,
        [Description("RM_RC_Audit_Action_SaveApprovalCommentOption")]
        SaveApprovalCommentOption = 1215,
        [Description("RM_RC_Audit_Action_RunFolderViewActionJob")]
        RunFolderViewActionJob = 1216,
        [Description("RM_RC_Audit_Action_MarkToPause")]
        MarkToPause = 1217,
        [Description("RM_RC_Audit_Action_MarkToResume")]
        MarkToResume = 1218,
        [Description("RM_FS_Audit_Action_GenerateDisposalHistory")]
        GenerateDisposalHistory = 1219,
        #endregion

        #region Business Classification Management (2000 ~ 2999)
        //Term Management (2000 ~ 2099)
        [Description("RM_RC_Audit_Action_CreateTerm")]
        CreateTerm = 2000,
        [Description("RM_RC_Audit_Action_RenameTerm")]
        RenameTerm = 2001,
        [Description("RM_RC_Audit_Action_DeprecateTerm")]
        DeprecateTerm = 2002,
        [Description("RM_RC_Audit_Action_DeleteTerm")]
        DeleteTerm = 2003,
        [Description("RM_RC_Audit_Action_ConfigureTermGeneralSetting")]
        ConfigureTermGeneralSetting = 2004,
        [Description("RM_RC_Audit_Action_RenameTermGroup")]
        RenameTermGroup = 2005,
        [Description("RM_RC_Audit_Action_RenameTermSet")]
        RenameTermSet = 2006,

        [Description("RM_RC_Audit_Action_CreateLocationTerm")]
        CreateLocationTerm = 2007,
        [Description("RM_RC_Audit_Action_RenameLocationTerm")]
        RenameLocationTerm = 2008,
        [Description("RM_RC_Audit_Action_DeleteLocationTerm")]
        DeleteLocationTerm = 2009,
        [Description("RM_RC_Audit_Action_RenameLocationTermSet")]
        RenameLocationTermSet = 2010,
        [Description("RM_RC_Audit_Action_GetSitecollectionManagedMetadataServices")]
        GetManagedMetadataServices = 2011,
        [Description("RM_RC_Audit_Action_ImportTerm")]
        ImportTerm = 2012,
        [Description("RM_RC_Audit_Action_ImportTerm")]
        ImportGoogleTerm = 2019,
        [Description("RM_RC_Audit_Action_EnableTerm")]
        EnableTerm = 2013,
        [Description("RM_RC_Audit_Action_ExportTerm")]
        ExportTerm = 2014,
        [Description("RM_RC_Audit_Action_ConfigureLocationTermSetting")]
        ConfigLocationTermSettings = 2015,
        [Description("RM_RC_Audit_Action_DeleteRootTerms")]
        DeleteRootTerms = 2016,
        [Description("RM_JS_TM_CreateTermSet")]
        CreateTermSet = 2017,
        [Description("RM_RC_Audit_Action_RunSPOnpremSyncTermJob")]
        RunSPOnpremSyncTermJob = 2018,
        [Description("RM_JS_Common_GenerateReport")]
        AIRecommendation = 2020,
        //Enforce Retention

        [Description("RM_BCM_Audit_Action_RunEnforceRetentionJob")]
        RunEnforceRetentionJob = 2060,

        //SharePoint Settings (2100 ~ 2199)
        [Description("RM_RC_Audit_Action_ConfigureGroupGlobalsetting")]
        ConfigureGroupGlobalsetting = 2100,
        [Description("RM_RC_Audit_Action_ConfigureCustomSetting")]
        ConfigureCustomSetting = 2101,
        #region Obsolete Action
        [Obsolete]
        [Description("RM_RC_Audit_Action_ConfigureInheritSetting")]
        ConfigureInheritSetting = 2102,
        #endregion
        [Description("RM_RC_Audit_Action_SaveGlobalSettintExistColumn")]
        SaveGlobalSettingExistColumn = 2103,
        [Description("RM_RC_Audit_Action_ApplySharePointSetting")]
        ApplySharePointSetting = 2104,
        [Description("RM_RC_Audit_Action_RunCollectionJob")]
        RunCollectionJob = 2105,
        #region Obsolete Action
        [Obsolete]
        [Description("RM_RC_Audit_Action_UseBCSColumn")]
        EnableRecordsManagement = 2106,
        #endregion
        [Description("RM_RC_Audit_Action_RunFSDashboardJob")]
        RunFSDashboardJob = 2107,
        [Description("RM_RC_Audit_Action_GeneralSetting4SPO")]
        GeneralSetting4SPO = 2108,
        [Description("RM_RC_Audit_Action_ArchiverGeneralSetting4SPO")]
        ArchiverGeneralSetting = 2109,
        //Term Synchronization (2200 ~ 2299)
        [Description("RM_RC_Audit_Action_ConfigureScheduleForTermSynchronization")]
        ConfigureScheduleForTermSynchronization = 2201,
        [Description("RM_RC_Audit_Action_RunTermSyncJob")]
        RunTermSyncJob = 2202,

        [Description("RM_RC_Audit_Action_ConfigSharePointSchedule")]
        ConfigureSharePointSettingsSchedule = 2203,

        [Description("RM_RC_Audit_Action_RunSharePointScheduleJob")]
        RunSharePointSettingsScheduleJob = 2204,

        [Description("RM_RC_Audit_Action_ConfigureTermGroupSetting")]
        ConfigureTermGroupSetting = 2205,

        [Description("RM_RC_Audit_Action_DeleteTermGroup")]
        DeleteTermGroup = 2206,

        [Description("RM_RC_Audit_Action_CreateTermGroup")]
        CreateTermGroup = 2207,

        [Description("RM_RC_Audit_Action_ConfigureTermSetSetting")]
        ConfigureTermSetSetting = 2208,

        [Description("RM_RC_Audit_Action_UniqueIdSetting")]
        UniqueIDSetting = 2209,
        [Description("RM_RC_Audit_Action_RunUniqueIdSettingJob")]
        RunUniqueIDSettingJob = 2210,
        [Description("RM_RC_Audit_Action_ConfigureUniqueIdSettingJobSchedule")]
        ConfigureUniqueIDSettingSchedule = 2211,
        #region Obsolete Action
        [Obsolete]
        [Description("RM_RC_Audit_Action_EnableIsSync")]
        EnableIsSync = 2212,
        #endregion
        [Description("RM_RC_Audit_Action_ConfigSharePointOnlineSchedule")]
        ConfigureSharePointOnlineSettingsSchedule = 2213,
        [Description("RM_RC_Audit_Action_SP_ImportSetting")]
        ImportSPSetting = 2214,
        [Description("RM_RC_Audit_Action_SP_ExportSetting")]
        ExportSPSetting = 2215,
        [Description("RM_RC_Audit_Action_SPSO_ExportSetting")]
        ExportSPSOSetting = 2216,

        //Explorer (2300 ~ 2399)
        [Description("RM_BCM_Audit_Action_ChangeTerm")]
        ChangeTerm = 2301,

        [Description("RM_BCM_Audit_Action_ManageRelatedRecords")]
        ManageRelatedRecords = 2302,
        [Description("RM_BCM_Audit_Action_DeclareAsRecord")]
        DeclareAsRecord = 2303,

        [Description("RM_BCM_Audit_Action_CreateHoldTypeWithRecord")]
        CreateHoldTypeWithRecord = 2304,

        [Description("RM_BCM_Audit_Action_ReuseHoldTypeWithRecord")]
        ReuseHoldTypeWithRecord = 2305,

        [Description("RM_BCM_Audit_Action_CancelHoldByRecords")]
        CancelHoldByRecords = 2306,

        [Description("RM_BCM_Audit_Action_SusPendRecords")]
        SusPendRecords = 2307,

        [Description("RM_BCM_Audit_Action_CancelHold")]
        CancelHold = 2308,

        [Description("RM_BCM_Audit_Action_SuspendHold")]
        SuspendHold = 2309,

        [Description("RM_BCM_Audit_Action_DeleteHold")]
        DeleteHold = 2310,

        [Description("RM_BCM_Audit_Action_UndeclareAsRecord")]
        UndeclareAsRecord = 2311,

        [Description("RM_BCM_Audit_Action_CreateHold")]
        CreateHold = 2312,

        [Description("RM_BCM_Audit_Action_ChangeHoldCreate")]
        ChangeHoldCreate = 2313,

        [Description("RM_BCM_Audit_Action_ChangeHoldReuse")]
        ChangeHoldReuse = 2314,

        [Description("RM_BCM_Audit_Action_FSManageRelatedRecords")]
        FSManageRelatedRecords = 2315,
        [Description("RM_BCM_Audit_Action_ExplorerRecordsMove")]
        ExplorerRecordsMove = 2316,
        [Description("RM_BCM_Audit_Action_MoveCheckSPUrl")]
        MoveCheckSPUrl = 2317,
        [Description("RM_BCM_Audit_Action_MoveCheckFSUNCLocation")]
        MoveCheckFSUNCLocation = 2318,
        //Please use MoveCheckSPUrl, this property is deprecated
        [Description("RM_BCM_Audit_Action_RuleCheckSPUrl")]
        RuleCheckSPUrl = 2319,
        //Please use MoveCheckFSUNCLocation, this property is deprecated
        [Description("RM_BCM_Audit_Action_RuleCheckFSUNCLocation")]
        RuleCheckFSUNCLocation = 2320,
        [Description("RM_BCM_Audit_Action_EditHold")]
        EditHold = 2321,
        [Description("RM_BCM_Audit_Action_RunPhysicalExplorerTimer")]
        RunPhysicalExplorerTimer = 2322,
        [Description("RM_BCM_Audit_Action_PhyExplorerRecordsMove")]
        PhysicalExplorerMove = 2323,
        [Description("RM_BCM_Audit_Action_PhyManageRelatedRecords")]
        PhysicalManageRelatedRecords = 2324,
        [Description("RM_BCM_Audit_Action_RemovePersonalHold")]
        RemovePersonalHold = 2325,
        [Description("RM_DC_Audit_Action_DownloadArchivedContent")]
        DownloadArchivedContent = 2326,
        [Description("RM_DC_Audit_Action_DeleteArchivedContent")]
        DeleteArchivedContent = 2327,
        [Description("RM_DC_Audit_StartDownloadArchivedContentJob")]
        StartDownloadArchivedContentJob = 2328,
        [Description("RM_BCM_Audit_Action_CreateAppendHoldTypeWithRecord")]
        CreateAppendHoldTypeWithRecord = 2329,
        [Description("RM_BCM_Audit_Action_ReuseAppendHoldTypeWithRecord")]
        ReuseAppendHoldTypeWithRecord = 2330,
        [Description("RM_BCM_Audit_Action_ChangeLabel")]
        ChangeLabel = 2331,
        [Description("RM_BCM_Audit_Action_ManageRelatedRecordsInApp")]
        SpfxManageRelatedRecords = 2332,
        [Description("RM_RC_Audit_Action_FSUniqueIDSetting")]
        FSUniqueIDSetting = 2333,
        [Description("RM_RC_Audit_Action_TeamsUniqueIDSetting")]
        TeamsUniqueIDSetting = 2334,
        [Description("RM_BCM_History_AddRecordLabel")]
        AddRecordLabel = 2335,
        [Description("RM_BCM_History_RemoveRecordLabel")]
        RemoveRecordLabel = 2336,
        [Description("RM_BCM_Audit_Action_DeclareAsRecord")]
        DeclareSPOOnPreAsRecord = 2337,
        [Description("RM_BCM_Audit_Action_UndeclareAsRecord")]
        UndeclareSPOOnPreAsRecord = 2311,
        [Description("RM_BCM_Audit_Action_ExportHoldRecords")]
        ExportHoldRecords = 2338,
        [Description("RM_BCM_Audit_Action_ImportHoldRecords")]
        ImportHoldRecords = 2339,
        [Description("RM_BCM_Audit_Action_ImportWorkspaceHold")]
        ImportWorkspaceHold = 2340,
        //New SharePoint Settings (2400 ~ 2499)
        [Description("RM_BCM_Audit_Action_ConfigureColumnSettings")]
        EditColumnSetting = 2400,
        [Description("RM_BCM_Audit_Action_ConfigureRecordOwnerSettings")]
        EditLocationOwnersSetting = 2401,
        [Description("RM_BCM_Audit_Action_ConfigureDocumentLevelTermSettings")]
        EditDocLevelSetting = 2402,
        [Description("RM_BCM_Audit_Action_ConfigureContainerLevelTermSettings")]
        EditConLevelSetting = 2403,
        [Description("RM_RC_Audit_Action_ConfigureInheritSetting")]
        EditInheritSetting = 2404,
        [Description("RM_RC_Audit_Action_ConfigureCollectionJob")]
        ConfigureCollectionJobSchedule = 2405,
        [Description("RM_RC_Audit_Action_ArchiverConfigureInheritSetting")]
        EditArchiverInheritSetting = 2406,
        [Description("RM_RC_Audit_Action_ConfigureArchiverSetting")]
        EditArchiverSetting = 2407,
        [Description("RM_RC_Audit_Action_InheritSubNodeToCurrent")]
        InheritSubNodeToCurrent = 2408,
        [Description("RM_RC_Audit_Action_InheritSubNodeToCurrent4OneDrive")]
        InheritSubNodeToCurrent4OneDrive = 2409,

        [Description("RM_RC_Audit_Action_SaveCustomMetadataColumn")]
        SaveCustomMetadataColumn = 2410,
        [Description("RM_RC_Audit_Action_SaveCustomIndexMetadata")]
        SaveCustomIndexMetadata = 2411,


        //EXO  Settings
        [Description("RM_BCM_Audit_Action_ConfigureRecordOwnerSettings4EXO")]
        EditEXOLocationOwnersSetting = 2501,
        [Description("RM_BCM_Audit_Action_ConfigureEXOTermSettings")]
        EditEXOTermSetting = 2502,
        [Description("RM_RC_Audit_Action_ConfigureInheritSetting4EXO")]
        EditEXOInheritSetting = 2503,
        [Description("RM_RC_Audit_Action_ConfigureDisposalJob4EXO")]
        ConfigureDisposalJobSchedule4EXO = 2504,
        [Description("RM_RC_Audit_Action_RunDisposalJob4EXO")]
        RunEXODisposalJob = 2505,
        [Description("RM_RC_Audit_Action_ApplyEXOSetting")]
        ApplyEXOSetting = 2506,
        #region Obsolete Action
        [Obsolete]
        [Description("RM_RC_Audit_Action_EnableIsSync")]
        EXOEnableIsSync = 2507,
        #endregion
        [Description("RM_RC_Audit_Action_RunEXOSettingsScheduleJob")]
        RunEXOSettingsScheduleJob = 2508,
        [Description("RM_RC_Audit_Action_ConfigureEXOSettingsScheduleJob")]
        ConfigureEXOSettingsScheduleJob = 2509,
        #region Obsolete Action
        [Obsolete]
        [Description("RM_RC_Audit_Action_UseBCSColumn")]
        EnableRecordsManagementEXO = 2510,
        #endregion
        [Description("RM_RC_Audit_Action_GeneralSetting4EXO")]
        GeneralSetting4EXO = 2511,
        [Description("RM_RC_Audit_Action_RunCollectionJob4EXO")]
        RunCollectionJob4EXO = 2512,

        //Physical Records Settings
        [Description("RM_BCM_Audit_Action_ConfigureRecordOwnerSettings4PR")]
        EditPRLocationOwnersSetting = 2601,
        [Description("RM_BCM_Audit_Action_ConfigureTermSettings4PR")]
        EditPRTermSetting = 2602,
        [Description("RM_RC_Audit_Action_ConfigureInheritSetting4PR")]
        EditPRInheritSetting = 2603,
        [Description("RM_RC_Audit_Action_RunDisposalJob4PR")]
        RunPRDisposalJob = 2608,

        [Description("RM_BCM_Audit_Action_DownLoadPhysicalExportBarcodeReport")]
        DownLoadPhysicalExportBarcodeReport = 2604,
        [Description("RM_BCM_Audit_Action_RunPhysicalExportBarcodeJob")]
        RunPhysicalExportBarcodeJob = 2605,
        [Description("RM_BCM_Audit_Action_RunPhysicalSetPermissionJob")]
        RunPhysicalSetPermissionJob = 2606,
        [Description("RM_RC_Audit_Action_ConfigureDisposalJob4PR")]
        ConfigureDisposalJobSchedule4PR = 2607,

        //FS Content Repository Management (2700 ~ 2799)
        [Description("RM_BCM_Audit_Action_FSDeactiveSetting")]
        FSDeactiveSetting = 2700,
        [Description("RM_BCM_Audit_Action_ConfigureRecordOwnerSettings4FS")]
        FSEditLocationOwnersSetting = 2701,
        [Description("RM_BCM_Audit_Action_ConfigureTermSettings4FS")]
        FSEditDocLevelSetting = 2702,
        #region Obsolete Action
        [Obsolete]
        [Description("RM_BCM_Audit_Action_ConfigureContainerLevelTermSettings")]
        FSEditConLevelSetting = 2703,
        #endregion
        [Description("RM_RC_Audit_Action_ConfigureInheritSetting4FS")]
        FSEditInheritSetting = 2704,
        [Description("RM_RC_Audit_Action_RunCollectionJob4FS")]
        RunFSCollectionJob = 2705,
        [Description("RM_RC_Audit_Action_RunDisposalJob4FS")]
        RunFSDisposalJob = 2706,
        [Description("RM_BCM_Audit_Action_FSActiveSetting")]
        FSActiveSetting = 2707,
        [Description("RM_BCM_Audit_Action_RunFSManageHoldJob")]
        RunFSManageHoldJob = 2708,
        [Description("RM_BCM_Audit_Action_RunFSReclassicfyJob")]
        RunFSReclassicfyJob = 2709,

        //global search action
        [Description("RM_BCM_Audit_Action_RunGlobalSearchActionJob")]
        RunGlobalSearchActionJob = 2710,
        [Description("RM_BCM_Audit_Action_ExportSearchResult")]
        ExportSearchResult = 2711,
        [Description("RM_BCM_Audit_Action_RunExportSearchResultJob")]
        RunExportSearchResultJob = 2712,

        [Description("RM_RC_Audit_Action_ConfigureDisposalJob4FS")]
        ConfigureDisposalJobSchedule4FS = 2713,
        [Description("RM_RC_Audit_Action_RunFSRestoreJob")]
        RunFSRestoreJob = 2714,
        [Description("RM_RC_Audit_Action_RunApplyClassCodeJob")]
        RunFSApplyClassCodeJob = 2715,
        [Description("RM_BCM_Audit_Action_ConfigureClassCodeSettings4FS")]
        FSEditDocLevelSettingForJPMC = 2716,
        [Description("RM_FS_ClassCodePolicy_ApplyClassCode")]
        ApplyClassCodeSettings4FS = 2718,
        [Description("RM_RC_Audit_Action_GeneralSetting4FS")]
        FSEditGeneralSettingForJPMC = 2719,
        [Description("RM_JS_FS_DisposalOnSpecificClassCode")]
        RunFSClassCodeDisposalJob = 2720,
        [Description("RM_JS_JM_JobType_FSMyHubDashboard")]
        FSMyHubDashboard = 2721,
        [Description("RM_JS_JM_JobType_MyhubClassify")]
        MyhubClassify = 2722,
        //SharePoint On Prem Settings (2800 ~ 2899)
        [Description("RM_BCM_Audit_Action_ConfigureColumnSettings4SPOnPrem")]
        EditSPOnPremColumnSetting = 2800,
        [Description("RM_BCM_Audit_Action_ConfigureRecordOwnerSettings4SPOnPrem")]
        EditSPOnPremLocationOwnersSetting = 2801,
        [Description("RM_BCM_Audit_Action_ConfigureDocumentLevelTermSettings4SPOnPrem")]
        EditSPOnPremDocLevelSetting = 2802,
        [Description("RM_BCM_Audit_Action_ConfigureContainerLevelTermSettings4SPOnPrem")]
        EditSPOnPremConLevelSetting = 2803,
        [Description("RM_RC_Audit_Action_ConfigureInheritSetting4SPOnPrem")]
        EditSPOnPremInheritSetting = 2804,
        #region Obsolete Action
        [Obsolete]
        [Description("RM_RC_Audit_Action_UseBCSColumn")]
        EnableRecordsManagementSPOnPrem = 2805,
        #endregion
        [Description("RM_RC_Audit_Action_RunSharePointOnPremSettingSchedule")]
        RunApplySharePointSettingSPOnPremSchedule = 2807,
        #region Obsolete Action
        [Obsolete]
        [Description("RM_RC_Audit_Action_EnableIsSync")]
        EnableIsSyncSPOnPrem = 2806,
        #endregion
        [Description("RM_RC_Audit_Action_ApplySharePointOnPremSetting")]
        ApplySharePointSettingSPOnPrem = 2808,
        [Description("RM_RC_Audit_Action_RunSPOnPremUniqueIdSettingJob")]
        RunSPOnPremUniqueIDSettingJob = 2809,
        [Description("RM_RC_Audit_Action_RunSPOnPremDashboardJob")]
        RunSPOnPremDashboardJob = 2810,
        [Description("RM_RC_Audit_Action_RunSPOnPremScanLocalNodeJob")]
        RunSPOnPremScanLocalNodeJob = 2811,
        [Description("RM_RC_Audit_Action_RunCollectionJob4SPOnPrem")]
        RunCollectionJob4SPOnPrem = 2812,
        [Description("RM_RC_Audit_Action_ConfigureScanSPOnPremScheduleJob")]
        ConfigureScanLocalNodeSettingsScheduleJob = 2813,
        [Description("RM_RC_Audit_Action_ConfigureSPOnPremApplySettingSchedule")]
        ConfigureSPOnPremApplySettingSchedule = 2814,
        [Description("RM_RC_Audit_Action_GeneralSetting4SPOnPrem")]
        GeneralSetting4SPOnPrem = 2815,
        [Description("RM_RC_Audit_Action_ConfigureDisposalJob4SPOnPrem")]
        ConfigureDisposalJobSchedule4SPOnPrem = 2816,
        [Description("RM_RC_Audit_Action_RunDisposalJob4SPOnPrem")]
        RunSPOnPremDisposalJob = 2817,

        //Data Sync
        [Description("RM_RC_Audit_Action_ConfigureSPOnlineSyncDataSchedule")]
        ConfigureSPOnlineSyncDataSchedule = 2901,
        [Description("RM_RC_Audit_Action_ConfigureEXOSyncDataSchedule")]
        ConfigureEXOSyncDataSchedule = 2902,
        [Description("RM_RC_Audit_Action_ConfigureFSSyncDataSchedule")]
        ConfigureFSSyncDataSchedule = 2903,
        [Description("RM_RC_Audit_Action_ConfigureSPOnPremSyncDataSchedule")]
        ConfigureSPOnPremSyncDataSchedule = 2904,
        [Description("RM_RC_Audit_Action_ConfigureOneDriveSyncDataSchedule")]
        ConfigureOneDriveSyncDataSchedule = 2905,
        [Description("RM_RC_Audit_Action_ConfigureAzureFileShareSyncDataSchedule")]
        ConfigureAzureFileShareDataSyncSchedule = 2906,
        [Description("RM_RC_Audit_Action_ConfigureBoxSyncDataSchedule")]
        ConfigureBoxDataSyncSchedule = 2907,

        //One Drive 
        [Description("RM_BCM_Audit_Action_ConfigureTermSettings4OneDrive")]
        EditOneDriveTermSetting = 2920,
        #region Obsolete Action
        [Obsolete]
        [Description("RM_RC_Audit_Action_UseBCSColumn")]
        EnableRecordsManagementOneDrive = 2921,
        #endregion
        [Description("RM_BCM_Audit_Action_ConfigureRecordOwnerSettings4OneDrive")]
        EditOneDriveLocationOwnersSetting = 2922,
        [Description("RM_RC_Audit_Action_ConfigureInheritSetting4OneDrive")]
        EditOneDriveInheritSetting = 2923,
        #region Obsolete Action
        [Obsolete]
        [Description("RM_RC_Audit_Action_EnableIsSync")]
        EnableIsSyncOneDrive = 2924,
        [Obsolete]
        [Description("RM_RC_Audit_Action_EnableIsShowUniqueId")]
        EnableOneDriveUniqueIdSetting = 2925,
        #endregion
        [Description("RM_RC_Audit_Action_GeneralSetting4OneDrive")]
        GeneralSetting4OneDrive = 2926,
        [Description("RM_RC_Audit_Action_ConfigureDisposalJob4OneDrive")]
        ConfigureDisposalJobSchedule4OneDrive = 2927,
        [Description("RM_RC_Audit_Action_RunDisposalJob4OneDrive")]
        RunOneDriveDisposalJob = 2928,
        [Description("RM_RC_Audit_Action_RunCollectionJob4OneDrive")]
        RunCollectionJob4OneDrive = 2929,
        [Description("RM_RC_Audit_Action_ConfigureArchiverSetting4OneDrive")]
        EditArchiverSetting4OneDrive = 2930,
        [Description("RM_RC_Audit_Action_ArchiverGeneralSetting4OneDrive")]
        ArchiverGeneralSetting4OneDrive = 2931,
        [Description("RM_RC_Audit_Action_ArchiverConfigureInheritSetting4OneDrive")]
        ArchiverInheritSetting4OneDrive = 2932,
        [Description("RM_RC_Audit_Action_ConfigureArchiverDisposalJob4SPO")]
        ConfigureArchiverDisposalJobSchedule4SPO = 2933,
        [Description("RM_RC_Audit_Action_ConfigureArchiverDisposalJob4OneDrive")]
        ConfigureArchiverDisposalJobSchedule4OneDrive = 2934,
        #endregion


        #region Report Center (3000 ~ 3999)
        //Content Due for Disposal Report (3000 ~ 3099)
        [Description("RM_RC_Audit_Action_CreateTermUsageProfile")]
        CreateTermUsageProfile = 3000,
        [Description("RM_RC_Audit_Action_EditTermUsageProfile")]
        EditTermUsageProfile = 3001,
        [Description("RM_RC_Audit_Action_DeleteTermUsageProfile")]
        DeleteTermUsageProfile = 3002,
        [Description("RM_RC_Audit_Action_GenerateContentDueDisposalReport")]
        GenerateContentDueDisposalReport = 3003,
        [Description("RM_RC_Audit_Action_ExportContentDueDisposalReport")]
        ExportContentDueDisposalReport = 3004,
        //Orphan Term Report
        [Description("RM_RC_Audit_Action_CreateOrphanTermProfile")]
        CreateOrphanTermProfile = 3005,
        [Description("RM_RC_Audit_Action_EditOrphanTermProfile")]
        EditOrphanTermProfile = 3006,
        [Description("RM_RC_Audit_Action_DeleteOrphanTermProfile")]
        DeleteOrphanTermProfile = 3007,
        [Description("RM_RC_Audit_Action_GenerateOrphanTermReport")]
        GenerateOrphanTermReport = 3008,
        [Description("RM_RC_Audit_Action_ExportOrphanTermReport")]
        ExportOrphanTermReport = 3009,
        //Retired Term Report
        [Description("RM_RC_Audit_Action_CreateRetiredTermProfile")]
        CreateRetiredTermProfile = 3010,
        [Description("RM_RC_Audit_Action_EditRetiredTermProfile")]
        EditRetiredTermProfile = 3011,
        [Description("RM_RC_Audit_Action_DeleteRetiredTermProfile")]
        DeleteRetiredTermProfile = 3012,
        [Description("RM_RC_Audit_Action_GenerateRetiredTermReport")]
        GenerateRetiredTermReport = 3013,
        [Description("RM_RC_Audit_Action_ExportRetiredTermReport")]
        ExportRetiredTermReport = 3014,
        [Description("RM_RC_Audit_Action_DashboardCollectionDataJob")]
        DashboardCollectionDataJob = 3015,

        [Description("RM_RC_Audit_Action_ManualApprovalSettingTimer")]
        ManualApprovalSettingTimer = 3016,

        [Description("RM_RC_Audit_Action_ManualApprovalConfigSetting")]
        ManualApprovalConfigSetting = 3017,
        [Description("RM_RC_Audit_Action_ManualApprovalConfigSetting")]
        ManualApprovalSetting = 3018,

        //BCS Term Usage Report (3100 ~ 3199)
        [Description("RM_RC_Audit_Action_CreateDueForDisposalProfile")]
        CreateDueForDisposalProfile = 3100,
        [Description("RM_RC_Audit_Action_EditDueForDisposalProfile")]
        EditDueForDisposalProfile = 3101,
        [Description("RM_RC_Audit_Action_DeleteDueForDisposalProfile")]
        DeleteDueForDisposalProfile = 3102,
        [Description("RM_RC_Audit_Action_GenerateBCSTermUsageReport")]
        GenerateBCSTermUsageReport = 3103,
        [Description("RM_RC_Audit_Action_ExportBCSTermUsageReport")]
        ExportBCSTermUsageReport = 3104,

        CreateProfile = 3105,
        EditProfile = 3106,
        GenerateReport = 3107,
        ExportReport = 3108,
        DeleteProfile = 3109,

        [Description("RM_RC_Audit_Action_CreateCreationAndDestructionReportProfile")]
        CreateCreationAndDestructionReport = 3110,
        [Description("RM_RC_Audit_Action_EditCreationAndDestructionReportProfile")]
        EditCreationAndDestructionReport = 3111,
        [Description("RM_RC_Audit_Action_DeleteCreationAndDestructionReportProfile")]
        DeleteCreationAndDestructionReport = 3112,
        [Description("RM_RC_Audit_Action_GenerateCreationAndDestructionReport")]
        GenerateCreationAndDestructionReport = 3113,
        [Description("RM_RC_Audit_Action_ExportCreationAndDestructionReport")]
        ExportCreationAndDestructionReport = 3114,


        [Description("RM_RC_Audit_Action_CreateAvailableSpaceReportProfile")]
        CreateAvailableSpaceReportProfile = 3115,
        [Description("RM_RC_Audit_Action_EditAvailableSpaceReportProfile")]
        EditAvailableSpaceReportProfile = 3116,
        [Description("RM_RC_Audit_Action_DeleteAvailableSpaceReportProfile")]
        DeleteAvailableSpaceReportProfile = 3117,
        [Description("RM_RC_Audit_Action_GenerateAvailableSpaceReport")]
        GenerateAvailableSpaceReport = 3118,
        [Description("RM_RC_Audit_Action_ExportAvailableSpaceReport")]
        ExportAvailableSpaceReport = 3119,
        [Description("RM_RC_Audit_Action_GenerateExportSiteMetricsReport")]
        GenerateExportSiteMetricsReport = 3120,

        //Rule Usage Report (3200 ~ 3299)
        [Description("RM_RC_Audit_Action_ExportRuleUsageReport")]
        ExportRuleUsageReport = 3200,
        [Description("RM_RC_Audit_Action_RuleUsageSearch")]
        RuleUsageSearch = 3201,

        [Description("RM_RC_Audit_Action_CreateActionAuditReportProfile")]
        CreateActionAuditReport = 3202,
        [Description("RM_RC_Audit_Action_EditActionAuditReportProfile")]
        EditActionAuditReport = 3203,
        [Description("RM_RC_Audit_Action_DeleteActionAuditReportProfile")]
        DeleteActionAuditReport = 3204,
        [Description("RM_RC_Audit_Action_GenerateActionAuditReport")]
        GenerateActionAuditReport = 3205,
        [Description("RM_RC_Audit_Action_ExportActionAuditReport")]
        ExportActionAuditReport = 3206,

        [Description("RM_RC_Audit_Action_CreateRestoreReportProfile")]
        CreateRestoreReportProfile = 3207,
        [Description("RM_RC_Audit_Action_EditRestoreReportProfile")]
        EditRestoreReportProfile = 3208,
        [Description("RM_RC_Audit_Action_DeleteRestoreReportProfile")]
        DeleteRestoreReportProfile = 3209,
        [Description("RM_RC_Audit_Action_GenerateRestoreReport")]
        GenerateRestoreReport = 3210,
        [Description("RM_RC_Audit_Action_ExportRestoreReport")]
        ExportRestoreReport = 3211,



        [Description("RM_RC_Audit_Action_ExportAuditorReport")]
        ExportAuditorReport = 3300,

        [Description("RM_RC_Audit_Action_ExportReportDetailsJob")]
        ExportReportDetailsJob = 3301,

        [Description("RM_RC_Audit_Action_CreateJobNotificationProfile")]

        CreateJobNotificationProfile = 3401,

        [Description("RM_RC_Audit_Action_EditJobNotificationProfile")]
        EditJobNotificationProfile = 3402,

        [Description("RM_RC_Audit_Action_DeleteJobNotificationProfile")]
        DeleteJobNotificationProfile = 3403,

        #endregion

        #region Job Monitor (4000 ~ 4999)
        //Delete Job (4000 ~ 4099)
        [Description("RM_RC_Audit_Action_DeleteJobs")]
        DeleteJobs = 4000,

        [Description("RM_RC_Audit_Action_UpdateJobMonitorPriority")]
        UpdateJobMonitorPriority = 4001,

        [Description("RM_RC_Audit_Action_DeleteQueues")]
        DeleteQueues = 4010,

        [Description("RM_RC_Audit_Action_UpdateJobQueuePriority")]
        UpdateJobQueuePriority = 4011,

        //Download Job Details (4100 ~ 4199)
        [Description("RM_RC_Audit_Action_DownloadJobDetails")]
        DownloadJobDetails = 4100,

        //Stop Job(4200 ~ 4299)
        [Description("RM_RC_Audit_Action_StopJobs")]
        StopJobs = 4200,

        [Description("RM_RC_Audit_Action_RunDownloadJobDetailsJob")]
        RunDownloadJobDetailsJob = 4300,
        [Description("RM_RC_Audit_Action_ConfigDownloadSetting")]
        ConfigDownloadSettings = 4301,
        #endregion

        #region Physical Record Management(5000~5999)
        [Description("RM_RC_Audit_Action_ConfigLocationSyncSchedule")]
        ConfigureScheduleForLocationTermSynchronization = 5001,
        [Description("RM_RC_Audit_Action_SyncLocationTerm")]
        RunLocationTermSyncJob = 5002,
        [Description("RM_RC_Audit_Action_ConfigRecordSchedule")]
        ConfigureUpdateRecordSchedule = 5003,
        [Description("RM_RC_Audit_Action_UpdateRecord")]
        RunUpdateRecordJob = 5004,
        [Description("RM_RC_Audit_Action_CreateContainer")]
        CreateContainer = 5005,
        [Description("RM_RC_Audit_Action_EditContainer")]
        EditContainer = 5006,
        [Description("RM_RC_Audit_Action_DeleteContainer")]
        DeleteContainer = 5007,
        [Description("RM_RC_Audit_Action_EditContainerDefault")]
        EditContainerDefault = 5008,
        [Description("RM_RC_Audit_Action_PhysicalItemImportReport")]
        PhysicalItemImportReport = 5009,
        [Description("RM_RC_Audit_Action_DownloadTemplate")]
        DownloadTemplate = 5010,
        [Description("RM_RC_Audit_Action_PhysicalBulkUpdateExport")]
        PhysicalBulkUpdateExport = 5011,
        [Description("RM_RC_Audit_Action_PhysicalBulkUpdateImport")]
        PhysicalBulkUpdateImport = 5012,

        [Description("RM_RC_Audit_Action_SaveTemplate")]
        EditTemplate = 5100,
        [Description("RM_RC_Audit_Action_CreateTemplate")]
        CreateTemplate = 5101,
        [Description("RM_RC_Audit_Action_DeleteTemplate")]
        DeleteTemplate = 5102,
        [Description("RM_RC_Audit_Action_CreateBarcodeTemplate")]
        CreateBarcodeTemplate = 5103,
        [Description("RM_RC_Audit_Action_UpdateBarcodeTemplate")]
        UpdateBarcodeTemplate = 5104,

        [Description("RM_RC_Audit_Action_CreateCustomBarcodeTemplate")]
        CreateCustomBarcodeTemplate = 5105,

        [Description("RM_RC_Audit_Action_UpdateCustomBarcodeTemplate")]
        UpdateCustomBarcodeTemplate = 5106,

        [Description("RM_RC_Audit_Action_DeleteCustomBarcodeTemplates")]
        DeleteCustomBarcodeTemplates = 5107,

        [Description("RM_RC_Audit_Action_PreviewCustomBarcodeTemplate")]
        PreviewCustomBarcodeTemplate = 5108,

        [Description("RM_RC_Audit_Action_CreateSuite")]
        CreateSuite = 5110,
        [Description("RM_RC_Audit_Action_UpdateSuite")]
        UpdateSuite = 5111,
        [Description("RM_RC_Audit_Action_DeleteSuite")]
        DeleteSuite = 5112,

        [Description("RM_RC_Audit_Action_SavePhysicalRequest")]
        SavePhysicalRequest = 5200,
        [Description("RM_RC_Audit_Action_UpdatePhysicalRequest")]
        UpdatePhysicalRequest = 5201,
        [Description("RM_RC_Audit_Action_ApprovePhysicalRequest")]
        ApprovePhysicalRequest = 5202,
        [Description("RM_RC_Audit_Action_RejectPhysicalRequest")]
        RejectPhysicalRequest = 5203,
        [Description("RM_RC_Audit_Action_LoanPhysicalRequest")]
        LoanPhysicalRequest = 5204,
        [Description("RM_RC_Audit_Action_CancelRequest")]
        CancelRequest = 5205,
        [Description("RM_RC_Audit_Action_PhyLoanBoxJob")]
        PhyLoanBoxJob = 5206,
        [Description("RM_RC_Audit_Action_PhyReturnBoxJob")]
        PhyReturnBoxJob = 5207,
        [Description("RM_RC_Audit_Action_MovePhysicalRequest")]
        MovePhysicalRequest = 5208,
        [Description("RM_RC_Audit_Action_ApprovePhyMoveDataJob")]
        PhyMoveDataJob = 5209,

        AddOrUpdatePhysicalObject = 5210,
        DeletePhysicalObject = 5120,

        [Description("RM_RC_Audit_Action_SavePhysicalBox")]
        SavePhysicalBox = 5211,
        [Description("RM_RC_Audit_Action_UpdatePhysicalBox")]
        UpdatePhysicalBox = 5212,
        [Description("RM_RC_Audit_Action_DeletePhysicalBox")]
        DeletePhysicalBox = 5213,

        [Description("RM_RC_Audit_Action_SavePhysicalRecord")]
        SavePhysicalRecord = 5215,
        [Description("RM_RC_Audit_Action_UpdatePhysicalRecord")]
        UpdatePhysicalRecord = 5216,
        [Description("RM_RC_Audit_Action_DeletePhysicalRecord")]
        DeletePhysicalRecord = 5217,

        [Description("RM_RC_Audit_Action_SavePhysicalFile")]
        SavePhysicalFile = 5221,
        [Description("RM_RC_Audit_Action_UpdatePhysicalFile")]
        UpdatePhysicalFile = 5222,
        [Description("RM_RC_Audit_Action_DeletePhysicalFile")]
        DeletePhysicalFile = 5223,

        [Description("RM_RC_Audit_Action_CreateLocation")]
        CreateLocation = 5224,
        [Description("RM_RC_Audit_Action_RenameLocation")]
        RenameLocation = 5225,
        [Description("RM_RC_Audit_Action_DeleteLocation")]
        DeleteLocation = 5226,
        //[Description("RM_RC_Audit_Action_DeleteLocationTerm")]
        [Description("RM_RC_Audit_Action_EditLocationSetting")]
        EditLocationSetting = 5227,

        [Description("RM_RC_Audit_Action_SavePhysicalContainer")]
        SavePhysicalContainer = 5228,
        [Description("RM_RC_Audit_Action_UpdatePhysicalContainer")]
        UpdatePhysicalContainer = 5229,
        [Description("RM_RC_Audit_Action_DeletePhysicalContainer")]
        DeletePhysicalContainer = 5230,
        [Description("RM_RC_Audit_Action_PhysicalLocationImport")]
        PhysicalLocationImport = 5231,

        [Description("RM_RC_Audit_Action_PhysicalLoanPickComplete")]
        PhysicalLoanPickComplete = 5232,
        [Description("RM_RC_Audit_Action_PhysicalDestructionPickComplete")]
        PhysicalDestructionPickComplete = 5233,
        [Description("RM_RC_Audit_Action_PhysicalLoanPickCompleteJob")]
        PhysicalLoanPickCompleteJob = 5234,
        [Description("RM_RC_Audit_Action_PhysicalDestructionPickCompleteJob")]
        PhysicalDestructionPickCompleteJob = 5235,
        [Description("RM_RC_Audit_Action_PhysicalLoanPickExportJob")]
        PhysicalLoanPickExportJob = 5236,
        [Description("RM_RC_Audit_Action_PhysicalDestructionPickExportJob")]
        PhysicalDestructionPickExportJob = 5237,
        [Description("RM_RC_Audit_Action_PhysicalReturnHistoryExportJob")]
        PhysicalReturnHistoryExportJob = 5238,
        [Description("RM_RC_Audit_Action_PhysicalMoveListPickExportJob")]
        PhysicalMovePickExportJob = 5238,

        #region Physical UniqueID settings

        [Description("RM_RC_Audit_Action_ToggleGlobalUniqueId")]
        ToggleGlobalUniqueId = 5301,

        [Description("RM_RC_Audit_Action_UpdateGlobalUniqueId")]
        UpdateGlobalUniqueId = 5302,

        #endregion

        #region Phyiscal Permission
        [Description("RM_RC_Audit_Action_SaveLocationPermission")]
        SavelocationPermission = 5310,
        #endregion


        #region Template Management

        [Description("RM_RC_Audit_Action_EditBoxTemplate")]
        EditBoxTemplate = 5400,
        [Description("RM_RC_Audit_Action_CreateBoxTemplate")]
        CreateBoxTemplate = 5401,
        [Description("RM_RC_Audit_Action_DeleteBoxTemplate")]
        DeleteBoxTemplate = 5402,
        [Description("RM_RC_Audit_Action_EditFolderTemplate")]
        EditFolderTemplate = 5403,
        [Description("RM_RC_Audit_Action_CreateFolderTemplate")]
        CreateFolderTemplate = 5404,
        [Description("RM_RC_Audit_Action_DeleteFolderTemplate")]
        DeleteFolderTemplate = 5405,
        [Description("RM_RC_Audit_Action_EditRecordTemplate")]
        EditRecordTemplate = 5406,
        [Description("RM_RC_Audit_Action_CreateRecordTemplate")]
        CreateRecordTemplate = 5407,
        [Description("RM_RC_Audit_Action_DeleteRecordTemplate")]
        DeleteRecordTemplate = 5408,

        [Description("RM_RC_Audit_Action_ImportTemplate")]
        ImportTemplate = 5409,
        #endregion

        #region Barcode setting
        [Description("RM_RC_Audit_Action_UpdateBarcodeStandard")]
        SaveBarcodeStandard = 5500,
        #endregion

        #endregion

        #region Mobile Action 6000~7000
        [Description("RM_RC_Audit_Action_MobileReturn")]
        MobileReturn = 6000,
        [Description("RM_RC_Audit_Action_MobileApprovalLoanRequest")]
        MobileApprovalLoanRequest = 6001,
        [Description("RM_RC_Audit_Action_MobileChangeStatus")]
        MobileChangeStatus = 6002,
        [Description("RM_RC_Audit_Action_MobileMove")]
        MobileMove = 6003,
        #endregion

        #region Google Drive Content Action 7001~8000
        [Description("RM_RC_Audit_Action_GoogleDataSynchronization")]
        GoogleDataSynchronization = 7001,
        [Description("RM_RC_Audit_Action_GoogleApplySettings")]
        GoogleApplySettings = 7002,
        RunGoogleReclassifyJob = 7003,
        [Description("RM_RC_Audit_Action_GeneralSettingGG")]
        SaveGeneralSetting = 7004,
        [Description("RM_RC_Audit_Action_LabelSettingGG")]
        SaveLabelSetting = 7005,
        [Description("RM_BCM_Audit_Action_GoogleRunDisposalAction")]
        RunGoogleDisposalJob = 7006,
        [Description("RM_RC_Audit_Action_ConfigureDisposalJobGG")]
        ConfigureGoogleDisposalJobSchedule = 7007,
        [Description("RM_RC_Audit_Action_ConfigureInheritSettingGG")]
        EditInheritSettingGoogle = 7008,
        [Description("RM_RC_Audit_Action_ConfigureGoogleApplySettingSchedule")]
        ConfigureGoogleApplySettingSchedule = 7009,
        [Description("RM_RC_Audit_Action_ConfigureGoogleSyncDataSchedule")]
        ConfigureGoogleDataSyncSchedule = 7010,
        #endregion

        #region FS Register 8000~9000
        [Description("RM_RC_Audit_Action_CreateFSGroup")]
        CreateFSGroup = 8001,

        [Description("RM_RC_Audit_Action_CreateFSConnection")]
        CreateFSConnection = 8002,

        [Description("RM_RC_Audit_Action_FSConnectionValidationTest")]
        FSConnectionValidationTest = 8003,

        [Description("RM_RC_Audit_Action_DeleteFSGroup")]
        DeleteFSGroup = 8004,

        [Description("RM_RC_Audit_Action_DeleteFSConnection")]
        DeleteFSConnection = 8005,

        [Description("RM_RC_Audit_Action_FSConnectionCorrelateGroup")]
        FSConnectionCorrelateGroup = 8006,

        [Description("RM_RC_Audit_Action_EditFSConnection")]
        EditFSConnection = 8007,

        [Description("RM_RC_Audit_Action_EditFSGroup")]
        EditFSGroup = 8008,

        [Description("RM_RC_Audit_Action_FSImportSetting")]
        ImportFSSetting = 8009,

        [Description("RM_RC_Audit_Action_FSClassificationSetting")]
        FSClassificationSetting = 8010,

        [Description("RM_RC_Audit_Action_FSExportSetting")]
        ExportFSSetting = 8011,

        [Description("RM_FS_EditPermission")]
        PermissionChange = 8012,

        [Description("RM_FS_DownloadRCCReport")]
        GenerateRCCReport = 8013,

        [Description("RM_FS_Audit_Action_DeleteRCCReport")]
        DeleteRCCReport = 8014,

        [Description("RM_FS_Audit_Action_JpmcDownloadRCCReport")]
        DownloadRCCReport = 8015,

        [Description("RM_FS_Audit_Action_DeleteDisposalHistory")]
        DeleteHistoryReport = 8016,

        [Description("RM_FS_Audit_Action_DownloadDisposalHistory")]
        DownloadHistoryReport = 8017,

        #endregion

        #region Box Register (8201 ~ 8400)

        [Description("RM_RC_Audit_Action_BoxCreateGroup")]
        BoxCreateGroup = 8201,

        [Description("RM_RC_Audit_Action_BoxCreateConnection")]
        BoxCreateConnection = 8202,

        [Description("RM_RC_Audit_Action_BoxDeleteGroup")]
        BoxDeleteGroup = 8203,

        [Description("RM_RC_Audit_Action_BoxDeleteConnection")]
        BoxDeleteConnection = 8204,

        [Description("RM_RC_Audit_Action_BoxEditGroup")]
        BoxEditGroup = 8205,

        [Description("RM_RC_Audit_Action_BoxEditConnection")]
        BoxEditConnection = 8206,

        #endregion

        #region Azure File Share Content Repository Management (9001 ~ 9100)
        [Description("RM_BCM_Audit_Action_AzureFileDeactiveSetting")]
        AzureFileDeactiveSetting = 9001,
        [Description("RM_BCM_Audit_Action_AzureFileSaveTermSetting")]
        AzureFileSaveTermSetting = 9002,
        [Description("RM_RC_Audit_Action_AzureFileInheritSetting")]
        AzureFileInheritSetting = 9003,
        [Description("RM_BCM_Audit_Action_AzureFileActiveSetting")]
        AzureFileActiveSetting = 9004,
        [Description("RM_BCM_Audit_Action_AzureFileRunDataSync")]
        AzureFileRunDataSyncJob = 9005,
        #endregion

        #region Azure File Share Register 9100~9200
        [Description("RM_RC_Audit_Action_AzureFileShareCreateGroup")]
        AzureFileShareCreateGroup = 9100,

        [Description("RM_RC_Audit_Action_AzureFileShareCreateConnection")]
        AzureFileShareCreateConnection = 9101,

        [Description("RM_RC_Audit_Action_AzureFileShareDeleteGroup")]
        AzureFileShareDeleteGroup = 9102,

        [Description("RM_RC_Audit_Action_AzureFileShareDeleteConnection")]
        AzureFileShareDeleteConnection = 9103,

        [Description("RM_RC_Audit_Action_AzureFileShareEditGroup")]
        AzureFileShareEditGroup = 9104,

        [Description("RM_RC_Audit_Action_AzureFileShareEditConnection")]
        AzureFileShareEditConnection = 9105,

        #endregion

        #region Customize Connector 9200 ~ 9300

        [Description("RM_RC_Audit_Action_CustomizeConnectorCreate")]
        CustomizeConnectorCreate = 9200,

        [Description("RM_RC_Audit_Action_CustomizeConnectorEdit")]
        CustomizeConnectorEdit = 9201,

        [Description("RM_RC_Audit_Action_CustomizeConnectorDelete")]
        CustomizeConnectorDelete = 9202,

        [Description("RM_BCM_Audit_Action_RunConnectorExplorerTimer")]
        RunConnectorExplorerTimer = 9203,

        #endregion

        #region Machine Learning 9301 ~ 9350
        [Description("RM_BCM_Audit_Action_ML_AddTerms")]
        AddTerms = 9301,

        [Description("RM_BCM_Audit_Action_ML_DeleteTerms")]
        DeleteTerms = 9302,

        [Description("RM_BCM_Audit_Action_ML_SetAutoApply")]
        SetAutoApply = 9303,

        [Description("RM_MA_Reassign")]
        MLReassign = 9304,

        [Description("RM_BCM_Audit_Action_ML_StartTrainingJob")]
        StartTrainingJob = 9305,

        [Description("RM_BCM_Audit_Action_ChangeTermAI")]
        MLChangeTerm = 9306,

        [Description("RM_MA_Approve")]
        MLReviewApprove = 9307,

        [Description("RM_JS_JM_JobType_MachineLearningReviewApprove")]
        MLApproveJob = 9308,

        [Description("RM_JS_JM_JobType_MachineLearningReviewReclassify")]
        MLChangeTermJob = 9309,

        [Description("RM_JS_JM_JobType_MachineLearningExportReportJob")]
        MLExportReportJob = 9310,

        [Description("RM_BCM_Audit_Action_ML_UpdateDescription")]
        UpdateTermDescription = 9311,
        [Description("RM_BCM_Audit_Action_ML_SwitchMode")]
        SwitchMode = 9312,
        [Description("RM_BCM_Audit_Action_ML_AddTrainingFileManual")]
        AddTrainingFileManual = 9313,
        [Description("RM_BCM_Audit_Action_ML_ChangeTrainingScopeOption")]
        ChangeTrainingScopeOption = 9314,
        [Description("RM_BCM_Audit_Action_ML_DeleteTrainingScopeFile")]
        DeleteTrainingScopeFile = 9315,
        #endregion

        #region Settings For Archiver 9400~9500
        [Description("RM_RC_Audit_Action_StorageDeviceCreate")]
        StorageDeviceCreate = 9400,
        [Description("RM_RC_Audit_Action_StorageDeviceUpdate")]
        StorageDeviceUpdate = 9401,
        [Description("RM_RC_Audit_Action_StorageDeviceSetIndexDevice")]
        StorageDeviceSetIndexDevice = 9402,
        [Description("RM_RC_Audit_Action_StorageDeviceDelete")]
        StorageDeviceDelete = 9403,
        [Description("RM_RC_Audit_Action_StubSettingCreate")]
        StubSettingCreate = 9404,
        [Description("RM_RC_Audit_Action_StubSettingEdit")]
        StubSettingUpdate = 9405,
        [Description("RM_RC_Audit_Action_StubSettingDelete")]
        StubSettingDelete = 9406,
        [Description("RM_RC_Audit_Action_ConfigureEndUserRestoreSetting")]
        ConfigureEndUserRestoreSetting = 9407,
        [Description("RM_RC_Audit_Action_ConfigureRetentionScheduleJob")]
        ConfigureRetentionScheduleJob = 9408,
        [Description("RM_RC_Audit_Action_EditArchiverPriceConfig")]
        EditArchiverPriceConfig = 9409,
        [Description("RM_RC_Audit_Action_ConfigureArchiverDeleteRestoredData")]
        ConfigureArchiverDeleteRestoredData = 9410,
        [Description("RM_RC_Audit_Action_ConfigureApprovalProcessScheduleJob")]
        ApprovalProcessConfig = 9411,
        [Description("RM_RC_Audit_Action_ConfigureDedupScheduleJob")]
        ConfigureDedupScheduleJob = 9412,
        #endregion

        #region Archiver Job 9501~9600
        [Description("RM_RC_Audit_Action_RunArchiverBackupJob")]
        RunArchiverBackupJob = 9501,
        [Description("RM_RC_Audit_Action_RunArchiverRestoreJob")]
        RunArchiverRestoreJob = 9502,
        [Description("RM_RC_Audit_Action_RunMoveIndexJob")]
        RunMoveIndexJob = 9503,
        [Description("RM_RC_Audit_Action_RunArchiverRetentionJob")]
        RunArchiverRetentionJob = 9504,
        [Description("RM_RC_Audit_Action_RunVeoMergeJob")]
        RunVeoMergeJob = 9505,
        [Description("RM_RC_Audit_Action_RunArchiverExportJob")]
        RunArchiverExportJob = 9506,
        [Description("RM_RC_Audit_Action_RunSOPreScanJob")]
        RunSOPreScanJob = 9507,
        [Description("RM_JS_JM_JobType_ArchiverFullTextIndex")]
        RunArchiverFullTextIndexJob = 9508,
        [Description("RM_RC_Audit_Action_RunDeleteRestoredDataJob")]
        RunArchiverDeleteRestoredDataJob = 9509,
        [Description("RM_RC_Audit_Action_RunArchiverDedupJob")]
        RunArchiverDedupJob = 9510,
        [Description("RM_RC_Audit_Action_RunArchiverDedupReportJob")]
        RunArchiverDedupReportJob = 9511,
        [Description("RM_RC_Audit_Action_RunExportIndexJob")]
        ExportIndex = 9512,
        [Description("RM_RC_Audit_Action_CopyExportIndexPassword")]
        CopyExportIndexPassword = 9513,
        [Description("RM_RC_Audit_Action_SaveRestoreSiteMapping")]
        SaveRestoreSiteMapping = 9514,
        [Description("RM_RC_Audit_Action_RunDeleteOrphanDatasJob")]
        RunDeleteOrphanDatasJob = 9515,
        [Description("RM_RC_Audit_Action_SimulateRunArchiverRestoreJob")]
        SimulateRunArchiverRestoreJob = 9516,
        [Description("RM_RC_Audit_Action_DeleteRestoreSiteMapping")]
        DeleteRestoreSiteMapping = 9517,
        [Description("RM_RC_Audit_Action_ImportRestoreSiteMapping")]
        ImportRestoreSiteMapping = 9518,
        [Description("RM_RC_Audit_Action_ExportRestoreSiteMapping")]
        ExportRestoreSiteMapping = 9519,
        [Description("RM_RC_Audit_Action_RunODPreScanJob")]
        RunODPreScanJob = 9520,
        [Description("RM_RC_Audit_Action_SaveRestoreSiteWhitelist")]
        SaveRestoreSiteWhitelist = 9521,
        [Description("RM_RC_Audit_Action_DeleteRestoreSiteWhitelist")]
        DeleteRestoreSiteWhitelist = 9522,
        [Description("RM_RC_Audit_Action_ImportRestoreSiteWhitelist")]
        ImportRestoreSiteWhitelist = 9523,
        [Description("RM_RC_Audit_Action_ExportRestoreSiteWhitelist")]
        ExportRestoreSiteWhitelist = 9524,
        [Description("RM_RC_Audit_Action_RunConvertStubJob")]
        RunConvertStubJob = 9525,
        [Description("RM_RC_Audit_Action_RunArchiverInPlaceRestoreJob")]
        RunArchiverInPlaceRestoreJob = 9526,
        [Description("RM_RC_Audit_Action_RunArchiverOutPlaceRestoreJob")]
        RunArchiverOutPlaceRestoreJob = 9527,
        [Description("RM_RC_Audit_Action_RunJobMonitorArchiveJob")]
        RunJobMonitorArchiveJob = 9528,
        [Description("RM_RC_Audit_Action_RunArchiverRestoreGoogleDriveJob")]
        RunArchiverRestoreGoogleDriveJob = 9529,
        [Description("RM_RC_Audit_Action_ImportRestoreSiteBlacklist")]
        ImportRestoreSiteBlacklist = 9530,
        [Description("RM_RC_Audit_Action_ExportRestoreSiteBlacklist")]
        ExportRestoreSiteBlacklist = 9531,
        [Description("RM_RC_Audit_Action_SaveRestoreSiteBlacklist")]
        SaveRestoreSiteBlacklist = 9532,
        [Description("RM_RC_Audit_Action_DeleteRestoreSiteBlacklist")]
        DeleteRestoreSiteBlacklist = 9533,
        [Description("RM_RC_Audit_Action_SwitchFullTextIndexType")]
        SwitchFullTextIndexType = 9534,
        [Description("RM_RC_Audit_Action_RunEndUserArchiverBackupJob")]
        RunEndUserArchiverBackupJob = 9535,
        [Description("RM_JS_JM_JobType_ArchiverByHSMXml")]
        ImportExternalArchivedData = 9536,
        [Description("RM_RC_Audit_Action_RunArchiverToSpoRestoreJob")]
        RunArchiverToSpoRestoreJob = 9537,
        [Description("RM_CP_ConfigArchiveDataWhiteList")]
        SaveCustomRetentionSettings = 9538,
        [Description("RM_RESTORE_PUB_SetRestoreGracePeriodSiteCollectionApi")]
        SetRestoreGracePeriodSiteCollectionApi = 9539,
        [Description("RM_RESTORE_PUB_SetRestoreGracePeriodTeamsGroupApi")]
        SetRestoreGracePeriodTeamsGroupApi = 9540,
        [Description("RM_RC_Audit_Action_RunStubArchiverRestoreJob")]
        RunStubArchiverRestoreJob = 9541,
        [Description("RM_RC_Audit_Action_RunM365ArchiverRestoreJob")]
        RunM365ArchiverRestoreJob = 9542,
        #endregion

        #region Box Content Repository Management 9700~9800

        [Description("RM_BCM_Audit_Action_BoxDeactiveSetting")]
        BoxDeactiveSetting = 9701,
        [Description("RM_BCM_Audit_Action_BoxSaveTermSetting")]
        BoxSaveTermSetting = 9702,
        [Description("RM_RC_Audit_Action_BoxInheritSetting")]
        BoxInheritSetting = 9703,
        [Description("RM_BCM_Audit_Action_BoxActiveSetting")]
        BoxActiveSetting = 9704,
        [Description("RM_BCM_Audit_Action_BoxRunDataSync")]
        BoxRunDataSyncJob = 9705,
        [Description("RM_BCM_Audit_Action_BoxRunDisposalAction")]
        BoxRunDisposalJob = 9706,
        [Description("RM_RC_Audit_Action_ConfigureBoxDisposalJob")]
        ConfigureBoxDisposalJobSchedule = 9707,

        #endregion

        #region Discovery (10000~11000)

        [Description("RM_RC_Audit_Action_SaveDiscoveryConfig")]
        SaveDiscoveryConfiguration = 10000,

        [Description("RM_RC_Audit_Action_SaveCostSavingInfo")]
        SaveCostSavingInfo = 10001,

        [Description("RM_RC_Audit_Action_SaveOptimizationDataSetting")]
        SaveOptimizationDataSetting = 10002,

        [Description("RM_RC_Audit_Action_CancelPlanOptimizableJob")]
        CancelPlanOptimizableJob = 10003,

        [Description("RM_FA_Discovery_Rescan_Btn")]
        DiscoveryRescanFailedSite = 10004,

        [Description("RM_FA_Discovery_NewlyPanel_AppendOpt")]
        DiscoveryAppend = 10005,

        [Description("RM_RC_Audit_Action_AddInactiveProfileInfo")]
        AddInactiveProfileInfo = 10006,

        [Description("RM_RC_Audit_Action_UpdateInactiveProfileInfo")]
        UpdateInactiveProfileInfo = 10007,

        [Description("RM_RC_Audit_Action_DeleteInactiveProfileInfo")]
        DeleteInactiveProfileInfo = 10008,

        [Description("RM_RC_Audit_Action_AddRotProfileInfo")]
        AddRotProfileInfo = 10009,

        [Description("RM_RC_Audit_Action_UpdateRotProfileInfo")]
        UpdateRotProfileInfo = 10010,

        [Description("RM_RC_Audit_Action_DeleteRotProfileInfo")]
        DeleteRotProfileInfo = 10011,
        [Description("RM_RC_Audit_Action_SaveOptimizationDataPreScanSetting")]
        SaveOptimizationDataPreScanSetting = 10012,

        [Description("RM_RC_Audit_Action_ExportO365Profile")]
        ExportO365Profile = 10015,

        [Description("RM_RC_Audit_Action_ExportO365RowData")]
        ExportO365RowData = 10016,

        [Description("RM_RC_Audit_Action_ExportDiscoveryO365DuplicationReport")]
        ExportDiscoveryDuplicationReport = 10017,
        [Description("RM_RC_Audit_Action_DiscoveryO365CleanUpDuplicateDatas")]
        DiscoveryCleanUpDuplicateDatas = 10018,
        [Description("RM_RC_Audit_Action_SharePointSiteMetricsReport")]
        SharePointSiteMetricsReport = 10019,

        [Description("RM_RC_Audit_Action_AddSCToDiscoveryM365ExcludeSCList")]
        AddSCToDiscoveryM365ExcludeSCList = 10020,
        [Description("RM_RC_Audit_Action_RemoveSCFromDiscoveryM365ExcludeSCList")]
        RemoveSCFromDiscoveryM365ExcludeSCList = 10021,
        [Description("RM_RC_Audit_Action_ExportDiscoveryM365ExcludeSCList")]
        ExportDiscoveryM365ExcludeSCList = 10022,
        [Description("RM_RC_Audit_Action_ImportDiscoveryM365ExcludeSCList")]
        ImportDiscoveryM365ExcludeSCList = 10023,

        [Description("RM_RC_Audit_Action_CreateDiscoveryPlanProfile")]
        CreateDiscoveryPlanProfile = 10024,

        [Description("RM_RC_Audit_Action_UpdateDiscoveryPlanProfile")]
        UpdateDiscoveryPlanProfile = 10025,

        [Description("RM_RC_Audit_Action_DeleteDiscoveryPlanProfile")]
        DeleteDiscoveryPlanProfile = 10026,
        [Description("RM_RC_Audit_Action_SaveDiscoveryPlanDalJobConfig")]
        SaveDiscoveryPlanDalJobConfiguration = 10027,
        #endregion

        #region Label (11001 ~ 20000)
        [Description("RM_RC_Audit_Action_CreateLabel")]
        CreateLabel = 11001,
        [Description("RM_RC_Audit_Action_DeleteLabel")]
        DeleteLabel = 11002,
        DeleteLabelRuleInfos = 11003,
        [Description("RM_RC_Audit_Action_RenameLabel")]
        RenameLabel = 11004,
        [Description("RM_RC_Audit_Action_SyncLabelFromGoogle")]
        SyncLabelFromGoogle = 11007,
        [Description("RM_RC_Audit_Action_SyncLabelToGoogle")]
        SyncLabelToGoogle = 11008,
        #endregion  

        #region Teams (20001 ~ 21000)

        [Description("RM_RC_Audit_Action_ConfigureArchiverSetting4Teams")]
        EditArchiverSetting4Teams = 20001,
        [Description("RM_RC_Audit_Action_ArchiverGeneralSetting4Teams")]
        ArchiverGeneralSetting4Teams = 20002,
        [Description("RM_RC_Audit_Action_InheritSubNodeToCurrent4Teams")]
        InheritSubNodeToCurrent4Teams = 20003,
        [Description("RM_RC_Audit_Action_ArchiverConfigureInheritSetting4Teams")]
        ArchiverInheritSetting4Teams = 20004,
        [Description("RM_RC_Audit_Action_ConfigureInheritSetting4Teams")]
        EditTeamsInheritSetting = 20005,
        [Description("RM_RC_Audit_Action_ConfigureDisposalJob4Teams")]
        ConfigureDisposalJobSchedule4Teams = 20006,
        [Description("RM_RC_Audit_Action_ConfigureArchiverDisposalJob4Teams")]
        ConfigureArchiverDisposalJobSchedule4Teams = 20007,
        [Description("RM_RC_Audit_Action_ConfigureTeamsSyncDataSchedule")]
        ConfigureTeamsSyncDataSchedule = 20008,
        [Description("RM_BCM_Audit_Action_ConfigureColumnSettings4Teams")]
        EditTeamsColumnSetting = 20009,
        [Description("RM_BCM_Audit_Action_ConfigureRecordOwnerSettings4Teams")]
        EditTeamsLocationOwnersSetting = 20010,
        [Description("RM_BCM_Audit_Action_ConfigureDocumentLevelTermSettings4Teams")]
        EditTeamsDocLevelSetting = 20011,
        [Description("RM_RC_Audit_Action_GeneralSetting4Teams")]
        GeneralSetting4Teams = 20012,
        [Description("RM_RC_Audit_Action_RunDisposalJob4Teams")]
        RunTeamsDisposalJob = 20013,
        [Description("RM_BCM_Audit_Action_ConfigureContainerLevelTermSettings4Teams")]
        EditTeamsConLevelSetting = 20014,
        [Description("RM_RC_Audit_Action_RunCollectionJob4Teams")]
        RunCollectionJob4Teams = 20015,
        [Description("RM_RC_Audit_Action_ConfigTeamsSchedule")]
        ConfigureTeamsSettingsSchedule = 20016,
        [Description("RM_RC_Audit_Action_TeamsApplySetting")]
        TeamsApplySetting = 20017,
        [Description("RM_RC_Audit_Action_RunTeamsArchiverRestoreJob")]
        RunTeamsArchiverRestoreJob = 20018,
        [Description("RM_RC_Audit_Action_RunTeamsUniqueIdSettingJob")]
        RunTeamsUniqueIDSettingJob = 20019,
        [Description("RM_RC_Audit_Action_Teams_ImportSetting")]
        ImportTeamsSetting = 20020,
        [Description("RM_RC_Audit_Action_Teams_ExportSetting")]
        ExportTeamsSetting = 20021,
        [Description("RM_RC_Audit_Action_RunTeamsPreScanJob")]
        RunTeamsPreScanJob = 20022,
        [Description("RM_RC_Audit_Action_RunTeamsUpgradeJob")]
        RunTeamsUpgradeJob = 20023,
        [Description("RM_RC_Audit_Action_RunTeamsConflictJob")]
        RunTeamsConflictJob = 20024,
        [Description("RM_RC_Audit_Action_RunExportSettingConflictJob")]
        RunExportSettingConflictJob = 20025,
        [Description("RM_RC_Audit_Action_RunTeamsDataUpdate")]
        RunTeamsDataUpdateJob = 20026,
        [Description("RM_RC_Audit_Action_TeamsSO_ExportSetting")]
        ExportTeamsSOSetting = 20027,
        [Description("RM_RC_Audit_Action_RunTeamsArchiverInPlaceRestoreJob")]
        RunTeamsArchiverInPlaceRestoreJob = 20028,
        [Description("RM_RC_Audit_Action_RunTeamsArchiverOutPlaceRestoreJob")]
        RunTeamsArchiverOutPlaceRestoreJob = 20029,
        [Description("RM_RC_Audit_Action_RunTeamsMailboxArchiverOutPlaceRestoreJob")]
        RunTeamsMailboxArchiverOutPlaceRestoreJob = 20030,
        #endregion

        #region Others (90000 ~ )
        //Log In
        [Description("RM_RC_Audit_Action_LogIn")]
        LogIn = 90000,
        //Log Out
        [Description("RM_RC_Audit_Action_LogOut")]
        LogOut = 90001,

        [Description("RM_RC_Audit_Action_SyncRemoteNode")]
        SyncRemoteNode = 90002,
        [Description("RM_HS_Criteria_View_Btn_Save")]
        SaveSearchCriteria = 90010,

        [Description("RM_HS_Criteria_View_Btn_Create")]
        CreateSearchCriteria = 90011,

        [Description("RM_HS_Criteria_View_Btn_Delete_View")]
        DeleteSearchCriteria = 90012,

        [Description("RM_HS_ShareProfiles")]
        ShareSearchCriteria = 90013,
        [Description("RM_HS_ShareProfiles")]
        CancelShareSearchCriteria = 90014,

        [Description("RM_HS_Criteria_View_Btn_SaveAsDefaultView")]
        SetSearchCriteriaAsDefault = 90015,

        [Description("RM_RC_Audit_Action_RunOfflineSearch")]
        RunOfflineSearch = 90016,
        [Description("RM_RC_Audit_Action_LogIn")]
        SSOLogIn = 90017,
        [Description("RM_RC_Audit_Action_RunDeclaredRecordsMigrationJob")]
        RunDeclaredRecordsMigrationJob = 90018,
        [Description("RM_RC_Audit_Action_RunStubDisposalJob")]
        RunStubDisposalJob = 90019,
        [Description("RM_RC_Audit_Action_ConfigureStubDisposalSchedule")]
        ConfigureStubDisposalSchedule = 90020,
        [Description("RM_RC_Audit_Action_RunDeleteArchivedSiteCollectionJob")]
        RunDeleteArchivedSiteCollectionJob = 90021,
        [Description("RM_RC_Audit_Action_EnableMultiGeoFeature")]
        EnableMultiGeoFeature = 90022,
        [Description("RM_RC_Audit_Action_SaveMultiGeoIPConfig")]
        SaveMultiGeoIPConfig = 90023,
        [Description("RM_RC_Audit_Action_RunMainDCSyncCommonDataJob")]
        RunMainDCSyncCommonDataJob = 90024,
        [Description("RM_RC_Audit_Action_RunOtherCSyncCommonDataJob")]
        RunOtherDCSyncCommonDataJob = 90025,
        #endregion
    }

    public enum AuditModule
    {
        [Description("RM_RC_Audit_Unknown")]
        Unknown = 0,

        [Description("RM_RC_Audit_Module_RetentionAndDisposalManagement")]
        RetentionAndDisposalManagement = 1,

        [Description("RM_RC_Audit_Module_ControlPanel")]
        ControlPanel = 4,

        [Description("RM_RC_Audit_Module_BusinessClassificationManagement")]
        BusinessClassificationManagement = 5,

        [Description("RM_RC_Audit_Module_ReportCenter")]
        ReportCenter = 6,

        [Description("RM_RC_Audit_Module_JobMonitor")]
        JobMonitor = 7,

        [Description("RM_RC_Audit_Module_PRM")]
        PhysicalRecordManagement = 8,

        [Description("RM_RC_Audit_Module_Login")]
        Login = 9,

        [Description("RM_RC_Audit_Module_Mobile")]
        Mobile = 10,

        [Description("RM_DC_Audit_Module_DC")]
        DownloadCenter = 11,

        [Description("RM_RC_Audit_Module_Connector")]
        CustomizeConnector = 12,

        [Description("RM_RC_Audit_Module_MachineLearning")]
        MachineLearning = 13,

        [Description("RM_RC_Audit_Module_RestoreCenter")]
        RestoreCenter = 14,

        [Description("RM_RC_Audit_Module_Discovery")]
        Discovery = 15,

        [Description("RM_RC_Audit_Module_MultiGeo")]
        MultiGeo = 16,

        GoogleDrive = 20,

        [Description("RM_RC_Audit_Module_Others")]
        Others = 99
    }

    public enum AuditCategory
    {
        [Description("RM_RC_Audit_Unknown")]
        Unknown = 0,
        [Description("RM_RC_Audit_Category_RuleManagement")]
        RuleManagement = 1,
        [Description("RM_RC_Audit_Category_GeneralSettings")]
        GeneralSettings = 2,
        [Description("RM_RC_Audit_Category_Globalsettings")]
        Globalsettings = 3,
        [Description("RM_RC_Audit_Category_DocAveConnection")]
        DocAveConnection = 4,
        [Description("RM_RC_Audit_Category_TermManagement")]
        TermManagement = 5,
        [Description("RM_RC_Audit_Category_TermSynchronization")]
        TermSynchronization = 6,
        [Description("RM_RC_Audit_Category_TermUsageReport")]
        TermUsageReport = 7,
        [Description("RM_RC_Audit_Category_ContentDueForDisposal")]
        ContentDueForDisposal = 8,
        [Description("RM_RC_Audit_Category_RuleUsageReport")]
        RuleUsageReport = 9,
        [Description("RM_RC_Audit_Category_SharePointSettings")]
        SharePointSettings = 10,
        [Description("RM_RC_Audit_Category_DeleteJob")]
        JobMonitor = 11,
        [Description("RM_RC_Audit_Category_Queue_DeleteJob")]
        JobQueueJobDelete = 32,

        //Please use "JobMonitor", this category is deprecated
        [Description("RM_RC_Audit_Category_StopJob")]
        JobMonitorJobStop = 31,

        //Others
        [Description("RM_RC_Audit_Category_LogIn")]
        LogIn = 12,
        [Description("RM_RC_Audit_Category_LogOut")]
        LogOut = 13,

        [Description("RM_RC_Audit_Category_AuthenticationManagement")]
        AuthenticationManagement = 14,
        [Description("RM_RC_Audit_Category_AccountManagement")]
        AccountManagement = 15,

        [Description("RM_RC_Audit_Category_DisposalActivityManagement")]
        DisposalActivityManagement = 16,

        //Please use "JobMonitor", this category is deprecated
        [Description("RM_RC_Audit_Category_JobMonitorDownloadJobDetails")]
        JobMonitorDownloadJobDetails = 17,

        [Description("RM_RC_Audit_Category_AuditorReport")]
        AuditorReport = 18,

        //Please use "SharePointSettings", this category is deprecated
        [Description("RM_RC_Audit_Category_SharePointSettingSchedule")]
        SharePointSettingsSchedule = 19,

        [Description("RM_RC_Audit_Category_LocationSync")]
        LocationTermSynchronisation = 20,

        [Description("RM_RC_Audit_Category_UpdateRecord")]
        UpdateRecordLocation = 21,

        [Description("RM_RC_Audit_Category_ContainerManagement")]
        ContainerManagement = 22,

        [Description("RM_RC_Audit_Category_CreationAndDestructionReport")]
        CreationAndDestructionReport = 23,

        [Description("RM_RC_Audit_Category_AvailableSpaceReport")]
        AvailableSpaceReport = 24,

        [Description("RM_RC_Audit_Category_LocationManagement")]
        LocationManagement = 25,

        [Description("RM_RC_Audit_Category_PhysicalItemImport")]
        PhysicalItemImport = 26,
        [Description("RM_RC_Audit_Category_ManualApproval")]
        ManualApproval = 27,
        [Description("RM_RC_Audit_Category_ManualApprovalTimer")]
        ManualApprovalTimer = 28,

        [Description("RM_RC_Audit_Category_ExportSettings")]
        ExportSettings = 30,
        [Description("RM_JS_EL_ExportSettings")]
        DownloadSettings = 31,
        [Description("RM_RC_Audit_Category_DashBoardSettings")]
        DashBoardSettings = 33,

        [Description("RM_RC_Audit_Category_DashboardCollectionDataJob")]
        DashboardCollectionDataJob = 34,

        [Description("RM_RC_Audit_Category_RecordsExplorer")]
        Explorer = 35,

        [Description("RM_RC_Audit_Category_TimerJobSettings")]
        TimerJobSettings = 36,

        [Description("RM_RC_Audit_Category_TemplateManagement")]
        TemplateManagement = 37,

        [Description("RM_RC_Audit_Category_PhyscialRequest")]
        PhyscialRequestManagement = 38,

        [Description("RM_RC_Audit_Category_PhyscialRecordsExplorer")]
        PhysicalRecordsExplorer = 39,

        [Description("RM_RC_Audit_Category_EmailTemplateManagement")]
        EmailTemplateManagement = 40,
        [Description("RM_RC_Audit_Category_Mobile")]
        Mobile = 41,

        [Description("RM_RDM_WorkFlowManagement")]
        WorkflowManagement = 42,

        [Description("RM_RC_Audit_Category_AgentManagement")]
        AgentManagement = 43,

        [Description("RM_RC_Audit_Category_RemoteNode")]
        RemoteNode = 44,

        //Please use "TimerJobSettings", this category is deprecated
        [Description("RM_RC_Audit_Category_SPOnPremLocalNode")]
        SPOnPremLocalNode = 45,

        [Description("RM_RC_Audit_Category_ReportCenter")]
        ReportCenter = 46,

        [Description("RM_DC_Audit_Category_DownloadCenter")]
        DownloadCenter = 47,

        [Description("RM_RC_Audit_Category_CSDConfigApiKey")]
        CSDConfigApiKey = 48,

        [Description("RM_RC_Audit_Category_Connector")]
        CustomizeConnector = 49,

        [Description("RM_RC_Audit_Category_MachineLearning")]
        MachineLearning = 50,

        [Description("RM_RC_Audit_Category_StubSettings")]
        StubSetting = 51,

        [Description("RM_RC_Audit_Category_StorageSettings")]
        StorageDeviceSettings = 52,

        [Description("RM_RC_Audit_Category_EndUserRestoreSetting")]
        EndUserRestoreSetting = 53,

        [Description("RM_DC_Audit_Category_RestoreCenter")]
        RestoreCenter = 54,

        [Description("RM_DC_Audit_Category_DiscoveryConfig")]
        DiscoveryConfiguration = 55,

        [Description("RM_JS_Phy_LoanPickExport")]
        LoanPickList = 56,

        [Description("RM_JS_Phy_DestructionPickExport")]
        DestructionPickList = 57,
        [Description("RM_AR_Report_ExportArchiverSite")]
        ArchivedSites = 58,

        [Description("RM_FA_Inactive")]
        InactiveData = 59,

        [Description("RM_FA_ROT")]
        ROTData = 60,

        [Description("RM_AR_CP_JobNotification")]
        JobNotification = 61,

        [Description("RM_DC_Audit_Module_DiscoveryDataOptimization")]
        DiscoveryDataOptimization = 62,
        [Description("RM_JS_Phy_ReturnHistoryExport")]
        ReturnHistoryExport = 63,

        [Description("RM_RC_Audit_AI_Generate")]
        AIRecommendation = 64,

        [Description("RM_JS_RDM_Hold_ManageHoldTitle")]
        ManageHold = 65,

        [Description("RM_RC_Audit_Category_ApprovalProcesses")]
        ApprovalProcesses = 66,
        [Description("RM_RC_Audit_Category_PhysicalRecordsGlobalSearch")]
        PhysicalRecordsGlobalSearch = 67,

        [Description("RM_RC_Audit_Category_MultiGeo")]
        MultiGeo = 68,

        [Description("RM_RC_Audit_Located_FS_Myhub")]
        FSMyhub = 69,

        [Description("RM_BCM_Audit_Category_PhyExplorerRecordsMove")]
        PhysicalExplorerMoveRequest = 70,

        [Description("RM_JS_Phy_MovePickExport")]
        MovePickListExport = 71,

        [Description("RM_RC_Audit_Category_DiscoveryPlanProfile")]
        DiscoveryPlanProfile = 72,
    }
}
