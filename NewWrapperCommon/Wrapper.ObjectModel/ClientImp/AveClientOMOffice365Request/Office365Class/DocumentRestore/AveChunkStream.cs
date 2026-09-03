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
namespace AvePoint.ObjectModel.ClientOM
{
    using System;
    using System.IO;

    /// <summary>
    /// SharePoint Online上传大文件用
    /// 强制Read方法只能读取制定大小数据
    /// </summary>
    public class AveChunkStream : Stream
    {
        private readonly Stream mInternalStream;
        private readonly int mDataBlockCapacity;
        private readonly int mLength;
        private int mPosition;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream">大文件Stream</param>
        /// <param name="dataBlockCapacity">每次读取数据块大小</param>
        public AveChunkStream(Stream stream, int dataBlockCapacity)
        {
            mInternalStream = stream;
            mDataBlockCapacity = dataBlockCapacity;
            mPosition = 0;
            long unreadBytesLength = mInternalStream.Length - mInternalStream.Position;
            if (unreadBytesLength < mDataBlockCapacity)
            {
                mLength = (int)unreadBytesLength;
            }
            else
            {
                mLength = mDataBlockCapacity;
            }
        }

        public override bool CanRead
        {
            get 
            {
                return mInternalStream.CanRead && mLength > mPosition;
            }
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
            get 
            {
                return mLength;
            }
        }

        public override long Position
        {
            get
            {
                return mPosition;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (mPosition >= mLength)
            {
                return 0;
            }
            int readLength = 0;
            if (mLength - mPosition < count)
            {
                count = mLength - mPosition;
            }
            readLength = mInternalStream.Read(buffer, offset, count);
            mPosition += readLength;

            return readLength;
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