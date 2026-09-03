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
namespace Microsoft365.Authentication.ServiceEndPoint
{
    using Microsoft365.Authentication;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using static MicrosoftOnlineInstanceConstString;
    public static class MicrosoftOnlineInstance
    {
        internal class TypeDependencies
        {
            internal virtual IList<string> DnsResolverResolveCNameRecord(string cnameAlias)
            {
                return DnsResolver.ResolveCNameRecord(cnameAlias);
            }
        }

        public static readonly MicrosoftOnlineInstanceDetail AzureCloud = new MicrosoftOnlineInstanceDetail(
            "https://login.microsoftonline.com/",
            "https://graph.windows.net",
            "https://graph.microsoft.com/",
            "microsoftonline.com",
            "onmicrosoft.com",
            "microsoftonline-p.net",
            "nexus.microsoftonline-p.com",
            "microsoftonline.com",
            "https://enterpriseregistration.windows.net",
            EWS_Global_ResourceUrl,
            AveAzureEnvironment.AzureCloud);

        public static readonly MicrosoftOnlineInstanceDetail AzureChinaCloud = new MicrosoftOnlineInstanceDetail(
            "https://login.chinacloudapi.cn/",
            "https://graph.chinacloudapi.cn",
            "https://microsoftgraph.chinacloudapi.cn/",
            "partner.microsoftonline.cn",
            "partner.onmschina.cn",
            "partner.microsoftonline-p.net.cn",
            "nexus.partner.microsoftonline-p.cn",
            "partner.microsoftonline.cn",
            string.Empty,
            EWS_China_ResourceUrl,
            AveAzureEnvironment.AzureChinaCloud);

        //public static readonly MicrosoftOnlineInstanceDetail USGovernment = new MicrosoftOnlineInstanceDetail("https://login.microsoftonline.us/", "https://graph.windows.net", "https://graph.microsoft.com/", "gov.us.microsoftonline.com", "onmicrosoft.com", "microsoftonline-p.net", "nexus.microsoftonline-p.com", "microsoftonline.com", "https://enterpriseregistration.windows.net");

        public static readonly MicrosoftOnlineInstanceDetail AzureUSGovernmentCloud = new MicrosoftOnlineInstanceDetail(
            "https://login.microsoftonline.us/",
            "https://graph.windows.net",
            "https://graph.microsoft.us/",
            "microsoftonline.us",
            "onmicrosoft.us",
            "microsoftonline.us",
            "login.microsoftonline.us",
            "microsoftonline.us",
            "https://enterpriseregistration.windows.us",
            EWS_Gov_ResourceUrl,
            AveAzureEnvironment.USGovernment);

        public static readonly MicrosoftOnlineInstanceDetail AzureUSGovernmentDODCloud = new MicrosoftOnlineInstanceDetail(
           "https://login.microsoftonline.us/",
           "https://graph.windows.net",
           "https://dod-graph.microsoft.us/",
           "microsoftonline.us",
           "onmicrosoft.us",
           "microsoftonline.us",
           "login.microsoftonline.us",
           "microsoftonline.us",
           "https://enterpriseregistration.windows.us",
           EWS_DOD_ResourceUrl,
           AveAzureEnvironment.USGovernmentDOD);

        public static readonly MicrosoftOnlineInstanceDetail AzureGermanyCloud = new MicrosoftOnlineInstanceDetail(
            "https://login.microsoftonline.de/",
            "https://graph.cloudapi.de",
            "https://graph.microsoft.de/",
            "microsoftonline.de",
            "onmicrosoft.de",
            "microsoftonline-p.net",
            "login.microsoftonline.de",
            "microsoftonline.com",
            "https://enterpriseregistration.microsoftonline.de",
            EWS_German_ResourceUrl,
            AveAzureEnvironment.AzureGermanyCloud);

        public static readonly MicrosoftOnlineInstanceDetail AzurePPE = new MicrosoftOnlineInstanceDetail(
            "https://login.windows-ppe.net/",
            "https://graph.ppe.windows.net",
            "https://graph.microsoft-ppe.com/",
            "ccsctp.com",
            "ccsctp.net",
            "microsoftonline-p.net",
            "nexus.microsoftonline-p.com",
            "microsoftonline.com",
            "https://enterpriseregistration-ppe.windows.net",
            EWS_Global_ResourceUrl,
            AveAzureEnvironment.AzurePPE);

        public static readonly MicrosoftOnlineInstanceDetail AzureOneBox = new MicrosoftOnlineInstanceDetail(
            "https://login.microsoftonline.com/",
            "https://graph.windows.net",
            "https://graph.microsoft.com/",
            "msol-test.com",
            "msol-test.com",
            "microsoftonline-p.net",
            "nexus.microsoftonline-p.com",
            "microsoftonline.com",
            string.Empty,
            EWS_Global_ResourceUrl,
            AveAzureEnvironment.None);

        private static TypeDependencies dependencies = new TypeDependencies();

        internal static TypeDependencies Dependencies
        {
            get
            {
                return dependencies;
            }
            set
            {
                dependencies = value;
            }
        }

        public static MicrosoftOnlineInstanceDetail FromEnvironment(AveAzureEnvironment environment)
        {
            switch (environment)
            {
                case AveAzureEnvironment.AzureChinaCloud:
                    return AzureChinaCloud;

                case AveAzureEnvironment.AzureGermanyCloud:
                    return AzureCloud;
                case AveAzureEnvironment.AzurePPE:
                    return AzureCloud;
                case AveAzureEnvironment.USGovernment:
                    return AzureUSGovernmentCloud;
                case AveAzureEnvironment.USGovernmentDOD:
                    return AzureUSGovernmentDODCloud;
                case AveAzureEnvironment.None:
                case AveAzureEnvironment.AzureCloud:
                default:
                    return AzureCloud;

            }
        }

        public static MicrosoftOnlineInstanceDetail FromDomainOrPrincipalName(string domainOrPrincipalName)
        {
            ArgumentValidator.ThrowIfNullOrEmpty(domainOrPrincipalName, "domainOrPrincipalName");
            MicrosoftOnlineInstanceDetail msoInstance = null;
            msoInstance = Office365Discover.GetEnvironment(domainOrPrincipalName).GetMsoInstance();
            if (msoInstance == null)
                msoInstance = AzureCloud; //set a default value
            return msoInstance;
        }

        /*private static MicrosoftOnlineInstanceDetail FromOrgIdClientConfigDomainNameSuffix(string orgIdClientConfigUrl)
        {
            MicrosoftOnlineInstanceDetail msoInstance = null;
            if (orgIdClientConfigUrl.Equals(AzureCloud.OrgIdClientConfigUrl, StringComparison.InvariantCultureIgnoreCase))
            {
                msoInstance = AzureCloud;
            }
            else if (orgIdClientConfigUrl.Equals(AzureChinaCloud.OrgIdClientConfigUrl, StringComparison.InvariantCultureIgnoreCase))
            {
                msoInstance = AzureChinaCloud;
            }
            else if (orgIdClientConfigUrl.Equals(AzureGermanyCloud.OrgIdClientConfigUrl, StringComparison.InvariantCultureIgnoreCase))
            {
                msoInstance = AzureGermanyCloud;
            }
            else if (orgIdClientConfigUrl.Equals(AzurePPE.OrgIdClientConfigUrl, StringComparison.InvariantCultureIgnoreCase))
            {
                msoInstance = AzurePPE;
            }
            return msoInstance;
        }*/
    }
}