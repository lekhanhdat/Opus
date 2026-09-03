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

namespace AvePoint.RA.SharePoint.ArchiverCommon
{
    internal sealed class AveMultiTaskThread : IDisposable
    {
        private static RALogger logger = RALogger.GetInstance(typeof(AveMultiTaskThread));

        private AutoResetEvent waitting;
        private bool continueRunning;
        private Thread thread;
        private Action taskExecution;

        public AveMultiTaskThread(string threadName)
        {
            this.continueRunning = true;
            this.waitting = new AutoResetEvent(false);

            this.thread = StartThread(threadName);
        }

        private Thread StartThread(string threadName)
        {
            var thread = new Thread(ExecutionThread);
            thread.IsBackground = true;
            thread.Name = threadName;
            thread.Start();

            return thread;
        }
        private void ExecutionThread()
        {
            try
            {
                while (continueRunning)
                {
                    this.waitting.WaitOne();
                    if (object.Equals(this.taskExecution, null))
                    {
                        logger.Info("The task thread finished. Name: {0}", thread.Name);
                        break;
                    }

                    try
                    {
                        this.taskExecution();
                    }
                    finally
                    {
                        this.taskExecution = null;
                    }
                }

                logger.Info("Task thread exit: {0}", thread.Name);
            }
            catch (Exception ex)
            {
                logger.Error("Critical error in thread: {0}, {1}", thread.Name, ex.ToString());
            }
        }

        public void ExecuteTask(Action taskExecution)
        {
            if (this.taskExecution != null) throw new Exception("Invalid logic.");

            this.taskExecution = taskExecution;
            this.waitting.Set();
        }

        public bool IsAvailable()
        {
            return (continueRunning && this.taskExecution == null);
        }

        public void Stop()
        {
            logger.Debug("Stop thread: {0}", thread.Name);

            this.continueRunning = false;
            this.waitting.Set();
        }


        public void Dispose()
        {
            if (waitting != null)
            {
                waitting.Close();
            }
        }

        public bool ForceDispose()
        {
            if (this.thread != null)
            {
                if (this.thread.IsAlive)
                {
                    try
                    {
                        logger.Info(string.Format("wait for thread [{0}] exit.", thread.Name));
                        if (!this.thread.Join(2 * 1000))
                        {
                            logger.Info(string.Format("wait for thread [{0}] exit timeout, so abort it.", thread.Name));
                            this.thread.Abort();
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn(e.ToString());
                    }
                    bool value = this.thread.IsAlive;
                    if (!value)
                    {
                        Dispose();
                    }
                    return !value;
                }
            }
            Dispose();
            return true;
        }
    }
}
