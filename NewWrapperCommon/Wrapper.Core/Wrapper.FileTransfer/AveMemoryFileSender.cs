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
using System.Linq;
using System.Text;
using AvePoint.GCommon;
using AvePoint.GCommon.FileTransfer;
using AvePoint.GCommon.Network;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.GCommon.Utility.Cryptography;
namespace AvePoint.Wrapper.FileTransfer
{
    class AveMemoryFileSender : IFileSender
    {
        protected AveBlockQueue mFreeQueue;
        protected AveBlockQueue mOutputQueue;
        protected AveSyncEvent mSyncEvent;
        private bool mIsTestRun;
        private long mFileSize;

        public AveMemoryFileSender(AveBlockQueue freeQueue, AveBlockQueue outputQueue, AveSyncEvent syncEvent)
        {
            mFreeQueue = freeQueue;
            mOutputQueue = outputQueue;
            mSyncEvent = syncEvent;
        }

        public string Open(string host, int port, string info, string reconnect)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public string Open(Dictionary<string, int> mediaHosts, string connectInfo, int reconnectTimeOut = 1800000, int reconnectInterval = 30000)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void SetServerFlag(long flag)
        {
        }

        public void SetTestRunFlag(bool flag)
        {
            mIsTestRun = flag;
        }

        public void SetCertificationFlag(int useCRC)
        {
        }

        public void SetQueueBufferSize(int size)
        {
        }

        public void ReceiveDataBlock(ref AveDataBlock dataBlock)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void WriteHead(string xml)
        {
            mSyncEvent.CheckIsRunning();
            mFileSize = 0;
            if (mIsTestRun)
            {
                return;
            }

            AveDataBlock availBlock = mFreeQueue.TakeBlock();
            availBlock.Type = AveDataBlockType.HEADER_TYPE;
            availBlock.PutString(xml);
            mOutputQueue.PutBlock(availBlock);
        }

        public void WriteData(byte[] buf, int offset, int length)
        {
            mSyncEvent.CheckIsRunning();
            mFileSize += length;

            if (mIsTestRun)
            {
                return;
            }

            AveDataBlock availBlock;
            int count;
            while (length > 0)
            {
                availBlock = mFreeQueue.TakeBlock();
                availBlock.Type = AveDataBlockType.DATA_TYPE;
                count = length > AveDataBlock.DATA_BLOCK_DATA_LEN ? AveDataBlock.DATA_BLOCK_DATA_LEN : length;
                Array.Copy(buf, offset, availBlock.Buffer, AveDataBlock.DATA_BLOCK_HEADER_LEN, count);
                availBlock.DataSize = count;
                mOutputQueue.PutBlock(availBlock);
                offset += count;
                length -= count;
            }
        }

        public void WriteContentData(byte[] buf, int offset, int length)
        {
            WriteData(buf, offset, length);
        }

        public long WriteTail(string xml)
        {
            return WriteTail(xml, true);
        }

        public long WriteTail(string xml, bool isOK)
        {
            mSyncEvent.CheckIsRunning();
            if (mIsTestRun)
            {
                return mFileSize;
            }
            if (isOK)
            {
                xml = "<FileTail length=\"" + mFileSize + "\">" + xml + "</FileTail>";
            }
            else
            {
                xml = "<FileTail failed=\"true\" length=\"" + mFileSize + "\">" + xml + "</FileTail>";
            }

            // take an available data block
            AveDataBlock dataBlock = mFreeQueue.TakeBlock();
            dataBlock.Type = AveDataBlockType.TAIL_TYPE;
            dataBlock.PutString(xml);
            // put a new input data block and notify processor
            mOutputQueue.PutBlock(dataBlock);

            return mFileSize;
        }

        public void SetReadMessageWorker(IFileSenderResponseWorker worker)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Close(int flag)
        {
            AveDataBlock closeBlock = mFreeQueue.TakeBlock();
            closeBlock.Type = AveDataBlockType.CLOSE_CONNECTION_TYPE;
            closeBlock.DataSize = 1;
            closeBlock.Buffer[AveDataBlock.DATA_BLOCK_HEADER_LEN] = (byte)flag;

            // put the close data block and notify the processor
            mOutputQueue.PutBlock(closeBlock);
        }

        public void Close(string flag)
        {
            AveDataBlock closeBlock = mFreeQueue.TakeBlock();
            closeBlock.Type = AveDataBlockType.CLOSE_CONNECTION_TYPE;
            closeBlock.DataSize = 1;
            closeBlock.Buffer[AveDataBlock.DATA_BLOCK_HEADER_LEN] = (byte)(Convert.ToInt32(flag));

            // put the close data block and notify the processor
            mOutputQueue.PutBlock(closeBlock);
        }


        public void SetEncryptionInfo(GCommon.Contract.Server.ControlPanel.Cryptography.DataEncryptionInfo info)
        {
            throw new NotImplementedException();
        }
    }
}
