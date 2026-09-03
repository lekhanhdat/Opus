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

using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Global.Exceptions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Common.Threads
{
    public class AveTenantTasks
    {
        #region private

        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(AveTenantTasks));
        private static void DealWithAggregateException(AggregateException ex)
        {
            foreach (var e1 in ex.InnerExceptions)
            {
                if (e1 is JobStopException) throw e1;
            }
        }
        #endregion
        public static void RunParallel<TSource>(IEnumerable<TSource> items, int itemsPerTask, CancellationTokenSource cts, Action<TSource> action)
        {
            //RunParallel<T>(items, cts, action);
            
            //var setting = ThreadSetting.GetSetting();

            var partioner = Partitioner.Create(0, items.Count(), itemsPerTask);
            RunParallel(partioner, items, cts, action);
        }
        /// <summary>
        /// 方法按Enumerable参数执行， Added by jlnan for performance improve
        /// </summary> 
        public static void RunParallelBatch<TSource>(IEnumerable<TSource> items, int itemsPerTask, CancellationTokenSource cts, Action<IEnumerable<TSource>> action)
        {
            //RunParallel<T>(items, cts, action);

            //var setting = ThreadSetting.GetSetting();

            var partioner = Partitioner.Create(0, items.Count(), itemsPerTask);
            RunParallelBatch(partioner, items, cts, action);
        }
        public static int RunAndWaitResult<TSource>(IEnumerable<TSource> items, int itemsPerTask, CancellationTokenSource cts, Func<TSource, int> func)
        {
            //return RunAndWaitResult(items, cts, func);
            #region
            var total = 0;
            //var setting = ThreadSetting.GetSetting();
            var partioner = Partitioner.Create(0, items.Count(), itemsPerTask);
            try
            {
                System.Threading.Tasks.Parallel.ForEach(
                    partioner,
                    () => 0,
                    (range, loopState, tempResult) =>
                    {
                        try
                        {
                            if (loopState.IsStopped) return tempResult;
                            if (cts.IsCancellationRequested)
                            {
                                loopState.Stop();
                                return tempResult;
                            }

                            //ThreadSetting.SetSetting(setting);

                            var startPos = range.Item1;
                            var endPos = range.Item2;
                            logger.Info($"enter new parallel task. startPos: {startPos}, endPos : {endPos}");
                            for (var j = startPos; j < endPos; j++)
                            {
                                tempResult += func(items.ElementAt(j));
                            }
                        }
                        catch (JobStopException ex)
                        {
                            logger.Warn("Job is stopped.");
                            loopState.Stop();
                            throw ex;
                        }
                        catch (Exception e)
                        {
                            logger.Warn($"An error occurred while executing the parallel task. error : {e.ToString()}");
                        }
                        return tempResult;
                    },
                    (finalResult) => Interlocked.Add(ref total, finalResult)
                );
            }
            catch (AggregateException ex)
            {
                DealWithAggregateException(ex);
            }
            catch (Exception e)
            {
                logger.Warn($"An error occurred while run parallel tasks. error : {e.ToString()}");
            }

            return total;
            #endregion
        }
        public static void RunAndWaitTasks<TSource>(IEnumerable<TSource> items, CancellationTokenSource cts, Action<TSource> action)
        {
            //var setting = ThreadSetting.GetSetting();
            var tasks = new List<System.Threading.Tasks.Task>();
            foreach (var item in items)
            {
                tasks.Add(System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    try
                    {
                        //ThreadSetting.SetSetting(setting);

                        action(item);
                    }
                    catch (JobStopException ex)
                    {
                        cts.Cancel();
                        logger.Warn("Job is stopped.");
                        throw ex;
                    }
                    catch (Exception e)
                    {
                        logger.Error($"An error occurred while executing the task. error : {e.ToString()}");
                    }
                },
                cts.Token));
            }
            try
            {
                if (tasks.Count == 0) return;

                System.Threading.Tasks.Task.WaitAll(tasks.ToArray(), cts.Token);
            }
            catch (AggregateException ex)
            {
                DealWithAggregateException(ex);
            }
            catch (Exception e)
            {
                logger.Warn($"An error occurred while wait all tasks to complete. error : {e.ToString()}");
            }

        }

        /// <summary>
        /// will create the parallel tasks based on CPU numbers.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="items"></param>
        /// <param name="cts"></param>
        /// <param name="action"></param>
        public static void RunParallel<TSource>(IEnumerable<TSource> items, CancellationTokenSource cts, Action<TSource> action)
        {
            //var setting = ThreadSetting.GetSetting();
            var cpuNumbers = OSInformation.CPUCount;
            var itemsCount = items.Count();
            var partioner = Partitioner.Create(0, itemsCount, itemsCount / cpuNumbers);

            RunParallel(partioner, items, cts, action);

        }

        private static void RunParallel<TSource>(Partitioner<Tuple<int,int>> partioner, IEnumerable<TSource> items, CancellationTokenSource cts, Action<TSource> action)
        {
            try
            {
                System.Threading.Tasks.Parallel.ForEach(partioner, (range, loopState) =>
                {
                    try
                    {
                        if (loopState.IsStopped) return;
                        if (cts.IsCancellationRequested)
                        {
                            loopState.Stop();
                            return;
                        }

                        //ThreadSetting.SetSetting(setting);

                        var startPos = range.Item1;
                        var endPos = range.Item2;
                        logger.Info($"enter new parallel task. startPos: {startPos}, endPos : {endPos}");
                        for (var j = startPos; j < endPos; j++)
                        {
                            action(items.ElementAt(j));
                        }
                    }
                    catch (JobStopException ex)
                    {
                        logger.Warn("Job is stopped.");
                        loopState.Stop();
                        throw ex;
                    }
                    catch (Exception e)
                    {
                        logger.Warn($"An error occurred while executing the parallel task. error : {e.ToString()}");
                    }
                });
            }
            catch (Exception e)
            {
                logger.Warn($"An error occurred while run parallel tasks. error : {e.ToString()}");
            }
        }
        private static void RunParallelBatch<TSource>(Partitioner<Tuple<int, int>> partioner, IEnumerable<TSource> items, CancellationTokenSource cts, Action<IEnumerable<TSource>> action)
        {
            try
            {
                System.Threading.Tasks.Parallel.ForEach(partioner, (range, loopState) =>
                {
                    try
                    {
                        if (loopState.IsStopped) return;
                        if (cts.IsCancellationRequested)
                        {
                            loopState.Stop();
                            return;
                        }

                        //ThreadSetting.SetSetting(setting);

                        var startPos = range.Item1;
                        var endPos = range.Item2;
                        logger.Info($"enter new parallel task. startPos: {startPos}, endPos : {endPos}");
                        List<TSource> tempList = new List<TSource>();
                        for (var j = startPos; j < endPos; j++)
                        {
                            tempList.Add(items.ElementAt(j));
                        }
                        action(tempList);
                    }
                    catch (JobStopException ex)
                    {
                        logger.Warn("Job is stopped.");
                        loopState.Stop();
                        throw ex;
                    }
                    catch (Exception e)
                    {
                        logger.Warn($"An error occurred while executing the parallel task batch. error : {e.ToString()}");
                    }
                });
            }
            catch (Exception e)
            {
                logger.Warn($"An error occurred while run parallel tasks. error : {e.ToString()}");
            }
        }
        private static int RunAndWaitResult<TSource>(Partitioner<Tuple<int, int>> partioner, IEnumerable<TSource> items, CancellationTokenSource cts, Func<TSource, int> func)
        {
            #region
            var total = 0;

            try
            {
                System.Threading.Tasks.Parallel.ForEach(
                    partioner,
                    () => 0,
                    (range, loopState, tempResult) =>
                    {
                        try
                        {
                            if (loopState.IsStopped) return tempResult;
                            if (cts.IsCancellationRequested)
                            {
                                loopState.Stop();
                                return tempResult;
                            }

                            //ThreadSetting.SetSetting(setting);

                            var startPos = range.Item1;
                            var endPos = range.Item2;
                            logger.Info($"enter new parallel task. startPos: {startPos}, endPos : {endPos}");
                            for (var j = startPos; j < endPos; j++)
                            {
                                tempResult += func(items.ElementAt(j));
                            }
                        }
                        catch (Exception e)
                        {
                           logger.Warn($"An error occurred while executing the parallel task. error : {e.ToString()}");
                        }
                        return tempResult;
                    },
                    (finalResult) => Interlocked.Add(ref total, finalResult)
                );
            }
            catch (AggregateException ex)
            {
                DealWithAggregateException(ex);
            }
            catch (Exception e)
            {
                logger.Warn($"An error occurred while run parallel tasks. error : {e.ToString()}");
            }

            return total;
            #endregion
        }

        #region obsolete
        [Obsolete("This method may result in the issue of 'There were not enough free threads in the ThreadPool to complete the operation'.")]
        public static void RunParallel<TSource, TArg>(IEnumerable<TSource> items, CancellationTokenSource cts, Action<TSource, TArg> action, TArg arg)
        {
            //var setting = ThreadSetting.GetSetting();

            try
            {
                System.Threading.Tasks.Parallel.ForEach(items, item =>
                {
                    try
                    {
                        //ThreadSetting.SetSetting(setting);

                        action(item, arg);
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
        #endregion

        /// <summary>
        /// will create the parallel tasks based on CPU numbers to caculate the result.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="items"></param>
        /// <param name="cts"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static int RunAndWaitResult<TSource>(IEnumerable<TSource> items, CancellationTokenSource cts, Func<TSource, int> func)
        {
            #region
            var total = 0;
           // var setting = ThreadSetting.GetSetting();
            
            var cpuNumbers = OSInformation.CPUCount;
            //var partioner = Partitioner.Create(0, items.Count(), itemsPerTask);
            var itemsCount = items.Count();
            var partioner = Partitioner.Create(0, itemsCount, itemsCount / cpuNumbers);
            try
            {
                System.Threading.Tasks.Parallel.ForEach(
                    partioner,
                    () => 0,
                    (range, loopState, tempResult) =>
                    {
                        try
                        {
                            if (loopState.IsStopped) return tempResult;
                            if (cts.IsCancellationRequested)
                            {
                                loopState.Stop();
                                return tempResult;
                            }

                            //ThreadSetting.SetSetting(setting);

                            var startPos = range.Item1;
                            var endPos = range.Item2;
                            logger.Info($"enter new parallel task. startPos: {startPos}, endPos : {endPos}");
                            for (var j = startPos; j < endPos; j++)
                            {
                                tempResult += func(items.ElementAt(j));
                            }
                        }
                        catch (JobStopException ex)
                        {
                            logger.Warn("Job is stopped.");
                            loopState.Stop();
                            throw ex;
                        }
                        catch (Exception e)
                        {
                            logger.Warn($"An error occurred while executing the parallel task. error : {e.ToString()}");
                        }
                        return tempResult;
                    },
                    (finalResult) => Interlocked.Add(ref total, finalResult)
                );
            }
            catch (AggregateException ex)
            {
                DealWithAggregateException(ex);
            }
            catch (Exception e)
            {
                logger.Warn($"An error occurred while run parallel tasks. error : {e.ToString()}");
            }

            return total;
            #endregion
        }


        public static void RunAndWaitByOneTask<TSource>(IEnumerable<TSource> items, CancellationTokenSource cts, Action<TSource> action)
        {
            //var setting = ThreadSetting.GetSetting();
            logger.Info($"Enter one task run.");
            List<Task> tasks = new List<Task>();
            foreach (var item in items)
            {
                var tempTask = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        //ThreadSetting.SetSetting(setting);
                        action(item);
                        
                
                    }
                    catch (JobStopException ex)
                    {
                        cts.Cancel();
                        logger.Warn("Job is stopped.");
                        throw ex;
                    }
                    catch (Exception e)
                    {
                        logger.Error($"An error occurred while executing the one task. error : {e.ToString()}");
                    }
                },
                cts.Token);
                tasks.Add(tempTask);
            }
            try
            {
                System.Threading.Tasks.Task.WaitAll(tasks.ToArray(), cts.Token);
            }
            catch (AggregateException ex)
            {
                DealWithAggregateException(ex);
            }
            catch (Exception e)
            {
                logger.Warn($"An error occurred while wait one tasks to complete. error : {e.ToString()}");
            }

        }

    }
}
