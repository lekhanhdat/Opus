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
using AvePoint.RA.Contract.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;

namespace AvePoint.GCommon.Utility
{
    public class AveThreadManager
    {
        private static Semaphore mSignals = new Semaphore(0, 20);
        private static AveLogger mLog = AveLogger.GetInstance(typeof(AveThreadManager));
        private static Queue<Action> mOperationsQueue = new Queue<Action>();
        private static List<Action> mPeriodicOperations = new List<Action>();
        /// <summary>
        /// 管理需要起线程执行的操作用以减少进程的线程数以提高效率
        /// </summary>
        static AveThreadManager()
        {
            int interval = 5 * 60 * 1000; //暂时定5分钟，考虑以后通过配置文件来配置
            System.Timers.Timer mTimer= new System.Timers.Timer(interval);
            mTimer.AutoReset = true;
            mTimer.Elapsed += OperatePeriodicQueue;
            mTimer.Start();
            Start();
        }

        /// <summary>
        /// 依次执行等待队列中的操作
        /// </summary>
        private static void Start()
        {
            Thread workThread = new Thread(() =>
            {
                while (true)
                {
                    mSignals.WaitOne();
                    try
                    {
                        lock (mOperationsQueue)
                        {
                            mOperationsQueue.Dequeue()();
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Error("operate registered operation failed.due to:{0}.", ex.ToString());
                    }
                }
            }
                );
            workThread.IsBackground = true;
            workThread.Start();
        }

        /// <summary>
        /// 注册需要执行的操作
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="isPeriodic">是否为周期操作</param>
        public static void RegisterOperation(Action operation, bool isPeriodic)
        {
            if (operation == null)
            {
                throw new ArgumentException("operation");
            }
            if (isPeriodic)
            {
                operation();
                lock (mPeriodicOperations)
                {
                    mPeriodicOperations.Add(operation);
                }
            }
            else
            {
                lock (mOperationsQueue)
                {
                    mOperationsQueue.Enqueue(operation);
                    mSignals.Release();
                }
            }
        }
        /// <summary>
        /// 将周期操作队列加入到执行队列
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OperatePeriodicQueue(object sender, ElapsedEventArgs e)
        {
            lock (mPeriodicOperations)
            {
                if (mPeriodicOperations.Count > 0)
                {
                    foreach (var item in mPeriodicOperations)
                    {
                        RegisterOperation(item, false);
                    }
                }
            }
        }
        /// <summary>
        /// 删除已经注册的周期操作
        /// </summary>
        /// <param name="operation"></param>
        public static void DeletePeriodicOperation(Action operation)
        {
            if (operation == null)
            {
                throw new ArgumentException("operation");
            }
            lock (mPeriodicOperations)
            {
                if (mPeriodicOperations.Contains(operation))
                {
                    mPeriodicOperations.Remove(operation);
                }
            }
        }

    }
}
