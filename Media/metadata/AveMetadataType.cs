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

namespace AvePoint.Metadata
{
    public enum AveMetadataType
    {
        #region Common
        Unknown,
        Security,
        Navigation,
        LanguageFile,
        // add for new wrapper

        UserCache,
        GroupCache,
        AudienceCache,

        UserProfile,
        Users,
        Groups,
        Roles,
        RoleAssignment,
        ItemTableInfo,
        #endregion

        #region WebApp
        WebAppFeature,
        WebAppProperty,
        WebAppPath,
        WebAppPolicyRole,
        WebAppPolicy,

        #endregion

        #region Site
        SiteBasicInfo,
        SiteProperty,
        SiteFeature,
        SiteSearchInfo,
        TeamInfo,

        SearchScope,
        SearchKeywords,
        #endregion

        #region Web
        WebBasicInfo,
        WebProperty,
        WebFeature,
        WebContentType,
        WebField,
        WebWorkflowAssociation,
        WebCTWorkflowAssociation,
        WebWorkflowInstance,
        WebWorkflowSchedule,
        WebEventReceiver,
        WebWorkflowTemplate,

        WebProjectPolicy,
        SocialFeed, //Added to support backing up / restoring social feeds

        #endregion

        #region List
        ListBasicInfo,
        ListProperty,
        ListField,
        ListContentType,
        ListWorkflowAssociation,
        ListCTWorkflowAssociation,
        ListEventReceiver,
        #endregion

        #region Project

        ProjectCalendar,
        ProjectLookupTable,
        ProjectCustomField,
        ProjectEnterpriseResource,
        ProjectPhase,
        ProjectStage,
        ProjectTimesheet,
        ProjectEnterpriseProjectType,
        ProjectWorkflowAssociation,
        ProjectTimeline,

        #endregion

        #region ListItem
        ListItemInfo,
        #endregion

        #region Doc
        DocProperty,
        DocData,
        DocDataJunction,
        DocWebPart,
        DocImmedSubscriptions,
        DocSchedSubscriptions,
        [Obsolete("This metadata type is not used anymore.")]
        DocSystemInfo,
        [Obsolete("This metadata type is not used anymore.")]
        DocRbsId,
        [Obsolete("This metadata type is not used anymore.")]
        DocStorageInfo,
        [Obsolete("This metadata type is not used anymore.")]
        DocVersions,
        //For replicator
        LookupFieldGuidValue,
        #endregion

        #region Attachment
        AttachmentData,
        #endregion

        DocumentTagging,
        FullSchemaXml,
        FullTextIndex,
        Report,
        MetadataEnd,

        #region MetadataService
        MetadataService,
        MetadataTermStore,
        MetadataGroup,
        MetadataTermSet,
        MetadataTerm,
        #endregion

        #region User Profile
        UserProfileMembership,
        UserProfileLink,
        UserProfileDetail,
        UserProfileProperties,
        UserProfileTag,
        UserProfileComment,
        UserProfileColleague,
        #endregion

        SocialTag,
        SocialComment,
        ContentTypeHub,
        WorkflowInstance,
        WorkflowSchedule,
        WorkflowTemplate,
        AppPackageInfo,

        #region ExchangeOnline
        ExchangeMailBox,
        ExchangeFolder,
        ExchangeItem,
        ExchangeFolderPermission,
        ExchangeUserConfiguration,
        ExchangeFolderMetadata,
        ExchangeMicrosoftTeams,
        ExchangeMicrosoftTeamsConversationItem,
        ExchangePlannerPlan,
        ExchangePlannerTask,
        ExchangePlannerTaskAttachment,
        ExchangeCalendarEvent,
        ExchangeAttachment,
        #endregion

        #region Yammer
        YammerGroup,
        YammerConversation,
        #endregion

        #region DPM Test Run
        ActiveFeature,
        DeActiveFeature,
        DependentFeature,
        #endregion

        ItemMetadataDto,

        #region RP real time event
        ListFieldDelete,
        #endregion
        //Used by Nintex workflow
        ReusableWorkflowTemplate,

        ProjectBasic,
        ProjectTimesheetSettings,
        ProjectQuichLaunch,
        ProjectTaskSettingsAndDisplay,
        ProjectADResourcePoolSyncSetting,
        ProjectAdditionalServerSettings,
        ProjectLineClassifications,
        ProjectAdministrativeTime,
        ProjectReportingSetting,
        ProjectUserSyncSettings,
        ProjectSecurityTemplates,
        ProjectCategories,
        ProjectGroups,
        ProjectResourcesAuthorization,
        ProjectDelegates,
        ProjectPermissions,
        ProjectViews,
        ProjectGanttSettings,
        ProjectGroupSettings,
        ProjectDelegateFilters,
        ProjectFiscalPeriods,
        ProjectReportingPeriods,
        ProjectDrivers,
        ProjectDriverPrioritizations,
        ProjectPortolioAnalyses,
        ProjectDependencies,

        ComplianceTag,

        #region Chat

        ChatUser,
        Chat,
        ChatMessage,

        #endregion

        TeamsChannel,
        ChatDetails,
        ChatMessageDetails,

        #region PowerBI
        PBIWorkspaceBasic,
        PBIWorkspaceUsers,
        PBIReportBasic,

        #endregion

        #region Drive
        Drive,
        DriveItem,
        DriveItemPermission,
        #endregion

        #region PowerPlatform
        PowerFlowBasic,
        PowerFlowPermission,
        PowerAppBasic,
        PowerAppPermission,
        #endregion

        #region Site and List
        List,
        #endregion
    }
}