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
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;


namespace Microsoft365.Common.Middleware;
public static class DefaultRetryOption
{
    public static HashSet<HttpStatusCode> ShouldRetryHttpCode = new()
    {
        //RetryHandler always retry for http 429,503,504 by default
        HttpStatusCode.TooManyRequests,//429
        HttpStatusCode.ServiceUnavailable,//503
        HttpStatusCode.GatewayTimeout,//504
        (HttpStatusCode)509,
        HttpStatusCode.BadGateway,//502
        HttpStatusCode.InternalServerError, //500
        HttpStatusCode.RequestTimeout,//408
        //NEVER retry for 401 here, AuthenticationHandler will take care of 401
    };
    public const int HttpMaxRetry = 4;
    public const int HttpInitialDelay = 3;
    public const int HttpRetriesTimeLimit = 600;//seconds

    public const int SocketInitialDelay = 2;
    public const int SocketMaxRetry = 3;
    public const int SocketRetriesTimeLimit = 60;//seconds
}
public abstract class RetryOptionBuilder
{
    #region HTTP retry
    public virtual int HttpMaxRetry { get; } = DefaultRetryOption.HttpMaxRetry;
    public virtual int HttpInitialDelay { get; } = DefaultRetryOption.HttpInitialDelay;
    public virtual int HttpRetriesTimeLimit { get; } = DefaultRetryOption.HttpRetriesTimeLimit;//seconds
    public virtual HashSet<HttpStatusCode> ShouldRetryHttpCode { get; } = DefaultRetryOption.ShouldRetryHttpCode;

    //https://learn.microsoft.com/en-us/graph/throttling
    //https://learn.microsoft.com/en-us/sharepoint/dev/general-development/how-to-avoid-getting-throttled-or-blocked-in-sharepoint-online
    //Retry grows exponentially. 3,6,12,24
    public RetryHandlerOption BuildHttpRetryOption() => new()
    {
        ShouldRetry = ShouldRetry,
        //there's a bug for retry handler,if single request retry-after over 3 mins, it will wait 3min,but record it as actually time,this may cause confuse,so don't assignee this value now.
        //RetriesTimeLimit = TimeSpan.FromSeconds(HttpRetriesTimeLimit),
        MaxRetry = HttpMaxRetry,
        Delay = HttpInitialDelay,
    };

    protected virtual bool ShouldRetry(int delay, int attempt, HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return false;
        if (!response.RequestMessage.IsBuffered()) return false;
        return response switch
        {
            //retry based on status code here
            { StatusCode: var code } when ShouldRetryHttpCode.Contains(code) => attempt < HttpMaxRetry,//return original error instead of too many retries
            //retry based on response headers here
            { Headers: var header } when ShouldRetry(header) => attempt < HttpMaxRetry,
            //retry based on response content here
            { StatusCode: var code, Content: var content } when ShouldRetry(code, content) => attempt < HttpMaxRetry,
            _ => false,
        };
    }

    protected virtual bool ShouldRetry(HttpStatusCode httpStatusCode, HttpContent content)
    {
        return false;
    }

    //retry based on response headers 
    protected virtual bool ShouldRetry(HttpResponseHeaders header)
    {
        if (header.TryGetRetryAfter(out var retryAfter))
        {
            RateLimitOptionBuilder.NotifyThrottled(TimeSpan.FromSeconds(retryAfter > 0 ? retryAfter : 10));
            return true;
        }
        return false;
    }
    #endregion

    #region Socket retry
    public virtual int SocketInitialDelay { get; } = DefaultRetryOption.SocketInitialDelay;
    public virtual int SocketMaxRetry { get; } = DefaultRetryOption.SocketMaxRetry;
    public virtual int SocketRetriesTimeLimit { get; } = DefaultRetryOption.SocketRetriesTimeLimit;//seconds
    //Retry grows exponentially. 2,4,8
    public SocketRetryOption BuildSocketRetryOption() => new()
    {
        ShouldRetry = ShouldRetry,
        Delay = SocketInitialDelay,
        MaxRetry = SocketMaxRetry,
    };
    protected virtual bool ShouldRetry(int delay, int attempt, System.Exception error)
    {
        return error switch
        {
            //Add more errors which should not retry here
            OperationCanceledException or//timeout or user canceled
            ArgumentException
            => false,
            //Reviewed with Fariel Zhang, set default to true.
            //It is a tradeoff between performance and stability
            _ => true,
        };
    }
    #endregion
}