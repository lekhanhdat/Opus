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
        #region workflow 1-10
        ListWorkflowDefinition=1,
        ListCTWorkflowDefinition,
        WebWorkflowDefinition,
        WebCTWorkflowDefinition,
        WorkflowInstance,
        WorkflowTemplate,
        #endregion
        #region content type 11-20
        ListContentType=11,
        WebContentType,
        #endregion
        #region Field 21-30
        ListField=21,
        WebField,
        #endregion
        #region Property 31-40
        SiteSetting=31,
        WebProperty,
        ListProperty,
        #endregion
        #region Security 41-50
        User=41,
        Group,
        RoleAssignment,
        WebRoles,
        UserSettings,
        #endregion
        #region Feature 51-60
        SiteFeature=51,
        WebFeature,
        EventReceiver,
        #endregion
        #region post action
        /*
        /// <summary>
        /// Used for clear navigation
        /// </summary>
        WebNavigation,
        /// <summary>
        /// Used for restore navigation in post action 
        /// </summary>
        NavNodes,
        /// <summary>
        /// Restore site logo url in post action
        /// </summary>
        SiteLogoUrl,
        /// <summary>
        /// Restore Associate Group in post action
        /// </summary>
        AssociteGroup,
        /// <summary>
        /// Restore Hidden site property in post action
        /// </summary>
        RestoreHiddenSiteProperty,
        /// <summary>
        /// Replace Url Id in post action
        /// </summary>
        RestoreUrlIDNeedReplace,
        /// <summary>
        /// Restore data source fields in post action
        /// </summary>
        RestoreDataSourceFields,
        /// <summary>
        /// Replace url in post action
        /// </summary>
        RestoreUrlNeedPost,
        /// <summary>
        /// Restore Master Page property in post action
        /// </summary>
        RestoreMasterPageProperty,
        /// <summary>
        /// Restore CalenderSetting in Post Action
        /// </summary>
        RestoreCalendarSettings,
        /// <summary>
        /// Restore Lookup Field values in post action
        /// </summary>
        RestoreLookupFieldValues,
        /// <summary>
        /// In post action, request access email in web
        /// </summary>
        RequestAccessEmail,
        /// <summary>
        /// in web post action, site theme css url
        /// </summary>
        SiteThemeCssFolderUrl,
        /// <summary>
        /// In Post Action, AlternateCssUrl
        /// </summary>
        AlternateCSSUrl,
        /// <summary>
        /// In post action, welcome page of folder
        /// </summary>
        WelcomePage,
        /// <summary>
        /// In post action, Hidden page property
        /// </summary>
        HiddenPageProperty,
        /// <summary>
        /// In post action, __CacheProfileListId in web property.
        /// </summary>
        CacheProfileListId,
        /// <summary>
        ///  In post action, _VarRelationshipsListId in web property.
        /// </summary>
        RelationShipListSetting,
        /// <summary>
        /// In post action, emailsubmittedrecordslistid when using content orginazer
        /// </summary>
        EmailSubmittedRecordsListIDProperty,
        /// <summary>
        /// In post action, site bin restore web title, never used in DocAve6 
        /// </summary>
        OriginTitle,
        /// <summary>
        /// Used in post action, content organization settings
        /// </summary>
        ContentOrganizationSetting,
        /// <summary>
        /// Used in post action, restore list setting
        /// </summary>
        ListSetting,
        /// <summary>
        /// Used in post action, metadata navigation setting
        /// </summary>
        MetadataNavigationSettings,
        /// <summary>
        /// Used in post action, document template url
        /// </summary>
        DocumentTemplateUrl,
        /// <summary>
        /// Used in post action, replace content in client_LocationBasedDefaults.html
        /// </summary>
        ListDefaultValue,
        /// <summary>
        /// Used in post action, restore group owner
        /// </summary>
        GroupOwner,
        /// <summary>
        /// Used in post action, hold and record setting
        /// </summary>
        HoldRecord,
        /// <summary>
        /// Used in post action, restore lookup fields
        /// </summary>
        LookupFields,
        */
        #endregion
        #region Service 61-80
        #region Search Service
        WebSearch=61,
        WebSearchScope,
        WebSearchKeyWords,
        SiteSearch,
        SiteSearchScope,
        SiteSearchKeyWords,
        #endregion
        MetadataService,
        #endregion
        #region Alert 81-90
        /// Alert
        /// </summary>
        Alert=81,
        #endregion
        #region Socail 91-100
        /// <summary>
        /// Name and objTitle incorrect
        /// </summary>        
        SocialTag=91,
        /// <summary>
        /// Name and objTitle incorrect
        /// </summary>
        SocailComment,
        /// <summary>
        /// Name and objTitle incorrect
        /// </summary>
        SocialFeed,
        #endregion
        #region View&Webpart 101-110
        /// <summary>
        /// objTitle incorrect
        /// </summary>
        WebPart=101,
        /// <summary>
        /// restore personal view
        /// </summary>
        PersonalView,
        #endregion
        #region SP App 111-120
        App=111,
        #endregion
        /// <summary>
        /// update Field Vaule, Name incorrect
        /// </summary>
        UpdateField,
        /// <summary>
        /// update property in list root folder
        /// </summary>
        UpdateContentType,
        /// <summary>
        /// DataJunction, name and objTitle incorrect
        /// </summary>
        /// <summary>
        DataJunctions, 
             
        WebNavigation,
        
    }

    public static class AveReportObjectTypeExtension
    {
        static AveReportObjectTypeExtension()
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
                        //{AveReportObjectType.ListSelf,"ListSelf"},
                        {AveReportObjectType.ListProperty,"List Property"},
                        {AveReportObjectType.User,"User"},
                        {AveReportObjectType.RoleAssignment,"RoleAssignment"},
                        {AveReportObjectType.SiteSetting,"SiteSetting"},
                    };
        }
        //Oliver:只在静态构造方法中初始化
        private static readonly Dictionary<AveReportObjectType, string> displayNameMapping;
        
        public static string GetDisplayName(this AveReportObjectType self)
        {
            if (displayNameMapping.ContainsKey(self))
            {
                return displayNameMapping[self];
            }
            return self.ToString();
        }
    }
}
