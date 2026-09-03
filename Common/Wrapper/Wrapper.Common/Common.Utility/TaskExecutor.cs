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
using AvePoint.Wrapper.Common.Common.Utility;

namespace AvePoint.Wrapper.Common
{
    public delegate void DelegateTask();

    public class TaskExecutionResult
    {
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public int TotalTaskCount { get; set; }
        public int FailedTaskCount { get; set; }
        public List<string> FailedErrorMessage { get; set; }

        private readonly object locker = new object(); 

        public void Finish()
        {
            lock (locker)
            {
                FinishTime = DateTime.Now;
            }
        }
        public void RecordFailed(string message)
        {
            lock (locker)
            {
                FailedTaskCount++;
                FailedErrorMessage.Add(message);
            }
        }
    }

    public class CountableTaskExecutor : IDisposable
    {
        private static AveLogger Log = AveLogger.GetInstance(typeof(AveLogger));
        private Semaphore mWorkerThreads;

        public CountableTaskExecutor(int maxThreadCount) 
        {
            mWorkerThreads = new Semaphore(maxThreadCount, maxThreadCount);
        }

        public TaskExecutionResult Execute(ICollection<DelegateTask> tasks, bool throwIfHasError=false)
        {
            TaskExecutionResult result= new TaskExecutionResult
            {
                StartTime = DateTime.Now,
                FailedErrorMessage = new List<string>{ },
                TotalTaskCount = tasks.Count,
                FailedTaskCount = 0,
            };
            if (tasks.Count == 0)
            {
                result.Finish();
                return result;
            }
            AveCountdownLatch taskLatch = new AveCountdownLatch(tasks.Count);
            foreach (DelegateTask task in tasks)
            {
                mWorkerThreads.WaitOne();
                AsyncExecuteTask(task, taskLatch,(string msg)=>
                {
                    result.RecordFailed(msg);
                });
            }
            taskLatch.WaitOne();
            taskLatch.Close();
            result.Finish();
            if (result.FailedTaskCount > 0)
            {
                StringBuilder errorMessageBuilder = new StringBuilder();
                errorMessageBuilder.AppendLine("One or more error happens when executing tasks.Details:");
                errorMessageBuilder.AppendLine("ErrorCount:"+ result.FailedTaskCount);
                result.FailedErrorMessage.ForEach(t=> { errorMessageBuilder.AppendLine(t); });
                Log.Error("One or more error happens when executing tasks.Details:{0}",errorMessageBuilder);
                if (throwIfHasError)
                {
                    throw new AveWrapperI18NException(errorMessageBuilder.ToString());
                }
            }
            return result;
        }

        private void AsyncExecuteTask(DelegateTask task, AveCountdownLatch latch,Action<string> onTaskFailed)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    task();
                }
                catch (Exception e)
                {
                    Log.Error("Failed to execute task,error:{0}.", e);
                    onTaskFailed(e.Message);
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

    [Obsolete("Use CountableTaskExecutor instead")]
    public class AveTaskExecutor : IDisposable
    {
        private static AveLogger Log = AveLogger.GetInstance(typeof(AveLogger));
        private Semaphore mWorkerThreads;

        public AveTaskExecutor(int maxThreadCount)
        {
            mWorkerThreads = new Semaphore(maxThreadCount, maxThreadCount);
        }

        public void AlterThreadCount(int maxThreadCount)
        {
            if (mWorkerThreads != null)
            {
                mWorkerThreads.Close();
            }
            mWorkerThreads = new Semaphore(maxThreadCount, maxThreadCount);
        }

        public void Execute(ICollection<DelegateTask> tasks)
        {
            if (tasks.Count == 0)
            {
                return;
            }
            AveCountdownLatch taskLatch = new AveCountdownLatch(tasks.Count);
            foreach (DelegateTask task in tasks)
            {
                mWorkerThreads.WaitOne();
                AsyncExecuteTask(task, taskLatch);
            }
            taskLatch.WaitOne();
            taskLatch.Close();
        }

        private void AsyncExecuteTask(DelegateTask task, AveCountdownLatch latch)
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

    public class AveAppendableTaskExecutor : IDisposable
    {
        private static AveLogger Log = AveLogger.GetInstance(typeof(AveLogger));
        private Semaphore mWorkerThreads;
        private BlockingQueue<DelegateTask> pendingTasks;
        private AveCountdownLatch mTaskLatch;
        private const int DEFAULT_WAIT_TIMEOUT = 12 * 60 * 60 * 1000;
        protected int WaitTimeout{get;set;}

        public AveAppendableTaskExecutor(int maxThreadCount,int waitTimeout=DEFAULT_WAIT_TIMEOUT)
        {
            WaitTimeout = waitTimeout;
            mWorkerThreads = new Semaphore(maxThreadCount, maxThreadCount);
            pendingTasks = new BlockingQueue<DelegateTask>(20);            
            mTaskLatch = new AveCountdownLatch(0, true);
        }

        public void StartExecute()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    while (true)
                    {
                        if (pendingTasks.IsClosed)
                        {
                            break;
                        }

                        DelegateTask task = pendingTasks.Dequeue();

                        if (task == null)
                        {
                            break;
                        }

                        mWorkerThreads.WaitOne();

                        AsyncExecuteTask(task);
                    }
                }
                catch (Exception e)
                {
                    Log.Warn("Appendable task executor encountered an exception , error msg : {0}", e.ToString());
                }
                finally
                {
                    Log.Info("Appendable task executor has exited.");
                }
            });
        }

        private void AsyncExecuteTask(DelegateTask task)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {                                        
                    task();
                }
                catch (Exception e)
                {
                    Log.Error("failed to execute task, message:{0}.", e.ToString());
                }
                finally
                {
                    mWorkerThreads.Release();
                    mTaskLatch.Release();
                }
            });
        }

        public void AddTask(DelegateTask task)
        {
            mTaskLatch.Wait();
            pendingTasks.Enqueue(task);
        }

        public void ResetThreadThreshold(int threshold)
        {
            mWorkerThreads.Close();
            mWorkerThreads = new Semaphore(threshold, threshold);
            mTaskLatch.Reset();
        }

        public void WaitForAllTasks()
        {
            WaitForAllTasks(WaitTimeout);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout">The number of milliseconds to wait, or System.Threading.Timeout.Infinite (-1) to wait indefinitely.</param>
        /// <returns>True: all task completed. False: timeout</returns>
        public bool WaitForAllTasks(int timeout)
        {
            var isTimeout = WaitHandle.WaitAny(new WaitHandle[] { mTaskLatch }, timeout) == WaitHandle.WaitTimeout;
            if (isTimeout)
            {
                Log.Error("Wait for executing tasks time out.");
            }
            mTaskLatch.Reset();
            return !isTimeout;
        }        

        public void Dispose()
        {
            if (pendingTasks != null)
            {
                pendingTasks.Close();
            }
            if (mWorkerThreads != null)
            {
                mWorkerThreads.Close();
            }            
            if (mTaskLatch != null)
            {
                mTaskLatch.Close();
            }
        }
    }
}
