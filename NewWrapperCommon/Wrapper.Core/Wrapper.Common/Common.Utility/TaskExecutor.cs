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
using System.Text;
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Common
{
    public delegate void Task();

    public class AveTaskExecutor : IDisposable
    {
        private static AveLogger Log = AveLogger.GetInstance(typeof(AveLogger));
        private Semaphore mWorkerThreads;

        public AveTaskExecutor(int maximumThreads)
        {
            mWorkerThreads = new Semaphore(maximumThreads, maximumThreads);
        }

        public void Execute(ICollection<Task> tasks)
        {
            if (tasks.Count == 0)
            {
                return;
            }
            AveCountdownLatch taskLatch = new AveCountdownLatch(tasks.Count);
            foreach (Task task in tasks)
            {
                mWorkerThreads.WaitOne();
                AsyncExecuteTask(task, taskLatch);
            }
            taskLatch.WaitOne();
            taskLatch.Close();
        }

        private void AsyncExecuteTask(Task task, AveCountdownLatch latch)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    task();
                }
                catch(Exception e)
                {
                    Log.Error("failed to execute task, message:{0}.", e.ToString());
                }
                finally
                {                    
                    mWorkerThreads.Release();
                    latch.Release();
                }
            });
        }

        public void Dispose()
        {
            if (mWorkerThreads != null)
            {
                mWorkerThreads.Close();
            }
        }
    }
}
