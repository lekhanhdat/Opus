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
using AvePoint.RA.Contract.RMWeb.Audit;
using System;

namespace AvePoint.RA.Contract.Audit
{
    /// <summary>
    /// have reviewed by allen yin
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class AuditAttribute : Attribute
    {
        public AuditAttribute()
        {

        }

        /// <summary>
        /// 表明是否各个功能自己处理Audit信息，如果是true的话，公共的逻辑不保存该信息，默认是false
        /// </summary>
        public bool IsHandled { get; set; }

        private bool startNewThread = true;

        /// <summary>
        /// 表明是否要新起线程执行handler操作，默认是true
        /// </summary>
        public bool StartNewThread
        {
            get { return startNewThread; }
            set { startNewThread = value; }
        }
        public AuditModule Module { get; set; }

        public AuditAction Action { get; set; }

        public AuditCategory Category { get; set; }

        /// <summary>
        /// 我们需要具体实现类的类名去IOC，所以这里用Type而不是接口？
        /// </summary>
        public Type BeforeHandler { get; set; }

        /// <summary>
        /// 我们需要具体实现类的类名去IOC，所以这里用Type而不是接口？
        /// </summary>
        public Type AfterHandler { get; set; }

    }
}
