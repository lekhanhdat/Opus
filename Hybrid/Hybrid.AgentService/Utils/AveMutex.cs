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

namespace AvePoint.Hybrid.AgentService
{
    using AvePoint.RA.CommonUtil;
    using AvePoint.Hybrid.Utility.Hash;
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Threading;
    #endregion

    /// <summary>
    /// 提供两类方法：
    /// 1. 静态的方法，可以用于检查Mutex是否可用，如果可用，直接创建，如果不可用，则不创建，一般用于同一个Job只能运行一个EXE。
    /// 2. 非静态的方法，一般用于还原某个SharePoint时，锁住当前对象的还原。
    /// </summary>
    public sealed class AveMutex : IDisposable
    {
        private static AvePoint.GCommon.AveLogger logger = AvePoint.GCommon.AveLogger.GetInstance(typeof(AveMutex));

        private static Dictionary<String, Mutex> mutexsDictionary = new Dictionary<String, Mutex>(StringComparer.OrdinalIgnoreCase);

        #region -- Private Properties --
        /// <summary>
        /// 由于该AveMutex可能会被其他还原逻辑使用，所以会传递具体还原对象的URL作为mutex的名字，在使用的时候，需要将URL转船为MD5的GUID,因为URL里面包含特殊字符不能作为mutex的名字
        /// </summary>
        String mutexName;
        bool escapeName;
        Mutex mutex;

        #endregion

        public AveMutex(string name)
        {
            this.mutexName = name;
            this.escapeName = true;
        }

        public AveMutex(string name, bool escapeName)
        {
            this.mutexName = name;
            this.escapeName = escapeName;
        }

        /// <summary>
        /// 获取Mutex，并且使用该Mutex。
        /// </summary>
        public void WaitLocked()
        {
            if (this.mutex == null)
            {
                string mutexMd5Name = mutexName;
                if (escapeName)
                {
                    mutexMd5Name = HashCodeHelper.StringHash(mutexName).ToString();
                }
                try
                {
                    this.mutex = new Mutex(false, mutexMd5Name);
                    //this.mutex = Mutex.OpenExisting(mutexMd5Name);
                }
                catch (WaitHandleCannotBeOpenedException e)
                {
                    this.mutex = new Mutex(false, mutexMd5Name);
                }
            }
            try
            {
                this.mutex.WaitOne();
            }
            catch (System.Threading.AbandonedMutexException ame)
            {
                //The wait completed because a thread exited without releasing a mutex.
            }
        }

        /// <summary>
        /// 获取Mutex，并且使用该Mutex。
        /// </summary>
        public void WaitLocked(int milliSecondsTimeOut)
        {
            if (this.mutex == null)
            {
                string mutexMd5Name = mutexName;
                if (escapeName)
                {
                    mutexMd5Name = HashCodeHelper.StringHash(mutexName).ToString();
                }
                try
                {
                    this.mutex = Mutex.OpenExisting(mutexMd5Name);
                }
                catch (WaitHandleCannotBeOpenedException e)
                {
                    this.mutex = new Mutex(false, mutexMd5Name);
                }
            }
            try
            {
                this.mutex.WaitOne(milliSecondsTimeOut);
            }
            catch (System.Threading.AbandonedMutexException ame)
            {
                //The wait completed because a thread exited without releasing a mutex.
            }
        }

        /// <summary>
        /// 释放该Mutex
        /// </summary>
        public void ReleaseLock()
        {
            if (this.mutex != null)
            {
                //it will throw System.ApplicationException if the calling thread does not own the mutex.
                this.mutex.ReleaseMutex();
            }
        }

        #region -- Static Methods --
        /// <summary>
        /// 判断该Mutex是否存在，如果不存在，则创建，如果存在，则直接返回
        /// </summary>
        /// <param name="mutexName"></param>
        /// <returns></returns>
        public static bool CheckMutex(string mutexName)
        {
            if (!IsMutexExists(mutexName))
            {
                if (CreateMutext(mutexName))
                {
                    logger.Info("Mutex was created: " + mutexName);
                    return true;
                }
                else
                {
                    logger.Warn("Can't create mutex name: " + mutexName);
                    return false;
                }
            }
            else
            {
                logger.Info("Process with mutex already exist. " + mutexName);
                return false;
            }
        }

        /// <summary>
        /// 结束的时候关闭对应的Mutex
        /// </summary>
        public static void Close()
        {
            lock (mutexsDictionary)
            {
                foreach (var mutex in mutexsDictionary.Values)
                {
                    try
                    {
                        mutex.ReleaseMutex();
                        mutex.Close();
                    }
                    catch (Exception e)
                    {
                        logger.Warn(e.ToString());
                    }
                }

                mutexsDictionary.Clear();
            }
        }

        /// <summary>
        /// 判断Mutex是否存在
        /// </summary>
        /// <param name="mutexName"></param>
        /// <returns></returns>
        public static bool IsMutexExists(string mutexName)
        {
            Mutex mut = null;
            try
            {
                mut = Mutex.OpenExisting(mutexName);
                return true;//存在这个name
            }
            catch (WaitHandleCannotBeOpenedException e)
            {
                return false;//不存在这个name
            }
            catch (UnauthorizedAccessException e)
            {
                return true;//存在这个name，但是权限不够
            }
            finally
            {
                if (mut != null)
                {
                    mut.Close();
                }
            }
        }

        private static bool CreateMutext(string mutexName)
        {
            try
            {
                bool mutexWasCreated;
                Mutex mutex = new Mutex(true, mutexName, out mutexWasCreated);
                if (!mutexWasCreated)
                {
                    return false;
                }
                lock (mutexsDictionary)
                {
                    mutexsDictionary[mutexName] = mutex;
                }
                return true;
            }
            catch (Exception e)
            {
                logger.Error("Create Mutex:{0} failed:{1}", mutexName, e.ToString());
                return false;
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Release all resources
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
    }
}
