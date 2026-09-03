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

namespace RAGoogle.API
{
    public class ErrorCodeFactory
    {
        public List<ErrorCode> ErrorCodeList { get; set; }
        public ErrorCodeFactory()
        {
            ErrorCodeList = new List<ErrorCode>()
            {
                GSuiteUserNotExist,
                GSuiteSDNotExist,
                GSuiteClassNotExist,
                GSuiteSDNotFound,
                GsuiteRetrieveUsersInfoError,
                GsuiteArchivedUser,
                GsuiteSuspendedUser,
                GsuiteRestoreArchivedUser,
                GsuiteRestoreSuspendedUser,
                GsuiteUnauthorizedClient,
                GsuiteSDUnauthorizedClient,
                GSuiteFileAccessDenied,
                GsuiteSDNoMember,
                GsuiteSDNoAvailableMember,
                GsuiteSDNoManagerPermission,
                GSuiteDailyLimitExceeded,
                GSuiteRateLimitExceeded,
                GSuite404NotFound,
                GSuiteServiceUnavailable,
                GSuiteServerErrorOccurred,
                GSuiteUnsafeFile,
                GSuiteAborted,
                GSuiteAlreadyExists,
                GsuiteRestoreSharedWithMeError,
                GsuiteExportSharedWithMeError,
                GsuiteExportSharedWithMeFileError,
                GsuiteExportSharedWithMeFolderError,
                GsuiteRestoreSharedWithMeFileError,
                GsuiteRestoreSharedWithMeFolderError,
                GSuiteChangeSubscription,
                GSuiteFailedPrecondition,
                GSuiteFieldValueExceedsLimit,
                GSuiteInvalidArgument,
                //NO ERROR CODE
                GSuiteThirdPartyFile,
                GSuiteTaskError,
                GSuiteTransferredError,
                InvalidAccountInfo,
                GSuiteMediaPermission,
                GSuiteMediaCommon,
                GSuiteIndexDBDownload,
                DownloadFileError,
                UploadFileError,
            };
        }

        public static readonly ErrorCode GSuiteThirdPartyFile = new ErrorCode("", "JobReport.Details.Drive.Backup.File.Download.3rd.Error");
        public static readonly ErrorCode GSuiteTaskError = new ErrorCode("", "JobReport.Details.TaskWasCanceled.Error");
        public static readonly ErrorCode GSuiteTransferredError = new ErrorCode("", "JobReport.Details.TransferredAPartialFile.Error");
        public static readonly ErrorCode InvalidAccountInfo = new ErrorCode("", "JobReport.Details.InvalidAccountInfo.Error");
        public static readonly ErrorCode GSuiteMediaPermission = new ErrorCode("", "JobReport.Details.Backup.Media.Permission.Error");
        public static readonly ErrorCode GSuiteMediaCommon = new ErrorCode("", "JobReport.Details.Backup.Media.Common.Error");
        public static readonly ErrorCode GSuiteIndexDBDownload = new ErrorCode("", "JobReport.Details.IndexDB.Download.Error");
        public static readonly ErrorCode DownloadFileError = new ErrorCode("", "JobReport.Details.DownloadFile.Error");
        public static readonly ErrorCode UploadFileError = new ErrorCode("", "JobReport.Details.Drive.Restore.File.Upload.Error");
        //for error code
        public static readonly ErrorCode GSuiteUserNotExist = new ErrorCode("B-UserNotExist", "JobReport.Details.User.Deleted.Comment.Error");
        public static readonly ErrorCode GSuiteSDNotExist = new ErrorCode("B-SDNotExist", "JobReport.Details.SharedDrive.Not.Found.Skip");
        public static readonly ErrorCode GSuiteClassNotExist = new ErrorCode("B-ClassNotExist", "JobReport.Details.Classroom.Deleted.Comment.Error");
        public static readonly ErrorCode GSuiteSDNotFound = new ErrorCode("R-SDNotFound", "JobReport.Details.SharedDrive.Not.Found.OOP.Failed");
        public static readonly ErrorCode GsuiteRetrieveUsersInfoError = new ErrorCode("RetrieveUsersInfoError", "JobReport.Details.User.Authorized.Comment.Error");
        public static readonly ErrorCode GsuiteArchivedUser = new ErrorCode("ArchivedUser", "JobReport.Details.User.Archived.Error");
        public static readonly ErrorCode GsuiteSuspendedUser = new ErrorCode("SuspendedUser", "JobReport.Details.User.Suspended.Error");
        public static readonly ErrorCode GsuiteRestoreArchivedUser = new ErrorCode("ArchivedUser", "JobReport.Details.User.Archived.Comment.Error");
        public static readonly ErrorCode GsuiteRestoreSuspendedUser = new ErrorCode("SuspendedUser", "JobReport.Details.User.Suspended.Comment.Error");
        public static readonly ErrorCode GsuiteUnauthorizedClient = new ErrorCode("UnauthorizedClient", "JobReport.Details.UnauthorizedClient.Error");
        public static readonly ErrorCode GsuiteSDUnauthorizedClient = new ErrorCode("UnauthorizedClient", "JobReport.Details.UnauthorizedClient.SharedDrive.Error");
        public static readonly ErrorCode GSuiteFileAccessDenied = new ErrorCode("B-NoDownloadPermission", "JobReport.Details.Drive.Backup.File.Download.Permission.Error");
        public static readonly ErrorCode GsuiteSDNoMember = new ErrorCode("B-SDNoMember", "JobReport.Details.SharedDrive.GetOrganizer.Error");
        public static readonly ErrorCode GsuiteSDNoAvailableMember = new ErrorCode("R-SDNoAvailableMember", "JobReport.Details.SharedDrive.GetOrganizer.Error.Restore");
        public static readonly ErrorCode GsuiteSDNoManagerPermission = new ErrorCode("R-SDNoManagerPermission", "JobReport.Details.SharedDrive.GetOrganizer.Error.RestorePermission");
        public static readonly ErrorCode GSuiteDailyLimitExceeded = new ErrorCode("GoogleAPIQuota", "JobReport.Details.DailyLimitExceeded.Error");
        public static readonly ErrorCode GSuiteRateLimitExceeded = new ErrorCode("GoogleAPIQuota", "JobReport.Details.RateLimitExceeded.Error");
        public static readonly ErrorCode GSuite404NotFound = new ErrorCode("B-GoogleAPI404NotFound", "JobReport.Details.404NotFound.Error");
        public static readonly ErrorCode GSuiteServiceUnavailable = new ErrorCode("B-GoogleAPIServiceError", "JobReport.Details.ServiceUnavailable.Error");
        public static readonly ErrorCode GSuiteServerErrorOccurred = new ErrorCode("B-GoogleAPIServiceError", "JobReport.Details.ServerErrorOccurred.Error");
        public static readonly ErrorCode GSuiteUnsafeFile = new ErrorCode("B-GoogleAPIRiskFile", "JobReport.Details.Drive.Backup.File.Download.Spam.Error");
        public static readonly ErrorCode GSuiteAborted = new ErrorCode("R-GoogleAPIAbortedError", "JobReport.Details.Aborted.Error");
        public static readonly ErrorCode GSuiteAlreadyExists = new ErrorCode("R-GoogleAPIAlreadyExistsError", "JobReport.Details.AlreadyExists.Error");
        public static readonly ErrorCode GsuiteRestoreSharedWithMeError = new ErrorCode("SharedWithMeError", "JobReport.Details.Drive.Restore.SharedWithMe.User.Error");
        public static readonly ErrorCode GsuiteExportSharedWithMeError = new ErrorCode("SharedWithMeError", "JobReport.Details.Drive.Export.SharedWithMe.User.Error");
        public static readonly ErrorCode GsuiteExportSharedWithMeFileError = new ErrorCode("SharedWithMeFileError", "JobReport.Details.Drive.Export.SharedWithMe.File.Error");
        public static readonly ErrorCode GsuiteExportSharedWithMeFolderError = new ErrorCode("SharedWithMeFolderError", "JobReport.Details.Drive.Export.SharedWithMe.Folder.Error");
        public static readonly ErrorCode GsuiteRestoreSharedWithMeFileError = new ErrorCode("SharedWithMeFileError", "JobReport.Details.Drive.Restore.SharedWithMe.File.Error");
        public static readonly ErrorCode GsuiteRestoreSharedWithMeFolderError = new ErrorCode("SharedWithMeFolderError", "JobReport.Details.Drive.Restore.SharedWithMe.Folder.Error");
        public static readonly ErrorCode GSuiteChangeSubscription = new ErrorCode("R-ChangeSubscription", "JobReport.Details.ChangeOwnPrimarySubscription.Error");
        public static readonly ErrorCode GSuiteFailedPrecondition = new ErrorCode("GoogleAPIFailedPrecondition", "JobReport.Details.FailedPrecondition.Error");
        public static readonly ErrorCode GSuiteFieldValueExceedsLimit = new ErrorCode("R-GoogleAPIFieldValueExceedsLimit", "JobReport.Details.FieldValueExceedsLimit.Error");
        public static readonly ErrorCode GSuiteInvalidArgument = new ErrorCode("R-GoogleAPIInvalidArgument", "JobReport.Details.InvalidArgument.Error");
    }
}