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
    using AvePoint.GCommon;

    class UserCredentialAPSTokenManager : IAPSTokenManager
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(UserCredentialAPSTokenManager), false);
        private static readonly string Policy = "MCMBI";

        private AveBPOSAccountInfo account;
        private string token;
        private BecWebServiceInstance becWebServiceInstance;

        public UserCredentialAPSTokenManager(AveBPOSAccountInfo account, BecWebServiceInstance becWebServiceInstance)
        {
            this.account = account;
            this.becWebServiceInstance = becWebServiceInstance;
        }

        private string GetToken()
        {
            string token = null;

            logger.Info("start to get token with user:{0}", account.UserName);
            ISharePointOnlineAuthenticationProvider sharePointOnlineAuthenticationProvider = SharePointOnlineAuthenticationProviderHelper.CreateDefaultProvider();
            token = sharePointOnlineAuthenticationProvider.GetADToken(becWebServiceInstance.GetBecWebServiceLogonSiteName(), Policy, account.UserName, account.Password);
            logger.Info("finish to get token with user:{0}", account.UserName);

            return token;
        }

        public string Token
        {
            get
            {
                if (token == null)
                {
                    token = GetToken();
                }

                return token;
            }
        }

        public APSTokenType TokenType
        {
            get
            {
                return APSTokenType.LiveId;
            }
        }

        public override string ToString()
        {
            return account.UserName;
        }
    }
}
