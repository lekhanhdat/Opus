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

namespace AvePoint.Metadata
{
    using AvePoint.RA.CommonUtil;

    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class CoordinatedStream : Stream, IDisposable
    {
        private static RALogger logger = RALogger.GetInstance(typeof(CoordinatedStream));
        protected string CacheDirectory { get; set; }
        private int StreamSizeLimit = 64 * 1024;
        private Stream innerStream;
        private bool mExplictlyClose = false;
        private readonly string mInternalName;
        private string mFileName = string.Empty;
        private string mTempPath = string.Empty;
        private readonly long order;

        /// <summary>
        /// 调用此类时，需要注意当Stream大于64K时，需要手动将Stream翻译，否则会遗留临时文件
        /// </summary>
        /// <param name="fileName"></param>
        public CoordinatedStream(string internalName, string cacheDirectory, long order = 0, bool explictlyClose = false, int streamSizeLimit = 64 * 1024)
        {
            ArgumentNullException.ThrowIfNull(cacheDirectory);
            innerStream = new MemoryStream(StreamSizeLimit);
            this.mExplictlyClose = explictlyClose;
            this.mInternalName = internalName;
            StreamSizeLimit = streamSizeLimit;
            this.order = order;
            CacheDirectory = cacheDirectory;
        }

        public CoordinatedStream(string internalName = null, long order = 0, bool explictlyClose = false, int streamSizeLimit = 64 * 1024) : this(internalName, Path.GetTempPath(), order, explictlyClose, streamSizeLimit) { }


        public bool IsExplictlyClose
        {
            get
            {
                return mExplictlyClose;
            }
        }

        public long Order
        {
            get { return order; }
        }

        private string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(this.mFileName))
                {
                    this.mFileName = mInternalName + Guid.NewGuid().ToString();
                }
                return this.mFileName;
            }
        }

        public override bool CanRead
        {
            get
            {
                return innerStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return innerStream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return innerStream.CanWrite;
            }
        }

        public override void Flush()
        {
            innerStream.Flush();
        }

        public override long Length
        {
            get
            {
                return innerStream.Length;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            innerStream.SetLength(value);
        }

        public override long Position
        {
            get
            {
                return innerStream.Position;
            }
            set
            {
                innerStream.Position = value;
            }
        }

        //bind some object here, ie:report
        public object Attachment
        {
            get;
            set;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return await ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
        }
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return await innerStream.ReadAsync(buffer, cancellationToken);
        }
        public override int Read(Span<byte> buffer)
        {
            return innerStream.Read(buffer);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return innerStream.Read(buffer.AsSpan(offset, count));
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
            }
            if (innerStream == null)
            {
                innerStream = new MemoryStream(StreamSizeLimit);
            }
            if (innerStream.GetType().Equals(typeof(MemoryStream)))
            {
                if (innerStream.Position + count < StreamSizeLimit)
                {
                    innerStream.Write(buffer, offset, count);
                    return;
                }
                //Translate memory stream to FileStream
#if DEBUG
                logger.Info($"[{mInternalName}]Translate MemoryStream to FileStream.SizeLimit:{StreamSizeLimit},Id:{FileName}");
#endif
                mTempPath = Path.Combine(CacheDirectory, this.FileName);
                if (!Directory.Exists(CacheDirectory))
                {
                    Directory.CreateDirectory(CacheDirectory);
                    logger.Warn($"Create direcoty {CacheDirectory} since it doesn't exist.");
                }
                FileStream largeContent = new FileStream(mTempPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                if (innerStream.Length > 0 && innerStream.Position >= 0)
                {
                    byte[] tmpBuffer = (innerStream as MemoryStream).GetBuffer();
                    largeContent.Write(tmpBuffer, 0, (int)innerStream.Position);
                }
                innerStream = largeContent;
            }
            innerStream.Write(buffer, offset, count);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public bool IsEndOfStream
        {
            get { return (this.Position == this.Length); }
        }

        protected override void Dispose(bool disposing)
        {
            if (!mExplictlyClose)
            {
                ExplictlyClose();
                mExplictlyClose = true;
            }
        }

        public void ExplictlyCloseAndKeepFile()
        {
            if (innerStream != null)
            {
                innerStream.Dispose();
                innerStream = null;
                //CleanStreamFile();
                logger.Warn("The file:{0} is kept for future analysis", mTempPath);
            }
            base.Dispose(true);
        }

        public void ExplictlyClose()
        {
            if (innerStream != null)
            {
                innerStream.Dispose();
                innerStream = null;
                CleanStreamFile();
            }
            base.Dispose(true);
        }

        private void CleanStreamFile()
        {
            if (!string.IsNullOrEmpty(mTempPath) && System.IO.File.Exists(mTempPath))
            {
                //log.Debug("start to delete file:{0}", filePath);
                System.IO.File.Delete(mTempPath);
            }
        }

        public override string ToString()
        {
            return mFileName;
        }

        public void ReopenFileStream()
        {
            if (innerStream.GetType().Equals(typeof(FileStream)) && !string.IsNullOrEmpty(mTempPath) && File.Exists(mTempPath))
            {
                innerStream.Dispose();
                innerStream = new FileStream(mTempPath, FileMode.Truncate, FileAccess.ReadWrite);
                logger.Info($"Reopen temp file {mTempPath} with truncate mode.");
            }
        }
    }
}