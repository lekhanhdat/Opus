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
using zlib;
using AvePoint.GCommon.Utility.Cryptography;

namespace AvePoint.GCommon.Utility.FilteringBox
{
    public class ZlibCompressionFilteringBox : IDataFilteringBox
    {
        private static AveLogger logger = new AveLogger(typeof(ZlibCompressionFilteringBox));

        private ZStream mZipStream = new ZStream();
        private byte[] mSourceBuffer = new byte[0];
        private int mCompressionType = -1;
        private bool mIsCompress;
        private bool mIsEnd = false;
        private bool mHasInput = false;
        private bool mHasReceiveOutput = false;

        public ZlibCompressionFilteringBox()
        {
            mIsCompress = false;
        }

        public ZlibCompressionFilteringBox(int compressionType)
        {
            mIsCompress = true;
            mCompressionType = compressionType >= 1 && compressionType <= 9 ? compressionType : 6;
        }

        #region IDataFilteringBox Members
        public void InputBegin(string key, EncryptionAlgorithm alg)
        {
            InputBegin();
        }
        public void InputBegin()
        {
            if (mIsEnd)
            {
                Finish();
            }
            if (mIsCompress)
            {
                mZipStream.deflateInit(mCompressionType);
            }
            else
            {
                mZipStream.inflateInit();
            }
            mIsEnd = false;
            mHasInput = false;
            mHasReceiveOutput = false;
        }

        public void Input(byte[] data, int offset, int count)
        {
            mSourceBuffer = new byte[count];

            Array.Copy(data, offset, mSourceBuffer, 0, count);

            // set the source data
            mZipStream.next_in = mSourceBuffer;
            mZipStream.next_in_index = 0;
            mZipStream.avail_in = count;

            mHasInput = true;
        }

        public void InputEnd()
        {
            mIsEnd = true;
            mHasReceiveOutput = false;
        }

        public int ReceiveOutput(byte[] data, int offset, int count)
        {
            if (mHasInput)
                return RealReceiveOutput(data, offset, count);
            else if (mIsEnd)
                return -1;
            return 0;
        }

        #endregion

        private void Finish()
        {
            if (mIsCompress)
            {
                this.mZipStream.deflateEnd();
            }
            else
            {
                this.mZipStream.inflateEnd();
            }
        }

        private int RealReceiveOutput(byte[] data, int offset, int count)
        {
            int realLen = -1;
            bool shouldContinue2Read = true;
            if (mHasReceiveOutput)
            {
                shouldContinue2Read = ((mZipStream.avail_in > 0 || mZipStream.avail_out == 0));
            }
            if (shouldContinue2Read)
            {
                do
                {
                    mZipStream.next_out = data;
                    mZipStream.next_out_index = offset;
                    mZipStream.avail_out = count;

                    int flush = mIsEnd ? zlibConst.Z_FINISH : zlibConst.Z_NO_FLUSH;

                    int err;
                    if (mIsCompress)
                    {
                        err = mZipStream.deflate(flush);
                    }
                    else
                    {
                        err = mZipStream.inflate(flush);
                    }
                    if (err == zlibConst.Z_BUF_ERROR && mZipStream.avail_in == 0)
                    {
                        logger.Info("There is a buffer error when available in is zero.");
                        break;
                    }
                    else if (err != zlibConst.Z_OK && err != zlibConst.Z_STREAM_END)
                    {
                        logger.Error("There is an error. {0}", err);
                        throw new ZStreamException((mIsCompress ? "de" : "in") + "flatting: " + mZipStream.msg);
                    }
                    if (count - mZipStream.avail_out > 0 || err == zlibConst.Z_STREAM_END)
                    {
                        realLen = count - mZipStream.avail_out;
                        break;
                    }
                }
                while (mZipStream.avail_in > 0 || mZipStream.avail_out == 0);
            }
            if (realLen <= 0)
            {
                realLen = mIsEnd ? -1 : 0;
            }
            mHasReceiveOutput = true;
            return realLen;
        }
    }

    public class CompressionTest
    {
        public static void Test(string srcFile, string destFile, int type = -1)
        {
            ZlibCompressionFilteringBox compress = new ZlibCompressionFilteringBox(1);
            if (type > 0)
            {
                compress = new ZlibCompressionFilteringBox(type);
            }
            else
            {
                compress = new ZlibCompressionFilteringBox();
            }
            compress.InputBegin();
            byte[] buffer = new byte[64 * 1024];
            byte[] outBuffer = new byte[64 * 1024];
            using (System.IO.FileStream writer = new System.IO.FileStream(destFile, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                using (System.IO.FileStream reader = new System.IO.FileStream(srcFile, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    int len = reader.Read(buffer, 0, buffer.Length);
                    while (len > 0)
                    {
                        compress.Input(buffer, 0, len);
                        while (true)
                        {
                            int outLen = compress.ReceiveOutput(outBuffer, 0, outBuffer.Length);
                            if (outLen == 0) break;
                            writer.Write(outBuffer, 0, outLen);
                        }
                        len = reader.Read(buffer, 0, buffer.Length);
                    }
                    compress.InputEnd();
                    while (true)
                    {
                        int outLen = compress.ReceiveOutput(outBuffer, 0, outBuffer.Length);
                        if (outLen == -1) break;
                        writer.Write(outBuffer, 0, outLen);
                    }
                }
            }
        }

        //static void Main(string[] args)
        //{
        //    Test(@"c:\c.exe", @"c:\compressed.zlib",1);
        //    Test(@"c:\compressed.zlib", @"c:\temp.exe");
        //}
    }
}
