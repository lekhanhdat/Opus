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
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml;
//using System.Security.Cryptography.X509Certificates;
//using System.Net.Security;


namespace AvePoint.Wrapper.Common
{
    public class SPOnlineAuthentication
    {
        private const string msoStsUrl = "https://login.microsoftonline.com/extSTS.srf";
        private const string msoHrdUrl = "https://login.microsoftonline.com/GetUserRealm.srf";
        private const string msoRstUrl = "https://login.microsoftonline.com/RST2.srf";
        private const string userAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)";
        private const string spowssigninUri = "_forms/default.aspx?wa=wsignin1.0";
        private const string defaultSuffix = "_forms/default.aspx";
        private const int TimeOut = 3 * 60 * 1000;
        private const int ReadWriteTimeOut = 3 * 60 * 1000;
        private string mSiteUrl;
        private string mWebAppUrl;
        private bool mIsRedirectedToHttps = false;
        private bool mUseAPI = true;
        private string mDomainUrl;
        private string mWebRelativeUrl;
        private string mUrlHeader;
        private const string defaultO365Suffix = "onmicrosoft.com";
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(SPOnlineAuthentication));

        public bool IsRedirectedToHttps { get { return mIsRedirectedToHttps; } }
        public string SiteUrl { get { return mSiteUrl; } }

        public SPOnlineAuthentication(string siteUrl)
        {
            mSiteUrl = siteUrl;
            mUrlHeader = mSiteUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ? "https://" : "http://";
            Uri siteUri = new Uri(siteUrl);
            mDomainUrl = siteUri.Host;
            mWebRelativeUrl = siteUri.AbsolutePath;
            mWebAppUrl = AveUrlUtility.GetServerUrl(mSiteUrl);
            if (siteUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                mIsRedirectedToHttps = GetRealUrlForHttpSite("/_layouts/authenticate.aspx").StartsWith("https", StringComparison.OrdinalIgnoreCase);
                mSiteUrl = "https" + mSiteUrl.Substring("http".Length);
            }
        }

        public SPOnlineAuthentication(string siteUrl, bool useAPI)
            : this(siteUrl)
        {
            mUseAPI = useAPI;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Rps is the part of a http request. ")]
        private CookieContainer LiveIdLogin(string userName, string password)
        {
            LoginPageInfo loginInfo = VisitAuthenticateAspx();
            loginInfo.UserName = userName;
            loginInfo.Password = password;
            LiveIdLoginPageInfo liveIdLoginInfo = VisitWinLiveSecurePostSrf(loginInfo, mSiteUrl);
            VisitDefaultPageInfo defaulPageInfo = VisitLoginSrf(liveIdLoginInfo, loginInfo);
            return PostHttpRequest(defaulPageInfo.PostUrl,
                                   "application/x-www-form-urlencoded",
                                   defaulPageInfo.PostData,
                                   new Cookie[] { new Cookie("RpsContextCookie", defaulPageInfo.RpsContextCookie, "/", mDomainUrl) });
        }

        private static object obj = new object();

        #region Login with SharePoint CSOM API
        /// <summary>
        /// Reflect SharePoint CSOM API
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Invoke SharePoint API.")]
        private CookieContainer SPOnlineLoginWithAPI(string userName, string password)
        {
            lock (obj)
            {             
                try
                {
             
                    Assembly clientRuntime = System.Reflection.Assembly.LoadFrom(System.IO.Path.GetDirectoryName(typeof(WrapperConfiguration).Assembly.Location) + "\\Office365\\Microsoft.SharePoint.Client.Runtime.dll");                              
                    Type spoCredential = clientRuntime.GetType("Microsoft.SharePoint.Client.SharePointOnlineCredentials");
                    MethodInfo method = spoCredential.GetMethod("GetAuthenticationCookie", new Type[] { typeof(Uri) });
                    object spoInstance = Activator.CreateInstance(spoCredential, new object[] { userName, ConvertStringToSecureString(password) });
                    string securityToken = method.Invoke(spoInstance, new object[] { new Uri(mSiteUrl) }).ToString();

                    return AssembleSPOIDCRLFromStsToken(securityToken);
                }
                catch (Exception err)
                {
                    mLogger.Warn(string.Format("Failed to login with SharePoint Client API, Message: {0}", err.ToString()));
                    CheckUnauthenticatedExceptionDetail(userName, password);
                    throw;
                }

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

        private CookieContainer AssembleSPOIDCRLFromStsToken(string stsToken)
        {
            string cookieName = stsToken.Substring(0, stsToken.IndexOf('='));
            string cookieValue = stsToken.Substring(stsToken.IndexOf('=') + 1);
            CookieContainer cookies = new CookieContainer();
            cookies.Add(new Cookie(cookieName, cookieValue, "/", this.mDomainUrl));

            return cookies;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are xml value")]
        private void CheckUnauthenticatedExceptionDetail(string username, string password)
        {
            string relyingPartyTrustUrl = GetRelyingPartyUrl();
            relyingPartyTrustUrl = new Uri(string.IsNullOrEmpty(relyingPartyTrustUrl) ? mSiteUrl : relyingPartyTrustUrl).Host;
            string response = SendHttpWebRequest(msoStsUrl, WebRequestMethods.Http.Post, "application/soap+xml; charset=utf-8", ParameterizeSoapRequestTokenMsgWithUsernamePassword(relyingPartyTrustUrl, username, password, msoStsUrl));
            XmlDocument doc = null;
            XmlNamespaceManager nsManager = null;
            string faultCode = GetXmlNodeInnerXmlUsingSoapNameSpace(ref doc, "/s:Envelope/s:Body/s:Fault/s:Detail/psf:error/psf:internalerror/psf:code", response, ref nsManager);
            if (faultCode.Equals("0x80041084") || faultCode.Equals("0x80041082"))   //need to change password
            {
                throw new PasswordExpiredException(AveInternalResourceKey.Wrapper_Exception_Common_PasswordExpired);
            }
            else if (faultCode.Equals("0x80041012"))
            {
                throw new PasswordNotMatchException(AveInternalResourceKey.Wrapper_Exception_Common_PasswordNotMatch);
            }
            else if (!string.IsNullOrEmpty(faultCode))
            {
                throw new Exception(GetXmlNodeInnerXmlUsingSoapNameSpace(ref doc, "/s:Envelope/s:Body/s:Fault/s:Detail/psf:error/psf:internalerror/psf:text", response, ref nsManager));
            }
        }
        #endregion

        /// <summary>
        /// simulate SharePoint Request
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        //[Obsolete("Use SPOnlineLoginWithAPI instead.")]
        private CookieContainer SPOnlineLogin(string userName, string password)
        {
            string securityToken = string.Empty;
            try
            {
                string stsAuthUrl = GetSTSAuthUrl(userName);
                if (!string.IsNullOrEmpty(stsAuthUrl))
                {
                    Uri stsAuthUri = new Uri(stsAuthUrl);
                    if (stsAuthUri.Port == 443)
                    {
                        stsAuthUrl = string.Format("{0}://{1}:443{2}", stsAuthUri.Scheme, stsAuthUri.Host, stsAuthUri.AbsolutePath);
                    }
                    string samlBody = GetSAMLBodyFromSTS(stsAuthUrl, userName, password);
                    securityToken = GetSecurityTokenFromRST(samlBody);
                    return GetSPOAuthCookies(securityToken);
                }
            }
            catch (Exception err)
            {
                string msg = string.Format("Failed to use sts auth to get cookies, error msg : {0}", err.Message);
            }
            string adfsAuthUrl = GetCustomAdfsAuthUrl(userName);
            if (string.IsNullOrEmpty(adfsAuthUrl))
            {
                securityToken = GetSecurityTokenDirectlyFromOffice365STS(userName, password);
            }
            else
            {
                //custom ADFS
                string assertion = GetSecurityTokenFromCustomAdfs(adfsAuthUrl, userName, password);
                securityToken = GetSecurityTokenFromOffice365STS(assertion);
            }
            return GetSPOAuthCookies(securityToken);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "onmicrosoft is the part of a user name ")]
        public CookieContainer Login(string userName, string password)
        {

            var loginSuccessful = false;
            CookieContainer cookieContainer = null;
            var loginWithAPI = mUseAPI;

            if (loginWithAPI)
            {
                loginSuccessful = LoginWithAPI(userName, password, ref cookieContainer);
            }
            if (!loginSuccessful)
            {
                loginSuccessful = SPOnlineLogin(userName, password, ref cookieContainer);
            }
            if (loginSuccessful)
            {
                Uri uri = new Uri(SiteUrl);
                string newDomain = uri.Host.Contains(".") ? uri.Host.Substring(uri.Host.IndexOf('.')) : uri.Host;
                foreach (Cookie cookie in cookieContainer.GetCookies(uri))
                {
                    if (!cookie.Domain.Equals(newDomain, StringComparison.OrdinalIgnoreCase))
                    {
                        Cookie tempCookie = new Cookie(cookie.Name, cookie.Value, cookie.Path, newDomain);
                        cookieContainer.Add(tempCookie);
                    }
                }
                return cookieContainer;
            }

            cookieContainer = LiveIdLogin(userName, password);
            if (cookieContainer != null && (cookieContainer.GetCookies(new Uri(mSiteUrl))["SPOIDCRL"] != null || cookieContainer.GetCookies(new Uri(mSiteUrl))["FedAuth"] != null))
            {
                return cookieContainer;
            }

            throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Common_LoginFailedForFedAuthCookie);
        }

        private bool SPOnlineLogin(string userName, string password, ref CookieContainer cookieContainer)
        {
            bool loginSuccessful = false;
            try
            {
                cookieContainer = SPOnlineLogin(userName, password);
                loginSuccessful = true;
            }
            catch (PasswordExpiredException)
            {
                throw;
            }
            catch (PasswordNotMatchException)
            {
                throw;
            }
            catch (Exception e)
            {
                mLogger.Info("Online login failed. error {0}", e);
            }
            return loginSuccessful;
        }

        private bool LoginWithAPI(string userName, string password, ref CookieContainer cookieContainer)
        {
            bool loginSuccessful = false;
            try
            {
                cookieContainer = SPOnlineLoginWithAPI(userName, password);
                loginSuccessful = true;
            }
            catch (PasswordExpiredException)
            {
                throw;
            }
            catch (PasswordNotMatchException)
            {
                throw;
            }
            catch (Exception e)
            {
                mLogger.Info("Login with api failed. error {0}", e);
            }
            return loginSuccessful;
        }

        #region local adfs

        public CookieContainer Login(string userName, string password, string domain)
        {
            string securityContent = GetSecurityContentFromLocalSTS(userName, password, domain);
            string url = this.mWebAppUrl + "/_trust/";
            return PostHttpRequest(url, "application/x-www-form-urlencoded", securityContent);
            //return GetCookieContainer(url, securityContent);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "wa,wtcx,wresult are the part of content;wtrealm is the part of url. ")]
        private string GetSecurityContentFromLocalSTS(string username, string password, string domain)
        {
            StringBuilder content = new StringBuilder();
            string returnUrl = "ReturnUrl=" + this.mWebRelativeUrl + "/_layouts/Authenticate.aspx?Source=" + this.mWebRelativeUrl;
            //string source = "&Source=" + this.mWebRelativeUrl;
            string url = this.mWebAppUrl + "/_login/default.aspx?" + returnUrl;// +source;
            //ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
            string trustUrl = GetLocation(url);
            string adfsUrl = GetLocation(trustUrl);
            if (!string.IsNullOrEmpty(domain))
            {
                username = domain + "\\" + username;
            }
            NameValueCollection collections = HttpUtility.ParseQueryString(adfsUrl);
            if (collections.Count >= 3)
            {
                string wa = collections[0];
                string urn = collections["wtrealm"];
                string wctx = collections["wctx"];
                string token = GetSecurityTokenFromLocalAdfs(adfsUrl, urn, username, password);
                content = content.Append("wa=" + wa + "&");
                content = content.Append("wresult=" + HttpUtility.UrlEncode(token) + "&");
                content = content.Append("wctx=" + HttpUtility.UrlEncode(wctx));
            }
            return content.ToString();
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "usernamemixed is the part of a url. ")]
        private string GetSecurityTokenFromLocalAdfs(string adfsAuthUrl, string urn, string username, string password)
        {
            // the corporate ADFS proxy endpoint that issues SAML seurity tokens given username/password credentials 
            string stsUsernameMixedUrl = String.Format("https://{0}/adfs/services/trust/2005/usernamemixed/", new Uri(adfsAuthUrl).Host);
            string samlRT = ParameterizeSoapRequestTokenMsgWithUsernamePassword(urn, // requesting a logon token to talk to the Microsoft Federation Gateway
                                                                                username,
                                                                                password,
                                                                                stsUsernameMixedUrl);
            string response = SendHttpWebRequest(stsUsernameMixedUrl, WebRequestMethods.Http.Post, "application/soap+xml; charset=utf-8", samlRT);
            // the logon token is in the SAML assertion element of the message body
            return GetXmlNodeInnerXmlUsingSoapNameSpace("/s:Envelope/s:Body/t:RequestSecurityTokenResponse", response, true);
        }

        public string GetLocation(string url, bool allowAutoRedirect = false)
        {
            HttpWebRequest webRequest = HttpWebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = "GET";
            webRequest.AllowAutoRedirect = allowAutoRedirect;
            try
            {
                using (HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse)
                {
                    if (webResponse.StatusCode == HttpStatusCode.OK || webResponse.StatusCode == HttpStatusCode.Found)
                    {
                        string location = webResponse.Headers["Location"];
                        if (!location.StartsWith("https://", StringComparison.OrdinalIgnoreCase) && !location.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                        {
                            location = this.mWebAppUrl + location;
                        }
                        return location;
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
                            doc.LoadXml(responseText);
                            XmlNamespaceManager nsManager = GetExceptionSoapNameSpace(doc);
                            errorDetail = GetXmlNodeInnerXmlUsingSoapNameSpace(ref doc, "/s:Envelope/s:Body/s:Fault/s:Reason/s:Text", "", ref nsManager);
                        }
                    }
                    throw new AuthenticationFailedException(errorDetail, errorResponse.StatusCode);
                }
            }
        }

        //public bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        //{
        //    //直接确认，否则打不开   
        //    return true;
        //}

        #endregion

        private void InitSSLSetting()
        {
            if (mSiteUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The microsoftonline is the part of url.")]
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
                    throw new PasswordExpiredException(AveInternalResourceKey.Wrapper_Exception_Common_PasswordExpired);
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
                mSiteUrl = (mSiteUrl.StartsWith("http:", StringComparison.OrdinalIgnoreCase) && IsRedirectedToHttps) ? "https:" + mSiteUrl.Substring("http:".Length) : mSiteUrl;    //support 2013 office365 http site
                return pageInfo;
            }
            catch (AuthenticationFailedException e)
            {
                throw new AuthenticationFailedException("Failed in step two: post login.srf, status : {0}", e.FailedStatusCode);
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "MSPRequ is the value in array. ")]
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
                throw new AuthenticationFailedException("Failed in step one: visit authenticate aspx", webResponse.StatusCode);
            }
        }


        private string GetSHA1(string text)
        {
            byte[] dataHashed = new System.Security.Cryptography.SHA1CryptoServiceProvider().ComputeHash(new ASCIIEncoding().GetBytes(text));
            return BitConverter.ToString(dataHashed).Replace("-", "");
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are the part of a url. ")]
        private LiveIdLoginPageInfo VisitWinLiveSecurePostSrf(LoginPageInfo loginInfo, string siteUrl)
        {
            string postContent = string.Format("login={0}&type=16&hpwd={1}&LoginOptions=3", HttpUtility.UrlEncode(loginInfo.UserName), GetSHA1(loginInfo.Password).ToLower(CultureInfo.CurrentCulture));
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
                throw new AuthenticationFailedException("Failed in step two: post login.srf, status : {0}", e.FailedStatusCode);
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "microsoftonline is the part of a url ")]
        /// <summary>
        /// Make a post request with the user's login name to MSO HRD (Home Realm Discovery) service to find out the url of the federation service (corporate ADFS) responsible for authenticating the user      
        /// </summary>
        /// <param name="username">username</param>
        /// <returns>ADFS authentication url</returns>
        private string GetCustomAdfsAuthUrl(string username)
        {
            string content = String.Format("handler=1&login={0}", username);
            string response = SendHttpWebRequest(msoHrdUrl, WebRequestMethods.Http.Post, "application/x-www-form-urlencoded", content);

            return new JavaScriptSerializer().Deserialize<ReamlInfo>(response).AuthURL;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are the part of url. ")]
        private string GetSTSAuthUrl(string username)
        {
            string response = SendHttpWebRequest(string.Format("{0}?login={1}&xml=1", msoHrdUrl, username), WebRequestMethods.Http.Post, "application/soap+xml", null);

            return GetXmlNodeInnerXml("/RealmInfo/STSAuthURL", response);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "usernamemixed is the part of a url. ")]
        /// <summary>
        /// makes a seurity token request to the corporate ADFS proxy usernamemixed endpoint using
        /// the user's corporate credentials. The logon token is used to talk to MSO STS to get
        /// an O365 service token that can then be used to sign into SPO.
        /// </summary>
        /// <param name="adfsAuthUrl">ADFS authentication url</param>
        /// <param name="username">username</param>
        /// <param name="password">password</param>
        /// <returns></returns>
        private string GetSecurityTokenFromCustomAdfs(string adfsAuthUrl, string username, string password)
        {
            // the corporate ADFS proxy endpoint that issues SAML seurity tokens given username/password credentials 
            string stsUsernameMixedUrl = String.Format("https://{0}/adfs/services/trust/2005/usernamemixed/", new Uri(adfsAuthUrl).Host);
            string samlRT = ParameterizeSoapRequestTokenMsgWithUsernamePassword("urn:federation:MicrosoftOnline", // requesting a logon token to talk to the Microsoft Federation Gateway
                                                                                username,
                                                                                password,
                                                                                stsUsernameMixedUrl);
            string response = SendHttpWebRequest(stsUsernameMixedUrl, WebRequestMethods.Http.Post, "application/soap+xml; charset=utf-8", samlRT);
            // the logon token is in the SAML assertion element of the message body
            return GetXmlNodeInnerXmlUsingSoapNameSpace("/s:Envelope/s:Body/t:RequestSecurityTokenResponse/t:RequestedSecurityToken/saml:Assertion", response, true);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "microsoftonline is the part of a url ")]
        private string GetSecurityTokenFromOffice365STS(string samlAssertion)
        {
            string relyingPartyTrustUrl = GetRelyingPartyUrl();
            relyingPartyTrustUrl = new Uri(string.IsNullOrEmpty(relyingPartyTrustUrl) ? mSiteUrl : relyingPartyTrustUrl).Host;
            string saml11RT = ParameterizeSoapRequestTokenMsgWithAssertion(relyingPartyTrustUrl, samlAssertion, msoStsUrl);
            string response = SendHttpWebRequest(msoStsUrl, WebRequestMethods.Http.Post, "application/soap+xml; charset=utf-8", saml11RT);  // make the post request to MSO STS with the WS-Trust payload
            return GetXmlNodeInnerXmlUsingSoapNameSpace("/s:Envelope/s:Body/t:RequestSecurityTokenResponse/wst:RequestedSecurityToken/wsse:BinarySecurityToken", response);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "internalerroris the part of file path. ")]
        private string GetSecurityTokenDirectlyFromOffice365STS(string username, string password)
        {
            string relyingPartyTrustUrl = GetRelyingPartyUrl();
            relyingPartyTrustUrl = new Uri(string.IsNullOrEmpty(relyingPartyTrustUrl) ? mSiteUrl : relyingPartyTrustUrl).Host;
            string response = SendHttpWebRequest(msoStsUrl, WebRequestMethods.Http.Post, "application/soap+xml; charset=utf-8", ParameterizeSoapRequestTokenMsgWithUsernamePassword(relyingPartyTrustUrl, username, password, msoStsUrl));
            XmlDocument doc = null;
            XmlNamespaceManager nsManager = null;
            string faultCode = GetXmlNodeInnerXmlUsingSoapNameSpace(ref doc, "/s:Envelope/s:Body/s:Fault/s:Detail/psf:error/psf:internalerror/psf:code", response, ref nsManager);
            if (faultCode.Equals("0x80041084") || faultCode.Equals("0x80041082"))   //need to change password
            {
                throw new PasswordExpiredException(AveInternalResourceKey.Wrapper_Exception_Common_PasswordExpired);
            }
            else if (faultCode.Equals("0x80041012"))
            {
                throw new PasswordNotMatchException(AveInternalResourceKey.Wrapper_Exception_Common_PasswordNotMatch);
            }
            else if (!string.IsNullOrEmpty(faultCode))
            {
                throw new Exception(GetXmlNodeInnerXmlUsingSoapNameSpace(ref doc, "/s:Envelope/s:Body/s:Fault/s:Detail/psf:error/psf:internalerror/psf:text", response, ref nsManager));
            }
            return GetXmlNodeInnerXmlUsingSoapNameSpace(ref doc, "/s:Envelope/s:Body/t:RequestSecurityTokenResponse/wst:RequestedSecurityToken/wsse:BinarySecurityToken", response, ref nsManager);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are the part of url. ")]
        private string GetSecurityTokenFromRST(string samlBody)
        {
            string msg = FillRSTMsgWithSamlBody(samlBody);
            string response = SendHttpWebRequest(msoRstUrl, WebRequestMethods.Http.Post, "application/soap+xml", msg);

            return GetXmlNodeInnerXmlUsingSoapNameSpace("/s:Envelope/s:Body/wst:RequestSecurityTokenResponseCollection/wst:RequestSecurityTokenResponse/wst:RequestedSecurityToken/wsse:BinarySecurityToken", response, false);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are the part of url. ")]
        private string GetSAMLBodyFromSTS(string stsUrl, string username, string password)
        {
            string requestBody = ParameterizeSTSRequestMsg(stsUrl, username, password);
            string response = SendHttpWebRequest(stsUrl, WebRequestMethods.Http.Post, "application/soap+xml", requestBody);

            return GetXmlNodeInnerXmlUsingSoapNameSpace("/s:Envelope/s:Body/wst:RequestSecurityTokenResponse/wst:RequestedSecurityToken", response);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "wreply is a collection value. ")]
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
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return string.Empty;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are the part of url. ")]
        /// <summary>
        /// Signs in to SPO with the security token issued by MSO STS and gets the fed auth cookies
        /// the fed auth cookie needs to be attached to all SPO REST services requests
        /// </summary>
        /// <param name="stsToken">stsToken</param>
        /// <returns>cookieContainer</returns>
        private CookieContainer GetSPOAuthCookies(string stsToken)
        {
            if (mSiteUrl.StartsWith("http:", StringComparison.OrdinalIgnoreCase))
            {
                mIsRedirectedToHttps = GetRealUrlForHttpSite("/_layouts/authenticate.aspx").StartsWith("https", StringComparison.OrdinalIgnoreCase);
                mSiteUrl = IsRedirectedToHttps ? "https:" + mSiteUrl.Substring("http:".Length) : mSiteUrl;    //support 2013 office365 http site
            }
            Uri siteUri = new Uri(mSiteUrl);
            Uri wsSigninUrl = new Uri(String.Format("{0}://{1}/{2}", siteUri.Scheme, siteUri.Authority, spowssigninUri));
            return PostHttpRequest(wsSigninUrl.ToString(), "application/x-www-form-urlencoded", stsToken);
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
            else
            {
                webRequest.ContentLength = 0;
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
                        if (!string.IsNullOrEmpty(responseText))
                        {
                            doc.LoadXml(responseText);
                            XmlNamespaceManager nsManager = GetExceptionSoapNameSpace(doc);
                            errorDetail = GetXmlNodeInnerXmlUsingSoapNameSpace(ref doc, "/s:Envelope/s:Body/s:Fault/s:Reason/s:Text", "", ref nsManager);
                        }
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

        private string ParameterizeSTSRequestMsg(string stsUrl, string username, string password)
        {
            string msgBody = GetXmlNodeInnerXml("/Authentication/STSRequestBody", null, typeof(SPOnlineAuthentication).Assembly.GetManifestResourceStream("AvePoint.Wrapper.Common.AdfsProtocol.xml"));
            msgBody = msgBody.Replace("[username]", username);
            msgBody = msgBody.Replace("[password]", password);
            msgBody = msgBody.Replace("[mustUnderstand]", stsUrl);
            Uri stsUri = new Uri(stsUrl);
            msgBody = msgBody.Replace("[address]", stsUrl.Substring(stsUri.Scheme.Length + 3));

            return msgBody;
        }

        private string FillRSTMsgWithSamlBody(string samlBody)
        {
            string msgBody = GetXmlNodeInnerXml("/Authentication/RST2RequestMsg", null, typeof(SPOnlineAuthentication).Assembly.GetManifestResourceStream("AvePoint.Wrapper.Common.AdfsProtocol.xml"));

            return msgBody.Replace("[SAMLBody]", samlBody);
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

        private string ParameterizeSoapRequestTokenMsgWithAssertion(string url, string samlAssertion, string toUrl)
        {
            string samlRTString = GetXmlNodeInnerXml("/Authentication/SAML11RequestTokenSOAPMsgAssertion", null,
                                                     typeof(SPOnlineAuthentication).Assembly.GetManifestResourceStream("AvePoint.Wrapper.Common.AdfsProtocol.xml"));
            samlRTString = samlRTString.Replace("[assertion]", samlAssertion);
            samlRTString = samlRTString.Replace("[url]", url);
            samlRTString = samlRTString.Replace("[toUrl]", toUrl);

            return samlRTString;
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
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "The wrong words are the part of a url ")]
        private XmlNamespaceManager GetSoapNameSpace(XmlDocument doc)
        {
            XmlNamespaceManager soapNP = new XmlNamespaceManager(doc.NameTable);
            soapNP.AddNamespace("psf", "http://schemas.microsoft.com/Passport/SoapServices/SOAPFault");
            soapNP.AddNamespace("saml", "urn:oasis:names:tc:SAML:1.0:assertion");
            soapNP.AddNamespace("t", "http://schemas.xmlsoap.org/ws/2005/02/trust");
            soapNP.AddNamespace("s", "http://www.w3.org/2003/05/soap-envelope");
            soapNP.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            soapNP.AddNamespace("wst", "http://schemas.xmlsoap.org/ws/2005/02/trust");
            soapNP.AddNamespace("wsa", "http://www.w3.org/2005/08/addressing");
            soapNP.AddNamespace("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
            return soapNP;
        }

        private string GetXmlNodeInnerXml(string nodePath, string content)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(content);
            XmlNode node = doc.SelectSingleNode(nodePath);
            return node != null ? node.InnerXml : string.Empty;
        }

        private string GetXmlNodeInnerXmlUsingSoapNameSpace(string nodePath, string content, bool needOuterXml = false)
        {
            XmlDocument doc = null;
            XmlNamespaceManager nsManager = null;
            return GetXmlNodeInnerXmlUsingSoapNameSpace(ref doc, nodePath, content, ref nsManager, needOuterXml);
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
    }

    class LiveIdLoginPageInfo
    {
        public string Wctx { get; set; }
        public string NAP { get; set; }
        public string Wresult { get; set; }
        public string Wa { get; set; }
        public string ANON { get; set; }
    }

    class ReamlInfo
    {
        public int State { get; set; }
        public int UserState { get; set; }
        public string LoginName { get; set; }
        public string DomainName { get; set; }
        public string AuthURL { get; set; }
        public string SiteGroup { get; set; }
        public string FederationBrandName { get; set; }
        public string NameSpaceType { get; set; }
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

        public override string Message
        {
            get
            {
                return string.Format("FailedStatusCode: {0} \r\n {1} ", FailedStatusCode, base.Message);
            }
        }
    }

    [Serializable]
    public class PasswordExpiredException : AveWrapperBaseException
    {
        public PasswordExpiredException(AveInternalResourceKey key, params object[] args)
            : base(key, args)
        { }

        public PasswordExpiredException(string message) : base(message) { }
    }

    [Serializable]
    public class AppTokenTenantIdException : AveWrapperBaseException
    {
        public AppTokenTenantIdException(string message) : base(message) { }

        public AppTokenTenantIdException(AveInternalResourceKey key, params object[] args)
            : base(key, args)
        {
        }
    }

    [Serializable]
    public class AppTokenClientIdException : AveWrapperBaseException
    {
        public AppTokenClientIdException(string message) : base(message) { }

        public AppTokenClientIdException(AveInternalResourceKey key, params object[] args)
            : base(key, args)
        {
        }
    }

    [Serializable]
    public class AppTokenCertificateException : AveWrapperBaseException
    {
        public AppTokenCertificateException(string message) : base(message) { }

        public AppTokenCertificateException(AveInternalResourceKey key, params object[] args)
            : base(key, args)
        {
        }
    }

    [Serializable]
    public class AppTokenUnknownException : AveWrapperBaseException
    {
        public AppTokenUnknownException(string message) : base(message) { }

        public AppTokenUnknownException(AveInternalResourceKey key, params object[] args)
            : base(key, args)
        {
        }
    }

    [Serializable]
    public class PasswordNotMatchException : AveWrapperBaseException
    {
        public PasswordNotMatchException(AveInternalResourceKey key, params object[] args)
            : base(key, args)
        {
        }
        public PasswordNotMatchException(string message) : base(message) { }
    }
}

