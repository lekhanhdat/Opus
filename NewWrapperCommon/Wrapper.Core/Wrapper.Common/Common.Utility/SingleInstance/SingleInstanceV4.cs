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

namespace AvePoint.Wrapper.Common
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Linq;

    /// <summary>
    /// Lazy mode, non-default constructor, thread safe
    /// 使用weak reference减轻内存压力。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class SingleInstanceV4<T, TArg>
        where T : class
    {
        //private static readonly Dictionary<TArg, T> _instances = new Dictionary<TArg, T>();
        private static readonly Dictionary<TArg, WeakReference> _weakRefInstances = new Dictionary<TArg, WeakReference>();
        private static object _instancesLock = new object();
        static SingleInstanceV4()
        {
            var clearThread = new Thread(ClearCache);
            clearThread.IsBackground = true;
            clearThread.Name = "Clear Instance Cache Thread";
            clearThread.Start();
        }
        public static T GetInstance(TArg arg)
        {
            T temp = default(T);
            lock (_instancesLock)
            {
                if (!TryGetInstanceFromWeakRefList(arg, out temp))
                {
                    try
                    {
                        temp = Activator.CreateInstance(typeof(T), arg) as T;
                    }
                    catch (MissingMethodException ex)
                    {
                        throw new InvalidOperationException(
                            string.Format("Cannot found a constructor of Type[{0}({1})]", typeof(T).FullName, typeof(TArg).FullName),
                            ex);
                    }
                    _weakRefInstances[arg] = new WeakReference(temp);
                }
            }
            return temp;
        }

        private static bool TryGetInstanceFromWeakRefList(TArg arg, out T temp)
        {
            temp = null;
            WeakReference weakRef;
            if (_weakRefInstances.TryGetValue(arg, out weakRef))
            {
                temp = weakRef.Target as T;
            }
            return temp != null;
        }

        private static void ClearCache()
        {
            while (true)
            {
                Thread.Sleep(1000);
                InternalClear();
            }
        }

        private static void InternalClear()
        {
            lock (_instancesLock)
            {
                _weakRefInstances.
                    Where(kv => kv.Value.Target == null).
                    Select(kv => kv.Key).
                    ToList().
                    ForEach(arg => _weakRefInstances.Remove(arg));
            }
        }
    }
}
