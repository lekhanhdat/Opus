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
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;

    internal class IdcrlAuth
    {
        // Fields
        private IdcrlEnvironment m_env;
        private EventHandler<SharePointOnlineCredentialsWebRequestEventArgs> m_executingWebRequest;
        private string m_federationTokenIssuer;
        private string m_securityTokenServiceUrl;
        private string m_userRealmServiceUrl;
        private static FederationProviderInfoCache s_FederationProviderInfoCache;
        private static Dictionary<string, int> s_partnerSoapErrorMap;
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);


        // Methods
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

        public IdcrlAuth(IdcrlEnvironment env, EventHandler<SharePointOnlineCredentialsWebRequestEventArgs> executingWebRequest)
        {
            this.m_env = env;
            log.Debug("IDCRL Environment {0}", env );
            if (this.m_env == IdcrlEnvironment.Production)
            {
                this.m_userRealmServiceUrl = "https://login.microsoftonline.com/GetUserRealm.srf";
                this.m_securityTokenServiceUrl = "https://login.microsoftonline.com/rst2.srf";
                this.m_federationTokenIssuer = "urn:federation:MicrosoftOnline";
            }
            else if (this.m_env == IdcrlEnvironment.Ppe)
            {
                this.m_userRealmServiceUrl = "https://login.windows-ppe.net/GetUserRealm.srf";
                this.m_securityTokenServiceUrl = "https://login.windows-ppe.net/rst2.srf";
                this.m_federationTokenIssuer = "urn:federation:MicrosoftOnline";
            }
            else
            {
                this.m_userRealmServiceUrl = "https://login.microsoftonline-int.com/GetUserRealm.srf";
                this.m_securityTokenServiceUrl = "https://login.microsoftonline-int.com/rst2.srf";
                this.m_federationTokenIssuer = "urn:federation:MicrosoftOnline-int";
            }
            this.m_executingWebRequest = executingWebRequest;
        }

        private string BuildWsSecurityUsingUsernamePassword(string username, string password)
        {
            DateTime utcNow = DateTime.UtcNow;
            return string.Format(CultureInfo.InvariantCulture, "\r\n            <wsse:UsernameToken wsu:Id=\"user\">\r\n                <wsse:Username>{0}</wsse:Username>\r\n                <wsse:Password>{1}</wsse:Password>\r\n            </wsse:UsernameToken>\r\n            <wsu:Timestamp Id=\"Timestamp\">\r\n                <wsu:Created>{2}</wsu:Created>\r\n                <wsu:Expires>{3}</wsu:Expires>\r\n            </wsu:Timestamp>\r\n", new object[] { IdcrlUtility.XmlValueEncode(username), IdcrlUtility.XmlValueEncode(password), utcNow.ToString("o", CultureInfo.InvariantCulture), utcNow.AddDays(1.0).ToString("o", CultureInfo.InvariantCulture) });
        }

        private static Exception CreateIdcrlException(int hr)
        {
            string str;
            if (!IdcrlErrorCodes.TryGetErrorStringId(hr, out str)) str = "PPCRL_REQUEST_E_UNKNOWN";
            return new IdcrlException("Unable to get ticket due to unknown error", hr);
        }

        private XDocument DoGet(string url)
        {
            XDocument document2;
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "GET";
            log.Debug("Sending GET request to {0}",url);
            if (this.m_executingWebRequest != null) this.m_executingWebRequest(this, new SharePointOnlineCredentialsWebRequestEventArgs(webRequest));
            HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse;
            if (response == null)
            {
                log.Warn("Unexpected response for GET request to URL {0}", url);
                throw new InvalidOperationException();
            }
            using (response)
            {
                using (TextReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string s = reader.ReadToEnd();
                    log.Debug("StatusCode={0}, ResponseText={1}", (int)response.StatusCode, s );
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
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = contentType;
            log.Debug("Sending POST request to {0}", url);
            if (this.m_executingWebRequest != null) this.m_executingWebRequest(this, new SharePointOnlineCredentialsWebRequestEventArgs(webRequest));
            using (Stream stream = webRequest.GetRequestStream())
            {
                if (body != null)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(body);
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
            try
            {
                HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse;
                if (response == null)
                {
                    log.Warn("Unexpected response for POST request to {0}", url);
                    throw new InvalidOperationException();
                }
                using (response)
                {
                    using (TextReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string s = reader.ReadToEnd();
                        log.Debug("URL={0}, StatusCode={1}, ResponseText={2}", url, (int)response.StatusCode, s);
                        using (XmlReader reader2 = XmlReader.Create(new StringReader(s)))
                        {
                            document2 = XDocument.Load(reader2);
                        }
                    }
                }
            }
            catch (WebException exception)
            {
                log.Warn("URL={0}, WebException={1}", url, exception);
                if (webExceptionHandler == null) throw;
                Exception exception2 = webExceptionHandler(exception);
                if (exception2 == null) throw;
                throw exception2;
            }
            return document2;
        }

        private FederationProviderInfo GetFederationProviderInfo(string domainname)
        {
            FederationProviderInfo info;
            if (s_FederationProviderInfoCache.TryGetValue(domainname, out info))
            {
                log.Debug("Get federation provider information for {0} from cache. UserRealmServiceUrl={1}, SecurityTokenServiceUrl={2}, FederationTokenIssuer={3}", domainname, (info == null) ? null : info.UserRealmServiceUrl, (info == null) ? null : info.SecurityTokenServiceUrl, (info == null) ? null : info.FederationTokenIssuer);
                return info;
            }
            info = this.RequestFederationProviderInfo(domainname);
            s_FederationProviderInfoCache.Put(domainname, info);
            log.Debug("Get federation provider information for {0} and put it in cache. UserRealmServcieUrl={1}, SecurityTokenServiceUrl={2}, FederationTokenIssuer={3}", domainname, (info == null) ? null : info.UserRealmServiceUrl, (info == null) ? null : info.SecurityTokenServiceUrl, (info == null) ? null : info.FederationTokenIssuer );
            return info;
        }

        private string GetPartnerTicketFromAdfs(string adfsUrl, string username, string password)
        {
            string body = string.Format(CultureInfo.InvariantCulture, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<s:Envelope xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\" xmlns:saml=\"urn:oasis:names:tc:SAML:1.0:assertion\" xmlns:wsp=\"http://schemas.xmlsoap.org/ws/2004/09/policy\" xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" xmlns:wsa=\"http://www.w3.org/2005/08/addressing\" xmlns:wssc=\"http://schemas.xmlsoap.org/ws/2005/02/sc\" xmlns:wst=\"http://schemas.xmlsoap.org/ws/2005/02/trust\">\r\n    <s:Header>\r\n        <wsa:Action s:mustUnderstand=\"1\">http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue</wsa:Action>\r\n        <wsa:To s:mustUnderstand=\"1\">{0}</wsa:To>\r\n        <wsa:MessageID>{1}</wsa:MessageID>\r\n        <ps:AuthInfo xmlns:ps=\"http://schemas.microsoft.com/Passport/SoapServices/PPCRL\" Id=\"PPAuthInfo\">\r\n            <ps:HostingApp>Managed IDCRL</ps:HostingApp>\r\n            <ps:BinaryVersion>6</ps:BinaryVersion>\r\n            <ps:UIVersion>1</ps:UIVersion>\r\n            <ps:Cookies></ps:Cookies>\r\n            <ps:RequestParams>AQAAAAIAAABsYwQAAAAxMDMz</ps:RequestParams>\r\n        </ps:AuthInfo>\r\n        <wsse:Security>\r\n            <wsse:UsernameToken wsu:Id=\"user\">\r\n                <wsse:Username>{2}</wsse:Username>\r\n                <wsse:Password>{3}</wsse:Password>\r\n            </wsse:UsernameToken>\r\n            <wsu:Timestamp Id=\"Timestamp\">\r\n                <wsu:Created>{4}</wsu:Created>\r\n                <wsu:Expires>{5}</wsu:Expires>\r\n            </wsu:Timestamp>\r\n        </wsse:Security>\r\n    </s:Header>\r\n    <s:Body>\r\n        <wst:RequestSecurityToken Id=\"RST0\">\r\n            <wst:RequestType>http://schemas.xmlsoap.org/ws/2005/02/trust/Issue</wst:RequestType>\r\n            <wsp:AppliesTo>\r\n                <wsa:EndpointReference>\r\n                    <wsa:Address>{6}</wsa:Address>\r\n                </wsa:EndpointReference>\r\n            </wsp:AppliesTo>\r\n            <wst:KeyType>http://schemas.xmlsoap.org/ws/2005/05/identity/NoProofKey</wst:KeyType>\r\n        </wst:RequestSecurityToken>\r\n    </s:Body>\r\n</s:Envelope>", new object[] { IdcrlUtility.XmlValueEncode(adfsUrl), Guid.NewGuid().ToString(), IdcrlUtility.XmlValueEncode(username), IdcrlUtility.XmlValueEncode(password), DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), DateTime.UtcNow.AddMinutes(10.0).ToString("o", CultureInfo.InvariantCulture), this.FederationTokenIssuer });
            XDocument xdoc = this.DoPost(adfsUrl, "application/soap+xml; charset=utf-8", body, new Func<WebException, Exception>(IdcrlAuth.HandleWebException));
            Exception soapException = GetSoapException(xdoc);
            if (soapException != null)
            {
                log.Warn("SOAP error from {0}. Exception={1}",adfsUrl, soapException );
                throw soapException;
            }
            XElement elementAtPath = IdcrlUtility.GetElementAtPath(xdoc.Root, new string[] { "{http://www.w3.org/2003/05/soap-envelope}Body", "{http://schemas.xmlsoap.org/ws/2005/02/trust}RequestSecurityTokenResponse", "{http://schemas.xmlsoap.org/ws/2005/02/trust}RequestedSecurityToken", "{urn:oasis:names:tc:SAML:1.0:assertion}Assertion" });
            if (elementAtPath == null)
            {
                log.Warn("Cannot get security assertion for user {0} from {1}", username, adfsUrl);
                throw CreateIdcrlException(-2147186451);
            }
            return elementAtPath.ToString((SaveOptions)2 | SaveOptions.DisableFormatting);
        }

        private string GetServiceToken(string securityXml, string serviceTarget, string servicePolicy)
        {
            string serviceTokenUrl = this.ServiceTokenUrl;
            string str2 = string.Empty;
            if (!string.IsNullOrEmpty(servicePolicy)) str2 = string.Format(CultureInfo.InvariantCulture, "<wsp:PolicyReference URI=\"{0}\"></wsp:PolicyReference>", new object[] { servicePolicy });
            string body = string.Format(CultureInfo.InvariantCulture, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<S:Envelope xmlns:S=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\" xmlns:wsp=\"http://schemas.xmlsoap.org/ws/2004/09/policy\" xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" xmlns:wsa=\"http://www.w3.org/2005/08/addressing\" xmlns:wst=\"http://schemas.xmlsoap.org/ws/2005/02/trust\">\r\n  <S:Header>\r\n    <wsa:Action S:mustUnderstand=\"1\">http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue</wsa:Action>\r\n    <wsa:To S:mustUnderstand=\"1\">{0}</wsa:To>\r\n    <ps:AuthInfo xmlns:ps=\"http://schemas.microsoft.com/LiveID/SoapServices/v1\" Id=\"PPAuthInfo\">\r\n      <ps:BinaryVersion>5</ps:BinaryVersion>\r\n      <ps:HostingApp>Managed IDCRL</ps:HostingApp>\r\n    </ps:AuthInfo>\r\n    <wsse:Security>{1}</wsse:Security>\r\n  </S:Header>\r\n  <S:Body>\r\n    <wst:RequestSecurityToken xmlns:wst=\"http://schemas.xmlsoap.org/ws/2005/02/trust\" Id=\"RST0\">\r\n      <wst:RequestType>http://schemas.xmlsoap.org/ws/2005/02/trust/Issue</wst:RequestType>\r\n      <wsp:AppliesTo>\r\n        <wsa:EndpointReference>\r\n          <wsa:Address>{2}</wsa:Address>\r\n        </wsa:EndpointReference>\r\n      </wsp:AppliesTo>\r\n      {3}\r\n    </wst:RequestSecurityToken>\r\n  </S:Body>\r\n</S:Envelope>\r\n", new object[] { IdcrlUtility.XmlValueEncode(serviceTokenUrl), securityXml, IdcrlUtility.XmlValueEncode(serviceTarget), str2 });
            XDocument xdoc = this.DoPost(serviceTokenUrl, "application/soap+xml; charset=utf-8", body, new Func<WebException, Exception>(IdcrlAuth.HandleWebException));
            Exception soapException = GetSoapException(xdoc);
            if (soapException != null)
            {
                log.Warn("Soap error from {0}. Exception={1}", serviceTokenUrl, soapException );
                throw soapException;
            }
            XElement elementAtPath = IdcrlUtility.GetElementAtPath(xdoc.Root, new string[] { "{http://www.w3.org/2003/05/soap-envelope}Body", "{http://schemas.xmlsoap.org/ws/2005/02/trust}RequestSecurityTokenResponse", "{http://schemas.xmlsoap.org/ws/2005/02/trust}RequestedSecurityToken", "{http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd}BinarySecurityToken" });
            if (elementAtPath == null)
            {
                log.Warn("Cannot get binary security token for from {0}", serviceTokenUrl );
                throw CreateIdcrlException(-2147186656);
            }
            return elementAtPath.Value;
        }

        public string GetServiceToken(string username, string password, string serviceTarget, string servicePolicy)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentNullException("username");
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException("password");
            if (string.IsNullOrEmpty(serviceTarget)) throw new ArgumentNullException("serviceTarget");
            this.InitFederationProviderInfoForUser(username);
            UserRealmInfo userRealm = this.GetUserRealm(username);
            if (userRealm.IsFederated)
            {
                var partnerTicketInitialized = false;

                try
                {
                    string partnerTicketFromAdfs = this.GetPartnerTicketFromAdfs(userRealm.STSAuthUrl, username, password);
                    partnerTicketInitialized = true;

                    return this.GetServiceToken(partnerTicketFromAdfs, serviceTarget, servicePolicy);
                }
                catch (Exception ex)
                {
                    //need to retry the default authentication if the password is the app password.
                    //ADFS User + MFA --> <State>4</State><UserState>1</UserState>
                    //ADFS User       --> <State>3</State><UserState>2</UserState>
                    if ("1".Equals(userRealm.UserState, StringComparison.OrdinalIgnoreCase) && (!partnerTicketInitialized))
                    {
                        //ClientULS.SendTraceTag(3454920u, ClientTraceCategory.Authentication, ClientTraceLevel.High,
                        //    "Failed to get user's service token for user {0}, so try to get the service token from the WS Security. {1}",
                        //    username, ex);
                        try
                        {
                            return GetServiceTokenUsingWsSecurity(username, password, serviceTarget, servicePolicy);
                        }
                        catch (Exception newEx)
                        {
                            log.Error("Failed to get user's service token for user {0} with the WS Security. {1} \r\n--> \r\n{2}",username, newEx, ex);
                        }
                    }
                    throw;
                }
            }
            return GetServiceTokenUsingWsSecurity(username, password, serviceTarget, servicePolicy);
        }

        private string GetServiceTokenUsingWsSecurity(string username, string password, string serviceTarget, string servicePolicy)
        {
            string securityXml = BuildWsSecurityUsingUsernamePassword(username, password);
            return GetServiceToken(securityXml, serviceTarget, servicePolicy);
        }

        private static Exception GetSoapException(XDocument xdoc)
        {
            int num2;
            if (IdcrlUtility.GetElementAtPath(xdoc.Root, new string[] { "{http://www.w3.org/2003/05/soap-envelope}Body", "{http://www.w3.org/2003/05/soap-envelope}Fault" }) == null) return null;
            XElement elementAtPath = IdcrlUtility.GetElementAtPath(xdoc.Root, new string[] { "{http://www.w3.org/2003/05/soap-envelope}Body", "{http://www.w3.org/2003/05/soap-envelope}Fault", "{http://www.w3.org/2003/05/soap-envelope}Code", "{http://www.w3.org/2003/05/soap-envelope}Subcode", "{http://www.w3.org/2003/05/soap-envelope}Value" });
            XElement element3 = IdcrlUtility.GetElementAtPath(xdoc.Root, new string[] { "{http://www.w3.org/2003/05/soap-envelope}Body", "{http://www.w3.org/2003/05/soap-envelope}Fault", "{http://www.w3.org/2003/05/soap-envelope}Detail", "{http://schemas.microsoft.com/Passport/SoapServices/SOAPFault}error", "{http://schemas.microsoft.com/Passport/SoapServices/SOAPFault}value" });
            XElement element4 = IdcrlUtility.GetElementAtPath(xdoc.Root, new string[] { "{http://www.w3.org/2003/05/soap-envelope}Body", "{http://www.w3.org/2003/05/soap-envelope}Fault", "{http://www.w3.org/2003/05/soap-envelope}Detail", "{http://schemas.microsoft.com/Passport/SoapServices/SOAPFault}error", "{http://schemas.microsoft.com/Passport/SoapServices/SOAPFault}internalerror", "{http://schemas.microsoft.com/Passport/SoapServices/SOAPFault}text" });
            string code = null;
            if (elementAtPath != null)
            {
                code = elementAtPath.Value;
                int index = code.IndexOf(':');
                if (index >= 0) code = code.Substring(index + 1);
            }
            string str2 = null;
            if (element3 != null) str2 = element3.Value;
            string str3 = null;
            if (element4 != null) str3 = element4.Value;
            log.Debug("PassportErrorCode={0}, PassportDetailCode={1}, PassportErrorText={2}", code, str2, str3);
            if (string.IsNullOrEmpty(str2))
                num2 = MapPartnerSoapFault(code);
            else
            {
                long num3;
                if (str2.StartsWith("0x", StringComparison.OrdinalIgnoreCase) && long.TryParse(str2.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num3) || long.TryParse(str2, NumberStyles.Integer, CultureInfo.InvariantCulture, out num3))
                {
                    num2 = (int)num3;
                    if (string.Compare(code, "FailedAuthentication", StringComparison.OrdinalIgnoreCase) == 0) num2 = (num2 == -2147186639) ? num2 : -2147186655;
                }
                else
                    num2 = -2147186656;
            }
            return CreateIdcrlException(num2);
        }

        private UserRealmInfo GetUserRealm(string login)
        {
            if (string.IsNullOrEmpty(login)) throw new ArgumentNullException("login");
            string userRealmServiceUrl = this.UserRealmServiceUrl;
            string body = string.Format(CultureInfo.InvariantCulture, "login={0}&xml=1", new object[] { Uri.EscapeDataString(login) });
            XDocument document = this.DoPost(userRealmServiceUrl, "application/x-www-form-urlencoded", body, null);
            XAttribute attribute = document.Root.Attribute("Success");
            if (attribute == null || string.Compare(attribute.Value, "true", StringComparison.OrdinalIgnoreCase) != 0)
            {
                log.Warn("Failed to get user's realm for user {0}", login);
                throw CreateIdcrlException(-2147186539);
            }
            XElement element = document.Root.Element("NameSpaceType");
            if (element == null)
            {
                log.Warn("There is no NameSpaceType element in the response when get user realm for user {0}",login );
                throw CreateIdcrlException(-2147186539);
            }
            if (string.Compare(element.Value, "Federated", StringComparison.OrdinalIgnoreCase) != 0 && string.Compare(element.Value, "Managed", StringComparison.OrdinalIgnoreCase) != 0)
            {
                log.Warn("Unknown namespace type for user {0}", login);
                throw CreateIdcrlException(-2147186539);
            }
            UserRealmInfo info = new UserRealmInfo
            {
                IsFederated = 0 == string.Compare(element.Value, "Federated", StringComparison.OrdinalIgnoreCase)
            };
            element = document.Root.Element("STSAuthURL");
            if (element != null) info.STSAuthUrl = element.Value;

            element = document.Root.Element("State");
            if (element != null)
            {
                info.State = element.Value;
            }

            element = document.Root.Element("UserState");
            if (element != null)
            {
                info.UserState = element.Value;
            }
            if (info.IsFederated && string.IsNullOrEmpty(info.STSAuthUrl))
            {
                log.Warn("User {0} is a federated account, but there is no STSAuthUrl for the user.", login);
                throw CreateIdcrlException(-2147186539);
            }
            log.Debug("User={0}, IsFederated={1}, STSAuthUrl={2}", login, info.IsFederated, info.STSAuthUrl);
            return info;
        }

        private static Exception HandleWebException(WebException webException)
        {
            HttpWebResponse response = webException.Response as HttpWebResponse;
            if (response != null && response.ContentType != null && response.ContentType.IndexOf("application/soap+xml", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                try
                {
                    using (TextReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string s = reader.ReadToEnd();
                        log.Debug("StatusCode={0}, ResponseText={1}", (int)response.StatusCode, s);
                        using (XmlReader reader2 = XmlReader.Create(new StringReader(s)))
                        {
                            return GetSoapException(XDocument.Load(reader2));
                        }
                    }
                }
                catch (XmlException exception2)
                {
                    log.Warn("Error when read error response. Exception={0}",  exception2 );
                }
                catch (IOException exception3)
                {
                    log.Warn("Error when read error response. Exception={0}", exception3 );
                }
            }
            return null;
        }

        private void InitFederationProviderInfoForUser(string username)
        {
            int index = username.IndexOf('@');
            if (index < 0 || index == username.Length - 1) throw new ArgumentException("username");
            string domainname = username.Substring(index + 1);
            FederationProviderInfo federationProviderInfo = this.GetFederationProviderInfo(domainname);
            if (federationProviderInfo != null)
            {
                this.m_userRealmServiceUrl = federationProviderInfo.UserRealmServiceUrl;
                this.m_securityTokenServiceUrl = federationProviderInfo.SecurityTokenServiceUrl;
                this.m_federationTokenIssuer = federationProviderInfo.FederationTokenIssuer;
            }
            log.Debug("UserName={0}, UserRealmServiceUrl={1}, SecurityTokenServiceUrl={1}, FederationTokenIssuer={2}",  username, this.m_userRealmServiceUrl, this.m_securityTokenServiceUrl, this.m_federationTokenIssuer );
        }

        private static int MapPartnerSoapFault(string code)
        {
            int num;
            if (s_partnerSoapErrorMap.TryGetValue(code, out num)) return num;
            return -2147186451;
        }

        private static FederationProviderInfo ParseFederationProviderInfo(XDocument xdoc, string fpDomainName)
        {
            foreach (XElement element in xdoc.Root.Elements("FP"))
            {
                if (element.Attribute("DomainName") == null || !string.Equals(element.Attribute("DomainName").Value, fpDomainName, StringComparison.OrdinalIgnoreCase)) continue;
                XElement elementAtPath = IdcrlUtility.GetElementAtPath(element, new string[] { "URL", "GETUSERREALM" });
                XElement element3 = IdcrlUtility.GetElementAtPath(element, new string[] { "URL", "RST2" });
                XElement element4 = IdcrlUtility.GetElementAtPath(element, new string[] { "URL", "ENTITYID" });
                if (elementAtPath == null || element3 == null || element4 == null)
                {
                    log.Warn("Cannot get the user realm service url or security token service url for federation provider {0}", fpDomainName );
                    throw CreateIdcrlException(-2147186646);
                }
                log.Debug("Find federation provider information for federation provider domain name {0}. UserRealmServiceUrl={1}, SecurityTokenServiceUrl={2}, FederationTokenIssuer={3}", fpDomainName, elementAtPath.Value, element3.Value, element4.Value);
                return new FederationProviderInfo { UserRealmServiceUrl = elementAtPath.Value, SecurityTokenServiceUrl = element3.Value, FederationTokenIssuer = element4.Value };
            }
            log.Warn("Cannot find federation provider information for federation domain {0}", fpDomainName );
            throw CreateIdcrlException(-2147186646);
        }

        private static string ParseFPDomainName(XDocument xdoc)
        {
            XElement elementAtPath = IdcrlUtility.GetElementAtPath(xdoc.Root, new string[] { "FPDOMAINNAME" });
            if (elementAtPath == null)
            {
                return null;
                //log.Warn("Cannot find FPDOMAINNAME element");
                //throw CreateIdcrlException(-2147186646);
            }
            return elementAtPath.Value;
        }

        private FederationProviderInfo RequestFederationProviderInfo(string domainname)
        {
            int num;
            while ((num = domainname.IndexOf('.')) > 0)
            {
                string url = string.Format(CultureInfo.InvariantCulture, IdcrlMessageConstants.FPUrlFullUrlFormat, new object[] { domainname });
                try
                {
                    string fpDomainName = ParseFPDomainName(this.DoGet(url));
                    if (!string.IsNullOrEmpty(fpDomainName))
                        url = string.Format(CultureInfo.InvariantCulture, IdcrlMessageConstants.FPListFullUrlFormat, new object[] { domainname });
                    return ParseFederationProviderInfo(this.DoGet(url), fpDomainName);
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.NameResolutionFailure)
                    {
                        log.Debug("Exception when request {0}. Exception={1}", url, ex.Message);
                    }
                    else
                    {
                        log.Debug("Exception when request {0}. Exception={1}", url, ex);
                    }
                }
                catch (XmlException e)
                {
                    log.Debug("Exception when request {0}. Exception={1}", url, e);
                }
                domainname = domainname.Substring(num + 1);
            }
            return null;
        }

        // Properties
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

        // Nested Types
        private class FederationProviderInfo
        {
            // Properties
            public string FederationTokenIssuer { get; set; }

            public string SecurityTokenServiceUrl { get; set; }

            public string UserRealmServiceUrl { get; set; }
        }

        private class FederationProviderInfoCache
        {
            // Fields
            private const int CacheLifetimeMinutes = 30;
            private Dictionary<string, IdcrlAuth.FederationProviderInfoCacheEntry> m_cache = new Dictionary<string, IdcrlAuth.FederationProviderInfoCacheEntry>(StringComparer.OrdinalIgnoreCase);
            private object m_lock = new object();

            // Methods
            public void Put(string domainname, IdcrlAuth.FederationProviderInfo value)
            {
                lock (this.m_lock)
                {
                    IdcrlAuth.FederationProviderInfoCacheEntry entry = new IdcrlAuth.FederationProviderInfoCacheEntry
                    {
                        Value = value,
                        Expires = DateTime.UtcNow.AddMinutes(30.0)
                    };
                    this.m_cache[domainname] = entry;
                }
            }

            public bool TryGetValue(string domainname, out IdcrlAuth.FederationProviderInfo value)
            {
                lock (this.m_lock)
                {
                    IdcrlAuth.FederationProviderInfoCacheEntry entry;
                    if (this.m_cache.TryGetValue(domainname, out entry) && entry.Expires > DateTime.UtcNow)
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
            // Fields
            public DateTime Expires;
            public IdcrlAuth.FederationProviderInfo Value;
        }

        private class UserRealmInfo
        {
            // Properties
            public bool IsFederated { get; set; }

            public string STSAuthUrl { get; set; }

            public String UserState { get; set; }

            public String State { get; set; }
        }
    }
}

