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
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.TransientFault
{
    internal class AveAsyncExecution : AveAsyncExecution<bool>
    {
        private static Task<bool> cachedBoolTask;
        public AveAsyncExecution(Func<Task> taskAction, ShouldRetry shouldRetry, Func<Exception, bool> isTransient, Action<int, Exception, TimeSpan> onRetrying, bool fastFirstRetry, CancellationToken cancellationToken)
            : base(() => AveAsyncExecution.StartAsGenericTask(taskAction), shouldRetry, isTransient, onRetrying, fastFirstRetry, cancellationToken)
        {
        }
        private static Task<bool> StartAsGenericTask(Func<Task> taskAction)
        {
            Task task = taskAction();
            if (task == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The specified argument '{0}' cannot return a null task when invoked.", new object[]
				{
					"taskAction"
				}), "taskAction");
            }
            if (task.Status == TaskStatus.RanToCompletion)
            {
                return AveAsyncExecution.GetCachedTask();
            }
            if (task.Status == TaskStatus.Created)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The specified argument '{0}' must return a scheduled task (also known as \"hot\" task) when invoked.", new object[]
				{
					"taskAction"
				}), "taskAction");
            }
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            task.ContinueWith(delegate(Task t)
            {
                if (t.IsFaulted)
                {
                    tcs.TrySetException(t.Exception.InnerExceptions);
                    return;
                }
                if (t.IsCanceled)
                {
                    tcs.TrySetCanceled();
                    return;
                }
                tcs.TrySetResult(true);
            }, TaskContinuationOptions.ExecuteSynchronously);
            return tcs.Task;
        }
        private static Task<bool> GetCachedTask()
        {
            if (AveAsyncExecution.cachedBoolTask == null)
            {
                TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
                taskCompletionSource.TrySetResult(true);
                AveAsyncExecution.cachedBoolTask = taskCompletionSource.Task;
            }
            return AveAsyncExecution.cachedBoolTask;
        }
    }

    internal class AveAsyncExecution<TResult>
    {
        private RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Func<Task<TResult>> taskFunc;
        private readonly ShouldRetry shouldRetry;
        private readonly Func<Exception, bool> isTransient;
        private readonly Action<int, Exception, TimeSpan> onRetrying;
        private readonly bool fastFirstRetry;
        private readonly CancellationToken cancellationToken;
        private Task<TResult> previousTask;
        private int retryCount;
        public AveAsyncExecution(Func<Task<TResult>> taskFunc, ShouldRetry shouldRetry, Func<Exception, bool> isTransient, Action<int, Exception, TimeSpan> onRetrying, bool fastFirstRetry, CancellationToken cancellationToken)
        {
            this.taskFunc = taskFunc;
            this.shouldRetry = shouldRetry;
            this.isTransient = isTransient;
            this.onRetrying = onRetrying;
            this.fastFirstRetry = fastFirstRetry;
            this.cancellationToken = cancellationToken;
        }
        internal Task<TResult> ExecuteAsync()
        {
            return this.ExecuteAsyncImpl(null);
        }
        private Task<TResult> ExecuteAsyncImpl(Task ignore)
        {
            if (this.cancellationToken.IsCancellationRequested)
            {
                if (this.previousTask != null)
                {
                    return this.previousTask;
                }
                TaskCompletionSource<TResult> taskCompletionSource = new TaskCompletionSource<TResult>();
                taskCompletionSource.TrySetCanceled();
                return taskCompletionSource.Task;
            }
            else
            {
                Task<TResult> task;
                try
                {
                    logger.Debug("Starting to async execute with {0} time(s).", this.retryCount);
                    task = this.taskFunc();
                }
                catch (Exception ex)
                {
                    if (!this.isTransient(ex))
                    {
                        throw;
                    }
                    TaskCompletionSource<TResult> taskCompletionSource2 = new TaskCompletionSource<TResult>();
                    taskCompletionSource2.TrySetException(ex);
                    task = taskCompletionSource2.Task;
                }
                if (task == null)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The specified argument '{0}' cannot return a null task when invoked.", new object[]
					{
						"taskFunc"
					}), "taskFunc");
                }
                if (task.Status == TaskStatus.RanToCompletion)
                {
                    return task;
                }
                if (task.Status == TaskStatus.Created)
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The specified argument '{0}' must return a scheduled task (also known as \"hot\" task) when invoked.", new object[]
					{
						"taskFunc"
					}), "taskFunc");
                }
                return task.ContinueWith<Task<TResult>>(new Func<Task<TResult>, Task<TResult>>(this.ExecuteAsyncContinueWith), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default).Unwrap<TResult>();
            }
        }
        private Task<TResult> ExecuteAsyncContinueWith(Task<TResult> runningTask)
        {
            if (!runningTask.IsFaulted || this.cancellationToken.IsCancellationRequested)
            {
                return runningTask;
            }
            TimeSpan zero = TimeSpan.Zero;
            Exception innerException = runningTask.Exception.InnerException;
            if (innerException is AvePoint.RA.Common.TransientFault.AveRetryLimitExceededException)
            {
                TaskCompletionSource<TResult> taskCompletionSource = new TaskCompletionSource<TResult>();
                if (innerException.InnerException != null)
                {
                    taskCompletionSource.TrySetException(innerException.InnerException);
                }
                else
                {
                    taskCompletionSource.TrySetCanceled();
                }
                return taskCompletionSource.Task;
            }
            if (!this.isTransient(innerException) || !this.shouldRetry(this.retryCount++, innerException, out zero))
            {
                return runningTask;
            }
            if (zero < TimeSpan.Zero)
            {
                zero = TimeSpan.Zero;
            }
            this.onRetrying(this.retryCount, innerException, zero);
            this.previousTask = runningTask;
            if (zero > TimeSpan.Zero && (this.retryCount > 1 || !this.fastFirstRetry))
            {
                return Task.Delay(zero).ContinueWith<Task<TResult>>(new Func<Task, Task<TResult>>(this.ExecuteAsyncImpl), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default).Unwrap<TResult>();
            }
            return this.ExecuteAsyncImpl(null);
        }
    }
}
