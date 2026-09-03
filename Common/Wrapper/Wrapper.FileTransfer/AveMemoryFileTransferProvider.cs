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

namespace AvePoint.Wrapper.FileTransfer
{
    public class AveMemoryFileTransferProvider
    {
        private readonly AveSyncEvent mSyncEvent;
        private readonly AveMemoryFileSender mFileSender;
        private readonly AveMemoryFileReceiver mFileReceiver;

        public AveMemoryFileTransferProvider()
        {
            AveBlockQueue freeQueue = new AveBlockQueue(0, AveBlockQueue.DEFAULT_QUEUE_SIZE);
            AveBlockQueue workingQueue = new AveBlockQueue(0, AveBlockQueue.DEFAULT_QUEUE_SIZE);
            for (int i = 0; i < AveBlockQueue.DEFAULT_QUEUE_SIZE; ++i)
            {
                freeQueue.PutBlock(new AveDataBlock());
            }
            mSyncEvent = new AveSyncEvent();
            mFileSender = new AveMemoryFileSender(freeQueue, workingQueue, mSyncEvent);
            mFileReceiver = new AveMemoryFileReceiver(workingQueue, freeQueue, mSyncEvent);
        }

        public IFileSender FileSender
        {
            get { return mFileSender; }
        }

        public IFileReceiver FileReceiver
        {
            get { return mFileReceiver; }
        }

        public AveSyncEvent SyncEvent
        {
            get { return mSyncEvent; }
        }

        public void Fail(Exception exception)
        {
            mSyncEvent.Fail(exception);
        }
    }
}
