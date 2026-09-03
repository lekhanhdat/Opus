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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Extentions.Middleware.SecurityHeaders
{
    public class SecurityHeadersWithNonceMiddleware
    {
        private readonly RequestDelegate next;

        public SecurityHeadersWithNonceMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var cuurentHeader = context.Response.Headers["Content-Security-Policy"];
            if (cuurentHeader.IsNotNullOrEmpty())
            {
                var nonce = Generate64BitNonce();
                context.Items["Nonce"] = nonce;
                var scriptSrc = "script-src 'self'";
                var scriptSrcWithNonce = $"script-src 'self' 'nonce-{nonce}'";
                context.Response.Headers["Content-Security-Policy"] = cuurentHeader.ToString().Replace(scriptSrc, scriptSrcWithNonce);
            }
            await next(context);
        }

        private string Generate64BitNonce()
        {
            string combined = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            if (combined.Length < 64)
            {
                return combined;
            }
            Random random = new Random();
            int startIndex = random.Next(0, combined.Length - 64 + 1);
            return combined.Substring(startIndex, 64);
        }
    }
}
