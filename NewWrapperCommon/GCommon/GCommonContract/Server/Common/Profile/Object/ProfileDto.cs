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
using System.Reflection;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.Common.Profile.Object
{
    #region Known types

    //[KnownType(typeof(OnewayPullContent))]
    //[KnownType(typeof(ReplicatorNetworkControlContent))]
    //[KnownType(typeof(ReplicatorConfigDBContent))]
    //[KnownType(typeof(ReplicatorSnapshotContent))]
    //[KnownType(typeof(OnlineMappingProfileContent))]
    //[KnownType(typeof(OnlineReplicationSubProfileContent))]
    //[KnownType(typeof(OnlineConflictionSubProfileContent))]
    //[KnownType(typeof(ExportMappingProfileContent))]
    //[KnownType(typeof(ExportReplicationSubProfileContent))]
    //[KnownType(typeof(ImportMappingProfileContent))]
    //[KnownType(typeof(ImportReplicationSubProfileContent))]
    //[KnownType(typeof(ImportConflictionSubProfileContent))]
    //[KnownType(typeof(FilterPolicyInfo))]
    //[KnownType(typeof(SPMigrationExportProfile))]
    //[KnownType(typeof(MigrationProfileContent))]
    //[KnownType(typeof(ColumnMappingDataContract))]
    //[KnownType(typeof(ListTitleMappingDataContract))]
    //[KnownType(typeof(ContentTypeMappingDataContract))]
    //[KnownType(typeof(TemplateMappingContract))]
    //[KnownType(typeof(DashboardReportSettingContent))]
    ////--Connector Settings--
    //[KnownType(typeof(CommonSetting))]
    //[KnownType(typeof(ProperitySetting))]
    //[KnownType(typeof(SecuritySetting))]
    //[KnownType(typeof(SPPermission))]
    //[KnownType(typeof(NotificationDto))]
    //[KnownType(typeof(GroupMappingContract))]
    //[KnownType(typeof(ReplicatorDetailsAlertContent))]
    //[KnownType(typeof(HealthCheckAlertContent))]
    //--
    #endregion
    [DataContract(Namespace = ContractConstants.Namespace)]
    [KnownType("GetKnownTypes")]
    public class ProfileDto
    {
        public static IEnumerable<Type> GetKnownTypes()
        {
            return AveKnownTypeContext.GetKnonwTypes(MethodBase.GetCurrentMethod().DeclaringType);
        }

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

        /// <summary>
        /// 用于CA Policy Enforcer存储Profile对应的filter policy id
        /// 对应Miscprofile表的string2列
        /// </summary>
        [DataMember]
        public string FilterPolicyIDs { get; set; }

        /// <summary>
        ///App Profile State, 对应MiscProfile表的Int2
        /// </summary>
        [DataMember]
        public AppProfileState AppProfileState { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ProfileType : int
    {
        [EnumMember]
        UnSpecified = 0,

        [EnumMember]
        FilterPolicy = 1,

        [EnumMember]
        ContentManagerDevice = 2,

        [EnumMember]
        ExportLocation = 3,

        [EnumMember]
        CacheSetting = 4,

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

        [EnumMember]
        Office365RemoteWebApplication = 12,

        [EnumMember]
        Office365RemoteSitecollection = 13,

        [EnumMember]
        QuickBackupDefaultSetting = 14,

        [EnumMember]
        BackUpScheduleScheme = 15,

        [EnumMember]
        PRQuickBackupDefaultSetting = 16,

        [EnumMember]
        PRBackStagingPolicy = 18,

        [EnumMember]
        SCOM = 19,

        [EnumMember]
        Notification = 20,

        [EnumMember]
        PRBackUpScheduleScheme = 24,

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
        ReplicatorReportLocation = 43,

        [EnumMember]
        ReplicatorHealthCheckAlert = 44,

        [EnumMember]
        ReplicatorPublishingTemplate = 45,

        [EnumMember]
        ReplicatorRDCSetting = 46,

        #endregion

        [EnumMember]
        DomainMapping = 61,

        [EnumMember]
        SPPermissionSetting = 62,//Connector Use

        [EnumMember]
        LicenseManager = 63,

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
        ArchiverProfile = 71,

        [EnumMember]
        EndUserArchiverSetting = 72,

        [EnumMember]
        ExtenderProfile = 73,

        [EnumMember]
        ArchiverDatabase = 74,

        [EnumMember]
        EndUserErrorMessageSetting = 75,

        [EnumMember]
        BoxConnectorPropertiesSetting = 76,

        [EnumMember]
        ExtenderLicenseLock = 77,

        [EnumMember]
        ArchiverLifeCycleRule = 78,

        [EnumMember]
        FSArchiverRule = 79,
        [EnumMember]
        FSArchiverConnFarm = 80,
        [EnumMember]
        FSArchiverConnGroup = 81,
        [EnumMember]
        FSArchiverConnection = 82,
        [EnumMember]
        FSAEndUserDomain = 83,
        [EnumMember]
        FSAEndUserSetting = 84,
        [EnumMember]
        FSArchiveQuantity = 85,

        [EnumMember]
        DefaultContentDBMasterKey = 86,
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
        LivelinkMigrationMappingsExport = 146,

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
        eRoomMigratinERMConnection = 168,

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

        [EnumMember]
        SP07To13Quantity = 177,

        [EnumMember]
        SP10To13Quantity = 178,

        [Obsolete("O365不再区分10/13")]
        [EnumMember]
        FileRemoteQuantity = 179,

        #region QuickPlace 180-186
        [EnumMember]
        QuickPlaceConnection = 180,

        [EnumMember]
        QuickPlaceMigration = 181,

        [EnumMember]
        QuickPlaceMigrationOptions = 182,

        [EnumMember]
        QuickPlaceMigrationFilterOptions = 183,

        [EnumMember]
        QuickPlaceMigrationMappings = 184,

        [EnumMember]
        QuickPlaceMigrationPermissionMapping = 185,

        [EnumMember]
        QuickPlaceQuantity = 186,

        #endregion

        [EnumMember]
        NotesMigrationOptionsExport = 187,

        [EnumMember]
        MigrationAzureConnection = 188,

        [EnumMember]
        FileSystemMigrationExportLocation = 189,

        /// <summary>
        /// FileSystemHighSpeedMigration Main Profile Type
        /// </summary>
        [EnumMember]
        FileSystemHighSpeedMigration = 190,

        /// <summary>
        /// FileSystemHighSpeedMigration Option Sub Profile Type
        /// </summary>
        [EnumMember]
        FileSystemHighSpeedMigrationOption = 191,

        /// <summary>
        /// FileSystemHighSpeedMigration Mapping Sub Profile Type
        /// </summary>
        [EnumMember]
        FileSystemHighSpeedMigrationMapping = 192,

        [EnumMember]
        FileSystemHighSpeedMigrationFilterOption = 193,

        [EnumMember]
        DocumentumMigrationExportLocation = 194,

        [EnumMember]
        DocumentumHighSpeedMigration = 195,

        [EnumMember]
        DocumentumHighSpeedMigrationOptions = 196,

        [EnumMember]
        DocumentumHighSpeedMigrationMappings = 197,

        [EnumMember]
        DocumentumHighSpeedMigrationFilterOptions = 198,

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

        #region Define For PlatformRecovery ScriptProfileDto type
        [EnumMember]
        BackupScriptOperationType = 210,
        [EnumMember]
        RestoreScriptOperationType = 211,
        [EnumMember]
        VerifyScriptOperationType = 212,
        #endregion

        [EnumMember]
        GroupMapping = 213,
        [EnumMember]
        SystemProfile = 214,

        [EnumMember]
        PRRememberSetting = 230,

        #region Security Profile
        [EnumMember]
        DataEncryptionProfile = 301,
        #endregion

        [EnumMember]
        PRSQLInstance = 215,

        [EnumMember]
        PRCatchTree = 216,

        [EnumMember]
        PRRememberTree = 217,

        [EnumMember]
        VaultProcessingPool = 302,

        [EnumMember]
        VaultRule = 303,

        [EnumMember]
        LicenseNotification = 360,

        [EnumMember]
        LicenseNotCompliantDuration = 361,

        [EnumMember]
        RequestLicenseNotification = 362,

        [EnumMember]
        LicenseRetrieveUser = 363,

        [EnumMember]
        PRKeepLiveResult = 380,

        [EnumMember]
        PRKeepLiveDeadLine = 381,

        [EnumMember]
        PRKeepLiveResultForGui = 382,

        [EnumMember]
        PRKeepLiveDeadLineForGui = 383,

        [EnumMember]
        PRRetention = 384,
        #region Documentum Migration 400-420
        [EnumMember]
        DocumentumMigration = 400,

        [EnumMember]
        DocumentumConnection = 401,

        [EnumMember]
        DocumentumMigrationOptions = 402,

        [EnumMember]
        DocumentumMigrationFilterOptions = 403,

        [EnumMember]
        DocumentumMigrationMappings = 404,

        [EnumMember]
        DocumentumMigrationPermissionMapping = 405,

        [EnumMember]
        DocumentumQuantity = 406,

        //[EnumMember]//need to delete
        //DocumentumRepositoryLevel = 407,

        //[EnumMember]//need to delete
        //DocumentumLevel = 408,

        //[EnumMember]//need to delete
        //DocumentumCabinetAndFolderLevel = 410,

        [EnumMember]
        DocumentumRepositoryPermission = 407,

        [EnumMember]
        DocumentumCabinetAndFolderPermission = 408,

        [EnumMember]
        DocumentumDocumentPermission = 409,

        [EnumMember]
        DocumentumSharePointGroup = 410,

        [EnumMember]
        DocumentumSharePointPermissionLevel = 411,

        #endregion

        [EnumMember]
        SnapshotTransport = 421,

        #region Migration quantity for SP2013
        [EnumMember]
        DocumentumQuantity2013 = 430,

        [EnumMember]
        eRoomQuantity2013 = 431,

        [EnumMember]
        FileQuantity2013 = 432,

        [EnumMember]
        LivelinkQuantity2013 = 433,

        [EnumMember]
        NotesQuantity2013 = 434,

        [EnumMember]
        PublicFolderQuantity2013 = 435,

        [EnumMember]
        QuickPlaceQuantity2013 = 436,

        [Obsolete("O365不再区分10/13")]
        [EnumMember]
        FileRemoteQuantity2013 = 437,

        #endregion

        #region Migration quantity for SP2016

        [EnumMember]
        DocumentumQuantity2016 = 438,

        [EnumMember]
        eRoomQuantity2016 = 439,

        [EnumMember]
        FileQuantity2016 = 440,

        [EnumMember]
        LivelinkQuantity2016 = 441,

        [EnumMember]
        NotesQuantity2016 = 442,

        [EnumMember]
        PublicFolderQuantity2016 = 443,

        [EnumMember]
        QuickPlaceQuantity2016 = 444,

        #endregion

        #region Migration quantity for SP2019

        [EnumMember]
        DocumentumQuantity2019 = 445,

        [EnumMember]
        eRoomQuantity2019 = 446,

        [EnumMember]
        FileQuantity2019 = 447,

        [EnumMember]
        LivelinkQuantity2019 = 448,

        [EnumMember]
        NotesQuantity2019 = 449,

        [EnumMember]
        PublicFolderQuantity2019 = 481,

        [EnumMember]
        QuickPlaceQuantity2019 = 482,

        #endregion

        [EnumMember]
        SQLRecoveryManagerStagingPolicy = 450,

        [EnumMember]
        SRMFilterPolicy = 451,

        [EnumMember]
        HASync = 452,

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

        [EnumMember]
        SRMKeepLiveResult = 455,

        [EnumMember]
        SRMKeepLiveDeadLine = 456,

        #region Migration dynamic mapping 461-470
        [EnumMember]
        DocumentumMigrationDynamicMapping = 461,

        [EnumMember]
        eRoomMigrationDynamicMapping = 462,

        [EnumMember]
        FileMigrationDynamicMapping = 463,

        [EnumMember]
        LivelinkMigrationDynamicMapping = 464,

        [EnumMember]
        NotesMigrationDynamicMapping = 465,

        [EnumMember]
        PublicFolderMigrationDynamicMapping = 466,

        [EnumMember]
        QuickPlaceMigrationDynamicMapping = 467,

        [EnumMember]
        SPMigrationDynamicRule = 468,
        #endregion

        [EnumMember]
        CAExportLocation = 471,

        [EnumMember]
        SP07ToRemote13Quantity = 473,

        [EnumMember]
        SP10ToRemote13Quantity = 474,

        [EnumMember]
        ListTitleMapping = 477,

        //for ca policy schedule
        [EnumMember]
        CAPolicySchedule = 478,

        //for CA Policy Enforcer DB Configuration
        [EnumMember]
        CAPolicyEnforcerConfigDB = 479,

        //for CA Access List
        [EnumMember]
        CAAccessList = 480,

        [EnumMember]
        PhysicalDevice = 600,

        [EnumMember]
        LogicalDevice = 601,

        #region MigrationOffice365Quantity  611-630
        [EnumMember]
        DocumentumOnlineQuantity = 611,

        [EnumMember]
        eRoomOnlineQuantity = 612,

        [EnumMember]
        FileOnlineQuantity = 613,

        [EnumMember]
        LivelinkOnlineQuantity = 614,

        [EnumMember]
        NotesOnlineQuantity = 615,

        [EnumMember]
        PublicFolderOnlineQuantity = 616,

        [EnumMember]
        QuickrOnlineQuantity = 617,

        //[EnumMember]
        //DocumentumRemote10Quantity = 611,

        //[EnumMember]
        //DocumentumRemote13Quantity = 612,

        //[EnumMember]
        //eRoomRemote10Quantity = 613,

        //[EnumMember]
        //eRoomRemote13Quantity = 614,

        //[EnumMember]
        //LivelinkRemote10Quantity = 615,

        //[EnumMember]
        //LivelinkRemote13Quantity = 616,

        //[EnumMember]
        //NotesRemote10Quantity = 617,

        //[EnumMember]
        //NotesRemote13Quantity = 618,

        //[EnumMember]
        //PublicFolderRemote10Quantity = 619,

        //[EnumMember]
        //PublicFolderRemote13Quantity = 620,

        //[EnumMember]
        //QuickrRemote10Quantity = 621,

        //[EnumMember]
        //QuickrRemote13Quantity = 622,
        #endregion

        [EnumMember]
        Office365AntoScanProfile = 690,

        [EnumMember]
        Office365AppToken = 691,

        [EnumMember]
        Office365Account = 699,

        [EnumMember]
        JobPerformance = 700,

        #region Log Manager
        [EnumMember]
        LogManagerRetrieve = 800,

        [EnumMember]
        LogManagerCache = 801,
        #endregion

        [EnumMember]
        DeletedOffice365RemoteSitecollection = 900,

        [EnumMember]
        AgentProxy = 950,

        [EnumMember]
        ConnectorSetting = 960,

        [EnumMember]
        FileSystemDriveLocation = 970,

        /// <summary>
        /// PR SMSP script
        /// </summary>
        [EnumMember]
        PRVssScript = 971,

        /// <summary>
        /// PR SMSP command
        /// </summary>
        [EnumMember]
        PRVssCommand = 972,

        //VM Credential Profile
        [EnumMember]
        VMCredential = 973,

        [EnumMember]
        PRVMScript = 974,

        [EnumMember]
        VMServerQuantity = 975,

        [EnumMember]
        PRWFAProfile = 976,

        #region High Availability

        [EnumMember]
        HACommand = 980,

        [EnumMember]
        HAScript = 981,

        [EnumMember]
        HAThrottle = 475,

        [EnumMember]
        HAInstance = 476,

        [EnumMember]
        HAConnectorCacheSetting = 982,

        [EnumMember]
        HALogshippingCacheSetting = 983,

        #endregion High Availability

        [EnumMember]
        RCCachingDB = 1001,

        //SP high speed Migration
        [EnumMember]
        SPHSMigrationOnlineMapping = 1006,

        [EnumMember]
        SPHSMigrationExportMapping = 1007,

        [EnumMember]
        SPHSMigrationImportMapping = 1008,

        [EnumMember]
        SP13ToRemote13Quantity = 1010,

        [EnumMember]
        RCWebPartSettings = 1002,

        [EnumMember]
        MigrationImportForHighSpeedLocation = 1009,

        #region High Speed Migration takes 1200~1300

        [EnumMember]
        LivelinkHighSpeedMigration = 1201,

        [EnumMember]
        LivelinkHighSpeedMigrationOption = 1202,

        [EnumMember]
        LivelinkHighSpeedMigrationMapping = 1203,

        [EnumMember]
        LivelinkHighSpeedMigrationFilterOption = 1204,

        [EnumMember]
        LivelinkHighSpeedMigrationMappingImport = 1205,

        [EnumMember]
        LivelinkHighSpeedMigrationOptionExport = 1206,

        [EnumMember]
        LivelinkHighSpeedMigrationExport = 1207,

        [EnumMember]
        LivelinkHighSpeedMigrationMappingExport = 1208,

        [EnumMember]
        LivelinkHighSpeedMigrationImport = 1209,

        [EnumMember]
        LivelinkHighSpeedMigrationOptionImport = 1210,

        [EnumMember]
        eRoomHighSpeedMigration = 1211,

        [EnumMember]
        eRoomHighSpeedMigrationOption = 1212,

        [EnumMember]
        eRoomHighSpeedMigrationMapping = 1213,

        [EnumMember]
        eRoomHighSpeedMigrationFilterOption = 1214,

        [EnumMember]
        eRoomHighSpeedMigrationMappingImport = 1215,

        [EnumMember]
        eRoomHighSpeedMigrationOptionExport = 1216,

        [EnumMember]
        eRoomHighSpeedMigrationExport = 1217,

        [EnumMember]
        eRoomHighSpeedMigrationMappingExport = 1218,

        [EnumMember]
        eRoomHighSpeedMigrationImport = 1219,

        [EnumMember]
        eRoomHighSpeedMigrationOptionImport = 1220,

        [EnumMember]
        NotesHighSpeedMigration = 1221,

        [EnumMember]
        NotesHighSpeedMigrationOption = 1222,

        [EnumMember]
        NotesHighSpeedMigrationMapping = 1223,

        [EnumMember]
        NotesHighSpeedMigrationFilterOption = 1224,

        [EnumMember]
        NotesHighSpeedMigrationMappingImport = 1225,

        [EnumMember]
        NotesHighSpeedMigrationOptionExport = 1226,

        [EnumMember]
        NotesHighSpeedMigrationExport = 1227,

        [EnumMember]
        NotesHighSpeedMigrationMappingExport = 1228,

        [EnumMember]
        NotesHighSpeedMigrationImport = 1229,

        [EnumMember]
        NotesHighSpeedMigrationOptionImport = 1230,
        #endregion

        [EnumMember]
        SP07To16Quantity = 1301,

        [EnumMember]
        SP10To16Quantity = 1302,

        [EnumMember]
        SP13To16Quantity = 1303,

        [EnumMember]
        SP13ToRemote16Quantity = 1304,

        [EnumMember]
        SP07To19Quantity = 1305,

        [EnumMember]
        SP10To19Quantity = 1306,

        [EnumMember]
        SP13To19Quantity = 1307,

        [EnumMember]
        SP16To19Quantity = 1308,

        [EnumMember]
        SP16ToRemote19Quantity = 1309,

        [EnumMember]
        SPMigrationLastBackupIndex = 1400,

        [EnumMember]
        SuperUserConfig = 1401,

        [EnumMember]
        AccountProfilePwdCrc = 1500,

        [EnumMember]
        AccountLogonInfo = 1501,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AppProfileState
    {
        [EnumMember]
        Draft = -1,
        [EnumMember]
        Active = 0,
        [EnumMember]
        InActive = 1,
    }
}