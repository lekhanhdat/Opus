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

namespace Microsoft365.Graph.Util;


internal static class GraphPagingUtil
{
    /// <summary>
    /// Page through a collection and return all items in it
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="request">Inital request</param>
    /// <param name="requestConfigurator">Set request header\option for next page</param>
    /// <param name="callback">Callback at the end of each page, with PageIterator which contains the status and delta link</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async IAsyncEnumerable<TEntity> GetAllAsync<TEntity, TCollectionPage>(
        [NotNull] this IBaseClient client,
        [NotNull] Func<Task<TCollectionPage?>> initialCollection,
        Func<RequestInformation, RequestInformation>? requestConfigurator,
        PagingCallback? callback,
        [EnumeratorCancellation] CancellationToken token = default) where TCollectionPage : IParsable, IAdditionalDataHolder, new()
    {
        ArgumentNullException.ThrowIfNull(initialCollection);
        const int CacheCount = 11;
        var cache = new List<TEntity>(CacheCount);
        string? nextlink = default;
        string? deltalink = default;
        PageIterator<TEntity, TCollectionPage>? iterator =
            PageIterator<TEntity, TCollectionPage>
            .CreatePageIterator(
                client,
                (await initialCollection().ConfigureAwait(false)).EnsureIfNotNull(),
                async (TEntity item) =>
                {
                    cache.Add(item);
                    return await Task.FromResult(cache.Count < CacheCount);
                }, requestConfigurator);
        try
        {
            do
            {
                token.ThrowIfCancellationRequested();
                await iterator.IterateAsync(token).ConfigureAwait(false);
                foreach (var item in cache)
                {
                    yield return item;
                }
                cache.Clear();
                if (IsLinkChanged())
                {
                    nextlink = iterator.Nextlink;
                    //handle last page, return deltalink ONLY if iteration finish, if break or cancel, we need to return nextlink instead of deltalink in callback
                    deltalink = iterator.Deltalink;
                    if (!IsFinalState())// avoid duplicated callback for last page
                    {
                        await InvokeCallback();
                    }
                }
            }
            while (!IsFinalState());
        }
        finally
        {
            // If there are still items in the cache, we should not have a deltalink yet (deltalink should be empty).
            if (!string.IsNullOrEmpty(deltalink))
            {
                System.Diagnostics.Debug.Assert(cache.IsNullOrEmpty(),
                    "Cache should be empty when deltalink is set; otherwise, deltalink must be empty and nextlink should be used.");
            }
            await InvokeCallback();
        }

        bool IsLinkChanged()
        {
            return !string.Equals(nextlink, iterator.Nextlink, StringComparison.OrdinalIgnoreCase) || !string.Equals(deltalink, iterator.Deltalink, StringComparison.OrdinalIgnoreCase);
        }

        bool IsFinalState()
        {
            return iterator.State == PagingState.Complete || iterator.State == PagingState.Delta;
        }

        async Task InvokeCallback()
        {
            if (callback != null)
            {
                await callback.Invoke((nextlink, deltalink, iterator.State));
            }
        }
    }

    public static IAsyncEnumerable<TEntity> GetAllAsync<TEntity, TCollectionPage>(
        [NotNull] this IBaseClient client,
        [NotNull] Func<Task<TCollectionPage?>> initialCollection,
        CancellationToken token = default) where TCollectionPage : IParsable, IAdditionalDataHolder, new()
    {
        return client.GetAllAsync<TEntity, TCollectionPage>(initialCollection, (Func<RequestInformation, RequestInformation>?)null, null, token);
    }

    ///// <summary>
    ///// Page through a collection and callback for each item
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="request">Inital request</param>
    ///// <param name="callback">return true to continue, false to break</param>
    ///// <param name="requestConfigurator">Set request header\option for next page</param>
    ///// <param name="token"></param>
    ///// <returns></returns>
    //public static async Task<PageIterator<T>> GetAllAsync<T>(
    //    this IBaseRequest request,
    //    Func<T, bool> callback,
    //    Func<IBaseRequest, IBaseRequest>? requestConfigurator = null,
    //    CancellationToken token = default)
    //{
    //    // We need access to the GetAsync. IBaseRequest doesn't define GetAsync.
    //    // We are making this dynamic so we can access GetAsync.
    //    dynamic request2 = request;
    //    ICollectionPage<T> page = await request2.GetAsync(token).ConfigureAwait(false);
    //    var iterator = PageIterator<T>
    //        .CreatePageIterator(
    //        request.Client,
    //        page,
    //        callback,
    //        requestConfigurator);
    //    await iterator.IterateAsync(token).ConfigureAwait(false);
    //    return iterator;
    //}

    ///// <summary>
    /////  Page through a collection and callback for each item
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="page">First page</param>
    ///// <param name="callback">return true to continue, false to break</param>
    ///// <param name="requestConfigurator">Set request header\option for next page</param>
    ///// <param name="token"></param>
    ///// <returns></returns>
    //public static async Task GetAllAsync<T>(
    //    this ICollectionPage<T> page,
    //    Func<T, bool> callback,
    //    Func<IBaseRequest, IBaseRequest>? requestConfigurator = null,
    //    CancellationToken token = default)
    //{
    //    // We need access to the NextPageRequest to call and get the next page. ICollectionPage<T> doesn't define NextPageRequest.
    //    // We are making this dynamic so we can access NextPageRequest.
    //    dynamic page2 = page;
    //    IBaseRequest nextPage = page2.NextPageRequest;
    //    if (nextPage is null)
    //    {
    //        foreach (var item in page)
    //        {
    //            if (!callback(item))
    //            {
    //                break;
    //            }
    //        }
    //    }
    //    else
    //    {
    //        var iterator = PageIterator<T>
    //            .CreatePageIterator(
    //            nextPage.Client,
    //            page,
    //            callback,
    //            requestConfigurator);
    //        await iterator.IterateAsync(token).ConfigureAwait(false);
    //    }
    //}
}