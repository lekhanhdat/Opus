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
    using AvePoint.Application.AosApi.Invoker;
    using AvePoint.Wrapper.Common;
    using Microsoft.Exchange.WebServices.Data;
    using Microsoft.Identity.Client;
    using Microsoft365.Authentication.ADAL;
    using Microsoft365.Authentication.TokenProvider;
    using AvePoint.RA.CommonUtil;

    using System;
    using System.Threading.Tasks;
    using Util.MSAzure;
    using M365.Wrapper.Backup.Auth.Common;

    public class AOSTokenAuthObjectV2 : ServiceAccout2AppTokenAuthObject
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(AOSTokenAuthObjectV2));
        private readonly AosTokenType aosTokenType;
        private readonly GraphTokenType graphTokenType;

        public  IATokenProviderBase TokenProvider { get; init; }

        internal AOSTokenAuthObjectV2(IATokenProviderBase tokenProvider, AuthenticationInfo authenticationInfo, AOSAuthInfo aosAuthInfo, string ewsServiceUrl, AzureEnvironment cloudType, ImpersonateUserInfo impersonateUserInfo = null)
            : base(authenticationInfo, aosAuthInfo.Username, ewsServiceUrl, cloudType, impersonateUserInfo)
        {
            if (string.IsNullOrEmpty(authenticationInfo?.TenantId)) throw new ArgumentNullException("authenticationInfo.TenantId");
            TokenProvider = tokenProvider;
            aosTokenType = aosAuthInfo.AosTokenType;
            graphTokenType = aosAuthInfo.GraphTokenType;
        }

        public override AuthObjectType AuthType
            =>
            aosTokenType switch
            {
                AosTokenType.ServiceAccount => AuthObjectType.PasswordAccessToken,
                _ => AuthObjectType.AccessToken
            };

        public override TokenPermissionType PermissionType
            =>
            (aosTokenType, IsDelegateApp) switch
            {
                (AosTokenType.ServiceAccount, _) or (_, true) => TokenPermissionType.Delegated,
                _ => TokenPermissionType.Application
            };

        public override string GetAccessToken()
        {
            RefreshToken();
            return accessToken;
        }

        public bool RefreshToken()
        {
            accessToken = (aosTokenType, graphTokenType) switch
            {
                (AosTokenType.DelegateApp, GraphTokenType.Delegate) => TokenProvider.GetGraphTokenAsync(MSGraphTokenType.MicrosoftDelegate).ExecuteAsyncTask()?.AccessToken,
                (AosTokenType.DelegateApp, _) => TokenProvider.GetGraphTokenAsync(MSGraphTokenType.MicrosoftDelegateCombineServiceAccount).ExecuteAsyncTask()?.AccessToken,
                (AosTokenType.ServiceAccount, GraphTokenType.ExchangeWebService) => TokenProvider.GetEwsTokenAsync(EwsTokenType.DelegateBear).ExecuteAsyncTask()?.AccessToken,
                (_, GraphTokenType.ExchangeWebService) => TokenProvider.GetEwsTokenAsync(EwsTokenType.ApplicationBear).ExecuteAsyncTask()?.AccessToken,
                (AosTokenType.ServiceAccount, GraphTokenType.Outlook) => TokenProvider.GetOutlookTokenAsync(OutlookTokenType.DelegateBear).ExecuteAsyncTask()?.AccessToken,
                (_, GraphTokenType.Outlook) => TokenProvider.GetOutlookTokenAsync(OutlookTokenType.ApplicationBear).ExecuteAsyncTask()?.AccessToken,
                (_, GraphTokenType.TeamsSkype) => TokenProvider.GetTeamsSkypeTokenAsync(TeamsSkypeTokenType.DelegateUserBear).ExecuteAsyncTask()?.AccessToken,
                (AosTokenType.ServiceAccount, _) => TokenProvider.GetGraphTokenAsync(MSGraphTokenType.DelegateGroupTeamBear).ExecuteAsyncTask()?.AccessToken,
                _ => TokenProvider.GetGraphTokenAsync(MSGraphTokenType.ApplicationBear).ExecuteAsyncTask()?.AccessToken
            };

            if (accessToken.IsNullOrEmpty())
            {
                logger.Error("Something went wrong and the access token could not be obtained.");
                throw new AccessTokenException("Wrapper_JobFailedUnexpected");
            }
            return true;
        }

        public override void BindToExchangeService(ExchangeService service)
        {
            RefreshToken();
            service.Credentials = new OAuthCredentials(this.accessToken);
        }

        public override void BindToPOXAutoDiscoverService(POXAutodiscoverService poxAutodiscoverService)
        {
            RefreshToken();
            poxAutodiscoverService.Credentials = new POXCredential(this.accessToken);
        }

        public override void AddImpersonationHeader(ExchangeService service, string mailbox)
        {
            service.HttpHeaders[ExchangeConstants.IMPERSONATION_HEADER_NAME] = mailbox;
        }

        public bool TestConnectivity(bool throwException = false)
        {
            if (TestConnectivity(out var exception))
                return true;
            if (throwException)
                throw exception;
            return false;
        }

        public bool TestConnectivity(out Exception exception)
        {
            exception = null;
            try
            {
                RefreshToken();
                if (string.IsNullOrEmpty(accessToken))
                    throw new ArgumentNullException("AccessToken");
                return true;
            }
            catch (MsalException msalEx) when (msalEx.ErrorCode.EqualsIgnoreCase("invalid_grant") && msalEx.Message.Contains("invalid username or password"))
            {
                logger.Error($"TestConnection: {msalEx}");
                //studoexception = new NoAuthenticationException();
            }
            catch (AdalException adalEx) when (adalEx.ErrorCode.EqualsIgnoreCase("invalid_grant"))
            {
                logger.Error($"TestConnection: {adalEx}");
                //studo::exception = new NoAuthenticationException();
            }
            catch (Exception e) when (e.Message.Contains("Can't find active build-in app profile", StringComparison.InvariantCultureIgnoreCase))
            {
                logger.Error($"TestConnection: {e}");
                exception = new Exception("Agent.Common.AppProfileNotExist_5ff7f8f3-c778-4912-a992-d6a9a27cf5c1");
            }
            catch (Exception e)
            {
                logger.Error($"TestConnection: {e}");
                exception = new Exception("Wrapper_JobFailedUnexpected");
            }
            return false;
        }

    }
}