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


using AvePoint.GCommon.Contract.Common;
using System.Runtime.Serialization;

namespace AvePoint.GCommon.Contract.Server.Audit
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AveAction
    {
        [EnumMember]
        Unknown = 0,

        [EnumMember]
        LoginIn = 1,

        [EnumMember]
        LoginOut = 2,

        //plan
        [EnumMember]
        LoadPlan = 3,

        [EnumMember]
        CreatePlan = 4,

        [EnumMember]
        EditPlan = 5,

        [EnumMember]
        DeletePlan = 6,

        //job
        [EnumMember]
        RunJob = 7,

        //Report Center Action
        [EnumMember]
        RCSaveProfile = 11,

        [EnumMember]
        RCUpdateProfile = 12,

        [EnumMember]
        RCDeleteProfile = 13,

        [EnumMember]
        RCGetProfile = 14,

        [EnumMember]
        RCUpdateCollectorSetting = 15,

        [EnumMember]
        RCConfigReportingService = 16,

        [EnumMember]
        RCIISSettings = 17,

        [EnumMember]
        RCRunCollector = 18,

        [EnumMember]
        RCSaveConfig = 19,

        [EnumMember]
        RCRunAdminReport = 20,

        [EnumMember]
        RCRunAuditorRetrieve = 21,

        [EnumMember]
        RCRunAuditorApply = 22,

        [EnumMember]
        RCRunAuditorPruning = 23,

        [EnumMember]
        RCRunAuditorReport = 24,

        [EnumMember]
        RCActivateFeature = 25,

        [EnumMember]
        RCDeactivateFeature = 26,

        [EnumMember]
        RCUpdateAdvancedSettings = 27,

        [EnumMember]
        RCGenerateReport = 28,

        [EnumMember]
        RCExportReport = 29,

        //Central Admin Action
        [EnumMember]
        CAConfiguration = 30,

        [EnumMember]
        CAConfigurationSet = 31,

        [EnumMember]
        CAConfigurationAdd = 32,

        [EnumMember]
        CAConfigurationDelete = 33,

        [EnumMember]
        CAConfigurationRead = 34,

        [EnumMember]
        CAConfigurationTest = 35,

        [EnumMember]
        CAConfigurationClonePermission = 36,

        //Content Manager
        [EnumMember]
        CMModifyDefaultSettings = 50,

        [EnumMember]
        CMDeleteJobAndBackupData = 51,

        //Granular Backup And Restore
        [EnumMember]
        ItemBackupDefaultSettings = 66,

        [EnumMember]
        ItemSaveScheduleScheme = 67,

        [EnumMember]
        ItemDeleteScheduleScheme = 68,

        [EnumMember]
        ItemUpdateScheduleScheme = 69,

        [EnumMember]
        ItemEndUserRestoreSetting = 70,

        [EnumMember]
        ItemEndUserRestoreActive = 71,

        [EnumMember]
        ItemEndUserRestoreDeactive = 72,

        [EnumMember]
        ItemEndUserRestoreAdvanceSetting = 73,

        //Deployment Manager
        [EnumMember]
        DeploymentManagerAddQueue = 80,

        [EnumMember]
        DeploymentManagerEditQueue = 81,

        [EnumMember]
        SolutionManagement = 82,

        [EnumMember]
        DeploymentManagerDeleteQueue = 83,

        [EnumMember]
        DeploymentManagerDownloadPlan = 84,

        [EnumMember]
        DeploymentManagerUploadPlan = 85,

        [EnumMember]
        DeploymentManagerCreateCompareReportPlan = 86,

        [EnumMember]
        DeploymentManagerCreatePattern = 87,

        [EnumMember]
        DeploymentManagerEditPattern = 88,

        [EnumMember]
        DeploymentManagerCreateDeployPatternQueue = 89,

        [EnumMember]
        DeploymentManagerCreateUpdateScopeQueue = 90,

        [EnumMember]
        DeploymentManagerDeletePattern = 91,

        [EnumMember]
        DeploymentManagerDeletePatternVersion = 92,

        //  Replicator
        [EnumMember]
        ReplicatorCreatePlan = 100,

        [EnumMember]
        ReplicatorUpdatePlan = 101,

        [EnumMember]
        ReplicatorDeletePlan = 102,

        [EnumMember]
        ReplicatorRunJob = 103,

        [EnumMember]
        ReplicatorCreateProfile = 104,

        [EnumMember]
        ReplicatorUpdateProfile = 105,

        [EnumMember]
        ReplicatorDeleteProfile = 106,

        [EnumMember]
        ReplicatorSetProfileAsDefault = 107,

        [EnumMember]
        ReplicatorSaveTemplate = 108,

        [EnumMember]
        ReplicatorUpdateTemplate = 109,

        [EnumMember]
        ReplicatorDeleteTemplates = 110,

        //Storage Optimization Action
        [EnumMember]
        CreateProcessingPool = 200,

        [EnumMember]
        DeleteProcessingPools = 201,

        [EnumMember]
        DeleteProcessingPool = 202,

        [EnumMember]
        UpdateProcessingPool = 203,

        [EnumMember]
        CreateStubDatabase = 204,

        [EnumMember]
        SetupBlobProvider = 205,

        [EnumMember]
        RunRBSSetting = 206,

        [EnumMember]
        LoadAllSettings = 207,

        [EnumMember]
        SaveSyncDeletionPlans = 208,

        [EnumMember]
        DeleteRules = 209,

        [EnumMember]
        GetRealtimeRuleInfo = 210,

        [EnumMember]
        GetScheduledRuleInfo = 211,

        [EnumMember]
        GetArchiverRuleInfo = 212,

        [EnumMember]
        GetRulesAndSettings = 213,

        [EnumMember]
        GetRealtimeRulesAndSettings = 214,

        [EnumMember]
        GetScheduledRulesAndSettings = 215,

        [EnumMember]
        GetArchiverRulesAndSettings = 216,

        [EnumMember]
        GetRuleInfoByRule = 217,

        [EnumMember]
        GetRuleInfoByNode = 218,

        [EnumMember]
        ViewPagerStub = 219,

        [EnumMember]
        BrowserFiles = 220,

        [EnumMember]
        DoStubRestore = 221,

        [EnumMember]
        LoadJob = 222,

        [EnumMember]
        CreateRealtimeRule = 223,

        [EnumMember]
        CreateScheduledRule = 224,

        [EnumMember]
        CreateArchiverRule = 225,

        [EnumMember]
        EditRealtimeRule = 226,

        [EnumMember]
        EditScheduledRule = 227,

        [EnumMember]
        EditArchiverRule = 228,

        [EnumMember]
        RemoveRealtimeRules = 229,

        [EnumMember]
        RemoveScheduledRules = 230,

        [EnumMember]
        RemoveArchiverRules = 231,

        [EnumMember]
        RemoveRuleNodes = 232,

        [EnumMember]
        RunArchiverRules = 233,

        [EnumMember]
        InheritRealtimeRule = 234,

        [EnumMember]
        InheritScheduledRule = 235,

        [EnumMember]
        InheritArchiverRule = 236,

        [EnumMember]
        StopInheritRealtimeRule = 237,

        [EnumMember]
        StopInheritScheduledRule = 238,

        [EnumMember]
        StopInheritArchiverRule = 239,

        [EnumMember]
        ConfigIndexDevice = 240,

        [EnumMember]
        GetNodeCalculateSummary = 241,

        [EnumMember]
        SaveRestorePlan = 242,

        [EnumMember]
        ValidateFSDestPathInfo = 243,

        [EnumMember]
        GetSearchTreeResult = 244,

        [EnumMember]
        RunRetentionJob = 245,

        [EnumMember]
        RestartRetentionJob = 246,

        [EnumMember]
        EditScheduledSettings = 247,

        [EnumMember]
        RunScheduledRules = 248,

        [EnumMember]
        EditArchiverSettings = 249,

        [EnumMember]
        ConnectorCreateMapping = 250,

        [EnumMember]
        ConnectorEditMapping = 251,

        [EnumMember]
        ConnectorDeleteMapping = 252,

        [EnumMember]
        ConnectorConfigurationPathInfo = 253,

        [EnumMember]
        ConnectorConfigurationSyncSetting = 254,

        [EnumMember]
        ConnectorRemoveConnectionInfo = 255,

        [EnumMember]
        ConnectorManagerFeatures = 256,

        [EnumMember]
        ConnectorManagerFeaturesActiveFeature = 257,

        [EnumMember]
        ConnectorManagerFeaturesDeActiveFeature = 258,

        [EnumMember]
        ConnectorCreateCommonMapping = 260,

        [EnumMember]
        ConnectorCreatePropertyMapping = 261,

        [EnumMember]
        ConnectorCreateSecurityMapping = 262,

        [EnumMember]
        ConnectorCreateSPPermissionMapping = 263,

        [EnumMember]
        ConnectorEditCommonMapping = 264,

        [EnumMember]
        ConnectorEditPropertyMapping = 265,

        [EnumMember]
        ConnectorEditSecurityMapping = 266,

        [EnumMember]
        ConnectorEditSPPermissionLevel = 267,

        [EnumMember]
        ConnectorDeleteCommonMapping = 268,

        [EnumMember]
        ConnectorDeletePropertyMapping = 269,

        [EnumMember]
        ConnectorDeleteSecurityMapping = 270,

        [EnumMember]
        ConnectorDeleteSPPermissionLevel = 271,

        [EnumMember]
        ArchiverCreateProfile = 272,

        [EnumMember]
        ArchiverEditProfile = 273,

        [EnumMember]
        ArchiverDeleteProfile = 274,

        [EnumMember]
        ExtenderCreateProfile = 275,

        [EnumMember]
        ExtenderEditProfile = 276,

        [EnumMember]
        ExtenderDeleteProfile = 277,

        [EnumMember]
        StorageReportGetPlanJobMapping = 278,

        [EnumMember]
        StorageReportSaveOrUpdateProfile = 279,

        [EnumMember]
        StorageReportValidateDeletingProfiles = 280,

        [EnumMember]
        StorageReportDeleteProfiles = 281,

        [EnumMember]
        StorageReportOKAndRunNow = 282,

        [EnumMember]
        StorageReportRunNow = 283,

        [EnumMember]
        StorageReportHandleSummaryAndDetail = 284,

        [EnumMember]
        StorageReportGetSummary = 285,

        [EnumMember]
        StorageReportGetExportProgress = 286,

        [EnumMember]
        StorageReportGenerateExportData = 287,

        [EnumMember]
        SetShreddedSize = 288,

        [EnumMember]
        DeactiveStubTraceManageFeature = 289,

        [EnumMember]
        ActiveStubTraceManageFeature = 290,

        [EnumMember]
        RunPreRetentionJob = 291,

        [EnumMember]
        DeactiveAlternateFileFeature = 292,

        [EnumMember]
        ActiveAlternateFileFeature = 293,

        [EnumMember]
        ConfigureStorageManagerFeatureSetting = 294,

        [EnumMember]
        ArchiverApprovalCenterExport = 295,
        
        [EnumMember]
        ArchiverApprovalCenterAlert = 296,


        //Control Panel
        [EnumMember]
        ConfigAgent = 300,

        [EnumMember]
        DeleteAgentGroup = 301,

        [EnumMember]
        UpdateAgentGroup = 302,

        [EnumMember]
        CreateAgentGroup = 303,

        [EnumMember]
        RemoveAgents = 304,

        [EnumMember]
        AgentControl = 305,

        [EnumMember]
        AddDomain = 306,

        [EnumMember]
        AddPermissionLevel = 308,

        [EnumMember]
        EditPermissionLevel = 309,

        [EnumMember]
        DeletePermissionLevels = 310,

        [EnumMember]
        AddUsertoGivenGroup = 311,

        [EnumMember]
        EditAccount = 312,

        [EnumMember]
        RemoveUsersFromGroup = 313,

        [EnumMember]
        EnableSpecificUsers = 314,

        [EnumMember]
        DisableSpecificUsers = 315,

        [EnumMember]
        DeleteGroups = 316,

        [EnumMember]
        DeleteAccounts = 317,

        [EnumMember]
        AddGroup = 318,

        [EnumMember]
        EditGroup = 319,

        [EnumMember]
        ChangePermission = 320,

        [EnumMember]
        AddAccount = 322,

        [EnumMember]
        UpdateDomain = 323,

        [EnumMember]
        LogOffSpecificUsers = 325,

        [EnumMember]
        AgentControlForAgentMonitor = 330,

        [EnumMember]
        PruningJob = 331,

        [EnumMember]
        UpdatePruningSettings = 332,

        [EnumMember]
        UpdateRuleDetailDtos = 333,

        [EnumMember]
        DoActionForSolution = 335,

        [EnumMember]
        DoActionForWebApp = 336,

        [EnumMember]
        DeleteAllLanguageLogs = 337,

        [EnumMember]
        DeleteLanguagePackage = 338,

        [EnumMember]
        ChangePassphrsaseChar = 339,

        [EnumMember]
        BackupSecurityInfo = 340,

        [EnumMember]
        SaveTranslationEngine = 341,

        [EnumMember]
        SaveSystemSetting = 342,

        [EnumMember]
        SaveLogoImage = 343,

        [EnumMember]
        ResetWarningByUser = 344,

        [EnumMember]
        UpdateSystemPasswordPolicy = 345,

        [EnumMember]
        UpdateSystemSecurityPolicy = 346,

        [EnumMember]
        SaveUserConfirm = 347,

        [EnumMember]
        RenewLicenseNotificationSettings = 348,

        [EnumMember]
        ApplyLicense = 349,

        [EnumMember]
        SaveCacheSetting = 350,

        [EnumMember]
        ControlServices = 351,

        [EnumMember]
        CreateRemoteSiteCollection = 356,

        [EnumMember]
        UpdateRemoteSiteCollection = 357,

        [EnumMember]
        DeleteRemoteSiteCollection = 358,

        [EnumMember]
        CreateRemoteWebApplication = 359,

        [EnumMember]
        UpdateRemoteWebApplication = 360,

        [EnumMember]
        DeleteRemoteWebApplication = 361,

        [EnumMember]
        SaveNotificationSetting = 362,

        [EnumMember]
        SendTestEmailNotification = 363,

        [EnumMember]
        DeleteNotificationSetting = 364,

        [EnumMember]
        ResetNotificationSettingStatus = 365,

        [EnumMember]
        SaveLogSettings = 366,

        [EnumMember]
        RunNow = 367,

        [EnumMember]
        Install = 368,

        [EnumMember]
        SaveSettings = 369,

        [EnumMember]
        StartInstaller = 370,

        [EnumMember]
        StartPatchControl = 371,

        [EnumMember]
        SaveSettingsAndRemovePatch = 372,

        [EnumMember]
        DeletePatch = 373,

        [EnumMember]
        UnInstallPatch = 374,

        [EnumMember]
        CreateMorePatchDownload = 375,

        [EnumMember]
        DropMorePatchDownload = 376,

        [EnumMember]
        CreatePatchDownload = 377,

        [EnumMember]
        DropPatchDownload = 378,

        [EnumMember]
        PausePatchDownload = 379,

        [EnumMember]
        DoPatchDownload = 380,

        [EnumMember]
        DeleteAgentGroupByDto = 381,

        [EnumMember]
        ChangeAllControlService = 382,

        [EnumMember]
        SaveDataEncryptionProfileBackupInfo = 383,

        [EnumMember]
        SaveDocAveDBEncryptionKeyBackupInfo = 384,

        [EnumMember]
        RetrieveVersion = 385,

        [EnumMember]
        UpdateMessage = 386,

        [EnumMember]
        DeleteFilterPolicy = 387,

        [EnumMember]
        CreateFilterPolicy = 388,

        [EnumMember]
        UpdateFilterPolicy = 389,

        [EnumMember]
        CreateProfile = 390,

        [EnumMember]
        DeleteProfile = 391,

        [EnumMember]
        UpdateProfile = 392,

        [EnumMember]
        DeleteUserMappings = 393,

        [EnumMember]
        CreateUserMapping = 394,

        [EnumMember]
        UpdateUserMapping = 395,

        [EnumMember]
        SaveLanguageMapping = 396,

        [EnumMember]
        DeleteMapping = 397,

        [EnumMember]
        UpdateMapping = 398,

        [EnumMember]
        UpLoadMappingFile = 399,

        [EnumMember]
        CreatePhysicalDevice = 400,

        [EnumMember]
        UpdatePhysicalDevice = 401,

        [EnumMember]
        DeletePhysicalDevice = 402,

        [EnumMember]
        CreateLogicalDevice = 403,

        [EnumMember]
        UpdateLogicalDevice = 404,

        [EnumMember]
        DeleteLogicalDevice = 405,

        [EnumMember]
        CreateStoragePolicy = 406,

        [EnumMember]
        UpdateStoragePolicy = 407,

        [EnumMember]
        DeleteStoragePolicy = 408,

        [EnumMember]
        ChangeLicenseStatus = 409,

        [EnumMember]
        ValidateProxy = 410,

        [EnumMember]
        AgentInstall = 411,

        [EnumMember]
        AgentUnInstall = 412,

        [EnumMember]
        AgentConfigFile = 413,

        [EnumMember]
        CreatePlanGroup = 414,

        [EnumMember]
        UpdatePlanGroup = 415,

        [EnumMember]
        DeletePlanGroup = 416,

        [EnumMember]
        RunPlanGroup = 417,

        [EnumMember]
        AddPlanToGroup = 418,

        [EnumMember]
        RemovePlanOfGroup = 419,

        [EnumMember]
        UpdatePlanOfGroup = 420,

        [EnumMember]
        CreateMapping = 421,

        [EnumMember]
        CreateExportLocation = 422,

        [EnumMember]
        UpdateExportLocation = 423,

        [EnumMember]
        DeleteExportLocation = 424,

        [EnumMember]
        CreateNotificationMessage = 425,

        [EnumMember]
        UpdateNotificationMessage = 426,

        [EnumMember]
        DeleteNotificationMessage = 427,

        [EnumMember]
        UpdateNotificationMessageOnGroup = 428,

        [EnumMember]
        CreateDomainMapping = 450,

        [EnumMember]
        UpdateDomainMapping = 451,

        [EnumMember]
        DeleteDomainMapping = 452,

        [EnumMember]
        UpLoadDomainMapping = 453,

        [EnumMember]
        DeleteUserMapping = 454,

        [EnumMember]
        UpLoadUserMapping = 455,

        [EnumMember]
        CreateGroupMapping = 456,

        [EnumMember]
        UpdateGroupMapping = 457,

        [EnumMember]
        DeleteGroupMapping = 458,

        [EnumMember]
        UpLoadGroupMapping = 459,

        [EnumMember]
        CreateLanguageMapping = 460,

        [EnumMember]
        UpdateLanguageMapping = 461,

        [EnumMember]
        DeleteLanguageMapping = 462,

        [EnumMember]
        UpLoadLanguageMapping = 463,

        [EnumMember]
        CreateTemplateMapping = 464,

        [EnumMember]
        DeleteTemplateMapping = 465,

        [EnumMember]
        UpLoadTemplateMapping = 466,

        [EnumMember]
        CreateColumnMapping = 467,

        [EnumMember]
        UpdateColumnMapping = 468,

        [EnumMember]
        DeleteColumnMapping = 469,

        [EnumMember]
        UpLoadColumnMapping = 470,

        [EnumMember]
        CreateContentTypeMapping = 471,

        [EnumMember]
        UpdateContentTypeMapping = 472,

        [EnumMember]
        DeleteContentTypeMapping = 473,

        [EnumMember]
        UpLoadContentTypeMapping = 474,

        [EnumMember]
        CreateSecurityProfile = 475,

        [EnumMember]
        UpdateSecurityProfile = 476,

        [EnumMember]
        ConfigureEndUserArchiverTree = 477,

        [EnumMember]
        ConfigArchiverDatabase = 478,

        [EnumMember]
        CreateListNameMapping = 479,

        [EnumMember]
        UpdateListNameMapping = 480,

        [EnumMember]
        DeleteListNameMapping = 481,

        [EnumMember]
        UpLoadListNameMapping = 482,

        [EnumMember]
        DeployForSolution = 483,

        [EnumMember]
        InstallForSolution = 484,

        [EnumMember]
        UpgradeForSolution = 485,

        [EnumMember]
        RemoveForSolution = 486,

        [EnumMember]
        RepairForSolution = 487,

        [EnumMember]
        ResetIISForSolution = 488,

        [EnumMember]
        RetractForSolution = 489,

        [EnumMember]
        RetrieveForSolution = 490,

        [EnumMember]
        JobPerformanceAlert = 491,

        [EnumMember]
        JobPerformanceNoAlert = 492,

        [EnumMember]
        DeployForSolutionAndResetIIS = 493,

        [EnumMember]
        CreateAccountProfile = 494,

        [EnumMember]
        UpdateAccountProfile = 495,

        [EnumMember]
        DeleteAccountProfile = 496,

        //Vault
        [EnumMember]
        VaultCreateProcessingPool = 500,

        [EnumMember]
        VaultEditProcessingPool = 501,

        [EnumMember]
        VaultDeleteProcessingPool = 502,

        [EnumMember]
        VaultCreateProfile = 503,

        [EnumMember]
        VaultUpdateProfile = 504,

        [EnumMember]
        VaultDeleteProfile = 505,

        [EnumMember]
        VaultApply = 506,

        [EnumMember]
        VaultRunNow = 507,

        [EnumMember]
        VaultInherit = 508,

        [EnumMember]
        VaultStopInherit = 509,

        [EnumMember]
        VaultRemove = 510,

        [EnumMember]
        VaultRetract = 511,

        [EnumMember]
        VaultCreateRule = 512,

        [EnumMember]
        VaultUpdateRule = 513,

        [EnumMember]
        VaultDeleteRule = 514,

        //Central Admin Detailed Action
        [EnumMember]
        CACreateWebApplication = 600,

        [EnumMember]
        CACreateSiteCollection = 601,

        [EnumMember]
        CACreateSite = 602,

        [EnumMember]
        CACreateListOrLibrary = 603,

        [EnumMember]
        CACreateFolder = 604,

        [EnumMember]
        CAAdminSearch = 610,

        [EnumMember]
        CASecuritySearch = 611,

        [EnumMember]
        CADuplicateFileSearch = 612,

        [EnumMember]
        CAWebPartSearch = 613,

        [EnumMember]
        CACloneUserPermission = 614,

        [EnumMember]
        CACloneSitePermission = 615,

        [EnumMember]
        CAImportConfigurationFile = 616,

        [EnumMember]
        CADeadAccountCleaner = 617,

        [EnumMember]
        CAMoveSiteCollection = 618,

        [EnumMember]
        CADeleteOrphanSite = 619,

        [EnumMember]
        CACheckBrokenLink = 620,

        [EnumMember]
        CABreakInheritance = 621,

        [EnumMember]
        CAPushInheritance = 622,

        [EnumMember]
        CADeleteTempPermission = 623,

        [EnumMember]
        CAChangeMetadateOnline = 624,

        [EnumMember]
        CAOfflineDownloadReport = 625,

        [EnumMember]
        CAChangeMetadateOffline = 626,

        [EnumMember]
        CAAdministratorProfile = 627,

        [EnumMember]
        CAPolicyEnforcerFix = 628,

        [EnumMember]
        CAPolicyEnforcerHide = 629,

        [EnumMember]
        CADelete = 630,

        [EnumMember]
        CACreateHostNameSiteCollection = 631,

        [EnumMember]
        CACreateSiteCollectionOnline = 632,

        //Migration
        [EnumMember]
        MigrationSaveMainProfile = 700,

        [EnumMember]
        MigrationUpdateMainProfile = 701,

        [EnumMember]
        MigrationDeleteMainProfile = 702,

        [EnumMember]
        MigrationSetAsDefaultProfile = 703,

        [EnumMember]
        MigrationSaveConnection = 704,

        [EnumMember]
        MigrationUpdateConnection = 705,

        [EnumMember]
        MigrationSavePlan = 706,

        [EnumMember]
        MigrationUpdatePlan = 707,

        [EnumMember]
        MigrationDeletePlan = 708,

        [EnumMember]
        MigrationJobRun = 709,

        [EnumMember]
        MigrationJobRunTest = 710,

        [EnumMember]
        MigrationDownloadProfile = 711,

        [EnumMember]
        MigrationDeleteConnection = 712,

        [EnumMember]
        MigrationSaveSubProfile = 713,

        [EnumMember]
        MigrationUpdateSubProfile = 714,

        [EnumMember]
        MigrationDeleteSubProfile = 715,

        [EnumMember]
        MigrationAddToPlanGroup = 716,

        [EnumMember]
        MigrationRemoveFromGroup = 717,

        [EnumMember]
        MigrationUploadProfile = 718,

        [EnumMember]
        MigrationSaveExportLocation = 719,

        [EnumMember]
        MigrationUpdateExportLocation = 720,

        [EnumMember]
        MigrationDeleteExportLocation = 721,

        [EnumMember]
        MigrationSaveImprotLocation = 722,

        [EnumMember]
        MigrationUpdateImprotLocation = 723,

        [EnumMember]
        MigrationDeleteImprotLocation = 724,

        [EnumMember]
        MigrationSaveMigrationDb = 725,

        [EnumMember]
        MigrationUpdateMigrationDb = 726,

        [EnumMember]
        MigrationJobRerun = 727,

        [EnumMember]
        MigrationSaveAzureConnection = 728,

        [EnumMember]
        MigrationUpdateAzureConnection = 729,

        // eDiscovery Action
        [EnumMember]
        EDConfigComplianceDatabase = 800,

        [EnumMember]
        EDConfigSearchServiceApplication = 801,

        [EnumMember]
        EDCreateContentSource = 802,

        [EnumMember]
        EDDeleteContentSource = 803,

        [EnumMember]
        EDCreateHold = 804,

        [EnumMember]
        EDDeleteHold = 805,

        [EnumMember]
        EDCreatePlan = 806,

        [EnumMember]
        EDDeletePlan = 807,

        [EnumMember]
        EDApplyLegalHold = 808,

        [EnumMember]
        EDReleaseHold = 809,

        [EnumMember]
        EDExport = 810,

        [EnumMember]
        EDCreateExportLocation = 811,

        [EnumMember]
        EDDeleteExportLocation = 812,

        [EnumMember]
        EDSearch = 813,

        [EnumMember]
        EDCrawl = 814,

        [EnumMember]
        EDSync = 815,

        [EnumMember]
        EDRunPlan = 816,

        [EnumMember]
        CORunSyncNow = 817,

        [EnumMember]
        CORunInventoryReport = 818,

        [EnumMember]
        SaveAgentProxy = 819,

        [EnumMember]
        CreateSystemProfile = 820,

        [EnumMember]
        UpdateSystemProfile = 821,

        [EnumMember]
        DeleteSystemProfile = 822,

        #region HA Actions (850~889)

        [EnumMember]
        HACreateGroup = 850,

        [EnumMember]
        HAViewGroup = 851,

        [EnumMember]
        HAEditGroup = 852,

        [EnumMember]
        HADeleteGroup = 853,

        [EnumMember]
        HARunPrescanJob = 860,

        [EnumMember]
        HARunSyncJob = 861,

        [EnumMember]
        HARunFailoverJob = 862,

        [EnumMember]
        HARunFallbackJob = 863,

        [EnumMember]
        HACreateThrottle = 870,

        [EnumMember]
        HAEditThrottle = 871,

        [EnumMember]
        HADeleteThrottle = 872,

        [EnumMember]
        HACreateScriptProfile = 873,

        [EnumMember]
        HAEditScriptProfile = 874,

        [EnumMember]
        HADeleteScriptProfile = 875,

        [EnumMember]
        HACreateCommandProfile = 876,

        [EnumMember]
        HAEditCommandProfile = 877,

        [EnumMember]
        HADeleteCommandProfile = 878,

        [EnumMember]
        HADealCommand = 879,

        [EnumMember]
        HAEditSQLInstanceSetting = 880,

        [EnumMember]
        HADealCacheSetting = 881,

        [EnumMember]
        HACreateConnectorCacheSetting = 882,

        [EnumMember]
        HAEditConnectorCacheSetting = 883,

        [EnumMember]
        HADeleteConnectorCacheSetting = 884,

        [EnumMember]
        HACreateLogshippingCacheSetting = 885,

        [EnumMember]
        HAEditLogshippingCacheSetting = 886,

        [EnumMember]
        HADeleteLogshippingCacheSetting = 887,

        #endregion HA Actions (850~889)

        [EnumMember]
        DeploymentManagerTestRun = 890,

        [EnumMember]
        DeploymentManagerCompareReport = 891,

        [EnumMember]
        DeploymentManagerPushAppUpdate = 892,

        [EnumMember]
        DeploymentManagerCheckAppUpdate = 893,

        #region SSDM Actions (901~910)
        [EnumMember]
        SSDMDealStagingPolicy = 901,

        [EnumMember]
        SSDMDealFilterPolicy = 902,

        [EnumMember]
        SSDMDealAnalyzeSQLBackup = 903,

        [EnumMember]
        SSDMDealRestore = 904,

        [EnumMember]
        SSDMDealSQLMapping = 905,

        [EnumMember]
        SSDMDealRestoreFromLiveDB = 906,

        [EnumMember]
        SSDMDealAnalyzeVHDBackup = 907,
        #endregion

        #region PR Actions (911~950)
        [EnumMember]
        PRRunMigration = 911,
        [EnumMember]
        PRFarmRebuild = 912,
        [EnumMember]
        PRFarmRepair = 913,
        [EnumMember]
        PRRunMaintenance = 914,
        [EnumMember]
        PRRunRestoreJob = 915,
        [EnumMember]
        PRLoadCustomizedDBNode = 916,
        [EnumMember]
        PRRunFarmRebuildWithVMJob = 917,
        [EnumMember]
        PRRunFarmCloneJob = 918,
        [EnumMember]
        PRRunAlternateLocationJob = 919,
        [EnumMember]
        PRRunEndUserRestoreJob = 920,
        #endregion

        #region VM Actions (951~1000) 
        [EnumMember]
        VMInplaceTimeBasedRestore = 951,
        [EnumMember]
        VMOOPTimeBasedRestore = 952,
        [EnumMember]
        VMInplaceObjectBasedRestore = 953,
        [EnumMember]
        VMOOPObjectBasedRestore = 954,
        [EnumMember]
        VMFileLevelInplaceRestore = 955,
        [EnumMember]
        VMFileLevelOOPRestore = 956,
        [EnumMember]
        VMFileLevelFileSystemOOPRestore = 957,
        #endregion

        #region HealthAnalyzer Action 1001~1100
        [EnumMember]
        HealAnaCreateProfile = 1001,
        [EnumMember]
        HealAnaEditProfile = 1002,
        [EnumMember]
        HealAnaDeleteProfile = 1003,
        [EnumMember]
        HealAnaStopScaning = 1004,
        [EnumMember]
        HealAnaRescan = 1005,
        [EnumMember]
        HealAnaRunJob = 1006,
        [EnumMember]
        HealAnaExportReport = 1007,
        #endregion

        #region DeploymentManager (1101~1200)
        [EnumMember]
        DeploymentManagerCreatePlanAndRun = 1101,

        [EnumMember]
        DeploymentManagerCreatePlanAndTestRun = 1102,

        [EnumMember]
        DeploymentManagerUpdatePlanAndRun = 1103,

        [EnumMember]
        DeploymentManagerUpdatePlanAndTestRun = 1104,
        #endregion
        #region  Host Manager (1201~1210)
        [EnumMember]
        CreateHostProfile = 1201,

        [EnumMember]
        UpdateHostProfile = 1202,

        [EnumMember]
        DeleteHostProfiles = 1203,
        #endregion

        #region  Report Center (1300~1350)
        [EnumMember]
        RCEnableUPAPlan = 1300,

        [EnumMember]
        RCDisableUPAPlan = 1301,

        [EnumMember]
        RCDataPruning = 1302,

        [EnumMember]
        RCDataRestore = 1304,

        [EnumMember]
        RCItemCachingSaveDBInfo = 1305,

        [EnumMember]
        RCItemCachingSaveTreeInfo = 1306,
        #endregion

        [EnumMember]
        ConnectorRemoveSyncSetting = 1351,

        [EnumMember]
        HADealThrottle = 1352,

        [EnumMember]
        ConnectorUNCLinkSetting = 1353,

        [EnumMember]
        RestartServices = 1355,
        [EnumMember]
        DisableServices = 1356,
        [EnumMember]
        EnableServices = 1357,
        [EnumMember]
        DeleteServices = 1358,
        [EnumMember]
        UninstallServices = 1359,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AveAuditStatus
    {
        [EnumMember]
        Successful = 0,

        [EnumMember]
        Failed = 1,
    }
}