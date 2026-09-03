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



using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

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
        [EnumMember]
        TestRunJob = 8,


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
        CMModifyMoveDefaultSettings = 51,
        [EnumMember]
        CMModifyCopyDefaultSettings = 52,
        [EnumMember]
        CMPlanManagerViewDetails = 53,
        [EnumMember]
        CMCreatePlanAndRunJob = 54,

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
        ItemCreateBackupPlan = 70,
        [EnumMember]
        ItemUpdateBackupPlan = 71,
        [EnumMember]
        ItemDeleteBackupPlan = 72,
        [EnumMember]
        ItemCreateRestorePlan = 73,

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
        ReplicatorCreatePlanAndRunNow = 108,

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

        //Control Panel
        #region Control Panel
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
        CreateBatchRemoteSiteCollection = 355,
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
        #endregion

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
        CADelete = 630,
        [EnumMember]
        CAHandlePlan = 631,
        [EnumMember]
        CACreatePlan = 632,
        [EnumMember]
        CAUpdatePlan = 633,
        [EnumMember]
        CADeletePlan = 634,

        [EnumMember]
        CAHandleProfile = 641,
        [EnumMember]
        CACreateProfile = 642,
        [EnumMember]
        CAUpdateProfile = 643,
        [EnumMember]
        CADeleteProfile = 644,



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

        //Granular Backup And Restore
        [EnumMember]
        ExchangeOnlineBackupDefaultSettings = 830,
        [EnumMember]
        ExchangeOnlineSaveScheduleScheme = 831,
        [EnumMember]
        ExchangeOnlineDeleteScheduleScheme = 832,
        [EnumMember]
        ExchangeOnlineUpdateScheduleScheme = 833,
        [EnumMember]
        ExchangeOnlineCreateBackupPlan = 834,
        [EnumMember]
        ExchangeOnlineCreateRestorePlan = 835,
        [EnumMember]
        ExchangeOnlineUpdateBackupPlan = 836,
        [EnumMember]
        ExchangeOnlineDeleteBackupPlan = 837,


        #region CP Online
        [EnumMember]
        CreateSharePointSite = 900,
        [EnumMember]
        UpdateSharePointSite = 901,
        [EnumMember]
        DeleteSharePointSite = 902,
        [EnumMember]
        CreateSharePointGroup = 903,
        [EnumMember]
        UpdateSharePointGroup = 904,
        [EnumMember]
        DeleteSharePointGroup = 905,
        [EnumMember]
        CreateOneDriveSite = 906,
        [EnumMember]
        UpdateOneDriveSite = 907,
        [EnumMember]
        DeleteOneDriveSite = 908,
        [EnumMember]
        CreateOneDriveGroup = 909,
        [EnumMember]
        UpdateOneDriveGroup = 910,
        [EnumMember]
        DeleteOneDriveGroup = 911,
        [EnumMember]
        CreateMailBox = 912,
        [EnumMember]
        UpdateMailBox = 913,
        [EnumMember]
        DeleteMailBox = 914,
        [EnumMember]
        CreateMailBoxGroup = 915,
        [EnumMember]
        UpdateMailBoxGroup = 916,
        [EnumMember]
        DeleteMailBoxGroup = 917,
        [EnumMember]
        CreateOffice365Account = 918,
        [EnumMember]
        UpdateOffice365Account = 919,
        [EnumMember]
        DeleteOffice365Account = 920,
        [EnumMember]
        CreateEmailNotification = 921,
        [EnumMember]
        UpdateEmailNotification = 922,
        [EnumMember]
        SetDefaultEmailNotification = 923,
        [EnumMember]
        DeleteEmailNotification = 924,
        [EnumMember]
        CreateSecurityProfile = 925,
        [EnumMember]
        UpdateSecurityProfile = 926,
        [EnumMember]
        DeleteSecurityProfile = 927,
        [EnumMember]
        ImportSecurityProfile = 928,
        [EnumMember]
        ChangeUserPermission = 929,
        [EnumMember]
        InviteSupportUser = 930,

        [EnumMember]
        DeleteExportReportLocation = 950,
        [EnumMember]
        CreateExportReportLocation = 951,
        [EnumMember]
        UpdateExportReportLocation = 952,
        [EnumMember]
        DeleteLanguageMappings = 954,
        [EnumMember]
        CreateLanguageMapping = 955,
        [EnumMember]
        UpdateLanguageMapping = 956,
        [EnumMember]
        DeleteColumnMappings = 957,
        [EnumMember]
        CreateColunmnMapping = 958,
        [EnumMember]
        UpdateColumnMapping = 959,
        [EnumMember]
        DeleteContentTypeMappings = 960,
        [EnumMember]
        CreateContentTypeMapping = 961,
        [EnumMember]
        UpdateContentTypeMapping = 962,
        #endregion
    }
}