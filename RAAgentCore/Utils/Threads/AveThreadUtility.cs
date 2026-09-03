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

namespace AvePoint.Hybrid.Utility.Threads
{
    using AvePoint.GCommon;
    #region using directives
    using AvePoint.RA.CommonUtil;
    using AvePoint.RA.Contract.Services;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    #endregion

    public class AveThreadUtility
    {
        static AveLogger logger = AveLogger.GetInstance(typeof(AveThreadUtility));
        static Dictionary<Int32, AveThreadWrapper> threads = new Dictionary<Int32, AveThreadWrapper>();
        static AveThreadWrapper monitorThreadWrapper;
        static String debugFileName = "C:\\DumpMonitoredThreads.AvePoint";

        static AveThreadUtility()
        {
            monitorThreadWrapper = StartThread(MonitorThread, "MonitorThread", string.Empty);
        }

        public static void Init(string checkFileName)
        {
            if (!string.IsNullOrEmpty(checkFileName))
            {
                debugFileName = checkFileName;
            }
        }

        public static AveThreadWrapper StartThread(ThreadStart start, string threadName, string groupId)
        {
            AveThreadWrapper threadWrapper = new AveThreadWrapper(start, threadName, groupId);
            AddThreadWrapper(threadWrapper);
            threadWrapper.Start();

            return threadWrapper;
        }

        public static AveThreadWrapper StartThread(ParameterizedThreadStart start, object obj, string threadName, string groupId)
        {
            AveThreadWrapper threadWrapper = new AveThreadWrapper(start, obj, threadName, groupId);
            AddThreadWrapper(threadWrapper);
            threadWrapper.Start();

            return threadWrapper;
        }

        public static void SafeStopAllThreads(int millisecondsTimeout = 60000, string message = "")
        {
            SafeStopThread(string.Empty, millisecondsTimeout, message);
        }

        public static void SafeStopThread(string groupId, int millisecondsTimeout = 60000, string message = "")
        {
            List<AveThreadWrapper> filtererThreads = GetThreadByGroup(groupId, false);

            //先设置每个Thread的控制，让所有Thread并行停止，不能一下子停止一个。
            foreach (var thread in filtererThreads)
            {
                thread.KeepRunning = false;
            }

            foreach (AveThreadWrapper thread in filtererThreads)
            {
                SafeStopThread(thread, millisecondsTimeout, message, false);
            }
        }

        public static void SafeStopThread(AveThreadWrapper thread, int millisecondsTimeout, string message, bool removeItemInCollection = true)
        {
            thread.SafeStop(millisecondsTimeout, message);

            if (removeItemInCollection)
            {
                lock (threads)
                {
                    if (threads.ContainsKey(thread.ManagedThreadId))
                    {
                        threads.Remove(thread.ManagedThreadId);
                    }
                }
            }
        }

        /// <summary>
        /// 等待指定Group中的Thread退出
        /// </summary>
        /// <param name="groupId"></param>
        public static void WaitForExit(string groupId)
        {
            List<AveThreadWrapper> filtererThreads = GetThreadByGroup(groupId, false);
            foreach (AveThreadWrapper threadWrapper in filtererThreads)
            {
                threadWrapper.Stop(int.MaxValue, "WaitForExist", false);
            }
        }

        public static bool IsThreadRunning
        {
            get
            {
                AveThreadWrapper threadWrapper = CurrentThreadWrapper;

                if (threadWrapper != null)
                {
                    return threadWrapper.KeepRunning;
                }

                return true;//default keep running for unregistered thread 
            }
        }

        public static AveThreadWrapper CurrentThreadWrapper
        {
            get
            {
                AveThreadWrapper threadWrapper = null;

                lock (threads)
                {
                    int currentManagedThreadId = Thread.CurrentThread.ManagedThreadId;
                    if (threads.ContainsKey(currentManagedThreadId))
                    {
                        threadWrapper = threads[currentManagedThreadId];
                    }
                }

                return threadWrapper;
            }
        }

        public static List<AveThreadWrapper> GetThreadByGroup(string groupId, bool removedFromHost = true)
        {
            List<AveThreadWrapper> mFilteredThreads = new List<AveThreadWrapper>();

            lock (threads)
            {
                if (string.IsNullOrEmpty(groupId))
                {
                    mFilteredThreads.AddRange(threads.Values);
                    if (removedFromHost)
                    {
                        threads.Clear();
                    }
                }
                else
                {
                    foreach (AveThreadWrapper thread in threads.Values)
                    {
                        if (thread.GroupId.Equals(groupId, StringComparison.OrdinalIgnoreCase))
                        {
                            mFilteredThreads.Add(thread);
                        }
                    }

                    if (removedFromHost)
                    {
                        foreach (AveThreadWrapper thread in mFilteredThreads)
                        {
                            threads.Remove(thread.ManagedThreadId);
                        }
                    }
                }
            }

            if (mFilteredThreads.Contains(monitorThreadWrapper))
            {
                mFilteredThreads.Remove(monitorThreadWrapper);
                mFilteredThreads.Add(monitorThreadWrapper);
            }

            return mFilteredThreads;
        }

        public static void AddThreadWrapper(AveThreadWrapper threadWrapper)
        {
            lock (threads)
            {
                threads[threadWrapper.ManagedThreadId] = threadWrapper;
            }
        }

        private static void MonitorThread()
        {
            AveThreadWrapper currentThreadWrapper = AveThreadUtility.CurrentThreadWrapper;
            while (currentThreadWrapper.KeepRunning)
            {
                Thread.Sleep(5000);

                #region -- Dump Info --
                try
                {
                    if (File.Exists(debugFileName))
                    {
                        lock (threads)
                        {
                            foreach (AveThreadWrapper threadWrapper in threads.Values)
                            {
                                if (threadWrapper.ManagedThreadId != monitorThreadWrapper.ManagedThreadId)
                                {
                                    threadWrapper.DumpThreadInfo();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("Dump Thread Info Failed:{0}", ex.ToString());
                }
                #endregion

                #region -- Remove Dead Threads --
                try
                {
                    lock (threads)
                    {
                        List<int> tempKeys = new List<int>();
                        foreach (var keyValue in threads)
                        {
                            if (keyValue.Value.Status == 2)
                            {
                                //已经结束了
                                tempKeys.Add(keyValue.Key);
                            }
                            else if (keyValue.Value.Status == 1 && (!keyValue.Value.IsAlive))
                            {
                                //标识是running，但是被abort等情况需要考虑。
                                tempKeys.Add(keyValue.Key);
                            }
                        }

                        foreach (var tempKey in tempKeys)
                        {
                            threads.Remove(tempKey);
                        }
                        tempKeys.Clear();
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("Remove dead threads failed:{0}", ex.ToString());
                }
                #endregion
            }
        }

        public static bool IsCustomThread()
        {
            var currentPrincipal = TenantThreadLocalValue.CurrentPrincipal as CustomThreadPrincipal;
            if (currentPrincipal != null && currentPrincipal.Identity != null)
            {
                if (string.Equals("DocAve Custom Thread Identity", currentPrincipal.Identity.Name, StringComparison.OrdinalIgnoreCase)
                    && currentPrincipal.IsInRole("DocAve Custom Thread Identity"))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
