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

namespace AvePoint.RA.Web.Extentions.Util
{
    public static class HttpRequestExtensions
    {
        public static Uri GetUrl(this HttpRequest request)
        {
            return new Uri($"{request.Scheme}://{request.Host}{request.PathBase}{request.Path}{request.QueryString}");
        }

        /// <summary>
        /// Return empty string if not found
        /// </summary>
        public static string GetRequestHeadersParam(this HttpRequest request, string param)
        {
            return request.Headers.GetHeaderValue(param) ?? string.Empty;
        }

        public static string GetFirstHeaderValue(this IHeaderDictionary headers, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }
            if (headers.TryGetValue(key, out var headerVal))
            {
                return headerVal.FirstOrDefault();
            }
            var lowerCaseKey = key.ToLower();
            if (headers.TryGetValue(lowerCaseKey, out headerVal))
            {
                return headerVal.FirstOrDefault();
            }
            return null;
        }

        /// <summary>
        /// Return null if not found
        /// </summary>
        public static string GetHeaderValue(this IHeaderDictionary headers, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            if (headers.TryGetValue(key, out var headerVal))
            {
                return headerVal;
            }

            var lowerCaseKey = key.ToLower();
            if (headers.TryGetValue(lowerCaseKey, out headerVal))
            {
                return headerVal;
            }

            return null;
        }

        public static bool ContainsHeader(this IHeaderDictionary headers, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            if (headers.ContainsKey(key))
            {
                return true;
            }
            return headers.ContainsKey(key.ToLower());
        }
    }
}
