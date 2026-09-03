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
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.GranularBackup.Object;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Contract.Server.Login;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365Account.Object;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Tree;

namespace AvePoint.GCommon.Contract.StorageOptimization.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Rule : INotifyPropertyChanged
    {
        /// <summary>
        /// Manager端数据库Id，Agent端不使用
        /// </summary>
        [XmlIgnore]
        [DataMember(EmitDefaultValue = false)]
        public string Id { get; set; }

        /// <summary>
        /// Manager端使用，Agent端不使用
        /// </summary>
        [XmlIgnore]
        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }

        /// <summary>
        /// GUI显示用，agent不用
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string RuleScope { set; get; }

        /// <summary>
        /// Export是否勾选ExportDataBeforeArchiving
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool ExportDataBeforeArchiving { get; set; }
        /// <summary>
        /// 决定Azure BYOS storage是否在archiver上传数据块时置成archiver tier
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool MoveToArchiverTierWhenArchiving { get; set; }
        /// <summary>
        /// rule manager页面使用
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string ProfileInfo { set; get; }

        /// <summary>
        /// GUI显示用，agent不用
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Module { set; get; }

        /// <summary> 
        /// GUI显示用，agent不用, statistics Rules页面
        /// </summary>
        [DataMember]
        public NodeLevel NodeLevel { set; get; }

        /// <summary>
        /// GUI显示用，agent不用
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Detail { set; get; }

        /// <summary>
        /// GUI显示用，agent不用
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string IncludeNew { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public Detail Details { set; get; }

        /// <summary>
        /// for scheduled rule
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public RuleType Type { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public byte[] FileVEO { get; set; } //add for RevIM export 客户自定义时使用

        [DataMember(EmitDefaultValue = false)]
        public byte[] RecordVEO { get; set; } //add for RevIM export 客户自定义时使用

        [DataMember(EmitDefaultValue = false)]
        public byte[] ManifestVEO { get; set; } //add for RevIM export 客户自定义时使用

        [DataMember(EmitDefaultValue = false)]
        public byte[] VEOContent { get; set; } //add for RevIM export 客户自定义时使用

        [DataMember(EmitDefaultValue = false)]
        public byte[] VEOHistory { get; set; } //add for RevIM export 客户自定义时使用

        [DataMember(EmitDefaultValue = false)]
        public byte[] NAAConfigFile { get; set; } //add for RevIM export 客户自定义时使用
        [DataMember(EmitDefaultValue = false)]
        public byte[] NARAConfigFile { get; set; } //add for RevIM export 客户自定义时使用

        [DataMember(EmitDefaultValue = false)]
        public string ExportDataEncryptionKey { get; set; } //add for RevIM export 加密导出数据
        [DataMember(EmitDefaultValue = false)]
        public string ExportDataEncryptionIV { get; set; } //add for RevIM export 加密导出数据
        /// <summary>
        /// GUI填入的关于rule的描述信息
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Description { get; set; }

        /// <summary>
        /// rule type for realtime scheduled archiver
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public ProfileType ProfileType { get; set; }

        /// <summary>
        /// 用于GUI排序
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int Order { get; set; }

        /// <summary>
        /// 用于表示enabled状态
        /// </summary>
        private RuleStatus _ruleStatus = RuleStatus.None;

        [DataMember(EmitDefaultValue = false)]
        public RuleStatus RuleStatus
        {
            get
            {
                return this._ruleStatus;
            }
            set
            {
                if (value != this._ruleStatus)
                {
                    this._ruleStatus = value;
                    NotifyPropertyChanged("RuleStatus");
                }
            }
        }

        /// <summary>
        /// 用于表示GUI页面上rule是否勾选
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public ActionStatus CheckStatus { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsManualApproval { get; set; }
        
        [DataMember(EmitDefaultValue = false)]
        public bool IsGControlManualApproval { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public ReviewType ReviewType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string WorkflowId { get; set; }

        /// <summary>
        /// Archive and remove data from SharePoint keep lastest version count
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int ArchivedLatestVersion { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int ArchiverOnlyLastestVersion { get; set; }

        /// <summary>
        /// delete和keep的三个选项
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int KeepDataOption { get; set; }

        /// <summary>
        /// keep的三个选项
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<TagContentInfo> TagContentInfo { get; set; }

        //Only for exchange
        //[DataMember(EmitDefaultValue = false)]
        //public EXORuleAction EXORuleAction { get; set; }
        /// <summary>
        ///  GUI页面应用于Scehduled Rules ComboBox
        /// </summary>
        public List<int> OrderList { get; set; }

        /// <summary>
        ///  GUI页面应用于IsEabled ComboBox
        /// </summary>
        private bool _isEnabledComboBox = true;
        [DataMember(EmitDefaultValue = false)]
        public int? MoveToAnotherTierType { get; set; }

        public bool IsEnabledComboBox
        {
            get
            {
                return this._isEnabledComboBox;
            }
            set
            {
                if (value != this._isEnabledComboBox)
                {
                    this._isEnabledComboBox = value;
                    NotifyPropertyChanged("IsEnabledComboBox");
                }
            }
        }

        /// <summary>
        ///  GUI页面应用于IsChecked CheckBox
        /// </summary>
        private bool _isCheckedCheckBox = false;

        //[DataMember]
        public bool IsCheckedCheckBox
        {
            get
            {
                return this._isCheckedCheckBox;
            }
            set
            {
                if (value != this._isCheckedCheckBox)
                {
                    this._isCheckedCheckBox = value;
                    NotifyPropertyChanged("IsCheckedCheckBox");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {

            if (PropertyChanged != null)
            {

                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }

        }

        /// <summary>
        /// 用于GUI和Manager传数据
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<RuleNodeContract> ContentDBs { get; set; }

        #region == Scheduled Compression And Encryption ==
        /// <summary>
        /// GUI页面选择的关于压缩的类型
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int Compression { get; set; }

        /// <summary>
        /// GUI页面选择的关于加密的类型
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int Encryption { get; set; }

        /// <summary>
        /// manager端根据Compression和Encryption组装DataSecurity属性给agent端使用，高四位加密，低四位压缩
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int DataSecurity { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string EncryptionInfoId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string EncryptionInfoName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DataEncryptionInfoWrapper EncryptionInfoWrapper { get; set; }

        #endregion

        #region == Archiver Record Center ==
        [DataMember(EmitDefaultValue = false)]
        public MoveToRecordCenterAndDelareSetting MoveToRecordCenterAndDelareSetting { get; set; }
        #endregion

        #region == Archiver Compression And Encryption ==
        [DataMember(EmitDefaultValue = false)]
        public CompressionType ArchiverCompressionType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DataSecurity ArchiverDataSecurity { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public EncryptionMethods EncryptionMethods { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public String DataEncryptionProfileId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public String DataEncryptionProfileName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DataEncryptionInfoWrapper DataEncryptionInfoWrapper { get; set; }

        /// <summary>
        /// The export file format of the export job
        /// GUI显示用
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public SOExportInfo ExportInfo { get; set; }

        /// <summary>
        /// 与agent通信用，保存Vault export格式
        /// 注：由于当ExportInfo==null时ExportType为Autonomy,因此不能用ExportType区分VaultJob和ArchiverJob.
        /// 区分VaultJob和ArchiverJob判断方法:
        /// result.ExportInfo != null && result.ExportInfo.exportType != null
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public ExportTypeValue ExportType { set; get; }

        /// <summary>
        /// 与agent 通信用，保存导出location
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public PhysicalDeviceDto PhysicalDeviceDto { set; get; }
        #endregion

        #region == Storage ==
        /// <summary>
        /// for extender scheduled
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string LogicalDeviceId { get; set; }

        /// <summary>
        /// for extender gui display
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string LogicalDeviceName { get; set; }

        /// <summary>
        /// for archiver
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string StoragePolicyId { get; set; }

        /// <summary>
        /// 用于Archiver做数据给agent发消息
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public StoragePolicyDto StoragePolicyDto { get; set; }
        /// <summary>
        /// for GUI display
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string StoragePolicyName { get; set; }
        /// <summary>
        /// Netapp的StoragePolicy, 是否使用SnapLock的Physical
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool UseSnapLock { get; set; }
        #endregion

        #region == Filter Policy ==
        /// <summary>
        /// for archvier rule define object level
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public PolicyLevel PolicyLevel { get; set; }

        /// <summary>
        ///  GUI将condition的and or表达式传给control,control保存,发给client
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Dictionary<PolicyLevel, string> AndOrExpression { get; set; }

        /// <summary>
        /// manager端更具SOFilter得到Filter发给agent使用
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy> Filters { get; set; }

        /// <summary>
        /// Manager端与GUI使用
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<SOFilterPolicy> SOFilters { get; set; }
        #endregion

        [DataMember(EmitDefaultValue = false)]
        public bool KeepStructrue { get; set; }

        /// <summary>
        /// 这个值只有agent schedule中使用。 不需要声明Attribute.
        /// </summary>
        public bool NotToCheck { get; set; }

        #region for RevIM Online
        /// <summary>
        /// 需要前台显示modifytime
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public long ModifyTime { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public ArchiverSetting ArchiverSetting { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public ArchiverVEOSetting ArchiverVEOSetting { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool DeleteRecords { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IncludeDeleteRecordLabel { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool LockRecordBeforeDestroy { get; set; } = true;
        [DataMember(EmitDefaultValue = false, Name = "DeleteSiteCollectionToRecycleBin")]
        private bool? deleteSiteCollectionToRecycleBin { get; set; } = false;

        public bool DeleteSiteCollectionToRecycleBin
        {
            get
            {
                if (deleteSiteCollectionToRecycleBin == null) return true;
                return deleteSiteCollectionToRecycleBin.Value;
            }
            set => deleteSiteCollectionToRecycleBin = value;
        }
        [DataMember(EmitDefaultValue = false)]
        public bool DeleteToRecycleBin { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool DeclareLinkFile { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string DisposalClass { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsSendEamilToOwner { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<UserInfo> UserInfos { get; set; }
        #endregion
        [DataMember(EmitDefaultValue = false)]
        public Rule EXORule { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string EXORuleString { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Rule FSRule { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string FSRuleString { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Rule SPLocalRule { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string SPLocalRuleString { get; set; }
        /// <summary>
        /// Records Physical
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Rule PhysicalRule { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Rule OneDriveRule { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string OneDriveRuleString { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Rule AzureFileRule { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string AzureFileRuleString { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Rule BoxRule { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string BoxRuleString { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Rule ConnectorRule { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Rule GoogleDriveRule { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Rule TeamsRule { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string ConnectorRuleString { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public RelatedRecordOption RelatedRecordOption { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public MoveOption spMoveOption { get; set; }

        /// <summary>
        /// Records Physical Delete folder parent Box.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool IsDeleteParentBox { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsDeleteParentFolder { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string LeaveStubMessage { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public LeaveStubType LeaveStubType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool LeaveStubIsEnabledRetention { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int LeaveStubRetentionValue { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public DateUnit LeaveStubRetentionUnit { get; set; }
        /// <summary>
        /// IsLeaveStubRemoveMetadata only used in DAO, currently OPUS does not use.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public bool IsLeaveStubRemoveMetadata { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsArchivedTier { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsFileName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsFilePath { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsArchivedDate { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsRuleName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsRestoreLink { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsEnableRetention { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public RetentionInfo RetentionInfo { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public DeclareStubType DeclareStubOption { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int KeepLatestMajorAndMinorVersion { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int KeepLatestMajorAndMinorVersionAndArchiveOthers { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string StubTemplateId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string StubTemplateName { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string AOSPStubContent { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int AOSPStubType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsEnableStoreContentRetention { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public List<RetentionRule> StoreContentRetentionInfos { set; get; }
        public bool NeedRecordStubId { get; set; }
        [DataMember]
        public bool IsCalculationDisposalDate { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MoveMetadataInfo
    {
        [DataMember(EmitDefaultValue = false)]
        public string ExoColumn { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string SPColumn { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public MoveEXODataTypeValue DataType { get; set; }

    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RetentionInfo
    {
        [DataMember(EmitDefaultValue = false)]
        public string ColumnName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public TimeFilterCondition Condition { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public TimeUnit KeepDateUnite { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int KeepDateNumber { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public long Date { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsManualApproval { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public ReviewType ReviewType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string WorkflowId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsSendEamilToOwner { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<UserInfo> UserInfos { get; set; }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UserInfo
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string TenantId { get; set; }
        [DataMember]
        public string UserPrincipalName { get; set; }
        [DataMember]
        public InviteType InviteType { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string Email { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RuleType
    {
        [EnumMember]
        NONE = 0,
        [EnumMember]
        ENTERPRISE = 1,
        [EnumMember]
        ADMIN = 2,
        [EnumMember]
        USER = 3
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum MoveEXODataTypeValue
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Text = 1,
        [EnumMember]
        Number = 2,
        [EnumMember]
        Boolean = 3,
        [EnumMember]
        DateTime = 4,
        [EnumMember]
        User = 5
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RuleCollection
    {
        [DataMember]
        public Dictionary<int, Rule> Rules { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Detail
    {
        /// <summary>
        /// rule maybe apply on multi farm
        /// </summary>
        [DataMember]
        //public List<Scope> Scope { get; set; }
        public List<RuleNodeContract> Scopes { get; set; }

        /// <summary>
        /// rule condition
        /// </summary>
        //[DataMember]
        //public string Criteria { get; set; }

        /// <summary>
        /// storage place
        /// </summary>
        //[DataMember]
        //public string Storage { get; set; }

        /// <summary>
        /// rule manager页面使用
        /// </summary>
        [DataMember]
        public List<string> ProfileList { get; set; }
    }

    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public class Scope
    //{
    //    /// <summary>
    //    /// farm name; rule maybe apply on multi farm
    //    /// </summary>
    //    [DataMember]
    //    public string FarmName { get; set; }

    //    /// <summary>
    //    /// full path; children of farm
    //    /// </summary>
    //    [DataMember]
    //    public List<RuleNodeContract> ChildNodes { get; set; }
    //}
    [Flags, DataContract(Namespace = ContractConstants.Namespace)]
    public enum KeepDataOption
    {
        [EnumMember]
        Delete = 0,
        [EnumMember]
        TagContent = 1,
        [EnumMember]
        LeaveOnlyStub = 2,
        [EnumMember]
        DeclareRecord = 4,
        [EnumMember]
        LockConversation = 8,
        [EnumMember]
        Keep = 16,
        [EnumMember]
        Remove = 32,
        [EnumMember]
        LinkDocument = 128,//add for RevIM link a document
        [EnumMember]
        NotBackup = 256,//not backup.
        [EnumMember]
        UndeclaredRecord = 512,
        [EnumMember]
        Archive = 1024,
        [EnumMember]
        ArchiveAndLeaveStub = 2048,
        [EnumMember]
        ArchiveBackupAndRemove = 4096,
        [EnumMember]
        ArchiveBackupAndRemoveLeaveStub = 8192,

        //DAO 中对应4096，目前只有WPP使用，如果后续DAO 4096被广泛使用.需要注意升级时进行转换
        [EnumMember]
        DeleteOnly = 16384,
        [EnumMember]
        KeepLatestVersion = 32768,
        [EnumMember]
        ArchiveLatestVersion = 65536,
        [EnumMember]
        KeepLatestVersionAndArhiveOthers = 131072,
        [EnumMember]
        IsEnableRemoveRetentionLabel = 262144,
        [EnumMember]
        ArchiverOnly = 524288,
        [EnumMember]
        ArchiveOnlyLastestVersion = 1048576,
        [EnumMember]
        TriggerMicrosoft365Archiving = 2097152

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReviewType
    {
        [EnumMember]
        RecordOwner = 0,
        [EnumMember]
        Workflow = 1
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MoveToRecordCenterAndDelareSetting
    {
        [DataMember(EmitDefaultValue = false)]
        public OperatingSharePointDataMode OperateDataMode { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public DestinationLocationInfo DestinationLocation { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public ContentConflictResolution ContentConflictResolution { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public UseTransferedFileMode UseTransferedFileMode { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool OriginalMetaDataAsXML { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool DelaredRecord { get; set; }//add for Records Online for Not Declare Records.

        [DataMember(EmitDefaultValue = false)]
        public bool KeepFolderStructure { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsMoveVersions { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public bool LeaveLinkInSource { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public bool DeleteSourceItem { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool KeepSourceClassification { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum OperatingSharePointDataMode
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        MoveToRecordCenterAndDelare = 1
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DestinationLocationInfo
    {
        [DataMember(EmitDefaultValue = false)]
        public string Url { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public GoogleDriveTreeNodeDto GoogleTreeNode { set; get; }
        
        [DataMember(EmitDefaultValue = false)]
        public string UserName { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Password { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ServiceAccountName { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public BposInfo BposInfo { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string ContainerId { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string DestinationId { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ContentConflictResolution
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Skip = 1,
        [EnumMember]
        Overwrite = 2,
        [EnumMember]
        Append = 3
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum UseTransferedFileMode
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        KeepOriginalContentType,
        [EnumMember]
        IsAutoMatchContentType,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SOExportInfo
    {
        [DataMember(EmitDefaultValue = false)]
        public ExportTypeValue exportType { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public ExportSPDataOption exportSPDataOption { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string exportLocationId { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string exportLocationName { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public MoveOption spMoveOption { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool newOptionsOfExportInfo{ get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ExportTypeValue
    {
        [EnumMember]
        None = -1,
        [EnumMember]
        Autonomy = 0,
        [EnumMember]
        Concordance = 1,
        [EnumMember]
        EDRM = 2,
        [EnumMember]
        VEO = 3,
        [EnumMember]
        NAA = 4,
        [EnumMember]
        NARA = 5
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ExportSPDataOption
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        ExportBeforeArchive = 1,
        [EnumMember]
        ExportWithoutArchive = 2,
    }

    /// <summary>
    /// Archiver readonly stub
    /// 保存Tag the content信息
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class TagContentInfo
    {
        [DataMember(EmitDefaultValue = false)]
        public TagContentInfoType Type { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ColumnName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Value { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public DateTime DateTime { get; set; }
        [DataMember]
        public int Option { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum TagContentInfoType
    {
        [EnumMember]
        Text = 1,
        [EnumMember]
        Number = 2,
        [EnumMember]
        DateTime = 3,
        [EnumMember]
        Boolean = 4,
        [EnumMember]
        Archived = 5,
        [EnumMember]
        ArchivedBy = 6,
        [EnumMember]
        ArchivedDate = 7,
        [EnumMember]
        RetentionLabel = 8,
        [EnumMember]
        SensitivityLabel = 9
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RetentionLabelOptions
    {
        [EnumMember]
        Default = 0,
        [EnumMember]
        GetFromGeneralSetting = 1
    }

    //这个枚举值，只决定了Rule下面选择的Action 是什么，具体细节做什么不通过此属性控制
    //[DataContract(Namespace = ContractConstants.Namespace)]
    //[Flags]
    //public enum EXORuleAction
    //{
    //    [EnumMember]
    //    None = 0,//考虑到老数据升级，Delete Action 必须是0
    //    [EnumMember]
    //    Remove = 1,
    //    [EnumMember]
    //    Backup = 2,
    //    [EnumMember]
    //    Move = 4,
    //    [EnumMember]
    //    Export = 8,
    //    [EnumMember]
    //    Tag = 16,
    //    [EnumMember]
    //    Declare = 32,
    //    [EnumMember]
    //    LeaveLink = 64,
    //}
    /// <summary>
    /// for merge veo job
    /// 保存起merge veo job时需要用到的配置信息
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverSetting
    {
        [DataMember]
        public int NumberOfThreadSendingEmail { get; set; }
        [DataMember]
        public bool EnableArchiverVEOMerge { get; set; }
        [DataMember]
        public bool IsDeleteOldFile { get; set; }
        [DataMember]
        public double FileSize { get; set; }
        [DataMember]
        public int FileNumber { get; set; }
        [DataMember]
        public string FolderName { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverVEOSetting
    {
        [DataMember]
        public string AgencyId { get; set; }
        [DataMember]
        public string SeriesNumber { get; set; }
        [DataMember]
        public string SeriesIdentifier { get; set; }
        [DataMember]
        public string ConsignmentNumber { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RelatedRecordOption
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Both = 1
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    #region add for records new rule design move to settings
    public class MoveOption
    {
        [DataMember(EmitDefaultValue = false)]
        public RecordFlag SourceFlag { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public RecordFlag DestFlag { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public MoveRecordSetting MoveSetting { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public MoveDestination MoveDestination { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsMoveToSP { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public List<MoveMetadataInfo> MoveToSPDataList { set; get; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RecordFlag
    {
        [EnumMember]
        None = -1,
        [EnumMember]
        SP = 1,
        [EnumMember]
        FS = 2,
        /// <summary>
        /// Only use in Records
        /// </summary>
        [EnumMember]
        Physical = 3,
        [EnumMember]
        EXO = 4,
        [EnumMember]
        SPLocal = 5,
        [EnumMember]
        AzureFile = 7,
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MoveRecordSetting
    {
        [DataMember(EmitDefaultValue = false)]
        public ConflictType ConflictType { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public ConflictOption ContainerLevelConflictOption { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public ConflictOption ItemLevelConflictOption { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool FolderInherit { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool FolderUnderInherit { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool FileInherit { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public FilePropertiesMapping FilePropertiesMapping { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public PhysicalHoldConflictOption PhysicalHoldConflictOption { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ConflictType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        SharePointConflict = 1,
        [EnumMember]
        FileSystemConflict = 2,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PhysicalHoldConflictOption
    {
        [EnumMember]
        UseDesDefinedHoldSetting = 1,
        [EnumMember]
        CompareHoldSetting = 2,
    }

    [DataContract]
    public enum ConflictOption
    {
        [EnumMember]
        Skip,
        [EnumMember]
        NotOverwrite,
        [EnumMember]
        AppendByName,
        [EnumMember]
        AppendByVersion,
        [EnumMember]
        Overwrite,
        [EnumMember]
        Replace,
        [EnumMember]
        Merge,
        [EnumMember]
        OverwriteByLastModifiedTime
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum NameConflictOption
    {
        [EnumMember]
        Merge = 1,
        [EnumMember]
        Skip = 2,
        [EnumMember]
        Rename = 3
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MoveDestination
    {
        [DataMember(EmitDefaultValue = false)]
        public DestMode DestMode { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public SPTreeNodeDto SPTreeNode { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public GoogleDriveTreeNodeDto GoogleTreeNode { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public FSTreeNodeDto FSTreeNode { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public PhysicalDestTree PhysicalTree { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public string SPUrl { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string DestinationId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public string ContainerId { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public string SPAccountProfileId { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public Office365AccountInfo SPAccount { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public string FSPath { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string FSConectionPath { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public string FSAccountProfileId { set; get; }

        //[DataMember(EmitDefaultValue = false)]
        //public AccountProfileDto FSAccount { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string FSTreeStr { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string SPTreeStr { get; set; }
        
        [DataMember(EmitDefaultValue = false)]
        public string GoogleTreeStr { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string PhysicalTreeStr { set; get; }

        [DataMember(EmitDefaultValue = false)]
        public bool NotDeclareMovedData { get; set; }
        //add for EXO
        [DataMember(EmitDefaultValue = false)]
        public bool DeleteSourceItem { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool KeepSourceClassification { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool KeepFolderStructure { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsMoveVersions { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DestMode
    {
        [EnumMember]
        TreeMode = 0,
        [EnumMember]
        UrlMode = 1
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PhysicalDestTree
    {
        [DataMember(EmitDefaultValue = false)]
        public string LocationId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string FullPath { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string BoxId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string FileId { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FilePropertiesMapping
    {
        [DataMember(EmitDefaultValue = false)]
        public List<PropertiesMappingItem> PropertiesMappingItems { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PropertiesMappingItem
    {
        [DataMember(EmitDefaultValue = false)]
        public string FileSystemProperty { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string SharePointProperty { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public ColumnType ColumnType { get; set; }

        //For the Lookup and MetadataColumn, we may support them later, so leave it here, 
        //If we need to support them, we can refer to the common contract : common\ModuleContract\Migration\Migration.Contract\Contract\Object\File\FileMigrationMappingsSubProfileContent.cs
        //[DataMember]//only used when ColumnType is Lookup
        //public Lookup Lookup { get; set; }

        //[DataMember]//only used when ColumnType is MetadataColumn
        //public MetadataColumn MetadataColumn { get; set; }
    }

    /// <summary>
    /// sharepoint column type
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ColumnType
    {
        [EnumMember]
        [Description("Invalid")]
        Invalid = 0,

        [Description("Single line of text")]
        [EnumMember]
        Text = 1,

        [Description("Multiple lines of text")]
        [EnumMember]
        Note = 2,

        [Description("Multiple lines of text_Plain text")]
        [EnumMember]
        PlainText = 3,

        [Description("Multiple lines of text_Rich text")]
        [EnumMember]
        RichText = 4,

        [Description("Multiple lines of text_Enhanced rich text")]
        [EnumMember]
        EnhancedRichText = 5,

        [Description("Choice_Checkboxes(allow multiple selections)")]
        [EnumMember]
        CheckBoxChoice = 6,

        [Description("Choice_Drop-Down Menu")]
        [EnumMember]
        DropDownChoice = 7,

        [Description("Choice_Radio Buttons")]
        [EnumMember]
        RadioChoice = 8,

        [Description("Number")]
        [EnumMember]
        Number = 9,

        [Description("Date and Time_Date Only")]
        [EnumMember]
        DateOnly = 10,

        [Description("Date and Time_Date & Time")]
        [EnumMember]
        DateAndTime = 11,

        [Description("Yes/No")]
        [EnumMember]
        Boolean = 12,

        [Description("Person or Group")]
        [EnumMember]
        User = 13,

        [Description("Managed Metadata")]
        [EnumMember]
        MetadataColumn = 14,

        [Description("Lookup")]
        [EnumMember]
        Lookup = 15,

        [Description("MultiChoice")]
        [EnumMember]
        MultiChoice = 16,

        [EnumMember]//don't use this value, it is about to be deleted.
        Choice = 18,

        [EnumMember]//don't use this value, it is about to be deleted.
        DateTime = 19,

        [Description("HyperLink")]
        [EnumMember]
        HyperLinkOrPicture = 20,

        [Description("Number_Show as percentage(for example, 50%)")]
        [EnumMember]
        PercentNumber = 21,

        [Description("Currency")]
        [EnumMember]
        CurrencyNumber = 22,

        [Description("All Day Event")]
        [EnumMember]
        AllDayEvent = 23,

        [Description("Calculated")]
        [EnumMember]
        Calculated = 24,
    }
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum LeaveStubType
    {
        [EnumMember]
        None = -1,
        [EnumMember]
        Aspx = 0,
        [EnumMember]
        Txt = 1,
        [EnumMember]
        Html = 2,
        [EnumMember]
        Link = 3,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DeclareStubType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Declare = 1,
        [EnumMember]
        UnDeclare = 2,
        [EnumMember]
        AddRecordLabel = 3,
        [EnumMember]
        DeleteRecordLabel = 4
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RuleModel
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Records = 1,
        [EnumMember]
        SOArchiver = 2
    }
}
