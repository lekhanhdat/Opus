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
namespace ExchangeUtility.Graph
{
    using AvePoint.GCommon.Contract.CentralAdmin.Object;
    using Microsoft365.Authentication.TokenProvider;
    using AvePoint.RA.CommonUtil;
    using System;
    using System.Threading.Tasks;
    using Util.MSAzure;

    public class YammerAppTokenAuthObject : IAuthObject
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(YammerAppTokenAuthObject));
        private readonly IATokenProviderBase tokenProvider;

        private YammerAppTokenAuthObject(string userName)
        {
            UserName = userName;
        }

        //studo::
        public YammerAppTokenAuthObject(IATokenProviderBase tokenProvider, BposUserAccountInfo yammerBPOSInfo, string customerId) : this(yammerBPOSInfo.Username)
        {
            if (customerId.IsNullOrEmpty()) return;
            this.tokenProvider = tokenProvider;
        }

        public string GetAccessToken()
        {
            RefreshToken();
            return accessToken;
        }

        public void RefreshToken()
        {
            //studo::accessToken = tokenProvider.GetVivaEngageTokenAsync().ExecuteAsyncTask()?.AccessToken;
            if (accessToken.IsNullOrEmpty())
            {
                logger.Error("Failed to get yammer token from AOS.");
                throw new AccessTokenException("Wrapper_JobFailedUnexpected");
            }
        }

        public string UserName { get; init; }

        public AuthObjectType AuthType => AuthObjectType.YammerAppToken;

        public AzureEnvironment Environment => throw new NotImplementedException();

        private string accessToken;
    }
}