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
using AvePoint.Records.Core.Utilities.Extensions;
using Cloud.Sdk.Data.Aos;
using Microsoft.Identity.Client;
//using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Security.Cryptography.X509Certificates;

namespace AvePoint.RA.Common.Graph
{
    internal class TokenHelper
    {
        private string _resourceGraph;
        private string _authority;

        public TokenHelper(string resourceGraph, string authority)
        {
            _resourceGraph = resourceGraph;
            _authority = authority;
        }

        private AuthenticationResult AcquireToken(string authority, string resource, string clientId, byte[] certBytes, Boolean removeCache = false)
        {
            var cert = new X509Certificate2(certBytes);

            return AcquireToken(authority, resource, clientId, cert, removeCache);
        }

        private AuthenticationResult AcquireToken(string authority, string resource, string clientId, X509Certificate2 cert, Boolean removeCache = false)
        {
            //var authenticationContext = new AuthenticationContext(authority, false);
            //if (removeCache && authenticationContext.TokenCache != null)
            //{
            //    authenticationContext.TokenCache.Clear();
            //}

            //var cac = new ClientAssertionCertificate(clientId, cert);
            //var authenticationResult = authenticationContext.AcquireTokenAsync(resource, cac).Result;
            var app = ConfidentialClientApplicationBuilder.Create(clientId)
                      .WithCertificate(cert)
                      .WithAuthority(authority)
                      .Build();
            var authenticationResult = app.AcquireTokenForClient(
                        new[] { new Uri(resource).GetLeftPart(UriPartial.Authority).TrimEnd('/') + "/.default" })
                        // .WithTenantId(specificTenant)
                        // See https://aka.ms/msal.net/withTenantId
                        .ExecuteAsync()
                        .Result;

            return authenticationResult;
        }

        public AuthenticationResult GetAccessToken(string tenantId, string clientId, byte[] certBytes)
        {
            return AcquireToken(string.Format(this._authority, tenantId), this._resourceGraph, clientId, certBytes, true);
        }

        public AuthenticationResult GetAccessToken(string tenantId, string clientId, X509Certificate2 cert)
        {
            return AcquireToken(string.Format(this._authority, tenantId), this._resourceGraph, clientId, cert, true);
        }
    }
}
