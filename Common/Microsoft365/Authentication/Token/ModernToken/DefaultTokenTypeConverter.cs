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
namespace Microsoft365.Authentication.Token.Modern
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;

    using Microsoft365.Authentication.Token.Idclr;
    using Microsoft365.Authentication.Token.ModernToken;
    using Microsoft365.Common.Exception;
    using Microsoft365.Common.HttpUtil;
    using Microsoft365.Common.Logger;
    using Microsoft365.Configuration;

    public class DefaultTokenTypeConverter : ITokenTypeConverter
    {
        private static IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(DefaultTokenTypeConverter));
        private const string convertTokenEndpoint = "/_api/SP.OAuth.NativeClient/Authenticate";
        static DefaultTokenTypeConverter()
        {
            Instance = new DefaultTokenTypeConverter();
        }
        public static ITokenTypeConverter Instance { get; private set; }
        public string ConvertBearToCookie(Uri accessSharePointUrl, string bearTokenWithHeader, bool alwaysThrowOnFailure, EventHandler<SPOCredentialsWebRequestEventArgs> executingWebRequest)
        {
            if (!string.IsNullOrEmpty(bearTokenWithHeader))
            {
                return GetCookie(accessSharePointUrl, convertTokenEndpoint, bearTokenWithHeader, alwaysThrowOnFailure, executingWebRequest);
            }
            logger.SendTraceTag(3991708u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.Medium, "Cannot get IDCRL cookie from bear token.{0}", new object[]
            {
                    accessSharePointUrl
            });
            if (alwaysThrowOnFailure)
            {
                throw new AuthenticationIdclrException(Mirosoft365ApiErrorMessage.PPCRL_REQUEST_E_UNKNOWNFormat()); 
            }
            return null;
        }

        private string GetCookie(Uri url, string endpoint, string token, bool throwIfFail, EventHandler<SPOCredentialsWebRequestEventArgs> executingWebRequest)
        {
            logger.SendTraceTag(5825556u, AuthenticationTraceCategory.Authentication, AuthenticationTraceLevel.Verbose, "modern mode,The url is  {0}", new object[]
                {
                    url.ToString()
                });
            var convertRequestSiteUrl = AppSiteAuthenticationConvertUrlCache.GetMappedUrl(url.ToString());
            Uri uri = new Uri(convertRequestSiteUrl.TrimEnd('/') + endpoint);
            logger.Info($"[AppWebUrlMapping]Get Cookie With Request Url:{uri},OriginalUrl:{url}");

            //using (var client = RestClientFactory.CreateSharePointRestClient("Authentication"))
            //{
            //    var message = new HttpRequestMessage(HttpMethod.Post, uri);
            //}

       
            var httpWebRequest = Microsoft365Configuration.CommonConfiguration.WebRequestProvider.CreateRequest(uri);
            CookieContainer cookieContainer = new CookieContainer();
            httpWebRequest.CookieContainer = cookieContainer;
            httpWebRequest.Headers[HttpRequestHeader.Authorization] = token;
            httpWebRequest.Headers["X-IDCRL_ACCEPTED"] = "t";
            //httpWebRequest.Headers["X-FeatureVersion"] = "2";
            httpWebRequest.ContentLength = 0;
            //httpWebRequest.UserAgent = "Microsoft Office Core Storage Infrastructure/2.0";
            httpWebRequest.Method = "Post";

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
                    throw new AuthenticationRequestException(Mirosoft365ApiErrorMessage.CannotGetCookieFormat(new object[]
                    {
                        url
                    }));
                }
            }
            return cookieHeader;
        }
    }
}