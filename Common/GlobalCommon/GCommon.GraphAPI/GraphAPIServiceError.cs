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

namespace AvePoint.GCommon.GraphAPI
{
    public enum ServiceError
    {
        Unknown = 0,
        AccessDenied = 1,
        ActivityLimitReached = 2,
        GeneralException = 3,
        InvalidRange = 4,
        InvalidRequest = 5,
        ItemNotFound = 6,
        MalwareDetected = 7,
        NameAlreadyExists = 8,
        NotAllowed = 9,
        NotSupported = 10,
        ResourceModified = 11,
        ResyncRequired = 12,
        ServiceNotAvailable = 13,
        QuotaLimitReached = 14,
        Unauthenticated = 15,

        AccessRestricted = 16,
        CannotSnapshotTree = 17,
        ChildItemCountExceeded = 18,
        EntityTagDoesNotMatch = 19,
        FragmentLengthMismatch = 20,
        FragmentOutOfOrder = 21,
        FragmentOverlap = 22,
        InvalidAcceptType = 23,
        InvalidParameterFormat = 24,
        InvalidPath = 25,
        InvalidQueryOption = 26,
        InvalidStartIndex = 27,
        LockMismatch = 28,
        LockNotFoundOrAlreadyExpired = 29,
        LockOwnerMismatch = 30,
        MalformedEntityTag = 31,
        MaxDocumentCountExceeded = 32,
        MaxFileSizeExceeded = 33,
        MaxFolderCountExceeded = 34,
        MaxFragmentLengthExceeded = 35,
        MaxItemCountExceeded = 36,
        MaxQueryLengthExceeded = 37,
        MaxStreamSizeExceeded = 38,
        ParameterIsTooLong = 39,
        ParameterIsTooSmall = 40,
        PathIsTooLong = 40,
        PathTooDeep = 42,
        PropertyNotUpdateable = 43,
        ResyncApplyDifferences = 44,
        ResyncUploadDifferences = 45,
        ServiceReadOnly = 46,
        ThrottledRequest = 47,
        TooManyResultsRequested = 48,
        TooManyTermsInQuery = 49,
        TotalAffectedItemCountExceeded = 50,
        TruncationNotAllowed = 51,
        UploadSessionFailed = 52,
        UploadSessionIncomplete = 53,
        UploadSessionNotFound = 54,
        VirusSuspicious = 55,
        ZeroOrFewerResultsRequested = 56,

        MaximumProjectsOwnedByUser = 57,
        MaximumProjectsSharedWithUser = 58,
        MaximumTasksCreatedByUser = 59,
        MaximumTasksAssignedToUser = 60,
        MaximumTasksInProject = 61,
        MaximumActiveTasksInProject = 62,
        MaximumBucketsInProject = 63,
        MaximumUsersSharedWithProject = 64,
        MaximumReferencesOnTask = 65,
        MaximumChecklistItemsOnTask = 66,
        MaximumAssigneesInTasks = 67,
        MaximumPlannerPlans=68,

        ErrorInvalidGroup = 69,
        ErrorNonExistentMailbox = 70,
    }
}