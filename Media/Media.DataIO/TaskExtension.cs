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
using Storage;
using System;
using System.Linq;

namespace MediaContract
{
    public static class TaskExtension
    {
        public static (XFileInfo? FileInfo, IXSystem WorkingSystem) OpenFileExt(this IXSystem xSystem, StorageInfo storageInfo)
        {
            if (xSystem is XLibrary library)
            {
                var defaultSystem = library.GetWorkingSystem();
                var fileInfo = defaultSystem.OpenFile(storageInfo);
                if (fileInfo == null || !fileInfo.Exists)
                {
                    foreach (var subSystem in library.SubSystems)
                    {
                        if (subSystem != defaultSystem)
                        {
                            var subFileInfo = subSystem.OpenFile(storageInfo);
                            if (subFileInfo != null && subFileInfo.Exists)
                            {
                                defaultSystem = subSystem;
                                fileInfo = subFileInfo;
                                break;
                            }
                        }
                    }
                }
                return (fileInfo, defaultSystem);
            }
            return (xSystem.OpenFile(storageInfo), xSystem);
        }

        public static T ExecuteAsyncTask<T>(this Task<T> task)
        {
            return task.ConfigureAwait(continueOnCapturedContext: false).GetAwaiter().GetResult();
        }

        public static void ExecuteAsyncTask(this Task task)
        {
            task.ConfigureAwait(continueOnCapturedContext: false).GetAwaiter().GetResult();
        }

        public static T ExecuteAsyncTask<T>(this ValueTask<T> task)
        {
            return task.ConfigureAwait(continueOnCapturedContext: false).GetAwaiter().GetResult();
        }

        public static void ExecuteAsyncTask(this ValueTask task)
        {
            task.ConfigureAwait(continueOnCapturedContext: false).GetAwaiter().GetResult();
        }

        public static Task<TResult> WithState<TResult>(this Task<TResult> task, object state)
        {
            if (task.AsyncState == state)
            {
                return task;
            }

            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>(state);
            task.ContinueWith(delegate (Task<TResult> t)
            {
                if (t.IsFaulted)
                {
                    TaskCompletionSource<TResult> taskCompletionSource = tcs;
                    IEnumerable<Exception> enumerable = t.Exception?.InnerExceptions;
                    taskCompletionSource.TrySetException(enumerable ?? new List<Exception>());
                }
                else if (t.IsCanceled)
                {
                    tcs.TrySetCanceled();
                }
                else
                {
                    tcs.TrySetResult(t.Result);
                }
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            return tcs.Task;
        }
    }
}
