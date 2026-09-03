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
using System.Drawing;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;

using AvePoint.Metadata;

using log4net.Repository.Hierarchy;

using MediaDataIO;

using AvePoint.RA.CommonUtil;

namespace AvePoint.Metadata;
public class RestoreStream : IRestoreStream
{
    private static RALogger logger = RALogger.GetInstance(typeof(RestoreStream));
    protected IItemDataReader DataReader { get; set; }
    protected Lazy<IAveMetadataReader> MetadataReader { get; set; }
    protected Lazy<WrapperMetadataStream> WrapperMetadataStream { get; set; }
    protected Lazy<CoordinatedStream> CacheStream { get; set; }

    public Int64 Size { get { return GetReadSize(WrapperMetadataStream); } }

    public Int64 ContentSize { get { return WrapperMetadataStream.Value.Header.ContentLength; } }
    public RestoreStream(IItemDataReader itemDataReader, String cacheDirectory)
    {
        DataReader = itemDataReader;
        CacheStream = new Lazy<CoordinatedStream>(() =>
        {
            var cache = new CoordinatedStream("MetadataStream", cacheDirectory, 0, true, 250 * 1024 * 1024);
            using (var stream = DataReader.OpenMetadataStreamAsync().ConfigureAwait(false).GetAwaiter().GetResult())
            {
                stream.CopyTo(cache);
            }
            cache.Position = 0;
            return cache;
        });
        WrapperMetadataStream = new Lazy<WrapperMetadataStream>(() => { return new WrapperMetadataStream(CacheStream.Value); });
        MetadataReader = new Lazy<IAveMetadataReader>(() => { return new AveMemoryMetadataReader(WrapperMetadataStream.Value); });
    }

    private static Int64 GetReadSize(Lazy<WrapperMetadataStream> streamWrapper)
    {
        if (streamWrapper?.IsValueCreated ?? false)
        {
            return streamWrapper.Value.Header.MetadataLength + streamWrapper.Value.Header.ContentLength;
        }
        logger.Warn("Stream is not initiallized,will return 0 size.");
        return 0;
    }

    /// <summary>
    /// should be disposed by the caller
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Stream OpenContentStream()
    {
        if (ContentSize == 0)
        {
            return new MemoryStream();
        }
        return OpenContentStreamAsync(default).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <summary>
    /// should be disposed by the caller
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async ValueTask<Stream> OpenContentStreamAsync(CancellationToken cancellationToken)
    {
        if (ContentSize == 0)
        {
            return await Task.FromResult(new MemoryStream());
        }
        return await DataReader.OpenContentStreamAsync(cancellationToken);
    }

    public AveMetadata ReadMetadata()
    {
        return MetadataReader.Value.ReadMetadata();
    }

    public AveMetadata TryReadMetadata(AveMetadataType metadataName)
    {
        return MetadataReader.Value.TryReadMetadata(metadataName);
    }

    public List<AveMetadata> TryReadMetadataList(AveMetadataType metadataName)
    {
        return MetadataReader.Value.TryReadMetadataList(metadataName);
    }

    public void Dispose()
    {
        if (MetadataReader?.IsValueCreated ?? false)
        {
            MetadataReader?.Value?.Dispose();
            MetadataReader = null;
        }
        if (WrapperMetadataStream?.IsValueCreated ?? false)
        {
            WrapperMetadataStream?.Value?.Dispose();
            WrapperMetadataStream = null;
        }
        if (CacheStream?.IsValueCreated ?? false)
        {
            CacheStream.Value?.ExplictlyClose();
            CacheStream = null;
        }
        DataReader = null;
    }
}