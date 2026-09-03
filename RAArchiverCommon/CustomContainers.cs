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
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using AvePoint.GCommon;
using System.Reflection;
using LOGRESOURCE = Merged18NResources.Archive.Archive;


namespace AvePoint.RA.SharePoint.ArchiverCommon
{
    //public interface IInterDependencyNode
    //{
    //    /// <summary>
    //    /// 这个值只能从子结点影响父结点。
    //    /// </summary>
    //    object InterNodesPassValue { get; set; }
    //}
    /// <summary>
    /// 在cache处理过程中这个对象做为一个辅助方式生成了这个对象的定义
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    public class BackwardDependenceNode<T1>
        where T1 : class
    {
        /// <summary>
        /// 0...+∝， 0 is the top level
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// 存取阈值，如果这个值是true就表示这个对象是需要被处理的对象，那么这个时候它的父结点就对受到影响。
        /// </summary>
        public bool SFThreshold { get; set; }
        /// <summary>
        /// 表示这个对象是否已经被处理（存\取）
        /// </summary>
        public bool HasProcessed { get; internal set; }
        /// <summary>
        /// HasReported
        /// </summary>
        public bool HasReported { get; internal set; }
        /// <summary>
        /// 这个对象上挂载的一个实际需要保存的值。
        /// </summary>
        public T1 Value { get; set; }
    }
    /// <summary>
    /// 实现这个接口的类型提供两种功能， 将T1类型的数据存储成格式化数据集，并能反向从数据集读取显示给用户
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    public interface IScheduleContainer<T1> : IDisposable
        where T1 : class
    {
        /// <summary>
        /// 存储   hasReported参数为了解决Bug[ADO-74489]
        /// </summary>
        /// <param name="node"></param>
        void Store(T1 node, bool hasReport);
        /// <summary>
        /// AddReport  为了解决Bug[ADO-74489]
        /// </summary>
        /// <param name="node"></param>
        void AddReport(T1 node);
        /// <summary>
        /// 获取
        /// </summary>
        /// <returns>如果是空则结束</returns>
        BackwardDependenceNode<T1> FetchNext();
        /// <summary>
        /// 存储结束时调用以保证所有的数据正确存储。
        /// </summary>
        void Flush();
    }
    /// <summary>
    /// 定义这个接口的主要目的是规范Cache管理， 尽量避免对实现类型的修改对调用者的影响。
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    public interface IBackwardDependencyNodeCache<T1> : IDisposable
        where T1 : class
    {
        /// <summary>
        /// 加入缓存。
        /// </summary>
        /// <param name="node"></param>
        void PutIn(T1 node, int level, bool threshold);
        /// <summary>
        /// AddDocScanReport  为了解决Bug[ADO-74489]。
        /// </summary>
        /// <param name="node"></param>
        void AddDocScanReport(T1 node);
        /// <summary>
        /// 从实现IScheduleContainer<T1>的类型读取下一个需要处理的数据，经过处理之后， 显示给用户 。
        /// </summary>
        /// <returns>如果是空，则结束</returns>
        T1 FetchNext();
        /// <summary>
        /// 读取缓存中的指定level的数据
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        T1 ValueInCacheOfLevel(int level);
        /// <summary>
        /// 读取缓存中制定Level的Parent的数据
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        T1 ParentValueInCacheOfLevel(int level);
        /// <summary>
        /// 存储的时候， flush会将内存中的还未写入container中的数据做写入操作。
        /// </summary>
        void Flush();
        /// <summary>
        /// Close
        /// </summary>
        void Close();
        /// <summary>
        /// Executes a custom action against the underlying container when specialized behavior is required.
        /// </summary>
        /// <param name="action">Action that receives the wrapped container instance.</param>
        void ExecuteContainerAction(Action<IScheduleContainer<T1>> action);
    }
    /// <summary>
    /// 默认的一个空操作的IScheduleContainer<T1>的对象，目的在于对cache的数据对象控制，不需要将数据写入其它容器。
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    public class ScheduleNullContainer<T1> : IScheduleContainer<T1>
        where T1 : class
    {
        public void Store(T1 node, bool hasReport)
        {
            if (node != null && node is IDisposable)
            {
                using (node as IDisposable) { }
            }
        }
        public void AddReport(T1 node)
        {
            if (node != null && node is IDisposable)
            {
                using (node as IDisposable) { }
            }
        }
        public BackwardDependenceNode<T1> FetchNext() { return null; }
        public void Flush() { }
        public void Dispose() { }
    }
    /// <summary>
    /// 提供的功能为：对一个结点的操作依赖于它是否有子结点会被操作或者结点本身被显式指定需要操作，如果一个结点本身没有被指定显式操作并且它没有子结点被操作， 
    /// 那么它本身不需要被操作，并且发现结点的顺序是从父结点开始的。
    /// 例如，将一个代表SiteCollection的结点（未显式指定“操作”）放入Cache. 直到遍历完SiteCollection下的所有子结点后才能确定SiteCollection是不是需要处理。通过使用
    /// 这个类型的功能。 只需要正确指定子结点的Level和指定是否需要操作而将子结点加入cache,是否会保存，则由主个cache决定。
    /// 还提供了维护父结点的功能。
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    public class BackwardDependenceNodeCache<T1> : IBackwardDependencyNodeCache<T1>
        where T1 : class
    {
        private AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private IScheduleContainer<T1> mContainer;
        private List<BackwardDependenceNode<T1>> mCacheNodes;
        private List<BackwardDependenceNode<T1>> mFetchNodes;
        private BackwardDependenceNode<T1> mPeekCacheNode;
        private BackwardDependenceNode<T1> mPeekFetchNode;
        private int mStoredIndex;
        private int mFetchIndex;

        private bool hasDisposed;
        private bool mFetchedFinished;
        private static readonly object obj = new object();

        private List<BackwardDependenceNode<T1>> CacheNodes
        {
            get
            {
                if (mCacheNodes == null)
                {
                    mCacheNodes = new List<BackwardDependenceNode<T1>>();
                }
                return mCacheNodes;
            }
        }

        private List<BackwardDependenceNode<T1>> FetchNodes
        {
            get
            {
                if (mFetchNodes == null)
                {
                    mFetchNodes = new List<BackwardDependenceNode<T1>>();
                }
                return mFetchNodes;
            }
        }

        public BackwardDependenceNodeCache(IScheduleContainer<T1> container)
        {
            mContainer = container;
            mStoredIndex = mFetchIndex = -1;
        }

        public BackwardDependenceNodeCache()
        {
            mContainer = new ScheduleNullContainer<T1>();
            mStoredIndex = mFetchIndex = -1;
        }

        public T1 ValueInCacheOfLevel(int level)
        {
            lock (obj)
            {
                for (int k = CacheNodes.Count - 1; k >= 0; k--)
                {
                    if (CacheNodes[k].Level == level)
                    {
                        return CacheNodes[k].Value;
                    }
                }
            }
            
            return default(T1);
        }

        public T1 ParentValueInCacheOfLevel(int level)
        {
            lock (obj)
            {
                for (int k = CacheNodes.Count - 1; k >= 0; k--)
                {
                    if (CacheNodes[k].Level < level)
                    {
                        return CacheNodes[k].Value;
                    }
                }
            }
            
            return default(T1);
        }

        public void AddDocScanReport(T1 node)
        {
            mContainer.AddReport(node);
            int mReportIndex = -1;
            lock (obj)
            {
                for (; mReportIndex < CacheNodes.Count - 1; )
                {
                    mReportIndex++;
                    if (CacheNodes[mReportIndex].HasReported)
                    {
                        continue;
                    }
                    else
                    {
                        mContainer.AddReport(CacheNodes[mReportIndex].Value);
                        CacheNodes[mReportIndex].HasReported = true;
                    }
                }
            }
        }

        public void PutIn(T1 node, int level, bool threshold)
        {
            BackwardDependenceNode<T1> tmp = new BackwardDependenceNode<T1>();
            tmp.Level = level;
            tmp.SFThreshold = threshold;
            tmp.Value = node;

            Debug.Assert(tmp != null);
            Debug.Assert(!hasDisposed);
            Debug.Assert(!tmp.HasProcessed);
            if ((CacheNodes.Count <= 0) || (mPeekCacheNode.Level < tmp.Level))
            {
                lock (obj)
                {
                    CacheNodes.Add(tmp);
                }
                
                mPeekCacheNode = tmp;
                mStoredIndex = -1;
            }
            else
            {
                this.Flush();
                lock (obj)
                {
                    int k = this.GetInsertIndexOf(tmp, CacheNodes);
                    mStoredIndex = mStoredIndex >= k ? k - 1 : mStoredIndex;
                    while (CacheNodes.Count > k)
                    {
                        T1 tmp1 = CacheNodes[CacheNodes.Count - 1].Value;
                        if (tmp1 != null && tmp1 is IDisposable)
                        {
                            using (tmp1 as IDisposable) { }
                        }
                        CacheNodes.RemoveAt(CacheNodes.Count - 1);
                    }
                    CacheNodes.Add(tmp);
                }
            }
        }

        public T1 FetchNext()
        {
            try
            {
                Debug.Assert(!hasDisposed);
                T1 ret = default(T1);
                if (!mFetchedFinished && mPeekFetchNode == null)
                {
                    FetchNodes.Clear();
                    mPeekFetchNode = this.mContainer.FetchNext();
                    if (mPeekFetchNode != null)
                    {
                        lock (obj)
                        {
                            FetchNodes.Add(mPeekFetchNode);
                        }
                    }
                    else
                    {
                        return ret;
                    }
                }

                BackwardDependenceNode<T1> tmp;
                lock (obj)
                {
                    while (!mFetchedFinished && (!mPeekFetchNode.SFThreshold || mPeekFetchNode.HasProcessed))
                    {
                        tmp = mContainer.FetchNext();
                        if (tmp == null)
                        {
                            //mFetchedFinished = true;
                            break;
                        }
                        if (tmp.Level <= mPeekFetchNode.Level)
                        {
                            int k = this.GetInsertIndexOf(tmp, FetchNodes);
                            mFetchIndex = mFetchIndex >= k ? k - 1 : mFetchIndex;
                            while (FetchNodes.Count > k)
                            {
                                T1 tmp1 = FetchNodes[FetchNodes.Count - 1].Value;
                                if (tmp1 != null && tmp1 is IDisposable)
                                {
                                    using (tmp1 as IDisposable) { }
                                }
                                FetchNodes.RemoveAt(FetchNodes.Count - 1);
                            }
                        }
                        mPeekFetchNode = tmp;
                        FetchNodes.Add(mPeekFetchNode);
                    }

                    if (mFetchedFinished || (!mPeekFetchNode.SFThreshold || mPeekFetchNode.HasProcessed))
                    {
                        return default(T1);
                    }

                    tmp = FetchNodes[++mFetchIndex];
                }
                
                //如果tmp.SFThreshold为true,表示这个结点自己本身已经是与子结点具有相同的InterNodesPassValue.
                //if (tmp.Value is IInterDependencyNode && !tmp.SFThreshold)
                //{
                //    //将子结点的这个值赋给父结点。
                //    (tmp.Value as IInterDependencyNode).InterNodesPassValue
                //        = (mPeekFetchNode.Value as IInterDependencyNode).InterNodesPassValue ?? (tmp.Value as IInterDependencyNode).InterNodesPassValue;
                //}
                tmp.HasProcessed = true;
                return tmp.Value;
            }
            catch (Exception ex)
            {
                mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARCOMCustomContainersFetchNext + ex.ToString());
            }
            return null;
        }

        public void Flush()
        {
            //Debug.Assert(!hasDisposed);
            if (CacheNodes.Count <= 0)
            {
                return;
            }

            int index = -1;
            lock (obj)
            {
                for (int i = CacheNodes.Count - 1; i > mStoredIndex; i--)
                {
                    if (CacheNodes[i].SFThreshold)
                    {
                        index = i;
                        break;
                    }
                }
                if (-1 == index)
                {
                    return;
                }

                for (; mStoredIndex < index; )
                {
                    mStoredIndex++;
                    if (CacheNodes[mStoredIndex].HasProcessed)
                    {
                        continue;
                    }
                    else
                    {
                        mContainer.Store(CacheNodes[mStoredIndex].Value, CacheNodes[mStoredIndex].HasReported);
                        CacheNodes[mStoredIndex].HasProcessed = true;
                        CacheNodes[mStoredIndex].HasReported = true;
                    }
                    //mContainer.Store(CacheNodes[++mStoredIndex].Value);
                    //CacheNodes[mStoredIndex].HasProcessed = true;
                }
            }
            
        }

        public void Close()
        {
            this.Flush();
            this.mContainer.Flush();

            using (this.mContainer) { }
            lock (obj)
            {
                for (int i = CacheNodes.Count - 1; i >= 0; i--)
                {
                    BackwardDependenceNode<T1> tmp1 = CacheNodes[i];
                    if (tmp1 != null && tmp1.Value != null && tmp1.Value is IDisposable)
                    {
                        using (tmp1.Value as IDisposable) { }
                    }
                }

                /*
                             foreach (BackwardDependenceNode<T1> tmp1 in CacheNodes)
                {
                    if (tmp1 != null && tmp1.Value!=null&&tmp1.Value is IDisposable)
                    {
                        using (tmp1.Value as IDisposable) { }
                    }
                }
                 */
                CacheNodes.Clear();

                foreach (BackwardDependenceNode<T1> tmp2 in FetchNodes)
                {
                    if (tmp2 != null && tmp2 is IDisposable)
                    {
                        using (tmp2 as IDisposable) { }
                    }
                }
                FetchNodes.Clear();
            }
            this.hasDisposed = true;
            this.mFetchedFinished = true;
        }

        public void Dispose() { this.Close(); }

        public void ExecuteContainerAction(Action<IScheduleContainer<T1>> action)
        {
            if (action == null)
            {
                return;
            }

            IScheduleContainer<T1> container = mContainer;
            if (container == null)
            {
                return;
            }

            action(container);
        }

        private int GetInsertIndexOf(BackwardDependenceNode<T1> node, List<BackwardDependenceNode<T1>> coll)
        {
            for (int k = 0; k < coll.Count; k++)
            {
                if (coll[k].Level >= node.Level)
                {
                    return k;
                }
            }
            return coll.Count;
        }

    }


}