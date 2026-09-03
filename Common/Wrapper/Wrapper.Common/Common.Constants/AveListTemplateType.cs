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






namespace AvePoint.Wrapper.Common
{
    // Summary:
    // Copy from ListTemplateType

    //     Specifies the type of a list definition or a list template.
    public enum AveListTemplateType
    {
        // Summary:
        //     Not used.
        InvalidType = -1,
        NoListTemplate = 0,
        //
        // Summary:
        //     Custom list.
        GenericList = 100,
        //
        // Summary:
        //     Document library.
        DocumentLibrary = 101,
        //
        // Summary:
        //     Survey.
        Survey = 102,
        //
        // Summary:
        //     Links.
        Links = 103,
        //
        // Summary:
        //     Announcements.
        Announcements = 104,
        //
        // Summary:
        //     Contacts.
        Contacts = 105,
        //
        // Summary:
        //     Calendar.
        Events = 106,
        //
        // Summary:
        //     Tasks.
        Tasks = 107,
        TasksWithTimelineAndHierarchy = 171,
        //
        // Summary:
        //     Discussion board.
        DiscussionBoard = 108,
        //
        // Summary:
        //     Picture library.
        PictureLibrary = 109,
        //
        // Summary:
        //     Data sources for a site.
        DataSources = 110,
        //
        // Summary:
        //     Site template gallery.
        WebTemplateCatalog = 111,
        //
        // Summary:
        //     User Information.
        UserInformation = 112,
        //
        // Summary:
        //     Web Part gallery.
        WebPartCatalog = 113,
        //
        // Summary:
        //     List template gallery.
        ListTemplateCatalog = 114,
        //
        // Summary:
        //     XML Form library.
        XMLForm = 115,
        //
        // Summary:
        //     Master Page gallery.
        MasterPageCatalog = 116,
        //
        // Summary:
        //     No Code Workflows.
        NoCodeWorkflows = 117,
        //
        // Summary:
        //     Custom Workflow Process.
        WorkflowProcess = 118,
        //
        // Summary:
        //     Wiki Page Library.
        WebPageLibrary = 119,
        //
        // Summary:
        //     Custom grid for a list.
        CustomGrid = 120,
        SolutionCatalog = 121,
        NoCodePublic = 122,
        ThemeCatalog = 123,

        //Theme
        DesignCatalog = 124,
        AppDataCatalog = 125,

        DataConnectionLibrary = 130,
        //
        // Summary:
        //     Workflow History.
        WorkflowHistory = 140,
        //
        // Summary:
        //     Project Tasks.
        GanttTasks = 150,
        //
        // Summary:
        //     Maintenance Log Library.
        MaintenanceLogLibrary = 175,
        //
        // Summary:
        //     Meeting Series (Meeting).
        Meetings = 200,
        //
        // Summary:
        //     Agenda (Meeting).
        Agenda = 201,
        //
        // Summary:
        //     Attendees (Meeting).
        MeetingUser = 202,
        //
        // Summary:
        //     Decisions (Meeting).
        Decision = 204,
        //
        // Summary:
        //     Objectives (Meeting).
        MeetingObjective = 207,
        //
        // Summary:
        //     Text Box (Meeting).
        TextBox = 210,
        //
        // Summary:
        //     Things To Bring (Meeting).
        ThingsToBring = 211,
        //
        // Summary:
        //     Workspace Pages (Meeting).
        HomePageLibrary = 212,
        //
        // Summary:
        //     Posts (Blog).
        Posts = 301,
        //
        // Summary:
        //     Comments (Blog).
        Comments = 302,
        //
        // Summary:
        //     Categories (Blog).
        Categories = 303,

        SiteCollectionAppCatalog = 336,
        Facility = 402,
        Whereabouts = 403,
        CallTrack = 404,
        Circulation = 405,
        Timecard = 420,
        Holidays = 421,
        IMEDic = 499,
        Social=550,
        ExternalList = 600,
        //
        // Summary:
        //     Issue tracking.
        IssueTracking = 1100,
        //
        // Summary:
        //     Administrator Tasks.
        AdminTasks = 1200,
        HealthRules = 1220,
        HealthReports = 1221,
        PreservationHoldLibrary = 1310,
        NintexWrokflow = 5001,
        WFSVC = 4501,
        PagesLibrary = 850,
        ImagesLibrary = 851,
        RecordLib = 1302,

        AccessRequest = 160,
        DeveloperSiteDraftApps = 0x4ce,
        MaintenanceLogs = 0xaf,
        MySiteDocumentLibrary = 700,        
        MicroFeed = 544,
        AnnoucementTile = 563,
        /// <summary>
        /// [LS]Documents library in O365 OneDrive, value is 700
        /// </summary>
        OneDriveDocumentLibrary = 700,
        CommunityMember = 880,
        ExternalSubscriptionStore = 2001,

        AccessApp = 3100,
        AlchemyMobileForm = 3101,
        AlchemyApprovalWorkflow = 3102,
        SharingLinks = 3300,
        HashtagStore = 3400,
        RecipesTable = 3410,
        FormulasTable = 3411,
        WebTemplateExtensionsList = 3415,
        ItemReferenceCollection = 3500,
        ItemReferenceReference = 3501,
        ItemReferenceReferenceCollection = 3502
    }
}
