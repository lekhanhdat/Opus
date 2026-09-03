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

namespace AvePoint.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Security.Permissions;
    using System.Runtime.Serialization;

    public abstract class AveCache<T, TV> : IDisposable
    {
        // Fields
        internal bool Enabled;
        internal AveReaderWriterLock m_Lock;

        // Methods
        protected AveCache()
            : this(null)
        {
        }
        protected AveCache(string cacheName)
        {
            string name = cacheName;
            if (string.IsNullOrEmpty(name))
            {
                name = base.GetType().Name;
            }
            this.m_Lock = new AveReaderWriterLock(name);
        }
        protected abstract TV GetValue(T key);
        internal abstract bool Invalidate(T key);
        public bool Remove(T key)
        {
            if (!this.Enabled)
            {
                return false;
            }
            using (this.m_Lock.AcquireWriterLock())
            {
                return this.RemoveByKey(key);
            }
        }
        protected abstract bool RemoveByKey(T key);
        protected abstract void SetValue(T key, TV value);

        public abstract int Count
        {
            get;
        }

        // Properties
        public TV this[T key]
        {
            get
            {
                if (!this.Enabled)
                {
                    return default(TV);
                }
                return this.GetValue(key);
            }
            set
            {
                if (this.Enabled)
                {
                    this.SetValue(key, value);
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (m_Lock != null)
            {
                m_Lock.Dispose();
                m_Lock = null;
            }
        }

        #endregion
    }

    public class AveVolatileCache<T, TV> : AveCache<T, TV>
    {
        // Fields
        private Dictionary<T, TV> m_Cache;
        IEqualityComparer<T> m_Comparer = null;
        // Methods
        public AveVolatileCache()
            : this(null)
        {
        }

        public AveVolatileCache(string cacheName)
            : base(cacheName)
        {
            this.m_Cache = new Dictionary<T, TV>();
            base.Enabled = true;
        }

        public AveVolatileCache(string cacheName, IEqualityComparer<T> comparer)
            : base(cacheName)
        {
            this.m_Cache = new Dictionary<T, TV>(comparer);
            m_Comparer = comparer;
            base.Enabled = true;
        }

        public void Clear()
        {
            if (m_Comparer == null)
            {
                m_Cache = new Dictionary<T, TV>();
            }
            else
            {
                m_Cache = new Dictionary<T, TV>(m_Comparer);
            }
        }

        public bool ContainsKey(T key)
        {
            return this.m_Cache.ContainsKey(key);
        }

        public bool FindAndAddValue(T key, TV value)
        {
            using (base.m_Lock.AcquireWriterLock())
            {
                if (this.ContainsKey(key))
                {
                    return false;
                }
                this.m_Cache[key] = value;
                return true;
            }
        }

        internal TV[] GetAllValuesAndClear()
        {
            TV[] localArray;
            using (base.m_Lock.AcquireWriterLock())
            {
                localArray = new TV[this.m_Cache.Count];
                this.m_Cache.Values.CopyTo(localArray, 0);
                this.Clear();
            }
            return localArray;
        }

        protected override TV GetValue(T key)
        {
            TV local;
            bool flag;
            using (base.m_Lock.AcquireReaderLock())
            {
                flag = this.m_Cache.TryGetValue(key, out local);
            }
            if (flag)
            {
                return local;
            }
            return default(TV);
        }

        internal override bool Invalidate(T key)
        {
            return base.Remove(key);
        }

        internal bool Remove(T key, out TV value)
        {
            value = default(TV);
            if (!base.Enabled)
            {
                return false;
            }
            bool flag = false;
            using (base.m_Lock.AcquireWriterLock())
            {
                flag = this.m_Cache.TryGetValue(key, out value);
                if (flag)
                {
                    return this.RemoveByKey(key);
                }
                value = default(TV);
            }
            return flag;
        }

        protected override bool RemoveByKey(T key)
        {
            return this.m_Cache.Remove(key);
        }

        protected override void SetValue(T key, TV value)
        {
            using (base.m_Lock.AcquireWriterLock())
            {
                this.m_Cache[key] = value;
            }
        }

        public bool TryGetValue(T key, out TV retval)
        {
            using (base.m_Lock.AcquireReaderLock())
            {
                return this.m_Cache.TryGetValue(key, out retval);
            }
        }

        public override int Count
        {
            get
            {
                using (base.m_Lock.AcquireReaderLock())
                {
                    return this.m_Cache.Count;
                }
            }
        }

        // Properties
        public T[] Keys
        {
            get
            {
                T[] localArray;
                using (base.m_Lock.AcquireWriterLock())
                {
                    localArray = new T[this.m_Cache.Count];
                    this.m_Cache.Keys.CopyTo(localArray, 0);
                }
                return localArray;
            }
        }

        public TV[] Values
        {
            get
            {
                TV[] localArray;
                using (base.m_Lock.AcquireWriterLock())
                {
                    localArray = new TV[this.m_Cache.Count];
                    this.m_Cache.Values.CopyTo(localArray, 0);
                }
                return localArray;
            }
        }
        
    }

    internal interface IAveReaderWriterLock : IDisposable
    {
        // Methods
        IDisposable AcquireReaderLock();
        IDisposable AcquireUpgradableReaderLock();
        IDisposable AcquireWriterLock();

        // Properties
        bool IsReaderLockHeld { get; }
        bool IsWriterLockHeld { get; }
    }

    internal class AveReaderWriterLock : IAveReaderWriterLock, IDisposable
    {
        private AveReaderWriterLockSlim m_Lock;
        private string m_LockName;
        private const int DefaultTimeOut = 300000;

        private class AveReaderWriterLockScope : IDisposable
        {
            // Fields
            private bool m_Disposed;
            private bool m_IsReaderLock;
            private bool m_IsUpgradable;
            private AveReaderWriterLock m_Lock;

            // Methods
            public AveReaderWriterLockScope(AveReaderWriterLock myLock, bool isReaderLock, bool isUpgradable)
            {
                this.m_IsReaderLock = isReaderLock;
                this.m_IsUpgradable = isUpgradable;
                this.m_Lock = myLock;

            }
            public void Dispose()
            {
                if (!this.m_Disposed)
                {
                    if (this.m_IsReaderLock)
                    {
                        if (this.m_IsUpgradable)
                        {
                            this.m_Lock.m_Lock.ExitUpgradeableReadLock();
                        }
                        else
                        {
                            this.m_Lock.m_Lock.ExitReadLock();
                        }
                    }
                    else
                    {
                        this.m_Lock.m_Lock.ExitWriteLock();
                    }
                    this.m_Disposed = true;
                    GC.SuppressFinalize(this);
                }
            }
        }

        internal AveReaderWriterLock(string lockName)
            : this(lockName, DefaultTimeOut, AveLockRecursionPolicy.NoRecursion)
        {
        }

        internal AveReaderWriterLock(string lockName, int timeout, AveLockRecursionPolicy recursionPolicy)
        {
            this.m_LockName = lockName;
            this.m_Lock = new AveReaderWriterLockSlim(recursionPolicy);
            TimeOut = timeout;
        }

        public static IAveReaderWriterLock Create(string lockName)
        {
            return new AveReaderWriterLock(lockName);
        }

        public static IAveReaderWriterLock Create(string lockName, int timeout, AveLockRecursionPolicy recursionPolicy)
        {
            return new AveReaderWriterLock(lockName, timeout, recursionPolicy);
        }

        internal IDisposable AcquireLock(bool readerLock, bool upgradable)
        {
            AveReaderWriterLockScope scope = null;
            bool flag = false;
            try
            {
                if (readerLock)
                {
                    if (upgradable)
                    {
                        flag = this.m_Lock.TryEnterUpgradeableReadLock(this.TimeOut);
                    }
                    else
                    {
                        flag = this.m_Lock.TryEnterReadLock(this.TimeOut);
                    }
                }
                else
                {
                    flag = this.m_Lock.TryEnterWriteLock(this.TimeOut);
                }
                if (!flag)
                {
                    //throw new exception
                }
            }
            finally
            {
                if (flag)
                {
                    scope = new AveReaderWriterLockScope(this, readerLock, upgradable);
                }
            }
            return scope;
        }

        public IDisposable AcquireReaderLock()
        {
            return this.AcquireLock(true, false);
        }

        public IDisposable AcquireUpgradableReaderLock()
        {
            return this.AcquireLock(true, true);
        }

        public IDisposable AcquireWriterLock()
        {
            return this.AcquireLock(false, false);
        }

        public void Dispose()
        {
            if (this.m_Lock != null)
            {
                this.m_Lock.Dispose();
                this.m_Lock = null;
            }
        }

        internal int TimeOut { get; set; }

        public bool IsReaderLockHeld
        {
            get
            {
                return this.m_Lock.IsReadLockHeld;
            }
        }

        public bool IsWriterLockHeld
        {
            get
            {
                return this.m_Lock.IsWriteLockHeld;
            }
        }
    }

    public enum AveLockRecursionPolicy
    {
        NoRecursion,
        SupportsRecursion
    }

    [Serializable]
    public class AveLockRecursionException : Exception
    {
        // Methods
        public AveLockRecursionException()
        {
        }

        public AveLockRecursionException(string message)
            : base(message)
        {
        }

        protected AveLockRecursionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public AveLockRecursionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class AveReaderWriterLockSlim : IDisposable
    {
        // Fields
        private bool fDisposed;
        private bool fIsReentrant;
        private bool fNoWaiters;
        private bool fUpgradeThreadHoldingRead;
        private const int hashTableSize = 0xff;
        private const int LockSleep0Count = 5;
        private const int LockSpinCount = 10;
        private const int LockSpinCycles = 20;
        private const uint MAX_READER = 0xffffffe;
        private const int MaxSpinCount = 20;
        private int myLock;
        private uint numReadWaiters;
        private uint numUpgradeWaiters;
        private uint numWriteUpgradeWaiters;
        private uint numWriteWaiters;
        private uint owners;
        private const uint READER_MASK = 0xfffffff;
        private EventWaitHandle readEvent;
        private AveReaderWriterCount[] rwc;
        private EventWaitHandle upgradeEvent;
        private int upgradeLockOwnerId;
        private const uint WAITING_UPGRADER = 0x20000000;
        private const uint WAITING_WRITERS = 0x40000000;
        private EventWaitHandle waitUpgradeEvent;
        private EventWaitHandle writeEvent;
        private int writeLockOwnerId;
        private const uint WRITER_HELD = 0x80000000;
        private readonly ManualResetEvent mEvent = new ManualResetEvent(true);

        // Methods
        public AveReaderWriterLockSlim()
            : this(AveLockRecursionPolicy.NoRecursion)
        {
        }

        public AveReaderWriterLockSlim(AveLockRecursionPolicy recursionPolicy)
        {
            if (recursionPolicy == AveLockRecursionPolicy.SupportsRecursion)
            {
                this.fIsReentrant = true;
            }
            this.InitializeThreadCounts();
        }

        private void ClearUpgraderWaiting()
        {
            this.owners &= 0xdfffffff;
        }

        private void ClearWriterAcquired()
        {
            this.owners &= 0x7fffffff;
        }

        private void ClearWritersWaiting()
        {
            this.owners &= 0xbfffffff;
        }

        public void Dispose()
        {
            mEvent.WaitOne();
            lock (mEvent)
            {
                this.Dispose(true);
                mEvent.Close();
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.fDisposed)
                {
                    throw new ObjectDisposedException(null);
                }
                if (((this.WaitingReadCount > 0) || (this.WaitingUpgradeCount > 0)) || (this.WaitingWriteCount > 0))
                {
                    throw new SynchronizationLockException("SynchronizationLockException_IncorrectDispose");
                }
                if ((this.IsReadLockHeld || this.IsUpgradeableReadLockHeld) || this.IsWriteLockHeld)
                {
                    throw new SynchronizationLockException("SynchronizationLockException_IncorrectDispose");
                }
                if (this.writeEvent != null)
                {
                    this.writeEvent.Close();
                    this.writeEvent = null;
                }
                if (this.readEvent != null)
                {
                    this.readEvent.Close();
                    this.readEvent = null;
                }
                if (this.upgradeEvent != null)
                {
                    this.upgradeEvent.Close();
                    this.upgradeEvent = null;
                }
                if (this.waitUpgradeEvent != null)
                {
                    this.waitUpgradeEvent.Close();
                    this.waitUpgradeEvent = null;
                }
                this.fDisposed = true;
            }
        }

        private void EnterMyLock()
        {
            if (Interlocked.CompareExchange(ref this.myLock, 1, 0) != 0)
            {
                this.EnterMyLockSpin();
            }
        }

        private void EnterMyLockSpin()
        {
            int processorCount = Environment.ProcessorCount;
            int num2 = 0;
            while (true)
            {
                if ((num2 < 10) && (processorCount > 1))
                {
                    Thread.SpinWait(20 * (num2 + 1));
                }
                else if (num2 < 15)
                {
                    Thread.Sleep(0);
                }
                else
                {
                    Thread.Sleep(1);
                }
                if ((this.myLock == 0) && (Interlocked.CompareExchange(ref this.myLock, 1, 0) == 0))
                {
                    return;
                }
                num2++;
            }
        }

        public void EnterReadLock()
        {
            mEvent.Reset();
            this.TryEnterReadLock(-1);
        }

        public void EnterUpgradeableReadLock()
        {
            mEvent.Reset();
            this.TryEnterUpgradeableReadLock(-1);
        }

        public void EnterWriteLock()
        {
            mEvent.Reset();
            this.TryEnterWriteLock(-1);
        }

        private void ExitAndWakeUpAppropriateWaiters()
        {
            if (this.fNoWaiters)
            {
                this.ExitMyLock();
            }
            else
            {
                this.ExitAndWakeUpAppropriateWaitersPreferringWriters();
            }
        }

        private void ExitAndWakeUpAppropriateWaitersPreferringWriters()
        {
            bool flag = false;
            bool flag2 = false;
            uint numReaders = this.GetNumReaders();
            if ((this.fIsReentrant && (this.numWriteUpgradeWaiters > 0)) && (this.fUpgradeThreadHoldingRead && (numReaders == 2)))
            {
                this.ExitMyLock();
                this.waitUpgradeEvent.Set();
            }
            else if ((numReaders == 1) && (this.numWriteUpgradeWaiters > 0))
            {
                this.ExitMyLock();
                this.waitUpgradeEvent.Set();
            }
            else if ((numReaders == 0) && (this.numWriteWaiters > 0))
            {
                this.ExitMyLock();
                this.writeEvent.Set();
            }
            else if (numReaders >= 0)
            {
                if ((this.numReadWaiters == 0) && (this.numUpgradeWaiters == 0))
                {
                    this.ExitMyLock();
                }
                else
                {
                    if (this.numReadWaiters != 0)
                    {
                        flag2 = true;
                    }
                    if ((this.numUpgradeWaiters != 0) && (this.upgradeLockOwnerId == -1))
                    {
                        flag = true;
                    }
                    this.ExitMyLock();
                    if (flag2)
                    {
                        this.readEvent.Set();
                    }
                    if (flag)
                    {
                        this.upgradeEvent.Set();
                    }
                }
            }
            else
            {
                this.ExitMyLock();
            }
        }

        private void ExitMyLock()
        {
            this.myLock = 0;
        }

        public void ExitReadLock()
        {
            int managedThreadId = Thread.CurrentThread.ManagedThreadId;
            AveReaderWriterCount threadRWCount = null;
            this.EnterMyLock();
            threadRWCount = this.GetThreadRWCount(managedThreadId, true);
            if (!this.fIsReentrant)
            {
                if (threadRWCount == null)
                {
                    this.ExitMyLock();
                    throw new SynchronizationLockException("SynchronizationLockException_MismatchedRead");
                }
            }
            else
            {
                if ((threadRWCount == null) || (threadRWCount.readercount < 1))
                {
                    this.ExitMyLock();
                    throw new SynchronizationLockException("SynchronizationLockException_MismatchedRead");
                }
                if (threadRWCount.readercount > 1)
                {
                    threadRWCount.readercount--;
                    this.ExitMyLock();
                    return;
                }
                if (managedThreadId == this.upgradeLockOwnerId)
                {
                    this.fUpgradeThreadHoldingRead = false;
                }
            }
            this.owners--;
            threadRWCount.readercount--;
            this.ExitAndWakeUpAppropriateWaiters();

            SetEvent();
        }

        private void SetEvent()
        {
            if (!this.IsReadLockHeld && this.IsWriteLockHeld && this.WaitingReadCount + this.WaitingUpgradeCount + this.WaitingWriteCount == 0)
            {
                mEvent.Set();
            }
        }

        public void ExitUpgradeableReadLock()
        {
            int managedThreadId = Thread.CurrentThread.ManagedThreadId;
            if (!this.fIsReentrant)
            {
                if (managedThreadId != this.upgradeLockOwnerId)
                {
                    throw new SynchronizationLockException("SynchronizationLockException_MismatchedUpgrade");
                }
                this.EnterMyLock();
            }
            else
            {
                this.EnterMyLock();
                AveReaderWriterCount threadRWCount = this.GetThreadRWCount(managedThreadId, true);
                if (threadRWCount == null)
                {
                    this.ExitMyLock();
                    throw new SynchronizationLockException("SynchronizationLockException_MismatchedUpgrade");
                }
                AveRecursiveCounts rc = threadRWCount.rc;
                if (rc.upgradecount < 1)
                {
                    this.ExitMyLock();
                    throw new SynchronizationLockException("SynchronizationLockException_MismatchedUpgrade");
                }
                rc.upgradecount--;
                if (rc.upgradecount > 0)
                {
                    this.ExitMyLock();
                    return;
                }
                this.fUpgradeThreadHoldingRead = false;
            }
            this.owners--;
            this.upgradeLockOwnerId = -1;
            this.ExitAndWakeUpAppropriateWaiters();

            SetEvent();
        }

        public void ExitWriteLock()
        {
            int managedThreadId = Thread.CurrentThread.ManagedThreadId;
            if (!this.fIsReentrant)
            {
                if (managedThreadId != this.writeLockOwnerId)
                {
                    throw new SynchronizationLockException("SynchronizationLockException_MismatchedWrite");
                }
                this.EnterMyLock();
            }
            else
            {
                this.EnterMyLock();
                AveReaderWriterCount threadRWCount = this.GetThreadRWCount(managedThreadId, false);
                if (threadRWCount == null)
                {
                    this.ExitMyLock();
                    throw new SynchronizationLockException("SynchronizationLockException_MismatchedWrite");
                }
                AveRecursiveCounts rc = threadRWCount.rc;
                if (rc.writercount < 1)
                {
                    this.ExitMyLock();
                    throw new SynchronizationLockException("SynchronizationLockException_MismatchedWrite");
                }
                rc.writercount--;
                if (rc.writercount > 0)
                {
                    this.ExitMyLock();
                    return;
                }
            }
            this.ClearWriterAcquired();
            this.writeLockOwnerId = -1;
            this.ExitAndWakeUpAppropriateWaiters();

            SetEvent();
        }

        private uint GetNumReaders()
        {
            return (this.owners & 0xfffffff);
        }

        private AveReaderWriterCount GetThreadRWCount(int id, bool DontAllocate)
        {
            int index = id & 0xff;
            AveReaderWriterCount count = null;
            if (this.rwc[index].threadid == id)
            {
                return this.rwc[index];
            }
            if (IsRWEntryEmpty(this.rwc[index]) && !DontAllocate)
            {
                if (this.rwc[index].next == null)
                {
                    this.rwc[index].threadid = id;
                    return this.rwc[index];
                }
                count = this.rwc[index];
            }
            AveReaderWriterCount next = this.rwc[index].next;
            while (next != null)
            {
                if (next.threadid == id)
                {
                    return next;
                }
                if ((count == null) && IsRWEntryEmpty(next))
                {
                    count = next;
                }
                next = next.next;
            }
            if (DontAllocate)
            {
                return null;
            }
            if (count == null)
            {
                next = new AveReaderWriterCount(this.fIsReentrant);
                next.threadid = id;
                next.next = this.rwc[index].next;
                this.rwc[index].next = next;
                return next;
            }
            count.threadid = id;
            return count;
        }

        private void InitializeThreadCounts()
        {
            this.rwc = new AveReaderWriterCount[0x100];
            for (int i = 0; i < this.rwc.Length; i++)
            {
                this.rwc[i] = new AveReaderWriterCount(this.fIsReentrant);
            }
            this.upgradeLockOwnerId = -1;
            this.writeLockOwnerId = -1;
        }

        private static bool IsRWEntryEmpty(AveReaderWriterCount rwc)
        {
            return ((rwc.threadid == -1) || (((rwc.readercount == 0) && (rwc.rc == null)) || (((rwc.readercount == 0) && (rwc.rc.writercount == 0)) && (rwc.rc.upgradecount == 0))));
        }

        private static bool IsRwHashEntryChanged(AveReaderWriterCount lrwc, int id)
        {
            return (lrwc.threadid != id);
        }

        private bool IsWriterAcquired()
        {
            return ((this.owners & 0xbfffffff) == 0);
        }

        private void LazyCreateEvent(ref EventWaitHandle waitEvent, bool makeAutoResetEvent)
        {
            EventWaitHandle handle;
            this.ExitMyLock();
            if (makeAutoResetEvent)
            {
                handle = new AutoResetEvent(false);
            }
            else
            {
                handle = new ManualResetEvent(false);
            }
            this.EnterMyLock();
            if (waitEvent == null)
            {
                waitEvent = handle;
            }
            else
            {
                handle.Close();
            }
        }

        private void SetUpgraderWaiting()
        {
            this.owners |= 0x20000000;
        }

        private void SetWriterAcquired()
        {
            this.owners |= 0x80000000;
        }

        private void SetWritersWaiting()
        {
            this.owners |= 0x40000000;
        }

        private static void SpinWait(int SpinCount)
        {
            if ((SpinCount < 5) && (Environment.ProcessorCount > 1))
            {
                Thread.SpinWait(20 * SpinCount);
            }
            else if (SpinCount < 0x11)
            {
                Thread.Sleep(0);
            }
            else
            {
                Thread.Sleep(1);
            }
        }

        public bool TryEnterReadLock(int millisecondsTimeout)
        {
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeout");
            }
            if (this.fDisposed)
            {
                throw new ObjectDisposedException(null);
            }
            AveReaderWriterCount lrwc = null;
            int managedThreadId = Thread.CurrentThread.ManagedThreadId;
            if (!this.fIsReentrant)
            {
                if (managedThreadId == this.writeLockOwnerId)
                {
                    throw new AveLockRecursionException(AveSR.GetString("LockRecursionException_ReadAfterWriteNotAllowed"));
                }
                this.EnterMyLock();
                lrwc = this.GetThreadRWCount(managedThreadId, false);
                if (lrwc.readercount > 0)
                {
                    this.ExitMyLock();
                    throw new AveLockRecursionException(AveSR.GetString("LockRecursionException_RecursiveReadNotAllowed"));
                }
                if (managedThreadId == this.upgradeLockOwnerId)
                {
                    lrwc.readercount++;
                    this.owners++;
                    this.ExitMyLock();
                    return true;
                }
            }
            else
            {
                this.EnterMyLock();
                lrwc = this.GetThreadRWCount(managedThreadId, false);
                if (lrwc.readercount > 0)
                {
                    lrwc.readercount++;
                    this.ExitMyLock();
                    return true;
                }
                if (managedThreadId == this.upgradeLockOwnerId)
                {
                    lrwc.readercount++;
                    this.owners++;
                    this.ExitMyLock();
                    this.fUpgradeThreadHoldingRead = true;
                    return true;
                }
                if (managedThreadId == this.writeLockOwnerId)
                {
                    lrwc.readercount++;
                    this.owners++;
                    this.ExitMyLock();
                    return true;
                }
            }
            bool flag = true;
            int spinCount = 0;
            while (this.owners >= 0xffffffe)
            {
                if (spinCount < 20)
                {
                    this.ExitMyLock();
                    if (millisecondsTimeout == 0)
                    {
                        return false;
                    }
                    spinCount++;
                    SpinWait(spinCount);
                    this.EnterMyLock();
                    if (IsRwHashEntryChanged(lrwc, managedThreadId))
                    {
                        lrwc = this.GetThreadRWCount(managedThreadId, false);
                    }
                }
                else if (this.readEvent == null)
                {
                    this.LazyCreateEvent(ref this.readEvent, false);
                    if (IsRwHashEntryChanged(lrwc, managedThreadId))
                    {
                        lrwc = this.GetThreadRWCount(managedThreadId, false);
                    }
                }
                else
                {
                    flag = this.WaitOnEvent(this.readEvent, ref this.numReadWaiters, millisecondsTimeout);
                    if (!flag)
                    {
                        return false;
                    }
                    if (IsRwHashEntryChanged(lrwc, managedThreadId))
                    {
                        lrwc = this.GetThreadRWCount(managedThreadId, false);
                    }
                }
            }

            this.owners++;
            lrwc.readercount++;

            this.ExitMyLock();
            return flag;
        }

        public bool TryEnterReadLock(TimeSpan timeout)
        {
            long totalMilliseconds = (long)timeout.TotalMilliseconds;
            if ((totalMilliseconds < -1L) || (totalMilliseconds > 0x7fffffffL))
            {
                throw new ArgumentOutOfRangeException("timeout");
            }
            int millisecondsTimeout = (int)timeout.TotalMilliseconds;
            return this.TryEnterReadLock(millisecondsTimeout);
        }

        public bool TryEnterUpgradeableReadLock(int millisecondsTimeout)
        {
            AveReaderWriterCount threadRWCount;
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeout");
            }
            if (this.fDisposed)
            {
                throw new ObjectDisposedException(null);
            }
            int managedThreadId = Thread.CurrentThread.ManagedThreadId;
            if (!this.fIsReentrant)
            {
                if (managedThreadId == this.upgradeLockOwnerId)
                {
                    throw new AveLockRecursionException(AveSR.GetString("LockRecursionException_RecursiveUpgradeNotAllowed"));
                }
                if (managedThreadId == this.writeLockOwnerId)
                {
                    throw new AveLockRecursionException(AveSR.GetString("LockRecursionException_UpgradeAfterWriteNotAllowed"));
                }
                this.EnterMyLock();
                threadRWCount = this.GetThreadRWCount(managedThreadId, true);
                if ((threadRWCount != null) && (threadRWCount.readercount > 0))
                {
                    this.ExitMyLock();
                    throw new AveLockRecursionException(AveSR.GetString("LockRecursionException_UpgradeAfterReadNotAllowed"));
                }
            }
            else
            {
                this.EnterMyLock();
                threadRWCount = this.GetThreadRWCount(managedThreadId, false);
                if (managedThreadId == this.upgradeLockOwnerId)
                {
                    threadRWCount.rc.upgradecount++;
                    this.ExitMyLock();
                    return true;
                }
                if (managedThreadId == this.writeLockOwnerId)
                {
                    this.owners++;
                    this.upgradeLockOwnerId = managedThreadId;
                    threadRWCount.rc.upgradecount++;
                    if (threadRWCount.readercount > 0)
                    {
                        this.fUpgradeThreadHoldingRead = true;
                    }
                    this.ExitMyLock();
                    return true;
                }
                if (threadRWCount.readercount > 0)
                {
                    this.ExitMyLock();
                    throw new AveLockRecursionException(AveSR.GetString("LockRecursionException_UpgradeAfterReadNotAllowed"));
                }
            }
            int spinCount = 0;

            while (!((this.upgradeLockOwnerId == -1) && (this.owners < 0xffffffe)))
            {
                if (spinCount < 20)
                {
                    this.ExitMyLock();
                    if (millisecondsTimeout == 0)
                    {
                        return false;
                    }
                    spinCount++;
                    SpinWait(spinCount);
                    this.EnterMyLock();
                    continue;
                }
                if (this.upgradeEvent == null)
                {
                    this.LazyCreateEvent(ref this.upgradeEvent, true);
                    continue;
                }
                if (this.WaitOnEvent(this.upgradeEvent, ref this.numUpgradeWaiters, millisecondsTimeout))
                {
                    continue;
                }
                return false;
            }

            this.owners++;
            this.upgradeLockOwnerId = managedThreadId;

            if (this.fIsReentrant)
            {
                if (IsRwHashEntryChanged(threadRWCount, managedThreadId))
                {
                    threadRWCount = this.GetThreadRWCount(managedThreadId, false);
                }
                if (threadRWCount != null)
                {
                    threadRWCount.rc.upgradecount++;
                }
            }
            this.ExitMyLock();
            return true;
        }

        public bool TryEnterUpgradeableReadLock(TimeSpan timeout)
        {
            long totalMilliseconds = (long)timeout.TotalMilliseconds;
            if ((totalMilliseconds < -1L) || (totalMilliseconds > 0x7fffffffL))
            {
                throw new ArgumentOutOfRangeException("timeout");
            }
            int millisecondsTimeout = (int)timeout.TotalMilliseconds;
            return this.TryEnterUpgradeableReadLock(millisecondsTimeout);
        }

        public bool TryEnterWriteLock(int millisecondsTimeout)
        {
            AveReaderWriterCount threadRWCount;
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeout");
            }
            if (this.fDisposed)
            {
                throw new ObjectDisposedException(null);
            }
            int managedThreadId = Thread.CurrentThread.ManagedThreadId;
            bool flag = false;
            if (!this.fIsReentrant)
            {
                if (managedThreadId == this.writeLockOwnerId)
                {
                    throw new AveLockRecursionException(AveSR.GetString("LockRecursionException_RecursiveWriteNotAllowed"));
                }
                if (managedThreadId == this.upgradeLockOwnerId)
                {
                    flag = true;
                }
                this.EnterMyLock();
                threadRWCount = this.GetThreadRWCount(managedThreadId, true);
                if ((threadRWCount != null) && (threadRWCount.readercount > 0))
                {
                    this.ExitMyLock();
                    throw new AveLockRecursionException(AveSR.GetString("LockRecursionException_WriteAfterReadNotAllowed"));
                }
            }
            else
            {
                this.EnterMyLock();
                threadRWCount = this.GetThreadRWCount(managedThreadId, false);
                if (managedThreadId == this.writeLockOwnerId)
                {
                    threadRWCount.rc.writercount++;
                    this.ExitMyLock();
                    return true;
                }
                if (managedThreadId == this.upgradeLockOwnerId)
                {
                    flag = true;
                }
                else if (threadRWCount.readercount > 0)
                {
                    this.ExitMyLock();
                    throw new AveLockRecursionException(AveSR.GetString("LockRecursionException_WriteAfterReadNotAllowed"));
                }
            }
            int spinCount = 0;

            while (!this.IsWriterAcquired())
            {
                if (flag)
                {
                    uint numReaders = this.GetNumReaders();
                    if (numReaders == 1)
                    {
                        break;
                    }
                    if ((numReaders == 2) && (threadRWCount != null))
                    {
                        if (IsRwHashEntryChanged(threadRWCount, managedThreadId))
                        {
                            threadRWCount = this.GetThreadRWCount(managedThreadId, false);
                        }
                        if (threadRWCount.readercount > 0)
                        {
                            break;
                        }
                    }
                }
                if (spinCount < 20)
                {
                    this.ExitMyLock();
                    if (millisecondsTimeout == 0)
                    {
                        return false;
                    }
                    spinCount++;
                    SpinWait(spinCount);
                    this.EnterMyLock();
                    continue;
                }
                if (flag)
                {
                    if (this.waitUpgradeEvent != null)
                    {
                        if (!this.WaitOnEvent(this.waitUpgradeEvent, ref this.numWriteUpgradeWaiters, millisecondsTimeout))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        this.LazyCreateEvent(ref this.waitUpgradeEvent, true);
                    }
                    continue;
                }
                if (this.writeEvent == null)
                {
                    this.LazyCreateEvent(ref this.writeEvent, true);
                    continue;
                }
                if (this.WaitOnEvent(this.writeEvent, ref this.numWriteWaiters, millisecondsTimeout))
                {
                    continue;
                }
                return false;
            }

            this.SetWriterAcquired();

            if (this.fIsReentrant)
            {
                if (IsRwHashEntryChanged(threadRWCount, managedThreadId))
                {
                    threadRWCount = this.GetThreadRWCount(managedThreadId, false);
                }
                if (threadRWCount != null)
                {
                    threadRWCount.rc.writercount++;
                }
            }
            this.ExitMyLock();
            this.writeLockOwnerId = managedThreadId;
            return true;
        }

        public bool TryEnterWriteLock(TimeSpan timeout)
        {
            long totalMilliseconds = (long)timeout.TotalMilliseconds;
            if ((totalMilliseconds < -1L) || (totalMilliseconds > 0x7fffffffL))
            {
                throw new ArgumentOutOfRangeException("timeout");
            }
            int millisecondsTimeout = (int)timeout.TotalMilliseconds;
            return this.TryEnterWriteLock(millisecondsTimeout);
        }

        private bool WaitOnEvent(EventWaitHandle waitEvent, ref uint numWaiters, int millisecondsTimeout)
        {
            waitEvent.Reset();
            numWaiters++;
            this.fNoWaiters = false;
            if (this.numWriteWaiters == 1)
            {
                this.SetWritersWaiting();
            }
            if (this.numWriteUpgradeWaiters == 1)
            {
                this.SetUpgraderWaiting();
            }
            bool flag = false;
            this.ExitMyLock();
            try
            {
                flag = waitEvent.WaitOne(millisecondsTimeout, false);
            }
            finally
            {
                this.EnterMyLock();
                numWaiters--;
                if (((this.numWriteWaiters == 0) && (this.numWriteUpgradeWaiters == 0)) && ((this.numUpgradeWaiters == 0) && (this.numReadWaiters == 0)))
                {
                    this.fNoWaiters = true;
                }
                if (this.numWriteWaiters == 0)
                {
                    this.ClearWritersWaiting();
                }
                if (this.numWriteUpgradeWaiters == 0)
                {
                    this.ClearUpgraderWaiting();
                }
                if (!flag)
                {
                    this.ExitMyLock();
                }
            }
            return flag;
        }

        // Properties
        public int CurrentReadCount
        {
            get
            {
                int numReaders = (int)this.GetNumReaders();
                if (this.upgradeLockOwnerId != -1)
                {
                    return (numReaders - 1);
                }
                return numReaders;
            }
        }

        public bool IsReadLockHeld
        {
            get
            {
                return (this.RecursiveReadCount > 0);
            }
        }

        public bool IsUpgradeableReadLockHeld
        {
            get
            {
                return (this.RecursiveUpgradeCount > 0);
            }
        }

        public bool IsWriteLockHeld
        {
            get
            {
                return (this.RecursiveWriteCount > 0);
            }
        }

        public AveLockRecursionPolicy RecursionPolicy
        {
            get
            {
                if (this.fIsReentrant)
                {
                    return AveLockRecursionPolicy.SupportsRecursion;
                }
                return AveLockRecursionPolicy.NoRecursion;
            }
        }

        public int RecursiveReadCount
        {
            get
            {
                int managedThreadId = Thread.CurrentThread.ManagedThreadId;
                int readercount = 0;
                this.EnterMyLock();
                AveReaderWriterCount threadRWCount = this.GetThreadRWCount(managedThreadId, true);
                if (threadRWCount != null)
                {
                    readercount = threadRWCount.readercount;
                }
                this.ExitMyLock();
                return readercount;
            }
        }

        public int RecursiveUpgradeCount
        {
            get
            {
                int managedThreadId = Thread.CurrentThread.ManagedThreadId;
                if (this.fIsReentrant)
                {
                    int upgradecount = 0;
                    this.EnterMyLock();
                    AveReaderWriterCount threadRWCount = this.GetThreadRWCount(managedThreadId, true);
                    if (threadRWCount != null)
                    {
                        upgradecount = threadRWCount.rc.upgradecount;
                    }
                    this.ExitMyLock();
                    return upgradecount;
                }
                if (managedThreadId == this.upgradeLockOwnerId)
                {
                    return 1;
                }
                return 0;
            }
        }

        public int RecursiveWriteCount
        {
            get
            {
                int managedThreadId = Thread.CurrentThread.ManagedThreadId;
                int writercount = 0;
                if (this.fIsReentrant)
                {
                    this.EnterMyLock();
                    AveReaderWriterCount threadRWCount = this.GetThreadRWCount(managedThreadId, true);
                    if (threadRWCount != null)
                    {
                        writercount = threadRWCount.rc.writercount;
                    }
                    this.ExitMyLock();
                    return writercount;
                }
                if (managedThreadId == this.writeLockOwnerId)
                {
                    return 1;
                }
                return 0;
            }
        }

        public int WaitingReadCount
        {
            get
            {
                return (int)this.numReadWaiters;
            }
        }

        public int WaitingUpgradeCount
        {
            get
            {
                return (int)this.numUpgradeWaiters;
            }
        }

        public int WaitingWriteCount
        {
            get
            {
                return (int)this.numWriteWaiters;
            }
        }
    }

    public static class AveSR
    {
        public static string GetString(string name)
        {
            return name;
        }
    }

    internal class AveRecursiveCounts
    {
        // Fields
        public int upgradecount;
        public int writercount;
    }

    internal class AveReaderWriterCount
    {
        // Fields
        public AveReaderWriterCount next;
        public AveRecursiveCounts rc;
        public int readercount;
        public int threadid = -1;

        // Methods
        public AveReaderWriterCount(bool fIsReentrant)
        {
            if (fIsReentrant)
            {
                this.rc = new AveRecursiveCounts();
            }
        }
    }
}