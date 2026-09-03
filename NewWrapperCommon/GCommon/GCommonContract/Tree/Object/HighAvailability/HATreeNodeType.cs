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


namespace AvePoint.Adonis.HighAvailability.Browse
{
    using System.Runtime.Serialization;

    [DataContract]
    public enum HATreeNodeType
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        Unknown = -1,

        [EnumMember]
        HAGroup = 2,

        [EnumMember]
        Farm = 4,

        [EnumMember]
        AgentFarm = 10,

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
        FormsService = 32,

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
        StubDB = 49,

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
        ProjectDB = 102,

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
        BDCServiceAppProxy = 124,

        [EnumMember]
        WordConversionServiceProxy = 125,

        [EnumMember]
        WordServiceAppProxy = 126,

        [EnumMember]
        StateServiceProxy = 127,

        [EnumMember]
        StateServiceAppProxy = 128,

        [EnumMember]
        ManagedMetadataWebServiceProxy = 129,

        [EnumMember]
        ManagedMetadataWebServiceAppProxy = 130,

        [EnumMember]
        SecureStoreServiceProxy = 131,

        [EnumMember]
        SecureStoreServiceAppProxy = 132,

        [EnumMember]
        SearchServiceProxy = 133,

        [EnumMember]
        WebAnalyticsServiceProxy = 135,

        [EnumMember]
        WebAnalyticsServiceAppProxy = 136,

        [EnumMember]
        UserProfileServiceProxy = 137,

        [EnumMember]
        UserProfileServiceAppProxy = 138,

        [EnumMember]
        WebServiceEndpoint = 139,

        [EnumMember]
        WebServiceEndPointGroup = 140,

        [EnumMember]
        BDCService = 141,

        [EnumMember]
        BDCServiceApp = 142,

        [EnumMember]
        WordService = 143,

        [EnumMember]
        WordServiceProxy = 703,

        [EnumMember]
        WordServiceApp = 144,

        [EnumMember]
        StateService = 145,

        [EnumMember]
        StateServiceApp = 146,

        [EnumMember]
        ManagedMetadataWebService = 147,

        [EnumMember]
        ManagedMetadataWebServiceApp = 148,

        [EnumMember]
        SecureStoreService = 149,

        [EnumMember]
        SecureStoreServiceApp = 150,

        [EnumMember]
        SearchService = 151,

        [EnumMember]
        WebAnalyticsWebService = 153,

        [EnumMember]
        WebAnalyticsServiceApp = 154,

        [EnumMember]
        UserProfileService = 155,

        [EnumMember]
        UserProfileServiceApp = 156,

        [EnumMember]
        SecureStoreServiceDB = 157,

        [EnumMember]
        ManagedMetadataWebServiceDB = 158,

        [EnumMember]
        WebAnalyticsWebServiceReportDB = 159,

        [EnumMember]
        WebAnalyticsWebServiceStagingDB = 160,

        [EnumMember]
        WordServiceDB = 161,

        [EnumMember]
        BDCServiceDB = 162,

        [EnumMember]
        ExcelCalculationService = 163,

        [EnumMember]
        ExcelCalculationServiceApp = 164,

        [EnumMember]
        VisioGraphicsService = 165,

        [EnumMember]
        VisioGraphicsServiceApp = 166,

        [EnumMember]
        AccessService = 167,

        [EnumMember]
        AccessServiceApp = 168,

        [EnumMember]
        UserProfileServerProfileDB = 169,

        [EnumMember]
        UserProfileServerSyncDB = 170,

        [EnumMember]
        UserProfileServerSocialDB = 171,

        [EnumMember]
        VisioGraphicsServiceAppProxy = 172,

        [EnumMember]
        SharedServices = 180,

        [EnumMember]
        SharedServicesApplications = 181,

        [EnumMember]
        SearchServiceApp = 182,

        [EnumMember]
        SearchAdminDB = 183,

        [EnumMember]
        SearchPropertyStoreDB = 184,

        [EnumMember]
        SearchGathererDB = 185,

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
        SearchServiceAppProxy = 191,

        [EnumMember]
        DiagnosticsService = 200,

        [EnumMember]
        SPDiagnosticsService = 201,

        [EnumMember]
        BIMonitoringService = 220,

        [EnumMember]
        BIMonitoringServiceProxy = 221,

        [EnumMember]
        BIMonitoringServiceApp = 222,

        [EnumMember]
        BIMonitoringServiceAppProxy = 223,

        [EnumMember]
        BIMonitoringServiceDB = 224,

        [EnumMember]
        StateServiceDB = 225,

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
        ApplicationRegistryServiceApp = 251,

        [EnumMember]
        ApplicationRegistryServiceDB = 252,

        [EnumMember]
        ApplicationRegistryServiceAppProxy = 253,

        [EnumMember]
        SecurityTokenServiceApp = 260,

        [EnumMember]
        ClaimEncodingManager = 261,

        [EnumMember]
        SecurityTokenServiceManager = 262,

        [EnumMember]
        ClaimProviderManager = 263,

        [EnumMember]
        SecurityTokenService = 264,

        [EnumMember]
        AccessServiceAppProxy = 265,

        [EnumMember]
        ExcelServiceAppProxy = 266,

        [EnumMember]
        LotusNotesConnectorProxy = 267,

        [EnumMember]
        UsageApplicationProxy = 268,

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
        SubscriptionSettingsServiceApp = 290,

        [EnumMember]
        SubscriptionSettingsDB = 291,

        [EnumMember]
        SubscriptionSettingsServiceAppProxy = 292,

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
        CustomDB = 303,

        [EnumMember]
        SharedSearchSettings = 115,

        [EnumMember]
        NgSocialSiteServiceApp = 310,

        [EnumMember]
        NgSocialSiteServiceDB = 311,

        [EnumMember]
        NgSocialSiteReportingDB = 312,

        [EnumMember]
        NgSocialServiceAppProxy = 313,

        [EnumMember]
        NgSocialSitesServiceAppProxy = 315,

        [EnumMember]
        NgDiagnosticsService = 314,

        [EnumMember]
        KlImagingData = 320,

        [EnumMember]
        KlImagingServiceApp = 321,

        [EnumMember]
        KlImagingServiceAppProxy = 322,

        [EnumMember]
        KlImagingDataDB = 323,

        [EnumMember]
        KlImagingServiceDB = 324,

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

        #endregion Usage and Health Data Collection Service Application

        #region Word Viewing Service Application

        [EnumMember]
        ConversionServiceApp = 340,

        [EnumMember]
        ConversionServiceAppProxy = 341,

        [EnumMember]
        ConversionService = 342,

        #endregion Word Viewing Service Application

        #region Powerpoint Service Application

        [EnumMember]
        PowerPointWebService = 345,

        [EnumMember]
        PowerPointWebServiceApp = 346,

        [EnumMember]
        PowerPointWebServiceAppProxy = 347,

        #endregion Powerpoint Service Application

        #region Session State Service

        [EnumMember]
        SessionStateService = 350,

        [EnumMember]
        SessionStateServiceApplication = 351,

        [EnumMember]
        SessionStateDatabase = 352,

        #endregion Session State Service

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
        ProjectServiceApp = 602,

        [EnumMember]
        ProjectServiceAppProxy = 603,

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

        [EnumMember]
        AppManagementServiceApp = 650,

        [EnumMember]
        AppManagementServiceDB = 651,

        [EnumMember]
        AppManagementServiceAppProxy = 652,

        [EnumMember]
        TranslationServiceApp = 660,

        [EnumMember]
        TranslationServiceAppProxy = 661,

        [EnumMember]
        TranslationServiceDB = 662,

        [EnumMember]
        WorkManagementServiceApp = 665,

        [EnumMember]
        WorkManagementServiceAppProxy = 666,

        [EnumMember]
        PowerPointConversionService = 670,

        [EnumMember]
        PowerPointConversionServiceApp = 671,

        [EnumMember]
        PowerPointConversionServiceAppProxy = 672,

        [EnumMember]
        AccessServicesWebServiceApp = 680,

        [EnumMember]
        AccessServicesWebServiceAppProxy = 681,

        // BLOB
        [EnumMember]
        ConnectorBlob = 701,

        [EnumMember]
        ExtenderBlob = 702,

        [EnumMember]
        StubDatabaseRoot = 703,

        [EnumMember]
        IncludeNew = 704,

        [EnumMember]
        VirtualIncludeNew = 705,

        #region For SharePoint 2013

        [EnumMember]
        SearchLinksDB = 706,

        [EnumMember]
        SearchAnalyticsReportingDB = 707,

        #endregion For SharePoint 2013

        #region Sharepoint 2016
        [EnumMember]
        SearchSettingDB = 708,
        #endregion
    }
}