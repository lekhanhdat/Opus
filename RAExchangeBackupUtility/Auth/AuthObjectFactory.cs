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
    using System;
    using System.Security.Cryptography.X509Certificates;
    using AvePoint.GCommon.Contract.CentralAdmin.Object;
    using AvePoint.GCommon.Utility.Cloud;
    using AvePoint.GCommon.Utility.Cryptography;
    using AvePoint.GCommon;
    using AvePoint.RA.CommonUtil;
    using AvePoint.RA.Common.Aos;
    using AvePoint.Common;
    using AvePoint.RA.Common.Global.Utils;

    public static class AuthObjectFactory
    {
        static RALogger logger = RALogger.GetInstance(typeof(AuthObjectFactory));

        public static AuthObject CreateAuthObject(BposInfo info, AuthResourceType resourceType)
        {
            var ewsServiceUrl = info.SiteUrl;
            if (info.ConnectionType == BposConnectionType.ServiceAccount)
            {
                return GenerateServiceAccountAuthObject(info, resourceType);
            }
            else
            {
                AppTokenAuthObject.AppInfo appInfo;
                GetCertificate(info, out appInfo);
                var azureEnv = GetAzureEnv(info);
                return new AppTokenAuthObject(
                    new AppTokenAuthObject.AuthenticationInfo { Authority = azureEnv.ResourceUrls.Authority, Resource = ConvertToResourceUrl(azureEnv, resourceType), TenantId = info.UserAccountInfo.TenantId, SiteUrl = info.SiteUrl, TenantGroupId = info.TenantGroupId },
                    appInfo,
                    info.UserAccountInfo.Username,
                    ewsServiceUrl);
            }
        }

        private static bool GetCertificate(BposInfo info, out AppTokenAuthObject.AppInfo appInfo)
        {
            var id = info?.UserAccountInfo?.AppClientId;
            if (string.IsNullOrEmpty(id))
            {
                ArgumentCheck.NotNull(info, nameof(info));
                logger.Warn("info.UserAccountInfo.AppClientId is null, use default app profile, AppType: {0}", info?.AppType);
                appInfo = new AppTokenAuthObject.AppInfo
                {
                   // Certificate = AppTokenHelper.AppOnlyCertificate,
                    ClientId = GCommonRoleConfiguration.GetClientId(info.AppType),
                    AppType = info.AppType,
                    AppId = info.UserAccountInfo?.AppId
                };
                return false;
            }
            else
            {
                var secret = info?.UserAccountInfo?.AppCertSecret;
                //var certContent = info?.UserAccountInfo?.AppCertContent;
                var appCertSecretContent = info?.UserAccountInfo?.AppCertSecretContent;
                //X509Certificate2 appOnlyCert = RMAosApiClient.GetAppCertificate(secret, certContent, appCertSecretContent);
                appInfo = new AppTokenAuthObject.AppInfo
                {
                    //Certificate = appOnlyCert,
                    ClientId = id,
                    AppType = info.AppType,
                    AppId = info.UserAccountInfo?.AppId
                };

                logger.Info($"info.UserAccountInfo. AppClientId is not null");
                return true;
            }
        }

        private static string ConvertToResourceUrl(AzureEnvironmentInstance azureEnv, AuthResourceType resourceType)
        {
            switch (resourceType)
            {
                case AuthResourceType.EWS:
                    return azureEnv.ResourceUrls.EWS;
                //case AuthResourceType.Graph:
                //    return CreateAuthObject(info, ExchangeConstants.ServiceUrls.Global_GraphResourceUrl);
                case AuthResourceType.MicrosoftGraph:
                    return azureEnv.ResourceUrls.MSGraph;
                case AuthResourceType.None:
                default:
                    throw new System.ArgumentNullException("resourceType");
            }
        }

        private static AzureEnvironmentInstance GetAzureEnv(BposInfo info)
        {
            //service account通过user name自动检测region
            if (info.ConnectionType == BposConnectionType.ServiceAccount)
            {
                var env = AzureEnvironment.FromDomainOrPrincipalName(info.UserAccountInfo.Username);
                return env ?? AzureEnvironment.DefaultCloud;
            }
            //app profile使用AOS传过来的枚举
            logger.Info($"AADEnvironment:{info.UserAccountInfo.AADEnvironment}");
            switch (info.UserAccountInfo.AADEnvironment)
            {
                case AADEnvironment.AzureCloud:
                    return AzureEnvironment.GlobalCloud;
                case AADEnvironment.USGovernment:
                    return AzureEnvironment.GovCloud;
                case AADEnvironment.AzureChinaCloud:
                    return AzureEnvironment.ChinaCloud;
                case AADEnvironment.AzureGermanyCloud:
                    return AzureEnvironment.GermanCloud;
                case AADEnvironment.USGovernment_DoD:
                    return AzureEnvironment.GovDoDCloud;
                case AADEnvironment.AzurePPE:
                default:
                    return AzureEnvironment.DefaultCloud;
            }
        }

        private static AuthObject GenerateServiceAccountAuthObject(BposInfo info, AuthResourceType resourceType)
        {
            var azureEnv = GetAzureEnv(info);
            if (resourceType == AuthResourceType.MicrosoftGraph)
            {
                var clientId = azureEnv.CloudType.Equals(AzureCloudType.China) ? "1b730954-1685-4b74-9bfd-dac224a7b894" : "12128f48-ec9e-42f0-b203-ea49fb6af367";
                return new ServiceAccout2AppTokenAuthObject(
                                           new AppTokenAuthObject.AuthenticationInfo { Authority = azureEnv.ResourceUrls.Authority, Resource = ConvertToResourceUrl(azureEnv, resourceType), TenantId = info.UserAccountInfo.TenantId, SiteUrl = info.SiteUrl, TenantGroupId = info.TenantGroupId },
                                           clientId,
                                           info.UserAccountInfo.Username,
                                           info.UserAccountInfo.Password,
                                           info.SiteUrl,
                                           azureEnv.CloudType
                                           );
            }
            else
            {
                var clientId = "d3590ed6-52b3-4102-aeff-aad2292ab01c";
                return new ServiceAccout2AppTokenAuthObject(
                                           new AppTokenAuthObject.AuthenticationInfo { Authority = azureEnv.ResourceUrls.Authority, Resource = ConvertToResourceUrl(azureEnv, resourceType), TenantId = info.UserAccountInfo.TenantId, SiteUrl = info.SiteUrl, TenantGroupId = info.TenantGroupId },
                                           clientId,
                                           info.UserAccountInfo.Username,
                                           info.UserAccountInfo.Password,
                                           info.SiteUrl,
                                           azureEnv.CloudType
                                           );
            }
        }

        //private static AppTokenAuthObject FakeAppTokenAuthObj(AuthResourceType resourceType)
        //{
        //    var cache = GetCacheTokenFile(resourceType);
        //    if (!System.IO.File.Exists(cache)) return null;
        //    var lines = System.IO.File.ReadAllLines(cache);
        //    if (lines.Length != 5) throw new System.InvalidOperationException();

        //    var userName = lines[0];
        //    var authority = lines[1];
        //    var clientId = lines[2];
        //    var resource = lines[3];
        //    var refreshToken = lines[4];
        //    return new AppTokenAuthObject(userName, authority, clientId, resource, refreshToken);
        //}

        //private static string GetCacheTokenFile(AuthResourceType resourceType)
        //{
        //    switch (resourceType)
        //    {
        //        case AuthResourceType.EWS:
        //            return @"c:\data\ewstoken.txt";
        //        case AuthResourceType.Graph:
        //            return @"c:\data\graphtoken.txt";
        //        default:
        //            throw new System.InvalidOperationException("Unreachable code.");
        //    }
        //}
    }

    public enum AuthResourceType
    {
        None = 0,
        EWS = 1,
        Graph = 2,
        MicrosoftGraph = 3,//Rest Api
    }
}