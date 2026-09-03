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
using System.IO;
using System.IO.Pipes;
using System.Threading;
using AvePoint.GCommon;
using AvePoint.GCommon.Network;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.FileTransfer
{
    class AveNamedPipeFileSender : AveMemoryFileSender
    {
        private NamedPipeClientStream mNamedPipeClientStream;

        public AveNamedPipeFileSender(NamedPipeClientStream namedPipeClientStream, AveBlockQueue freeQueue, AveBlockQueue outputQueue, AveSyncEvent syncEvent) : base(freeQueue, outputQueue, syncEvent)
        {
            mNamedPipeClientStream = namedPipeClientStream;
        }

        public void StartTransfer()
        {
            Thread thread = new Thread(StartSend);
            thread.Name = "NamedPipeFileSender";
            thread.Start();
        }

        public void StartSend()
        {
            mNamedPipeClientStream.Connect();
            byte[] blockSize = new byte[4];
            while (true)
            {
                AveDataBlock dataBlock = mOutputQueue.TakeBlock();

                AveConvert.ToBigBytes(dataBlock.DataSize, blockSize, 0);
                mNamedPipeClientStream.Write(blockSize, 0, blockSize.Length);                
                mNamedPipeClientStream.Write(dataBlock.Buffer, 0, dataBlock.DataSize + AveDataBlock.DATA_BLOCK_HEADER_LEN);
                mFreeQueue.PutBlock(dataBlock);

                if (dataBlock.Type == AveDataBlockType.CLOSE_CONNECTION_TYPE)
                {                    
                    mNamedPipeClientStream.Dispose();                    
                    break;
                }                
            }
        }        
    }
}
