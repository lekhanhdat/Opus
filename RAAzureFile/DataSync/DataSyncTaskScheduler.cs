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
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RAAzureFile.DataSync
{
    public class DataSyncTaskScheduler : TaskScheduler
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(DataSyncTaskScheduler));

        [ThreadStatic]
        private static bool CurrentThreadIsProcessingItems;

        private readonly LinkedList<Task> Tasks = new LinkedList<Task>();

        private readonly int MaxDegreeOfParallelism;

        private int DelegatesQueuedOrRunning = 0;

        public override int MaximumConcurrencyLevel => MaxDegreeOfParallelism;

        public DataSyncTaskScheduler(int maxDegreeOfParallelism)
        {
            if(maxDegreeOfParallelism < 1)
            {
                throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
            }
            MaxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        protected override void QueueTask(Task task)
        {
            lock(Tasks)
            {
                Tasks.AddLast(task);
                if(DelegatesQueuedOrRunning < MaxDegreeOfParallelism)
                {
                    ++DelegatesQueuedOrRunning;
                    NotifyThreadPoolOfPendingWork();
                }
            }
        }

        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
            {
                CurrentThreadIsProcessingItems = true;
                try
                {
                    while(true)
                    {
                        Task item;
                        lock (Tasks)
                        {
                            if (Tasks.Count == 0)
                            {
                                --DelegatesQueuedOrRunning;
                                break;
                            }

                            item = Tasks.First.Value;
                            Tasks.RemoveFirst();
                        }

                        base.TryExecuteTask(item);
                    }
                }
                catch(Exception e)
                {
                    Logger.Error($"An error occurred while notify thread pool of pending work. Error: {e}");
                }
                finally
                {
                    CurrentThreadIsProcessingItems = false;
                }
            }, null);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if(!CurrentThreadIsProcessingItems)
            {
                return false;
            }

            if(taskWasPreviouslyQueued)
            {
                if(TryDequeue(task))
                {
                    return base.TryExecuteTask(task);
                }
            }

            return base.TryExecuteTask(task);
        }

        protected override bool TryDequeue(Task task)
        {
            lock(Tasks)
            {
                return Tasks.Remove(task);
            }
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            var lockTaken = false;
            try
            {
                Monitor.TryEnter(Tasks, ref lockTaken);
                if (lockTaken)
                {
                    return Tasks;
                }

                throw new NotSupportedException();
            }
            catch(Exception e)
            {
                Logger.Error($"An error occured while get scheduled tasks. Error: {e}");
                return new List<Task>();
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(Tasks);
                }
            }
        }
    }
}
