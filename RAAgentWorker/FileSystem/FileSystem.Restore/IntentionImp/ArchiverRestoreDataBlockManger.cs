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
using System.Collections.Generic;

namespace AvePoint.Media.Service.ArchiverBackup.Restore
{
    using GCommon;
    using System.Reflection;
    using System.Threading;

    public class ArchiverRestoreDataBlockManger : IArchiverRestoreDataBlockManager
    {
        private Queue<ArchiverRestoreDataBlock> dataBlockQueue = new Queue<ArchiverRestoreDataBlock>();

        private Semaphore producer = new Semaphore(60, 60);
        private Semaphore consumer = new Semaphore(0, 60);
        public void Add(ArchiverRestoreDataBlock restoreDataBlock)
        {
            producer.WaitOne();
            var tempDataBlock = new ArchiverRestoreDataBlock
            {
                RestoreData = restoreDataBlock.RestoreData,
                DataBlockType = restoreDataBlock.DataBlockType,
                RestoreMessage = restoreDataBlock.RestoreMessage,
            };
            lock (this.dataBlockQueue)
            {
                this.dataBlockQueue.Enqueue(tempDataBlock);
                consumer.Release(1);
            }
        }

        public ArchiverRestoreDataBlock Get()
        {
            var result = default(ArchiverRestoreDataBlock);
            consumer.WaitOne();
            lock (this.dataBlockQueue)
            {
                result = this.dataBlockQueue.Dequeue();
                producer.Release(1);
            }
            return result;
        }

        public void Clear()
        {
            this.dataBlockQueue.Clear();
        }
    }
}
