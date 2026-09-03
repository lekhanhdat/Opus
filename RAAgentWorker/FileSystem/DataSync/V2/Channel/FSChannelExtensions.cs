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
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.DataSync.V2
{
    public static class FSChannelExtensions
    {
        public static async Task WriteWithRetryAsync<T>(this ChannelWriter<T> writer, T item, CancellationToken token)
        {
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    await writer.WriteAsync(item, token).ConfigureAwait(false);
                    return;
                }
                catch (ChannelClosedException)
                {
                    throw;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception) when (attempt < 3)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(200), token).ConfigureAwait(false);
                }
            }
            throw new InvalidOperationException("WriteAsync failed after retries.");
        }

        public static async Task WriteBatchWithRetryAsync<T>(this ChannelWriter<T> writer, List<T> items, int batchSize, CancellationToken token)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            if (items == null || items.Count == 0) return;

            if (batchSize <= 0) batchSize = 100;

            int count = items.Count;
            int index = 0;

            try
            {
                while (index < count)
                {
                    token.ThrowIfCancellationRequested();
                    int currentBatchSize = Math.Min(batchSize, count - index);
                    for (int i = 0; i < currentBatchSize; i++)
                    {
                        await WriteWithRetryAsync(writer, items[index + i], token).ConfigureAwait(false);
                    }
                    index += currentBatchSize;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
        }

        public static async Task DrainChannelAsync<T>(this ChannelReader<T> reader, Func<T, Task> handlerAsync, CancellationToken token)
        { 
            if (reader == null) throw new ArgumentNullException(nameof(reader)); 
            if (handlerAsync == null) throw new ArgumentNullException(nameof(handlerAsync));
            while (await reader.WaitToReadAsync(token).ConfigureAwait(false))
            {
                while (reader.TryRead(out T item))
                {
                    token.ThrowIfCancellationRequested();
                    await handlerAsync(item).ConfigureAwait(false);
                }
            }
        }

        public static async Task DrainChannel<T>(this ChannelReader<T> reader, Action<T> handler, CancellationToken token)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            while (await reader.WaitToReadAsync(token).ConfigureAwait(false))
            {
                while (reader.TryRead(out T item))
                {
                    token.ThrowIfCancellationRequested();
                    handler(item);
                }
            }
        }
    }
}
