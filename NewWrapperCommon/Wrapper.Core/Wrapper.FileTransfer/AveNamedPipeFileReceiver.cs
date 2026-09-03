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
using System.IO.Pipes;
using System.Threading;
using AvePoint.GCommon.Network;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.FileTransfer
{
    class AveNamedPipeFileReceiver : AveMemoryFileReceiver
    {
        private NamedPipeServerStream mNamedPipeServerStream;

        public AveNamedPipeFileReceiver(NamedPipeServerStream namedPipeServerStream, AveBlockQueue inputQueue, AveBlockQueue freeQueue, AveSyncEvent syncEvent) : base(inputQueue, freeQueue, syncEvent)
        {
            mNamedPipeServerStream = namedPipeServerStream;
        }

        public void StartTransfer()
        {
            Thread thread = new Thread(StartReceive);
            thread.Name = "NamedPipeFileReceiver";
            thread.Start();
        }

        public void StartReceive()
        {
            mNamedPipeServerStream.WaitForConnection();
            byte[] dataBlockSize = new byte[4];
            while (true)
            {
                AveDataBlock dataBlock = mFreeQueue.TakeBlock();
                mNamedPipeServerStream.Read(dataBlockSize, 0, dataBlockSize.Length);
                int blockDataSize = AveConvert.ToBigInt(dataBlockSize, 0);

                if (blockDataSize > AveDataBlock.DATA_BLOCK_DATA_LEN)
                {
                    byte[] buffer = new byte[blockDataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN];
                    mNamedPipeServerStream.Read(buffer, 0, blockDataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN);
                    dataBlock.Buffer = buffer;
                }
                else
                {
                    mNamedPipeServerStream.Read(dataBlock.Buffer, 0, blockDataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN);                    
                }
                dataBlock.DataSize = blockDataSize;
                mInputQueue.PutBlock(dataBlock);
                if (dataBlock.Type == AveDataBlockType.CLOSE_CONNECTION_TYPE)
                {                    
                    mNamedPipeServerStream.Dispose();
                    break;
                }
            }
        }
    }
}
