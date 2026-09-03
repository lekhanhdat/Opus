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
using AvePoint.GCommon.Utility.Cryptography;
using ICSharpCode.SharpZipLib.Zip.Compression;
//using zlib;

namespace AvePoint.GCommon.Utility.FilteringBox
{
    public class CompressionFilteringBox : IDataFilteringBox
    {
        protected virtual IDataFilteringBox InnerFilteringBox { get; set; }
        public CompressionFilteringBox()
        {
            InnerFilteringBox = new CompressionFilteringOutputBox();
        }

        public CompressionFilteringBox(int level)
        {
            InnerFilteringBox = new CompressionFilteringInputBox(level);
        }
        public void Input(byte[] data, int offset, int count)
        {
            InnerFilteringBox.Input(data, offset, count);
        }

        public void InputBegin(string key = null, EncryptionAlgorithm alg = EncryptionAlgorithm.AES_ENCRYPTION)
        {
            InnerFilteringBox.InputBegin(key, alg);
        }

        public void InputBegin()
        {
            InnerFilteringBox.InputBegin();
        }

        public void InputEnd()
        {
            InnerFilteringBox.InputEnd();
        }

        public int ReceiveOutput(byte[] data, int offset, int count)
        {
            return InnerFilteringBox.ReceiveOutput(data, offset, count);
        }

        public void Dispose()
        {
        }
    }
    public class CompressionFilteringOutputBox : IDataFilteringBox
    {
        private Inflater inflater;
        private bool mIsEnd = false;
        private bool mHasInput = false;
        public CompressionFilteringOutputBox()
        {
        }

        #region IDataFilteringBox Members

        public void InputBegin(string key, EncryptionAlgorithm alg)
        {
            InputBegin();
        }

        public void InputBegin()
        {
            inflater = new Inflater(false);
            mIsEnd = false;
            mHasInput = false;
        }
        public void Input(byte[] data, int offset, int count)
        {
            inflater.SetInput(data, offset, count);
            mHasInput = true;
        }

        public void InputEnd()
        {
            mIsEnd = true;
        }

        public int ReceiveOutput(byte[] data, int offset, int count)
        {
            if (mHasInput)
                return RealReceiveOutput(data, offset, count);
            return 0;
        }

        #endregion

        private int RealReceiveOutput(byte[] data, int offset, int count)
        {
            var realLen = inflater.Inflate(data, offset, count);

            if (realLen <= 0)
            {
                realLen = mIsEnd ? -1 : 0;
            }
            return realLen;
        }

        public void Dispose()
        {
        }
    }

    /// <summary>
    /// Compress
    /// </summary>
    public class CompressionFilteringInputBox : IDataFilteringBox
    {
        private Deflater deflater;
        private int mCompressionType = -1;
        private bool mIsEnd = false;
        private bool mHasInput = false;

        public CompressionFilteringInputBox(int compressionType)
        {
            mCompressionType = compressionType >= 1 && compressionType <= 9 ? compressionType : 6;
        }

        #region IDataFilteringBox Members

        public void InputBegin(string key, EncryptionAlgorithm alg)
        {
            InputBegin();
        }

        public void InputBegin()
        {
            deflater = new Deflater(mCompressionType, false);
            mIsEnd = false;
            mHasInput = false;
        }

        public void Input(byte[] data, int offset, int count)
        {
            deflater.SetInput(data, offset, count);
            mHasInput = true;
        }

        public void InputEnd()
        {
            deflater.Finish();
            mIsEnd = true;
        }

        public int ReceiveOutput(byte[] data, int offset, int count)
        {
            if (mHasInput)
                return RealReceiveOutput(data, offset, count);
            return 0;
        }

        #endregion

        private int RealReceiveOutput(byte[] data, int offset, int count)
        {
            var realLen = deflater.Deflate(data, offset, count);

            if (realLen <= 0)
            {
                realLen = mIsEnd ? -1 : 0;
            }
            return realLen;
        }

        public void Dispose()
        {
        }
    }
}
