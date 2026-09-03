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

namespace AvePoint.Wrapper.Core.Common
{
    internal static class WrapperResourceKey
    {
        public const string Wrapper_UnsupportedMode = "Wrapper_UnsupportedMode";
        public const string Wrapper_FileNotFound = "Wrapper_FileNotFound";
        public const string Wrapper_NoAvailableActionAccordingToType = "Wrapper_NoAvailableActionAccordingToType";//"Cannot get action accoding to metadata type:{0}"
        public const string Wrapper_SiteNotFoundWithException = "Wrapper_SiteNotFoundWithException";
        public const string Wrapper_SiteNotFound = "Wrapper_SiteNotFound";
        public const string Wrapper_SiteExistInDifferentWebApp = "Wrapper_SiteExistInDifferentWebApp";
        public const string Wrapper_WebAppNotFound = "Wrapper_WebAppNotFound";
        public const string Wrapper_GetUserNameAndEmailFailed = "Wrapper_GetUserNameAndEmailFailed";
        public const string Wrapper_InvalidTemplateNumber = "Wrapper_InvalidTemplateNumber";
        public const string Wrapper_CreateSiteFailed = "Wrapper_CreateSiteFailed";
        public const string Wrapper_RestoreFailed = "Wrapper_RestoreFailed";
        public const string Wrapper_UsingOriginalLCID = "Wrapper_UsingOriginalLCID";
        public const string Wrapper_LanguageMappingNotFound = "Wrapper_LanguageMappingNotFound";
        public const string Wrapper_DuplicatedLanguageMapping = "Wrapper_DuplicatedLanguageMapping";
        public const string Wrapper_CreateSiteInfo = "Wrapper_CreateSiteInfo";
        public const string Wrapper_CreateMySiteInfo = "Wrapper_CreateMySiteInfo";
        public const string Wrapper_ExecutePostActionFailed = "Wrapper_ExecutePostActionFailed";
        public const string Wrapper_AddPortalUrlToPostAction = "Wrapper_AddPortalUrlToPostAction";
        public const string Wrapper_OptionIsNotEnabled = "Wrapper_OptionIsNotEnabled";
        public const string Wrapper_ActiveFeatureFailed = "Wrapper_ActiveFeatureFailed";
        public const string Wrapper_FeatureScopeNotMatch = "Wrapper_FeatureScopeNotMatch";
        public const string Wrapper_StartActiveFeature = "Wrapper_StartActiveFeature";
        public const string Wrapper_FeatureNotFound = "Wrapper_FeatureNotFound";
        public const string Wrapper_ActiveFeatureSuccessfully = "Wrapper_ActiveFeatureSuccessfully";
        public const string Wrapper_StartRestoreUser = "Wrapper_StartRestoreUser";
        public const string Wrapper_UserIsSystemAccount = "Wrapper_UserIsSystemAccount";
        public const string Wrapper_NoPermissionOrActiveUser = "Wrapper_NoPermissionOrActiveUser";
        public const string Wrapper_MigrateUserFailed = "Wrapper_MigrateUserFailed";
        public const string Wrapper_EnsureUserFailed = "Wrapper_EnsureUserFailed";
        public const string Wrapper_RestoreUserSettingFailed = "Wrapper_RestoreUserSettingFailed";
        public const string Wrapper_DeleteUser = "Wrapper_DeleteUser";
        public const string Wrapper_StartRestoreGroup = "Wrapper_StartRestoreGroup";
        public const string Wrapper_NoPermissionGroup = "Wrapper_NoPermissionGroup";
        public const string Wrapper_GroupNotFound = "Wrapper_GroupNotFound";
        public const string Wrapper_EnsureGroupFailed = "Wrapper_EnsureGroupFailed";
        public const string Wrapper_EnsureUserSuccessfully = "Wrapper_EnsureUserSuccessfully";
        public const string Wrapper_EnsureGroupSuccessfully = "Wrapper_EnsureGroupSuccessfully";
        public const string Wrapper_RestoreGroupDistributionGroupFailed = "Wrapper_RestoreGroupDistributionGroupFailed";
        public const string Wrapper_RestoreGroupMembersFailed = "Wrapper_RestoreGroupMembersFailed";
        public const string Wrapper_RestoreGroupSettingsFailed = "Wrapper_RestoreGroupSettingsFailed";
        public const string Wrapper_RestoreGroupSettingsSuccessfully = "Wrapper_RestoreGroupSettingsSuccessfully";
        public const string Wrapper_RestoreGroupOwnerFailed = "Wrapper_RestoreGroupOwnerFailed";
        public const string Wrapper_DeleteUserFailed = "Wrapper_DeleteUserFailed";
        public const string Wrapper_UserInfoNotFound = "Wrapper_UserInfoNotFound";
        public const string Wrapper_EnsureDefaultUserFailed = "Wrapper_EnsureDefaultUserFailed";
        public const string Wrapper_WriteResourceFileFailed = "Wrapper_WriteResourceFileFailed";
        public const string Wrapper_DeleteFileFailed = "Wrapper_DeleteFileFailed";
        public const string Wrapper_FindMappingNameInXmlMapping = "Wrapper_FindMappingNameInXmlMapping";
        public const string Wrapper_FindMappingName = "Wrapper_FindMappingName";
        public const string Wrapper_UserProfileIsNotAvailableInFoundation = "Wrapper_UserProfileIsNotAvailableInFoundation";
        public const string Wrapper_UserProfileIsNotAvailable = "Wrapper_UserProfileIsNotAvailable";
        public const string Wrapper_RestoreAudienceFailed = "Wrapper_RestoreAudienceFailed";
        public const string Wrapper_SearchInfoIsNotAvailableInFoundation = "Wrapper_SearchInfoIsNotAvailableInFoundation";
        public const string Wrapper_SearchServiceIsNotAvailable = "Wrapper_SearchServiceIsNotAvailable";
        public const string Wrapper_GetKeywordsFailed = "Wrapper_GetKeywordsFailed";
        public const string Wrapper_RestoreSearchKeywordFailed = "Wrapper_RestoreSearchKeywordFailed";
        public const string Wrapper_RestoreKeywordsFailed = "Wrapper_RestoreKeywordsFailed";
        public const string Wrapper_GetScopesFailed = "Wrapper_GetScopesFailed";
        public const string Wrapper_RestoreScopesFailed = "Wrapper_RestoreScopesFailed";
        public const string Wrapper_RestoreSearchScopeFailed = "Wrapper_RestoreSearchScopeFailed";
        public const string Wrapper_UnsupportedRule = "Wrapper_UnsupportedRule";
        public const string Wrapper_RestoreSearchScopeDisplayGroupFailed = "Wrapper_RestoreSearchScopeDisplayGroupFailed";
        public const string Wrapper_ManagedPropertyNotFound = "Wrapper_ManagedPropertyNotFound";
        public const string Wrapper_RestoreUserProfilePropertyError = "Wrapper_RestoreUserProfilePropertyError";
        public const string Wrapper_RestoreUserProfilePropertiesFailed = "Wrapper_RestoreUserProfilePropertiesFailed";
        public const string Wrapper_IgnoreSiteLockIssueFailed = "Wrapper_IgnoreSiteLockIssueFailed";
        public const string Wrapper_RestoreUserProfileFailed = "Wrapper_RestoreUserProfileFailed";
        public const string Wrapper_UserProfileNotExist = "Wrapper_UserProfileNotExist";
        public const string Wrapper_UserProfilePropertyNotExist = "Wrapper_UserProfilePropertyNotExist";
        public const string Wrapper_RestoreUserProfileDetailError = "Wrapper_RestoreUserProfileDetailError";
        public const string Wrapper_RestoreUserProfileColleagueError = "Wrapper_RestoreUserProfileColleagueError";
        public const string Wrapper_RestoreUserProfileDetailsFailed = "Wrapper_RestoreUserProfileDetailsFailed";
        public const string Wrapper_RestoreColleaguesFailed = "Wrapper_RestoreColleaguesFailed";
        public const string Wrapper_RestoreSocialCommentError = "Wrapper_RestoreSocialCommentError";
        public const string Wrapper_RestoreSocialTagError = "Wrapper_RestoreSocialTagError";
        public const string Wrapper_VerifyRedirectAssemblyFailed = "Wrapper_VerifyRedirectAssemblyFailed";
        public const string Wrapper_SetUserProfileFieldForSocialCommentManagerError = "Wrapper_SetUserProfileFieldForSocialCommentManagerError";
        public const string Wrapper_NoUserProfileHasBeenRestored = "Wrapper_NoUserProfileHasBeenRestored";
        public const string Wrapper_RestoreUserProfileQuickLinkError = "Wrapper_RestoreUserProfileQuickLinkError";
        public const string Wrapper_GenerateSPContextFailed = "Wrapper_GenerateSPContextFailed";
        public const string Wrapper_SetSocialDataManagerUserProfileFieldError = "Wrapper_SetSocialDataManagerUserProfileFieldError";
        public const string Wrapper_MetadataServiceIsNotAvailable = "Wrapper_MetadataServiceIsNotAvailable";
        public const string Wrapper_RestoreMetadataServiceFailed = "Wrapper_RestoreMetadataServiceFailed";
        public const string Wrapper_SkipTermGroupForTheConfigration = "Wrapper_SkipTermGroupForTheConfigration";
        public const string Wrapper_RestoreTermGroupError = "Wrapper_RestoreTermGroupError";
        public const string Wrapper_RestoreTermSetError = "Wrapper_RestoreTermSetError";
        public const string Wrapper_RestoreSocialCommentSkipped = "Wrapper_RestoreSocialCommentSkipped";
        public const string Wrapper_SocialCommentTitle = "Wrapper_SocialCommentTitle";
        public const string Wrapper_RestoreSocialTagSkipped = "Wrapper_RestoreSocialTagSkipped";
        public const string Wrapper_SocialTagTitle = "Wrapper_SocialTagTitle";
        public const string Wrapper_RestoreTermError = "Wrapper_RestoreTermError";
        public const string Wrapper_UsedTermStoreInfoForRestoring = "Wrapper_UsedTermStoreInfoForRestoring";
        public const string Wrapper_SkipRestoreBuiltinUser = "Wrapper_SkipRestoreBuiltinUser";
        public const string Wrapper_MetadataServiceNodataToRestored = "Wrapper_MetadataServiceNodataToRestored";
        public const string Wrapper_MetadataCustomPropertyNotSupport = "Wrapper_MetadataCustomPropertyNotSupport";
        public const string Wrapper_DefaultProxyInfo = "Wrapper_DefaultProxyInfo";
        public const string Wrapper_InitializeServicePointManagerCertificatePolicyFailed = "Wrapper_InitializeServicePointManagerCertificatePolicyFailed";
        public const string Wrapper_DisableUriIriParsingFailed = "Wrapper_DisableUriIriParsingFailed";
        public const string Wrapper_LoginO365WithWindowsAuthenticationFailed = "Wrapper_LoginO365WithWindowsAuthenticationFailed";
        public const string Wrapper_LoadNodeFailed = "Wrapper_LoadNodeFailed";
        public const string Wrapper_CreateInstanceFailed = "Wrapper_CreateInstanceFailed";
        public const string Wrapper_ResolveO365AuthenticationsFailed = "Wrapper_ResolveO365AuthenticationsFailed";
        public const string Wrapper_DeploymentAPIIsNotAvailable = "Wrapper_DeploymentApiIsNotAvailable";
        public const string Wrapper_SPAPIIsNotAvailable = "Wrapper_SPApiIsNotAvailable";
        public const string Wrapper_InstanceNotAvailable = "Wrapper_InstanceNotAvailable";
        public const string Wrapper_SpecialVersionNotFound = "Wrapper_SpecialVersionNotFound";
        public const string Wrapper_WebNotFoundWithException = "Wrapper_WebNotFoundWithException";
        public const string Wrapper_Exception_Restore_RestoreAddDataFailedForCheckAppWebUrl = "Wrapper_Exception_Restore_RestoreAddDataFailedForCheckAppWebUrl";
        public const string Wrapper_CreatedWebFailed = "Wrapper_CreateWebFailed";
        public const string Wrapper_AuthenticationNotAvailable = "Wrapper_AuthenticationNotAvailable";
        public const string Wrapper_CreateWebInfo = "Wrapper_CreateWebInfo";
        public const string Wrapper_WebNotFound = "Wrapper_WebNotFound";
        public const string Wrapper_RestoreEventReceiver = "Wrapper_RestoreEventReceiver";
        public const string Wrapper_RestoreLanguageFile = "Wrapper_RestoreLanguageFile";
        public const string Wrapper_AddRoleDefinition = "Wrapper_AddRoleDefinition";
        public const string Wrapper_RestoreRoleDefinitions = "Wrapper_RestoreRoleDefinitions";
        public const string Wrapper_ColumnMappingXml = "Wrapper_ColumnMappingXml";
        public const string Wrapper_ContentTypeMappingXml = "Wrapper_ContentTypeMappingXml";
        public const string Wrapper_GetUserLoginNameFailedBySid = "Wrapper_GetUserLoginNameFailedBySid";
        public const string Wrapper_RestoreTermOnlyError = "Wrapper_RestoreTermOnlyError";
        public const string Wrapper_GetTermSetByIdFailed = "Wrapper_GetTermSetByIdFailed";
        public const string Wrapepr_RestorePortalFailed = "Wrapepr_RestorePortalFailed";
        public const string Wrapper_RestoreRssFailed = "Wrapper_RestoreRssFailed";
        public const string Wrapper_ActiveFeatureError = "Wrapper_ActiveFeatureError";
        public const string Wrapper_RestoreGroupError = "Wrapper_RestoreGroupError";
        public const string Wraper_GetFeatureTargetError = "Wraper_GetFeatureTargetError";
        public const string Wrapper_NotFindFeatureFromHtml = "Wrapper_NotFindFeatureFromHtml";
        public const string Wrapper_RestoreRoleAssignment_GetUserFailed = "Wrapper_RestoreRoleAssignment_GetUserFailed";
        public const string Wrapper_RestoreRoleAssignment_GetRoleFailed = "Wrapper_RestoreRoleAssignment_GetUserFailed";
        public const string wrapper_RestoreAuditFailed = "wrapper_RestoreAuditFailed";
        public const string Wrapepr_GetUserPropertyFailed = "Wrapepr_GetUserPropertyFailed";
        public const string Wrapper_GetTermByNameFailed = "Wrapper_GetTermByNameFailed";
        public const string Wrapper_GetTermStoreByIdFailed = "Wrapper_GetTermStoreByIdFailed";
        public const string Wrapper_GetTermGroupByIdFailed = "Wrapper_GetTermGroupByIdFailed";
        public static string Wrapper_RestoreSubTypeError = "Wrapper_RestoreSubTypeError";
        public static string Wrapper_SkipRestoreMetadataService = "Wrapper_SkipRestoreMetadataService";
        public static string Wrapper_UpdateUserSetting = "Wrapper_UpdateUserSetting";
        public static string Wrapper_UpdateUserSettingSuccessful = "Wrapper_UpdateUserSettingSuccessful";
        public static string Wrapper_UpdateUserSettingFailed = "Wrapper_UpdateUserSettingFailed";
        public static string Wrapper_FindUserInDestinationSuccessfully = "Wrapper_FindUserInDestinationSuccessfully";
        public static string Wrapper_TermGroupRestored = "Wrapper_TermGroupRestored";
        public static string Wrapper_TermSetRestored = "Wrapper_TermSetRestored";
        public static string Wrapper_TermRestored = "Wrapper_TermRestored";
        public static string Wrapper_NotFoundContentDatabaseById = "Wrapper_NotFoundContentDatabaseById";
        public static string Wrapper_RestoreTermPostActionError = "Wrapper_RestoreTermPostActionError";
        public static string Wrapper_RestoreTermSetPostActionError = "Wrapper_RestoreTermSetPostActionError";
        
    }
}
