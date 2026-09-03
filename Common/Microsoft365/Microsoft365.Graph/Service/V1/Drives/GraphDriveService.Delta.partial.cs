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


using Microsoft.Graph.Drives.Item.Items.Item.DeltaWithToken;
using Microsoft.Graph.Drives.Item.Items.Item.Delta;

//#define PAGING
namespace Microsoft365.Graph.Service;
public partial class GraphDriveService
{
    /// <summary>
    /// Track change(aka Delta) for drive items.
    /// Return an async stream of drive items
    /// </summary>
    /// <param name="driveId">Drive Id</param>
    /// <param name="token"> Delta token.
    /// If null, enumerates the hierarchy's current state. 
    /// If latest, returns empty response with latest delta token. 
    /// If a previous delta token, returns new state since that token.</param>
    /// <param name="callback"> Callback when the async stream iteration complete, break, cancel or timeout.
    /// State: Delta, iteration complete, deltalink is not empty, nextlink is empty.
    /// State: NotStarted, iteration not started yet, deltalink is empty, nextlink is empty.
    /// State: Complete, invalid.
    /// State other than above, iteration break, cancel or timeout, deltalink is empty, nextlink is not empty if first page was consumed.
    /// Note that the nextlink may be "order" than the last drive item, it happened when break iterating across the contents of a page.
    /// So if you break or cancel current iteration, and replay delta with nextlink later, make sure your application can handle duplication.
    /// </param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    [GraphAPI("/drives/{drive-id}/root/microsoft.graph.delta(token={token})")]
    public IAsyncEnumerable<DriveItem> DeltaAsync(
       string driveId,
       string? token,
       PagingCallback? callback,
       CancellationToken cancellationToken = default)
    {
        return DeltaAsync(driveId, "root", token, callback, cancellationToken);
    }

    [GraphAPI("/drives/{drive-id}/root/microsoft.graph.delta(token={token})")]
    public IAsyncEnumerable<DriveItem> DeltaAsync(
        string driveId,
        string driveItemId,
        string? token,
        PagingCallback? callback,
        CancellationToken cancellationToken = default)
    {
        driveId.ThrowIfNullOrEmpty();
        if (string.IsNullOrEmpty(token))
        {
            return DeltaWithoutTokenAsync(driveId, driveItemId, token, callback, cancellationToken);
        }
        return DeltaWithTokenAsync(driveId, driveItemId, token, callback, cancellationToken);
    }

    private IAsyncEnumerable<DriveItem> DeltaWithoutTokenAsync(string driveId, string driveItemId, string? token, PagingCallback? callback, CancellationToken cancellationToken)
    {
        return client.GetAllAsync<DriveItem, DeltaGetResponse>(
            async () =>
            {
                return await client.Drives[driveId].Items[driveItemId].Delta.GetAsDeltaGetResponseAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = PARA_DRIVEITEM_DELTA_SELECT;
                    requestConfiguration.Headers.Add("Prefer", "hierarchicalsharing");

#if PAGING //test paging
                        requestConfiguration.QueryParameters.Top = 5;
#endif
                });
            },
            (request) => { request.Headers.Add("Prefer", "hierarchicalsharing"); return request; },
            callback,
            cancellationToken);
    }

    private IAsyncEnumerable<DriveItem> DeltaWithTokenAsync(string driveId, string driveItemId, string? token, PagingCallback? callback, CancellationToken cancellationToken)
    {
        return client.GetAllAsync<DriveItem, DeltaWithTokenGetResponse>(
            async () =>
            {
                return await client.Drives[driveId].Items[driveItemId].DeltaWithToken(token).GetAsDeltaWithTokenGetResponseAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = PARA_DRIVEITEM_DELTA_SELECT;
                    requestConfiguration.Headers.Add("Prefer", "hierarchicalsharing");

#if PAGING //test paging
                    requestConfiguration.QueryParameters.Top = 5;
#endif
                });
            },
            (request) => { request.Headers.Add("Prefer", "hierarchicalsharing"); return request; },
            callback,
            cancellationToken);
    }

}
