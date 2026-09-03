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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace AvePoint.ObjectModel.Common
{
    public class ADFSAuthenticationProvider : IAuthenticationProvider
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(ADFSAuthenticationProvider));
        private string providerName = string.Empty;//set provider name for multiple provide environment in the feature
        public AuthenticationResult Login(string siteUrl, AveBPOSAccountInfo userAccountInfo)
        {
            AuthenticationResult result = new AuthenticationResult(AutheStatus.Failed, AveAuthenticationMode.ADFS);
            try
            {
                CookieContainer cookie;
                try
                {
                    cookie = InternalGetAdfsFormSamlToken(siteUrl, userAccountInfo.UserName, userAccountInfo.Password);
                }
                catch
                {
                    cookie = InternalGetAdfsWindowsSamlToken(siteUrl, userAccountInfo.UserName, userAccountInfo.Password);
                }
                if (cookie != null)
                {
                    result = new AuthenticationResult(AutheStatus.Successful, AveAuthenticationMode.ADFS, cookie);
                    log.Debug("login site {0} successfully using ADFS authentication", siteUrl);
                }
            }
            catch (Exception e)
            {
                log.Warn("Login failed by ADFS authentication. Url:{0}, user:{1}, Error:{2}", siteUrl, userAccountInfo.UserName, e);
            }
            return result;
        }
        
        private CookieContainer InternalGetAdfsFormSamlToken(string siteUrl, string username, string password)
        {
            Uri hosturi = new Uri(siteUrl);
            string sphost = String.Format("{0}://{1}:{2}", hosturi.Scheme, hosturi.Host, hosturi.Port);
            string AdfsUrl = GetAdfsServerUrl(sphost, this.providerName);
            Uri AdfsUri = new Uri(AdfsUrl);
            string identifier = System.Web.HttpUtility.ParseQueryString(AdfsUri.Query).Get("wtrealm");
            string wtrealm = System.Web.HttpUtility.UrlEncode(identifier);
            string wa = System.Web.HttpUtility.UrlEncode(System.Web.HttpUtility.ParseQueryString(AdfsUri.Query).Get("wa"));
            string wctx = string.Format("{0}/_layouts/15/Authenticate.aspx?Source={1}", siteUrl, HttpUtility.UrlEncode(hosturi.AbsoluteUri));
            wctx = HttpUtility.UrlEncode(wctx);
            string AdfsEndPoint = String.Format("{0}://{1}/adfs/ls?wa={2}&wtrealm={3}&wctx={4}", AdfsUri.Scheme, AdfsUri.Host, wa, wtrealm, wctx);
            HttpWebRequest webRequest = HttpWebRequest.Create(AdfsEndPoint) as HttpWebRequest;
            XmlDocument document = new XmlDocument();
            webRequest.Method = "POST";
            webRequest.Referer = AdfsEndPoint;
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Accept = "text/html, application/xhtml+xml, */*";
            webRequest.CookieContainer = new CookieContainer();
            webRequest.ContentLength = 0;
            webRequest.Timeout = 30000;
            webRequest.Credentials = new NetworkCredential(username, password);
            webRequest.ClientCertificates = new System.Security.Cryptography.X509Certificates.X509CertificateCollection();
            //webRequest.Headers.Add(HttpRequestHeader.Authorization, "UserName=kbh%5Csp_ap1&Password=1qaz2wsx%21&AuthMethod=FormsAuthentication");
            string wresult = string.Empty;
            string FedTokenHost = string.Empty;
            string content = string.Format("UserName={0}&Password={1}&AuthMethod={2}", username, password, "FormsAuthentication");
            if (!string.IsNullOrEmpty(content))
            {
                byte[] contentBytes = Encoding.UTF8.GetBytes(content);
                webRequest.ContentLength = contentBytes.Length;
                using (Stream requestStream = webRequest.GetRequestStream())
                {
                    requestStream.Write(contentBytes, 0, contentBytes.Length);
                }
            }
            using (HttpWebResponse commitResponse = webRequest.GetResponse() as HttpWebResponse)
            {
                using (StreamReader reader = new StreamReader(commitResponse.GetResponseStream()))
                {
                    document.LoadXml(reader.ReadToEnd());
                }
            }
            return InternalGetAuthenticationCookie(document);
        }

        private CookieContainer InternalGetAdfsWindowsSamlToken(string siteUrl, string username, string password)
        {
            Uri hosturi = new Uri(siteUrl);
            string sphost = String.Format("{0}://{1}:{2}", hosturi.Scheme, hosturi.Host, hosturi.Port);
            string AdfsUrl = GetAdfsServerUrl(sphost, this.providerName);
            Uri AdfsUri = new Uri(AdfsUrl);
            string identifier = System.Web.HttpUtility.ParseQueryString(AdfsUri.Query).Get("wtrealm");
            string wtrealm = System.Web.HttpUtility.UrlEncode(identifier);
            string wa = System.Web.HttpUtility.UrlEncode(System.Web.HttpUtility.ParseQueryString(AdfsUri.Query).Get("wa"));
            string wctx = string.Format("{0}/_layouts/15/Authenticate.aspx?Source={1}", siteUrl, System.Web.HttpUtility.UrlEncode(hosturi.AbsoluteUri));
            wctx = System.Web.HttpUtility.UrlEncode(wctx);
            string AdfsEndPoint = String.Format("{0}://{1}/adfs/ls/auth/integrated/?wa={2}&wtrealm={3}&wctx={4}", AdfsUri.Scheme, AdfsUri.Host, wa, wtrealm, wctx);
            HttpWebRequest webRequest = HttpWebRequest.Create(AdfsEndPoint) as HttpWebRequest;
            webRequest.Method = "GET";
            webRequest.CookieContainer = new CookieContainer();
            webRequest.ContentLength = 0;
            webRequest.Timeout = 30000;
            webRequest.Credentials = new NetworkCredential(username, password);
            webRequest.ClientCertificates = new System.Security.Cryptography.X509Certificates.X509CertificateCollection();
            XmlDocument document = new XmlDocument();
            using (HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse)
            {
                if (webResponse.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        string value = reader.ReadToEnd();

                        document.LoadXml(value);

                    }
                }
            }
            return InternalGetAuthenticationCookie(document);
        }

        private CookieContainer InternalGetAuthenticationCookie(XmlDocument document)
        {
            string wctx = string.Empty;
            string wa = string.Empty;
            string wresult = string.Empty;
            string FedTokenHost = string.Empty;
            XmlElement form = document.DocumentElement.GetElementsByTagName("form")[0] as XmlElement;
            FedTokenHost = form.GetAttribute("action");
            foreach (XmlElement ele in document.DocumentElement.GetElementsByTagName("input"))
            {
                if (ele.GetAttribute("name").Equals("wa"))
                {
                    wa = System.Web.HttpUtility.UrlEncode(ele.GetAttribute("value"));
                }
                if (ele.GetAttribute("name").Equals("wresult"))
                {
                    wresult = System.Web.HttpUtility.UrlEncode(ele.GetAttribute("value"));
                }
                if (ele.GetAttribute("name").Equals("wctx"))
                {
                    wctx = System.Web.HttpUtility.UrlEncode(ele.GetAttribute("value"));
                }
            }
            if (!String.IsNullOrEmpty(wresult) && !String.IsNullOrEmpty(FedTokenHost))
            {
                HttpWebRequest webRequest = HttpWebRequest.Create(FedTokenHost) as HttpWebRequest;
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.Method = "POST";
                webRequest.CookieContainer = new CookieContainer();
                webRequest.ClientCertificates = new System.Security.Cryptography.X509Certificates.X509CertificateCollection();
                string content = string.Format("wa={0}&wresult={1}&wctx={2}", wa, wresult, wctx);
                if (!string.IsNullOrEmpty(content))
                {
                    byte[] contentBytes = Encoding.UTF8.GetBytes(content);
                    webRequest.ContentLength = contentBytes.Length;
                    using (Stream requestStream = webRequest.GetRequestStream())
                    {
                        requestStream.Write(contentBytes, 0, contentBytes.Length);
                    }
                }
                else
                {
                    webRequest.ContentLength = 0;
                }
                using (HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse)
                {
                    if (webResponse.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                        {
                            string value = reader.ReadToEnd();
                        }
                        return webRequest.CookieContainer;
                    }
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private string GetAdfsServerUrl(string sphost, string providerName)
        {
            string requestUrl = String.Format(@"{0}/_trust/default.aspx?trust={1}&ReturnUrl=", sphost, providerName);
            HttpWebRequest sharepointRequest = HttpWebRequest.Create(requestUrl) as HttpWebRequest;
            sharepointRequest.Method = "POST";
            sharepointRequest.ContentType = "application/x-www-form-urlencoded";
            sharepointRequest.CookieContainer = new CookieContainer();
            sharepointRequest.AllowAutoRedirect = false; // This is important
            sharepointRequest.ContentLength = 0;
            sharepointRequest.KeepAlive = false;
            sharepointRequest.Timeout = 3000;
            sharepointRequest.ClientCertificates = new System.Security.Cryptography.X509Certificates.X509CertificateCollection();
            HttpWebResponse webResponse = sharepointRequest.GetResponse() as HttpWebResponse;
            string result = webResponse.Headers["Location"];
            webResponse.Close();
            sharepointRequest.Abort();
            return result;
        }
    }
}
