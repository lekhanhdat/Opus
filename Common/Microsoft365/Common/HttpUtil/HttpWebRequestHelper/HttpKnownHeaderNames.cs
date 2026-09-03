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
namespace Microsoft365.Common.HttpUtil
{
    using System;

    internal static partial class HttpKnownHeaderNames
    {
        // When adding a new constant, add it to HttpKnownHeaderNames.TryGetHeaderName.cs as well.

        public const string Accept = "Accept";
        public const string AcceptCharset = "Accept-Charset";
        public const string AcceptEncoding = "Accept-Encoding";
        public const string AcceptLanguage = "Accept-Language";
        public const string AcceptPatch = "Accept-Patch";
        public const string AcceptRanges = "Accept-Ranges";
        public const string AccessControlAllowCredentials = "Access-Control-Allow-Credentials";
        public const string AccessControlAllowHeaders = "Access-Control-Allow-Headers";
        public const string AccessControlAllowMethods = "Access-Control-Allow-Methods";
        public const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
        public const string AccessControlExposeHeaders = "Access-Control-Expose-Headers";
        public const string AccessControlMaxAge = "Access-Control-Max-Age";
        public const string Age = "Age";
        public const string Allow = "Allow";
        public const string AltSvc = "Alt-Svc";
        public const string Authorization = "Authorization";
        public const string CacheControl = "Cache-Control";
        public const string Connection = "Connection";
        public const string ContentDisposition = "Content-Disposition";
        public const string ContentEncoding = "Content-Encoding";
        public const string ContentLanguage = "Content-Language";
        public const string ContentLength = "Content-Length";
        public const string ContentLocation = "Content-Location";
        public const string ContentMD5 = "Content-MD5";
        public const string ContentRange = "Content-Range";
        public const string ContentSecurityPolicy = "Content-Security-Policy";
        public const string ContentType = "Content-Type";
        public const string Cookie = "Cookie";
        public const string Cookie2 = "Cookie2";
        public const string Date = "Date";
        public const string ETag = "ETag";
        public const string Expect = "Expect";
        public const string Expires = "Expires";
        public const string From = "From";
        public const string Host = "Host";
        public const string IfMatch = "If-Match";
        public const string IfModifiedSince = "If-Modified-Since";
        public const string IfNoneMatch = "If-None-Match";
        public const string IfRange = "If-Range";
        public const string IfUnmodifiedSince = "If-Unmodified-Since";
        public const string KeepAlive = "Keep-Alive";
        public const string LastModified = "Last-Modified";
        public const string Link = "Link";
        public const string Location = "Location";
        public const string MaxForwards = "Max-Forwards";
        public const string Origin = "Origin";
        public const string P3P = "P3P";
        public const string Pragma = "Pragma";
        public const string ProxyAuthenticate = "Proxy-Authenticate";
        public const string ProxyAuthorization = "Proxy-Authorization";
        public const string ProxyConnection = "Proxy-Connection";
        public const string PublicKeyPins = "Public-Key-Pins";
        public const string Range = "Range";
        public const string Referer = "Referer"; // NB: The spelling-mistake "Referer" for "Referrer" must be matched.
        public const string RetryAfter = "Retry-After";
        public const string SecWebSocketAccept = "Sec-WebSocket-Accept";
        public const string SecWebSocketExtensions = "Sec-WebSocket-Extensions";
        public const string SecWebSocketKey = "Sec-WebSocket-Key";
        public const string SecWebSocketProtocol = "Sec-WebSocket-Protocol";
        public const string SecWebSocketVersion = "Sec-WebSocket-Version";
        public const string Server = "Server";
        public const string SetCookie = "Set-Cookie";
        public const string SetCookie2 = "Set-Cookie2";
        public const string StrictTransportSecurity = "Strict-Transport-Security";
        public const string TE = "TE";
        public const string TSV = "TSV";
        public const string Trailer = "Trailer";
        public const string TransferEncoding = "Transfer-Encoding";
        public const string Upgrade = "Upgrade";
        public const string UpgradeInsecureRequests = "Upgrade-Insecure-Requests";
        public const string UserAgent = "User-Agent";
        public const string Vary = "Vary";
        public const string Via = "Via";
        public const string WWWAuthenticate = "WWW-Authenticate";
        public const string Warning = "Warning";
        public const string XAspNetVersion = "X-AspNet-Version";
        public const string XContentDuration = "X-Content-Duration";
        public const string XContentTypeOptions = "X-Content-Type-Options";
        public const string XFrameOptions = "X-Frame-Options";
        public const string XMSEdgeRef = "X-MSEdge-Ref";
        public const string XPoweredBy = "X-Powered-By";
        public const string XRequestID = "X-Request-ID";
        public const string XUACompatible = "X-UA-Compatible";

        private static readonly string[] s_wellKnownContentHeaders = {
            HttpKnownHeaderNames.ContentDisposition,
            HttpKnownHeaderNames.ContentEncoding,
            HttpKnownHeaderNames.ContentLanguage,
            HttpKnownHeaderNames.ContentLength,
            HttpKnownHeaderNames.ContentLocation,
            HttpKnownHeaderNames.ContentMD5,
            HttpKnownHeaderNames.ContentRange,
            HttpKnownHeaderNames.ContentType,
            HttpKnownHeaderNames.Expires,
            HttpKnownHeaderNames.LastModified
        };

        internal static bool IsWellKnownContentHeader(string header)
        {
            foreach (string contentHeaderName in s_wellKnownContentHeaders)
            {
                if (string.Equals(header, contentHeaderName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}