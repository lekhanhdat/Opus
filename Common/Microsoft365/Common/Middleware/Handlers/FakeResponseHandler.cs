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
namespace Microsoft365.Common.Middleware.Handlers;

public class FakeResponseHandler : DelegatingHandler
{
    internal FakeResponseOption Option { get; private set; }

    public FakeResponseHandler() : this(new FakeResponseOption())
    { }

    public FakeResponseHandler(FakeResponseOption option)
    {
        Option = option;
    }


    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var option = request.GetRequestOption<FakeResponseOption>() ?? Option;
        option.Callback?.Invoke(request);
        return await FakeHttpResponse(option, request);
    }

    protected Task<HttpResponseMessage> SendToInnerHandlerAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return base.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> FakeHttpResponse(FakeResponseOption option, HttpRequestMessage message)
    {
        var isInvalidToken = message.Headers.Authorization?.Parameter?.EqualsIgnoreCase("InvalidToken") ?? false;
        if (message.Content != null)
        {
            var memoryStream = new MemoryStream();
            await message.Content.CopyToAsync(memoryStream);
        }
        var rsp = new HttpResponseMessage(isInvalidToken ? HttpStatusCode.Unauthorized : option.StatusCode)
        {
            Content = option.ResponseContent,
            RequestMessage = message
        };
        if (option.Headers is not null)
        {
            foreach (var header in option.Headers)
            {
                if (header.Value is IEnumerable<string> values)
                {
                    rsp.Headers.TryAddWithoutValidation(header.Key, values);
                }
                else if (header.Value is string str)
                {
                    rsp.Headers.TryAddWithoutValidation(header.Key, str);
                }
                else
                {
                    throw new ArgumentException($"Header value is not valid, key:{header.Key}, value:{header.Value}");
                }
            }
        }
        return rsp;
    }
}

public class FakeFirstResponseHandler : FakeResponseHandler
{
    private bool first = true;

    public FakeFirstResponseHandler(FakeResponseOption option) : base(option)
    {
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (first)
        {
            first = false;
            return await base.SendAsync(request, cancellationToken);
        }

        // For subsequent requests, delegate to the inner handler
        return await base.SendToInnerHandlerAsync(request, cancellationToken);
    }
}