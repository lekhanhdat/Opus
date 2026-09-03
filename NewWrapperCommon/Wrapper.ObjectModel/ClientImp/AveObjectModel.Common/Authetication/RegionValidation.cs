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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.SharePointBrowser;
using DnsClient;
using DnsClient.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

/// <summary>
/// 这部分code来自于FLY Manager，由于DocAve Manager存在没有外网的情况，因此通过Browser 来获取tenantid和AzureRegions
/// </summary>

namespace AvePoint.Wrapper.Common
{
    public class TenantServiceUrls
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
        public const string USGoverment_EWS_ResourceUrl = "https://outlook.office365.us";

        public const string Global_GraphResourceUrl = "https://graph.windows.net";
        public const string German_GraphResourceUrl = "https://graph.cloudapi.de";
        public const string China_GraphResourceUrl = "https://graph.chinacloudapi.cn";
        public const string USGoverment_GraphResourceUrl = "https://graph.windows.net";

        public const string Global_MicrosoftGraph_ResourceUrl = "https://graph.microsoft.com";
        public const string German_MicrosoftGraph_ResourceUrl = "https://graph.microsoft.de";
        public const string China_MicrosoftGraph_ResourceUrl = "https://microsoftgraph.chinacloudapi.cn";
        public const string USGoverment_MicrosoftGraph_ResourceUrl = "https://graph.microsoft.us";

        public const string Global_Authority = "https://login.microsoftonline.com";
        public const string German_Authority = "https://login.microsoftonline.de";
        public const string China_Authority = "https://login.chinacloudapi.cn";
        public const string USGoverment_Authority = "https://login.microsoftonline.us";

        public const string Global_InitialDomainNameSuffix = "onmicrosoft.com";
        public const string German_InitialDomainNameSuffix = "onmicrosoft.de";
        public const string China_InitialDomainNameSuffix = "partner.onmschina.cn";
        public const string USGoverment_InitialDomainNameSuffix = "onmicrosoft.us";

        public static string DefaultAppClientId = "c661a4d2-b179-4a62-a19b-2076ae563d0b";
        public static string DefaultCNAppClientId = "69124d17-7123-4a22-bebb-9faaf098ed00";

        public static readonly Dictionary<string, AzureRegions> ResourceMapping = new Dictionary<string, AzureRegions>(StringComparer.OrdinalIgnoreCase)
        {
            { "graph.microsoft.us",AzureRegions.AzureUSGov },
            { "dod-graph.microsoft.us", AzureRegions.AzureUSGovDoD},
            { "graph.microsoft.de", AzureRegions.AzureGerman},
            { "microsoftgraph.chinacloudapi.cn", AzureRegions.Azure21V},
            { "graph.microsoft.com",AzureRegions.AzureGlobal }
        };

        public static string AuthURLFromTenantRegion(AzureRegions region)
        {
            string authURL = TenantServiceUrls.Global_Authority;
            switch (region)
            {
                case AzureRegions.Azure21V:
                    authURL = TenantServiceUrls.China_Authority;
                    break;
                case AzureRegions.AzureGerman:
                    authURL = TenantServiceUrls.German_Authority;
                    break;
                case AzureRegions.AzureUSGov:
                case AzureRegions.AzureUSGovDoD:
                    authURL = TenantServiceUrls.USGoverment_Authority;
                    break;
                case AzureRegions.AzureGlobal:
                default:
                    authURL = TenantServiceUrls.Global_Authority;
                    break;
            }
            return authURL;
        }

        public static string GraphURLFromTenantRegion(AzureRegions region)
        {
            string authURL = TenantServiceUrls.Global_GraphResourceUrl;
            switch (region)
            {
                case AzureRegions.Azure21V:
                    authURL = TenantServiceUrls.China_GraphResourceUrl;
                    break;
                case AzureRegions.AzureGerman:
                    authURL = TenantServiceUrls.German_GraphResourceUrl;
                    break;
                case AzureRegions.AzureUSGov:
                    authURL = TenantServiceUrls.USGoverment_GraphResourceUrl;
                    break;
                case AzureRegions.AzureGlobal:
                default:
                    authURL = TenantServiceUrls.Global_GraphResourceUrl;
                    break;
            }
            return authURL;
        }

        public static AzureRegions GetTenantRegion(string domainAccount)
        {
            if (domainAccount.EndsWith(TenantServiceUrls.China_InitialDomainNameSuffix, StringComparison.OrdinalIgnoreCase))
            {
                return AzureRegions.Azure21V;
            }
            else if (domainAccount.EndsWith(TenantServiceUrls.German_InitialDomainNameSuffix, StringComparison.OrdinalIgnoreCase))
            {
                return AzureRegions.AzureGerman;
            }
            else if(domainAccount.EndsWith(TenantServiceUrls.USGoverment_InitialDomainNameSuffix, StringComparison.OrdinalIgnoreCase))
            {
                return AzureRegions.AzureUSGov;
            }
            return AzureRegions.AzureGlobal;
        }
    }

    class RegionValidation
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(RegionValidation));
        public static AzureRegions LoadTenantRegionWithUserName(string userName, ref string tenantId)
        {
            string tenantName = userName.Substring(userName.IndexOf('@') + 1);
            AzureRegions region = GetAzureRegion(tenantName, ref tenantId);
            if (region == AzureRegions.Unknown)
            {
                logger.Info("We can't get tenant region by welknown endpoint with name {0} will load region by name directlly.", userName);
                region = TenantServiceUrls.GetTenantRegion(userName);
            }
            return region;
        }

        private static AzureRegions GetTenantIDByName(List<AzureRegions> supportRegions, string tenantName, ref string tenantId)
        {
            foreach (var region in supportRegions)
            {
                var loginResource = TenantServiceUrls.AuthURLFromTenantRegion(region);
                try
                {
                    string postUrl = loginResource + "/" + tenantName + "/.well-known/openid-configuration";
                    string html = HttpGet(postUrl);
                    if (!string.IsNullOrEmpty(html) && html.IndexOf("authorization_endpoint", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        int start = html.IndexOf(loginResource) + loginResource.Length + 1;
                        tenantId = html.Substring(start, 36);
                        logger.Info("Get Tenant Region with name {0} for resource {1}.", tenantName, loginResource);
                        return region;
                    }
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while validate Tenant with name {0} for resource {1}. Error{2} ", tenantName, loginResource, e.ToString());
                }
            }
            return AzureRegions.Unknown;
        }

        private static AzureRegions GetAzureRegion(string tenantName, ref string tenantId)
        {
            AzureRegions azureType;
            try
            {
                string endPointUrl = string.Format("https://odc.officeapps.live.com/odc/emailhrd/getfederationprovider?domain={0}", tenantName);
                string federationProvider = HttpGet(endPointUrl);
                switch (federationProvider)
                {
                    case "Global":
                        azureType = AzureRegions.AzureGlobal;
                        break;
                    case "partner.microsoftonline.cn":
                        azureType = AzureRegions.Azure21V;
                        break;
                    case "microsoftonline.de":
                        azureType = AzureRegions.AzureGerman;
                        break;
                    case "microsoftonline.us":
                        azureType = AzureRegions.AzureUSGov;
                        break;
                    default:
                        azureType = GetAzureTypeByFederationProvider(federationProvider);
                        break;
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while looking up azure type by officeapps. Error{0} ", e);
                // find with dns
                azureType = LookupByDns(tenantName);
            }
            if (azureType != AzureRegions.Unknown)
            {
                var loginResource = TenantServiceUrls.AuthURLFromTenantRegion(azureType);
                try
                {
                    string postUrl = loginResource + "/" + tenantName + "/.well-known/openid-configuration";
                    var html = HttpGet(postUrl);
                    if (!string.IsNullOrEmpty(html) && html.IndexOf("authorization_endpoint", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        int start = html.IndexOf(loginResource) + loginResource.Length + 1;
                        tenantId = html.Substring(start, 36);
                        logger.Info("Get Tenant Region with name {0} for resource {1}.", tenantName, loginResource);
                    }
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while validate Tenant with name {0} for resource {1}. Error{2} ", tenantName, loginResource, e.ToString());
                    List<AzureRegions> supportRegions = new List<AzureRegions>() { AzureRegions.AzureGlobal, AzureRegions.AzureGerman, AzureRegions.AzureUSGov, AzureRegions.Azure21V };
                    supportRegions.Remove(azureType);
                    logger.Info("start to validate Tenant with name {0} by welknown endpoint", tenantName);
                    azureType = GetTenantIDByName(supportRegions, tenantName, ref tenantId);
                }
            }
            return azureType;
        }

        private static AzureRegions GetAzureTypeByFederationProvider(string fp)
        {
            using (var client = new WebClient())
            {
                var webResult = HttpGet($"https://officeclient.microsoft.com/config16?fp={fp}&lcid=1033&syslcid=1033&uilcid=1033&crev=3a&tokens=MsGraphBaseURL");
                XmlDocument xd = new XmlDocument();
                xd.LoadXml(webResult);
                var ns = new XmlNamespaceManager(xd.NameTable);
                ns.AddNamespace("o", "urn:schemas-microsoft-com:office:office");
                var node = xd.DocumentElement.SelectSingleNode("o:tokens/o:token[@o:name='MsGraphBaseURL']", ns);
                var baseUrl = node.InnerText;
                return TenantServiceUrls.ResourceMapping.ContainsKey(baseUrl) ? TenantServiceUrls.ResourceMapping[baseUrl] : AzureRegions.AzureGlobal;
            }
        }

        private static AzureRegions LookupByDns(string tenantName)
        {
            var azureType = AzureRegions.AzureGlobal;
            try
            {
                var lookup = new LookupClient();
                var result = lookup.Query(tenantName, QueryType.MX);
                foreach (var item in result.Answers)
                {
                    var mxRecord = item as MxRecord;
                    if (mxRecord != null)
                    {
                        var exchange = mxRecord.Exchange;
                        if (exchange.ToString().EndsWith("com."))
                        {
                            azureType = AzureRegions.AzureGlobal;
                        }
                        else if (exchange.ToString().EndsWith("us."))
                        {
                            azureType = AzureRegions.AzureUSGov;
                        }
                        else if (exchange.ToString().EndsWith("cn."))
                        {
                            azureType = AzureRegions.Azure21V;
                        }
                        else if (exchange.ToString().EndsWith("de."))
                        {
                            azureType = AzureRegions.AzureGerman;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while looking up azure type by dns. Error{0} ",  e);
            }
            return azureType;
        }

        private static string HttpGet(string url, int retryCount = 3)
        {
            string sharePointPageHtml = string.Empty;

            for (int i = 0; i < retryCount; i++)
            {
                var httpGetRequest = HttpWebRequest.Create(url) as HttpWebRequest;
                httpGetRequest.Method = "GET";
                try
                {
                    WebResponse response = httpGetRequest.GetResponse();
                    sharePointPageHtml = new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
                    response.Close();
                    break;
                }
                catch (WebException ex)
                {
                    logger.Warn("failed to send request:{0} due to: {1}, retry: {2}", url, ex, i);
                }
            }
            return sharePointPageHtml;
        }
    }
}
