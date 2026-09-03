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
namespace Microsoft.Office.Project.Server.Interfaces
{
    using Microsoft.Office.Project.Server.BusinessLayer;
    using Microsoft.Office.Project.Server.Library;
    using Microsoft.Office.Project.Server.Schema;
    using System;
    using System.Data;
    using System.ServiceModel;

    [ServiceContract(Namespace = "http://schemas.microsoft.com/office/project/server/webservices/PWA/", Name = "PWA")]
    [XmlSerializerFormat(Style = OperationFormatStyle.Document)]
    public interface IPWA
    {
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/ProjectReadServerTimelineDataForJSON", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/ProjectReadServerTimelineDataForJSONResponse")]
        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        string ProjectReadServerTimelineDataForJSON(Guid projUid, DataStoreEnum storeId);

        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/ProjectUpdateServerTimelineData", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/ProjectUpdateServerTimelineDataResponse")]
        void ProjectUpdateServerTimelineData(Guid timelineType, string tlData);


        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/ProjectGetProjectActiveSession", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/ProjectGetProjectActiveSessionResponse")]
        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        string ProjectGetProjectActiveSession(Guid projectUid);

        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/ProjectCheckOutProjectWithResult", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/ProjectCheckOutProjectWithResultResponse")]
        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        bool ProjectCheckOutProjectWithResult(Guid projectUid, Guid sessionUid, string sessionDescription);

        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/ProjectUpdateTeam", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/ProjectUpdateTeamResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        string ProjectUpdateTeam(string updatesJson, Guid projUid, Guid sessionUid, bool checkInProject);

        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/ProjectQueueCheckInProject", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/ProjectQueueCheckInProjectResponse")]
        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        void ProjectQueueCheckInProject(Guid jobUid, Guid projectUid, bool force, Guid sessionUid, string sessionDescription);

        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminReadSiteMapTyped", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminReadSiteMapTypedResponse")]
        SiteMapDataSet AdminReadSiteMapTyped();

        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminSetSiteMap", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminSetSiteMapResponse")]
        void AdminSetSiteMap(SiteMapDataSet newSiteMap);

        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminReadServerConfigSettings", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminReadServerConfigSettingsResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        WebAdminDataSet AdminReadServerConfigSettings();

        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/SecuritySetPermissionMode", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/SecuritySetPermissionModeResponse")]
        void SecuritySetPermissionMode(Security.PermissionMode newMode);

        [FaultContract(typeof(DefaultServerFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminUpdateServerConfigSettings", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminUpdateServerConfigSettingsResponse")]
        [FaultContract(typeof(ServerExecutionFault))]
        void AdminUpdateServerConfigSettings(WebAdminDataSet settings);

        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminGetUserWebpartProperties", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminGetUserWebpartPropertiesResponse")]
        DataSet AdminGetUserWebpartProperties(string webpartId);

        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminPersistUserWebpartProperties", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminPersistUserWebpartPropertiesResponse")]
        void AdminPersistUserWebpartProperties(string webpartId, string[] propnames, string[] propstrings, object[] propobjects);

        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/ProjectGetTaskItemLink", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/ProjectGetTaskItemLinkResponse")]
        string ProjectGetTaskItemLink(Guid projectUid, Guid taskUid);

        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminGetGanttSettingsForGridTyped", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminGetGanttSettingsForGridTypedResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        GanttSettingsDataSet AdminGetGanttSettingsForGridTyped();

        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminReadGanttSchemes", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminReadGanttSchemesResponse")]
        GanttSchemesDataSet AdminReadGanttSchemes();

        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminUpdateGanttSettings", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminUpdateGanttSettingsResponse")]
        int AdminUpdateGanttSettings(GanttSettingsDataSet dsDelta);

        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminRenameGanttScheme", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminRenameGanttSchemeResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        void AdminRenameGanttScheme(Guid GanttSchemeUID, string NewName);

        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminGetGroupSettingsForGridTyped", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminGetGroupSettingsForGridTypedResponse")]
        GroupSettingsDataSet AdminGetGroupSettingsForGridTyped();

        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminReadGroupSchemes", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminReadGroupSchemesResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        GroupSchemesDataSet AdminReadGroupSchemes();

        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminRenameGroupScheme", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminRenameGroupSchemeResponse")]
        void AdminRenameGroupScheme(Guid GroupSchemeUID, string NewName);

        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminUpdateGroupSettings", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminUpdateGroupSettingsResponse")]
        int AdminUpdateGroupSettings(GroupSettingsDataSet dsDelta);

        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/ProjectReadTasksForTimeline", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/ProjectReadTasksForTimelineResponse")]
        [FaultContract(typeof(DefaultServerFault))]
        DataSet ProjectReadTasksForTimeline(Guid projectUid, string viewName, int storeId);

        // Token: 0x060001EE RID: 494
        [FaultContract(typeof(DefaultServerFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/ProjectSaveTasksForTimeline", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/ProjectSaveTasksForTimelineResponse")]
        [FaultContract(typeof(ServerExecutionFault))]
        void ProjectSaveTasksForTimeline(Guid projectUid, string viewName, string tlViewData);

        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminDeleteFiscalYears", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminDeleteFiscalYearsResponse")]
        [FaultContract(typeof(ServerExecutionFault))]
        [FaultContract(typeof(DefaultServerFault))]
        void AdminDeleteFiscalYears(int[] fiscalYears);

        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminUpdateFiscalPeriods", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/AdminUpdateFiscalPeriodsResponse")]
        void AdminUpdateFiscalPeriods(FiscalPeriodDataSet dsDelta);

        [FaultContract(typeof(DefaultServerFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/ProjectReadProjectImpactsForPWA", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/ProjectReadProjectImpactsForPWAResponse")]
        [FaultContract(typeof(ServerExecutionFault))]
        ProjectImpactDataSet ProjectReadProjectImpactsForPWA(Guid projectUid, Guid pdpUid, Guid webPartUid);

        [FaultContract(typeof(DefaultServerFault))]
        [FaultContract(typeof(ServerExecutionFault))]
        [OperationContract(Action = "http://schemas.microsoft.com/office/project/server/webservices/PWA/ProjectQueueUpdateProjectImpactsForPWA", ReplyAction = "http://schemas.microsoft.com/office/project/server/webservices/PWA/ProjectQueueUpdateProjectImpactsForPWAResponse")]
        void ProjectQueueUpdateProjectImpactsForPWA(Guid jobUid, Guid sessionUid, ProjectImpactDataSet projectImpactDataSet, Guid pdpUid, Guid webPartUid);


    }
}
