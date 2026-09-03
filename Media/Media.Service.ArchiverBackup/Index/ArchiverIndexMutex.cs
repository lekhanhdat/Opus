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
using System.Collections.Concurrent;

namespace AvePoint.Media.Service.ArchiverBackup
{
    public sealed class ArchiverIndexMutex
    {
        private static ConcurrentDictionary<Guid, SemaphoreSlim> mutexsDictionary = new ConcurrentDictionary<Guid, SemaphoreSlim>();

        #region -- Private Properties --
        /// <summary>
        /// 由于该AveMutex可能会被其他还原逻辑使用，所以会传递具体还原对象的URL作为mutex的名字，在使用的时候，需要将URL转船为MD5的GUID,因为URL里面包含特殊字符不能作为mutex的名字
        /// </summary>
        String mutexName;
        SemaphoreSlim? mutex;

        #endregion

        public ArchiverIndexMutex(string name)
        {
            this.mutexName = name;
        }

        /// <summary>
        /// 获取Mutex，并且使用该Mutex。
        /// </summary>
        public async Task<bool> WaitAsync(int milliSecondsTimeOut)
        {
            SemaphoreSlim? semaphoreSlim = this.mutex;
            if (this.mutex == null)
            {
                var key = HashCodeHelper.StringHash(this.mutexName);
                lock (mutexsDictionary)
                {
                    semaphoreSlim = this.mutex;
                    if (this.mutex == null)
                    {
                        if (!mutexsDictionary.TryGetValue(key, out semaphoreSlim))
                        {
                            semaphoreSlim = new SemaphoreSlim(1);
                            mutexsDictionary[key] = semaphoreSlim;
                        }
                    }
                }
            }

            if (milliSecondsTimeOut <= 0)
            {
                milliSecondsTimeOut = 30 * 60 * 1000;
            }

            bool gotLock = false;
            if(semaphoreSlim != null)
            {
                gotLock = await semaphoreSlim.WaitAsync(milliSecondsTimeOut);
                if (gotLock)
                {
                    this.mutex = semaphoreSlim;
                }
            }
            
            return gotLock;
        }

        /// <summary>
        /// 释放该Mutex
        /// </summary>
        public void Release()
        {
            if (this.mutex != null)
            {
                lock (mutexsDictionary)
                {
                    if (this.mutex != null)
                    {
                        this.mutex.Release();
                        this.mutex = null;
                    }
                }
                
            }
        }

    }
}
