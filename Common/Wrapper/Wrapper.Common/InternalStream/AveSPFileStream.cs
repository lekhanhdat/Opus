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

namespace AvePoint.Wrapper.Common
{
    /// <summary>
    /// For SPFileCollection.Add(string url, Stream stream)
    /// It accepts a IAveRestoreStream and semulates a Stream
    /// for SPFileCollection
    /// </summary>
    public class AveSPFileStream : Stream
    {
        private IAveRestoreStream mStream;
        private long mPosition;
        private long mLength;

        public AveSPFileStream(IAveRestoreStream stream)
        {
            mStream = stream;
            mLength = stream.ContentLength;
            if (mLength < 0)
            {
                throw new IOException(string.Format("Invalid stream size '{0}'", mLength));
            }
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
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { return mLength; }
        }

        public override long Position
        {
            get
            {
                return mPosition;
            }
            set
            {
                mPosition = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (mPosition >= mLength)
            {
                return 0;
            }
            if (count + mPosition > mLength)
            {
                count = Convert.ToInt32(mLength - mPosition);
            }
            int len = mStream.ReadContent(buffer, offset, count);
            if (len < 0)
            {
                len = 0;
            }
            mPosition += len;
            return len;
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
    }
}
