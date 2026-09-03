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

namespace MediaDataIO;

using AvePoint.RA.CommonUtil;
using System;
using System.Threading.Tasks;


public class IndexReadOnlyIvStream : AesReadOnlyIvStream
{
    private static readonly RALogger logger = RALogger.GetInstance(typeof(IndexReadOnlyIvStream));

    private Stream header;

    public const int HEADER_LENGTH = 4 * 1024;
    public const int FIXED_HEADER_LENGTH = 128;

    public IndexReadOnlyIvStream(Stream encryptedStream, byte[] key) : base(encryptedStream, key)
    {
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (FirstRead)
        {
            var headerBuffer = new byte[IndexFileHeader.HEADER_LENGTH];
            await EncryptedStream.ReadAtLeastAsync(headerBuffer, headerBuffer.Length, true, cancellationToken);

            try
            {
                var indexFileHeader = new IndexFileHeader(headerBuffer);
                if (indexFileHeader.Encrypted)
                {
                    var tempBuffer = new byte[Aes.BlockSize / 8];
                    await EncryptedStream.ReadAtLeastAsync(tempBuffer, tempBuffer.Length, true, cancellationToken);
                    Aes.IV = tempBuffer;
                    InternalStream = new CryptoStream(EncryptedStream, Aes.CreateDecryptor(), CryptoStreamMode.Read);
                }
                else
                {
                    throw new InvalidOperationException("Unreachable code.");
                }
            }
            catch (ArgumentException aEx)
            {
                logger.Warn("The index file is not encrypted, error: {0}.", aEx);
                if (EncryptedStream.CanSeek)
                {
                    EncryptedStream.Seek(0, SeekOrigin.Begin);
                }
                else
                {
                    header = new MemoryStream(headerBuffer, 0, headerBuffer.Length);
                }
                InternalStream = EncryptedStream;
            }

            FirstRead = false;
        }
        return await InternalReadAsync(buffer, cancellationToken);

        async ValueTask<int> InternalReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var remainCacheLength = header is null ? 0 : header.Length - header.Position;
            if (remainCacheLength <= 0)
            {
                return await InternalStream.ReadAsync(buffer, cancellationToken);
            }
            else
            {
                if (remainCacheLength >= buffer.Length)
                {
                    return await header.ReadAsync(buffer, cancellationToken);
                }
                else
                {
                    var read = await header.ReadAsync(buffer, cancellationToken);
                    read += await InternalStream.ReadAsync(buffer[read..], cancellationToken);
                    return read;
                }
            }
        }
    }

    public async override ValueTask DisposeAsync()
    {
        if (header is not null)
        {
            await header.DisposeAsync();
            header = null;
        }
        await base.DisposeAsync();
    }
}
