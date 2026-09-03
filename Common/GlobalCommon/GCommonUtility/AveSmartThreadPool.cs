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
using Amib.Threading;

namespace AvePoint.GCommon.Utility
{
    /// <summary>
    /// 为Manager端封装SmartThreadPool
    /// 此pool的使用不会影响系统线程pool
    /// </summary>
    public class AveSmartThreadPool<T> : IDisposable
    {
        private SmartThreadPool _SmartThreadPool { get; set; }
        private Action<T> _Action { get; set; }
       
        private readonly object stateLock = new object();
        private AveSmartThreadPoolState _state;

        /// <summary>
        /// 线程池要处理的遍历对象，在start函数调用前应先赋值
        /// </summary>
        public List<T> TargetList { get; set; }

        /// <summary>
        /// 封装SmartThreadPool的状态
        /// Busy时不要启动同一功能Task的下一个轮回，否则内存资源占用越来越多，系统会越来越卡
        /// </summary>
        public AveSmartThreadPoolState State
        {
            get
            {
                lock (stateLock)
                {
                    return this._state;
                }
            }
            private set 
            {
                lock (stateLock)
                {
                    this._state = value;
                }
            }
        }

        /// <summary>
        /// 为Manager端封装SmartThreadPool
        /// 此构造函数是在当前能取得目标List的情况下调用
        /// </summary>
        public AveSmartThreadPool(Action<T> action, List<T> targetList)
            : this(action)
        {
            this.TargetList = targetList;
        }

        /// <summary>
        /// 为Manager端封装SmartThreadPool
        /// 此构造函数是在当前不能取得目标List的情况下调用，调用Start函数前，需要通过setter方法给目标List赋值
        /// </summary>
        public AveSmartThreadPool(Action<T> action)
        {
            _SmartThreadPool = new SmartThreadPool() { MaxThreads = 10, };
            _Action = action;
        }

        /// <summary>
        /// 启动线程池，会遍历目标List，每个item作为Action的参数
        /// 每个线程搭载一个Action，直道所有目标List中的item都被执行Action完毕，线程池回到AveSmartThreadPoolState.Idle状态
        /// </summary>
        public void Start()
        {
            if (this.TargetList != null && this.TargetList.Count > 0)
            {
                State = AveSmartThreadPoolState.Busy;
                foreach (var item in this.TargetList)
                {
                    IWorkItemResult wir = _SmartThreadPool.QueueWorkItem(_Action, item);
                }
                _SmartThreadPool.WaitForIdle();
                State = AveSmartThreadPoolState.Idle;
            }
        }

        public void Shutdown()
        {
            _SmartThreadPool.Shutdown(TimeSpan.FromSeconds(30));
        }

        public void Dispose()
        {
            Shutdown();
        }
    }

    /// <summary>
    /// 封装SmartThreadPool的状态
    /// Busy时不要启动同一功能Task的下一个轮回，否则内存资源占用越来越多，系统会越来越卡
    /// </summary>
    public enum AveSmartThreadPoolState
    {
        /// <summary>
        /// 封装SmartThreadPool的状态
        /// 空闲，即线程池中没有一个线程在跑Action
        /// </summary>
        Idle = 0,

        /// <summary>
        /// 封装SmartThreadPool的状态
        /// 忙碌，即线程池中至少有一个线程在跑Action
        /// </summary>
        Busy = 1,
    }
}
