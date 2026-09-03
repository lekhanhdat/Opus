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
using System.Text;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.PlatformRecovery;
using AvePoint.GCommon.Contract.PlatformRecovery.Object;

namespace AvePoint.GCommon.Contract.Tree.Object
{


    /// <summary>
    /// tree节点类.
    /// **请在PRTreeNodeDto添加属性后,在PRTreeNodeModel对象里加入相同属性,并进行赋值.例如在PRTreeNodeDto加入新的属性**
    /// **在已有属性的对象中加入属性不需要改操作,例如在PRTreeDataNodeDto对象中加入VerifyStatus属性**
    /// </summary>
    [DataContract]
    [XmlRootAttribute("PRTreeNodeDto")]
    public class PRTreeNodeDto : AveTreeNodeDto<PRTreeNodeDto>
    {
        [DataMember]
        public Guid SPObjectId { get; set; }
        [DataMember]
        public string DataFormatVersion { get; set; }
        [DataMember]
        public PRNodeTypeId TypeId { get; set; }
        [DataMember]
        public string ClassName { get; set; }
        [DataMember]
        public string ErrorMessage { get; set; }
        [DataMember]
        public string Location { get; set; }//the location about where can find the data, for example, some identifier for NetApp ,(index location)
        [DataMember]
        public bool CanSelectBackup { get; set; }
        [DataMember]
        public bool CanSelectSingleNode { get; set; }
        [DataMember]
        public bool OutOfPlaceRestoreSupported { get; set; }
        [DataMember]
        public bool CanSelectRestore { get; set; }
        [DataMember]
        public PRSelectMode BackupSelected { get; set; }
        [DataMember]
        public PRSelectMode RestoreSelected { get; set; }
        [DataMember]
        public PRBackupMethod BackupMethod { get; set; }
        /// <summary>
        /// Agent short name in ServiceDto
        /// </summary>
        [DataMember]
        public string Agent { get; set; }
        [DataMember]
        public string Server { get; set; }
        [DataMember]
        public string RealInstanceName { get; set; }
        [DataMember]
        public bool CanExpand { get; set; }//for front end web servers
        [DataMember]
        public List<PRTreeDataNodeDto> DataNodes { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public bool IsIndex { get; set; }
        [DataMember]
        public Guid IndexGuId { get; set; }
        [DataMember]
        public string RestorePlanNode { get; set; }
        [DataMember]
        public bool NoAgentInstall { get; set; }
        [DataMember]
        public string PSConfigUserName { get; set; }
        [DataMember]
        public string PSConfigPassword { get; set; }
        [DataMember]
        public long BeforeOperationSize { get; set; }
        [DataMember]
        public DateTime BeforeOperationTime { get; set; }
        [DataMember]
        public string FBAParameters { get; set; }//for[DOC-10882],cannot load FBA node.
        [DataMember]
        public PRBackupType BackupType { get; set; }//for[DOC-21440].
        [DataMember]
        public List<Guid> TreeNodeMappings { get; set; }// support Nintex workflow
        [DataMember]
        public bool IsConfigFastServers { get; set; }
        [DataMember]
        public string FastServerUsername { get; set; }
        [DataMember]
        public string FastServerPassword { get; set; }
        [DataMember]
        public PRFastCertRestoreMode CertificateSelectMode { get; set; }
        [DataMember]
        public string CertificatePassword { get; set; }
        [DataMember]
        public string FastParameters { get; set; }
        [DataMember]
        public bool FirstTimeRestoreSucceeded { get; set; }//this is used for farm service node only and just for client side.
        [DataMember]
        public string DBSchemaVersion { get; set; }
        [DataMember]
        public string Url { get; set; }
        [DataMember]
        public Guid NodeAssociatedId { get; set; }
        [DataMember]
        public List<Guid> GroupMemberIds { get; set; }
        [DataMember]
        public PROutOfPlaceRestoreInfo OutOfPlaceRestoreInfo { get; set; }
        [DataMember]
        public PRNodeExtraInfo ExtraInfo { get; set; }
        [DataMember]
        public ManuallyRestoreType ManuallyRestoreDBType { get; set; }//for NetApp
        [DataMember]
        public string OriginalLocation { get; set; }
        /// <summary>
        /// 1mb =1 weight
        /// </summary>
        [DataMember]
        public long WeightForBackup { get; set; }
        [DataMember]
        public long WeightForRestore { get; set; }
        [DataMember]
        public PRHWState SupportByHWState { get; set; }
        [DataMember]
        public List<Guid> HardWareProviderList { get; set; }
        /// <summary>
        /// 加载fase search节点时存放agent 
        /// </summary>
        [DataMember]
        public string CurrentAgentId { get; set; }

        [DataMember]
        public bool IsVerifyStorageLayout { get; set; }

        #region NetApp
        [DataMember]
        public bool IsOnLun{get;set;}
        [DataMember]
        public bool IsSupportMirror { get; set; }
        [DataMember]
        public bool IsSupportVault { get; set; }
        [DataMember]
        public LunCheckType LunType = LunCheckType.Unknown;
        #endregion

        #region blob
        [DataMember]
        public List<ConnectorItem> ConnectorInfoItemList { get; set; }
        [DataMember]
        public List<ExtenderItem> ExtenderInfoItemList { get; set; }
        [DataMember]
        public Guid ContentDBId { get; set; }
        #endregion

        #region Availability Group
        [DataMember]
        public string PrimaryReplica { get; set; }
        [DataMember]
        public string AvailabilityGroupListeners { get; set; }
        [DataMember]
        public string LastUsedReplica { get; set; }
        [DataMember]
        public string PreferredReplica { get; set; }
        #endregion

        #region SimpleDB
        [DataMember]
        public bool IsSimpleDB { get; set; }
        #endregion

        // for customer tree
        //[DataMember]
        //public bool IsLoadDatabasesSuccess { get; set; }
        [DataMember]
        public bool IsLoadDatabasesFailed { get; set; }
        
        public PRTreeNodeDto()
        {
            this.DataNodes = new List<PRTreeDataNodeDto>();
            this.SPObjectId = Guid.Empty;
            this.FarmID = Guid.Empty.ToString();
            this.ID = Guid.Empty.ToString();
            this.CanChildrenBeLoaded = true;
            this.Type = NodeType.Unused;
            this.CanSelectBackup = true;
            this.CanSelectRestore = false;
            this.BackupSelected = PRSelectMode.NotSelected;
            this.RestoreSelected = PRSelectMode.NotSelected;
            this.TypeId = PRNodeTypeId.Unknown;
            this.ExtraInfo = new PRNodeExtraInfo();
            WeightForBackup = 0;
            SupportByHWState = PRHWState.UnKnown;
            HardWareProviderList = new List<Guid>();
        }

        public override string ToString()
        {
            return TextNode("", true);
        }

        /// <summary>
        /// 转换Tree型结构Text
        /// </summary>
        /// <param name="prefix">用于此节点之前的\t和连线</param>
        /// <param name="isLastChild">此节点是否是该层最后一个节点</param>
        /// <returns></returns>
        public string TextNode(string prefix, bool isLastChild)
        {
            StringBuilder textBuilder = new StringBuilder();
            textBuilder.Append(prefix + (isLastChild ? "└" : "├") +
                Name + " " + SPObjectId + " " + CheckNumber + " " +
                TypeId + " " + " " + CanSelectBackup + " " +
                CanSelectRestore + " " + " " + CanSelectSingleNode + " " + " " + IndexGuId + " " + LunType + "\r\n");
            if (Children != null)
            {
                for (int i = 0; i < Children.Count - 1; i++)
                {
                    if (isLastChild)
                    {
                        textBuilder.Append(Children[i].TextNode(prefix + "" + "\t", false));
                    }
                    else
                    {
                        textBuilder.Append(Children[i].TextNode(prefix + "│" + "\t", false));
                    }
                }
                if (Children.Count > 0)
                {
                    if (isLastChild)
                    {
                        textBuilder.Append(Children[Children.Count - 1].TextNode(prefix + "" + "\t", true));
                    }
                    else
                    {
                        textBuilder.Append(Children[Children.Count - 1].TextNode(prefix + "│" + "\t", true));
                    }
                }
            }
            return textBuilder.ToString();
        }
    }

    [DataContract]
    public enum LunCheckType
    {
        [EnumMember]
        Unknown = -1,
        [EnumMember]
        GetALLSucessfully = 0,
        [EnumMember]
        GetALLFailed = 1,
    }

    [DataContract]
    public enum ManuallyRestoreType
    {
        [EnumMember]
        Unknown = 0,
        [EnumMember]
        Successfully = 1,//db detach ssp delete
        [EnumMember]
        Failed = 2,
        [EnumMember]
        Online = 4,
        [EnumMember]
        Offline = 5,
    }

    [DataContract]
    public enum PRNodeTypeId
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Unknown = -1,
        [EnumMember]
        Farm = 10,
        [EnumMember]
        FarmPersistObject = 11,
        [EnumMember]
        WebService = 21,
        [EnumMember]
        AdminWebService = 22,
        [EnumMember]
        OSearchService = 23,
        [EnumMember]
        SPSearchService = 24,
        [EnumMember]
        SPSearchServiceInstance = 25,
        [EnumMember]
        Solutions = 30,
        [EnumMember]
        Solution = 31,
        [EnumMember]
        FormService = 32,
        [EnumMember]
        InfoPathFormsServices = 33,
        [EnumMember]
        InfoPathForm = 34,
        [EnumMember]
        InfoPath = 35,
        [EnumMember]
        GlobalTemplates = 36,
        [EnumMember]
        Template = 37,
        [EnumMember]
        ExemptUserAgents = 38,
        [EnumMember]
        ExemptUserAgent = 39,
        [EnumMember]
        SSP = 40,
        [EnumMember]
        SSPSharedObject = 41,
        [EnumMember]
        InfoPathFormTemplates = 42,
        [EnumMember]
        InfoPathFormTemplate = 43,
        [EnumMember]
        FrontEndWebServers = 45,
        [EnumMember]
        FrontEndWebServer = 46,
        [EnumMember]
        DataConnectionFiles = 47,
        [EnumMember]
        DataConnectionFile = 48,
        [EnumMember]
        WebApp = 50,
        [EnumMember]
        AdminWebApp = 51,
        [EnumMember]
        FewList = 52,
        [EnumMember]
        FewBackupList = 56, //DOC-20577,20578, for a list when restore.
        [EnumMember]
        IISSettings = 60,
        [EnumMember]
        IISWebSite = 61,
        [EnumMember]
        IISFolder = 62,
        [EnumMember]
        IISTemplates = 63,
        [EnumMember]
        IISWebconfig = 64,
        [EnumMember]
        FileSystemFolder = 65,
        [EnumMember]
        FileSystemFile = 66,
        [EnumMember]
        FileSystem = 67,
        [EnumMember]
        CustomFeatures = 68,
        [EnumMember]
        Feature = 69,
        [EnumMember]
        SystemFeatures = 70,
        //Index = 70,
        [EnumMember]
        OSearchIndex = 71,
        //SPSearchIndex = 72,
        [EnumMember]
        SolutionFeatures = 72,
        [EnumMember]
        SiteDefinitions = 73,
        [EnumMember]
        SiteDefinition = 74,
        [EnumMember]
        TempLateFolder = 75,
        [EnumMember]
        GACAll = 76,
        [EnumMember]
        NetVersion = 77,
        [EnumMember]
        AssemblyFolder = 78,
        [EnumMember]
        DB = 80,
        [EnumMember]
        ConfigDB = 81,
        [EnumMember]
        ContentDB = 82,
        [EnumMember]
        AdminContentDB = 83,
        [EnumMember]
        SSPAdminDB = 86,//ssp service db
        [EnumMember]
        SSPSearchDB = 87,//osearch db,ssp search db
        [EnumMember]
        SPSearchDB = 88,// spsearch db
        [EnumMember]
        SSODB = 90,
        [EnumMember]
        FBA = 91,
        [EnumMember]
        FBADB = 92,
        [EnumMember]
        FBAWebapp = 93,
        [EnumMember]
        SLK = 94,
        [EnumMember]
        SLKDB = 95,
        [EnumMember]
        SLKSite = 96,
        [EnumMember]
        ProjectPSISharedApplication = 100,
        [EnumMember]
        ProjectSite = 101,
        [EnumMember]
        ProjectDatabase = 102,
        [EnumMember]
        SolutionDepend = 103,
        [EnumMember]
        SolutionLanguagePack = 104,
        [EnumMember]
        Nintex = 110,
        [EnumMember]
        NintexConfigDB = 111,
        [EnumMember]
        NintexContentDB = 112,
        //[EnumMember]
        //SSPSettingProperty = 115,
        [EnumMember]
        ACS_SETTINGS = 116, // ACS Settings node
        [EnumMember]
        ACS_DB = 117, // antivirus and content shield db
        [EnumMember]
        ATM_DB = 118, //Antivirus for trend micro db
        [EnumMember]
        JobDefinitionGroup = 120,
        [EnumMember]
        ServiceProxy = 122,
        [EnumMember]
        BDCServiceProxy = 123,
        [EnumMember]
        BDCServiceApplicationProxy = 124,
        [EnumMember]
        WordConversionServiceProxy = 125,
        [EnumMember]
        WordConversionServiceApplicationProxy = 126,
        [EnumMember]
        StateServiceProxy = 127,
        [EnumMember]
        StateServiceApplicationProxy = 128,
        [EnumMember]
        ManagedMetadataWebServiceProxy = 129,
        [EnumMember]
        ManagedMetadataWebServiceApplicationProxy = 130,
        [EnumMember]
        SecureStoreServiceProxy = 131,
        [EnumMember]
        SecureStoreServiceApplicationProxy = 132,
        [EnumMember]
        SearchServiceProxy = 133,
        [EnumMember]
        WebAnalyticsWebServiceProxy = 135,
        [EnumMember]
        WebAnalyticsWebServiceApplicationProxy = 136,
        [EnumMember]
        UserProfileServiceProxy = 137,
        [EnumMember]
        UserProfileServiceApplicationProxy = 138,
        [EnumMember]
        WebServiceEndPoint = 139,
        [EnumMember]
        WebServiceEndPointGroup = 140,
        [EnumMember]
        BDCService = 141,
        [EnumMember]
        BDCServiceApplication = 142,
        [EnumMember]
        WordConversionService = 143,
        [EnumMember]
        WordConversionServiceApplication = 144,
        [EnumMember]
        StateService = 145,
        [EnumMember]
        StateServiceApplication = 146,
        [EnumMember]
        ManagedMetadataWebService = 147,
        [EnumMember]
        ManagedMetadataWebServiceApplication = 148,
        [EnumMember]
        SecureStoreService = 149,
        [EnumMember]
        SecureStoreServiceApplication = 150,
        [EnumMember]
        SearchService = 151,
        [EnumMember]
        WebAnalyticsWebService = 153,
        [EnumMember]
        WebAnalyticsWebServiceApplication = 154,
        [EnumMember]
        UserProfileService = 155,
        [EnumMember]
        UserProfileServiceApplication = 156,
        [EnumMember]
        SecureStoreServiceDB = 157,
        [EnumMember]
        ManagedMetadataWebServiceDB = 158,
        [EnumMember]
        WebAnalyticsWebServiceReportDB = 159,
        [EnumMember]
        WebAnalyticsWebServiceStagingDB = 160,
        [EnumMember]
        WordConversionServiceDB = 161,
        [EnumMember]
        BDCServiceDB = 162,
        [EnumMember]
        ExcelCalculationService = 163,
        [EnumMember]
        ExcelCalculationServiceApplication = 164,
        [EnumMember]
        VisioGraphicsService = 165,
        [EnumMember]
        VisioGraphicsServiceApplication = 166,
        [EnumMember]
        AssessService = 167,
        [EnumMember]
        AssessServiceApplication = 168,
        [EnumMember]
        UserProfileServerProfileDB = 169,
        [EnumMember]
        UserProfileServerSyncDB = 170,
        [EnumMember]
        UserProfileServerSocialDB = 171,
        [EnumMember]
        VisioGraphicsServiceApplicationProxy = 172,
        [EnumMember]
        SharedServices = 180,
        [EnumMember]
        SharedServicesApplications = 181,
        [EnumMember]
        SearchServiceApplication = 182,
        [EnumMember]
        SearchAdminDatabase = 183,
        [EnumMember]
        SearchPropertyStoreDatabase = 184,
        [EnumMember]
        SearchGathererDatabase = 185,
        [EnumMember]
        BRAdminComponent = 186,
        [EnumMember]
        BRIndexPartition = 187,
        [EnumMember]
        BRQueryComponent = 188,
        [EnumMember]
        BRCrawlComponent = 189,
        [EnumMember]
        SharedServicesProxies = 190,
        [EnumMember]
        SearchServiceApplicationProxy = 191,
        [EnumMember]
        DiagnosticsService = 200,
        [EnumMember]
        SPDiagnosticsService = 201,
        [EnumMember]
        BIMonitoringService = 220,
        [EnumMember]
        BIMonitoringServiceProxy = 221,
        [EnumMember]
        BIMonitoringServiceApplication = 222,
        [EnumMember]
        BIMonitoringServiceApplicationProxy = 223,
        [EnumMember]
        BIMonitoringServiceDatabase = 224,
        [EnumMember]
        StateServiceApplicationDatabase = 225,
        [EnumMember]
        UserCodeService = 240,
        [EnumMember]
        SolutionValidatorGroup = 241,
        [EnumMember]
        DefaultSolutionValidator = 242,
        [EnumMember]
        PopularityLoadBalancerProvider = 244,
        [EnumMember]
        ResourceMeasureGroup = 245,
        [EnumMember]
        ResourceMeasure = 246,
        [EnumMember]
        ExecutionTierGroup = 247,
        [EnumMember]
        ExecutionTier = 248,
        [EnumMember]
        ApplicationRegistryService = 250,
        [EnumMember]
        ApplicationRegistryServiceApplication = 251,
        [EnumMember]
        ApplicationRegistryServiceDatabase = 252,
        [EnumMember]
        ApplicationRegistryServiceApplicationProxy = 253,
        [EnumMember]
        SecurityTokenServiceApplication = 260,
        [EnumMember]
        ClaimEncodingManager = 261,
        [EnumMember]
        SecurityTokenServiceManager = 262,
        [EnumMember]
        ClaimProviderManager = 263,
        [EnumMember]
        SecurityTokenService = 264,
        [EnumMember]
        AccessServiceApplicationProxy = 265,
        [EnumMember]
        ExcelServiceApplicationProxy = 266,
        [EnumMember]
        LotusNotesConnectorProxy = 267,
        [EnumMember]
        UsageandHealthDataCollectionProxy = 268,
        [EnumMember]
        KlImaging = 270,
        [EnumMember]
        KlIndexComponent = 271,
        [EnumMember]
        KlViewComponent = 272,
        [EnumMember]
        KlExportComponent = 273,
        [EnumMember]
        KlSearchComponent = 274,
        [EnumMember]
        KlPrintComponent = 275,
        [EnumMember]
        KlIndexDB = 276,
        [EnumMember]
        KlViewDB = 277,
        [EnumMember]
        FastSearchFarms = 280,
        [EnumMember]
        FastSearchAdminServer = 282,
        [EnumMember]
        FastSearchServer = 283,
        [EnumMember]
        FastSearchAdminDB = 284,
        [EnumMember]
        Webparts = 285,
        [EnumMember]
        webpart = 286,
        [EnumMember]
        SubscriptionSettingsServiceApplication = 290,
        [EnumMember]
        SubscriptionSettingsDatabase = 291,
        [EnumMember]
        SubscriptionSettingsServiceApplicationProxy = 292,
        [EnumMember]
        SubscriptionSettingsService = 293,
        [EnumMember]
        SubscriptionSettingsServiceProxy = 294,
        [EnumMember]
        CustomDatabasesRoot = 300,
        [EnumMember]
        CustomDatabaseServer = 301,
        [EnumMember]
        CustomDatabaseInstance = 302,
        [EnumMember]
        CustomDatabase = 303,
        [EnumMember]
        SharedSearchSettings = 115,
        [EnumMember]
        NgSocialSiteServiceApp = 310,
        [EnumMember]
        NgSocialSiteServiceDB = 311,
        [EnumMember]
        NgSocialSiteReportingDB = 312,
        [EnumMember]
        NgSocialSiteServiceProxy = 313,
        [EnumMember]
        NgDiagnosticsService = 314,
        [EnumMember]
        KlImagingData = 320,
        [EnumMember]
        KlImagingServiceApplication = 321,
        [EnumMember]
        KlImagingServiceApplicationProxy = 322,
        [EnumMember]
        KlImagingDataDatabase = 323,
        [EnumMember]
        KlImagingServiceDatabase = 324,
        #region Usage and Health Data Collection Service Application
        [EnumMember]
        SPUsageService = 330,
        [EnumMember]
        SPUsageApplication = 331,
        [EnumMember]
        SPUsageDatabase = 332,
        [EnumMember]
        SPUsageApplicationProxy = 333,
        [EnumMember]
        SPUsageServiceProxy = 334,
        #endregion
        #region Word Viewing Service Application
        [EnumMember]
        ConversionServiceApplication = 340,
        [EnumMember]
        ConversionApplicationProxy = 341,
        [EnumMember]
        ConversionService = 342,
        #endregion
        #region Powerpoint Service Application
        [EnumMember]
        PowerPointWebService = 345,
        [EnumMember]
        PowerPointWebServiceApplication = 346,
        [EnumMember]
        PowerPointWebServiceApplicationProxy = 347,
        #endregion

        #region Session State Service
        [EnumMember]
        SessionStateService = 350,
        [EnumMember]
        SessionStateServiceApplication = 351,
        [EnumMember]
        SessionStateDatabase = 352,
        #endregion
        [EnumMember]
        DataNode = 500,
        [EnumMember]
        InvisibleDataNode = 501,
        [EnumMember]
        Unknow = -1,
        [EnumMember]
        ContentSoureces = 499,
        [EnumMember]
        OneConentSource = 502,
        [EnumMember]
        CrawlRules = 503,
        [EnumMember]
        OneCrawlRule = 504,
        [EnumMember]
        FileTypes = 505,
        [EnumMember]
        Extension = 506,
        [EnumMember]
        CrawlerImpactRules = 507,
        [EnumMember]
        OneCrawlerImpactRule = 508,
        [EnumMember]
        AuthoritativePages = 510,
        [EnumMember]
        FederatedLocations = 511,
        [EnumMember]
        OneFederatedLocation = 512,
        [EnumMember]
        Scopes = 513,
        [EnumMember]
        OneScope = 514,
        [EnumMember]
        MetadataProperties = 515,
        [EnumMember]
        ManagedProperties = 517,
        [EnumMember]
        OneManagedProperty = 518,
        [EnumMember]
        CrawledProperties = 519,
        [EnumMember]
        Category = 521,
        [EnumMember]
        CrawledProperty = 522,
        [EnumMember]
        OwningSite = 523,
        [EnumMember]
        SearchBasedAlerts = 524,
        [EnumMember]
        AuthoritativeNode = 525,
        [EnumMember]
        AuthoritativeLevels = 526,
        [EnumMember]
        AuthoritativePageUrl = 527,
        [EnumMember]
        StartAddress = 528,
        [EnumMember]
        NonAuthorNode = 529,
        [EnumMember]
        ssaSettings = 600,
        [EnumMember]
        ProjectServiceDiagnosticsService = 601,
        [EnumMember]
        ProjectServiceApplication = 602,
        [EnumMember]
        ProjectServiceApplicationProxy = 603,
        [EnumMember]
        ProjectPublishedDB = 604,
        [EnumMember]
        ProjectReportingDB = 605,
        [EnumMember]
        ProjectVersionsDB = 606,
        [EnumMember]
        ProjectWorkingDB = 607,
        [EnumMember]
        NotesWebServiceApplication = 608,
        [EnumMember]
        NotesWebApplicationProxy = 609,

        // BLOB
        [EnumMember]
        ConnectorBlob = 701,
        [EnumMember]
        ExtenderBlob = 702,
    }

    [DataContract]
    public enum PRFastCertRestoreMode
    {
        [EnumMember]
        Default = -1,
        [EnumMember]
        None = 0,
        [EnumMember]
        GenerateNew = 1,
        [EnumMember]
        RestoreBackup = 2
    }

    [DataContract]
    public enum PRBackupMethod
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Unknown = -1,
        [EnumMember]
        NetApp = 1,
        [EnumMember]
        VSS = 2,
        [EnumMember]
        VDI = 3,
    }

    [DataContract]
    public enum PRSelectMode
    {
        [EnumMember]
        Unknown = -1,
        [EnumMember]
        NotSelected = 0,
        [EnumMember]
        Selected = 1,
    }
    [DataContract]
    public enum PRVssDataLocation
    {
        /// <summary>
        /// 数据在本地
        /// </summary>
        [EnumMember]
        OnLocalDevice = 0,

        /// <summary>
        /// 数据已向Media发送, 且本地的已删除
        /// </summary>
        [EnumMember]
        OnMedia = 1,

        /// <summary>
        /// 数据已向Media发送, 且本地也有数据
        /// </summary>
        [EnumMember]
        OnBoth = 2,

        /// <summary>
        /// 正在向Media发送数据
        /// </summary>
        [EnumMember]
        Transporting = 3,

        /// <summary>
        /// 本地和Media都无数据
        /// </summary>
        [EnumMember]
        None = 4,
    }
    [DataContract]
    public enum PRHWState
    {
        [EnumMember]
        UnKnown = -1,
        [EnumMember]
        NotSupportByHW = 0,
        [EnumMember]
        SupportByHW = 1,
    }

    [DataContract]
    public enum PRIndexStatus
    {
        /// <summary>
        /// 未生成Index
        /// </summary>
        [EnumMember]
        NotStart = 0,

        /// <summary>
        /// 正在生成Index
        /// </summary>
        [EnumMember]
        Indexing = 1,

        /// <summary>
        /// 生成Index成功
        /// </summary>
        [EnumMember]
        IndexSucceed = 2,

        /// <summary>
        /// 生成Index失败
        /// </summary>
        [EnumMember]
        IndexFailed = 3,

        /// <summary>
        /// 不支持生成Index
        /// </summary>
        [EnumMember]
        Nonsupport = 4,

        /// <summary>
        /// Defer生成Index
        /// </summary>
        [EnumMember]
        Partial = 5,

        /// <summary>
        /// IndexLevel == None
        /// </summary>
        [EnumMember]
        NoneLevel = 6,
    }

    [DataContract]
    public enum PRNAVerifyStatus
    {
        /// <summary>
        /// 未Verify
        /// </summary>
        [EnumMember]
        NotStart = 0,

        /// <summary>
        /// Verify succeed
        /// </summary>
        [EnumMember]
        VerifySucceed = 1,

        /// <summary>
        /// Verify failed
        /// </summary>
        [EnumMember]
        VerifyFailed = 2,

        /// <summary>
        /// not support
        /// </summary>
        [EnumMember]
        NonSupport = 3,

        /// <summary>
        /// not exist
        /// </summary>
        [EnumMember]
        NotExist = 4,
    }

    [DataContract]
    public enum PRNAArchiveStatus
    {
        /// <summary>
        /// Not Archive
        /// </summary>
        [EnumMember]
        NotStart = 0,

        /// <summary>
        /// Archive succeed
        /// </summary>
        [EnumMember]
        ArchiveSucceed = 1,

        /// <summary>
        /// Archive failed
        /// </summary>
        [EnumMember]
        ArchiveFailed = 2,

        /// <summary>
        /// not support
        /// </summary>
        [EnumMember]
        NonSupport = 3,

        /// <summary>
        /// not exist
        /// </summary>
        [EnumMember]
        NotExist = 4,
    }

    [DataContract]
    public enum PRMappingStatus
    {
        /// <summary>
        /// 未生成Mapping
        /// </summary>
        [EnumMember]
        NotStart = 0,

        /// <summary>
        /// 正在生成Mapping
        /// </summary>
        [EnumMember]
        OnMapping = 1,

        /// <summary>
        /// 生成Mapping成功
        /// </summary>
        [EnumMember]
        MappingSucceed = 2,

        /// <summary>
        /// 生成Mapping失败
        /// </summary>
        [EnumMember]
        MappingFailed = 3,

        /// <summary>
        /// 不支持生成Mapping
        /// </summary>
        [EnumMember]
        Nonsupport = 4,
    }

    [DataContract(IsReference = true)]
    [KnownType(typeof(PRTreeNodeDto))]
    [KnownType(typeof(PRTreeBlobDataNodeDto))]
    public class PRTreeDataNodeDto : AveTreeNodeDto<PRTreeDataNodeDto>
    {
        [DataMember]
        public string PlanId { get; set; }
        [DataMember]
        public string JobId { get; set; }
        [DataMember]
        public string ClassName { get; set; }
        [DataMember]
        public string DBCertificateName { get; set; }
        [DataMember]
        public string BackupStartedTime { get; set; }
        [DataMember]
        public string BackupCompletedTime { get; set; }
        [DataMember]
        public string Error { get; set; }//any error message during backup        
        [DataMember]
        public PRNodeTypeId TypeId { get; set; }
        [DataMember]
        public PRIndexStatus IndexStatus { get; set; }
        [DataMember]
        public PRMappingStatus MappingStatus { get; set; }
        [DataMember]
        public PRVssDataLocation DataRealLocation { get; set; }
        [DataMember]
        public PRTreeNodeDto Parent { get; set; }
        [DataMember]
        public string Location { get; set; }
        [DataMember]
        public long BackupSize { get; set; }
        [DataMember]
        public string Agent { get; set; }
        [DataMember]
        public string Server { get; set; }
        [DataMember]
        public PRBackupMethod BackupMethod { get; set; }
        [DataMember]
        public long Weight { get; set; }
        [DataMember]
        public DataSecurity DataSecurity { get; set; }
        [DataMember]
        public CompressionType CompressionType { get; set; }
        [DataMember]
        public PRNodeExtraInfo ExtraInfo { get; set; }
        [DataMember]
        public string InstanceVersion { get; set; }
        [DataMember]
        public string DBVersion { get; set; }
        [DataMember]
        public string ActiveClusterNode { get; set; }
        #region add for NetApp
        [DataMember]
        public PRNAVerifyStatus VerifyStatus { get; set; }
        [DataMember]
        public PRNAArchiveStatus ArchiveStatus { get; set; }
        #endregion
        public PRTreeDataNodeDto()
        {
            this.TypeId = PRNodeTypeId.DataNode;
            this.DataRealLocation = PRVssDataLocation.OnMedia;
            this.ExtraInfo = new PRNodeExtraInfo();
        }
    }

    [DataContract(IsReference = true)]
    public class PRTreeBlobDataNodeDto : PRTreeDataNodeDto
    {
        [DataMember]
        public List<OutSideBlobInfoDto> BackupedBlobRecords { get; set; }
    }

    [DataContract(IsReference = true)]
    [KnownType(typeof(PRTreeNodeDto))]
    public class OutSideBlobInfoDto
    {
        [DataMember]
        public ConnectorInfo internalConnector;

        [DataMember]
        public ExtenderItem internalExtender;

        [DataMember]
        public string shareName;

        [DataMember]
        public string snapshotName;

        [DataMember]
        public string snapShotRootPath;

        [DataMember]
        public Guid FolderId;

        [DataMember]
        public Guid ParentId;

        [DataMember]
        public string VolumeName;
    }
    /// <summary>
    /// When you add new typeId do not forget add it in below groups.
    /// </summary> 
    public class AveVssTypeIdsCollections
    {
        public static List<PRNodeTypeId> VSS_SP_DB_TYPEIDS = new List<PRNodeTypeId>();
        public static List<PRNodeTypeId> VSS_OTHER_DB_TYPEIDS = new List<PRNodeTypeId>();
        public static List<PRNodeTypeId> VSS_INDEX_TYPEIDS = new List<PRNodeTypeId>();

        public static List<PRNodeTypeId> VSS_GRANDGRANDPARENTSELECT_TYPEIDS = new List<PRNodeTypeId>();
        public static List<PRNodeTypeId> VSS_GRANDPARENTSELECT_TYPEIDS = new List<PRNodeTypeId>();
        public static List<PRNodeTypeId> VSS_PARENTSELECT_TYPEIDS = new List<PRNodeTypeId>();
        public static List<PRNodeTypeId> VSS_SELFSELECT_TYPEIDS = new List<PRNodeTypeId>();

        static AveVssTypeIdsCollections()
        {
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.AdminContentDB);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.ConfigDB);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.ContentDB);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.ProjectDatabase);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.SPSearchDB);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.SSPAdminDB);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.SSPSearchDB);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.SSODB);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.WordConversionServiceDB);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.WebAnalyticsWebServiceStagingDB);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.WebAnalyticsWebServiceReportDB);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.UserProfileServerProfileDB);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.UserProfileServerSocialDB);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.UserProfileServerSyncDB);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.NgSocialSiteServiceDB);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.NgSocialSiteReportingDB);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.SecureStoreServiceDB);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.BIMonitoringServiceDatabase);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.ManagedMetadataWebServiceDB);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.StateServiceApplicationDatabase);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.SessionStateDatabase);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.BDCServiceDB);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.ApplicationRegistryServiceDatabase);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.SearchAdminDatabase);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.SearchGathererDatabase);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.SearchPropertyStoreDatabase);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.SubscriptionSettingsDatabase);
            VSS_SP_DB_TYPEIDS.Add(PRNodeTypeId.SPUsageDatabase);

            VSS_OTHER_DB_TYPEIDS.Add(PRNodeTypeId.ACS_DB);
            VSS_OTHER_DB_TYPEIDS.Add(PRNodeTypeId.ATM_DB);
            VSS_OTHER_DB_TYPEIDS.Add(PRNodeTypeId.FBADB);
            VSS_OTHER_DB_TYPEIDS.Add(PRNodeTypeId.NintexConfigDB);
            VSS_OTHER_DB_TYPEIDS.Add(PRNodeTypeId.NintexContentDB);
            VSS_OTHER_DB_TYPEIDS.Add(PRNodeTypeId.SLKDB);
            VSS_OTHER_DB_TYPEIDS.Add(PRNodeTypeId.SearchAdminDatabase);
            VSS_OTHER_DB_TYPEIDS.Add(PRNodeTypeId.SearchGathererDatabase);
            VSS_OTHER_DB_TYPEIDS.Add(PRNodeTypeId.SearchPropertyStoreDatabase);
            VSS_OTHER_DB_TYPEIDS.Add(PRNodeTypeId.CustomDatabase);
            VSS_OTHER_DB_TYPEIDS.Add(PRNodeTypeId.FastSearchAdminDB);
            VSS_OTHER_DB_TYPEIDS.Add(PRNodeTypeId.KlImagingServiceDatabase);
            VSS_OTHER_DB_TYPEIDS.Add(PRNodeTypeId.KlImagingDataDatabase);

            VSS_INDEX_TYPEIDS.Add(PRNodeTypeId.OSearchIndex);
            VSS_INDEX_TYPEIDS.Add(PRNodeTypeId.SPSearchServiceInstance);
            VSS_INDEX_TYPEIDS.Add(PRNodeTypeId.BRAdminComponent);
            VSS_INDEX_TYPEIDS.Add(PRNodeTypeId.BRCrawlComponent);
            VSS_INDEX_TYPEIDS.Add(PRNodeTypeId.BRQueryComponent);
            //VSS_INDEX_TYPEIDS.Add(PRNodeTypeId.BRIndexPartition);

            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.BRQueryComponent);

            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.BRCrawlComponent);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.BRAdminComponent);

            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.SPSearchDB);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.SSPAdminDB);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.SSPSearchDB);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.SearchGathererDatabase);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.SearchPropertyStoreDatabase);
            //VSS_PARENTSELECT_TYPEIDS.Add(PRNodeTypeId.BRAdminComponent);

            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.AdminContentDB);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.ConfigDB);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.ContentDB);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.ProjectDatabase);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.SSODB);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.ACS_DB);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.ATM_DB);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.FBADB);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.NintexConfigDB);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.NintexContentDB);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.SLKDB);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.OSearchIndex);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.SPSearchServiceInstance);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.WordConversionServiceDB);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.WebAnalyticsWebServiceStagingDB);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.WebAnalyticsWebServiceReportDB);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.UserProfileServerProfileDB);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.UserProfileServerSocialDB);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.UserProfileServerSyncDB);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.NgSocialSiteServiceDB);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.NgSocialSiteReportingDB);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.SPSearchServiceInstance);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.SecureStoreServiceDB);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.BIMonitoringServiceDatabase);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.ManagedMetadataWebServiceDB);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.StateServiceApplicationDatabase);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.BDCServiceDB);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.SessionStateDatabase);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.ApplicationRegistryServiceDatabase);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.SearchAdminDatabase);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.CustomDatabase);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.FastSearchAdminDB);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.SubscriptionSettingsDatabase);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.SPUsageDatabase);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.KlImagingServiceDatabase);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.KlImagingDataDatabase);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.ConnectorBlob);
            VSS_SELFSELECT_TYPEIDS.Add(PRNodeTypeId.ExtenderBlob);
        }

        public static bool IsSelectedDatabaseNode(PRTreeNodeDto backupNode)
        {
            if (VSS_SELFSELECT_TYPEIDS.Contains(backupNode.TypeId) && backupNode.BackupSelected == PRSelectMode.Selected
                || VSS_PARENTSELECT_TYPEIDS.Contains(backupNode.TypeId) && backupNode.Parent.BackupSelected == PRSelectMode.Selected)
            {
                if (VSS_SP_DB_TYPEIDS.Contains(backupNode.TypeId) || VSS_OTHER_DB_TYPEIDS.Contains(backupNode.TypeId))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsSelectedIndexNode(PRTreeNodeDto backupNode)
        {
            if (VSS_SELFSELECT_TYPEIDS.Contains(backupNode.TypeId) && backupNode.BackupSelected == PRSelectMode.Selected
                || VSS_PARENTSELECT_TYPEIDS.Contains(backupNode.TypeId) && backupNode.Parent.BackupSelected == PRSelectMode.Selected
                || VSS_GRANDPARENTSELECT_TYPEIDS.Contains(backupNode.TypeId) && backupNode.Parent.Parent.BackupSelected == PRSelectMode.Selected
                || VSS_GRANDGRANDPARENTSELECT_TYPEIDS.Contains(backupNode.TypeId) && backupNode.Parent.Parent.Parent.BackupSelected == PRSelectMode.Selected)
            {
                if (VSS_INDEX_TYPEIDS.Contains(backupNode.TypeId))
                    return true;
            }
            return false;
        }

        public static bool IsBackupSelectedVSSNode(PRTreeNodeDto backupNode)
        {
            if ((VSS_SELFSELECT_TYPEIDS.Contains(backupNode.TypeId) && backupNode.BackupSelected == PRSelectMode.Selected)
                || (VSS_PARENTSELECT_TYPEIDS.Contains(backupNode.TypeId) && backupNode.Parent.BackupSelected == PRSelectMode.Selected)
                || (VSS_GRANDPARENTSELECT_TYPEIDS.Contains(backupNode.TypeId) && backupNode.Parent.Parent.BackupSelected == PRSelectMode.Selected)
                || (VSS_GRANDGRANDPARENTSELECT_TYPEIDS.Contains(backupNode.TypeId) && backupNode.Parent.Parent.Parent.BackupSelected == PRSelectMode.Selected))
            {
                return true;
            }
            return false;
        }

        public static bool IsRestoreSelectedVSSNode(PRTreeNodeDto backupNode)
        {
            if (VSS_SELFSELECT_TYPEIDS.Contains(backupNode.TypeId) && backupNode.RestoreSelected == PRSelectMode.Selected
                || VSS_PARENTSELECT_TYPEIDS.Contains(backupNode.TypeId) && backupNode.Parent.RestoreSelected == PRSelectMode.Selected
                || VSS_GRANDPARENTSELECT_TYPEIDS.Contains(backupNode.TypeId) && backupNode.Parent.Parent.RestoreSelected == PRSelectMode.Selected
                || VSS_GRANDGRANDPARENTSELECT_TYPEIDS.Contains(backupNode.TypeId) && backupNode.Parent.Parent.Parent.RestoreSelected == PRSelectMode.Selected)
            {
                return true;
            }
            return false;
        }

        public static bool IsVssSupportNode(PRTreeNodeDto backupNode)
        {
            if (VSS_SP_DB_TYPEIDS.Contains(backupNode.TypeId)
                || VSS_OTHER_DB_TYPEIDS.Contains(backupNode.TypeId)
                || VSS_INDEX_TYPEIDS.Contains(backupNode.TypeId))
            {
                return true;
            }
            return false;
        }

        public static bool IsVssSupportDBNode(PRTreeNodeDto backupNode)
        {
            if (VSS_SP_DB_TYPEIDS.Contains(backupNode.TypeId) || VSS_OTHER_DB_TYPEIDS.Contains(backupNode.TypeId))
            {
                return true;
            }
            return false;
        }

        public static bool IsVssSupportIndexNode(PRTreeNodeDto backupNode)
        {
            if (VSS_INDEX_TYPEIDS.Contains(backupNode.TypeId))
            {
                return true;
            }
            return false;
        }


    }
}