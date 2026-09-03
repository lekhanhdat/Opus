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
using AvePoint.zlib;

namespace AvePoint.GCommon.Utility.FilteringBox
{
    public class ZlibDeCompressionFilteringBox : IDataFilteringBox
    {
        private ZStream mZStream = new ZStream();

        private MemoryPipeStream mMemoryPipeStream = new MemoryPipeStream(2 * FilteringBoxConstants.KB * FilteringBoxConstants.KB);

        #region IDataFilteringBox Members

        public void InputBegin()
        {
            mZStream.inflateInit();
            mMemoryPipeStream.Reset();
        }

        public void Input(byte[] buffer, int offset, int count)
        {
            if (count > 64 * FilteringBoxConstants.KB)
            {
                throw new Exception("max input unit: 64KB");
            }
            byte[] mSourceBuffer = new byte[count];
            Array.Copy(buffer, offset, mSourceBuffer, 0, count);
            mZStream.next_in = mSourceBuffer;
            mZStream.next_in_index = 0;
            mZStream.avail_in = count;

            byte[] mDestBuffer = new byte[FilteringBoxConstants.KB * 64];
            do
            {
                mZStream.next_out = mDestBuffer;
                mZStream.next_out_index = 0;
                mZStream.avail_out = mDestBuffer.Length;

                int err = mZStream.inflate(zlibConst.Z_NO_FLUSH);
                if (err != zlibConst.Z_OK && err != zlibConst.Z_STREAM_END)
                    throw new ZStreamException("inflating: " + mZStream.msg);

                mMemoryPipeStream.Write(mDestBuffer, 0, mDestBuffer.Length - mZStream.avail_out);
            }
            while (mZStream.avail_in > 0 || (mZStream.avail_out == 0));
        }

        public void InputEnd()
        {
            byte[] mDestBuffer = new byte[FilteringBoxConstants.KB * 64];
            do
            {
                mZStream.next_out = mDestBuffer;
                mZStream.next_out_index = 0;
                mZStream.avail_out = mDestBuffer.Length;

                int err = mZStream.inflate(zlibConst.Z_FINISH);
                if (err != zlibConst.Z_OK && err != zlibConst.Z_STREAM_END)
                    throw new ZStreamException("inflating: " + mZStream.msg);

                mMemoryPipeStream.Write(mDestBuffer, 0, mDestBuffer.Length - mZStream.avail_out);
            }
            while (mZStream.avail_in > 0 || (mZStream.avail_out == 0));
            this.mZStream.inflateEnd();
            this.mZStream.free();
            this.mMemoryPipeStream.FinishWrite();
        }

        public int ReceiveOutput(byte[] data, int offset, int count)
        {
            return mMemoryPipeStream.Read(data, offset, count);
        }

        #endregion
    }
}
