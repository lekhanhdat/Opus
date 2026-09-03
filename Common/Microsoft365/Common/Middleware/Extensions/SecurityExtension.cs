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
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

using Newtonsoft.Json.Linq;

namespace Microsoft365.Common.Middleware;

internal static class SecurityExtension
{

    //Headers may contains sensitive information, whitelist to avoid output sensitive information like token, username, file name.
    private static readonly string[] IncludeHeaderNames = new[]
    {
            "client-request-id", "request-id",  "x-ms-ags-diagnostic","SPRequestGuid","Date",
            "CTag", "docID",  "Content-Length", "Content-Range",
            "Retry-After", "RateLimit-Limit", "RateLimit-Remaining", "RateLimit-Reset","RateLimit-Policy"
    };

    public static string RemoveSensitiveInfo(this Uri requestUri)
    {
        if (requestUri is null) return null;
        return RemoveSensitiveInfo(requestUri.AbsoluteUri);
    }

    private static string RemoveSensitiveInfo(this string url)
    {
        //sharepoint file download url
        if (url?.IndexOf("tempauth=", out var index) ?? false)
        {
            return url.Remove(index + 9);
        }
        if (url?.IndexOf("authtoken=", out index) ?? false)
        {
            return url.Remove(index + 10);
        }
        return url;
    }

    private static bool IndexOf(this string self, string value, out int index)
    {
        index = self.IndexOf(value, StringComparison.OrdinalIgnoreCase);
        return index > 0;
    }

    public static string ToFormatedString(this HttpHeaders headers, bool removeSensitiveInfo = true)
    {
#if DEBUG
        removeSensitiveInfo = false;
#endif
        if (headers is null) return null;
        var builder = new StringBuilder();
        foreach (var kv in headers)
        {
            if ((!removeSensitiveInfo) || IncludeHeaderNames.Contains(kv.Key))
            {
                builder.Append(kv.Key);
                builder.Append(": ");
                builder.AppendLine(string.Join(", ", kv.Value.Select(v => v.RemoveSensitiveInfo())));
            }
        }
        return builder.ToString();
    }
}