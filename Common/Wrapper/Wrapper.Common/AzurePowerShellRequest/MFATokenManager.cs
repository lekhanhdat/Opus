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
namespace AvePoint.Wrapper.Common
{
    using System;
    using System.Text;
    using AvePoint.GCommon;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    class MFATokenManager : IAPSTokenManager
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(MFATokenManager), false);
        private AveBPOSAccountInfo account;
        private IAPSTokenManager[] apsTokenManagers;
        /// <summary>
        /// 如果被过滤就返回true
        /// </summary>
        private Func<Exception, bool>[] filterOutExceptionHandlers;

        private IAPSTokenManager validAPSTM;

        public MFATokenManager(AveBPOSAccountInfo account, BecWebServiceInstance becWebServiceInstance, string customerId, string tenantId, string aosApiUrl, string clientId)
        {
            this.account = account;
            apsTokenManagers = new IAPSTokenManager[]
            {
                new UserCredentialAPSTokenManager(account, becWebServiceInstance),
                //new AppAPSTokenManager(account),
                new AppOnlyAPSTokenManager(account, customerId, tenantId, aosApiUrl, clientId)
            };

            filterOutExceptionHandlers = new Func<Exception, bool>[]
            {
                UserCredentialAPSTokenExchangeHandler,
                //AppAPSTokenExchangeHandler,
                AppOnlyAPSTokenExchangeHandler
            };
        }

        private bool UserCredentialAPSTokenExchangeHandler(Exception ex)
        {
            var idcrlException = (Microsoft.SharePoint.Client.IdcrlException)ex;

            if (idcrlException != null)
            {
                if (idcrlException.ErrorCode == IdcrlErrorCodes.CUSTOM_MFA_REQUIRE_STRONG_PASSWORD ||
                    idcrlException.ErrorCode == IdcrlErrorCodes.PPCRL_REQUEST_E_BAD_MEMBER_NAME_OR_PASSWORD)
                {
                    return true;
                }
            }

            return false;
        }

        private bool AppAPSTokenExchangeHandler(Exception ex)
        {
            /*
             * MFA is enabled
             *      Original password -->
             *          Error Code          :   interaction_required
             *          ServerErrorCodes    :   50076 (the user is required to use multi-factor authentication to access resource.)
             *          IdlException.ErrorCode == PPCRL_REQUEST_E_BAD_MEMBER_NAME_OR_PASSWORD
             *          
             *      App password -->    (same as the wrong password)
             *          IdlException.ErrorCode == IdcrlErrorCodes.CUSTOM_MFA_REQUIRE_STRONG_PASSWORD
             *      
             *      the password is original password, if the error code is 'interaction_required',
             *      please input app password.
             *      IdlException.ErrorCode == PPCRL_REQUEST_E_BAD_MEMBER_NAME_OR_PASSWORD
             *   
             *   
             *      app password-->Error Code: invalid_grant (invalid username and password)
             *   
             *   
             * MFA is not enabled
             *      wrong password --> 
             *          Error Code          :   invalid_grant
             *          ServerErrorCodes    :   70002 (error validating credentials), 50126 (invalid username or password)
             *      right password -->
             */

            var adalException = (AdalException)ex;

            if (adalException != null)
            {
                //因为无法判断 invalid_grant是否来自于MFA，所以都尝试下
                if ("interaction_required".Equals(adalException.ErrorCode, StringComparison.OrdinalIgnoreCase) ||
                    "invalid_grant".Equals(adalException.ErrorCode, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private bool AppOnlyAPSTokenExchangeHandler(Exception ex)
        {
            return false;
        }

        public IAPSTokenManager GetValidAPSTM()
        {
            System.Collections.Generic.List<Exception> exceptions = null;

            for (var index = 0; index < apsTokenManagers.Length; index++)
            {
                try
                {
                    var tokenManager = apsTokenManagers[index];
                    var token = tokenManager.Token;

                    return tokenManager;
                }
                catch (Exception ex)
                {
                    if (index + 1 >= apsTokenManagers.Length || (!filterOutExceptionHandlers[index](ex)))
                    {
                        var stringBuilder = new StringBuilder();
                        if (exceptions != null)
                        {
                            foreach (var innerException in exceptions)
                            {
                                stringBuilder.AppendLine("Exception:" + innerException);
                            }
                        }

                        logger.Error("Get token for user:{0} failed:{1}, the previous exceptions:{2}", account.UserName, ex, stringBuilder);
                        throw;
                    }
                    else
                    {
                        logger.Warn("Get token for user:{0} failed:{1}\r\n-->The application will try another aps token manager.", account.UserName, ex.Message);

                        if (exceptions == null)
                        {
                            exceptions = new System.Collections.Generic.List<Exception>();
                        }
                        exceptions.Add(ex);
                    }
                }
            }

            throw new InvalidOperationException();
        }

        public string Token
        {
            get
            {
                if (validAPSTM == null)
                {
                    validAPSTM = GetValidAPSTM();
                }

                return validAPSTM.Token;
            }
        }

        public APSTokenType TokenType
        {
            get
            {
                if (validAPSTM == null)
                {
                    validAPSTM = GetValidAPSTM();
                }

                return validAPSTM.TokenType;
            }
        }

        public override string ToString()
        {
            return account.UserName;
        }
    }
}
