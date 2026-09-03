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
    using AvePoint.GCommon;
    using AvePoint.GCommon.Utility.Cloud;
    using AvePoint.GCommon.Utility.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using AGCCO = AvePoint.GCommon.Contract.CentralAdmin.Object;
    using Microsoft365.Authentication;

    public static class BposInfoExtension
    {
        static AveLogger logger = AveLogger.GetInstance(typeof(BposInfoExtension));

        public static AveBPOSAccountInfo ConvertToAveBPOSAccountInfo(this AGCCO.BposInfo info)
        {
            if (info == null) throw new ArgumentNullException("info");

            AveBPOSAccountInfo accountInfo = null;
            if (info.ConnectionType == AGCCO.BposConnectionType.ServiceAccount)
            {
                accountInfo = Convert2ServiceAccountInfo(info.UserAccountInfo, info.TenantGroupId);
            }
            else
            {
                accountInfo = Convert2AppTokenInfo(info.UserAccountInfo, info.AppType, info.TenantGroupId);
            }

            logger.Info($"SiteUrl:{info.SiteUrl}, Info:{accountInfo.ConnectionType} {accountInfo.AppType}");
            return accountInfo;
        }

        static AveBPOSAccountInfo Convert2ServiceAccountInfo(AGCCO.BposUserAccountInfo info, string tenantGroupId)
        {
            if (info == null) throw new ArgumentNullException("info");

            string domain = ".".Equals(info.Domain) ? string.Empty : info.Domain;
            string username = info.Username;
            return new AveBPOSAccountInfo()
            {
                Domain = domain,
                UserName = username,
                Password = info.Password.ToSecureStringWithEmptyCheck(),
                AdminUrl = info.AdminUrl,
                ConnectionType = BposConnectionType.ServiceAccount,
                AADEnvironment = (AveAzureEnvironment)(int)info.AADEnvironment,
                SecurityGroup = info.SecurityGroup,
                TenantGroupId = tenantGroupId
            };
        }


        static AveBPOSAccountInfo Convert2AppTokenInfo(AGCCO.BposUserAccountInfo info, AGCCO.AppType appType, string tenantGroupId)
        {
            if (info == null) throw new ArgumentNullException("info");

            var clientId = string.Empty;
            X509Certificate2 cert = null;
            if (!string.IsNullOrEmpty(info.AppClientId))
            {
                clientId = info.AppClientId;
                //cert = CreateCert(info.AppCertSecret, info.AppCertContent, info.AppCertSecretContent);
            }
            else
            {
                logger.Warn("Use default app profile");
                clientId = GCommonRoleConfiguration.GetClientId(appType);
                //cert = AppTokenHelper.AppOnlyCertificate;
            }

            return new AveBPOSAccountInfo()
            {
                TenantId = info.TenantId,
                AdminUrl = info.AdminUrl,
                ClientId = clientId,
                //AppCert = cert,
                ConnectionType = BposConnectionType.AppToken,
                AADEnvironment = (AveAzureEnvironment)(int)info.AADEnvironment,
                SecurityGroup = info.SecurityGroup,
                TenantGroupId = tenantGroupId,
                AuthenticationProfileId = info.AppId,
                AppType = appType
            };
        }



        /*private static X509Certificate2 CreateCert(string secret, string certContent, string newCertContent)
        {
            X509Certificate2 cert;
            if (!string.IsNullOrEmpty(newCertContent))
            {
                cert = CreateCert(newCertContent);
            }
            else
            {
                if (string.IsNullOrEmpty(secret)) throw new ArgumentNullException("secret");
                if (string.IsNullOrEmpty(certContent)) throw new ArgumentNullException("certContent");

                secret = CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(secret));
                var contentBytes = Convert.FromBase64String(certContent);

                cert = new X509Certificate2(contentBytes, secret,
                    X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);
            }
            return cert;
        }*/
    }
}
