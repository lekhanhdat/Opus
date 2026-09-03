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


using Microsoft365.Common.Middleware.Handlers;
using Microsoft365.Common.RequestMonitor;

using System.Net;
using System.Net.Http;
using System;
public class TelemetryOptionBuilder
{
    //private static readonly ICloudBackupLogger logger = CloudBackupLogManager.Get(typeof(TelemetryOptionBuilder));
    
    public TelemetryOption Build() => new()
    {
        OnSuccessResponse = SuccessResponse,
        OnErrorResponse = ErrorResponse,
    };

    public void SuccessResponse(HttpResponseMessage response)
    {
        Microsoft365RequestMonitorService.Instance.AddRequest(ResponseStateType.OK);
        SetRateLimit(response);
    }

    public void ErrorResponse(HttpResponseMessage response, Exception error)
    {
        if (response is not null)
        {
            SetRateLimit(response);
            if (response.TryGetRetryAfter(out var retryAfter) ||
                response.StatusCode == HttpStatusCode.TooManyRequests ||
                response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                response.StatusCode == HttpStatusCode.GatewayTimeout)
            {
                Microsoft365RequestMonitorService.Instance.AddRequest(ResponseStateType.Throttled);
                RateLimitOptionBuilder.NotifyThrottled(retryAfter > 0
                    ? TimeSpan.FromSeconds(retryAfter)
                    : TimeSpan.Zero);
                return;
            }
        }
        Microsoft365RequestMonitorService.Instance.AddRequest(ResponseStateType.Failed);
    }

   
    private static void SetRateLimit(HttpResponseMessage response)
    {
        if (response?.Headers?.TryGetRateLimit(out var limit, out var remaining, out var reset) ?? false)
        {
            RateLimitOptionBuilder.NotifyRateLimitHeader(limit, remaining, reset);
        }
    }
}