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
using AvePoint.RA.Contract.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Threads
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
            var currentGroupId = TenantLocalValue.LogonGroupId;
            var currentUserId = TenantLocalValue.LogonUserId;
            var currentUserType = TenantLocalValue.AccountType;
            var displayName = TenantLocalValue.DisplayName;
            var currentUserName = TenantLocalValue.LogonUserEmail;
            var currentPrincipal = Thread.CurrentPrincipal;
            var partnerUser = TenantLocalValue.PartnerUser;
            var callerType = TenantLocalValue.CallerType;
            ThreadStart startWithTenant = () =>
            {
                TenantLocalValue.LogonGroupId = currentGroupId;
                TenantLocalValue.LogonUserId = currentUserId;
                TenantLocalValue.LogonUserEmail = currentUserName;
                TenantLocalValue.AccountType = currentUserType;
                TenantLocalValue.DisplayName = displayName;
                TenantLocalValue.PartnerUser = partnerUser;
                TenantLocalValue.CallerType = callerType;
                TenantLocalValue.CurrentCulture = null;
                Thread.CurrentPrincipal = currentPrincipal;

                start();
            };
            t = new Thread(startWithTenant);
        }

        public AveTenantThread(ParameterizedThreadStart start)
        {
            var currentGroupId = TenantLocalValue.LogonGroupId;
            var currentUserId = TenantLocalValue.LogonUserId;
            var currentUserType = TenantLocalValue.AccountType;
            var displayName = TenantLocalValue.DisplayName;
            var currentUserName = TenantLocalValue.LogonUserEmail;
            var currentPrincipal = Thread.CurrentPrincipal;
            var partnerUser = TenantLocalValue.PartnerUser;
            var callerType = TenantLocalValue.CallerType;
            ParameterizedThreadStart startWithTenant = (o) =>
            {
                TenantLocalValue.LogonGroupId = currentGroupId;
                TenantLocalValue.LogonUserId = currentUserId;
                TenantLocalValue.AccountType = currentUserType;
                TenantLocalValue.DisplayName = displayName;
                TenantLocalValue.PartnerUser = partnerUser;
                TenantLocalValue.CallerType = callerType;
                TenantLocalValue.LogonUserEmail = currentUserName;
                TenantLocalValue.CurrentCulture = null;
                Thread.CurrentPrincipal = currentPrincipal;

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

        public void WaitThreadFinish()
        {
            t.Join();
        }
    }
}
