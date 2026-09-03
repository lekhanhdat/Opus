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
    public class FilterOutputStream : Stream
    {
        public const int BUFFER_SIZE = 64 * 1024;
        private Stream mInnerStream;
        protected IDataFilteringBox mDataFilteringBox { get; set; }
        private byte[] mOutputBuffer = new byte[BUFFER_SIZE];

        public FilterOutputStream(Stream innerStream)
        {
            this.mInnerStream = innerStream;
        }

        public override bool CanRead
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanSeek
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            mDataFilteringBox.InputEnd();
            int readLen = 0;
            while (true)
            {
                readLen = mDataFilteringBox.ReceiveOutput(mOutputBuffer, 0, mOutputBuffer.Length);
                if (readLen <= 0) break;
                this.mInnerStream.Write(mOutputBuffer, 0, readLen);
            }
            this.mInnerStream.Flush();
        }

        public override void Close()
        {
            Flush();
            this.mInnerStream.Close();
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
            throw new NotImplementedException();
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
            while (count > FilterOutputStream.BUFFER_SIZE)
            {
                WriteInternal(buffer, offset, FilterOutputStream.BUFFER_SIZE);
                offset += FilterOutputStream.BUFFER_SIZE;
                count -= FilterOutputStream.BUFFER_SIZE;
            }
            if (count > 0)
            {
                WriteInternal(buffer, offset, count);
            }
        }

        private void WriteInternal(byte[] buffer, int offset, int count)
        {
            mDataFilteringBox.Input(buffer, offset, count);
            int readLen = 0;
            while (true)
            {
                readLen = mDataFilteringBox.ReceiveOutput(mOutputBuffer, 0, mOutputBuffer.Length);
                if (readLen == 0) break;
                mInnerStream.Write(mOutputBuffer, 0, readLen);
            }
        }
    }

}
