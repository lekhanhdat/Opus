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





namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    #region using directives
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAListVersioningSettingOperation : CAOperation
    {
        [DataMember]
        public int ListTemplateType { get; set; }

        /// <summary>
        /// 当从SP取出来的ListTemplateType值超出了契约中定义
        /// 的枚举范围之后需要使用此值存储从SP取出来的值
        /// </summary>
        [DataMember]
        public int ListTemplateTypeIntValue { get; set; }

        [DataMember]
        public BaseType BaseType { get; set; }

        [DataMember]
        public bool IsLibrary { get; set; }

        [DataMember]
        public bool ContentApproval { get; set; }

        [DataMember]
        public bool EnableVersioning { get; set; }

        [DataMember]
        public bool EnableMinorVersions { get; set; }

        [DataMember]
        public int MajorVersionLimit { get; set; }

        [DataMember]
        public int MajorWithMinorVersionsLimit { get; set; }

        [DataMember]
        public DraftVisibilityType DraftVersionVisibility { get; set; }

        [DataMember]
        public bool ForceCheckout { get; set; }

        [DataMember]
        public string FullPath { get; set; }

    }

    // Summary:
    //     Specifies the kind of user who can view the minor version of a document draft.
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DraftVisibilityType
    {
        // Summary:
        //     Reader. Value = 0.
        [EnumMember]
        Reader = 0,
        //
        // Summary:
        //     Author. Value = 1.
        [EnumMember]
        Author = 1,
        //
        // Summary:
        //     Approver. Value = 2.
        [EnumMember]
        Approver = 2,
    }

    // Summary:
    //     Specifies the type of a list definition or a list template.
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ListTemplateType
    {
        // Summary:
        //     Not used. Value = -1.
        [EnumMember]
        InvalidType = -1,
        [EnumMember]
        NoListTemplate = 0,
        //
        // Summary:
        //     Custom list. Value = 100.
        [EnumMember]
        GenericList = 100,
        //
        // Summary:
        //     Document library. Value = 101.
        [EnumMember]
        DocumentLibrary = 101,
        //
        // Summary:
        //     Survey. Value = 102.
        [EnumMember]
        Survey = 102,
        //
        // Summary:
        //     Links. Value = 103.
        [EnumMember]
        Links = 103,
        //
        // Summary:
        //     Announcements. Value = 104.
        [EnumMember]
        Announcements = 104,
        //
        // Summary:
        //     Contacts. Value = 105.
        [EnumMember]
        Contacts = 105,
        //
        // Summary:
        //     Calendar. Value = 106.
        [EnumMember]
        Events = 106,
        //
        // Summary:
        //     Tasks. Value = 107.
        [EnumMember]
        Tasks = 107,
        //
        // Summary:
        //     Discussion board. Value = 108.
        [EnumMember]
        DiscussionBoard = 108,
        //
        // Summary:
        //     Picture library. Value = 109.
        [EnumMember]
        PictureLibrary = 109,
        //
        // Summary:
        //     Data sources for a site. Value = 110.
        [EnumMember]
        DataSources = 110,
        //
        // Summary:
        //     Site template gallery. Value = 111.
        [EnumMember]
        WebTemplateCatalog = 111,
        //
        // Summary:
        //     User Information. Value = 112.
        [EnumMember]
        UserInformation = 112,
        //
        // Summary:
        //     Web Part gallery. Value = 113.
        [EnumMember]
        WebPartCatalog = 113,
        //
        // Summary:
        //     List Template gallery. Value = 114.
        [EnumMember]
        ListTemplateCatalog = 114,
        //
        // Summary:
        //     XML Form library. Value = 115.
        [EnumMember]
        XMLForm = 115,
        //
        // Summary:
        //     Master Page gallery. Value = 116.
        [EnumMember]
        MasterPageCatalog = 116,
        //
        // Summary:
        //     No Code Workflows. Value = 117.
        [EnumMember]
        NoCodeWorkflows = 117,
        //
        // Summary:
        //     Custom Workflow Process. Value = 118.
        [EnumMember]
        WorkflowProcess = 118,
        //
        // Summary:
        //     Wiki Page Library. Value = 119.
        [EnumMember]
        WebPageLibrary = 119,
        //
        // Summary:
        //     Custom grid for a list. Value = 120.
        [EnumMember]
        CustomGrid = 120,
        [EnumMember]
        SolutionCatalog = 121,
        [EnumMember]
        NoCodePublic = 122,
        [EnumMember]
        ThemeCatalog = 123,
        [EnumMember]
        DesignCatalog = 124,
        [EnumMember]
        AppDataCatalog = 125,
        //
        // Summary:
        //     Data connection library for sharing information about external data connections.
        //     Value = 130.
        [EnumMember]
        DataConnectionLibrary = 130,
        //
        // Summary:
        //     Workflow History. Value = 140.
        [EnumMember]
        WorkflowHistory = 140,
        //
        // Summary:
        //     Project Tasks. Value = 150.
        [EnumMember]
        GanttTasks = 150,

        /// <summary>
        /// SP2013, Tasks, SAAS-1076
        /// </summary>
        [EnumMember]
        TasksWithTimelineAndHierarchy = 171,

        //
        // Summary:
        //     Meeting Series (Meeting). Value = 200.
        [EnumMember]
        Meetings = 200,
        //
        // Summary:
        //     Agenda (Meeting). Value = 201.
        [EnumMember]
        Agenda = 201,
        //
        // Summary:
        //     Attendees (Meeting). Value = 202.
        [EnumMember]
        MeetingUser = 202,
        //
        // Summary:
        //     Decisions (Meeting). Value = 204.
        [EnumMember]
        Decision = 204,
        //
        // Summary:
        //     Objectives (Meeting). Value = 207.
        [EnumMember]
        MeetingObjective = 207,
        //
        // Summary:
        //     Text Box (Meeting). Value = 210.
        [EnumMember]
        TextBox = 210,
        //
        // Summary:
        //     Things To Bring (Meeting). Value = 211.
        [EnumMember]
        ThingsToBring = 211,
        //
        // Summary:
        //     Workspace Pages (Meeting). Value = 212.
        [EnumMember]
        HomePageLibrary = 212,
        //
        // Summary:
        //     Posts (Blog). Value = 301.
        [EnumMember]
        Posts = 301,
        //
        // Summary:
        //     Comments (Blog). Value = 302.
        [EnumMember]
        Comments = 302,
        //
        // Summary:
        //     Categories (Blog). Value = 303.
        [EnumMember]
        Categories = 303,
        [EnumMember]
        Facility = 402,
        [EnumMember]
        Whereabouts = 403,
        [EnumMember]
        CallTrack = 404,
        [EnumMember]
        Circulation = 405,
        [EnumMember]
        Timecard = 420,
        [EnumMember]
        Holidays = 421,
        [EnumMember]
        StatusList = 432,
        [EnumMember]
        ReportLibrary = 433,
        [EnumMember]
        IMEDic = 499,
        [EnumMember]
        ExternalList = 600,
        [EnumMember]
        AssetLibrary = 851,
        //
        // Summary:
        //     Issue tracking. Value = 1100.
        [EnumMember]
        IssueTracking = 1100,
        //
        // Summary:
        //     Administrator Tasks. Value = 1200.
        [EnumMember]
        AdminTasks = 1200,
        [EnumMember]
        HealthRules = 1220,
        [EnumMember]
        HealthReports = 1221,
        [EnumMember]
        SlideLibrary = 2100,
        [EnumMember]
        ConvertedForms = 10102,
        [EnumMember]
        ContentLibrary = 30000,
        [EnumMember]
        MediaLibrary = 32888,
        [EnumMember]
        PagesLibrary = 850,
        [EnumMember]
        ImagesLibrary = 851,
        [EnumMember]
        RecordLib = 1302,
        [EnumMember]
        AccessRequest = 160,
        [EnumMember]
        DeveloperSiteDraftApps = 0x4ce,
        [EnumMember]
        MaintenanceLogs = 0xaf,
        [EnumMember]
        MySiteDocumentLibrary = 700,
        [EnumMember]
        MicroFeed = 544,
        [EnumMember]
        AnnoucementTile = 563,
        [EnumMember]
        CommunityMember = 880
    }

    // Summary:
    //     Specifies the base type for a list.
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum BaseType
    {
        // Summary:
        //     No base type specified.
        [EnumMember]
        UnspecifiedBaseType = -1,
        //
        // Summary:
        //     Generic type of list template used for most lists.
        [EnumMember]
        GenericList = 0,
        //
        // Summary:
        //     Document library.
        [EnumMember]
        DocumentLibrary = 1,
        //
        // Summary:
        //     Unused.
        [EnumMember]
        Unused = 2,
        //
        // Summary:
        //     Discussion board.
        [EnumMember]
        DiscussionBoard = 3,
        //
        // Summary:
        //     Survey list.
        [EnumMember]
        Survey = 4,
        //
        // Summary:
        //     Issue-tracking list.
        [EnumMember]
        Issue = 5,
    }
}
