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
//using Microsoft.IdentityModel.Clients.ActiveDirectory;


using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using Cloud.Sdk.AosModern;
using Cloud.Sdk.Data.AosModern;
using Cloud.Sdk.Token;
using Microsoft.Graph.Me.GetManagedDevicesWithAppFailures;
using Microsoft.Identity.Client;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CloudAos = Cloud.Sdk.Data.AosModern;

namespace AvePoint.Wrapper.Common.Graph
{
    internal class AppOnlyGrapahTokenProvider : GraphTokenProviderBase
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(AppOnlyGrapahTokenProvider));
        public AppOnlyAuthInfo AuthInfo { get; private set; }
        public AppOnlyGrapahTokenProvider(AppOnlyAuthInfo authInfo)
            : base(authInfo.Resource)
        {
            AuthInfo = authInfo;
        }

        protected override void RefreshToken()
        {
            //var authenticationContext = new AuthenticationContext(string.Format("{0}{1}", AuthInfo.Authority, AuthInfo.TenantId), false);
            //var cac = new ClientAssertionCertificate(AuthInfo.ClientId, AuthInfo.Certificate);
            //var result = authenticationContext.AcquireTokenAsync(AuthInfo.Resource, cac).Result;
            //var app = ConfidentialClientApplicationBuilder.Create(AuthInfo.ClientId)
            //                  .WithCertificate(AuthInfo.Certificate)
            //                  .WithAuthority(string.Format("{0}{1}", AuthInfo.Authority, AuthInfo.TenantId))
            //                  .Build();
            //var result = app.AcquireTokenForClient(new[] { new Uri(AuthInfo.Resource).GetLeftPart(UriPartial.Authority).TrimEnd('/') + "/.default" }).ExecuteAsync().Result;

            var result = GetO365AccessToken(AuthInfo);
            CachedTokenItem = new TokenItem(result.AccessToken, "Bearer", result.ExpiresOn);
        }

        private static int GetAospProfilePriority(CloudAos.IdentityProviderType type)
        {
            return type switch
            {
                CloudAos.IdentityProviderType.AospCustomDelegateApp => 0,
                CloudAos.IdentityProviderType.AospSecurityAnalysis => 1,
                CloudAos.IdentityProviderType.AospSecurityAnalysisCsp => 2,
                _ => int.MaxValue,
            };
        }


        private CloudAos.TokenResult GetO365AccessToken(AppOnlyAuthInfo profile)
        {
            return Execute(() =>
            {
                var tokenApiClient = AosApiUtility.CloudSdkTokenClientFactory.CreateModernTokenApiClient(TenantLocalValue.LogonGroupId);
                var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
                var aospApp = CheckAppIdIsAOSPAppAndGetIt(client, profile.AuthenticationProfileId).GetAwaiter().GetResult();
                AppProfileInfo appProfile = null;
                if (aospApp != null)
                {
                    appProfile = aospApp;
                    var aospTokenResult = GetAOSPToken(tokenApiClient, profile, appProfile).GetAwaiter().GetResult();
                    if (!string.IsNullOrEmpty(aospTokenResult.Error))
                    {
                        mLog.Error($"An error occurred while get aosp token from AOS.error : {aospTokenResult.Error}");
                        return null;
                    }
                    return aospTokenResult;
                }
                else
                {
                    appProfile = client.AppProfileService.GetByIdAsync(profile.AuthenticationProfileId).GetAwaiter().GetResult();
                }
                if (appProfile.Status != CloudAos.AppProfileStatus.Active)
                {
                    mLog.Warn($"This app profile is not Active. App: {appProfile.Id}, Domain: {appProfile.DomainName} , Status: {appProfile.Status}.");
                    return null;
                }
                var tokenResult = tokenApiClient.ModernTokenService.GetTokenByAppProfileAsync(
                    appProfile.Type,
                    CloudAos.TokenResourceType.Graph,
                    profile.TenantId,
                    profile.AuthenticationProfileId,
                    null,
                    CloudAos.TokenType.ApplicationToken
                ).GetAwaiter().GetResult();

                if (!string.IsNullOrEmpty(tokenResult.Error))
                {
                    mLog.Error($"An error occurred while get O365 token from AOS.error : {tokenResult.Error}");
                    return null;
                }

                return tokenResult;
            });
        }
        private async Task<TokenResult> GetAOSPToken(ModernTokenApiClient tokenApiClient, AppOnlyAuthInfo profile, AppProfileInfo appProfile)
        {
            try
            {
                var result = await tokenApiClient.ImpersonateCallerInvoke<ModernTokenApiClient, TokenResult>(Cloud.Sdk.Data.Core.CallerType.PartnerPortal,
                async (tokenApiClient) =>
                {
                    return await tokenApiClient.ModernTokenService.GetTokenByAppProfileAsync(
                    appProfile.Type,
                    CloudAos.TokenResourceType.Graph,
                    profile.TenantId,
                    profile.AuthenticationProfileId,
                    null,
                    CloudAos.TokenType.ApplicationToken
                    );
                });

                return result;
            }
            catch (Exception ex)
            {
                mLog.Error($"GetAOSPToken failed, error: {ex}");
                return null;
            }
        }
        private async Task<AppProfileInfo> CheckAppIdIsAOSPAppAndGetIt(AosModernApiTenantClient client,string aospAppId)
        {
            try
            {
                var appProfile = await client.ImpersonateCallerInvoke<AosModernApiTenantClient, AppProfileInfo?>(Cloud.Sdk.Data.Core.CallerType.PartnerPortal, async (client) =>
                {

                    var profiles = await client.AppProfileService.GetByTypesAsync([IdentityProviderType.AospSecurityAnalysis, IdentityProviderType.AospSecurityAnalysisCsp, IdentityProviderType.AospCustomDelegateApp]);
                    mLog.Info($"CheckAppIdIsAOSPAppAndGetIt get all profiles,the profile count is:{profiles?.Count}");
                    foreach (var profile in profiles)
                    {
                        mLog.Info($"CheckAppIdIsAOSPAppAndGetIt.current profile info is:customerId:{TenantLocalValue.LogonGroupId},name:{profile.Name},id:{profile.Id},tenantID:{profile.TenantId},type:{(int)profile.Type}");
                    }
                    return profiles.Where(a => a.Id.Equals(aospAppId, StringComparison.OrdinalIgnoreCase) && (a.Type != CloudAos.IdentityProviderType.AospCustomDelegateApp || a.Products.Contains(CloudAos.ModernProductType.PartnerWorkspaceOnboarding)))
                    .OrderBy(p => GetAospProfilePriority(p.Type))
                    .FirstOrDefault();
                });
                if (appProfile != null)
                {
                    mLog.Info($"CheckAppIdIsAOSPAppAndGetIt appProfile is not null,return it,id:{appProfile.Id}");
                    return appProfile;
                }
                return null;
            }
            catch (Exception e)
            {
                mLog.Error($"CheckAppIdIsAOSPApp failed,error:{e}");
                return null;
            }
        }
        private static T Execute<T>(Func<T> func)
        {
            RetryPolicy retry = Policy.Handle<Exception>().WaitAndRetry(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5) });
            TimeoutPolicy timeout = Policy.Timeout(TimeSpan.FromMinutes(4), TimeoutStrategy.Pessimistic);
            var wrap = retry.Wrap(timeout);
            try
            {
                return wrap.Execute(func);
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while get data from aos. {0}, {1} {2}", func.Method.Name, e.Message, e);
                throw;
            }
        }
    }
}
