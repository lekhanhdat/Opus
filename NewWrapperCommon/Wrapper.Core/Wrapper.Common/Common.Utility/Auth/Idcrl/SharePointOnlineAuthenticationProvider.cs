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

namespace AvePoint.Wrapper.Common
{
    using GCommon;
    using Microsoft.Win32;
    using System;
    using System.Net;
    using System.Net.Security;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;

    internal class SharePointOnlineAuthenticationProvider : ISharePointOnlineAuthenticationProvider
    {
        // Fields
        private static string s_idcrlEnvironment;
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        // Methods
        internal bool DoesSupportIdcrl(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException("uri");
            return (this.GetIdcrlHeader(uri, true, null) != null);
        }

        public string GetADToken(string serviceTarget, string policy, string username, string password)
        {
            IdcrlEnvironment @int;
            if (string.Compare(IdcrlServiceEnvironment, "INT-MSO", StringComparison.OrdinalIgnoreCase) == 0)
            {
                @int = IdcrlEnvironment.Int;
            }
            else
            {
                @int = IdcrlEnvironment.Production;
            }
            IdcrlAuth auth = new IdcrlAuth(@int, null);
            string str2 = auth.GetServiceToken(username, password, serviceTarget, policy);
            if (string.IsNullOrEmpty(str2))
            {
                return null;
            }
            return str2;
        }

        private static string FromSecureString(SecureString value)
        {
            string str;
            IntPtr ptr = Marshal.SecureStringToBSTR(value);
            if (ptr == IntPtr.Zero) return string.Empty;
            try
            {
                str = Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                Marshal.FreeBSTR(ptr);
            }
            return str;
        }

        public string GetAuthenticationCookie(Uri url, string username, SecureString password, bool alwaysThrowOnFailure, EventHandler<SharePointOnlineCredentialsWebRequestEventArgs> executingWebRequest)
        {
            IdcrlEnvironment @int;
            if (url == null) throw new ArgumentNullException("url");
            if (string.IsNullOrEmpty(username)) throw new ArgumentNullException("username");
            if (password == null) throw new ArgumentNullException("password");
            IdcrlHeader header = this.GetIdcrlHeader(url, alwaysThrowOnFailure, executingWebRequest);
            if (header == null)
            {
                log.Debug("Cannot get IDCRL header for {0}",  url );
                if (alwaysThrowOnFailure) throw new AveClientRequestException(string.Format("Cannot contact site at the specified URL {0}", url));
                return null;
            }
            if (string.Compare(IdcrlServiceEnvironment, "INT-MSO", StringComparison.OrdinalIgnoreCase) == 0)
                @int = IdcrlEnvironment.Int;
            else if (string.Equals(IdcrlServiceEnvironment, "PPE-MSO", StringComparison.OrdinalIgnoreCase))
                @int = IdcrlEnvironment.Ppe;
            else
                @int = IdcrlEnvironment.Production;
            IdcrlAuth auth = new IdcrlAuth(@int, executingWebRequest);
            string str = FromSecureString(password);
            string str2 = auth.GetServiceToken(username, str, header.ServiceTarget, header.ServicePolicy);
            if (!string.IsNullOrEmpty(str2)) return this.GetCookie(url, header.Endpoint, str2, alwaysThrowOnFailure, executingWebRequest);
            log.Debug("Cannot get IDCRL ticket for username {0}", username);
            if (alwaysThrowOnFailure) throw new IdcrlException("Unable to get ticket due to unknown error", -2147186615);
            return null;
        }

        private string GetCookie(Uri url, string endpoint, string ticket, bool throwIfFail, EventHandler<SharePointOnlineCredentialsWebRequestEventArgs> executingWebRequest)
        {
            Uri baseUri = url;
            baseUri = new Uri(baseUri, endpoint);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(baseUri);
            CookieContainer container = new CookieContainer();
            webRequest.CookieContainer = container;
            webRequest.Headers[HttpRequestHeader.Authorization] = "BPOSIDCRL " + ticket;
            webRequest.Headers["X-IDCRL_ACCEPTED"] = "t";
            if (executingWebRequest != null) executingWebRequest(this, new SharePointOnlineCredentialsWebRequestEventArgs(webRequest));
            WebResponse response = webRequest.GetResponse();
            string cookieHeader = container.GetCookieHeader(baseUri);
            if (string.IsNullOrEmpty(cookieHeader))
            {
                //It's webrequest 4.0 property.
                UriBuilder builder = new UriBuilder(baseUri)
                {
                    Host = baseUri.Host
                };
                log.Debug("Try get cookie using {0}", builder.ToString() );
                cookieHeader = container.GetCookieHeader(builder.Uri);
                log.Debug("Get cookie using {0} and cookie value is {0}",  builder.ToString(), cookieHeader );
            }
            if (response != null) response.Close();
            if (string.IsNullOrEmpty(cookieHeader))
            {
                log.Debug( "Cannot get cookie for {0}", url );
                if (throwIfFail) throw new AveClientRequestException(string.Format("Cannot get cookie for URL '{0}'.", url ));
            }
            return cookieHeader;
        }

        private IdcrlHeader GetIdcrlHeader(Uri url, bool alwaysThrowOnFailure, EventHandler<SharePointOnlineCredentialsWebRequestEventArgs> executingWebRequest)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Headers["X-IDCRL_ACCEPTED"] = "t";
            webRequest.AuthenticationLevel = AuthenticationLevel.None;
            if (executingWebRequest != null) executingWebRequest(this, new SharePointOnlineCredentialsWebRequestEventArgs(webRequest));
            HttpWebResponse response = null;
            try
            {
                response = webRequest.GetResponse() as HttpWebResponse;
            }
            catch (WebException exception)
            {
                log.Debug("Exception in request. Url={0}, WebException={1}", new object[] { url, exception.Message });
                response = exception.Response as HttpWebResponse;
                if (alwaysThrowOnFailure && (response == null || response.StatusCode != HttpStatusCode.Forbidden && response.StatusCode != HttpStatusCode.Unauthorized)) throw;
            }
            if (response != null)
            {
                string webResponseHeader = IdcrlUtility.GetWebResponseHeader(response);
                HttpStatusCode statusCode = response.StatusCode;
                log.Debug("Response.StatusCode={0}, Headers={1}", statusCode, webResponseHeader);
                string str2 = response.Headers["X-IDCRL_AUTH_PARAMS_V1"];
                if (string.IsNullOrEmpty(str2)) str2 = response.Headers[HttpResponseHeader.WwwAuthenticate];
                response.Close();
                if (!string.IsNullOrEmpty(str2))
                {
                    log.Debug("IdcrlHeader={0}", str2);
                }
                else
                {
                    str2 = "IDCRL Type=\"BPOSIDCRL\", EndPoint=\"/_vti_bin/idcrl.svc/\", RootDomain=\"sharepoint.com\", Policy=\"MBI\"";
                    log.Debug("Using Default IdcrlHeader={0}", str2);
                }
                return this.ParseIdcrlHeader(str2, url, statusCode, webResponseHeader, alwaysThrowOnFailure);
            }
            log.Warn("Cannot get response for request to {0}", url);
            if (alwaysThrowOnFailure) throw new AveClientRequestException(string.Format("Cannot contact site at the specified URL {0}", url));
            return null;
        }

        private IdcrlHeader ParseIdcrlHeader(string headerValue, Uri url, HttpStatusCode statusCode, string allResponseHeaders, bool alwaysThrowOnFailure)
        {
            if (string.IsNullOrEmpty(headerValue))
            {
                log.Debug("IDCRL header value is empty");
                if (alwaysThrowOnFailure) throw new NotSupportedException(string.Format("Cannot contact web site '{0}' or the web site does not support SharePoint Online credentials. The response status code is '{1}'. The response headers are '{2}'", url.OriginalString, statusCode, allResponseHeaders));
                return null;
            }
            IdcrlHeader header = new IdcrlHeader();
            foreach (string str in headerValue.Split(new char[] { ',' }))
            {
                string[] strArray = str.Trim().Split(new char[] { '=' });
                if (strArray.Length == 2)
                {
                    strArray[0] = strArray[0].Trim().ToUpperInvariant();
                    strArray[1] = strArray[1].Trim(new char[] { ' ', '"' });
                    string str3 = strArray[0];
                    if (str3 != null)
                    {
                        if (!(str3 == "IDCRL TYPE"))
                        {
                            if (str3 == "ENDPOINT") goto Label_012A;
                            if (str3 == "ROOTDOMAIN") goto Label_0135;
                            if (str3 == "POLICY") goto Label_0140;
                        }
                        else
                            header.IdcrlType = strArray[1];
                    }
                }
                goto Label_0149;
            Label_012A:
                header.Endpoint = strArray[1];
                goto Label_0149;
            Label_0135:
                header.ServiceTarget = strArray[1];
                goto Label_0149;
            Label_0140:
                header.ServicePolicy = strArray[1];
            Label_0149:;
            }
            if (!(header.IdcrlType != "BPOSIDCRL") && !string.IsNullOrEmpty(header.ServicePolicy) && !string.IsNullOrEmpty(header.ServiceTarget) && !string.IsNullOrEmpty(header.Endpoint)) return header;
            log.Debug("Cannot extract required information from IDCRL header. Header={0}, IdcrlType={1}, ServicePolicy={2}, ServiceTarget={3}, Endpoint={4}", new object[] { headerValue, header.IdcrlType, header.ServicePolicy, header.ServiceTarget, header.Endpoint });
            if (alwaysThrowOnFailure) throw new AveClientRequestException(string.Format("The IDCRL response header from server '{0}' is not valid. The response header value is '{1}'. The response status code is '{2}'. All response headers are '{3}'",url.OriginalString, headerValue, statusCode, allResponseHeaders ));
            return null;
        }

        // Properties
        private static string IdcrlServiceEnvironment
        {
            get
            {
                string str = s_idcrlEnvironment;
                if (str == null)
                {
                    str = "production";
                    RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\MSOIdentityCRL");
                    if (key != null)
                    {
                        string strA = (string)key.GetValue("ServiceEnvironment", null);
                        if (string.Compare(strA, "INT-MSO", StringComparison.OrdinalIgnoreCase) == 0)
                            str = "INT-MSO";
                        else if (string.Equals(strA, "PPE-MSO", StringComparison.OrdinalIgnoreCase)) str = "PPE-MSO";
                        key.Close();
                    }
                    log.Debug("IdcrlServiceEnvironment={0}", str);
                    s_idcrlEnvironment = str;
                }
                return str;
            }
        }

        // Nested Types
        private class IdcrlHeader
        {
            // Fields
            public string Endpoint;
            public string IdcrlType;
            public string ServicePolicy;
            public string ServiceTarget;
        }
    }
}

