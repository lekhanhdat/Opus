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
using System.IO;
using System.IO.Pipes;
using System.Text;
using AvePoint.GCommon;
using AvePoint.GCommon.FileTransfer;
using AvePoint.GCommon.Network;

namespace AvePoint.Wrapper.FileTransfer
{
    public class AveNamedPipeFileTransferProvider
    {
        private string mPipeName = "AvePoint.FileTransfer";
        private string mServerName = ".";
        private AveNamedPipeFileSender mNamedPipeSender;
        private AveNamedPipeFileReceiver mNamedPipeReceiver;

        public IFileSender FileSender
        {
            get 
            {
                if (mNamedPipeSender == null)
                {
                    mNamedPipeSender = CreateSender();
                    mNamedPipeSender.StartTransfer();
                }
                return mNamedPipeSender; 
            }
        }

        public IFileReceiver FileReceiver
        {
            get 
            {
                if (mNamedPipeReceiver == null)
                {
                    mNamedPipeReceiver = CreateReceiver();
                    mNamedPipeReceiver.StartTransfer();
                }
                return mNamedPipeReceiver;
            }
        }       

        private AveNamedPipeFileSender CreateSender()
        {
            AveBlockQueue freeQueue = new AveBlockQueue(0, AveBlockQueue.DEFAULT_QUEUE_SIZE);
            AveBlockQueue workingQueue = new AveBlockQueue(0, AveBlockQueue.DEFAULT_QUEUE_SIZE);
            for (int i = 0; i < AveBlockQueue.DEFAULT_QUEUE_SIZE; ++i)
            {
                freeQueue.PutBlock(new AveDataBlock());
            }
            AveSyncEvent syncEvent = new AveSyncEvent();
            NamedPipeClientStream namedPipeClientStream = new NamedPipeClientStream(mServerName, mPipeName, PipeDirection.Out);

            return new AveNamedPipeFileSender(namedPipeClientStream, freeQueue, workingQueue, syncEvent);            
        }

        private AveNamedPipeFileReceiver CreateReceiver()
        {
            AveBlockQueue freeQueue = new AveBlockQueue(0, AveBlockQueue.DEFAULT_QUEUE_SIZE);
            AveBlockQueue workingQueue = new AveBlockQueue(0, AveBlockQueue.DEFAULT_QUEUE_SIZE);
            for (int i = 0; i < AveBlockQueue.DEFAULT_QUEUE_SIZE; ++i)
            {
                freeQueue.PutBlock(new AveDataBlock());
            }
            AveSyncEvent syncEvent = new AveSyncEvent();
            NamedPipeServerStream namedPipeServerStream = new NamedPipeServerStream(mPipeName, PipeDirection.In);
            
            return new AveNamedPipeFileReceiver(namedPipeServerStream, workingQueue, freeQueue, syncEvent);            
        }
    }
}
