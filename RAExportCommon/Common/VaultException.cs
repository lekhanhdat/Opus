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
using LOGRESOURCE = Merged18NResources.Export;
using AvePoint.GCommon.Contract.CodeReview;

namespace RAExportCommon
{
    [Serializable]
    public class SPContainerLevelException : Exception
    {
        public SPContainerLevelException(Exception ex)
            : base(ex.Message, ex)
        { }
    }

    [Serializable]
    public class ExportServiceException : Exception
    {
        public ExportServiceException(Exception ex)
            : base(ex.Message, ex)
        { }
    }

    [Serializable]
    public class ScheduleManualStoppedException : Exception
    {
        public ScheduleManualStoppedException()
            : base(LOGRESOURCE.Vault_SCUScheduleExceptionsScheduleManualStoppedException)
        {
        }

        public ScheduleManualStoppedException(string message)
            : base(message)
        {
        }

        public ScheduleManualStoppedException(string message, string scheduleId)
            : base(string.Format("Schedule:{0} is stopped.\n{1}.", scheduleId, message))
        {
        }
    }

    [Serializable]
    public class SPObjectNotFoundException : Exception
    {
        public SPObjectNotFoundException()
            : base(LOGRESOURCE.Vault_SCUScheduleExceptionsSPObjectNotFoundException)
        {
        }

        public SPObjectNotFoundException(string message)
            : base(message)
        {
        }

        public SPObjectNotFoundException(string message, string objectType, string fullName)
            : base(string.Format("Object:{0} with FullPath {1} can not be found.\n{2}.", objectType, fullName, message))
        {
        }
    }

    [Serializable]
    public class SPObjectReadOnlyException : Exception
    {
        public SPObjectReadOnlyException(string message, string objectType, string fullName)
            : base(string.Format("Object:{0} with FullPath {1} is readonly.\n{2}.", objectType, fullName, message))
        {
        }
    }

    [Serializable]
    public class SPObjectLockedException : Exception
    {
        public SPObjectLockedException(string message, string objectType, string fullName)
            : base(string.Format("Object:{0} with FullPath {1} has been locked.\n{2}.", objectType, fullName, message))
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
    public class TestHttpRequestException : Exception
    {
        public TestHttpRequestException(Exception ex)
            : base(LOGRESOURCE.Vault_SOVTVaultExportMHTContentErrorException, ex)
        {
        }
        public TestHttpRequestException()
            : base(LOGRESOURCE.Vault_SOVTVaultExportMHTContentErrorException)
        {
        }
    }

    [Serializable]
    public class VaultFailedException : Exception
    {
        public VaultFailedException()
            : base("The Vault Export Failed.")
        {
        }

        public VaultFailedException(string ms)
            : base(ms)
        {
        }
    }

    [Serializable]
    public class VaultRuleMatchingException : Exception
    {
        /// <summary>
        /// control 做国际化，Vault_CPLVaultScanNotMatchRuleLevel 为key。修改 message 内容要通知control。
        /// </summary>
        public VaultRuleMatchingException()
            : base(LOGRESOURCE.Vault_CPLVaultScanNotMatchRuleLevelKey)
        {
        }
        public VaultRuleMatchingException(string ms)
            : base(ms)
        {
        }
    }

    [Serializable]
    public class VaultExportPathRepetition : Exception
    {
        public VaultExportPathRepetition()
            : base(LOGRESOURCE.Vault_CPLVaultExportPathRepetitionErrorKey)
        {
        }
        public VaultExportPathRepetition(string ms)
            : base(ms)
        {
        }
    }

    [Serializable]
    public class VaultScanReportServiceError : Exception
    {
        public VaultScanReportServiceError()
            : base(LOGRESOURCE.Vault_CPLVaultScanLastAccessRuleErrorKey)
        {
        }
        public VaultScanReportServiceError(string ms)
            : base(ms)
        {
        }
    }

    [Serializable]
    public class VaultPermissionDeniedError : Exception
    {
        public VaultPermissionDeniedError()
            : base(LOGRESOURCE.Vault_CPLVaultPermissionDeniedError)
        {
        }
        public VaultPermissionDeniedError(string ms)
            : base(ms)
        {
        }
    }

    [Serializable]
    public class ExportConfigurationFileError : Exception
    {
        public ExportConfigurationFileError(string ms)
            : base(ms)
        {
        }
    }
}
