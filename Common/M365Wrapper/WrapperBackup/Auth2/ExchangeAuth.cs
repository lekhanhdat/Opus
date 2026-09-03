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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using Cloud.Sdk.Data.AosModern;
using M365.Wrapper.Backup.Auth.Common;
using System;
using Util.MSAzure;
using AADEnvironment = AvePoint.GCommon.Contract.CentralAdmin.Object.AADEnvironment;

namespace ExchangeUtility.Graph;

public static class AuthObjectFactory4TeamsJob
{
    static RALogger logger = RALogger.GetInstance(typeof(AuthObjectFactory4TeamsJob));

    public static IAppTokenAuthObject GetGraphAuthObjectForDelegateCustomApp(BposInfo info, TokenPermissionType permissionType)
    {
        return new InnerGraphApiAppTokenAuthObject(info, permissionType);
    }


    class InnerGraphApiAppTokenAuthObject : IAppTokenAuthObject
    {
        private const string Global_MicrosoftGraph_ResourceUrl = "https://graph.microsoft.com";
        private const string German_MicrosoftGraph_ResourceUrl = "https://graph.microsoft.de";
        private const string China_MicrosoftGraph_ResourceUrl = "https://microsoftgraph.chinacloudapi.cn";
        private const string Gov_MicrosoftGraph_ResourceUrl = "https://graph.microsoft.us";
        private const string Dod_MicrosoftGraph_ResourceUrl = "https://dod-graph.microsoft.us";
        private static readonly TimeSpan Token_EXPIRES_EDGE = new TimeSpan(0, 5, 0);

        private BposInfo _appInfo;
        private TokenResult _tokenResult;
        private IdentityProviderType _providerType;

        public string UserName => null;

        public AuthObjectType AuthType => AuthObjectType.AccessToken;

        public string ResourceUrl { get; }

        public TokenPermissionType PermissionType { get; }

        public AzureEnvironment Environment { get; }

        public InnerGraphApiAppTokenAuthObject(BposInfo appInfo, TokenPermissionType permissionType)
        {
            _appInfo = appInfo;
            _providerType = _appInfo.AppType == AvePoint.GCommon.Contract.CentralAdmin.Object.AppType.CloudRecords ? IdentityProviderType.CloudRecords : IdentityProviderType.CustomDelegateApp;
            Environment = GetAzureEnv(appInfo);
            ResourceUrl = GetGraphResourceUrl(Environment);
            PermissionType = permissionType;
        }

        public string GetAccessToken()
        {
            if(_tokenResult != null && _tokenResult.ExpiresOn - DateTime.UtcNow > Token_EXPIRES_EDGE)
            {
                return _tokenResult.AccessToken;
            }

            return GetAppToken();
        }

        private AzureEnvironment GetAzureEnv(BposInfo info)
        {
            if (info.ConnectionType == BposConnectionType.ServiceAccount)
            {
                throw new NotSupportedException("ServiceAccount not supported.");
            }

            //app profile使用AOS传过来的枚举
            logger.Info($"AADEnvironment:{info.UserAccountInfo.AADEnvironment}");
            switch (info.UserAccountInfo.AADEnvironment)
            {
                case AADEnvironment.AzureCloud:
                    return AzureEnvironment.Worldwide;
                case AADEnvironment.USGovernment:
                    return AzureEnvironment.USGovGCCHigh;
                case AADEnvironment.AzureChinaCloud:
                    return AzureEnvironment.China;
                case AADEnvironment.AzureGermanyCloud:
                    return AzureEnvironment.Germany;
                case AADEnvironment.USGovernment_DoD:
                    return AzureEnvironment.USGovDoD;
                case AADEnvironment.AzurePPE:
                default:
                    return AzureEnvironment.Worldwide;
            }
        }

        private string GetGraphResourceUrl(AzureEnvironment azureEnv)
        {
            switch (azureEnv)
            {
                case AzureEnvironment.China:
                    return China_MicrosoftGraph_ResourceUrl;
                case AzureEnvironment.Germany:
                    return German_MicrosoftGraph_ResourceUrl;
                case AzureEnvironment.USGovGCCHigh:
                    return Gov_MicrosoftGraph_ResourceUrl;
                case AzureEnvironment.USGovDoD:
                    return Dod_MicrosoftGraph_ResourceUrl;
                case AzureEnvironment.Worldwide:
                case AzureEnvironment.GCC:
                default:
                    return Global_MicrosoftGraph_ResourceUrl;

            }
        }

        private string GetAppToken()
        {
            var tokenApiClient = AosApiUtility.CloudSdkTokenClientFactory.CreateModernTokenApiClient(TenantLocalValue.LogonGroupId);
            var tokenType = PermissionType == TokenPermissionType.Application ? TokenType.ApplicationToken : TokenType.DelegatedToken;

            _tokenResult = tokenApiClient.ModernTokenService.GetTokenByAppProfileAsync(
                _providerType,
                TokenResourceType.Graph,
                _appInfo.UserAccountInfo.TenantId,
                _appInfo.UserAccountInfo.AppId,
                null,
                tokenType
            ).GetAwaiter().GetResult();

            return _tokenResult.AccessToken;
        }
    }

}
