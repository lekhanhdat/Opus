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

using AvePoint.GCommon;
using Microsoft.Identity.Client;
using Microsoft365.Authentication;
using Microsoft365.Authentication.ServiceEndPoint;
using System;
using System.Reflection;
using System.Security;

namespace AvePoint.Wrapper.Common.Graph
{
    public static class GraphTokenProviderFactory
    {
        private static IAveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public static IGraphTokenProvider CreateProvider(AveBPOSAccountInfo bposInfo)
        {
            switch (bposInfo.ConnectionType)
            {
                case BposConnectionType.ServiceAccount:
                    var sa2AppAuthInfo = bposInfo.ConvertToSA2AppAuthInfo();
                    return new ServiceAccountGrapahTokenProvider(sa2AppAuthInfo);
                case BposConnectionType.AppToken:
                    var appOnlyAuthInfo = bposInfo.ConvertToAppOnlyAuthInfo();
                    return new AppOnlyGrapahTokenProvider(appOnlyAuthInfo);
                default:
                    return null;
            }
        }

        public static IGraphTokenProvider CreateDriveGraphProvider(AveBPOSAccountInfo bposInfo)
        {
            var connectionType = GetConnectionType(bposInfo, false);
            switch (connectionType)
            {
                case BposConnectionType.ServiceAccount:
                    var msoInstance = MicrosoftOnlineInstanceExtension.ResoveEnvironment(bposInfo.UserName, bposInfo.AADEnvironment);
                    if (msoInstance.Item2 == null)
                    {
                        log.Info("MicrosoftOnlineInstance is null");
                        if (bposInfo.AADEnvironment == AveAzureEnvironment.None && (!string.IsNullOrEmpty(bposInfo.UserName)))
                        {
                            bposInfo.AADEnvironment = Office365Discover.GetEnvironment(bposInfo.UserName);
                        }
                        msoInstance = new Tuple<AveAzureEnvironment, MicrosoftOnlineInstanceDetail>(
                            bposInfo.AADEnvironment,
                            MicrosoftOnlineInstanceExtension.GetMsoInstance(bposInfo.AADEnvironment));
                        log.Info($"New AADEnvironment {bposInfo.AADEnvironment}");
                    }
                    var sa2AppAuthInfo = new SA2AppAuthInfo(
                            bposInfo.UserName,
                            bposInfo.Password.ToPlainString(),
                            msoInstance.Item2.AdalAuthorityEndpointUrl,
                            msoInstance.Item2.AdalMsGraphServiceResource,
                            "d3590ed6-52b3-4102-aeff-aad2292ab01c",
                            msoInstance.Item1);
                    return new ServiceAccountGrapahTokenProvider(sa2AppAuthInfo);
                    //return new ServiceAccountGrapahTokenProvider(bposInfo.CustomerId, bposInfo.TenantId, bposInfo.UserName, sa2AppAuthInfo, GraphTokenType.ExchangeGraph);
                case BposConnectionType.Both:
                case BposConnectionType.AppToken:
                    var appOnlyAuthInfo = bposInfo.ConvertToAppOnlyAuthInfo();
                    return new AppOnlyGrapahTokenProvider(appOnlyAuthInfo);
                    //return new AppOnlyGrapahTokenProvider(bposInfo.CustomerId, bposInfo.TenantId, bposInfo.AppType == GCommon.Contract.CentralAdmin.Object.AppType.CustomAzureApp ? bposInfo.CustomAppId : bposInfo.UserName, bposInfo.AppType, appOnlyAuthInfo);
                default:
                    return null;
            }
        }

        private static BposConnectionType GetConnectionType(AveBPOSAccountInfo bposInfo, bool preferServiceAccount)
        {
            if (bposInfo.ConnectionType == BposConnectionType.Both && preferServiceAccount) return BposConnectionType.ServiceAccount;
            return bposInfo.ConnectionType;
        }

        private static AppOnlyAuthInfo ConvertToAppOnlyAuthInfo(this AveBPOSAccountInfo accountInfo)
        {
            var msoInstance = MicrosoftOnlineInstanceExtension.GetMsoInstance(accountInfo.AADEnvironment);
            return new AppOnlyAuthInfo(
                accountInfo.ClientId,
                accountInfo.AppCert,
                accountInfo.TenantId,
                msoInstance.AdalAuthorityEndpointUrl,
                msoInstance.AdalMsGraphServiceResource,
                accountInfo.AuthenticationProfileId);
        }

        private static SA2AppAuthInfo ConvertToSA2AppAuthInfo(this AveBPOSAccountInfo accountInfo)
        {
            var msoInstance = MicrosoftOnlineInstanceExtension.ResoveEnvironment(accountInfo.UserName, accountInfo.AADEnvironment);
            if (msoInstance.Item2 == null)
            {
                log.Info("MicrosoftOnlineInstance is null");
                if (accountInfo.AADEnvironment == AveAzureEnvironment.None && (!string.IsNullOrEmpty(accountInfo.UserName)))
                {
                    accountInfo.AADEnvironment = Office365Discover.GetEnvironment(accountInfo.UserName);
                }
                msoInstance = new Tuple<AveAzureEnvironment, MicrosoftOnlineInstanceDetail>(
                accountInfo.AADEnvironment,
                MicrosoftOnlineInstanceExtension.GetMsoInstance(accountInfo.AADEnvironment));
                log.Info($"New AADEnvironment {accountInfo.AADEnvironment}");
            }
            //log.Info($@"[ConvertToSA2AppAuthInfo]User:{accountInfo.UserName},Environment:{msoInstance.Item1},Endpoint:{msoInstance.Item2.AdalAuthorityEndpointUrl},Resource:{msoInstance.Item2.AdalMsGraphServiceResource}");
            return new SA2AppAuthInfo(
                accountInfo.UserName,
                accountInfo.Password.ToPlainString(),
                msoInstance.Item2.AdalAuthorityEndpointUrl,
                msoInstance.Item2.AdalMsGraphServiceResource,
                MicrosoftOnlineInstanceExtension.GetAADPublicClientIdByEnvironment(msoInstance.Item1),
                msoInstance.Item1);
        }

        internal static TokenItem ConvertToTokenItem(this AuthenticationResult result)
        {
            return new TokenItem(result.AccessToken, result.TokenType, result.ExpiresOn);
        }
    }
}
