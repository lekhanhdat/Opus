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
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft365.Common.Exception;
using Microsoft365.Common.HttpUtil;
using Microsoft365.Common.Logger;
using Microsoft365.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Microsoft365.Authentication.Token.Idclr
{
    internal class IdcrlAuth
    {

        private static IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(IdcrlAuth));

        private class FederationProviderInfo
        {
            public string UserRealmServiceUrl
            {
                get;
                set;
            }

            public string SecurityTokenServiceUrl
            {
                get;
                set;
            }

            public string FederationTokenIssuer
            {
                get;
                set;
            }
        }

        private class FederationProviderInfoCacheEntry
        {
            public FederationProviderInfo Value;

            public DateTime Expires;
        }

        private class FederationProviderInfoCache
        {

            private Dictionary<string, FederationProviderInfoCacheEntry> cache = new Dictionary<string, FederationProviderInfoCacheEntry>(StringComparer.OrdinalIgnoreCase);

            public bool TryGetValue(string domainname, out FederationProviderInfo value)
            {
                lock (cache)
                {
                    FederationProviderInfoCacheEntry federationProviderInfoCacheEntry;
                    if (cache.TryGetValue(domainname, out federationProviderInfoCacheEntry) && federationProviderInfoCacheEntry.Expires > DateTime.UtcNow)
                    {
                        value = federationProviderInfoCacheEntry.Value;
                        return true;
                    }
                }
                value = null;
                return false;
            }

            public void Put(string domainname, FederationProviderInfo value)
            {
                lock (cache)
                {
                    cache[domainname] = new FederationProviderInfoCacheEntry
                    {
                        Value = value,
                        Expires = DateTime.UtcNow.AddMinutes(30.0)
                    };
                }
            }
        }

        private readonly AveAzureEnvironment environment;

        private string userRealmServiceUrl;

        private string securityTokenServiceUrl;

        private string federationTokenIssuer;

        private string userDomainName;

        private EventHandler<SPOCredentialsWebRequestEventArgs> executingWebRequest;

        private static Dictionary<string, int> partnerSoapErrorMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "InvalidRequest",
                -2147186474
            },
            {
                "FailedAuthentication",
                -2147186446
            },
            {
                "RequestFailed",
                -2147186473
            },
            {
                "InvalidSecurityToken",
                -2147186472
            },
            {
                "AuthenticationBadElements",
                -2147186471
            },
            {
                "BadRequest",
                -2147186470
            },
            {
                "ExpiredData",
                -2147186469
            },
            {
                "InvalidTimeRange",
                -2147186468
            },
            {
                "InvalidScope",
                -2147186467
            },
            {
                "RenewNeeded",
                -2147186466
            },
            {
                "UnableToRenew",
                -2147186465
            }
        };

        private static FederationProviderInfoCache federationProviderInfoCache = new FederationProviderInfoCache();

        public IdcrlAuth(IdcrlEnvironment env, AveAzureEnvironment environment, EventHandler<SPOCredentialsWebRequestEventArgs> executingWebRequest)
        {
            this.environment = environment;

            switch (environment)
            {
                case AveAzureEnvironment.AzureCloud:
                case AveAzureEnvironment.AzurePPE:
                case AveAzureEnvironment.None:
                    {
                        userRealmServiceUrl = "https://login.microsoftonline.com/GetUserRealm.srf";
                        securityTokenServiceUrl = "https://login.microsoftonline.com/rst2.srf";
                        federationTokenIssuer = "urn:federation:MicrosoftOnline";
                    }
                    break;
                case AveAzureEnvironment.AzureGermanyCloud:
                    {
                        userRealmServiceUrl = "https://login.microsoftonline.de/GetUserRealm.srf";
                        securityTokenServiceUrl = "https://login.microsoftonline.de/rst2.srf";
                        federationTokenIssuer = "urn:federation:microsoftonline.de";
                    }
                    break;
                case AveAzureEnvironment.USGovernment:
                case AveAzureEnvironment.USGovernmentDOD:
                {
                        userRealmServiceUrl = "https://login.microsoftonline.us/GetUserRealm.srf";
                        securityTokenServiceUrl = "https://login.microsoftonline.us/rst2.srf";
                        federationTokenIssuer = "urn:federation:microsoftonline.us";
                    }
                    break;
                case AveAzureEnvironment.AzureChinaCloud:
                    {
                        userRealmServiceUrl = "https://login.partner.microsoftonline.cn/GetUserRealm.srf";
                        securityTokenServiceUrl = "https://login.partner.microsoftonline.cn/rst2.srf";
                        federationTokenIssuer = "urn:federation:partner.microsoftonline.cn";
                    }
                    break;
            }

            //if (env == IdcrlEnvironment.Production)
            //{
            //	userRealmServiceUrl = "https://login.microsoftonline.com/GetUserRealm.srf";
            //	securityTokenServiceUrl = "https://login.microsoftonline.com/rst2.srf";
            //	federationTokenIssuer = "urn:federation:MicrosoftOnline";
            //}
            //else
            //{
            //	userRealmServiceUrl = "https://login.microsoftonline-int.com/GetUserRealm.srf";
            //	securityTokenServiceUrl = "https://login.microsoftonline-int.com/rst2.srf";
            //	federationTokenIssuer = "urn:federation:MicrosoftOnline-int";
            //}
            this.executingWebRequest = executingWebRequest;
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
            InitFederationProviderInfoForUser(username);
            OutputDetails(username, password, serviceTarget, servicePolicy);
            UserRealmInfo userRealm = GetUserRealm(username);
            if (userRealm.IsFederated)
            {
                var partnerTicketInitialized = false;

                try
                {
                    string partnerTicketFromAdfs = null;

                    var authApi = AuthenticationFramework.GetAuthProviderApi(userDomainName);

                    if (authApi != null)
                    {
                        var federationAuthApi = authApi.CreateFederationAuthApi();

                        partnerTicketFromAdfs = federationAuthApi.GetTicket(userRealm.STSAuthUrl, username, password, federationTokenIssuer);
                    }
                    else
                    {
                        partnerTicketFromAdfs = GetPartnerTicketFromAdfs(userRealm.STSAuthUrl, username, password);
                    }

                    partnerTicketInitialized = true;

                    return GetServiceToken(partnerTicketFromAdfs, serviceTarget, servicePolicy);
                }
                catch (Exception ex)
                {
                    //ADFS User + MFA --> <State>4</State><UserState>1</UserState>
                    //ADFS User       --> <State>3</State><UserState>2</UserState>
                    if ("1".Equals(userRealm.UserState, StringComparison.OrdinalIgnoreCase) && !partnerTicketInitialized)
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
                            logger.SendTraceTag(3454920u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.High,
                            "Failed to get user's service token for user {0} with the WS Security. {1} \r\n--> \r\n{2}",
                            username, newEx, ex);
                        }
                    }

                    throw;
                }
            }
            return GetServiceTokenUsingWsSecurity(username, password, serviceTarget, servicePolicy);
        }

        public Authentication.UserRealmInfo GetUserRealmInfo(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username");
            }
            InitFederationProviderInfoForUser(username);
            OutputDetails(username, null, null, null);
            var internalUserRealm = GetUserRealm(username);

            //ADFS User + MFA --> <State>4</State><UserState>1</UserState>
            //ADFS User       --> <State>3</State><UserState>2</UserState>

            var userRealmInfo = new UserRealmInfo()
            {
                STSAuthUrl = internalUserRealm.STSAuthUrl,
                IsFederated = internalUserRealm.IsFederated,
                State = internalUserRealm.State,
                UserState = internalUserRealm.UserState,
            };

            if (userRealmInfo.IsFederated && "1".Equals(userRealmInfo.UserState, StringComparison.OrdinalIgnoreCase))
            {
                userRealmInfo.MFAEnabled = true;
            }

            return userRealmInfo;
        }

        private string GetServiceTokenUsingWsSecurity(string username, string password, string serviceTarget, string servicePolicy)
        {
            string securityXml = BuildWsSecurityUsingUsernamePassword(username, password);
            return GetServiceToken(securityXml, serviceTarget, servicePolicy);
        }

        private UserRealmInfo GetUserRealm(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentNullException("login");
            }
            string userRealmServiceUrl = this.userRealmServiceUrl;
            string body = string.Format(CultureInfo.InvariantCulture, "login={0}&xml=1", new object[]
            {
                Uri.EscapeDataString(login)
            });
            XDocument xDocument = DoPost(userRealmServiceUrl, "application/x-www-form-urlencoded", body, null);
            XAttribute xAttribute = xDocument.Root.Attribute("Success");
            if (xAttribute == null || string.Compare(xAttribute.Value, "true", StringComparison.OrdinalIgnoreCase) != 0)
            {
                logger.SendTraceTag(3454919u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.High, "Failed to get user's realm for user {0}", new object[]
                {
                    login
                });
                throw CreateIdcrlException(-2147186539);
            }
            XElement xElement = xDocument.Root.Element("NameSpaceType");
            if (xElement == null)
            {
                logger.SendTraceTag(3454920u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.High, "There is no NameSpaceType element in the response when get user realm for user {0}", new object[]
                {
                    login
                });
                throw CreateIdcrlException(-2147186539);
            }
            if (string.Compare(xElement.Value, "Federated", StringComparison.OrdinalIgnoreCase) != 0 && string.Compare(xElement.Value, "Managed", StringComparison.OrdinalIgnoreCase) != 0)
            {
                logger.SendTraceTag(3454921u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.High, "Unknown namespace type for user {0}", new object[]
                {
                    login
                });
                throw CreateIdcrlException(-2147186539);
            }
            UserRealmInfo userRealmInfo = new UserRealmInfo();
            userRealmInfo.IsFederated = 0 == string.Compare(xElement.Value, "Federated", StringComparison.OrdinalIgnoreCase);
            xElement = xDocument.Root.Element("STSAuthURL");
            if (xElement != null)
            {
                userRealmInfo.STSAuthUrl = xElement.Value;
            }

            xElement = xDocument.Root.Element("State");

            if (xElement != null)
            {
                userRealmInfo.State = xElement.Value;
            }

            xElement = xDocument.Root.Element("UserState");

            if (xElement != null)
            {
                userRealmInfo.UserState = xElement.Value;
            }

            if (userRealmInfo.IsFederated && string.IsNullOrEmpty(userRealmInfo.STSAuthUrl))
            {
                logger.SendTraceTag(3454922u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.High, "User {0} is a federated account, but there is no STSAuthUrl for the user.", new object[]
                {
                    login
                });
                throw CreateIdcrlException(-2147186539);
            }
            logger.SendTraceTag(3454923u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.Verbose, "User={0}, IsFederated={1}, STSAuthUrl={2}", new object[]
            {
                login,
                userRealmInfo.IsFederated,
                userRealmInfo.STSAuthUrl
            });
            return userRealmInfo;
        }

        private string GetPartnerTicketFromAdfs(string adfsUrl, string username, string password)
        {
            string body = string.Format(CultureInfo.InvariantCulture, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<s:Envelope xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\" xmlns:saml=\"urn:oasis:names:tc:SAML:1.0:assertion\" xmlns:wsp=\"http://schemas.xmlsoap.org/ws/2004/09/policy\" xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" xmlns:wsa=\"http://www.w3.org/2005/08/addressing\" xmlns:wssc=\"http://schemas.xmlsoap.org/ws/2005/02/sc\" xmlns:wst=\"http://schemas.xmlsoap.org/ws/2005/02/trust\">\r\n    <s:Header>\r\n        <wsa:Action s:mustUnderstand=\"1\">http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue</wsa:Action>\r\n        <wsa:To s:mustUnderstand=\"1\">{0}</wsa:To>\r\n        <wsa:MessageID>{1}</wsa:MessageID>\r\n        <ps:AuthInfo xmlns:ps=\"http://schemas.microsoft.com/Passport/SoapServices/PPCRL\" Id=\"PPAuthInfo\">\r\n            <ps:HostingApp>Managed IDCRL</ps:HostingApp>\r\n            <ps:BinaryVersion>6</ps:BinaryVersion>\r\n            <ps:UIVersion>1</ps:UIVersion>\r\n            <ps:Cookies></ps:Cookies>\r\n            <ps:RequestParams>AQAAAAIAAABsYwQAAAAxMDMz</ps:RequestParams>\r\n        </ps:AuthInfo>\r\n        <wsse:Security>\r\n            <wsse:UsernameToken wsu:Id=\"user\">\r\n                <wsse:Username>{2}</wsse:Username>\r\n                <wsse:Password>{3}</wsse:Password>\r\n            </wsse:UsernameToken>\r\n            <wsu:Timestamp Id=\"Timestamp\">\r\n                <wsu:Created>{4}</wsu:Created>\r\n                <wsu:Expires>{5}</wsu:Expires>\r\n            </wsu:Timestamp>\r\n        </wsse:Security>\r\n    </s:Header>\r\n    <s:Body>\r\n        <wst:RequestSecurityToken Id=\"RST0\">\r\n            <wst:RequestType>http://schemas.xmlsoap.org/ws/2005/02/trust/Issue</wst:RequestType>\r\n            <wsp:AppliesTo>\r\n                <wsa:EndpointReference>\r\n                    <wsa:Address>{6}</wsa:Address>\r\n                </wsa:EndpointReference>\r\n            </wsp:AppliesTo>\r\n            <wst:KeyType>http://schemas.xmlsoap.org/ws/2005/05/identity/NoProofKey</wst:KeyType>\r\n        </wst:RequestSecurityToken>\r\n    </s:Body>\r\n</s:Envelope>", new object[]
            {
                IdcrlUtility.XmlValueEncode(adfsUrl),
                Guid.NewGuid().ToString(),
                IdcrlUtility.XmlValueEncode(username),
                IdcrlUtility.XmlValueEncode(password),
                DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                DateTime.UtcNow.AddMinutes(10.0).ToString("o", CultureInfo.InvariantCulture),
                federationTokenIssuer
            });
            XDocument xDocument = DoPost(adfsUrl, "application/soap+xml; charset=utf-8", body, new Func<WebException, Exception>(HandleWebException));
            Exception soapException = GetSoapException(xDocument);
            if (soapException != null)
            {
                logger.SendTraceTag(3454924u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.High, "SOAP error from {0}. Exception={1}", new object[]
                {
                    adfsUrl,
                    soapException
                });
                throw soapException;
            }
            XElement elementAtPath = IdcrlUtility.GetElementAtPath(xDocument.Root, new string[]
            {
                "{http://www.w3.org/2003/05/soap-envelope}Body",
                "{http://schemas.xmlsoap.org/ws/2005/02/trust}RequestSecurityTokenResponse",
                "{http://schemas.xmlsoap.org/ws/2005/02/trust}RequestedSecurityToken",
                "{urn:oasis:names:tc:SAML:1.0:assertion}Assertion"
            });
            if (elementAtPath == null)
            {
                logger.SendTraceTag(3454925u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.High, "Cannot get security assertion for user {0} from {1}", new object[]
                {
                    username,
                    adfsUrl
                });
                throw CreateIdcrlException(-2147186451);
            }
            return elementAtPath.ToString(SaveOptions.DisableFormatting | SaveOptions.OmitDuplicateNamespaces);
        }

        private string GetServiceToken(string securityXml, string serviceTarget, string servicePolicy)
        {
            string serviceTokenUrl = securityTokenServiceUrl;
            string text = string.Empty;
            if (!string.IsNullOrEmpty(servicePolicy))
            {
                text = string.Format(CultureInfo.InvariantCulture, "<wsp:PolicyReference URI=\"{0}\"></wsp:PolicyReference>", new object[]
                {
                    servicePolicy
                });
            }
            string body = string.Format(CultureInfo.InvariantCulture, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<S:Envelope xmlns:S=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\" xmlns:wsp=\"http://schemas.xmlsoap.org/ws/2004/09/policy\" xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" xmlns:wsa=\"http://www.w3.org/2005/08/addressing\" xmlns:wst=\"http://schemas.xmlsoap.org/ws/2005/02/trust\">\r\n  <S:Header>\r\n    <wsa:Action S:mustUnderstand=\"1\">http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue</wsa:Action>\r\n    <wsa:To S:mustUnderstand=\"1\">{0}</wsa:To>\r\n    <ps:AuthInfo xmlns:ps=\"http://schemas.microsoft.com/LiveID/SoapServices/v1\" Id=\"PPAuthInfo\">\r\n      <ps:BinaryVersion>5</ps:BinaryVersion>\r\n      <ps:HostingApp>Managed IDCRL</ps:HostingApp>\r\n    </ps:AuthInfo>\r\n    <wsse:Security>{1}</wsse:Security>\r\n  </S:Header>\r\n  <S:Body>\r\n    <wst:RequestSecurityToken xmlns:wst=\"http://schemas.xmlsoap.org/ws/2005/02/trust\" Id=\"RST0\">\r\n      <wst:RequestType>http://schemas.xmlsoap.org/ws/2005/02/trust/Issue</wst:RequestType>\r\n      <wsp:AppliesTo>\r\n        <wsa:EndpointReference>\r\n          <wsa:Address>{2}</wsa:Address>\r\n        </wsa:EndpointReference>\r\n      </wsp:AppliesTo>\r\n      {3}\r\n    </wst:RequestSecurityToken>\r\n  </S:Body>\r\n</S:Envelope>\r\n", new object[]
            {
                IdcrlUtility.XmlValueEncode(serviceTokenUrl),
                securityXml,
                IdcrlUtility.XmlValueEncode(serviceTarget),
                text
            });
            XDocument xDocument = DoPost(serviceTokenUrl, "application/soap+xml; charset=utf-8", body, new Func<WebException, Exception>(HandleWebException));
            Exception soapException = GetSoapException(xDocument);
            if (soapException != null)
            {
                logger.SendTraceTag(3454926u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.High, "Soap error from {0}. Exception={1}", new object[]
                {
                    serviceTokenUrl,
                    soapException
                });
                throw soapException;
            }
            XElement elementAtPath = IdcrlUtility.GetElementAtPath(xDocument.Root, new string[]
            {
                "{http://www.w3.org/2003/05/soap-envelope}Body",
                "{http://schemas.xmlsoap.org/ws/2005/02/trust}RequestSecurityTokenResponse",
                "{http://schemas.xmlsoap.org/ws/2005/02/trust}RequestedSecurityToken",
                "{http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd}BinarySecurityToken"
            });
            if (elementAtPath == null)
            {
                logger.SendTraceTag(3454927u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.High, "Cannot get binary security token for from {0}", new object[]
                {
                    serviceTokenUrl
                });
                throw CreateIdcrlException(-2147186656);
            }
            return elementAtPath.Value;
        }

        private string BuildWsSecurityUsingUsernamePassword(string username, string password)
        {
            DateTime utcNow = DateTime.UtcNow;
            return string.Format(CultureInfo.InvariantCulture, "\r\n            <wsse:UsernameToken wsu:Id=\"user\">\r\n                <wsse:Username>{0}</wsse:Username>\r\n                <wsse:Password>{1}</wsse:Password>\r\n            </wsse:UsernameToken>\r\n            <wsu:Timestamp Id=\"Timestamp\">\r\n                <wsu:Created>{2}</wsu:Created>\r\n                <wsu:Expires>{3}</wsu:Expires>\r\n            </wsu:Timestamp>\r\n", new object[]
            {
                IdcrlUtility.XmlValueEncode(username),
                IdcrlUtility.XmlValueEncode(password),
                utcNow.ToString("o", CultureInfo.InvariantCulture),
                utcNow.AddDays(1.0).ToString("o", CultureInfo.InvariantCulture)
            });
        }

        private XDocument DoPost(string url, string contentType, string body, Func<WebException, Exception> webExceptionHandler)
        {
            var httpWebRequest = Microsoft365Configuration.CommonConfiguration.WebRequestProvider.CreateRequest(new Uri(url));
            httpWebRequest.UserAgent = Microsoft365Configuration.CommonConfiguration.UserAgent;
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = contentType;
            //ClientULS.SendTraceTag(3454928u, ClientTraceCategory.Authentication, ClientTraceLevel.Verbose, "Sending POST request to {0}", new object[]
            //{
            //	url
            //});
            if (executingWebRequest != null)
            {
                executingWebRequest(this, new SPOCredentialsWebRequestEventArgs(httpWebRequest));
            }
            using (Stream requestStream = httpWebRequest.GetRequestStream())
            {
                if (body != null)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(body);
                    requestStream.Write(bytes, 0, bytes.Length);
                }
            }
            XDocument result;
            try
            {
                HttpWebResponse httpWebResponse = httpWebRequest.WebRequest.GetResponseByHttpClient(null,"Authentication",RestClientFactory.DefaultStrategies);
                if (httpWebResponse == null)
                {
                    logger.SendTraceTag(3454929u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.High, "Unexpected response for POST request to {0}", new object[]
                    {
                        url
                    });
                    throw new InvalidOperationException();
                }
                using (httpWebResponse)
                {
                    using (TextReader textReader = new StreamReader(httpWebResponse.GetResponseStream()))
                    {
                        string text = textReader.ReadToEnd();
                        string responseValue = text.Contains("<S:Fault>") ? text : string.Empty;
                        logger.SendTraceTag(3454930u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.Verbose, "URL={0}, StatusCode={1}, ResponseText={2}", new object[]
                        {
                            url,
                            (int)httpWebResponse.StatusCode,
                            responseValue
                        });
                        using (XmlReader xmlReader = XmlReader.Create(new StringReader(text)))
                        {
                            XDocument xDocument = XDocument.Load(xmlReader);
                            result = xDocument;
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                logger.SendTraceTag(3454931u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.High, "URL={0}, WebException={1}", new object[]
                {
                    url,
                    ex
                });
                if (webExceptionHandler == null)
                {
                    throw;
                }
                Exception ex2 = webExceptionHandler(ex);
                if (ex2 == null)
                {
                    throw;
                }
                throw ex2;
            }
            return result;
        }

        private static Exception HandleWebException(WebException webException)
        {
            HttpWebResponse httpWebResponse = webException.Response as HttpWebResponse;
            if (httpWebResponse != null && httpWebResponse.ContentType != null && httpWebResponse.ContentType.IndexOf("application/soap+xml", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                try
                {
                    using (TextReader textReader = new StreamReader(httpWebResponse.GetResponseStream()))
                    {
                        string text = textReader.ReadToEnd();
                        logger.SendTraceTag(3454932u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.High, "StatusCode={0}, ResponseText={1}", new object[]
                        {
                            (int)httpWebResponse.StatusCode,
                            text
                        });
                        using (XmlReader xmlReader = XmlReader.Create(new StringReader(text)))
                        {
                            XDocument xdoc = XDocument.Load(xmlReader);
                            return GetSoapException(xdoc);
                        }
                    }
                }
                catch (XmlException ex)
                {
                    logger.SendTraceTag(3454933u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.Medium, "Error when read error response. Exception={0}", new object[]
                    {
                        ex
                    });
                }
                catch (IOException ex2)
                {
                    logger.SendTraceTag(3454934u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.Medium, "Error when read error response. Exception={0}", new object[]
                    {
                        ex2
                    });
                }
            }
            return null;
        }

        private static Exception GetSoapException(XDocument xdoc)
        {
            if (IdcrlUtility.GetElementAtPath(xdoc.Root, new string[]
            {
                "{http://www.w3.org/2003/05/soap-envelope}Body",
                "{http://www.w3.org/2003/05/soap-envelope}Fault"
            }) == null)
            {
                return null;
            }
            XElement elementAtPath = IdcrlUtility.GetElementAtPath(xdoc.Root, new string[]
            {
                "{http://www.w3.org/2003/05/soap-envelope}Body",
                "{http://www.w3.org/2003/05/soap-envelope}Fault",
                "{http://www.w3.org/2003/05/soap-envelope}Code",
                "{http://www.w3.org/2003/05/soap-envelope}Subcode",
                "{http://www.w3.org/2003/05/soap-envelope}Value"
            });
            XElement elementAtPath2 = IdcrlUtility.GetElementAtPath(xdoc.Root, new string[]
            {
                "{http://www.w3.org/2003/05/soap-envelope}Body",
                "{http://www.w3.org/2003/05/soap-envelope}Fault",
                "{http://www.w3.org/2003/05/soap-envelope}Detail",
                "{http://schemas.microsoft.com/Passport/SoapServices/SOAPFault}error",
                "{http://schemas.microsoft.com/Passport/SoapServices/SOAPFault}value"
            });
            XElement elementAtPath3 = IdcrlUtility.GetElementAtPath(xdoc.Root, new string[]
            {
                "{http://www.w3.org/2003/05/soap-envelope}Body",
                "{http://www.w3.org/2003/05/soap-envelope}Fault",
                "{http://www.w3.org/2003/05/soap-envelope}Detail",
                "{http://schemas.microsoft.com/Passport/SoapServices/SOAPFault}error",
                "{http://schemas.microsoft.com/Passport/SoapServices/SOAPFault}internalerror",
                "{http://schemas.microsoft.com/Passport/SoapServices/SOAPFault}text"
            });
            string text = null;
            if (elementAtPath != null)
            {
                text = elementAtPath.Value;
                int num = text.IndexOf(':');
                if (num >= 0)
                {
                    text = text.Substring(num + 1);
                }
            }
            string text2 = null;
            if (elementAtPath2 != null)
            {
                text2 = elementAtPath2.Value;
            }
            string text3 = null;
            if (elementAtPath3 != null)
            {
                text3 = elementAtPath3.Value;
            }
            logger.SendTraceTag(3454935u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.High, "PassportErrorCode={0}, PassportDetailCode={1}, PassportErrorText={2}", new object[]
            {
                text,
                text2,
                text3
            });
            int num2;
            long num3;
            if (string.IsNullOrEmpty(text2))
            {
                num2 = MapPartnerSoapFault(text);
            }
            else if (text2.StartsWith("0x", StringComparison.OrdinalIgnoreCase) && long.TryParse(text2.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num3) || long.TryParse(text2, NumberStyles.Integer, CultureInfo.InvariantCulture, out num3))
            {
                num2 = (int)num3;

                if (string.Compare(text3, "Login requires strong authentication.\r\n", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    num2 = IdcrlErrorCodes.PPCRL_REQUEST_E_STRONG_PASSWORD_APPLIED;
                }
                //else if (string.Compare(text, "FailedAuthentication", StringComparison.OrdinalIgnoreCase) == 0)
                //{
                //	num2 = ((num2 == -2147186639) ? num2 : -2147186655);
                //}
            }
            else
            {
                num2 = -2147186656;
            }
            return CreateIdcrlException(num2, text3);
        }

        private static int MapPartnerSoapFault(string code)
        {
            int result;
            if (partnerSoapErrorMap.TryGetValue(code, out result))
            {
                return result;
            }
            return -2147186451;
        }

        private static Exception CreateIdcrlException(int hr)
        {
            return CreateIdcrlException(hr, null);
        }

        private static Exception CreateIdcrlException(int hr, string details)
        {
            string message;
            if (!IdcrlErrorCodes.TryGetErrorStringId(hr, out message))
            {
                message = "PPCRL_REQUEST_E_UNKNOWN";
            }
  
            if (!string.IsNullOrEmpty(details))
            {
                message = string.Concat(message, Environment.NewLine, details);
            }

            return new AuthenticationIdclrException(message, hr);
        }

        private void InitFederationProviderInfoForUser(string username)
        {
            int num = username.IndexOf('@');
            if (num < 0 || num == username.Length - 1)
            {
                throw new ArgumentException(Mirosoft365ApiErrorMessage.InvalidEmailFormat(username));
            }
            userDomainName = username.Substring(num + 1);

            if (environment == AveAzureEnvironment.None)
            {
                FederationProviderInfo federationProviderInfo = GetFederationProviderInfo(userDomainName);
                if (federationProviderInfo != null)
                {
                    userRealmServiceUrl = federationProviderInfo.UserRealmServiceUrl;
                    securityTokenServiceUrl = federationProviderInfo.SecurityTokenServiceUrl;
                    federationTokenIssuer = federationProviderInfo.FederationTokenIssuer;
                }
            }
        }

        private void OutputDetails(string username, string password, string serviceTarget, string servicePolicy)
        {
            logger.SendTraceTag(3454936u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.Verbose,
                "UserName={0}, UserRealmServiceUrl={1}, SecurityTokenServiceUrl={2}, FederationTokenIssuer={3}, Environment={4}, HashCode={5}, ServiceTarget={6}, ServicePolicy={7}", new object[]
            {
                username,
                userRealmServiceUrl,
                securityTokenServiceUrl,
                federationTokenIssuer,
                environment,
                password?.GetHashCode(),
                serviceTarget,
                servicePolicy
            });
        }

        private FederationProviderInfo GetFederationProviderInfo(string domainname)
        {
            FederationProviderInfo federationProviderInfo;
            if (federationProviderInfoCache.TryGetValue(domainname, out federationProviderInfo))
            {
                logger.SendTraceTag(3454937u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.Verbose, "Get federation provider information for {0} from cache. UserRealmServiceUrl={1}, SecurityTokenServiceUrl={2}, FederationTokenIssuer={3}", new object[]
                {
                    domainname,
                    federationProviderInfo == null ? null : federationProviderInfo.UserRealmServiceUrl,
                    federationProviderInfo == null ? null : federationProviderInfo.SecurityTokenServiceUrl,
                    federationProviderInfo == null ? null : federationProviderInfo.FederationTokenIssuer
                });
                return federationProviderInfo;
            }
            federationProviderInfo = RequestFederationProviderInfo(domainname);
            federationProviderInfoCache.Put(domainname, federationProviderInfo);
            logger.SendTraceTag(3454938u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.Medium, "Get federation provider information for {0} and put it in cache. UserRealmServcieUrl={1}, SecurityTokenServiceUrl={2}, FederationTokenIssuer={3}", new object[]
            {
                domainname,
                federationProviderInfo == null ? null : federationProviderInfo.UserRealmServiceUrl,
                federationProviderInfo == null ? null : federationProviderInfo.SecurityTokenServiceUrl,
                federationProviderInfo == null ? null : federationProviderInfo.FederationTokenIssuer
            });
            return federationProviderInfo;
        }

        private FederationProviderInfo RequestFederationProviderInfo(string domainname)
        {
            int num;
            while ((num = domainname.IndexOf('.')) > 0)
            {
                string text = string.Format(CultureInfo.InvariantCulture, IdcrlMessageConstants.FPUrlFullUrlFormat, new object[]
                {
                    domainname
                });
                try
                {
                    XDocument xdoc = DoGet(text);
                    string fpDomainName = ParseFPDomainName(xdoc);
                    if (!string.IsNullOrEmpty(fpDomainName))
                    {
                        text = string.Format(CultureInfo.InvariantCulture, IdcrlMessageConstants.FPListFullUrlFormat, new object[]
                        {
                        domainname
                        });
                        xdoc = DoGet(text);
                        return ParseFederationProviderInfo(xdoc, fpDomainName);
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.NameResolutionFailure)
                    {
                        logger.SendTraceTag(3454939u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.Medium, "Exception when request {0}. Exception={1}", new object[]
                        {
                            text,
                            ex.Message
                        });
                    }
                    else
                    {
                        logger.SendTraceTag(3454939u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.Medium, "Exception when request {0}. Exception={1}", new object[]
                        {
                            text,
                            ex
                        });
                    }
                }
                catch (XmlException e)
                {
                    logger.SendTraceTag(3454939u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.Medium, "Exception when request {0}. Exception={1}", new object[]
                    {
                        text,
                        e
                    });
                }
                domainname = domainname.Substring(num + 1);
            }
            return null;
        }

        private static string ParseFPDomainName(XDocument xdoc)
        {
            XElement elementAtPath = IdcrlUtility.GetElementAtPath(xdoc.Root, new string[]
            {
                "FPDOMAINNAME"
            });
            if (elementAtPath == null)
            {
                return null;
                //ClientULS.SendTraceTag(3454940u, ClientTraceCategory.Authentication, ClientTraceLevel.High, "Cannot find FPDOMAINNAME element", new object[0]);
                //throw IdcrlAuth.CreateIdcrlException(-2147186646);
            }
            return elementAtPath.Value;
        }

        private static FederationProviderInfo ParseFederationProviderInfo(XDocument xdoc, string fpDomainName)
        {
            foreach (XElement current in xdoc.Root.Elements("FP"))
            {
                if (current.Attribute("DomainName") != null && string.Equals(current.Attribute("DomainName").Value, fpDomainName, StringComparison.OrdinalIgnoreCase))
                {
                    XElement elementAtPath = IdcrlUtility.GetElementAtPath(current, new string[]
                    {
                        "URL",
                        "GETUSERREALM"
                    });
                    XElement elementAtPath2 = IdcrlUtility.GetElementAtPath(current, new string[]
                    {
                        "URL",
                        "RST2"
                    });
                    XElement elementAtPath3 = IdcrlUtility.GetElementAtPath(current, new string[]
                    {
                        "URL",
                        "ENTITYID"
                    });
                    if (elementAtPath == null || elementAtPath2 == null || elementAtPath3 == null)
                    {
                        logger.SendTraceTag(3454941u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.High, "Cannot get the user realm service url or security token service url for federation provider {0}", new object[]
                        {
                            fpDomainName
                        });
                        throw CreateIdcrlException(-2147186646);
                    }
                    logger.SendTraceTag(3454942u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.High, "Find federation provider information for federation provider domain name {0}. UserRealmServiceUrl={1}, SecurityTokenServiceUrl={2}, FederationTokenIssuer={3}", new object[]
                    {
                        fpDomainName,
                        elementAtPath.Value,
                        elementAtPath2.Value,
                        elementAtPath3.Value
                    });
                    return new FederationProviderInfo
                    {
                        UserRealmServiceUrl = elementAtPath.Value,
                        SecurityTokenServiceUrl = elementAtPath2.Value,
                        FederationTokenIssuer = elementAtPath3.Value
                    };
                }
            }
            logger.SendTraceTag(3454943u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.High, "Cannot find federation provider information for federation domain {0}", new object[]
            {
                fpDomainName
            });
            throw CreateIdcrlException(-2147186646);
        }

        private XDocument DoGet(string url)
        {
            var httpWebRequest = Microsoft365Configuration.CommonConfiguration.WebRequestProvider.CreateRequest(new Uri(url));
            httpWebRequest.UserAgent = Microsoft365Configuration.CommonConfiguration.UserAgent;
            httpWebRequest.Method = "GET";
            //ClientULS.SendTraceTag(3454944u, ClientTraceCategory.Authentication, ClientTraceLevel.Verbose, "Sending GET request to {0}", new object[]
            //{
            //	url
            //});
            if (executingWebRequest != null)
            {
                executingWebRequest(this, new SPOCredentialsWebRequestEventArgs(httpWebRequest));
            }
            HttpWebResponse httpWebResponse = httpWebRequest.WebRequest.GetResponseByHttpClient(null, "Authentication", RestClientFactory.DefaultStrategies);
            if (httpWebResponse == null)
            {
                logger.SendTraceTag(3454945u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.High, "Unexpected response for GET request to URL {0}", new object[]
                {
                    url
                });
                throw new InvalidOperationException();
            }
            XDocument result;
            using (httpWebResponse)
            {
                using (TextReader textReader = new StreamReader(httpWebResponse.GetResponseStream()))
                {
                    string text = textReader.ReadToEnd();
                    using (XmlReader xmlReader = XmlReader.Create(new StringReader(text)))
                    {
                        XDocument xDocument = XDocument.Load(xmlReader);
                        result = xDocument;
                    }
                    logger.SendTraceTag(3454946u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.Verbose, "RequestUrl={0}, StatusCode={1}, ResponseText={2}", new object[]
                    {
                        url,
                        (int)httpWebResponse.StatusCode,
                        text
                    });
                }
            }
            return result;
        }
    }
}