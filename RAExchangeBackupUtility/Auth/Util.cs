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

namespace ExchangeUtility
{
    using AvePoint.RA.CommonUtil;
    using System;
    using System.Collections.Generic;
    using System.Security;

    public static class Util
    {
        public static SecureString ToSecureString(this string value)
        {
            var ss = new SecureString();
            foreach (var c in value)
            {
                ss.AppendChar(c);
            }
            return ss;
        }

        /// <summary>
        /// This method force run auto discover. 
        /// It is recommended to do AutoDiscover and cache the EWS URL then set it on the service object the next time you need to set the EWS URL on the service object until calls fail using that URL.
        /// </summary>
        /// <param name="authObj"></param>
        /// <returns></returns>
        public static Uri AutoDiscoverServiceUrl(AuthObject authObj, string emailAddress)
        {
            return new AutoDiscoverObj(authObj).Run(emailAddress);
        }

        public static void SetRetry(int maxRetryCount, Microsoft.Exchange.WebServices.Data.ExchangeService service, AuthObject AuthObject)
        {
            service.RetryController = AssemblyRetryController(maxRetryCount, service, AuthObject);
        }
        private static Microsoft.Exchange.WebServices.Data.IRetryable AssemblyRetryController(int maxRetryCount, Microsoft.Exchange.WebServices.Data.ExchangeService service, AuthObject AuthObject)
        {
            #region Data flow for basic retry implement
            /// 
            ///  User context                                                   User context 
            ///             ∨                                                  ∧
            /// ExceptionWrapper (Wrap wellknown error to user friendly format) ExceptionWrapper
            ///             ∨                                                  ∧
            ///         Retryable     (wait and retry n times when error)       Retryable
            ///             ∨                                                  ∧
            /// AADTokenRefresher     (Refress access token when expired)       AADTokenRefresher
            ///             ∨                                                  ∧
            ///            Exchange Web Service(Microsoft.Exchange.WebServive.DLL)
            #endregion
            Microsoft.Exchange.WebServices.Data.IRetryable retryable = null;
            if (AuthObject.AuthType == AuthObjectType.AccessToken)
            {
                retryable = new AADTokenRefresher(AuthObject as AppTokenAuthObject, service, retryable); //refresh add token when expired
            }
            retryable = new Retryable(maxRetryCount, retryable); //retry for EWS error
            retryable = new ExceptionWrapper(new FormattedMessageException.Context() { AuthObject = AuthObject }, retryable); //wrapper EWS exception and format msg
            return retryable;
        }

        class AutoDiscoverObj : ExchangeObjectBase
        {
            public AutoDiscoverObj(AuthObject authObj) : base(authObj)
            {
            }

            public Uri Run(string emailAddress)
            {
                var service = CreateExchangeService(-1);
                RunAutoDiscoverUrl(service, emailAddress);
                return service.Url;
            }
        }
    }

    static class EXOPowerShellUtil
    {
        static RALogger logger = RALogger.GetInstance(typeof(EXOPowerShellUtil));
        //const string CNConnectionUrl = "https://partner.outlook.cn/PowerShell/";
        //const string CommonConnectionUrl = "https://outlook.office365.com/powershell-liveid/";//"https://ps.outlook.com/powershell/";
        //const string DEConnectionUrl = "https://outlook.office.de/powershell";
        //const string InitialDomainNameSuffixCN = "partner.onmschina.cn";
        //const string InitialDomainNameSuffixDE = "onmicrosoft.de";

        public static string GetConnectionUri(string upn)
        {
            var env = AzureEnvironment.FromDomainOrPrincipalName(upn);
            if (env == null)
            {
                logger.Warn("No information of the user {0}.", upn);
                return AzureEnvironment.DefaultCloud.PSConnectionUrl;
            }
            else
            {
                return env.PSConnectionUrl;
            }
        }
    }

    class AutoDiscoverCallback
    {
        static RALogger logger = RALogger.GetInstance(typeof(AutoDiscoverCallback));

        private HashSet<string> urlList = new HashSet<string>();

        public bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            if (urlList.Contains(redirectionUrl))
            {
                logger.Info("Redirection url list contain {0}.", redirectionUrl);
                return false;
            }
            else
            {
                urlList.Add(redirectionUrl);
                return true;
            }
        }
    }

    public static class ExchangePathExtension
    {
        public static string ToParentInternalPath(this string internalPath)
        {
            return internalPath.Substring(0, internalPath.LastIndexOf(ExchangeConstants.PathParser));
        }

        public static string ToDisplayPath(this string internalPath)
        {
            return internalPath.Replace(ExchangeConstants.PathParser, ExchangeConstants.PathCombineChar);
        }

        public static string ToTitle(this string internalPath)
        {
            var index = internalPath.LastIndexOf(ExchangeConstants.PathParser);
            if (index > 0)
            {
                return internalPath.Substring(index + 1);
            }
            return internalPath;
        }
    }

    static class StringExtension
    {
        public static string GetDomain(this string emailAddressOrEmailDomain)
        {
            if (string.IsNullOrEmpty(emailAddressOrEmailDomain)) throw new ArgumentNullException(nameof(emailAddressOrEmailDomain));
            int index = emailAddressOrEmailDomain.LastIndexOf('@');
            if (index <= 0) return emailAddressOrEmailDomain;
            return emailAddressOrEmailDomain.Substring(index + 1);
        }
    }
}
