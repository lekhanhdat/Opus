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




namespace AvePoint.GCommon.Utility
{
    #region using directives
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Threading;
    #endregion

    /// <summary>
    /// 封装了ThreadPool的一些方法，用来记录所有的Thread。
    /// 在存在问题的时候，可以打印Thread的运行堆栈，不过运行的时候，需要继承AveThreadPoolItemBase
    /// </summary>
    public class AveThreadPoolRunner
    {
        static AveLogger logger = AveLogger.GetInstance(typeof(AveThreadPoolRunner));
        static string debugFileName = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "DumpThreadPool.AvePoint");

        static Hashtable poolItems = new Hashtable();
        static AveThreadWrapper dumpWorkingThread;

        static AveThreadPoolRunner()
        {
            dumpWorkingThread = AveThreadUtility.StartThread(DumpWorkingThreads, "DumpWorkingThreads", "ThreadPool");

            ThreadPool.SetMaxThreads(500, 1000);
        }

        public static void Init(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                debugFileName = fileName;
            }
        }

        public static void RunThread(AveThreadPoolItemBase threadItem)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(InternalRun), (object)threadItem);
        }

        private static void InternalRun(object tItem)
        {
            AveThreadPoolItemBase threadItem = (AveThreadPoolItemBase)tItem;
            threadItem.StartTime = DateTime.Now;
            lock (poolItems)
            {
                poolItems.Add(threadItem.UniqueId, threadItem);
            }
            try
            {
                threadItem.Run();
            }
            finally
            {
                lock (poolItems)
                {
                    poolItems.Remove(threadItem.UniqueId);
                }
            }
        }

        private static void DumpWorkingThreads()
        {
            try
            {
                AveThreadWrapper currentThreadWrapper = AveThreadUtility.CurrentThreadWrapper;
                while (currentThreadWrapper.KeepRunning)
                {
                    Thread.Sleep(1000);

                    if (File.Exists(debugFileName))
                    {
                        int workerThreads;
                        int completionPortThreads;
                        ThreadPool.GetMaxThreads(out workerThreads, out completionPortThreads);
                        lock (poolItems)
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append("-----------Dump Time:" + DateTime.Now.ToString() + "-------------\n");
                            sb.Append("workerThreads = " + workerThreads.ToString() + " completionPortThreads = " + completionPortThreads.ToString() + "\n");
                            sb.Append("-----------------------------------------------------------------\n");
                            if (poolItems.Keys.Count == 0)
                            {
                                sb.Append("There is no thread pool item running now.\n\n");
                            }
                            foreach (Guid uniqueId in poolItems.Keys)
                            {
                                AveThreadPoolItemBase threadBase = (AveThreadPoolItemBase)poolItems[uniqueId];
                                sb.Append("UniqueId: " + threadBase.UniqueId + "\n");
                                sb.Append("Name: " + threadBase.Name + "\n");
                                sb.Append("StartTime: " + threadBase.StartTime + "\n\n");
                            }
                            logger.Info(sb.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());//TODO
            }
        }
    }
}
