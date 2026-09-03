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



using Microsoft365.Common.Logger;

namespace Microsoft365.Common.Middleware.Handlers;
public class RateLimitHandler : DelegatingHandler
{
    private static IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(RateLimitHandler));
    internal RateLimitOption Option { get; private set; }
    //if we need to add delay time to telemetry, put it in HttpRequestMessage.Options 
    //internal int OverAllDelayInSecond = 0;

    public RateLimitHandler(RateLimitOption option)
    {
        Option = option ?? new();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await Delay(request, cancellationToken);
        return await base.SendAsync(request, cancellationToken);
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Delay(request, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
        return base.Send(request, cancellationToken);
    }

    private async Task Delay(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        int retryCount = 0;
        var totalDelay = TimeSpan.Zero;
        var scopeId = Guid.NewGuid();
        try
        {
            //if service is not healthy, block sending the request until services is healthy or timeout reached.
            while (!Option.IsHealthy(request) &&
                !ExceedTimeLimit(totalDelay))
            {
                if (retryCount == 0)
                {
                    LogStart(request, scopeId);
                }
                var delay = Delay(retryCount++, totalDelay);
                await Task.Delay(delay, cancellationToken);
                totalDelay += delay;
            }
        }
        finally
        {
            LogExit(request, totalDelay, scopeId);
        }
    }

    private static void LogExit(HttpRequestMessage request, TimeSpan totalDelay, Guid scopeId)
    {
        if (totalDelay > TimeSpan.Zero)
        {
            //Interlocked.Add(ref OverAllDelayInSecond, (int)totalDelay.TotalSeconds);
            logger.Info($"RateLimitDelay-{scopeId}-exit-{totalDelay.TotalSeconds}s-{request.RequestUri.RemoveSensitiveInfo()}");
        }
    }

    private static void LogStart(HttpRequestMessage request, Guid scopeId)
    {
        logger.Info($"RateLimitDelay-{scopeId}-start-{request.RequestUri.RemoveSensitiveInfo()}");
    }

    private bool ExceedTimeLimit(TimeSpan totalDelay)
    {
        return Option.DelayTimeLimit > TimeSpan.Zero && totalDelay > Option.DelayTimeLimit;
    }

    private TimeSpan Delay(int retryCount, TimeSpan total)
    {
        var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount) * Option.InitalDelay);
        if (ExceedTimeLimit(total + delay))
        {
            return Option.DelayTimeLimit - total + TimeSpan.FromSeconds(1);//add one more second to avoid boundary conditions bugs
        }
        return delay;
    }
}