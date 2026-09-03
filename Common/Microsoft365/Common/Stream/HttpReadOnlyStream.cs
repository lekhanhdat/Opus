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
#nullable enable
namespace Microsoft365.Common
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class HttpReadOnlyStream : Stream
    {
        private HttpResponseMessage response;
        private Stream innerStream;
        public HttpReadOnlyStream(HttpResponseMessage response, Stream stream)
        {
            this.response = response ?? throw new ArgumentNullException(nameof(response));
            this.innerStream = stream ?? throw new ArgumentNullException(nameof(stream));
            //DO NOT dispose innerStream in CancellationToken callback
            //cancellationToken.Register(() => this.innerStream?.Dispose());
            //对于ContentLengthReadStream和ChunkedEncodingReadStream，
            //1.如果在ReadAsync过程中，dispose掉stream，会抛异常。
            //   System.IO.IOException: The read operation failed, see inner exception.
            //     ---> System.ObjectDisposedException: Cannot access a disposed object.
            //    Object name: 'SslStream'.
            //2.如果是两次ReadAsync之间，dispose掉stream, stream.ReadAsync()不会抛异常，而是返回0，返回0则意味着已到达流结尾。业务层会正常退出，并且只下载了不完整的流，而不是超时报错。
            
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => this.response?.Content?.Headers?.ContentLength ?? throw new NotImplementedException();

        public override long Position { get => this.innerStream.Position; set => throw new System.NotImplementedException(); }

        public string? ETag => this.response.Headers.ETag?.ToString();

        public override void Flush()
        {
            this.innerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.innerStream.Read(buffer, offset, count);
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return await this.innerStream.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return await this.innerStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
        }

        public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            await this.innerStream.CopyToAsync(destination, bufferSize, cancellationToken).ConfigureAwait(false);
        }

        public override void CopyTo(Stream destination, int bufferSize)
        {
            this.innerStream.CopyTo(destination, bufferSize);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.innerStream?.Dispose();
                this.response?.Dispose();
                this.innerStream = null;
                this.response = null;
            }
            base.Dispose(disposing);
        }
    }
}