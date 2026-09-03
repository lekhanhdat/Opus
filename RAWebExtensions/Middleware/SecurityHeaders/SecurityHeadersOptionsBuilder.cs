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
using Microsoft.AspNetCore.Http;

namespace AvePoint.RA.Web.Extentions.Middleware.SecurityHeaders
{
    public class SecurityHeadersOptionsBuilder
    {
        private HashSet<string> noCacheWhiteList = new HashSet<string>();
        private SecurityHeadersPolicy policy = new SecurityHeadersPolicy();

        public SecurityHeadersPolicy Build()
        {
            return policy;
        }

        // white list path will use cache
        public SecurityHeadersOptionsBuilder AddNoCacheWhiteList(params string[] whiteList)
        {
            if (whiteList != null)
            {
                foreach (var item in whiteList)
                {
                    noCacheWhiteList.Add(item.ToLowerInvariant());
                }
            }
            
            return this;
        }

        public SecurityHeadersOptionsBuilder RemoveCustomizedHeader(string header)
        {
            policy.AddHandler((headers, ctx) => headers.Remove(header));
            return this;
        }

        public SecurityHeadersOptionsBuilder AddAccessControlAllowOriginHeader(string url = null)
        {
            policy.AddHandler((headers, ctx) => headers["Access-Control-Allow-Origin"] = url ?? "https://*.avepointonlineservices.com");
            return this;
        }

        public SecurityHeadersOptionsBuilder AddPragmaNoCacheHeader()
        {
            policy.AddHandler((headers, ctx) =>
            {
                if (!IsInNoCacheWhiteList(ctx))
                {
                    headers["Pragma"] = "no-cache";
                }
            });
            return this;
        }

        public SecurityHeadersOptionsBuilder AddExpires()
        {
            policy.AddHandler((headers, ctx) =>
            {
                if (!IsInNoCacheWhiteList(ctx))
                {
                    headers["Expires"] = "-1";
                }
            });
            return this;
        }

        public SecurityHeadersOptionsBuilder AddCacheControlHeader(int seconds = 0)
        {
            if (seconds == 0)
            {
                policy.AddHandler((headers, ctx) => headers["Cache-Control"] = "no-cache, no-store, must-revalidate");
            }
            else
            {
                policy.AddHandler((headers, ctx) => headers["Cache-Control"] = "public,max-age=" + seconds);
            }
            return this;
        }

        public SecurityHeadersOptionsBuilder AddXContentTypeOptionsNoSniffHeader()
        {
            policy.AddHandler((headers, ctx) => headers["X-Content-Type-Options"] = "nosniff");
            return this;
        }

        public SecurityHeadersOptionsBuilder AddXSSProtectionHeader()
        {
            policy.AddHandler((headers, ctx) => headers["X-XSS-Protection"] = "1; mode=block");
            return this;
        }

        public SecurityHeadersOptionsBuilder AddFrameOptionsSameOriginHeader()
        {
            policy.AddHandler((headers, ctx) => headers["X-Frame-Options"] = "SAMEORIGIN");
            return this;
        }

        public SecurityHeadersOptionsBuilder AddStrictTransportSecurityHeader()
        {
            policy.AddHandler((headers, ctx) => headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload");
            return this;
        }

        public SecurityHeadersOptionsBuilder AddPermissionsPolicyHeader()
        {
            policy.AddHandler((headers, ctx) => headers["Permissions-Policy"] = "midi=(),camera=(),microphone=()");
            return this;
        }

        public SecurityHeadersOptionsBuilder AddMicrophonePermissionsPolicyHeader()
        {
            policy.AddHandler((headers, ctx) => headers["Permissions-Policy"] = "geolocation=(self),microphone=(self)");
            return this;
        }

        public SecurityHeadersOptionsBuilder AddContentSecurityPolicyHeader()
        {
            policy.AddHandler((headers, ctx) => headers["Content-Security-Policy"] = "script-src 'self' 'unsafe-inline' *.pendo.io *.storage.googleapis.com 'unsafe-eval' *.aptrinsic.com; object-src 'self';frame-ancestors 'self';default-src 'self';style-src 'self' 'unsafe-inline' fonts.googleapis.com *.aptrinsic.com;img-src data: 'self' storage.googleapis.com data.pendo.io *.aptrinsic.com;connect-src *.aptrinsic.com *.table.core.windows.net 'self' *.table.core.chinacloudapi.cn *.table.core.usgovcloudapi.net;");
            return this;
        }

        public SecurityHeadersOptionsBuilder AddReferrerPolicyHeader(string value = "strict-origin-when-cross-origin")
        {
            policy.AddHandler((headers, ctx) => headers["Referrer-Policy"] = value);
            return this;
        }

        public SecurityHeadersOptionsBuilder AddFeaturePolicyHeader()
        {
            policy.AddHandler((headers, ctx) => headers["Feature-Policy"] = "");
            return this;
        }

        public SecurityHeadersOptionsBuilder AddCustomizedHeader(string header, string value)
        {
            policy.AddHandler((headers, ctx) => headers[header] = value);
            return this;
        }


        private bool IsInNoCacheWhiteList(HttpContext httpContext)
        {
            string path = httpContext.Request.Path.ToString().ToLowerInvariant();
            return noCacheWhiteList.Any(w => path.Contains(w)); //start with ?
        }
    }
}