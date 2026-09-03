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
using System.Threading;
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Common.MultiThread
{
    public sealed class TaskThread : IDisposable
    {
        private AutoResetEvent runInfiniteSleeper = new AutoResetEvent(false);
        private AutoResetEvent waitForNewTask = new AutoResetEvent(false);
        private AutoResetEvent waitForNewTaskToStart = new AutoResetEvent(false);
        private Thread thread = null;
        private string threadName = string.Empty;
        private bool runInfinite = false;
        private int sleepTime = -1;
        private bool killThread = false;

        private BaseTask task = null;

        public Action<TaskThread> BeforeExecuteAction { get; set; }
        public Action<TaskThread> AfterExecuteAction { get; set; }

        /// <summary>
        /// ????????????????
        /// </summary>
        public bool IsAlive
        {
            get
            {
                if (this.thread != null)
                {
                    return this.thread.IsAlive;
                }

                return false;
            }
        }

        /// <summary>
        /// ????????????????job
        /// </summary>
        public bool IsAvailable
        {
            get
            {
                if (this.thread == null || this.thread.ThreadState == ThreadState.Stopped)
                {
                    this.InitAndCreateThread(this.threadName);
                    return true;
                }
                //return (((this.thread.ThreadState & ThreadState.WaitSleepJoin) == ThreadState.WaitSleepJoin) && this.task == null);
                return this.task == null;
            }
        }

        /// <summary>
        /// ????????????????
        /// </summary>
        public bool IsInfinite
        {
            get
            {
                return this.runInfinite;
            }
        }

        public bool IsThreadHealthy
        {
            get
            {
                if (this.thread != null)
                {
                    return (this.thread.IsAlive && this.task != null);
                }
                return false;
            }
        }

        public string ThreadName
        {
            get { return threadName; }
        }

        public TaskThread(string threadName)
        {
            InitAndCreateThread(threadName);
        }

        /// <summary>
        /// ??????TaskThread??????????????
        /// </summary>
        /// <param name="threadName"></param>
        private void InitAndCreateThread(string threadName)
        {
            if (this.waitForNewTask == null)
            {
                this.waitForNewTask = new AutoResetEvent(false);
            }
            if (this.waitForNewTaskToStart == null)
            {
                this.waitForNewTaskToStart = new AutoResetEvent(false);
            }
            if (this.runInfiniteSleeper == null)
            {
                this.runInfiniteSleeper = new AutoResetEvent(false);
            }
            this.threadName = threadName;
            this.runInfinite = false;
            this.sleepTime = -1;
            this.killThread = false;
            this.thread = new Thread(MainThread);
            this.thread.Name = this.threadName;
            this.thread.IsBackground = true;
            this.task = null;
            this.thread.Start();
            //while (this.thread.ThreadState != (ThreadState.WaitSleepJoin | ThreadState.Background))
            //{
            //    Thread.Sleep(10);
            //}
        }

        /// <summary>
        /// ????????
        /// </summary>
        private void MainThread()
        {
            try
            {
                while (!this.killThread)
                {
                    this.waitForNewTask.WaitOne();
                    this.waitForNewTaskToStart.Set();
                    do
                    {
                        try
                        {
                            if (this.task != null)
                            {
                                this.task.Process();
                                this.task.CompleteTask();
                            }
                        }
                        catch (Exception ex)
                        {
                            MultiThreadUtility.Logger(AveLogLevel.ERROR, "Process task in thread:{0} failed:{1}", threadName, ex.ToString());
                            if (this.task != null)
                            {
                                this.task.CompleteTask(ex);
                            }
                        }
                        if (!this.runInfinite)
                        {
                            break;
                        }
                        this.runInfiniteSleeper.WaitOne(this.sleepTime, false);
                    }
                    while (this.runInfinite);
                    if (this.task != null)
                    {
                        lock (this.task)
                        {
                            this.task = null;
                        }
                    }
                    if (AfterExecuteAction != null)
                    {
                        AfterExecuteAction(this);
                    }
                }
            }
            catch (Exception ex)
            {
                MultiThreadUtility.Logger(AveLogLevel.ERROR, "The thread:{0} has exception:{1}", threadName, ex.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <param name="runInfinite"></param>
        /// <param name="sleepTime"></param>
        public void ExecuteThread(BaseTask task)
        {
            if (this.IsAvailable)
            {
                if (BeforeExecuteAction != null)
                {
                    BeforeExecuteAction(this);
                }
                this.task = task;
                this.runInfinite = task.RunInfinite;
                this.sleepTime = task.SleepTime;
                this.waitForNewTask.Set();
                this.waitForNewTaskToStart.WaitOne();
            }
            else
            {
                throw new Exception(string.Format("The current thread:{0} is not available.", threadName));
            }
        }

        /// <summary>
        /// Dispose????
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (this.thread != null)
                {
                    this.runInfinite = false;
                    this.killThread = true;
                    this.runInfiniteSleeper.Set();
                    this.waitForNewTask.Set();
                    this.waitForNewTaskToStart.WaitOne(500, false);
                    this.thread = null;
                }
                if (this.runInfiniteSleeper != null)
                {
                    this.runInfiniteSleeper.Close();
                    this.runInfiniteSleeper = null;
                }
                if (this.waitForNewTask != null)
                {
                    this.waitForNewTask.Close();
                    this.waitForNewTask = null;
                }
                if (this.waitForNewTaskToStart != null)
                {
                    this.waitForNewTaskToStart.Close();
                    this.waitForNewTaskToStart = null;
                }
            }
            catch (Exception ex)
            {
                MultiThreadUtility.Logger(AveLogLevel.ERROR, "Release the resource for thread:{0} failed:{1}", threadName, ex.ToString());
            }
        }
    }
}