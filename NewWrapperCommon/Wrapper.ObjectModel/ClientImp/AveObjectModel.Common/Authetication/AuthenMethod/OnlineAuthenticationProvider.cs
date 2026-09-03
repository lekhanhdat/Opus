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
using AvePoint.Office365.Api;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Protocols;
using System.Xml;

namespace AvePoint.ObjectModel.Common
{
    public class OnlineAuthenticationProvider : IAuthenticationProvider
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(OnlineAuthenticationProvider));

        public AuthenticationResult Login(string siteUrl, AveBPOSAccountInfo userAccountInfo)
        {
            AveAuthenticationMode AuthMode = AveAuthenticationMode.None;
            AveAuthenticationMode ErrorAuthMode = AveAuthenticationMode.None;
            try
            {
                var tokenProviders = Convert2TokenProviders(userAccountInfo, userAccountInfo.ConnectionType);
                AuthMode = GetAuthenticationMode(userAccountInfo.ConnectionType);
                for (var i = 1; i <= tokenProviders.Count; i++)
                {
                    var p = tokenProviders[i - 1];
                    try
                    {
                        ErrorAuthMode = p.TokenType == TokenType.Bearer ? AveAuthenticationMode.OnlineAppToken : AveAuthenticationMode.OnlineServiceAccount;
                        var token = p.GetToken(new Uri(siteUrl));
                    }
                    catch (Exception ex)
                    {
                        log.Warn("try to authecate failed with {0}, exception : {1}", ErrorAuthMode, ex);
                        throw;
                    }
                }
                log.Debug("Login site {0} successfully using online authentication", siteUrl);
                return new AuthenticationResult(AutheStatus.Successful, AuthMode, null, tokenProviders);
            }
            catch (Exception e)
            {
                if (e.GetType().FullName.Equals("Microsoft.SharePoint.Client.IdcrlException"))
                {
                    int errorCode = Convert.ToInt32(AveAssemblyUtility.GetPropertyValue(e, "ErrorCode"));
                    log.Warn("IDCRL Error Code: {0}", errorCode);
                    if (errorCode == -2147186445 || errorCode == -2147186446)
                    {
                        throw new IncorrectUserNameOrPasswordException(AveInternalResourceKey.Wrapper_Exception_Common_IncorrectUserNameOrPassword);
                    }
                    if (errorCode == -2147186631 || errorCode == -2147186639)
                    {
                        throw new PasswordExpiredException(AveInternalResourceKey.Wrapper_Exception_Common_PasswordExpired);
                    }
                    if (errorCode == -2147186643)
                    {
                        throw new NonOffice365AccountException(AveInternalResourceKey.Wrapper_Exception_Common_NonOffice365Account);
                    }
                    if (errorCode == -2147186655)
                    {
                        throw new AccountDisableException(AveInternalResourceKey.Wrapper_Exception_Common_AccountDisable);
                    }
                }
                else if (e.GetType().FullName.Equals("System.AggregateException"))
                {
                    var internalException = e.InnerException;
                    if(internalException != null && internalException.GetType().FullName.Equals("Microsoft.IdentityModel.Clients.ActiveDirectory.AdalServiceException"))
                    {
                        if (internalException.Message.ToLower().Contains("AADSTS90002".ToLower()))
                        {
                            throw new AppTokenTenantIdException(internalException.Message);
                        }
                        else if(internalException.Message.ToLower().Contains("AADSTS700016".ToLower()))
                        {
                            throw new AppTokenClientIdException(internalException.Message);
                        }
                        else if (internalException.Message.ToLower().Contains("AADSTS700027".ToLower()))
                        {
                            throw new AppTokenCertificateException(internalException.Message);
                        }
                        else
                        {
                            throw new AppTokenUnknownException(internalException.Message);
                        }
                    }
                }
                else if (e.GetType().FullName.Equals("AvePoint.Wrapper.Common.Office365SiteExpiredException"))
                {
                    throw new Office365SiteExpiredException(AveInternalResourceKey.Wrapper_Exception_Common_Office365SiteExpired);
                }
                if (ErrorAuthMode == AveAuthenticationMode.OnlineAppToken)
                {
                    log.Warn("Failed to login SharePoint Online. Site Collection Url: {0}, clientId: {1}, Message: {2}", siteUrl, userAccountInfo.ClientId, e.ToString());
                }
                else
                {
                    log.Warn("Failed to login SharePoint Online. Site Collection Url: {0}, Username: {1}, Message: {2}", siteUrl, userAccountInfo.UserName, e.ToString());
                }
            }
            return new AuthenticationResult(AutheStatus.Failed, AuthMode);
        }

        private AveAuthenticationMode GetAuthenticationMode(BposConnectionType connectionType)
        {
            switch(connectionType)
            {
                case BposConnectionType.ServiceAccount:
                    return AveAuthenticationMode.OnlineServiceAccount;
                case BposConnectionType.AppToken:
                    return AveAuthenticationMode.OnlineAppToken;
                case BposConnectionType.MixAuthorize:
                    return AveAuthenticationMode.OnlineAppToken | AveAuthenticationMode.OnlineServiceAccount;
            }
            throw new Exception(string.Format("Unhandled connection type: {0}", connectionType));
        }

        //rectify url, for example: convert http(s)://webappUrl/sites/site1/SitePages/Home.aspx to http(s)://webappUrl/sites/site1
        private string TryToRectifyUrl(string url , ITokenProvider tokenProvider)
        {
            int index = url.LastIndexOf('/');
            string urlHeader = url.StartsWith("https") ? "https://" : "http://";
            
            if (url.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
            {
                url = url.Substring(0, index);
            }
            if (url.Contains('%'))
            {
                url = System.Web.HttpUtility.UrlDecode(url);
            }
            while (true)
            {
                try
                {
                    AveSiteService siteService = CreateSiteService(url, tokenProvider);
                    string siteInfo = siteService.GetSite(url);
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(siteInfo);
                    if (doc.DocumentElement.HasAttribute("Url"))
                    {
                        url = doc.DocumentElement.GetAttribute("Url");
                    }
                }
                catch (WebException we)
                {
                    using (HttpWebResponse response = we.Response as HttpWebResponse)
                    {
                       log.Warn("rectify url {0} failed with webexception, Error Message:{1}", url, we.ToString());
                    }
                }
                catch (SoapException se) //SAAS-14000 判断传入的Url存不存在,SAAS-23125,删除备份源端节点，IB时提示语错误，如果传入The system cannot find the path specified.应提示为节点不存在。
                {
                    //if (!string.IsNullOrEmpty(se.Detail.InnerText) &&
                    //    (se.Detail.InnerText.Contains("Operation is not valid due to the current state of the object.") ||
                    //    se.Detail.InnerText.Contains("The system cannot find the path specified.")))
                    //{
                    //    log.Warn("The site collection {0} is not available.", url);
                    //    throw new FileNotFoundException(string.Format("The site collection {0} is not available", url));
                    //}
                    log.Warn("rectify url {0} failed with soapexception, Error Message:{1}", url, se.Detail.InnerText);
                }
                catch (Exception e)
                {
                    log.Warn("rectify url {0} failed, Error Message:{1}", url, e.ToString());
                }
                return url;
            }
        }

        private AveSiteService CreateSiteService(string url , ITokenProvider tokenProvider)
        {
            AveSiteService siteService = new AveSiteService(url + "/_vti_bin/Sites.asmx");
            siteService.Timeout = 3 * 60 * 1000;
            siteService.TokenProvider = tokenProvider;
            return siteService;
        }


        private List<ITokenProvider> Convert2TokenProviders(AveBPOSAccountInfo info, BposConnectionType connectionType)
        {
            List<ITokenProvider> providers = new List<ITokenProvider>();

            if (info != null)
            {
                var region = GetAveAzureEnvironment(info);
                switch(connectionType)
                {
                    case BposConnectionType.ServiceAccount:
                        providers.Add(new SPOIDCLRTokenProvider(info.UserName, info.Password, region));
                        break;
                    case BposConnectionType.AppToken:
                        providers.Add(new AppOnlyBearerTokenProvider(info.TenantId, info.ClientId, info.AppCert, region));
                        break;
                    case BposConnectionType.MixAuthorize:
                        providers.Add(new SPOIDCLRTokenProvider(info.UserName, info.Password, region));
                        providers.Add(new AppOnlyBearerTokenProvider(info.TenantId, info.ClientId, info.AppCert, region));
                        break;
                }
            }
            return providers;
        }

        public static AveAzureEnvironment GetAveAzureEnvironment(AveBPOSAccountInfo info)
        {

            var regionInfo = info.AzureRegion;
            if (regionInfo == AzureRegions.Unknown)
            {
                string tenantID = string.Empty;
                regionInfo = RegionValidation.LoadTenantRegionWithUserName(info.UserName, ref tenantID);
            }
            switch (regionInfo)
            {
                case AzureRegions.AzureGlobal:
                    return AveAzureEnvironment.AzureCloud;
                case AzureRegions.Azure21V:
                    return AveAzureEnvironment.AzureChinaCloud;
                case AzureRegions.AzureGerman:
                    return AveAzureEnvironment.AzureGermanyCloud;
                case AzureRegions.AzureUSGov:
                case AzureRegions.AzureUSGovDoD:
                    return AveAzureEnvironment.USGovernment;
                default:
                    return AveAzureEnvironment.None;
            }
        }
        private CookieContainer AssembleSPOIDCRLFromStsToken(string siteUrl,string stsToken)
        {
            var domain = new Uri(siteUrl).Host;
            string cookieName = stsToken.Substring(0, stsToken.IndexOf('='));
            string cookieValue = stsToken.Substring(stsToken.IndexOf('=') + 1);
            CookieContainer cookies = new CookieContainer();
            string newDomain = domain.Contains(".") ? domain.Substring(domain.IndexOf('.')) : domain;
            cookies.Add(new Cookie(cookieName, cookieValue, "/", newDomain));
            return cookies;
        }

    }
}
