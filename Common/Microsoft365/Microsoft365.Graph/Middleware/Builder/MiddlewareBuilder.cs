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

/// <summary>
/// Default and base class, Graph SDK builtin middlewares.
/// Add or remove your own middlewares in derived class
/// </summary>
public abstract class MiddlewareBuilder
{
    internal static readonly MiddlewareBuilder Default = new DefaultMiddlewareBuilder();

    //You can add and remove middleware components here.
    //It is important to note that the order in which middleware components run is significant
    //Base class return default middlewares, override to add or remove your own middleware componments.
    //  new AuthenticationHandler(authenticationProvider),
    //  new CompressionHandler(),
    //  new RetryHandler(),
    //  new RedirectHandler()
    public virtual IList<DelegatingHandler> GetMiddlewares(IAccessTokenProvider? tokenProvider)
    {
        return GraphClientFactory.CreateDefaultHandlers(null);
    }

    //!!!READ before you start to write your own retry handler
    //1. Microsoft.Graph.RetryHandler is the first recommandation since it is the builtin middleware.
    //   It is based on HttpResponseMessage internal, so cannot handle exceptions before HttpResponseMessage return, like socket layer error.
    //   If you have to handle these exceptions, write another handler to take care of them. And let Microsoft.Graph.RetryHandler to take care of http 
    //2. Microsoft.Rest.RetryDelegatingHandler is the second recommandation, it is default retry handler for Microsoft.Rest.ServiceClient<T>.
    //   It is based on Exception internal which have more control, it always works together with RetryAfterDelegatingHandler
    //     new RetryDelegatingHandler(),
    //     new RetryAfterDelegatingHandler()
    //3. Try use these two approach first before write your own implementation
}