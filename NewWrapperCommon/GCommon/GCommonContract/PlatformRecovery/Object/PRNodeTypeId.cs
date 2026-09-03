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


namespace AvePoint.GCommon.Contract.Tree.Object
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/AvePoint.GCommon.Contract.Tree.Object")]
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
        BRTopologyComponent = 192,
        [EnumMember]
        BRIndexComponent = 193,
        [EnumMember]
        SearchLinksDatabase = 194,
        [EnumMember]
        SearchAnalyticsReportingDatabase = 195,
        [EnumMember]
        SearchSettingsDatabase = 196,
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
        #region Powerpoint Service Application For 2010
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
        [EnumMember]
        PowerPivotService = 610,
        [EnumMember]
        PowerPivotServiceApplication = 611,
        [EnumMember]
        PowerPivotServiceApplicationDatabase = 612,
        [EnumMember]
        PowerPivotServiceApplicationProxy = 613,

        [EnumMember]
        SPLicenseEntityMappingManager = 630,
        [EnumMember]
        WorkflowServiceProxy = 640,
        [EnumMember]
        WorkflowServiceApplicationProxy = 641,
        [EnumMember]
        WorkflowServiceApplication = 642,
        [EnumMember]
        AppManagementServiceApplication = 650,
        [EnumMember]
        AppManagementServiceDatabase = 651,
        [EnumMember]
        AppManagementServiceApplicationProxy = 652,
        [EnumMember]
        TranslationServiceApplication = 660,
        [EnumMember]
        TranslationServiceApplicationProxy = 661,
        [EnumMember]
        TranslationServiceDB = 662,
        [EnumMember]
        WorkManagementServiceApplication = 665,
        [EnumMember]
        WorkManagementServiceApplicationProxy = 666,
        [EnumMember]
        PowerPointConversionService = 670,
        [EnumMember]
        PowerPointConversionServiceApplication = 671,
        [EnumMember]
        PowerPointConversionServiceApplicationProxy = 672,
        [EnumMember]
        AccessServicesWebServiceApplication = 680,
        [EnumMember]
        AccessServicesWebServiceApplicationProxy = 681,

        // BLOB
        [EnumMember]
        ConnectorBlob = 701,
        [EnumMember]
        ExtenderBlob = 702,
        [EnumMember]
        StubDatabases = 710,
        [EnumMember]
        StubDatabase = 711,

        [EnumMember]
        IISApplication = 721,

        // snapshot
        [EnumMember]
        SnapShot = 722,
        [EnumMember]
        LocalBackup = 723,
        [EnumMember]
        RemoteBackup = 724,

        //News Gator Video Stream Service Application
        [EnumMember]
        NgVideoStreamService = 725,
        [EnumMember]
        NgVideoStreamServiceProxy = 726,
        [EnumMember]
        NgVideoStreamServiceApplication = 727,
        [EnumMember]
        NgVideoStreamServiceApplicationProxy = 728,
        [EnumMember]
        NgVideoStreamServiceApplicationVideoReportDB = 729,

        //SQLReportService
        [EnumMember]
        SQLReportServiceApplication = 731,
        [EnumMember]
        SQLReportServiceApplicationProxy = 732,
        [EnumMember]
        SQLReportServiceAlterDatabase = 733,
        [EnumMember]
        SQLReportServiceDatabase = 734,
        [EnumMember]
        SQLReportServiceTempDatabase = 735,

        //News Gator News Stream Service Application
        [EnumMember]
        NgNewsStreamServiceApplication = 738,
        [EnumMember]
        NgNewsStreamServiceApplicationProxy = 739,
        [EnumMember]
        NgNewsStreamServiceApplicationDB = 740,

        [EnumMember]
        NgLearningPointServiceApplication = 745,
        [EnumMember]
        NgLearningPointServiceApplicationProxy = 746,

        [EnumMember]
        NgInternalCommunicationServiceApplication = 747,
        [EnumMember]
        NgInternalCommunicationServiceApplicationProxy = 748,
        [EnumMember]
        NgInnovationServiceApplication = 749,
        [EnumMember]
        NgInnovationServiceDB = 750,
        [EnumMember]
        NgInnovatiaonServiceApplicationProxy = 751,

        [EnumMember]
        SQL08ReportingService = 741,

        //WFE File System virtual node TypeId
        [EnumMember]
        FileSystemFolders = 743,
        [EnumMember]
        FileSystemFiles = 744,

    }
}
