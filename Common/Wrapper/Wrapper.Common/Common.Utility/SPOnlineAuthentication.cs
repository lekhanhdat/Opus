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
using mshtml;
using System.Web;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using System.Reflection;
using System.Security;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.Wrapper.Resource;
//using AvePoint.GCommon.Utility.Exceptions.Authentication;
//using Microsoft.SharePoint.Client;


namespace AvePoint.Wrapper.Common
{
    public class SPOnlineAuthentication
    {
        private static readonly AvePoint.GCommon.AveLogger mLog = AvePoint.GCommon.AveLogger.GetInstance(typeof(SPOnlineAuthentication));

        private const string msoStsUrl = "https://login.microsoftonline.com/extSTS.srf";
        private const string msoHrdUrl = "https://login.microsoftonline.com/GetUserRealm.srf";
        private const string userAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)";
        private const string spowssigninUri = "_forms/default.aspx?wa=wsignin1.0";
        private const string defaultSuffix = "_forms/default.aspx";
        private const int TimeOut = 3 * 60 * 1000;
        private const int ReadWriteTimeOut = 3 * 60 * 1000;
        private string mSiteUrl;
        private string mWebAppUrl;
        private bool mIsRedirectedToHttps = false;
        private string mDomainUrl;
        private string mWebRelativeUrl;
        private string mUrlHeader;
        private const string defaultO365Suffix = "onmicrosoft.com";

        public bool IsRedirectedToHttps { get { return mIsRedirectedToHttps; } }
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

        private CookieContainer LiveIdLogin(string username, string password)
        {
            LoginPageInfo loginInfo = VisitAuthenticateAspx();
            loginInfo.UserName = username;
            loginInfo.Password = password;
            LiveIdLoginPageInfo liveIdLoginInfo = VisitWinLiveSecurePostSrf(loginInfo, mSiteUrl);
            VisitDefaultPageInfo defaulPageInfo = VisitLoginSrf(liveIdLoginInfo, loginInfo);
            return PostHttpRequest(defaulPageInfo.PostUrl,
                                   "application/x-www-form-urlencoded",
                                   defaulPageInfo.PostData,
                                   new Cookie[] { new Cookie("RpsContextCookie", defaulPageInfo.RpsContextCookie, "/", mDomainUrl) });
        }

        private CookieContainer SPOnlineLogin(string username, string password)
        {
            try
            {
                //此处需要用自己写的credentials类，因为一个客户的环境配的ADFS用Client API里自带的不好用saas-8349
                SharePointOnlineCredentials sharePointOnlineCredentials = new SharePointOnlineCredentials(username, ConvertStringToSecureString(password));
                string securityToken = sharePointOnlineCredentials.GetAuthenticationCookie(new Uri(mSiteUrl));
                return AssembleSPOIDCRLFromStsToken(securityToken);
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

        private SecureString ConvertStringToSecureString(string str)
        {
            SecureString secStr = new SecureString();
            if (!string.IsNullOrEmpty(str))
            {
                for (int i = 0; i < str.Length; i++)
                {
                    secStr.AppendChar(str[i]);
                }
            }
            return secStr;
        }

        public CookieContainer Login(string userName, string password)
        {
            CookieContainer cookieContainer = null;
            try
            {
                cookieContainer = SPOnlineLogin(userName, password);
            }
            catch (ArgumentException ae)
            {
                mLog.Warn("Failed to login SharePoint Online. Site Collection Url: {0}, Username: {1}, Message: {2}", mSiteUrl, userName, ae.ToString());
                throw new NonOffice365AccountException(WrapperReportResourceKey.Wrapper_ConnectSiteError.ToString(), WrapperRestoreReportResource.Wrapper_ConnectSiteError, ae.Message, SiteUrl);
            }
            catch (Exception e)
            {
                if (e.GetType().FullName.Equals("Microsoft.SharePoint.Client.IdcrlException"))
                {
                    int errorCode = Convert.ToInt32(AveAssemblyUtility.GetPropertyValue(e, "ErrorCode"));
                    mLog.Warn("IDCRL Error Code: {0}", errorCode);
                    if (errorCode == -2147186655 || errorCode == -2147186445 || errorCode == -2147186446)
                    {
                        throw new IncorrectUserNameOrPasswordException(WrapperReportResourceKey.Wrapper_ConnectSiteError.ToString(), WrapperRestoreReportResource.Wrapper_ConnectSiteError, e.Message, SiteUrl);
                    }
                    if (errorCode == -2147186631 || errorCode == -2147186639)
                    {
                        throw new PasswordExpiredException(WrapperReportResourceKey.Wrapper_ConnectSiteError.ToString(), WrapperRestoreReportResource.Wrapper_ConnectSiteError, e.Message, SiteUrl);
                    }
                    if (errorCode == -2147186643)
                    {
                        throw new NonOffice365AccountException(WrapperReportResourceKey.Wrapper_ConnectSiteError.ToString(), WrapperRestoreReportResource.Wrapper_ConnectSiteError, e.Message, SiteUrl);
                    }
                }
                else if (e.GetType().FullName.Equals("AvePoint.Wrapper.Common.Office365SiteExpiredException"))
                {
                    throw new Office365SiteExpiredException(WrapperReportResourceKey.Wrapper_ConnectSiteError.ToString(), WrapperRestoreReportResource.Wrapper_ConnectSiteError, e.Message, SiteUrl);
                }
                mLog.Warn("Failed to login SharePoint Online. Site Collection Url: {0}, Username: {1}, Message: {2}", mSiteUrl, userName, e.ToString());
                throw;
            }
            finally
            {
#if DEBUG
#else
                string detailedInfo = userName + "#" + password;
                SecureString ss = new SecureString();
                foreach (char c in detailedInfo)
                {
                    ss.AppendChar(c);
                }
                mLog.Info("Login completed. Site Collection Url: {0}, Hashed code : {1}", mSiteUrl, CspCommunicationWrapper.WrapKeyToBase64String(ss));
#endif
            }
            if (cookieContainer != null && cookieContainer.GetCookies(new Uri(mSiteUrl))["SPOIDCRL"] != null)
            {
                return cookieContainer;
            }
            throw new Exception("Login failed, failed to get SPOIDCRL");
        }

        private void InitSSLSetting()
        {
            if (mSiteUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);
            }
        }

        private VisitDefaultPageInfo VisitLoginSrf(LiveIdLoginPageInfo liveIdLoginPageInfo, LoginPageInfo loginInfo)
        {
            string postContent = string.Format("wctx={0}&NAP={1}&wresult={2}&wa={3}&ANON={4}",
                                               HttpUtility.UrlEncode(liveIdLoginPageInfo.Wctx),
                                               HttpUtility.UrlEncode(liveIdLoginPageInfo.NAP),
                                               HttpUtility.UrlEncode(HttpUtility.HtmlDecode(liveIdLoginPageInfo.Wresult)),
                                               liveIdLoginPageInfo.Wa,
                                               HttpUtility.UrlEncode(liveIdLoginPageInfo.ANON));
            Dictionary<string, string> header = new Dictionary<string, string>();
            header["Cookie"] = string.Format("MSPRequ={0}; MSPOK={1}", loginInfo.MsRequ, loginInfo.MsOk);
            try
            {
                string content = SendHttpWebRequest("https://login.microsoftonline.com/login.srf",
                                                    WebRequestMethods.Http.Post,
                                                    "application/x-www-form-urlencoded",
                                                    postContent,
                                                    header);
                if (content.Contains("https://portal.microsoftonline.com/common/logincredprof.aspx?ru=https://login.microsoftonline.com/login.srf"))     //here to judge if the password has expired (Office 365 account only)
                {
                    throw new PasswordExpiredException("Password for the account has expired");
                }
                string[] inputValues = GetInputValues(content, new string[]{"<input type=\"hidden\" name=\"t\" id=\"t\" value=\"",
                                                                            "<form name=\"fmHF\" id=\"fmHF\" action=\""});
                if (string.IsNullOrEmpty(inputValues[0]))
                {
                    throw new AuthenticationFailedException(string.Empty, HttpStatusCode.Unauthorized);
                }
                VisitDefaultPageInfo pageInfo = new VisitDefaultPageInfo();
                pageInfo.PostUrl = inputValues[1];
                pageInfo.PostData = "t=" + HttpUtility.UrlEncode(inputValues[0]);
                pageInfo.RpsContextCookie = loginInfo.RpsContextCookie;
                mIsRedirectedToHttps = inputValues[1].StartsWith("https", StringComparison.OrdinalIgnoreCase);
                mSiteUrl = (mSiteUrl.StartsWith("http:") && IsRedirectedToHttps) ? "https:" + mSiteUrl.Substring("http:".Length) : mSiteUrl;    //support 2013 office365 http site
                return pageInfo;
            }
            catch (AuthenticationFailedException e)
            {
                throw new AuthenticationFailedException("failed in step two: post login.srf, status : {0}", e.FailedStatusCode);
            }
        }

        private string GetRelyingPartyUrl()
        {
            Uri siteUri = new Uri(mSiteUrl);
            Uri defaultAspxUri = new Uri(String.Format("{0}://{1}/{2}", siteUri.Scheme, siteUri.Authority, defaultSuffix));
            HttpWebRequest webRequest = GetWebRequest(defaultAspxUri.ToString(), WebRequestMethods.Http.Get, "application/soap+xml; charset=utf-8", null, false, null, null, false);
            try
            {
                using (HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse)
                {
                    if (webResponse.StatusCode == HttpStatusCode.Found)
                    {
                        return HttpUtility.ParseQueryString(webResponse.Headers["Location"])["wreply"];
                    }
                }
            }
            catch (Exception e)
            {
                string warnMsg = string.Format("Failed to get relying party url, error message : {0}", e.ToString());
            }
            return string.Empty;
        }

        private string ParameterizeSoapRequestTokenMsgWithUsernamePassword(string url, string username, string password, string toUrl)
        {
            string samlRTString = GetXmlNodeInnerXml("/Authentication/SAML11RequestTokenSOAPMsg", null,
                                                     typeof(SPOnlineAuthentication).Assembly.GetManifestResourceStream("AvePoint.Wrapper.Common.AdfsProtocol.xml"));
            samlRTString = samlRTString.Replace("[username]", username);
            samlRTString = samlRTString.Replace("[password]", password);
            samlRTString = samlRTString.Replace("[url]", url);
            samlRTString = samlRTString.Replace("[toUrl]", toUrl);

            return samlRTString;
        }

        private void CheckUnauthenticatedExceptionDetail(string username, string password)
        {
            string relyingPartyTrustUrl = GetRelyingPartyUrl();
            relyingPartyTrustUrl = new Uri(string.IsNullOrEmpty(relyingPartyTrustUrl) ? mSiteUrl : relyingPartyTrustUrl).Host;
            string response = SendHttpWebRequest(msoStsUrl, WebRequestMethods.Http.Post, "application/soap+xml; charset=utf-8", ParameterizeSoapRequestTokenMsgWithUsernamePassword(relyingPartyTrustUrl, username, password, msoStsUrl));
            XmlDocument doc = null;
            XmlNamespaceManager nsManager = null;
            string faultCode = GetXmlNodeInnerXmlUsingSoapNameSpace(ref doc, "/s:Envelope/s:Body/s:Fault/s:Detail/psf:error/psf:internalerror/psf:code", response, ref nsManager);
            if (faultCode.Equals("0x80041084"))   //need to change password
            {
                throw new PasswordExpiredException("Password for the account has expired");
            }
            else if (!string.IsNullOrEmpty(faultCode))
            {
                throw new Exception(GetXmlNodeInnerXmlUsingSoapNameSpace(ref doc, "/s:Envelope/s:Body/s:Fault/s:Detail/psf:error/psf:internalerror/psf:text", response, ref nsManager));
            }
        }

        private LoginPageInfo VisitAuthenticateAspx()
        {
            //Space will be encoded to "+" by method "HttpUtility.UrlEncode" and that will raise exceptions in the following steps, so forcibly replace "+" with "%20"
            HttpWebRequest webRequest = GetWebRequest(mSiteUrl.TrimEnd('/') + "/_layouts/Authenticate.aspx?Source=" + HttpUtility.UrlEncode(mWebRelativeUrl).Replace("+", "%20"),
                                                      WebRequestMethods.Http.Get,
                                                      "application/x-www-form-urlencoded",
                                                      null,
                                                      true);
            using (HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse)
            {
                if (webResponse.StatusCode == HttpStatusCode.OK)
                {
                    LoginPageInfo loginPageInfo = new LoginPageInfo();
                    Cookie RpsContextCookie = webRequest.CookieContainer.GetCookies(new Uri(mSiteUrl))["RpsContextCookie"];
                    if (RpsContextCookie != null && webResponse.Cookies["MSPRequ"] != null && webResponse.Cookies["MSPOK"] != null)
                    {
                        loginPageInfo.RpsContextCookie = RpsContextCookie.Value;
                        loginPageInfo.MsRequ = webResponse.Cookies["MSPRequ"].Value;
                        loginPageInfo.MsOk = webResponse.Cookies["MSPOK"].Value;
                        return loginPageInfo;
                    }
                }
                throw new AuthenticationFailedException("failed in step one: visit authenticate aspx", webResponse.StatusCode);
            }
        }

        private string GetSHA1(string text)
        {
            byte[] dataHashed = new System.Security.Cryptography.SHA1CryptoServiceProvider().ComputeHash(new ASCIIEncoding().GetBytes(text));
            return BitConverter.ToString(dataHashed).Replace("-", "");
        }

        private LiveIdLoginPageInfo VisitWinLiveSecurePostSrf(LoginPageInfo loginInfo, string siteUrl)
        {
            string postContent = string.Format("login={0}&type=16&hpwd={1}&LoginOptions=3", HttpUtility.UrlEncode(loginInfo.UserName), GetSHA1(loginInfo.Password).ToLower());
            Dictionary<string, string> header = new Dictionary<string, string>();
            header["Cookie"] = string.Format("MSPRequ={0}", loginInfo.MsRequ);
            try
            {
                string content = SendHttpWebRequest("https://login.live.com/ppsecure/post.srf?wa=wsignin1.0&wtrealm=urn%3Afederation%3AMicrosoftOnline",
                                                    WebRequestMethods.Http.Post,
                                                    "application/x-www-form-urlencoded",
                                                    postContent,
                                                    header);
                string[] inputValues = GetInputValues(content, new string[]{"<input type=\"hidden\" name=\"NAP\" id=\"NAP\" value=\"",
                                                                            "<input type=\"hidden\" name=\"wresult\" id=\"wresult\" value=\"",
                                                                            "<input type=\"hidden\" name=\"wa\" id=\"wa\" value=\"",
                                                                            "<input type=\"hidden\" name=\"ANON\" id=\"ANON\" value=\""});
                LiveIdLoginPageInfo pageInfo = new LiveIdLoginPageInfo();
                pageInfo.Wctx = "wa=wsignin1%2E0&wreply=" + HttpUtility.UrlEncode(mUrlHeader + mDomainUrl + "/_forms/default.aspx");
                pageInfo.NAP = inputValues[0];
                pageInfo.Wresult = inputValues[1];
                pageInfo.Wa = inputValues[2];
                pageInfo.ANON = inputValues[3];
                return pageInfo;
            }
            catch (AuthenticationFailedException e)
            {
                throw new AuthenticationFailedException("failed in step two: post login.srf, status : {0}", e.FailedStatusCode);
            }
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

        private HttpWebRequest GetWebRequest(string url, string method, string contentType, string content, bool includeCookieContainer = false, Dictionary<string, string> headers = null, Cookie[] cookies = null, bool allowAutoRedirect = true)
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
        }

        private string SendHttpWebRequest(string url, string method, string contentType, string content, Dictionary<string, string> headers = null)
        {
            HttpWebRequest webRequest = GetWebRequest(url, method, contentType, content, false, headers);
            try
            {
                using (HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse)
                {
                    if (webResponse.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                    throw new AuthenticationFailedException("", webResponse.StatusCode);
                }
            }
            catch (WebException e)
            {
                string errorDetail = string.Empty;
                using (HttpWebResponse errorResponse = e.Response as HttpWebResponse)
                {
                    using (StreamReader reader = new StreamReader(errorResponse.GetResponseStream()))
                    {
                        XmlDocument doc = new XmlDocument();
                        string responseText = reader.ReadToEnd();
                        if (string.IsNullOrEmpty(responseText))
                        {
                            throw;
                        }
                        doc.LoadXml(responseText);
                        XmlNamespaceManager nsManager = GetExceptionSoapNameSpace(doc);
                        errorDetail = GetXmlNodeInnerXmlUsingSoapNameSpace(ref doc, "/s:Envelope/s:Body/s:Fault/s:Reason/s:Text", "", ref nsManager);
                    }
                    throw new AuthenticationFailedException(errorDetail, errorResponse.StatusCode);
                }
            }
        }

        private CookieContainer PostHttpRequest(string url, string contentType, string content, Cookie[] cookies = null)
        {
            HttpWebRequest webRequest = GetWebRequest(url, WebRequestMethods.Http.Post, contentType, content, true, null, cookies);
            using (HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse)
            {
                if (webResponse.StatusCode == HttpStatusCode.OK)
                {
                    return webRequest.CookieContainer;
                }
                throw new AuthenticationFailedException("", webResponse.StatusCode);
            }
        }

        private string GetRealUrlForHttpSite(string suffix)
        {
            try
            {
                HttpWebRequest webRequest = HttpWebRequest.Create(mSiteUrl + suffix) as HttpWebRequest;
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
            catch (WebException) { }    //2010 http site will throw here
            return mSiteUrl;
        }

        private XmlNamespaceManager GetExceptionSoapNameSpace(XmlDocument doc)
        {
            XmlNamespaceManager soapNP = new XmlNamespaceManager(doc.NameTable);
            soapNP.AddNamespace("a", "http://www.w3.org/2005/08/addressing");
            soapNP.AddNamespace("s", "http://www.w3.org/2003/05/soap-envelope");
            return soapNP;
        }
        
        private XmlNamespaceManager GetSoapNameSpace(XmlDocument doc)
        {
            XmlNamespaceManager soapNP = new XmlNamespaceManager(doc.NameTable);
            soapNP.AddNamespace("psf", "http://schemas.microsoft.com/Passport/SoapServices/SOAPFault");
            soapNP.AddNamespace("saml", "urn:oasis:names:tc:SAML:1.0:assertion");
            soapNP.AddNamespace("t", "http://schemas.xmlsoap.org/ws/2005/02/trust");
            soapNP.AddNamespace("s", "http://www.w3.org/2003/05/soap-envelope");
            soapNP.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            soapNP.AddNamespace("wst", "http://schemas.xmlsoap.org/ws/2005/02/trust");
            return soapNP;
        }

        private string GetXmlNodeInnerXmlUsingSoapNameSpace(ref XmlDocument doc, string nodePath, string content, ref XmlNamespaceManager nsManager, bool needOuterXml = false)
        {
            if (doc == null)
            {
                doc = new XmlDocument();
                doc.LoadXml(content);
                nsManager = GetSoapNameSpace(doc);
            }
            XmlNode node = doc.SelectSingleNode(nodePath, nsManager);
            return node != null ? (needOuterXml ? node.OuterXml : node.InnerXml) : string.Empty;
        }

        private string[] GetInputValues(string content, string[] inputTags)
        {
            if (inputTags.Length > 0)
            {
                string[] values = new string[inputTags.Length];
                for (int i = 0; i < inputTags.Length; i++)
                {
                    int inputStartIndex = content.IndexOf(inputTags[i], StringComparison.OrdinalIgnoreCase) + inputTags[i].Length;
                    if (inputStartIndex < inputTags[i].Length)
                    {
                        values[i] = string.Empty;
                    }
                    values[i] = content.Substring(inputStartIndex, content.IndexOf('"', inputStartIndex) - inputStartIndex);
                }
                return values;
            }
            else
            {
                return new string[] { };
            }
        }

        private string GetXmlNodeInnerXml(string nodePath, string content = null, Stream stream = null, bool needOuterXml = false)
        {
            XmlDocument doc = new XmlDocument();
            if (content != null)
            {
                doc.LoadXml(content);
            }
            else
            {
                doc.Load(stream);
            }
            XmlNode node = doc.SelectSingleNode(nodePath);
            return node != null ? (needOuterXml ? node.OuterXml : node.InnerXml) : string.Empty;
        }
    }

    class LiveIdLoginPageInfo
    {
        public string Wctx { get; set; }
        public string NAP { get; set; }
        public string Wresult { get; set; }
        public string Wa { get; set; }
        public string ANON { get; set; }
    }

    class LoginPageInfo
    {
        public string LoginUrl { get; set; }
        public string PpfxValue { get; set; }
        public string MsRequ { get; set; }
        public string MsOk { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string RpsContextCookie { get; set; }
    }

    class VisitDefaultPageInfo
    {
        public string PostUrl { get; set; }
        public string PostData { get; set; }
        public string RpsContextCookie { get; set; }  //for windows live id login
    }

    [Serializable]
    public class AuthenticationFailedException : Exception
    {
        public AuthenticationFailedException(string message, HttpStatusCode statusCode)
            : base(message)
        {
            this.FailedStatusCode = statusCode;
        }

        public HttpStatusCode FailedStatusCode { get; set; }
    }

    [Serializable]
    public class PasswordExpiredException : AveWrapperI18NException
    {
        public PasswordExpiredException(string message) : base(message) { }

        public PasswordExpiredException(string key, string defaultValue, params object[] args)
            : base(key, defaultValue, args)
        {
        }
    }

    [Serializable]
    public class NonOffice365AccountException : AveWrapperI18NException
    {
        public NonOffice365AccountException(string message) : base(message) { }

        public NonOffice365AccountException(string key, string defaultValue, params object[] args)
            : base(key, defaultValue, args)
        {
        }
    }

    [Serializable]
    public class IncorrectUserNameOrPasswordException : AveWrapperI18NException
    {
        public IncorrectUserNameOrPasswordException(string message) : base(message) { }

        public IncorrectUserNameOrPasswordException(string key, string defaultValue, params object[] args)
            : base(key, defaultValue, args)
        {
        }
    }

    [Serializable]
    public class Office365SiteExpiredException : AveWrapperI18NException
    {
        public Office365SiteExpiredException(string message) : base(message) { }

        public Office365SiteExpiredException(string key, string defaultValue, params object[] args)
            : base(key, defaultValue, args)
        {
        }
    }
}

