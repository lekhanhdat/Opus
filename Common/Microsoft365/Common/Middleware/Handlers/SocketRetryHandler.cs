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

using Microsoft365.Common.Middleware;

namespace Microsoft365.Common.Middleware.Handlers;

public class SocketRetryHandler : DelegatingHandler
{
    private const long MBInBytes = 1024 * 1024;
    internal SocketRetryOption RetryOption { get; set; }

    public SocketRetryHandler(SocketRetryOption retryOption = null)
    {
        RetryOption = retryOption ?? new();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var option = request.GetRequestOption<SocketRetryOption>() ?? RetryOption;
        int retryCount = 0;
        TimeSpan totalDelay = TimeSpan.Zero;
        while (true)
        {
            try
            {
                var request2 = request;
                if (retryCount > 0)//Do not need clone for first request
                {
                    request2 = await request.CloneAsync().ConfigureAwait(false);
                }
                var rsp = await base.SendAsync(request2, cancellationToken).ConfigureAwait(false);
                //if rsp content is application/json, we should load it into buffer to avoid connection leak
                if (rsp.Content.Headers.ContentType?.MediaType.EqualsIgnoreCase("application/json") == true && (rsp.Content.Headers.ContentLength is null || rsp.Content.Headers.ContentLength < MBInBytes))
                {
                    //Todo try LoadIntoBufferAsync(cancellationToken) instead of WaitAsync(cancellationToken) for .NET 10
                    await rsp.Content.LoadIntoBufferAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                }
                return rsp;
            }
            catch (System.Exception ex)
            {
                if (request.IsBuffered() &&
                    retryCount < option.MaxRetry &&
                    option.ShouldRetry(option.Delay, retryCount, ex))
                {
                    var retryAfter = Delay(retryCount, option);
                    totalDelay += retryAfter;
                    if (ExceedRetriesTimeLimit(totalDelay, option))
                    {
                        //totalDelay -= retryAfter;
                        throw;
                    }
                    await Task.Delay(retryAfter, cancellationToken).ConfigureAwait(false);
                    retryCount++;
                }
                else
                {
                    throw;
                }
            }
        }

    }

    private bool ExceedRetriesTimeLimit(TimeSpan totalDelay, SocketRetryOption option)
    {
        return option.RetriesTimeLimit > TimeSpan.Zero && totalDelay > option.RetriesTimeLimit;
    }

    private TimeSpan Delay(int retryCount, SocketRetryOption option)
    {
        var retryAfter = (int)Math.Pow(2, retryCount) * option.Delay;
        return TimeSpan.FromSeconds(retryAfter);
    }
}