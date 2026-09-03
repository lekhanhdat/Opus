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
using AvePoint.Hybrid.Utility.Threads;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AvePoint.RA.FileSystem.Core
{
    /// <summary>
    /// 定期去做的点啥
    /// </summary>
    public class STaskManager
    {
        static object locker = new object();
        ManualResetEvent resetEvent = new ManualResetEvent(false);
        List<STask> tasks = new List<STask>();
        public STaskManager()
        {

        }
        public void Insert(string name, Action proc)
        {
            CodeContract.NullOrEmptyThrowing(name, "task name");
            CodeContract.NullThrowing(proc, "task");

            var task = new STask(name, proc);
            lock (locker)
            {
                tasks.Add(task);
            }
        }
        public void StartSchedule()
        {
            AveTenantThread thread = new AveTenantThread(new ThreadStart(SchedueRunSTask));
            thread.Start();
        }
        public void StopSchedule()
        {
            lock (locker)
            {
                tasks = new List<STask>();
            }
            resetEvent.Set();
        }

        private void SchedueRunSTask()
        {
            Random random = new Random();
            int waitSecs = 3;
                //random.Next(5, 8);
            while (!resetEvent.WaitOne(waitSecs * 1000))
            {
                List<STask> localTasks;
                lock (locker)
                {
                    localTasks = new List<STask>(tasks);
                }
                foreach (var task in localTasks)
                {
                    RunSTask(task);
                }
            }
        }

        private void RunSTask(STask task)
        {
            try
            {
                task.Proc();
            }
            catch (Exception)
            {
                //TODO HYW  TRACE 
            }
        }
    }
}
