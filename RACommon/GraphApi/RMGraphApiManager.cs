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
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Graph;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Tenant;
using AvePoint.Wrapper.Common;
using Cloud.Sdk.Data.AosModern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.GraphApi
{
    public abstract class RMGraphApiManager
    {

        protected const string ApiVersion = "v1.0";

        protected const string BetaApi = "beta";
        public RALogger logger = RALogger.GetInstance(typeof(RMGraphApiManager));
        public AppProfileInfo Profile { get; }
        public AveBPOSAccountInfo AccountInfo { get; }
        public string GraphEndPoint { get; }
        public bool useServiceAccount { get; set; }
        public TokenType tokenType = TokenType.ApplicationToken;
        protected string AccessToken => GetAccessToken(TokenType.ApplicationToken);
        protected string DelegateAccessToken => GetAccessToken(TokenType.DelegatedToken);

        public RMGraphApiManager(string o365TenantId,bool useDelegateToken = false)
        {
            if (useDelegateToken)
            {
                Profile = RMAosApiClient.GetCustomDelegateAppProfile(TenantLocalValue.LogonGroupId, o365TenantId).GetAwaiter().GetResult();
                logger.Info($"the backup mainbox RMGraphApiManager use Delegate profile id:{Profile?.Id},app id:{Profile?.AppId}, appType: {Profile?.Type}");
                GraphEndPoint = EndpointUtil.GetGraphEndpoint(Profile.AADEnvironment);
            }
            else
            {
                Profile = RMAosApiClient.GetHighLevelPermissionAppProfile(TenantLocalValue.LogonGroupId, o365TenantId).GetAwaiter().GetResult();
                logger.Info($"the backup mainbox RMGraphApiManager use App profile id:{Profile?.Id},app id:{Profile?.AppId}, appType: {Profile?.Type}");
                GraphEndPoint = EndpointUtil.GetGraphEndpoint(Profile.AADEnvironment);
            }
        }
        public RMGraphApiManager(AppProfileInfo profile)
        {
            Profile = profile;
            GraphEndPoint = EndpointUtil.GetGraphEndpoint(Profile.AADEnvironment);
        }
        public RMGraphApiManager(AveBPOSAccountInfo accountInfo)
        {
            AccountInfo = accountInfo;
            GraphEndPoint = EndpointUtil.GetServiceAccountGraphEndpoint(AccountInfo.AADEnvironment);
            useServiceAccount = true;
        }
        private string GetAccessToken(TokenType tokenTypeParam = TokenType.ApplicationToken)
        {
            string token = string.Empty;
            if (useServiceAccount)
            {
                token = CacheService.Get(CacheNamespace.O365AccessToken, AccountInfo.TenantId + "AOS" + AccountInfo.TenantGroupId);
                if (!string.IsNullOrEmpty(token))
                {
                    return token;
                }
                token = AccountInfo.TokenProvider.GetToken(new Uri(GraphEndPoint));
                token = token.Replace("Bearer ", string.Empty);
                CacheService.Set(CacheNamespace.O365AccessToken, AccountInfo.TenantId + "AOS" + AccountInfo.TenantGroupId, token, DateTime.MaxValue);
                return token;
            }
            else
            {
                string tokenTypeString = string.Empty;
                if (tokenTypeParam == TokenType.DelegatedToken)
                {
                    logger.Info("this GetAccessToken will get the token type is:DelegatedToken");
                    tokenTypeString = "AOSDelegatedToken";
                }
                else
                {
                    logger.Info($"this GetAccessToken will get the token type is:ApplicationToken");
                    tokenTypeString = "AOSApplicationToken";
                }
                token = CacheService.Get(CacheNamespace.O365AccessToken, Profile.TenantId + tokenTypeString + Profile.Id);
                if (!string.IsNullOrEmpty(token))
                {
                    return token;
                }
                var tokenResult = RMAosApiClient.GetO365AccessToken(Profile, tokenTypeParam);
                if (tokenResult != null)
                {
                    token = tokenResult.AccessToken;
                    CacheService.Set(CacheNamespace.O365AccessToken, Profile.TenantId + tokenTypeString + Profile.Id, token, tokenResult.ExpiresOn.UtcDateTime);
                    return token;
                }
            }
            throw new Exception($"Error occurred while get graph token.");
        }
    }
}
