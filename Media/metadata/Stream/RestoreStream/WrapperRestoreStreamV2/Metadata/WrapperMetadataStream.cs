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

namespace AvePoint.Metadata;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class WrapperMetadataStream : Stream
{
    private readonly Stream internalStream;
    public HeaderV1 Header { get; protected set; }
    public WrapperMetadataStream(Stream stream)
    {
        internalStream = stream;
        Header = SkipHeader();
    }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotImplementedException();

    public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count) => ReadAsync(new Memory<byte>(buffer, offset, count), CancellationToken.None).Result;

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => await internalStream.ReadAsync(buffer, cancellationToken);

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => await ReadAsync(new Memory<byte>(buffer, offset, count), CancellationToken.None);

    public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

    public override void SetLength(long value) => throw new NotImplementedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

    public override async ValueTask DisposeAsync()
    {
        if (internalStream is not null)
        {
            await internalStream.DisposeAsync();
        }
        await base.DisposeAsync();
    }

    private HeaderV1 SkipHeader()
    {
        var buffer = new byte[AveMetadataConstants.HEADER_SIZE];
        if (internalStream.Read(buffer, 0, buffer.Length) > 0)
        {
            var major = buffer[8];
            switch (major)
            {
                case 1:
                    var newBuffer = new byte[HeaderV1.HEADER_LENGTH];
                    buffer.CopyTo(newBuffer, 0);
                    internalStream.ReadEx(newBuffer, AveMetadataConstants.HEADER_SIZE, HeaderV1.HEADER_LENGTH - AveMetadataConstants.HEADER_SIZE);
                    return new HeaderV1(newBuffer);
                case 0:
                default:
                    return new HeaderV0(buffer).ToV1Header();
            }
        }
        throw new InvalidDataException();
    }
}
