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
    using System.Collections.Generic;

    public enum AveStatus
    {
        Successful = 0,
        Failed,
        Skipped,
    }
    /// <summary>
    /// Type of reportDto. if you are adding an enum value, add a value in AveReportObjectTypeExtension.DisplayNameMapping corresponding
    /// </summary>
    public enum AveReportObjectType
    {
        Undefined = 0,
        #region workflow
        ListWorkflowDefinition,
        ListCTWorkflowDefinition,
        WebWorkflowDefinition,
        WebCTWorkflowDefinition,
        ProjectWorkflowDefinition,
        WorkflowInstance,
        #endregion
        #region content type
        ListContentType,
        WebContentType,
        #endregion
        #region Field
        ListField,
        WebField,
        #endregion
        #region securitytriming
        ListSelf,
        CreateList,
        ListProperty,
        ListCreatedBy,
        ListScheduledItemSetting,
        ListRatingSetting,
        ListRootFolder,
        User,
        RoleAssignment,
        CreateNewSite,
        SiteSetting,
        SiteFeature,
        WebFeature,
        Group,
        WebSelf,
        WebProperty,
        WebNavigation,
        WebRoles,
        WebRoleAssignments,
        EventReceiver,
        WebSearch,
        WebSearchScope,
        WebSearchKeyWords,
        SiteSearch,
        SiteSearchScope,
        SiteSearchKeyWords,
        UserSettings,
        WebMetaInfo,
        UpdateContentType,
        ListRoleAssignments,
        SiteLogoUrl,
        AssociteGroup,
        NavNodes,
        RestoreHiddenSiteProperty,
        RestoreUrlIDNeedReplace,
        RestoreDataSourceFields,
        RestoreUrlNeedPost,
        RestoreMasterPageProperty,
        RestoreCalendarSettings,
        RestoreLookupFieldValues,
        RestoreMetadataService,
        Folder,
        DataJunctions,
        Alert,
        RequestAccessEmail,
        SiteTheme,
        AlternateCSSUrl,
        WelcomePage,
        HiddenPageProperty,
        RestorePostUserInfo,
        CacheProfileListId,
        RelationShipListSetting,
        EmailSubmittedRecordsListIDProperty,
        OriginTitle,
        ContentOrganizationSetting,
        ListSetting,
        MetadataNavigationSettings,
        DocumentTemplateUrl,
        ListRssViewField,
        ListDefaultValue,
        GroupOwner,
        HoldRecord,
        LookupFields,
        CreateItem,
        DocumentTag,
        SocialTag,
        SocailComment,
        RoleAssignments,
        WebPart,
        #endregion

    }

    public static class AveReportObjectTypeExtension
    {
        private static Dictionary<AveReportObjectType, string> displayNameMapping;
        private static Dictionary<AveReportObjectType, string> DisplayNameMapping
        {
            get
            {
                if (displayNameMapping == null)
                {
                    displayNameMapping = new Dictionary<AveReportObjectType, string>(16)
                    {
                        {AveReportObjectType.Undefined,"Unknown"},
                        {AveReportObjectType.ListWorkflowDefinition,"List Workflow Definition"},
                        {AveReportObjectType.ListCTWorkflowDefinition,"Workflow Definition for List Content Type"},
                        {AveReportObjectType.WebWorkflowDefinition,"Site Workflow Definition"},
                        {AveReportObjectType.WebCTWorkflowDefinition,"Workflow Definition for Site Content Type"},
                        {AveReportObjectType.WorkflowInstance,"Workflow Instance"},
                        {AveReportObjectType.ListContentType,"List Content Type"},
                        {AveReportObjectType.WebContentType,"Site Content Type"},
                        {AveReportObjectType.ListField,"List Column"},
                        {AveReportObjectType.WebField,"Site Column"},
                        {AveReportObjectType.ListSelf,"ListSelf"},
                        {AveReportObjectType.ListProperty,"List Property"},
                        {AveReportObjectType.ListRootFolder,"List RootFolder"},
                        {AveReportObjectType.User,"User"},
                        {AveReportObjectType.RoleAssignment,"RoleAssignment"},
                        {AveReportObjectType.CreateNewSite,"CreateNewSite"},
                        {AveReportObjectType.SiteSetting,"SiteSetting"},
                    };
                }
                return displayNameMapping;

            }
        }
        public static string GetDisplayName(this AveReportObjectType self)
        {
            if (DisplayNameMapping.ContainsKey(self))
            {
                return DisplayNameMapping[self];
            }
            return self.ToString();
        }
    }
}
