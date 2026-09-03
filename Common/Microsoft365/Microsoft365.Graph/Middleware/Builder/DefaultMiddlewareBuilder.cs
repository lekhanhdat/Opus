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

namespace Microsoft365.Graph.Middleware;

public class DefaultMiddlewareBuilder : MiddlewareBuilder
{
    /// <summary>
    ///  new AuthenticationHandler(authenticationProvider),
    ///  new CompressionHandler(),
    ///  new RetryHandler(),
    ///  new RedirectHandler(),
    ///  new SocketRetryHandler(),
    ///  new TelemetryHandler(),
    /// </summary>
    /// <param name="provider"></param>
    /// <returns></returns>
    public override IList<DelegatingHandler> GetMiddlewares(IAccessTokenProvider? tokenProvider)
    {
        var handlers = base.GetMiddlewares(tokenProvider);

        //add or remove your own middlewares here
        //the order in which middleware components run is significant
        AddHttpRetryPolicy(handlers);
        AddSocketRetryPolicy(handlers);
        //rate limit handler does not work without telemetry handler
        AddRateLimit(handlers);
        AddAuthentication(handlers, tokenProvider);
        AddTelemetry(handlers);
        return handlers;
    }

    private static void AddAuthentication(IList<DelegatingHandler> handlers, IAccessTokenProvider? tokenProvider)
    {
        if (tokenProvider != null)
        {
            handlers.Add(new AuthenticationHandler(tokenProvider));
        }
    }

    private static void AddRateLimit(IList<DelegatingHandler> handlers)
    {
        var option = new RateLimitOptionBuilder().Build();
        handlers.Add(new RateLimitHandler(option));
    }

    private static void AddSocketRetryPolicy(IList<DelegatingHandler> handlers)
    {
        var option = new GraphRetryOptionBuilder().BuildSocketRetryOption();
        handlers.Add(new SocketRetryHandler(option));
    }

    private static void AddTelemetry(IList<DelegatingHandler> handlers)
    {
        //if you want to filter out redirect request, add TelemetryHandler before RedirectHandler
        var option = new TelemetryOptionBuilder().Build();
        handlers.Add(new Microsoft365.Common.Middleware.Handlers.TelemetryHandler(option));
    }

    public static void AddHttpRetryPolicy(IList<DelegatingHandler> handlers)
    {
        //http retry handler
        var retryHandler = handlers.OfType<RetryHandler>().FirstOrDefault() ?? throw new ArgumentException("Cannot find the default retry handler");
        retryHandler.SetRetryHandlerOption(new GraphRetryOptionBuilder().BuildHttpRetryOption());
    }
}