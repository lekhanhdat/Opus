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
using AvePoint.RA.Contract.Tenant;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Common
{
    [Obsolete("This class is obsolete, please use class AvePoint.RA.Common.Threads.AveTenantTasks instead.")]
    public class MultiTasksHelper
    {
        private static RALogger logger = RALogger.GetInstance(typeof(MultiTasksHelper));
        public static int RunAndWaitResult(IAveListItemCollection items, int itemsPerTask, CancellationTokenSource cts, Func<IAveListItem, int> func)
        {
            var finalResults = 0;
            var currentGroupId = TenantLocalValue.LogonGroupId;
            var currentUserId = TenantLocalValue.LogonUserId;
            var currentUserType = TenantLocalValue.AccountType;
            var displayName = TenantLocalValue.DisplayName;
            var currentUserName = TenantLocalValue.LogonUserEmail;
            var currentPrincipal = Thread.CurrentPrincipal;
            var taskCount = (items.Count + itemsPerTask - 1) / itemsPerTask;
            var tasks = new System.Threading.Tasks.Task<int>[taskCount];
            for (var i = 0; i < taskCount; i++)
            {
                tasks[i] = System.Threading.Tasks.Task.Factory.StartNew((state) =>
                {
                    int result = 0;
                    try
                    {

                        TenantLocalValue.LogonGroupId = currentGroupId;
                        TenantLocalValue.LogonUserId = currentUserId;
                        TenantLocalValue.LogonUserEmail = currentUserName;
                        TenantLocalValue.AccountType = currentUserType;
                        TenantLocalValue.DisplayName = displayName;
                        TenantLocalValue.CurrentCulture = null;
                        Thread.CurrentPrincipal = currentPrincipal;

                        var startPos = (int)state;
                        var endPos = (startPos + itemsPerTask) < items.Count ? startPos + itemsPerTask : items.Count;
                        logger.Info($"enter new thread. startPos: {startPos}, endPos : {endPos}");

                        for (var j = startPos; j < endPos; j++)
                        {
                            result += func(items.ElementAt(j));
                        }
                    }
                    catch(Exception e)
                    {
                        logger.Warn($"An error occurred while executing the task. error : {e.ToString()}");
                    }
                    return result;
                },
                i * itemsPerTask,
                cts.Token);
            }
            try
            {
                System.Threading.Tasks.Task.WaitAll(tasks, cts.Token);
            }
            catch(Exception e)
            {
                logger.Warn($"An error occurred while wait all tasks to complete. error : {e.ToString()}");
            }
            
            for (int i = 0; i < tasks.Length; i++)
            {
                finalResults += tasks[i].Result;
            }

            return finalResults;
        }

        public static void RunAndWait(IAveListItemCollection items, int itemsPerTask, CancellationTokenSource cts, Action<IAveListItem> action)
        {
            var currentGroupId = TenantLocalValue.LogonGroupId;
            var currentUserId = TenantLocalValue.LogonUserId;
            var currentUserType = TenantLocalValue.AccountType;
            var displayName = TenantLocalValue.DisplayName;
            var currentUserName = TenantLocalValue.LogonUserEmail;
            var currentPrincipal = Thread.CurrentPrincipal;
            var taskCount = (items.Count + itemsPerTask - 1) / itemsPerTask;
            var tasks = new System.Threading.Tasks.Task[taskCount];
            for (var i = 0; i < taskCount; i++)
            {
                tasks[i] = System.Threading.Tasks.Task.Factory.StartNew((state) =>
                {
                    try
                    {

                        TenantLocalValue.LogonGroupId = currentGroupId;
                        TenantLocalValue.LogonUserId = currentUserId;
                        TenantLocalValue.LogonUserEmail = currentUserName;
                        TenantLocalValue.AccountType = currentUserType;
                        TenantLocalValue.DisplayName = displayName;
                        TenantLocalValue.CurrentCulture = null;
                        Thread.CurrentPrincipal = currentPrincipal;

                        var startPos = (int)state;
                        var endPos = (startPos + itemsPerTask) < items.Count ? startPos + itemsPerTask : items.Count;
                        logger.Info($"enter new thread. startPos: {startPos}, endPos : {endPos}");
                        for (var j = startPos; j < endPos; j++)
                        {
                            action(items.ElementAt(j));
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn($"An error occurred while executing the task. error : {e.ToString()}");
                    }
                },
                i * itemsPerTask,
                cts.Token);
            }
            try
            {
                System.Threading.Tasks.Task.WaitAll(tasks, cts.Token);
            }
            catch (Exception e)
            {
                logger.Warn($"An error occurred while wait all tasks to complete. error : {e.ToString()}");
            }
        }

        public static void RunParallel(IAveListItemCollection items, int itemsPerTask, CancellationTokenSource cts, Action<IAveListItem> action)
        {
            var currentGroupId = TenantLocalValue.LogonGroupId;
            var currentUserId = TenantLocalValue.LogonUserId;
            var currentUserType = TenantLocalValue.AccountType;
            var displayName = TenantLocalValue.DisplayName;
            var currentUserName = TenantLocalValue.LogonUserEmail;
            var currentPrincipal = Thread.CurrentPrincipal;

            var partioner = Partitioner.Create(0, items.Count, itemsPerTask);
            try
            {
                System.Threading.Tasks.Parallel.ForEach(partioner, (range, loopState) =>
                {
                    try
                    {
                        TenantLocalValue.LogonGroupId = currentGroupId;
                        TenantLocalValue.LogonUserId = currentUserId;
                        TenantLocalValue.LogonUserEmail = currentUserName;
                        TenantLocalValue.AccountType = currentUserType;
                        TenantLocalValue.DisplayName = displayName;
                        TenantLocalValue.CurrentCulture = null;
                        Thread.CurrentPrincipal = currentPrincipal;

                        var startPos = range.Item1;
                        var endPos = range.Item2;
                        logger.Info($"enter new paralell task. startPos: {startPos}, endPos : {endPos}");
                        for (var j = startPos; j < endPos; j++)
                        {
                            action(items.ElementAt(j));
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn($"An error occurred while executing the parallel task. error : {e.ToString()}");
                    }
                });
            }
            catch (Exception e)
            {
                logger.Warn($"An error occurred while run paralell tasks. error : {e.ToString()}");
            }
        }
    }
}
