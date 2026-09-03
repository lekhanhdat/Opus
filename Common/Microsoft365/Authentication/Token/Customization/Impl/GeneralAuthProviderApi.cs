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
namespace Microsoft365.Authentication
{
    using System.Security;
    using Microsoft365.Authentication.Configuration;
    using Microsoft365.Authentication.Token.AzureToken;
    using Microsoft365.Authentication.Token.Idclr.Impl;
    using Microsoft365.Common.Extension;
    using Microsoft365.Common.Logger;

    class GeneralAuthProviderApi : IAuthProviderApi
    {


        public GeneralAuthProviderApi()
        {
        }

        public GeneralAuthProviderApi(AuthenticationParameter parameter)
        {
        }

        public IFederationAuthApi CreateFederationAuthApi()
        {
            System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls13 | System.Net.SecurityProtocolType.Tls12;
            return new GeneralAdfsAuthApi();
        }

        public ITokenProvider CreateTokenProvider(string username, SecureString password, AveAzureEnvironment environment)
        {
            System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls13 | System.Net.SecurityProtocolType.Tls12;
            return new GeneralSPOTokenProvider(username, password, environment);
        }
    }
}