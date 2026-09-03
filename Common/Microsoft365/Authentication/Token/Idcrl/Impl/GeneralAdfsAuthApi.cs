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
namespace Microsoft365.Authentication.Token.Idclr.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft365.Authentication;
    using Microsoft365.Authentication.Token.Idclr;
    using Microsoft365.Common.HttpUtil;
    using Microsoft365.Common.Logger;
    using Microsoft365.Configuration;

    class GeneralAdfsAuthApi : IFederationAuthApi
    {
        private static IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(GeneralAdfsAuthApi));
        protected EventHandler<SPOCredentialsWebRequestEventArgs> m_executingWebRequest;

        public string GetTicket(string stsAuthUrl, string username, string password, string federationTokenIssuer)
        {
            string body = string.Format(CultureInfo.InvariantCulture, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<s:Envelope xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\" xmlns:saml=\"urn:oasis:names:tc:SAML:1.0:assertion\" xmlns:wsp=\"http://schemas.xmlsoap.org/ws/2004/09/policy\" xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" xmlns:wsa=\"http://www.w3.org/2005/08/addressing\" xmlns:wssc=\"http://schemas.xmlsoap.org/ws/2005/02/sc\" xmlns:wst=\"http://schemas.xmlsoap.org/ws/2005/02/trust\">\r\n    <s:Header>\r\n        <wsa:Action s:mustUnderstand=\"1\">http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue</wsa:Action>\r\n        <wsa:To s:mustUnderstand=\"1\">{0}</wsa:To>\r\n        <wsa:MessageID>{1}</wsa:MessageID>\r\n        <ps:AuthInfo xmlns:ps=\"http://schemas.microsoft.com/Passport/SoapServices/PPCRL\" Id=\"PPAuthInfo\">\r\n            <ps:HostingApp>Managed IDCRL</ps:HostingApp>\r\n            <ps:BinaryVersion>6</ps:BinaryVersion>\r\n            <ps:UIVersion>1</ps:UIVersion>\r\n            <ps:Cookies></ps:Cookies>\r\n            <ps:RequestParams>AQAAAAIAAABsYwQAAAAxMDMz</ps:RequestParams>\r\n        </ps:AuthInfo>\r\n        <wsse:Security>\r\n            <wsse:UsernameToken wsu:Id=\"user\">\r\n                <wsse:Username>{2}</wsse:Username>\r\n                <wsse:Password>{3}</wsse:Password>\r\n            </wsse:UsernameToken>\r\n            <wsu:Timestamp Id=\"Timestamp\">\r\n                <wsu:Created>{4}</wsu:Created>\r\n                <wsu:Expires>{5}</wsu:Expires>\r\n            </wsu:Timestamp>\r\n        </wsse:Security>\r\n    </s:Header>\r\n    <s:Body>\r\n        <wst:RequestSecurityToken Id=\"RST0\">\r\n            <wst:RequestType>http://schemas.xmlsoap.org/ws/2005/02/trust/Issue</wst:RequestType>\r\n            <wsp:AppliesTo>\r\n                <wsa:EndpointReference>\r\n                    <wsa:Address>{6}</wsa:Address>\r\n                </wsa:EndpointReference>\r\n            </wsp:AppliesTo>\r\n            <wst:KeyType>http://schemas.xmlsoap.org/ws/2005/05/identity/NoProofKey</wst:KeyType>\r\n        </wst:RequestSecurityToken>\r\n    </s:Body>\r\n</s:Envelope>", new object[]
            {
                IdcrlUtility.XmlValueEncode(stsAuthUrl),
                Guid.NewGuid().ToString(),
                IdcrlUtility.XmlValueEncode(username),
                IdcrlUtility.XmlValueEncode(password),
                DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                DateTime.UtcNow.AddMinutes(10.0).ToString("o", CultureInfo.InvariantCulture),
                federationTokenIssuer
            });
            XDocument xDocument = DoPost(stsAuthUrl, "application/soap+xml; charset=utf-8", body, new Func<WebException, Exception>(HandleWebException));
            Exception soapException = GetSoapException(xDocument);
            if (soapException != null)
            {
                logger.SendTraceTag(3454924u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.High, "SOAP error from {0}. Exception={1}", new object[]
                {
                    stsAuthUrl,
                    soapException
                });
                throw soapException;
            }
            XElement elementAtPath = GetElementAtPath(xDocument.Root, new string[]
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
                    stsAuthUrl
                });
                throw CreateIdcrlException(-2147186451);
            }
            return elementAtPath.ToString(SaveOptions.DisableFormatting | SaveOptions.OmitDuplicateNamespaces);
        }

        private XDocument DoPost(string url, string contentType, string body, Func<WebException, Exception> webExceptionHandler)
        {
            var httpWebRequest = Microsoft365Configuration.CommonConfiguration.WebRequestProvider.CreateRequest(new Uri(url));
            httpWebRequest.UserAgent = Microsoft365Configuration.CommonConfiguration.UserAgent;
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = contentType;
            //ClientULS.SendTraceTag(3454928u, ClientTraceCategory.Authentication, ClientTraceLevel.Verbose, "Sending POST request to {0}", new object[]
            //{
            //    url
            //});
            if (m_executingWebRequest != null)
            {
                m_executingWebRequest(this, new SPOCredentialsWebRequestEventArgs(httpWebRequest));
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

        private static (Stream Content,HttpStatusCode Status) GetResponseStream(WebException webException)
        {
            HttpWebResponse httpWebResponse = webException.Response as HttpWebResponse;
            if (httpWebResponse != null && httpWebResponse.ContentType != null && httpWebResponse.ContentType.IndexOf("application/soap+xml", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return (httpWebResponse.GetResponseStream(), httpWebResponse.StatusCode);
            }
            return (null,default);
        }

        private static Exception HandleWebException(WebException webException)
        {
            var result = GetResponseStream(webException);
            if (result.Content == null)
            {
                return null;
            }
            return HandleWebExceptionInternal(result.Content,result.Status);
        }

        private static Exception HandleWebExceptionInternal(Stream stream,HttpStatusCode httpStatusCode)
        {
                try
                {
                    using (TextReader textReader = new StreamReader(stream))
                    {
                        string text = textReader.ReadToEnd();
                        logger.SendTraceTag(3454932u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.High, "StatusCode={0}, ResponseText={1}", new object[]
                        {
                            (int)httpStatusCode,
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
                if (string.Compare(text, "FailedAuthentication", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    num2 = num2 == -2147186639 ? num2 : -2147186655;
                }
            }
            else
            {
                num2 = -2147186656;
            }
            return CreateIdcrlException(num2);
        }

        internal static XElement GetElementAtPath(XElement elem, params string[] paths)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                string expandedName = paths[i];
                if (elem == null)
                {
                    return null;
                }
                elem = elem.Element(XName.Get(expandedName));
            }
            return elem;
        }

        private static Exception CreateIdcrlException(int hr)
        {
            string resourceId;
            if (!IdcrlErrorCodes.TryGetErrorStringId(hr, out resourceId))
            {
                resourceId = "PPCRL_REQUEST_E_UNKNOWN";
            }
            return new AuthenticationIdclrException(resourceId, hr);
        }

        private static int MapPartnerSoapFault(string code)
        {
            int result;
            if (s_partnerSoapErrorMap.TryGetValue(code, out result))
            {
                return result;
            }
            return -2147186451;
        }

        private static Dictionary<string, int> s_partnerSoapErrorMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
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
    }
}