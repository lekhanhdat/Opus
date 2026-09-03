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



namespace AvePoint.Common
{
    #region using directives
    using System;
    using System.Threading;
    using AvePoint.GCommon;
    #endregion

    /// <summary>
    /// Just a method signature which may be used as a process started handler
    /// </summary>
    public delegate void SingletonProcessDelegate();

    /// <summary>
    /// A function class that can be use to make the process run once
    /// </summary>
    public class SingletonProcess
    {
        static AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Start a special method that only can be run once
        /// </summary>
        /// <typeparam name="T">A main class object which implements interface ISingletonProcess</typeparam>
        /// <param name="obj">an instance of the main class</param>
        /// <param name="mutexName">global indicator of a process</param>
        public static void StartProcess<T>(T obj, String mutexName)
            where T : class, ISingletonProcess
        {
            if (obj != null)
            {
                StartProcess
                    (mutexName,
                    new SingletonProcessDelegate(obj.Start),
                    new SingletonProcessDelegate(obj.ProcessHasStartedHandler));
            }
        }

        /// <summary>
        /// Start a special method that only can be run once
        /// </summary>
        /// <param name="mutexName">global indicator of a process</param>
        /// <param name="processHandler">a delegate method that will be run once</param>
        public static void StartProcess(String mutexName, SingletonProcessDelegate processHandler)
        {
            StartProcess(mutexName, processHandler, null);
        }

        /// <summary>
        /// Start a special method that only can be run once
        /// </summary>
        /// <param name="mutexName">global indicator of a process</param>
        /// <param name="processHandler">a delegate method that will be run once</param>
        /// <param name="processHasStartedHandler">a delegate method that will runs at the condition that the specific process has running</param>
        public static void StartProcess(String mutexName, SingletonProcessDelegate processHandler, SingletonProcessDelegate processHasStartedHandler)
        {
            StartProcess(mutexName, processHandler, processHasStartedHandler, null);
        }

        /// <summary>
        /// Start a special method that only can be run once
        /// </summary>
        /// <param name="mutexName">global indicator of a process</param>
        /// <param name="processHandler">a delegate method that will be run once</param>
        /// <param name="processHasStartedHandler">a delegate method that will runs at the condition that the specific process has running</param>
        /// <param name="args">arguments that will be pass into the process handler method</param>
        public static void StartProcess
            (String mutexName,
            SingletonProcessDelegate processHandler,
            SingletonProcessDelegate processHasStartedHandler,
            params Object[] args)
        {
            Mutex mutex = null;
            try
            {
                Boolean mutexWasCreated;
                mutex = new Mutex(2 > 1, mutexName, out mutexWasCreated);
                if (mutexWasCreated)
                {
                    if (processHandler != null)
                        processHandler.DynamicInvoke(args);
                }
                else
                {
                    if (processHasStartedHandler != null)
                        processHasStartedHandler.DynamicInvoke(null);
                }
            }
            finally
            {
                try
                {
                    if (mutex != null)
                    {
                        mutex.ReleaseMutex();
                        mutex.Close();
                    }
                }
                catch (Exception e) { logger.Warn(e.ToString()); }
            }
        }
    }
}
