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
using AvePoint.Api.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace DocAveOnline.WebApi.Contracts
{

    public class ArchiverDataBaseConfigInfo : BaseContract
    {
        public string AccountName { get; set; }
        public string AccountKey { get; set; }
        public string Endpoint { get; set; }
    }


    public class RetentionInfo
    {

        public string ColumnName { get; set; }

        public int Condition { set; get; }
        public int KeepDateUnite { get; set; }

        public int KeepDateNumber { get; set; }

        public long Date { set; get; }

        public bool IsManualApproval { get; set; }

        public ReviewType ReviewType { get; set; }

        public string WorkflowId { get; set; }

        public bool IsSendEamilToOwner { get; set; }

        public List<UserInfo> UserInfos { get; set; }

    }

    public enum ReviewType
    {
        [EnumMember]
        RecordOwner = 0,
        [EnumMember]
        Workflow = 1
    }

    public class RecordsStorageInfo
    {
        //public DataSecurity ArchiverDataSecurity { get; set; }
        //public CompressionType ArchiverCompressionType { get; set; }
        public String DataEncryptionProfileId { get; set; }
        public String DataEncryptionProfileName { get; set; }
        public string StoragePolicyId { get; set; }
        public string StoragePolicyName { get; set; }
        public string ExportLocationId { set; get; }
        public string ExportLocationName { set; get; }
        public byte[] FileVEO { get; set; }
        public byte[] RecordVEO { get; set; }
        public byte[] ManifestVEO { get; set; }
        public byte[] NAAConfigFile { get; set; }
        public byte[] NARAConfigFile { get; set; }
        public string ExportDataEncryptionKey { get; set; }
        public string ExportDataEncryptionIV { get; set; }
        public ArchiverSetting ArchiverSetting { get; set; }
        public ArchiverVEOSetting ArchiverVEOSetting { get; set; }
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

        //public DataSecurity CompressionMethod { get; set; }

        //public DataSecurity EncryptionMethod { get; set; }

        public string Extentions { get; set; }
    }


    public class CosmosConnectionInfo
    {
        public string Endpoint { get; set; }
        public string Key { get; set; }
        public string DatabaseId { get; set; }
        public string CollectionId { get; set; }
    }
    public class TagContentInfo
    {
        public TagContentInfoType Type { get; set; }
        public string ColumnName { get; set; }
        public string Value { get; set; }
        public DateTime DateTime { get; set; }
    }
    public class MoveToRecordCenterAndDelareSetting
    {
        public OperatingSharePointDataMode OperateDataMode { get; set; }
        public DestinationLocationInfo DestinationLocation { get; set; }
        public ContentConflictResolution ContentConflictResolution { get; set; }
        public UseTransferedFileMode UseTransferedFileMode { get; set; }
        public bool OriginalMetaDataAsXML { get; set; }
        public bool DelaredRecord { get; set; }
        public bool LeaveLinkInSource { set; get; }
    }
    public class DestinationLocationInfo
    {
        public string Url { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    #region add for records new rule design move to settings

    public enum RecordFlag
    {
        [EnumMember]
        None = -1,
        [EnumMember]
        SP = 1,
        [EnumMember]
        FS = 2,
        [EnumMember]
        Physical = 3,
        [EnumMember]
        EXO = 4,
        [EnumMember]
        SPLocal = 5,
        [EnumMember]
        AzureFile = 7,
        [EnumMember]
        Connector = 999
    }
    public class MoveRecordSetting
    {
        public ConflictType ConflictType { get; set; }
        public ConflictOption ContainerLevelConflictOption { get; set; }
        public ConflictOption ItemLevelConflictOption { get; set; }
        public bool FolderInherit { get; set; }
        public bool FolderUnderInherit { get; set; }
        public bool FileInherit { get; set; }
        public FilePropertiesMapping FilePropertiesMapping { get; set; }
        public PhysicalHoldConflictOption PhysicalHoldConflictOption { get; set; }
    }

    public enum ConflictType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        SharePointConflict = 1,
        [EnumMember]
        FileSystemConflict = 2,
    }

    [DataContract]
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

    public enum DestMode
    {
        [EnumMember]
        TreeMode = 0,
        [EnumMember]
        UrlMode = 1
    }
    public class PhysicalDestTree
    {
        public string LocationId { get; set; }
        public string FullPath { get; set; }
        public string BoxId { get; set; }
        public string FileId { get; set; }
    }
    public class FilePropertiesMapping
    {
        [DataMember(EmitDefaultValue = false)]
        public List<PropertiesMappingItem> PropertiesMappingItems { get; set; }
    }

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
    public enum ColumnType
    {
        Invalid = 0,
        Text = 1,
        Note = 2,
        PlainText = 3,
        RichText = 4,
        EnhancedRichText = 5,
        CheckBoxChoice = 6,
        DropDownChoice = 7,
        RadioChoice = 8,
        Number = 9,
        DateOnly = 10,
        DateAndTime = 11,
        Boolean = 12,
        User = 13,
        MetadataColumn = 14,
        Lookup = 15,
        MultiChoice = 16,
        Choice = 18,
        DateTime = 19,
        HyperLinkOrPicture = 20,
        PercentNumber = 21,
        CurrencyNumber = 22,
        AllDayEvent = 23,
        Calculated = 24,
    }
    #endregion
    public class SOExportInfo
    {
        public ExportTypeValue exportType { set; get; }
        public ExportSPDataOption exportSPDataOption { set; get; }
        public string exportLocationId { set; get; }
        public string exportLocationName { set; get; }
    }
    public class SOFilterPolicy : FilterPolicy
    {
        public bool IsAnd { get; set; }
        public DisplayDateTime BeginTime { get; set; }
        public DisplayDateTime EndTime { get; set; }
    }
    public class DisplayDateTime
    {
        public string StartTime { get; set; }
        public string TimeZoneId { get; set; }
        public bool IsDayLightSaving { get; set; }
    }
    [DataContract]
    public class AdvanceSearchCondition
    {
        [DataMember]
        public string Keyword { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Content { get; set; }
        [DataMember]
        public string MetadataInfo { get; set; }
        [DataMember]
        public string FolderNameOrPath { get; set; }
        [DataMember]
        public string Scope { get; set; }
        [DataMember]
        public long CreatedDateFrom { get; set; }
        [DataMember]
        public long CreatedDateTo { get; set; }

        [DataMember]
        public long ArchivedDateFrom { get; set; }
        [DataMember]
        public long ArchivedDateTo { get; set; }

        [DataMember]
        public string CreatedBy { get; set; }
        [DataMember]
        public string ModifiedBy { get; set; }
        [DataMember]
        public string Office365TenantID { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public List<string> Ids { get; set; }
        [DataMember]
        public Office365User User { get; set; }
        [DataMember]
        public Office365User Group { get; set; }
        [DataMember]
        public ModuleType ModuleType { get; set; }
        [DataMember]
        public int Page { get; set; }
        [DataMember]
        public int Size { get; set; }
        [DataMember]
        public Order Order { get; set; }
        [DataMember]
        public String OrderBy { get; set; }
        [IgnoreDataMember]
        public string SiteId { get; set; }
        [IgnoreDataMember]
        public string SiteUrl { get; set; }
        [DataMember]
        public int CategoryId { get; set; }
        [DataMember]
        public string ContinuationToken { get; set; }

        [DataMember]
        public int PolicyLevel { get; set; }

        [DataMember]
        public bool IsAOSPSearch { get; set; }

        [DataMember]
        public bool IsShowTotalCount { get; set; }
        public override string ToString()
        {
            return $"Keyword: {Keyword},Name: {Name},Content: {Content}, MetadataInfo: {MetadataInfo}, FolderNameOrPath: {FolderNameOrPath}, Scope: {Scope}, CreatedDateFrom: {CreatedDateFrom}, CreatedDateTo: {CreatedDateTo}, CreatedBy: {CreatedBy}, ModifiedBy: {ModifiedBy}, ModuleType: {ModuleType}, SiteId: {SiteId}, Id: {Id}, Ids: {string.Join("|", Ids ?? new List<string>())}, page: {Page}, Size: {Size}, Order: {Order}, OrderBy: {OrderBy}, CategoryId: {CategoryId}, ContinuationToken: {ContinuationToken}";
        }
    }



    [DataContract]
    public class Office365User
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Name { get; set; }
    }

    [DataContract]
    public class SearchResult : BaseContract
    {
        [DataMember]
        public string SiteTitle { get; set; }
        [DataMember]
        public string SiteUrl { get; set; }
        [DataMember]
        public NodeType NodeType { get; set; }
        [DataMember]
        public bool HasNext { get; set; }
        [DataMember]
        public int CategoryId { get; set; }
        [DataMember]
        public string ContinuationToken { get; set; }
        [DataMember]
        public List<AdvanceSearchResult> AdvanceSearchResults { get; set; }
    }
    [DataContract]
    public class AdvanceSearchResult
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string FullPath { get; set; }
        [DataMember]
        public string PathMD5 { get; set; }
        [DataMember]
        public long CreateTime { get; set; }
        [DataMember]
        public string ModifiedBy { get; set; }
        [DataMember]
        public string AbsolutePath { get; set; }
        [DataMember]
        public long ContentLenth { get; set; }
        [DataMember]
        public long ModifiedTime { get; set; }
        [DataMember]
        public long ArchiveTime { get; set; }
        [DataMember]
        public bool IsArchiveTier { get; set; }
        [DataMember]
        public Guid ItemId { get; set; }
    }
    [DataContract]
    public class StubParseResult : BaseContract
    {
        [DataMember]
        public string BackUpJobId { get; set; }
        [DataMember]
        public string SiteUrl { get; set; }
        [DataMember]
        public string Office365UserID { get; set; }
        [DataMember]
        public string Office365TenantID { get; set; }
        [DataMember]
        public string StubType { get; set; }
        [DataMember]
        public string StubId { get; set; }
        [DataMember]
        public AdvanceSearchResult AdvanceSearchResult { get; set; }
        [DataMember]
        public string FileSize { get; set; }

        #region end user restore setting
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
        public bool IsRestoreStubLink { get; set; }
        [DataMember]
        public bool IsExportStubLink { get; set; }
        [DataMember]
        public bool IsArchiveTier { get; set; }
        [DataMember]
        public int StubProductSource { get; set; }
        #endregion
    }
    [DataContract]
    public class EndUserRestoreConfig
    {
        [DataMember]
        public SearchResult SearchJobInfo { get; set; }
        [DataMember]
        public StubParseResult StubJobInfo { get; set; }
        [DataMember]
        public string Office365User { get; set; }
        [DataMember]
        public ModuleType ModuleType { get; set; }
        [DataMember]
        public Office365User Office365GroupInfo { get; set; }
        [DataMember]
        public string Office365TenantID { get; set; }
        [DataMember]
        public string OopStubUrl { get; set; }
        [DataMember]
        public bool IsExportAllSearchResult { get; set; }
        [DataMember]
        public AdvanceSearchCondition searchCondition { get; set; }
        [DataMember]
        public string JobId { get; set; }
        [DataMember]
        public string RunJobUser { get; set; }
    }
    [DataContract]
    public class ParseStubParameters
    {
        [DataMember]
        public string StubString { get; set; }
        [DataMember]
        public string Office365UserID { get; set; }
        [DataMember]
        public string Office365TenantId { get; set; }
    }
    [DataContract]
    public class ArchivedContentRestoreConfig : BaseContract
    {
        [DataMember]
        public string SiteUrl { get; set; }
        [DataMember]
        public string RestoreStorage { get; set; }
        [DataMember]
        public List<ArchivedContentInfo> ArchivedContentInfos { get; set; }
    }
    [DataContract]
    public class ExportArchivedContentConfig : BaseContract
    {
        [DataMember]
        public string SiteUrl { get; set; }
        [DataMember]
        public string Office365UserMail { get; set; }
        [DataMember]
        public bool IsSearchResultExport { get; set; }
        [DataMember]
        public string Office365TenantID { get; set; }
        [DataMember]
        public ModuleType ModuleType { get; set; }
        [DataMember]
        public Office365User Office365GroupInfo { get; set; }
        [DataMember]
        public List<ExportArchivedDataInfo> ExportContentInfos { get; set; }
        [DataMember]
        public string StubType { get; set; }
    }
    [DataContract]
    public class ArchivedContentInfo
    {
        [DataMember]
        public string BackUpJobId { get; set; }
        [DataMember]
        public string PathMD5 { get; set; }
        [DataMember]
        public string FileUrl { get; set; }
        [DataMember]
        public string ExtensionString { get; set; }
    }
    [DataContract]
    public class ExportArchivedDataInfo
    {
        [DataMember]
        public string BackUpJobId { get; set; }
        [DataMember]
        public string PathMD5 { get; set; }
        [DataMember]
        public string FullPath { get; set; }
    }
    [DataContract]
    public class ExportJobInfo
    {
        [DataMember]
        public string ExportJobId { get; set; }

        //using for verify run job user
        [DataMember]
        public string Office365UserMail { get; set; }
        [DataMember]
        public bool IsDownload { get; set; }
        [DataMember]
        public bool IsStub { get; set; }
    }
    [DataContract]
    public class ExportedDataResult
    {
        [DataMember]
        public string DataSASString { get; set; }
        [DataMember]
        public string ZipPassword { get; set; }
    }

    [DataContract]
    public class MigrationJobReportSASResult : BaseContract
    {
        [DataMember]
        public string SasUri { get; set; }
        [DataMember]
        public DateTime Expired { get; set; }
    }


    public class ArchiverSetting
    {
        public int NumberOfThreadSendingEmail { get; set; }
        public bool EnableArchiverVEOMerge { get; set; }
        public bool IsDeleteOldFile { get; set; }
        public double FileSize { get; set; }
        public int FileNumber { get; set; }
        public string FolderName { get; set; }
    }
    public class ArchiverVEOSetting
    {
        public string AgencyId { get; set; }
        public string SeriesNumber { get; set; }
        public string SeriesIdentifier { get; set; }
        public string ConsignmentNumber { get; set; }
    }
    public class RuleNodeExtension
    {
        public string ContentDatabaseName { get; set; }
        public bool UsedcrawlProfile { get; set; }
        public bool IsUpgradeData { get; set; }
    }

    public class FilterPolicy
    {
        public int SequenceNo { get; set; }
        public PolicyLevel Level { get; set; }
        public PolicyRuleType RuleType { get; set; }
        public PolicyRuleBase Rule { get; set; }
        public PolicyCondition Condition { get; set; }
        public PolicyValue Value { get; set; }
        /// <summary>
        /// result field is an extension used for supporting rule that common filter engine can't evaluate.
        /// like CA UserAndGroup rule. common filter engine user is responsible for evaluate the policy result.
        /// and put the evaluation result into this filed.
        /// </summary>
        public Nullable<bool> Result { get; set; }
        public RuleGUIType RuleGUIType { get; set; }

        public override string ToString()
        {
            //SAAS-12633 重新写filter toString()方法、
            StringBuilder filterString = new StringBuilder();
            filterString.AppendFormat("RuleType:{0},", this.RuleType.ToString());
            filterString.AppendFormat("SequenceNo:{0},", this.SequenceNo.ToString());
            //filterString.AppendFormat("Level:{0},", this.Level.ToString());
            filterString.AppendFormat("Condition:{0},", this.Condition.ToString());
            if (this.Value != null)
            {
                if (!string.IsNullOrEmpty(Value.Value1))
                {
                    filterString.AppendFormat("Value1:{0},", this.Value.Value1);
                }
                if (!string.IsNullOrEmpty(Value.Value2))
                {
                    filterString.AppendFormat("Value2:{0}", this.Value.Value2);
                }
            }
            return filterString.ToString();
        }
    }

    public class PolicyValue
    {
        private string value1;
        private PolicyValueUnit value1Unit;
        private string value2;
        private PolicyValueUnit value2Unit;

        public string Value1
        {
            get { return value1; }
            set { value1 = value; }
        }
        public PolicyValueUnit Value1Unit
        {
            get { return value1Unit; }
            set { value1Unit = value; }
        }
        public string Value2
        {
            get { return value2; }
            set { value2 = value; }
        }
        public PolicyValueUnit Value2Unit
        {
            get { return value2Unit; }
            set { value2Unit = value; }
        }
        public Extention Extension { get; set; }
        public PolicyValue()
            : this(string.Empty)
        {
        }
        public PolicyValue(string value1)
            : this(value1, string.Empty)
        {
        }
        public PolicyValue(string value1, string value2)
            : this(value1, PolicyValueUnit.None, value2, PolicyValueUnit.None)
        {
        }
        public PolicyValue(string value1, PolicyValueUnit unit1)
            : this(value1, unit1, string.Empty, PolicyValueUnit.None)
        {
        }
        public PolicyValue(string value1, PolicyValueUnit unit1, string value2, PolicyValueUnit unit2)
        {
            this.value1 = value1;
            this.value1Unit = unit1;
            this.value2 = value2;
            this.value2Unit = unit2;

        }
    }

    [KnownType(typeof(SecurityFilterPolicy))]
    public class Extention
    {
        /// <summary>
        /// CA用于存储时间类型的Filter的时区ID
        /// </summary>
        [DataMember]
        public string TimeZoneId { get; set; }
        /// <summary>
        /// 保存夏令时
        /// </summary>
        [DataMember]
        public bool isDST { get; set; }
    }


    public class SecurityFilterPolicy : Extention
    {
        //policy role: user and group; policy condition: contains


        public string LoginName { get; set; }

        public SearchForPermissionOption Permission { get; set; }

        public bool ExtractPermission { get; set; }

        public string CustomizedPermissionLevels { get; set; }
    }
    [DataContract]
    public class UserInfo
    {
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string UserPrincipalName { get; set; }
        [DataMember]
        public InviteType InviteType { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string Email { get; set; }
    }
    [DataContract]
    public class Microsoft365Group
    {
        [DataMember]
        public String Id { get; set; }
        [DataMember]
        public String DisplayName { get; set; }
        [DataMember]
        public String GroupName { get; set; }
    }
    [DataContract]
    public class Microsoft365User
    {
        [IgnoreDataMember]
        public string UserName { get; set; }
        [DataMember]
        public string UserEmail { get; set; }
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string TenantId { get; set; }
        [DataMember]
        public string StubId { get; set; }
    }
    [DataContract]
    public class PreviewDataParam
    {
        [DataMember]
        public string FullPath{ get; set; }
        [DataMember]
        public string CycleId{ get; set; }
        [DataMember]
        public string JobId { get; set; }
        [DataMember]
        public string UserId { get; set;}//logon in userid
        [DataMember]
        public string IndexParam{ get; set; }
        [DataMember]
        public string SitePath { get; set; }
        [DataMember]
        public string PathMd5 { get; set; }
    }
    public enum SearchForPermissionOption
    {

        SearchForAnyPermission = 0,

        FullControl = 1,

        Design = 2,

        Contribute = 3,

        Read = 4,

        ViewOnly = 5,

        LimitedAccess = 6,

        InputedPermission = 7,

        Edit = 8,

        Administrator = 9,

        CustomizedPermissionLevels = 10,
    }

    public enum RuleType
    {
        NONE = 0,
        ENTERPRISE = 1,
        ADMIN = 2,
        USER = 3
    }
    public enum RuleStatus
    {
        None = 0,
        Enabled = 1,
        Disabled = 2
    }
    public enum ActionStatus
    {
        Disable = 0,
        Enable = 1,
        Not_Collected = -1
    }
    public enum TagContentInfoType
    {
        Text = 1,
        Number = 2,
        DateTime = 3,
        Boolean = 4,
        Archived = 5,
        ArchivedBy = 6,
        ArchivedDate = 7,
        RetentionLabel = 8
    }
    public enum OperatingSharePointDataMode
    {
        None = 0,
        MoveToRecordCenterAndDelare = 1
    }
    public enum ContentConflictResolution
    {
        None = 0,
        Skip = 1,
        Overwrite = 2,
        Append = 3
    }
    public enum UseTransferedFileMode
    {
        None = 0,
        KeepOriginalContentType,
        IsAutoMatchContentType,
    }

    [Flags]
    public enum ExportTypeValue
    {
        None = -1,
        Autonomy = 0,
        Concordance = 1,
        EDRM = 2,
        VEO = 3,
        NAA = 4,
        NARA = 5,
    }
    public enum ExportSPDataOption
    {
        None = 0,
        ExportBeforeArchive = 1,
        ExportWithoutArchive = 2,
    }
    public enum RuleNodeType
    {
        RealTime = 0,
        Scheduled = 1,
        Archiver = 2,
        IndexDevice = 3,
        Connector = 4,
        EndUserArchiverSetting = 5,
        VaultIndexDevice = 6,
        VaultSettingNode = 7,
        V5ArchiveSiteMasterIndex = 8,
        V5ProviderMapping = 9,
        V5ExtenderIndexDevice = 10,
    }

    public enum PolicyValueUnit
    {

        None,

        KB,

        MB,

        GB,

        Days,

        Weeks,

        Months,

        Years
    }
    public enum PolicyRuleType
    {
        None = 0,

        ResultLevel = 1,

        Url = 2,

        Title = 4,

        Name = 8,

        Template = 16,

        CreatedBy = 32,

        CreatedTime = 64,

        ModifiedTime = 128,

        Owner = 256,

        Inheritance = 512,

        Permission = 1024,

        Attribute = 2048,

        FullTextIndex = 4096,
        //for auditor

        Country = 8192,

        //Add for CA

        UserAndGroup = 16384,

        ContentType = 32768,

        Versions = 65536,

        Auditing = 131072,

        Versioning = 262144,

        CustomProperty = 524288,

        AnonymousAccess = 1048576,

        LockStatus = 2097152,

        Size = 4194304,

        Category = 8388608,

        SendDate = 16777216,

        Attachment = 33554432,

        SendFrom = 67108864,

        SendTo = 134217728,

        //Add for Granular Object-based Restore

        ModifiedBy = 268435456
    }

    public enum PolicyCondition
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Exactly = 1,
        [EnumMember]
        StartWith = 2,
        [EnumMember]
        EndWith = 4,
        [EnumMember]
        Contains = 8,
        [EnumMember]
        LessOrEqualThan = 16,
        [EnumMember]
        GreaterOrEqualThan = 32,
        [EnumMember]
        OnlyLastNVersions = 64,
        [EnumMember]
        OnlyLastMajorNVersions = 128,
        [EnumMember]
        OnlyMajorVersions = 256,
        [EnumMember]
        OnlyMionrVersions = 512,
        [EnumMember]
        OnlyApproved = 1024,
        [EnumMember]
        FromTo = 2048,
        [EnumMember]
        Before = 4096,
        [EnumMember]
        After = 8192,
        [EnumMember]
        On = 16384,
        [EnumMember]
        WithIn = 32867,
        [EnumMember]
        OlderThan = 65734,
        [EnumMember]
        IsEmpty = 65736,
        [EnumMember]
        ExceptLastNVersions = 131468,
        [EnumMember]
        Equals = 262936,
        [EnumMember]
        DoesNotContains = 525872,
        [EnumMember]
        Match = 1051744,
        [EnumMember]
        DoesNotMatch = 2103488,
        [EnumMember]
        IsExactlyNot = 4206976,
        [EnumMember]
        MajorAndMintorVersions = 8413952,
        [EnumMember]
        DoesNotEquals = 16827904,
        [EnumMember]
        ExceptLastNMajorVersions = 16777216,
        [EnumMember]
        MajorWithoutMinorVersions = 33554432,
        [EnumMember]
        MinorOfEachMajorVersion = 67108864,
        [EnumMember]
        MinorOfTheLatestMajorVersion = 134217728,
    }

    public enum RuleGUIType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        ColumnText = 1,
        [EnumMember]
        CustomPropertyText = 2,
        [EnumMember]
        ColumnNumber = 3,
        [EnumMember]
        CustomPropertyNumber = 4,
        [EnumMember]
        ColumnBoolean = 5,
        [EnumMember]
        CustomPropertyBoolean = 6,
        [EnumMember]
        ColumnDateTime = 7,
        [EnumMember]
        CustomPropertyDateTime = 8,
        [EnumMember]
        Workflow = 9,
        [EnumMember]
        AnonymousAccess = 10,
        [EnumMember]
        Attribute = 11,
        [EnumMember]
        Attachment = 12,
        [EnumMember]
        Auditing = 13,
        [EnumMember]
        Category = 14,
        [EnumMember]
        ContentType = 15,
        [EnumMember]
        CreatedBy = 16,
        [EnumMember]
        Created = 17,
        [EnumMember]
        KeepHistoryVersion = 18,
        [EnumMember]
        ListType = 19,
        [EnumMember]
        ModifiedBy = 20,
        [EnumMember]
        Modified = 21,
        [EnumMember]
        NameAndExtention = 22,
        [EnumMember]
        Name = 23,
        [EnumMember]
        Owner = 24,
        [EnumMember]
        SendDate = 25,
        [EnumMember]
        Size = 26,
        [EnumMember]
        Template = 27,
        [EnumMember]
        Title = 28,
        [EnumMember]
        Url = 29,
        [EnumMember]
        Versions = 30,
        [EnumMember]
        Versioning = 31,
        [EnumMember]
        UserAndGroup = 32,
        [EnumMember]
        Inheritance = 33,
        [EnumMember]
        StubCreationTime = 34,
        [EnumMember]
        StubLastAccessTime = 35,
        [EnumMember]
        TemplateId = 36,
        [EnumMember]
        LockStatus = 37,
    }
    [DataContract]
    public enum InviteType
    {
        [EnumMember]
        User = 0,
        [EnumMember]
        Group = 1,
        [EnumMember]
        UserInGroup = 2
    }
    [DataContract]
    public enum RelatedRecordOption
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Both = 1
    }
    [DataContract]
    public enum ScanModeOption
    {
        [EnumMember]
        Full = 0,
        [EnumMember]
        Quick = 1
    }

    /// <summary>
    /// 1.目前OneDriveForBusiness没用到，因为Archiver数据在Recenter没有One Drive Source.
    /// 2.目前Stub Restore，Stub Export ReCenter传的Module是None.
    /// </summary>
    [DataContract]
    public enum ModuleType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        OneDriveForBusiness = 1,
        [EnumMember]
        SharePointOnline = 2,
        [EnumMember]
        Microsoft365Groups = 3,
        [EnumMember]
        MicrosoftTeams = 4,
    }
    [DataContract]
    public enum Order
    {
        [EnumMember]
        Asc = 0,
        [EnumMember]
        Desc = 1,
    }

    [DataContract]
    public class EndUserRestoreSettingResult : BaseContract
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
        public bool IsAllowRestore { get; set; }
        [DataMember]
        public EndUserPermissionSetting PermissionSetting { get; set; }
    }
    [DataContract]
    public class EndUserPermissionSetting
    {
        [DataMember]
        public TeamsPermissionSetting TeamsAndGroup { get; set; }
        [DataMember]
        public SP365PermissionLevel SiteCollection { get; set; }
        [DataMember]
        public string SiteCollectionSpecialGroupNames { get; set; }
        [DataMember]
        public bool IsRestoreGroupTeamSite { get; set; }
        [DataMember]
        public bool IsExportGroupTeamSite { get; set; }
        [DataMember]
        public bool IsRestoreSiteCollection { get; set; }
        [DataMember]
        public bool IsExportSiteCollection { get; set; }
        [DataMember]
        public bool IsRestoreStubLink { get; set; }
        [DataMember]
        public bool IsExportStubLink { get; set; }
        [DataMember]
        public bool IsSearchGroupTeamSite { get; set; }
        [DataMember]
        public bool IsSearchSiteCollection { get; set; }
        [DataMember]
        public StubOopRestoreSetting StubOopRestoreSetting { get;set;}

    }
    [DataContract]
    public class StubOopRestoreSetting
    {
        [DataMember]
        public bool IsEnableStubOopRestore { get; set; }
        [DataMember]
        public bool IsEnableSearchStubLocation { get; set; }
        [DataMember]
        public bool IsEnableManualInputDesStubLocation { get; set; }
    }

    [DataContract]
    public class CreateJobResult: BaseContract
    {
        [DataMember]
        public string JobId { get; set; }
    }
    [DataContract]
    public class JobStatusResult : BaseContract
    {
        [DataMember]
        public int Progress { get; set; }
        [DataMember]
        public JobStatus Status { get; set; }
        [DataMember]
        public string Comment { get; set; }
    }
    [DataContract]
    public class BooleanResult : BaseContract
    {
        [DataMember]
        public Boolean Value { get; set; }
    }
    [DataContract]
    public enum SP365PermissionLevel
    {
        [EnumMember]
        SiteOwner,
        [EnumMember]
        SiteOwnerAndSiteMemberGroup,
        [EnumMember]
        SiteOwnerAndSpecialGroup,
        [EnumMember]
        SiteOwnerAndSiteMemberGroupAndSiteVisitor
    }
    [DataContract]
    public enum TeamsPermissionSetting
    {
        [EnumMember]
        Owner,
        [EnumMember]
        OwnerOrMembler
    }

    [DataContract]
    public class SiteMetricsReportParameters
    {
        //[DataMember]
        //public string JobId { get; set; }
        [DataMember]
        public string WebUrl { get; set; }
        [DataMember]
        public string LibraryRelativePath { get; set; }
        [DataMember]
        public string UserId { get; set; }
    }
}

