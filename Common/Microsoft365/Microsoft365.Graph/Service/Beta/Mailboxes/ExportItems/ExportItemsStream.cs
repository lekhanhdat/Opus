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

namespace Microsoft365.Graph.Service.ExportItems;

/// <summary>
/// https://learn.microsoft.com/en-us/graph/api/resources/exportitemresponse?view=graph-rest-beta
/// This class is used to read the response stream from the ExportItems API.
/// It handles the JSON response and extracts the data property with fixed buffer size.
/// It also handles the case where the response contains an error.
/// </summary>
internal class ExportItemsStream : Stream
{
    private const byte DOUBLE_QUOTE = 34;// ASCII code for double quote
    private const int BUFFER_SIZE = 4096;// 4 KB
    private const int MAX_BUFFER_SIZE = 16 * 1024 * 1024;// 16 MB
    private readonly Stream innerStream;
    private Memory<byte> cacheBuffer = Memory<byte>.Empty;
    private long position = 0;
    private bool endOfStream = false;
    private bool disposed = false;

    public ExportItemsStream(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        this.innerStream = new BufferedStream(stream);
        ReadToDataProperty();
    }

    public override bool CanRead => innerStream.CanRead;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => position; set => throw new NotSupportedException(); }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override void Flush() { }

    public override int Read(Span<byte> buffer)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (endOfStream) return 0;

        if (cacheBuffer.Length > 0)
        {
            int bytesToRead = CopyFromBuffer(buffer);
            return AdjustBytesRead(buffer[..bytesToRead]);
        }
        else
        {
            var bytesRead = innerStream.Read(buffer);
            if (bytesRead == 0)
            {
                throw new InvalidDataException("Cannot find ending double quote util EOF");
            }
            return AdjustBytesRead(buffer[..bytesRead]);
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return Read(buffer.AsSpan(offset, count));
    }
    /// <summary>
    /// ReadToDataProperty reads the JSON stream until it finds the "data" property.
    /// It handles the case where the stream contains an error response.
    /// </summary>
    /// <exception cref="ODataError"></exception>
    /// <exception cref="InvalidDataException"></exception>
    private void ReadToDataProperty()
    {
        GetInitalBytesFromStream(out var buffer, out var reader);

        string? currentProperty = null;
        while (buffer.Length > 0)
        {
            try
            {
                if (reader.TokenType == JsonTokenType.PropertyName && !string.IsNullOrEmpty(reader.GetString()))
                {
                    currentProperty = reader.GetString()?.ToLowerInvariant();
                }
                switch (currentProperty)
                {
                    case "data":
                        MoveToDataValue(buffer, reader);
                        return;
                    case "error":
                        var error = ReadMailTipsError(ref reader);
                        throw new GraphBetaODataErrors.ODataError() { Error = new() { Message = error.Message, Code = error.Code } };

                    default:
                        break;
                }
                if (!reader.Read())
                {
                    // Not enough of the JSON is in the buffer to complete a read.
                    GetMoreBytesFromStream(innerStream, ref buffer, ref reader);
                }
            }
            catch (JsonException ex) when (ex.Message.Contains("There is not enough data to read"))
            {
                // Handle the case where the JSON is incomplete
                // and we need to read more bytes from the stream.
                GetMoreBytesFromStream(innerStream, ref buffer, ref reader);
            }
        }
        throw new InvalidDataException("Cannot find the required property name in the JSON stream.");
    }

    private void GetInitalBytesFromStream(out Span<byte> buffer, out Utf8JsonReader reader)
    {
        buffer = new byte[BUFFER_SIZE];
        var read = innerStream.Read(buffer);
        if (read <= 0)
        {
            throw new InvalidDataException("Emtpy stream");
        }
        if (read < BUFFER_SIZE)
        {
            buffer = buffer[..read];
        }
        // We set isFinalBlock to false since we expect more data in a subsequent read from the stream.
        //https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/use-utf8jsonreader
        reader = new Utf8JsonReader(buffer, isFinalBlock: false, state: default);
    }

    private static MailTipsError ReadMailTipsError(ref Utf8JsonReader reader)
    {
        var error = JsonSerializer.Deserialize<Error>(ref reader)!;

        return new MailTipsError()
        {
            Code = error.Code,
            Message = error.Message
        };
    }

    /// <summary>
    /// Moves the reader position to the beginning of a data value in a JSON stream.
    /// </summary>
    /// <param name="buffer">The current buffer of bytes being processed.</param>
    /// <param name="reader">The JSON reader that is consuming the buffer.</param>
    /// <remarks>
    /// This method has two main behaviors:
    /// 1. If the reader hasn't consumed all bytes in the buffer, it looks for a quote character in the leftover bytes.
    ///    If found, it caches the remainder of the buffer after the quote.
    /// 2. If the reader has consumed all bytes or no quote is found in the leftover bytes, it reads directly from
    ///    the inner stream until it finds a double quote character or reaches the end of the stream.
    /// </remarks>
    /// <exception cref="InvalidDataException">Thrown when the end of the stream is reached without finding a quote character.</exception>
    private void MoveToDataValue(ReadOnlySpan<byte> buffer, Utf8JsonReader reader)
    {
        if (SkipUtilQuoteFromBuffer(buffer, reader)) return;
        SkipUtilQuoteFromStream();
    }

    private void SkipUtilQuoteFromStream()
    {
        // Skip bytes until we find the opening quote
        int byteValue;
        while ((byteValue = innerStream.ReadByte()) != -1)
        {
            if (byteValue == DOUBLE_QUOTE)
            {
                // Found the opening quote, now return
                return;
            }
            // Continue skipping bytes
        }
        // End of stream reached without finding the quote
        throw new InvalidDataException("Unexpected end of stream: Unable to find the required double quote before reaching EOF.");
    }

    private bool SkipUtilQuoteFromBuffer(ReadOnlySpan<byte> buffer, Utf8JsonReader reader)
    {
        if (reader.BytesConsumed < buffer.Length)// The reader has not consumed all the bytes in the buffer.
        {
            ReadOnlySpan<byte> leftover = buffer[(int)reader.BytesConsumed..];
            var index = IndexOfQuote(leftover);
            // if there is a quote in the leftover buffer, we need to move the buffer to the start of the quote
            // and return the rest of the buffer as cacheBuffer

            if (index >= 0)
            {
                cacheBuffer = leftover[(index + 1)..].ToArray();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Reads more bytes from the stream into the buffer and creates a new JsonReader instance
    /// to continue parsing from where the previous reader left off.
    /// </summary>
    /// <param name="stream">The source stream to read additional bytes from.</param>
    /// <param name="buffer">A reference to the current buffer that will be filled with more data.
    /// If the reader hasn't consumed any bytes and we need more, the buffer size may be doubled.</param>
    /// <param name="reader">A reference to the JSON reader that will be recreated with the updated buffer and state.</param>
    /// <remarks>
    /// This method handles two main scenarios:
    /// 1. When the reader has partially consumed the buffer, it preserves unconsumed bytes and appends new data.
    /// 2. When the reader hasn't consumed any bytes (likely due to an incomplete token), it may increase the buffer size.
    /// </remarks>
    private static void GetMoreBytesFromStream(Stream stream, ref Span<byte> buffer, ref Utf8JsonReader reader)
    {
        int read;
        var bytesConsumed = reader.BytesConsumed;
        if (bytesConsumed < buffer.Length)// The reader has not consumed all the bytes in the buffer.
        {
            ReadOnlySpan<byte> leftover = buffer[(int)bytesConsumed..];

            if (bytesConsumed == 0)
            {
                if (buffer.Length * 2 > MAX_BUFFER_SIZE)
                {
                    throw new InvalidDataException("Buffer size exceeded maximum limit.");
                }
                // No bytes consumed, so we need double the buffer size
                buffer = new byte[buffer.Length * 2];
            }

            leftover.CopyTo(buffer);
            read = stream.ReadAtLeast(buffer[leftover.Length..], buffer.Length - leftover.Length, false);
            buffer = buffer[..(leftover.Length + read)];
        }
        else
        {
            read = stream.ReadAtLeast(buffer, buffer.Length, false);
            buffer = buffer[..read];
        }
        reader = new Utf8JsonReader(buffer, isFinalBlock: read == 0, reader.CurrentState);
    }

    private int CopyFromBuffer(Span<byte> to)
    {
        var from = cacheBuffer;
        var bytesToRead = Math.Min(from.Length, to.Length);
        from[..bytesToRead].Span.CopyTo(to);
        cacheBuffer = from[bytesToRead..];
        return bytesToRead;
    }
    /// <summary>
    /// Adjusts the number of bytes read from the buffer based on the position of the double quote.
    /// If a double quote is found, it updates the position and sets endOfStream to true.
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    private int AdjustBytesRead(Span<byte> buffer)
    {
        int index = IndexOfQuote(buffer);
        if (index >= 0)
        {
            // leftoverBuffer = buffer[(index + 1)..].ToArray();
            endOfStream = true;
            position += index;
            return index;
        }
        position += buffer.Length;
        return buffer.Length;
    }

    private static int IndexOfQuote(ReadOnlySpan<byte> buffer)
    {
        return buffer.IndexOf(DOUBLE_QUOTE);
        // 34 is the ASCII code for double quote
    }

    protected override void Dispose(bool disposing)
    {
        if (disposed) return;
        if (disposing)
        {
            innerStream.Dispose();
            cacheBuffer = Memory<byte>.Empty;
        }
        disposed = true;
    }


    // Internal class for error response
    internal class Error
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }
}