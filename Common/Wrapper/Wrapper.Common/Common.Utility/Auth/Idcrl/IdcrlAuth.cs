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
    using AvePoint.GCommon;
    using Microsoft.SharePoint.Client;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using Office365.Api;

    internal class IdcrlAuth
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(IdcrlAuth));
        private IdcrlEnvironment m_env;
        private string m_federationTokenIssuer;
        private string m_securityTokenServiceUrl;
        private string m_userRealmServiceUrl;
        private string m_userDomainName;
        private static FederationProviderInfoCache s_FederationProviderInfoCache;
        private static Dictionary<string, int> s_partnerSoapErrorMap;

        static IdcrlAuth()
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            dictionary.Add("InvalidRequest", -2147186474);
            dictionary.Add("FailedAuthentication", -2147186446);
            dictionary.Add("RequestFailed", -2147186473);
            dictionary.Add("InvalidSecurityToken", -2147186472);
            dictionary.Add("AuthenticationBadElements", -2147186471);
            dictionary.Add("BadRequest", -2147186470);
            dictionary.Add("ExpiredData", -2147186469);
            dictionary.Add("InvalidTimeRange", -2147186468);
            dictionary.Add("InvalidScope", -2147186467);
            dictionary.Add("RenewNeeded", -2147186466);
            dictionary.Add("UnableToRenew", -2147186465);
            s_partnerSoapErrorMap = dictionary;
            s_FederationProviderInfoCache = new FederationProviderInfoCache();
        }

        public IdcrlAuth(IdcrlEnvironment env)
        {
            this.m_env = env;            
            if (this.m_env == IdcrlEnvironment.Production)
            {
                this.m_userRealmServiceUrl = "https://login.microsoftonline.com/GetUserRealm.srf";
                this.m_securityTokenServiceUrl = "https://login.microsoftonline.com/rst2.srf";
                this.m_federationTokenIssuer = "urn:federation:MicrosoftOnline";
            }
            else
            {
                this.m_userRealmServiceUrl = "https://login.microsoftonline-int.com/GetUserRealm.srf";
                this.m_securityTokenServiceUrl = "https://login.microsoftonline-int.com/rst2.srf";
                this.m_federationTokenIssuer = "urn:federation:MicrosoftOnline-int";
            }
        }

        private string BuildWsSecurityUsingUsernamePassword(string username, string password)
        {
            DateTime utcNow = DateTime.UtcNow;
            return string.Format(CultureInfo.InvariantCulture, "\r\n            <wsse:UsernameToken wsu:Id=\"user\">\r\n                <wsse:Username>{0}</wsse:Username>\r\n                <wsse:Password>{1}</wsse:Password>\r\n            </wsse:UsernameToken>\r\n            <wsu:Timestamp Id=\"Timestamp\">\r\n                <wsu:Created>{2}</wsu:Created>\r\n                <wsu:Expires>{3}</wsu:Expires>\r\n            </wsu:Timestamp>\r\n", new object[] { IdcrlUtility.XmlValueEncode(username), IdcrlUtility.XmlValueEncode(password), utcNow.ToString("o", CultureInfo.InvariantCulture), utcNow.AddDays(1.0).ToString("o", CultureInfo.InvariantCulture) });
        }

        private static Exception CreateIdcrlException(int hr)
        {
            return CreateIdcrlException(hr, null);
        }

        private static Exception CreateIdcrlException(int hr, string defaultErrorMessage)
        {
            var errorMessage = defaultErrorMessage;
            string str;
            if (!IdcrlErrorCodes.TryGetErrorStringId(hr, out str))
            {
                if(string.IsNullOrEmpty(defaultErrorMessage))
                {
                    errorMessage = Resources.GetString("PPCRL_REQUEST_E_UNKNOWN");
                }
            }
            else
            {
                errorMessage = Resources.GetString(str);
            }


            return new IdcrlException(errorMessage, hr);
        }

        private XDocument DoGet(string url)
        {
            XDocument document2;
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            request.Method = "GET";            
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            if (response == null)
            {                
                throw new InvalidOperationException();
            }
            using (response)
            {
                using (TextReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string s = reader.ReadToEnd();                    
                    using (XmlReader reader2 = XmlReader.Create(new StringReader(s)))
                    {
                        document2 = XDocument.Load(reader2);
                    }
                }
            }
            return document2;
        }

        private XDocument DoPost(string url, string contentType, string body, Func<WebException, Exception> webExceptionHandler)
        {
            XDocument document2;
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = contentType;
            using (Stream stream = request.GetRequestStream())
            {
                if (body != null)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(body);
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
            try
            {
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                if (response == null)
                {
                    throw new InvalidOperationException();
                }
                using (response)
                {
                    using (TextReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string s = reader.ReadToEnd();
                        using (XmlReader reader2 = XmlReader.Create(new StringReader(s)))
                        {
                            document2 = XDocument.Load(reader2);
                        }
                    }
                }
            }
            catch (WebException exception)
            {
                if (webExceptionHandler == null)
                {
                    throw;
                }
                Exception exception2 = webExceptionHandler(exception);
                if (exception2 == null)
                {
                    throw;
                }
                throw exception2;
            }
            return document2;
        }

        private FederationProviderInfo GetFederationProviderInfo(string domainname)
        {
            FederationProviderInfo info;
            if (s_FederationProviderInfoCache.TryGetValue(domainname, out info))
            {
                return info;
            }
            info = this.RequestFederationProviderInfo(domainname);
            s_FederationProviderInfoCache.Put(domainname, info);
            return info;
        }

        private string GetPartnerTicketFromAdfs(string adfsUrl, string username, string password)
        {
            string body = string.Format(CultureInfo.InvariantCulture, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<s:Envelope xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\" xmlns:saml=\"urn:oasis:names:tc:SAML:1.0:assertion\" xmlns:wsp=\"http://schemas.xmlsoap.org/ws/2004/09/policy\" xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" xmlns:wsa=\"http://www.w3.org/2005/08/addressing\" xmlns:wssc=\"http://schemas.xmlsoap.org/ws/2005/02/sc\" xmlns:wst=\"http://schemas.xmlsoap.org/ws/2005/02/trust\">\r\n    <s:Header>\r\n        <wsa:Action s:mustUnderstand=\"1\">http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue</wsa:Action>\r\n        <wsa:To s:mustUnderstand=\"1\">{0}</wsa:To>\r\n        <wsa:MessageID>{1}</wsa:MessageID>\r\n        <ps:AuthInfo xmlns:ps=\"http://schemas.microsoft.com/Passport/SoapServices/PPCRL\" Id=\"PPAuthInfo\">\r\n            <ps:HostingApp>Managed IDCRL</ps:HostingApp>\r\n            <ps:BinaryVersion>6</ps:BinaryVersion>\r\n            <ps:UIVersion>1</ps:UIVersion>\r\n            <ps:Cookies></ps:Cookies>\r\n            <ps:RequestParams>AQAAAAIAAABsYwQAAAAxMDMz</ps:RequestParams>\r\n        </ps:AuthInfo>\r\n        <wsse:Security>\r\n            <wsse:UsernameToken wsu:Id=\"user\">\r\n                <wsse:Username>{2}</wsse:Username>\r\n                <wsse:Password>{3}</wsse:Password>\r\n            </wsse:UsernameToken>\r\n            <wsu:Timestamp Id=\"Timestamp\">\r\n                <wsu:Created>{4}</wsu:Created>\r\n                <wsu:Expires>{5}</wsu:Expires>\r\n            </wsu:Timestamp>\r\n        </wsse:Security>\r\n    </s:Header>\r\n    <s:Body>\r\n        <wst:RequestSecurityToken Id=\"RST0\">\r\n            <wst:RequestType>http://schemas.xmlsoap.org/ws/2005/02/trust/Issue</wst:RequestType>\r\n            <wsp:AppliesTo>\r\n                <wsa:EndpointReference>\r\n                    <wsa:Address>{6}</wsa:Address>\r\n                </wsa:EndpointReference>\r\n            </wsp:AppliesTo>\r\n            <wst:KeyType>http://schemas.xmlsoap.org/ws/2005/05/identity/NoProofKey</wst:KeyType>\r\n        </wst:RequestSecurityToken>\r\n    </s:Body>\r\n</s:Envelope>", new object[] { IdcrlUtility.XmlValueEncode(adfsUrl), Guid.NewGuid().ToString(), IdcrlUtility.XmlValueEncode(username), IdcrlUtility.XmlValueEncode(password), DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), DateTime.UtcNow.AddMinutes(10.0).ToString("o", CultureInfo.InvariantCulture), this.FederationTokenIssuer });
            XDocument xdoc = this.DoPost(adfsUrl, "application/soap+xml; charset=utf-8", body, new Func<WebException, Exception>(IdcrlAuth.HandleWebException));
            Exception soapException = GetSoapException(xdoc);
            if (soapException != null)
            {
                throw soapException;
            }
            XElement elementAtPath = IdcrlUtility.GetElementAtPath(xdoc.Root, new string[] { "{http://www.w3.org/2003/05/soap-envelope}Body", "{http://schemas.xmlsoap.org/ws/2005/02/trust}RequestSecurityTokenResponse", "{http://schemas.xmlsoap.org/ws/2005/02/trust}RequestedSecurityToken", "{urn:oasis:names:tc:SAML:1.0:assertion}Assertion" });
            if (elementAtPath == null)
            {
                throw CreateIdcrlException(-2147186451);
            }
            return elementAtPath.ToString(SaveOptions.OmitDuplicateNamespaces | SaveOptions.DisableFormatting);
        }

        private string GetServiceToken(string securityXml, string serviceTarget, string servicePolicy)
        {
            string serviceTokenUrl = this.ServiceTokenUrl;
            string str2 = string.Empty;
            if (!string.IsNullOrEmpty(servicePolicy))
            {
                str2 = string.Format(CultureInfo.InvariantCulture, "<wsp:PolicyReference URI=\"{0}\"></wsp:PolicyReference>", new object[] { servicePolicy });
            }
            string body = string.Format(CultureInfo.InvariantCulture, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<S:Envelope xmlns:S=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\" xmlns:wsp=\"http://schemas.xmlsoap.org/ws/2004/09/policy\" xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" xmlns:wsa=\"http://www.w3.org/2005/08/addressing\" xmlns:wst=\"http://schemas.xmlsoap.org/ws/2005/02/trust\">\r\n  <S:Header>\r\n    <wsa:Action S:mustUnderstand=\"1\">http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue</wsa:Action>\r\n    <wsa:To S:mustUnderstand=\"1\">{0}</wsa:To>\r\n    <ps:AuthInfo xmlns:ps=\"http://schemas.microsoft.com/LiveID/SoapServices/v1\" Id=\"PPAuthInfo\">\r\n      <ps:BinaryVersion>5</ps:BinaryVersion>\r\n      <ps:HostingApp>Managed IDCRL</ps:HostingApp>\r\n    </ps:AuthInfo>\r\n    <wsse:Security>{1}</wsse:Security>\r\n  </S:Header>\r\n  <S:Body>\r\n    <wst:RequestSecurityToken xmlns:wst=\"http://schemas.xmlsoap.org/ws/2005/02/trust\" Id=\"RST0\">\r\n      <wst:RequestType>http://schemas.xmlsoap.org/ws/2005/02/trust/Issue</wst:RequestType>\r\n      <wsp:AppliesTo>\r\n        <wsa:EndpointReference>\r\n          <wsa:Address>{2}</wsa:Address>\r\n        </wsa:EndpointReference>\r\n      </wsp:AppliesTo>\r\n      {3}\r\n    </wst:RequestSecurityToken>\r\n  </S:Body>\r\n</S:Envelope>\r\n", new object[] { IdcrlUtility.XmlValueEncode(serviceTokenUrl), securityXml, IdcrlUtility.XmlValueEncode(serviceTarget), str2 });
            XDocument xdoc = this.DoPost(serviceTokenUrl, "application/soap+xml; charset=utf-8", body, new Func<WebException, Exception>(IdcrlAuth.HandleWebException));
            Exception soapException = GetSoapException(xdoc);
            if (soapException != null)
            {
                throw soapException;
            }
            XElement elementAtPath = IdcrlUtility.GetElementAtPath(xdoc.Root, new string[] { "{http://www.w3.org/2003/05/soap-envelope}Body", "{http://schemas.xmlsoap.org/ws/2005/02/trust}RequestSecurityTokenResponse", "{http://schemas.xmlsoap.org/ws/2005/02/trust}RequestedSecurityToken", "{http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd}BinarySecurityToken" });
            if (elementAtPath == null)
            {
                throw CreateIdcrlException(-2147186656);
            }
            return elementAtPath.Value;
        }

        public string GetServiceToken(string username, string password, string serviceTarget, string servicePolicy)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username");
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException("password");
            }
            if (string.IsNullOrEmpty(serviceTarget))
            {
                throw new ArgumentNullException("serviceTarget");
            }
            this.InitFederationProviderInfoForUser(username);
            UserRealmInfo userRealm = this.GetUserRealm(username);
            if (userRealm.IsFederated)
            {
                Office365Api.InitializeLogger(new Office365ApiLogger());

                var authApi = AuthenticationFramework.GetAuthProviderApi(m_userDomainName);

                string ticket = null;

                if (authApi != null)
                {
                    ticket = authApi.CreateFederationAuthApi().GetTicket(userRealm.STSAuthUrl, username, password, FederationTokenIssuer);
                }
                else
                {
                    ticket = this.GetPartnerTicketFromAdfs(userRealm.STSAuthUrl, username, password);
                }

                return this.GetServiceToken(ticket, serviceTarget, servicePolicy);
            }
            string securityXml = this.BuildWsSecurityUsingUsernamePassword(username, password);
            return this.GetServiceToken(securityXml, serviceTarget, servicePolicy);
        }

        private static Exception GetSoapException(XDocument xdoc)
        {
            int num2;
            if (IdcrlUtility.GetElementAtPath(xdoc.Root, new string[] { "{http://www.w3.org/2003/05/soap-envelope}Body", "{http://www.w3.org/2003/05/soap-envelope}Fault" }) == null)
            {
                return null;
            }
            XElement elementAtPath = IdcrlUtility.GetElementAtPath(xdoc.Root, new string[] { "{http://www.w3.org/2003/05/soap-envelope}Body", "{http://www.w3.org/2003/05/soap-envelope}Fault", "{http://www.w3.org/2003/05/soap-envelope}Code", "{http://www.w3.org/2003/05/soap-envelope}Subcode", "{http://www.w3.org/2003/05/soap-envelope}Value" });
            XElement element3 = IdcrlUtility.GetElementAtPath(xdoc.Root, new string[] { "{http://www.w3.org/2003/05/soap-envelope}Body", "{http://www.w3.org/2003/05/soap-envelope}Fault", "{http://www.w3.org/2003/05/soap-envelope}Detail", "{http://schemas.microsoft.com/Passport/SoapServices/SOAPFault}error", "{http://schemas.microsoft.com/Passport/SoapServices/SOAPFault}value" });
            XElement element4 = IdcrlUtility.GetElementAtPath(xdoc.Root, new string[] { "{http://www.w3.org/2003/05/soap-envelope}Body", "{http://www.w3.org/2003/05/soap-envelope}Fault", "{http://www.w3.org/2003/05/soap-envelope}Detail", "{http://schemas.microsoft.com/Passport/SoapServices/SOAPFault}error", "{http://schemas.microsoft.com/Passport/SoapServices/SOAPFault}internalerror", "{http://schemas.microsoft.com/Passport/SoapServices/SOAPFault}text" });
            string code = null;
            if (elementAtPath != null)
            {
                code = elementAtPath.Value;
                int index = code.IndexOf(':');
                if (index >= 0)
                {
                    code = code.Substring(index + 1);
                }
            }
            string str2 = null;
            if (element3 != null)
            {
                str2 = element3.Value;
            }
            string str3 = null;
            if (element4 != null)
            {
                str3 = element4.Value;
            }
            if (string.IsNullOrEmpty(str2))
            {
                num2 = MapPartnerSoapFault(code);
            }
            else
            {
                long num3;
                if ((str2.StartsWith("0x", StringComparison.OrdinalIgnoreCase) && long.TryParse(str2.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num3)) || long.TryParse(str2, NumberStyles.Integer, CultureInfo.InvariantCulture, out num3))
                {
                    num2 = (int) num3;
                    if (string.Compare(code, "FailedAuthentication", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        switch(num2)
                        {
                            case -2147186639:
                            case -2147207980: //Login requires strong authentication.
                                break;
                            default:
                                num2 = -2147186655;
                                break;
                        }

                        //num2 = (num2 == -2147186639) ? num2 : -2147186655;
                    }
                }
                else
                {
                    num2 = -2147186656;
                }
            }
            return CreateIdcrlException(num2, str3);
        }

        private UserRealmInfo GetUserRealm(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentNullException("login");
            }
            string userRealmServiceUrl = this.UserRealmServiceUrl;
            string body = string.Format(CultureInfo.InvariantCulture, "login={0}&xml=1", new object[] { Uri.EscapeDataString(login) });
            XDocument document = this.DoPost(userRealmServiceUrl, "application/x-www-form-urlencoded", body, null);
            XAttribute attribute = document.Root.Attribute("Success");
            if ((attribute == null) || (string.Compare(attribute.Value, "true", StringComparison.OrdinalIgnoreCase) != 0))
            {
                throw CreateIdcrlException(-2147186539);
            }
            XElement element = document.Root.Element("NameSpaceType");
            if (element == null)
            {
                throw CreateIdcrlException(-2147186539);
            }
            if ((string.Compare(element.Value, "Federated", StringComparison.OrdinalIgnoreCase) != 0) && (string.Compare(element.Value, "Managed", StringComparison.OrdinalIgnoreCase) != 0))
            {
                throw CreateIdcrlException(-2147186539);
            }
            UserRealmInfo info = new UserRealmInfo();
            info.IsFederated = 0 == string.Compare(element.Value, "Federated", StringComparison.OrdinalIgnoreCase);
            element = document.Root.Element("STSAuthURL");
            if (element != null)
            {
                info.STSAuthUrl = element.Value;
            }
            if (info.IsFederated && string.IsNullOrEmpty(info.STSAuthUrl))
            {
                throw CreateIdcrlException(-2147186539);
            }
            return info;
        }

        private static Exception HandleWebException(WebException webException)
        {
            HttpWebResponse response = webException.Response as HttpWebResponse;
            if (((response != null) && (response.ContentType != null)) && (response.ContentType.IndexOf("application/soap+xml", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                try
                {
                    using (TextReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string s = reader.ReadToEnd();
                        logger.Warn("The webexception detail is {0}",s);
                        using (XmlReader reader2 = XmlReader.Create(new StringReader(s)))
                        {
                            return GetSoapException(XDocument.Load(reader2));
                        }
                    }
                }
                catch (XmlException exception2)
                {
                    logger.Warn(exception2.ToString());
                }
                catch (IOException exception3)
                {
                    logger.Warn(exception3.ToString());
                }
            }
            return null;
        }

        private void InitFederationProviderInfoForUser(string username)
        {
            int index = username.IndexOf('@');
            if ((index < 0) || (index == (username.Length - 1)))
            {
                throw ClientUtility.CreateArgumentException("username");
            }
            m_userDomainName = username.Substring(index + 1);
            FederationProviderInfo federationProviderInfo = this.GetFederationProviderInfo(m_userDomainName);
            if (federationProviderInfo != null)
            {
                this.m_userRealmServiceUrl = federationProviderInfo.UserRealmServiceUrl;
                this.m_securityTokenServiceUrl = federationProviderInfo.SecurityTokenServiceUrl;
                this.m_federationTokenIssuer = federationProviderInfo.FederationTokenIssuer;
            }
        }

        private static int MapPartnerSoapFault(string code)
        {
            int num;
            if (s_partnerSoapErrorMap.TryGetValue(code, out num))
            {
                return num;
            }
            return -2147186451;
        }

        private static FederationProviderInfo ParseFederationProviderInfo(XDocument xdoc, string fpDomainName)
        {
            foreach (XElement element in xdoc.Root.Elements("FP"))
            {
                if ((element.Attribute("DomainName") == null) || !string.Equals(element.Attribute("DomainName").Value, fpDomainName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                XElement elementAtPath = IdcrlUtility.GetElementAtPath(element, new string[] { "URL", "GETUSERREALM" });
                XElement element3 = IdcrlUtility.GetElementAtPath(element, new string[] { "URL", "RST2" });
                XElement element4 = IdcrlUtility.GetElementAtPath(element, new string[] { "URL", "ENTITYID" });
                if (((elementAtPath == null) || (element3 == null)) || (element4 == null))
                {
                    throw CreateIdcrlException(-2147186646);
                }
                FederationProviderInfo info = new FederationProviderInfo();
                info.UserRealmServiceUrl = elementAtPath.Value;
                info.SecurityTokenServiceUrl = element3.Value;
                info.FederationTokenIssuer = element4.Value;
                return info;
            }
            throw CreateIdcrlException(-2147186646);
        }

        private static string ParseFPDomainName(XDocument xdoc)
        {
            XElement elementAtPath = IdcrlUtility.GetElementAtPath(xdoc.Root, new string[] { "FPDOMAINNAME" });
            if (elementAtPath == null)
            {
                throw CreateIdcrlException(-2147186646);
            }
            return elementAtPath.Value;
        }

        private FederationProviderInfo RequestFederationProviderInfo(string domainname)
        {
            int num;
            while ((num = domainname.IndexOf('.')) > 0)
            {
                string url = string.Format(CultureInfo.InvariantCulture, "http://msoid.{0}/FPUrl.xml", new object[] { domainname });
                try
                {
                    string fpDomainName = ParseFPDomainName(this.DoGet(url));
                    url = string.Format(CultureInfo.InvariantCulture, "http://msoid.{0}/FPList.xml", new object[] { domainname });
                    return ParseFederationProviderInfo(this.DoGet(url), fpDomainName);
                }
                catch (Exception exception)
                {
                    logger.Warn(exception.ToString());
                }
                domainname = domainname.Substring(num + 1);
            }
            return null;
        }

        private string FederationTokenIssuer
        {
            get
            {
                return this.m_federationTokenIssuer;
            }
        }

        private string ServiceTokenUrl
        {
            get
            {
                return this.m_securityTokenServiceUrl;
            }
        }

        private string UserRealmServiceUrl
        {
            get
            {
                return this.m_userRealmServiceUrl;
            }
        }

        private class FederationProviderInfo
        {            
            public string FederationTokenIssuer
            {
                get;
                set;
            }

            public string SecurityTokenServiceUrl
            {
                get;
                set;
            }

            public string UserRealmServiceUrl
            {
                get;
                set;
            }
        }

        private class FederationProviderInfoCache
        {
            private const int CacheLifetimeMinutes = 30;
            private Dictionary<string, IdcrlAuth.FederationProviderInfoCacheEntry> m_cache = new Dictionary<string, IdcrlAuth.FederationProviderInfoCacheEntry>(StringComparer.OrdinalIgnoreCase);
            private object m_lock = new object();

            public void Put(string domainname, IdcrlAuth.FederationProviderInfo value)
            {
                lock (this.m_lock)
                {
                    IdcrlAuth.FederationProviderInfoCacheEntry entry = new IdcrlAuth.FederationProviderInfoCacheEntry();
                    entry.Value = value;
                    entry.Expires = DateTime.UtcNow.AddMinutes(30.0);
                    this.m_cache[domainname] = entry;
                }
            }

            public bool TryGetValue(string domainname, out IdcrlAuth.FederationProviderInfo value)
            {
                lock (this.m_lock)
                {
                    IdcrlAuth.FederationProviderInfoCacheEntry entry;
                    if (this.m_cache.TryGetValue(domainname, out entry) && (entry.Expires > DateTime.UtcNow))
                    {
                        value = entry.Value;
                        return true;
                    }
                }
                value = null;
                return false;
            }
        }

        private class FederationProviderInfoCacheEntry
        {
            public DateTime Expires;
            public IdcrlAuth.FederationProviderInfo Value;
        }

        private class UserRealmInfo
        {           
            public bool IsFederated
            {
                get;
                set;
            }

            public string STSAuthUrl
            {
                get;
                set;
            }
        }
    }
}

