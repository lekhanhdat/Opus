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
using AvePoint.GCommon.Network;
using AvePoint.GCommon.FileTransfer;

namespace AvePoint.Wrapper.FileTransfer
{
    class AveMemoryFileReceiver : IFileReceiver
    {
        private static AveLogger mLog = AveLogger.GetInstance(typeof(AveMemoryFileReceiver));
        protected AveBlockQueue mInputQueue;
        protected AveBlockQueue mFreeQueue;
        protected AveSyncEvent mSyncEvent;

        protected AveDataBlock mWorkingBlock;
        private int mLength;
        private int mOffset;

        public AveMemoryFileReceiver(AveBlockQueue inputQueue, AveBlockQueue freeQueue, AveSyncEvent syncEvent)
        {
            mInputQueue = inputQueue;
            mFreeQueue = freeQueue;
            mSyncEvent = syncEvent;
        }

        public string GetNextFileHead()
        {
            while (true)
            {
                if (mWorkingBlock != null &&
                    (mWorkingBlock.Type == AveDataBlockType.HEADER_TYPE
                      || mWorkingBlock.Type == AveDataBlockType.CLOSE_CONNECTION_TYPE))
                {
                    break;
                }
                GetNextBlock();
            }
            if (mWorkingBlock.Type == AveDataBlockType.CLOSE_CONNECTION_TYPE)
            {
                return null;
            }
            string head = mWorkingBlock.RetrieveString();
            GetNextBlock();
            return head;
        }

        public string GetFileTail()
        {
            if (mWorkingBlock == null)
            {
                mLog.Error("Illegal state in GetFileTail, please call GetNextFileHead before call GetFileTail.");
                //throw new AveSPErrorException("Illegal state in GetFileTail, please call GetNextFileHead before call GetFileTail.");
            }
            while (true)
            {
                if (mWorkingBlock.Type == AveDataBlockType.TAIL_TYPE)
                {
                    break;
                }
                GetNextBlock();
            }
            string tail = mWorkingBlock.RetrieveString();
            GetNextBlock();
            return tail;
        }

        public int CRC32Match()
        {
            return 1;
        }

        public int ReadBytes(byte[] buffer, int len)
        {
            return ReadBytes(buffer, 0, len);
        }

        public int ReadBytes(byte[] buffer, int offset, int length)
        {
            int count = 0;
            int ret;
            while (length > 0)
            {
                ret = SafeRead(buffer, offset, length);
                if (ret == 0)
                {
                    break;
                }
                count += ret;
                offset += ret;
                length -= ret;
            }
            return count;
        }

        private int SafeRead(byte[] buffer, int offset, int length)
        {
            if (mWorkingBlock == null)
            {
                mLog.Error("Please call GetNextFileHead before calling readBytes while coming across the illegal state in readBytes.");
                //throw new AveSPErrorException("Please call GetNextFileHead before calling readBytes while coming across the illegal state in readBytes.");
            }
            if (mWorkingBlock.Type == AveDataBlockType.TAIL_TYPE)
            {
                return 0;
            }
            if (mWorkingBlock.Type != AveDataBlockType.DATA_TYPE)
            {
                mLog.Error(string.Format("Illegal data block in ReadBytes, expected block type:{0}, but:{1}",
                    AveDataBlockType.DATA_TYPE.ToString("X"), mWorkingBlock.Type.ToString("X")));
                //throw new AveSPErrorException(string.Format("Illegal data block in ReadBytes, expected block type:{0}, but:{1}",
                //    AveDataBlock.DATA_TYPE.ToString("X2"), mWorkingBlock.Type.ToString("X2")));
            }
            if (mLength < length)
            {
                length = mLength;
            }
            Array.Copy(mWorkingBlock.Buffer, mOffset, buffer, offset, length);
            mLength -= length;
            mOffset += length;
            if (mLength <= 0)
            {
                GetNextBlock();
            }
            return length;
        }

        public string Close(string message)
        {
            return string.Empty;
        }

        private void GetNextBlock()
        {
            if (mWorkingBlock != null)
            {
                mFreeQueue.PutBlock(mWorkingBlock);
                mWorkingBlock = null;
            }
            mSyncEvent.CheckIsRunning();
            if (mInputQueue.Count > 0 || mSyncEvent.IsRunning)
            {
                mWorkingBlock = mInputQueue.TakeBlock();
                mOffset = AveDataBlock.DATA_BLOCK_HEADER_LEN;
                mLength = mWorkingBlock.DataSize;
            }
            else
            {
                mLog.Error("Block receiver has already exited");
                //throw new AveSPErrorException("Block receiver has already exited");
            }
        }

        public string Open(string host, int port, string info)
        {
            throw new NotImplementedException();
        }


        public void SetEncryptionInfo(AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.DataEncryptionInfo info)
        {
            throw new NotImplementedException();
        }


    }
}
