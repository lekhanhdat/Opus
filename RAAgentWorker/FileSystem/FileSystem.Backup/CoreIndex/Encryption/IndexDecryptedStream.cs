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


namespace AvePoint.Media.Core.Index
{
    using GCommon;
    using GCommon.Contract.Server.ControlPanel.Cryptography;
    using System;
    using System.IO;

    public class IndexDecryptedStream : Stream
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(IndexDecryptedStream));
        private Stream innerStream;
        private Stream header;

        public IndexDecryptedStream(Stream inner, DataEncryptionInfo info)
        {
            Init(inner, info);
        }

        private void Init(Stream inner, DataEncryptionInfo info)
        {
            byte[] headerBuffer;
            int count = ReadHeader(inner, out headerBuffer);
            try
            {
                var header = new IndexFileHeader(headerBuffer);
                if (header.Encrypted)
                {
                    ValidateEncryptionInfo(info, header);
                    this.innerStream = new GCommon.Utility.FilteringBox.FilteringStream.EncryptedInputStream(inner, info);
                    return;
                }
                else
                {
                    throw new InvalidOperationException("Unreachable code.");
                }
            }
            catch (ArgumentException aEx)
            {
                logger.Warn("The index file is not encrypted, error: {0}", aEx);
            }

            this.innerStream = inner;
            this.header = new MemoryStream(headerBuffer, 0, count);
        }
        private static void ValidateEncryptionInfo(DataEncryptionInfo info, IndexFileHeader header)
        {
            logger.Info($"Index header: {header}");
            if (info == null) throw new InvalidDataException("Index data base is encrypted, but decryption info is null.");
            logger.Info($"DataEncryptionInfo from control     : {info}");
        }

        private static int ReadHeader(Stream inner, out byte[] buffer)
        {
            buffer = new byte[IndexFileHeader.HEADER_LENGTH];
            int read;
            int offset = 0;
            while ((read = inner.Read(buffer, offset, buffer.Length - offset)) > 0)
            {
                offset += read;
                if (offset == buffer.Length) break;
            }
            return offset;
        }

        public override bool CanRead
        {
            get
            {
                return this.innerStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override long Length
        {
            get
            {
                return this.innerStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return this.innerStream.Position;
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        public override void Flush()
        {
            this.innerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int remainCacheLength = RemainCacheLength();
            if (remainCacheLength <= 0)
            {
                return this.innerStream.Read(buffer, offset, count);
            }
            else
            {
                return ReadFromCache(buffer, offset, count, remainCacheLength);
            }

        }

        private int ReadFromCache(byte[] buffer, int offset, int count, int remainCacheLength)
        {
            if (remainCacheLength >= count)
            {
                return this.header.Read(buffer, offset, count);
            }
            else
            {
                int read = this.header.Read(buffer, offset, remainCacheLength);
                read += this.innerStream.Read(buffer, offset + remainCacheLength, count - remainCacheLength);
                return read;
            }
        }

        private int RemainCacheLength()
        {
            if (this.header == null) return 0;
            return (int)(this.header.Length - this.header.Position);
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
    }
}
