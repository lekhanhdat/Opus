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
namespace Microsoft365.Common.Service;

public class DownloadService
{
    private readonly HttpClient Client;

    private readonly ResponseHandler responseHandler;

    public DownloadService(HttpClient client, ResponseHandler responseHandler = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        this.Client = client;
        this.responseHandler = responseHandler ?? new ResponseHandler();
    }

    /// <summary>
    /// Download file content from a preauthencticated url
    /// </summary>
    /// <param name="getDownloadUrl">Get preauthenticated url, string getDownloadUrl(bool force). Force=true means temptoken in url expired</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Stream> OpenStreamAsync(Func<bool, Task<string>> getDownloadUrl, CancellationToken cancellationToken)
    {
        var stream = await GetContentAsync(
            getDownloadUrl,
            range: null,
            eTag: null,
            cancellationToken).ConfigureAwait(false);
        var eTag = (stream as HttpReadOnlyStream)!.ETag;
        return RetriableStream.Create(
            stream,
            offset => GetContentAsync(
                getDownloadUrl,
                new RangeHeaderValue(offset, null),
                eTag,
                cancellationToken));
    }
    /// <summary>
    /// Download file content
    /// </summary>
    /// <param name="downloadUrl">download url</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Stream> OpenStreamAsync(string downloadUrl, CancellationToken cancellationToken)
    {
        return await OpenStreamAsync(force => Task.FromResult(downloadUrl), cancellationToken);
    }

    internal async Task<Stream> GetContentAsync(
        Func<bool, Task<string>> getDownloadUrl,
        RangeHeaderValue? range,
        string? eTag,
        CancellationToken cancellationToken)
    {
        var response = await GetResponseAsync(
            await getDownloadUrl(false).ConfigureAwait(false),
            range,
            eTag,
            cancellationToken).ConfigureAwait(false);
        if (response.IsUnauthorized())
        {
            //Drain response to reset connection.
            await response.DrainAsync(cancellationToken).ConfigureAwait(false);
            //get download url with force=true to refreash token in url.
            response = await GetResponseAsync(
                await getDownloadUrl(true).ConfigureAwait(false),
                range,
                eTag,
                cancellationToken).ConfigureAwait(false);
        }
        return await responseHandler.HandleResponseAsync(response, cancellationToken).ConfigureAwait(false);
    }

    private async Task<HttpResponseMessage> GetResponseAsync(string downloadUrl, RangeHeaderValue? range, string? eTag, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
        request.Headers.Add("If-Match", eTag ?? string.Empty);
        if (range != null)
        {
            request.Headers.Add("range", range?.ToString() ?? string.Empty);
        }

        var response = await Client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        return response;
    }

    public class ResponseHandler
    {
        public virtual async Task<Stream> HandleResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            return await ToHttpStream(response, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<HttpReadOnlyStream> ToHttpStream(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(response);
            response.EnsureSuccessStatusCode();
            return new HttpReadOnlyStream(
                response,
                await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false));
        }
    }
}
