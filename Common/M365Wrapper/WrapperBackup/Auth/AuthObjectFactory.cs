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
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AvePoint.Application.AosApi.Invoker;
    //using AvePoint.Application.TokenManager.TokenManagement;
    using AvePoint.GCommon.Contract.CentralAdmin.Object;

    using AvePoint.RA.CommonUtil;
    using Util.MSAzure;

    public class AuthObjectFactory
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(AuthObjectFactory));

        public TokenManagementService TokenManagementService = new TokenManagementService();

        public AuthObject CreateAOSAuthObjectsForExchagnePS(BposInfo info, ImpersonateUserInfo impersonateUserInfo = null)
        {
            switch (info.ConnectionType)
            {
                //case BposConnectionType.Both:
                //    if (info.UserAccountInfo.ServiceAccountIsMFA)
                //    {
                //        logger.Info("Service account {0} is MFA user, so ps use app.", info.UserAccountInfo.ServiceAccountUsername);
                //        return GenerateAosAppTokenAuthObject(info, AuthResourceType.ExchangePowerShell, impersonateUserInfo);
                //    }
                //    else
                //    {
                //        return GenerateAosServiceAccountAuthObject(info, AuthResourceType.ExchangePowerShell, impersonateUserInfo);
                //    }
                case BposConnectionType.ServiceAccount:
                    return GenerateAosServiceAccountAuthObject(info, AuthResourceType.ExchangePowerShell, impersonateUserInfo);
                case BposConnectionType.AppToken:
                    return GenerateAosAppTokenAuthObject(info, AuthResourceType.ExchangePowerShell, impersonateUserInfo);
                default:
                    throw new InvalidOperationException("Unreachable code.");
            }
        }

        public (AuthObject AppTokenAuthObject, AuthObject ServiceAccountAuthObject, List<AuthObject> DelegateAppAuthObject4ServiceAccounts, List<AuthObject> DelegateAppAuthObject4AppTokens) CreateAOSAuthObjects(BposInfo info, AuthResourceType resourceType, ImpersonateUserInfo impersonateUserInfo = null)
        {
            AuthObject appTokenAuth = null;
            AuthObject serviceAccountAuth = null;
            List<AuthObject> delegateAppAuth4ServiceAccounts = null;
            List<AuthObject> delegateAppAuth4AppTokens = null;
            switch (info.ConnectionType)
            {
                case BposConnectionType.ServiceAccount:
                    serviceAccountAuth = GenerateAosServiceAccountAuthObject(info, resourceType, impersonateUserInfo);
                    delegateAppAuth4ServiceAccounts = GenerateAosDelegateApp4ServiceAccountAuthObjects(info, resourceType, impersonateUserInfo);
                    break;
                case BposConnectionType.AppToken:
                    appTokenAuth = GenerateAosAppTokenAuthObject(info, resourceType, impersonateUserInfo);
                    delegateAppAuth4AppTokens = GenerateAosDelegateApp4AppTokenAuthObjects(info, resourceType, impersonateUserInfo);
                    break;
                case BposConnectionType.Modern:
                    appTokenAuth = GenerateAosAppTokenAuthObject(info, resourceType, impersonateUserInfo);
                    delegateAppAuth4AppTokens = GenerateAosDelegateApp4AppTokenAuthObjects(info, resourceType, impersonateUserInfo);
                    break;
                //case BposConnectionType.Both:
                //    appTokenAuth = GenerateAosAppTokenAuthObject(info, resourceType, impersonateUserInfo);
                //    if (info.UserAccountInfo.ServiceAccountIsMFA)
                //    {
                //        logger.Info("Service account {0} is MFA user, so skiped.", info.UserAccountInfo.ServiceAccountUsername);
                //    }
                //    else
                //    {
                //        serviceAccountAuth = GenerateAosServiceAccountAuthObject(info, resourceType, impersonateUserInfo);
                //    }
                //    delegateAppAuth4ServiceAccounts = GenerateAosDelegateApp4ServiceAccountAuthObjects(info, resourceType, impersonateUserInfo);
                //    delegateAppAuth4AppTokens = GenerateAosDelegateApp4AppTokenAuthObjects(info, resourceType, impersonateUserInfo);
                //    break;
                default:
                    throw new InvalidOperationException("Unreachable code.");
            }
            return (appTokenAuth, serviceAccountAuth, delegateAppAuth4ServiceAccounts, delegateAppAuth4AppTokens);
        }

        private AuthObject GenerateAosServiceAccountAuthObject(BposInfo info, AuthResourceType resourceType, ImpersonateUserInfo impersonateUserInfo = null)
        {
            var (azureEnv, endpoints) = GetAzureEnv(info);
            if (resourceType == AuthResourceType.MicrosoftGraph)
            {
                var graphTokenType = azureEnv is AzureEnvironment.China ? GraphTokenType.Graph : GraphTokenType.Teams;
                return InternalCreate(graphTokenType);
            }
            if (resourceType == AuthResourceType.ExchangePowerShell)
            {
                return InternalCreate(GraphTokenType.Outlook);
            }
            return InternalCreate(GraphTokenType.ExchangeWebService);

            AOSTokenAuthObjectV2 InternalCreate(GraphTokenType graphTokenType)
            {
                logger.Info($"Token Resource Type: {graphTokenType}");
                return new AOSTokenAuthObjectV2(
                    TokenManagementService.CreateTokenProvider(info),
                    new AuthenticationInfo
                    {
                        Resource = ConvertToResourceUrl(endpoints, resourceType),
                        TenantId = info.UserAccountInfo.TenantId,
                        Environment = azureEnv
                    },
                    new AOSAuthInfo
                    {
                        Username = info.UserAccountInfo.Username,
                        AosTokenType = AosTokenType.ServiceAccount,
                        GraphTokenType = graphTokenType
                    },
                    info.SiteUrl,
                    azureEnv,
                    impersonateUserInfo
                    );
            }
        }

        private AuthObject GenerateAosAppTokenAuthObject(BposInfo info, AuthResourceType resourceType, ImpersonateUserInfo impersonateUserInfo = null)
        {
            var (azureEnv, endpoints) = GetAzureEnv(info);

            if (resourceType == AuthResourceType.MicrosoftGraph)
            {
                return InternalCreate(GraphTokenType.Teams);
            }
            else if (resourceType == AuthResourceType.ExchangePowerShell)
            {
                return InternalCreate(GraphTokenType.Outlook);
            }
            else
            {
                return InternalCreate(GraphTokenType.ExchangeWebService);
            }
            AOSTokenAuthObjectV2 InternalCreate(GraphTokenType graphTokenType)
            {
                logger.Info("Token Resource Type: {0}.", graphTokenType.ToString());
                return new AOSTokenAuthObjectV2(
                    TokenManagementService.CreateTokenProvider(info),
                    new AuthenticationInfo
                    {
                        Resource = ConvertToResourceUrl(endpoints, resourceType),
                        TenantId = info.UserAccountInfo.TenantId,
                        Environment = azureEnv
                    },
                    new AOSAuthInfo
                    {
                        Username = info.UserAccountInfo.Username,
                        AosTokenType = AosTokenType.SharePoint,
                        GraphTokenType = graphTokenType
                    },
                    info.SiteUrl,
                    azureEnv,
                    impersonateUserInfo)
                { IsCustomerApp = info?.AppType == AppType.CustomAzureApp };
            }
        }

        private List<AuthObject> GenerateAosDelegateApp4ServiceAccountAuthObjects(BposInfo info, AuthResourceType resourceType, ImpersonateUserInfo impersonateUserInfo = null)
        {
            return null;
            //studo
            //if (resourceType == AuthResourceType.EWS) return null;

            //return info.ExternalBposInfos
            //    ?.Where(app => app.AppType == AppType.MicrosoftDelegate)
            //    .Select(app =>
            //    {
            //        var (azureEnv, endpoints) = GetAzureEnv(info);

            //        var graphTokenType = azureEnv is AzureEnvironment.China ? GraphTokenType.Graph : GraphTokenType.Teams;
            //        logger.Info("AOS delegate app for token resource type: {0}.", graphTokenType);
            //        return new AOSTokenAuthObjectV2(
            //            TokenManagementService.CreateTokenProvider(info),
            //            new AuthenticationInfo { Resource = ConvertToResourceUrl(endpoints, resourceType), TenantId = info.UserAccountInfo.TenantId, Environment = azureEnv },
            //            new AOSAuthInfo { Username = info.UserAccountInfo.ServiceAccountUsername, AosTokenType = AosTokenType.DelegateApp, GraphTokenType = graphTokenType },
            //            info.SiteUrl,
            //            azureEnv,
            //            impersonateUserInfo)
            //        { IsDelegateApp = true, DelegateAppCloudBackupModuleType = app.DelegateAppCloudBackupModuleType };
            //    })
            //    .Cast<AuthObject>()
            //    .ToList();
        }

        private List<AuthObject> GenerateAosDelegateApp4AppTokenAuthObjects(BposInfo info, AuthResourceType resourceType, ImpersonateUserInfo impersonateUserInfo = null)
        {
            return null;
            //studo
            //if (resourceType == AuthResourceType.EWS) return null;

            //return info.ExternalBposInfos
            //    ?.Where(app => app.AppType == AppType.MicrosoftDelegate)
            //    .Select(app =>
            //    {
            //        var (azureEnv, endpoints) = GetAzureEnv(info);
            //        return new AOSTokenAuthObjectV2(
            //            TokenManagementService.CreateTokenProvider(info),
            //            new AuthenticationInfo { Resource = ConvertToResourceUrl(endpoints, resourceType), TenantId = info.UserAccountInfo.TenantId, Environment = azureEnv },
            //            new AOSAuthInfo { Username = app.AppProfileUsername, AosTokenType = AosTokenType.DelegateApp, GraphTokenType = GraphTokenType.Delegate },
            //            info.SiteUrl,
            //            azureEnv,
            //            impersonateUserInfo)
            //        { IsCustomerApp = info?.AppType == AppType.CustomAzureApp, IsDelegateApp = true, DelegateAppCloudBackupModuleType = app.DelegateAppCloudBackupModuleType };
            //    })
            //    .Cast<AuthObject>()
            //    .ToList();
        }
       
        private static string ConvertToResourceUrl(Endpoints endpoints, AuthResourceType resourceType)
        {
            return resourceType switch
            {
                AuthResourceType.EWS or AuthResourceType.ExchangePowerShell => endpoints.ExchangeWeb,
                AuthResourceType.MicrosoftGraph => endpoints.MicrosoftGraph,
                _ => throw new ArgumentNullException(nameof(resourceType)),
            };
        }

        private static (AzureEnvironment, Endpoints) GetAzureEnv(BposInfo info)
        {
            return info.UserAccountInfo.AADEnvironment switch
            {
                AADEnvironment.USGovernment => (AzureEnvironment.USGovGCCHigh, Endpoints.USGovGCCHigh),
                AADEnvironment.AzureChinaCloud => (AzureEnvironment.China, Endpoints.China),
                AADEnvironment.AzureGermanyCloud => (AzureEnvironment.Germany, Endpoints.Germany),
                AADEnvironment.USGovernment_DoD => (AzureEnvironment.USGovDoD, Endpoints.USGovDoD),
                //AADEnvironment.USGovernment => (AzureEnvironment.GCC, Endpoints.GCC),
                _ => (AzureEnvironment.Worldwide, Endpoints.Worldwide),
            };
        }

        private static AosTokenType GetTokenType(AppType appType)
        {
            switch (appType)
            {
                case AppType.Exchange:
                    return AosTokenType.Exchange;
                case AppType.SharePoint:
                    return AosTokenType.SharePoint;
                case AppType.Office365:
                    return AosTokenType.Office365;
                //studo::case AppType.CBForExchangeApp:
                    return AosTokenType.CBForExchangeApp;
                    //studo::case AppType.CBForSharePointApp:
                    return AosTokenType.CBForSharePointApp;
                    //studo::case AppType.CBForM365:
                    return AosTokenType.CBForM365;
                case AppType.CustomAzureApp:
                    return AosTokenType.CustomAzureApp;
                default:
                    return AosTokenType.Office365;
            }
        }
    }

    public enum AuthResourceType
    {
        None = 0,
        EWS = 1,
        //Graph = 2,
        MicrosoftGraph = 3,//Rest Api
        ExchangePowerShell = 4
    }
}