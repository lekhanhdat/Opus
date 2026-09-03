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


using AvePoint.GCommon;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using System;
using System.Threading;


namespace AvePoint.Hybrid.Utility.Threads
{
    public class AveTenantThreadPool
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveTenantThreadPool));

        public static void QueueUserWorkItem(Action action)
        {
            var currentGroupId = TenantThreadLocalValue.LogonGroupId;
            var currentUserId = TenantThreadLocalValue.LogonUserId;
            var currentUserName = TenantThreadLocalValue.LogonUserName;
            var currentPrincipal = TenantThreadLocalValue.CurrentPrincipal;
            ThreadPool.QueueUserWorkItem(o =>
            {
                TenantThreadLocalValue.LogonGroupId = currentGroupId;
                TenantThreadLocalValue.LogonUserId = currentUserId;
                TenantThreadLocalValue.LogonUserName = currentUserName;
                TenantThreadLocalValue.CurrentCulture = null;
                TenantThreadLocalValue.CurrentPrincipal = currentPrincipal;

                try
                {
                    action();

                }
                catch (System.Exception e)
                {
                    logger.Error("An error occurred while running ave tenant thread", e);
                }
            });
        }

        public static void QueueUserWorkItem(WaitCallback action)
        {
            QueueUserWorkItem(action, null);
        }

        public static void QueueUserWorkItem(WaitCallback action, System.Object state)
        {
            var currentGroupId = TenantThreadLocalValue.LogonGroupId;
            var currentUserId = TenantThreadLocalValue.LogonUserId;
            var currentUserName = TenantThreadLocalValue.LogonUserName;
            var currentPrincipal = TenantThreadLocalValue.CurrentPrincipal;

            ThreadPool.QueueUserWorkItem(o =>
            {
                TenantThreadLocalValue.LogonGroupId = currentGroupId;
                TenantThreadLocalValue.LogonUserId = currentUserId;
                TenantThreadLocalValue.LogonUserName = currentUserName;
                TenantThreadLocalValue.CurrentCulture = null;
                TenantThreadLocalValue.CurrentPrincipal = currentPrincipal;

                try
                {
                    action(o);
                }
                catch (System.Exception e)
                {
                    logger.Error("An error occurred while running ave tenant thread", e);
                }
            }, state);
        }

        public static void QueueUserWorkItemWithPrincipal(WaitCallback action)
        {
            var currentGroupId = TenantThreadLocalValue.LogonGroupId;
            var currentUserId = TenantThreadLocalValue.LogonUserId;
            var currentUserName = TenantThreadLocalValue.LogonUserName;
            var currentPrincipal = TenantThreadLocalValue.CurrentPrincipal;

            ThreadPool.QueueUserWorkItem(o =>
            {
                TenantThreadLocalValue.LogonGroupId = currentGroupId;
                TenantThreadLocalValue.LogonUserId = currentUserId;
                TenantThreadLocalValue.LogonUserName = currentUserName;
                TenantThreadLocalValue.CurrentCulture = null;
                TenantThreadLocalValue.CurrentPrincipal = new CustomThreadPrincipal();

                try
                {
                    action(o);
                }
                catch (System.Exception e)
                {
                    logger.Error("An error occurred while running ave tenant thread", e);
                }
            });
        }
    }
}
