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




using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement;

namespace AvePoint.GCommon.Utility.FilteringBox
{
    internal class MixedFilteringBox : IDataFilteringBox
    {
        private IDataFilteringBox mFirstBox;
        private IDataFilteringBox mSecondBox;
        private byte[] mBuffer;
        private bool mIsEnd;
        private MemoryPipeStream mMemoryPipe;

        public MixedFilteringBox(IDataFilteringBox compressionBox, IDataFilteringBox encrytionBox, bool isCompAndEncry)
        {
            mIsEnd = false;
            mFirstBox = isCompAndEncry ? compressionBox : encrytionBox;
            mSecondBox = isCompAndEncry ? encrytionBox : compressionBox;
            mBuffer = new byte[64 * 1024];
            mMemoryPipe = new MemoryPipeStream(512 * 1024);
        }

        #region IDataFilteringBox Members

        public void InputBegin()
        {
            mFirstBox.InputBegin();
            mSecondBox.InputBegin();
            mMemoryPipe.Reset();
            mIsEnd = false;
        }

        public void InputBegin(string key, EncryptionAlgorithm alg)
        {
            mFirstBox.InputBegin(key, alg);
            mSecondBox.InputBegin(key, alg);
            mMemoryPipe.Reset();
            mIsEnd = false;
        }

        public void Input(byte[] data, int offset, int count)
        {
            mFirstBox.Input(data, offset, count);
            while (true)
            {
                int len = mFirstBox.ReceiveOutput(mBuffer, 0, mBuffer.Length);
                if (len == 0) break;
                mMemoryPipe.Write(mBuffer, 0, len);
            }
        }

        public void InputEnd()
        {
            mIsEnd = true;
            mFirstBox.InputEnd();
            while (true)
            {
                int len = mFirstBox.ReceiveOutput(mBuffer, 0, mBuffer.Length);
                if (len == -1) break;
                mMemoryPipe.Write(mBuffer, 0, len);
            }
        }

        public int ReceiveOutput(byte[] data, int offset, int count)
        {
            int len = mSecondBox.ReceiveOutput(data, offset, count);
            if (len > 0 || len == -1)
                return len;
            len = mMemoryPipe.Read(mBuffer, 0, mBuffer.Length);
            if (len > 0)
                mSecondBox.Input(mBuffer, 0, len);
            else if (mIsEnd)
                mSecondBox.InputEnd();
            else if (len == 0)
                return len;
            return mSecondBox.ReceiveOutput(data, offset, count);
        }

        public void Dispose()
        {
            if(mMemoryPipe != null)
            {
                mMemoryPipe.Dispose();
            }
        }

        #endregion
    }
    public class MixedBoxTest
    {
        //static void TestMixedBox(string srcFile,string destFile,int type)
        //{
        //    DataEncryptionInfoWrapper wrapper = DataEncryptionInfoManager.ResolveDynamicKey(DataEncryptionInfoManager.StaticEncryptionInfo);
        //    IDataFilteringBox box;
        //    if (type > 0)
        //    {
        //        box = DataFilteringBoxFactory.GetCompressionAndEncryptionFilteringBox((EncryptionAlgorithm)wrapper.EncryptionInfo.EncryptionType, wrapper.DynamicKey, CompressionMethods.ZLIB_COMPRESSION, 1);
        //    }
        //    else
        //    {
        //        box = DataFilteringBoxFactory.GetDeCompressionAndDecryptionFilteringBox((EncryptionAlgorithm)wrapper.EncryptionInfo.EncryptionType, wrapper.DynamicKey, CompressionMethods.ZLIB_COMPRESSION);
        //    }
        //    box.InputBegin();
        //    byte[] buffer = new byte[64 * 1024];
        //    byte[] outBuffer = new byte[64 * 1024];
        //    using (System.IO.FileStream writer = new System.IO.FileStream(destFile, System.IO.FileMode.Create, System.IO.FileAccess.Write))
        //    {
        //        using (System.IO.FileStream reader = new System.IO.FileStream(srcFile, System.IO.FileMode.Open, System.IO.FileAccess.Read))
        //        {
        //            int len = reader.Read(buffer, 0, buffer.Length);
        //            while (len > 0)
        //            {
        //                box.Input(buffer, 0, len);
        //                while (true)
        //                {
        //                    int outLen = box.ReceiveOutput(outBuffer, 0, outBuffer.Length);
        //                    if (outLen == 0) break;
        //                    writer.Write(outBuffer, 0, outLen);
        //                }
        //                len = reader.Read(buffer, 0, buffer.Length);
        //            }
        //            box.InputEnd();
        //            while (true)
        //            {
        //                int outLen = box.ReceiveOutput(outBuffer, 0, outBuffer.Length);
        //                if (outLen == -1) break;
        //                writer.Write(outBuffer, 0, outLen);
        //            }
        //        }
        //    }
        //}

        //static void Main(string[] args)
        //{
        //    TestMixedBox(@"c:\a.txt", @"c:\temp.dat", 1);
        //    TestMixedBox(@"c:\temp.dat", @"c:\tempc.txt", -1);
        //}
    }
}
