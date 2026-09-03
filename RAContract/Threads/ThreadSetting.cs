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
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Threads
{
    public class ThreadSetting
    {
        public string LogonGroupId { get; set; }
        public string LogonUserId { get; set; }
        public Contract.RMWeb.RMAccountType AccountType { get; set; }
        public string DisplayName { get; set; }
        public string LogonUserEmail { get; set; }
        public string PartnerUser { get; set; }
        public string CallerType { get; set; }
        public System.Security.Principal.IPrincipal CurrentPrincipal { get; set; }

        public static ThreadSetting GetSetting()
        {
            var setting = new ThreadSetting()
            {
                LogonGroupId = TenantLocalValue.LogonGroupId,
                LogonUserId = TenantLocalValue.LogonUserId,
                AccountType = TenantLocalValue.AccountType,
                DisplayName = TenantLocalValue.DisplayName,
                LogonUserEmail = TenantLocalValue.LogonUserEmail,
                PartnerUser = TenantLocalValue.PartnerUser,
                CallerType = TenantLocalValue.CallerType,
                CurrentPrincipal = Thread.CurrentPrincipal
            };
            return setting;
        }

        public static void SetSetting(ThreadSetting t)
        {
            TenantLocalValue.LogonGroupId = t.LogonGroupId;
            TenantLocalValue.LogonUserId = t.LogonUserId;
            TenantLocalValue.AccountType = t.AccountType;
            TenantLocalValue.DisplayName = t.DisplayName;
            TenantLocalValue.LogonUserEmail = t.LogonUserEmail;
            TenantLocalValue.PartnerUser = t.PartnerUser;
            TenantLocalValue.CallerType = t.CallerType;
            Thread.CurrentPrincipal = t.CurrentPrincipal;
            TenantLocalValue.CurrentCulture = null;
        }
    }
}
