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
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public enum AveReportResource
    {
        Wrapper_Report_AddEventReceiverError,
        Wrapper_Report_AddGroupContributorError,
        Wrapper_Report_AddTermSetStakeholderError,
        Wrapper_Report_CannotCreateMMSSession,
        Wrapper_Report_CannotFindWebPartAssembly,
        Wrapper_Report_CannotGetScope,
        Wrapper_Report_CannotGetViewByID,
        Wrapper_Report_CannotGetWebPart,
        Wrapper_Report_CannotGetWebPartObject,
        Wrapper_Report_ClearMultipleDataListDefaultFoldersError,
        Wrapper_Report_CreateNewDisplayGroupError,
        Wrapper_Report_CreateNewScopeError,
        Wrapper_Report_CreateNewScopeRuleError,
        Wrapper_Report_CreateSiteCollectionError,
        Wrapper_Report_CreateSiteGroupError,
        Wrapper_Report_CreateSubTermError,
        Wrapper_Report_CreateTermError,
        Wrapper_Report_CreateTermGroupError,
        Wrapper_Report_CreateTermSetError,
        Wrapper_Report_DonnotHavePermissionRestoreTag,
        Wrapper_Report_GetOrCreateKeywordError,
        Wrapper_Report_GetSearchServiceApplicationProxyError,
        Wrapper_Report_GroupRestoreFailedError,
        Wrapper_Report_GroupRestoreSkipError,
        Wrapper_Report_ListPropertyRestoreFailed,
        Wrapper_Report_NoPermissionToAddFolder,
        Wrapper_Report_NoPermissionToClearWebNavigation,
        Wrapper_Report_NoPermissionToCreateSite,
        Wrapper_Report_NoPermissionToDeleteWebNavigations,
        Wrapper_Report_NoPermissionToRestoreAlternateCSSUrl,
        Wrapper_Report_NoPermissionToRestoreAssociateGroups,
        Wrapper_Report_NoPermissionToRestoreCacheProfileListId,
        Wrapper_Report_NoPermissionToRestoreCalendarSettings,
        Wrapper_Report_NoPermissionToRestoreConentTypes,
        Wrapper_Report_NoPermissionToRestoreContentOrganizationSetting,
        Wrapper_Report_NoPermissionToRestoreDataJunctions,
        Wrapper_Report_NoPermissionToRestoreDataSourceFields,
        Wrapper_Report_NoPermissionToRestoreDocumentTemplateUrl,
        Wrapper_Report_NoPermissionToRestoreEmailSubmittedRecordsListIDProperty,
        Wrapper_Report_NoPermissionToRestoreEventReceiver,
        Wrapper_Report_NoPermissionToRestoreGroup,
        Wrapper_Report_NoPermissionToRestoreGroupOwner,
        Wrapper_Report_NoPermissionToRestoreHiddenPageProperty,
        Wrapper_Report_NoPermissionToRestoreHiddenSiteProperty,
        Wrapper_Report_NoPermissionToRestoreHoldRecord,
        Wrapper_Report_NoPermissionToRestoreItem,
        Wrapper_Report_NoPermissionToRestoreItemRoleAssignments,
        Wrapper_Report_NoPermissionToRestoreListDefaultView,
        Wrapper_Report_NoPermissionToRestoreListRoleAssignments,
        Wrapper_Report_NoPermissionToRestoreListRssViewField,
        Wrapper_Report_NoPermissionToRestoreListSetting,
        Wrapper_Report_NoPermissionToRestoreLookupFields,
        Wrapper_Report_NoPermissionToRestoreLookupFieldValues,
        Wrapper_Report_NoPermissionToRestoreMasterPageProperty,
        Wrapper_Report_NoPermissionToRestoreMetadataService,
        Wrapper_Report_NoPermissionToRestoreNavigationSettings,
        Wrapper_Report_NoPermissionToRestoreNavNodes,
        Wrapper_Report_NoPermissionToRestoreRelationShipListSetting,
        Wrapper_Report_NoPermissionToRestoreRequestAccessEmail,
        Wrapper_Report_NoPermissionToRestoreRoles,
        Wrapper_Report_NoPermissionToRestoreSiteFeature,
        Wrapper_Report_NoPermissionToRestoreSiteLogo,
        Wrapper_Report_NoPermissionToRestoreSiteSearch,
        Wrapper_Report_NoPermissionToRestoreSiteTheme,
        Wrapper_Report_NoPermissionToRestoreSocailTag,
        Wrapper_Report_NoPermissionToRestoreSocialComment,
        Wrapper_Report_NoPermissionToRestoreUrlIDNeedReplace,
        Wrapper_Report_NoPermissionToRestoreUrlNeedPost,
        Wrapper_Report_NoPermissionToRestoreUser,
        Wrapper_Report_NoPermissionToRestoreUserSetting,
        Wrapper_Report_NoPermissionToRestoreWebFeature,
        Wrapper_Report_NoPermissionToRestoreWebMetaInfo,
        Wrapper_Report_NoPermissionToRestoreWebPart,
        Wrapper_Report_NoPermissionToRestoreWebRoleAssignments,
        Wrapper_Report_NoPermissionToRestoreWebSearch,
        Wrapper_Report_NoPermissionToRestoreWebSetting,
        Wrapper_Report_NoPermissionToRestoreWelcomePage,
        Wrapper_Report_NoPermissionToUpdateContentType,
        Wrapper_Report_NoPermissionToUpdateListRootFolder,
        Wrapper_Report_NoPermissionToUpdateListSetting,
        Wrapper_Report_NoPermissionToUpdateSiteSetting,
        Wrapper_Report_NotRelativeToMetadataService,
        Wrapper_Report_ProcessListRatingSettingError,
        Wrapper_Report_RestoreAlertError,
        Wrapper_Report_RestoreAveMetadataServiceError,
        Wrapper_Report_RestoreBestBetsError,
        Wrapper_Report_RestoreCustomOrderError,
        Wrapper_Report_RestoreDocumentTaggingError,
        Wrapper_Report_RestoreKeywordError,
        Wrapper_Report_RestoreListPropertyError,
        Wrapper_Report_RestoreListRootFolderError,
        Wrapper_Report_RestoreSocialFeedError,
        Wrapper_Report_RestoreSynonymError,
        Wrapper_Report_RestoreTermFailed,
        Wrapper_Report_RestoreWebPartError,
        Wrapper_Report_SetDisplayGroupListInfoError,
        Wrapper_Report_SetNewCreateGroupPropertyError,
        Wrapper_Report_SetScheduledItemSettingError,
        Wrapper_Report_SetSubTermOwnerError,
        Wrapper_Report_SetSubTermPropertyError,
        Wrapper_Report_SetTermOwnerError,
        Wrapper_Report_SetTermPropertyError,
        Wrapper_Report_SetTermSetOwnerError,
        Wrapper_Report_SetTermSetPropertyError,
        Wrapper_Report_SetWebRequestAccessEmailError,
        Wrapper_Report_SkipListContentType,
        Wrapper_Report_SkipTheNoPermissionGroup,
        Wrapper_Report_SkipTheNoPermissionUser,
        Wrapper_Report_SkipBuiltInUser,
        Wrapper_Report_TheGroupIsUser,
        Wrapper_Report_TheUserIsGroup,
        Wrapper_Report_UpdateEventReceiverToListError,
        Wrapper_Report_UpdateEventReceiverToWebError,
        Wrapper_Report_UpdateGroupPropertyError,
        Wrapper_Report_UpdateWebPropertyError,
        Wrapper_Report_UserHasRestored,
        Wrapper_Report_UserRestoreFailedError,
        Wrapper_Report_UserRestoreSkipError,
        Wrapper_Report_ViewIDNotEqualError,



        Wrapper_Report_RestorePersonalViewError,
        Wrapper_Report_UserPermissionNotEnough,
        Wrapper_Report_CannotUpdateColumValueError,
        Wrapper_Report_GetTermSetByNameError,
        Wrapper_Report_NoPermissionRestoreItemRoleAssignment,
        Wrapper_Report_Office365EnvironmentIssue,
        Wrapper_Report_SkipCheckOutFileWorkflowInstance,
        Wrapper_Report_RestoreWFWithoutPermission,
        Wrapper_Report_UpdateListFailed,
        Wrapper_Report_None,

        Wrapper_Report_SkipFoundationMetadataColum,

    }
}