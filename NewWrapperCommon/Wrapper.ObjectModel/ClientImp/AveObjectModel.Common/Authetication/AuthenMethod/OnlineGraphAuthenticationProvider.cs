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
using System;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using AvePoint.Office365.Api;
using System.Net;
using Microsoft.Identity.Client;

namespace AvePoint.ObjectModel.Common
{
    public class OnlineGraphAuthenticationProvider : IAuthenticationProvider
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(OnlineGraphAuthenticationProvider));
        public AuthenticationResult Login(string siteUrl, AveBPOSAccountInfo userAccountInfo)
        {
            try
            {
                var provider = new GraphTokenProvider(userAccountInfo);
                var token = provider.GetToken(null);
                log.Debug("login site {0} successfully using Graph authentication", siteUrl);
                return new AuthenticationResult(AutheStatus.Successful, AveAuthenticationMode.OnlineGraphToken, null, new System.Collections.Generic.List<ITokenProvider> { provider });
            }
            catch (Exception e)
            {
                var thumbprint = userAccountInfo.AppCert == null ? string.Empty : userAccountInfo.AppCert.Thumbprint;
                log.Warn("Login failed by Graph authentication. Site Url:{0}, TenantId: {1}, ClientId: {2}, Thumbprint: {3}, Error: {4}",
                    siteUrl, userAccountInfo.TenantId, userAccountInfo.ClientId, thumbprint, e);
                return new AuthenticationResult(AutheStatus.Failed, AveAuthenticationMode.OnlineGraphToken);
            }
        }
    }
    public class GraphTokenProvider : ITokenProvider
    {
        IConfidentialClientApplication certApplication;
        IConfidentialClientApplication secretApplication;
        object locker = new object();
        AzureCloudInstance azureRegion;
        string microsoftGraphEndpoint;

        AveBPOSAccountInfo userAccountInfo;
        public GraphTokenProvider(AveBPOSAccountInfo userAccountInfo)
        {
            this.userAccountInfo = userAccountInfo;
            this.userAccountInfo.TenantId.ArgumentNullValidation("tenantId");
            this.userAccountInfo.ClientId.ArgumentNullValidation("clientId");
            if (string.IsNullOrEmpty(this.userAccountInfo.APPSecret) && this.userAccountInfo.AppCert == null)
            {
                throw new ArgumentException("The appSecret and certificate are both null");
            }
            Identifier = userAccountInfo.TenantId;
            GetAzureEndpoint(userAccountInfo.AzureRegion, out azureRegion, out microsoftGraphEndpoint);
        }
        public string Identifier
        {
            get;
            private set;
        }

        public TokenType TokenType
        {
            get
            {
                return TokenType.Bearer;
            }
        }

        public NetworkCredential GetCredential(Uri uri, string authType)
        {
            return null;
        }
        /// <summary>
        /// 请注意此方法获取到的Token串前面没有"Bearer "，如有需要请自行添加。
        /// </summary>
        /// <param name="url"></param>
        /// <param name="refresh"></param>
        /// <returns></returns>
        public string GetToken(Uri url, bool refresh = false)
        {
            if (userAccountInfo.AppCert != null)
            {
                return RetieveAppTokenByCertificate();
            }
            else
            {
                return RetrieveAppTokenByPW();
            }
        }
        private string RetrieveAppTokenByPW()
        {
            if (secretApplication == null)
            {
                lock (locker)
                {
                    secretApplication = ConfidentialClientApplicationBuilder.Create(userAccountInfo.ClientId)
                        .WithClientSecret(userAccountInfo.APPSecret)
                        .WithAuthority(azureRegion, userAccountInfo.TenantId)
                        .Build();
                }
            }
            return secretApplication.AcquireTokenForClient(new string[] { microsoftGraphEndpoint }).ExecuteAsync().Result.AccessToken;
        }
        private string RetieveAppTokenByCertificate()
        {
            if (certApplication == null)
            {
                lock (locker)
                {
                    certApplication = ConfidentialClientApplicationBuilder.Create(userAccountInfo.ClientId)
                    .WithCertificate(userAccountInfo.AppCert)
                    .WithAuthority(azureRegion,userAccountInfo.TenantId)
                    .Build();
                }
            }
            return certApplication.AcquireTokenForClient(new string[] { microsoftGraphEndpoint }).ExecuteAsync().Result.AccessToken;
        }
        static void GetAzureEndpoint(AzureRegions azureRegion, out AzureCloudInstance azureInstance, out string microsoftGraphEndpoint)
        {
            switch (azureRegion)
            {
                case AzureRegions.Azure21V:
                    azureInstance = AzureCloudInstance.AzureChina;
                    microsoftGraphEndpoint = "https://microsoftgraph.chinacloudapi.cn/.default";
                    break;
                case AzureRegions.AzureGerman:
                    azureInstance = AzureCloudInstance.AzureGermany;
                    microsoftGraphEndpoint = "https://graph.cloudapi.de/.default";
                    break;
                case AzureRegions.AzureUSGov:
                    azureInstance = AzureCloudInstance.AzureUsGovernment;
                    microsoftGraphEndpoint = "https://graph.microsoft.us/.default";
                    break;
                case AzureRegions.AzureUSGovDoD:
                    azureInstance = AzureCloudInstance.AzureUsGovernment;
                    microsoftGraphEndpoint = "https://dod-graph.microsoft.us/.default";
                    break;
                case AzureRegions.Unknown:
                case AzureRegions.AzureGlobal:
                    azureInstance = AzureCloudInstance.AzurePublic;
                    microsoftGraphEndpoint = "https://graph.microsoft.com/.default";
                    break;
                default:
                    throw new ArgumentException("Not support region.", "azureRegion");
            }
        }
    }
}
