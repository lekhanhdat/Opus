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
namespace System.Collections.Generic
{
    using System.Threading;
    public class ReadLockScope : IDisposable
    {
        private ReaderWriterLockSlim mLocker;
        public ReadLockScope(ReaderWriterLockSlim locker)
        {
            mLocker = locker;
            while (!mLocker.TryEnterReadLock(new TimeSpan(0, 0, 15)))
            {
                //todo log here
            }
        }
        public void Dispose()
        {
            mLocker.ExitReadLock();
            mLocker = null;
        }
    }

    public class WriteLockScope : IDisposable
    {
        private ReaderWriterLockSlim mLocker;
        public WriteLockScope(ReaderWriterLockSlim locker)
        {
            mLocker = locker;
            while (!mLocker.TryEnterWriteLock(new TimeSpan(0, 0, 15)))
            {
                //todo log here
            }
        }
        public void Dispose()
        {
            mLocker.ExitWriteLock();
        }
    }

    public abstract class ThreadLocker:IDisposable
    {
        ReaderWriterLockSlim mLock = new ReaderWriterLockSlim();
        public ThreadLocker()
        {
            Locker = new object();
        }

        protected object Locker { get; }

        [Obsolete]
        protected void LockExecution(Action action)
        {
            lock (Locker)
            {
                action();
            }
        }

        [Obsolete]
        protected T LockExecution<T>(Func<T> action)
        {
            lock (Locker)
            {
                return action();
            }
        }

        protected void AcquireReadLock(Action action)
        {
            using (CreateReadLockScope())
            {
                action();
            }
        }

        protected void AcquireWriteLock(Action action)
        {
            using (CreateWriteLockScope())
            {
                action();
            }
        }

        protected T AcquireReadLock<T>(Func<T> action)
        {
            using (CreateReadLockScope())
            {
                return action();
            }
        }

        protected T AcquireWriteLock<T>(Func<T> action)
        {
            using (CreateWriteLockScope())
            {
                return action();
            }
        }

        protected IDisposable CreateReadLockScope()
        {
            return new ReadLockScope(mLock);
        }

        protected IDisposable CreateWriteLockScope()
        {
            return new WriteLockScope(mLock);
        }

        public void Dispose()
        {
            mLock.Dispose();
            mLock = null;
        }
    }
}
