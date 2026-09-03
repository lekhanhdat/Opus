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

namespace AvePoint.Media.Storage.TSM
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using AvePoint.GCommon; 
    #endregion

    class TSMSystemSafeguard
    {
        private static List<TSMSystem> WaitingSystemList { get; set; }
        private static Dictionary<string, Thread> threadPool;
        private static readonly string threadName = "SystemSafeguardThread";
        private static AveLogger logger = new AveLogger(typeof(TSMSystemSafeguard));
        private static object objLock = new object();

        public static bool IsAlive()
        {
            if (threadPool != null && threadPool.ContainsKey(threadName) && threadPool[threadName].IsAlive)
            {
                logger.Debug("safeguard thread is alive");
                return true;
            }
            else
            {
                logger.Debug("safeguard thread is dead");
                return false;
            }
        }

        public static void AddTSMSystem(TSMSystem system)
        {
            lock (objLock)
            {
                if (WaitingSystemList == null)
                {
                    WaitingSystemList = new List<TSMSystem>();
                    WaitingSystemList.Add(system);
                }
                else
                {
                    if (!WaitingSystemList.Contains(system))
                    {
                        WaitingSystemList.Add(system);
                    }
                }
                logger.Debug("add dead system:" + system.ToString());
            }
        }

        public static void RemoveTSMSystem(TSMSystem system)
        {
            lock (objLock)
            {
                if (WaitingSystemList != null && WaitingSystemList.Contains(system))
                {
                    WaitingSystemList.Remove(system);
                }
                logger.Debug("remove waiting system:" + system.ToString());
            }
        }

        public static void WaitingUserAndKilledSystem()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(1000 * 3600);
                    lock (TSMSystem.countLocker)
                    {
                        if (WaitingSystemList != null && WaitingSystemList.Count > 0)
                        {
                            foreach (TSMSystem sys in WaitingSystemList)
                            {
                                logger.Info("clean up tsm system:" + sys.MapKey);
                                sys.KilledAllSession();
                                TSMSystem.mapping.Remove(sys.MapKey);
                            }
                            WaitingSystemList.Clear();
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("safeguard system failed: {0}.", ex);
                }
            }
        }

        public static void StartTSMSystemSafeguard()
        {
            if (threadPool == null)
            {
                threadPool = new System.Collections.Generic.Dictionary<string, Thread>();
            }
            if (!threadPool.ContainsKey(threadName) || !threadPool[threadName].IsAlive)
            {
                Thread thread = new Thread(new ThreadStart(WaitingUserAndKilledSystem));
                thread.Name = threadName;
                threadPool[threadName] = thread;
                thread.IsBackground = true;
                thread.Start();
                logger.Info("start tsm system clean up thread:" + threadName);
            }
        }
    }
}
