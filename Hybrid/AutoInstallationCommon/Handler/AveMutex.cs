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

namespace AutoInstallationCommon.Utility.Handler
{
    public class AveMutex
    {
        private static readonly Dictionary<string, Mutex> mutexsDictionary =
            new Dictionary<string, Mutex>(StringComparer.OrdinalIgnoreCase);

        public AveMutex(string name)
        {
            mutexName = name;
            escapeName = true;
        }

        public AveMutex(string name, bool escapeName)
        {
            mutexName = name;
            this.escapeName = escapeName;
        }

        public void WaitLocked()
        {
            if (mutex == null)
            {
                var mutexMd5Name = mutexName;
                try
                {
                    mutex = new Mutex(false, mutexMd5Name);
                    //this.mutex = Mutex.OpenExisting(mutexMd5Name);
                }
                catch (WaitHandleCannotBeOpenedException e)
                {
                    mutex = new Mutex(false, mutexMd5Name);
                }
            }

            try
            {
                mutex.WaitOne();
            }
            catch (AbandonedMutexException ame)
            {
                //The wait completed because a thread exited without releasing a mutex.
            }
        }

        public void WaitLocked(int milliSecondsTimeOut)
        {
            if (mutex == null)
            {
                var mutexMd5Name = mutexName;
                try
                {
                    mutex = Mutex.OpenExisting(mutexMd5Name);
                }
                catch (WaitHandleCannotBeOpenedException e)
                {
                    mutex = new Mutex(false, mutexMd5Name);
                }
            }

            try
            {
                mutex.WaitOne(milliSecondsTimeOut);
            }
            catch (AbandonedMutexException ame)
            {
                //The wait completed because a thread exited without releasing a mutex.
            }
        }


        /// <summary>
        ///     释放该Mutex
        /// </summary>
        public void ReleaseLock()
        {
            if (mutex != null) mutex.ReleaseMutex();
        }

        #region IDisposable Members

        /// <summary>
        ///     Release all resources
        /// </summary>
        public void Dispose()
        {
            if (mutex != null)
            {
                mutex.Close();
                mutex = null;
            }
        }

        #endregion

        #region -- Private Properties --

        /// <summary>
        ///     由于该AveMutex可能会被其他还原逻辑使用，所以会传递具体还原对象的URL作为mutex的名字，在使用的时候，需要将URL转船为MD5的GUID,因为URL里面包含特殊字符不能作为mutex的名字
        /// </summary>
        private readonly string mutexName;

        private bool escapeName;
        private Mutex mutex;

        #endregion

        #region -- Static Methods --

        /// <summary>
        ///     判断该Mutex是否存在，如果不存在，则创建，如果存在，则直接返回
        /// </summary>
        /// <param name="mutexName"></param>
        /// <returns></returns>
        public static bool CheckMutex(string mutexName)
        {
            if (!IsMutexExists(mutexName))
            {
                if (CreateMutext(mutexName))
                    return true;
                return false;
            }

            //logger.Info("Process with mutex already exist. " + mutexName);
            return false;
        }

        /// <summary>
        ///     结束的时候关闭对应的Mutex
        /// </summary>
        public static void Close()
        {
            lock (mutexsDictionary)
            {
                foreach (var mutex in mutexsDictionary.Values)
                    try
                    {
                        mutex.ReleaseMutex();
                        mutex.Close();
                    }
                    catch (Exception e)
                    {
                        //logger.Warn(e.ToString());
                    }

                mutexsDictionary.Clear();
            }
        }

        /// <summary>
        ///     判断Mutex是否存在
        /// </summary>
        /// <param name="mutexName"></param>
        /// <returns></returns>
        public static bool IsMutexExists(string mutexName)
        {
            Mutex mut = null;
            try
            {
                mut = Mutex.OpenExisting(mutexName);
                return true; //存在这个name
            }
            catch (WaitHandleCannotBeOpenedException e)
            {
                return false; //不存在这个name
            }
            catch (UnauthorizedAccessException e)
            {
                return true; //存在这个name，但是权限不够
            }
            finally
            {
                if (mut != null) mut.Close();
            }
        }

        private static bool CreateMutext(string mutexName)
        {
            try
            {
                bool mutexWasCreated;
                var mutex = new Mutex(true, mutexName, out mutexWasCreated);
                if (!mutexWasCreated) return false;
                lock (mutexsDictionary)
                {
                    mutexsDictionary[mutexName] = mutex;
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static Mutex CreateNewMutext(string mutexName)
        {
            try
            {
                bool mutexWasCreated;
                var mutex = new Mutex(true, mutexName, out mutexWasCreated);

                return mutex;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        #endregion
    }
}