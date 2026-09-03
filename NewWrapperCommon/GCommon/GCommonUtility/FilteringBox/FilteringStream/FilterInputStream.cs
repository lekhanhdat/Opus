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
using System.Text;
using System.IO;

namespace AvePoint.GCommon.Utility.FilteringBox.FilteringStream
{
    public class FilterInputStream : Stream
    {
        public const int BUFFER_SIZE = 64 * 1024;
        private Stream mInnerStream;
        protected IDataFilteringBox mDataFilteringBox { get; set; }
        private byte[] mInputBuffer = new byte[BUFFER_SIZE];

        public FilterInputStream(Stream innerStream)
        {
            this.mInnerStream = innerStream;
            FileLength = 0;
        }
        public long FileLength
        {
            get;
            set;
        }
        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanWrite
        {
            get { throw new NotImplementedException(); }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int outputLen = this.mDataFilteringBox.ReceiveOutput(buffer, offset, count);
            if (outputLen != 0) return outputLen;
            while (true)
            {
                byte[] data = new byte[BUFFER_SIZE];
                int readLen = this.mInnerStream.Read(mInputBuffer, 0, FileLength == 0 ? mInputBuffer.Length : (int)Math.Min(FileLength, mInputBuffer.Length));
                if (FileLength != 0)
                    FileLength -= readLen;
                if ((((this.mInnerStream.GetType() == typeof(CompressedInputStream) || this.mInnerStream.GetType() == typeof(EncryptedInputStream)) && readLen == -1))
                    || (this.mInnerStream.GetType() != typeof(CompressedInputStream) && this.mInnerStream.GetType() != typeof(EncryptedInputStream) && readLen <= 0))
                {
                    this.mDataFilteringBox.InputEnd();
                }
                else
                {
                    this.mDataFilteringBox.Input(mInputBuffer, 0, readLen);
                }
                outputLen = this.mDataFilteringBox.ReceiveOutput(buffer, offset, count);
                if (outputLen == 0) continue;
                return outputLen;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            this.mInnerStream.Close();
        }
    }
}
