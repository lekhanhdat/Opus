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

using Storage;
using System.IO.Compression;

namespace MediaDataIO;

public class ItemDataReader : IItemDataReader
{
    protected DataContextBase Context { get; set; }
    protected IXSystem System { get; set; }
    protected string DataVolume { get; set; }
    public ItemDataReader(DataContextBase itemContext, IXSystem system)
    {
        Context = itemContext;
        System = system;
    }
    /// <summary>
    /// please dispose the stream at the place you call it.
    /// </summary>
    /// <returns></returns>
    public async Task<Stream> OpenContentStreamAsync(CancellationToken cancellationToken)
    {
        return await OpenStreamAsync(Context, System, Context.ContentDataPosition, cancellationToken);
    }
    /// <summary>
    /// please dispose the stream at the place you call it.
    /// </summary>
    /// <returns></returns>
    public async Task<Stream> OpenMetadataStreamAsync(CancellationToken cancellationToken)
    {
        return await OpenStreamAsync(Context, System, Context.MetaDataPosition, cancellationToken);
    }

    private static async Task<Stream> OpenStreamAsync(DataContextBase context, IXSystem system, DataPosition position, CancellationToken cancellationToken)
    {
        Stream inner = new ChunkedReadOnlyStream(system, position, context.DataPathGenerator,context.UnDeleteSoftDeletedDataBlock);
        if (context.ItemDataMode.IsMediaEncrypted())
        {
            inner = new AesReadOnlyIvStream(inner, context.EncryptionKey);
        }
        if (context.ItemDataMode.IsZlibCompressed())
        {
            //should use zlib compress data format https://www.ietf.org/rfc/rfc1950.txt
            //https://www.ietf.org/rfc/rfc1951.txt is standard raw deflate format.we are not using it.
            inner = new ZLibStream(inner, CompressionMode.Decompress);
        }
        else if (context.ItemDataMode.IsBrotliCompressed())
        {
            inner = new BrotliStream(inner, CompressionMode.Decompress);
        }

        return await Task.FromResult(inner);
    }
}