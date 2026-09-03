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
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Configuration;

using AvePoint.GCommon;

namespace AvePoint.Wrapper.BackupRestore
{
    internal class ProcessorConfig
    {
        public string Name { get; set; }
        //[pending]
        public string WebUrl { get; set; }
        //default value 10M
        private long mLargeFileSizeThreshold = 10 * 1024 * 1024;
        public long LargeFileSizeThreshold
        {
            get { return mLargeFileSizeThreshold; }
            set
            {
                if (value < 0) throw new ArgumentException("Threshold value cannot less than 0", "LargeFileSizeThreshold");

                mLargeFileSizeThreshold = value;
            }
        }

        private long mTempFileSizeThreshold = 0L;
        public long TempFileSieThreshold
        {
            get { return mTempFileSizeThreshold; }
            set
            {
                if (value < 0) throw new ArgumentException("Threshold value cannot less than 0", "TempFileSizeThreshold");

                mTempFileSizeThreshold = value;
            }
        }

        public string FolderUrl { get; set; }
        public ProcessorType ProcessorType { get; set; }
    }
    internal class AveDataProcessor : IDisposable
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private BlockingCollection<AveBRItemInfo> mOutputCollection;
        private IAveDataProcessor mInternalProcessor;

        public event EventHandler<ProcessorFailedEventArgs> FailProcessor
        {
            add
            {
                mInternalProcessor.ProcessorFailed += value;
            }
            remove
            {
                mInternalProcessor.ProcessorFailed -= value;
            }
        }


        internal AveDataProcessor(ProcessorConfig config, AveOD4BRequestController controller)
        {
            mOutputCollection = new BlockingCollection<AveBRItemInfo>();
            InitInternalProcessor(config, controller);
        }

        internal IEnumerable<AveBRItemInfo> Results
        {
            get
            {
                return this.mOutputCollection.GetConsumingEnumerable();
            }
        }

        internal void StartProcess(IEnumerable<AveBRItemInfo> inputData)
        {
            mInternalProcessor.Process(inputData, this.mOutputCollection);
        }

        private void InitInternalProcessor(ProcessorConfig config, AveOD4BRequestController controller)
        {
            switch (config.ProcessorType)
            {
                case ProcessorType.Ordered:
                    mInternalProcessor = new OrderedDataProcessor(config, controller);
                    break;
                case ProcessorType.Unordered:
                    mInternalProcessor = new UnorderedDataProcessor(config, controller);
                    break;
                default:
                    mInternalProcessor = new UnorderedDataProcessor(config, controller);
                    break;
            }
        }

        internal void ReleaseFileUsage(long fileSize)
        {
            mInternalProcessor.ReleaseItemUsage(fileSize);
        }

        public void Dispose()
        {
            if (this.mInternalProcessor != null)
            {
                this.mInternalProcessor.Dispose();
                this.mInternalProcessor = null;
            }
        }
    }

    internal enum ProcessorType
    {
        Ordered,
        Unordered,
    }

    internal class OrderedDataProcessor : IAveDataProcessor
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly AveOD4BRequestController mController;
        protected ProcessorConfig mConfig;
        protected TempStorageManager mStorageManager = null;
        protected string mWebUrl = null;
        private const int COUNTER = 10;
        private ActionScheduler<AveBRItemInfo> mContentBackupScheduler = null;
        // 0 means add completed, otherwise 1.
        private int mCompleteAdding = 0;

        private WaitingCollection<Stream> mContentMaps = new WaitingCollection<Stream>();
        private AutoResetEvent mStructureDataEvent = new AutoResetEvent(false);
        protected ConcurrentDictionary<string, ConcurrentQueue<AveBRItemInfo>> mStructureData = new ConcurrentDictionary<string, ConcurrentQueue<AveBRItemInfo>>();

        //version collections of mCurrentName
        protected ConcurrentQueue<AveBRItemInfo> mCurrentItem = null;
        //name of item that is backing up
        protected string mCurrentName = string.Empty;
        //version number of item that is backing up
        protected int mCurrentVersion = 0;
        protected List<string> mFinishAddingItems = new List<string>();

        public event EventHandler<ProcessorFailedEventArgs> ProcessorFailed;

        internal OrderedDataProcessor(ProcessorConfig config, AveOD4BRequestController controller)
        {
            if (config == null) throw new ArgumentNullException("config");
            if (string.IsNullOrEmpty(config.Name)) throw new ArgumentNullException("Name", "ProcessorConfig.Name cannot be empty");
            if (string.IsNullOrEmpty(config.WebUrl)) throw new ArgumentNullException("WebUrl", "ProcessorConfig.WebUrl cannot be empty");

            mStorageManager = new TempStorageManager(config.Name, config.TempFileSieThreshold);
            mWebUrl = config.WebUrl;
            mConfig = config;
            mController = controller;
            int count = 0;
            Int32.TryParse(ConfigurationManager.AppSettings["ContentBackupCounter"], out count);
            if (count <= 0)
            {
                count = COUNTER;
            }
            mLog.Info("ContentBackupCounter is {0}.", count.ToString());
            this.mContentBackupScheduler = new ActionScheduler<AveBRItemInfo>(this, count);
        }
        protected bool AddCompleted
        {
            get
            {
                return Interlocked.CompareExchange(ref this.mCompleteAdding, 1, 1) == 1;
            }
        }

        private void OnProcessorFailed(ProcessorFailedEventArgs eventArgs)
        {
            if (ProcessorFailed != null)
            {
                ProcessorFailed.Invoke(this, eventArgs);
            }
        }

        public void Process(IEnumerable<AveBRItemInfo> versions, BlockingCollection<AveBRItemInfo> results)
        {
            StartBackupContentAsync(versions, results);
            //
            CompleteBackupAsync(results);
        }

        private Task StartBackupContentAsync(IEnumerable<AveBRItemInfo> versions, BlockingCollection<AveBRItemInfo> results)
        {
            return Task.Run(() =>
            {
                string preName = string.Empty;
                try
                {
                    foreach (var version in versions)
                    {
                        try
                        {
                            mLog.Info("Start backing up item. Name:{0}. Version:{1}", version.Name, version.UIVersion.ToString());

                            AddToStructureData(version);
                            if (!string.IsNullOrEmpty(preName) && !string.Equals(preName, version.Name))
                            {
                                ItemFinishAdding(preName);
                            }
                        }
                        catch (Exception ex)
                        {
                            mLog.Error("Failed to prepare backup file or file version. FileUrl:{0}, Version:{1}. Error:{2}",
                                version.ServerRelativeUrl, version.UIVersion.ToString(), ex);
                            version.FailedCount++;
                            version.Result.SetFailed(ex);
                        }

                        mContentBackupScheduler.DoTaskAsync(version);
                        preName = version.Name;
                    }
                    ItemFinishAdding(preName);
                }
                catch (Exception ex)
                {
                    mLog.Error("Failed to prepare file or file version under folder:{0}. Error:{1}", mConfig.FolderUrl, ex);
                    ItemFinishAdding(preName);
                    var itemInfo = new AveBRItemInfo()
                    {
                        Name = "[Discovery Logic]",
                        ServerRelativeUrl = Path.Combine(mConfig.FolderUrl, "[Discovery Logic]"),
                        IsCurrent = true,
                    };
                    itemInfo.Result.SetFailed(ex);
                    results.Add(itemInfo);
                    //OnProcessorFailed(new ProcessorFailedEventArgs() { Exception = ex });
                }
                finally
                {
                    CompleteAdding();
                    mLog.Info("Complate adding data of content backup");
                }
            });
        }

        private Task CompleteBackupAsync(BlockingCollection<AveBRItemInfo> results)
        {
            return Task.Run(() =>
            {
                try
                {
                    while (GetNextItem())
                    {
                        AveBRItemInfo version = null;
                        if (!this.mCurrentItem.TryDequeue(out version))
                        {
                            continue;
                        }
                        this.mCurrentVersion = version.UIVersion;
                        SetVersionContent(version);

                        results.Add(version);
                    }
                }
                catch (Exception ex)
                {
                    mLog.Error("An error occurred while complete backup. Error:{0}", ex);
                }
                finally
                {
                    results.CompleteAdding();
                    mLog.Info("Finishing adding data to results of DataProcessor");
                }
            });
        }

        private bool GetNextItem()
        {
            if (ShouldMoveNext())
            {
                string tempName = GetItemName();
                if (string.IsNullOrEmpty(tempName))
                {
                    mLog.Info("Finish Reading StructureData");
                    return false;
                }
                ConcurrentQueue<AveBRItemInfo> tempItem;
                if (this.mStructureData.TryRemove(tempName, out tempItem))
                {
                    if (this.mCurrentItem != null && this.mCurrentItem.Count > 0)
                    {
                        throw new Exception("Missing Data!");
                    }
                    this.mCurrentItem = tempItem;
                    this.mCurrentName = tempName;
                    mLog.Info("Current item name:{0}", this.mCurrentName);
                }
            }
            return true;
        }

        private bool ShouldMoveNext()
        {
            if (this.mCurrentItem == null)
            {
                return true;
            }
            if (this.mCurrentItem != null && this.mCurrentItem.Count > 0)
            {
                return false;
            }
            if (!CheckItemFinishAdding(this.mCurrentName))
            {
                //如果add data方法慢的话，mCurrentItem不为空并且count为0
                //出现这种情况比较少, 暂时先不使用信号量只是sleep 500毫秒, 避免出现空转的情况
                mLog.Warn("Add data {0} slowly.", this.mCurrentName);
                Thread.Sleep(500);
                return false;
            }

            return true;
        }

        private void SetVersionContent(AveBRItemInfo version)
        {
            mLog.Info("Try to get file content. Name:{0}. Version:{1}", version.Name, version.UIVersion.ToString());
            string key = string.Format("{0}_{1}", version.UIVersion.ToString(), version.Name);
            version.Content = this.mContentMaps.WaitedPopValue(key);
            mLog.Info("Get file content successfully. Name:{0}. Version:{1}", version.Name, version.UIVersion.ToString());
        }

        private void CompleteAdding()
        {
            Interlocked.CompareExchange(ref this.mCompleteAdding, 1, 0);
        }

        protected void BackupContent(AveBRItemInfo version)
        {
            Stream content = null;
            string key = string.Format("{0}_{1}", version.UIVersion.ToString(), version.Name);
            try
            {
                //[pending]
                mLog.Info("Start backing up file content. Name:{0}. Version:{1}. Length:{2} bytes", version.Name, version.UIVersion.ToString(), version.Length.ToString());
                int uiVersion = 0;
                if (!version.IsCurrent)
                {
                    uiVersion = version.UIVersion;
                }
                content = this.mController.GetFileContent(this.mWebUrl, version.ServerRelativeUrl, version.UniqueId, uiVersion);
                version.Length = Convert.ToInt64(content.Length);
                mLog.Info("Finish backing up file content. Name:{0}. Version:{1}. Length:{2} bytes", version.Name, version.UIVersion.ToString(), version.Length.ToString());
                CompleteOneVersion(version.Name);
            }
            catch (FileNotFoundException ffe)
            {
                mLog.Warn("{0} File url: {1}, version: {2}", ffe.Message, ffe.FileName, version.UIVersion);
                ReleaseItemUsage(version.Length);
                version.Length = 0L;
                version.Result.SetSkipped(ffe);
                CompleteOneVersion(version.Name);
            }
            catch (Exception ex)
            {
                mLog.Warn("Failed backup file version content. File url:{0}, version:{1}. Error:{2}", version.ServerRelativeUrl, version.UIVersion.ToString(), ex);
                ReleaseItemUsage(version.Length);
                version.Length = 0L;
                version.FailedCount++;
                version.Result.SetFailed(ex);
                CompleteOneVersion(version.Name);
            }
            finally
            {
                this.mContentMaps.AddOrUpdate(key, content);
            }
        }

        private void AddToStructureData(AveBRItemInfo info)
        {
            if (string.Equals(info.Name, this.mCurrentName))
            {
                this.mCurrentItem.Enqueue(info);
                RecordData(info.Name, 1);
                return;
            }
            ConcurrentQueue<AveBRItemInfo> queue = null;
            if (!this.mStructureData.TryGetValue(info.Name, out queue))
            {
                queue = new ConcurrentQueue<AveBRItemInfo>();
                this.mStructureData[info.Name] = queue;
            }
            queue.Enqueue(info);
            RecordData(info.Name, 1);
            this.mStructureDataEvent.Set();
        }

        protected virtual void CompleteOneVersion(string name)
        { }

        private void ItemFinishAdding(string name)
        {
            lock (this.mFinishAddingItems)
            {
                this.mFinishAddingItems.Add(name);
            }
            mLog.Info("Finish adding item:{0}", name);
        }

        protected bool CheckItemFinishAdding(string itemName)
        {
            if (string.IsNullOrEmpty(this.mCurrentName))
            {
                return false;
            }

            lock (this.mFinishAddingItems)
            {
                return this.mFinishAddingItems.Contains(itemName);
            }
        }
        protected virtual void RecordData(string name, int count)
        { }
        protected virtual string GetItemName()
        {
            if (this.mStructureData.Count == 0 && this.AddCompleted)
            {
                return string.Empty;
            }
            while (this.mStructureData.Count == 0)
            {
                if (this.AddCompleted)
                {
                    if (this.mStructureData.Count == 0)
                    {
                        return string.Empty;
                    }
                    break;
                }
                this.mStructureDataEvent.WaitOne(2000);
            }

            lock (this.mFinishAddingItems)
            {
                foreach (var item in this.mFinishAddingItems)
                {
                    if (this.mStructureData.ContainsKey(item))
                    {
                        return item;
                    }
                }
            }

            return this.mStructureData.Keys.First();
        }

        public void ReleaseItemUsage(long size)
        {
            this.mStorageManager.ReleaseFileUsage(size);
        }

        public void Dispose()
        {
            this.mStorageManager.Dispose();
            this.mContentMaps.Dispose();
            this.mStructureDataEvent.Dispose();
            if (this.mContentBackupScheduler != null)
            {
                this.mContentBackupScheduler.Dispose();
                this.mContentBackupScheduler = null;
            }
        }
        class WaitingCollection<TValue> : IDisposable
        {
            object lockObj = new object();
            Dictionary<string, TValue> mInternalDictionary = new Dictionary<string, TValue>();
            AutoResetEvent mContentEvent = new AutoResetEvent(false);
            string mCurrentWaitingItem;

            public TValue WaitedPopValue(string key)
            {
                if (key == null) throw new ArgumentNullException("key", "Key cannot be null");

                bool needWait = false;
                lock (lockObj)
                {
                    TValue content = default(TValue);
                    if (this.mInternalDictionary.TryGetValue(key, out content))
                    {
                        this.mInternalDictionary.Remove(key);
                        return content;
                    }
                    else
                    {
                        this.mCurrentWaitingItem = key;
                        needWait = true;
                    }
                }

                if (needWait)
                {
                    this.mContentEvent.WaitOne();
                }

                lock (lockObj)
                {
                    TValue content = this.mInternalDictionary[key];
                    this.mInternalDictionary.Remove(key);
                    this.mCurrentWaitingItem = null;
                    return content;
                }
            }

            public void AddOrUpdate(string key, TValue value)
            {
                lock (lockObj)
                {
                    this.mInternalDictionary[key] = value;
                    if (string.Equals(key, this.mCurrentWaitingItem, StringComparison.OrdinalIgnoreCase))
                    {
                        this.mContentEvent.Set();
                    }
                }
            }

            public void Dispose()
            {
                this.mContentEvent.Dispose();
            }
        }

        class ActionScheduler<T> : IDisposable where T : AveBRItemInfo
        {
            private SemaphoreSlim mActionCounter = null;
            private Action<T> mAction;
            OrderedDataProcessor mProcessor = null;
            public ActionScheduler(OrderedDataProcessor processor, int threshold)
            {
                mProcessor = processor;
                this.mAction = processor.BackupContent;
                mActionCounter = new SemaphoreSlim(threshold);
            }

            private bool IsCurrentVersionTask(T task)
            {
                if (mProcessor.mCurrentVersion == 0) return false;

                return string.Equals(task.Name, mProcessor.mCurrentName, StringComparison.OrdinalIgnoreCase)
                    && task.UIVersion == mProcessor.mCurrentVersion;
            }
            
            public Task DoTaskAsync(T task)
            {
                mProcessor.mStorageManager.ReserveDiskSize(task.Length);
                this.mActionCounter.Wait();
                return Task.Run(() => WrapperTask(task));
            }

            private void WrapperTask(T task)
            {
                try
                {
                    this.mAction(task);
                }
                finally
                {
                    this.mActionCounter.Release();
                }
            }

            public void Dispose()
            {
                if (this.mActionCounter != null)
                {
                    this.mActionCounter.Dispose();
                    this.mActionCounter = null;
                }
            }
        }
    }

    internal class UnorderedDataProcessor : OrderedDataProcessor
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        ConcurrentQueue<string> mFinishedItems = new ConcurrentQueue<string>();
        Dictionary<string, int> mVersionCount = new Dictionary<string, int>();
        private object lockObj = new object();

        internal UnorderedDataProcessor(ProcessorConfig config, AveOD4BRequestController controller)
            : base(config, controller)
        { }

        protected override string GetItemName()
        {
            if (!mFinishedItems.IsEmpty)
            {
                string name = string.Empty;
                mFinishedItems.TryDequeue(out name);
                return name;
            }
            return base.GetItemName();
        }

        protected override void RecordData(string name, int count)
        {
            lock (lockObj)
            {
                int value = 0;
                mVersionCount.TryGetValue(name, out value);
                Interlocked.Add(ref value, count);
                mVersionCount[name] = value;
            }
        }

        protected override void CompleteOneVersion(string name)
        {
            lock (lockObj)
            {
                int value = 0;
                if (mVersionCount.TryGetValue(name, out value))
                {
                    Interlocked.Decrement(ref value);
                    if (value == 0 && !string.Equals(name, mCurrentName) && CheckItemFinishAdding(name))
                    {
                        mFinishedItems.Enqueue(name);
                        //mLog.Info("Item {0} finished in backend.", name);
                        //remove item in mFinishAddingItems
                    }
                }
                else
                {
                    mLog.Warn("Cannot find item version record. Item {0}", name);
                }
            }
        }
    }
}
