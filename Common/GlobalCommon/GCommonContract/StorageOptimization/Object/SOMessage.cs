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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper;
using AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.StorageOptimization.Object
{
    [AveCodeReview("2012/07/10",
       "dlliu@avepoint.com",
       "dlliu@avepoint.com",
       new string[]
        {
            CodeReviewConstants.CHECK_LIST_ID_CO_2,
            CodeReviewConstants.CHECK_LIST_ID_CO_3,
            CodeReviewConstants.CHECK_LIST_ID_CO_4,
            CodeReviewConstants.CHECK_LIST_ID_CO_5,
            CodeReviewConstants.CHECK_LIST_ID_CO_6,
            CodeReviewConstants.CHECK_LIST_ID_CO_7,
            CodeReviewConstants.CHECK_LIST_ID_CO_8,
            CodeReviewConstants.CHECK_LIST_ID_CO_9,
            CodeReviewConstants.CHECK_LIST_ID_CO_10,
            CodeReviewConstants.CHECK_LIST_ID_CO_11,
            CodeReviewConstants.CHECK_LIST_ID_CO_12,
            CodeReviewConstants.CHECK_LIST_ID_EH_1,
            CodeReviewConstants.CHECK_LIST_ID_EH_2,
            CodeReviewConstants.CHECK_LIST_ID_CS_1,
        },
       null,
    true)]
    /// <summary>
    /// SO模块与Agent通信使用此contract，修改此contract需要通知Agent端
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SOMessage : AveMessage
    {
        //message for common
        [DataMember]
        public SOAction Action { get; set; }
        [DataMember]
        public SOMessageType MessageType { get; set; }
        [DataMember]
        public string ErrorMessage { get; set; }
        [DataMember]
        public string FarmId { get; set; }
        [DataMember]
        public SOPlan Plan { get; set; }
        [DataMember]
        public SOJob Job { get; set; }

        //for stub restore job and view stubs-----start---------
        //for end user archiver browser tree
        [DataMember]
        public SPTreeNodeDto SPTreeNode { get; set; }  //used by sync deleteion
        [DataMember]
        public int CurrentPageNumber { set; get; }
        [DataMember]
        public int PerPageCount { set; get; }
        //for end user archiver web application end user archiver feature
        [DataMember]
        public List<SOTreeNode> SPTreeNodes { get; set; }
        //[DataMember]
        //public List<TagMaping> ArchiverTagMappings { get; set; }

        /// <summary>
        /// used for connector sync schedule.
        /// </summary>
        [DataMember]
        public List<SPTreeNodeDto> TreeNodes { get; set; }
        [DataMember]
        public int TotalPageCount { set; get; }
        //----------------------------------------end-----------

        //message for realtime
        [DataMember]
        public List<SPTreeNodeDto> WebAppNodes { get; set; }
        [DataMember]
        public Dictionary<string, List<RuleNodeContract>> ContentDBDic { get; set; }
        [DataMember]
        public List<string> EnabledContentDBIds { get; set; }
        [DataMember]
        public List<string> DisabledContentDBIds { get; set; }
        [DataMember]
        public Dictionary<string, long> ContentDBWithMinValue { get; set; }

        //message for scheduled
        [DataMember]
        public List<RuleNodeContract> ScheduledConfigs { get; set; }

        /// <summary>
        /// 只是scheduled和archiver使用
        /// </summary>
        [DataMember]
        public string ProcessingPoolId { get; set; }

        [DataMember]
        public Dictionary<string, BlobProviderContract> BlobProviderNodes { get; set; }

        [DataMember]
        public BlobProviderContract BlobProviderContract { get; set; }

        //message for StubDB
        [DataMember]
        public StubDatabaseInfo StubDatabaseInfo { get; set; }
        [DataMember]
        public bool SpecifiedStubDBIsExist { get; set; }

        //message for install provider by one server agent
        [DataMember]
        public EBSAction EBSAction { get; set; }
        [DataMember]
        public BlobProviderBinary BlobProviderBinary { get; set; }

        #region == define job request for somessage ==
        [DataMember]
        public ScheduledJobRequest ScheduledJobRequest { get; set; }

        //[DataMember]
        //public ArchiverBackupRequest ArchiverBackupRequest { get; set; }

        //[DataMember]
        //public VaultBackupRequest VaultBackupRequest { get; set; }

        //[DataMember]
        //public ArchiverJobRequest ArchiverJobRequest { get; set; }

        //[DataMember]
        //public ArchiverRestoreJobRequest ArchiverRestoreJobRequest { get; set; }

        [DataMember]
        public RealTimeSettingRequest RealTimeSettingRequest { get; set; }

        [DataMember]
        public SOCacheRequest SOCacheRequest { get; set; }

        [DataMember]
        public SyncDeletionJobRequest SyncDeletionJobRequest { get; set; }

        [DataMember]
        public StopSOJobRequest StopSOJobRequest { get; set; }
        #endregion

        [DataMember]
        public long PhysicalDeviceData { get; set; }

        [DataMember]
        public Guid[] physicalDevices { get; set; }

        /// <summary>
        /// for report center 
        /// </summary>
        [DataMember]
        public Dictionary<Guid, long> PhysicalDeviceDatas { get; set; }

        [DataMember]
        public ArchiverRetentionMetaDataResponse ArchiverRetentionMetaData { get; set; }

        //#region ===== vault =====
        //[DataMember]
        //public ScanType ScanType { set; get; }
        //[DataMember]
        //public ExportJobRequest VaultExportRequest { get; set; }
        //#endregion
    }

    #region == Define SOAction
    /// <summary>
    /// Define SO Action,Agent端会根据具体的Action执行不同的操作
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SOAction
    {
        //SO action for realtime
        [EnumMember]
        NONE = 0,
        [EnumMember]
        REAL_TIME_SENT_CONFIGS = 101,
        [EnumMember]
        REAL_TIME_GET_DISCOVER = 102,
        [EnumMember]
        REAL_TIME_GET_CONTENT_DB = 103,
        [EnumMember]
        REAL_TIME_DELETE_RULE_FOR_RBS = 104,
        [EnumMember]
        REAL_TIME_SET_MIN_SIZE_TO_CONTENTDB = 105,
        //SO action for stub restore
        [EnumMember]
        BROWSER_FILES = 201,
        [EnumMember]
        STUB_RESTORE = 202,
        //SO action for Provider Setting
        [EnumMember]
        GET_PROVIDER_CONFIG = 301,
        [EnumMember]
        GET_PROVIDER_NODE = 302,
        [EnumMember]
        DEPLOY_PROVIDER = 303,
        [EnumMember]
        DEPLOY_EBS = 304,
        [EnumMember]
        DEPLOY_RBS = 305,
        //SO action for Cache
        [EnumMember]
        CACHE_INFO = 401,
        //SO action for StubDB
        [EnumMember]
        CREATE_STUBDB = 501,
        [EnumMember]
        CHECK_STUBDB_ISEXISTING = 502,
        [EnumMember]
        STUB_RETENTION = 601,
        //SO action for scheduled
        [EnumMember]
        SCHEDULED_SEND_JOB_REQUEST = 701,
        //SO action for archiver
        //[EnumMember]
        //ARCHIVER_SCAN_JOB_REQUEST = 801,

        //[EnumMember]
        //ARCHIVER_BACKUP_JOB_REQUEST = 802,

        //[EnumMember]
        //ARCHIVER_RESTORE_JOB_REQUEST = 803,

        //[EnumMember]
        //END_USER_ARCHIVER_GET_FEATURE = 804,

        //[EnumMember]
        //END_USER_ARCHIVER_INSTALL_FEATURE = 805,

        //[EnumMember]
        //ENDUSER_ARCHIVER_BACKUP_JOB_REQUEST = 806,

        //[EnumMember]
        //ENDUSER_ARCHIVER_RESTORE_JOB_REQUEST = 807,

        //[EnumMember]
        //VALIDATE_END_USER_ARCHIVER_TAG_MAPPING = 808,

        //[EnumMember]
        //ARCHIVER_DATABASE_REQUEST = 809,

        [EnumMember]
        CONNECTOR_SYNC_JOB_REQUEST = 901,

        [EnumMember]
        PHYCIAL_DEVICE_ARCHIVE_DATA_REQUEST = 1001,

        [EnumMember]
        PHYCIAL_DEVICE_ARCHIVE_DATA_REQUEST_FOR_REPORTCENTER = 1002,

        [EnumMember]
        SEND_STOP_SO_JOB_REQUEST = 1101,

        [EnumMember]
        ARCHIVER_RETENTION_METADATA = 1200,

        [EnumMember]
        STUB_UPGRADE_JOB_REQUEST = 1300,

        [EnumMember]
        THIRD_STUB_UPGRADE_JOB_REQUEST = 1301,

        [EnumMember]
        VALIDATE_CONTENTDB_EXIST_EBS_STUB = 1302,

        [EnumMember]
        EBS_STUB_UPGRADE_JOB_REQUEST = 1303

        //[EnumMember]
        //VAULT_SCAN_JOB_REQUEST = 1400,

        //[EnumMember]
        //VAULT_BACKUP_JOB_REQUEST = 1500,

        //[EnumMember]
        //VAULT_EXPORT_JOB_REQUEST = 1501
    }

    /// <summary>
    /// 定义SO Cache Action
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CacheAction
    {
        [EnumMember]
        CACHE_ALL = 0,
        [EnumMember]
        CACHE_LOGICALDEVICE = 1,
        [EnumMember]
        CACHE_PHYSICALDEVICE = 2,
        [EnumMember]
        CACHE_DEVICERELATION = 3,
        //节点信息[Farm WebApplication ContentDB]
        [EnumMember]
        CACHE_PROVIDERNODE = 4,
        [EnumMember]
        CACHE_STUBDB = 5,
        [EnumMember]
        CACHE_ALLSTUBDBID = 6
    }
    #endregion

    /// <summary>
    /// 用于GUI调Manager方法时的返回消息
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SOReturnMessage
    {
        [DataMember]
        public SOMessageType MessageType { get; set; }
        //message for SO Index Page,
        [DataMember]
        public bool isConfigedStub { get; set; }
        //message for page to show FeedBack
        [DataMember]
        public string ReturnName { get; set; }

        /// <summary>
        /// 返回操作的对象个数
        /// </summary>
        [DataMember]
        public int OperatedCount { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }
        [DataMember]
        public FailedType FailedType { get; set; }
        /// <summary>
        /// 操作成功以后，需要前台显示提示语的时候用此属性来判断
        /// </summary>
        [DataMember]
        public SucceedType SucceedType { get; set; }
        //message for SO Index Page
        [DataMember]
        public List<BlobProviderContract> FarmInfos { get; set; }

        //define processing pool setting
        [DataMember]
        public ProcessingPoolSetting ProcessingPoolSetting { get; set; }
        [DataMember]
        public string StubDBID { get; set; }
        [DataMember]
        public Rule ReturnRule { get; set; }
        [DataMember]
        public string ReturnId { get; set; }

        //Message for end user archiver active action
        //[DataMember]
        //public List<SOTreeNode> SuccessNodes { get; set; }
        [DataMember]
        public List<TagMaping> ArchiverTagMappings { get; set; }

        //Message for EBS stub upgrade
        /// <summary>
        /// 用于返回disable EBS rule的时候返回失败的agent，供前台显示使用
        /// </summary>
        [DataMember]
        public List<string> FailedAgents { get; set; }
        /// <summary>
        /// 用于返回有EBS stub但是没激活RBS的content database的名字，供前台显示使用
        /// </summary>
        [DataMember]
        public List<string> ContentDBNames { get; set; }
        /// <summary>
        /// 用于返回是否有正在run的scheduled job
        /// </summary>
        [DataMember]
        public bool IsRunningJobExist { get; set; }
        /// <summary>
        /// 用于返回是否有EBS realtime rule或者extender scheduled rule
        /// </summary>
        [DataMember]
        public bool IsEBSRulesExist { get; set; }
        //Message for realtime CLI
        [DataMember]
        public List<Rule> Rules { get; set; }

        [DataMember]
        public List<RuleNodeContract> NodeConfigs { get; set; }

        [DataMember]
        public RuleNodeContract NodeConfigState { get; set; }
 
        [DataMember]
        public List<SORuleInfoContract> ReturnProfiles { get; set; }

        [DataMember]
        public List<ArchiverSiteMasterIndexMessage> ReturnSiteMasters { get; set; }
        [DataMember]
        public string SiteTitle { get; set; }
        [DataMember]
        public NodeType SiteCollectionType { get; set; }
        [DataMember]
        public bool IsReadOnlySite { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SOMessageType
    {
        [EnumMember]
        Successful,
        [EnumMember]
        Failed
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum FailedType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        NameExisting = 1,
        [EnumMember]
        SameSize = 2,
        [EnumMember]
        EarlierStartTime = 3,
        [EnumMember]
        PoolNotExisting = 4,
        [EnumMember]
        ConnStrError = 5,
        [EnumMember]
        CanNotRunAnyMore = 6,
        [EnumMember]
        NodeNotExisting = 34,
        [EnumMember]
        RuleIsRunnning = 35,
        [EnumMember]
        PermissionError = 36,
        [EnumMember]
        UserCannotFound = 37,
        [EnumMember]
        SecurityTrimingException = 38,
        [EnumMember]
        InsufficientPrivilegesForStub = 39,
        [EnumMember]
        InsufficientPrivilegesForSite = 40,
        [EnumMember]
        SiteCollectionLocked = 41,
        [EnumMember]
        UserNotGroupOwner = 42,
        [EnumMember]
        SiteNotRegistered = 43,
        [EnumMember]
        RequestResourceNotFound = 44,
        [EnumMember]
        UserNotGroupOwnerOrMember = 45,
        [EnumMember]
        UserNotOwnerForSharePointSite = 46,
        [EnumMember]
        UserNotOwnerOrMemberForSharePointSite = 47,
        [EnumMember]
        UserNotOwnerOrSpecifiedGroupForSharePointSite = 48,
        [EnumMember]
        SiteCollectionReadOnly = 49,
        [EnumMember]
        EmptyCredential = 50,
        [EnumMember]
        SiteTypeNotSupport = 51,
        [EnumMember]
        StubFileNotExsit = 52,
        [EnumMember]
        UserNotOwnerOrMemberOrVisitorForSharePointSite = 53,
        [EnumMember]
        ActiveAppProfileNotFound = 54,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SucceedType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        MustRunNow = 1,
        [EnumMember]
        ScheduleIsAvailable = 2
    }

    #region == Define Request ==
    /// <summary>
    /// define scheduled job request
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RealTimeSettingRequest
    {
        [DataMember]
        public List<RuleNodeContract> RealTimeConfigs { get; set; }
        [DataMember]
        public Dictionary<string, Rule> RealTimeRulesDic { get; set; }
    }

    /// <summary>
    /// define scheduled job request
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ScheduledJobRequest
    {
        [DataMember]
        public List<RuleNodeContract> ScheduledConfigs { get; set; }

        [DataMember]
        public SOJob Job { get; set; }

        [DataMember]
        public string LogicalDeviceId { get; set; }

        [DataMember]
        public int DataSecurity { get; set; }

        [DataMember]
        public DataEncryptionInfoWrapper EncryptionInfoWrapper { get; set; }

        [DataMember]
        public PlatformType PlatformType { set; get; }

        [DataMember]
        public UpgradeStubType UpgradeStubType { set; get; }

        [DataMember]
        public Dictionary<string, RuleNodeContract> ProviderMappingForUpgrade { set; get; }

        [DataMember]
        public string IndexDeviceIDForUpgrade { set; get; }

    }

    ///// <summary>
    ///// define archiver scan job request
    ///// </summary>
    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public class ArchiverJobRequest
    //{
    //    [DataMember]
    //    public List<RuleNodeContract> ArchiverConfigs { get; set; }

    //    [DataMember]
    //    public StubDatabaseInfo ArchiverDBInfo { set; get; }

    //    [DataMember]
    //    public SOJob Job { get; set; }

    //    [DataMember]
    //    public SOPlan Plan { get; set; }

    //    [DataMember]
    //    public Dictionary<string, CacheSettingDto> MediaCacheInfoDic { get; set; }

    //    /// <summary>
    //    /// End user archiver用于给client发送其所需要的信息
    //    /// </summary>
    //    [DataMember]
    //    public string MetaData { get; set; }
    //}

    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public class ArchiverRestoreJobRequest
    //{
    //    [DataMember]
    //    public RestoreConfig Config { get; set; }

    //    [DataMember]
    //    public ArchiverRestoreRequest ConfigForMedia { get; set; }

    //    [DataMember]
    //    public ServiceDto MediaInfo { get; set; }
    //}

    /// <summary>
    /// define move stubDB request
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MoveStubDatabaseRequest
    {
        [DataMember]
        public List<BlobProviderContract> BlobProviderConfigs { get; set; }

        [DataMember]
        public StubDatabaseInfo StubDatabaseInfo { get; set; }
    }

    /// <summary>
    /// define stop SO job request
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class StopSOJobRequest
    {
        [DataMember]
        public string SOSubJobId { get; set; }

        [DataMember]
        public string SOJobId { get; set; }

        [DataMember]
        public string ProcessingPoolId { get; set; }

    }

    /// <summary>
    /// define so cache request
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SOCacheRequest
    {
        // message for SO Cache
        [DataMember]
        public CacheAction CacheAction { get; set; }
        [DataMember]
        public Dictionary<string, LogicalDeviceDto> LogicalDevice { get; set; }
        [DataMember]
        public Dictionary<string, PhysicalDeviceDto> PhysicalDevice { get; set; }
        [DataMember]
        public Dictionary<int, DeviceRelationContract> DeviceRelation { get; set; }
        [DataMember]
        public Dictionary<string, BlobProviderContract> BlobProviderNodes { get; set; }
        [DataMember]
        public Dictionary<string, StubDatabaseInfo> StubDBInfo { get; set; }
        [DataMember]
        public Dictionary<string, string> StubDBId { get; set; }
    }

    /// <summary>
    /// define archiver scan job request
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SyncDeletionJobRequest
    {
        [DataMember]
        public SOJob Job { get; set; }

        [DataMember]
        public BlobProviderContract BlobProviderContract { get; set; }

        [DataMember]
        public int SyncDeletionDelay { set; get; }

        [DataMember]
        public DateTimeUnit DelayUnit { set; get; }
    }

    /// <summary>
    /// define processing pool setting
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ProcessingPoolSetting
    {
        [DataMember]
        public List<ProcessingPoolContract> DeletedPool { get; set; }
    }

    /// <summary>
    /// archiver retention meta data
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverRetentionMetaDataResponse
    {
        [DataMember]
        public List<AvePoint.GCommon.Contract.StorageOptimization.Object.ArchiverRetentionMetaData> farmTree { get; set; }
        [DataMember]
        public Dictionary<string, LogicalDeviceDto> LogicalDevice { get; set; }
        [DataMember]
        public Dictionary<string, PhysicalDeviceDto> PhysicalDevice { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum UpgradeStubType
    {
        [EnumMember]
        Extender = 0,
        [EnumMember]
        Connector = 1,
        [EnumMember]
        Archiver = 2,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverIndexMessage
    {
        [DataMember]
        public List<SPTreeNodeDto> TreeNodes { get; set; }
        [DataMember]
        public LogicalDeviceDto LogicalDevice { get; set; }
        [DataMember]
        public StoragePolicyDto FullTextIndexSetting { get; set; }
        [DataMember]
        public bool IsBtnConfig { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverSiteMasterIndexMessage
    {
        [DataMember]
        public string SiteUrl { get; set; }
        [DataMember]
        public string ArchiverTime { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverExportReportData
    {
        [DataMember]
        public string SelectExportLocationId { get; set; }
        [DataMember]
        public List<ArchiverSiteMasterIndexMessage> SelectArchiverSiteMasterIndexs { get; set; }
    }

    #endregion

    /// <summary>
    /// Vault scan job has two types full or increnmental when run scan job
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ScanType  
    {
        [EnumMember]
        Incremental,
        [EnumMember]
        Full
    }

    /// <summary>
    /// vault module when excute export job has two types for exported files
    /// </summary>
    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public enum ExportType 
    //{ 
    //    [EnumMember]
    //    Autonomy,
    //    [EnumMember]
    //    Concordance  
    //}
    
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ChangeType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        ArchiverIndexDevice = 1,
        [EnumMember]
        CrawlProfile = 2,
        [EnumMember]
        ArchiverIndexDeviceAndCrawlProfile = 3
    }

    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public class ExportJobRequest 
    //{
    //    [DataMember]
    //    public String JobId { get; set; } 
    //    [DataMember]
    //    public String PlanId { get; set; }
    //    [DataMember]
    //    public String ParentJobId { get; set; }
    //    [DataMember]
    //    public ExportType ExportType { set; get; }
    //    [DataMember]
    //    public string MediaStorageXri { set; get; }
    //    [DataMember]
    //    public PhysicalDeviceDto PhysicalDeviceDto { set; get; }
    //}
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EndUserRestoreSetting : IProfileContent
    {
        [DataMember]
        public bool IsRestoreArchivedTier { get; set; }
        [DataMember]
        public bool IsCustomizeStubRestorePage { get; set; }
        [DataMember]
        public string Logo { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public string Footer { get; set; }
        [DataMember]
        public Status IsAllowRestore { get; set; }
        [DataMember]
        public EndUserPermissionSetting PermissionSetting { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EndUserPermissionSetting
    {
        [DataMember]
        public GroupOrTeamSitePermissionSetting TeamsAndGroup { get; set; }
        [DataMember]
        public SharePointSitePermissionSetting SiteCollection { get; set; }
        [DataMember]
        public string SiteCollectionSpecialGroupNames { get; set; }
        [DataMember]
        public Status IsRestoreGroupTeamSite { get; set; }
        [DataMember]
        public Status IsExportGroupTeamSite { get; set; }
        [DataMember]
        public Status IsRestoreSiteCollection { get; set; }
        [DataMember]
        public Status IsExportSiteCollection { get; set; }
        [DataMember]
        public Status IsRestoreStubLink { get; set; }
        [DataMember]
        public Status IsExportStubLink { get; set; }
    }

    public enum Status
    {
        True,
        False,
    }

    public enum SharePointSitePermissionSetting
    {
        [DataMember]
        SiteOwner,
        [DataMember]
        SiteOwnerAndSiteMemberGroup,
        [DataMember]
        SiteOwnerAndSpecialGroup,
        [DataMember]
        SiteOwnerAndSiteMemberGroupAndSiteVisitor
    }

    public enum GroupOrTeamSitePermissionSetting
    {
        [EnumMember]
        Owner,
        [EnumMember]
        OwnerOrMembler
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class StubRetentionMessage
    {
        [DataMember]
        public List<SOPlan> plans { set; get; }
        /// <summary>
        /// 是设置还是取消设置, Yes--True; No -- false;
        /// </summary>
        [DataMember]
        public bool IsSetSchedule { set; get; }
        /// <summary>
        /// 是否覆盖子节点的设置
        /// </summary>
        [DataMember]
        public bool IsOverride { set; get; }
    }
}