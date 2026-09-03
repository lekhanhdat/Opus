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

namespace AvePoint.GCommon.Transfer.Data.Multiple.Util
{
    public sealed class AveTaskHierarchyScheduler:IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveTaskHierarchyScheduler));

        private AveMultiTaskTree taskTree;
        private AveMultiTaskThreadPool threadPool;
        private ManualResetEvent startedEvent;
        private Thread schedulerThread;
        private StringBuilder errorMessage;
        private AveMultiTask threadRoot;

        public bool IsEmpty
        {
            get
            {
                return taskTree.Root == null;
            }
        }

        internal AveTaskHierarchyScheduler(AveMultiTaskTree taskTree, AveMultiTaskThreadPool pool)
        {
            this.taskTree = taskTree;
            this.threadPool = pool;
            this.startedEvent = new ManualResetEvent(false);
            this.errorMessage = new StringBuilder();

            StartSchedulerThread();
        }

        private void StartSchedulerThread()
        {
            schedulerThread = new Thread(SchedulerThread);
            schedulerThread.IsBackground = true;
            schedulerThread.Name = "SchedulerThread";
            schedulerThread.Start();
        }

        private void CheckError()
        {
            if (errorMessage.Length > 0)
            {
                if (threadPool != null)
                {
                    logger.Error("Error message. Stop all threads.");
                    threadPool.StopAllThreads();
                }
                throw new Exception("Exception occurred: " + errorMessage.ToString());
            }
        }

        public void SchedulerThread()
        {
            try
            {
                startedEvent.WaitOne();

                threadPool.ExecuteTask(new TaskExecutionHierarchyLogic(taskTree, taskTree.Root).ExecuteTask);

                threadRoot = taskTree.Root;
                while ((threadRoot = taskTree.EnsureNext(threadRoot)) != null)
                {
                    if (threadRoot.IsMultiple)
                    {
                        while (threadRoot.Parent.Status < TaskStatus.ProcessEnd)
                        {
                            Thread.Sleep(10);
                        }

                        threadPool.ExecuteTask(new TaskExecutionHierarchyLogic(taskTree, threadRoot).ExecuteTask);
                    }
                }

                logger.Info("The scheduler thread finished normally.");
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                errorMessage.AppendLine(ex.ToString());
            }
        }

        public void Finish()
        {
            CheckError();

            AddTask(null);
            logger.Debug("Scheduler finished. Empty task added.");
        }

        public void Stop(string stopMessage)
        {
            AddTask(null);

            errorMessage.AppendLine(stopMessage);
            logger.Error("Stop scheduler, message:{0}", stopMessage);
        }

        public AveMultiTask AddTask(AveMultiTask task, Action<int> treeIsFullCallback = null)
        {
            CheckError();

            AveMultiTask t;
            try
            {
                t = this.taskTree.AddToTree(task, treeIsFullCallback);
            }
            catch (Exception ex)
            {
                errorMessage.AppendLine(ex.ToString());
                throw;
            }

            this.startedEvent.Set();

            return t;
        }

        public void Wait()
        {
            try
            {
                startedEvent.WaitOne();

                logger.Debug("Start to wait scheduler finished.");

                do
                {
                    CheckError();

                    if (!this.taskTree.Finished || !(this.taskTree.Root.Status >= TaskStatus.Finished))
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                    else
                    {
                        this.threadPool.StopAllThreads();
                        break;
                    }

                } while (true);

                logger.Debug("Wait finished, and stopped all threads.");
            }
            finally
            {
                this.taskTree.Root = null;
            }
        }

        public void Dispose()
        {
            if (startedEvent != null)
            {
                startedEvent.Close();
            }
            if (threadPool != null)
            {
                threadPool.Dispose();
            }
        }
    }

}
