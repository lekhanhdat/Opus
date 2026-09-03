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
using System.Threading;

namespace  AvePoint.Hybrid.Utility.Threads
{
    public class AveTenantSmartThreadPool<T> : IDisposable
    {
        private AveSmartThreadPool<T> _SmartThreadPool;

        public AveSmartThreadPoolState State
        {
            get
            {
                return _SmartThreadPool.State;
            }
        }

        /// <summary>
        /// 线程池要处理的遍历对象，在start函数调用前应先赋值
        /// </summary>
        public List<T> TargetList
        {
            get
            {
                return _SmartThreadPool.TargetList;
            }
            set
            {
                _SmartThreadPool.TargetList = value;
            }
        }

        public AveTenantSmartThreadPool(Action<T> action)
        {
            var currentGroupId = TenantThreadLocalValue.LogonGroupId;
            var currentUserId = TenantThreadLocalValue.LogonUserId;
            var currentUserName = TenantThreadLocalValue.LogonUserName;
            var currentPrincipal = TenantThreadLocalValue.CurrentPrincipal;

            Action<T> actionWithTenant = (o) =>
            {
                TenantThreadLocalValue.LogonGroupId = currentGroupId;
                TenantThreadLocalValue.LogonUserId = currentUserId;
                TenantThreadLocalValue.LogonUserName = currentUserName;
                TenantThreadLocalValue.CurrentCulture = null;
                TenantThreadLocalValue.CurrentPrincipal = currentPrincipal;

                action(o);
            };
            _SmartThreadPool = new AveSmartThreadPool<T>(actionWithTenant);
        }

        public void Start()
        {
            _SmartThreadPool.Start();
        }

        public void Shutdown()
        {
            _SmartThreadPool.Shutdown();
        }

        public void Dispose()
        {
            _SmartThreadPool.Dispose();
        }
    }
}
