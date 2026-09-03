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


using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft365.Common.Middleware;
public static class HttpRequestMessageExtenion
{
    public static bool IsBuffered(this HttpRequestMessage request)
    {
        if (request == null) return true; //When to get batch responses with the Graph API in OneDrive modern backup, the RequestMessage in the HttpResponseMessage is not assigned the value, it will be null.
        var content = request.Content;

        if ((request.Method == HttpMethod.Put || request.Method == HttpMethod.Post || request.Method.Method.Equals("PATCH"))
            && content != null && !content.Available())
        {
            return false;
        }
        return true;
    }
    private static bool CanSeek(this HttpContent content)
    {
        try
        {
            if (content is StreamContent streamContent)
            {
                var stream = streamContent.ReadAsStream();
                return stream.CanSeek;
            }
            return true;
        }
        catch { return true; }
    }

    private static bool Available(this HttpContent content)
    {
        return content.Headers.ContentLength != null && (int)content.Headers.ContentLength != -1 && content.CanSeek();
    }

    public static bool IsUnauthorized(this HttpResponseMessage httpResponseMessage)
    {
        return httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized;
    }

    public static async Task DrainAsync(this HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content != null)
        {
            await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public static bool TryGetRetryAfter(this HttpResponseMessage response, out int retryAfter)
    {
        return response.Headers.TryGetRetryAfter(out retryAfter);
    }

    public static int GetRetryAfterOrDefault(this HttpResponseMessage response)
    {
        if (response.Headers.TryGetRetryAfter(out int retryAfter))
        {
            return retryAfter;
        }
        return default;
    }

    public static bool TryGetRetryAfter(this HttpResponseHeaders headers, out int retryAfter)
    {
        return headers.TryGetValue("Retry-After", out retryAfter);
    }

    /// <summary>
    /// Try get rate limit headers
    /// </summary>
    /// <param name="headers"></param>
    /// <param name="limit">RateLimit-Limit</param>
    /// <param name="remaining">RateLimit-Remaining</param>
    /// <param name="reset">RateLimit-Reset</param>
    /// <returns>true if all the three headers are returned, otherwise false</returns>
    public static bool TryGetRateLimit(this HttpResponseHeaders headers, out int limit, out int remaining, out int reset)
    {
        remaining = 0;
        reset = 0;
        return headers.TryGetValue("RateLimit-Limit", out limit) &&
            headers.TryGetValue("RateLimit-Remaining", out remaining) &&
            headers.TryGetValue("RateLimit-Reset", out reset);
    }

    public static bool TryGetValue(this HttpResponseHeaders headers, string name, out int value)
    {
        if (headers.TryGetValues(name, out var values))
        {
            var valueStr = values.FirstOrDefault();

            if (int.TryParse(valueStr, out value))
            {
                //value is an integer
                //RateLimit-Limit: 1200
                return true;
            }
            else
            {
                //backward compatibility
                //value is sf-list, based on draft-ietf-httpapi-ratelimit-headers-03 section 5.1.
                //RateLimit-Limit: 100, 100;w=300;x-spo-scope="user";comment="Resource Percentage"
                //https://datatracker.ietf.org/doc/draft-ietf-httpapi-ratelimit-headers/03/

                //Each member is separated by a comma and optional whitespace.
                return int.TryParse(valueStr?.Split(',')?.FirstOrDefault()?.Trim(), out value);
            }
        }
        value = 0;
        return false;
    }

    public static async Task<HttpRequestMessage> CloneAsync(this HttpRequestMessage originalRequest)
    {
        var newRequest = new HttpRequestMessage(originalRequest.Method, originalRequest.RequestUri);

        // Copy request headers.
        foreach (var header in originalRequest.Headers)
        {
            newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        IDictionary<string, object?> options = newRequest.Options;
        // Copy request options.
        foreach (var option in originalRequest.Options)
        {
            options.Add(option.Key, option.Value);
        }

        // Set Content if previous request had one.
        if (originalRequest.Content != null)
        {
            // HttpClient doesn't rewind streams and we have to explicitly do so.
            var stream = await originalRequest.Content.ReadAsStreamAsync().ConfigureAwait(false);
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
            newRequest.Content = new StreamContent(stream);

            // Copy content headers.
            foreach (var contentHeader in originalRequest.Content.Headers)
            {
                newRequest.Content.Headers.TryAddWithoutValidation(contentHeader.Key, contentHeader.Value);
            }
        }

        return newRequest;
    }

    public static async Task<Stream> ReadStreamAsync(this HttpContent content, bool decompress, CancellationToken cancellationToken = default)
    {
        var bytes = await content.ReadAsByteArrayAsync(cancellationToken);
        Stream stream = new MemoryStream(bytes);
        if (decompress && content.IsCompress())
        {
            stream = new GZipStream(stream, CompressionMode.Decompress);
        }
        return stream;
    }

    public static bool IsCompress(this HttpContent content)
    {
        return content != null && content.Headers.ContentEncoding.Contains("gzip");
    }
}