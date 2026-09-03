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

namespace Microsoft365.Graph.Service.ImportItems;
internal class ImportItemPostStream : Stream
{
    private readonly ImportItemPostRequestBody request;
    private ReadOnlyMemory<byte> beforeData;
    private ReadOnlyMemory<byte> afterData;
    private readonly Stream base64Data;
    private bool eof;
    private bool disposed;

    public ImportItemPostStream(ImportItemPostRequestBody requestBody)
    {
        request = requestBody.EnsureIfNotNull();
        base64Data = new CryptoStream(requestBody.DataStream.EnsureIfNotNull(), new ToBase64Transform(), CryptoStreamMode.Read, true);
        WriteProperties();
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return Read(buffer.AsSpan(offset, count));
    }

    public override int Read(Span<byte> buffer)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (TryReadFromBeforeDataBuffer(buffer, out int read)) return read;
        if (TryReadFromDataStream(buffer, out read)) return read;
        if (TryReadFromAfterDataBuffer(buffer, out read)) return read;
        return 0;
    }

    private bool TryReadFromDataStream(Span<byte> buffer, out int read)
    {
        if (eof)
        {
            read = 0;
            return false;
        }
        read = base64Data.Read(buffer);
        eof = read <= 0;
        return read > 0;

    }

    private bool TryReadFromAfterDataBuffer(Span<byte> buffer, out int read)
    {
        if (afterData.IsEmpty)
        {
            read = 0;
            return false;
        }

        read = Math.Min(buffer.Length, afterData.Length);
        afterData.Span[..read].CopyTo(buffer);
        afterData = afterData[read..];
        return read > 0;
    }

    private bool TryReadFromBeforeDataBuffer(Span<byte> buffer, out int read)
    {
        if (beforeData.IsEmpty)
        {
            read = 0;
            return false;
        }

        read = Math.Min(buffer.Length, beforeData.Length);
        beforeData.Span[..read].CopyTo(buffer);
        beforeData = beforeData[read..];
        return read > 0;
    }

    private void WriteProperties()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new Utf8JsonWriter(buffer);
        writer.WriteStartObject();
        WriteStringValue("FolderId", request.FolderId);
        WriteStringValue("Mode", EnumHelpers.GetEnumStringValue(request.Mode.GetValueOrDefault()));
        WriteStringValue("ItemId", request.ItemId);
        WriteStringValue("ChangeKey", request.ChangeKey);
        WriteStringValue("Data", "");
        writer.WriteEndObject();
        writer.Flush();

        beforeData = buffer.WrittenMemory[..^2];
        afterData = buffer.WrittenMemory[^2..];

        void WriteStringValue(string? key, string? value)
        {
            if (value != null)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    writer.WritePropertyName(key);
                }
                writer.WriteStringValue(value);
            }
        }

    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposed) return;
        if (disposing)
        {
            base64Data.Dispose();
        }
        disposed = true;
    }
}