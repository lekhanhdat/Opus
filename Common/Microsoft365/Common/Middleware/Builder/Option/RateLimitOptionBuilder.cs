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
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Microsoft365.Graph.Tests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("TestModel")]
namespace Microsoft365.Common.Middleware;
using Microsoft365.Common.RequestMonitor;
using System;
using System.Net.Http;

public class RateLimitOptionBuilder
{
    internal static int InitalDelay { get; set; } = 3;
    internal static TimeSpan DelayTimeLimit { get; set; } = TimeSpan.FromSeconds(180);
    internal const int MaxRateLimitReset = 60;

    /// <summary>
    /// Registers a throttle window by writing into <see cref="Microsoft365RequestMonitorService.AddThrottlingBlockedTimeRange"/>.
    /// This is the single source of truth for throttle state — no separate in-memory flags needed.
    /// </summary>
    public static void NotifyThrottled(TimeSpan retryAfter)
    {
        var duration = retryAfter > TimeSpan.Zero ? retryAfter : TimeSpan.FromSeconds(10);
        var start = DateTime.UtcNow;
        var end = start.Add(duration);
        Microsoft365RequestMonitorService.Instance.AddThrottlingBlockedTimeRange(start, end);
    }

    public RateLimitOption Build() => new()
    {
        IsHealthy = IsHealthy,
        InitalDelay = InitalDelay,
        DelayTimeLimit = DelayTimeLimit
    };

    public bool IsHealthy(HttpRequestMessage request)
    {
        return !Microsoft365RequestMonitorService.Instance.IsCurrentlyThrottled();
    }

    /// <summary>
    /// Evaluates Graph API RateLimit-* response headers and registers a throttle window when quota is low.
    /// </summary>
    /// <param name="limit">RateLimit-Limit header value</param>
    /// <param name="remaining">RateLimit-Remaining header value</param>
    /// <param name="reset">RateLimit-Reset header value in seconds</param>
    internal static void NotifyRateLimitHeader(int limit, int remaining, int reset)
    {
        if (limit <= 0 || remaining < 0 || reset < 0) return;

        var remainingPercentage = (double)remaining / limit * 100;
        reset = Math.Min(reset, MaxRateLimitReset);

        if (remainingPercentage < 5)
        {
            NotifyThrottled(TimeSpan.FromSeconds(reset > 0 ? reset : 10));
        }
        else if (remainingPercentage < 10 && Random.Shared.Next(0, 10) < 5)
        {
            // Probabilistic throttle at 5-10% remaining to spread load gradually
            NotifyThrottled(TimeSpan.FromSeconds(reset > 0 ? reset / 2 : 5));
        }
    }
}