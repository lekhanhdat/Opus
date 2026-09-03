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
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Web;

namespace AvePoint.RA.Web.Common.WIF
{
    public class AuthModule : WSFederationAuthenticationModule
    {
        public AuthModule()
        {

        }

        protected override void InitializeModule(HttpApplication context)
        {
            base.InitializeModule(context);

            FederatedAuthentication.WSFederationAuthenticationModule.PassiveRedirectEnabled = true;
            FederatedAuthentication.FederationConfiguration.IdentityConfiguration.IssuerNameRegistry = new MyName();
            FederatedAuthentication.FederationConfiguration.IdentityConfiguration.CertificateValidator = new MyCertificateValidator();
            FederatedAuthentication.FederationConfiguration.IdentityConfiguration.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.Custom;

        }
    }

    public class MyCertificateValidator : X509CertificateValidator
    {
        public override void Validate(System.Security.Cryptography.X509Certificates.X509Certificate2 certificate)
        {

        }
    }

    public class MyName : IssuerNameRegistry
    {
        public override string GetIssuerName(SecurityToken securityToken)
        {
            return "RecordManager";
        }
    }
}