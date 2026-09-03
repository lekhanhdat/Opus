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



using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Replicator.Object.ProfileContents;
using AvePoint.GCommon.Contract.Server.ControlPanel.ColumnMapping.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.ContentTypeMapping.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.GroupMapping.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.TemplateMapping.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Connector.Object.Settings;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.CloudAppAdmin.Object;

namespace AvePoint.GCommon.Contract.Server.Common.Profile.Object
{
    #region Known types

    [KnownType(typeof(OnewayPullContent))]
    [KnownType(typeof(ReplicatorNetworkControlContent))]
    [KnownType(typeof(ReplicatorConfigDBContent))]
    [KnownType(typeof(ReplicatorSnapshotContent))]
    [KnownType(typeof(OnlineMappingProfileContent))]
    [KnownType(typeof(OnlineReplicationSubProfileContent))]
    [KnownType(typeof(OnlineConflictionSubProfileContent))]
    [KnownType(typeof(ExportMappingProfileContent))]
    [KnownType(typeof(ExportReplicationSubProfileContent))]
    [KnownType(typeof(ImportMappingProfileContent))]
    [KnownType(typeof(ImportReplicationSubProfileContent))]
    [KnownType(typeof(ImportConflictionSubProfileContent))]
    [KnownType(typeof(FilterPolicyInfo))]
    [KnownType(typeof(ColumnMappingDataContract))]
    [KnownType(typeof(ContentTypeMappingDataContract))]
    [KnownType(typeof(TemplateMappingContract))]
    [KnownType(typeof(AutoScanProfileContent))]
    //--Connector Settings--
    [KnownType(typeof(CommonSetting))]
    [KnownType(typeof(ProperitySetting))]
    [KnownType(typeof(SecuritySetting))]
    [KnownType(typeof(SPPermission))]
    [KnownType(typeof(NotificationDto))]
    [KnownType(typeof(GroupMappingContract))]
    //--

    //--CloudAppAdministration contents--
    [KnownType(typeof(SearchProfileContent))]
    [KnownType(typeof(UserSetProfileContent))]
    [KnownType(typeof(GroupSetProfileContent))]
    [KnownType(typeof(EOCredentialProfileContent))]
    [KnownType(typeof(TempUserProfileContent))]
    [KnownType(typeof(CAAPEProfileContent))]
    //--
    #endregion
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ProfileDto
    {
        [DataMember]
        public string Id { set; get; }

        [DataMember]
        public string Name { set; get; }

        [DataMember]
        public ProfileType Type { set; get; }

        [DataMember]
        public string ModuleName { set; get; }

        [DataMember]
        public string ParentId { get; set; }

        [DataMember]
        public string AgentId { get; set; }

        [DataMember]
        public string Extension { set; get; }

        [DataMember]
        public ObjectInfoDto ObjectInfo { get; set; }

        [DataMember]
        public string AgentGroupId { get; set; }

        [DataMember]
        public FarmDto Farm { get; set; }

        [DataMember]
        public IProfileContent Content { get; set; }

        [DataMember]
        public bool IsDefault { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public List<ObjectPermissionDto> ObjectPermissions { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ProfileType : int
    {
        [EnumMember]
        UnSpecified = 0,

        [EnumMember]
        FilterPolicy = 1,

        //[EnumMember]
        //ContentManagerDevice = 2,

        [EnumMember]
        ExportLocation = 3,

        //[EnumMember]
        //CacheSetting = 4,

        [EnumMember]
        RealTimeRule = 5,

        [EnumMember]
        UserMapping = 6,

        [EnumMember]
        LanguageMapping = 7,

        [EnumMember]
        ColumnMapping = 8,

        [EnumMember]
        ContentTypeMapping = 9,

        [EnumMember]
        TemplateMapping = 10,

        [EnumMember]
        StoragePolicy = 11,

        //[EnumMember]
        //Office365RemoteWebApplication = 12,

        [EnumMember]
        Office365RemoteSitecollection = 13,

        [EnumMember]
        QuickBackupDefaultSetting = 14,

        [EnumMember]
        BackUpScheduleScheme = 15,

        //[EnumMember]
        //PRQuickBackupDefaultSetting = 16,

        [EnumMember]
        SuperUserConfiguration = 17,

        //[EnumMember]
        //PRBackStagingPolicy = 18,

        //[EnumMember]
        //SCOM = 19,

        [EnumMember]
        Notification = 20,

        //[EnumMember]
        //PRBackUpScheduleScheme = 24,

        [EnumMember]
        BackUpScheduleDefaultScheme = 25,

        [EnumMember]
        ConnectorCommonSetting = 26,

        [EnumMember]
        ConnectorSecuritySetting = 27,

        [EnumMember]
        ConnectorPropertiesSetting = 28,

        [EnumMember]
        ConnectorSPPermission = 29,

        //Replicator will use 30--60.
        #region Replicator

        [EnumMember]
        ReplicatorNetworkControl = 30,

        [EnumMember]
        ReplicatorConfigDB = 31,

        [EnumMember]
        ReplicatorOldConfigDB = 32,

        [EnumMember]
        ReplicatorByteLevel = 33,

        [EnumMember]
        ReplicatorOnlineMapping = 34,

        [EnumMember]
        ReplicatorExportMapping = 35,

        [EnumMember]
        ReplicatorImportMapping = 36,

        [EnumMember]
        ReplicatorOnlineReplication = 37,

        [EnumMember]
        ReplicatorExportReplication = 38,

        [EnumMember]
        ReplicatorImportReplication = 39,

        [EnumMember]
        ReplicatorOnlineConfliction = 40,

        [EnumMember]
        ReplicatorImportConfliction = 41,

        [EnumMember]
        ReplicatorOnewayPullSetting = 42,

        [EnumMember]
        ReplicatorReportStorageSetting = 43,
        #endregion

        [EnumMember]
        DomainMapping = 61,

        //[EnumMember]
        //SPPermissionSetting = 62,//Connector Use

        //[EnumMember]
        //LicenseManager = 63,

        [EnumMember]
        ReportExportLocation = 64,

        #region Define For StorageOptimization
        [EnumMember]
        ScheduledRule = 65,

        [EnumMember]
        ArchiverRule = 66,

        [EnumMember]
        StubDatabase = 67,

        [EnumMember]
        BinaryStatus = 68,

        [EnumMember]
        ProcessingPool = 69,

        [EnumMember]
        RealTimeLicenseLock = 70,

        [EnumMember]
        DatabaseConfiguration = 74,

        [EnumMember]
        RecycleDBConfig = 75,

        [EnumMember]
        ArchiverProfile = 71,

        [EnumMember]
        EndUserArchiverSetting = 72,
        [EnumMember]
        ExtenderProfile = 73,
        [EnumMember]
        ArchiverRuleForRevIM = 76,

        [EnumMember]
        ArchiverDatabase = 1074,

        [EnumMember]
        ArchiverRetentionCloud = 77,

        [EnumMember]
        ExchangeArchiverRuleForRevIM = 78,

        [EnumMember]
        AOSPArchiverRuleForRevIM = 79,
        #endregion

        //Migration will use 100--200
        #region Migration
        [EnumMember]
        FileConnection = 100,

        [EnumMember]
        FileMigration = 101,

        [EnumMember]
        FileMigrationOptions = 102,

        [EnumMember]
        FileMigrationFilterOptions = 103,

        [EnumMember]
        FileMigrationMappings = 104,

        [EnumMember]
        FileMigrationPermissionMapping = 105,

        //SP 07To10 Migration
        [EnumMember]
        SP07To10MigrationOnlineMapping = 106,

        [EnumMember]
        SP07To10MigrationExportMapping = 107,

        [EnumMember]
        SP07To10MigrationImportMapping = 108,

        [EnumMember]
        LivelinkConnection = 109,

        [EnumMember]
        NotesConnection = 110,

        [EnumMember]
        NotesMigration = 111,

        [EnumMember]
        NotesMigrationOptions = 112,

        [EnumMember]
        NotesMigrationFilterOptions = 113,

        [EnumMember]
        NotesMigrationMappings = 114,

        [EnumMember]
        NotesMigrationPermissionMapping = 115,

        [EnumMember]
        MigrationSPPermissionLevel = 118,

        [EnumMember]
        MigrationConfigDB = 119,

        [EnumMember]
        eRoomMigration = 120,

        [EnumMember]
        eRoomMigrationOptions = 121,

        [EnumMember]
        eRoomMigrationFilterOptions = 122,

        [EnumMember]
        eRoomMigrationMappings = 123,

        [EnumMember]
        eRoomMigrationPermissionMapping = 124,

        //PublicFolder Migration 125-129
        [EnumMember]
        ExchangeConnection = 125,

        [EnumMember]
        PublicFolderMigration = 126,

        [EnumMember]
        PublicFolderMigrationFilterOptions = 1261,

        [EnumMember]
        PublicFolderMigrationMappings = 127,

        [EnumMember]
        PublicFolderMigrationOptions = 128,

        [EnumMember]
        PublicFolderMigrationPermissionLevel = 1291,

        [EnumMember]
        PublicFolderMigrationPermissionMapping = 129,

        //Livelink Migration 130-135
        [EnumMember]
        LivelinkMigration = 130,

        [EnumMember]
        LivelinkMigrationMappings = 131,

        [EnumMember]
        LivelinkMigrationOptions = 132,

        [EnumMember]
        LivelinkPermissionMapping = 133,

        [EnumMember]
        LivelinkPrivilege = 134,

        [EnumMember]
        SharepointGroup = 135,

        [EnumMember]
        LivelinkMigrationFilterOption = 136,

        [EnumMember]
        LivelinkMigrationMappingsImport = 137,

        [EnumMember]
        LivelinkMigrationOptionExport = 138,

        [EnumMember]
        LivelinkMigrationExport = 139,

        [EnumMember]
        MigrationImportLocation = 140,

        [EnumMember]
        eRoomMigrationExportLocation = 141,

        [EnumMember]
        NotesMigrationExportLocation = 142,

        [EnumMember]
        LivelinkMigrationExportLocation = 143,

        [EnumMember]
        LivelinkMigrationImport = 144,

        [EnumMember]
        LivelinkMigrationOptionsImport = 145,

        [EnumMember]
        NotesMigrationExport = 150,

        [EnumMember]
        NotesMigrationFilterOptionsExport = 152,

        [EnumMember]
        NotesMigrationMappingsExport = 153,

        [EnumMember]
        eRoomMigrationExport = 154,

        [EnumMember]
        eRoomMigrationOptionsExport = 155,

        [EnumMember]
        eRoomMigrationFilterOptionsExport = 156,

        [EnumMember]
        NotesMigrationImport = 160,

        [EnumMember]
        NotesMigrationOptionsImport = 161,

        [EnumMember]
        NotesMigrationFilterOptionsImport = 162,

        [EnumMember]
        NotesMigrationMappingsImport = 163,

        [EnumMember]
        eRoomMigrationImport = 164,

        [EnumMember]
        eRoomMigrationOptionsImport = 165,

        [EnumMember]
        eRoomMigrationFilterOptionsImport = 166,

        [EnumMember]
        eRoomMigrationMappingsImport = 167,

        [EnumMember]
        eRoomQuantity = 170,

        [EnumMember]
        FileQuantity = 171,

        [EnumMember]
        SP07To10Quantity = 172,

        [EnumMember]
        NotesQuantity = 173,

        [EnumMember]
        LivelinkQuantity = 174,

        [EnumMember]
        PublicFolderQuantity = 175,

        [EnumMember]
        MigrationDynamicMapping = 176,
        #endregion

        #region Define For Compliance
        [EnumMember]
        EDSearchServiceApplication = 201,
        [EnumMember]
        EDComplianceDBSettings = 203,
        [EnumMember]
        EDSearchResultLocation = 204,
        [EnumMember]
        EDExportLocation = 205,
        #endregion

        [EnumMember]
        NotificationForService = 202,

        //[EnumMember]
        //BackupScriptOperationType = 210,
        //[EnumMember]
        //RestoreScriptOperationType = 211,
        //[EnumMember]
        //VerifyScriptOperationType = 212,

        [EnumMember]
        GroupMapping = 213,
        [EnumMember]
        SystemProfile = 214,

        [EnumMember]
        PRRememberSetting = 230,

        #region Security Profile
        [EnumMember]
        DataEncryptionProfile = 301,
        [EnumMember]
        AOSAppliedSecurityProfile = 302,
        #endregion

        //[EnumMember]
        //PRSQLInstance = 215,

        //[EnumMember]
        //PRCatchTree = 216,

        //[EnumMember]
        //VaultProcessingPool = 302,

        //[EnumMember]
        //LicenseNotification = 360,

        //[EnumMember]
        //LicenseNotCompliantDuration = 361,

        //[EnumMember]
        //MaxJobThread = 330,

        //[EnumMember]
        //CentralDataBaseProfile = 310,

        [EnumMember]
        PhysicalDevice = 400,

        [EnumMember]
        LogicalDevice = 401,

        /// <summary>
        /// DocAve Policy Rule Definition, 组成Policy(454)的元素
        /// </summary>
        [EnumMember]
        CAPolicyRuleDefinition = 453,

        /// <summary>
        /// DocAve Policy Definition, 一个Policy包含多个Rule(453)
        /// </summary>
        [EnumMember]
        CAPolicyDefinition = 454,

        //for ca policy schedule
        [EnumMember]
        CAPolicySchedule = 478,

        //for CA Policy Enforcer DB Configuration
        //[EnumMember]
        //CAPolicyEnforcerConfigDB = 479,

        [EnumMember]
        CAAccessList = 480,

        [EnumMember]
        UserFeedback = 500,

        [EnumMember]
        Office365Account = 600,
        [EnumMember]
        AutoScanForSharePointSites = 601,
        [EnumMember]
        AutoScanForMailbox = 602,
        [EnumMember]
        AutoScanForOneDrive = 603,
        [EnumMember]
        AutoScanForO365Group = 604,

        [EnumMember]
        ExchangeOnlineFilter = 700,

        [EnumMember]
        ExchangeOnlineBackUpScheduleDefaultScheme = 701,

        [EnumMember]
        ExchangeOnlineBackUpScheduleScheme = 702,

        //[EnumMember]
        //DeletedOffice365RemoteSitecollection = 900,

        [EnumMember]
        ExportReport = 901,

        [EnumMember]
        AuditDatabase = 902,

        [EnumMember]
        DBAlertEmail = 903,

        [EnumMember]
        ArchiverExportSetting = 999,

        [EnumMember]
        ArchiverIndexDevice = 1000,

        [EnumMember]
        CollectRunningJob = 1001,

        //--CloudAppAdministration profile types(1010~1020)--
        [EnumMember]
        CAASearchUserProfile = 1010,
        [EnumMember]
        CAASearchGroupProfile = 1011,
        [EnumMember]
        CAAUserSetProfile = 1012,
        [EnumMember]
        CAAGroupSetProfile = 1013,
        [EnumMember]
        CAAEOCredentialProfile = 1014,
        [EnumMember]
        CAATempUserProfile = 1015,
        [EnumMember]
        CAAOperationResultProfile = 1016,
        [EnumMember]
        CAAPEProfile = 1017,
        [EnumMember]
        CAAPEResultProfile = 1018,
        [EnumMember]
        CAAPEConflictProfile = 1019,
        //--
        AutoRegistrationProfile = 1020,
        [EnumMember]
        AnonymousProfile = 1021,
        //用于标记客户是否支持定制job status[SAAS-27355]
        [EnumMember]
        JobStatusOption = 1022,
        [EnumMember]
        ProductLicense = 1023,
        [EnumMember]
        EmailTemplate = 1024,
        [EnumMember]
        TemplateDefaultLanguage = 1025,
        [EnumMember]
        SiteMasterSubInfoOnStorage = 1026,
        [EnumMember]
        AuthenticationSetting = 1027,
        [EnumMember]
        ArchiverExtendSetting = 1028,
        [EnumMember]
        EndUserRestoreSetting = 1029,
        [EnumMember]
        EndUserRestoreMasterKey = 1030,
        [EnumMember]
        ArchiveDBSEEMasterKey = 1031,
        [EnumMember]
        DataSizeAccumulate = 1032,
        [EnumMember]
        SubJobControlSetting = 1033,
        [EnumMember]
        ArchiverControlSetting = 1034,
        [EnumMember]
        ArchiverMultiThreadSetting = 1035,
        [EnumMember]
        PreNonce = 1036,
        [EnumMember]
        MasterKey = 1037,
        [EnumMember]
        IsScanPreservationHoldLibrary = 1038,
        [EnumMember]
        StubSetting = 1100,
        [EnumMember]
        CompliantExportSetting = 1101
    }
}