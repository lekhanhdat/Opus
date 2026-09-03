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
using Cloud.Sdk.Data.Aos;
using Microsoft365.Authentication;
using CloudAos = Cloud.Sdk.Data.AosModern;

namespace AvePoint.RA.Common.Graph
{
    internal class EndpointUtil
    {
        public static string GetGraphEndpoint(CloudAos.AADEnvironment env)
        {
            switch (env)
            {
                case CloudAos.AADEnvironment.AzureCloud:
                    return "https://graph.microsoft.com";
                case CloudAos.AADEnvironment.AzureChinaCloud:
                    return "https://microsoftgraph.chinacloudapi.cn";
                case CloudAos.AADEnvironment.AzureGermanyCloud:
                    return "https://graph.microsoft.de";
                case CloudAos.AADEnvironment.AzurePPE:
                    return "https://graph.microsoft-ppe.com";
                case CloudAos.AADEnvironment.USGovernment:
                    return "https://graph.microsoft.us";
                case CloudAos.AADEnvironment.USGovernment_DoD:
                    return "https://dod-graph.microsoft.us";
                default:
                    return "https://graph.microsoft.com";
            }
        }
        public static string GetServiceAccountGraphEndpoint(AveAzureEnvironment env)
        {
            switch (env)
            {
                case AveAzureEnvironment.AzureCloud:
                    return "https://graph.microsoft.com";
                case AveAzureEnvironment.AzureChinaCloud:
                    return "https://microsoftgraph.chinacloudapi.cn";
                case AveAzureEnvironment.AzureGermanyCloud:
                    return "https://graph.microsoft.de";
                case AveAzureEnvironment.AzurePPE:
                    return "https://graph.microsoft-ppe.com";
                case AveAzureEnvironment.USGovernment:
                    return "https://graph.microsoft.us";
                case AveAzureEnvironment.USGovernmentDOD:
                    return "https://dod-graph.microsoft.us";
                default:
                    return "https://graph.microsoft.com";
            }
        }
        public static string GetAuthority(CloudAos.AADEnvironment env)
        {
            switch (env)
            {              
                case CloudAos.AADEnvironment.AzureCloud:
                    return "https://login.microsoftonline.com/{0}/oauth2/token";
                case CloudAos.AADEnvironment.AzureChinaCloud:
                    return "https://login.chinacloudapi.cn/{0}/oauth2/token";
                case CloudAos.AADEnvironment.AzureGermanyCloud:
                    return "https://login.microsoftonline.de/{0}/oauth2/token";
                case CloudAos.AADEnvironment.AzurePPE:
                    return "https://login.microsoftonline.us/{0}/oauth2/token";
                case CloudAos.AADEnvironment.USGovernment:
                case CloudAos.AADEnvironment.USGovernment_DoD:
                    return "https://login.microsoftonline.us/{0}/oauth2/token";
                default:
                    return "https://login.microsoftonline.com/{0}/oauth2/token";
            }
        }
    }
}
