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
using AvePoint.Wrapper.Resource;
using System.Net;
using AvePoint.GCommon.Contract.ErrorCode;

namespace AvePoint.Wrapper.Common
{
    public enum ErrorLevel
    {
        Normal,
        Wraning,
        Error,
    }

    [Serializable]
    public class AveInternalException : Exception
    {
        public AveInternalException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public AveInternalException(string message, Exception innerException, ErrorLevel level)
            : base(message, innerException)
        {
            ErrorLevel = level;
        }

        public bool NeedRetry { get; set; }

        public ErrorLevel ErrorLevel { get; set; }
    }

    [Serializable]
    public class AveSkipLockSiteException : AveWrapperI18NException
    {
        public string SiteCollectionUrl;
        public string SiteCollectionTitle;
        public AveSkipLockSiteException(string message)
            : base(message)
        {
        }

        public AveSkipLockSiteException(string key, string defaultValue)
            : base(key, defaultValue)
        {
        }
        public AveSkipLockSiteException(string key, string defaultValue, params object[] args)
            : base(key, defaultValue, args)
        {
        }

        public AveSkipLockSiteException(string message, Exception e)
            : base(message, e)
        {
        }

        public SiteState SiteState { get; set; }

        public override TroubleshootingErrorCode ErrorCode
        {
            get
            {
                switch (SiteState)
                {
                    case SiteState.ReadOnly:
                        return TroubleshootingErrorCode.SP_SiteReadOnly;
                    case SiteState.NoAccess:
                        return TroubleshootingErrorCode.SP_SiteLocked;
                    default:
                        return TroubleshootingErrorCode.Default;
                }
            }
        }
    }

    #region common exception
    [Serializable]
    public class AveTermSetNotFoundException : AveWrapperI18NException
    {
        public AveTermSetNotFoundException(string message)
            : base(message)
        {
        }

        public AveTermSetNotFoundException(string key, string defaultValue)
            : base(key, defaultValue)
        {
        }

        public AveTermSetNotFoundException(string key, string defaultValue, params object[] args)
            : base(key, defaultValue, args)
        {
        }
    }

    [Serializable]
    public class AveConnectStorageException : AveWrapperI18NException
    {
        public AveConnectStorageException(string message)
            : base(message)
        {
        }

        public AveConnectStorageException(string key, string defaultValue)
            : base(key, defaultValue)
        {
        }

        public AveConnectStorageException(string key, string defaultValue, params object[] args)
            : base(key, defaultValue, args)
        {
        }
    }

    [Serializable]
    public class AveExceedStorageLimitException : AveWrapperI18NException
    {
        public AveExceedStorageLimitException(string message)
            : base(message)
        {
        }

        public AveExceedStorageLimitException(string key, string defaultValue)
            : base(key, defaultValue)
        {
        }

        public AveExceedStorageLimitException(string key, string defaultValue,Exception innerException)
            : base(key, defaultValue,innerException)
        {
        }

        public AveExceedStorageLimitException(string key, string defaultValue, params object[] args)
            : base(key, defaultValue, args)
        {
        }
    }

    [Serializable]
    public class AveExceedTempLimitException : AveWrapperI18NException
    {
        public AveExceedTempLimitException(string message)
            : base(message)
        {
        }

        public AveExceedTempLimitException(string key, string defaultValue)
            : base(key, defaultValue)
        {
        }

        public AveExceedTempLimitException(string key, string defaultValue, Exception innerException)
            : base(key, defaultValue, innerException)
        {
        }

        public AveExceedTempLimitException(string key, string defaultValue, params object[] args)
            : base(key, defaultValue, args)
        {
        }
    }

    [Serializable]
    public class AveLabelAppliedException : Exception
    {
        public AveLabelAppliedException(string message) : base(message)
        {
        }
    }

    #endregion

    #region Authentication, UserName, Password

    [Serializable]
    public class AuthenticationFailedException : Exception
    {
        public AuthenticationFailedException(string message, HttpStatusCode statusCode)
            : base(message)
        {
            this.FailedStatusCode = statusCode;
        }

        public HttpStatusCode FailedStatusCode { get; set; }
    }

    [Serializable]
    public class PasswordExpiredException : AveWrapperI18NException
    {
        public PasswordExpiredException(string message) : base(message) { }

        public PasswordExpiredException(string key, string defaultValue, params object[] args)
            : base(key, defaultValue, args)
        {
        }
    }

    [Serializable]
    public class NonOffice365AccountException : AveWrapperI18NException
    {
        public NonOffice365AccountException(string message) : base(message) { }

        public NonOffice365AccountException(string key, string defaultValue, params object[] args)
            : base(key, defaultValue, args)
        {
        }
    }

    [Serializable]
    public class MailboxFolderDeadLoopException : AveWrapperI18NException
    {
        //public override TroubleshootingErrorCode ErrorCode { get { return TroubleshootingErrorCode.CO_MailboxFolderDeadLoop; } }

        public MailboxFolderDeadLoopException() : base("Wrapper_MailboxFolderDeadLoop", "Wrapper_MailboxFolderDeadLoop")
        {
        }
    }

    [Serializable]
    public class IncorrectUserNameOrPasswordException : AveWrapperI18NException
    {
        public IncorrectUserNameOrPasswordException(string message) : base(message) { }

        public IncorrectUserNameOrPasswordException(string key, string defaultValue, params object[] args)
            : base(key, defaultValue, args)
        {
        }
    }

    [Serializable]
    public class Office365SiteExpiredException : AveWrapperI18NException
    {
        public Office365SiteExpiredException(string message) : base(message) { }

        public Office365SiteExpiredException(string key, string defaultValue, params object[] args)
            : base(key, defaultValue, args)
        {
        }
    }

    [Serializable]
    public class AccountDisableException : AveWrapperI18NException
    {
        public AccountDisableException(string message) : base(message) { }

        public AccountDisableException(string key, string defaultValue, params object[] args)
            : base(key, defaultValue, args)
        {
        }
    }

    [Serializable]
    public class NoAuthenticationException : AveWrapperI18NException
    {
        public NoAuthenticationException(string key, string defaultValue, params object[] args) : base(key, defaultValue, args)
        {
        }

        public NoAuthenticationException() : base("WrapperReportResourceKey.Wrapper_NoAuthentication", "WrapperReportResource.Wrapper_NoAuthentication")
        {
        }
    }

    [Serializable]
    public class NoDelegateAppException : AveWrapperI18NException
    {
        public NoDelegateAppException(string key, string defaultValue, params object[] args) : base(key, defaultValue, args)
        {
        }

        public NoDelegateAppException() : base("WrapperReportResourceKey.Wrapper_NoDelegateApp", "WrapperReportResource.Wrapper_NoDelegateApp")
        {
        }
    }

    #endregion

    #region Granular  CM  DPM  Exception
    [Serializable]
    public class SkipException : AveWrapperI18NException
    {
        public SkipException() { }
        public SkipException(string message) : base(message) { }
        public SkipException(string message, Exception inner) : base(message, inner) { }
        protected SkipException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

        public SkipException(string key, string defaultValue)
            : base(key, defaultValue)
        {
        }

        public SkipException(string key, string defaultValue, params object[] args)
            : base(key, defaultValue, args)
        {
        }
    }

    //public class PauseProcessException : Exception
    //{
    //    public PauseProcessException()
    //        : base()
    //    {
    //    }
    //    public PauseProcessException(string message)
    //        : base(message)
    //    {
    //    }
    //}

    //public class StopProcessException : Exception
    //{
    //    public List<String> Paramters = new List<String>();
    //    public StopProcessException()
    //        : base()
    //    {
    //    }

    //    public StopProcessException(String message)
    //        : base(message)
    //    {
    //    }
    //}
    #endregion

    #region CM  Exception
    [Serializable]
    public class AveObjectMissingException : AveWrapperI18NException
    {
        public AveObjectMissingException()
            : base(WrapperReportResourceKey.Wrapper_MissingObject.ToString(), WrapperRestoreReportResource.Wrapper_MissingObject)
        {
        }

        public AveObjectMissingException(string message)
            : base(message)
        {
        }
    }

    [Serializable]
    public class AveSecurityTrimingException : AveWrapperI18NException
    {
        public AveSecurityTrimingException()
        { }

        public AveSecurityTrimingException(string message, Exception e)
            : base(message, e)
        { }

        public AveSecurityTrimingException(string key, string defaultValue)
            : base(key, defaultValue)
        {
        }

        public AveSecurityTrimingException(string key, string defaultValue, params object[] args)
            : base(key, defaultValue, args)
        {
        }
    }

    [Serializable]
    public class JobInitializationException : AveWrapperI18NException
    {
        public JobInitializationException(string message)
            : base(message)
        {
        }

        public JobInitializationException(string key, string defaultValue)
            : base(key, defaultValue)
        {
        }

        public JobInitializationException(string key, string defaultValue, params object[] args)
            : base(key, defaultValue, args)
        {
        }
    }

    [Serializable]
    public class InvalidOperationExceptionForCM : AveWrapperI18NException
    {
        public List<String> Paramters = new List<String>();
        public InvalidOperationExceptionForCM(String message)
            : base(message)
        {
        }
    }

    [Serializable]
    public class EncryptionException : Exception
    {
        public EncryptionException(string message)
            : base(message)
        {
        }
    }

    [Serializable]
    public class FailedSkipException : Exception
    {

    }

    [Serializable]
    public class TeamChannalFolderUpdateFailed : AveWrapperI18NException
    {
        public TeamChannalFolderUpdateFailed() { }
    }

    [Serializable]
    public class TemplateConflictException : Exception
    {

    }

    [Serializable]
    public class DeleteItemFromExcelException : Exception
    {

    }
    #endregion

    #region DPM Exception
    [Serializable]
    public class ControlLevelException : Exception
    {
        public Dictionary<string, string> SiteLockErrorMessageParameter = new Dictionary<string, string>();
        public List<string> ErrorMessageParameter = new List<string>();

        public ControlLevelException(string errormsg)
            : base(errormsg)
        {
        }
        public ControlLevelException(Exception e)
            : base(e.Message, e)
        {
        }
    }

    [Serializable]
    public class RestoreErrorException : Exception
    {
        public RestoreErrorException(Exception e)
            : base(e.Message, e)
        {
        }
        public RestoreErrorException(string errormsg)
            : base(errormsg)
        {
        }
    }

    [Serializable]
    public class SiteLockException : Exception
    {
        public readonly string ExceptionKey = "DPM_DISCOVER_SITECOLLECTIONLOCK";
        public string SiteCollectionUrl;
        public string LockStatus;
        public SiteLockException(string msg)
            : base(msg)
        {
        }
        public SiteLockException(string msg, Exception e)
            : base(msg, e)
        {
        }
    }

    [Serializable]
    public class AppUseSystemAccountException : Exception
    {
        public readonly string ExceptionKey = "DPM_AppUpdate_USESYSTEMACCOUNT";
        public string WebApplicationName;
        public string ErrorMessage;
        public AppUseSystemAccountException(string msg)
            : base(msg)
        {
        }
        public AppUseSystemAccountException(string msg, Exception e)
            : base(msg, e)
        {
        }
    }

    [Serializable]
    public class AccessdeniedException : Exception
    {
        public readonly string ExceptionKey = "DPM_DISCOVER_ACCESSDENIED";
        public string SiteCollectionUrl;
        public string DeniedUser;
        public AccessdeniedException(string msg)
            : base(msg)
        {
        }
        public AccessdeniedException(string msg, Exception e)
            : base(msg, e)
        {
        }
    }
    #endregion

    #region IRM Exception
    [Serializable]
    public sealed class AveIRMEnvironmentException : AveWrapperI18NException
    {
        public AveIRMEnvironmentException(string errorMessage)
            : base(WrapperReportResourceKey.Wrapper_IRMMSIPCClientNotAvailable.ToString(),
                  WrapperRestoreReportResource.Wrapper_IRMMSIPCClientNotAvailable, errorMessage)
        {
        }
    }

    [Serializable]
    public sealed class AveIRMSuperUserNotConfiguredException : AveWrapperI18NException
    {
        public AveIRMSuperUserNotConfiguredException(string tenantId, string appPrincipalId)
            : base(WrapperReportResourceKey.Wrapper_IRMSuperUserNotConfigured.ToString(), 
                  WrapperRestoreReportResource.Wrapper_IRMSuperUserNotConfigured, 
                  tenantId, 
                  appPrincipalId)
        {
        }
    }

    [Serializable]
    public sealed class AveIRMUnprotectFileFailedException : AveWrapperI18NException
    {
        public AveIRMUnprotectFileFailedException(string fileName, string tenantId, string appPrincipalId, string errorMessage)
            : base(WrapperReportResourceKey.Wrapper_IRMUnprotectFileFailed.ToString(),
                  WrapperRestoreReportResource.Wrapper_IRMUnprotectFileFailed,
                  fileName,
                  tenantId,
                  appPrincipalId,
                  errorMessage)
        {
        }
    }


    #endregion

    #region ChangeTokenExpireException
    [Serializable]
    public class AveChangeTokenExpireException : AveWrapperI18NException
    {
        public AveChangeTokenExpireException(string message)
            : base(message)
        {
        }

        public AveChangeTokenExpireException(string key, string defaultValue)
            : base(key, defaultValue)
        {
        }

        public AveChangeTokenExpireException(string key, string defaultValue, Exception innerException)
            : base(key, defaultValue, innerException)
        {
        }

        public AveChangeTokenExpireException(string key, string defaultValue, params object[] args)
            : base(key, defaultValue, args)
        {
        }
    }
    #endregion ChangeTokenExpireException

    #region App-Profile Exception
    [Serializable]
    public sealed class AveObjectNotSupportedWithAppProfileException : AveWrapperI18NException
    {
        public AveObjectNotSupportedWithAppProfileException(string objectIdentity)
            : base(WrapperReportResourceKey.Wrapper_ObjectNotSupportedWithAppProfile.ToString(),
                  WrapperRestoreReportResource.Wrapper_ObjectNotSupportedWithAppProfile, objectIdentity)
        {
        }
    }

    //public sealed class AveObjectNotAccessableWithAppProfileException : AveWrapperI18NException
    //{
    //    public AveObjectNotAccessableWithAppProfileException(string objectIdentity)
    //        : base(WrapperReportResourceKey.Wrapper_ObjectNotSupportedWithAppProfile.ToString(),
    //              WrapperRestoreReportResource.Wrapper_ObjectNotSupportedWithAppProfile, objectIdentity)
    //    {
    //    }
    //}
    #endregion
}
