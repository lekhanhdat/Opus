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
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.ReportCenter
{
    public class ManagementAPIReportConstants
    {

        public static Dictionary<string, string> I18nEvents
        {
            get
            {
                return new Dictionary<string, string>
                {
                   {SPCheckedInFile, I18nCheckedInFile},
                   {SPCheckOutFile, I18nCheckOutFile},
                   {SPDiscardedFileCheckout, I18nDiscardedFileCheckout},
                   {SPCopiedFile, I18nCopiedFile},
                   {SPDeletedFile, I18nDeletedFile},
                   {SPFileRecycled, I18nDeletedFile },
                   {SPDownloadedFile, I18nDownloadedFile},
                   {SPAccessedFile, I18nAccessedFile},
                   {SPModifiedFile, I18nModifiedFile},
                   {SPMovedFile, I18nMovedFile},
                   {SPRenamedFile, I18nRenamedFile},
                   {SPRestoredFile, I18nRestoredFile},
                   {SPUploadedFile, I18nUploadedFile},
                   {SPVideoRequest ,I18nVideoRequest},
                   {SPPageViewed, I18nPageViewed},
                   {SPFileVersionDeleted,I18nFileVersionDeleted },

                   {SPUnsharedFileFolder, I18nUnsharedFileFolder},
                   {SPSharedFileFolder, I18nSharedFileFolder},
                   {SPCreateSharingInvitation, I18nCreateSharingInvitation},
                   {SPAcceptSharingInvitation, I18nAcceptSharingInvitation},
                   {SPWithdrewSharingInvitation, I18nWithdrewSharingInvitation},
                   {SPCreateAnonymousLink, I18nCreateAnonymousLink},
                   {SPUsedAnonymousLink, I18nUsedAnonymousLink},
                   {SPRemovedAnonymousLink, I18nRemovedAnonymousLink},
                   {SPUpdatedAnonymousLink, I18nUpdatedAnonymousLink},
                   {SPCreateCompanyShareLink, I18nCreateCompanyShareLink},
                   {SPUsedCompanyShareLink, I18nUsedCompanyShareLink},
                   {SPRemovedCompanyShareLink, I18nRemovedCompanyShareLink},
                   {SPAcceptAccessRequest, I18nAcceptAccessRequest},
                   {SPCreateAccessRequest, I18nCreateAccessRequest},
                   {SPDenyAccessRequest, I18nDenyAccessRequest},

                   {AllowComputerSyncFile, I18nAllowComputerSyncFile},
                   {BlockComputerSyncfile, I18nBlockComputerSyncfile},

                   {AllowUserCreateGroup, I18nAllowUserCreateGroup},
                   {ChangeExemptUserAgent, I18nChangeExemptUserAgent},
                   {AddExemptUserAgent, I18nAddExemptUserAgenty},
                   {SPCreatedGrouop, I18nCreatedGrouop},
                   {SPDeletedGroup, I18nDeletedGroup},
                   {SPUpdatedGroup, I18nUpdatedGroup},
                   {SetHostSite, I18nSetHostSite},
                   {EnableLegacyWorkflow, I18nEnableLegacyWorkflow},
                   {EnableRSSFeeds, I18nEnableRSSFeeds},
                   {EnableOfficeDemand, I18nEnableOfficeDemand},
                   {EnableDocumentPreview, I18nEnableDocumentPreview},
                   {EnableResultPeopleSear, I18nEnableResultPeopleSear},
                   {CreateSentConnection, I18nCreateSentConnection},
                   {DeleteSentConnection, I18nDeleteSentConnection},
                   {SPRequestSiteAdminPermission, I18nRequestSiteAdminPermission},
                   {SPAddSiteCollectionAdmin, I18nAddSiteCollectionAdmin},
                   {SPCreateSiteCollection, I18nCreateSiteCollection},
                   {ModifySitePermission, I18nModifySitePermission},
                   {RenamedSite, I18nRenamedSite},
                   {SPAddUserGroupToSP, I18nAddUserGroupToSP},
                   {SPChangeSharingPolicy, I18nChangeSharingPolicy},
                   {SPRemoveUserGroupFromSP, I18nRemoveUserGroupFromSP},

                   {CreateReceiveMsg, I18nCreateReceiveMsg},
                   {CopyMsgAnotherFolder, I18nCopyMsgAnotherFolder},
                   {UserSignMailbox, I18nUserSignMailbox},
                   {SentMsgOnBehalfPermission, I18nSentMsgOnBehalfPermission},
                   {PurgeMsgFromMailbox, I18nPurgeMsgFromMailbox},
                   {MoveMsgDeleteFolder, I18nMoveMsgDeleteFolder},
                   {MoveMsgAnotherFolder, I18nMoveMsgAnotherFolder},
                   {SentMsgSendAsPermission, I18nSentMsgSendAsPermission},
                   {UpdatedMsg, I18nUpdatedMsg},
                   {DeleteMsgFromDeleteFolder, I18nDeleteMsgFromDeleteFolder},

                    {SPComplianceSettingChanged, I18nComplianceSettingChanged },
                    {SPLockRecord, I18nLockRecord },
                    {SPUnlockRecord, I18nUnlockRecord },
                    {SPComplianceRecordDelete, I18nComplianceRecordDelete },
                    {SPDocumentSensitivityMismatchDetected, I18nDocumentSensitivityMismatchDetected },
                    {SPFileMalwareDetected, I18nFileMalwareDetected },
                    {SPFileModifiedExtended,  I18nFileModifiedExtended },
                    {SPSearchQueryPerformed,  I18nSearchQueryPerformed },
                    {SPFileVersionsAllMinorsRecycled,  I18nFileVersionsAllMinorsRecycled },
                    {SPFileVersionsAllRecycled,  I18nFileVersionsAllRecycled },
                    {SPFileVersionRecycled, I18nFileVersionRecycled },
                    {SPPageViewedExtended,  I18nPageViewedExtended },
                    {SPClientViewSignaled,  I18nClientViewSignaled },
                    {SPPagePrefetched,  I18nPagePrefetched },
                    {SPFileAccessedExtended,  I18nFileAccessedExtended },
                    {SPFolderCopied, I18nFolderCopied },
                    {SPFolderCreated, I18nFolderCreated },
                    {SPFolderDeletedFirstStageRecycleBin, I18nFolderDeletedFirstStageRecycleBin },
                    {SPFolderDeletedSecondStageRecycleBin,  I18nFolderDeletedSecondStageRecycleBin },
                    {SPFolderMoved,  I18nFolderMoved },
                    {SPFolderRenamed, I18nFolderRenamed },
                    {SPFolderRestored,  I18nFolderRestored },
                    {SPListCreated,  I18nListCreated },
                    {SPListColumnCreated, I18nListColumnCreated },
                    {SPListContentTypeCreated, I18nListContentTypeCreated },
                    {SPListItemCreated, I18nListItemCreated },
                    {SPSiteColumnCreated, I18nSiteColumnCreated },
                    {SPSiteContentTypeCreated, I18nSiteContentTypeCreated },
                    {SPListDeleted,  I18nListDeleted },
                    {SPListColumnDeleted, I18nListColumnDeleted },
                    {SPListContentTypeDeleted, I18nListContentTypeDeleted },
                    {SPListItemDeleted, I18nListItemDeleted },
                    {SPSiteColumnDeleted, I18nSiteColumnDeleted },
                    {SPSiteContentTypeDeleted, I18nSiteContentTypeDeleted },
                    {SPListItemRecycled,  I18nListItemRecycled },
                    {SPListRestored, I18nListRestored },
                    {SPListItemRestored,  I18nListItemRestored },
                    {SPListUpdated,  I18nListUpdated },
                    {SPListColumnUpdated, I18nListColumnUpdated },
                    {SPListContentTypeUpdated, I18nListContentTypeUpdated },
                    {SPListItemUpdated, I18nListItemUpdated },
                    {SPSiteColumnUpdated, I18nSiteColumnUpdated },
                    {SPSiteContentTypeUpdated, I18nSiteContentTypeUpdated },
                    {SPListViewCreated, I18nListViewCreated },
                    {SPListViewUpdated, I18nListViewUpdated },
                    {SPListViewed, I18nListViewed },
                    {SPPermissionLevelAdded,  I18nPermissionLevelAdded },
                    {SPAccessRequestAccepted, I18nAccessRequestAccepted },
                    {SPSharingInvitationBlocked, I18nSharingInvitationBlocked },
                    {SPSecureLinkCreated, I18nSecureLinkCreated },
                    {SPSecureLinkDeleted, I18nSecureLinkDeleted },
                    {SPAccessRequestDenied, I18nAccessRequestDenied },
                    {SPAccessRequestUpdated,  I18nAccessRequestUpdated },
                    {SPSharingInvitationUpdated, I18nSharingInvitationUpdated },
                    {SPSecureLinkUsed,  I18nSecureLinkUsed },
                    {SPAddedToSecureLink, I18nAddedToSecureLink },
                    {SPRemovedFromSecureLink, I18nRemovedFromSecureLink },
                    {SPWebAccessRequestApproverModified, I18nWebAccessRequestApproverModified },
                    {SPManagedSyncClientAllowed, I18nManagedSyncClientAllowed },
                    {SPUnmanagedSyncClientBlocked, I18nUnmanagedSyncClientBlocked },
                    {SPFileSyncDownloadedFull, I18nFileSyncDownloadedFull },
                    {SPFileSyncDownloadedPartial,  I18nFileSyncDownloadedPartial },
                    {SPFileSyncUploadedFull,  I18nFileSyncUploadedFull },
                    {SPFileSyncUploadedPartial,  I18nFileSyncUploadedPartial },
                    {SPPermissionLevelsInheritanceBroken, I18nPermissionLevelsInheritanceBroken },
                    {SPSharingInheritanceBroken, I18nSharingInheritanceBroken },
                    {SPWebRequestAccessModified, I18nWebRequestAccessModified },
                    {SPWebMembersCanShareModified, I18nWebMembersCanShareModified },
                    {SPPermissionLevelModified,  I18nPermissionLevelModified },
                    {SPPermissionLevelRemoved, I18nPermissionLevelRemoved },
                    {SPSharingInheritanceReset,  I18nSharingInheritanceReset },
                    {SPAllowedDataLocationAdded, I18nAllowedDataLocationAdded },
                    {SPExemptUserAgentSet,  I18nExemptUserAgentSet },
                    {SPGeoAdminAdded, I18nGeoAdminAdded },
                    {SPAllowGroupCreationSet, I18nAllowGroupCreationSet },
                    {SPSiteGeoMoveCancelled,  I18nSiteGeoMoveCancelled },
                    {SPDeviceAccessPolicyChanged,  I18nDeviceAccessPolicyChanged },
                    {SPCustomizeExemptUsers,  I18nCustomizeExemptUsers },
                    {SPNetworkAccessPolicyChanged, I18nNetworkAccessPolicyChanged },
                    {SPSiteGeoMoveCompleted,  I18nSiteGeoMoveCompleted },
                    {SPHubSiteOrphanHubDeleted,  I18nHubSiteOrphanHubDeleted },
                    {SPSiteDeleted,  I18nSiteDeleted },
                    {SPLegacyWorkflowEnabledSet, I18nLegacyWorkflowEnabledSet },
                    {SPOfficeOnDemandSet, I18nOfficeOnDemandSet },
                    {SPPeopleResultsScopeSet, I18nPeopleResultsScopeSet },
                    {SPNewsFeedEnabledSet,  I18nNewsFeedEnabledSet },
                    {SPHubSiteJoined, I18nHubSiteJoined },
                    {SPHubSiteRegistered, I18nHubSiteRegistered },
                    {SPAllowedDataLocationDeleted, I18nAllowedDataLocationDeleted },
                    {SPGeoAdminDeleted, I18nGeoAdminDeleted },
                    {SPSiteRenamed,  I18nSiteRenamed },
                    {SPSiteGeoMoveScheduled,  I18nSiteGeoMoveScheduled },
                    {SPHostSiteSet,  I18nHostSiteSet },
                    {SPGeoQuotaAllocated, I18nGeoQuotaAllocated },
                    {SPHubSiteUnjoined, I18nHubSiteUnjoined },
                    {SPHubSiteUnregistered, I18nHubSiteUnregistered },  
										 

                   //{CreatedSway, I18nCreatedSway},
                   //{ViewedSway, I18nViewedSway},
                   //{SharedSway, I18nSharedSway},
                   //{DeletedSway, I18nDeletedSway},
                   //{DisableSwayDuplication, I18nDisableSwayDuplication},
                   //{DuplicatedSway, I18nDuplicatedSway},
                   //{EditedSway, I18nEditedSway},
                   //{EnableSwayDuplication, I18nEnableSwayDuplication},
                   //{TurnOffExternalSway, I18nTurnOffExternalSway},
                   //{TurnOnExternalSway, I18nTurnOnExternalSway},
                   //{RevokedSwaySharing, I18nRevokedSwaySharing},
                   //{TurnOffSwayService, I18nTurnOffSwayService},
                   //{TurnOnSwayService, I18nTurnOnSwayService},
                   //{ChangeSwayShareLevel, I18nChangeSwayShareLevel},

                   {ADAddedUser, I18nAddedUser},
                   {ADDeletedUser, I18nDeletedUser},
                   {SetLicensProperties, I18nSetLicensProperties},
                   {ADResetUserPassword, I18nResetUserPassword},
                   {ADChangedUserPassword, I18nChangedUserPassword},
                   {ADChangedUserLicense, I18nChangedUserLicense},
                   {ADUpdatedUser, I18nUpdatedUser},
                   {SetPropertyChangePassword, I18nSetPropertyChangePassword},

                   {ADAddedGroup, I18nAddedGroup},
                   {ADUpdateGroup, I18nUpdateGroup},
                   {ADDeleteGroup, I18nDeleteGroup},
                   {ADAddedMemberGroup, I18nAddedMemberGroup},
                   {ADRemovedMemberGroup, I18nRemovedMemberGroup},

                   {ADAddedServicePrincipal, I18nAddedServicePrincipal},
                   {ADRemoveServicePrincipal, I18nRemoveServicePrincipal},
                   {SetDelegationEntry, I18nSetDelegationEntry},
                   {RemoveCredential, I18nRemoveCredential},
                   {AddedDelegationEntry, I18nAddedDelegationEntry},
                   {AddedCredential, I18nAddedCredential},
                   {RemoveDelegationEntry, I18nRemoveDelegationEntry},

                   {ADAddedMemberRole, I18nAddedMemberRole},
                   {ADRemoveUserFromRole, I18nRemoveUserFromRole},
                   {SetCompanyContact, I18nSetCompanyContact},

                   {AddedPartner, I18nAddedPartner},
                   {RemovedPartner, I18nRemovedPartner},
                   {AddedDomain, I18nAddedDomain},
                   {RemovedDomain, I18nRemovedDomain},
                   {UpdatedDomain, I18nUpdatedDomain},
                   {SetDomainAuthentication, I18nSetDomainAuthentication},
                   {ADVerifiedDomain, I18nVerifiedDomain},
                   {UpdatedFederation, I18nUpdatedFederation},
                   {VerifiedEmailDomain, I18nVerifiedEmailDomain},
                   {TurnOnAzureADSync, I18nTurnOnAzureADSync},
                   {ADSetPasswordPolicy, I18nSetPasswordPolicy},
                   {ADSetCompanyInfo, I18nSetCompanyInfo},

                   //{CreatedContentSerach, I18nCreatedContentSerach },
                   //{DeletedContentSearch, I18nDeletedContentSearch },
                   //{ChangedContentSerach, I18nChangedContentSerach },
                   //{StartedContentSearch, I18nStartedContentSearch },
                   //{StoppedContentSearch, I18nStoppedContentSearch },
                   //{CreatedContentSearchAction, I18nCreatedContentSearchAction },
                   //{ChangedContentSearchAction, I18nChangedContentSearchAction },
                   //{DeletedContentSearchAction, I18nDeletedContentSearchAction },
                   //{CreatedSearchPermissionsFilter, I18nCreatedSearchPermissionsFilter },
                   //{DeletedSearchPermissionsFilter, I18nDeletedSearchPermissionsFilter },
                   //{ChangedSearchPermissionsFilter, I18nChangedSearchPermissionsFilter },
                   //{CreatedHoldIneDiscoveryCase, I18nCreatedHoldIneDiscoveryCase },
                   //{DeletedHoldIneDiscoveryCase, I18nDeletedHoldIneDiscoveryCase },
                   //{ChangedHoldIneDiscoveryCase, I18nChangedHoldIneDiscoveryCase },
                   //{CreatedSearchQueryeDiscovery, I18nCreatedSearchQueryeDiscovery },
                   //{DeletedSearchQueryeDiscovery, I18nDeletedSearchQueryeDiscovery },
                   //{ChangedSearchQueryeDiscovery, I18nChangedSearchQueryeDiscovery },
                   //{CreatedeDiscoveryCase, I18nCreatedeDiscoveryCase },
                   //{DeletedeDiscoveryCase, I18nDeletedeDiscoveryCase },
                   //{ChangedeDiscoveryCase, I18nChangedeDiscoveryCase },
                   //{AddedMemberToeDiscoveryCase,  I18nAddedMemberToeDiscoveryCase},
                   //{RemoveMemberFromeDiscovery,  I18nRemoveMemberFromeDiscovery},
                   //{ChangedeDiscoveryCaseMembership, I18nChangedeDiscoveryCaseMembership },
                   //{CreatedeDiscoveryAdministrator, I18nCreatedeDiscoveryAdministrator },
                   //{DeletedeDiscoveryAdministrator, I18nDeletedeDiscoveryAdministrator },
                   //{ChangedeDiscoveryAdministrator, I18nChangedeDiscoveryAdministrator },

                   //{ViewPowerBIDashboard, I18nViewPowerBIDashboard },
                   //{CreatedPowerBIDashboard, I18nCreatedPowerBIDashboard },
                   //{EditedPowerBIDashboard, I18nEditedPowerBIDashboard },
                   //{DeletedPowerBIDashboard, I18nDeletedPowerBIDashboard },
                   //{SharedPowerBIDashboard, I18nSharedPowerBIDashboard },
                   //{DeletedPowerBIReport, I18nDeletedPowerBIReport },
                   //{DeletedPowerBIDatasets, I18nDeletedPowerBIDatasets },
                   //{CreatedPowerBIGroup, I18nCreatedPowerBIGroup },
                   //{AddedPowerBIGroupMember, I18nAddedPowerBIGroupMember },
                   //{CreatedOrgnizationPowerBI, I18nCreatedOrgnizationPowerBI }

                    {PasswordLogonInitialAuthUsingPassword ,I18nPasswordLogonInitialAuthUsingPassword},
                    {UserLoggedIn, I18nUserLoggedIn },
                    {PasswordLogonCookieCopyUsingDAToken, I18nPasswordLogonCookieCopyUsingDAToken },
                    {SiteCollectionAdminRemoved, I18nSiteCollectionAdminRemoved },
                    {FilePreviewed, I18nFilePreviewed },
                    {ConsentToApplication, I18nConsentToApplication },
                    {AddOAuth2PermissionGrant, I18nAddOAuth2PermissionGrant },
                    {AddAppRoleAssignmentGrantToUser, I18nAddAppRoleAssignmentGrantToUser },
                    {RemovedFromSiteCollection, I18nRemovedFromSiteCollection },
                    {CreateCompany, I18nCreateCompany },
                    {EnableAddressListPaging, I18nEnableAddressListPaging },
                    {SetTransportConfig, I18nSetTransportConfig },
                    {SetMailbox,I18nSetMailbox },
                    {SetOwaMailboxPolicy,I18nSetOwaMailboxPolicy },
                    {SetTenantObjectVersion,I18nSetTenantObjectVersion },
                    {DeleteApplicationPassword,I18nDeleteApplicationPassword },
                    {CreateApplicationPassword,I18nCreateApplicationPassword },
                    {FolderDeleted,I18nFolderDeleted },
                    {SPFolderRecycled, I18nFolderDeleted },
                    {FolderModified,I18nFolderModified },
                    {NewExchangeAssistanceConfig,I18nNewExchangeAssistanceConfig },
                    {InstallDefaultSharingPolicy,I18nInstallDefaultSharingPolicy },
                    {InstallAdminAuditLogConfig,I18nInstallAdminAuditLogConfig },
                    {InstallDataClassificationConfig,I18nInstallDataClassificationConfig },
                    {InstallResourceConfig, I18nInstallResourceConfig},
                    {SetRecipientEnforcementProvisioningPolicy, I18nSetRecipientEnforcementProvisioningPolicy },
                    {SetExchangeAssistanceConfig, I18nSetExchangeAssistanceConfig },
                    {NewDkimSigningConfig, I18nNewDkimSigningConfig },
                    {SetAdminAuditLogConfig, I18nSetAdminAuditLogConfig },
                    {NewMailbox, I18nNewMailbox },
                    {FileDeletedFirstStageRecycleBin, I18nFileDeletedFirstStageRecycleBin },
                    {FileDeletedSecondStageRecycleBin, I18nFileDeletedSecondStageRecycleBin },

                    
                    //teams
                    { BotAddedToTeam, I18nBotAddedToTeam },
                    { BotRemovedFromTeam, I18nBotRemovedFromTeam },
                    { ChannelAddedForTeam, I18nChannelAddedForTeam },
                    { ChannelDeletedForTeam, I18nChannelDeletedForTeam },
                    { ChannelSettingChanged, I18nChannelSettingChanged },
                    { ConnectorAddedForTeam, I18nConnectorAddedForTeam },
                    { ConnectorRemovedForTeam, I18nConnectorRemovedForTeam },
                    { ConnectorUpdated, I18nConnectorUpdated },
                    { MemberAddedForTeam, I18nMemberAddedForTeam },
                    { MemberRemovedForTeam, I18nMemberRemovedForTeam},
                    { MemberRoleChanged, I18nMemberRoleChanged},
                    { TabAdded, I18nTabAdded},
                    { TabRemoved, I18nTabRemoved},
                    { TabUpdated, I18nTabUpdated},
                    { TeamCreatedForTeam, I18nTeamCreatedForTeam},
                    { TeamDeletedForTeam, I18nTeamDeletedForTeam},
                    { TeamSettingChangedForTeam, I18nTeamSettingChangedForTeam},
                    { TeamsSessionStartedForTeam, I18nTeamsSessionStartedForTeam},
                    { TeamsTenantSettingChanged, I18nTeamsTenantSettingChanged},

                    //forms                            
                    { CreateComment, I18nCreateComment },
                    { CreateForm, I18nCreateForm},
                    { EditForm, I18nEditForm},
                    { MoveForm, I18nMoveForm},
                    { DeleteForm, I18nDeleteForm},
                    { ViewForm, I18nViewForm},
                    { PreviewForm, I18nPreviewForm},
                    { ExportForm, I18nExportForm},
                    { AllowShareFormForCopy, I18nAllowShareFormForCopy},
                    { DisallowShareFormForCopy, I18nDisallowShareFormForCopy},
                    { AddFormCoauthor, I18nAddFormCoauthor},
                    { RemoveFormCoauthor, I18nRemoveFormCoauthor},
                    { ViewRuntimeForm, I18nViewRuntimeForm},
                    { CreateResponse, I18nCreateResponse},
                    { UpdateResponse, I18nUpdateResponse},
                    { DeleteAllResponses, I18nDeleteAllResponses},
                    { DeleteResponse, I18nDeleteResponse},
                    { ViewResponses, I18nViewResponses},
                    { ViewResponse, I18nViewResponse},
                    { GetSummaryLink, I18nGetSummaryLink},
                    { DeleteSummaryLink, I18nDeleteSummaryLink},
                    { UpdatePhishingStatus, I18nUpdatePhishingStatus},
                    { ProInvitation, I18nProInvitation},
                    { UpdateFormSetting, I18nUpdateFormSetting},
                    { UpdateUserSetting, I18nUpdateUserSetting},
                    { ListForms, I18nListForms},
                    { SubmitResponse, I18nSubmitResponse},

                    //stream
                    { StreamInvokeVideoView, I18nStreamInvokeVideoView},
                    { StreamEditVideoPermissions, I18nStreamEditVideoPermissions},
                    { StreamInvokeVideoUpload, I18nStreamInvokeVideoUpload},
                    { StreamEditVideo, I18nStreamEditVideo},
                    { StreamInvokeVideoSetLink, I18nStreamInvokeVideoSetLink},
                    { StreamCreateVideo, I18nStreamCreateVideo},
                    { StreamInvokeVideoDownload, I18nStreamInvokeVideoDownload},
                    { StreamInvokeVideoShare, I18nStreamInvokeVideoShare},
                    { StreamCreateChannel, I18nStreamCreateChannel},
                    { StreamEditChannel, I18nStreamEditChannel},
                    { StreamDeleteVideo, I18nStreamDeleteVideo},
                    { StreamInvokeVideoLike, I18nStreamInvokeVideoLike},
                    { StreamCreateGroup, I18nStreamCreateGroup},
                    { StreamDeleteVideoComment, I18nStreamDeleteVideoComment},
                    { StreamEditGroup, I18nStreamEditGroup},
                    { StreamCreateVideoComment, I18nStreamCreateVideoComment},
                    { StreamInvokeVideoUnLike, I18nStreamInvokeVideoUnLike},
                    { StreamInvokeChannelSetThumbnail, I18nStreamInvokeChannelSetThumbnail},
                    { StreamEditUserSettings, I18nStreamEditUserSettings},

                    //mailbox
                    { MailItemsAccessed, I18nMailItemsAccessed},
                    { AddMailboxPermission, I18nAddMailboxPermissions},
                    { UpdateCalendarDelegation, I18nUpdateCalendarDelegation},
                    { AddFolderPermissions, I18nAddFolderPermissions},
                    { MailboxCopy, I18nMailboxCopy},
                    { MailboxCreate, I18nMailboxCreate},
                    { NewInboxRule, I18nNewInboxRule},
                    { SoftDelete, I18nSoftDelete},
                    { ApplyRecordLabel, I18nApplyRecordLabel},
                    { MailboxMove, I18nMailboxMove},
                    { MoveToDeletedItems, I18nMoveToDeletedItems},
                    { UpdateFolderPermissions, I18nUpdateFolderPermissions},
                    { SetInboxRule, I18nSetInboxRule},
                    { HardDelete, I18nHardDelete},
                    { RemoveMailboxPermission, I18nRemoveMailboxPermission},
                    { RemoveFolderPermissions, I18nRemoveFolderPermissions},
                    { MailboxSend, I18nMailboxSend},
                    { MailboxSendAs, I18nMailboxSendAs},
                    { SendOnBehalf, I18nSendOnBehalf},
                    { UpdateInboxRules, I18nUpdateInboxRules},
                    { MailboxUpdate, I18nMailboxUpdate},
                    { MailboxLogin, I18nMailboxLogin},
                    { ModifyFolderPermissions, I18nModifyFolderPermissions}

                };
            }
        }

        #region Event Actions

        #region File and Page Activities
        public static readonly string SPCheckedInFile = "FileCheckedIn";
        public static readonly string SPCheckOutFile = "FileCheckedOut";
        public static readonly string SPDiscardedFileCheckout = "FileCheckOutDiscarded";
        public static readonly string SPCopiedFile = "FileCopied";
        public static readonly string SPDeletedFile = "FileDeleted";
        public static readonly string SPDownloadedFile = "FileDownloaded";
        public static readonly string SPAccessedFile = "FileAccessed";
        public static readonly string SPModifiedFile = "FileModified";
        public static readonly string SPMovedFile = "FileMoved";
        public static readonly string SPRenamedFile = "FileRenamed";
        public static readonly string SPRestoredFile = "FileRestored";
        public static readonly string SPUploadedFile = "FileUploaded";
        public static readonly string SPVideoRequest = "VideoRequested";
        public static readonly string SPPageViewed = "PageViewed";
        public static readonly string SPFileVersionDeleted = "FileVersionDeleted";
        //new operation for Delete File
        public static readonly string SPFileRecycled = "FileRecycled";

        // not support filter
        public static readonly string SPComplianceSettingChanged = "ComplianceSettingChanged";
        public static readonly string SPLockRecord = "LockRecord";
        public static readonly string SPUnlockRecord = "UnlockRecord";
        public static readonly string SPComplianceRecordDelete = "ComplianceRecordDelete";
        public static readonly string SPDocumentSensitivityMismatchDetected = "DocumentSensitivityMismatchDetected";
        public static readonly string SPFileMalwareDetected = "FileMalwareDetected";
        public static readonly string SPFileModifiedExtended = "FileModifiedExtended";
        public static readonly string SPSearchQueryPerformed = "SearchQueryPerformed";
        public static readonly string SPFileVersionsAllMinorsRecycled = "FileVersionsAllMinorsRecycled";
        public static readonly string SPFileVersionsAllRecycled = "FileVersionsAllRecycled";
        public static readonly string SPFileVersionRecycled = "FileVersionRecycled";
        public static readonly string SPPageViewedExtended = "PageViewedExtended";
        public static readonly string SPClientViewSignaled = "ClientViewSignaled";
        public static readonly string SPPagePrefetched = "PagePrefetched";
        public static readonly string SPFileAccessedExtended = "FileAccessedExtended";

        #endregion

        #region folder activities (not support filter)
        public static readonly string SPFolderCopied = "FolderCopied";
        public static readonly string SPFolderCreated = "FolderCreated";
        public static readonly string SPFolderDeletedFirstStageRecycleBin = "FolderDeletedFirstStageRecycleBin";
        public static readonly string SPFolderDeletedSecondStageRecycleBin = "FolderDeletedSecondStageRecycleBin";
        public static readonly string SPFolderMoved = "FolderMoved";
        public static readonly string SPFolderRenamed = "FolderRenamed";
        public static readonly string SPFolderRestored = "FolderRestored";
        //new operation for delete folder
        public static readonly string SPFolderRecycled = "FolderRecycled";

        #endregion

        #region SharePoint list activities(not support filter)
        public static readonly string SPListCreated = "ListCreated";
        public static readonly string SPListColumnCreated = "ListColumnCreated";
        public static readonly string SPListContentTypeCreated = "ListContentTypeCreated";
        public static readonly string SPListItemCreated = "ListItemCreated";
        public static readonly string SPSiteColumnCreated = "SiteColumnCreated";
        public static readonly string SPSiteContentTypeCreated = "SiteContentTypeCreated";
        public static readonly string SPListDeleted = "ListDeleted";
        public static readonly string SPListColumnDeleted = "List Column Deleted";
        public static readonly string SPListContentTypeDeleted = "ListContentTypeDeleted";
        public static readonly string SPListItemDeleted = "ListItemDeleted";
        public static readonly string SPSiteColumnDeleted = "SiteColumnDeleted";
        public static readonly string SPSiteContentTypeDeleted = "SiteContentTypeDeleted";
        public static readonly string SPListItemRecycled = "ListItemRecycled";
        public static readonly string SPListRestored = "ListRestored";
        public static readonly string SPListItemRestored = "ListItemRestored";
        public static readonly string SPListUpdated = "ListUpdated";
        public static readonly string SPListColumnUpdated = "ListColumnUpdated";
        public static readonly string SPListContentTypeUpdated = "ListContentTypeUpdated";
        public static readonly string SPListItemUpdated = "ListItemUpdated";
        public static readonly string SPSiteColumnUpdated = "SiteColumnUpdated";
        public static readonly string SPSiteContentTypeUpdated = "SiteContentTypeUpdated";
        public static readonly string SPListViewCreated = "ListViewCreated";
        public static readonly string SPListViewUpdated = "ListViewUpdated";
        public static readonly string SPListViewed = "ListViewed";
        #endregion

        #region Sharing and access request activities
        //not international
        public static readonly string SPPermissionLevelAdded = "PermissionLevelAdded";
        public static readonly string SPAccessRequestAccepted = "AccessRequestAccepted";
        public static readonly string SPSharingInvitationAccepted = "SharingInvitationAccepted";
        public static readonly string SPSharingInvitationBlocked = "SharingInvitationBlocked";
        public static readonly string SPSecureLinkCreated = "SecureLinkCreated";
        public static readonly string SPSecureLinkDeleted = "SecureLinkDeleted";
        public static readonly string SPAccessRequestDenied = "AccessRequestDenied";
        public static readonly string SPAccessRequestUpdated = "AccessRequestUpdated";
        public static readonly string SPSharingInvitationUpdated = "SharingInvitationUpdated";
        public static readonly string SPSecureLinkUsed = "SecureLinkUsed";
        public static readonly string SPAddedToSecureLink = "AddedToSecureLink";
        public static readonly string SPRemovedFromSecureLink = "RemovedFromSecureLink";
        public static readonly string SPWebAccessRequestApproverModified = "WebAccessRequestApproverModified";


        public static readonly string SPUnsharedFileFolder = "SharingRevoked";
        public static readonly string SPSharedFileFolder = "SharingSet";
        public static readonly string SPCreateSharingInvitation = "SharingInvitationCreated";
        public static readonly string SPAcceptSharingInvitation = "SharingInvitationAccepted";
        public static readonly string SPWithdrewSharingInvitation = "SharingInvitationRevoked";
        public static readonly string SPCreateAnonymousLink = "AnonymousLinkCreated";
        public static readonly string SPUsedAnonymousLink = "AnonymousLinkUsed";
        public static readonly string SPRemovedAnonymousLink = "AnonymousLinkRemoved";
        public static readonly string SPUpdatedAnonymousLink = "AnonymousLinkUpdated";
        public static readonly string SPCreateCompanyShareLink = "CompanyLinkCreated";
        public static readonly string SPUsedCompanyShareLink = "CompanyLinkUsed";
        public static readonly string SPRemovedCompanyShareLink = "CompanyLinkRemoved";
        public static readonly string SPAcceptAccessRequest = "AccessRequestApproved";
        public static readonly string SPCreateAccessRequest = "AccessRequestCreated";
        public static readonly string SPDenyAccessRequest = "AccessRequestRejected";

        #endregion

        #region Synchronization activities
        public static readonly string SPManagedSyncClientAllowed = "ManagedSyncClientAllowed";
        public static readonly string SPUnmanagedSyncClientBlocked = "UnmanagedSyncClientBlocked";
        public static readonly string SPFileSyncDownloadedFull = "FileSyncDownloadedFull";
        public static readonly string SPFileSyncDownloadedPartial = "FileSyncDownloadedPartial";
        public static readonly string SPFileSyncUploadedFull = "FileSyncUploadedFull";
        public static readonly string SPFileSyncUploadedPartial = "FileSyncUploadedPartial";
        #endregion

        #region Site permissions activities
        public static readonly string SPPermissionLevelsInheritanceBroken = "PermissionLevelsInheritanceBroken";
        public static readonly string SPSharingInheritanceBroken = "SharingInheritanceBroken";
        public static readonly string SPWebRequestAccessModified = "WebRequestAccessModified";
        public static readonly string SPWebMembersCanShareModified = "WebMembersCanShareModified";
        public static readonly string SPPermissionLevelModified = "PermissionLevelModified";
        public static readonly string SPPermissionLevelRemoved = "PermissionLevelRemoved";
        public static readonly string SPSharingInheritanceReset = "SharingInheritanceReset";
        #endregion

        #region Site administration activities
        public static readonly string SPAllowedDataLocationAdded = "AllowedDataLocationAdded";
        public static readonly string SPExemptUserAgentSet = "ExemptUserAgentSet";
        public static readonly string SPGeoAdminAdded = "GeoAdminAdded";
        public static readonly string SPAllowGroupCreationSet = "AllowGroupCreationSet";
        public static readonly string SPSiteGeoMoveCancelled = "SiteGeoMoveCancelled";
        public static readonly string SPSharingPolicyChanged = "SharingPolicyChanged";
        public static readonly string SPDeviceAccessPolicyChanged = "DeviceAccessPolicyChanged";
        public static readonly string SPCustomizeExemptUsers = "CustomizeExemptUsers";
        public static readonly string SPNetworkAccessPolicyChanged = "NetworkAccessPolicyChanged";
        public static readonly string SPSiteGeoMoveCompleted = "SiteGeoMoveCompleted";
        public static readonly string SPHubSiteOrphanHubDeleted = "HubSiteOrphanHubDeleted";
        public static readonly string SPSiteDeleted = "SiteDeleted";
        public static readonly string SPLegacyWorkflowEnabledSet = "LegacyWorkflowEnabledSet";
        public static readonly string SPOfficeOnDemandSet = "OfficeOnDemandSet";
        public static readonly string SPPeopleResultsScopeSet = "PeopleResultsScopeSet";
        public static readonly string SPNewsFeedEnabledSet = "NewsFeedEnabledSet";
        public static readonly string SPHubSiteJoined = "HubSiteJoined";
        public static readonly string SPHubSiteRegistered = "HubSiteRegistered";
        public static readonly string SPAllowedDataLocationDeleted = "AllowedDataLocationDeleted";
        public static readonly string SPGeoAdminDeleted = "GeoAdminDeleted";
        public static readonly string SPSiteRenamed = "SiteRenamed";
        public static readonly string SPSiteGeoMoveScheduled = "SiteGeoMoveScheduled";
        public static readonly string SPHostSiteSet = "HostSiteSet";
        public static readonly string SPGeoQuotaAllocated = "GeoQuotaAllocated";
        public static readonly string SPHubSiteUnjoined = "HubSiteUnjoined";
        public static readonly string SPHubSiteUnregistered = "HubSiteUnregistered";
        #endregion

        #region SharePoint administration activies
        //synchronization activities sp
        public static readonly string AllowComputerSyncFile = "AllowComputerSyncFile";
        public static readonly string BlockComputerSyncfile = "BlockComputerSyncfile";
        //Site administration activities  sp
        public static readonly string AllowUserCreateGroup = "AllowUserCreateGroup";
        public static readonly string ChangeExemptUserAgent = "ChangeExemptUserAgent";
        public static readonly string AddExemptUserAgent = "AddExemptUserAgent";
        public static readonly string SPCreatedGrouop = "GroupAdded";
        public static readonly string SPDeletedGroup = "GroupRemoved";
        public static readonly string SPUpdatedGroup = "GroupUpdated";
        public static readonly string SetHostSite = "SetHostSite";
        public static readonly string EnableLegacyWorkflow = "EnableLegacyWorkflow";
        public static readonly string EnableRSSFeeds = "EnableRSSFeeds";
        public static readonly string EnableOfficeDemand = "EnableOfficeDemand";
        public static readonly string EnableDocumentPreview = "PreviewModeEnabledSet";
        public static readonly string EnableResultPeopleSear = "EnableResultPeopleSear";
        public static readonly string CreateSentConnection = "SendToConnectionAdded";
        public static readonly string DeleteSentConnection = "SendToConnectionRemoved";
        public static readonly string SPRequestSiteAdminPermission = "SiteAdminChangeRequest";
        public static readonly string SPAddSiteCollectionAdmin = "SiteCollectionAdminAdded";
        public static readonly string SPCreateSiteCollection = "SiteCollectionCreated";
        public static readonly string ModifySitePermission = "SitePermissionsModified";
        public static readonly string RenamedSite = "RenamedSite";
        public static readonly string SPAddUserGroupToSP = "AddedToGroup";
        public static readonly string SPChangeSharingPolicy = "SharingPolicyChanged";
        public static readonly string SPRemoveUserGroupFromSP = "RemovedFromGroup";
        //Exchange mailbox activities ex
        public static readonly string CreateReceiveMsg = "CreateReceiveMsg";
        public static readonly string CopyMsgAnotherFolder = "CopyMsgAnotherFolder";
        public static readonly string UserSignMailbox = "UserSignMailbox";
        public static readonly string SentMsgOnBehalfPermission = "SentMsgOnBehalfPermission";
        public static readonly string PurgeMsgFromMailbox = "PurgeMsgFromMailbox";
        public static readonly string MoveMsgDeleteFolder = "MoveMsgDeleteFolder";
        public static readonly string MoveMsgAnotherFolder = "MoveMsgAnotherFolder";
        public static readonly string SentMsgSendAsPermission = "SentMsgSendAsPermission";
        public static readonly string UpdatedMsg = "UpdatedMsg";
        public static readonly string DeleteMsgFromDeleteFolder = "DeleteMsgFromDeleteFolder";
        #endregion

        #region sway 
        //Sway activities sway
        //public static readonly string CreatedSway = "CreatedSway";
        //public static readonly string ViewedSway = "ViewedSway";
        //public static readonly string SharedSway = "SharedSway";
        //public static readonly string DeletedSway = "DeletedSway";
        //public static readonly string DisableSwayDuplication = "DisableSwayDuplication";
        //public static readonly string DuplicatedSway = "DuplicatedSway";
        //public static readonly string EditedSway = "EditedSway";
        //public static readonly string EnableSwayDuplication = "EnableSwayDuplication";
        //public static readonly string TurnOffExternalSway = "TurnOffExternalSway";
        //public static readonly string TurnOnExternalSway = "TurnOnExternalSway";
        //public static readonly string RevokedSwaySharing = "RevokedSwaySharing";
        //public static readonly string TurnOffSwayService = "TurnOffSwayService";
        //public static readonly string TurnOnSwayService = "TurnOnSwayService";
        //public static readonly string ChangeSwayShareLevel = "ChangeSwayShareLevel";
        #endregion

        #region Azure AD
        //User administration activities  ad
        public static readonly string ADAddedUser = "Add user.";
        public static readonly string ADDeletedUser = "Delete user.";
        public static readonly string SetLicensProperties = "SetLicensProperties";
        public static readonly string ADResetUserPassword = "Reset user password.";
        public static readonly string ADChangedUserPassword = "Change user password.";
        public static readonly string ADChangedUserLicense = "Change user license.";
        public static readonly string ADUpdatedUser = "Update user.";
        public static readonly string SetPropertyChangePassword = "SetPropertyChangePassword";
        //Group administration activities  ad
        public static readonly string ADAddedGroup = "Add group.";
        public static readonly string ADUpdateGroup = "Update group.";
        public static readonly string ADDeleteGroup = "Delete group.";
        public static readonly string ADAddedMemberGroup = "Add member to group.";
        public static readonly string ADRemovedMemberGroup = "Remove member from group.";
        //Application administration activities  ad
        public static readonly string ADAddedServicePrincipal = "Add service principal.";
        public static readonly string ADRemoveServicePrincipal = "Remove service principal.";
        public static readonly string SetDelegationEntry = "SetDelegationEntry";
        public static readonly string RemoveCredential = "RemoveCredential";
        public static readonly string AddedDelegationEntry = "AddedDelegationEntry";
        public static readonly string AddedCredential = "AddedCredential";
        public static readonly string RemoveDelegationEntry = "RemoveDelegationEntry";
        //Role administration activities   ad
        public static readonly string ADAddedMemberRole = "Add member to role.";
        public static readonly string ADRemoveUserFromRole = "Remove member from role.";
        public static readonly string SetCompanyContact = "SetCompanyContact";

        // Directory administation activities  azure ad
        public static readonly string AddedPartner = "AddedPartner";
        public static readonly string RemovedPartner = "RemovedPartner";
        public static readonly string AddedDomain = "AddedDomain";
        public static readonly string RemovedDomain = "RemovedDomain";
        public static readonly string UpdatedDomain = "UpdatedDomain";
        public static readonly string SetDomainAuthentication = "SetDomainAuthentication";
        public static readonly string ADVerifiedDomain = "Verify domain.";
        public static readonly string UpdatedFederation = "UpdatedFederation";
        public static readonly string VerifiedEmailDomain = "VerifiedEmailDomain";
        public static readonly string TurnOnAzureADSync = "TurnOnAzureADSync";
        public static readonly string ADSetPasswordPolicy = "Set password policy.";
        public static readonly string ADSetCompanyInfo = "Set Company Information.";
        #endregion

        #region other
        //eDiscovery activities  sp
        //public static readonly string CreatedContentSerach = "CreatedContentSerach";
        //public static readonly string DeletedContentSearch = "DeletedContentSearch";
        //public static readonly string ChangedContentSerach = "ChangedContentSerach";
        //public static readonly string StartedContentSearch = "StartedContentSearch";
        //public static readonly string StoppedContentSearch = "StoppedContentSearch";
        //public static readonly string CreatedContentSearchAction = "CreatedContentSearchAction";
        //public static readonly string ChangedContentSearchAction = "ChangedContentSearchAction";
        //public static readonly string DeletedContentSearchAction = "DeletedContentSearchAction";
        //public static readonly string CreatedSearchPermissionsFilter = "CreatedSearchPermissionsFilter";
        //public static readonly string DeletedSearchPermissionsFilter = "DeletedSearchPermissionsFilter";
        //public static readonly string ChangedSearchPermissionsFilter = "ChangedSearchPermissionsFilter";
        //public static readonly string CreatedHoldIneDiscoveryCase = "CreatedHoldIneDiscoveryCase";
        //public static readonly string DeletedHoldIneDiscoveryCase = "DeletedHoldIneDiscoveryCase";
        //public static readonly string ChangedHoldIneDiscoveryCase = "ChangedHoldIneDiscoveryCase";
        //public static readonly string CreatedSearchQueryeDiscovery = "CreatedSearchQueryeDiscovery";
        //public static readonly string DeletedSearchQueryeDiscovery = "DeletedSearchQueryeDiscovery";
        //public static readonly string ChangedSearchQueryeDiscovery = "ChangedSearchQueryeDiscovery";
        //public static readonly string CreatedeDiscoveryCase = "CreatedeDiscoveryCase";
        //public static readonly string DeletedeDiscoveryCase = "DeletedeDiscoveryCase";
        //public static readonly string ChangedeDiscoveryCase = "ChangedeDiscoveryCase";
        //public static readonly string AddedMemberToeDiscoveryCase = "AddedMemberToeDiscoveryCase";
        //public static readonly string RemoveMemberFromeDiscovery = "RemoveMemberFromeDiscovery";
        //public static readonly string ChangedeDiscoveryCaseMembership = "ChangedeDiscoveryCaseMembership";
        //public static readonly string CreatedeDiscoveryAdministrator = "CreatedeDiscoveryAdministrator";
        //public static readonly string DeletedeDiscoveryAdministrator = "DeletedeDiscoveryAdministrator";
        //public static readonly string ChangedeDiscoveryAdministrator = "ChangedeDiscoveryAdministrator";
        //PowerBI activities 
        //public static readonly string ViewPowerBIDashboard = "ViewPowerBIDashboard";
        //public static readonly string CreatedPowerBIDashboard = "CreatedPowerBIDashboard";
        //public static readonly string EditedPowerBIDashboard = "EditedPowerBIDashboard";
        //public static readonly string DeletedPowerBIDashboard = "DeletedPowerBIDashboard";
        //public static readonly string SharedPowerBIDashboard = "SharedPowerBIDashboard";
        //public static readonly string DeletedPowerBIReport = "DeletedPowerBIReport";
        //public static readonly string DeletedPowerBIDatasets = "DeletedPowerBIDatasets";
        //public static readonly string CreatedPowerBIGroup = "CreatedPowerBIGroup";
        //public static readonly string AddedPowerBIGroupMember = "AddedPowerBIGroupMember";
        //public static readonly string CreatedOrgnizationPowerBI = "CreatedOrgnizationPowerBI";
        #endregion

        #region teams action
        public static readonly string BotAddedToTeam = "BotAddedToTeam";
        public static readonly string BotRemovedFromTeam = "BotRemovedFromTeam";
        public static readonly string ChannelAddedForTeam = "ChannelAdded";
        public static readonly string ChannelDeletedForTeam = "ChannelDeleted";
        public static readonly string ChannelSettingChanged = "ChannelSettingChanged";
        public static readonly string ConnectorAddedForTeam = "ConnectorAdded";
        public static readonly string ConnectorRemovedForTeam = "ConnectorRemoved";
        public static readonly string ConnectorUpdated = "ConnectorUpdated";
        public static readonly string MemberAddedForTeam = "MemberAdded";
        public static readonly string MemberRemovedForTeam = "MemberRemoved";
        public static readonly string MemberRoleChanged = "MemberRoleChanged";
        public static readonly string TabAdded = "TabAdded";
        public static readonly string TabRemoved = "TabRemoved";
        public static readonly string TabUpdated = "TabUpdated";
        public static readonly string TeamCreatedForTeam = "TeamCreated";
        public static readonly string TeamDeletedForTeam = "TeamDeleted";
        public static readonly string TeamSettingChangedForTeam = "TeamSettingChanged";
        public static readonly string TeamsSessionStartedForTeam = "TeamsSessionStarted";
        public static readonly string TeamsTenantSettingChanged = "TeamsTenantSettingChanged";
        #endregion

        #region forms action
        public static readonly string CreateComment = "CreateComment";
        public static readonly string CreateForm = "CreateForm";
        public static readonly string EditForm = "EditForm";
        public static readonly string MoveForm = "MoveForm";
        public static readonly string DeleteForm = "DeleteForm";
        public static readonly string ViewForm = "ViewForm";
        public static readonly string PreviewForm = "PreviewForm";
        public static readonly string ExportForm = "ExportForm";
        public static readonly string AllowShareFormForCopy = "AllowShareFormForCopy";
        public static readonly string DisallowShareFormForCopy = "DisallowShareFormForCopy";
        public static readonly string AddFormCoauthor = "AddFormCoauthor";
        public static readonly string RemoveFormCoauthor = "RemoveFormCoauthor";
        public static readonly string ViewRuntimeForm = "ViewRuntimeForm";
        public static readonly string CreateResponse = "CreateResponse";
        public static readonly string UpdateResponse = "UpdateResponse";
        public static readonly string DeleteAllResponses = "DeleteAllResponses";
        public static readonly string DeleteResponse = "DeleteResponse";
        public static readonly string ViewResponses = "ViewResponses";
        public static readonly string ViewResponse = "ViewResponse";
        public static readonly string GetSummaryLink = "GetSummaryLink";
        public static readonly string DeleteSummaryLink = "DeleteSummaryLink";
        public static readonly string UpdatePhishingStatus = "UpdatePhishingStatus";
        public static readonly string ProInvitation = "ProInvitation";
        public static readonly string UpdateFormSetting = "UpdateFormSetting";
        public static readonly string UpdateUserSetting = "UpdateUserSetting";
        public static readonly string ListForms = "ListForms";
        public static readonly string SubmitResponse = "SubmitResponse";
        #endregion

        #region Exchange mailbox activities
        public static readonly string MailItemsAccessed = "MailItemsAccessed";
        public static readonly string AddMailboxPermissions = "AddMailboxPermissions";
        public static readonly string UpdateCalendarDelegation = "UpdateCalendarDelegation";
        public static readonly string AddFolderPermissions = "AddFolderPermissions";
        public static readonly string MailboxCopy = "Copy";
        public static readonly string MailboxCreate = "Create";
        public static readonly string NewInboxRule = "New-InboxRule";
        public static readonly string SoftDelete = "SoftDelete";
        public static readonly string ApplyRecordLabel = "ApplyRecordLabel";
        public static readonly string MailboxMove = "Move";
        public static readonly string MoveToDeletedItems = "MoveToDeletedItems";
        public static readonly string UpdateFolderPermissions = "UpdateFolderPermissions";
        public static readonly string SetInboxRule = "Set-InboxRule";
        public static readonly string HardDelete = "HardDelete";
        public static readonly string RemoveMailboxPermission = "Remove-MailboxPermission";
        public static readonly string RemoveFolderPermissions = "RemoveFolderPermissions";
        public static readonly string MailboxSend = "Send";
        public static readonly string MailboxSendAs = "SendAs";
        public static readonly string SendOnBehalf = "SendOnBehalf";
        public static readonly string UpdateInboxRules = "UpdateInboxRules";
        public static readonly string MailboxUpdate = "Update";
        public static readonly string MailboxLogin = "MailboxLogin";
        public static readonly string ModifyFolderPermissions = "ModifyFolderPermissions";
        #endregion

        #region stream activities
        public static readonly string StreamInvokeVideoView = "StreamInvokeVideoView";
        public static readonly string StreamEditVideoPermissions = "StreamEditVideoPermissions";
        public static readonly string StreamInvokeVideoUpload = "StreamInvokeVideoUpload";
        public static readonly string StreamEditVideo = "StreamEditVideo";
        public static readonly string StreamInvokeVideoSetLink = "StreamInvokeVideoSetLink";
        public static readonly string StreamCreateVideo = "StreamCreateVideo";
        public static readonly string StreamInvokeVideoDownload = "StreamInvokeVideoDownload";
        public static readonly string StreamInvokeVideoShare = "StreamInvokeVideoShare";
        public static readonly string StreamCreateChannel = "StreamCreateChannel";
        public static readonly string StreamEditChannel = "StreamEditChannel";
        public static readonly string StreamDeleteVideo = "StreamDeleteVideo";
        public static readonly string StreamInvokeVideoLike = "StreamInvokeVideoLike";
        public static readonly string StreamCreateGroup = "StreamCreateGroup";
        public static readonly string StreamDeleteVideoComment = "StreamDeleteVideoComment";
        public static readonly string StreamEditGroup = "StreamEditGroup";
        public static readonly string StreamCreateVideoComment = "StreamCreateVideoComment";
        public static readonly string StreamInvokeVideoUnLike = "StreamInvokeVideoUnLike";
        public static readonly string StreamInvokeChannelSetThumbnail = "StreamInvokeChannelSetThumbnail";
        public static readonly string StreamEditUserSettings = "StreamEditUserSettings";
        #endregion

        #region other actions
        public static readonly string PasswordLogonInitialAuthUsingPassword = "PasswordLogonInitialAuthUsingPassword";
        public static readonly string UserLoggedIn = "UserLoggedIn";
        public static readonly string PasswordLogonCookieCopyUsingDAToken = "PasswordLogonCookieCopyUsingDAToken";
        public static readonly string SiteCollectionAdminRemoved = "SiteCollectionAdminRemoved";
        public static readonly string FilePreviewed = "FilePreviewed";
        public static readonly string ConsentToApplication = "Consent to application.";
        public static readonly string AddOAuth2PermissionGrant = "Add OAuth2PermissionGrant.";
        public static readonly string AddAppRoleAssignmentGrantToUser = "Add app role assignment grant to user.";
        public static readonly string RemovedFromSiteCollection = "RemovedFromSiteCollection";
        public static readonly string CreateCompany = "Create company";
        public static readonly string EnableAddressListPaging = "Enable-AddressListPaging";
        public static readonly string SetTransportConfig = "Set-TransportConfig";
        public static readonly string SetMailbox = "Set-Mailbox";
        public static readonly string SetOwaMailboxPolicy = "Set-OwaMailboxPolicy";
        public static readonly string AddMailboxPermission = "Add-MailboxPermission";
        public static readonly string SetTenantObjectVersion = "Set-TenantObjectVersion";
        public static readonly string DeleteApplicationPassword = "Delete application password for user.";
        public static readonly string CreateApplicationPassword = "Create application password for user.";
        public static readonly string FolderDeleted = "FolderDeleted";
        public static readonly string FolderModified = "FolderModified";
        public static readonly string NewExchangeAssistanceConfig = "New-ExchangeAssistanceConfig";
        public static readonly string InstallDefaultSharingPolicy = "Install-DefaultSharingPolicy";
        public static readonly string InstallAdminAuditLogConfig = "Install-AdminAuditLogConfig";
        public static readonly string InstallDataClassificationConfig = "Install-DataClassificationConfig";
        public static readonly string InstallResourceConfig = "Install-ResourceConfig";
        public static readonly string SetRecipientEnforcementProvisioningPolicy = "Set-RecipientEnforcementProvisioningPolicy";
        public static readonly string SetExchangeAssistanceConfig = "Set-ExchangeAssistanceConfig";
        public static readonly string NewDkimSigningConfig = "New-DkimSigningConfig";
        public static readonly string SetAdminAuditLogConfig = "Set-AdminAuditLogConfig";
        public static readonly string NewMailbox = "New-Mailbox";
        public static readonly string FileDeletedFirstStageRecycleBin = "FileDeletedFirstStageRecycleBin";
        public static readonly string FileDeletedSecondStageRecycleBin = "FileDeletedSecondStageRecycleBin";
        #endregion
        #endregion

        #region event action 国际化
        #region Sharepoint Online
        //File and folder activities
        private static string I18nCheckedInFile { get { return I18NEntity.GetString("ReportCenter.Common_da090915-cb10-458f-8453-4e09680a4102", "Checked in file"); } }
        private static string I18nCheckOutFile { get { return I18NEntity.GetString("ReportCenter.Common_7a1c907a-68bd-4c46-b9a0-dcf7667a9d84", "Checked out file"); } }
        private static string I18nDiscardedFileCheckout { get { return I18NEntity.GetString("ReportCenter.Common_71606b21-0c11-474b-a000-d26e35fed0b0", "Discarded file checkout"); } }
        private static string I18nCopiedFile { get { return I18NEntity.GetString("ReportCenter.Common_88123bd6-c319-40da-90f5-a01264e19ce3", "Copied file"); } }
        private static string I18nDeletedFile { get { return I18NEntity.GetString("ReportCenter.Common_c0e35652-6453-41a0-bf39-b5e4e7c441a1", "Deleted file"); } }
        private static string I18nDownloadedFile { get { return I18NEntity.GetString("ReportCenter.Common_c8b5ba0a-1dc7-484e-ab0f-c866cbed4c25", "Downloaded file"); } }
        private static string I18nAccessedFile { get { return I18NEntity.GetString("ReportCenter.Common_799b5273-9922-48a5-9b77-8518d1efe1fa", "FileAccessed"); } }
        private static string I18nModifiedFile { get { return I18NEntity.GetString("ReportCenter.Common_f43e65ef-ef8f-4b2f-acad-d5ef80fa8bb8", "Modified file"); } }
        private static string I18nMovedFile { get { return I18NEntity.GetString("ReportCenter.Common_ea77a85e-d1b7-48ae-afd4-89b0c106036f", "Moved file"); } }
        private static string I18nRenamedFile { get { return I18NEntity.GetString("ReportCenter.Common_a618c69c-f537-4fc8-85f8-058fa6144acd", "Renamed file"); } }
        private static string I18nRestoredFile { get { return I18NEntity.GetString("ReportCenter.Common_efbbb2f8-a656-444d-85a6-9add3e78958a", "Restored file"); } }
        private static string I18nUploadedFile { get { return I18NEntity.GetString("ReportCenter.Common_0441e1aa-a422-4448-a065-560344fcd12b", "Uploaded file"); } }
        private static string I18nVideoRequest { get { return I18NEntity.GetString("ReportCenter.Common_d55d7ee3-ef2e-4a67-8721-3089b1f51f39", "Video requested"); } }
        private static string I18nPageViewed { get { return I18NEntity.GetString("ReportCenter.Common_78b076fb-dea2-4ff8-bb0e-d312c66465a6", "PageViewed"); } }
        private static string I18nFileVersionDeleted { get { return I18NEntity.GetString("ReportCenter.Common_8db8f9f1-8bb7-47dc-836f-1283f785a78f", "Deleted file version"); } }

        private static string I18nComplianceSettingChanged { get { return I18NEntity.GetString("ReportCenter.Common_37c95fa4-f03e-40a6-a02a-0e927818d5ec", "Changed compliance policy label"); } }
        private static string I18nLockRecord { get { return I18NEntity.GetString("ReportCenter.Common_9fb2af1e-f0e4-4674-8374-b3550664b352", "Changed record status to locked"); } }
        private static string I18nUnlockRecord { get { return I18NEntity.GetString("ReportCenter.Common_086a9892-b04d-430b-bc1a-bf65aabbb515", "Changed record status to unlocked"); } }
        private static string I18nComplianceRecordDelete { get { return I18NEntity.GetString("ReportCenter.Common_8d567247-dd5b-4094-813a-25adbf22a9f9", "Deleted record compliance policy label"); } }
        private static string I18nDocumentSensitivityMismatchDetected { get { return I18NEntity.GetString("ReportCenter.Common_2682ef55-7cf3-4467-ab3f-d24ceac526ba", "Detected document sensitivity mismatch"); } }
        private static string I18nFileMalwareDetected { get { return I18NEntity.GetString("ReportCenter.Common_3da283bc-66d3-4a3f-836a-e0ffdf017724", "Detected malware in file"); } }
        private static string I18nFileModifiedExtended { get { return I18NEntity.GetString("ReportCenter.Common_1fb696bf-ec9c-4c3c-b34f-2c4f3fdecdff", "FileModifiedExtended"); } }
        private static string I18nSearchQueryPerformed { get { return I18NEntity.GetString("ReportCenter.Common_8f491e9b-ec47-4393-ab7c-3dd73bef7d4a", "Performed search query"); } }
        private static string I18nFileVersionsAllMinorsRecycled { get { return I18NEntity.GetString("ReportCenter.Common_f5060a96-f164-4628-af85-797557de9e3c", "Recycled all minor versions of file"); } }
        private static string I18nFileVersionsAllRecycled { get { return I18NEntity.GetString("ReportCenter.Common_5f2147b8-30e2-44e4-9c1c-1ce6c81fdf77", "Recycled all versions of file"); } }
        private static string I18nFileVersionRecycled { get { return I18NEntity.GetString("ReportCenter.Common_894ef9aa-8de6-445a-80cb-92c76b9dba1e", "Recycled version of file"); } }
        private static string I18nPageViewedExtended { get { return I18NEntity.GetString("ReportCenter.Common_1a3fd829-37b1-44db-8d08-203a1f050fd8", "PageViewedExtended"); } }
        private static string I18nClientViewSignaled { get { return I18NEntity.GetString("ReportCenter.Common_538d6abe-14f4-453c-9d90-fd82088069f6", "View signaled by client"); } }
        private static string I18nPagePrefetched { get { return I18NEntity.GetString("ReportCenter.Common_2a4f6c5c-2034-42d6-a101-d95a3720f560", "PagePrefetched"); } }
        private static string I18nFileAccessedExtended { get { return I18NEntity.GetString("ReportCenter.Common_be515db9-6aff-4d5c-8364-8a5f3f518375", "FileAccessedExtended"); } }

        private static string I18nFolderCopied { get { return I18NEntity.GetString("ReportCenter.Common_61f5a45a-b9e4-4ab6-b782-a16328d14a22", "Copied folder"); } }
        private static string I18nFolderCreated { get { return I18NEntity.GetString("ReportCenter.Common_377f00dc-cd76-4168-bc51-0213da26730e", "Created folder"); } }
        private static string I18nFolderDeletedFirstStageRecycleBin { get { return I18NEntity.GetString("ReportCenter.Common_ae573d2a-2eed-47ca-9ccb-8bfe131faabd", "Deleted folder from recycle bin"); } }
        private static string I18nFolderDeletedSecondStageRecycleBin { get { return I18NEntity.GetString("ReportCenter.Common_ab8f0856-8991-4257-9667-660e662e0176", "Deleted folder from second-stage recycle bin"); } }
        private static string I18nFolderMoved { get { return I18NEntity.GetString("ReportCenter.Common_8603225c-ae55-4263-bf9a-489a7c5b16a2", "Moved folder"); } }
        private static string I18nFolderRenamed { get { return I18NEntity.GetString("ReportCenter.Common_00824bde-6669-464c-a888-50eb2f438e88", "Renamed folder"); } }
        private static string I18nFolderRestored { get { return I18NEntity.GetString("ReportCenter.Common_9ce6ca5d-6bad-43b0-8f65-f8b05d82fdb3", "Restored folder"); } }

        private static string I18nListCreated { get { return I18NEntity.GetString("ReportCenter.Common_43437e22-1fc6-4f19-a558-db070a9975ef", "Created list"); } }
        private static string I18nListColumnCreated { get { return I18NEntity.GetString("ReportCenter.Common_d81005e9-b76c-4256-bc02-452f60e8d8b5", "Created list column"); } }
        private static string I18nListContentTypeCreated { get { return I18NEntity.GetString("ReportCenter.Common_24b10fe7-495d-40ca-98aa-c2f4a693270d", "Created list content type"); } }
        private static string I18nListItemCreated { get { return I18NEntity.GetString("ReportCenter.Common_cee7a58b-6716-4367-85d5-265830b70779", "Created list item"); } }
        private static string I18nSiteColumnCreated { get { return I18NEntity.GetString("ReportCenter.Common_1540e508-6e85-4092-b99f-e1bdffddb49a", "Created site column"); } }
        private static string I18nSiteContentTypeCreated { get { return I18NEntity.GetString("ReportCenter.Common_83ef655f-4952-46ea-924b-27685b851017", "Created site content type"); } }
        private static string I18nListDeleted { get { return I18NEntity.GetString("ReportCenter.Common_7c3f733b-0a22-4a22-a186-f9e0af7786c2", "Deleted list"); } }
        private static string I18nListColumnDeleted { get { return I18NEntity.GetString("ReportCenter.Common_21642a2d-dd32-4340-a14f-78b136e90915", "Deleted list column"); } }
        private static string I18nListContentTypeDeleted { get { return I18NEntity.GetString("ReportCenter.Common_2198f3a0-2159-4b52-a61c-668822622d69", "Deleted list content type"); } }
        private static string I18nListItemDeleted { get { return I18NEntity.GetString("ReportCenter.Common_175884d7-f41b-4a0a-9b52-4a132f22b3d1", "Deleted list item"); } }
        private static string I18nSiteColumnDeleted { get { return I18NEntity.GetString("ReportCenter.Common_62aadbf6-f1c6-4cf3-804c-4481c60118d3", "Deleted site column"); } }
        private static string I18nSiteContentTypeDeleted { get { return I18NEntity.GetString("ReportCenter.Common_56c009ef-7140-4859-940c-7eb1f8f97b55", "Deleted site content type"); } }
        private static string I18nListItemRecycled { get { return I18NEntity.GetString("ReportCenter.Common_2d54af19-010c-44e6-9efd-15ed14b3a86b", "Recycled list item"); } }
        private static string I18nListRestored { get { return I18NEntity.GetString("ReportCenter.Common_fbc90122-2454-43f0-aab0-c4ea2173a8c6", "Restored list"); } }
        private static string I18nListItemRestored { get { return I18NEntity.GetString("ReportCenter.Common_fcce6dea-5c2b-4a09-aa75-963a3099b980", "Restored list item"); } }
        private static string I18nListUpdated { get { return I18NEntity.GetString("ReportCenter.Common_1505dd63-dd78-4fb1-ace8-dfe3d7322184", "Updated list"); } }
        private static string I18nListColumnUpdated { get { return I18NEntity.GetString("ReportCenter.Common_0cc8c029-b23c-4da9-82f0-032462401d5c", "Updated list column"); } }
        private static string I18nListContentTypeUpdated { get { return I18NEntity.GetString("ReportCenter.Common_9f9b30e9-1bd8-4fe6-a523-5ef3a016554b", "Updated list content type"); } }
        private static string I18nListItemUpdated { get { return I18NEntity.GetString("ReportCenter.Common_70de5883-639f-4286-9a9e-923ff3dc14d5", "Updated list item"); } }
        private static string I18nSiteColumnUpdated { get { return I18NEntity.GetString("ReportCenter.Common_20477709-5bbb-41ab-9f74-5e9413ec2ea8", "Updated site column"); } }
        private static string I18nSiteContentTypeUpdated { get { return I18NEntity.GetString("ReportCenter.Common_0780412e-f055-4e2a-bc71-d741f4ae4d00", "Updated site content type"); } }
        private static string I18nListViewCreated { get { return I18NEntity.GetString("ReportCenter.Common_3dd003b7-0e2f-453a-b351-285bc544f097", "Created list view"); } }
        private static string I18nListViewUpdated { get { return I18NEntity.GetString("ReportCenter.Common_8382ee65-91fe-4cbf-9979-2a3a2f73e587", "Updated list view"); } }
        private static string I18nListViewed { get { return I18NEntity.GetString("ReportCenter.Common_695f0fbb-9945-8823-9663-2dbf0227942a", "View list"); } }

        private static string I18nPermissionLevelAdded { get { return I18NEntity.GetString("ReportCenter.Common_8718bbbd-69d1-4987-9537-9fb35b4627f4", "Added permission level to site collection"); } }
        private static string I18nAccessRequestAccepted { get { return I18NEntity.GetString("ReportCenter.Common_3d7abc0b-139e-460f-9151-28e4b7ee8e93", "Accepted access request"); } }

        private static string I18nSharingInvitationBlocked { get { return I18NEntity.GetString("ReportCenter.Common_97b6db67-884d-4957-ad43-6b023d965b58", "Blocked sharing invitation"); } }
        private static string I18nSecureLinkCreated { get { return I18NEntity.GetString("ReportCenter.Common_ada25d4b-875d-4d90-8961-cf1eb8c3303a", "Created secure link"); } }
        private static string I18nSecureLinkDeleted { get { return I18NEntity.GetString("ReportCenter.Common_f309a813-125a-41bd-8224-3cbd151542df", "Deleted secure link"); } }
        private static string I18nAccessRequestDenied { get { return I18NEntity.GetString("ReportCenter.Common_bcc1620c-0e32-43c4-98b7-eb9f956438c9", "Denied access request"); } }
        private static string I18nAccessRequestUpdated { get { return I18NEntity.GetString("ReportCenter.Common_7e8b0973-477d-4fb8-8aaa-0c32a315941f", "Updated access request"); } }
        private static string I18nSharingInvitationUpdated { get { return I18NEntity.GetString("ReportCenter.Common_d3373064-6c20-488e-8920-10212f2a6977", "Updated sharing invitation"); } }
        private static string I18nSecureLinkUsed { get { return I18NEntity.GetString("ReportCenter.Common_8a2008ff-3859-41c8-a0e3-cc6b742cc4a2", "Used secure link"); } }
        private static string I18nAddedToSecureLink { get { return I18NEntity.GetString("ReportCenter.Common_0e94c867-8a9e-4eb6-9197-4960d3a87e43", "User added to secure link"); } }
        private static string I18nRemovedFromSecureLink { get { return I18NEntity.GetString("ReportCenter.Common_110c399d-2347-4ea6-9fce-fbb0d4265360", "User removed from secure link"); } }
        private static string I18nWebAccessRequestApproverModified { get { return I18NEntity.GetString("ReportCenter.Common_e9ac9800-dca5-45e9-8c36-58f6f8f83dc6", "Modified web access request approver"); } }

        private static string I18nManagedSyncClientAllowed { get { return I18NEntity.GetString("ReportCenter.Common_fa8d6995-89fc-4345-bb55-52a9f1e1387b", "Allowed computer to sync files"); } }
        private static string I18nUnmanagedSyncClientBlocked { get { return I18NEntity.GetString("ReportCenter.Common_a2e3ab2d-64d1-4f8b-ad40-9904ba8cf2c3", "Blocked computer from syncing files"); } }
        private static string I18nFileSyncDownloadedFull { get { return I18NEntity.GetString("ReportCenter.Common_f928e564-8180-4572-bcbe-680940abb55a", "Downloaded files to computer"); } }
        private static string I18nFileSyncDownloadedPartial { get { return I18NEntity.GetString("ReportCenter.Common_3c46fdc9-d45f-4f3a-afc2-24e12b1be156", "Downloaded file changes to computer"); } }
        private static string I18nFileSyncUploadedFull { get { return I18NEntity.GetString("ReportCenter.Common_c8d05d2c-4e4f-4881-bc10-04a783140f73", "Uploaded files to document library"); } }
        private static string I18nFileSyncUploadedPartial { get { return I18NEntity.GetString("ReportCenter.Common_7cc9e7c1-d333-4970-b513-c08d751b21a4", "Uploaded file changes to document library"); } }

        private static string I18nPermissionLevelsInheritanceBroken { get { return I18NEntity.GetString("ReportCenter.Common_34fbca93-8437-45ac-a0d7-c2565614b4cf", "Broke permission level inheritance"); } }
        private static string I18nSharingInheritanceBroken { get { return I18NEntity.GetString("ReportCenter.Common_6934014b-a686-445b-a5c8-59e73616c93e", "Broke sharing inheritance"); } }
        private static string I18nWebRequestAccessModified { get { return I18NEntity.GetString("ReportCenter.Common_e27efced-c70b-4b75-9874-4ef4b39720b5", "Modified access request setting"); } }
        private static string I18nWebMembersCanShareModified { get { return I18NEntity.GetString("ReportCenter.Common_93c9ddf2-f9b2-4531-8e2c-5f42f13033d0", "Modified Members Can Share' setting"); } }
        private static string I18nPermissionLevelModified { get { return I18NEntity.GetString("ReportCenter.Common_3f458520-448d-43ab-8496-0f0559493e9b", "Modified permission level on site collection"); } }
        private static string I18nPermissionLevelRemoved { get { return I18NEntity.GetString("ReportCenter.Common_179e5e48-be47-4d41-81dd-73515470113e", "Removed permission level from site collection"); } }
        private static string I18nSharingInheritanceReset { get { return I18NEntity.GetString("ReportCenter.Common_7a0c71f2-e893-4ee9-b271-f90d0035d9ea", "Restored sharing inheritance"); } }

        private static string I18nAllowedDataLocationAdded { get { return I18NEntity.GetString("ReportCenter.Common_c1448533-49a2-4adb-a011-35f578a3769f", "Added allowed data location"); } }
        private static string I18nExemptUserAgentSet { get { return I18NEntity.GetString("ReportCenter.Common_506e064d-fb2d-40e0-a630-08981749829c", "Added exempt user agent"); } }
        private static string I18nGeoAdminAdded { get { return I18NEntity.GetString("ReportCenter.Common_753cef58-33d8-4374-8d45-1c6cb0d332e4", "Added geo location admin"); } }
        private static string I18nAllowGroupCreationSet { get { return I18NEntity.GetString("ReportCenter.Common_9b4c1b17-daed-4543-98cf-4f4189e6b504", "Allowed user to create groups"); } }
        private static string I18nSiteGeoMoveCancelled { get { return I18NEntity.GetString("ReportCenter.Common_4461fd8b-b582-428c-9151-9066a44220bd", "Cancelled site geo move"); } }
        private static string I18nDeviceAccessPolicyChanged { get { return I18NEntity.GetString("ReportCenter.Common_9ada6ffb-ff58-44ae-8619-4a803d688756", "Changed device access policy"); } }
        private static string I18nCustomizeExemptUsers { get { return I18NEntity.GetString("ReportCenter.Common_1bab5765-c82c-4a42-946a-5fefab52c245", "Changed exempt user agents"); } }
        private static string I18nNetworkAccessPolicyChanged { get { return I18NEntity.GetString("ReportCenter.Common_65ce8120-5078-4154-8663-0d707eb8e260", "Changed network access policy"); } }
        private static string I18nSiteGeoMoveCompleted { get { return I18NEntity.GetString("ReportCenter.Common_718dc860-ea17-4614-86ec-fd76f4f0ecf9", "Completed site geo move"); } }
        private static string I18nHubSiteOrphanHubDeleted { get { return I18NEntity.GetString("ReportCenter.Common_9952b945-ebe6-4490-a0e8-b0334698e3a2", "Deleted orphaned hub site"); } }
        private static string I18nSiteDeleted { get { return I18NEntity.GetString("ReportCenter.Common_0a687f6f-e7c6-456a-85cc-ff93e4cef402", "Deleted site"); } }
        private static string I18nLegacyWorkflowEnabledSet { get { return I18NEntity.GetString("ReportCenter.Common_9cd40315-9daa-4e7a-adbf-0609da455fed", "Enabled legacy workflow"); } }
        private static string I18nOfficeOnDemandSet { get { return I18NEntity.GetString("ReportCenter.Common_206d4eab-61fa-44dd-9519-4f3067fa544d", "Enabled Office on Demand"); } }
        private static string I18nPeopleResultsScopeSet { get { return I18NEntity.GetString("ReportCenter.Common_156d2849-d3c5-4f95-bf90-5b56a7f81a31", "Enabled result source for People Searches"); } }
        private static string I18nNewsFeedEnabledSet { get { return I18NEntity.GetString("ReportCenter.Common_06561881-1e3d-4a7a-90cf-ec079cee9a7c", "Enabled RSS feeds"); } }
        private static string I18nHubSiteJoined { get { return I18NEntity.GetString("ReportCenter.Common_0e382f06-be6a-4f24-9493-518ae0a77786", "Joined site to hub site"); } }
        private static string I18nHubSiteRegistered { get { return I18NEntity.GetString("ReportCenter.Common_f89d87d4-bb48-44ec-be22-32d3e81ad69e", "Registered hub site"); } }
        private static string I18nAllowedDataLocationDeleted { get { return I18NEntity.GetString("ReportCenter.Common_e7bbca47-6d79-465e-8e2c-dd6cbae0cf5c", "Removed allowed data location"); } }
        private static string I18nGeoAdminDeleted { get { return I18NEntity.GetString("ReportCenter.Common_214fca8d-138c-4a93-add1-4cb5bdef0167", "Removed geo location admin"); } }
        private static string I18nSiteRenamed { get { return I18NEntity.GetString("ReportCenter.Common_79c4f818-ea63-47c7-b38a-550def75cf4b", "Renamed site"); } }
        private static string I18nSiteGeoMoveScheduled { get { return I18NEntity.GetString("ReportCenter.Common_9b9be527-fd8b-46ed-bad1-08270a4db782", "Scheduled site geo move"); } }
        private static string I18nHostSiteSet { get { return I18NEntity.GetString("ReportCenter.Common_a02d8224-afeb-4298-a4a2-afae06178da4", "Set host site"); } }
        private static string I18nGeoQuotaAllocated { get { return I18NEntity.GetString("ReportCenter.Common_4307da29-7d0e-4124-9865-66b87310709a", "Set storage quota for geo location"); } }
        private static string I18nHubSiteUnjoined { get { return I18NEntity.GetString("ReportCenter.Common_007a1322-264b-4789-8b27-017860a61b09", "Unjoined site from hub site"); } }
        private static string I18nHubSiteUnregistered { get { return I18NEntity.GetString("ReportCenter.Common_0492b484-0379-41bc-8d6f-ca2ce5e89a1f", "Unregistered hub site"); } }
        //sharing and access request activities
        private static string I18nUnsharedFileFolder { get { return I18NEntity.GetString("ReportCenter.Common_69f0dadf-76b0-4a8a-b2db-10e25572c8fe", "Unshared file, folder, or site"); } }
        private static string I18nSharedFileFolder { get { return I18NEntity.GetString("ReportCenter.Common_64e1ee51-91d4-4fed-bc64-d9d562bd1604", "Shared file, folder, or site"); } }
        private static string I18nCreateSharingInvitation { get { return I18NEntity.GetString("ReportCenter.Common_e5526b14-86c8-4c67-af4e-1602c673b64f", "Created sharing invitation"); } }
        private static string I18nAcceptSharingInvitation { get { return I18NEntity.GetString("ReportCenter.Common_230e4a45-a3a2-432b-bfa4-c12d9206474b", "Accepted sharing invitation"); } }
        private static string I18nWithdrewSharingInvitation { get { return I18NEntity.GetString("ReportCenter.Common_ae571cff-4244-4f3f-a55a-0201aa596a14", "Withdrew sharing invitation"); } }
        private static string I18nCreateAnonymousLink { get { return I18NEntity.GetString("ReportCenter.Common_f5725f89-708b-49ac-bf5a-1c17c868e436", "Created an anonymous link"); } }
        private static string I18nUsedAnonymousLink { get { return I18NEntity.GetString("ReportCenter.Common_c31404ea-70fb-4c30-8047-b9c98cde5c4c", "Used an anonymous link"); } }
        private static string I18nRemovedAnonymousLink { get { return I18NEntity.GetString("ReportCenter.Common_972c12e7-d2ef-4921-9ecb-4cf5e81b56ea", "Removed an anonymous link"); } }
        private static string I18nUpdatedAnonymousLink { get { return I18NEntity.GetString("ReportCenter.Common_8778ad59-cc84-4dd4-9c48-3beb7b0d3792", "Updated an anonymous link"); } }
        private static string I18nCreateCompanyShareLink { get { return I18NEntity.GetString("ReportCenter.Common_17fb1df0-e798-4da4-8cdc-820bdc1e83cb", "Created a company shareable link"); } }
        private static string I18nUsedCompanyShareLink { get { return I18NEntity.GetString("ReportCenter.Common_b60bc47c-1754-44c1-844b-5563ab14dde1", "Used a company shareable link"); } }
        private static string I18nRemovedCompanyShareLink { get { return I18NEntity.GetString("ReportCenter.Common_7f664774-d319-400d-962a-cac8ced40fe9", "Removed a company shareable link"); } }
        private static string I18nAcceptAccessRequest { get { return I18NEntity.GetString("ReportCenter.Common_fd6ee278-817d-4210-8d93-b93bb12a6b89", "Accepted access request"); } }
        private static string I18nCreateAccessRequest { get { return I18NEntity.GetString("ReportCenter.Common_ad607f13-4342-4153-8314-f1964a1491d5", "Created access request"); } }
        private static string I18nDenyAccessRequest { get { return I18NEntity.GetString("ReportCenter.Common_fce8ae3f-5b40-4cfb-a2ad-bffd28040be5", "Denied access request"); } }
        //synchronization activities
        private static string I18nAllowComputerSyncFile { get { return I18NEntity.GetString("ReportCenter.Common_58e23019-0563-4953-8bbb-909e85ae1d67", "Allowed computer to sync files"); } }
        private static string I18nBlockComputerSyncfile { get { return I18NEntity.GetString("ReportCenter.Common_ec6bf1d5-08f8-4854-985a-c4398f0e8356", "Blocked computer from syncing file"); } }
        //Site administration activities
        private static string I18nAllowUserCreateGroup { get { return I18NEntity.GetString("ReportCenter.Common_fde80911-2b3b-4932-9b04-0df2d232483f", "Allowed user to create groups"); } }
        private static string I18nChangeExemptUserAgent { get { return I18NEntity.GetString("ReportCenter.Common_afd7c878-8262-4dae-9ca4-9ddef3cc896f", "Changed exempt user agents"); } }
        private static string I18nAddExemptUserAgenty { get { return I18NEntity.GetString("ReportCenter.Common_025e4295-a7ba-4ae9-993b-054d9788c0be", "Added exempt user agent"); } }
        private static string I18nCreatedGrouop { get { return I18NEntity.GetString("ReportCenter.Common_decea9e2-7677-4186-82db-67f6471fbfbb", "Created group"); } }
        private static string I18nDeletedGroup { get { return I18NEntity.GetString("ReportCenter.Common_bc139b04-7d60-4d5a-b67c-32384239dc7b", "Deleted group"); } }
        private static string I18nUpdatedGroup { get { return I18NEntity.GetString("ReportCenter.Common_8241b0e7-a794-4dc2-b662-1591f5fb1265", "Updated group"); } }
        private static string I18nSetHostSite { get { return I18NEntity.GetString("ReportCenter.Common_c8dffdcd-3fa4-42d6-aa96-d8ab020e3fca", "Set host site"); } }
        private static string I18nEnableLegacyWorkflow { get { return I18NEntity.GetString("ReportCenter.Common_79e75b8c-cd00-4501-9340-56919d731ba2", "Enabled legacy workflow"); } }
        private static string I18nEnableRSSFeeds { get { return I18NEntity.GetString("ReportCenter.Common_952847cd-3458-4797-9cda-c9286e293d0d", "Enabled RSS feeds"); } }
        private static string I18nEnableOfficeDemand { get { return I18NEntity.GetString("ReportCenter.Common_ceffb87b-0ec1-452b-890f-7b753f409698", "Enabled office on Demand"); } }
        private static string I18nEnableDocumentPreview { get { return I18NEntity.GetString("ReportCenter.Common_50282ba7-88c2-4750-89c4-a448decbbfa4", "Enabled document preview"); } }
        private static string I18nEnableResultPeopleSear { get { return I18NEntity.GetString("ReportCenter.Common_0d97443c-2456-44de-a9a7-dedad4b953ee", "Enabled result source for pepple searches"); } }
        private static string I18nCreateSentConnection { get { return I18NEntity.GetString("ReportCenter.Common_12edf549-9d0a-442b-aaba-028bbe5c1d79", "Created sent to connection"); } }
        private static string I18nDeleteSentConnection { get { return I18NEntity.GetString("ReportCenter.Common_e5cd0da6-25b6-442e-8f54-8bbaf0b5fe2f", "Deleted sent to connection"); } }
        private static string I18nRequestSiteAdminPermission { get { return I18NEntity.GetString("ReportCenter.Common_85e2f828-3a88-44b8-b5a8-020171776314", "Requested site admin permission"); } }
        private static string I18nAddSiteCollectionAdmin { get { return I18NEntity.GetString("ReportCenter.Common_30ae2589-7734-4899-bff7-11f4bc901706", "Added site collection admin"); } }
        private static string I18nCreateSiteCollection { get { return I18NEntity.GetString("ReportCenter.Common_0a655fcf-d647-43db-9465-8a50bcb2ec30", "Created site collection"); } }
        private static string I18nModifySitePermission { get { return I18NEntity.GetString("ReportCenter.Common_03b631b1-a57b-4c33-b1b6-bbab23667374", "Modified site permissions"); } }
        private static string I18nRenamedSite { get { return I18NEntity.GetString("ReportCenter.Common_dd3ee290-c2e7-40c8-be56-16e8c3e6796f", "Renamed site"); } }
        private static string I18nAddUserGroupToSP { get { return I18NEntity.GetString("ReportCenter.Common_d666169f-78e7-4c25-b442-c767cdd0a5c2", "Added user or group to SharePoint group"); } }
        private static string I18nChangeSharingPolicy { get { return I18NEntity.GetString("ReportCenter.Common_304785fa-3820-4d0b-9461-5407c6984885", "Changed a sharing policy"); } }
        private static string I18nRemoveUserGroupFromSP { get { return I18NEntity.GetString("ReportCenter.Common_8503d864-87fe-4bf1-98aa-59de2e77f1cd", "Removed user or group from SharePoint group"); } }
        #endregion
        #region Exchange mailbox activities
        private static string I18nCreateReceiveMsg { get { return I18NEntity.GetString("ReportCenter.Common_801651e6-b30e-43f8-bd21-55d923ddb8c3", "Created or received message"); } }
        private static string I18nCopyMsgAnotherFolder { get { return I18NEntity.GetString("ReportCenter.Common_fad42fdc-148f-4edf-98ab-20c93f021433", "Copied message to another folder"); } }
        private static string I18nUserSignMailbox { get { return I18NEntity.GetString("ReportCenter.Common_7dcc5c53-81fb-4bbd-8b6c-2bdbdc0e67ec", "User signed in to mailbox"); } }
        private static string I18nSentMsgOnBehalfPermission { get { return I18NEntity.GetString("ReportCenter.Common_81a924f6-8728-4ca6-8cee-4ad47e6d1042", "Sent message using Send On Behalf permissions"); } }
        private static string I18nPurgeMsgFromMailbox { get { return I18NEntity.GetString("ReportCenter.Common_7a8f75ff-926c-445b-a4ae-5fb90f8a8469", "Purged messages from mailbox"); } }
        private static string I18nMoveMsgDeleteFolder { get { return I18NEntity.GetString("ReportCenter.Common_1b4f8ecf-758b-4f61-b38a-081eefc1fbbe", "Moved message to Deleted items folder"); } }
        private static string I18nMoveMsgAnotherFolder { get { return I18NEntity.GetString("ReportCenter.Common_e1bd015e-c3d6-40bd-b4df-974b4d50f5ad", "Moved message to anpther folder"); } }
        private static string I18nSentMsgSendAsPermission { get { return I18NEntity.GetString("ReportCenter.Common_3147eb7b-6cf6-4265-a491-cddd118deaf1", "Sent message using Send As permissions"); } }
        private static string I18nUpdatedMsg { get { return I18NEntity.GetString("ReportCenter.Common_47af9098-0885-4277-b2e1-fbcea355d8ac", "Updated message"); } }
        private static string I18nDeleteMsgFromDeleteFolder { get { return I18NEntity.GetString("ReportCenter.Common_8f66e988-b88b-446d-9bdd-454912fb6a20", "Deleted message from Deleted items folder"); } }
        #endregion
        #region sway
        //Sway activities
        //private static string I18nCreatedSway { get { return I18NEntity.GetString("ReportCenter.Common_589c6f5e-a9f8-4f60-9137-fad753859d9e", "Created Sway"); } }
        //private static string I18nViewedSway { get { return I18NEntity.GetString("ReportCenter.Common_98bb428b-988f-494e-b629-c784565163fd", "Viewed Sway"); } }
        //private static string I18nSharedSway { get { return I18NEntity.GetString("ReportCenter.Common_3d771129-f49b-42da-af78-8b91de83622d", "Shared Sway"); } }
        //private static string I18nDeletedSway { get { return I18NEntity.GetString("ReportCenter.Common_58436c73-7c78-40ca-9c1a-2f052b3bf1d7", "Deleted Sway"); } }
        //private static string I18nDisableSwayDuplication { get { return I18NEntity.GetString("ReportCenter.Common_c1d685e2-814e-4ca6-bcab-6854e24b6068", "Disabled Sway dupliction"); } }
        //private static string I18nDuplicatedSway { get { return I18NEntity.GetString("ReportCenter.Common_0187cb8b-dd83-404d-ad9c-ccb83094b7a6", "Duplicated Sway"); } }
        //private static string I18nEditedSway { get { return I18NEntity.GetString("ReportCenter.Common_627a0ab1-ed16-4bb2-8655-0c1836acb73e", "Edited Sway"); } }
        //private static string I18nEnableSwayDuplication { get { return I18NEntity.GetString("ReportCenter.Common_fb51d73e-13e2-4821-b76d-4cb017ece21f", "Enabled Sway Duplication"); } }
        //private static string I18nTurnOffExternalSway { get { return I18NEntity.GetString("ReportCenter.Common_3325cb7e-25a2-4793-a7be-61bb4ca0243a", "Turned off external sharing of Sway"); } }
        //private static string I18nTurnOnExternalSway { get { return I18NEntity.GetString("ReportCenter.Common_13d986fc-45b8-43f9-9b4b-ea60767eab2c", "Turned on external sharing of Sway"); } }
        //private static string I18nRevokedSwaySharing { get { return I18NEntity.GetString("ReportCenter.Common_946293d7-995b-440b-8fdd-a6cc70238f9b", "Revoked Sway sharing"); } }
        //private static string I18nTurnOffSwayService { get { return I18NEntity.GetString("ReportCenter.Common_ed9623e1-005d-495c-a6dc-84348d73520b", "Turned off Sway service"); } }
        //private static string I18nTurnOnSwayService { get { return I18NEntity.GetString("ReportCenter.Common_9f87f67f-0684-4c90-b610-42aab65cedcc", "Turned on Sway service"); } }
        //private static string I18nChangeSwayShareLevel { get { return I18NEntity.GetString("ReportCenter.Common_418beee8-9a91-4a6d-8359-248e441aaa8b", "Changed Sway share level"); } }
        #endregion
        #region User administration activities
        private static string I18nAddedUser { get { return I18NEntity.GetString("ReportCenter.Common_1107ccbe-2692-45a6-a553-0e0b773503a6", "Added user"); } }
        private static string I18nDeletedUser { get { return I18NEntity.GetString("ReportCenter.Common_fd4084b8-69ce-4bf7-ae6b-49ea6b052789", "Deleted user"); } }
        private static string I18nSetLicensProperties { get { return I18NEntity.GetString("ReportCenter.Common_4c1edfc2-2255-443e-b807-1d054ad3a253", "Set license properties"); } }
        private static string I18nResetUserPassword { get { return I18NEntity.GetString("ReportCenter.Common_ec6fc33a-7642-46db-b5ef-eefb6930c5fa", "Reset user password"); } }
        private static string I18nChangedUserPassword { get { return I18NEntity.GetString("ReportCenter.Common_33cba2ca-22fe-41b2-9ace-faed44bfca78", "Changed user password"); } }
        private static string I18nChangedUserLicense { get { return I18NEntity.GetString("ReportCenter.Common_283b0f79-4253-4102-99ba-04e734236a91", "Changed user license"); } }
        private static string I18nUpdatedUser { get { return I18NEntity.GetString("ReportCenter.Common_b3875434-e24d-429d-8393-bcad04fb98e6", "Updated user"); } }
        private static string I18nSetPropertyChangePassword { get { return I18NEntity.GetString("ReportCenter.Common_fa54a730-a7fe-4a15-86b7-efbad5b01d87", "Set property that forces user to change password"); } }
        //Group administration activities
        private static string I18nAddedGroup { get { return I18NEntity.GetString("ReportCenter.Common_bf3df49f-98bf-40c4-925f-2edffbd79280", "Added group"); } }
        private static string I18nUpdateGroup { get { return I18NEntity.GetString("ReportCenter.Common_4d797b5a-37bf-4f4f-925f-9d7be6f8a191", "Updated group"); } }
        private static string I18nDeleteGroup { get { return I18NEntity.GetString("ReportCenter.Common_bf751f48-6459-4a39-a12b-f6f1a397a428", "Deleted group"); } }
        private static string I18nAddedMemberGroup { get { return I18NEntity.GetString("ReportCenter.Common_f9b536e5-eb2b-4309-884e-c61553518e31", "Added member to group"); } }
        private static string I18nRemovedMemberGroup { get { return I18NEntity.GetString("ReportCenter.Common_3a006573-9b1d-4f1f-bd1c-bacc61dd5cd3", "Removed member from group"); } }
        //Application administration activities
        private static string I18nAddedServicePrincipal { get { return I18NEntity.GetString("ReportCenter.Common_f0bc2f9d-e7b6-42a4-987c-9c49dd51a4e4", "Added service principal"); } }
        private static string I18nRemoveServicePrincipal { get { return I18NEntity.GetString("ReportCenter.Common_62ba551c-c3a6-4706-a718-095482dad2cb", "Removed a service principal from the directory"); } }
        private static string I18nSetDelegationEntry { get { return I18NEntity.GetString("ReportCenter.Common_93f63af7-acab-4fbe-9e47-55519058d750", "Set delegation entry"); } }
        private static string I18nRemoveCredential { get { return I18NEntity.GetString("ReportCenter.Common_2cdac283-e1c6-43ca-8a63-a3b0b92d725c", "Removed credentials from a service principal"); } }
        private static string I18nAddedDelegationEntry { get { return I18NEntity.GetString("ReportCenter.Common_b6608188-d195-4eae-9633-b44c86fad799", "Added delegation entry"); } }
        private static string I18nAddedCredential { get { return I18NEntity.GetString("ReportCenter.Common_649b283c-9846-4ac4-ae73-c50500269c7f", "Added credentials to a service principal"); } }
        private static string I18nRemoveDelegationEntry { get { return I18NEntity.GetString("ReportCenter.Common_bc27400d-e9e3-4967-8b95-0730afc234f0", "Removed delegation entry"); } }
        //Role administration activities
        private static string I18nAddedMemberRole { get { return I18NEntity.GetString("ReportCenter.Common_23a11949-aaa1-471e-afed-1cc5268b0338", "Added member to Role"); } }
        private static string I18nRemoveUserFromRole { get { return I18NEntity.GetString("ReportCenter.Common_22dc28ce-13f3-4c92-ba31-81c23935c16a", "Removed a user from a directory role"); } }
        private static string I18nSetCompanyContact { get { return I18NEntity.GetString("ReportCenter.Common_9f8391bf-bd4b-4a12-9166-9f4a1e998a73", "Set Company contact information"); } }
        //Directory administation activities
        private static string I18nAddedPartner { get { return I18NEntity.GetString("ReportCenter.Common_266bd500-565c-4abf-a4a0-703f4211333d", "Added the partner to the directory"); } }
        private static string I18nRemovedPartner { get { return I18NEntity.GetString("ReportCenter.Common_342bc84f-6c13-4af9-a255-82f22647957b", "Removed a partner from the directory"); } }
        private static string I18nAddedDomain { get { return I18NEntity.GetString("ReportCenter.Common_7feafa7d-5a49-4ea0-8e4b-e90e1eacb236", "Added domain to company"); } }
        private static string I18nRemovedDomain { get { return I18NEntity.GetString("ReportCenter.Common_ac593440-9447-4ad0-999b-2fada1e1c4f8", "Removed domain from company"); } }
        private static string I18nUpdatedDomain { get { return I18NEntity.GetString("ReportCenter.Common_541daf9b-c86d-422f-8dd9-9b41909832af", "Updated domain"); } }
        private static string I18nSetDomainAuthentication { get { return I18NEntity.GetString("ReportCenter.Common_e3361ea7-82b0-4f05-b7c7-9d6f23a4abb6", "Set domain authentication"); } }
        private static string I18nVerifiedDomain { get { return I18NEntity.GetString("ReportCenter.Common_45fbd1e0-e325-4c19-af79-b723d6b176e1", "Verified domain"); } }
        private static string I18nUpdatedFederation { get { return I18NEntity.GetString("ReportCenter.Common_9232230e-ffbe-4404-82c0-677f1fa70d31", "Updated the federation settings for a domain"); } }
        private static string I18nVerifiedEmailDomain { get { return I18NEntity.GetString("ReportCenter.Common_daa8f9d9-53ff-4e8a-8f66-f6b8993f76e2", "Verified email verified domain"); } }
        private static string I18nTurnOnAzureADSync { get { return I18NEntity.GetString("ReportCenter.Common_c02ff782-9b92-4d04-85ba-474f355d5bfa", "Turned on Azure AD sync"); } }
        private static string I18nSetPasswordPolicy { get { return I18NEntity.GetString("ReportCenter.Common_791ab73b-bc23-4225-8474-563b85ba1931", "Set password policy"); } }
        private static string I18nSetCompanyInfo { get { return I18NEntity.GetString("ReportCenter.Common_47e51c50-ccbd-4a70-8e08-1051e935c291", "Set company information"); } }
        #endregion
        #region eDiscovery activities
        //private static string I18nCreatedContentSerach { get { return I18NEntity.GetString("ReportCenter.Common_9c9cc235-0e46-4447-8eed-f34e9ca8f909", "Created content search"); } }
        //private static string I18nDeletedContentSearch { get { return I18NEntity.GetString("ReportCenter.Common_31fad16e-359f-450e-8ffe-4133e7f9b256", "Deleted content search"); } }
        //private static string I18nChangedContentSerach { get { return I18NEntity.GetString("ReportCenter.Common_a19ef28a-70fb-479b-8e05-100bd256f20f", "Changed content search"); } }
        //private static string I18nStartedContentSearch { get { return I18NEntity.GetString("ReportCenter.Common_142a1fc6-5950-4768-bdbb-fd78f18dfff7", "Started content search"); } }
        //private static string I18nStoppedContentSearch { get { return I18NEntity.GetString("ReportCenter.Common_fe88fefc-be1c-4087-9f14-2047020085d8", "Stopped content search"); } }
        //private static string I18nCreatedContentSearchAction { get { return I18NEntity.GetString("ReportCenter.Common_3630b67f-faca-48c6-986c-e7d62d69ee28", "Created content search action"); } }
        //private static string I18nChangedContentSearchAction { get { return I18NEntity.GetString("ReportCenter.Common_b50981c2-87c1-446c-a9e6-b73a58634455", "Changed content search action"); } }
        //private static string I18nDeletedContentSearchAction { get { return I18NEntity.GetString("ReportCenter.Common_26c65e23-8580-47af-b952-f324e73d763a", "Deleted content search action"); } }
        //private static string I18nCreatedSearchPermissionsFilter { get { return I18NEntity.GetString("ReportCenter.Common_f624ecbb-b43f-468e-9fca-7ec3011ab40f", "Created search permission filter"); } }
        //private static string I18nDeletedSearchPermissionsFilter { get { return I18NEntity.GetString("ReportCenter.Common_782de95d-6a61-4d25-ad3b-85601eb91040", "Deleted search permission filter"); } }
        //private static string I18nChangedSearchPermissionsFilter { get { return I18NEntity.GetString("ReportCenter.Common_0f22c8a0-eb2b-456c-9f01-9c6675477cbf", "Changed search permission filter"); } }
        //private static string I18nCreatedHoldIneDiscoveryCase { get { return I18NEntity.GetString("ReportCenter.Common_429d7deb-bdfe-4ef1-bc85-04b891628653", "Created hold in eDiscovery case"); } }
        //private static string I18nDeletedHoldIneDiscoveryCase { get { return I18NEntity.GetString("ReportCenter.Common_c6332149-1983-4091-8d64-2ed35f10c4fc", "Deleted hold in eDiscovery case"); } }
        //private static string I18nChangedHoldIneDiscoveryCase { get { return I18NEntity.GetString("ReportCenter.Common_86295f83-1af4-40ab-a54f-b7c0065f56af", "Changed hold in eDiscovery case"); } }
        //private static string I18nCreatedSearchQueryeDiscovery { get { return I18NEntity.GetString("ReportCenter.Common_e6108c18-d1d6-49a7-8e7e-1abe1cf30a6f", "Created search query for eDiscovery"); } }
        //private static string I18nDeletedSearchQueryeDiscovery { get { return I18NEntity.GetString("ReportCenter.Common_44bef034-b959-40a5-a0ae-b45cc1be3b5a", "Deleted search query for eDiscovery"); } }
        //private static string I18nChangedSearchQueryeDiscovery { get { return I18NEntity.GetString("ReportCenter.Common_92ab920d-1845-495f-9459-f90d9581501a", "Changed search query for eDiscovery"); } }
        //private static string I18nCreatedeDiscoveryCase { get { return I18NEntity.GetString("ReportCenter.Common_419212ed-d9f0-4a77-9ddc-34e99fd334b6", "Created eDiscovery case"); } }
        //private static string I18nDeletedeDiscoveryCase { get { return I18NEntity.GetString("ReportCenter.Common_5a49f279-22c4-46ca-93cb-8f2cc4be7f2c", "Deleted eDiscovery case"); } }
        //private static string I18nChangedeDiscoveryCase { get { return I18NEntity.GetString("ReportCenter.Common_8501dd4e-0fc5-40c7-a171-2374ca0f7c99", "Changed eDiscovery case"); } }
        //private static string I18nAddedMemberToeDiscoveryCase { get { return I18NEntity.GetString("ReportCenter.Common_4acc0b59-5bc6-4dc0-9139-5e220cc1956a", "Added member to eDiscovery case"); } }
        //private static string I18nRemoveMemberFromeDiscovery { get { return I18NEntity.GetString("ReportCenter.Common_61f5553b-a006-47ff-b4f6-5fd54c309d4d", "Remove member from eDiscovery case"); } }
        //private static string I18nChangedeDiscoveryCaseMembership { get { return I18NEntity.GetString("ReportCenter.Common_ba37131b-d0ff-4b6a-8c78-c21074adacf0", "Changed eDiscory case membership"); } }
        //private static string I18nCreatedeDiscoveryAdministrator { get { return I18NEntity.GetString("ReportCenter.Common_ede61b6d-0444-4931-bac1-d9f9841502ce", "Created eDiscovery administrator"); } }
        //private static string I18nDeletedeDiscoveryAdministrator { get { return I18NEntity.GetString("ReportCenter.Common_01ca7997-18eb-4166-b730-afae29f5f424", "Deleted eDiscovery administrator"); } }
        //private static string I18nChangedeDiscoveryAdministrator { get { return I18NEntity.GetString("ReportCenter.Common_40aecd98-ba46-4e24-8be7-29ee51f813b6", "Changed eDiscovery administrator"); } }
        //PowerBI activities
        //private static string I18nViewPowerBIDashboard { get { return I18NEntity.GetString("ReportCenter.Common_32fabd2d-1d59-4071-8d35-0c0875584f48", "View PowerBI dashboard"); } }
        //private static string I18nCreatedPowerBIDashboard { get { return I18NEntity.GetString("ReportCenter.Common_bcd050b2-ab08-4995-9929-b71a4d83635f", "Created PowerBI dash board"); } }
        //private static string I18nEditedPowerBIDashboard { get { return I18NEntity.GetString("ReportCenter.Common_f2882057-5c6f-4ba4-a5d6-b5ffe8aab1bb", "Edited PowerBI dashboard"); } }
        //private static string I18nDeletedPowerBIDashboard { get { return I18NEntity.GetString("ReportCenter.Common_0e7066ff-7000-4420-b2ce-67cfd58d726c", "Deleted PowerBI dashboard"); } }
        //private static string I18nSharedPowerBIDashboard { get { return I18NEntity.GetString("ReportCenter.Common_7f932c25-7db5-4692-b52f-cd2d82c3c6b4", "Shared PowerBI dashboard"); } }
        //private static string I18nDeletedPowerBIReport { get { return I18NEntity.GetString("ReportCenter.Common_1b0b6961-4cb1-4af5-8806-1247411a630c", "Deleted PowerBI report"); } }
        //private static string I18nDeletedPowerBIDatasets { get { return I18NEntity.GetString("ReportCenter.Common_fdbc5e28-9ec8-493a-82d6-4272206ddbaa", "Deleted PowerBI datasets"); } }
        //private static string I18nCreatedPowerBIGroup { get { return I18NEntity.GetString("ReportCenter.Common_450aede1-560f-49e8-9863-ac83430e3d5c", "Created PowerBI group"); } }
        //private static string I18nAddedPowerBIGroupMember { get { return I18NEntity.GetString("ReportCenter.Common_c2cdc744-db32-4319-9c2f-312bb7f803b0", "Added PowerBI group member"); } }
        //private static string I18nCreatedOrgnizationPowerBI { get { return I18NEntity.GetString("ReportCenter.Common_2d805bbe-90bd-4ee3-bdbf-359b2d4bd192", "Created orgnization PowerBI content pack"); } }
        #endregion
        #region other 
        private static string I18nPasswordLogonInitialAuthUsingPassword { get { return I18NEntity.GetString("ReportCenter.Common_174c8bc5-72a0-4ad2-8658-c8b1b2d8fe9f", "PasswordLogonInitialAuthUsingPassword"); } }
        private static string I18nUserLoggedIn { get { return I18NEntity.GetString("ReportCenter.Common_6290817f-2623-40cd-870d-8a4c1a7660aa", "UserLoggedIn"); } }
        private static string I18nPasswordLogonCookieCopyUsingDAToken { get { return I18NEntity.GetString("ReportCenter.Common_5aff959d-cec7-49ab-9507-e7e3a6d60081", "PasswordLogonCookieCopyUsingDAToken"); } }
        private static string I18nSiteCollectionAdminRemoved { get { return I18NEntity.GetString("ReportCenter.Common_46415c45-c169-4fb8-8690-42dc2306f068", "SiteCollectionAdminRemoved"); } }
        private static string I18nFilePreviewed { get { return I18NEntity.GetString("ReportCenter.Common_e2692e96-a3a1-4c58-81e8-1e6eaad17e4a", "FilePreviewed"); } }
        private static string I18nConsentToApplication { get { return I18NEntity.GetString("ReportCenter.Common_df4d2cfa-77ce-4cc8-8f2c-adb887681b30", "Consent to application."); } }
        private static string I18nAddOAuth2PermissionGrant { get { return I18NEntity.GetString("ReportCenter.Common_4a5bbe4f-09d2-4639-99e2-b4ea3c54c484", "Add OAuth2PermissionGrant."); } }
        private static string I18nAddAppRoleAssignmentGrantToUser { get { return I18NEntity.GetString("ReportCenter.Common_0d3f786f-2a9f-40b0-9703-2892d46b8964", "Add app role assignment grant to user."); } }
        private static string I18nRemovedFromSiteCollection { get { return I18NEntity.GetString("ReportCenter.Common_a588f669-a1d0-4807-b75b-c189dd5501cb", "RemovedFromSiteCollection"); } }
        private static string I18nCreateCompany { get { return I18NEntity.GetString("ReportCenter.Common_ae6bcf42-b0eb-47ca-97be-9aa612bfc540", "Create company"); } }
        private static string I18nEnableAddressListPaging { get { return I18NEntity.GetString("ReportCenter.Common_658a1899-31f1-45ef-b9b8-90dde1363694", "Enable-AddressListPaging"); } }
        private static string I18nSetTransportConfig { get { return I18NEntity.GetString("ReportCenter.Common_6e5713b8-c447-4f78-8dbb-e614fc27d9d5", "Set-TransportConfig"); } }
        private static string I18nSetMailbox { get { return I18NEntity.GetString("ReportCenter.Common_a118e715-5407-40f8-b77f-eb2d6197346b", "Set-Mailbox"); } }
        private static string I18nSetOwaMailboxPolicy { get { return I18NEntity.GetString("ReportCenter.Common_adcdeba8-910d-4f04-82b1-30f14009b73a", "Set-OwaMailboxPolicy"); } }

        private static string I18nSetTenantObjectVersion { get { return I18NEntity.GetString("ReportCenter.Common_bd12da85-b18e-40f5-9bcc-3035f6ccd34e", "Set-TenantObjectVersion"); } }
        private static string I18nDeleteApplicationPassword { get { return I18NEntity.GetString("ReportCenter.Common_4441ebff-0262-4969-a5b9-93b08d422624", "Delete application password for user."); } }
        private static string I18nCreateApplicationPassword { get { return I18NEntity.GetString("ReportCenter.Common_54a6438a-e8f2-4018-9610-58aff9a5f752", "Create application password for user."); } }
        private static string I18nFolderDeleted { get { return I18NEntity.GetString("ReportCenter.Common_2c52eaff-4443-4e15-b59f-18c21ad633de", "Folder Deleted"); } }
        private static string I18nFolderModified { get { return I18NEntity.GetString("ReportCenter.Common_5e0c9737-d15d-4e7d-8104-d9d6b509d07a", "FolderModified"); } }
        private static string I18nNewExchangeAssistanceConfig { get { return I18NEntity.GetString("ReportCenter.Common_78c02f89-9543-468e-9fd3-0d7f19702931", "New-ExchangeAssistanceConfig"); } }
        private static string I18nInstallDefaultSharingPolicy { get { return I18NEntity.GetString("ReportCenter.Common_27128e67-e425-42c7-8b4f-ecde2399e452", "Install-DefaultSharingPolicy"); } }
        private static string I18nInstallAdminAuditLogConfig { get { return I18NEntity.GetString("ReportCenter.Common_ff5b5b16-e8f8-41a0-8499-4393b6c9aa7c", "Install-AdminAuditLogConfig"); } }
        private static string I18nInstallDataClassificationConfig { get { return I18NEntity.GetString("ReportCenter.Common_39997ad6-50b2-4cc8-a0bc-23e44ae47719", "Install-DataClassificationConfig"); } }
        private static string I18nInstallResourceConfig { get { return I18NEntity.GetString("ReportCenter.Common_dc6b9f9f-3cb1-46a3-a4bf-76fba3d9fc12", "Install-ResourceConfig"); } }
        private static string I18nSetRecipientEnforcementProvisioningPolicy { get { return I18NEntity.GetString("ReportCenter.Common_44562f61-91bd-4d69-97b8-ac3f56dc7c9f", "Set-RecipientEnforcementProvisioningPolicy"); } }
        private static string I18nSetExchangeAssistanceConfig { get { return I18NEntity.GetString("ReportCenter.Common_a4c7856b-4b22-4b36-83e8-1e1ce9789064", "Set-ExchangeAssistanceConfig"); } }
        private static string I18nNewDkimSigningConfig { get { return I18NEntity.GetString("ReportCenter.Common_2e6d0ab3-7eb0-4514-a2b3-409fa96edf44", "New-DkimSigningConfig"); } }
        private static string I18nSetAdminAuditLogConfig { get { return I18NEntity.GetString("ReportCenter.Common_f693aae8-8af8-41f1-b306-7d07ecbcf812", "Set-AdminAuditLogConfig"); } }
        private static string I18nNewMailbox { get { return I18NEntity.GetString("ReportCenter.Common_39b220d6-78a0-42ad-adb2-bc71600c44a0", "New-Mailbox"); } }
        private static string I18nFileDeletedFirstStageRecycleBin { get { return I18NEntity.GetString("ReportCenter.Common_48ffafea-5585-42e1-9caf-812ebfd7b6d9", "FileDeletedFirstStageRecycleBin"); } }
        private static string I18nFileDeletedSecondStageRecycleBin { get { return I18NEntity.GetString("ReportCenter.Common_0c37d059-3e39-4494-9a7c-8c56702b34c5", "FileDeletedSecondStageRecycleBin"); } }
        #endregion
        #region team
        private static string I18nBotAddedToTeam { get { return I18NEntity.GetString("ReportCenter.Common_761041e7-730d-49db-a88a-6c5b625a118b", "Added bot to team"); } }
        private static string I18nBotRemovedFromTeam { get { return I18NEntity.GetString("ReportCenter.Common_fa03cf77-b2bc-4ac9-b01c-19026d052b01", "Removed bot from team"); } }
        private static string I18nChannelAddedForTeam { get { return I18NEntity.GetString("ReportCenter.Common_7d4ce388-4edf-445e-938d-41e286a179f5", "Added channel to team"); } }
        private static string I18nChannelDeletedForTeam { get { return I18NEntity.GetString("ReportCenter.Common_ea382b6c-beda-4932-942c-aac09c734345", "Deleted channel"); } }
        private static string I18nChannelSettingChanged { get { return I18NEntity.GetString("ReportCenter.Common_a632e197-e34d-40e4-8965-92eb0c860c5d", "Changed channel setting"); } }
        private static string I18nConnectorAddedForTeam { get { return I18NEntity.GetString("ReportCenter.Common_092f0aec-489e-471d-b393-92ea8c51e521", "Added connector to channel"); } }
        private static string I18nConnectorRemovedForTeam { get { return I18NEntity.GetString("ReportCenter.Common_c90f0c47-fa21-4167-b050-a8b25613457c", "Removed connector from channel"); } }
        private static string I18nConnectorUpdated { get { return I18NEntity.GetString("ReportCenter.Common_9229069d-c5ec-449a-afdf-8cdb1510ce48", "Updated connector"); } }
        private static string I18nMemberAddedForTeam { get { return I18NEntity.GetString("ReportCenter.Common_c58a33fb-ed91-4c5d-8dcf-bf46fd810eff", "Added member to team"); } }
        private static string I18nMemberRemovedForTeam { get { return I18NEntity.GetString("ReportCenter.Common_df5d34c7-eddc-4930-a8fa-36b2f15d6d2c", "Removed member from team"); } }
        private static string I18nMemberRoleChanged { get { return I18NEntity.GetString("ReportCenter.Common_81ca68a0-cc80-485e-9799-9ec3ca1fa41f", "Changed member role"); } }
        private static string I18nTabAdded { get { return I18NEntity.GetString("ReportCenter.Common_e041e007-1e86-4859-82aa-39091c064ae8", "Added tab to channel"); } }
        private static string I18nTabRemoved { get { return I18NEntity.GetString("ReportCenter.Common_894f2975-1629-4f40-bc44-b503f68a7802", "Removed tab from channel"); } }
        private static string I18nTabUpdated { get { return I18NEntity.GetString("ReportCenter.Common_c8ac2943-2dc1-4f77-97d8-b187f4ceb42d", "Updated tab"); } }
        private static string I18nTeamCreatedForTeam { get { return I18NEntity.GetString("ReportCenter.Common_21a459f0-0d32-4a56-a6bb-85a61cceca06", "Created team"); } }
        private static string I18nTeamDeletedForTeam { get { return I18NEntity.GetString("ReportCenter.Common_ca977a67-cb69-46c0-b689-25f3780e9be2", "Deleted team"); } }
        private static string I18nTeamSettingChangedForTeam { get { return I18NEntity.GetString("ReportCenter.Common_c741c2bb-e44c-42f3-80c1-bbfc12265f8b", "Changed team setting"); } }
        private static string I18nTeamsSessionStartedForTeam { get { return I18NEntity.GetString("ReportCenter.Common_96756523-63f8-454b-b13d-ed4de4418d6c", "User signed in to Teams"); } }
        private static string I18nTeamsTenantSettingChanged { get { return I18NEntity.GetString("ReportCenter.Common_0d2564fd-4136-4ab4-8e81-99f6b9df0be1", "Changed team tenant setting"); } }
        #endregion
        #region form 
        private static string I18nCreateComment { get { return I18NEntity.GetString("ReportCenter.Common_6b55faae-0ac4-4a07-84e4-0b6dbf208648", "Created comment"); } }
        private static string I18nCreateForm { get { return I18NEntity.GetString("ReportCenter.Common_f6ed8a0a-57f3-4618-99b3-f27c490ab886", "Created form"); } }
        private static string I18nEditForm { get { return I18NEntity.GetString("ReportCenter.Common_ce7e046e-e081-4bf0-a43f-06fe5b195674", "Edited form"); } }
        private static string I18nMoveForm { get { return I18NEntity.GetString("ReportCenter.Common_fcc9b5eb-acc8-4e2e-909a-6c0ab0a42449", "Moved form"); } }
        private static string I18nDeleteForm { get { return I18NEntity.GetString("ReportCenter.Common_4c2a0966-b656-4e41-bd83-60d46f6422a5", "Deleted form"); } }
        private static string I18nViewForm { get { return I18NEntity.GetString("ReportCenter.Common_16787a16-b3de-42eb-8f88-a5539fe7ee9c", "Viewed form"); } }
        private static string I18nPreviewForm { get { return I18NEntity.GetString("ReportCenter.Common_4d14fca9-acb1-4c65-9a5d-397bc01cebd6", "Previewed form"); } }
        private static string I18nExportForm { get { return I18NEntity.GetString("ReportCenter.Common_50b6dfa9-e420-4549-8295-849b9b886aea", "Exported form"); } }
        private static string I18nAllowShareFormForCopy { get { return I18NEntity.GetString("ReportCenter.Common_727b4f24-1b6c-4b02-8d03-b788d5139aa6", "Allowed share form for copy"); } }
        private static string I18nDisallowShareFormForCopy { get { return I18NEntity.GetString("ReportCenter.Common_5bb000d4-d6f6-44e0-a263-2756812a7f66", "Disallowed share form for copy"); } }
        private static string I18nAddFormCoauthor { get { return I18NEntity.GetString("ReportCenter.Common_0e174e91-8471-4b4a-8ff4-eab53fa96e6d", "Added form co-author"); } }
        private static string I18nRemoveFormCoauthor { get { return I18NEntity.GetString("ReportCenter.Common_9aed1869-7511-416f-b02c-07886b2be5d3", "Removed form co-author"); } }
        private static string I18nViewRuntimeForm { get { return I18NEntity.GetString("ReportCenter.Common_dada67e2-de0f-4905-9a8e-7dad6d13665c", "Viewed response page"); } }
        private static string I18nCreateResponse { get { return I18NEntity.GetString("ReportCenter.Common_2be95f46-cd16-4ab0-b8af-09b341ea9907", "Created response"); } }
        private static string I18nUpdateResponse { get { return I18NEntity.GetString("ReportCenter.Common_d0699162-55b3-4fc6-a3c0-0892de089bad", "Updated response"); } }
        private static string I18nDeleteAllResponses { get { return I18NEntity.GetString("ReportCenter.Common_04d65848-c451-4813-ab2d-4b28717e58b1", "Deleted all responses"); } }
        private static string I18nDeleteResponse { get { return I18NEntity.GetString("ReportCenter.Common_6f9e1ac9-90f4-4d0b-9d9e-d3084210882e", "Deleted Response"); } }
        private static string I18nViewResponses { get { return I18NEntity.GetString("ReportCenter.Common_3ae15dcb-6607-429c-ad17-2eda712dc71c", "Viewed responses"); } }
        private static string I18nViewResponse { get { return I18NEntity.GetString("ReportCenter.Common_923c941a-ef50-49b0-aed8-555115a2017b", "Viewed response"); } }
        private static string I18nGetSummaryLink { get { return I18NEntity.GetString("ReportCenter.Common_2a96de75-28e3-4cb7-ab73-f0c425eeb2c1", "Created summary link"); } }
        private static string I18nDeleteSummaryLink { get { return I18NEntity.GetString("ReportCenter.Common_7f85b4aa-ded9-4824-8ba9-5335542cfa19", "Deleted summary link"); } }
        private static string I18nUpdatePhishingStatus { get { return I18NEntity.GetString("ReportCenter.Common_98927537-fa58-4e1c-9235-21fd93733a3a", "Updated form phishing status"); } }
        private static string I18nProInvitation { get { return I18NEntity.GetString("ReportCenter.Common_171ccca4-3a34-4e93-9ccf-2d87a59d653a", "Sent Forms Pro invitation"); } }
        private static string I18nUpdateFormSetting { get { return I18NEntity.GetString("ReportCenter.Common_5173e733-f7ab-4cdb-9df8-f629501738ae", "Updated form setting"); } }
        private static string I18nUpdateUserSetting { get { return I18NEntity.GetString("ReportCenter.Common_b4276425-89d8-48d7-8074-bc2f5440fb12", "Updated user setting"); } }
        private static string I18nListForms { get { return I18NEntity.GetString("ReportCenter.Common_77d32e9c-e1ec-4b0e-a7b4-8a30e43d4821", "Listed forms"); } }
        private static string I18nSubmitResponse { get { return I18NEntity.GetString("ReportCenter.Common_f4568574-4a10-4853-8d3a-e1b92ab1f2da", "Submitted response"); } }
        #endregion
        #region Exchange mailbox activities
        private static string I18nMailItemsAccessed { get { return I18NEntity.GetString("ReportCenter.Common_5c2c37ed-2a55-69da-a3c4-a3bdf390e43a", "Accessed mailbox items"); } }
        private static string I18nAddMailboxPermissions { get { return I18NEntity.GetString("ReportCenter.Common_87afa6e4-2794-b1e7-692e-8f931ed62cd9", "Added delegate mailbox permissions"); } }
        private static string I18nUpdateCalendarDelegation { get { return I18NEntity.GetString("ReportCenter.Common_1e45e1d1-49a9-fa93-deea-a73f055303ab", "Added or removed user with delegate access to calendar folder"); } }
        private static string I18nAddFolderPermissions { get { return I18NEntity.GetString("ReportCenter.Common_030543a1-50fa-e826-0589-8d62a8beede0", "Added permissions to folder"); } }
        private static string I18nMailboxCopy { get { return I18NEntity.GetString("ReportCenter.Common_3d061cee-c4ca-e5d6-dc16-4f2c44f12f9e", "Copied messages to another folder"); } }
        private static string I18nMailboxCreate { get { return I18NEntity.GetString("ReportCenter.Common_9e09d2ec-5520-1522-898e-eba44550a42c", "Created mailbox item"); } }
        private static string I18nNewInboxRule { get { return I18NEntity.GetString("ReportCenter.Common_762f5e83-f09d-1405-e8a3-385c5a2536c4", "Created new inbox rule in Outlook web app"); } }
        private static string I18nSoftDelete { get { return I18NEntity.GetString("ReportCenter.Common_7f8e603c-968f-46f8-539e-43fdf42fe590", "Deleted messages from Deleted Items folder"); } }
        private static string I18nApplyRecordLabel { get { return I18NEntity.GetString("ReportCenter.Common_ab4f197b-857c-7cf0-fc7a-bc99ed8cc4b5", "Labeled message as a record"); } }
        private static string I18nMailboxMove { get { return I18NEntity.GetString("ReportCenter.Common_1eff704d-fe01-bb8c-8c4f-19a01443aae8", "Moved messages to another folder"); } }
        private static string I18nMoveToDeletedItems { get { return I18NEntity.GetString("ReportCenter.Common_1004c56f-b77d-54dd-d122-384e58264d18", "Moved messages to Deleted Items folder"); } }
        private static string I18nUpdateFolderPermissions { get { return I18NEntity.GetString("ReportCenter.Common_7399a8f9-23f5-c626-1b49-ffead344c96c", "Modified folder permission"); } }
        private static string I18nSetInboxRule { get { return I18NEntity.GetString("ReportCenter.Common_a3212d4f-342c-fcdc-ad26-41ec30667633", "Modified inbox rule from Outlook web app"); } }
        private static string I18nHardDelete { get { return I18NEntity.GetString("ReportCenter.Common_287fe8ad-48de-2f34-6967-bd2ef8b3b25b", "Purged messages from the mailbox"); } }
        private static string I18nRemoveMailboxPermission { get { return I18NEntity.GetString("ReportCenter.Common_974f30c8-7476-7e06-b3f2-81e0120279ab", "Removed delegate mailbox permissions"); } }
        private static string I18nRemoveFolderPermissions { get { return I18NEntity.GetString("ReportCenter.Common_8299c5bf-ac0b-809e-cac1-d89618be3c6b", "Removed permissions from folder"); } }
        private static string I18nMailboxSend { get { return I18NEntity.GetString("ReportCenter.Common_dde96367-9d62-fb17-ff86-7923c7a5b38f", "Sent message"); } }
        private static string I18nMailboxSendAs { get { return I18NEntity.GetString("ReportCenter.Common_dde7899f-9589-24bb-ce05-c3266ac7a0f9", "Sent message using Send As"); } }
        private static string I18nSendOnBehalf { get { return I18NEntity.GetString("ReportCenter.Common_6156297b-f5f9-6252-f881-2f0685600ca4", "Sent message using Send On Behalf permissions"); } }
        private static string I18nUpdateInboxRules { get { return I18NEntity.GetString("ReportCenter.Common_7faf81aa-b559-1b50-b542-a78aa97872ab", "Updated inbox rules from Outlook client"); } }
        private static string I18nMailboxUpdate { get { return I18NEntity.GetString("ReportCenter.Common_ed711163-82db-2e98-4361-76e16b00270b", "Updated message"); } }
        private static string I18nMailboxLogin { get { return I18NEntity.GetString("ReportCenter.Common_a609f453-43ad-7af5-7ce2-8ba566d5926c", "User signed in to mailbox"); } }
        private static string I18nModifyFolderPermissions { get { return I18NEntity.GetString("ReportCenter.Common_3717a766-0017-63b1-a672-b879bdb64e7d", "Modified permissions of folder"); } }
        #endregion
        #region stream activities
        private static string I18nStreamInvokeVideoView { get { return I18NEntity.GetString("ReportCenter.Common_a5dad8ae-57f5-782b-f3ed-b43f672ff8b7", "Viewed video"); } }
        private static string I18nStreamEditVideoPermissions { get { return I18NEntity.GetString("ReportCenter.Common_07a9566b-a1c4-9812-c1de-984de345e51f", "Edited video permission"); } }
        private static string I18nStreamInvokeVideoUpload { get { return I18NEntity.GetString("ReportCenter.Common_5ecbfb5c-3b61-8d84-bbcb-bd2d7d54584a", "Uploaded video"); } }
        private static string I18nStreamEditVideo { get { return I18NEntity.GetString("ReportCenter.Common_cd01db05-8af7-90ca-1be1-b22debde9709", "Edited video"); } }
        private static string I18nStreamInvokeVideoSetLink { get { return I18NEntity.GetString("ReportCenter.Common_cadeb891-a4a9-abdd-1a0b-c1677d19db61", "Linked on Video"); } }
        private static string I18nStreamCreateVideo { get { return I18NEntity.GetString("ReportCenter.Common_4036d0ed-6a5b-03cb-1ab8-8093378cc5a8", "Created video"); } }
        private static string I18nStreamInvokeVideoDownload { get { return I18NEntity.GetString("ReportCenter.Common_363bdc99-6b7d-ae55-7117-0c7d1ea82fc5", "Downloaded video"); } }
        private static string I18nStreamInvokeVideoShare { get { return I18NEntity.GetString("ReportCenter.Common_45aff3fd-b36b-956e-a029-477e3cbffa43", "Shared video"); } }
        private static string I18nStreamCreateChannel { get { return I18NEntity.GetString("ReportCenter.Common_987714a4-a0b0-222f-1aa7-26d7ec461c94", "Created channel"); } }
        private static string I18nStreamEditChannel { get { return I18NEntity.GetString("ReportCenter.Common_ab6a8bfb-66d8-6d26-290f-d746191430bd", "Edited channel"); } }
        private static string I18nStreamDeleteVideo { get { return I18NEntity.GetString("ReportCenter.Common_1f54ca6c-5c7c-35da-48c5-a838acefd448", "Deleted video"); } }
        private static string I18nStreamInvokeVideoLike { get { return I18NEntity.GetString("ReportCenter.Common_c30ec56d-b5c2-4fed-b62b-5be984548ee8", "Liked video"); } }
        private static string I18nStreamCreateGroup { get { return I18NEntity.GetString("ReportCenter.Common_aaa9b438-b938-b4f5-3789-abb7b6a9e221", "Created group"); } }
        private static string I18nStreamDeleteVideoComment { get { return I18NEntity.GetString("ReportCenter.Common_295b33d1-b523-63c9-f06e-69df9ccdbc4d", "Deleted video comment"); } }
        private static string I18nStreamEditGroup { get { return I18NEntity.GetString("ReportCenter.Common_33d495f7-5402-1faa-1c94-dfc8a570836c", "Edited group"); } }
        private static string I18nStreamCreateVideoComment { get { return I18NEntity.GetString("ReportCenter.Common_ef0f267e-a9ee-4df6-0978-2fb68503fa9b", "Commented on video"); } }
        private static string I18nStreamInvokeVideoUnLike { get { return I18NEntity.GetString("ReportCenter.Common_0557bc55-136b-e2a2-4af1-b6c22206b640", "Unliked video"); } }
        private static string I18nStreamInvokeChannelSetThumbnail { get { return I18NEntity.GetString("ReportCenter.Common_344ec14f-7a26-26c7-de8e-6b5ae043e3b0", "Set channel thumbnail"); } }
        private static string I18nStreamEditUserSettings { get { return I18NEntity.GetString("ReportCenter.Common_701f8423-b3c2-0d46-96ee-1c446214de7d", "Edit user settings"); } }
        #endregion
        #endregion
    }
}
