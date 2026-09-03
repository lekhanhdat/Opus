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
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using AvePoint.GCommon;

namespace AvePoint.Wrapper.BackupRestore
{
    internal class CacheManager_Old
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private List<Cache_Old> mCaches = new List<Cache_Old>(AveBackupRestoreConfig.CACHECOUNT);
        private AveOD4BList mList = null;
        //columns that need to backup its value
        private List<string> mColumns = new List<string>();
        //Controller能知道是否有item 备份完成
        private AveOD4BRequestController mController = null;
        //记录正在备份的item信息，因为外围每次只会read一个item
        Dictionary<int, AveBRItemInfo> item = new Dictionary<int, AveBRItemInfo>();

        public CacheManager_Old(AveOD4BList list, List<AveBRItemInfo> incItems = null)
        {
            mList = list;
            mColumns = list.Columns;
            mController = list.Controller;
            InitCache();
            //list.GetFolders();
            RequestData("");
        }

        private void InitCache()
        {
            for (int i = 0; i < mCaches.Capacity; i++)
            {
                Cache_Old cache = new Cache_Old("");
                mCaches.Add(cache);
            }
        }

        private void RequestData(string folderUrl)
        {
            var itemInfos = mController.GetItemsInfo(mList.WebUrl, folderUrl, mColumns);

            foreach (var itemInfo in itemInfos)
            {
                if (!SkipCheckVersions(itemInfo.UIVersion))
                {
                    
                    continue;
                }
                List<AveBRItemInfo> versionInfos = null;//mController.GetVersion();
                Write(folderUrl, versionInfos);
                foreach (var version in versionInfos)
                {
                    
                }
            }
        }

        private void FillContent()
        {
            string fileUrl = string.Empty;
        }

        private bool SkipCheckVersions(int version)
        {
            if (version == 1)
            {
                return true;
            }
            if (version == 512)
            {
                if (mList.EnableVersioning && !mList.EnableMinorVersions)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Read data from cache
        /// Delete data after reading
        /// </summary>
        public AveBRItemInfo Read()
        {
            return mCaches[0].Read();
        }

        
        /// <summary>
        /// Write data to cache
        /// </summary>
        public bool Write(string folderUrl, List<AveBRItemInfo> infos)
        {
            bool successed = true;
            mCaches[0].Write(folderUrl, infos);
            return successed;
        }

        public bool RequestContent(string folderUrl, AveBRItemInfo info)
        {
            //mController.GetFileContent(mList.WebUrl,)
            string contentPath = "";
            mCaches[0].SetContent(info.Name, info.UIVersion, contentPath);
            return true;
        }

        /// <summary>
        /// Delete data in cache
        /// </summary>
        public void Delete()
        { }
    }

    internal delegate void ProcessData(object sender, ProcessDataEventArgs args);

    internal class Cache_Old
    {
        internal event ProcessData mDataProcessor;

        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string mIdentity;
        //item count that waiting for be backed up
        private int mLeftItemsCount = 0;
        //internal collection stores item version info in case of there are a lot of versions of one item
        
        private ConcurrentDictionary<string, string> contentMap = new ConcurrentDictionary<string, string>();
        private ConcurrentQueue<string> mFinishedItem = new ConcurrentQueue<string>();
        
        private ConcurrentDictionary<string, CacheQueue<AveBRItemInfo>> mAllItems = new ConcurrentDictionary<string, CacheQueue<AveBRItemInfo>>();
        private CacheQueue<AveBRItemInfo> mCurrentNode = null;
        private string mCurrentLeafName = string.Empty;
        private AutoResetEvent mContentEvent = new AutoResetEvent(false);
        private SemaphoreSlim mCounter = new SemaphoreSlim(10, 10);
        private readonly bool mKeepOrder;
        // 0 means not complete writing
        private int mCompleted = 0;
        
        internal int Capacity
        {
            get { return mLeftItemsCount; }
        }

        internal string Identity
        {
            get { return mIdentity; }
        }

        internal void CompleteWrite()
        {
            Interlocked.CompareExchange(ref mCompleted, 1, 0);
        }

        internal void CompleteVersionWrite(string leafName)
        {
            mAllItems[leafName].CompleteWrite();
        }

        public Cache_Old(string id, bool keepOrder = false)
        {
            mIdentity = id;
            mKeepOrder = keepOrder;
        }
        
        //private void GetNextItem()
        //{
        //    int count = 0;
        //    while (mFinishedItem.IsEmpty)
        //    {
        //        mEvent.WaitOne(1000);
        //        count++;
        //        if (count == 30 * 60 * 60)
        //        {
        //            throw new Exception("Cannot read available data in 30 mins");
        //        }
        //    }
        //    string key = string.Empty;
        //    if (mFinishedItem.TryDequeue(out key))
        //    {
        //        lock (mCurrent)
        //        {
        //            versions.TryRemove(key, out mCurrent);
        //        }
        //        //log
        //    }
        //    else
        //    {
        //        //log 
        //    }
        //}

        private bool MapContent(AveBRItemInfo info)
        {
            string key = string.Format("{0}:{1}", info.Name, info.UIVersion.ToString());
            string value = string.Empty;
            if (!contentMap.TryRemove(key, out value))
            {
                return false;
            }
            info.Content = null;
            return true;
        }
        
        public bool SetContent(string leafName, int version, string contentPath)
        {
            string key = string.Format("{0}:{1}", leafName, version.ToString());
            contentMap[key] = contentPath;
            mContentEvent.Set();
            return true;
        }

        public void Write(string leafName, List<AveBRItemInfo> items)
        {
            //OnWriting
            InternalWrite(leafName, items);
            //OnWrited
        }
        private void InternalWrite(string leafName, List<AveBRItemInfo> items)
        {
            if (items == null || items.Count == 0)
            {
                return;
            }
            mCounter.Wait();
            foreach (var item in items)
            {
                mAllItems[leafName].Enqueue(item);
            }
        }

        public AveBRItemInfo Read()
        {
            //OnReading
            AveBRItemInfo info = InternalRead();
            while (!MapContent(info))
            {
                mContentEvent.WaitOne(1000);
            }
            return info;
            //OnReaded
        }

        private AveBRItemInfo InternalRead()
        {
            if (mCurrentNode == null || mCurrentNode.Count == 0)
            {
                GetNextNode();
            }
            AveBRItemInfo info = mCurrentNode.Dequeue();

            return info;
        }

        //async sync
        private void GetNextNode()
        {
            if (mFinishedItem.IsEmpty)
            {
                mCurrentLeafName = mAllItems.Keys.First();
            }
            else
            {
                mFinishedItem.TryDequeue(out mCurrentLeafName);
                //log
            }

            while (mAllItems.TryRemove(mCurrentLeafName, out mCurrentNode)) break;
            mCounter.Release();
        }

        private void ProcessContent()
        {
            if (mDataProcessor == null)
            {
                return;
            }
            foreach (string key in mAllItems.Keys)
            {
                AveBRItemInfo info = null;
                while (mAllItems[key].TryReadNext(out info))
                {
                    Array.ForEach(mDataProcessor.GetInvocationList(), action =>
                    {
                        try
                        {
                            action.DynamicInvoke(info, new ProcessDataEventArgs());
                        }
                        catch (Exception e)
                        {
                            mLog.Warn("When invoke a method error, details:[{0}] ", e);
                        }
                    });
                }

                if (!mKeepOrder)
                {
                    mFinishedItem.Enqueue(key);
                }
            }
        }
        class CacheQueue<T>
        {
            private ConcurrentQueue<T> data = null;
            private T[] mArray = null;
            private int mArrayCount = 0;
            private bool mArrayInit = false;
            private int mIndex = 0;
            private int mCompleted = 0;
            private int mCurrentCount = 0;
            private object lockObj = new object();
            
            private AutoResetEvent mAutoEvent = new AutoResetEvent(false);

            internal CacheQueue()
            {   
                data = new ConcurrentQueue<T>();
            }

            public int Count
            {
                get { return this.data.Count; }
            }

            public void Enqueue(T info)
            {
                if (Interlocked.CompareExchange(ref mCompleted, 0, 1) == 1)
                {
                    throw new Exception("Can not enqueue data after completing write.");
                }
                this.data.Enqueue(info);
                Interlocked.Increment(ref mCurrentCount);
                if (mArrayInit)
                {
                    mArray[mArrayCount] = info;
                    Interlocked.Increment(ref mArrayCount);
                    mAutoEvent.Set();
                }
            }

            public T Dequeue()
            {
                T result;
                // sync mArray ???
                while (this.data.TryDequeue(out result)) break;
                Interlocked.Decrement(ref mCurrentCount);

                return result;
            }

            internal void CompleteWrite()
            {
                Interlocked.CompareExchange(ref mCompleted, 1, 0);
            }

            internal bool TryReadNext(out T result)
            {
                if (Interlocked.CompareExchange(ref mCompleted, 0, 1) == 1 && mIndex == mCurrentCount)
                {
                    result = default(T);
                    return false;
                }
                if (mArray == null)
                {
                    lock(lockObj)
                    {
                        if (mArray == null)
                        {
                            mArray = data.ToArray();
                            Interlocked.Add(ref mArrayCount, mArray.Length);
                            mArrayInit = true;
                        }
                    }
                }
                if (mIndex == mArrayCount)
                {
                    mAutoEvent.WaitOne();
                }
                result = mArray[mIndex];
                Interlocked.Increment(ref mIndex);
                return true;
            }

        }
    }

    internal class ProcessDataEventArgs : EventArgs
    {

    }
    
}
