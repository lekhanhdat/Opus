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
using System.Text;
using AvePoint.GCommon;
using System.Threading;

namespace AvePoint.Wrapper.Common
{
    public sealed class WrapperRuntime
    {
        [ThreadStatic]
        private static LocalDataStoreSlot key;
        private static readonly WrapperContext DefaultContext;
        private static WrapperContext GlobalContext;
        private readonly static object InstanceCreateLock = new object();
        public static AveWrapperCache WrapperCache { get; set; }

        static WrapperRuntime()
        {
            WrapperRuntime.DefaultContext = new WrapperContext();
            WrapperRuntime.DefaultContext.LoggerType = typeof(AveLogger);
            WrapperRuntime.ContextInstanceMode = ContextInstanceMode.Thread;

            WrapperRuntime.WrapperCache = AveWrapperCache.GetInstance();
        }

        public static void SetGlobalRuntimeSetting(bool optimized, AveWrapperRunningAccountInfo trimmingAccount)
        {
            WrapperRuntime.SetGlobalRuntimeSetting(typeof(AveLogger), optimized, trimmingAccount, ContextInstanceMode.Process);
        }

        public static void ClearGlobalContext()
        {
            GlobalContext = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggerType">设置wrapper内部用的log</param>
        /// <param name="optimized">是否启用sql优化</param>
        /// <param name="instanceMode">Context的实例模型，可以是整个进程共享，也可以是每个线程单独拥有一个context</param>
        public static void SetGlobalRuntimeSetting(Type loggerType, bool optimized, AveWrapperRunningAccountInfo trimmingAccount, ContextInstanceMode instanceMode)
        {
            WrapperRuntime.ContextInstanceMode = instanceMode;
            if (WrapperRuntime.GlobalContext == null)
            {
                lock (InstanceCreateLock)
                {
                    if (WrapperRuntime.GlobalContext == null)
                    {
                        WrapperRuntime.GlobalContext = new WrapperContext();
                        WrapperRuntime.GlobalContext.LoggerType = loggerType;
                        WrapperRuntime.GlobalContext.Opimized = optimized;
                        WrapperRuntime.GlobalContext.SecurityTrimmingAccount = trimmingAccount;
                    }
                    else
                    {
                        throw new WrapperStateException("global context can only be configured once.");
                    }
                }
            }
            else
            {
                throw new WrapperStateException("global context can only be configured once.");
            }
        }

        public static bool IsInitialized
        {
            get
            {
                return WrapperRuntime.GlobalContext != null;
            }
        }

        public static WrapperContext CurrentContext
        {
            get
            {
                if (WrapperRuntime.GlobalContext == null)
                {
                    WrapperRuntime.GlobalContext = WrapperRuntime.DefaultContext;
                }
                
                switch (WrapperRuntime.ContextInstanceMode)
                {
                    case ContextInstanceMode.Process:
                        return WrapperRuntime.GlobalContext;                        
                    case ContextInstanceMode.Thread:
                        if (key == null)
                        {
                            key = Thread.AllocateDataSlot();
                        }
                        WrapperContext threaContext = Thread.GetData(key) as WrapperContext;
                        if (threaContext == null)
                        {
                            threaContext = WrapperRuntime.GlobalContext.Clone() as WrapperContext;
                            Thread.SetData(key, threaContext);
                        }
                        return threaContext;
                    default:
                        return WrapperRuntime.GlobalContext;                        
                }                                              
            }
        }

        public static ContextInstanceMode ContextInstanceMode
        {            
            private set;            
            get;
        }
    }
}
