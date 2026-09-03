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
namespace Microsoft365.Common;
public static class RetriableStream
{
    /// <summary>
    /// Create a retriable stream, usually used for reading stream from poor network
    /// </summary>
    /// <param name="initialStream">Initial inner stream</param>
    /// <param name="getStreamAsync">position=>new Stream(from that position), optional validate ETag or ModifiedTime in case file was Modified.</param>
    /// <param name="maxRetries">max retries, default is 2</param>
    /// <returns></returns>
    public static Stream Create(
        Stream initialStream,
        Func<long, Task<Stream>> getStreamAsync,
        int maxRetries = 4)
    {
        return new RetriableStreamImpl(initialStream, getStreamAsync, maxRetries);
    }

    private class RetriableStreamImpl : Stream
    {
        //private static readonly ICloudBackupLogger logger = CloudBackupLogManager.Get(typeof(RetriableStreamImpl));
        private static readonly TimeSpan internalTimeout = TimeSpan.FromSeconds(240);
        private readonly Func<long, Task<Stream>> getStreamAsync;

        private readonly int maxRetries;

        private readonly long? length;

        private Stream currentStream;

        private long position;

        private int retryCount;

        private List<System.Exception> exceptions;

        public RetriableStreamImpl(Stream initialStream, Func<long, Task<Stream>> getStreamAsync, int maxRetries)
        {
            try
            {
                length = EnsureStream(initialStream).Length;
            }
            catch
            {
                // ignore
            }

            this.currentStream = EnsureStream(initialStream);
            this.getStreamAsync = getStreamAsync;
            this.maxRetries = maxRetries;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            while (true)
            {
                // 调用者给CancellationToken设置了足够大超时，仍然会遇到一些OperationCanceledException或TaskCanceledExcdeption。
                // 请阅读下面的文章。这里使用一个internal canceltoken对这种情况进行重试。
                // https://git.avepoint.net/dl_devops/cloud-documentation/cloud-backup-docs/-/blob/master/release-docs/cloud-backup-March-2023/AOSBR-33583-QinglongLuo/httpclient-hang-poor-network.md
                using var internalCTS = new CancellationTokenSource(internalTimeout);
                using var linkedRCTS = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, internalCTS.Token);
                try
                {
                    var read = await this.currentStream.ReadAsync(buffer, linkedRCTS.Token).ConfigureAwait(false);
                    this.position += read;
                    return read;
                }
                // Do not retry when user cancellation
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (System.Exception e) when (ShouldRetry(e))
                {
                    await RetryAsync(e).ConfigureAwait(false);
                }
            }
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return await ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
        }

        private async Task RetryAsync(System.Exception exception)
        {
            this.exceptions ??= [];
            this.exceptions.Add(exception);

            this.retryCount++;

            if (this.retryCount > this.maxRetries)
            {
                throw new AggregateException($"Retry failed after {this.retryCount} tries", this.exceptions);
            }

            //logger.Warn($"Retry from position {position}, error:{exception}");
            await this.currentStream.DisposeAsync();

            try
            {
                this.currentStream = EnsureStream(await getStreamAsync(position).ConfigureAwait(false));
            }
            catch (HttpRequestException hrEx) when (hrEx.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                //logger.Warn("[PreconditionFailed] Throw the original exception.");
                //throw original exception for 412 PreconditionFailed
                throw exception;
            }
        }

        private static bool ShouldRetry(System.Exception exception)
        {
            //此处暂时给默认实现。理论上只要底层使用HttpClient，在读取网络流上并无不同，故暂时不支持更灵活的配置。
            return exception.IsSocketOrIOException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("Use ReadAsync instead");
        }

        public override bool CanRead => this.currentStream.CanRead;
        public override bool CanSeek => false;
        public override long Length => length ?? throw new NotSupportedException();

        public override long Position
        {
            get => position;
            set => throw new NotSupportedException();
        }

        private static Stream EnsureStream(Stream stream)
        {
            if (stream == null)
            {
                throw new InvalidOperationException("The response didn't have content");
            }

            return stream;
        }

        public override bool CanWrite => false;

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
            // Flush is allowed on read-only stream
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.currentStream?.Dispose();
        }
    }
}
