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
using System.Threading;

namespace AvePoint.RA.SharePoint.ArchiverCommon
{
    internal sealed class AveMultiTaskThreadPool:IDisposable
    {
        private int maxCount;
        private List<AveMultiTaskThread> threads;

        public AveMultiTaskThreadPool(int maxCount)
        {
            this.maxCount = maxCount;
            this.threads = new List<AveMultiTaskThread>(maxCount);

            EnsureThreads();
        }

        private void EnsureThreads()
        {
            for (int i = 0; i < maxCount; i++)
            {
                this.threads.Add(new AveMultiTaskThread(Thread.CurrentThread.Name + "_Multi" + i));
            }
        }

        private AveMultiTaskThread GetAvailableThread()
        {
            return this.threads.FirstOrDefault(t => t.IsAvailable());
        }

        public void ExecuteTask(Action taskExecution)
        {
            AveMultiTaskThread thread;
            while ((thread = GetAvailableThread()) == null)
            {
                Thread.Sleep(10);
            }

            thread.ExecuteTask(taskExecution);
        }

        public void StopAllThreads()
        {
            threads.ForEach(t => t.Stop());
        }

        public void Dispose()
        {
            if (threads != null)
            {
                foreach (var t in threads)
                {
                    t.Dispose();
                }
            }
        }
    }
}
