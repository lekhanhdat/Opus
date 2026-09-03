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
using LOGRESOURCECOMMON = Merged18NResources.Common;
using LOGRESOURCEARCHIVEINTER = Merged18NResources.Archive.ArchiveForInternationalization;
using Storage.Util;
using AvePoint.RA.I18N.Core;

namespace AvePoint.RA.SharePoint.ArchiverCommon
{
    [Serializable]
    public class ScheduleManualStoppedException : Exception
    {
        public ScheduleManualStoppedException()
            : base(LOGRESOURCECOMMON.SCUScheduleExceptionsScheduleManualStoppedException)
        {
        }

        public ScheduleManualStoppedException(string message)
            : base(message)
        {
        }

        public ScheduleManualStoppedException(string message, string scheduleId)
            : base(string.Format(LOGRESOURCECOMMON.SCUScheduleExceptionsScheduleManualStoppedExceptionA, scheduleId, message))
        {
        }
    }

    [Serializable]
    public class SPObjectNotFoundException : Exception
    {
        public SPObjectNotFoundException()
            : base(LOGRESOURCECOMMON.SCUScheduleExceptionsSPObjectNotFoundException)
        {
        }

        public SPObjectNotFoundException(string message)
            : base(message)
        {
        }

        public SPObjectNotFoundException(string message, string objectType, string fullName)
            : base(string.Format(LOGRESOURCECOMMON.SCUScheduleExceptionsSPObjectNotFoundExceptionA, objectType, fullName, message))
        {
        }
    }

    [Serializable]
    public class SPObjectReadOnlyException : Exception
    {
        public SPObjectReadOnlyException(string message, string objectType, string fullName)
            : base(string.Format(LOGRESOURCECOMMON.SCUScheduleExceptionsSPObjectReadOnlyException, objectType, fullName, message))
        {
        }
    }

    [Serializable]
    public class SPObjectLockedException : Exception
    {
        public SPObjectLockedException(string message, string objectType, string fullName)
            : base(string.Format(LOGRESOURCECOMMON.SCUScheduleExceptionsSPObjectLockedException, objectType, fullName, message))
        {
        }
    }

    [Serializable]
    public class AveCLReaderException : Exception
    {
        public AveCLReaderException(string msg)
            : base(msg)
        {
        }

        public AveCLReaderException(string msg, Exception exception)
            : base(msg, exception)
        {
        }

        public AveCLReaderException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }
    }

    [Serializable]
    public class ExternalIntegrationDBNotFoundException : Exception
    {
        public ExternalIntegrationDBNotFoundException(string message)
            : base(message)
        {
        }
    }

    [Serializable]
    public class StubNameConflictException : Exception
    {
        public StubNameConflictException() : base(LOGRESOURCEARCHIVEINTER.StorageOptimization_SOARCOMArchiverReportDtoAddDeletionCommonsItem)
        { }
    }

    [Serializable]
    public class BackupDataStoreException : Exception
    {
        public BackupDataStoreException(string message) : base(message)
        {
        }
    }

    [Serializable]
    public class StorageFactoryException : Exception
    {
        public StorageFactoryException(string msg) :
            base(msg)
        {
        }
        public StorageFactoryException(string msg, Exception innerException)
            : base(msg, innerException)
        {
        }

    }

    [Serializable]
    public class NotEnoughFreeSpaceException : XException
    {
        public NotEnoughFreeSpaceException(string msg, Exception e) : base(msg, e) { }

        public NotEnoughFreeSpaceException(string msg) : base(msg) { }
    }

    [Serializable]
    public class ConetentSkipException : Exception
    {
        public ConetentSkipException(string message) : base(message) { }
    }

    [Serializable]
    public class MergeIndexException : Exception
    {
        public MergeIndexException(string message) : base(message) { }
    }

    [Serializable]
    public class CheckOutDocumentDeleteException : Exception
    {
        public CheckOutDocumentDeleteException(string msg)
            : base(msg)
        {
        }

        public CheckOutDocumentDeleteException(string msg, Exception exception)
            : base(msg, exception)
        {
        }

        public CheckOutDocumentDeleteException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }
    }

    [Serializable]
    public class CheckOutDocumentDeclareException : Exception
    {
        public CheckOutDocumentDeclareException(string msg)
            : base(msg)
        {
        }

        public CheckOutDocumentDeclareException(string msg, Exception exception)
            : base(msg, exception)
        {
        }

        public CheckOutDocumentDeclareException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }
    }

    [Serializable]
    public class DocumentSetContentTypeFileDeclareException : Exception
    {
        public DocumentSetContentTypeFileDeclareException(string msg)
            : base(msg)
        {
        }

        public DocumentSetContentTypeFileDeclareException(string msg, Exception exception)
            : base(msg, exception)
        {
        }

        public DocumentSetContentTypeFileDeclareException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }
    }

    [Serializable]
    public class LabelDocumentDeleteException : Exception
    {
        public LabelDocumentDeleteException(string msg)
            : base(msg)
        {
        }

        public LabelDocumentDeleteException(string msg, Exception exception)
            : base(msg, exception)
        {
        }

        public LabelDocumentDeleteException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }
    }

    [Serializable]
    public class StubUnableGenerateRestoreLinkException : Exception
    {
        public StubUnableGenerateRestoreLinkException()
            : base("RM_Archiver_StubUnableGenerateRestoreLinkException")
        {}
    }

    [Serializable]
    public class ScheduleJobConfigurationError : Exception
    {
        public ScheduleJobConfigurationError()
            : base("RM_JS_RDM_CreateRule_InvalidRuleInfo")
        { }
    }

    [Serializable]
    public class CGDBSCTableNotFoundException : Exception
    {
        public CGDBSCTableNotFoundException(string message)
            : base(message)
        {
        }
    }

    [Serializable]
    public class CGDBSummaryTableException : Exception
    {
        public CGDBSummaryTableException(string message)
            : base(message)
        {
        }
    }

    [Serializable]
    public class LicenseMismatchOfAvePointStorageException : Exception
    {
        public LicenseMismatchOfAvePointStorageException()
            : base("StorageOptimization_LicenseMismatchOfStorageException")
        {
        }
    }

    [Serializable]
    public class FSNotSurpportAvePointStorageException : Exception
    {
        public FSNotSurpportAvePointStorageException()
            : base("RM_FS_Retain_MoveAction_NotSurpportAvepointStorage")
        {
        }
    }

    [Serializable]
    public class FileContentLengthNullException : Exception
    {
        private readonly string mMessage;

        public FileContentLengthNullException()
        {
            // Add implementation.
        }

        public FileContentLengthNullException(string message)
            : base(message)
        {
            mMessage = message;
        }
    }
}
