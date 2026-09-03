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
using System.IO;
using System.Linq;
using System.Text;
using AvePoint.GCommon.FileTransfer;

namespace AvePoint.Wrapper.Common
{
    class AveInternalRestoreStream : Stream
    {
        protected IInputStreamWrapper inputStream;
        protected long mLength;
        private HeaderV1 header;

        public AveInternalRestoreStream(IInputStreamWrapper inputStream, HeaderV1 header)
        {
            this.header = header;
            this.inputStream = inputStream;
            this.mLength = this.MetadataLength;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
        }

        public override long Length
        {
            get { return inputStream.Length; }
        }

        public override long Position
        {
            get { return this.inputStream.Position; }
            set { this.inputStream.Position = value; }
        }

        public virtual long ContentLength
        {
            get
            {
                return header.ContentLength;
            }
        }
        public virtual long MetadataLength
        {
            get
            {
                return header.MetadataLength;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadMetadata(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("This class does not support seek operation.");
        }

        public override void SetLength(long value)
        {
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("This class does not support write operation.");
        }

        public virtual int ReadMetadata(byte[] buffer, int offset, int count)
        {
            if (mLength > 0)
            {
                if (count > mLength)
                {
                    count = (int)mLength;
                }
                int readLen = SafeRead(buffer, offset, count);
                mLength -= readLen;
                return readLen;
            }
            return 0;
        }

        protected virtual int SafeRead(byte[] buffer, int offset, int length)
        {
            int readLen = inputStream.ReadMetadata(buffer, offset, length);
            if (readLen > 0)
            {
                return readLen;
            }
            throw new EndOfStreamException("Unexpected end of stream.");
        }

        public virtual int ReadContent(byte[] buffer, int offset, int length)
        {
            return inputStream.ReadContent(buffer, offset, length);
        }
    }
}
