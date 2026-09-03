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

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions.Authentication;

public class AuthenticationHandler : DelegatingHandler
{
    private int MaxRetry { get; set; } = 1;

    public IAccessTokenProvider TokenProvider { get; set; }

    public AuthenticationHandler(IAccessTokenProvider tokenProvider)
    {
        TokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
    }


    private async Task<HttpResponseMessage> SendRetryAsync(HttpResponseMessage response, IAccessTokenProvider authProvider, CancellationToken cancellationToken)
    {
        int retryAttempt = 0;
        while (retryAttempt < MaxRetry)
        {
            // Drain response content to free connections.
            await response.DrainAsync(cancellationToken).ConfigureAwait(false);
            var newRequest = await response.RequestMessage.CloneAsync().ConfigureAwait(false);
            response = await InnerSendAsync(newRequest, cancellationToken).ConfigureAwait(false);
            retryAttempt++;
            if (!response.IsUnauthorized() || !newRequest.IsBuffered())
            {
                return response;
            }
        }
        return response;
    }



    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (this.TokenProvider != null)
        {
            var httpResponseMessage = await InnerSendAsync(request, cancellationToken).ConfigureAwait(false);
            if (httpResponseMessage.IsUnauthorized() && request.IsBuffered())
            {
                httpResponseMessage = await SendRetryAsync(httpResponseMessage, this.TokenProvider, cancellationToken).ConfigureAwait(false);
            }
            return httpResponseMessage;
        }
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private async Task<HttpResponseMessage> InnerSendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await Authorization(request, cancellationToken);
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private async Task Authorization(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var option = request.GetRequestOption<AuthenticationOption>();
        if (option != null && option.Anonymous)
        {
            return;
        }
        var token = await this.TokenProvider.GetAuthorizationTokenAsync(request.RequestUri!, null, cancellationToken);
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}