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
namespace Microsoft365.Authentication.Token.Idclr
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Security;
    using System.Security;
    using Microsoft365.Authentication;
    using Microsoft365.Authentication.Extension;
    using Microsoft365.Common.Exception;
    using Microsoft365.Common.HttpUtil;
    using Microsoft365.Common.Logger;
    using Microsoft365.Configuration;

    internal class SPOAuthenticationProvider 
    {
        private class IdcrlHeader
        {
            public string IdcrlType;

            public string ServiceTarget;

            public string ServicePolicy;

            public string Endpoint;
        }

        /// <summary>
        /// Available value is INT-MSO, production
        /// </summary>
		private static IdcrlEnvironment s_idcrlEnvironment = IdcrlEnvironment.Production;
        private static IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(SPOAuthenticationProvider));

        private static string ValidateAndGetSafeCookie(string inputHeader)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(inputHeader, @"[\r\n;,:\""\0]"))
            {
                logger.Warn("Error: The specified header contains ilelgal character, Please provide a valid value.");
                throw new Exception("Invalid cookie");
            }
            return inputHeader;
        }

        public string GetAuthenticationCookie(Uri url, string username, SecureString password, AveAzureEnvironment environment, bool alwaysThrowOnFailure, EventHandler<SPOCredentialsWebRequestEventArgs> executingWebRequest)
        {
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
            IdcrlHeader idcrlHeader = GetIdcrlHeader(url, alwaysThrowOnFailure, executingWebRequest, environment);
            if (idcrlHeader == null)
            {
                logger.SendTraceTag(3991707u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.Medium, "Cannot get IDCRL header for {0}", new object[]
                {
                    url
                });
                if (alwaysThrowOnFailure)
                {
                    throw new AuthenticationRequestException(Mirosoft365ApiErrorMessage.CannotContactSiteFormat(url));
                }
                return null;
            }
            else
            {
                IdcrlAuth idcrlAuth = new IdcrlAuth(s_idcrlEnvironment, environment, executingWebRequest);
                string serviceToken = idcrlAuth.GetServiceToken(username, password.ToPlainString(), idcrlHeader.ServiceTarget, idcrlHeader.ServicePolicy);
                if (!string.IsNullOrEmpty(serviceToken))
                {
                    return ValidateAndGetSafeCookie(GetCookie(url, idcrlHeader.Endpoint, serviceToken, alwaysThrowOnFailure, executingWebRequest));
                }
                logger.SendTraceTag(3991708u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.Medium, "Cannot get IDCRL ticket for username {0}", new object[]
                {
                    username
                });
                if (alwaysThrowOnFailure)
                {
                    throw new AuthenticationIdclrException("Unable to get ticket due to unknown error.");
                }
                return null;
            }
        }

        private string GetCookie(Uri url, string endpoint, string ticket, bool throwIfFail, EventHandler<SPOCredentialsWebRequestEventArgs> executingWebRequest)
        {
            Uri uri = new Uri(url, endpoint);
            
            var httpWebRequest = Microsoft365Configuration.CommonConfiguration.WebRequestProvider.CreateRequest(uri);
            CookieContainer cookieContainer = new CookieContainer();
            httpWebRequest.CookieContainer = cookieContainer;
            httpWebRequest.Headers[HttpRequestHeader.Authorization] = "BPOSIDCRL " + ticket;
            httpWebRequest.Headers["X-IDCRL_ACCEPTED"] = "t";
            if (executingWebRequest != null)
            {
                executingWebRequest(this, new SPOCredentialsWebRequestEventArgs(httpWebRequest));
            }
            //WebResponse response = httpWebRequest.GetResponse() as HttpWebResponse;
            WebResponse response = httpWebRequest.WebRequest.GetResponseByHttpClient(null, "Authentication", RestClientFactory.DefaultStrategies);
            string cookieHeader = response.Headers[HttpResponseHeader.SetCookie]?.Split(';').FirstOrDefault();
            if (string.IsNullOrWhiteSpace(cookieHeader))
            {
                cookieHeader = cookieContainer.GetCookieHeader(uri);
            }
            if (string.IsNullOrWhiteSpace(cookieHeader))
            {
                UriBuilder uriBuilder = new UriBuilder(uri);
                uriBuilder.Host = httpWebRequest.Host;
                logger.SendTraceTag(5825556u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.Verbose, "Try get cookie using {0}", new object[]
                {
                    uriBuilder.ToString()
                });
                cookieHeader = cookieContainer.GetCookieHeader(uriBuilder.Uri);
                logger.SendTraceTag(5825557u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.Medium, "Get cookie using {0} and cookie value is {0}", new object[]
                {
                    uriBuilder.ToString(),
                    cookieHeader
                });
            }
            if (response != null)
            {
                response.Close();
            }
            if (string.IsNullOrWhiteSpace(cookieHeader))
            {
                logger.SendTraceTag(3991709u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.Medium, "Cannot get cookie for {0}", new object[]
                {
                    url
                });
                if (throwIfFail)
                {
                    throw new AuthenticationRequestException(Mirosoft365ApiErrorMessage.CannotGetCookieFormat(url));
                }
            }
            return cookieHeader;
        }

        private IdcrlHeader GetIdcrlHeader(Uri url, bool alwaysThrowOnFailure, EventHandler<SPOCredentialsWebRequestEventArgs> executingWebRequest, AveAzureEnvironment environment)
        {
            var httpWebRequest = Microsoft365Configuration.CommonConfiguration.WebRequestProvider.CreateRequest(url);
            httpWebRequest.Headers["X-IDCRL_ACCEPTED"] = "t";
            httpWebRequest.AuthenticationLevel = AuthenticationLevel.None;
            if (executingWebRequest != null)
            {
                executingWebRequest(this, new SPOCredentialsWebRequestEventArgs(httpWebRequest));
            }
            HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebResponse = httpWebRequest.WebRequest.GetResponseByHttpClient(null, "Authentication", RestClientFactory.DefaultStrategies);
            }
            catch (WebException ex)
            {
                logger.SendTraceTag(3991710u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.Verbose, "Exception in request. Url={0}, WebException={1}", new object[]
                {
                    url,
                    ex.Message
                });
                httpWebResponse = ex.Response as HttpWebResponse;
                if (alwaysThrowOnFailure && (httpWebResponse == null || httpWebResponse.StatusCode != HttpStatusCode.Forbidden && httpWebResponse.StatusCode != HttpStatusCode.Unauthorized))
                {
                    throw;
                }
            }
            if (httpWebResponse != null)
            {
                string webResponseHeader = IdcrlUtility.GetWebResponseHeader(httpWebResponse);
                HttpStatusCode statusCode = httpWebResponse.StatusCode;
                logger.SendTraceTag(4839637u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.Verbose, "Response.StatusCode={0}, Headers={1}", new object[]
                {
                    statusCode,
                    webResponseHeader
                });
                string text = httpWebResponse.Headers["X-IDCRL_AUTH_PARAMS_V1"];
                if (string.IsNullOrEmpty(text))
                {
                    text = httpWebResponse.Headers[HttpResponseHeader.WwwAuthenticate];
                }
                httpWebResponse.Close();
                if (!string.IsNullOrEmpty(text))
                {
                    logger.SendTraceTag(3991712u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.Verbose, "IdcrlHeader={0}", new object[]
                    {
                    text
                    });
                }
                else
                {
                    switch (environment)//We do not know the IDCRL header of Azure Germany so far.
                    {
                        case AveAzureEnvironment.USGovernment:
                            text = "IDCRL Type=\"BPOSIDCRL\", EndPoint=\"/_vti_bin/idcrl.svc/\", RootDomain=\"sharepoint.us\", Policy=\"MBI\"";
                            break;
                        case AveAzureEnvironment.USGovernmentDOD:
                            text = "IDCRL Type=\"BPOSIDCRL\", EndPoint=\"/_vti_bin/idcrl.svc/\", RootDomain=\"sharepoint-mil.us\", Policy=\"MBI\"";
                            break;
                        case AveAzureEnvironment.AzureChinaCloud:
                            text = "IDCRL Type=\"BPOSIDCRL\", EndPoint=\"/_vti_bin/idcrl.svc/\", RootDomain=\"sharepoint.cn\", Policy=\"MBI\"";
                            break;
                        default:
                            text = "IDCRL Type=\"BPOSIDCRL\", EndPoint=\"/_vti_bin/idcrl.svc/\", RootDomain=\"sharepoint.com\", Policy=\"MBI\"";
                            break;
                    }

                    logger.SendTraceTag(3991712u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.High, "Using Default IdcrlHeader={0}", new object[]
                    {
                    text
                    });
                }
                return ParseIdcrlHeader(text, url, statusCode, webResponseHeader, alwaysThrowOnFailure);
            }
            logger.SendTraceTag(3991711u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.High, "Cannot get response for request to {0}", new object[]
            {
                url
            });
            if (alwaysThrowOnFailure)
            {
                throw new AuthenticationRequestException(Mirosoft365ApiErrorMessage.CannotContactSiteFormat(url));
            }
            return null;
        }

        private IdcrlHeader ParseIdcrlHeader(string headerValue, Uri url, HttpStatusCode statusCode, string allResponseHeaders, bool alwaysThrowOnFailure)
        {
            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                IdcrlHeader idcrlHeader = new IdcrlHeader();
                string[] array = headerValue.Split(new char[]
                {
                    ','
                });
                for (int i = 0; i < array.Length; i++)
                {
                    string text = array[i];
                    string text2 = text.Trim();
                    string[] array2 = text2.Split(new char[]
                    {
                        '='
                    });
                    if (array2.Length == 2)
                    {
                        array2[0] = array2[0].Trim().ToUpperInvariant();
                        array2[1] = array2[1].Trim(new char[]
                        {
                            ' ',
                            '"'
                        });
                        string a;
                        if ((a = array2[0]) != null)
                        {
                            if (!(a == "IDCRL TYPE"))
                            {
                                if (!(a == "ENDPOINT"))
                                {
                                    if (!(a == "ROOTDOMAIN"))
                                    {
                                        if (a == "POLICY")
                                        {
                                            idcrlHeader.ServicePolicy = array2[1];
                                        }
                                    }
                                    else
                                    {
                                        idcrlHeader.ServiceTarget = array2[1];
                                    }
                                }
                                else
                                {
                                    idcrlHeader.Endpoint = array2[1];
                                }
                            }
                            else
                            {
                                idcrlHeader.IdcrlType = array2[1];
                            }
                        }
                    }
                }
                if (idcrlHeader.IdcrlType != "BPOSIDCRL" || string.IsNullOrEmpty(idcrlHeader.ServicePolicy) || string.IsNullOrEmpty(idcrlHeader.ServiceTarget) || string.IsNullOrEmpty(idcrlHeader.Endpoint))
                {
                    logger.SendTraceTag(3991714u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.Medium, "Cannot extract required information from IDCRL header. Header={0}, IdcrlType={1}, ServicePolicy={2}, ServiceTarget={3}, Endpoint={4}", new object[]
                    {
                        headerValue,
                        idcrlHeader.IdcrlType,
                        idcrlHeader.ServicePolicy,
                        idcrlHeader.ServiceTarget,
                        idcrlHeader.Endpoint
                    });
                    if (alwaysThrowOnFailure)
                    {
                         throw new AuthenticationRequestException(Mirosoft365ApiErrorMessage.InvalidIdcrlHeaderFormat( new object[]
                        {
                            url.OriginalString,
                            headerValue,
                            statusCode,
                            allResponseHeaders
                        }));
                    }
                    idcrlHeader = null;
                }
                return idcrlHeader;
            }
            logger.SendTraceTag(3991713u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.Medium, "IDCRL header value is empty", new object[0]);
            if (alwaysThrowOnFailure)
            {
                throw new NotSupportedException(Mirosoft365ApiErrorMessage.SharePointClientCredentialsNotSupportedFormat(new object[]
                {
                    url.OriginalString,
                    statusCode,
                    allResponseHeaders
                }));
            }
            return null;
        }

        //private static string FromSecureString(SecureString value)
        //{
        //	IntPtr intPtr = Marshal.SecureStringToBSTR(value);
        //	if (intPtr == IntPtr.Zero)
        //	{
        //		return string.Empty;
        //	}
        //	string result;
        //	try
        //	{
        //		result = Marshal.PtrToStringBSTR(intPtr);
        //	}
        //	finally
        //	{
        //		Marshal.FreeBSTR(intPtr);
        //	}
        //	return result;
        //}

        //internal bool DoesSupportIdcrl(Uri uri)
        //{
        //	if (uri == null)
        //	{
        //		throw new ArgumentNullException("uri");
        //	}
        //	return this.GetIdcrlHeader(uri, true, null) != null;
        //}

        public string GetADToken(string serviceTarget, string policy, string username, string password, AveAzureEnvironment environment, EventHandler<SPOCredentialsWebRequestEventArgs> executingWebRequest)
        {
            IdcrlAuth auth = new IdcrlAuth(s_idcrlEnvironment, environment, executingWebRequest);
            return auth.GetServiceToken(username, password, serviceTarget, policy);
        }

        public UserRealmInfo GetUserRealmInfo(string username, AveAzureEnvironment environment, EventHandler<SPOCredentialsWebRequestEventArgs> executingWebRequest)
        {
            IdcrlAuth auth = new IdcrlAuth(s_idcrlEnvironment, environment, executingWebRequest);
            return auth.GetUserRealmInfo(username);
        }
    }
}