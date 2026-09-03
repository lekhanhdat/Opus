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
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.RMWeb.Tree.Base
{
    /// <summary>
    /// Match DAEnums.jsx
    /// </summary>
    [DataContract]
    public enum RMNodeLevel
    {
        [EnumMember]
        RMSelectAll = -4,
        [EnumMember]
        RMIncludeNew = -3,
        [EnumMember]
        Root = -2,
        [EnumMember]
        Farm = -1,
        [EnumMember]
        Undefined = 0,
        [EnumMember]
        WebApplication = 2,
        [EnumMember]
        ContentDBs = 4,
        [EnumMember]
        SiteCollections = 6,
        [EnumMember]
        ContentDB = 30,
        [EnumMember]
        SiteCollection = 100,
        [EnumMember]
        Site = 200,
        [EnumMember]
        Lists = 201,
        [EnumMember]
        Sites = 202,
        [EnumMember]
        VDesignLists = 203,
        [EnumMember]
        VSiteAdmin = 204,
        [EnumMember]
        VSiteColumns = 205,
        [EnumMember]
        VContentTypes = 206,
        [EnumMember]
        VLookAndFeels = 207,
        [EnumMember]
        VUsersAndPerms = 208,
        [EnumMember]
        ContentTypeGroup = 209,
        [EnumMember]
        SiteContentType = 210,
        [EnumMember]
        SiteColumnGroup = 211,
        [EnumMember]
        SiteColumn = 212,
        [EnumMember]
        SiteCTWorkflow = 213,
        [EnumMember]
        VSiteCTWorkflow = 214,
        [EnumMember]
        SiteWorkflow = 215,
        [EnumMember]
        VSiteWorkflow = 216,
        [EnumMember]
        Deprecated_Sites = 251,
        [EnumMember]
        Deprecated_Lists = 252,
        [EnumMember]
        SiteSetting = 255,
        [EnumMember]
        Apps = 280,
        [EnumMember]
        App = 281,
        [EnumMember]
        AppData = 282,
        [EnumMember]
        List = 300,
        [EnumMember]
        Library = 301,
        [EnumMember]
        Folder = 400,
        [EnumMember]
        Folders = 401,
        [EnumMember]
        RootFolder = 402,
        [EnumMember]
        DesignObjRootFolder = 403,
        [EnumMember]
        DesignFolder = 404,
        [EnumMember]
        Deprecated_Item = 453,
        [EnumMember]
        Deprecated_Folder = 454,
        [EnumMember]
        Item = 500,
        [EnumMember]
        Items = 501,
        [EnumMember]
        DesignItem = 502,
        [EnumMember]
        DesignItems = 503,
        [EnumMember]
        DesignFolders = 504,
        [EnumMember]
        ItemVersion = 550,
        [EnumMember]
        ListSetting = 600,
        [EnumMember]
        VListAdmin = 601,
        [EnumMember]
        VListColumns = 602,
        [EnumMember]
        VListContentTypes = 603,
        [EnumMember]
        ListContentTypeGroup = 604,
        [EnumMember]
        ListContentType = 605,
        [EnumMember]
        ListColumnGroup = 606,
        [EnumMember]
        ListColumn = 607,
        [EnumMember]
        ListWorkflow = 608,
        [EnumMember]
        VListWorkflow = 609,
        [EnumMember]
        ListCTWorkflow = 610,
        [EnumMember]
        VListCTWorkflow = 611,
        [EnumMember]
        Groups = 1000,
        [EnumMember]
        SharePointGroup = 1001,
        [EnumMember]
        DomainGroup = 1002,
        [EnumMember]
        SharePointUser = 1003,
        [EnumMember]
        Users = 1100,
        [EnumMember]
        User = 1101,
        [EnumMember]
        AgentGroup = 2000,
        [EnumMember]
        Device = 2002,
        [EnumMember]
        FSFolder = 2100,
        [EnumMember]
        FSFile = 2200,
        [EnumMember]
        DMVirtualNode = 2300,
        [EnumMember]
        FEWVirtualNode = 2301,
        [EnumMember]
        SLCVirtualNode = 2302,
        [EnumMember]
        SharedServices = 2303,
        [EnumMember]
        FEWAgentNode = 2400,
        [EnumMember]
        IISettingsVirtualNode = 2401,
        [EnumMember]
        GACVirtualNode = 2402,
        [EnumMember]
        CustomFeatureVirtualNode = 2403,
        [EnumMember]
        SiteDefinitionVirtualNode = 2404,
        [EnumMember]
        FileSystemVirtualNode = 2405,
        [EnumMember]
        IISPNode = 2406,
        [EnumMember]
        IISDefaultSiteNode = 2407,
        [EnumMember]
        IISNonIISiteNode = 2408,
        [EnumMember]
        GACFirstVirtualNode = 2409,
        [EnumMember]
        GACSecondVirtualNode = 2410,
        [EnumMember]
        GACNode = 2411,
        [EnumMember]
        CustomFeatureNode = 2412,
        [EnumMember]
        SiteDefinitionNode = 2413,
        [EnumMember]
        FileSystemDiskNode = 2414,
        [EnumMember]
        FileSystemFolderNode = 2415,
        [EnumMember]
        FileSystemFileNode = 2416,
        [EnumMember]
        FileSystemFoldersNode = 2417,
        [EnumMember]
        FileSystemFilesNode = 2418,
        [EnumMember]
        IISTemplatesNode = 2419,
        [EnumMember]
        IISiteNode = 2420,
        [EnumMember]
        IISFolderNode = 2421,
        [EnumMember]
        IISWebConfigNode = 2422,
        [EnumMember]
        SolutionNode = 2423,
        [EnumMember]
        IISFileNode = 2424,
        [EnumMember]
        GACThirdVirtualNode = 2425,
        [EnumMember]
        VSiteSolutionNode = 2500,
        [EnumMember]
        StorageLevel = 2550,
        [EnumMember]
        ManagedMetadataService = 2600,
        [EnumMember]
        MMS = 2610,
        [EnumMember]
        Traditional = 2611,
        [EnumMember]
        Multitenant = 2612,
        [EnumMember]
        MutiTenantModeTermStore = 2613,
        [EnumMember]
        TermStore = 2620,
        [EnumMember]
        GlobalTermGroup = 2630,
        [EnumMember]
        LocalTermGroup = 2631,
        [EnumMember]
        TermGroup = 2640,
        [EnumMember]
        TermSet = 2650,
        [EnumMember]
        Term = 2670,
        [EnumMember]
        ContentTypeHub = 2680,
        [EnumMember]
        PublishingContentType = 2690,
        [EnumMember]
        PatternVersions = 2700,
        [EnumMember]
        Pattern = 2710,
        [EnumMember]
        PatternQueue = 2720,
        [EnumMember]
        Agent = 3000,
        [EnumMember]
        FileConnection = 3010,
        [EnumMember]
        LivelinkConnection = 3011,
        [EnumMember]
        NotesConnection = 3012,
        [EnumMember]
        ExchangeConnection = 3013,
        [EnumMember]
        ExchangeFolder = 3014,
        [EnumMember]
        ExchangeItem = 3015,
        [EnumMember]
        QuickPlaceConnection = 3016,
        [EnumMember]
        DocumentumConnection = 3017,
        [EnumMember]
        FileItems = 3020,
        [EnumMember]
        eRoomItems = 3021,
        [EnumMember]
        LivelinkItems = 3022,
        [EnumMember]
        NotesItems = 3023,
        [EnumMember]
        DocumentumItems = 3025,
        [EnumMember]
        eRoomCommunity = 3100,
        [EnumMember]
        eRoomFacility = 3101,
        [EnumMember]
        eRoomRoom = 3102,
        [EnumMember]
        eRoomList = 3103,
        [EnumMember]
        eRoomFolder = 3104,
        [EnumMember]
        eRoomItem = 3105,
        [EnumMember]
        ERMRoot = 3106,
        [EnumMember]
        ERMAgent = 3107,
        [EnumMember]
        ERMConnection = 3108,
        [EnumMember]
        ERMFacility = 3109,
        [EnumMember]
        ERMeRoom = 3110,
        [EnumMember]
        ERMList = 3111,
        [EnumMember]
        ERMFolder = 3112,
        [EnumMember]
        ERMItem = 3113,
        [EnumMember]
        ERMItems = 3114,
        [EnumMember]
        LivelinkWorkspace = 3150,
        [EnumMember]
        LivelinkProject = 3151,
        [EnumMember]
        LivelinkList = 3152,
        [EnumMember]
        LivelinkItem = 3153,
        [EnumMember]
        LotusNotesDominoServer = 3160,
        [EnumMember]
        LotusNotesDatabase = 3161,
        [EnumMember]
        LotusNotesView = 3162,
        [EnumMember]
        LotusNotesDocument = 3163,
        [EnumMember]
        QuickPlaceDominoServer = 3170,
        [EnumMember]
        QuickPlacePlace = 3171,
        [EnumMember]
        QuickPlaceRoom = 3172,
        [EnumMember]
        DocumentumCabinet = 3180,
        [EnumMember]
        DocumentumObject = 3181,
        [EnumMember]
        DocumentumFolder = 3182,
        [EnumMember]
        DocumentumVirtualDocument = 3183,
        [EnumMember]
        DocumentumSnapShot = 3184,
        [EnumMember]
        DocumentumSnapShort = 3185,
        [EnumMember]
        CustomDatabase = 4000,
        [EnumMember]
        Plan = 4010,
        [EnumMember]
        Cycle = 4012,
        [EnumMember]
        Job = 4013,
        [EnumMember]
        SRMFilePath = 4020,
        [EnumMember]
        SRMFolder = 4021,
        [EnumMember]
        SRMFile = 4022,
        [EnumMember]
        SRMBAKFilePath = 4023,
        [EnumMember]
        SRMDatabase = 4024,
        [EnumMember]
        SDMInstance = 4025,
        [EnumMember]
        HAGroup = 4030,
        [EnumMember]
        PERule = 4040,
        [EnumMember]
        PEDetail = 4041,
        [EnumMember]
        SSDMVHDFile = 4050,
        [EnumMember]
        VHDFolders = 4051,
        [EnumMember]
        VHDItems = 4052,
        [EnumMember]
        VHDItem = 4053,
        [EnumMember]
        VHDFilePath = 4054,
        [EnumMember]
        LDFFile = 4055,
        [EnumMember]
        NDFFile = 4056,
        [EnumMember]
        MDFFile = 4057,


        [EnumMember]
        ExchangeOnlineItem = 5110,

        #region Azure File Share

        [EnumMember]
        AzureFileShareGroup = 7000,
        [EnumMember]
        AzureFileShareConnection = 7001,
        [EnumMember]
        AzureFileShareDirectory = 7002,
        [EnumMember]
        AzureFileShareFile = 7003,
        #endregion

        #region Box
        [EnumMember]
        BoxConnectionGroup = 7100,
        [EnumMember]
        BoxConnection = 7101,
        [EnumMember]
        BoxUser=7102,
        [EnumMember]
        BoxFolder = 7103,
        [EnumMember]
        BoxFile = 7104,
        #endregion

        #region Google
        [EnumMember]
        GoogleContainer = 7200,
        GoogleDrive = 7201,
        GoogleFolder = 7202,
        GoogleFile = 7203,
        #endregion

        //Physical Objects -- Explorer Tree Node Level
        [EnumMember]
        PhysicalRootLocation = 9000,
        [EnumMember]
        PhysicalNormalLocation = 9100,
        [EnumMember]
        PhysicalBottomLocation = 9200,
        /// <summary>
        /// custom container
        /// </summary>
        [EnumMember]
        PhysicalCustom = 9250,
        [EnumMember]
        PhysicalBox = 9300,
        [EnumMember]
        PhysicalFile = 9400,
        [EnumMember]
        PhysicalRecord = 9500,

        [EnumMember]
        FSRoot = 11000,
        [EnumMember]
        FSGroup = 11001,
        [EnumMember]
        FSConnection = 11002,
        //FSFolder = 11002,
        //FSFile = 11003

        [EnumMember]
        RuleContainerRoot = 12000,
        [EnumMember]
        RuleContainer = 12001,

        [EnumMember]
        CustomizeConnectorItem = 13000,
    }

    /// <summary>
    /// Match DAEnums.jsx
    /// </summary>
    [DataContract]
    public enum RMNodeType
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
        DiscussionBoard = 3,
        [EnumMember]
        Survey = 4,
        [EnumMember]
        Issue = 5,
        [EnumMember]
        Document = 6,
        [EnumMember]
        ListItem = 7,
        [EnumMember]
        ManualInput = 100,
        [EnumMember]
        Announcements = 104,
        [EnumMember]
        Contacts = 105,
        [EnumMember]
        Calendar = 106,
        [EnumMember]
        Tasks = 107,
        [EnumMember]
        CustomListInDB = 120,
        [EnumMember]
        Links = 130,
        [EnumMember]
        projectTask = 150,
        [EnumMember]
        CAWebapp = 201,
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
        SiteCollectionImages = 305,
        [EnumMember]
        ThemeGallery = 306,
        [EnumMember]
        UserInformationList = 307,
        [EnumMember]
        wfpub = 308,
        [EnumMember]
        TaxomonyHiddenList = 310,
        [EnumMember]
        SitePages = 311,
        [EnumMember]
        SiteAssets = 312,
        [EnumMember]
        ReportingTemplates = 313,
        [EnumMember]
        ReportingMetadata = 314,
        [EnumMember]
        FormTemplates = 315,
        [EnumMember]
        ConvertedForms = 316,
        [EnumMember]
        ContenttypePublishingErrorLog = 317,
        [EnumMember]
        Solutions = 318,
        [EnumMember]
        GACFirstNode = 400,
        [EnumMember]
        GACSecondNode = 401,
        [EnumMember]
        GACThirdNode = 402,
        [EnumMember]
        StatusList = 432,
        [EnumMember]
        SystemTermGroup = 500,
        [EnumMember]
        UserTermGroup = 501,
        [EnumMember]
        ExternalList = 600,
        [EnumMember]
        eRoomHomeFolder = 600,
        [EnumMember]
        eRoomFolder = 601,
        [EnumMember]
        eRoomInbox = 602,
        [EnumMember]
        eRoomDiscussionPage = 603,
        [EnumMember]
        eRoomPollPage = 604,
        [EnumMember]
        eRoomCalendarPage = 605,
        [EnumMember]
        eRoomProjectSchedulePage = 606,
        [EnumMember]
        eRoomDBPage = 607,
        [EnumMember]
        eRoomDBProcess = 608,
        [EnumMember]
        eRoomDashboardPage = 609,
        [EnumMember]
        eRoomFolderPage = 610,
        [EnumMember]
        eRoomAllLinks = 611,
        [EnumMember]
        eRoomAllNotes = 612,
        [EnumMember]
        eRoomLinkedFolder = 613,
        [EnumMember]
        LivelinkAppearance = 700,
        [EnumMember]
        LivelinkCategory = 701,
        [EnumMember]
        LivelinkChannel = 702,
        [EnumMember]
        LivelinkCollection = 703,
        [EnumMember]
        LivelinkCompoundDocument = 704,
        [EnumMember]
        LivelinkCustomView = 705,
        [EnumMember]
        LivelinkDiscussion = 706,
        [EnumMember]
        LivelinkFolder = 707,
        [EnumMember]
        LivelinkLiveReport = 708,
        [EnumMember]
        LivelinkPoll = 709,
        [EnumMember]
        LivelinkProspector = 710,
        [EnumMember]
        LivelinkTaskList = 711,
        [EnumMember]
        LivelinkWorkflowMap = 712,
        [EnumMember]
        LivelinkEnterpriseWS = 713,
        [EnumMember]
        LivelinkPersonalWS = 714,
        [EnumMember]
        LivelinkOtherAccessWS = 715,
        [EnumMember]
        LivelinkProject = 716,
        [EnumMember]
        LivelinkContractFolder = 717,
        [EnumMember]
        LivelinkBusinessLeads = 718,
        [EnumMember]
        LivelinkDocument = 750,
        [EnumMember]
        LivelinkShortcut = 751,
        [EnumMember]
        LivelinkTextDocument = 752,
        [EnumMember]
        LivelinkURL = 753,
        [EnumMember]
        LivelinkWorkflowStatus = 754,
        [EnumMember]
        LivelinkXmlDtd = 755,
        [EnumMember]
        LivelinkProjectTemplate = 756,
        [EnumMember]
        LivelinkTaskGroup = 757,
        [EnumMember]
        LivelinkAppearanceWorkspaceFolder = 758,
        [EnumMember]
        LivelinkProspectorSnapshot = 759,
        [EnumMember]
        LivelinkTopic = 760,
        [EnumMember]
        LivelinkTask = 761,
        [EnumMember]
        LivelinkNews = 762,
        [EnumMember]
        LivelinkMilestone = 763,
        [EnumMember]
        LivelinkGeneration = 764,
        [EnumMember]
        LivelinkItem = 765,
        [EnumMember]
        LivelinkCADDocument = 766,
        [EnumMember]
        PFPublicFolder = 800,
        [EnumMember]
        PFUrnContentClassesFolder = 810,
        [EnumMember]
        PFUrnContentClassesMailfolder = 811,
        [EnumMember]
        PFUrnContentClassesCalendarfolder = 812,
        [EnumMember]
        PFUrnContentClassesContactfolder = 813,
        [EnumMember]
        PFUrnContentClassesTaskfolder = 814,
        [EnumMember]
        PFUrnContentClassesJournalfolder = 815,
        [EnumMember]
        PFUrnContentClassesNotefolder = 816,
        [EnumMember]
        PFIPF = 820,
        [EnumMember]
        PFIPFNote = 821,
        [EnumMember]
        PFIPFAppointment = 822,
        [EnumMember]
        PFIPFContact = 823,
        [EnumMember]
        PFIPFTask = 824,
        [EnumMember]
        PFIPFJournal = 825,
        [EnumMember]
        PFIPFStickyNote = 826,
        [EnumMember]
        PFIPFNoteInfoPathForm = 827,
        [EnumMember]
        AdminCenter = 830,
        [EnumMember]
        NewsfeedPost = 831,
        [EnumMember]
        NewsfeedReply = 832,
        [EnumMember]
        SharePointSitesGroup = 833,
        [EnumMember]
        OneDriveSitesGroup = 834,
        [EnumMember]
        SharePointSites = 835,
        [EnumMember]
        OneDriveSites = 836,
        [EnumMember]
        IssueTracking = 1100,

        //Physical Objects -- Explorer Tree Node Level
        [EnumMember]
        PhysicalRootLocation = 9000,
        [EnumMember]
        PhysicalNormalLocation = 9100,
        [EnumMember]
        PhysicalBottomLocation = 9200,
        [EnumMember]
        PhyCustom = 9250,
        [EnumMember]
        PhyBox = 9300,
        [EnumMember]
        PhyFile = 9400,
        [EnumMember]
        PhyRecord = 9500,

        [EnumMember]
        ImportSpreadsheet = 10001,
        [EnumMember]
        CustomList = 10100
    }

}