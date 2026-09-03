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
    public enum AveAuditorAction
    {
        [EnumMember]
        DefaultAuditorAction = 100000000,
        //Common-Control Panel
        [EnumMember]
        CPCreateContainerSharePointObjects = 001001001,
        [EnumMember]
        CPChangeUserPermission = 001001002,
        //[EnumMember]
        //CPChangeObjectPermission = 001002002,
        [EnumMember]
        CPCreateNotificationProfile = 001003001,
        [EnumMember]
        CPEditNotificationProfile = 001003002,
        [EnumMember]
        CPDeleteNotificationProfile = 001003003,
        [EnumMember]
        CPSetAsDefaultNotificationProfile = 001003004,
        [EnumMember]
        CPAddSiteCollection = 001004001,
        [EnumMember]
        CPRemoveSiteCollection = 001004003,
        [EnumMember]
        CPReconnectSiteCollection = 001004005,
        [EnumMember]
        CPChangeGroupSiteCollection = 001004032,
        [EnumMember]
        CPAddSitesGroup = 001005001,
        [EnumMember]
        CPEditSitesGroup = 001005002,
        [EnumMember]
        CPDeleteSitesGroup = 001005003,
        [EnumMember]
        CPAddOneDriveForBusiness = 001006001,
        [EnumMember]
        CPRemoveOneDriveForBusiness = 001006003,
        [EnumMember]
        CPReconnectOneDriveForBusiness = 001006005,
        [EnumMember]
        CPChangeGroupOneDriveForBusiness = 001006032,
        [EnumMember]
        CPAddOneDriveForBusinessGroup = 001007001,
        [EnumMember]
        CPEditOneDriveForBusinessGroup = 001007002,
        [EnumMember]
        CPDeleteOneDriveForBusinessGroup = 001007003,
        [EnumMember]
        CPAddExchangeOnlineMailbox = 001008001,
        [EnumMember]
        CPRemoveExchangeOnlineMailbox = 001008003,
        [EnumMember]
        CPReconnectExchangeOnlineMailbox = 001008005,
        [EnumMember]
        CPChangeGroupExchangeOnlineMailbox = 001008032,
        [EnumMember]
        CPAddMailBoxesGroup = 001009001,
        [EnumMember]
        CPEditMailBoxesGroup = 001009002,
        [EnumMember]
        CPDeleteMailBoxGroup = 001009003,
        [EnumMember]
        CPCreate0365AuthenticationProfiles = 001010001,
        [EnumMember]
        CPEdit0365AuthenticationProfiles = 001010002,
        [EnumMember]
        CPDelete0365AuthenticationProfiles = 001010003,
        [EnumMember]
        CPCreateSecurityProfile = 001011001,
        [EnumMember]
        CPEditSecurityProfile = 001011002,
        [EnumMember]
        CPDeleteSecurityProfile = 001011003,
        [EnumMember]
        CPImportSecurityProfile = 001011006,
        [EnumMember]
        CPCreatePhysicalDevice = 001012001,
        [EnumMember]
        CPEditPhysicalDevice = 001012002,
        [EnumMember]
        CPDeletePhysicalDevice = 001012003,
        [EnumMember]
        CPCreateLogicalDevice = 001013001,
        [EnumMember]
        CPEditLogicalDevice = 001013002,
        [EnumMember]
        CPDeleteLogicalDevice = 001013003,
        [EnumMember]
        CPCreateStoragePolicy = 001014001,
        [EnumMember]
        CPEditStoragePolicy = 001014002,
        [EnumMember]
        CPDeleteStoragePolicy = 001014003,
        [EnumMember]
        CPCreateFilterPolicy = 001015001,
        [EnumMember]
        CPEditFilterPolicy = 001015002,
        [EnumMember]
        CPDeleteFilterPolicy = 001015003,
        [EnumMember]
        CPCreateUserMapping = 001016001,
        [EnumMember]
        CPEditUserMapping = 001016002,
        [EnumMember]
        CPDeleteUserMapping = 001016003,
        [EnumMember]
        CPImportUserMapping = 001016006,
        [EnumMember]
        CPCreateLanguageMapping = 001017001,
        [EnumMember]
        CPEditLanguageMapping = 001017002,
        [EnumMember]
        CPDeleteLanguageMapping = 001017003,
        [EnumMember]
        CPImportLanguageMapping = 001017006,
        [EnumMember]
        CPCreateColumnMapping = 001018001,
        [EnumMember]
        CPEditColumnMapping = 001018002,
        [EnumMember]
        CPDeleteColumnMapping = 001018003,
        [EnumMember]
        CPImportColumnMapping = 001018006,
        [EnumMember]
        CPCreateContentTypeMapping = 001019001,
        [EnumMember]
        CPEditContentTypeMapping = 001019002,
        [EnumMember]
        CPDeleteContentTypeMapping = 001019003,
        [EnumMember]
        CPImportContentTypeMapping = 001019006,
        [EnumMember]
        CPCreateExportLocation = 001020001,
        [EnumMember]
        CPEditExportLocation = 001020002,
        [EnumMember]
        CPDeleteExportLocation = 001020003,
        [EnumMember]
        CPCreateReportCenterDatabase = 001024001,
        [EnumMember]
        CPEditReportCenterDatabase = 001024002,
        [EnumMember]
        CPDeleteReportCenterDatabase = 001024003,
        [EnumMember]
        CPCreatePermissionLevel = 001025001,
        [EnumMember]
        CPEditPermissionLevel = 001025002,
        [EnumMember]
        CPCreateEmailTemplate = 001026001,
        [EnumMember]
        CPEditEmailTemplate = 001026002,
        [EnumMember]
        CPDeleteEmailTemplate = 001026003,
        [EnumMember]
        CPConfigureDefaultEmailTemplate = 001026004,
        [EnumMember]
        CPRemoveOffice365GroupMailbox = 001027003,
        [EnumMember]
        CPDeleteOffice365GroupMailboxGroup = 001028003,
        [EnumMember]
        CPRemoveOffice365GroupTeamSite = 001029003,
        [EnumMember]
        CPDeleteOffice365GroupTeamSiteGroup = 001030003,
        [EnumMember]
        CPConfigureSuperUser = 001031001,
        //Common- JobMonitor
        [EnumMember]
        JMDeleteJobRecord = 002001003,
        [EnumMember]
        JMDeleteBackupDataJobRecord = 002001019,
        [EnumMember]
        JMDeleteDataJobRecord = 002001020,
        [EnumMember]
        JMRollbackJobRecord = 002001021,
        [EnumMember]
        JMStopJobRecord = 002001029,
        [EnumMember]
        JMEnableScheduleJobRecord = 002002022,
        [EnumMember]
        JMDisableScheduleJobRecord = 002002023,
        [EnumMember]
        JMDeleteScheduleJobRecord = 002002003,
        [EnumMember]
        JMPromoteJobQueueRecord = 002003024,
        [EnumMember]
        JMDeleteJobQueueRecord = 002003003,

        //Common- PlanGroup
        [EnumMember]
        PGCreatePlanGroup = 003001001,
        [EnumMember]
        PGEditPlanGroup = 003001002,
        [EnumMember]
        PGDeletePlanGroup = 003001003,
        [EnumMember]
        PGRunNowPlanGroup = 003001009,
        [EnumMember]
        PGSaveAndRunNow = 003001010,

        //Common- Control Panel
        [EnumMember]
        CPEditMySettings = 001021002,
        [EnumMember]
        CPInviteSupport = 001022007,
        [EnumMember]
        CPSubmitFeedback = 001023008,


        //Granular Backup And Restore
        [EnumMember]
        GBEditDefaultSettings = 004001002,
        [EnumMember]
        GBCreateBackupPlan = 004002001,
        [EnumMember]
        GBEditBackupPlan = 004002002,
        [EnumMember]
        GBDeleteBackupPlan = 004002003,
        [EnumMember]
        GBRunNowBackupPlan = 004002009,
        [EnumMember]
        GBSaveAndRunNowBackupPlan = 004002010,
        [EnumMember]
        GBTestRunBackupPlan = 004002028,
        [EnumMember]
        GBCreateRestoreJob = 004005001,
        [EnumMember]
        GBSaveAsBackupPlan = 004002050,
        [EnumMember]
        GBRunInstanceJob = 004004009,
        [EnumMember]
        GBCreatePredefinedScheme = 004003001,
        [EnumMember]
        GBEditPredefinedScheme = 004003002,
        [EnumMember]
        GBDeletePredefinedScheme = 004003003,
        [EnumMember]
        GRRunRestoreJob = 005001009,

        //Exchange Online Backup and Restore
        [EnumMember]
        EBEditDefaultSettings = 006001002,
        [EnumMember]
        EBCreateFilterPolicy = 006002001,
        [EnumMember]
        EBEditFilterPolicy = 006002002,
        [EnumMember]
        EBDeleteFilterPolicy = 006002003,
        [EnumMember]
        EBCreateBackupPlan = 006003001,
        [EnumMember]
        EBEditBackupPlan = 006003002,
        [EnumMember]
        EBDeleteBackupPlan = 006003003,
        [EnumMember]
        EBRunNowBackupPlan = 006003009,
        [EnumMember]
        EBSaveAndRunNowBackupPlan = 006003010,
        [EnumMember]
        EBCreatePredefinedScheme = 006004001,
        [EnumMember]
        EBEditPredefinedScheme = 006004002,
        [EnumMember]
        EBDeletePredefinedScheme = 006004003,
        [EnumMember]
        EBRunInstanceJob = 006005009,
        [EnumMember]
        ERRunRestoreJob = 007001009,
        [EnumMember]
        ERCreateRestoreJob = 007001001,
        [EnumMember]
        EBSaveAsBackupPlan = 007001002,

        //Administrator
        [EnumMember]
        ADCreateContainerSharePointObjects = 008001001,
        [EnumMember]
        ADDeleteSharePointObjects = 008001003,
        [EnumMember]
        ADCreatePlan = 008002001,
        [EnumMember]
        ADEditPlan = 008002002,
        [EnumMember]
        ADDeletePlan = 008002003,
        [EnumMember]
        ADRunNowAdminSearchPlan = 008002009,
        [EnumMember]
        ADFinishAndRunNowAdminSearchPlan = 008002010,
        [EnumMember]
        ADActivateOrDeactivateSiteCollectionFeatures = 008003002,
        [EnumMember]
        ADEditProtalSiteCollection = 008004002,
        [EnumMember]
        ADEditContentTypePublishing = 008005002,
        [EnumMember]
        ADEditWebPart = 008006002,
        [EnumMember]
        ADDeleteWebPart = 008006003,
        [EnumMember]
        ADResetWebPart = 008006018,
        [EnumMember]
        ADCloseWebPart = 008006027,
        [EnumMember]
        ADEditThemes = 008007002,
        [EnumMember]
        ADDeleteThemes = 008007003,
        [EnumMember]
        ADEditSolutions = 008008002,
        [EnumMember]
        ADDeleteSolutions = 008008003,
        //[EnumMember]
        //ADCreateSearchWebPartPlan = 008009001,
        //[EnumMember]
        //ADEditSearchWebPartPlan = 008009002,
        //[EnumMember]
        //ADRunNowSearchWebPartPlan = 008009009,
        //[EnumMember]
        //ADFinishAndRunNowSearchWebPartPlan = 008009010,
        [EnumMember]
        ADEditDeploySiteMaximumDepth = 008010002,
        [EnumMember]
        ADEditRSS = 008011002,
        [EnumMember]
        ADEditHelpSetting = 008012002,
        [EnumMember]
        ADEditSharePointDesignerSettings = 008013002,
        [EnumMember]
        ADAddUsers = 008014001,
        [EnumMember]
        ADDeleteUserFromSiteCollection = 008014003,
        [EnumMember]
        ADDeleteUserFromGroup = 008014014,
        [EnumMember]
        ADCreateGroup = 008015001,
        [EnumMember]
        ADEditGroupSettings = 008015002,
        [EnumMember]
        ADDeleteGroupFromSiteCollection = 008015003,
        [EnumMember]
        ADGrantPermissions = 008016001,
        [EnumMember]
        ADEditUserPermissions = 008016002,
        [EnumMember]
        ADRemoveUserPermission = 008016003,
        [EnumMember]
        ADAddPermissionLevel = 008017001,
        [EnumMember]
        ADEditPermissionLevel = 008017002,
        [EnumMember]
        ADDeletePermissionLevel = 008017003,
        [EnumMember]
        ADEditSiteCollectionAdministrator = 008018002,
        [EnumMember]
        ADRunNowOfflineExportReport = 008048009,
        //[EnumMember]
        //ADCreateSecuritySearchPlan = 008019001,
        //[EnumMember]
        //ADEditSecuritySearchPlan = 008019002,
        //[EnumMember]
        //ADRunNowSecuritySearchPlan = 008019009,
        //[EnumMember]
        //ADFinishAndRunNowSecuritySearchPlan = 008019010,
        [EnumMember]
        ADRunNowCloneUserPermissions = 008020009,
        [EnumMember]
        ADRunNowGrantTemporaryPermission = 008021009,
        [EnumMember]
        ADRunNowSearchTemporaryPermission = 008022009,
        [EnumMember]
        ADRunNowImportConfigurationFile = 008025002,
        [EnumMember]
        ADCreatePEProfile = 008026001,
        [EnumMember]
        ADEditPEProfile = 008026002,
        [EnumMember]
        ADDeletePEProfile = 008026003,
        [EnumMember]
        ADApplyPEProfile = 008026012,
        [EnumMember]
        ADRunNowPEProfile = 008026009,
        [EnumMember]
        ADApplyAndRunNowPEProfile = 008026015,
        [EnumMember]
        ADRemoveProfileFromSCPEProfile = 008026016,
        [EnumMember]
        ADFixGeneratedReport = 008027017,
        [EnumMember]
        ADCreateSourceCollectionPolicy = 008028001,
        [EnumMember]
        ADSetAsDefaultSourceCollectionPolicy = 008028004,
        [EnumMember]
        ADEditSourceCollectionPolicy = 008028002,
        [EnumMember]
        ADDeleteSourceCollectionPolicy = 008028003,
        [EnumMember]
        ADActivateOrDeactivateSiteFeatures = 008029002,
        [EnumMember]
        ADResetToSiteDefinition = 008030018,
        [EnumMember]
        ADEditRegionalSettings = 008031002,
        [EnumMember]
        ADEditRSSSettings = 008032002,
        [EnumMember]
        ADEditSearchAndOfflineAvailability = 008033002,
        [EnumMember]
        ADEditTreeView = 008034002,
        [EnumMember]
        ADRunNowBreakInheritanceForSelectedNode = 008035009,
        [EnumMember]
        ADEditMetadataAndKeywordsSettings = 008036002,
        [EnumMember]
        ADEditVersionSettings = 008037002,
        [EnumMember]
        ADEditAdvancedSettings = 008038002,
        [EnumMember]
        ADEditValidationSettings = 008039002,
        [EnumMember]
        ADEditRatingSettings = 008040002,
        [EnumMember]
        ADEditAudienceTargetingSettings = 008041002,
        [EnumMember]
        ADEditMetadataNavigationSettings = 008042002,
        [EnumMember]
        ADEditTitleDescriptionAndNavigation = 008043002,
        [EnumMember]
        ADEditApplyInheritanceToSelectedNode = 008044002,
        [EnumMember]
        ADSaveAdminSearchPlan = 008045001,
        [EnumMember]
        ADEditAdminSearchPlan = 008045002,
        [EnumMember]
        ADDeleteAdminSearchPlan = 008045003,
        [EnumMember]
        ADRunNowSearchWebPartPlan = 008046009,
        [EnumMember]
        ADFinishAndRunNowSearchWebPartPlan = 008046010,
        [EnumMember]
        ADSaveSearchWebPartPlan = 008046001,
        [EnumMember]
        ADEditSearchWebPartPlan = 008046002,
        [EnumMember]
        ADDeleteSearchWebPartPlan = 008046003,
        [EnumMember]
        ADRunNowCheckBrokenLinkPlan = 008047009,
        [EnumMember]
        ADFinishAndRunNowCheckBrokenLinkPlan = 008047010,
        [EnumMember]
        ADSaveCheckBrokenLinkPlan = 008047001,
        [EnumMember]
        ADEditCheckBrokenLinkPlan = 008047002,
        [EnumMember]
        ADDeleteCheckBrokenLinkPlan = 008047003,
        [EnumMember]
        ADEditStorageQuota = 008049002,
        [EnumMember]
        ADEditExternalSharing = 008050002,
        [EnumMember]
        ADEditNavigationElements = 008051002,
        [EnumMember]
        ADRunNowSecuritySearchPlan = 008052009,
        [EnumMember]
        ADFinishAndRunNowSecuritySearchPlan = 008052010,
        [EnumMember]
        ADSaveSecuritySearchPlan = 008052001,
        [EnumMember]
        ADEditSecuritySearchPlan = 008052002,
        [EnumMember]
        ADDeleteSecuritySearchPlan = 008052003,
        [EnumMember]
        ADRunNowBreakInheritanceForSub_nodesPlan = 008053009,
        [EnumMember]
        ADFinishAndRunNowBreakInheritanceForSub_nodesPlan = 008053010,
        [EnumMember]
        ADSaveBreakInheritanceForSub_nodesPlan = 008053001,
        [EnumMember]
        ADEditBreakInheritanceForSub_nodesPlan = 008053002,
        [EnumMember]
        ADDeleteBreakInheritanceForSub_nodesPlan = 008053003,
        [EnumMember]
        ADRunNowPushInheritanceForSub_nodesPlan = 008054009,
        [EnumMember]
        ADFinishAndRunNowPushInheritanceForSub_nodesPlan = 008054010,
        [EnumMember]
        ADSavePushInheritanceForSub_nodesPlan = 008054001,
        [EnumMember]
        ADEditPushInheritanceForSub_nodesPlan = 008054002,
        [EnumMember]
        ADDeletePushInheritanceForSub_nodesPlan = 008054003,
        [EnumMember]
        ADRunNowDeactivatedAccountCleanerPlan = 008055009,
        [EnumMember]
        ADFinishAndRunNowDeactivatedAccountCleanerPlan = 008055010,
        [EnumMember]
        ADSaveDeactivatedAccountCleanerPlan = 008055001,
        [EnumMember]
        ADEditDeactivatedAccountCleanerPlan = 008055002,
        [EnumMember]
        ADDeleteDeactivatedAccountCleanerPlan = 008055003,
        [EnumMember]
        ADCreateDefinedGroup = 008056001,
        [EnumMember]
        ADEditDefinedGroup = 008056002,
        [EnumMember]
        ADDeleteDefinedGroup = 008056003,
        //Content Manager
        [EnumMember]
        CMEditDefaultCopySettings = 009001002,
        [EnumMember]
        CMEditDefaultMoveSettings = 009002002,
        [EnumMember]
        CMCreatePlan = 009003001,
        [EnumMember]
        CMEditPlan = 009003002,
        [EnumMember]
        CMDeletePlan = 009003003,
        [EnumMember]
        CMRunNowPlan = 009003009,
        [EnumMember]
        CMSaveAndRunNowPlan = 009003010,
        [EnumMember]
        CMTestRunPlan = 009003028,
        [EnumMember]
        CMRunNowInstancePlan = 009004009,

        //Deployment Manager
        [EnumMember]
        DPMCreatePlan = 010001001,
        [EnumMember]
        DPMEditPlan = 010001002,
        [EnumMember]
        DPMDeletePlan = 010001003,
        [EnumMember]
        DPMSaveAsPlan = 010001001,
        [EnumMember]
        DPMRunNowPlan = 010001009,
        [EnumMember]
        DPMSaveAndRunNowPlan = 010001010,
        [EnumMember]
        DPMRunNowInstancePlan = 010002009,

        //Replicatior
        [EnumMember]
        RPCreatePlan = 011001001,
        [EnumMember]
        RPEditPlan = 011001002,
        [EnumMember]
        RPDeletePlan = 011001003,
        [EnumMember]
        RPRunNowPlan = 011001009,
        [EnumMember]
        RPSaveAndRunNowPlan = 011001010,
        [EnumMember]
        RPTestRunPlan = 011001028,
        [EnumMember]
        RPCreateMainProfile = 011002001,
        [EnumMember]
        RPEditMainProfile = 011002002,
        [EnumMember]
        RPDeleteMainProfile = 011002003,
        [EnumMember]
        RPSetAsDefaultMainProfile = 011002004,
        [EnumMember]
        RPCreateReplicationOptionsProfile = 011003001,
        [EnumMember]
        RPEditReplicationOptionsProfile = 011003002,
        [EnumMember]
        RPDeleteReplicationOptionsProfile = 011003003,
        [EnumMember]
        RPCreateConflictOptionsProfile = 011004001,
        [EnumMember]
        RPEditConflictOptionsProfile = 011004002,
        [EnumMember]
        RPDeleteConflictOptionsProfile = 011004003,
        [EnumMember]
        RPRunRollbackJob = 011005009,

        //Compliance Report
        [EnumMember]
        CRCreateReportProfile = 012001001,
        [EnumMember]
        CREditReportProfile = 012001002,
        [EnumMember]
        CRDeleteReportProfile = 012001003,
        [EnumMember]
        CRRunNowReportProfile = 012001009,
        [EnumMember]
        CRSaveAndRunNowReportProfile = 012001010,
        [EnumMember]
        CRRunNowInstancePlan = 012002009,
        [EnumMember]
        CRCreatePlan = 012003001,
        [EnumMember]
        CREditPlan = 012003002,
        [EnumMember]
        CRDeletePlan = 012003003,
        [EnumMember]
        CRRetriveDataPlan = 012003011,
        [EnumMember]
        CRApplyRulesPlan = 012003012,
        [EnumMember]
        CROKAndRetriveDataPlan = 012003025,
        [EnumMember]
        CROKAndApplyRulePlan = 012003026,
        [EnumMember]
        CRCreateAuditPrunningProfile = 012004001,
        [EnumMember]
        CREditAuditPrunningProfile = 012004002,
        [EnumMember]
        CRDeleteAuditPrunningProfile = 012004003,
        [EnumMember]
        CRRunNowAuditPrunningProfile = 012004009,
        [EnumMember]
        CROKAndRunNowAuditPrunningProfile = 012004010,
        [EnumMember]
        CRExportToDatasheetReport = 012005013,

        //Adminnistration
        [EnumMember]
        ARCreateReportProfile = 013001001,
        [EnumMember]
        AREditReportProfile = 013001002,
        [EnumMember]
        ARDeleteReportProfile = 013001003,
        [EnumMember]
        ARRunNowReportProfile = 013001009,
        [EnumMember]
        ARSaveAndRunNowReportProfile = 013001010,

        // Archiver
        [EnumMember]
        ARProfileCreate = 014001001, //Create,Copy from
        [EnumMember]
        ARProfileEdit = 014001002,
        [EnumMember]
        ARProfileDelete = 014001003,
        [EnumMember]
        ARProfileApply = 014001012,
        [EnumMember]
        ARProfileApplyAndRunNow = 014001015,
        [EnumMember]
        ARInstancePlanApplyAndRunNow = 014005015,
        [EnumMember]
        ARRuleCreate = 014002001,
        [EnumMember]
        ARRuleEdit = 014002002,
        [EnumMember]
        ARRuleDelete = 014002003,
        [EnumMember]
        ARIndexDeviceEdit = 014003002,
        [EnumMember]
        ARStopInherit = 014004030,
        [EnumMember]
        ARInherit = 014004031,
        [EnumMember]
        ARRestore = 015001009,

        //Cloud App Admin
        [EnumMember]
        CAAADUserCreate = 016001001,
        [EnumMember]
        CAAADUserEdit = 016001002,
        [EnumMember]
        CAAADUserDelete = 016001003,
        [EnumMember]
        CAAADUserResetPassword = 016001033,
        [EnumMember]
        CAAADUserAddtoGroup = 016001034,
        [EnumMember]
        CAAADUserRemoveFromGroup = 016001035,
        [EnumMember]
        CAAADUserAssignLicense = 016001038,
        [EnumMember]
        CAAADUserRemoveLicense = 016001039,
        [EnumMember]
        CAAADUserReplaceLicense = 016001040,
        [EnumMember]
        CAAADUserAddApplication = 016001041,
        [EnumMember]
        CAAADUserRemoveApplication = 016001042,
        [EnumMember]
        CAAADUserReplaceApplication = 016001043,
        [EnumMember]
        CAAADUserAddEmailAccess = 016001044,
        [EnumMember]
        CAAADUserRemoveEmailAccess = 016001045,
        [EnumMember]
        CAAADGroupCreate = 016002001,
        [EnumMember]
        CAAADGroupEdit = 016002002,
        [EnumMember]
        CAAADGroupDelete = 016002003,
        [EnumMember]
        CAAADGroupAddUser = 016002036,
        [EnumMember]
        CAAADGroupRemoveUser = 016002037,
        [EnumMember]
        CAAADGroupAssignLicense = 016002038,
        [EnumMember]
        CAAADGroupRemoveLicense = 016002039,
        [EnumMember]
        CAAADGroupReplaceLicense = 016002040,
        [EnumMember]
        CAAADGroupAddApplication = 016002041,
        [EnumMember]
        CAAADGroupRemoveApplication = 016002042,
        [EnumMember]
        CAAADGroupReplaceApplication = 016002043,
        [EnumMember]
        CAAADGroupAddEmailAccess = 016002044,
        [EnumMember]
        CAAADGroupRemoveEmailAccess = 016002045,
        [EnumMember]
        CAAUserSetCreate = 016003001,
        [EnumMember]
        CAAUserSetDelete = 016003003,
        [EnumMember]
        CAAGroupSetCreate = 016004001,
        [EnumMember]
        CAAGroupSetDelete = 016004003,
        [EnumMember]
        CAAUserFilterProfileCreate = 016005001,
        [EnumMember]
        CAAUserFilterProfileDelete = 016005003,
        [EnumMember]
        CAAGroupFilterProfileCreate = 016006001,
        [EnumMember]
        CAAGroupFilterProfileDelete = 016006003,
        [EnumMember]
        CAAUserPermanentDelete = 016001046,
        [EnumMember]
        CAAUserRestore = 016001047,
        [EnumMember]
        CAATempUserEdit = 016001048,
        [EnumMember]
        CAAO365ProfileCreate = 016007001,
        [EnumMember]
        CAAO365ProfileEdit = 016007002,
        [EnumMember]
        IMUserBatchCreate = 016001049,
        [EnumMember]
        IMGroupBatchCreate = 016002049,
        //RC UsageReport
        [EnumMember]
        URCreatePlan = 017001001,
        [EnumMember]
        UREditPlan = 017001002,
        [EnumMember]
        URDeletePlan = 017001003,
        [EnumMember]
        URExportNowReport = 017001051
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AveAuditStatus
    {
        [EnumMember]
        Undefined = -1,
        [EnumMember]
        Successful = 0,
        [EnumMember]
        Failed = 1,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AuditorStatusInfo
    {
        public AveAuditStatus Status { get; set; }

        public string Comment { get; set; }

        //public int ErrorCode { get; set; }
    }

    public enum CASubAction
    {
        Create = 1,
        Edit = 2,
        Delete = 3,
        SetAsDefault = 4,
        Reconnect = 5,
        Import = 6,
        InviteSupport = 7,
        SubmitFeedbacks = 8,
        RunNow = 9,
        SaveAndRunNow = 10,
        RetriveData = 11,
        ExportToDatasheet = 13,
        ApplyRules = 14,
        ApplyAndRunNow = 15,
        RemoveProfileFromSC = 16,
        Fix = 17,
        Reset = 18,
        Close = 27,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ModuleEnum
    {
        [EnumMember]
        Undefined = 0,
        [EnumMember]
        ControlPanel = 1,
        [EnumMember]
        JobMonitor = 2,
        [EnumMember]
        PlanGroup = 3,
        [EnumMember]
        GranularBackup = 4,
        [EnumMember]
        GranularRestore = 5,
        [EnumMember]
        ExchangeBackup = 6,
        [EnumMember]
        ExchangeRestore = 7,
        [EnumMember]
        Administrator = 8,
        [EnumMember]
        ContentManager = 9,
        [EnumMember]
        DeploymentManager = 10,
        [EnumMember]
        Replicator = 11,
        [EnumMember]
        ComplianceReport = 12,
        [EnumMember]
        AdministratorReprot = 13,
        [EnumMember]
        Archiver = 14,
        [EnumMember]
        ArchiverRestore = 15,
        [EnumMember]
        CloudAppAdministration = 16,
        [EnumMember]
        UsageReport = 17,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AveAuditorObjectType
    {
        [EnumMember]
        Undefined = 0,
        [EnumMember]
        AuditPrunningProfile = 1,
        [EnumMember]
        AdvancedSettings = 2,
        [EnumMember]
        ApplyInheritancetoSelectedNode = 3,
        [EnumMember]
        AudienceTargetingSettings = 4,
        [EnumMember]
        BreakInheritanceForSelectedNode = 5,
        [EnumMember]
        CloneUserPermissions = 6,
        [EnumMember]
        ColumnMapping = 7,
        [EnumMember]
        ConflictOptionsProfile = 8,
        [EnumMember]
        ContentTypePublishing = 9,
        [EnumMember]
        ContentTypeMapping = 10,
        [EnumMember]
        DefaultCopySettings = 11,
        [EnumMember]
        DefaultMoveSettings = 12,
        [EnumMember]
        DefaultSettings = 13,
        [EnumMember]
        DeploySiteMaximumDepth = 14,
        [EnumMember]
        ExchangeOnlineMailbox = 15,
        [EnumMember]
        ExportLocation = 16,
        [EnumMember]
        FilterPolicy = 17,
        [EnumMember]
        GeneratedReport = 18,
        [EnumMember]
        GrantPermissions = 19,
        [EnumMember]
        GrantTemporaryPermission = 20,
        [EnumMember]
        HelpSettings = 21,
        [EnumMember]
        ImportConfigurationFile = 22,
        [EnumMember]
        InstanceJob = 23,
        [EnumMember]
        InviteSupport = 24,
        [EnumMember]
        JobQueueRecord = 25,
        [EnumMember]
        JobRecord = 26,
        [EnumMember]
        LanguageMapping = 27,
        [EnumMember]
        LogicalDevice = 28,
        [EnumMember]
        MailboxesGroup = 29,
        [EnumMember]
        MainProfile = 30,
        [EnumMember]
        MetadataAndKeywordsSettings = 31,
        [EnumMember]
        MetadataNavigationSettings = 32,
        [EnumMember]
        MySettings = 33,
        [EnumMember]
        NotificationProfile = 34,
        [EnumMember]
        O365AuthenticationProfiles = 35,
        [EnumMember]
        ObjectPermission = 36,
        [EnumMember]
        OneDriveForBusiness = 37,
        [EnumMember]
        OneDriveForBusinessGroup = 38,
        [EnumMember]
        PEProfile = 39,
        [EnumMember]
        PhysicalDevice = 40,
        [EnumMember]
        Plan = 41,
        [EnumMember]
        PlanGroup = 42,
        [EnumMember]
        PortalSiteCollection = 43,
        [EnumMember]
        PredefinedScheme = 44,
        [EnumMember]
        PushInheritancetoSubNodes = 45,
        [EnumMember]
        RatingSettings = 46,
        [EnumMember]
        ReplicationOptionsProfile = 47,
        [EnumMember]
        Report = 48,
        [EnumMember]
        ReportProfile = 49,
        [EnumMember]
        ResetToSiteDefinition = 50,
        [EnumMember]
        RestoreJob = 51,
        [EnumMember]
        RegionalSettings = 52,
        [EnumMember]
        RSS = 53,
        [EnumMember]
        RSSSettings = 54,
        [EnumMember]
        ScheduleJobRecord = 55,
        [EnumMember]
        SearchAndOfflineAvailability = 56,
        [EnumMember]
        SearchTemporaryPermission = 57,
        [EnumMember]
        SecurityProfile = 58,
        [EnumMember]
        SharePointDesignerSettings = 59,
        [EnumMember]
        SharePointObjects = 60,
        [EnumMember]
        SiteCollection = 61,
        [EnumMember]
        SiteCollectionFeature = 62,
        [EnumMember]
        SiteFeatures = 63,
        [EnumMember]
        SitesGroup = 64,
        [EnumMember]
        Solutions = 65,
        [EnumMember]
        SourceCollectionPolicy = 66,
        [EnumMember]
        StoragePolicy = 67,
        [EnumMember]
        SubmitFeedback = 68,
        [EnumMember]
        Themes = 69,
        [EnumMember]
        TitleDescriptionAndNavigation = 70,
        [EnumMember]
        TreeView = 71,
        [EnumMember]
        UserMapping = 72,
        [EnumMember]
        UserPermission = 73,
        [EnumMember]
        UsersAndPermissionsPeopleAndGroups = 74,
        [EnumMember]
        UsersAndPermissionsSitePermissions_Permission = 75,
        [EnumMember]
        UsersAndPermissionsSitePermissions_Group = 76,
        [EnumMember]
        UsersAndPermissionsSitePermissions_Permission_Level = 77,
        [EnumMember]
        UsersAndPermissionsSitePermissions_Site_Collection_Admin = 78,
        [EnumMember]
        ValidationSettings = 79,
        [EnumMember]
        VersionSettings = 80,
        [EnumMember]
        WebPart = 81,
        [EnumMember]
        ArchiverProfile = 82,
        [EnumMember]
        ArchiverRule = 83,
        [EnumMember]
        IndexDevice = 84,
        [EnumMember]
        ReportCenterDatabase = 85,
        [EnumMember]
        ADUser = 86,
        [EnumMember]
        ADGroup = 87,
        [EnumMember]
        ADUserSet = 88,
        [EnumMember]
        ADGroupSet = 89,
        [EnumMember]
        ADUserFilterProfile = 90,
        [EnumMember]
        ADGroupFilterProfile = 91,
        [EnumMember]
        ADO365Profile = 92,
        [EnumMember]
        EmailTemplate = 93,
        [EnumMember]
        GroupTeamSite = 94,
        [EnumMember]
        GroupTeamSiteGroups = 95,
        [EnumMember]
        SuperUser = 96,
        [EnumMember]
        GroupMailbox = 97,
        [EnumMember]
        PermissionLevel = 98,
        [EnumMember]
        AdminSearchPlan = 99,
        [EnumMember]
        SearchWebPartPlan = 100,
        [EnumMember]
        CheckBrokenLinkPlan = 101,
        [EnumMember]
        SecuritySearchPlan = 102,
        [EnumMember]
        BreakInheritanceForSub_nodesPlan = 103,
        [EnumMember]
        PushInheritanceForSub_nodesPlan = 104,
        [EnumMember]
        DeactivatedAccountCleanerPlan = 105,
        [EnumMember]
        StorageQuota = 106,
        [EnumMember]
        ExternalSetting = 107,
        [EnumMember]
        NavigationElements = 108,
        [EnumMember]
        DefinedGroup = 109,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AveAuditorActionType
    {
        [EnumMember]
        Undefined = 0,
        [EnumMember]
        Create = 1,
        [EnumMember]
        Edit = 2,
        [EnumMember]
        Delete = 3,
        [EnumMember]
        SetAsDefault = 4,
        [EnumMember]
        Reconnect = 5,
        [EnumMember]
        Import = 6,
        [EnumMember]
        InviteSupport = 7,
        [EnumMember]
        SubmitFeedbacks = 8,
        [EnumMember]
        RunNow = 9,
        [EnumMember]
        SaveAndRunNow = 10,
        [EnumMember]
        RetriveData = 11,
        [EnumMember]
        ApplyRules = 12,
        [EnumMember]
        ExportToDatasheet = 13,
        [EnumMember]
        DeleteUserFromGroup = 14,
        [EnumMember]
        ApplyAndRunNow = 15,
        [EnumMember]
        RemoveProfileFromSC = 16,
        [EnumMember]
        Fix = 17,
        [EnumMember]
        Reset = 18,
        [EnumMember]
        DeleteBackupData = 19,
        [EnumMember]
        DeleteData = 20,
        [EnumMember]
        Rollback = 21,
        [EnumMember]
        Enable = 22,
        [EnumMember]
        Disable = 23,
        [EnumMember]
        Promote = 24,
        [EnumMember]
        OKAndRetriveData = 25,
        [EnumMember]
        OKAndApplyRule = 26,
        [EnumMember]
        Close = 27,
        [EnumMember]
        TestRun = 28,
        [EnumMember]
        Stop = 29,
        [EnumMember]
        BreakInheritance = 30,
        [EnumMember]
        Inherit = 31,
        [EnumMember]
        ChangeGroup = 32,
        [EnumMember]
        ResetPassword = 33,
        [EnumMember]
        AddToGroup = 34,
        [EnumMember]
        RemoveFromGroup = 35,
        [EnumMember]
        AddUser = 36,
        [EnumMember]
        RemoveUser = 37,
        [EnumMember]
        AssignLicense = 38,
        [EnumMember]
        RemoveLicense = 39,
        [EnumMember]
        ReplaceLicense = 40,
        [EnumMember]
        AddApplication = 41,
        [EnumMember]
        RemoveApplication = 42,
        [EnumMember]
        ReplaceApplication = 43,
        [EnumMember]
        AddEmailAccess = 44,
        [EnumMember]
        RemoveEmailAccess = 45,
        [EnumMember]
        PermanentDeleteUser = 46,
        [EnumMember]
        RestoreUser = 47,
        [EnumMember]
        EditTempUser = 48,
        [EnumMember]
        BatchCreate = 49,
        [EnumMember]
        SaveAs = 50,
        [EnumMember]
        ExportNow = 51,

    }
}
