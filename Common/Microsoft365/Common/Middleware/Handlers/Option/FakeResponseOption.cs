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

namespace Microsoft365.Common.Middleware;

using System.Net;
using System.Net.Http.Json;

using Microsoft.Kiota.Abstractions;

public class FakeResponseOption : IRequestOption
{
    private sealed class Error
    {
        /// <summary>
        /// The error code
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// The error message
        /// </summary>
        public string? Message { get; set; }
    }

    public object FakeObject { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public bool Compress { get; private set; }

    public Action<HttpRequestMessage>? Callback { get; set; }
    public IDictionary<string, object>? Headers { get; set; }

    public FakeResponseOption(object fakeObject, HttpStatusCode statusCode = HttpStatusCode.OK, bool compress = false)
    {
        FakeObject = fakeObject;
        StatusCode = statusCode;
        Compress = compress;
    }

    public FakeResponseOption() : this(new Error { Message = "Fake response is null" }, HttpStatusCode.NotFound)
    {
    }

    public HttpContent ResponseContent
    {
        get
        {
            HttpContent content = this.FakeObject switch
            {
                Stream stream => new StreamContent(stream),
                string str => new StringContent(str, null, "application/json"),
                _ => JsonContent.Create(FakeObject, FakeObject.GetType(), MediaTypeHeaderValue.Parse("application/json")),
            };
            return Compress ? ToGzipContent(content) : content;
        }
    }

    private static HttpContent ToGzipContent(HttpContent content)
    {
        var mediaType = content.Headers.ContentType?.MediaType;
        if (@"application/json".EqualsIgnoreCase(mediaType))
        {
            var sc = new StreamContent(ToGzipStream(content));
            sc.Headers.ContentType = content.Headers.ContentType;
            sc.Headers.ContentEncoding.Add("gzip");
            return sc;
        }
        return content;

        static MemoryStream ToGzipStream(HttpContent content)
        {
            var stream = new MemoryStream();
            using (var zip = new GZipStream(stream, CompressionMode.Compress, true))
            {
                content.CopyTo(zip, null, default);
            }
            stream.Position = 0L;
            return stream;
        }
    }

    public static FakeResponseOption Returns429 => new()
    {
        StatusCode = HttpStatusCode.TooManyRequests,
        Headers = new Dictionary<string, object>
        {
            { "Retry-After", "1" }
        },
        FakeObject = "Rate limit exceeded",
        Callback = request =>
        {
            request.Content?.CopyTo(new MemoryStream(), null, default);
        }
    };

    public static FakeResponseOption Return410 => new()
    {
        StatusCode = HttpStatusCode.Gone,
        FakeObject = MESSAGE_410,
        Callback = request =>
        {
            request.Content?.CopyTo(new MemoryStream(), null, default);
        }
    };

    const string MESSAGE_410 = "{\"error\":{\"code\":\"resyncRequired\",\"message\":\"Resync required. Replace any local items with the server's version (including deletes) if you're sure that the service was up to date with your local changes when you last sync'd. Upload any local changes that the server doesn't know about.\",\"innerError\":{\"date\":\"2023-08-10T06:53:02\",\"request-id\":\"********-****-****-****-************\",\"client-request-id\":\"********-****-****-****-************\"}}}";
}