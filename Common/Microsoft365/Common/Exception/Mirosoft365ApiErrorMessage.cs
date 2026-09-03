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
namespace Microsoft365.Common.Exception
{
    public static class Mirosoft365ApiErrorMessage
    {
        #region error code for Office365ApiErrorCode
        private const string TokenProviderNotSupported = "The {0} token provider is not supported.";
        private const string InvalidFolder = "The folder {0} is not a valid list item.";
        private const string UserNotFoundInOffice365 = "The user:{0} is not available in tenant:{1}";
        private const string AuthenticationFailed = "If the multi-factor authentication is enabled for user:{0}. Please navigate to AOS to add an app profile with particular Office 365 tenant, and make sure the app password is used for DAOL to access SharePoint Online and Exchange Online. Otherwise, please verify the username and password is correct and the account has enough permission to access the resource. More details: Office 365 tenant id:{1}, customer id:{2}, client id:{3}, aosApiUrl:{4}";
        private const string AIRMSIPCClientNotFound = "The RMS client was not found, please install it first.";
        private const string AIRMSIPCClientLoadFailed = "Set MSIPC dll directory failed with {0}";
        private const string AIRSuperUserNotConfigured = "Please configure the super user feature to unprotect the protected content.";
        #endregion error code for Office365ApiErrorCode

        #region error code for other exceptiontype
        private const string InvalidEmail = "The string {0} is not email.";
        private const string NotAbsoluteUrl = "The url {0} is not absolute.";
        #endregion error code for other exceptiontype

        #region error code for Office365ApiErrorCode format
        public static string TokenProviderNotSupportedFormat(params object[] args)
        {
            return string.Format(TokenProviderNotSupported, args);
        }
        public static string InvalidFolderFormat(params object[] args)
        {
            return string.Format(InvalidFolder, args);
        }
        public static string UserNotFoundInOffice365Format(params object[] args)
        {
            return string.Format(UserNotFoundInOffice365, args);
        }
        public static string AuthenticationFailedFormat(params object[] args)
        {
            return string.Format(AuthenticationFailed, args);
        }
        public static string AIRMSIPCClientNotFoundFormat(params object[] args)
        {
            return string.Format(AIRMSIPCClientNotFound, args);
        }
        public static string AIRMSIPCClientLoadFailedFormat(params object[] args)
        {
            return string.Format(AIRMSIPCClientLoadFailed, args);
        }
        public static string AIRSuperUserNotConfiguredFormat(params object[] args)
        {
            return string.Format(AIRSuperUserNotConfigured, args);
        }

        #endregion error code for Office365ApiErrorCode format

        #region error code for other exceptiontype format
        public static string InvalidEmailFormat(params object[] args)
        {
            return string.Format(InvalidEmail, args);
        }
        public static string NotAbsoluteUrlFormat(params object[] args)
        {
            return string.Format(NotAbsoluteUrl, args);
        }
        #endregion error code for other exceptiontype format

        #region error message from idclr exception

        private const string CannotContactSite = "Cannot contact site at the specified URL {0}.";
        private const string PPCRL_REQUEST_E_UNKNOWN = "Unable to get ticket due to unknown error.";
        private const string CannotGetCookie = "Cannot get cookie for URL '{0}'.";
        private const string InvalidIdcrlHeader = "The IDCRL response header from server '{0}' is not valid. The response header value is '{1}'. The response status code is '{2}'. All response headers are '{3}'.";
        private const string SharePointClientCredentialsNotSupported = "Cannot contact web site '{0}' or the web site does not support SharePoint Online credentials. The response status code is '{1}'. The response headers are '{2}'.";

        public static string CannotContactSiteFormat(params object[] args)
        {
            return string.Format(CannotContactSite,args);
        }
        public static string PPCRL_REQUEST_E_UNKNOWNFormat(params object[] args)
        {
            return PPCRL_REQUEST_E_UNKNOWN;
        }
        public static string CannotGetCookieFormat(params object[] args)
        {
            return string.Format(CannotGetCookie, args);
        }
        public static string InvalidIdcrlHeaderFormat(params object[] args)
        {
            return string.Format(InvalidIdcrlHeader, args);
        }
        public static string SharePointClientCredentialsNotSupportedFormat(params object[] args)
        {
            return string.Format(SharePointClientCredentialsNotSupported, args);
        }
        #endregion

    }
}