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
using AvePoint.GCommon.Utility.Cryptography;
using Microsoft.Identity.Client;
//using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Utility.Cloud
{
    public class AppTokenHelper
    {
        private static X509Certificate2 apponlyCertificate;

        public static string GetAccessTokenFromCert(string tenantId, string clientId, string resourceUrl)
        {
            return GetTokenFromCert(tenantId, clientId, resourceUrl).AccessToken;
        }

        /// <summary>
        /// The App-only certificate
        /// </summary>
        public static X509Certificate2 AppOnlyCertificate
        {
            get
            {
                if (apponlyCertificate == null)
                {
                    var certPath = SecurityUtils.SafeCombinePath(System.AppDomain.CurrentDomain.BaseDirectory, GCommonRoleConfiguration.AppCertFile);
                    if (!System.IO.File.Exists(certPath))
                    {
                        certPath = SecurityUtils.SafeCombinePath(System.AppDomain.CurrentDomain.BaseDirectory, "bin", Path.DirectorySeparatorChar.ToString(),  GCommonRoleConfiguration.AppCertFile);
                    }
                    using (var certfile = System.IO.File.OpenRead(certPath))
                    {
                        var certificateBytes = new byte[certfile.Length];
                        var readLen = certfile.Read(certificateBytes, 0, (int)certfile.Length);
                        if (readLen < 0)
                        {
                            throw new Exception("Read certificate error");
                        }

                        string secret = CryptoUtil.ConvertBytesToString(CspCrossPlatformExchangeWrapper.UnWrapKey(GCommonRoleConfiguration.AppCertSecret));
                        apponlyCertificate = new X509Certificate2(
                            certificateBytes,
                            secret,
                            X509KeyStorageFlags.Exportable |
                            X509KeyStorageFlags.MachineKeySet);
                    }
                }

                return apponlyCertificate;
            }
        }

        public static AuthenticationResult GetTokenFromCert(string tenantId, string clientId, string resourceUrl)
        {
            return GetTokenFromCert(tenantId, clientId, resourceUrl, AppOnlyCertificate);
        }

        public static AuthenticationResult GetTokenFromCert(string tenantId, string clientId, string resourceUrl, X509Certificate2 cert)
        {
            //Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext authenticationContext = new Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext(string.Format("https://login.windows.net/{0}", tenantId), false);
            //ClientAssertionCertificate cac = new ClientAssertionCertificate(clientId, cert);
            //return authenticationContext.AcquireTokenAsync(resourceUrl, cac).Result;



            var app = CreateApplication(tenantId, clientId, cert);
            var authResult = app.AcquireTokenForClient(
            new[] { new Uri(resourceUrl).GetLeftPart(UriPartial.Authority).TrimEnd('/') + "/.default" })
            .ExecuteAsync()
            .GetAwaiter().GetResult();
            return authResult;
        }

        public static IConfidentialClientApplication CreateApplication(string tenantId, string clientId, X509Certificate2 cert)
        {
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(clientId)
            .WithCertificate(cert)
            .WithAuthority(string.Format("https://login.windows.net/{0}", tenantId))
            .Build();
            return app;
        }
    }
}
