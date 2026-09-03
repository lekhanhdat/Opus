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

namespace Microsoft365.Authentication.TokenProvider
{
    public static class DelegateAuthenticationHelper
    {
        private const string MicrosoftApp = "1b730954-1685-4b74-9bfd-dac224a7b894";
        private const string MicrosoftExchangeApp = "d3590ed6-52b3-4102-aeff-aad2292ab01c";
        private const string MicrosoftOutlookApp = "fb78d390-0c51-40cd-8e17-fdbfab77341b";
        private const string MicrosoftTeamApp = "12128f48-ec9e-42f0-b203-ea49fb6af367";

        public static string GetClientId(AuthenticationResourceType type)
        {
            switch (type)
            {
                case AuthenticationResourceType.Graph:
                case AuthenticationResourceType.SharePoint:
                    return MicrosoftApp;
                case AuthenticationResourceType.ExchangeWebService:
                case AuthenticationResourceType.ExchangeGraph:
                    return MicrosoftExchangeApp;
                case AuthenticationResourceType.Outlook:
                    return MicrosoftOutlookApp;
                case AuthenticationResourceType.Teams:
                case AuthenticationResourceType.TeamsSkype:
                    return MicrosoftTeamApp;
                default: throw new NotSupportedException($"Not support resource type {type}");
            }
        }
    }
}