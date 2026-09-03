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
using AvePoint.RA.CommonUtil;
using Microsoft365.Authentication.ServiceEndPoint;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ExchangeUtility
{
    //https://msdn.microsoft.com/en-us/office/office365/api/o365-china-endpoints
    class AzureEnvironmentInstance
    {
        public string EWSServiceUrl { get { return $"{ResourceUrls.EWS}/EWS/Exchange.asmx"; } }
        public string PSConnectionUrl { get { return $"{ResourceUrls.EWS}/Powershell/"; } }
        public Resource ResourceUrls { get; private set; }

        public AzureCloudType CloudType { get; private set; }

        public AzureEnvironmentInstance(AzureCloudType act, Resource resourceUrls)
        {
            this.CloudType = act;
            this.ResourceUrls = resourceUrls;
        }
    }
    class AzureEnvironment
    {
        private static RALogger logger = RALogger.GetInstance(typeof(AzureEnvironment));
        public static readonly AzureEnvironmentInstance ChinaCloud;
        public static readonly AzureEnvironmentInstance GlobalCloud;
        public static readonly AzureEnvironmentInstance GermanCloud;
        public static readonly AzureEnvironmentInstance GovCloud;
        public static readonly AzureEnvironmentInstance GovDoDCloud;
        public static readonly AzureEnvironmentInstance DefaultCloud;
        static readonly AzureEnvironmentCache cache = new AzureEnvironmentCache();
        static readonly Dictionary<string, AzureEnvironmentInstance> fpNameMapping;

        static AzureEnvironment()
        {
            ChinaCloud = FromCloudType(AzureCloudType.China);
            GlobalCloud = FromCloudType(AzureCloudType.Global);
            GermanCloud = FromCloudType(AzureCloudType.German);
            GovCloud = FromCloudType(AzureCloudType.USGovernment);
            GovDoDCloud = FromCloudType(AzureCloudType.USGovernment_DoD);
            DefaultCloud = GlobalCloud;
            fpNameMapping = new Dictionary<string, AzureEnvironmentInstance>()
            {
                { "partner.microsoftonline.cn", ChinaCloud },
                { "microsoftonline.de", GermanCloud },
                { "microsoftonline.us", GovCloud}
                //do not add global to this mapping. mailto:qlluo@avepoint.com for detail
            };
        }

        private static AzureEnvironmentInstance FromDomainOrPrincipalNameInternal(string domainOrPrincipalName)
        {
            try
            {
                AzureEnvironmentInstance azureEnv;

                //1. DomainNameSuffix, DnsCName
                var userDetail = MicrosoftOnlineInstance.FromDomainOrPrincipalName(domainOrPrincipalName);
                azureEnv = FromMicrosoftOnlineInstanceDetail(userDetail);
                if (azureEnv != null) return azureEnv;

                //2. ODC getfederationprovider
                var fpName = OfficeDataConnectionHelper.GetFederationProvider(domainOrPrincipalName);
                if (!string.IsNullOrEmpty(fpName) && fpNameMapping.TryGetValue(fpName, out azureEnv))
                {
                    return azureEnv;
                }

                //3. log openid configuration, will log internal
                OfficeDataConnectionHelper.GetOpenIdConfiguration(domainOrPrincipalName);
                return null;
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to get azure env info from upn, {domainOrPrincipalName}, error: {ex}");
                return null;
            }

        }

        public static AzureEnvironmentInstance FromDomainOrPrincipalName(string domainOrPrincipalName)
        {
            if (string.IsNullOrEmpty(domainOrPrincipalName)) return null; //throw new ArgumentNullException(nameof(domainOrPrincipalName));
            //0. From cache
            AzureEnvironmentInstance azureEnv;
            var domain = domainOrPrincipalName.GetDomain();

            if (cache.TryGetValue(domain, out azureEnv))
            {
                return azureEnv;
            }
            var env = FromDomainOrPrincipalNameInternal(domainOrPrincipalName);
            cache[domain] = env;
            return env;
        }

        private static AzureEnvironmentInstance FromMicrosoftOnlineInstanceDetail(MicrosoftOnlineInstanceDetail detail)
        {
            var domainSuffix = detail?.InitialDomainNameSuffix?.ToLower();
            if (string.IsNullOrEmpty(domainSuffix)) return null;
            switch (domainSuffix)
            {
                case ServiceUrls.China_InitialDomainNameSuffix:
                    return ChinaCloud;
                case ServiceUrls.German_InitialDomainNameSuffix:
                    return GermanCloud;
                case ServiceUrls.Global_InitialDomainNameSuffix:
                    return GlobalCloud;
            }
            return null;
        }

        private static AzureEnvironmentInstance FromCloudType(AzureCloudType act)
        {
            switch (act)
            {
                case AzureCloudType.China:
                    return new AzureEnvironmentInstance(act, new Resource(
                        ServiceUrls.China_Authority,
                        ServiceUrls.China_EWS_ResourceUrl,
                        ServiceUrls.China_MicrosoftGraph_ResourceUrl));
                case AzureCloudType.German:
                    return new AzureEnvironmentInstance(act, new Resource(
                        ServiceUrls.German_Authority,
                        ServiceUrls.German_EWS_ResourceUrl,
                        ServiceUrls.German_MicrosoftGraph_ResourceUrl));
                case AzureCloudType.Global:
                    return new AzureEnvironmentInstance(act, new Resource(
                        ServiceUrls.Global_Authority,
                        ServiceUrls.Global_EWS_ResourceUrl,
                        ServiceUrls.Global_MicrosoftGraph_ResourceUrl));
                case AzureCloudType.USGovernment:
                    return new AzureEnvironmentInstance(act, new Resource(
                       ServiceUrls.Gov_Authority,
                       ServiceUrls.Gov_EWS_ResourceUrl,
                       ServiceUrls.Gov_MicrosoftGraph_ResourceUrl));
                case AzureCloudType.USGovernment_DoD:
                    return new AzureEnvironmentInstance(act, new Resource(
                       ServiceUrls.Gov_Authority,
                       ServiceUrls.DoD_EWS_ResourceUrl,
                       ServiceUrls.Dod_MicrosoftGraph_ResourceUrl));
                case AzureCloudType.PPE:
                default:
                    return null;

            }
        }

        class ServiceUrls
        {
            //public const string Global_EWS_ServiceUrl = "https://outlook.office365.com/EWS/Exchange.asmx";
            //public const string German_EWS_ServiceUrl = "https://outlook.office.de/EWS/Exchange.asmx";
            //public const string China_EWS_ServiceUrl = "https://partner.outlook.cn/EWS/Exchange.asmx";

            //public const string Global_PS_ConnectionUrl = "https://outlook.office365.com/Powershell/";
            //public const string German_PS_ConnectionUrl = "https://outlook.office.de/Powershell/";
            //public const string China_PS_ConnectionUrl = "https://partner.outlook.cn/PowerShell/";

            public const string Global_EWS_ResourceUrl = "https://outlook.office365.com";
            public const string German_EWS_ResourceUrl = "https://outlook.office.de";
            public const string China_EWS_ResourceUrl = "https://partner.outlook.cn";
            public const string Gov_EWS_ResourceUrl = "https://outlook.office365.us";
            public const string DoD_EWS_ResourceUrl = "https://outlook-dod.office365.us";

            //public const string Global_GraphResourceUrl = "https://graph.windows.net";
            //public const string German_GraphResourceUrl = "https://graph.cloudapi.de";
            //public const string China_GraphResourceUrl = "https://graph.chinacloudapi.cn/";

            public const string Global_MicrosoftGraph_ResourceUrl = "https://graph.microsoft.com";
            public const string German_MicrosoftGraph_ResourceUrl = "https://graph.microsoft.de";
            public const string China_MicrosoftGraph_ResourceUrl = "https://microsoftgraph.chinacloudapi.cn";
            public const string Gov_MicrosoftGraph_ResourceUrl = "https://graph.microsoft.us";
            public const string Dod_MicrosoftGraph_ResourceUrl = "https://dod-graph.microsoft.us";

            public const string Global_Authority = "https://login.microsoftonline.com";
            public const string German_Authority = "https://login.microsoftonline.de";
            public const string China_Authority = "https://login.chinacloudapi.cn";
            public const string Gov_Authority = "https://login.microsoftonline.us";

            public const string Global_InitialDomainNameSuffix = "onmicrosoft.com";
            public const string German_InitialDomainNameSuffix = "onmicrosoft.de";
            public const string China_InitialDomainNameSuffix = "partner.onmschina.cn";

        }

        class AzureEnvironmentCache
        {
            static readonly ConcurrentDictionary<string, AzureEnvironmentInstance> map = new ConcurrentDictionary<string, AzureEnvironmentInstance>(StringComparer.Ordinal);

            public AzureEnvironmentInstance this[string domain]
            {
                get
                {
                    return map[domain.ToLower()];
                }
                set
                {
                    map[domain.ToLower()] = value;
                }
            }

            public bool TryGetValue(string domain, out AzureEnvironmentInstance value)
            {
                return map.TryGetValue(domain.ToLower(), out value);
            }

        }
    }

    public enum AzureCloudType
    {
        Global,
        China,
        German,
        USGovernment,
        USGovernment_DoD,
        PPE,
    }

    class Resource
    {
        public string Authority { get; private set; }

        public string EWS { get; private set; }
        public string MSGraph { get; private set; }

        public Resource(string authority, string ewsResourceUrl, string msGroupResourceUrl)
        {
            this.Authority = authority;
            this.EWS = ewsResourceUrl;
            this.MSGraph = msGroupResourceUrl;
        }

    }
}
