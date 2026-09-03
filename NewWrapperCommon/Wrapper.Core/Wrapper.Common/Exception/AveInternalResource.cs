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
    public enum AveInternalResourceKey
    {
        #region QueryService
        #region Common
        Wrapper_Exception_QueryService_NotGenerateRBSStub,
        Wrapper_Exception_QueryService_CreateArchivePoolFailed,
        Wrapper_Exception_QueryService_ErrorCode,
        #endregion

        #region 10
        Wrapper_Exception_QueryService_CRCMatchFailed,
        Wrapper_Exception_QueryService_SONotRBSOrEBSData,
        Wrapper_Exception_QueryService_SONotFindItem,
        Wrapper_Exception_QueryService_SONotGenerateBlobRecord,
        Wrapper_Exception_QueryService_SONotFindRBSBlobId,
        Wrapper_Exception_QueryService_NotStubException,
        Wrapper_Exception_QueryService_NotFindMetaInfoInRecycleBin,
        Wrapper_Exception_QueryService_NotFindContentTypeIdInRecycleBin,
        Wrapper_Exception_QueryService_GenerateBlobRecordError,
        #endregion

        #endregion

        #region Workflow
        Wrapper_Exception_Workflow_EndOfFile,
        Wrapper_Exception_Workflow_NotFindParentFolder,
        Wrapper_Exception_Workflow_NotFindConfigFile,
        Wrapper_Exception_Workflow_NotFindWFDefinition,
        Wrapper_Exception_Workflow_NotFindListOrFolder,
        Wrapper_Exception_Workflow_ServiceNotAvailable,
        Wrapper_Exception_Workflow_CustomWorkflowNotSupported,
        Wrapper_Exception_Workflow_DependencyFeatureNotActivated,
        Wrapper_Exception_Workflow_NotBuildinAssociationSkipException,
        Wrapper_Exception_Workflow_AssociationConflictWithInstanceExist,
        Wrapper_Exception_Workflow_TemplateTypeNotSupportIn365,
        Wrapper_Exception_Workflow_RestoreWorkflowInstanceError,
        Wrapper_Exception_Workflow_DefinitionNotFound,
        #endregion

        #region Restore
        Wrapper_Exception_Restore_NotEnableAttachment,
        Wrapper_Exception_Restore_VerifyFilePageLayoutFailed,
        Wrapper_Exception_Restore_VerifyItemMetadataValueNotFound,
        Wrapper_Exception_Restore_DifferentBaseTypeList,
        Wrapper_Exception_Restore_NotFindSpecificField,
        Wrapper_Exception_Restore_NoRowIdForItem,
        Wrapper_Exception_Restore_NotFindCurrentUser,
        Wrapper_Exception_Restore_NotGetContentDatabaseById,
        Wrapper_Exception_Restore_ReplaceInvalidUserFailed,
        Wrapper_Exception_Restore_NotHavePermissionToCreateSiteCollection,
        Wrapper_Exception_Restore_NotFindWebApplication,
        Wrapper_Exception_Restore_FieldNotExist,
        Wrapper_Exception_Restore_ItemHasForcedUniqueField,
        Wrapper_Exception_Restore_CanNotGetRequestUserInSourceSite,
        Wrapper_Exception_Restore_RestoreAddDataFailedForNotRestoredCorrectly,
        Wrapper_Exception_Restore_RestoreAddDataFailedForInstallAppFailed,
        Wrapper_Exception_Restore_RestoreAddDataFailedForCheckAppWebUrl,
        Wrapper_Exception_Restore_Office365Environmental,
        Wrapper_Exception_Restore_SkipRestoreAppData,
        Wrapper_Exception_Restore_WFDeConflictError,
        Wrapper_Exception_Restore_WFDeNameOrTemplateConflictError,
        Wrapper_Exception_Restore_HostHeaderSiteCollectionAlreadyExists,
        Wrapper_Exception_Restore_CannotCreateSiteCollectionForUserPermission,
        Wrapper_Exception_Restore_SetLanguageResourcePath,
        Wrapper_Exception_Restore_CompatibilityLevelConflictError,
        Wrapper_Exception_Restore_CreateTaxonomyGroupError,
        Wrapper_Exception_Restore_CreateTermError,
        Wrapper_Exception_Restore_CreateTermSetError,
        Wrapper_Exception_Restore_NotSiteCollection,
        Wrapper_Exception_Restore_SkipRecycleBinConflict,
        Wrapper_Exception_Restore_ManagedPathNotFound,
        Wrapper_Exception_Restore_SingleDocumentVersionNotSupported,
        Wrapper_Exception_Restore_TermStoreNotFound,
        Wrapper_Exception_Restore_TermGroupNotFound,
        Wrapper_Exception_Restore_TermSetNotFound,
        Wrapper_Exception_Restore_TermNotFound,
        Wrapper_Exception_Restore_SharePointVersionNotSupportAudit,
        Wrapper_Exception_Restore_SkipRestoreDocumentWithInvalidUserData,
        Wrapper_Exception_Restore_ItemTypeConflict,
        Wrapper_Exception_Restore_DocumentTypeConflict,
        Wrapper_Exception_Restore_ContentTypeFaild,
        Wrapper_Exception_Restore_SkipListWhenDenyAddAndCustomizePagesStatus,
        #endregion

        #region Server
        Wrapper_Exception_Server_NotFindSetupPathForGhostedPage,
        Wrapper_Exception_Server_ConfigurationFileIllegal,
        Wrapper_Exception_Server_AttachmentUrlIncorrect,
        Wrapper_Exception_Server_FileSizeTooLarge,
        Wrapper_Exception_Server_NotFindWebApplication,
        Wrapper_Exception_Server_NoVersionId,
        Wrapper_Exception_Server_NotFindGroupWithId,
        Wrapper_Exception_Server_ExportWebPartError,
        Wrapper_Exception_Server_NotFindTermwithId,
        Wrapper_Exception_Server_FileNotFoundException,

        Wrapper_Exception_Server07_FolderOccupyRowIdWithFolderUrl,
        Wrapper_Exception_Server07_FolderOccupyRowIdWithoutFolderUrl,

        Wrapper_Exception_Server13_InstallAppFailed,
        Wrapper_Exception_Server13_FaileRestoreHistoricalVersions,
        Wrapper_Exception_Server13_NotCreateAppCatalog,
        Wrapper_Exception_Server13_NotGetAppLicenseForUser,
        Wrapper_Exception_Server13_UploadEmptyAttachmentError,

        Wrapper_Exception_Server16_FaileRestoreHistoricalVersions,


        #endregion

        #region Mapping
        Wrapper_Exception_Mapping_PathTooLongException,
        Wrapper_Exception_Mapping_FailedToCreateExcelFile,
        Wrapper_Exception_Mapping_DirectoryNotFound,
        Wrapper_Exception_Mapping_LookupFieldNotFound,

        #endregion

        #region Contract
        Wrapper_Exception_Contract_ContentTypeConflict,
        Wrapper_Exception_Contract_ConnotFindSchemaDependency,
        #endregion

        #region Backup
        Wrapper_Exception_Backup_GetDocumentContentError,
        Wrapper_Exception_Backup_NotFindSiteCollection,
        Wrapper_Exception_Backup_BlockSite,
        Wrapper_Exception_Backup_SkipBackupDocumentWithInvalidUserData,
        #endregion

        #region Discovery
        Wrapper_Exception_Discovery_AWDGetSubFoldersError,
        Wrapper_Exception_Discovery_AWDGetItemsError,
        Wrapper_Exception_Discovery_AWDGetAttachmentsError,
        Wrapper_Exception_Discovery_AWDGetAttachmentsForPRError,
        Wrapper_Exception_Discovery_AWDGetSecuritiesError,
        Wrapper_Exception_Discovery_AWDGetStubItemsError,
        Wrapper_Exception_Discovery_AWDGetStubContentsError,
        Wrapper_Exception_Discovery_AWDGetListRoorFolderError,
        Wrapper_Exception_Discovery_AWDGetViewsError,
        Wrapper_Exception_Discovery_AWDGetCTsError,
        Wrapper_Exception_Discovery_AWDGetAlertsError,
        Wrapper_Exception_Discovery_AWDGetListError,
        Wrapper_Exception_Discovery_AWDGetChangeSizeError,
        Wrapper_Exception_Discovery_AWDGetListSizeError,
        Wrapper_Exception_Discovery_AWDGetFolderSizeError,
        Wrapper_Exception_Discovery_AWDInitListPropertyError,
        Wrapper_Exception_Discovery_AWDGetWebError,
        Wrapper_Exception_Discovery_AWDGetRootWebError,
        Wrapper_Exception_Discovery_AWDGetChangedSiteError,
        Wrapper_Exception_Discovery_AWDGetChangedWebError,
        Wrapper_Exception_Discovery_AWDGetChangedUsersError,
        Wrapper_Exception_Discovery_AWDGetVersionsError,
        Wrapper_Exception_Discovery_AWDGetItemModifyTimeError,
        Wrapper_Exception_Discovery_AWDGetFolderError,
        Wrapper_Exception_Discovery_AWDGetGuidDocIdMappingError,
        Wrapper_Exception_Discovery_AWDValidateNameError,
        Wrapper_Exception_Discovery_AWDGetItemWebpartsError,
        Wrapper_Exception_Discovery_AWDGetItemSizeError,
        Wrapper_Exception_Discovery_AWDGetSizeSizeError,
        Wrapper_Exception_Discovery_AWDGetWebRootFolderError,
        Wrapper_Exception_Discovery_AWDGetSubSitesError,
        Wrapper_Exception_Discovery_CurrentFilterNotExist,
        Wrapper_Exception_Discovery_Office365NotSupportSiteCollectionFilter,
        Wrapper_Exception_Discovery_Office365NotSupportWebCreatedByRule,
        Wrapper_Exception_Discovery_Office365NotSupportListCreatedByRule,
        Wrapper_Exception_Discovery_Office365NotSupportlistRootFolderFilter,
        Wrapper_Exception_Discovery_SOARAuditorEnableException,
        Wrapper_Exception_Discovery_SOARReportServiceException,
        Wrapper_Exception_Discovery_SOARNotFindAuditorJobException,
        Wrapper_Exception_Discovery_SOARAuditorJobException,
        Wrapper_Exception_Discovery_SOARCheckAuditorJobTimeException,
        Wrapper_Exception_Discovery_SOARNodeModifiedAfterAuditorRetriveJob,
        Wrapper_Exception_Discovery_AWDOffice365NotSupportListCreatedByRule,
        #endregion

        #region Common
        Wrapper_Exception_Common_PolicySchemaIsNull,
        //Wrapper_Exception_Common_AWCAveAssUtilGetPropertyInternal,
        // Wrapper_Exception_Common_AWCAveAssUtilGetMethodInternal,
        // Wrapper_Exception_Common_AWCAveAssUtilGetCtorInternal,
        // Wrapper_Exception_Common_AWCAveAssUtilGetFieldInternal,
        Wrapper_Exception_Common_BlobPoolnotGeneratedByDocAve6Extender,
        Wrapper_Exception_Common_NotFindData,
        Wrapper_Exception_Common_TermSetNotExist,
        Wrapper_Exception_Common_NotFindTermSetwithId,
        Wrapper_Exception_Common_FakeUserException,
        Wrapper_Exception_Common_PasswordExpired,
        Wrapper_Exception_Common_IncorrectUserNameOrPassword,
        Wrapper_Exception_Common_NonOffice365Account,
        Wrapper_Exception_Common_AccountDisable,
        Wrapper_Exception_Common_Office365SiteExpired,
        Wrapper_Exception_Common_LoginFailedForFedAuthCookie,
        Wrapper_Exception_Common_PasswordNotMatch,
        Wrapper_Exception_Common_AIRMSIPCClientNotFound,
        Wrapper_Exception_Common_AIRMSIPCClientLoadFailed,
        Wrapper_Exception_Common_AIRSuperUserNotConfigured,

        #endregion

        #region  Office365

        #region Common
        Wrapper_Exception_Office365_Common_NotEnoughInfo,
        Wrapper_Exception_Office365_Common_RemoteServerError,
        Wrapper_Exception_Office365_Common_NotFoundFolder,
        Wrapper_Exception_Office365_Common_DeleteItemVersionFailed,
        Wrapper_Exception_Office365_Common_UsedTermError,
        Wrapper_Exception_Office365_Common_NotFindRole,
        Wrapper_Exception_Office365_Common_CannotDeleteGroup,
        Wrapper_Exception_Office365_Common_CannotCreateDuplicatedLabel,
        Wrapper_Exception_Office365_Common_UniqueGroupNameError,
        Wrapper_Exception_Office365_Common_RootWebError,
        Wrapper_Exception_Office365_Common_FileConentBroken,
        #endregion

        #region RequestCommon
        Wrapper_Exception_Office365_RequestCommon_MoveNavigationFailed,

        #endregion

        #region Rquest
        Wrapper_Exception_Office365_Request_OpenThemeFailed,
        Wrapper_Exception_Office365_Request_AccessDenied,

        #endregion
        #endregion

    }
}