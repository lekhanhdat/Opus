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
using System.Reflection;

namespace Microsoft365.Graph.Extensions;

internal static class MiddlewareExtensions
{
    internal static RequestConfiguration<T> WithAnonymousAuthentication<T>(this RequestConfiguration<T> config) where T : class, new()
    {
        config.Options.TryAddRequestOption(() => new AuthenticationOption(true));
        return config;
    }

    internal static HttpRequestMessage WithOptions(this HttpRequestMessage request, IRequestOption option)
    {
        request.Options.Set(new HttpRequestOptionsKey<IRequestOption>(option.GetType().FullName!), option);
        return request;
    }

    internal static RequestConfiguration<T> WithMoreRetry<T>(this RequestConfiguration<T> config, int additionalRetry = 2) where T : class, new()
    {
        var option = config.Options.TryAddRequestOption(() => new GraphRetryOptionBuilder().BuildHttpRetryOption());
        option.MaxRetry += additionalRetry;
        var option2 = config.Options.TryAddRequestOption(() => new GraphRetryOptionBuilder().BuildSocketRetryOption());
        option2.MaxRetry += additionalRetry;
        return config;
    }
    internal static RequestConfiguration<T> WithShouldRetry<T>(this RequestConfiguration<T> config, Func<int, int, HttpResponseMessage, bool> shouldRetry) where T : class, new()
    {
        var option = config.Options.TryAddRequestOption(() => new GraphRetryOptionBuilder().BuildHttpRetryOption());
        option.ShouldRetry = shouldRetry;
        return config;
    }

    internal static RequestConfiguration<T> WithShouldRetry<T>(this RequestConfiguration<T> config, Func<int, int, Exception, bool> shouldRetry) where T : class, new()
    {
        var option = config.Options.TryAddRequestOption(() => new GraphRetryOptionBuilder().BuildSocketRetryOption());
        option.ShouldRetry = shouldRetry;
        return config;
    }
    internal static RequestConfiguration<T> WithRequestOptions<T>(this RequestConfiguration<T> config, params IRequestOption[] options) where T : class, new()
    {
        foreach (var option in options)
        {
            config.Options.Add(option);
        }
        return config;
    }

    internal static TOption TryAddRequestOption<TOption>(this IList<IRequestOption> options, Func<TOption> creator) where TOption : IRequestOption
    {
        var option = options.GetRequestOption<TOption>();
        if (option is null)
        {
            option = creator();
            options.Add(option);
        }
        return option;
    }
    internal static TOption? GetRequestOption<TOption>(this IList<IRequestOption> options)
    {
        return options.OfType<TOption>().FirstOrDefault();
    }

    internal static void SetRetryHandlerOption(this RetryHandler handler, RetryHandlerOption option)
    {
        var property = handler.GetType().GetProperty("RetryOption", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException("RetryHandler.RetryOption does not exist.");
        property.SetValue(handler, option);
    }
}
