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


using System.Threading;

namespace  AvePoint.Hybrid.Utility.Threads
{
    public class AveTenantThreadUtility
    {
        public static AveThreadWrapper StartThread(ThreadStart start, string threadName, string groupId)
        {
            var currentGroupId = TenantThreadLocalValue.LogonGroupId;
            var currentUserId = TenantThreadLocalValue.LogonUserId;
            var currentUserName = TenantThreadLocalValue.LogonUserName;
            var currentPrincipal = TenantThreadLocalValue.CurrentPrincipal;

            ThreadStart startWithTenant = () =>
            {
                TenantThreadLocalValue.LogonGroupId = currentGroupId;
                TenantThreadLocalValue.LogonUserId = currentUserId;
                TenantThreadLocalValue.LogonUserName = currentUserName;
                TenantThreadLocalValue.CurrentPrincipal = currentPrincipal;

                start();
            };
            return AveThreadUtility.StartThread(startWithTenant, threadName, groupId);
        }

        public static AveThreadWrapper StartThread(ParameterizedThreadStart start, object obj, string threadName, string groupId)
        {
            var currentGroupId = TenantThreadLocalValue.LogonGroupId;
            var currentUserId = TenantThreadLocalValue.LogonUserId;
            var currentUserName = TenantThreadLocalValue.LogonUserName;
            var currentPrincipal = TenantThreadLocalValue.CurrentPrincipal;
            ParameterizedThreadStart startWithTenant = (o) =>
            {
                TenantThreadLocalValue.LogonGroupId = currentGroupId;
                TenantThreadLocalValue.LogonUserId = currentUserId;
                TenantThreadLocalValue.LogonUserName = currentUserName;
                TenantThreadLocalValue.CurrentPrincipal = currentPrincipal;

                start(o);
            };
            return AveThreadUtility.StartThread(startWithTenant, obj, threadName, groupId);
        }
    }
}
