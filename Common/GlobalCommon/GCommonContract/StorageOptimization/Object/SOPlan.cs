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
using AvePoint.GCommon.Contract.GranularBackup.Object;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using System;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;

namespace AvePoint.GCommon.Contract.StorageOptimization.Object
{
    /// <summary>
    /// 定义SO Plan，继承PlanDto，所有SO模块共用此contract
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SOPlan : PlanDto
    {
        /// <summary>
        /// 是否立即跑job，此时不存schedule
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool RunNow { get; set; }

        /// <summary>
        /// 用于查看scheduled rule时候的回显，需要回台根据pool id取得pool对象，为GUI上显示name
        /// </summary>
        //[DataMember]
        //public ProcessingPoolContract ProcessingPool { get; set; }

        /// <summary>
        /// SO模块中自定义属性需要存到数据库中的，请直接定义在SOPlanExtension类中
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public SOPlanExtension SOPlanExtension { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public ArchiverType ArchiverType { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public string AgentId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public SOPlanDetails SOPlanDetails { get; set; }
        /// <summary>
        /// 根据Record DB的信息拼成的链接字符串
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public String RecordWebDBConnectionString { set; get; }
        /// <summary>
        /// 根据Record Explorer DB的信息拼成的DB对象
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public CosmosConnectionInfo RecordExplorerDB { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string RunJobUser { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string RecordsStoragePolicyId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public RecordsGlobalStorageSettings RecordsGlobalStorageSettings { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool SkipRemoveContentAndDestroyAction { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Dictionary<Guid, Tuple<bool, string>> GroupBCSColumnDictionary { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsRecordsOneDriveNode { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string RecordsHistoryDBConnectionString { get; set; }
        [DataMember]
        public bool IsEndUserRequest { get; set; }
        [DataMember]
        public List<EndUserRestoreItem> EndUserRequestItems { get; set; }
        public string JobId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsNullClassificationSetting { get; set; }
    }

    public class RecordsGlobalStorageSettings
    {
        public int Id { set; get; }

        public Guid StoragePolicyId { get; set; }

        public string StoragePolicyName { get; set; }

        public Guid ExportLocationId { get; set; }

        public string ExportLocationName { get; set; }

        public Guid SecurityProfileId { get; set; }

        public string SecurityProfileName { get; set; }

        public bool UseCompression { get; set; }

        public bool UseEncryption { get; set; }

        public int CompressionSpeed { get; set; }

        public DataSecurity CompressionMethod { get; set; }

        public DataSecurity EncryptionMethod { get; set; }

        public string Extentions { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CosmosConnectionInfo
    {
        [DataMember]
        public string Endpoint { get; set; }
        [DataMember]
        public string Key { get; set; }
        [DataMember]
        public string DatabaseId { get; set; }
        [DataMember]
        public string CollectionId { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SOPlanDetails
    {
        [DataMember]
        public List<RuleNodeContract> RuleNodeContract { get; set; }
        [DataMember]
        public List<RuleNodeContract> BreakInheritingChilren { get; set; }
    }

    /// <summary>
    ///存储SO Plan中自定义的属性，序列化为xml，赋值给Plan的Extension属性
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SOPlanExtension
    {
        /// <summary>
        /// 只是scheduled和archiver使用
        /// </summary>
        [DataMember]
        public string ProcessingPoolId { get; set; }
        [DataMember]
        public RemoteSiteCollection Site { get; set; }
        [DataMember]
        public ScheduleDto Schedule { get; set; }

        /// <summary>
        /// full text index for archvier
        /// </summary>
        [DataMember]
        public bool FullTextIndex { get; set; }

        /// <summary>
        /// for email notification
        /// </summary>
        [DataMember]
        public List<string> Notifications { get; set; }

        /// <summary>
        /// ApprovalType for archiver
        /// </summary>
        [DataMember]
        public ApprovalType ApprovalType { get; set; }

        /// <summary>
        /// ConnectionType for archiver
        /// </summary>
        [DataMember]
        public ConnectionTypeOption ConnectionType { get; set; }

        /// <summary>
        /// 目前只会Sync deletion用到, key: webappId,  value: ScheduleId(including delay)
        /// </summary>
        [DataMember]
        public Dictionary<string, SOSyncDelDelay> WebAppScheduleMapping { set; get; }

        /// <summary>
        /// full text index export 用.
        /// </summary>
        [DataMember]
        public List<SearchRequestResult> CrawlIndexSearchResult { set; get; }
        /// <summary>
        /// full text index export 用.
        /// </summary>
        #region   Archiver Restore options
        /// <summary>
        /// 如果还原类型为out place，用于存储Location信息
        /// </summary>
        [DataMember]
        public OutPlaceLocation OutPlaceLocation { set; get; }

        /// <summary>
        /// 区分是否是CONCORDANCE类型的restore.
        /// </summary>
        [DataMember]
        public RestoreFSOption RestoreFSOption { set; get; }

        /// <summary>
        /// 区分General mode/full text index mode
        /// </summary>
        [DataMember]
        public RestoreMode RestoreMode { set; get; }
        /// <summary>
        /// enum type : inplace(default), outplance  for archiver restore
        /// </summary>
        [DataMember]
        public RestoreType RestoreType { set; get; }
        /// <summary>
        /// enum type:  overwrite(default), not overwrite for archiver restore
        /// </summary>
        [DataMember]
        public RestoreOption RestoreOption { set; get; }

        /// <summary>
        /// enum type:  overwrite(default), not overwrite for app archiver restore
        /// </summary>
        [DataMember]
        public RestoreOption RestoreAPPOption { set; get; }

        /// <summary>
        /// 页面的work flow 选项, 适用于Archiver, Archiver restore;
        /// </summary>
        [DataMember]
        public BackupRestoreWorkflow WorkflowState { get; set; }
        /// <summary>
        /// Extender 数据升级用, 高四位压缩, 低四位加密, 同Scheduled Rule
        /// </summary>
        [DataMember]
        public int DataSecurity { get; set; }

        [DataMember]
        public string EncryptionInfoId { get; set; }

        [DataMember]
        public string EncryptionInfoName { get; set; }
        /// <summary>
        /// 数据升级用, 区分docave, netapp, imb
        /// </summary>
        [DataMember]
        public PlatformType PlatformType { get; set; }
        [DataMember]
        public string ProfileCategory { get; set; }

        /// <summary>
        /// NetApp Device Setting,  保存SnapMirror和SnapVault设置
        /// </summary>
        [DataMember]
        public bool SnapMirror { get; set; }

        [DataMember]
        public bool SnapVault { get; set; }

        [DataMember]
        public bool IncludeListView { get; set; }

        [DataMember]
        public bool IsRemoveTheStubAfterRestore { get; set; }

        [DataMember]
        public bool DisableIRMSetting { get; set; }

        [DataMember]
        public bool IncludeTerm { get; set; }

        [DataMember]
        public bool ManualArchive { get; set; }

        [DataMember]
        public bool EnableSuperUserDecryptsFiles { get; set; }

        [DataMember]
        public string DestStoragePolicyId { get; set; }

        [DataMember]
        public string ZipFilePassword { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public bool UseBackupStorageQuota { get; set; }

        [DataMember]
        public long StorageQuota { get; set; }

        [DataMember]
        public double ResourceQuota { get; set; }

        [DataMember]
        public string SitesGroupName { get; set; }

        [DataMember]
        public string AdminUrl { get; set; }

        [DataMember]
        public string TenantId { get; set; }


        [DataMember]
        public bool OverwriteRecyclebin { get; set; }
        #endregion

        #region  EBS stub to RBS stub
        [DataMember]
        public List<RuleAllianceContract> ScheduledRuleAlliances { get; set; }

        [DataMember]
        public Dictionary<string, List<string>> RealTimeRuleAlliances { get; set; }
        #endregion
        /// <summary>
        /// RecordsOnline cache before start job
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public String RecordWebDBConnectionString { set; get; }
        /// <summary>
        /// RecordsOnline cache before start job
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public CosmosConnectionInfo RecordExplorerDB { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public String RunJobUser { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public RecordsGlobalStorageSettings RecordsGlobalStorageSettings { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public bool SkipRemoveContentAndDestroyAction { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Dictionary<string, Tuple<bool, string>> SiteBCSColumnDictionary { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsScan { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsRecordsOneDriveNode { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public String RecordsHistoryDBConnectionString { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsEndUserRequest { get; set; }
        [DataMember]
        public List<EndUserRestoreItem> EndUserRequestItems { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string JobId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string RestoreStorageString { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public ArchiveIntegrationModules IntegrationModule { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsNullClassificationSetting { get; set; }
    }
    /// <summary>
    /// net use Location信息
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class OutPlaceLocation
    {
        [DataMember]
        public string LocationUrl { set; get; }
        [DataMember]
        public string UserName { set; get; }
        [DataMember]
        public string Password { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RestoreFSOption
    {
        [EnumMember]
        NONE = 0,
        [EnumMember]
        CONCORDANCE = 1
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RestoreMode
    {
        [EnumMember]
        General = 0,
        [EnumMember]
        FullTextIndex = 1
    }
    /// <summary>
    ///记录sync deletion 的delay
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SOSyncDelDelay
    {
        [DataMember]
        public IncludeNewContentDBState IncludeNewState { set; get; }
        [DataMember]
        public NodeLevel Level { set; get; }
        [DataMember]
        public string ParentId { set; get; }
        [DataMember]
        public string ParentName { set; get; }
        [DataMember]
        public string ScheduleId { set; get; }
        [DataMember]
        public int DelaySize { set; get; }
        [DataMember]
        public DateTimeUnit DelayUnit { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum IncludeNewContentDBState
    {
        [EnumMember]
        IncludeNew_Yes = 0,   //默认值, 老数据
        [EnumMember]
        IncludeNew_No = 1,
    }
    /// <summary>
    /// 定义SO Plan所需要的type
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SOPlanType
    {
        [EnumMember]
        NONE = 0,
        [EnumMember]
        CONFIG_RBS = 1,
        [EnumMember]
        STUB_RETENTION = 2,
        [EnumMember]
        Profile = 3,
        [EnumMember]
        CONNECTOR_DATAUPGRADE = 4,
        [EnumMember]
        EXTENDER_DATAUPGRADE = 5,
        [EnumMember]
        THIRDPARTY_DATAUPGRADE = 6,
        [EnumMember]
        EBSSTUB_DATAUPGRADE = 7,
        [EnumMember]
        WrapperPlan = 8,
        [EnumMember]
        MOVE_INDEX = 9,
    }

    /// <summary>
    /// 定义ApprovalType
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ApprovalType
    {
        [EnumMember]
        AUTO = 0,
        [EnumMember]
        MANUAL = 1
    }
    /// <summary>
    /// in place(default), out place for restore
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RestoreType
    {
        [EnumMember]
        InPlace,
        [EnumMember]
        OutPlace,
        [EnumMember]
        ToFileSystem,
        [EnumMember]
        StubOop,
        [EnumMember]
        AOPSOop,
        [EnumMember]
        ToSPOLocation,
        [EnumMember]
        ArchivedStubs,
        [EnumMember]
        M365InPlaceArchivedFiles,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RestoreObjectLevel
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        SiteCollection = 2,
        [EnumMember]
        Site = 4,
        [EnumMember]
        List = 8,
        [EnumMember]
        Folder = 16,
        [EnumMember]
        Item = 32,
        [EnumMember]
        Document = 64,
        [EnumMember]
        Attachment = 128,
        [EnumMember]
        DocumentVersion = 256,
        [EnumMember]
        GoogleDriveDocument  = 16777216,
        [EnumMember]
        Teams = 33554432,
        [EnumMember]
        Mailbox = 33554433,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RestoreScope
    {
        [EnumMember]
        IncludeChildrenContainersAndFolders,
        [EnumMember]
        SelectedLocationOnly,
    }

    /// <summary>
    /// restore option overwrite(default), not overwrite for archiver restore
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RestoreOption
    {
        [EnumMember]
        OverWrite,
        [EnumMember]
        NotOverWrite,
        [EnumMember]
        Append
    }

    [DataContract]
    public enum ConflictSolutionType
    {
        [EnumMember]
        Skip,
        [EnumMember]
        Append,
        [EnumMember]
        Overwrite,
        [EnumMember]
        Replace,
        [EnumMember]
        Merge
    }

    [DataContract]
    public enum ConnectionTypeOption
    {
        [EnumMember]
        None = -1,

        [EnumMember]
        ServiceAccount = 0,

        [EnumMember]
        AppToken = 1
    }
}
