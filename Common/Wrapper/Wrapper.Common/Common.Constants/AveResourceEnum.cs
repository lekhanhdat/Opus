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
using System.Threading.Tasks;

namespace AvePoint.Wrapper.Common
{
    public enum WrapperReportResourceKey
    {
        //restore 
        Wrapper_SkippedAccessRequestListItem,
        Wrapper_SkippedItemByLastModifiedTime,
        Wrapper_SkippedItemByHasUniqueValue,
        Wrapper_SkippedByIsPersonalView,
        Wrapper_SkipSpecialListsWhileOffice365CustomScriptDisabled,
        Wrapper_SkippedByDeclaredDocument,
        Wrapper_SkippedApp,
        Wrapper_RestoreAppDataFailedForInstallAppFailed,
        Wrapper_ContentTypeConflict,
        Wrapper_ConnotFindSchemaDependency,
        Wrapper_ConnotFindContentTypeSchemaDependency,
        Wrapper_ConnotFindFieldSchemaDependency,
        Wrapper_ContentTypeFailed,
        Wrapper_DiscoverFailed,
        Wrapper_MissingObject,
        Wrapper_ConnectSiteError, 
        Wrapper_TermSetNotFound,
        Wrapper_ConnectStorageError,
        Wrapper_AccessDenied,
        Wrapper_ExceedStorageLimit,
        Wrapper_SkipSpecialList,
        Wrapper_ExceedTempLimit,
        Wrapper_SkipO365SpecialList,
        Wrapper_SkippedByNotIncludeListView,
        Wrapper_SkippedItemByIsSameItem,
        Wrapper_IncorrectUserNameOrPasswordError,
        Wrapper_SkipProject,
        Wrapper_IRMSuperUserNotConfigured,
        Wrapper_IRMUnprotectFileFailed,
        Wrapper_IRMMSIPCClientNotAvailable,
        Wrapper_AccountDisableError,
        Wrapper_ConfirmUserPermission,
        Wrapper_ObjectNotSupportedWithAppProfile,
        Wrapper_SkippedByCannotEditItem,

        Wrapper_RecordingDriveNotSkip,
        Wrapper_RecordingDriveAccessDenied,
        Wrapper_RecordingDriveAccessDenied_Team,
        Wrapper_SkippedItemByTargetGtSourceVersion,
    }

    //granular module
    public enum RestoreReportKey
    {
        Item_Unknown,
        Item_SecurityListSkipped,
        Item_RestoreListError,
        Item_CanNotFindFolderParent,
        Item_SecurityFolderSkipped,
        Item_RestoreFolderErrorReport,
        Item_CanNotFindItemParent,
        Item_SkipBackupFailedItem,
        Item_SecurityItemSkipped,
        Item_RestoreItemError,
        Item_RestoreProjectError,
        Item_ReceivErrorMessage,
        Item_UserProfleFaild,
        Item_ItemSkipped,
        Item_RestoreSiteError,
        Item_CanNotFindWebParent,
        Item_SecurityWebSkipped,
        Item_RestoreWebError,
        Item_CanNotFindListParent,
        Item_CanNotFindProjectParent,
        Item_DisposeError,
        Item_AWRRestoreSiteError,
        Item_AWRRestoreWebError,
        Item_ASAWRRestoreSiteError,
        Item_MediaError,
        Item_CanNotFindAppParent,
        Item_RestoreAppError,
        Item_GlobalRestoreOptionWorkerSkip,

    }

    public enum BackupReportKey
    {
        Item_CloseFileSenderError,
        Item_MainError,
        Item_CheckSiteError,
        Item_BackSiteError,
        Item_BackWebError,
        Item_BackupListError,
        Item_BackupProjectError,
        Item_BackupFolderError,
        Item_BackupFolderSkip,
        Item_BackupItemError,
        Item_ASBBackupSiteError,
        Item_AWBBackupSiteErrorReport,
        Item_AWBBackupWebErrorReport,
        Item_NoAvaliableMediaError,
        Item_ConnectMediaFailed,
        Item_UnusableMediaInfo,
        Item_BackAppError,
        Item_AIBSkipToBackupStub,
        Item_BuildContentError,
        Item_SkipFailedItem,
    }

    //CM module
    public enum CMPrimaryKey
    {
        CM_ItemBackupFailed,
        CM_InvalidOperation,


    }

    public enum CMSecondaryKey
    {
        CM_ItemSkipped,
        CM_ItemDeleteFromExcel,
        CM_RestoreCtrlRunTemplateError,
        CM_RestoreCtrlRunFailed,
        CM_RestoreFailed,
        CM_NoRestoreAttachmentTooLarge,
        CM_NoRestoreConflictObject,
        CM_NoRestoreExistDocument,
        CM_NoRestoreExistItem,
        CM_NoRestoreNullParentObject,

    }

    //DPM module
    public enum DPMPrimaryKey
    {
        DPM_ItemBackupFailed,
        DPM_InvalidOperation,
        DPM_DiscoverAccessdenied,
        DPM_DiscoverSiteCollectionLock,
        DPM_ConnectError,
        DPM_StartSecondaryFailed,
        DPM_BackupSolutionFileFailed,

    }

    public enum DPMSecondaryKey
    {
        DPM_ItemSkipped,
        DPM_ItemDeleteFromExcel,
        DPM_RestoreCtrlRunTemplateError,
        DPM_RestoreCtrlRunFailed,
        DPM_RestoreFailed,
        DPM_LimitError,
        DPM_DeploySolutionError,
        DPM_SkipRestoreObj,
        DPM_NoRestoreNullParentObject,
        DPM_NoRestoreConflictObject,
        DPM_NoRestoreExistItem,
        DPM_NoRestoreExistDocument,
        DPM_UserSolutionDeploymentExistsUpgradeException,
    }

    public enum SiteState
    {
        NoAccess,

        ReadOnly,

        Unlock,
    }

}
