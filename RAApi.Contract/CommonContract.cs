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
using System.ComponentModel;
using System.Runtime.Serialization;

namespace DocAveOnline.WebApi.Contracts
{
    public static class CommonConstString
    {
        public const string ENDUSEREXPORTCONTAINERNAME= "cloudarchivercontent";
    }
    public class JobIdStateInfo
    {
        public int MergeIndexState { get; set; }
        public long MediaDataSize { get; set; }
        public string JobId { get; set; }
    }
    public class CommonInfo
    {
        public int Key { get; set; }
    }
    public class DaoLoginInfo
    {
        public string ProductName { get; set; }
        public string AppUrl { get; set; }
        public string Signature { get; set; }
        public string UserName { get; set; }
        public string TenantGroupId { get; set; }
        public override string ToString()
        {
            return string.Format("{0}$${1}", this.ProductName, this.AppUrl);
        }
    }
    public class DaoRefreshInfo : DaoLoginInfo
    {
        public string ExpireTime { get; set; }
        public override string ToString()
        {
            return string.Format("{0}$${1}##{2}", ProductName, AppUrl, ExpireTime);
        }
    }
    public class User
    {
        public string UserName { get; set; }
        public string TenantGroupId { get; set; }
    }
    public class LoginResult
    {
        public String Signature { get; set; }
        public String DocAveToken { get; set; }
    }

    public class DaoAccessToken
    {
        public string Token { get; set; }
        public string TokenType { get; set; }
    }

    public class StoragePolicy
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public StorageDeviceType StorageType { get; set; }
    }

    public class StoragePolicyInfo : BaseContract
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public StorageDeviceType StorageType { get; set; }
    }

    public class StoragePolicyInfos : BaseContract
    {
        public List<StoragePolicyInfo> Values { get; set; }
    }

    public class ExportLocation
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ExportReportType ReportType { get; set; }
        public StorageDeviceType StorageType { get; set; }
        public string ConnectionString { get; set; }
    }

    public class ExportLocationInfo : BaseContract
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ExportReportType ReportType { get; set; }
        public StorageDeviceType StorageType { get; set; }
        public string ConnectionString { get; set; }
    }

    public class ExportLocationInfos : BaseContract
    {
        public List<ExportLocationInfo> Values { get; set; }
    }

    public class SecurityProfile
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
    }

    public class SecurityProfiles : BaseContract
    {
        public List<SecurityProfile> Values { get; set; }
    }

    public class SiteCollection
    {
        public String Id { get; set; }
        public String Url { get; set; }
        public String ParentId { get; set; }
        public String Username { get; set; }
        public String Domain { get; set; }
        public String Password { get; set; }
        public SiteCollectionState State { get; set; }
        public String BPOSMould { get; set; }
        public List<String> AvailableAgentIds { get; set; }
        public long CreateTime { get; set; }
        public String TemplateName { get; set; }
        public String SPVersion { get; set; }
        public String TemplateTitle { get; set; }
        public bool IsPublicWebSite { get; set; }
        public String Name { get; set; }
        public RemoveNodeType NodeType { get; set; }
        public String TenantGroupId { get; set; }
        public SiteCollectionType SiteCollectionType { get; set; }
        public string AdminUrl { get; set; }
        public string ServiceAccountId { get; set; }
        public string TenantId { get; set; }
        public AppType AppType { get; set; }
        public BposConnectionType AuthType { get; set; }
        public AADEnvironment AADEnvironment { get; set; }
    }
    public class WebApplication
    {
        public String Id { get; set; }
        public String DomainName { get; set; }
        public Boolean UseSSL { get; set; }
        public String Url { get; set; }
        public String Description { get; set; }
        public long ModifiedDate { get; set; }
        public RemoveNodeType NodeType { get; set; }
    }
    public class FSTreeNodeDto
    {
        public string Id { get; set; }
        public NodeLevel Level { get; set; }
        public FSTreeNodeDto Parent { get; set; }
        //public string SPObjectId { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string FullPath { get; set; }
        //public string FarmId { get; set; }
        public NodeExtension NodeExtension { get; set; }
        public Int32 SPVersion { get; set; }
        public IList<FSTreeNodeDto> Children { get; set; }
        public string LoginName { get; set; }
        //public string Description { get; set; }
        //public SPType SPType { get; set; }
        public NodeType NodeType { get; set; }
        public bool CanChildrenBeLoaded { get; set; }
        public int OffSet { get; set; }
       // public String FarmName { get; set; }
        public string Title { get; set; }
        public bool Expanded { get; set; }
       // public Boolean Hidden { get; set; }
        public int ChildrenCount { get; set; }
        public int CheckNumber { get; set; }
        //public Int32 Template { get; set; }
        //public string Url { get; set; }
        public bool ChildrenLoaded { get; set; }
        public string TeamName { get; set; }
    }
    public class SPTreeNodeDto
    {
        public string Id { get; set; }
        public NodeLevel Level { get; set; }
        public SPTreeNodeDto Parent { get; set; }
        public string SPObjectId { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string FullPath { get; set; }
        public string FarmId { get; set; }
        public NodeExtension NodeExtension { get; set; }
        public Int32 SPVersion { get; set; }
        public IList<SPTreeNodeDto> Children { get; set; }
        public string LoginName { get; set; }
        public string Description { get; set; }
        public SPType SPType { get; set; }
        public NodeType NodeType { get; set; }
        public bool CanChildrenBeLoaded { get; set; }
        public int OffSet { get; set; }
        public String FarmName { get; set; }
        public string Title { get; set; }
        public bool Expanded { get; set; }
        public Boolean Hidden { get; set; }
        public int ChildrenCount { get; set; }
        public int CheckNumber { get; set; }
        public Int32 Template { get; set; }
        public string Url { get; set; }
        public bool ChildrenLoaded { get; set; }
        public string TeamName { get; set; }
    }
    public class Farm : SPTreeNodeDto
    {
    }
    public class FarmDto
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string FarmId { get; set; }
        public string Name { get; set; }
        public int SPVersion { get; set; }
    }
    public class Tree
    {
        public TreeType TreeType { get; set; }
        public string PageInfo { get; set; }
        public SPTreeNodeDto Node { get; set; }
        public IList<SPTreeNodeDto> NodeList { get; set; }
        public int ChildrenCount { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasError { get; set; }
        public string Message { get; set; }
    }
    public class BackupRestoreWorkflow
    {
        public Boolean IncludeWorkflowDefinition { get; set; }
        public Boolean IncludeWorkflowInstance { get; set; }
        public WorkflowConflictResolutionType DefinitionConflictResolution { get; set; }
        public WorkflowConflictResolutionType InstanceConflictResolution { get; set; }
    }
    public class BposInfo
    {
        public String SiteUrl { get; set; }
        public BposUserAccountInfo UserAccountInfo { get; set; }
        public BPOSMode Mode { get; set; }
        public BposConnectionType ConnectionType { get; set; }
        public AppType AppType { get; set; }
        public MailboxType MailboxType { get; set; }
        public TokenType TokenType { get; set; }
        public string TenantGroupId { get; set; }
    }
    public class BposUserAccountInfo
    {
        public string Domain { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string TenantId { get; set; }
        public string AdminUrl { get; set; }
        public string SecondaryUsername { get; set; }
        public string SecondaryPassword { get; set; }
        public string AppClientId { get; set; }
        public string AppCertSecret { get; set; }
        public string AppCertContent { get; set; }
        public string AppCertSecretContent { get; set; }
        public AADEnvironment AADEnvironment { get; set; }
        public string AppId { get; set; }
    }
    public class NodeExtension
    {
        public BposInfo BposInfo { get; set; }
        public TreeType TreeType { get; set; }
    }
    public class GAOTreeNode
    {
        public string FullPath { get; set; }
        public string SiteCollectionUrl { get; set; }
        public string SPObjectId { get; set; }
        public GATreeNodeType Type { get; set; }
        public bool Checked { get; set; }
        public string ContainerId { get; set; }
        public string ContainerName { get; set; }
    }
    public class OneDriveForBusinessTestResult
    {
        public SiteCollectionState State { get; set; }
        public string Url { get; set; }
        public string UserName { get; set; }
        public string SPVersion { get; set; }
        public string TemplateName { get; set; }
        public string TemplateTitle { get; set; }
    }
    public class BaseFilterItem : FilterPolicy
    {
        public DisplayDateTime BeginTime { get; set; }
        public DisplayDateTime EndTime { get; set; }
        public string FilterItemId { get; set; }
        public FilterRule FilterRule { get; set; }
        public FilterPolicyType FilterType { get; set; }
        public bool IsAnd { get; set; }
        public string Operator { get; set; }
    }
    public class SharePointOnlineObject
    {
        public string FolderPath { get; set; }
        public Guid ItemGuid { get; set; }
        public SPObjectNodeLevel Level { get; set; }
        public string ListTitle { get; set; }
        public string SiteGroupName { get; set; }
        public string SiteUrl { get; set; }
        public string WebServerRelativeUrl { get; set; }
    }
    public class Office365AccountInfo
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string AdminUrl { get; set; }
        public string Url { get; set; }
    }
    public class CreateSitecollectionInfo
    {
        public SiteCollection SiteCollection { get; set; }
        public string ExistUrl { get; set; }
    }

    public class ImportOnedriveInfo
    {
        public Office365AccountInfo Office365AccountInfo { get; set; }
        public string ImportUser { get; set; }
    }

    public class DeploymentInfo
    {
        public List<DocAveOnlineRole> Roles { get; set; }
    }

    public class DocAveOnlineRole
    {
        public DAODeploymentRoleType Type { get; set; }

        public string Version { get; set; } // if version not found, return 0.0.0.0

        internal bool VersionLessOrEqualThan(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                return false;
            }
            if (Version == version)
            {
                return true; // equal
            }
            var localV = Version.Split('.');
            var targetV = version.Split('.');
            for (var i = 0; i < 4; i++)
            {
                if (int.Parse(localV[i]) > int.Parse(targetV[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }

    public enum DAODeploymentRoleType
    {
        Web = 1,
        Timer = 2,
        Agent = 3,
        TimerTask = 4,
        API = 5
    }

    public enum ExportReportType
    {
        Storage = 0,
        SharePoint = 1
    }
    public enum StorageDeviceType
    {
        None = -1,
        NetShare = 0,
        FTP = 1,
        TSM = 2,
        EMCCentera = 3,
        Cloud = 4,
        CloudAmazon = 401,
        CloudRackspace = 402,
        CloudAzure = 403,
        CloudAtmos = 404,
        CloudAT_TSynaptic = 405,
        HCP = 406,
        Dropbox = 407,
        DELL = 5,
        NetApp_Alta_Vault = 510,
        S3Compatible = 601,
        Wasabi = 602,
        NetApp = 7,
        NetApp_LUN = 701,
        NetApp_CIFS = 702,
        Caringo = 8,
        Box = 9,
        SkyDrive = 11,
        SFTP = 12,
    }
    public enum SiteCollectionState
    {
        AccessAll,
        AccessSome,
        AccessNone,
        AccountExpired,
        Notinitialize,
        AdminCenterUrlInvalid,
    }
    [DataContract]
    public enum RemoveNodeType
    {
        [EnumMember]
        SiteCollection,
        [EnumMember]
        SkyDrivePro,
        [EnumMember]
        O365GroupSites,
        [EnumMember]
        O365TeamSites
    }
    public enum SiteCollectionType
    {
        Normal = 0,
        AdminCenter = 1,
    }
    public enum AppType
    {
        Office365 = 0,
        SharePoint = 1,
        Exchange = 2,
        CustomAzureApp = 3
    }
    public enum BposConnectionType
    {
        ServiceAccount = 0,
        AppToken = 1
    }
    public enum NodeLevel
    {
        Undefined = 0,
        Root = -2,
        Farm = -1,
        WebApplication = 2,
        SiteCollections = 6,
        SiteCollection = 100,
        Site = 200,
        Lists = 201,
        Sites = 202,
        Apps = 280,
        App = 281,
        AppData = 282,
        List = 300,
        Folder = 400,
        DesignFolder = 404,//Design Folder，一般是Root folder下面的hidden folder
        Item = 500,
        Document = 531,
        ItemVersion = 550,
        DesignItem = 502,//Design Folder下面的item，一般是system file.
        Library = 301,
        RootFolder = 402, //list rootfolder & web rootfolder
        DesignObjRootFolder = 403, //和rootfolder同级别的，但是rootfolder
        Folders = 401,
        Items = 501,
        DesignItems = 503,
        DesignFolders = 504,
        FSFolder = 2100,
        FSFile = 2200,
        SkyDrivePro = 6000,
        SkyDriveProGroup = 6010,
        O365GroupSites = 6020,
        O365GroupSitesGroup = 6030,
        ProjectOnline = 6040,
        ProjectOnlines = 6050,
        Office365GroupEntire = 6080,
    }
    public enum ProfileType : int
    {
        UnSpecified = 0,
        ArchiverRule = 66,
        ArchiverRuleForRevIM = 76,
        ExchangeArchiverRuleForRevIM = 78,
    }
    public enum CompressionType
    {
        None = 0,
        Fastest = 1,
        Level2 = 2,
        Fast = 3,
        Level4 = 4,
        Normal = 5,
        Level6 = 6,
        Good = 7,
        Level8 = 8,
        Best = 9
    }
    [Flags]
    public enum DataSecurity
    {
        None = 0,
        CompressionMedia = 4,
        CompressionAgent = 16,
        EncryptionMedia = 8,
        EncryptionAgent = 32
    }
    public enum EncryptionMethods
    {
        BLOWFISH_ENCRYPTION = 0,
        AES_ENCRYPTION = 1
    }
    public enum PolicyLevel
    {
        None = 0,
        WebApplication = 1,
        SiteCollection = 2,
        Site = 4,
        List = 8,
        Folder = 16,
        Item = 32,
        Document = 64,
        Attachment = 128,
        DocumentVersion = 256,
        ItemVersion = 512,
        User = 1024,
        ADProfile = 2048,
        Url = 4096,
        Library = 8192,
        //For Records physical
        PhysicalBox = 10001,
        //For Records physical
        PhysicalFile = 10002,
        ExchangeOnlineMailbox = 16384,
        ExchangeOnlineFolder = 32768,
        ExchangeOnlineItem = 65536,
        ExchangeOnlineItem_Message = 6553601,
        ExchangeOnlineItem_Task = 6553602,
        ExchangeOnlineItem_Post = 6553603,
        ExchangeOnlineItem_Event = 6553604,
        ExchangeOnlineItem_Journal = 6553605,
        ExchangeOnlineItem_Note = 6553606,
        ExchangeOnlineItem_Contact = 6553607,
        ExchangeOnlineItem_Document = 6553608,
        Newsfeed = 131072,
        AdvancedSearch = 524288,
        FileSysFile = 1048576,
        FileSysFolder = 2097152,
        AzureFileDocument = 4194304,
    }
    public enum PlanCategory
    {
        None = 0,
        CentralAdmin = 2,
        ContentManager = 3,
        GranularRestore = 4,
        Replicator = 5,
        PlatformRecoveryBackup = 6,
        PlatformRecoveryRestore = 7,
        ConvertStubToContent = 8,
        ExtenderScheduled = 9,
        StorageOptimizationConfig = 10,
        StubRetention = 11,
        DeploymentManager = 14,
        ReportCenter = 15,
        Archiver = 16,
        GranularBackup = 18,
        Connector = 19,
        ArchiverRestore = 20,
        LogManager = 21,
        ArchiverRetention = 22,
        JobPruning = 23,
        PlatformRecoveryMaintenance = 24,
        SPMigration07To10 = 30,
        PlanGroup = 40,
        DeploymentManagerBackup = 71,
        CAPolicyEnforcer = 91,
        ExchangeOnlineBackup = 100,
        ExchangeOnlineRestore = 101,
        ExportReport = 103,
        CloudAppAdmin = 106,
        CloudAppAdminPE = 107,
        ExchangeOnlineLocate = 108,
    }
    public enum WorkflowConflictResolutionType
    {
        None,
        NotOverwrite,
        Overwrite,
        Append,
        OverwriteOrSkipDefinition,
        OverwriteDefinitionByForce
    }
    public enum BPOSMode
    {
        Undetermined,
        SecurityTrimming,
        Office365
    }
    public enum MailboxType
    {
        None = 0,
        PublicFolder = 1,
        User = 2,
        Group = 3,
    }
    public enum TokenType
    {
        Basic = 0,
        ADAL = 1,
        MSAL = 2
    }
    public enum BlobProviderType
    {
        None = 0,
        EBS = 1,
        RBS = 2,
        ALL = 3
    }
    public enum TreeType
    {
        Undefined = 0,
        ContentManagerSrcTree = 3,
        ContentManagerDestTree = 4,
        StorageOptimizationTree = 10,
        SOArchiverTree = 108,
        ExchangeOnlineArchiverTree = 115,
    }
    public enum SPType
    {
        Moss,
        BPOS
    }
    [DataContract]
    public enum NodeType
    {
        [EnumMember]
        UnspecifiedBaseType = -1,
        [EnumMember]
        GenericList = 0,
        [EnumMember]
        DocumentLibrary = 1,
        [EnumMember]
        Unused = 2,
        [EnumMember]
        ManualInput = 100,
        [EnumMember]
        CAWebapp = 201,
        [EnumMember]
        Document = 6,
        [EnumMember]
        ListItem = 7,
        [EnumMember]
        Announcements = 104,
        [EnumMember]
        Contacts = 105,
        [EnumMember]
        Calendar = 106,
        [EnumMember]
        CustomList = 10100,//由于和ManualInput冲突，加了10000
        [EnumMember]
        CustomListInDB = 120,
        [EnumMember]
        IssueTracking = 1100,
        [EnumMember]
        Links = 130,
        [EnumMember]
        projectTask = 150,
        [EnumMember]
        StatusList = 432,
        [EnumMember]
        Tasks = 107,
        [EnumMember]
        ExternalList = 600,
        [EnumMember]
        ImportSpreadsheet = 10001,// 为对应数据库，自定义
        [EnumMember]
        ListTemplate = 300,
        [EnumMember]
        MasterPageGallery = 301,
        [EnumMember]
        Images = 302,
        [EnumMember]
        WebPartGallery = 303,
        [EnumMember]
        StyleLibrary = 304,
        [EnumMember]
        ThemeGallery = 306,
        [EnumMember]
        UserInformationList = 307,
        [EnumMember]
        SitePages = 311,
        [EnumMember]
        SiteAssets = 312,
        [EnumMember]
        FormTemplates = 315,
        [EnumMember]
        Solutions = 318,
        [EnumMember]
        SystemTermGroup = 500,
        [EnumMember]
        UserTermGroup = 501,
        [EnumMember]
        SkyDriveProSitesGroup = 1000,
        [EnumMember]
        SharePointSitesGroup = 1001,
        [EnumMember]
        O365GroupSitesGroup = 1002,
        [EnumMember]
        SkyDriveProSites = 1010,
        [EnumMember]
        SharePointSites = 1011,
        [EnumMember]
        O365GroupSites = 1012,
        [EnumMember]
        AdminCenter = 830,
    }
    public enum GATreeNodeType
    {
        SelectAll = -3,
        Root = 0,
        None = -2,
        Farm = -1,
        Group = -4,
        WebApplication = 2,
        WebApplication_D = -5,
        ManagedPath = 10,
        SiteCollection = 100,
        Site = 200,
        Sites = 201,
        Lists = 202,
        List = 300,
        DocumentLibrary = 310,
        Folder = 400,
        Folders = 401,
        Items = 402,
        Item = 500,
        Stores = 600,
        Store = 610,
        Locations = 700,
        Location = 710,
        Solution = 800,
        All = 1000
    }
    public enum FilterExpressionType
    {
        BasicFilter = 0,
        AdvancedFilter = 1
    }
    public enum FilterPolicyType
    {
        None = 0,
        ServiceFilter = 1,
        DomainFilter = 2,
        DateTimeFilter = 3,
        TextFilter = 4,
        NumberFilter = 5,
        UserFilter = 6,
        BoolFilter = 7
    }
    public enum FilterRule
    {
        None = 0,
        URL = 1,
        SiteCollectionTitle = 2,
        ModifiedTime = 3,
        CreatedTime = 4,
        Owner = 5,
        TemplateName = 6,
        CreateBy = 7,
        ModifiedBy = 8,
        ContentType = 9,
        ColumnText = 10,
        Versions = 11,
        DocumentNameAndExtension = 12,
        DocumentSize = 13,
        AttachmentNameAndExtension = 14,
        Size = 15,
        SiteTitle = 16,
        ListName = 17,
        FolderName = 18,
        ItemName = 19,
        ColumnNumber = 20,
        ColumnBool = 21,
        ColumnDate = 22
    }
    public enum SPObjectNodeLevel
    {
        SiteCollection = 100,
        Site = 200,
        List = 300,
        Folder = 400,
        RootFolder = 402,
        Item = 500,
    }
    public enum StoragePolicyDataType
    {
        STORAGE_NEW_DATA_TYPE = 0,
        STORAGE_OLD_DATA_TYPE = 1,
    }

    public enum AADEnvironment
    {
        AzureCloud = 0,
        AzureChinaCloud = 1,
        USGovernment = 2,
        AzureGermanyCloud = 3,
        AzurePPE = 99,
        None = 255
    }

    public class StringValue : BaseContract
    {
        public String Value { get; set; }
    }

    public class BooleanValue : BaseContract
    {
        public Boolean Value { get; set; }
    }

    public class InitializeInfo : BaseContract
    {
        public InitializeStatus InitializeStatus { get; set; }
    }

    public enum InitializeStatus
    {
        Sucessful,
        Exception,
        Failed,
        Exist,
        Initializing,
        SoftDelete
    }

    public class DefaultPhysicalDeviceContent
    {
        public string Name { get; set; }
        public StorageDeviceType Type { get; set; }
        public string Description { get; set; }
        public int TenantPercent { get; set; }
        public int ExtendPercent { get; set; }
        public List<string> Notifications { get; set; }
        public List<string> ExtendNotifications { get; set; }
        public List<AzureStorage> AzureStorages { get; set; }
        public List<AzureStorage> AzureExtendStorages { get; set; }
        public List<AmazonStorage> AmazonStorages { get; set; }
        public List<AmazonStorage> AmazonExtendStorages { get; set; }
    }

    public enum AmazonRegion
    {
        [Description("Asia Pacific (Mumbai)")]
        Mumbai,

        [Description("Asia Pacific (Singapore)")]
        Apac,

        [Description("Asia Pacific (Sydney)")]
        Sydney,

        [Description("Asia Pacific (Tokyo)")]
        Tokyo,

        [Description("Asia Pacific (Seoul)")]
        Seoul,

        [Description("Canada (Central)")]
        CanadaCentral,

        [Description("EU (Frankfurt)")]
        Frankfurt,

        [Description("EU (Ireland)")]
        EU,

        [Description("EU (London)")]
        London,

        [Description("South America (Sao Paulo)")]
        SaoPaulo,

        [Description("US East (Ohio)")]
        Ohio,

        [Description("US East (N. Virginia)")]
        USStandard,

        [Description("US West (Northern California)")]
        USWest,

        [Description("US West (Oregon)")]
        Oregon
    }

    public class DefaultPhysicalStorageBaseDto
    {
        public int TenantGroupCount { get; set; }
        //public long AssignedSpace { get; set; }
        public bool Advanced { get; set; }
        public List<string> ExtendedParameters { get; set; }
    }

    public class AzureStorage : DefaultPhysicalStorageBaseDto
    {
        public string AccessPoint { get; set; }
        public string ContainerName { get; set; }
        public string AccountName { get; set; }
        public string AccountKey { get; set; }

    }

    public class AmazonStorage : DefaultPhysicalStorageBaseDto
    {
        public string BucketName { get; set; }
        public string AccessId { get; set; }
        public string AccessKey { get; set; }
        public AmazonRegion Region { get; set; }
    }

    public class TokenResult : BaseContract
    {
        public string Token { get; set; }
        public AADEnvironment AADEnvironment { get; set; }
    }
}

