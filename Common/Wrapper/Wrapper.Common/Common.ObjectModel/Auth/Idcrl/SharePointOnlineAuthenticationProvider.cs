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
    using Microsoft.SharePoint.Client;
    using Microsoft.Win32;
    using System;
    using System.Net;
    using System.Net.Security;
    using System.Runtime.InteropServices;
    using System.Security;

    internal class SharePointOnlineAuthenticationProvider : ISharePointOnlineAuthenticationProvider
    {
        private static string s_idcrlEnvironment;

        private static string FromSecureString(SecureString value)
        {
            string str;
            IntPtr ptr = Marshal.SecureStringToBSTR(value);
            if (ptr == IntPtr.Zero)
            {
                return string.Empty;
            }
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

        public string GetAuthenticationCookie(Uri url, string username, SecureString password)
        {
            IdcrlEnvironment @int;
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username");
            }
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            IdcrlHeader idcrlHeader = this.GetIdcrlHeader(url);
            if (idcrlHeader == null)
            {
                return null;
            }
            if (string.Compare(IdcrlServiceEnvironment, "INT-MSO", StringComparison.OrdinalIgnoreCase) == 0)
            {
                @int = IdcrlEnvironment.Int;
            }
            else
            {
                @int = IdcrlEnvironment.Production;
            }
            IdcrlAuth auth = new IdcrlAuth(@int);
            string str = FromSecureString(password);
            string str2 = auth.GetServiceToken(username, str, idcrlHeader.ServiceTarget, idcrlHeader.ServicePolicy);
            if (string.IsNullOrEmpty(str2))
            {
                return null;
            }
            return this.GetCookie(url, idcrlHeader.Endpoint, str2);
        }

        private string GetCookie(Uri url, string endpoint, string ticket)
        {
            Uri baseUri = url;
            baseUri = new Uri(baseUri, endpoint);
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(baseUri);
            CookieContainer container = new CookieContainer();
            request.CookieContainer = container;
            request.Headers[HttpRequestHeader.Authorization] = "BPOSIDCRL " + ticket;
            WebResponse response = request.GetResponse();
            string cookieHeader = container.GetCookieHeader(baseUri);
            if (response != null)
            {
                response.Close();
            }
            return cookieHeader;
        }

        private IdcrlHeader GetIdcrlHeader(Uri url)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            request.Headers["X-FORMS_BASED_AUTH_ACCEPTED"] = "f";
            request.Headers["X-IDCRL_ACCEPTED"] = "t";
            request.AuthenticationLevel = AuthenticationLevel.None;
            WebResponse response = null;
            try
            {
                response = request.GetResponse();
            }
            catch (WebException exception)
            {
                response = exception.Response;
                if (exception.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    throw new Office365SiteExpiredException("Office365 site has expired");
                }
            }
            if (response == null)
            {
                return null;
            }
            string str = response.Headers["X-IDCRL_AUTH_PARAMS_V1"];
            if (string.IsNullOrEmpty(str))
            {
                str = response.Headers[HttpResponseHeader.WwwAuthenticate];
            }
            response.Close();
            return this.ParseIdcrlHeader(str);
        }

        private IdcrlHeader ParseIdcrlHeader(string headerValue)
        {
            if (!string.IsNullOrWhiteSpace(headerValue))
            {
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
                                if (str3 == "ENDPOINT")
                                {
                                    goto Label_00DB;
                                }
                                if (str3 == "ROOTDOMAIN")
                                {
                                    goto Label_00E6;
                                }
                                if (str3 == "POLICY")
                                {
                                    goto Label_00F1;
                                }
                            }
                            else
                            {
                                header.IdcrlType = strArray[1];
                            }
                        }
                    }
                    goto Label_00FA;
                Label_00DB:
                    header.Endpoint = strArray[1];
                    goto Label_00FA;
                Label_00E6:
                    header.ServiceTarget = strArray[1];
                    goto Label_00FA;
                Label_00F1:
                    header.ServicePolicy = strArray[1];
                Label_00FA:;
                }
                if ((!(header.IdcrlType != "BPOSIDCRL") && !string.IsNullOrEmpty(header.ServicePolicy)) && (!string.IsNullOrEmpty(header.ServiceTarget) && !string.IsNullOrEmpty(header.Endpoint)))
                {
                    return header;
                }
            }
            return null;
        }

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
                        string strA = (string) key.GetValue("ServiceEnvironment", null);
                        if (string.Compare(strA, "INT-MSO", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            str = "INT-MSO";
                        }
                        key.Close();
                    }
                    s_idcrlEnvironment = str;
                }
                return str;
            }
        }

        private class IdcrlHeader
        {
            public string Endpoint;
            public string IdcrlType;
            public string ServicePolicy;
            public string ServiceTarget;
        }
    }
}

