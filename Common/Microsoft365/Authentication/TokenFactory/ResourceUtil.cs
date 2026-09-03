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
    using Microsoft365.Authentication.ServiceEndPoint;
    using Microsoft365.Authentication.TokenProvider;
    using System;
    using System.Runtime.CompilerServices;

    public class ResourceUtil
    {
        internal const string AzureRMSProtectionTool = "4186465f-9980-40eb-98ca-35fea66b63e4";
        public const string MicrosoftApp = "1b730954-1685-4b74-9bfd-dac224a7b894";
        public const string MicrosoftExchangeApp = "d3590ed6-52b3-4102-aeff-aad2292ab01c";
        public const string MicrosoftOutlookApp = "fb78d390-0c51-40cd-8e17-fdbfab77341b";
        public const string MicrosoftTeamApp = "12128f48-ec9e-42f0-b203-ea49fb6af367";
        public const string CsOnlineSessionResource = "48ac35b8-9aa8-4d74-927d-1f4a14a0b239";
        public const string MicrosoftAADApp = "1950a258-227b-4e31-a9cf-717495945fc2";
        public const string AzureManagementResource = "74658136-14ec-4630-ad9b-26e160ff0fc6";
        public const string MicrosoftPowerApp = "a672d62c-fc7b-4e81-a576-e60dc46e951d";
        public const string TeamsSkypeResource = "https://api.spaces.skype.com";


        internal static string GenerateMsalScope(string resoure, AuthenticationResourceType resourceType, AveAzureEnvironment environment, [CallerMemberName] string caller = "")
        {
            var resource= GenerateResourceUrl(resoure, resourceType, environment, caller);
            return new Uri(resource).GetLeftPart(UriPartial.Authority).TrimEnd('/') + "/.default";
        }
        internal static string GenerateResourceUrl(string resource, AuthenticationResourceType resourceType, AveAzureEnvironment environment, [CallerMemberName] string caller="")
        {
            switch (resourceType)
            {

                case AuthenticationResourceType.Teams: return environment.GetMsoInstance().AdalMsGraphServiceResource;
                case AuthenticationResourceType.Graph: return environment.GetMsoInstance().AdalMsGraphServiceResource;
                case AuthenticationResourceType.ExchangeGraph: return environment.GetMsoInstance().AdalMsGraphServiceResource;
                case AuthenticationResourceType.ExchangeWebService: return environment.GetMsoInstance().EWSServiceUrl;
                case AuthenticationResourceType.Outlook: return environment.GetMsoInstance().ExchangeWebServiceEndpoint;
                case AuthenticationResourceType.TeamsSkype: return TeamsSkypeResource;
                case AuthenticationResourceType.SharePoint: return new Uri(resource).GetLeftPart(UriPartial.Authority);
                case AuthenticationResourceType.None:
                default:
                    throw new NotSupportedException($"{resourceType} not supported, called from {caller}");
            }
        }

        internal static string GetDelegateAppClientId(AuthenticationResourceType resourceType, [CallerMemberName] string caller = "")
        {

            switch (resourceType)
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
                default: throw new NotSupportedException($"Not support resource type {resourceType},caller:{caller}");
            }
        }
    }
}