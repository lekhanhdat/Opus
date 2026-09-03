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
    public class AveTenantThread
    {
        private Thread t;

        public bool IsBackground
        {
            get 
            {
                return t.IsBackground;
            }
            set 
            {
                t.IsBackground = value;
            }
        }

        public AveTenantThread(ThreadStart start)
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
                TenantThreadLocalValue.CurrentCulture = null;
                TenantThreadLocalValue.CurrentPrincipal = currentPrincipal;

                start();
            };
            t = new Thread(startWithTenant);
        }

        public AveTenantThread(ParameterizedThreadStart start) 
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
                TenantThreadLocalValue.CurrentCulture = null;
                TenantThreadLocalValue.CurrentPrincipal = currentPrincipal;

                start(o);
            };
            t = new Thread(startWithTenant);
        }

        public void Start()
        {
            t.Start();
        }

        public void Start(object parameter) 
        {
            t.Start(parameter);
        }
    }
}
