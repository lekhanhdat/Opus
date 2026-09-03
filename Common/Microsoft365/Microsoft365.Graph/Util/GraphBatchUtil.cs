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

namespace Microsoft365.Graph.Util;

internal static class GraphBatchUtil
{
    /// <summary>
    /// Batch request honour default RetryOption
    /// </summary>
    internal static RetryHandlerOption RetryOption { get; private set; }

    private readonly static IMicrosoft365Logger _logger;

    static GraphBatchUtil() 
    {
        RetryOption = new GraphRetryOptionBuilder().BuildHttpRetryOption();
        _logger = Microsoft365LoggerManager.CreateLogger(typeof(GraphBatchUtil));
    }

    public static async ValueTask<BatchRequestStep> CreateBatchStepAsync(this IBaseClient client, string requestId, RequestInformation requestInformation)
    {
        var requestMessage=await client.RequestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInformation);
        return new BatchRequestStep(
            requestId.IsNullOrEmpty() ? Guid.NewGuid().ToString() : requestId,
            requestMessage);
    }

    /// <summary>
    /// Send a batch request, return response as async stream, order are same as <paramref name="batchRequestSteps"/>
    /// Honours the default RetryOption, retry may not work well for dependent requests.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="client"></param>
    /// <param name="requests"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async IAsyncEnumerable<(T? Result, ApiException? Error)> SendBatchRequestAsync<T>(
        this IBaseClient client,
        RequestInformation[] requests,
        [EnumeratorCancellation]CancellationToken token = default) where T : IParsable, new()
    {
        ArgumentNullException.ThrowIfNull(requests);
        List<BatchRequestStep> steps = new();
        foreach (var request in requests)
        {
            var requestMessage = await client.RequestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(request, token).ConfigureAwait(false);
            steps.Add(new BatchRequestStep(Guid.NewGuid().ToString(), requestMessage));
        }
        var result = client.SendBatchRequestAsync<T>(
            steps.ToArray(),
            token);
        await foreach (var (_, Result, Error) in result)
        {
            yield return (Result, Error);
        }
    }

    /// <summary>
    /// Send a batch request, return response as async stream, order are same as <paramref name="batchRequestSteps"/>
    /// Honours the default RetryOption, retry may not work well for dependent requests.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="client"></param>
    /// <param name="batchRequestSteps"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async IAsyncEnumerable<(string RequestId, T? Result, ApiException? Error)> SendBatchRequestAsync<T>(
        this IBaseClient client,
        BatchRequestStep[] batchRequestSteps,
        [EnumeratorCancellation] CancellationToken token = default) where T : IParsable, new()
    {
        var dic = new Dictionary<string, (T? Result, ApiException? Error)>();
        await SendBatchRequestAsync<T>(
            client,
            batchRequestSteps,
            (requestId, result, error) => dic[requestId] = (result, error),
            token).
            ConfigureAwait(false);
        foreach (var id in batchRequestSteps.Select(s => s.RequestId))
        {
            yield return (id, dic[id].Result, dic[id].Error);
        }
    }

    /// <summary>
    /// Send a batch request, return response as callback, note response may appear in a different order.
    /// Honours the default RetryOption, retry may not work well for dependent requests.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="client"></param>
    /// <param name="batchRequestSteps"></param>
    /// <param name="callback">a callback with RequestId, T instance(optional), ApiException(optional)</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task SendBatchRequestAsync<T>(
        this IBaseClient client,
        BatchRequestStep[] batchRequestSteps,
        Action<string, T?, ApiException?> callback,
        CancellationToken token = default) where T : IParsable, new()
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(batchRequestSteps);
        ArgumentNullException.ThrowIfNull(callback);
        if (batchRequestSteps.Length <= 0) throw new ArgumentOutOfRangeException(nameof(batchRequestSteps));
        #pragma warning disable CS0618 // Type or member is obsolete, TODO: upgrade to BatchRequestContentCollection in next upgrade window
        var baseBatchContent = new BatchRequestContent(client.RequestAdapter, batchRequestSteps);
        #pragma warning restore CS0618 // Type or member is obsolete
        int retryCount = 0;
        var batchContent = baseBatchContent;
        while (true)
        {
            var (RetryBatchContent, RetryAfters) = await SendBatchRequestInternalAsync(client, batchContent, callback, retryCount, token).ConfigureAwait(false);
            //return the request which need retry, callback the successful request
            if (RetryBatchContent != null)
            {
                batchContent = RetryBatchContent;
                //retry all the failed requests in a new batch after the longest retry-after value.
                //https://docs.microsoft.com/en-us/graph/throttling?view=graph-rest-1.0#throttling-and-batching
                await Delay(retryCount, RetryAfters, token).ConfigureAwait(false);
                retryCount++;
            }
            else
            {
                //all request successed or reach max retry limit
                break;
            }
        }
    }

    /// <summary>
    /// Send a batch request without parsing response to a specific type.
    /// Returns raw response information with status code and content in the same order as input.
    /// Honours the default RetryOption, retry may not work well for dependent requests.
    /// </summary>
    /// <param name="client">The Graph client</param>
    /// <param name="batchRequestSteps">Array of batch request steps</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>Async enumerable of batch response results</returns>
    public static async IAsyncEnumerable<BatchResponseResult> SendBatchRequestAsync(
        this IBaseClient client,
        BatchRequestStep[] batchRequestSteps,
        [EnumeratorCancellation] CancellationToken token = default)
    {
        var results = new Dictionary<string, BatchResponseResult>();
        await SendBatchRequestAsync(
            client,
            batchRequestSteps,
            (requestId, statusCode, content, error) => results[requestId] = new BatchResponseResult(requestId, statusCode, content, error),
            token).ConfigureAwait(false);

        foreach (var id in batchRequestSteps.Select(s => s.RequestId))
        {
            yield return results[id];
        }
    }

    /// <summary>
    /// Send a batch request without parsing response to a specific type.
    /// Returns response via callback. Note: responses may appear in a different order.
    /// Honours the default RetryOption, retry may not work well for dependent requests.
    /// </summary>
    /// <param name="client">The Graph client</param>
    /// <param name="batchRequestSteps">Array of batch request steps</param>
    /// <param name="callback">Callback with RequestId, HttpStatusCode, response content, and optional error</param>
    /// <param name="token">Cancellation token</param>
    public static async Task SendBatchRequestAsync(
        this IBaseClient client,
        BatchRequestStep[] batchRequestSteps,
        Action<string, HttpStatusCode, string?, Exception?> callback,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(batchRequestSteps);
        ArgumentNullException.ThrowIfNull(callback);
        if (batchRequestSteps.Length <= 0) throw new ArgumentOutOfRangeException(nameof(batchRequestSteps));

        var baseBatchContent = new BatchRequestContentCollection(client.RequestAdapter);
        foreach (var step in batchRequestSteps)
        {
            baseBatchContent.AddBatchRequestStep(step);
        }

        int retryCount = 0;
        var batchContent = baseBatchContent;

        while (true)
        {
            var (RetryBatchContent, RetryAfters) = await SendBatchRequestRawInternalAsync(
                client,
                batchContent,
                callback,
                retryCount,
                token).ConfigureAwait(false);

            if (RetryBatchContent != null)
            {
                batchContent = RetryBatchContent;
                await Delay(retryCount, RetryAfters, token).ConfigureAwait(false);
                retryCount++;
            }
            else
            {
                break;
            }
        }
    }

    private static async ValueTask<(BatchRequestContentCollection? RetryBatchContent, IEnumerable<int> RetryAfters)> SendBatchRequestRawInternalAsync(
        IBaseClient client,
        BatchRequestContentCollection batchContent,
        Action<string, HttpStatusCode, string?, Exception?> callback,
        int retryCount,
        CancellationToken token)
    {
        var batchResponse = await client.Batch.PostAsync(batchContent, token).ConfigureAwait(false);

        List<(string RequestId, HttpResponseMessage Response)> pendingRetryItems = [];

        foreach (var step in batchContent.BatchRequestSteps)
        {
            var requestId = step.Key;
            var response = await batchResponse.GetResponseByIdAsync(requestId).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode &&
                retryCount < RetryOption.MaxRetry &&
                step.Value.Request.IsBuffered() &&
                RetryOption.ShouldRetry(RetryOption.Delay, retryCount, response))
            {
                pendingRetryItems.Add((requestId, response));
            }
            else
            {
                string? content = null;
                Exception? error = null;
                try
                {
                    content = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                    {
                        error = new HttpRequestException($"Request failed with status code {response.StatusCode}: {content}");
                    }
                }
                catch (Exception ex)
                {
                    error = ex;
                }
                callback.Invoke(requestId, response.StatusCode, content, error);
            }
        }

        if (pendingRetryItems.Count > 0)
        {
            var retryBatch = new BatchRequestContentCollection(client.RequestAdapter);
            foreach (var (requestId, _) in pendingRetryItems)
            {
                var originalStep = batchContent.BatchRequestSteps[requestId];
                retryBatch.AddBatchRequestStep(originalStep);
            }

            var retryAfters = pendingRetryItems.Select(t => t.Response.GetRetryAfterOrDefault());
            return (retryBatch, retryAfters);
        }

        return default;
    }

    /// <summary>
    /// Send a batch request, return response as async stream, order are same as <paramref name="batchRequestSteps"/>
    /// Honours the default RetryOption, retry may not work well for dependent requests.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="client"></param>
    /// <param name="batchRequestSteps"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async IAsyncEnumerable<(string RequestId, T? Result, ApiException? Error)> SendBatchRequestV2Async<T>(
        this IBaseClient client,
        BatchRequestStep[] batchRequestSteps,
        [EnumeratorCancellation] CancellationToken token = default) where T : IParsable, new()
    {
        var dic = new Dictionary<string, (T? Result, ApiException? Error)>();
        await SendBatchRequestV2Async<T>(
            client,
            batchRequestSteps,
            (requestId, result, error) => dic[requestId] = (result, error),
            token).ConfigureAwait(false);
        foreach (var id in batchRequestSteps.Select(s => s.RequestId))
        {
            yield return (id, dic[id].Result, dic[id].Error);
        }
    }

    /// <summary>
    /// Send a batch request, return response as callback, note response may appear in a different order.
    /// Honours the default RetryOption, retry may not work well for dependent requests.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="client"></param>
    /// <param name="batchRequestSteps"></param>
    /// <param name="callback">a callback with RequestId, T instance(optional), ApiException(optional)</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task SendBatchRequestV2Async<T>(
        this IBaseClient client,
        BatchRequestStep[] batchRequestSteps,
        Action<string, T?, ApiException?> callback,
        CancellationToken token = default) where T : IParsable, new()
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(batchRequestSteps);
        ArgumentNullException.ThrowIfNull(callback);
        if (batchRequestSteps.Length <= 0) throw new ArgumentOutOfRangeException(nameof(batchRequestSteps));

        var baseBatchContent = new BatchRequestContentCollection(client.RequestAdapter);
        foreach (var step in batchRequestSteps)
        {
            baseBatchContent.AddBatchRequestStep(step);
        }

        int retryCount = 0;
        var batchContent = baseBatchContent;
        _logger.Info($"Start sending batch request with {batchRequestSteps.Length} steps.");
        while (true)
        {
            var (RetryBatchContent, RetryAfters) = await SendBatchRequestInternalAsync(client, batchContent, callback, retryCount, token).ConfigureAwait(false);
            //return the request which need retry, callback the successful request
            if (RetryBatchContent != null)
            {
                batchContent = RetryBatchContent;
                //retry all the failed requests in a new batch after the longest retry-after value.
                //https://docs.microsoft.com/en-us/graph/throttling?view=graph-rest-1.0#throttling-and-batching
                await Delay(retryCount, RetryAfters, token).ConfigureAwait(false);
                retryCount++;
            }
            else
            {
                //all request successed or reach max retry limit
                break;
            }
        }
    }

    private static async ValueTask<(BatchRequestContentCollection RetryBatchContent, IEnumerable<int> RetryAfters)> SendBatchRequestInternalAsync<T>(
        IBaseClient client,
        BatchRequestContentCollection batchContent,
        Action<string, T?, ApiException?> callback,
        int retryCount,
        CancellationToken token) where T : IParsable, new()
    {
        var batchResponse = await client.Batch.PostAsync(batchContent, token).ConfigureAwait(false);
        var statusCodes = await batchResponse.GetResponsesStatusCodesAsync();
        List<(string RequestId, HttpResponseMessage Response)> pendingRetryItems = [];
        foreach (var step in batchContent.BatchRequestSteps)
        {
            var requestId = step.Key;
            var response = await batchResponse.GetResponseByIdAsync(requestId).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode &&
                retryCount < RetryOption.MaxRetry &&
                step.Value.Request.IsBuffered() &&
                RetryOption.ShouldRetry(RetryOption.Delay, retryCount, response))
            {
                pendingRetryItems.Add((requestId, response));
            }
            else
            {
                T? result = default;
                ApiException? error = null;
                try
                {
                    //callback may need to be invoked with ConfigureAwait(true)
                    //https://devblogs.microsoft.com/dotnet/configureawait-faq/
                    result = await batchResponse.GetResponseByIdAsync<T>(requestId).ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                    {
                        error = new ApiException($"Request failed with status code {response.StatusCode}: {response.Content}");
                    }
                }
                catch (ApiException ex)
                {
                    error = ex;
                }
                catch (Exception ex)
                {
                    error = new ApiException("Failed to get response", ex);
                }
                callback.Invoke(requestId, result, error);
            }
        }
        // var nextlink = await batchResponse.GetNextLinkAsync().ConfigureAwait(false);

        // Currently batch seems always return full result(nextlink is null).
        // Do not known how to deal with nextlink, so throw if nextlink is not empty. This avoid data lost silently, in case graph batch support partial or async response in future.
        // Reference:
        // OData JSON Format section 19.5,19.6
        // http://docs.oasis-open.org/odata/odata-json-format/v4.01/odata-json-format-v4.01.html#sec_BatchResponse
        // https://docs.microsoft.com/en-us/graph/known-issues#json-batching
        // if (!string.IsNullOrEmpty(nextlink)) throw new InvalidDataException("Batch response contains nextlink, response may be partial.");
        if (pendingRetryItems.Count > 0)
        {
            var retryBatch = new BatchRequestContentCollection(client.RequestAdapter);
            foreach (var (requestId, _) in pendingRetryItems)
            {
                var originalStep = batchContent.BatchRequestSteps[requestId];
                retryBatch.AddBatchRequestStep(originalStep);
            }
            var retryAfters = pendingRetryItems.Select(t => t.Response.GetRetryAfterOrDefault());
            return (retryBatch, retryAfters);
        }
        return default;
    }

    private static async Task<TimeSpan> Delay(int retryCount, IEnumerable<int> retryAfters, CancellationToken token)
    {
        var retryAfter = retryAfters.Max();
        if (retryAfter <= 0)
        {
            retryAfter = (int)Math.Pow(2, retryCount) * RetryOption.Delay;
        }
        var delay = TimeSpan.FromSeconds(retryAfter);
        _logger.Warn($"Batch retry #{retryCount} | Items: {retryAfters.ToList().Count} | Delay: {delay.TotalSeconds}s");
        await Task.Delay(delay, token).ConfigureAwait(false);
        return delay;
    }

    private static async ValueTask<(BatchRequestContent RetryBatchContent,IEnumerable<int> RetryAfters)> SendBatchRequestInternalAsync<T>(
        IBaseClient client,
        BatchRequestContent batchContent,
        Action<string, T?, ApiException?> callback,
        int retryCount,
        CancellationToken token) where T : IParsable,new()
    {
        var batchResponse = await client.Batch.PostAsync(batchContent, token).ConfigureAwait(false);
        var responses = await batchResponse.GetResponsesAsync().ConfigureAwait(false);
        var statusCodes = await batchResponse.GetResponsesStatusCodesAsync();

        List<KeyValuePair<string, HttpResponseMessage>> pendingRetyItems = new();

        foreach (var step in responses)
        {
            var requestId = step.Key;
            var response=step.Value;
            if ((!BatchResponseContent.IsSuccessStatusCode(step.Value.StatusCode)) &&
                retryCount < RetryOption.MaxRetry &&
                batchContent.BatchRequestSteps[requestId].Request.IsBuffered() &&
                RetryOption.ShouldRetry(RetryOption.Delay, retryCount, response))
            {
                pendingRetyItems.Add(step);
            }
            else
            {
                T? result = default;
                ApiException? error = default;
                try
                {
                    //callback may need to be invoked with ConfigureAwait(true)
                    //https://devblogs.microsoft.com/dotnet/configureawait-faq/
                    result = await batchResponse.GetResponseByIdAsync<T>(requestId).ConfigureAwait(false);
                }
                catch (ApiException sEx)
                {
                    error = sEx;
                }
                callback.Invoke(requestId, result, error);
            }
        }
        // var nextlink = await batchResponse.GetNextLinkAsync().ConfigureAwait(false);

        // Currently batch seems always return full result(nextlink is null).
        // Do not known how to deal with nextlink, so throw if nextlink is not empty. This avoid data lost silently, in case graph batch support partial or async response in future.
        // Reference:
        // OData JSON Format section 19.5,19.6
        // http://docs.oasis-open.org/odata/odata-json-format/v4.01/odata-json-format-v4.01.html#sec_BatchResponse
        // https://docs.microsoft.com/en-us/graph/known-issues#json-batching
        // if (!string.IsNullOrEmpty(nextlink)) throw new InvalidDataException("Batch response contains nextlink, response may be partial.");

        if (pendingRetyItems.Any())
        {
            var retryBatch = batchContent.NewBatchWithFailedRequests(pendingRetyItems.ToDictionary(item => item.Key, item => item.Value.StatusCode));
            var retryAfters = pendingRetyItems.Select(t => t.Value.GetRetryAfterOrDefault());
            return (retryBatch, retryAfters);
        }
        return default;
    }

}

/// <summary>
/// Represents the result of a batch request without type parsing
/// </summary>
/// <param name="RequestId">The request identifier</param>
/// <param name="StatusCode">HTTP status code of the response</param>
/// <param name="Content">Raw response content as string</param>
/// <param name="Error">Exception if the request failed</param>
public record BatchResponseResult(
    string RequestId,
    HttpStatusCode StatusCode,
    string? Content,
    Exception? Error)
{
    /// <summary>
    /// Indicates whether the request was successful (2xx status code)
    /// </summary>
    public bool IsSuccessStatusCode => (int)StatusCode >= 200 && (int)StatusCode < 300;
}