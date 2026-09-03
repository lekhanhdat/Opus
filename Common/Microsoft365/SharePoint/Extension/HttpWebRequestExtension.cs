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
namespace Microsoft365.SharePoint.Extension
{
    using System;
    using System.Net;
    using Microsoft365.Authentication;
    using Microsoft365.Common.Http;
    using Microsoft365.Common.Logger;

    public static class HttpWebRequestExtension
    {
        private static IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(HttpWebRequestExtension));
        /// <summary>
        /// set token provider with request digest info
        /// </summary>
        /// <param name="request"></param>
        /// <param name="webFullUrl"></param>
        /// <param name="tokenProvider"></param>
        public static void SetTokenProvider(this WebRequest request, string webFullUrl, ITokenProvider tokenProvider)
        {
            request.SetTokenProvider(webFullUrl, tokenProvider, true);
        }

        public static void SetTokenProvider(this WebRequest request, string webFullUrl, ITokenProvider tokenProvider, bool provideRequestDigest)
        {
            if (provideRequestDigest)
            {
                var digestInfo = SharePointContext.GetFormDigestProvider().GetFormDigest(webFullUrl, tokenProvider);
                if (digestInfo == null || digestInfo.DigestValue == null)
                {
                    if (tokenProvider != null && tokenProvider.TokenType != TokenType.Bearer)
                    {
                        logger.Error("[SetTokenProvider]FormDigestInfo is null. Context.Url:{0}", webFullUrl);
                    }
                }
                else
                {
                    request.Headers["X-RequestDigest"] = digestInfo.DigestValue;
                }
            }

            if (tokenProvider?.TokenType == TokenType.IDCLR)
            {
                request.Headers["X-FORMS_BASED_AUTH_ACCEPTED"] = "f";

                request.Headers[HttpRequestHeader.Cookie] = tokenProvider?.GetToken(new Uri(webFullUrl));
            }
            else
            {
                request.Headers[HttpRequestHeader.Authorization] = tokenProvider?.GetToken(new Uri(webFullUrl));
            }
        }


        public static void SetToken(this IHttpWebRequest request, string webFullUrl, ITokenProvider tokenProvider, bool provideRequestDigest)
        {
            if (provideRequestDigest)
            {
                var digestInfo = SharePointContext.GetFormDigestProvider().GetFormDigest(webFullUrl, tokenProvider);

                if (digestInfo == null || digestInfo.DigestValue == null)
                {
                    logger.Error("[SetTokenProvider]FormDigestInfo is null. Context.Url:{0}", webFullUrl);
                }
                else
                {
                    request.Headers["X-RequestDigest"] = digestInfo.DigestValue;
                }
            }

            if (tokenProvider.TokenType == TokenType.IDCLR)
            {
                request.Headers["X-FORMS_BASED_AUTH_ACCEPTED"] = "f";

                request.Headers[HttpRequestHeader.Cookie] = tokenProvider.GetToken(new Uri(webFullUrl));
            }
            else
            {
                request.Headers[HttpRequestHeader.Authorization] = tokenProvider.GetToken(new Uri(webFullUrl));
            }
        }
    }
}