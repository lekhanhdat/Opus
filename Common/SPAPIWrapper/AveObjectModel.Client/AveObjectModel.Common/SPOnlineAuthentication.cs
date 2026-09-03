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
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using System.Threading;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;
using System.Security;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.Wrapper.Resource;
using AvePoint.Wrapper.Common;
using Microsoft365.Authentication;
using System.Web;
using AvePoint.GCommon.Utility;

namespace AvePoint.ObjectModel.Common
{
    /// <summary>
    /// Cannot be removed because Common Browser/RC are using this class
    /// </summary>
    class SPOnlineAuthentication
    {
        private static readonly AvePoint.GCommon.AveLogger mLog = AvePoint.GCommon.AveLogger.GetInstance(typeof(SPOnlineAuthentication));



        private string mSiteUrl;
        private string mWebAppUrl;
        private bool mIsRedirectedToHttps = false;
        private string mDomainUrl;
        private string mWebRelativeUrl;
        private string mUrlHeader;


        public string SiteUrl { get { return mSiteUrl; } }

        public SPOnlineAuthentication(string siteUrl)
        {
            mSiteUrl = siteUrl;
            mUrlHeader = mSiteUrl.StartsWith("https://") ? "https://" : "http://";
            Uri siteUri = new Uri(siteUrl);
            mDomainUrl = siteUri.Host;
            mWebRelativeUrl = siteUri.AbsolutePath;
            mWebAppUrl = AveUrlUtility.GetServerUrl(mSiteUrl);
            if (siteUrl.StartsWith("http://"))
            {
                mIsRedirectedToHttps = GetRealUrlForHttpSite("/_layouts/authenticate.aspx").StartsWith("https", StringComparison.OrdinalIgnoreCase);
                mSiteUrl = "https" + mSiteUrl.Substring("http".Length);
            }
        }

        private CookieContainer SPOnlineLogin(ITokenProvider tokenProvider)
        {
            try
            {
                //此处需要用自己写的credentials类，因为一个客户的环境配的ADFS用Client API里自带的不好用saas-8349

                var securityToken = tokenProvider.GetToken(new Uri(mSiteUrl));

                if (tokenProvider.TokenType != TokenType.Bearer)
                {
                    return AssembleSPOIDCRLFromStsToken(securityToken);
                }

                return null;
            }
            catch (TargetInvocationException te)
            {
                mLog.Warn("failed to pass authentication due to: {0}", te.InnerException.ToString());
                throw te.InnerException;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public CookieContainer Login(ITokenProvider tokenProvider)
        {
            CookieContainer cookieContainer = null;
            try
            {
                cookieContainer = SPOnlineLogin(tokenProvider);
            }
            catch (ArgumentException ae)
            {
                mLog.Warn("Failed to login SharePoint Online. Site Collection Url: {0}, Username: {1}, Message: {2}", mSiteUrl, tokenProvider.Identifier, ae.ToString());
                throw new NonOffice365AccountException(WrapperReportResourceKey.Wrapper_ConnectSiteError.ToString(), WrapperRestoreReportResource.Wrapper_ConnectSiteError, ae.Message, SiteUrl);
            }
            catch (Exception e)
            {
                if (e.GetType().FullName.Equals("Microsoft.SharePoint.Client.IdcrlException"))
                {
                    int errorCode = Convert.ToInt32(AveAssemblyUtility.GetPropertyValue(e, "ErrorCode"));
                    mLog.Warn("IDCRL Error Code: {0}", errorCode);
                    if (errorCode == -2147186445 || errorCode == -2147186446)
                    {
                        throw new IncorrectUserNameOrPasswordException(WrapperReportResourceKey.Wrapper_IncorrectUserNameOrPasswordError.ToString(), WrapperRestoreReportResource.Wrapper_IncorrectUserNameOrPasswordError, SiteUrl);
                    }
                    if (errorCode == -2147186631 || errorCode == -2147186639)
                    {
                        throw new PasswordExpiredException(WrapperReportResourceKey.Wrapper_ConnectSiteError.ToString(), WrapperRestoreReportResource.Wrapper_ConnectSiteError, e.Message, SiteUrl);
                    }
                    if (errorCode == -2147186643)
                    {
                        throw new NonOffice365AccountException(WrapperReportResourceKey.Wrapper_ConnectSiteError.ToString(), WrapperRestoreReportResource.Wrapper_ConnectSiteError, e.Message, SiteUrl);
                    }
                    if (errorCode == -2147186655)
                    {
                        throw new AccountDisableException(WrapperReportResourceKey.Wrapper_AccountDisableError.ToString(), WrapperRestoreReportResource.Wrapper_AccountDisableError, tokenProvider.Identifier, SiteUrl);
                    }
                }
                else if (e.GetType().FullName.Equals("AvePoint.Wrapper.Common.Office365SiteExpiredException"))
                {
                    throw new Office365SiteExpiredException(WrapperReportResourceKey.Wrapper_ConnectSiteError.ToString(), WrapperRestoreReportResource.Wrapper_ConnectSiteError, e.Message, SiteUrl);
                }
                mLog.Warn("Failed to login SharePoint Online. Site Collection Url: {0}, Username: {1}, Message: {2}", mSiteUrl, tokenProvider.Identifier, e.ToString());
                throw;
            }
            finally
            {
            }
            //异常底层provider会抛出来，外围不需要自己抛
            return cookieContainer;
        }

      

        



        private CookieContainer AssembleSPOIDCRLFromStsToken(string stsToken)
        {
            string cookieName = stsToken.Substring(0, stsToken.IndexOf('='));
            string cookieValue = stsToken.Substring(stsToken.IndexOf('=') + 1);
            CookieContainer cookies = new CookieContainer();
            string newDomain = this.mDomainUrl.Contains(".") ? this.mDomainUrl.Substring(this.mDomainUrl.IndexOf('.')) : this.mDomainUrl;
            cookies.Add(new Cookie(cookieName, cookieValue, "/", newDomain));

            return cookies;
        }

        /*private HttpWebRequest GetWebRequest(string url, string method, string contentType, string content, bool includeCookieContainer = false, Dictionary<string, string> headers = null, Cookie[] cookies = null, bool allowAutoRedirect = true)
        {
            HttpWebRequest webRequest = HttpWebRequest.Create(url) as HttpWebRequest;
            webRequest.ContentType = contentType;
            webRequest.UserAgent = userAgent;
            webRequest.Method = method;
            webRequest.AllowAutoRedirect = allowAutoRedirect;
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    webRequest.Headers[header.Key] = header.Value;
                }
            }
            if (includeCookieContainer)
            {
                webRequest.CookieContainer = new CookieContainer();
            }
            if (cookies != null)
            {
                foreach (Cookie cookie in cookies)
                {
                    webRequest.CookieContainer.Add(cookie);
                }
            }
            if (!string.IsNullOrEmpty(content))
            {
                byte[] contentBytes = Encoding.UTF8.GetBytes(content);
                webRequest.ContentLength = contentBytes.Length;
                using (Stream requestStream = webRequest.GetRequestStream())
                {
                    requestStream.Write(contentBytes, 0, contentBytes.Length);
                }
            }

            return webRequest;
        }*/

        private string GetRealUrlForHttpSite(string suffix)
        {
            try
            {
                HttpWebRequest webRequest = HttpWebRequest.Create(SecurityUtils.SanitizeRequestUrl(mSiteUrl + suffix)) as HttpWebRequest;
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.Method = WebRequestMethods.Http.Get;
                webRequest.AllowAutoRedirect = false;
                using (HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse)
                {
                    if (webResponse.StatusCode == HttpStatusCode.Found)
                    {
                        string realUrl = webResponse.Headers["Location"];
                        return realUrl.Equals("/_layouts/15/authenticate.aspx") ? GetRealUrlForHttpSite(realUrl) : realUrl;
                    }
                }
            }
            catch (WebException e) 
            {
                mLog.Warn($@"fail get real url for http site,ex:{e}");
            }    //2010 http site will throw here
            return mSiteUrl;
        }

       /* private XmlNamespaceManager GetExceptionSoapNameSpace(XmlDocument doc)
        {
            XmlNamespaceManager soapNP = new XmlNamespaceManager(doc.NameTable);
            soapNP.AddNamespace("a", "http://www.w3.org/2005/08/addressing");
            soapNP.AddNamespace("s", "http://www.w3.org/2003/05/soap-envelope");
            return soapNP;
        }*/
        


       /* private string GetXmlNodeInnerXmlUsingSoapNameSpace(ref XmlDocument doc, string nodePath, string content, ref XmlNamespaceManager nsManager, bool needOuterXml = false)
        {
            if (doc == null)
            {
                doc = new XmlDocument();
                doc.LoadXml(content);
                nsManager = GetSoapNameSpace(doc);
            }
            XmlNode node = doc.SelectSingleNode(nodePath, nsManager);
            return node != null ? (needOuterXml ? node.OuterXml : node.InnerXml) : string.Empty;
        }*/
    }
}

