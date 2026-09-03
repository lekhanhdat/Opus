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
using System.Collections.Concurrent;
using System.Threading;

namespace AvePoint.RA.Common.Lock
{
    public class SqliteLockManager
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new(StringComparer.OrdinalIgnoreCase);

        private static readonly TimeSpan _lockTimeout = TimeSpan.FromMinutes(10);

        public static IDisposable AcquireLock(string lockKey)
        {
            if(string.IsNullOrEmpty(lockKey))
            {
                throw new ArgumentException($"The lock key has no value");
            }
            
            var semaphore = _semaphores.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

            if (!semaphore.Wait(_lockTimeout))
            {
                throw new TimeoutException($"Timeout acquiring lock for key [{lockKey}]");
            }

            return new SemaphoreReleaser(semaphore);
        }

        private sealed class SemaphoreReleaser : IDisposable
        {
            private SemaphoreSlim _semaphore;

            public SemaphoreReleaser(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }

            public void Dispose()
            {
                _semaphore?.Release();
                _semaphore = null;
            }
        }
    }
}
