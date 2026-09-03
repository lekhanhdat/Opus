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
    public sealed class TaskThreadPool : IDisposable
    {
        private readonly AutoResetEvent threadStateChanged;
        private readonly int threadCount;
        private readonly string threadPoolName;
        private readonly List<TaskThread> availableThreads = new List<TaskThread>();
        private readonly List<TaskThread> workingThreads = new List<TaskThread>();

        public TaskThreadPool(int threadCount, string threadPoolName)
        {
            this.threadCount = threadCount;
            this.threadPoolName = threadPoolName;
            this.EnsureThreads();
            this.threadStateChanged = new AutoResetEvent(false);
        }

        private void EnsureThreads()
        {
            if (this.availableThreads.Count == 0)
            {
                for (int i = 0; i < threadCount; i++)
                {
                    var thread = new TaskThread(this.threadPoolName + i.ToString());
                    thread.BeforeExecuteAction = BeforeExecuteTask;
                    thread.AfterExecuteAction = AfterExecuteTask;

                    this.availableThreads.Add(thread);
                }
            }
        }

        private void BeforeExecuteTask(TaskThread thread)
        {
            lock (workingThreads)
            {
                workingThreads.Add(thread);
            }
        }

        private void AfterExecuteTask(TaskThread thread)
        {
            lock (workingThreads)
            {
                workingThreads.Remove(thread);
            }
            lock (availableThreads)
            {
                availableThreads.Add(thread);
            }
            threadStateChanged.Set();
        }

        /// <summary>
        /// ????Task
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public void ExecuteTask(BaseTask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException("task");
            }

            var availableThread = GetAvailableTask();
            availableThread.ExecuteThread(task);
        }

        private TaskThread GetAvailableTask()
        {
            TaskThread thread = null;
            while (true)
            {
                if (availableThreads.Count > 0)
                {
                    lock (availableThreads)
                    {
                        if (availableThreads.Count > 0)
                        {
                            thread = availableThreads[availableThreads.Count - 1];
                            availableThreads.RemoveAt(availableThreads.Count - 1);
                        }
                    }
                }

                if (thread == null)
                {
                    threadStateChanged.WaitOne();
                }
                else
                {
                    break;
                }
            }

            return thread;
        }

        public void WaitForRunningTask()
        {
            while (TotalRunningTasks != 0)
            {
                threadStateChanged.WaitOne();
            }
        }

        /// <summary>
        /// ????running??task
        /// </summary>
        public int TotalRunningTasks
        {
            get
            {
                return workingThreads.Count;
            }
        }

        /// <summary>
        /// ????????????
        /// </summary>
        public int TotalIdleTasks
        {
            get
            {
                return availableThreads.Count;
            }
        }

        /// <summary>
        /// ????????
        /// </summary>
        public void Dispose()
        {
            lock (availableThreads)
            {
                foreach (var thread in availableThreads)
                {
                    thread.Dispose();
                }
                availableThreads.Clear();
            }
            lock (workingThreads)
            {
                foreach (var thread in workingThreads)
                {
                    thread.Dispose();
                }
                workingThreads.Clear();
            }
        }
    }
}