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
using System.Threading;

namespace AvePoint.Wrapper.Common.MultiThread
{
    public abstract class TransferDataTask<T> : BaseTask where T : BaseTask
    {
        protected readonly Queue<T> tasks = new Queue<T>();
        protected int maxQueueNumber;
        protected readonly AutoResetEvent transferedEvent;
        protected T currentTask;
        protected Exception exception;
        public int QueueNumber { get { return maxQueueNumber; } }

        protected TransferDataTask(int maxQueueNumber)
        {
            this.maxQueueNumber = maxQueueNumber;
            transferedEvent = new AutoResetEvent(false);
            RunInfinite = true;
            SleepTime = 1000;
        }

        public void AddBackupTask(T task, int transferQueueNumber = 0)
        {
            if (exception != null)
            {
                throw exception;
            }

            if (task != null)
            {
                while (true)
                {
                    if (tasks.Count > maxQueueNumber)
                    {
                        transferedEvent.WaitOne();
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        lock (tasks)
                        {
                            tasks.Enqueue(task);
                            if (transferQueueNumber != 0)
                            {
                                this.maxQueueNumber = transferQueueNumber;
                            }
                        }
                        break;
                    }
                }
            }
        }

        public void ResetQueueCapacity(int maxQueueNumber)
        {
            this.maxQueueNumber = maxQueueNumber;
        }

        private T Peek()
        {
            lock (tasks)
            {
                if (tasks.Count > 0)
                {
                    return tasks.Peek();
                }
            }

            return default(T);
        }

        private void RemoveFirstOne()
        {
            lock (tasks)
            {
                tasks.Dequeue();
            }
            transferedEvent.Set();
        }

        public override void Process()
        {
            while (true)
            {
                var task = Peek();
                if (task != null)
                {
                    try
                    {
                        currentTask = task;
                        ProcessTask(task);
                    }
                    finally
                    {
                        RemoveFirstOne();
                        task.Dispose();
                    }
                }
                else
                {
                    break;
                }
            }
        }

        protected abstract void ProcessTask(T task);

        protected override void Close()
        {
            RunInfinite = false;
            transferedEvent.Dispose();
        }

        public void WaitForTransferJob()
        {
            while (tasks.Count > 0)
            {
                transferedEvent.WaitOne();
            }
        }
    }
}